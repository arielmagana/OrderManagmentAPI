namespace OrderManagement.IntegrationTests.Infrastructure;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence.Repositories;

[Collection(SqlServerCollection.Name)]
public class PersistenceRepositoryTests(SqlServerFixture fixture)
{
    [Theory]
    [InlineData("ada@example.com")]
    [InlineData("grace@example.com")]
    [InlineData("linus@example.com")]
    [InlineData("margaret@example.com")]
    [InlineData("alan@example.com")]
    public async Task CustomerRepository_PersistsAndFindsUniqueEmail(string email)
    {
        await using var context = await fixture.CreateContextAsync();
        var repository = new CustomerRepository(context);
        var customer = await repository.AddAsync(Customer(email));

        (await repository.GetByEmailAsync(email))!.Id.Should().Be(customer.Id);
        (await repository.ExistsByEmailAsync(email)).Should().BeTrue();
    }

    [Theory]
    [InlineData("SKU-001")]
    [InlineData("SKU-002")]
    [InlineData("SKU-003")]
    [InlineData("SKU-004")]
    [InlineData("SKU-005")]
    public async Task ProductRepository_PersistsAndFindsUniqueSku(string sku)
    {
        await using var context = await fixture.CreateContextAsync();
        var repository = new ProductRepository(context);
        var product = await repository.AddAsync(Product(sku));

        (await repository.GetBySkuAsync(sku))!.Id.Should().Be(product.Id);
        (await repository.ExistsBySkuAsync(sku)).Should().BeTrue();
    }

    [Theory]
    [InlineData("ORD-001")]
    [InlineData("ORD-002")]
    [InlineData("ORD-003")]
    [InlineData("ORD-004")]
    [InlineData("ORD-005")]
    public async Task OrderRepository_PersistsAggregateAndIncludesItems(string orderNumber)
    {
        await using var context = await fixture.CreateContextAsync();
        var customer = await new CustomerRepository(context).AddAsync(Customer($"{orderNumber}@example.com"));
        var product = await new ProductRepository(context).AddAsync(Product($"SKU-{orderNumber}"));
        var order = new Order { OrderNumber = orderNumber, CustomerId = customer.Id, Status = OrderStatus.Pending, TotalAmount = 20m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, OrderItems = [new OrderItem { ProductId = product.Id, Quantity = 2, UnitPrice = 10m, LineTotal = 20m }] };

        await new OrderRepository(context).AddAsync(order);
        var saved = await new OrderRepository(context).GetByOrderNumberAsync(orderNumber);

        saved!.OrderItems.Should().ContainSingle(item => item.ProductId == product.Id && item.LineTotal == 20m);
        saved.Status.Should().Be(OrderStatus.Pending);
    }

    [Theory]
    [InlineData("duplicate@example.com")]
    [InlineData("another-duplicate@example.com")]
    [InlineData("third-duplicate@example.com")]
    [InlineData("fourth-duplicate@example.com")]
    [InlineData("fifth-duplicate@example.com")]
    public async Task CustomerEmail_UniqueIndexRejectsDuplicates(string email)
    {
        await using var context = await fixture.CreateContextAsync();
        var repository = new CustomerRepository(context);
        await repository.AddAsync(Customer(email));
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(Customer(email)));

    }

    [Theory]
    [InlineData("DUP-001")]
    [InlineData("DUP-002")]
    [InlineData("DUP-003")]
    [InlineData("DUP-004")]
    [InlineData("DUP-005")]
    public async Task ProductSku_UniqueIndexRejectsDuplicates(string sku)
    {
        await using var context = await fixture.CreateContextAsync();
        var repository = new ProductRepository(context);
        await repository.AddAsync(Product(sku));
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(Product(sku)));

    }

    [Theory]
    [InlineData("ORD-DUP-001")]
    [InlineData("ORD-DUP-002")]
    [InlineData("ORD-DUP-003")]
    [InlineData("ORD-DUP-004")]
    [InlineData("ORD-DUP-005")]
    public async Task OrderNumber_UniqueIndexRejectsDuplicates(string orderNumber)
    {
        await using var context = await fixture.CreateContextAsync();
        var customer = await new CustomerRepository(context).AddAsync(Customer($"{orderNumber}@example.com"));
        var repository = new OrderRepository(context);
        await repository.AddAsync(new Order { OrderNumber = orderNumber, CustomerId = customer.Id, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(new Order { OrderNumber = orderNumber, CustomerId = customer.Id, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }));   
    }

    [Fact]
    public async Task OrderDeletion_CascadesToOrderItems()
    {
        await using var context = await fixture.CreateContextAsync();
        var customer = await new CustomerRepository(context).AddAsync(Customer("cascade@example.com"));
        var product = await new ProductRepository(context).AddAsync(Product("SKU-CASCADE"));
        var repository = new OrderRepository(context);
        var order = await repository.AddAsync(new Order { OrderNumber = "ORD-CASCADE", CustomerId = customer.Id, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, OrderItems = [new OrderItem { ProductId = product.Id, Quantity = 1, UnitPrice = 1m, LineTotal = 1m }] });

        await repository.DeleteAsync(order.Id);

        (await context.OrderItems.CountAsync()).Should().Be(0);
    }

    private static Customer Customer(string email) => new() { Name = "Customer", Email = email, Phone = string.Empty, Address = string.Empty, City = string.Empty, PostalCode = string.Empty, Country = string.Empty, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    private static Product Product(string sku) => new() { Sku = sku, Name = "Product", Description = string.Empty, Price = 10m, StockQuantity = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
}
