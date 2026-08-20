namespace OrderManagement.Application.Orders.Queries;

using Domain;

/// <summary>
/// Query to retrieve a paginated list of orders.
/// CQRS-style query object passed to handler.
/// </summary>
public class GetOrdersPagedQuery
{
    public int Page { get; }
    public int PageSize { get; }
    public int? CustomerId { get; }
    public OrderStatus? Status { get; }

    public GetOrdersPagedQuery(int page = 1, int pageSize = 20, int? customerId = null, OrderStatus? status = null)
    {
        Page = page;
        PageSize = pageSize;
        CustomerId = customerId;
        Status = status;
    }
}
