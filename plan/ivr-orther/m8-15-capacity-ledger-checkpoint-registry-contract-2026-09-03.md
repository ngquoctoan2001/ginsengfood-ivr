# M8-15 — Capacity ledger checkpoint monotonic registry contract

Work: `W-0160` · ngày lập: `2026-09-03`

Trạng thái:
`EVIDENCE_SUBMITTED / CONTRACT_DRAFT_READY / PLATFORM_SECURITY_M8_SIGNATURES_REQUIRED / PROVIDER_NOT_SELECTED / CODE_NOT_AUTHORIZED / EXTERNAL_TRUST_STORE_NOT_CONNECTED / NO_GATE_PROMOTION`

Owner ký bắt buộc: **Platform + Security + Module 8**. Release/Privacy có thể review phần
retention/audit nhưng không thay chữ ký của ba owner trên.

Tài liệu này là proposed contract và code-open gate. Nó không chọn provider, không tạo trust-store
record, không cấp credential và không cho phép viết adapter trước exact-hash sign-off.

## 1. Kết luận audit và mục tiêu

W-0158 tạo append-only metadata ledger có internal hash chain. W-0159 tạo checkpoint chứa exact
full-ledger hash/count/head và verifier bắt buộc một trusted checkpoint SHA-256 nằm ngoài ledger và
checkpoint. Hai lớp đó phát hiện sửa byte, truncate giữa dòng và rollback ledger về valid prefix
**nếu caller đưa đúng checkpoint mới nhất**.

Gap còn lại: một checkpoint cũ và hash cũ đều có thể hợp lệ. Nếu caller, cache hoặc trust store trả
về cặp cũ, W-0159 không có dữ kiện độc lập để biết đó là rollback. W-0160 vì vậy khóa contract cho
một registry bên ngoài có:

1. sequence tăng đúng một, không reuse/decrement/gap;
2. mỗi record bind `previous_checkpoint_sha256`;
3. một authoritative latest-selection có strong/linearizable read;
4. atomic compare-and-swap (CAS) cho record + head;
5. idempotency khi mất response hoặc retry;
6. custody tách khỏi ledger/checkpoint writer;
7. immutable audit/retention và recovery không hạ sequence.

## 2. Scope và non-goals

Trong scope:

- proposed registry record, read/commit semantics và invariants;
- concurrency, stale read, retry và error contract;
- identity, IAM, KMS, network, audit, retention và separation-of-duties requirements;
- outage, corruption, backup restore, regional disaster và split-brain recovery;
- decision matrix, signature record và acceptance trước code.

Ngoài scope:

- chọn AWS/Azure/GCP/Vault/database hoặc tên dịch vụ cụ thể;
- code SDK/adapter, schema migration, deployment, secret, KMS key hoặc network policy;
- tạo production ledger/checkpoint/registry record;
- thay đổi W-0155..W-0159 validator, capacity model, scheduler, policy hoặc channel count;
- coi contract draft là calibration, shared E2E, release hoặc production approval.

## 3. Trust boundary và ownership

| Thành phần | Quyền tối thiểu | Không được có |
| --- | --- | --- |
| W-0158 ledger writer | Append ledger qua receipt đã verify | Write/update/delete registry; sửa checkpoint cũ |
| W-0159 checkpoint writer | Read/validate ledger; exclusive-create checkpoint | Chọn latest registry; update/delete registry record |
| Registry commit service | Verify supplied checkpoint metadata; conditional create/advance | Raw rows, receipt/submission paths, ledger mutation, unconditional head write |
| Registry verifier/reader | Strong read authoritative latest record | Chọn sequence tùy ý trong production; write/delete |
| Platform custodian | Provision/operate store, replication, backup, recovery workflow | Business override để hạ sequence; đọc raw capacity rows |
| Security custodian | IAM/KMS/network/audit/break-glass policy và review | Daily ledger mutation; đơn phương xóa audit/history |
| Module 8 owner | Định nghĩa ledger identity/checkpoint binding; approve integrity semantics | Tự xác nhận Platform durability hoặc Security custody |

