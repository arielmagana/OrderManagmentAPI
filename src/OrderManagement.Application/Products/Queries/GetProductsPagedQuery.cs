namespace OrderManagement.Application.Products.Queries;

/// <summary>
/// Query to retrieve a paginated list of products.
/// CQRS-style query object passed to handler.
/// </summary>
public class GetProductsPagedQuery
{
    public int Page { get; }
    public int PageSize { get; }

    public GetProductsPagedQuery(int page = 1, int pageSize = 20)
    {
        Page = page;
        PageSize = pageSize;
    }
}
