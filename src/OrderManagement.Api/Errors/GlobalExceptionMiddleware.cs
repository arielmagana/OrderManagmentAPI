using System.Text.Json;
using AppExceptions = OrderManagement.Application.Common.Exceptions;

namespace OrderManagement.Api.Errors;

public sealed class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var error = Map(exception);

            if (error.Status == StatusCodes.Status500InternalServerError)
            {
                // Do not include exception messages or request bodies: either can contain customer data.
                logger.LogError(
                    "Unhandled {ExceptionType} while processing an HTTP {Method} request; TraceId: {TraceId}",
                    exception.GetType().FullName,
                    context.Request.Method,
                    context.TraceIdentifier);
            }

            context.Response.Clear();
            context.Response.StatusCode = error.Status;
            await context.Response.WriteAsJsonAsync(error);
        }
    }

    private static ApiError Map(Exception exception) => exception switch
    {
        AppExceptions.CustomerNotFoundException or
        AppExceptions.ProductNotFoundException or
        AppExceptions.OrderNotFoundException => FromApplicationException(
            (AppExceptions.ApplicationException)exception,
            StatusCodes.Status404NotFound,
            "Resource not found"),

        AppExceptions.DuplicateEmailException or
        AppExceptions.DuplicateSkuException or
        AppExceptions.InactiveCustomerException or
        AppExceptions.InactiveProductException or
        AppExceptions.InvalidStatusTransitionException => FromApplicationException(
            (AppExceptions.ApplicationException)exception,
            StatusCodes.Status409Conflict,
            "Conflict"),

        AppExceptions.ValidationException validationException => ApiErrorFactory.Create(
            StatusCodes.Status422UnprocessableEntity,
            "Validation failed",
            validationException.Message,
            validationException.Code,
            validationException.Errors),

        BadHttpRequestException or JsonException => ApiErrorFactory.Create(
            StatusCodes.Status400BadRequest,
            "Invalid request",
            "The request body is malformed or could not be parsed.",
            "INVALID_REQUEST"),

        _ => ApiErrorFactory.Create(
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "An unexpected error occurred. Please contact support if the problem persists.",
            "INTERNAL_ERROR")
    };

    private static ApiError FromApplicationException(
        AppExceptions.ApplicationException exception,
        int status,
        string title) => ApiErrorFactory.Create(status, title, exception.Message, exception.Code);
}
