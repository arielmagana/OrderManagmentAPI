using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using OrderManagement.Api.Errors;
using OrderManagement.Api.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddTransient<GlobalExceptionMiddleware>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Order Management API",
            Version = "v1",
            Description = "HTTP API for managing customers, products, and orders. Errors follow ADR-007."
        };

        return Task.CompletedTask;
    });
});
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var error = ApiErrorFactory.FromModelState(context.ModelState);
            return new BadRequestObjectResult(error);
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitializeOrderManagementDatabaseAsync();
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Order Management API v1"));
}

// Keep exception handling ahead of all request handlers so every API failure has one contract.
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllers();
app.MapDefaultEndpoints();
app.MapGet("/api/health", async (HttpContext context, HealthCheckService healthChecks) =>
{
    var report = await healthChecks.CheckHealthAsync(context.RequestAborted);
    context.Response.StatusCode = report.Status == HealthStatus.Unhealthy
        ? StatusCodes.Status503ServiceUnavailable
        : StatusCodes.Status200OK;
    await HealthCheckResponseWriter.WriteAsync(context, report);
})
    .WithName("GetApiHealth")
    .WithTags("Health")
    .WithSummary("Get API health")
    .WithDescription("Reports API and database connectivity health.")
    .WithMetadata(
        new ProducesResponseTypeAttribute(
            typeof(HealthResponse), StatusCodes.Status200OK, "application/json"),
        new ProducesResponseTypeAttribute(
            typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable, "application/json"));
app.MapGet("/", () => "Hello World!");

app.Run();
