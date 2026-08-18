namespace OrderManagement.Application.Customers.Queries;

using Common.Pagination;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for GetCustomersPagedQuery.
/// Retrieves a paginated list of all customers.
/// Per api.md, supports page and pageSize parameters (default pageSize: 20).
/// </summary>
public class GetCustomersPagedQueryHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersPagedQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PaginatedResponse<CustomerDto>> HandleAsync(GetCustomersPagedQuery query)
    {
        // Retrieve all customers
        var allCustomers = await _customerRepository.GetAllAsync();
        var customerList = allCustomers.ToList();

        // Calculate pagination
        var totalCount = customerList.Count;
        var itemsToSkip = (query.Page - 1) * query.PageSize;
        var pagedItems = customerList
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(CustomerMappings.ToDto)
            .ToList();

        return new PaginatedResponse<CustomerDto>(query.Page, query.PageSize, totalCount, pagedItems);
    }
}
