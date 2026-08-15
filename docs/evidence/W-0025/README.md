# W-0025 — P3-1 Admin UI Foundation

| | |
| --- | --- |
| Work ID | `W-0025` · Prompt `P3-1` · Phase 3 |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |
| Prereq status | `W-0012` (P0-3), `W-0018` (P2-1), `W-0065` (P2-8) all `TESTS_PASS` |

## 1. What was built

Next.js App Router foundation under `admin-ui/`. The prompt's §7 paths are
logical; the repository uses the `src/` convention established by P0-1, so
`admin-ui/app/**` lives at `admin-ui/src/app/**` and so on.

| Area | Files |
| --- | --- |
| Shell / routing | `src/app/layout.tsx`, `src/app/page.tsx`, `src/app/(console)/layout.tsx`, `src/app/(console)/queue/**`, `src/app/error.tsx`, `src/app/not-found.tsx` |
| Auth | `src/proxy.ts`, `src/app/login/**`, `src/app/api/auth/sign-in/route.ts`, `src/app/api/auth/sign-out/route.ts`, `src/lib/auth/**` |
| API client | `src/lib/api/{client,admin,errors,types,correlation}.ts` |
| RBAC | `src/lib/rbac/permissions.ts`, `src/components/rbac/**` |
| i18n | `src/i18n/vi.json`, `src/lib/i18n/index.ts` |
| Components | `src/components/{shell,feedback,admin,privacy}/**` |
| Tests | `admin-ui/tests/**`, `vitest.config.mts`, `vitest.setup.ts` |

### Architecture — BFF, not a browser client

`specs/ui/08` §4 forbids the browser from calling internal APIs or holding a
service token. The browser therefore talks only to the Next.js server, which is
the sole caller of Ivr.Api. `src/lib/api/client.ts` and `src/lib/config/env.ts`
are marked `server-only`, so a Client Component that imports them fails the
build rather than shipping the API base URL to the browser.

Authorization is checked twice on purpose. `src/proxy.ts` performs an optimistic
cookie-presence check for a fast redirect; `requireSession()` runs adjacent to
every read and is the check that actually gates data. Neither replaces Ivr.Api,
which re-derives permissions on every request.

## 2. Decisions taken

| Item | Decision | Rationale |
| --- | --- | --- |
| Auth provider (§5 `NEED_CONFIRMATION`) | MOCK directory from `seed/agents.sample.json`, gated on `IVR_EXECUTION_MODE=MOCK` | `defaults-and-confirmations.md` confirms "mock JWT issuer/test keys, dev only". Real SSO/JWT stays `BLOCKED_EXTERNAL` (`G-AUTH`, `W-0006`). |
| Component library (§5 `NEED_CONFIRMATION`) | **Still open.** Built with plain CSS Modules + tokens, zero UI dependencies | The prompt's own default (Tailwind + headless) is unconfirmed. Adding no library keeps the choice reversible: adopting one later replaces these modules instead of unwinding a locked-in dependency. |
| Session transport | HMAC-SHA256 signed cookie via `node:crypto` | No new dependency, and tamper detection is unit-testable. |
| Sign-in mechanism | Route Handlers + plain form posts, not Server Actions | Works without JavaScript, and makes the auth flow reachable over plain HTTP so `E2E-UI-AUTH-05` can drive it without browser binaries. |
| Runtime-gate permission | `IVR_RUNTIME_GATE_ADMIN` granted to **no** role | `OD-V1-20` is unapproved; a test asserts no seeded role carries it. |

New dependencies: `server-only` (runtime, 1 file). Dev-only: `vitest`,
`@vitejs/plugin-react`, `jsdom`, `@testing-library/*`, `js-yaml`,
`@types/js-yaml`. `npm audit`: 0 vulnerabilities.

## 3. Tests — §8 mapping

`npm --prefix admin-ui test` → **8 files, 70 tests, 70 passed, 0 failed** (8.8 s).

