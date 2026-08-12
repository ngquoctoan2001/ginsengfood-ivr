# GinsengFood IVR

Standalone .NET 10 service for IVR order confirmation. This P0-1 bootstrap has
health probes, an empty worker, an empty EF Core PostgreSQL context, and a
Next.js admin placeholder. It contains no order-confirmation business logic and
does not connect to the Java sales platform, a SIM, or a customer.

## Safety baseline

Local development is mock-only:

```text
IVR_EXECUTION_MODE=MOCK
SALES_PROVIDER=FAKE_TARGET_V1
SIM_PROVIDER=MOCK
REAL_CUSTOMER_CALL_ALLOWED=NO
ConnectionStrings__IvrDb=Host=localhost;Port=55433;Database=ivr;Username=ivr
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

- `Ivr.Api`: `/health/live`, `/health/ready`, and `/health/startup`.
- `Ivr.Worker`: mock heartbeat every 30 seconds.
- `Ivr.Infrastructure`: empty `IvrDbContext`; migrations begin in P1-2.
- `Ivr.Domain`: empty business boundary.
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

Expected test IDs are `UT-BOOT-01`, `IT-BOOT-02`, and `UT-BOOT-03`. CI is
introduced by P0-2 using GitLab CI; P0-1 is verified locally only.
