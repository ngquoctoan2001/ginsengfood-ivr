# IVR Master Implementation Progress Ledger

Trạng thái: `ACTIVE_SINGLE_SOURCE` · Cập nhật: `2026-08-12`
Scope: mọi planned work, unplanned work, dependency, decision, implementation, test, evidence và acceptance của IVR.

> **Không tạo tracker/backlog thứ hai.** File này là sổ tiến độ duy nhất. Không xóa lịch sử; sửa factual error bằng một Activity entry mới. Khi bảng dài, tiếp tục append, không tách file.

## 1. Operating rules

1. Work ID toàn cục dạng `W-0001`, tăng tuần tự. `NEXT_WORK_ID` ở §2 phải được tăng ngay khi cấp ID.
2. Mỗi prompt dùng Work ID đã dành ở §5. Việc phát sinh ngoài plan dùng ID kế tiếp, `Origin=UNPLANNED`, chen vào đúng thứ tự phát sinh chứ không tạo danh sách phụ.
3. Trước khi làm: ghi owner, baseline, dependency, acceptance, `IN_PROGRESS` và Activity `START`.
4. Trong khi làm: append `CHECKPOINT`, `DISCOVERY`, `DECISION_REQUEST`, `BLOCKED` hoặc `SCOPE_CHANGE`.
5. Sau khi làm: ghi files/artifacts, command+result, evidence, residual gates, next action và Activity `FINISH/HANDOFF`.
6. Không chuyển `ACCEPTED` nếu evidence chưa được reviewer/owner chấp nhận. Mock/lab/real evidence không thay thế nhau.
7. External item chỉ đóng khi có artifact thật (OpenAPI, signed decision, credential test, lab report…), không đóng vì IVR đã mock.

Status: `PLANNED`, `NOT_STARTED`, `IN_PROGRESS`, `CODE_DONE`, `TESTS_PASS`, `EVIDENCE_SUBMITTED`, `ACCEPTED`, `BLOCKED_INTERNAL`, `BLOCKED_EXTERNAL`, `DEFERRED_TARGET`, `N/A`, `CANCELLED`.

## 2. Ledger control

| Field | Value |
| --- | --- |
| `NEXT_WORK_ID` | `W-0061` |
| Last allocated | `W-0060` |
| Last activity sequence | `A-0003` |
| Contract state | `TARGET_CONTRACT_V1=DRAFT` |
| Default mode | `MOCK` |
| Real customer calls | `NO` |
| Current lab target | 1 real SIM + destination allowlist |
| Production telephony target | 32 eSIM channels, configurable |

## 3. Current gate snapshot

| Gate ID | Gate | Owner | Status | Mock/parallel path | Closure evidence |
| --- | --- | --- | --- | --- | --- |
| `G-CONTRACT` | Sales task/callback Target V1 | Sales API/Core | BLOCKED_EXTERNAL | fake Sales + WireMock | approved OpenAPI + CDC tests |
| `G-SPEECH` | privacy-safe order summary | Sales/Product/Privacy | BLOCKED_EXTERNAL | fake summaries/renderer | schema/examples/privacy approval |
| `G-DIAL` | dial-token issue/resolve | Sales/Security/Telephony | BLOCKED_EXTERNAL | fake resolver | threat model/API/tests |
| `G-AUTH` | production JWT/mTLS profile | Security/Platform | BLOCKED_EXTERNAL | mock JWT | auth profile + sandbox credential/tests |
| `G-POLICY` | production attempt policy | Product/Core | BLOCKED_EXTERNAL | candidate mock-lab-v1 | signed policy/version |
| `G-LAB-SIM` | one real SIM lab | Infra/vendor | BLOCKED_EXTERNAL | mock SIM | lab report/allowlist/kill-switch evidence |
| `G-ESIM32` | 32 eSIM capacity | Infra/procurement | BLOCKED_EXTERNAL | load simulator | procurement + measured capacity/failover |
| `G-LEGAL` | script/privacy/retention/legal | Legal/Privacy | BLOCKED_EXTERNAL | recording off/redaction | signed review |
| `G-RELEASE` | production release | Release owner | BLOCKED_EXTERNAL | none | accepted go/no-go/evidence |

## 4. External request register

