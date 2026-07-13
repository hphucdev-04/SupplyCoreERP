# SupplyCoreERP Backend Guide

This document describes the backend source tree, ABP layer boundaries, runtime
configuration, and common backend commands. Clone, full local startup, and
production deployment steps are documented in
[`../CUSTOMER-HANDOVER-GUIDE.md`](../CUSTOMER-HANDOVER-GUIDE.md).

## Backend Solution Tree

```text
src/
+-- SupplyCoreERP.Domain.Shared/
|   +-- Localization/
|   +-- SupplyCoreERPDomainSharedModule.cs
|   +-- SupplyCoreERP.Domain.Shared.csproj
+-- SupplyCoreERP.Domain/
|   +-- Agent/
|   +-- Catalog/
|   +-- Common/
|   +-- Data/
|   +-- Identity/
|   +-- Inventory/
|   +-- Locations/
|   +-- OpenIddict/
|   +-- Partner/
|   +-- Procurement/
|   +-- Sales/
|   +-- Settings/
|   +-- SupplyCoreERPDomainModule.cs
|   +-- SupplyCoreERP.Domain.csproj
+-- SupplyCoreERP.Application.Contracts/
|   +-- ActiveIngredients/
|   +-- Agent/
|   +-- Balances/
|   +-- BaseUnits/
|   +-- Batches/
|   +-- Categories/
|   +-- Customers/
|   +-- Dashboard/
|   +-- DosageForms/
|   +-- Locations/
|   +-- Manufacturers/
|   +-- Mcp/
|   +-- Medicines/
|   +-- Notifications/
|   +-- Permissions/
|   +-- Prices/
|   +-- PurchaseOrders/
|   +-- PurchaseRequisitions/
|   +-- PurchaseReturnRequests/
|   +-- PurchaseReturns/
|   +-- SalesOrders/
|   +-- SalesRecalls/
|   +-- Settings/
|   +-- Suppliers/
|   +-- Tickets/
|   +-- Transactions/
|   +-- Warehouses/
|   +-- SupplyCoreERPApplicationContractsModule.cs
|   +-- SupplyCoreERP.Application.Contracts.csproj
+-- SupplyCoreERP.Application/
|   +-- ActiveIngredients/
|   +-- Balances/
|   +-- Categories/
|   +-- Customers/
|   +-- Dashboard/
|   +-- Medicines/
|   +-- PurchaseOrders/
|   +-- PurchaseRequisitions/
|   +-- PurchaseReturns/
|   +-- SalesOrders/
|   +-- Settings/
|   +-- Suppliers/
|   +-- Warehouses/
|   +-- SupplyCoreERPApplicationMappers.cs
|   +-- SupplyCoreERPApplicationModule.cs
|   +-- SupplyCoreERPAppService.cs
|   +-- SupplyCoreERP.Application.csproj
+-- SupplyCoreERP.EntityFrameworkCore/
|   +-- EntityFrameworkCore/
|   +-- Migrations/
|   +-- SupplyCoreERP.EntityFrameworkCore.csproj
|   +-- EntityFrameworkCore/SupplyCoreERPEntityFrameworkCoreModule.cs
+-- SupplyCoreERP.HttpApi/
|   +-- SupplyCoreERPHttpApiModule.cs
|   +-- SupplyCoreERP.HttpApi.csproj
+-- SupplyCoreERP.HttpApi.Client/
|   +-- SupplyCoreERPHttpApiClientModule.cs
|   +-- SupplyCoreERP.HttpApi.Client.csproj
+-- SupplyCoreERP.HttpApi.Host/
|   +-- HealthChecks/
|   +-- Pages/
|   +-- SignalR/
|   +-- wwwroot/
|   +-- appsettings.json
|   +-- appsettings.Development.json
|   +-- appsettings.Production.json
|   +-- Program.cs
|   +-- SupplyCoreERPHttpApiHostModule.cs
|   +-- SupplyCoreERP.HttpApi.Host.csproj
+-- SupplyCoreERP.DbMigrator/
|   +-- appsettings.json
|   +-- appsettings.Development.json
|   +-- appsettings.Production.json
|   +-- Program.cs
|   +-- SupplyCoreERPDbMigratorModule.cs
|   +-- SupplyCoreERP.DbMigrator.csproj
+-- SupplyCoreERP.Mcp.Client/
    +-- SupplyCoreERPMcpClientModule.cs
    +-- SupplyCoreERP.Mcp.Client.csproj
```

## Layer Responsibilities

