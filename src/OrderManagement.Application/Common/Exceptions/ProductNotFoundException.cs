namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a product cannot be found (ADR-007: 404 Not Found).
/// </summary>
public class ProductNotFoundException : ApplicationException
{
    public ProductNotFoundException(int productId)
        : base(
            "PRODUCT_NOT_FOUND",
            $"Product with ID {productId} does not exist",
            404)
    {
    }

    public ProductNotFoundException(string message)
        : base(
            "PRODUCT_NOT_FOUND",
            message,
            404)
    {
    }
}
