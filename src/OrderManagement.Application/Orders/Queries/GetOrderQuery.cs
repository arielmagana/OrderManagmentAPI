namespace OrderManagement.Application.Orders.Queries;

/// <summary>
/// Query to retrieve a single order by ID.
/// CQRS-style query object passed to handler.
/// </summary>
public class GetOrderQuery
{
    public int OrderId { get; }

    public GetOrderQuery(int orderId)
    {
        OrderId = orderId;
    }
}
