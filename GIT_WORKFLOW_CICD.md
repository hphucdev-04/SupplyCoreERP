# Git Workflow and CI/CD Guide

This guide defines the recommended Git workflow, pull request rules, CI checks,
and deployment flow for SupplyCoreERP.

## Branch Strategy

Use a simple GitHub Flow model:

```text
main
+-- feature/<short-description>
+-- fix/<short-description>
+-- chore/<short-description>
+-- hotfix/<short-description>
+-- deploy/dev
+-- deploy/mcp-server
```

| Branch | Purpose |
| --- | --- |
| `main` | Production-ready branch. Deployments should come from this branch or tagged commits on this branch. |
| `feature/*` | New product or technical features. |
| `fix/*` | Bug fixes that are not urgent production hotfixes. |
| `chore/*` | Maintenance tasks, documentation, dependency updates, and build changes. |
| `hotfix/*` | Urgent production fixes branched from `main`. |
| `deploy/dev` | Deployment branch for the main application pipeline. |
| `deploy/mcp-server` | Deployment branch for the MCP server pipeline. |

Do not commit directly to `main` unless the repository owner explicitly allows
it for an emergency.

## Commit Convention

Use Conventional Commits:

```text
<type>(<scope>): <summary>
```

Examples:

```text
feat(application): add purchase requisition approval flow
fix(angular): correct sales order validation message
chore(ci): update backend build pipeline
docs(readme): add production endpoints
```

Recommended types:

| Type | Usage |
| --- | --- |
| `feat` | User-facing feature or new capability. |
| `fix` | Bug fix. |
| `docs` | Documentation-only change. |
| `test` | Test additions or corrections. |
| `refactor` | Code change without intended behavior change. |
| `chore` | Tooling, dependency, build, or maintenance work. |
| `ci` | CI/CD pipeline changes. |

Use concise scopes such as `domain`, `application`, `efcore`, `httpapi`,
`host`, `angular`, `mcp`, `ci`, or `docs`.

## Pull Request Rules

Each pull request should include:

- Summary of the change.
- Affected areas: backend, frontend, MCP, database, deployment, or docs.
- Verification commands that were run.
- Screenshots or screen recordings for visible UI changes.
- Migration notes when EF Core migrations or seed data changed.
- Deployment notes when runtime configuration or infrastructure changes.

Before requesting review:

```bash
dotnet build SupplyCoreERP.slnx
dotnet test SupplyCoreERP.slnx
cd angular && npm run lint
cd angular && npm run build:prod
cd mcp-server && npm run build
```

Run the checks that match the changed area. For cross-cutting changes, run all
checks.

## Current GitHub Actions Workflows

The repository defines two GitHub Actions workflows:

- `.github/workflows/ci-cd-dev.yml`
- `.github/workflows/ci-cd-mcp-server.yaml`

### Main Application Pipeline

Workflow file:

```text
.github/workflows/ci-cd-dev.yml
```

Triggers:

- `push` to `deploy/dev`
- `pull_request` targeting `deploy/dev`
- Manual run through `workflow_dispatch`

Pipeline order:

```text
static-analysis
    -> build-and-test
        -> database-migration
            -> deploy-backend
            -> deploy-frontend
```

#### `static-analysis`

Purpose:

- Checkout source code.
- Setup .NET SDK.
- Restore .NET dependencies.
- Verify code formatting.

Commands:

```bash
dotnet restore
dotnet format --verify-no-changes
```

#### `build-and-test`

Purpose:

- Checkout source code.
- Setup .NET SDK.
- Restore .NET dependencies.
- Build the backend solution in `Release`.
- Run backend unit tests.
- Run backend integration tests.
- Setup Node.js 20.
- Install Angular dependencies.
- Build Angular in production mode.
- Upload test result artifacts.

Backend commands:

```bash
dotnet restore
dotnet build --no-restore --configuration Release
dotnet test test/SupplyCoreERP.Domain.Tests/SupplyCoreERP.Domain.Tests.csproj \
  --no-build \
  --no-restore \
  --configuration Release \
  --verbosity normal \
  --logger trx \
  --blame-hang \
  --blame-hang-timeout 60s
dotnet test test/SupplyCoreERP.EntityFrameworkCore.Tests/SupplyCoreERP.EntityFrameworkCore.Tests.csproj \
  --no-build \
  --no-restore \
  --configuration Release \
  --verbosity normal \
  --logger trx \
  --blame-hang \
  --blame-hang-timeout 400s \
  -- xunit.parallelizeAssembly=false xunit.parallelizeTestCollections=false xunit.maxParallelThreads=1
```

Frontend commands:

```bash
cd angular
npm ci --legacy-peer-deps
npm run build:prod
```

Test responsibilities:

| Test project | Type | Purpose |
| --- | --- | --- |
| `test/SupplyCoreERP.Domain.Tests` | Unit/domain tests | Validates domain behavior and business rules. |
| `test/SupplyCoreERP.EntityFrameworkCore.Tests` | Integration tests | Validates EF Core mappings, repositories, database integration, and infrastructure behavior. |

#### `database-migration`

Purpose:

- Build `SupplyCoreERP.DbMigrator`.
- Run database migrations and seed data against the configured production database.

Runs only for:

- `push` events
- `workflow_dispatch`

Environment:

```text
production
```

Commands:

```bash
dotnet restore
dotnet build src/SupplyCoreERP.DbMigrator/SupplyCoreERP.DbMigrator.csproj --no-restore --configuration Release
dotnet run \
  --project src/SupplyCoreERP.DbMigrator/SupplyCoreERP.DbMigrator.csproj \
  --configuration Release \
  --no-build \
  --no-restore
```

Required secret:

```text
DB_CONNECTION_STRING
```

#### `deploy-backend`

Purpose:

- Install Railway CLI.
- Deploy the backend service to Railway.

Runs only after `database-migration` succeeds.

Command:

```bash
npm install -g @railway/cli
railway up --detach --service rxlogistics-backend
```

Required secret:

```text
RAILWAY_TOKEN
```

#### `deploy-frontend`

Purpose:

- Deploy the frontend to Vercel production.

Runs only after `database-migration` succeeds.

Action:

```text
amondnet/vercel-action@v25
```

Required secrets:

```text
VERCEL_TOKEN
VERCEL_ORG_ID
VERCEL_PROJECT_ID
```

### MCP Server Pipeline

Workflow file:

```text
.github/workflows/ci-cd-mcp-server.yaml
```

Triggers:

- `push` to `deploy/mcp-server`
- Manual run through `workflow_dispatch`

Pipeline order:

```text
build
    -> deploy-mcp-server
```

#### `build`

Purpose:

- Checkout source code.
- Setup Node.js 20.
- Install MCP server dependencies.
- Compile the TypeScript MCP server.

Commands:

```bash
cd mcp-server
npm ci
npm run build
```

#### `deploy-mcp-server`

Purpose:

- Install Railway CLI.
- Deploy the MCP server service to Railway.

Runs only after `build` succeeds.

Commands:

```bash
npm install -g @railway/cli
cd mcp-server
railway up --detach --service mcp-server
```

Required secret:

```text
RAILWAY_TOKEN
```

## Release and Tagging

Use semantic version tags when creating an explicit release:

```text
vMAJOR.MINOR.PATCH
```

Examples:

```text
v1.0.0
v1.1.0
v1.1.1
```

Tag only commits that have passed CI and are intended for deployment.

## CD Targets

Current production endpoints:

| Component | Hosting target | Production URL |
| --- | --- | --- |
| Frontend | Vercel | `https://rxlogistics.vercel.app` |
| Backend API | Railway | `https://rxlogistics.up.railway.app` |
| MCP Server | Railway | `https://rxlogistics-mcp.up.railway.app/mcp` |

## Deployment Order

Deploy in this order:

1. Review database migrations and required configuration changes.
2. Back up the production database when schema changes are included.
3. Deploy or run `SupplyCoreERP.DbMigrator`.
4. Deploy backend API.
5. Deploy frontend.
6. Deploy MCP server when MCP code or environment changes.
7. Run smoke tests.

