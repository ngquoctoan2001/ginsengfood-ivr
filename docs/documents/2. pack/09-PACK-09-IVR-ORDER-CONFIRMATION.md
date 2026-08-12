# PACK-09 - IVR ORDER CONFIRMATION

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay nhu `GOLDEN_HOUR 2/10` va `TWENTY_FOUR_SEVEN 3/15` chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca hai chuong trinh; `GOLDEN_HOUR` window 5 phut voi A2@`T0+2:30`; `TWENTY_FOUR_SEVEN` window 15 phut voi A2@`T0+7:30`. Nguon canonical: `plan/ivr-orther/decisions-log.md` D-10 va `specs/functional/03-scheduler-attempt-policy.md`.

## IVR ORDER CONFIRMATION / INTERNAL SIM GATEWAY GOVERNANCE

## 1.2. Vai trò tài liệu

PACK-09 là tài liệu quản trị cấp pack cho hợp phần IVR Order Confirmation của hệ thống Ginsengfood.

PACK-09 chuẩn hóa:

- Cuộc gọi tự động xác nhận đơn hàng.

- Mô hình triển khai bằng Internal SIM Gateway Server.

- Ranh giới giữa IVR và Core Order State Machine.

- Ranh giới giữa IVR và AI Advisor.

- Ranh giới giữa IVR và Facebook Gateway / Messenger / Live.

- Ranh giới giữa IVR và CRM / Member Lifecycle.

- Ranh giới giữa IVR và Payment / Warehouse / MISA / Traceability.

- Ranh giới giữa IVR và Notification owner.

- Nguyên tắc SIM vật lý.

- Nguyên tắc call job.

- Nguyên tắc DTMF.

- Nguyên tắc audit/evidence.

- Nguyên tắc gửi tin nhắn giải thích sau khi đơn bị Core hủy.

- Điều kiện triển khai, test, smoke, security và release gate.

PACK-09 không phải tài liệu marketing.

PACK-09 không phải tài liệu CRM.

PACK-09 không phải tài liệu chăm sóc khách hàng.

PACK-09 không phải tài liệu bán hàng.

PACK-09 là lớp xác nhận đơn hàng bằng cuộc gọi tự động có kiểm soát, phục vụ Core Order State Machine ra quyết định tiếp tục xử lý hoặc hủy đơn theo rule.

## 2. PACK STATUS

## 2.1. Trạng thái pack

## PACK_09_NAME = IVR_ORDER_CONFIRMATION

## PACK_09_VERSION = V1.1_CLEAN_FINAL

PACK-09 đã được mở ở tầng tài liệu governance.

PACK-09 chưa được triển khai production.

PACK-09 chưa được gọi khách thật.

PACK-09 chưa được cập nhật trạng thái đơn thật.

PACK-09 chưa được cho phép downstream phụ thuộc IVR result.

PACK-09 chưa được cho phép gửi post-IVR cancellation notice production.

## 2.2. Trạng thái gate

## CALLABLE_IVR_API_ALLOWED_IN_PRODUCTION = KHÔNG

## IVR_RESULT_SIMULATION_ALLOWED = KHÔNG

## DOWNSTREAM_IVR_DEPENDENCY_ALLOWED = KHÔNG

## POST_IVR_CANCELLATION_NOTICE_ALLOWED_IN_PRODUCTION = KHÔNG

Ý nghĩa:

- IVR chưa được mở production.

- Không module nào được gọi IVR production.

- Không được giả lập IVR result để downstream dùng như dữ liệu thật.

- Không được để Order, AI, Facebook Gateway, CRM, Admin, Warehouse, Payment hoặc MISA phụ thuộc IVR result trước khi PACK-09 release.

- Không được để Notification owner gửi tin nhắn hủy đơn production nếu chưa có release decision.

- IVR chỉ được chuẩn bị kỹ thuật nội bộ trong phạm vi test/sandbox theo rule.

## 2.3. Trạng thái toàn hệ

PACK-09 hoàn tất tài liệu không đồng nghĩa hệ thống được go-live.

Không được gọi:

- Gateway PASS.

- Completion PASS.

- Production Ready.

- Release Approved.

- Go-live Approved.

nếu chưa có implementation, test, smoke, security, evidence accepted, owner sign-off và release decision.

## 3. PACK PURPOSE

## 3.1. Mục đích duy nhất

PACK-09 phục vụ một nghiệp vụ duy nhất:

## CALL_PURPOSE = ORDER_CONFIRMATION_ONLY

Mục tiêu:

- Gọi tự động xác nhận đơn hàng.

- Giảm đơn ảo.

- Giảm đơn sai số điện thoại.

- Giảm đơn đặt nhầm.

- Giảm đơn khách không xác nhận.

- Hỗ trợ Core Order State Machine quyết định tiếp tục xử lý hoặc hủy đơn theo rule.

- Tạo audit/evidence cho trạng thái xác nhận đơn.

- Bảo vệ quy trình vận hành khỏi đơn hàng không chắc chắn.

- Tạo cơ sở hợp lệ để Notification owner gửi tin nhắn giải thích sau khi Core đã hủy đơn, nếu đúng điều kiện.

## 3.2. IVR không phải kênh bán hàng

PACK-09 không được biến IVR thành:

- Kênh bán hàng.

- Kênh tư vấn sản phẩm.

- Kênh upsell.

- Kênh cross-sell.

- Kênh CRM.

- Kênh chăm sóc khách hàng.

- Kênh nhắc mua lại.

- Kênh thông báo chương trình.

- Kênh mời thành viên.

- Kênh mời Diamond.

- Kênh marketing.

- Kênh khảo sát.

- Kênh thu thập dữ liệu khách hàng ngoài phạm vi xác nhận đơn.

IVR chỉ được gọi để xác nhận đơn hàng theo rule.

## 3.3. IVR không thay thế xác nhận đơn của Core

IVR result chỉ là tín hiệu đầu vào.

Core Order State Machine mới là lớp quyết định cuối.

IVR không tự quyết định đơn được tiếp tục.

IVR không tự hủy đơn.

IVR không tự chuyển đơn sang kho.

IVR không tự cho giao vận.

IVR không tự xác nhận doanh thu.

IVR không tự cập nhật order state.

IVR không tự phát hành tin nhắn hủy đơn.

## 4. DEPLOYMENT MODEL

## 4.1. Mô hình triển khai chính thức

Mô hình triển khai chính thức của PACK-09:

## IVR_DEPLOYMENT_MODEL = INTERNAL_SIM_GATEWAY_SERVER

Hệ thống sử dụng:

- SIM Gateway nội bộ.

- SIM vật lý.

- Server nội bộ điều phối cuộc gọi.

- Call scheduler nội bộ.

- DTMF capture nội bộ.

- Call result normalizer nội bộ.

- Audit/evidence nội bộ.

- Core Order State Machine xử lý kết quả cuối.

- Notification owner xử lý tin nhắn sau Core cancellation, nếu đúng điều kiện.

## 4.2. Các mô hình không dùng làm mặc định

Các mô hình sau không được dùng làm mặc định:

Điều này không có nghĩa là vĩnh viễn cấm các mô hình trên.

Điều này có nghĩa là nếu sau này muốn chuyển sang Voice Brandname, SIP Trunk, Cloud IVR hoặc nhà mạng quản lý IVR, phải có owner decision riêng, tài liệu riêng, security review riêng, cost review riêng, smoke riêng và release decision riêng.

Không được để dev tự chọn cloud IVR hoặc provider ngoài làm mặc định.

## 4.3. Nguyên tắc SIM channel

Nguyên tắc bắt buộc:

## 1 SIM = 1 ACTIVE OUTBOUND CALL

Một SIM chỉ được xử lý một cuộc gọi outbound đang active tại một thời điểm.

Không được assign nhiều call job vào cùng một SIM đang bận.

Không được tính năng lực IVR theo số lượng call job trong queue mà phải tính theo số SIM active khả dụng.

Cấu hình SIM | Số cuộc gọi đồng thời tối đa

12 SIM | 12 cuộc gọi

24 SIM | 24 cuộc gọi

32 SIM | 32 cuộc gọi

Nếu scheduler giao trùng SIM channel, PACK-09 Fail Gate.

## 5. IVR CORE PRINCIPLES

## 5.1. IVR là lớp xác nhận, không phải lớp xử lý đơn

IVR chỉ trả về kết quả xác nhận.

Các kết quả IVR không được tự động đổi trạng thái đơn nếu chưa đi qua Core Order State Machine.

Nguyên tắc:

## IVR_RESULT_IS_INPUT_SIGNAL_ONLY

## ORDER_STATE_MACHINE_IS_FINAL_DECISION_LAYER

IVR có thể báo:

- Khách xác nhận đơn.

- Khách bấm hủy / không đặt.

- Khách không nghe.

- Khách không bấm phím.

- Sai phím.

- Lỗi DTMF.

- Lỗi SIM Gateway.

- Lỗi server.

- Hết confirmation window.

- Cần Admin Review.

- Capacity incident.

- Notification waiting sau khi Core đã hủy đơn.

Nhưng Core Order State Machine mới quyết định:

- Tiếp tục xử lý đơn.

- Hủy đơn.

- Giữ đơn chờ review.

- Chuyển Admin Review.

- Chặn đơn do lỗi kỹ thuật.

- Không cập nhật nếu input không hợp lệ.

- Có phát hành trigger hợp lệ cho Notification owner hay không.

## 5.2. IVR không được tự tạo call ngoài rule

IVR chỉ được tạo call job khi có rule hợp lệ.

Không được gọi khách nếu:

- Chưa có order_code.

- Đơn chưa đến trạng thái cần IVR.

- Đơn đã có kết quả cuối.

- Đơn đã hủy.

- Đơn đã hết confirmation window.

- Số điện thoại không hợp lệ theo rule.

- Khách nằm trong nhóm không được gọi theo policy nếu có.

- IVR Gate chưa release.

- Production SIM Gateway chưa được duyệt.

- Downstream chưa được phép phụ thuộc IVR.

## 5.3. Một đơn không được gọi vô hạn

IVR phải có giới hạn attempt theo từng chương trình.

Nguyên tắc nền:

## MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED

> [!IMPORTANT]
> Superseded by D-10: section nay giu noi dung baseline cu. Khi thiet ke/test/implement, dung rule moi: max 2 attempts cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH `T0`, `T0+2:30`, expire `T0+5:00`; 24/7 `T0`, `T0+7:30`, expire `T0+15:00`.

Program-based attempt rule:

- GOLDEN_HOUR: MAX_ATTEMPT = 2 trong IVR_CONFIRMATION_WINDOW = 10_MINUTES.

- TWENTY_FOUR_SEVEN: MAX_ATTEMPT = 3 trong IVR_CONFIRMATION_WINDOW = 15_MINUTES.

Không được gọi lại vô hạn.

Không được gọi dồn liên tục.

Không được gọi ngoài confirmation window.

Không được tạo manual retry ngoài rule nếu chưa có owner-approved exception.

Nếu một cuộc gọi đã có kết quả cuối, không gọi các cuộc tiếp theo.

Kết quả cuối gồm:

- IVR_CONFIRMED.

- IVR_CUSTOMER_CANCELLED.

- INVALID_PHONE_FINAL nếu rule xác nhận.

- IVR_NO_ANSWER_FINAL.

- IVR_CONFIRMATION_WINDOW_EXPIRED.

- TECHNICAL_EXCEPTION_FINAL nếu rule xác nhận.

Important:

- Giờ Vàng giữ 2 cuộc gọi.

- 24/7 dùng 3 cuộc gọi.

- Technical exception không được tính là no answer.

- Capacity incident không được tính là no answer.

- Post-IVR cancellation notice không được gửi cho từng attempt, chỉ được gửi sau khi Core đã hủy đơn chính thức.

## 5.4. IVR phải tôn trọng program window

PACK-09 có rule riêng cho:

- Giờ Vàng.

- 24/7.

Giờ Vàng có confirmation window riêng.

24/7 có confirmation window riêng.

IVR không được dùng một window chung cho mọi chương trình nếu tài liệu đã khóa khác nhau.

IVR không được kéo dài thời gian xác nhận sau khi quote/chương trình hết hiệu lực.

IVR không được gọi sau window rồi vẫn coi kết quả là hợp lệ.

## 6. SOURCE-OF-TRUTH BOUNDARY

## 6.1. Bảng ranh giới source-of-truth

Lớp nghiệp vụ | Source-of-truth owner | PACK-09 được làm gì | PACK-09 không được làm gì

Official Order / order_code | Core Order / Commerce | Nhận đơn đủ điều kiện IVR | Không tự tạo order

Order State Machine | Core Order | Gửi IVR result làm input signal | Không tự đổi order state

Giá trị đơn / tổng thanh toán | Commerce Runtime | Đọc biến được phép cho call script | Không tự tính giá

Chương trình Giờ Vàng / 24/7 | Commerce Runtime | Dùng program window đã khóa | Không tự mở/kéo dài chương trình

Payment | Payment owner | Không can thiệp | Không xác nhận paid

COD / Delivery | Delivery / Fulfillment owner | Không can thiệp | Không xác nhận giao hàng/COD success

AI Advisor | PACK-05 | Không nhận lệnh gọi trực tiếp từ AI | AI không tạo IVR call job

Facebook/Messenger/Live | PACK-06 / PACK-08 | Không nhận raw comment làm IVR result | Gateway/Live không gọi IVR trực tiếp

Ads/ROAS | PACK-07 | Cung cấp evidence nếu owner cho phép | Không tự tính ROAS

Notification | Notification owner | Nhận trigger sau khi Core đã hủy đơn | SIM Gateway/AI/CRM không tự gửi tin nhắn

CRM / Member | CRM/Commerce owner | Không dùng IVR cho CRM | Không nhắc mua lại/chăm sóc

MISA / Accounting | PACK-04 / Finance | Không can thiệp | Không sync MISA

Warehouse / Traceability | Operational Core | Không can thiệp | Không cho xuất kho, không tác động lô/batch/QR

## 6.2. Không tạo parallel order truth

PACK-09 không được tạo nguồn sự thật song song cho:

- Order status.

- Order cancellation.

- Order release.

- Order hold.

- Payment status.

- Delivery status.

- Revenue status.

- Warehouse release.

- MISA accounting.

- CRM eligibility.

- Customer profile.

- Member status.

- Diamond/referral.

- Verified revenue.

- Notification eligibility.

IVR result là tín hiệu xác nhận, không phải trạng thái đơn cuối cùng.

## 6.3. Không downstream dependency trước release

Nguyên tắc:

## DOWNSTREAM_IVR_DEPENDENCY_ALLOWED = KHÔNG

Khi IVR chưa release, các module sau không được phụ thuộc IVR result như dữ liệu production:

- Order.

- AI Advisor.

- Facebook Gateway.

- Messenger.

- Live.

- CRM.

- Admin.

- Warehouse.

- Payment.

- Delivery.

- MISA.

- Ads/ROAS.

- Notification.

- Completion Report.

Nếu module nào dùng IVR result trước khi IVR test/smoke/evidence pass, PACK-09 Fail Gate.

## 7. ORDER STATE MACHINE BOUNDARY

## 7.1. IVR không cập nhật đơn trực tiếp

Nguyên tắc bắt buộc:

## IVR_CAN_TRIGGER_CANCEL_REQUEST = CÓ

## IVR_CAN_DIRECTLY_CANCEL_ORDER = KHÔNG

## SIM_GATEWAY_CAN_CANCEL_ORDER = KHÔNG

## CORE_ORDER_STATE_MACHINE_CANCEL_REQUIRED = CÓ

IVR có thể gửi tín hiệu:

- Khách bấm 1.

- Khách bấm 0.

- Không nghe sau attempts theo program.

- Hết window.

- Invalid phone nếu rule xác nhận.

- Technical exception.

- Capacity exception.

Nhưng IVR không được:

- Hủy đơn trực tiếp.

- Cho đơn tiếp tục trực tiếp.

- Chuyển đơn sang kho.

- Chuyển đơn sang giao vận.

- Đánh dấu đơn verified.

- Đánh dấu doanh thu verified.

- Tự đóng order issue.

- Tự phát hành notification.

Core Order State Machine xử lý kết quả cuối.

## 7.2. IVR result không phải order action

IVR result là input signal.

Order action là quyết định của Core.

IVR result | Ý nghĩa | Core mới được quyết định

IVR_CONFIRMED | Khách bấm 1 xác nhận | Tiếp tục xử lý đơn nếu các điều kiện khác pass

IVR_CUSTOMER_CANCELLED | Khách bấm 0 | Hủy đơn qua state machine

IVR_NO_ANSWER_FINAL | Không nghe sau đủ attempts theo program | Hủy/hold theo rule đã khóa

IVR_CONFIRMATION_WINDOW_EXPIRED | Hết window | Hủy/quote hết hiệu lực theo rule

IVR_TECHNICAL_EXCEPTION | Lỗi kỹ thuật | Admin Review, không tính khách không nghe

IVR_CAPACITY_EXCEPTION | Không đủ năng lực gọi đúng hạn | Owner/Admin Review, không tính khách không nghe

Không được map IVR result trực tiếp thành update order state ngoài Core.

## 7.3. Reason code phải do Core ghi nhận

Các reason code hủy đơn liên quan IVR phải được Core Order State Machine ghi nhận, không phải SIM Gateway tự ghi vào order.

Reason code nền:

- CUSTOMER_CANCELLED_BY_IVR_KEY_0.

- IVR_NO_ANSWER_AFTER_PROGRAM_ATTEMPTS.

- IVR_CONFIRMATION_WINDOW_EXPIRED.

- IVR_INVALID_PHONE_IF_RULE_CONFIRMED.

Program-specific reason mapping:

- GOLDEN_HOUR: IVR_NO_ANSWER_AFTER_2_ATTEMPTS.

- TWENTY_FOUR_SEVEN: IVR_NO_ANSWER_AFTER_3_ATTEMPTS.

Important:

- IVR_NO_ANSWER_AFTER_PROGRAM_ATTEMPTS chỉ được dùng khi các cuộc gọi là attempts hợp lệ và khách không nghe máy.

- Không dùng reason này cho lỗi kỹ thuật.

- Không dùng reason này cho capacity incident.

- Không dùng reason này cho invalid phone.

- Không dùng reason này cho khách bấm phím 0.

- SIM Gateway không tự ghi order cancel reason trực tiếp.

## 8. TECHNICAL EXCEPTION / POST-IVR NOTICE BOUNDARY

## 8.1. Lỗi kỹ thuật không phải khách không nghe

Các lỗi kỹ thuật sau không được xử lý như NO_ANSWER:

- SIM_GATEWAY_ERROR.

- SERVER_ERROR.

- DTMF_CAPTURE_ERROR.

- AUDIO_PLAYBACK_ERROR.

- SIM_CHANNEL_FAILURE.

- INTERNAL_CALLBACK_ERROR.

- SCHEDULER_ERROR.

Các lỗi này phải đi vào:

## IVR_TECHNICAL_EXCEPTION

## ADMIN_REVIEW_REQUIRED

Nếu SIM lỗi nhưng order bị hủy bằng IVR_NO_ANSWER_AFTER_PROGRAM_ATTEMPTS, PACK-09 Fail Gate.

## 8.2. Technical exception không được làm mất quyền khách

Khi lỗi kỹ thuật xảy ra, hệ thống không được kết luận khách không nghe.

Không được hủy đơn chỉ vì:

- SIM lỗi.

- Server lỗi.

- Không phát được audio.

- Không bắt được DTMF.

- Scheduler lỗi.

- Callback lỗi.

- SIM channel lỗi.

Technical exception cần Admin Review hoặc retry theo rule kỹ thuật được owner phê duyệt.

## 8.3. Post-IVR cancellation notice boundary

## POST_IVR_CANCELLATION_NOTICE_ALLOWED = CÓ

## NOTICE_TRIGGER = AFTER_CORE_ORDER_CANCELLED_ONLY

## NOTICE_TYPE = TRANSACTIONAL_ORDER_STATUS_NOTICE

## SIM_GATEWAY_CAN_SEND_NOTICE = KHÔNG

## AI_ADVISOR_CAN_SELF_SEND_NOTICE = KHÔNG

## CRM_MARKETING_CAN_SEND_NOTICE = KHÔNG

## FACEBOOK_GATEWAY_CAN_SELF_SEND_NOTICE = KHÔNG

Rule:

- Tin nhắn giải thích hủy đơn chỉ được gửi sau khi Core Order State Machine đã hủy đơn chính thức.

- SIM Gateway không được gửi tin nhắn.

- AI Advisor không được tự phát tin nhắn.

- CRM không được dùng tin nhắn này như marketing, chăm sóc lại hoặc upsell.

- Facebook Gateway / Messenger chỉ là channel gửi nếu Notification owner phát hành message hợp lệ.

- Notification phải có audit/evidence.

- Tin nhắn không được chứa full address.

- Tin nhắn không được chứa payment detail.

- Tin nhắn không được chứa member tier.

- Tin nhắn không được chứa Diamond/referral info.

- Tin nhắn không được chứa health/sensitive note.

- Tin nhắn không được cam kết giữ giá cũ khi quote/window đã hết.

- Tin nhắn có thể hướng dẫn khách phản hồi để tạo đơn mới theo chương trình hiện hành.

Allowed trigger reasons:

- IVR_NO_ANSWER_AFTER_2_ATTEMPTS for GOLDEN_HOUR.

- IVR_NO_ANSWER_AFTER_3_ATTEMPTS for TWENTY_FOUR_SEVEN.

- IVR_CONFIRMATION_WINDOW_EXPIRED.

Conditional trigger:

- IVR_INVALID_PHONE_IF_RULE_CONFIRMED chỉ gửi qua channel gốc nếu số điện thoại không hợp lệ nhưng vẫn còn channel hợp lệ như Messenger / Website / Facebook.

- Không gửi SMS vào số đã xác định không có thật.

bị chặn trigger:

- IVR_TECHNICAL_EXCEPTION không được gửi tin nhắn hủy đơn.

- IVR_CAPACITY_EXCEPTION không được gửi tin nhắn hủy đơn như lỗi khách.

- ORDER_NOT_CANCELLED_BY_CORE không được gửi tin nhắn hủy đơn.

## 9. PRIVACY / DATA MINIMIZATION PRINCIPLES

## 9.1. IVR chỉ đọc thông tin tối thiểu

Call script chỉ được đọc những biến được phép.

Các biến được phép ở mức nền:

- order_code_short.

- total_amount_display.

- customer_name_short nếu có và policy cho phép.

- program_name nếu cần và policy cho phép.

Không đọc:

- Full customer profile.

- Full address.

- Member tier.

- Diamond referral info.

- Payment detail.

- Order history.

- AI consultation content.

- CRM content.

- Health or sensitive note.

Nguyên tắc: IVR xác nhận đơn, không public dữ liệu riêng qua cuộc gọi tự động quá mức cần thiết.

## 9.2. IVR không xác minh thông tin nhạy cảm

IVR không dùng để hỏi:

- Địa chỉ đầy đủ.

- Bệnh nền.

- Thông tin sức khỏe.

- Lịch sử mua hàng.

- Thành viên hạng nào.

- Diamond/referral relationship.

- Nội dung tư vấn AI.

- Thông tin thanh toán chi tiết.

- Thông tin người nhận nhạy cảm.

Nếu cần xử lý thông tin riêng, phải chuyển sang luồng human/admin theo owner policy.

## 10. CROSS-PACK NO-BYPASS RULE

## 10.1. Với AI Advisor

AI Advisor không được:

- Tạo IVR call job.

- Gọi SIM Gateway.

- Hiển thị IVR result như dữ liệu production khi IVR chưa release.

