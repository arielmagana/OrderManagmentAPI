namespace OrderManagement.Application.Customers.DTOs;

/// <summary>
/// Request DTO for updating an existing customer.
/// Per ADR-008 (manual DTO mapping).
/// </summary>
public class UpdateCustomerRequest
{
    /// <summary>
    /// Optional: Updated customer name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional: Updated customer email address (must be unique if provided).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Optional: Updated phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Optional: Updated street address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Optional: Updated city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Optional: Updated postal code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Optional: Updated country.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Optional: Updated active status.
    /// </summary>
    public bool? IsActive { get; set; }
}
