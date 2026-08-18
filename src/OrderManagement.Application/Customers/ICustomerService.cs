namespace OrderManagement.Application.Customers;

using DTOs;
using Commands;
using Queries;
using Validators;
using Common.Validation;
using FluentValidation;

/// <summary>
/// Service interface for customer operations.
/// Abstracts handler classes for dependency injection in API layer.
/// Per ADR-005, controllers depend on this interface, not directly on handlers.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Creates a new customer.
    /// </summary>
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);

    /// <summary>
    /// Retrieves a customer by ID.
    /// </summary>
    Task<CustomerDto> GetCustomerAsync(int customerId);

    /// <summary>
    /// Retrieves a paginated list of customers.
    /// </summary>
    Task<Common.Pagination.PaginatedResponse<CustomerDto>> GetCustomersPagedAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    Task<CustomerDto> UpdateCustomerAsync(int customerId, UpdateCustomerRequest request);
}

/// <summary>
/// Concrete implementation of ICustomerService.
/// Orchestrates command/query handlers.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly CreateCustomerCommandHandler _createCommandHandler;
    private readonly UpdateCustomerCommandHandler _updateCommandHandler;
    private readonly GetCustomerQueryHandler _getQueryHandler;
    private readonly GetCustomersPagedQueryHandler _getPagedQueryHandler;
    private readonly IValidator<CreateCustomerRequest> _createValidator;
    private readonly IValidator<UpdateCustomerRequest> _updateValidator;

    public CustomerService(
        CreateCustomerCommandHandler createCommandHandler,
        UpdateCustomerCommandHandler updateCommandHandler,
        GetCustomerQueryHandler getQueryHandler,
        GetCustomersPagedQueryHandler getPagedQueryHandler,
        IValidator<CreateCustomerRequest> createValidator,
        IValidator<UpdateCustomerRequest> updateValidator)
    {
        _createCommandHandler = createCommandHandler;
        _updateCommandHandler = updateCommandHandler;
        _getQueryHandler = getQueryHandler;
        _getPagedQueryHandler = getPagedQueryHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        await RequestValidator.ValidateAsync(_createValidator, request);
        var command = new CreateCustomerCommand(request);
        return await _createCommandHandler.HandleAsync(command);
    }

    public async Task<CustomerDto> GetCustomerAsync(int customerId)
    {
        var query = new GetCustomerQuery(customerId);
        return await _getQueryHandler.HandleAsync(query);
    }

    public async Task<Common.Pagination.PaginatedResponse<CustomerDto>> GetCustomersPagedAsync(int page = 1, int pageSize = 20)
    {
        RequestValidator.ValidatePagination(page, pageSize);
        var query = new GetCustomersPagedQuery(page, pageSize);
        return await _getPagedQueryHandler.HandleAsync(query);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(int customerId, UpdateCustomerRequest request)
    {
        await RequestValidator.ValidateAsync(_updateValidator, request);
        var command = new UpdateCustomerCommand(customerId, request);
        return await _updateCommandHandler.HandleAsync(command);
    }
}
