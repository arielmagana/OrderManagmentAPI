namespace OrderManagement.Application.Products.Commands;

using Common.Exceptions;
using Domain.Repositories;
using Domain.Entities;
using DTOs;
using Mappings;

/// <summary>
/// Handler for CreateProductCommand.
/// Implements business logic: validates SKU uniqueness, creates product, returns DTO.
/// </summary>
public class CreateProductCommandHandler
{
    private readonly IProductRepository _productRepository;

    public CreateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// Handles the create product command.
    /// Throws ApplicationException if SKU already exists.
    /// </summary>
    public async Task<ProductDto> HandleAsync(CreateProductCommand command)
    {
        // Check for duplicate SKU
        if (await _productRepository.ExistsBySkuAsync(command.Request.Sku))
        {
            throw new DuplicateSkuException(command.Request.Sku);
        }

        // Map request to domain entity
        var product = ProductMappings.ToEntity(command.Request);
        product.IsActive = true;
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        // Save to repository
        var createdProduct = await _productRepository.AddAsync(product);

        // Map domain entity back to DTO
        return ProductMappings.ToDto(createdProduct);
    }
}
