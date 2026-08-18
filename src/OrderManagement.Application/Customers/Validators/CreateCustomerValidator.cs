namespace OrderManagement.Application.Customers.Validators;

using FluentValidation;
using DTOs;

/// <summary>
/// Validator for CreateCustomerRequest.
/// Per ADR-008, uses FluentValidation for complex business rule validation.
/// Basic constraints use Data Annotations; complex rules here.
/// </summary>
public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email address must be in valid format");
    }
}
