# W-0158 — Append-only verified-receipt capacity intake ledger evidence

> Ngày: `2026-09-03`  
> Trạng thái: `TESTS_PASS / LOCAL_APPEND_ONLY_LEDGER_READY / EXTERNAL_RECEIPT_NOT_RECEIVED / EXTERNAL_SUBMISSIONS_NOT_RECEIVED / CALIBRATION_NOT_RUN / NO_GATE_PROMOTION`

## 1. Kết quả

Đã thêm ledger mode vào `deploy/ci/scripts/capacity-data-intake-validator.mjs`:

```powershell
node deploy/ci/scripts/capacity-data-intake-validator.mjs `
  --verify-receipt <validation-receipt.json> `
  --expected-receipt-sha256 <trusted-hash-from-separate-delivery-record> `
  --append-intake-ledger <capacity-intake-ledger.jsonl>
```

Writer luôn gọi full W-0157 verifier trước khi tạo thư mục, lock hoặc ledger. Normal CLI không có cờ
bypass `TEST_ONLY`. Nếu verification fail thì ledger không được tạo hoặc thay đổi.

Khi append hoặc gặp duplicate, output chỉ có metadata an toàn:

```text
CAPACITY_DATA_INTAKE_LEDGER_APPENDED idempotency_key=<receipt-sha256> appended=YES entries=<n> entry_sha256=<hash> ledger_sha256=<hash> raw_rows=NO calibration=NOT_RUN
CAPACITY_DATA_INTAKE_LEDGER_ALREADY_PRESENT idempotency_key=<receipt-sha256> appended=NO entries=<n> entry_sha256=<hash> ledger_sha256=<hash> raw_rows=NO calibration=NOT_RUN
```

## 2. Ledger contract

Format là canonical one-object-per-line JSONL, schema
`m8-capacity-intake-ledger-entry.v1`. Mỗi entry chỉ chứa:

- `work_id=W-0158`, status và UTC append timestamp;
- `idempotency_key` bằng đúng `receipt_sha256`;
- receipt status, safe bundle alias và bundle-manifest hash;
- W-0157 verification status, validator/source-contract hash, authority boundary, group count và
  total record count;
- `previous_entry_sha256`, là SHA-256 của exact JSONL line ngay trước gồm LF;
- safety flags cố định giữ calibration/production/real-call fail-closed.

Ledger **không** ghi receipt path, artifact path/hash list, submission rows, source/signer aliases,
raw timing/arrival/policy/outcome/infra rows, credential hoặc PII.

## 3. Append-only và idempotency guards

- Writer không có code path truncate/rewrite ledger; chỉ mở file ở append mode.
- Cooperative lock `<ledger>.lock` dùng exclusive-create; lock đang tồn tại làm writer fail ngay.
- Existing ledger phải là regular `.jsonl`, không symlink, UTF-8, LF-terminated, canonical JSON và
  không quá 50 MiB; append cũng không được làm vượt giới hạn này.
- Toàn bộ existing entry được kiểm exact schema, PII/sensitive fields, unique receipt hash và
  previous-entry hash chain trước append.
- Receipt SHA-256 là idempotency key. Cùng hash đã có trả
  `CAPACITY_DATA_INTAKE_LEDGER_ALREADY_PRESENT`, không thêm dòng và không đổi byte ledger.
- Unique receipt được append thành đúng một dòng rồi `fsync`; output trả exact entry/ledger hash.
- Receipt verification xảy ra trước mọi ledger mutation; tampered receipt không tạo ledger.

## 4. Self-test evidence

```text
CAP-INTAKE-RECEIPT-VERIFY-06 PASS — trusted-hash receipt accepted; normal mode rejects TEST_ONLY; verify_refusals=10
CAP-INTAKE-LEDGER-07 PASS — verified-only append, metadata-only shape, idempotent duplicate, hash chain, receipt/ledger tamper refusal and cooperative lock are fail-closed
CAPACITY_DATA_INTAKE_SELFTEST_PASS valid=1 mode_guard=2 template_guard=1 receipt_guard=7 receipt_verify_guard=12 ledger_guard=9 refusals=14 external_submissions=0 calibration=NOT_RUN
```

`ledger_guard=9` phủ:

1. normal writer từ chối TEST_ONLY trước khi tạo ledger;
2. first verified append;
3. ledger không raw rows/receipt path/signer metadata;
4. duplicate receipt là byte-identical no-op;
5. unique receipt thứ hai append và nối đúng previous-entry hash;
6. tampered receipt không tạo ledger;
7. broken existing hash chain bị từ chối và không đổi ledger;
8. existing entry có field `rows` bị từ chối và không đổi ledger;
9. cooperative lock conflict bị từ chối và không đổi ledger.

## 5. Hash provenance

| Artifact | SHA-256 |
|---|---|
| Validator baseline tại W-0157 | `7229604aea4e7433aad4779cf3b1f06c02ca7a2cf92ab8bd957230bf8d1aba4f` |
| Validator current sau W-0158 | `0427abb392fc8529284a3dce378aa675dbbd0f8f6ae50fde4b988682eab6fca2` |
| M8-14 source contract, giữ nguyên | `933c55255c538987d1b86ff6d8f46b6657c68821cd00a232a55827cc751fa879` |

> Supersession: hash validator W-0158 ở trên là baseline lịch sử. W-0159 mở rộng cùng file bằng
> ledger-head checkpoint/verifier; current hash và verification record nằm tại
> [W-0159](../W-0159/README.md).

## 6. Verification record

| Kiểm tra | Kết quả |
|---|---|
| Node syntax | `PASS` |
| Validator/receipt/verifier/ledger self-test | `PASS — receipt verify=12, ledger=9, bundle refusals=14` |
| W-0158 evidence PII scan | `PASS — 1 text file, 0 binary skipped` |
| Capacity self-test | `PASS — 6/6, CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| Docs/traceability | `PASS — API_DOCS_GENERATED=14; API_DOCS_SELFTEST_PASS; TEST_TRACEABILITY_CURRENT=476` |
| Gate mirror | `PASS — 11 gates, 156 work items, 23 open decisions, production=false` |
| Markdown map | `PASS — 644 Markdown files; M8-14, W-0157, W-0158 và target worklist đều 0 unresolved` |
| Diff check | `PASS — git diff --check` |

## 7. Limitations và non-inference

- Tool là cooperative append-only writer; filesystem administrator vẫn có thể sửa/xóa/truncate file.
- Hash chain phát hiện sửa entry hoặc phá chuỗi, nhưng một tail bị truncate sạch chỉ phát hiện được nếu
  `ledger_sha256`/last `entry_sha256` trước đó đã được checkpoint ở trust store độc lập. W-0158 chỉ
  xuất các hash này, chưa triển khai external checkpoint store.
- Lock file còn lại sau process crash làm mọi append sau fail-closed; tool không tự xóa stale lock.
- Ledger không xác minh signer authority ngoài đời và không phải calibration approval, shared E2E,
  release evidence hoặc production database.
- Không có external receipt/data hoặc ledger production thật nào được tạo trong W-0158; self-test
  chỉ dùng temporary directory và xóa cuối test.
- Không sửa runtime/model/scheduler/policy/channel count.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` và `production=false` giữ nguyên.

## 8. Bước tiếp theo

Khi receipt thật đầu tiên đến, lấy expected hash từ approved separate delivery record, chạy W-0158
command và lưu output `entry_sha256`/`ledger_sha256` trở lại trust store độc lập. Chưa freeze hoặc
calibrate cho tới khi ledger có đủ bốn group từ external owner và owner chấp nhận input set.
