# W-0141 — Controlled withdrawal of stale Module 8 V0.3 DOCX

Ngày: `2026-08-29`

Repository HEAD lúc thực thi: `main@0baed74cd384cd661aed068c263a92ef97ead1f4`

Trạng thái: **`TESTS_PASS / WITHDRAWAL_EXECUTED / EXTERNAL_GATES_UNCHANGED`**

Authority: `OD-20=OPTION_1_WITHDRAW`, ký bởi
**Tôi — Module 8 / Project Owner**, ngày `2026-08-29`.

## 1. Phạm vi được phép

Thực hiện đúng một rename recoverable:

```text
docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.docx
→ docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN_SUPERSEDED.docx
```

Không sửa nội dung Word, không xóa binary, không đụng V0.2 DOCX hoặc report DOCX, không thay
runtime/contract/gate.

## 2. Preflight

| Kiểm | Kết quả |
|---|---|
| Source nằm trong workspace | `YES` |
| Source tồn tại trước rename | `YES` |
| Destination tồn tại trước rename | `NO` |
| Kích thước source | `45.101` byte |
| SHA-256 source | `b2b95c9cb62e14b8138538b8447117040207641e5c565e4e1881f3a55af0935c` |
| Exact filename reference trước rename | 1 mention lịch sử trong W-0136 evidence; 0 active Markdown link |

## 3. Kết quả rename

| Kiểm | Kết quả |
|---|---|
| Source cũ tồn tại sau rename | `NO` |
| Destination `_SUPERSEDED` tồn tại | `YES` |
| Kích thước destination | `45.101` byte |
| SHA-256 destination | `b2b95c9cb62e14b8138538b8447117040207641e5c565e4e1881f3a55af0935c` |
| Hash trước/sau | `EQUAL` |

Rename không thay một byte. Artifact được giữ để audit/recovery nhưng tên `_SUPERSEDED` làm rõ nó
không phải bản hiện hành. Bản Markdown V0.3 tiếp tục là nguồn có hiệu lực.

## 4. Hồ sơ đã đồng bộ

- `OD-20`: `IMPLEMENTED / OPTION_1_WITHDRAW` trong
  [`decisions-log.md`](../../../plan/ivr-orther/decisions-log.md).
- Errata `22` trong
  [`MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md`](../../MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md)
  ghi tên `_SUPERSEDED` và hash.
- W-0137 evidence giữ nguyên snapshot lịch sử rồi append follow-up W-0141.
- TODAY-01 signed pack append controlled-execution note; worklist append handoff W-0141.
- Tracker/readiness/gate-status được cập nhật từ nguồn chuẩn; không có ledger thứ hai.

## 5. Verification

| Gate | Kết quả |
|---|---|
| Source absent + destination present | `PASS` |
| Size/hash equality | `PASS` |
| Exact-reference classification | `PASS` — mention tên cũ chỉ còn trong audit/history |
| `node deploy/ci/scripts/gate-status.mjs` | `PASS` — 11 gate, 139 work, 23 open decision; rung 0 |
| `node deploy/ci/scripts/docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |
| Markdown map/link targets | `PASS` |
| Scoped `git diff --check` | `PASS` |

## 6. Trạng thái không được suy diễn

- W-0137 và W-0141 giữ `TESTS_PASS`, không tự nâng `ACCEPTED`.
- Việc thu hồi một tài liệu sai không đóng contract, Legal, Security, Platform, target DB, hosted CI,
  shared integration hoặc Release gate.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ nguyên.
- Bản V0.2 DOCX vẫn còn các reference lịch sử/nguồn cũ; W-0141 không mở rộng sang xử lý nó.

## 7. Handoff

> **HANDOFF W-0141 — OD-20 WITHDRAWAL EXECUTED, BYTES PRESERVED**
>
> DOCX V0.3 lỗi thời đã được thu hồi khỏi tên hiện hành bằng rename `_SUPERSEDED`; hash và kích thước
> không đổi. Không được gửi artifact `_SUPERSEDED` cho vendor như tài liệu hiện hành.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **29/08/2026**.
>
> Chữ ký xác nhận controlled withdrawal; không thay Release acceptance hoặc external approval.

