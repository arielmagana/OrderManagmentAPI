namespace OrderManagement.Application.Orders.DTOs;

/// <summary>
/// Request DTO for creating a new order.
/// Per api.md and ADR-008 (manual DTO mapping).
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// The ID of the customer placing the order (required).
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// The order line items (required, must not be empty).
    /// </summary>
    public List<OrderItemRequest> Items { get; set; } = new();

    /// <summary>
    /// Optional: Special notes or instructions for the order.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Represents a single line item in an order request.
/// </summary>
public class OrderItemRequest
{
    /// <summary>
    /// The product ID (required).
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// The quantity ordered (required, must be positive).
    /// </summary>
    public int Quantity { get; set; }
}
