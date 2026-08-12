# TECH-09 - IVR ORDER CONFIRMATION AUTO CALL VERIFICATION ANTI FAKE ORDER CONTROL

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay nhu `Golden Hour 2 attempts/10 phut` va `24/7 3 attempts/15 phut` chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca hai chuong trinh; `GOLDEN_HOUR` window 5 phut voi A2@`T0+2:30`; `TWENTY_FOUR_SEVEN` window 15 phut voi A2@`T0+7:30`. Nguon canonical: `plan/ivr-orther/decisions-log.md` D-10 va `specs/functional/03-scheduler-attempt-policy.md`.

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

IVR CALL GOVERNANCE / ORDER CONFIRMATION STATE MACHINE / NEW CUSTOMER VERIFICATION / INVALID PHONE / KHONG ANSWER / CUSTOMER CANCEL / TECHNICAL FAILURE / NOTIFICATION HANDOFF / PRIVACY-SAFE CALL RESULT

## PHẦN 1/4 - IVR ORDER CONFIRMATION PRINCIPLES / SOURCE-OF-TRUTH / ELIGIBILITY BOUNDARY / NO-MASS-CALLING RULE

## 1. MỤC TIÊU CỦA TECH-09

TECH-09 khóa lớp IVR Order Confirmation cho hệ thống Ginsengfood theo hướng greenfield.

Tài liệu này không viết code, không thiết kế API chi tiết, không thiết kế database chi tiết, không thiết kế UI chi tiết.

TECH-09 có nhiệm vụ xác định rõ:

IVR được phép làm gì.

IVR không được phép làm gì.

Đơn hàng nào được đưa vào IVR.

Khách hàng nào cần IVR xác nhận.

Khách hàng nào không gọi IVR đại trà.

Kết quả cuộc gọi IVR ảnh hưởng thế nào đến trạng thái đơn.

Khi nào đơn bị hủy do không xác nhận được.

Ai được phép gửi thông báo sau khi đơn bị hủy.

IVR phải chống đơn ảo, đơn phá hoại nhưng không được làm phiền khách hàng tin cậy.

IVR không được thay Commerce, Payment, Delivery, Verified Revenue, AI Advisor, Facebook Gateway, MC AI Live hoặc Human Owner.

## 2. VAI TRÒ CỐT LÕI CỦA IVR TRONG GINSENGFOOD

IVR trong Ginsengfood là lớp tự động gọi xác nhận đơn hàng có điều kiện.

IVR không phải hệ thống bán hàng.

IVR không phải hệ thống tư vấn.

IVR không phải hệ thống tạo đơn.

IVR không phải hệ thống thanh toán.

IVR không phải hệ thống ghi nhận doanh thu.

IVR không phải hệ thống chăm sóc khách hàng đại trà.

IVR chỉ là lớp xác minh tự động nhằm hỗ trợ:

Xác nhận ý chí đặt hàng của khách.

Xác nhận thông tin cơ bản theo chính sách.

Giảm đơn ảo.

Giảm đơn phá hoại.

Giảm rủi ro giao sai, giao không ai nhận.

Tạo evidence cho trạng thái xác nhận đơn.

Hỗ trợ Core Order State Machine quyết định bước tiếp theo.

## 3. NGUYÊN TẮC NO-MASS-CALLING RULE

## 3.1. IVR không gọi đại trà

IVR không được gọi cho mọi khách hàng.

IVR chỉ áp dụng khi đơn hàng và khách hàng thỏa điều kiện cần xác minh theo chính sách.

Mục tiêu của IVR là kiểm soát rủi ro, không phải làm phiền toàn bộ khách hàng.

## 3.2. Nhóm khách hàng được đưa vào IVR

IVR được phép áp dụng cho các nhóm sau:

Khách mới.

Khách chưa có lịch sử mua hàng.

Khách chưa đăng ký thành viên.

Khách chưa có đủ thông tin xác thực.

Khách có thông tin nhận hàng chưa đủ độ tin cậy.

Khách có tín hiệu rủi ro theo Anti-Fake-Order Policy.

Đơn có dấu hiệu bất thường cần xác nhận lại.

Đơn đến từ kênh có rủi ro spam / phá hoại / đơn ảo.

Đơn có số điện thoại chưa từng xác minh.

Đơn có thông tin người nhận khác với lịch sử khách hàng cũ và cần xác minh theo policy.

## 3.3. Nhóm khách hàng không gọi IVR đại trà

IVR không áp dụng đại trà cho:

Khách cũ có lịch sử mua hàng tốt.

Thành viên đã có dữ liệu tin cậy.

Khách đã từng xác minh số điện thoại.

Khách có hồ sơ nhận hàng ổn định.

Khách có tỷ lệ nhận hàng tốt.

Khách đã có Customer Trust Score đạt ngưỡng tin cậy.

Đơn lặp lại với thông tin nhận hàng trùng khớp lịch sử đáng tin cậy.

Đơn không có tín hiệu rủi ro theo policy.

## 3.4. P0 điểm chặn

FAIL nếu hệ thống IVR gọi tất cả khách hàng theo mặc định.

FAIL nếu khách trusted vẫn bị gọi đại trà mà không có tín hiệu rủi ro.

FAIL nếu IVR được dùng như công cụ chăm sóc khách hàng, marketing hoặc upsell.

## 4. IVR SOURCE-OF-TRUTH BOUNDARY

## 4.1. IVR không phải nguồn sự thật nghiệp vụ

IVR không sở hữu sự thật về:

Giá.

Khuyến mãi.

Thành viên.

Diamond referral.

Thanh toán.

Doanh thu.

Giao hàng thành công.

Tồn kho.

Sellable status.

Recall / Sale Lock.

Product Knowledge.

Claim được phép nói.

Customer Profile đầy đủ.

Order lifecycle gốc.

## 4.2. Nguồn sự thật upstream của IVR

IVR chỉ được nhận dữ liệu từ các nguồn đã khóa trước đó:

TECH-03 Operational Core
Cung cấp trạng thái sellable, warehouse, inventory, traceability, recall, sale lock.

TECH-04 Commerce Runtime
Cung cấp official order, order status, quote snapshot đã hết vai trò báo giá, customer confirmation flow, payment boundary, delivery boundary.

TECH-05 AI Advisor Runtime
Cung cấp ngữ cảnh tư vấn và handoff đơn, nhưng không phải nguồn xác nhận thanh toán hoặc doanh thu.

TECH-06 Facebook Gateway
Cung cấp channel delivery context, Messenger handoff context, nhưng không được tự tạo order hoặc tự xác nhận đơn.

TECH-07 Ads Measurement
Chỉ nhận verified revenue sau Commerce, không lấy IVR confirmation làm revenue.

TECH-08 MC AI Live
Cung cấp ngữ cảnh live/order intent, nhưng không được dùng live signal làm xác nhận order chính thức.

## 4.3. Nguồn sự thật downstream nhận từ IVR

IVR chỉ được trả về:

Kết quả cuộc gọi.

Loại kết quả cuộc gọi.

Evidence cuộc gọi.

Thời điểm gọi.

Số lần gọi.

Call attempt result.

Outcome classification.

Cờ cần human review nếu có.

Cờ đủ điều kiện xác nhận / hủy / chờ xử lý theo policy.

IVR không được tự ghi đè trạng thái nghiệp vụ nếu không qua Core Order State Machine.

## 5. ORDER CONFIRMATION BOUNDARY

## 5.1. IVR chỉ xử lý Official Order đủ điều kiện

IVR không được gọi cho:

Quote.

Cart.

Order Draft.

Order Intent.

Comment mua hàng.

Messenger conversation chưa tạo official order.

Live signal.

Payment WAITING chưa có official order.

Đơn chưa qua Commerce eligibility.

Đơn không có order identity chính thức.

IVR chỉ xử lý Official Order đã được Commerce tạo ra và đánh dấu đủ điều kiện đưa vào IVR theo policy.

## 5.2. Order Draft không phải Official Order

Order Draft là bản nháp ghi nhận thông tin đặt hàng.

Order Draft chưa phải đơn chính thức.

Order Draft chưa được gọi IVR.

Order Draft chưa có giá trị xác nhận giao hàng.

Order Draft chưa được dùng để tính revenue.

Order Draft chưa được dùng để đo ROAS.

Order Draft chưa được dùng để tạo call verification.

## 5.3. Official Order vẫn chưa đồng nghĩa đã thanh toán

Official Order là đơn chính thức trong Commerce.

Tuy nhiên:

Official Order không đồng nghĩa PAID.

Official Order không đồng nghĩa Verified Revenue.

Official Order không đồng nghĩa giao hàng thành công.

Official Order không đồng nghĩa khách chắc chắn nhận hàng.

Official Order không đồng nghĩa đủ điều kiện scale Ads.

Official Order không đồng nghĩa đơn đã hoàn tất.

## 5.4. IVR confirmation chỉ xác nhận ý chí đặt hàng

IVR confirmation chỉ xác nhận rằng khách đã phản hồi theo kịch bản xác nhận đơn.

IVR confirmation có thể xác nhận:

Khách có biết đơn hàng.

Khách đồng ý nhận hàng.

Khách xác nhận số lượng cơ bản theo policy.

Khách xác nhận thông tin nhận hàng ở mức privacy-safe.

Khách cần hỗ trợ thêm.

Khách chủ động hủy.

IVR confirmation không xác nhận:

Đã thanh toán.

Đã giao hàng.

Đã thu COD.

Đã ghi nhận doanh thu.

Đã tạo Verified Revenue.

Đã hoàn thành nghĩa vụ giao hàng.

Đã đủ điều kiện Ads ROAS.

Đã đủ điều kiện commission.

## 6. CUSTOMER ELIGIBILITY BOUNDARY

## 6.1. Điều kiện đưa khách vào IVR

Một khách hàng chỉ được đưa vào IVR khi có ít nhất một điều kiện:

Customer status = NEW.

Customer trust chưa đủ.

Customer profile thiếu dữ liệu xác thực.

Số điện thoại chưa từng xác minh.

Đơn hàng có rủi ro đơn ảo.

Đơn hàng có rủi ro phá hoại.

Thông tin nhận hàng bất thường.

Đơn hàng có thay đổi người nhận.

Đơn hàng có thay đổi địa chỉ nhận.

Đơn hàng có số lượng bất thường theo policy.

Đơn hàng phát sinh từ kênh có rủi ro cao.

Customer Memory chưa đủ để bỏ qua IVR.

## 6.2. Điều kiện loại trừ khỏi IVR

Khách hàng được loại trừ khỏi IVR nếu:

Là khách cũ đáng tin cậy.

Có lịch sử mua hàng thành công.

Có hồ sơ thành viên xác thực.

Có số điện thoại đã xác minh.

Thông tin nhận hàng trùng lịch sử đáng tin cậy.

Không có tín hiệu rủi ro mới.

Không có thay đổi bất thường trong đơn.

Được Commerce đánh dấu trusted checkout theo policy.

## 6.3. Không được hardcode trusted customer

Trusted customer phải do Customer Trust Resolver hoặc Commerce/Customer Memory policy xác định.

Không được hardcode theo tên khách.

Không được hardcode theo số điện thoại thủ công.

Không được hardcode theo kênh bán.

Không được hardcode theo cảm tính.

## 6.4. P0 điểm chặn

FAIL nếu IVR tự quyết định khách trusted mà không qua policy.

FAIL nếu IVR bỏ qua khách rủi ro chỉ vì đã từng chat.

FAIL nếu IVR gọi lại khách cũ tin cậy trong mọi đơn mà không có rủi ro mới.

## 7. ANTI-FAKE-ORDER PRINCIPLE

## 7.1. Mục tiêu chống đơn ảo

IVR giúp giảm:

Đơn đặt bằng số điện thoại không thật.

Đơn phá hoại live.

Đơn spam từ comment.

Đơn tạo bằng thông tin giả.

Đơn cố tình đặt để gây chi phí vận hành.

Đơn có số điện thoại không liên lạc được.

Đơn có khách không xác nhận ý chí nhận hàng.

Đơn có rủi ro giao thất bại cao.

## 7.2. IVR không được thay Anti-Fake-Order System

IVR chỉ là một lớp xác minh.

Anti-Fake-Order Control phải kết hợp:

Commerce eligibility.

Customer trust.

Order risk.

Channel risk.

Phone validation.

Call result.

Human review nếu cần.

Evidence/audit.

Admin override có kiểm soát.

## 7.3. Không quy lỗi khách khi lỗi thuộc hệ thống

Nếu IVR lỗi kỹ thuật, hệ thống không được kết luận khách không nghe máy.

Nếu telco lỗi, hệ thống không được kết luận số không hợp lệ.

Nếu call provider timeout, hệ thống không được kết luận khách hủy.

Nếu recording/call result thiếu evidence, hệ thống không được dùng kết quả đó làm căn cứ hủy đơn.

## 8. PHONE VALIDATION PRINCIPLE

## 8.1. Invalid phone phải tách khỏi No Answer

Số điện thoại không có thật / invalid phone là một loại outcome riêng.

No Answer là trường hợp máy có thể gọi nhưng khách không nghe sau khi đổ chuông hợp lệ.

Hai trạng thái này không được trộn lẫn.

## 8.2. Nhóm Invalid Phone

Invalid Phone bao gồm:

Số sai định dạng.

Số thiếu chữ số.

Số không thuộc dải hợp lệ theo policy.

Số không thể quay gọi.

Số bị nhà mạng báo không tồn tại.

Số không thể thiết lập cuộc gọi vì lý do định danh số.

## 8.3. Nhóm No Answer

No Answer bao gồm:

Máy đổ chuông hợp lệ nhưng khách không nghe.

Hết thời lượng chuông theo policy.

Không có phản hồi từ khách sau attempt hợp lệ.

Đã gọi đủ số lần nhưng không có người bắt máy.

## 8.4. Nhóm Busy / Rejected / Unreachable

Busy / Rejected / Unreachable phải được phân loại riêng:

Busy: máy bận.

Rejected: khách bấm từ chối cuộc gọi.

Unreachable: thuê bao không liên lạc được trong thời điểm gọi.

Network unavailable: không kết nối được do mạng/telco.

Call setup failed: cuộc gọi chưa thiết lập được.

Những nhóm này không được tự động coi là No Answer nếu chưa đủ rule.

## 8.5. P0 điểm chặn

FAIL nếu invalid phone bị ghi thành no answer.

FAIL nếu technical failure bị ghi thành khách không nghe.

FAIL nếu khách bấm hủy bị ghi thành no answer.

FAIL nếu provider lỗi nhưng đơn bị hủy như lỗi khách.

## 9. CALL ATTEMPT POLICY PRINCIPLE

## 9.1. MAX_ATTEMPT_PER_ORDER

> [!IMPORTANT]
> Superseded by D-10: section nay giu baseline cu. Khi thiet ke/test/implement, dung rule moi: max 2 attempts cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH `T0`, `T0+2:30`, expire `T0+5:00`; 24/7 `T0`, `T0+7:30`, expire `T0+15:00`.

TECH-09 V1.0 khóa baseline:

## MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED

Attempt policy phải theo chương trình: Golden Hour 2 attempts trong 10 phút; 24/7 3 attempts trong 15 phút.

Không được chỉnh sửa tạm lẻ.

Không được áp một max attempt chung cho mọi chương trình hoặc áp sai attempt policy theo chương trình.

## 9.2. Golden Hour timing context

Với đơn thuộc bối cảnh Giờ Vàng, baseline hiện tại:

Quote freeze window: 5 phút.

IVR attempt policy tham chiếu: 2 cuộc.

Khoảng cách tham chiếu: 5 phút.

Tổng logic phải không phá vỡ quote/order confirmation window của Commerce.

Nếu quote/order hết hiệu lực, IVR không được tiếp tục xác nhận như đơn còn hợp lệ.

## 9.3. 24/7 timing context

Với đơn thuộc bối cảnh chương trình 24/7, baseline hiện tại:

Quote freeze window: 15 phút.

IVR attempt policy tham chiếu: 3 cuộc.

Khoảng cách tham chiếu: 5 phút.

Tổng logic phải không làm sai giá trị quote/order window.

Nếu đơn hết hiệu lực theo Commerce, IVR dừng và trả về outcome phù hợp.

## 9.4. IVR không được gọi ngoài trạng thái hợp lệ

IVR không được tiếp tục gọi nếu:

Đơn đã bị khách hủy.

Đơn đã được xác nhận.

Đơn đã bị Commerce hủy.

Đơn đã hết hiệu lực.

Đơn bị Sale Lock.

Đơn bị Recall/Suppression.

Đơn bị admin hold.

Runtime source-of-truth unavailable.

Call attempt đã đạt max.

Customer opt-out/call restriction theo policy.

## 9.5. P0 điểm chặn

FAIL nếu số lần gọi không rõ.

FAIL nếu khoảng cách giữa các cuộc gọi không rõ.

FAIL nếu IVR tự gọi lại vô hạn.

FAIL nếu mỗi lần no answer đều gửi tin nhắn/hủy riêng.

FAIL nếu hết attempts nhưng IVR tự gửi SMS mà chưa qua Core Order State Machine.

## 10. CORE ORDER STATE MACHINE CONTROL

## 10.1. IVR không tự hủy đơn

IVR không được tự hủy đơn.

IVR chỉ gửi outcome về Core Order State Machine.

Core Order State Machine mới là nơi quyết định:

Đơn tiếp tục xử lý.

Đơn chờ xác minh lại.

Đơn cần human review.

Đơn bị hủy do khách chủ động hủy.

Đơn bị hủy do không xác nhận được.

Đơn bị hold do dữ liệu không rõ.

Đơn bị block do rủi ro phá hoại.

## 10.2. IVR không tự xác nhận đơn hoàn tất

IVR không được tự chuyển đơn sang completed.

IVR không được tự chuyển đơn sang delivered.

IVR không được tự chuyển đơn sang paid.

IVR không được tự chuyển đơn sang verified revenue.

IVR chỉ có thể đóng góp evidence cho trạng thái confirmed theo policy.

## 10.3. No Answer sau đủ attempts

Nếu máy đổ chuông nhưng khách không nghe sau đủ attempts hợp lệ:

IVR ghi nhận outcome = NO_ANSWER_MAX_ATTEMPT_REACHED.

IVR gửi outcome về Core Order State Machine.

Core Order State Machine quyết định hủy hoặc chuyển human review tùy policy.

Chỉ sau khi Core Order State Machine hủy đơn chính thức, Notification Owner mới được gửi thông báo giao dịch.

IVR không tự nhắn ngay sau từng cuộc gọi không nghe.

SIM Gateway không tự gửi tin nhắn.

AI Advisor không tự phát tin nhắn sau hủy.

## 11. CUSTOMER OUTCOME PRINCIPLE

## 11.1. Customer Confirm

Nếu khách xác nhận qua IVR:

Outcome phải ghi rõ CUSTOMER_CONFIRMED.

Evidence phải đủ.

Thời điểm xác nhận phải được ghi nhận.

Attempt number phải được ghi nhận.

Core Order State Machine nhận kết quả để xử lý bước tiếp theo.

Payment status không thay đổi nếu chưa có Payment Confirmation.

Verified Revenue không phát sinh từ IVR confirmation.

## 11.2. Customer Cancel

Nếu khách chủ động bấm hủy qua IVR:

Outcome phải ghi rõ CUSTOMER_CANCELLED_VIA_IVR.

Không được ghi là no answer.

Không được ghi là invalid phone.

Không được ghi là technical failure.

Core Order State Machine nhận outcome và quyết định trạng thái hủy.

Evidence phải đủ.

Nếu cần human review, phải có policy rõ.

## 11.3. Customer Need Support

Nếu khách chọn cần hỗ trợ:

Outcome phải ghi rõ CUSTOMER_NEED_SUPPORT.

Đơn không được tự hủy.

Đơn không được tự xác nhận.

Phải route sang CSKH/Human/Admin theo policy.

Không được tự động upsell.

Không được tự động gửi marketing.

Không được lộ dữ liệu riêng tư qua kênh sai.

## 11.4. Technical Failure

Nếu lỗi kỹ thuật:

Outcome phải ghi rõ TECHNICAL_FAILURE.

Không được quy lỗi cho khách.

Không được tính là attempt hợp lệ nếu policy quy định attempt lỗi không hợp lệ.

Không được hủy đơn nếu chỉ có lỗi kỹ thuật.

Phải route retry/human review theo policy.

Evidence lỗi kỹ thuật phải được lưu.

## 12. NOTIFICATION OWNER HANDOFF PRINCIPLE

## 12.1. IVR không sở hữu notification sau hủy

IVR không được tự gửi thông báo sau khi không xác nhận được đơn.

SIM Gateway không được tự gửi thông báo sau từng cuộc gọi thất bại.

AI Advisor không được tự phát tin nhắn hủy đơn.

Notification Owner mới là bên gửi thông báo giao dịch sau khi Core Order State Machine đã hủy đơn chính thức.

## 12.2. Điều kiện gửi thông báo sau hủy

Chỉ được gửi thông báo sau hủy khi có đủ:

Core Order State Machine đã chuyển đơn sang trạng thái hủy chính thức.

Lý do hủy đã rõ.

Outcome IVR đã có evidence.

Notification Owner nhận handoff hợp lệ.

Nội dung thông báo đã qua privacy-safe policy.

Không có suppression/opt-out/legal block cấm gửi.

Không chứa thông tin nhạy cảm vượt quyền.

## 12.3. Nội dung thông báo sau hủy

Thông báo sau hủy là tin nhắn giao dịch.

Không phải CRM.

Không phải marketing.

Không phải upsell.

Không phải quảng cáo.

Không phải chăm sóc bán thêm.

Thông báo có thể nói ngắn gọn:

Đơn đã hết hiệu lực hoặc đã bị hủy do không xác nhận được qua cuộc gọi tự động.

Khách có thể phản hồi để tạo đơn mới theo chương trình hiện hành.

Không cam kết giữ giá cũ nếu quote/order window đã hết hạn.

Không nhắc đầy đủ địa chỉ.

Không nhắc payment detail.

Không nhắc member tier.

Không nhắc Diamond/referral.

Không nhắc health note.

Không nhắc thông tin riêng tư không cần thiết.

## 12.4. P0 điểm chặn

FAIL nếu IVR tự gửi SMS hủy đơn.

FAIL nếu SIM Gateway tự gửi tin nhắn hủy sau mỗi lần gọi không nghe.

FAIL nếu AI Advisor tự nhắn hủy đơn khi Core Order State Machine chưa hủy.

FAIL nếu thông báo sau hủy chứa full address/payment/member/Diamond/health note.

FAIL nếu thông báo sau hủy được dùng để upsell.

## 13. PRIVACY-SAFE CALL RESULT PRINCIPLE

## 13.1. IVR phải tối thiểu hóa dữ liệu đọc qua cuộc gọi

IVR không được đọc toàn bộ dữ liệu riêng tư nếu không có policy cho phép.

Theo mặc định, IVR không đọc:

Full address.

Payment detail.

Member tier.

Diamond/referral benefit.

Health note.

Ghi chú cá nhân.

Lịch sử mua hàng.

Thông tin người nhận ngoài phạm vi cần xác nhận.

Thông tin nhạy cảm của khách.

Nội dung tư vấn sức khỏe.

## 13.2. IVR chỉ xác nhận thông tin cơ bản theo policy

IVR có thể xác nhận ở mức tối thiểu:

Khách có đặt đơn không.

Khách có đồng ý nhận hàng không.

Sản phẩm/nhóm sản phẩm ở mức phù hợp policy.

Số lượng cơ bản nếu cần.

Tỉnh/thành hoặc thông tin nhận hàng rút gọn nếu cần.

Lựa chọn xác nhận / hủy / cần hỗ trợ.

## 13.3. Public/private boundary

Kết quả IVR là dữ liệu vận hành nội bộ.

Không public kết quả IVR.

Không đưa kết quả IVR ra comment public.

Không để MC AI Live đọc kết quả IVR.

Không để Ads dùng kết quả IVR làm revenue.

Không để AI Advisor suy luận vượt policy từ kết quả IVR.

## 14. EVIDENCE / CALL LOG / AUDIT PRINCIPLE

## 14.1. Mọi kết quả IVR phải có evidence

Không có evidence thì không được dùng kết quả IVR làm căn cứ trạng thái đơn.

Evidence tối thiểu gồm:

Order identity.

Customer identity reference.

Phone reference.

Attempt number.

Call start time.

Call end time hoặc failure time.

Call status.

Outcome classification.

Provider/telco result nếu có.

Policy version.

Correlation id.

Actor/system identity.

Audit timestamp.

Evidence integrity status.

## 14.2. Evidence không đồng nghĩa audio public

Nếu có recording, recording phải tuân thủ privacy/legal policy.

Nếu không lưu recording, call log và provider outcome phải đủ audit theo policy.

Không được public recording.

Không được đưa recording cho AI Advisor xử lý tự do.

Không được dùng recording cho marketing.

## 14.3. Audit bắt buộc

Mọi thay đổi trạng thái từ IVR outcome phải có audit.

Mọi admin override phải có audit.

Mọi human review phải có audit.

Mọi technical failure phải có audit.

Mọi notification handoff sau hủy phải có audit.

