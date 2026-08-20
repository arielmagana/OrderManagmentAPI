namespace OrderManagement.Application.Products.Queries;

/// <summary>
/// Query to retrieve a single product by ID.
/// CQRS-style query object passed to handler.
/// </summary>
public class GetProductQuery
{
    public int ProductId { get; }

    public GetProductQuery(int productId)
    {
        ProductId = productId;
    }
}
