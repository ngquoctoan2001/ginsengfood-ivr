# W-0157 — Independent capacity-validation receipt verifier evidence

> Ngày: `2026-09-03`  
> Trạng thái: `TESTS_PASS / LOCAL_RECEIPT_VERIFIER_READY / EXTERNAL_RECEIPT_NOT_RECEIVED / EXTERNAL_SUBMISSIONS_NOT_RECEIVED / CALIBRATION_NOT_RUN / NO_GATE_PROMOTION`

## 1. Kết quả

Đã thêm chế độ chỉ-đọc vào `deploy/ci/scripts/capacity-data-intake-validator.mjs`:

```powershell
node deploy/ci/scripts/capacity-data-intake-validator.mjs `
  --verify-receipt <validation-receipt.json> `
  --expected-receipt-sha256 <trusted-hash-from-separate-delivery-record>
```

Chế độ này không cần raw bundle và không ghi ledger. Nó là precheck bắt buộc trước khi một receipt
được chuyển sang intake ledger.

Khi PASS, CLI chỉ in metadata an toàn:

```text
CAPACITY_DATA_INTAKE_RECEIPT_VERIFY_PASS bundle=<safe-alias> groups=4 records=<n> receipt_sha256=<hash> authority=METADATA_ONLY_NOT_EXTERNALLY_VERIFIED ledger_precheck=PASS_METADATA_ONLY calibration=NOT_RUN
```

## 2. Trust boundary

`--expected-receipt-sha256` là bắt buộc và phải đến từ nguồn nằm **ngoài receipt đang kiểm**, ví dụ
signed delivery record, dispatch receipt hoặc ledger pre-registration được owner kiểm soát. Không được
tính hash từ chính file candidate tại thời điểm intake rồi dùng kết quả đó làm trust anchor: cách đó
không phát hiện được người sửa cả file lẫn hash đi kèm.

Verifier kiểm exact bytes trước khi parse JSON. Vì vậy thêm whitespace, đổi field hoặc serialize lại
đều làm fail nếu hash tin cậy không đổi.

## 3. Các guard đã triển khai

- receipt path phải là regular `.json`, không symlink, không rỗng, tối đa 50 MiB;
- expected hash phải là lowercase SHA-256 hợp lệ và khớp exact receipt bytes;
- UTF-8, PII/phone/email/address/secret scan và forbidden sensitive-field scan;
- exact top-level/nested/submission schema của `m8-capacity-intake-validation-receipt.v1`;
- `work_id=W-0156`, external receipt/bundle status trong normal mode; `TEST_ONLY` chỉ self-test;
- current validator path/hash và M8-14 current source-contract path/hash;
- exact validation scope, authority boundary, bốn group theo canonical order và đúng group schema;
- unique submission/provenance aliases, source/version, time window, artifact hash và record count;
- group count/total record reconciliation;
- exact safety flags và ba limitations chống suy diễn calibration/E2E/production approval.

## 4. Self-test evidence

```text
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=missing-trust-anchor
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=wrong-trust-anchor
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=byte-tamper
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=validator-drift
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=source-contract-drift
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=provenance-drift
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=count-drift
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=safety-drift
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=schema-drift
CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=pii-injection
CAP-INTAKE-RECEIPT-VERIFY-06 PASS — trusted-hash receipt accepted; normal mode rejects TEST_ONLY; verify_refusals=10
CAPACITY_DATA_INTAKE_SELFTEST_PASS valid=1 mode_guard=2 template_guard=1 receipt_guard=7 receipt_verify_guard=12 refusals=14 external_submissions=0 calibration=NOT_RUN
```

`receipt_verify_guard=12` gồm một positive pinned-hash verification, normal-mode `TEST_ONLY`
rejection và 10 negative controls nêu trên.

## 5. Hash provenance

| Artifact | SHA-256 |
|---|---|
| Validator baseline tại W-0156 | `194716ade08e8e09bbcc230d1287773008cc6089828e2f4878f711689d541faa` |
| Validator current sau W-0157 | `7229604aea4e7433aad4779cf3b1f06c02ca7a2cf92ab8bd957230bf8d1aba4f` |
| M8-14 source contract, giữ nguyên | `933c55255c538987d1b86ff6d8f46b6657c68821cd00a232a55827cc751fa879` |

> Supersession W-0158: hash `722960...` ở trên là baseline lịch sử của verifier tại W-0157.
> W-0158 tiếp tục thay đổi cùng CLI để thêm append-only intake-ledger writer; current validator hash
> và test record nằm tại [W-0158](../W-0158/README.md). Không dùng hash W-0157 cho receipt mới.

## 6. Verification record

| Kiểm tra | Kết quả |
|---|---|
| Node syntax | `PASS` |
| Validator + receipt verifier self-test | `PASS — valid=1, mode=2, template=1, receipt=7, receipt verify=12, bundle refusals=14` |
| Normal mode rejects TEST_ONLY receipt | `PASS` |
| W-0157 evidence PII scan | `PASS — 1 text file, 0 binary skipped` |
| Capacity self-test | `PASS — 6/6, CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| Docs/traceability | `PASS — API_DOCS_GENERATED=14; API_DOCS_SELFTEST_PASS; TEST_TRACEABILITY_CURRENT=476` |
| Gate mirror | `PASS — 11 gates, 155 work items, 23 open decisions, production=false` |
| Markdown map | `PASS — 643 Markdown files; M8-14, W-0156, W-0157 và target worklist đều 0 unresolved` |
| Diff check | `PASS — git diff --check` |

## 7. Compatibility và non-inference

- Receipt W-0156 cũ sẽ fail nếu `validator.sha256` không còn bằng bytes validator hiện hành. Đây là
  fail-closed behavior trước ledger, không phải bằng chứng receipt đã bị sửa.
- Xác minh lịch sử sau khi validator thay đổi cần lưu/attest exact validator artifact theo hash; W-0157
  chưa tạo artifact registry đó.
- Không có external bundle/receipt thật nào được tạo hoặc intake trong W-0157.
- Self-test receipt chỉ tồn tại trong temporary directory và bị xóa cuối test.
- PASS chỉ xác nhận integrity/schema/current-contract binding và metadata shape; không xác minh
  signer authority ngoài đời, business correctness, sample adequacy, calibration, shared E2E hoặc
  production approval.
- Không sửa runtime/model/scheduler/policy/channel count.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` và `production=false` giữ nguyên.

## 8. Bước tiếp theo

Khi external bundle đầu tiên đến: validate bundle và ghi receipt vào filename mới; chuyển exact
receipt hash qua một approved separate channel/record; chạy W-0157 verifier bằng hash đó; chỉ khi
precheck PASS mới append metadata vào intake ledger. Chưa calibration nếu chưa đủ 4/4 group PASS.
