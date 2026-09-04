# W-0165 — Offline external decision response validator

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

> Pin rotation W-0170 (`2026-09-04`): current response input dùng manifest
> `docs/evidence/W-0170/artifact-sha256.txt`; hash W-0165 gốc bên dưới được supersede bởi bảng current.

Trạng thái: **`TESTS_PASS_LOCAL / RESPONSE_PROVENANCE_VALIDATOR_READY /
EXTERNAL_RESPONSES_NOT_RECEIVED / EXTERNAL_AUTHORITY_UNVERIFIED /
W-0163_BLOCKED_EXTERNAL / NO_GATE_PROMOTION`**

## 1. Kết quả

Đã tạo:

- `deploy/ci/scripts/external-decision-response-validator.mjs` — CLI Node chỉ đọc;
- `docs/evidence/W-0165/decision-response-input.template.json` — template metadata-only, không
  chứa response thật, PII, credential hoặc raw attachment.

CLI kiểm response/signature record trước khi record được xem xét đưa vào approval ledger. Nó không
gửi message, không xác minh danh tính/authority ngoài đời, không ghi ledger và không mở code/release
gate.

## 2. Cách chạy

Kiểm pending template:

```powershell
node deploy/ci/scripts/external-decision-response-validator.mjs `
  --check-template docs/evidence/W-0165/decision-response-input.template.json
```

Kết quả đúng là `RESPONSE_TEMPLATE_VALID_NOT_READY`, exit `0`. Copy template sang file riêng,
thay pending record bằng một hoặc nhiều response metadata record rồi chạy:

```powershell
node deploy/ci/scripts/external-decision-response-validator.mjs --input <decision-response.json>
```

Output PASS cố ý là `RESPONSE_PROVENANCE_VALID_AUTHORITY_UNVERIFIED`; không bao giờ tự xuất
`APPROVED` hoặc `AUTHORITY_VERIFIED`.

## 3. Guard

- exact schema, không field thiếu/thừa, không duplicate JSON key/response ID;
- exact current M8-12 + manifest hash; recompute đủ `18/18` artifact trong manifest trước mỗi lần
  nhận input;
- chỉ nhận sheet external `S-01..S-09,S-11`; loại `S-10` vì W-0141 đã xử lý local;
- exact sheet↔batch: S-05 cho phép D-01 hoặc D-02, các sheet còn lại chỉ batch đã khóa trong M8-12;
- accepted artifact path/hash phải đúng và đủ tập riêng của sheet, luôn gồm exact M8-12;
- decision chỉ `APPROVE`, `APPROVE_WITH_CONDITIONS`, `REJECT`, `NEEDS_REVISION`; decision text
  không được chỉ là “OK” hoặc câu mơ hồ;
- `decision_ids` luôn gồm sheet ID; S-01/S-06/S-07/S-08/S-09 còn bị giới hạn theo exact
  `OD18-C*`/`OPT-*`/`RVK-*`/`DTK-*`/`ATP-*` range;
- bắt buộc signer alias, role/organization, allowlisted authority source, dispatch receipt ref,
  external response ref/hash, evidence ref và timestamps có timezone;
- approval-like decision bắt buộc effective/cutover/compatibility window; rejection/revision phải
  dùng `NOT_APPLICABLE`; cutover không được trước effective;
- `APPROVE` không được giữ blocker; conditional/revision phải có blocker + owner + target;
- S-11 bắt buộc scope `PROCUREMENT`; mọi scope thuộc exact allowlist;
- chặn email, phone-like value, street-address-like value, credential/secret/JWT-like material;
- input là regular non-symlink strict UTF-8 file trong repo, không BOM và tối đa 256 KiB;
- bảy safety flag luôn `false`, gồm external authority verified, approval-ledger update, gate
  promotion và real-customer-call permission.

## 4. Verification

```text
node --check: PASS
W0165_SELFTEST_PASS template=1 valid=2 refusals=27
RESPONSE_TEMPLATE_VALID_NOT_READY responses=0 approval_like=0
pending template with --input: REFUSED, exit=1
manifest/source preflight: PASS 18/18 + 2 control hashes
```

Hai positive synthetic record phủ S-06 conditional approval và S-11 rejection. Hai mươi bảy
refusal phủ pending normal mode, missing/extra/duplicate field or record, source/artifact hash,
non-dispatch sheet, sheet↔batch, missing/extra artifact, decision ID, vague decision, email, phone,
address, secret, timestamps/order, cutover order, scope, blocker coherence, safety, malformed/duplicate
JSON, oversized input và path ngoài root.

| Artifact | SHA-256 |
| --- | --- |
| `deploy/ci/scripts/external-decision-response-validator.mjs` | `f39891979b9fcf37974070567be5000f601f78f4a5fdaeaa08ea71585338fa8c` |
| `decision-response-input.template.json` | `77039df220cca033d02cf51e3c991390f3bc34cdc557e2d754f8c1b64ab74af4` |

## 5. Impact và non-inference

- Direct inventory trước edit: **LOW**, 0 existing caller/import/runtime flow; CLI mới không nối
  scheduler, callback, database hoặc outbound connector. GitNexus không có symbol cho file chưa
  index và repo không được re-index trong lượt này.
- Schema/hash/provenance PASS chỉ chứng minh record tự nhất quán với current local artifact set.
  Signer identity, authority source, external receipt và raw response hash vẫn là metadata cần chief
  auditor kiểm ở trust boundary riêng.
- Decision `APPROVE` trong input không tự đổi sheet, tracker hoặc gate sang approved.
- W-0163 vẫn `BLOCKED_EXTERNAL`, 0/5 dispatch và 0/5 response.
- `TARGET_CONTRACT_V1=DRAFT`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Bước tiếp theo

Sau khi W-0163 có dispatch receipt thật và external owner trả lời, tạo một metadata record từ
template, chạy `--input`, lưu exact output + input hash trong evidence intake. Chief auditor chỉ cập
nhật approval ledger sau khi kiểm độc lập identity, authority source, external response artifact và
phạm vi ký.

## 7. Current-head pin refresh — W-0186

W-0186 phục hồi đủ 18 artifact mà response validator kiểm, xoay T-09/M8-12/manifest và không thay
sheet/artifact/quorum rule. Current validator SHA-256 là
`33b341e1d11c6383cd9f72ede018d510d103d348134c40d98a9f67a5d736e538`; template SHA-256 là
`056ee7b325950da4380d167cd876d40195b44a96ef81df2f97365fdb5cea5be3`.
Self-test current: `W0165_SELFTEST_PASS template=1 valid=2 refusals=27`.
