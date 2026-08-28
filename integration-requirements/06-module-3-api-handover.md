# IR-06 — Bàn giao API Module 3 ↔ IVR

**Gửi:** Team **Module 3 — `ginsengfood-business-platform`** (Commerce / Order Core / Sales Extensions / CRM — Customer Identity)

**Từ:** Team Module 8 — IVR Order Confirmation (.NET, service tách biệt)

**Cập nhật:** 2026-08-28
**Trạng thái:** `TARGET_V1_DRAFT` — chờ Module 3 review/sign-off; IVR repo đã alignment theo `W-0123`, external integration/production gates vẫn mở. **Thêm §4A ngày 28/08/2026:** hợp đồng bề mặt quản trị sau khi IVR xoá toàn bộ hệ thống tài khoản/phân quyền (`W-0128`) — phần này M3 chưa từng nhận, và client viết theo bản trước 28/08 sẽ hỏng. **OpenAPI đã lên `1.0.0-draft.22`:** 11 endpoint auth/accounts đã bị gỡ khỏi spec, M3 cần sinh lại client (§4A.7).

> **Ranh giới đã được owner làm rõ ngày 2026-08-27:** **Module 3 quyết định nghiệp vụ; IVR thực thi cuộc gọi.**
>
> Module 3 chỉ gửi task sau khi đã quyết định đơn đó cần gọi. IVR không tự phân loại khách cũ/khách mới, không tự tính rủi ro bán hàng và không tự bỏ qua cuộc gọi vì “khách quen”. IVR vẫn được phép từ chối hoặc giữ task vì lỗi contract, auth, idempotency, dữ liệu quay số, privacy, policy thực thi, cửa sổ thời gian hoặc năng lực hệ thống.
>
> Ranh giới này đánh dấu placement của `OD-15` cũ là **`SUPERSEDED`**: quyết định bỏ qua khách cũ không còn nằm trong IVR. Mục tiêu business “không gọi khách cũ nếu Module 3 xác định không cần gọi” không đổi; nơi ra quyết định chuyển về Module 3. Khi tài liệu cũ mâu thuẫn với file này về trách nhiệm khách cũ/khách mới, file này là nguồn bàn giao ưu tiên cho Target V1.

Nguồn kỹ thuật liên quan — **đường dẫn tính từ gốc repository IVR** (`ginsengfood-ivr`), không phải từ thư mục chứa file này:

| Tài liệu | Đường dẫn trong repo IVR |
| --- | --- |
| IR-01 — Sales platform requirements | `integration-requirements/01-sales-platform-requirements.md` |
| API-05 — Order Core contracts | `specs/api/05-order-core-contracts.md` |
| Callback OpenAPI Target V1 | `specs/api/openapi/order-core-ivr-callback.target-v1.yaml` |
| Closure pack T-01…T-09 | `docs/contracts/target-v1-closure-pack/README.md` |
| Decisions log | `plan/ivr-orther/decisions-log.md` |
| **OpenAPI IVR — bản mới `1.0.0-draft.22`** | `specs/api/openapi/ivr-order-confirmation.v1.yaml` |
| **So sánh draft.20 → draft.22** | `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.20-to-v1.0.0-draft.22.md` |

_Sửa 27/08/2026: bản trước dùng đường dẫn tương đối, nên khi IR-06 được gửi đi dạng file rời thì cả năm link đều không mở được — M3 báo lại ở review §3.3. Cả năm file đều tồn tại trong repo IVR; nếu cần bản sao, yêu cầu owner IVR gửi kèm._

_Thêm 28/08/2026: **§4A** là mục mới và là phần duy nhất trong bản này M3 chưa từng đọc. Nó mô tả bề mặt quản trị sau khi IVR xoá sạch hệ thống tài khoản/vai trò của chính mình (`W-0128`), theo đúng ranh giới dev M3 đề xuất: IVR là module chức năng, M3 giữ tài khoản và quyền. §0, §7, §9 và §10 được cập nhật để trỏ tới nó; các mục §1–§4 về luồng đơn hàng **không đổi**._

---

## 0. Đọc trong 2 phút

Luồng runtime có **2 API chính**:

| Hướng | API | Ý nghĩa |
| --- | --- | --- |
| **Module 3 → IVR** | `POST {ivr}/v1/ivr/order-confirmation/tasks` | Module 3 đã quyết định cần gọi và giao task cho IVR thực thi |
| **IVR → Module 3** | `POST {sales}/api/v1/internal/orders/{orderId}/ivr-result-callbacks` | IVR trả tín hiệu kết quả; Module 3 revalidate và quyết định trạng thái đơn |

Ngoài hai API nghiệp vụ trên còn **một bề mặt thứ ba** và hai dependency cần chốt:

| Hướng | API | Ý nghĩa |
| --- | --- | --- |
| **Màn hình quản trị M3 → IVR** | `{ivr}/v1/ivr/order-confirmation/...` (31 endpoint) | Xem hàng đợi, kill switch, cắt cuộc gọi, duyệt lời thoại — **§4A**, hợp đồng mới ngày 28/08/2026 |

1. Cơ chế cấp/resolve/refresh `dial_token` để IVR lấy số E.164 lúc quay số.
2. Service auth production: issuer, JWKS, audience, scope, credential và quyết định mTLS.

Mô hình đúng:

```text
Module 3 lọc và quyết định CALL_REQUIRED
        │
        ├── Không cần gọi ──> Module 3 tự tiếp tục workflow; không gửi task sang IVR
        │
        └── Cần gọi ───────> POST /v1/ivr/order-confirmation/tasks
                                      │
                                      ├── IVR reject/hold vì lỗi kỹ thuật hoặc an toàn
                                      │
                                      └── TASK_ACCEPTED_CALL_JOB_CREATED
                                                   │
                                                   ├── IVR gọi khách
                                                   └── callback kết quả về Module 3
                                                                │
                                                                └── Module 3 revalidate và đổi state
```

**Câu chốt:** Module 3 gửi task là **lệnh thực thi đã qua quyết định nghiệp vụ**, không phải yêu cầu IVR đánh giá lại xem khách có cần gọi hay không.

`CALL_REQUIRED` trong tài liệu là tên logic/pseudocode phía Module 3, **không phải field mới trên wire**. Tín hiệu wire hiện có là việc Module 3 gọi endpoint với `ivr_confirmation_required=true` và evidence `decision=ELIGIBLE`.

---

## 1. Ranh giới trách nhiệm

| Quyết định / hành động | Owner |
| --- | --- |
| Khách cũ hay khách mới; risk policy; đơn nào cần gọi | **Module 3** |
| Official Order có ở trạng thái được phép xác nhận hay không | **Module 3** |
| Đặt `ivr_confirmation_required=true` và mở cửa sổ `CONFIRMING` | **Module 3** |
| Lọc bỏ đơn không cần gọi trước khi push | **Module 3** |
| Tạo payload call-ready, privacy-safe và evidence có version | **Module 3** |
| Xác thực caller, schema, idempotency và contract compatibility | **IVR** |
| Kiểm tra số/token dùng được, cửa sổ còn hạn, script/policy được duyệt, privacy và capacity | **IVR** |
| Thực hiện cuộc gọi, retry kỹ thuật và chuẩn hoá kết quả | **IVR** |
| Ghi/đổi trạng thái đơn sau callback | **Module 3** |
| Revalidate version, state, inventory, recall, sale-lock, quality hold | **Module 3** |

### 1.1. Module 3 phải lọc trước khi gửi

Module 3 **không gửi** task nếu một trong các điều kiện sau đúng:

- Business rule kết luận không cần gọi, gồm chính sách khách cũ/khách mới của Module 3.
- Đơn không phải Official Order hoặc chưa ở trạng thái callable.
- `ivr_confirmation_required=false`.
- Có do-not-call/call restriction hoặc blocker nghiệp vụ đang active.
- Eligibility/risk evaluation chưa hoàn tất hoặc kết luận không đủ điều kiện.
- Không tạo được snapshot gọi hoàn chỉnh, số/token hợp lệ hoặc lời thoại privacy-safe.

Với đơn bị lọc trước khi gửi, Module 3 **tự tiếp tục workflow của mình**. IVR không biết đơn đó tồn tại và sẽ không có callback.

### 1.2. “IVR thực thi” không có nghĩa “gọi mù”

IVR không đánh giá lại khách cũ/khách mới, nhưng vẫn fail-closed trước lỗi kỹ thuật hoặc an toàn. Ví dụ:

- JWT, source, idempotency hoặc correlation không hợp lệ;
- body sai schema hoặc tự mâu thuẫn;
- program/payment chưa được IVR hỗ trợ;
- cửa sổ đã hết hạn;
- số điện thoại, `dial_token` hoặc TTL không dùng được;
- payload lời thoại chứa PII/địa chỉ đầy đủ;
- script/policy chưa được duyệt cho môi trường chạy;
- không đủ capacity để gọi trước deadline.

Các gate này bảo vệ việc thực thi, **không chuyển quyền quyết định nghiệp vụ từ Module 3 sang IVR**.

---

## 2. Hướng gọi — hai chiều push, không polling

