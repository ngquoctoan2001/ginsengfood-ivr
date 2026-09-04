# W-0183 — Offline contact/dial-token production-bundle validator

Ngày: `2026-09-04`

Baseline: `main@5c0b17085030cd69722a8422fe635bbcfbd9f5de` + scoped W-0183/W-0184
documentation WIP; external plan-file deletions được bảo toàn.

Trạng thái: **`TESTS_PASS_LOCAL / OFFLINE_PRODUCTION_BUNDLE_VALIDATOR_READY /
EXTERNAL_BUNDLE_AND_SIGNATURES_NOT_RECEIVED / CODE_NOT_AUTHORIZED`**.

## 1. Kết quả

Đã hoàn tất:

- `deploy/ci/scripts/dial-token-production-bundle-validator.mjs` — CLI offline, metadata-only;
- `dial-token-production-bundle.template.json` — template pending, không chứa contact/token thật;
- `artifact-sha256.txt` — manifest 9 artifact local được pin.

Validator không chọn issuer, token model, TTL, resolver, vendor hoặc custody thay owner. Nó không
đọc raw contact/token, gọi network, mount secret, ghi DB/ledger, tạo adapter hoặc mở egress. Output
cao nhất là `DIAL_TOKEN_DECISION_BUNDLE_VALID_ELIGIBLE_FOR_IMPLEMENTATION_REVIEW_ONLY`; output này
không phải runtime, release hoặc production authorization.

## 2. Contract fail-closed được khóa

Completed input chỉ được nhận khi đồng thời đủ:

1. exact hash của W-0150, T-04, IR-03, IR-06, task OpenAPI, resolver port và intake service;
2. exact M8/M3 candidate SHA, environment/config và review chronology;
3. đủ đúng thứ tự `DTK-01..DTK-15`, trạng thái `APPROVED`, ref/hash và canonical decision hash;
4. chọn đúng một trong bốn model: scalar reusable có attempt binding, per-attempt array, reissue
   endpoint hoặc token bundle; wire delta/reissue/replay/refresh phải nhất quán với model đã chọn;
5. contact authority/producer, requiredness, TTL/skew, subject/scope/audience/provider binding,
   resolver output, raw-E.164 boundary, protocol/auth/timeout/idempotency và trust diagram;
6. custody/workload identity/least privilege/rotation/revoke, atomic concurrency và cross-scope
   replay guard;
7. fail-closed outage/retry/deadline, technical failure không tính customer attempt;
8. audit outcome-only, cấm log raw token/ciphertext/destination/raw E.164, retention/purge proof;
9. vendor capability, recording-off, allowlist, kill switch, caller-ID/disposition/DTMF;
10. contract-before-runtime, sandbox → allowlisted lab → pilot, exact candidate, rollback và
    production vẫn disabled;
11. đủ `14/14` canonical test plan từ valid contact đến TTL, replay, outage, rotation, concurrency,
    lost response, privacy/purge và rollback; không claim đã chạy runtime;
12. đủ `9/9` sign-off: Project Owner, M8, M3 Contact, Product, Security, Platform,
    Telephony/vendor, Privacy/Legal và Release; signer/verifier tách biệt, cùng bind exact bundle;
13. đủ `8/8` external artifact và SHA-256 reviewer truyền độc lập;
14. strict UTF-8 không BOM, regular non-symlink file trong repo, tối đa 768 KiB, không duplicate
    JSON key, email/phone/credential/query-bearing ref hoặc safety flag khác `false`.

## 3. Verification local

| Gate | Kết quả |
| --- | --- |
| Node syntax | **PASS** |
| Positive model/self-test | **PASS `4 token models / 64 refusal`** |
| Pending template | **PASS `15 decisions / 14 test plans`** — `DIAL_TOKEN_TEMPLATE_VALID_NOT_READY` |
| Production authorization từ template | **NO** — `production_authorized=false` |
| Validator SHA-256 | `e1b4445289257b0134c9571fdb4421b2706288c88ca254dd50f6c5f2aca2d6b1` |
| Template SHA-256 | `31ae0ee58c88b666faf8362f386cb2512ac5dcfc35bb18c2107034e49fcd1bc8` |
| Local artifact manifest | `artifact-sha256.txt` — **PASS `9/9`** |
| Detached clean verification | **PASS** — exact `5c0b170` + scoped W-0150/W-0183 files; self-test, generated-template byte equality và manifest `9/9` |
| PII scan | **PASS `4 files / 0 binary`**; scanner negative-control self-test PASS |
| API docs / CI config | **PASS `14` generated artifact / PASS** |
| Test traceability | **CURRENT `485`** |
| Readiness mirror | **PASS `11 gates / 182 work items / 23 open decisions`**, production=false |
| Markdown map | **PASS W-0183 `0 unresolved`** |
| GitNexus detect | **LOW aggregate tracked tree `11 files / 12 symbols / 0 process`**; standalone CLI chưa có trong index |
| Scoped diff / new-file whitespace | **PASS** |
| PII | **PASS `4 files / 0 binary`** + scanner negative-control self-test |
| API docs / CI config | **PASS** — `14` generated artifact / config topology |
| Test traceability | **CURRENT `485`** |
| Readiness mirror | **PASS `11 gates / 182 work items / 23 open decisions`**, production=false |
| Markdown map | **W-0183 `0 unresolved`**; aggregate backlog `145` |
| Scoped diff | **PASS** — không có whitespace error |

