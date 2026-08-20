# DF-07 — Retention policy

Trạng thái: **`LEGAL_SIGNOFF_REQUIRED`** · Ngày dự thảo: `2026-08-19` · Work: `W-0059` / `P11-3`

## 1. Tài liệu này chưa phải một quyết định

`P11-3` §11 cấm đánh dấu retention là đã duyệt khi chưa có chữ ký của Legal/chủ sở hữu. Trạng thái ở
đầu trang là thứ cổng `LEGAL-RET-01` đọc, và nó sẽ ở nguyên đó cho tới khi §4 có tên người.

Đầu vào: `docs/compliance/ivr-data-inventory.md` (ai sở hữu cái gì) và
`docs/compliance/ivr-retention-options.md` (các phương án và cái mất đi ở mỗi phương án).

## 2. Quyết định cần ký

| Data class | Chủ sở hữu | Cơ chế xoá | Chu kỳ đã ký |
| --- | --- | --- | --- |
| `speech_snapshot` | Privacy | `ANONYMIZE` | `LEGAL_SIGNOFF_REQUIRED` |
| `task_metadata` | IVR owner | `DELETE` | `LEGAL_SIGNOFF_REQUIRED` |
| `attempt_metadata` | IVR owner | `DELETE` | `LEGAL_SIGNOFF_REQUIRED` |
| `result_metadata` | IVR owner | `DELETE` | `LEGAL_SIGNOFF_REQUIRED` |
| `callback_metadata` | Sales + IVR | `DELETE` | `LEGAL_SIGNOFF_REQUIRED` |
| `raw_call_event` | Telephony | `DELETE` | `LEGAL_SIGNOFF_REQUIRED` |
| `evidence_link` | IVR owner | `DELETE` | `LEGAL_SIGNOFF_REQUIRED` |
| `idempotency_key` | IVR owner | `DELETE` | `LEGAL_SIGNOFF_REQUIRED` |
| `review_item` | Ops | `ANONYMIZE` | `LEGAL_SIGNOFF_REQUIRED` |
| `audit_log` | Security | **không xoá** | vĩnh viễn — quyết định thiết kế, không phải chu kỳ |
| `active_config` | IVR owner | **không xoá** | vĩnh viễn — quyết định thiết kế |
| `retention_control` | IVR owner | **không xoá** | vĩnh viễn — quyết định thiết kế |
| `analytics_derived` | IVR owner | thừa kế từ nguồn | = chu kỳ nguồn — phụ thuộc, không phải chu kỳ |

## 3. Điều người ký cần biết

1. **Một con số ký ở đây phải được điền vào hai chỗ**: bảng trên **và**
   `Ivr:Retention:PeriodDays:<class>` của từng môi trường. Không cổng nào kiểm hai chỗ khớp nhau;
   đó là việc của người điền.
2. **Chưa cấu hình = không xoá.** Một class không có số ngày hợp lệ ở trạng thái `NOT_CONFIGURED` và
   **không xoá, không ẩn danh gì cả**, đồng thời phát cảnh báo theo tuổi. Đó là hành vi đúng cho một
   chính sách chưa ký — không hành động, và **nói ra** rằng mình không hành động.
3. **`IVR_BACKUP_MAX_AGE_DAYS` phải ≤ chu kỳ dài nhất ký ở đây.** Nếu không, chu kỳ **thật** của dữ
   liệu là tuổi bản backup, và con số ký ở đây chỉ còn là mô tả.
4. **Một yêu cầu xoá của chủ thể không chạm tới bản backup.** Không có cơ chế xoá chọn lọc bên trong
   một bản backup đã mã hoá; chúng hết theo tuổi.

## 4. Chữ ký

| Vai trò | Tên | Ngày | Ghi chú |
| --- | --- | --- | --- |
| Legal | _(trống)_ | | |
| Privacy | _(trống)_ | | |
| Chủ sở hữu IVR | _(trống)_ | | |

Khi ba ô có tên: đổi trạng thái đầu trang thành `SIGNED`, điền §2, rồi điền config từng môi trường.
Trước lúc đó, `P9-1` phải đọc tài liệu này là **no-go**.
