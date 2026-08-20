# Retention policy — `W-0052` · `P10-1` · DF-07

Ngày: `2026-08-19` · Trạng thái: **`UNSIGNED`** — **mọi con số dưới đây để trống**

## 1. Vì sao bảng này trống

`DF-07` / `OD-V1-11` giao việc chốt chu kỳ cho Legal. Điền một con số nghe hợp lý vào đây sẽ tạo ra
**chính xác** thứ nguy hiểm nhất: một chính sách trông như đã ký, mà job sẽ thi hành, và không ai
từng đồng ý.

Cơ chế **đã có và đã kiểm**. Cái thiếu là chữ ký.

## 2. Hành vi khi chưa cấu hình

Một class không có số ngày dương hợp lệ ở `Ivr:Retention:PeriodDays:<class>` sẽ ở trạng thái
`NOT_CONFIGURED` và **không xoá, không ẩn danh gì cả**. Job ghi checkpoint kèm tuổi của trạng thái
đó, và cảnh báo khi tuổi vượt ngưỡng.

Đây là hành vi **đúng** cho một chính sách chưa ký: không hành động, và **nói ra** rằng mình không
hành động. Một mặc định "30 ngày cho an toàn" sẽ xoá dữ liệu theo một chính sách không tồn tại.

## 3. Bảng chu kỳ (chờ ký)

| Data class | Chiến lược | Chu kỳ (ngày) | Cơ sở |
| --- | --- | --- | --- |
| `task_metadata` | `DELETE` | _(trống)_ | |
| `speech_snapshot` | `ANONYMIZE` | _(trống)_ | |
| `attempt_metadata` | `DELETE` | _(trống)_ | |
| `result_metadata` | `DELETE` | _(trống)_ | |
| `callback_metadata` | `DELETE` | _(trống)_ | |
| `raw_call_event` | `DELETE` | _(trống)_ | |
| `evidence_link` | `DELETE` | _(trống)_ | |
| `idempotency_key` | `DELETE` | _(trống)_ | |
| `review_item` | `ANONYMIZE` | _(trống)_ | |
| `audit_log` | `PRESERVE` | **không xoá** | append-only, ép bởi database |
| `active_config` | `PRESERVE` | **không xoá** | cấu hình có phiên bản |
| `analytics_derived` | **thừa kế** | **= chu kỳ nguồn** | fact tồn tại chỉ khi kết quả nguồn còn tồn tại |

Ba dòng cuối **không cần ký** vì chúng không phải một con số: `PRESERVE` là quyết định thiết kế đã
ghi trong spec, và `analytics_derived` là một **phụ thuộc** chứ không phải một chu kỳ — không có cách
nào đặt hai bên lệch nhau vì chỉ có một bên.

## 4. Ràng buộc mà chữ ký phải tôn trọng

1. **`speech_snapshot` phải ngắn hơn `task_metadata`.** Nó redact các trường *bên trong* dòng trước
   khi dòng bị xoá; đặt ngược lại thì bước ẩn danh không bao giờ chạy.
2. **Chu kỳ con phải ≤ chu kỳ cha.** `callback_metadata`, `raw_call_event`, `attempt_metadata` và
   `result_metadata` đều bị chặn bởi phụ thuộc: job xoá child trước, và một child sống lâu hơn parent
   sẽ chặn parent vĩnh viễn.
3. **Tuổi tối đa của backup ≤ chu kỳ dài nhất.** Một bản backup 90 ngày của bảng 30 ngày làm chu kỳ
   **thật** thành 90, và con số 30 chỉ còn là mô tả. Xem `deploy/backup/prune.sh`.

## 5. Chữ ký

| Vai trò | Tên | Ngày |
| --- | --- | --- |
| Legal | _(trống)_ | |
| Privacy | _(trống)_ | |
| Chủ sở hữu IVR | _(trống)_ | |

Sau khi ký, điền số vào §3 **và** vào `Ivr:Retention:PeriodDays:*` của từng môi trường. Hai chỗ, và
`COMP-RETENTION-04` không kiểm được chúng khớp nhau — đó là việc của người điền.
