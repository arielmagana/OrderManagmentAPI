namespace OrderManagement.IntegrationTests.Api;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderManagement.Application.Common.Pagination;
using OrderManagement.Application.Customers;
using OrderManagement.Application.Customers.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.IntegrationTests.Infrastructure;

[Collection(SqlServerCollection.Name)]
public sealed class ApiHttpTests : IAsyncLifetime
{
    private readonly SqlServerFixture _database;
    private readonly OrderManagementApiFactory _factory;
    private HttpClient _client = null!;

    public ApiHttpTests(SqlServerFixture database)
    {
        _database = database;
        _factory = new OrderManagementApiFactory(database);
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Customers_support_create_lookup_list_and_pagination()
    {
        var first = await PostAsync("/api/customers", new { name = "Ada", email = "ada@example.com" }, HttpStatusCode.Created);
        var id = first.GetProperty("id").GetInt32();
        first.GetProperty("email").GetString().Should().Be("ada@example.com");
        await PostAsync("/api/customers", new { name = "Grace", email = "grace@example.com" }, HttpStatusCode.Created);

        var lookup = await GetAsync($"/api/customers/{id}", HttpStatusCode.OK);
        lookup.GetProperty("name").GetString().Should().Be("Ada");
        var page = await GetAsync("/api/customers?page=2&pageSize=1", HttpStatusCode.OK);
        AssertPage(page, 2, 1, 2, 2, 1);
    }

    [Fact]
    public async Task Customer_errors_map_duplicate_missing_malformed_and_validation()
    {
        await PostAsync("/api/customers", new { name = "Ada", email = "same@example.com" }, HttpStatusCode.Created);
        await AssertErrorAsync(await _client.PostAsJsonAsync("/api/customers", new { name = "Other", email = "same@example.com" }), 409, "DUPLICATE_EMAIL");
        await AssertErrorAsync(await _client.GetAsync("/api/customers/999999"), 404, "CUSTOMER_NOT_FOUND");
        await AssertErrorAsync(await _client.PostAsync("/api/customers", new StringContent("{ broken", Encoding.UTF8, "application/json")), 400, "INVALID_REQUEST", true);
        await AssertErrorAsync(await _client.PostAsJsonAsync("/api/customers", new { name = new string('x', 101), email = "valid@example.com" }), 422, "VALIDATION_FAILED", expectErrors: true);
        await AssertErrorAsync(await _client.GetAsync("/api/customers?page=0"), 422, "VALIDATION_FAILED", expectErrors: true);
    }

    [Fact]
    public async Task Products_support_create_lookup_list_and_pagination()
    {
        var createdResponse = await _client.PostAsJsonAsync("/api/products", new { sku = "SKU-1", name = "Widget", unitPrice = 12.50m, stockQuantity = 3 });
        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createdResponse.Headers.Location.Should().NotBeNull();
        AssertJson(createdResponse);
        var created = await ReadAsync(createdResponse);
        var id = created.GetProperty("id").GetInt32();
        await PostAsync("/api/products", new { sku = "SKU-2", name = "Gadget", unitPrice = 4m }, HttpStatusCode.Created);

        (await GetAsync($"/api/products/{id}", HttpStatusCode.OK)).GetProperty("sku").GetString().Should().Be("SKU-1");
        var page = await GetAsync("/api/products?page=1&pageSize=1", HttpStatusCode.OK);
        AssertPage(page, 1, 1, 2, 2, 1);
    }

    [Fact]
    public async Task Product_errors_map_duplicate_missing_and_validation()
    {
        await PostAsync("/api/products", new { sku = "DUP", name = "One", unitPrice = 1m }, HttpStatusCode.Created);
        await AssertErrorAsync(await _client.PostAsJsonAsync("/api/products", new { sku = "DUP", name = "Two", unitPrice = 1m }), 409, "DUPLICATE_SKU");
        await AssertErrorAsync(await _client.GetAsync("/api/products/999999"), 404, "PRODUCT_NOT_FOUND");
        await AssertErrorAsync(await _client.PostAsJsonAsync("/api/products", new { sku = "VALID", name = "Valid", unitPrice = 1m, stockQuantity = -1 }), 422, "VALIDATION_FAILED", true);
        await AssertErrorAsync(await _client.GetAsync("/api/products?pageSize=0"), 422, "VALIDATION_FAILED", true);
    }

    [Fact]
    public async Task Orders_support_creation_lookup_pagination_filters_and_valid_status_changes()
    {
        var customer = await PostAsync("/api/customers", new { name = "Buyer", email = "buyer@example.com" }, HttpStatusCode.Created);
        var product = await PostAsync("/api/products", new { sku = "ORDER-SKU", name = "Item", unitPrice = 7.25m }, HttpStatusCode.Created);
        var customerId = customer.GetProperty("id").GetInt32();
        var productId = product.GetProperty("id").GetInt32();
        var response = await _client.PostAsJsonAsync("/api/orders", new { customerId, items = new[] { new { productId, quantity = 2 } } });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        AssertJson(response);
        var order = await ReadAsync(response);
        order.GetProperty("totalAmount").GetDecimal().Should().Be(14.50m);
        var orderId = order.GetProperty("id").GetInt32();

        (await GetAsync($"/api/orders/{orderId}", HttpStatusCode.OK)).GetProperty("status").GetString().Should().Be("Pending");
        var page = await GetAsync($"/api/orders?page=1&pageSize=1&customerId={customerId}&status=Pending", HttpStatusCode.OK);
        AssertPage(page, 1, 1, 1, 1, 1);
        (await PutAsync($"/api/orders/{orderId}/status", new { status = "Confirmed" }, HttpStatusCode.OK)).GetProperty("status").GetString().Should().Be("Confirmed");
        (await PutAsync($"/api/orders/{orderId}/status", new { status = "Completed" }, HttpStatusCode.OK)).GetProperty("status").GetString().Should().Be("Completed");
    }

    [Fact]
    public async Task Order_errors_cover_missing_resources_validation_and_invalid_transitions()
    {
        await AssertErrorAsync(await _client.GetAsync("/api/orders/999999"), 404, "ORDER_NOT_FOUND");
        await AssertErrorAsync(await _client.PostAsJsonAsync("/api/orders", new { customerId = 999999, items = new[] { new { productId = 1, quantity = 1 } } }), 404, "CUSTOMER_NOT_FOUND");
        await AssertErrorAsync(await _client.PostAsJsonAsync("/api/orders", new { customerId = 0, items = Array.Empty<object>() }), 400, "INVALID_REQUEST", true);
        await AssertErrorAsync(await _client.GetAsync("/api/orders?customerId=0"), 422, "INVALID_VALUE", true);
        await AssertErrorAsync(await _client.GetAsync("/api/orders?status=nonsense"), 422, "INVALID_VALUE", true);

        var (customerId, productId) = await SeedEntitiesAsync(true, true);
        await AssertErrorAsync(await _client.PostAsJsonAsync("/api/orders", new { customerId, items = new[] { new { productId = 999999, quantity = 1 } } }), 404, "PRODUCT_NOT_FOUND");
        var order = await PostAsync("/api/orders", new { customerId, items = new[] { new { productId, quantity = 1 } } }, HttpStatusCode.Created);
        var orderId = order.GetProperty("id").GetInt32();
        await AssertErrorAsync(await _client.PutAsJsonAsync($"/api/orders/{orderId}/status", new { status = "Completed" }), 409, "INVALID_STATUS_TRANSITION");
        await AssertErrorAsync(await _client.PutAsJsonAsync("/api/orders/999999/status", new { status = "Confirmed" }), 404, "ORDER_NOT_FOUND");
        await AssertErrorAsync(await _client.PutAsJsonAsync($"/api/orders/{orderId}/status", new { status = "bogus" }), 422, "VALIDATION_FAILED", true);
    }

    [Fact]
    public async Task Orders_reject_inactive_customer_and_product()
    {
        var inactiveCustomer = await SeedEntitiesAsync(false, true);
        await AssertErrorAsync(await _client.PostAsJsonAsync("/api/orders", new { customerId = inactiveCustomer.CustomerId, items = new[] { new { productId = inactiveCustomer.ProductId, quantity = 1 } } }), 409, "CUSTOMER_INACTIVE");
        await _factory.ResetDatabaseAsync();
        var inactiveProduct = await SeedEntitiesAsync(true, false);
        await AssertErrorAsync(await _client.PostAsJsonAsync("/api/orders", new { customerId = inactiveProduct.CustomerId, items = new[] { new { productId = inactiveProduct.ProductId, quantity = 1 } } }), 409, "PRODUCT_INACTIVE");
    }

    [Fact]
    public async Task Unhandled_exceptions_return_safe_complete_ADR_007_shape()
    {
        await using var factory = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddScoped<ICustomerService, ThrowingCustomerService>();
        }));
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/customers");
        var error = await AssertErrorAsync(response, 500, "INTERNAL_ERROR");
        error.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo("type", "title", "status", "detail", "code");
        error.ToString().Should().NotContain("secret database failure");
    }

    [Fact]
    public async Task Health_reports_database_dependency_and_unhealthy_behavior()
    {
        var healthyResponse = await _client.GetAsync("/api/health");
        healthyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertJson(healthyResponse);
        var healthy = await ReadAsync(healthyResponse);
        healthy.GetProperty("status").GetString().Should().Be("healthy");
        healthy.GetProperty("checks").EnumerateArray().Should().Contain(x => x.GetProperty("name").GetString() == "database" && x.GetProperty("status").GetString() == "healthy");

        await using var unhealthyFactory = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddHealthChecks().AddCheck("forced", () => HealthCheckResult.Unhealthy())));
        using var client = unhealthyFactory.CreateClient();
        var unhealthyResponse = await client.GetAsync("/api/health");
        unhealthyResponse.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        AssertJson(unhealthyResponse);
        (await ReadAsync(unhealthyResponse)).GetProperty("status").GetString().Should().Be("unhealthy");
    }

    [Fact]
    public async Task OpenApi_document_is_available_and_contains_every_Phase_4_route()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertJson(response);
        var paths = (await ReadAsync(response)).GetProperty("paths");
        foreach (var route in new[] { "/api/customers", "/api/customers/{id}", "/api/products", "/api/products/{id}", "/api/orders", "/api/orders/{id}", "/api/orders/{id}/status", "/api/health" })
            paths.TryGetProperty(route, out _).Should().BeTrue($"OpenAPI should contain {route}");
    }

    private async Task<(int CustomerId, int ProductId)> SeedEntitiesAsync(bool customerActive, bool productActive)
    {
        await using var context = _database.CreateContext();
        var now = DateTime.UtcNow;
        var customer = new Customer { Name = "Seed", Email = $"seed-{Guid.NewGuid():N}@example.com", IsActive = customerActive, CreatedAt = now, UpdatedAt = now };
        var product = new Product { Sku = $"SKU-{Guid.NewGuid():N}", Name = "Seed", Price = 3m, IsActive = productActive, CreatedAt = now, UpdatedAt = now };
        context.AddRange(customer, product);
        await context.SaveChangesAsync();
        return (customer.Id, product.Id);
    }

    private async Task<JsonElement> PostAsync(string path, object body, HttpStatusCode expected) => await AssertSuccessAsync(await _client.PostAsJsonAsync(path, body), expected);
    private async Task<JsonElement> PutAsync(string path, object body, HttpStatusCode expected) => await AssertSuccessAsync(await _client.PutAsJsonAsync(path, body), expected);
    private async Task<JsonElement> GetAsync(string path, HttpStatusCode expected) => await AssertSuccessAsync(await _client.GetAsync(path), expected);

    private static async Task<JsonElement> AssertSuccessAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        response.StatusCode.Should().Be(expected, await response.Content.ReadAsStringAsync());
        AssertJson(response);
        return await ReadAsync(response);
    }

    private static async Task<JsonElement> AssertErrorAsync(HttpResponseMessage response, int status, string code, bool expectErrors = false)
    {
        response.StatusCode.Should().Be((HttpStatusCode)status, await response.Content.ReadAsStringAsync());
        AssertJson(response);
        var error = await ReadAsync(response);
        error.GetProperty("status").GetInt32().Should().Be(status);
        error.GetProperty("code").GetString().Should().Be(code);
        error.GetProperty("type").GetString().Should().Be($"https://example.com/problems/{code.ToLowerInvariant().Replace('_', '-')}");
        error.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
        error.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
        error.TryGetProperty("errors", out var errors).Should().Be(expectErrors);
        if (expectErrors) errors.ValueKind.Should().Be(JsonValueKind.Object);
        return error;
    }

    private static void AssertPage(JsonElement page, int number, int size, int count, int pages, int items)
    {
        page.GetProperty("pageNumber").GetInt32().Should().Be(number);
        page.GetProperty("pageSize").GetInt32().Should().Be(size);
        page.GetProperty("totalCount").GetInt32().Should().Be(count);
        page.GetProperty("totalPages").GetInt32().Should().Be(pages);
        page.GetProperty("items").GetArrayLength().Should().Be(items);
    }

    private static void AssertJson(HttpResponseMessage response) => response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response) => (await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync())).RootElement.Clone();

    private sealed class ThrowingCustomerService : ICustomerService
    {
        private static Exception Failure() => new InvalidOperationException("secret database failure");
        public Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request) => throw Failure();
        public Task<CustomerDto> GetCustomerAsync(int customerId) => throw Failure();
        public Task<PaginatedResponse<CustomerDto>> GetCustomersPagedAsync(int page = 1, int pageSize = 20) => throw Failure();
        public Task<CustomerDto> UpdateCustomerAsync(int customerId, UpdateCustomerRequest request) => throw Failure();
    }
}
