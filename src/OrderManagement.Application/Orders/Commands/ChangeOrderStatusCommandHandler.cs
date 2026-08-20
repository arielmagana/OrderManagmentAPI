namespace OrderManagement.Application.Orders.Commands;

using Common.Exceptions;
using Domain;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for ChangeOrderStatusCommand.
/// Complex business logic per ADR-006:
/// - Validates order exists
/// - Validates status transition is allowed
/// - Updates order status
/// Valid transitions:
///   Pending → Confirmed, Cancelled
///   Confirmed → Completed
///   Completed, Cancelled → immutable (no transitions allowed)
/// </summary>
public class ChangeOrderStatusCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public ChangeOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Handles the change order status command.
    /// Throws exceptions per ADR-007 error codes and ADR-006 transitions.
    /// </summary>
    public async Task<OrderDto> HandleAsync(ChangeOrderStatusCommand command)
    {
        // Retrieve order
        var order = await _orderRepository.GetByIdAsync(command.OrderId);
        if (order == null)
        {
            throw new OrderNotFoundException(command.OrderId);
        }

        // Parse new status
        if (!Enum.TryParse<OrderStatus>(command.Request.Status, ignoreCase: true, out var newStatus))
        {
            throw new InvalidStatusTransitionException(order.Status.ToString(), command.Request.Status);
        }

        // Validate transition is allowed (per ADR-006)
        ValidateStatusTransition(order.Status, newStatus);

        // Delegate the mutation to the domain entity so its invariant remains authoritative.
        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Completed:
                order.Complete();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
            default:
                throw new InvalidStatusTransitionException(order.Status.ToString(), newStatus.ToString());
        }

        // Save to repository
        var updatedOrder = await _orderRepository.UpdateAsync(order);

        // Map to DTO
        return OrderMappings.ToDto(updatedOrder);
    }

    /// <summary>
    /// Validates that the requested status transition is allowed per ADR-006.
    /// Throws InvalidStatusTransitionException if transition is not allowed.
    /// </summary>
    private static void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        var isValid = (currentStatus, newStatus) switch
        {
            // Valid: Pending can go to Confirmed or Cancelled
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            // Valid: Confirmed can go to Completed
            (OrderStatus.Confirmed, OrderStatus.Completed) => true,
            // Invalid: Any transition FROM Completed
            (OrderStatus.Completed, _) => false,
            // Invalid: Any transition FROM Cancelled
            (OrderStatus.Cancelled, _) => false,
            // Invalid: Any other transition
            _ => false,
        };

        if (!isValid)
        {
            throw new InvalidStatusTransitionException(currentStatus.ToString(), newStatus.ToString());
        }
    }
}
