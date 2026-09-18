# DSAR runbook — `W-0052` · `P10-1`

Ngày: `2026-08-19` · sửa `2026-09-18` (`W-0314`) · Phạm vi: **chỉ dữ liệu IVR giữ**. Đơn hàng, khách hàng và liên hệ thuộc Sales;
yêu cầu về những thứ đó phải đi tới Sales.

## 1. Vì sao không có endpoint HTTP

Xoá dữ liệu khách cần một **thẩm quyền riêng**. Treo chức năng xoá lên một permission vận hành sẵn
có — ví dụ `IVR_QUEUE_VIEW` — nghĩa là **ai xem được hàng đợi thì xoá được dữ liệu khách**.

Cập nhật 2026-08-22 (`OD-V1-20`): `IVR_RUNTIME_GATE_ADMIN` **đã được cấp cho role `Admin`**. Điều đó
**không** làm nó thành permission đúng cho DSAR — nó là quyền đổi runtime gate (kill switch, allowlist,
cho phép gọi khách thật), nên treo xoá lên nó chỉ đổi câu trên thành *ai bật/tắt được kill switch thì
xoá được dữ liệu khách*. Cùng một lỗi ghép quyền, khác cái tên.

Nên `DsarService` vẫn chạy qua thủ tục tay có audit dưới đây, và endpoint **chờ một permission DSAR
riêng**. Đây là quyết định, không phải thiếu sót.

## 2. Bốn điều nói với người yêu cầu **trước** khi bắt đầu

Không phát hiện giữa chừng:

1. **Audit không xoá được.** `ivr_audit_log` và `ivr_admin_actions` là append-only **ép bởi
   database** — `UPDATE` và `DELETE` bị từ chối. Một bản ghi *ai đã làm gì* mà chủ thể xoá được thì
   không phải bản ghi.
2. **`order_code` được giữ.** Đó là khoá mà yêu cầu đi tới. Xoá nó làm **mọi** yêu cầu sau về cùng
   đơn không trả lời được, kể cả của chính người đó.
3. **Payload callback được giữ tới hết hạn retention.** Đó là bản ghi giao nhận với Sales; bỏ payload
   đi thì nó không còn giải quyết được tranh chấp mà nó tồn tại vì.
4. **`customer_id` được giữ.** Khoá khách của Sales, giữ vì cùng lý do với `order_code`: là cách đối
   chiếu lịch sử đơn với Sales, và tự nó không định danh ai nếu không có dữ liệu của Sales. Số điện
   thoại, khoá liên hệ và hai giá trị trust thì **bị xoá** (Toàn chốt `18/09`, `W-0314`).

## 3. Quy trình

### 3.1 Truy cập (yêu cầu xem)

1. Xác minh danh tính chủ thể — **Sales làm**, không phải IVR. IVR không có cách xác minh ai là ai.
2. Chạy `FindAsync(orderCode)`.
3. Kết quả là **số lượng theo bảng**, không phải giá trị. Đây là chủ ý: một dịch vụ in ra dữ liệu cá
   nhân đã lưu là **một lối đọc mới**, mở cho bất kỳ ai gọi được dịch vụ.
4. Người trả lời ghép câu trả lời từ số lượng này + dữ liệu Sales. Nếu cần giá trị cụ thể, lấy qua
   console admin dưới quyền đã có và ghi audit của chính console đó.

### 3.2 Xoá (yêu cầu xoá)

1. **Chạy dry-run trước.** `EraseAsync(..., dryRun: true)` báo sẽ chạm gì, đổi **không gì cả**, và
   vẫn ghi một dòng audit.
2. Đọc lại §2 với người yêu cầu.
3. Chạy thật: `EraseAsync(..., dryRun: false)` với **lý do ≥ 8 ký tự** — lý do đi vào audit, và
   `"ok"` ở ô đó bằng không có bản ghi.
4. Kết quả: các trường của `ivr_confirmation_tasks` bị redact bằng **đúng câu SQL retention job
   dùng** — `phone_ref`, `phone_masked`, `phone_validation_status`, `dial_token_ciphertext`,
   `privacy_safe_order_summary_json`, và từ `W-0314` đặt NULL `phone_e164` (chính số điện thoại),
   `official_contact_id`, `customer_trust_status`, `trusted_skip_allowed` — và `anonymized_at` được
   đặt. `customer_id` giữ lại (§2 mục 4).
5. Ghi `audit_ref` trả về vào hồ sơ yêu cầu.

**Chạy lại cho cùng một đơn là an toàn** (`W-0314`). Câu xoá chỉ chạm task **chưa** xoá: task Sales gửi
lại sau lần xoá trước sẽ được xoá ở lần sau, task đã xoá giữ nguyên dấu thời gian, và dry-run đếm
đúng những task lần chạy thật sẽ chạm. Trước `W-0314`, lần xoá thứ hai cho một đơn **ném lỗi** và
không xoá được gì.

### 3.4 Không chạy xoá trong lúc rollout `W-0314`

Pod cũ chạy câu xoá thiếu `phone_e164`; trigger mới **từ chối** câu đó trên dòng có số. Lỗi theo hướng
đóng — không dòng nào bị đánh dấu đã xoá khi chưa xoá — nhưng yêu cầu sẽ hỏng giữa chừng. Chờ mọi pod
lên bản mới rồi mới chạy. Task chỉ mang token vẫn xoá được bằng pod cũ.

**Phạm vi nổ đúng một đơn.** `COMP-DSAR-02` khẳng định đơn thứ hai **không bị chạm** — một lần xoá
DSAR lan sang khách khác là một vụ rò rỉ gây ra **trong lúc** đang tôn trọng quyền riêng tư.

### 3.3 Yêu cầu về đơn IVR không giữ

`FindAsync` trả `Found=false` kèm **vẫn đủ** danh sách giới hạn ở §2. "Chúng tôi không giữ gì" là một
câu trả lời phải đầy đủ như mọi câu trả lời khác, nếu không yêu cầu sau lại hỏi đúng câu đó.

## 4. Thời hạn

Chưa có thời hạn nào được ký. Luật áp dụng và deadline phản hồi là đầu vào của Legal (`W-0009`); ghi
ra để trống thay vì điền một con số nghe hợp lý.

## 5. Cái runbook này KHÔNG làm được

- **Không xác minh danh tính.** IVR không có kênh nào để làm việc đó.
- **Không chạm dữ liệu Sales.** Đơn, khách và liên hệ thuộc hệ thống khác.
- **Không xoá bản backup.** Một bản backup còn trong hạn vẫn chứa dữ liệu trước khi redact. Nó hết
  theo lịch prune (`docs/dr-topology.md` §4), và **không có cơ chế nào xoá có chọn lọc bên trong một
  bản backup đã mã hoá**. Đây là giới hạn thật, phải nói với người yêu cầu.
- **Không có endpoint** — xem §1.
- **Chưa có lối chạy nào.** `DsarService` hiện **chỉ được test gọi**: không endpoint, không CLI, không
  script, và không đăng ký trong DI. Thủ tục ở §3 vì vậy chưa làm được nếu không có người viết code.
  Vì `S3`, đây lại là luồng xoá **duy nhất**. Đã ghi thành vướng mắc để Toàn/Sếp quyết ai được xoá và
  bằng công cụ gì (`W-0314`,
  [bản vướng mắc `17/09`](../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md)).
