namespace OrderManagement.UnitTests.Domain;

using OrderManagement.Domain.Entities;

public class CustomerEntityTests
{
    [Fact]
    public void CreateCustomer_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "555-1234",
            Address = "123 Main St",
            City = "New York",
            PostalCode = "10001",
            Country = "USA"
        };

        // Assert
        Assert.Equal(1, customer.Id);
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("john@example.com", customer.Email);
        Assert.Equal("555-1234", customer.Phone);
        Assert.Equal("123 Main St", customer.Address);
        Assert.Equal("New York", customer.City);
        Assert.Equal("10001", customer.PostalCode);
        Assert.Equal("USA", customer.Country);
    }

    [Fact]
    public void Customer_ShouldHaveEmptyOrdersCollectionInitially()
    {
        // Arrange & Act
        var customer = new Customer { Name = "Test Customer" };

        // Assert
        Assert.NotNull(customer.Orders);
        Assert.Empty(customer.Orders);
    }

    [Fact]
    public void Customer_CanHaveOrdersAdded()
    {
        // Arrange
        var customer = new Customer { Id = 1, Name = "Test Customer" };
        var order = new Order { Id = 1, CustomerId = 1 };

        // Act
        customer.Orders.Add(order);

        // Assert
        Assert.Single(customer.Orders);
        Assert.Contains(order, customer.Orders);
    }

    [Fact]
    public void Customer_EmailCanBeEmpty()
    {
        // Arrange & Act
        var customer = new Customer { Name = "Test" };

        // Assert
        Assert.Equal(string.Empty, customer.Email);
    }

    [Fact]
    public void Customer_InitializationSetsTimestamps()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow;
        var customer = new Customer { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(customer.CreatedAt >= beforeCreation && customer.CreatedAt <= afterCreation);
        Assert.True(customer.UpdatedAt >= beforeCreation && customer.UpdatedAt <= afterCreation);
    }
}
