**PACK-09 IVR INPUT BASELINE**

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

**INTERNAL SIM GATEWAY / CALL SCRIPT / ATTEMPT RULE / CAPACITY PLAN**

**V1.0 CLEAN FINAL**

**SRS ALIGNMENT NOTE**

Tài liệu này là baseline kỹ thuật đầu vào cho bộ SRS Phase 8. Khi có khác biệt giữa bản baseline này và bộ SRS `IVR-00` đến `IVR-09`, phải xử lý theo thứ tự: nguồn gốc trong `docs/source-map.md`, PACK-09, TECH-09, `ivr-pre-srs-gap-closure.md`, rồi đến từng file SRS chi tiết. Không được dùng tài liệu này để mở production nếu `IVR-09` chưa pass release gate.

**1\. MÔ HÌNH TRIỂN KHAI CHÍNH THỨC**

**1.1. Owner Deployment Decision**

PACK-09 IVR được thiết kế theo hướng:

IVR_DEPLOYMENT_MODEL = INTERNAL_SIM_GATEWAY_SERVER

Nghĩa là hệ thống sử dụng:

- Bộ SIM Gateway nội bộ.
- SIM vật lý.
- Server nội bộ điều phối cuộc gọi.
- Call scheduler nội bộ.
- DTMF capture nội bộ.
- Call result callback nội bộ về Core.
- Audit/evidence nội bộ.

Không lấy mô hình mặc định là:

VOICE_BRANDNAME_PROVIDER = NO  
SIP_TRUNK_PROVIDER = NO  
CLOUD_IVR_PROVIDER = NO  
TELECOM_PROVIDER_MANAGED_IVR = NO

**1.2. Nguyên tắc vận hành**

1 SIM = 1 concurrent outbound call

Nghĩa là:

- Bộ 12 SIM xử lý tối đa khoảng 12 cuộc gọi đồng thời.
- Bộ 24 SIM xử lý tối đa khoảng 24 cuộc gọi đồng thời.
- Bộ 32 SIM xử lý tối đa khoảng 32 cuộc gọi đồng thời.

Nếu một SIM đang gọi, SIM đó không được nhận thêm call job mới.

**2\. KIẾN TRÚC IVR NỘI BỘ**

**2.1. Các thành phần bắt buộc**

Order Core  
↓  
IVR Eligibility Rule  
↓  
IVR Call Scheduler  
↓  
SIM Gateway Channel Pool  
↓  
GSM/SIM Call Execution  
↓  
DTMF Capture  
↓  
IVR Result Normalizer  
↓  
Core Order State Machine  
↓  
Audit / Evidence / Admin Monitoring

**2.2. Vai trò từng lớp**

| **Lớp**                  | **Vai trò**                                 |
| ------------------------ | ------------------------------------------- |
| Order Core               | Quyết định đơn nào cần IVR                  |
| IVR Eligibility Rule     | Kiểm tra chương trình, window, risk, trust  |
| IVR Call Scheduler       | Xếp lịch gọi đúng thời hạn                  |
| SIM Gateway Channel Pool | Quản lý SIM rảnh/bận/lỗi                    |
| Call Execution           | Thực hiện cuộc gọi                          |
| DTMF Capture             | Ghi nhận phím 1 hoặc 0                      |
| Result Normalizer        | Chuẩn hóa kết quả cuộc gọi                  |
| Core State Machine       | Xử lý tiếp tục đơn hoặc hủy đơn             |
| Audit/Evidence           | Lưu log, lý do, thời điểm, kết quả          |
| Admin Monitoring         | Theo dõi cuộc gọi, lỗi, backlog, SIM health |

**3\. NỘI DUNG CUỘC GỌI CHÍNH THỨC**

**3.1. Call Purpose**

CALL_PURPOSE = ORDER_CONFIRMATION_ONLY

IVR chỉ xác nhận đơn hàng.

IVR không được:

- Bán thêm.
- Tư vấn sản phẩm.
- Đọc combo.
- Đọc chương trình khuyến mãi.
- Gọi chăm sóc khách hàng.
- Gọi CRM.
- Mời thành viên.
- Mời Diamond.
- Đọc địa chỉ đầy đủ.
- Đọc thông tin nhạy cảm.

**3.2. Mẫu lời gọi chính thức**

