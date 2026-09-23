# W-0323 — đối chiếu các chốt còn mở

Ngày 23/09/2026. `REAL_CUSTOMER_CALL_ALLOWED=NO`; production vẫn `BLOCKED`.

[README](README.md) ngày 21/09 kết thúc ở `CODE_DONE` với bốn chốt còn mở. Cả bốn đã được đóng bằng
bằng chứng của các work item làm sau, trên đúng máy đích S5. File này chỉ đối chiếu. Nó không chạy
lại phép đo nào và không sửa README gốc.

| Chốt còn mở (21/09) | Đóng bởi | Bằng chứng | Trạng thái của work item đó |
| --- | --- | --- | --- |
| Khởi động lạnh vượt ngân sách 10 giây; tải và CPU/memory trên máy đích | `W-0333` bộ đo S5; `W-0335` hàng chờ chuẩn bị lời thoại, 122/122 đơn trên S5; `W-0338` giới hạn toàn bộ bước chuẩn bị 30/90/120 giây, 128/128 đơn trên S5 với 2 CPU/4 GiB; `W-0343` S5: khởi động 8,2 s, khởi động lại 5,5 s, 2 lượt soak 457 s mỗi lượt 63/63 job, 0 lỗi | [W-0333](../W-0333/README.md) · [W-0335](../W-0335/README.md) · [W-0338](../W-0338/README.md) · [W-0343](../W-0343/s5-target-result.md) | W-0338 `ACCEPTED`; các mục còn lại `EVIDENCE_SUBMITTED` |
| Pháp lý model | `W-0341` rà quyền dùng theo nguồn công bố; `W-0342` đưa bằng chứng giấy phép vào hồ sơ và verifier; `W-0343` Toàn xác nhận Legal/Privacy có thẩm quyền kiêm nhiệm | [W-0341](../W-0341/README.md) · [W-0342](../W-0342/README.md) · [W-0343](../W-0343/legal-approval.json) | `EVIDENCE_SUBMITTED` |
| Mirror nội bộ | `W-0340` mirror trên vps61, 39/39 file khôi phục được; `W-0343` bản phát hành `vieneu-w0343` trên cùng mirror | [W-0340](../W-0340/README.md) · [W-0343](../W-0343/s5-target-result.md) | `EVIDENCE_SUBMITTED` |
| Ba ca DTMF bị phím ngoài can thiệp, chưa xác minh | `W-0329` tự động hóa DTMF 6/6 và không bấm phím; `W-0339` full-flow trên lab SIP riêng; `W-0344` full-flow 7 ca trên S5 (bấm 1, bấm 0, không bấm), mọi lần bấm đều sau khi nghe hết lời thoại | [W-0329](../W-0329/README.md) · [W-0339](../W-0339/clean-commit-closeout.md) · [W-0344](../W-0344/cpu-retry.md) | W-0339 `ACCEPTED`; W-0329, W-0344 `EVIDENCE_SUBMITTED` |

Phần của chính W-0323 đã xong từ 21/09 và không đổi: giọng và câu ghép được Owner chấp nhận
([phiếu](owner-decision.md)), đã chọn nền Chainguard, và đã thử đơn mở rộng.

**Đề nghị:** chuyển W-0323 sang `EVIDENCE_SUBMITTED`. Chỉ Toàn chuyển sang `ACCEPTED`. Ba chốt dựa
trên các work item mới ở `EVIDENCE_SUBMITTED`, nên nghiệm thu W-0323 nên đi cùng hoặc sau các mục đó.
Không mục nào ở đây bật khách thật hay chứng nhận production.
