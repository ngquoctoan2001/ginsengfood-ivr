# Cần gì từ Module 3 — và từ các bên ngoài khác

> **HISTORICAL_EVIDENCE / SUPERSEDED — 2026-08-27:** Báo cáo này khóa tại baseline ngày
> 2026-08-26. Yêu cầu Module 3 gửi trust/risk-evidence để IVR tự skip đã bị `OD-18`/`W-0123`
> thay thế. Module 3 quyết định task cần gọi; authority bàn giao hiện hành là
> [`IR-06`](../../../integration-requirements/06-module-3-api-handover.md).

**Ngày:** 2026-08-26 · **Baseline:** `main@bdde72c`
**Tài liệu bàn giao đầy đủ (có payload copy-paste được + ô ký):**
[`integration-requirements/06-module-3-api-handover.md`](../../../integration-requirements/06-module-3-api-handover.md)

> **Về tên gọi.** Module 3 là **một** module — repository `ginsengfood-business-platform` — gồm cả
> Commerce/Order Core, Sales Extensions và CRM/Customer Identity. Tài liệu cũ trong repo IVR tách
> thành "Module 3" và "Module 3.1" (mã `D-*` vs `DC-*`); đó là cách đánh số của **vòng hỏi
> 2026-07-02**, không phải hai đội.

---

## 1. Chỗ dễ hiểu nhầm nhất — hướng gọi

**IVR KHÔNG lấy đơn từ Module 3.** Không polling, không `GET /orders`, IVR không truy vấn gì cả.
Luồng thật là **hai chiều push**, nên **Module 3 phải làm 2 việc, không phải 1**:

```
                    ┌─────────────────────────────────────────┐
                    │  (A) Module 3 GỌI API của IVR           │
   ┌──────────┐     │  POST {ivr}/v1/ivr/order-confirmation/  │     ┌──────────┐
   │ Module 3 │─────┤        tasks                            ├────>│   IVR    │
   │Order Core│     │  → "đơn này cần gọi xác nhận, đây là    │     │  (.NET)  │
   └──────────┘     │     toàn bộ dữ liệu"                    │     └──────────┘
        ▲           └─────────────────────────────────────────┘          │
        │                                                          ┌──────▼──────┐
        │                                                          │ gọi khách,  │
        │                                                          │ nhận phím   │
        │           ┌─────────────────────────────────────────┐   └──────┬──────┘
        │           │  (B) IVR GỌI API của Module 3           │          │
        └───────────┤  POST {sales}/api/v1/internal/orders/   │<─────────┘
                    │        {orderId}/ivr-result-callbacks   │
                    │  → "khách bấm 1", Module 3 tự quyết định│
                    └─────────────────────────────────────────┘
```

| Nếu chỉ làm | Hậu quả |
| --- | --- |
| A mà không làm B | IVR gọi được khách nhưng **không có lối trả kết quả** — đơn treo mãi ở `CONFIRMING` |
| B mà không làm A | **không đơn nào được gọi** |

---

## 2. Bốn hạng mục Module 3 phải giao

| # | Cần gì | Ai làm | Ưu tiên | Trạng thái hiện tại |
| --- | --- | --- | --- | --- |
| **A** | Module 3 **gọi API của IVR** để đẩy task xác nhận đơn | Order Core | **P0** | Chưa có producer cho 24/7 COD |
| **B** | Module 3 **mở 1 endpoint** để IVR trả kết quả cuộc gọi về | Sales API/Core | **P0** | Endpoint generic **chưa tồn tại** |
| **C** | Cơ chế `dial_token`: cấp, resolve, TTL, dùng mấy lần | Sales + Security | **P0** cho gọi thật | Chưa chốt, có 4 phương án |
| **D** | Service auth production: issuer, JWKS, audience, scope, sandbox credential | Security/Platform | **P0** | **Chưa có gì** |

Cộng thêm **1 yêu cầu nhỏ** đang mở: `OD-15` — chỉ tốn **đúng một field** (xem §6).

---

## 3. Việc A — Module 3 đẩy task sang IVR

### 3.1 · Endpoint và header

```
POST {ivr_base_url}/v1/ivr/order-confirmation/tasks
Content-Type: application/json
```

| Header | Bắt buộc | Ghi chú |
| --- | --- | --- |
| `Authorization: Bearer <token>` | ✅ | Service JWT. Dev dùng mock JWT; production chờ **Việc D** |
| `Idempotency-Key` | ✅ | 8–200 ký tự. Cùng key + cùng body → trả lại kết quả cũ, không tạo job trùng |
| `X-Correlation-Id` | ✅ | 1–200 ký tự, xuyên suốt toàn luồng |
| `X-Source-System` | ✅ | Định danh hệ gửi |

