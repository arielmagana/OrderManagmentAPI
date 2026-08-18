namespace OrderManagement.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>Enables EF tooling to create migrations without starting the API host.</summary>
public class OrderManagementDbContextFactory : IDesignTimeDbContextFactory<OrderManagementDbContext>
{
    public OrderManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__OrderManagement")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=OrderManagement;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new OrderManagementDbContext(options);
    }
}