Ginsengfood kính chào Mình.  
<br/>Ginsengfood cảm ơn Mình đã chọn Cháo Sâm Savigin.  
<br/>Đơn hàng {{order_code_short}} của Mình có tổng thanh toán {{total_amount_display}}.  
<br/>Để xác nhận tiếp tục xử lý đơn hàng, Mình vui lòng bấm phím 1.  
<br/>Nếu Mình không đặt hoặc muốn hủy đơn này, Mình vui lòng bấm phím 0.  
<br/>Ginsengfood trân trọng cảm ơn ạ.

**3.3. Phím bấm chính thức**

KEY_1 = CONFIRM_ORDER  
KEY_0 = CANCEL_ORDER_BY_CUSTOMER

Giai đoạn đầu chưa mở:

KEY_9_HUMAN_SUPPORT = NOT_ENABLED

Lý do: nếu mở phím 9 trong giờ cao điểm, đội CSKH có thể bị dồn cuộc gọi ngược và làm nghẽn vận hành.

**3.4. Sau khi khách bấm phím 1**

Ginsengfood đã ghi nhận xác nhận của Mình.  
<br/>Đơn hàng sẽ được chuyển sang bước xử lý tiếp theo.  
<br/>Ginsengfood trân trọng cảm ơn ạ.

Không được nói:

Đơn hàng đã được xuất kho.  
Đơn hàng chắc chắn giao thành công.  
Đơn hàng đã hoàn tất.

Vì phím 1 chỉ là xác nhận IVR, đơn vẫn phải đi qua Core Order State Machine.

**3.5. Sau khi khách bấm phím 0**

Ginsengfood đã ghi nhận Mình muốn hủy đơn hàng này.  
<br/>Đơn hàng sẽ được cập nhật theo quy trình xử lý đơn hàng.  
<br/>Ginsengfood trân trọng cảm ơn ạ.

Khóa kỹ thuật:

KEY_0 triggers cancel request.  
SIM Gateway must not cancel order directly.  
Core Order State Machine performs cancellation.

**4\. QUY TẮC GỌI THEO CHƯƠNG TRÌNH**

**4.1. Rule chung**

MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED  
ATTEMPT_SCHEDULE = PROGRAM_BASED  
SIM_GATEWAY_DIRECT_ORDER_UPDATE = NO  
ORDER_STATE_CHANGE_MUST_PASS_CORE_STATE_MACHINE = YES

Nếu một attempt có kết quả cuối thì không gọi các attempt còn lại.

Kết quả cuối gồm:

IVR_CONFIRMED  
IVR_CUSTOMER_CANCELLED  
INVALID_PHONE_FINAL  
TECHNICAL_EXCEPTION_FINAL

**4.2. Giờ Vàng**

PROGRAM = GOLDEN_HOUR  
IVR_CONFIRMATION_WINDOW = 10_MINUTES  
MAX_ATTEMPT = 2  
ATTEMPT_INTERVAL = 5_MINUTES

Lịch gọi:

T0 = thời điểm đơn cần IVR  
<br/>ATTEMPT_1 = T0  
ATTEMPT_2 = T0 + 5 phút  
WINDOW_EXPIRES = T0 + 10 phút

Mapping:

| **Tình huống**           | **IVR Result**                  | **Core Action**                   |
| ------------------------ | ------------------------------- | --------------------------------- |
| Cuộc 1 nghe + bấm 1      | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 1 nghe + bấm 0      | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 1 không nghe        | ATTEMPT_1_NO_ANSWER             | Gọi cuộc 2 sau 5 phút             |
| Cuộc 2 nghe + bấm 1      | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 2 nghe + bấm 0      | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 2 không nghe        | IVR_NO_ANSWER_FINAL             | Core hủy đơn                      |
| Hết 10 phút chưa xác nhận | IVR_CONFIRMATION_WINDOW_EXPIRED | Core hủy đơn / quote hết hiệu lực |

**4.3. Chương trình 24/7**

PROGRAM = TWENTY_FOUR_SEVEN  
IVR_CONFIRMATION_WINDOW = 15_MINUTES  
MAX_ATTEMPT = 3  
ATTEMPT_INTERVAL = 5_MINUTES

Lịch gọi:

T0 = thời điểm đơn cần IVR  
<br/>ATTEMPT_1 = T0  
ATTEMPT_2 = T0 + 5 phút  
ATTEMPT_3 = T0 + 10 phút  
WINDOW_EXPIRES = T0 + 15 phút

Mapping:

| **Tình huống**            | **IVR Result**                  | **Core Action**                   |
| ------------------------- | ------------------------------- | --------------------------------- |
| Cuộc 1 nghe + bấm 1       | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 1 nghe + bấm 0       | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 1 không nghe         | ATTEMPT_1_NO_ANSWER             | Gọi cuộc 2 sau 5 phút             |
| Cuộc 2 nghe + bấm 1       | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 2 nghe + bấm 0       | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 2 không nghe         | ATTEMPT_2_NO_ANSWER             | Gọi cuộc 3 sau 5 phút             |
| Cuộc 3 nghe + bấm 1       | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 3 nghe + bấm 0       | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 3 không nghe         | IVR_NO_ANSWER_FINAL             | Core hủy đơn                      |
| Hết 15 phút chưa xác nhận | IVR_CONFIRMATION_WINDOW_EXPIRED | Core hủy đơn / quote hết hiệu lực |

**5\. THỜI LƯỢNG CUỘC GỌI ĐỂ TÍNH TẢI**

**5.1. Thời lượng thiết kế**

| **Thành phần**                         | **Thời lượng** |
| -------------------------------------- | -------------- |
| Đổ chuông trước khi tính không nghe    | 20-25 giây     |
| Lời gọi chính                          | 20-25 giây     |
| Chờ bấm phím                           | 5 giây         |
| Lời xác nhận sau phím 1/0              | 5-8 giây       |
| Cooldown giữa 2 cuộc gọi trên cùng SIM | 5 giây         |

**5.2. Thời lượng dùng để tính năng lực**

AVERAGE_CALL_DURATION = 35_SECONDS  
CONSERVATIVE_CALL_CYCLE = 50_SECONDS

Trong tài liệu vận hành nên dùng **50 giây/cuộc/SIM** để tính an toàn.

Nghĩa là 1 SIM trong điều kiện an toàn xử lý khoảng:

60 / 50 = 1.2 cuộc gọi/phút

**6\. BẢNG TÍNH NĂNG LỰC THEO 12 / 24 / 32 SIM**

**6.1. Năng lực theo 5 phút**

Dùng mức an toàn 50 giây/cuộc.

| **Số SIM** | **Cuộc gọi xử lý được trong 10 phút** |
| ---------- | ------------------------------------ |
| 12 SIM     | ~72 cuộc                             |
| 24 SIM     | ~144 cuộc                            |
| 32 SIM     | ~192 cuộc                            |

Ý nghĩa:

- Nếu trong cùng 5 phút phát sinh hơn 192 cuộc gọi cần xử lý, bộ 32 SIM sẽ bắt đầu nghẽn.
- Với Giờ Vàng, đây là điểm rất quan trọng vì window xác nhận chỉ có 10 phút.

**6.2. Năng lực theo 15 phút**

| **Số SIM** | **Cuộc gọi xử lý được trong 15 phút** |
| ---------- | ------------------------------------- |
| 12 SIM     | ~216 cuộc                             |
| 24 SIM     | ~432 cuộc                             |
| 32 SIM     | ~576 cuộc                             |

Ý nghĩa:

- 24/7 dễ xử lý hơn Giờ Vàng vì window dài 15 phút.
- 32 SIM có thể xử lý khoảng 576 cuộc trong 15 phút ở mức an toàn.

**6.3. Năng lực theo một phiên Giờ Vàng 45 phút**

Giả định phiên Giờ Vàng kéo dài 45 phút và IVR chạy rolling theo từng đơn, không dồn cuối phiên.

| **Số SIM** | **Năng lực xử lý trong 45 phút** |
| ---------- | -------------------------------- |
| 12 SIM     | ~648 cuộc gọi                    |
| 24 SIM     | ~1.296 cuộc gọi                  |
| 32 SIM     | ~1.728 cuộc gọi                  |

Kết luận:

| **Phương án** | **Đánh giá**                                                 |
| ------------- | ------------------------------------------------------------ |
| 12 SIM        | Chỉ phù hợp pilot / tải thấp                                 |
| 24 SIM        | Có thể đáp ứng mục tiêu 800-1.200 cuộc/phiên nếu rolling tốt |
| 32 SIM        | Khuyến nghị cho tháng 1-2 vì có buffer an toàn hơn           |
| Trên 32 SIM   | Cần tính cho tháng 3-6 nếu sản lượng tăng mạnh               |

