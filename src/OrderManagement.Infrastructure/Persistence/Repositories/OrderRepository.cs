namespace OrderManagement.Infrastructure.Persistence.Repositories;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class OrderRepository(OrderManagementDbContext dbContext) : IOrderRepository
{
    private IQueryable<Order> OrdersWithItems => dbContext.Orders
        .Include(order => order.OrderItems)
        .ThenInclude(item => item.Product);

    public Task<Order?> GetByIdAsync(int id) => OrdersWithItems.SingleOrDefaultAsync(order => order.Id == id);

    public async Task<IEnumerable<Order>> GetAllAsync() => await OrdersWithItems.OrderBy(order => order.Id).ToListAsync();

    public Task<Order?> GetByOrderNumberAsync(string orderNumber) => OrdersWithItems.SingleOrDefaultAsync(order => order.OrderNumber == orderNumber);

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId) => await OrdersWithItems
        .Where(order => order.CustomerId == customerId)
        .OrderBy(order => order.Id)
        .ToListAsync();

    public async Task<Order> AddAsync(Order order)
    {
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        if (dbContext.Entry(order).State == EntityState.Detached)
        {
            // Attach the graph as unchanged and mark only the aggregate root as modified.
            // DbSet.Update would recursively mark products and order items as modified.
            dbContext.Orders.Attach(order);
            dbContext.Entry(order).State = EntityState.Modified;
        }

        await dbContext.SaveChangesAsync();
        return order;
    }

    public async Task DeleteAsync(int id)
    {
        var order = await dbContext.Orders.Include(order => order.OrderItems).SingleOrDefaultAsync(order => order.Id == id);
        if (order is null) return;
        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync();
    }

    public Task<bool> ExistsByOrderNumberAsync(string orderNumber) => dbContext.Orders.AnyAsync(order => order.OrderNumber == orderNumber);
}
