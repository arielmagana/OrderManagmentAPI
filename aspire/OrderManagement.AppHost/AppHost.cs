var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume();

var database = sql.AddDatabase("OrderManagement");

builder.AddProject<Projects.OrderManagement_Api>("order-management-api")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health")
    .WithUrlForEndpoint("http", _ => new()
    {
        Url = "/scalar/v1",
        DisplayText = "Scalar API reference"
    })
    .WithUrlForEndpoint("https", _ => new()
    {
        Url = "/scalar/v1",
        DisplayText = "Scalar API reference"
    })
    .WithUrlForEndpoint("http", _ => new()
    {
        Url = "/openapi/v1.json",
        DisplayText = "OpenAPI document"
    })
    .WithUrlForEndpoint("https", _ => new()
    {
        Url = "/openapi/v1.json",
        DisplayText = "OpenAPI document"
    })
    .WithUrlForEndpoint("http", _ => new()
    {
        Url = "/api/health",
        DisplayText = "API health"
    })
    .WithUrlForEndpoint("https", _ => new()
    {
        Url = "/api/health",
        DisplayText = "API health"
    });

builder.Build().Run();
