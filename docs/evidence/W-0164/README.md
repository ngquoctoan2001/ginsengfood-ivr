# W-0164 — Offline external-decision routing input validator

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

> Pin rotation W-0170 (`2026-09-04`): current routing input dùng manifest
> `docs/evidence/W-0170/artifact-sha256.txt`; hash W-0164 gốc bên dưới được supersede bởi bảng current.

Trạng thái: **`TESTS_PASS_LOCAL / OFFLINE_ROUTING_VALIDATOR_READY /
EXTERNAL_ROUTING_INPUT_NOT_RECEIVED / W-0163_BLOCKED_EXTERNAL / NO_GATE_PROMOTION`**

## 1. Kết quả

Đã tạo:

- `deploy/ci/scripts/external-decision-routing-validator.mjs` — CLI Node chỉ đọc;
- `docs/evidence/W-0164/recipient-routing-input.template.json` — template máy đọc được,
  5/5 batch giữ `PENDING_OWNER_INPUT`.

CLI không có connector, network call, database write, receipt writer hoặc production adapter. Nó chỉ
kiểm input trước khi dữ liệu routing được đưa vào W-0163.

## 2. Cách chạy

Kiểm canonical pending template:

```powershell
node deploy/ci/scripts/external-decision-routing-validator.mjs `
  --check-template docs/evidence/W-0164/recipient-routing-input.template.json
```

Kết quả đúng là `TEMPLATE_VALID_NOT_READY`, exit `0`. Đây không phải dispatch readiness.

Sau khi copy template sang một file input riêng và điền đủ ít nhất một batch:

```powershell
node deploy/ci/scripts/external-decision-routing-validator.mjs --input <routing-input.json>
```

CLI chấp nhận một phần theo batch: 1–4 row ready thì root status phải là `PARTIAL_READY`; đủ 5/5
thì phải là `READY_FOR_HASH_RECHECK_AND_DISPATCH`. Row còn chờ phải giữ nguyên toàn bộ placeholder
và `NOT_READY`; row điền dở luôn bị từ chối.

## 3. Guard

- exact root/source/batch/safety schema, không chấp nhận field thiếu/thừa hoặc duplicate JSON key;
- đúng thứ tự và đủ duy nhất `D-01..D-05`;
- exact current SHA-256 của M8-12, W-0152 manifest và M8-13;
- input phải là regular non-symlink file, strict UTF-8, không BOM, trong repository root và tối đa
  64 KiB;
- channel thuộc allowlist; `due_at` và `dispatch_authorized_at` là ISO-8601 có timezone và due date
  phải sau thời điểm cấp quyền;
- chặn row điền dở, root status không khớp ready count và normal mode có 0 ready batch;
- chặn email cá nhân, phone-like value, street-address-like value, credential/secret/JWT-like value
  trong routing metadata;
- bốn safety flag bắt buộc `false`: không personal contact detail, không credential/secret, chưa
  dispatch và chưa ghi receipt.

## 4. Verification

```text
node --check: PASS
W0164_SELFTEST_PASS template=1 valid=2 refusals=19
TEMPLATE_VALID_NOT_READY ready=0 pending=5
pending template with --input: REFUSED, exit=1
```

Self-test positive gồm một partial-ready synthetic fixture và một full-ready synthetic fixture.
Mười chín refusal phủ pending normal mode, missing/duplicate/out-of-order batch, extra field, hash
drift, email, phone, address, secret, timestamp, reversed due date, channel, mixed row, root status,
safety flag, malformed JSON, duplicate JSON key, oversized input và path ngoài root.

| Artifact | SHA-256 |
| --- | --- |
| `deploy/ci/scripts/external-decision-routing-validator.mjs` | `d6c6dc92bb02479295c5271da9e7e2370a507cd76c4e7f7c4649ce50d11ee6f4` |
| `recipient-routing-input.template.json` | `1ed18e3344a8d60b61e7d7a1bd88818dfb1d6f09014e3ccbce85d2bb68df0454` |

## 5. Impact và giới hạn

- Direct inventory trước edit: **LOW**, 0 existing caller/import/runtime flow; file mới là CLI
  standalone. GitNexus không có symbol cho file chưa được index và repo không được re-index ở lượt
  này.
- Validator xác minh shape, hash, provenance reference và dữ liệu nhạy cảm theo pattern; nó không
  xác minh ngoài đời rằng recipient hoặc người cấp quyền thật sự có authority.
- `ROUTING_INPUT_VALID` chỉ cho phép W-0163 chạy lại hash preflight và xem xét dispatch từng batch;
  không đồng nghĩa `SENT`, `DELIVERED`, `APPROVED` hoặc production-ready.
- W-0163 vẫn `BLOCKED_EXTERNAL`, 0/5 dispatch và 0/5 response.
- `TARGET_CONTRACT_V1=DRAFT`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Bước tiếp theo

Module 8 Owner/chief auditor copy JSON template, điền ít nhất một batch bằng identity alias,
authority reference, approved channel/destination và hai timestamp có timezone; chạy `--input`.
Chỉ sau PASS mới tiếp tục W-0163 để recheck exact hash và thực hiện dispatch thật trong kênh đã được
cấp quyền.

## 7. Current-head pin refresh — W-0186

Commit `8ed62e9` đã xóa source set mà validator pin; W-0186 phục hồi đúng bytes từ `e7184e7`, chấp
nhận riêng phần T-09 W-0180 additive/fail-closed và xoay dependency chain mà không đổi rule. Current
validator SHA-256 là `de192cb4f14435247a149e2d0cd27c4e0b054a5746ff3e228e72670f6a37be91`;
template SHA-256 là `590b4682905c62162f7d612558d6036f5f1497fd152bfaad104d50f977aabef9`.
Self-test current: `W0164_SELFTEST_PASS template=1 valid=2 refusals=19`.
