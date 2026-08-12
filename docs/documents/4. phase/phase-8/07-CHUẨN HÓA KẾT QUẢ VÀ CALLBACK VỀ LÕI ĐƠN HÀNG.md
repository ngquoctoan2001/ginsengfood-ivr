# IVR-07 - Result Normalization / IVR -> Order Core Callback

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa result taxonomy, callback contract và race-condition revalidation.

## 1. Mục tiêu

IVR Result Normalizer chuyển raw SIM/DTMF result thành outcome chuẩn và gửi callback về Order Core. Order Core luôn revalidate trước khi transition. IVR không được ép trạng thái đơn.

## 2. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | IVR result là input signal. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Outcome classification và callback boundary. |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md` | Order Core/Payment/Verified Revenue boundary. |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md` | Race condition và callback contract baseline. |

## 3. Tổng quan contract

| Thuộc tính | Giá trị |
| --- | --- |
| Contract name | `IvrConfirmationResultCallbackV1` |
| Direction | IVR Runtime -> Order Core |
| Producer | IVR Runtime Orchestrator |
| Consumer | Commerce Order Core / Order State Machine |
| Role | Result signal only |
| Idempotency | Bắt buộc |
| Correlation | Bắt buộc |

## 4. Trường dữ liệu bắt buộc

| Field | Required | Ghi chú |
| --- | --- | --- |
| `callback_id` | Có | Unique callback. |
| `task_id` | Có | Link task gốc. |
| `call_job_id` | Có điều kiện | Có nếu đã tạo CallJob. |
| `attempt_id` | Có điều kiện | Có với attempt-level result. |
| `idempotency_key` | Có | Chống duplicate transition. |
| `correlation_id` | Có | Trace. |
| `order_id` | Có | Order gốc. |
| `order_version_seen_by_ivr` | Có | Race guard. |
| `program_code` | Có | Context chương trình. |
| `attempt_policy_code` | Có | Context policy. |
| `attempt_no` | Có điều kiện | Required cho attempt result. |
| `max_attempts` | Có | 2 hoặc 3 theo chương trình. |
| `result_type` | Có | Outcome chuẩn. |
| `result_reason` | Có | Lý do chi tiết. |
| `dtmf_key` | Có điều kiện | `1`, `0`, invalid hoặc null. |
| `is_counted_customer_attempt` | Có | False cho technical failure. |
| `is_final_for_ivr` | Có | IVR có dừng không. |
| `recommended_core_action` | Có | Recommendation only. |
| `technical_error_code` | Có điều kiện | Required nếu technical exception. |
| `evidence_ref` | Có | Evidence Registry reference. |
| `audit_ref` | Có | Audit reference. |
| `privacy_policy_version` | Có | Privacy context. |
| `script_version` | Có | Script đã phát. |
| `created_at` | Có | Callback timestamp. |

## 5. Loại kết quả

| Result type | Ý nghĩa | Counted? | Final? |
| --- | --- | --- | --- |
| `IVR_CONFIRMED` | Khách bấm `1` | Có | Có |
| `IVR_CUSTOMER_CANCELLED` | Khách bấm `0` | Có | Có |
| `IVR_NO_ANSWER_ATTEMPT` | Attempt hợp lệ nhưng không nghe/không input | Có | Không nếu chưa max |
| `IVR_NO_ANSWER_FINAL` | No-answer sau max attempt | Có | Có |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | Hết window trước confirmation hợp lệ | Tùy attempt | Có |
| `INVALID_PHONE_FINAL` | Phone invalid theo policy | Không | Có |
| `IVR_TECHNICAL_EXCEPTION` | Lỗi kỹ thuật | Không | Tùy policy |
| `IVR_POLICY_BLOCKED` | Policy/source unavailable hoặc blocked | Không | Có hoặc review |
| `IVR_OPERATIONAL_BLOCKED` | Sale Lock/Recall/Suppression/call restriction | Không | Có |
| `IVR_CUSTOMER_NEEDS_SUPPORT` | Khách cần hỗ trợ nếu future key enabled | Có | Có/review |

## 6. Hành động khuyến nghị cho Core

`recommended_core_action` không phải command. Order Core phải revalidate.

Allowed values:

- `CORE_REVALIDATE_AND_CONFIRM_ORDER`.
- `CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST`.
- `CORE_REVALIDATE_AND_CANCEL_NO_ANSWER`.
- `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION`.
- `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW`.
- `CORE_IGNORE_STALE_CALLBACK`.
- `CORE_BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT`.

## 7. Result state machine

| State | Ý nghĩa | Next states |
| --- | --- | --- |
| `RESULT_NOT_NORMALIZED` | Chỉ có raw adapter result | `RESULT_NORMALIZED`, `RESULT_REJECTED_NEEDS_REVIEW` |
| `RESULT_NORMALIZED` | Outcome đã phân loại | `RESULT_EVIDENCE_PENDING`, `RESULT_REJECTED_NEEDS_REVIEW` |
| `RESULT_EVIDENCE_PENDING` | Chờ ghi evidence | `RESULT_READY_FOR_CALLBACK`, `RESULT_EVIDENCE_FAILED` |
| `RESULT_READY_FOR_CALLBACK` | Payload callback sẵn sàng | `RESULT_SENT_TO_CORE` |
| `RESULT_SENT_TO_CORE` | Đã gửi Core | `RESULT_CORE_ACCEPTED`, `RESULT_CORE_REJECTED_STALE`, `RESULT_CALLBACK_RETRY_PENDING` |
| `RESULT_CORE_ACCEPTED` | Core nhận hợp lệ | Terminal cho IVR |
| `RESULT_CORE_REJECTED_STALE` | Core reject do stale/version/state | Terminal cho IVR |
| `RESULT_CALLBACK_RETRY_PENDING` | Retry callback kỹ thuật | `RESULT_SENT_TO_CORE`, `RESULT_ADMIN_REVIEW_REQUIRED` |
| `RESULT_EVIDENCE_FAILED` | Không ghi evidence | `RESULT_ADMIN_REVIEW_REQUIRED` |
| `RESULT_ADMIN_REVIEW_REQUIRED` | Cần người review | Terminal đến khi admin xử lý |

## 8. Revalidation khi có race condition

Order Core phải revalidate khi nhận callback:

- Callback idempotency.
- `task_id`, `order_id`, `order_version_seen_by_ivr`.
- Order state hiện tại còn nhận IVR outcome không.
- Order amount/items/contact có thay đổi làm confirmation cũ không hợp lệ không.
- Sale Lock/Recall/Suppression/payment/availability hiện tại.
- Evidence/audit refs tồn tại.
- Result type khớp attempt policy.

Nếu khách bấm `1` nhưng Sale Lock/Recall/payment issue xuất hiện trước khi Core accept:

- IVR result vẫn là `IVR_CONFIRMED` như raw customer signal.
- Core action là block hoặc hold admin review.
- Order không được tiếp tục chỉ vì phím `1`.
- Evidence phải link cả IVR signal và blocker.

## 9. Tiêu chí chấp nhận

- Callback đủ dữ liệu để Core revalidate.
- Result taxonomy tách confirm/cancel/no-answer/invalid/technical/block.
- Stale callback không ép order transition.
- No-answer max không tự notification.

## 10. Result normalization mapping table

| Input từ adapter/service | Điều kiện | Normalized result | Counted? | Final? |
| --- | --- | --- | --- | --- |
| DTMF `1` | Answered, script valid, evidence recorded | `IVR_CONFIRMED` | Yes | Yes |
| DTMF `0` | Answered, script valid, evidence recorded | `IVR_CUSTOMER_CANCELLED` | Yes | Yes |
| No answer | Attempt valid, not max | `IVR_NO_ANSWER_ATTEMPT` | Yes | No |
| No answer | Attempt valid, max reached | `IVR_NO_ANSWER_FINAL` | Yes | Yes |
| Window expired | No accepted final signal | `IVR_CONFIRMATION_WINDOW_EXPIRED` | Depends | Yes |
| Phone invalid | Validation confirms invalid | `INVALID_PHONE_FINAL` | No | Yes |
| SIM/server/audio/DTMF technical error | Technical source | `IVR_TECHNICAL_EXCEPTION` | No | Depends/review |
| Capacity hold | Queue/SIM capacity unsafe | `IVR_CAPACITY_EXCEPTION` | No | Review |
| Sale Lock/Recall/payment issue | Core revalidation | `IVR_OPERATIONAL_BLOCKED` | No | Yes/review |
| Ambiguous raw status | No source-backed mapping | `IVR_CUSTOMER_NEEDS_SUPPORT` or review | Depends | Review |

