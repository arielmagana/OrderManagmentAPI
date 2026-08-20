namespace OrderManagement.Application.Customers.Commands;

using Common.Exceptions;
using Domain.Repositories;
using Domain.Entities;
using DTOs;
using Mappings;

/// <summary>
/// Handler for CreateCustomerCommand.
/// Implements business logic: validates email uniqueness, creates customer, returns DTO.
/// Per TDD: write tests first, then implement handler.
/// </summary>
public class CreateCustomerCommandHandler
{
    private readonly ICustomerRepository _customerRepository;

    public CreateCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    /// <summary>
    /// Handles the create customer command.
    /// Throws ApplicationException if email already exists.
    /// </summary>
    public async Task<CustomerDto> HandleAsync(CreateCustomerCommand command)
    {
        // Check for duplicate email
        if (await _customerRepository.ExistsByEmailAsync(command.Request.Email))
        {
            throw new DuplicateEmailException(command.Request.Email);
        }

        // Map request to domain entity
        var customer = CustomerMappings.ToEntity(command.Request);
        customer.IsActive = true;
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;

        // Save to repository
        var createdCustomer = await _customerRepository.AddAsync(customer);

        // Map domain entity back to DTO
        return CustomerMappings.ToDto(createdCustomer);
    }
}