**7\. ĐÁNH GIÁ THEO MỤC TIÊU KINH DOANH**

**7.1. Mục tiêu tải đã chốt**

| **Giai đoạn** | **Cuộc gọi / phiên** | **2 phiên / ngày** |
| ------------- | -------------------- | ------------------ |
| Tháng 1-2     | 800-1.200            | 1.600-2.400        |
| Tháng 3-4     | 1.600-2.400          | 3.200-4.800        |
| Tháng 5-6     | 2.400-3.600          | 4.800-7.200        |

**7.2. So với năng lực 32 SIM**

Với 32 SIM, năng lực an toàn khoảng:

1.728 cuộc gọi / phiên 45 phút  
3.456 cuộc gọi / ngày nếu 2 phiên  
103.680 cuộc gọi / tháng nếu 30 ngày

Đánh giá:

| **Giai đoạn** | **Nhu cầu**       | **32 SIM có đáp ứng không?**             |
| ------------- | ----------------- | ---------------------------------------- |
| Tháng 1-2     | 800-1.200/phiên   | Có, nếu rolling queue tốt                |
| Tháng 3-4     | 1.600-2.400/phiên | Chỉ đáp ứng phần thấp, không đủ phần cao |
| Tháng 5-6     | 2.400-3.600/phiên | Không đủ nếu vẫn gọi cùng tỷ lệ          |

**7.3. Khuyến nghị phase theo SIM Gateway nội bộ**

PHASE_1_MONTH_1_2 = 32_SIM_RECOMMENDED  
PHASE_2_MONTH_3_4 = 64_SIM_RECOMMENDED_IF_CALL_VOLUME_DOUBLES  
PHASE_3_MONTH_5_6 = 96_SIM_RECOMMENDED_IF_CALL_VOLUME_REACHES_3600_PER_SESSION

Nếu chỉ mua tối đa 32 SIM thì phải có các biện pháp giảm tải:

- Chỉ gọi khách mới/chưa đủ trust.
- Không gọi khách cũ đủ tin cậy.
- Không gọi đơn đã xác thực tốt qua kênh khác.
- Giảm retry kỹ thuật lỗi.
- Rolling queue theo từng đơn, không batch cuối phiên.
- Cảnh báo backlog nếu vượt năng lực SIM.

**8\. QUY TẮC SCHEDULER CHO SIM GATEWAY**

**8.1. Không được dồn cuộc gọi cuối phiên**

BATCH_AFTER_SESSION_CALLING = PROHIBITED  
ROLLING_REAL_TIME_IVR = REQUIRED

Lý do:

- Giờ Vàng có 10 phút.
- 32 SIM chỉ xử lý an toàn khoảng 192 cuộc trong 5 phút.
- Nếu dồn 1.200 cuộc cuối phiên, 32 SIM không thể xử lý kịp.

**8.2. Scheduler phải chạy theo deadline từng đơn**

SCHEDULER_MODEL = DEADLINE_AWARE_ROLLING_QUEUE

Ưu tiên gọi theo:

- Đơn sắp hết confirmation window.
- Đơn thuộc Giờ Vàng.
- Đơn có attempt 2 đúng hạn.
- Đơn có risk cao.
- Đơn còn đủ thời gian xử lý.

**8.3. Rule phân bổ SIM**

ONE_SIM_ONE_ACTIVE_CALL = YES  
SIM_COOLDOWN_AFTER_CALL = 5_SECONDS  
SIM_HEALTH_CHECK_REQUIRED = YES  
FAILED_SIM_AUTO_DISABLE = YES

Nếu một SIM lỗi liên tục, phải tự động loại khỏi pool.

Ví dụ rule:

IF SIM_FAIL_COUNT >= 3_WITHIN_10_MINUTES  
THEN SIM_STATUS = TEMPORARILY_DISABLED  
ADMIN_ALERT = YES

**8.4. Capacity incident**

Nếu queue vượt năng lực, phải ghi nhận:

IVR_CAPACITY_INCIDENT = YES

Các trường cần có:

