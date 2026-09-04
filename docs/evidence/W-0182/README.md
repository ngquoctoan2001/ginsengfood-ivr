# W-0182 — Offline monotonic-registry decision/sign-off intake validator

Ngày: `2026-09-04`

Baseline: `main@5c0b17085030cd69722a8422fe635bbcfbd9f5de` + shared WIP được bảo toàn.

Trạng thái: **`TESTS_PASS_LOCAL / OFFLINE_REGISTRY_DECISION_VALIDATOR_READY /
EXTERNAL_PROVIDER_AND_SIGNATURES_REQUIRED / CODE_NOT_AUTHORIZED / CALIBRATION_NOT_RUN`**

## 1. Kết quả audit B1

Chuỗi local hiện có đã bao phủ:

```text
W-0154 intake contract
  -> W-0155 four-submission validator
  -> W-0156 immutable PII-safe receipt
  -> W-0157 independent receipt verifier
  -> W-0158 append-only metadata ledger
  -> W-0159 ledger-head checkpoint/verifier
  -> W-0160 proposed monotonic external registry contract
```

Khoảng trống còn lại không nằm ở capacity arithmetic hay thêm một file ledger. W-0160 đã có
`REG-01..REG-16` và `CHK-01..CHK-15`, nhưng chưa có machine-checkable intake cho provider evidence,
ba exact-hash approval và independent verification. W-0182 đóng đúng khoảng trống đó bằng CLI
metadata-only tại `deploy/ci/scripts/capacity-registry-decision-pack-validator.mjs` và template
pending tại `docs/evidence/W-0182/capacity-registry-decision-pack.template.json`.

Không sửa `capacity-model.mjs`, scheduler, database, OpenAPI, W-0154..W-0159 writer/verifier hoặc
kết nối external trust store.

## 2. Điều kiện completed pack

Validator chỉ trả eligible khi đồng thời đủ:

1. exact W-0160 evidence, exact M8-15 contract hash và current W-0159 capacity validator hash;
2. provider/profile production có linearizable read, native revision, atomic record+head+audit CAS,
   immutable sequence/audit và server timestamp;
3. cấm delete history, client-side max, cache fallback và last-write-wins;
4. exact registry scope/schema, decimal sequence, genesis, authoritative latest-selection,
   single-transaction commit và request-ID idempotency;
5. writer, reader và auditor là ba workload alias khác nhau;
6. đủ đúng thứ tự `CHK-01..CHK-15`, đúng owner set, trạng thái `ACCEPTED`, bind đúng evidence hash
   và quyết định không có trước evidence;
7. đủ sáu evidence bundle: provider capability, canonical schema/fixtures, IAM/KMS/network/retention,
   recovery/failover drill, sandbox/cutover conformance và approval signatures;
8. ba approval `PLATFORM_OWNER`, `SECURITY_OWNER`, `MODULE8_PROJECT_OWNER`, signer khác nhau, cùng bind
   exact M8-15; independent verifier khác cả ba signer;
9. approval bundle được tạo sau ba chữ ký và independent verification diễn ra sau bundle;
10. toàn bộ safety flag giữ `false`, gồm provider selection by validator, adapter start, registry
    connection, submission receipt, calibration, gate promotion và real-customer call.

Proposal khác W-0160 phải tạo revision contract/hash mới; không sửa JSON để ép validator nhận.

## 3. Trust boundary

Completed mode bắt buộc reviewer truyền bảy SHA-256 độc lập qua CLI:

- exact M8-15 contract;
- provider capability;
- canonical schema/fixtures;
- IAM/KMS/network/retention;
- recovery/failover drill;
- sandbox/cutover conformance;
- approval signature bundle.

Hash tự khai trong JSON không phải trust anchor. Validator chỉ kiểm schema/hash/provenance/coherence;
nó không đăng nhập provider, xác minh danh tính ngoài đời, kiểm chữ ký mật mã, tạo registry record hay
chứng minh sandbox/production thật sự tồn tại.

Input phải là regular non-symlink file trong repository, tối đa 512 KiB, strict UTF-8 không BOM,
không duplicate JSON key, không email/phone/address/credential/query-bearing ref. Output cao nhất là
`CAPACITY_REGISTRY_DECISION_PACK_VALID_ELIGIBLE_FOR_ADAPTER_REVIEW_ONLY`.

## 4. Cách dùng khi có artifact thật

1. Copy template sang file intake mới; không sửa template gốc.
2. Điền safe alias/ref/hash/timestamp; không dán provider payload, token, path, raw rows hoặc danh tính
   cá nhân.
3. Lấy bảy expected hash từ nguồn độc lập với người lập JSON.
4. Chạy:

```powershell
node deploy/ci/scripts/capacity-registry-decision-pack-validator.mjs `
  --input <registry-decision-pack.json> `
  --expected-contract-sha <64hex> `
  --expected-provider-capability-sha <64hex> `
  --expected-schema-fixtures-sha <64hex> `
  --expected-custody-retention-sha <64hex> `
  --expected-recovery-drill-sha <64hex> `
  --expected-sandbox-cutover-sha <64hex> `
  --expected-approval-bundle-sha <64hex>
```

