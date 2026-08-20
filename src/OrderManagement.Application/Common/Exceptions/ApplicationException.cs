namespace OrderManagement.Application.Common.Exceptions;

/// <summary>
/// Base exception for all application-level errors.
/// Inherits from System.Exception and provides standard error handling.
/// </summary>
public class ApplicationException : Exception
{
    /// <summary>
    /// The machine-readable error code for programmatic handling.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The HTTP status code associated with this error.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Optional field-level validation errors.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationException"/>.
    /// </summary>
    public ApplicationException(
        string code,
        string message,
        int statusCode = 500,
        Dictionary<string, string[]>? errors = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
        Errors = errors;
    }
}
