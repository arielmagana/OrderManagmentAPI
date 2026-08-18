# ADR-001: Use Clean Architecture

## Status

Accepted

## Context

The Order Management API is intended to demonstrate enterprise backend development practices.

The solution contains business rules, application use cases, persistence, and HTTP/API concerns.

Without clear separation between these concerns, business logic could become tightly coupled to ASP.NET Core or Entity Framework Core, making the application harder to test and maintain.

## Decision

The solution will follow Clean Architecture principles.

The solution will be divided into:

- Domain
- Application
- Infrastructure
- API

Dependencies will point toward the Domain layer.

The Domain layer will not depend on infrastructure or web frameworks.

## Consequences

### Positive

- Business rules remain isolated from infrastructure.
- Unit testing becomes easier.
- Infrastructure can be replaced with limited impact.
- API concerns remain separate from application logic.
- The architecture is familiar to enterprise .NET development teams.

### Negative

- More projects and files than a simple single-project API.
- Additional abstractions may increase development effort.
- Developers must understand and respect dependency boundaries.

## Notes

Clean Architecture will be applied pragmatically.

The project will avoid introducing abstractions that do not provide meaningful value for the current scope.
