**PACK-09 - IVR_ORDER_CONFIRMATION**

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

**IVR ORDER CONFIRMATION / INTERNAL SIM GATEWAY GOVERNANCE**

**V1.0 CLEAN FINAL**

**1\. DOCUMENT STATUS**

**1.1. Tên tài liệu**

**PACK-09 - IVR_ORDER_CONFIRMATION**  
**IVR ORDER CONFIRMATION / INTERNAL SIM GATEWAY GOVERNANCE**  
**V1.0 CLEAN FINAL**

**1.2. Vai trò tài liệu**

Tài liệu này là tài liệu quản trị cấp pack cho hợp phần IVR của hệ thống Ginsengfood.

PACK-09 được dùng để chuẩn hóa:

- Cuộc gọi tự động xác nhận đơn hàng.
- Mô hình triển khai bằng SIM Gateway nội bộ.
- Quy tắc gọi theo chương trình Giờ Vàng và 24/7.
- Nội dung cuộc gọi.
- Phím xác nhận/hủy đơn.
- Năng lực SIM Gateway.
- Ranh giới với Core Order State Machine.
- Ranh giới với AI Advisor, CRM, Facebook Gateway, Admin, kho, MISA.
- Điều kiện triển khai, kiểm thử, evidence và production gate.

**2\. PACK STATUS**

**2.1. Trạng thái pack**

PACK_09_NAME = IVR_ORDER_CONFIRMATION  
PACK_09_STATUS = DOCUMENTATION_ACTIVE  
PACK_09_IMPLEMENTATION_STATUS = NOT_STARTED  
PACK_09_PRODUCTION_STATUS = NOT_READY

**2.2. Trạng thái gate**

IVR_GATE = BLOCKED  
IVR_PRODUCTION_READY = NO  
CALLABLE_IVR_API_ALLOWED_IN_PRODUCTION = NO  
IVR_RESULT_SIMULATION_ALLOWED = NO  
DOWNSTREAM_IVR_DEPENDENCY_ALLOWED = NO

**2.3. Trạng thái toàn hệ**

GATEWAY_STATUS = BLOCKED  
COMPLETION_REPORT_STATUS = PENDING_EVIDENCE  
PRODUCTION_READY = NO  
RELEASE_APPROVED = NO  
GO_LIVE_APPROVED = NO

**3\. PACK PURPOSE**

PACK-09 phục vụ một nghiệp vụ duy nhất:

CALL_PURPOSE = ORDER_CONFIRMATION_ONLY

Mục tiêu:

- Xác nhận đơn hàng bằng cuộc gọi tự động.
- Giảm đơn ảo, đơn sai số điện thoại, đơn đặt nhầm, đơn không xác nhận.
- Hỗ trợ Core Order State Machine quyết định tiếp tục xử lý hoặc hủy đơn theo rule.
- Không biến IVR thành kênh bán hàng, CRM, chăm sóc khách hàng hay marketing.

**4\. DEPLOYMENT MODEL**

**4.1. Mô hình triển khai chính thức**

IVR_DEPLOYMENT_MODEL = INTERNAL_SIM_GATEWAY_SERVER

Hệ thống sử dụng:

- SIM Gateway nội bộ.
- SIM vật lý.
- Server nội bộ điều phối cuộc gọi.
- Call scheduler nội bộ.
- DTMF capture nội bộ.
- Call result normalizer nội bộ.
- Audit/evidence nội bộ.
- Core Order State Machine xử lý kết quả cuối.

**4.2. Mô hình không dùng làm mặc định**

VOICE_BRANDNAME_PROVIDER_MODEL = NOT_DEFAULT  
SIP_TRUNK_PROVIDER_MODEL = NOT_DEFAULT  
CLOUD_IVR_PROVIDER_MODEL = NOT_DEFAULT  
TELECOM_PROVIDER_MANAGED_IVR = NOT_DEFAULT

**4.3. Nguyên tắc SIM channel**

1 SIM = 1 ACTIVE OUTBOUND CALL

