# Target Contract V1 Draft — IVR ↔ Sales Platform

Trạng thái: `TARGET_CONTRACT_V1=DRAFT`
Cập nhật: `2026-08-12`
Mục đích: làm nguồn điều khiển cho plan/spec/prompt mới trong khi chờ dev/owner Sales khóa endpoint, auth, attempt policy và payload lời thoại.

> Đây là **đặc tả mục tiêu để hai bên build song song**, không phải bằng chứng rằng Sales Platform hiện đã implement. Nếu tài liệu cũ mâu thuẫn với file này, file này có hiệu lực cao hơn đối với Target V1; lịch sử cũ vẫn được giữ để truy vết.

## 1. Baseline đã kiểm tra

| Thành phần | Baseline ứng viên | Trạng thái |
| --- | --- | --- |
| Sales Platform | `PhucApu@a3aad246d986fbc273cf41aaa93eec6659669656` | `CANDIDATE_BASELINE` |
| IVR | `main@ab7de4d59eb04eb9f172385a1ffa4d25023064e5` | `CANDIDATE_BASELINE` |
| Sales OpenAPI | SHA-256 `F67B9D...5A4A` | đã khớp file đã rà soát; chưa có Target V1 đầy đủ |
| IVR OpenAPI trước realignment | SHA-256 `B1DA...C3F6` | chỉ dùng để nhận diện baseline cũ |
| Target task OpenAPI ứng viên | SHA-256 `59a201df6807252c23b6b9c76394c5d99e8790b9642d7bf4c1879870799fb759` | `DRAFT`, cần Sales review/CDC |
| Target callback OpenAPI ứng viên | SHA-256 `fc31d7c151437d490431a54815ea55dd69a3ff96ae668e6489055638bd9b3da9` | `DRAFT`, chưa có trong Sales source hiện tại |

Baseline chỉ giúp tái lập lần rà soát. Mọi thay đổi source/OpenAPI sau các SHA trên phải được review lại trước integration.

## 2. Ranh giới kiến trúc

- IVR là service **.NET 10 + PostgreSQL + Next.js**, repository và deployment riêng.
- Sales Platform/Order Core là **Java Spring Boot + Next.js**, sở hữu order truth, eligibility, revalidation và order transition.
- Hai hệ thống tích hợp bằng versioned HTTP/OpenAPI contract; không dùng chung database, entity hay source code.
- IVR sở hữu intake, task/job/attempt state, scheduler, dialer, SIM adapter, DTMF, result normalization, callback delivery, audit và admin UI.
- IVR chỉ gửi tín hiệu. IVR **không** trực tiếp xác nhận/hủy/expire đơn và **không** gửi SMS/notification trong V1.

## 3. Chế độ vận hành bắt buộc

| Mode | Adapter/data | Được gọi ai | Mục đích |
| --- | --- | --- | --- |
| `MOCK` | fake Sales provider + mock telephony | không gọi số thật | dev/unit/integration/E2E |
| `LAB_REAL_SIM` | mock hoặc sandbox Sales + **1 SIM thật** | chỉ số trong allowlist đã duyệt | kiểm chứng dial/DTMF/disposition; tuyệt đối không gọi khách |
| `PRODUCTION_REAL` | Sales API thật + gateway/eSIM thật | khách đủ eligibility | chỉ sau contract, legal, security và release gate |

`REAL_CUSTOMER_CALL_ALLOWED=NO` là mặc định ở mọi môi trường. `LAB_REAL_SIM` không đồng nghĩa với cho phép gọi khách thật.

## 4. Ma trận chương trình và thanh toán

| Program | Payment | Điều kiện tạo IVR task | Ý nghĩa phím |
| --- | --- | --- | --- |
| `GOLDEN_HOUR` | `ONLINE` | Official Order, trạng thái callable do Core xác định, `ivr_confirmation_required=true` | xác nhận/hủy **ý định đặt hàng**; không xác nhận thanh toán |

> **Delta chưa được owner duyệt (`OD-V1-13`, `OD-V1-14`).** Business source đọc được hiện vẫn COD-only (`decisions-log.md` DS-01, source-read từ Sales platform), và `ivr_confirmation_required` **không xuất hiện lần nào** trong `docs/documents/`. Dòng `GOLDEN_HOUR+ONLINE` ở trên là **đề xuất Target V1**, được build song song sau mock, **không** được coi là đã phê duyệt. Test `non-COD reject` cũ đã được sửa để không loại Golden Hour ONLINE (`P5-1` IT-05a/b/c).

| `TWENTY_FOUR_SEVEN` | `COD` | Official Order, trạng thái callable do Core xác định, `ivr_confirmation_required=true` | xác nhận/hủy đơn COD |

- Canonical code là `TWENTY_FOUR_SEVEN`. Giá trị legacy `24_7` chỉ được nhận trong `CURRENT_COMPAT` adapter rồi normalize; không phát tán tiếp.
- Không còn bất biến “toàn hệ thống COD-only”. Payment/program khác bị từ chối fail-closed trừ khi có contract version mới.