```text
                    (A) Module 3 gọi IVR
    ┌──────────┐    POST {ivr}/v1/ivr/order-confirmation/tasks    ┌──────────┐
    │ Module 3 │ ────────────────────────────────────────────────> │   IVR    │
    │Order Core│    “Đơn này đã được quyết định phải gọi”          │  (.NET)  │
    └──────────┘                                                   └────┬─────┘
         ▲                                                               │
         │                                                        gọi khách
         │                                                               │
         │            (B) IVR gọi Module 3                               │
         └──────────── POST {sales}/api/v1/internal/orders/ ─────────────┘
                       {orderId}/ivr-result-callbacks
                       “Đây là kết quả; Core tự revalidate và quyết định”
```

IVR:

- không polling `GET /orders`;
- không truy vấn lại customer/order để tự quyết định có gọi hay không;
- không trực tiếp ghi trạng thái đơn;
- không coi khách bấm `1` là bằng chứng đơn đã được xác nhận trong Core.

---

## 3. API A — Module 3 giao task cho IVR

### 3.1. Endpoint và headers

```http
POST {ivr_base_url}/v1/ivr/order-confirmation/tasks
Content-Type: application/json
Authorization: Bearer <service-jwt>
Idempotency-Key: <8-200 chars>
X-Correlation-Id: <1-200 chars>
X-Source-System: <module-3-source-id>
```

| Header | Bắt buộc | Ý nghĩa |
| --- | --- | --- |
| `Authorization` | Có | Service identity của Module 3 |
| `Idempotency-Key` | Có | Cùng key + cùng body trả kết quả cũ; cùng key + khác body là conflict |
| `X-Correlation-Id` | Có | Mã truy vết xuyên suốt task, call và callback |
| `X-Source-System` | Có | Định danh producer được phép gửi task |

Nếu body có `correlation_id`, giá trị phải trùng `X-Correlation-Id`.

### 3.2. Preconditions thuộc Module 3

Trước khi gọi API, Module 3 xác nhận:

1. Đây là Official Order.
2. Đơn đang ở state callable; Target V1 hiện dùng `CONFIRMING`.
3. Module 3 đã xử lý khách cũ/khách mới và risk policy.
4. Kết luận cuối là **cần gọi**; nếu không cần gọi thì không gửi.
5. `ivr_confirmation_required=true`.
6. `call_restriction=false` và không có blocker nghiệp vụ.
7. Snapshot version, cửa sổ, attempt policy, phone/token và lời thoại đã hoàn chỉnh.

### 3.3. Program/payment profile IVR hiện hỗ trợ

| `program_code` | `payment_method_snapshot` | Target V1 hiện tại |
| --- | --- | --- |
| `GOLDEN_HOUR` | `ONLINE` | Hỗ trợ theo contract draft |
| `TWENTY_FOUR_SEVEN` | `COD` | Hỗ trợ theo contract draft |
| Cặp khác | Bất kỳ | Reject vì execution profile chưa được hỗ trợ |

Đây là compatibility gate của contract thực thi, không phải IVR tự đánh giá customer/order. Module 3 vẫn cần ký ma trận cuối cùng vì tài liệu business cũ có chỗ nói `GOLDEN_HOUR + ONLINE` không callable.

### 3.4. 22 field bắt buộc trên wire

| Field | Kiểu | Ý nghĩa / ràng buộc |
| --- | --- | --- |
| `contract_version` | string | Hằng số `ivr-order-confirmation.v1` |
| `task_id` | string | ID task do Module 3 sinh |
| `order_id` | string | ID đơn; phải trùng URL callback sau này |
| `order_code` | string | Mã đơn đầy đủ; IVR không đọc cho khách |
| `order_version` | string | Snapshot chống race; IVR trả nguyên giá trị trong callback |
| `order_state` | string | Target hiện dùng `CONFIRMING`; Module 3 sở hữu state machine |
| `payment_method_snapshot` | string | `ONLINE` hoặc `COD`, khớp profile được hỗ trợ |
| `program_code` | string | `GOLDEN_HOUR` hoặc `TWENTY_FOUR_SEVEN` |
| `ivr_confirmation_required` | boolean | Bắt buộc `true`; đây là tín hiệu Module 3 đã quyết định cần gọi |
| `confirmation_window_started_at` | date-time | `T0` mở cửa sổ xác nhận |
| `confirmation_window_expires_at` | date-time | Deadline không được gọi sau mốc này |
| `attempt_policy_version` | string | Version policy IVR đã duyệt |
| `max_customer_attempts` | int 1–10 | Số lần làm phiền khách tối đa |
| `attempt_offsets_seconds` | int[] | Lịch gọi tính từ `T0`; phải có `0` cho lần gọi đầu |
| `phone_ref` | string | Tham chiếu số, không phải số E.164 |
| `phone_masked` | string | Số đã che để hiển thị/audit |
| `dial_token` | string | Token mờ dùng để resolve số thật khi quay |
| `dial_token_expires_at` | date-time | Phải ≥ `confirmation_window_expires_at` |
| `privacy_safe_order_summary` | object | Nội dung được phép đọc cho khách |
| `call_restriction` | boolean | Module 3 phải gửi `false`; `true` sẽ bị IVR chặn vì an toàn |
| `eligibility_snapshot` | object | Evidence cho quyết định call-ready của Module 3 |
| `evidence_ref` | string | Con trỏ evidence để đối soát |

### 3.5. Field optional và field bắt buộc trên thực tế

Contract dùng `additionalProperties: false`; không gửi field ngoài danh sách được công bố.

| Field | Target mới |
| --- | --- |
| `phone_validation_status` | **Bắt buộc trên thực tế**, phải là `VALID` |
| `correlation_id` | Optional; nếu có phải trùng header |
| `created_at` | Metadata audit |
| `order_code_short` | Mã rút gọn; có thể dùng cho lời thoại |
| `is_ivr_callable` | Optional; nếu gửi phải là `true` |
| `customer_ref` | Optional, privacy-safe reference |
| `customer_trust_status` | Optional, **audit-only**; IVR không dùng để quyết định gọi/skip |
| `risk_flags` | Optional; có thể dùng cho audit/ưu tiên thực thi, nhưng **không được quyết định gọi/skip** |
| `trusted_skip_allowed` | `LEGACY_READ`; **deprecated cho Target mới, không gửi** |
| `call_script_template_id` / `call_script_version` | Optional; nếu thiếu, IVR chọn script đã duyệt theo mode |
| `allowed_script_variables` | Optional; chỉ biến nằm trong whitelist |
| `evidence_policy_version` / `privacy_policy_version` | Target xem đây là IVR-owned/optional; code non-MOCK hiện đang hold khi thiếu — `CONTRACT_DRIFT`, không biến thành nghĩa vụ M3 trước khi hai bên ký |

`trust.risk_evidence_available` chỉ còn `LEGACY_READ`; không còn yêu cầu Module 3 gửi field này để IVR tự skip. Nếu Module 3 vẫn gửi trust/risk metadata phục vụ audit, IVR không được dùng chúng để đảo quyết định `CALL_REQUIRED` đã được Module 3 đưa ra.

### 3.6. `privacy_safe_order_summary`

`additionalProperties: false`. Tất cả field dưới bắt buộc trừ `pronunciation_hints` và `unit_label`.

| Field | Kiểu | Ràng buộc |
| --- | --- | --- |
| `customer_display_name` | string | Tên/xưng hô an toàn, ví dụ `chị An` |
| `order_code_short` | string | Mã rút gọn để đọc |
| `items[]` | array | Ít nhất một item; mỗi item có `public_name`, `quantity`, optional `unit_label` |
| `total_amount` | number | Số không âm; IVR tự đọc thành lời |
| `currency` | string | Chỉ `VND` |
| `delivery_area_short` | string | Chỉ khu vực rút gọn; không gửi địa chỉ đầy đủ |
| `program_display_name` | string | Tên chương trình để đọc |
| `locale` | string | Chỉ `vi-VN` |
| `pronunciation_hints` | object | Optional, gợi ý phát âm |

Module 3 chịu trách nhiệm normalize `delivery_area_short`. IVR vẫn chạy detector PII và có quyền reject nếu nội dung có số nhà/địa chỉ đường phố đầy đủ.

### 3.7. `eligibility_snapshot` sau khi đổi ranh giới

`eligibility_snapshot` là **evidence cho quyết định của Module 3**, không phải đầu vào để IVR tự phân loại khách.

Shape tối thiểu Module 3 gửi:

| Key | Yêu cầu | Ý nghĩa |
| --- | --- | --- |
| `decision` | `ELIGIBLE` | Module 3 xác nhận task đã qua business eligibility và cần gọi |
| `source_version` | Bắt buộc | Version rule/source đã ra quyết định |
| `captured_at` | Bắt buộc | Thời điểm chụp evidence, nằm trong cửa sổ xác nhận |
| `source_available` | Nên là `true` | Nguồn quyết định hoạt động bình thường |
| `blockers` | Nên là `[]` | Task gửi sang không được tự mâu thuẫn bằng blocker active |
| `voice_restriction` | Optional | Provenance của kiểm tra do-not-call |
| `trust` | Optional, audit-only | Không dùng để quyết định skip phía IVR |

IVR được kiểm tra snapshot có đủ, còn hạn và không tự mâu thuẫn; IVR không tự tính lại risk/customer classification của Module 3.

### 3.8. Payload mẫu Module 3 → IVR

