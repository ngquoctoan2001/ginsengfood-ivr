# W-0099 — SIM channel surface (`GET /sim-channels` + console controls)

| | |
| --- | --- |
| Work ID | `W-0099` · Origin `RED_TEAM_REMEDIATION` (Phase 3 audit finding #1) |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |

## 1. The gap

`specs/ui/08` §3 lists seven console actions. Two of them —

```
| Disable SIM | IVR_SIM_DISABLE | POST /sim-channels/{id}:disable |
| Enable SIM  | IVR_SIM_ENABLE  | POST /sim-channels/{id}:enable  |
```

— had no control anywhere in the console at the end of Phase 3. The operations
themselves shipped with P2-8 (`W-0065`); what was missing was any way to reach
them. `W-0025` deferred them explicitly ("they arrive with the configuration
screens that own them, P3-3 / `W-0027`"), `W-0027` did not build them, and no
remaining Phase 3 prompt owned them.

The consequence was concrete: `IVR_SIM_ENABLE` and `IVR_SIM_DISABLE` were seeded
to Ops and AdminIM but unusable, and the roles screen told the operator the
controls were coming in "màn cấu hình kênh, P3-3 sau" — a screen with no spec and
no prompt behind it.

The blocker was data, not permission. The dashboard SIM panel showed counts
(`total/idle/active/disabled/health_failed`) and no channel identity, so there
was nothing to act on.

## 2. What was added

**API** — `GET /sim-channels` (`IVR_QUEUE_VIEW`), in `AdminReadService`:

| Projected | Withheld | Why |
| --- | --- | --- |
| `sim_channel_id`, `enabled`, `status`, `adapter_mode`, `provider_name`, `busy`, `active_call_job_id`, `fail_count`, `quarantined`, `quarantine_until`, `cooldown_until`, `last_health_check_at`, `disabled_reason` | `sim_number_ref` | It points at a phone identity the console has no use for (D-05). |
| | `lease_token`, `lease_fencing_generation`, `leased_by_worker_id`, `lease_acquired_at`, `lease_expires_at` | Scheduler mechanics, not operator information. Showing them invites manual interference with fencing. |

**Console** — the roster renders as a table inside the dashboard's existing SIM
section (`specs/ui/01` puts the SIM panel there, so no new screen was invented),
with one control per row.

Three decisions worth stating:

- **Only the meaningful control is offered.** An enabled channel offers disable,
  a disabled one offers enable. Each is wrapped in `RequirePermission`, so an
  actor holding only `IVR_SIM_DISABLE` never sees an enable button.
- **`busy` is surfaced, and the copy tells the truth about it.** Disabling a
  channel that is carrying a call is accepted — it stops new dispatch and takes
  effect when the call ends. The dialog says exactly that rather than implying
  the change is immediate or that the call will be cut.
- **The channel id travels as a hidden form field**, not a closure argument, so
  the control keeps working without client JavaScript like every other admin
  action in this console.

## 3. Contract governance

```text
oasdiff changelog draft.2 → current : 11 endpoints added, nothing else
oasdiff breaking  --fail-on WARN    : "No breaking changes to report"
```

`1.0.0-draft.5` → `1.0.0-draft.6`. Codegen regenerated, manifest re-pinned,
changelog and portal rebuilt, `openapi:drift` reports
`OPENAPI_HASHES_PINNED=3` / `OPENAPI_HUMAN_DIFF_CURRENT=YES`. No new permission,
no change to any existing operation.

## 4. Tests

| Test ID | Asserts |
| --- | --- |
| `IT-SIM-READ-01` | `200` for `IVR_QUEUE_VIEW`, `403 IVR_FORBIDDEN_CALLER` for an actor holding only `IVR_MANUAL_RETRY`, `405` for a POST to the roster route. |
| `IT-SIM-READ-02` | Idle / busy / disabled-and-quarantined channels each report correctly; `active_call_job_id` is carried; the response contains no `sim_number_ref`, no seeded `sim-ref-secret*` value and no lease field. |
| `UT-UI-SIM-05` | Enabled offers disable and disabled offers enable; an actor with neither permission sees no button; an actor with only disable sees nothing on a disabled channel; the busy copy says the change is not immediate; the channel id is a form field; a reason is required. |
| `E2E-UI-LOG-01` (extended) | The dashboard lists both seeded channels, marks the quarantined one, offers both controls to `AGT-ADMIN-01`, and never emits `sim_number_ref`. |

.NET: **301/301** (21 contract, 168 unit, 112 integration), build 0 warnings.
admin-ui: **180/180** across 17 files, lint and `tsc --noEmit` clean, build 16
routes.

## 5. Not claimed

- The controls are exercised against MOCK channels only. No real SIM, modem or
  carrier is involved; `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- Disabling does not terminate an in-flight call, and nothing here changes the
  scheduler's lease or fencing behaviour — the operations are the ones P2-8
  already shipped and tested.
- Owner and reviewer acceptance: **pending**. `TESTS_PASS`, not `ACCEPTED`.
- Hosted GitLab pipeline evidence: `NOT_RUN`.
