# W-0348 — Phiếu duyệt một lượt các việc XEM

Ngày 23/09/2026 · Claude · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Đã trình [phiếu](approval-request.md). Toàn đã duyệt nhóm A và B.**

## Vì sao có việc này

Toàn hỏi vì sao mới có 71/335 việc `ACCEPTED`, và yêu cầu gom các việc duyệt nhanh để duyệt một lần.

Phân bố trạng thái khi trình, tại `a7ff0e7`:

| Trạng thái | Số việc |
| --- | ---: |
| `TESTS_PASS` | 192 |
| `EVIDENCE_SUBMITTED` | 25 |
| `ACCEPTED` | 71 |
| `CANCELLED` | 21 |
| `BLOCKED_EXTERNAL` | 19 |
| `CODE_DONE` | 3 |
| `N/A` | 2 |
| `DEFERRED_TARGET` | 2 |

217 việc đã xong phía dev và chờ duyệt. Danh sách tại `ca4f442` chia chúng như sau:
- 84 mục XEM: đạt C1, C2, C4, chỉ còn đọc Residual. Phiếu này xử lý nhóm đó.
- 132 mục KHÔNG ĐẠT. 101 mục không khai test hay phép kiểm nào; phần lớn là việc tài liệu, quyết
  định và kiểm toán. 31 mục trích TestId không còn trong traceability.
- W-0347 nộp sau lượt collector nên chưa có trong danh sách.

## Kết quả

| Nhóm | Số mục | Nghĩa |
| --- | ---: | --- |
| A | 73 | Chỉ còn việc bên ngoài, hoặc câu hỏi đã được việc sau trả lời. 9 mục thuộc kế hoạch |
| B | 5 | Mô hình capacity, gate đạt có điều kiện chưa hiệu chỉnh |
| C | 6 | Chưa trình: còn việc phía IVR hoặc cần rà lại phạm vi |

## Kiểm chứng

- Script đối chiếu phiếu với danh sách: 84 mục XEM, mỗi mục xuất hiện đúng một lần.
- C1, C2, C4 lấy từ danh sách tại `ca4f442`. Phiếu không nhận commit tài liệu là lượt test mới.
- Sáu Residual được đối chiếu với cây hiện tại, ghi ở cuối phiếu.

Chỉ Toàn chuyển các việc này sang `ACCEPTED`.

## Owner duyệt — 23/09/2026

Toàn trả lời “chấp nhận nhóm A và B, push luôn” sau phiếu tại `7fc9806`. 78 việc chuyển sang
`ACCEPTED`: 73 việc nhóm A và 5 việc capacity nhóm B, kèm điều kiện mô hình chưa hiệu chỉnh. Tổng
`ACCEPTED` từ 71 lên 149/336, Nấc 1 từ 25 lên 35/54.

Mỗi hồ sơ của 78 việc có mục "Owner nghiệm thu", trừ W-0161: README của nó bị validator opt-out ghim
hash, nên bản ghi của W-0161 nằm ở đây và trong tracker. Nhóm C chưa trình. W-0348 giữ
`EVIDENCE_SUBMITTED`. REAL_CUSTOMER_CALL_ALLOWED=NO.

## Khai báo phép kiểm C2 — W-0351, 24/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

[Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: Phiếu duyệt một lượt các việc XEM; không đổi code, test hay gate.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.
