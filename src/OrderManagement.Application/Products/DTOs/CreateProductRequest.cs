namespace OrderManagement.Application.Products.DTOs;

/// <summary>
/// Request DTO for creating a new product.
/// Per api.md and ADR-008 (manual DTO mapping).
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// The product SKU (Stock Keeping Unit) - required and must be unique.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// The product name (required).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Detailed product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The unit price (required, must be greater than 0).
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Optional: Initial stock quantity (default 0).
    /// </summary>
    public int StockQuantity { get; set; } = 0;
}
