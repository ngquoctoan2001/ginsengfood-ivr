# W-0148 — M8-08 opt-out/suppression decision reconciliation

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

Trạng thái: **`EVIDENCE_SUBMITTED / M8_POSITION_SIGNED / CURRENT_LOOP_NOT_WIRED / EXPLICIT_ONLY_V1_PROPOSED / CRM_M3_LEGAL_SIGNOFF_REQUIRED / RUNTIME_NOT_AUTHORIZED`**

Người ký phía M8: **Tôi — Module 8 / Project Owner**.

## 1. Phạm vi

- Audit inbound `call_restriction` và outbound opt-out feedback loop.
- Đối chiếu result/review, threshold policy, queue proposer, DB/admin/retention, P4-6 và CRM/M3 ownership.
- Lập exact decision matrix/handoff; không sửa source/runtime/OpenAPI/migration.

## 2. Kết quả chính

- Inbound restriction đã chặn fail-closed trong intake/eligibility.
- `Rejected` vẫn là counted `NO_ANSWER` + review, không phải cancel/explicit opt-out.
- `OptOutSuppressionPolicy.Decide` và `QueueOnlySuppressionProposer` không có production caller;
  exact source search chỉ thấy definition và direct test caller. GitNexus query không có FTS nên
  direct source/current HEAD mới là authority.
- Không có signal counter/stable CRM key/orchestrator/delivery/ACK/reversal.
- Proposal `PENDING_CRM` không được `ReviewAsync` xử lý vì endpoint chỉ nhận `OPEN`; không có writer
  cho `ACCEPTED_BY_CRM`.
- Current retention chỉ anonymize review item đã resolved; queue hiện không có đường terminal nên
  chưa an toàn để wire.
- Cross-repo snapshot business-platform `PhucApu@a3aad246d986` có registry/read/user-consent
  primitives, nhưng chưa có signed service proposal contract cho M3/IVR.

## 3. Artifact

- [M8-08 decision pack](../../../plan/ivr-orther/m8-08-opt-out-suppression-decision-pack-2026-09-03.md)
- S-06/TODAY-01 và M8 worklist handoff
- IR-SALES-CRM-01 factual status correction
- tracker/readiness/gate mirror và official Markdown map

## 4. Verification

| Gate | Kết quả 03/09/2026 |
| --- | --- |
| Focused unit — opt-out policy, do-not-call supremacy, eligibility restriction, no-CRM-egress architecture | **PASS `8/8`** |
| Focused PostgreSQL integration — proposal persistence | **PASS_LOCAL_POSTGRES `2/2`** — W-0161 full integration `236/236`, 0 fail/skip; gồm `IT-OPTOUT-PROPOSE-03`, `IT-OPTOUT-FAILSAFE-04` |
| API docs selftest | **PASS** — `14` generated artifacts |
| OpenAPI validation | **PASS** — `2` file, `9` fixture, `12` schema-negative, `13` domain-negative, `1` compatibility fixture |
| Tracker/readiness mirror | **PASS** — `11` gate, `146` work item, `23` open decision; production flag `false` |
| Official Markdown map | **PASS** — `627` Markdown file; decision pack và W-0148 evidence đều `0` unresolved link |
| `git diff --check` | **PASS** — chỉ có line-ending conversion warnings của shared worktree |
| GitNexus advisory | Query không có FTS; exact source search chứng minh chỉ có definition + direct test caller |
| Source/OpenAPI/DB change thuộc W-0148 | **Không có** — decision/evidence only |

Ghi chú lịch sử: lần chạy W-0148 ban đầu dừng ở fixture. W-0161 đã chạy assertion thật qua local
Docker/Testcontainers; xem [evidence W-0161](../W-0161/README.md). Không suy PASS local thành CRM/M3
acceptance hoặc shared E2E.

## 5. Residual gates

- CRM/M3/Legal/Product/Security signatures và provenance: `NOT_RECEIVED`; chữ ký M8 đã có.
- Authoritative proposal/write schema, service auth và sandbox: `NOT_RECEIVED`.
- Runtime/orchestrator/migration: `NOT_STARTED / NOT_AUTHORIZED`.
- Shared E2E: `NOT_RUN`.

Không nâng `ACCEPTED`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Focused closure-path follow-up — W-0179

[W-0179](../W-0179/README.md) bổ sung positive end-to-end self-test riêng cho `D-03 / S-06` qua
nguyên W-0164/W-0165/W-0170: đủ 5 authority group và `OPT-01..OPT-11`; 6 mutation thiếu/sai
quorum, decision, batch, approval hoặc separation-of-duties đều bị từ chối. Đây là synthetic local
proof, không phải dispatch/receipt/signature thật và không đổi `RUNTIME_NOT_AUTHORIZED`.