| **Cấu hình** | **Số cuộc gọi đồng thời tối đa** |
| ------------ | -------------------------------- |
| 12 SIM       | 12 cuộc gọi                      |
| 24 SIM       | 24 cuộc gọi                      |
| 32 SIM       | 32 cuộc gọi                      |

**5\. CALL SCRIPT**

**5.1. Nội dung cuộc gọi chuẩn**

Ginsengfood kính chào Mình.  
<br/>Ginsengfood cảm ơn Mình đã chọn Cháo Sâm Savigin.  
<br/>Đơn hàng {{order_code_short}} của Mình có tổng thanh toán {{total_amount_display}}.  
<br/>Để xác nhận tiếp tục xử lý đơn hàng, Mình vui lòng bấm phím 1.  
<br/>Nếu Mình không đặt hoặc muốn hủy đơn này, Mình vui lòng bấm phím 0.  
<br/>Ginsengfood trân trọng cảm ơn ạ.

**5.2. Biến được phép dùng**

| **Biến**             | **Ý nghĩa**             | **Trạng thái** |
| -------------------- | ----------------------- | -------------- |
| order_code_short     | Mã đơn rút gọn          | ALLOWED        |
| total_amount_display | Tổng thanh toán         | ALLOWED        |
| customer_name_short  | Tên gọi ngắn nếu có     | OPTIONAL       |
| program_name         | Giờ Vàng / 24/7 nếu cần | OPTIONAL       |

Không đọc trong cuộc gọi:

FULL_CUSTOMER_PROFILE = NO  
FULL_ADDRESS = NO  
MEMBER_TIER = NO  
DIAMOND_REFERRAL_INFO = NO  
PAYMENT_DETAIL = NO  
ORDER_HISTORY = NO  
AI_CONSULTATION_CONTENT = NO  
CRM_CONTENT = NO  
HEALTH_OR_SENSITIVE_NOTE = NO

**6\. DTMF KEY RULE**

KEY_1 = CONFIRM_ORDER  
KEY_0 = CANCEL_ORDER_BY_CUSTOMER  
KEY_9_HUMAN_SUPPORT = NOT_ENABLED

| **Phím**  | **Ý nghĩa**                | **Hành động**                  |
| --------- | -------------------------- | ------------------------------ |
| 1         | Khách xác nhận đơn         | Core tiếp tục xử lý đơn        |
| 0         | Khách không đặt / muốn hủy | Core hủy đơn qua state machine |
| Không bấm | Không có xác nhận hợp lệ   | Xử lý theo attempt/window      |
| Sai phím  | Không có input hợp lệ      | Xử lý theo rule IVR            |
| Lỗi DTMF  | Lỗi kỹ thuật               | Không tính là khách không nghe |

**7\. PROGRAM-BASED CALL RULE**

**7.1. Rule chung**

MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED  
ATTEMPT_SCHEDULE = PROGRAM_BASED  
SIM_GATEWAY_DIRECT_ORDER_UPDATE = NO  
ORDER_STATE_CHANGE_MUST_PASS_CORE_STATE_MACHINE = YES

Nếu một attempt có kết quả cuối, không gọi các attempt còn lại.

Kết quả cuối gồm:

IVR_CONFIRMED  
IVR_CUSTOMER_CANCELLED  
INVALID_PHONE_FINAL  
TECHNICAL_EXCEPTION_FINAL

**8\. GOLDEN HOUR RULE**

PROGRAM = GOLDEN_HOUR  
IVR_CONFIRMATION_WINDOW = 10_MINUTES  
MAX_ATTEMPT = 2  
ATTEMPT_INTERVAL = 5_MINUTES

Lịch gọi:

T0 = thời điểm đơn cần xác nhận IVR  
<br/>ATTEMPT_1 = T0  
ATTEMPT_2 = T0 + 5 phút  
WINDOW_EXPIRES = T0 + 10 phút

Mapping:

