namespace OrderManagement.UnitTests.Domain;

using OrderManagement.Domain.Entities;

public class ProductEntityTests
{
    [Fact]
    public void CreateProduct_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "High-performance laptop",
            Price = 999.99m,
            StockQuantity = 50,
            Sku = "LAPTOP-001"
        };

        // Assert
        Assert.Equal(1, product.Id);
        Assert.Equal("Laptop", product.Name);
        Assert.Equal("High-performance laptop", product.Description);
        Assert.Equal(999.99m, product.Price);
        Assert.Equal(50, product.StockQuantity);
        Assert.Equal("LAPTOP-001", product.Sku);
    }

    [Fact]
    public void Product_ShouldHaveEmptyOrderItemsCollectionInitially()
    {
        // Arrange & Act
        var product = new Product { Name = "Test Product" };

        // Assert
        Assert.NotNull(product.OrderItems);
        Assert.Empty(product.OrderItems);
    }

    [Fact]
    public void Product_CanHaveOrderItemsAdded()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product" };
        var orderItem = new OrderItem { Id = 1, ProductId = 1 };

        // Act
        product.OrderItems.Add(orderItem);

        // Assert
        Assert.Single(product.OrderItems);
        Assert.Contains(orderItem, product.OrderItems);
    }

    [Fact]
    public void Product_CanHaveZeroPrice()
    {
        // Arrange & Act
        var product = new Product { Price = 0m };

        // Assert
        Assert.Equal(0m, product.Price);
    }

    [Fact]
    public void Product_CanHaveZeroStock()
    {
        // Arrange & Act
        var product = new Product { StockQuantity = 0 };

        // Assert
        Assert.Equal(0, product.StockQuantity);
    }

    [Fact]
    public void Product_CanHaveNegativeStock()
    {
        // Arrange & Act
        var product = new Product { StockQuantity = -10 };

        // Assert
        Assert.Equal(-10, product.StockQuantity);
    }

    [Fact]
    public void Product_PriceCanBeDecimal()
    {
        // Arrange & Act
        var product = new Product { Price = 19.99m };

        // Assert
        Assert.Equal(19.99m, product.Price);
    }
}