| Project | Responsibility |
| --- | --- |
| `SupplyCoreERP.Domain.Shared` | Shared constants, localization, enums, and values that can be referenced across layers. |
| `SupplyCoreERP.Domain` | Entities, aggregate roots, domain services, repository interfaces, domain events, and business rules. |
| `SupplyCoreERP.Application.Contracts` | DTOs, app service interfaces, permissions, and contracts consumed by clients and API layers. |
| `SupplyCoreERP.Application` | Application service implementations, orchestration, validation, object mapping, and use-case workflows. |
| `SupplyCoreERP.EntityFrameworkCore` | EF Core `DbContext`, entity configuration, migrations, repository implementations, and database integration. |
| `SupplyCoreERP.HttpApi` | HTTP API layer and controller exposure when explicit controllers are required. |
| `SupplyCoreERP.HttpApi.Client` | Typed client project for API consumption scenarios. |
| `SupplyCoreERP.HttpApi.Host` | Final ASP.NET Core host, middleware pipeline, authentication, Swagger, SignalR, health checks, and DI composition. |
| `SupplyCoreERP.DbMigrator` | Console app for applying database migrations and seeding initial data. |
| `SupplyCoreERP.Mcp.Client` | Backend-side MCP client integration. |

## Dependency Rules

Follow the ABP layered dependency direction:

```text
Domain.Shared
    ^
Domain
    ^
Application.Contracts
    ^
Application
    ^
HttpApi
    ^
HttpApi.Host
```

Keep these rules strict:

- Domain logic does not depend on EF Core, `DbContext`, HTTP, or application services.
- Application services use repository abstractions and domain services, not direct `DbContext` access.
- DTOs and app service interfaces live in `Application.Contracts`.
- Repository interfaces live in `Domain` when custom query contracts are needed.
- Repository implementations live in `EntityFrameworkCore`.
- `HttpApi` depends on contracts, not concrete application service implementations.
- `HttpApi.Host` composes the final runtime dependencies.

## Configuration

Backend configuration is mainly stored in:

- `SupplyCoreERP.HttpApi.Host/appsettings.json`
- `SupplyCoreERP.HttpApi.Host/appsettings.Development.json`
- `SupplyCoreERP.HttpApi.Host/appsettings.Production.json`
- `SupplyCoreERP.DbMigrator/appsettings.json`
- `SupplyCoreERP.DbMigrator/appsettings.Development.json`
- `SupplyCoreERP.DbMigrator/appsettings.Production.json`

Important settings:

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:Default` | PostgreSQL connection string. |
| `App:SelfUrl` | Backend public URL. |
| `App:AngularUrl` | Frontend public URL. |
| `App:CorsOrigins` | Allowed browser origins for API calls. |
| `App:RedirectAllowedUrls` | Allowed OAuth redirect URLs. |
| `AuthServer:Authority` | OpenIddict authority URL. |
| `AuthServer:RequireHttpsMetadata` | Must be `true` in production. |
| `AuthServer:CertificatePassPhrase` | Passphrase for the OpenIddict certificate. |
| `StringEncryption:DefaultPassPhrase` | Secret used for ABP string encryption. |

Do not commit production credentials or certificates.

## Database Migration

Run migrations and seed data with:

```bash
dotnet run --project src/SupplyCoreERP.DbMigrator
```

Use `DbMigrator` before the backend starts in a fresh environment and before
deploying a backend version that includes schema changes.

## Local Backend Run

From the repository root:

```bash
dotnet run --project src/SupplyCoreERP.HttpApi.Host
```

Default manual development endpoints:

- Backend: `https://localhost:44367`
- Swagger: `https://localhost:44367/swagger`

When running through Docker Compose, the backend is exposed at:

- Backend: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`

## Build and Test

Build the backend solution:

```bash
dotnet build SupplyCoreERP.slnx
```

Run all backend tests:

```bash
dotnet test SupplyCoreERP.slnx
```

Backend tests are under `../test/`:

```text
test/
+-- SupplyCoreERP.Application.Tests/
+-- SupplyCoreERP.Domain.Tests/
+-- SupplyCoreERP.EntityFrameworkCore.Tests/
+-- SupplyCoreERP.HttpApi.Client.ConsoleTestApp/
+-- SupplyCoreERP.TestBase/
```

## Docker Notes

The root-level Dockerfiles are used for backend deployment:

- `../Dockerfile` builds and runs `SupplyCoreERP.HttpApi.Host`.
- `../Dockerfile.dbmigrator` builds and runs `SupplyCoreERP.DbMigrator`.
- `../docker-compose.yml` starts PostgreSQL, migrator, backend, frontend, and MCP server for local container runs.

For the complete production deployment sequence, use
[`../CUSTOMER-HANDOVER-GUIDE.md`](../CUSTOMER-HANDOVER-GUIDE.md).

## Backend Development Checklist

- Put domain rules in `SupplyCoreERP.Domain`.
- Put DTOs, app service interfaces, and permission definitions in `SupplyCoreERP.Application.Contracts`.
- Put use-case orchestration in `SupplyCoreERP.Application`.
- Put EF Core mappings, migrations, and repository implementations in `SupplyCoreERP.EntityFrameworkCore`.
- Run `dotnet build SupplyCoreERP.slnx` before opening a backend PR.
- Run `dotnet test SupplyCoreERP.slnx` when behavior changes.
