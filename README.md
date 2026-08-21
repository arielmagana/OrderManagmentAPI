# Order Management API

## Overview

An ASP.NET Core order-management API built with Clean Architecture, EF Core, SQL Server, and .NET Aspire.

## Architecture
## Technologies
## Project structure

## Running locally

### Prerequisites

- .NET 10 SDK
- Docker Desktop or another Docker-compatible container runtime
- Trusted ASP.NET Core development certificate for the HTTPS profile (`dotnet dev-certs https --trust`)

Start the complete development stack with:

```bash
dotnet run --project aspire/OrderManagement.AppHost
```

Aspire starts a persistent SQL Server container, supplies the generated connection string to the API, and waits for SQL Server before starting the API. In Development, the API applies pending EF Core migrations automatically. Use the endpoint links shown in the Aspire dashboard; service ports are assigned dynamically.

To run only the API against Windows LocalDB:

```bash
dotnet run --project src/OrderManagement.Api
```

The direct API workflow uses the fallback connection string in `appsettings.Development.json` and also applies pending migrations in Development.

The Aspire SQL data volume survives normal AppHost restarts. To intentionally reset it, stop AppHost, identify the volume attached to the `sql` resource with `docker volume ls`, and remove that specific volume with `docker volume rm <volume-name>`. The next AppHost start creates and migrates a fresh database.

## Running tests

```bash
dotnet test OrderManagement.sln
```

Integration and Aspire orchestration tests require Docker. If they report that the Docker endpoint is unavailable, start the container runtime and rerun the command.

## API documentation

In Development, use the Aspire dashboard links for:

- Scalar interactive reference: `/scalar/v1`
- OpenAPI JSON: `/openapi/v1.json`
- Detailed API health: `/api/health`
- Readiness and liveness: `/health` and `/alive`

## CI/CD
## Azure deployment
## Architecture decisions
## Future improvements
* Authentication
* Authorization
* Inventory
* Payments
* Asynchronous messaging
* Distributed architecture
