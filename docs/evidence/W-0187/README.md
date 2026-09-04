# W-0187 — C9 opt-out/suppression production decision-bundle validator

Ngày: `2026-09-04`

Baseline: `main@8ed62e93f5ec0ff7a4c694181ac73ee04f1eb34b` + W-0185/W-0186 shared
WIP được bảo toàn.

Trạng thái: **`TESTS_PASS_LOCAL / OFFLINE_DECISION_BUNDLE_VALIDATOR_READY /
EXTERNAL_BUNDLE_AND_SIGNATURES_NOT_RECEIVED / RUNTIME_NOT_AUTHORIZED`**.

## 1. Khoảng trống được đóng

W-0179 đã chứng minh fail-closed tuyến `D-03 → S-06`, năm authority group và coverage
`OPT-01..OPT-11`. Tuy nhiên W-0165 cố ý nhận `decision_text` free-form, còn W-0170 chỉ đóng
provenance/quorum; hai công cụ đó không xác minh một production decision bundle có nội dung đủ để
mở implementation review.

W-0187 thêm:

- `deploy/ci/scripts/opt-out-suppression-bundle-validator.mjs` — CLI offline, metadata-only;
- `opt-out-suppression-decision-bundle.template.json` — template pending, không chứa PII/credential;
- `artifact-sha256.txt` — manifest cho validator, template và chín source artifact local.

Không sửa `OptOutSuppressionPolicy`, proposer, intake, OpenAPI, database, scheduler hoặc runtime;
không bật CRM egress và không tạo suppression thật.

## 2. Guard fail-closed

Completed bundle chỉ hợp lệ khi đồng thời đủ:

1. chín source artifact còn đúng exact SHA-256 đã pin;
2. M8/M3 candidate SHA và canonical decision-bundle hash khớp pin reviewer độc lập;
3. `OPT-01..OPT-11` đủ, đúng thứ tự, tất cả `APPROVED` và có approval ref/hash;
4. signal là `EXPLICIT_CUSTOMER_ACTION_WITH_PROOF`; `Rejected`, DTMF `0` và DTMF `1` đều không
   được coi là opt-out;
5. weak signal chỉ `DISABLED` hoặc `MANUAL_REVIEW_ONLY`, tuyệt đối không tự mutate registry;
6. identity là opaque contract field do CRM phát; cấm tên field chỉ raw/direct customer identifier,
   raw phone và IVR-owned hash; exact field vẫn do owner ký trong bundle;
7. topology chọn `M3_RELAY` hoặc `CRM_PULL_QUEUE`, direct IVR→CRM egress vẫn `false`;
8. idempotency gồm signal + policy + proposal version; same-body trả original, changed-body conflict;
9. đủ ACK lifecycle accepted/duplicate/invalid/retryable/terminal, correlation/retry/DLQ;
10. effective registry writer là CRM Customer Identity; IVR/admin không được ghi suppression trực
    tiếp khi chưa có delegated authority;
11. reversal cần proof mới hơn; merge/unlink/appeal/effective timestamp phải có contract;
12. retention tách observation/proposal/ACK/idempotency/audit, `PENDING_CRM` có hạn, có legal hold và
    purge test;
13. M3 revalidate trước task; nếu không revalidate trước attempt thì bắt buộc có D-06 revoke
    callback; unknown/unavailable fail-closed và short-TTL recheck chỉ hợp lệ khi có pre-attempt;
14. admin chỉ annotate/tạo proposal theo permission/audit đã ký, không mutate registry trực tiếp;
15. đủ `13/13` signed test plan nhưng giữ `SIGNED_PLAN_NOT_EXECUTED`, không giả runtime PASS;
16. đủ tám external artifact, sáu sign-off, signer/verifier tách biệt và bind cùng canonical hash;
17. strict UTF-8, regular non-symlink file trong repo, không duplicate key/PII/secret/query ref; mọi
    safety flag và `real_customer_call_allowed` bắt buộc `false`.

## 3. Cách dùng khi có bundle thật

1. Copy template sang một file intake riêng; không sửa template gốc.
2. Điền decision, contract, test-plan, artifact và sign-off metadata; không dán raw contact, raw
   response, credential hoặc customer data.
3. Tính canonical hash cho `contract_version + decisions + production_contract + test_plans +
   external_artifacts`, rồi điền `context.decision_bundle_sha256` và bind sáu sign-off vào hash đó.