Mutation suite từ chối source/candidate/hash drift; raw E.164 hoặc token exposure; model/wire/reissue/
TTL/replay contradiction; fail-open resolver/retry; mapping key trong IVR; sai custody/rotation;
logging/recording/allowlist/sandbox/production flag; thiếu/sai decision, artifact, test plan hoặc
sign-off; signer-verifier collision; chronology; PII/credential; extra/duplicate/oversized/outside
repository input.

## 4. Cách dùng khi có bundle ký thật

1. Copy template sang file intake riêng trong `ci-artifacts/W-0183/`; không sửa template gốc.
2. Điền alias/ref/hash/timestamp, không dán raw contact, raw token, ciphertext, credential hoặc
   customer data.
3. Tính canonical `decision_coverage_sha256`; tính `bundle_sha256` sau khi bỏ trường hash tự thân.
4. Reviewer lấy độc lập: M8 SHA, M3 SHA, bundle SHA và tám external artifact SHA.
5. Chạy:

```powershell
node deploy/ci/scripts/dial-token-production-bundle-validator.mjs `
  --input <completed-bundle.json> `
  --m8-commit-sha <40hex> --m3-commit-sha <40hex> --bundle-sha256 <64hex> `
  --m3-contact-sha256 <64hex> --issuer-token-sha256 <64hex> `
  --security-threat-sha256 <64hex> --platform-custody-sha256 <64hex> `
  --telephony-capability-sha256 <64hex> --privacy-retention-sha256 <64hex> `
  --shared-e2e-plan-sha256 <64hex> --release-packet-sha256 <64hex>
```

Chỉ output `...ELIGIBLE_FOR_IMPLEMENTATION_REVIEW_ONLY` mới là schema/hash/coherence PASS; chief
auditor vẫn phải xác minh authority và artifact trong trust store/ticket system-of-record.

## 5. Routing D-02 và closure S-08

Bundle W-0183 không tự gửi đi. Module 8 Owner/chief auditor còn phải copy W-0164 routing template và
điền riêng row `D-02`: recipient alias, role/organization, authority source ref, approved channel,
destination ref, due time, dispatch authorizer và authorization time. Không ghi email/số điện thoại/
credential vào repository.

Sau khi routing input W-0164 PASS:

```text
W-0164 routing D-02
  -> dispatch thật + receipt export/hash
  -> W-0165 response bundle S-08
  -> independent authority attestations
  -> W-0170 closure S-08
```

W-0170 chỉ đóng khi đủ bảy authority group và `DTK-01..DTK-15`. Focused synthetic guard W-0184 đã
chứng minh local positive/refusal path, nhưng không thay receipt hoặc signature thật.

## 6. Dữ liệu external còn thiếu

- completed W-0183 bundle và exact M8/M3/bundle pins: `NOT_RECEIVED`;
- tám external artifact + hash: `NOT_RECEIVED`;
- chín sign-off và independent verification: `NOT_RECEIVED`;
- routing D-02, approved destination và dispatch authority: `NOT_RECEIVED`;
- dispatch receipt, S-08 responses và authority attestations: `NOT_RECEIVED`;
- shared CDC/E2E, sandbox/lab/pilot/cutover/release evidence: `NOT_RUN`;
- production vault/resolver/adapter/secret/egress: `CODE_NOT_AUTHORIZED`;
- `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Bước tiếp theo

Module 8 Owner gửi routing metadata D-02; M3/Security/Platform/Telephony/Product/Privacy/Release điền
và ký completed W-0183 bundle, giao tám artifact và independent pins. Chạy W-0183 trước, sau đó
W-0164 → dispatch/receipt → W-0165 → authority attestation → W-0170 S-08. Chỉ sau cả bundle và
closure PASS mới mở Work ID implementation riêng.