| **Tình huống**                  | **IVR Result**                  | **Core Action**                   |
| ------------------------------- | ------------------------------- | --------------------------------- |
| Cuộc 1 nghe + bấm 1             | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 1 nghe + bấm 0             | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 1 không nghe               | ATTEMPT_1_NO_ANSWER             | Gọi cuộc 2 sau 5 phút             |
| Cuộc 2 nghe + bấm 1             | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 2 nghe + bấm 0             | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 2 không nghe               | IVR_NO_ANSWER_FINAL             | Core hủy đơn                      |
| Hết 10 phút chưa xác nhận hợp lệ | IVR_CONFIRMATION_WINDOW_EXPIRED | Core hủy đơn / quote hết hiệu lực |

**9\. 24/7 RULE**

PROGRAM = TWENTY_FOUR_SEVEN  
IVR_CONFIRMATION_WINDOW = 15_MINUTES  
MAX_ATTEMPT = 3  
ATTEMPT_INTERVAL = 5_MINUTES

Lịch gọi:

T0 = thời điểm đơn cần xác nhận IVR  
<br/>ATTEMPT_1 = T0  
ATTEMPT_2 = T0 + 5 phút  
ATTEMPT_3 = T0 + 10 phút  
WINDOW_EXPIRES = T0 + 15 phút

Mapping:

| **Tình huống**                   | **IVR Result**                  | **Core Action**                   |
| -------------------------------- | ------------------------------- | --------------------------------- |
| Cuộc 1 nghe + bấm 1              | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 1 nghe + bấm 0              | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 1 không nghe                | ATTEMPT_1_NO_ANSWER             | Gọi cuộc 2 sau 5 phút             |
| Cuộc 2 nghe + bấm 1              | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 2 nghe + bấm 0              | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 2 không nghe                | ATTEMPT_2_NO_ANSWER             | Gọi cuộc 3 sau 5 phút             |
| Cuộc 3 nghe + bấm 1              | IVR_CONFIRMED                   | Tiếp tục xử lý đơn                |
| Cuộc 3 nghe + bấm 0              | IVR_CUSTOMER_CANCELLED          | Core hủy đơn                      |
| Cuộc 3 không nghe                | IVR_NO_ANSWER_FINAL             | Core hủy đơn                      |
| Hết 15 phút chưa xác nhận hợp lệ | IVR_CONFIRMATION_WINDOW_EXPIRED | Core hủy đơn / quote hết hiệu lực |

**10\. ORDER STATE BOUNDARY**

**10.1. IVR không cập nhật đơn trực tiếp**

IVR_CAN_TRIGGER_CANCEL_REQUEST = YES  
IVR_CAN_DIRECTLY_CANCEL_ORDER = NO  
SIM_GATEWAY_CAN_CANCEL_ORDER = NO  
CORE_ORDER_STATE_MACHINE_CANCEL_REQUIRED = YES

**10.2. Reason code hủy đơn**

ORDER_CANCEL_REASON = CUSTOMER_CANCELLED_BY_IVR_KEY_0  
ORDER_CANCEL_REASON = IVR_NO_ANSWER_AFTER_2_ATTEMPTS     # GOLDEN_HOUR
ORDER_CANCEL_REASON = IVR_NO_ANSWER_AFTER_3_ATTEMPTS     # TWENTY_FOUR_SEVEN  
ORDER_CANCEL_REASON = IVR_CONFIRMATION_WINDOW_EXPIRED  
ORDER_CANCEL_REASON = IVR_INVALID_PHONE_IF_RULE_CONFIRMED

**10.3. Lỗi kỹ thuật không phải khách không nghe**

Các lỗi sau không được xử lý như NO_ANSWER:

SIM_GATEWAY_ERROR  
SERVER_ERROR  
DTMF_CAPTURE_ERROR  
AUDIO_PLAYBACK_ERROR  
SIM_CHANNEL_FAILURE  
INTERNAL_CALLBACK_ERROR  
SCHEDULER_ERROR

Các lỗi này phải đi vào:

IVR_TECHNICAL_EXCEPTION  
ADMIN_REVIEW_REQUIRED

**11\. CAPACITY BASELINE**

**11.1. Chu kỳ tính tải**

AVERAGE_CALL_DURATION = 35_SECONDS  
CONSERVATIVE_CALL_CYCLE = 50_SECONDS_PER_CALL_PER_SIM

