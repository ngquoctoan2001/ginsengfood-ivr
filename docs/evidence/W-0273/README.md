# W-0273 — Phiếu điền kỳ hạn lưu trữ, dựng từ mã nguồn

Ngày: 2026-09-10 · Baseline: `main@00b7c93` · Trạng thái: **BLOCKED_EXTERNAL** (chờ pháp chế).

Owner chốt ngày 2026-09-10: tôi dựng bản đề xuất để pháp chế điền số, **không tự đề xuất con số**.

Sản phẩm: [`docs/compliance/retention-period-proposal.md`](../../compliance/retention-period-proposal.md).

## 1. Vì sao là phiếu điền chứ không phải đề xuất

Kỳ hạn lưu trữ là nghĩa vụ pháp lý. Thứ kỹ thuật trả lời được là **hệ quả vận hành của một kỳ hạn
quá ngắn** — mất bằng chứng gì, hỏng hành vi nào. Thứ kỹ thuật **không** trả lời được là con số.
Nên mỗi dòng có đủ dữ kiện để luật sư quyết, và cột số để trống thật.

## 2. Dữ kiện rút từ mã nguồn, không từ mô tả

| Nguồn | Rút ra gì |
| --- | --- |
| `RetentionTargetCatalog.cs` | 9 nhóm → bảng nào, cột mốc nào, `Delete` hay `Anonymize` |
| `PersonalDataInventory.cs` | cột nào là dữ liệu cá nhân, căn cứ pháp lý, cách xoá |
| `RetentionOptions.cs` | `DryRun = true` mặc định |
| migration `P1_2` | trigger `ivr_audit_log_append_only` — `BEFORE UPDATE OR DELETE ON ivr_audit_log` |

## 3. Ba điều phiếu nêu mà không ai yêu cầu

**Ràng buộc thứ tự giữa hai nhóm.** `speech_snapshot` (ẩn danh) và `task_metadata` (xoá) chạm **cùng
bảng** `ivr_confirmation_tasks`, cùng mốc `created_at`. Nếu `speech_snapshot` **dài hơn**
`task_metadata`, dòng bị xoá trước khi kịp ẩn danh — bước ẩn danh không bao giờ chạy. Điền hai số
độc lập mà không biết điều này là điền ra một cấu hình vô nghĩa.

**`ivr_audit_log` giữ vĩnh viễn.** Nó **không có** nhóm retention nào (0 lần xuất hiện trong
catalog) và có trigger chặn `UPDATE`/`DELETE`. Nếu điều đó sai về pháp lý thì không bảng nào sửa
được — đó là quyết định riêng.

**Ẩn danh ≠ xoá.** Hai nhóm `speech_snapshot` và `review_item` **giữ nguyên dòng**. Nếu nghĩa vụ là
"xoá dữ liệu" thì phải đổi mã nguồn, không phải đổi số.

## 4. Trạng thái

Không đổi mã sản phẩm, không đổi cấu hình. `Ivr:Retention:PeriodDays` vẫn `{}` — **chưa xoá gì**.

Đây là rủi ro tuân thủ **kể từ ngày đầu tiên có dữ liệu khách thật**, không sớm hơn. Hiện
`REAL_CUSTOMER_CALL_ALLOWED=NO` ở cả 4 môi trường.

`BLOCKED_EXTERNAL` cho tới khi pháp chế trả lại phiếu đã điền.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
