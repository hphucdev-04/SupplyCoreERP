# SupplyCoreERP

SupplyCoreERP is an ABP Framework layered monolith for supply, inventory, and
business operation workflows. The solution combines an ASP.NET Core backend,
Angular frontend, PostgreSQL database, and a TypeScript MCP server.

This root README is intentionally high level. Detailed setup, development, and
deployment instructions are kept in the linked documents below.

## Architecture Overview

- Backend: ASP.NET Core / .NET 10 with ABP layered architecture.
- Frontend: Angular 20 with ABP Angular modules and LeptonX theme.
- Database: PostgreSQL with EF Core migrations.
- Authentication: OpenIddict through ABP.
- MCP: TypeScript MCP server connected to PostgreSQL.
- Local orchestration: Docker Compose.

## Repository Layout

| Path | Purpose |
| --- | --- |
| `src/` | Backend source code, including Domain, Application, EF Core, HttpApi, Host, and DbMigrator projects. |
| `test/` | Backend test projects split by ABP layer. |
| `angular/` | Angular frontend application and generated API proxies. |
| `mcp-server/` | TypeScript MCP server. |

## Documentation

- [Customer Handover and Deployment Guide](./CUSTOMER-HANDOVER-GUIDE.md)
- [Backend Guide](./src/README.md)
- [Frontend Guide](./angular/README.md)
- [Git Workflow and CI/CD](./GIT_WORKFLOW_CICD.md)
- [License](./LICENSE)

## Common Commands

Build and test backend:

```bash
dotnet build SupplyCoreERP.slnx
dotnet test SupplyCoreERP.slnx
```

Run database migrations and seed data:

```bash
dotnet run --project src/SupplyCoreERP.DbMigrator
```

Run backend API:

```bash
dotnet run --project src/SupplyCoreERP.HttpApi.Host
```

Run frontend:

```bash
cd angular
npm install
npm start
```

Build frontend:

```bash
cd angular
npm run build:prod
```

Build MCP server:

```bash
cd mcp-server
npm install
npm run build
```

Run the full local stack with Docker:

```bash
docker compose up --build
```

## Local Endpoints

- Frontend: `http://localhost:4200`
- Backend Swagger: `http://localhost:8080/swagger`
- MCP server: `http://localhost:3000`
- PostgreSQL: `http://localhost:5432`

## Production Endpoints

- Frontend: `https://rxlogistics.vercel.app`
- Backend: `https://rxlogistics.up.railway.app`
- MCP server: `https://rxlogistics-mcp.up.railway.app/mcp`

## Security Notes

Do not commit production secrets, database passwords, certificates, or
environment-specific credentials. Before deployment, replace development
connection strings, OpenIddict certificates, encryption passphrases, CORS
origins, OAuth redirect URLs, and MCP allowed origins.

See [Customer Handover and Deployment Guide](./CUSTOMER-HANDOVER-GUIDE.md) for
the production checklist.