```json
{
  "contract_version": "ivr-order-confirmation.v1",
  "task_id": "TASK-0001",
  "order_id": "ORDER-0001",
  "order_code": "GF-2026-0001",
  "order_code_short": "0001",
  "order_version": "17",
  "order_state": "CONFIRMING",
  "program_code": "GOLDEN_HOUR",
  "payment_method_snapshot": "ONLINE",
  "ivr_confirmation_required": true,
  "is_ivr_callable": true,
  "confirmation_window_started_at": "2026-08-12T03:00:00Z",
  "confirmation_window_expires_at": "2026-08-12T03:05:00Z",
  "attempt_policy_version": "gh-v1",
  "max_customer_attempts": 2,
  "attempt_offsets_seconds": [0, 150],
  "customer_ref": "CUST-001",
  "phone_ref": "phref-0001",
  "phone_masked": "84xxxxx0001",
  "phone_validation_status": "VALID",
  "dial_token": "dtok-0001",
  "dial_token_expires_at": "2026-08-12T03:05:00Z",
  "call_restriction": false,
  "privacy_safe_order_summary": {
    "customer_display_name": "chị An",
    "order_code_short": "0001",
    "items": [
      { "public_name": "Nước hồng sâm", "quantity": 2, "unit_label": "hộp" }
    ],
    "total_amount": 560000,
    "currency": "VND",
    "delivery_area_short": "Phường Bến Nghé, Quận Một",
    "program_display_name": "Giờ Vàng",
    "locale": "vi-VN"
  },
  "eligibility_snapshot": {
    "decision": "ELIGIBLE",
    "source_version": "sales-call-decision-v1",
    "captured_at": "2026-08-12T03:00:30Z",
    "source_available": true,
    "blockers": []
  },
  "evidence_ref": "evidence://sales/order-0001/call-decision"
}
```

### 3.9. Response IVR → Module 3

Body response:

| Field | Ý nghĩa |
| --- | --- |
| `decision` | Kết quả intake |
| `ivr_call_job_id` | ID job nếu IVR đã tạo job |
| `blocked_reasons` | Lý do giữ/từ chối nếu có |
| `evidence_ref` | Evidence liên quan quyết định intake |

Các decision Module 3 cần xử lý:

| HTTP / decision | Ý nghĩa | Module 3 làm gì |
| --- | --- | --- |
| `200 TASK_ACCEPTED_CALL_JOB_CREATED` | IVR đã nhận task real-mode và tạo job | Lưu `ivr_call_job_id`, chờ callback |
| `200 TASK_ACCEPTED_DRY_RUN_ONLY` | Chỉ ghi nhận MOCK, không gọi thật | Không chờ callback khách thật |
| `200 TASK_HELD_ADMIN_REVIEW` | IVR chưa thể thực thi vì gate kỹ thuật/an toàn | Không coi là đã gọi; đưa vận hành xử lý |
| `200 TASK_HELD_POLICY_MISSING` | Policy/version thực thi chưa sẵn sàng | Sửa cấu hình/payload; không chờ callback |
| `422 IVR_NOT_OFFICIAL_ORDER` | `order_state` là `QUOTE`/`CART`/`DRAFT`, hoặc định danh đơn không hợp lệ | **Không chờ callback.** Sửa producer: chỉ gửi Official Order |
| `422 IVR_STATE_NOT_CALLABLE` | `is_ivr_callable=false`, hoặc `order_state` không nằm trong tập được gọi | **Không chờ callback.** M3 tự tiếp tục workflow của mình |
| `409 IVR_POLICY_MISMATCH` | attempt policy/cửa sổ trong payload lệch snapshot đã duyệt | **Không chờ callback.** Sửa policy version/payload |
| `422 IVR_CONTACT_INVALID` | `phone_ref`/`dial_token` không dùng được hoặc không đủ hạn | **Không chờ callback.** Sửa dữ liệu liên lạc |
| `422 IVR_SCRIPT_NOT_APPROVED` | Chưa có script version được duyệt cho chế độ đang chạy | **Không chờ callback.** Vấn đề cấu hình phía IVR; báo IVR |
| `409 IVR_OPERATIONAL_BLOCKED` | `call_restriction=true` hoặc blocker vận hành đang active | **Không chờ callback.** Tôn trọng chặn; không retry |
| `400` | JSON/schema sai | Sửa producer |
| `401/403` | Auth/scope/source sai | Sửa auth/allowlist |
| `409` | Idempotency hoặc policy conflict | Không đổi key/body tuỳ tiện; audit |
| `422` | Contract không hợp lệ: state/profile/contact/privacy… | Sửa dữ liệu/contract |

> **`TASK_SKIPPED_TRUSTED_CUSTOMER` là `LEGACY_READ` và bị deprecated trong ranh giới mới.** Module 3 không được dựa vào decision này. Khách cũ/khách không cần gọi phải được Module 3 lọc trước khi gọi API.

Target guarantee cần đạt trước integration thật:

- Chỉ `TASK_ACCEPTED_CALL_JOB_CREATED` nghĩa là IVR đã nhận trách nhiệm thực thi.
- Mọi task đã accepted phải đi đến một terminal outcome/callback hoặc một incident kỹ thuật có thể quan sát; không được âm thầm business-skip.
- Module 3 không chờ callback cho `DRY_RUN` hoặc `HELD_*`.

### 3.10. Quy định producer — `ivr_confirmation_required` và cặp program × payment

> **Trạng thái: ĐỀ XUẤT CỦA IVR, CHỜ M3 + PRODUCT XÁC NHẬN.** Mục này viết ra để M3 có cái cụ thể mà gật hoặc phản bác, thay vì một câu hỏi mở. Nó **chưa** đóng ưu tiên #4 ở §9; đóng bằng chữ ký, không bằng việc mục này tồn tại. Thêm 27/08/2026.

#### R1 — Phải đọc đúng response shape, không suy từ HTTP hay tên decision

Đây là quy định quan trọng nhất trong mục này.

Correction W-0129: runtime **không** trả `200` cho cả 10 decision. Endpoint validate schema trước
service; reject cứng có error envelope `4xx`. Chỉ response `200` mới có
`IvrTaskIntakeResult.decision` và `blocked_reasons`.

Producer của M3 **PHẢI** rẽ nhánh theo HTTP/response shape trước, rồi mới đọc `decision` khi body là
`IvrTaskIntakeResult`. Nếu chỉ kiểm `2xx` mà không đọc decision, M3 vẫn có thể chờ callback cho
`DRY_RUN`/`HELD`; nếu cố đọc decision từ `4xx`, nó sẽ bỏ qua stable `error.code`.

Chỉ đúng một decision nghĩa là IVR đã nhận trách nhiệm gọi: `TASK_ACCEPTED_CALL_JOB_CREATED`. Mọi giá trị khác đều là "M3 tự xử lý tiếp".

#### R2 — Không bao giờ gửi task kèm `ivr_confirmation_required=false`

`ivr_confirmation_required` là **tuyên bố rằng M3 đã quyết định đơn này cần gọi**, không phải một cờ để IVR đọc rồi tự quyết. Hệ quả trực tiếp của ranh giới ở §1: đơn không cần gọi thì **M3 không gửi task** (§1.1), chứ không gửi kèm `false`.

| Producer gửi | Điều gì xảy ra |
| --- | --- |
| `true` | Bình thường |
| Không có field | Deserialize lỗi → `400`. Ồn ào, phát hiện được ngay |
| **`false`** | Schema validator từ chối trước service → `400 IVR_MALFORMED_REQUEST` |

Ca thứ ba hiện fail ồn bằng `400`, nhưng vẫn là lỗi producer: đơn không cần gọi thì M3 không gửi task.

M3 cần trả lời trong §10: **producer set field này ở bước nào, điều kiện nào làm nó thành `true`, và có đường nào gửi `false` sang IVR không.**

#### R3 — Cặp program × payment phải khớp bảng dưới

IVR hiện chỉ nhận hai tổ hợp:

| `program_code` | `payment_method_snapshot` | Kết quả |
| --- | --- | --- |
| `GOLDEN_HOUR` | `ONLINE` | Nhận |
| `TWENTY_FOUR_SEVEN` | `COD` | Nhận |
| `GOLDEN_HOUR` | `COD` | **Loại tại schema** — `400 IVR_MALFORMED_REQUEST` |
| `TWENTY_FOUR_SEVEN` | `ONLINE` | **Loại tại schema** — `400 IVR_MALFORMED_REQUEST` |

**Cập nhật 27/08/2026 — đã có nguồn business, đóng thắc mắc cũ.** Bản trước của mục này cảnh báo rằng cặp `GOLDEN_HOUR + ONLINE` chưa có nguồn business duyệt và lo rằng Giờ Vàng thật có thể là COD. **Thông tin đó đã cũ.** M3 dẫn source of truth trong review ngày 27/08:

| Nguồn | Nội dung đã khóa |
| --- | --- |
| `bussiness-flows/04-tao-don-thanh-toan-va-giao-hang.md:838-850` | Khóa hai use case IVR: `24_7 + COD` và `GOLDEN_HOUR + ONLINE` |
| `bussiness-flows/05-golden-hour-reservation.md:426-435` | Golden Hour chỉ ONLINE; phải từ chối `COD_NOT_ALLOWED`. IVR là bước xác nhận **bổ sung**, không thay thế `PAID`/`COMMITTED`/bind hợp lệ |

