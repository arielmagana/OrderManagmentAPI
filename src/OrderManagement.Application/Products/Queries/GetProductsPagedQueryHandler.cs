namespace OrderManagement.Application.Products.Queries;

using Common.Pagination;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for GetProductsPagedQuery.
/// Retrieves a paginated list of all products.
/// Per api.md, supports page and pageSize parameters (default pageSize: 20).
/// </summary>
public class GetProductsPagedQueryHandler
{
    private readonly IProductRepository _productRepository;

    public GetProductsPagedQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PaginatedResponse<ProductDto>> HandleAsync(GetProductsPagedQuery query)
    {
        // Retrieve all products
        var allProducts = await _productRepository.GetAllAsync();
        var productList = allProducts.ToList();

        // Calculate pagination
        var totalCount = productList.Count;
        var itemsToSkip = (query.Page - 1) * query.PageSize;
        var pagedItems = productList
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(ProductMappings.ToDto)
            .ToList();

        return new PaginatedResponse<ProductDto>(query.Page, query.PageSize, totalCount, pagedItems);
    }
}
