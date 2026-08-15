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
| `/review` | P3-3 | Human review queue, linking into call detail |
| `/config` | P3-3 | Script versions, approval matrix, DTMF map, variable whitelist — read-only |
| `/integration` | P3-3 | Dependency status and recent fail-closed events — view only |
| `/seed` | P3-3 | Adapter mode and test profiles — read-only, locked outside non-prod |
| `/roles` | P3-3 | Role and permission reference matrix |

The back-office screens are read-only on purpose. Script approval is an owner
decision (`OD-V1-15`), permission assignment belongs to Permission Core (DF-01),
and no seed write path is exposed from a browser. Dependency cards that IVR
cannot probe are shown as `NOT_WIRED` rather than healthy until `P6-1`
(`W-0040`) delivers real probing.

## Boundaries

- **The browser never talks to Ivr.Api.** Every call goes through this Next.js
  server, which holds the credentials and the API base URL. The browser holds
  only an httpOnly session cookie (`specs/ui/08-role-permission-ui.md` §4).
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

`IVR_EXECUTION_MODE=MOCK` exposes the sign-in directory seeded from
`seed/agents.sample.json` — three roles, no password, no real identity. Any
other mode refuses to mint a session: platform SSO/JWT is gate `G-AUTH`
(`W-0006`), still `BLOCKED_EXTERNAL`.

The session is an HMAC-SHA256 signed cookie: `httpOnly`, `SameSite=Strict`,
`Secure` outside development, 8-hour lifetime. Sign-in and sign-out are Route
Handlers reached by plain form posts, so both work without JavaScript and both
reject cross-site requests.

## Configuration

Copy `.env.example` to `.env.local`. All variables are read on the server only.

| Variable | Purpose |
| --- | --- |
| `IVR_API_BASE_URL` | Ivr.Api origin (default `http://127.0.0.1:5005`) |
| `IVR_EXECUTION_MODE` | Canonical mode key; only `MOCK` enables mock sign-in |
| `IVR_ADMIN_UI_SESSION_SECRET` | Session HMAC key, ≥ 32 characters, required |
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
