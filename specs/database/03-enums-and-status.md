# DB-03 — Enums & Status

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p07` · Nguồn: `phase-8/07`,`/12`; `api/*`; DT-02, D-10.

## 1. `program_type`
`GOLDEN_HOUR` (window 300s, spacing 150s, max 2) · `TWENTY_FOUR_SEVEN` (window 900s, spacing 450s, max 2). — D-10

## 2. `ivr-call-job-status`
`CREATED` → `ELIGIBILITY_RECHECK` → `QUEUED` → `SIM_RESERVED` → `DIALING` → `RESULT` → `CALLBACK` → `CLOSED`. Nhánh: `HELD_ADMIN_REVIEW`, `BLOCKED_OPERATIONAL`, `CAPACITY_HELD`, `DRY_RUN`.

## 3. `ivr-call-attempt-status`
`SCHEDULED` · `DISPATCHING` · `RINGING` · `ANSWERED` · `CAPTURING_DTMF` · `COMPLETED` · `NO_ANSWER` · `INVALID_PHONE` · `TECHNICAL_EXCEPTION` · `WINDOW_EXPIRED`.

## 4. `ivr-result-type` (taxonomy — functional/05 + DT-02)
`IVR_CONFIRMED` · `IVR_CUSTOMER_CANCELLED` · `IVR_NO_ANSWER_ATTEMPT` · `IVR_NO_ANSWER_FINAL` · `IVR_CONFIRMATION_WINDOW_EXPIRED` · `IVR_INVALID_PHONE_FINAL` · `IVR_WRONG_INPUT` · `IVR_TECHNICAL_EXCEPTION` · `IVR_CAPACITY_EXCEPTION` · `IVR_OPERATIONAL_BLOCKED`.

### Counted / Final
| Result | Counted | Final |
| --- | --- | --- |
| CONFIRMED / CUSTOMER_CANCELLED | có | có |
| NO_ANSWER_ATTEMPT | có | không |
| NO_ANSWER_FINAL / WINDOW_EXPIRED | có | có |
| INVALID_PHONE_FINAL | **không** | có |
| WRONG_INPUT | có | tùy attempt |
| TECHNICAL_EXCEPTION / CAPACITY_EXCEPTION | **không** | tùy/review |
| OPERATIONAL_BLOCKED | **không** | có/review |

## 5. `result_state` (callback state machine — phase-8/07 §7; current/target)
Current: `RESULT_NOT_NORMALIZED` → `RESULT_NORMALIZED` → `RESULT_EVIDENCE_PENDING` → `RESULT_READY_FOR_CALLBACK` → `RESULT_SENT_TO_CORE` → {`RESULT_CORE_ACCEPTED`, `RESULT_CORE_REJECTED_422`, `RESULT_CALLBACK_RETRY_PENDING`}; nhánh `RESULT_EVIDENCE_FAILED`, `RESULT_ADMIN_REVIEW_REQUIRED`.

Target IR-SALES-OC1/OC2: khi Core expose version + semantic callback codes, có thể tách `RESULT_CORE_REJECTED_STALE` (version guard) khỏi các reject/block khác.

## 6. Core callback response (D-04; current/target)
Current: lưu `core_http_status` ∈ {`200`, `422`}; `422` là Core revalidate fail-closed do state/COD/sellable hiện tại không còn hợp lệ.

Target `core_response_code` (IR-SALES-OC2): `CALLBACK_ACCEPTED_FOR_REVALIDATION` · `CALLBACK_REJECTED_STALE` · `CALLBACK_BLOCKED_BY_CORE` · `CALLBACK_NEEDS_ADMIN_REVIEW` · `CALLBACK_TECHNICAL_RETRY_ALLOWED` · `CALLBACK_TECHNICAL_RETRY_BLOCKED`.

## 7. `intake decision` (phase-8/04 §12)
`TASK_ACCEPTED_CALL_JOB_CREATED` · `TASK_ACCEPTED_DRY_RUN_ONLY` · `TASK_SKIPPED_TRUSTED_CUSTOMER` · `TASK_REJECTED_*` (NOT_OFFICIAL_ORDER/STATE_NOT_CALLABLE/POLICY_MISMATCH/CONTACT_INVALID/SCRIPT_NOT_APPROVED/INVALID_TRACE) · `TASK_BLOCKED_OPERATIONAL` · `TASK_HELD_ADMIN_REVIEW` · `TASK_HELD_POLICY_MISSING`.

## 8. `sim status`
`IDLE` · `RESERVED` · `ACTIVE_CALL` · `DISABLED` · `HEALTH_FAILED`. `adapter_mode`: `MOCK` | `REAL` (DT-01).

## 9. `SellableStatus.decision` (consume từ ops — DO-01)
`SELLABLE` · `NOT_SELLABLE` · `BLOCKED` · `UNKNOWN`. Cờ: `RecallHold`/`SaleLock`/`QualityHold`/`StockAvailable`/`BatchReleased`/`WarehouseReceiptConfirmed`/`HsdValid`/`TraceReady`.

## 10. Ops error codes (consume — DO-06)
`SALE_LOCK_ACTIVE` · `RECALL_IMPACT_ACTIVE` · `SELLABLE_GATE_BLOCKED` · `INVENTORY_NOT_SELLABLE` · `QUALITY_HOLD` · `TRACE_GAP_DETECTED` · `RATE_LIMITED` · `INTERNAL_ERROR`.
