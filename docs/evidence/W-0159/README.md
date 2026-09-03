# W-0159 — Ledger-head checkpoint/verifier evidence

> Ngày: `2026-09-03`  
> Trạng thái: `TESTS_PASS / LOCAL_LEDGER_HEAD_CHECKPOINT_VERIFIER_READY / EXTERNAL_TRUST_STORE_NOT_CONNECTED / EXTERNAL_LEDGER_NOT_RECEIVED / CALIBRATION_NOT_RUN / NO_GATE_PROMOTION`

## 1. Kết quả

Đã mở rộng `deploy/ci/scripts/capacity-data-intake-validator.mjs` bằng hai mode độc lập:

```powershell
node deploy/ci/scripts/capacity-data-intake-validator.mjs `
  --checkpoint-intake-ledger <capacity-intake-ledger.jsonl> `
  --ledger-id <safe-ledger-alias> `
  --checkpoint-out <ledger-head-checkpoint.json>

node deploy/ci/scripts/capacity-data-intake-validator.mjs `
  --verify-intake-ledger <capacity-intake-ledger.jsonl> `
  --checkpoint <ledger-head-checkpoint.json> `
  --expected-checkpoint-sha256 <latest-trusted-hash-from-separate-trust-store>
```

Checkpoint chỉ được tạo sau khi toàn bộ W-0158 ledger đã qua regular-file, UTF-8, canonical JSONL,
exact schema, PII, unique receipt và hash-chain validation. File checkpoint dùng exclusive-create;
không có overwrite path.

Verifier kiểm exact checkpoint bytes với trusted SHA-256 **trước khi parse**, sau đó revalidate toàn
bộ ledger và so `ledger_sha256`, `entry_count`, last `entry_sha256`, last `receipt_sha256` và source
contract hash. Vì vậy một ledger bị thay bằng valid prefix vẫn fail dù internal hash chain của prefix
đó tự hợp lệ.

## 2. Checkpoint contract

Schema `m8-capacity-intake-ledger-head-checkpoint.v1` chỉ chứa:

- `work_id=W-0159`, status và UTC checkpoint timestamp;
- safe `ledger_id`, ledger-entry schema version và entry count;
- exact full-ledger SHA-256, last-entry SHA-256, last-receipt SHA-256;
- source-contract và checkpoint-validator SHA-256;
- authority/safety/limitation flags fail-closed.

Checkpoint không ghi ledger path, receipt path, submission/signer metadata, entry payload, raw rows,
credentials hoặc PII.

## 3. Trust boundary và rollback rule

- `expected-checkpoint-sha256` phải đến từ trust store tách riêng khỏi **cả ledger lẫn checkpoint**.
- Caller/trust-store adapter phải chọn checkpoint mới nhất theo một quy tắc monotonic đã được owner
  phê duyệt. Tool local không xác minh freshness/authority của giá trị do caller cung cấp.
- Nếu attacker rollback đồng thời ledger, checkpoint và làm caller dùng lại hash checkpoint cũ hợp
  lệ, verifier không thể tự biết đó là checkpoint stale. Đây là blocker thật cho external trust-store
  integration, không được che bằng self-test.
- Append hợp lệ sau checkpoint cũng làm verifier fail cho tới khi tạo checkpoint mới và trust store
  cập nhật atomically/monotonically.

## 4. Self-test evidence

```text
CAP-INTAKE-LEDGER-07 PASS — verified-only append, metadata-only shape, idempotent duplicate, hash chain, receipt/ledger tamper refusal and cooperative lock are fail-closed
CAP-INTAKE-CHECKPOINT-REFUSAL PASS mutation=missing-trust-anchor
CAP-INTAKE-CHECKPOINT-REFUSAL PASS mutation=wrong-trust-anchor
CAP-INTAKE-CHECKPOINT-REFUSAL PASS mutation=byte-tamper
CAP-INTAKE-CHECKPOINT-REFUSAL PASS mutation=ledger-hash-drift
CAP-INTAKE-CHECKPOINT-REFUSAL PASS mutation=raw-row-field
CAP-INTAKE-CHECKPOINT-08 PASS — immutable metadata checkpoint and trusted-hash verifier reject checkpoint tamper, valid-prefix rollback, partial truncation and post-checkpoint append
CAPACITY_DATA_INTAKE_SELFTEST_PASS valid=1 mode_guard=2 template_guard=1 receipt_guard=7 receipt_verify_guard=12 ledger_guard=9 checkpoint_guard=13 refusals=14 external_submissions=0 calibration=NOT_RUN
```

