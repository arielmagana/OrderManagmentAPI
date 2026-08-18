namespace OrderManagement.Application.Products.Commands;

using Common.Exceptions;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for UpdateProductCommand.
/// Implements business logic: validates product exists, SKU uniqueness, updates product.
/// </summary>
public class UpdateProductCommandHandler
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// Handles the update product command.
    /// Throws ProductNotFoundException if product doesn't exist.
    /// Throws DuplicateSkuException if SKU already exists (and is being changed).
    /// </summary>
    public async Task<ProductDto> HandleAsync(UpdateProductCommand command)
    {
        // Retrieve existing product
        var product = await _productRepository.GetByIdAsync(command.ProductId);
        if (product == null)
        {
            throw new ProductNotFoundException(command.ProductId);
        }

        // Check for duplicate SKU if SKU is being updated
        if (!string.IsNullOrWhiteSpace(command.Request.Sku) &&
            command.Request.Sku != product.Sku &&
            await _productRepository.ExistsBySkuAsync(command.Request.Sku))
        {
            throw new DuplicateSkuException(command.Request.Sku);
        }

        // Apply updates (only non-null fields)
        ProductMappings.UpdateEntityFromRequest(product, command.Request);
        product.UpdatedAt = DateTime.UtcNow;

        // Save to repository
        var updatedProduct = await _productRepository.UpdateAsync(product);

        // Map back to DTO
        return ProductMappings.ToDto(updatedProduct);
    }
}
