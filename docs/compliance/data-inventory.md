# Data inventory & legal basis — `W-0052` · `P10-1`

Ngày: `2026-08-19` · sửa `2026-09-18` (`W-0314`) · Nguồn sự thật: `src/Ivr.Infrastructure/Governance/PersonalDataInventory.cs`
· Trạng thái pháp lý: **`LEGAL_REVIEW_PENDING`** (`W-0009`)

## 1. Tài liệu này không phải nguồn sự thật

Danh mục nằm **trong code**; tài liệu được kiểm **ngược lại** nó. `COMP-PII-01` đỏ nếu một cột mang
dấu hiệu dữ liệu cá nhân xuất hiện trong model mà không có ở đây, và đỏ nếu tài liệu thiếu một mục
code có.

Một danh mục privacy là loại tài liệu **đúng vào ngày viết và sai một tháng sau**, vì không có gì
trong việc thêm một cột khiến ai đó mở nó ra. Đọc thẳng model EF biến điều đó từ thói quen thành cổng.

**Cổng đã bắt ngay lần chạy đầu**: năm cột chưa được phân loại (`invalid_phone_count`,
`invalid_phone`, `dial_token_expires_at`, `phone_validation_status`, `customer_attempt_counted`) và
một mục tôi viết cho cột **không tồn tại**. Bốn cột là dương tính giả có lý do, một cột
(`phone_validation_status`) là dữ liệu cá nhân thật mà tôi đã bỏ sót.

## 2. Cơ sở pháp lý

Chỉ **hai** cơ sở, và không cơ sở nào là **đồng ý (consent)**:

| Cơ sở | Áp cho | Vì sao |
| --- | --- | --- |
| `ContractPerformance` | dữ liệu phục vụ việc xác nhận đơn | Gọi xác nhận COD là **một bước thực hiện hợp đồng** khách đã giao kết, không phải tiếp thị về nó |
| `LegalRecordKeeping` | audit, payload callback đã gửi | Bản ghi về việc *đã quyết gì* phải tồn tại **độc lập** với bên mà nó nói tới |

**Không mục nào dựa trên đồng ý**, và đó là khẳng định có kiểm chứng chứ không phải lựa chọn văn
phong: IVR **không bao giờ** hiển thị hộp thoại xin đồng ý và **không lưu** quyết định đồng ý nào.
Một trường khai cơ sở là "đồng ý" sẽ là khai một cơ sở **chưa ai thu thập**. `COMP-PII-01` khẳng
định điều này.

Hệ quả trực tiếp: do-not-call cho `PHONE_CALL` được tôn trọng như một **chặn cứng**, không phải một
lợi ích chính đáng đem ra cân đo. Xem `COMP-DNC-03`.

## 3. Danh mục trường

