# GinsengFood IVR

[![GitLab pipeline](https://img.shields.io/badge/GitLab_pipeline-NOT_RUN-lightgrey)](deploy/ci/README.md#gitlab-project-settings--hosted-evidence)

Standalone .NET 10 service for IVR order confirmation. The foundation includes
health probes, cross-cutting security and traceability primitives, typed
feature flags with fail-closed kill switches, an empty worker, an EF Core
feature-flag model, and a Next.js admin placeholder.
It contains no order-confirmation business logic and does not connect to the
Java sales platform, a SIM, or a customer.

## Safety baseline

Local development is mock-only:

```text
IVR_EXECUTION_MODE=MOCK
SALES_PROVIDER=FAKE_TARGET_V1
SIM_PROVIDER=MOCK
REAL_CUSTOMER_CALL_ALLOWED=NO
ConnectionStrings__IvrDb=Host=localhost;Port=55433;Database=ivr;Username=ivr
```

`ORDER_CORE_SERVICE_TOKEN` is required by the API allowlist. The Development
configuration contains an obvious fake local-only value so `pnpm api:dev` does
not require repeatedly setting an environment variable. Deployed environments
must override it through an environment variable or secret provider.

`IVR_INTERNAL_SERVICE_TOKEN` is independently required at startup for the six
IVR-owned worker/adapter lifecycle endpoints. Development also contains a
separate fake local-only value for this token. Non-Development startup remains
fail-closed unless the deployment injects both real values. Never promote the
Development values to staging or production.

Do not introduce real provider credentials, customer data, shared Java entities,
or access to the sales platform database. The Postgres `trust` configuration in
`docker-compose.dev.yml` is restricted to localhost development and must never
be copied to a deployed environment.

## Components

```text
admin-ui (Next.js)

Ivr.Api ---------> Ivr.Infrastructure ---------> Ivr.Domain
   |                         ^
   +----> Ivr.Contracts     |
                             |
Ivr.Worker -----------------+
   |
   +----> Ivr.Contracts
```

- `Ivr.Api`: health probes plus reusable correlation, stable error envelope,
  permission enforcement, the Order Core service allowlist, and the feature-
  flag read/admin endpoints.
- `Ivr.Worker`: mock heartbeat every 30 seconds.
- `Ivr.Infrastructure`: in-memory MOCK idempotency, append-only audit and
  evidence stores; typed dynamic config, audited feature-flag mutations,
  centralized dispatch/kill gates, and the `ivr_feature_flags` EF model. P1-2
  still owns the physical migration and persistent command transaction.
- `Ivr.Domain`: stable error catalog and PII masking/guard primitives.
- `Ivr.Contracts`: generated IVR DTOs and Sales Target V1 client plus a separate
  pinned Golden Hour current-compat client; see `docs/contracts/openapi-codegen.md`.
- `admin-ui`: strict TypeScript App Router console. Session/RBAC/i18n foundation
  (P3-1) plus the dashboard, call log and call detail screens (P3-2). The browser
  talks only to the Next.js server, which is the sole caller of `Ivr.Api`.

`/health/ready` always returns HTTP 200 in P0-1. It is only a bootstrap
placeholder and is not a fail-closed dependency-readiness signal until W-0040.

## Prerequisites

- .NET SDK 10
- Node.js 20 or newer and npm
- Docker Engine with Compose

## Run locally

The root `package.json` provides the canonical local commands. On first use:

```powershell
pnpm setup
```

Stop any running `pnpm dev` process before changing/installing frontend
dependencies; Windows locks Next's native SWC binary while the dev server is
running.

Prepare PostgreSQL and apply every pending migration. This also stops the three
Docker app containers so a host worker is never competing with a containerized
worker for the same database:

```powershell
pnpm local:prepare
```

Then use three PowerShell terminals:

```powershell
pnpm dev
pnpm api:dev
pnpm worker
```

The frontend is available at `http://127.0.0.1:3005`, the API at
`http://127.0.0.1:5005`, and PostgreSQL at `127.0.0.1:55433`.
The worker's standalone HTTP health listener is disabled only in Development
because Windows `HttpListener` requires a machine-level URLACL; container and
deployment health configuration is unchanged.

Database helpers:

```powershell
pnpm db:migration:list
pnpm db:migration:add -- W0106ExampleChange
pnpm db:migrate
pnpm db:seed
```

`db:seed` asks for the bootstrap password without echoing it and is idempotent.
Run it after a fresh database volume or whenever the controlled account seed
definition changes. Do not commit that password into source.

### Ports

IVR runs entirely inside its own range so it can share a machine with
`ginsengfood-ops-core` (backend `5000`, frontend `3000`) without either side
being stopped.

| Component | Port | Pinned in |
| --- | --- | --- |
| `Ivr.Api` | `5005` | `src/Ivr.Api/Properties/launchSettings.json` |
| `admin-ui` | `3005` | `admin-ui/package.json` (`dev` and `start`) |
| PostgreSQL | `55433` | `docker-compose.dev.yml` (`IVR_POSTGRES_PORT`) |

Keep these distinct from any other local service. The admin UI test suite binds
an ephemeral port instead, so it never conflicts with a running dev server.

Start the dedicated development database:

```powershell
docker compose -f docker-compose.dev.yml up -d postgres
```

The IVR database uses host port `55433` by default to avoid other local
PostgreSQL instances. Override it with `IVR_POSTGRES_PORT` and update
`ConnectionStrings__IvrDb` to the same value when needed.

Run the API and inspect its probes:

```powershell
dotnet run --project src/Ivr.Api
Invoke-WebRequest http://127.0.0.1:5005/health/live
Invoke-WebRequest http://127.0.0.1:5005/health/ready
Invoke-WebRequest http://127.0.0.1:5005/health/startup
```

Run the worker:

```powershell
dotnet run --project src/Ivr.Worker
```

Run the admin UI:

```powershell
npm --prefix admin-ui run dev
```

The fake Sales, mock SIM, and mock JWT containers are inert placeholders. To
inspect them locally, run `docker compose -f docker-compose.dev.yml --profile
mocks up -d`; they do not expose ports and their Compose network is internal.
Postgres alone uses a local bridge so the host-run API can reach its
localhost-only published port.

## Verify

```powershell
dotnet restore Ivr.sln
dotnet build Ivr.sln --no-restore
dotnet test Ivr.sln --no-build
npm --prefix admin-ui run lint
npm --prefix admin-ui run build
docker compose -f docker-compose.dev.yml config --quiet
```

Foundation test IDs include `UT-BOOT-01`, `IT-BOOT-02`, `UT-BOOT-03`, and the
`UT-FND-*` suite for configuration, idempotency, correlation, RBAC, service
allowlisting, error envelopes, audit, and PII. CI is implemented by P0-2 using
GitLab CI. Until W-0061 provisions the GitLab project, runner, protected branch,
and merge checks, the badge and hosted evidence remain `NOT_RUN`; see [the CI
runbook](deploy/ci/README.md).
