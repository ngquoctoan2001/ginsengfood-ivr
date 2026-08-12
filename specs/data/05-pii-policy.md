# DATA-05 — PII & Privacy Policy

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p06` · Nguồn: `phase-8/02 §11`, `/08`, `/15`; D-05, DT-05, DF-07.

## 1. Phân lớp & xử lý
| Lớp | Dữ liệu | Lưu ở IVR? | Hiển thị admin? | Trong script? |
| --- | --- | --- | --- | --- |
| **RESTRICTED** | raw phone thật; `dial_token`→số | **KHÔNG** (token→số chỉ ở SIM adapter vault) | KHÔNG | KHÔNG |
| **SENSITIVE** | `phone_masked`, `official_contact_id`, `customer_ref`, `risk_flags`, `call_restriction` | ref/masked | chỉ `phone_masked` | KHÔNG |
| **INTERNAL** | order refs, `order_state`, program, result, evidence refs | có | có (masked view) | KHÔNG |
| **PUBLIC-SAFE** | `order_code_short`, `total_amount_display`, (opt) `customer_name_short`, `program_name` | có | có | ✅ ALLOWED |

## 2. Quy tắc P0 (phase-8/02 §11, /08)
- ✅ Chỉ dùng `phone_ref`/`phone_masked`/`dial_token` để gọi; **cấm** raw phone trong log/UI/DB IVR (D-05; P0-IVR-007). `dial_token` TTL ≤ confirmation window, one-use/attempt; mapping token→số thật **nằm ở SIM adapter/token vault**, không ở IVR.
- ✅ **Cấm đọc/log**: full address, payment/COD detail, member tier, Diamond, order history, health/sensitive note, AI/CRM content (phase-8/02 §11; call script whitelist `functional/04`).
- ✅ **Recording OFF** mặc định (DT-05); bật chỉ khi có consent + legal + retention (DF-07/DG-08); nếu bật chỉ lưu `recording_ref` + audit truy cập.
- ✅ DTMF chỉ lưu **semantic** (`1`/`0`/none/invalid) — không lưu audio nhạy cảm (phase-8/12 §11).
- ✅ Admin UI **masked** mặc định; RBAC + audit cho mọi truy cập (DF-01).

## 3. Retention (đề xuất — số cụ thể chờ DF-07/Legal)
| Dữ liệu | Đề xuất | Trạng thái |
| --- | --- | --- |
| raw phone / dial_token | TTL ngắn nhất (≤ window) | ⏳ DF-07 |
| DTMF evidence | chỉ key + timestamp | ⏳ DF-07 |
| Recording | OFF (nếu bật: theo legal) | ⏳ DG-08 |
| Call log kỹ thuật (sanitized) | ⏳ số cụ thể | ⏳ DF-07 |
| Admin audit | theo foundation (append-only) | CONFIRMED TECH-01 |
| Task/job/result/callback metadata | đủ audit/release trace | ⏳ DF-07 |

## 4. Trace vs PII
- Evidence/audit dùng **ref/id** (`sale_lock_id`, `recall_case_id`, `evidence_ref`, `correlation_id`) — không nhúng PII thô (MASTER-03/DO-07).
- Soft-delete không che audit trail (phase-8/12 §2).

## 5. FAIL nếu
- Raw phone xuất hiện trong app log/admin UI không được duyệt (phase-8/00 §9; P0-IVR-007).
- Script đọc field ngoài whitelist.
- Bật recording khi chưa có consent/legal.

## Báo cáo (PII)
- Phân 4 lớp; RESTRICTED (raw phone/token) không lưu ở IVR; recording OFF; retention **PENDING DF-07 (Legal)** = điểm PII rủi ro cao nhất còn mở.
