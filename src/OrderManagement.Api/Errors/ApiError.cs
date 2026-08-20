using System.Text.Json.Serialization;

namespace OrderManagement.Api.Errors;

/// <summary>
/// The error representation defined by ADR-007.
/// </summary>
public sealed record ApiError(
    string Type,
    string Title,
    int Status,
    string Detail,
    string Code,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyDictionary<string, string[]>? Errors = null);
