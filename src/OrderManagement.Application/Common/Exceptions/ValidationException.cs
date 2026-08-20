namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when validation of a request fails (ADR-007: 400/422 errors).
/// </summary>
public class ValidationException : ApplicationException
{
    public ValidationException(
        string code,
        string message,
        int statusCode,
        Dictionary<string, string[]>? errors = null)
        : base(code, message, statusCode, errors)
    {
    }

    /// <summary>
    /// Creates a validation exception for missing required fields (400 Bad Request).
    /// </summary>
    public static ValidationException MissingRequiredField(string fieldName)
    {
        var errors = new Dictionary<string, string[]>
        {
            { fieldName, new[] { $"{fieldName} is required" } }
        };
        return new ValidationException(
            "MISSING_REQUIRED_FIELD",
            "The request contains validation errors",
            400,
            errors);
    }

    /// <summary>
    /// Creates a validation exception for invalid field values (422 Unprocessable Entity).
    /// </summary>
    public static ValidationException InvalidFieldValue(string fieldName, string errorMessage)
    {
        var errors = new Dictionary<string, string[]>
        {
            { fieldName, new[] { errorMessage } }
        };
        return new ValidationException(
            "INVALID_VALUE",
            "The request contains validation errors",
            422,
            errors);
    }

    /// <summary>
    /// Creates a validation exception with multiple field errors.
    /// </summary>
    public static ValidationException FromFluentValidationErrors(
        Dictionary<string, string[]> fieldErrors)
    {
        return new ValidationException(
            "VALIDATION_FAILED",
            "The request contains validation errors",
            422,
            fieldErrors);
    }
}
