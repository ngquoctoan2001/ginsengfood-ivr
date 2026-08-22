# W-0107 — Bảng duyệt từ vựng (OD-L10N-05)

> Sinh tự động từ `admin-ui/src/i18n/enums.vi.json`. Đây là từ vựng nhân viên vận hành
> đọc hằng ngày. Sửa nhãn = sửa một chuỗi JSON, không ảnh hưởng code hay test.

Cột **Mã** luôn hiển thị cạnh nhãn trên giao diện (tooltip ở bảng, dòng mono ở màn chi tiết),
nên một bản dịch sai là **thấy được và sửa được**, không âm thầm.

Tổng: **39 họ / 212 giá trị**

---

## Phần 1 — Bốn họ trọng yếu (đọc kỹ)

Dịch sai ở đây làm nhân viên hành động sai, không chỉ đọc khó.

### `resultType` — 11 giá trị

**Vì sao trọng yếu:** Kết quả cuộc gọi — nhân viên đọc để biết đơn còn cứu được hay đã xong
**Hiện ở:** Nhật ký · Chi tiết · Chờ duyệt · Báo cáo · bộ lọc

| Mã | Nhãn tiếng Việt | Sửa thành (nếu cần) |
| --- | --- | --- |
| `IVR_CONFIRMED` | Khách đã xác nhận | |
| `IVR_CUSTOMER_CANCELLED` | Khách huỷ đơn | |
| `IVR_NO_ANSWER_ATTEMPT` | Không nghe máy — còn lượt gọi | |
| `IVR_NO_ANSWER_FINAL` | Không nghe máy — đã hết lượt | |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | Hết hạn cửa sổ xác nhận | |
| `IVR_INVALID_PHONE_FINAL` | Số điện thoại không dùng được | |
| `IVR_WRONG_INPUT` | Khách bấm sai phím | |
| `IVR_TECHNICAL_EXCEPTION` | Lỗi kỹ thuật | |
| `IVR_CAPACITY_EXCEPTION` | Không đủ năng lực gọi | |
| `IVR_OPERATIONAL_BLOCKED` | Bị chặn do vận hành | |
| `IVR_POLICY_BLOCKED` | Bị chặn do chính sách | |

### `eligibilityDecision` — 12 giá trị

**Vì sao trọng yếu:** REJECTED (bỏ hẳn) và HELD (chạy tiếp) là hai kết cục trái ngược
**Hiện ở:** Chi tiết cuộc gọi

| Mã | Nhãn tiếng Việt | Sửa thành (nếu cần) |
| --- | --- | --- |
| `TASK_ACCEPTED_CALL_JOB_CREATED` | Đã nhận — đã tạo lệnh gọi | |
| `TASK_ACCEPTED_DRY_RUN_ONLY` | Đã nhận — chỉ chạy thử, không gọi thật | |
| `TASK_SKIPPED_TRUSTED_CUSTOMER` | Bỏ qua — khách quen tin cậy | |
| `TASK_REJECTED_NOT_OFFICIAL_ORDER` | Từ chối — không phải đơn chính thức | |
| `TASK_REJECTED_STATE_NOT_CALLABLE` | Từ chối — trạng thái đơn không cho gọi | |
| `TASK_REJECTED_POLICY_MISMATCH` | Từ chối — sai chính sách gọi lại | |
| `TASK_REJECTED_CONTACT_INVALID` | Từ chối — thông tin liên hệ không hợp lệ | |
| `TASK_REJECTED_SCRIPT_NOT_APPROVED` | Từ chối — kịch bản chưa được duyệt | |
| `TASK_REJECTED_INVALID_TRACE` | Từ chối — thiếu hoặc sai correlation id | |
| `TASK_BLOCKED_OPERATIONAL` | Bị chặn — có blocker vận hành | |
| `TASK_HELD_ADMIN_REVIEW` | Giữ lại — chờ quản trị duyệt | |
| `TASK_HELD_POLICY_MISSING` | Giữ lại — thiếu chính sách gọi | |

### `jobStatus` — 27 giá trị

**Vì sao trọng yếu:** 8 giá trị HELD_* chỉ khác phần đuôi — đọc nhầm là gỡ sai nguyên nhân
**Hiện ở:** Nhật ký (2 cột) · Chi tiết · Lượt gọi · bộ lọc

