# IR-06 — Module 3 cần cung cấp gì cho IVR

**Gửi:** Team **Module 3 — `ginsengfood-business-platform`** (Commerce / Order Core / Sales Extensions / CRM — Customer Identity)
**Từ:** Team Module 8 — IVR Order Confirmation (.NET, service tách biệt)
**Ngày:** 2026-08-25 · **Trạng thái:** `TARGET_V1_DRAFT` — chờ Module 3 trả lời

> **Tài liệu này thay thế việc phải đọc 5 file khác.** Nó gom tất cả những gì IVR cần từ Module 3 vào một chỗ, kèm payload mẫu copy-paste được và ô ký ở cuối. Nguồn chi tiết: [IR-01](01-sales-platform-requirements.md), [API-05](../specs/api/05-order-core-contracts.md), [closure pack T-01…T-09](../docs/contracts/target-v1-closure-pack/README.md), [decisions-log](../plan/ivr-orther/decisions-log.md).
>
> **Về tên gọi:** Module 3 là **một** module — repository `ginsengfood-business-platform` — gồm cả Commerce/Order Core, Sales Extensions và CRM/Customer Identity. Các tài liệu cũ trong repo IVR tách thành "Module 3" và "Module 3.1" (ví dụ `questions-to-module-3-and-3.1.md`, mã quyết định `D-*` vs `DC-*`, path `phase-3.1/07`). Đó là cách đánh số của **vòng hỏi 2026-07-02**, không phải hai đội. Khi đọc tài liệu cũ, hiểu `3.1` = cùng Module 3.

---

## 0. Đọc trong 2 phút

IVR cần **4 thứ** từ Module 3. Hai thứ đầu chặn cứng ngày cắm thật.

| # | Cần gì | Ai làm | Ưu tiên | Trạng thái |
| --- | --- | --- | --- | --- |
| **A** | Module 3 **gọi API của IVR** để đẩy task xác nhận đơn | Order Core | **P0** | Chưa có producer cho 24/7 COD |
| **B** | Module 3 **mở 1 endpoint** để IVR trả kết quả cuộc gọi về | Sales API/Core | **P0** | Endpoint generic **chưa tồn tại** |
| **C** | Cơ chế `dial_token` — cấp, resolve, TTL, dùng mấy lần | Sales + Security | **P0** cho gọi thật | Chưa chốt, có 4 phương án |
| **D** | Service auth production: issuer, JWKS, audience, scope, sandbox credential | Security/Platform | **P0** | Chưa có gì |