Nếu body cũng có `correlation_id` thì nó **phải trùng** header, lệch → `422`.

### 3.2 · 🚨 Ma trận `program × payment` — điểm nghi ngờ số 1

| `program_code` | `payment_method_snapshot` | Kết quả |
| --- | --- | --- |
| `GOLDEN_HOUR` | `ONLINE` | ✅ nhận |
| `TWENTY_FOUR_SEVEN` | `COD` | ✅ nhận |
| `GOLDEN_HOUR` | `COD` | ❌ `422` |
| `TWENTY_FOUR_SEVEN` | `ONLINE` | ❌ `422` |

**Mâu thuẫn cần Module 3 giải quyết dứt điểm:** tài liệu business mà IVR đọc được (`DS-01`,
đọc từ source Sales platform) nói **ngược lại** — IVR-callable là **COD-only**, và
`GOLDEN_HOUR + ONLINE` **không** callable.

IVR đang enforce ma trận trên ở **4 tầng độc lập**: OpenAPI · intake · eligibility ·
`CHECK` constraint của DB. **Nếu ma trận business khác, cả 4 tầng phải sửa cùng lúc.**

> **Sai ma trận = 100% task bị từ chối, im lặng.** Không alert nào bắt được, vì với IVR đó là
> hành vi đúng. Đây là lý do hạng mục này đứng đầu danh sách ưu tiên.

Chi tiết: [T-01](../../contracts/target-v1-closure-pack/T-01-program-matrix.md).

### 3.3 · 22 field bắt buộc

Thiếu bất kỳ field nào → `422`, không tạo job.

| Field | Kiểu | Ví dụ | Bẫy |
| --- | --- | --- | --- |
| `contract_version` | string | `"ivr-order-confirmation.v1"` | hằng số, chỉ nhận đúng giá trị này |
| `task_id` | string | `"TASK-0001"` | Module 3 sinh |
| `order_id` | string | `"ORDER-0001"` | phải trùng `{orderId}` trên URL callback sau này |
| `order_code` | string | `"GF-2026-0001"` | **không bao giờ được đọc cho khách nghe** |
| `order_version` | string | `"17"` | ảnh chụp chống race — IVR giữ nguyên si, trả lại y hệt |
| `order_state` | string | `"CONFIRMING"` | ⚠️ xem §3.7 |
| `payment_method_snapshot` | string | `"ONLINE"` | theo ma trận §3.2 |
| `program_code` | string | `"GOLDEN_HOUR"` | theo ma trận §3.2 |
| `ivr_confirmation_required` | bool | `true` | ⚠️ **chỉ nhận `true`** — xem cảnh báo |
| `confirmation_window_started_at` | date-time | `"2026-08-12T03:00:00Z"` | `T0` = lúc Module 3 **mở cửa sổ**, không phải lúc khách bấm đặt |
| `confirmation_window_expires_at` | date-time | `"2026-08-12T03:05:00Z"` | GH = `T0+5′`, 24/7 = `T0+15′` (theo `D-10`) |
| `attempt_policy_version` | string | `"gh-v1"` | version lạ → fail-closed |
| `max_customer_attempts` | int 1–10 | `2` | |
| `attempt_offsets_seconds` | int[] 1–10 | `[0, 150]` | phải có `0` cho lần gọi đầu |
| `phone_ref` | string | `"phref-0001"` | tham chiếu, **không phải số thật** |
| `phone_masked` | string | `"84xxxxx0001"` | chỉ để hiển thị màn admin |
| `dial_token` | string | `"dtok-0001"` | **tuyệt đối không phải số E.164** |
| `dial_token_expires_at` | date-time | `"2026-08-12T03:05:00Z"` | ⚠️ phải **≥ `confirmation_window_expires_at`** |
| `privacy_safe_order_summary` | object | xem §3.5 | nội dung đọc cho khách nghe |
| `call_restriction` | bool | `false` | `true` = khách đã từ chối nhận cuộc gọi → chặn ngay |
| `eligibility_snapshot` | object | xem §3.7 | bằng chứng Module 3 đã kiểm điều kiện |
| `evidence_ref` | string | `"evidence://sales/order-0001"` | con trỏ evidence |

> 🚨 **`ivr_confirmation_required` — rủi ro làm hỏng 100% task.**
> Field khai `enum: [true]`. Thiếu hoặc gửi `false` → **`422`, task bị từ chối**.
> `grep` toàn bộ tài liệu business **không tìm thấy field này ở đâu** (`OD-V1-14`).
> Nếu producer của Module 3 không set nó, **không một task nào chạy được** ngay ngày cắm thật.
> Xin xác nhận: ai set, set khi nào, có bao giờ `false` không.

