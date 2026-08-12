# API-08 — External API Needs (contract IVR cần từ team khác)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: decisions-log D-*/DO-*/DF-*, DC-01/IR-CRM-01.
Đây là **con trỏ** sang `integration-requirements/*` (sẽ sinh ở p09). Khác bản draft ban đầu: phần lớn contract **đã KHÓA** (không còn "chưa tồn tại").

## 1. Order Core (Commerce, module 3) — ✅ đã khóa
| Cần | Contract/endpoint | Quyết định |
| --- | --- | --- |
| Push task sang IVR | `POST /v1/ivr/order-confirmation/tasks` (Core gọi) | D-03 |
| Nhận callback từ IVR | `POST {orderCore}/v1/orders/{order_id}/ivr-result-callbacks` → response codes | D-04 |
| Cấp order state cho IVR | current: `order_state`(đục) + COD gate (`payment_method_snapshot=COD`) + optional derived `is_ivr_callable`; target IR-SALES-OC1: `order_version` | D-02/DS-01/DS-04 |
| order_code lifecycle | cấp khi tạo Official Order; fulfillment gated | D-01 |
| Official contact + dial_token | `phone_ref`/`phone_masked`/`phone_validation_status`/`dial_token` | D-05 |
| Fan-out blocker | Core gọi ops sellable gate per-line, nhúng `sellable_status[]` + revalidate | DO-CORR-1/DO-02/DO-03 |

## 2. Sales Extensions (module 3.1) — ✅ đã khóa
| Cần | Contract | Quyết định |
| --- | --- | --- |
| IVR-required decision | event `order.ivr_required_decisioned` (Core tạo task; IVR không nhận trực tiếp) | D-09 |
| Quota release Golden Hour | IVR chỉ signal qua callback; QuotaReleaseGuard (Sales) thực hiện | D-11 |
| Trust/skip + risk flags | Customer Trust Resolver → task fields | D-12/D-13 |

## 3. Ops-Core (module 1/2) — ✅ đã khóa (do **Order Core** gọi, không phải IVR)
| Cần | Endpoint | Quyết định |
| --- | --- | --- |
| Blocker gộp (sellable gate) | `POST /api/v1/admin/availability/check` → `SellableStatus` | DO-01 |
| Snapshot per-line + `captured_at` | Core fan-out, nhúng vào task | DO-02 |
| Revalidate realtime | Core gọi (service-cred `SellableCheck`/`RecallHoldView`) | DO-03 |
| Chi tiết lock/recall (evidence) | `GET /v1/sale-locks/{id}`, `GET /v1/recall-cases/{id}` | DO-07 |
| Hold sớm (optional) | webhook `ops-core.sellable.sku-became-not-sellable.v1` | DO-04 |
| Fail-closed | `/health/ready`(503) + error codes | DO-06 |

## 4. CRM / business-platform — ✅ source resolved (DC-01), P1 build IR-CRM-01
| Cần | Trạng thái |
| --- | --- |
| **do-not-call / opt-out / call-restriction** (blocker thương mại) | ✅ **DC-01/Q-C1 resolved** — endpoint `crm-ads-eligibility` PHONE_CALL có thể dùng `eligible` để block cơ bản. Còn **IR-CRM-01 P1**: rich fields `do_not_call/opt_out_scope/reason/effective_at` + Core wiring `call_restriction`. |
| Nhận outcome event sau Core decision | ⏳ QC5 (IVR không ghi CRM — D-14) |

## 5. Foundation — ✅ đã khóa
Auth/allowlist (DF-06), RBAC `IVR_*` (DF-01), OpenAPI (DF-02), idempotency/audit (DF-04), correlation/outbox (DF-05), release gate (DF-03). Retention DF-07 ⏳ (Legal).

## Báo cáo (external)
- **External contract đã khóa:** Order Core (6), Sales 3.1 (3), Ops-core (6), Foundation (6), CRM source DC-01. **P0 còn mở:** SIM procurement/protocol (DT-01/04/06) + release sign-off (DF-03). CRM rich do-not-call còn **IR-CRM-01 P1**. Chi tiết → `integration-requirements/*` (p09).
