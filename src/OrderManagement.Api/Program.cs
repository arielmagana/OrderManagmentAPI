using OrderManagement.Api.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddTransient<GlobalExceptionMiddleware>();
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var error = ApiErrorFactory.FromModelState(context.ModelState);
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(error);
        };
    });

var app = builder.Build();

// Keep exception handling ahead of all request handlers so every API failure has one contract.
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();