`checkpoint_guard=13` phủ:

1. normal writer từ chối TEST_ONLY và không tạo checkpoint;
2. internal self-test tạo immutable metadata-only checkpoint;
3. exact trusted checkpoint hash + exact ledger PASS trong internal test mode;
4. normal verifier từ chối TEST_ONLY;
5. overwrite checkpoint bị từ chối, byte cũ giữ nguyên;
6. thiếu trust anchor bị từ chối;
7. sai trust anchor bị từ chối;
8. checkpoint byte tamper bị từ chối;
9. sửa `ledger_sha256` rồi recompute checkpoint hash vẫn bị từ chối bởi ledger comparison;
10. chèn raw-row field bị exact-schema/PII-safe guard từ chối;
11. valid-prefix rollback về ledger một entry bị từ chối;
12. partial tail truncation bị từ chối;
13. append entry mới sau checkpoint bị từ chối cho tới checkpoint mới.

## 5. Hash provenance

| Artifact | SHA-256 |
|---|---|
| Validator baseline tại W-0158 | `0427abb392fc8529284a3dce378aa675dbbd0f8f6ae50fde4b988682eab6fca2` |
| Validator current sau W-0159 | `4208614b44f55e8b9dc39b304021a7004e693b7dbb72ead84ab6d2cc2ed9ef83` |
| M8-14 source contract, giữ nguyên | `933c55255c538987d1b86ff6d8f46b6657c68821cd00a232a55827cc751fa879` |

## 6. Verification record

| Kiểm tra | Kết quả |
|---|---|
| Node syntax | `PASS` |
| Validator/receipt/verifier/ledger/checkpoint self-test | `PASS — checkpoint=13, ledger=9, receipt verify=12, bundle refusals=14` |
| Checkpoint CLI missing-argument/no-artifact guard | `PASS — exit 1; artifact_exists=False` |
| W-0159 evidence PII scan | `PASS — 1 text file, 0 binary skipped` |
| Capacity self-test | `PASS — 6/6, CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| Docs/traceability | `PASS — API_DOCS_GENERATED=14; API_DOCS_SELFTEST_PASS; TEST_TRACEABILITY_CURRENT=476` |
| Gate mirror | `PASS — 11 gates, 157 work items, 23 open decisions, production=false` |
| Markdown map | `PASS — 645 Markdown files; W-0159/target worklist 0 unresolved` |
| GitNexus detect-changes | `LOW — 42 tracked-WIP files, 170 indexed symbols, 0 affected process; new validator remains untracked/unindexed` |
| Diff check | `PASS — git diff --check` |

## 7. Limitations và non-inference

- Đây là local file checkpoint + verifier, không phải external trust-store adapter hay vault.
- Tool không tự xác minh trust-store custody, writer identity, retention, monotonic latest-selection,
  compare-and-swap hoặc disaster-recovery semantics.
- Không có external submission, receipt, production ledger, checkpoint hoặc trust-store record thật
  nào được tạo; self-test chỉ dùng temporary directory.
- Checkpoint PASS không chứng minh signer authority, business-data correctness, calibration, shared
  E2E, release approval hoặc production readiness.
- Không sửa runtime/model/scheduler/policy/channel count.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` và `production=false` giữ nguyên.

## 8. Bước tiếp theo

W-0160 đã lập [M8-15 monotonic registry contract](../../../plan/ivr-orther/m8-15-capacity-ledger-checkpoint-registry-contract-2026-09-03.md)
với sequence/latest-selection, previous checkpoint hash, atomic CAS, custody và recovery. Trạng thái
vẫn `EXTERNAL_SIGNATURES_REQUIRED / CODE_NOT_AUTHORIZED`; chỉ code adapter sau khi
Platform/Security/M8 ký exact hash và giao provider/sandbox/drill evidence. Chưa freeze/calibrate
trước 4/4 external submission.
