namespace OrderManagement.Application.Orders.Validators;

using FluentValidation;
using DTOs;
using Domain;

/// <summary>
/// Validator for ChangeOrderStatusRequest.
/// Per ADR-006, validates that the new status is a valid enum value.
/// Transition validation (current status → new status) happens in handler.
/// </summary>
public class ChangeOrderStatusValidator : AbstractValidator<ChangeOrderStatusRequest>
{
    public ChangeOrderStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .WithMessage("NewStatus is required")
            .Must(IsValidOrderStatus)
            .WithMessage("NewStatus must be one of: Pending, Confirmed, Completed, Cancelled");
    }

    private static bool IsValidOrderStatus(string status)
    {
        return Enum.TryParse<OrderStatus>(status, ignoreCase: true, out _);
    }
}