| Trường | Mục đích | Cơ sở | Xoá theo DSAR |
| --- | --- | --- | --- |
| `ivr_confirmation_tasks.phone_ref` | tham chiếu mờ, adapter SIM giải ra số lúc quay (IVR **không bao giờ** thấy số — D-05) | hợp đồng | thay bằng giá trị redacted |
| `ivr_confirmation_tasks.phone_masked` | dạng hiển thị cho console, phân biệt hai đơn mà không lộ số | hợp đồng | thay bằng giá trị redacted |
| `ivr_confirmation_tasks.phone_validation_status` | số liên hệ có qua kiểm tra không — **là dữ kiện về liên hệ của khách**, nên vào danh mục dù không chứa số | hợp đồng | redacted cùng tham chiếu nó mô tả |
| `ivr_confirmation_tasks.dial_token_ciphertext` | token quay số một lần, đã mã hoá, TTL ≤ cửa sổ xác nhận. Với task chỉ gửi số (`W-0312`) cột này giữ **tham chiếu quay trực tiếp** của chính task — SHA-256 của `task_id`, không định danh ai | hợp đồng | redacted; token đã hết hạn từ rất lâu trước khi có thể xoá |
| `ivr_confirmation_tasks.phone_e164` | ⚠️ **số điện thoại khách, để nguyên, không mã hoá** (`W-0310` phương án `B`). Đây là trường **nhạy cảm nhất** IVR giữ và là trường duy nhất định danh trực tiếp một con người, thay vì qua một khoá do bên khác giữ. Chủ sở hữu chọn ngày `2026-09-17` rằng M3 gửi thẳng số thay vì dial token — đổi lại **bỏ được kho khoá** và `3` quyết định treo (`OD-V1-05`/`17`/`18`); dòng này là **mặt còn lại** của lựa chọn đó. Chỉ đọc trên **đúng một đường** — lúc quay số — còn `phone_masked` vẫn là thứ mọi log, audit, callback và API quản trị dùng | hợp đồng | đặt NULL. **Khác token: nó không tự hết hạn**, nên xoá là cách duy nhất làm nó biến mất. *Tới `W-0314`, câu xoá dùng chung bỏ sót cột này — dòng đã xoá vẫn giữ nguyên số* |
| `ivr_confirmation_tasks.privacy_safe_order_summary_json` | các trường **được whitelist** cho script đọc — không địa chỉ, không chi tiết thanh toán, không ghi chú sức khoẻ (`OD-V1-15`) | hợp đồng | thay bằng giá trị redacted |
| `ivr_confirmation_tasks.order_code` | khoá nghiệp vụ Sales, để đối chiếu hệ thống đơn | hợp đồng | **giữ** — đây là khoá mà một yêu cầu DSAR đi tới, xoá nó làm yêu cầu sau **không trả lời được** |
| `ivr_confirmation_tasks.customer_id` | khoá khách của Sales, chỉ có khi task mang theo | hợp đồng | **giữ**, như `order_code` — là cách đối chiếu lịch sử đơn với Sales, và tự nó không định danh ai nếu không có dữ liệu của Sales. Có trong `NotErasable` nên người yêu cầu được báo trước (Toàn chốt `18/09`, `W-0314`) |
| `ivr_confirmation_tasks.customer_trust_status` | tín hiệu trust **legacy** từ CRM (`OD-18` LEGACY_READ), lưu nguyên để giữ lịch sử — **không quyết định gọi/bỏ qua nào đọc nó** từ `draft.21` | hợp đồng | đặt NULL (`W-0314`). Trước đây *"không xoá riêng được vì là đầu vào của một quyết định"* — từ `OD-18` nó là đầu vào của **không** quyết định nào |
| `ivr_confirmation_tasks.trusted_skip_allowed` | cờ **legacy** từ CRM: có được bỏ qua cuộc gọi xác nhận cho khách này không (`OD-18` LEGACY_READ). Không quyết định nào đọc, nhưng vẫn là một dữ kiện về một người | hợp đồng | đặt NULL, cùng trust status nó đi kèm (`W-0314`) |
| `ivr_confirmation_tasks.official_contact_id` | khoá liên hệ Sales, xác định gọi ai trên đơn | hợp đồng | đặt NULL (`W-0314`). Trước đây để *"xoá cùng dòng khi hết hạn"* — `S3` bỏ kỳ hạn nên thực tế **không bao giờ** bị xoá |
| `ivr_raw_call_events.recording_ref` | tham chiếu bản ghi âm — **OFF mặc định** (DT-05), null trừ khi có phê duyệt riêng | hợp đồng | xoá cùng dòng raw event; **không có bản ghi nào để xoá** ngay từ đầu |
| `ivr_idempotency_keys.response_snapshot_json` | response đã trả, để lần thử lại trả cùng đáp án | hợp đồng | xoá khi key hết hạn — dữ liệu cá nhân **sống ngắn nhất** IVR giữ |
| `ivr_result_callbacks.payload_json` | bản sao bất biến của thứ đã gửi Sales, để tranh chấp giao nhận có câu trả lời | ghi chép pháp lý | xoá cùng dòng callback khi hết hạn; **không** xoá sớm được — một bản ghi giao nhận đã bỏ payload không giải quyết được tranh chấp nó tồn tại vì |
| `ivr_audit_log.actor_id` | ai đã thực hiện hành động quản trị | ghi chép pháp lý | **KHÔNG BAO GIỜ xoá** |
| `ivr_admin_actions.actor_id` | ai đã thực hiện hành động nào lên đối tượng nào, kèm lý do | ghi chép pháp lý | **KHÔNG BAO GIỜ xoá** |
| `fact_call_outcome.order_ref_hash` | SHA-256 mã đơn, để báo cáo đếm số đơn phân biệt mà không mang mã | hợp đồng | xoá khi kết quả nguồn bị xoá — hook retention làm chu kỳ warehouse **bằng** chu kỳ nguồn |
| `fact_call_job.order_ref_hash` | cùng hash ở hạt job | hợp đồng | xoá khi job nguồn bị xoá |

## 4. Giới hạn của quyền xoá, nói trước chứ không nói lúc nhận yêu cầu

Bốn nhóm **không xoá được**, và mỗi nhóm có lý do khác nhau:

1. **Audit** (`ivr_audit_log`, `ivr_admin_actions`) — append-only **ép bởi database**: `UPDATE` và
   `DELETE` bị từ chối. Một bản ghi *ai đã làm gì* mà chủ thể xoá được thì không phải bản ghi.
2. **`order_code`** — là khoá mà yêu cầu DSAR đi tới. Xoá nó làm mọi yêu cầu sau về cùng đơn
   **không trả lời được**, kể cả yêu cầu của chính người đó.
3. **`payload_json` của callback** — bản ghi giao nhận. Bỏ payload đi thì nó không còn giải quyết
   được tranh chấp mà nó tồn tại vì.
4. **`customer_id`** — khoá khách của Sales, giữ vì cùng lý do với `order_code`: là cách đối chiếu
   lịch sử đơn với Sales. Khoá liên hệ, hai giá trị trust và số điện thoại thì **bị xoá** (Toàn chốt
   `18/09`, `W-0314`).

Runbook DSAR nói đúng bốn điều này **trước** khi xử lý yêu cầu, chứ không phát hiện ra giữa chừng.
`COMP-PII-02` giữ cho danh sách đó khớp với danh mục: trường nào danh mục ghi *"Retained"* mà
`NotErasable` không nêu tên thì đỏ.

## 5. Cái này KHÔNG chứng minh

- **Chưa ai ở Legal ký.** Cơ sở pháp lý ở §2 là **đề xuất kỹ thuật**, không phải kết luận pháp lý
  (`W-0009` vẫn `BLOCKED_EXTERNAL`).
- **Cổng là heuristic theo tên cột.** Nó bắt `customer_email`; nó **không** bắt một cột tên
  `notes_2` chứa số điện thoại. Lớp giá trị (`PiiGuard`) là thứ bắt cái đó, và cũng không hoàn hảo.
- **Chu kỳ retention chưa ai ký** — xem `docs/compliance/retention.md`.
- **Danh mục chỉ phủ PostgreSQL.** Log, metric và evidence file không nằm trong model EF nên không
  cổng nào ở đây chạm tới chúng.
