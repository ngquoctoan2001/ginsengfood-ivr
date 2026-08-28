# IVR data inventory (class level, có chủ sở hữu) — `W-0059` · `P11-3`

Ngày: `2026-08-19` · Trạng thái: **`LEGAL_SIGNOFF_REQUIRED`** cho mọi chu kỳ
· Nguồn kỹ thuật: `src/Ivr.Infrastructure/Governance/DataClassification.cs`

## 1. Khác gì `docs/compliance/data-inventory.md`

| Tài liệu | Ống kính | Người đọc |
| --- | --- | --- |
| `data-inventory.md` (P10-1) | **trường**: mục đích, cơ sở pháp lý, hành vi khi xoá | cơ quan quản lý, người trả lời DSAR |
| tài liệu này (P11-3) | **class**: ai sở hữu, mức nhạy cảm, lưu ở đâu, xoá bằng gì | người ký DF-07 |

Người ký retention không cần biết `phone_ref` là gì. Họ cần biết **ai chịu trách nhiệm** về mỗi
nhóm, dữ liệu **nằm ở đâu**, và **cơ chế nào** thực sự xoá nó — vì đó là những thứ họ đang ký.

## 2. Bảng

| Data class | Chủ sở hữu | Nhạy cảm | Lưu ở đâu | Cơ chế xoá | Chu kỳ |
| --- | --- | --- | --- | --- | --- |
| `task_metadata` | IVR owner | **cao** — chứa tham chiếu liên hệ | `ivr_confirmation_tasks`, `ivr_call_jobs`, `ivr_task_intake_outbox`, `ivr_capacity_incidents` | `DELETE` child-first, P1-5 | `LEGAL_SIGNOFF_REQUIRED` |
| `speech_snapshot` | Privacy | **cao** — nội dung đọc cho khách | `ivr_confirmation_tasks` (các cột) | `ANONYMIZE` tại chỗ, P1-5 | `LEGAL_SIGNOFF_REQUIRED` |
| `attempt_metadata` | IVR owner | trung bình | `ivr_call_attempts`, `ivr_technical_exceptions` | `DELETE`, P1-5 | `LEGAL_SIGNOFF_REQUIRED` |
| `result_metadata` | IVR owner | trung bình | `ivr_call_results` | `DELETE`, P1-5 | `LEGAL_SIGNOFF_REQUIRED` |
| `callback_metadata` | Sales + IVR | **cao** — payload đã gửi | `ivr_result_callbacks` | `DELETE`, P1-5 | `LEGAL_SIGNOFF_REQUIRED` |
| `raw_call_event` | Telephony | **cao** — sự kiện nhà mạng | `ivr_raw_call_events` | `DELETE`, P1-5 | `LEGAL_SIGNOFF_REQUIRED` |
| `evidence_link` | IVR owner | trung bình | `ivr_evidence_links`, `ivr_evidence` | `DELETE`, bảo vệ khi `accepted_at` khác null | `LEGAL_SIGNOFF_REQUIRED` |
| `idempotency_key` | IVR owner | **cao** — chứa response snapshot | `ivr_idempotency_keys` | `DELETE`, P1-5 | `LEGAL_SIGNOFF_REQUIRED` |
| `review_item` | Ops | trung bình | `ivr_review_items` | `ANONYMIZE`, chỉ item đã resolved | `LEGAL_SIGNOFF_REQUIRED` |
| `audit_log` | Security | **cao** — ai làm gì | `ivr_audit_log`, `ivr_admin_actions` | **không xoá** — append-only ép bởi database | vĩnh viễn theo thiết kế |
| `active_config` | IVR owner | thấp | `ivr_feature_flags`, `ivr_attempt_policies`, `ivr_script_versions`, `ivr_sim_channels` | **không xoá** — cấu hình có phiên bản | vĩnh viễn theo thiết kế |
| `retention_control` | IVR owner | thấp | `ivr_retention_checkpoints` | **không xoá** — chỉ số đếm tổng hợp | vĩnh viễn theo thiết kế |
| `analytics_derived` | IVR owner | trung bình — bí danh mã đơn | schema `analytics` (7 bảng) | **thừa kế**: purge hook xoá fact khi nguồn mất | **= chu kỳ nguồn**, không ký riêng |
| **ghi âm** | — | — | **không tồn tại** | — | DT-05: TẮT |

## 3. Ba dòng không cần chữ ký, và vì sao

- **`audit_log`** — `PRESERVE` là quyết định thiết kế, ép bởi database (`UPDATE`/`DELETE` bị từ chối).
  Ký một chu kỳ cho nó sẽ là ký một thứ hệ thống không thi hành được.
- **`active_config` / `retention_control`** — cấu hình có phiên bản và số đếm tổng hợp; không phải
  dữ liệu cá nhân.
- **`analytics_derived`** — là một **phụ thuộc**, không phải một chu kỳ. Fact tồn tại chỉ khi kết quả
  nguồn còn tồn tại, nên chu kỳ bằng nhau **theo cấu trúc** và không có cách nào đặt lệch.

## 4. Nơi dữ liệu tồn tại **ngoài** PostgreSQL

Bảng §2 chỉ nói về database. Ba nơi khác giữ dữ liệu và **không** nằm dưới retention job:

| Nơi | Cái gì | Trạng thái |
| --- | --- | --- |
| bản backup đã mã hoá | ảnh chụp toàn bộ database tại một thời điểm | hết theo `prune.sh` (tuổi), **không xoá chọn lọc được bên trong** |
| log ứng dụng | correlation id, quyết định; PII guard chạy trên chúng | **chưa có** pipeline log tập trung (`W-0063`) |
| evidence file | `docs/evidence/**` trong repo | không chứa dữ liệu khách thật — seed dùng dải test |

Dòng đầu là giới hạn thật của quyền xoá và runbook DSAR phải nói ra.

## 5. Chữ ký

Xem `specs/decisions/DF-07-retention-policy.md`. Tài liệu này là **đầu vào** cho quyết định đó, không
phải quyết định.