## 15. HUMAN / ADMIN OVERRIDE PRINCIPLE

## 15.1. Khi nào cần Human/Admin Review

Human/Admin Review được kích hoạt khi:

Outcome không rõ.

Call result mâu thuẫn.

Technical failure lặp lại.

Khách cần hỗ trợ.

Customer cancel nhưng có dấu hiệu nhầm.

Đơn giá trị cao.

Đơn có rủi ro phá hoại.

Khách cũ nhưng thông tin thay đổi bất thường.

Provider result thiếu evidence.

Policy không đủ quyết định tự động.

## 15.2. Admin Override không được tùy tiện

Admin Override phải có:

Lý do.

Evidence.

Người thực hiện.

Thời điểm.

Trạng thái trước/sau.

Policy reference.

Audit log.

Nếu override ảnh hưởng hủy/xác nhận đơn, phải có owner review theo policy.

## 15.3. P0 điểm chặn

FAIL nếu admin override không có audit.

FAIL nếu human review xử lý bằng cảm tính không có evidence.

FAIL nếu override dùng để bỏ qua Sale Lock / Recall / Suppression.

FAIL nếu override biến IVR confirmation thành PAID.

## 16. RUNTIME UNAVAILABLE FAIL-SAFE

## 16.1. Khi runtime không sẵn sàng

Nếu một trong các nguồn sau không sẵn sàng, IVR phải fail-safe:

Commerce Runtime.

Order State Machine.

Customer Trust Resolver.

Call Attempt Policy Resolver.

Phone Validation Resolver.

Anti-Fake-Order Resolver.

Notification Owner Handoff.

Evidence/Audit system.

Privacy Policy Resolver.

Suppression/Sale Lock/Recall check.

## 16.2. Fail-safe nghĩa là không tự suy đoán

Khi runtime unavailable:

Không tự gọi nếu chưa đủ eligibility.

Không tự xác nhận.

Không tự hủy.

Không tự gửi tin nhắn.

Không tự đánh dấu khách không nghe.

Không tự đánh dấu số invalid.

Không tự chuyển revenue.

Không tự bypass Core Order State Machine.

Route sang retry/human review theo policy.

## 16.3. P0 điểm chặn

FAIL nếu runtime unavailable nhưng IVR vẫn tự hủy đơn.

FAIL nếu thiếu evidence nhưng vẫn dùng kết quả.

FAIL nếu không gọi được policy mà hệ thống tự chọn rule mặc định gây rủi ro.

## 17. GLOBAL DEPENDENCY CỦA TECH-09

TECH-09 phụ thuộc các lớp đã khóa:

## TECH-00 - Global Technical Governance
Quy tắc source-of-truth, evidence, smoke, owner sign-off, release decision.

## TECH-01 - Foundation / RBAC / Audit / Idempotency / Evidence Registry
Quy tắc audit, evidence, quyền, idempotency, owner action.

## TECH-02 - Product / SKU / Recipe / Activation
Quy tắc product active không đồng nghĩa sellable.

## TECH-03 - Operational Core
Quy tắc inventory, sale lock, recall, traceability, warehouse, production.

## TECH-04 - Commerce Runtime
Quy tắc Quote / Cart / Order Draft / Official Order / Payment / Delivery / Verified Revenue.

## TECH-05 - AI Advisor Runtime
Quy tắc AI không tự tạo official order, không tự xác nhận payment, không tự phát response vượt guard.

## TECH-06 - Facebook Gateway
Quy tắc public/private, Messenger handoff, delivery control, rate limit, suppression.

## TECH-07 - Ads Measurement
Quy tắc IVR không phải revenue, không dùng IVR confirmation làm ROAS.

## TECH-08 - MC AI Live
Quy tắc Live không tự xác nhận đơn, không dùng live signal làm revenue, không bypass Commerce/IVR.

## 18. P0 điểm chặn REGISTRY - PHẦN 1/4

Các lỗi sau là P0 điểm chặn:

IVR gọi cho mọi khách hàng.

IVR gọi khách trusted đại trà.

IVR gọi cho Quote.

IVR gọi cho Cart.

IVR gọi cho Order Draft.

IVR xử lý đơn không phải Official Order.

IVR tự tạo order.

IVR tự sửa tiền.

IVR tự xác nhận payment.

IVR tự xác nhận Verified Revenue.

IVR confirmation bị hiểu là PAID.

IVR confirmation bị hiểu là revenue.

Invalid phone bị trộn với no answer.

No answer bị trộn với technical failure.

Customer cancel bị trộn với no answer.

Technical failure bị quy lỗi khách.

Không có MAX_ATTEMPT_PER_ORDER rõ.

IVR gọi lại vô hạn.

IVR tự hủy đơn.

IVR tự gửi tin nhắn sau từng cuộc gọi không nghe.

SIM Gateway tự gửi tin nhắn hủy đơn.

AI Advisor tự phát tin nhắn hủy đơn.

Notification gửi trước khi Core Order State Machine hủy chính thức.

Tin nhắn sau hủy chứa full address/payment/member/Diamond/health note.

Tin nhắn sau hủy dùng để marketing/upsell.

Runtime unavailable nhưng IVR vẫn tự xác nhận hoặc tự hủy.

Không có call log/evidence nhưng vẫn dùng outcome.

Human/Admin override không có audit.

IVR bypass Sale Lock / Recall / Suppression.

Documentation Complete bị gọi nhầm là Production Ready.

## 19. EVIDENCE YÊU CẦU CHO PHẦN 1/4

Để PHẦN 1/4 được coi là đạt documentation gate, cần có evidence sau ở cấp tài liệu:

Boundary IVR đã được mô tả rõ.

No-Mass-Calling Rule đã được khóa.

Customer eligibility đã được khóa.

Trusted customer exclusion đã được khóa.

Order Draft vs Official Order boundary đã được khóa.

IVR không thay Commerce/Payment/Verified Revenue đã được khóa.

Invalid phone / no answer / customer cancel / technical failure đã được tách.

MAX_ATTEMPT_PER_ORDER baseline đã được khóa.

Golden Hour / 24/7 timing context đã được nêu.

Notification Owner Handoff đã được khóa.

Privacy-safe notification đã được khóa.

Runtime unavailable fail-safe đã được khóa.

Human/Admin override evidence/audit đã được khóa.

P0 điểm chặn Registry đã có.

Release Handoff sang PHẦN 2/4 đã rõ.

## 20. SMOKE ĐỊNH HƯỚNG CHO PHẦN 1/4

Các smoke ở PHẦN 1/4 chưa phải smoke execution cuối cùng, nhưng là smoke định hướng để sang PHẦN 4/4 chi tiết hóa.

IVR-SMK-001 - New Customer Eligible

Given khách mới, chưa có lịch sử mua hàng, official order đủ điều kiện
When Commerce gửi eligibility sang IVR
Then IVR được phép đưa đơn vào call confirmation queue theo policy.

IVR-SMK-002 - Trusted Customer Excluded

Given khách cũ, trusted, thông tin nhận hàng ổn định, không có rủi ro mới
When order được tạo
Then IVR không gọi đại trà.

IVR-SMK-003 - Quote Not Callable

Given chỉ có quote
When IVR nhận request gọi
Then hệ thống phải block vì quote không phải official order.

IVR-SMK-004 - Order Draft Not Callable

Given chỉ có order draft
When IVR nhận request gọi
Then hệ thống phải block vì order draft chưa phải official order.

IVR-SMK-005 - Invalid Phone Separated

Given số điện thoại sai định dạng hoặc không thể quay gọi
When phone validation chạy
Then outcome phải là INVALID_PHONE, không phải NO_ANSWER.

IVR-SMK-006 - No Answer Separated

Given máy đổ chuông hợp lệ nhưng khách không nghe
When đủ attempts hợp lệ
Then outcome là NO_ANSWER_MAX_ATTEMPT_REACHED, không phải INVALID_PHONE.

IVR-SMK-007 - Technical Failure Not Customer Fault

Given provider lỗi kỹ thuật
When call không thực hiện được
Then outcome là TECHNICAL_FAILURE, không được quy lỗi khách.

IVR-SMK-008 - Customer Cancel Separated

Given khách bấm hủy qua IVR
When call outcome được ghi nhận
Then outcome là CUSTOMER_CANCELLED_VIA_IVR, không phải NO_ANSWER.

IVR-SMK-009 - Notification Only After Core Cancellation

Given khách không nghe sau đủ attempts
When IVR gửi outcome về Core Order State Machine
Then Notification Owner chỉ được gửi tin nhắn sau khi Core Order State Machine hủy đơn chính thức.

IVR-SMK-010 - Privacy-Safe Cancellation Notice

Given đơn đã bị hủy chính thức do không xác nhận được
When Notification Owner gửi thông báo
Then nội dung không chứa full address/payment/member/Diamond/health note và không dùng để upsell.

## 21. DONE GATE - PHẦN 1/4

## PHẦN 1/4 được xem là Documentation Complete ở cấp nguyên tắc khi đạt đủ:

Đã khóa IVR Runtime Boundary.

Đã khóa IVR Source-of-Truth Boundary.

Đã khóa No-Mass-Calling Rule.

Đã khóa Customer Eligibility Boundary.

Đã khóa Trusted Customer Exclusion.

Đã khóa Order Draft vs Official Order Boundary.

Đã khóa IVR không thay Commerce/Payment/Verified Revenue.

Đã khóa call outcome separation.

Đã khóa baseline MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED.

Đã khóa Golden Hour / 24/7 timing context ở mức nguyên tắc.

Đã khóa Core Order State Machine control.

Đã khóa Notification Owner Handoff.

Đã khóa privacy-safe cancellation notification.

Đã khóa evidence/call log/audit principle.

Đã khóa Human/Admin Override principle.

Đã khóa runtime unavailable fail-safe.

Đã có P0 điểm chặn Registry.

Đã có smoke định hướng.

Đã sẵn sàng chuyển sang PHẦN 2/4 - IVR Module Contracts.

## 22. FAIL GATE - PHẦN 1/4

## PHẦN 1/4 FAIL nếu còn bất kỳ điểm nào sau:

IVR bị hiểu là gọi cho mọi khách hàng.

IVR bị hiểu là công cụ marketing/cskh đại trà.

IVR được phép gọi Quote/Cart/Order Draft.

IVR được phép tự tạo order.

IVR được phép tự xác nhận thanh toán.

IVR confirmation bị hiểu là PAID.

IVR confirmation bị hiểu là Verified Revenue.

Invalid phone/no answer/technical failure/customer cancel chưa tách rõ.

MAX_ATTEMPT_PER_ORDER chưa rõ.

Timing Golden Hour/24/7 bị lẫn.

IVR được phép tự hủy đơn.

IVR được phép tự gửi SMS sau từng attempt.

Notification Owner Handoff chưa rõ.

Privacy-safe notification chưa rõ.

Runtime unavailable chưa có fail-safe.

Human/Admin override chưa có evidence/audit.

Không có P0 điểm chặn Registry.

Không có smoke định hướng.

Tài liệu gọi Documentation Complete là Production Ready.

## 23. RELEASE HANDOFF - SANG PHẦN 2/4

## PHẦN 1/4 bàn giao sang PHẦN 2/4 các nguyên tắc đã khóa để triển khai module contract.

## PHẦN 2/4 phải viết module contract cho các khối sau:

IVR Runtime Orchestrator.

IVR Eligibility Resolver.

Order Confirmation State Connector.

Customer Trust Resolver.

Call Attempt Policy Resolver.

Phone Validation Resolver.

Call Outcome Classifier.

Customer Confirmation Resolver.

Customer Cancel Resolver.

No Answer Resolver.

Technical Failure Resolver.

Anti-Fake-Order Resolver.

Notification Handoff Resolver.

Human/Admin Override.

Evidence / Call Log / Audit.

Release Handoff.

Mỗi module ở PHẦN 2/4 phải có:

Scope In.

Scope Out.

Upstream Dependency.

Downstream Handoff.

P0 điểm chặn.

Evidence.

Smoke.

## 24. TRẠNG THÁI SAU PHẦN 1/4

Sau PHẦN 1/4:

Không được gọi là Production Ready.

Không được gọi là Release Ready.

Không được gọi là Go-live Approved.

Không được gọi là IVR Ready.

Không được gọi là Anti-Fake-Order Ready.

Global Gateway vẫn mặc định:

bị chặn

Chỉ khi có đủ:

## PHẦN 2/4 module contracts.

## PHẦN 3/4 lifecycle/state machine/attempt policy/outcome classification.

## PHẦN 4/4 smoke matrix/evidence/done gate/release handoff.

Accepted evidence.

Smoke pass.

Owner sign-off.

Release decision.

thì mới được xét các trạng thái cao hơn.

## KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 đã khóa vai trò IVR trong Ginsengfood là lớp gọi tự động xác nhận đơn có điều kiện, phục vụ chống đơn ảo và kiểm soát rủi ro đơn hàng.

IVR không gọi đại trà.

IVR không gọi cho Quote, Cart, Order Draft.

IVR chỉ xử lý Official Order đủ điều kiện từ Commerce.

IVR không tự tạo đơn, không sửa tiền, không xác nhận thanh toán, không ghi nhận Verified Revenue.

IVR confirmation chỉ xác nhận ý chí đặt hàng và thông tin cơ bản theo policy.

Invalid phone, no answer, customer cancel và technical failure phải tách rõ.

Máy đổ chuông nhưng khách không nghe sau đủ attempts không được tự nhắn/hủy ngay từng cuộc.

Chỉ sau khi Core Order State Machine hủy đơn chính thức thì Notification Owner mới được gửi tin nhắn giao dịch privacy-safe.

TECH-09 V1.0 khóa baseline MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED: Golden Hour 2 attempts / 10 phút; 24/7 3 attempts / 15 phút. Không chỉnh sửa tạm lẻ theo từng module.

## PHẦN 1/4 sẵn sàng bàn giao sang:

## PHẦN 2/4 - IVR MODULE CONTRACTS.

## PHẦN 2/4 - IVR MODULE CONTRACTS

## 1. MỤC TIÊU PHẦN 2/4

## PHẦN 2/4 khóa các module nghiệp vụ bắt buộc của lớp IVR Order Confirmation.

Mục tiêu là để dev hiểu rõ từng module:

Module đó chịu trách nhiệm gì.

Module đó không được làm gì.

Module đó nhận dữ liệu từ đâu.

Module đó bàn giao kết quả cho ai.

Khi nào module phải chặn xử lý.

Evidence tối thiểu cần có.

Smoke bắt buộc phải pass.

## PHẦN 2/4 không viết code.

## PHẦN 2/4 không thiết kế API chi tiết.

## PHẦN 2/4 không thiết kế database chi tiết.

## PHẦN 2/4 không thiết kế UI chi tiết.

## PHẦN 2/4 chỉ khóa module boundary và nghiệp vụ bắt buộc để sang PHẦN 3/4 viết lifecycle/state machine/attempt policy.

## 2. NGUYÊN TẮC MODULE CONTRACT CHUNG

Mỗi module trong TECH-09 phải tuân thủ các nguyên tắc sau:

Không tự suy đoán.

Không hardcode chính sách nghiệp vụ.

Không bypass Commerce.

Không bypass Core Order State Machine.

Không bypass Customer Trust Policy.

Không bypass Sale Lock / Recall / Suppression.

Không tự tạo order.

Không tự sửa giá.

Không tự xác nhận thanh toán.

Không tự ghi nhận doanh thu.

Không tự gửi tin nhắn sau hủy.

Không gọi đại trà cho toàn bộ khách.

Không trộn invalid phone với no answer.

Không trộn customer cancel với no answer.

Không trộn technical failure với lỗi khách.

Không dùng outcome thiếu evidence để quyết định trạng thái đơn.

Không gọi Documentation Complete là Production Ready.

## 3. MODULE CONTRACT 01 - IVR RUNTIME ORCHESTRATOR

## 3.1. Mục tiêu

IVR Runtime Orchestrator là module điều phối trung tâm của IVR.

Module này nhận official order đủ điều kiện từ Commerce/Core Order layer, điều phối các resolver liên quan, quyết định có tạo call attempt hay không, nhận call outcome và bàn giao kết quả về Core Order State Machine.

## 3.2. Scope In

IVR Runtime Orchestrator được phép:

Nhận yêu cầu xác minh official order.

Kiểm tra order có đủ điều kiện vào IVR hay không thông qua IVR Eligibility Resolver.

Gọi Customer Trust Resolver để xác định khách mới/untrusted/trusted.

Gọi Call Attempt Policy Resolver để xác định số lần gọi, spacing, call window.

Gọi Phone Validation Resolver trước khi tạo attempt.

Gửi yêu cầu thực hiện call attempt đến IVR provider/adapter.

Nhận kết quả cuộc gọi.

Gọi Call Outcome Classifier để phân loại kết quả.

Gửi outcome đã phân loại về Order Confirmation State Connector.

Ghi nhận evidence/call log/audit.

Kích hoạt Human/Admin Review nếu outcome không đủ rõ.

Dừng xử lý khi runtime hoặc policy không sẵn sàng.

## 3.3. Scope Out

IVR Runtime Orchestrator không được:

Tạo official order.

Tạo quote.

Tạo cart.

Tạo order draft.

Sửa giá.

Sửa khuyến mãi.

Sửa quyền lợi thành viên.

Sửa Diamond/referral benefit.

Xác nhận PAID.

Xác nhận COD success.

Xác nhận Verified Revenue.

Hủy đơn trực tiếp.

Gửi SMS/thông báo sau hủy.

Gọi khách không đủ eligibility.

Gọi lại vô hạn.

Tự chọn policy nếu policy resolver lỗi.

Đọc dữ liệu riêng tư vượt privacy policy.

## 3.4. Upstream Dependency

Module này phụ thuộc:

Commerce Runtime.

Official Order Source.

Core Order State Machine.

Customer Trust Resolver.

IVR Eligibility Resolver.

Call Attempt Policy Resolver.

Phone Validation Resolver.

Anti-Fake-Order Resolver.

Suppression/Sale Lock/Recall Check.

Evidence / Audit Registry.

## 3.5. Downstream Handoff

Module này bàn giao cho:

Call Provider / IVR Adapter.

Call Outcome Classifier.

Order Confirmation State Connector.

Evidence / Call Log / Audit.

Human/Admin Review.

Notification Handoff Resolver, nhưng chỉ sau khi Core Order State Machine có trạng thái phù hợp.

## 3.6. P0 điểm chặn

FAIL nếu IVR Runtime Orchestrator tự hủy đơn.

FAIL nếu IVR Runtime Orchestrator tự xác nhận thanh toán.

FAIL nếu IVR Runtime Orchestrator tự xác nhận Verified Revenue.

FAIL nếu IVR Runtime Orchestrator gọi khách khi chưa có eligibility.

FAIL nếu IVR Runtime Orchestrator tiếp tục gọi khi policy unavailable.

FAIL nếu IVR Runtime Orchestrator gọi Quote/Cart/Order Draft.

## 3.7. Evidence

Evidence tối thiểu:

Order reference.

Eligibility decision reference.

Customer trust decision.

Attempt policy version.

Phone validation result.

Call attempt record.

Call outcome classification.

Handoff record về Core Order State Machine.

Audit record.

Runtime error record nếu có.

## 3.8. Smoke

## IVR-MOD-SMK-001

Given official order đủ điều kiện IVR
When IVR Runtime Orchestrator nhận order
Then module phải gọi eligibility, trust, attempt policy, phone validation trước khi tạo call attempt.

## IVR-MOD-SMK-002

Given order draft
When IVR Runtime Orchestrator nhận request
Then module phải block và không tạo call attempt.

## 4. MODULE CONTRACT 02 - IVR ELIGIBILITY RESOLVER

## 4.1. Mục tiêu

IVR Eligibility Resolver quyết định official order có được đưa vào IVR hay không.

Module này là chốt chặn để đảm bảo IVR không gọi đại trà.

## 4.2. Scope In

Module được phép xác định:

Order có phải official order không.

Order có đến từ Commerce hợp lệ không.

Order có nằm trong trạng thái được phép gọi IVR không.

Customer có thuộc nhóm cần xác minh không.

Order có rủi ro anti-fake-order không.

Order có bị Sale Lock / Recall / Suppression không.

Order có còn trong call window không.

Order đã từng được xác nhận/hủy/chặn chưa.

Order đã đạt max attempt chưa.

Có cần route sang Human Review thay vì gọi IVR không.

## 4.3. Scope Out

Module không được:

Tự gọi khách.

Tự tạo attempt.

Tự hủy đơn.

Tự xác nhận đơn.

Tự xác nhận payment.

Tự xác nhận revenue.

Tự sửa trust score.

Tự sửa customer profile.

Tự bỏ qua khách trusted nếu không có policy.

Tự thêm khách vào blacklist nếu chưa có anti-fake-order evidence.

## 4.4. Upstream Dependency

Phụ thuộc:

Commerce Official Order.

Order status.

Customer Trust Resolver.

Anti-Fake-Order Resolver.

Sale Lock / Recall / Suppression check.

Call Attempt Policy Resolver.

Runtime policy registry.

## 4.5. Downstream Handoff

Bàn giao:

Eligibility decision: ELIGIBLE / NOT_ELIGIBLE / NEED_HUMAN_REVIEW / bị chặn.

Reason code.

Policy version.

Evidence reference.

Decision timestamp.

Handoff về IVR Runtime Orchestrator.

## 4.6. P0 điểm chặn

FAIL nếu Quote được đánh dấu eligible.

FAIL nếu Cart được đánh dấu eligible.

FAIL nếu Order Draft được đánh dấu eligible.

FAIL nếu khách trusted không có rủi ro vẫn bị eligible đại trà.

FAIL nếu Sale Lock / Recall order vẫn được eligible.

FAIL nếu eligibility không có reason code.

## 4.7. Evidence

Evidence tối thiểu:

Order type.

Order status.

Customer trust status.

Risk signal nếu có.

Suppression check.

Sale Lock/Recall check.

Eligibility result.

Reason code.

Policy version.

Audit log.

## 4.8. Smoke

## IVR-MOD-SMK-003

Given khách mới, official order hợp lệ
When eligibility resolver chạy
Then result = ELIGIBLE nếu không có điểm chặn.

## IVR-MOD-SMK-004

Given khách trusted không có rủi ro mới
When eligibility resolver chạy
Then result = NOT_ELIGIBLE_FOR_IVR.

## 5. MODULE CONTRACT 03 - ORDER CONFIRMATION STATE CONNECTOR

## 5.1. Mục tiêu

Order Confirmation State Connector là cầu nối giữa IVR outcome và Core Order State Machine.

Module này đảm bảo IVR không tự hủy, không tự xác nhận hoàn tất, không tự chuyển payment/revenue.

## 5.2. Scope In

Module được phép:

Nhận outcome từ IVR.

Kiểm tra outcome có evidence hợp lệ không.

Kiểm tra order hiện còn ở trạng thái được phép nhận IVR outcome không.

Gửi outcome vào Core Order State Machine.

Ghi nhận state transition request.

Nhận kết quả state transition từ Core Order State Machine.

Bàn giao notification handoff nếu Core Order State Machine đã hủy chính thức.

Route human review nếu outcome không đủ rõ.

## 5.3. Scope Out

Module không được:

Tự quyết định hủy đơn ngoài Core Order State Machine.

Tự quyết định xác nhận đơn ngoài Core Order State Machine.

Tự chuyển PAID.

Tự chuyển Verified Revenue.

Tự chuyển Delivery Success.

Tự gửi notification.

Tự sửa call outcome.

Tự tạo outcome nếu IVR chưa có evidence.

## 5.4. Upstream Dependency

Phụ thuộc:

IVR Runtime Orchestrator.

Call Outcome Classifier.

Evidence / Call Log.

Core Order State Machine.

Commerce Order Status.

Policy Registry.

## 5.5. Downstream Handoff

Bàn giao:

Core Order State Machine.

Notification Handoff Resolver nếu đơn đã hủy chính thức.

Human/Admin Review nếu cần.

Evidence/Audit Registry.

## 5.6. P0 điểm chặn

FAIL nếu connector tự hủy đơn.

FAIL nếu connector tự xác nhận PAID.

FAIL nếu connector nhận outcome thiếu evidence.

FAIL nếu connector gửi notification khi Core Order State Machine chưa hủy.

FAIL nếu connector chuyển no answer thành cancelled mà không qua Core Order State Machine.

## 5.7. Evidence

Evidence tối thiểu:

Outcome received.

Outcome evidence reference.

Order state before.

State machine request.

State machine result.

Order state after.

Actor/system identity.

Correlation id.

Audit log.

## 5.8. Smoke

## IVR-MOD-SMK-005

Given outcome CUSTOMER_CONFIRMED có evidence
When connector gửi vào Core Order State Machine
Then order chỉ được chuyển theo rule của Core Order State Machine, không chuyển PAID.

## IVR-MOD-SMK-006

Given NO_ANSWER_MAX_ATTEMPT_REACHED
When connector nhận outcome
Then connector không tự gửi SMS, chỉ bàn giao state machine xử lý.

## 6. MODULE CONTRACT 04 - CUSTOMER TRUST RESOLVER

## 6.1. Mục tiêu