**11.2. Năng lực trong 5 phút**

| **Số SIM** | **Năng lực 5 phút** |
| ---------- | ------------------- |
| 12 SIM     | ~72 cuộc            |
| 24 SIM     | ~144 cuộc           |
| 32 SIM     | ~192 cuộc           |

**11.3. Năng lực trong 15 phút**

| **Số SIM** | **Năng lực 15 phút** |
| ---------- | -------------------- |
| 12 SIM     | ~216 cuộc            |
| 24 SIM     | ~432 cuộc            |
| 32 SIM     | ~576 cuộc            |

**11.4. Năng lực trong phiên 45 phút nếu rolling queue**

| **Số SIM** | **Năng lực 45 phút** |
| ---------- | -------------------- |
| 12 SIM     | ~648 cuộc            |
| 24 SIM     | ~1.296 cuộc          |
| 32 SIM     | ~1.728 cuộc          |

**11.5. Khuyến nghị cấu hình**

PILOT_SIM_COUNT = 12  
MINIMUM_LAUNCH_SIM_COUNT = 24  
RECOMMENDED_MONTH_1_2_SIM_COUNT = 32  
RECOMMENDED_MONTH_3_4_SIM_COUNT = 64  
RECOMMENDED_MONTH_5_6_SIM_COUNT = 96

**11.6. Scheduler rule**

ROLLING_REAL_TIME_IVR = REQUIRED  
BATCH_AFTER_SESSION_CALLING = PROHIBITED  
SCHEDULER_MODEL = DEADLINE_AWARE_ROLLING_QUEUE

Không dồn toàn bộ cuộc gọi cuối phiên.

**12\. CROSS-PACK BOUNDARY**

**12.1. Boundary với Commerce Order Core / IVR / Operational Core**

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

PACK-09 chỉ cung cấp IVR result như tín hiệu đầu vào.

IVR_RESULT_IS_INPUT_SIGNAL_ONLY  
ORDER_STATE_MACHINE_IS_FINAL_DECISION_LAYER

**12.2. Với AI Advisor**

AI Advisor không được:

- Tạo IVR call job.
- Gọi SIM Gateway.
- Hiển thị IVR result.
- Tự nói đơn đã IVR xác nhận nếu Core chưa công nhận.

**12.3. Với Facebook Gateway / Messenger / Live**

Facebook Gateway không được:

- Gọi IVR trực tiếp.
- Tạo call job.
- Dùng comment/Messenger để giả lập IVR result.
- Thay IVR bằng tin nhắn xác nhận không có rule.

**12.4. Với CRM / Member Lifecycle**

CRM không được dùng IVR cho:

- Tin nhắn chăm sóc.
- Nhắc mua lại.
- Nhắc quyền lợi thành viên.
- Chúc mừng sinh nhật.
- Nhắc Giờ Vàng.
- Mời Diamond.

**12.5. Với Payment / MISA / Warehouse / Traceability**

IVR không được:

- Xác nhận thanh toán.
- Đối soát MISA.
- Cho xuất kho.
- Cho giao vận.
- Tác động lô/batch/QR/truy vết.
- Tác động recall.

**13\. DOCUMENT SET**

PACK-09 gồm các file chính:

IVR-00 - Governance / Source of Truth / Scope Boundary  
IVR-01 - Business Purpose / Confirmation Use Case  
IVR-02 - Ownership Boundary / Connected Systems  
IVR-03 - Eligibility Rule / Customer Trust / Official Contact  
IVR-04 - Order Core -> IVR Task Contract  
IVR-05 - Attempt Policy / Scheduler / Queue  
IVR-06 - Internal SIM Gateway Adapter  
IVR-07 - Result Normalization / IVR -> Order Core Callback  
IVR-08 - Admin Monitoring / Evidence / Audit / Privacy  
IVR-09 - Test Matrix / Smoke / Release Gate

Tài liệu gap-closure tiền SRS:

IVR-PRE-SRS-GAP-CLOSURE - ivr-pre-srs-gap-closure.md

