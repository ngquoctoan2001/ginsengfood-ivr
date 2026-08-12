# API-02 — IVR-Owned Internal API

Trạng thái: `TARGET_V1_DRAFT` · Base path `/v1/ivr/order-confirmation`.

| Endpoint | Producer → Consumer | Vai trò |
| --- | --- | --- |
| `POST /tasks` | Sales → IVR | receive/validate/idempotently create call job |
| `POST /eligibility-checks` | IVR internal | persist policy decision/evidence |
| `POST /call-jobs` | IVR internal | internal job lifecycle command |
| `GET /call-jobs/{id}` | Admin/internal | masked task/job view |
| `POST /call-attempts` | Scheduler/adapter → IVR | attempt lifecycle |
| `POST /call-results` | Normalizer → IVR | canonical result |
| `POST /result-callbacks` | IVR internal | persist callback delivery lifecycle, not the Sales endpoint |

All POSTs require bearer/service auth, `Idempotency-Key` and `X-Correlation-Id`.

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
