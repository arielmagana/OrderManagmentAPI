namespace OrderManagement.Application.Customers.Commands;

using DTOs;

/// <summary>
/// Command to update an existing customer.
/// CQRS-style command object passed to handler.
/// </summary>
public class UpdateCustomerCommand
{
    public int CustomerId { get; }
    public UpdateCustomerRequest Request { get; }

    public UpdateCustomerCommand(int customerId, UpdateCustomerRequest request)
    {
        CustomerId = customerId;
        Request = request;
    }
}