| Test ID | File | Count | Asserts |
| --- | --- | --- | --- |
| `UT-UI-RBAC-01` | `tests/component/rbac.test.tsx` | 6 | Action hidden without permission, shown with it; fallback path; the whole `AdminActionDialog` — trigger *and* reason field — absent without permission; consumer outside the provider throws. |
| `UT-UI-ERR-02` | `tests/component/error-envelope.test.tsx` | 4 | `{error:{code,message,correlationId,details}}` renders localized message + raw code + correlation id; unknown code degrades to `IVR_INTERNAL_ERROR`; non-envelope body falls back; every API-06 code has a Vietnamese message. |
| `UT-UI-CORR-03` | `tests/unit/api-client.test.ts` | 9 | `X-Correlation-Id` on GET, on mutations, and with no session; `Idempotency-Key` on mutations; caller-supplied id honoured; 2 000 generated ids satisfy `InternalRequestGuard` charset and PiiGuard's MSISDN rule; `X-Actor-Id` bound to the session; non-MOCK refuses without calling; error and transport failures become typed envelopes. |
| `UT-UI-PII-04` | `tests/component/masked-phone.test.tsx` | 13 | Eight raw phone formats — including separator-obfuscated `0912 341 234` and `+84 912 341 234` — all redact; already-masked values render; a masked-but-still-complete value redacts; absent renders `—`; prices and order codes are not mistaken for numbers. |
| `E2E-UI-AUTH-05` | `tests/e2e/auth-flow.test.ts` | 8 | Real `next build` + `next start`, driven over HTTP: unauthenticated redirect with `?next=`, login page content, sign-in → cookie flags → console access → `/login` bounce while authenticated → sign-out → cookie cleared → redirect again; unknown actor issues no session; off-origin `next=` ignored; cross-site post rejected; forged cookie rejected; redirect `Location`s are relative. |

Beyond §8, three suites guard the seams this foundation depends on:

| File | Count | Asserts |
| --- | --- | --- |
| `tests/unit/contract-drift.test.ts` | 10 | UI types vs `specs/api/openapi/ivr-order-confirmation.v1.yaml` required lists and `ErrorCode` enum; `IVR_PERMISSIONS` vs `IvrPermissions.cs`; mock directory vs `seed/agents.sample.json`; no role holds `IVR_RUNTIME_GATE_ADMIN`. |
| `tests/unit/session.test.ts` | 11 | Round trip; wrong key rejected; payload edited to widen permissions rejected; expiry; malformed tokens; unknown role/permission; actor-id charset. |
| `tests/unit/sign-in.test.ts` | 9 | Directory resolution; refusal outside MOCK; unknown actor; five open-redirect payloads neutralised. |

Because there is no committed TypeScript generator (P1-1 generates .NET only),
`contract-drift.test.ts` is what makes "type-safe from OpenAPI" checkable rather
than asserted: each sample object is typed by the interface, so a removed field
breaks compilation and a changed contract breaks the comparison.

## 4. Commands and results

```text
npm --prefix admin-ui run lint          exit 0   (eslint --max-warnings 0)
npm --prefix admin-ui run typecheck     exit 0   (tsc --noEmit, strict, no any)
npm --prefix admin-ui test              8 files / 70 tests / 70 pass
npm --prefix admin-ui run build         exit 0   6 routes + Proxy
npm audit                               0 vulnerabilities
```

Routes produced by the production build:

```text
┌ ○ /                    ├ ○ /_not-found
├ ƒ /api/auth/sign-in    ├ ƒ /api/auth/sign-out
├ ƒ /login               └ ƒ /queue
ƒ Proxy (Middleware)
```

## 5. Evidence — §10 mapping

Captured against the live stack: PostgreSQL (`docker-compose.dev.yml`, port
55433, migrations applied), Ivr.Api on `127.0.0.1:5088` in MOCK, admin-ui via
`next start`.

> Port note: after this capture the local ports were moved to `5005` (API) and
> `3005` (UI) so IVR can share a machine with `ginsengfood-ops-core`, which
> occupies 5000 and 3000. The transcripts below keep the ports they were
> recorded on; the flows were re-verified on the new ports afterwards.

| §10 item | Where |
| --- | --- |
| Login flow + shell | `rbac-and-envelope-capture.txt` §1–3, §5; browser render transcript in §6 below |
| RBAC hide/show demo | `rbac-and-envelope-capture.txt` §2 vs §3 |
| Error-envelope render | `rbac-and-envelope-capture.txt` §4 (live `IVR_FORBIDDEN_CALLER`); `E2E-UI-AUTH-05` asserts `IVR_INTERNAL_ERROR` rendering when Ivr.Api is unreachable |
| Masked phone | `UT-UI-PII-04`, 13 assertions. No phone field exists on any P3-1 screen; `MaskedPhone` is the component P3-2 must use. |
| Correlation header network capture | `correlation-header-capture.txt` — literal proxy capture of headers and bodies |

### 6. End-to-end admin action, actually executed

