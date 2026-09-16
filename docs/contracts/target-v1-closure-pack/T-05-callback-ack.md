# T-05 — Generic callback target, ACK taxonomy, idempotency/version

External work `W-0005` · quyết định `OD-V1-02` · gate **real integration** · trạng thái `OPEN`

Owner: **Sales API/Core**.

Due: chốt **trước khi gỡ fail-closed guard và bật real `TARGET_V1`**. `P4-1` đã chỉ wiring
provider phía IVR; nó không tạo consumer M3 và không đóng integration gate. Ngày cam kết của owner:
`<owner điền>`.

## 1. Current evidence — đã đọc từ nguồn

**Target V1 callback đã được đặc tả đầy đủ và đã sinh client.** [`order-core-ivr-callback.target-v1.yaml`](../../../specs/api/openapi/order-core-ivr-callback.target-v1.yaml):

- Path: `POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks`
- Header bắt buộc: `Idempotency-Key` (8–200), `X-Correlation-Id` (1–200)
- Body `IvrResultCallbackV1`: 13 field bắt buộc, `additionalProperties: false`
- `ResultType`: **11 giá trị** (`IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, `IVR_NO_ANSWER_ATTEMPT`, `IVR_NO_ANSWER_FINAL`, `IVR_CONFIRMATION_WINDOW_EXPIRED`, `IVR_INVALID_PHONE_FINAL`, `IVR_WRONG_INPUT`, `IVR_TECHNICAL_EXCEPTION`, `IVR_CAPACITY_EXCEPTION`, `IVR_OPERATIONAL_BLOCKED`, `IVR_POLICY_BLOCKED`). Correction `W-0145`: IVR runtime có 9 producer path; chỉ 6 final result vào callback outbox; hai blocked code không phải call result.
- `RecommendedCoreAction`: 7 giá trị, **advisory** (D-02)

**ACK taxonomy:**

| HTTP | Code | IVR làm gì |
| --- | --- | --- |
| `200` | `ACCEPTED` | xong |
| `200` | `DUPLICATE_ACCEPTED` | xong, không gửi lại |
| `200` | `BLOCKED_BY_CORE` | dừng, ghi evidence — [`TargetV1CallbackTransport.cs:142`](../../../src/Ivr.Infrastructure/Callbacks/TargetV1CallbackTransport.cs) |
| `200` | `REVIEW_REQUIRED` | đưa vào hàng đợi review admin |
| `409` | `REJECTED_STALE` | **không** transport-retry |
| `409` | `IDEMPOTENCY_CONFLICT` | **không** transport-retry |
| `401`/`403`/`422` | — | fail, không retry |
| `429` | — | retry theo `Retry-After` |
| `500`/`503` | — | retry có backoff |

**Correction `W-0147` / M8-07:** audit ngày 03/09/2026 phát hiện runtime trước đó phân loại `429`
retryable nhưng bỏ qua `Retry-After`. M8 đã sửa transport/dispatcher để schedule theo delay lớn hơn
giữa local backoff và server `Retry-After`, đồng thời giữ nguyên retry budget, key và immutable body.
Hai test `UT-CALLBACK-RETRY-AFTER-02B/09B` khóa behavior này. Đây là local candidate proof, không
phải shared integration.

**Correction `W-0149` / M8-09:** `BLOCKED_BY_CORE` và `REJECTED_STALE` là ACK của **callback result**,
không phải ACK cho business revoke command. Current IVR không có revoke/update route hoặc business
recheck trước mỗi attempt. Read-only snapshot M3 `PhucApu@a3aad246d986` không có exact hit cho
generic Target V1 callback consumer hay hai ACK code này; vì vậy D-06 runtime vẫn
`NOT_FOUND/NOT_PROVEN`. Xem
[M8-09 decision pack](../../../plan/ivr-orther/m8-09-revoke-freshness-decision-pack-2026-09-03.md).

**Endpoint hiện tại của Sales là một hình dạng khác hẳn.** Fixture compat đã verify tại commit ghim [`specs/api/compat/current-golden-hour-callback.a3aad246.schema.json`](../../../specs/api/compat/current-golden-hour-callback.a3aad246.schema.json):

| | Current Golden Hour | Target V1 |
| --- | --- | --- |
| ID | `callId`, `reservationId`, `orderId`, `customerId` — **`int64`** | `task_id`, `order_id` — **string** |
| Kết quả | `result`: 4 giá trị | `result_type`: **11 giá trị** |
| Stale guard | **không có field version nào** | `order_version_seen_by_ivr` bắt buộc |
| Attempt | không có | `attempt_number`, `is_counted_customer_attempt`, `is_final_for_ivr` |
| Evidence | không có | `evidence_ref`, `audit_ref` bắt buộc |
| Advisory | không có | `recommended_core_action` |

## 2. Target delta — chính xác là gì

**(a) Endpoint generic chưa tồn tại.** Endpoint hiện tại là Golden Hour riêng, và theo `specs/_review/open-decisions-register.md` §Explicit non-decisions, nó **không được nhận kết quả 24/7**. Nghĩa là chương trình 24/7 hiện **không có đường trả kết quả nào cả**. Đây là ticket chặn cứng, không phải tối ưu.

**(b) Schema 11 → 4 là ánh xạ compatibility mất mát, nhưng không được nói sai producer behavior.**
Target V1 giữ đủ 11 code để hai bên hiểu cùng vocabulary. Runtime IVR hiện chỉ enqueue callback
cho 6 final result: `IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, `IVR_NO_ANSWER_FINAL`,
`IVR_CONFIRMATION_WINDOW_EXPIRED`, `IVR_INVALID_PHONE_FINAL`, `IVR_CAPACITY_EXCEPTION`.
`IVR_NO_ANSWER_ATTEMPT`, `IVR_WRONG_INPUT`, `IVR_TECHNICAL_EXCEPTION` được persist non-final nhưng
không callback; hai blocked code không được persist/send như call result. Đường compat vẫn mất
version, counted/final, evidence và advisory semantics. Sales phải ký rõ phần mất mát này; không
được dùng bảng 11 → 4 để tuyên bố cả 11 sẽ xuất hiện trên outbound callback.

**(c) Current GH không có version → không thể có `REJECTED_STALE`.** Trên đường compat, kết quả của một đơn đã đổi trạng thái vẫn được nhận như bình thường. Rủi ro thật: khách bấm "xác nhận" trong lúc CSKH vừa huỷ đơn — Sales không có cách phát hiện.

**(d) `reservationId` và `customerId` không có nguồn trong Target V1.** Current GH bắt buộc hai field này. Target task không mang chúng. Cần chốt: Target V1 có cần chúng không, hay chúng là khái niệm chỉ tồn tại ở đường compat.

**(e) Semantics `DUPLICATE_ACCEPTED` vs `IDEMPOTENCY_CONFLICT`.** Cần Sales nói rõ ranh giới: cùng `Idempotency-Key` **cùng** body → `DUPLICATE_ACCEPTED`; cùng key **khác** body → `IDEMPOTENCY_CONFLICT`. IVR đang giả định vậy; nếu Sales định nghĩa khác, outbox sẽ retry sai.

## 3. Sample payload

```json
{
  "contract_version": "ivr-order-confirmation.v1",
  "callback_id": "cb_01",
  "task_id": "task_01",
  "order_id": "order_01",
  "order_version_seen_by_ivr": "17",
  "result_type": "IVR_CONFIRMED",
  "is_counted_customer_attempt": true,
  "is_final_for_ivr": true,
  "attempt_number": 1,
  "occurred_at": "2026-08-18T08:30:00Z",
  "recommended_core_action": "CORE_REVALIDATE_AND_CONFIRM_ORDER",
  "evidence_ref": "evidence://ivr/task_01/result",
  "audit_ref": "audit://ivr/cb_01"
}
```

ACK mong đợi:

```json
{ "code": "ACCEPTED", "callback_id": "cb_01", "correlation_id": "corr-0001", "order_state": "CONFIRMED" }
```

Trường hợp Sales đã huỷ đơn trong lúc IVR đang gọi:

```json
{ "code": "BLOCKED_BY_CORE", "callback_id": "cb_01", "correlation_id": "corr-0001", "detail": "..." }
```

## 4. Acceptance test — phải xanh khi đóng

| Test | Ở đâu | Khẳng định |
| --- | --- | --- |
| `CT-CONTRACT-TARGET-ACK-04` | [`tests/Ivr.ContractTests/SalesContractScaffoldTests.cs`](../../../tests/Ivr.ContractTests/SalesContractScaffoldTests.cs) | Cả 6 ACK code map đúng hành vi outbox |
| `CT-CONTRACT-TARGET-ERROR-05` | cùng file | 401/403/422/429/500/503 phân loại retry đúng |
| `CT-CONTRACT-CURRENT-06` | cùng file | Đường compat khớp schema ghim `a3aad246` |
| `CT-CONTRACT-SEPARATION-01` | cùng file | Internal API và outbound callback là **hai surface riêng**, map bằng mapper tường minh |
| `IT-CALLBACK-OUTBOX-06` | `tests/Ivr.IntegrationTests/` | Outbox bền, không mất, không gửi trùng |
| `CT-CONTRACT-WIREMOCK-07` | cùng file | Kịch bản WireMock phủ đủ ACK |
| `UT-CALLBACK-RETRY-AFTER-02B` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | Transport giữ positive `Retry-After` từ `429` |
| `UT-CALLBACK-RETRY-AFTER-09B` | cùng file | Dispatcher không retry trước server delay; local backoff vẫn là floor |
| **`CDC-CALLBACK-01`** *(Sales viết)* | provider test phía Sales | Endpoint thật trả đúng 6 ACK code trong đúng điều kiện |

## 5. Mock fallback

WireMock phát đủ 6 ACK code + 6 lớp lỗi; outbox đã có test bền vững trên Postgres thật. Đường compat đã verify tại commit ghim — **`CURRENT_COMPAT_VERIFIED_AT_PINNED_SHA`**, không phải "đã tích hợp". Gói handoff hiện hành của M8-07 nằm tại
[`m8-07-target-v1-shared-callback-handoff-2026-09-03.md`](../../../plan/ivr-orther/m8-07-target-v1-shared-callback-handoff-2026-09-03.md).

## 6. Closure artifact — owner điền

- [ ] **OpenAPI endpoint generic** từ Sales (path, body, ACK taxonomy), phủ cả hai chương trình.
- [ ] **Bảng compatibility 11 → 4** cho đường compat, có xác nhận Sales chấp nhận phần mất mát và
  ghi rõ current IVR outbound chỉ có 6 final producer path; hai blocked code bị cấm gửi.
- [ ] **Định nghĩa idempotency**: ranh giới `DUPLICATE_ACCEPTED` / `IDEMPOTENCY_CONFLICT`, cửa sổ thời gian giữ key.
- [ ] **Quy tắc revalidation**: sau khi nhận `IVR_CONFIRMED`, Sales kiểm lại blocker nào trước khi chuyển trạng thái; khi nào trả `BLOCKED_BY_CORE`.
- [ ] **Provider test đã merge** phía Sales.
- [ ] **Auth profile + credential custody** đã Security ký; không ghi secret vào evidence.
- [ ] **Reachable sandbox/network/TLS** do Platform cấp và smoke pass.
- [ ] **Shared E2E exact SHA** phủ hai program, replay/conflict, stale/block/review, auth/invalid,
  `429 Retry-After`, outage/circuit/recovery.

## 7. Rủi ro nếu để mở

24/7 không có đường trả kết quả là lỗi **chặn phát hành**, không phải nợ kỹ thuật. Và mục (c) là loại lỗi gây tranh cãi với khách hàng: đơn đã huỷ mà hệ thống ghi nhận xác nhận, không có dấu vết nào giải thích được vì đường compat không mang version.
