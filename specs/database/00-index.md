# Database SRS — Index

Trạng thái: `SRS_DRAFT` · Sinh bởi: `plan/ivr-orther/prompts/p07-generate-database-design.md`
Nguồn: `phase-8/12` (DB baseline), `/13` (services); `MASTER-03` (trace-id); `TECH-01` (idempotency/audit); `specs/srs/data/*`, `api/*`; decisions D-10, DO-02, DT-*, OD-DR-03.

## 1. Cấu trúc
| File | Nội dung |
| --- | --- |
| [01-erd.md](01-erd.md) | ERD (Mermaid) |
| [02-tables.md](02-tables.md) | Bảng: cột, type, required, index, constraint |
| [03-enums-and-status.md](03-enums-and-status.md) | program/job/attempt/result/callback/sim enums |
| [04-indexes.md](04-indexes.md) | Index scheduler-deadline, unique idempotency, race guard |
| [05-retention-and-privacy.md](05-retention-and-privacy.md) | Retention theo loại, raw phone không lưu, recording OFF |
| [06-migration-plan.md](06-migration-plan.md) | Migration gates, rollback, seed non-prod SIM disabled |

## 2. Nguyên tắc (P0)
- Tên bảng là **đề xuất**; implementation có thể theo convention repo nhưng **không mất semantic contract** (phase-8/12 §1).
- **Order state KHÔNG là source-of-truth trong DB IVR** — current chỉ lưu `order_state`(đục)+COD gate snapshot để revalidate; `order_version` là target/nullable IR-SALES-OC1 (D-02/DS-04; phase-8/12 §2).
- **KHÔNG lưu raw phone / dial_token→số**; dùng `phone_ref`/`phone_masked` (D-05).
- Technical failure **không** cộng customer attempt (`is_counted_customer_attempt=false`).
- Idempotency/correlation bắt buộc (DF-04/05); version race-guard là target IR-SALES-OC1.
- Recording **OFF** mặc định; seed SIM chỉ ở non-prod/disabled (DT-05/migration gate).

## 3. Thay đổi so với phase-8/12 gốc (do quyết định mới)
- ✅ **D-10:** `max_attempts=2` cho **cả hai** program (phase-8/12 gốc 24/7=3); Golden Hour `confirmation_window_seconds=300` (gốc 600), spacing 150; 24/7 window 900, spacing 450. **Sửa CHECK constraint.**
- ✅ **OD-DR-03:** thêm bảng **`ivr_raw_call_event`** (từ docx V0.2) giữ raw SIM/DTMF trước normalize.
- ✅ **DO-02:** thêm snapshot **`sellable_status` per-line + `captured_at`** trong task.
- ✅ **DT-01:** `ivr_sim_channels.adapter_mode` (MOCK/REAL) cho giai đoạn chưa mua SIM.
- ✅ **DC-01/Q-C1:** cột `call_restriction` có nguồn CRM; nullable tới khi IR-CRM-01 build rich response/Core wiring xong.

## 4. Danh sách bảng (11)
`ivr_confirmation_tasks` · `ivr_call_jobs` · `ivr_call_attempts` · `ivr_raw_call_event` · `ivr_call_results` · `ivr_result_callbacks` · `ivr_sim_channels` · `ivr_capacity_incidents` · `ivr_technical_exceptions` · `ivr_admin_actions` · `ivr_evidence_links`.
