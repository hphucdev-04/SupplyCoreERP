# SupplyCoreERP Customer Handover and Deployment Guide

This guide explains how to clone, configure, run, and deploy **SupplyCoreERP**.
It is intended for customer handover, operators, and developers who need a
repeatable setup path.

SupplyCoreERP is an ABP Framework layered monolith built with:

- ASP.NET Core / .NET 10 backend
- Angular 20 frontend
- PostgreSQL database
- TypeScript MCP server
- Docker Compose for local container orchestration

## 1. System Components

| Component | Path | Purpose | Default local endpoint |
| --- | --- | --- | --- |
| Backend API | `src/SupplyCoreERP.HttpApi.Host` | ASP.NET Core host, Swagger, authentication, application APIs | `http://localhost:8080` in Docker, `https://localhost:44367` manually |
| DbMigrator | `src/SupplyCoreERP.DbMigrator` | Applies EF Core migrations and seeds initial data | Runs as a console process |
| Frontend | `angular/` | Angular web client served by Angular CLI or Nginx | `http://localhost:4200` |
| MCP Server | `mcp-server/` | TypeScript MCP server connected to PostgreSQL | `http://localhost:3000` |
| Database | PostgreSQL | Main application database | `localhost:5432` in Docker |

## 2. Prerequisites

### Docker-based local run

- Docker Desktop with Docker Compose enabled

### Manual development run

- .NET 10 SDK
- Node.js 20
- Yarn or npm
- PostgreSQL 15 or later
- ABP CLI, if running `abp install-libs` manually

Install ABP CLI when needed:

```bash
dotnet tool install -g Volo.Abp.Cli
```

## 3. Clone the Repository

```bash
git clone <REPOSITORY_URL>
cd SupplyCoreERP
```

Replace `<REPOSITORY_URL>` with the real Git repository URL provided during
handover.

## 4. Run Locally with Docker Compose

This is the recommended local handover path because it starts PostgreSQL,
DbMigrator, backend, frontend, and MCP server from one command.

From the repository root:

```bash
docker compose up --build
```

Docker Compose starts these services:

- `supplycore-db`: PostgreSQL 15 database
- `supplycore-migrator`: applies migrations and seed data, then exits
- `supplycore-backend`: backend API on port `8080`
- `supplycore-frontend`: Angular app served by Nginx on port `4200`
- `supplycore-mcp-server`: MCP server on port `3000`

After startup:

- Frontend: `http://localhost:4200`
- Backend Swagger: `http://localhost:8080/swagger`
- MCP Server: `http://localhost:3000`

Default ABP administrator account:

- Username: `admin`
- Password: `1q2w3E*`

Stop the local stack:

```bash
docker compose down
```

Stop the stack and remove the PostgreSQL volume:

```bash
docker compose down -v
```

Use `docker compose down -v` only when the local database can be discarded.

## 5. Manual Development Setup

Use this path when developers need to debug individual services.

### 5.1 Configure PostgreSQL

Create a PostgreSQL database and update the backend and migrator connection
strings:

- `src/SupplyCoreERP.HttpApi.Host/appsettings.json`
- `src/SupplyCoreERP.DbMigrator/appsettings.json`

Example:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=SupplyCore;Username=postgres;Password=<DB_PASSWORD>;Port=5432;"
  }
}
```

Do not commit real production credentials to Git.

### 5.2 Install Backend Client Libraries

From the repository root:

```bash
abp install-libs
```

### 5.3 Create the OpenIddict Development Certificate

The backend expects an `openiddict.pfx` certificate for token signing and
encryption.

For local development:

```bash
dotnet dev-certs https -v -ep src/SupplyCoreERP.HttpApi.Host/openiddict.pfx -p <CERTIFICATE_PASSWORD>
```

The password must match `AuthServer:CertificatePassPhrase` in the backend
configuration.

### 5.4 Apply Migrations and Seed Data

From the repository root:

```bash
dotnet run --project src/SupplyCoreERP.DbMigrator
```

### 5.5 Run the Backend API

```bash
dotnet run --project src/SupplyCoreERP.HttpApi.Host
```

The default manual development URL is:

- Backend: `https://localhost:44367`
- Swagger: `https://localhost:44367/swagger`

