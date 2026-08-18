namespace OrderManagement.Application.Orders.Queries;

/// <summary>
/// Query to retrieve a paginated list of orders.
/// CQRS-style query object passed to handler.
/// </summary>
public class GetOrdersPagedQuery
{
    public int Page { get; }
    public int PageSize { get; }

    public GetOrdersPagedQuery(int page = 1, int pageSize = 20)
    {
        Page = page;
        PageSize = pageSize;
    }
}
