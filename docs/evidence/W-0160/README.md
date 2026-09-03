# W-0160 — Monotonic ledger-checkpoint registry contract evidence

> Ngày: `2026-09-03`  
> Trạng thái: `EVIDENCE_SUBMITTED / CONTRACT_DRAFT_READY / PLATFORM_SECURITY_M8_SIGNATURES_REQUIRED / PROVIDER_NOT_SELECTED / CODE_NOT_AUTHORIZED / EXTERNAL_TRUST_STORE_NOT_CONNECTED / NO_GATE_PROMOTION`

## 1. Kết quả

Đã tạo [M8-15 monotonic registry contract](../../../plan/ivr-orther/m8-15-capacity-ledger-checkpoint-registry-contract-2026-09-03.md) cho gap còn lại sau W-0159:

- immutable sequence record và authoritative latest head;
- sequence genesis/advance, không gap/reuse/decrement;
- exact `previous_checkpoint_sha256` binding;
- strong/linearizable latest-selection, cấm client-side `max` và cache/manual fallback;
- atomic CAS cho record creation + head advance + required audit;
- request idempotency và two-writer conflict semantics;
- custody/separation-of-duties, IAM/KMS/network/retention/WORM/audit;
- outage, truncate/rollback, corruption, backup restore, regional failover, split brain và total-loss
  recovery không hạ sequence;
- `REG-01..REG-16`, `CHK-01..CHK-15`, error matrix, signature record và code-open checklist.

Không sửa validator/runtime/OpenAPI/database/config/secret/deployment và không tạo adapter hay
external trust-store record.

## 2. Audit finding

W-0159 chỉ có thể phát hiện valid-prefix rollback khi caller cung cấp hash của checkpoint mới nhất.
Một checkpoint cũ và trusted hash cũ đều có thể hợp lệ. Vì vậy:

1. hash chain tự thân không chứng minh latest;
2. list rồi client chọn `max(sequence)` không phải authoritative latest-selection;
3. eventual read/cache fallback có thể làm rollback trông hợp lệ;
4. ghi immutable record rồi cập nhật pointer bằng hai thao tác không atomic để lại partial state;
5. restore backup đọc được nhưng thấp hơn independent witness không được trở thành primary;
6. total trust-store loss không thể tái tạo authority chỉ từ local ledger/checkpoint.

Contract giữ fail-closed cho toàn bộ các trường hợp trên.

## 3. Contract checks

| Kiểm tra tài liệu | Kết quả |
| --- | --- |
| Registry invariants | `PASS — REG-01..REG-16 đủ 16/16 unique ID` |
| External decisions | `PASS — CHK-01..CHK-15 đủ 15/15 unique ID` |
| Required approval rows | `PASS — Platform, Security, Module 8 đều NOT_RECEIVED` |
| CAS race/idempotency matrix | `PASS — one-winner conflict, lost-response retry, request-ID conflict, stale head, gap và wrong previous hash` |
| Recovery matrix | `PASS — unregistered checkpoint, registry-ahead artifact loss, truncate, corruption, stale backup, region outage, split brain, total loss và compromise` |
| Code-open gate | `PASS — adapter bị chặn tới exact-hash 3-owner sign-off + provider/sandbox/drill evidence` |
| Source/runtime mutation | `NONE — docs-only W-0160` |

## 4. Hash provenance

| Artifact | SHA-256 |
| --- | --- |
| M8-15 contract draft | `e1d0fd37d610a1696b8e6b4117469ea3f8e929eff72dc95121e3ce9679200417` |
| W-0159 validator, giữ nguyên | `4208614b44f55e8b9dc39b304021a7004e693b7dbb72ead84ab6d2cc2ed9ef83` |
| M8-14 source contract, giữ nguyên | `933c55255c538987d1b86ff6d8f46b6657c68821cd00a232a55827cc751fa879` |

Hash M8-15 trên là artifact để review. Bất kỳ thay đổi nào vào schema/invariant/decision matrix đều
phải tạo hash mới và ba owner ký lại exact hash.

## 5. Verification record

| Kiểm tra | Kết quả |
| --- | --- |
| M8-15 structural/count/hash check | `PASS — 346 lines; REG 16; CHK 15; approvals NOT_RECEIVED 3; SHA-256 exact` |
| W-0160 artifact PII scan | `PASS — contract 1/1 + evidence 1/1; 0 binary skipped` |
| Capacity validator regression | `PASS — checkpoint=13, ledger=9, receipt verify=12, bundle refusals=14` |
| Capacity model self-test | `PASS — 6/6, CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| Docs/traceability | `PASS — API_DOCS_GENERATED=14; API_DOCS_SELFTEST_PASS; TEST_TRACEABILITY_CURRENT=476` |
| Gate mirror | `PASS — 11 gates, 158 work items, 23 open decisions, production=false` |
| Markdown map | `PASS — 647 Markdown files; M8-15/W-0160/target/tracker 0 unresolved` |
| GitNexus detect-changes | `LOW — 42 tracked-WIP files, 170 indexed symbols, 0 affected process; W-0160 docs untracked/unindexed` |
| Diff check | `PASS — git diff --check` |

## 6. Non-inference và blockers

- Contract là proposed decision pack, chưa phải `ACCEPTED` hay production design đã ký.
- Chưa chọn provider/profile; chưa có capability proof cho linearizable read, atomic CAS hoặc WORM.
- Chưa có production workload identity, IAM/KMS/network/rotation/break-glass evidence.
- Chưa có retention, RTO/RPO, backup/restore, regional fencing hoặc split-brain drill.
- Chưa có external submission, receipt, ledger, checkpoint hoặc registry record thật.
- Không có calibration/shared E2E/release/go-live evidence.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` và `production=false` giữ nguyên.

## 7. Bước tiếp theo

Route exact-hash M8-15 cho Platform/Security/M8, thu câu trả lời `CHK-01..CHK-15` và ba approval
record. Không mở adapter Work ID cho tới khi provider contract, custody/recovery và sandbox/drill
evidence đều có thật.
