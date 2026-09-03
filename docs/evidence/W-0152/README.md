# W-0152 — External decision provenance and dispatch-ready consolidation

Ngày: `2026-09-03`

Baseline code được audit: `main@b21ec676e490`

Trạng thái: **`EVIDENCE_SUBMITTED / LOCAL_HANDOFF_PACKAGE_READY /
EXTERNAL_DISPATCH_NOT_PERFORMED / EXTERNAL_APPROVAL_NOT_RECEIVED / NO_GATE_PROMOTION`**

Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Phạm vi

W-0152 hoàn thiện phần local còn tự làm được của `C5`:

- Đối chiếu TODAY-01 `S-01..S-10` với W-0128, W-0135, W-0141 và các pack W-0145..W-0151.
- Sửa trạng thái S-10 theo controlled execution W-0141.
- Bổ sung S-11 cho VoLTE/procurement mà không tự sửa source spec hoặc phê duyệt model/SKU.
- Tạo một matrix duy nhất có owner, exact artifact/hash, required response, dispatch/approval state,
  rollback/stop rule và signature provenance fields.
- Chia route thành batch `D-01..D-05`; không thực hiện gửi hoặc ký thay owner.

Không có thay đổi source, OpenAPI, DB, migration, seed, config, scheduler, adapter, vault, secret,
egress hoặc runtime trong W-0152.

## 2. Artifact

- [M8-12 — provenance/dispatch pack](../../../plan/ivr-orther/m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md).
- SHA-256 manifest: `docs/evidence/W-0152/artifact-sha256.txt`.
- [TODAY-01 current routing pack](../../../plan/ivr-orther/today-01-decision-signoff-pack-2026-08-29.md).
- [Target worklist/status](../../../plan/toan-viec-can-lam-m8-2026-09-03.md).

M8-12 SHA-256 tại thời điểm nộp evidence:
`691568b3fa48e613ecab1c52835e40f483073698d4aa1c8b1a41df5d42d34fe0`.

## 3. Audit findings đã đóng phía local

| Finding | Trước W-0152 | Sau W-0152 |
| --- | --- | --- |
| S-10 stale state | TODAY-01 còn ghi controlled execution đang chờ dù §8.1 đã chứng minh W-0141 xong | Bảng, sheet, routing và owner-position đều ghi `OPTION_A_EXECUTED_W0141 / BYTES_PRESERVED` |
| VoLTE/procurement route | Pack nói 10 sheet và không có sheet errata 21 | S-11 ghi fact, owner, acceptance artifact, stop rule và batch D-05 |
| OD-18 provenance | Target list nói chưa có văn bản làm rõ | Phiếu OD-18 hiện hữu được link và ghim exact hash; M3/Privacy vẫn chưa ký |
| Cross-pack provenance | Owner/artifact/response nằm rải rác | M8-12 map S-01..S-11, 18 artifact hashes và signature-intake template |
| Dispatch truth | Chưa có gửi thật | Ledger ghi rõ `NOT_PERFORMED`; không suy handoff-ready thành delivered/approved |

## 4. Verification

| Gate | Kết quả |
| --- | --- |
| Verify 18 SHA-256 entries against current bytes | `W0152_SHA256_MANIFEST_PASS entries=18` |
| Markdown mapper / W-0152 unresolved links | `PASS` — 635 Markdown files; M8-12, W-0152 evidence và target worklist đều `0` unresolved |
| `node deploy/ci/scripts/docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` — 14 generated artifacts |
| `node deploy/ci/scripts/generate-test-traceability.mjs --check` | `TEST_TRACEABILITY_CURRENT=476` |
| `node deploy/ci/scripts/gate-status.mjs --write` | `GATE_STATUS_WRITTEN` — 11 gate, 150 work item, 23 decision; production không được mở |
| Scoped `git diff --check` | `PASS` — chỉ có EOL conversion warnings của shared worktree, không có whitespace error |

## 5. Trạng thái không được suy diễn

- `EVIDENCE_SUBMITTED` chỉ nói gói local đã sẵn; không phải `ACCEPTED`.
- Không có external dispatch, message/ticket receipt hoặc external signature nào được tạo ở W-0152.
- Chữ ký Module 8 ngày 29/08 chỉ bao phủ vị trí S-01..S-10 đã ghi khi đó; S-11 không được hồi tố.
- W-0135 sửa factual timeline, không duyệt exact gateway model/SKU, channel count hoặc purchase.
- Không decision pack nào tự mở `G-CONTRACT`, `G-POLICY`, Security, Platform, Telephony, Legal,
  shared E2E, Release hoặc production gate.

## 6. Handoff

> **Bước kế tiếp:** Module 8 Owner/chief auditor route M8-12 batch `D-01..D-05`, lưu
> channel/message/ticket ID, người nhận, timestamp và exact manifest hash. Chỉ nhận phản hồi có đủ
> signer identity, authority source, artifact hash, scope, cutover/rollback và evidence. Sau đó mới
> cấp work item implementation riêng cho sheet đã đủ chữ ký; `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ
> nguyên.
