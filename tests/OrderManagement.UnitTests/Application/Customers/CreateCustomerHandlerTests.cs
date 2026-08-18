namespace OrderManagement.IntegrationTests.Application.Customers;

using Moq;
using FluentAssertions;
using Xunit;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Application.Common.Pagination;
using OrderManagement.Application.Customers.Commands;
using OrderManagement.Application.Customers.DTOs;
using OrderManagement.Application.Customers.Queries;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.Entities;

/// <summary>
/// Integration tests for CreateCustomerCommandHandler.
/// Per TDD: tests run against mocked repositories, covering happy path and error scenarios per ADR-007.
/// </summary>
public class CreateCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepo;

    public CreateCustomerHandlerTests()
    {
        _mockCustomerRepo = new Mock<ICustomerRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_CreatesCustomerAndReturnsDto()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "123-456-7890",
            City = "Springfield"
        };

        // Mock: email doesn't exist yet
        _mockCustomerRepo
            .Setup(x => x.ExistsByEmailAsync("john@example.com"))
            .ReturnsAsync(false);

        // Mock: AddAsync returns the created customer with ID
        _mockCustomerRepo
            .Setup(x => x.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) =>
            {
                c.Id = 1;
                return c;
            });

        var command = new CreateCustomerCommand(request);
        var handler = new CreateCustomerCommandHandler(_mockCustomerRepo.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        result.IsActive.Should().BeTrue();
        result.Phone.Should().Be("123-456-7890");
        result.City.Should().Be("Springfield");

        _mockCustomerRepo.Verify(
            x => x.ExistsByEmailAsync("john@example.com"),
            Times.Once);
        _mockCustomerRepo.Verify(
            x => x.AddAsync(It.IsAny<Customer>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateEmail_ThrowsDuplicateEmailException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com"
        };

        // Mock: email already exists
        _mockCustomerRepo
            .Setup(x => x.ExistsByEmailAsync("jane@example.com"))
            .ReturnsAsync(true);

        var command = new CreateCustomerCommand(request);
        var handler = new CreateCustomerCommandHandler(_mockCustomerRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateEmailException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("DUPLICATE_EMAIL");
        exception.StatusCode.Should().Be(409);
        exception.Message.Should().Contain("jane@example.com");

        _mockCustomerRepo.Verify(
            x => x.AddAsync(It.IsAny<Customer>()),
            Times.Never);
    }

}

/// <summary>
/// Integration tests for UpdateCustomerCommandHandler.
/// </summary>
public class UpdateCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepo;

    public UpdateCustomerHandlerTests()
    {
        _mockCustomerRepo = new Mock<ICustomerRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesCustomerAndReturnsDto()
    {
        // Arrange
        var customerId = 1;
        var existingCustomer = new Customer
        {
            Id = customerId,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var updateRequest = new UpdateCustomerRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com"
        };

        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);

        // New email doesn't exist
        _mockCustomerRepo
            .Setup(x => x.ExistsByEmailAsync("jane@example.com"))
            .ReturnsAsync(false);

        _mockCustomerRepo
            .Setup(x => x.UpdateAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => c);

        var command = new UpdateCustomerCommand(customerId, updateRequest);
        var handler = new UpdateCustomerCommandHandler(_mockCustomerRepo.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.Name.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentCustomer_ThrowsCustomerNotFoundException()
    {
        // Arrange
        var customerId = 999;
        var updateRequest = new UpdateCustomerRequest { Name = "Jane Doe" };

        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        var command = new UpdateCustomerCommand(customerId, updateRequest);
        var handler = new UpdateCustomerCommandHandler(_mockCustomerRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomerNotFoundException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("CUSTOMER_NOT_FOUND");
        exception.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateEmailOnUpdate_ThrowsDuplicateEmailException()
    {
        // Arrange
        var customerId = 1;
        var existingCustomer = new Customer
        {
            Id = customerId,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true
        };

        var updateRequest = new UpdateCustomerRequest { Email = "existing@example.com" };

        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);

        // New email already exists
        _mockCustomerRepo
            .Setup(x => x.ExistsByEmailAsync("existing@example.com"))
            .ReturnsAsync(true);

        var command = new UpdateCustomerCommand(customerId, updateRequest);
        var handler = new UpdateCustomerCommandHandler(_mockCustomerRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateEmailException>(
            () => handler.HandleAsync(command));

        exception.Code.Should().Be("DUPLICATE_EMAIL");
        exception.StatusCode.Should().Be(409);
    }
}

/// <summary>
/// Integration tests for GetCustomerQueryHandler.
/// </summary>
public class GetCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepo;

    public GetCustomerHandlerTests()
    {
        _mockCustomerRepo = new Mock<ICustomerRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithExistingCustomer_ReturnsCustomerDto()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(customer);

        var query = new GetCustomerQuery(1);
        var handler = new GetCustomerQueryHandler(_mockCustomerRepo.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentCustomer_ThrowsCustomerNotFoundException()
    {
        // Arrange
        _mockCustomerRepo
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Customer?)null);

        var query = new GetCustomerQuery(999);
        var handler = new GetCustomerQueryHandler(_mockCustomerRepo.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomerNotFoundException>(
            () => handler.HandleAsync(query));

        exception.Code.Should().Be("CUSTOMER_NOT_FOUND");
        exception.StatusCode.Should().Be(404);
    }
}

/// <summary>
/// Integration tests for GetCustomersPagedQueryHandler.
/// </summary>
public class GetCustomersPagedHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepo;

    public GetCustomersPagedHandlerTests()
    {
        _mockCustomerRepo = new Mock<ICustomerRepository>();
    }

    [Fact]
    public async Task HandleAsync_WithValidPage_ReturnsPaginatedCustomers()
    {
        // Arrange
        var customers = new[]
        {
            new Customer { Id = 1, Name = "John Doe", Email = "john@example.com", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Customer { Id = 2, Name = "Jane Doe", Email = "jane@example.com", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Customer { Id = 3, Name = "Bob Smith", Email = "bob@example.com", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _mockCustomerRepo
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(customers);

        var query = new GetCustomersPagedQuery(page: 1, pageSize: 20);
        var handler = new GetCustomersPagedQueryHandler(_mockCustomerRepo.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
        result.Items[0].Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task HandleAsync_WithPage2_ReturnSecondPageItems()
    {
        // Arrange
        var customers = Enumerable.Range(1, 25)
            .Select(i => new Customer
            {
                Id = i,
                Name = $"Customer {i}",
                Email = $"customer{i}@example.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            })
            .ToArray();

        _mockCustomerRepo
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(customers);

        var query = new GetCustomersPagedQuery(page: 2, pageSize: 20);
        var handler = new GetCustomersPagedQueryHandler(_mockCustomerRepo.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.PageNumber.Should().Be(2);
        result.TotalPages.Should().Be(2);
        result.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(5);
        result.Items[0].Id.Should().Be(21);
    }
}