Bộ SRS chi tiết Phase 8:

IVR-SRS-TRACE - IVR-SRS-trace-matrix.md  
IVR-00 - IVR-00-governance-source-of-truth-scope-boundary.md  
IVR-01 - IVR-01-business-purpose-confirmation-use-case.md  
IVR-02 - IVR-02-ownership-boundary-connected-systems.md  
IVR-03 - IVR-03-eligibility-customer-trust-official-contact.md  
IVR-04 - IVR-04-order-core-to-ivr-task-contract.md  
IVR-05 - IVR-05-attempt-policy-scheduler-queue.md  
IVR-06 - IVR-06-internal-sim-gateway-adapter.md  
IVR-07 - IVR-07-result-normalization-order-core-callback.md  
IVR-08 - IVR-08-admin-monitoring-evidence-audit-privacy.md  
IVR-09 - IVR-09-test-matrix-smoke-release-gate.md

**14\. IMPLEMENTATION BOUNDARY**

**14.1. Trạng thái triển khai**

PACK_09_IMPLEMENTATION_STATUS = NOT_STARTED  
PRODUCTION_IMPLEMENTATION_ALLOWED = NO

**14.2. Được phép chuẩn bị kỹ thuật nội bộ**

Đội kỹ thuật được phép chuẩn bị:

- Khảo sát SIM Gateway.
- Test server kết nối SIM Gateway.
- Test số nội bộ.
- Test DTMF nội bộ.
- Draft skeleton backend.
- Draft Admin UI.
- Draft test matrix.

Nhưng bắt buộc khóa:

REAL_CUSTOMER_CALL_ALLOWED = NO  
REAL_ORDER_STATE_UPDATE_ALLOWED = NO  
PRODUCTION_SIM_GATEWAY_ALLOWED = NO  
PRODUCTION_ORDER_CANCEL_BY_IVR_ALLOWED = NO

**15\. ENGINEER HANDOFF MODEL**

**15.1. Kỹ sư 1 - IVR Backend / SIM Gateway**

Phụ trách:

SimChannelManager  
CallJobQueue  
DeadlineAwareScheduler  
CallExecutionAdapter  
DtmfCaptureHandler  
CallResultNormalizer  
OrderStateMachineAdapter  
AuditEvidenceWriter  
CapacityIncidentMonitor  
SimHealthMonitor

Không được:

Gọi khách thật  
Tự hủy đơn trực tiếp  
Tự cập nhật order state  
Tự mở API production  
Tự đổi rule gọi  
Tự thêm phím 9  
Tính lỗi kỹ thuật là khách không nghe

**15.2. Kỹ sư 2 - Admin UI / Monitoring / Evidence / Test**

Phụ trách:

/admin/ivr/dashboard  
/admin/ivr/call-jobs  
/admin/ivr/call-jobs/{id}  
/admin/ivr/sim-channels  
/admin/ivr/capacity-incidents  
/admin/ivr/audit-evidence

Không được:

Cho Admin sửa IVR result giả  
Cho Admin hủy đơn ngoài Core State Machine  
Hiển thị dữ liệu nhạy cảm không cần thiết  
Tạo button gọi lại ngoài retry rule  
Tạo action chưa có permission

**16\. P0 RULES**

**P0-01 - Internal SIM Gateway**

**MUST**  
PACK-09 dùng mô hình Internal SIM Gateway Server.

**MUST NOT**  
Không mặc định dùng cloud IVR / SIP / voice brandname provider.

**FAIL IF**  
Tài liệu hoặc code lấy provider bên ngoài làm mặc định khi không có quyết định riêng.

**P0-02 - One SIM one active call**

**MUST**  
Một SIM chỉ xử lý một cuộc gọi đang active.

**MUST NOT**  
Không assign nhiều call job vào cùng một SIM đang bận.

**FAIL IF**  
Scheduler giao trùng SIM channel.

**P0-03 - Program window**

**MUST**  
Giờ Vàng: 10 phút, 2 cuộc, cách 5 phút.  
24/7: 15 phút, 3 cuộc, lịch T0 / T0 + 5 phút / T0 + 10 phút.

