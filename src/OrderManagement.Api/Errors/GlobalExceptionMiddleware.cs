using System.Text.Json;
using ApplicationException = OrderManagement.Application.Common.Exceptions.ApplicationException;
using ValidationException = OrderManagement.Application.Common.Exceptions.ValidationException;
using OrderManagement.Application.Common.Exceptions;

namespace OrderManagement.Api.Errors;

public sealed class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var error = Map(exception);

            if (error.Status == StatusCodes.Status500InternalServerError)
            {
                // Do not log exception messages or request bodies: either can contain customer data.
                logger.LogError(
                    "Unhandled {ExceptionType} while processing an HTTP {Method} request. Trace identifier: {TraceIdentifier}",
                    exception.GetType().FullName,
                    context.Request.Method,
                    context.TraceIdentifier);
            }

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = error.Status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(error, cancellationToken: context.RequestAborted);
        }
    }

    private static ApiError Map(Exception exception) => exception switch
    {
        ValidationException validation => ApiErrorFactory.Create(
            StatusCodes.Status422UnprocessableEntity,
            "Validation failed",
            validation.Message,
            validation.Code,
            validation.Errors),

        CustomerNotFoundException or ProductNotFoundException or OrderNotFoundException
            => FromApplicationException((ApplicationException)exception, StatusCodes.Status404NotFound, "Resource not found"),

        DuplicateEmailException or DuplicateSkuException or InactiveCustomerException
            or InactiveProductException or InvalidStatusTransitionException
            => FromApplicationException((ApplicationException)exception, StatusCodes.Status409Conflict, "Conflict"),

        JsonException or BadHttpRequestException => ApiErrorFactory.Create(
            StatusCodes.Status400BadRequest,
            "Bad request",
            "The request body is malformed or could not be read.",
            "INVALID_REQUEST"),

        _ => ApiErrorFactory.Create(
            StatusCodes.Status500InternalServerError,
            "Internal server error",
            "An unexpected error occurred.",
            "INTERNAL_ERROR")
    };

    private static ApiError FromApplicationException(ApplicationException exception, int status, string title) =>
        ApiErrorFactory.Create(status, title, exception.Message, exception.Code, exception.Errors);
}
