namespace OrderManagement.Application.Products.DTOs;

/// <summary>
/// Response DTO for product queries.
/// Per api.md specification.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// The unique identifier of the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The product SKU (Stock Keeping Unit).
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// The product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The product description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The unit price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The quantity available in stock.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Whether the product is active (can be used in orders).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Timestamp when the product record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the product record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