> 🚨 **`dial_token_expires_at` phải ≥ `confirmation_window_expires_at`.**
> IVR chặn với `CONTACT_INVALID` nếu token hết hạn **trước** khi cửa sổ đóng — vì lần gọi thứ hai
> sẽ chết. An toàn nhất: đặt **bằng nhau**.

### 3.4 · Ba field "optional" thực chất bắt buộc

Contract khai `additionalProperties: false` — field lạ sẽ bị từ chối.

| Field | Có bắt buộc thật không |
| --- | --- |
| `phone_validation_status` | 🔴 **THỰC CHẤT BẮT BUỘC** — phải đúng chuỗi `"VALID"`. Thiếu hoặc giá trị khác (kể cả `"PASS"`) → `CONTACT_INVALID` |
| ~~`sellable_status[]`~~ | ⛔ **ĐÃ GỠ KHỎI CONTRACT `2026-08-26`** — gửi nó nay làm task bị `400 IVR_MALFORMED_REQUEST`. Xem §3.6 |
| `risk_flags` | 🟡 cần cho `OD-15` — xem §6 |
| `trusted_skip_allowed` | ⚠️ đây là **VETO** — xem §6.1 |

### 3.5 · `privacy_safe_order_summary` — nội dung đọc cho khách nghe

`additionalProperties: false`. Tất cả bắt buộc trừ `pronunciation_hints`.

| Field | Ràng buộc |
| --- | --- |
| `customer_display_name` | 1–80, ví dụ `"chị An"` |
| `order_code_short` | 1–40 |
| `items[]` | ≥ 1 phần tử; mỗi item `public_name` (1–160) + `quantity` (> 0), optional `unit_label` |
| `total_amount` | ≥ 0, **là số, không phải chuỗi đã format** — IVR tự đọc thành lời |
| `currency` | chỉ `"VND"` |
| `delivery_area_short` | 1–160 + regex chặn địa chỉ nhà |
| `program_display_name` | 1–80, ví dụ `"Giờ Vàng"` |
| `locale` | chỉ `"vi-VN"` |

> 🚨 **`delivery_area_short` — trách nhiệm normalize thuộc Module 3, không phải IVR.**
> Regex `^(?!\s*\d)(?!.*\d+\s*/\s*\d+).*$` chặn địa chỉ có chữ số đứng đầu (`"123 Lê Lợi"`) và dạng gạch chéo
> (`"12/3 Lê Lợi"`). Đơn vị hành chính có số vẫn hợp lệ (`"Quận 7"`, `"Phường 12"`).
> **Nhưng địa chỉ phố không có chữ số vẫn lọt regex** — IVR có thêm detector ngữ nghĩa và sẽ
> **từ chối task** với `IVR_PII_POLICY_VIOLATION`.
> Hợp lệ: `"Phường Bến Nghé, Quận Một"` · Bị từ chối: địa chỉ giao đầy đủ ở **bất kỳ** dạng nào.

> ⚠️ **`items[]` chưa có giới hạn trên.** Đơn 40 dòng → câu thoại dài vài phút, khách cúp máy trước
> khi tới phần bấm phím. Cần Module 3 + Product chốt: đọc tối đa bao nhiêu dòng, phần dư diễn đạt
> sao (`"và 12 sản phẩm khác"`), ai quyết thứ tự dòng.

### 3.6 · Tồn kho / thu hồi — Module 3 **không** phải gửi gì

> ⚠️ **Sửa `2026-08-26`.** Bản đầu của báo cáo này ghi `sellable_status[]` là "🔴 THỰC CHẤT BẮT
> BUỘC" và đặc tả cả shape. **Sai.** Owner Module 8 đã gỡ field đó khỏi contract, database và
> console cùng ngày (commit `8cd106c`, [IR-06 §3.6](../../../integration-requirements/06-module-3-api-handover.md)).
> Vì task schema là `additionalProperties: false`, Module 3 mà code theo bản cũ sẽ bị
> **`400 IVR_MALFORMED_REQUEST` trên 100% task** — đúng lỗi đã xảy ra thật với script lab hôm nay.

IVR **không đọc** tồn kho, thu hồi, sale-lock hay quality-hold. Đã có **hai** tầng kiểm, và tầng
thứ hai mới là tầng quyết định:

| Tầng | Ai kiểm | Khi nào | Chặn được đơn không bán được? |
| --- | --- | --- | --- |
| `eligibility_snapshot.decision` | Module 3 | trước khi đẩy task | ✅ nếu M3 đánh `BLOCKED` |
| ~~`sellable_status[]`~~ | ~~IVR~~ | ~~trước khi quay số~~ | **đã gỡ** |
| **Revalidate với ops (`D-06`)** | **Module 3** | **lúc nhận callback** | ✅ **tầng quyết định** |

