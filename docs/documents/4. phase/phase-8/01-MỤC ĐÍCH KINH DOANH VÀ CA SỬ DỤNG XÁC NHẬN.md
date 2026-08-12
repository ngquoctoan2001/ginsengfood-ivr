# IVR-01 - Business Purpose / Confirmation Use Case

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa mục đích nghiệp vụ và các use case xác nhận đơn hàng.

## 1. Mục tiêu

IVR được xây dựng để giảm đơn ảo, đơn sai số điện thoại, đơn đặt nhầm và đơn không xác nhận được, nhưng không làm phiền khách hàng tin cậy một cách đại trà.

IVR chỉ xác nhận ý chí đặt hàng ở mức tối thiểu, không xác nhận thanh toán, xuất kho, giao hàng, doanh thu hoặc kế toán.

## 2. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Mục đích pack, call script, phím bấm, attempt policy. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Eligibility, outcome classification, privacy-safe confirmation. |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md` | Official Order không đồng nghĩa Paid/Verified Revenue. |
| `docs/documents/4. phase/phase-8/ivr-input-baseline.md` | Nội dung cuộc gọi, DTMF và rule chương trình. |

## 3. Mục tiêu nghiệp vụ

| ID | Mục tiêu |
| --- | --- |
| IVR01-BG-001 | Xác nhận khách còn muốn tiếp tục xử lý đơn hàng. |
| IVR01-BG-002 | Giảm rủi ro đơn ảo, số điện thoại sai, khách đặt nhầm, khách không phản hồi. |
| IVR01-BG-003 | Tách rõ khách xác nhận, khách hủy, khách không nghe, số điện thoại invalid và lỗi kỹ thuật. |
| IVR01-BG-004 | Tránh gọi đại trà khách trusted nếu không có tín hiệu rủi ro mới. |
| IVR01-BG-005 | Cung cấp evidence cho Order Core ra quyết định, không thay Order Core. |

## 4. Tác nhân

| Actor | Vai trò |
| --- | --- |
| Khách hàng | Nhận cuộc gọi, bấm `1` để xác nhận hoặc `0` để hủy/không đặt. |
| Order Core | Tạo task, nhận callback, revalidate và transition order. |
| IVR Runtime | Điều phối queue, gọi, capture DTMF, normalize result. |
| IVR Operator | Theo dõi queue, SIM health, lỗi kỹ thuật. |
| Admin/Incident Manager | Pause queue, disable SIM, review exception theo permission. |
| Evidence/Audit Owner | Kiểm tra evidence, audit, release readiness. |

## 5. Tình huống nghiệp vụ chính

### UC-01 - Khách bấm `1`

Điều kiện:

- Official Order đủ điều kiện IVR.
- Phone valid.
- Không có Sale Lock/Recall/Suppression/call restriction.
- Attempt còn trong window.

Kết quả:

- IVR ghi `IVR_CONFIRMED`.
- IVR gửi callback về Order Core.
- Order Core revalidate.
- Nếu vẫn hợp lệ, Order Core chuyển đơn sang bước xử lý tiếp theo theo state machine.

### UC-02 - Khách bấm `0`

Kết quả:

- IVR ghi `IVR_CUSTOMER_CANCELLED`.
- IVR gửi callback về Order Core.
- Order Core revalidate và thực hiện cancel theo state machine.
- IVR không tự cancel order.

### UC-03 - Khách không nghe

Kết quả:

- IVR ghi no-answer attempt nếu attempt hợp lệ.
- Nếu chưa đạt max attempt, scheduler chờ attempt tiếp theo theo chương trình.
- Nếu đạt max attempt, IVR gửi `IVR_NO_ANSWER_FINAL` về Order Core.
- Notification chỉ được gửi bởi Notification Owner sau khi Order Core đã ra quyết định.

### UC-04 - Số điện thoại invalid

Kết quả:

- IVR không gọi nếu phone validation xác định invalid trước dispatch.
- IVR gửi/ghi outcome invalid phone theo policy.
- Invalid phone không được ghi thành no-answer.

### UC-05 - Lỗi kỹ thuật

Kết quả:

- IVR ghi `IVR_TECHNICAL_EXCEPTION`.
- Attempt kỹ thuật không tính là customer attempt.
- Hệ thống route admin review hoặc technical retry trong giới hạn policy.
- Không tự hủy đơn.

