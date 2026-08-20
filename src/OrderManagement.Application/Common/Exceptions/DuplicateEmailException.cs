namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when attempting to create/update a customer with a duplicate email (ADR-007: 409 Conflict).
/// </summary>
public class DuplicateEmailException : ApplicationException
{
    public DuplicateEmailException(string email)
        : base(
            "DUPLICATE_EMAIL",
            $"Email address '{email}' already exists",
            409)
    {
    }
}