| Mã | Nhãn tiếng Việt | Sửa thành (nếu cần) |
| --- | --- | --- |
| `OPEN` | Đang mở | |
| `QUEUED` | Chờ gọi | |
| `READY_FOR_SCHEDULER` | Sẵn sàng điều phối | |
| `LEASED` | Đã nhận lượt điều phối | |
| `LEASED_PENDING_DISPATCH` | Đã nhận lượt — chờ quay số | |
| `DISPATCH_LEASED` | Đang giữ lượt quay số | |
| `DIALING` | Đang quay số | |
| `ACTIVE_CALL` | Đang trong cuộc gọi | |
| `DISPOSITION_PENDING_NORMALIZATION` | Chờ chuẩn hoá kết quả | |
| `PROVIDER_EVENT_PENDING_NORMALIZATION` | Chờ chuẩn hoá sự kiện nhà mạng | |
| `RESULT_READY_FOR_CALLBACK` | Kết quả sẵn sàng gửi Core | |
| `TECHNICAL_RETRY_QUEUED` | Đã xếp lịch gọi lại do lỗi kỹ thuật | |
| `HELD_MOCK` | Giữ ở chế độ MOCK | |
| `HELD_ADMIN_REVIEW` | Giữ — chờ quản trị duyệt | |
| `HELD_ELIGIBILITY` | Giữ — chờ xét eligibility | |
| `HELD_CAPACITY` | Giữ — thiếu năng lực | |
| `HELD_CALLBACK` | Giữ — chờ callback | |
| `HELD_TECHNICAL_REVIEW` | Giữ — chờ rà lỗi kỹ thuật | |
| `HELD_NORMALIZATION` | Giữ — chờ chuẩn hoá | |
| `HELD_LEASE_RECOVERY` | Giữ — đang khôi phục lượt | |
| `CAPACITY_HELD` | Bị giữ do năng lực | |
| `CAPACITY_MISSED` | Trễ deadline do năng lực | |
| `CLOSED_CAPACITY` | Đóng do thiếu năng lực | |
| `RECOVERY_REQUIRED` | Cần khôi phục | |
| `BLOCKED` | Bị chặn | |
| `SKIPPED` | Đã bỏ qua | |
| `CLOSED` | Đã đóng | |

### `recommendedCoreAction` — 7 giá trị

**Vì sao trọng yếu:** Đề xuất gửi Order Core — đã viết lại để động từ phân biệt lên đầu
**Hiện ở:** Chi tiết cuộc gọi → Kết quả

| Mã | Nhãn tiếng Việt | Sửa thành (nếu cần) |
| --- | --- | --- |
| `REVALIDATE_AND_CONFIRM_ORDER` | Xác nhận đơn — sau khi Core kiểm lại | |
| `REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST` | Huỷ theo yêu cầu khách — sau khi Core kiểm lại | |
| `NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` | Giữ nguyên trạng thái — chờ hết hạn | |
| `REVALIDATE_AND_EXPIRE_CONFIRMATION` | Cho hết hạn xác nhận — sau khi Core kiểm lại | |
| `REVALIDATE_AND_HOLD_ADMIN_REVIEW` | Giữ chờ quản trị duyệt — sau khi Core kiểm lại | |
| `IGNORE_STALE_CALLBACK` | Bỏ qua — callback đã cũ | |
| `BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT` | Chặn — ràng buộc vận hành | |

---

## Phần 2 — Các họ còn lại (rà nhanh)

Ánh xạ cơ học. Dịch sai ở đây là lỗi chính tả, không phải sự cố vận hành.

### `accountRole` — 2 giá trị

**Hiện ở:** Tài khoản

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `Admin` | Quản trị viên |
| `Operator` | Nhân viên vận hành |

### `accountStatus` — 3 giá trị

**Hiện ở:** Tài khoản

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `ACTIVE` | Đang hoạt động |
| `DISABLED` | Đã vô hiệu hoá |
| `DELETED` | Đã xoá mềm |

### `adminActionStatus` — 2 giá trị

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `APPLIED` | Đã thực hiện |
| `RESOLVED` | Đã xử lý |

### `analyticsDimension` — 3 giá trị

**Hiện ở:** Báo cáo

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `RESULT_TYPE` | Loại kết quả |
| `SCRIPT_VARIANT` | Biến thể kịch bản (A/B) |
| `PROGRAM` | Chương trình |

### `approvalType` — 4 giá trị

**Hiện ở:** Cấu hình kịch bản

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `MOCK_TEST` | Kiểm thử mô phỏng |
| `LAB` | Lab |
| `CONTENT` | Nội dung |
| `PRIVACY_LEGAL` | Pháp lý & quyền riêng tư |

### `blockedReason` — 8 giá trị

