namespace OrderManagement.Application.Customers.Queries;

/// <summary>
/// Query to retrieve a single customer by ID.
/// CQRS-style query object passed to handler.
/// </summary>
public class GetCustomerQuery
{
    public int CustomerId { get; }

    public GetCustomerQuery(int customerId)
    {
        CustomerId = customerId;
    }
}
