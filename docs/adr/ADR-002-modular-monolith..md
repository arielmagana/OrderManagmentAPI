# ADR-002: Use a Modular Monolith

## Status

Accepted

## Context

The Order Management API has a relatively small functional scope.

The system manages customers, products, and orders and does not currently require independent scaling, independent deployment, or asynchronous communication between independently owned services.

Introducing microservices would add operational complexity without providing sufficient business value for the current requirements.

## Decision

The system will be implemented as a modular monolith.

The application will maintain clear module and layer boundaries while being deployed as a single application.

The initial domain areas are:

- Customers
- Products
- Orders

The architecture should allow individual capabilities to be extracted into separate services in the future if business or technical requirements justify it.

## Consequences

### Positive

- Simpler deployment.
- Lower operational overhead.
- Easier local development.
- Simpler transactions.
- Easier debugging.
- Lower infrastructure cost.
- Clear path toward future decomposition if required.

### Negative

- All modules share the same deployment lifecycle.
- Independent scaling is not possible at module level.
- A future migration to microservices would require additional work.

## Alternatives Considered

### Microservices

Rejected because the current scope does not justify the additional operational and architectural complexity.

### Single-layer application

Rejected because it would provide weaker separation of concerns and would not demonstrate the desired enterprise architecture practices.
