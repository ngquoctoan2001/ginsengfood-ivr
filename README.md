# GinsengFood IVR

[![GitLab pipeline](https://img.shields.io/badge/GitLab_pipeline-runs%2C_not_green-orange)](deploy/ci/README.md#gitlab-project-settings--hosted-evidence)

Standalone .NET 10 service for IVR order confirmation. The repository now
contains the order-confirmation workflow, PostgreSQL persistence, background
dispatch/callback/retention jobs and fail-closed runtime gates. The operations
console belongs to Module 3 and is not in this repository (see the safety
baseline below). Local development remains MOCK/fake by default; connection
to the real Sales sandbox, a carrier/SIM provider, or a real customer still
requires the separately governed external gates.

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
Ivr.Api ---------> Ivr.Infrastructure ---------> Ivr.Domain
   |                         ^
   +----> Ivr.Contracts     |
                             |
Ivr.Worker -----------------+
   |
   +----> Ivr.Contracts
```

- `Ivr.Api`: health probes plus reusable correlation, stable error envelope,
  three-tier service-token enforcement, the Order Core service allowlist, and the feature-
  flag read/admin endpoints.
- `Ivr.Worker`: scheduler/dispatcher plus callback delivery, retention,
  analytics and lifecycle jobs; real dispatch remains behind runtime gates.
- `Ivr.Infrastructure`: EF/PostgreSQL repositories, append-only audit/evidence,
  idempotency, speech/telephony adapters, typed dynamic config, audited
  feature-flag mutations, and centralized dispatch/kill gates.
- `Ivr.Domain`: stable error catalog and PII masking/guard primitives.
- `Ivr.Contracts`: generated IVR DTOs and Sales Target V1 client plus a separate
  pinned Golden Hour current-compat client; see `docs/contracts/openapi-codegen.md`.

There is no operator console in this repository. `W-0128` removed it from the
deployable topology and `W-0253` deleted the directory: Module 3 owns and builds
the console, on its own identity. The Vietnamese labels a console needs for every
enum on this API live in `specs/ui/enum-labels.vi.json`, next to the screen specs
in `specs/ui/`.

`/health/ready` is a fail-closed dependency-readiness probe: it returns `503`
when PostgreSQL is unreachable or the schema is behind. W-0040 implemented this
behavior; it is no longer a bootstrap probe. Its `sales_callback` check reads
the callback circuit only in a host that delivers callbacks, and the API does
not (delivery runs in the worker), so on the API that check always reports
`not_configured`.

## Prerequisites

- .NET SDK 10
- Node.js 20 or newer and npm
- Docker Engine with Compose

## Run locally

The root `package.json` provides the canonical local commands. On first use:

```powershell
pnpm setup
```

Prepare PostgreSQL and apply every pending migration. This also stops the two
Docker app containers so a host worker is never competing with a containerized
worker for the same database:

```powershell
pnpm local:prepare
```

To prepare the database, start a temporary Development API, load all ten task
fixtures and verify `SCN-001-confirm` in one repeatable command, run:

```powershell
pnpm dev:bootstrap
```

The command validates the three required seed files before starting Docker. It
uses only the documented fake Development credentials, pins execution to
`MOCK / MOCK / NO`, starts no worker, and chooses a free loopback port rather
than competing with an API already running on port 5005. Nine fixtures become
dry-run-only jobs; `TASK-TARGET-247-0005` remains safely blocked by its
`call_restriction`. Re-running the command reports the existing jobs instead of
duplicating them. Logs are written under `ci-artifacts/dev-bootstrap/`.

Then use two PowerShell terminals:

```powershell
pnpm api:dev
pnpm worker
```

The API is available at `http://127.0.0.1:5005`, and PostgreSQL at
`127.0.0.1:55433`.
The worker's standalone HTTP health listener is disabled only in Development
because Windows `HttpListener` requires a machine-level URLACL; container and
deployment health configuration is unchanged.

Database helpers:

```powershell
pnpm db:migration:list
pnpm db:migration:add -- W0106ExampleChange
pnpm db:migrate
```

### Ports

IVR runs entirely inside its own range so it can share a machine with
`ginsengfood-ops-core` (backend `5000`, frontend `3000`) without either side
being stopped.

| Component | Port | Pinned in |
| --- | --- | --- |
| `Ivr.Api` | `5005` | `src/Ivr.Api/Properties/launchSettings.json` |
| PostgreSQL | `55433` | `docker-compose.dev.yml` (`IVR_POSTGRES_PORT`) |

Keep these distinct from any other local service.

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

The fake Sales, mock SIM, and mock JWT containers are inert placeholders. To
inspect them locally, run `docker compose -f docker-compose.dev.yml --profile
mocks up -d`; they do not expose ports and their Compose network is internal.
Postgres alone uses a local bridge so the host-run API can reach its
localhost-only published port.

### Run one whole call lifecycle

The worker ships with scheduler, normalisation, callback delivery and MOCK
telephony all disabled, so a plain `pnpm api:dev` + `pnpm worker` accepts tasks
and then does nothing with them. That is the correct default — a stack started
to look at the console must not dial anything — but it means switching the
pipeline on takes a dozen environment variables that used to be re-derived by
hand every time.

```powershell
pnpm e2e:local
```

W-0190 wrote that configuration down. The command starts PostgreSQL, applies
migrations, starts a fake Sales endpoint, runs the API and worker with MOCK
telephony armed, admits five tasks and asserts the result taxonomy each one must
produce — including that a technical exception is never counted as a customer
attempt and that only a final result reaches the callback outbox. It stops
everything it started; `pnpm e2e:local:keep` leaves it up for poking at the
console. Process logs land in `ci-artifacts/local-e2e/`.

`IVR_EXECUTION_MODE`, `SIM_PROVIDER` and `REAL_CUSTOMER_CALL_ALLOWED` are not
relaxed by it. `MockSchedulerDispatchGateway.IsReady` requires all three, so the
run cannot reach a vendor; the kill switch is the only safety lifted, and it is
lifted against a fake gateway.

### Non-production developer surface

`Ivr:DevTooling:SeedDirectory` points the UI-07 seed loader, scenario runner and
integration-status profiles at the repository's `seed/` folder. Development
configuration sets it to `../../seed`, resolved against the content root rather
than the working directory, so the same value holds for `dotnet run`,
`dotnet test` and the container image. It is deliberately **empty** everywhere
else: an unset seed directory disables the surface, and those routes are not
mapped at all in production.

## Verify

```powershell
dotnet restore Ivr.sln
dotnet build Ivr.sln --no-restore
dotnet test Ivr.sln --no-build
docker compose -f docker-compose.dev.yml config --quiet
```

Foundation test IDs include `UT-BOOT-01`, `IT-BOOT-02`, `UT-BOOT-03`, and the
`UT-FND-*` suite for configuration, idempotency, tier authorization, service
allowlisting, error envelopes, audit, and PII. CI is implemented by P0-2 using
GitLab CI. The hosted pipeline has run on the project's own runner since W-0292
(2026-09-14) but has not been green: W-0121 records why, and
`docs/evidence/W-0354/hosted-ci-diagnosis.json` the jobs that failed. Protected
branch and merge checks still wait on W-0061; see [the CI
runbook](deploy/ci/README.md).