Ngoài ra có **1 yêu cầu nhỏ** đang mở, chỉ tốn 1 field: xem [§6 — không gọi khách cũ](#6-không-gọi-khách-cũ-od-15--chỉ-cần-1-field).

---

## 1. IVR là gì, và tại sao nó cần Module 3

IVR Order Confirmation là hệ thống **gọi điện tự động ra ngoài** để xác nhận đơn hàng, chống đơn ảo. Khách nghe máy, nghe đọc tóm tắt đơn, bấm phím `1` (xác nhận) hoặc `0` (huỷ).

Ba điều cần nắm để hiểu phần còn lại của tài liệu:

1. **IVR không có database đơn hàng.** Nó không biết đơn nào tồn tại. Mọi thứ nó biết đều do Module 3 gửi sang trong một payload duy nhất.
2. **IVR không bao giờ ghi trạng thái đơn** (quyết định `D-02`). Nó chỉ gửi **tín hiệu** về. Module 3 nhận tín hiệu, tự revalidate, tự quyết định chuyển trạng thái. Khách bấm `1` **không có nghĩa** đơn được xác nhận — Module 3 mới là bên quyết.
3. **IVR fail-closed.** Thiếu bất kỳ bằng chứng nào, nó **không gọi**. Nó không đoán. Điều này nghĩa là: gửi thiếu field → đơn không được gọi, và Module 3 sẽ không biết trừ khi đọc mã lỗi trả về.

---

## 2. ⚠️ Hướng gọi — chỗ dễ hiểu nhầm nhất

**IVR KHÔNG lấy đơn từ Module 3.** Không có polling, không có `GET /orders`, IVR không truy vấn gì cả.

Luồng thật là **hai chiều push**:

```
                    ┌─────────────────────────────────────────┐
                    │  (A) Module 3 GỌI API của IVR           │
   ┌──────────┐     │  POST {ivr}/v1/ivr/order-confirmation/  │     ┌──────────┐
   │ Module 3 │─────┤        tasks                            ├────>│   IVR    │
   │Order Core│     │  → "đơn này cần gọi xác nhận, đây là    │     │  (.NET)  │
   └──────────┘     │     toàn bộ dữ liệu"                    │     └──────────┘
        ▲           └─────────────────────────────────────────┘          │
        │                                                                 │
        │                                                          ┌──────▼──────┐
        │                                                          │ gọi khách,  │
        │                                                          │ nhận phím   │
        │                                                          └──────┬──────┘
        │           ┌─────────────────────────────────────────┐          │
        │           │  (B) IVR GỌI API của Module 3           │          │
        └───────────┤  POST {sales}/api/v1/internal/orders/   │<─────────┘
                    │        {orderId}/ivr-result-callbacks   │
                    │  → "khách bấm 1", Module 3 tự quyết định│
                    └─────────────────────────────────────────┘
```

**Nghĩa là Module 3 phải làm 2 việc, không phải 1:**

| Việc | Module 3 đóng vai | Phải làm gì |
| --- | --- | --- |
| **A** | **Client** | Viết code gọi `POST` sang IVR mỗi khi có đơn cần xác nhận |
| **B** | **Server** | Mở một endpoint `POST` để IVR gọi về báo kết quả |

Nếu chỉ làm A mà không làm B: IVR gọi được khách nhưng **không có lối trả kết quả**, đơn treo mãi ở `CONFIRMING`.
Nếu chỉ làm B mà không làm A: không có đơn nào được gọi.

---

## 3. VIỆC A — Module 3 đẩy task sang IVR

### 3.1. Endpoint

```
POST {ivr_base_url}/v1/ivr/order-confirmation/tasks
Content-Type: application/json
```

| Header | Bắt buộc | Ghi chú |
| --- | --- | --- |
| `Authorization: Bearer <token>` | ✅ | Service JWT. Dev dùng mock JWT; production chờ **Việc D** |
| `Idempotency-Key` | ✅ | 8–200 ký tự. Gửi lại **cùng key + cùng body** → IVR trả lại kết quả cũ, không tạo job trùng |
| `X-Correlation-Id` | ✅ | 1–200 ký tự, xuyên suốt toàn luồng để tra log |
| `X-Source-System` | ✅ | Định danh hệ gửi |

**Quan trọng:** nếu body cũng có `correlation_id` thì nó **phải trùng** với header, lệch → `422`.

### 3.2. Ma trận program × payment — chỉ 2 cặp giá trị được chấp nhận

| `program_code` | `payment_method_snapshot` | Kết quả |
| --- | --- | --- |
| `GOLDEN_HOUR` | `ONLINE` | ✅ nhận |
| `TWENTY_FOUR_SEVEN` | `COD` | ✅ nhận |
| `GOLDEN_HOUR` | `COD` | ❌ `422` |
| `TWENTY_FOUR_SEVEN` | `ONLINE` | ❌ `422` |

> ⚠️ **Đây là điểm nghi ngờ số 1 cần Module 3 xác nhận.** Tài liệu business mà IVR đọc được (`DS-01`) nói ngược lại: IVR-callable là **COD-only**, và `GOLDEN_HOUR + ONLINE` **không** callable. IVR đang enforce ma trận trên ở **4 tầng độc lập** (OpenAPI, intake, eligibility, CHECK constraint của DB). Nếu ma trận business khác, **cả 4 tầng phải sửa cùng lúc**. Chi tiết: [T-01](../docs/contracts/target-v1-closure-pack/T-01-program-matrix.md).

`24_7` (có gạch dưới) chỉ được chấp nhận ở tầng compat cũ, không phải Target V1.

### 3.3. 22 field BẮT BUỘC

Thiếu bất kỳ field nào → `422`, không tạo job.

| Field | Kiểu | Ví dụ | Ý nghĩa / bẫy |
| --- | --- | --- | --- |
| `contract_version` | string | `"ivr-order-confirmation.v1"` | Hằng số, chỉ nhận đúng giá trị này |
| `task_id` | string | `"TASK-0001"` | ID của task, do Module 3 sinh |
| `order_id` | string | `"ORDER-0001"` | ID đơn. Phải trùng `{orderId}` trên URL callback sau này |
| `order_code` | string | `"GF-2026-0001"` | Mã đơn đầy đủ. **Không bao giờ được đọc cho khách nghe** |
| `order_version` | string | `"17"` | **Ảnh chụp chống race.** IVR giữ nguyên si, trả lại y hệt lúc callback |
| `order_state` | string | `"CONFIRMING"` | ⚠️ Hợp đồng khai "opaque" nhưng IVR đang **hard-code so sánh `"CONFIRMING"`**. Xem §3.7 |
| `payment_method_snapshot` | string | `"ONLINE"` | `ONLINE` hoặc `COD`, theo ma trận §3.2 |
| `program_code` | string | `"GOLDEN_HOUR"` | `GOLDEN_HOUR` hoặc `TWENTY_FOUR_SEVEN` |
| `ivr_confirmation_required` | bool | `true` | ⚠️ **Chỉ nhận `true`.** Xem cảnh báo dưới bảng |
| `confirmation_window_started_at` | date-time | `"2026-08-12T03:00:00Z"` | Mốc `T0` — thời điểm Module 3 **mở cửa sổ xác nhận**, không phải lúc khách bấm đặt |
| `confirmation_window_expires_at` | date-time | `"2026-08-12T03:05:00Z"` | Hết cửa sổ. GH = `T0+5′`, 24/7 = `T0+15′` (theo `D-10`) |
| `attempt_policy_version` | string | `"gh-v1"` | IVR tra trong registry của mình. Version lạ → fail-closed |
| `max_customer_attempts` | int 1–10 | `2` | Số lần gọi khách tối đa |
| `attempt_offsets_seconds` | int[] 1–10 phần tử | `[0, 150]` | Giây tính từ `T0`. Phải có `0` cho lần gọi đầu |
| `phone_ref` | string | `"phref-0001"` | Tham chiếu số, **không phải số thật** |
| `phone_masked` | string | `"84xxxxx0001"` | Số đã che, chỉ để hiển thị màn admin |
| `dial_token` | string | `"dtok-0001"` | Token mờ. **Tuyệt đối không phải số E.164** |
| `dial_token_expires_at` | date-time | `"2026-08-12T03:05:00Z"` | ⚠️ Phải **≥ `confirmation_window_expires_at`**. Xem cảnh báo dưới |
| `privacy_safe_order_summary` | object | xem §3.5 | Nội dung đọc cho khách nghe |
| `call_restriction` | bool | `false` | `true` = khách đã từ chối nhận cuộc gọi → IVR chặn ngay |
| `eligibility_snapshot` | object | xem §3.4 | Bằng chứng Module 3 đã kiểm điều kiện |
| `evidence_ref` | string | `"evidence://sales/order-0001"` | Con trỏ evidence để đối soát sau |

> 🚨 **`ivr_confirmation_required` — rủi ro làm hỏng 100% task.**
> Field này khai `enum: [true]`. Thiếu, hoặc gửi `false` → **`422`, task bị từ chối**.
> `grep` toàn bộ tài liệu business không tìm thấy field này ở đâu (`OD-V1-14`). Nếu producer của Module 3 không set nó, **không một task nào chạy được** ngay ngày cắm thật. Xin xác nhận: ai set field này, set khi nào, có bao giờ `false` không.

> 🚨 **`dial_token_expires_at` phải ≥ `confirmation_window_expires_at`.**
> IVR chặn với `CONTACT_INVALID` nếu token hết hạn **trước** khi cửa sổ đóng — vì lần gọi thứ hai sẽ chết. Cách an toàn nhất: đặt **bằng nhau**, đúng như `seed/sales-target-v1.sample.json` đang làm.

### 3.4. Field OPTIONAL — nhưng 3 cái trong đó thực chất bắt buộc

Contract khai `additionalProperties: false`, nên **field lạ không nằm trong danh sách này sẽ bị từ chối**.

| Field | Kiểu | Có bắt buộc thật không |
| --- | --- | --- |
| `phone_validation_status` | string | 🔴 **THỰC CHẤT BẮT BUỘC.** Phải đúng chuỗi `"VALID"`. Thiếu hoặc giá trị khác (kể cả `"PASS"`) → chặn với `CONTACT_INVALID` |
| `risk_flags` | string[] | 🟡 Cần cho `OD-15`. Xem §6 |
| `correlation_id` | string | Nếu gửi thì phải trùng header |
| `created_at` | date-time | Thông tin |
| `order_code_short` | string | Mã rút gọn để **đọc cho khách**; nếu thiếu, IVR lấy từ `privacy_safe_order_summary` |
| `is_ivr_callable` | bool | Cờ tiện lợi; Module 3 vẫn là nguồn chân lý |
| `customer_ref` | string | Tham chiếu khách |
| `customer_trust_status` | string | Chỉ audit từ `OD-15`, không tham gia quyết định |
| `trusted_skip_allowed` | bool | ⚠️ Đây là **VETO**. Xem §6 |
| `call_script_template_id` / `call_script_version` | string | IVR tự chọn script nếu thiếu — **không** phải nghĩa vụ của Module 3 |
| `allowed_script_variables` | object | Biến lời thoại |
| `evidence_policy_version` / `privacy_policy_version` | string | Version policy |

### 3.5. `privacy_safe_order_summary` — nội dung đọc cho khách nghe

`additionalProperties: false`. Tất cả field dưới đều bắt buộc trừ `pronunciation_hints`.

| Field | Kiểu | Ràng buộc |
| --- | --- | --- |
| `customer_display_name` | string | 1–80. Ví dụ `"chị An"` |
| `order_code_short` | string | 1–40 |
| `items[]` | array | ≥ 1 phần tử. Mỗi item: `public_name` (1–160) + `quantity` (> 0), optional `unit_label` |
| `total_amount` | number | ≥ 0. **Là số, không phải chuỗi đã format** — IVR tự đọc thành lời |
| `currency` | string | Chỉ `"VND"` |
| `delivery_area_short` | string | 1–160 + regex chặn số nhà. Xem cảnh báo |
| `program_display_name` | string | 1–80. Ví dụ `"Giờ Vàng"` |
| `locale` | string | Chỉ `"vi-VN"` |
| `pronunciation_hints` | map | Optional, gợi ý phát âm |

> 🚨 **`delivery_area_short` — trách nhiệm normalize thuộc Module 3, không phải IVR.**
> Regex `^(?!\s*\d)(?!.*\d+\s*/\s*\d+).*$` chặn được số nhà đứng đầu (`"123 Lê Lợi"`) và dạng gạch chéo (`"12/3 Lê Lợi"`). Đơn vị hành chính có số vẫn hợp lệ (`"Quận 7"`, `"Phường 12"`).
> **Nhưng địa chỉ phố không có chữ số vẫn lọt regex** — IVR có thêm một detector ngữ nghĩa và sẽ **từ chối task** với `IVR_PII_POLICY_VIOLATION`. Module 3 phải gửi vùng giao rút gọn, không phải địa chỉ đầy đủ.
> Hợp lệ: `"Phường Bến Nghé, Quận Một"` · Bị từ chối: `"<số nhà> <tên đường>, <phường>, <quận>"` — tức là địa chỉ giao đầy đủ ở bất kỳ dạng nào.

> ⚠️ **`items[]` chưa có giới hạn trên.** Đơn 40 dòng → câu thoại dài vài phút, khách cúp máy trước khi tới phần bấm phím. Cần Module 3 + Product chốt: đọc tối đa bao nhiêu dòng, phần dư diễn đạt sao (`"và 12 sản phẩm khác"`), ai quyết thứ tự dòng.

### 3.6. Tồn kho / thu hồi — Module 3 **không** phải gửi gì cho IVR

Bản trước của tài liệu này yêu cầu Module 3 gửi `sellable_status[]` (ảnh chụp per-line SKU/batch lấy từ ops-core). **Yêu cầu đó đã bị gỡ bỏ.**

Owner Module 8 chốt: IVR **không đọc** tồn kho, thu hồi, sale-lock hay quality-hold. Field `sellable_status` đã bị xoá khỏi contract, khỏi database và khỏi console.

**Vì sao gỡ được:** đã có **hai** tầng kiểm, và tầng thứ hai mới là tầng quyết định.

| Tầng | Ai kiểm | Khi nào | Có chặn được đơn không bán được không? |
| --- | --- | --- | --- |
| `eligibility_snapshot.decision` | Module 3 | trước khi đẩy task | ✅ (nếu M3 đánh `BLOCKED`) |
| ~~`sellable_status[]`~~ | ~~IVR~~ | ~~trước khi quay số~~ | **đã gỡ** |
| **Revalidate với ops (`D-06`)** | **Module 3** | **lúc nhận callback** | ✅ **đây là tầng quyết định** |

Tầng thứ hai chỉ tránh được một **cuộc gọi thừa**, không tránh được một **xác nhận sai** — vì kể cả IVR có chặn hay không, `D-06` vẫn buộc Module 3 revalidate trước khi chuyển trạng thái.

> 🚨 **Đánh đổi Module 3 phải biết:** IVR nay có thể gọi khách về một đơn vừa bị recall hoặc sale-lock trong khoảng 5–15 phút của cửa sổ xác nhận. Khách bấm `1`, rồi Module 3 revalidate và trả `BLOCKED_BY_CORE` → đơn vẫn huỷ dù khách đã đồng ý. Tần suất thấp, nhưng **`D-06` là lưới an toàn duy nhất còn lại** — nếu Module 3 bỏ bước revalidate đó thì đơn không bán được sẽ được xác nhận.

### 3.7. `eligibility_snapshot` — bằng chứng Module 3 đã kiểm điều kiện

Trên wire nó là object mở (`additionalProperties: true`) vì shape chưa được owner duyệt (`OD-V1-03`). Shape IVR **thực sự đọc** nằm ở [`specs/api/evidence/eligibility-snapshot.v1.schema.json`](../specs/api/evidence/eligibility-snapshot.v1.schema.json):

| Key | Bắt buộc | Ý nghĩa |
| --- | --- | --- |
| `decision` | ✅ | Chỉ `"ELIGIBLE"` mới được gọi. `BLOCKED`/`NOT_ELIGIBLE`/`INELIGIBLE` → chặn. Giá trị lạ → giữ chờ review |
| `source_version` | ✅ | Version nguồn sinh snapshot. Thiếu/rỗng → giữ lại (không quy trách nhiệm được thì không gọi) |
| `captured_at` | ✅ | Phải nằm trong `[confirmation_window_started_at, thời điểm đánh giá]` |
| `source_available` | ○ | Mặc định `true`. `false` = Module 3 nói rõ "tôi không đọc được nguồn của mình" → giữ lại |
| `blockers[]` | ○ | Mã blocker privacy-safe. **Không rỗng → chặn, kể cả khi `decision=ELIGIBLE`** |
| `voice_restriction{}` | ○ | Provenance cho quyết định do-not-call |
| `trust.risk_evidence_available` | ○ | Cần cho `OD-15`. Xem §6 |

> ⚠️ **`order_state`: hợp đồng nói opaque, code lại hard-code.**
> OpenAPI khai `order_state` là chuỗi opaque do Order Core sở hữu. Nhưng IVR đang so sánh literal `"CONFIRMING"` và chặn thêm `"QUOTE"`, `"CART"`, `"DRAFT"`.
> **Hệ quả:** nếu Module 3 đổi tên state, tách `CONFIRMING` thành hai state, hoặc thêm state callable mới → IVR trả `ORDER_STATE_NOT_CALLABLE` cho **toàn bộ** task mới, im lặng, không alert nào bắt được (với IVR thì đó là hành vi đúng).
> **Cần chọn một:** (a) Module 3 công bố danh sách state callable như **dữ liệu**, IVR thôi hard-code; hoặc (b) Module 3 cam kết `"CONFIRMING"` là **hằng số hợp đồng**, đổi phải qua deprecation.

### 3.8. IVR trả về gì

**`200 OK`** kèm `decision` — đây là **kết quả nghiệp vụ**, không phải lỗi:

| `decision` | Nghĩa | Module 3 nên làm gì |
| --- | --- | --- |
| `TASK_ACCEPTED_CALL_JOB_CREATED` | Đã nhận, đã tạo job gọi | Chờ callback |
| `TASK_ACCEPTED_DRY_RUN_ONLY` | Nhận nhưng đang mode MOCK, không gọi thật | Chỉ có ở môi trường dev |
| `TASK_SKIPPED_TRUSTED_CUSTOMER` | **Khách cũ, đơn sạch → cố tình không gọi** (`OD-15`) | **Không chờ callback.** Module 3 tự tiếp tục workflow |
| `TASK_HELD_ADMIN_REVIEW` | Bằng chứng thiếu/khó hiểu, đang giữ chờ người xử lý | Không chờ callback tự động |
| `TASK_HELD_POLICY_MISSING` | Thiếu policy version | Sửa `attempt_policy_version` |

**`4xx`** kèm error envelope `{error:{code,message,details,correlationId}}`:

| HTTP | Khi nào |
| --- | --- |
| `400` | JSON hỏng / thiếu body |
| `401` / `403` | Auth sai / caller không được phép |
| `409` | Xung đột idempotency (cùng key, khác body) |
| `422` | Vi phạm schema: thiếu field, sai ma trận, `ivr_confirmation_required` không phải `true`, PII trong lời thoại, contact không hợp lệ |

> 🚨 **`200 + TASK_SKIPPED_TRUSTED_CUSTOMER` và `200 + TASK_HELD_*` không có callback theo sau.** Nếu Module 3 code theo kiểu "cứ gửi task rồi chờ callback" thì 3 nhánh này sẽ treo vô thời hạn. Phải đọc `decision`.

### 3.9. Payload mẫu — copy được, chạy được

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
  "confirmation_window_started_at": "2026-08-12T03:00:00Z",
  "confirmation_window_expires_at": "2026-08-12T03:05:00Z",
  "attempt_policy_version": "gh-v1",
  "max_customer_attempts": 2,
  "attempt_offsets_seconds": [0, 150],
  "customer_ref": "CUST-001",
  "risk_flags": ["NEW_CUSTOMER", "VERIFIED_ORDER_COUNT_0"],
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
    "source_version": "sales-eligibility-v1",
    "captured_at": "2026-08-12T03:00:30Z",
    "source_available": true,
    "blockers": [],
    "trust": { "risk_evidence_available": true }
  },
  "evidence_ref": "evidence://sales/order-0001/eligibility"
}
```

Bộ fixture đầy đủ hơn (nhiều kịch bản): [`seed/sales-target-v1.sample.json`](../seed/sales-target-v1.sample.json).

---

## 4. VIỆC B — Module 3 mở endpoint nhận kết quả

### 4.1. Endpoint Module 3 phải xây

```
POST {sales_base_url}/api/v1/internal/orders/{orderId}/ivr-result-callbacks
```

`{orderId}` trên URL **phải bằng** `order_id` trong body, lệch → từ chối.

| Header IVR gửi | Ghi chú |
| --- | --- |
| `Authorization: Bearer <token>` | Service JWT |
| `Idempotency-Key` | 8–200 ký tự |
| `X-Correlation-Id` | 1–200, chính là correlation của task |

> 🚨 **Endpoint này chưa tồn tại.** Module 3 hiện chỉ có `POST /api/v1/internal/ivr/golden-hour/callbacks` — riêng cho Giờ Vàng, hình dạng khác hẳn (ID là `int64` thay vì string, chỉ 4 giá trị kết quả thay vì 11, **không có field version nào**). Nghĩa là **chương trình 24/7 hiện không có lối trả kết quả nào cả.** Đây là ticket chặn cứng, không phải tối ưu. Chi tiết: [T-05](../docs/contracts/target-v1-closure-pack/T-05-callback-ack.md).

### 4.2. Body IVR gửi — 13 field, tất cả bắt buộc

`additionalProperties: false`.

| Field | Kiểu | Ý nghĩa |
| --- | --- | --- |
| `contract_version` | string | `"ivr-order-confirmation.v1"` |
| `callback_id` | string 1–120 | ID của lần callback này |
| `task_id` | string 1–120 | Task tương ứng |
| `order_id` | string 1–120 | Phải khớp `{orderId}` trên URL |
| `order_version_seen_by_ivr` | string 1–120 | **Chính là `order_version` Module 3 đã gửi lúc tạo task.** IVR không so sánh, không suy luận — Module 3 là bên duy nhất quyết định version đó còn tươi hay không |
| `result_type` | enum | 1 trong 11 giá trị, xem §4.3 |
| `result_reason` | string ≤500 | Optional, mô tả thêm |
| `is_counted_customer_attempt` | bool | `false` cho lỗi kỹ thuật. Xem cảnh báo §4.5 |
| `is_final_for_ivr` | bool | `true` = IVR đã xong với đơn này, không gọi nữa |
| `attempt_number` | int 1–10 | Lần gọi thứ mấy |
| `occurred_at` | date-time | Thời điểm xảy ra |
| `recommended_core_action` | enum | **Chỉ là gợi ý** (`D-02`), Module 3 không bắt buộc theo |
| `evidence_ref` | string 1–500 | Con trỏ evidence |
| `audit_ref` | string 1–500 | Con trỏ audit |

### 4.3. 11 giá trị `result_type`

| Giá trị | Nghĩa | `is_counted_customer_attempt` |
| --- | --- | --- |
| `IVR_CONFIRMED` | Khách bấm `1` | ✅ |
| `IVR_CUSTOMER_CANCELLED` | Khách bấm `0` | ✅ |
| `IVR_NO_ANSWER_ATTEMPT` | Không nghe máy, **còn lượt** | ✅ |
| `IVR_NO_ANSWER_FINAL` | Không nghe máy, **hết lượt** | ✅ |
| `IVR_WRONG_INPUT` | Bấm sai phím | ✅ |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | Hết cửa sổ xác nhận | — |
| `IVR_INVALID_PHONE_FINAL` | Số không tồn tại / sai số | ❌ (final riêng, không tính như no-answer) |
| `IVR_TECHNICAL_EXCEPTION` | Lỗi SIM/audio/mạng | ❌ |
| `IVR_CAPACITY_EXCEPTION` | Hết kênh gọi | ❌ |
| `IVR_OPERATIONAL_BLOCKED` | Blocker vận hành | ❌ |
| `IVR_POLICY_BLOCKED` | Policy chặn | ❌ |

7 giá trị `recommended_core_action`: `CORE_REVALIDATE_AND_CONFIRM_ORDER` · `CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST` · `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` · `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION` · `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` · `CORE_IGNORE_STALE_CALLBACK` · `CORE_BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT`.

### 4.4. ACK Module 3 phải trả — và IVR xử lý ra sao

| HTTP | `code` | IVR làm gì |
| --- | --- | --- |
| `200` | `ACCEPTED` | Xong |
| `200` | `DUPLICATE_ACCEPTED` | Xong, không gửi lại |
| `200` | `BLOCKED_BY_CORE` | Dừng, ghi evidence |
| `200` | `REVIEW_REQUIRED` | Đưa vào hàng đợi admin review |
| `409` | `REJECTED_STALE` | **Không retry.** Version đã trôi |
| `409` | `IDEMPOTENCY_CONFLICT` | **Không retry.** Ghi audit |
| `401`/`403`/`422` | — | Fail, không retry, đưa vào DLQ |
| `429` | — | Retry theo `Retry-After` |
| `500`/`503`/timeout | — | Retry có backoff |

> **`ACCEPTED` nghĩa là "Module 3 đã nhận tín hiệu để đưa vào luồng quyết định của mình"**, không có nghĩa đơn đã được xác nhận.

**Cần Module 3 định nghĩa rõ ranh giới:** IVR đang giả định cùng `Idempotency-Key` **cùng** body → `DUPLICATE_ACCEPTED`; cùng key **khác** body → `IDEMPOTENCY_CONFLICT`. Nếu Module 3 định nghĩa khác, outbox của IVR sẽ retry sai. Kèm theo: giữ key bao lâu.

### 4.5. Module 3 phải revalidate gì trước khi chuyển trạng thái

IVR gửi tín hiệu, **không** gửi lệnh. Trước khi đổi state, Module 3 kiểm lại:

1. Idempotency
2. `order_id` + `order_version_seen_by_ivr` còn tươi không → nếu không, trả `409 REJECTED_STALE`
3. `order_state` hiện tại có cho phép transition không
4. `program_code` / `payment_method_snapshot` còn khớp không
5. Blocker realtime: sale lock, recall, quality hold, tồn kho → nếu mới xuất hiện, trả `BLOCKED_BY_CORE` **dù khách đã bấm `1`**
6. Evidence còn hợp lệ không

> 🚨 **`DT-02` — Module 3 phải tôn trọng `is_counted_customer_attempt`, không đếm theo số callback nhận được.**
> IVR gửi `false` cho lỗi kỹ thuật. Nếu Module 3 đếm số lần gọi bằng cách đếm callback, số lần làm phiền khách thực tế sẽ **vượt policy đã duyệt** — vi phạm chính cái policy ở §7 mục T-09.

### 4.6. Payload mẫu

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

---

## 5. VIỆC C — `dial_token`: một mâu thuẫn số học cần Module 3 + Security chọn phương án

**Vấn đề:** task mang **đúng một** `dial_token` (scalar, không phải mảng, không có endpoint reissue). Nhưng cùng task đó phải quay **ít nhất 2 lần** (`max_customer_attempts ≥ 2`), cộng thêm n lần retry kỹ thuật không đoán trước được. Và 5 tài liệu ghi token là **one-use/attempt**.

Một token, nhiều lần quay. Không cộng được.

| # | Phương án | Đánh đổi |
| --- | --- | --- |
| **a** | `dial_tokens[]` per-attempt trong task | Module 3 phải biết trước số lần quay — kể cả retry kỹ thuật, mà cái đó không đoán được |
| **b** | Endpoint reissue/refresh | Thêm round-trip đồng bộ ngay trước mỗi lần quay; thêm một điểm chết |
| **c** | Token bundle (n token) | Vẫn phải bao được retry kỹ thuật |
| **d** | Token reusable có TTL + risk control | Bỏ tính chất one-use; cần threat model nói rõ chấp nhận rủi ro gì |

**Câu hỏi thứ hai, quan trọng hơn:** cái vault giữ mapping `dial_token → E.164` **chạy ở đâu, ai vận hành, ai audit**? Nếu câu trả lời là "trong process của IVR" thì nguyên tắc `D-05` bị vi phạm trên thực tế, dù code có che `ToString()` đi nữa.

Chi tiết: [T-04](../docs/contracts/target-v1-closure-pack/T-04-dial-token.md).

---

## 6. Không gọi khách cũ (`OD-15`) — chỉ cần 1 field

Owner Module 8 đã khóa `OD-15` ngày 2026-08-25: **IVR không gọi khách cũ.**

Vòng hỏi trước (`DC-06`) kết luận CRM chưa build `CustomerTrustResolver` nên không skip được ai. **`OD-15` bỏ hẳn ràng buộc đó** — IVR không cần trust score, vì khách mới **đã** được Module 3 báo qua `risk_flags` (`NEW_CUSTOMER`, `VERIFIED_ORDER_COUNT_0`).

**Việc cần làm: thêm 1 field vào `eligibility_snapshot`.**

```json
"trust": { "risk_evidence_available": true }
```

Nghĩa: *"Module 3 đã chạy đánh giá rủi ro cho đơn này, và `risk_flags` ở cấp task là danh sách đầy đủ."*

| Module 3 gửi | IVR làm |
| --- | --- |
| `risk_evidence_available=true` + `risk_flags` **rỗng** | **Bỏ qua** → `TASK_SKIPPED_TRUSTED_CUSTOMER`, không gọi |
| `risk_evidence_available=true` + có risk flag | **Gọi** |
| Không gửi / `false` | **Gọi** ← trạng thái hôm nay |

**Vì sao phải có field này thay vì chỉ nhìn `risk_flags` rỗng:** list rỗng có hai nguyên nhân không phân biệt được — *đã đánh giá, không có gì* và *chưa đánh giá bao giờ*. Nếu IVR đọc cả hai là "không rủi ro" thì đúng những đơn Module 3 chưa kịp đánh giá sẽ bị bỏ qua xác minh. Đó là đơn ảo lọt lưới.

### 6.1. 🚨 `trusted_skip_allowed` đổi nghĩa: opt-in → **veto**

| Trước (`D-12`) | Từ `OD-15` |
| --- | --- |
| `true` = bắt buộc phải có, thiếu thì không skip | `false` = **veto**, chặn skip riêng đơn đó |
| | absent / `true` = không veto |

Shape trên wire **không đổi** (vẫn `boolean` optional). Nhưng **nếu Module 3 đang gửi `false` như giá trị mặc định thì mọi đơn sẽ bị veto và không bao giờ skip.**

Muốn "không có ý kiến" thì **bỏ hẳn field**, đừng gửi `false`.

> **Đây là điểm dễ sai nhất trong toàn bộ `OD-15`.** Xin xác nhận Module 3 không gửi `trusted_skip_allowed=false` như default.

### 6.2. Danh sách `risk_flags` — xin xác nhận tên mã

`OD-15` dựa hoàn toàn vào tính đầy đủ của list này, nên nó thành contract thật chứ không còn là metadata tham khảo. Theo `phase-3.1/07 §7.1` và `D-13`:

`NEW_CUSTOMER` · `VERIFIED_ORDER_COUNT_0` · không có lịch sử mua thành công · `SUSPICIOUS_DUPLICATE` · `COD_FAIL_HISTORY` · địa chỉ giao rủi ro · phone pattern nghi ngờ · giá trị đơn bất thường · hành vi Giờ Vàng rủi ro · contact vừa mới đổi

Ngưỡng cụ thể thuộc Risk Policy bên Module 3 — IVR chỉ consume boolean, **không** tự định nghĩa "giá trị bất thường" bằng số tiền.

### 6.3. Cách đo lúc nào xong

Không cần Module 3 báo. Advisory `TRUST_RISK_EVIDENCE_UNAVAILABLE` hiện xuất hiện trên **mọi** task đủ điều kiện. Khi nó biến mất khỏi log eligibility của IVR, nghĩa là Module 3 đã bật field và skip đang chạy.

Tài liệu đầy đủ: [workflows/07-trusted-skip.md](../specs/workflows/07-trusted-skip.md) · sign-off riêng: [questions-to-module-3-od15-risk-evidence.md](../plan/ivr-orther/questions-to-module-3-od15-risk-evidence.md).

---

## 7. VIỆC D — Auth production

Hiện **không có gì**: không issuer, không sandbox credential, không quyết định mTLS. Không có credential nghĩa là **không chạy được một test tích hợp thật nào**.

Cần từ Security/Platform (không phải Module 3, nhưng Module 3 bị chặn theo):

- Issuer URL, JWKS URL, thuật toán ký, chu kỳ xoay khoá
- Audience đặt là gì cho IVR, TTL token
- Scope set — đề xuất: `ivr.task.write` (Module 3 → `POST /tasks`), `ivr.internal.write`, `ivr.admin.read`, `ivr.admin.write`
- **Sandbox credential** + hướng dẫn lấy token
- Quyết định mTLS: có/không, ai cấp cert, xoay ra sao
- Ngày tắt `X-Internal-Token` (hiện chỉ để tương thích cũ)

Lưu ý kỹ thuật: `bearerAuth` hiện là `type: http, scheme: bearer` nên **không mang được scope**. Muốn có scope phải chuyển sang `oauth2/clientCredentials` — **đây là thay đổi contract, không phải cấu hình**.

---

## 8. Những gì IVR **không** làm — để Module 3 không kỳ vọng nhầm

| IVR không | Vì sao |
| --- | --- |
| Ghi/đổi trạng thái đơn | `D-02` — Module 3 sở hữu chân lý về đơn |
| Tự tạo order, tự sinh `order_code` | Ngoài scope |
| Xác nhận thanh toán, xác nhận doanh thu | `IVR_CONFIRMED` ≠ `PAID` ≠ Verified Revenue |
| Huỷ đơn | Kể cả khi khách bấm `0` — IVR chỉ báo tín hiệu |
| Đọc tồn kho / thu hồi / sale-lock | Đã gỡ hoàn toàn (§3.6). Module 3 revalidate với ops lúc callback (`D-06`) |
| Truy vấn/polling đơn hàng | Xem §2 |
| Gửi SMS / notification | `TV1-07` — tắt trong V1 |
| Ghi note vào CRM | `D-14` — chỉ audit nội bộ |
| Gọi cho Quote, Cart, Order Draft | Chỉ Official Order |
| Gọi khi thiếu bằng chứng | Fail-closed, luôn nghiêng về "không gọi" |

---

## 9. Thứ tự ưu tiên — cái gì chặn cái gì

| Ưu tiên | Việc | Chặn cái gì nếu chưa xong |
| --- | --- | --- |
| **1** | §3.2 Chốt ma trận program/payment + định nghĩa `ivr_confirmation_required` | **Chặn tất cả.** Sai ma trận = 100% task bị từ chối, im lặng |
| **2** | §4.1 Xây endpoint callback generic | Chương trình 24/7 **không có lối trả kết quả** |
| **3** | §7 Auth + sandbox credential | Không chạy được test tích hợp thật nào |
| **4** | §3.7 Chốt shape `eligibility_snapshot` | IVR không fail-closed đúng trên thứ nó không hiểu |
| **5** | §5 Chọn phương án `dial_token` | Không quay số thật được |
| **6** | §3.5 Duyệt whitelist lời thoại (cần Privacy/Legal) | **Rủi ro pháp lý** — không rollback được sau khi đã gọi |
| **7** | §6 Thêm `trust.risk_evidence_available` | Chỉ mất tối ưu — vẫn gọi tất cả, an toàn |

---

## 10. Checklist Module 3 trả lời

### Chặn cứng

- [ ] **Ma trận đã ký**: `program_code × payment_method_snapshot × order_state → callable`. Trả lời dứt khoát: `GOLDEN_HOUR + COD` có callable không? `GOLDEN_HOUR + ONLINE` có thuộc scope IVR V1 không?
- [ ] **`ivr_confirmation_required`**: nguồn business ở đâu, ai set, set khi nào, có bao giờ `false` không?
- [ ] **`order_state`**: cam kết `"CONFIRMING"` là hằng số hợp đồng, **hoặc** công bố danh sách state callable như dữ liệu?
- [ ] **OpenAPI endpoint callback generic** phủ cả hai chương trình, kèm ACK taxonomy
- [ ] **Ranh giới idempotency**: `DUPLICATE_ACCEPTED` vs `IDEMPOTENCY_CONFLICT`, giữ key bao lâu?
- [ ] **`order_version` bump khi nào?** Mỗi lần sửa đơn, hay chỉ khi sửa field ảnh hưởng xác nhận? *(Bump theo mọi thay đổi kể cả note nội bộ → tỉ lệ `REJECTED_STALE` cao giả tạo, kết quả khách đã bấm phím bị vứt)*

### Cần sớm

- [ ] **Schema `eligibility_snapshot`** + ví dụ pass/block/stale/source-unavailable
- [ ] **Phương án `dial_token`**: chọn (a)/(b)/(c)/(d) ở §5, kèm chữ ký Security + sơ đồ trust boundary
- [ ] **Giới hạn `items[]`**: đọc tối đa bao nhiêu dòng, phần dư diễn đạt sao?
- [ ] **Quy tắc normalize `delivery_area_short`**
- [ ] **`attempt_policy_version` production**: bảng đầy đủ mỗi program → số attempt / offsets / window. *(Hiện `D-10` và tài liệu phase-8 lệch nhau: GH window 5′ vs 10′, 24/7 là 2 hay 3 lần gọi)*
- [ ] **Hành vi timeout worker**: sau `NO_ANSWER_FINAL`, đơn nằm ở `CONFIRMING` bao lâu, ai chuyển đi, chuyển sang đâu?
- [ ] **4 tình huống race** — ai thắng: ① khách bấm phím đúng lúc window hết hạn ② Module 3 huỷ đơn trong lúc IVR đang gọi ③ khách bấm xác nhận nhưng `order_version` đã bump vì lý do khác ④ IVR gửi `NO_ANSWER_FINAL`, Module 3 chưa expire, khách gọi lại tổng đài
- [ ] **Xác nhận Module 3 revalidate tồn kho/thu hồi với ops lúc nhận callback** (`D-06`) — sau khi gỡ `sellable_status[]` đây là lưới an toàn duy nhất
- [ ] **Xác nhận tôn trọng `is_counted_customer_attempt`**, không đếm attempt bằng số callback

### `OD-15`

- [ ] **Sẽ gửi `trust.risk_evidence_available`?** Dự kiến release: ______
- [ ] **Xác nhận KHÔNG gửi `trusted_skip_allowed=false` như default**
- [ ] **Xác nhận danh sách + tên mã `risk_flags`** ở §6.2

### Vận hành

- [ ] **Auth profile đã ký** + sandbox credential
- [ ] **Vị trí công bố OpenAPI của Module 3** + cách IVR fetch theo version
- [ ] **Cam kết deprecation hai chiều**: báo trước bao lâu, qua kênh nào?

---

## Ô ký

| Vai trò | Tên | Ngày |
| --- | --- | --- |
| Owner Module 3 — `ginsengfood-business-platform` | ____________ | ______ |
| Security/Platform (mục §7) | ____________ | ______ |
| Privacy/Legal (mục §3.5) | ____________ | ______ |

**Ghi chú chung:** ______________________________________________
