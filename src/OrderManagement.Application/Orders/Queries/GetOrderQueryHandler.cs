namespace OrderManagement.Application.Orders.Queries;

using Common.Exceptions;
using Domain.Repositories;
using DTOs;
using Mappings;

/// <summary>
/// Handler for GetOrderQuery.
/// Retrieves a single order by ID.
/// Throws OrderNotFoundException if order doesn't exist.
/// </summary>
public class GetOrderQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> HandleAsync(GetOrderQuery query)
    {
        var order = await _orderRepository.GetByIdAsync(query.OrderId);
        if (order == null)
        {
            throw new OrderNotFoundException(query.OrderId);
        }

        return OrderMappings.ToDto(order);
    }
}
