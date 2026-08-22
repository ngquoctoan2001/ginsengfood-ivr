# IVR Admin UI

Next.js App Router console for the GinsengFood IVR administration surface
(`P3-1` / `W-0025`). It provides the foundation the business screens build on:
session handling, RBAC, a type-safe Ivr.Api client, Vietnamese i18n, and a
consistent loading/empty/error vocabulary.

`P3-2` (`W-0026`) adds the operational screens on top of it:

| Route | Prompt | Contents |
| --- | --- | --- |
| `/dashboard` | P3-2 | Result rates, queue panel, attempt panel, SIM pool, open capacity incidents, queue pause/resume |
| `/calls` | P3-2 | Masked, paginated call-job log with filters |
| `/calls/[id]` | P3-2 | Full lifecycle trace, technical retry and admin review |
| `/reports` | P3-4 | Aggregate KPI, trend, breakdown, freshness banner and audited CSV export |
| `/review` | P3-3 | Human review queue, linking into call detail |
| `/config` | P3-3 | Script versions, approval matrix, DTMF map, variable whitelist — read-only |
| `/integration` | P3-3 | Dependency status and recent fail-closed events — view only |
| `/seed` | P3-3 | Adapter mode and test profiles — read-only, locked outside non-prod |
| `/accounts` | W-0105 | Admin-only account list, create, edit, password reset, session revoke and soft-delete |
| `/profile` | W-0105 | Current account profile; available to Admin and Operator |
| `/roles` | W-0105 | API-backed Admin/Operator permission matrix; Admin only |

The configuration, integration and seed screens remain read-only. Script
approval is an owner decision (`OD-V1-15`), and no seed write path is exposed
from a browser. Dependency cards that IVR
cannot probe are shown as `NOT_WIRED` rather than healthy until `P6-1`
(`W-0040`) delivers real probing.

## Boundaries

- **The browser never talks to Ivr.Api.** Every call goes through this Next.js
  server. The browser holds only the opaque API session token in an `httpOnly`,
  `SameSite=Strict` cookie (`specs/ui/08-role-permission-ui.md`).
- **No order transitions.** There is no confirm/cancel control and there will
  not be one: order state belongs to Order Core (D-02). The console can only
  hold and release IVR's own queue, retry technical exceptions, and annotate
  reviews.
- **Masked data only.** `MaskedPhone` refuses to render anything that still
  contains a full number, and no component renders recordings or full addresses
  (D-05).
- **Permissions are hidden client-side, enforced server-side.** `usePermissions`
  and `<RequirePermission>` decide what to paint. Ivr.Api decides what is
  allowed, and answers `403 IVR_FORBIDDEN_CALLER` to anyone who forges the
  difference (DF-01).

## Authentication

The sign-in form accepts username/password in every execution mode and forwards
them server-side to `POST /auth/sign-in`. Ivr.Api stores accounts and opaque
8-hour sessions in PostgreSQL, resolves the current subject through
`GET /auth/session`, and remains the authority for role and permission checks.
The Next.js cookie is `httpOnly`, `SameSite=Strict`, and `Secure` outside
development. Sign-in and sign-out are same-origin Route Handlers; sign-out also
revokes the API session on a best-effort basis. Invalid username, password,
disabled account and lockout deliberately share the same generic 401 response.

Only two roles exist: `Admin` and `Operator`. Operator has exactly
`IVR_QUEUE_VIEW`, `IVR_SIM_DISABLE`, `IVR_MANUAL_RETRY`, and
`IVR_ACCOUNT_SELF_VIEW`; Admin receives the approved operational and account
management permissions. `seed/agents.sample.json` is only a fake RBAC drift
fixture and is not a credential source.

## Configuration

Copy `.env.example` to `.env.local`. All variables are read on the server only.

| Variable | Purpose |
| --- | --- |
| `IVR_API_BASE_URL` | Ivr.Api origin (default `http://127.0.0.1:5005`) |
| `IVR_EXECUTION_MODE` | Canonical runtime mode (`MOCK`, `LAB_REAL`, or governed production mode) |
| `REAL_CUSTOMER_CALL_ALLOWED` | Drives the governance banner; anything but `YES` reads as off |
| `IVR_ENVIRONMENT_LABEL` | Text shown in the environment badge |

## Local commands

The console listens on **3005** and expects Ivr.Api on **5005**. Those ports are
pinned in `package.json` so this repository never collides with
`ginsengfood-ops-core`, which runs its backend on 5000 and its frontend on 3000.

From the repository root:

```powershell
npm --prefix admin-ui run dev
npm --prefix admin-ui run lint
npm --prefix admin-ui run typecheck
npm --prefix admin-ui test
npm --prefix admin-ui run build
```

`npm test` runs unit and component tests plus `E2E-UI-AUTH-05`, which builds the
app and drives a real `next start` server over HTTP. No browser binaries are
needed; browser-level accessibility and visual QA belong to `P5-5` (`W-0039`).

Deployment is not configured here. It is governed by `P7-*`; do not add a Vercel
or GitHub CI path.
