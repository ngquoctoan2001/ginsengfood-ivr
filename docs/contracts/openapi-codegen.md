# OpenAPI Code Generation and Contract Drift Policy

Status: `TARGET_CONTRACT_V1=DRAFT` · Work: `W-0014` · Generator: `NSwag.ConsoleCore 14.7.1`.

## Contract boundaries

| Boundary | Authority | Output | Runtime status |
| --- | --- | --- | --- |
| Sales → IVR task and IVR-owned API | `specs/api/openapi/ivr-order-confirmation.v1.yaml` | `Ivr.Contracts.Generated.IvrServer.V1` DTOs | Target draft; MOCK fixtures only |
| IVR → Sales Target callback | `specs/api/openapi/order-core-ivr-callback.target-v1.yaml` | `Ivr.Contracts.Generated.SalesTarget.V1` client + DTOs | Target draft; fake Sales only |
| IVR → current Golden Hour | pinned Sales Java DTO at `a3aad246…` + `specs/api/compat/current-golden-hour-callback.a3aad246.schema.json` | `Ivr.Contracts.Sales.CurrentGoldenHour` | compatibility-only; no approved runtime mode |

The three DTO families are unrelated types. There is no inheritance, implicit
conversion, shared callback request base class or “smart” DTO that changes wire
shape by provider. The internal callback record is also not the Target outbound
wire DTO; mapping is explicit in P2-6.

## Reproduce generated code

From repository root:

```powershell
dotnet tool restore
& deploy/ci/scripts/regenerate-openapi.ps1
git diff -- src/Ivr.Contracts/Generated
```

Generated files are committed. GitLab runs the same generator and fails when a
regeneration changes the committed files. Do not hand-edit `*.g.cs`.
NSwag 14.7.1 emits a few trailing spaces, so `.gitattributes` disables Git's
whitespace diagnostic only under `src/Ivr.Contracts/Generated/**`; byte-level
regeneration drift is still mandatory and handwritten files remain checked.

## Hash and human-diff gate

The reviewed SHA-256 values and generator version live in
`specs/api/openapi/contract-manifest.json`. The deterministic readable inventory
is `docs/contracts/openapi-contract-diff.md`.

Normal validation is read-only:

```powershell
npm --prefix deploy/ci run openapi:validate
npm --prefix deploy/ci run openapi:drift
```

If an OAS or the verified compatibility fixture changes, the drift command must
fail. Review paths, required fields, enums, auth, privacy rules, ACK taxonomy,
generated-code diff and downstream compatibility first. Only after that review
may a developer explicitly run:

```powershell
npm --prefix deploy/ci run openapi:accept-reviewed-draft
& deploy/ci/scripts/regenerate-openapi.ps1
```

That command only pins a new reviewed draft baseline. It does not mean Sales has
implemented or approved Target V1, and it must not change
`TARGET_CONTRACT_V1=DRAFT` without the external owner artifact.

The IVR draft comparison baseline is `1.0.0-draft.20` (rotated from `1.0.0-draft.2`
by W-0124, after OD-17's owner-approved `sellable_status` removal made the older
window permanently red). Every superseded baseline — `1.0.0`, `1.0.0-draft.2` —
and its generated transition report remain committed as audit history; never
overwrite them when rotating a future reviewed draft. Rotating the baseline is
how an approved breaking change stops masking the next unapproved one; deleting
the report it rotated past is how the approval itself gets lost.

## Current compatibility and deprecation

- Endpoint: `POST /api/v1/internal/ivr/golden-hour/callbacks`.
- Transitional auth: `X-Internal-Token`; it must never leak into Target V1.
- Scope: Golden Hour only. `TWENTY_FOUR_SEVEN` is rejected before dispatch.
- Unsupported current fields include contract/callback/task versioning, order
  version, Target attempt flags/action/evidence and semantic ACK.
- Current 200/422 behavior is not mapped to Target semantic codes.
- Runtime startup remains fail-closed because no approved execution-mode/provider
  combination enables current-compat. P4-1 may enable a reviewed sandbox/cutover
  profile after Sales supplies URL/auth/compatibility evidence.
- Removal requires a Sales-published generic endpoint, CDC pass, zero remaining
  consumers, an owner-approved sunset date and rollback evidence.

## External gates deliberately left open

No production URL, credential, JWT issuer, mTLS profile or real-provider pass is
stored here. `W-0002`, `W-0005`, `W-0006` and `W-0061` remain
`BLOCKED_EXTERNAL`; fake/WireMock evidence is only `TESTS_PASS`.