Do not deploy frontend changes that depend on new backend contracts before the
backend version is available.

## Backend Deployment

Backend deployment must provide these settings through Railway environment
variables or the selected secret store:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__Default=<PRODUCTION_CONNECTION_STRING>
App__SelfUrl=https://rxlogistics.up.railway.app
App__AngularUrl=https://rxlogistics.vercel.app
App__CorsOrigins=https://rxlogistics.vercel.app
App__RedirectAllowedUrls=https://rxlogistics.vercel.app
AuthServer__Authority=https://rxlogistics.up.railway.app
AuthServer__RequireHttpsMetadata=true
AuthServer__CertificatePassPhrase=<CERTIFICATE_PASSWORD>
StringEncryption__DefaultPassPhrase=<STRONG_RANDOM_SECRET>
```

Run the migrator before the backend version goes live when schema changes are
included.

## Frontend Deployment

Frontend deployment must use production runtime values:

```text
application.baseUrl=https://rxlogistics.vercel.app
oAuthConfig.issuer=https://rxlogistics.up.railway.app/
oAuthConfig.redirectUri=https://rxlogistics.vercel.app
apis.default.url=https://rxlogistics.up.railway.app
apis.AbpAccountPublic.url=https://rxlogistics.up.railway.app/
oAuthConfig.requireHttps=true
```

For Vercel deployment, ensure the build command matches the project:

```bash
cd angular
npm run build:prod
```

The output directory is:

```text
angular/dist/SupplyCoreERP
```

## MCP Deployment

MCP deployment must provide:

```text
NODE_ENV=production
PORT=3000
DATABASE_URL=<PRODUCTION_DATABASE_URL>
ALLOWED_ORIGINS=https://rxlogistics.vercel.app
```

Keep MCP internal unless the production architecture requires public access.
When public access is required, restrict allowed origins and route only the
required MCP endpoint.

## Required GitHub Secrets

Configure these secrets in the GitHub repository before running deployment
workflows:

| Secret | Used by | Purpose |
| --- | --- | --- |
| `DB_CONNECTION_STRING` | `ci-cd-dev.yml` / `database-migration` | Production PostgreSQL connection string used by `SupplyCoreERP.DbMigrator`. |
| `RAILWAY_TOKEN` | `ci-cd-dev.yml` / `deploy-backend`; `ci-cd-mcp-server.yaml` / `deploy-mcp-server` | Authenticates Railway CLI deployments. |
| `VERCEL_TOKEN` | `ci-cd-dev.yml` / `deploy-frontend` | Authenticates Vercel deployment. |
| `VERCEL_ORG_ID` | `ci-cd-dev.yml` / `deploy-frontend` | Identifies the Vercel organization/team. |
| `VERCEL_PROJECT_ID` | `ci-cd-dev.yml` / `deploy-frontend` | Identifies the Vercel frontend project. |

## Secret Handling

Never commit:

- Production database credentials.
- `openiddict.pfx` production certificates.
- Certificate passphrases.
- `StringEncryption__DefaultPassPhrase`.
- API keys, tokens, or provider credentials.

Store secrets in GitHub Actions secrets, Vercel environment variables, Railway
environment variables, or another approved secret manager.

## Smoke Tests After Deployment

After deployment, verify:

- Frontend opens at `https://rxlogistics.vercel.app`.
- Login works with the expected admin account.
- Backend Swagger or health endpoint is reachable.
- A basic authenticated API request succeeds.
- MCP endpoint is reachable when it is intended to be public.
- No database migration error appears in backend logs.

## Rollback

For application failures:

1. Revert to the previous frontend deployment on Vercel when the UI is broken.
2. Roll back the backend Railway deployment to the previous known-good version.
3. Roll back the MCP Railway deployment when MCP behavior is broken.
4. Re-run smoke tests.

For database failures:

1. Stop newly deployed services that depend on the failed schema.
2. Restore the latest verified database backup when required.
3. Redeploy the previous known-good application version.
4. Investigate the failed migration before attempting another release.

Do not run destructive database rollback steps without a verified backup.
