namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when attempting to use an inactive customer (ADR-007: 409 Conflict).
/// </summary>
public class InactiveCustomerException : ApplicationException
{
    public InactiveCustomerException(int customerId)
        : base(
            "CUSTOMER_INACTIVE",
            $"Cannot create order for inactive customer (ID: {customerId})",
            409)
    {
    }
}