Nghĩa là bảng trên **khớp nghiệp vụ**, không phải giả định của IVR. Giờ Vàng COD không tồn tại về mặt nghiệp vụ, nên kịch bản "mất 100% đơn Giờ Vàng vì COD" không phát sinh được.

Việc còn lại **không phải** hỏi Product quyết định lại business pair, mà chỉ là **ký wire mapping và policy version**: xác nhận giá trị chuỗi trên dây khớp §3.11, và gắn `attempt_policy_version` tương ứng. Xem `OD-V1-13` (Module 8 §26) — nay chuyển từ "chưa có nguồn" sang "có nguồn, chờ ký mapping".

_Hai file nguồn nằm trong repo Module 3 (`ginsengfood`), owner IVR không đọc trực tiếp được; ghi nhận theo dẫn chiếu của M3._

#### R4 — W-0129: reason chi tiết nằm ở service boundary, chưa phải wire contract

Service hiện phân loại riêng `IVR_CONFIRMATION_REQUIRED_NOT_TRUE` và
`PROGRAM_PAYMENT_MATRIX_REJECTED`, cùng bảy lỗi contact cụ thể. Nhưng public route có một tầng trước/
sau service:

| Nhóm | Reason service | M3 nhận hiện hành |
| --- | --- | --- |
| `ivr_confirmation_required=false` | `IVR_CONFIRMATION_REQUIRED_NOT_TRUE` | `400 IVR_MALFORMED_REQUEST`; schema chặn trước service |
| program/payment sai | `PROGRAM_PAYMENT_MATRIX_REJECTED` | `400 IVR_MALFORMED_REQUEST`; schema chặn trước service |
| attempt-policy snapshot lệch | `ATTEMPT_POLICY_SNAPSHOT_MISMATCH` | `409 IVR_POLICY_MISMATCH` |
| contact/dial-token sai | một trong bảy mã W-0129 | `422 IVR_CONTACT_INVALID` |

Vì vậy M3 không được branch trên các reason chi tiết này ở public client. Đưa safe reason vào error
details hoặc đổi reject sang `200 decision` là contract change cần M3/owner ký; W-0129 chỉ khóa
taxonomy nội bộ và giữ nguyên wire semantics.

### 3.11. Từ vựng trên dây — giá trị chuỗi chính xác IVR chờ nhận

> Thêm 27/08/2026 sau review của M3; correction W-0129: các mismatch dưới đây không cùng một
> response shape. Bảng ghi đúng status/error hiện hành.

Bảng này là **danh sách đối chiếu bắt buộc trước buổi lab**. Mọi giá trị dưới đây đã được đối chiếu trực tiếp với code IVR, không phải chép từ tài liệu.

| Field | IVR chờ đúng chuỗi | M3 hiện dùng | Sai thì hỏng thế nào |
| --- | --- | --- | --- |
| `program_code` | `GOLDEN_HOUR` / `TWENTY_FOUR_SEVEN` | `24_7` | Enum deserialize lỗi → **`400`**. Ồn ào, phát hiện ngay |
| `phone_validation_status` | `VALID` | `PHONE_VALID` | → `422 IVR_CONTACT_INVALID` |
| `eligibility_snapshot.decision` | `ELIGIBLE` | `ELIGIBLE_FOR_IVR` | → `200 TASK_HELD_ADMIN_REVIEW`. **Im lặng**, và mọi task dồn vào hàng đợi review |
| `order_state` | `CONFIRMING` | khớp | — |
| `payment_method_snapshot` | `ONLINE` / `COD` | khớp | — |

#### Quyết định mapping `24_7` (chốt 27/08/2026)

**M3 map khi gửi.** Producer của M3 chuyển `24_7` → `TWENTY_FOUR_SEVEN` tại lớp assembler trước khi gọi API. IVR **không** nhận thêm biến thể.

Lý do chọn hướng này thay vì để IVR nhận cả hai:

- `TWENTY_FOUR_SEVEN` đã nằm trong OpenAPI baseline và trong enum sinh ra từ contract; đổi nó là breaking change cho một contract đã publish.
- Nhận hai chuỗi cho cùng một khái niệm nghĩa là từ đó về sau dữ liệu lưu trữ có hai dạng, và mọi truy vấn/báo cáo phải nhớ cả hai. Chi phí đó không mất đi, nó chỉ dời sang tương lai.
- Mapping ở một điểm duy nhất phía producer thì kiểm thử được bằng một test; nhận hai dạng thì phải kiểm thử ở mọi nơi đọc field đó.

#### Hai điều kiện chưa từng có trong tài liệu

Review của M3 không nêu hai điều này vì chúng chỉ đọc được từ code IVR. Ghi ra đây để M3 không mất thời gian dò:

1. **`phone_masked` bắt buộc chứa ít nhất một ký tự che** (`x`, `X` hoặc `*`). Gửi số chưa che → `422 IVR_CONTACT_INVALID`.
2. **`dial_token_expires_at` phải lớn hơn `confirmation_window_expires_at`** và phải còn hạn tại thời điểm intake. Token hết hạn sớm hơn cửa sổ → cùng mã từ chối đó.

#### Ghi chú cho người đọc code IVR

Trong repo IVR, `ELIGIBLE_FOR_IVR` **cũng** tồn tại — nhưng nó là decision **IVR tự phát ra sau khi đánh giá**, không phải giá trị IVR chờ nhận. Chiều vào dùng `ELIGIBLE`. Hai từ vựng, hai chiều, hiện nằm cùng một file mà không có chú thích, nên rất dễ nhầm. Khi ký mapping cần nói rõ chiều nào dùng chuỗi nào.

#### Việc M3 xác nhận trước lab

- [ ] Producer map `24_7` → `TWENTY_FOUR_SEVEN`.
- [ ] Producer gửi `phone_validation_status=VALID`, không phải `PHONE_VALID`.
- [ ] Producer gửi `eligibility_snapshot.decision=ELIGIBLE`, không phải `ELIGIBLE_FOR_IVR`.
- [ ] `phone_masked` đã che ít nhất một ký tự.
- [ ] `dial_token_expires_at` > `confirmation_window_expires_at`.

---

## 4. API B — IVR trả kết quả cho Module 3

### 4.1. Endpoint Module 3 phải xây

```http
POST {sales_base_url}/api/v1/internal/orders/{orderId}/ivr-result-callbacks
Authorization: Bearer <service-jwt>
Idempotency-Key: <8-200 chars>
X-Correlation-Id: <1-200 chars>
Content-Type: application/json
```

`{orderId}` phải bằng `order_id` trong body.

> Endpoint generic này hiện được tài liệu đánh dấu **chưa tồn tại** trong Module 3. Endpoint cũ `POST /api/v1/internal/ivr/golden-hour/callbacks` chỉ là compatibility cho Giờ Vàng, shape khác và không phủ 24/7. Không dùng endpoint cũ thay cho Target V1.

### 4.2. Body IVR gửi: 13 field bắt buộc + 1 optional

| Field | Yêu cầu | Ý nghĩa |
| --- | --- | --- |
| `contract_version` | Bắt buộc | `ivr-order-confirmation.v1` |
| `callback_id` | Bắt buộc | ID duy nhất của callback |
| `task_id` | Bắt buộc | Task Module 3 đã gửi |
| `order_id` | Bắt buộc | Phải khớp `{orderId}` |
| `order_version_seen_by_ivr` | Bắt buộc | Chính là `order_version` IVR đã nhận |
| `result_type` | Bắt buộc | Kết quả chuẩn hoá, xem §4.3 |
| `result_reason` | Optional | Mô tả thêm, tối đa 500 ký tự |
| `is_counted_customer_attempt` | Bắt buộc | Có tính là một lần làm phiền khách không |
| `is_final_for_ivr` | Bắt buộc | `true` = IVR không thực hiện thêm attempt |
| `attempt_number` | Bắt buộc | Lần gọi thứ mấy, 1–10 |
| `occurred_at` | Bắt buộc | Thời điểm kết quả xảy ra |
| `recommended_core_action` | Bắt buộc | Gợi ý; Module 3 vẫn tự quyết định |
| `evidence_ref` | Bắt buộc | Evidence của kết quả |
| `audit_ref` | Bắt buộc | Audit reference |

### 4.3. `result_type`

| Giá trị | Ý nghĩa | Tính customer attempt |
| --- | --- | --- |
| `IVR_CONFIRMED` | Khách bấm `1` | Có |
| `IVR_CUSTOMER_CANCELLED` | Khách bấm `0` | Có |
| `IVR_NO_ANSWER_ATTEMPT` | Không nghe máy, còn lượt | Có |
| `IVR_NO_ANSWER_FINAL` | Không nghe máy, hết lượt | Có |
| `IVR_WRONG_INPUT` | Khách bấm sai phím | Có |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | Hết cửa sổ | Không |
| `IVR_INVALID_PHONE_FINAL` | Số không hợp lệ | Không |
| `IVR_TECHNICAL_EXCEPTION` | Lỗi SIM/audio/mạng | Không |
| `IVR_CAPACITY_EXCEPTION` | Không đủ capacity | Không |
| `IVR_OPERATIONAL_BLOCKED` | Blocked trước cuộc gọi | Không |
| `IVR_POLICY_BLOCKED` | Policy chặn trước cuộc gọi | Không |

