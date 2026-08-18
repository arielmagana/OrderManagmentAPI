namespace OrderManagement.Application.Customers.DTOs;

/// <summary>
/// Response DTO for customer queries.
/// Per api.md specification.
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// The unique identifier of the customer.
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
    /// Whether the customer is active (can be used in orders).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Timestamp when the customer record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the customer record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
