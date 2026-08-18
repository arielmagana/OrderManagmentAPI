namespace OrderManagement.IntegrationTests.Application.Customers;

using Moq;
using FluentAssertions;
using Xunit;
using FluentValidation;
using OrderManagement.Application.Customers.DTOs;
using OrderManagement.Application.Customers.Validators;

/// <summary>
/// Unit tests for CreateCustomerValidator.
/// Per ADR-008, FluentValidation validators are tested to ensure business rule validation.
/// </summary>
public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator;

    public CreateCustomerValidatorTests()
    {
        _validator = new CreateCustomerValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_Succeeds()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@example.com"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMissingName_HasError()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "",
            Email = "john@example.com"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithMissingEmail_HasError()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = ""
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_HasError()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "invalid-email"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_WithNameExceeding100Characters_HasError()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = new string('A', 101),
            Email = "john@example.com"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
}