Principal có thể sửa ledger/checkpoint **không được** có quyền update/delete registry. Registry writer
chỉ được conditional create/advance qua controlled service. Không principal người dùng thường ngày
nào có quyền vừa mutate source artifacts vừa hạ/ghi đè trust head.

## 4. Proposed registry partition và record

Partition identity bắt buộc là bộ ba:

```text
(environment, registry_scope, ledger_id)
```

- `environment` là alias đã phê duyệt; không dùng hostname hoặc path.
- `registry_scope` cố định `CAPACITY_DATA_INTAKE_LEDGER` cho contract này.
- `ledger_id` là safe logical alias đã dùng trong W-0159, không phải filesystem path.
- Không được dùng chung partition giữa dev/lab/staging/production.

Proposed exact record `m8-capacity-intake-checkpoint-registry-record.v1`:

```yaml
schema_version: m8-capacity-intake-checkpoint-registry-record.v1
environment: <approved-environment-alias>
registry_scope: CAPACITY_DATA_INTAKE_LEDGER
ledger_id: <safe-ledger-alias>
checkpoint_sequence: <positive-decimal-string>
previous_checkpoint_sha256: <lowercase-sha256-or-null-for-genesis>
checkpoint_sha256: <lowercase-sha256-of-exact-W-0159-checkpoint-bytes>
checkpoint_schema_version: m8-capacity-intake-ledger-head-checkpoint.v1
ledger_entry_count: <positive-decimal-string>
ledger_sha256: <lowercase-sha256>
last_entry_sha256: <lowercase-sha256>
last_receipt_sha256: <lowercase-sha256>
source_contract_sha256: <lowercase-sha256>
checkpoint_validator_sha256: <lowercase-sha256>
write_request_id: <globally-unique-safe-id>
writer_principal_alias: <non-personal-workload-alias>
approval_reference: <exact-hash-signed-approval-reference>
committed_at_utc: <server-assigned-UTC-instant>
recovery_incident_reference: <safe-incident-reference-or-null>
status: COMMITTED
safety:
  raw_rows_persisted: false
  source_paths_persisted: false
  credential_material_persisted: false
  external_authority_verified: false
  calibration_status: NOT_RUN
  production_gate_promoted: false
  real_customer_call_allowed: NO
```

`checkpoint_sequence` và `ledger_entry_count` dùng decimal string không leading zero để tránh mất
độ chính xác khi đi qua JSON runtimes. Exact wire encoding/canonicalization phải được khóa ở
`CHK-04` trước code; YAML trên chỉ là mẫu dễ đọc, không phải signed payload. Normal commit dùng
`recovery_incident_reference=null`; recovery/re-attestation bắt buộc incident reference.

## 5. Record invariants `REG-01..REG-16`

| ID | Invariant bắt buộc |
| --- | --- |
| `REG-01` | Exact schema; unknown/missing field bị từ chối, không auto-coerce |
| `REG-02` | Partition là exact `(environment, registry_scope, ledger_id)`; cross-environment read/write bị từ chối |
| `REG-03` | Sequence là positive decimal string, không leading zero; genesis duy nhất là `1` |
| `REG-04` | Genesis có `previous_checkpoint_sha256=null` chỉ khi strong read + create-if-absent chứng minh partition chưa tồn tại |
| `REG-05` | Sau genesis, `new.sequence=current.sequence+1`, không gap/reuse/decrement |
| `REG-06` | `new.previous_checkpoint_sha256=current.checkpoint_sha256` exact lowercase hex |
| `REG-07` | `checkpoint_sha256` là hash exact bytes của checkpoint đã qua W-0159 verify; không hash parsed/reformatted JSON |
| `REG-08` | Ledger/count/head/receipt/source/validator fields phải exact-match checkpoint cùng hash |
| `REG-09` | `committed_at_utc` do trusted service/store gán; client time không là ordering authority |
| `REG-10` | Chỉ `COMMITTED` được latest read trả về; không expose partial/pending record |
| `REG-11` | Record committed là immutable: không update/delete; correction/recovery luôn tạo sequence mới |
| `REG-12` | `write_request_id` unique trong partition; same ID + same payload trả same result, same ID + khác payload conflict |
| `REG-13` | Record + authoritative latest head được commit atomically trong một linearizable transaction |
| `REG-14` | Không fallback cache, previous sequence hoặc caller-supplied hash khi latest read/CAS không chứng minh được |
| `REG-15` | Không raw rows, paths, signer personal identity, secrets/credentials hoặc provider token trong record/log/error |
| `REG-16` | Production read/commit phải phát audit event immutable có principal alias, request ID, partition, old/new sequence/hash và result |

