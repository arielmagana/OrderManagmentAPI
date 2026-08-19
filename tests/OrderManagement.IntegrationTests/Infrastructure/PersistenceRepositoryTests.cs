namespace OrderManagement.IntegrationTests.Infrastructure;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence.Repositories;

[Collection(SqlServerCollection.Name)]
public class PersistenceRepositoryTests(SqlServerFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CustomerRepository_PerformsQueriesUpdatesAndDeletesAcrossContexts()
    {
        int firstId;
        int secondId;
        await using (var context = fixture.CreateContext())
        {
            var repository = new CustomerRepository(context);
            var first = await repository.AddAsync(Customer("first@example.com"));
            var second = await repository.AddAsync(Customer("second@example.com"));
            firstId = first.Id;
            secondId = second.Id;
            first.Name = "Tracked update";
            await repository.UpdateAsync(first);
        }

        await using (var context = fixture.CreateContext())
        {
            var repository = new CustomerRepository(context);
            (await repository.GetByIdAsync(firstId))!.Name.Should().Be("Tracked update");
            (await repository.GetByEmailAsync("first@example.com"))!.Id.Should().Be(firstId);
            (await repository.ExistsByEmailAsync("first@example.com")).Should().BeTrue();
            (await repository.GetAllAsync()).Select(customer => customer.Id).Should().Equal(firstId, secondId);
            await repository.DeleteAsync(firstId);
            await repository.DeleteAsync(int.MaxValue);
        }

        await using var assertionContext = fixture.CreateContext();
        (await new CustomerRepository(assertionContext).GetByIdAsync(firstId)).Should().BeNull();
    }

    [Fact]
    public async Task ProductRepository_UpdatesDetachedRootAndPerformsQueriesAndDeletes()
    {
        Product detached;
        int secondId;
        await using (var context = fixture.CreateContext())
        {
            var repository = new ProductRepository(context);
            detached = await repository.AddAsync(Product("SKU-001"));
            secondId = (await repository.AddAsync(Product("SKU-002"))).Id;
        }

        detached.Name = "Detached update";
        detached.Price = 15.25m;
        await using (var context = fixture.CreateContext())
            await new ProductRepository(context).UpdateAsync(detached);

        await using (var context = fixture.CreateContext())
        {
            var repository = new ProductRepository(context);
            (await repository.GetByIdAsync(detached.Id))!.Name.Should().Be("Detached update");
            (await repository.GetBySkuAsync("SKU-001"))!.Price.Should().Be(15.25m);
            (await repository.ExistsBySkuAsync("SKU-001")).Should().BeTrue();
            (await repository.GetAllAsync()).Select(product => product.Id).Should().Equal(detached.Id, secondId);
            await repository.DeleteAsync(secondId);
            await repository.DeleteAsync(int.MaxValue);
        }

        await using var assertionContext = fixture.CreateContext();
        (await new ProductRepository(assertionContext).GetByIdAsync(secondId)).Should().BeNull();
    }

    [Fact]
    public async Task OrderRepository_RehydratesCompleteAggregateAndSupportsAllQueries()
    {
        var seeded = await SeedOrderAsync("ORD-001", "order@example.com", "SKU-ORDER");

        await using var context = fixture.CreateContext();
        var repository = new OrderRepository(context);
        var byId = await repository.GetByIdAsync(seeded.OrderId);

        byId!.OrderItems.Should().ContainSingle();
        byId.OrderItems.Single().Product!.Name.Should().Be("Product");
        byId.Status.Should().Be(OrderStatus.Pending);
        (await repository.GetByOrderNumberAsync("ORD-001"))!.Id.Should().Be(seeded.OrderId);
        (await repository.GetByCustomerIdAsync(seeded.CustomerId)).Should().ContainSingle(order => order.Id == seeded.OrderId);
        (await repository.GetAllAsync()).Should().ContainSingle(order => order.Id == seeded.OrderId);
    }

    [Fact]
    public async Task OrderRepository_TrackedStatusUpdateDoesNotOverwriteStaleProductOrItems()
    {
        var seeded = await SeedOrderAsync("ORD-TRACKED", "tracked@example.com", "SKU-TRACKED");
        await using var staleContext = fixture.CreateContext();
        var staleRepository = new OrderRepository(staleContext);
        var staleOrder = (await staleRepository.GetByIdAsync(seeded.OrderId))!;

        await using (var concurrentContext = fixture.CreateContext())
        {
            var repository = new ProductRepository(concurrentContext);
            var product = (await repository.GetByIdAsync(seeded.ProductId))!;
            product.Price = 99m;
            await repository.UpdateAsync(product);
        }

        staleOrder.Confirm();
        await staleRepository.UpdateAsync(staleOrder);

        await using var assertionContext = fixture.CreateContext();
        (await assertionContext.Products.FindAsync(seeded.ProductId))!.Price.Should().Be(99m);
        (await assertionContext.OrderItems.FindAsync(seeded.OrderItemId))!.Quantity.Should().Be(2);
        (await assertionContext.Orders.FindAsync(seeded.OrderId))!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task OrderRepository_DetachedUpdateModifiesOnlyOrderRoot()
    {
        var seeded = await SeedOrderAsync("ORD-DETACHED", "detached@example.com", "SKU-DETACHED");
        Order detached;
        await using (var context = fixture.CreateContext())
            detached = (await new OrderRepository(context).GetByIdAsync(seeded.OrderId))!;

        detached.Cancel();
        detached.OrderItems.Single().Product!.Name = "Must not be persisted";
        detached.OrderItems.Single().Quantity = 999;
        await using (var context = fixture.CreateContext())
            await new OrderRepository(context).UpdateAsync(detached);

        await using var assertionContext = fixture.CreateContext();
        (await assertionContext.Orders.FindAsync(seeded.OrderId))!.Status.Should().Be(OrderStatus.Cancelled);
        (await assertionContext.Products.FindAsync(seeded.ProductId))!.Name.Should().Be("Product");
        (await assertionContext.OrderItems.FindAsync(seeded.OrderItemId))!.Quantity.Should().Be(2);
    }

    [Fact]
    public async Task UniqueIndexesRejectDuplicateNaturalKeys()
    {
        await using var context = fixture.CreateContext();
        var customers = new CustomerRepository(context);
        await customers.AddAsync(Customer("duplicate@example.com"));
        await new ProductRepository(context).AddAsync(Product("DUPLICATE-SKU"));
        var customerId = (await customers.GetByEmailAsync("duplicate@example.com"))!.Id;
        await new OrderRepository(context).AddAsync(NewOrder("ORD-DUPLICATE", customerId));

        await Assert.ThrowsAsync<DbUpdateException>(() => customers.AddAsync(Customer("duplicate@example.com")));
        context.ChangeTracker.Clear();
        await Assert.ThrowsAsync<DbUpdateException>(() => new ProductRepository(context).AddAsync(Product("DUPLICATE-SKU")));
        context.ChangeTracker.Clear();
        await Assert.ThrowsAsync<DbUpdateException>(() => new OrderRepository(context).AddAsync(NewOrder("ORD-DUPLICATE", customerId)));
    }

    [Fact]
    public async Task ForeignKeysRejectInvalidOrderAndOrderItemReferences()
    {
        await using (var context = fixture.CreateContext())
            await Assert.ThrowsAsync<DbUpdateException>(() => new OrderRepository(context).AddAsync(NewOrder("ORD-BAD-CUSTOMER", int.MaxValue)));

        await using (var context = fixture.CreateContext())
        {
            var customer = await new CustomerRepository(context).AddAsync(Customer("fk@example.com"));
            var order = NewOrder("ORD-BAD-PRODUCT", customer.Id);
            order.OrderItems.Add(new OrderItem { ProductId = int.MaxValue, Quantity = 1, UnitPrice = 1m, LineTotal = 1m });
            await Assert.ThrowsAsync<DbUpdateException>(() => new OrderRepository(context).AddAsync(order));
        }
    }

    [Fact]
    public async Task ReferencedCustomerAndProductCannotBeDeleted()
    {
        var seeded = await SeedOrderAsync("ORD-RESTRICT", "restrict@example.com", "SKU-RESTRICT");
        await using (var context = fixture.CreateContext())
            await Assert.ThrowsAsync<DbUpdateException>(() => new CustomerRepository(context).DeleteAsync(seeded.CustomerId));
        await using (var context = fixture.CreateContext())
            await Assert.ThrowsAsync<DbUpdateException>(() => new ProductRepository(context).DeleteAsync(seeded.ProductId));
    }

    [Fact]
    public async Task DeletingOrderCascadesToOrderItems()
    {
        var seeded = await SeedOrderAsync("ORD-CASCADE", "cascade@example.com", "SKU-CASCADE");
        await using (var context = fixture.CreateContext())
        {
            await new OrderRepository(context).DeleteAsync(seeded.OrderId);
            await new OrderRepository(context).DeleteAsync(int.MaxValue);
        }

        await using var assertionContext = fixture.CreateContext();
        (await assertionContext.OrderItems.FindAsync(seeded.OrderItemId)).Should().BeNull();
    }

    [Fact]
    public async Task ConfiguredMaximumLengthsAreEnforcedBySqlServer()
    {
        var customer = Customer("length@example.com");
        customer.Name = new string('x', 101);
        await using var context = fixture.CreateContext();
        await Assert.ThrowsAsync<DbUpdateException>(() => new CustomerRepository(context).AddAsync(customer));
    }

    [Fact]
    public async Task DecimalScaleAndStatusStringConversionAreApplied()
    {
        int productId;
        int orderId;
        await using (var context = fixture.CreateContext())
        {
            var customer = await new CustomerRepository(context).AddAsync(Customer("mapping@example.com"));
            var product = Product("SKU-MAPPING");
            product.Price = 12.345m;
            productId = (await new ProductRepository(context).AddAsync(product)).Id;
            var order = NewOrder("ORD-MAPPING", customer.Id);
            order.Status = OrderStatus.Confirmed;
            orderId = (await new OrderRepository(context).AddAsync(order)).Id;
        }

        await using var readContext = fixture.CreateContext();
        (await readContext.Products.FindAsync(productId))!.Price.Should().Be(12.35m);
        (await readContext.Orders.FindAsync(orderId))!.Status.Should().Be(OrderStatus.Confirmed);
        var storedStatus = await readContext.Database.SqlQueryRaw<string>(
            "SELECT [Status] AS [Value] FROM [Orders] WHERE [Id] = {0}", orderId).SingleAsync();
        storedStatus.Should().Be("Confirmed");
    }

    [Fact]
    public async Task InitialMigrationCreatesUsableSchema()
    {
        await using var context = fixture.CreateContext();
        (await context.Database.GetAppliedMigrationsAsync()).Should()
            .ContainSingle(migration => migration.EndsWith("InitialPersistence"));
        await new CustomerRepository(context).AddAsync(Customer("migration@example.com"));
        (await context.Customers.CountAsync()).Should().Be(1);
    }

    private async Task<(int OrderId, int CustomerId, int ProductId, int OrderItemId)> SeedOrderAsync(string number, string email, string sku)
    {
        await using var context = fixture.CreateContext();
        var customer = await new CustomerRepository(context).AddAsync(Customer(email));
        var product = await new ProductRepository(context).AddAsync(Product(sku));
        var order = NewOrder(number, customer.Id);
        order.TotalAmount = 20m;
        order.OrderItems.Add(new OrderItem { ProductId = product.Id, Quantity = 2, UnitPrice = 10m, LineTotal = 20m });
        await new OrderRepository(context).AddAsync(order);
        return (order.Id, customer.Id, product.Id, order.OrderItems.Single().Id);
    }

    private static Customer Customer(string email) => new() { Name = "Customer", Email = email, Phone = string.Empty, Address = string.Empty, City = string.Empty, PostalCode = string.Empty, Country = string.Empty, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    private static Product Product(string sku) => new() { Sku = sku, Name = "Product", Description = string.Empty, Price = 10m, StockQuantity = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    private static Order NewOrder(string number, int customerId) => new() { OrderNumber = number, CustomerId = customerId, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
}
