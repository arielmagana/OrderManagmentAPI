namespace OrderManagement.Application.Customers.DTOs;

using System.Text.Json.Serialization;

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
    [JsonIgnore]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// The customer's street address.
    /// </summary>
    [JsonIgnore]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The customer's city.
    /// </summary>
    [JsonIgnore]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// The customer's postal code.
    /// </summary>
    [JsonIgnore]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// The customer's country.
    /// </summary>
    [JsonIgnore]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Whether the customer is active (can be used in orders).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Timestamp when the customer record was created.
    /// </summary>
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the customer record was last updated.
    /// </summary>
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; }
}