## 11. Callback payload validation

Callback must include:

| Field | Validation |
| --- | --- |
| `callback_id` | Unique/idempotent. |
| `task_id` | Existing task. |
| `call_job_id` | Existing job if job was created. |
| `attempt_id` | Required for attempt-level outcome. |
| `order_id` | Matches task. |
| `order_version_seen_by_ivr` | Required race guard. |
| `program_code` | Matches task. |
| `attempt_no` | Within program policy. |
| `result_type` | Known result taxonomy. |
| `is_counted_customer_attempt` | False for technical/invalid/capacity. |
| `evidence_ref` | Required before final action. |
| `audit_ref` | Required. |
| `privacy_policy_version` | Required. |

## 12. Order Core callback response contract

| Core response | Meaning | IVR action |
| --- | --- | --- |
| `CALLBACK_ACCEPTED_FOR_REVALIDATION` | Core accepted signal. | Mark callback accepted; wait/read final order state if needed. |
| `CALLBACK_REJECTED_STALE` | Task/order version stale. | Mark stale; do not retry as new business signal. |
| `CALLBACK_BLOCKED_BY_CORE` | Sale Lock/Recall/payment/suppression/policy block. | Mark blocked; evidence link. |
| `CALLBACK_NEEDS_ADMIN_REVIEW` | Core requires owner/admin review. | Mark review required. |
| `CALLBACK_TECHNICAL_RETRY_ALLOWED` | Core unavailable/transient. | Retry bounded with same idempotency key. |
| `CALLBACK_TECHNICAL_RETRY_BLOCKED` | Retry unsafe or expired. | Admin review. |

## 13. Race condition matrix

| Race | Detection | Required behavior |
| --- | --- | --- |
| Order version changed after task | `order_version_seen_by_ivr` mismatch | Reject stale or re-evaluate by Core. |
| Customer presses `1`, Sale Lock appears | Core blocker check | Do not confirm; block/hold. |
| Customer presses `0`, order already cancelled | Order state check | Idempotent no-op or stale. |
| No-answer final, payment issue appears | Core revalidation | Core decides hold/cancel; IVR no direct action. |
| Duplicate callback | Idempotency | Return previous ack. |
| Evidence missing | Evidence check | Reject/hold/review. |

## 14. Callback retry policy

Retry is technical only. It must not:

- Create a new result.
- Increment customer attempt.
- Change result status.
- Bypass stale/order state guard.
- Retry forever.

Retry requires:

- Same `callback_id` or same idempotency scope.
- Retry counter.
- Last error.
- Next retry time.
- Admin escalation after max retry.

Exact retry count/backoff remains `Owner Decision Required`.

## 15. Result event publication rules

Events may be published for signal visibility:

- `ivr-confirmation-requested`.
- `ivr-confirmed`.
- `ivr-customer-cancelled`.
- `ivr-no-answer-final`.
- `ivr-invalid-phone-final`.
- `ivr-technical-exception`.
- `ivr-operational-blocked`.
- `ivr-capacity-incident-opened`.

Events must not:

- Represent final order state unless event name belongs to Order Core.
- Replace callback.
- Feed verified revenue.
- Trigger notification without Core decision.

## 16. Result normalization P0 tests

| Test ID | Scenario | Expected |
| --- | --- | --- |
| IVR07-P0-001 | DTMF `1` | Confirm signal only, Core revalidates. |
| IVR07-P0-002 | DTMF `0` | Cancel signal only, Core decides. |
| IVR07-P0-003 | SIM error | Technical exception, not no-answer. |
| IVR07-P0-004 | Invalid phone | Invalid final, not no-answer. |
| IVR07-P0-005 | Stale callback | No order transition. |
| IVR07-P0-006 | Missing evidence | Callback held/rejected. |
| IVR07-P0-007 | Operational block after key `1` | Core block, no auto confirm. |
