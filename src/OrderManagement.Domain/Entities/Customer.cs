namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents a customer in the order management system.
/// </summary>
public class Customer
{
    /// <summary>
    /// Unique identifier for the customer.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The customer's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The customer's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The customer's phone number.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// The customer's street address.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The customer's city.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// The customer's postal code.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// The customer's country.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the customer record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the customer record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property for orders placed by this customer.
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
