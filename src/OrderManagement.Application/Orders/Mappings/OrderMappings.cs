namespace OrderManagement.Application.Orders.Mappings;

using Domain.Entities;
using DTOs;

/// <summary>
/// Manual DTO mappings for Order and OrderItem entities.
/// Per ADR-008, explicit mappings instead of AutoMapper.
/// Mappings are co-located with use cases for clarity.
/// </summary>
public static class OrderMappings
{
    /// <summary>
    /// Maps Order domain entity to OrderDto response.
    /// Includes nested OrderItem DTOs.
    /// </summary>
    public static OrderDto ToDto(Order entity)
    {
        return new OrderDto
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            CustomerId = entity.CustomerId,
            Status = entity.Status.ToString(),
            TotalAmount = entity.TotalAmount,
            Notes = entity.Notes,
            Items = entity.OrderItems.Select(ToDto).ToList(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    /// <summary>
    /// Maps OrderItem domain entity to OrderItemDto response.
    /// </summary>
    public static OrderItemDto ToDto(OrderItem entity)
    {
        return new OrderItemDto
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductName = entity.Product?.Name ?? string.Empty,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            LineTotal = entity.LineTotal,
        };
    }
}
