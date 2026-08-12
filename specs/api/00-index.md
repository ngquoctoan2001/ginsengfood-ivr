# API SRS — Index

Trạng thái: `SRS_DRAFT` · Sinh bởi: `plan/ivr-orther/prompts/p05-generate-api-specs.md`
Module: IVR Order Confirmation (`ivr-order-confirmation`). API **nội bộ/admin**, KHÔNG public consumer API.
Nguồn: `phase-8/11` (API design), `/04` (task contract), `/07` (callback), `/06` (SIM adapter); `TECH-01` (auth/idempotency/audit); và các quyết định đã khóa `plan/ivr-orther/decisions-log.md` (D-*, DO-*, DF-*, DT-*).

## 1. Cấu trúc
| File | Nội dung |
| --- | --- |
| [01-conventions.md](01-conventions.md) | Version, base path, headers, envelope, auth, fail-safe |
| [02-internal-api.md](02-internal-api.md) | tasks · eligibility-checks · call-jobs · call-attempts · call-results · result-callbacks |
| [03-admin-api.md](03-admin-api.md) | queue pause/resume · sim enable/disable · technical-retries · admin-reviews (+ RBAC) |
| [04-sim-adapter-contract.md](04-sim-adapter-contract.md) | Adapter port (dial/play/capture/disposition/health) — internal; SIM chưa mua → mock |
| [05-order-core-contracts.md](05-order-core-contracts.md) | `IvrConfirmationTaskV1` · callback `Current/Target` |
| [06-error-codes.md](06-error-codes.md) | HTTP mapping + business reject + consume ops-core error codes |
| [07-idempotency-and-correlation.md](07-idempotency-and-correlation.md) | Idempotency scope, duplicate behavior, correlation chain |
| [08-external-api-needs.md](08-external-api-needs.md) | Contract IVR cần từ Order Core / ops-core / CRM (đã khóa + build/open items) |
| `openapi/ivr-order-confirmation.v1.yaml` | OpenAPI 3.1 (DF-02) — validate ở CI |

## 2. Nguyên tắc bao trùm (P0)
- ✅ KHÔNG endpoint nào cho IVR **update order state** trực tiếp (D-02; P0-IVR-002).
- ✅ Mọi POST rủi ro (task/callback/admin/retry) bắt buộc `Idempotency-Key` (DF-04).
- ✅ SIM adapter **không** có credential ghi order, không gửi SMS (DT-01; P0).
- ✅ Chỉ **Order Core** (allowlist) được gọi task-intake; auth service-token/admin RBAC, không anonymous (DF-06/DF-01).
- ✅ Dependency owner down → **fail-closed**, không dispatch cuộc gọi thật (D-06/DO-06).
- ✅ `REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi release gate pass (DF-03). SIM chưa mua → dry-run/mock (DT-01).

## 3. Trạng thái quyết định liên quan API
- Order Core contract & transport: ✅ D-01..D-06.
- Ops-core sellable gate (Core gọi, fan-out per-line): ✅ DO-01..DO-07.
- Foundation auth/RBAC/OpenAPI/idempotency/audit: ✅ DF-01..DF-06.
- SIM adapter port + disposition: ✅ DT-01 (port)/DT-02 (mapping); protocol PENDING mua SIM.
- **CRM do-not-call/opt-out (blocker): ✅ DC-01/Q-C1 resolved** — còn IR-CRM-01 P1 trong `08-external-api-needs.md`.
- Task OpenAPI: ✅ current yêu cầu `order_state=CONFIRMING` + `payment_method_snapshot=COD`; `order_version` và `is_ivr_callable` không còn là required source fields (OC1/derived).
- Callback OpenAPI: ✅ tách **current** (`IvrConfirmationResultCallbackCurrentV1`, `200/422`, chưa version race-guard) và **target** (`IvrConfirmationResultCallbackTargetV1`, `order_version_seen_by_ivr`, `CALLBACK_*`) theo DS-03/DS-04 + IR-SALES-OC1/OC2.

## 4. Báo cáo cuối p05
Xem cuối `02` (internal), `03` (admin), `08` (external), `06` (error), và mục "Báo cáo" ở `openapi`.
