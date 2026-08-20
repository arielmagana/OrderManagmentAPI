namespace OrderManagement.Application.Orders.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// Response DTO for a single order line item.
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// The unique identifier of the order item.
    /// </summary>
    [JsonIgnore]
    public int Id { get; set; }

    /// <summary>
    /// The product ID.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// The product name.
    /// </summary>
    [JsonIgnore]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// The quantity ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// The unit price at the time of order.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// The line total (Quantity × UnitPrice).
    /// </summary>
    public decimal Subtotal { get; set; }

    [JsonIgnore]
    public decimal LineTotal { get => Subtotal; set => Subtotal = value; }
}
