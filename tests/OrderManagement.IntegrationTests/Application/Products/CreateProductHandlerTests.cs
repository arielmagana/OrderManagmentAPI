namespace OrderManagement.IntegrationTests.Application.Products;

using Moq;
using FluentAssertions;
using Xunit;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Application.Products.Commands;
using OrderManagement.Application.Products.DTOs;
using OrderManagement.Application.Products.Queries;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.Entities;

/// <summary>
/// Integration tests for CreateProductCommandHandler.
/// Per TDD: tests run against mocked repositories, covering happy path and error scenarios per ADR-007.
/// </summary>
public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepo;

    public CreateProductHandlerTests()
    {
        _mockProductRepo = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_CreatesProductAndReturnsDto()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "PROD-001",
            Name = "Test Product",
            Description = "A test product",
            Price = 99.99m,
            StockQuantity = 100
        };

        _mockProductRepo
            .Setup(x => x.ExistsBySkuAsync("PROD-001"))
            .ReturnsAsync(false);

        _mockProductRepo
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = 1; return p; });

        var command = new CreateProductCommand(request);
        var handler = new CreateProductCommandHandler(_mockProductRepo.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Sku.Should().Be("PROD-001");
        result.Name.Should().Be("Test Product");
        result.Price.Should().Be(99.99m);
        result.IsActive.Should().BeTrue();

        _mockProductRepo.Verify(
            x => x.ExistsBySkuAsync("PROD-001"),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateSku_ThrowsDuplicateSkuException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "PROD-001",
            Name = "Test Product",
            Price = 99.99m
        };

        _mockProductRepo
            .Setup(x => x.ExistsBySkuAsync("PROD-001"))
            .ReturnsAsync(true);

        var command = new CreateProductCommand(request);
        var handler = new CreateProductCommandHandler(_mockProductRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateSkuException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("DUPLICATE_SKU");
        exception.StatusCode.Should().Be(409);
        exception.Message.Should().Contain("PROD-001");
    }
}

/// <summary>
/// Integration tests for UpdateProductCommandHandler.
/// </summary>
public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepo;

    public UpdateProductHandlerTests()
    {
        _mockProductRepo = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesProductAndReturnsDto()
    {
        // Arrange
        var productId = 1;
        var existingProduct = new Product
        {
            Id = productId,
            Sku = "PROD-001",
            Name = "Old Name",
            Price = 50.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var updateRequest = new UpdateProductRequest
        {
            Name = "New Name",
            Price = 75.00m
        };

        _mockProductRepo
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _mockProductRepo
            .Setup(x => x.UpdateAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        var command = new UpdateProductCommand(productId, updateRequest);
        var handler = new UpdateProductCommandHandler(_mockProductRepo.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        result.Price.Should().Be(75.00m);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProduct_ThrowsProductNotFoundException()
    {
        // Arrange
        var updateRequest = new UpdateProductRequest { Name = "New Name" };

        _mockProductRepo
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var command = new UpdateProductCommand(999, updateRequest);
        var handler = new UpdateProductCommandHandler(_mockProductRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("PRODUCT_NOT_FOUND");
        exception.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateSkuOnUpdate_ThrowsDuplicateSkuException()
    {
        // Arrange
        var productId = 1;
        var existingProduct = new Product
        {
            Id = productId,
            Sku = "PROD-001",
            Name = "Test Product",
            Price = 50.00m
        };

        var updateRequest = new UpdateProductRequest { Sku = "PROD-002" };

        _mockProductRepo
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _mockProductRepo
            .Setup(x => x.ExistsBySkuAsync("PROD-002"))
            .ReturnsAsync(true);

        var command = new UpdateProductCommand(productId, updateRequest);
        var handler = new UpdateProductCommandHandler(_mockProductRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateSkuException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("DUPLICATE_SKU");
        exception.StatusCode.Should().Be(409);
    }
}

/// <summary>
/// Integration tests for GetProductQueryHandler.
/// </summary>
public class GetProductHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepo;

    public GetProductHandlerTests()
    {
        _mockProductRepo = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Sku = "PROD-001",
            Name = "Test Product",
            Price = 99.99m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockProductRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        var query = new GetProductQuery(1);
        var handler = new GetProductQueryHandler(_mockProductRepo.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Sku.Should().Be("PROD-001");
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProduct_ThrowsProductNotFoundException()
    {
        // Arrange
        _mockProductRepo
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var query = new GetProductQuery(999);
        var handler = new GetProductQueryHandler(_mockProductRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => handler.HandleAsync(query));

        exception.Code.Should().Be("PRODUCT_NOT_FOUND");
        exception.StatusCode.Should().Be(404);
    }
}

/// <summary>
/// Integration tests for GetProductsPagedQueryHandler.
/// </summary>
public class GetProductsPagedHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepo;

    public GetProductsPagedHandlerTests()
    {
        _mockProductRepo = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithValidPage_ReturnsPaginatedProducts()
    {
        // Arrange
        var products = new[]
        {
            new Product { Id = 1, Sku = "PROD-001", Name = "Product 1", Price = 10.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 2, Sku = "PROD-002", Name = "Product 2", Price = 20.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _mockProductRepo
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(products);

        var query = new GetProductsPagedQuery(page: 1, pageSize: 20);
        var handler = new GetProductsPagedQueryHandler(_mockProductRepo.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }
}