**Hiện ở:** Chi tiết → Lý do bị chặn

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `RECALL_HOLD_ACTIVE` | Đang giữ do thu hồi sản phẩm |
| `SALE_LOCK_ACTIVE` | Đang khoá bán |
| `QUALITY_HOLD_ACTIVE` | Đang giữ do chất lượng |
| `SELLABLE_SNAPSHOT_MISSING` | Thiếu ảnh chụp khả năng bán |
| `SELLABLE_SNAPSHOT_STALE` | Ảnh chụp khả năng bán đã cũ |
| `SELLABLE_STATUS_UNKNOWN` | Chưa rõ khả năng bán |
| `TRUSTED_CUSTOMER_SKIP` | Bỏ qua vì khách quen tin cậy |
| `BLOCKED_BY_CORE` | Order Core chặn khi kiểm lại |

### `bucket` — 2 giá trị

**Hiện ở:** Báo cáo

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `DAY` | Ngày |
| `HOUR` | Giờ |

### `callbackResultState` — 1 giá trị

**Hiện ở:** Chi tiết → Callback

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `PENDING_CORE_REVALIDATION` | Chờ Core kiểm lại |

### `closedReason` — 1 giá trị

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `IVR_CAPACITY_EXCEPTION` | Đóng do không đủ năng lực gọi |

### `deliveryStatus` — 7 giá trị

**Hiện ở:** Chi tiết → Callback

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `READY` | Sẵn sàng gửi |
| `SENDING` | Đang gửi |
| `RETRY_PENDING` | Chờ gửi lại |
| `SENT` | Đã gửi |
| `ACKED` | Core đã nhận |
| `FAILED` | Gửi thất bại |
| `DEAD_LETTER` | Đưa vào hàng chờ xử lý tay |

### `dependencyDetail` — 5 giá trị

**Hiện ở:** Trạng thái tích hợp → cột Chi tiết

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `OPS_SELLABLE_GATE` | Chưa có thăm dò health của Ops; /health/ready không mang tín hiệu dependency cho tới W-0040. |
| `CRM_DO_NOT_CALL` | Thông tin chặn gọi và mức tin cậy đi kèm trong task từ Sales (W-0031); IVR không giữ client CRM và không thăm dò gì (UT-ARCH-NO-CRM-EGRESS-06). |
| `EVIDENCE_REGISTRY` | Bằng chứng được ghi tại chỗ; chưa có thăm dò kho bên ngoài. |
| `DIAL_KILL_SWITCH_ENGAGED` | Kill switch đang bật; đã chặn quay số |
| `DIAL_KILL_SWITCH_RELEASED` | Kill switch đang tắt |

### `dependencyName` — 6 giá trị

**Hiện ở:** Trạng thái tích hợp

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `SIM_GATEWAY` | Cổng SIM |
| `DIAL_KILL_SWITCH` | Kill switch quay số |
| `ORDER_CORE` | Order Core |
| `OPS_SELLABLE_GATE` | Cổng kiểm khả năng bán (Ops) |
| `CRM_DO_NOT_CALL` | CRM — danh sách chặn gọi |
| `EVIDENCE_REGISTRY` | Kho bằng chứng |

### `dependencyState` — 4 giá trị

**Hiện ở:** Trạng thái tích hợp

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `UP` | Hoạt động |
| `DOWN` | Mất kết nối |
| `READY_503` | Sẵn sàng nhưng trả 503 |
| `NOT_WIRED` | Chưa đấu nối |

### `disposition` — 11 giá trị

**Hiện ở:** Chi tiết → Các lượt gọi

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `Answered` | Khách bắt máy |
| `RingTimeout` | Đổ chuông không ai nghe |
| `Busy` | Máy bận |
| `Rejected` | Khách từ chối cuộc gọi |
| `Unreachable` | Không liên lạc được |
| `InvalidDestination` | Số không tồn tại |
| `Dropped` | Rớt cuộc gọi |
| `NetworkError` | Lỗi mạng |
| `SimError` | Lỗi SIM |
| `AudioError` | Lỗi âm thanh |
| `DtmfError` | Lỗi nhận phím bấm |

### `dtmfMeaning` — 3 giá trị

**Hiện ở:** Cấu hình → Bản đồ phím DTMF

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `CONFIRM` | Xác nhận |
| `CANCEL` | Huỷ |
| `NOT_ENABLED` | Chưa mở |

### `executionMode` — 3 giá trị

**Hiện ở:** Tổng quan · Seed/Mock

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `MOCK` | Mô phỏng |
| `LAB_REAL_SIM` | Lab SIM thật |
| `PRODUCTION_REAL` | Vận hành thật |

### `failClosedEffect` — 6 giá trị