session_id  
program_code  
active_sim_count  
pending_call_jobs  
expired_call_jobs  
missed_deadline_count  
capacity_shortage_reason  
created_at

Không được im lặng làm đơn hết hạn mà không có log.

**9\. QUY TẮC HỦY ĐƠN**

**9.1. Hủy do khách bấm 0**

IF DTMF_KEY = 0  
THEN IVR_RESULT = IVR_CUSTOMER_CANCELLED  
THEN CORE_STATE_MACHINE_CANCEL_ORDER

Reason code:

CUSTOMER_CANCELLED_BY_IVR_KEY_0

**9.2. Hủy do không nghe sau 2 cuộc**

IF ATTEMPT_1 = NO_ANSWER  
AND ATTEMPT_2 = NO_ANSWER  
THEN IVR_RESULT = IVR_NO_ANSWER_FINAL  
THEN CORE_STATE_MACHINE_CANCEL_ORDER

Reason code:

IVR_NO_ANSWER_AFTER_PROGRAM_ATTEMPTS

**9.3. Hủy do hết window**

IF CURRENT_TIME > WINDOW_EXPIRES  
AND NO_VALID_IVR_CONFIRMATION  
THEN IVR_RESULT = IVR_CONFIRMATION_WINDOW_EXPIRED  
THEN CORE_STATE_MACHINE_CANCEL_ORDER

Reason code:

IVR_CONFIRMATION_WINDOW_EXPIRED

**9.4. Lỗi kỹ thuật không được tính là khách không nghe**

Nếu lỗi do SIM Gateway, server, modem, DTMF, audio, callback nội bộ, không được hủy theo lý do khách không nghe.

SIM_GATEWAY_ERROR != CUSTOMER_NO_ANSWER  
SERVER_ERROR != CUSTOMER_NO_ANSWER  
DTMF_CAPTURE_ERROR != CUSTOMER_NO_ANSWER

Các lỗi kỹ thuật phải đi vào:

IVR_TECHNICAL_EXCEPTION  
ADMIN_REVIEW_REQUIRED

**10\. YÊU CẦU PHẦN CỨNG / SERVER NỘI BỘ**

**10.1. Cấu hình triển khai theo phase**

| **Phase**             | **SIM Gateway** | **Vai trò**                               |
| --------------------- | --------------- | ----------------------------------------- |
| Pilot                 | 12 SIM          | Test kỹ thuật, test DTMF, test Core state |
| Launch tháng 1-2      | 24-32 SIM       | Chạy thật nếu rolling queue ổn            |
| Khuyến nghị tháng 1-2 | 32 SIM          | An toàn hơn cho 800-1.200 cuộc/phiên      |
| Tháng 3-4             | 64 SIM          | Nếu tải tăng 100%                         |
| Tháng 5-6             | 96 SIM          | Nếu tải lên 2.400-3.600 cuộc/phiên        |

**10.2. Yêu cầu server điều phối**

Server nội bộ phải có tối thiểu:

SIM_CHANNEL_MANAGER  
CALL_JOB_QUEUE  
DEADLINE_AWARE_SCHEDULER  
DTMF_CAPTURE_HANDLER  
CALL_RESULT_NORMALIZER  
CORE_CALLBACK_ADAPTER  
AUDIT_LOGGER  
EVIDENCE_WRITER  
ADMIN_MONITORING_API  
SIM_HEALTH_MONITOR  
CAPACITY_INCIDENT_MONITOR

**10.3. Yêu cầu log**

Mỗi cuộc gọi phải có log:

call_job_id  
order_id  
program_code  
session_id  
sim_slot_id  
phone_masked  
attempt_no  
scheduled_at  
started_at  
ended_at  
call_duration  
ring_duration  
dtmf_key  
ivr_result  
failure_code  
core_transition_request_id  
correlation_id  
idempotency_key  
created_at

Không được lưu số điện thoại đầy đủ ở các màn hình không cần thiết.

**11\. P0 RULES CHO SIM GATEWAY**

**P0-SIM-001 - Không dùng nhà mạng/provider làm mặc định**

**MUST**

Thiết kế theo SIM Gateway nội bộ.

**MUST NOT**

Không mặc định phụ thuộc Voice Brandname/SIP/Cloud IVR provider.

**FAIL IF**

Fail nếu tài liệu viết provider callback như nguồn chính thay vì SIM Gateway internal result.

