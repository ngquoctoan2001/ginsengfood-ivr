# W-0170 — External decision receipt hash and sheet-quorum validator

Ngày: `2026-09-04`

Baseline: `main@0188fdfbfc6412e5fb2363dca8c7b7bf2439d20f`

Trạng thái: **`TESTS_PASS_LOCAL / RECEIPT_HASH_BINDING_READY /
SHEET_QUORUM_VALIDATOR_READY / EXTERNAL_EVIDENCE_NOT_RECEIVED /
0_OF_5_DISPATCHED / NO_GATE_PROMOTION`**

## 1. Kết quả

Đã thêm `deploy/ci/scripts/external-decision-closure-validator.mjs`, một CLI offline chỉ đọc để
đóng gap sau W-0164/W-0165:

- bind từng response vào receipt thật qua `dispatch_receipt_ref` và SHA-256 của receipt export;
- bind receipt vào exact W-0164 routing input, batch, channel, destination và thời điểm cấp quyền;
- yêu cầu chief auditor attestation tách biệt signer, có authority evidence ref/hash;
- tổng hợp quorum owner theo `S-01..S-09,S-11`, gồm cross-batch `S-05` phải có M3 từ D-01 và
  Security + Platform từ D-02;
- yêu cầu đủ toàn bộ `OD18-C1..C5`, `OPT-01..11`, `RVK-01..12`, `DTK-01..15` hoặc
  `ATP-01..15` trước khi đóng sheet tương ứng;
- chỉ nhận unconditional `APPROVE`, không residual blocker và xuất trạng thái
  `DECISION_PROVENANCE_CLOSED`; trạng thái này không phải implementation/release/production approval.

Template pending nằm tại `docs/evidence/W-0170/decision-closure-input.template.json`.
CLI không gửi message, không đọc raw external response, không ghi approval ledger và không cho phép
real-customer call.

## 2. Pin rotation có kiểm soát

Self-test W-0170 phát hiện manifest W-0152 đã drift đúng một artifact: phiếu OD-18 được sửa ở commit
`c213bf7` để trỏ tài liệu OD-15 đã gỡ khỏi cây. Content current có SHA-256
`fed2fe7a68dc41ac6f658fc6479163ac89e007e1ea1b6fa0126522bab54c6b0d`, khác pin lịch sử
`62e4be4e...`.

Không sửa manifest W-0152 lịch sử. W-0170 tạo manifest hiện hành
`docs/evidence/W-0170/artifact-sha256.txt`, cập nhật
M8-12/M8-13 bằng một controlled pin rotation và xoay source pin của W-0164/W-0165. Bảng sau là
snapshot W-0170 lịch sử; current W-0186 nằm tại §7:

| Artifact | SHA-256 |
| --- | --- |
| M8-12 | `311e2feda84bac04a51d2177c095ec15757763293753b0537320fbe0d25c119d` |
| W-0170 manifest — 18 artifact | `945663bb3d3b12a18c506f8bad47e1cddda44b1f5cb47ce36ba43578c26a5e92` |
| M8-13 | `dd1b972a4a402cd7e6929a7d06ceeba5b286e917125d8186a509d66cc3045a34` |
| W-0164 routing validator | `d6c6dc92bb02479295c5271da9e7e2370a507cd76c4e7f7c4649ce50d11ee6f4` |
| W-0164 routing template | `1ed18e3344a8d60b61e7d7a1bd88818dfb1d6f09014e3ccbce85d2bb68df0454` |
| W-0165 response validator | `f39891979b9fcf37974070567be5000f601f78f4a5fdaeaa08ea71585338fa8c` |
| W-0165 response template | `77039df220cca033d02cf51e3c991390f3bc34cdc557e2d754f8c1b64ab74af4` |
| W-0170 closure validator | `a06e3c2aff52c89ef17e507ade3e3fafeb98fa6b6d31801dcddc2a41c1b4bce9` |
| W-0170 closure template | `2b0dcf77731e2ed782ceb50cc4d8de61e1aa0b6597dbacada7abca95e32f9820` |

W-0176 thực hiện rotation thứ hai sau khi W-0173/W-0174 làm M8-07 đổi byte. Preflight xác nhận
chỉ M8-07 drift; rotation đi theo dependency M8-12 → manifest → M8-13 → W-0164/W-0165 → W-0170
và phục hồi toàn bộ self-test. Evidence chi tiết: [W-0176](../W-0176/README.md).

M8-12/M8-13 và current manifest phải tiếp tục bất biến trong một dispatch cycle. Receipt không được
ghi trực tiếp vào ba artifact hash-bound này; dùng ticket/audit system làm system-of-record và chỉ
đưa reference/hash vào closure input.

## 3. Cách dùng

Kiểm pending template:

```powershell
node deploy/ci/scripts/external-decision-closure-validator.mjs `
  --check-template docs/evidence/W-0170/decision-closure-input.template.json
```

Sau khi có routing input W-0164, response bundle W-0165, receipt export hash và authority
attestations, copy template vào `ci-artifacts/W-0170/`, điền metadata rồi chạy:

```powershell
node deploy/ci/scripts/external-decision-closure-validator.mjs `
  --input ci-artifacts/W-0170/decision-closure.json
```

