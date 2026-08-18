namespace OrderManagement.Application.Customers.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request DTO for creating a new customer.
/// Per api.md and ADR-008 (manual DTO mapping).
/// </summary>
public class CreateCustomerRequest
{
    /// <summary>
    /// The customer's full name (required).
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The customer's email address (required, must be unique).
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional: The customer's phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Optional: The customer's street address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Optional: The customer's city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Optional: The customer's postal code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Optional: The customer's country.
    /// </summary>
    public string? Country { get; set; }
}
