namespace OrderManagement.Application.Orders.Commands;

using DTOs;

/// <summary>
/// Command to change an order's status.
/// CQRS-style command object passed to handler.
/// Per ADR-006, status transitions are validated in the handler.
/// </summary>
public class ChangeOrderStatusCommand
{
    public int OrderId { get; }
    public ChangeOrderStatusRequest Request { get; }

    public ChangeOrderStatusCommand(int orderId, ChangeOrderStatusRequest request)
    {
        OrderId = orderId;
        Request = request;
    }
}
