# REVIEW — Phase 0-11 Spec Alignment

Trạng thái: `REVIEW` · Ngày rà soát: 2026-07-06 · Phạm vi: toàn bộ prompt active `P0-*` đến `P11-*`, `prompt/00-index.md`, `prompt/README-governance.md`, OpenAPI/spec/database/testing/review register hiện hành.

## Kết luận
Bộ prompt **P0-P11 hiện đã khớp với spec hiện hành** để triển khai IVR từ zero đến production theo mô hình:
- P0-P7: build service, admin UI, quality, observability, deploy trong MOCK/fail-closed.
- P8-P9: real SIM pilot, release gate, cutover/ops.
- P10: compliance/data governance/capacity/SLA.
- P11: external closure cho SIM procurement, cross-team contracts, legal/retention/sign-off, readiness evidence.

Không còn conflict active trong prompt sau vòng vá này. Các mục còn `target`, `pending`, hoặc `NEED_CONFIRMATION` là deferred/owner-driven có chủ ý, không phải lệch spec.

## Spec Invariants Đã Đối Chiếu
| Invariant | Spec hiện hành | Prompt coverage | Kết quả |
| --- | --- | --- | --- |
| Attempt policy D-10 | `max_attempts=2` cả GH/24-7; GH 300/150; 24-7 900/450 | P1-2, P1-3, P2-1, P2-3, P5-*; negative tests reject `max_attempts=3` | PASS |
| Order callable | current = `order_state=CONFIRMING` + `payment_method_snapshot=COD`; `is_ivr_callable` optional derived | P2-1 patched to not require `is_ivr_callable`; P1/P4/P5 align | PASS |
| Callback current/target | current callback = 200/422, no `order_version_seen_by_ivr`; target OC1/OC2 adds version + `CALLBACK_*` | P1-1, P2-6, P4-1, P5-2, P11-2 split current/target | PASS |
| Q-C1 / do-not-call | Q-C1 resolved by DC-01; rich response/Core wiring remains IR-CRM-01 build item | P2-2, P4-3, P10-1, P11-2 keep fail-closed and build extension | PASS |
| DG-03 / order state | DG-03 resolved by DS-01..05; no-answer/technical do not transition order | P2-3, P2-6, P4-1, P5-2 align | PASS |
| Production gate | `REAL_CUSTOMER_CALL_ALLOWED=NO` until DF-03 and evidence; SIM/legal blockers explicit | P7/P8/P9 plus P11 closure; index/governance align | PASS |
| P3-4 prompt index | Reporting/analytics UI exists and is indexed | P3-4 + P10-4 dependency in `prompt/00-index.md` | PASS |

## Findings Đã Vá
| Severity | Finding | Files patched | Trạng thái |
| --- | --- | --- | --- |
| P1 | Nhiều `SOURCE SPECS` dùng đường dẫn rút gọn (`ui/...`, `workflows/...`, `05-resilience.md`) làm checker/agent resolve sai | P0-4, P1-1/2/3/4, P2-2/3/4/5/6, P3-1/2/3, P4-1, P5-1/2/3, P6-2, P7-2/4, P8-1, P9-2, P10-1/2/3/5, P11-4 | FIXED |
| P1 | `P2-1` wording có thể hiểu `is_ivr_callable` là required field dù spec current coi là optional derived | `prompt/phase-2-core-runtime/P2-1-task-intake.md` | FIXED |
| P2 | `P9-2` source trỏ `docs/pilot-runbook.md` khi file này là output tương lai, không phải source hiện có | `prompt/phase-9-release-ops/P9-2-cutover-ops-runbook.md` | FIXED |
| P2 | `specs/05-current-docs-review.md` còn dòng TODO đọc PACK-09 dù D-10 đã chốt attempt policy | `specs/05-current-docs-review.md` | FIXED |

## Verification Snapshot
- `NO_MISSING_SOURCE_REFS`: mọi path trong section `## 3. SOURCE SPECS` của 51 prompt active resolve được.
- `ALL_PROMPTS_HAVE_REQUIRED_SECTIONS`: mọi prompt active có đủ section 0-12 theo template.
- OpenAPI current/target có schema riêng: `IvrConfirmationResultCallbackCurrentV1`, `IvrConfirmationResultCallbackTargetV1`, `CallbackCoreResponseCurrent`, `CallbackCoreResponseTarget`.
- D-10 trong OpenAPI/DB spec: `max_attempts` enum/check = 2; window = 300/900; spacing = 150/450.
- `open-decisions-register`: Q-C1 và DG-03 resolved; P0 production vẫn bị chặn bởi DT-01/DF-03 và legal/retention evidence, được P11 prompt hóa.

## Residual Non-Conflicts
- `PENDING` còn lại cho SIM procurement, DF-07, OC1/OC2/OC3, DC-05/06, IR-CRM-01 là trạng thái owner/provider chưa giao, không phải mâu thuẫn prompt/spec.
- `NEED_CONFIRMATION` còn lại là lựa chọn toolchain/runtime (CI provider, secret store, codegen, observability backend, BI tool...) đã có default hợp lý; không chặn zero-to-production prompt flow.
- Các dòng nhắc rule cũ 2/10 hoặc 3/15 trong review/spec lịch sử chỉ dùng để ghi superseded/conflict history; prompt active và executable specs dùng D-10.

## Kết Luận Triển Khai
Agent triển khai theo `prompt/00-index.md` có thể đi từ repo trống tới production handoff nếu tuân thủ evidence gate. Không được tuyên bố production-ready chỉ vì code/test pass; production thật cần P11/P8/P9 evidence accepted, DT-01 procurement, DF-03 sign-off, và DF-07/legal retention closure.
