# API-02 — IVR-Owned Internal API

Trạng thái: `TARGET_V1_DRAFT` · Base path `/v1/ivr/order-confirmation`.

| Endpoint | Producer → Consumer | Vai trò |
| --- | --- | --- |
| `POST /tasks` | Sales → IVR | receive/validate/idempotently create call job |
| `POST /eligibility-checks` | IVR internal | persist policy decision/evidence |
| `POST /call-jobs` | IVR internal | internal job lifecycle command |
| `GET /call-jobs/{id}` | IVR internal | masked task/job view; admin browser không gọi trực tiếp |
| `POST /call-attempts` | Scheduler/adapter → IVR | attempt lifecycle |
| `POST /call-results` | Normalizer → IVR | canonical result |
| `POST /result-callbacks` | IVR internal | persist callback delivery lifecycle, not the Sales endpoint |

All POSTs require bearer/service auth, `Idempotency-Key` and `X-Correlation-Id`.

## Internal service boundary (P2-8 / W-0065)

- Chỉ service identity cấu hình bằng secret `IVR_INTERNAL_SERVICE_TOKEN` được gọi 6 endpoint lifecycle; token không được commit vào repository.
- Bắt buộc `X-Source-System: ivr-worker|ivr-adapter` và `X-Service-Scope: ivr.internal.write`. Admin session/header permission không thay thế được service identity; sai identity/scope trả `403 IVR_FORBIDDEN_CALLER`.
- Các POST lifecycle chỉ **reassert/đọc lifecycle đã được owner tương ứng tạo**: eligibility do P2-2 đánh giá, job/attempt do scheduler P2-3/P2-4 tạo, result do P2-5 chuẩn hóa, callback do P2-6 tạo. API không cho caller tự tạo arbitrary result/callback/order transition.
- `POST /result-callbacks` trả trạng thái delivery nội bộ (`result_state`, `delivery_status`, `retry_count`); đây không phải ACK của Sales và không được map thành `CallbackCoreResponseTarget`.
- Response chỉ chứa operational identifiers/state; không trả raw phone, dial token, full address, payment/health/recording data.
- Màn admin cần call-detail phải đi qua admin backend/BFF: BFF xác thực user + `IVR_QUEUE_VIEW`, sau đó gọi internal API bằng service identity riêng. Không đưa `IVR_INTERNAL_SERVICE_TOKEN` hoặc internal headers xuống browser.

## Task validation

- contract/order/version/window present;
- exact program matrix: Golden Hour ONLINE or 24/7 COD; `ivr_confirmation_required=true`;
- approved policy version and offsets; production dispatch forbidden on candidate/unapproved policy;
- dial token valid/not expired; no raw phone;
- speech summary has allowed fields and no full address;
- eligibility/call restriction/evidence pass fail-closed;
- execution mode/gates permit the requested dispatch.

## Outbound callback distinction

The Sales-owned Target endpoint is `POST {sales}/api/v1/internal/orders/{orderId}/ivr-result-callbacks`. It is described in [05-order-core-contracts.md](05-order-core-contracts.md) and separate OpenAPI. Current Golden Hour path is compatibility-only.