## 6. Nội dung cuộc gọi

Call script V1.0 chỉ được dùng biến đã duyệt:

- `order_code_short`.
- `total_amount_display`.
- `customer_name_short` nếu có và policy cho phép.
- `program_name` nếu cần.

Không được đọc:

- Full address.
- Payment detail.
- Member tier.
- Diamond/referral information.
- Health note.
- AI consultation content.
- CRM content.
- Full order history.

## 7. Quy tắc DTMF

| Phím | Ý nghĩa | Outcome |
| --- | --- | --- |
| `1` | Khách xác nhận tiếp tục xử lý đơn | `IVR_CONFIRMED` |
| `0` | Khách không đặt hoặc muốn hủy | `IVR_CUSTOMER_CANCELLED` |
| Không bấm | Không có xác nhận hợp lệ | `IVR_NO_ANSWER_ATTEMPT` hoặc `IVR_NO_ANSWER_FINAL` |
| Sai phím | Input không hợp lệ | Xử lý theo policy attempt |
| Lỗi DTMF | Lỗi kỹ thuật | `IVR_TECHNICAL_EXCEPTION` |

`KEY_9_HUMAN_SUPPORT` chưa bật trong V1.0. Nếu mở sau này phải có owner decision, CSKH capacity và SRS update.

## 8. Quy tắc nghiệp vụ

| ID | Rule |
| --- | --- |
| IVR01-BR-001 | IVR confirmation không đồng nghĩa `PAID`. |
| IVR01-BR-002 | IVR confirmation không đồng nghĩa `DELIVERED`. |
| IVR01-BR-003 | IVR confirmation không đồng nghĩa `VERIFIED_REVENUE`. |
| IVR01-BR-004 | IVR không được dùng làm kênh marketing, upsell, CRM hoặc chăm sóc khách hàng đại trà. |
| IVR01-BR-005 | Không có evidence thì kết quả IVR không được dùng làm căn cứ transition. |

## 9. Tiêu chí chấp nhận

- Use case trả lời đủ cho confirm, cancel, no-answer, invalid phone và technical failure.
- DTMF result tách rõ business outcome và technical exception.
- Không có câu nào cho phép IVR tự cập nhật order.
- Call script không chứa dữ liệu riêng tư ngoài phạm vi.

## 10. Business capability breakdown

| Capability | Người dùng/hệ thống hưởng lợi | Giá trị nghiệp vụ | Không được hiểu là |
| --- | --- | --- | --- |
| Auto confirm order intent | Order Core, vận hành đơn | Giảm đơn ảo, đơn đặt nhầm, phone sai. | Xác nhận thanh toán. |
| Customer cancel by key `0` | Khách hàng, Order Core | Ghi nhận ý chí không đặt/hủy. | IVR tự hủy đơn. |
| No-answer classification | Order Core, Ops | Có signal để xử lý đơn không xác nhận. | Notification tự động. |
| Invalid phone classification | Order Core, Admin | Tách phone sai khỏi khách không nghe. | Kết luận khách cố tình không nhận. |
| Technical exception classification | IVR Ops | Không làm sai attempt/no-answer. | Lý do hủy đơn. |
| Admin monitoring | Ops/Admin | Điều hành queue/SIM/review an toàn. | Quyền override order. |

## 11. Use case catalog

| Use case ID | Tên | Primary actor | Trigger | Final owner |
| --- | --- | --- | --- | --- |
| IVR01-UC-001 | Xác nhận đơn bằng phím `1` | Khách hàng | Official Order cần IVR | Order Core |
| IVR01-UC-002 | Hủy/không đặt bằng phím `0` | Khách hàng | Khách nghe call và bấm `0` | Order Core |
| IVR01-UC-003 | Không nghe trong Golden Hour | Khách hàng không nghe | 2 attempt hợp lệ đều no-answer | Order Core |
| IVR01-UC-004 | Không nghe trong 24/7 | Khách hàng không nghe | 3 attempt hợp lệ đều no-answer | Order Core |
| IVR01-UC-005 | Trusted customer skip | Order Core | Trust policy cho phép skip | Order Core |
| IVR01-UC-006 | Invalid official phone | Phone resolver | Phone invalid/missing/not official | Order Core/Admin |
| IVR01-UC-007 | Technical failure | IVR runtime/SIM | SIM/server/DTMF/evidence lỗi | IVR Ops/Admin |
| IVR01-UC-008 | Operational blocker after confirm | Operational Core | Sale Lock/Recall/payment issue xuất hiện | Order Core |
| IVR01-UC-009 | Admin pause queue | Admin/Ops | Capacity/security/incident | IVR Ops |
| IVR01-UC-010 | Manual technical retry | Admin/Ops | Technical exception retryable | IVR Ops |