**Hiện ở:** Trạng thái tích hợp → cột Hiệu ứng fail-closed

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `SIM_GATEWAY` | SIM lỗi được ghi là IVR_TECHNICAL_EXCEPTION, không bao giờ ghi thành không nghe máy (DT-02). |
| `DIAL_KILL_SWITCH` | Khi đang bật, không cuộc gọi nào được quay số ở bất kỳ chế độ nào. |
| `ORDER_CORE` | Order Core mất kết nối ⇒ không nhận task mới; callback chỉ gửi lại có giới hạn rồi chuyển chờ quản trị duyệt. |
| `OPS_SELLABLE_GATE` | Trả 503 hoặc mất kết nối ⇒ fail-closed: không quay số và không xác nhận (DO-06). |
| `CRM_DO_NOT_CALL` | CRM mất kết nối ⇒ không xác định được khách đã chặn gọi hay chưa, nên không quay số (DC-01). |
| `EVIDENCE_REGISTRY` | Kho bằng chứng mất kết nối ⇒ không gửi được callback cuối, phiếu bị giữ lại. |

### `failClosedEventSource` — 2 giá trị

**Hiện ở:** Trạng thái tích hợp → Sự kiện

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `CAPACITY_INCIDENT` | Sự cố năng lực |
| `REVIEW_ITEM` | Mục chờ duyệt |

### `freshnessStatus` — 3 giá trị

**Hiện ở:** Báo cáo → độ tươi dữ liệu

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `FRESH` | Mới |
| `STALE` | Đã cũ |
| `NO_DATA` | Chưa có dữ liệu |

### `incidentScope` — 3 giá trị

**Hiện ở:** Tổng quan → Sự cố năng lực

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `ADMIN_QUEUE_PAUSE` | Quản trị tạm dừng hàng đợi |
| `ELIGIBILITY_DEADLINE` | Trễ hạn xét eligibility |
| `SCHEDULER_DEADLINE` | Trễ hạn điều phối |

### `incidentStatus` — 2 giá trị

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `OPEN` | Đang mở |
| `RESOLVED` | Đã xử lý |

### `intakeOutboxStatus` — 3 giá trị

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `HELD_MOCK` | Giữ ở chế độ MOCK |
| `READY_FOR_ELIGIBILITY` | Chờ xét eligibility |
| `PUBLISHED` | Đã phát hành |

### `paymentMethod` — 2 giá trị

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `ONLINE` | Thanh toán online |
| `COD` | Thu hộ khi giao (COD) |

### `programType` — 2 giá trị

**Hiện ở:** Nhật ký · Chi tiết · Báo cáo · bộ lọc

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `GOLDEN_HOUR` | Giờ vàng |
| `TWENTY_FOUR_SEVEN` | 24/7 |

### `resultReason` — 11 giá trị

**Hiện ở:** Chi tiết cuộc gọi

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `CUSTOMER_PRESSED_1` | Khách bấm phím 1 (đồng ý) |
| `CUSTOMER_PRESSED_0` | Khách bấm phím 0 (huỷ) |
| `ANSWERED_NO_INPUT` | Bắt máy nhưng không bấm phím |
| `UNSUPPORTED_DTMF_KEY` | Bấm phím không hợp lệ |
| `WRONG_INPUT_MAX_ATTEMPTS` | Bấm sai đến hết lượt gọi |
| `RING_TIMEOUT` | Đổ chuông hết giờ |
| `BUSY` | Máy bận |
| `REJECTED_REVIEW_REQUIRED` | Khách từ chối — cần người xem lại |
| `UNREACHABLE` | Không liên lạc được |
| `INVALID_DESTINATION` | Số không tồn tại |
| `CAPACITY_UNAVAILABLE` | Không còn kênh gọi |

### `reviewReason` — 17 giá trị

