# Phiếu chốt P2/P3 — 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

**Đề nghị 9 ACCEPTED phạm vi local/MOCK và 4 CANCELLED do bỏ deliverable UI. Tất cả đang PENDING.**
Phiếu này nối tiếp W-0324, bổ sung W-0019 đã đủ hồ sơ và kiểm lại tại aaba3d2 sạch:
1171/1171 test, 42/42 gate; [verification](verification.json) có 142 lượt TestId của chín P2
và ba pin quyết định. Đây là bằng chứng cùng candidate, không phải test HEAD tài liệu/WIP.

## 1. Chín việc P2 đề nghị ACCEPTED

| Việc | Phạm vi được đề nghị | Residual đã xử lý / giới hạn giữ | Việc ngoài phạm vi và người phụ trách |
| --- | --- | --- | --- |
| [W-0018 / P2-1](../W-0018/README.md) | Intake, validation, idempotency và persist local/MOCK | Eligibility và capacity đã có ở W-0019/W-0020; P2-2 không còn là phần chưa triển khai. Ghi chú push-block là lịch sử, không dùng để kết luận remote hôm nay | Dev M3 cấp producer revision/schema/payload; Toàn cấp đầu mối credential/script/key/hạ tầng; dev IVR chạy shared E2E khi đủ đầu vào. Sales/CDC/SIM và LAB/PROD chưa được local proof chứng minh |
| [W-0019 / P2-2](../W-0019/acceptance-addendum.md) | Eligibility kỹ thuật/an toàn, capacity và persist theo OD-17/OD-18 hiện hành | Hai ID retired có pin và 11 test thay thế; tổng24 TestId hiện hành được xét. M3 quyết định gọi; IVR không tự trust-skip hay đọc Ops/CRM trực tiếp. Capacity placeholder đã được scheduler thay | Dev M3 cấp eligibility/contact có nguồn và revision; Toàn bố trí credential/đích; dev IVR kiểm fail-closed bằng payload thật và shared E2E. Phần SIM/LAB/PROD riêng |
| [W-0020 / P2-3](../W-0020/README.md) | Scheduler, attempt policy, lease/fencing/capacity local | P2-4 đã có gateway; flag off/unavailable cho cấu hình thiếu là chặn có chủ đích. Policy phía M8 có bản ký và giờ gọi mới. Mô phỏng 1/32 channel không phải phép đo SIM thật | Toàn cấp thiết bị/lịch đo/đầu mối nhà mạng; dev IVR đo W-0008/W-0048, lưu tải/DTMF/disposition; dev M3 xác nhận policy/version/cutover producer. Không bật cờ để khép việc local |
| [W-0021 / P2-4](../W-0021/README.md) | Mock dispatch adapter, dial-token/fencing, rendering và no-egress | Normalizer và TTS port đã có ở W-0022/W-0066. Mock không chứng minh modem/carrier/live TTS; lời thoại có tên khách trong fixture cũ không phải script hiện hành | Toàn/nhà cung cấp bố trí hạ tầng; dev IVR kiểm codec, DTMF, allowlist, adapter thật tại W-0008/W-0048; dev M3 cung cấp token/data thật |
| [W-0022 / P2-5](../W-0022/README.md) | DTMF/disposition mapping, counted/final/technical semantics và persist local | Callback đã có ở W-0023; policy M8 ký không thay bằng chứng raw-code nhà mạng. Normalization disabled mặc định là activation boundary; IVR chỉ gửi tín hiệu, không đổi đơn | Toàn chỉ định đầu mối nhà mạng; dev IVR thu DTMF/disposition/retry thật, đối chiếu DT-02 và sai khác trước khi mở đích lab tương ứng |
| [W-0023 / P2-6](../W-0023/acceptance-test-matrix.md) | Outbox, retry, ACK, fake Target và Golden Hour compatibility; 22 TestId | C2 đọc đủ ma trận đính kèm. Delivery disabled/real Target bị chặn có chủ đích. JWT profile đã chọn nhưng auth thật chưa được chứng minh; Target vẫn DRAFT, không notification/order mutation | Dev M3 cấp consumer revision/OpenAPI/ACK/idempotency và sandbox; Toàn cấp secret/JWKS/network và legacy identity map; dev IVR nối auth thật, chạy shared E2E ở work tích hợp. W-0005/W-0006 vẫn riêng |
| [W-0024 / P2-7](../W-0024/README.md) | Script lifecycle, approval guard, whitelist/rendering/privacy trong MOCK | OD-V1-15 đã chốt whitelist; W-0003 còn cần dữ liệu M3. Lời chào chung đã thay fixture lịch sử. Creator/Content/PrivacyLegal phải khác actor theo luật đang chạy; không tạo approver giả | Dev M3 cấp speech-safe payload; Toàn bố trí người duyệt script production đủ quyền hoặc quyết định đổi luật trong việc riêng; dev IVR kiểm đúng luật. LAB/production script, audio/media path và dữ liệu thật không được ký bằng fixture MOCK |
| [W-0065 / P2-8](../W-0065/README.md) | API internal/admin, authz/tier/idempotency/audit/PII local | Xoá UI không xoá API. Permission-name/actor của console cũ là lịch sử; identity/role thuộc M3, IVR enforce service tier. Coverage88.80% là số lịch sử, không phải đo hôm nay | Dev M3 chốt BFF/role→tier/actor và consumer tests; Toàn cấp secret/hạ tầng; dev IVR kiểm shared authz/E2E và hosted đúng SHA khi release. Không Sales write/SMS/call thật |
| [W-0066 / P2-9](../W-0066/test-report.md) | TTS port, fake provider, cache/timeout/privacy; đủ9 TestId | Whitelist và VieNeu tự host đã chọn; không còn yêu cầu chọn vendor. Fake không chứng minh phát âm/media path/model hiện hành. Bản sửa TTS/S5 WIP riêng không nằm trong candidate này | Dev IVR kiểm model/image/media path/queue/tải trên đúng candidate tương ứng; Toàn xử lý S2/S5, máy đích, quyền model/CVE và bằng chứng vận hành còn thiếu. Không yêu cầu nghe lại chỉ vì ký port MOCK; không phê duyệt WIP qua phiếu này |