## 6. Authoritative latest-selection contract

Production consumer không được tự list records rồi chọn `max(sequence)`. Nó chỉ dùng operation
`ReadLatest(partition)` của trust boundary với các yêu cầu:

1. strong/linearizable read, không eventual-consistency fallback;
2. trả đúng một `COMMITTED` record cùng native store revision/version token;
3. verify record schema/invariants và checkpoint chain link tới previous record;
4. trả `NOT_FOUND` chỉ cho genesis workflow có create-if-absent; normal verify coi đó là blocker;
5. timeout, stale replica, ambiguous read hoặc audit dependency failure đều fail-closed;
6. caller chuyển `latest.checkpoint_sha256` trực tiếp vào W-0159 verifier; production API không nhận
   tùy chọn manual sequence/hash override;
7. read cache, nếu owner vẫn yêu cầu, chỉ được dùng cho display không-authoritative. Nó không được
   cho phép verify, calibration, release hoặc recovery.

Latest pointer không phải nguồn duy nhất: immutable sequence records + WORM audit/replica phải đủ
để phát hiện pointer rollback. Provider phải chứng minh rollback/delete protection hoặc contract
không đạt production.

## 7. Atomic CAS commit protocol

### 7.1 Request

```yaml
partition: <environment/scope/ledger-id>
expected_latest:
  checkpoint_sequence: <current-sequence-or-null-for-genesis>
  checkpoint_sha256: <current-checkpoint-hash-or-null-for-genesis>
  store_revision: <opaque-native-revision-or-create-if-absent>
new_record: <record-fields-except-server-committed-at/status>
write_request_id: <same-id-as-new-record>
```

### 7.2 One transaction

Registry service phải thực hiện như một linearizable transaction:

1. authenticate/authorize workload and partition;
2. require caller-side W-0159 full-ledger verification, rồi independently recompute exact checkpoint
   bytes/hash/schema và repeated safe metadata; registry service không cần đọc ledger;
3. read current latest cùng native revision;
4. compare exact expected sequence, checkpoint hash và revision;
5. validate `REG-01..REG-16`;
6. create immutable sequence record **và** advance authoritative latest head;
7. append immutable audit result;
8. commit tất cả hoặc không expose gì;
9. strong read-after-write và trả committed record/revision.

Nếu provider không thể atomic record-create + head-advance, provider đó không đạt contract. Không
được bù bằng thứ tự “ghi record rồi ghi pointer”, background reconciliation hoặc last-write-wins.

### 7.3 Concurrency và idempotency

| Tình huống | Kết quả bắt buộc |
| --- | --- |
| Hai writer cùng expected head, checkpoint khác nhau | Chính xác một commit; writer còn lại `CAS_CONFLICT`, phải reread/revalidate |
| Retry same request ID + same payload sau mất response | Trả lại cùng committed sequence/hash, không tạo record mới |
| Same request ID + payload khác | `IDEMPOTENCY_CONFLICT`; không mutation |
| Expected sequence đúng nhưng hash/revision sai | `CAS_CONFLICT`; không force/rebase tự động |
| New sequence gap/reuse/decrement | `SEQUENCE_INVALID`; không mutation |
| Previous hash không bằng current checkpoint hash | `PREVIOUS_CHECKPOINT_MISMATCH`; không mutation |
| Checkpoint/metadata verification fail | `CHECKPOINT_NOT_VERIFIED`; không mutation |
| CAS conflict rồi client retry | Bắt buộc reread latest, dựng checkpoint mới nếu ledger đã đổi; không thay previous hash mù |

