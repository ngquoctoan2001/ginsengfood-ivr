# TECH-STACK — IVR Order Confirmation Service

Trạng thái: `LOCKED (DTS-01..05, 2026-07-03)` · Nguồn: `plan/ivr-orther/decisions-log.md` §Tech Stack (DTS).
Phạm vi: nền tảng công nghệ cho **service IVR độc lập**. Đây là spec chi phối `prompt/` (bộ triển khai A–Z).

## 1. Tổng quan kiến trúc triển khai
```
                         (Java/Spring — ginsengfood-business-platform)
   ┌─────────────┐  OpenAPI  ┌───────────────┐  webhook  ┌──────────────┐
   │ Order Core  │◀────────▶ │   IVR (.NET)  │◀────────  │  Ops-Core    │
   │ CRM / Ident │  callback │  api + worker │  sellable │  sellable gate│
   └─────────────┘           └──────┬────────┘           └──────────────┘
          ▲                         │ ISimGateway (MOCK→REAL)
          │ Next.js Admin UI        ▼
          └───────────────   Internal SIM Gateway (nội bộ, mua sau — DT-01)
```
IVR **KHÔNG** share DB/entity/codebase với platform Java. Mọi dữ liệu order/consent/sellable đọc **qua API**; kết quả trả **qua callback**. IVR chỉ sở hữu DB `ivr_*` (Postgres) của chính nó.

## 2. Thành phần & công nghệ
| Thành phần | Công nghệ | Vai trò |
| --- | --- | --- |
| `ivr-api` | **.NET 10 / ASP.NET Core** (Minimal API hoặc Controllers) | Task intake (`POST /v1/ivr/order-confirmation/tasks`), callback client, admin API. |
| `ivr-worker` | **.NET 10 Worker Service** (`IHostedService`/`BackgroundService`) | Scheduler (attempt policy D-10), dispatch, `ISimGateway` driver, DTMF normalize, callback dispatch. |
| `ivr-admin-ui` | **Next.js** (React + TypeScript) | Dashboard, call-log, call-detail, config, integration-status, RBAC. i18n `vi`. |
| DB | **PostgreSQL** | `ivr_*` tables; migration EF Core; retention CronJob. |
| Contract | **OpenAPI 3.1** + webhook | `specs/api/openapi/ivr-order-confirmation.v1.yaml`. |
| Observability | **OpenTelemetry** (OTLP) | log/metric/trace + correlation `X-Correlation-Id`. |
| Deploy | **Docker + Kubernetes (Helm)** | HPA theo SIM concurrency; secrets; NetworkPolicy; CronJob retention. |

## 3. Ràng buộc bắt buộc (giữ nguyên từ decisions cũ)
- **D-02:** IVR không transition order — chỉ gửi signal; UI/worker không ghi order state.
- **DS-01:** intake chỉ `order_status=CONFIRMING` + `payment_method_snapshot=COD`.
- **D-10:** `max_attempts=2` cả hai program; window/spacing GH 300/150, 24-7 900/450.
- **D-05:** raw phone/dial_token không lưu ở IVR (token vault ở SIM adapter boundary).
- **DO-06:** fail-closed mọi blocker check; `/health/ready=503` = không dispatch.
- **DT-01:** `ISimGateway` port; `IVR_ADAPTER_MODE=MOCK` mặc định; `REAL` chỉ khi mua SIM + release gate.
- **DF-03:** `REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi release gate pass.

## 4. Điểm tham số hóa (NEED_CONFIRMATION — chốt khi vào phase tương ứng)
| Điểm | Default đề xuất | Chốt ở |
| --- | --- | --- |
| ORM | EF Core (migrations first-class) | Phase 1 |
| Outbox/dispatch | .NET BackgroundService + Postgres outbox table | Phase 1/4 |
| CI provider | GitHub Actions | Phase 0 |
| Secret store | K8s Secret (dev) → Vault/KMS (prod) | Phase 7 |
| Container registry | (theo hạ tầng platform) | Phase 7 |
| Message bus (nếu cần) | HTTP webhook outbox (khớp DF-05, không thêm broker) | Phase 4 |

## 5. Mapping sang bộ prompt
Xem `prompt/00-index.md`. Mọi prompt Phase 0–9 giả định stack này; code/test/CI/deploy sinh ra phải là **.NET 10 / Postgres / Next.js / K8s** cụ thể, không generic.
