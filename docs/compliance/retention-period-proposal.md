# Kỳ hạn lưu trữ IVR — phiếu điền cho pháp chế

Ngày dựng: 2026-09-10 · Work item: `W-0273` · Trạng thái: **chờ pháp chế điền số**

## 0. Tài liệu này là gì, và không là gì

**Là**: mô tả chính xác, rút từ mã nguồn, về **mỗi nhóm dữ liệu IVR đang giữ** — nó chứa gì, có dữ
liệu cá nhân không, hết hạn thì hệ thống **xoá** hay **ẩn danh**, bảng nào bị chạm, và **cái gì hỏng
nếu đặt kỳ hạn quá ngắn**.

**Không là**: đề xuất con số. Kỳ hạn lưu trữ là nghĩa vụ pháp lý, không phải lựa chọn kỹ thuật.
Cột cuối để trống — pháp chế điền.

## 1. Trạng thái hiện tại: chưa xoá gì cả

`Ivr:Retention:PeriodDays` đang là `{}` ở mọi môi trường. `RetentionPolicyProvider` đọc thiếu khoá
thì coi như **không có kỳ hạn**, nên job retention **không xoá và không ẩn danh dòng nào**.

Hôm nay điều đó vô hại: chưa có dữ liệu khách hàng thật (`REAL_CUSTOMER_CALL_ALLOWED=NO` ở cả 4 môi
trường, mọi bằng chứng đều là mock). Nó thành rủi ro tuân thủ **đúng vào ngày đầu tiên có dữ liệu
thật**, không phải muộn hơn.

## 2. Điền một số thì chuyện gì xảy ra

| Cơ chế | Hành vi |
| --- | --- |
| `Delete` | Xoá hẳn dòng khi `<cột mốc> + kỳ hạn` đã qua |
| `Anonymize` | **Giữ dòng**, thay các trường cá nhân bằng giá trị đã che |
| `LegalHoldUntil` | Dòng có giá trị này ở tương lai thì **được miễn**, bất kể kỳ hạn |
| `protectedSql` | Điều kiện miễn trừ riêng của từng nhóm (xem `evidence_link`) |

Job chạy một lần rồi thoát (`RetentionRunOnceHost`), mặc định là **dry run**.

## 3. Chín nhóm dữ liệu

Cột **"Quá ngắn thì mất gì"** là hệ quả vận hành, do kỹ thuật xác định. Cột **"Số ngày"** do pháp chế
điền.

### 3.1 Nhóm có dữ liệu cá nhân

| Nhóm | Chiến lược | Bảng (cột mốc) | Dữ liệu cá nhân | Quá ngắn thì mất gì | **Số ngày** |
| --- | --- | --- | --- | --- | --- |
| `speech_snapshot` | **Ẩn danh** | `ivr_confirmation_tasks` (`created_at`) | **9 cột** — `phone_ref`, `phone_masked`, `dial_token_ciphertext`, `privacy_safe_order_summary_json`, `order_code`, `customer_id`, `customer_trust_status`, `official_contact_id`, `phone_validation_status` | Mất khả năng dựng lại **nội dung đã đọc cho khách nghe** khi có khiếu nại | ☐ |
| `task_metadata` | Xoá | `ivr_confirmation_tasks` (`created_at`), `ivr_call_jobs`, `ivr_task_intake_outbox`, `ivr_capacity_incidents` | Cùng 9 cột trên (**cùng bảng** với `speech_snapshot`) | Mất **toàn bộ dấu vết** đơn nào đã được gọi | ☐ |
| `raw_call_event` | Xoá | `ivr_raw_call_events` (`received_at`) | `recording_ref` — ghi âm **mặc định TẮT** (DT-05), cột này `null` trừ khi có phê duyệt riêng | Mất chẩn đoán sự cố kỹ thuật ở tầng thấp | ☐ |
| `callback_metadata` | Xoá | `ivr_result_callbacks` (`created_at`) | `payload_json` — thân callback gửi Sales | Mất bằng chứng **đã báo kết quả cho Sales**, thứ dùng khi hai bên bất đồng | ☐ |
| `idempotency_key` | Xoá | `ivr_idempotency_keys` (`created_at`) | `response_snapshot_json` — ảnh chụp phản hồi | Request lặp lại sau khi hết hạn sẽ **chạy lại** thay vì trả cùng đáp án | ☐ |

> **Ràng buộc thứ tự, cần chú ý khi điền.** `speech_snapshot` và `task_metadata` chạm **cùng một
> bảng**. Nếu `speech_snapshot` **dài hơn** `task_metadata` thì dòng bị xoá trước khi kịp ẩn danh —
> bước ẩn danh không bao giờ chạy. Muốn "che dữ liệu cá nhân sớm, giữ khung đơn lâu hơn" thì
> `speech_snapshot` phải **ngắn hơn hoặc bằng** `task_metadata`.

### 3.2 Nhóm không có dữ liệu cá nhân

| Nhóm | Chiến lược | Bảng (cột mốc) | Nội dung | Quá ngắn thì mất gì | **Số ngày** |
| --- | --- | --- | --- | --- | --- |
| `attempt_metadata` | Xoá | `ivr_call_attempts` (`scheduled_at`), `ivr_technical_exceptions` (`created_at`) | Đã gọi mấy lần, lúc nào, hỏng ra sao | Mất cơ sở đối chiếu chính sách số lần gọi | ☐ |
| `result_metadata` | Xoá | `ivr_call_results` (`created_at`) | Khách bấm gì, phân loại kết quả | Mất **kết quả xác nhận** — thứ dùng khi khách phủ nhận đã đồng ý | ☐ |
| `review_item` | **Ẩn danh** | `ivr_review_items` (`resolved_at`) | Việc cần người xem lại | Mất lịch sử xử lý ngoại lệ | ☐ |
| `evidence_link` | Xoá | `ivr_evidence_links` (`created_at`), `ivr_evidence` (`created_at`) | Con trỏ tới gói bằng chứng | **Đã có miễn trừ**: dòng có `accepted_at IS NOT NULL` **không bao giờ bị xoá**, bất kể kỳ hạn | ☐ |

## 4. Ba điều pháp chế nên biết trước khi điền

**`ivr_audit_log` không nằm trong danh sách này.** Nó là bảng **append-only**, có trigger chặn
`UPDATE`/`DELETE`, và **không có nhóm retention nào**. Nghĩa là nhật ký kiểm toán được giữ **vĩnh
viễn**. Nếu điều đó sai về mặt pháp lý thì đây là một quyết định riêng, không điền được bằng bảng
trên.

**Ẩn danh không phải xoá.** `speech_snapshot` và `review_item` **giữ nguyên dòng**. Nếu nghĩa vụ là
"xoá dữ liệu", chiến lược của hai nhóm này phải đổi — đó là thay đổi mã nguồn, không phải thay số.

**Xoá là không hoàn tác được, và job hiện mặc định dry run.** Điền số xong vẫn cần một bước bật thật
sự. Đề nghị: chạy dry run trên staging có dữ liệu đại diện trước, đọc số dòng sẽ bị chạm, rồi mới bật.

## 5. Sau khi pháp chế điền

Kỹ thuật sẽ: đặt số vào `Ivr:Retention:PeriodDays` cho từng môi trường, thêm test khoá từng kỳ hạn
(để đổi số là một thay đổi có chủ ý, không phải trôi cấu hình), chạy dry run và ghi lại số dòng bị
chạm trước khi bật.

Không tự điền số nào. Ô trống ở trên là ô trống thật.