## 8. State/error contract

| Mã | Điều kiện | Disposition bắt buộc |
| --- | --- | --- |
| `REGISTRY_NOT_CONFIGURED` | Không có approved provider/profile | Block; không code/runtime fallback |
| `REGISTRY_UNAVAILABLE` | Timeout/network/store outage | Block intake checkpoint trust/calibration; alert; không cache fallback |
| `LATEST_NOT_AUTHORITATIVE` | Read không strong, ambiguous replica/revision | Block và incident |
| `LATEST_NOT_FOUND` | Không có head | Chỉ genesis flow được create-if-absent; normal verify block |
| `CAS_CONFLICT` | Expected latest/revision drift | No mutation; reread/revalidate; bounded retry policy phải ký |
| `IDEMPOTENCY_CONFLICT` | Request ID đã dùng với payload khác | Block và security/audit event |
| `SEQUENCE_INVALID` | Gap/reuse/decrement/overflow | Block và integrity incident |
| `PREVIOUS_CHECKPOINT_MISMATCH` | Chain link không trỏ current latest | Block và integrity incident |
| `CHECKPOINT_NOT_VERIFIED` | W-0159 hash/schema/ledger binding fail | Block; không commit registry |
| `PARTITION_MISMATCH` | Environment/scope/ledger khác | Block và audit |
| `PERMISSION_DENIED` | Workload/role không đủ quyền | Block và security audit; không leak store detail |
| `AUDIT_NOT_DURABLE` | Required audit write không chứng minh được | Transaction fail toàn bộ |
| `SPLIT_BRAIN_DETECTED` | Hai region nhận writer hoặc head khác nhau | Freeze writes/verification; incident + governed recovery |
| `RECOVERY_AUTHORITY_REQUIRED` | Restore/repoint/re-attest chưa đủ approvals | Block; không break-glass bypass tự động |
| `INTEGRITY_FAILURE` | Record/head/audit/backup chain không khớp | Quarantine; không chọn “record cao nhất có vẻ hợp lệ” |

Error/output chỉ chứa safe aliases, sequence, hashes, request/correlation ID và reason code. Không
log path, rows, signer personal identity, credential, raw provider response hoặc secret-bearing URL.

## 9. Custody, retention và audit requirements

### 9.1 Custody

- Store ở account/project/database trust boundary tách khỏi ledger/checkpoint storage.
- Workload identity ngắn hạn; không static token trong repo/config/log.
- Least privilege theo partition/environment; production credential không dùng ở dev/lab.
- KMS key alias/version, rotation, revoke, recovery và access-review owner phải được ký.
- Network path, TLS, egress/ingress, private endpoint và certificate trust phải có Platform/Security
  evidence; local/mock không thay thế.
- Break-glass cần two-person approval, incident reference, time bound, immutable audit và post-review;
  không được cấp update/delete history hoặc hạ sequence.

### 9.2 Retention và audit

- Committed record/head history/audit là immutable/WORM theo thời hạn đã ký; thời hạn không được
  ngắn hơn ledger/checkpoint evidence mà nó bảo vệ.
- Backup/replica tách failure domain và credential; restore drill phải chứng minh sequence/head/audit
  continuity, không chỉ chứng minh file đọc được.
- Audit tối thiểu: partition, request ID, principal alias, old/new sequence, old/new checkpoint hash,
  native revision, result code, server UTC, approval/incident reference.
- Retention duration, legal hold, deletion authority và export format đang `OWNER_DECISION_REQUIRED`;
  không điền số giả trong W-0160.

## 10. Recovery contract

Nguyên tắc: recovery không update/delete record cũ, không giảm/reuse sequence và không tự biến local
ledger thành authority. Mọi recovery tạo record mới sau verification + approvals, hoặc giữ block.

