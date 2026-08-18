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

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public async Task<OrderManagementDbContext> CreateContextAsync()
    {
        var baseConnectionString = _container.GetConnectionString();

        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = "OrderManagementTestDb"
        };

        var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
            .UseSqlServer(builder.ConnectionString)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new OrderManagementDbContext(options);

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        return context;
    }
}
