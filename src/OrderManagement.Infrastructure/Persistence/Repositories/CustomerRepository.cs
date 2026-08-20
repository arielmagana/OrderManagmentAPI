namespace OrderManagement.Infrastructure.Persistence.Repositories;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class CustomerRepository(OrderManagementDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(int id) => dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id);

    public async Task<IEnumerable<Customer>> GetAllAsync() => await dbContext.Customers.OrderBy(customer => customer.Id).ToListAsync();

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = dbContext.Customers.AsNoTracking();
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(customer => customer.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Customer?> GetByEmailAsync(string email) => dbContext.Customers.SingleOrDefaultAsync(customer => customer.Email == email);

    public async Task<Customer> AddAsync(Customer customer)
    {
        await dbContext.Customers.AddAsync(customer);
        await dbContext.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        if (dbContext.Entry(customer).State == EntityState.Detached)
        {
            dbContext.Customers.Attach(customer);
            dbContext.Entry(customer).State = EntityState.Modified;
        }

        await dbContext.SaveChangesAsync();
        return customer;
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await dbContext.Customers.FindAsync(id);
        if (customer is null) return;
        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync();
    }

    public Task<bool> ExistsByEmailAsync(string email) => dbContext.Customers.AnyAsync(customer => customer.Email == email);
}
