# 10 — Integration Gap Analysis

Phân tích khoảng trống tích hợp hiện tại. Với mỗi gap: mô tả, tác động IVR, priority, mock được không, ai xử lý, specs cần sinh, câu hỏi cần xác nhận.

> ✅ **Cập nhật 2026-07-02 (Module 3/3.1 trả lời — xem [decisions-log.md](../decisions-log.md)):**
> **ĐÃ ĐÓNG (còn lại là hiện thực API, không phải gap thiết kế):** GAP-S1 (contract → D-03/D-04), GAP-S2 (order state → D-02), GAP-S3 (tension order_code → **D-01**), GAP-S4 (IVRRequiredDecision → D-09), GAP-S5 (quota release → D-11), GAP-S6 (trust → D-12), GAP-S7 (dial token → D-05); GAP-O1 **phần Core** → D-06; GAP-O2 (availability → D-07 qua commerce).
> **CÒN LẠI:** GAP-T1/T2 (SIM), GAP-A1/A2 (auth), GAP-E1/E2 (evidence/retention), GAP-N1 (notification).
> **✅ Cập nhật 2026-07-02 (Ops-Core trả lời DO-01..DO-09):** GAP-O1 phần Ops → **DO-01** (sellable gate `availability/check`), GAP-O3 (event push) → **DO-04** (webhook `sku-became-not-sellable`). **GAP MỚI phát sinh:** do-not-call/opt-out **không thuộc ops** → cần **CRM/business-platform** cấp (GAP-N2/Q-C1).

## A. Sales Platform / Order Core

| ID | Mô tả gap | Tác động IVR | Priority | Mock? | Ai xử lý | Specs sinh sau | Câu hỏi |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GAP-S1 | **Contract task/callback chưa hiện thực** (`IvrConfirmationTaskV1`, `IvrConfirmationResultCallbackV1` mới ở mức SRS, chưa có API thật/OpenAPI) | Không có luồng task/callback → IVR không chạy thật | P0 | Có (mock task + mock callback endpoint) | Order Core (sales) + IVR | api/05, integration-requirements/01 | Transport (REST/command)? push hay poll? |
| GAP-S2 | **Order state machine chưa chốt** (tên state, IVR-callable set, transition sau mỗi result type) | Callback sai thời điểm; revalidate không rõ | P0 | Một phần | Order Core owner | functional/05, workflows/09 | Danh sách state & transition? |
| GAP-S3 | **Tension thứ tự IVR ↔ order_code** (phase-3.1/07: IVR trước order_code; phase-8: IVR sau Official Order) | Sai mô hình → thiết kế lại | P0 | Không | Order Core + Sales owner | context/scope, integration-requirements/01 | IVR trước hay sau order_code? Hai cơ chế hay một? |
| GAP-S4 | **`IVRRequiredDecision` chưa có endpoint** (contract có ở phase-3.1/07, không có API) | Không biết khi nào tạo task | P1 | Có | Sales (3.1) | api/08, integration-requirements/01 | GET decision hay event push? |
| GAP-S5 | **Golden Hour quota release khi IVR fail/timeout** chưa có API/event | Quota không được giải phóng | P1 | Có | Sales (3.1) | integration-requirements/01 | API hay event? idempotency? |
| GAP-S6 | **Trust decision & risk flags** format/resolver chưa chốt | Trusted skip không chính xác | P1 | Có | Customer/Trust owner | data/02 | Resolver nào, ngưỡng trust? |
| GAP-S7 | **Official contact projection** (phone_ref/masked/dial token TTL) chưa chốt | Không dial/privacy risk | P0 | Có (token giả) | Customer/Commerce owner | data/05, api/05 | Cấp token thế nào, TTL? |
| GAP-S8 | (Inbound) **lookup customer/order by phone, call note** không có API | Chỉ ảnh hưởng nếu mở inbound | P2 | Có | Sales | (khi mở scope) | Có mở inbound không? |

## B. Ops-Core

