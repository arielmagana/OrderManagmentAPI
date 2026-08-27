# Database Migration Manifest

This manifest is updated in the same pull request as every Entity Framework Core migration. The API artifact and migration bundle are built from the same validated commit.

## Migration policy

- Production migrations run from the deployment workflow, never from API startup.
- Development startup may apply migrations for the Aspire-managed local database.
- Prefer expand/contract changes: add compatible schema first, backfill separately, and remove old schema only after all deployed application versions stop using it.
- Keep schema changes bounded and review table scans, locks, index creation, and data movement before production approval.
- Do not automatically execute `Down` migrations in production. Roll back the application while compatible additive schema remains in place; use Azure SQL point-in-time restore for an approved database recovery.
- Test and demo records are not schema migrations. Production reference data, if introduced, must use stable business keys and a reviewed, idempotent data migration.

## Current migrations

| Migration ID | Purpose | Compatibility | Rollback notes |
| --- | --- | --- | --- |
| `20260818182030_InitialPersistence` | Creates Customers, Products, Orders, and OrderItems with their indexes, relationships, constraints, and status conversion. | Initial schema; there is no prior application version to preserve. | For an empty demonstration environment, recreate the database. For any environment containing business data, restore through the documented Azure SQL recovery procedure instead of running `Down`. |

## Adding a migration

```bash
dotnet tool restore
dotnet ef migrations add MigrationName --project src/OrderManagement.Infrastructure --startup-project src/OrderManagement.Api
dotnet test tests/OrderManagement.IntegrationTests/OrderManagement.IntegrationTests.csproj --configuration Release
```

Before merging, add the new migration to the table above and document compatibility, any data backfill, expected locking, and recovery implications.