Customer Trust Resolver xác định khách hàng thuộc nhóm NEW, UNTRUSTED, PARTIALLY_TRUSTED hoặc TRUSTED.

Module này giúp IVR tránh gọi đại trà khách đã đủ tin cậy.

## 6.2. Scope In

Module được phép đánh giá:

Khách mới hay khách cũ.

Có lịch sử mua hàng thành công không.

Có hồ sơ thành viên xác thực không.

Số điện thoại đã xác minh chưa.

Thông tin nhận hàng có trùng lịch sử đáng tin cậy không.

Có lịch sử hủy/không nhận hàng bất thường không.

Có tín hiệu rủi ro mới không.

Đơn hiện tại có thay đổi người nhận/địa chỉ/số điện thoại không.

Customer Memory có đủ dữ liệu để loại trừ IVR không.

Có cần Human Review không.

## 6.3. Scope Out

Module không được:

Tự gọi khách.

Tự xác nhận order.

Tự bỏ qua IVR nếu có tín hiệu rủi ro P0.

Tự thêm khách vào blacklist.

Tự xác nhận thanh toán.

Tự xác nhận revenue.

Tự public dữ liệu customer.

Tự sửa lịch sử mua hàng.

Tự hardcode trusted status theo tên hoặc số điện thoại.

## 6.4. Upstream Dependency

Phụ thuộc:

Customer Profile.

Customer Memory.

Commerce order history.

Payment/Delivery outcome history.

Anti-Fake-Order Resolver.

Membership/Profile verification policy.

Privacy policy.

## 6.5. Downstream Handoff

Bàn giao:

Trust classification.

Trust reason code.

IVR exclusion reason nếu trusted.

Risk escalation reason nếu untrusted.

Evidence reference.

Handoff về Eligibility Resolver.

## 6.6. P0 điểm chặn

FAIL nếu trusted customer bị gọi đại trà.

FAIL nếu khách rủi ro bị đánh trusted không có evidence.

FAIL nếu trust status hardcode.

FAIL nếu Customer Trust Resolver đọc/ghi dữ liệu vượt privacy policy.

## 6.7. Evidence

Evidence tối thiểu:

Customer status.

Purchase history reference.

Phone verification status.

Delivery success reference nếu có.

Risk signal nếu có.

Trust classification.

Reason code.

Policy version.

Audit log.

## 6.8. Smoke

## IVR-MOD-SMK-007

Given khách cũ có lịch sử nhận hàng tốt
When Customer Trust Resolver chạy
Then result = TRUSTED và eligibility loại khỏi IVR nếu không có rủi ro mới.

## IVR-MOD-SMK-008

Given khách cũ nhưng đổi số điện thoại/người nhận bất thường
When Customer Trust Resolver chạy
Then result không được auto TRUSTED; phải route risk/IVR/human review theo policy.

## 7. MODULE CONTRACT 05 - CALL ATTEMPT POLICY RESOLVER

## 7.1. Mục tiêu

Call Attempt Policy Resolver xác định chính sách gọi cho từng official order đủ điều kiện.

Module này khóa số lần gọi, khoảng cách gọi, call window, attempt hợp lệ, attempt không hợp lệ và điều kiện dừng gọi.

## 7.2. Scope In

Module được phép xác định:

## MAX_ATTEMPT_PER_ORDER.

Attempt spacing.

Call window.

Attempt timeout.

Attempt nào được tính là hợp lệ.

Attempt nào không được tính vì lỗi kỹ thuật.

Khi nào dừng gọi.

Khi nào chuyển Human Review.

Khi nào không được gọi tiếp.

Chính sách theo Golden Hour.

Chính sách theo 24/7.

Chính sách khi quote/order window hết hạn.

## 7.3. Scope Out

Module không được:

Tự gọi khách.

Tự hủy đơn.

Tự xác nhận đơn.

Tự gửi notification.

Tự đổi số lần gọi mà không có policy version mới.

Tự áp attempt policy sai chương trình.

Tự kéo dài quote freeze window.

Tự giữ giá quá Commerce policy.

Tự cho gọi ngoài call window.

Tự bỏ qua opt-out/suppression.

## 7.4. Baseline Policy

TECH-09 V1.0 khóa baseline:

## MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED.

Golden Hour tham chiếu: 10 phút, 2 cuộc, cách 5 phút.

24/7 tham chiếu: 15 phút, 3 cuộc, lịch T0 / T0 + 5 phút / T0 + 10 phút.

24/7 dùng 3 attempts ngay trong V1.0 theo program-based policy.

Không được áp một max attempt chung cho mọi chương trình hoặc áp sai attempt policy theo chương trình.

## 7.5. Upstream Dependency

Phụ thuộc:

Commerce program context.

Quote/order timing context.

Order created time.

Current order status.

Suppression/opt-out policy.

IVR policy registry.

Customer risk policy.

## 7.6. Downstream Handoff

Bàn giao:

Attempt policy decision.

Max attempt.

Attempt spacing.

Call window.

Attempt validity rule.

Stop condition.

Policy version.

Handoff về IVR Runtime Orchestrator.

## 7.7. P0 điểm chặn

FAIL nếu MAX_ATTEMPT_PER_ORDER không rõ.

FAIL nếu attempt spacing không rõ.

FAIL nếu IVR gọi vô hạn.

FAIL nếu program-based attempt rule bị áp sai chương trình.

FAIL nếu Golden Hour và 24/7 timing bị lẫn.

FAIL nếu attempt lỗi kỹ thuật bị tính như khách không nghe mà không có policy.

## 7.8. Evidence

Evidence tối thiểu:

Program context.

Policy version.

Max attempt.

Spacing.

Call window.

Attempt validity rule.

Stop condition.

Decision timestamp.

Audit log.

## 7.9. Smoke

## IVR-MOD-SMK-009

Given order thuộc Golden Hour
When Call Attempt Policy Resolver chạy
Then policy trả về 2 attempts, spacing 5 phút, call window 10 phút theo Golden Hour baseline.

## IVR-MOD-SMK-010

Given order thuộc 24/7
When Call Attempt Policy Resolver chạy
Then policy trả về 3 attempts, spacing 5 phút, call window 15 phút theo 24/7 baseline.

## 8. MODULE CONTRACT 06 - PHONE VALIDATION RESOLVER

## 8.1. Mục tiêu

Phone Validation Resolver kiểm tra số điện thoại trước khi IVR tạo call attempt.

Module này tách rõ invalid phone khỏi no answer.

## 8.2. Scope In

Module được phép xác định:

Số điện thoại có định dạng hợp lệ không.

Số có đủ chữ số không.

Số có thuộc dải hợp lệ theo policy không.

Số có thể quay gọi không.

Provider/telco có báo số không tồn tại không.

Số có bị suppression/call block không.

Số có thuộc danh sách không được gọi không.

Có cần human review vì dữ liệu số điện thoại bất thường không.

## 8.3. Scope Out

Module không được:

Tự gọi khách.

Tự hủy đơn.

Tự xác nhận no answer.

Tự xác nhận customer cancel.

Tự sửa số điện thoại.

Tự thay số điện thoại bằng dữ liệu khác.

Tự gửi tin nhắn yêu cầu sửa số.

Tự kết luận khách phá hoại nếu chỉ sai định dạng.

## 8.4. Upstream Dependency

Phụ thuộc:

Order phone reference.

Customer profile phone reference.

Phone validation policy.

Suppression/call block policy.

Provider/telco result nếu có.

Privacy policy.

## 8.5. Downstream Handoff

Bàn giao:

## VALID_PHONE.

## INVALID_PHONE.

## PHONE_NEEDS_REVIEW.

## CALL_BLOCKED_BY_POLICY.

Reason code.

Evidence reference.

Handoff về IVR Runtime Orchestrator.

## 8.6. P0 điểm chặn

FAIL nếu invalid phone bị ghi là no answer.

FAIL nếu số sai định dạng vẫn được gọi.

FAIL nếu phone validation lỗi nhưng hệ thống vẫn tự gọi.

FAIL nếu module tự sửa số điện thoại không có xác nhận/human review.

## 8.7. Evidence

Evidence tối thiểu:

Phone reference.

Validation rule.

Validation result.

Provider/telco status nếu có.

Reason code.

Policy version.

Audit log.

## 8.8. Smoke

## IVR-MOD-SMK-011

Given số điện thoại sai định dạng
When Phone Validation Resolver chạy
Then result = INVALID_PHONE và IVR không tạo call attempt.

## IVR-MOD-SMK-012

Given số hợp lệ về định dạng
When Phone Validation Resolver chạy
Then module cho phép chuyển sang attempt policy/call attempt nếu không có điểm chặn khác.

## 9. MODULE CONTRACT 07 - CALL OUTCOME CLASSIFIER

## 9.1. Mục tiêu

Call Outcome Classifier phân loại kết quả cuộc gọi IVR thành các outcome nghiệp vụ rõ ràng.

Module này đảm bảo invalid phone, no answer, busy, rejected, unreachable, customer cancel, customer confirm, need support và technical failure không bị trộn lẫn.

## 9.2. Scope In

Module được phép phân loại:

## CUSTOMER_CONFIRMED.

## CUSTOMER_CANCELLED_VIA_IVR.

## CUSTOMER_NEED_SUPPORT.

## NO_ANSWER.

## NO_ANSWER_MAX_ATTEMPT_REACHED.

## BUSY.

## REJECTED.

## UNREACHABLE.

## INVALID_PHONE.

## TECHNICAL_FAILURE.

## PROVIDER_FAILURE.

## OUTCOME_UNKNOWN.

## NEED_HUMAN_REVIEW.

## 9.3. Scope Out

Module không được:

Tự hủy đơn.

Tự xác nhận đơn.

Tự xác nhận payment.

Tự xác nhận revenue.

Tự gửi notification.

Tự sửa kết quả provider.

Tự đoán customer intent khi thiếu tín hiệu.

Tự quy lỗi khách khi provider lỗi.

Tự coi busy/rejected/unreachable là no answer nếu policy chưa quy định.

## 9.4. Upstream Dependency

Phụ thuộc:

Call provider result.

IVR keypress result.

Call duration.

Ring status.

Attempt number.

Attempt policy.

Phone validation result.

Technical failure signal.

Evidence registry.

## 9.5. Downstream Handoff

Bàn giao:

Outcome classification.

Outcome reason code.

Attempt validity.

Evidence reference.

Human review flag nếu cần.

Handoff về Order Confirmation State Connector.

## 9.6. P0 điểm chặn

FAIL nếu customer cancel bị ghi là no answer.

FAIL nếu technical failure bị ghi là no answer.

FAIL nếu invalid phone bị ghi là no answer.

FAIL nếu no answer bị ghi là invalid phone.

FAIL nếu outcome thiếu evidence vẫn được gửi vào state machine.

## 9.7. Evidence

Evidence tối thiểu:

Provider raw result reference.

Keypress/input result nếu có.

Attempt number.

Attempt policy.

Ring/call connection status.

Outcome classification.

Classifier rule version.

Audit log.

## 9.8. Smoke

## IVR-MOD-SMK-013

Given khách bấm xác nhận
When Call Outcome Classifier chạy
Then outcome = CUSTOMER_CONFIRMED.

## IVR-MOD-SMK-014

Given provider báo call setup failed
When Call Outcome Classifier chạy
Then outcome = TECHNICAL_FAILURE hoặc PROVIDER_FAILURE, không phải NO_ANSWER.

## 10. MODULE CONTRACT 08 - CUSTOMER CONFIRMATION RESOLVER

## 10.1. Mục tiêu

Customer Confirmation Resolver xử lý trường hợp khách xác nhận qua IVR.

Module này chỉ xác nhận ý chí đặt hàng/thông tin cơ bản theo policy, không xác nhận thanh toán hoặc doanh thu.

## 10.2. Scope In

Module được phép:

Xác nhận khách có phản hồi đồng ý.

Xác nhận khách đồng ý nhận hàng.

Xác nhận thông tin cơ bản theo privacy policy.

Ghi nhận confirmation timestamp.

Ghi nhận attempt number.

Ghi nhận evidence.

Bàn giao confirmation outcome cho Order Confirmation State Connector.

Route human review nếu confirmation không rõ.

## 10.3. Scope Out

Module không được:

Xác nhận PAID.

Xác nhận COD success.

Xác nhận Verified Revenue.

Xác nhận giao hàng thành công.

Sửa tiền.

Sửa sản phẩm.

Sửa số lượng.

Tạo order mới.

Đọc full address/payment/member/Diamond/health note nếu không có policy.

Bỏ qua payment flow.

## 10.4. Upstream Dependency

Phụ thuộc:

Call outcome.

IVR script policy.

Privacy policy.

Official order reference.

Evidence/call log.

Core Order State Machine.

## 10.5. Downstream Handoff

Bàn giao:

## CUSTOMER_CONFIRMED.

Confirmation evidence.

Privacy-safe confirmation summary.

Handoff về Order Confirmation State Connector.

## 10.6. P0 điểm chặn

FAIL nếu confirmation bị hiểu là PAID.

FAIL nếu confirmation bị hiểu là Verified Revenue.

FAIL nếu confirmation đọc dữ liệu riêng tư vượt policy.

FAIL nếu confirmation tự chuyển order sang completed.

## 10.7. Evidence

Evidence tối thiểu:

Customer confirmation input.

Attempt number.

Call timestamp.

Confirmation policy version.

Privacy policy version.

Outcome record.

Audit log.

## 10.8. Smoke

## IVR-MOD-SMK-015

Given khách bấm xác nhận
When Customer Confirmation Resolver xử lý
Then chỉ tạo CUSTOMER_CONFIRMED outcome và không thay đổi payment status.

## 11. MODULE CONTRACT 09 - CUSTOMER CANCEL RESOLVER

## 11.1. Mục tiêu

Customer Cancel Resolver xử lý trường hợp khách chủ động hủy qua IVR.

Module này đảm bảo customer cancel không bị trộn với no answer, invalid phone hoặc technical failure.

## 11.2. Scope In

Module được phép:

Nhận tín hiệu khách bấm hủy.

Ghi nhận CUSTOMER_CANCELLED_VIA_IVR.

Ghi nhận lý do hủy nếu IVR script có hỗ trợ và privacy policy cho phép.

Ghi nhận attempt number.

Ghi nhận evidence.

Bàn giao outcome cho Core Order State Machine thông qua connector.

Route human review nếu tín hiệu hủy không rõ.

## 11.3. Scope Out

Module không được:

Tự hủy đơn trực tiếp.

Tự gửi tin nhắn hủy.

Tự đưa khách vào blacklist.

Tự kết luận khách phá hoại.

Tự xác nhận refund.

Tự xác nhận payment reversal.

Tự dùng hủy đơn để cập nhật Ads revenue.

Tự chuyển customer sang suppression nếu chưa có policy.

## 11.4. Upstream Dependency

Phụ thuộc:

IVR input.

Call outcome.

Official order reference.

Core Order State Machine.

Evidence/Audit Registry.

Human Review Policy nếu cancel không rõ.

## 11.5. Downstream Handoff

Bàn giao:

CUSTOMER_CANCELLED_VIA_IVR outcome.

Evidence reference.

Handoff về Order Confirmation State Connector.

Human Review nếu cần.

Notification Handoff chỉ sau Core Order State Machine hủy chính thức.

## 11.6. P0 điểm chặn

FAIL nếu customer cancel bị ghi là no answer.

FAIL nếu customer cancel tự động gửi SMS hủy từ IVR.

FAIL nếu customer cancel tự động đưa khách vào blacklist.

FAIL nếu customer cancel không có evidence.

## 11.7. Evidence

Evidence tối thiểu:

Cancel input.

Attempt number.

Call timestamp.

Cancel outcome.

State connector handoff.

Audit log.

## 11.8. Smoke

## IVR-MOD-SMK-016

Given khách bấm hủy qua IVR
When Customer Cancel Resolver xử lý
Then outcome = CUSTOMER_CANCELLED_VIA_IVR và đơn chỉ hủy qua Core Order State Machine.

## 12. MODULE CONTRACT 10 - KHONG ANSWER RESOLVER

## 12.1. Mục tiêu

No Answer Resolver xử lý trường hợp máy đổ chuông hợp lệ nhưng khách không nghe.

Module này đảm bảo no answer không bị trộn với invalid phone hoặc technical failure.

## 12.2. Scope In

Module được phép:

Ghi nhận attempt có ring hợp lệ nhưng không có người nghe.

Xác định attempt có được tính là hợp lệ không.

Đếm số attempt no answer hợp lệ.

Xác định khi nào đạt max attempt.

Trả outcome NO_ANSWER hoặc NO_ANSWER_MAX_ATTEMPT_REACHED.

Bàn giao outcome cho Order Confirmation State Connector.

Route human review nếu outcome bất thường.

## 12.3. Scope Out

Module không được:

Tự hủy đơn.

Tự gửi tin nhắn sau từng attempt.

Tự gửi tin nhắn sau max attempt nếu Core Order State Machine chưa hủy.

Tự đánh invalid phone.

Tự đánh customer cancel.

Tự đánh technical failure.

Tự gọi lại ngoài policy.

Tự tăng max attempt.

## 12.4. Upstream Dependency

Phụ thuộc:

Call provider ring status.

Attempt policy.

Attempt count.

Call window.

Order status.

Evidence registry.

## 12.5. Downstream Handoff

Bàn giao:

## NO_ANSWER.

## NO_ANSWER_MAX_ATTEMPT_REACHED.

Attempt count.

Evidence reference.

Handoff về Order Confirmation State Connector.

## 12.6. P0 điểm chặn

FAIL nếu no answer tự hủy đơn.

FAIL nếu no answer tự gửi SMS sau từng cuộc gọi.

FAIL nếu no answer bị dùng khi call chưa đổ chuông hợp lệ.

FAIL nếu technical failure bị tính là no answer.

FAIL nếu invalid phone bị tính là no answer.

## 12.7. Evidence

Evidence tối thiểu:

Ring status.

Attempt number.

Call start/end timestamp.

Attempt validity.

Attempt count.

Max attempt policy.

Outcome.

Audit log.

## 12.8. Smoke

## IVR-MOD-SMK-017

Given máy đổ chuông hợp lệ nhưng khách không nghe lần 1
When No Answer Resolver xử lý
Then outcome = NO_ANSWER và chưa tự hủy đơn.

## IVR-MOD-SMK-018

Given khách không nghe đủ 2 attempts hợp lệ
When No Answer Resolver xử lý
Then outcome = NO_ANSWER_MAX_ATTEMPT_REACHED và chỉ bàn giao Core Order State Machine.

## 13. MODULE CONTRACT 11 - TECHNICAL FAILURE RESOLVER

## 13.1. Mục tiêu

Technical Failure Resolver xử lý lỗi kỹ thuật trong quá trình gọi IVR.

Module này đảm bảo lỗi kỹ thuật không bị quy cho khách.

## 13.2. Scope In

Module được phép xử lý:

Provider failure.

Call setup failed.

Network/telco failure.

Timeout từ provider.

Runtime adapter lỗi.

Evidence write failure.

Policy resolver unavailable.

Audio/script unavailable nếu có.

Unknown technical exception.

Retry/human review theo policy.

## 13.3. Scope Out

Module không được:

Ghi lỗi kỹ thuật thành no answer.

Ghi lỗi kỹ thuật thành invalid phone nếu chưa có validation.

Ghi lỗi kỹ thuật thành customer cancel.

Hủy đơn vì lỗi kỹ thuật.

Xác nhận đơn vì lỗi kỹ thuật.

Gửi tin nhắn hủy vì lỗi kỹ thuật.

Tính technical failure là attempt hợp lệ nếu policy không cho phép.

Ẩn lỗi kỹ thuật khỏi audit.

## 13.4. Upstream Dependency

Phụ thuộc:

IVR provider/adapter.

Runtime health check.

Evidence/Audit Registry.

Attempt Policy.

Core Order State Machine.

Human Review Policy.

## 13.5. Downstream Handoff

Bàn giao:

## TECHNICAL_FAILURE.

## PROVIDER_FAILURE.

## RUNTIME_UNAVAILABLE.

## EVIDENCE_WRITE_FAILURE.

## NEED_RETRY.

## NEED_HUMAN_REVIEW.

Audit record.

## 13.6. P0 điểm chặn

FAIL nếu technical failure bị quy lỗi khách.

FAIL nếu technical failure dẫn đến tự hủy đơn.

FAIL nếu thiếu evidence nhưng vẫn dùng outcome.

FAIL nếu provider lỗi vẫn tính là no answer.

## 13.7. Evidence

Evidence tối thiểu:

Failure type.

Failure timestamp.

Provider/adapter reference.

Runtime component reference.

Retry eligibility.

Human review flag.

Audit log.

## 13.8. Smoke

## IVR-MOD-SMK-019

Given provider lỗi call setup
When Technical Failure Resolver xử lý
Then outcome = TECHNICAL_FAILURE và không tính là khách không nghe.

## 14. MODULE CONTRACT 12 - ANTI-FAKE-ORDER RESOLVER

## 14.1. Mục tiêu

Anti-Fake-Order Resolver đánh giá rủi ro đơn ảo, đơn phá hoại, đơn có dấu hiệu bất thường.

Module này hỗ trợ quyết định có đưa đơn vào IVR, human review, hold hoặc block theo policy.

## 14.2. Scope In

Module được phép đánh giá:

Khách mới chưa xác thực.

Số điện thoại mới/chưa xác minh.

Thông tin nhận hàng bất thường.

Địa chỉ không rõ.

Nhiều đơn lặp bất thường.

Số lượng bất thường.

Comment/live signal có dấu hiệu phá hoại.

Customer bị Purchase Block/Blacklist theo policy.

Khách từng hủy/không nhận nhiều lần theo policy.

Đơn có dấu hiệu tấn công vận hành.

Cần IVR hay Human Review.

Cần block order theo policy hay không.

## 14.3. Scope Out

Module không được:

Tự gọi khách.

Tự hủy đơn.

Tự xác nhận đơn.

Tự blacklist khách nếu chưa đủ evidence/policy.

Tự xử complaint thật như troll.

Tự bỏ qua khiếu nại có mã đơn/mã lô/bằng chứng.

Tự kết luận ác ý nếu chỉ thiếu dữ liệu.

Tự public risk status.

Tự gửi tin nhắn khách hàng.

## 14.4. Upstream Dependency

Phụ thuộc:

Commerce order context.

Customer Trust Resolver.

Channel risk context.

Gateway moderation flags.

Purchase Block/Blacklist policy.

Complaint/CSKH/Quality route policy.

Historical order behavior.

Privacy policy.

## 14.5. Downstream Handoff

Bàn giao:

Risk level.

Risk reason code.

Recommended action: IVR / HUMAN_REVIEW / HOLD / BLOCK / ALLOW.

Evidence reference.

Handoff về Eligibility Resolver và Core Order State Machine.

## 14.6. P0 điểm chặn

FAIL nếu complaint thật bị xử như troll.

FAIL nếu troll/malicious vẫn được tạo order không qua block policy.

FAIL nếu anti-fake-order tự hủy đơn không qua state machine.

FAIL nếu khách bị blacklist không có evidence/audit.

FAIL nếu risk decision không có reason code.

## 14.7. Evidence

Evidence tối thiểu:

Risk signal.

Source channel.

Customer/order reference.

Historical reference nếu có.

Moderation flag nếu có.

Risk decision.

Reason code.

Policy version.

Audit log.

## 14.8. Smoke

## IVR-MOD-SMK-020

Given đơn từ khách mới với số điện thoại chưa xác minh
When Anti-Fake-Order Resolver chạy
Then module có thể đề xuất IVR verification nếu không có điểm chặn khác.

## IVR-MOD-SMK-021

Given complaint thật có mã đơn/mã lô/bằng chứng
When Anti-Fake-Order Resolver chạy
Then không xử như troll, phải route Complaint/CSKH/Quality.

## 15. MODULE CONTRACT 13 - NOTIFICATION HANDOFF RESOLVER

## 15.1. Mục tiêu

Notification Handoff Resolver xử lý bàn giao thông báo giao dịch sau khi Core Order State Machine đã có quyết định chính thức.

Module này đảm bảo IVR, SIM Gateway và AI Advisor không tự phát tin nhắn hủy đơn.

## 15.2. Scope In

Module được phép:

Nhận handoff từ Core Order State Machine.

Kiểm tra đơn đã hủy chính thức chưa.

Kiểm tra lý do hủy có đủ rõ không.

Kiểm tra IVR outcome có evidence không.

Kiểm tra notification có được phép gửi không.

Kiểm tra suppression/opt-out/legal block.

Chuẩn bị nội dung giao dịch privacy-safe.

Bàn giao cho Notification Owner.

Ghi audit handoff.

## 15.3. Scope Out

Module không được:

Tự gửi notification nếu không phải Notification Owner.

Gửi trước khi Core Order State Machine hủy.

Gửi sau từng attempt no answer.

Gửi nội dung marketing/upsell.

Gửi full address.

Gửi payment detail.

Gửi member tier.

Gửi Diamond/referral info.

Gửi health note.

Cam kết giữ giá cũ khi quote/order window hết hạn.

Bỏ qua opt-out/suppression.

## 15.4. Upstream Dependency

Phụ thuộc:

Core Order State Machine.

Order cancellation status.

