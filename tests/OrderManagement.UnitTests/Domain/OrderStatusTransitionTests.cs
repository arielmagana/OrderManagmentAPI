namespace OrderManagement.UnitTests.Domain;

using OrderManagement.Domain;
using OrderManagement.Domain.Entities;

public class OrderStatusTransitionTests
{
    [Fact]
    public void NewOrder_ShouldHavePendingStatus()
    {
        // Arrange & Act
        var order = new Order
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerId = 1,
            Status = OrderStatus.Pending
        };

        // Assert
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void PendingOrder_CanTransitionToConfirmed()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Pending };

        // Act
        order.Confirm();

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void ConfirmedOrder_CanTransitionToCompleted()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Confirmed };

        // Act
        order.Complete();

        // Assert
        Assert.Equal(OrderStatus.Completed, order.Status);
    }

    [Fact]
    public void PendingOrder_CanBeCancelled()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Pending };

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void ConfirmedOrder_CannotBeRevertedToPending()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Confirmed };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    [Fact]
    public void ConfirmedOrder_CannotBeCancelled()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Confirmed };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => order.Cancel());
        Assert.Contains("Only Pending orders can be cancelled", exception.Message);
    }

    [Fact]
    public void CompletedOrder_CannotTransition()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Completed };

        // Act & Assert - Cannot complete an already completed order
        Assert.Throws<InvalidOperationException>(() => order.Complete());
        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void CancelledOrder_CannotTransition()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Cancelled };

        // Act & Assert - Cannot confirm a cancelled order
        Assert.Throws<InvalidOperationException>(() => order.Confirm());
        Assert.Throws<InvalidOperationException>(() => order.Complete());
    }

    [Fact]
    public void Confirm_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Pending, UpdatedAt = DateTime.MinValue };
        var beforeUpdate = DateTime.UtcNow;

        // Act
        order.Confirm();

        // Assert
        Assert.True(order.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public void CanTransitionTo_ReturnsTrueForValidTransitions()
    {
        // Arrange
        var pendingOrder = new Order { Status = OrderStatus.Pending };
        var confirmedOrder = new Order { Status = OrderStatus.Confirmed };

        // Act & Assert
        Assert.True(pendingOrder.CanTransitionTo(OrderStatus.Confirmed));
        Assert.True(pendingOrder.CanTransitionTo(OrderStatus.Cancelled));
        Assert.True(confirmedOrder.CanTransitionTo(OrderStatus.Completed));
    }

    [Fact]
    public void CanTransitionTo_ReturnsFalseForInvalidTransitions()
    {
        // Arrange
        var pendingOrder = new Order { Status = OrderStatus.Pending };
        var confirmedOrder = new Order { Status = OrderStatus.Confirmed };
        var completedOrder = new Order { Status = OrderStatus.Completed };

        // Act & Assert
        Assert.False(pendingOrder.CanTransitionTo(OrderStatus.Completed));
        Assert.False(confirmedOrder.CanTransitionTo(OrderStatus.Confirmed));
        Assert.False(confirmedOrder.CanTransitionTo(OrderStatus.Cancelled));
        Assert.False(completedOrder.CanTransitionTo(OrderStatus.Pending));
        Assert.False(completedOrder.CanTransitionTo(OrderStatus.Confirmed));
    }
}
