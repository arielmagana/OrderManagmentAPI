namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when attempting to create/update a product with a duplicate SKU (ADR-007: 409 Conflict).
/// </summary>
public class DuplicateSkuException : ApplicationException
{
    public DuplicateSkuException(string sku)
        : base(
            "DUPLICATE_SKU",
            $"SKU '{sku}' already exists",
            409)
    {
    }
}
