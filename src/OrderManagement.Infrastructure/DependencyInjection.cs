namespace Microsoft.Extensions.DependencyInjection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Persistence.Repositories;

public static class InfrastructureDependencyInjection
{
    private const string ConnectionStringName = "OrderManagement";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured.");
        }

        services.AddDbContext<OrderManagementDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddHealthChecks()
            .AddDbContextCheck<OrderManagementDbContext>("database");

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }

    public static async Task InitializeOrderManagementDatabaseAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
