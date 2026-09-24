# W-0281 — Phiếu yêu cầu Platform/Infra cho `G-GITLAB` và `G-PLATFORM`, và đính chính ma trận API trong báo cáo tuần

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0281 soạn phiếu yêu cầu một vòng gửi Platform/Hạ tầng cho G-GITLAB (runner thẻ ginsengfood-docker chạy được Docker, nâng gói GitLab Premium/Ultimate, một người rà soát thứ hai) và G-PLATFORM (sáu hạng mục staging), kèm số đo tài nguyên từ lượt chạy tay W-0280, ở trạng thái READY_TO_DISPATCH / NOT_SENT; đồng thời đính chính báo cáo tuần 12/09 rằng ma trận hành vi 38 lệnh API đã có từ W-0197 (38/38 lệnh, 460 tình huống HTTP, 0 lỗi hành vi). Kiểm chứng là đọc lại .artifacts/api-matrix/http-observations.json sinh lại trong ngày (thư mục bị gitignore, không có trong repo) và docs/gate self-test PASS; commit d5a57c1 không đổi mã nguồn, test hay cổng kiểm. Tệp phiếu đã bị W-0297 xoá khỏi cây (toàn văn ở d5a57c1 và 257cbef^), còn W-0309 chuyển nội dung sang Mục 1 và Mục 3 của phiếu cho Sếp, đã được trả lời ngày 17/09.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- d5a57c1
- d5a57c1:plan/ivr-orther/questions-to-platform-ci-and-staging-2026-09-12.md
- 257cbef
- docs/reports/12-09-bao-cao-tien-do-module-8-ivr.md
- plan/ivr-orther/00-CHUA-XONG.md
- d655989
- plan/ivr-orther/phieu-quyet-dinh-cho-sep-2026-09-17.md

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: Mục này chỉ soạn phiếu yêu cầu gửi Hạ tầng và đính chính báo cáo tuần 12/09; không đổi mã nguồn, test hay cổng kiểm nào. Tệp phiếu sau đó bị W-0297 xoá; nội dung chuyển vào phiếu quyết định 17/09, còn lại một dòng trong báo cáo 12/09. Danh sách 1 tài liệu nằm trong khai báo.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0281 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
