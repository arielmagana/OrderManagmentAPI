namespace OrderManagement.Application.Customers.Commands;

using DTOs;

/// <summary>
/// Command to create a new customer.
/// CQRS-style command object passed to handler.
/// </summary>
public class CreateCustomerCommand
{
    public CreateCustomerRequest Request { get; }

    public CreateCustomerCommand(CreateCustomerRequest request)
    {
        Request = request;
    }
}
