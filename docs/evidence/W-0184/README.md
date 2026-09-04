# W-0184 — B5+C12/S-08 focused closure-path self-test

Ngày: `2026-09-04`

Baseline: detached clean `main@5c0b17085030cd69722a8422fe635bbcfbd9f5de` cộng duy nhất
script W-0184 có SHA-256
`e4ad343b22914db45a515c8e29b00eb3ba8d82f672c6ea04b1030e9280b3e505`.

Trạng thái: **`TESTS_PASS_LOCAL / DIAL_TOKEN_S08_CLOSURE_PATH_PROVEN / SYNTHETIC_ONLY /
EXTERNAL_DECISIONS_NOT_RECEIVED / CODE_NOT_AUTHORIZED`**.

## 1. Khoảng trống được đóng

W-0164/W-0165/W-0170 đã khai báo đúng contact/dial-token qua routing batch `D-02`, decision sheet
`S-08`, bảy authority group, exact artifact set và `DTK-01..DTK-15`. Tuy nhiên positive self-test
W-0170 chỉ dựng closure `S-05`; nhánh dial-token chưa có lượt positive end-to-end riêng để chứng
minh rule tổng hợp có thể đóng đúng `S-08`.

W-0184 thêm
`deploy/ci/scripts/external-decision-dial-token-selftest.mjs`, một harness synthetic chỉ gọi các
validator đã pin. Không sửa W-0164/W-0165/W-0170, không xoay provenance hash, không thay rule và
không đụng contact schema, resolver, vault, gateway hoặc runtime.

W-0183 là work song song khác: validator cho production decision bundle trước khi review. W-0184
không thay thế W-0183; nó kiểm chuỗi routing/response/receipt/quorum sau khi decision artifact thật
đã được ký và dispatch.

## 2. Positive path

Harness dựng trong `ci-artifacts/` rồi tự dọn:

1. một routing input chỉ `D-02` ở trạng thái ready và qua W-0164;
2. bảy response `S-08` qua W-0165, lần lượt đại diện `M3_PRODUCER`, `SECURITY`, `PLATFORM`,
   `TELEPHONY_VENDOR`, `PRODUCT`, `LEGAL_PRIVACY`, `RELEASE`;
3. mỗi response nhận đủ exact M8-10 decision pack, T-04 và M8-12 dispatch pack từ current manifest;
4. một receipt D-02 hash-bound và bảy authority attestation có signer/verifier tách biệt;
5. union decision coverage đủ chính xác `DTK-01..DTK-15`;
6. một closure W-0170 trả
   `DECISION_PROVENANCE_CLOSURE_VALID_NO_GATE_PROMOTION sheets=1 sheet_ids=S-08`.

Mọi signer, receipt, response, authority evidence và hash ngoại vi trong self-test đều synthetic.
Không mục nào là bằng chứng external đã nhận.

## 3. Fail-closed matrix

| Mutation | Kết quả |
| --- | --- |
| thiếu authority attestation | `REFUSED` |
| thiếu `DTK-15` trong closure | `REFUSED` |
| response `S-08` đi sai batch D-01 | `REFUSED` |
| một owner chỉ `APPROVE_WITH_CONDITIONS` | `REFUSED` |
| signer đồng thời là verifier | `REFUSED` |
| authority group ngoài quorum S-08 | `REFUSED` |
| response thiếu T-04 trong accepted artifact set | `REFUSED` |
| receipt khai sai batch D-03 | `REFUSED` |

Kết quả exact detached checkout:

```text
node --check: PASS
W0184_DIAL_TOKEN_SELFTEST_PASS valid=1 refusals=8 authorities=7 decisions=15
W0164_SELFTEST_PASS template=1 valid=2 refusals=19
W0165_SELFTEST_PASS template=1 valid=2 refusals=27
W0170_SELFTEST_PASS valid=1 refusals=21
```

Supporting gates tại verification snapshot:

- API docs self-test: PASS `14` generated artifact;
- CI config self-test: PASS;
- test traceability: CURRENT `485`;
- scoped PII: PASS `3/3`; scanner negative-control self-test `CT-CI-06..06h` PASS;
- gate mirror: PASS `11` gate / `182` work item / `23` open decision / production `false`;
- Markdown map: W-0184 có `0` unresolved link; global unresolved vẫn phản ánh corpus backlog và
  deletion WIP hiện hữu;
- scoped tracked `git diff --check`: PASS; file mới không có trailing whitespace;
- GitNexus aggregate shared dirty tree: LOW, `11 file / 12 symbol / 0 affected process`; standalone
  CLI W-0184 chưa thuộc index.

## 4. Boundary và phương pháp kiểm

- Shared checkout có 29 deletion WIP trong `plan/ivr-orther`, gồm M8-10/M8-12 là source
  hash-bound của validation chain. W-0184 không restore, stage hoặc sửa các deletion đó.
- Self-test chạy trong detached clean worktree tại exact baseline, sau đó thêm duy nhất script
  W-0184 vào checkout tạm. Không lấy trạng thái xanh từ shared checkout bị thiếu source artifact.
- GitNexus query bị thiếu FTS nên direct source là authority. Bốn symbol mới
  `runDialTokenS08ClosureSelfTest`, `buildDialTokenRoutingFixture`,
  `buildDialTokenResponseFixture`, `buildDialTokenClosureFixture` đều not-found với `0 impacted`
  trước edit; standalone CLI không có runtime caller.
- Harness không có input external, connector, network call, database write, ledger write,
  production adapter hoặc contact/raw phone.

## 5. Non-inference và phần còn lại

W-0184 chỉ chứng minh validator chain có positive path và fail-closed guard riêng cho `S-08`. Nó
không chứng minh dispatch D-02, receipt, signer identity/authority, production contact producer,
issuer/resolver/vault, TTL/reissue/replay semantics, custody, external trust boundary, telephony
vendor compatibility hoặc shared E2E.

B5+C12 vẫn `CODE_NOT_AUTHORIZED`: M3/Security/Platform/Telephony/Product/Legal/Release phải ký đủ
`DTK-01..DTK-15`, cung cấp contract và evidence thật, rồi closure qua W-0164 → W-0165 → W-0170.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Bước tiếp theo:** hoàn tất W-0183 decision-bundle validator; khi có bundle thật và routing D-02,
chạy W-0164 → dispatch/receipt → W-0165 → independent authority attestation → W-0170 cho `S-08`.
Chỉ sau closure hợp lệ mới mở Work ID riêng để impact-analyze production implementation.
