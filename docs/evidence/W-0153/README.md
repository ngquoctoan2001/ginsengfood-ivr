# W-0153 — External decision dispatch-message kit evidence

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490` + shared documentation WIP

Trạng thái: **`EVIDENCE_SUBMITTED / LOCAL_MESSAGE_KIT_READY /
RECIPIENT_IDENTITIES_REQUIRED / EXTERNAL_DISPATCH_NOT_PERFORMED /
EXTERNAL_APPROVAL_NOT_RECEIVED / NO_GATE_PROMOTION`**

Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Mục tiêu và kết quả

W-0153 chuyển batch `D-01..D-05` của M8-12 thành năm message copy/paste có:

- recipient role và placeholder danh tính/channel/due date;
- exact M8-12 + manifest hash;
- sheet/decision IDs và artifact cần đính kèm;
- response fields, stop rule và trạng thái chưa gửi;
- receipt-capture table không cho phép suy message ID thành approval.

Artifact: [M8-13 dispatch-message kit](../../../plan/ivr-orther/m8-13-external-decision-dispatch-message-kit-2026-09-03.md).

M8-13 SHA-256 tại thời điểm nộp evidence:
`261b33fd4832793240b837e090efe7424929278d454da98a9a454cfdcfacc103`.

W-0153 không gửi ra ngoài, không điền giả người nhận, không tạo ticket/message ID và không ký thay
owner. Không sửa M8-12 hoặc 18 source artifacts đã ghim tại W-0152.

## 2. B1 không bị làm lại

Trước khi cấp W-0153, B1 được rà lại để tìm phần local tiếp theo:

| Artifact | Phần đã có |
| --- | --- |
| W-0131 / PT-CAP-02 | 32 kênh, 800 job/5 phút, 768 incident, không mất job/đốt attempt |
| W-0132 | Một nơi khai báo 40/50/60 và drift guard; cố ý `UNCALIBRATED` |
| W-0133 / OD-19 | Owner decision request cho volume unit/session length/arrival profile |
| W-0142 | Data-intake schema, calibrated-path guard và stop rule |

Phần còn lại cần W-0008 timing thật, M3 arrival buckets/session, Product signed attempt policy và
Infra reserve/failure factor. Vì vậy không tạo thêm B1 pack hoặc đổi runtime assumption.

## 3. Verification

| Gate | Kết quả |
| --- | --- |
| M8-12 SHA-256 trước/sau W-0153 | `PASS` — giữ `691568b3fa48e613ecab1c52835e40f483073698d4aa1c8b1a41df5d42d34fe0` |
| W-0152 manifest SHA-256 trước/sau W-0153 | `PASS` — giữ `49ed4c153bb71db1cad6c1af446fe3c3c1892cd40b4d8355441868d60c349406` |
| 5 message + 5 receipt rows + placeholder guard | `W0153_MESSAGE_KIT_PASS messages=5 subjects=5 receiptRows=5` |
| `node deploy/ci/scripts/docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` — 14 generated artifact |
| `node deploy/ci/scripts/generate-test-traceability.mjs --check` | `TEST_TRACEABILITY_CURRENT=476` |
| `node deploy/ci/scripts/gate-status.mjs --write` | `GATE_STATUS_WRITTEN` — 11 gate, 151 work item, 23 decision |
| Markdown mapper / W-0153 unresolved | `PASS` — 637 Markdown file; M8-13, W-0153 evidence và target worklist đều `0` unresolved |
| `git diff --check` | `PASS` — chỉ có EOL conversion warnings, không có whitespace error |

## 4. Trạng thái không được suy diễn

- Message kit hoàn chỉnh không đồng nghĩa message đã được gửi hoặc nhận.
- Placeholder recipient không phải owner identity; không được thay bằng tên suy đoán.
- M8-12/manifest hash trong message chỉ chứng minh version nếu sender verify lại trước khi gửi.
- Receipt chứng minh delivery không tự chứng minh authority, approval, shared E2E hoặc production.
- Không code, contract, security, platform, telephony, legal, release hoặc production gate nào mở.

## 5. Handoff

> **Bước kế tiếp:** Module 8 Owner/chief auditor điền actual recipient identity, authority source,
> channel/ticket và due date; verify exact M8-12/manifest hashes; gửi D-01..D-05 và ghi receipt.
> Chỉ sau phản hồi hợp lệ mới cấp work item implementation riêng.
