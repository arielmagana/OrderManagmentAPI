namespace OrderManagement.Application.Orders.DTOs;

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
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// The customer who placed this order.
    /// </summary>
    public int CustomerId { get; set; }

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
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// The line items in this order.
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = new();

    /// <summary>
    /// Timestamp when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the order was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
