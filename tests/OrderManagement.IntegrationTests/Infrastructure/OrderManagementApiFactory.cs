extern alias ApiAssembly;

namespace OrderManagement.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed class OrderManagementApiFactory(SqlServerFixture database)
    : WebApplicationFactory<ApiAssembly::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // Host settings are applied after appsettings and environment providers, ensuring
        // every production database registration (including its health check) uses the fixture.
        builder.UseSetting("ConnectionStrings:OrderManagement", database.ConnectionString);
    }

    public Task ResetDatabaseAsync() => database.ResetDatabaseAsync();
}