- Tự nói đơn đã IVR xác nhận nếu Core chưa công nhận.

- Dùng tin nhắn khách để giả lập IVR result.

- Dùng AI confirmation thay IVR nếu order rule yêu cầu IVR.

- Tự hủy đơn do IVR.

- Tự gửi tin nhắn hủy đơn sau IVR.

AI Advisor chỉ được consume trạng thái đã được owner runtime công nhận nếu được phép.

## 10.2. Với Facebook Gateway / Messenger / Live

Facebook Gateway, Messenger và Live không được:

- Gọi IVR trực tiếp.

- Tạo call job.

- Dùng comment làm IVR result.

- Dùng Messenger text làm IVR result.

- Dùng live comment làm IVR confirmation.

- Thay IVR bằng tin nhắn xác nhận không có rule.

- Tự hủy đơn do khách comment “hủy” nếu chưa qua owner state machine.

- Tự gửi tin nhắn hủy đơn nếu Notification owner chưa phát hành message hợp lệ.

Facebook Gateway chỉ là channel.

Messenger chỉ là channel/private conversation.

Live chỉ là channel/tương tác public.

IVR confirmation là pack riêng, và Core mới quyết định order state.

## 10.3. Với CRM / Member Lifecycle

CRM không được dùng IVR cho:

- Tin nhắn chăm sóc.

- Nhắc mua lại.

- Nhắc quyền lợi thành viên.

- Chúc mừng sinh nhật.

- Nhắc Giờ Vàng.

- Mời Diamond.

- Mời chương trình khởi nghiệp.

- Khảo sát marketing.

- Upsell.

- Cross-sell.

CRM không được dùng post-IVR cancellation notice như marketing, chăm sóc lại hoặc upsell.

PACK-09 không phải CRM calling system.

## 10.4. Với Payment

IVR không được:

- Xác nhận thanh toán.

- Đối chiếu chuyển khoản.

- Đọc chi tiết giao dịch.

- Đánh dấu paid.

- Đánh dấu payment failed.

- Xử lý tranh chấp thanh toán.

- Thay Payment Core.

Khách bấm 1 chỉ xác nhận tiếp tục xử lý đơn, không phải xác nhận đã thanh toán.

## 10.5. Với Warehouse / Delivery

IVR không được:

- Cho xuất kho.

- Cho đóng hàng.

- Cho giao vận.

- Xác nhận giao hàng.

- Xác nhận COD success.

- Tác động shipment.

- Tác động warehouse picking.

- Tác động inventory.

Warehouse/Delivery chỉ consume trạng thái hợp lệ từ Core/Fulfillment owner, không consume raw IVR result trực tiếp nếu chưa được owner rule cho phép.

## 10.6. Với MISA / Accounting

IVR không được:

- Tạo chứng từ kế toán.

- Sync MISA.

- Xác nhận doanh thu.

- Xác nhận công nợ.

- Xác nhận thanh toán.

- Tạo accounting event.

- Tác động báo cáo tài chính.

MISA chỉ đi qua PACK-04 / Finance owner.

IVR không có quyền kế toán.

## 10.7. Với PACK-07 Ads / ROAS

PACK-07 có thể dùng IVR result như một evidence/risk signal sau khi PACK-09 release và owner cho phép.

PACK-07 không được dùng IVR result khi:

- IVR chưa release.

- IVR evidence chưa accepted.

- IVR result là simulation.

- IVR result chưa được Core công nhận.

- IVR technical exception chưa xử lý.

- Order chưa đạt verified revenue.

IVR confirmation không phải verified revenue.

Post-IVR cancellation notice không phải revenue event.

## 11. PRODUCTION BLOCK PRINCIPLES

## 11.1. Những gì chưa được phép ở production

Khi PACK-09 chưa release, các hành động sau bị cấm:

## REAL_CUSTOMER_CALL_ALLOWED = KHÔNG

## REAL_ORDER_STATE_UPDATE_ALLOWED = KHÔNG

## PRODUCTION_SIM_GATEWAY_ALLOWED = KHÔNG

## PRODUCTION_ORDER_CANCEL_BY_IVR_ALLOWED = KHÔNG

## POST_IVR_CANCELLATION_NOTICE_ALLOWED_IN_PRODUCTION = KHÔNG

Không được gọi khách thật.

Không được cập nhật đơn thật.

Không được dùng SIM Gateway production.

Không được hủy đơn thật bằng IVR.

Không được cho downstream dùng IVR result thật.

Không được gửi cancellation notice production từ rule IVR nếu chưa release.

## 11.2. Những gì được phép chuẩn bị nội bộ

Đội kỹ thuật được phép chuẩn bị nội bộ:

- Khảo sát SIM Gateway.

- Test server kết nối SIM Gateway.

- Test số nội bộ.

- Test DTMF nội bộ.

- Draft skeleton backend.

- Draft Admin UI.

- Draft test matrix.

- Test scheduler trong sandbox.

- Test call result normalizer bằng dữ liệu giả lập nội bộ.

- Test audit/evidence trong môi trường không production.

- Test notification owner handoff bằng dữ liệu giả lập.

Nhưng mọi chuẩn bị này không được gọi khách thật và không được cập nhật đơn thật.

## 12. FAIL-CLOSED PRINCIPLES

PACK-09 phải fail-closed trong các trường hợp:

- Không có order_code.

- Không có order đủ điều kiện IVR.

- Không có confirmation window.

- Không có program rule hợp lệ.

- Không có SIM channel khả dụng.

- SIM channel đang bận.

- Scheduler không xác định được deadline.

- DTMF mapping không đúng phím 1/phím 0.

- Technical exception xảy ra.

- Core Order State Machine không khả dụng.

- IVR Gate chưa release.

- Evidence capture không hoạt động.

- Downstream cố dùng IVR result khi chưa release.

- Admin cố sửa IVR result giả.

- SIM Gateway cố cập nhật order trực tiếp.

- Notification cố gửi tin nhắn hủy trước khi Core hủy đơn.

- AI/CRM/Gateway tự gửi cancellation notice.

Fail-closed nghĩa là:

- Không gọi production.

- Không cập nhật order.

- Không hủy đơn.

- Không tính khách không nghe nếu là lỗi kỹ thuật.

- Không gửi tin nhắn hủy đơn nếu Core chưa hủy.

- Chuyển Admin Review nếu cần.

- Ghi audit/evidence.

- Chặn downstream dependency nếu chưa release.

## 13. PACK-09 DOCUMENT SET

PACK-09 gồm các file chính:

- IVR-00 — Governance / Source of Truth / Scope Boundary.

- IVR-01 — Business Purpose / Confirmation Use Case.

- IVR-02 — Ownership Boundary / Connected Systems.

- IVR-03 — Eligibility Rule / Customer Trust / Official Contact.

- IVR-04 — Order Core -> IVR Task Contract.

- IVR-05 — Attempt Policy / Scheduler / Queue.

- IVR-06 — Internal SIM Gateway Adapter.

- IVR-07 — Result Normalization / IVR -> Order Core Callback.

- IVR-08 — Admin Monitoring / Evidence / Audit / Privacy.

- IVR-09 — Test Matrix / Smoke / Release Gate.

PACK-09 hiện là pack governance cấp cha.

Các file IVR-00 -> IVR-09 là bộ tài liệu triển khai chi tiết tiếp theo, nhưng vẫn phải tuân thủ ranh giới đã khóa trong PACK-09.

V1.1 inheritance note:

- IVR-00 phải ghi PACK_09_VERSION = V1.1_CLEAN_FINAL.

- IVR-02 phải sửa 24/7 scheduler thành 3 attempts: T0 / T0 + 5 phút / T0 + 10 phút.

## 14. CẤU TRÚC PACK-09

PACK-09 được viết thành 4 phần:

- PHẦN 1/4 — IVR ORDER CONFIRMATION PRINCIPLES / INTERNAL SIM GATEWAY / SOURCE-OF-TRUTH BOUNDARY / NO-BYPASS RULE

Khóa mục tiêu, trạng thái pack, mô hình Internal SIM Gateway, source-of-truth boundary, Core Order State Machine boundary, post-IVR notice boundary, production block và no-bypass rule.

- PHẦN 2/4 — CALL SCRIPT / DTMF / PROGRAM WINDOW / GOLDEN HOUR / 24-7 / DEADLINE-AWARE SCHEDULER

Khóa nội dung cuộc gọi, biến được phép đọc, phím 1/phím 0, rule Giờ Vàng, rule 24/7, attempt, window, rolling queue và deadline-aware scheduler.

- PHẦN 3/4 — CALL RESULT / TECHNICAL EXCEPTION / CAPACITY BASELINE / CROSS-PACK CONTROL / ENGINEER HANDOFF

Khóa result normalization, lỗi kỹ thuật, capacity theo SIM, không batch cuối phiên, cross-pack boundary chi tiết, notification boundary và engineer handoff model.

- PHẦN 4/4 — P0 RULES / SMOKE MATRIX / IVR EVIDENCE / DONE GATE / FAIL GATE / RELEASE CONTROL / FINAL CONCLUSION

Khóa P0 MUST/MUST NOT/FAIL IF, smoke matrix, evidence, done/fail gate, release readiness và kết luận cuối PACK-09.

## 15. DONE CONDITION CỦA PHẦN 1/4

## PHẦN 1/4 được xem là hoàn thành về mặt tài liệu khi đã khóa được:

- Tên tài liệu PACK-09.

- Vai trò PACK-09.

- Trạng thái DOCUMENTATION_ACTIVE.

- Trạng thái NOT_STARTED / NOT_READY.

- Production not allowed.

- CALL_PURPOSE = ORDER_CONFIRMATION_ONLY.

- IVR không phải sales/CRM/marketing.

- Mô hình Internal SIM Gateway Server.

- Không dùng cloud/SIP/voice brandname làm mặc định.

- Nguyên tắc 1 SIM = 1 active outbound call.

- IVR là input signal only.

- Core Order State Machine là final decision layer.

- IVR không cập nhật order trực tiếp.

- Lỗi kỹ thuật không phải khách không nghe.

- Data minimization/privacy principle.

- Cross-pack no-bypass với AI, Facebook, CRM, Payment, Warehouse, MISA, PACK-07.

- Post-IVR cancellation notice boundary.

- Production block principles.

- Fail-closed principles.

- Document set IVR-00 -> IVR-09.

- Cấu trúc 4 phần của PACK-09.

## KẾT LUẬN PHẦN 1/4

PACK-09 là pack chính thức cho IVR_ORDER_CONFIRMATION của hệ thống Ginsengfood.

PACK-09 đã được mở tài liệu ở trạng thái DOCUMENTATION_ACTIVE, nhưng implementation chưa bắt đầu, production chưa sẵn sàng và IVR Gate vẫn bị chặn.

PACK-09 dùng mô hình triển khai chính thức là Internal SIM Gateway Server.

PACK-09 phục vụ một mục đích duy nhất:

## ORDER_CONFIRMATION_ONLY

IVR không phải kênh bán hàng.

IVR không phải CRM.

IVR không phải chăm sóc khách hàng.

IVR không phải marketing.

IVR không phải Payment.

IVR không phải Delivery.

IVR không phải Warehouse.

IVR không phải MISA.

IVR không phải AI Advisor.

IVR không phải Facebook Gateway.

IVR không phải Ads/ROAS.

IVR chỉ tạo tín hiệu xác nhận đơn hàng.

Core Order State Machine mới là lớp quyết định cuối.

Notification owner chỉ được gửi tin nhắn giải thích sau khi Core Order State Machine đã hủy đơn chính thức.

Nguyên tắc khóa của PHẦN 1/4 là:

Internal SIM Gateway là mô hình chính thức.

1 SIM = 1 active outbound call.

IVR chỉ xác nhận đơn hàng.

IVR result chỉ là input signal.

Core Order State Machine là final decision layer.

SIM Gateway không cập nhật order trực tiếp.

Lỗi kỹ thuật không phải khách không nghe.

Capacity incident không phải khách không nghe.

Invalid phone không được nhầm với no answer.

AI / Facebook / CRM / Admin / Payment / Warehouse / MISA không được bypass IVR boundary.

Notification owner mới được gửi transactional notice sau Core cancellation.

Chưa release thì không gọi khách thật.

Chưa release thì không cập nhật order thật.

Chưa release thì downstream không được phụ thuộc IVR result.

Không có evidence thì không PASS.

## PHẦN 2/4 — CALL SCRIPT / DTMF / PROGRAM WINDOW / GOLDEN HOUR / 24-7 / DEADLINE-AWARE SCHEDULER

## 16. MỤC TIÊU CỦA PHẦN 2/4

## PHẦN 2/4 khóa lớp vận hành cuộc gọi IVR, bao gồm nội dung cuộc gọi, biến được phép đọc, phím xác nhận/hủy, rule gọi theo chương trình Giờ Vàng và 24/7, confirmation window, attempt interval, call scheduling và deadline-aware rolling queue.

Mục tiêu của phần này là xác định rõ:

- IVR được phép nói gì trong cuộc gọi.

- IVR không được phép đọc thông tin gì.

- Khách bấm phím nào để xác nhận đơn.

- Khách bấm phím nào để hủy đơn.

- Không bấm phím xử lý ra sao.

- Sai phím xử lý ra sao.

- Lỗi DTMF xử lý ra sao.

- Giờ Vàng được gọi trong bao lâu.

- 24/7 được gọi trong bao lâu.

- Mỗi đơn được gọi tối đa mấy lần theo từng program.

- Khi nào không được gọi tiếp.

- Khi nào IVR result được xem là kết quả cuối.

- Khi nào phải chuyển technical exception.

- Scheduler phải phân bổ cuộc gọi như thế nào để không dồn cuối phiên.

- Vì sao SIM Gateway không được cập nhật order trực tiếp.

- Khi nào được gửi post-IVR cancellation notice.

## PHẦN 2/4 không thiết kế API, database, UI hoặc code.

## PHẦN 2/4 chỉ khóa logic governance/domain để đội kỹ thuật triển khai đúng.

## 17. CALL SCRIPT GOVERNANCE

## 17.1. Nguyên tắc call script

Call script của PACK-09 phải ngắn, rõ, đúng mục đích và chỉ phục vụ xác nhận đơn hàng.

Call script không được biến thành:

- Nội dung bán hàng.

- Nội dung giới thiệu sản phẩm.

- Nội dung upsell.

- Nội dung chăm sóc khách hàng.

- Nội dung nhắc mua lại.

- Nội dung mời thành viên.

- Nội dung mời Diamond.

- Nội dung quảng cáo chương trình ngoài phạm vi đơn.

- Nội dung thu thập thêm dữ liệu khách.

- Nội dung tư vấn sản phẩm.

IVR chỉ gọi để xác nhận khách có tiếp tục xử lý đơn hàng hay không.

## 17.2. Nội dung cuộc gọi chuẩn

Call script chuẩn:

Ginsengfood kính chào Mình.

Ginsengfood cảm ơn Mình đã chọn Cháo Sâm Savigin.

Đơn hàng {{order_code_short}} của Mình có tổng thanh toán {{total_amount_display}}.

Để xác nhận tiếp tục xử lý đơn hàng, Mình vui lòng bấm phím 1.

Nếu Mình không đặt hoặc muốn hủy đơn này, Mình vui lòng bấm phím 0.

Ginsengfood trân trọng cảm ơn ạ.

Script này là script nền.

Không được tự ý thêm câu bán hàng.

Không được tự ý thêm câu mời mua thêm.

Không được tự ý thêm câu nhắc chương trình nếu không có runtime và owner rule.

Không được tự ý thêm thông tin cá nhân.

Không được tự ý đọc địa chỉ đầy đủ.

Không được tự ý đọc nội dung tư vấn AI.

Không được tự ý đọc ghi chú sức khỏe.

Không được tự ý đọc quyền lợi thành viên.

Không được tự ý đọc Diamond/referral.

Không được tự ý đọc thông tin thanh toán chi tiết.

## 17.3. Lý do call script phải ngắn

Call script phải ngắn vì:

- Khách cần hiểu ngay đây là cuộc gọi xác nhận đơn.

- Giảm thời lượng cuộc gọi để tăng năng lực SIM Gateway.

- Giảm rủi ro khách cúp máy giữa chừng.

- Giảm rủi ro hiểu sai nội dung.

- Giảm rủi ro lộ dữ liệu cá nhân.

- Giữ IVR đúng vai trò xác nhận đơn, không thành kênh marketing.

- Giúp DTMF bấm phím 1/0 rõ ràng.

- Giúp scheduler tính tải chính xác.

- Giúp evidence/audit rõ ràng.

- Giảm rủi ro khách hiểu IVR là cuộc gọi tư vấn/bán hàng.

PACK-09 khóa thời lượng gọi trung bình nền là 35 giây, và dùng chu kỳ bảo thủ 50 giây/cuộc/SIM để tính capacity ở PHẦN 3/4.

## 18. CALL SCRIPT VARIABLE GOVERNANCE

## 18.1. Biến được phép dùng

Biến | Ý nghĩa | Trạng thái

order_code_short | Mã đơn rút gọn | ALLOWED

total_amount_display | Tổng thanh toán | ALLOWED

customer_name_short | Tên gọi ngắn nếu có | OPTIONAL

program_name | Giờ Vàng / 24/7 nếu cần | OPTIONAL

Chỉ các biến trên được phép đưa vào call script nền.

Các biến optional chỉ được dùng khi policy cho phép và runtime cung cấp dữ liệu hợp lệ.

## 18.2. Biến bắt buộc

Biến bắt buộc cho call script chuẩn:

- order_code_short.

- total_amount_display.

Nếu thiếu order_code_short, không được gọi IVR.

Nếu thiếu total_amount_display, không được gọi IVR.

Nếu tổng tiền chưa được Commerce Runtime xác nhận, không được đọc tổng thanh toán trong cuộc gọi.

Nếu order chưa có official order/order_code, không được tạo IVR call job.

Nếu order đã hết hiệu lực, không được tạo IVR call job.

Nếu order đã bị Core hủy, không được tạo IVR call job.

## 18.3. Biến optional

Các biến optional:

- customer_name_short.

- program_name.

Chỉ được dùng khi:

- Có runtime hợp lệ.

- Có policy cho phép.

- Không vi phạm privacy.

- Không làm dài script quá mức cần thiết.

- Không khiến khách hiểu IVR là cuộc gọi marketing.

- Không khiến khách hiểu chương trình cũ vẫn còn hiệu lực nếu đã hết window.

Ví dụ: program_name chỉ nên dùng để nhắc đơn thuộc Giờ Vàng hoặc 24/7 nếu cần phân biệt confirmation window, không dùng để quảng cáo chương trình.

## 18.4. Thông tin không được đọc trong cuộc gọi

IVR không được đọc:

- Full customer profile.

- Full address.

- Member tier.

- Diamond referral info.

- Payment detail.

- Order history.

- AI consultation content.

- CRM content.

- Health or sensitive note.

- Ghi chú nội bộ.

- Nội dung khiếu nại.

- Số điểm tích lũy.

- Quyền lợi cá nhân.

- Lịch sử mua lại.

- Thông tin người nhận đầy đủ.

- Mã vận đơn.

- Trạng thái thanh toán chi tiết.

- Thông tin MISA/kế toán.

- Thông tin lô hàng/batch/traceability.

- Thông tin nội bộ về stock/kho.

Nguyên tắc:

IVR chỉ đọc đủ thông tin để khách nhận diện đơn và xác nhận tiếp tục xử lý.

## 18.5. Không đọc full address

Full address không được đọc trong cuộc gọi vì:

- Có thể người nghe không phải khách đặt hàng.

- Dễ lộ dữ liệu cá nhân.

- Làm dài cuộc gọi.

- Không cần thiết cho mục đích bấm 1/0.

- Có thể gây rủi ro privacy nếu điện thoại bật loa ngoài.

Nếu cần xác minh địa chỉ, phải chuyển qua luồng human/admin hoặc order confirmation policy khác, không đưa vào script IVR nền.

## 18.6. Không đọc member / Diamond / referral

IVR không được đọc:

- Khách là Silver/Gold/Platinum/Diamond.

- Quyền lợi thành viên.

- Diamond referral info.

- Commission.

- Referral owner.

- Link giới thiệu.

- Trạng thái nâng hạng.

- Số tiền còn thiếu để lên hạng.

Lý do:

- Đây là dữ liệu cá nhân/kinh doanh.

- IVR không phải CRM.

- IVR không phải kênh member care.

- IVR không phải kênh Diamond/referral.

- IVR chỉ xác nhận đơn.

## 18.7. Không đọc payment detail

IVR không được đọc:

- Khách đã chuyển khoản hay chưa.

- Số tài khoản.

- Nội dung chuyển khoản.

- Thời điểm chuyển khoản.

- Mã giao dịch.

- Trạng thái đối chiếu ngân hàng.

- Thông tin COD chi tiết vượt script.

- Lỗi thanh toán.

Khách bấm 1 không đồng nghĩa đã thanh toán.

Khách bấm 1 chỉ đồng nghĩa khách xác nhận tiếp tục xử lý đơn.

Payment vẫn do Payment owner/Core xử lý.

## 19. DTMF KEY RULE

## 19.1. Phím xác nhận/hủy đơn

Phím | Ý nghĩa | Hành động

1 | Khách xác nhận đơn | Core tiếp tục xử lý đơn theo state machine

0 | Khách không đặt / muốn hủy | Core hủy đơn qua state machine

Không bấm | Không có xác nhận hợp lệ | Xử lý theo attempt/window

Sai phím | Không có input hợp lệ | Xử lý theo rule IVR

Lỗi DTMF | Lỗi kỹ thuật | Không tính là khách không nghe

Mapping khóa:

## KEY_1 = CONFIRM_ORDER

## KEY_0 = CANCEL_ORDER_BY_CUSTOMER

Phím 1 và phím 0 là hai phím duy nhất trong scope hiện tại.

Không tự thêm phím 9.

Không tự thêm phím 2/3/4.

Không tự thêm menu nhiều tầng.

Không biến IVR thành tổng đài CSKH.

## 19.2. Phím 1 — CONFIRM_ORDER

Khi khách nghe cuộc gọi và bấm phím 1:

## IVR_RESULT = IVR_CONFIRMED

Ý nghĩa:

- Khách xác nhận có đơn.

- Khách đồng ý tiếp tục xử lý đơn.

- IVR result được gửi về Core làm input signal.

- Core Order State Machine quyết định bước tiếp theo.

- Không gọi attempt tiếp theo.

- Không gọi lại khách cho cùng confirmation window.

Phím 1 không có nghĩa:

- Khách đã thanh toán.

- Đơn đã giao.

- Đơn đã verified revenue.

- Đơn đã được xuất kho.

- Đơn đã được MISA ghi nhận.

- Đơn đã hoàn tất.

- Đơn đủ điều kiện commission.

- Đơn đủ điều kiện ROAS verified.

## 19.3. Phím 0 — CANCEL_ORDER_BY_CUSTOMER

Khi khách nghe cuộc gọi và bấm phím 0:

## IVR_RESULT = IVR_CUSTOMER_CANCELLED

Ý nghĩa:

- Khách xác nhận không đặt hoặc muốn hủy đơn.

- IVR gửi cancel request signal về Core.

- Core Order State Machine xử lý hủy đơn.

- SIM Gateway không tự hủy đơn.

- Không gọi attempt tiếp theo.

- Không tiếp tục xử lý đơn nếu Core hủy thành công.

Reason code nền:

## ORDER_CANCEL_REASON = CUSTOMER_CANCELLED_BY_IVR_KEY_0

Reason code phải do Core ghi nhận.

SIM Gateway không ghi thẳng reason vào order.

## 19.4. Không bấm phím

