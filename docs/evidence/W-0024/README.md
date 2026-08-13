# W-0024 / P2-7 — Script, Content Approval and Speech Safety

Status: `TESTS_PASS`. This is local MOCK/disposable-PostgreSQL evidence, not LAB, real-call or production approval.

## Delivered scope

- Versioned lifecycle `DRAFT → IN_REVIEW → APPROVED → RETIRED` with explicit RBAC, actor, reason, correlation and privacy-safe audit.
- `IScriptRegistry.TryGetApproved(templateId, version, mode)` returns no version unless the mode-specific approval gate passes.
- Synthetic Vietnamese `MOCK_TEST` fixture `SCRIPT-ORDER-CONFIRM:v1-test-approved`; LAB and PROD remain fail-closed.
- Target V1-only template/input whitelist; unknown/missing/oversized/HTML/control/PII input is rejected.
- Deterministic Vietnamese preview for one/many items, remainder collapse, VND display, short area and fixed DTMF 1/0 instructions.
- Preview preserves the PUBLIC-SAFE input snapshot plus input/template/content hashes and estimated duration.
- PostgreSQL version/approval tables, lifecycle/four-eyes insert guards, append-only approval trigger, approved-content immutability trigger, migration seed and rollback/recreate coverage.

## Approval gates

| Mode | Required state | Repository default |
| --- | --- | --- |
| `MOCK` | `MOCK_TEST` | seeded synthetic fixture allowed |
| `LAB_REAL_SIM` | `LAB` | blocked; no seed |
| `PRODUCTION_REAL` | `CONTENT` + `PRIVACY_LEGAL` from different actors, plus owner decision for Target V1 items/area | blocked; `ProductionTargetV1FieldsApproved=NO` |

No A/B selection, notification template, recording or fallback to an unapproved version was added.

## Focused evidence

| Test ID | Coverage | Result |
| --- | --- | --- |
| `UT-SCRIPT-SEED-01` | MOCK seed; LAB/PROD fail-closed | PASS |
| `UT-SCRIPT-LIFECYCLE-02` | RBAC, four-eyes, audit, mode gates, retirement | PASS |
| `UT-SCRIPT-PROD-GATE-03` | `OD-V1-15` production lock | PASS |
| `UT-SCRIPT-TEMPLATE-GUARD-04` | unknown/HTML/raw phone/unsupported key | PASS |
| `UT-SCRIPT-INPUT-GUARD-05` | control/missing/oversized/full-address/PII input | PASS |
| `UT-SCRIPT-RENDER-GOLDEN-06` | Vietnamese diacritics, one item, VND, area, 1/0, hashes | PASS |
| `UT-SCRIPT-RENDER-COLLAPSE-07` | many items, decimal quantity, collapse, large amount | PASS |
| `IT-SCRIPT-SEED-07` | PostgreSQL seed and mode gates | PASS |
| `IT-SCRIPT-PERSISTENCE-08` | durable lifecycle/audit and database immutability | PASS |
| `IT-DB-MIGRATE-01` | 20-table apply/rollback/recreate | PASS |

Focused commands executed on 2026-08-13:

```text
dotnet test tests/Ivr.UnitTests/Ivr.UnitTests.csproj -c Release --filter "TestId~UT-SCRIPT"
PASS: 10/10

dotnet test tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj -c Release --filter "TestId~IT-SCRIPT|TestId=IT-DB-MIGRATE-01"
PASS: 3/3
```

Final local gate:

```text
locked restore PASS
Release build PASS — 0 warnings / 0 errors
dotnet format verify PASS
contract 19 + unit 67 + integration 31 = 117/117 PASS
TOTAL_LINE_COVERAGE=94.71% (14435/15241, 3 reports)
EF pending model changes: none
CI config, OpenAPI lint/parse/schema/hash/negative and API docs PASS
admin-ui lint + production build PASS
NuGet HIGH and both npm HIGH vulnerability gates PASS (0 findings)
Compose config, Gitleaks worktree and locale-stable PII scan PASS
Markdown map: 411 files / 375 resolved links / 0 unresolved
```

This evidence must not be interpreted as hosted, LAB, real-call or production evidence.

## Artifacts

- Approved synthetic MOCK fixture: `approved-mock-fixture.json`
- Vietnamese golden preview: `golden-preview.vi-VN.txt`
- [Privacy test report](privacy-test-report.md)
- Canonical executable fixture: `seed/ivr-menu.sample.json`

## Residual gates

- `OD-V1-15`: Product + Privacy/Legal decision for reading Target V1 items and short delivery area in production remains `OWNER_DECISION_REQUIRED`.
- `W-0003`: real Sales/customer data is not connected; synthetic fixtures do not close it.
- LAB physical SIM/eSIM, vendor TTS/audio pronunciation, real destination allowlist and customer calls are `NOT_RUN`.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`; recording remains OFF; IVR sends no customer SMS/notification.
