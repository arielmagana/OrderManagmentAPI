namespace OrderManagement.Application.Orders;

using DTOs;
using Commands;
using Queries;
using Validators;
using Common.Validation;
using FluentValidation;

/// <summary>
/// Service interface for order operations.
/// Abstracts handler classes for dependency injection in API layer.
/// Per ADR-005, controllers depend on this interface, not directly on handlers.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Creates a new order.
    /// </summary>
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);

    /// <summary>
    /// Retrieves an order by ID.
    /// </summary>
    Task<OrderDto> GetOrderAsync(int orderId);

    /// <summary>
    /// Retrieves a paginated list of orders.
    /// </summary>
    Task<Common.Pagination.PaginatedResponse<OrderDto>> GetOrdersPagedAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Changes an order's status (per ADR-006 transitions).
    /// </summary>
    Task<OrderDto> ChangeOrderStatusAsync(int orderId, ChangeOrderStatusRequest request);
}

/// <summary>
/// Concrete implementation of IOrderService.
/// Orchestrates command/query handlers.
/// </summary>
public class OrderService : IOrderService
{
    private readonly CreateOrderCommandHandler _createCommandHandler;
    private readonly ChangeOrderStatusCommandHandler _changeStatusCommandHandler;
    private readonly GetOrderQueryHandler _getQueryHandler;
    private readonly GetOrdersPagedQueryHandler _getPagedQueryHandler;
    private readonly IValidator<CreateOrderRequest> _createValidator;
    private readonly IValidator<ChangeOrderStatusRequest> _changeStatusValidator;

    public OrderService(
        CreateOrderCommandHandler createCommandHandler,
        ChangeOrderStatusCommandHandler changeStatusCommandHandler,
        GetOrderQueryHandler getQueryHandler,
        GetOrdersPagedQueryHandler getPagedQueryHandler,
        IValidator<CreateOrderRequest> createValidator,
        IValidator<ChangeOrderStatusRequest> changeStatusValidator)
    {
        _createCommandHandler = createCommandHandler;
        _changeStatusCommandHandler = changeStatusCommandHandler;
        _getQueryHandler = getQueryHandler;
        _getPagedQueryHandler = getPagedQueryHandler;
        _createValidator = createValidator;
        _changeStatusValidator = changeStatusValidator;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        await RequestValidator.ValidateAsync(_createValidator, request);
        var command = new CreateOrderCommand(request);
        return await _createCommandHandler.HandleAsync(command);
    }

    public async Task<OrderDto> GetOrderAsync(int orderId)
    {
        var query = new GetOrderQuery(orderId);
        return await _getQueryHandler.HandleAsync(query);
    }

    public async Task<Common.Pagination.PaginatedResponse<OrderDto>> GetOrdersPagedAsync(int page = 1, int pageSize = 20)
    {
        RequestValidator.ValidatePagination(page, pageSize);
        var query = new GetOrdersPagedQuery(page, pageSize);
        return await _getPagedQueryHandler.HandleAsync(query);
    }

    public async Task<OrderDto> ChangeOrderStatusAsync(int orderId, ChangeOrderStatusRequest request)
    {
        await RequestValidator.ValidateAsync(_changeStatusValidator, request);
        var command = new ChangeOrderStatusCommand(orderId, request);
        return await _changeStatusCommandHandler.HandleAsync(command);
    }
}