Nếu khách nghe nhưng không bấm phím, kết quả chưa phải xác nhận hợp lệ.

Không bấm phím có thể do:

- Khách không hiểu.

- Khách nghe nhưng chưa quyết định.

- Khách nghe máy nhưng không thao tác.

- DTMF không ghi nhận.

- Máy khách không gửi tone.

- Cuộc gọi bị cúp giữa chừng.

- Lỗi kỹ thuật.

Cần phân biệt:

- NO_INPUT do khách không bấm.

- DTMF_CAPTURE_ERROR do lỗi kỹ thuật.

- CALL_DROPPED do cuộc gọi rớt.

- AUDIO_PLAYBACK_ERROR do không phát được nội dung.

Không được gom tất cả thành khách không nghe.

## 19.5. Sai phím

Nếu khách bấm sai phím, ví dụ phím 2/3/4/5/6/7/8/9/*/#, trạng thái là input không hợp lệ.

Sai phím không được hiểu là xác nhận.

Sai phím không được hiểu là hủy.

Sai phím không tự tạo order action.

Tùy rule, sai phím có thể:

- Được xem là invalid input trong attempt hiện tại.

- Chờ khách bấm lại nếu còn trong call script.

- Kết thúc attempt nếu không có input hợp lệ.

- Chuyển retry theo window.

- Chuyển Admin Review nếu lặp nhiều hoặc nghi ngờ lỗi DTMF.

Không được tự thêm phím 9 hỗ trợ người thật khi rule đã khóa NOT_ENABLED.

## 19.6. Lỗi DTMF

Lỗi DTMF là lỗi kỹ thuật.

Các tình huống:

- DTMF_CAPTURE_ERROR.

- Tone không nhận diện được.

- DTMF handler lỗi.

- Callback DTMF lỗi.

- SIM Gateway không trả input.

- Server không ghi được input.

- Call result normalizer lỗi.

Lỗi DTMF không phải khách không nghe.

Lỗi DTMF không được hủy đơn theo reason no-answer.

Lỗi DTMF phải đi vào:

## IVR_TECHNICAL_EXCEPTION

## ADMIN_REVIEW_REQUIRED

## 20. CALL RESULT CLASSIFICATION

## 20.1. Nhóm kết quả hợp lệ cuối

Các kết quả cuối hợp lệ:

- IVR_CONFIRMED.

- IVR_CUSTOMER_CANCELLED.

- IVR_NO_ANSWER_FINAL.

- IVR_CONFIRMATION_WINDOW_EXPIRED.

- INVALID_PHONE_FINAL nếu rule xác nhận.

- TECHNICAL_EXCEPTION_FINAL nếu rule xác nhận.

Khi đã có kết quả cuối, không gọi tiếp attempt còn lại.

## 20.2. Nhóm kết quả tạm thời

Các kết quả tạm thời:

- ATTEMPT_1_NO_ANSWER.

- ATTEMPT_1_NO_INPUT.

- ATTEMPT_1_INVALID_INPUT.

- ATTEMPT_1_BUSY.

- ATTEMPT_1_CALL_DROPPED nếu không phải lỗi kỹ thuật final.

- ATTEMPT_2_NO_ANSWER.

- ATTEMPT_2_NO_INPUT.

- ATTEMPT_2_INVALID_INPUT.

- ATTEMPT_2_BUSY.

- ATTEMPT_3_NO_ANSWER nếu program là TWENTY_FOUR_SEVEN.

- ATTEMPT_3_NO_INPUT nếu program là TWENTY_FOUR_SEVEN.

- ATTEMPT_3_INVALID_INPUT nếu program là TWENTY_FOUR_SEVEN.

- RETRY_waiting.

- CALL_waiting.

- CALL_SCHEDULED.

Kết quả tạm thời không được làm thay đổi order state cuối.

Kết quả tạm thời chỉ dùng để scheduler quyết định attempt tiếp theo trong confirmation window.

## 20.3. Nhóm lỗi kỹ thuật

Các lỗi kỹ thuật:

- SIM_GATEWAY_ERROR.

- SERVER_ERROR.

- DTMF_CAPTURE_ERROR.

- AUDIO_PLAYBACK_ERROR.

- SIM_CHANNEL_FAILURE.

- INTERNAL_CALLBACK_ERROR.

- SCHEDULER_ERROR.

- CALL_RESULT_NORMALIZER_ERROR.

- AUDIT_WRITE_ERROR.

- EVIDENCE_WRITE_ERROR.

Các lỗi kỹ thuật phải đi vào technical exception.

Không được tính là khách không nghe.

Không được làm hủy đơn tự động như no answer.

Không được kích hoạt post-IVR cancellation notice.

## 21. PROGRAM-BASED CALL RULE

## 21.1. Rule chung

Rule chung cho PACK-09:

## MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED

## ATTEMPT_INTERVAL = PROGRAM_BASED

## SIM_GATEWAY_DIRECT_ORDER_UPDATE = KHÔNG

## ORDER_STATE_CHANGE_MUST_PASS_CORE_STATE_MACHINE = CÓ

Program-based rule:

- GOLDEN_HOUR:

o | IVR_CONFIRMATION_WINDOW = 10_MINUTES

o | MAX_ATTEMPT = 2

o | ATTEMPT_1 = T0

o | ATTEMPT_2 = T0 + 5_MINUTES

o | WINDOW_EXPIRES = T0 + 10_MINUTES

- TWENTY_FOUR_SEVEN:

o | IVR_CONFIRMATION_WINDOW = 15_MINUTES

o | MAX_ATTEMPT = 3

o | ATTEMPT_1 = T0

o | ATTEMPT_2 = T0 + 5_MINUTES

o | ATTEMPT_3 = T0 + 10_MINUTES

o | WINDOW_EXPIRES = T0 + 15_MINUTES

Ý nghĩa:

- Số lần gọi tối đa phụ thuộc chương trình.

- Giờ Vàng giữ 2 cuộc.

- 24/7 dùng 3 cuộc.

- SIM Gateway không cập nhật order trực tiếp.

- Mọi thay đổi trạng thái đơn phải qua Core Order State Machine.

## 21.2. Không gọi cuộc tiếp theo nếu cuộc trước đã có kết quả cuối

Nếu một attempt đã có kết quả cuối, không gọi các attempts còn lại.

Kết quả cuối gồm:

- IVR_CONFIRMED.

- IVR_CUSTOMER_CANCELLED.

- INVALID_PHONE_FINAL.

- IVR_NO_ANSWER_FINAL.

- IVR_CONFIRMATION_WINDOW_EXPIRED.

- TECHNICAL_EXCEPTION_FINAL nếu rule xác định không retry tự động.

Ví dụ:

- Khách bấm 1 ở cuộc 1 -> không gọi các cuộc tiếp theo.

- Khách bấm 0 ở cuộc 1 -> không gọi các cuộc tiếp theo.

- Số điện thoại invalid final -> không gọi các cuộc tiếp theo.

- Technical exception final -> không gọi các cuộc tiếp theo, chuyển Admin Review.

- Với 24/7, nếu khách xác nhận ở cuộc 2 thì không gọi cuộc 3.

## 21.3. Attempt interval không được tùy tiện

Attempt interval phải theo từng program.

## GOLDEN_HOUR:

- Attempt interval = 5 phút.

- Attempt 2 = T0 + 5 phút.

- Window expires = T0 + 5 phút.

## TWENTY_FOUR_SEVEN:

- Attempt interval = 5 phút.

- Attempt 2 = T0 + 5 phút.

- Attempt 3 = T0 + 10 phút.

- Window expires = T0 + 15 phút.

Không được gọi lại ngay sau 5 giây.

Không được gọi liên tục gây phiền khách.

Không được gọi sau khi window hết hạn.

Không được gọi dồn cuối window nếu attempt đã quá hạn.

Không được để Admin bấm gọi lại ngoài rule nếu chưa có owner-approved exception.

## 21.4. Confirmation window là giới hạn cứng

Confirmation window là thời gian tối đa để nhận xác nhận hợp lệ.

Khi window hết:

- Không tạo attempt mới.

- Không nhận kết quả muộn như xác nhận hợp lệ nếu policy không cho phép.

- Không tiếp tục giữ quote/chương trình quá hạn.

- Core xử lý hủy đơn / quote hết hiệu lực theo rule.

- Ghi IVR_CONFIRMATION_WINDOW_EXPIRED.

Không được kéo dài window bằng manual thao tác nếu không có owner-approved override.

## 22. GOLDEN HOUR RULE

## 22.1. Cấu hình Giờ Vàng

Rule Giờ Vàng:

## PROGRAM = GOLDEN_HOUR

## IVR_CONFIRMATION_WINDOW = 10_MINUTES

## MAX_ATTEMPT = 2

## ATTEMPT_INTERVAL = 5_MINUTES

Lịch gọi:

T0 = thời điểm đơn cần xác nhận IVR

## ATTEMPT_1 = T0

ATTEMPT_2 = T0 + 5 phút

WINDOW_EXPIRES = T0 + 10 phút

Giờ Vàng có cửa sổ xác nhận ngắn vì quote/giá/chương trình có tính thời điểm.

IVR không được gọi sau 10 phút rồi vẫn coi đơn là xác nhận trong phiên.

## 22.2. Mapping kết quả Giờ Vàng

Tình huống | IVR Result | Core Action

Cuộc 1 nghe + bấm 1 | IVR_CONFIRMED | Tiếp tục xử lý đơn qua Core Order State Machine

Cuộc 1 nghe + bấm 0 | IVR_CUSTOMER_CANCELLED | Core hủy đơn

Cuộc 1 không nghe | ATTEMPT_1_NO_ANSWER | Gọi cuộc 2 sau 5 phút

Cuộc 1 lỗi kỹ thuật | IVR_TECHNICAL_EXCEPTION | Không tính là khách không nghe, chuyển Admin Review

Cuộc 2 nghe + bấm 1 | IVR_CONFIRMED | Tiếp tục xử lý đơn qua Core Order State Machine

Cuộc 2 nghe + bấm 0 | IVR_CUSTOMER_CANCELLED | Core hủy đơn

Cuộc 2 không nghe | IVR_NO_ANSWER_FINAL | Core hủy đơn theo rule

Cuộc 2 lỗi kỹ thuật | IVR_TECHNICAL_EXCEPTION | Không tính là khách không nghe, chuyển Admin Review

Hết 10 phút chưa xác nhận hợp lệ | IVR_CONFIRMATION_WINDOW_EXPIRED | Core hủy đơn / quote hết hiệu lực

## 22.3. Giờ Vàng không được gọi dồn cuối phiên

Trong Giờ Vàng, scheduler không được dồn toàn bộ cuộc gọi cuối phiên.

Không được để hàng trăm call job nằm chờ rồi đến cuối 10 phút mới gọi.

Nếu call job không thể thực hiện trong window, phải đánh dấu theo rule capacity/deadline.

Không được giả lập đã gọi nếu SIM không đủ năng lực.

Không được kéo dài xác nhận ngoài 5 phút vì nghẽn SIM.

## 22.4. Giờ Vàng không được tự kéo dài quote

IVR không có quyền kéo dài quote.

Nếu window hết:

- IVR_CONFIRMATION_WINDOW_EXPIRED.

- Core xử lý theo rule.

- Quote hết hiệu lực nếu policy quy định.

- Không được nói khách vẫn giữ giá.

- Không được tự tạo quote mới.

Quote/giá/chương trình do Commerce Runtime quyết định.

## 22.5. Golden Hour post-IVR cancellation notice

## POST_IVR_CANCELLATION_NOTICE_ALLOWED = CÓ

## NOTICE_TRIGGER = AFTER_CORE_ORDER_CANCELLED_ONLY

## NOTICE_TYPE = TRANSACTIONAL_ORDER_STATUS_NOTICE

## SIM_GATEWAY_CAN_SEND_NOTICE = KHÔNG

## AI_ADVISOR_CAN_SELF_SEND_NOTICE = KHÔNG

## CRM_MARKETING_CAN_SEND_NOTICE = KHÔNG

## NOTIFICATION_OWNER_REQUIRED = CÓ

Rule:

- Nếu khách không nghe sau đủ 2 attempts hợp lệ trong Golden Hour, IVR tạo IVR_NO_ANSWER_FINAL.

- IVR_NO_ANSWER_FINAL chỉ là input signal.

- Core Order State Machine xử lý hủy đơn / quote hết hiệu lực theo rule.

- Chỉ sau khi Core đã hủy đơn chính thức, Notification owner mới được gửi tin nhắn giao dịch giải thích.

- Không gửi tin nhắn hủy sau attempt 1.

- Không gửi tin nhắn hủy trước khi Core hủy đơn.

- Không hứa giữ lại giá Giờ Vàng nếu window đã hết.

- Không dùng tin nhắn này để upsell, CRM hoặc marketing.

Golden Hour cancellation notice template:

Ginsengfood kính chào Mình.

Đơn hàng {{order_code_short}} của Mình chưa được xác nhận qua cuộc gọi tự động trong thời gian giữ đơn Giờ Vàng, nên đơn hàng đã hết hiệu lực và được hủy theo quy trình.

Nếu Mình vẫn có nhu cầu, Mình vui lòng phản hồi lại tin nhắn này để được hỗ trợ tạo đơn mới theo chương trình hiện hành ạ.

Ginsengfood trân trọng cảm ơn Mình.

## 23. 24/7 RULE

## 23.1. Cấu hình 24/7

Rule 24/7:

## PROGRAM = TWENTY_FOUR_SEVEN

## IVR_CONFIRMATION_WINDOW = 15_MINUTES

## MAX_ATTEMPT = 3

## ATTEMPT_INTERVAL = 5_MINUTES

Lịch gọi:

T0 = thời điểm đơn cần xác nhận IVR

## ATTEMPT_1 = T0

ATTEMPT_2 = T0 + 5 phút

ATTEMPT_3 = T0 + 10 phút

WINDOW_EXPIRES = T0 + 15 phút

24/7 có confirmation window 15 phút và được phép gọi tối đa 3 attempts.

Không được gọi sau 15 phút rồi vẫn coi xác nhận là hợp lệ nếu rule không cho phép.

## 23.2. Mapping kết quả 24/7

Tình huống | IVR Result | Core Action

Cuộc 1 nghe + bấm 1 | IVR_CONFIRMED | Tiếp tục xử lý đơn qua Core Order State Machine

Cuộc 1 nghe + bấm 0 | IVR_CUSTOMER_CANCELLED | Core hủy đơn

Cuộc 1 không nghe | ATTEMPT_1_NO_ANSWER | Gọi cuộc 2 sau 5 phút

Cuộc 1 lỗi kỹ thuật | IVR_TECHNICAL_EXCEPTION | Không tính là khách không nghe, chuyển Admin Review

Cuộc 2 nghe + bấm 1 | IVR_CONFIRMED | Tiếp tục xử lý đơn qua Core Order State Machine

Cuộc 2 nghe + bấm 0 | IVR_CUSTOMER_CANCELLED | Core hủy đơn

Cuộc 2 không nghe | ATTEMPT_2_NO_ANSWER | Gọi cuộc 3 sau 5 phút

Cuộc 2 lỗi kỹ thuật | IVR_TECHNICAL_EXCEPTION | Không tính là khách không nghe, chuyển Admin Review

Cuộc 3 nghe + bấm 1 | IVR_CONFIRMED | Tiếp tục xử lý đơn qua Core Order State Machine

Cuộc 3 nghe + bấm 0 | IVR_CUSTOMER_CANCELLED | Core hủy đơn

Cuộc 3 không nghe | IVR_NO_ANSWER_FINAL | Core hủy đơn theo rule

Cuộc 3 lỗi kỹ thuật | IVR_TECHNICAL_EXCEPTION | Không tính là khách không nghe, chuyển Admin Review

Hết 15 phút chưa xác nhận hợp lệ | IVR_CONFIRMATION_WINDOW_EXPIRED | Core hủy đơn / quote hết hiệu lực

## 23.3. 24/7 không được gọi ngoài window

Không được tạo attempt sau 15 phút.

Không được tạo attempt 4.

Không được reset window bằng thao tác Admin nếu không có owner-approved rule.

Không được gọi lại nhiều lần vì khách chưa nghe.

Không được biến IVR thành kênh follow-up.

Nếu hết window, Core xử lý theo state machine.

## 23.4. 24/7 post-IVR cancellation notice

## POST_IVR_CANCELLATION_NOTICE_ALLOWED = CÓ

## NOTICE_TRIGGER = AFTER_CORE_ORDER_CANCELLED_ONLY

## NOTICE_TYPE = TRANSACTIONAL_ORDER_STATUS_NOTICE

## SIM_GATEWAY_CAN_SEND_NOTICE = KHÔNG

## AI_ADVISOR_CAN_SELF_SEND_NOTICE = KHÔNG

## CRM_MARKETING_CAN_SEND_NOTICE = KHÔNG

## NOTIFICATION_OWNER_REQUIRED = CÓ

Rule:

- Nếu khách không nghe sau đủ 3 attempts hợp lệ trong 24/7, IVR tạo IVR_NO_ANSWER_FINAL.

- IVR_NO_ANSWER_FINAL chỉ là input signal.

- Core Order State Machine xử lý hủy đơn theo rule.

- Chỉ sau khi Core đã hủy đơn chính thức, Notification owner mới được gửi tin nhắn giao dịch giải thích.

- Không gửi tin nhắn hủy sau attempt 1 hoặc attempt 2.

- Không gửi tin nhắn hủy trước khi Core hủy đơn.

- Không dùng tin nhắn này để upsell, CRM hoặc marketing.

24/7 cancellation notice template:

Ginsengfood kính chào Mình.

Đơn hàng {{order_code_short}} của Mình chưa được xác nhận qua cuộc gọi tự động trong thời gian xác nhận đơn, nên đơn hàng đã được hủy theo quy trình.

Nếu Mình vẫn có nhu cầu đặt Cháo Sâm Savigin, Mình vui lòng phản hồi lại tin nhắn này để được hỗ trợ tạo đơn mới theo chương trình hiện hành ạ.

Ginsengfood trân trọng cảm ơn Mình.

## 24. INVALID PHONE / BUSY / KHÔNG ANSWER / KHÔNG INPUT GOVERNANCE

## 24.1. No Answer

No Answer là khi cuộc gọi được thực hiện hợp lệ, có tín hiệu gọi/đổ chuông hoặc không kết nối được theo rule nhà mạng/SIM Gateway, nhưng khách không nghe máy hoặc không có xác nhận hợp lệ.

No Answer sau attempt 1:

## ATTEMPT_1_NO_ANSWER

No Answer sau attempt 2:

## ATTEMPT_2_NO_ANSWER

Final No Answer:

## IVR_NO_ANSWER_FINAL

Program-specific final rule:

- GOLDEN_HOUR: IVR_NO_ANSWER_FINAL sau 2 attempts hợp lệ.

- TWENTY_FOUR_SEVEN: IVR_NO_ANSWER_FINAL sau 3 attempts hợp lệ.

Program-specific reason code:

- GOLDEN_HOUR: ORDER_CANCEL_REASON = IVR_NO_ANSWER_AFTER_2_ATTEMPTS.

- TWENTY_FOUR_SEVEN: ORDER_CANCEL_REASON = IVR_NO_ANSWER_AFTER_3_ATTEMPTS.

Important:

- Chỉ dùng nếu thực sự là no answer.

- Không dùng cho technical exception.

- Không dùng cho capacity incident.

- Không dùng cho invalid phone.

- Không dùng cho khách bấm phím 0.

- Không gửi tin nhắn hủy đơn ngay sau từng attempt.

- Chỉ sau khi Core Order State Machine hủy đơn chính thức, Notification owner mới được gửi transactional order status notice.

## 24.2. Busy

Busy là khi số khách bận máy.

Busy không tự động là khách từ chối.

Busy có thể xử lý như retry waiting nếu còn window và attempt.

Nếu hết attempts/window, Core xử lý theo rule tương ứng.

Busy không được xem là khách bấm 0.

Busy không được xem là technical exception nếu SIM Gateway trả busy hợp lệ.

## 24.3. No Input

No Input là khi cuộc gọi kết nối nhưng không có phím hợp lệ.

No Input không phải xác nhận.

No Input không phải hủy.

Nếu còn attempt/window, scheduler có thể gọi attempt tiếp theo.

Nếu hết attempts/window, Core xử lý theo rule.

## 24.4. Invalid Input

Invalid Input là khi khách bấm phím ngoài 1/0.

Invalid Input không được map thành confirm.

Invalid Input không được map thành cancel.

Invalid Input không được tạo order action.

Nếu invalid input xảy ra do DTMF lỗi kỹ thuật, phải chuyển technical exception.

Nếu invalid input là phím sai do khách bấm, xử lý theo rule IVR và window.

## 24.5. Invalid Phone

Invalid Phone có thể gồm:

- Số sai định dạng.

- Số không gọi được.

- Số không tồn tại theo phản hồi kỹ thuật đáng tin cậy.

- Số bị chặn.

- Số không đủ điều kiện gọi.

Invalid Phone final chỉ được kết luận khi rule xác nhận rõ.

Không được kết luận invalid phone chỉ vì một lần không nghe.

Invalid Phone final có thể dẫn tới:

## ORDER_CANCEL_REASON = IVR_INVALID_PHONE_IF_RULE_CONFIRMED

Core mới ghi nhận action cuối.

Nếu số điện thoại đã xác định không có thật, không gửi SMS vào chính số đó.

Nếu vẫn còn channel gốc hợp lệ như Messenger / Website / Facebook, Notification owner có thể gửi thông báo qua channel gốc theo policy.

## 25. DEADLINE-AWARE ROLLING QUEUE

## 25.1. Scheduler model chính thức

Scheduler model bắt buộc:

## ROLLING_REAL_TIME_IVR = REQUIRED

## BATCH_AFTER_SESSION_CALLING = PROHIBITED

## SCHEDULER_MODEL = DEADLINE_AWARE_ROLLING_QUEUE

Ý nghĩa:

- Call job được đưa vào queue ngay khi đơn cần IVR.

- Scheduler phải xét deadline của từng order.

- Scheduler phải xét số SIM khả dụng.

- Scheduler phải xét attempt number.

- Scheduler phải xét confirmation window.

- Scheduler phải xét program type.

- Scheduler không được gom cuối phiên.

- Scheduler không được gọi sau deadline.

- Scheduler không được assign call vào SIM đang bận.

- Scheduler phải ghi evidence nếu không đủ capacity.

## 25.2. Không batch cuối phiên

Batch cuối phiên bị cấm.

Không được dồn toàn bộ cuộc gọi sau live hoặc cuối Giờ Vàng.

Ví dụ lỗi:

- Giờ Vàng có 800 đơn cần IVR trong 10 phút.

- Hệ thống chỉ có 32 SIM.

- Scheduler dồn 800 cuộc vào cuối 5 phút.

- Nhiều cuộc gọi quá hạn.

- Hệ thống vẫn coi như gọi hợp lệ.

Đây là Fail Gate.

Với 32 SIM, năng lực 10 phút chỉ khoảng 384 cuộc theo baseline bảo thủ. Nếu có 800–1.200 cuộc cần IVR trong 10 phút, hệ thống phải tạo capacity incident, không được giả lập gọi thành công.

## 25.3. Deadline-aware nghĩa là gì

Deadline-aware nghĩa là mỗi call job phải biết:

- T0.

- Program type.

- Confirmation window.

- Attempt deadline.

- Next attempt time.

- Window expiry.

- Remaining time.

- SIM availability.

- Retry eligibility.

- Finalization rule.

Scheduler phải ưu tiên job có deadline gần hơn nếu rule cho phép.

Scheduler phải tránh tạo attempt không còn đủ thời gian.

Scheduler phải không tạo attempt tiếp theo nếu attempt trước đã final.

## 25.4. Rolling queue nghĩa là gì

Rolling queue nghĩa là hệ thống gọi liên tục theo thời gian thực khi đơn phát sinh, không đợi cuối phiên.

Ví dụ với Giờ Vàng:

- Đơn phát sinh tại T0.

- Attempt 1 gọi ngay tại T0 nếu có SIM.

- Nếu không nghe, attempt 2 được lên lịch T0 + 5 phút.

- Nếu hết 5 phút, window expired.

- Không gọi sau T0 + 5 phút.

Ví dụ với 24/7:

- Đơn phát sinh tại T0.

- Attempt 1 gọi ngay tại T0 nếu có SIM.

- Nếu không nghe, attempt 2 lên lịch T0 + 5 phút.

- Nếu tiếp tục không nghe, attempt 3 lên lịch T0 + 10 phút.

- Nếu hết 15 phút, window expired.

- Không gọi sau T0 + 15 phút.

## 25.5. SIM allocation rule

Scheduler chỉ được assign call job vào SIM channel:

- Đang available.

- Không có active outbound call.

- Health check pass.

- Không bị lỗi SIM_CHANNEL_FAILURE.

- Không bị cooldown nếu rule kỹ thuật yêu cầu.

- Không vượt capacity limit.

- Không bị owner/admin disable.

Không được assign hai cuộc gọi cùng lúc vào một SIM.

Nếu SIM health fail, job phải được chuyển SIM khác nếu còn thời gian và có SIM.

Nếu không còn SIM, ghi capacity issue.

## 25.6. Capacity incident

Capacity incident xảy ra khi:

- Số call job vượt năng lực SIM trong window.

- Không đủ SIM để gọi attempt đúng hạn.

- Queue backlog làm nhiều order hết window.

- SIM failure làm capacity giảm mạnh.

- Scheduler không đáp ứng deadline.

- Live/Golden Hour tạo lượng đơn vượt baseline.

- IVR job không thể xử lý real-time.

Capacity incident phải ghi evidence.

Capacity incident không được che giấu.

Capacity incident không được biến thành khách không nghe.

Capacity incident không được tự động hủy đơn như lỗi khách nếu nguyên nhân là năng lực hệ thống.

Capacity incident không được kích hoạt post-IVR cancellation notice.

Core/Owner cần rule xử lý capacity incident.

## 26. CALL JOB LIFECYCLE GOVERNANCE

## 26.1. Vòng đời call job

Call job nên đi qua các trạng thái logic:

- CALL_JOB_CREATED.

- CALL_JOB_ELIGIBILITY_CHECKED.

- CALL_JOB_SCHEDULED.

- SIM_CHANNEL_ASSIGNED.

- CALL_ATTEMPT_STARTED.

- CALL_CONNECTED hoặc CALL_NOT_CONNECTED.

- AUDIO_PLAYED.

- DTMF_WAITING.

- DTMF_CAPTURED hoặc NO_INPUT.

- RESULT_NORMALIZED.

- IVR_RESULT_RECORDED.

- CORE_SIGNAL_SENT.

- EVIDENCE_WRITTEN.

- CALL_JOB_FINALIZED.

Các trạng thái lỗi:

- CALL_JOB_BLOCKED.

- CALL_JOB_EXPIRED.

- SIM_CHANNEL_FAILURE.

- DTMF_CAPTURE_ERROR.

- AUDIO_PLAYBACK_ERROR.

- SCHEDULER_ERROR.

- RESULT_NORMALIZER_ERROR.

- EVIDENCE_WRITE_ERROR.

- ADMIN_REVIEW_REQUIRED.

## 26.2. Không tạo call job nếu order chưa đủ điều kiện

Không tạo call job nếu:

- Chưa có official order/order_code.

- Order chưa cần IVR.

- Order đã hủy.

- Order đã xác nhận IVR.

- Order đã hết confirmation window.

- Order không thuộc chương trình cần IVR theo rule.

- Thiếu số điện thoại hợp lệ.

- Thiếu total_amount_display.

- Thiếu order_code_short.

- Production SIM Gateway chưa được release.

- Evidence writer không sẵn sàng nếu production.

## 26.3. Không tạo duplicate call job

Một order không được có nhiều call job active cùng lúc trong cùng confirmation window.

Duplicate call job có thể gây:

- Gọi khách nhiều lần.

- Khách bấm phím nhiều kết quả khác nhau.

- Race condition với Core.

- Sai evidence.

- Sai order state.

- Khiếu nại.

- Sai ROAS/risk measurement nếu downstream dùng evidence.

Scheduler phải chống duplicate.

Duplicate call job là Fail Gate.

## 26.4. Call job expiry

Call job phải hết hiệu lực khi:

- Window expired.

- Order đã final.

- Khách đã bấm 1.

- Khách đã bấm 0.

- Core đã hủy đơn.

- Technical exception final.

- Owner/admin block theo rule.

- Order không còn đủ điều kiện IVR.

Call job expired không được gọi tiếp.

## 27. RESULT NORMALIZATION GOVERNANCE

## 27.1. Vì sao cần result normalizer

SIM Gateway có thể trả nhiều loại kết quả kỹ thuật.

PACK-09 cần call result normalizer để chuyển kết quả kỹ thuật thành IVR result chuẩn.

Normalizer phải phân biệt:

- Khách xác nhận.

- Khách hủy.

- Không nghe.

- Máy bận.

- Không bấm phím.

- Sai phím.

- Lỗi DTMF.

- Lỗi audio.

- Lỗi SIM.

- Lỗi server.

- Lỗi scheduler.

- Hết window.

- Capacity incident.

- Notification after Core cancellation.

Nếu không normalize đúng, Core có thể hủy nhầm đơn hoặc xử lý sai.

## 27.2. Normalized IVR result tối thiểu

Các normalized result tối thiểu:

- IVR_CONFIRMED.

- IVR_CUSTOMER_CANCELLED.

- ATTEMPT_1_NO_ANSWER.

- ATTEMPT_1_NO_INPUT.

- ATTEMPT_1_INVALID_INPUT.

- ATTEMPT_2_NO_ANSWER.

- ATTEMPT_2_NO_INPUT.

- ATTEMPT_2_INVALID_INPUT.

- ATTEMPT_3_NO_ANSWER.

- ATTEMPT_3_NO_INPUT.

- ATTEMPT_3_INVALID_INPUT.

- IVR_NO_ANSWER_FINAL.

- IVR_CONFIRMATION_WINDOW_EXPIRED.

- IVR_INVALID_PHONE_FINAL.

- IVR_TECHNICAL_EXCEPTION.

- IVR_ADMIN_REVIEW_REQUIRED.

- IVR_RESULT_INVALID.

- IVR_RESULT_DUPLICATE_BLOCKED.

- IVR_CAPACITY_EXCEPTION.

- IVR_POST_CANCEL_NOTICE_waiting.

- IVR_POST_CANCEL_NOTICE_SENT.

- IVR_POST_CANCEL_NOTICE_BLOCKED.

## 27.3. Result normalizer không được đổi order state

Call result normalizer chỉ chuẩn hóa kết quả.

Normalizer không được:

- Hủy đơn.

- Tiếp tục đơn.

- Cho xuất kho.

- Cho giao hàng.

- Đánh dấu verified revenue.

- Ghi MISA.

- Tạo payment status.

- Tạo delivery status.

- Tự gửi cancellation notice.

Normalizer chỉ gửi result về Core Order State Machine qua boundary hợp lệ.

## 28. ADMIN / MANUAL CONTROL PRINCIPLES TRONG PHẦN CALL

## 28.1. Admin không được sửa IVR result giả

Admin không được sửa IVR result để làm như khách đã bấm phím.

Không được tạo:

- Fake IVR_CONFIRMED.

- Fake IVR_CUSTOMER_CANCELLED.

- Fake NO_ANSWER.

- Fake DTMF result.

- Fake call evidence.

- Fake post-cancel notice sent.

Nếu cần xử lý thủ công, phải dùng Admin Review / Manual Review flow riêng, có reason, permission và audit.

Manual review không được giả danh IVR.

## 28.2. Admin không được tạo retry ngoài rule

Admin không được bấm gọi lại ngoài rule nếu:

- Đã hết confirmation window.

- Đã có result final.

- Đơn đã hủy.

- Đơn không còn eligible.

- Retry vượt attempt theo program.

- Owner rule không cho phép exception.

Nếu có trường hợp ngoại lệ, phải có owner-approved override, audit và evidence.

## 29. DONE CONDITION CỦA PHẦN 2/4

## PHẦN 2/4 được xem là hoàn thành về mặt tài liệu khi đã khóa được:

- Call script governance.

- Nội dung cuộc gọi chuẩn.

- Biến được phép dùng.

- Biến không được đọc.

- Data minimization trong cuộc gọi.

- DTMF key rule.

- Phím 1 = CONFIRM_ORDER.

- Phím 0 = CANCEL_ORDER_BY_CUSTOMER.

- Không bấm phím / sai phím / lỗi DTMF.

- Call result classification.

- Program-based call rule.

- MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED.

- ATTEMPT_INTERVAL = PROGRAM_BASED.

- Giờ Vàng: 10 phút, 2 attempts, 5 phút.

- 24/7: 15 phút, 3 attempts, T0 / T0 + 5 phút / T0 + 10 phút.

- Post-IVR cancellation notice chỉ gửi sau khi Core đã hủy đơn chính thức.

- Confirmation window là giới hạn cứng.

- Deadline-aware rolling queue.

- Không batch cuối phiên.

- SIM allocation rule.

- Capacity incident.

- Call job lifecycle.

- Duplicate call job block.

- Result normalization.

- Admin/manual control principles.

## PHẦN 2/4 chưa viết chi tiết capacity baseline theo 12/24/32 SIM, engineer handoff, cross-pack implementation boundary, P0 MUST/MUST NOT/FAIL IF, Smoke Matrix, Evidence, Done Gate tổng thể, Fail Gate tổng thể và Release Control. Các nội dung đó thuộc PHẦN 3/4 và PHẦN 4/4.

## KẾT LUẬN PHẦN 2/4

PACK-09 dùng call script ngắn, rõ và chỉ phục vụ xác nhận đơn hàng.

IVR chỉ được đọc mã đơn rút gọn và tổng thanh toán, có thể dùng tên ngắn hoặc tên chương trình nếu policy cho phép.

IVR không được đọc full profile, full address, member tier, Diamond referral, payment detail, order history, AI consultation, CRM content hoặc health/sensitive note.

DTMF chỉ có hai phím trong scope hiện tại:

Phím 1 = xác nhận tiếp tục xử lý đơn.

Phím 0 = khách không đặt hoặc muốn hủy đơn.

Phím 9 hỗ trợ người thật hiện NOT_ENABLED.

Giờ Vàng có confirmation window 10 phút, tối đa 2 cuộc, attempt 2 cách attempt 1 đúng 5 phút.

24/7 có confirmation window 15 phút, tối đa 3 cuộc, lịch gọi T0 / T0 + 5 phút / T0 + 10 phút.

Scheduler bắt buộc là:

## DEADLINE_AWARE_ROLLING_QUEUE

Không được batch toàn bộ cuộc gọi cuối phiên.

Không được gọi sau khi window hết hạn.

Không được gọi lại vô hạn.

Không được tạo duplicate call job.

Không được tính lỗi kỹ thuật là khách không nghe.

Không được để SIM Gateway hoặc IVR normalizer cập nhật order trực tiếp.

Post-IVR cancellation notice chỉ được gửi sau khi Core Order State Machine đã hủy đơn chính thức.

Nguyên tắc khóa của PHẦN 2/4 là:

Call script chỉ xác nhận đơn.

IVR không phải marketing.

IVR không phải CRM.

Phím 1 xác nhận, phím 0 hủy.

Không có phím 9 trong scope hiện tại.

Giờ Vàng 10 phút / 2 attempts.

24/7 15 phút / 3 attempts.

Attempt interval theo program: Giờ Vàng cách 5 phút; 24/7 cách 5 phút.

Confirmation window là giới hạn cứng.

Rolling queue là bắt buộc.

Batch cuối phiên bị cấm.

Technical error không phải No Answer.

Capacity incident không phải No Answer.

Core Order State Machine mới là lớp quyết định cuối.

Tin nhắn giải thích hủy đơn chỉ gửi sau khi Core đã hủy đơn.

## PHẦN 3/4 — CALL RESULT / TECHNICAL EXCEPTION / CAPACITY BASELINE / CROSS-PACK CONTROL / ENGINEER HANDOFF

## 30. MỤC TIÊU CỦA PHẦN 3/4

## PHẦN 3/4 khóa lớp xử lý kết quả cuộc gọi IVR, phân biệt lỗi nghiệp vụ và lỗi kỹ thuật, xác định năng lực SIM Gateway, kiểm soát biên tích hợp với các pack khác và chuẩn hóa handoff cho đội kỹ thuật triển khai.

Mục tiêu của phần này là xác định rõ:

- IVR result là gì.

- IVR result không phải là order state cuối.

- Call result phải được normalize trước khi gửi Core.

- Lỗi kỹ thuật không được tính là khách không nghe.

- SIM Gateway không được hủy đơn trực tiếp.

- Core Order State Machine là lớp quyết định cuối.

- Năng lực gọi phải tính theo số SIM vật lý.

- Một SIM chỉ có một active outbound call.

- Không được batch toàn bộ cuộc gọi cuối phiên.

- Capacity incident phải được ghi nhận.

- AI Advisor không được tạo call job.

- Facebook Gateway / Messenger / Live không được gọi IVR trực tiếp.

- CRM không được dùng IVR cho chăm sóc/marketing.

- Notification owner là bên duy nhất được gửi post-IVR cancellation notice sau khi Core đã hủy đơn.

- Payment / MISA / Warehouse / Traceability không được consume sai IVR result.

- Kỹ sư backend/SIM Gateway và kỹ sư Admin UI/Monitoring/Evidence phải có ranh giới nhiệm vụ rõ.

## PHẦN 3/4 không thiết kế code, API, database hoặc UI chi tiết.

## PHẦN 3/4 chỉ khóa governance/domain và handoff kỹ thuật ở mức định hướng triển khai.

## 31. CALL RESULT GOVERNANCE

## 31.1. IVR result là input signal

Nguyên tắc khóa:

## IVR_RESULT_IS_INPUT_SIGNAL_ONLY

## ORDER_STATE_MACHINE_IS_FINAL_DECISION_LAYER

IVR result chỉ là tín hiệu đầu vào cho Core Order State Machine.

IVR result không tự động là order state.

IVR result không tự động hủy đơn.

IVR result không tự động xác nhận đơn hoàn tất.

IVR result không tự động cho xuất kho.

IVR result không tự động xác nhận thanh toán.

IVR result không tự động xác nhận giao hàng.

IVR result không tự động tạo verified revenue.

IVR result không tự động kích hoạt notification nếu Core chưa quyết định.

## 31.2. Call result phải được normalize

SIM Gateway có thể trả về nhiều kết quả kỹ thuật khác nhau.

Các kết quả này phải được Call Result Normalizer chuẩn hóa thành IVR result hợp lệ trước khi gửi về Core.

Không được gửi raw SIM Gateway result trực tiếp vào Core Order State Machine.

Không được để Core phải tự hiểu từng loại lỗi kỹ thuật của SIM Gateway.

Không được để Admin UI tự diễn giải raw call result thành trạng thái đơn.

Call Result Normalizer là lớp chuẩn hóa kết quả kỹ thuật thành ngôn ngữ nghiệp vụ IVR.

## 31.3. Nhóm IVR result hợp lệ

Nhóm | IVR result | Ý nghĩa

Xác nhận | IVR_CONFIRMED | Khách bấm phím 1 xác nhận tiếp tục xử lý đơn

Hủy bởi khách | IVR_CUSTOMER_CANCELLED | Khách bấm phím 0, không đặt hoặc muốn hủy

Không nghe tạm thời | ATTEMPT_1_NO_ANSWER / ATTEMPT_2_NO_ANSWER / ATTEMPT_3_NO_ANSWER | Attempt chưa final, còn quyền gọi tiếp nếu program cho phép

Không nghe cuối | IVR_NO_ANSWER_FINAL | Hết attempts theo program, không có xác nhận hợp lệ

Hết thời gian | IVR_CONFIRMATION_WINDOW_EXPIRED | Hết window 10 phút hoặc 15 phút

Sai số / không gọi được | IVR_INVALID_PHONE_FINAL | Số điện thoại không hợp lệ nếu rule xác nhận

Lỗi kỹ thuật | IVR_TECHNICAL_EXCEPTION | Lỗi SIM/server/DTMF/audio/scheduler/callback

Cần Admin review | ADMIN_REVIEW_REQUIRED | Cần người có quyền xem xét

Quá tải năng lực | IVR_CAPACITY_EXCEPTION | Không đủ SIM/capacity trong window

Duplicate | IVR_RESULT_DUPLICATE_BLOCKED | Kết quả bị chặn do trùng

Notice waiting | IVR_POST_CANCEL_NOTICE_waiting | Chờ Notification owner gửi notice sau Core cancellation

Notice sent | IVR_POST_CANCEL_NOTICE_SENT | Notification owner đã gửi transactional notice

Notice bị chặn | IVR_POST_CANCEL_NOTICE_BLOCKED | Notice bị chặn do chưa đủ điều kiện

## 31.4. IVR_CONFIRMED

Khi khách bấm phím 1, IVR result là:

## IVR_CONFIRMED

Ý nghĩa:

- Khách xác nhận tiếp tục xử lý đơn.

- Không gọi attempt tiếp theo.

- IVR gửi signal về Core.

- Core Order State Machine quyết định trạng thái tiếp theo.

- Evidence phải được ghi nhận.

IVR_CONFIRMED không đồng nghĩa:

- Đã thanh toán.

- Đã giao hàng.

- Đã xuất kho.

- Đã hoàn tất đơn.

- Đã verified revenue.

- Đã đủ điều kiện commission.

- Đã đủ điều kiện ROAS.

- Đã sync MISA.

## 31.5. IVR_CUSTOMER_CANCELLED

Khi khách bấm phím 0, IVR result là:

## IVR_CUSTOMER_CANCELLED

Ý nghĩa:

- Khách xác nhận không đặt hoặc muốn hủy đơn.

- IVR gửi cancel request signal về Core.

- Core Order State Machine mới được xử lý hủy đơn.

- SIM Gateway không được hủy đơn trực tiếp.

- Evidence phải được ghi nhận.

Reason code nền:

## ORDER_CANCEL_REASON = CUSTOMER_CANCELLED_BY_IVR_KEY_0

Reason code phải do Core ghi nhận.

Nếu Core chưa hủy đơn, không được gửi tin nhắn nói đơn đã hủy.

## 31.6. IVR_NO_ANSWER_FINAL

Khi đã gọi đủ số attempts hợp lệ theo program mà khách không nghe hoặc không có xác nhận hợp lệ, IVR result có thể là:

## IVR_NO_ANSWER_FINAL

Program-specific final rule:

- GOLDEN_HOUR: đủ 2 attempts hợp lệ trong 5 phút.

- TWENTY_FOUR_SEVEN: đủ 3 attempts hợp lệ trong 15 phút.

Program-specific reason code:

- GOLDEN_HOUR: ORDER_CANCEL_REASON = IVR_NO_ANSWER_AFTER_2_ATTEMPTS.

- TWENTY_FOUR_SEVEN: ORDER_CANCEL_REASON = IVR_NO_ANSWER_AFTER_3_ATTEMPTS.

Điều kiện bắt buộc:

- Các attempts đã diễn ra hợp lệ theo program.

- Không có xác nhận phím 1.

- Không có phím 0.

- Không có technical exception.

- Không có capacity incident gây thất bại gọi.

- Confirmation window đã xử lý đúng rule.

- Core Order State Machine là bên quyết định hủy đơn cuối cùng.

Không được dùng IVR_NO_ANSWER_FINAL nếu nguyên nhân là lỗi kỹ thuật.

Không được dùng IVR_NO_ANSWER_FINAL nếu nguyên nhân là capacity incident.

Không được gửi tin nhắn hủy đơn trước khi Core hủy đơn chính thức.

## 31.7. IVR_CONFIRMATION_WINDOW_EXPIRED

Khi hết confirmation window nhưng chưa có xác nhận hợp lệ:

## IVR_CONFIRMATION_WINDOW_EXPIRED

Core Action:

- Core hủy đơn theo rule.

- Quote hết hiệu lực nếu policy quy định.

- Không nhận kết quả IVR muộn như xác nhận hợp lệ.

- Không gọi thêm attempt ngoài window.

- Không kéo dài quote bằng IVR.

Reason code nền:

## ORDER_CANCEL_REASON = IVR_CONFIRMATION_WINDOW_EXPIRED

Notification owner chỉ được gửi tin nhắn giải thích nếu Core đã hủy đơn chính thức và policy cho phép.

## 31.8. IVR_INVALID_PHONE_FINAL

Invalid phone chỉ được kết luận nếu rule xác nhận rõ.

Không được kết luận invalid phone chỉ vì một lần không nghe.

Không được kết luận invalid phone chỉ vì SIM Gateway lỗi.

Không được kết luận invalid phone nếu lỗi thuộc nhà mạng/SIM/server không chắc chắn.

Reason code nền:

## ORDER_CANCEL_REASON = IVR_INVALID_PHONE_IF_RULE_CONFIRMED

Core mới được ghi reason code.

Nếu số điện thoại invalid, không gửi SMS vào chính số đó.

Nếu còn channel gốc hợp lệ, Notification owner có thể gửi transactional notice qua channel gốc theo policy.

## 31.9. IVR_CAPACITY_EXCEPTION

IVR_CAPACITY_EXCEPTION xảy ra khi hệ thống không đủ năng lực gọi đúng deadline.

Ví dụ:

- Số call job vượt năng lực SIM trong window.

- Queue backlog làm attempt 1, attempt 2 hoặc attempt 3 quá hạn.

- SIM failure làm năng lực giảm đột ngột.

- Scheduler không thể assign SIM trước khi window hết.

- Live/Giờ Vàng phát sinh số đơn vượt capacity baseline.

- Hệ thống không thể gọi rolling real-time.

Capacity exception không phải khách không nghe.

Capacity exception không được dùng để hủy đơn như lỗi khách.

Capacity exception không được kích hoạt post-IVR cancellation notice.

Capacity exception phải chuyển owner/admin review hoặc xử lý theo rule Core đã được phê duyệt.

## 31.10. Duplicate IVR result

Một order không được có nhiều IVR result final cạnh tranh nhau.

Ví dụ lỗi:

- Một call job trả IVR_CONFIRMED.

- Call job duplicate khác trả IVR_CUSTOMER_CANCELLED.

- Scheduler tạo 2 cuộc gọi cùng order.

- Callback retry tạo duplicate result.

- Admin thao tác trùng.

Duplicate phải bị chặn và đưa vào review.

Trạng thái:

## IVR_RESULT_DUPLICATE_BLOCKED

## ADMIN_REVIEW_REQUIRED

Không được để duplicate result tự cập nhật Core.

Không được gửi notification dựa trên duplicate result chưa được Core công nhận.

## 32. CORE ORDER STATE MACHINE BOUNDARY

## 32.1. Core là final decision layer

Core Order State Machine chịu trách nhiệm:

- Tiếp nhận IVR result.

- Kiểm tra order còn đủ điều kiện xử lý không.

- Kiểm tra order đã final chưa.

- Kiểm tra order có bị hủy/hold/sale lock/risk không.

- Kiểm tra result có hợp lệ không.

- Quyết định tiếp tục xử lý đơn.

- Quyết định hủy đơn.

- Quyết định Admin Review.

- Ghi state transition.

- Ghi audit.

- Phát hành trigger hợp lệ cho Notification owner nếu đơn đã hủy và policy cho phép.

IVR không làm thay các bước này.

## 32.2. IVR không được cancel order trực tiếp

Nguyên tắc khóa:

## IVR_CAN_TRIGGER_CANCEL_REQUEST = CÓ

## IVR_CAN_DIRECTLY_CANCEL_ORDER = KHÔNG

## SIM_GATEWAY_CAN_CANCEL_ORDER = KHÔNG

## CORE_ORDER_STATE_MACHINE_CANCEL_REQUIRED = CÓ

IVR có thể gửi tín hiệu khách bấm phím 0.

IVR có thể gửi tín hiệu no answer final.

IVR có thể gửi tín hiệu window expired.

Nhưng IVR không được trực tiếp đổi order state sang cancelled.

Mọi hủy đơn phải đi qua Core Order State Machine.

## 32.3. IVR không được release order trực tiếp

IVR_CONFIRMED không đồng nghĩa order được release.

Sau IVR_CONFIRMED, Core vẫn có thể cần kiểm tra:

- Payment status.

- COD/payment method.

- Stock/sellable status.

- Sale Lock.

- Fraud/risk.

- Address validity.

- Warehouse readiness.

- Delivery eligibility.

- Program/quote validity.

- Other owner rules.

IVR không được cho phép xuất kho trực tiếp.

IVR không được cho phép giao hàng trực tiếp.

## 32.4. State transition phải audit

Mọi thay đổi trạng thái order do IVR signal dẫn đến phải có audit.

Audit tối thiểu:

- Order ID/order_code.

- IVR result.

- Call job reference.

- Attempt number.

- DTMF key nếu có.

- Core decision.

- State before.

- State after.

- Reason code.

- Timestamp.

- Correlation ID.

- Actor/system.

- Evidence reference.

- Notification trigger nếu có.

Không có audit thì không được coi là xử lý hợp lệ.

## 33. TECHNICAL EXCEPTION GOVERNANCE

## 33.1. Lỗi kỹ thuật không phải lỗi khách

Nguyên tắc khóa:

Lỗi kỹ thuật không được xử lý như khách không nghe.

Các lỗi kỹ thuật gồm:

- SIM_GATEWAY_ERROR.

- SERVER_ERROR.

- DTMF_CAPTURE_ERROR.

- AUDIO_PLAYBACK_ERROR.

- SIM_CHANNEL_FAILURE.

- INTERNAL_CALLBACK_ERROR.

- SCHEDULER_ERROR.

- RESULT_NORMALIZER_ERROR.

- AUDIT_WRITE_ERROR.

- EVIDENCE_WRITE_ERROR.

Các lỗi này phải đi vào:

## IVR_TECHNICAL_EXCEPTION

## ADMIN_REVIEW_REQUIRED

## 33.2. SIM_GATEWAY_ERROR

SIM Gateway error có thể gồm:

- Gateway không phản hồi.

- Gateway mất kết nối.

- Gateway trả lỗi không xác định.

- Gateway không tạo được cuộc gọi.

- Gateway reset.

- Gateway quá tải.

- Gateway lỗi firmware/phần cứng.

SIM Gateway error không được tính là no answer.

SIM Gateway error không được hủy đơn theo lý do khách không nghe.

SIM Gateway error không được kích hoạt cancellation notice.

## 33.3. SERVER_ERROR

Server error có thể gồm:

- Server điều phối cuộc gọi lỗi.

- Server không ghi được call job.

- Server không xử lý callback.

- Server timeout.

- Server mất kết nối với SIM Gateway.

- Server lỗi internal.

- Server không ghi được audit.

Server error phải chuyển technical exception.

Không được dùng server error để tạo IVR result final theo hướng khách không xác nhận.

Server error không được kích hoạt cancellation notice.

## 33.4. DTMF_CAPTURE_ERROR

DTMF capture error xảy ra khi hệ thống không ghi nhận được phím bấm do lỗi kỹ thuật.

DTMF_CAPTURE_ERROR không phải khách không bấm phím.

DTMF_CAPTURE_ERROR không phải khách không nghe.

DTMF_CAPTURE_ERROR không được map thành NO_INPUT nếu nguyên nhân là lỗi capture.

DTMF_CAPTURE_ERROR phải đi vào technical exception.

## 33.5. AUDIO_PLAYBACK_ERROR

Audio playback error xảy ra khi hệ thống không phát được nội dung cuộc gọi.

Nếu khách không nghe được nội dung, không thể coi là khách đã nhận thông tin xác nhận.

AUDIO_PLAYBACK_ERROR không được tính là no answer.

AUDIO_PLAYBACK_ERROR không được tính là no input.

AUDIO_PLAYBACK_ERROR phải đi vào technical exception.

## 33.6. SIM_CHANNEL_FAILURE

SIM channel failure xảy ra khi SIM cụ thể bị lỗi.

Ví dụ:

- SIM mất sóng.

- SIM bị khóa.

- SIM không gọi ra được.

- SIM đang treo trạng thái bận.

- SIM bị lỗi phần cứng.

- SIM không trả kết quả.

- SIM bị nhà mạng chặn.

SIM channel failure không phải khách không nghe.

Call job có thể được chuyển SIM khác nếu còn window và rule cho phép.

Nếu không còn thời gian hoặc không còn SIM, ghi capacity/technical exception.

## 33.7. SCHEDULER_ERROR

Scheduler error xảy ra khi:

- Scheduler không tính đúng deadline.

- Scheduler không assign SIM được.

- Scheduler assign trùng SIM.

- Scheduler gọi ngoài window.

- Scheduler tạo duplicate call job.

- Scheduler không tạo attempt đúng thời điểm.

- Scheduler batch cuối phiên trái rule.

- Scheduler tạo attempt vượt program rule.

Scheduler error là lỗi kỹ thuật nghiêm trọng.

Không được che scheduler error bằng no answer.

Scheduler error không được kích hoạt cancellation notice.

## 33.8. Technical exception final

Technical exception có thể trở thành final nếu:

- Không thể retry an toàn.

- Lỗi lặp lại.

- Window đã hết nhưng nguyên nhân là lỗi hệ thống.

- Evidence writer không ghi được.

- SIM Gateway không đủ trạng thái xác nhận.

- Owner rule yêu cầu Admin Review.

Trạng thái:

## TECHNICAL_EXCEPTION_FINAL

## ADMIN_REVIEW_REQUIRED

Core xử lý theo rule riêng, không dùng reason khách không nghe.

Technical exception final không kích hoạt post-IVR cancellation notice.

## 34. CAPACITY BASELINE GOVERNANCE

## 34.1. Nguyên tắc tính capacity

Năng lực IVR phải tính theo số SIM vật lý và chu kỳ cuộc gọi.

Baseline khóa:

## AVERAGE_CALL_DURATION = 35_SECONDS

## CONSERVATIVE_CALL_CYCLE = 50_SECONDS_PER_CALL_PER_SIM

Ý nghĩa:

- Trung bình một cuộc gọi khoảng 35 giây.

- Tính bảo thủ 50 giây/cuộc/SIM.

- Mỗi SIM xử lý tuần tự từng cuộc.

- Không tính một SIM có thể gọi song song.

- Không tính theo số call job trong queue.

- Không tính theo mong muốn vận hành.

- Không giả định SIM có thể vượt capacity vật lý.

## 34.2. Năng lực trong 5 phút

Số SIM | Năng lực 5 phút

12 SIM | ~72 cuộc

24 SIM | ~144 cuộc

32 SIM | ~192 cuộc

Ý nghĩa:

- Giờ Vàng window 10 phút rất ngắn.

- 12 SIM không thể xử lý hàng trăm đơn trong 5 phút.

- 32 SIM cũng chỉ xử lý khoảng 192 cuộc trong 5 phút theo baseline.

- Nếu số đơn cần IVR vượt capacity, phải tạo capacity incident.

- Không được giả lập đã gọi đủ nếu SIM không đủ.

## 34.3. Năng lực trong 15 phút

Số SIM | Năng lực 15 phút

12 SIM | ~216 cuộc

24 SIM | ~432 cuộc

32 SIM | ~576 cuộc

Ý nghĩa:

- 24/7 window 15 phút rộng hơn Giờ Vàng.

- 24/7 có thể có tối đa 3 attempts/order.

- Capacity planning phải tính theo tổng số call attempts, không chỉ số order.

- Nếu demand vượt capacity, scheduler phải ghi capacity incident.

- Không được gọi trễ ngoài 15 phút rồi vẫn coi là hợp lệ.

## 34.4. Năng lực trong phiên 45 phút nếu rolling queue

Số SIM | Năng lực 45 phút

12 SIM | ~648 cuộc

24 SIM | ~1.296 cuộc

32 SIM | ~1.728 cuộc

Ý nghĩa:

- Rolling queue giúp phân tán tải trong toàn phiên.

- Không dồn toàn bộ cuộc gọi cuối phiên.

- Không lấy capacity 45 phút để hợp thức hóa window 10 phút.

- Đơn Giờ Vàng vẫn phải theo window 10 phút của từng order.

- Đơn 24/7 vẫn phải theo window 15 phút của từng order.

- 24/7 3 attempts làm tăng tổng call load nếu khách không nghe attempt 1/2.

## 34.5. Khuyến nghị cấu hình SIM

## PILOT_SIM_COUNT = 12

## MINIMUM_LAUNCH_SIM_COUNT = 24

## RECOMMENDED_MONTH_1_2_SIM_COUNT = 32

## RECOMMENDED_MONTH_3_4_SIM_COUNT = 64

## RECOMMENDED_MONTH_5_6_SIM_COUNT = 96

Ý nghĩa:

- 12 SIM phù hợp pilot nội bộ.

- 24 SIM là mức tối thiểu launch.

- 32 SIM phù hợp tháng 1–2 nếu volume chưa quá lớn.

- 64 SIM phù hợp giai đoạn tăng tải tháng 3–4.

- 96 SIM phù hợp giai đoạn scale mạnh tháng 5–6.

- Mọi mức cấu hình vẫn phải theo test/smoke/capacity evidence thực tế.

## 34.6. Capacity không thay thế release gate

Có đủ SIM không đồng nghĩa IVR production ready.

Ngoài capacity, vẫn phải có:

- Implementation complete.

- Scheduler đúng rule.

- DTMF pass.

- Result normalizer pass.

- Core boundary pass.

- Technical exception pass.

- Admin monitoring pass.

- Evidence pass.

- Security pass.

- Owner sign-off.

- Release decision.

Capacity chỉ là một điều kiện trong release readiness.

## 35. CAPACITY INCIDENT GOVERNANCE

## 35.1. Khi nào tạo capacity incident

Capacity incident bắt buộc khi:

- Call job vượt năng lực SIM trong window.

- Scheduler không gọi kịp attempt 1.

- Scheduler không gọi kịp attempt 2.

- Scheduler không gọi kịp attempt 3 của 24/7.

- Queue backlog vượt ngưỡng.

- SIM failure làm giảm năng lực.

- Live/Giờ Vàng phát sinh đơn quá nhanh.

- 32 SIM bị giao xử lý 800–1.200 cuộc trong 5 phút.

- Batch cuối phiên khiến nhiều cuộc hết hạn.

- Rolling queue không hoạt động.

- Scheduler không tính được deadline.

## 35.2. Capacity incident không phải no answer

Nếu hệ thống không đủ năng lực gọi, không được kết luận khách không nghe.

Không được map capacity incident thành:

## IVR_NO_ANSWER_FINAL

Không được hủy đơn theo reason:

## IVR_NO_ANSWER_AFTER_PROGRAM_ATTEMPTS

hoặc bất kỳ reason no-answer theo program nào khi nguyên nhân là capacity.

Cụ thể:

- Không dùng IVR_NO_ANSWER_AFTER_2_ATTEMPTS cho Golden Hour nếu nguyên nhân là capacity.

- Không dùng IVR_NO_ANSWER_AFTER_3_ATTEMPTS cho 24/7 nếu nguyên nhân là capacity.

- Capacity incident không được kích hoạt post-IVR cancellation notice.

Capacity incident phải chuyển:

- ADMIN_REVIEW_REQUIRED.

- OWNER_REVIEW_REQUIRED nếu ảnh hưởng nhiều đơn.

- CAPACITY_INCIDENT_MONITOR.

- Core handling theo rule riêng.

## 35.3. Capacity evidence

Capacity incident phải ghi evidence:

- Thời điểm.

- Program.

- Window.

- Số call job.

- Số SIM active.

- Số SIM available.

- Số SIM lỗi.

- Queue backlog.

- Số job quá deadline.

- Số job chưa gọi.

- Số job bị expired.

- Scheduler status.

- Owner review status.

Không có capacity evidence thì không được đóng incident.

## 36. SCHEDULER GOVERNANCE

## 36.1. Deadline-aware rolling queue là bắt buộc

Nguyên tắc khóa:

## ROLLING_REAL_TIME_IVR = REQUIRED

## BATCH_AFTER_SESSION_CALLING = PROHIBITED

## SCHEDULER_MODEL = DEADLINE_AWARE_ROLLING_QUEUE

Scheduler phải:

- Nhận call job theo thời gian thực.

- Xác định program window.

- Xác định T0.

- Tính attempt 1.

- Tính attempt 2.

- Tính attempt 3 nếu program là TWENTY_FOUR_SEVEN.

- Tính window expiry.

- Xếp queue theo deadline.

- Kiểm tra SIM available.

- Không assign SIM đang bận.

- Không gọi sau deadline.

- Không batch cuối phiên.

- Ghi capacity incident nếu quá tải.

## 36.2. Scheduler không được làm gì

Scheduler không được:

- Gọi ngoài confirmation window.

- Tạo attempt vượt rule theo program.

- Gọi dồn liên tục.

- Gọi lại ngay sau vài giây.

- Gọi sau khi đã có final result.

- Gọi order đã hủy.

- Gọi order không còn eligible.

- Assign nhiều call vào một SIM.

- Batch toàn bộ cuối phiên.

- Tự gia hạn window.

- Tự giả lập result khi không gọi được.

- Tự cập nhật order state.

- Tự kích hoạt cancellation notice.

Important:

- Với GOLDEN_HOUR, tạo attempt 3 là sai.

- Với TWENTY_FOUR_SEVEN, attempt 3 là hợp lệ nếu đúng lịch T0 + 10 phút và còn trong 15 phút.

- Với TWENTY_FOUR_SEVEN, tạo attempt 4 là sai.

## 36.3. Scheduler phải chống duplicate

Scheduler phải chống:

- Duplicate call job.

- Duplicate attempt.

- Duplicate callback.

- Duplicate DTMF result.

- Duplicate Core signal.

- Duplicate evidence.

- Duplicate cancellation notice.

Một order trong một confirmation window không được có nhiều call job active.

Nếu duplicate phát hiện, phải block và review.

## 37. CROSS-PACK BOUNDARY

## 37.1. Với Commerce Order Core / IVR / Operational Core

Boundary đúng:

| Thành phần | Vai trò đúng |
| ---------- | ------------ |
| Commerce Order Core / Order State Machine | Sở hữu trạng thái đơn, xác nhận, hủy, hết hạn, release tiếp |
| IVR | Nhận task, gọi khách, capture DTMF, normalize result, gửi callback |
| Operational Core | Cung cấp sale lock, recall, availability, warehouse/traceability constraints |

Luồng chuẩn:

1. Order Core tạo IVR confirmation task.
2. IVR thực hiện cuộc gọi.
3. IVR gửi result signal/callback về Order Core.
4. Order Core revalidate toàn bộ điều kiện.
5. Order Core quyết định transition trạng thái đơn.

PACK-09 chỉ cung cấp IVR result như input signal.

Không được để IVR thay Core.

Không được để SIM Gateway đổi order state.

Không được để Admin UI IVR hủy đơn ngoài Core State Machine.

## 37.2. Với AI Advisor

AI Advisor không được:

- Tạo IVR call job.

- Gọi SIM Gateway.

- Gọi số khách.

- Hiển thị IVR result như production data khi IVR chưa release.

- Tự nói đơn đã IVR xác nhận nếu Core chưa công nhận.

- Dùng nội dung chat để giả lập phím 1.

- Dùng nội dung chat để giả lập phím 0.

- Tự hủy đơn do khách nói trong chat.

- Tự retry IVR.

- Tự gửi post-IVR cancellation notice.

AI Advisor chỉ được consume trạng thái đã được Core/owner runtime công nhận nếu policy cho phép.

## 37.3. Với Facebook Gateway / Messenger / Live

Facebook Gateway / Messenger / Live không được:

- Gọi IVR trực tiếp.

- Tạo IVR call job.

- Gọi SIM Gateway.

- Dùng comment làm IVR result.

- Dùng Messenger text làm IVR result.

- Dùng live comment làm IVR confirmation.

- Thay IVR bằng tin nhắn xác nhận không có rule.

- Hủy đơn ngoài Core State Machine.

- Hiển thị IVR result chưa release như production truth.

- Tự gửi cancellation notice nếu Notification owner chưa phát hành message hợp lệ.

PACK-06 là channel gateway.

PACK-08 là live orchestration.

PACK-09 là IVR order confirmation.

Các pack này không được lẫn vai trò.

## 37.4. Với CRM / Member Lifecycle

CRM không được dùng IVR cho:

- Tin nhắn chăm sóc.

- Nhắc mua lại.

- Nhắc quyền lợi thành viên.

- Chúc mừng sinh nhật.

- Nhắc Giờ Vàng.

- Mời Diamond.

- Nhắc nâng hạng.

- Nhắc duy trì hạng.

- Chăm sóc sau mua.

- Quảng bá sản phẩm mới.

- Mời chương trình khởi nghiệp.

- Survey marketing.

CRM không được dùng post-IVR cancellation notice như marketing/upsell.

PACK-09 không phải CRM call center.

IVR call chỉ để xác nhận đơn.

## 37.5. Với Notification owner

Notification owner là bên duy nhất được phát hành post-IVR cancellation notice sau khi Core đã hủy đơn chính thức.

Notification owner phải:

- Nhận trigger hợp lệ từ Core/Order owner.

- Dùng template transactional.

- Không biến notice thành marketing.

- Không chứa dữ liệu bị cấm.

- Ghi audit/evidence.

- Gửi qua channel hợp lệ.

- Không gửi nếu reason là technical exception.

- Không gửi nếu reason là capacity incident.

- Không gửi nếu order chưa cancelled bởi Core.

## 37.6. Với Payment

Payment owner sở hữu:

- Payment confirmation.

- Bank transfer reconciliation.

- COD payment status nếu thuộc payment layer.

- Payment failed.

- Payment dispute.

- Payment evidence.

IVR không được:

- Xác nhận thanh toán.

- Đánh dấu paid.

- Xác nhận chuyển khoản.

- Xử lý ảnh chuyển khoản.

- Đối chiếu ngân hàng.

- Đánh dấu COD success.

- Tạo payment evidence.

- Đọc payment detail.

Khách bấm 1 chỉ xác nhận tiếp tục xử lý đơn.

## 37.7. Với Warehouse / Delivery

Warehouse / Delivery owner sở hữu:

- Xuất kho.

- Đóng gói giao hàng.

- Bàn giao vận chuyển.

- Trạng thái giao hàng.

- COD success nếu thuộc delivery flow.

- Delivery fail.

- Return.

- Fulfillment issue.

IVR không được:

- Cho xuất kho trực tiếp.

- Cho giao hàng trực tiếp.

- Đánh dấu delivery success.

- Đánh dấu COD success.

- Tạo shipment.

- Tác động warehouse ledger.

- Tác động inventory.

- Tác động lô/batch/QR/truy vết.

## 37.8. Với MISA / Accounting

MISA / Accounting thuộc PACK-04 / Finance.

IVR không được:

- Tạo chứng từ kế toán.

- Sync MISA.

- Đối chiếu MISA.

- Tạo doanh thu kế toán.

- Xác nhận công nợ.

- Tạo accounting event.

- Ghi nhận thanh toán.

- Ghi nhận hóa đơn.

- Tác động báo cáo tài chính.

Nếu sau này cần dùng IVR result làm evidence hỗ trợ kiểm soát đơn, phải qua owner policy, không sync trực tiếp MISA.

## 37.9. Với Traceability / Recall

Traceability / Recall thuộc Operational Core.

IVR không được:

- Tác động lô sản xuất.

- Tác động batch.

- Tác động QR.

- Tác động public trace.

- Tác động recall.

- Tác động sale lock.

- Tác động recovery/CAPA.

- Tác động warehouse trace.

Nếu đơn bị recall/sale lock, Core/Operational owner quyết định, không phải IVR.

## 37.10. Với PACK-07 Ads / ROAS

PACK-07 có thể đo IVR như một evidence/risk signal sau khi PACK-09 release.

Nhưng PACK-07 không được:

- Dùng IVR result khi IVR chưa release.

- Dùng simulated IVR result như production evidence.

- Gọi IVR_CONFIRMED là verified revenue.

- Gọi IVR_CONFIRMED là paid revenue.

- Gọi IVR_CONFIRMED là delivered revenue.

- Gọi IVR_NO_ANSWER_FINAL là cancel final nếu Core chưa công nhận.

- Tính ROAS dựa trên IVR result thay verified revenue.

IVR chỉ hỗ trợ order confirmation evidence, không thay revenue verification.

## 38. IMPLEMENTATION BOUNDARY

## 38.1. Trạng thái triển khai

## PRODUCTION_IMPLEMENTATION_ALLOWED = KHÔNG

Ý nghĩa:

- Chưa triển khai production.

- Chưa gọi khách thật.

- Chưa cập nhật đơn thật.

- Chưa hủy đơn thật bằng IVR.

- Chưa cho downstream phụ thuộc IVR.

- Chưa release SIM Gateway production.

## 38.2. Được phép chuẩn bị kỹ thuật nội bộ

Đội kỹ thuật được phép chuẩn bị:

- Khảo sát SIM Gateway.

- Test server kết nối SIM Gateway.

- Test số nội bộ.

- Test DTMF nội bộ.

- Draft skeleton backend.

- Draft Admin UI.

- Draft test matrix.

- Test call scheduler nội bộ.

- Test call result normalizer nội bộ.

- Test audit/evidence writer nội bộ.

- Test sim health monitor nội bộ.

- Test capacity incident monitor nội bộ.

- Test notification owner handoff bằng dữ liệu giả lập.

## 38.3. Các hành động bị cấm trước release

Trước release, bắt buộc khóa:

## REAL_CUSTOMER_CALL_ALLOWED = KHÔNG

## REAL_ORDER_STATE_UPDATE_ALLOWED = KHÔNG

## PRODUCTION_SIM_GATEWAY_ALLOWED = KHÔNG

## PRODUCTION_ORDER_CANCEL_BY_IVR_ALLOWED = KHÔNG

## POST_IVR_CANCELLATION_NOTICE_ALLOWED_IN_PRODUCTION = KHÔNG

Không được:

- Gọi khách thật.

- Gọi số khách thật.

- Dùng đơn thật để test gọi.

- Hủy đơn thật bằng IVR.

- Cập nhật order state thật.

- Cho Admin thao tác IVR production.

- Cho AI/FB/CRM gọi IVR.

- Cho downstream dùng IVR result.

- Cho Notification gửi cancellation notice production.

- Cho Completion Report PASS dựa trên IVR chưa có evidence.

## 39. ENGINEER HANDOFF MODEL

## 39.1. Nguyên tắc handoff kỹ sư

PACK-09 cần phân tách rõ nhiệm vụ để tránh một người hoặc một module vượt quyền.

Không để kỹ sư SIM Gateway tự quyết định order state.

Không để kỹ sư Admin UI tạo nút hủy đơn ngoài Core.

Không để kỹ sư backend thêm phím mới ngoài rule.

Không để kỹ sư monitoring sửa IVR result giả.

Không để kỹ sư notification gửi tin nhắn hủy đơn trước khi Core hủy.

Không để kỹ sư nào mở production khi chưa đủ gate.

## 39.2. Kỹ sư 1 — IVR Backend / SIM Gateway

Kỹ sư 1 phụ trách lớp IVR Backend / SIM Gateway.

Phạm vi phụ trách:

- SimChannelManager.

- CallJobQueue.

- DeadlineAwareScheduler.

- CallExecutionAdapter.

- DtmfCaptureHandler.

- CallResultNormalizer.

- OrderStateMachineAdapter.

- AuditEvidenceWriter.

- CapacityIncidentMonitor.

- SimHealthMonitor.

## 39.3. Kỹ sư 1 không được làm

Kỹ sư 1 không được:

- Gọi khách thật khi chưa release.

- Tự hủy đơn trực tiếp.

- Tự cập nhật order state.

- Tự mở API production.

- Tự đổi rule gọi.

- Tự thêm phím 9.

- Tự thêm menu IVR mới.

- Tính lỗi kỹ thuật là khách không nghe.

- Gọi ngoài confirmation window.

- Assign nhiều cuộc vào một SIM.

- Batch cuối phiên.

- Tự bỏ qua evidence writer.

- Tự cho downstream dùng IVR result.

- Tự sửa result để test PASS.

- Tự gửi cancellation notice.

## 39.4. Kỹ sư 2 — Admin UI / Monitoring / Evidence / Test

Kỹ sư 2 phụ trách lớp Admin UI / Monitoring / Evidence / Test.

Phạm vi phụ trách:

- /admin/ivr/dashboard.

- /admin/ivr/call-jobs.

- /admin/ivr/call-jobs/{id}.

- /admin/ivr/sim-channels.

- /admin/ivr/capacity-incidents.

- /admin/ivr/audit-evidence.

- Monitoring status.

- Evidence review view.

- Test matrix view nếu có.

- Smoke result display nếu có.

- Post-IVR cancellation notice evidence view.

## 39.5. Kỹ sư 2 không được làm

Kỹ sư 2 không được:

- Cho Admin sửa IVR result giả.

- Cho Admin hủy đơn ngoài Core State Machine.

- Hiển thị dữ liệu nhạy cảm không cần thiết.

- Tạo button gọi lại ngoài retry rule.

- Tạo action chưa có permission.

- Cho phép manual retry sau window nếu không có owner rule.

- Cho phép fake IVR_CONFIRMED.

- Cho phép fake IVR_CUSTOMER_CANCELLED.

- Cho phép xóa evidence lỗi.

- Cho phép sửa call result history.

- Cho phép ẩn capacity incident.

- Cho phép ẩn technical exception.

- Cho phép bật production từ UI khi gate bị chặn.

- Cho phép fake post-cancel notice sent.

## 39.6. Phân tách trách nhiệm giữa hai kỹ sư

Hạng mục | Kỹ sư 1 — Backend/SIM | Kỹ sư 2 — Admin/Evidence

SIM channel | Thiết kế quản lý SIM | Hiển thị trạng thái

Call job | Queue/scheduler/execution | Xem job, không sửa result giả

DTMF | Capture/normalize | Hiển thị kết quả

Result | Normalize, gửi Core signal | Audit/evidence view

Core boundary | Adapter gửi signal | Không hủy đơn ngoài Core

Capacity | Monitor incident | Dashboard incident

Evidence | Ghi evidence | Hiển thị/review evidence

Retry | Theo rule scheduler | Không tạo retry ngoài rule

Notification | Không tự gửi notice | Hiển thị evidence notice

Production gate | Không tự mở | Không tạo nút bypass gate

## 40. ADMIN UI / MONITORING GOVERNANCE Ở MỨC PACK

## 40.1. Admin UI chỉ để quan sát và review

Admin UI trong PACK-09 dùng để:

- Xem dashboard IVR.

- Xem call job.

- Xem attempt.

- Xem SIM channel.

- Xem capacity incident.

- Xem technical exception.

- Xem audit/evidence.

- Xem trạng thái handoff Core.

- Xem smoke/test result nếu có.

- Xem trạng thái post-cancel notice nếu có.

Admin UI không phải nơi sửa sự thật IVR.

Admin UI không phải nơi hủy đơn trực tiếp.

Admin UI không phải nơi giả lập xác nhận khách.

Admin UI không phải nơi tự gửi tin nhắn hủy đơn.

## 40.2. Admin action phải có permission

Nếu Admin UI có action, action phải có permission.

Các action nhạy cảm cần kiểm soát:

- Mark Admin Review.

- Request manual review.

- Disable SIM channel.

- Re-enable SIM channel.

- Acknowledge capacity incident.

- Acknowledge technical exception.

- Request retest.

- Close evidence review.

- Export audit.

- Escalate owner review.

- Review notification evidence.

Không có permission thì không được thao tác.

## 40.3. Admin không được thấy dữ liệu không cần thiết

Admin UI IVR không nên hiển thị dữ liệu nhạy cảm không cần cho xác nhận cuộc gọi.

Không hiển thị nếu không cần:

- Full address.

- Full customer profile.

- Member tier.

- Diamond referral info.

- Health/sensitive note.

- AI consultation content.

- CRM content.

- Payment detail.

- Order history toàn bộ.

- MISA/accounting data.

Admin UI phải tuân thủ data minimization.

## 41. AUDIT / EVIDENCE GOVERNANCE Ở MỨC PACK

## 41.1. Evidence bắt buộc

Mỗi call job production sau này phải có evidence.

Evidence tối thiểu:

- Order reference.

- Call job reference.

- Program.

- T0.

- Confirmation window.

- Attempt number.

- SIM channel.

- Call start time.

- Call end time.

- Call result raw nếu được phép lưu.

- Normalized IVR result.

- DTMF key nếu có.

- Technical exception nếu có.

- Capacity incident nếu có.

- Core signal sent status.

- Core response status.

- Post-cancel notice trigger nếu có.

- Notification owner event nếu có.

- Audit timestamp.

- Correlation ID.

## 41.2. Evidence không được sửa/xóa tùy tiện

Evidence phải append-only hoặc có version/audit nếu điều chỉnh metadata.

Không được:

- Xóa evidence lỗi.

- Sửa result cho đẹp.

- Sửa DTMF key.

- Sửa timestamp.

- Sửa SIM channel.

- Sửa technical exception thành no answer.

- Sửa capacity incident thành khách không nghe.

- Xóa duplicate result.

- Xóa failed smoke.

- Xóa rejected evidence.

- Sửa notice chưa gửi thành đã gửi.

- Sửa notice bị chặn thành đã gửi.

Nếu cần correction, phải tạo correction record, không sửa mất lịch sử.

## 41.3. Evidence cho Core signal

Khi gửi result sang Core, phải có evidence:

- IVR result.

- Order reference.

- Call job reference.

- Result final hay temporary.

- Technical exception flag.

- Capacity incident flag.

- Duplicate bị chặn flag.

- Timestamp.

- Correlation ID.

- Core response.

Không có Core response evidence thì không được gọi signal đã xử lý thành công.

## 41.4. Evidence cho Post-IVR cancellation notice

Khi gửi tin nhắn giải thích hủy đơn, phải có evidence:

- Core cancellation evidence.

- Cancel reason.

- Notification owner event.

- Message template used.

- Channel used.

- Timestamp.

- Delivery status nếu có.

- Audit/correlation ID.

- Block reason nếu không gửi.

Không có evidence thì không được gọi notice đã gửi.

## 42. SECURITY / PRIVACY GOVERNANCE

## 42.1. IVR phải bảo vệ dữ liệu khách

PACK-09 phải bảo vệ:

- Số điện thoại.

- Mã đơn.

- Tổng thanh toán.

- Nội dung call result.

- DTMF input.

- Lịch sử gọi.

- Technical exception.

- Admin review.

- Evidence.

- Notification evidence.

Không hiển thị dữ liệu vượt nhu cầu.

Không xuất dữ liệu không có quyền.

Không dùng IVR data cho marketing.

## 42.2. Không dùng IVR cho mục đích ngoài xác nhận đơn

IVR data không được dùng cho:

- CRM chăm sóc.

- Retargeting.

- Ads audience.

- Diamond recruitment.

- Member lifecycle.

- Survey.

- Upsell.

- Cross-sell.

- Product recommendation.

- Health profiling.

PACK-09 chỉ phục vụ xác nhận đơn.

## 43. DONE CONDITION CỦA PHẦN 3/4

## PHẦN 3/4 được xem là hoàn thành về mặt tài liệu khi đã khóa được:

- Call Result Governance.

- IVR result là input signal only.

- Core Order State Machine là final decision layer.

- IVR_CONFIRMED.

- IVR_CUSTOMER_CANCELLED.

- IVR_NO_ANSWER_FINAL theo program.

- IVR_CONFIRMATION_WINDOW_EXPIRED.

- IVR_INVALID_PHONE_FINAL.

- IVR_CAPACITY_EXCEPTION.

- Post-IVR cancellation notice result.

- Duplicate IVR result handling.

- Core Order State Machine Boundary.

- Technical Exception Governance.

- SIM Gateway error.

- Server error.

- DTMF capture error.

- Audio playback error.

- SIM channel failure.

- Scheduler error.

- Technical exception final.

- Capacity baseline 5 phút.

- Capacity baseline 15 phút.

- Capacity baseline 45 phút.

- Khuyến nghị cấu hình SIM.

- Capacity incident governance.

- Scheduler governance.

- Cross-pack boundary với Operational Core.

- Cross-pack boundary với AI Advisor.

- Cross-pack boundary với Facebook/Messenger/Live.

- Cross-pack boundary với CRM.

- Cross-pack boundary với Notification owner.

- Cross-pack boundary với Payment.

- Cross-pack boundary với Warehouse/Delivery.

- Cross-pack boundary với MISA.

- Cross-pack boundary với PACK-07.

- Implementation boundary.

- Engineer handoff model.

- Admin UI / Monitoring Governance.

- Audit / Evidence Governance.

- Security / Privacy Governance.

## PHẦN 3/4 chưa viết P0 Rules, Smoke Matrix, Done Gate tổng thể, Fail Gate tổng thể, Evidence Acceptance, Release Control và Final Conclusion. Các nội dung đó thuộc PHẦN 4/4.

## KẾT LUẬN PHẦN 3/4

PACK-09 chỉ tạo IVR result.

IVR result chỉ là input signal.

Core Order State Machine mới là final decision layer.

SIM Gateway không được cập nhật order trực tiếp.

Call Result Normalizer chỉ chuẩn hóa kết quả, không đổi trạng thái đơn.

Technical exception không phải khách không nghe.

Capacity incident không phải khách không nghe.

Một SIM chỉ có một active outbound call.

Năng lực IVR phải tính theo số SIM vật lý và chu kỳ 50 giây/cuộc/SIM.

Trong 5 phút, 12 SIM xử lý khoảng 72 cuộc, 24 SIM khoảng 144 cuộc, 32 SIM khoảng 192 cuộc.

Trong 15 phút, 12 SIM xử lý khoảng 216 cuộc, 24 SIM khoảng 432 cuộc, 32 SIM khoảng 576 cuộc.

Trong 45 phút rolling queue, 12 SIM xử lý khoảng 648 cuộc, 24 SIM khoảng 1.296 cuộc, 32 SIM khoảng 1.728 cuộc.

Scheduler bắt buộc là deadline-aware rolling queue.

Batch cuối phiên bị cấm.

AI Advisor không được tạo IVR call job.

Facebook Gateway / Messenger / Live không được gọi IVR trực tiếp.

CRM không được dùng IVR cho chăm sóc, nhắc mua lại hoặc marketing.

Notification owner là bên duy nhất được gửi post-IVR cancellation notice sau khi Core đã hủy đơn chính thức.

Payment không được dùng IVR để xác nhận paid.

Warehouse/Delivery không được dùng IVR để cho xuất kho/giao hàng.

MISA không được dùng IVR để ghi nhận kế toán.

PACK-07 không được dùng IVR result như verified revenue.

Nguyên tắc khóa của PHẦN 3/4 là:

IVR result không phải order state.

Core mới quyết định đơn.

Technical error không phải No Answer.

Capacity overload không phải lỗi khách.

SIM Gateway không được hủy đơn.

Admin không được sửa IVR result giả.

Notification owner chỉ gửi notice sau Core cancellation.

Engineer không được mở production khi gate bị chặn.

Không có audit/evidence thì không PASS.

## PHẦN 4/4 — P0 RULES / SMOKE MATRIX / IVR EVIDENCE / DONE GATE / FAIL GATE / RELEASE CONTROL / FINAL CONCLUSION

## 44. MỤC TIÊU CỦA PHẦN 4/4

## PHẦN 4/4 thiết lập lớp kiểm soát cuối cho PACK-09, bao gồm P0 Rules, Smoke Matrix, IVR Evidence, Done Gate, Fail Gate, Release Control và kết luận cuối.

Mục tiêu của phần này là khóa rõ:

- Rule nào là P0 bắt buộc.

- Rule nào tuyệt đối không được vi phạm.

- Điều kiện nào làm PACK-09 fail.

- Smoke nào bắt buộc trước khi release.

- Evidence nào phải có.

- Khi nào IVR Governance được xem là done.

- Khi nào IVR Implementation được xem là done.

- Khi nào IVR được phép production.

- Khi nào IVR bị bị chặn.

- Khi nào Completion Report không được PASS.

- Khi nào Gateway / Production / Go-live vẫn phải giữ bị chặn.

- Cách handoff PACK-09 sang bộ file IVR-00 -> IVR-09.

## PHẦN 4/4 không thiết kế code, API, database hoặc UI chi tiết.

## PHẦN 4/4 là lớp governance cuối để ngăn việc triển khai IVR bằng cách “làm cho có”, thiếu evidence, thiếu smoke hoặc vượt ranh giới Core Order State Machine.

## 45. P0 RULE REGISTRY

## 45.1. Nguyên tắc P0

P0 Rule là rule bắt buộc.

P0 Rule không phải khuyến nghị.

P0 Rule không được override tùy tiện.

P0 Rule không được bỏ qua để chạy nhanh.

P0 Rule không được xử lý bằng lời hứa “sẽ sửa sau”.

Nếu vi phạm P0, PACK-09 phải chuyển trạng thái:

## 45.2. P0-01 — Internal SIM Gateway

## MUST

PACK-09 dùng mô hình:

## INTERNAL_SIM_GATEWAY_SERVER

Hệ thống dùng SIM Gateway nội bộ, SIM vật lý, server nội bộ, call scheduler nội bộ, DTMF capture nội bộ, result normalizer nội bộ và audit/evidence nội bộ.

## MUST NOT

Không mặc định dùng:

- Cloud IVR.

- SIP Trunk.

- Voice Brandname Provider.

- Telecom Provider Managed IVR.

- Third-party calling platform.

- External marketing call center.

## FAIL IF

- Tài liệu hoặc triển khai lấy provider bên ngoài làm mặc định khi chưa có quyết định riêng.

- Dev tự chọn Cloud IVR vì dễ làm hơn.

- Provider ngoài được gắn vào production mà chưa có security review.

- Call result từ provider ngoài được coi là source-of-truth khi chưa có owner approval.

Evidence bắt buộc

- SIM Gateway deployment decision.

- Internal SIM Gateway test evidence.

- SIM channel list.

- Server internal routing evidence.

- Owner approval cho mô hình triển khai.

## 45.3. P0-02 — One SIM One Active Call

## MUST

Một SIM chỉ xử lý một cuộc gọi outbound đang active tại một thời điểm.

## 1 SIM = 1 ACTIVE OUTBOUND CALL

## MUST NOT

- Không assign nhiều call job vào cùng một SIM đang bận.

- Không tính một SIM gọi song song nhiều cuộc.

- Không giả định SIM Gateway có thể vượt giới hạn vật lý.

- Không tính capacity theo số call job trong queue.

## FAIL IF

- Scheduler giao trùng SIM channel.

- Một SIM có 2 active outbound call.

- Dashboard báo capacity cao hơn số SIM vật lý hợp lệ.

- Call result lẫn giữa hai cuộc gọi cùng SIM.

Evidence bắt buộc

- SIM channel state log.

- Active call tracking.

- Scheduler assignment log.

- SimHealthMonitor evidence.

- Duplicate assignment test evidence.

## 45.4. P0-03 — Program Window

## MUST

Giờ Vàng:

- Confirmation window = 5 phút.

- Max attempt = 2.

- Attempt 1 = T0.

- Attempt 2 = T0 + 5 phút.

- Window expires = T0 + 5 phút.

24/7:

- Confirmation window = 15 phút.

- Max attempt = 3.

- Attempt 1 = T0.

- Attempt 2 = T0 + 5 phút.

- Attempt 3 = T0 + 10 phút.

- Window expires = T0 + 15 phút.

## MUST NOT

- Không gọi dồn liên tục.

- Không gọi attempt sai thời điểm.

- Không gọi sau khi hết window.

- Không tự gia hạn window.

- Không tạo attempt vượt số lần đã khóa theo program.

- Không reset window bằng thao tác Admin nếu không có owner-approved rule.

- Không gửi tin nhắn hủy đơn trước khi Core Order State Machine hủy đơn chính thức.

## FAIL IF

- Giờ Vàng attempt 2 không ở T0 + 5 phút.

- Giờ Vàng có attempt 3.

- 24/7 attempt 2 không ở T0 + 5 phút.

- 24/7 attempt 3 không ở T0 + 10 phút.

- 24/7 có attempt 4.

- Có cuộc gọi sau window expiry.

- Có tin nhắn hủy đơn được gửi trước khi Core hủy đơn.

- Technical exception bị tính là no answer.

- Capacity incident bị tính là no answer.

Evidence bắt buộc

- T0.

- Program type.

- Attempt 1 timestamp.

- Attempt 2 timestamp.

- Attempt 3 timestamp nếu program là TWENTY_FOUR_SEVEN.

- Window expiry timestamp.

- Scheduler decision log.

- Final IVR result.

- Core response.

- Notification evidence nếu có post-IVR cancellation notice.

## 45.5. P0-04 — Key Mapping

## MUST

DTMF mapping khóa:

- Phím 1 = CONFIRM_ORDER.

- Phím 0 = CANCEL_ORDER_BY_CUSTOMER.

## MUST NOT

- Không tự thêm phím 9.

- Không tự thêm phím hỗ trợ người thật.

- Không tự thêm menu nhiều tầng.

- Không tự thêm phím upsell.

- Không tự thêm phím khảo sát.

- Không tự thêm phím CRM.

## FAIL IF

- DTMF mapping sai rule.

- Phím 0 bị hiểu thành xác nhận.

- Phím 1 bị hiểu thành hủy.

- Phím 9 được bật khi rule đang khóa NOT_ENABLED.

- Sai phím được tự động hiểu thành xác nhận/hủy.

Evidence bắt buộc

- DTMF test result.

- Key 1 test.

- Key 0 test.

- Invalid key test.

- No input test.

- DTMF error test.

- Normalized result evidence.

## 45.6. P0-05 — Core State Machine

## MUST

Mọi hủy đơn hoặc tiếp tục xử lý đơn phải đi qua Core Order State Machine.

## MUST NOT

- SIM Gateway không cập nhật order trực tiếp.

- IVR callback không đổi order state trực tiếp.

- Admin UI IVR không hủy đơn ngoài Core.

- Call Result Normalizer không release order.

- Scheduler không hủy order.

- Notification không được gửi tin nhắn hủy nếu chưa có Core cancellation.

## FAIL IF

- SIM Gateway đổi order state trực tiếp.

- IVR callback đổi order state trực tiếp.

- Admin UI tạo nút hủy đơn không qua Core.

- IVR_CONFIRMED tự chuyển order sang trạng thái xử lý tiếp.

- IVR_CUSTOMER_CANCELLED tự hủy order ngoài Core.

- IVR_NO_ANSWER_FINAL tự hủy order ngoài Core.

- Notification gửi tin nhắn hủy trước khi Core hủy đơn.

Evidence bắt buộc

- Core signal evidence.

- Core response evidence.

- State transition log.

- Order state before/after.

- Reason code.

- Audit correlation.

- Notification trigger evidence nếu có.

## 45.7. P0-06 — Technical Error Boundary

## MUST

Lỗi kỹ thuật đi vào:

## IVR_TECHNICAL_EXCEPTION

## ADMIN_REVIEW_REQUIRED

## MUST NOT

- Không tính lỗi kỹ thuật là khách không nghe.

- Không hủy đơn vì SIM lỗi.

- Không hủy đơn vì server lỗi.

- Không hủy đơn vì DTMF lỗi.

- Không hủy đơn vì audio playback lỗi.

- Không hủy đơn vì scheduler lỗi.

- Không hủy đơn vì callback lỗi.

- Không gửi cancellation notice do lỗi kỹ thuật.

## FAIL IF

- SIM lỗi nhưng order bị cancel bằng IVR_NO_ANSWER_AFTER_PROGRAM_ATTEMPTS.

- DTMF_CAPTURE_ERROR bị tính là NO_INPUT.

- AUDIO_PLAYBACK_ERROR bị tính là khách không nghe.

- SCHEDULER_ERROR bị che giấu.

- TECHNICAL_EXCEPTION không vào Admin Review.

- Technical exception kích hoạt post-IVR cancellation notice.

Evidence bắt buộc

- Technical error log.

- Error type.

- Attempt context.

- Result normalizer output.

- Admin Review status.

- Core handling evidence.

- Notice bị chặn evidence nếu có.

## 45.8. P0-07 — Rolling Queue

## MUST

Dùng:

## DEADLINE_AWARE_ROLLING_QUEUE

## MUST NOT

- Không batch toàn bộ cuộc gọi cuối phiên.

- Không dồn tất cả cuộc gọi sau live.

- Không gọi ngoài window.

- Không bỏ qua deadline.

- Không assign job theo FIFO mù nếu deadline khác nhau.

- Không để queue backlog bị che giấu.

- Không giả lập result khi không gọi kịp.

## FAIL IF

- 32 SIM bị giao xử lý 800–1.200 cuộc trong 5 phút.

- Scheduler dồn call cuối Giờ Vàng.

- Attempt quá hạn nhưng vẫn gọi.

- Hệ thống giả lập gọi thành công khi SIM không đủ.

- Capacity incident không được tạo.

- 24/7 attempt 3 bị batch cuối window sai rule.

Evidence bắt buộc

- Queue snapshot.

- Deadline calculation.

- SIM availability.

- Attempt schedule.

- Backlog count.

- Capacity incident evidence.

- Expired job evidence.

## 45.9. P0-08 — No Downstream Dependency

## MUST

Downstream chưa được phụ thuộc IVR result khi chưa release.

## MUST NOT

Các module sau không được dùng IVR result như production data khi IVR chưa pass test/smoke/evidence:

- Order.

- CRM.

- AI Advisor.

- Facebook Gateway.

- Messenger.

- Live.

- Admin.

- Warehouse.

- Payment.

- MISA.

- PACK-07 Ads/ROAS.

- Notification.

- Completion Report.

## FAIL IF

- Có module dùng IVR result trước khi IVR test/smoke/evidence pass.

- AI nói đơn đã IVR xác nhận khi Core chưa công nhận.

- Warehouse cho xuất kho theo raw IVR result.

- PACK-07 dùng IVR result chưa release làm evidence ROAS.

- Admin hiển thị IVR simulation như production truth.

- Notification gửi notice production khi IVR/Order gate chưa cho phép.

Evidence bắt buộc

- Dependency review.

- Feature flag status.

- Downstream usage check.

- Integration smoke.

- Release gate evidence.

## 45.10. P0-09 — No Release Without Evidence

## MUST

Test, smoke, security và evidence là bắt buộc.

## MUST NOT

- Không release bằng lời xác nhận miệng.

- Không release vì “dev nói chạy được”.

- Không release vì demo nội bộ một lần.

- Không Completion Report PASS nếu thiếu evidence.

- Không Production Ready nếu evidence waiting.

- Không Owner sign-off nếu smoke fail chưa retest.

## FAIL IF

- Completion Report PASS nhưng thiếu evidence.

- Production enabled nhưng evidence rejected.

- Release decision không có audit.

- Security/privacy chưa kiểm tra nhưng go-live.

Evidence bắt buộc

- Test result.

- Smoke result.

- Security review.

- Evidence accepted.

- Owner sign-off.

- Release decision.

## 45.11. P0-10 — Post-IVR Cancellation Notice

## MUST

Tin nhắn giải thích hủy đơn chỉ được gửi sau khi Core Order State Machine đã hủy đơn chính thức.

Tin nhắn này là:

## TRANSACTIONAL_ORDER_STATUS_NOTICE

Notification owner là bên phát hành tin nhắn.

## MUST NOT

- SIM Gateway không được tự gửi tin nhắn.

- AI Advisor không được tự gửi tin nhắn.

- CRM marketing không được gửi tin nhắn này như chăm sóc hoặc upsell.

- Facebook Gateway không được tự quyết định gửi tin nhắn này.

- Không gửi tin nhắn sau từng attempt không nghe.

- Không gửi tin nhắn khi lỗi kỹ thuật.

- Không gửi tin nhắn khi capacity incident.

- Không gửi tin nhắn nếu Core chưa hủy đơn.

- Không chứa full address.

- Không chứa payment detail.

- Không chứa member tier.

- Không chứa Diamond/referral info.

- Không chứa health/sensitive note.

- Không cam kết giữ giá cũ khi quote/window đã hết.

## FAIL IF

- Có tin nhắn hủy đơn gửi sau attempt 1 hoặc attempt 2 khi chưa có Core cancellation.

- SIM Gateway tự gửi tin nhắn.

- AI Advisor tự phát tin nhắn.

- CRM dùng tin nhắn này để marketing.

- Facebook Gateway tự phát tin nhắn khi chưa có Notification owner event.

- Tin nhắn chứa dữ liệu bị cấm.

- Tin nhắn được gửi do IVR_TECHNICAL_EXCEPTION.

- Tin nhắn được gửi do IVR_CAPACITY_EXCEPTION.

- Tin nhắn được gửi khi order chưa có trạng thái cancelled từ Core.

Evidence bắt buộc

- Core cancellation evidence.

- Cancel reason.

- Notification owner event.

- Message template used.

- Channel used.

- Timestamp.

- Delivery status nếu có.

- Audit/correlation ID.

## 46. SMOKE GOVERNANCE PRINCIPLES

## 46.1. Smoke không phải chạy thử hình thức

Smoke PACK-09 phải chứng minh IVR vận hành đúng nghiệp vụ.

Smoke không chỉ kiểm tra gọi được một cuộc.

Smoke phải chứng minh:

- Gọi đúng order đủ điều kiện.

- Không gọi order chưa đủ điều kiện.

- Phím 1 đúng.

- Phím 0 đúng.

- Không bấm phím đúng rule.

- Sai phím đúng rule.

- Lỗi DTMF đúng technical exception.

- Giờ Vàng đúng 10 phút.

- 24/7 đúng 15 phút với 3 attempts.

- Scheduler không batch cuối phiên.

- Một SIM chỉ một active call.

- Technical error không thành no answer.

- Capacity incident không thành lỗi khách.

- Core Order State Machine là final decision.

- Evidence ghi đầy đủ.

- Downstream không phụ thuộc khi chưa release.

- Post-IVR cancellation notice chỉ gửi sau khi Core đã hủy đơn.

- Technical exception không kích hoạt cancellation notice.

- Capacity incident không kích hoạt cancellation notice.

## 46.2. Smoke phải có positive path và negative path

Positive path chứng minh luồng đúng chạy được.

Negative path chứng minh hệ thống chặn đúng.

PACK-09 không đủ Done Gate nếu chỉ test khách bấm 1.

Bắt buộc test:

- Khách bấm 1.

- Khách bấm 0.

- Không nghe cuộc 1, nghe cuộc sau.

- Không nghe đủ attempts theo program: Giờ Vàng 2 cuộc, 24/7 3 cuộc.

- Hết window.

- Sai phím.

- DTMF lỗi.

- SIM lỗi.

- Scheduler quá tải.

- Core unavailable.

- Duplicate call job.

- Admin cố sửa result.

- Downstream cố consume IVR result khi chưa release.

- Post-IVR cancellation notice chỉ gửi sau khi Core đã hủy đơn.

- Technical exception không kích hoạt cancellation notice.

- Capacity incident không kích hoạt cancellation notice.

## 47. SMOKE MATRIX TỔNG THỂ

## 47.1. IVR-SMK-001 — Internal SIM Gateway Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-001

Mục tiêu | Kiểm tra mô hình Internal SIM Gateway Server

Input | SIM Gateway nội bộ, SIM vật lý, server nội bộ

Expected result | Hệ thống gọi qua Internal SIM Gateway, không dùng provider ngoài làm mặc định

Negative path | Cấu hình Cloud IVR / SIP / Voice Brandname làm default

Fail condition | Provider ngoài được dùng làm default khi chưa owner decision

Evidence | Deployment model snapshot, SIM Gateway test, owner approval

## 47.2. IVR-SMK-002 — One SIM One Active Call Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-002

Mục tiêu | Kiểm tra 1 SIM chỉ có 1 active outbound call

Input | Nhiều call job đồng thời

Expected result | Scheduler chỉ assign mỗi SIM một cuộc active

Negative path | Cố assign 2 call vào cùng SIM

Fail condition | Một SIM có nhiều active call

Evidence | SIM channel log, assignment log, active call monitor

## 47.3. IVR-SMK-003 — Call Script Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-003

Mục tiêu | Kiểm tra call script đúng nội dung chuẩn

Input | Order có order_code_short và total_amount_display

Expected result | IVR đọc đúng script, không thêm nội dung marketing/CRM

Negative path | Script đọc địa chỉ, member tier, Diamond, payment detail

Fail condition | IVR đọc thông tin không được phép

Evidence | Script version, audio playback log, variable render evidence

## 47.4. IVR-SMK-004 — Allowed Variables Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-004

Mục tiêu | Kiểm tra chỉ biến được phép được dùng

Input | Order có nhiều dữ liệu khách hàng

Expected result | Chỉ dùng order_code_short, total_amount_display, optional đúng policy

Negative path | Full address, payment detail, health note xuất hiện trong script

Fail condition | Script render biến bị cấm

Evidence | Variable render log, data minimization result

## 47.5. IVR-SMK-005 — DTMF Key 1 Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-005

Mục tiêu | Kiểm tra phím 1 xác nhận đơn

Input | Khách nghe và bấm 1

Expected result | IVR_CONFIRMED, gửi signal về Core

Negative path | Phím 1 bị map sai hoặc đổi order trực tiếp

Fail condition | SIM Gateway tự chuyển order state

Evidence | DTMF log, normalized result, Core response

## 47.6. IVR-SMK-006 — DTMF Key 0 Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-006

Mục tiêu | Kiểm tra phím 0 là khách không đặt/muốn hủy

Input | Khách nghe và bấm 0

Expected result | IVR_CUSTOMER_CANCELLED, Core xử lý hủy qua state machine

Negative path | SIM Gateway tự hủy order

Fail condition | Order bị hủy ngoài Core

Evidence | DTMF log, cancel signal, Core transition evidence

## 47.7. IVR-SMK-007 — Key 9 Not Enabled Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-007

Mục tiêu | Kiểm tra phím 9 chưa được bật

Input | Khách bấm 9

Expected result | Invalid input / no valid confirmation theo rule

Negative path | Hệ thống chuyển human support bằng phím 9

Fail condition | Phím 9 được enable khi rule đang NOT_ENABLED

Evidence | DTMF mapping evidence, invalid key result

## 47.8. IVR-SMK-008 — No Input / Invalid Input Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-008

Mục tiêu | Kiểm tra không bấm/sai phím không thành confirm/cancel

Input | Không bấm phím hoặc bấm phím ngoài 1/0

Expected result | No valid input, xử lý theo attempt/window

Negative path | Sai phím được hiểu thành xác nhận

Fail condition | Invalid input tạo order action

Evidence | DTMF event, normalized result, scheduler next step

## 47.9. IVR-SMK-009 — Golden Hour Window Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-009

Mục tiêu | Kiểm tra Giờ Vàng 10 phút, 2 attempts, cách 5 phút

Input | Order thuộc GOLDEN_HOUR

Expected result | Attempt 1 tại T0, attempt 2 tại T0 + 5:00, hết T0 + 10:00

Negative path | Attempt 2 sai thời gian, gọi sau window 10 phút, có attempt 3

Fail condition | Xác nhận sau window vẫn được coi hợp lệ; Giờ Vàng có attempt 3

Evidence | Attempt timeline, window expiry, Core action

## 47.10. IVR-SMK-010 — 24/7 Window Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-010

Mục tiêu | Kiểm tra 24/7 15 phút, 3 attempts, lịch gọi T0 / T0 + 5 phút / T0 + 10 phút

Input | Order thuộc TWENTY_FOUR_SEVEN

Expected result | Attempt 1 tại T0, attempt 2 tại T0 + 5 phút, attempt 3 tại T0 + 10 phút, hết window tại T0 + 15 phút

Negative path | Attempt 2 sai thời gian, attempt 3 sai thời gian, gọi ngoài 15 phút, tạo attempt 4

Fail condition | Gọi sau window nhưng vẫn coi xác nhận hợp lệ; 24/7 không tạo attempt 3 đúng rule; tạo attempt 4

Evidence | Attempt timeline, window expiry, Core action, scheduler decision log

## 47.11. IVR-SMK-011 — No Answer After Program Attempts Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-011

Mục tiêu | Kiểm tra không nghe đủ attempts theo program tạo IVR_NO_ANSWER_FINAL

Expected result | IVR_NO_ANSWER_FINAL, Core xử lý theo program-specific reason code

Negative path | Một trong các attempts là technical error hoặc capacity incident

Fail condition | Technical error/capacity incident bị tính là no answer; 24/7 không test attempt 3

Evidence | Attempt logs, result normalizer, Core reason code, Core cancellation evidence

## 47.12. IVR-SMK-012 — Technical Exception Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-012

Mục tiêu | Kiểm tra lỗi kỹ thuật đi vào IVR_TECHNICAL_EXCEPTION

Input | SIM_GATEWAY_ERROR, DTMF_CAPTURE_ERROR, AUDIO_PLAYBACK_ERROR

Expected result | IVR_TECHNICAL_EXCEPTION, ADMIN_REVIEW_REQUIRED

Negative path | Lỗi kỹ thuật bị map thành NO_ANSWER

Fail condition | Order bị hủy do lỗi kỹ thuật

Evidence | Error log, normalized result, admin review record

## 47.13. IVR-SMK-013 — Core State Machine Boundary Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-013

Mục tiêu | Kiểm tra mọi order action đi qua Core

Input | IVR_CONFIRMED / IVR_CUSTOMER_CANCELLED / IVR_NO_ANSWER_FINAL

Expected result | IVR gửi signal, Core quyết định state transition

Negative path | SIM Gateway hoặc callback đổi order trực tiếp

Fail condition | Order state thay đổi ngoài Core

Evidence | Core signal, Core transition log, audit

## 47.14. IVR-SMK-014 — Deadline-Aware Rolling Queue Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-014

Mục tiêu | Kiểm tra scheduler dùng rolling queue theo deadline

Input | Nhiều order phát sinh theo thời gian

Expected result | Job được gọi rolling real-time, không dồn cuối phiên

Negative path | Batch cuối phiên

Fail condition | Scheduler gọi hàng loạt cuối window

Evidence | Queue timeline, job schedule, deadline calculation

## 47.15. IVR-SMK-015 — Capacity Incident Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-015

Mục tiêu | Kiểm tra quá tải tạo capacity incident

Input | 32 SIM, 800–1.200 job trong 5 phút

Expected result | Capacity incident, không giả lập gọi đủ

Negative path | Hệ thống coi toàn bộ no answer hoặc called

Fail condition | Overcapacity bị che giấu

Evidence | SIM count, job count, backlog, expired job, capacity event

## 47.16. IVR-SMK-016 — Duplicate Call Job Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-016

Mục tiêu | Kiểm tra một order không có nhiều call job active

Input | Cố tạo duplicate call job cho cùng order/window

Expected result | Duplicate bị block, Admin Review nếu cần

Negative path | Hai call job cùng gọi một order

Fail condition | Duplicate final result cạnh tranh nhau

Evidence | Duplicate detection log, bị chặn result, audit

## 47.17. IVR-SMK-017 — Admin Manual Control Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-017

Mục tiêu | Kiểm tra Admin không sửa IVR result giả

Input | Admin cố mark fake confirmed/cancelled

Expected result | Action bị chặn hoặc đi qua manual review riêng có audit

Negative path | Admin sửa result trực tiếp

Fail condition | Fake IVR result được ghi như khách bấm phím

Evidence | Permission log, rejected action, audit

## 47.18. IVR-SMK-018 — Downstream Dependency Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-018

Mục tiêu | Kiểm tra downstream không consume IVR result trước release

Input | AI/FB/CRM/Admin/Warehouse/PACK-07 cố đọc IVR result production

Negative path | Downstream dùng simulated IVR result

Fail condition | Module phụ thuộc IVR result trước release

Evidence | Dependency block log, feature flag, integration test

## 47.19. IVR-SMK-019 — Evidence Write Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-019

Mục tiêu | Kiểm tra mọi call job/result có evidence

Input | Call job hoàn tất hoặc lỗi

Expected result | Evidence record có metadata đầy đủ

Negative path | Evidence writer fail

Fail condition | Result không có evidence nhưng vẫn PASS

Evidence | Evidence record, metadata, review status

## 47.20. IVR-SMK-020 — Release Gate Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-020

Mục tiêu | Kiểm tra release không được PASS khi thiếu test/smoke/security/evidence

Input | Release request

Expected result | Release bị chặn nếu thiếu accepted evidence hoặc còn P0

Negative path | Owner/developer xác nhận miệng

Evidence | Release checklist, smoke result, evidence accepted, owner sign-off

## 47.21. IVR-SMK-021 — 24/7 Three-Attempt Schedule Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-021

Mục tiêu | Kiểm tra 24/7 gọi đúng T0 / T0 + 5 phút / T0 + 10 phút

Input | Order 24/7 không có final result ở attempt 1 và attempt 2

Expected result | 24/7 có đủ attempt 3 nếu attempt 1 và attempt 2 chưa có final result

Negative path | Không tạo attempt 3 hoặc tạo attempt 3 sai thời điểm

Fail condition | Attempt 3 không đúng lịch hoặc gọi sau T0 + 15 phút

Evidence | Attempt timeline, scheduler log, window expiry

## 47.22. IVR-SMK-022 — Post-IVR Cancellation Notice Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-022

Mục tiêu | Kiểm tra tin nhắn giải thích chỉ gửi sau khi Core Order State Machine đã hủy đơn

Input | IVR_NO_ANSWER_FINAL và Core cancellation

Expected result | Core hủy đơn trước, Notification owner phát hành transactional notice sau

Negative path | Tin nhắn gửi sau từng attempt không nghe hoặc trước khi Core hủy

Fail condition | Tin nhắn gửi trước Core cancellation

Evidence | Core cancellation evidence, notification owner event, message template

## 47.23. IVR-SMK-023 — Invalid Phone vs No Answer Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-023

Mục tiêu | Phân biệt số không có thật với máy đổ chuông không nghe

Input | Case invalid phone và case ringing no answer

Expected result | Invalid phone chỉ dùng khi rule xác nhận rõ. No answer dùng khi gọi hợp lệ nhưng khách không nghe sau đủ attempts

Negative path | Một lần không nghe bị kết luận invalid phone

Fail condition | Invalid phone vẫn gửi SMS vào số không có thật

Evidence | Call result, normalized result, channel policy, notice block evidence

## 47.24. IVR-SMK-024 — Technical Exception No-Notice Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-024

Mục tiêu | Technical exception không được kích hoạt tin nhắn hủy đơn

Input | SIM/server/DTMF/audio/scheduler error

Expected result | Technical exception đi vào Admin Review, không gửi cancellation notice

Negative path | Technical exception bị map thành no answer

Fail condition | Technical exception làm gửi cancellation notice

Evidence | Technical error log, admin review, notice bị chặn evidence

## 47.25. IVR-SMK-025 — Notification Owner Boundary Smoke

Hạng mục | Nội dung

Smoke ID | IVR-SMK-025

Mục tiêu | Kiểm tra Notification owner là bên phát hành transactional notice

Input | Core cancellation trigger

Expected result | Notification owner phát hành notice; SIM Gateway, AI Advisor, CRM không tự gửi

Negative path | SIM Gateway / AI / CRM tự gửi

Fail condition | SIM Gateway, AI Advisor hoặc CRM tự gửi tin nhắn hủy đơn

Evidence | Notification event, actor/source, audit/correlation ID

## 48. IVR EVIDENCE GOVERNANCE

## 48.1. Không có evidence thì không PASS

PACK-09 không được gọi PASS nếu thiếu evidence.

Evidence bắt buộc cho:

- Internal SIM Gateway.

- SIM channel.

- Call job.

- Attempt.

- DTMF.

- Result normalizer.

- Technical exception.

- Capacity incident.

- Core signal.

- Core response.

- Admin review.

- Post-IVR cancellation notice.

- Smoke result.

- Security review.

- Release decision.

Không có evidence accepted thì không Release Ready.

## 48.2. Evidence Registry tối thiểu

PACK-09 cần Evidence Registry cho:

- IVR Deployment Evidence.

- SIM Channel Evidence.

- Call Script Evidence.

- Variable Rendering Evidence.

- DTMF Mapping Evidence.

- Golden Hour Window Evidence.

- 24/7 Window Evidence.

- Scheduler Evidence.

- Rolling Queue Evidence.

- Capacity Incident Evidence.

- Technical Exception Evidence.

- Call Result Normalization Evidence.

- Core Signal Evidence.

- Core State Transition Evidence.

- Post-IVR Cancellation Notice Evidence.

- Admin Review Evidence.

- Security/Privacy Evidence.

- Smoke Evidence.

- Release Evidence.

## 48.3. Evidence status

Evidence phải có trạng thái:

- EVIDENCE_waiting.

- EVIDENCE_SUBMITTED.

- EVIDENCE_ACCEPTED.

- EVIDENCE_REJECTED.

- EVIDENCE_NEEDS_RETEST.

- EVIDENCE_EXPIRED.

- EVIDENCE_SUPERSEDED.

- EVIDENCE_OWNER_REVIEW_REQUIRED.

Chỉ EVIDENCE_ACCEPTED mới được dùng cho PASS.

Evidence waiting không được dùng cho PASS.

Evidence rejected phải sửa và retest.

Evidence expired phải review lại.

## 48.4. Evidence metadata

Mỗi evidence record cần có:

- Evidence ID.

- Pack ID.

- Smoke ID hoặc Gate ID.

- Order reference nếu có.

- Call job reference nếu có.

- Program.

- Environment.

- Timestamp.

- Source system.

- Result.

- Reviewer.

- Review status.

- Correlation ID.

- Audit reference.

- Retest reference nếu có.

Không có metadata thì evidence chưa đủ chuẩn governance.

## 48.5. Evidence không được sửa/xóa tùy tiện

Không được:

- Xóa failed evidence.

- Xóa technical exception.

- Xóa capacity incident.

- Sửa DTMF key.

- Sửa timestamp.

- Sửa SIM channel.

- Sửa result để PASS.

- Sửa no answer thành confirmed.

- Sửa technical exception thành no answer.

- Sửa smoke fail thành smoke pass.

- Sửa notice bị chặn thành notice sent.

Nếu cần correction, tạo correction record.

Không ghi đè lịch sử.

## 49. DONE GATE — GOVERNANCE

## 49.1. PACK-09 Governance Done Gate

PACK-09 governance chỉ PASS khi có đủ:

- Mô hình Internal SIM Gateway.

- Call script chuẩn.

- Phím 1/phím 0.

- Phím 9 NOT_ENABLED.

- Rule Giờ Vàng 10 phút / 2 attempts.

- Rule 24/7 15 phút / 3 attempts.

- Capacity baseline.

- Cross-pack boundary.

- Core State Machine boundary.

- Technical exception boundary.

- Post-IVR cancellation notice boundary.

- Engineer handoff model.

- P0 MUST / MUST NOT / FAIL IF.

- Smoke Matrix IVR-SMK-001 -> IVR-SMK-025.

- Evidence Governance.

- Không có nội dung nào cho phép production runtime khi chưa release.

Trạng thái:

## PACK_09_GOVERNANCE_DONE_GATE = PASS

## 49.2. Governance Fail Gate

Governance phải FAIL nếu:

- Tài liệu cho phép IVR làm CRM.

- Tài liệu cho phép IVR làm marketing.

- Tài liệu cho phép IVR gọi chăm sóc khách hàng.

- Tài liệu không khóa Internal SIM Gateway.

- Tài liệu cho phép cloud IVR mặc định.

- Tài liệu thiếu Core State Machine boundary.

- Tài liệu cho phép SIM Gateway hủy đơn trực tiếp.

- Tài liệu thiếu technical exception boundary.

- Tài liệu thiếu capacity baseline.

- Tài liệu thiếu post-IVR cancellation notice boundary.

- Tài liệu thiếu P0 rules.

- Tài liệu cho phép production runtime khi implementation chưa có.

## 50. DONE GATE — IMPLEMENTATION

## 50.1. Implementation Done Gate

Implementation Done chỉ được gọi khi:

- Internal SIM Gateway kết nối pass.

- SIM channel manager pass.

- Call job queue pass.

- Deadline-aware scheduler pass.

- DTMF capture pass.

- Call execution adapter pass.

- Call result normalizer pass.

- Order State Machine adapter pass.

- Audit/evidence writer pass.

- Capacity incident monitor pass.

- SIM health monitor pass.

- Notification owner handoff pass.

- Admin monitoring pass.

- Security/privacy pass.

- All P0 smoke pass.

- Evidence accepted.

## 50.2. Implementation Fail Gate

Implementation phải FAIL nếu:

- SIM Gateway gọi được nhưng DTMF fail.

- DTMF pass nhưng Core boundary fail.

- Scheduler gọi ngoài window.

- Scheduler batch cuối phiên.

- Scheduler assign trùng SIM.

- Technical error bị tính no answer.

- Capacity overload bị che giấu.

- Admin có thể fake result.

- Evidence writer không hoạt động.

- Notification gửi notice trước Core cancellation.

- Downstream dùng IVR trước release.

- Security/privacy chưa kiểm tra.

- P0 smoke fail.

## 51. DONE GATE — CALL SCRIPT / DTMF

## 51.1. Call Script Done Gate

Call Script chỉ PASS khi:

- Script đúng nội dung chuẩn.

- Chỉ đọc biến được phép.

- Không đọc full address.

- Không đọc member tier.

- Không đọc Diamond/referral.

- Không đọc payment detail.

- Không đọc AI/CRM content.

- Không đọc health/sensitive note.

- Không có nội dung marketing/CRM/upsell.

- Evidence accepted.

## 51.2. DTMF Done Gate

DTMF chỉ PASS khi:

- Phím 1 map đúng IVR_CONFIRMED.

- Phím 0 map đúng IVR_CUSTOMER_CANCELLED.

- Phím 9 NOT_ENABLED.

- Sai phím không tạo order action.

- No input không tạo confirm/cancel.

- DTMF error đi technical exception.

- Evidence accepted.

## 52. DONE GATE — SCHEDULER / CAPACITY

## 52.1. Scheduler Done Gate

Scheduler chỉ PASS khi:

- Deadline-aware rolling queue hoạt động.

- Attempt 1 đúng T0.

- Attempt schedule đúng theo program.

- Golden Hour attempt 2 đúng T0 + 5 phút.

- 24/7 attempt 2 đúng T0 + 5 phút.

- 24/7 attempt 3 đúng T0 + 10 phút.

- Không gọi ngoài window.

- Không tạo attempt vượt rule theo program.

- Không batch cuối phiên.

- Không assign trùng SIM.

- Duplicate job bị block.

- Capacity incident được tạo khi quá tải.

- Evidence accepted.

## 52.2. Capacity Done Gate

Capacity chỉ PASS khi:

- Capacity tính theo số SIM vật lý.

- 1 SIM = 1 active call.

- 5 phút capacity đúng baseline.

- 15 phút capacity đúng baseline.

- 45 phút rolling capacity đúng baseline.

- 24/7 3 attempts được tính vào call load.

- SIM failure làm giảm capacity đúng.

- Overload tạo capacity incident.

- Capacity incident không bị map thành no answer.

- Capacity incident không kích hoạt cancellation notice.

- Evidence accepted.

## 53. DONE GATE — CORE / EXCEPTION

## 53.1. Core Boundary Done Gate

Core boundary chỉ PASS khi:

- IVR chỉ gửi signal.

- Core quyết định order state.

- SIM Gateway không đổi order state.

- Callback không đổi order state.

- Admin UI không hủy order ngoài Core.

- Reason code do Core ghi nhận.

- State transition có audit.

- Post-IVR cancellation notice trigger chỉ phát hành sau Core cancellation.

- Evidence accepted.

## 53.2. Technical Exception Done Gate

Technical exception chỉ PASS khi:

- SIM error -> IVR_TECHNICAL_EXCEPTION.

- Server error -> IVR_TECHNICAL_EXCEPTION.

- DTMF error -> IVR_TECHNICAL_EXCEPTION.

- Audio error -> IVR_TECHNICAL_EXCEPTION.

- Scheduler error -> IVR_TECHNICAL_EXCEPTION.

- Technical exception không map no answer.

- Technical exception không kích hoạt cancellation notice.

- Admin Review được tạo khi cần.

- Evidence accepted.

## 54. DONE GATE — CROSS-PACK / SECURITY

## 54.1. Cross-Pack Done Gate

Cross-pack boundary chỉ PASS khi:

- AI Advisor không tạo call job.

- AI Advisor không tự gửi cancellation notice.

- Facebook Gateway không gọi IVR trực tiếp.

- Facebook Gateway không tự gửi cancellation notice.

- Messenger text không giả lập IVR result.

- Live comment không giả lập IVR confirmation.

- CRM không dùng IVR cho chăm sóc/marketing.

- CRM không dùng cancellation notice như upsell/marketing.

- Payment không dùng IVR xác nhận paid.

- Warehouse không dùng raw IVR để xuất kho.

- MISA không sync IVR.

- PACK-07 không dùng IVR result làm verified revenue.

- Notification owner boundary pass.

- Downstream dependency bị chặn khi IVR chưa release.

- Evidence accepted.

## 54.2. Security / Privacy Done Gate

Security / Privacy chỉ PASS khi:

- IVR data minimization pass.

- Không đọc dữ liệu bị cấm.

- Admin UI không hiển thị dữ liệu không cần thiết.

- Cancellation notice không chứa dữ liệu bị cấm.

- Role/permission pass.

- Evidence access controlled.

- Call result protected.

- DTMF result protected.

- Audit immutable hoặc có correction record.

- No unauthorized export.

- Evidence accepted.

## 55. FAIL GATE TỔNG THỂ

PACK-09 phải FAIL hoặc bị chặn nếu có bất kỳ điều kiện sau:

- IVR được dùng cho mục đích ngoài xác nhận đơn.

- IVR trở thành kênh sales/CRM/marketing.

- Cloud IVR/provider ngoài được dùng làm default khi chưa có owner decision.

- Một SIM xử lý nhiều active call.

- Scheduler assign trùng SIM.

- Call script đọc full address.

- Call script đọc member tier.

- Call script đọc Diamond/referral.

- Call script đọc payment detail.

- Call script đọc health/sensitive note.

- Phím 1/phím 0 map sai.

- Phím 9 được bật ngoài rule.

- Giờ Vàng không đúng 10 phút.

- 24/7 không đúng 15 phút.

- Attempt schedule không đúng theo program.

- Giờ Vàng attempt 2 không đúng T0 + 5 phút.

- Giờ Vàng tạo attempt 3.

- 24/7 attempt 2 không đúng T0 + 5 phút.

- 24/7 attempt 3 không đúng T0 + 10 phút.

- 24/7 tạo attempt 4.

- Gọi ngoài confirmation window.

- Batch toàn bộ cuộc gọi cuối phiên.

- Scheduler không deadline-aware.

- Technical error bị tính no answer.

- Capacity overload bị tính no answer.

- SIM Gateway hủy đơn trực tiếp.

- IVR callback đổi order state trực tiếp.

- Admin sửa IVR result giả.

- Admin hủy đơn ngoài Core.

- AI Advisor tạo IVR call job.

- AI Advisor tự gửi cancellation notice.

- Facebook/Messenger/Live gọi IVR trực tiếp.

- Facebook Gateway tự gửi cancellation notice.

- CRM dùng IVR cho chăm sóc.

- CRM dùng cancellation notice như marketing/upsell.

- Payment dùng IVR xác nhận paid.

- Warehouse dùng IVR cho xuất kho.

- MISA dùng IVR để ghi kế toán.

- PACK-07 dùng IVR result như verified revenue.

- Downstream phụ thuộc IVR result khi IVR chưa release.

- Tin nhắn hủy đơn được gửi trước khi Core Order State Machine hủy đơn chính thức.

- SIM Gateway tự gửi cancellation notice.

- Technical exception kích hoạt cancellation notice.

- Capacity incident kích hoạt cancellation notice.

- Evidence thiếu.

- Evidence rejected.

- P0 smoke fail.

- Security/privacy chưa pass.

- Owner sign-off thiếu.

- Release decision thiếu.

- Completion Report PASS khi thiếu evidence.

Fail Gate ưu tiên an toàn nghiệp vụ, bảo vệ khách hàng, bảo vệ order state, bảo vệ privacy và bảo vệ audit hơn tốc độ triển khai.

## 56. RELEASE CONTROL GOVERNANCE

## 56.1. PACK-09 không được Release Ready chỉ vì tài liệu xong

Hoàn tất tài liệu không đồng nghĩa production ready.

PACK-09 chỉ Release Ready khi có đủ:

- Governance complete.

- IVR-00 -> IVR-09 complete nếu scope yêu cầu.

- Implementation complete.

- Test complete.

- Smoke complete.

- Security review complete.

- Evidence accepted.

- No P0 open.

- Downstream dependency review pass.

- Notification owner boundary pass.

- Owner sign-off.

- Release decision approved.

Trước khi đạt đủ, trạng thái vẫn là:

## 56.2. Release status registry

Các trạng thái release tối thiểu:

- RELEASE_NOT_STARTED.

- DOCUMENTATION_ACTIVE.

- GOVERNANCE_DOCUMENTATION_COMPLETE.

- IMPLEMENTATION_NOT_STARTED.

- IMPLEMENTATION_IN_PROGRESS.

- IMPLEMENTATION_COMPLETE_waiting_TEST.

- TEST_waiting.

- TEST_FAILED.

- SMOKE_waiting.

- SMOKE_FAILED.

- SMOKE PASS WAITING EVIDENCE.

- SECURITY_REVIEW_waiting.

- SECURITY_REVIEW_FAILED.

- EVIDENCE_waiting.

- EVIDENCE_REJECTED.

- EVIDENCE_ACCEPTED.

- OWNER_REVIEW_waiting.

- OWNER_SIGNED_OFF.

- RELEASE_DECISION_waiting.

- RELEASE_BLOCKED.

- ROLLBACK_REQUIRED nếu có sự cố sau release.

PACK-09 không được nhảy trạng thái.

## 56.3. Release STOP_POINTS P0

Release bị bị chặn nếu có một trong các STOP_POINTS:

- Không có Internal SIM Gateway evidence.

- Không có DTMF evidence.

- Không có Scheduler evidence.

- Không có Core boundary evidence.

- Không có technical exception evidence.

- Không có capacity evidence.

- Không có security/privacy evidence.

- Không có notification boundary evidence.

- Không có smoke evidence.

- Không có owner sign-off.

- Có P0 issue open.

- Có downstream dependency chưa được chặn.

- Có module dùng IVR result trước release.

- Có quyền Admin có thể fake result.

- Có scheduler batch cuối phiên.

- Có technical error bị map thành no answer.

- Có cancellation notice gửi trước Core cancellation.

## 56.4. Production enablement rule

Chỉ được bật production khi:

- CALLABLE_IVR_API_ALLOWED_IN_PRODUCTION = CÓ theo release decision.

- REAL_CUSTOMER_CALL_ALLOWED = CÓ theo release decision.

- PRODUCTION_SIM_GATEWAY_ALLOWED = CÓ theo release decision.

- PRODUCTION_ORDER_CANCEL_BY_IVR_ALLOWED = CÓ theo release decision và Core boundary.

- DOWNSTREAM_IVR_DEPENDENCY_ALLOWED = CÓ theo scope được phê duyệt.

- POST_IVR_CANCELLATION_NOTICE_ALLOWED_IN_PRODUCTION = CÓ theo release decision.

- Monitoring active.

- Alert active.

- Rollback plan active.

- Owner sign-off active.

Nếu chưa có release decision, tất cả vẫn là KHÔNG.

## 57. ROLLBACK / RECOVERY GOVERNANCE

## 57.1. Rollback không được xóa lịch sử IVR

Nếu IVR lỗi sau release, rollback không được xóa evidence.

Rollback chỉ được:

- Tắt call job mới.

- Disable production SIM Gateway.

- Stop scheduler.

- Disable downstream dependency.

- Disable post-cancel notice nếu lỗi liên quan notification.

- Chuyển Core handling sang safe mode.

- Đưa affected orders vào Admin Review.

- Ghi incident.

- Giữ toàn bộ call/result/evidence.

- Retest.

- Owner review.

Không được xóa log lỗi để làm sạch hệ thống.

## 57.2. Khi nào rollback bắt buộc

Rollback bắt buộc khi:

- SIM Gateway gọi sai khách hàng.

- Script đọc dữ liệu bị cấm.

- Phím 1/0 map sai.

- Technical error bị hủy đơn hàng loạt.

- Capacity overload bị tính no answer.

- Scheduler gọi ngoài window.

- Scheduler batch cuối phiên.

- Admin sửa được result giả.

- SIM Gateway đổi order state trực tiếp.

- Downstream dùng IVR result sai.

- Cancellation notice gửi sai điều kiện.

- Cancellation notice chứa dữ liệu bị cấm.

- Privacy/security incident.

- Evidence writer mất dữ liệu.

- P0 issue phát sinh production.

## 57.3. Recovery Done Gate

Recovery chỉ done khi:

- Nguyên nhân lỗi được xác định.

- Phạm vi order/call/notice bị ảnh hưởng được khoanh vùng.

- Dữ liệu evidence được giữ.

- Affected orders được đưa Admin Review nếu cần.

- Scheduler/DTMF/Core/Notification boundary được sửa.

- Smoke liên quan retest pass.

- Evidence mới accepted.

- Owner sign-off recovery.

- Release/resume decision rõ.

- No P0 open.

## 58. COMPLETION REPORT CONTROL

## 58.1. Completion Report không được PASS nếu PACK-09 thiếu evidence

Completion Report không được PASS nếu:

- PACK-09 là dependency production mà chưa release.

- IVR smoke chưa pass.

- IVR evidence chưa accepted.

- IVR security chưa pass.

- IVR Core boundary chưa pass.

- IVR technical exception chưa pass.

- IVR capacity chưa pass.

- Post-IVR cancellation notice smoke chưa pass nếu scope dùng.

- Downstream dependency chưa review.

- Owner sign-off thiếu.

Không được ghi Completion PASS bằng lời xác nhận miệng.

## 58.2. Gateway / Production / Go-live status

Khi PACK-09 chưa đủ release:

Không được dùng PACK-09 documentation complete để gọi toàn hệ production ready.

## 59. HANDOFF SANG IVR-00 -> IVR-09

## 59.1. IVR-00 — Governance / Source of Truth / Scope Boundary

- PACK_09_VERSION = V1.1_CLEAN_FINAL.

- Mục tiêu ORDER_CONFIRMATION_ONLY.

- Internal SIM Gateway model.

- Production bị chặn.

- Source-of-truth boundary.

- Core State Machine boundary.

- P0 rules.

- Release gate.

- Evidence requirement.

- Post-IVR cancellation notice boundary.

## 59.2. IVR-01 — Business Purpose / Confirmation Use Case

IVR-01 phải khóa:

- Đơn nào cần IVR.

- Đơn nào không cần IVR.

- Điều kiện có order_code.

- Điều kiện chương trình Giờ Vàng.

- Điều kiện chương trình 24/7.

- Điều kiện số điện thoại hợp lệ.

- Điều kiện không gọi.

- Invalid phone rule.

- No answer rule.

- Technical exception rule.

- Customer trust/risk rule.

- Call eligibility gate.

- Post-IVR cancellation notice eligibility.

## 59.3. IVR-02 — Ownership Boundary / Connected Systems

IVR-02 phải khóa:

- Call job lifecycle.

- SIM channel manager.

- One SIM one active call.

- Deadline-aware rolling queue.

- Golden Hour schedule.

- 24/7 schedule 3 attempts: T0 / T0 + 5 phút / T0 + 10 phút.

- DTMF key 1/0.

- Phím 9 NOT_ENABLED.

- No duplicate call job.

- Capacity incident.

## 59.4. IVR-03 — Eligibility Rule / Customer Trust / Official Contact

IVR-03 phải khóa:

- IVR result taxonomy.

- Result normalization.

- Core signal.

- Core response.

- Cancel reason.

- IVR_CONFIRMED.

- IVR_CUSTOMER_CANCELLED.

- IVR_NO_ANSWER_FINAL.

- IVR_CONFIRMATION_WINDOW_EXPIRED.

- Technical exception boundary.

- Order state transition audit.

- Post-IVR Cancellation Notice Trigger.

- Notification owner boundary.

- No-answer vs invalid-phone vs technical-exception distinction.

## 59.5. IVR-04 — Order Core -> IVR Task Contract

IVR-04 phải khóa:

- Admin dashboard.

- Call job view.

- SIM channel view.

- Capacity incident view.

- Technical exception view.

- Audit evidence.

- Permission.

- Không fake result.

- Không hủy đơn ngoài Core.

- Evidence review.

- Post-IVR cancellation notice evidence.

- Notification owner audit.

- Cancellation notice delivery status nếu có.

## 59.6. IVR-05 — Attempt Policy / Scheduler / Queue

IVR-05 phải khóa:

- IVR-SMK-001 -> IVR-SMK-025.

- Positive path.

- Negative path.

- Security test.

- Privacy test.

- Capacity test.

- DTMF test.

- Core boundary test.

- Downstream dependency test.

- Release gate.

- Post-IVR cancellation notice smoke.

- Technical exception no-notice smoke.

- Notification owner boundary smoke.

- 24/7 three-attempt schedule smoke.

## 59.7. IVR-06 — Internal SIM Gateway Adapter

IVR-06 phải khóa:

- Internal SIM Gateway adapter boundary.
- SIM channel lifecycle.
- Device/provider protocol owner decision.
- DTMF capture handoff.
- SIM health and disable rule.

## 59.8. IVR-07 — Result Normalization / IVR -> Order Core Callback

IVR-07 phải khóa:

- Raw SIM result normalization.
- Callback contract về Order Core.
- Idempotency / correlation / evidence id.
- No direct order update.
- Technical exception vs customer no-answer distinction.

## 59.9. IVR-08 — Admin Monitoring / Evidence / Audit / Privacy

IVR-08 phải khóa:

- Admin monitoring.
- Evidence registry.
- Audit fields.
- Permission boundary.
- Phone/call-log privacy.

## 59.10. IVR-09 — Test Matrix / Smoke / Release Gate

IVR-09 phải khóa:

- Full smoke matrix.
- Security/privacy tests.
- Capacity tests.
- Release evidence.
- Owner sign-off.

## 60. PACK-09 FINAL DONE GATE

## 60.1. Documentation Done

PACK-09 được xem là hoàn tất tài liệu khi có đủ 4 phần:

- PHẦN 1/4 — IVR Order Confirmation Principles / Internal SIM Gateway / Source-of-Truth Boundary / No-Bypass Rule.

- PHẦN 2/4 — Call Script / DTMF / Program Window / Golden Hour / 24-7 / Deadline-Aware Scheduler.

- PHẦN 3/4 — Call Result / Technical Exception / Capacity Baseline / Cross-Pack Control / Engineer Handoff.

- PHẦN 4/4 — P0 Rules / Smoke Matrix / IVR Evidence / Done Gate / Fail Gate / Release Control / Final Conclusion.

Trạng thái:

## 60.2. Implementation Done

Implementation Done chỉ được gọi khi:

- Internal SIM Gateway triển khai xong.

- SIM channel manager triển khai xong.

- Call job queue triển khai xong.

- Deadline-aware scheduler triển khai xong.

- DTMF capture triển khai xong.

- Call result normalizer triển khai xong.

- Order State Machine adapter triển khai xong.

- Audit/evidence writer triển khai xong.

- Capacity incident monitor triển khai xong.

- Admin monitoring triển khai xong.

- Notification owner handoff triển khai xong nếu scope dùng.

- Security/privacy pass.

- Smoke pass.

- Evidence accepted.

## 60.3. Smoke Done

Smoke Done chỉ được gọi khi:

- IVR-SMK-001 đến IVR-SMK-025 được test.

- Positive path pass.

- Negative path pass.

- No P0 smoke fail.

- Retest pass nếu có lỗi.

- Evidence accepted cho từng smoke.

- Owner review pass.

## 60.4. IVR PASS

IVR PASS chỉ được gọi khi:

- Governance Done Gate pass.

- Implementation Done Gate pass.

- Call Script Done Gate pass.

- DTMF Done Gate pass.

- Scheduler Done Gate pass.

- Capacity Done Gate pass.

- Core Boundary Done Gate pass.

- Technical Exception Done Gate pass.

- Cross-Pack Done Gate pass.

- Security/Privacy Done Gate pass.

- Post-IVR cancellation notice gate pass nếu scope dùng.

- Smoke Done.

- Evidence accepted.

- No P0 open.

- Owner sign-off.

## 60.5. Release Ready

PACK-09 Release Ready chỉ được gọi khi:

- IVR PASS.

- IVR-00 -> IVR-09 hoàn tất theo scope.

- Production SIM Gateway ready.

- Real customer call policy approved.

- Monitoring/alert active.

- Rollback plan active.

- Downstream dependency approved theo scope.

- Notification owner production policy approved nếu scope dùng.

- Completion evidence accepted.

- Owner sign-off.

- Release decision approved.

## 61. FINAL STATUS

## 61.1. Trạng thái cuối tài liệu

## PACK_09_NAME = IVR_ORDER_CONFIRMATION

## PACK_09_VERSION = V1.1_CLEAN_FINAL

## PACK_09_DEPLOYMENT_MODEL = INTERNAL_SIM_GATEWAY_SERVER

## 61.2. Trạng thái triển khai

## 61.3. Trạng thái gate

## CALLABLE_IVR_API_ALLOWED_IN_PRODUCTION = KHÔNG

## DOWNSTREAM_IVR_DEPENDENCY_ALLOWED = KHÔNG

## 61.4. Trạng thái toàn hệ

## KẾT LUẬN CUỐI PACK-09

PACK-09 là pack chính thức cho IVR_ORDER_CONFIRMATION của hệ thống Ginsengfood.

PACK-09 triển khai theo mô hình:

## INTERNAL_SIM_GATEWAY_SERVER

PACK-09 phục vụ một mục đích duy nhất:

## ORDER_CONFIRMATION_ONLY

PACK-09 không phải kênh bán hàng.

PACK-09 không phải CRM.

PACK-09 không phải chăm sóc khách hàng.

PACK-09 không phải marketing.

PACK-09 không phải Payment Core.

PACK-09 không phải Delivery Core.

PACK-09 không phải Warehouse.

PACK-09 không phải MISA.

PACK-09 không phải AI Advisor.

PACK-09 không phải Facebook Gateway.

PACK-09 không phải Ads/ROAS.

PACK-09 chỉ gọi tự động xác nhận đơn hàng bằng SIM Gateway nội bộ.

PACK-09 dùng phím:

1 = khách xác nhận tiếp tục xử lý đơn.

0 = khách không đặt hoặc muốn hủy đơn.

Phím 9 hỗ trợ người thật hiện:

## NOT_ENABLED

Giờ Vàng:

10 phút / 2 attempts / attempt interval 5 phút.

24/7:

15 phút / 3 attempts / lịch gọi T0, T0 + 5 phút, T0 + 10 phút.

Scheduler bắt buộc:

## DEADLINE_AWARE_ROLLING_QUEUE

Batch cuối phiên:

## PROHIBITED

Nguyên tắc khóa cuối cùng của PACK-09:

Internal SIM Gateway là mô hình chính thức.

1 SIM = 1 active outbound call.

IVR chỉ xác nhận đơn hàng.

IVR result chỉ là input signal.

Core Order State Machine là final decision layer.

SIM Gateway không cập nhật order trực tiếp.

Admin không được sửa IVR result giả.

Phím 1 xác nhận, phím 0 hủy.

Phím 9 không bật trong scope hiện tại.

Giờ Vàng 10 phút / 2 attempts.

24/7 15 phút / 3 attempts.

Attempt schedule theo program: Giờ Vàng mỗi 5 phút; 24/7 mỗi 5 phút.

Technical error không phải khách không nghe.

Capacity overload không phải lỗi khách.

Invalid phone không được nhầm với no answer.

No answer là máy có tín hiệu gọi hợp lệ nhưng khách không nghe sau đủ attempts theo program.

Không batch cuối phiên.

Không downstream dependency trước release.

Post-IVR cancellation notice chỉ được gửi sau khi Core Order State Machine đã hủy đơn chính thức.

SIM Gateway không được gửi tin nhắn hủy đơn.

AI Advisor không được tự gửi tin nhắn hủy đơn.

CRM không được dùng tin nhắn hủy đơn như marketing hoặc upsell.

Technical exception không kích hoạt cancellation notice.

Capacity incident không kích hoạt cancellation notice.

Không có test/smoke/security/evidence thì không release.

Không có evidence accepted thì không PASS.

PACK-09 hoàn tất về mặt governance/documentation, nhưng chưa được gọi là triển khai xong, chưa được gọi là IVR PASS, chưa được gọi là Release Approved và chưa được gọi là Production Ready nếu chưa có implementation, test, smoke, security, accepted evidence, owner sign-off và release decision.
