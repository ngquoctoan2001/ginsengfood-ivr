# TEST-02 — Unit Test Plan

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p11` · Theo service block `architecture/02`; FR ở `functional/*`.
Mỗi ca có **PASS** và **negative**.

## 1. Task Intake
| ID | Given | Then |
| --- | --- | --- |
| UT-INTAKE-01 | Golden Hour task hợp lệ (max_attempts=2, window=300, spacing=150) | CallJob tạo, 2 attempt schedule [0,150] |
| UT-INTAKE-02 | 24/7 task hợp lệ (window=900, spacing=450) | CallJob 2 attempt [0,450] |
| UT-INTAKE-03 (neg) | `program=GOLDEN_HOUR` nhưng `max_attempts=3` | `409 IVR_POLICY_MISMATCH` (D-10) |
| UT-INTAKE-04 (neg) | entity không `CONFIRMING` hoặc không `COD`; nếu `is_ivr_callable=false` được gửi thì reject như derived mismatch | `422 IVR_NOT_OFFICIAL_ORDER`/`STATE_NOT_CALLABLE` |
| UT-INTAKE-05 (neg) | thiếu `idempotency_key`/`correlation_id` | `422 IVR_MISSING_TRACE` |
| UT-INTAKE-06 | same key + same payload | trả kết quả cũ (idempotent) |
| UT-INTAKE-07 (neg) | same key + khác payload | `409 IVR_IDEMPOTENCY_CONFLICT` |
| UT-INTAKE-08 (neg) | caller không allowlist | `403 IVR_FORBIDDEN_CALLER` |

## 2. Eligibility Resolver
| ID | Given | Then |
| --- | --- | --- |
| UT-ELIG-01 | TRUSTED + trusted_skip_allowed + no risk/blocker (SCN-010) | `TASK_SKIPPED_TRUSTED_CUSTOMER` |
| UT-ELIG-02 (neg) | TRUSTED nhưng có risk_flag (CUST-004) | KHÔNG skip, vẫn gọi (D-12) |
| UT-ELIG-03 (neg) | `call_restriction=true` (SCN-012) | `TASK_BLOCKED_OPERATIONAL` (do-not-call) |
| UT-ELIG-04 (neg) | sellable_status có `recall_hold/sale_lock/BLOCKED` (SCN-008) | `TASK_BLOCKED_OPERATIONAL` |
| UT-ELIG-05 (neg) | `phone_validation_status=INVALID` (SCN-005) | không dispatch → invalid phone path |

## 3. Scheduler / Attempt Policy (D-10)
| ID | Given | Then |
| --- | --- | --- |
| UT-SCH-01 | A1 no-answer | A2 lên lịch đúng interval (½ window) |
| UT-SCH-02 (neg) | đã đủ 2 attempt | **không** tạo attempt 3 (D-10) |
| UT-SCH-03 | rolling queue nhiều job | ưu tiên near-expiry/GH/attempt2/risk; không batch cuối phiên |
| UT-SCH-04 | technical retry | `is_counted_customer_attempt=false`; không tăng attempt_number |

## 4. Result Normalizer (DT-02)
| ID | Given | Then |
| --- | --- | --- |
| UT-NORM-01 | answered+`1` | `IVR_CONFIRMED` (counted, final) |
| UT-NORM-02 | answered+`0` | `IVR_CUSTOMER_CANCELLED` |
| UT-NORM-03 | busy / rejected | `IVR_NO_ANSWER` (counted) — **không** cancel |
| UT-NORM-04 (neg) | sim_error/audio/dtmf error (SCN-006) | `IVR_TECHNICAL_EXCEPTION` (**không** no-answer) |
| UT-NORM-05 (neg) | unreachable/sai số (SCN-005) | `IVR_INVALID_PHONE_FINAL` (**không** no-answer) |
| UT-NORM-06 | answered+`9` | `IVR_WRONG_INPUT` (KEY_9 not-enabled) |

## 5. Callback Adapter / Idempotency
| ID | Given | Then |
| --- | --- | --- |
| UT-CB-01 | result final | callback payload current đủ field; target variant thêm `order_version_seen_by_ivr` |
| UT-CB-02 (neg) | duplicate callback (SCN-011) | trả ack cũ, không transition mới |
| UT-CB-03 | core trả TECHNICAL_RETRY_ALLOWED | retry bounded cùng idempotency key |
| UT-CB-04 (neg) | thiếu evidence_ref | không final-callback → hold/review |

## Báo cáo
~30 unit case; mỗi nhóm có negative; P0 (max-attempt, technical≠no-answer, invalid≠no-answer, idempotent) đủ.
