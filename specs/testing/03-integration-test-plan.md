# TEST-03 — Integration Test Plan

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p11` · Mock Order Core/ops/SIM qua `seed/*` + `integration-status.sample.json`.

## 1. Task → CallJob → Attempt (mock Order Core push)
| ID | Given (seed) | Then |
| --- | --- | --- |
| IT-01 | TASK-0001 (GH sellable) | intake accept → CallJob → attempt 1 dispatch (MOCK) |
| IT-02 | TASK-0002 (24-7) | schedule [0,450], window 900 |
| IT-03 (neg) | TASK-0006 (recall snapshot) | `TASK_BLOCKED_OPERATIONAL`, không dispatch |
| IT-04 (neg) | TASK-0008 (call_restriction) | block (do-not-call) |
| IT-05 (neg) | TASK-0010 (draft) | reject NOT_OFFICIAL_ORDER |
| IT-05a (neg) | order state ≠ `CONFIRMING` | reject `STATE_NOT_CALLABLE` (DS-01) |
| IT-05b (neg) | order `payment_method_snapshot` ≠ COD | reject `STATE_NOT_CALLABLE` — **IVR chỉ đơn COD** (DS-01) |

## 2. Callback → Order Core revalidate (mock Core)
| ID | Given | Then |
| --- | --- | --- |
| IT-06 | SCN-001 confirm | Core `ACCEPTED_FOR_REVALIDATION` (revalidate ≤3–5s) |
| IT-07 (neg) | SCN-009 race: phím 1 + recall lúc revalidate | Core `BLOCKED_BY_CORE`, không confirm (D-06) |
| IT-08 (neg) ⏳**target** | order_version mismatch (ORD-0005 v3 vs task v1) | **target** `REJECTED_STALE`, không transition (IR-SALES-OC1). ⚠️ Core **hiện chưa** check `order_version` (DS-04) → **deferred** tới khi Core expose; nay Core chỉ block qua state+COD+sellable revalidate |
| IT-09 | callback timeout | retry bounded cùng idempotency (D-04) |

## 3. Blocker fan-out (mock ops sellable gate)
| ID | Given | Then |
| --- | --- | --- |
| IT-10 | order nhiều line, 1 line NOT_SELLABLE | Core fan-out phát hiện → block (DO-CORR-1) |
| IT-11 | webhook `sku-became-not-sellable` (dedupe EventId) | hold sớm (optional), không thay revalidate (DO-04) |

## 4. Fail-closed (integration-status profiles)
| ID | Profile | Then |
| --- | --- | --- |
| IT-12 (neg) | STATUS-order-core-down | không tạo task mới; callback retry/admin review |
| IT-13 (neg) | STATUS-ops-down | không dispatch / Core block (DO-06) |
| IT-14 (neg) | STATUS-ops-ready-503 | coi không xác thực → fail-closed |
| IT-15 (neg) | STATUS-crm-down | không dispatch (DC-01 fail-closed) |
| IT-16 (neg) | STATUS-evidence-down | không final-callback → hold |
| IT-17 (neg) | STATUS-sim-down | `IVR_TECHNICAL_EXCEPTION` (không no-answer) |

## Báo cáo
19 integration case; mọi fail-safe profile (6) có test; race + stale + fan-out + **COD-only gate (DS-01)** phủ. IT-08 (order_version stale) = **target/deferred** cho tới khi Core expose `order_version` (DS-04 / IR-SALES-OC1).
