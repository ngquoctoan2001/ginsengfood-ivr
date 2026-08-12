# IR-05 — Open Contract Questions (tổng hợp còn mở)

Trạng thái: `REQUIREMENTS` · Tổng hợp các điểm chưa chốt còn chặn/ảnh hưởng integration. Phần lớn contract đã khóa (D-*/DO-*/DF-*/DT-*); dưới đây là phần **còn cần người/mua sắm/legal**.

| ID | Câu hỏi/việc mở | Prio | Team | Chặn gì | File |
| --- | --- | --- | --- | --- | --- |
| ~~OQ-01 / Q-C1~~ ✅ | **do-not-call / opt-out / call-restriction**: nguồn = `crm-ads-eligibility` PHONE_CALL (DC-01); `eligible` dùng được ngay; rich fields/Core wiring theo IR-CRM-01 | P1 build | CRM / Customer Identity | ✅ Hết chặn P0; còn build extension | `questions-to-crm-3.1-followup.md` (QC1/QC2), IR-CRM-01 |
| ~~OQ-02 / DG-03~~ ✅ | **Order-state enum values + transition table**: `CONFIRMING+COD`; confirm→CONFIRMED, cancel→CANCELLED, timeout→EXPIRED; no-answer/technical không transition | ✅ resolved | Order Core (M3) | ✅ DS-01..05; còn OC1/OC2/OC3 target | `questions-to-order-core-state.md`, `data/04-missing-data` |
| OQ-03 | **SIM procurement**: protocol (DT-01), số SIM launch (DT-04), caller-ID (DT-06) | P0 (gọi thật) | Telephony/Infra/procurement | Chặn gọi khách thật; không chặn specs/mock | `03-telephony-sim-requirements` |
| OQ-04 | **Retention duration** từng loại dữ liệu | P1 | Owner + Legal | Compliance/PII | `04-shared-auth-audit-requirements` DF-07 |
| OQ-05 | **Release sign-off authority + pilot scope** | P0 (khi release) | Release Owner (bạn) + security/privacy | Mở `REAL_CUSTOMER_CALL_ALLOWED` | DF-03 |
| OQ-06 | **Technical retry count/backoff** (bounded) | P1 | IVR Owner | Config retry callback/technical | OD-10 |
| OQ-07 | **captured_at/ETag** trên SellableStatus + lock | P1 | Ops-Core | Độ tươi snapshot (DO-02) | `02-ops-core-requirements` IR-OPS-02 |
| OQ-08 | **Notification template** sau Core hủy/expire + CRM outcome event name | P1 | CRM/Notification | Thông báo sau no-answer/expire (QC5/OD-16) | `questions-to-crm-3.1-followup` QC5 |

## Câu hỏi chặn lớn nhất
**OQ-03 (mua SIM)** và **OQ-05 (release sign-off)** là hai P0 còn chặn gọi khách thật. **Q-C1/DC-01** đã có nguồn, không còn là P0; **IR-CRM-01** chỉ còn là build P1 cho rich response/Core wiring.

## Báo cáo (p09)
- **Yêu cầu theo team:** Sales/Order Core **10** (IR-SALES-01..10), Ops-Core **7** (IR-OPS-01..07), Telephony **6** (IR-TEL-01..06), Foundation **7** (IR-FND-01..07) = **30 yêu cầu**.
- **P0:** IR-SALES-01..06, IR-OPS-01..04, IR-TEL-01..02, IR-FND-01..04 (+ OQ-03/05). OQ-01/Q-C1 đã chuyển thành IR-CRM-01 P1.
- **Câu hỏi chặn lớn nhất:** OQ-03 (mua SIM) + OQ-05 (release sign-off).
- **Tension order_code:** ✅ đã giải quyết (D-01).
