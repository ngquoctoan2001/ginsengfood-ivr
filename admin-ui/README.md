# IVR Admin UI — reference implementation

`admin-ui` is a local Next.js reference for the Module 3 operations console. W-0128 makes the
boundary explicit: Module 3 owns operator accounts, sign-in, roles and the deployed UI; IVR owns
the functional admin API and its three service-token tiers. This directory is not an IVR
deployable and contains no account/session authority.

## Screens

| Route | Purpose |
| --- | --- |
| `/dashboard` | Result rates, queue, attempts, SIM pool and queue controls |
| `/calls` / `/calls/[id]` | Masked call-job list and lifecycle detail |
| `/reports` | Aggregate KPI, trend, breakdown and audited export |
| `/review` | Human review queue |
| `/config` | Script catalogue and lifecycle controls |
| `/integration` | Dependency state and fail-closed events |
| `/seed` | Non-production developer tools |

There is no `/login`, `/profile`, `/accounts` or `/roles` route. Order Core remains the only
owner of order state; no screen may force confirm/cancel or bypass do-not-call policy.

## Trust boundary

- The browser never receives an IVR service token and never calls `Ivr.Api` directly.
- Next.js route handlers select the lowest required tier and attach the server-only token.
- Every table/admin operation sends `X-Actor-Id`, copied from Module 3's stable operator subject.
- `danger` operations additionally require `X-Action-Reason`.
- Client-side visibility is presentation only. Ivr.Api enforces tier, actor, reason and business
  invariants again.

Module 3 must implement the same boundary in its own BFF. It must not copy these placeholder
credentials into browser-visible `NEXT_PUBLIC_*` variables, HTML, JavaScript bundles or cookies.

## Role to tier mapping

The definitive Module 3 role catalogue is external to this repository. Until Module 3 signs off
an explicit mapping, the integration must deny by default. IVR supplies the capability classes:

| Tier | Intended capability | Mapping status |
| --- | --- | --- |
| `read` | dashboards, queue, reports, scripts and SIM status | `M3_OWNER_DECISION_REQUIRED` |
| `write` | admin reviews, script lifecycle and non-prod tools | `M3_OWNER_DECISION_REQUIRED` |
| `danger` | pause/resume, terminate, retry, SIM mutation, runtime flags | `M3_OWNER_DECISION_REQUIRED`; never granted as an implicit default |

See [`specs/ui/08-role-permission-ui.md`](../specs/ui/08-role-permission-ui.md) and
[`integration-requirements/06-module-3-api-handover.md`](../integration-requirements/06-module-3-api-handover.md) §4A.

## Local configuration

Copy `.env.example` to `.env.local`. All token variables are server-only.

| Variable | Purpose |
| --- | --- |
| `IVR_API_BASE_URL` | Ivr.Api origin, default `http://127.0.0.1:5005` |
| `IVR_ADMIN_READ_TOKEN` | Local read-tier credential |
| `IVR_ADMIN_WRITE_TOKEN` | Local write-tier credential |
| `IVR_ADMIN_DANGER_TOKEN` | Local danger-tier credential |
| `IVR_ADMIN_ACTOR_ID` | Local stable actor reference sent as `X-Actor-Id` |
| `IVR_EXECUTION_MODE` | Runtime mode shown in governance UI |
| `REAL_CUSTOMER_CALL_ALLOWED` | Governance banner; anything but `YES` is off |
| `IVR_ENVIRONMENT_LABEL` | Environment badge |

Production custody and rotation happen in the deployment secret store. During rotation, Ivr.Api
accepts a distinct `*_PREVIOUS` value only until its absolute `*_PREVIOUS_RETIRES_AT` instant.

## Local commands

```powershell
npm --prefix admin-ui run dev
npm --prefix admin-ui run lint
npm --prefix admin-ui run typecheck
npm --prefix admin-ui test
npm --prefix admin-ui run build
```

The reference UI listens on port `3005` and expects Ivr.Api on `5005`. It is intentionally absent
from Docker Compose and disabled in the Helm chart.