| ID | Mô tả gap | Tác động IVR | Priority | Mock? | Ai xử lý | Specs sinh sau | Câu hỏi |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GAP-O1 | **Sale-lock/recall/suppression: snapshot vs realtime** chưa rõ chiều lấy | Xác nhận nhầm đơn bị khóa (nếu snapshot cũ) | P0 | Có | Ops + Order Core | data/03, integration-requirements/02 | Snapshot qua task hay gọi ops realtime lúc revalidate? |
| GAP-O2 | **Availability nguồn nào** (ops trực tiếp vs commerce tổng hợp) | Xác nhận đơn không đủ hàng | P1 | Có | Ops/Commerce owner | data/03 | Ai cấp availability cho revalidate? |
| GAP-O3 | **Event push khi lock/recall activate giữa cuộc gọi** chưa có | Không hold kịp trong cuộc gọi dài | P1 | Khó | Ops | architecture/05 | Có event subscription không? |
| GAP-O4 | (Inbound) **public trace lookup** whitelist chưa cấp cho IVR | Chỉ nếu mở inbound "hỏi batch an toàn" | P2 | Có | Ops | (khi mở scope) | IVR được gọi public trace? |
| GAP-O5 | **Tất cả API ops mới ở mức conceptual** (report ops: 12 projection chưa hiện thực) | Không có nguồn realtime | P0/P1 | Có | Ops | integration-requirements/02 | Timeline hiện thực? |

## C. Telephony / Internal SIM Gateway

| ID | Mô tả gap | Tác động IVR | Priority | Mock? | Ai xử lý | Specs sinh sau | Câu hỏi |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GAP-T1 | **Production SIM gateway protocol chưa chốt** (`Owner Decision Required`) | Không dial thật được | P0 | Có (dry-run) | IVR Infra owner | api/04, architecture/04, integration-requirements/03 | Hardware/API protocol? |
| GAP-T2 | **Mapping tín hiệu SIM thật** (busy/rejected/unreachable/dropped → no-answer vs technical) chưa chốt | Nhầm technical↔no-answer (FAIL P0) | P0 | Một phần | IVR Infra + owner | functional/06, integration-requirements/03 | Bảng mapping disposition? |
| GAP-T3 | **Recording policy** (bật/tắt, consent, retention) chưa quyết | Privacy/pháp lý | P1 | N/A | Owner + Legal | database/05, integration-requirements/03 | Có bật recording? |
| GAP-T4 | **Webhook provider ngoài** không có trong mô hình mặc định | Chỉ nếu đổi sang cloud provider | P2 | N/A | Owner | (future) | Có dùng provider ngoài? |

## D. Shared Auth / Permission

| ID | Mô tả gap | Tác động IVR | Priority | Mock? | Ai xử lý | Specs sinh sau | Câu hỏi |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GAP-A1 | **Service identity allowlist** (ai được tạo task) chưa cấu hình | Task giả mạo | P0 | Có | Foundation | api/01, architecture/03 | Danh sách service token? |
| GAP-A2 | **RBAC permission IVR_*** chưa map vào Permission Core | Admin action không enforce | P0 | Có | Foundation + IVR | ui/08, api/03 | Tạo permission ở đâu? |

## E. Notification

| ID | Mô tả gap | Tác động IVR | Priority | Mock? | Ai xử lý | Specs sinh sau | Câu hỏi |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GAP-N1 | **Notification chỉ sau Core decision** — handoff chưa định nghĩa | IVR có thể bị ép tự gửi (FAIL) | P1 | Có | Notification owner + Core | architecture/03 | Template & trigger sau Core hủy/expire? |

## F. Audit / Logging / Evidence

| ID | Mô tả gap | Tác động IVR | Priority | Mock? | Ai xử lý | Specs sinh sau | Câu hỏi |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GAP-E1 | **Evidence Registry integration** cho IVR chưa cấu hình | Không mở được release gate | P0 | Có | Foundation/Evidence owner | testing/*, database/* | Evidence packet format cho IVR? |
| GAP-E2 | **Retention duration** từng loại (call log/DTMF/recording/audit) chưa quyết | Compliance risk | P1 | N/A | Owner | database/05 | Retention mỗi loại? |

## G. Tổng hợp gap P0 lớn nhất

- ✅ ĐÃ ĐÓNG (Module 3/3.1, 2026-07-02): GAP-S3 (order_code → D-01), GAP-S1/S2 (contract & order state → D-02/D-03/D-04) → chuyển thành **việc hiện thực API** phía Order Core/Sales.
- ✅ ĐÃ ĐÓNG thêm (Ops-Core, 2026-07-02): GAP-O1 phần Ops → DO-01 (sellable gate); GAP-O3 → DO-04 (webhook); GAP-O4/O5 (public trace/product) → DO-08/DO-09.
- ⏳ CÒN LẠI (chặn gọi thật / bảo mật):
  1. **GAP MỚI (P0): do-not-call/opt-out** — thuộc **CRM/business-platform** (không phải ops), cần Module 3.1 cấp nguồn (Q-C1).
  2. GAP-T1/T2 — SIM protocol & disposition mapping (Infra/Telephony).
  3. GAP-A1/A2, GAP-E1 — auth allowlist/RBAC/evidence (Foundation) & release gate.
