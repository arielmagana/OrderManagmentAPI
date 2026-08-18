# ADR-003: Use Entity Framework Core with SQL Server

## Status

Accepted

## Context

The application requires relational persistence for customers, products, orders, and order items.

The development environment and target deployment environment are based on the Microsoft .NET ecosystem.

The application requires:

- Relational integrity
- Transactions
- Foreign keys
- Decimal precision
- Migrations
- Straightforward local development

## Decision

Entity Framework Core will be used as the application's ORM and SQL Server will be used as the relational database.

Database schema management will use EF Core migrations.

The Infrastructure layer will contain all EF Core-specific implementation details.

## Consequences

### Positive

- Strong integration with .NET.
- Supports migrations.
- Supports transactions and relational constraints.
- Reduces boilerplate data-access code.
- Easy local development with SQL Server containers.
- Natural fit for Azure SQL Database.

### Negative

- Application becomes partially coupled to EF Core through persistence implementation.
- Complex queries may require explicit optimization.
- Developers must understand EF Core tracking and query behavior.

## Alternatives Considered

### Dapper

Provides more direct SQL control but would require more manual mapping and persistence code.

### ADO.NET

Provides maximum control but adds unnecessary implementation complexity for this project.

### PostgreSQL

Technically viable, but SQL Server provides better alignment with the target .NET/Azure demonstration environment.
