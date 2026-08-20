using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Errors;
using OrderManagement.Api.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddTransient<GlobalExceptionMiddleware>();
builder.Services.AddEndpointsApiExplorer();
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

// Keep exception handling ahead of all request handlers so every API failure has one contract.
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllers();
app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
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
