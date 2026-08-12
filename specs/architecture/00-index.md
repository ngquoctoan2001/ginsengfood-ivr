# Architecture SRS — Index

Trạng thái: `SRS_DRAFT` · Sinh bởi: `plan/ivr-orther/prompts/p08-generate-architecture-design.md`
Nguồn: `phase-8/10` (deployment), `/13` (services), `/16` (NFR), `/17` (integration), `/18` (observability), `/02 §10` (failure contracts); `MASTER-04` (resolver/guard); `TECH-00`; `specs/srs/api/*`,`data/*`,`database/*`; decisions D-*/DO-*/DF-*/DT-*.

## 1. Cấu trúc
| File | Nội dung |
| --- | --- |
| [01-system-context.md](01-system-context.md) | C4 context: IVR ↔ Order Core ↔ Ops ↔ SIM ↔ Evidence ↔ Admin ↔ CRM |
| [02-module-boundaries.md](02-module-boundaries.md) | Service blocks nội bộ IVR |
| [03-integration-architecture.md](03-integration-architecture.md) | sync command/callback, async event, allowlist |
| [04-deployment-architecture.md](04-deployment-architecture.md) | Internal SIM Gateway, SIM pool, capacity |
| [05-resilience.md](05-resilience.md) | Failure matrix, fail-closed, retry, circuit breaker, caching |
| [06-observability.md](06-observability.md) | Metrics, trace, alert, incident |
| [07-diagrams.md](07-diagrams.md) | Component + deployment diagram (Mermaid) |

## 2. Ràng buộc kiến trúc (P0)
- Mô hình chốt: **INTERNAL_SIM_GATEWAY_SERVER**, `ONE_SIM_ONE_ACTIVE_CALL`. Cloud IVR/SIP/brandname = future (`NEED_CONFIRMATION`, không mặc định).
- IVR **không** có đường ghi order state (D-02); SIM adapter **không** quyền order (DT-01).
- **Fail-closed**: source-of-truth/policy/ops down → không gọi khách thật (D-06/DO-06; phase-8/02 §10).
- Blocker realtime do **Order Core** gọi ops (DO-03) — IVR không gọi ops trực tiếp.
- SIM chưa mua → adapter port + `adapter_mode=MOCK` (DT-01); `REAL_CUSTOMER_CALL_ALLOWED=NO` tới release gate (DF-03).

## 3. Điểm hạ tầng còn cần owner/mua sắm (tổng hợp)
- Provider/protocol SIM (DT-01), số SIM pool thật (DT-04), caller-ID (DT-06) — mua SIM.
- Retention/recording (DF-07/DT-05) — Legal.
- CRM do-not-call (DC-01/Q-C1 resolved; IR-CRM-01 P1) — không còn là P0, vẫn fail-closed nếu không xác định opt-out.
