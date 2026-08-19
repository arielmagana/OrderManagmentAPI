using System.Text.Json.Serialization;

namespace OrderManagement.Api.Errors;

/// <summary>
/// The error representation defined by ADR-007.
/// </summary>
public sealed record ApiError(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("detail")] string Detail,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("errors"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyDictionary<string, string[]>? Errors = null);