Module 3 phải tôn trọng `is_counted_customer_attempt`; không suy ra attempt bằng cách đếm callback.

Schema chung giữ đủ 11 giá trị để tương thích. `IVR_OPERATIONAL_BLOCKED` và `IVR_POLICY_BLOCKED` là pre-call/compatibility outcome, không được hiểu là khách đã nghe máy hoặc đã đưa ra lựa chọn.

### 4.4. `recommended_core_action`

| Giá trị | Ý nghĩa |
| --- | --- |
| `CORE_REVALIDATE_AND_CONFIRM_ORDER` | Revalidate rồi cân nhắc xác nhận đơn |
| `CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST` | Revalidate rồi xử lý yêu cầu huỷ |
| `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` | Chưa đổi state; chờ timeout policy |
| `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION` | Revalidate rồi hết hạn xác nhận |
| `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` | Revalidate và chuyển review |
| `CORE_IGNORE_STALE_CALLBACK` | Bỏ qua callback stale |
| `CORE_BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT` | Chặn vì điều kiện vận hành |

Đây chỉ là advisory. Module 3 sở hữu state machine và không bắt buộc thực hiện theo gợi ý.

### 4.5. Payload mẫu IVR → Module 3

```json
{
  "contract_version": "ivr-order-confirmation.v1",
  "callback_id": "CB-0001",
  "task_id": "TASK-0001",
  "order_id": "ORDER-0001",
  "order_version_seen_by_ivr": "17",
  "result_type": "IVR_CONFIRMED",
  "result_reason": "customer pressed 1",
  "is_counted_customer_attempt": true,
  "is_final_for_ivr": true,
  "attempt_number": 1,
  "occurred_at": "2026-08-12T03:01:12Z",
  "recommended_core_action": "CORE_REVALIDATE_AND_CONFIRM_ORDER",
  "evidence_ref": "evidence://ivr/task-0001/result",
  "audit_ref": "audit://ivr/task-0001/result"
}
```

### 4.6. Module 3 phải revalidate trước khi đổi state

1. Idempotency của callback.
2. `order_id` và `order_version_seen_by_ivr` còn tươi.
3. State hiện tại còn cho phép transition.
4. Program/payment snapshot còn phù hợp.
5. Blocker realtime: inventory, recall, sale-lock, quality hold.
6. Evidence và thời gian kết quả còn hợp lệ.

`IVR_CONFIRMED` chỉ có nghĩa khách bấm `1`; không đồng nghĩa `CONFIRMED`, `PAID` hoặc Verified Revenue trong Module 3.

### 4.7. ACK Module 3 trả cho IVR

Body ACK `200` hoặc `409` có các field:

| Field | Yêu cầu | Ý nghĩa |
| --- | --- | --- |
| `code` | Bắt buộc | Semantic outcome |
| `callback_id` | Bắt buộc | Echo ID callback đã xử lý |
| `correlation_id` | Bắt buộc | Correlation của luồng |
| `order_state` | Optional | State sau xử lý nếu được phép công bố |
| `detail` | Optional | Chi tiết privacy-safe, tối đa 500 ký tự |

| HTTP | `code` | IVR xử lý |
| --- | --- | --- |
| `200` | `ACCEPTED` | Hoàn tất delivery |
| `200` | `DUPLICATE_ACCEPTED` | Hoàn tất; không gửi lại |
| `200` | `BLOCKED_BY_CORE` | Dừng; ghi evidence về blocker |
| `200` | `REVIEW_REQUIRED` | Dừng auto-flow; expose cho vận hành |
| `409` | `REJECTED_STALE` | Không retry transport |
| `409` | `IDEMPOTENCY_CONFLICT` | Không retry transport; audit |
| `401/403/422` | error body | Không retry; đưa DLQ/review |
| `429` | error body | Retry theo `Retry-After` |
| `500/503/timeout` | error body | Retry bounded với cùng key và body |

`ACCEPTED` nghĩa là Module 3 đã nhận tín hiệu vào decision path, không đảm bảo đơn đã được xác nhận.

---

## 4A. API C — Bề mặt quản trị: Module 3 điều khiển và quan sát IVR

> **Mục này mới, thêm 28/08/2026.** Nó **không** thuộc luồng đơn hàng ở §3–§4. Đây là bề mặt thứ ba: những endpoint mà **màn hình quản trị** gọi — xem hàng đợi, bật/tắt kill switch, cắt cuộc gọi đang chạy, duyệt lời thoại.
>
> Trước đây IVR tự giữ tài khoản nhân viên, vai trò, mật khẩu và màn hình đăng nhập riêng. **Toàn bộ phần đó đã bị xoá khỏi code, database và tài liệu (`W-0128`, ngày 28/08/2026).** Module 3 sở hữu identity của nhân viên; IVR chỉ còn nhận **credential của service** kèm lời khai *ai bên M3 đang bấm nút*.
>
> Đây là hợp đồng M3 phải code theo khi dựng giao diện quản trị IVR. Client viết theo bản IR-06 trước 28/08 sẽ **401/403 toàn bộ**.

### 4A.1. Ba token riêng biệt, chia theo mức thiệt hại khi lộ

| Biến cấu hình phía IVR | Tầng | Phủ những gì | Nếu credential này lộ |
| --- | --- | --- | --- |
| `IVR_ADMIN_READ_TOKEN` | `read` | Dashboard, hàng đợi, chi tiết call job, báo cáo, danh sách review, trạng thái SIM, đọc feature flag | Rò rỉ dữ liệu vận hành. **Không** dừng được cuộc gọi nào |
| `IVR_ADMIN_WRITE_TOKEN` | `write` | Mở phiếu admin review, vòng đời kịch bản (tạo draft, submit, approve, retire) | Ghi thêm dữ liệu. Vẫn **không** cắt được cuộc gọi đang chạy |
| `IVR_ADMIN_DANGER_TOKEN` | `danger` | Kill switch, pause/resume hàng đợi, disable/enable SIM, manual retry, cắt 1 cuộc gọi, cắt **toàn bộ** cuộc gọi | Dừng được mọi cuộc gọi đang phục vụ khách |

**Vì sao ba token chứ không phải một token + header khai scope:** header là thứ chính người gọi tự viết ra. Ai cầm được token thì viết header nào cũng được, nên header một mình không phải ranh giới. Tách **bí mật** ra làm ba mới là thứ khiến credential nằm sau màn hình báo cáo không đồng thời dừng được cả tổng đài.

**Tầng lồng nhau:** `danger` ⊇ `write` ⊇ `read`. Token danger gọi được endpoint read. Chiều ngược lại thì không. Nhờ vậy M3 không phải mang cả ba token qua mọi code path.

**Fail-closed:** tầng nào **chưa cấu hình token** (chuỗi rỗng) thì **không bao giờ khớp**. Môi trường mới deploy mà quên set biến sẽ **từ chối sạch**, chứ không hiểu nhầm "chưa cấu hình" thành "không cần credential".

#### Rotation không downtime, nhưng overlap có hạn

Mỗi tier có hai biến tùy chọn: `IVR_ADMIN_<TIER>_TOKEN_PREVIOUS` và
`IVR_ADMIN_<TIER>_TOKEN_PREVIOUS_RETIRES_AT`. Quy tắc runtime:

- previous chỉ hợp lệ khi có current và retirement instant ISO-8601 tuyệt đối;
- mọi current/previous của cả ba tier phải khác nhau và dài tối thiểu 24 ký tự;
- trong overlap, current và previous cùng phân giải về đúng **một** tier;
- đúng retirement instant, previous bị từ chối dù biến chưa bị xoá khỏi secret store;
- cấu hình thiếu/trùng/ngắn làm startup validation fail, không hạ xuống chế độ permissive.

Trình tự rollout: cấp current mới + previous cũ + instant kết thúc → rollout IVR cho hội tụ → đổi
caller M3 sang current → chờ qua instant → xoá previous. Nếu nghi rò rỉ, không dùng overlap: thay
khẩn và retire cũ ngay. Token chỉ nằm trong secret store/BFF, không bao giờ tới browser.

### 4A.2. Header trên request quản trị

```http
POST {ivr_base_url}/v1/ivr/order-confirmation/queue:pause
Authorization: Bearer <IVR_ADMIN_DANGER_TOKEN>
X-Service-Scope: ivr.admin.danger
X-Actor-Id: <id tài khoản M3 đang thao tác>
X-Action-Reason: <lý do, ghi vào audit>
```

| Header | Bắt buộc | Giá trị | Ý nghĩa |
| --- | --- | --- | --- |
| `Authorization` | Có | `Bearer <token>` | Một trong ba token ở §4A.1 |
| `X-Service-Scope` | Có | `ivr.admin.read` \| `ivr.admin.write` \| `ivr.admin.danger` | **Khai đúng tầng của token vừa gửi** — xem Bẫy 1 |
| `X-Actor-Id` | Có, gần như mọi endpoint | ≤ 128 ký tự | Nhân viên M3 chịu trách nhiệm hành động — xem Bẫy 2 và §4A.4 |
| `X-Action-Reason` | Chỉ tầng `danger` | ≤ 500 ký tự | Lý do, ghi vào audit |
| `X-Script-Permissions` | Chỉ endpoint kịch bản | CSV, tối đa 16 mục | Quyền duyệt kịch bản M3 tự khai — §4A.5 |
| `X-Destination-Ref` | Không | chuỗi | Phạm vi đích của một lần đổi feature flag |
| `Idempotency-Key` | Chỉ `POST feature-flags/{environment}` | chuỗi | Thiếu là `400` |

