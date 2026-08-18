# ADR-008: Framework and Infrastructure Technology Choices

## Status

Accepted

## Context

The Order Management API requires decisions on several cross-cutting concerns:

- Data validation across layers
- Mapping between Domain, Application, and API DTOs
- Logging and observability
- Configuration management

The solution is a small demonstration project focused on architecture and API design.

Without explicit framework choices, developers would need to make individual decisions and the project could lack consistency.

## Decision

The following frameworks and approaches will be used:

### 1. Validation

**Choice: Data Annotations + FluentValidation**

**Approach:**
- Use .NET Data Annotations for basic validation (Required, Range, StringLength, etc.) on DTOs
- Use FluentValidation for complex validation rules in the Application layer
- Validation happens in the Application layer before persistence
- The Domain layer remains validation-framework-agnostic

**Why this choice:**
- Data Annotations are built into .NET and familiar to enterprise teams
- FluentValidation provides powerful, readable validation rules for complex business logic
- Clear separation: DTOs for simple constraints, Application validators for business rules
- Pragmatic approach for demonstration project scope
- FluentValidation adds one small external dependency, but the tradeoff is acceptable because it keeps validation rules expressive and testable.

### 2. DTO Mapping

**Choice: Manual Mapping in Application Layer**

**Approach:**
- Create explicit mapping methods in the Application layer (e.g., `ToApplicationDto()`, `ToDomainEntity()`)
- Map between: Domain entities ↔ Application DTOs ↔ API request/response models
- Each mapping method is explicit and testable
- Single responsibility: mappings are not hidden in a separate configuration layer

**Why this choice:**
- Explicit mappings demonstrate clean architecture principles
- No black-box abstraction hiding transformation logic
- Mappings are clear, debuggable, and easy to understand
- Small project scope does not justify AutoMapper overhead
- Manual mapping is educational and shows exactly what transformations occur
- Zero external dependency
- Mappings are co-located with use cases for clarity

### 3. Logging

**Choice: Microsoft.Extensions.Logging**

**Approach:**
- Use the built-in .NET logging framework (no external dependency)
- Log at appropriate levels: Debug, Information, Warning, Error
- Application layer logs significant operations and business events
- Infrastructure layer logs database operations and external service calls
- API layer logs incoming requests (via middleware)
- Sensitive data (customer emails, passwords) must not be logged

**Why this choice:**
- Built into ASP.NET Core and .NET runtime
- No external dependency required
- Sufficient for demonstration project scope
- Integrates with .NET Aspire for local development
- Logs are structured and can be consumed by various providers (console, file, cloud)
- Future production scenarios can add Serilog or other providers without changing application code

### 4. Configuration

**Choice: Microsoft.Extensions.Configuration**

**Approach:**
- Configuration sources in order of precedence:
  1. Command-line arguments (highest priority)
  2. Environment variables
  3. `appsettings.{Environment}.json` (environment-specific settings)
  4. `appsettings.json` (shared settings)
- Connection strings and secrets use environment-specific configuration
- Local development uses .NET User Secrets for sensitive data
- Cloud deployments use Azure App Service configuration
- Configuration keys use the `:` separator (e.g., `ConnectionStrings:DefaultConnection`)

**Why this choice:**
- Built into ASP.NET Core
- Industry standard for .NET applications
- Supports multiple configuration sources
- Separates configuration from code
- Supports secrets management via User Secrets (local) and Azure Key Vault (cloud)
- No external dependency

### 5. Health Checks

**Choice: .NET Health Checks + .NET Aspire Built-in Checks**

**Approach:**
- Use `AddHealthChecks()` middleware for the `/api/health` endpoint
- Configure checks for critical dependencies: database connectivity
- .NET Aspire provides built-in health checks for SQL Server
- Health checks are synchronous and return quickly
- Response indicates overall system health

**Why this choice:**
- Built into ASP.NET Core
- Standard pattern for cloud deployments
- Supports readiness that includes SQL Server
- Aspire integration simplifies local development
- No external dependency

## Consequences

### Positive

- All frameworks are built into .NET/ASP.NET Core (no external dependencies)
- Consistent with enterprise .NET development practices
- Clear separation of concerns between Domain, Application, and API layers
- Explicit, maintainable code without magic or hidden abstractions
- All choices align with clean architecture principles
- Educational value for demonstrating architecture and design patterns
- Easy to test and debug
- Future migration to alternative frameworks is straightforward

### Negative

- Manual DTO mapping requires more code than AutoMapper
- Developers must discipline themselves to validate in Application layer, not in API layer
- No sophisticated validation framework for very complex rules
- Simple logging may require enhancement for production tracing scenarios
- Configuration management requires understanding of multiple configuration sources

## Alternatives Considered

### DTO Mapping Alternatives
- **AutoMapper**: Provides powerful mapping conventions but adds abstraction overhead for demonstration project
- **Mapster**: Similar to AutoMapper with good performance, but still unnecessary complexity for project scope

### Logging Alternatives
- **Serilog**: Excellent structured logging but adds external dependency for demonstration project
- **NLog**: Feature-rich but overhead not justified for current scope

### Validation Alternatives
- **Purely Data Annotations**: Insufficient for complex business rules
- **Custom validation attributes**: Would require more custom code than FluentValidation

## Notes

These choices are pragmatic for the demonstration project scope while maintaining enterprise-grade architecture.

All choices can be enhanced or replaced in future production scenarios:
- Manual mapping → Introduce AutoMapper/Mapster if code becomes too verbose
- Microsoft.Extensions.Logging → Add Serilog for structured logging in production
- Custom validation → Extend with domain-driven validation if rules become more complex

The architecture remains agnostic to these framework choices—they are implementation details in the Application and Infrastructure layers.
