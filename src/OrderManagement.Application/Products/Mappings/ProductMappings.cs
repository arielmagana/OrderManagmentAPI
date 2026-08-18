namespace OrderManagement.Application.Products.Mappings;

using Domain.Entities;
using DTOs;

/// <summary>
/// Manual DTO mappings for Product entity.
/// Per ADR-008, explicit mappings instead of AutoMapper.
/// Mappings are co-located with use cases for clarity.
/// </summary>
public static class ProductMappings
{
    /// <summary>
    /// Maps CreateProductRequest to Product domain entity.
    /// Note: IsActive and timestamps are set by the caller.
    /// </summary>
    public static Product ToEntity(CreateProductRequest request)
    {
        return new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
        };
    }

    /// <summary>
    /// Maps Product domain entity to ProductDto response.
    /// </summary>
    public static ProductDto ToDto(Product entity)
    {
        return new ProductDto
        {
            Id = entity.Id,
            Sku = entity.Sku,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            StockQuantity = entity.StockQuantity,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    /// <summary>
    /// Updates an existing Product entity with fields from UpdateProductRequest.
    /// Only non-null fields in the request are applied.
    /// </summary>
    public static void UpdateEntityFromRequest(Product entity, UpdateProductRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Sku))
            entity.Sku = request.Sku;

        if (!string.IsNullOrWhiteSpace(request.Name))
            entity.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Description))
            entity.Description = request.Description;

        if (request.Price.HasValue)
            entity.Price = request.Price.Value;

        if (request.StockQuantity.HasValue)
            entity.StockQuantity = request.StockQuantity.Value;

        if (request.IsActive.HasValue)
            entity.IsActive = request.IsActive.Value;
    }
}