> ### ⚠️ Bẫy 1 — `X-Service-Scope` khai theo **token đang cầm**, không phải theo endpoint
>
> Header này phải khớp tầng mà **token của bạn** phân giải ra, chứ không phải tầng mà endpoint yêu cầu.
>
> Gọi endpoint read bằng token danger thì vẫn phải khai `ivr.admin.danger`. Khai `ivr.admin.read` cho "đúng với endpoint" sẽ bị **403**.
>
> Lý do: nếu so header với yêu cầu của endpoint thì tầng lồng nhau sẽ vỡ — token write hợp lệ gọi endpoint read sẽ bị từ chối chỉ vì nó khai thật rằng nó là write.

> ### ⚠️ Bẫy 2 — `X-Actor-Id` bắt buộc trên **29/31** endpoint quản trị, không riêng tầng danger
>
> Kể cả `GET /queue` và `GET /dashboard` cũng đòi header này. Thiếu là **403**, không phải 401.
>
> Chỉ hai endpoint không đòi: `GET feature-flags/{environment}` và `GET feature-flags/{environment}/kill-switch`.
>
> Khác biệt giữa các tầng nằm ở `X-Action-Reason`: chỉ tầng `danger` mới đòi thêm nó.

### 4A.3. Bảng endpoint → tầng bắt buộc

Đường dẫn gốc: `{ivr_base_url}/v1/ivr/order-confirmation`

**Tầng `read` — 15 endpoint**

| Method | Đường dẫn | `X-Actor-Id` |
| --- | --- | --- |
| `GET` | `/queue` | Có |
| `GET` | `/dashboard` | Có |
| `GET` | `/call-jobs` | Có |
| `GET` | `/call-jobs/{ivrCallJobId}/detail` | Có |
| `GET` | `/sim-channels` | Có |
| `GET` | `/scripts` | Có |
| `GET` | `/integration-status` | Có |
| `GET` | `/review-items` | Có |
| `GET` | `/analytics/summary` | Có |
| `GET` | `/analytics/trend` | Có |
| `GET` | `/analytics/breakdown` | Có |
| `GET` | `/analytics/export` | Có |
| `GET` | `/scripts/{templateId}/{version}` | Có |
| `GET` | `/feature-flags/{environment}` | **Không** |
| `GET` | `/feature-flags/{environment}/kill-switch` | **Không** |

`GET /analytics/export` cố tình là `GET` chứ không phải `POST`: bản trích xuất là thao tác **đọc có audit**, giữ verb read-only để bảo toàn tính chất "nhóm báo cáo không có bề mặt ghi".

**Tầng `write` — 5 endpoint** *(+3 endpoint dev chỉ tồn tại ngoài production)*

| Method | Đường dẫn | Ghi chú |
| --- | --- | --- |
| `POST` | `/admin-reviews` | Mở phiếu review |
| `POST` | `/scripts` | Tạo draft kịch bản |
| `POST` | `/scripts/{templateId}/{version}:submit` | Trình duyệt |
| `POST` | `/scripts/{templateId}/{version}:approve` | Cần `X-Script-Permissions` — §4A.5 |
| `POST` | `/scripts/{templateId}/{version}:retire` | Cần `X-Script-Permissions` |
| `POST` | `/dev/seed:load` | **Chỉ non-production** |
| `POST` | `/dev/scenarios/{scenarioId}:dry-run` | **Chỉ non-production** |
| `POST` | `/dev/integration-profiles/{profileId}:apply` | **Chỉ non-production** |

Ba route `/dev` **không được đăng ký** khi môi trường là production hoặc khi đang cho phép gọi khách thật. Chúng không tồn tại chứ không phải bị chặn — M3 sẽ nhận `404`, không phải `403`.

**Tầng `danger` — 8 endpoint** *(mọi endpoint đều đòi thêm `X-Action-Reason`)*

| Method | Đường dẫn | Hậu quả tức thì |
| --- | --- | --- |
| `POST` | `/queue:pause` | Ngừng phát cuộc gọi mới |
| `POST` | `/queue:resume` | Phát lại |
| `POST` | `/sim-channels/{simChannelId}:disable` | Rút một kênh khỏi vận hành |
| `POST` | `/sim-channels/{simChannelId}:enable` | Đưa kênh trở lại |
| `POST` | `/technical-retries` | Quay lại thủ công |
| `POST` | `/call-jobs/{ivrCallJobId}:terminate` | **Cắt một cuộc đang nói với khách** |
| `POST` | `/call-jobs:terminate-all` | **Cắt toàn bộ cuộc đang chạy** |
| `POST` | `/feature-flags/{environment}` | Kill switch. Đòi thêm `Idempotency-Key`; nhận `X-Destination-Ref` tuỳ chọn |

`:terminate` và `:terminate-all` là **hai route riêng, hai lần bấm riêng, hai lý do riêng**. Kill switch chặn cuộc *tiếp theo*; `:terminate-all` cắt cuộc *đang nói*. Giao diện M3 không nên gộp hai nút này.

### 4A.4. Tầng `danger` đòi thêm một con người và một lý do

Token chứng minh **tầng**; `X-Actor-Id` + `X-Action-Reason` chứng minh **ai bên M3 yêu cầu và vì sao**. Thiếu cả hai thì dòng audit "ai đã dừng toàn bộ cuộc gọi lúc 3 giờ sáng" chỉ ghi được chữ `service` — sáng hôm sau không trả lời được gì.

Ràng buộc IVR áp:

| Header | Rỗng | Độ dài | Nội dung |
| --- | --- | --- | --- |
| `X-Actor-Id` | Không được | ≤ 128 | Qua bộ lọc PII |
| `X-Action-Reason` | Không được | ≤ 500 | Qua bộ lọc PII |

> ### ⚠️ Bẫy 3 — đừng đặt **tên người** vào `X-Actor-Id`
>
> Bộ lọc PII từ chối chuỗi trông giống địa chỉ Việt Nam. Nhánh không dấu bắt các từ `duong`, `so nha`, `ngo`, `hem`, `ngach`, `thon`, `ap` khi theo sau là khoảng trắng rồi chữ hoặc số. Hệ quả đã kiểm chứng:
>
> | `X-Actor-Id` | Kết quả |
> | --- | --- |
> | `Duong Minh Tuan` | ❌ `403` |
> | `Ngo Van A` | ❌ `403` |
> | `Ap Bac 3` | ❌ `403` |
> | `Tran Van Duong` | ✅ qua |
> | `ngo-van-a` | ✅ qua |
> | `op-8842` | ✅ qua |
>
> Dương và Ngô là họ phổ biến. Nếu M3 đẩy tên hiển thị vào header này, **nhân viên họ Dương/Ngô sẽ bị 403 còn người khác thì không** — lỗi rất khó truy vì nó phụ thuộc vào người đang trực.
>
> **Khuyến nghị:** gửi **id tài khoản đục** (`op-8842`, `m3-user-1193`, hoặc UUID). Tên hiển thị tra ở phía M3 khi đọc audit. Áp dụng y hệt cho `X-Action-Reason`: câu lý do có chữ "đường Lê Lợi" cũng bị chặn.

### 4A.5. `X-Script-Permissions` — quyền duyệt kịch bản do M3 tự khai

Chỉ dùng cho nhóm `/scripts`. Định dạng CSV, IVR đọc tối đa 16 mục.

| Giá trị | Cho phép |
| --- | --- |
| `IVR_SCRIPT_EDIT` | Tạo/sửa draft |
| `IVR_SCRIPT_REVIEW` | Trình duyệt |
| `IVR_SCRIPT_APPROVE_MOCK` | Duyệt cho môi trường mock |
| `IVR_SCRIPT_APPROVE_LAB` | Duyệt cho lab |
| `IVR_SCRIPT_APPROVE_CONTENT` | Duyệt **nội dung** cho production |
| `IVR_SCRIPT_APPROVE_PRIVACY_LEGAL` | Duyệt **privacy/pháp lý** cho production |
| `IVR_SCRIPT_RETIRE` | Ngừng một version |

**Đây là lời khai, không phải bằng chứng.** M3 sở hữu identity nên M3 là nguồn có thẩm quyền về việc nhân viên của mình có quyền gì; IVR ghi nhận lời khai đó. Nghĩa là: quyền này chỉ mạnh bằng đúng khả năng M3 bảo vệ token và không để client tự viết header.

**Nhưng IVR vẫn tự giữ hai luật bốn mắt, không nhường:**

1. Người **tạo** một version không được **duyệt** chính version đó.
2. Duyệt **nội dung** và duyệt **privacy/pháp lý** phải là **hai `X-Actor-Id` khác nhau**.

