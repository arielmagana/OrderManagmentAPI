namespace OrderManagement.IntegrationTests.Application.Orders;

using Moq;
using FluentAssertions;
using Xunit;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Application.Orders.Commands;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Application.Orders.Queries;
using OrderManagement.Domain;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.Entities;

/// <summary>
/// Integration tests for CreateOrderCommandHandler.
/// Per TDD: complex business logic tests covering:
/// - Customer validation (exists and is active)
/// - Product validation (exists and is active)
/// - Order total calculation
/// - OrderItem creation
/// All per ADR-007 error codes and ADR-006 order status rules.
/// </summary>
public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<ICustomerRepository> _mockCustomerRepo;
    private readonly Mock<IProductRepository> _mockProductRepo;

    public CreateOrderHandlerTests()
    {
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockCustomerRepo = new Mock<ICustomerRepository>();
        _mockProductRepo = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_CreatesOrderAndReturnsDto()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true
        };

        var product = new Product
        {
            Id = 10,
            Sku = "PROD-001",
            Name = "Test Product",
            Price = 100.00m,
            IsActive = true
        };

        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 10, Quantity = 2 }
            },
            Notes = "Special instructions"
        };

        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _mockProductRepo
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(product);

        _mockOrderRepo
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => { o.Id = 1; return o; });

        var command = new CreateOrderCommand(request);
        var handler = new CreateOrderCommandHandler(
            _mockOrderRepo.Object,
            _mockCustomerRepo.Object,
            _mockProductRepo.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be(OrderStatus.Pending.ToString());
        result.TotalAmount.Should().Be(200.00m); // 2 * 100
        result.Items.Should().HaveCount(1);
        result.Notes.Should().Be("Special instructions");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentCustomer_ThrowsCustomerNotFoundException()
    {
        // Arrange
        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Customer?)null);

        var request = new CreateOrderRequest
        {
            CustomerId = 999,
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = 1, Quantity = 1 } }
        };

        var command = new CreateOrderCommand(request);
        var handler = new CreateOrderCommandHandler(
            _mockOrderRepo.Object,
            _mockCustomerRepo.Object,
            _mockProductRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomerNotFoundException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("CUSTOMER_NOT_FOUND");
        exception.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCustomer_ThrowsInactiveCustomerException()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "Inactive Customer",
            Email = "inactive@example.com",
            IsActive = false
        };

        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(customer);

        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = 1, Quantity = 1 } }
        };

        var command = new CreateOrderCommand(request);
        var handler = new CreateOrderCommandHandler(
            _mockOrderRepo.Object,
            _mockCustomerRepo.Object,
            _mockProductRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InactiveCustomerException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("CUSTOMER_INACTIVE");
        exception.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProduct_ThrowsProductNotFoundException()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true
        };

        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(customer);

        _mockProductRepo
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = 999, Quantity = 1 } }
        };

        var command = new CreateOrderCommand(request);
        var handler = new CreateOrderCommandHandler(
            _mockOrderRepo.Object,
            _mockCustomerRepo.Object,
            _mockProductRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("PRODUCT_NOT_FOUND");
        exception.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveProduct_ThrowsInactiveProductException()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true
        };

        var inactiveProduct = new Product
        {
            Id = 10,
            Sku = "PROD-001",
            Name = "Inactive Product",
            Price = 100.00m,
            IsActive = false
        };

        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(customer);

        _mockProductRepo
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(inactiveProduct);

        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = 10, Quantity = 1 } }
        };

        var command = new CreateOrderCommand(request);
        var handler = new CreateOrderCommandHandler(
            _mockOrderRepo.Object,
            _mockCustomerRepo.Object,
            _mockProductRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InactiveProductException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("PRODUCT_INACTIVE");
        exception.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleItems_CalculatesTotalCorrectly()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true
        };

        var product1 = new Product { Id = 10, Sku = "PROD-001", Name = "Product 1", Price = 50.00m, IsActive = true };
        var product2 = new Product { Id = 20, Sku = "PROD-002", Name = "Product 2", Price = 75.00m, IsActive = true };

        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(customer);

        _mockProductRepo
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(product1);

        _mockProductRepo
            .Setup(x => x.GetByIdAsync(20))
            .ReturnsAsync(product2);

        _mockOrderRepo
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => { o.Id = 1; return o; });

        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 10, Quantity = 2 }, // 50 * 2 = 100
                new OrderItemRequest { ProductId = 20, Quantity = 3 }  // 75 * 3 = 225
            }
        };

        var command = new CreateOrderCommand(request);
        var handler = new CreateOrderCommandHandler(
            _mockOrderRepo.Object,
            _mockCustomerRepo.Object,
            _mockProductRepo.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.TotalAmount.Should().Be(325.00m); // 100 + 225
        result.Items.Should().HaveCount(2);
    }
}

