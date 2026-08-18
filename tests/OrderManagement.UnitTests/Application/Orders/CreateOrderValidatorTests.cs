namespace OrderManagement.IntegrationTests.Application.Orders;

using FluentAssertions;
using Xunit;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Application.Orders.Validators;

/// <summary>
/// Unit tests for CreateOrderValidator.
/// Per ADR-008, FluentValidation validators are tested to ensure basic validation.
/// Complex validation (customer/product existence, active status) happens in handlers.
/// </summary>
public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator;

    public CreateOrderValidatorTests()
    {
        _validator = new CreateOrderValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_Succeeds()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 10, Quantity = 2 }
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithoutItems_HasError()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<OrderItemRequest>()
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact]
    public async Task Validate_WithInvalidCustomerId_HasError()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = 0,
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 10, Quantity = 2 }
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public async Task Validate_WithInvalidProductId_HasError()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 0, Quantity = 2 }
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items[0].ProductId");
    }

    [Fact]
    public async Task Validate_WithZeroQuantity_HasError()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 10, Quantity = 0 }
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items[0].Quantity");
    }

    [Fact]
    public async Task Validate_WithNegativeQuantity_HasError()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 10, Quantity = -5 }
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items[0].Quantity");
    }
}

/// <summary>
/// Unit tests for ChangeOrderStatusValidator.
/// </summary>
public class ChangeOrderStatusValidatorTests
{
    private readonly ChangeOrderStatusValidator _validator;

    public ChangeOrderStatusValidatorTests()
    {
        _validator = new ChangeOrderStatusValidator();
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Confirmed")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public async Task Validate_WithValidStatus_Succeeds(string status)
    {
        // Arrange
        var request = new ChangeOrderStatusRequest { NewStatus = status };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidStatus_HasError()
    {
        // Arrange
        var request = new ChangeOrderStatusRequest { NewStatus = "InvalidStatus" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }

    [Fact]
    public async Task Validate_WithEmptyStatus_HasError()
    {
        // Arrange
        var request = new ChangeOrderStatusRequest { NewStatus = "" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }
}
