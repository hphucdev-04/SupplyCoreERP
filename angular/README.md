# SupplyCoreERP Frontend Guide

This document describes the Angular frontend structure, runtime configuration,
local development commands, build/test commands, and Docker/Nginx notes. Clone,
full local startup, and production deployment steps are documented in
[`../CUSTOMER-HANDOVER-GUIDE.md`](../CUSTOMER-HANDOVER-GUIDE.md).

## Frontend Solution Tree

```text
angular/
+-- src/
|   +-- app/
|   |   +-- catalogs/
|   |   +-- common/
|   |   +-- home/
|   |   +-- inventories/
|   |   +-- partners/
|   |   +-- procurement/
|   |   +-- proxy/
|   |   |   +-- active-ingredients/
|   |   |   +-- agent/
|   |   |   +-- balances/
|   |   |   +-- base-units/
|   |   |   +-- batches/
|   |   |   +-- categories/
|   |   |   +-- customers/
|   |   |   +-- dashboard/
|   |   |   +-- dosage-forms/
|   |   |   +-- locations/
|   |   |   +-- manufacturers/
|   |   |   +-- medicines/
|   |   |   +-- notifications/
|   |   |   +-- prices/
|   |   |   +-- purchase-orders/
|   |   |   +-- purchase-requisitions/
|   |   |   +-- purchase-return-requests/
|   |   |   +-- purchase-returns/
|   |   |   +-- sales-orders/
|   |   |   +-- sales-recalls/
|   |   |   +-- settings/
|   |   |   +-- suppliers/
|   |   |   +-- system/
|   |   |   +-- tickets/
|   |   |   +-- transactions/
|   |   |   +-- warehouses/
|   |   |   +-- generate-proxy.json
|   |   |   +-- index.ts
|   |   |   +-- README.md
|   |   +-- sales/
|   |   +-- settings/
|   |   +-- shared/
|   |   |   +-- components/
|   |   |   +-- directives/
|   |   |   +-- models/
|   |   |   +-- services/
|   |   |   +-- untils/
|   |   |   +-- shared.module.ts
|   |   +-- app.component.ts
|   |   +-- app.config.ts
|   |   +-- app.routes.ts
|   +-- assets/
|   +-- environments/
|   |   +-- environment.ts
|   |   +-- environment.prod.ts
|   +-- favicon.ico
|   +-- index.html
|   +-- main.ts
|   +-- polyfills.ts
|   +-- styles.scss
|   +-- test.ts
+-- scripts/
+-- e2e/
+-- angular.json
+-- Dockerfile
+-- Dockerfile.local
+-- dynamic-env.json
+-- dynamic-env.docker.json
+-- nginx.conf
+-- package.json
+-- tsconfig.app.json
+-- tsconfig.json
+-- tsconfig.spec.json
+-- vercel.json
+-- web.config
```

## Technology Stack

- Angular 20
- TypeScript 5.8
- ABP Angular packages `~10.0.1`
- LeptonX theme packages
- RxJS
- SignalR client
- ApexCharts / ng-apexcharts
- Karma/Jasmine for unit tests
- Angular ESLint
- Nginx for containerized static hosting

## Application Structure

| Path | Purpose |
| --- | --- |
| `src/app/catalogs` | Catalog-facing pages and feature code. |
| `src/app/inventories` | Inventory feature pages and workflows. |
| `src/app/partners` | Partner/customer/supplier-facing feature code. |
| `src/app/procurement` | Procurement feature pages and workflows. |
| `src/app/sales` | Sales feature pages and workflows. |
| `src/app/settings` | Settings UI and configuration screens. |
| `src/app/shared` | Shared components, directives, models, and services. |
| `src/app/proxy` | Generated ABP API proxies. Do not manually edit generated proxy files. |
| `src/environments` | Build-time Angular environment files. |
| `dynamic-env.json` | Runtime environment file copied into production builds. |
| `dynamic-env.docker.json` | Docker Compose runtime environment file mounted as `dynamic-env.json`. |
| `nginx.conf` | Nginx SPA routing and `/getEnvConfig` runtime config endpoint. |

## Runtime Configuration

The frontend uses two configuration paths:

- Build-time Angular files: `src/environments/environment.ts` and `src/environments/environment.prod.ts`.
- Runtime static JSON: `dynamic-env.json`, served by Nginx through `/getEnvConfig`.

Local development defaults:

- Frontend: `http://localhost:4200`
- Backend issuer/API: `https://localhost:44367`
- OAuth client: `SupplyCoreERP_App`

Docker Compose defaults are stored in `dynamic-env.docker.json`:

- Frontend: `http://localhost:4200`
- Backend issuer/API: `http://localhost:8080`
- OAuth client: `SupplyCoreERP_App`
- `requireHttps`: `false` for local Docker

Production values must use HTTPS domains and must match backend settings:

- `application.baseUrl`
- `oAuthConfig.issuer`
- `oAuthConfig.redirectUri`
- `apis.default.url`
- `apis.AbpAccountPublic.url`
- Backend `App:AngularUrl`
- Backend `App:CorsOrigins`
- Backend `App:RedirectAllowedUrls`
- Backend `AuthServer:Authority`

## Install Dependencies

From `angular/`:

```bash
npm install
```

The repository also contains `yarn.lock`; keep package manager usage consistent
inside a branch to avoid lockfile churn.

## Run Locally

Start the Angular development server:

```bash
npm start
```

The app runs at:

```text
http://localhost:4200
```

The backend must be running and reachable at the URL configured in
`src/environments/environment.ts`.

## Build

Production build:

```bash
npm run build:prod
```

Default Angular build:

```bash
npm run build
```

The build output path is:

```text
dist/SupplyCoreERP
```

## Test and Lint

Run unit tests:

```bash
npm test
```

Run lint:

```bash
npm run lint
```

## API Proxies

Generated proxies live under:

```text
src/app/proxy
```

Treat proxy files as generated code. Regenerate them from the backend API when
backend application contracts change, then review the generated diff before
committing.

## Docker and Nginx

The production container build uses:

```text
Dockerfile
```

It performs these steps:

1. Installs frontend dependencies.
2. Runs `yarn build:prod`.
3. Copies `dist/SupplyCoreERP` into an Nginx image.
4. Copies `dynamic-env.json` and `nginx.conf` into the Nginx image.

The Nginx config:

- Serves the Angular SPA from `/usr/share/nginx/html`.
- Falls back to `index.html` for client-side routes.
- Exposes `/getEnvConfig` to return `dynamic-env.json`.

Docker Compose mounts `dynamic-env.docker.json` over the container's
`dynamic-env.json` for local container runs.

## Frontend Development Checklist

- Use SCSS for Angular components.
- Keep the Angular component prefix as `app`.
- Put shared UI and utility code under `src/app/shared`.
- Keep generated API proxies under `src/app/proxy`.
- Update runtime environment values when backend URLs or OAuth settings change.
- Run `npm run lint` and `npm run build:prod` before frontend PRs.
- Run `npm test` when changing tested behavior or shared frontend logic.
