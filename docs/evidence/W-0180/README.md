# W-0180 — Attempt-policy production bundle validator

Ngày: `2026-09-04`

Baseline: `main@5c0b17085030cd69722a8422fe635bbcfbd9f5de` + shared WIP được bảo toàn.

Trạng thái: **`TESTS_PASS_LOCAL / OFFLINE_VALIDATOR_READY /
EXTERNAL_INPUT_NOT_RECEIVED / PRODUCTION_POLICY_NOT_APPROVED`**

## 1. Kết quả và ranh giới

Đã tạo validator metadata-only tại
`deploy/ci/scripts/attempt-policy-production-bundle-validator.mjs` và template pending tại
`docs/evidence/W-0180/attempt-policy-production-bundle.template.json`.

Đây là phần khắc phục local duy nhất có thể làm an toàn khi `ATP-01..ATP-15` chưa được ký. Validator
không chọn số policy, không promote/rename `mock-lab-v1`, không sửa scheduler, registry, OpenAPI,
DB, seed hoặc config, không kết nối M3 và không cho phép gọi khách hàng thật.

Output PASS chỉ là `ELIGIBLE_FOR_RUNTIME_REVIEW_ONLY`. Implementation production vẫn phải mở thành
change riêng sau khi authority và evidence thật được xác minh.

## 2. Những điều validator khóa fail-closed

1. Local source W-0151, T-09, functional spec và domain policy phải còn đúng SHA-256 đã pin.
2. `policy_version` phải là version mới, không chứa mock/candidate/unapproved; bundle phải có đúng hai
   program `GOLDEN_HOUR` và `ALWAYS_ON`.
3. Mỗi program có exact max-attempt/offset/window; offset bắt đầu `0`, tăng nghiêm ngặt, số offset
   bằng max attempt và mọi offset nằm trong window.
4. Bundle phải quyết định T0/clock skew, counted/terminal taxonomy, manual retry, technical retry và
   backoff; invalid input/technical failure không được tính thành customer attempt.
5. Bundle phải quyết định timezone, quiet-hours, holiday và window-crossing.
6. Wire giữ exact version + snapshot, M3 authoritative, mismatch `409 IVR_POLICY_MISMATCH`, và bind
   producer/CDC vào hash evidence M3.
7. Registry phải có controlled writer/reader, four-eyes, atomic two-program publish, immutable
   version, no hard-delete, effective/retire, custody/recovery, audit hash và bind SHA-256 của
   evidence custody/recovery vào canonical bundle.
8. ATP-11 phải chọn rõ một pre-dial coherence strategy; unknown/unavailable/drift đều `FAIL_CLOSED`
   và kiểm trước mỗi dial. Validator không tự chọn strategy thay owner.
9. Capacity/channel-token/rate-limit phải được recalibrate cho bundle; cutover/in-flight/rollback/
   canary phải đầy đủ.
10. Đủ đúng thứ tự `ATP-01..ATP-15`, tất cả `APPROVED`, có ref/SHA-256 riêng; canonical hash của
    toàn bộ 15 dòng phải được bind vào authority provenance bên trong bundle.
11. Đủ chữ ký Product, Order Core, M3 Owner; signer/verifier tách biệt và cùng bind exact
    `policy_version + bundle_sha256`.
12. M3 producer/CDC, registry custody/recovery, capacity/token, shared E2E và release packet phải có
    provenance; từng hash khớp pin reviewer truyền độc lập.
13. Canonical SHA-256 được tính từ bundle sau khi bỏ trường hash tự thân; hash trong file và hash
    reviewer truyền phải cùng khớp.
14. Input chỉ được là UTF-8 không BOM, regular non-symlink file trong repo, tối đa 512 KiB, không
    duplicate JSON key; ref/alias bị quét email, phone-like, query/fragment và secret-like material.
15. Production vẫn default disabled, `real_customer_call_allowed=false`; mọi safety flag phải false.

## 3. Cách dùng khi nhận bundle thật

1. Copy template sang file mới; không sửa template gốc.
2. Điền toàn bộ quyết định và metadata ref/hash, không nhúng raw row, payload, credential hoặc PII.
3. Canonicalize object `bundle` theo JSON key sort, bỏ `bundle_sha256`, tính SHA-256 UTF-8 rồi điền
   lại `bundle_sha256`.
4. Reviewer lấy bảy hash từ trust source độc lập với người lập bundle.
5. Chạy:

```powershell
node deploy/ci/scripts/attempt-policy-production-bundle-validator.mjs `
  --input <completed-bundle.json> `
  --expected-bundle-sha <64hex> `
  --expected-decision-pack-sha <64hex> `
  --expected-m3-producer-sha <64hex> `
  --expected-registry-sha <64hex> `
  --expected-capacity-sha <64hex> `
  --expected-shared-e2e-sha <64hex> `
  --expected-release-packet-sha <64hex>
```

Chỉ output bắt đầu bằng
`ATTEMPT_POLICY_BUNDLE_VALID_ELIGIBLE_FOR_RUNTIME_REVIEW_ONLY` mới là schema/hash/provenance PASS.
Sau đó vẫn phải review implementation, rerun shared E2E và đi qua release gate riêng.

## 4. Verification local

| Gate | Kết quả |
| --- | --- |
| Node syntax | **PASS** |
| Positive/mutation self-test | **PASS `1 valid / 35 refusal`** |
| Pending template | **PASS `15/15`**, output `ATTEMPT_POLICY_TEMPLATE_VALID_NOT_READY` |
| Pending template ở completed mode | **REFUSED**, exit `1` |
| Path escape | **REFUSED**, exit `1` |
| Duplicate JSON key | **REFUSED**, exit `1` |
| LF provenance | **PASS** — W-0180 JSON/TXT, W-0151, T-09, functional spec và domain source đều `eol=lf` |
| Template SHA-256 | `4695ff51be56dd208682f80a32b7dedaf748ede09066da2ae6039f51b1e1f4df` |
| Validator SHA-256 | `7c25e10f12ac13012ea18d13a6cdc7027c6dd19ce2a0728366d47bb8df3e1cdb` |
| Artifact manifest | `docs/evidence/W-0180/artifact-sha256.txt` |

Mutation suite từ chối reserved version, thiếu program, sai offsets/window/time, sai counting/retry,
timezone/wire mismatch, registry thiếu four-eyes/no-delete, ATP-11 chưa chọn hoặc fail-open, capacity
chưa recalibrate, rollback/rollout sai, canonical/independent hash sai, thiếu/sai ATP/signoff/evidence,
signer-verifier không tách, safety flag, PII/secret-like value và extra key.

## 5. Trạng thái production còn lại

- `ATP-01..ATP-15`: `NOT_SIGNED`.
- Signed numeric two-program bundle/version/hash: `NOT_RECEIVED`.
- Product + Order Core + M3 sign-off: `NOT_RECEIVED`.
- M3 producer/OpenAPI/schema/CDC/sandbox: `NOT_RECEIVED / NOT_RUN`.
- Registry governance/custody/recovery và ATP-11 strategy: `NOT_APPROVED`.
- Capacity/token recalibration, shared E2E, cutover/canary/rollback: `NOT_RUN`.
- Production release packet: `NOT_RECEIVED`.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Bước tiếp theo

Product, Order Core và M3 điền/ ký bundle từ template, còn Platform/Release giao đúng evidence thuộc
phạm vi của họ. Reviewer lấy bảy hash độc lập và chạy validator. Chỉ sau PASS mới mở change review
riêng cho registry/producer/pre-dial/scheduler; không code theo proposal pending.
