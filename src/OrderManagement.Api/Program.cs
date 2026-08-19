using OrderManagement.Api.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<GlobalExceptionMiddleware>();
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var error = ApiErrorFactory.FromModelState(context.ModelState);
            return new Microsoft.AspNetCore.Mvc.ObjectResult(error)
            {
                StatusCode = error.Status,
                ContentTypes = { "application/problem+json" }
            };
        };
    });

var app = builder.Build();

// Keep exception translation ahead of all request handlers.
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapGet("/", () => "Hello World!");
app.MapControllers();

app.Run();
