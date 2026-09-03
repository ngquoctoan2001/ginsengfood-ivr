# W-0163 — External decision dispatch execution preflight

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

Trạng thái: **`BLOCKED_EXTERNAL / HASH_PREFLIGHT_PASS / RECIPIENT_IDENTITIES_REQUIRED /
LOCAL_ROUTING_INPUT_TEMPLATE_READY / EXTERNAL_DISPATCH_NOT_PERFORMED /
EXTERNAL_APPROVAL_NOT_RECEIVED / NO_GATE_PROMOTION`**

## 1. Phạm vi

- Mở bước thực thi dispatch `D-01..D-05` từ M8-12/M8-13.
- Verify exact hashes trước khi gửi.
- Kiểm tra đủ actual recipient identity, authority source, channel/ticket và due date.
- Chỉ ghi delivery/receipt khi có hành động gửi thật; không tự điền hoặc gửi tới nhóm chung.

## 2. Hash preflight

| Check | Kết quả |
| --- | --- |
| 18 artifact trong manifest | **PASS `18/18`**, 0 missing, 0 drift |
| M8-12 SHA-256 | `691568b3fa48e613ecab1c52835e40f483073698d4aa1c8b1a41df5d42d34fe0` — khớp M8-13 |
| Manifest SHA-256 | `49ed4c153bb71db1cad6c1af446fe3c3c1892cd40b4d8355441868d60c349406` — khớp M8-13 |
| M8-13 current SHA-256 | `261b33fd4832793240b837e090efe7424929278d454da98a9a454cfdcfacc103` |

Nguồn:

- [M8-12 dispatch pack](../../../plan/ivr-orther/m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md)
- [M8-13 message kit](../../../plan/ivr-orther/m8-13-external-decision-dispatch-message-kit-2026-09-03.md)
- `docs/evidence/W-0152/artifact-sha256.txt`
- [Recipient routing input template](recipient-routing-input.template.md) — SHA-256
  `dcbeead41f56340c2dffac17e5540959d7571971e117fb03dd89d9d261859dc0`
- [W-0164 offline routing validator](../W-0164/README.md) — JSON schema/hash/provenance/PII
  precheck; không phải dispatch hoặc receipt evidence.

## 3. Dispatch readiness

| Batch | Recipient/authority | Channel/ticket | Due date | Hash | Delivery | Response |
| --- | --- | --- | --- | --- | --- | --- |
| `D-01` | `NOT_PROVIDED` | `NOT_PROVIDED` | `NOT_PROVIDED` | `PASS` | `NOT_PERFORMED` | `NOT_RECEIVED` |
| `D-02` | `NOT_PROVIDED` | `NOT_PROVIDED` | `NOT_PROVIDED` | `PASS` | `NOT_PERFORMED` | `NOT_RECEIVED` |
| `D-03` | `NOT_PROVIDED` | `NOT_PROVIDED` | `NOT_PROVIDED` | `PASS` | `NOT_PERFORMED` | `NOT_RECEIVED` |
| `D-04` | `NOT_PROVIDED` | `NOT_PROVIDED` | `NOT_PROVIDED` | `PASS` | `NOT_PERFORMED` | `NOT_RECEIVED` |
| `D-05` | `NOT_PROVIDED` | `NOT_PROVIDED` | `NOT_PROVIDED` | `PASS` | `NOT_PERFORMED` | `NOT_RECEIVED` |

Không có configured connector hoặc destination cụ thể trong phạm vi W-0163. Tự chọn người nhận,
mailbox, ticket project hoặc authority chain sẽ làm sai trust boundary của M8-12.

## 4. Verification record

| Gate | Kết quả |
| --- | --- |
| Artifact hash preflight | **PASS `18/18`** — 0 missing, 0 drift |
| Control hashes | **PASS `2/2`** — M8-12 và manifest khớp M8-13 |
| Dispatch readiness | **BLOCKED `5/5`** — thiếu recipient/authority/channel/due date |
| W-0163 PII scan | **PASS** — 2/2 Markdown, 0 binary skipped |
| API docs | **PASS** — 14 generated artifacts; boundary/link/topology/PII checks PASS |
| Test traceability | **PASS `476`** |
| Gate mirror | **PASS** — 11 gates, 161 work items, 23 open decisions, production=false |
| Official Markdown map | **PASS** — 651 Markdown files; W-0163/template/target worklist 0 unresolved; global 199 unresolved là corpus backlog có sẵn |
| `git diff --check` | **PASS** — chỉ có line-ending warnings của shared worktree |
| GitNexus symbol impact | **N/A** — W-0163 chỉ thêm/cập nhật documentation và evidence |

## 5. Stop rule

- Không đổi `NOT_PERFORMED` thành `SENT/DELIVERED` nếu không có message/ticket ID và timestamp.
- Không đổi `NOT_RECEIVED` thành approval nếu response thiếu identity, authority source, exact hash,
  scope, timestamp, evidence và rollback/rejection path.
- Không mở implementation cho sheet chỉ có phản hồi “OK” hoặc chữ ký ngoài phạm vi.
- Không gửi dữ liệu bí mật/PII trong dispatch pack.
- `TARGET_CONTRACT_V1=DRAFT`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Input để tiếp tục

Module 8 Owner/chief auditor phải cung cấp cho từng batch:

1. actual recipient identity;
2. role và authority source;
3. channel/project + message hoặc ticket destination;
4. requested response date có timezone;
5. quyền thực hiện external dispatch trong kênh đó.

Sau khi có input, verify lại M8-12/manifest hash ngay trước send, gửi từng batch riêng và append
receipt vào M8-13 §7. Nếu artifact drift, dừng gửi và regenerate manifest/message trước.

## 7. Continuation log

| Lần | Yêu cầu nhận được | Hash preflight | External input mới | Kết quả |
| --- | --- | --- | --- | --- |
| `1` | Owner yêu cầu “tiếp” | **PASS** — 18/18 artifact, M8-12 và manifest không drift | Không có recipient/authority/channel/due date hoặc quyền gửi | Giữ `BLOCKED_EXTERNAL`; 0/5 dispatch, không mutation receipt |
| `2` | Owner yêu cầu “tiếp” lần nữa | Không cần đổi control hash; pack giữ nguyên | Chưa có identity/authority/destination | Tạo routing-input template 5 batch, hash-pin; vẫn 0/5 dispatch |
