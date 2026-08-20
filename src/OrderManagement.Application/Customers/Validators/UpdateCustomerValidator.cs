namespace OrderManagement.Application.Customers.Validators;

using FluentValidation;
using DTOs;

/// <summary>
/// Validator for UpdateCustomerRequest.
/// Per ADR-008, uses FluentValidation for validation rules.
/// </summary>
public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerValidator()
    {
        // Name validation (optional but if provided, must be valid)
        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Name must not exceed 100 characters");
        });

        // Email validation (optional but if provided, must be valid)
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Email address must be in valid format");
        });
    }
}
