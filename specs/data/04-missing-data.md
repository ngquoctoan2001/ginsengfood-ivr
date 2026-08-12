# DATA-04 — Missing Data (GAP)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p06` · Dữ liệu IVR cần nhưng chưa có nguồn/định dạng chốt.

| ID | Dữ liệu thiếu | Ảnh hưởng | Priority | Owner | Chờ ở |
| --- | --- | --- | --- | --- | --- |
| DG-01 ✅ | **do-not-call / opt-out / call-restriction** (field `call_restriction` trong task) | ✅ Source đã có: `crm-ads-eligibility` PHONE_CALL; còn build rich fields/Core wiring | P1 build | CRM/Customer Identity | DC-01 resolved; IR-CRM-01 |
| DG-02 | `captured_at`/`policy_version`/ETag trên `SellableStatus` + lock | Không biết độ tươi snapshot | P1 | Ops-core (owner cùng team) | DO-02 (ops bổ sung) |
| DG-03 ✅ | **Danh sách state đơn "IVR-callable" + bảng transition per result** (giá trị enum cụ thể) | ✅ Đã có DS-01..05: `CONFIRMING+COD`, transition thật đã rõ | ✅ resolved | Order Core | DS-01..05; deltas IR-SALES-OC1/OC2/OC3 |
| DG-04 | Notification template sau khi Core hủy/expire | CRM/notification gửi gì sau no-answer/expire | P1 | CRM/Notification | QC5 / OD-16 |
| DG-05 | **Mã disposition telco thật** (busy/rejected/unreachable/dropped… của gateway) | Ánh xạ DT-02 phải re-verify với gateway thật | P1 | Telephony/Infra | DT-01 (mua SIM) |
| DG-06 | Technical retry count/backoff | Bounded retry cần số cụ thể | P1 | IVR Owner | OD-10 |
| DG-07 | Retention duration từng loại (call log/DTMF/recording/audit/raw phone-token) | Compliance/PII | P1 | Owner + Legal | DF-07 |
| DG-08 | Recording enable + consent basis (nếu bật) | Chỉ khi muốn bật recording | P2 | Owner + Legal | DT-05 |
| DG-09 | Ngưỡng risk cụ thể ("abnormal order value"…) | IVR chỉ consume boolean; ngưỡng ở Risk Policy | P2 | Risk Policy (3.1) | D-13 (ngưỡng ở resolver, không ở IVR) |

## Ghi chú
- DG-03: ✅ **đã trả DS-01..05**; integration/contract test đầy đủ giờ chỉ còn target/deferred cho OC1/OC2/OC3.
- DG-01/Q-C1: ✅ **đã có nguồn DC-01**; không còn GAP P0 dữ liệu. IR-CRM-01 là build P1 để mở rộng response và wiring `call_restriction`.
- Các GAP còn lại là P1/P2, xử lý được bằng mock/adapter cho tới khi có nguồn thật.

## Báo cáo (missing)
- **7 GAP còn mở**: 0 P0 từ dữ liệu, 5 P1, 2 P2. DG-01 và DG-03 đã resolved theo open-decisions-register.
