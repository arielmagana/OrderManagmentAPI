namespace OrderManagement.Application.Products.Queries;

using Common.Exceptions;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for GetProductQuery.
/// Retrieves a single product by ID.
/// Throws ProductNotFoundException if product doesn't exist.
/// </summary>
public class GetProductQueryHandler
{
    private readonly IProductRepository _productRepository;

    public GetProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> HandleAsync(GetProductQuery query)
    {
        var product = await _productRepository.GetByIdAsync(query.ProductId);
        if (product == null)
        {
            throw new ProductNotFoundException(query.ProductId);
        }

        return ProductMappings.ToDto(product);
    }
}
