# IR-06 — Bàn giao API Module 3 ↔ IVR

**Gửi:** Team **Module 3 — `ginsengfood-business-platform`** (Commerce / Order Core / Sales Extensions / CRM — Customer Identity)

**Từ:** Team Module 8 — IVR Order Confirmation (.NET, service tách biệt)

**Cập nhật:** 2026-09-03
**Trạng thái:** `TARGET_V1_DRAFT` — chờ Module 3 review/sign-off; IVR repo đã alignment theo `W-0123`, external integration/production gates vẫn mở. **Thêm §4A ngày 28/08/2026:** hợp đồng bề mặt quản trị sau khi IVR xoá toàn bộ hệ thống tài khoản/phân quyền (`W-0128`) — phần này M3 chưa từng nhận, và client viết theo bản trước 28/08 sẽ hỏng. **OpenAPI hiện hành `1.0.0-draft.33` (đối soát 25/09; `draft.33` ghi đúng kiểu OpenAPI 3.1 cho ba field có thể null của `audit-evidence` và bắt `total_amount` là số đồng nguyên; `draft.32` thêm endpoint replay callback ở tầng `danger`, §4A.3):** lịch sử `draft.23` đã gỡ auth/accounts; 11 endpoint auth/accounts đã bị gỡ khỏi spec, M3 cần sinh lại client (§4A.7). **Thêm §3.5A ngày 03/09/2026:** M8 ký đề xuất `golden_hour_session_id`; code/OpenAPI/DB chưa được phép đổi trước chữ ký M3 (`W-0146`).

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
| **OpenAPI IVR — bản hiện hành `1.0.0-draft.33`** | `specs/api/openapi/ivr-order-confirmation.v1.yaml` |
| **So sánh draft.20 → draft.22** | `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.20-to-v1.0.0-draft.22.md` |
| **So sánh draft.22 → draft.23** | `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.22-to-v1.0.0-draft.23.md` |
| **So sánh draft.23 → draft.24 — có breaking, đọc trước khi sinh client** | `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.23-to-v1.0.0-draft.24.md` |
| **So sánh draft.25 → 26 → 27 → 28 → 29 → 30 → 31 → 32 → 33** | [25→26](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.25-to-v1.0.0-draft.26.md), [26→27](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.26-to-v1.0.0-draft.27.md), [27→28](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.27-to-v1.0.0-draft.28.md), [28→29](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.28-to-v1.0.0-draft.29.md), [29→30](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.29-to-v1.0.0-draft.30.md), [30→31](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.30-to-v1.0.0-draft.31.md), [31→32](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.31-to-v1.0.0-draft.32.md), [32→33](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.32-to-v1.0.0-draft.33.md); sinh bằng oasdiff đã ghim; oasdiff không báo breaking ở bước nào — `28→29` **thêm** `GET /audit-evidence` (`W-0307`) và hai schema; `29→30` thêm field tuỳ chọn `phone_e164` (`W-0311`); `30→31` nới `dial_token` + `dial_token_expires_at` thành *"cặp token **hoặc** số"* (`W-0312`, §3.4.0); `32→33` (`W-0354`) thêm `multipleOf: 1` cho `total_amount` — số lẻ nay bị `400`, oasdiff không tính là breaking nhưng đó **là** một phép siết, nên có đính chính ngày `25/09` ở `IR-07` — và đổi `nullable: true` (không có trong OpenAPI 3.1, nên trước đây bị bỏ qua) thành `type: [string, 'null']` ở ba field của `IvrAuditEvidenceRow`. Baseline CI giữ `draft.27` để lượt sau còn so được. *Link `28→29` từng trỏ tới một file không tồn tại (`…draft.28-to-…draft.30.md`) — dấu vết của lần thay chuỗi version trần ở `W-0311`, sửa ở `W-0312`* |
| **Bảng nhãn tiếng Việt cho mọi enum** | `specs/ui/enum-labels.vi.json` |
| **M8-06 — Upstream session trace sign-off** | `plan/ivr-orther/m8-06-upstream-session-trace-signoff-2026-09-03.md` |

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
| **Màn hình quản trị M3 → IVR** | `{ivr}/v1/ivr/order-confirmation/...` (33 endpoint) | Xem hàng đợi, kill switch, cắt cuộc gọi, duyệt lời thoại — **§4A**, hợp đồng mới ngày 28/08/2026 |

1. Cơ chế cấp/resolve/refresh `dial_token` — vị trí phía M8 ký ở `OD-V1-05/17/18` (`2026-09-05`, vế
   TTL `2026-09-09`), **chưa chốt**: `OD-V1-05` là `M8_POSITION_SIGNED / M3_NOT_RECEIVED`;
   `OD-V1-17/18` phụ thuộc phương án B về số điện thoại (Module 3 gửi `phone_e164`, IVR lưu số), chờ
   Sếp trả lời mục `B2` phiếu Sếp `25/09`. Xem `§6`.
2. Service auth production — `OD-V1-07` (`2026-09-05`): JWT khóa bất đối xứng, JWKS, TTL ≤ 10 phút,
   scope `ivr.task.write`, mTLS hoãn. **Chưa chốt:** chờ Tech Lead ký, sau dòng ủy quyền `N14` phiếu
   Sếp `25/09`. Sau chữ ký còn dựng issuer và cấp sandbox credential, xem `§7`.

> *Sửa `25/09` (mục `A1` trong danh sách chief): bản trước ghi cả hai dòng trên là **đã chốt**. Trạng
> thái trên theo chốt chief `25/09`; sổ quyết định (`specs/_review/open-decisions-register.md`) sửa theo
> cùng chốt.*

### Bắt đầu từ đâu — bốn thứ cần lấy

| # | Lấy gì | Ở đâu |
| ---: | --- | --- |
| 1 | Contract hiện hành `1.0.0-draft.33` | `specs/api/openapi/ivr-order-confirmation.v1.yaml` |
| 2 | **Đọc trước khi sinh client**: `draft.23 → draft.24` **có breaking** | `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.23-to-v1.0.0-draft.24.md` |
| 3 | Fixture âm/dương để tự kiểm producer | `seed/sales-target-v1.sample.json` — 10 task hợp lệ (trong đó `golden-hour-online-number-only` chỉ gửi số, §3.4.0), 17 `schema_negative` (`400`; thêm `NEG-SCHEMA-AMOUNT-01` ngày 25/09), 13 `domain_negative` |
| 4 | Nhãn tiếng Việt cho mọi enum, nếu dựng console | `specs/ui/enum-labels.vi.json` + đặc tả màn hình `specs/ui/` |

**Hai thay đổi breaking ở `draft.24` cần biết trước khi code:**

- `phone_validation_status` thành `required` + `enum: [VALID]`. Gửi giá trị khác nay bị **schema**
  chặn: `400 IVR_MALFORMED_REQUEST`, không còn `422 IVR_CONTACT_INVALID`.
