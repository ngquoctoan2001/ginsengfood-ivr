# W-0328 — C2 kiểm đủ TestId viết tắt

REAL_CUSTOMER_CALL_ALLOWED=NO

Baseline: `3e06c17933f7f14a40baf95ef42cc780952e508f`. Phạm vi: công cụ nghiệm thu và kiểm thử của nó.

## Lỗi và bản sửa

W-0327 phát hiện `IT-API-TERMINATE-09/10` trong W-0207 chỉ được C2 đọc thành
`IT-API-TERMINATE-09`. Test thứ hai có thể thiếu hoặc thất bại mà hồ sơ vẫn qua C2.

Parser nay mở từng hậu tố slash, giữ độ rộng số, hỗ trợ hậu tố chữ như `CT-CI-06/06b`
và chuỗi nhiều phần tử. Slash là danh sách, không tự thêm các số ở giữa. Chuỗi trộn
slash và khoảng được đọc đủ; khoảng đảo/ngưỡng quá lớn/hậu tố số sai cú pháp bị từ chối.
Các chặn commit, tree, kết quả cũ, hash và đủ gate giữ nguyên.

## Kiểm chứng ban đầu

- Regression mới chạy trên parser cũ: FAIL, thiếu `IT-API-TERMINATE-10` đúng lỗi đã nêu.
- Sau sửa: `acceptance-batches.mjs --self-test` PASS, 40 kiểm tra C2 (thêm 15).
- Ca âm gồm thiếu kết quả, Failed, NotExecuted, thiếu traceability và hậu tố sai; ca dương đòi đủ test xanh.
- GitNexus impact: citedTestIds LOW, 2 caller trực tiếp, 5 phụ thuộc, 0 flow.
  c2SelfTest chưa có trong index; đối chiếu source xác nhận chỉ selfTest gọi trực tiếp.
  Query báo thiếu FTS; không dùng kết quả rỗng để kết luận không có phụ thuộc.

## Phần phải hoàn tất

Chốt commit chứa bản sửa, chạy toàn bộ solution rồi full gate sweep trên checkout sạch,
lưu bundle đúng SHA và sinh lại danh sách nghiệm thu. Kết quả ban đầu bên trên chưa thay thế bước đó.
Toàn quyết định ACCEPTED; hồ sơ này không cho phép gọi khách thật hoặc xác nhận M3 tích hợp.
