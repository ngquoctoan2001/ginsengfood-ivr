# W-0010 — P0-1 Repo & Solution Bootstrap Evidence

Date: 2026-08-12

Baseline: `main@5c6f39e892b2c6d85c56065f8c10dcdba2ba8770`

Execution mode: `MOCK`

Real customer calls: `NO`

Final acceptance: `ACCEPTED` on 2026-08-12 by Codex self-review under explicit
IVR owner authorization. This acceptance is limited to the P0-1 local bootstrap
gate and does not approve real integration, telephony, or production release.

## Scope verified

- .NET 10 solution with five source projects and three xUnit projects.
- One-way source references: API/Worker to Infrastructure to Domain; Domain and
  Contracts have no project references.
- Empty EF Core/Npgsql `IvrDbContext`; no entity or migration.
- Health endpoints for live, ready, and startup.
- Worker heartbeat interval of 30 seconds.
- Strict TypeScript Next.js App Router placeholder.
- PostgreSQL 16 local Compose service and inert provider placeholders.

## Build and test results

| Command | Exact result |
| --- | --- |
| `dotnet restore Ivr.sln` | PASS; 8/8 projects restored |
| `dotnet build Ivr.sln --no-restore` | PASS; 0 warnings, 0 errors |
| `dotnet test Ivr.sln --no-build --logger "console;verbosity=normal"` | PASS; 3 tests total: Ivr.UnitTests 2/2, Ivr.IntegrationTests 1/1 |
| `dotnet format Ivr.sln --no-restore --verify-no-changes --verbosity diagnostic` | PASS; formatted 0 of 39 files; code-style and analyzer passes completed |
| `npm --prefix admin-ui run lint` | PASS; ESLint exit 0 |
| `npm --prefix admin-ui run build` | PASS; Next.js 16.3.0 production build and strict TypeScript pass; `/` prerendered static; no warning on final run |
| `npm --prefix admin-ui audit --audit-level=high` | PASS; 0 vulnerabilities |
| `docker compose -f docker-compose.dev.yml config --quiet` | PASS |
| official `markdown-doc-reader` mapper | PASS; 388 Markdown files, 368 links resolved, 0 unresolved |
| `node .gitnexus/run.cjs status` | PASS; index at `main@5c6f39e`, up to date |
| `node .gitnexus/run.cjs detect-changes --scope all --repo ivr` | PASS; LOW risk, 0 affected process |
| `node .gitnexus/run.cjs detect-changes --scope staged --repo ivr` | PASS; 59 files mapped to 79 symbols, LOW risk, 0 affected process |
| `node .gitnexus/run.cjs check --cycles --repo ivr` | PASS; no circular imports |
| `git diff --check` | PASS |

The three implemented test IDs are:

- `UT-BOOT-01`: canonical keys bind to `IvrOptions`.
- `IT-BOOT-02`: all three bootstrap health endpoints return JSON and HTTP 200.
- `UT-BOOT-03`: `Ivr.Domain` does not reference `Ivr.Infrastructure`.

`Ivr.ContractTests` intentionally has no test case in P0-1. Its empty project
builds successfully; generated OpenAPI contracts and their tests begin in P1-1.

## Runtime results

The machine already had PostgreSQL on `5432` and project containers on `55431`
and `55432`. IVR therefore uses its own localhost-only port `55433`.

```text
docker compose -f docker-compose.dev.yml up -d postgres
ginsengfood-ivr-dev-postgres-1: Up (healthy)
127.0.0.1:55433 -> 5432/tcp
pg_isready: accepting connections
Port55433Reachable=True
```

API executed with all canonical mock variables and the IVR-local connection
string:

```text
GET /health/live -> 200 {"status":"Healthy","probe":"live"}
GET /health/ready -> 200 {"status":"Healthy","probe":"ready","dependencyChecks":"NOT_IMPLEMENTED_UNTIL_W-0040"}
GET /health/startup -> 200 {"status":"Healthy","probe":"startup"}
```

`/health/ready` is deliberately not dependency-aware in P0-1. It must not be
used as fail-closed readiness evidence before W-0040.

The final runtime check used the committed launch profile URL
`http://127.0.0.1:5088`. After evidence capture, the verified workspace API PID
was stopped and port `5088` was confirmed free. The IVR Postgres container was
also stopped cleanly without deleting its container or named volume; rerun the
README Compose command to resume it.

## UI evidence

Browser verification against the production Next.js build confirmed:

- heading `IVR Admin — MOCK mode`;
- execution `MOCK`;
- Sales provider `FAKE_TARGET_V1`;
- real calls `DISABLED`;
- no browser console warning or error.

Screenshot artifact: `docs/evidence/W-0010/admin-ui-mock-mode.png`

## Residual gates

- P0-1 local bootstrap review: `ACCEPTED`.
- GitLab CI/hosted pipeline: `NOT_RUN`; owned by W-0011/P0-2.
- Sales API, authentication, SIM/telephony, real customer data: `NOT_RUN` and
  intentionally absent.
- Lab SIM, 32 eSIM, real integration, staging, production: `NOT_RUN`.
- GitNexus index and staged change review passed before the P0-1 commit. Its graph is
  advisory; build, tests, runtime probes, and direct source review remain the
  acceptance evidence.