| Sự cố | Bắt buộc làm | Cấm làm |
| --- | --- | --- |
| Commit response bị mất | Lookup bằng request ID/strong latest; retry same ID+payload | Tạo request ID mới ngay và sinh duplicate |
| Checkpoint tạo xong nhưng registry chưa commit | Checkpoint là unregistered; retry exact CAS/idempotency | Dùng checkpoint đó làm trusted latest trước commit |
| Registry ahead nhưng checkpoint/artifact tạm unavailable | Khôi phục exact artifact từ protected store, verify hash | Hạ head về sequence cũ còn file local |
| Ledger bị truncate/rollback | W-0159 verify fail; restore exact ledger matching trusted latest | Advance registry để “chấp nhận” ledger đã mất tail |
| Record/head corruption | Freeze, quarantine, đối chiếu immutable records + WORM audit/replica | Chọn max sequence bằng client scan hoặc rewrite in place |
| Backup chứa sequence thấp hơn independent witness | Reject restore làm primary; tìm đủ missing chain hoặc giữ block | Coi backup đọc được là latest |
| Regional outage | Failover chỉ khi replicated head/revision continuity và single-writer fencing đã chứng minh | Cho hai region accept write song song |
| Split brain | Freeze both sides, preserve evidence, Security/Platform/M8 incident review | Last-write-wins hoặc merge theo timestamp |
| Total trust-store loss | Restore từ independent immutable backup/audit; verify chain + witness | Reconstruct authority chỉ từ ledger/checkpoint local |
| Store/key compromise | Revoke identity/key, freeze, forensic export, governed re-attestation bằng sequence mới | Xóa record/audit hoặc rollback pointer để che incident |

Sau restore/re-attestation, tạo checkpoint W-0159 mới cho exact restored ledger rồi commit sequence
`n+1` trỏ `previous_checkpoint_sha256` của last independently trusted sequence `n`, kèm
`recovery_incident_reference`. Nếu không xác định được `n`, trạng thái giữ `BLOCKED_EXTERNAL`.

RTO/RPO, replica topology, writer fencing, backup frequency, restore authority và evidence format
phải được Platform/Security/M8 ký ở `CHK-13`; W-0160 không tự đặt con số.

## 11. Decision matrix bắt buộc `CHK-01..CHK-15`

| ID | Quyết định phải ký | Owner bắt buộc | Artifact đóng |
| --- | --- | --- | --- |
| `CHK-01` | Provider/product/profile nào; bằng chứng strong read, conditional write và transaction | Platform + Security + M8 | Provider capability proof + architecture record |
| `CHK-02` | Exact partition identity, environment aliases, ledger-ID lifecycle/collision rule | Platform + M8 + Security | Signed namespace/partition registry |
| `CHK-03` | Sequence representation, genesis, max/overflow, no-gap/reuse/decrement rule | Platform + M8 + Security | Signed invariant examples |
| `CHK-04` | Exact record wire schema, encoding/canonicalization, unknown-field và versioning rule | M8 + Platform + Security | Canonical schema + positive/negative fixtures + SHA-256 |
| `CHK-05` | Exact W-0159 checkpoint/hash/metadata binding và validator-version compatibility | M8 + Security + Platform | Binding contract + drift matrix |
| `CHK-06` | Authoritative latest API, consistency level, native revision và no-cache/manual-override rule | Platform + Security + M8 | Read contract + stale-read proof |
| `CHK-07` | Atomic CAS topology cho immutable record + head + required audit | Platform + Security + M8 | Transaction design + two-writer race proof |
| `CHK-08` | Request-ID scope, retry/lost-response/idempotency/conflict semantics | Platform + M8 + Security | Idempotency contract + retry tests |
| `CHK-09` | Workload/admin/reader/auditor/break-glass IAM và separation of duties | Security + Platform + M8 | Signed IAM matrix + deny tests |
| `CHK-10` | Credential, KMS, TLS/network, rotation/revoke và environment isolation | Security + Platform + M8 | Custody/network/rotation evidence |
| `CHK-11` | Immutability/WORM, retention, legal hold, backup/replica/audit và deletion authority | Security + Platform + M8 | Retention/audit/backup policy + drill evidence |
| `CHK-12` | Outage/stale/cache/CAS retry/alert/on-call behavior và fail-closed stop rule | Platform + Security + M8 | Error matrix + outage exercise |
| `CHK-13` | Recovery, RTO/RPO, regional failover, writer fencing, split-brain và total-loss authority | Platform + Security + M8 | Signed recovery runbook + restore/failover drills |
| `CHK-14` | Cutover from no registry, reconciliation, rollback and decommission without lowering sequence | Platform + Security + M8 | Cutover/rollback packet + exact candidate SHA |
| `CHK-15` | Sandbox/shared conformance, race, tamper, truncate, backup/restore and go/no-go evidence | Platform + Security + M8 | Exact-SHA test report + three approvals |

