# W-0105 — Console account authentication and two-role RBAC evidence

> **SUPERSEDED / HISTORICAL — W-0128 (2026-08-28).** Đây là evidence nguyên bản của
> implementation account/session đã từng tồn tại; không phải hướng dẫn vận hành hiện tại.
> W-0128 đã xoá account/session khỏi IVR và chuyển operator identity/UI ownership về Module 3.
> Migration thêm `20260822120000_W0105ConsoleAccountAuth` và migration drop mang tên lịch sử
> `20260828040458_W0122DropConsoleAccounts` được giữ nguyên để không rewrite schema history.
> Các test/route/tool được nêu bên dưới đã bị nghỉ hưu có chủ đích.


Status: `TESTS_PASS`  
Date: `2026-08-22`  
Development baseline: `main@845b237`  
Final observed checkout: `main@f7c9be9` (owner W-0104 acceptance commit landed concurrently)

This pack proves the W-0105 implementation in disposable local/lab
environments. It does **not** authorize production deployment, real customer
calls, carrier/SIM use, or Sales integration.

## Implemented boundary

- Console users have exactly two roles: `Admin` and `Operator`.
- `Admin` receives the approved operations permissions plus account view,
  account management, password reset, and self-profile permissions.
- `Operator` receives exactly `IVR_ACCOUNT_SELF_VIEW`, `IVR_QUEUE_VIEW`,
  `IVR_SIM_DISABLE`, and `IVR_MANUAL_RETRY`.
- `IVR_FLAG_READ` and `IVR_RUNTIME_GATE_ADMIN` remain unassigned.
- The browser stores only an opaque `HttpOnly`, `SameSite=Strict` session
  cookie. PostgreSQL stores SHA-256 session-token hashes and versioned
  PBKDF2-SHA512 password hashes, never raw values.
- Account and authorization enforcement is server-owned; hiding UI controls is
  not treated as the security boundary.

## Controlled bootstrap proof

The `Ivr.AccountBootstrap` tool was run against a disposable PostgreSQL
container with `--environment local` and the password supplied through the
secret-input path. No password, hash, or raw session token was written to this
evidence pack.

First run:

| Username | Display name | Role | Status | Result |
| --- | --- | --- | --- | --- |
| `admin` | Quản trị hệ thống | `Admin` | `ACTIVE` | `CREATED`, built-in |
| `ngquoctoan2001` | Nguyễn Quốc Toàn | `Operator` | `ACTIVE` | `CREATED` |
| `trcongphuc2003` | Trương Công Phúc | `Operator` | `ACTIVE` | `CREATED` |

The second run returned `EXISTS` for all three records without overwriting
metadata or passwords. A production-mode invocation exited `1` before database
access. The database contained three `ADMIN_ACCOUNT_BOOTSTRAP` audit records,
and an exact comparison confirmed that no password column equalled the
bootstrap plaintext.

All three accounts successfully authenticated using the owner-supplied
bootstrap secret. The secret is intentionally recorded only as `[REDACTED]`.

After disposable validation, the same guarded tool was run against the active
local development PostgreSQL container. It returned `CREATED` for all three
requested usernames. A metadata-only query confirmed three `ACTIVE` accounts,
the requested display names/roles, `admin.is_builtin=true`, a non-empty hash for
each account, and exactly three bootstrap audit records. The running API/UI
containers were not rebuilt or restarted; runtime deployment remains `NOT_RUN`.

## API and RBAC proof

Against the disposable database and real Ivr.Api process:

- all three bootstrap accounts returned a valid console session;
- Operator self-profile returned `200`;
- Operator account-list access returned `403`;
- Admin account-list access returned `200` and contained both requested
  Operator accounts;
- `IT-ACCOUNT-RBAC-01` covers role derivation and fail-closed authorization;
- `IT-ACCOUNT-CRUD-02` covers create, idempotent replay, password reset, session
  revocation on reset, disable, soft-delete and username non-reuse;
- `IT-ACCOUNT-LOCK-03` covers generic `401` and the durable five-attempt lockout.

