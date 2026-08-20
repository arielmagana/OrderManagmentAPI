namespace OrderManagement.Application.Products;

using DTOs;
using Commands;
using Queries;
using Validators;
using Common.Validation;
using FluentValidation;

/// <summary>
/// Service interface for product operations.
/// Abstracts handler classes for dependency injection in API layer.
/// Per ADR-005, controllers depend on this interface, not directly on handlers.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Creates a new product.
    /// </summary>
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);

    /// <summary>
    /// Retrieves a product by ID.
    /// </summary>
    Task<ProductDto> GetProductAsync(int productId);

    /// <summary>
    /// Retrieves a paginated list of products.
    /// </summary>
    Task<Common.Pagination.PaginatedResponse<ProductDto>> GetProductsPagedAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    Task<ProductDto> UpdateProductAsync(int productId, UpdateProductRequest request);
}

/// <summary>
/// Concrete implementation of IProductService.
/// Orchestrates command/query handlers.
/// </summary>
public class ProductService : IProductService
{
    private readonly CreateProductCommandHandler _createCommandHandler;
    private readonly UpdateProductCommandHandler _updateCommandHandler;
    private readonly GetProductQueryHandler _getQueryHandler;
    private readonly GetProductsPagedQueryHandler _getPagedQueryHandler;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductService(
        CreateProductCommandHandler createCommandHandler,
        UpdateProductCommandHandler updateCommandHandler,
        GetProductQueryHandler getQueryHandler,
        GetProductsPagedQueryHandler getPagedQueryHandler,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _createCommandHandler = createCommandHandler;
        _updateCommandHandler = updateCommandHandler;
        _getQueryHandler = getQueryHandler;
        _getPagedQueryHandler = getPagedQueryHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        await RequestValidator.ValidateAsync(_createValidator, request);
        var command = new CreateProductCommand(request);
        return await _createCommandHandler.HandleAsync(command);
    }

    public async Task<ProductDto> GetProductAsync(int productId)
    {
        var query = new GetProductQuery(productId);
        return await _getQueryHandler.HandleAsync(query);
    }

    public async Task<Common.Pagination.PaginatedResponse<ProductDto>> GetProductsPagedAsync(int page = 1, int pageSize = 20)
    {
        RequestValidator.ValidatePagination(page, pageSize);
        var query = new GetProductsPagedQuery(page, pageSize);
        return await _getPagedQueryHandler.HandleAsync(query);
    }

    public async Task<ProductDto> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        await RequestValidator.ValidateAsync(_updateValidator, request);
        var command = new UpdateProductCommand(productId, request);
        return await _updateCommandHandler.HandleAsync(command);
    }
}
