namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when an invalid order status transition is attempted (ADR-007: 409 Conflict).
/// Per ADR-006, not all status transitions are allowed.
/// </summary>
public class InvalidStatusTransitionException : ApplicationException
{
    public InvalidStatusTransitionException(string currentStatus, string requestedStatus)
        : base(
            "INVALID_STATUS_TRANSITION",
            $"Cannot transition order status from '{currentStatus}' to '{requestedStatus}'",
            409)
    {
    }
}