IVR outcome evidence.

Privacy Policy.

Notification Policy.

Suppression/opt-out/legal block.

Notification Owner system.

## 15.5. Downstream Handoff

Bàn giao:

Transactional notification request.

Cancellation reason summary.

Privacy-safe message payload.

Handoff evidence.

Notification Owner.

Audit log.

## 15.6. P0 điểm chặn

FAIL nếu Notification Handoff gửi khi order chưa hủy chính thức.

FAIL nếu IVR tự gửi SMS.

FAIL nếu SIM Gateway tự gửi SMS.

FAIL nếu AI Advisor tự gửi tin hủy.

FAIL nếu thông báo chứa full address/payment/member/Diamond/health note.

FAIL nếu thông báo dùng để upsell.

## 15.7. Evidence

Evidence tối thiểu:

Core cancellation state.

Cancellation reason.

IVR outcome reference.

Privacy check result.

Suppression check result.

Notification handoff record.

Notification Owner acceptance.

Audit log.

## 15.8. Smoke

## IVR-MOD-SMK-022

Given no answer max attempts nhưng order chưa hủy chính thức
When Notification Handoff Resolver chạy
Then không được gửi notification.

## IVR-MOD-SMK-023

Given order đã hủy chính thức do không xác nhận được
When Notification Handoff Resolver chạy
Then chỉ tạo transactional privacy-safe notification handoff.

## 16. MODULE CONTRACT 14 - HUMAN / ADMIN OVERRIDE RESOLVER

## 16.1. Mục tiêu

Human/Admin Override Resolver xử lý các trường hợp cần người có quyền xem xét hoặc can thiệp.

Module này đảm bảo override không tùy tiện, không bypass Sale Lock/Recall/Suppression, không biến IVR confirmation thành payment/revenue.

## 16.2. Scope In

Module được phép xử lý:

Outcome không rõ.

Evidence thiếu hoặc mâu thuẫn.

Technical failure lặp lại.

Customer need support.

Customer cancel nghi ngờ nhầm.

Đơn giá trị cao.

Đơn có rủi ro phá hoại.

Khách cũ có thông tin thay đổi bất thường.

Phone validation cần review.

Admin quyết định gọi lại thủ công theo policy.

Admin xác nhận hủy/hold/retry theo policy.

Owner review trước release hoặc go-live.

## 16.3. Scope Out

Module không được:

Bỏ qua Sale Lock.

Bỏ qua Recall.

Bỏ qua Suppression.

Bỏ qua Payment Confirmation.

Tự xác nhận PAID.

Tự xác nhận Verified Revenue.

Sửa outcome không có evidence.

Xóa audit.

Override bằng miệng không lưu lý do.

Cho phép mọi user override.

## 16.4. Upstream Dependency

Phụ thuộc:

RBAC/Permission.

Evidence Registry.

Audit Registry.

Core Order State Machine.

IVR outcome.

Risk policy.

Privacy policy.

Owner approval policy.

## 16.5. Downstream Handoff

Bàn giao:

Override decision.

Reason code.

Evidence attachment/reference.

Actor identity.

Before/after state.

Core Order State Machine nếu có state change.

Audit log.

## 16.6. P0 điểm chặn

FAIL nếu override không có quyền.

FAIL nếu override không có lý do.

FAIL nếu override không có audit.

FAIL nếu override bypass Sale Lock/Recall.

FAIL nếu override chuyển order sang PAID.

FAIL nếu override tạo Verified Revenue.

## 16.7. Evidence

Evidence tối thiểu:

Actor.

Permission.

Override reason.

Related outcome.

Evidence reference.

State before.

State after.

Approval record nếu cần.

Audit log.

## 16.8. Smoke

## IVR-MOD-SMK-024

Given admin muốn override kết quả IVR
When thiếu reason/evidence
Then hệ thống phải block override.

## IVR-MOD-SMK-025

Given order bị Recall/Sale Lock
When admin override IVR confirmation
Then không được bypass Recall/Sale Lock.

## 17. MODULE CONTRACT 15 - EVIDENCE / CALL LOG / AUDIT RESOLVER

## 17.1. Mục tiêu

Evidence / Call Log / Audit Resolver đảm bảo mọi quyết định IVR đều có bằng chứng và dấu vết kiểm tra.

Không có evidence thì không được dùng outcome để ra quyết định trạng thái đơn.

## 17.2. Scope In

Module được phép ghi nhận:

Order reference.

Customer reference.

Phone reference.

Attempt number.

Attempt policy version.

Call start time.

Call end/failure time.

Provider result.

Phone validation result.

Outcome classification.

State connector handoff.

Notification handoff.

Human/Admin override.

Runtime failure.

Audit trail.

## 17.3. Scope Out

Module không được:

Tự quyết định order state.

Tự gửi notification.

Tự sửa outcome.

Tự xóa evidence.

Tự public call record.

Đưa recording cho AI xử lý tự do.

Dùng evidence cho marketing.

Ghi log thiếu correlation id.

Cho phép sửa append-only log tùy tiện.

## 17.4. Upstream Dependency

Phụ thuộc:

IVR Runtime Orchestrator.

Call provider/adapter.

Phone Validation Resolver.

Outcome Classifier.

State Connector.

Notification Handoff Resolver.

Human/Admin Override Resolver.

Audit policy.

Privacy/legal policy.

## 17.5. Downstream Handoff

Bàn giao:

Evidence reference.

Audit reference.

Call log reference.

Integrity status.

Review flag nếu evidence thiếu.

Release evidence package.

## 17.6. P0 điểm chặn

FAIL nếu outcome không có evidence.

FAIL nếu call log thiếu attempt number.

FAIL nếu không có correlation id.

FAIL nếu evidence bị sửa/xóa không audit.

FAIL nếu recording bị public.

FAIL nếu evidence dùng cho marketing.

## 17.7. Evidence

Evidence tối thiểu chính là bộ evidence chuẩn gồm:

Order identity/reference.

Customer identity/reference.

Phone reference.

Attempt number.

Attempt policy version.

Call timestamps.

Provider status.

Phone validation result.

Outcome classification.

State machine handoff.

Notification handoff nếu có.

Actor/system identity.

Correlation id.

Audit timestamp.

Evidence integrity status.

## 17.8. Smoke

## IVR-MOD-SMK-026

Given call outcome bất kỳ
When evidence thiếu order reference hoặc attempt number
Then outcome không được dùng để chuyển state.

## IVR-MOD-SMK-027

Given call recording tồn tại
When hệ thống lưu evidence
Then recording không được public hoặc dùng cho marketing.

## 18. MODULE CONTRACT 16 - PRIVACY / CALL SCRIPT POLICY RESOLVER

## 18.1. Mục tiêu

Privacy / Call Script Policy Resolver kiểm soát nội dung IVR được phép đọc, hỏi, xác nhận trong cuộc gọi.

Module này đảm bảo IVR không đọc lộ dữ liệu riêng tư.

## 18.2. Scope In

Module được phép xác định:

Nội dung IVR được phép nói.

Thông tin nào được phép xác nhận.

Thông tin nào phải ẩn hoặc rút gọn.

Có được đọc tên sản phẩm không.

Có được đọc số lượng không.

Có được đọc tỉnh/thành không.

Có được yêu cầu bấm xác nhận/hủy/cần hỗ trợ không.

Ngôn ngữ phải trung lập, không marketing.

Nội dung không chứa claim chưa approved.

Nội dung không chứa thông tin nhạy cảm.

## 18.3. Scope Out

Module không được cho phép IVR đọc:

Full address mặc định.

Payment detail.

Member tier.

Diamond/referral benefit.

Health note.

Lịch sử mua hàng.

Ghi chú cá nhân.

Nội dung tư vấn sức khỏe.

Claim điều trị/chữa bệnh.

Nội dung upsell/marketing.

## 18.4. Upstream Dependency

Phụ thuộc:

Privacy policy.

Approved call script policy.

Product claim whitelist nếu có nhắc sản phẩm.

Commerce order summary.

Customer data minimization rule.

Legal/compliance review.

## 18.5. Downstream Handoff

Bàn giao:

Approved IVR script intent.

Allowed fields.

Masked fields.

Forbidden fields.

Privacy policy version.

Handoff về IVR Runtime Orchestrator.

## 18.6. P0 điểm chặn

FAIL nếu IVR đọc full address khi chưa có policy.

FAIL nếu IVR đọc payment detail.

FAIL nếu IVR đọc member tier/Diamond/referral.

FAIL nếu IVR đọc health note.

FAIL nếu IVR dùng ngôn ngữ marketing/upsell.

FAIL nếu IVR nói claim điều trị.

## 18.7. Evidence

Evidence tối thiểu:

Script policy version.

Allowed field list.

Masked field list.

Forbidden field list.

Script approval record.

Privacy review record.

Audit log.

## 18.8. Smoke

## IVR-MOD-SMK-028

Given order có full address/payment/member info
When IVR script được tạo
Then script không được đọc các trường này nếu chưa có privacy policy rõ.

## 19. MODULE CONTRACT 17 - RELEASE HANDOFF RESOLVER

## 19.1. Mục tiêu

Release Handoff Resolver kiểm soát việc TECH-09 được bàn giao sang testing, release readiness và go-live approval.

Module này đảm bảo Documentation Complete không bị gọi nhầm là Production Ready.

## 19.2. Scope In

Module được phép kiểm tra:

TECH-09 đủ 4 phần chưa.

Module contracts đã đủ chưa.

State machine đã đủ chưa.

Attempt policy đã rõ chưa.

Outcome classification đã rõ chưa.

Evidence package đã đủ chưa.

Smoke matrix đã đủ chưa.

Smoke đã pass chưa.

Owner sign-off đã có chưa.

Privacy review đã pass chưa.

Anti-fake-order review đã pass chưa.

Notification handoff review đã pass chưa.

Release decision đã có chưa.

## 19.3. Scope Out

Module không được:

Tự đánh dấu Production Ready khi mới Documentation Complete.

Tự đánh dấu Release Ready khi chưa smoke pass.

Tự đánh dấu Release Approved khi chưa owner sign-off.

Tự đánh dấu Go-live Approved khi chưa release decision.

Bỏ qua evidence.

Bỏ qua privacy review.

Bỏ qua anti-fake-order review.

Bỏ qua notification handoff review.

## 19.4. Upstream Dependency

Phụ thuộc:

## TECH-09 PHẦN 1/4.

## TECH-09 PHẦN 2/4.

## TECH-09 PHẦN 3/4.

## TECH-09 PHẦN 4/4.

Evidence Package.

Smoke Matrix.

Owner Review.

Release Decision.

## 19.5. Downstream Handoff

Bàn giao:

Documentation Complete status.

Evidence status.

Smoke status.

Owner sign-off status.

Release status.

Go-live decision status.

Global Gateway status.

## 19.6. P0 điểm chặn

FAIL nếu Documentation Complete bị gọi là Production Ready.

FAIL nếu không có accepted evidence mà Completion Decision.

FAIL nếu không có smoke pass mà Release Ready.

FAIL nếu không có owner sign-off mà Release Approved.

FAIL nếu không có release decision mà Go-live Approved.

FAIL nếu Global Gateway mở khi chưa đủ gate.

## 19.7. Evidence

Evidence tối thiểu:

Document completion checklist.

Evidence acceptance record.

Smoke execution record.

Owner sign-off record.

Release decision record.

Global Gateway decision.

Audit log.

## 19.8. Smoke

## IVR-MOD-SMK-029

Given TECH-09 mới viết xong tài liệu
When Release Handoff Resolver kiểm tra
Then status chỉ được là DOCUMENTATION COMPLETE, không phải Production Ready.

## IVR-MOD-SMK-030

Given smoke chưa pass
When Release Handoff Resolver kiểm tra
Then không được Release Ready.

## 20. CROSS-MODULE DEPENDENCY MAP

## 20.1. Luồng dependency chính

Luồng chuẩn giữa các module:

Commerce tạo Official Order.

IVR Runtime Orchestrator nhận order.

IVR Eligibility Resolver kiểm tra điều kiện.

Customer Trust Resolver xác định khách mới/untrusted/trusted.

Anti-Fake-Order Resolver kiểm tra rủi ro.

Call Attempt Policy Resolver xác định attempt policy.

Phone Validation Resolver kiểm tra số điện thoại.

Privacy / Call Script Policy Resolver kiểm soát nội dung gọi.

IVR Runtime Orchestrator tạo call attempt nếu đủ điều kiện.

Call Outcome Classifier phân loại kết quả.

Customer Confirmation / Cancel / No Answer / Technical Failure Resolver xử lý outcome chuyên biệt.

Evidence / Call Log / Audit Resolver ghi nhận.

Order Confirmation State Connector bàn giao Core Order State Machine.

Notification Handoff Resolver chỉ chạy khi Core Order State Machine hủy chính thức.

Human/Admin Override Resolver xử lý ngoại lệ.

Release Handoff Resolver kiểm soát release.

## 20.2. No-bypass dependency

Không module nào được bỏ qua:

Commerce Official Order.

Eligibility decision.

Customer Trust decision.

Attempt Policy.

Phone Validation.

Privacy Policy.

Outcome Classification.

Evidence/Audit.

Core Order State Machine.

Notification Owner Handoff.

## 21. P0 điểm chặn REGISTRY - PHẦN 2/4

Các lỗi sau là P0 điểm chặn ở cấp module:

Module nào tự tạo order.

Module nào gọi Quote/Cart/Order Draft.

Module nào tự sửa giá.

Module nào tự xác nhận PAID.

Module nào tự xác nhận Verified Revenue.

Module nào tự hủy đơn không qua Core Order State Machine.

Module nào tự gửi notification sau hủy.

Module nào gọi khách trusted đại trà.

Module nào bỏ qua eligibility.

Module nào bỏ qua attempt policy.

Module nào bỏ qua phone validation.

Module nào bỏ qua privacy policy.

Module nào bỏ qua evidence/audit.

Module nào trộn invalid phone với no answer.

Module nào trộn customer cancel với no answer.

Module nào trộn technical failure với lỗi khách.

Module nào gọi lại vô hạn.

Module nào áp attempt policy sai chương trình.

Module nào dùng no answer để tự gửi SMS sau từng cuộc gọi.

Module nào dùng IVR confirmation làm revenue.

Module nào dùng IVR confirmation làm payment.

Module nào bypass Sale Lock/Recall/Suppression.

Module nào xử complaint thật như troll.

Module nào public dữ liệu private.

Module nào cho phép admin override không audit.

Module nào cho phép release khi chưa có smoke/evidence/sign-off/decision.

Module nào gọi Documentation Complete là Production Ready.

## 22. EVIDENCE PACKAGE - PHẦN 2/4

## PHẦN 2/4 yêu cầu evidence thiết kế ở cấp module gồm:

IVR Runtime Orchestrator contract accepted.

Eligibility Resolver contract accepted.

Order Confirmation State Connector contract accepted.

Customer Trust Resolver contract accepted.

Call Attempt Policy Resolver contract accepted.

Phone Validation Resolver contract accepted.

Call Outcome Classifier contract accepted.

Customer Confirmation Resolver contract accepted.

Customer Cancel Resolver contract accepted.

No Answer Resolver contract accepted.

Technical Failure Resolver contract accepted.

Anti-Fake-Order Resolver contract accepted.

Notification Handoff Resolver contract accepted.

Human/Admin Override Resolver contract accepted.

Evidence/Call Log/Audit Resolver contract accepted.

Privacy/Call Script Policy Resolver contract accepted.

Release Handoff Resolver contract accepted.

Cross-module dependency accepted.

P0 điểm chặn Registry accepted.

Owner review checklist prepared.

## 23. SMOKE MATRIX ĐỊNH HƯỚNG - PHẦN 2/4

Các smoke ở PHẦN 2/4 là smoke module-level, sẽ được tổng hợp và mở rộng ở PHẦN 4/4.

IVR-P2-SMK-001 - Orchestrator Không Gọi Draft

Given Order Draft
When IVR Runtime Orchestrator nhận request
Then không tạo call attempt.

IVR-P2-SMK-002 - Eligibility Chặn Trusted Customer

Given trusted customer không có rủi ro mới
When Eligibility Resolver chạy
Then không đưa vào IVR đại trà.

IVR-P2-SMK-003 - Attempt Policy Rõ

Given official order eligible
When Call Attempt Policy Resolver chạy
Then trả về max attempt, spacing, call window, stop condition rõ ràng.

IVR-P2-SMK-004 - Phone Invalid Không Gọi

Given số điện thoại invalid
When Phone Validation Resolver chạy
Then không tạo call attempt.

IVR-P2-SMK-005 - Outcome Không Trộn

Given technical failure
When Call Outcome Classifier chạy
Then không ghi là no answer.

IVR-P2-SMK-006 - Customer Cancel Không Tự Hủy

Given khách bấm hủy
When Customer Cancel Resolver xử lý
Then chỉ gửi outcome về Core Order State Machine.

IVR-P2-SMK-007 - No Answer Không Tự Nhắn

Given khách không nghe lần 1
When No Answer Resolver xử lý
Then không gửi SMS, không hủy đơn.

IVR-P2-SMK-008 - Max Attempts Chỉ Bàn Giao Core

Given no answer đủ 2 attempts hợp lệ
When resolver xử lý
Then chỉ bàn giao Core Order State Machine, không tự notification.

IVR-P2-SMK-009 - Notification Sau Hủy Chính Thức

Given Core Order State Machine chưa hủy
When Notification Handoff Resolver chạy
Then không gửi notification.

IVR-P2-SMK-010 - Privacy Script An Toàn

Given order có full address/payment/member/Diamond/health note
When Call Script Policy Resolver chạy
Then các trường này không được đọc nếu chưa có policy rõ.

IVR-P2-SMK-011 - Admin Override Có Audit

Given admin override
When thiếu reason/evidence
Then override bị block.

IVR-P2-SMK-012 - Release Không Nhầm Trạng Thái

Given TECH-09 mới hoàn tất tài liệu
When Release Handoff Resolver chạy
Then không được đánh Production Ready.

## 24. DONE GATE - PHẦN 2/4

## PHẦN 2/4 đạt Documentation Complete ở cấp module contract khi:

Đã khóa đủ IVR Runtime Orchestrator.

Đã khóa IVR Eligibility Resolver.

Đã khóa Order Confirmation State Connector.

Đã khóa Customer Trust Resolver.

Đã khóa Call Attempt Policy Resolver.

Đã khóa Phone Validation Resolver.

Đã khóa Call Outcome Classifier.

Đã khóa Customer Confirmation Resolver.

Đã khóa Customer Cancel Resolver.

Đã khóa No Answer Resolver.

Đã khóa Technical Failure Resolver.

Đã khóa Anti-Fake-Order Resolver.

Đã khóa Notification Handoff Resolver.

Đã khóa Human/Admin Override Resolver.

Đã khóa Evidence/Call Log/Audit Resolver.

Đã khóa Privacy/Call Script Policy Resolver.

Đã khóa Release Handoff Resolver.

Mỗi module có Scope In rõ.

Mỗi module có Scope Out rõ.

Mỗi module có Upstream Dependency rõ.

Mỗi module có Downstream Handoff rõ.

Mỗi module có P0 điểm chặn rõ.

Mỗi module có Evidence rõ.

Mỗi module có Smoke rõ.

Có Cross-Module Dependency Map.

Có P0 điểm chặn Registry cấp phần.

Có Evidence Package cấp phần.

Có Smoke Matrix định hướng.

Sẵn sàng chuyển sang PHẦN 3/4.

## 25. FAIL GATE - PHẦN 2/4

## PHẦN 2/4 FAIL nếu:

Thiếu module contract bắt buộc.

Có module chưa có Scope In.

Có module chưa có Scope Out.

Có module chưa có Upstream Dependency.

Có module chưa có Downstream Handoff.

Có module chưa có P0 điểm chặn.

Có module chưa có Evidence.

Có module chưa có Smoke.

Có module được phép tự tạo order.

Có module được phép tự sửa giá.

Có module được phép tự xác nhận PAID.

Có module được phép tự xác nhận Verified Revenue.

Có module được phép tự hủy đơn.

Có module được phép tự gửi notification sau hủy.

Có module cho gọi Quote/Cart/Order Draft.

Có module cho gọi khách trusted đại trà.

Có module trộn invalid phone/no answer/technical failure/customer cancel.

Có module bypass Core Order State Machine.

Có module bypass Evidence/Audit.

Có module bypass Privacy Policy.

Có module bypass Sale Lock/Recall/Suppression.

Có module gọi Documentation Complete là Production Ready.

## 26. RELEASE HANDOFF - SANG PHẦN 3/4

## PHẦN 2/4 bàn giao sang PHẦN 3/4 các nội dung đã khóa:

Danh sách module IVR đầy đủ.

Boundary từng module.

Dependency từng module.

Handoff từng module.

P0 điểm chặn từng module.

Evidence từng module.

Smoke từng module.

Cross-module dependency map.

Baseline MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED.

Golden Hour timing context.

24/7 timing context.

Outcome classification cần viết thành state/lifecycle.

Notification handoff chỉ sau Core Order State Machine hủy chính thức.

Privacy-safe call script và notification boundary.

Runtime unavailable fail-safe.

Human/Admin Override bắt buộc evidence/audit.

## PHẦN 3/4 phải tiếp tục viết:

IVR Lifecycle / State Machine / Attempt Policy / Outcome Classification / Core Order Handoff

Trong đó phải khóa rõ:

IVR eligibility state.

Official order confirmation state.

Call attempt state.

Phone validation state.

Call result state.

Customer confirmed state.

Customer cancelled state.

Customer need support state.

No answer state.

Invalid phone state.

Busy/rejected/unreachable state.

Technical failure state.

Max attempts reached state.

Core order cancellation handoff.

Notification owner handoff.

Fail-safe state.

Human review state.

Admin override state.

## 27. TRẠNG THÁI SAU PHẦN 2/4

Sau PHẦN 2/4:

## PHẦN 1/4 đã khóa nguyên tắc.

## PHẦN 2/4 đã khóa module contracts.

Chưa được gọi là Production Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Go-live Approved.

Chưa được gọi là IVR Ready.

Chưa được gọi là Anti-Fake-Order Ready.

Global Gateway vẫn mặc định:

bị chặn

Chỉ sau khi hoàn tất PHẦN 3/4 và PHẦN 4/4, có accepted evidence, smoke pass, owner sign-off và release decision thì mới được xét trạng thái cao hơn.

## KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 đã khóa đầy đủ module contracts cho TECH-09.

Trọng tâm đã được cố định:

IVR Runtime Orchestrator chỉ điều phối, không sở hữu sự thật nghiệp vụ.

Eligibility Resolver chặn gọi đại trà.

Customer Trust Resolver loại trừ khách trusted đủ điều kiện.

Call Attempt Policy Resolver khóa số lần gọi, spacing và call window.

Phone Validation Resolver tách invalid phone khỏi no answer.

Call Outcome Classifier tách rõ customer confirm, customer cancel, no answer, busy, rejected, unreachable, invalid phone và technical failure.

No Answer Resolver không tự nhắn/hủy sau từng cuộc gọi.

Notification Handoff Resolver chỉ chạy sau khi Core Order State Machine hủy chính thức.

Privacy / Call Script Policy Resolver chặn lộ full address/payment/member/Diamond/health note.

Evidence / Audit là bắt buộc cho mọi outcome.

Human/Admin Override phải có quyền, lý do, evidence và audit.

Release Handoff không cho phép gọi Documentation Complete là Production Ready.

## PHẦN 2/4 sẵn sàng bàn giao sang:

## PHẦN 3/4 - IVR LIFECYCLE / STATE MACHINE / ATTEMPT POLICY / OUTCOME CLASSIFICATION / CORE ORDER HANDOFF.

## PHẦN 3/4 - IVR LIFECYCLE / STATE MACHINE / ATTEMPT POLICY / OUTCOME CLASSIFICATION / CORE ORDER HANDOFF

## 1. MỤC TIÊU PHẦN 3/4

## PHẦN 3/4 khóa vòng đời vận hành của IVR Order Confirmation.

PHẦN này xác định rõ:

Khi nào một official order được đưa vào IVR.

IVR đi qua những trạng thái nào.

Cuộc gọi được tạo, ghi nhận, phân loại và dừng như thế nào.

Số điện thoại invalid xử lý khác no answer như thế nào.

Khách không nghe máy xử lý khác khách bấm hủy như thế nào.

Lỗi kỹ thuật xử lý khác lỗi từ phía khách như thế nào.

Khi nào đạt max attempts.

Khi nào phải bàn giao Core Order State Machine.

Khi nào Notification Owner được phép gửi thông báo.

Khi nào phải chuyển Human/Admin Review.

Khi runtime unavailable thì phải fail-safe.

Khi có rủi ro đơn ảo/đơn phá hoại thì IVR phối hợp với Anti-Fake-Order Control như thế nào.

## PHẦN 3/4 không viết code.

## PHẦN 3/4 không thiết kế API chi tiết.

## PHẦN 3/4 không thiết kế database chi tiết.

## PHẦN 3/4 không thiết kế UI chi tiết.

## PHẦN 3/4 chỉ khóa state machine và nghiệp vụ vận hành.

## 2. NGUYÊN TẮC LIFECYCLE CHUNG