Hai luật này chạy theo **danh tính**, không theo quyền. Cấp đủ cả 7 quyền cho một actor vẫn không vượt được. Giao diện M3 phải chuẩn bị cho việc một thao tác duyệt bị từ chối dù người bấm "có đủ quyền".

### 4A.6. Lỗi và cách chẩn đoán

| HTTP | `error.code` | Nghĩa |
| --- | --- | --- |
| `401` | `IVR_UNAUTHENTICATED` | Không có `Authorization`, sai định dạng, hoặc token không khớp tầng nào |
| `403` | `IVR_FORBIDDEN_CALLER` | Token hợp lệ nhưng request bị từ chối |
| `400` | `IVR_MALFORMED_REQUEST` | Thiếu `Idempotency-Key` ở endpoint feature flag |
| `404` | — | Route `/dev` không tồn tại ở môi trường này |

`403` gộp nhiều nguyên nhân vào một mã. Thứ tự kiểm tra khi M3 gặp `403`:

| # | Kiểm tra | Triệu chứng đặc trưng |
| --- | --- | --- |
| 1 | `X-Service-Scope` có khớp tầng **token đang cầm** không? | Sai ở **mọi** endpoint, kể cả read |
| 2 | `X-Actor-Id` có mặt và ≤ 128 ký tự không? | Sai ở 29/31 endpoint |
| 3 | `X-Actor-Id` có dính bộ lọc PII không? | Sai **chỉ với một số nhân viên** — xem Bẫy 3 |
| 4 | Endpoint tầng danger: có `X-Action-Reason` chưa? | Chỉ sai ở nhóm danger |
| 5 | Token có đủ tầng cho endpoint không? | Token read gọi endpoint danger |
| 6 | Endpoint kịch bản: có vi phạm luật bốn mắt không? | Chỉ sai ở `:approve` |

Nguyên nhân 1 và 5 chặn ở tầng policy, trước khi handler chạy. Nguyên nhân 6 chặn trong domain, sau khi đã qua auth.

### 4A.7. Những thứ **không còn tồn tại** — đừng đi tìm

| Đã xoá | Thay bằng |
| --- | --- |
| Bảng tài khoản console và bảng vai trò trong DB | Không có. M3 giữ tài khoản |
| **11 endpoint trong OpenAPI**: `/auth/sign-in`, `/auth/session`, `/auth/sign-out`, sáu route `/accounts*`, `/account-roles` | Không có |
| **15 schema `Console*`**, tham số `AccountId`, response `ConsoleAccountError` | Không có |
| Hai mã lỗi `IVR_ACCOUNT_CONFLICT`, `IVR_ACCOUNT_POLICY_VIOLATION` | Danh mục lỗi còn **16 mã** |
| Trang `/login`, `/accounts`, `/roles`, `/profile`, session cookie, màn hình đổi mật khẩu | Không có |
| Catalogue 19 permission và policy theo từng permission | Ba tầng ở §4A.1 |
| Header `X-Permissions` (seam mock cũ) | Đã gỡ, không còn được đọc ở bất kỳ đâu |

> **Quan trọng cho M3 nếu đã sinh client từ OpenAPI.** Bản spec anh nhận trước ngày 28/08/2026
> (`1.0.0-draft.21` trở về trước) **vẫn còn** 11 endpoint đó. Sinh client từ bản cũ sẽ ra
> `signInConsoleAccount()`, `listConsoleAccounts()`, `createConsoleAccount()`… — gọi vào là `404`.
>
> Lấy lại spec ở `specs/api/openapi/ivr-order-confirmation.v1.yaml`, phiên bản **`1.0.0-draft.22`**,
> rồi sinh lại. So sánh đầy đủ hai bản nằm ở
> `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.20-to-v1.0.0-draft.22.md`.

**Admin UI trong repo IVR giữ lại làm bản mẫu tham chiếu local.** Helm từ chối deploy nó; không có
Service/UI pod của IVR. Module 3 BFF là caller duy nhất được platform cấu hình NetworkPolicy tới
API. Mục đích của mẫu là để M3 đọc và dựng lại nhanh trong console của mình: route handler server
chọn credential tier, còn browser không thấy credential.

Những gì bản mẫu **đã bỏ** và M3 phải tự dựng ở phía mình: màn hình đăng nhập, quản trị tài khoản, quản trị vai trò và trang hồ sơ cá nhân. Thẻ danh tính ở chân sidebar nay chỉ còn là **nhãn tĩnh** hiển thị `X-Actor-Id`, không phải link — đúng chỗ để M3 gắn lại link hồ sơ khi có phiên đăng nhập thật.

Khi M3 dựng lại, phần kiểm tra phiên đăng nhập đặt ở **phía M3**, rồi vẫn gửi token tầng xuống IVR — **IVR không nhận session của M3 thay cho token**.

### 4A.8. Việc Module 3 phải quyết

| # | Quyết định | Vì sao không thể để mặc định |
| --- | --- | --- |
| 1 | Ai giữ ba token, secret-store path nào, lịch/owner rotation nào | Runtime overlap hữu hạn đã có; custody và thao tác production vẫn thuộc M3/Platform |
| 2 | Vai trò nào bên M3 ánh xạ sang tầng nào | Nếu mọi vai trò đều nhận token danger thì việc tách ba tầng vô nghĩa |
| 3 | Định dạng `X-Actor-Id` | Đẩy tên hiển thị vào sẽ vỡ theo họ của nhân viên — Bẫy 3 |
| 4 | UI bắt nhập lý do ở tầng danger thế nào | Thiếu `X-Action-Reason` là 403; UI cần chặn trước khi gửi |
| 5 | Ai bên M3 giữ quyền duyệt nội dung và ai giữ quyền duyệt privacy/pháp lý | Phải là hai người khác nhau, IVR cưỡng chế |

---

## 5. Khách cũ/khách mới và risk policy — Module 3 xử lý hoàn toàn

### 5.1. Quy tắc tích hợp

| Kết quả business decision trong Module 3 | Module 3 làm gì | IVR làm gì |
| --- | --- | --- |
| Không cần gọi | Không gửi task; tự tiếp tục workflow | Không biết đơn tồn tại |
| Cần gọi | Gửi task call-ready với `ivr_confirmation_required=true` | Validate execution gates rồi gọi |
| Chưa quyết định được | Không gửi cho đến khi có quyết định hoặc xử lý theo workflow review của M3 | Không tham gia |

Module 3 tự sở hữu tiêu chí khách cũ/khách mới, verified-order history, duplicate, COD-fail, risk address/phone/value và các rule khác. IVR không hardcode ngưỡng hoặc suy luận lại từ các field này.

### 5.2. Những cơ chế IVR-side không còn thuộc Target mới

| Cơ chế cũ | Target mới |
| --- | --- |
| IVR đọc `risk_flags` để quyết định gọi/skip | `risk_flags` không được đảo quyết định call/skip; nếu giữ thì chỉ phục vụ audit/ưu tiên thực thi |
| IVR cần `trust.risk_evidence_available=true` để skip | `LEGACY_READ`; không còn yêu cầu tích hợp |
| `trusted_skip_allowed=false` là veto | `LEGACY_READ`; field deprecated, Module 3 không gửi |
| IVR trả `TASK_SKIPPED_TRUSTED_CUSTOMER` | `LEGACY_READ`; decision deprecated, M3 lọc trước khi push |
| Thiếu trust evidence thì IVR gọi mặc định | Không áp dụng; M3 không gửi khi chưa quyết định |

### 5.3. Trạng thái alignment phía IVR

`W-0123` đã alignment repo IVR theo ranh giới mới:

- active domain/service/config/persistence không còn tạo trusted-skip;
- OpenAPI `draft.21` đánh dấu trust fields là deprecated/ignored và giữ
  `TASK_SKIPPED_TRUSTED_CUSTOMER` ở mức `LEGACY_READ` cho client/row lịch sử;
- authority tests chứng minh trust/risk metadata không đảo quyết định gọi, trong khi
  do-not-call/contact/window/capacity gates vẫn fail-closed;
- không drop migration/cột/enum cũ trong rolling compatibility window.

Đây là `LOCAL_CODE_DONE`, **không** phải production readiness. Target DB preflight, M3 consumer
evidence, sandbox/auth, callback endpoint, hosted CI và owner sign-off vẫn phải đóng độc lập.

Trạng thái gate: `LOCAL_ALIGNMENT_IMPLEMENTED_EXTERNAL_GATES_OPEN`.

---

## 6. `dial_token` — chưa chốt

Task hiện mang một `dial_token`, nhưng một task có thể cần nhiều attempt và retry kỹ thuật. Module 3 + Security cần chọn một trong các phương án:

| Phương án | Đánh đổi |
| --- | --- |
| `dial_tokens[]` per-attempt | Phải dự đoán đủ số lần retry |
| Endpoint reissue/refresh | Có round-trip đồng bộ trước khi quay |
| Token bundle | Vẫn phải sizing cho retry kỹ thuật |
| Token reusable theo TTL + risk control | Bỏ one-use; cần threat model và audit |

Cần xác nhận thêm:

- service nào giữ mapping `dial_token → E.164`;
- ai vận hành/audit vault;
- TTL và số lần resolve;
- cơ chế rotation/revocation;
- hành vi khi token hết hạn giữa các attempt.

Không gửi số E.164 trực tiếp trong task.

---

