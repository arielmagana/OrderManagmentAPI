namespace OrderManagement.IntegrationTests.Aspire;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using global::Aspire.Hosting.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

public sealed class AspireOrchestrationTests
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(3);

    [Fact]
    public async Task AppHost_model_contains_persistent_sql_database_and_dependent_api()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.OrderManagement_AppHost>();

        var sql = builder.Resources.Single(resource => resource.Name == "sql");
        var database = builder.Resources.Single(resource => resource.Name == "OrderManagement");
        var api = builder.Resources.Single(resource => resource.Name == "order-management-api");

        sql.Should().BeOfType<SqlServerServerResource>();
        database.Should().BeOfType<SqlServerDatabaseResource>();
        api.Should().BeOfType<ProjectResource>();
        sql.Annotations.Should().Contain(annotation =>
            annotation.GetType().Name == "ContainerMountAnnotation");
        api.Annotations.Should().Contain(annotation =>
            annotation.GetType().Name == "ResourceRelationshipAnnotation");
        api.Annotations.Should().Contain(annotation =>
            annotation.GetType().Name == "EnvironmentCallbackAnnotation");
        api.Annotations.Should().Contain(annotation =>
            annotation.GetType().Name == "HealthCheckAnnotation");
        api.Annotations.Count(annotation =>
                annotation.GetType().Name == "ResourceUrlsCallbackAnnotation")
            .Should().Be(6);
    }

    [Fact]
    public async Task Aspire_stack_is_healthy_documented_and_uses_migrated_database()
    {
        var email = $"aspire-{Guid.NewGuid():N}@example.com";
        int id;

        await using (var firstRun = await StartAppHostAsync())
        {
            using var client = firstRun.CreateHttpClient("order-management-api");
            using var timeout = new CancellationTokenSource(StartupTimeout);

            (await client.GetAsync("/health", timeout.Token)).StatusCode.Should().Be(HttpStatusCode.OK);
            (await client.GetAsync("/alive", timeout.Token)).StatusCode.Should().Be(HttpStatusCode.OK);
            (await client.GetAsync("/openapi/v1.json", timeout.Token)).StatusCode.Should().Be(HttpStatusCode.OK);
            (await client.GetAsync("/scalar/v1", timeout.Token)).StatusCode.Should().Be(HttpStatusCode.OK);

            var health = await client.GetFromJsonAsync<JsonElement>("/api/health", timeout.Token);
            health.GetProperty("status").GetString().Should().Be("healthy");
            health.GetProperty("checks").EnumerateArray().Should().Contain(check =>
                check.GetProperty("name").GetString() == "self"
                && check.GetProperty("status").GetString() == "healthy");
            health.GetProperty("checks").EnumerateArray().Should().Contain(check =>
                check.GetProperty("name").GetString() == "database"
                && check.GetProperty("status").GetString() == "healthy");

            var createResponse = await client.PostAsJsonAsync(
                "/api/customers",
                new { name = "Aspire Smoke Test", email },
                timeout.Token);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(timeout.Token);
            id = created.GetProperty("id").GetInt32();
        }

        await using (var secondRun = await StartAppHostAsync())
        {
            using var client = secondRun.CreateHttpClient("order-management-api");
            using var timeout = new CancellationTokenSource(StartupTimeout);
            var readResponse = await client.GetAsync($"/api/customers/{id}", timeout.Token);
            readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var read = await readResponse.Content.ReadFromJsonAsync<JsonElement>(timeout.Token);
            read.GetProperty("email").GetString().Should().Be(email);
        }
    }

    private static async Task<DistributedApplication> StartAppHostAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.OrderManagement_AppHost>();
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
            clientBuilder.AddStandardResilienceHandler());

        var app = await builder.BuildAsync();
        using var timeout = new CancellationTokenSource(StartupTimeout);
        await app.StartAsync(timeout.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync(
            "order-management-api", timeout.Token);
        return app;
    }
}