### 5.6 Run the Angular Frontend

```bash
cd angular
npm install
npm start
```

The Angular development server runs at:

```text
http://localhost:4200
```

For production-style local build validation:

```bash
npm run build:prod
```

### 5.7 Run the MCP Server

```bash
cd mcp-server
npm install
npm run build
npm run dev
```

Set these environment variables before running the MCP server outside Docker:

```text
DATABASE_URL=postgresql://<USER>:<PASSWORD>@<HOST>:<PORT>/<DATABASE>
PORT=3000
NODE_ENV=development
ALLOWED_ORIGINS=http://localhost:4200,http://localhost:3000
```

## 6. Production Deployment

The repository contains Dockerfiles for backend, DbMigrator, frontend, and MCP
server. Production deployment should use environment-specific configuration and
externalized secrets.

### 6.1 Deployment Order

Deploy in this order:

1. Provision PostgreSQL.
2. Configure production secrets and environment variables.
3. Build and run `DbMigrator` against the production database.
4. Deploy the backend API.
5. Deploy the Angular frontend.
6. Deploy the MCP server if the environment requires it.
7. Configure reverse proxy, TLS, CORS, and OAuth redirect URLs.
8. Run smoke tests.

### 6.2 PostgreSQL

Create a production PostgreSQL database. Store the connection string in a secret
manager or deployment environment variable.

Required backend/migrator setting:

```text
ConnectionStrings__Default=Host=<DB_HOST>;Database=<DB_NAME>;Username=<DB_USER>;Password=<DB_PASSWORD>;Port=5432;SSL Mode=Require;Trust Server Certificate=true;
```

Adjust SSL settings according to the actual PostgreSQL provider.

### 6.3 DbMigrator

Run the migrator as a one-off job before starting the new backend version:

```bash
docker build -f Dockerfile.dbmigrator -t supplycore-migrator:<VERSION> .
docker run --rm \
  -e ConnectionStrings__Default="<PRODUCTION_CONNECTION_STRING>" \
  supplycore-migrator:<VERSION>
```

Run migrations during a controlled deployment window. Review migration scripts
before production release when schema changes are included.

### 6.4 Backend API

Build the backend image:

```bash
docker build -f Dockerfile -t supplycore-backend:<VERSION> .
```

Run the backend with production settings:

```bash
docker run -d \
  --name supplycore-backend \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS="http://+:8080" \
  -e ConnectionStrings__Default="<PRODUCTION_CONNECTION_STRING>" \
  -e App__SelfUrl="https://api.example.com" \
  -e App__AngularUrl="https://app.example.com" \
  -e App__CorsOrigins="https://app.example.com" \
  -e App__RedirectAllowedUrls="https://app.example.com" \
  -e AuthServer__Authority="https://api.example.com" \
  -e AuthServer__RequireHttpsMetadata=true \
  -e AuthServer__CertificatePassPhrase="<CERTIFICATE_PASSWORD>" \
  -e StringEncryption__DefaultPassPhrase="<STRONG_RANDOM_SECRET>" \
  supplycore-backend:<VERSION>
```

The public production backend should be served through HTTPS by a reverse proxy
or cloud load balancer.

### 6.5 OpenIddict Certificate

Do not use a development certificate in production.

Production requires a protected `openiddict.pfx` certificate and a matching
`AuthServer__CertificatePassPhrase`. Store the certificate and passphrase in the
deployment platform's secret storage. Mount or copy the certificate into the
backend container according to the selected hosting platform.

### 6.6 Angular Frontend

The Angular Dockerfile builds the frontend and serves it with Nginx. Runtime
environment values are provided through `dynamic-env.json`.

Build the frontend image:

```bash
docker build -f angular/Dockerfile -t supplycore-frontend:<VERSION> angular
```

For production, provide a `dynamic-env.json` matching the deployed domains:

