namespace OrderManagement.IntegrationTests.Application.Products;

using FluentAssertions;
using Xunit;
using OrderManagement.Application.Products.DTOs;
using OrderManagement.Application.Products.Validators;

/// <summary>
/// Unit tests for CreateProductValidator.
/// Per ADR-008, FluentValidation validators are tested to ensure business rule validation.
/// </summary>
public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator;

    public CreateProductValidatorTests()
    {
        _validator = new CreateProductValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_Succeeds()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "PROD-001",
            Name = "Test Product",
            Price = 99.99m
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMissingSku_HasError()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "",
            Name = "Test Product",
            Price = 99.99m
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    [Fact]
    public async Task Validate_WithMissingName_HasError()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "PROD-001",
            Name = "",
            Price = 99.99m
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithNegativePrice_HasError()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "PROD-001",
            Name = "Test Product",
            Price = -10.00m
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task Validate_WithZeroPrice_HasError()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "PROD-001",
            Name = "Test Product",
            Price = 0
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task Validate_WithNegativeStockQuantity_HasError()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "PROD-001",
            Name = "Test Product",
            Price = 99.99m,
            StockQuantity = -5
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StockQuantity");
    }
}