Input và mọi referenced JSON phải là regular non-symlink strict UTF-8 file trong repository.
`ci-artifacts/` đã được ignore; raw response/receipt có PII hoặc credential phải ở trust store ngoài
repo, closure input chỉ giữ alias, external ref và SHA-256.

Output PASS là:

```text
DECISION_PROVENANCE_CLOSURE_VALID_NO_GATE_PROMOTION sheets=<n> sheet_ids=<S-xx> sha256=<hash>
```

## 4. Quorum tối thiểu

| Sheet | Authority group bắt buộc |
| --- | --- |
| `S-01` | M3 Contract, Legal/Privacy |
| `S-02` | M3 Contract, Product, Order Core |
| `S-03` | M3 Operator, Security, Platform |
| `S-04` | M3 Producer |
| `S-05` | M3 Contract qua D-01; Security + Platform qua D-02 |
| `S-06` | Project Owner, CRM/M3.1, M3 Contract, Legal/Privacy, Product |
| `S-07` | Project Owner, M3 Contract, Order Core, Product |
| `S-08` | M3 Producer, Security, Platform, Telephony/vendor, Product, Legal/Privacy, Release |
| `S-09` | Product, Order Core, M3 Producer, Platform, M8 Owner, Release |
| `S-11` | M8 Owner, Product, Infra/Procurement, Telephony/vendor |

Một response chỉ được một authority attestation. Signer không được tự làm verifier. Các technical
owner phát sinh theo strategy vẫn có thể giữ sheet mở bằng cách không cấp unconditional APPROVE;
validator không suy authority từ chức danh tự khai.

## 5. Verification

```text
node --check: PASS
W0164_SELFTEST_PASS template=1 valid=2 refusals=19
W0165_SELFTEST_PASS template=1 valid=2 refusals=27
W0170_SELFTEST_PASS valid=1 refusals=21
CLOSURE_TEMPLATE_VALID_NOT_READY: PASS
current manifest: PASS 18/18
pending templates with --input: REFUSED, exit=1 for W-0164/W-0165/W-0170
PII deliverables: PASS 11/11; PII scanner self-test: PASS
docs self-test: PASS 14; traceability: CURRENT 476
gate mirror: PASS 11 gates / 168 work items / 23 open decisions / production=false
git diff --check: PASS; GitNexus tracked-scope risk LOW / 0 affected process
```

Current rerun sau W-0176 (rotation thứ hai, không thay đổi acceptance rule):

```text
current manifest: PASS 18/18, drift=0
W0164_SELFTEST_PASS template=1 valid=2 refusals=19
W0165_SELFTEST_PASS template=1 valid=2 refusals=27
W0170_SELFTEST_PASS valid=1 refusals=21
pending templates: VALID_NOT_READY; --input REFUSED 3/3
PII deliverables: PASS 10/10; PII scanner self-test: PASS
traceability: CURRENT 485
```

Positive W-0170 self-test đóng S-05 chỉ khi đủ ba signer group và hai receipt D-01/D-02. Refusal
matrix phủ receipt/hash/routing/channel/destination/recipient/time/delivery, authority source/hash/
separation-of-duties, PII-like alias, missing/wrong quorum, receipt set, decision IDs, state và safety flags.

## 6. Giới hạn và bước tiếp theo

- External dispatch/response/authority evidence thật vẫn là `NOT_RECEIVED`; self-test chỉ dùng alias
  và hash tổng hợp.
- SHA-256 external receipt/authority evidence phải được chief auditor đối chiếu với artifact ở
  ticket/trust store riêng; CLI không có connector và không tự chứng thực authority ngoài đời.
- `DECISION_PROVENANCE_CLOSED` chỉ đóng completeness/provenance của sheet, không mở runtime,
  egress, release hoặc production.
- `TARGET_CONTRACT_V1=DRAFT`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Bước tiếp theo:** Module 8 Owner/chief auditor chọn ticket/audit system-of-record và cung cấp một
routing row thật cho D-01. Chạy W-0164 → dispatch/receipt → W-0165 → authority attestation → W-0170;
sau pilot mới route D-02..D-05 song song.

## 7. Current-head restore và pin rotation — W-0186

W-0186 phục hồi đúng byte 9 manifest member bị `8ed62e9` xóa cùng M8-13/TODAY-01. `17/18` member
khớp manifest cũ ngay sau restore; member duy nhất khác là T-09 do W-0180 append section
hardening fail-closed. Rotation giữ nguyên schema, sheet, decision ID, quorum và stop rule:

| Artifact hiện hành | SHA-256 |
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

Current self-tests PASS W-0164 `2/19`, W-0165 `2/27`, W-0170 `1/21`; dependent W-0179 `1/6`
và W-0184 `1/8` cũng PASS. Đây chỉ là local provenance-chain recovery; dispatch vẫn `0/5`, external
response/authority vẫn `NOT_RECEIVED` và không có gate production nào được mở.
