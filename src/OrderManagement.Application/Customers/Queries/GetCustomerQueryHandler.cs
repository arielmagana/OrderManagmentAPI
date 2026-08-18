namespace OrderManagement.Application.Customers.Queries;

using Common.Exceptions;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for GetCustomerQuery.
/// Retrieves a single customer by ID.
/// Throws CustomerNotFoundException if customer doesn't exist.
/// </summary>
public class GetCustomerQueryHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> HandleAsync(GetCustomerQuery query)
    {
        var customer = await _customerRepository.GetByIdAsync(query.CustomerId);
        if (customer == null)
        {
            throw new CustomerNotFoundException(query.CustomerId);
        }

        return CustomerMappings.ToDto(customer);
    }
}
