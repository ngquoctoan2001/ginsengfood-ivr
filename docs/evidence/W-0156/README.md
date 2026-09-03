# W-0156 — Immutable PII-safe capacity validation receipt evidence

> Ngày: `2026-09-03`  
> Trạng thái: `TESTS_PASS / LOCAL_RECEIPT_MODE_READY / EXTERNAL_RECEIPT_NOT_CREATED / EXTERNAL_SUBMISSIONS_NOT_RECEIVED / CALIBRATION_NOT_RUN / NO_GATE_PROMOTION`

## 1. Kết quả

Đã mở rộng `deploy/ci/scripts/capacity-data-intake-validator.mjs` bằng receipt mode tùy chọn:

```powershell
node deploy/ci/scripts/capacity-data-intake-validator.mjs `
  --bundle-dir <thu-muc-bundle> `
  --receipt-out <thu-muc-evidence>\validation-receipt.json
```

Receipt chỉ được ghi **sau khi** toàn bộ validation W-0155 PASS. Nếu validation fail, receipt không
được tạo. File output dùng exclusive-create `wx`, nên path đã tồn tại sẽ bị từ chối thay vì overwrite.

CLI in hai dòng PII-safe:

```text
CAPACITY_DATA_INTAKE_PASS bundle=<safe-alias> groups=4 records=<n> manifest_sha256=<hash> authority=METADATA_ONLY_NOT_EXTERNALLY_VERIFIED
CAPACITY_DATA_INTAKE_RECEIPT_WRITTEN sha256=<exact-receipt-hash> raw_rows=NO overwrite=DENIED calibration=NOT_RUN
```

## 2. Receipt schema

Schema: `m8-capacity-intake-validation-receipt.v1`.

Receipt khóa:

- `W-0156`, validation timestamp UTC và receipt status;
- bundle ID/status và exact bundle-manifest SHA-256;
- M8-14 source path/hash;
- current validator path/hash;
- danh sách validation scope;
- mỗi submission: safe ID, group/schema, artifact hash, source/version, observation window, record
  count và signer/authority aliases;
- safety flags: `raw_rows_persisted=false`, `credential_material_persisted=false`,
  `external_authority_verified=false`, `calibration_status=NOT_RUN`,
  `production_gate_promoted=false`, `real_customer_call_allowed=NO`;
- ba limitations cố định chống suy diễn approval/readiness.

Receipt không chứa artifact path hoặc raw timing/arrival/policy/outcome/infra rows.

## 3. Receipt integrity guards

- Chỉ nhận validation result có `CAPACITY_DATA_INTAKE_VALID` và đúng bundle status.
- Normal writer từ chối `TEST_ONLY`; chỉ self-test nội bộ được phép dựng test receipt.
- Re-derive M8-14 hash và current validator hash tại lúc ghi.
- Re-validate 4/4 groups, group schema, artifact hashes, source/version, observation window,
  signer aliases, signer metadata và total record reconciliation.
- Scan receipt serialization bằng chính PII/secret/sensitive-field guards trước khi ghi.
- Exclusive-create ngăn overwrite; caller phải chọn filename mới cho một validation run mới.
- Output receipt SHA-256 được tính trên exact serialized bytes đã ghi.

## 4. Self-test evidence

```text
CAP-INTAKE-VALID-01 PASS — TEST_ONLY four-group bundle accepted only by self-test path
CAP-INTAKE-RECEIPT-05 PASS — deterministic fixed-clock receipt is hash-bound, PII-safe, raw-row-free, tamper-rejecting and no-overwrite
CAP-INTAKE-MODE-02 PASS — normal acceptance path rejects TEST_ONLY data
CAP-INTAKE-MODE-03 PASS — external status cannot hide TEST_ONLY provenance
CAP-INTAKE-TEMPLATE-04 PASS — pending template is fail-closed
CAP-INTAKE-REFUSAL PASS x14
CAPACITY_DATA_INTAKE_SELFTEST_PASS valid=1 mode_guard=2 template_guard=1 receipt_guard=7 refusals=14 external_submissions=0 calibration=NOT_RUN
```

CLI receipt path cũng được chạy với pending template và trả exit `1`; receipt target không được tạo.

## 5. Hash provenance

| Artifact | SHA-256 |
|---|---|
| Validator baseline tại W-0155 | `ce928e3be9c746657fd8fdbabd61ceec8077247c7afecb9ea56c6648913ab754` |
| Validator current sau W-0156 | `194716ade08e8e09bbcc230d1287773008cc6089828e2f4878f711689d541faa` |
| M8-14 source contract, giữ nguyên | `933c55255c538987d1b86ff6d8f46b6657c68821cd00a232a55827cc751fa879` |

> Supersession W-0157: hash `194716...` ở trên là baseline lịch sử của receipt writer tại W-0156.
> W-0157 tiếp tục thay đổi cùng validator để thêm independent verify mode; current hash và test record
> nằm tại [W-0157](../W-0157/README.md). Không dùng hash W-0156 để xác minh receipt mới.

## 6. Verification record

| Kiểm tra | Kết quả |
|---|---|
| Node syntax | `PASS` |
| Validator + receipt self-test | `PASS — valid=1, mode=2, template=1, receipt=7, refusals=14` |
| Pending-template receipt path | `PASS — exit 1, receipt absent` |
| W-0156 evidence PII scan | `PASS — 1 text file, 0 binary skipped` |
| Capacity self-test | `PASS — 6/6, CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| Docs/traceability | `PASS — API_DOCS_GENERATED=14; API_DOCS_SELFTEST_PASS; TEST_TRACEABILITY_CURRENT=476` |
| Gate mirror | `PASS — 11 gates, 154 work items, 23 open decisions, production=false` |
| Markdown map | `PASS — 642 Markdown files; M8-14, W-0155, W-0156 và target worklist đều 0 unresolved` |
| Diff check | `PASS — git diff --check` |

## 7. Non-inference

- Không có external bundle hay receipt thật nào được tạo trong W-0156.
- Self-test receipt chỉ tồn tại trong temporary directory và bị xóa cuối test.
- Receipt PASS không chứng minh signer authority, data correctness, sample adequacy, calibration,
  vendor/carrier readiness, shared E2E hoặc production approval.
- Không sửa runtime/model/scheduler/policy/channel count.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` và `production=false` giữ nguyên.

## 8. Bước tiếp theo

Khi external bundle đầu tiên đến, chạy validator với `--receipt-out` tới một filename mới, recompute
receipt SHA-256 độc lập, ghi receipt/hash vào intake ledger và giữ raw bundle ở approved secure
channel. Chỉ khi đủ 4/4 group PASS mới freeze calibration input.
