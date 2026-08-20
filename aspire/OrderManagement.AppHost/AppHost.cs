var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OrderManagement_Api>("order-management-api");

builder.Build().Run();