Signed in as `AGT-ADMIN-01` in a browser, opened the "Tạm dừng hàng đợi" dialog,
entered a reason and an evidence ref, confirmed. Captured request:

```text
POST /v1/ivr/order-confirmation/queue:pause
  x-correlation-id: ui-cf7e-1f60-5ef7-262a-1d7d-59e9
  idempotency-key:  uikey-9280-0fcb-6faf-3f1f-0e45-a4e6
  x-actor-id:       AGT-ADMIN-01
  {"reason":"Sự cố năng lực: …","evidence_ref":"docs/evidence/W-0025/queue-pause-demo"}
→ 200 {"admin_action_id":"ADMIN-714586a5…","status":"APPLIED","no_policy_bypass":true}
```

Resulting rows (`ivr_admin_actions`, `ivr_audit_log`):

```text
QUEUE_PAUSE  | AGT-ADMIN-01 | IVR_QUEUE_PAUSE  | docs/evidence/W-0025/queue-pause-demo | t | ui-cf7e-…
QUEUE_RESUME | AGT-ADMIN-01 | IVR_QUEUE_RESUME |                                       | t | ui-cleanup-0001

QUEUE_PAUSE  | admin | queue | global | before {"status":"RUNNING"} | after {"status":"PAUSED"}
QUEUE_RESUME | admin | queue | global | before {"status":"PAUSED"}  | after {"status":"RUNNING"}
```

The correlation id the UI generated is the one persisted in the audit row. The
queue was resumed afterwards; the database is back to `paused=false`,
`open_hold_incidents=0`.

Rendered queue screen (Vietnamese, `Asia/Ho_Chi_Minh`):

```text
Hàng đợi IVR · Ảnh chụp năng lực và tồn đọng, đã che dữ liệu cá nhân.
Trạng thái hàng đợi: Đang chạy | Job chờ xử lý 0 | Lượt gọi đang chạy 0
Kênh SIM đang bật 0 | Sự cố đang giữ hàng đợi 0
Thời điểm chụp: 08:11:26 15/8/26
[Tạm dừng hàng đợi] [Tiếp tục hàng đợi]
```

`Kênh SIM đang bật 0` is consistent with finding `E-02` of the Phase 1/2 review:
no runtime path provisions `ivr_sim_channels`.

## 7. Defect found and fixed during browser verification

The HTTP suite passed while the real browser could not sign in.
`NextResponse.redirect(new URL(path, request.url))` emits an **absolute**
`Location` built from the Host header. A visitor on `127.0.0.1` was redirected to
`localhost`, a different cookie origin, so the freshly issued `SameSite=Strict`
session was left behind and the browser bounced straight back to sign-in. The
same failure mode applies behind any reverse proxy that rewrites Host.

Fixed by emitting relative `Location` headers (`src/lib/auth/redirect-response.ts`),
setting the cookie explicitly on the returned response rather than the ambient
store (`applySessionCookie`), and cloning `request.nextUrl` in the proxy. A
regression test asserts every auth redirect `Location` starts with `/`.

## 8. Not claimed / residual gates

- Owner and reviewer acceptance: **pending**. This entry is `TESTS_PASS`, not `ACCEPTED`.
- Component library choice: **still `NEED_CONFIRMATION`**. No library was adopted.
- Real authentication: `BLOCKED_EXTERNAL` (`G-AUTH` / `W-0006`). The UI cannot
  call Ivr.Api outside MOCK — it fails closed with `IVR_UNAUTHENTICATED`.
- Hosted GitLab pipeline evidence: `NOT_RUN`. All results above are local.
- Browser-level accessibility, visual and i18n QA: `NOT_RUN`, owned by `P5-5` (`W-0039`).
- Business screens (dashboard, call log, call detail, config, integration
  status, seed/mock management): not built here; `W-0026` / `W-0027`.
- No screenshot images are attached: the preview browser pane does not composite
  frames in this environment, so rendered output is recorded as text transcripts
  and DOM assertions instead of images.
- No real customer contacted, no SIM enabled, no order state changed, no Sales
  write, no recording. `REAL_CUSTOMER_CALL_ALLOWED=NO` throughout.

## 9. Files touched outside `admin-ui/`

- `deploy/ci/ci.gitlab-ci.yml` — `build_lint_ui` now runs `typecheck` and runs
  the test suite unconditionally (the P0-2 "no test script yet" fallback is
  obsolete), and the job summary records both. Flagged because this file is
  owned by P0-2 / `W-0011`.