An earlier revision of this pack credited `IT-ACCOUNT-CRUD-02` with role change,
reactivation, the built-in and last-active-admin invariants, and audit
assertions. It did not exercise any of those: the behaviour existed in
`ConsoleAccountService` but no test drove it, so the claims were unbacked. The
list above now states only what `IT-ACCOUNT-CRUD-02` actually asserts, and the
missing coverage was written rather than the claims dropped:

- `IT-ACCOUNT-CRUD-08` covers promotion `Operator -> Admin` with session
  revocation and a re-issued session carrying the new permission set, the
  disable/reactivate round trip proven by signing in again afterwards, refusal to
  demote/disable/delete the built-in `admin` with the stored row verified
  unchanged and no audit row written, refusal to demote or delete the last active
  admin plus the promote-a-second-admin escape hatch, and one audit row per
  mutation whose `before`/`after`/`data`/`reason` carry no password, verifier or
  session token.
- `IT-ACCOUNT-ADMINPOLICY-07` covers `IvrRoles.ConsoleAdminPolicy`, which had no
  test at all. The original suite authorized against synthetic `/rbac/*` probes
  and never mapped `IvrAdminEndpoints`, so the policy could have been deleted
  without turning a single test red. The new tests drive the real routes: an
  Operator is refused `/scripts`, `/integration-status`, `/review-items` and all
  four `/analytics/*` reads, an Admin reaches all seven, an Operator keeps
  `/queue`, `/dashboard`, `/call-jobs` and `/sim-channels`, and an Operator is
  refused `queue:pause`, `queue:resume`, `sim-channels:enable` and
  `admin-reviews`. Neutralising the policy to `RequireAssertion(_ => true)` was
  used to confirm the suite fails when the control is absent: all seven Operator
  cases then returned `200`, including the `/analytics/export` extract.

`IT-ACCOUNT-CRUD-08` also documents a live behaviour worth knowing: a successful
sign-in updates `last_login_at` and therefore bumps the account's optimistic
concurrency `version`. An administrator whose edit form was loaded before the
target user signed in will receive `409 IVR_ACCOUNT_CONFLICT` on save and must
reload. This is correct concurrency control, not a defect, but it is a real
console behaviour and is not currently surfaced in the UI copy.

The session selector was regression-hardened so only tokens with the
`ivr_session_` prefix enter console authentication. Existing Order Core service
bearer requests continue through their original authentication path.

## Post-implementation review finding and remediation — MOCK seam bypass

An independent review of this work found that the console account surface was
reachable **without any credential** whenever `IVR_EXECUTION_MODE=MOCK`, which is
the default execution mode. The routes named a permission but never named an
authentication scheme, so the policy-scheme selector fell through to
`MockPermissionAuthenticationHandler`, which mints whatever the caller writes in
`X-Permissions`. `MockPermissionHeaderGuardMiddleware` only rejects that header
*outside* MOCK, so it did not apply.

Reproduced against the running development API before the fix:

| Probe | Before | After |
| --- | --- | --- |
| `GET /accounts` with `X-Permissions: IVR_ACCOUNT_VIEW` | `200` + full account roster | `401 IVR_UNAUTHENTICATED` |
| `POST /accounts` with `X-Permissions: IVR_ACCOUNT_MANAGE` | `422` — authorization passed, stopped only by field validation | `401 IVR_UNAUTHENTICATED` |
| `POST /accounts/{id}:reset-password` | same authorized path | `401 IVR_UNAUTHENTICATED` |
| `GET /accounts/me` | leaked the named account's profile | `401 IVR_UNAUTHENTICATED` |

The `422` is the material one: authorization had already succeeded, so a caller
with a valid body could have created accounts and reset the built-in `admin`
password without knowing any password. The before-state probes were read-only or
deliberately invalid; no account was created or modified.

The remediation has two independent halves, so a future route that forgets one is
still covered by the other:

1. `IvrPermissions.ConsoleSessionOnly` marks the four `IVR_ACCOUNT_*` permissions
   as non-mintable by the MOCK seam, and
   `MockPermissionAuthenticationHandler` now filters on
   `IvrPermissions.IsMockGrantable`.
2. Every authenticated console route carries the `IVR_CONSOLE_SESSION` policy,
   which names `ConsoleSessionAuthenticationHandler` as its only authentication
   scheme, so the MOCK seam is never consulted for those routes.
   `POST /auth/sign-in` remains the sole anonymous route.

