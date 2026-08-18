namespace OrderManagement.Application.Orders.Validators;

using FluentValidation;
using DTOs;

/// <summary>
/// Validator for CreateOrderRequest.
/// Per ADR-008, uses FluentValidation for complex business rule validation.
/// Validates basic structure; repository checks (customer/product existence) happen in handler.
/// </summary>
public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId must be a valid positive integer");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator());
    }
}

/// <summary>
/// Validator for OrderItemRequest line items.
/// </summary>
public class OrderItemValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("ProductId must be a valid positive integer");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be a positive integer");
    }
}
