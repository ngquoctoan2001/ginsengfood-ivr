# Retention options (trước khi Legal ký) — `W-0059` · `P11-3`

Ngày: `2026-08-19` · Trạng thái: **`LEGAL_SIGNOFF_REQUIRED`** — đây là **phương án**, không phải
chính sách

## 1. Vì sao là "phương án" chứ không phải một con số

Người ký cần ba thứ mà một bảng số trống không cho họ: **các lựa chọn có thật**, **cái gì mất đi ở
mỗi lựa chọn**, và **ràng buộc kỹ thuật nào giới hạn lựa chọn**. Đưa một con số duy nhất là bắt
người ký hoặc gật hoặc tự nghĩ lại từ đầu.

Mỗi dòng dưới đây có **chủ sở hữu**, **cơ chế xoá**, và **giá trị hoặc `LEGAL_SIGNOFF_REQUIRED`** —
ba thứ `LEGAL-RET-01` đọc.

## 2. Ràng buộc kỹ thuật (không thương lượng được)

1. **`speech_snapshot` < `task_metadata`.** Nó redact trường bên trong dòng trước khi dòng bị xoá;
   đặt ngược thì bước ẩn danh không bao giờ chạy.
2. **Chu kỳ con ≤ chu kỳ cha.** Job xoá child trước; một child sống lâu hơn parent **chặn parent
   vĩnh viễn** và class parent sẽ báo `dependency blocked` mãi.
3. **Tuổi backup ≤ chu kỳ dài nhất.** Nếu không, chu kỳ **thật** là tuổi backup.
4. **`analytics_derived` không có chu kỳ riêng** — thừa kế từ nguồn theo cấu trúc.

## 3. Phương án

| Data class | Chủ sở hữu | Cơ chế xoá | Phương án A (ngắn) | Phương án B (dài) | Mất gì nếu chọn A |
| --- | --- | --- | --- | --- | --- |
| `speech_snapshot` | Privacy | `ANONYMIZE` | 7 ngày | 30 ngày | không tái dựng được lời thoại đã đọc khi có khiếu nại sau 7 ngày |
| `task_metadata` | IVR owner | `DELETE` | 90 ngày | 365 ngày | không đối chiếu được đơn cũ với hoạt động IVR |
| `attempt_metadata` | IVR owner | `DELETE` | 90 ngày | 365 ngày | mất dữ liệu chẩn đoán tần suất gọi |
| `result_metadata` | IVR owner | `DELETE` | 90 ngày | 365 ngày | mất chuỗi kết quả cho phân tích xu hướng dài hạn |
| `callback_metadata` | Sales + IVR | `DELETE` | 90 ngày | 365 ngày | **tranh chấp giao nhận quá 90 ngày không có bằng chứng** |
| `raw_call_event` | Telephony | `DELETE` | 30 ngày | 90 ngày | không đối soát được với hoá đơn nhà mạng quá 30 ngày |
| `evidence_link` | IVR owner | `DELETE` (bảo vệ `accepted_at`) | 90 ngày | 365 ngày | evidence chưa nghiệm thu biến mất sớm |
| `idempotency_key` | IVR owner | `DELETE` | 7 ngày | 30 ngày | lần thử lại sau 7 ngày tạo tác dụng phụ lần hai |
| `review_item` | Ops | `ANONYMIZE` | 90 ngày | 365 ngày | mất ngữ cảnh review cũ |
| `audit_log` | Security | **không xoá** | vĩnh viễn | vĩnh viễn | — |
| `active_config` | IVR owner | **không xoá** | vĩnh viễn | vĩnh viễn | — |
| `retention_control` | IVR owner | **không xoá** | vĩnh viễn | vĩnh viễn | — |
| `analytics_derived` | IVR owner | thừa kế | = nguồn | = nguồn | — |

**Cả hai cột A và B đều là `LEGAL_SIGNOFF_REQUIRED`.** Chúng là điểm khởi đầu cho một cuộc trao đổi,
không phải hai đáp án để chọn nhanh.

## 4. Tương tác với backup

Với phương án A, chu kỳ dài nhất là **90 ngày**, nên `IVR_BACKUP_MAX_AGE_DAYS` phải **≤ 90**.
Với phương án B là **365**, và giữ backup một năm là một quyết định lưu trữ riêng cần cân nhắc chi
phí lẫn rủi ro.

Và điều `prune.sh` **không** làm được: **xoá chọn lọc bên trong một bản backup đã mã hoá**. Một yêu
cầu xoá của chủ thể không chạm tới dữ liệu trong các bản backup còn hạn; chúng hết theo tuổi. Đây là
giới hạn phải nói với người yêu cầu, và là một đầu vào cho việc chọn A hay B.

## 5. Cơ chế xoá — cái gì thực sự chạy

| Cơ chế | Ở đâu | Đã kiểm bằng |
| --- | --- | --- |
| `DELETE` child-first, batch ngắn, resume được | `RetentionJob` | `IT-RET-DELETE-03`, `IT-RET-RESUME-06` |
| `ANONYMIZE` tại chỗ | `RetentionTargetCatalog.SpeechSnapshotRedactionSql` | `IT-RET-PII-07` |
| `legal_hold_until` luôn thắng retention | `RetentionJob` | `IT-RET-HOLD-04` |
| dry-run mặc định | `RetentionOptions` | `IT-RET-DRYRUN-02` |
| purge hook cho bản sao phái sinh | `AnalyticsRetentionHook` | `COMP-RETENTION-04` |
| prune backup theo tuổi | `deploy/backup/prune.sh` | `DG-RETENTION-04` |

Không dòng nào ở đây chờ chữ ký — **cơ chế đã có và đã kiểm**. Cái chờ chữ ký là **con số**.