## 12. Detailed scenario - customer confirms

Preconditions:

- Official Order đã tạo.
- Order state còn callable.
- Customer không được trusted skip.
- Official contact valid.
- Program policy resolved.
- Không có Sale Lock/Recall/Suppression/opt-out.

Main flow:

1. Order Core phát hành task.
2. IVR tạo CallJob và attempt schedule.
3. Scheduler dispatch attempt trong window.
4. SIM Adapter phát script đã duyệt.
5. Khách bấm `1`.
6. Result Normalizer tạo `IVR_CONFIRMED`.
7. IVR callback Order Core.
8. Order Core revalidate.
9. Nếu pass, Order Core transition đơn theo state machine.

Alternate flows:

| Điều kiện | Kết quả |
| --- | --- |
| Callback stale | Order Core reject stale; order không đổi. |
| Sale Lock xuất hiện | Order Core block/hold; order không confirm. |
| Evidence thiếu | Route admin review/technical exception. |
| Duplicate callback | Idempotency trả kết quả cũ. |

## 13. Detailed scenario - customer cancels

Preconditions giống confirm.

Main flow:

1. Khách bấm `0`.
2. Result Normalizer tạo `IVR_CUSTOMER_CANCELLED`.
3. IVR callback Order Core.
4. Order Core revalidate.
5. Nếu chính sách cho phép, Order Core hủy đơn.
6. Notification owner gửi thông báo nếu có template/rule được duyệt.

Hard blocks:

- IVR/SIM không tự gửi thông báo hủy.
- Admin IVR không sửa result thành cancel.
- Technical exception không được chuyển thành cancel.

## 14. Detailed scenario - no-answer

Golden Hour:

- Attempt 1 tại `T0`.
- Attempt 2 tại `T0 + 5`.
- Final no-answer nếu attempt 2 hợp lệ không nghe hoặc hết window theo policy.

24/7:

- Attempt 1 tại `T0`.
- Attempt 2 tại `T0 + 5`.
- Attempt 3 tại `T0 + 10`.
- Final no-answer nếu attempt 3 hợp lệ không nghe hoặc hết window theo policy.

No-answer không được sinh khi:

- SIM lỗi.
- Server lỗi.
- DTMF capture lỗi.
- Evidence writer lỗi.
- Callback lỗi.
- Phone validation technical error.

## 15. Business rule matrix

| Rule | Allowed | Blocked |
| --- | --- | --- |
| Khách mới/untrusted | Có thể IVR nếu đủ điều kiện. | Không skip nếu risk flag. |
| Khách trusted | Có thể skip theo trust decision. | Không hardcode trong IVR. |
| Phone invalid | Không gọi. | Không count no-answer. |
| Key `1` | Signal confirm. | Không force confirm order. |
| Key `0` | Signal cancel request. | Không tự hủy order. |
| No-answer max | Signal final no-answer. | Không tự notification. |
| Technical failure | Technical exception. | Không count customer attempt. |

## 16. Business acceptance tests

| Test ID | Given | When | Then |
| --- | --- | --- | --- |
| IVR01-BAT-001 | Official Order untrusted | Khách bấm `1` | Callback confirm, Order Core revalidate. |
| IVR01-BAT-002 | Official Order untrusted | Khách bấm `0` | Callback cancel signal, Core quyết định. |
| IVR01-BAT-003 | Golden Hour | 2 no-answer hợp lệ | Final no-answer, không attempt 3. |
| IVR01-BAT-004 | 24/7 | 3 no-answer hợp lệ | Final no-answer, không attempt 4. |
| IVR01-BAT-005 | Trusted skip | Trust pass | Không tạo call job. |
| IVR01-BAT-006 | Phone invalid | Task created | Không dispatch SIM. |
| IVR01-BAT-007 | SIM failure | During attempt | Technical exception, attempt not counted. |
| IVR01-BAT-008 | Sale Lock after key `1` | Callback | Core block/hold, không confirm. |
