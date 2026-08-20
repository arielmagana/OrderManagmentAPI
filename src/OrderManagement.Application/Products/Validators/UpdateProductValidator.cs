namespace OrderManagement.Application.Products.Validators;

using FluentValidation;
using DTOs;

/// <summary>
/// Validator for UpdateProductRequest.
/// Per ADR-008, uses FluentValidation for validation rules.
/// </summary>
public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        // SKU validation (optional but if provided, must be valid)
        When(x => !string.IsNullOrWhiteSpace(x.Sku), () =>
        {
            RuleFor(x => x.Sku)
                .MaximumLength(50)
                .WithMessage("SKU must not exceed 50 characters");
        });

        // Name validation (optional but if provided, must be valid)
        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(200)
                .WithMessage("Name must not exceed 200 characters");
        });

        // Price validation (optional but if provided, must be valid)
        When(x => x.UnitPrice.HasValue, () =>
        {
            RuleFor(x => x.UnitPrice)
                .GreaterThan(0)
                .WithMessage("Unit price must be greater than zero");
        });

        // Stock quantity validation (optional but if provided, must be valid)
        When(x => x.StockQuantity.HasValue, () =>
        {
            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Stock quantity must not be negative");
        });
    }
}
