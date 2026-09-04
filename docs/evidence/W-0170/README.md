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

Template pending nằm tại [decision-closure-input.template.json](decision-closure-input.template.json).
CLI không gửi message, không đọc raw external response, không ghi approval ledger và không cho phép
real-customer call.

## 2. Pin rotation có kiểm soát

Self-test W-0170 phát hiện manifest W-0152 đã drift đúng một artifact: phiếu OD-18 được sửa ở commit
`c213bf7` để trỏ tài liệu OD-15 đã gỡ khỏi cây. Content current có SHA-256
`fed2fe7a68dc41ac6f658fc6479163ac89e007e1ea1b6fa0126522bab54c6b0d`, khác pin lịch sử
`62e4be4e...`.

Không sửa manifest W-0152 lịch sử. W-0170 tạo [manifest hiện hành](artifact-sha256.txt), cập nhật
M8-12/M8-13 bằng một controlled pin rotation và xoay source pin của W-0164/W-0165. Kết quả current:

| Artifact | SHA-256 |
| --- | --- |
| M8-12 | `9da8e5698bc99df73338b3d6886e61f18c93e492431d07cb730074f6ef3aa499` |
| W-0170 manifest — 18 artifact | `3352479690e424b88138654b1a91aa5c55908b19d47ee63870795b113e616471` |
| M8-13 | `95632b90ab99df6892ba6f0a231e9429c5021fd200d982609c52584e1b920ca3` |
| W-0164 routing validator | `e70eb8b90e2a5697219f375baab7e6c0d6cb7d58053310ca8fd47caf07180d45` |
| W-0164 routing template | `5fc84bee77beff876511cc41737123d7362806590e6015018b723ffdfd95abeb` |
| W-0165 response validator | `1d14a46eeceb4a59586e23cd84668be50836831ef71c3a50c53a85386d72e1dc` |
| W-0165 response template | `5e19f8a9342135ecb503050cc208262c8b4081b2a96007d381124c191d6437da` |
| W-0170 closure validator | `8ef4881404fce45d0905cdf74a763938919138ad2ca99520b60aaff2cba3bda0` |
| W-0170 closure template | `611c2481faf4d1b4741b8468a3977203286c6c6875e4bdc9db85f3166a7d2a1d` |

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
