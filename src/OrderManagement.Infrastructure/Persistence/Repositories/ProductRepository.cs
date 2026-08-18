namespace OrderManagement.Infrastructure.Persistence.Repositories;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class ProductRepository(OrderManagementDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id) => dbContext.Products.SingleOrDefaultAsync(product => product.Id == id);

    public async Task<IEnumerable<Product>> GetAllAsync() => await dbContext.Products.OrderBy(product => product.Id).ToListAsync();

    public Task<Product?> GetBySkuAsync(string sku) => dbContext.Products.SingleOrDefaultAsync(product => product.Sku == sku);

    public async Task<Product> AddAsync(Product product)
    {
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await dbContext.Products.FindAsync(id);
        if (product is null) return;
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();
    }

    public Task<bool> ExistsBySkuAsync(string sku) => dbContext.Products.AnyAsync(product => product.Sku == sku);
}
