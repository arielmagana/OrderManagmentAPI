namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when an order cannot be found (ADR-007: 404 Not Found).
/// </summary>
public class OrderNotFoundException : ApplicationException
{
    public OrderNotFoundException(int orderId)
        : base(
            "ORDER_NOT_FOUND",
            $"Order with ID {orderId} does not exist",
            404)
    {
    }

    public OrderNotFoundException(string message)
        : base(
            "ORDER_NOT_FOUND",
            message,
            404)
    {
    }
}
