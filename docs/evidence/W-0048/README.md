# W-0048 — One-SIM lab + Sales integration readiness

Ngày audit: `2026-08-20`

Trạng thái: **`BLOCKED_EXTERNAL / OWNER_DATA_REQUIRED`**

Cho phép gọi khách thật: **`REAL_CUSTOMER_CALL_ALLOWED=NO`**

## 1. Baseline đã đọc trực tiếp

| Hệ thống | Baseline | Phạm vi đã kiểm tra |
| --- | --- | --- |
| IVR | `main@7195ba8c2f8b7283eb2349550dd47a8ba2bc0f7b` | scheduling/dispatch DI, `DispatchGate`, `ISimGateway`, dial-token, speech provider, callback transports/options/validators, OpenAPI Target V1 và prompt P8-1 |
| Sales (`C:\Projects\ginsengfood-business-platform`) | `PhucApu@a3aad246d986fbc273cf41aaa93eec6659669656` | runtime OpenAPI, Golden Hour enqueue/callback controller, DTO/result enum, token validation, callback service và tìm kiếm outbound IVR client |

Baseline Sales chỉ được đọc; W-0048 không sửa repository Sales.

## 2. Kết luận hiện trạng Sales

### 2.1 Endpoint thật đang có

Sales hiện có đúng endpoint callback tương thích Golden Hour:

`POST /api/v1/internal/ivr/golden-hour/callbacks`

- Auth hiện tại: header `X-Internal-Token` so khớp constant-time với secret cấu hình bởi `GOLDEN_HOUR_IVR_CALLBACK_TOKEN`.
- Không có issuer/audience/JWKS/scope OAuth/JWT cho endpoint này.
- Request thật:

```json
{
  "callId": 0,
  "reservationId": 0,
  "orderId": 0,
  "customerId": 0,
  "result": "CONFIRMED | REJECTED | NO_ANSWER | FAILED",
  "occurredAt": "2026-08-20T00:00:00Z",
  "idempotencyKey": "non-empty"
}
```

- ACK thật hiện là HTTP `200` với envelope `success/message/data`; `data` chứa `callId`, `reservationId`, `orderId`, `beforeStatus`, `afterStatus`, `occurredAt`, `idempotencyKey`.
- Đây chưa phải semantic ACK Target V1 (`ACCEPTED`, `DUPLICATE_ACCEPTED`, `BLOCKED_BY_CORE`, ...).

Nguồn code Sales:

- `back-end/src/main/java/com/ginsengfood/ginsengfood_backend/ivr/api/InternalGoldenHourIvrCallbackController.java`
- `back-end/src/main/java/com/ginsengfood/ginsengfood_backend/ivr/dto/request/GoldenHourIvrCallbackRequest.java`
- `back-end/src/main/java/com/ginsengfood/ginsengfood_backend/ivr/dto/request/GoldenHourIvrCallbackResult.java`
- `back-end/src/main/java/com/ginsengfood/ginsengfood_backend/ivr/dto/response/GoldenHourIvrCallbackResponse.java`
- `back-end/src/main/java/com/ginsengfood/ginsengfood_backend/goldenhour/service/impl/GoldenHourPaymentGateServiceImpl.java`

### 2.2 Những gì Sales chưa có

Runtime OpenAPI Sales không có:

- `POST /v1/ivr/order-confirmation/tasks`;
- `POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks`;
- Target V1 task payload/ACK;
- outbound HTTP producer đẩy task từ Sales sang IVR;
- luồng 24/7 COD producer.

Golden Hour hiện chỉ ghi một row vào `ivr_call_queue` nội bộ của Sales, chứa cả số nhận hàng thô. Nó chưa gọi IVR Target API. Vì vậy không được mô tả hệ thống hiện tại là “Sales đã tích hợp IVR”.

## 3. Kết luận hiện trạng IVR

### 3.1 Callback

