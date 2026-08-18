# ADR-004: Use Azure App Service and Azure SQL Database

## Status

Accepted

## Context

The project requires a cloud deployment that demonstrates the ability to deploy a .NET backend to Azure.

The deployment should remain simple, inexpensive, and appropriate for a small demonstration application.

The project does not require Kubernetes, complex networking, or multiple distributed services.

## Decision

The API will be deployed to Azure App Service.

The relational database will be deployed to Azure SQL Database.

GitHub Actions will be used for CI/CD.

## Consequences

### Positive

- Native support for ASP.NET Core.
- Simple deployment model.
- Low operational overhead.
- Straightforward CI/CD integration.
- Azure SQL is compatible with the application's SQL Server data model.
- The architecture can evolve toward more advanced Azure services if required.

### Negative

- Less infrastructure control than container orchestration platforms.
- App Service is less suitable than container/Kubernetes-based hosting for some advanced workloads.
- Database cost and availability depend on the selected Azure tier.

## Alternatives Considered

### Azure Container Apps

Potentially useful for containerized workloads, but unnecessary for the initial demonstration.

### Azure Kubernetes Service

Rejected because the operational complexity is not justified.

### Azure Virtual Machines

Rejected because App Service provides a simpler managed hosting model.

### AWS

Technically viable, but Azure aligns better with the project's .NET and enterprise integration focus.
