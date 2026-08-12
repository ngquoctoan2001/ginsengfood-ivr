# DATA-02 — Mapping: Sales Platform (Order Core / Module 3 · 3.1)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p06` · Nguồn: `api/05-order-core-contracts`, `phase-8/04`,`/07`; D-01..D-14.
Chiều: **snapshot** = nhận trong `IvrConfirmationTaskV1`; **callback** = IVR gửi ra Order Core; **read** = đọc realtime.
Privacy: R=RESTRICTED · S=SENSITIVE · I=INTERNAL · P=PUBLIC-SAFE.

## 1. Task fields ↔ owner/source (Order Core → IVR, snapshot)
| IVR field | Owner/Resolver | Chiều | Privacy | Trạng thái |
| --- | --- | --- | --- | --- |
| `order_id`, `order_code`, `order_code_short` | Order Core | snapshot | I / P(`order_code_short`) | CONFIRMED D-01 |
| `order_version` | Order Core | snapshot target | I | IR-SALES-OC1; current chưa expose (DS-04), nullable/không required |
| `order_state` (đục), `payment_method_snapshot=COD`, `is_ivr_callable` optional | Order Core | snapshot | I | CONFIRMED D-02/DS-01; callable là rule derive từ `CONFIRMING+COD` |
| `program_code`, `max_attempts`, `confirmation_window_seconds`, `attempt_schedule` | Commerce/Program Runtime | snapshot | I | CONFIRMED D-10 |
| `customer_ref` | Customer/Commerce | snapshot | S | CONFIRMED |
| `customer_trust_status`, `trusted_skip_allowed`, `risk_flags[]` | business-platform (CRM) | snapshot | S | ⚠️ D-12/13 model; **resolver CHƯA build (DC-06)** → hiện require-IVR |
| `official_contact_id` | OfficialContactResolver | snapshot | S | CONFIRMED D-05 |
| `phone_ref` | OfficialContactResolver | snapshot | R | CONFIRMED D-05 |
| `phone_masked` | OfficialContactResolver | snapshot | S | CONFIRMED D-05 (admin display) |
| `phone_validation_status` | Phone Resolver | snapshot | I | CONFIRMED |
| `dial_token` | OfficialContactResolver → SIM vault | snapshot (không lưu ở IVR) | R | CONFIRMED D-05 (TTL≤window, one-use) |
| `call_script_template_id`, `call_script_version`, `allowed_script_variables` | IVR Owner (approved) | snapshot | I | CONFIRMED |
| `call_restriction` / `opt_out` | **Customer Identity** (`POST /api/v1/admin/customer-identity/crm-ads-eligibility`, `channelType=PHONE_CALL`) | snapshot (Order Core gọi) | S | ✅ **DC-01** (endpoint có; cần extend response `do_not_call/opt_out_scope/reason/effective_at`) |
| `evidence_policy_version`, `privacy_policy_version` | Governance/Foundation | snapshot | I | CONFIRMED |

## 2. Callback fields (IVR → Order Core)
| Field | Owner | Chiều | Ghi chú |
| --- | --- | --- | --- |
| `callback_id`, `task_id`, `order_id` | IVR/Order Core | callback current | link + idempotency |
| `order_version_seen_by_ivr` | IVR/Order Core | callback target | IR-SALES-OC1 race guard; current không gửi/không nhận |
| `result_type`, `result_reason`, `dtmf_key`, `is_counted_customer_attempt`, `is_final_for_ivr` | IVR | callback | taxonomy + DT-02 |
| `recommended_core_action` | IVR | callback | **advisory** — Core revalidate (D-04) |
| `evidence_ref`, `audit_ref` | IVR→Evidence Registry | callback | bắt buộc trước final |

## 3. Contract/event từ Sales (không phải task field)
| Nhu cầu | Cơ chế | Trạng thái |
| --- | --- | --- |
| IVR-required | event `order.ivr_required_decisioned` (Core tạo task) | CONFIRMED D-09 |
| Quota release Golden Hour | IVR signal qua callback; Sales QuotaReleaseGuard | CONFIRMED D-11 |
| Nhận outcome vào CRM | event sau Core decision; IVR không ghi CRM | CONFIRMED D-14 (chi tiết QC5 chờ) |

## 4. Trace-id (MASTER-03)
`order_code` (Order Core) · `ivr_call_job_id`/`ivr_call_result_id` (IVR) · `correlation_id`/`idempotency_key` (xuyên hệ). IVR result phải trace về `order_code` + CustomerConfirmation (MASTER-03 §27).

## Báo cáo (sales)
- **~20 field task + 8 callback** mapped tới Order Core/Sales; `call_restriction` đã có nguồn CRM (**DC-01**), rich response/Core wiring còn **IR-CRM-01 P1**.
