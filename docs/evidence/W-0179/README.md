# W-0179 — C9/S-06 focused closure-path self-test

Ngày: `2026-09-04`

Baseline: detached clean `main@5c0b17085030cd69722a8422fe635bbcfbd9f5de` cộng duy nhất
script W-0179 có SHA-256
`b3e8fcd1198ac337e4cad82b17fd1b20adbd13fa28f11320a678947673cac4c7`.

Trạng thái: **`TESTS_PASS_LOCAL / C9_S06_CLOSURE_PATH_PROVEN / SYNTHETIC_ONLY /
EXTERNAL_DECISIONS_NOT_RECEIVED / RUNTIME_NOT_AUTHORIZED`**.

## 1. Khoảng trống được đóng

W-0164/W-0165/W-0170 đã khai báo đúng C9 qua routing batch `D-03`, decision sheet `S-06`, năm
authority group và `OPT-01..OPT-11`. Tuy nhiên positive self-test W-0170 chỉ dựng closure `S-05`;
nhánh C9 chưa có một lượt positive end-to-end riêng để chứng minh rule tổng hợp có thể đóng đúng
`S-06`.

W-0179 thêm
`deploy/ci/scripts/external-decision-c9-selftest.mjs`, một harness synthetic chỉ gọi các validator
đã pin. Không sửa W-0164/W-0165/W-0170, không xoay provenance hash và không thay validation rule.

## 2. Positive path

Harness dựng trong `ci-artifacts/` rồi tự dọn:

1. một routing input chỉ `D-03` ở trạng thái ready và qua W-0164;
2. năm response `S-06` qua W-0165, lần lượt đại diện `PROJECT_OWNER`, `CRM_M31`, `M3_CONTRACT`,
   `LEGAL_PRIVACY`, `PRODUCT`;
3. một receipt D-03 hash-bound và năm authority attestation có signer/verifier tách biệt;
4. union decision coverage đủ chính xác `OPT-01..OPT-11`;
5. một closure W-0170 trả
   `DECISION_PROVENANCE_CLOSURE_VALID_NO_GATE_PROMOTION sheets=1 sheet_ids=S-06`.

Mọi signer, receipt, response và hash ngoại vi trong self-test đều synthetic. Không mục nào là bằng
chứng external đã nhận.

## 3. Fail-closed matrix

| Mutation | Kết quả |
| --- | --- |
| thiếu authority attestation | `REFUSED` |
| thiếu `OPT-11` trong closure | `REFUSED` |
| response `S-06` đi sai batch D-01 | `REFUSED` |
| một owner chỉ `APPROVE_WITH_CONDITIONS` | `REFUSED` |
| signer đồng thời là verifier | `REFUSED` |
| authority group ngoài quorum S-06 | `REFUSED` |

Kết quả tổng:

```text
node --check: PASS
W0179_C9_SELFTEST_PASS valid=1 refusals=6 authorities=5 decisions=11
W0164_SELFTEST_PASS template=1 valid=2 refusals=19
W0165_SELFTEST_PASS template=1 valid=2 refusals=27
W0170_SELFTEST_PASS valid=1 refusals=21
W-0164/W-0165/W-0170 pending templates: VALID_NOT_READY
```

Supporting gates tại verification snapshot:

- API docs self-test: PASS `14` generated artifact;
- CI config self-test: PASS;
- test traceability: CURRENT `485`;
- scoped PII: PASS `2/2`; scanner negative-control self-test `CT-CI-06..06h` PASS;
- gate mirror: PASS `11` gate / `179` work item / `23` open decision / production `false`;
- Markdown map: `578` file; W-0179 có `0` unresolved link. Global unresolved phản ánh corpus backlog
  và 29 deletion WIP hiện hữu, không được W-0179 che giấu;
- scoped `git diff --check`: PASS; GitNexus aggregate hiện hữu LOW, `0` affected process.

## 4. Boundary và phương pháp kiểm

- Shared checkout đang có 29 deletion WIP trong `plan/ivr-orther`, gồm các source hash-bound của
  W-0164/W-0165/W-0170. W-0179 không khôi phục, stage hoặc sửa các deletion đó.
- Vì vậy self-test được chạy trong detached clean worktree tại exact baseline, sau đó copy duy nhất
  script W-0179 vào checkout tạm. Checkout tạm được dọn sau khi PASS.
- GitNexus impact cho `runSelfTest` hiện hữu là LOW: 1 caller trực tiếp, 2 symbol, 0 process. Phương
  án cuối không sửa symbol này; bốn symbol mới của harness đều chưa có trong graph và có 0 impacted
  symbol trước edit.
- Harness không có input external, connector, network call, database write, ledger write hoặc
  production adapter.

## 5. Non-inference và phần còn lại

W-0179 chỉ chứng minh validator chain có positive path và fail-closed guard riêng cho C9. Nó không
chứng minh dispatch D-03, receipt, signer identity/authority, external approval, CRM sandbox hay
shared E2E.

C9 vẫn cần đủ quyết định/chữ ký thật cho `OPT-01..OPT-11`, authoritative proposal/ACK/reversal
contract, retention/legal basis, M3 relay/read-back và shared E2E trước khi impact-analyze hoặc code
orchestrator/schema/sender. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Bước tiếp theo:** khi có routing và authority thật, chạy W-0164 → dispatch/receipt → W-0165 →
independent authority attestation → W-0170 cho `S-06`. Chỉ sau closure hợp lệ mới mở một Work ID
riêng để review implementation; không tự chuyển closure thành runtime authorization.
