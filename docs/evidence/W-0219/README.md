# W-0219 — Dọn worklist: gộp phần đã xong, xếp lại phần còn lại theo người phải quyết

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

Worklist 07/09 đang xếp theo nhóm audit (0.x/A/B/C/X) và không nói ai phải quyết. W-0219 xếp lại thành tám nhóm theo người quyết: anh, M3 ± Security/Product, chief auditor, Security/Platform, Product+CRM+Legal, Order Core, nội bộ M8, và việc cần môi trường thật. Chín mục đã xong được gom thành một bảng trỏ evidence, `B4` được gộp vào `0.5`, và bảng 'mã cũ → mục mới' được thêm để không mục nào trông như bị bỏ (690 → 425 dòng, commit `e391ca8`). Việc chỉ đổi cách trình bày, không đổi kết luận; kiểm bằng đối chiếu đủ 31 mục cũ sang mục mới (0 mục rơi) và 7/7 link nội bộ resolve.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0219 (không có activity riêng)
- e391ca8 (commit duy nhất; worklist 690 → 425 dòng)
- plan/toan-viec-can-lam-m8-2026-09-07.md (bản tại e391ca8; cấu trúc tám nhóm, bảng 'Mã cũ → mục mới', bảng 'Đã xong — không mở lại')
- 39a6243 (W-0224 ghi lý do không viết gói evidence cho W-0218/W-0219)

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: Việc dọn tài liệu: xếp lại worklist 07/09 theo người phải quyết và gom phần đã xong thành một bảng trỏ evidence; không đổi code, test hay gate. Danh sách 1 tài liệu nằm trong khai báo.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0219 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
