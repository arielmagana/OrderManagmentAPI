using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrderManagement.Api.Errors;

internal static class ApiErrorFactory
{
    private const string ProblemBaseUri = "https://example.com/problems/";

    public static ApiError Create(
        int status,
        string title,
        string detail,
        string code,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        var problemName = code.ToLowerInvariant().Replace('_', '-');
        return new ApiError($"{ProblemBaseUri}{problemName}", title, status, detail, code, errors);
    }

    public static ApiError FromModelState(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The supplied value is invalid."
                        : error.ErrorMessage)
                    .ToArray());

        return Create(
            StatusCodes.Status400BadRequest,
            "Invalid request",
            "The request body is malformed or structurally incomplete.",
            "INVALID_REQUEST",
            errors);
    }
}
