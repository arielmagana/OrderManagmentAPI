namespace OrderManagement.IntegrationTests.Infrastructure;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Persistence;
using Testcontainers.MsSql;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SQL Server";
}

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest").Build();
    private string _connectionString = string.Empty;

    public string ConnectionString => _connectionString;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var builder = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "OrderManagementTestDb"
        };
        _connectionString = builder.ConnectionString;

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public OrderManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
            .UseSqlServer(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;

        return new OrderManagementDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM [OrderItems]; DELETE FROM [Orders]; DELETE FROM [Customers]; DELETE FROM [Products];");
    }
}