**Hiện ở:** Chờ duyệt · Chi tiết · Trạng thái tích hợp

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `CALLBACK_TIMEOUT` | Gửi callback quá hạn |
| `CALLBACK_CIRCUIT_OPEN` | Ngắt mạch bảo vệ đang mở |
| `CALLBACK_TRANSPORT_FAILURE` | Lỗi truyền tải |
| `CALLBACK_TRANSPORT_UNEXPECTED_FAILURE` | Lỗi truyền tải ngoài dự kiến |
| `CALLBACK_AUTH_REJECTED` | Core từ chối xác thực |
| `CALLBACK_PAYLOAD_INVALID` | Nội dung gửi không hợp lệ |
| `CALLBACK_PATH_BODY_MISMATCH` | Đường dẫn và nội dung không khớp |
| `CALLBACK_ACK_INVALID` | Core phản hồi ACK không hợp lệ |
| `CALLBACK_UNPROCESSABLE` | Core không xử lý được |
| `CALLBACK_UNSUPPORTED_RESPONSE` | Core trả phản hồi không hỗ trợ |
| `CALLBACK_RETRYABLE_RESPONSE` | Core báo có thể gửi lại |
| `CALLBACK_ADAPTER_SELECTION_REJECTED` | Không chọn được adapter |
| `CAPACITY_DEADLINE_UNAVAILABLE` | Không xác định được deadline năng lực |
| `CAPACITY_SOURCE_UNAVAILABLE` | Không đọc được nguồn năng lực |
| `IVR_CAPACITY_EXCEPTION` | Không đủ năng lực gọi |
| `LEASE_EXPIRED_RECONCILIATION_REQUIRED` | Lượt điều phối hết hạn — cần đối soát |
| `NO_DISPATCH_BEFORE_DEADLINE` | Không kịp quay số trước hạn chót |

### `reviewSourceType` — 4 giá trị

**Hiện ở:** Chờ duyệt · Trạng thái tích hợp

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `IVR_CALL_RESULT` | Kết quả cuộc gọi |
| `IVR_RESULT_CALLBACK` | Callback gửi Core |
| `ELIGIBILITY_DECISION` | Quyết định eligibility |
| `IVR_OPTOUT_PROPOSAL` | Đề xuất chặn gọi (opt-out) |

### `reviewStatus` — 2 giá trị

**Hiện ở:** Chờ duyệt · Chi tiết

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `OPEN` | Đang mở |
| `RESOLVED` | Đã xử lý |

### `scriptStatus` — 4 giá trị

**Hiện ở:** Cấu hình kịch bản

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `DRAFT` | Bản nháp |
| `IN_REVIEW` | Đang duyệt |
| `APPROVED` | Đã duyệt |
| `RETIRED` | Đã ngừng |

### `sellableDecision` — 4 giá trị

**Hiện ở:** Chi tiết → Khả năng bán

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `SELLABLE` | Bán được |
| `NOT_SELLABLE` | Không bán được |
| `BLOCKED` | Bị chặn |
| `UNKNOWN` | Chưa xác định |

### `shortageReason` — 4 giá trị

**Hiện ở:** Tổng quan → Sự cố năng lực

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `NO_DISPATCH_BEFORE_DEADLINE` | Không kịp quay số trước hạn chót |
| `CAPACITY_DEADLINE_UNAVAILABLE` | Không xác định được deadline năng lực |
| `CAPACITY_SOURCE_UNAVAILABLE` | Không đọc được nguồn năng lực |
| `LEASE_EXPIRED_RECONCILIATION_REQUIRED` | Lượt điều phối hết hạn — cần đối soát |

### `simStatus` — 8 giá trị

**Hiện ở:** Tổng quan → bảng kênh SIM

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `IDLE` | Rảnh |
| `RESERVED` | Đã giữ chỗ |
| `LEASED` | Đã nhận lượt |
| `DIALING` | Đang quay số |
| `ACTIVE_CALL` | Đang gọi |
| `DISABLED` | Đã tắt |
| `QUARANTINED` | Đang cách ly |
| `HEALTH_FAILED` | Health check lỗi |

### `technicalExceptionType` — 6 giá trị

**Hiện ở:** Chi tiết → Lượt gọi, Lỗi kỹ thuật

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `PROVIDER_DROPPED` | Nhà mạng ngắt cuộc gọi |
| `NETWORK_ERROR` | Lỗi mạng |
| `SIM_ERROR` | Lỗi SIM |
| `AUDIO_ERROR` | Lỗi âm thanh |
| `DTMF_ERROR` | Lỗi nhận phím bấm |
| `UNMAPPED_PROVIDER_DISPOSITION` | Nhà mạng trả mã lạ, chưa ánh xạ được |

### `voiceRegion` — 3 giá trị

**Hiện ở:** Chi tiết cuộc gọi

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `North` | Miền Bắc |
| `Central` | Miền Trung |
| `South` | Miền Nam |

### `warehouseStatus` — 4 giá trị

**Hiện ở:** Báo cáo → độ tươi dữ liệu

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `COMPLETE` | Pipeline đã nạp đủ |
| `BACKLOG` | Pipeline còn tồn — số liệu chưa đủ |
| `MISMATCH` | Pipeline lệch số dòng so với nguồn — cần rà soát |
| `NOT_RUN` | Pipeline chưa chạy lần nào |

