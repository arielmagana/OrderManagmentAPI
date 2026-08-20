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
        var (products, totalCount) = await _productRepository.GetPagedAsync(query.Page, query.PageSize);
        var pagedItems = products
            .Select(ProductMappings.ToDto)
            .ToList();

        return new PaginatedResponse<ProductDto>(query.Page, query.PageSize, totalCount, pagedItems);
    }
}
