namespace OrderManagement.Application.Common.Validation;

using System.ComponentModel.DataAnnotations;
using Exceptions;
using FluentValidation;

/// <summary>
/// Applies the validation approaches required by ADR-008 at the application boundary.
/// </summary>
public static class RequestValidator
{
    public static async Task ValidateAsync<T>(IValidator<T> validator, T request)
    {
        var validationResult = await validator.ValidateAsync(request);
        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => ToCamelCase(group.Key),
                group => group.Select(error => error.ErrorMessage).ToArray());

        var annotationResults = new List<ValidationResult>();
        Validator.TryValidateObject(request!, new ValidationContext(request!), annotationResults, validateAllProperties: true);

        foreach (var annotationResult in annotationResults)
        {
            var members = annotationResult.MemberNames.DefaultIfEmpty(string.Empty);
            foreach (var member in members)
            {
                var field = ToCamelCase(member);
                errors[field] = errors.TryGetValue(field, out var existing)
                    ? [.. existing, annotationResult.ErrorMessage ?? "Invalid value"]
                    : [annotationResult.ErrorMessage ?? "Invalid value"];
            }
        }

        if (errors.Count > 0)
        {
            throw Exceptions.ValidationException.FromFluentValidationErrors(errors);
        }
    }

    public static void ValidatePagination(int page, int pageSize)
    {
        var errors = new Dictionary<string, string[]>();
        if (page < 1)
            errors["page"] = ["Page must be greater than or equal to 1"];
        if (pageSize < 1)
            errors["pageSize"] = ["Page size must be greater than or equal to 1"];

        if (errors.Count > 0)
            throw Exceptions.ValidationException.FromFluentValidationErrors(errors);
    }

    private static string ToCamelCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToLowerInvariant(value[0]) + value[1..];
}
