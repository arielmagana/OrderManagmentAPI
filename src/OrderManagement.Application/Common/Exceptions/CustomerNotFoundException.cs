namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a customer cannot be found (ADR-007: 404 Not Found).
/// </summary>
public class CustomerNotFoundException : ApplicationException
{
    public CustomerNotFoundException(int customerId)
        : base(
            "CUSTOMER_NOT_FOUND",
            $"Customer with ID {customerId} does not exist",
            404)
    {
    }

    public CustomerNotFoundException(string message)
        : base(
            "CUSTOMER_NOT_FOUND",
            message,
            404)
    {
    }
}
