# Deployment and Database Operations

## 1. Deployment model

The Order Management API targets Azure App Service and Azure SQL Database. Local development remains orchestrated by .NET Aspire.

Production releases promote one immutable artifact created by the `Continuous Integration` workflow. That artifact contains the published API, a self-contained Linux x64 EF Core migration bundle, SHA-256 checksums, the source commit, and the expected migration range. The deployment workflow never rebuilds it.

```text
CI on main -> API + migration release artifact -> environment approval
           -> migrate Azure SQL -> deploy App Service -> verify health
```

Database migration is deliberately separate from API startup. `Development` startup applies pending migrations for Aspire convenience; Staging and Production do not.

## 2. Local development

Prerequisites are .NET 10, Docker Desktop (or another compatible container runtime), Git, and trusted development HTTPS certificates.

```bash
dotnet run --project aspire/OrderManagement.AppHost
```

Aspire provisions persistent SQL Server, injects `ConnectionStrings__OrderManagement`, applies migrations during Development startup, and exposes API, documentation, telemetry, and health links through the dashboard.

## 3. Migration lifecycle

The authoritative inventory and compatibility policy are in [migrations.md](./migrations.md).

Create and test a migration with:

```bash
dotnet tool restore
dotnet ef migrations add MigrationName --project src/OrderManagement.Infrastructure --startup-project src/OrderManagement.Api
dotnet test tests/OrderManagement.IntegrationTests/OrderManagement.IntegrationTests.csproj --configuration Release
```

Commit the migration, model snapshot, and manifest update together. CI verifies the migrations against SQL Server and creates the bundle from the same Release build as the API.

Production migration rules:

- Prefer additive, backward-compatible expand/contract changes.
- Separate long data backfills from schema changes.
- Review table size, locks, indexes, and expected execution time.
- Use a dedicated migration credential with schema-change permission.
- Give the runtime API identity only its required data permissions.
- Never run automatic EF `Down` migrations in the deployment workflow.

The bundle uses `__EFMigrationsHistory`, so rerunning a successful migration is idempotent. Environment deployment concurrency prevents two workflow runs from migrating the same target simultaneously.

## 4. Seeding policy

There is no required production reference data, so Phase 7 intentionally provides no production seed operation. Customers, products, and orders are user-owned business data and must never be inserted or reset during deployment.

Integration fixtures stay in the test project. If non-production demo data is added later, it must be an explicit opt-in command, reject Production, use stable business keys, and be safe to rerun without duplicates.

## 5. Azure and GitHub prerequisites

Create the Azure App Service and Azure SQL Database described by ADR-004. Configure App Service with:

- .NET 10 runtime
- HTTPS-only access
- `ConnectionStrings__OrderManagement` as an App Service setting or Key Vault reference
- App Service Health Check path `/api/health`
- a runtime database identity/credential that cannot alter schema

Create GitHub Environments named `development`, `staging`, and `production` as needed. Production must require reviewer approval. Configure these environment variables:

| Name | Purpose |
| --- | --- |
| `AZURE_CLIENT_ID` | Federated application or managed identity client ID |
| `AZURE_TENANT_ID` | Microsoft Entra tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Target Azure subscription |
| `AZURE_WEBAPP_NAME` | App Service application name |
| `HEALTH_BASE_URL` | HTTPS origin without a trailing slash |
| `AZURE_SQL_RECOVERY_POLICY` | Short operator-visible description of backup/PITR readiness |

Configure one environment secret:

| Name | Purpose |
| --- | --- |
| `MIGRATION_CONNECTION_STRING` | Azure SQL connection for the short-lived migration job |

The secret is a pragmatic GitHub-hosted runner option. Where network and identity design permit it, replace it with passwordless Azure-hosted execution and managed identity. Never expose production values in repository files, workflow inputs, logs, or artifacts.

Configure GitHub OIDC federation for the repository/environment subject and grant only the Azure roles required to deploy the named App Service. The workflow grants `id-token: write` only to jobs that authenticate to Azure.

## 6. Release procedure

1. Merge the reviewed change to `main`.
2. Wait for `Continuous Integration` to succeed and note its workflow run ID.
3. Confirm the release artifact contains the intended commit and migration ID in `release-metadata.txt`.
4. Confirm Azure SQL automated backups/PITR retention and the recovery owner for the target environment.
5. Dispatch `Deploy validated release` with the target GitHub Environment and successful CI run ID.
6. Approve the protected environment after reviewing the commit, migration manifest, compatibility notes, and recovery readiness.
7. The workflow verifies the source run is a successful `main` CI run and validates all checksums.
8. The migration job authenticates through OIDC and executes the approved bundle once.
9. The deployment job promotes the matching API artifact to App Service.
10. The workflow polls `/alive`, `/health`, and `/api/health` over HTTPS and records release evidence in its job summary.
11. Perform a non-destructive operator smoke test: read an existing record or create uniquely named test data only when an approved cleanup path exists.

Do not rerun a failed deployment blindly. Identify whether failure occurred before migration, after migration but before application deployment, or during health verification, then use the recovery table below.

## 7. Health verification

| Endpoint | Meaning | Healthy result |
| --- | --- | --- |
| `/alive` | Process liveness only | HTTP 200 |
| `/health` | Internal readiness including database | HTTP 200 |
| `/api/health` | Public JSON health contract including `self` and `database` checks | HTTP 200 and `status: healthy` |

The operational endpoints disclose only standard health status. Do not add exception details, connection information, or credentials to their responses.

## 8. Failure and rollback runbook

| Failure point | Action |
| --- | --- |
| Artifact or checksum validation | Stop. Do not migrate or deploy. Correct the selected CI run ID or produce a new validated release. |
| Migration before any schema change | Stop and retain logs. Correct connectivity/permission/configuration, then rerun after review. |
| Migration after partial schema work | Do not run `Down`. Inspect `__EFMigrationsHistory` and database state; engage the database owner before retry or restore. |
| App deployment after successful compatible migration | Redeploy the previously successful API artifact. Leave additive schema in place. |
| Health verification | Stop promotion, inspect App Service and database telemetry, and redeploy the previous API artifact if the new version is responsible. |
| Destructive/incompatible database failure | Stop writes, follow Azure SQL point-in-time restore into a new database, validate it, repoint configuration under operator approval, and preserve the failed database for investigation. |

Rollback is complete only when the selected prior API artifact is running, all three health endpoints pass, a read-only business smoke test succeeds, and the incident record contains the deployed commit, migration state, timestamps, and operator.

For production, record the last successful workflow run ID and artifact retention location after every release. A production migration that cannot coexist with the previous API requires a maintenance window, rehearsed restore, and explicit approval before execution.

## 9. Current readiness boundary

Repository automation, migration policy, checksums, OIDC workflow permissions, operational health endpoints, and recovery instructions are implemented. A real deployment remains intentionally blocked by missing or unverified external state until an operator provisions Azure resources, establishes OIDC federation, configures GitHub Environments, validates database networking, and completes a staging rehearsal.