> 🚨 **Đánh đổi Module 3 phải biết:** IVR nay có thể gọi khách về một đơn vừa bị recall hoặc
> sale-lock trong 5–15 phút của cửa sổ xác nhận. Khách bấm `1`, Module 3 revalidate rồi trả
> `BLOCKED_BY_CORE` → đơn vẫn huỷ dù khách đã đồng ý. Tần suất thấp, nhưng **`D-06` là lưới an
> toàn duy nhất còn lại**.

### 3.7 · `eligibility_snapshot` và vấn đề `order_state`

Shape IVR thực sự đọc:
[`specs/api/evidence/eligibility-snapshot.v1.schema.json`](../../../specs/api/evidence/eligibility-snapshot.v1.schema.json)

| Key | Bắt buộc | Ý nghĩa |
| --- | --- | --- |
| `decision` | ✅ | chỉ `"ELIGIBLE"` mới được gọi; giá trị lạ → giữ chờ review |
| `source_version` | ✅ | thiếu/rỗng → giữ lại (không quy trách nhiệm được thì không gọi) |
| `captured_at` | ✅ | phải nằm trong `[confirmation_window_started_at, thời điểm đánh giá]` — sớm hơn là mô tả trạng thái khác, muộn hơn là lỗi đồng hồ |
| `source_available` | ○ | mặc định `true`; `false` = "tôi không đọc được nguồn của mình" → giữ lại |
| `blockers[]` | ○ | **không rỗng → chặn, kể cả khi `decision=ELIGIBLE`** |
| `voice_restriction{}` | ○ | provenance cho quyết định do-not-call |
| `trust.risk_evidence_available` | ○ | cần cho `OD-15` — xem §6 |

> ⚠️ **`order_state`: hợp đồng nói opaque, code lại hard-code.**
> OpenAPI khai `order_state` là chuỗi opaque do Order Core sở hữu. Nhưng IVR đang so sánh literal
> `"CONFIRMING"` và chặn thêm `"QUOTE"`, `"CART"`, `"DRAFT"`.
> **Hệ quả:** nếu Module 3 đổi tên state, tách `CONFIRMING` thành hai state, hoặc thêm state
> callable mới → IVR trả `ORDER_STATE_NOT_CALLABLE` cho **toàn bộ** task mới, **im lặng**.
> **Cần chọn một:** (a) Module 3 công bố danh sách state callable **như dữ liệu**, IVR thôi
> hard-code; hoặc (b) Module 3 cam kết `"CONFIRMING"` là **hằng số hợp đồng**, đổi phải qua
> deprecation.

### 3.8 · IVR trả về gì

**`200 OK`** kèm `decision` — đây là **kết quả nghiệp vụ**, không phải lỗi:

| `decision` | Module 3 nên làm gì |
| --- | --- |
| `TASK_ACCEPTED_CALL_JOB_CREATED` | chờ callback |
| `TASK_ACCEPTED_DRY_RUN_ONLY` | chỉ có ở môi trường dev (mode MOCK) |
| `TASK_SKIPPED_TRUSTED_CUSTOMER` | **không chờ callback** — tự tiếp tục workflow |
| `TASK_HELD_ADMIN_REVIEW` | **không chờ callback** tự động |
| `TASK_HELD_POLICY_MISSING` | sửa `attempt_policy_version` |

**`4xx`** kèm error envelope `{error:{code,message,details,correlationId}}`:
`400` JSON hỏng · `401`/`403` auth · `409` xung đột idempotency · `422` vi phạm schema.

> 🚨 **`TASK_SKIPPED_*` và `TASK_HELD_*` KHÔNG có callback theo sau.** Nếu Module 3 code theo kiểu
> "cứ gửi task rồi chờ callback" thì 3 nhánh này sẽ **treo vô thời hạn**. Phải đọc `decision`.

---

## 4. Việc B — Module 3 mở endpoint nhận kết quả

### 4.1 · Endpoint phải xây

```
POST {sales_base_url}/api/v1/internal/orders/{orderId}/ivr-result-callbacks
```

`{orderId}` trên URL **phải bằng** `order_id` trong body.

> 🚨 **Endpoint này chưa tồn tại.** Module 3 hiện chỉ có
> `POST /api/v1/internal/ivr/golden-hour/callbacks` — riêng Giờ Vàng, hình dạng khác hẳn:
> ID là `int64` thay vì string, chỉ **4 giá trị kết quả thay vì 11**, và **không có field version
> nào**.
> Nghĩa là: **chương trình 24/7 hiện không có lối trả kết quả nào cả.**
> Đây là ticket chặn cứng, không phải tối ưu. Chi tiết:
> [T-05](../../contracts/target-v1-closure-pack/T-05-callback-ack.md).

### 4.2 · 13 field IVR gửi, tất cả bắt buộc

