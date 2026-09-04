# W-0174 — Offline shared-E2E report validator

Ngày: `2026-09-04`

Baseline: `main@c213bf7663708dfca7184bf443e66d6552e2daea` + shared WIP được bảo toàn.

Trạng thái: **`TESTS_PASS_LOCAL / OFFLINE_VALIDATOR_READY /
EXTERNAL_E2E_NOT_RUN / DELIVERY_DISABLED`**

## 1. Kết quả

Đã tạo validator metadata-only tại
[`target-v1-shared-e2e-report-validator.mjs`](../../../deploy/ci/scripts/target-v1-shared-e2e-report-validator.mjs)
và template pending tại [`shared-e2e-report.template.json`](shared-e2e-report.template.json).

Validator chỉ trả `ELIGIBLE_FOR_GUARD_REVIEW_ONLY` khi đồng thời đủ:

1. exact M8 và M3 Git SHA 40 ký tự khớp giá trị reviewer truyền độc lập qua CLI;
2. M8 Target V1 OAS local còn đúng SHA-256 đã pin;
3. M3 authoritative OAS, consumer/CDC, Security auth/custody và Platform sandbox/network/TLS
   đều có ref, producer, timestamp, SHA-256 và khớp hash reviewer truyền độc lập;
4. đủ đúng thứ tự 11 case M8-07 §6, tất cả `PASS`, cùng candidate/environment/config;
5. từng case có hash metadata request/response, idempotency fingerprint, state trước/sau và bộ
   assertion riêng; bảy nhánh không được đổi state buộc before/after hash giống nhau;
6. đủ sign-off M8, M3, Security, Platform và Release, có signer–verifier separation và cùng
   candidate;
7. toàn bộ cờ raw payload, credential, PII, mock-as-E2E, guard removal, production enablement và
   real-call authorization đều `false`.

## 2. Trust boundary

- SHA/hash trong report **không tự làm authority**. Chế độ `--input` từ chối chạy nếu reviewer
  không truyền đủ sáu pin độc lập: M8 SHA, M3 SHA, M3 OAS, consumer/CDC, auth và platform.
- Ref evidence là alias metadata, không phải raw HTTP payload, token, secret, số điện thoại hay dữ
  liệu khách hàng. Validator chặn email/phone/address/credential-like value ở các ref/alias.
- Input phải là regular non-symlink file bên trong repository, tối đa 512 KiB, UTF-8 không BOM và
  không có duplicate JSON key.
- Validator không có network client, DB client hoặc khả năng gọi runtime; không sửa config/gate/ledger.
- `PASS` chỉ cho phép **xem xét** một change gỡ guard riêng. Nó không phải production approval và
  không thay chữ ký của M3/Security/Platform/Release.

## 3. Matrix bắt buộc

| ID | Nhánh | Outcome khóa |
| --- | --- | --- |
| `TV1-E2E-01` | Golden Hour accepted | `ACCEPTED` |
| `TV1-E2E-02` | 24/7 accepted qua generic endpoint | `ACCEPTED` |
| `TV1-E2E-03` | Exact replay | `DUPLICATE_ACCEPTED` |
| `TV1-E2E-04` | Same key, changed body | `IDEMPOTENCY_CONFLICT` |
| `TV1-E2E-05` | Stale version/state | `REJECTED_STALE` |
| `TV1-E2E-06` | Core blocker | `BLOCKED_BY_CORE_OR_REVIEW_REQUIRED` |
| `TV1-E2E-07` | Auth negative | `AUTH_REJECTED` + `401/403` |
| `TV1-E2E-08` | Invalid schema/result | `INVALID_DEAD_LETTER` + `422` |
| `TV1-E2E-09` | Rate limit | `RETRY_PENDING` + `429` |
| `TV1-E2E-10` | M3 outage/timeout | `RECOVERED_AFTER_BOUNDED_RETRY` |
| `TV1-E2E-11` | No-answer final | `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` |

Không cho phép nộp selected green cases rồi ghi `complete_matrix=true`.

## 4. Cách dùng khi có evidence thật

1. Copy template thành một report mới; không sửa template gốc.
2. Điền alias/ref/hash metadata và 11 case. Không dán raw body, token hay thông tin liên hệ.
3. Lấy sáu expected pin từ trust source độc lập với người lập report.
4. Chạy:

```powershell
node deploy/ci/scripts/target-v1-shared-e2e-report-validator.mjs `
  --input <report.json> `
  --expected-m8-sha <40hex> `
  --expected-m3-sha <40hex> `
  --expected-m3-oas-sha <64hex> `
  --expected-consumer-cdc-sha <64hex> `
  --expected-auth-sha <64hex> `
  --expected-platform-sha <64hex>
```

Chỉ output bắt đầu bằng
`SHARED_E2E_REPORT_VALID_ELIGIBLE_FOR_GUARD_REVIEW_ONLY` mới là schema/hash/provenance PASS.
Sau đó vẫn phải mở change review riêng; không sửa guard trong cùng bước intake này.

## 5. Verification local

| Gate | Kết quả |
| --- | --- |
| Node syntax | **PASS** |
| Positive/mutation self-test | **PASS `1 valid / 46 refusal`** |
| Pending template | **PASS `11/11` case**, output `SHARED_E2E_TEMPLATE_VALID_NOT_READY` |
| Pending template ở chế độ completed report | **REFUSED** — `status must be SHARED_E2E_EVIDENCE_COMPLETE` |
| PII scan | **PASS `6/6`** — validator/template/evidence/handoff/worklist scope |
| API docs self-test | **PASS** — 14 generated artifact; boundary/link/topology/PII checks |
| Readiness mirror | **PASS** — 11 gate, 173 work item, 23 open decision, no rung, production=false |
| GitNexus post-change | **LOW aggregate dirty tree** — 29 file, 81 symbol, 0 process; standalone untracked CLI không graph-indexed |
| Template SHA-256 | `381b6b59126955182f53a90fab2c8032547f296e57a80ad9206ee54da958d91a` |
| Validator SHA-256 | `a0abd96deb8130f274988c6964d8966c50cb19c0f4b87ef565c221690cdafc89` |
| M8 Target V1 OAS SHA-256 | `af0cb5cc3f47aaa4c8e232418c216b228fd996e316fe129a7cbf1d4636659697` |
| Artifact manifest | [`artifact-sha256.txt`](artifact-sha256.txt) |

Self-test refusal bao phủ SHA/hash độc lập sai, OAS drift, thiếu/trùng/sai thứ tự case, case không
PASS, cross-candidate/config, outcome/HTTP/assertion sai, state hash đổi ở nhánh no-transition,
timestamp ngoài run, PII/secret-like ref, summary selected-green, thiếu/sai sign-off, signer trùng
verifier, evidence sinh sau run, cờ guard/production/real-call, duplicate JSON key, oversized input
và path ngoài repo.

## 6. Phần còn lại

- M3 generic consumer + authoritative OAS + CDC exact artifacts: `NOT_RECEIVED`.
- Security auth/trust/custody evidence: `NOT_RECEIVED`.
- Platform sandbox/network/TLS/service identity evidence: `NOT_RECEIVED`.
- Shared E2E report thật: `NOT_RUN`.
- Năm sign-off trên cùng candidate: `NOT_RECEIVED`.
- Delivery guard: **giữ nguyên**; `TARGET_CONTRACT_V1=DRAFT` và
  `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Bước tiếp theo

Khi external artifact có đủ, copy template, lấy sáu pin từ trust source độc lập và chạy validator.
Chỉ sau PASS mới mở một change review riêng cho delivery guard; không gỡ guard trong W-0174.
