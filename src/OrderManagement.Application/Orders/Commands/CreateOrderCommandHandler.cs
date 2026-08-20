namespace OrderManagement.Application.Orders.Commands;

using Common.Exceptions;
using Domain;
using Domain.Repositories;
using Domain.Entities;
using DTOs;
using Mappings;

/// <summary>
/// Handler for CreateOrderCommand.
/// Complex business logic:
/// - Validates customer exists and is active
/// - Validates products exist and are active
/// - Calculates order total
/// - Creates OrderItems
/// Per ADR-006, all new orders start in Pending status.
/// </summary>
public class CreateOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
    }

    /// <summary>
    /// Handles the create order command.
    /// Throws exceptions per ADR-007 error codes.
    /// </summary>
    public async Task<OrderDto> HandleAsync(CreateOrderCommand command)
    {
        // Validate customer exists and is active
        var customer = await _customerRepository.GetByIdAsync(command.Request.CustomerId);
        if (customer == null)
        {
            throw new CustomerNotFoundException(command.Request.CustomerId);
        }

        if (!customer.IsActive)
        {
            throw new InactiveCustomerException(command.Request.CustomerId);
        }

        // Validate all products exist and are active
        decimal orderTotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var itemRequest in command.Request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
            if (product == null)
            {
                throw new ProductNotFoundException(itemRequest.ProductId);
            }

            if (!product.IsActive)
            {
                throw new InactiveProductException(itemRequest.ProductId);
            }

            // Create OrderItem with current product price
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price,
                LineTotal = itemRequest.Quantity * product.Price,
            };

            orderItems.Add(orderItem);
            orderTotal += orderItem.LineTotal;
        }

        // Create order (always starts in Pending status per ADR-006)
        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().GetHashCode() % 10000}",
            CustomerId = customer.Id,
            Status = OrderStatus.Pending,
            TotalAmount = orderTotal,
            Notes = command.Request.Notes ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Add items to order
        foreach (var item in orderItems)
        {
            order.OrderItems.Add(item);
        }

        // Save to repository
        var createdOrder = await _orderRepository.AddAsync(order);

        // Map to DTO
        return OrderMappings.ToDto(createdOrder);
    }
}
