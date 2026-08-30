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

### Frontend

The client in `src/OrderManagement.Web` is a Vite, React, and TypeScript SPA. Copy
`.env.example` to `.env.local` and set `VITE_API_BASE_URL` to the independently
hosted API origin (without a trailing slash), then run:

```bash
cd src/OrderManagement.Web
npm install
npm run dev
```

Run `npm test` for component tests and `npm run build` for the production check.
The static production output is `src/OrderManagement.Web/dist/`.

To deploy on Azure Static Web Apps Free, use `src/OrderManagement.Web` as the app
location, `npm run build` as the build command, and `dist` as the output location.
Configure `VITE_API_BASE_URL` as a build environment variable and allow the Static
Web Apps origin in the App Service API's CORS settings. The included
`staticwebapp.config.json` sends client routes to `index.html` while excluding
assets. It intentionally defines no managed API route because the ASP.NET API is
deployed separately to Azure App Service.

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

GitHub Actions validates every pull request targeting `main`, every push to
`main`, and optional manual runs. The validation workflow restores and builds
the solution in Release mode with warnings treated as errors, runs the unit
suite, runs the SQL Server Testcontainers and Aspire orchestration tests, and
publishes the API.

The workflow stores TRX test results, Cobertura coverage reports, and the
published API in the run's **Artifacts** section. Coverage is provided for
inspection and is not used as a percentage gate. The GitHub-hosted runner must
provide Docker; unavailable containers fail validation rather than skipping
integration tests.

## Azure deployment

`Deploy validated release` promotes an immutable artifact from a successful
`main` CI run. It verifies release checksums, authenticates to Azure through
GitHub OIDC, applies the matching EF Core migration bundle, deploys to App
Service, and verifies the health endpoints.

Azure resources, OIDC federation, database networking, and GitHub Environment
values must be configured before the workflow can run. See
[Deployment and Database Operations](docs/deployment.md) for prerequisites,
release steps, and recovery procedures.

## Architecture decisions

## Future improvements

- Authentication
- Authorization
- Inventory
- Payments
- Asynchronous messaging
- Distributed architecture