`contract_version` · `callback_id` · `task_id` · `order_id` · `order_version_seen_by_ivr` ·
`result_type` · `result_reason` · `is_counted_customer_attempt` · `is_final_for_ivr` ·
`attempt_number` · `occurred_at` · `recommended_core_action` · `evidence_ref` · `audit_ref`

`order_version_seen_by_ivr` chính là `order_version` Module 3 đã gửi lúc tạo task.
**IVR không so sánh, không suy luận** — Module 3 là bên duy nhất quyết định version đó còn tươi
hay không.

### 4.3 · 11 giá trị `result_type` — và cột quan trọng nhất

| Giá trị | Nghĩa | `is_counted_customer_attempt` |
| --- | --- | --- |
| `IVR_CONFIRMED` | khách bấm `1` | ✅ |
| `IVR_CUSTOMER_CANCELLED` | khách bấm `0` | ✅ |
| `IVR_NO_ANSWER_ATTEMPT` | không nghe máy, còn lượt | ✅ |
| `IVR_NO_ANSWER_FINAL` | không nghe máy, hết lượt | ✅ |
| `IVR_WRONG_INPUT` | bấm sai phím | ✅ |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | hết cửa sổ | — |
| `IVR_INVALID_PHONE_FINAL` | số không tồn tại / sai số | ❌ |
| `IVR_TECHNICAL_EXCEPTION` | lỗi SIM/audio/mạng | ❌ |
| `IVR_CAPACITY_EXCEPTION` | hết kênh gọi | ❌ |
| `IVR_OPERATIONAL_BLOCKED` | blocker vận hành | ❌ |
| `IVR_POLICY_BLOCKED` | policy chặn | ❌ |

**7 giá trị `recommended_core_action`** — **chỉ là gợi ý** (`D-02`), Module 3 không bắt buộc theo:
`CORE_REVALIDATE_AND_CONFIRM_ORDER` · `CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST` ·
`CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` · `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION` ·
`CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` · `CORE_IGNORE_STALE_CALLBACK` ·
`CORE_BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT`

### 4.4 · ACK Module 3 phải trả

| HTTP | `code` | IVR làm gì |
| --- | --- | --- |
| `200` | `ACCEPTED` | xong |
| `200` | `DUPLICATE_ACCEPTED` | xong, không gửi lại |
| `200` | `BLOCKED_BY_CORE` | dừng, ghi evidence |
| `200` | `REVIEW_REQUIRED` | đưa vào hàng đợi admin review |
| `409` | `REJECTED_STALE` | **không retry** — version đã trôi |
| `409` | `IDEMPOTENCY_CONFLICT` | **không retry** — ghi audit |
| `401`/`403`/`422` | — | fail, không retry, đưa vào DLQ |
| `429` | — | retry theo `Retry-After` |
| `500`/`503`/timeout | — | retry có backoff |

> `ACCEPTED` nghĩa là **"Module 3 đã nhận tín hiệu để đưa vào luồng quyết định của mình"**,
> **không** có nghĩa đơn đã được xác nhận.

**Cần Module 3 định nghĩa rõ ranh giới idempotency:** IVR đang giả định cùng key **cùng** body →
`DUPLICATE_ACCEPTED`; cùng key **khác** body → `IDEMPOTENCY_CONFLICT`. Nếu Module 3 định nghĩa
khác, outbox của IVR sẽ retry sai. Kèm theo: **giữ key bao lâu**.

### 4.5 · Module 3 phải revalidate 6 thứ trước khi chuyển trạng thái

IVR gửi **tín hiệu**, không gửi **lệnh**:

1. Idempotency
2. `order_id` + `order_version_seen_by_ivr` còn tươi không → nếu không, trả `409 REJECTED_STALE`
3. `order_state` hiện tại có cho phép transition không
4. `program_code` / `payment_method_snapshot` còn khớp không
5. **Blocker realtime**: sale lock, recall, quality hold, tồn kho → nếu mới xuất hiện, trả
   `BLOCKED_BY_CORE` **dù khách đã bấm `1`**
6. Evidence còn hợp lệ không

> 🚨 **`DT-02` — Module 3 phải tôn trọng `is_counted_customer_attempt`, không đếm theo số callback
> nhận được.** IVR gửi `false` cho lỗi kỹ thuật. Nếu Module 3 đếm số lần gọi bằng cách đếm
> callback, **số lần làm phiền khách thực tế sẽ vượt policy đã duyệt**.

---

## 5. Việc C — `dial_token`: một mâu thuẫn số học

**Vấn đề:** task mang **đúng một** `dial_token` (scalar, không phải mảng, không có endpoint
reissue). Nhưng cùng task đó phải quay **ít nhất 2 lần** (`max_customer_attempts ≥ 2`), cộng thêm
n lần retry kỹ thuật không đoán trước được. Và **5 tài liệu ghi token là one-use/attempt**.

