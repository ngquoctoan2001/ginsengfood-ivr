# W-0178 — Offline M3 D-06 revalidation evidence validator

## Kết luận

`TESTS_PASS / M3_D06_EVIDENCE_NOT_RECEIVED / STRATEGY_UNSIGNED / CODE_NOT_AUTHORIZED`.

W-0178 đóng khoảng trống kiểm tra offline đã xác định trong W-0149/W-0161: W-0174 kiểm report shared-E2E ở mức callback tổng quát, nhưng chưa bắt buộc bằng chứng riêng cho từng business blocker của D-06. Validator mới buộc đủ 12 case, exact M8/M3 SHA, provenance độc lập, signer/verifier separation và safety flags trước khi bundle được đưa vào shared-E2E review.

PASS của validator chỉ có nghĩa `D06_EVIDENCE_VALID_FOR_SHARED_E2E_REVIEW_ONLY`. Nó không chọn Option A/B/hybrid, không chứng minh authority ngoài đời, không gỡ delivery guard, không authorize production và không cho phép cuộc gọi khách hàng thật.

## Ranh giới đã giữ

- Không sửa scheduler, intake, claim, dial, callback runtime, DB hay OpenAPI.
- Không thêm revoke endpoint/state/generation/fence.
- Không gọi M3, Ops Core, Platform hoặc Telephony; không đọc credential hay raw callback row.
- `observed_decision` và `observed_blocker` trong report là vocabulary của evidence, không phải wire enum mới.
- Module 3 vẫn sở hữu D-06 revalidation tại thời điểm callback; IVR không tự truy vấn Ops source để thay M3.

## Matrix bắt buộc

| Case | Điều kiện phải chứng minh | Kỳ vọng fail-closed |
| --- | --- | --- |
| D06-01 | Version/state hiện hành, program/payment hợp lệ, không blocker | Accepted sau revalidation; đúng một transition |
| D06-02 | Order version đổi sau intake | Stale; không transition |
| D06-03 | Order state không còn callable | Core-blocked; không transition |
| D06-04 | Recall bật sau intake | Core-blocked; không transition |
| D06-05 | Sale-lock bật sau intake | Core-blocked; không transition |
| D06-06 | Quality-hold bật sau intake | Core-blocked; không transition |
| D06-07 | Program/payment không còn hợp lệ | Core-blocked; không transition |
| D06-08 | Business evidence hết hạn | Core-blocked; không transition |
| D06-09 | Một source bắt buộc không truy cập được | Không accepted ACK; retry/review disposition |
| D06-10 | Replay cùng key và immutable body | Trả prior decision; không transition trùng |
| D06-11 | Replay cùng key nhưng đổi immutable body | Idempotency conflict; không transition |
| D06-12 | Source phục hồi sau retry có giới hạn | Revalidate lại toàn bộ; đúng một transition |

Validator không cho chọn chỉ case xanh: `selected_green_cases_only` phải là `false`, đủ 12/12 case trên cùng candidate/environment/config mới hợp lệ.

## Contract và provenance được ghim

Validator kiểm byte SHA-256 hiện tại của:

- `docs/evidence/W-0149/README.md`
- `integration-requirements/01-sales-platform-requirements.md`
- `integration-requirements/06-module-3-api-handover.md`
- `specs/api/openapi/order-core-ivr-callback.target-v1.yaml`
- `deploy/ci/scripts/target-v1-shared-e2e-report-validator.mjs`

Bundle hoàn tất còn phải được đối chiếu với tám pin độc lập truyền qua CLI: exact M8 SHA, exact M3 SHA, M3 authoritative OAS, M3 implementation, M3 consumer CDC, Ops truth contract, Security auth/custody và Platform sandbox/network/TLS evidence.

Các artifact bên ngoài phải được tạo trước lúc run bắt đầu. Bảy vai trò ký duyệt là Project Owner, M3 Owner, Product Owner, Ops Source Owner, Security, Platform và Release Owner; signer phải duy nhất và mọi verifier phải nằm ngoài toàn bộ tập signer.

## Cách dùng

Kiểm template pending:

```powershell
node deploy/ci/scripts/d06-revalidation-evidence-validator.mjs --check-template docs/evidence/W-0178/d06-revalidation-evidence.template.json
```

Sau khi các owner điền report thật, lưu thành file mới, không ghi đè template, rồi chạy:

```powershell
node deploy/ci/scripts/d06-revalidation-evidence-validator.mjs `
  --input <completed-report.json> `
  --m8-commit-sha <40hex> --m3-commit-sha <40hex> `
  --m3-oas-sha256 <64hex> --m3-implementation-sha256 <64hex> `
  --m3-cdc-sha256 <64hex> --ops-truth-sha256 <64hex> `
  --security-auth-sha256 <64hex> --platform-evidence-sha256 <64hex>
```

Các SHA truyền qua CLI phải đến từ reviewer/custodian độc lập, không sao chép từ chính report cần kiểm.

## Evidence local 2026-09-04

- `node --check .../d06-revalidation-evidence-validator.mjs`: PASS.
- `node .../d06-revalidation-evidence-validator.mjs --self-test`: `W0178_SELFTEST_PASS template=1 valid=1 refusals=31`.
- `--check-template`: `D06_TEMPLATE_VALID_NOT_READY cases=12 production_authorized=false`.
- Chạy template pending qua `--input`: REFUSED, exit 1, do status chưa phải `M3_D06_EVIDENCE_COMPLETE`.
- Hash manifest: `artifact-sha256.txt`.
- PII scanner self-test PASS; scoped scan PASS `4 files / 0 binary` bằng Git Bash (WSL bash không có trên host).
- API docs self-test PASS `14` artifact; CI config PASS; test traceability current `485`.
- W-0174 regression PASS `1 valid / 46 refusal`; readiness mirror PASS `11 gates / 179 work items / 23 open decisions`, production=false.
- Markdown map ghi W-0178 `0 unresolved`; aggregate `145 unresolved` phản ánh các plan document đang bị xóa trong external WIP, không được W-0178 restore hoặc nhận làm evidence.

Self-test phủ positive fixture, pending template, exact schema/order, source drift, independent pin drift, late artifact, missing/reordered case, candidate/run binding, từng decision/blocker/assertion/revision/state, replay body/key, green-only selection, signer/verifier separation, review time, PII/credential marker, unknown key, duplicate JSON key, oversized input và path escape.

## Việc bên ngoài còn thiếu

1. Project Owner và M3 chọn, ký Option A, B hoặc hybrid. D-06 vẫn bắt buộc với cả ba.
2. M3/Platform/Security/Ops cung cấp sáu artifact thật và tám pin độc lập nêu trên.
3. Chạy đủ 12 case trên cùng exact M8/M3 candidate trong shared environment, rồi lấy đủ bảy sign-off.
4. W-0178 PASS mới cho phép đưa bundle sang W-0174/shared-E2E review; chỉ authority/release process bên ngoài mới có thể xem xét gỡ delivery guard.

`REAL_CUSTOMER_CALL_ALLOWED=NO`.
