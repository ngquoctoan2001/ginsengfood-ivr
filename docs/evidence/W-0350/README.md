# W-0350 — Đóng các việc đã xong theo chỉ thị owner

Ngày 24/09/2026 · Claude · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Đã ghi sổ. 92 việc chuyển `ACCEPTED`, 2 việc chuyển `CANCELLED`, 3 việc giữ lại.**

## Chỉ thị

Sau báo cáo [W-0349](../W-0349/README.md) tại `11fa118`, Toàn trả lời: *“thì cái nào xong cho xong luôn đi”*.
Theo tiền lệ A-0691, câu này được hiểu là chỉ thị đóng mọi việc đã thật sự xong, không cần phiếu riêng cho từng
việc. Toàn giao việc chọn cho Claude, nên Claude chọn theo đúng tiêu chí của [phiếu W-0348](../W-0348/approval-request.md):
một việc được đóng khi C1, C2, C4 đạt và cột Residual chỉ còn việc bên ngoài, giới hạn đã ghi, hoặc câu hỏi mà
việc sau đã trả lời. Việc nào Residual còn việc phía IVR thì giữ lại. Toàn có thể đảo bất kỳ dòng nào.

## Kết quả

| Kết quả | Số việc |
| --- | ---: |
| `ACCEPTED` | 92 |
| `CANCELLED` | 2 |
| Giữ lại, còn việc phía IVR | 3 |
| Chưa khai được phép kiểm, không đóng | 4 |

- **92 việc `ACCEPTED`** là 92 trong 95 việc mà W-0349 đưa lên `XEM`. 62 việc có Residual chỉ còn việc bên
  ngoài; 30 việc có Residual nhắc việc mà việc sau đã làm, và mỗi việc ghi rõ việc nào đã làm. Trong số đó có
  **W-0016 (P1-3)**, nên Nấc 1 lên **36/54**. Tổng `ACCEPTED` lên 241/338.
- **2 việc `CANCELLED`**, theo đề nghị đã trình ở W-0349:
  - **W-0200**: mọi thay đổi đã bị rút khi merge ở `4fde987`;
  - **W-0097**: Admin UI đã bị gỡ khỏi repo ở W-0253.
- **3 việc giữ lại**, vì Residual còn việc phía IVR mà chưa ai làm. Đối chiếu với cây hiện tại xác nhận cả ba:
  - **W-0203**: F-2 và F-3 còn mở. Dead letter chỉ replay được bằng SQL tay, vì không có endpoint nào. Luồng
    quay số MOCK không nhận cổng kill switch runtime. F-1 đã được W-0286 sửa.
  - **W-0225**: hai bản luật, trong `tts-provenance-gate.mjs` và `verify-model.py`, vẫn nằm hai nơi. Việc gộp
    về một chỗ mà Residual ghi là chưa làm.
  - **W-0297**: còn việc chạy lại bản đồ tài liệu. Lượt dồn này cũng từng làm đỏ 11 gate và xoá gói quyết định
    đã ký; W-0304 phải khôi phục. Nên để Toàn tự đọc.
- **4 việc chưa khai được phép kiểm** (W-0078, W-0094, W-0168, W-0205) không được đóng. Lý do ghi trong README
  của từng việc.

Danh sách đầy đủ, kèm lý do từng việc, nằm ở [closeout.json](closeout.json).

## Cách ghi

- Tracker: đổi trạng thái 94 dòng; activity `A-0859` phân loại, `A-0860`..`A-0953` cho từng việc, `A-0954` ghi
  quyết định.
- 90 README có thêm mục ghi nhận ngày 24/09. README của W-0128, W-0135, W-0141 và W-0343 bị ghim hash nên không
  sửa; quyết định của bốn việc này nằm ở tracker và ở đây.
- Bằng chứng vẫn là lượt collector tại `1859243`: 1200/1200 test .NET, GATE_SWEEP_PASS 43/43. Commit tài liệu
  sau đó không được coi là một lượt test mới.

Không mở cuộc gọi khách thật, không phê duyệt production, không đóng việc của M3, Sales, Platform hay Security.

## Owner nghiệm thu — 24/09/2026

Toàn hỏi “29 việc này cần quyết gì không? ko thì clean đi”, sau chỉ thị “thì cái nào xong cho xong
luôn đi”. Theo [danh sách W-0351](../W-0351/README.md), W-0350 chuyển **EVIDENCE_SUBMITTED →
ACCEPTED** cho phần đã làm. Việc ghi sổ theo chỉ thị owner đã xong. Claude chọn theo tiêu chí phiếu
W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`a8a3e57`. REAL_CUSTOMER_CALL_ALLOWED=NO.
