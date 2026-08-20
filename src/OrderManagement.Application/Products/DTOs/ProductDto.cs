namespace OrderManagement.Application.Products.DTOs;

using System.Text.Json.Serialization;

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
    [JsonIgnore]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The unit price.
    /// </summary>
    public decimal UnitPrice { get; set; }

    [JsonIgnore]
    public decimal Price { get => UnitPrice; set => UnitPrice = value; }

    /// <summary>
    /// The quantity available in stock.
    /// </summary>
    [JsonIgnore]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Whether the product is active (can be used in orders).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Timestamp when the product record was created.
    /// </summary>
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the product record was last updated.
    /// </summary>
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; }
}