Mọi phần còn lại đều có phạm vi riêng; local proof không ký thay M3 hoặc người vận hành. Bằng
chứng nghe/model đã có ở các work khác không bị xoá hoặc phủ nhận bởi giới hạn W-0066.
Full Residual và đối chiếu lịch sử: [W-0322 §4](../W-0322/README.md#4-đối-chiếu-từng-việc).

## 2. Bốn việc P3 đề nghị CANCELLED

Căn cứ: owner đã chuyển console về M3; [W-0253](../W-0253/README.md) tại c0e6609 xoá source/test/CI UI.
Các pin của từng closeout đều khớp. **CANCELLED nghĩa là kết thúc deliverable UI trong repo IVR**,
không xoá lịch sử công việc, không nghiệm thu UI M3 và không nhận các thiếu sót cũ là đã triển khai.

| Việc / closeout | Phần kết thúc | Phần chuyển tiếp và người phụ trách |
| --- | --- | --- |
| [W-0025 / P3-1](../W-0025/scope-closeout.md) | Foundation/session/RBAC/i18n UI cũ | Toàn xác nhận đầu mối M3; M3 chọn component library, identity/role→tier và cung cấp UI/E2E/visual-a11y nếu triển khai. Capture cũ không thay visual proof; runtime permission cũ đã đổi |
| [W-0026 / P3-2](../W-0026/scope-closeout.md) | Dashboard/log/detail UI cũ | M3 chốt auto-refresh bắt buộc cũ, CSV tuỳ chọn, masked data/KPI từ API và UI tests; Toàn cấp đầu vào giá cho work cost riêng; dev IVR xử lý cost backend khi có đầu vào. Không làm lại UI chỉ để đóng work |
| [W-0027 / P3-3](../W-0027/scope-closeout.md) | Config/integration/seed/roles UI cũ | M3 chốt UI/permissions theo API hiện hành; role assignment thuộc M3, KEY_9 vẫn tắt. Script/runtime-gate/seed từng được bổ sung trước khi UI bị bỏ; REVIEW không chứng minh replay; badge không thay dependency test |
| [W-0028 / P3-4](../W-0028/scope-closeout.md) | Reporting UI cũ | M3 kiểm source/freshness/export/RBAC-denied/visual-a11y; Toàn/dev IVR giữ cost/ETL deployment ở work riêng. BI backend còn sống; test BI không thay test UI. KPI từ API, aggregate drill-down và audit export vẫn là ranh giới privacy |

Hồ sơ này sẵn để bàn giao khi owner có người nhận; **chưa gửi ra ngoài, chưa có M3 xác nhận nhận
hoặc hoàn tất**. Enum labels/spec/backend vẫn được giữ theo W-0253, không dựng lại admin-ui.

## 3. Quyết định cần owner ghi

- **ACCEPTED local/MOCK:** W-0018, W-0019, W-0020, W-0021, W-0022, W-0023, W-0024, W-0065, W-0066.
- **CANCELLED theo scope UI đã bỏ:** W-0025, W-0026, W-0027, W-0028.
- Người duyệt, phản hồi và thời điểm: **PENDING**.

[Tracker §1 luật6/T7](../../../prompt/_execution/prompt-execution-tracker.md#1-operating-rules) yêu cầu
owner chấp nhận phạm vi/bằng chứng; lượt “tiếp tục” chỉ được ghi là yêu cầu hoàn thiện hồ sơ.
Toàn có thể duyệt cả hai nhóm hoặc chỉ rõ ID cần giữ lại. Trước khi có quyết định, 13 trạng thái
gốc và tổng **57 ACCEPTED** giữ nguyên; không mở quyền gọi khách thật hay production.