Một token, nhiều lần quay. **Không cộng được.**

| # | Phương án | Đánh đổi |
| --- | --- | --- |
| **a** | `dial_tokens[]` per-attempt trong task | Module 3 phải biết trước số lần quay — kể cả retry kỹ thuật, mà cái đó không đoán được |
| **b** | Endpoint reissue/refresh | thêm round-trip đồng bộ ngay trước mỗi lần quay; thêm một điểm chết |
| **c** | Token bundle (n token) | vẫn phải bao được retry kỹ thuật |
| **d** | Token reusable có TTL + risk control | bỏ tính chất one-use; cần threat model nói rõ chấp nhận rủi ro gì |

**Câu hỏi thứ hai, quan trọng hơn:** cái vault giữ mapping `dial_token → E.164` **chạy ở đâu,
ai vận hành, ai audit**? Nếu câu trả lời là "trong process của IVR" thì nguyên tắc `D-05`
bị vi phạm trên thực tế, dù code có che `ToString()` đi nữa. (`OD-V1-18` ghi nhận mâu thuẫn:
`specs/api/04` nói adapter **không** nhận số; `P2-4` đặt resolver trong IVR; gateway GSM/SIP
thương mại thì quay số E.164.)

Chi tiết: [T-04](../../contracts/target-v1-closure-pack/T-04-dial-token.md) +
[R-01 §4](../../contracts/telephony-procurement-pack/R-01-vendor-requirements.md).

---

## 6. `OD-15` — không gọi khách cũ: chỉ cần **1 field**

Owner Module 8 đã khoá `OD-15` ngày 2026-08-25: **IVR không gọi khách cũ.**

Vòng hỏi trước (`DC-06`) kết luận CRM chưa build `CustomerTrustResolver` nên không skip được ai.
**`OD-15` bỏ hẳn ràng buộc đó** — IVR không cần trust score, vì khách mới **đã** được Module 3 báo
qua `risk_flags` (`NEW_CUSTOMER`, `VERIFIED_ORDER_COUNT_0`).

**Việc cần làm: thêm 1 field vào `eligibility_snapshot`.**

```json
"trust": { "risk_evidence_available": true }
```

Nghĩa: *"Module 3 đã chạy đánh giá rủi ro cho đơn này, và `risk_flags` ở cấp task là danh sách
đầy đủ."*

| Module 3 gửi | IVR làm |
| --- | --- |
| `risk_evidence_available=true` + `risk_flags` **rỗng** | **bỏ qua** → `TASK_SKIPPED_TRUSTED_CUSTOMER`, không gọi |
| `risk_evidence_available=true` + có risk flag | **gọi** |
| không gửi / `false` | **gọi** ← trạng thái hôm nay |

**Vì sao phải có field này thay vì chỉ nhìn `risk_flags` rỗng:** list rỗng có **hai nguyên nhân
không phân biệt được** — *đã đánh giá, không có gì* và *chưa đánh giá bao giờ*. Nếu IVR đọc cả hai
là "không rủi ro" thì đúng những đơn Module 3 chưa kịp đánh giá sẽ bị bỏ qua xác minh. **Đó là đơn
ảo lọt lưới** — đúng thứ module này tồn tại để chặn.

### 6.1 · 🚨 `trusted_skip_allowed` đổi nghĩa: opt-in → **veto**

| Trước (`D-12`) | Từ `OD-15` |
| --- | --- |
| `true` = bắt buộc phải có, thiếu thì không skip | `false` = **veto**, chặn skip riêng đơn đó |
| | absent / `true` = không veto |

Shape trên wire **không đổi** (vẫn `boolean` optional). Nhưng **nếu Module 3 đang gửi `false` như
giá trị mặc định thì mọi đơn sẽ bị veto và không bao giờ skip.**

Muốn "không có ý kiến" thì **bỏ hẳn field**, đừng gửi `false`.

> **Đây là điểm dễ sai nhất trong toàn bộ `OD-15`.** Xin xác nhận Module 3 **không** gửi
> `trusted_skip_allowed=false` như default.

### 6.2 · Xin xác nhận danh sách mã `risk_flags`

`OD-15` dựa hoàn toàn vào **tính đầy đủ** của list này, nên nó thành contract thật chứ không còn
là metadata tham khảo. Theo `phase-3.1/07 §7.1` và `D-13`:

`NEW_CUSTOMER` · `VERIFIED_ORDER_COUNT_0` · không có lịch sử mua thành công ·
`SUSPICIOUS_DUPLICATE` · `COD_FAIL_HISTORY` · địa chỉ giao rủi ro · phone pattern nghi ngờ ·
giá trị đơn bất thường · hành vi Giờ Vàng rủi ro · contact vừa mới đổi

