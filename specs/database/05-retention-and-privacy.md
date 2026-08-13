# DB-05 — Retention & Privacy

Trạng thái: `TESTS_PASS` cho cơ chế P1-5 · Giá trị retention production: `OWNER_DECISION_REQUIRED` (`DF-07` / `OD-V1-11`) · Nguồn: `phase-8/12` §11, `data/05-pii-policy`, D-05, DT-05, DF-07.

## 1. Nguyên tắc bất biến

- IVR không lưu raw phone hoặc ánh xạ dial token sang số thật; ánh xạ thuộc SIM adapter vault.
- Recording mặc định `OFF`; `ivr_raw_call_events.recording_ref` phải null nếu chưa có phê duyệt riêng.
- `DryRun=true` là mặc định. Chạy ghi phải bật tường minh.
- Period lấy từ `Ivr:Retention:PeriodDays:<data_class>`. Không có số ngày dương hợp lệ thì class ở `NOT_CONFIGURED` và **không bị xoá/ẩn danh**.
- `legal_hold_until > now` luôn thắng retention và loại bản ghi khỏi batch.
- `ivr_audit_log` và `ivr_admin_actions` là append-only, không thuộc catalog purge V1.
- Evidence đã `accepted_at` không bị xoá. Muốn thay đổi cần quyết định Legal/Privacy riêng.
- Chỉ ghi aggregate count vào report/audit/metric; không ghi row ID, phone ref, nội dung lời thoại hay payload khách hàng.

## 2. Data class, chiến lược và nguồn period

> Mọi period dưới đây đều là **config key**, không phải số ngày đã được Legal ký. Cấu hình test chỉ là evidence kỹ thuật và không được dùng làm production policy.

| Data class | Strategy | Period source | Phạm vi |
| --- | --- | --- | --- |
| `task_metadata` | `DELETE` | `Ivr:Retention:PeriodDays:task_metadata` | intake outbox, task, job và capacity incident đã resolved, theo thứ tự child-first |
| `attempt_metadata` | `DELETE` | `Ivr:Retention:PeriodDays:attempt_metadata` | attempt và technical exception |
| `result_metadata` | `DELETE` | `Ivr:Retention:PeriodDays:result_metadata` | kết quả cuộc gọi khi không còn callback phụ thuộc |
| `callback_metadata` | `DELETE` | `Ivr:Retention:PeriodDays:callback_metadata` | callback/outbox đã quá hạn |
| `raw_call_event` | `DELETE` | `Ivr:Retention:PeriodDays:raw_call_event` | provider event đã sanitize |
| `speech_snapshot` | `ANONYMIZE` | `Ivr:Retention:PeriodDays:speech_snapshot` | thay phone ref/mask, ciphertext và summary bằng giá trị redacted; giữ contract identity |
| `evidence_link` | `DELETE` | `Ivr:Retention:PeriodDays:evidence_link` | evidence/link chưa accepted; accepted evidence luôn protected |
| `idempotency_key` | `DELETE` | `Ivr:Retention:PeriodDays:idempotency_key` | idempotency response snapshot đã quá hạn |
| `review_item` | `ANONYMIZE` | `Ivr:Retention:PeriodDays:review_item` | chỉ item đã resolved; xoá source ID, reason, assignee và resolution nhạy cảm |

## 3. Phủ bảng `02-tables.md` §1–§8

