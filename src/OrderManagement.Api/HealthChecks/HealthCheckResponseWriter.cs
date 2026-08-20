using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderManagement.Api.HealthChecks;

internal static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new HealthResponse(
            FormatStatus(report.Status),
            report.Entries
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => new HealthCheckResultResponse(
                    entry.Key,
                    FormatStatus(entry.Value.Status)))
                .ToArray());

        return context.Response.WriteAsJsonAsync(response, SerializerOptions);
    }

    private static string FormatStatus(HealthStatus status) =>
        status.ToString().ToLowerInvariant();
}

internal sealed record HealthResponse(
    string Status,
    IReadOnlyList<HealthCheckResultResponse> Checks);

internal sealed record HealthCheckResultResponse(string Name, string Status);