| Work ID | Request | Owner | Status | Exact deliverable requested | IVR fallback | Next action |
| --- | --- | --- | --- | --- | --- | --- |
| `W-0002` | Program/task producer | Sales/Product | BLOCKED_EXTERNAL | GH ONLINE + 24/7 COD matrix, callable states, required flag, task OpenAPI/tests | fake producer | send Target V1 pack |
| `W-0003` | Speech payload | Sales/Product/Privacy | BLOCKED_EXTERNAL | schema + samples + item/area rules | fake summary fixtures | request design review |
| `W-0004` | Dial token | Sales/Security/Telephony | BLOCKED_EXTERNAL | issue/resolve/TTL/one-use contract | fake resolver | request threat model/API |
| `W-0005` | Callback/revalidation | Sales API/Core | BLOCKED_EXTERNAL | target endpoint, DTO, ACK, version/idempotency/timeout tests | target WireMock + GH compat | request OpenAPI review |
| `W-0006` | Auth | Security/Platform | BLOCKED_EXTERNAL | JWT issuer/audience/scope/TTL/JWKS and mTLS decision | mock JWT | request auth profile |
| `W-0007` | Attempt policy | Product/Core | BLOCKED_EXTERNAL | approved version/max/window/offsets | candidate MOCK/LAB only | owner sign-off |
| `W-0008` | Telephony lab/32 eSIM | Infra/vendor | BLOCKED_EXTERNAL | 1 SIM lab protocol + future 32 eSIM capacity/caller-ID/cost | mock SIM/load model | procure/test |
| `W-0009` | Legal/release | Legal/Privacy/Release | BLOCKED_EXTERNAL | script, retention, legal basis, pilot/go-live approval | recording off/no customers | start review early |

## 5. Planned implementation register

Every row is planned work. Detailed build/test/evidence requirements live in the linked prompt and specs; actual results must be written back here.

