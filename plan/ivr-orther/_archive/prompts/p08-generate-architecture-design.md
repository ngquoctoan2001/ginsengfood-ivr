# P08 — Generate Architecture Design

## Tên nhiệm vụ
Sinh system context, module boundaries, integration/deployment architecture, caching, retry, circuit breaker, failure handling + Mermaid diagrams.

## Bối cảnh
Baseline `IVR-10` (kiến trúc triển khai), `IVR-13` (hàm/dịch vụ), `IVR-16` (NFR), `IVR-17` (thiết kế tích hợp), `IVR-18` (quan sát/vận hành) là nguồn. Mô hình triển khai chính thức: `INTERNAL_SIM_GATEWAY_SERVER`, `ONE_SIM_ONE_ACTIVE_CALL`.

## Input cần đọc
- `specs/srs/api/*`, `specs/srs/data/*`, `specs/srs/workflows/*`
- `docs/documents/4. phase/phase-8/10, 13, 16, 17, 18`
- `docs/documents/1. master/05-MASTER-04-RUNTIME-RESOLUTION-GUARD.md`
- `docs/documents/3. tech/01-TECH-00-...MASTER-PLAN.md`

## Output cần tạo
- `specs/srs/architecture/`:
  - `00-index.md`
  - `01-system-context.md` (C4 context: IVR ↔ Order Core ↔ Ops Core ↔ SIM Gateway ↔ Evidence/Audit ↔ Admin)
  - `02-module-boundaries.md` (service blocks: intake, scheduler, adapter, normalizer, callback, admin)
  - `03-integration-architecture.md` (sync command/callback, async event, allowlist service identity)
  - `04-deployment-architecture.md` (internal SIM gateway server, SIM pool 12/24/32, capacity baseline)
  - `05-resilience.md` (fail-safe khi Order Core/Ops/Trust/Evidence/SIM unavailable, bounded retry, circuit breaker, không dispatch khi blocker check down)
  - `06-observability.md` (metrics, correlation trace, alert, capacity incident)
  - `07-diagrams.md` (Mermaid tổng hợp)

## Quy tắc
- Bám failure contracts `IVR-02` §10 (before/during attempt/during callback).
- Fail-closed: source-of-truth/policy resolver down → không gọi khách thật.
- SIM adapter tách biệt, không quyền order.
- Nêu rõ điểm cache (nếu có) và rủi ro cache sale-lock (không cache blocker critical realtime).

## Checklist hoàn thành
- [ ] Context + boundary + integration + deployment + resilience + observability đủ.
- [ ] Diagram render được.
- [ ] Failure matrix khớp `IVR-02`.
- [ ] Capacity baseline có số.

## Điều cấm
- KHÔNG chọn cloud IVR/SIP/brandname làm mặc định (future owner decision → `NEED_CONFIRMATION`).
- KHÔNG thiết kế IVR có đường ghi order state.

## Báo cáo cuối
1. Các service block chính.
2. Failure/resilience đã phủ.
3. Điểm hạ tầng cần owner quyết (provider, SIM pool size).
