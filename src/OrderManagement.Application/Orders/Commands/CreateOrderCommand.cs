namespace OrderManagement.Application.Orders.Commands;

using DTOs;

/// <summary>
/// Command to create a new order.
/// CQRS-style command object passed to handler.
/// </summary>
public class CreateOrderCommand
{
    public CreateOrderRequest Request { get; }

    public CreateOrderCommand(CreateOrderRequest request)
    {
        Request = request;
    }
}