IVR lifecycle phải tuân thủ các nguyên tắc sau:

Chỉ xử lý official order.

Không xử lý quote.

Không xử lý cart.

Không xử lý order draft.

Không gọi khách hàng đại trà.

Không gọi khách trusted nếu không có rủi ro mới.

Không tự tạo order.

Không tự sửa tiền.

Không tự xác nhận thanh toán.

Không tự ghi nhận Verified Revenue.

Không tự hủy đơn.

Không tự gửi notification sau từng cuộc gọi.

Không tự gửi notification sau max attempt nếu Core Order State Machine chưa hủy chính thức.

Không trộn invalid phone với no answer.

Không trộn customer cancel với no answer.

Không trộn technical failure với lỗi khách.

Không gọi lại vô hạn.

Không bỏ qua evidence/call log/audit.

Không bỏ qua privacy policy.

Không bỏ qua Sale Lock / Recall / Suppression.

Không gọi Documentation Complete là Production Ready.

## 3. IVR LIFECYCLE TỔNG THỂ

## 3.1. Luồng chuẩn tổng thể

Lifecycle chuẩn của IVR gồm các bước:

Commerce tạo Official Order.

Commerce/Core xác định đơn có thể xét IVR hay không.

IVR Eligibility Resolver kiểm tra eligibility.

Customer Trust Resolver xác định khách NEW / UNTRUSTED / TRUSTED.

Anti-Fake-Order Resolver đánh giá rủi ro.

Call Attempt Policy Resolver xác định số lần gọi, spacing, call window.

Phone Validation Resolver kiểm tra số điện thoại.

Privacy / Call Script Policy Resolver kiểm soát nội dung gọi.

IVR Runtime Orchestrator tạo call attempt nếu đủ điều kiện.

Call provider/adapter thực hiện cuộc gọi.

Call Outcome Classifier phân loại kết quả.

Outcome resolver tương ứng xử lý kết quả.

Evidence / Call Log / Audit ghi nhận.

Order Confirmation State Connector bàn giao kết quả về Core Order State Machine.

Core Order State Machine quyết định trạng thái đơn tiếp theo.

Notification Handoff chỉ được kích hoạt khi Core Order State Machine đã có trạng thái phù hợp.

Human/Admin Review xử lý ngoại lệ nếu cần.

## 3.2. Luồng cấm

Các luồng sau bị cấm:

Comment -> IVR.

Messenger conversation -> IVR nếu chưa có official order.

Quote -> IVR.

Cart -> IVR.

Order Draft -> IVR.

Live signal -> IVR.

IVR outcome -> PAID.

IVR outcome -> Verified Revenue.

IVR no answer -> SMS hủy ngay.

IVR technical failure -> hủy đơn.

IVR customer cancel -> blacklist tự động.

IVR max attempt -> notification tự động nếu Core Order State Machine chưa hủy.

## 4. IVR ELIGIBILITY STATE MODEL

## 4.1. Danh sách trạng thái eligibility

IVR eligibility có các trạng thái:

NOT_EVALUATED
Chưa đánh giá điều kiện IVR.

EVALUATING
Đang kiểm tra official order, customer trust, risk, suppression, sale lock, recall.

ELIGIBLE_FOR_IVR
Official order đủ điều kiện đưa vào IVR.

NOT_ELIGIBLE_TRUSTED_CUSTOMER
Khách đủ trusted, không có rủi ro mới, không gọi IVR đại trà.

NOT_ELIGIBLE_NOT_OFFICIAL_ORDER
Không phải official order, không được gọi.

BLOCKED_BY_SALE_LOCK_OR_RECALL
Đơn bị Sale Lock / Recall / Suppression.

BLOCKED_BY_POLICY
Bị chặn bởi policy khác.

NEED_HUMAN_REVIEW
Dữ liệu không đủ rõ, cần người có quyền xem xét.

ELIGIBILITY_FAILED_TECHNICAL
Lỗi kỹ thuật khi đánh giá eligibility.

## 4.2. Transition hợp lệ

Các transition hợp lệ:

## NOT_EVALUATED -> EVALUATING.

## EVALUATING -> ELIGIBLE_FOR_IVR.

## EVALUATING -> NOT_ELIGIBLE_TRUSTED_CUSTOMER.

## EVALUATING -> NOT_ELIGIBLE_NOT_OFFICIAL_ORDER.

## EVALUATING -> BLOCKED_BY_SALE_LOCK_OR_RECALL.

## EVALUATING -> BLOCKED_BY_POLICY.

## EVALUATING -> NEED_HUMAN_REVIEW.

## EVALUATING -> ELIGIBILITY_FAILED_TECHNICAL.

## 4.3. Transition bị cấm

Các transition bị cấm:

## NOT_ELIGIBLE_NOT_OFFICIAL_ORDER -> CALL_ATTEMPT_CREATED.

NOT_ELIGIBLE_TRUSTED_CUSTOMER -> CALL_ATTEMPT_CREATED nếu không có risk override hợp lệ.

## BLOCKED_BY_SALE_LOCK_OR_RECALL -> CALL_ATTEMPT_CREATED.

## ELIGIBILITY_FAILED_TECHNICAL -> CALL_ATTEMPT_CREATED.

NEED_HUMAN_REVIEW -> CALL_ATTEMPT_CREATED nếu chưa có quyết định review hợp lệ.

## 4.4. P0 điểm chặn

FAIL nếu Order Draft vẫn chuyển sang ELIGIBLE_FOR_IVR.

FAIL nếu khách trusted không có rủi ro vẫn chuyển sang ELIGIBLE_FOR_IVR.

FAIL nếu Sale Lock / Recall order vẫn được gọi IVR.

FAIL nếu eligibility failed technical nhưng hệ thống vẫn gọi khách.

## 5. OFFICIAL ORDER CONFIRMATION STATE MODEL

## 5.1. Danh sách trạng thái xác nhận đơn

Official Order trong bối cảnh IVR có các trạng thái xác nhận:

ORDER_CONFIRMATION_NOT_REQUIRED
Đơn không cần IVR.

ORDER_CONFIRMATION_REQUIRED
Đơn cần IVR theo policy.

ORDER_CONFIRMATION_WAITING
Đơn đang chờ IVR xác nhận.

ORDER_CONFIRMATION_CALLING
Đang thực hiện attempt.

ORDER_CONFIRMATION_CONFIRMED
Khách đã xác nhận qua IVR.

ORDER_CONFIRMATION_CANCEL_REQUESTED_BY_CUSTOMER
Khách bấm hủy qua IVR.

ORDER_CONFIRMATION_NEED_SUPPORT
Khách cần hỗ trợ.

ORDER_CONFIRMATION_NO_ANSWER_WAITING_RETRY
Khách chưa nghe, còn attempt hợp lệ.

ORDER_CONFIRMATION_NO_ANSWER_MAX_REACHED
Khách không nghe sau đủ attempts hợp lệ.

ORDER_CONFIRMATION_INVALID_PHONE
Số điện thoại không hợp lệ.

ORDER_CONFIRMATION_TECHNICAL_FAILURE
Lỗi kỹ thuật.

ORDER_CONFIRMATION_HUMAN_REVIEW_REQUIRED
Cần người xử lý.

ORDER_CONFIRMATION_BLOCKED
Không được tiếp tục do policy.

## 5.2. Ý nghĩa quan trọng

ORDER_CONFIRMATION_CONFIRMED không đồng nghĩa PAID.

ORDER_CONFIRMATION_CONFIRMED không đồng nghĩa Verified Revenue.

ORDER_CONFIRMATION_CONFIRMED không đồng nghĩa Delivery Success.

ORDER_CONFIRMATION_CANCEL_REQUESTED_BY_CUSTOMER chưa đồng nghĩa đơn đã hủy cho tới khi Core Order State Machine xử lý.

ORDER_CONFIRMATION_NO_ANSWER_MAX_REACHED chưa đồng nghĩa Notification Owner được gửi thông báo, nếu Core Order State Machine chưa hủy chính thức.

## 5.3. Transition hợp lệ

Các transition hợp lệ:

## ORDER_CONFIRMATION_REQUIRED -> ORDER_CONFIRMATION_WAITING.

## ORDER_CONFIRMATION_WAITING -> ORDER_CONFIRMATION_CALLING.

## ORDER_CONFIRMATION_CALLING -> ORDER_CONFIRMATION_CONFIRMED.

## ORDER_CONFIRMATION_CALLING -> ORDER_CONFIRMATION_CANCEL_REQUESTED_BY_CUSTOMER.

## ORDER_CONFIRMATION_CALLING -> ORDER_CONFIRMATION_NEED_SUPPORT.

## ORDER_CONFIRMATION_CALLING -> ORDER_CONFIRMATION_NO_ANSWER_WAITING_RETRY.

ORDER_CONFIRMATION_NO_ANSWER_WAITING_RETRY -> ORDER_CONFIRMATION_CALLING nếu còn attempt.

ORDER_CONFIRMATION_NO_ANSWER_WAITING_RETRY -> ORDER_CONFIRMATION_NO_ANSWER_MAX_REACHED nếu đủ attempts.

ORDER_CONFIRMATION_WAITING -> ORDER_CONFIRMATION_INVALID_PHONE nếu phone invalid.

ORDER_CONFIRMATION_CALLING -> ORDER_CONFIRMATION_TECHNICAL_FAILURE nếu lỗi kỹ thuật.

Bất kỳ trạng thái chưa hoàn tất -> ORDER_CONFIRMATION_HUMAN_REVIEW_REQUIRED nếu policy yêu cầu.

Bất kỳ trạng thái chưa hoàn tất -> ORDER_CONFIRMATION_BLOCKED nếu Sale Lock / Recall / Suppression phát sinh.

## 5.4. Transition bị cấm

Các transition bị cấm:

## ORDER_CONFIRMATION_CONFIRMED -> PAID.

## ORDER_CONFIRMATION_CONFIRMED -> VERIFIED_REVENUE.

## ORDER_CONFIRMATION_CONFIRMED -> DELIVERED.

ORDER_CONFIRMATION_NO_ANSWER_MAX_REACHED -> NOTIFICATION_SENT nếu Core Order State Machine chưa hủy.

## ORDER_CONFIRMATION_TECHNICAL_FAILURE -> ORDER_CANCELLED_BY_CUSTOMER.

## ORDER_CONFIRMATION_INVALID_PHONE -> NO_ANSWER.

## ORDER_CONFIRMATION_CANCEL_REQUESTED_BY_CUSTOMER -> NO_ANSWER.

## ORDER_CONFIRMATION_NEED_SUPPORT -> AUTO_CANCELLED.

## ORDER_CONFIRMATION_BLOCKED -> CALLING.

ORDER_CONFIRMATION_HUMAN_REVIEW_REQUIRED -> CALLING nếu chưa có quyết định review.

## 6. CALL ATTEMPT STATE MODEL

## 6.1. Danh sách trạng thái call attempt

Mỗi cuộc gọi IVR có các trạng thái:

ATTEMPT_NOT_CREATED
Chưa tạo attempt.

ATTEMPT_POLICY_CHECKED
Đã kiểm tra attempt policy.

ATTEMPT_READY
Đủ điều kiện tạo attempt.

ATTEMPT_CREATED
Attempt đã được tạo.

ATTEMPT_DISPATCHED
Đã gửi sang call provider/adapter.

ATTEMPT_RINGING
Cuộc gọi đã đổ chuông hợp lệ.

ATTEMPT_CONNECTED
Khách bắt máy hoặc hệ thống ghi nhận kết nối hợp lệ.

ATTEMPT_COMPLETED
Cuộc gọi kết thúc có outcome.

ATTEMPT_FAILED_TECHNICAL
Cuộc gọi lỗi kỹ thuật.

ATTEMPT_NOT_COUNTED
Attempt không được tính vì lỗi kỹ thuật hoặc lỗi policy.

ATTEMPT_COUNTED
Attempt được tính là attempt hợp lệ.

ATTEMPT_SUPPRESSED
Attempt bị chặn do policy/suppression.

ATTEMPT_EXPIRED
Attempt không còn hợp lệ do quá call window.

## 6.2. Nguyên tắc attempt hợp lệ

Một attempt chỉ được tính là hợp lệ khi:

Order vẫn còn ở trạng thái cho phép gọi.

Eligibility vẫn hợp lệ.

Phone validation pass.

Attempt nằm trong call window.

Không bị Sale Lock / Recall / Suppression.

Không bị opt-out/call restriction.

Call provider có kết quả đủ tin cậy.

Evidence được ghi nhận.

Attempt không phải lỗi kỹ thuật.

Attempt tuân thủ spacing policy.

## 6.3. Attempt không hợp lệ

Attempt không được tính vào MAX_ATTEMPT_PER_ORDER nếu:

Call provider lỗi kỹ thuật.

Runtime adapter lỗi.

Evidence không ghi được.

Policy resolver unavailable.

Cuộc gọi chưa được thiết lập.

Không có thông tin ring hợp lệ.

Bị chặn bởi suppression.

Bị chặn bởi Sale Lock / Recall phát sinh.

Gọi ngoài call window.

Có lỗi hệ thống làm sai call script/privacy policy.

## 6.4. P0 điểm chặn

FAIL nếu technical failure bị tính là no answer.

FAIL nếu attempt không có evidence vẫn được tính.

FAIL nếu gọi ngoài call window vẫn được tính.

FAIL nếu attempt bị suppression vẫn dispatch.

FAIL nếu IVR gọi quá MAX_ATTEMPT_PER_ORDER.

## 7. BASELINE CALL ATTEMPT POLICY

## 7.1. Baseline V1.0

TECH-09 V1.0 khóa baseline:

## MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED

Baseline này áp dụng cho bản V1.0.

Không được áp một max attempt chung cho mọi chương trình hoặc áp sai attempt policy theo chương trình.

24/7 dùng 3 attempts ngay trong V1.0 theo program-based policy.

## 7.2. Golden Hour attempt policy

Với đơn thuộc bối cảnh Giờ Vàng:

Quote freeze context do Commerce quyết định.

IVR confirmation window: 10 phút.

## MAX_ATTEMPT_PER_ORDER = 2.

Attempt spacing tham chiếu: 5 phút.

Attempt 1 thực hiện khi order confirmation required và đủ điều kiện.

Attempt 2 chỉ thực hiện nếu attempt 1 là no answer hợp lệ và order vẫn còn trong call window.

Không gọi attempt 2 nếu order đã hết hiệu lực theo Commerce.

Không gọi attempt 2 nếu khách đã xác nhận.

Không gọi attempt 2 nếu khách đã bấm hủy.

Không gọi attempt 2 nếu khách cần hỗ trợ.

Không gọi attempt 2 nếu phát sinh Sale Lock / Recall / Suppression.

## 7.3. 24/7 attempt policy

Với đơn thuộc bối cảnh 24/7:

Quote freeze context: 15 phút.

## MAX_ATTEMPT_PER_ORDER = 3.

Attempt spacing tham chiếu: 5 phút.

Attempt 1 thực hiện khi order confirmation required và đủ điều kiện.

Attempt 2 chỉ thực hiện nếu attempt 1 là no answer hợp lệ và order vẫn còn trong call window.

Không gọi attempt 2 nếu order đã hết hiệu lực theo Commerce.

Không gọi attempt 2 nếu khách đã xác nhận.

Không gọi attempt 2 nếu khách đã bấm hủy.

Không gọi attempt 2 nếu khách cần hỗ trợ.

Không gọi attempt 2 nếu phát sinh Sale Lock / Recall / Suppression.

## 7.4. Điều kiện dừng gọi

IVR phải dừng gọi khi có một trong các điều kiện:

Khách xác nhận.

Khách bấm hủy.

Khách cần hỗ trợ.

Số điện thoại invalid.

Đã đạt max attempts hợp lệ.

Order hết hiệu lực.

Order bị Core Order State Machine hủy.

Order bị Sale Lock.

Order bị Recall/Suppression.

Runtime unavailable.

Evidence system unavailable.

Privacy script unavailable.

Human review required.

Admin hold.

Call restriction/opt-out.

## 7.5. P0 điểm chặn

FAIL nếu IVR gọi attempt 2 sau khi khách đã xác nhận.

FAIL nếu IVR gọi attempt 2 sau khi khách đã hủy.

FAIL nếu IVR gọi attempt 2 sau khi đơn hết hiệu lực.

FAIL nếu IVR gọi attempt 2 ngoài spacing policy.

FAIL nếu IVR gọi quá 2 attempts trong V1.0.

FAIL nếu policy 3 attempts được thêm chỉnh sửa tạm lẻ vào bản 2 attempts.

## 8. PHONE VALIDATION STATE MODEL

## 8.1. Trạng thái phone validation

Phone validation có các trạng thái:

PHONE_NOT_CHECKED
Chưa kiểm tra số điện thoại.

PHONE_CHECKING
Đang kiểm tra.

PHONE_VALID
Số điện thoại hợp lệ để gọi.

PHONE_INVALID_FORMAT
Sai định dạng.

PHONE_INVALID_RANGE
Không thuộc dải hợp lệ theo policy.

PHONE_NOT_CALLABLE
Không thể quay gọi.

PHONE_PROVIDER_REPORTED_INVALID
Provider/telco báo không tồn tại hoặc không hợp lệ.

PHONE_BLOCKED_BY_POLICY
Không được gọi do suppression/call restriction.

PHONE_NEED_HUMAN_REVIEW
Dữ liệu số điện thoại không đủ rõ.

PHONE_VALIDATION_TECHNICAL_FAILURE
Lỗi kỹ thuật khi kiểm tra.

## 8.2. Transition hợp lệ

## PHONE_NOT_CHECKED -> PHONE_CHECKING.

## PHONE_CHECKING -> PHONE_VALID.

## PHONE_CHECKING -> PHONE_INVALID_FORMAT.

## PHONE_CHECKING -> PHONE_INVALID_RANGE.

## PHONE_CHECKING -> PHONE_NOT_CALLABLE.

## PHONE_CHECKING -> PHONE_PROVIDER_REPORTED_INVALID.

## PHONE_CHECKING -> PHONE_BLOCKED_BY_POLICY.

## PHONE_CHECKING -> PHONE_NEED_HUMAN_REVIEW.

## PHONE_CHECKING -> PHONE_VALIDATION_TECHNICAL_FAILURE.

## 8.3. Quy tắc xử lý

Nếu PHONE_VALID thì mới được xét tạo call attempt.

Nếu PHONE_INVALID_FORMAT thì không tạo call attempt.

Nếu PHONE_PROVIDER_REPORTED_INVALID thì không tạo call attempt.

Nếu PHONE_BLOCKED_BY_POLICY thì không tạo call attempt.

Nếu PHONE_NEED_HUMAN_REVIEW thì không gọi tự động.

Nếu PHONE_VALIDATION_TECHNICAL_FAILURE thì fail-safe, không tự gọi.

## 8.4. P0 điểm chặn

FAIL nếu PHONE_INVALID_FORMAT vẫn được gọi.

FAIL nếu PHONE_PROVIDER_REPORTED_INVALID bị ghi là no answer.

FAIL nếu PHONE_VALIDATION_TECHNICAL_FAILURE bị ghi là invalid phone.

FAIL nếu PHONE_NEED_HUMAN_REVIEW vẫn gọi tự động.

## 9. CALL OUTCOME CLASSIFICATION MODEL

## 9.1. Nhóm outcome chính

Call Outcome Classifier phải phân loại tối thiểu các nhóm sau:

CUSTOMER_CONFIRMED
Khách xác nhận nhận đơn.

CUSTOMER_CANCELLED_VIA_IVR
Khách chủ động hủy qua IVR.

CUSTOMER_NEED_SUPPORT
Khách cần hỗ trợ.

NO_ANSWER
Máy đổ chuông hợp lệ nhưng khách không nghe, chưa đạt max attempts.

NO_ANSWER_MAX_ATTEMPT_REACHED
Máy đổ chuông hợp lệ nhưng khách không nghe sau đủ attempts.

BUSY
Máy bận.

REJECTED
Khách từ chối cuộc gọi.

UNREACHABLE
Không liên lạc được tại thời điểm gọi.

INVALID_PHONE
Số điện thoại không hợp lệ.

TECHNICAL_FAILURE
Lỗi kỹ thuật hệ thống.

PROVIDER_FAILURE
Lỗi từ call provider/telco/adapter.

OUTCOME_UNKNOWN
Không đủ dữ liệu phân loại.

NEED_HUMAN_REVIEW
Cần người xử lý.

## 9.2. CUSTOMER_CONFIRMED

CUSTOMER_CONFIRMED được ghi nhận khi:

Khách đã kết nối cuộc gọi.

Khách chọn thao tác xác nhận hợp lệ.

Input hợp lệ theo IVR script.

Attempt có evidence.

Attempt nằm trong policy hợp lệ.

Privacy script không vi phạm.

Order vẫn ở trạng thái cho phép xác nhận.

CUSTOMER_CONFIRMED không được làm các việc sau:

Không chuyển PAID.

Không chuyển Verified Revenue.

Không chuyển Delivery Success.

Không tự phát commission.

Không tự tạo Ads ROAS.

Không tự chuyển order completed.

## 9.3. CUSTOMER_CANCELLED_VIA_IVR

CUSTOMER_CANCELLED_VIA_IVR được ghi nhận khi:

Khách kết nối cuộc gọi.

Khách chọn thao tác hủy hợp lệ.

Input hủy rõ ràng.

Evidence đầy đủ.

Không có technical failure làm sai kết quả.

Order vẫn ở trạng thái được phép nhận cancel request.

CUSTOMER_CANCELLED_VIA_IVR không được làm các việc sau:

Không tự hủy đơn trực tiếp.

Không tự gửi notification.

Không tự đưa khách vào blacklist.

Không tự kết luận khách phá hoại.

Không tự xử lý refund/payment.

Không tự ghi nhận Ads exclusion nếu Commerce chưa xử lý state.

## 9.4. CUSTOMER_NEED_SUPPORT

CUSTOMER_NEED_SUPPORT được ghi nhận khi:

Khách chọn cần hỗ trợ.

Khách không xác nhận rõ.

Khách không hủy rõ.

Khách cần người gọi lại hoặc hỗ trợ thông tin.

IVR script có lựa chọn hỗ trợ hợp lệ.

CUSTOMER_NEED_SUPPORT phải route Human/CSKH/Admin Review.

Không được tự hủy.

Không được tự xác nhận.

Không được upsell tự động.

## 9.5. NO_ANSWER

NO_ANSWER được ghi nhận khi:

Số điện thoại đã valid.

Cuộc gọi đã đổ chuông hợp lệ.

Khách không nghe.

Attempt nằm trong call window.

Attempt không phải technical failure.

Attempt có evidence.

Chưa đạt max attempts.

NO_ANSWER không được dùng khi:

Số điện thoại invalid.

Call setup failed.

Provider failure.

Runtime failure.

Khách bấm hủy.

Khách từ chối cuộc gọi.

Khách cần hỗ trợ.

Không có evidence.

## 9.6. NO_ANSWER_MAX_ATTEMPT_REACHED

NO_ANSWER_MAX_ATTEMPT_REACHED được ghi nhận khi:

Đã có đủ số attempt hợp lệ.

Mỗi attempt đều có ring hợp lệ.

Khách không nghe ở các attempt hợp lệ.

Không có customer cancel.

Không có customer confirm.

Không có need support.

Không có technical failure được tính sai.

Evidence đầy đủ.

Attempt policy xác nhận đã đạt MAX_ATTEMPT_PER_ORDER.

NO_ANSWER_MAX_ATTEMPT_REACHED chỉ được bàn giao Core Order State Machine.

Không tự hủy.

Không tự notification.

Không tự SMS.

Không tự gắn khách xấu.

## 9.7. BUSY / REJECTED / UNREACHABLE

BUSY / REJECTED / UNREACHABLE phải được phân loại riêng.

Không mặc định gộp vào NO_ANSWER.

Tùy policy, các trạng thái này có thể:

Được retry.

Được chuyển Human Review.

Được tính hoặc không tính là attempt hợp lệ.

Được giữ chờ trong call window.

Được bàn giao state machine nếu đạt điều kiện dừng.

V1.0 không cho phép tự động coi BUSY / REJECTED / UNREACHABLE là NO_ANSWER nếu policy chưa định nghĩa rõ.

## 9.8. INVALID_PHONE

INVALID_PHONE được ghi nhận khi Phone Validation Resolver hoặc provider/telco evidence xác định số không hợp lệ.

INVALID_PHONE không phải NO_ANSWER.

INVALID_PHONE không phải TECHNICAL_FAILURE.

INVALID_PHONE không phải CUSTOMER_CANCEL.

INVALID_PHONE phải bàn giao Core Order State Machine hoặc Human Review theo policy.

## 9.9. TECHNICAL_FAILURE / PROVIDER_FAILURE

TECHNICAL_FAILURE hoặc PROVIDER_FAILURE được ghi nhận khi:

Call provider lỗi.

Adapter lỗi.

Runtime lỗi.

Policy resolver lỗi.

Evidence write failure.

Call setup failed do kỹ thuật.

Timeout hệ thống.

Không đủ dữ liệu tin cậy do lỗi hệ thống.

TECHNICAL_FAILURE không được quy lỗi khách.

