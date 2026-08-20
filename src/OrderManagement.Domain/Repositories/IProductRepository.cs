namespace OrderManagement.Domain.Repositories;

using Entities;

/// <summary>
/// Repository contract for Product aggregate.
/// Implementations must provide persistence operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all products.
    /// </summary>
    Task<IEnumerable<Product>> GetAllAsync();

    /// <summary>
    /// Retrieves one page of products and the total number of products.
    /// </summary>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Retrieves a product by its SKU.
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku);

    /// <summary>
    /// Adds a new product to the repository.
    /// </summary>
    Task<Product> AddAsync(Product product);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    Task<Product> UpdateAsync(Product product);

    /// <summary>
    /// Removes a product from the repository.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if a product with the given SKU exists.
    /// </summary>
    Task<bool> ExistsBySkuAsync(string sku);
}
