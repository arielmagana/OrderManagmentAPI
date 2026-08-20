namespace OrderManagement.UnitTests.Application;

using System.Text.Json;
using FluentAssertions;
using Moq;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Application.Common.Validation;
using OrderManagement.Application.Customers;
using OrderManagement.Application.Customers.Commands;
using OrderManagement.Application.Customers.DTOs;
using OrderManagement.Application.Customers.Queries;
using OrderManagement.Application.Customers.Validators;
using OrderManagement.Application.Orders;
using OrderManagement.Application.Orders.Commands;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Application.Orders.Queries;
using OrderManagement.Application.Orders.Validators;
using OrderManagement.Application.Products;
using OrderManagement.Application.Products.Commands;
using OrderManagement.Application.Products.DTOs;
using OrderManagement.Application.Products.Queries;
using OrderManagement.Application.Products.Validators;
using OrderManagement.Domain;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

public class Phase2RemediationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    public void ValidatePagination_RejectsNonPositiveValues(int page, int pageSize)
    {
        var exception = Assert.Throws<ValidationException>(() => RequestValidator.ValidatePagination(page, pageSize));

        exception.StatusCode.Should().Be(422);
    }

    [Fact]
    public void ValidatePagination_RejectsPageSizesAboveDocumentedMaximum()
    {
        var exception = Assert.Throws<ValidationException>(() => RequestValidator.ValidatePagination(1, 101));

        exception.StatusCode.Should().Be(422);
        exception.Errors.Should().ContainKey("pageSize");
    }

    [Fact]
    public void CustomerDto_SerializesOnlyDocumentedFields()
    {
        var json = JsonSerializer.Serialize(new CustomerDto { Id = 1, Name = "Ada", Email = "ada@example.com", IsActive = true, Phone = "555" }, JsonOptions);

        json.Should().Contain("\"id\"").And.Contain("\"isActive\"");
        json.Should().NotContain("phone");
    }

    [Fact]
    public void ProductDto_UsesUnitPriceInTheJsonContract()
    {
        var json = JsonSerializer.Serialize(new ProductDto { Id = 1, Sku = "SKU-1", Name = "Item", UnitPrice = 10m, Description = "internal" }, JsonOptions);

        json.Should().Contain("\"unitPrice\"").And.NotContain("\"price\"").And.NotContain("description");
    }

    [Fact]
    public void OrderDto_UsesDocumentedOrderAndItemFields()
    {
        var dto = new OrderDto
        {
            Id = 1,
            CustomerId = 2,
            OrderDate = DateTime.UnixEpoch,
            Status = "Pending",
            TotalAmount = 20m,
            OrderNumber = "internal",
            Items = [new OrderItemDto { Id = 3, ProductId = 4, Quantity = 2, UnitPrice = 10m, Subtotal = 20m, ProductName = "internal" }]
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);

        json.Should().Contain("\"orderDate\"").And.Contain("\"subtotal\"");
        json.Should().NotContain("orderNumber").And.NotContain("lineTotal").And.NotContain("productName");
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task ChangeStatus_RejectsEverySelfTransition(OrderStatus status)
    {
        var repository = new Mock<IOrderRepository>();
        repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Order { Id = 1, Status = status });
        var handler = new ChangeOrderStatusCommandHandler(repository.Object);

        var exception = await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => handler.HandleAsync(new ChangeOrderStatusCommand(1, new ChangeOrderStatusRequest { Status = status.ToString() })));

        exception.StatusCode.Should().Be(409);
        repository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CustomerService_RejectsInvalidRequestsBeforeRepositoryAccess()
    {
        var repository = new Mock<ICustomerRepository>();
        var service = new CustomerService(
            new CreateCustomerCommandHandler(repository.Object), new UpdateCustomerCommandHandler(repository.Object),
            new GetCustomerQueryHandler(repository.Object), new GetCustomersPagedQueryHandler(repository.Object),
            new CreateCustomerValidator(), new UpdateCustomerValidator());

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateCustomerAsync(new CreateCustomerRequest()));
        repository.Verify(x => x.ExistsByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProductService_RejectsInvalidRequestsBeforeRepositoryAccess()
    {
        var repository = new Mock<IProductRepository>();
        var service = new ProductService(
            new CreateProductCommandHandler(repository.Object), new UpdateProductCommandHandler(repository.Object),
            new GetProductQueryHandler(repository.Object), new GetProductsPagedQueryHandler(repository.Object),
            new CreateProductValidator(), new UpdateProductValidator());

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(new CreateProductRequest()));
        repository.Verify(x => x.ExistsBySkuAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OrderService_RejectsInvalidRequestsBeforeRepositoryAccess()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var customerRepository = new Mock<ICustomerRepository>();
        var productRepository = new Mock<IProductRepository>();
        var service = new OrderService(
            new CreateOrderCommandHandler(orderRepository.Object, customerRepository.Object, productRepository.Object),
            new ChangeOrderStatusCommandHandler(orderRepository.Object), new GetOrderQueryHandler(orderRepository.Object),
            new GetOrdersPagedQueryHandler(orderRepository.Object), new CreateOrderValidator(), new ChangeOrderStatusValidator());

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateOrderAsync(new CreateOrderRequest()));
        customerRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData("", "valid@example.com")]
    [InlineData("Ada", "invalid-email")]
    public async Task CustomerRequestValidation_RejectsRequiredAndMalformedValues(string name, string email)
    {
        await Assert.ThrowsAsync<ValidationException>(() => RequestValidator.ValidateAsync(new CreateCustomerValidator(), new CreateCustomerRequest { Name = name, Email = email }));
    }

    [Theory]
    [InlineData("", "Item", 10)]
    [InlineData("SKU", "", 10)]
    [InlineData("SKU", "Item", 0)]
    public async Task ProductRequestValidation_RejectsRequiredAndInvalidPrices(string sku, string name, decimal unitPrice)
    {
        await Assert.ThrowsAsync<ValidationException>(() => RequestValidator.ValidateAsync(new CreateProductValidator(), new CreateProductRequest { Sku = sku, Name = name, UnitPrice = unitPrice }));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public async Task OrderRequestValidation_RejectsInvalidIdentifiersAndQuantities(int customerId, int quantity)
    {
        var request = new CreateOrderRequest { CustomerId = customerId, Items = [new OrderItemRequest { ProductId = 1, Quantity = quantity }] };

        await Assert.ThrowsAsync<ValidationException>(() => RequestValidator.ValidateAsync(new CreateOrderValidator(), request));
    }
}
