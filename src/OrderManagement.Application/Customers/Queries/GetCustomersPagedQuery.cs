namespace OrderManagement.Application.Customers.Queries;

/// <summary>
/// Query to retrieve a paginated list of customers.
/// CQRS-style query object passed to handler.
/// </summary>
public class GetCustomersPagedQuery
{
    public int Page { get; }
    public int PageSize { get; }

    public GetCustomersPagedQuery(int page = 1, int pageSize = 20)
    {
        Page = page;
        PageSize = pageSize;
    }
}
