# Integration Requirements — Index (IVR Order Confirmation)

Trạng thái: `REQUIREMENTS` · Sinh bởi: `plan/ivr-orther/prompts/p09-generate-integration-requirements.md`
Người gửi: Team IVR / Module 8. Nguồn: `plan/ivr-orther/decisions-log.md` (D-*/DO-*/DF-*/DT-*), `specs/srs/api/*`, `data/*`, `10-integration-gap-analysis.md`, `11`,`12`; `phase-8/17`,`/02`; `phase-3.1/07`.

## 1. Mục đích
Tài liệu **yêu cầu tích hợp** gửi các team để hiện thực contract IVR cần. Phần lớn contract **đã được các team chốt** (D-*/DO-*/DF-*/DT-*) — đây là bản chuyển từ Q&A sang "cần build gì". Phần chưa chốt đánh ⏳.

## 2. Cấu trúc
| File | Team | Trạng thái |
| --- | --- | --- |
| [01-sales-platform-requirements.md](01-sales-platform-requirements.md) | Order Core (M3) + Sales Extensions (M3.1) | ✅ contract chốt D-01..14; cần build |
| [02-ops-core-requirements.md](02-ops-core-requirements.md) | Ops-Core (M1/2) | ✅ DO-01..09; cần build/điều chỉnh |
| [03-telephony-sim-requirements.md](03-telephony-sim-requirements.md) | Telephony/Infra | ⏳ DT-01..06; mua SIM |
| [04-shared-auth-audit-requirements.md](04-shared-auth-audit-requirements.md) | Foundation | ✅ DF-01..07 (owner kiêm) |
| [05-open-contract-questions.md](05-open-contract-questions.md) | tổng hợp câu hỏi mở | ✅ Q-C1/DC-01 + DG-03/DS-01..05 resolved; P0 còn DT-01 + DF-03 |

## 3. Tension order_code — ✅ ĐÃ GIẢI QUYẾT
`phase-3.1/07` ("không order_code trước IVR") vs `phase-8` ("IVR sau Official Order") → **D-01**: order_code cấp khi tạo Official Order; đơn vào `CONFIRMATION_REQUIRED/IVR_PENDING`; **fulfillment/downstream khóa** tới khi Core chấp nhận IVR signal. "Không order_code trước IVR" = "không release/verify downstream trước IVR". Không còn là câu hỏi mở.

## 4. Mock fallback
Mọi dependency có thể chạy **MOCK** (adapter/port + `INTEGRATION_MODE`) để IVR làm smoke/dry-run trước khi có API/hạ tầng thật (seed p10). `REAL_CUSTOMER_CALL_ALLOWED=NO` tới release gate (DF-03).

## 5. Quy ước mỗi yêu cầu
`ID · mục đích · priority · input/output · sync/async · idempotency · mock? · ai build · trạng thái`.
