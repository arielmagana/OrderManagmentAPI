namespace OrderManagement.Domain;

/// <summary>
/// Represents the status of an order throughout its lifecycle.
/// Per ADR-006, orders transition through specific states with validation rules.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been created but not yet confirmed by the customer.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Customer has confirmed the order and payment is accepted.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Order has been fulfilled and delivered.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Order has been cancelled and will not be fulfilled.
    /// </summary>
    Cancelled = 3
}
