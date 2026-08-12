# P05 — Generate API Specs

## Tên nhiệm vụ
Sinh API specs cho IVR: internal/admin API, telephony/SIM adapter, sales-required, ops-required, error code, auth, idempotency.

## Bối cảnh
Baseline `IVR-11` đã khóa nhóm endpoint `/v1/ivr/order-confirmation/*` và tham chiếu contract `openapi/business-platform/ivr-order-confirmation.v1.yaml`. Prompt này tạo API spec chi tiết (OpenAPI 3.1 nếu được duyệt) + phần "API cần từ team khác" (draft, không phải final của họ).

## Input cần đọc
- `specs/srs/functional/*`, `specs/srs/workflows/*`
- `docs/documents/4. phase/phase-8/11-THIẾT KẾ API.md`
- `docs/documents/4. phase/phase-8/04-...TÁC VỤ IVR.md` và `07-...CALLBACK...md` (contract)
- `docs/documents/4. phase/phase-8/06-BỘ CHUYỂN ĐỔI CỔNG SIM NỘI BỘ.md`
- `docs/documents/3. tech/02-TECH-01-FOUNDATION-RBAC-AUDIT-IDEMPOTENCY-EVIDENCE-REGISTRY.md` (auth/idempotency convention)
- `plan/ivr-orther/11-sales-platform-api-needs-draft.md`, `12-ops-core-api-needs-draft.md`

## Output cần tạo
- `specs/srs/api/`:
  - `00-index.md`
  - `01-conventions.md` (version, envelope, headers `Authorization`/`X-Correlation-Id`/`Idempotency-Key`/`X-Actor-Id`/`X-Source-System`)
  - `02-internal-api.md` (tasks, eligibility-checks, call-jobs, call-attempts, call-results, result-callbacks)
  - `03-admin-api.md` (queue pause/resume, sim enable/disable, technical-retries, admin-reviews + permission)
  - `04-sim-adapter-contract.md` (input/output adapter, DTMF, call disposition — internal, không public)
  - `05-order-core-contracts.md` (`IvrConfirmationTaskV1`, `IvrConfirmationResultCallbackV1`)
  - `06-error-codes.md` (400/401/403/404/409/422/429/500 mapping)
  - `07-idempotency-and-correlation.md`
  - `08-external-api-needs.md` (con trỏ sang integration-requirements: sales/ops APIs IVR cần)
- (Tùy owner duyệt) `specs/srs/api/openapi/ivr-order-confirmation.v1.yaml`

## Quy tắc
- KHÔNG có endpoint nào cho phép IVR update order state trực tiếp.
- Mọi POST rủi ro có `Idempotency-Key`; mô tả hành vi duplicate key.
- SIM adapter KHÔNG có credential ghi order.
- Phần "external API needs" chỉ là **draft yêu cầu**, ghi rõ "cần team sales/ops xác nhận", không coi là đã tồn tại.
- Telephony webhook: chỉ mô tả nếu chuyển sang mô hình có provider webhook; mặc định là INTERNAL_SIM_GATEWAY (đánh `NEED_CONFIRMATION`).

## Checklist hoàn thành
- [ ] Đủ internal + admin endpoints từ `IVR-11`.
- [ ] Error code mapping đầy đủ.
- [ ] Idempotency/correlation rõ.
- [ ] External API needs trỏ đúng sang integration-requirements.
- [ ] OpenAPI (nếu sinh) parse pass 3.1.

## Điều cấm
- KHÔNG bịa endpoint sales/ops đã tồn tại — mọi API bên ngoài là draft-needs.
- KHÔNG public consumer API.

## Báo cáo cuối
1. Số endpoint internal/admin.
2. Số external API needs (sales/ops).
3. Điểm `NEED_CONFIRMATION` về telephony provider.