Ngưỡng cụ thể thuộc Risk Policy bên Module 3 — IVR chỉ consume boolean, **không** tự định nghĩa
"giá trị bất thường" bằng số tiền.

### 6.3 · Cách đo lúc nào xong — không cần Module 3 báo

Advisory `TRUST_RISK_EVIDENCE_UNAVAILABLE` hiện xuất hiện trên **mọi** task đủ điều kiện.
**Khi nó biến mất khỏi log eligibility của IVR, nghĩa là Module 3 đã bật field và skip đang chạy.**

---

## 7. Việc D — Auth production (Security/Platform, nhưng Module 3 bị chặn theo)

Hiện **không có gì**: không issuer, không sandbox credential, không quyết định mTLS.
**Không có credential nghĩa là không chạy được một test tích hợp thật nào.**

Cần:

- Issuer URL, JWKS URL, thuật toán ký, chu kỳ xoay khoá
- Audience đặt là gì cho IVR, TTL token
- Scope set — đề xuất: `ivr.task.write` (Module 3 → `POST /tasks`), `ivr.internal.write`,
  `ivr.admin.read`, `ivr.admin.write`
- **Sandbox credential** + hướng dẫn lấy token
- Quyết định mTLS: có/không, ai cấp cert, xoay ra sao
- Ngày tắt `X-Internal-Token` (hiện chỉ để tương thích cũ)

> **Lưu ý kỹ thuật:** `bearerAuth` hiện là `type: http, scheme: bearer` nên **không mang được
> scope**. Muốn có scope phải chuyển sang `oauth2/clientCredentials` — **đây là thay đổi contract,
> không phải cấu hình.**

---

## 8. Những gì IVR **không** làm — để Module 3 không kỳ vọng nhầm

| IVR không | Vì sao |
| --- | --- |
| Ghi/đổi trạng thái đơn | `D-02` — Module 3 sở hữu chân lý về đơn |
| Tự tạo order, tự sinh `order_code` | ngoài scope |
| Xác nhận thanh toán / doanh thu | `IVR_CONFIRMED` ≠ `PAID` ≠ Verified Revenue |
| Huỷ đơn | kể cả khi khách bấm `0` — IVR chỉ báo tín hiệu |
| Gọi trực tiếp Ops-core | IVR không đọc tồn kho/thu hồi; `D-06` buộc Module 3 revalidate với ops lúc nhận callback |
| Truy vấn/polling đơn hàng | xem §1 |
| Gửi SMS / notification | `TV1-07` — tắt trong V1 |
| Ghi note vào CRM | `D-14` — chỉ audit nội bộ |
| Gọi cho Quote, Cart, Order Draft | chỉ Official Order |
| Gọi khi thiếu bằng chứng | fail-closed, luôn nghiêng về "không gọi" |

---

## 9. Thứ tự ưu tiên — cái gì chặn cái gì

| Ưu tiên | Việc | Chặn cái gì nếu chưa xong |
| --- | --- | --- |
| **1** | §3.2 chốt ma trận + định nghĩa `ivr_confirmation_required` | **chặn tất cả** — sai ma trận = 100% task bị từ chối, im lặng |
| **2** | §4.1 xây endpoint callback generic | chương trình 24/7 **không có lối trả kết quả** |
| **3** | §7 auth + sandbox credential | **không chạy được test tích hợp thật nào** |
| **4** | §3.7 chốt shape `eligibility_snapshot` | IVR không fail-closed đúng trên thứ nó không hiểu |
| **5** | §5 chọn phương án `dial_token` | không quay số thật được |
| **6** | §3.5 duyệt whitelist lời thoại (cần Privacy/Legal) | **rủi ro pháp lý** — không rollback được sau khi đã gọi |
| **7** | §6 thêm `trust.risk_evidence_available` | chỉ mất tối ưu — vẫn gọi tất cả, an toàn |

---

## 10. Checklist gửi Module 3

### Chặn cứng

- [ ] **Ma trận đã ký**: `program_code × payment_method_snapshot × order_state → callable`.
      Trả lời dứt khoát: `GOLDEN_HOUR + COD` có callable không? `GOLDEN_HOUR + ONLINE` có thuộc
      scope IVR V1 không?
- [ ] **`ivr_confirmation_required`**: nguồn business ở đâu, ai set, set khi nào, có bao giờ `false`?
- [ ] **`order_state`**: cam kết `"CONFIRMING"` là hằng số hợp đồng, **hoặc** công bố danh sách
      state callable như dữ liệu?