- `X-Correlation-Id` và `Idempotency-Key` bị siết cú pháp: `1..128` ký tự trong `[A-Za-z0-9._:-]`.
  Header đang hợp lệ thì vẫn hợp lệ; header lạ ký tự sẽ bị từ chối sớm hơn trước.

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
Idempotency-Key: <1-128 ký tự, chỉ [A-Za-z0-9._:-]>
X-Correlation-Id: <1-128 ký tự, chỉ [A-Za-z0-9._:-]>
X-Source-System: <module-3-source-id>
```

| Header | Bắt buộc | Ý nghĩa |
| --- | --- | --- |
| `Authorization` | Có | Service identity của Module 3 |
| `Idempotency-Key` | Có | Cùng key + cùng body trả kết quả cũ; cùng key + khác body là conflict. **Cú pháp: §3.1.1** |
| `X-Correlation-Id` | Có | Mã truy vết xuyên suốt task, call và callback. **Cú pháp: §3.1.1** |
| `X-Source-System` | Có | Định danh producer được phép gửi task |

#### 3.1.1. Cú pháp hai header bắt buộc — `1-128`, bảng chữ cái đóng

Tài liệu này trước đây ghi `Idempotency-Key: <8-200 chars>` và `X-Correlation-Id: <1-200 chars>`.
**Cả ba con số đều sai** so với `TaskIntakeEndpoint.RequiredHeader`, và chỗ sai nguy hiểm nhất là
chỗ tài liệu **không nói gì**:

| | Tài liệu cũ nói | Code thực sự thi hành |
| --- | --- | --- |
| Dài tối đa | `200` | **`128`** |
| Dài tối thiểu (`Idempotency-Key`) | `8` | **`1`** — không có sàn 8 |
| Bảng chữ cái | *không nói* | **chỉ `[A-Za-z0-9._:-]`** |

Thiếu header → `422` + `IVR_MISSING_TRACE`. Sai cú pháp → `400` + `IVR_MALFORMED_REQUEST`.

> ⚠️ **Bảng chữ cái là chỗ dễ hỏng nhất.** `+`, `/`, `=`, dấu cách, `{}` đều bị từ chối — nghĩa là
> **key base64 sẽ bị chặn**, mà sinh key ngẫu nhiên bằng base64 là việc rất thường. UUID và ULID thì
> an toàn. Ràng buộc này chưa từng được viết ra ở đâu trước hôm nay.

Ghim bằng `IT-INTAKE-HEADER-07`: `128` nhận · `129` `400` · `sB3+xQ/9dGVzdA==` `400` ·
`X-Correlation-Id` 129 ký tự `400`.

**Chưa đồng bộ, đã biết:** OpenAPI khai hai header này là `{ type: string }` trần, trong khi
`GeneratedCorrelationId` ở route khác **đã** ghi đúng
`minLength: 1, maxLength: 128, pattern: '^[A-Za-z0-9._:-]+$'`. Từ `W-0221` thì schema **mô tả được**
cả hai route bằng một `$ref` — trước đó thì không, vì hai route hai luật. Nhưng sửa OAS là một
**lượt phát hành contract**, không phải sửa code: siết một header bắt buộc là breaking theo oasdiff,
kéo theo bump draft, sinh lại client, và re-pin hash ở **5 nơi**. Nên gộp cùng lượt sửa `TTL` (§3.4.1)
thành một lần phát hành thay vì hai — xem mục `2.1` và `7.1` của
[worklist](../plan/toan-viec-can-lam-m8-2026-09-07.md).

**Đã chốt `2026-09-07` (`W-0221`): một luật cho cả API.** Trước đó `Idempotency-Key` ở route intake
bị ràng bảng chữ cái còn ở route admin/internal thì không — cùng một tên header, cùng một API, hai
hành vi, và cùng **một** `$ref` OpenAPI nên schema không thể mô tả đúng cả hai. Owner chọn **siết
admin theo intake**. Luật nay nằm một chỗ (`TraceHeaderSyntax`) và cả ba nơi đọc — intake, internal
guard, correlation middleware — cùng gọi nó.

Với M3 việc này **không đổi gì ở API A**: `1-128` và bảng chữ cái `[A-Za-z0-9._:-]` vốn đã là luật
của route intake. Nó chỉ đổi route admin/internal, tức phần BFF của M3 sẽ gọi sau này — nên biết
trước thì rẻ hơn.

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

### 3.4. 21 field bắt buộc trên wire

> `W-0204`: bảng này và mảng `required` trong OpenAPI nay được **so khớp bằng gate**
> (`FREEZE-03` trong `deploy/ci/scripts/contract-freeze-verifier.mjs`). Nếu hai bên lệch nhau, CI
> đỏ — vì một chữ ký lên tài liệu mà spec không thực thi thì không ký lên cái gì cả. Bản đầy đủ cả
> field lẫn enum, sinh từ spec đã ghim: [`docs/contracts/target-v1-field-inventory.md`](../docs/contracts/target-v1-field-inventory.md).

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
| `phone_validation_status` | string | Chỉ nhận `VALID`. Trở thành `required` + `enum: [VALID]` ở `draft.24` (`W-0250`) — trước đó OAS khai `{ type: string }` optional trong khi runtime đã ép, và bảng này đã ghi *"bắt buộc trên thực tế"* từ trước. Vì enum đóng ở tầng schema, giá trị khác nay bị chặn trước service: **`400 IVR_MALFORMED_REQUEST`**, không còn `422 IVR_CONTACT_INVALID` |
| `privacy_safe_order_summary` | object | Nội dung được phép đọc cho khách |
| `call_restriction` | boolean | Module 3 phải gửi `false`; `true` sẽ bị IVR chặn vì an toàn |
| `eligibility_snapshot` | object | Evidence cho quyết định call-ready của Module 3 |
| `evidence_ref` | string | Con trỏ evidence để đối soát |

#### 3.4.0. Cách IVR tới được khách — số, **hoặc** cặp token (`draft.31`, `W-0312`)

`phone_e164`, `dial_token` và `dial_token_expires_at` **không nằm trong bảng trên** vì không field nào
bắt buộc một mình: task phải mang **ít nhất một trong hai cách** tới được khách. Luật nằm trong
**một `anyOf`** trên `IvrConfirmationTaskV1`, và `TaskIntakeEndpoint.ValidateSchema` thi hành đúng
luật đó.

| Cách | Gửi gì | Ghi chú |
| --- | --- | --- |
| **A — số** *(chốt `17/09`)* | `phone_e164`: `+84` rồi đúng 9 chữ số, **không** kèm field token nào | IVR lưu số, đường quay production quay bằng số |
| **B — token** | `dial_token` **và** `dial_token_expires_at`, luôn đi thành cặp | `dial_token_expires_at` phải bằng đúng window end — §3.4.1 |
| **A + B** | cả ba | Nhận; token vẫn bị kiểm theo §3.4.1, đường quay production quay bằng số |

Bị từ chối `400 IVR_MALFORMED_REQUEST`: không có cách nào · **nửa** cặp token, kể cả khi có số ·
`phone_e164` sai pattern (có `0` đứng đầu, thiếu `+`, thừa hoặc thiếu chữ số).

> ⚠️ **`draft.30` đã hứa điều này nhưng không làm được.** Mô tả nói *"gửi số **hoặc** token"*, còn
> mảng `required` vẫn đòi cặp token, nên task chỉ có số bị `400`. Ngoài ra, ở production intake luôn
> mã hoá token bằng bộ mã hoá mà phương án B đã bỏ, nên **không task nào được nhận** — kể cả task gửi
> cả số lẫn token. `draft.31` sửa cả hai. **Không breaking:** mọi body hợp lệ ở `draft.30` vẫn hợp lệ.

#### 3.4.1. `dial_token_expires_at` — ba guard, một giá trị hợp lệ

**Chỉ áp dụng khi task mang token** (§3.4.0). Gửi số không kèm field token nào thì bỏ qua mục này.

Tài liệu này trước đây nói ba điều khác nhau về trường này (`≥`, `>`, và equality). Đây là những gì
code **thực sự** thi hành hôm nay, đọc từ ba tầng:

| Tầng | Vị trí | Luật |
| --- | --- | --- |
| Intake | `TaskIntakeService.ContactRejectionReason` | từ chối nếu `dial_token_expires_at` **≠** `confirmation_window_expires_at` → `422 IVR_CONTACT_INVALID`, mỗi chiều một reason: **<** là `DIAL_TOKEN_EXPIRES_BEFORE_WINDOW`, **>** là `DIAL_TOKEN_EXPIRES_AFTER_WINDOW` (chiều **>** có từ `W-0302`, `draft.28`) |
| Persistence | `PersistenceInvariantValidator.ValidateTask` | throw nếu **>** `confirmation_window_expires_at` |
| Dispatch | `PostgresTelephonyDispatchStore.LoadAsync` | throw nếu **>** `lease.Deadline`, và `lease.Deadline` = `job.expires_at` = window end |

Giao của ba luật là **một điểm duy nhất**: `dial_token_expires_at == confirmation_window_expires_at`.

Hai hệ quả M3 cần biết:

- Gửi **sớm hơn** window end → bị từ chối sạch ở intake: `422` / `DIAL_TOKEN_EXPIRES_BEFORE_WINDOW`.
- Gửi **muộn hơn** window end → cũng bị từ chối sạch ở intake: `422` / `DIAL_TOKEN_EXPIRES_AFTER_WINDOW`.
  *Trước `W-0302` (`draft.28`), chiều này **lọt qua intake** rồi hỏng ở tầng persistence thành `500` —
  chế độ hỏng khó chẩn đoán nhất trong toàn bộ seam, và là lý do bảng trên tồn tại.* Hai tầng dưới vẫn
  giữ luật của chúng như lớp chặn thứ hai.

> ✅ **Đã chốt `2026-09-09` (`W-0246`): `dial_token_expires_at` = **đúng** confirmation-window end.**
>
> Quyết định này **thay thế** vế TTL của `OD-V1-17` (`CLOSED` 05/09, ghi *"TTL = cửa sổ xác nhận
> + 60s"*). Con số `+60s` chưa từng thi hành được: nó qua intake rồi bị persistence từ chối, nên
> equality vốn đã là hợp đồng thực tế — nay nó là hợp đồng **đã ký**, và ba tầng không phải sửa.
>
> Vì sao equality đứng vững chứ không chỉ là chấp nhận hiện trạng: token hết hạn đúng cuối cửa sổ
> nghĩa là **không quyền quay số nào sống lâu hơn cửa sổ gọi**. Cuộc đang gọi không bị cắt, vì
> dispatch đã có ràng buộc riêng `DialTokenExpiresAt <= lease.Deadline` lo phần đó.
>
> **M3 không phải đổi gì** — equality là thứ M3 vẫn gửi.
> Theo dõi ở `DTK-02`/`DTK-06` trong M8-10 và mục 0.1 của
> [worklist hiện hành](../plan/toan-viec-can-lam-m8-2026-09-07.md).
>
> *Đính chính `25/09` (mục `A1` trong danh sách chief): `OD-V1-17` nay **không còn** `CLOSED` — sổ
> quyết định ghi phụ thuộc phương án B về số điện thoại, chờ Sếp trả lời mục `B2` phiếu Sếp `25/09`.
> Luật equality ở trên không đổi: ba tầng vẫn thi hành nó cho mọi task mang token. Cái đổi là trạng
> thái chữ ký.*

#### 3.4.2. Giờ phát task muộn nhất còn đủ hai cuộc gọi

Khung giờ gọi đóng lúc **21:08** giờ VN (`CallingWindowOptions` mặc định — owner chốt `2026-09-07`,
`W-0220`; trước đó là `21:00` theo `OD-V1-16`). Attempt 2 nằm ở `T0 + 150s` (Giờ Vàng) hoặc
`T0 + 450s` (24/7) theo `OD-V1-08`. Nhân hai điều đó với nhau ra một mốc mà M3 cần biết khi quyết
định lúc nào còn phát task:

| `program_code` | Offset attempt 2 | T0 muộn nhất còn đủ **hai** cuộc |
| --- | ---: | ---: |
| `TWENTY_FOUR_SEVEN` | `450s` | **21:00:29** |
| `GOLDEN_HOUR` | `150s` | **21:05:29** |

*Sửa `25/09` (mục `D11` trong danh sách chief): bản trước ghi `21:00:30` / `21:05:30`. Đó là mốc **đầu
tiên mất** cuộc thứ hai — attempt 2 rơi đúng `21:08:00`, đã ngoài giờ gọi — chứ không phải mốc cuối
còn đủ hai cuộc. `UT-SCH-WINDOW-09` ghim mốc cắt của cả hai chương trình (`21:00:30`, `21:05:30`) và
kiểm `21:05:29` còn đủ hai cuộc.*

> **Vì sao là `21:08` chứ không phải `21:07:30`.** Owner chốt `21:07:30` — đúng bằng `21:00` cộng
> offset 450s, tức "đơn cuối cùng nhận lúc chín giờ vẫn đủ hai cuộc". Nhưng
> `EndMinuteOfLocalDay` là **phút**, và gate bỏ phần giây: tại `21:07:30` thì minute-of-day là
> `1267`, không nhỏ hơn `1267`, nên đặt `21:07` sẽ **đóng cửa đúng lúc cần mở** và quyết định coi
> như không xảy ra. `21:08` là giá trị nhỏ nhất thi hành được ý đó, đắt thêm 30 giây.

> ⚠️ `TWENTY_FOUR_SEVEN` **cắt sớm hơn** `GOLDEN_HOUR` 5 phút. Tên chương trình nói về lúc Sales
> nhận đơn, **không** phải lúc IVR được phép gọi: khung giờ không theo program.

**Việc này đổi kết quả M3 nhận được, không chỉ số cuộc gọi.** Task phát sau mốc trên vẫn được nhận
và vẫn được gọi **một** lần. Nhưng nếu khách không nghe:

| Tình huống | `result_type` | `recommended_core_action` |
| --- | --- | --- |
| Đủ 2 attempt, không nghe | `IVR_NO_ANSWER_FINAL` | `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` — chỉ là nhãn, Module 3 **không** làm theo (§4.4) |
| Attempt 2 rơi ngoài giờ gọi | `IVR_CONFIRMATION_WINDOW_EXPIRED` | `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION` hoặc `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` — Module 3 **phải** đọc (§4.3) |

*Sửa `25/09`: bản trước ghi tên action không có tiền tố `CORE_`. Đó là giá trị IVR lưu trong DB của
mình; trên dây luôn có tiền tố (`TargetV1ContractMapper`).*

Cùng một hành vi khách hàng, hai kết quả khác nhau, quyết bởi giờ đặt đơn. Consumer của M3 phải xử
lý được cả hai cho cùng một kịch bản "khách không nghe máy".

**Đã chốt (`W-0220`, `2026-09-07`) — mục này không còn mở.** Owner chọn vế nới khung giờ:
`End = 21:08`, ghi ở đầu §3.4.2 kèm lý do vì sao là `21:08` chứ không phải `21:07:30`. Câu *"chưa
chốt (`W-0215`)"* từng đứng ở đây đã hết đúng kể từ ngày đó, và mâu thuẫn với chính đoạn mở đầu
mục này — hai mốc trong bảng trên **đã** tính theo `21:08`. `UT-SCH-WINDOW-09` vẫn ghim chúng: nó
suy mốc từ policy + window nên sẽ đỏ khi một trong hai đổi.

> **Bổ sung `W-0304` (16/09/2026) — đơn đêm nay bị từ chối ngay tại intake (`W-0298`).**
>
> Trước `W-0298`, một task mà **mọi** attempt đều rơi ngoài `08:00–21:08` vẫn được nhận (`200`
> kèm quyết định `TASK_ACCEPTED_*`), rồi nằm im cho tới khi hết cửa sổ và trả
> `IVR_CONFIRMATION_WINDOW_EXPIRED`. Tức IVR nhận một lời
> hứa gọi mà nó đã biết trước là không giữ được. Nay intake **từ chối ngay**:
>
> | | |
> | --- | --- |
> | Điều kiện | `T0` (`confirmation_window_started_at`) nằm ngoài giờ gọi `08:00–21:08` (giờ VN) — *từ `25/09` (`B17`); trước đó: không attempt nào của `attempt_policy` rơi vào giờ gọi, trên toàn bộ confirmation window* |
> | Quyết định | `TASK_BLOCKED_OPERATIONAL` |
> | Reason code | `CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW` — giữ nguyên tên dù điều kiện đổi `25/09` |
> | Test | `UT-INTAKE-NIGHT-01/02/03`, `UT-INTAKE-MORNING-01/02`, `UT-INTAKE-EVENING-01`, `UT-INTAKE-WINDOW-SWEEP-01` |
>
> Guard này **tắt cùng `CallingWindow`**: khi `Enabled=false` thì mọi task đi qua như cũ, vì lúc đó
> không có giờ nào bị coi là ngoài giờ.
>
> **M3 phải xử lý phần của mình:** đơn đặt ban đêm không còn được IVR nhận. Đây là thay đổi **thấy
> được trên wire**, không phải chi tiết nội bộ: cùng một payload, HTTP vẫn `200`, nhưng quyết định
> đổi từ nhận sang blocked.
>
> *Sửa `25/09` (`W-0354`): bản trước ghi mã `202` ở hai chỗ trong mục này. Endpoint trả `200` kèm
> `decision` (`TaskIntakeEndpoint`, `Results.Ok`), trước lẫn sau `W-0298`. Câu "hoặc M3 giữ lại tới
> `08:00`, hoặc xử lý ngoài IVR" nay chỉ còn một hướng, do chief chốt `25/09`:* **M3 giữ đơn
> `TWENTY_FOUR_SEVEN` (COD) phát sinh ngoài `08:00–21:08`, rồi gửi task từ `08:00` với cửa sổ mới và
> `Idempotency-Key` mới.** *Không retry trong cùng cửa sổ, vì kết quả sẽ không đổi. Chi tiết ở `IR-07`,
> đính chính `25/09`.*

> **Đính chính `25/09` (mục `B17` trong danh sách chief) — biên buổi sáng.** Điều kiện cũ xét **từng**
> lịch attempt (`T0 + offset`), còn scheduler thì **gọi bù**: hàng rào giờ gọi của scheduler chỉ chặn
> việc quay số *tại thời điểm quét* (`SchedulerRuntime`), còn câu claim
> (`PostgresSchedulerStore.TryClaimDueDispatchAsync`) nhận mọi attempt đã tới hạn khi cửa sổ xác nhận
> chưa hết. Attempt có lịch trước `08:00` vì thế được quay ngay lúc `08:00`, và hai vùng `T0` buổi sáng
> bị xử lý sai:
>
> | Vùng `T0` (giờ VN) | `TWENTY_FOUR_SEVEN` | `GOLDEN_HOUR` | Trước `25/09` | Từ `25/09` |
> | --- | --- | --- | --- | --- |
> | Từ chối dù gọi được | `07:45:01–07:52:29` | `07:55:01–07:57:29` | Từ chối, dù cửa sổ xác nhận còn mở qua `08:00` nên scheduler quay được ít nhất một cuộc | Từ chối |
> | Gọi bù, hai cuộc sát nhau | `07:52:30–07:59:59` | `07:57:30–07:59:59` | Nhận. Attempt 1 được quay bù lúc `08:00`; attempt 2 tới hạn ở `T0 + 450s` (24/7) hoặc `T0 + 150s` (Giờ Vàng), tức chỉ cách attempt 1 từ gần `0` tới `449 s` (24/7) hoặc `149 s` (Giờ Vàng). Ở đầu vùng, hai cuộc gần như liền nhau | Từ chối |
> | Buổi tối, chỉ kịp một cuộc | `21:00:30–21:07:59` | `21:05:30–21:07:59` | Nhận; attempt 2 rơi ngoài giờ gọi. Khách không nghe cuộc 1 thì kết quả là `IVR_CONFIRMATION_WINDOW_EXPIRED`, không phải `IVR_NO_ANSWER_FINAL` | **Không đổi** — chief đang quyết |
>
> Từ `25/09`, mọi `T0` trước `08:00:00` bị từ chối bằng đúng quyết định `TASK_BLOCKED_OPERATIONAL` và
> reason `CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW` của ca đơn đêm; với đơn
> `TWENTY_FOUR_SEVEN` (COD), Module 3 xử lý như khối `Sửa 25/09` ngay trên. Biên tối không đổi. Các mốc
> giây giả định lượt quét scheduler `≤ 1` giây.

#### 3.4A. W-0151 correction — attempt policy

Current IVR wire cố ý mang **version + snapshot**. Intake resolve registry theo
`(attempt_policy_version, program_code, execution_mode)` rồi so exact
`max_customer_attempts`, ordered `attempt_offsets_seconds` và window duration. Nếu lệch, IVR trả
`409 IVR_POLICY_MISMATCH` và không tạo job. Vì vậy M3 không được tự sửa payload cho “gần đúng” hoặc
chỉ đổi version string.

Chưa có version/matrix production được ký. `mock-lab-v1` là candidate; dev seed chỉ cho MOCK và
lab seed mặc định dùng `lab-softphone-v1`. Snapshot M3
`PhucApu@a3aad246d986fbc273cf41aaa93eec6659669656` chưa cho thấy Target V1 producer/schema phát
năm field window/policy ở trên. Trước code, Product + Order Core + M3 phải ký `ATP-01..ATP-15`,
M3 giao producer SHA/OpenAPI/schema/CDC và payload sandbox. Xem
[M8-11 decision pack](../plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md).

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

> **Bổ sung `W-0304` — không có field `priority` trên wire, và sẽ không có.**
>
> Contract dùng `additionalProperties: false`, nên gửi `priority` bị từ chối ở schema. Thứ tự thực
> thi do scheduler quyết, **không** nhận chỉ thị từ M3. Thứ tự thật, đọc từ
> `PostgresSchedulerStore.TryClaimDueDispatchAsync`:
>
> `expires_at` → `program_type` (`GOLDEN_HOUR` trước `TWENTY_FOUR_SEVEN`) → offset của attempt kế
> tiếp → **số lượng** `risk_flags` giảm dần → `created_at` → `ivr_call_job_id`.
>
> Hai điều đáng nói vì dễ hiểu nhầm theo hai hướng ngược nhau:
>
> - `risk_flags` **có** ảnh hưởng thứ tự — nhưng chỉ bằng **số phần tử**, không bằng nội dung, và
>   không bao giờ đảo quyết định gọi/skip (xem đúng bảng ở trên).
> - `created_at` và `ivr_call_job_id` là tie-break **tất định**: hai lần chạy trên cùng dữ liệu cho
>   cùng một thứ tự. Đó là thuộc tính kiểm được, không phải tình cờ.
>
> Nếu M3 cần một đơn được gọi trước, đòn bẩy duy nhất là `expires_at` — không phải một field mới.

### 3.5A. W-0146 — đề xuất upstream Golden Hour session

Current `IvrConfirmationTaskV1` **chưa có** session field. Current
`ivr_capacity_incidents.session_id` là capacity scope ID nội bộ/synthetic của IVR, gồm cả
`ADMIN-QUEUE-*` cho incident global `ProgramCode=ALL`; không được dùng nó như Golden Hour business
session.

M8 ký đề xuất sau, chờ M3 đồng ký:

| Thuộc tính | Đề xuất |
| --- | --- |
| Field | `golden_hour_session_id`; không alias `session_id`/`source_session_id` |
| Type | opaque string `1..128`, case-sensitive, no control/edge whitespace, không PII |
| `GOLDEN_HOUR` | required, non-null |
| `TWENTY_FOUR_SEVEN` | prohibited/absent; `null` cũng không hợp lệ |
| Owner | M3 / Golden Hour Core phát trước khi tạo IVR task |
| Stability | giữ nguyên qua retry/replay và mọi task thuộc cùng business session |
| Multiplicity | một session có thể có nhiều task; không unique index, không idempotency key |
| Persistence | task → job → cột nullable riêng trên task-scoped GH incident |
| Internal capacity ID | giữ nguyên `capacity_incident.session_id`; tuyệt đối không map đè |

Rollout bắt buộc theo store phase → M3 producer/CDC → shared E2E → enforce phase. Thêm property
có thể additive, nhưng bắt nó required cho Golden Hour là breaking với producer cũ; không được bỏ
qua cutover. Không backfill từ task/order/correlation/internal ID và không thêm field vào callback
nếu chưa có signed use case riêng.

**M8 signature:** **Tôi — Module 8 / Project Owner** · **2026-09-03** — ký đề xuất và stop rule.

**M3 signature/producer artifact:** **NOT_RECEIVED**. Vì vậy OpenAPI, generated DTO, domain, DB,
scheduler và test vẫn **CODE_NOT_AUTHORIZED**. Mẫu phản hồi/CDC đầy đủ nằm trong
[gói M8-06](../plan/ivr-orther/m8-06-upstream-session-trace-signoff-2026-09-03.md).

### 3.6. `privacy_safe_order_summary`

`additionalProperties: false`. Tất cả field dưới bắt buộc trừ `pronunciation_hints` và `unit_label`.

| Field | Kiểu | Ràng buộc |
| --- | --- | --- |
| `customer_display_name` | string | Tên/xưng hô an toàn, ví dụ `chị An` |
| `order_code_short` | string | Mã rút gọn để đọc |
| `items[]` | array | Ít nhất một item; mỗi item có `public_name`, `quantity`, optional `unit_label` |
| `total_amount` | number | Số **đồng nguyên** khách phải trả, sau lần làm tròn cuối — cùng nguồn với số phải thu `final_payable` phía Module 3. Không âm. Từ `draft.33` có `multipleOf: 1`: số lẻ (ví dụ `210636.8`) bị `400 IVR_MALFORMED_REQUEST`. Số này được đọc cho khách nghe, và tiền đồng đọc thành lời không có phần lẻ. *Sửa `25/09` (mục `B16` trong danh sách chief): bản trước ghi "Số không âm; IVR tự đọc thành lời"* |
| `currency` | string | Chỉ `VND` |
| `delivery_area_short` | string | Chỉ khu vực rút gọn; không gửi địa chỉ đầy đủ |
| `program_display_name` | string | Tên chương trình để đọc |
| `locale` | string | Chỉ `vi-VN` |
| `pronunciation_hints` | object | Optional, gợi ý phát âm. **Không dùng lúc gọi** — xem đính chính `25/09` ngay dưới |

> **Đính chính `25/09` (mục `C23` trong danh sách chief) — production không sinh giọng nói lúc gọi.**
> Theo luật quá độ Tech Lead ngày `24/09`, lời thoại production là audio dựng sẵn từ template đã
> duyệt; production **không** sinh giọng trong lúc gọi. Vì vậy `pronunciation_hints` **không** được
> dùng lúc gọi — nó chỉ còn nghĩa cho bước render trước, nếu bước đó dùng tới. Phần động của lời thoại
> gồm những gì (tên hàng và vùng giao, hay mã đơn) Tech Lead đang chốt; tới lúc đó Module 3 không dựng
> dữ liệu riêng cho việc đọc tên hàng.

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

### 3.8. Payload minh họa Module 3 → IVR

Các giá trị policy dưới đây chỉ minh họa shape candidate, **không phải production approval**.

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
  "attempt_policy_version": "mock-lab-v1",
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
| `200 TASK_BLOCKED_OPERATIONAL` | IVR không nhận task vì một chặn vận hành; task **không** được lưu. Hai reason trong `blocked_reasons`: `CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW` — `T0` ngoài giờ gọi (§3.4.2); `DIAL_TOKEN_PROTECTION_UNAVAILABLE` — task gửi `dial_token` **không** kèm `phone_e164`, trên deployment ngoài MOCK chưa có bộ mã hoá dial token (production hiện chưa có) | **Không chờ callback. Không retry với cùng payload** — kết quả không đổi. Giờ gọi: đơn `TWENTY_FOUR_SEVEN` (COD) thì giữ lại, gửi lại từ `08:00` với cửa sổ mới và `Idempotency-Key` mới (được dùng lại `task_id`) — §3.4.2. Bộ mã hoá dial token: vấn đề cấu hình phía IVR; báo IVR. *Thêm `25/09` (mục `C15` trong danh sách chief)* |
| `422 IVR_NOT_OFFICIAL_ORDER` | `order_state` là `QUOTE`/`CART`/`DRAFT`, hoặc định danh đơn không hợp lệ | **Không chờ callback.** Sửa producer: chỉ gửi Official Order |
| `422 IVR_STATE_NOT_CALLABLE` | `is_ivr_callable=false`, hoặc `order_state` không nằm trong tập được gọi | **Không chờ callback.** M3 tự tiếp tục workflow của mình |
| `409 IVR_POLICY_MISMATCH` | attempt policy/cửa sổ trong payload lệch snapshot đã duyệt | **Không chờ callback.** Sửa policy version/payload |
| `422 IVR_CONTACT_INVALID` | `phone_ref`/`dial_token` không dùng được hoặc không đủ hạn | **Không chờ callback.** Sửa dữ liệu liên lạc |
| `422 IVR_SCRIPT_NOT_APPROVED` | Chưa có script version được duyệt cho chế độ đang chạy | **Không chờ callback.** Vấn đề cấu hình phía IVR; báo IVR |
| `409 IVR_OPERATIONAL_BLOCKED` | `call_restriction=true` — ở route intake, đây là ca duy nhất ra mã này. *Sửa `25/09`: bản trước ghi thêm "hoặc blocker vận hành đang active"; các `TASK_BLOCKED_OPERATIONAL` khác ở intake đi `200` kèm `decision` (dòng trên)* | **Không chờ callback.** Tôn trọng chặn; không retry |
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
| `phone_validation_status` khác `VALID` | `PHONE_VALIDATION_STATUS_NOT_VALID` | `400 IVR_MALFORMED_REQUEST` từ `draft.24`; schema chặn trước service |
| contact/dial-token sai (sáu mã còn lại) | một trong bảy mã W-0129 | `422 IVR_CONTACT_INVALID` |
| `total_amount` có phần lẻ | *(không có — schema chặn trước service)* | `400 IVR_MALFORMED_REQUEST` từ `draft.33` (`W-0354`); trước đó intake nhận rồi hỏng lúc quay số. *Thêm `25/09`* |

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
| `phone_validation_status` | `VALID` | `PHONE_VALID` | Enum deserialize lỗi → **`400 IVR_MALFORMED_REQUEST`** từ `draft.24`. Ồn ào, phát hiện ngay — cùng kiểu `program_code` ở dòng trên. Trước `draft.24` là `422 IVR_CONTACT_INVALID` |
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
2. **`dial_token_expires_at` phải bằng đúng `confirmation_window_expires_at`** và phải còn hạn tại thời điểm intake. Sớm hơn cửa sổ → `422 IVR_CONTACT_INVALID`. **Muộn hơn cửa sổ → intake nhận, rồi hỏng ở tầng dưới.** Xem §3.4.1.

#### Ghi chú cho người đọc code IVR

Trong repo IVR, `ELIGIBLE_FOR_IVR` **cũng** tồn tại — nhưng nó là decision **IVR tự phát ra sau khi đánh giá**, không phải giá trị IVR chờ nhận. Chiều vào dùng `ELIGIBLE`. Hai từ vựng, hai chiều, hiện nằm cùng một file mà không có chú thích, nên rất dễ nhầm. Khi ký mapping cần nói rõ chiều nào dùng chuỗi nào.

#### Việc M3 xác nhận trước lab

- [ ] Producer map `24_7` → `TWENTY_FOUR_SEVEN`.
- [ ] Producer gửi `phone_validation_status=VALID`, không phải `PHONE_VALID`.
- [ ] Producer gửi `eligibility_snapshot.decision=ELIGIBLE`, không phải `ELIGIBLE_FOR_IVR`.
- [ ] `phone_masked` đã che ít nhất một ký tự.
- [ ] `dial_token_expires_at` **=** `confirmation_window_expires_at` (bằng đúng, không lớn hơn — §3.4.1).

---

## 4. API B — IVR trả kết quả cho Module 3

### 4.1. Endpoint Module 3 phải xây

```http
POST {sales_base_url}/api/v1/internal/orders/{orderId}/ivr-result-callbacks
Authorization: Bearer <service-jwt>
Idempotency-Key: ivr-result:RESULT-<raw_event_id>
X-Correlation-Id: <correlation_id của task>
Content-Type: application/json
```

`{orderId}` phải bằng `order_id` trong body.

> **Chiều này IVR là bên gửi, nên đây là giá trị IVR thật sự phát — không phải một khoảng để M3 tự
> chọn.** `Idempotency-Key` luôn là `"ivr-result:" + ivr_call_result_id`
> (`CallbackOutboxSnapshotFactory`), mà `ivr_call_result_id` là `"RESULT-" + raw_event_id`. Với
> `raw_event_id` dạng GUID-32 hiện hành thì key dài **50 ký tự** và chỉ dùng `[A-Za-z0-9:-]`.
> `X-Correlation-Id` là `correlation_id` của chính task M3 đã gửi ở API A, trả nguyên vẹn.
> M3 cứ sizing cột theo `128` cho khớp chiều còn lại là an toàn. Trước `W-0209` chỗ này ghi
> `<8-200 chars>` — không sai nguy hiểm như chiều API A, nhưng là một khoảng bịa thay cho một giá
> trị xác định.

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
| `recommended_core_action` | Bắt buộc | Gợi ý; Module 3 vẫn tự quyết định — **trừ** với `IVR_CONFIRMATION_WINDOW_EXPIRED`, nơi Module 3 bắt buộc đọc nó (§4.3, đính chính `25/09`) |
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

**Correction `W-0145` / M8-05:** 11 là contract vocabulary, không phải số callback producer.
Runtime IVR hiện persist 9 result type; 6 final type đi vào callback outbox
(`CONFIRMED`, `CUSTOMER_CANCELLED`, `NO_ANSWER_FINAL`, `CONFIRMATION_WINDOW_EXPIRED`,
`INVALID_PHONE_FINAL`, `CAPACITY_EXCEPTION`), còn `NO_ANSWER_ATTEMPT`, `WRONG_INPUT`,
`TECHNICAL_EXCEPTION` là non-final và không callback. Hai blocked code không được persist/send như
call result. Bảng đầy đủ và chữ ký phía M8 nằm ở
[M8-05](../plan/ivr-orther/m8-05-program-result-contract-signoff-2026-09-03.md).

> **Đính chính `25/09` (mục `C2` trong danh sách chief) — một dòng của M8-05 sai với runtime.** Bảng cầu
> nối ở M8-05 §3.1 ghi `IVR_OPT_OUT` *"chặn ở eligibility, ghi `IVR_POLICY_BLOCKED`"*. Thực tế: V1
> **không có** tín hiệu opt-out (`§9a`, `OD-V1-23`), và **không** nơi nào trong runtime phát
> `IVR_POLICY_BLOCKED` — giá trị này chỉ còn trong enum để tương thích. Đơn có `call_restriction=true`
> bị chặn ngay ở intake: tầng service ghi `TASK_BLOCKED_OPERATIONAL` + `PHONE_CALL_RESTRICTED`, còn trên
> dây Module 3 nhận `409 IVR_OPERATIONAL_BLOCKED` — mã lỗi của envelope, trùng tên với một `result_type`
> nhưng không phải callback — và không có callback nào. M8-05 bị ghim hash, nên đính chính nằm ở đây.

**Bổ sung `W-0304` — map `result_type` → `cancellation_reason_code` (đề xuất M8, M3 chốt).**

Khi huỷ đơn, M3 phải đổi `result_type` của IVR thành `cancellation_reason_code` của M3. Chưa có
bảng nào chốt việc đó, nên mỗi lần chạm tới lại phải suy từ đầu — và hai người suy có thể ra hai
kết quả. Đề xuất phía M8:

> **Đính chính `25/09` (mục `C22` trong danh sách chief) — bảng `v0` dưới đây không còn hiệu lực ở các
> dòng của khối này.** Chief phát hành bảng chuẩn `ivr-cancel-reason-map.v1` ngày `25/09` trong
> `_SPEC/FIX_M8.md` và nhận lỗi bảng `v0`: `v0` chỉ khoá theo `result_type`, nên bỏ mất hai phân biệt
> mà runtime đang phát. Nguyên văn `v1` sẽ được dán vào đây khi IVR nhận văn bản; tới lúc đó khối này
> chỉ ghi các dòng **chắc chắn** của `v1`.
>
> **Khoá bảng là cặp** (chương trình của đơn, `result_type`). Riêng `IVR_CONFIRMATION_WINDOW_EXPIRED`
> khoá thêm `recommended_core_action`, ở **cả hai** chương trình: với kết quả này action **không** còn
> là gợi ý — Module 3 **bắt buộc** đọc nó. IVR phát `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION` khi trước
> lúc hết cửa sổ đã có ít nhất một attempt tính lượt khách, và `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW`
> khi chưa có attempt nào như vậy, tức khách **chưa từng** được gọi tới vì lỗi phía IVR
> (`PostgresSchedulerStore.CloseMissedDeadlinesAsync`).
>
> | Chương trình của đơn | `result_type` (+ `recommended_core_action`) | Module 3 làm gì | `cancellation_reason_code` |
> | --- | --- | --- | --- |
> | `TWENTY_FOUR_SEVEN` (COD) | `IVR_NO_ANSWER_FINAL` | Hủy đơn | `IVR_NO_ANSWER_MAX` |
> | `TWENTY_FOUR_SEVEN` (COD) | `IVR_CONFIRMATION_WINDOW_EXPIRED` + `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION` | Hủy đơn | `IVR_CONFIRMATION_EXPIRED` |
> | `TWENTY_FOUR_SEVEN` (COD) | `IVR_CONFIRMATION_WINDOW_EXPIRED` + `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW`, hoặc `IVR_CAPACITY_EXCEPTION` | **Không tự hủy** — chuyển người trực. Người trực quyết hủy thì ghi mã ở cột phải | `IVR_CONFIRMATION_INVALID`, fault `NONE` |
> | `GOLDEN_HOUR` | `IVR_NO_ANSWER_FINAL` | Cho xác nhận hết hiệu lực, nhả suất theo flow 05 — **không** hủy như đơn COD | `IVR_NO_ANSWER_MAX` |
> | `GOLDEN_HOUR` | `IVR_CONFIRMATION_WINDOW_EXPIRED` + `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` (lỗi phía IVR) | Cho xác nhận hết hiệu lực theo flow 05 | `IVR_CONFIRMATION_INVALID` |
> | `GOLDEN_HOUR` | `IVR_CONFIRMATION_WINDOW_EXPIRED` + `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION` | Cho xác nhận hết hiệu lực theo flow 05 | *chờ nguyên văn `v1`* |
>
> **Chờ nguyên văn `v1`:** `IVR_CUSTOMER_CANCELLED` và `IVR_INVALID_PHONE_FINAL` ở cả hai chương
> trình; `IVR_CAPACITY_EXCEPTION` của đơn `GOLDEN_HOUR`.
>
> `IVR_NO_ANSWER_FINAL` mang một trong năm `result_reason`, là lý do của **attempt cuối**
> (`DispositionMapper`): `RING_TIMEOUT` (đổ chuông hết giờ), `BUSY` (máy bận), `ANSWERED_NO_INPUT`
> (nghe máy nhưng không bấm), `REJECTED_REVIEW_REQUIRED` (khách từ chối cuộc gọi),
> `WRONG_INPUT_MAX_ATTEMPTS` (nghe máy, bấm sai phím ở attempt cuối). Module 3 dùng trường này để tách
> bộ đếm không nghe máy; ngưỡng của bộ đếm chờ Sếp và Module 3 chốt.

*Bảng `v0` (đề xuất phía M8, `16/09`) giữ làm lịch sử; dòng nào khối trên đã thay thì không dùng:*

| M8 phát (`result_type`) | M3 dùng (`cancellation_reason_code`) |
| --- | --- |
| `IVR_NO_ANSWER_FINAL` | `IVR_NO_ANSWER_MAX` |
| `IVR_CUSTOMER_CANCELLED` | `IVR_DECLINED` |
| `IVR_INVALID_PHONE_FINAL` | `IVR_INVALID_NUMBER` |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | `IVR_CONFIRMATION_EXPIRED` |
| `IVR_CAPACITY_EXCEPTION` | ~~**M3 chốt**~~ — lỗi phía hệ thống, `fault=NONE`, **không** được tính là khách từ chối. *Đơn COD: đã có ở khối đính chính `25/09` ngay trên* |

`IVR_CONFIRMED` không có trong bảng vì nó không dẫn tới huỷ. Ba non-final (`NO_ANSWER_ATTEMPT`,
`WRONG_INPUT`, `TECHNICAL_EXCEPTION`) cũng không, vì chúng không đi vào callback outbox — M3 sẽ
không bao giờ nhận chúng như một kết quả cuối.

*Đính chính `25/09` (mục `C22` trong danh sách chief): câu trên chỉ đúng với **giá trị**
`IVR_WRONG_INPUT`. Khách nghe máy rồi bấm sai phím ở attempt cuối thì kết quả **có** tới Module 3,
dưới dạng `IVR_NO_ANSWER_FINAL` với `result_reason = WRONG_INPUT_MAX_ATTEMPTS`
(`DispositionMapper.MapWrongInput`) — xem khối đính chính ngay trên bảng `v0`.*

Hàng cuối là hàng phải bàn: `IVR_CAPACITY_EXCEPTION` nghĩa là **IVR** không gọi được, không phải
khách không muốn. Nếu M3 map nó vào một mã mang nghĩa khách từ chối thì con số từ chối sẽ mang
trong nó lỗi hạ tầng của IVR, và không ai tách lại được về sau.

⚠️ Đây là **đề xuất chưa ký**. Nếu M3 dùng vocabulary khác, sửa **cột phải**; cột trái là contract
đã công bố và M3 đã sinh client theo nó.

### 4.4. `recommended_core_action`

| Giá trị | Ý nghĩa |
| --- | --- |
| `CORE_REVALIDATE_AND_CONFIRM_ORDER` | Revalidate rồi cân nhắc xác nhận đơn |
| `CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST` | Revalidate rồi xử lý yêu cầu huỷ |
| `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` | Chưa đổi state; chờ timeout policy. *Với `IVR_NO_ANSWER_FINAL`: chỉ là nhãn, Module 3 **không** làm theo — đính chính `25/09` ngay dưới bảng* |
| `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION` | Revalidate rồi hết hạn xác nhận |
| `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` | Revalidate và chuyển review |
| `CORE_IGNORE_STALE_CALLBACK` | Bỏ qua callback stale |
| `CORE_BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT` | Chặn vì điều kiện vận hành |

Đây chỉ là advisory. Module 3 sở hữu state machine và không bắt buộc thực hiện theo gợi ý.

> **Đính chính `25/09` (mục `C7` và `C22` trong danh sách chief) — hai ngoại lệ của câu trên.**
>
> 1. **`IVR_NO_ANSWER_FINAL` + `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`.** Ở cặp này, giá trị action chỉ
>    là một **nhãn** bị ràng buộc trong DB của IVR khoá (và không đổi ở lượt này). Module 3 **không**
>    làm theo nó và **không** chờ timeout: đơn `TWENTY_FOUR_SEVEN` (COD) hủy với lý do
>    `IVR_NO_ANSWER_MAX`; đơn `GOLDEN_HOUR` cho xác nhận hết hiệu lực và nhả suất theo flow 05 (lý do
>    `IVR_NO_ANSWER_MAX`). IVR không tự hủy đơn. Nguồn: `IR-07`, đính chính `25/09`, dòng `A-10` và
>    `M3-13`.
> 2. **`IVR_CONFIRMATION_WINDOW_EXPIRED`.** Ở kết quả này action **không** phải gợi ý: Module 3 bắt
>    buộc đọc nó, vì bảng `ivr-cancel-reason-map.v1` khoá theo action — xem khối đính chính trong §4.3.

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

### 4.8. Correction W-0149 — revoke/freshness không được suy từ ACK callback

Audit ngày `03/09/2026` xác nhận IVR current không có revoke/update command hoặc business recheck từ
scheduler claim tới dial. ACK `BLOCKED_BY_CORE`/`REJECTED_STALE` ở §4.7 chỉ là outcome của M3 khi xử
lý callback; không phải ACK cho một revoke command và không được dùng để tuyên bố task đã bị dừng.

Snapshot Module 3 `PhucApu@a3aad246d986` chưa có exact hit cho generic Target V1 callback consumer
hoặc các ACK trên. M3 phải cung cấp code/OAS/CDC khác nếu implementation nằm ngoài snapshot này.
Trước khi có bằng chứng đó, D-06 là **contract requirement chưa được chứng minh runtime**.

Nếu Owner giữ phương án A, M3 phải hoàn thành §4.6 và shared E2E trước production. Nếu chọn B/hybrid,
hai bên phải ký `RVK-01..RVK-12` trong
[M8-09 decision pack](../plan/ivr-orther/m8-09-revoke-freshness-decision-pack-2026-09-03.md)
trước mọi thay đổi OpenAPI/runtime/DB. Không tái dùng intake POST hoặc admin terminate.

> *Sửa `25/09` (`W-0354`): đoạn trên viết ngày `03/09`, khi chưa ai chọn phương án. Ngày `09/09` phía
> M8 chọn **B** (`W-0248`) và chạy migration `W0249` (ba cột revoke, hai fence) **trước** khi hai bên
> ký `RVK-01..RVK-12`. Phía M8 đã nhận đó là sai ([phản hồi `16/09`](../plan/PHAN-HOI-toan-viec-can-lam-m8-2026-09-16.md),
> mục `C13`). Vì vậy vế "trước mọi thay đổi runtime/DB" không còn mô tả đúng: thay đổi DB/runtime phía
> M8 đã có — xem khối đồng bộ `W-0304` ngay dưới. Vế còn đúng: **OpenAPI**. Endpoint nhận lệnh thu hồi
> chỉ được mở sau khi M3 trả lời `M3-14` (`IR-07`); phần `RVK-*` của M3 (`D-06`, revalidate) vẫn chờ M3
> ký. Intake POST và admin terminate vẫn không được dùng làm lệnh thu hồi.*

**Correction `W-0147` / M8-07:** phía M8 đã sửa local defect từng bỏ qua `Retry-After` trên `429`.
Transport nay chuyển positive server delay sang dispatcher; `next_retry_at` không sớm hơn delay đó,
vẫn giữ bounded retry budget và cùng immutable key/body. Phần M8 local đã có unit/contract proof,
nhưng endpoint generic M3, auth thật, sandbox và shared E2E vẫn chưa được cung cấp. Xem
[M8-07 handoff](../plan/ivr-orther/m8-07-target-v1-shared-callback-handoff-2026-09-03.md).

> **Đồng bộ `W-0304` — hai fence thu hồi đã cài từ `09/09/2026` (`W-0249`).**
>
> Mục này được viết khi phía M8 chưa có gì. Nay đã có, nên câu mở đầu — *"không có revoke/update
> command hoặc business recheck từ scheduler claim tới dial"* — chỉ còn đúng cho **command đến từ
> M3**, và **không** còn đúng cho hàng rào phía M8:
>
> | Fence | Nằm ở | Bắt được gì |
> | --- | --- | --- |
> | 1/2 | `PostgresSchedulerStore` — lúc scheduler claim | đơn đã thu hồi **trước** khi claim |
> | 2/2 | `PostgresTelephonyDispatchStore` — lần đọc task cuối ngay trước khi quay số | đơn thu hồi **sau** claim, trong vài giây claim-to-dial |
>
> Ba cột revoke do migration `20260909034715_W0249OrderRevocationFence` thêm.
>
> **Phần còn thiếu vẫn nằm ở M3, và là phần quyết định:** chưa có endpoint để M3 báo thu hồi. Hai
> fence chỉ **đọc** cột trong DB của IVR; hiện không có đường nào từ ngoài ghi vào cột đó. M8
> **không** tự dựng endpoint thay M3 — làm vậy là bịa ra một hợp đồng M3 chưa ký.
>
> Vì vậy `D-06` vẫn là *contract requirement chưa được chứng minh runtime*, nhưng lý do nay hẹp hơn
> hẳn: thiếu **đường vào**, không phải thiếu **hàng rào**.
>
> *Đính chính `25/09` (mục `C13` trong danh sách chief): đoạn "Phần còn thiếu vẫn nằm ở M3" đọc như thể
> Module 3 phải dựng endpoint thu hồi. Không phải: **IVR** là bên mở endpoint nhận lệnh thu hồi
> (`IR-07` `E-2`), sau khi Module 3 trả lời `M3-14` — shape `task_id` + `order_version` + `reason`.
> Phần của Module 3 là trả lời `M3-14` rồi **gọi** endpoint đó khi hủy đơn hoặc bật `sale_lock`. Điều
> IVR không làm là mở endpoint **trước** câu trả lời `M3-14`, vì như vậy là tự chốt một shape Module 3
> chưa ký.*

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

> ### ⚠️ Bẫy 2 — `X-Actor-Id` bắt buộc trên **31/33** endpoint quản trị, không riêng tầng danger
>
> Kể cả `GET /queue` và `GET /dashboard` cũng đòi header này. Thiếu là **403**, không phải 401.
>
> Chỉ hai endpoint không đòi: `GET feature-flags/{environment}` và `GET feature-flags/{environment}/kill-switch`.
>
> *Sửa `25/09` (`W-0354`): bản trước ghi `29/31`, đếm trước khi có `GET /audit-evidence` (`draft.29`) và
> `POST /result-callbacks/{callbackId}:replay` (`draft.32`). Hai con số đều tính cả ba route `/dev` chỉ có
> ngoài production. `GET /audit-evidence` thiếu header này cũng trả `403 IVR_FORBIDDEN_CALLER`: đó là lỗi
> **thiếu header**, không phải token thiếu quyền.*
>
> Khác biệt giữa các tầng nằm ở `X-Action-Reason`: chỉ tầng `danger` mới đòi thêm nó.

### 4A.3. Bảng endpoint → tầng bắt buộc

Đường dẫn gốc: `{ivr_base_url}/v1/ivr/order-confirmation`

**Tầng `read` — 16 endpoint**

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
| `GET` | `/audit-evidence` | Có |
| `GET` | `/scripts/{templateId}/{version}` | Có |
| `GET` | `/feature-flags/{environment}` | **Không** |
| `GET` | `/feature-flags/{environment}/kill-switch` | **Không** |

`GET /analytics/export` cố tình là `GET` chứ không phải `POST`: bản trích xuất là thao tác **đọc có audit**, giữ verb read-only để bảo toàn tính chất "nhóm báo cáo không có bề mặt ghi".

`GET /audit-evidence` (**mới `W-0307`**, `draft.29`) cũng vậy, và có ba điểm M3 phải biết khi dựng màn hình cho nó:

1. **`target_type` và `target_id` đều bắt buộc.** Gọi mà thiếu một trong hai trả `400`. Không có chế độ *"xem tất cả"* — một lượt đọc không bộ lọc là trích xuất hàng loạt sổ audit, và đó không phải thứ một màn hình tra cứu cần.
2. **`reason` bắt buộc** (`≥ 8` ký tự), và **chính lượt đọc ghi một dòng audit** — id trả về trong `access_audit_id`. UI nên bắt người dùng nhập lý do trước khi gọi, chứ đừng điền sẵn một chuỗi mặc định: một lý do do máy điền không ghi lại điều gì cả.
3. **Đọc lịch sử của một đối tượng sẽ làm chính lịch sử đó dài thêm một dòng.** Đọc hai lần thì lần sau nhiều hơn đúng một hàng `IVR_AUDIT_EVIDENCE_READ`. Đây là **cố ý**: ai đã xem một bản ghi cũng là một phần của những gì đã xảy ra với bản ghi đó. Đừng thiết kế UI giả định số dòng ổn định giữa hai lần tải.

`truncated` là một field trong phản hồi — **đừng suy từ `rows.length == limit`**, phép suy đó sai đúng lúc số dòng tình cờ bằng `limit`.

**Tầng `write` — 5 endpoint** *(+3 endpoint dev chỉ tồn tại ngoài production)*

| Method | Đường dẫn | Ghi chú |
| --- | --- | --- |
| `POST` | `/admin-reviews` | Mở phiếu review |
| `POST` | `/scripts` | Tạo draft kịch bản. Cần `X-Script-Permissions` (`IVR_SCRIPT_EDIT`) |
| `POST` | `/scripts/{templateId}/{version}:submit` | Trình duyệt. Cần `X-Script-Permissions` (`IVR_SCRIPT_REVIEW`) |
| `POST` | `/scripts/{templateId}/{version}:approve` | Cần `X-Script-Permissions` — §4A.5 |
| `POST` | `/scripts/{templateId}/{version}:retire` | Cần `X-Script-Permissions` |
| `POST` | `/dev/seed:load` | **Chỉ non-production** |
| `POST` | `/dev/scenarios/{scenarioId}:dry-run` | **Chỉ non-production** |
| `POST` | `/dev/integration-profiles/{profileId}:apply` | **Chỉ non-production** |

Ba route `/dev` **không được đăng ký** khi môi trường là production hoặc khi đang cho phép gọi khách thật. Chúng không tồn tại chứ không phải bị chặn — M3 sẽ nhận `404`, không phải `403`.

**Tầng `danger` — 9 endpoint** *(mọi endpoint đều đòi thêm `X-Action-Reason`)*

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
| `POST` | `/result-callbacks/{callbackId}:replay` | **Gửi lại cho Sales một kết quả đã chết** |

`POST /result-callbacks/{callbackId}:replay` (**mới `W-0203`**, `draft.32`) đưa một callback đã chết (`RETRY_EXHAUSTED` hoặc `INVALID_DEAD_LETTER`) trở lại hàng đợi gửi. Trước bản này việc đó chỉ làm được bằng một lệnh `UPDATE` gõ thẳng vào database. Payload, hash và idempotency key giữ nguyên, nên Sales nhận lại đúng các byte của lần gửi đầu; lần gửi lại mà Sales đã xử lý là bản trùng để Sales nhận ra theo idempotency key. `409` và không ghi gì khi callback chưa chết hoặc task xác nhận của nó không còn. Mục review mà dead letter đã mở vẫn mở, đóng qua `/admin-reviews` sau khi giao được.

> **Bổ sung `25/09` (`W-0354`, mục `C21` trong danh sách của chief) — phát lại muộn.** Endpoint **không** giới hạn
> tuổi của callback: một callback chết từ nhiều ngày trước vẫn gửi lại được. `M3-10` đề xuất M3 giữ
> `Idempotency-Key` tối thiểu `7` ngày. Phát lại sau thời hạn đó thì M3 không còn chặn trùng bằng key được nữa;
> chỉ còn revalidate (`M3-11`). Vì vậy consumer của M3 phải xét `order_version` và trạng thái đơn trên **mọi**
> callback, kể cả callback trông như lần đầu. Đơn đã hết hạn, đã hủy hoặc đã đổi version thì trả `REJECTED_STALE`
> hoặc `BLOCKED_BY_CORE`, **không** xác nhận lại. Thêm vào `D-5` một ca: phát lại một `IVR_CONFIRMED` sau khi đơn
> đã hết hạn ⇒ M3 trả `REJECTED_STALE`. Phía IVR chưa đặt giới hạn tuổi; sẽ đặt khi M3 chốt thời hạn giữ key ở
> `M3-10`, để hai con số khớp nhau.

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
| 2 | `X-Actor-Id` có mặt và ≤ 128 ký tự không? | Sai ở 31/33 endpoint, kể cả `GET /audit-evidence` |
| 3 | `X-Actor-Id` có dính bộ lọc PII không? | Sai **chỉ với một số nhân viên** — xem Bẫy 3 |
| 4 | Endpoint tầng danger: có `X-Action-Reason` chưa? | Chỉ sai ở nhóm danger |
| 5 | Token có đủ tầng cho endpoint không? | Token read gọi endpoint danger |
| 6 | Endpoint kịch bản: có vi phạm luật bốn mắt không? | Chỉ sai ở `:approve` |

Nguyên nhân 1 và 5 chặn ở tầng policy, trước khi handler chạy. Nguyên nhân 6 chặn trong domain, sau khi đã qua auth.

### 4A.7. Những thứ **không còn tồn tại** — đừng đi tìm

| Đã xoá | Thay bằng |
| --- | --- |
| Hệ thống tài khoản/vai trò console — **ở tầng runtime và API** | Không có. M3 giữ tài khoản. *Về DB thì đọc ghi chú ngay dưới bảng* |
| **11 endpoint trong OpenAPI**: `/auth/sign-in`, `/auth/session`, `/auth/sign-out`, sáu route `/accounts*`, `/account-roles` | Không có |
| **15 schema `Console*`**, tham số `AccountId`, response `ConsoleAccountError` | Không có |
| Hai mã lỗi `IVR_ACCOUNT_CONFLICT`, `IVR_ACCOUNT_POLICY_VIOLATION` | Danh mục lỗi còn **16 mã** |
| Trang `/login`, `/accounts`, `/roles`, `/profile`, session cookie, màn hình đổi mật khẩu | Không có |
| Catalogue 19 permission và policy theo từng permission | Ba tầng ở §4A.1 |
| Header `X-Permissions` (seam mock cũ) | Đã gỡ, không còn được đọc ở bất kỳ đâu |

> **Bổ sung `25/09` (`W-0354`) — chuỗi `(perm IVR_…)` trong `summary` của OpenAPI: phần lớn là nhãn,
> riêng `IVR_SCRIPT_*` là quyền được kiểm thật.** Catalogue permission đã xoá (dòng trên), nhưng nhiều
> operation vẫn ghi một chuỗi `perm` trong summary. `IVR_CALLBACK_REPLAY` mới ở `draft.32` cũng vậy.
> Cách đọc thống nhất:
>
> - Endpoint **ghi** của hàng đợi, SIM, retry thủ công, review, cắt cuộc gọi và phát lại callback: chuỗi
>   này là nhãn IVR đóng lên dòng admin action để audit (`AdminAction.Permission`).
>   `InternalAdminApiService` từ chối lưu action nào mang nhãn khác với operation của nó.
> - `IVR_QUEUE_VIEW` (các endpoint đọc, kể cả `GET /audit-evidence`) và `IVR_FLAG_READ` (hai route
>   `GET /feature-flags/…`) là tên cũ còn sót, không được đóng lên đâu.
> - `IVR_RUNTIME_GATE_ADMIN` (`POST /feature-flags/{environment}`) cũng không được đóng lên đâu, nhưng
>   endpoint của nó có một cổng thật: **phê duyệt runtime**. Dòng phê duyệt `RUNTIME_GATE_ADMIN` cấp theo
>   `OD-V1-20` (seed ở `W0195`) đã bị thu hồi ngày `16/09` (`W-0301`) và không migration nào cấp lại, nên
>   một môi trường dựng từ migration **không có** phê duyệt sống: mọi thay đổi feature flag không thuần
>   giảm rủi ro — ví dụ tắt kill switch, mở rộng allowlist — bị `409 IVR_OPERATIONAL_BLOCKED`, dù token
>   và header đều đúng. Thay đổi thuần giảm rủi ro, như bật kill switch, không cần phê duyệt. Chữ
>   *"granted to Admin per OD-V1-20"* trong summary OpenAPI đã cũ, sửa ở lần bump contract kế tiếp.
> - **`IVR_SCRIPT_*` không phải nhãn.** Đó là giá trị header `X-Script-Permissions` (§4A.5), và IVR
>   **kiểm thật**: `ScriptLifecycleApiService` chỉ trao cho actor những quyền có trong header, rồi domain
>   đòi đúng quyền cho từng bước. Module 3 **phải** ánh xạ vai trò của mình sang bảy giá trị này cho bốn
>   endpoint kịch bản ghi — tạo draft, `:submit`, `:approve`, `:retire`. Thiếu giá trị mà bước đó cần thì
>   nhận `403 IVR_FORBIDDEN_CALLER`.
> - Hai lượt **đọc** có ghi audit, cả hai bắt buộc lý do `≥ 8` ký tự: `GET /audit-evidence` ghi
>   `IVR_AUDIT_EVIDENCE_READ`, `GET /analytics/export` ghi `IVR_ANALYTICS_EXPORT`.
>
> Ngoài `IVR_SCRIPT_*`, không chuỗi nào cấp quyền gì: quyền đến từ tầng của token (§4A.1). Nếu sau này
> cần biến một chuỗi khác thành quyền thật, việc đó phải qua một quyết định riêng như `OD-V1-20`, không
> thêm ngầm qua summary của OpenAPI.
>
> *Sửa cùng ngày (`25/09`, mục `B9` trong danh sách chief): bản đầu của khối này ghi "M3 không cần ánh
> xạ vai trò của mình sang các chuỗi này" — làm theo thì mọi thao tác ghi kịch bản bị `403` — và ghi
> "riêng lượt đọc `audit-evidence` ghi một dòng audit", bỏ sót `analytics/export`. Bản đầu cũng thiếu
> `IVR_FLAG_READ` và `IVR_RUNTIME_GATE_ADMIN`.*

> **Đính chính `2026-09-07` (`W-0211`) về dòng đầu bảng.** Trước đây dòng đó ghi *"Bảng tài khoản
> console và bảng vai trò trong DB — **đã xoá**"*. Đúng ở tầng M3 quan tâm (không endpoint, không
> auth, không màn hình) nhưng **sai ở tầng DB**: `W0122` nay có `Up()` rỗng và
> `P03PreserveConsoleCompatibility` chạy `CREATE TABLE IF NOT EXISTS ivr_console_accounts` và
> `ivr_console_sessions`. **Hai bảng đó vẫn tồn tại**, cố ý, để cửa sổ rollback của helm còn dùng
> được. Không có auth nào được bật lại và không có route nào đọc chúng — nhưng nếu M3 (hoặc ai đó
> soi DB) thấy hai bảng này thì chúng **không phải** tàn dư bị bỏ quên.

> **Quan trọng cho M3 nếu đã sinh client từ OpenAPI.** Bản spec anh nhận trước ngày 28/08/2026
> (`1.0.0-draft.21` trở về trước) **vẫn còn** 11 endpoint đó. Sinh client từ bản cũ sẽ ra
> `signInConsoleAccount()`, `listConsoleAccounts()`, `createConsoleAccount()`… — gọi vào là `404`.
>
> Lấy lại spec ở `specs/api/openapi/ivr-order-confirmation.v1.yaml`, phiên bản **`1.0.0-draft.33`**,
> rồi sinh lại. So sánh đầy đủ nằm ở hai changelog nối nhau:
> `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.20-to-v1.0.0-draft.22.md`,
> `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.22-to-v1.0.0-draft.23.md`, rồi
> `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.23-to-v1.0.0-draft.24.md` — bản cuối
> **có breaking**, xem `§3.4.2`.
> Bản `.22→.23` là nơi các route feature-flag nhận ràng buộc
> `minLength:1 / maxLength:128 / pattern:'^[A-Za-z0-9._:-]+$'` cho `x-correlation-id` — cùng luật mà
> route intake vẫn chưa khai (xem §3.1.1).

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

## 6. `dial_token` — đã chốt hợp đồng, còn lại là vận hành

**Correction `W-0150` (03/09/2026):** production path vẫn fail-closed và chưa được phép code.

> **Cập nhật `W-0250` (09/09/2026) — câu dưới đây đã hết đúng.** Bản `draft.24` đóng
> `phone_validation_status` thành `required` + `enum: [VALID]`, nên OpenAPI **không còn** cho phép
> thiếu hay sai giá trị: schema chặn trước service, trả `400 IVR_MALFORMED_REQUEST` thay vì
> `422 IVR_CONTACT_INVALID`. Phần TTL ba guard bên dưới **vẫn đúng**.

OpenAPI *trước* `draft.24` cho phép thiếu `phone_validation_status` nhưng runtime chỉ nhận exact `VALID`;
intake + persistence **+ dispatch** cùng nhau ép token expiry bằng đúng confirmation-window end
(ba guard, xem §3.4.1 — guard thứ ba ở `PostgresTelephonyDispatchStore.LoadAsync` nghĩa là sửa
OpenAPI và persistence thôi thì cuộc gọi vẫn hỏng lúc dial). MOCK/LAB chỉ ngăn
resolve lặp theo `(token fingerprint, attempt_id)` và có thể reuse scalar token ở attempt khác.
`DialAuthorization` current chỉ nhận opaque provider destination reference, không nhận E.164.
`DTK-01..DTK-15` trong
[M8-10 decision pack](../plan/ivr-orther/m8-10-contact-dial-token-production-decision-pack-2026-09-03.md)
là chi tiết vận hành **dưới** ba quyết định đã ký `OD-V1-05` / `OD-V1-17` / `OD-V1-18`; phần còn lại
quyết giữa **owner IVR và dev Module 3**, không chờ đội Security/Platform/Telephony nào — xem `§9a`.

> *Đính chính `25/09` (mục `A1` trong danh sách chief): tiêu đề mục này ("đã chốt hợp đồng"), cụm "ba
> quyết định đã ký" ở câu trên và cột "Thực ra đã quyết ở đâu" của correction `W-0304` bên dưới nay đọc
> là **vị trí phía M8**, chưa phải quyết định đã chốt: `OD-V1-05` là
> `M8_POSITION_SIGNED / M3_NOT_RECEIVED`; `OD-V1-17/18` phụ thuộc phương án B về số điện thoại, chờ
> Sếp trả lời mục `B2` phiếu Sếp `25/09`. Ba guard ở §3.4.1 vẫn thi hành như cũ.*

**Correction `W-0208` (07/09/2026) — đã giải quyết `2026-09-09` (`W-0246`):** `OD-V1-17` ký ngày
05/09 chọn phương án *"token reusable theo TTL"* với **TTL = cửa sổ xác nhận + 60s** (nguyên văn ở
`specs/_review/open-decisions-register.md`). Con số đó mâu thuẫn với ba guard đang chạy và chưa bao
giờ thi hành được. **Owner chốt `09/09`: TTL = đúng window end**, thay thế vế `+60s`. Không tầng nào
phải sửa, và M3 không phải đổi gì. Xem §3.4.1.

> Một lượt trung gian (`W-0245`) từng khẳng định `+60s` *"không tồn tại trong bất kỳ tài liệu ký
> nào"*. **Sai** — nó nằm trong register, chính là nguồn `od-v1-signoff` khai ở header, và là nguồn
> lượt đó không kiểm. Đã rút.

Task hiện mang một `dial_token`, nhưng một task có thể cần nhiều attempt và retry kỹ thuật. Module 3 + Security cần chọn một trong các phương án:

| Phương án | Đánh đổi |
| --- | --- |
| `dial_tokens[]` per-attempt | Phải dự đoán đủ số lần retry |
| Endpoint reissue/refresh | Có round-trip đồng bộ trước khi quay |
| Token bundle | Vẫn phải sizing cho retry kỹ thuật |
| Token reusable theo TTL + risk control | Bỏ one-use; cần threat model và audit |

Cần xác nhận thêm:

- service nào giữ mapping `dial_token → provider destination/E.164`, và IVR có được thấy gì;
- ai vận hành/audit vault;
- TTL và số lần resolve;
- cơ chế rotation/revocation;
- hành vi khi token hết hạn giữa các attempt.

Không gửi số E.164 trực tiếp trong task.

> **Correction `W-0304` (16/09/2026) — bốn phương án và năm câu hỏi ngay trên đây đã được quyết.**
>
> Tiêu đề mục này ghi *"đã chốt hợp đồng, còn lại là vận hành"*, trong khi thân mục vẫn trình bày
> như thể còn đang cân nhắc. Đó là mâu thuẫn nội bộ của chính tài liệu này: người đọc sau sẽ tưởng
> mình còn được chọn, và sẽ chọn lại thứ đã ký.
>
> | Câu hỏi để mở ở trên | Thực ra đã quyết ở đâu |
> | --- | --- |
> | Chọn phương án nào trong bảng bốn dòng | `OD-V1-17` — **token reusable theo TTL**; ba phương án kia đã loại |
> | Service nào giữ mapping `dial_token` → E.164, IVR được thấy gì | `OD-V1-05` / `OD-V1-18` — resolver nằm **trong tiến trình IVR** |
> | Ai vận hành/audit vault | `OD-V1-05` — không có vault ngoài, và không có đội Security/Platform nào trong tổ chức này (xem §7) |
> | TTL và số lần resolve | `OD-V1-17` + `W-0246` — TTL = **đúng** confirmation-window end; trần resolve = `max_customer_attempts` + technical retry |
> | Rotation/revocation, token hết hạn giữa hai attempt | `OD-V1-18` + §3.4.1 — ba guard ép đẳng thức; lệch ⇒ `422 IVR_CONTACT_INVALID` với hai mã riêng cho sớm/muộn (`W-0302`) |
>
> **Một nguồn sự thật duy nhất cho vị trí của `E.164`: `specs/api/04-sim-adapter-contract.md` §2.**
> Nguyên văn ở đó: số E.164 tồn tại **chỉ trong bộ nhớ tiến trình** cho đúng một lần quay số —
> không ghi DB, không vào log, không vào evidence, không vào callback payload.
>
> Câu *"Không gửi số E.164 trực tiếp trong task"* ngay trên là **hệ quả** của quy tắc đó, không
> phải một quy tắc thứ hai. Khi hai chỗ cùng nói về E.164, sửa API-04 trước rồi để chỗ này theo sau
> — đừng sửa ngược, và đừng sửa một chỗ.

---

## 7. Auth production

> **Cập nhật `W-0254` (09/09/2026).** Mục này từng ghi *"Cần Security/Platform cung cấp"*. Không có
> đội Security hay Platform nào trong tổ chức này — chỉ có owner IVR, và dev Module 3. `OD-V1-07`
> **đã `CLOSED` từ 2026-09-05** do owner ký. Phần dưới là hồ sơ đã ký, không phải yêu cầu đang chờ.

> *Đính chính `25/09` (mục `A1` trong danh sách chief): `OD-V1-07` **không còn** `CLOSED`. Chief chốt
> ngày `25/09`: chờ Tech Lead ký, sau dòng ủy quyền `N14` phiếu Sếp `25/09`. Bảng dưới là vị trí phía
> M8, chưa phải hồ sơ đã ký — và câu "không phải một quyết định đang chờ ai đó ký" ở gần cuối mục này
> vì thế cũng hết đúng.*

**Vị trí phía M8 (`OD-V1-07`, `2026-09-05`) — chưa chốt, xem đính chính ngay trên:**

| Hạng mục | Vị trí M8 |
| --- | --- |
| Thuật toán | JWT **ký khóa bất đối xứng**, phát hành qua JWKS |
| TTL token | **≤ 10 phút** |
| Scope bắt buộc | `ivr.task.write` |
| Token tĩnh dùng chung | **từ chối dứt điểm** khi provider là `TARGET_V1` |
| mTLS | **hoãn** tới khi có hạ tầng thật — không phải điều kiện để bắt đầu tích hợp |

**Còn phải làm, và là việc vận hành chứ không phải quyết định:**

- dựng issuer/JWKS thật và cấp **sandbox credential** cho dev M3;
- chốt ngày tắt `X-Internal-Token` compatibility.

Scope tối thiểu hai chiều:

- `ivr.task.write`: Module 3 → IVR task intake;
- `ivr.result.write`: IVR → Module 3 result callback.

Chưa có sandbox credential thì chưa chạy được integration test thật — nhưng đó là hạ tầng chờ dựng,
**không** phải một quyết định đang chờ ai đó ký.

**Mục này chỉ nói về hai API nghiệp vụ ở §3–§4.** Bề mặt quản trị (§4A) dùng **credential riêng, không dùng chung** với service JWT ở đây: ba token tĩnh `IVR_ADMIN_READ/WRITE/DANGER_TOKEN`. Hai hệ credential tách rời có chủ đích — token mà giao diện quản trị cầm không được đồng thời giao được task, và ngược lại.

Khi dựng issuer/JWKS thật, cần trả lời thêm cho §4A: ba token này có chuyển sang cùng issuer/JWKS không, ai cấp và xoay vòng thế nào, và cất ở đâu phía Module 3. Owner IVR quyết hai câu đầu, dev M3 quyết câu cuối.

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
| **4** | Ký wire mapping program/payment/state và nguồn `ivr_confirmation_required`. Business pair **đã có nguồn** (Flow 04/05, xem §3.10 R3) nên không cần Product quyết lại; còn lại là ký chuỗi trên dây theo **§3.11** và gắn `attempt_policy_version` | M3 + Product | `M8_SIGNED_W0145 / M3_PRODUCT_ARTIFACT_REQUIRED` |
| **4b** | Sửa 3 field lệch chuỗi ở **§3.11** — M3 map `24_7`→`TWENTY_FOUR_SEVEN`, `PHONE_VALID`→`VALID`, `ELIGIBLE_FOR_IVR`→`ELIGIBLE` | M3 | `IMPLEMENTATION_ALIGNMENT_REQUIRED` |
| **4c** | Đồng ký `golden_hour_session_id`, namespace/program semantics, store→enforce cutover và producer CDC theo §3.5A | M3 + M8 | `M8_POSITION_SIGNED_W0146 / M3_CONTRACT_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED` |
| **5** | Auth profile + sandbox credential | Tech Lead ký `OD-V1-07`; owner IVR dựng | ⏳ `OD-V1-07` chờ Tech Lead ký, sau dòng ủy quyền `N14` phiếu Sếp `25/09`; vị trí phía M8 ở `§7`. Sau chữ ký còn **dựng** issuer/JWKS và cấp sandbox credential. *Sửa `25/09` (mục `A1` trong danh sách chief): bản trước ghi `OD-V1-07 CLOSED 2026-09-05`* |
| **6** | Ký minimal `eligibility_snapshot` dùng làm evidence, không phải IVR business decision | M3 + M8 | `OWNER_SIGNOFF_REQUIRED` |
| **7** | Chọn `dial_token` model và trust boundary | owner IVR | ⏳ Vị trí phía M8, ký `2026-09-05` (vế TTL chốt lại `2026-09-09`, `W-0246`): Sales cấp token lúc tạo task; token dùng lại được, gắn cứng `task_id`, TTL = **đúng** confirmation-window end, trần resolve = `max_customer_attempts` + technical retry; resolver **trong** IVR. **Chưa chốt:** `OD-V1-05` là `M8_POSITION_SIGNED / M3_NOT_RECEIVED`; `OD-V1-17/18` phụ thuộc phương án B về số điện thoại, chờ Sếp trả lời mục `B2` phiếu Sếp `25/09`. *Sửa `25/09` (mục `A1` trong danh sách chief): bản trước ghi cả ba `CLOSED`, và ghi "E.164 chỉ trong bộ nhớ tiến trình" — vế đó đã bị phương án B thay từ `17/09`, chính phương án đang chờ Sếp* |
| **7a** | `DTK-01..DTK-15` — chi tiết vận hành dưới `OD-V1-05/17/18` | owner IVR + dev M3 | `W0150_EVIDENCE_SUBMITTED` — phần **hợp đồng** theo dòng 7, chưa chốt; phần còn lại là custody/rollout, quyết giữa owner và dev M3, **không** chờ đội ngoài. *Sửa `25/09`: bản trước ghi "đã ký" và "phần hợp đồng đã đóng ở dòng 7"* |
| **7b** | Ký `ATP-01..ATP-15`: authority/version bundle, program matrix/T0, counting/retry/quiet-hours, wire/producer, registry lifecycle, cutover/pre-dial coherence, capacity/audit/rollback | Product + Order Core + M3; Platform/M8/Release ở dòng kỹ thuật | `W0151_EVIDENCE_SUBMITTED / M3_ATTEMPT_POLICY_PRODUCER_NOT_FOUND / PRODUCTION_POLICY_NOT_APPROVED / CODE_NOT_AUTHORIZED` |
| **8** | Duyệt lời thoại/privacy và giới hạn `items[]` | owner IVR | ✅ `OD-V1-11 CLOSED 2026-09-10` — owner tuyên bố quorum là chính mình. Chính sách đã chốt: ghi âm TẮT vĩnh viễn ở V1. Metadata **không đặt kỳ hạn xoá** (sửa `17/09` theo quyết định giữ toàn bộ dữ liệu, bản ký `10/09` ghi `90` ngày) — vế giữ vĩnh viễn này **chưa có chữ ký Sếp**, chờ phiếu Sếp `25/09` mục `B2`. **Duyệt script cho `PRODUCTION_REAL` vẫn chặn** vì luật ba-actor, xem `§9a`. *Sửa `25/09` (mục `A1` trong danh sách chief): bản trước gộp vế giữ vĩnh viễn vào "chính sách đã chốt"* |
| **9** | Nhận bàn giao bề mặt quản trị **§4A**: ai giữ ba token, vai trò M3 nào ánh xạ sang tầng nào, định dạng `X-Actor-Id` | owner IVR + dev M3 | `OWNER_DECISION_REQUIRED` — thật sự còn mở, và quyết được ngay giữa hai bên |

Chưa được gọi integration/production ready khi các gate P0 trên chưa đóng.

### 9a. Ba mục treo trên một quorum không tồn tại

> *Đính chính `25/09` (mục `A1` trong danh sách chief): số đếm và các chữ `CLOSED` trong mục này là
> trạng thái ngày `09–10/09`. Sau đó nhiều dòng đã đổi — đổi nhãn `16/09`, chốt chief `25/09` (xem `§0`
> và các dòng 5, 7, 8 của bảng trên). Trạng thái hiện hành đọc ở sổ quyết định, không ở mục này.*

`W-0254` đối chiếu bảng trên với `specs/_review/open-decisions-register.md`. **23 trong 28** quyết
định đã `CLOSED`. Năm mục còn lại, đọc kỹ thì chỉ **hai** là còn việc thật:

| Mục | Trạng thái ghi trong register | Thực chất |
| --- | --- | --- |
| `OD-V1-09` — giao thức SIM lab | `HALF_SIGNED` | **việc thật**: giao thức đã ký, còn chờ SIM/nhà mạng thật |
| `OD-V1-10` — 32 eSIM capacity | `NOT_SIGNED` **cố ý** | **việc thật**: con số 32 là giả định chưa đo; ký bây giờ là ký một điều chưa biết |
| `OD-V1-11` — script/legal/retention | `CONTENT_SIGNED / APPROVER_QUORUM_UNRESOLVED` | owner **đã ký nội dung**; treo vì chờ *Legal/Privacy* |
| `OD-V1-21` — GitLab provisioning | `SIGNED_EXCEPT_INDEPENDENT_APPROVAL` | owner **đã ký**; treo vì chờ *independent approver* |
| `OD-V1-23` — opt-out boundary | `OWNER_POSITION_SIGNED / QUORUM_PENDING` | owner **đã ký**; treo vì chờ *Product + CRM + Legal/Privacy* |

Ba dòng cuối treo trên **cùng một thứ**: một quorum gồm những vai không tồn tại trong tổ chức này.
Cast thật là **owner IVR** (Toàn), **dev Module 3**, và các bên **thật sự** bên ngoài — nhà mạng cho
trunk thoại, và một ý kiến pháp lý mua ngoài nếu owner muốn.

### Đã quyết `2026-09-10`: owner tuyên bố quorum là chính mình cho cả ba

Cả ba chuyển sang `CLOSED`. Bảng gate không còn dòng nào chờ người không tồn tại; sổ quyết định đi
từ **5 mục mở xuống 2** (`OD-V1-09` chờ SIM thật, `OD-V1-10` cố ý chưa ký vì con số 32 chưa đo).
Owner chọn **không** mua ý kiến pháp lý ngoài cho V1 và nhận rủi ro pháp lý của `OD-V1-11`.

**Đóng quyết định không có nghĩa là mọi thứ mở ra.** Hai trong ba mục còn vướng ràng buộc **kỹ
thuật** mà chữ ký không gỡ được, và ghi ra đây để không ai đọc `CLOSED` rồi hiểu nhầm:

| Mục | Đóng cái gì | **Không** đóng cái gì |
| --- | --- | --- |
| `OD-V1-11` | Chính sách ghi âm và thời hạn lưu | `PRODUCTION_REAL` vẫn cần **ba actor id khác nhau** (`ScriptContentContracts.EnsureApprovalAllowed`); một người thì code **vẫn từ chối** duyệt script cho production |
| `OD-V1-21` | Quyết định cấu hình GitLab | Bằng chứng four-eyes cần **Premium/Ultimate + reviewer thứ hai** — là giới hạn được **chấp nhận**, không phải điều kiện đã thoả |
| `OD-V1-23` | Ranh giới opt-out là **explicit-only** | V1 **không có tín hiệu opt-out tường minh nào**: `DTMF-0` là phím **hủy đơn**, phím 9 ngoài scope và bị `TargetV1SpeechPolicy.ValidateTemplate` từ chối. Hệ quả thực tế: **V1 không có opt-out**; thêm tín hiệu là một `OD` mới |

Không mục nào trong ba mục chạm vào contract task intake hay callback ở `§3`/`§4`. M3 tích hợp được
ngay với `1.0.0-draft.33`.

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
- [ ] Ký đủ `ATP-01..ATP-15` theo [M8-11](../plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md), đặc biệt canonical two-program version/bundle hash, window/attempts/offsets/T0, counting/retry/quiet-hours, cutover và pre-dial coherence.
- [ ] Giao exact producer commit/OpenAPI/schema/CDC cho `attempt_policy_version`, `max_customer_attempts`, `attempt_offsets_seconds`, `confirmation_window_started_at`, `confirmation_window_expires_at`; payload sandbox phải khớp signed registry snapshot và chứng minh `409 IVR_POLICY_MISMATCH` được xử lý, không retry mù.
- [ ] Chốt cách normalize `delivery_area_short` và giới hạn `items[]`.
- [ ] Xác nhận không gửi `trusted_skip_allowed` (`LEGACY_READ`); trust/risk metadata nếu giữ không được dùng phía IVR để quyết định gọi/skip.
- [ ] Đồng ký đúng field `golden_hour_session_id`; nếu reject phải trả exact replacement + business source, không gửi hai alias.
- [ ] Xác nhận Golden Hour required/non-null và 24/7 prohibited/absent; nêu namespace, thời điểm phát, uniqueness và stability qua retry/replay.
- [ ] Giao producer commit/client revision + store/enforce cutover/rollback + CDC exact SHA trước khi yêu cầu M8 sửa code.
- [ ] Xác nhận không map upstream ID vào `capacity_incident.session_id` current và không synthesize từ task/order/correlation ID.

### Callback

- [ ] Cung cấp OpenAPI endpoint generic `/api/v1/internal/orders/{orderId}/ivr-result-callbacks`.
- [ ] Ký ACK taxonomy và body `code/callback_id/correlation_id`.
- [ ] Ký idempotency boundary và thời gian giữ key.
- [ ] Xác nhận revalidate version/state/inventory/recall/sale-lock/quality-hold trước transition.
- [ ] Xác nhận tôn trọng `is_counted_customer_attempt`.
- [ ] ~~Chốt timeout worker sau `IVR_NO_ANSWER_FINAL`.~~ *Sửa `25/09` (mục `C7` trong danh sách
  chief):* xử lý `IVR_NO_ANSWER_FINAL` ngay khi nhận callback, theo chương trình của đơn — đơn
  `TWENTY_FOUR_SEVEN` (COD) hủy với `IVR_NO_ANSWER_MAX`; đơn `GOLDEN_HOUR` cho xác nhận hết hiệu lực và
  nhả suất theo flow 05 (lý do `IVR_NO_ANSWER_MAX`). **Không** chờ timeout, **không** làm theo
  `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` (§4.4).
- [ ] Giao consumer commit + authoritative OpenAPI/CDC cho cả Golden Hour và 24/7; endpoint compat
  Golden Hour không được dùng thay.
- [ ] Chứng minh `429` trả `Retry-After` hợp lệ và shared E2E persist `next_retry_at` không sớm hơn
  header, với cùng key/body.
- [ ] Chạy shared E2E exact SHA cho accepted/duplicate/conflict/stale/block/review/auth/invalid/outage;
  local fake hoặc Postman screenshot không được tính. *Thêm `25/09` (mục `C21` trong danh sách chief):*
  cả ca **phát lại muộn** — phát lại một `IVR_CONFIRMED` sau khi đơn đã hết hạn ⇒ Module 3 trả
  `REJECTED_STALE` (§4A.3).
- [ ] Cung cấp code pointer exact SHA chứng minh D-06 revalidate version/state/recall/sale-lock/
  quality hold thực sự chạy trước transition; fixture phía IVR không được tính.
- [ ] ~~Ký phương án A/B/hybrid và approval provenance. Nếu B/hybrid, trả đủ `RVK-01..RVK-12`,
  authoritative command OAS/CDC và race/fencing tests trước khi yêu cầu IVR code.~~ *Sửa `25/09`
  (mục `C14` trong danh sách chief): dòng này viết khi chưa ai chọn phương án. Phía M8 đã chọn **B**
  của gói thu hồi `M8-09` (`09/09`, `W-0248`) và đã chạy migration `W0249` — hai fence thu hồi — trước
  khi hai bên ký, xem §4.8.* Việc còn lại của Module 3: trả lời `M3-14` (`IR-07`) — gọi endpoint thu
  hồi khi hủy đơn hoặc bật `sale_lock`, shape `task_id` + `order_version` + `reason`; IVR mở endpoint
  sau câu trả lời đó (`IR-07` `E-2`). Phần `RVK-*` của Module 3 (`D-06`, revalidate) vẫn chờ Module 3
  ký.

### Bề mặt quản trị (§4A — mới 28/08/2026)

- [ ] Xác nhận M3 sẽ dựng giao diện quản trị IVR và nhận bàn giao hợp đồng §4A.
- [ ] Chỉ định nơi cất ba token `IVR_ADMIN_READ/WRITE/DANGER_TOKEN` và chu kỳ xoay vòng.
- [ ] Ký ánh xạ **vai trò M3 → tầng IVR**. Nêu rõ vai trò nào được chạm `danger`.
- [ ] Chốt định dạng `X-Actor-Id` là **id đục**, không phải tên hiển thị (Bẫy 3, §4A.4).
- [ ] Xác nhận UI bắt buộc nhập `X-Action-Reason` trước khi gửi mọi thao tác tầng `danger`.
- [ ] Chỉ định hai người khác nhau giữ quyền duyệt **nội dung** và **privacy/pháp lý** (§4A.5).
- [ ] Xác nhận M3 không kỳ vọng IVR còn màn hình đăng nhập, bảng tài khoản hay endpoint `/api/auth/*` (§4A.7).
- [ ] **Sinh lại client từ OpenAPI `1.0.0-draft.33`.** Bản trước đó vẫn công bố 11 endpoint `auth`/`accounts` nay đã bị gỡ (§4A.7).

### Hạ tầng — owner IVR

Ba dòng cũ ở đây được gửi cho một đội *Platform* không tồn tại. ~~Hai dòng đầu **đã chốt**; chỉ dòng
thứ ba và phần dựng là còn việc.~~ *Sửa `25/09` (mục `A1` trong danh sách chief): hai dòng đầu **chưa**
chốt — xem `§9` dòng 5 và 7.*

- [ ] Chốt `dial_token` model, vault owner và audit boundary — vị trí phía M8 ở `OD-V1-05/17/18`
      (`2026-09-05`); chờ Module 3 (`OD-V1-05`) và chờ Sếp trả lời mục `B2` phiếu Sếp `25/09`
      (`OD-V1-17/18`).
- [ ] Chốt auth profile — `OD-V1-07` chờ Tech Lead ký, sau dòng ủy quyền `N14` phiếu Sếp `25/09`.
      Sau đó còn **dựng** issuer/JWKS và **cấp sandbox credential** cho dev M3.
- [ ] Cung cấp base URL và chính sách versioning/deprecation của OpenAPI.

---

## Ô ký

> **`W-0254`:** hai dòng cuối của ô ký cũ là *Security/Platform* và *Privacy/Legal*. Không có hai vai
> ấy trong tổ chức này, nên để trống chúng là để tài liệu trông như đang chờ ai đó — trong khi thật
> ra không ai sẽ đến. Ô ký nay chỉ liệt kê người có thật.

| Vai trò | Xác nhận | Tên | Ngày |
| --- | --- | --- | --- |
| Owner Module 3 — business decision + producer | ____________ | ____________ | ______ |
| Owner Module 8 / IVR — execution boundary, auth profile, dial-token model, speech payload | ____________ | ____________ | ______ |
| _(tuỳ chọn)_ Ý kiến pháp lý mua ngoài — chỉ cho `OD-V1-11`, xem `§9a` | ____________ | ____________ | ______ |

**Ghi chú chung:** ______________________________________________
