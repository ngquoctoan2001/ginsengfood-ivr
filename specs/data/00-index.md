# Data SRS — Index

Trạng thái: `SRS_DRAFT` · Sinh bởi: `plan/ivr-orther/prompts/p06-generate-data-mapping.md`
Nguồn: `phase-8/02` §11 (data allowed/prohibited), `/04`,`/07`,`/08`,`/12`; `MASTER-01` (source-of-truth), `MASTER-03` (trace-id); `specs/srs/api/*`; decisions D-*/DO-*/DF-*/DT-*.

## 1. Cấu trúc
| File | Nội dung |
| --- | --- |
| [01-data-ownership.md](01-data-ownership.md) | Nhóm dữ liệu → owner → IVR read/snapshot/write/none |
| [02-mapping-sales-platform.md](02-mapping-sales-platform.md) | Field IVR ↔ Order Core/Sales, chiều, resolver |
| [03-mapping-ops-core.md](03-mapping-ops-core.md) | `sellable_status[]` ↔ ops sellable gate; sale-lock/recall trace |
| [04-missing-data.md](04-missing-data.md) | `GAP` dữ liệu chưa có nguồn + priority + owner |
| [05-pii-policy.md](05-pii-policy.md) | phone_ref/masked/token; cấm raw phone/full profile; recording OFF |

## 2. Nguyên tắc dữ liệu (P0)
- IVR chỉ giữ **snapshot / ref**; version snapshot là target/nullable IR-SALES-OC1. **KHÔNG** là source-of-truth của order state, payment, inventory, recall, customer profile (phase-8/00 §5; MASTER-01).
- Order state không nằm trong DB IVR như chân lý — current dùng `order_state`(đục)+COD gate để revalidate; `order_version` chỉ bật khi OC1 expose.
- Blocker realtime do **Order Core** gọi ops (DO-03); IVR consume snapshot + kết quả revalidate.
- Trace-id theo MASTER-03: `order_code`, `ivr_call_id`(≈`ivr_call_job_id`), `ivr_call_result_event_id`(≈`ivr_call_result_id`), `correlation_id`, `idempotency_key`; blocker evidence kèm `sale_lock_id`/`recall_case_id` (DO-07).
- PII safe-by-default: `phone_ref`/`phone_masked`/`dial_token`; cấm raw phone/full address/payment/health (phase-8/02 §11, /08).

## 3. Phân lớp privacy (dùng xuyên các file)
`RESTRICTED` (raw phone thật, dial_token→số) · `SENSITIVE` (phone_masked, official_contact_id, customer_ref, risk_flags) · `INTERNAL` (order refs, program, result) · `PUBLIC-SAFE` (order_code_short, total_amount_display trong script). Chi tiết [05-pii-policy.md](05-pii-policy.md).