```json
{
  "production": true,
  "application": {
    "baseUrl": "https://app.example.com",
    "name": "SupplyCoreERP"
  },
  "oAuthConfig": {
    "issuer": "https://api.example.com/",
    "redirectUri": "https://app.example.com",
    "clientId": "SupplyCoreERP_App",
    "responseType": "code",
    "scope": "offline_access SupplyCoreERP",
    "requireHttps": true
  },
  "apis": {
    "default": {
      "url": "https://api.example.com",
      "rootNamespace": "SupplyCoreERP"
    },
    "AbpAccountPublic": {
      "url": "https://api.example.com/",
      "rootNamespace": "AbpAccountPublic"
    }
  }
}
```

Run the frontend:

```bash
docker run -d \
  --name supplycore-frontend \
  -p 4200:80 \
  -v /path/to/production-dynamic-env.json:/usr/share/nginx/html/dynamic-env.json \
  supplycore-frontend:<VERSION>
```

In production, expose the frontend through HTTPS on the final application
domain, not directly through port `4200`.

### 6.7 MCP Server

Build the MCP server image:

```bash
docker build -f mcp-server/Dockerfile -t supplycore-mcp-server:<VERSION> .
```

Run the MCP server:

```bash
docker run -d \
  --name supplycore-mcp-server \
  -p 3000:3000 \
  -e NODE_ENV=production \
  -e PORT=3000 \
  -e DATABASE_URL="postgresql://<USER>:<PASSWORD>@<HOST>:<PORT>/<DATABASE>" \
  -e ALLOWED_ORIGINS="https://app.example.com" \
  supplycore-mcp-server:<VERSION>
```

Expose the MCP server only if the production architecture requires external
access. Otherwise, keep it on a private network.

### 6.8 Reverse Proxy and TLS

Use a reverse proxy or cloud ingress to terminate HTTPS:

- `https://app.example.com` -> frontend container port `80`
- `https://api.example.com` -> backend container port `8080`
- Optional MCP endpoint -> MCP container port `3000`

Update backend and frontend settings so OAuth issuer, redirect URLs, CORS
origins, and API URLs all use the final HTTPS domains.

### 6.9 CI/CD Reference

If deployment is performed through a pipeline, follow the repository-level
workflow guide:

```text
GIT_WORKFLOW_CICD.md
```

That guide should define branch strategy, pull request rules, build checks,
image publishing, deployment order, secrets handling, and rollback.

## 7. Verification Commands

Backend:

```bash
dotnet build SupplyCoreERP.slnx
dotnet test SupplyCoreERP.slnx
```

Frontend:

```bash
cd angular
npm run lint
npm test
npm run build:prod
```

MCP server:

```bash
cd mcp-server
npm run build
```

Docker local smoke test:

```bash
docker compose up --build
```

Then verify:

- Frontend loads successfully.
- Login works with the expected account.
- Swagger is reachable.
- Database migrations completed.
- MCP server starts without database connection errors.

## 8. Production Security Checklist

Before go-live:

- Replace all development database credentials.
- Remove secrets from committed configuration files.
- Use production PostgreSQL credentials with least privilege.
- Use HTTPS for all public endpoints.
- Set `AuthServer__RequireHttpsMetadata=true`.
- Set Angular `oAuthConfig.requireHttps=true`.
- Replace the development OpenIddict certificate.
- Replace `StringEncryption__DefaultPassPhrase` with a strong random secret.
- Restrict `App__CorsOrigins` to the production frontend domain.
- Restrict `App__RedirectAllowedUrls` to approved production redirect URLs.
- Restrict `ALLOWED_ORIGINS` for the MCP server.
- Store secrets in the deployment platform or CI/CD secret manager.
- Back up the database before running production migrations.
- Keep previous container images available for rollback.

## 9. Rollback Notes

For application-only failures:

1. Stop the failed container version.
2. Start the previous known-good image.
3. Verify frontend login and key API workflows.

For database migration failures:

1. Stop new application containers.
2. Restore the latest verified database backup when required.
3. Redeploy the previous known-good application version.
4. Investigate and fix the failed migration before attempting deployment again.

Do not run destructive database rollback steps without a verified backup and a
clear rollback plan.