- [ ] **OpenAPI endpoint callback generic** phủ **cả hai** chương trình, kèm ACK taxonomy
- [ ] **Ranh giới idempotency**: `DUPLICATE_ACCEPTED` vs `IDEMPOTENCY_CONFLICT`, giữ key bao lâu?
- [ ] **`order_version` bump khi nào?** Mỗi lần sửa đơn, hay chỉ khi sửa field ảnh hưởng xác nhận?
      *(Bump theo mọi thay đổi kể cả note nội bộ → tỉ lệ `REJECTED_STALE` cao giả tạo, kết quả
      khách đã bấm phím bị vứt)*

### Cần sớm

- [ ] **Schema `eligibility_snapshot`** + ví dụ pass/block/stale/source-unavailable
- [ ] **Xác nhận đã bỏ `sellable_status[]` khỏi producer.** Gửi nó nay làm task bị `400 IVR_MALFORMED_REQUEST` (`additionalProperties: false`), không phải bị bỏ qua
- [ ] **Phương án `dial_token`**: chọn (a)/(b)/(c)/(d) ở §5, kèm chữ ký Security + sơ đồ trust boundary
- [ ] **Giới hạn `items[]`**: đọc tối đa bao nhiêu dòng, phần dư diễn đạt sao?
- [ ] **Quy tắc normalize `delivery_area_short`**
- [ ] **`attempt_policy_version` production**: bảng đầy đủ mỗi program → số attempt / offsets /
      window. *(Hiện `D-10` và tài liệu phase-8 **lệch nhau**: GH window 5′ vs 10′, 24/7 là 2 hay
      3 lần gọi — `OD-V1-16`)*
- [ ] **Hành vi timeout worker**: sau `NO_ANSWER_FINAL`, đơn nằm ở `CONFIRMING` bao lâu, ai chuyển
      đi, chuyển sang đâu?
- [ ] **4 tình huống race — ai thắng:**
      ① khách bấm phím đúng lúc window hết hạn
      ② Module 3 huỷ đơn trong lúc IVR đang gọi
      ③ khách bấm xác nhận nhưng `order_version` đã bump vì lý do khác
      ④ IVR gửi `NO_ANSWER_FINAL`, Module 3 chưa expire, khách gọi lại tổng đài
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

## 11. Cần gì từ các bên ngoài khác

Module 3 là bên lớn nhất, nhưng không phải bên duy nhất. Đây là toàn bộ 11 cổng ngoài:

| Cổng | Chủ sở hữu | Cần artifact gì để đóng | IVR đã chuẩn bị sẵn gì |
| --- | --- | --- | --- |
| `G-CONTRACT` | Sales API/Core (**Module 3**) | OpenAPI đã duyệt + CDC test | fake provider + WireMock + CDC sẵn sàng |
| `G-SPEECH` | Sales/Product/**Privacy** | schema + examples + phê duyệt privacy | DTO + validator + renderer đã có |
| `G-DIAL` | Sales/Security/Telephony | threat model + API + test | resolver port + mock vault đã có |
| `G-AUTH` | **Security/Platform** | auth profile + sandbox credential + test | mock JWT + negative test đã có |
| `G-POLICY` | **Product/Core** | policy đã ký + version | policy registry versioned đã có |
| `G-LAB-SIM` | **Infra + vendor** | lab report + allowlist + kill-switch evidence | chuỗi lab đã xong, chờ SIM + GSM gateway |
| `G-ESIM32` | **Infra/procurement** | procurement + capacity/failover đo được | capacity simulator đã có |
| `G-LEGAL` | **Legal/Privacy** | review đã ký (kịch bản, retention, do-not-call) | `W-0109` đã tạo **lối thi hành** cho chữ ký |
| `G-RELEASE` | **Release owner** | go/no-go đã chấp nhận + evidence | evidence pack đã nộp (`W-0060`) |
| `G-GITLAB` | **Platform/Infra** | nâng Premium/Ultimate + reviewer thứ hai + chứng minh 1 required approval trước merge | mọi control khác đã hosted-PASS |
| `G-PLATFORM` | **Platform/Infra** | endpoint + credential thật + smoke | docker-compose local stack |

`G-PLATFORM` gồm 8 mục hạ tầng cụ thể (`W-0063`): container registry · K8s cluster + credential
cho 4 môi trường · secret store (Vault/KMS) · observability backend (Tempo/Jaeger + Prometheus +
Loki hoặc APM) · Grafana/Alertmanager · Argo Rollouts/Flagger · analytics warehouse ·
visual-regression service.

---

## Ô ký

| Vai trò | Tên | Ngày |
| --- | --- | --- |
| Owner Module 3 — `ginsengfood-business-platform` | ____________ | ______ |
| Security/Platform (mục §7) | ____________ | ______ |
| Privacy/Legal (mục §3.5) | ____________ | ______ |
