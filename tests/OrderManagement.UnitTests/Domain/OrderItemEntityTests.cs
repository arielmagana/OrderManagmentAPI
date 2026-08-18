namespace OrderManagement.UnitTests.Domain;

using OrderManagement.Domain.Entities;

public class OrderItemEntityTests
{
    [Fact]
    public void CreateOrderItem_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var orderItem = new OrderItem
        {
            Id = 1,
            OrderId = 1,
            ProductId = 1,
            Quantity = 5,
            UnitPrice = 29.99m,
            LineTotal = 149.95m
        };

        // Assert
        Assert.Equal(1, orderItem.Id);
        Assert.Equal(1, orderItem.OrderId);
        Assert.Equal(1, orderItem.ProductId);
        Assert.Equal(5, orderItem.Quantity);
        Assert.Equal(29.99m, orderItem.UnitPrice);
        Assert.Equal(149.95m, orderItem.LineTotal);
    }

    [Fact]
    public void CalculateLineTotal_WithQuantityAndPrice_CalculatesCorrectly()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = 3,
            UnitPrice = 10.50m
        };

        // Act
        orderItem.CalculateLineTotal();

        // Assert
        Assert.Equal(31.50m, orderItem.LineTotal);
    }

    [Fact]
    public void CalculateLineTotal_WithZeroQuantity_ReturnsZero()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = 0,
            UnitPrice = 100m
        };

        // Act
        orderItem.CalculateLineTotal();

        // Assert
        Assert.Equal(0m, orderItem.LineTotal);
    }

    [Fact]
    public void CalculateLineTotal_WithZeroPrice_ReturnsZero()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = 10,
            UnitPrice = 0m
        };

        // Act
        orderItem.CalculateLineTotal();

        // Assert
        Assert.Equal(0m, orderItem.LineTotal);
    }

    [Fact]
    public void CalculateLineTotal_WithDecimalValues_HandlesDecimalArithmetic()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = 7,
            UnitPrice = 12.99m
        };

        // Act
        orderItem.CalculateLineTotal();

        // Assert
        Assert.Equal(90.93m, orderItem.LineTotal);
    }

    [Fact]
    public void OrderItem_CanHaveNegativeQuantity()
    {
        // Arrange & Act
        var orderItem = new OrderItem { Quantity = -5 };

        // Assert
        Assert.Equal(-5, orderItem.Quantity);
    }

    [Fact]
    public void OrderItem_NavigationPropertiesToOrderAndProduct()
    {
        // Arrange
        var order = new Order { Id = 1 };
        var product = new Product { Id = 1 };
        var orderItem = new OrderItem
        {
            OrderId = 1,
            ProductId = 1,
            Order = order,
            Product = product
        };

        // Act & Assert
        Assert.NotNull(orderItem.Order);
        Assert.NotNull(orderItem.Product);
        Assert.Equal(order.Id, orderItem.Order.Id);
        Assert.Equal(product.Id, orderItem.Product.Id);
    }
}
