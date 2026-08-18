namespace OrderManagement.Application.Customers.Commands;

using Common.Exceptions;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for UpdateCustomerCommand.
/// Implements business logic: validates customer exists, email uniqueness, updates customer.
/// </summary>
public class UpdateCustomerCommandHandler
{
    private readonly ICustomerRepository _customerRepository;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    /// <summary>
    /// Handles the update customer command.
    /// Throws CustomerNotFoundException if customer doesn't exist.
    /// Throws DuplicateEmailException if email already exists (and is being changed).
    /// </summary>
    public async Task<CustomerDto> HandleAsync(UpdateCustomerCommand command)
    {
        // Retrieve existing customer
        var customer = await _customerRepository.GetByIdAsync(command.CustomerId);
        if (customer == null)
        {
            throw new CustomerNotFoundException(command.CustomerId);
        }

        // Check for duplicate email if email is being updated
        if (!string.IsNullOrWhiteSpace(command.Request.Email) &&
            command.Request.Email != customer.Email &&
            await _customerRepository.ExistsByEmailAsync(command.Request.Email))
        {
            throw new DuplicateEmailException(command.Request.Email);
        }

        // Apply updates (only non-null fields)
        CustomerMappings.UpdateEntityFromRequest(customer, command.Request);
        customer.UpdatedAt = DateTime.UtcNow;

        // Save to repository
        var updatedCustomer = await _customerRepository.UpdateAsync(customer);

        // Map back to DTO
        return CustomerMappings.ToDto(updatedCustomer);
    }
}
