# IR-04 — Shared / Foundation Requirements

Trạng thái: `REQUIREMENTS` · Nguồn: DF-01..DF-07; `TECH-01`, `MASTER-03/05`, `phase-8/11`,`/09`.
✅ Owner kiêm Foundation → đa số chốt từ docs; ⏳ còn DF-07 (retention, Legal) + DF-03 sign-off (khi release).

| ID | Yêu cầu | Prio | I/O | idempotency | Ai build | Trạng thái |
| --- | --- | --- | --- | --- | --- | --- |
| IR-FND-01 | **Service identity allowlist + token**: allowlist = Order Core cho `POST /tasks` (`X-Source-System`+token); SIM adapter **không** order-write cred; cấp `SellableCheck`/`RecallHoldView` cho Order Core service-cred | P0 | authz | — | Foundation (owner) | ✅ DF-06 |
| IR-FND-02 | **RBAC `IVR_*`**: `IVR_QUEUE_VIEW/PAUSE/RESUME`, `IVR_SIM_ENABLE/DISABLE`, `IVR_MANUAL_RETRY`, `IVR_RESULT_REVIEW` ở Permission Core; enforce server-side | P0 | authz | — | Foundation (owner) | ✅ DF-01 |
| IR-FND-03 | **OpenAPI 3.1** `openapi/business-platform/ivr-order-confirmation.v1.yaml`; validate CI (parse + contract validator) | P0 | contract | — | IVR/Architect | ✅ DF-02 (file đã sinh; CI validate) |
| IR-FND-04 | **Release gate + Evidence Registry**: evidence packet (task/attempt/result/callback/admin/security/privacy/smoke); `REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi pass; **sign-off = Module 8 Owner + security/privacy** | P0 | evidence/gate | — | Release Owner (bạn) | ✅ DF-03 (model); ⏳ sign-off khi release |
| IR-FND-05 | **Idempotency store + audit sink**: dùng foundation TECH-01 (append-only) | P1 | store | có | Foundation | ✅ DF-04 |
| IR-FND-06 | **Correlation + event/outbox**: `X-Correlation-Id` xuyên suốt; outbox tái dùng pattern ops-core (`HttpWebhookOutboxEventDispatcher`), event không thay callback | P1 | trace/event | dedupe | Foundation | ✅ DF-05 |
| IR-FND-07 | **Retention duration** từng loại (call log/DTMF/recording/audit/raw phone-token) | P1 | policy | — | Owner + Legal | ⏳ DF-07 (PENDING số cụ thể) |

## Ghi chú
- Notification: chỉ **sau Core decision** — IVR không tự gửi (owner Notification/CRM).
- Downstream (AI/Facebook/Live/CRM) chỉ consume trạng thái Core-approved; không trigger IVR.