| Work ID | Prompt | Scope summary | Prereq | Status | Owner | Artifacts/PR | Tests/evidence | Residual/next |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `W-0001` | Planning realignment | Target V1 plan/spec/prompt/tracker/OpenAPI | docs/code review | EVIDENCE_SUBMITTED | Codex + IVR owner | Target V1 draft, two OpenAPI files, 51-prompt register, fake seed | JSON/YAML/schema/ref/link/tracker/diff checks pass | owner reviews defaults; start W-0010 |
| `W-0010` | `P0-1` | repo/solution bootstrap | owner defaults | NOT_STARTED |  |  |  |  |
| `W-0011` | `P0-2` | CI/quality baseline | W-0010 | NOT_STARTED |  |  |  |  |
| `W-0012` | `P0-3` | config/auth/audit/idempotency/correlation | W-0010 | NOT_STARTED |  |  |  |  |
| `W-0013` | `P0-4` | mode/provider flags + kill switches | W-0012 | NOT_STARTED |  |  |  |  |
| `W-0014` | `P1-1` | both OpenAPI/codegen/contract scaffold | W-0010..12 | NOT_STARTED |  |  |  |  |
| `W-0015` | `P1-2` | PostgreSQL/EF migrations, versioned policy/speech snapshots | W-0012 | NOT_STARTED |  |  |  |  |
| `W-0016` | `P1-3` | domain/DTO/provider ports/privacy guards | W-0014,W-0015 | NOT_STARTED |  |  |  |  |
| `W-0017` | `P1-4` | API docs/versioning/drift portal | W-0014 | NOT_STARTED |  |  |  |  |
| `W-0018` | `P2-1` | task intake for both program/payment paths | W-0014..16 | NOT_STARTED |  |  |  |  |
| `W-0019` | `P2-2` | eligibility/blockers/fail-closed | W-0018 | NOT_STARTED |  |  |  |  |
| `W-0020` | `P2-3` | policy registry/scheduler/channel leases | W-0019 | NOT_STARTED |  |  |  |  |
| `W-0021` | `P2-4` | speech + dial-token + mock SIM adapter | W-0020 | NOT_STARTED |  |  |  |  |
| `W-0022` | `P2-5` | DTMF/disposition normalizer | W-0021 | NOT_STARTED |  |  |  |  |
| `W-0023` | `P2-6` | target callback/outbox + GH compat | W-0022 | NOT_STARTED |  |  |  |  |
| `W-0024` | `P2-7` | script/content approval and safe variables | W-0018 | NOT_STARTED |  |  |  |  |
| `W-0025` | `P3-1` | Next.js/RBAC/API client/mode banners | W-0012,W-0018 | NOT_STARTED |  |  |  |  |
| `W-0026` | `P3-2` | dashboard/log/detail masked evidence | W-0025 | NOT_STARTED |  |  |  |  |
| `W-0027` | `P3-3` | config/integration/provider/channel/roles UI | W-0026 | NOT_STARTED |  |  |  |  |
| `W-0028` | `P3-4` | privacy-safe reporting/analytics | W-0026,W-0055 | NOT_STARTED |  |  |  |  |
| `W-0029` | `P4-1` | real Sales provider wiring and CDC | W-0023,W-0002..6 | BLOCKED_EXTERNAL |  |  |  | build adapters with mocks first |
| `W-0030` | `P4-2` | Sales-owned blocker contract validation | W-0019 | NOT_STARTED |  |  |  | IVR does not directly own ops transition |
| `W-0031` | `P4-3` | CRM/voice restriction provider wiring | W-0019 | NOT_STARTED |  |  |  |  |
| `W-0032` | `P4-4` | production service auth/audit federation | W-0012,W-0006 | BLOCKED_EXTERNAL |  |  |  | mock JWT first |
| `W-0033` | `P4-5` | notification disabled/no-op boundary | W-0031 | DEFERRED_TARGET |  |  |  | no customer message in V1 |
| `W-0034` | `P4-6` | optional opt-out feedback review loop | W-0031 | DEFERRED_TARGET |  |  |  | cannot alter consent automatically |
| `W-0035` | `P5-1` | unit/integration/Testcontainers suite | W-0018..24 | NOT_STARTED |  |  |  |  |
| `W-0036` | `P5-2` | OpenAPI/CDC/E2E both modes/programs | runtime/UI | NOT_STARTED |  |  |  |  |
| `W-0037` | `P5-3` | performance/security/privacy/mode-isolation | runtime/providers | NOT_STARTED |  |  |  |  |
| `W-0038` | `P5-4` | code review/static/security gates | W-0011 | NOT_STARTED |  |  |  |  |
| `W-0039` | `P5-5` | UI accessibility/i18n/visual QA | W-0025..28 | NOT_STARTED |  |  |  |  |
| `W-0040` | `P6-1` | redacted telemetry/tracing/metrics | runtime | NOT_STARTED |  |  |  |  |
| `W-0041` | `P6-2` | dashboards/SLO/alerts/readiness | W-0040 | NOT_STARTED |  |  |  |  |
| `W-0042` | `P6-3` | chaos/fail-closed/recovery exercises | W-0041,providers | NOT_STARTED |  |  |  |  |
| `W-0043` | `P7-1` | Docker/Compose incl fake Sales/mock SIM/JWT | runtime/UI | NOT_STARTED |  |  |  |  |
| `W-0044` | `P7-2` | Helm/K8s/modes/netpol/retention | W-0043,W-0041 | NOT_STARTED |  |  |  |  |
| `W-0045` | `P7-3` | CI/CD/promotions/evidence | W-0044,W-0038 | NOT_STARTED |  |  |  |  |
| `W-0046` | `P7-4` | canary/rollback/flag ramp | W-0045 | NOT_STARTED |  |  |  |  |
| `W-0047` | `P7-5` | secret/token/cert rotation | W-0044,W-0032 | NOT_STARTED |  |  |  |  |
| `W-0048` | `P8-1` | real vendor adapter + **1 SIM lab** | W-0021,W-0008 | BLOCKED_EXTERNAL |  |  |  | allowlist only |
| `W-0049` | `P8-2` | lab runbook/evidence, no customers | W-0048,W-0045 | BLOCKED_EXTERNAL |  |  |  | keep REAL_CUSTOMER_CALL_ALLOWED=NO |
| `W-0050` | `P9-1` | production release gate execution | real integrations/all gates | BLOCKED_EXTERNAL |  |  |  | 32 eSIM/legal/sign-off |
| `W-0051` | `P9-2` | cutover/rollback/ops/hypercare | W-0050 | BLOCKED_EXTERNAL |  |  |  |  |
| `W-0052` | `P10-1` | privacy/legal/script/DSAR | foundation/contracts | NOT_STARTED |  |  |  | owner/legal inputs |
| `W-0053` | `P10-2` | data governance/backup/DR | DB/deploy | NOT_STARTED |  |  |  |  |
| `W-0054` | `P10-3` | 1/32-channel capacity and cost model | metrics/load | NOT_STARTED |  |  |  | measured data needed |
| `W-0055` | `P10-4` | analytics pipeline | telemetry | NOT_STARTED |  |  |  |  |
| `W-0056` | `P10-5` | SLA/error budget/on-call | observability/ops | NOT_STARTED |  |  |  |  |
| `W-0057` | `P11-1` | telephony RFQ + one-SIM lab + 32-eSIM requirements | start early | NOT_STARTED |  |  |  | owns W-0008 closure |
| `W-0058` | `P11-2` | Sales/auth contract closure pack | W-0014 | NOT_STARTED |  |  |  | owns W-0002..7 closure |
| `W-0059` | `P11-3` | legal/retention/script/release package | W-0052,W-0053 | NOT_STARTED |  |  |  | owns W-0009 inputs |
| `W-0060` | `P11-4` | readiness board/evidence/go-no-go | continuous | NOT_STARTED |  |  |  | mirrors, does not replace this ledger |

