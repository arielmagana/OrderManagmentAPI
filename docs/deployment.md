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

## 5. Production Identity and Network Configuration

Production uses three independent Microsoft Entra identities:

| Identity | Purpose | Authorization |
| --- | --- | --- |
| GitHub deployment identity | Deploy the validated API artifact | `Website Contributor` on the named App Service only |
| GitHub migration identity | Apply the EF Core migration bundle | Database migration roles only; no Azure RBAC assignment |
| App Service system-assigned identity | Runtime API database access | Database read/write roles only |

No identity uses a client secret or SQL password. The two GitHub identities trust the exact GitHub `production` environment subject through separate federated credentials. For repositories using immutable OIDC subjects, the credential must include the owner and repository IDs emitted by GitHub.

### 5.1 Azure SQL Firewall

App Service F1 does not support VNet integration. For this demonstration, configure the Azure SQL logical server with public access restricted to selected networks and enable **Allow Azure services and resources to access this server**. Azure represents this exception as:

```text
Rule: AllowAllWindowsAzureIps
Start IP: 0.0.0.0
End IP: 0.0.0.0
```

Verify it from the virtual `master` database:

```sql
SELECT name, start_ip_address, end_ip_address
FROM sys.firewall_rules
WHERE name = N'AllowAllWindowsAzureIps';
```

This rule provides network reachability to Azure resources in any subscription; it does not grant database access. Microsoft Entra authentication and contained database roles remain mandatory. Replace this broad exception with private networking after moving to an App Service tier that supports VNet integration.

### 5.2 Migration Identity

Create an Entra application/service principal named `github-order-management-production-migrations` with no client secret and no Azure RBAC assignment. Add a federated credential with:

```text
Issuer: https://token.actions.githubusercontent.com
Audience: api://AzureADTokenExchange
Subject: the exact repository production-environment OIDC subject
```

Connect to the application database as its Entra administrator and use the migration service principal's object ID:

```sql
CREATE USER [github-order-management-production-migrations]
FROM EXTERNAL PROVIDER
WITH OBJECT_ID = '<migration-service-principal-object-id>';

ALTER ROLE db_ddladmin
ADD MEMBER [github-order-management-production-migrations];

ALTER ROLE db_datareader
ADD MEMBER [github-order-management-production-migrations];

ALTER ROLE db_datawriter
ADD MEMBER [github-order-management-production-migrations];
```

Do not grant `db_owner`, an App Service role, or subscription/resource-group permissions. The workflow uses `allow-no-subscriptions: true` to establish the Entra session and requests an Azure SQL token before executing the migration bundle.

### 5.3 App Service Runtime Identity

Enable the App Service system-assigned managed identity and obtain its principal ID. Connect to the application database as its Entra administrator:

```sql
CREATE USER [<app-service-name>]
FROM EXTERNAL PROVIDER
WITH OBJECT_ID = '<app-service-principal-id>';

ALTER ROLE db_datareader ADD MEMBER [<app-service-name>];
ALTER ROLE db_datawriter ADD MEMBER [<app-service-name>];
```

When executing the script, replace the placeholder but retain the square brackets, for example `[order-management-api]`. Do not grant this identity `db_ddladmin` or `db_owner`.

Configure App Service with the .NET 10 runtime, HTTPS-only access, Health Check path `/api/health`, and this application setting:

```text
ConnectionStrings__OrderManagement=Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;
```

### 5.4 GitHub Production Environment

The `production` environment must require reviewer approval, restrict deployments to `main`, and contain:

| Variable | Purpose |
| --- | --- |
| `AZURE_CLIENT_ID` | Existing App Service deployment identity client ID |
| `MIGRATION_AZURE_CLIENT_ID` | Migration-only identity client ID |
| `AZURE_TENANT_ID` | Microsoft Entra tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Target subscription used by the deployment identity |
| `AZURE_WEBAPP_NAME` | App Service name |
| `HEALTH_BASE_URL` | HTTPS origin without a trailing slash |
| `AZURE_SQL_RECOVERY_POLICY` | Operator-visible backup/PITR readiness statement |

Configure this environment secret:

| Secret | Value |
| --- | --- |
| `MIGRATION_CONNECTION_STRING` | `Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;` |

The connection string contains no reusable credential but remains an environment secret to avoid exposing environment topology. Never print it or an access token in workflow logs.

## 6. Release procedure

1. Verify the deployment, migration, and runtime identities have the roles defined in section 5.
2. Verify the Azure-services firewall exception and both passwordless connection strings.
3. Merge the reviewed change to `main`.
4. Wait for `Continuous Integration` to succeed and note its workflow run ID.
5. Confirm the release artifact contains the intended commit and migration ID in `release-metadata.txt`.
6. Confirm Azure SQL automated backups/PITR retention and the recovery owner for the target environment.
7. Dispatch `Deploy validated release` with `production` and the successful CI run ID.
8. Approve the protected environment after reviewing the commit, migration manifest, compatibility notes, and recovery readiness.
9. The workflow verifies the CI source and artifact checksums.
10. The migration job authenticates as the migration identity, verifies Azure SQL token acquisition, and executes the approved bundle once.
11. The deployment job authenticates as the deployment identity and promotes the matching API artifact to App Service.
12. The workflow polls `/alive`, `/health`, and `/api/health` over HTTPS and records release evidence in its job summary.
13. Perform a non-destructive operator smoke test: read an existing record or create uniquely named test data only when an approved cleanup path exists.

Before the first deployment, capture identity evidence with:

```sql
SELECT
    member_principal.name AS principal_name,
    role_principal.name AS role_name
FROM sys.database_role_members AS membership
JOIN sys.database_principals AS role_principal
    ON role_principal.principal_id = membership.role_principal_id
JOIN sys.database_principals AS member_principal
    ON member_principal.principal_id = membership.member_principal_id
WHERE member_principal.name IN (
    N'github-order-management-production-migrations',
    N'<app-service-name>')
ORDER BY member_principal.name, role_principal.name;
```

Expected results are `db_ddladmin`, `db_datareader`, and `db_datawriter` for the migration identity, and only `db_datareader` plus `db_datawriter` for the App Service identity. The deployment identity must not appear as a database principal.

After migration, verify:

```sql
SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId;
```

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

Repository automation, migration policy, checksums, segregated OIDC workflow permissions, operational health endpoints, and recovery instructions are implemented. A real deployment remains intentionally blocked until an operator completes the section 5 configuration and validates it through the first approved production deployment.
