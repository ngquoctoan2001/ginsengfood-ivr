# FR — Result Normalization & Callback

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p03`
Nguồn: `phase-8/07` (result taxonomy, callback, revalidation, race), `docx` §13 (normalization), §14 (callback boundary).

**Actor:** IVR Result Normalizer → Core Callback Adapter → Order Core.
**Precondition:** Có raw call event / attempt outcome.
**Trigger:** Attempt kết thúc / window expired / blocker.
**Postcondition:** Result normalized + evidence; callback gửi Order Core; Order Core revalidate & quyết định.

## Result taxonomy (superset chuẩn hóa — OD-DR-04; canonical + alias)
| Result (canonical) | Alias | Ý nghĩa | Counted? | Final? |
| --- | --- | --- | --- | --- |
| `IVR_CONFIRMED` | — | Khách bấm `1` | Yes | Yes |
| `IVR_CUSTOMER_CANCELLED` | — | Khách bấm `0` | Yes | Yes |
| `IVR_NO_ANSWER_ATTEMPT` | `ATTEMPT_1_NO_ANSWER` | No-answer, chưa max | Yes | No |
| `IVR_NO_ANSWER_FINAL` | — | No-answer sau max | Yes | Yes |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | — | Hết window chưa xác nhận hợp lệ | Tùy | Yes |
| `IVR_INVALID_PHONE_FINAL` | `INVALID_PHONE_FINAL` | Phone invalid | No | Yes |
| `IVR_WRONG_INPUT` | `NO_VALID_INPUT` | Sai phím/không hợp lệ | Theo policy | No/review |
| `IVR_TECHNICAL_EXCEPTION` | — | Lỗi kỹ thuật | **No** | Tùy/review |
| `IVR_CAPACITY_EXCEPTION` | — | Nghẽn capacity | No | Review |
| `IVR_OPERATIONAL_BLOCKED` | — | Sale Lock/Recall/Suppression | No | Yes/review |
| `IVR_POLICY_BLOCKED` | — | Policy/source unavailable | No | Yes/review |
| `IVR_OPT_OUT` | — | Khách từ chối nhận gọi | — | Yes |
| `IVR_CUSTOMER_NEEDS_SUPPORT` | — | (KEY_9 future, chưa bật) | — | Review |

## Recommended core action (advisory only — Core revalidate) — phase-8/07 §6
`CORE_REVALIDATE_AND_CONFIRM_ORDER` · `..._CANCEL_CUSTOMER_REQUEST` · `..._CANCEL_NO_ANSWER` · `..._EXPIRE_CONFIRMATION` · `..._HOLD_ADMIN_REVIEW` · `CORE_IGNORE_STALE_CALLBACK` · `CORE_BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT`.

## FR
| ID | Yêu cầu | Nguồn | Acceptance hint |
| --- | --- | --- | --- |
| FR-IVR-RES-001 | Chuẩn hóa raw SIM/DTMF → result code + reason + confidence + evidence refs; **không** để raw provider event đi thẳng Order Core | docx §13,§10 | Callback dùng result chuẩn |
| FR-IVR-RES-002 | Callback **current** (`IvrConfirmationResultCallbackCurrentV1`) bắt buộc: `callback_id`, `task_id`, `order_id`, `result_type`, `is_counted_customer_attempt`, `is_final_for_ivr`, `recommended_core_action`, `evidence_ref`, `audit_ref`, `idempotency_key`, `correlation_id`. Callback **target** (`IvrConfirmationResultCallbackTargetV1`, IR-SALES-OC1) bổ sung `order_version_seen_by_ivr`. | phase-8/07 §4; docx §14; DS-03/04 | Current thiếu field bắt buộc → reject/hold; target thiếu `order_version_seen_by_ivr` → schema fail |
| FR-IVR-RES-003 | IVR **không** transition order; `IVR_CAN_DIRECTLY_CANCEL_ORDER=NO`, `CORE_STATE_MACHINE_CANCEL_REQUIRED=YES` | docx §14; phase-8/00 P0-002 | IVR/SIM đổi state → FAIL (P0-IVR-002) |
| FR-IVR-RES-004 | Order Core **revalidate** trước transition: idempotency, version, state còn nhận IVR, blocker hiện tại, evidence tồn tại, result khớp attempt policy | phase-8/07 §8,§11 | Callback thiếu evidence → hold/reject |
| FR-IVR-RES-005 | **Race**: phím `1` nhưng Sale Lock/Recall/version mismatch xuất hiện trước Core accept → result vẫn `IVR_CONFIRMED` (raw signal) nhưng Core **block/hold**, không auto-confirm; evidence link cả signal + blocker | phase-8/07 §8,§13 | Race → không confirm (P0-IVR-003) |
| FR-IVR-RES-006 | Callback response **current** = HTTP `200` accept / `422` invalid state-COD từ Core revalidation; bộ semantic `CALLBACK_ACCEPTED_FOR_REVALIDATION` / `CALLBACK_REJECTED_STALE` / `CALLBACK_BLOCKED_BY_CORE` / `CALLBACK_NEEDS_ADMIN_REVIEW` / `CALLBACK_TECHNICAL_RETRY_ALLOWED|BLOCKED` là **target** (IR-SALES-OC2). | phase-8/07 §12; /11 §7; DS-03 | Current `422` → không transition; target stale → không transition |
| FR-IVR-RES-007 | Callback retry = **kỹ thuật, bounded**, cùng idempotency key; không tạo result mới/không tăng attempt/không bỏ qua stale guard | phase-8/07 §14 | Retry vô hạn → FAIL |
| FR-IVR-RES-008 | No-answer final KHÔNG tự notification; chỉ Core quyết + owner notification | phase-8/07 §9; /02 | IVR tự gửi → FAIL (P0-IVR-008) |
| FR-IVR-RES-009 | Result state machine current: `NOT_NORMALIZED → NORMALIZED → EVIDENCE_PENDING → READY_FOR_CALLBACK → SENT_TO_CORE → CORE_ACCEPTED | CORE_REJECTED_422 | CALLBACK_RETRY_PENDING | ADMIN_REVIEW_REQUIRED`; target tách thêm `CORE_REJECTED_STALE` khi OC1/OC2 bật. | phase-8/07 §7; DS-03/04 | Chi tiết ở [../workflows/09-state-machines.md](../workflows/09-state-machines.md) |

## Owner Decision
- OD-DR-04 (chuẩn hóa taxonomy), Q-S1 (order state transition sau mỗi result type), OD-DR-05 (invalid phone → cancel/review).