## 5. Target task contract tối thiểu

Sales Platform cần push `POST /v1/ivr/order-confirmation/tasks` với:

- `contract_version`, `task_id`, `order_id`, `order_code`, `order_version`, `order_state` (enum đục do Core sở hữu — D-02);
- `program_code`, `payment_method_snapshot`, `ivr_confirmation_required`;
- `confirmation_window_started_at`, `confirmation_window_expires_at`;
- `attempt_policy_version`, `max_customer_attempts`, `attempt_offsets_seconds`;
- `phone_ref`, `phone_masked`, `dial_token`, `dial_token_expires_at` (**`OD-V1-17`:** một token scalar không đủ cho 2 customer attempt + technical retry nếu ngữ nghĩa là one-use/attempt; Sales/Security phải chọn per-attempt token, reissue endpoint, token bundle hoặc reusable token có TTL/risk control. **`OD-V1-18`:** vị trí resolve `dial_token→E.164` chưa được định nghĩa — `specs/api/04` nói adapter không nhận số, `P2-4` đặt resolver trong IVR);
- `privacy_safe_order_summary` theo §6;
- `call_restriction`, `eligibility_snapshot`, `evidence_ref`;
- headers `Idempotency-Key`, `X-Correlation-Id`, service authentication.

Fake Sales provider phải sinh được cùng DTO và mọi lỗi contract để IVR hoàn thiện trước khi API thật sẵn sàng.

## 6. Payload lời thoại — P0 cho business acceptance

`privacy_safe_order_summary` bắt buộc có:

- `customer_display_name`: tên gọi ngắn, không đọc thông tin nhạy cảm dư thừa;
- `order_code_short`;
- `items[]`: `public_name`, `quantity`, tùy chọn `unit_label`; có policy giới hạn số dòng và câu tổng kết phần còn lại;
- `total_amount`, `currency=VND`;
- `delivery_area_short`: chỉ phường/xã, quận/huyện, tỉnh/thành hoặc mô tả rút gọn đã được Core chuẩn hóa; **không gửi/đọc địa chỉ đầy đủ**;
- `program_display_name`;

**Ràng buộc kiểm được bằng máy cho `delivery_area_short` (Sales phải tuân):**

- Giá trị **không được bắt đầu bằng chữ số** và **không được chứa dạng `x/y`** — đây là hai dấu hiệu số nhà. Schema enforce bằng `pattern`.
- Đơn vị hành chính có số vẫn hợp lệ: `"Quận 7, Thành phố Hồ Chí Minh"`, `"Phường 12, Quận Tân Bình"` — **không** cần viết chữ.
- Chuỗi không có chữ số nhưng vẫn là địa chỉ đường phố (ví dụ `"Đường Nguyễn Huệ, Phường Bến Nghé"`) **vượt qua schema** và phải bị chặn bởi semantic detector phía IVR (`FR-IVR-INTAKE-005`) với `IVR_PII_POLICY_VIOLATION`. Fixture `NEG-DOMAIN-PII-01` kiểm đúng nhánh này.


> **`OD-V1-15`:** ba tài liệu enforce whitelist biến script (`specs/data/05`, `specs/ui/04`, `specs/api/04`) hiện chỉ cho 4 biến và **không** có `items[]` hay `delivery_area_short` — tức đúng hai thứ mà câu thoại Target V1 bắt buộc phải đọc. Business source `PACK-09 §9.1` hậu thuẫn bộ hẹp. Mở rộng whitelist là một **quyết định privacy** cần Product + Privacy/Legal duyệt; fixture MOCK dùng bộ rộng nhưng không đóng gate.

- `locale=vi-VN` và `pronunciation_hints` tùy chọn.

Mẫu ý nghĩa cần hỗ trợ: “Xin chào anh/chị {name}. Anh/chị có đơn {code} gồm {items}, tổng tiền {amount}, giao đến {area}. Bấm 1 để xác nhận, bấm 0 để hủy.” Script thực tế phải qua content/privacy approval. IVR không tự truy vấn hoặc ghép raw address từ database Sales.

## 7. Attempt policy