| Bảng | Data class / policy | Strategy | Điều kiện bảo vệ hoặc ghi chú |
| --- | --- | --- | --- |
| `ivr_confirmation_tasks` | `task_metadata`; đồng thời chứa `speech_snapshot` | `DELETE`; `ANONYMIZE` snapshot trước | task chỉ xoá khi không còn job; trigger chỉ cho phép redaction một chiều đã định nghĩa |
| `ivr_task_intake_outbox` | `task_metadata` | `DELETE` | child đầu tiên của accepted task/job; chỉ giữ hash/correlation và không giữ request body |
| `ivr_attempt_policies` | `active_config` | `PRESERVE` | immutable versioned config, không thuộc catalog P1-5 |
| `ivr_call_jobs` | `task_metadata` | `DELETE` | chỉ xoá khi không còn attempt/result phụ thuộc |
| `ivr_call_attempts` | `attempt_metadata` | `DELETE` | chỉ xoá khi không còn raw event/technical exception phụ thuộc |
| `ivr_raw_call_events` | `raw_call_event` | `DELETE` | recording vẫn OFF mặc định |
| `ivr_call_results` | `result_metadata` | `DELETE` | chỉ xoá khi không còn callback phụ thuộc |
| `ivr_result_callbacks` | `callback_metadata` | `DELETE` | payload immutable cho tới khi row đủ điều kiện retention |
| `ivr_sim_channels` | `active_config` | `PRESERVE` | tài nguyên vận hành đang lease/quarantine; P1-5 không purge |
| `ivr_capacity_incidents` | `task_metadata` | `DELETE` | chỉ incident có `resolved_at` và đã quá hạn |
| `ivr_technical_exceptions` | `attempt_metadata` | `DELETE` | child được xử lý trước attempt |
| `ivr_admin_actions` | `audit_log` | `PRESERVE` | append-only, không UPDATE/DELETE |
| `ivr_evidence_links` | `evidence_link` | `DELETE` | `accepted_at != null` luôn protected |
| `ivr_idempotency_keys` | `idempotency_key` | `DELETE` | theo `created_at`, `retain_until` và legal hold |
| `ivr_audit_log` | `audit_log` | `PRESERVE` | append-only; retention report được append vào đây |
| `ivr_evidence` | `evidence_link` | `DELETE` | `accepted_at != null` luôn protected |
| `ivr_feature_flags` | `active_config` | `PRESERVE` | cấu hình runtime hiện hành, thay đổi qua audited flag service |
| `ivr_review_items` | `review_item` | `ANONYMIZE` | chỉ item đã resolved; giữ trạng thái/thời điểm để audit |
| `ivr_retention_checkpoints` | `retention_control` | `PRESERVE/UPSERT` | checkpoint aggregate, không chứa PII; job quản lý theo `(data_class, segment)` |

## 4. Thuật toán, batch và khả năng resume

1. Resolve class theo thứ tự child-first: callback → raw event → attempt → result → speech → evidence → idempotency → review → intake outbox → task/job.
2. Resolve period. Thiếu period thì upsert checkpoint `NOT_CONFIGURED`, phát metric/alert age và không chạy mutation.
3. Đếm `actionable`, `legal hold`, `protected`, `dependency blocked` cho dry-run/report.
4. Real-run chọn tối đa `BatchSize` bằng `FOR UPDATE SKIP LOCKED`, mutate trong transaction `READ COMMITTED` ngắn và upsert checkpoint trong cùng transaction.
5. Commit xong mới phát metric batch. Bị kill sau commit thì lần chạy sau chỉ thấy row còn lại hoặc snapshot đã có `anonymized_at`; không xoá/ẩn danh trùng.
6. Kết thúc run append `RetentionRunReport` privacy-safe vào `ivr_audit_log`.

Worker host chỉ chạy một pass khi `Ivr:Retention:Enabled=true`, sau đó tự dừng để P7-2 có thể schedule dưới dạng CronJob mà không sửa code. Cấu hình repository mặc định là `Enabled=false`, `DryRun=true`, `PeriodDays={}`.

## 5. Masking và privacy

- Admin projection chỉ trả `phone_masked` và order refs; không trả raw phone, full delivery detail, payment detail hoặc health data.
- `last_error`, evidence và report retention phải sanitized.
- `speech_snapshot` redaction đặt `phone_ref=redacted`, `phone_masked=***`, ciphertext thành opaque redacted marker, summary thành JSON rỗng và đặt `anonymized_at`.
- DB trigger vẫn cấm mọi thay đổi contract/policy/snapshot khác; rollback migration khôi phục trigger trước P1-5.

## 6. Gate và trạng thái còn mở

- Kỹ thuật local: 7 test `UT/IT-RET-*` chứng minh fail-closed, dry-run, delete, legal hold, audit append-only, resume và PII redaction.
- `DF-07` / `OD-V1-11`: Legal/Privacy phải cung cấp số ngày cho từng data class trước khi bật real-run ngoài test.
- Mọi production enablement phải giữ `REAL_CUSTOMER_CALL_ALLOWED=NO` cho tới release gate riêng; retention job không cấp quyền gọi khách.
- Thay đổi audit/evidence accepted retention là ngoài phạm vi V1 và cần owner decision/migration riêng.

## 7. Migration gate liên quan privacy

- Migration không được thêm raw phone hoặc raw recording.
- Migration P1-5 thêm `legal_hold_until`, `anonymized_at`, evidence lifecycle timestamps, bảng checkpoint và index phục vụ batch.
- Up/down migration phải qua PostgreSQL apply → rollback → recreate; model snapshot phải báo không có pending changes.
- Xem thêm [06-migration-plan.md](06-migration-plan.md) và evidence [W-0064](../../docs/evidence/W-0064/README.md).
