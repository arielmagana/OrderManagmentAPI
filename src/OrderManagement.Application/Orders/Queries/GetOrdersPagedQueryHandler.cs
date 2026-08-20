namespace OrderManagement.Application.Orders.Queries;

using Common.Pagination;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for GetOrdersPagedQuery.
/// Retrieves a filtered, paginated list of orders.
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
        var (orders, totalCount) = await _orderRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.CustomerId,
            query.Status);
        var pagedItems = orders
            .Select(OrderMappings.ToDto)
            .ToList();

        return new PaginatedResponse<OrderDto>(query.Page, query.PageSize, totalCount, pagedItems);
    }
}