TECHNICAL_FAILURE không được dùng để hủy đơn.

TECHNICAL_FAILURE không được tính là no answer.

TECHNICAL_FAILURE phải route retry hoặc Human Review theo policy.

## 10. CORE ORDER HANDOFF MODEL

## 10.1. Mục tiêu Core Order Handoff

Core Order Handoff đảm bảo mọi outcome từ IVR đều đi qua Core Order State Machine.

IVR không trực tiếp thay đổi trạng thái cuối cùng của đơn.

## 10.2. Outcome được bàn giao

Các outcome IVR được bàn giao gồm:

## CUSTOMER_CONFIRMED.

## CUSTOMER_CANCELLED_VIA_IVR.

## CUSTOMER_NEED_SUPPORT.

## NO_ANSWER.

## NO_ANSWER_MAX_ATTEMPT_REACHED.

## BUSY.

## REJECTED.

## UNREACHABLE.

## INVALID_PHONE.

## TECHNICAL_FAILURE.

## PROVIDER_FAILURE.

## OUTCOME_UNKNOWN.

## NEED_HUMAN_REVIEW.

## 10.3. Core Order State Machine quyết định

Core Order State Machine có quyền quyết định:

Giữ đơn chờ xác nhận.

Đưa đơn sang confirmed.

Đưa đơn sang customer cancel requested.

Hủy đơn chính thức.

Đưa đơn sang human review.

Đưa đơn sang support required.

Đưa đơn sang hold.

Block đơn theo risk policy.

Cho phép retry.

Dừng xử lý do Sale Lock / Recall / Suppression.

## 10.4. IVR không được quyết định

IVR không được quyết định:

ORDER_CANCELLED chính thức.

## ORDER_PAID.

## ORDER_DELIVERED.

## VERIFIED_REVENUE.

## COMMISSION_ELIGIBLE.

## ADS_REVENUE.

CUSTOMER_BLACKLIST chính thức.

## REFUND.

## COD_SUCCESS.

## DELIVERY_SUCCESS.

## 10.5. P0 điểm chặn

FAIL nếu IVR outcome bypass Core Order State Machine.

FAIL nếu IVR tự chuyển ORDER_CANCELLED.

FAIL nếu IVR tự chuyển PAID.

FAIL nếu IVR tự chuyển Verified Revenue.

FAIL nếu IVR tự kích hoạt Notification Owner khi Core Order State Machine chưa quyết định.

## 11. CORE ORDER CANCELLATION MODEL

## 11.1. Các lý do hủy liên quan IVR

Core Order State Machine có thể hủy đơn liên quan IVR với các reason nhóm sau:

## CUSTOMER_CANCELLED_VIA_IVR.

## UNABLE_TO_CONFIRM_AFTER_MAX_ATTEMPTS.

## INVALID_PHONE_UNRESOLVED.

## HIGH_RISK_FAKE_ORDER_CONFIRMED_BY_POLICY.

## ADMIN_CANCEL_AFTER_REVIEW.

## ORDER_EXPIRED_BEFORE_CONFIRMATION.

## SUPPRESSION_OR_SALE_LOCK_AFTER_ORDER.

## OTHER_POLICY_DEFINED_REASON.

## 11.2. Điều kiện hủy do không xác nhận được

Đơn chỉ được hủy do không xác nhận được khi:

Order là official order.

Order thuộc nhóm cần IVR.

Phone validation không invalid theo nghĩa cần tách riêng, hoặc đã xử lý invalid theo policy.

Đã gọi đủ MAX_ATTEMPT_PER_ORDER hợp lệ.

Các attempt đều có evidence.

Outcome là NO_ANSWER_MAX_ATTEMPT_REACHED.

Không có technical failure bị tính sai.

Không có customer need support đang mở.

Không có human review WAITING.

Core Order State Machine quyết định hủy chính thức.

## 11.3. Điều kiện hủy do khách chủ động hủy

Đơn chỉ được hủy do khách chủ động hủy khi:

Outcome CUSTOMER_CANCELLED_VIA_IVR có evidence.

Input hủy rõ ràng.

Không phải technical failure.

Không phải no answer.

Core Order State Machine nhận outcome.

State transition hợp lệ.

Audit đầy đủ.

## 11.4. Điều kiện không được hủy tự động

Không được hủy tự động nếu:

Technical failure.

Provider failure.

Evidence thiếu.

Outcome unknown.

Customer need support.

Human review WAITING.

Policy resolver unavailable.

Sale Lock/Recall cần xử lý theo flow riêng.

Complaint thật có mã đơn/mã lô/bằng chứng.

Admin hold.

## 12. NOTIFICATION HANDOFF STATE MODEL

## 12.1. Danh sách trạng thái notification handoff

Notification handoff có các trạng thái:

NOTIFICATION_NOT_ALLOWED
Chưa được phép gửi.

WAITING_CORE_ORDER_DECISION
Đang chờ Core Order State Machine.

CORE_ORDER_CANCELLED_CONFIRMED
Core đã hủy đơn chính thức.

PRIVACY_CHECK_WAITING
Đang kiểm tra nội dung privacy-safe.

SUPPRESSION_CHECK_WAITING
Đang kiểm tra suppression/opt-out/legal block.

NOTIFICATION_HANDOFF_READY
Sẵn sàng bàn giao Notification Owner.

NOTIFICATION_HANDOFF_SENT_TO_OWNER
Đã bàn giao Notification Owner.

NOTIFICATION_BLOCKED_BY_POLICY
Không được gửi do policy.

NOTIFICATION_HANDOFF_FAILED_TECHNICAL
Lỗi kỹ thuật khi bàn giao.

## 12.2. Điều kiện kích hoạt Notification Handoff

Notification Handoff chỉ được kích hoạt khi:

Core Order State Machine đã hủy đơn chính thức.

Lý do hủy rõ.

IVR outcome có evidence.

Privacy-safe content đã pass.

Suppression/opt-out/legal block đã pass.

Notification Owner tồn tại và nhận handoff.

Audit được ghi nhận.

## 12.3. Nội dung thông báo sau hủy

Thông báo sau hủy là tin nhắn giao dịch.

Không phải CRM.

Không phải marketing.

Không phải upsell.

Không phải nhắc lại chương trình ưu đãi như một thông điệp bán hàng.

Nội dung được phép ở mức tối thiểu:

Đơn đã bị hủy hoặc hết hiệu lực do không xác nhận được qua cuộc gọi tự động.

Khách có thể phản hồi nếu muốn tạo đơn mới theo chương trình hiện hành.

Không cam kết giữ giá cũ nếu quote/order window đã hết hạn.

## 12.4. Nội dung bị cấm

Thông báo không được chứa:

Full address.

Payment detail.

Member tier.

Diamond/referral benefit.

Health note.

Lịch sử mua hàng.

Thông tin riêng tư không cần thiết.

Nội dung chữa bệnh/điều trị.

Nội dung upsell.

Nội dung marketing trá hình.

## 12.5. P0 điểm chặn

FAIL nếu notification gửi trước khi Core Order State Machine hủy chính thức.

FAIL nếu IVR tự gửi notification.

FAIL nếu SIM Gateway tự gửi notification sau từng cuộc gọi.

FAIL nếu AI Advisor tự gửi notification hủy đơn.

FAIL nếu notification chứa dữ liệu riêng tư bị cấm.

FAIL nếu notification sau hủy biến thành upsell.

## 13. COMMAND BOUNDARY

## 13.1. Command được phép trong IVR

IVR chỉ được phát sinh các command nghiệp vụ ở mức yêu cầu xử lý, không tự quyết định trạng thái cuối cùng.

Các command hợp lệ:

Evaluate IVR Eligibility
Đánh giá đơn có cần IVR không.

Resolve Customer Trust
Đánh giá khách mới/untrusted/trusted.

Resolve Anti-Fake-Order Risk
Đánh giá rủi ro đơn ảo/đơn phá hoại.

Resolve Attempt Policy
Xác định số lần gọi, spacing, call window.

Validate Phone
Kiểm tra số điện thoại.

Prepare Privacy-Safe Script
Chuẩn bị nội dung gọi an toàn dữ liệu.

Create Call Attempt
Tạo attempt nếu đủ điều kiện.

Dispatch Call Attempt
Gửi attempt sang provider/adapter.

Classify Call Outcome
Phân loại kết quả.

Record Evidence / Audit
Ghi evidence/call log/audit.

Submit Outcome To Core Order State Machine
Bàn giao outcome.

Request Human Review
Yêu cầu human review.

Prepare Notification Handoff
Chỉ chuẩn bị handoff khi Core Order State Machine đã hủy chính thức.

## 13.2. Command bị cấm trong IVR

Các command bị cấm:

Create Official Order.

Update Final Price.

Apply Discount.

Apply Member Benefit.

Apply Diamond Referral Benefit.

Confirm Payment.

Confirm COD Success.

Mark Verified Revenue.

Mark Ads Revenue.

Mark Commission Eligible.

Cancel Order Directly.

Send Cancellation Notification Directly.

Send Marketing Message.

Public Call Result.

Add Customer To Blacklist Directly.

Override Sale Lock / Recall.

Force Release Gateway.

Mark Production Ready.

## 13.3. P0 điểm chặn

FAIL nếu IVR có command tự hủy đơn.

FAIL nếu IVR có command tự xác nhận payment.

FAIL nếu IVR có command tự gửi notification sau hủy.

FAIL nếu IVR có command tự blacklist khách không qua policy/evidence/admin review.

## 14. QUERY BOUNDARY

## 14.1. Query được phép

IVR được phép query các dữ liệu sau theo quyền và privacy policy:

Official order reference.

Order status.

Order program context.

Customer trust status.

Customer phone verification status.

Customer risk context.

Attempt history.

Attempt policy.

Sale Lock / Recall / Suppression status.

Phone validation status.

Privacy script policy.

Core Order State Machine status.

Evidence/call log reference.

Notification handoff status.

## 14.2. Query bị hạn chế

IVR không được tự do đọc:

Full customer profile nếu không cần.

Full address nếu không có policy.

Payment detail.

Member tier nếu không cần cho IVR.

Diamond/referral data.

Health note.

Nội dung tư vấn riêng tư.

Lịch sử mua hàng chi tiết vượt nhu cầu xác minh.

Dữ liệu Ads/ROAS.

Dữ liệu commission.

## 14.3. Query không được public

Tất cả query result của IVR là dữ liệu nội bộ.

Không public ra comment.

Không đưa ra live script.

Không đưa sang Ads dashboard như revenue.

Không đưa cho AI Advisor sử dụng vượt Final Response Guard.

Không gửi qua Notification nếu chưa qua privacy-safe rule.

## 15. PRIVACY BOUNDARY TRONG LIFECYCLE

## 15.1. Nguyên tắc tối thiểu hóa dữ liệu

IVR chỉ được dùng dữ liệu tối thiểu để xác nhận đơn.

Không đọc full address mặc định.

Không đọc payment detail.

Không đọc member tier.

Không đọc Diamond/referral info.

Không đọc health note.

Không đọc ghi chú riêng tư.

Không đọc lịch sử mua hàng.

Không đọc nội dung tư vấn sức khỏe.

## 15.2. Dữ liệu có thể xác nhận ở mức an toàn

Tùy policy, IVR có thể xác nhận:

Khách có đặt đơn không.

Khách có đồng ý nhận hàng không.

Số lượng cơ bản nếu cần.

Tên sản phẩm ở mức privacy-safe nếu policy cho phép.

Tỉnh/thành hoặc khu vực rút gọn nếu policy cho phép.

Lựa chọn xác nhận / hủy / cần hỗ trợ.

## 15.3. Privacy failure

Nếu script privacy không sẵn sàng hoặc có rủi ro lộ dữ liệu:

Không tạo call attempt.

Route technical/human review.

Ghi audit.

Không tự dùng script fallback không được duyệt.

Không đọc nội dung tự sinh từ AI.

## 15.4. P0 điểm chặn

FAIL nếu IVR đọc full address khi chưa có policy.

FAIL nếu IVR đọc payment detail.

FAIL nếu IVR đọc member tier/Diamond/referral.

FAIL nếu IVR đọc health note.

FAIL nếu IVR tự sinh script bằng AI chưa qua guard.

## 16. ANTI-FAKE-ORDER HANDLING TRONG LIFECYCLE

## 16.1. Nhóm tín hiệu rủi ro

IVR phối hợp Anti-Fake-Order Resolver với các tín hiệu:

Khách mới.

Số điện thoại chưa xác minh.

Thông tin nhận hàng thiếu tin cậy.

Đơn số lượng bất thường.

Nhiều đơn lặp bất thường.

Đơn phát sinh từ comment/live có dấu hiệu phá hoại.

Tài khoản/kênh có moderation flag.

Khách từng không nhận hàng nhiều lần theo policy.

Khách thuộc Purchase Block/Blacklist.

Địa chỉ/điện thoại trùng pattern rủi ro.

Đơn có dữ liệu mâu thuẫn.

Dấu hiệu tấn công vận hành.

## 16.2. Hành động theo rủi ro

Anti-Fake-Order Resolver có thể đề xuất:

## ALLOW_NO_IVR.

## REQUIRE_IVR.

## REQUIRE_HUMAN_REVIEW.

## HOLD_ORDER.

## BLOCK_ORDER_BY_POLICY.

## ROUTE_COMPLAINT_CS_QUALITY.

## REQUIRE_ADMIN_APPROVAL.

## SUPPRESS_CHANNEL_ACTION.

## 16.3. Không xử complaint thật như troll

Nếu khách có mã đơn, mã lô, bằng chứng, hình ảnh sản phẩm, phản ánh chất lượng hoặc thông tin khiếu nại rõ:

Không xử như troll.

Không auto blacklist.

Không chặn mua hàng nếu chưa có review.

Route Complaint/CSKH/Quality.

Ghi evidence.

Không dùng IVR như công cụ đối phó khiếu nại.

## 16.4. Troll/malicious và Purchase Block

Nếu có evidence khách phá live, abuse, đơn ảo, malicious attack:

Không kéo Messenger trái policy.

Không tạo order nếu đã bị Purchase Block.

Không cho IVR gọi để hợp thức hóa đơn rủi ro.

Route block/human/admin theo policy.

Ghi evidence.

Không public dữ liệu khách.

## 16.5. P0 điểm chặn

FAIL nếu complaint thật bị xử như troll.

FAIL nếu malicious/purchase-bị chặn customer vẫn tạo order và gọi IVR như bình thường.

FAIL nếu anti-fake-order tự blacklist không evidence/audit.

FAIL nếu IVR dùng để bypass moderation/suppression.

## 17. FAIL-SAFE MODEL

## 17.1. Runtime unavailable

Nếu một thành phần bắt buộc không sẵn sàng, IVR phải fail-safe.

Các thành phần bắt buộc:

Commerce Runtime.

Core Order State Machine.

Eligibility Resolver.

Customer Trust Resolver.

Attempt Policy Resolver.

Phone Validation Resolver.

Privacy Script Policy Resolver.

Evidence/Audit Registry.

Sale Lock / Recall / Suppression Check.

Notification Handoff Resolver.

Human/Admin Review Policy.

## 17.2. Fail-safe action

Khi fail-safe:

Không gọi khách nếu chưa đủ điều kiện.

Không tự xác nhận.

Không tự hủy.

Không tự gửi notification.

Không tự đánh no answer.

Không tự đánh invalid phone.

Không tự đánh customer cancel.

Không tự đánh revenue.

Không tự bypass state machine.

Route retry/human review theo policy.

## 17.3. Evidence unavailable

Nếu evidence/call log/audit unavailable:

Không dùng outcome để chuyển state.

Không tính attempt là hợp lệ nếu policy yêu cầu evidence bắt buộc.

Không gửi notification.

Không release outcome.

Route technical failure/human review.

Ghi nhận sự cố khi hệ thống evidence trở lại.

## 17.4. P0 điểm chặn

FAIL nếu runtime unavailable nhưng vẫn gọi khách.

FAIL nếu evidence unavailable nhưng vẫn chuyển order state.

FAIL nếu policy resolver unavailable nhưng IVR tự chọn rule mặc định.

FAIL nếu Core Order State Machine unavailable nhưng IVR tự hủy/xác nhận đơn.

## 18. HUMAN REVIEW STATE MODEL

## 18.1. Trạng thái Human Review

Human Review có các trạng thái:

## HUMAN_REVIEW_NOT_REQUIRED.

## HUMAN_REVIEW_REQUIRED.

## HUMAN_REVIEW_WAITING.

## HUMAN_REVIEW_IN_PROGRESS.

## HUMAN_REVIEW_APPROVED_RETRY.

## HUMAN_REVIEW_APPROVED_CANCEL.

## HUMAN_REVIEW_APPROVED_HOLD.

## HUMAN_REVIEW_APPROVED_CONFIRMATION.

## HUMAN_REVIEW_REJECTED_ACTION.

## HUMAN_REVIEW_ESCALATED_ADMIN.

## HUMAN_REVIEW_CLOSED_WITH_EVIDENCE.

## 18.2. Khi nào cần Human Review

Human Review bắt buộc khi:

Outcome unknown.

Evidence thiếu.

Provider result mâu thuẫn.

Technical failure lặp lại.

Customer need support.

Customer cancel không rõ.

Invalid phone nhưng có dữ liệu khách cũ đáng tin.

Khách cũ trusted nhưng đơn có thay đổi bất thường.

Đơn có rủi ro cao.

Anti-fake-order không đủ tự động quyết định.

Admin override được yêu cầu.

Privacy/script có vấn đề.

## 18.3. Quy tắc Human Review

Human Review phải có:

Người xử lý.

Quyền xử lý.

Lý do xử lý.

Evidence tham chiếu.

Trạng thái trước.

Quyết định sau.

Audit log.

Nếu ảnh hưởng hủy/xác nhận đơn, phải đi qua Core Order State Machine.

## 18.4. P0 điểm chặn

FAIL nếu Human Review không có evidence.

FAIL nếu Human Review không có audit.

FAIL nếu Human Review tự xác nhận PAID.

FAIL nếu Human Review bypass Sale Lock / Recall.

FAIL nếu Human Review biến IVR confirmation thành Verified Revenue.

## 19. ADMIN OVERRIDE STATE MODEL

## 19.1. Trạng thái Admin Override

Admin Override có các trạng thái:

## OVERRIDE_NOT_REQUESTED.

## OVERRIDE_REQUESTED.

## OVERRIDE_WAITING_EVIDENCE.

## OVERRIDE_WAITING_APPROVAL.

## OVERRIDE_APPROVED.

## OVERRIDE_REJECTED.

## OVERRIDE_APPLIED_TO_STATE_MACHINE.

## OVERRIDE_BLOCKED_BY_POLICY.

## OVERRIDE_AUDITED.

## 19.2. Override được phép

Admin Override có thể xử lý:

Cho phép retry khi technical failure.

Chuyển human support khi khách cần hỗ trợ.

Xác nhận hủy khi khách đã hủy rõ.

Hold đơn để kiểm tra.

Block đơn có rủi ro rõ.

Xử lý invalid phone cần xác minh lại.

Xử lý no answer max attempts theo policy.

Xử lý outcome mâu thuẫn.

## 19.3. Override bị cấm

Admin Override không được:

Bỏ qua Payment Confirmation.

Chuyển PAID.

Chuyển Verified Revenue.

Bypass Sale Lock.

Bypass Recall.

Bypass Suppression.

Xóa evidence.

Sửa call log không audit.

Gửi notification marketing.

Hợp thức hóa đơn từ khách malicious đã bị block.

## 19.4. P0 điểm chặn

FAIL nếu override không có quyền.

FAIL nếu override không có reason code.

FAIL nếu override không có evidence.

FAIL nếu override không qua audit.

FAIL nếu override mở Gateway khi chưa release decision.

## 20. IVR LIFECYCLE BY OUTCOME

## 20.1. Outcome CUSTOMER_CONFIRMED

Luồng xử lý:

Call connected.

Customer input xác nhận hợp lệ.

Outcome Classifier ghi CUSTOMER_CONFIRMED.

Evidence ghi nhận.

Order Confirmation State Connector gửi outcome về Core Order State Machine.

Core Order State Machine quyết định chuyển trạng thái order confirmation.

Payment vẫn chờ Payment/Commerce.

Delivery vẫn chờ Delivery/Commerce.

Verified Revenue vẫn chờ Commerce.

Không được:

Tự chuyển PAID.

Tự chuyển Verified Revenue.

Tự scale Ads.

Tự tính commission.

## 20.2. Outcome CUSTOMER_CANCELLED_VIA_IVR

Luồng xử lý:

Call connected.

Customer input hủy hợp lệ.

Outcome Classifier ghi CUSTOMER_CANCELLED_VIA_IVR.

Evidence ghi nhận.

Order Confirmation State Connector gửi Core Order State Machine.

Core Order State Machine quyết định hủy chính thức.

Notification Handoff chỉ chạy sau khi Core hủy.

Notification phải privacy-safe.

Không được:

IVR tự hủy.

IVR tự nhắn.

SIM Gateway tự nhắn.

AI Advisor tự phát tin.

Auto blacklist nếu chưa có risk evidence.

## 20.3. Outcome CUSTOMER_NEED_SUPPORT

Luồng xử lý:

Khách chọn cần hỗ trợ.

Outcome ghi CUSTOMER_NEED_SUPPORT.

Evidence ghi nhận.

State Connector gửi Core Order State Machine.

Core đưa đơn sang support/human review.

CSKH/Human xử lý theo quyền.

Không auto cancel.

Không auto confirm.

Không được:

Tự upsell.

Tự marketing.

Tự hủy.

Tự xác nhận.

## 20.4. Outcome NO_ANSWER

Luồng xử lý:

Phone valid.

Attempt hợp lệ.

Máy đổ chuông hợp lệ.

Khách không nghe.

Outcome ghi NO_ANSWER.

Evidence ghi nhận.

Nếu còn attempt, chờ spacing policy.

Không gửi notification.

Không hủy đơn.

Không được:

Nhắn ngay sau từng cuộc.

Hủy ngay sau từng cuộc.

Tính technical failure là no answer.

## 20.5. Outcome NO_ANSWER_MAX_ATTEMPT_REACHED

Luồng xử lý:

Đã đủ 2 attempts hợp lệ theo V1.0.

Cả hai attempts đều no answer hợp lệ.

Evidence đầy đủ.

Outcome ghi NO_ANSWER_MAX_ATTEMPT_REACHED.

State Connector gửi Core Order State Machine.

Core quyết định hủy/human review/hold theo policy.

Notification Handoff chỉ sau Core hủy chính thức.

Không được:

IVR tự hủy.

IVR tự gửi notification.

SIM Gateway tự gửi.

AI Advisor tự gửi.

Gửi nội dung upsell.

## 20.6. Outcome INVALID_PHONE

Luồng xử lý:

Phone Validation xác định invalid.

Không tạo call attempt.

Outcome ghi INVALID_PHONE.

Evidence ghi nhận.

State Connector gửi Core Order State Machine.

Core quyết định hold/human review/cancel theo policy.

Notification nếu có chỉ theo Notification Owner và privacy policy.

Không được:

Ghi invalid phone là no answer.

Gọi số invalid.

Tự sửa số điện thoại.

Tự nhắn khách khi Core chưa quyết định.

## 20.7. Outcome TECHNICAL_FAILURE

Luồng xử lý:

Runtime/provider/evidence/policy lỗi.

Outcome ghi TECHNICAL_FAILURE.

Không quy lỗi khách.

Không tính là no answer nếu policy không cho phép.

Route retry/human review.

Core Order State Machine không bị bypass.

Audit lỗi kỹ thuật.

Không được:

Hủy đơn vì lỗi kỹ thuật.

Gửi notification hủy.

Đánh khách không nghe.

Đánh số invalid nếu chưa có validation.

## 21. GOLDEN HOUR / 24/7 TIMING INTEGRATION

## 21.1. Nguyên tắc chung

IVR timing phải tôn trọng Commerce timing.

IVR không kéo dài quote freeze.

IVR không giữ giá.

IVR không quyết định giá.

IVR không xác nhận giá sau khi Commerce window hết hiệu lực.

## 21.2. Golden Hour

Trong Golden Hour:

Quote freeze context = 5 phút.

Attempt policy = 2 attempts.

Spacing = 5 phút.

IVR phải hoàn thành trong window phù hợp.

Nếu order/quote hết hiệu lực, IVR không tiếp tục xác nhận như đơn còn hợp lệ.

Nếu attempt chưa hoàn thành mà Core/Commerce báo hết hiệu lực, IVR dừng theo policy.

## 21.3. 24/7

Trong 24/7:

Quote freeze context = 15 phút.

Attempt policy = 2 attempts.

Schedule = T0 / T0 + 5 phút / T0 + 10 phút.

IVR phải tôn trọng window của Commerce.

Nếu qua ngày hoặc thay đổi program policy, Commerce quyết định hiệu lực.

IVR không tự giữ quyền lợi cũ.

## 21.4. P0 điểm chặn

FAIL nếu IVR kéo dài giá Golden Hour.

FAIL nếu IVR giữ giá 24/7 sau khi Commerce hết hiệu lực.

FAIL nếu IVR xác nhận order đã hết hiệu lực.

