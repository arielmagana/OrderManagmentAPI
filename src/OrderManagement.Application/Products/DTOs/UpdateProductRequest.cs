namespace OrderManagement.Application.Products.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// Request DTO for updating an existing product.
/// Per ADR-008 (manual DTO mapping).
/// </summary>
public class UpdateProductRequest
{
    /// <summary>
    /// Optional: Updated product SKU (must be unique if provided).
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Optional: Updated product name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional: Updated product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional: Updated unit price (must be greater than 0 if provided).
    /// </summary>
    public decimal? UnitPrice { get; set; }

    [JsonIgnore]
    public decimal? Price { get => UnitPrice; set => UnitPrice = value; }

    /// <summary>
    /// Optional: Updated stock quantity.
    /// </summary>
    public int? StockQuantity { get; set; }

    /// <summary>
    /// Optional: Updated active status.
    /// </summary>
    public bool? IsActive { get; set; }
}
