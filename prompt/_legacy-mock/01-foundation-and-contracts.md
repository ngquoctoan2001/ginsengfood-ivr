# DEV PROMPT 01 — Foundation & Contracts

## Slice
Nền tảng trước các slice M8.2A–H.

## Mục tiêu
Dựng khung: OpenAPI, DB migration, RBAC, idempotency/audit/correlation, SIM adapter port (MOCK), service allowlist.

## Requirement / Decision
`DF-01..DF-06`, `D-10`, `D-02`, `D-05`, `DO-02`, `DT-01`, `OD-DR-03`.

## Source spec (đọc trước)
- `specs/srs/api/01-conventions.md`, `openapi/ivr-order-confirmation.v1.yaml`
- `specs/srs/database/02-tables.md`, `03-enums-and-status.md`, `04-indexes.md`, `06-migration-plan.md`
- `specs/srs/architecture/02-module-boundaries.md`, `03-integration-architecture.md`
- `specs/srs/api/07-idempotency-and-correlation.md`

## Build scope
1. Sinh server stub từ OpenAPI 3.1 (validate CI — DF-02).
2. DB migration 11 bảng (gồm `ivr_raw_call_event`); **CHECK**: `max_attempts=2` cả hai program; GH `window=300/spacing=150`, 24-7 `window=900/spacing=450` (D-10); `attempt_number≤2`; `is_counted=false` khi technical. Unique idempotency/task/callback; index scheduler-deadline.
3. RBAC `IVR_*` ở Permission Core; enforce server-side (DF-01).
4. Idempotency store + audit append-only (foundation TECH-01, DF-04); `X-Correlation-Id` middleware (DF-05).
5. **Service allowlist**: chỉ `X-Source-System=order-core` + token gọi `POST /tasks` (DF-06).
6. **SIM adapter port** interface (`dial/play/capture/disposition/health`) + impl `MOCK` đọc `seed/call-scenarios` (DT-01); `adapter_mode` config.

## Done gate
- [ ] OpenAPI validate pass (CI).
- [ ] Migration chạy + rollback; constraint D-10 verify.
- [ ] RBAC + idempotency + audit + correlation hoạt động.
- [ ] Allowlist chặn caller lạ (`403`).
- [ ] Adapter MOCK chạy được scenario.

## Evidence expected
Migration log + constraint proof (D-10), OpenAPI validation report, RBAC/idempotency unit test, allowlist reject `403`, adapter MOCK run.

## Forbidden
KHÔNG raw phone/recording bắt buộc; KHÔNG `REAL` adapter; KHÔNG production code triển khai gọi thật.

## Test
`testing/04-contract-test-plan` (CT-OAS/CT-TASK), `testing/07` (SEC-01..03), `testing/02` (idempotency).