**MUST NOT**  
Không gọi dồn liên tục hoặc gọi sau khi hết window.

**FAIL IF**  
Attempt 2 không cách đúng 1/2 window.

**P0-04 - Key mapping**

**MUST**  
Phím 1 xác nhận. Phím 0 hủy đơn.

**MUST NOT**  
Không tự thêm phím 9 hoặc phím khác.

**FAIL IF**  
DTMF mapping sai rule.

**P0-05 - Core State Machine**

**MUST**  
Mọi hủy/tiếp tục xử lý đơn phải qua Core Order State Machine.

**MUST NOT**  
SIM Gateway không cập nhật order trực tiếp.

**FAIL IF**  
SIM Gateway hoặc IVR callback đổi order state trực tiếp.

**P0-06 - Technical error boundary**

**MUST**  
Lỗi kỹ thuật đi vào IVR_TECHNICAL_EXCEPTION.

**MUST NOT**  
Không tính lỗi kỹ thuật là khách không nghe.

**FAIL IF**  
SIM lỗi nhưng order bị cancel bằng IVR_NO_ANSWER_AFTER_PROGRAM_ATTEMPTS.

**P0-07 - Rolling queue**

**MUST**  
Dùng deadline-aware rolling queue.

**MUST NOT**  
Không batch toàn bộ cuộc gọi cuối phiên.

**FAIL IF**  
32 SIM bị giao xử lý 800-1.200 cuộc trong 5 phút.

**P0-08 - No downstream dependency**

**MUST**  
Downstream chưa được phụ thuộc IVR result khi chưa release.

**MUST NOT**  
Order/CRM/AI/Facebook/Admin không dùng IVR result như dữ liệu production.

**FAIL IF**  
Có module dùng IVR result trước khi IVR test/smoke/evidence pass.

**P0-09 - No release without evidence**

**MUST**  
Test/smoke/security/evidence là bắt buộc.

**MUST NOT**  
Không release bằng lời xác nhận miệng.

**FAIL IF**  
Completion Report PASS nhưng thiếu evidence.

**17\. DONE GATE**

PACK-09 governance chỉ đạt khi:

PACK_09_GOVERNANCE_DONE_GATE = PASS

Điều kiện PASS:

- Có mô hình Internal SIM Gateway.
- Có call script chuẩn.
- Có phím 1/phím 0.
- Có rule Giờ Vàng.
- Có rule 24/7.
- Có capacity baseline.
- Có cross-pack boundary.
- Có Core State Machine boundary.
- Có technical exception boundary.
- Có engineer handoff model.
- Có P0 MUST / MUST NOT / FAIL IF.
- Không có nội dung nào cho phép production runtime.

**18\. FINAL STATUS**

PACK_09_GOVERNANCE_STATUS = CLEAN_FINAL  
PACK_09_NAME = IVR_ORDER_CONFIRMATION  
PACK_09_DEPLOYMENT_MODEL = INTERNAL_SIM_GATEWAY_SERVER  
<br/>PACK_09_IMPLEMENTATION_STATUS = NOT_STARTED  
IVR_GATE = BLOCKED  
IVR_PRODUCTION_READY = NO  
CALLABLE_IVR_API_ALLOWED_IN_PRODUCTION = NO  
DOWNSTREAM_IVR_DEPENDENCY_ALLOWED = NO  
<br/>GATEWAY_STATUS = BLOCKED  
COMPLETION_REPORT_STATUS = PENDING_EVIDENCE  
PRODUCTION_READY = NO

**Kết luận:**  
Tài liệu sạch này xác định PACK-09 là hợp phần IVR chính thức của hệ thống Ginsengfood, triển khai theo mô hình Internal SIM Gateway Server, phục vụ xác nhận đơn hàng bằng phím 1/phím 0, có rule riêng cho Giờ Vàng và 24/7, có capacity baseline theo 12/24/32 SIM, nhưng vẫn chưa được mở production cho đến khi hoàn tất IVR-00 → IVR-09, implementation, test, smoke, security và evidence.
