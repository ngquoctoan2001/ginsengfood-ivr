# GinsengFood IVR

[![GitLab pipeline](https://img.shields.io/badge/GitLab_pipeline-NOT_RUN-lightgrey)](deploy/ci/README.md#gitlab-project-settings--hosted-evidence)

Standalone .NET 10 service for IVR order confirmation. The foundation includes
health probes, cross-cutting security and traceability primitives, an empty
worker, an empty EF Core PostgreSQL context, and a Next.js admin placeholder.
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

`ORDER_CORE_SERVICE_TOKEN` is required by the API allowlist and must be injected
through the process environment or a secret provider. It is deliberately absent
from `appsettings*.json` and tracked files. For a local MOCK process, set a
disposable value without writing it to disk:

```powershell
$env:ORDER_CORE_SERVICE_TOKEN = Read-Host "Local MOCK Order Core token"
```

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
  permission enforcement, and the Order Core service allowlist.
- `Ivr.Worker`: mock heartbeat every 30 seconds.
- `Ivr.Infrastructure`: in-memory MOCK idempotency, append-only audit and
  evidence stores; the empty `IvrDbContext` receives migrations in P1-2.
- `Ivr.Domain`: stable error catalog and PII masking/guard primitives.
- `Ivr.Contracts`: reserved for the P1-1 generated OpenAPI client and DTOs.
- `admin-ui`: strict TypeScript App Router placeholder; authentication begins
  in P3-1.

`/health/ready` always returns HTTP 200 in P0-1. It is only a bootstrap
placeholder and is not a fail-closed dependency-readiness signal until W-0040.

## Prerequisites

- .NET SDK 10
- Node.js 20 or newer and npm
- Docker Engine with Compose

## Run locally

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
Invoke-WebRequest http://127.0.0.1:5088/health/live
Invoke-WebRequest http://127.0.0.1:5088/health/ready
Invoke-WebRequest http://127.0.0.1:5088/health/startup
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