/// <summary>
/// Integration tests for ChangeOrderStatusCommandHandler.
/// Per ADR-006, tests validate order status transitions.
/// Valid: Pending→Confirmed, Pending→Cancelled, Confirmed→Completed
/// Invalid: Confirmed→Pending, Confirmed→Cancelled, Completed→*, Cancelled→*
/// </summary>
public class ChangeOrderStatusHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepo;

    public ChangeOrderStatusHandlerTests()
    {
        _mockOrderRepo = new Mock<IOrderRepository>();
    }

    [Fact]
    public async Task HandleAsync_TransitioningPendingToConfirmed_Succeeds()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerId = 1,
            Status = OrderStatus.Pending,
            TotalAmount = 100.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        _mockOrderRepo
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        var request = new ChangeOrderStatusRequest { NewStatus = "Confirmed" };
        var command = new ChangeOrderStatusCommand(1, request);
        var handler = new ChangeOrderStatusCommandHandler(_mockOrderRepo.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Status.Should().Be(OrderStatus.Confirmed.ToString());
    }

    [Fact]
    public async Task HandleAsync_TransitioningPendingToCancelled_Succeeds()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        _mockOrderRepo
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        var request = new ChangeOrderStatusRequest { NewStatus = "Cancelled" };
        var command = new ChangeOrderStatusCommand(1, request);
        var handler = new ChangeOrderStatusCommandHandler(_mockOrderRepo.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Status.Should().Be(OrderStatus.Cancelled.ToString());
    }

    [Fact]
    public async Task HandleAsync_TransitioningConfirmedToCompleted_Succeeds()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            Status = OrderStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        _mockOrderRepo
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        var request = new ChangeOrderStatusRequest { NewStatus = "Completed" };
        var command = new ChangeOrderStatusCommand(1, request);
        var handler = new ChangeOrderStatusCommandHandler(_mockOrderRepo.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Status.Should().Be(OrderStatus.Completed.ToString());
    }

    [Fact]
    public async Task HandleAsync_InvalidTransitionConfirmedToPending_ThrowsInvalidStatusTransitionException()
    {
        // Arrange (per ADR-006, Confirmed cannot go back to Pending)
        var order = new Order
        {
            Id = 1,
            Status = OrderStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        var request = new ChangeOrderStatusRequest { NewStatus = "Pending" };
        var command = new ChangeOrderStatusCommand(1, request);
        var handler = new ChangeOrderStatusCommandHandler(_mockOrderRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("INVALID_STATUS_TRANSITION");
        exception.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task HandleAsync_InvalidTransitionConfirmedToCancelled_ThrowsInvalidStatusTransitionException()
    {
        // Arrange (per ADR-006, Confirmed cannot go to Cancelled)
        var order = new Order
        {
            Id = 1,
            Status = OrderStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        var request = new ChangeOrderStatusRequest { NewStatus = "Cancelled" };
        var command = new ChangeOrderStatusCommand(1, request);
        var handler = new ChangeOrderStatusCommandHandler(_mockOrderRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("INVALID_STATUS_TRANSITION");
        exception.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task HandleAsync_InvalidTransitionCompletedToAny_ThrowsInvalidStatusTransitionException()
    {
        // Arrange (per ADR-006, Completed is immutable)
        var order = new Order
        {
            Id = 1,
            Status = OrderStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        var request = new ChangeOrderStatusRequest { NewStatus = "Cancelled" };
        var command = new ChangeOrderStatusCommand(1, request);
        var handler = new ChangeOrderStatusCommandHandler(_mockOrderRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("INVALID_STATUS_TRANSITION");
        exception.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task HandleAsync_InvalidTransitionCancelledToAny_ThrowsInvalidStatusTransitionException()
    {
        // Arrange (per ADR-006, Cancelled is immutable)
        var order = new Order
        {
            Id = 1,
            Status = OrderStatus.Cancelled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        var request = new ChangeOrderStatusRequest { NewStatus = "Pending" };
        var command = new ChangeOrderStatusCommand(1, request);
        var handler = new ChangeOrderStatusCommandHandler(_mockOrderRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("INVALID_STATUS_TRANSITION");
        exception.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentOrder_ThrowsOrderNotFoundException()
    {
        // Arrange
        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Order?)null);

        var request = new ChangeOrderStatusRequest { NewStatus = "Confirmed" };
        var command = new ChangeOrderStatusCommand(999, request);
        var handler = new ChangeOrderStatusCommandHandler(_mockOrderRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OrderNotFoundException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("ORDER_NOT_FOUND");
        exception.StatusCode.Should().Be(404);
    }
}

/// <summary>
/// Integration tests for GetOrderQueryHandler.
/// </summary>
public class GetOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepo;

    public GetOrderHandlerTests()
    {
        _mockOrderRepo = new Mock<IOrderRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithExistingOrder_ReturnsOrderDto()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerId = 1,
            Status = OrderStatus.Pending,
            TotalAmount = 100.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>()
        };

        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        var query = new GetOrderQuery(1);
        var handler = new GetOrderQueryHandler(_mockOrderRepo.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.OrderNumber.Should().Be("ORD-001");
        result.Status.Should().Be(OrderStatus.Pending.ToString());
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentOrder_ThrowsOrderNotFoundException()
    {
        // Arrange
        _mockOrderRepo
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Order?)null);

        var query = new GetOrderQuery(999);
        var handler = new GetOrderQueryHandler(_mockOrderRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OrderNotFoundException>(
            () => handler.HandleAsync(query));

        exception.Code.Should().Be("ORDER_NOT_FOUND");
        exception.StatusCode.Should().Be(404);
    }
}

/// <summary>
/// Integration tests for GetOrdersPagedQueryHandler.
/// </summary>
public class GetOrdersPagedHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepo;

    public GetOrdersPagedHandlerTests()
    {
        _mockOrderRepo = new Mock<IOrderRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithValidPage_ReturnsPaginatedOrders()
    {
        // Arrange
        var orders = new[]
        {
            new Order { Id = 1, OrderNumber = "ORD-001", CustomerId = 1, Status = OrderStatus.Pending, TotalAmount = 100.00m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, OrderItems = new List<OrderItem>() },
            new Order { Id = 2, OrderNumber = "ORD-002", CustomerId = 1, Status = OrderStatus.Confirmed, TotalAmount = 200.00m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, OrderItems = new List<OrderItem>() }
        };

        _mockOrderRepo
            .Setup(x => x.GetPagedAsync(1, 20, null, null))
            .ReturnsAsync((orders, orders.Length));

        var query = new GetOrdersPagedQuery(page: 1, pageSize: 20);
        var handler = new GetOrdersPagedQueryHandler(_mockOrderRepo.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }
}
