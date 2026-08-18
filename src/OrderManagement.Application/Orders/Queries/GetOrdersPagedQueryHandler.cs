namespace OrderManagement.Application.Orders.Queries;

using Common.Pagination;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for GetOrdersPagedQuery.
/// Retrieves a paginated list of all orders.
/// Per api.md, supports page and pageSize parameters (default pageSize: 20).
/// </summary>
public class GetOrdersPagedQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersPagedQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PaginatedResponse<OrderDto>> HandleAsync(GetOrdersPagedQuery query)
    {
        // Retrieve all orders
        var allOrders = await _orderRepository.GetAllAsync();
        var orderList = allOrders.ToList();

        // Calculate pagination
        var totalCount = orderList.Count;
        var itemsToSkip = (query.Page - 1) * query.PageSize;
        var pagedItems = orderList
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(OrderMappings.ToDto)
            .ToList();

        return new PaginatedResponse<OrderDto>(query.Page, query.PageSize, totalCount, pagedItems);
    }
}
