# P11 — Generate Testing Specs

## Tên nhiệm vụ
Sinh testing strategy + unit/integration/contract/e2e/performance/security test plan + acceptance criteria.

## Bối cảnh
Baseline `IVR-09` (ma trận kiểm thử khói & cổng phát hành, IVR-SMK-001..030) và `IVR-19` (kế hoạch smoke & phát hành) là nguồn. Hệ thống tuân mô hình evidence/smoke/completion-gate (`MASTER-05`, `PACK-10`, `TECH-10`).

## Input cần đọc
- Toàn bộ `specs/srs/*` đã sinh (p03..p09)
- `docs/documents/4. phase/phase-8/09-...KHÓI VÀ CỔNG PHÁT HÀNH.md`, `19-...SMOKE VÀ PHÁT HÀNH.md`
- `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md`
- `docs/documents/3. tech/11-TECH-10-...RELEASE-GATEWAY-PRODUCTION-READINESS...md`

## Output cần tạo
- `specs/srs/testing/`:
  - `00-index.md`, `01-strategy.md`
  - `02-unit-test-plan.md`, `03-integration-test-plan.md`
  - `04-contract-test-plan.md` (task/callback contract, error mapping)
  - `05-e2e-test-plan.md` (8 workflow, dry-run mode)
  - `06-performance-test-plan.md` (capacity SIM 12/24/32)
  - `07-security-privacy-test-plan.md` (RBAC, PII, no raw phone in log, SIM no order write)
  - `08-acceptance-criteria.md` + `09-smoke-matrix.md` (map IVR-SMK-*)

## Quy tắc
- Mọi smoke phải có cả PASS path và BLOCK/negative path.
- P0 test bắt buộc: no self order-update, no Quote/Cart/Draft, Golden Hour không attempt 3, 24/7 không attempt 4, technical≠no-answer, invalid≠no-answer, stale callback no transition, evidence-missing block, race Sale Lock block.
- Acceptance gắn evidence packet + owner sign-off (không hardcode PASS).

## Checklist hoàn thành
- [ ] Đủ 7 loại test plan + acceptance + smoke matrix.
- [ ] Map IVR-SMK-001..030.
- [ ] P0 negative cases đủ.
- [ ] Release gate điều kiện rõ (REAL_CUSTOMER_CALL_ALLOWED=NO tới khi pass).

## Điều cấm
- KHÔNG viết test gọi khách thật.
- KHÔNG tuyên bố production-ready.

## Báo cáo cuối
1. Số test case theo loại.
2. Coverage smoke matrix.
3. Điều kiện còn thiếu để mở release gate.