FAIL nếu IVR tiếp tục gọi khi order đã bị Commerce hủy/hết hạn.

## 22. SUPPRESSION / SALE LOCK / RECALL HANDLING

## 22.1. Sale Lock / Recall thắng IVR

Nếu có Sale Lock / Recall / Suppression:

IVR không gọi.

IVR dừng attempt đang chờ nếu policy yêu cầu.

IVR không xác nhận order.

IVR không đưa order vào delivery flow.

IVR không gửi customer notification tự phát.

Core Order State Machine xử lý theo TECH-03/TECH-04.

## 22.2. Khi Sale Lock / Recall phát sinh giữa lifecycle

Nếu phát sinh giữa lifecycle:

Đánh dấu IVR bị chặn.

Dừng attempt tiếp theo.

Không gửi script xác nhận.

Ghi evidence.

Bàn giao Core Order State Machine.

Route Human/Admin Review nếu cần.

## 22.3. P0 điểm chặn

FAIL nếu IVR tiếp tục gọi sau Sale Lock.

FAIL nếu IVR xác nhận order bị Recall.

FAIL nếu Admin Override bypass Sale Lock/Recall.

FAIL nếu Notification sau hủy lộ lý do nội bộ không được phép public.

## 23. EVIDENCE / CALL LOG / AUDIT LIFECYCLE

## 23.1. Evidence theo từng bước

Mỗi bước lifecycle phải có evidence:

Eligibility decision.

Customer trust decision.

Risk decision.

Attempt policy decision.

Phone validation result.

Privacy script approval.

Attempt creation.

Attempt dispatch.

Call provider result.

Outcome classification.

State connector handoff.

Core Order State Machine decision.

Notification handoff nếu có.

Human/Admin Review nếu có.

Runtime failure nếu có.

## 23.2. Evidence integrity

Evidence phải đảm bảo:

Có correlation id.

Có timestamp.

Có policy version.

Có actor/system identity.

Có order reference.

Có attempt number nếu liên quan call.

Có outcome reason code.

Có audit.

Không bị sửa/xóa tùy tiện.

Không dùng cho marketing.

## 23.3. P0 điểm chặn

FAIL nếu outcome không có evidence.

FAIL nếu attempt không có call log.

FAIL nếu state handoff không có audit.

FAIL nếu notification handoff không có Core cancellation reference.

FAIL nếu admin override không có reason/evidence/audit.

## 24. STATE MACHINE CONFLICT RESOLUTION

## 24.1. Conflict phổ biến

Các conflict cần xử lý:

Khách xác nhận nhưng order hết hiệu lực.

Khách xác nhận nhưng Sale Lock phát sinh.

Khách bấm hủy nhưng provider result lỗi.

Attempt no answer nhưng evidence thiếu.

Phone invalid nhưng customer cũ có số tin cậy khác.

Customer trusted nhưng order có risk mới.

No answer max reached nhưng human review đang WAITING.

Technical failure nhưng attempt count đã tăng sai.

Notification handoff được yêu cầu khi order chưa hủy.

Admin override trái policy.

## 24.2. Quy tắc ưu tiên

Ưu tiên xử lý:

Sale Lock / Recall / Suppression thắng tất cả.

Core Order State Machine thắng IVR outcome.

Payment/Commerce thắng IVR confirmation về payment/revenue.

Evidence integrity thắng outcome.

Privacy policy thắng call script.

Anti-fake-order policy thắng auto-confirm.

Human/Admin Review có quyền chỉ trong phạm vi policy.

Notification Owner chỉ gửi sau Core cancellation.

Runtime unavailable thì fail-safe.

Không có evidence thì không dùng làm căn cứ trạng thái.

## 24.3. P0 điểm chặn

FAIL nếu conflict tự resolve bằng suy đoán.

FAIL nếu IVR outcome thắng Sale Lock/Recall.

FAIL nếu IVR confirmation thắng Payment/Commerce.

FAIL nếu notification gửi khi conflict chưa giải quyết.

## 25. P0 điểm chặn REGISTRY - PHẦN 3/4

Các lỗi sau là P0 điểm chặn ở cấp lifecycle/state machine:

Quote được đưa vào IVR lifecycle.

Cart được đưa vào IVR lifecycle.

Order Draft được đưa vào IVR lifecycle.

Khách trusted không rủi ro vẫn gọi đại trà.

Order Sale Lock / Recall vẫn được gọi.

IVR tự tạo official order.

IVR tự sửa giá.

IVR tự xác nhận PAID.

IVR tự xác nhận Verified Revenue.

IVR confirmation chuyển thẳng sang PAID.

IVR confirmation chuyển thẳng sang revenue.

Customer cancel bị ghi thành no answer.

No answer bị ghi thành invalid phone.

Invalid phone bị ghi thành no answer.

Technical failure bị quy lỗi khách.

Provider failure bị ghi là khách không nghe.

Attempt lỗi kỹ thuật vẫn tính vào max attempts sai policy.

Attempt không evidence vẫn tính hợp lệ.

IVR gọi quá 2 attempts trong V1.0.

IVR áp attempt policy sai chương trình.

IVR tự gửi SMS sau từng no answer.

IVR tự gửi notification sau max attempts.

Notification gửi trước khi Core Order State Machine hủy chính thức.

Notification chứa full address/payment/member/Diamond/health note.

Notification sau hủy dùng để upsell.

IVR đọc dữ liệu riêng tư vượt policy.

IVR tự sinh script chưa approved.

Runtime unavailable nhưng vẫn tự xác nhận/hủy/gọi.

Human Review không evidence/audit.

Admin Override bypass Sale Lock / Recall / Suppression.

Documentation Complete bị gọi nhầm là Production Ready.

## 26. SMOKE ĐỊNH HƯỚNG - PHẦN 3/4

IVR-P3-SMK-001 - Official Order Eligible

Given official order từ Commerce, khách mới, không Sale Lock
When IVR lifecycle bắt đầu
Then state chuyển từ NOT_EVALUATED -> ELIGIBLE_FOR_IVR.

IVR-P3-SMK-002 - Order Draft bị chặn

Given order draft
When IVR lifecycle bắt đầu
Then state = NOT_ELIGIBLE_NOT_OFFICIAL_ORDER và không tạo attempt.

IVR-P3-SMK-003 - Trusted Customer Excluded

Given trusted customer không có risk mới
When eligibility chạy
Then state = NOT_ELIGIBLE_TRUSTED_CUSTOMER.

IVR-P3-SMK-004 - Phone Invalid

Given số điện thoại sai định dạng
When Phone Validation chạy
Then state = PHONE_INVALID_FORMAT và không tạo call attempt.

IVR-P3-SMK-005 - No Answer Attempt 1

Given phone valid, attempt 1 đổ chuông hợp lệ, khách không nghe
When outcome classified
Then outcome = NO_ANSWER và không gửi notification.

IVR-P3-SMK-006 - No Answer Max Reached

Given phone valid, 2 attempts hợp lệ đều không nghe
When outcome classified
Then outcome = NO_ANSWER_MAX_ATTEMPT_REACHED và chỉ handoff Core Order State Machine.

IVR-P3-SMK-007 - Technical Failure Not Customer Fault

Given provider failure
When outcome classified
Then outcome = TECHNICAL_FAILURE / PROVIDER_FAILURE, không phải NO_ANSWER.

IVR-P3-SMK-008 - Customer Confirm Not Paid

Given khách bấm xác nhận
When outcome = CUSTOMER_CONFIRMED
Then order confirmation có thể được Core xử lý, nhưng payment status không đổi.

IVR-P3-SMK-009 - Customer Cancel Through Core

Given khách bấm hủy
When outcome = CUSTOMER_CANCELLED_VIA_IVR
Then IVR không tự hủy, chỉ handoff Core Order State Machine.

IVR-P3-SMK-010 - Notification After Core Cancellation

Given no answer max reached
When Core chưa hủy chính thức
Then Notification Handoff không được gửi.

IVR-P3-SMK-011 - Privacy-Safe Script

Given order có full address/payment/member/Diamond/health note
When IVR script được chuẩn bị
Then các trường này không được đọc nếu chưa có policy rõ.

IVR-P3-SMK-012 - Sale Lock Stops IVR

Given Sale Lock phát sinh giữa lifecycle
When IVR chuẩn bị attempt tiếp theo
Then IVR dừng và bàn giao Core Order State Machine.

IVR-P3-SMK-013 - Runtime Unavailable Fail-Safe

Given Core Order State Machine unavailable
When IVR nhận outcome
Then IVR không tự xác nhận/hủy, route fail-safe.

IVR-P3-SMK-014 - Golden Hour Timing

Given order Golden Hour
When attempt policy chạy
Then chỉ 2 attempts, spacing 5 phút, không vượt IVR confirmation window 10 phút.

IVR-P3-SMK-015 - 24/7 Timing

Given order 24/7
When attempt policy chạy
Then chỉ 3 attempts theo lịch T0 / T0 + 5 phút / T0 + 10 phút, không giữ giá ngoài Commerce policy.

## 27. EVIDENCE PACKAGE - PHẦN 3/4

## PHẦN 3/4 yêu cầu evidence thiết kế cho lifecycle gồm:

Eligibility state model accepted.

Official order confirmation state model accepted.

Call attempt state model accepted.

Phone validation state model accepted.

Outcome classification model accepted.

Core Order Handoff model accepted.

Core cancellation model accepted.

Notification handoff state model accepted.

Command boundary accepted.

Query boundary accepted.

Privacy boundary accepted.

Anti-fake-order handling accepted.

Fail-safe model accepted.

Human Review state model accepted.

Admin Override state model accepted.

Golden Hour timing integration accepted.

24/7 timing integration accepted.

Suppression/Sale Lock/Recall handling accepted.

Conflict resolution model accepted.

P0 điểm chặn Registry accepted.

Smoke định hướng accepted.

## 28. DONE GATE - PHẦN 3/4

## PHẦN 3/4 đạt Documentation Complete ở cấp lifecycle/state machine khi:

Đã khóa IVR lifecycle tổng thể.

Đã khóa eligibility state model.

Đã khóa official order confirmation state model.

Đã khóa call attempt state model.

Đã khóa baseline MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED.

Đã khóa Golden Hour attempt policy.

Đã khóa 24/7 attempt policy.

Đã khóa điều kiện dừng gọi.

Đã khóa phone validation state model.

Đã khóa outcome classification model.

Đã khóa customer confirm flow.

Đã khóa customer cancel flow.

Đã khóa customer need support flow.

Đã khóa no answer flow.

Đã khóa no answer max reached flow.

Đã khóa invalid phone flow.

Đã khóa technical failure flow.

Đã khóa Core Order Handoff.

Đã khóa Core Order Cancellation model.

Đã khóa Notification Handoff state model.

Đã khóa command boundary.

Đã khóa query boundary.

Đã khóa privacy boundary.

Đã khóa anti-fake-order handling.

Đã khóa fail-safe model.

Đã khóa Human Review state model.

Đã khóa Admin Override state model.

Đã khóa Sale Lock / Recall / Suppression handling.

Đã khóa conflict resolution.

Đã có P0 điểm chặn Registry.

Đã có Smoke định hướng.

Đã sẵn sàng chuyển sang PHẦN 4/4.

## 29. FAIL GATE - PHẦN 3/4

## PHẦN 3/4 FAIL nếu:

State model thiếu official order boundary.

Quote/cart/order draft vẫn có đường vào IVR.

Khách trusted bị gọi đại trà.

Sale Lock / Recall không chặn IVR.

Attempt policy không rõ max attempt.

Attempt spacing không rõ.

Golden Hour và 24/7 bị lẫn.

Program-based attempt rule bị áp sai chương trình.

No answer và invalid phone chưa tách.

No answer và technical failure chưa tách.

Customer cancel và no answer chưa tách.

Customer need support bị auto cancel.

Technical failure bị quy lỗi khách.

No answer sau từng attempt tự gửi tin nhắn.

Max attempts tự hủy hoặc tự notification.

Notification handoff không chờ Core Order State Machine.

IVR confirmation bị hiểu là PAID.

IVR confirmation bị hiểu là Verified Revenue.

Privacy boundary chưa khóa.

Full address/payment/member/Diamond/health note có thể bị đọc/gửi sai policy.

Evidence/call log/audit không bắt buộc.

Human/Admin Override thiếu evidence/audit.

Runtime unavailable không fail-safe.

Conflict resolution chưa có rule ưu tiên.

Documentation Complete bị gọi là Production Ready.

## 30. RELEASE HANDOFF - SANG PHẦN 4/4

## PHẦN 3/4 bàn giao sang PHẦN 4/4 các nội dung sau:

IVR lifecycle tổng thể.

State machine đầy đủ.

Attempt policy baseline.

Golden Hour timing.

24/7 timing.

Outcome classification.

Core Order Handoff.

Notification Handoff.

Privacy boundary.

Anti-Fake-Order handling.

Fail-safe model.

Human/Admin Review model.

P0 điểm chặn Registry.

Smoke định hướng.

Evidence package yêu cầu.

## PHẦN 4/4 phải tiếp tục viết:

Final IVR Smoke Matrix / Evidence Package / Done Gate / Fail Gate / Release Handoff / TECH-09 Final Conclusion

Trong đó phải khóa rõ:

Smoke matrix tổng thể IVR.

Evidence package cuối cùng.

Owner review points.

Privacy review.

Anti-fake-order review.

Order state review.

Notification handoff review.

Release handoff sang các TECH tiếp theo.

Final conclusion.

## 31. TRẠNG THÁI SAU PHẦN 3/4

Sau PHẦN 3/4:

## PHẦN 1/4 đã khóa nguyên tắc.

## PHẦN 2/4 đã khóa module contracts.

## PHẦN 3/4 đã khóa lifecycle/state machine/attempt policy/outcome classification/core order handoff.

Chưa được gọi là Production Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Go-live Approved.

Chưa được gọi là IVR Ready.

Chưa được gọi là Anti-Fake-Order Ready.

Global Gateway vẫn mặc định:

bị chặn

Chỉ sau khi hoàn tất PHẦN 4/4, có accepted evidence, smoke pass, owner sign-off và release decision thì mới được xét trạng thái cao hơn.

## KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 đã khóa vòng đời vận hành IVR từ lúc official order được xét eligibility đến lúc kết quả cuộc gọi được phân loại và bàn giao về Core Order State Machine.

Các điểm đã khóa:

IVR chỉ đi sau official order từ Commerce.

Quote, cart, order draft không có đường vào IVR.

Khách trusted không bị gọi đại trà.

## MAX_ATTEMPT_PER_ORDER V1.0 = PROGRAM_BASED.

Golden Hour dùng 2 attempts cách 5 phút trong bối cảnh 10 phút.

24/7 dùng 3 attempts theo lịch T0 / T0 + 5 phút / T0 + 10 phút trong bối cảnh 15 phút.

Không được đổi 24/7 xuống 2 attempts hoặc áp một max attempt chung nếu chưa viết lại bản sạch.

Invalid phone, no answer, customer cancel, customer need support và technical failure được tách rõ.

No answer sau từng attempt không được tự nhắn/hủy.

No answer max attempts chỉ bàn giao Core Order State Machine.

Notification Owner chỉ được gửi sau khi Core Order State Machine hủy chính thức.

Tin nhắn sau hủy phải privacy-safe, không chứa full address/payment/member/Diamond/health note.

IVR confirmation không đồng nghĩa PAID.

IVR confirmation không đồng nghĩa Verified Revenue.

Technical failure không được quy lỗi khách.

Runtime unavailable thì fail-safe.

Human/Admin Override phải có quyền, lý do, evidence và audit.

Sale Lock / Recall / Suppression thắng IVR.

## PHẦN 3/4 sẵn sàng bàn giao sang:

## PHẦN 4/4 - FINAL IVR SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-09 FINAL CONCLUSION.

## PHẦN 4/4 - FINAL IVR SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-09 FINAL CONCLUSION

## 1. MỤC TIÊU PHẦN 4/4

## PHẦN 4/4 khóa lớp kiểm thử, bằng chứng, điều kiện hoàn tất tài liệu và điều kiện bàn giao release cho TECH-09 - IVR Order Confirmation.

PHẦN này xác định rõ:

Smoke test tổng thể bắt buộc cho IVR.

Evidence package bắt buộc.

Review point của từng owner.

Privacy review.

Anti-fake-order review.

Order state review.

Notification handoff review.

Done Gate.

Fail Gate.

Release Handoff.

Trạng thái cuối cùng của TECH-09 sau khi viết xong tài liệu.

## PHẦN 4/4 không viết code.

## PHẦN 4/4 không thiết kế API chi tiết.

## PHẦN 4/4 không thiết kế database chi tiết.

## PHẦN 4/4 không thiết kế UI chi tiết.

## 2. NGUYÊN TẮC FINAL GATE CỦA TECH-09

TECH-09 chỉ được xem là hoàn tất tài liệu khi đủ 4 phần:

## PHẦN 1/4 - Principles / Source-of-Truth / Eligibility Boundary / No-Mass-Calling Rule.

## PHẦN 2/4 - Module Contracts.

## PHẦN 3/4 - Lifecycle / State Machine / Attempt Policy / Outcome Classification / Core Order Handoff.

## PHẦN 4/4 - Final Smoke Matrix / Evidence Package / Done Gate / Fail Gate / Release Handoff.

Tuy nhiên:

Documentation Complete không đồng nghĩa Production Ready.

Documentation Complete không đồng nghĩa IVR Ready.

Documentation Complete không đồng nghĩa Release Ready.

Documentation Complete không đồng nghĩa Go-live Approved.

Documentation Complete không đồng nghĩa Global Gateway PASS.

Muốn xét lên các trạng thái cao hơn phải có:

Accepted evidence.

Smoke pass.

Owner sign-off.

Release decision.

Global Gateway decision.

## 3. FINAL SMOKE MATRIX - NHÓM A: ORDER ELIGIBILITY

IVR-FSMK-001 - Chỉ Official Order mới được vào IVR

Given hệ thống có Quote, Cart, Order Draft và Official Order
When IVR Eligibility Resolver chạy
Then chỉ Official Order hợp lệ mới được xét IVR
Must Not Quote/Cart/Order Draft được tạo call attempt.

P0 Fail If: IVR gọi cho Quote, Cart hoặc Order Draft.

IVR-FSMK-002 - Khách mới được xét IVR

Given khách mới, chưa có lịch sử mua hàng, official order hợp lệ
When eligibility được đánh giá
Then order được đánh dấu có thể cần IVR theo policy.

P0 Fail If: khách mới rủi ro bị bỏ qua xác minh mà không có policy.

IVR-FSMK-003 - Khách trusted không bị gọi đại trà

Given khách cũ, có lịch sử nhận hàng tốt, dữ liệu đáng tin cậy, không có rủi ro mới
When order được tạo
Then IVR không gọi đại trà.

P0 Fail If: trusted customer vẫn bị gọi mặc định.

IVR-FSMK-004 - Đơn có Sale Lock / Recall bị chặn

Given official order liên quan SKU/batch bị Sale Lock hoặc Recall
When IVR Eligibility Resolver chạy
Then IVR phải block, không gọi.

P0 Fail If: IVR xác nhận đơn đang Sale Lock / Recall.

## 4. FINAL SMOKE MATRIX - NHÓM B: CUSTOMER TRUST / RISK / ANTI-FAKE-ORDER

IVR-FSMK-005 - Customer Trust Resolver loại trừ đúng

Given khách có trust status đạt ngưỡng
When IVR Eligibility chạy
Then order được loại khỏi IVR nếu không có tín hiệu rủi ro mới.

Evidence Required: trust reason code, policy version, audit.

IVR-FSMK-006 - Khách cũ nhưng thông tin nhận hàng bất thường

Given khách cũ nhưng đổi số điện thoại/người nhận/địa chỉ bất thường
When Customer Trust Resolver chạy
Then không tự xem là trusted; route IVR hoặc Human Review theo policy.

P0 Fail If: hệ thống tự bỏ qua IVR chỉ vì khách từng mua.

IVR-FSMK-007 - Đơn có tín hiệu fake order

Given đơn từ kênh live/comment có dấu hiệu spam, phá hoại, số điện thoại mới
When Anti-Fake-Order Resolver chạy
Then hệ thống đánh risk và yêu cầu IVR/Human Review/Hold/Block theo policy.

P0 Fail If: đơn rủi ro vẫn đi thẳng giao hàng.

IVR-FSMK-008 - Complaint thật không bị xử như troll

Given khách có mã đơn/mã lô/bằng chứng khiếu nại thật
When Anti-Fake-Order Resolver đánh giá
Then route Complaint/CSKH/Quality, không auto blacklist, không xử như troll.

P0 Fail If: complaint thật bị đưa vào Purchase Block sai.

## 5. FINAL SMOKE MATRIX - NHÓM C: PHONE VALIDATION

IVR-FSMK-009 - Invalid phone do sai định dạng

Given số điện thoại thiếu chữ số hoặc sai định dạng
When Phone Validation Resolver chạy
Then outcome = INVALID_PHONE, không tạo call attempt.

P0 Fail If: số invalid vẫn được gọi.

IVR-FSMK-010 - Invalid phone không phải No Answer

Given provider/telco xác nhận số không tồn tại
When outcome được ghi nhận
Then kết quả là INVALID_PHONE, không phải NO_ANSWER.

P0 Fail If: invalid phone bị ghi là khách không nghe.

IVR-FSMK-011 - Phone validation lỗi kỹ thuật

Given Phone Validation Resolver unavailable
When IVR chuẩn bị gọi
Then fail-safe, không tự gọi, không tự đánh invalid phone.

P0 Fail If: validation lỗi nhưng IVR vẫn gọi khách.

## 6. FINAL SMOKE MATRIX - NHÓM D: ATTEMPT POLICY

IVR-FSMK-012 - Golden Hour attempt policy

Given order thuộc Golden Hour
When Call Attempt Policy Resolver chạy
Then MAX_ATTEMPT_PER_ORDER = 2, spacing = 5 phút, IVR confirmation window = 10 phút.

P0 Fail If: IVR gọi quá 2 attempts hoặc kéo dài giá Golden Hour.

IVR-FSMK-013 - 24/7 attempt policy

Given order thuộc chương trình 24/7
When Call Attempt Policy Resolver chạy
Then MAX_ATTEMPT_PER_ORDER = 3, schedule = T0 / T0 + 5 phút / T0 + 10 phút, call window = 15 phút.

P0 Fail If: IVR giữ giá 24/7 vượt Commerce policy.

IVR-FSMK-014 - Program-based attempt policy không áp sai chương trình

Given TECH-09 V1.0 khóa program-based attempts
When policy chạy
Then Golden Hour dùng 2 attempts và 24/7 dùng 3 attempts đúng chương trình.

P0 Fail If: tài liệu hoặc runtime áp attempt policy sai chương trình.

IVR-FSMK-015 - Không gọi lại vô hạn

Given khách không nghe
When đã đạt MAX_ATTEMPT_PER_ORDER
Then IVR dừng gọi và bàn giao Core Order State Machine.

P0 Fail If: IVR gọi lại vô hạn.

## 7. FINAL SMOKE MATRIX - NHÓM E: CALL OUTCOME CLASSIFICATION

IVR-FSMK-016 - Customer Confirm

Given khách bắt máy và bấm xác nhận hợp lệ
When Call Outcome Classifier chạy
Then outcome = CUSTOMER_CONFIRMED.

Must Not: chuyển PAID, Verified Revenue, Delivery Success.

IVR-FSMK-017 - Customer Cancel

Given khách bắt máy và bấm hủy hợp lệ
When Call Outcome Classifier chạy
Then outcome = CUSTOMER_CANCELLED_VIA_IVR.

P0 Fail If: customer cancel bị ghi thành no answer.

IVR-FSMK-018 - Customer Need Support

Given khách chọn cần hỗ trợ
When IVR xử lý outcome
Then route Human/CSKH/Admin Review, không tự hủy, không tự xác nhận.

P0 Fail If: need support bị auto cancel.

IVR-FSMK-019 - No Answer lần 1

Given số điện thoại valid, máy đổ chuông hợp lệ, khách không nghe attempt 1
When outcome được phân loại
Then outcome = NO_ANSWER, không gửi notification, không hủy đơn.

IVR-FSMK-020 - No Answer Max Reached

Given 2 attempts hợp lệ đều đổ chuông nhưng khách không nghe
When outcome được phân loại
Then outcome = NO_ANSWER_MAX_ATTEMPT_REACHED và chỉ bàn giao Core Order State Machine.

P0 Fail If: IVR tự gửi SMS hoặc tự hủy đơn.

IVR-FSMK-021 - Busy / Rejected / Unreachable

Given máy bận, khách từ chối cuộc gọi hoặc thuê bao tạm không liên lạc được
When outcome được phân loại
Then phải ghi BUSY / REJECTED / UNREACHABLE riêng theo policy, không tự gộp thành NO_ANSWER.

IVR-FSMK-022 - Technical Failure

Given provider lỗi, call setup failed, adapter lỗi hoặc runtime lỗi
When outcome được phân loại
Then outcome = TECHNICAL_FAILURE / PROVIDER_FAILURE, không quy lỗi khách.

P0 Fail If: lỗi kỹ thuật bị ghi thành khách không nghe.