- Trạng thái: `CANDIDATE_POLICY_OWNER_DECISION_REQUIRED`.
- Candidate để mock/lab: tối đa 2 customer attempts; Golden Hour window 5 phút với A2 tại +150 giây; 24/7 window 15 phút với A2 tại +450 giây.
- **Delta với business source (`OD-V1-16`):** `docs/documents/4. phase/phase-8/10-…` §8 và `16-…` §NFR (cả hai **không** mang banner D-10) ghi Golden Hour 2 attempts/**10 phút** và 24/7 **3** attempts/15 phút. Sales dev đã nêu xung đột này là `OWNER_DECISION_REQUIRED`. IVR **không** sửa business source; delta được theo dõi ở open-decision register.
- Không hard-code candidate vào database constraint hoặc domain constant. Task phải mang `attempt_policy_version`; IVR dùng policy registry/config có validation bounds.
- Không được dùng candidate cho `PRODUCTION_REAL` trước owner sign-off. Technical retry không tính là customer attempt.

## 8. Result callback Target V1

Endpoint Sales-owned đề xuất:

`POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks`

Headers: `Authorization: Bearer <short-lived-service-jwt>`, `Idempotency-Key`, `X-Correlation-Id`, `Content-Type: application/json`.

Payload tối thiểu:

- `contract_version`, `callback_id`, `task_id`, `order_id`, `order_version_seen_by_ivr`;
- `result_type`, `is_counted_customer_attempt`, `is_final_for_ivr`;
- `attempt_number`, `occurred_at`, `recommended_core_action` (advisory only);
- `evidence_ref`, `audit_ref`.

ACK taxonomy:

| HTTP | Semantic code | IVR xử lý |
| --- | --- | --- |
| `200` | `ACCEPTED`, `DUPLICATE_ACCEPTED` | complete callback delivery |
| `200` | `BLOCKED_BY_CORE`, `REVIEW_REQUIRED` | complete delivery; hiển thị quyết định Core, không retry |
| `409` | `REJECTED_STALE`, `IDEMPOTENCY_CONFLICT` | không retry tự động; audit/admin review theo code |
| `422` | invalid schema/outcome | dead-letter/admin review |
| `429`, `5xx`, timeout | transport/retryable | retry bounded với cùng idempotency key |

Current compatibility adapter tạm thời gọi `POST /api/v1/internal/ivr/golden-hour/callbacks`. Nó phải được cô lập sau interface, không làm biến dạng Target V1 domain và không được coi là integration hoàn chỉnh.

## 9. No-answer và notification

- `NO_ANSWER_FINAL` không yêu cầu Sales hủy ngay. Callback là advisory với `recommended_core_action=CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`.
- Sales timeout worker có thể chuyển `EXPIRED` khi hết window, nhưng phải revalidate state/version/blocker trước transition.
- `TECHNICAL_EXCEPTION` tách biệt no-answer và không tính customer attempt.
- V1 notification/SMS: `DISABLED`. P4-5 chỉ tạo no-op/extension boundary và contract test “không gửi”; không build consumer gửi tin cho khách.

## 10. Auth

- Dev/local: mock JWT issuer và test keys; có negative tests issuer/audience/expiry/scope.
- Production target: short-lived service-account JWT. mTLS là lựa chọn cần Security/Platform owner xác nhận.
- `X-Internal-Token` chỉ được giữ trong `CURRENT_COMPAT`, không phải Target V1.

## 11. SIM/eSIM

- Hiện tại: thiết kế port và hoàn thiện mock; sau đó kiểm thử `LAB_REAL_SIM` bằng **1 SIM thật** và allowlist số test.
- Tương lai: target **32 eSIM channels**. Scheduler/channel pool phải cấu hình động, không giả định 1/12/32 trong code.
- Protocol/SDK, DTMF mode, concurrency thực, health/disposition mapping, caller ID và secret provisioning vẫn cần vendor/infra cung cấp.

## 12. Điều kiện “implementation-complete” và “operational-ready”

`IMPLEMENTATION_COMPLETE_BEHIND_MOCKS` khi toàn bộ code, migration, UI, tests, observability, deployment manifests và adapters đã xong; fake Sales provider và mock SIM pass; real providers chỉ còn cấu hình/contract mapping.

Không được nói “chỉ cắm API và eSIM là vận hành” nếu chưa chứng minh:

1. Sales implement task producer + speech summary + dial-token + target callback/revalidation;
2. auth trust được khóa và test giữa hai service;
3. attempt policy owner sign-off;
4. real SIM lab pass bằng số allowlist;
5. 32 eSIM gateway/capacity được nghiệm thu;
6. legal/privacy/security/release sign-off và production evidence pass;
7. TTS/audio provider được chọn và nghiệm thu phát âm ở lab (`OD-V1-19`; prompt `P2-9`/W-0066 mới chỉ tạo port + fake);
8. IVR internal/admin API được implement (`P2-8`/W-0065 — 13 operation trước đây không có prompt nào build).

## 13. Các câu trả lời cần đòi từ Sales/owner

1. Xác nhận ma trận Golden Hour ONLINE và 24/7 COD, cùng điều kiện `ivr_confirmation_required`.
2. Xác nhận hoặc sửa endpoint callback Target V1 và ACK taxonomy.
3. Chốt auth production (JWT issuer/audience/scopes/TTL; mTLS có bắt buộc không).
4. Chốt D-10 attempt policy; trước đó candidate chỉ dùng MOCK/LAB.
5. Cung cấp schema + ví dụ `privacy_safe_order_summary` và dial-token issue/resolve.
6. Cung cấp timeout/revalidation behavior và idempotency retention.
7. Cung cấp OpenAPI thật + môi trường sandbox + test credentials khi integration bắt đầu.