Không đóng dòng bằng “OK”, MOCK/local test, provider brochure hoặc chữ ký một owner. Nếu câu trả lời
thay exact schema/invariant thì phát hành revision mới và ký lại exact SHA-256 toàn pack.

## 12. Approval record

| Role | Signer / authority / date / exact artifact SHA / approval reference | Trạng thái |
| --- | --- | --- |
| Platform | `<chưa nhận>` | `NOT_RECEIVED` |
| Security | `<chưa nhận>` | `NOT_RECEIVED` |
| Module 8 / Project Owner | `<chưa nhận>` | `NOT_RECEIVED` |

Approval chỉ hợp lệ khi bind exact hash của tài liệu này, trả lời `CHK-01..CHK-15`, nêu scope,
effective/cutover/rollback và authority source. Im lặng, chat “OK” hoặc ticket không có exact hash
không mở code gate.

## 13. Acceptance trước khi mở adapter

- [ ] `CHK-01..CHK-15` có câu trả lời và ba chữ ký exact-hash.
- [ ] Provider chứng minh linearizable `ReadLatest`, atomic record+head CAS và immutable history/audit.
- [ ] Canonical versioned record schema + fixtures + error contract đã ký.
- [ ] IAM/KMS/network/rotation/break-glass matrix có deny-path evidence.
- [ ] Retention/backup/RTO/RPO/recovery/split-brain runbook có drill evidence.
- [ ] Sandbox cho phép chạy genesis, sequential commits, two-writer race, lost-response retry, stale
  read, wrong previous hash, truncate/rollback, outage, restore và regional fencing.
- [ ] Candidate SHA và dependency versions được freeze; hosted/shared test chạy đúng candidate.
- [ ] Release owner nhận Platform/Security/M8 approval record; production flag vẫn false cho tới
  go/no-go riêng.

Chỉ khi tất cả checkbox trên có artifact thật mới cấp Work ID implementation cho adapter. Impact
analysis phải chạy lại trước source edit; W-0160 không cấp sẵn quyền đó.

## 14. Copy/paste signature request

**Subject:** `[M8/B1][W-0160] Chốt monotonic ledger-checkpoint registry contract trước code adapter`

**Recipients:** Platform owner; Security owner; Module 8 / Project Owner.

Vui lòng review exact-hash M8-15 và trả lời `CHK-01..CHK-15` bằng signer identity/role, authority
source, scope, date, effective/cutover/rollback và approval reference. Bắt buộc cung cấp provider
capability, IAM/KMS/network, retention/audit, RTO/RPO/recovery và sandbox/drill evidence.

Cho tới khi đủ ba chữ ký, trạng thái là `CODE_NOT_AUTHORIZED`; không chọn provider mặc định, không
code adapter/secret/network/deployment và không dùng local W-0159 test để suy production readiness.

## 15. Stop rule và bước tiếp theo

`W-0160` dừng ở decision-ready contract. External trust store, external data/ledger/checkpoint và
calibration đều chưa có; `REAL_CUSTOMER_CALL_ALLOWED=NO`, `production=false`.

Bước tiếp theo: route exact-hash M8-15 tới Platform/Security/M8 và thu ba approval record. Sau khi
đủ artifact, mở Work ID mới để freeze provider contract, chạy impact analysis và viết adapter +
conformance tests; nếu chưa đủ chữ ký thì chỉ được bổ sung clarification/evidence vào W-0160.
