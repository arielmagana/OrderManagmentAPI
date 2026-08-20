namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents an order placed by a customer.
/// Per ADR-006, orders transition through specific states with defined rules.
/// </summary>
public class Order
{
    /// <summary>
    /// Unique identifier for the order.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference number for the order (e.g., ORD-2024-001).
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// The customer who placed this order.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Navigation property to the associated customer.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// The current status of the order.
    /// Starts as Pending, transitions through Confirmed to Completed or Cancelled.
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// The total amount of the order (sum of all line items).
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Notes or special instructions for the order.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the order was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property for items in this order.
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Attempts to confirm the order (transition from Pending to Confirmed).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the order cannot transition to Confirmed status.</exception>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot confirm order. Current status is '{Status}'. Only Pending orders can be confirmed.");
        }

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Attempts to complete the order (transition from Confirmed to Completed).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the order cannot transition to Completed status.</exception>
    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException(
                $"Cannot complete order. Current status is '{Status}'. Only Confirmed orders can be completed.");
        }

        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Attempts to cancel the order.
    /// Can only be cancelled if the order is in Pending status.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the order cannot be cancelled.</exception>
    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot cancel order. Current status is '{Status}'. Only Pending orders can be cancelled.");
        }

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the order can transition to the specified status.
    /// </summary>
    public bool CanTransitionTo(OrderStatus newStatus)
    {
        return (Status, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Completed) => true,
            _ => false
        };
    }
}
