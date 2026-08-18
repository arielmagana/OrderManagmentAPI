namespace OrderManagement.Application.Orders.DTOs;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
    [Required]
    public string Status { get; set; } = string.Empty;

    [JsonIgnore]
    public string NewStatus { get => Status; set => Status = value; }
}
