namespace OrderManagement.Application.Orders.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// Response DTO for order queries.
/// Per api.md specification.
/// </summary>
public class OrderDto
{
    /// <summary>
    /// The unique identifier of the order.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The order number (e.g., ORD-2024-001).
    /// </summary>
    [JsonIgnore]
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// The customer who placed this order.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>The documented time at which the order was created.</summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// The current status of the order.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Special notes or instructions for the order.
    /// </summary>
    [JsonIgnore]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// The line items in this order.
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = new();

    /// <summary>
    /// Timestamp when the order was created.
    /// </summary>
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the order was last updated.
    /// </summary>
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; }
}