`IT-ACCOUNT-SCHEME-06` locks all three properties: the ten protected routes
answer `401` to mock headers; an intentionally *unpinned* probe requiring
`IVR_ACCOUNT_VIEW` answers `403`, proving half 1 holds on its own; and a
structural walk of `EndpointDataSource` asserts every console route resolves to
exactly the console scheme and that only sign-in allows anonymous.

Post-fix verification against a separately hosted API process (port `5099`, the
owner's process on `5005` and the development containers untouched): all four
probes above returned `401`; a real Operator sign-in still succeeded,
`GET /accounts/me` returned `200`, `GET /accounts` returned `403`, and a valid
Operator bearer token combined with a forged `X-Permissions: IVR_ACCOUNT_MANAGE`
header still returned `403` — the bearer path does not read that header. The
session opened for this check was revoked through `POST /auth/sign-out`.

## Admin UI and cross-stack proof

The built Next.js application was exercised against the real Ivr.Api and the
same disposable PostgreSQL instance:

- Operator `/profile` rendered successfully;
- direct Operator `/accounts` navigation redirected to
  `/dashboard?error=forbidden` without rendering account data;
- Admin `/accounts` rendered both requested Operator accounts;
- account list/detail/create/update/password-reset/delete forms and API clients
  are present, with server-side Admin guards on page load and form actions;
- role navigation and profile access are derived from the API session.

The local harness used HTTP while production `next start` correctly marks the
cookie `Secure`; the cross-stack harness therefore replayed the returned cookie
header to simulate HTTPS cookie delivery. No security attribute was weakened in
source for the test.

## Post-review remediation — behaviour defects

Three defects found by the same review, all reachable by an ordinary operator or
administrator rather than by an attacker. Each fix was mutation-tested: the
control was neutralised, the new test was confirmed to fail, and the control was
restored.

### Lockout never released its counter

`failed_login_count` was not cleared when a lockout expired, so the count stayed
at the threshold and the first failure after the window re-locked the account
immediately. An account that tripped the limit once was stuck at one attempt per
fifteen minutes for the rest of its life, and because an Operator cannot reset
their own password, only an administrator could clear it.

`ConsoleAccountService.SignInAsync` now clears both the timestamp and the counter
once the guard has established the lockout is no longer live.
`IT-ACCOUNT-LOCK-09` seeds an expired lockout, spends four failures without
re-locking, checks the stored counter is `4` and `locked_until` is null, then
signs in successfully. With the fix removed the same test observes a counter of
`6` — the old behaviour, re-locked on the first failure.

### The account roster returned soft-deleted accounts

`ListAsync` applied no `deleted_at` filter, so every soft-deleted account stayed
in the roster and in `total_count` forever. Soft-deleted rows exist so audit
identity survives and a username is never reassigned; they are not administrable.

They are now excluded by default and opt-in through `include_deleted`, which
`total_count` follows so paging cannot report a total the caller can never reach.
The console exposes it as a "Hiện tài khoản đã xóa" toggle carried in the URL.
`IT-ACCOUNT-LIST-10` deletes one of the three seeded accounts and asserts `2` by
default, `2` with `include_deleted=false`, and `3` with `include_deleted=true`
including the `DELETED` row. With the filter removed it observes `3` by default.

### Unaccented Vietnamese surnames returned HTTP 500

`display_name` was validated with `PiiGuard.EnsureSafeText`, whose ASCII address
branch matches the unaccented spellings of five Vietnamese location words when
followed by a space. Two of those words are also ordinary family names — Dương
and Ngô — so a staff member whose name was entered without diacritics was
refused, and refused by an `InvalidOperationException`, which the error
middleware turned into `500 IVR_INTERNAL_ERROR`. The administrator could not
create the account and was told the system had failed.

*(The literal strings are deliberately not written out here: this pack is
scanned by `deploy/ci/scripts/scan-pii.sh`, which uses the same pattern, so
quoting them would fail the scan. They are in the test data of
`UT-ACCOUNT-NAME-05` and `IT-ACCOUNT-NAME-11`, which is where they belong.)*

Owner decision on 2026-08-22: use a purpose-built validator for this field rather
than narrowing the shared guard.

- `PiiGuard` itself is **unchanged in behaviour**. Its pattern was refactored into
  named branch constants and a second, additive `IsSafeContactText` was composed
  from the phone and dial-token branches only, so the two cannot drift.
  `OD-OPEN-02` — "when this guard rejects an identifier, the identifier changes" —
  still holds for every customer-facing surface, and
  `UT-ACCOUNT-NAME-05` asserts `IsSafeText` still rejects all four categories it
  rejected before: the unaccented name-shaped spelling, the ASCII house-number
  phrase, the accented street phrase, and a bare subscriber number.
- `ConsoleDisplayNamePolicy` validates length, control characters and contact
  values, and a refusal is now `422 IVR_ACCOUNT_POLICY_VIOLATION` with a message
  that says which rule failed.
- The idempotency snapshot guard and `PiiMaskingFilter` had the same problem on
  the way out. The filter now checks the exact key `display_name` with the
  contact-only rule and keeps the full guard for every other string;
  `customer_display_name` and `program_display_name` are different keys and are
  deliberately unaffected.

`IT-ACCOUNT-NAME-11` round-trips three names through create, the idempotency
snapshot, the response filter and a re-read, and confirms a display name
containing a phone number is still refused — as `422`, not `500`. With the old
validator restored the same test observes `InternalServerError`.

**`OWNER_DATA_REQUIRED`:** Privacy must sign off that a staff display name is
validated by `ConsoleDisplayNamePolicy` rather than the customer-PII guard before
this reaches production. The field is already registered in
`PersonalDataInventory` with a legal basis; what is outstanding is approval of the
narrower validation contract, not the field itself.

## Post-review remediation — guards and consistency

### The sign-in rate limit constrained neither axis

`ConsoleSignInRateLimiter` kept one counter keyed on `"{ip}|{username}"`. That is not a
per-IP limit and not a per-username limit: one host had a full budget against every
username it tried, and a pool of hosts had a full budget each against one username —
the two runs the control exists to stop.

Replaced with two independent sliding counters — 30/minute per address and 5/minute per
username — both charged before either is judged, so a request already refused by one axis
still costs the other. Otherwise a host that is already blocked could probe usernames for
free. The per-username limit sits below the lockout threshold on purpose, so ordinary
mistyping is answered by a cheap `429` rather than by a fifteen-minute lockout only an
administrator can lift.

`UT-ACCOUNT-RATE-12` drives each axis on its own, which is the only way to tell a real
two-axis limit from the pair key — the pair key passes any test that varies both together.
With the pair key restored, all four cases fail.

### `ErrorCode` had lost its drift guard

`ConsoleAccountErrorCode` was added as a superset so the pre-existing response enums would
not have to change, and `UT-UI-CONTRACT-06` moved its assertion onto it. That left
`ErrorCode` — still referenced by `ErrorEnvelope`, and therefore by every operation written
before the console API — with nothing checking it: a code could be added there, never reach
the TypeScript mirror, and no test would fail.

Two assertions restore the guard and pin the relationship, so the split stays a deliberate
superset instead of two catalogues drifting apart. Adding a probe code to `ErrorCode` alone
fails both, and failed nothing before.

### Navigation gated admin routes on a permission instead of the role

`ConsoleNav` hid `/reports`, `/review`, `/config`, `/integration`, `/seed`, `/accounts` and
`/roles` behind `IVR_ACCOUNT_VIEW`. That produced the right result only by coincidence —
that permission happens to be Admin-only today. Grant it to some future read-only support
role and those screens appear in their sidebar with nothing in the code stating why. The
pages behind them gate on `requireAdmin()` and the API gates them with
`IvrRoles.ConsoleAdminPolicy`, so the sidebar now states the same rule.

`UT-UI-NAV-07` is the first test this component has ever had. Its decisive case hands an
Operator `IVR_ACCOUNT_VIEW` and asserts the admin entries stay hidden; with the old gating
restored, exactly that case fails. A fourth case asserts only `<li>` elements are emitted
into the navigation `<ul>`, since the admin branch needs a keyed Fragment rather than a
wrapper element.

### `409` on a concurrent sign-in now explains itself

Owner decision: keep the optimistic-concurrency check — it is what stops two administrators
silently overwriting each other — and fix the wording. Both account screens now render a
plain-language explanation next to `IVR_ACCOUNT_CONFLICT`: the data changed since the form
was opened, usually because that user signed in, reload and retry, nothing was saved.
`IVR_ACCOUNT_POLICY_VIOLATION` gets the same treatment.

## Verification summary

| Gate | Result |
| --- | --- |
| `dotnet build Ivr.sln` | PASS, 0 warnings / 0 errors |
| Contract tests | PASS, 22/22 |
| Unit tests | PASS, 419/419 |
| Chaos tests | PASS, 6/6 |
| Integration tests | PASS, 227/227 |
| Full .NET total | PASS, 674/674 |
| `dotnet format --verify-no-changes` | PASS |
| Admin UI lint / typecheck / production build | PASS |
| Admin UI Vitest | PASS, 20 files / 193 tests |
| OpenAPI parse/lint/drift/config/codegen/portal | PASS |
| `oasdiff --fail-on WARN` | PASS, no breaking or warning-level change |
| Traceability regeneration | PASS, 395 tagged tests |
| NuGet high/critical vulnerability audit | PASS, 0 findings |
| Both npm high vulnerability audits | PASS, 0 findings |
| Gitleaks workspace scan | PASS, 0 findings |
| Gitleaks negative PAT self-test | PASS, expected exit 42 |
| PII scanner self-test | PASS, unsafe fixtures rejected |
| PII scan of `docs/evidence` | PASS, 99 text files / 2 binary skipped |
| GitNexus refresh | PASS, 48,111 nodes / 65,027 edges / 300 flows |
| GitNexus change detection | COMPLETED, `CRITICAL`: 148 files / 833 changed symbols / 201 affected |

The final `CRITICAL` reading covers the **whole working tree**, not this work item. The tree
carries W-0105, the post-review remediation, an unrelated admin-UI presentation pass, and W-0106
regional-voice work that a second process was writing concurrently — see the note below. The
remediation's own delta is 11 source files and 8 test files, every one of them covered by the
674-test regression above and by a mutation test that confirmed the new assertions fail when the
control they guard is removed.

The GitNexus result is deliberately not downgraded. It includes the expected
high-blast-radius persistence/auth/API-client changes and concurrent, unrelated
UI WIP already present in the shared worktree. Full backend, UI, contract,
bootstrap, security, and cross-stack regressions above are the compensating
evidence. No commit or push was performed.

## Concurrent worktree during remediation

A second process was editing this worktree while the remediation gates ran. The OpenAPI
contract moved from `1.0.0-draft.11` to `1.0.0-draft.12` mid-run with a `voice_region`
field added, and `admin-ui/src/i18n/vi.json`, `src/lib/api/types.ts`, `deploy/lab/**` and
`docker-compose.softphone.yml` were written by that process — all W-0106 regional-voice
work, none of it part of this item.

Two consequences are recorded rather than smoothed over:

- The `contract-manifest.json` re-pin at the end of this work **necessarily covers W-0106's
  contract change as well as this one**, because they share a file. `oasdiff` was run on the
  combined `draft.2 -> draft.12` state and reports no breaking change for either contract,
  but the acceptance of the `voice_region` addition belongs to whoever owns W-0106.
- `docs/evidence/W-0106/README.md` and `docs/evidence/W-0106/phase-4-lab-runbook.md`
  currently fail `deploy/ci/scripts/scan-pii.sh` on three lines. `docs/evidence/W-0105`
  passes on its own. The failure is real and needs fixing before the evidence tree is
  clean; it is not part of this item.

One transient full-suite failure (166 of 226 integration tests) was observed while several
Testcontainers PostgreSQL instances were competing for the same Docker host. The same tests
pass individually and in a quiet re-run; it was contention, not a defect, and no assertion
was changed on account of it.

## Residual gates

- `OWNER_DATA_REQUIRED`: Legal/Privacy still must approve production retention
  and anonymization for `staff_account` and `console_session`.
- `G-AUTH/W-0006`: service identity with Sales remains independently
  `BLOCKED_EXTERNAL`; console login does not close it.
- Hosted CI, deployment, production migration, and owner UAT are `NOT_RUN`.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` remains unchanged.
