# API-01 — Conventions

Trạng thái: `TARGET_V1_DRAFT`.

## Paths/version

IVR-owned base `/v1/ivr/order-confirmation`; Sales-owned target callback `/api/v1/internal/orders/{orderId}/ivr-result-callbacks`. Contract version is explicit in DTO. Current Golden Hour path/auth is isolated compatibility.

## Headers/auth

| Header | Rule |
| --- | --- |
| `Authorization` | required; dev mock JWT, target short-lived service JWT |
| `Idempotency-Key` | required on command/outbound callback; scoped and payload-bound |
| `X-Correlation-Id` | required/propagated across task/job/attempt/result/callback |
| `X-Actor-Id` | admin metadata after authenticated RBAC; never trusted alone |
| `X-Source-System` | optional/required metadata by profile; never sole auth |

mTLS is pending owner decision. `X-Internal-Token` only current compatibility. Errors use stable redacted envelope with correlation ID.

## Compatibility/change

OpenAPI drift is reviewed, not auto-accepted. Required-field/enum/path/semantic changes are breaking unless a compatible version/provider is introduced. Target/current DTOs are distinct.

## Fail-safe/modes

- missing auth/policy/evidence/provider truth blocks the affected path;
- MOCK has no real egress; LAB requires destination allowlist and kill switch; PROD requires release gates;
- IVR never transitions orders or sends notification;
- logs/errors never contain raw phone, full address, token or secret.
