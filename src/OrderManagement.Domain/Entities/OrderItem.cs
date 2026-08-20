namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents a single line item within an order.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Unique identifier for the order item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The order that contains this item.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Navigation property to the associated order.
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// The product being ordered.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Navigation property to the associated product.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// The quantity of the product ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// The unit price of the product at the time of the order.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// The line total (Quantity × UnitPrice).
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Calculates the line total based on quantity and unit price.
    /// </summary>
    public void CalculateLineTotal()
    {
        LineTotal = Quantity * UnitPrice;
    }
}
