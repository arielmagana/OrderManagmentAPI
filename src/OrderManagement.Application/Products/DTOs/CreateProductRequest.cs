namespace OrderManagement.Application.Products.DTOs;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Request DTO for creating a new product.
/// Per api.md and ADR-008 (manual DTO mapping).
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// The product SKU (Stock Keeping Unit) - required and must be unique.
    /// </summary>
    [Required]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// The product name (required).
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Detailed product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The unit price (required, must be greater than 0).
    /// </summary>
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal UnitPrice { get; set; }

    /// <summary>Backward-compatible source alias. It is not part of the API contract.</summary>
    [JsonIgnore]
    public decimal Price { get => UnitPrice; set => UnitPrice = value; }

    /// <summary>
    /// Optional: Initial stock quantity (default 0).
    /// </summary>
    public int StockQuantity { get; set; } = 0;
}