Sau PASS vẫn phải mở Work ID implementation riêng, freeze provider SDK/schema/candidate, chạy impact
analysis rồi mới viết adapter và provider conformance tests.

## 5. Verification local

| Gate | Kết quả |
| --- | --- |
| Node syntax | **PASS** |
| Positive/mutation self-test | **PASS `1 template / 1 valid / 56 refusal`** |
| Decision/approval coverage | **PASS `15 decisions / 3 approvals`** |
| Pending template | **PASS** — `CAPACITY_REGISTRY_DECISION_TEMPLATE_VALID_NOT_READY` |
| Template SHA-256 | `de94b9b39103682fd338903302625eb47269af821edde994518f4547d6e8859e` |
| Validator SHA-256 | `8fdbe90f08c5fd0ad2afb2a2083921ed4a1d5735b4c4840dffb4e1e06e8a894e` |
| W-0160 evidence SHA-256 | `01d27f785fd96e7aadfad2ac659b26c6247d7cba8a72174cdba2270ebafe02e7` |
| M8-15 contract SHA-256 LF | `e1d0fd37d610a1696b8e6b4117469ea3f8e929eff72dc95121e3ce9679200417` |
| W-0159 capacity validator SHA-256 | `4208614b44f55e8b9dc39b304021a7004e693b7dbb72ead84ab6d2cc2ed9ef83` |
| Exact committed W-0154..W-0159 intake regression | **PASS** — `valid=1, mode=2, template=1, receipt=7, receipt verify=12, ledger=9, checkpoint=13, refusals=14` |
| Current capacity-model self-test | **PASS `6/6`**, vẫn `CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| Shared-tree intake regression | `ENV_BLOCKED_BY_EXTERNAL_PLAN_DELETION` — M8-14 đang absent; không restore/stage |
| Detached capacity-model rerun | `ENV_BLOCKED_DEPENDENCY_ONLY` — clean worktree không có package `yaml`; current-tree same source PASS 6/6 |
| Artifact manifest | `docs/evidence/W-0182/artifact-sha256.txt` |
| Artifact hash verification | **PASS `4/4`** |
| PII scan | **PASS `4 files / 0 binary`**; scanner negative/clean control PASS |
| API docs self-test | **PASS** — 14 generated artifacts |
| CI config self-test | **PASS** |
| Test traceability | **PASS `485` current entries** |
| Readiness mirror | **PASS `11 gates / 182 work items / 23 open decisions`**, production=false |
| Markdown map | **PASS W-0182 `0 unresolved`** |
| GitNexus detect | **LOW aggregate tracked tree `11 files / 12 symbols / 0 process`**; CLI mới chưa có trong stale index và không có runtime caller |
| Scoped diff check | **PASS** |

Self-test refusal bao phủ source/contract và sáu external pin drift; thiếu/trùng/pending/sai owner hoặc
evidence của CHK; eventual/stale provider semantics; thiếu strong read/CAS/immutability; delete,
client max, cache và last-write-wins; registry scope/sequence/latest/atomicity/principal separation;
thiếu/sai/conditional approval; signer-verifier collision; chronology; safety flags; placeholder;
PII/phone/secret; malformed/duplicate/BOM/oversized JSON và path ngoài repository.

## 6. Dirty-tree boundary

Hai source contract M8-14/M8-15 dưới `plan/ivr-orther/` đang bị xóa trong external WIP cùng nhiều plan
file khác. W-0182 không restore, stage hay nhận quyền sở hữu các deletion đó. Validator pin W-0160
evidence và current capacity CLI từ file còn sống; M8-15 hash phải đồng thời khớp input và reviewer pin.
Candidate implementation sau này phải resolve tình trạng contract artifact trước code review.

## 7. Phần còn lại

- External capacity submissions: `0_OF_4`.
- Provider/profile/capability proof: `NOT_RECEIVED`.
- `CHK-01..CHK-15` owner decisions: `NOT_RECEIVED`.
- Platform/Security/M8 exact-hash approvals: `NOT_RECEIVED`.
- IAM/KMS/network/retention/recovery/sandbox/cutover evidence: `NOT_RECEIVED / NOT_RUN`.
- External registry connection và adapter: `NOT_STARTED / CODE_NOT_AUTHORIZED`.
- Calibration/freeze/shared E2E/production: `NOT_RUN`.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 8. Bước tiếp theo

Platform, Security và Module 8 điền completed pack, giao sáu evidence bundle và bảy independent pins.
Chỉ sau W-0182 PASS mới mở provider-specific adapter review. Nếu chưa có artifact thật, B1 dừng đúng ở
`LOCAL_TOOLCHAIN_READY / DATA_0_OF_4 / BLOCKED_EXTERNAL`.
