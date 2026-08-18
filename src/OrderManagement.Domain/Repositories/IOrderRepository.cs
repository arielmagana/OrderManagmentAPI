namespace OrderManagement.Domain.Repositories;

using Entities;

/// <summary>
/// Repository contract for Order aggregate.
/// Implementations must provide persistence operations.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    Task<Order?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all orders.
    /// </summary>
    Task<IEnumerable<Order>> GetAllAsync();

    /// <summary>
    /// Retrieves an order by its order number.
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber);

    /// <summary>
    /// Retrieves all orders for a specific customer.
    /// </summary>
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);

    /// <summary>
    /// Adds a new order to the repository.
    /// </summary>
    Task<Order> AddAsync(Order order);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    Task<Order> UpdateAsync(Order order);

    /// <summary>
    /// Removes an order from the repository.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if an order with the given order number exists.
    /// </summary>
    Task<bool> ExistsByOrderNumberAsync(string orderNumber);
}
