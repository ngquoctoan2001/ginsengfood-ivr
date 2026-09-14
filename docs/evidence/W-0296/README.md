# W-0296 — Khôi phục runner Docker

Ngày 14/09/2026. **TESTS_PASS (runner recovery)**. Candidate `179a5eb`, [pipeline 2846110576](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2846110576). Hai remote main cùng SHA; Gitleaks HEAD 349 commit không có finding.

[Job 16478951064](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16478951064) lỗi `runner system failure`: không mở được pipe Docker engine, trước khi chạy script. Local cũng mất pipe; Docker Desktop chưa chạy. Lần mở lại báo socket cũ `sailor-ingest.sock` không truy cập được, tiếp đó là `engine.sock`. Nguồn: log Docker backend lúc 08:21 và 08:24 UTC.

Dừng phiên Docker khởi động lỗi bằng CLI; kiểm từng thư mục không phải reparse point, mọi entry đều là socket rỗng và không có thư mục con. Đổi tên giữ nguyên bản dự phòng dưới AppData/Local: `Docker/run.w0296-backup-20260914-1525` (6 entry), `docker-secrets-engine.w0296-backup-20260914-1528` (2 entry), rồi `Docker/run.w0296-backup-20260914-1529` (2 socket mới từ lần mở dở). Không đọc giá trị bí mật, không xóa volume/image/disk, không factory reset và không thay cấu hình runner.

Sau lần mở cuối, `docker info --format '{{.ServerVersion}}'` trả **29.7.2**, exit 0. Đã dùng nút Retry all failed or cancelled jobs trên pipeline hiện tại. [Gate sweep 16479006261](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16479006261) hoàn tất **39/39 PASS, 22 skip theo manifest**, `Job succeeded` lúc 08:37:04 UTC: W-0181/W-0183, regression JUnit, PII, DR và TTS đều PASS. Chốt phục hồi runner; toàn pipeline/publish/deploy vẫn theo W-0292.

Kiểm local bổ sung trước recovery chỉ **32/39**: bốn gate mất Docker và ba gate thiếu `sh` trong PATH. Giữ log lỗi trong `.artifacts/w0295/exact-head-gate-sweep.log`; không dùng lần này thay full sweep trước đó hoặc hosted proof. Phần pin W-0181/W-0183 vẫn PASS ở lượt này.