## 7. Auth production

Cần Security/Platform cung cấp:

- issuer URL, JWKS URL, thuật toán ký và rotation;
- audience và TTL token;
- scope cho Module 3 gọi IVR và IVR callback Module 3;
- sandbox credential + hướng dẫn lấy token;
- quyết định mTLS, cấp/rotation certificate;
- ngày tắt cơ chế `X-Internal-Token` compatibility.

Đề xuất scope tối thiểu:

- `ivr.task.write`: Module 3 → IVR task intake;
- `ivr.result.write`: IVR → Module 3 result callback.

Không có sandbox credential thì chưa chạy được integration test thật.

**Mục này chỉ nói về hai API nghiệp vụ ở §3–§4.** Bề mặt quản trị (§4A) dùng **credential riêng, không dùng chung** với service JWT ở đây: ba token tĩnh `IVR_ADMIN_READ/WRITE/DANGER_TOKEN`. Hai hệ credential tách rời có chủ đích — token mà giao diện quản trị cầm không được đồng thời giao được task, và ngược lại.

Khi Security/Platform chốt auth profile production, cần trả lời thêm cho §4A: ba token này có chuyển sang cùng issuer/JWKS không, ai cấp và xoay vòng thế nào, và cất ở đâu phía Module 3.

---

## 8. Những gì IVR không làm

| IVR không | Owner đúng |
| --- | --- |
| Phân loại khách cũ/khách mới hoặc quyết định đơn nào cần gọi | Module 3 |
| Tự đọc `risk_flags` để đảo quyết định của Module 3 | Module 3 |
| Polling hoặc truy vấn order/customer từ Module 3 | Module 3 push snapshot |
| Tạo order hoặc sinh `order_code` | Module 3 |
| Ghi/đổi trạng thái đơn | Module 3 |
| Xác nhận thanh toán/doanh thu | Module 3/payment domain |
| Huỷ đơn khi khách bấm `0` | Module 3 revalidate rồi quyết định |
| Đọc inventory/recall/sale-lock realtime | Module 3 revalidate khi callback |
| Gửi SMS/notification | Ngoài IVR Target V1 |
| Ghi CRM note | Ngoài IVR Target V1 |

---

## 9. Thứ tự ưu tiên và release gates

| Ưu tiên | Việc | Owner | Trạng thái |
| --- | --- | --- | --- |
| **1** | Ký ranh giới “M3 quyết định, IVR thực thi” và rule producer chỉ gửi `CALL_REQUIRED` | M3 + M8 | `OWNER_SIGNOFF_REQUIRED` |
| **2** | Gỡ IVR-side trusted skip khỏi Target contract/code/test; giữ `TASK_SKIPPED_TRUSTED_CUSTOMER` là `LEGACY_READ` | M8 | `CODE_DONE_LOCAL` — `W-0123`; external gates không suy xanh |
| **3** | Xây callback generic phủ Golden Hour + 24/7 | M3 | `NOT_BUILT_UPSTREAM` |
| **4** | Ký wire mapping program/payment/state và nguồn `ivr_confirmation_required`. Business pair **đã có nguồn** (Flow 04/05, xem §3.10 R3) nên không cần Product quyết lại; còn lại là ký chuỗi trên dây theo **§3.11** và gắn `attempt_policy_version` | M3 + Product | `OWNER_DECISION_REQUIRED` |
| **4b** | Sửa 3 field lệch chuỗi ở **§3.11** — M3 map `24_7`→`TWENTY_FOUR_SEVEN`, `PHONE_VALID`→`VALID`, `ELIGIBLE_FOR_IVR`→`ELIGIBLE` | M3 | `IMPLEMENTATION_ALIGNMENT_REQUIRED` |
| **5** | Auth profile + sandbox credential | Security/Platform | `BLOCKED_EXTERNAL` |
| **6** | Ký minimal `eligibility_snapshot` dùng làm evidence, không phải IVR business decision | M3 + M8 | `OWNER_SIGNOFF_REQUIRED` |
| **7** | Chọn `dial_token` model và trust boundary | M3 + Security + M8 | `OWNER_DECISION_REQUIRED` |
| **8** | Duyệt lời thoại/privacy và giới hạn `items[]` | Product + Privacy/Legal | `OWNER_APPROVAL_REQUIRED` |
| **9** | Nhận bàn giao bề mặt quản trị **§4A**: ai giữ ba token, vai trò M3 nào ánh xạ sang tầng nào, định dạng `X-Actor-Id` | M3 + Security | `OWNER_DECISION_REQUIRED` — mới 28/08/2026 |

Chưa được gọi integration/production ready khi các gate P0 trên chưa đóng.

---

## 10. Checklist Module 3 phản hồi

### Ranh giới nghiệp vụ

- [ ] Xác nhận Module 3 là owner duy nhất quyết định đơn nào cần gọi.
- [ ] Xác nhận Module 3 xử lý khách cũ/khách mới và risk policy trước khi push.
- [ ] Xác nhận đơn không cần gọi sẽ **không** được gửi sang IVR.
- [ ] Mô tả producer chạy ở bước nào và điều kiện chuyển đơn sang `CONFIRMING`.
- [ ] Xác nhận mọi task gửi sang đều có `ivr_confirmation_required=true`, `call_restriction=false`, evidence `ELIGIBLE` và không có blocker active.

### Contract task intake

- [ ] Ký ma trận `program_code × payment_method_snapshot × order_state`.
- [ ] Xác định nguồn và vòng đời của `ivr_confirmation_required` — trả lời theo §3.10 R2: producer set ở **bước nào**, điều kiện nào làm nó thành `true`, và có đường nào gửi `false` sang IVR không.
- [ ] Xác nhận producer rẽ nhánh theo trường `decision`, **không** chỉ theo HTTP status (§3.10 R1). Nêu rõ producer xử lý thế nào với từng `TASK_REJECTED_*` và `TASK_BLOCKED_OPERATIONAL`.
- [ ] Xác nhận tổ hợp `program_code × payment_method_snapshot` thật sẽ gửi (§3.10 R3). **Đã đóng 27/08** bằng Flow 04/05; chỉ còn ký wire mapping.
- [ ] Đối chiếu đủ 5 dòng bảng từ vựng §3.11 và 5 ô checklist cuối mục đó. Ba field đang lệch chuỗi, hai trong ba hỏng im lặng.
- [ ] Xác định khi nào `order_version` bump.
- [ ] Ký attempt policy theo từng program: window, số attempt, offsets.
- [ ] Chốt cách normalize `delivery_area_short` và giới hạn `items[]`.
- [ ] Xác nhận không gửi `trusted_skip_allowed` (`LEGACY_READ`); trust/risk metadata nếu giữ không được dùng phía IVR để quyết định gọi/skip.

### Callback

- [ ] Cung cấp OpenAPI endpoint generic `/api/v1/internal/orders/{orderId}/ivr-result-callbacks`.
- [ ] Ký ACK taxonomy và body `code/callback_id/correlation_id`.
- [ ] Ký idempotency boundary và thời gian giữ key.
- [ ] Xác nhận revalidate version/state/inventory/recall/sale-lock/quality-hold trước transition.
- [ ] Xác nhận tôn trọng `is_counted_customer_attempt`.
- [ ] Chốt timeout worker sau `IVR_NO_ANSWER_FINAL`.

### Bề mặt quản trị (§4A — mới 28/08/2026)

- [ ] Xác nhận M3 sẽ dựng giao diện quản trị IVR và nhận bàn giao hợp đồng §4A.
- [ ] Chỉ định nơi cất ba token `IVR_ADMIN_READ/WRITE/DANGER_TOKEN` và chu kỳ xoay vòng.
- [ ] Ký ánh xạ **vai trò M3 → tầng IVR**. Nêu rõ vai trò nào được chạm `danger`.
- [ ] Chốt định dạng `X-Actor-Id` là **id đục**, không phải tên hiển thị (Bẫy 3, §4A.4).
- [ ] Xác nhận UI bắt buộc nhập `X-Action-Reason` trước khi gửi mọi thao tác tầng `danger`.
- [ ] Chỉ định hai người khác nhau giữ quyền duyệt **nội dung** và **privacy/pháp lý** (§4A.5).
- [ ] Xác nhận M3 không kỳ vọng IVR còn màn hình đăng nhập, bảng tài khoản hay endpoint `/api/auth/*` (§4A.7).
- [ ] **Sinh lại client từ OpenAPI `1.0.0-draft.22`.** Bản trước đó vẫn công bố 11 endpoint `auth`/`accounts` nay đã bị gỡ (§4A.7).

### Platform

- [ ] Chốt `dial_token` model, vault owner và audit boundary.
- [ ] Cấp auth profile và sandbox credential.
- [ ] Cung cấp base URL/OpenAPI versioning/deprecation policy.

---

## Ô ký

| Vai trò | Xác nhận | Tên | Ngày |
| --- | --- | --- | --- |
| Owner Module 3 — business decision + producer | ____________ | ____________ | ______ |
| Owner Module 8 — IVR execution boundary | ____________ | ____________ | ______ |
| Security/Platform — auth + dial token | ____________ | ____________ | ______ |
| Privacy/Legal — speech payload | ____________ | ____________ | ______ |

**Ghi chú chung:** ______________________________________________
