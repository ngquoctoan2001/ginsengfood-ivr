# IVR API Contract Changelog

Status: `TARGET_CONTRACT_V1=DRAFT` · Generator: `oasdiff v1.26.1`.

This file is the human index for machine-generated contract comparisons. The
two linked reports are regenerated from the pinned historical baselines and
compared byte-for-byte in GitLab CI. A breaking change fails
`api_contract_diff`; updating a baseline requires an explicit contract review
and does not approve the external Sales contract.

## Current comparisons

| Contract | Baseline | Current | Generated report |
| --- | --- | --- | --- |
| IVR-owned Target V1 draft | `1.0.0-draft.20` | `1.0.0-draft.21` | [IVR API changelog](api/changelog/ivr-order-confirmation.md) |
| Sales callback Target V1 draft | `1.0.0-draft` | `1.0.0-draft` | [Sales callback changelog](api/changelog/order-core-ivr-callback.md) |

`1.0.0-draft.3` (W-0095) added three read-only admin operations — `GET /dashboard`,
`GET /call-jobs` and `GET /call-jobs/{ivrCallJobId}/detail` — so the admin console
can display queue, call-log and call-detail state without the browser reaching a
service-only lifecycle endpoint.

`1.0.0-draft.4` (W-0096) adds three more, all read-only: `GET /scripts`,
`GET /integration-status` and `GET /review-items`, backing the P3-3 back-office
screens.

`1.0.0-draft.5` (W-0098) adds four more read-only operations backing the P3-4
reporting console: `GET /analytics/summary`, `GET /analytics/trend`,
`GET /analytics/breakdown` and `GET /analytics/export`. They return aggregate
values only — counts, rates and dimension labels — and drop any bucket below the
server-side `min_bucket_size` before serialization. `warehouse_backed` is
reported `false` because the P10-4 pipeline (`W-0055`) does not exist yet, so the
console cannot present operational reads as a BI pipeline. The export operation
is a `GET` with a mandatory `reason`: it is a read that is audited, not a state
change, so no mutation surface was introduced.

`1.0.0-draft.6` (W-0099) adds one read operation, `GET /sim-channels`. The
enable and disable operations for a SIM channel have existed since P2-8, but
`specs/ui/08` §3 lists both as console actions and no screen could reach them:
the dashboard SIM panel carried counts only, with no channel identity to act on.
This supplies the roster. It projects no `sim_number_ref` — that points at a
phone identity the console has no use for (D-05) — and no lease internals.

`1.0.0-draft.7` (W-0101) adds no operation. It completes three read
projections against their UI specs: the dashboard gains `call_success_rate`,
`sim.failure_rate`, `queue.attempt_two_pending` and `queue.blocked` — four tiles
`specs/ui/01` asks for that had no field behind them — and the call detail gains
the per-line `sellable_status` snapshot `specs/ui/03` requires. All are response
fields on existing operations; nothing was removed or renamed.

`1.0.0-draft.8` (W-0055) adds no operation. It makes the analytics source
truthful after the warehouse pipeline landed: `data_quality.source` names the
store that answered and `warehouse_status` separately reports whether ETL is
complete, backlogged or mismatched.

`1.0.0-draft.9` (W-0103) adds no operation. It closes the blocked-result
ambiguity: `IVR_OPERATIONAL_BLOCKED` and `IVR_POLICY_BLOCKED` stay in the shared
compatibility enum, but the IVR producer does not emit them. The two analytics
fields that previously returned a misleading numeric zero now explicitly return
`null` until a dedicated intake/pre-call block fact source exists. Sales blocking
after revalidation remains callback ACK `BLOCKED_BY_CORE` and never rewrites the
observed customer result.

`1.0.0-draft.10` through `draft.12` are one combined reviewed candidate from the
concurrent W-0105/W-0106 stream: console account/session and two-role RBAC operations,
the OD-V1-20 runtime-gate authorization clarification, and the additive `voice_region`
read field. The intermediate draft numbers were not committed as standalone baselines.

