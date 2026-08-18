namespace OrderManagement.Application.Customers.Mappings;

using Domain.Entities;
using DTOs;

/// <summary>
/// Manual DTO mappings for Customer entity.
/// Per ADR-008, explicit mappings instead of AutoMapper.
/// Mappings are co-located with use cases for clarity.
/// </summary>
public static class CustomerMappings
{
    /// <summary>
    /// Maps CreateCustomerRequest to Customer domain entity.
    /// Note: IsActive and timestamps are set by the caller.
    /// </summary>
    public static Customer ToEntity(CreateCustomerRequest request)
    {
        return new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone ?? string.Empty,
            Address = request.Address ?? string.Empty,
            City = request.City ?? string.Empty,
            PostalCode = request.PostalCode ?? string.Empty,
            Country = request.Country ?? string.Empty,
        };
    }

    /// <summary>
    /// Maps Customer domain entity to CustomerDto response.
    /// </summary>
    public static CustomerDto ToDto(Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    /// <summary>
    /// Updates an existing Customer entity with fields from UpdateCustomerRequest.
    /// Only non-null fields in the request are applied.
    /// </summary>
    public static void UpdateEntityFromRequest(Customer entity, UpdateCustomerRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Name))
            entity.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Email))
            entity.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.Phone))
            entity.Phone = request.Phone;

        if (!string.IsNullOrWhiteSpace(request.Address))
            entity.Address = request.Address;

        if (!string.IsNullOrWhiteSpace(request.City))
            entity.City = request.City;

        if (!string.IsNullOrWhiteSpace(request.PostalCode))
            entity.PostalCode = request.PostalCode;

        if (!string.IsNullOrWhiteSpace(request.Country))
            entity.Country = request.Country;

        if (request.IsActive.HasValue)
            entity.IsActive = request.IsActive.Value;
    }
}
