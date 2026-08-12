# DB-05 — Retention & Privacy

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p07` · Nguồn: `phase-8/12` §11, `/08`; `data/05-pii-policy`; D-05, DT-05, DF-07.

## 1. Nguyên tắc
- **KHÔNG** cột nào lưu raw phone thật hay dial_token→số (D-05); mapping token→số ở SIM adapter vault, không ở DB IVR.
- Recording **OFF** mặc định → `ivr_raw_call_event.recording_ref` = null (DT-05).
- DTMF lưu **semantic** (`1`/`0`/invalid) — không audio nhạy cảm.
- Soft-delete không che audit trail; admin audit append-only (TECH-01).

## 2. Retention theo loại (số cụ thể ⏳ DF-07 — Owner+Legal)
| Dữ liệu | Retention | Trạng thái |
| --- | --- | --- |
| Task/job/attempt/result/callback metadata | đủ audit/release trace | ⏳ DF-07 |
| Call log kỹ thuật (`ivr_raw_call_event`, sanitized) | ngắn | ⏳ DF-07 |
| DTMF evidence | key + timestamp | ⏳ DF-07 |
| Recording | OFF (nếu bật: theo legal) | ⏳ DT-05/DG-08 |
| `phone_ref`/token | TTL ngắn nhất (≤ window) | ⏳ DF-07 |
| Admin audit (`ivr_admin_actions`) | theo foundation, append-only | ✅ TECH-01 |
| Evidence links | đủ hỗ trợ release/dispute | ⏳ DF-07 |

## 3. Masking
- Admin projection/`GET` chỉ trả `phone_masked` + order refs; **không** raw phone/full address/payment/health (phase-8/08; P0-IVR-007).
- `last_error` trong callbacks phải sanitized (không PII).

## 4. Migration gate liên quan privacy
- Migration **không** được thêm cột bắt buộc lưu full phone/raw recording nếu chưa có owner decision (DF-07). → xem [06-migration-plan.md](06-migration-plan.md).