`1.0.0-draft.13` (W-0109) adds the governed script lifecycle operations and detail
schemas. `draft.14` (W-0111) adds single-call and bulk in-flight termination operations.
`draft.15` (W-0112) adds the non-production seed loader, scenario dry-run and integration
profile operations. `draft.16` (W-0113) records the voice actually dispatched and exposes
its provenance instead of silently re-deriving audit history.

`1.0.0-draft.17` (W-0116) adds two optional response facts to the read-only integration
projection: `IvrDependencyStatus.detail_vi` is a Vietnamese companion while raw `detail`
remains unchanged for log lookup; `IvrFailClosedEvent.hold_new_calls` lets the console
translate `CAPACITY_INCIDENT` without inferring a boolean from English prose. Both remain
optional for draft.16/draft.17 rolling compatibility. No operation or permission changes.

`1.0.0-draft.18` (W-0117) changes no DTO semantics or permissions. It replaces 13
OpenAPI 3.0 `nullable: true` spellings with the equivalent OpenAPI 3.1 null unions and
publishes script-draft creation at canonical `POST /scripts`. Runtime and integration
tests keep `POST /scripts/` as a compatibility alias, so existing callers are not cut off.

`1.0.0-draft.19` (W-0118) documented the former `OD-15` trusted-skip placement. It added
descriptions without changing the three optional wire shapes. `draft.20` (OD-17) removed the
read-only `sellable_status` projection after IVR stopped consuming ops-core sellability data.

`1.0.0-draft.21` (W-0123/OD-18) makes Module 3 authoritative for the business call decision.
Trust fields/evidence remain deprecated `LEGACY_READ` inputs for rolling compatibility but active
eligibility ignores them; `risk_flags` is scheduler/audit metadata only. The persisted
`TASK_SKIPPED_TRUSTED_CUSTOMER` enum stays readable for historical clients/rows, while runtime
`draft.21` does not emit it. The incremental `draft.20 → draft.21` comparison has no breaking
change.

W-0124 rotated the IVR comparison baseline from `1.0.0-draft.2` to `1.0.0-draft.20`, and the
gate reports **no breaking changes** again. The rotation is a repair, not a suppression. OD-17
removed `sellable_status` with owner approval at draft.20 — a genuinely breaking change that
`--fail-on WARN` is built to report. Leaving draft.2 as the live baseline therefore left
`api_contract_diff` (`allow_failure: false`) red on every pipeline for a decision that had already
been signed off, which W-0123 recorded honestly as `PREEXISTING_GATE_FAILURE` but which no future
work item could have turned green either. A gate that cannot pass stops being read, and the next
unapproved breaking change would have arrived as one more red among reds.

What keeps the rotation auditable is that the closed window is frozen, not deleted: the full
`draft.2 → draft.20` comparison, `sellable_status` removal included, is preserved in
[the archived transition report](api/changelog/ivr-order-confirmation.v1.0.0-draft.2-to-v1.0.0-draft.20.md),
exactly as the earlier `1.0.0 → draft.2` reset was. Neither verdict approves a deployment or the
external Sales contract.

The Sales callback report still says `No changes detected`. The previous IVR baseline is
retained at `baselines/ivr-order-confirmation.v1.0.0.yaml`; its transition to
draft.2 contained `143` changes (`63` errors and `80` warnings) and is preserved
in [the archived transition report](api/changelog/ivr-order-confirmation.v1.0.0-to-v1.0.0-draft.2.md).
This reviewed draft reset repairs the comparison gate; it does not claim that
Target V1 is live, backward compatible, or approved by Sales.

## Change procedure

1. Change the authoritative OpenAPI file only after reviewing the consumer and provider impact.
2. Run lint, schema validation, generated-code drift and `api_contract_diff`.
3. Regenerate the matching report with the pinned oasdiff image.
4. Apply the SemVer and deprecation rules in [API versioning policy](api-versioning.md).
5. Update the historical baseline only when the reviewed version becomes the new comparison base.
6. Record external Sales approval separately; a clean diff never changes `TARGET_CONTRACT_V1=DRAFT` by itself.