## 6. Unplanned work insertion template

Copy a row into §5 using `NEXT_WORK_ID`, then increment control immediately:

| Work ID | Origin | Discovered by/date | Problem/scope | Priority/impact | Owner | Dependencies/mock | Acceptance/evidence | Status/next |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `W-XXXX` | `UNPLANNED` | prompt/incident + timestamp | exact work | P0/P1/P2 + affected gate | named owner | links | commands/artifacts/reviewer | status |

Never reuse or renumber an issued ID, even if cancelled.

## 7. Activity log (append-only)

| Seq | Timestamp | Work ID | Event | Detail | Actor | Evidence/result |
| --- | --- | --- | --- | --- | --- | --- |
| `A-0001` | 2026-08-12 | `W-0001` | START | Rà soát và realign plan/spec/prompt theo Sales dev answers và owner direction | Codex | baseline review |
| `A-0002` | 2026-08-12 | `W-0001` | CHECKPOINT | Target V1 draft, plan/spec/integration/OpenAPI/governance/tracker đang được cập nhật; validation chưa chạy | Codex | working tree |
| `A-0003` | 2026-08-12 | `W-0001` | HANDOFF | Hoàn tất realignment tài liệu; chưa khóa contract và chưa triển khai runtime | Codex | JSON 14/14; YAML 2/2; local refs pass; target seeds 2/2; Markdown links 0 unresolved; prompt IDs 51/51; git diff check pass |

## 8. Per-work completion record template

Append one block when a work item reaches a handoff/status boundary:

```text
Work ID:
Baseline/commit:
Scope completed:
Files/artifacts:
Commands and exact results:
Tests/evidence:
Review/acceptance by:
Mock-only evidence:
Lab evidence:
Real integration evidence:
Production evidence:
Residual blockers/risks:
Next allowed Work ID(s):
Final status:
```

## 9. Completion records

```text
Work ID: W-0001
Baseline/commit: Sales PhucApu@a3aad246d986fbc273cf41aaa93eec6659669656; IVR main@ab7de4d59eb04eb9f172385a1ffa4d25023064e5; documentation working tree 2026-08-12
Scope completed: realign Target V1 plan, specs, API contracts, integration requests, prompts, execution governance and the single progress ledger
Files/artifacts: plan/ivr-orther/target-contract-v1-draft.md; specs/api/openapi/*.yaml; seed/sales-target-v1.sample.json; prompt/00-index.md; prompt/RUNBOOK-execute-prompts.md; this ledger
Commands and exact results: JSON parse 14/14; YAML parse 2/2; OpenAPI local refs PASS; target seed schema 2/2; Markdown files 381 and unresolved links 0; prompt IDs 51/51 with no tracker omissions; W-0001..W-0060 no gaps; git diff --check PASS
Tests/evidence: documentation/contract static validation only; no runtime implementation, build, real API, telephony or production evidence
Review/acceptance by: pending IVR owner and Sales/Core reviewers
Mock-only evidence: canonical fake Sales fixtures prepared for GOLDEN_HOUR/ONLINE and TWENTY_FOUR_SEVEN/COD
Lab evidence: NOT_RUN; requires approved destination allowlist and one real SIM
Real integration evidence: NOT_RUN; blocked by W-0002..W-0008
Production evidence: NOT_RUN; requires 32 eSIM readiness and all release gates
Residual blockers/risks: Target Contract V1 remains DRAFT; attempt policy, Sales endpoints/payload, dial-token, auth, legal and telephony inputs remain external
Next allowed Work ID(s): W-0010; W-0057 and W-0058 may start early in parallel when owners are available
Final status: EVIDENCE_SUBMITTED
```
