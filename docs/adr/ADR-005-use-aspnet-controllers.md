# ADR-005: Use ASP.NET Core Controllers

## Status

Accepted

## Context

The Order Management API requires an HTTP interface for exposing customer, product, and order operations.

ASP.NET Core provides two suitable approaches for implementing HTTP APIs:

- Controllers
- Minimal APIs

Both approaches are technically capable of implementing the required functionality.

The project is intended to demonstrate enterprise-oriented .NET backend development practices and should use an approach that provides clear separation between HTTP concerns and application logic.

## Decision

The API will use ASP.NET Core Controllers.

Controllers will remain intentionally thin and will be responsible primarily for:

- HTTP request handling
- Model binding
- Request validation orchestration
- Calling application use cases
- Mapping application results to HTTP responses
- Returning appropriate HTTP status codes

Business rules and application logic must not be implemented directly inside controllers.

## Rationale

Controllers were selected because they:

- Are widely used in enterprise .NET applications.
- Provide a clear structure for resource-oriented APIs.
- Make HTTP concerns explicit.
- Provide a familiar structure for clients and development teams.
- Align well with the project's goal of demonstrating maintainable enterprise backend development.

Minimal APIs remain a valid alternative for smaller or simpler APIs but are not required for this demonstration.

## Consequences

### Positive

- Clear separation of HTTP and application concerns.
- Familiar structure for enterprise .NET teams.
- Easy organization by resource.
- Straightforward API testing.
- Explicit controller-level API contracts.

### Negative

- More ceremony than Minimal APIs.
- Additional files and classes.
- Some endpoints may require more boilerplate.

## Alternatives Considered

### Minimal APIs

Rejected for this project because the primary objective is to demonstrate an enterprise-oriented API structure rather than minimizing endpoint boilerplate.

Minimal APIs would be reconsidered for smaller services or scenarios where their reduced ceremony provides meaningful value.
