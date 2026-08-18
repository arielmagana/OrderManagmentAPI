namespace OrderManagement.Application.Orders.DTOs;

/// <summary>
/// Request DTO for changing an order's status.
/// Per api.md and ADR-006 (order status transitions).
/// </summary>
public class ChangeOrderStatusRequest
{
    /// <summary>
    /// The new status for the order (required).
    /// Must be a valid status: "Confirmed", "Completed", or "Cancelled".
    /// </summary>
    public string NewStatus { get; set; } = string.Empty;
}
