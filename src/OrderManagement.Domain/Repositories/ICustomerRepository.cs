namespace OrderManagement.Domain.Repositories;

using Entities;

/// <summary>
/// Repository contract for Customer aggregate.
/// Implementations must provide persistence operations.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
    Task<Customer?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all customers.
    /// </summary>
    Task<IEnumerable<Customer>> GetAllAsync();

    /// <summary>
    /// Retrieves a customer by email address.
    /// </summary>
    Task<Customer?> GetByEmailAsync(string email);

    /// <summary>
    /// Adds a new customer to the repository.
    /// </summary>
    Task<Customer> AddAsync(Customer customer);

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    Task<Customer> UpdateAsync(Customer customer);

    /// <summary>
    /// Removes a customer from the repository.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if a customer with the given email exists.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);
}