## 8. FINAL SMOKE MATRIX - NHÓM F: CORE ORDER STATE MACHINE

IVR-FSMK-023 - IVR không tự hủy đơn

Given outcome bất kỳ từ IVR
When cần thay đổi trạng thái đơn
Then phải bàn giao Core Order State Machine.

P0 Fail If: IVR tự chuyển ORDER_CANCELLED.

IVR-FSMK-024 - Customer Confirm không phải PAID

Given outcome = CUSTOMER_CONFIRMED
When Core Order State Machine xử lý
Then chỉ xử lý trạng thái xác nhận đơn, payment status không đổi nếu chưa có Payment Confirmation.

P0 Fail If: IVR confirmation làm đơn thành PAID.

IVR-FSMK-025 - No Answer Max chỉ là handoff

P0 Fail If: IVR tự hủy trước Core.

IVR-FSMK-026 - Technical Failure không làm hủy đơn

Given outcome = TECHNICAL_FAILURE
When Core Order State Machine nhận outcome
Then route retry/human review/hold theo policy, không hủy đơn như lỗi khách.

## 9. FINAL SMOKE MATRIX - NHÓM G: NOTIFICATION HANDOFF

IVR-FSMK-027 - Không gửi thông báo khi Core chưa hủy

Given no answer max reached nhưng Core Order State Machine chưa hủy chính thức
When Notification Handoff Resolver chạy
Then không gửi notification.

P0 Fail If: notification gửi trước trạng thái hủy chính thức.

IVR-FSMK-028 - Notification sau hủy là transactional

Given Core Order State Machine đã hủy chính thức do không xác nhận được
When Notification Owner nhận handoff
Then chỉ gửi tin nhắn giao dịch, không CRM, không marketing, không upsell.

IVR-FSMK-029 - Privacy-safe cancellation notice

Given thông báo sau hủy được tạo
When privacy check chạy
Then không chứa full address, payment detail, member tier, Diamond/referral, health note.

P0 Fail If: tin nhắn sau hủy lộ dữ liệu riêng tư.

IVR-FSMK-030 - SIM Gateway không tự gửi

Given cuộc gọi IVR thất bại hoặc khách không nghe
When SIM Gateway nhận trạng thái kỹ thuật
Then SIM Gateway không tự gửi tin nhắn hủy.

P0 Fail If: SIM Gateway tự gửi SMS sau từng attempt.

IVR-FSMK-031 - AI Advisor không tự phát tin hủy

Given đơn bị IVR no answer max reached
When AI Advisor có context đơn
Then AI Advisor không tự gửi tin hủy nếu chưa có Notification Owner handoff.

## 10. FINAL SMOKE MATRIX - NHÓM H: PRIVACY / CALL SCRIPT

IVR-FSMK-032 - IVR không đọc full address

Given order có full address
When IVR script được chuẩn bị
Then không đọc full address nếu chưa có privacy policy rõ.

IVR-FSMK-033 - IVR không đọc payment/member/Diamond

Given order có payment detail, member tier, Diamond/referral benefit
When IVR script được chuẩn bị
Then không đọc các thông tin này.

IVR-FSMK-034 - IVR không đọc health note

Given customer profile có health note hoặc nội dung tư vấn riêng tư
When IVR script chạy
Then không đọc health note.

IVR-FSMK-035 - IVR không tự sinh script bằng AI chưa duyệt

Given thiếu approved call script
When IVR chuẩn bị gọi
Then fail-safe, không dùng nội dung tự sinh chưa qua guard.

P0 Fail If: IVR đọc script tự sinh chưa approved.

## 11. FINAL SMOKE MATRIX - NHÓM I: EVIDENCE / AUDIT

IVR-FSMK-036 - Outcome phải có evidence

Given call outcome bất kỳ
When outcome được gửi vào Core Order State Machine
Then phải có evidence/call log/audit.

P0 Fail If: outcome thiếu evidence vẫn chuyển state.

IVR-FSMK-037 - Attempt phải có attempt number

Given call attempt được ghi nhận
When audit được kiểm tra
Then phải có attempt number, timestamp, policy version, correlation id.

IVR-FSMK-038 - Admin Override phải có audit

Given admin override IVR outcome
When override được thực hiện
Then phải có actor, permission, reason, evidence, before/after state, audit.

P0 Fail If: override bằng miệng hoặc không có audit.

IVR-FSMK-039 - Evidence không dùng cho marketing

Given call log/recording/evidence tồn tại
When hệ thống marketing/CRM truy cập
Then không được dùng evidence IVR cho marketing.

## 12. FINAL SMOKE MATRIX - NHÓM J: RUNTIME FAIL-SAFE

IVR-FSMK-040 - Commerce unavailable

Given Commerce Runtime unavailable
When IVR nhận yêu cầu
Then không tự gọi, không tự suy đoán order status.

IVR-FSMK-041 - Core Order State Machine unavailable

Given Core Order State Machine unavailable
When IVR có outcome
Then không tự hủy/xác nhận, route fail-safe.

IVR-FSMK-042 - Evidence Registry unavailable

Given Evidence/Audit unavailable
When call outcome xuất hiện
Then không dùng outcome để chuyển state.

IVR-FSMK-043 - Policy Resolver unavailable

Given Attempt Policy hoặc Privacy Policy unavailable
When IVR chuẩn bị gọi
Then không tự chọn rule mặc định gây rủi ro.

## 13. FINAL SMOKE MATRIX - NHÓM K: RELEASE GOVERNANCE

IVR-FSMK-044 - Documentation Complete không phải Production Ready

Given TECH-09 đã viết đủ 4 phần
When release status được kiểm tra
Then chỉ được đánh DOCUMENTATION COMPLETE, không Production Ready.

IVR-FSMK-045 - Không có evidence thì không Completion Decision

Given tài liệu đã viết xong nhưng chưa có accepted evidence
When completion gate chạy
Then Completion Decision vẫn WAITING/bị chặn.

IVR-FSMK-046 - Không có smoke pass thì không Release Ready

Given smoke chưa chạy hoặc chưa pass
When Release Gate kiểm tra
Then không được Release Ready.

IVR-FSMK-047 - Không có owner sign-off thì không Release Approved

Given smoke pass nhưng chưa có owner sign-off
When Release Gate kiểm tra
Then không được Release Approved.

IVR-FSMK-048 - Không có release decision thì không Go-live Approved

Given đã có evidence/smoke/sign-off nhưng chưa có release decision
When Go-live Gate kiểm tra
Then không được Go-live Approved.

## 14. FINAL EVIDENCE PACKAGE

TECH-09 yêu cầu evidence package tối thiểu gồm các nhóm sau.

## 14.1. Documentation Evidence

TECH-09 PHẦN 1/4 accepted.

TECH-09 PHẦN 2/4 accepted.

TECH-09 PHẦN 3/4 accepted.

TECH-09 PHẦN 4/4 accepted.

P0 điểm chặn Registry accepted.

Dependency map accepted.

No-Mass-Calling Rule accepted.

IVR Source-of-Truth Boundary accepted.

Order Draft vs Official Order Boundary accepted.

Notification Owner Handoff accepted.

## 14.2. Runtime Policy Evidence

IVR eligibility policy.

Customer trust policy.

Anti-fake-order policy.

Call attempt policy.

MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED policy.

Golden Hour spacing policy.

24/7 spacing policy.

Phone validation policy.

Call outcome classification policy.

Privacy call script policy.

Notification privacy policy.

Human/Admin Override policy.

## 14.3. Call Evidence

Order reference.

Customer reference.

Phone reference.

Attempt number.

Attempt timestamp.

Attempt policy version.

Provider result.

Ring status.

Connected status.

Keypress/input result nếu có.

Outcome classification.

Outcome reason code.

Evidence integrity status.

Correlation id.

Audit log.

## 14.4. State Machine Evidence

Order state before IVR.

Eligibility state.

Confirmation state.

Attempt state.

Outcome state.

Core Order State Machine handoff.

Core Order State Machine decision.

Order state after Core handling.

Cancellation reason nếu có.

Human review state nếu có.

Admin override state nếu có.

## 14.5. Notification Evidence

Core cancellation reference.

Cancellation reason.

IVR outcome reference.

Privacy-safe content check.

Suppression/opt-out/legal check.

Notification Owner handoff.

Notification Owner acceptance.

Notification delivery status.

Audit log.

## 14.6. Review Evidence

Commerce Owner review.

IVR Owner review.

Customer Trust / CRM Owner review.

Anti-Fake-Order Owner review.

Notification Owner review.

Privacy/Legal review.

Security/RBAC review.

Operational/Sale Lock review.

QA/UAT review.

Release Owner decision.

## 15. OWNER REVIEW POINTS

## 15.1. Commerce Owner Review

Commerce Owner phải xác nhận:

IVR chỉ xử lý Official Order.

Quote/Cart/Order Draft không vào IVR.

IVR confirmation không phải payment.

IVR confirmation không phải Verified Revenue.

IVR không tự sửa giá.

IVR không kéo dài quote freeze.

Golden Hour / 24/7 timing không phá Commerce Runtime.

Core Order State Machine mới quyết định hủy/xác nhận trạng thái đơn.

## 15.2. IVR Owner Review

IVR Owner phải xác nhận:

No-Mass-Calling Rule đã đúng.

Call attempt policy rõ.

## MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED.

Golden Hour spacing đúng.

24/7 spacing đúng.

Outcome classification đầy đủ.

Technical failure không quy lỗi khách.

Evidence/call log đầy đủ.

Không có auto retry vô hạn.

## 15.3. Customer Trust / CRM Owner Review

Customer Trust / CRM Owner phải xác nhận:

Khách trusted không gọi đại trà.

Khách mới/untrusted được xét IVR.

Customer Memory không bị dùng vượt privacy.

IVR không biến thành CRM/marketing.

Tin nhắn sau hủy không phải upsell.

Notification sau hủy không vi phạm opt-out/suppression.

## 15.4. Anti-Fake-Order Owner Review

Anti-Fake-Order Owner phải xác nhận:

Đơn ảo/đơn phá hoại được kiểm soát.

Troll/malicious không được hợp thức hóa bằng IVR.

Complaint thật không bị xử như troll.

Purchase Block/Blacklist có evidence.

Human/Admin Review có audit.

Risk reason code đầy đủ.

## 15.5. Notification Owner Review

Notification Owner phải xác nhận:

IVR không tự gửi notification.

SIM Gateway không tự gửi tin sau từng attempt.

AI Advisor không tự phát tin hủy.

Notification chỉ gửi sau Core cancellation.

Nội dung là transactional.

Nội dung privacy-safe.

Không chứa full address/payment/member/Diamond/health note.

## 15.6. Privacy / Legal Review

Privacy / Legal phải xác nhận:

IVR không đọc full address mặc định.

IVR không đọc payment detail.

IVR không đọc member tier.

IVR không đọc Diamond/referral.

IVR không đọc health note.

Call log/recording nếu có được kiểm soát.

Evidence không dùng cho marketing.

Notification sau hủy tuân thủ privacy.

## 15.7. Release Owner Review

Release Owner phải xác nhận:

Documentation Complete chỉ là trạng thái tài liệu.

Evidence đã accepted hay chưa.

Smoke đã pass hay chưa.

Owner sign-off đã đủ hay chưa.

Release decision đã có hay chưa.

Global Gateway có được mở hay vẫn bị chặn.

## 16. PRIVACY REVIEW GATE

TECH-09 FAIL Privacy Review nếu:

IVR đọc full address khi chưa có policy.

IVR đọc payment detail.

IVR đọc member tier.

IVR đọc Diamond/referral benefit.

IVR đọc health note.

IVR đọc lịch sử mua hàng chi tiết.

IVR đọc nội dung tư vấn riêng tư.

Notification sau hủy chứa dữ liệu riêng tư.

Call recording bị public.

Call evidence được dùng cho marketing.

AI Advisor nhận dữ liệu IVR vượt Final Response Guard.

Live/Comment public lộ kết quả IVR.

Privacy Review chỉ PASS khi có:

Approved privacy call script.

Allowed field list.

Masked field list.

Forbidden field list.

Notification privacy template.

Evidence access policy.

Audit trail.

## 17. ANTI-FAKE-ORDER REVIEW GATE

TECH-09 FAIL Anti-Fake-Order Review nếu:

IVR gọi mọi khách hàng.

Khách trusted bị gọi đại trà.

Khách mới/untrusted không được xét rủi ro.

Đơn phá hoại được hợp thức hóa bằng IVR.

Purchase Block bị bỏ qua.

Troll/malicious vẫn tạo order không qua policy.

Complaint thật bị xử như troll.

Invalid phone bị trộn với no answer.

Technical failure bị quy lỗi khách.

No answer tự động biến thành blacklist.

Customer cancel tự động biến thành malicious.

Risk decision không có reason code/evidence/audit.

Anti-Fake-Order Review chỉ PASS khi có:

Risk signal registry.

Risk reason code.

Purchase Block policy.

Complaint route policy.

Human review route.

Admin override audit.

Evidence accepted.

## 18. ORDER STATE REVIEW GATE

TECH-09 FAIL Order State Review nếu:

IVR tự tạo order.

IVR tự sửa order amount.

IVR tự xác nhận payment.

IVR tự xác nhận COD success.

IVR tự xác nhận Verified Revenue.

IVR tự hủy order.

IVR outcome không qua Core Order State Machine.

Customer confirmation chuyển thẳng sang PAID.

No answer max reached tự hủy đơn.

Technical failure làm hủy đơn.

Customer need support bị auto cancel.

Sale Lock / Recall không thắng IVR.

Order State Review chỉ PASS khi:

Official Order boundary đúng.

Core Order State Machine handoff đúng.

Payment boundary đúng.

Verified Revenue boundary đúng.

Cancellation reason đúng.

Evidence/audit đầy đủ.

No bypass rule được chứng minh bằng smoke.

## 19. NOTIFICATION HANDOFF REVIEW GATE

TECH-09 FAIL Notification Handoff Review nếu:

IVR tự gửi notification.

SIM Gateway tự gửi SMS sau từng attempt.

AI Advisor tự phát tin hủy.

Notification gửi trước khi Core hủy chính thức.

Notification sau hủy là marketing/upsell.

Notification sau hủy chứa full address.

Notification sau hủy chứa payment detail.

Notification sau hủy chứa member tier.

Notification sau hủy chứa Diamond/referral.

Notification sau hủy chứa health note.

Notification không có privacy check.

Notification không có audit.

Notification Handoff Review chỉ PASS khi:

Core cancellation reference có.

IVR outcome evidence có.

Privacy-safe content pass.

Suppression/opt-out/legal pass.

Notification Owner nhận handoff.

Delivery evidence/audit có.

## 20. FINAL DONE GATE - TECH-09

TECH-09 đạt DOCUMENTATION COMPLETE khi:

## PHẦN 1/4 hoàn tất.

## PHẦN 2/4 hoàn tất.

## PHẦN 3/4 hoàn tất.

## PHẦN 4/4 hoàn tất.

IVR Runtime Boundary đã khóa.

IVR Source-of-Truth Boundary đã khóa.

No-Mass-Calling Rule đã khóa.

Customer Eligibility Boundary đã khóa.

Trusted Customer Exclusion đã khóa.

Order Draft vs Official Order Boundary đã khóa.

IVR không thay Commerce/Payment/Verified Revenue đã khóa.

Call Attempt Policy đã khóa.

MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED đã khóa.

Golden Hour timing context đã khóa.

24/7 timing context đã khóa.

Phone Validation đã khóa.

Invalid phone / no answer / customer cancel / technical failure đã tách rõ.

Core Order State Machine Handoff đã khóa.

Notification Owner Handoff đã khóa.

Privacy-safe notification đã khóa.

Evidence / Call Log / Audit đã khóa.

Human/Admin Override đã khóa.

Anti-Fake-Order handling đã khóa.

Runtime unavailable fail-safe đã khóa.

Smoke Matrix tổng thể đã có.

Evidence Package đã có.

Owner Review Points đã có.

Privacy Review Gate đã có.

Anti-Fake-Order Review Gate đã có.

Order State Review Gate đã có.

Notification Handoff Review Gate đã có.

Release Handoff đã có.

Final Conclusion đã có.

## 21. FINAL FAIL GATE - TECH-09

TECH-09 FAIL nếu còn bất kỳ điểm nào sau:

IVR bị hiểu là gọi cho mọi khách hàng.

Khách trusted bị gọi đại trà.

Quote được gọi IVR.

Cart được gọi IVR.

Order Draft được gọi IVR.

IVR tự tạo order.

IVR tự sửa giá.

IVR tự xác nhận payment.

IVR tự xác nhận Verified Revenue.

IVR confirmation bị hiểu là PAID.

IVR confirmation bị hiểu là revenue.

IVR tự hủy order.

IVR tự gửi notification.

SIM Gateway tự gửi SMS sau từng attempt.

AI Advisor tự phát tin hủy đơn.

Notification gửi trước khi Core Order State Machine hủy chính thức.

Invalid phone bị trộn với no answer.

No answer bị trộn với technical failure.

Customer cancel bị trộn với no answer.

Customer need support bị auto cancel.

Technical failure bị quy lỗi khách.

Attempt lỗi kỹ thuật bị tính sai.

Gọi quá MAX_ATTEMPT_PER_ORDER.

áp attempt policy sai chương trình.

Golden Hour timing sai.

24/7 timing sai.

IVR kéo dài quote freeze.

IVR giữ giá ngoài Commerce policy.

IVR đọc full address/payment/member/Diamond/health note.

Notification sau hủy lộ dữ liệu riêng tư.

Notification sau hủy dùng để upsell.

Outcome thiếu evidence vẫn chuyển state.

Admin override thiếu audit.

Sale Lock / Recall / Suppression bị bypass.

Complaint thật bị xử như troll.

Runtime unavailable nhưng IVR vẫn tự gọi/xác nhận/hủy.

Documentation Complete bị gọi là Production Ready.

Không accepted evidence nhưng Completion Decision.

Không smoke pass nhưng Release Ready.

Không owner sign-off nhưng Release Approved.

Không release decision nhưng Go-live Approved.

Global Gateway mở khi gate còn thiếu.

## 22. RELEASE HANDOFF

## 22.1. Trạng thái sau khi hoàn tất TECH-09

Sau khi PHẦN 4/4 hoàn tất:

Nhưng:

Global Gate bị chặn

## 22.2. Điều kiện nâng từ Documentation Complete lên Completion Decision

Muốn xét Completion Decision cần:

Evidence package được nộp.

Evidence được review.

Evidence được accepted.

Không còn P0 điểm chặn.

Owner xác nhận tài liệu đúng scope.

Release Owner ghi nhận trạng thái.

Nếu chưa có accepted evidence:

## 22.3. Điều kiện nâng lên Release Ready

Muốn xét Release Ready cần:

Completion Decision.

Smoke execution đã chạy.

Toàn bộ P0 smoke pass.

Không có unresolved điểm chặn.

Privacy smoke pass.

Notification smoke pass.

Order state smoke pass.

Anti-fake-order smoke pass.

Fail-safe smoke pass.

Nếu smoke chưa pass:

## 22.4. Điều kiện nâng lên Release Approved

Muốn xét Release Approved cần:

Release Ready.

Commerce Owner sign-off.

IVR Owner sign-off.

Customer Trust / CRM Owner sign-off.

Notification Owner sign-off.

Privacy/Legal sign-off.

Anti-Fake-Order Owner sign-off.

Security/RBAC sign-off.

Release Owner approval.

Nếu owner chưa sign-off:

## 22.5. Điều kiện nâng lên Go-live Approved

Muốn xét Go-live Approved cần:

Release Approved.

Production configuration review.

Provider readiness evidence.

Call script approved.

Notification template approved.

Rollback/fallback plan.

Monitoring/alert readiness.

Incident owner assignment.

Final release decision.

Nếu chưa có release decision:

## 23. HANDOFF SANG CÁC TECH TIẾP THEO

TECH-09 bàn giao các rule sau cho các tài liệu tiếp theo:

## 23.1. Bàn giao sang Global Release / UAT

IVR chỉ test bằng official order hợp lệ.

Không test bằng quote/cart/order draft như đơn thật.

Test phải có case khách mới, khách trusted, khách rủi ro.

Test phải có invalid phone/no answer/customer cancel/technical failure.

Test phải có privacy-safe notification.

Test phải có no-notification-before-core-cancel.

Test phải có runtime unavailable fail-safe.

## 23.2. Bàn giao sang Production Config

## MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED.

Golden Hour spacing = 5 phút.

24/7 schedule = T0 / T0 + 5 phút / T0 + 10 phút.

Không để Golden Hour/24/7 dùng sai program-based attempt policy nếu chưa viết lại bản sạch.

Provider config không được tự gửi SMS.

Call script phải approved.

Recording/call log phải theo privacy policy.

Notification Owner phải tách khỏi IVR/SIM Gateway/AI Advisor.

## 23.3. Bàn giao sang Monitoring / Alert

Alert khi call provider failure tăng.

Alert khi technical failure tăng.

Alert khi evidence write failure.

Alert khi invalid phone rate bất thường.

Alert khi no answer rate bất thường.

Alert khi notification handoff failed.

Alert khi override tăng bất thường.

Alert khi fake-order risk tăng.

Alert khi IVR cố gọi order không eligible.

Alert khi có attempt vượt policy.

## 23.4. Bàn giao sang Operations / CSKH

Customer need support phải có queue xử lý.

Invalid phone unresolved phải có quy trình xử lý.

No answer max reached phải theo Core Order State Machine.

Customer cancel phải phân biệt với không nghe máy.

Technical failure không được đổ lỗi khách.

Complaint thật phải route CSKH/Quality.

Admin override phải có reason/evidence/audit.

Hạng mục

Trạng thái sau TECH-09

TECH-09 Documentation

Completion Decision

## WAITING_EVIDENCE

Production Ready

KHONG

IVR Ready

KHONG

Release Ready

KHONG

Release Approved

KHONG

Go-live Approved

KHONG

Global Gateway

bị chặn

Evidence Package

## REQUIRED

Smoke Execution

## REQUIRED

Owner Sign-off

## REQUIRED

Release Decision

## REQUIRED

## TECH-09 FINAL CONCLUSION

TECH-09 đã khóa lớp IVR Order Confirmation cho Ginsengfood theo hướng greenfield.

Các nguyên tắc cuối cùng được khóa:

IVR không gọi cho mọi khách hàng.

IVR chỉ áp dụng cho khách mới, khách untrusted, khách thiếu dữ liệu xác thực hoặc đơn có rủi ro theo policy.

Khách cũ/trusted đủ dữ liệu không bị gọi đại trà.

IVR không gọi cho Quote.

IVR không gọi cho Cart.

IVR không gọi cho Order Draft.

IVR chỉ xử lý Official Order đủ điều kiện từ Commerce.

IVR không tự tạo order.

IVR không tự sửa tiền.

IVR không tự xác nhận payment.

IVR không tự xác nhận Verified Revenue.

IVR confirmation không đồng nghĩa PAID.

IVR confirmation không đồng nghĩa Verified Revenue.

IVR chỉ xác nhận ý chí đặt hàng/thông tin cơ bản theo policy.

Invalid phone phải tách khỏi no answer.

Máy đổ chuông nhưng khách không nghe phải tách khỏi invalid phone.

Technical failure phải tách khỏi lỗi khách.

Customer cancel phải tách khỏi no answer.

Customer need support phải route Human/CSKH/Admin Review.

TECH-09 V1.0 khóa MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED.

Golden Hour tham chiếu 2 attempts cách 5 phút trong 10 phút.

24/7 tham chiếu 3 attempts theo lịch T0 / T0 + 5 phút / T0 + 10 phút.

Không được đổi 24/7 xuống 2 attempts hoặc áp một max attempt chung nếu chưa viết lại bản sạch.

No answer sau từng cuộc không được tự nhắn/hủy.

No answer max reached chỉ bàn giao Core Order State Machine.

Chỉ sau khi Core Order State Machine hủy chính thức thì Notification Owner mới được gửi tin nhắn giao dịch.

Tin nhắn sau hủy không phải CRM/marketing/upsell.

Tin nhắn sau hủy không do IVR tự gửi.

Tin nhắn sau hủy không do SIM Gateway tự gửi.

Tin nhắn sau hủy không do AI Advisor tự phát.

Tin nhắn sau hủy không chứa full address, payment detail, member tier, Diamond/referral, health note.

IVR không đọc dữ liệu riêng tư vượt privacy policy.

IVR result phải có evidence/call log/audit.

Technical failure không được quy lỗi khách.

Runtime unavailable thì fail-safe, không tự gọi, không tự xác nhận, không tự hủy.

Human/Admin Override phải có quyền, lý do, evidence và audit.

Sale Lock / Recall / Suppression thắng IVR.

Complaint thật không được xử như troll.

Troll/malicious/fake-order phải theo Purchase Block/Anti-Fake-Order policy.

Documentation Complete không phải Production Ready.

Trạng thái cuối cùng của tài liệu:

Nhưng các trạng thái vận hành vẫn là:

TECH-09 chỉ được đưa vào triển khai thật khi có đủ:

Accepted evidence.

Smoke pass.

Owner sign-off.

Release decision.

Global Gateway approval.
