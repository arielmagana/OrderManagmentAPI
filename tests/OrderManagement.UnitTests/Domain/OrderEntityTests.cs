namespace OrderManagement.UnitTests.Domain;

using OrderManagement.Domain;
using OrderManagement.Domain.Entities;

public class OrderEntityTests
{
    [Fact]
    public void CreateOrder_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var order = new Order
        {
            Id = 1,
            OrderNumber = "ORD-2024-001",
            CustomerId = 1,
            Status = OrderStatus.Pending,
            TotalAmount = 299.99m,
            Notes = "Express delivery"
        };

        // Assert
        Assert.Equal(1, order.Id);
        Assert.Equal("ORD-2024-001", order.OrderNumber);
        Assert.Equal(1, order.CustomerId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(299.99m, order.TotalAmount);
        Assert.Equal("Express delivery", order.Notes);
    }

    [Fact]
    public void CreateOrder_DefaultStatusIsPending()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Order_ShouldHaveEmptyOrderItemsCollectionInitially()
    {
        // Arrange & Act
        var order = new Order { OrderNumber = "ORD-001" };

        // Assert
        Assert.NotNull(order.OrderItems);
        Assert.Empty(order.OrderItems);
    }

    [Fact]
    public void Order_CanHaveOrderItemsAdded()
    {
        // Arrange
        var order = new Order { Id = 1, OrderNumber = "ORD-001" };
        var orderItem = new OrderItem { Id = 1, OrderId = 1 };

        // Act
        order.OrderItems.Add(orderItem);

        // Assert
        Assert.Single(order.OrderItems);
        Assert.Contains(orderItem, order.OrderItems);
    }

    [Fact]
    public void Order_TotalAmountCanBeZero()
    {
        // Arrange & Act
        var order = new Order { TotalAmount = 0m };

        // Assert
        Assert.Equal(0m, order.TotalAmount);
    }

    [Fact]
    public void Order_NotesCanBeEmpty()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        Assert.Equal(string.Empty, order.Notes);
    }

    [Fact]
    public void Order_CustomerNavigationProperty()
    {
        // Arrange
        var customer = new Customer { Id = 1, Name = "Test Customer" };
        var order = new Order
        {
            CustomerId = 1,
            Customer = customer
        };

        // Act & Assert
        Assert.NotNull(order.Customer);
        Assert.Equal(customer.Id, order.Customer.Id);
    }

    [Fact]
    public void Order_ConfirmUpdatesTimestamp()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Pending, UpdatedAt = DateTime.MinValue };
        var beforeUpdate = DateTime.UtcNow;

        // Act
        order.Confirm();

        // Assert
        Assert.True(order.UpdatedAt > beforeUpdate);
    }

    [Fact]
    public void Order_CompleteUpdatesTimestamp()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Confirmed, UpdatedAt = DateTime.MinValue };
        var beforeUpdate = DateTime.UtcNow;

        // Act
        order.Complete();

        // Assert
        Assert.True(order.UpdatedAt > beforeUpdate);
    }

    [Fact]
    public void Order_CancelUpdatesTimestamp()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Pending, UpdatedAt = DateTime.MinValue };
        var beforeUpdate = DateTime.UtcNow;

        // Act
        order.Cancel();

        // Assert
        Assert.True(order.UpdatedAt > beforeUpdate);
    }
}
