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
        var (customers, totalCount) = await _customerRepository.GetPagedAsync(query.Page, query.PageSize);
        var pagedItems = customers
            .Select(CustomerMappings.ToDto)
            .ToList();

        return new PaginatedResponse<CustomerDto>(query.Page, query.PageSize, totalCount, pagedItems);
    }
}
