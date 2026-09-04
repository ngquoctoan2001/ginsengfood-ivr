# W-0186 — C5 current-head provenance chain restoration

Ngày: `2026-09-04`

Baseline: `main@8ed62e93f5ec0ff7a4c694181ac73ee04f1eb34b` + shared W-0185 WIP được giữ nguyên.

Trạng thái: **`TESTS_PASS_LOCAL / CURRENT_PROVENANCE_CHAIN_VALID /
EXTERNAL_ROUTING_NOT_RECEIVED / 0_OF_5_DISPATCHED / NO_GATE_PROMOTION`**

## 1. Root cause

Commit `8ed62e9` xóa các source pack mà W-0164/W-0165/W-0170 pin. Kết quả trên current checkout:

- W-0164 fail vì thiếu M8-12;
- W-0165 fail vì thiếu OD-18 và tám M8-05..M8-11 artifact;
- W-0170, W-0179 và W-0184 fail theo prerequisite;
- sau khi restore exact source, `17/18` manifest member khớp; chỉ T-09 đổi vì W-0180 thêm section
  hardening fail-closed.

Đây là provenance regression, không phải business-rule hoặc validator-algorithm defect.

## 2. Khắc phục

1. Restore đúng byte từ `e7184e7` cho OD-18, M8-05..M8-11, M8-12, M8-13 và TODAY-01.
2. Xác minh tám decision artifact cùng OD-18 khớp exact manifest hash cũ.
3. Review diff T-09: chỉ append W-0180 fail-closed; không đổi policy/quorum/decision.
4. Xoay dependency theo thứ tự T-09 → M8-12 → manifest → M8-13 → W-0164/W-0165 → W-0170.
5. Không thay schema, validation rule, decision IDs, quorum, stop rule, delivery guard hoặc runtime.

## 3. Current hash chain

| Artifact | SHA-256 |
| --- | --- |
| T-09 | `4046e3c1cbeb8d3983da0745f25056968d0960b04b410904369bdb20e987eb11` |
| M8-12 | `59631b137f422840010a3d52e274196a19cbf644ad0bd3661d03bda48e5bc45e` |
| Manifest 18 member | `f4c04e4a3104ce02923230932ffd3e3140ae092b536b836db278da8312779288` |
| M8-13 | `a6e43aa1493d07bf03fba1de699e11300326ae87d5dcbbf47311602d1575cd39` |
| W-0164 validator | `de192cb4f14435247a149e2d0cd27c4e0b054a5746ff3e228e72670f6a37be91` |
| W-0164 template | `590b4682905c62162f7d612558d6036f5f1497fd152bfaad104d50f977aabef9` |
| W-0165 validator | `33b341e1d11c6383cd9f72ede018d510d103d348134c40d98a9f67a5d736e538` |
| W-0165 template | `056ee7b325950da4380d167cd876d40195b44a96ef81df2f97365fdb5cea5be3` |
| W-0170 validator | `1c5d2539010ca2a38d3dd60954a993d57be6f4dd8ef1d81990d21e8fe880d06e` |
| W-0170 template | `a5907f34c7a24feff635fa3e1633fc4664111550e11e804e2b93d39218195a98` |

## 4. Verification

| Gate | Kết quả |
| --- | --- |
| GitNexus impact, ba `verifySourcePins` | `LOW`; mỗi symbol 1 caller trực tiếp, 5 upstream symbol, 0 process |
| Restored manifest members | `9/9` exact historical SHA trước controlled T-09 rotation |
| Current manifest | `18/18 PASS`, drift `0` |
| W-0164 self-test | `PASS template=1 valid=2 refusals=19` |
| W-0165 self-test | `PASS template=1 valid=2 refusals=27` |
| W-0170 self-test | `PASS valid=1 refusals=21` |
| C9/W-0179 dependent self-test | `PASS valid=1 refusals=6 authorities=5 decisions=11` |
| B5+C12/W-0184 dependent self-test | `PASS valid=1 refusals=8 authorities=7 decisions=15` |
| Pending-template guard | W-0164/W-0165/W-0170 đều valid-not-ready ở check mode và bị từ chối ở input mode |
| PII | Current C5 docs/templates `14/14 PASS`; scanner negative-control `CT-CI-06..06h PASS` |
| Docs / CI / traceability | API docs `14` artifact PASS; CI config PASS; traceability `485` |
| Gate mirror | `11` gate / `185` work item / `23` open decision; production flag vẫn `false` |
| Markdown map | `595` Markdown file; `650` link resolved; C5 critical set `0` unresolved |
| Worklist | Đúng `1` bảng, `16` row, thứ tự `1..16`; C5 là `LOCAL_PROVENANCE_CHAIN_CLEAN` |
| Diff / impact | `git diff --check` PASS; refreshed GitNexus aggregate LOW `26 file / 41 symbol / 0 process` |

Scan rộng trên các source pack lịch sử có ba false positive do một từ chỉ lộ trình được dùng theo
nghĩa luồng xử lý hoặc rollback, không phải địa chỉ. Các file đó được giữ đúng byte/hash; không nới PII rule và không sửa
artifact lịch sử để ép gate xanh. Ba validator cũng chứa chính regex/negative fixture PII, nên kết quả
PII deliverable ở trên chỉ tính docs/template; hành vi từ chối của scanner được chứng minh riêng bằng
`CT-CI-06..06h`.

GitNexus index ban đầu ở `c213bf7` trong khi HEAD là `8ed62e9`; sau refresh theo repository rule,
ba `verifySourcePins` vẫn LOW (`5` impacted / `1` direct / `0` process mỗi symbol). Direct source,
exact SHA và executable self-test là evidence chính.

## 5. Boundary

- Không có recipient routing thật, approved destination, dispatch receipt, response hoặc independent
  authority attestation nào được tạo.
- Không gửi email/ticket/message, không gọi network/runtime, không ghi approval ledger.
- Local PASS không đổi W-0163 khỏi `BLOCKED_EXTERNAL`, không mở delivery guard và không authorize
  implementation/release/production.
- `TARGET_CONTRACT_V1=DRAFT`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Bước tiếp theo

Module 8 Owner/chief auditor cung cấp routing thật cho ít nhất D-01: recipient alias, role/org,
authority source, approved channel/destination, due time, dispatch authorizer và receipt
system-of-record. Chạy W-0164, recheck exact hash, dispatch trong kênh đã được cấp quyền, rồi mới
nhận W-0165 response và W-0170 authority closure.