- `CurrentGoldenHourCallbackTransport` đã ánh xạ callback IVR sang payload hiện tại của Sales và dùng `X-Internal-Token`.
- Transport này cần bảng identity `taskId -> callId/reservationId/orderId/customerId` do upstream cấp.
- Validator vẫn cố ý fail-closed: chưa có runtime mode được phê duyệt cho `CURRENT_GOLDEN_HOUR_COMPAT`.
- `TargetV1CallbackTransport` đã có contract Bearer audience mặc định `sales-order-core`, idempotency/correlation và semantic ACK, nhưng Sales chưa cung cấp endpoint/auth/sandbox thật; validator chặn cấu hình giả.

### 3.2 Telephony

- `ISimGateway`, scheduler lease/fencing và `DispatchGate` đã tồn tại.
- `DispatchGate.EvaluateAsync` chưa được nối vào luồng dial production.
- Khi không chạy MOCK, DI hiện đăng ký `UnavailableSchedulerDispatchGateway`; chưa có gateway thật cho `LAB_REAL_SIM`.
- Chưa có `IDialTokenResolver` thật, `ISimGateway` thật hay adapter vendor/Asterisk.
- Chưa có file-playback speech provider; external TTS provider chỉ là skeleton fail-closed.
- Cấu hình hiện không cho ghép `CURRENT_GOLDEN_HOUR_COMPAT` với `LAB_REAL_SIM` thành một runtime được phê duyệt.

Do đó chưa có cơ sở chạy cuộc gọi thật ở baseline này.

## 4. Trình tự tích hợp đã chốt cho readiness

| Lane | Mục đích | Sales | Telephony | Trạng thái |
| --- | --- | --- | --- | --- |
| A — current compatibility CDC | Chứng minh callback Golden Hour hiện hữu với payload/ACK thật | Sandbox Sales + test identity + secret reference | MOCK | `OWNER_DATA_REQUIRED` |
| B — one-SIM lab | Chứng minh dial/audio/DTMF/disposition/kill switch trên một kênh | fake Sales | Asterisk/vendor adapter + SIM thật + allowlist alias | `OWNER_DATA_REQUIRED` |
| C — Target V1 | Tích hợp vận hành mục tiêu | Sales xây task producer, generic callback, auth/audience và semantic ACK | adapter đã qua lane B | `BLOCKED_EXTERNAL` |

Lane A và B độc lập, có thể chuẩn bị song song nhưng phải báo cáo evidence riêng. Lane C chỉ bắt đầu sau khi contract thật của Sales được cung cấp và duyệt.

## 5. Evidence hiện có và chưa có

| Evidence | Trạng thái |
| --- | --- |
| Code audit IVR + Sales theo hai baseline ở §1 | `PASS` |
| Danh sách đầu vào có trường điền và quy tắc không lộ PII/secret | `PASS` — xem `external-input-request.md` |
| Sales sandbox credential test | `NOT_RUN` |
| Current Golden Hour callback CDC | `NOT_RUN` |
| Thiết bị/modem/SIM protocol handshake | `NOT_RUN` |
| Một cuộc gọi allowlist thật | `NOT_RUN` |
| Audio/DTMF/disposition/kill-switch lab | `NOT_RUN` |
| Target V1 task/callback E2E với Sales thật | `NOT_RUN` |

## 6. Điều kiện để chuyển khỏi `BLOCKED_EXTERNAL`

1. Owner điền và trả lại các mục bắt buộc trong `external-input-request.md` mà không đưa raw secret/raw phone vào git hoặc chat.
2. Dev Sales xác nhận lane A hay xây lane C trước; nếu chọn lane C phải cung cấp OpenAPI và auth profile thật.
3. Có model/firmware/protocol tài liệu của gateway, một SIM test gọi ra được và allowlist alias được ánh xạ ngoài IVR.
4. Trước mọi sửa code, chạy GitNexus impact cho các symbol bị chạm và cảnh báo nếu rủi ro HIGH/CRITICAL.
5. Lab chỉ gọi số test do owner kiểm soát; `REAL_CUSTOMER_CALL_ALLOWED` giữ `NO`.

Không có dòng nào ở trên tự đóng `W-0048`; physical lab và Sales CDC phải tạo evidence chạy thật.
