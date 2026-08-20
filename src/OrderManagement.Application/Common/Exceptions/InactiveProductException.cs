namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when attempting to use an inactive product in an order (ADR-007: 409 Conflict).
/// </summary>
public class InactiveProductException : ApplicationException
{
    public InactiveProductException(int productId)
        : base(
            "PRODUCT_INACTIVE",
            $"Cannot add inactive product (ID: {productId}) to order",
            409)
    {
    }
}