**P0-SIM-002 - 1 SIM chỉ xử lý 1 cuộc gọi**

**MUST**

Mỗi SIM chỉ có 1 active call.

**MUST NOT**

Không assign nhiều call job vào cùng một SIM đang bận.

**FAIL IF**

Fail nếu call scheduler không khóa SIM channel khi đang gọi.

**P0-SIM-003 - Không batch cuối phiên**

**MUST**

Dùng rolling queue theo từng đơn.

**MUST NOT**

Không dồn 800-1.200 cuộc gọi vào cuối phiên.

**FAIL IF**

Fail nếu 32 SIM bị giao xử lý 1.200 cuộc trong 5 phút.

**P0-SIM-004 - Scheduler phải theo deadline**

**MUST**

Ưu tiên đơn sắp hết confirmation window.

**MUST NOT**

Không dùng FIFO đơn giản làm trễ đơn Giờ Vàng.

**FAIL IF**

Fail nếu đơn hết 5 phút mà chưa được attempt 1 do queue xử lý sai ưu tiên.

**P0-SIM-005 - Lỗi kỹ thuật không được hủy như khách không nghe**

**MUST**

Phân biệt lỗi khách không nghe và lỗi hệ thống.

**MUST NOT**

Không hủy đơn do "khách không nghe" nếu SIM Gateway không gọi thành công.

**FAIL IF**

Fail nếu SIM lỗi nhưng order bị cancel bằng reason IVR_NO_ANSWER_AFTER_PROGRAM_ATTEMPTS.

**P0-SIM-006 - 32 SIM không đủ cho tháng 3-6 nếu tải tăng mạnh**

**MUST**

Có cảnh báo capacity.

**MUST NOT**

Không giả định 32 SIM đáp ứng mọi phase.

**FAIL IF**

Fail nếu tháng 3-4 hoặc 5-6 tăng tải mà không mở rộng SIM pool hoặc giảm tỷ lệ đơn cần gọi.

**12\. FINAL LOCK**

IVR_DEPLOYMENT_MODEL = INTERNAL_SIM_GATEWAY_SERVER  
<br/>PILOT_SIM_COUNT = 12  
MINIMUM_LAUNCH_SIM_COUNT = 24  
RECOMMENDED_MONTH_1_2_SIM_COUNT = 32  
RECOMMENDED_MONTH_3_4_SIM_COUNT = 64  
RECOMMENDED_MONTH_5_6_SIM_COUNT = 96  
<br/>SIM_PROVIDER_MODEL = INTERNAL  
VOICE_BRANDNAME_PROVIDER_MODEL = NOT_DEFAULT  
SIP_TRUNK_PROVIDER_MODEL = NOT_DEFAULT  
CLOUD_IVR_PROVIDER_MODEL = NOT_DEFAULT  
<br/>CALL_SCRIPT_VERSION = ORDER_CONFIRMATION_V1  
KEY_1 = CONFIRM_ORDER  
KEY_0 = CANCEL_ORDER_BY_CUSTOMER  
<br/>GOLDEN_HOUR_WINDOW = 10_MINUTES  
GOLDEN_HOUR_ATTEMPT_INTERVAL = 5_MINUTES  
<br/>TWENTY_FOUR_SEVEN_WINDOW = 15_MINUTES  
TWENTY_FOUR_SEVEN_ATTEMPT_INTERVAL = 5_MINUTES  
<br/>ONE_SIM_ONE_ACTIVE_CALL = YES  
ROLLING_REAL_TIME_IVR = REQUIRED  
BATCH_AFTER_SESSION_CALLING = PROHIBITED  
CORE_ORDER_STATE_MACHINE_REQUIRED = YES  
AUDIT_EVIDENCE_REQUIRED = YES

**Kết luận kỹ thuật:**  
Nếu triển khai bằng server nội bộ + SIM Gateway, thì **32 SIM là cấu hình nên dùng cho tháng 1-2**, còn **12 SIM chỉ phù hợp pilot**, **24 SIM là mức tối thiểu nhưng ít buffer**. Với mục tiêu tăng tải tháng 3-6, hệ thống phải chuẩn bị khả năng mở rộng lên **64 SIM rồi 96 SIM**, hoặc giảm mạnh tỷ lệ đơn cần gọi bằng Customer Trust Rule.