4. Reviewer lấy độc lập M8 SHA, M3 SHA, decision-bundle SHA và tám artifact SHA.
5. Chạy:

```powershell
node deploy/ci/scripts/opt-out-suppression-bundle-validator.mjs `
  --input <completed-bundle.json> `
  --expected-decision-bundle-sha <64hex> `
  --expected-m8-commit-sha <40hex> --expected-m3-commit-sha <40hex> `
  --expected-crm-proposal-contract-sha <64hex> `
  --expected-crm-registry-contract-sha <64hex> `
  --expected-crm-identity-contract-sha <64hex> `
  --expected-m3-relay-producer-contract-sha <64hex> `
  --expected-legal-privacy-packet-sha <64hex> `
  --expected-security-platform-packet-sha <64hex> `
  --expected-shared-e2e-plan-sha <64hex> `
  --expected-release-packet-sha <64hex>
```

Output cao nhất là
`OPTOUT_SUPPRESSION_BUNDLE_VALID_ELIGIBLE_FOR_IMPLEMENTATION_REVIEW_ONLY`. Nó chỉ cho phép review
change riêng; không authorize schema/sender/orchestrator, release hoặc production.

## 4. Verification local

| Gate | Kết quả |
| --- | --- |
| Node syntax | **PASS** |
| Positive/mutation self-test | **PASS `1 template / 2 valid / 52 refusal`** — weak signal `DISABLED` và `MANUAL_REVIEW_ONLY` |
| Pending template | **PASS `11 decisions / 13 tests`** — `OPTOUT_SUPPRESSION_TEMPLATE_VALID_NOT_READY` |
| Pending template ở completed mode | **REFUSED**, không thể dùng placeholder để mở review |
| Template SHA-256 | `81bc23b3e514d0fd54aa693bf8cbf67e5fccbe24c66b9c01632de7c2bd950276` |
| Validator SHA-256 | `fce19d578ab01c32c8abb4cc8fc0e1433eb5ec4b99a13d17ed2926600d21db8a` |
| Artifact manifest | **PASS `11/11`** |
| GitNexus impact trước edit | Ba symbol mới not-found, `0` impacted; standalone CLI có `0` runtime process |
| PII scope + scanner controls | **PASS `4/4`**; `CT-CI-06..06h` PASS |
| API docs / CI config | **PASS** — `14` generated artifact / config topology |
| Test traceability | **CURRENT `485`** |
| Gate mirror | **PASS `11` gate / `185` work item / `23` open decision**, production=false |
| Markdown map | **PASS `595` file; W-0187 `0` unresolved link** |
| `git diff --check` | **PASS** — chỉ có line-ending conversion warning của shared worktree |
| GitNexus detect | Aggregate shared tree **LOW `26` file / `41` symbol / `0` process**; gồm W-0185/W-0186 concurrent scope |

Mutation suite từ chối source/candidate/hash drift; thiếu/sai OPT; consent inference từ
Rejected/DTMF; weak-signal auto mutation; raw/IVR-owned identity; direct egress; contact-only
idempotency; thiếu ACK; writer sai owner; reversal/retention/freshness fail-open; admin direct write;
thiếu test/artifact/sign-off; signer-verifier collision; safety flag và extra key.

## 5. Phần external còn lại

- completed structured bundle và exact independent pins: `NOT_RECEIVED`;
- routing D-03, dispatch receipt, S-06 response và authority attestation: `NOT_RECEIVED`;
- CRM proposal/write/read/identity contract, M3 relay/producer/CDC: `NOT_RECEIVED`;
- Legal/Privacy wording, proof, retention/DSAR và Security/Platform auth/custody: `NOT_RECEIVED`;
- shared E2E, sandbox, cutover/rollback/release evidence: `NOT_RUN`;
- schema/sender/orchestrator/ACK/reversal implementation: `NOT_STARTED / NOT_AUTHORIZED`;
- `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Bước tiếp theo

CRM/M3/Legal/Product/Security điền và ký W-0187 bundle, reviewer lấy hash độc lập rồi chạy validator.
Song song Module 8 Owner cung cấp routing D-03 để đi qua W-0164 → dispatch/receipt → W-0165 →
W-0170 S-06. Chỉ sau khi cả structured bundle và provenance closure đều PASS mới mở Work ID riêng
cho implementation và shared E2E; không biến local synthetic PASS thành authority.
