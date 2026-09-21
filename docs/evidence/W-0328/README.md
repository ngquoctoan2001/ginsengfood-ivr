# W-0328 — C2 kiểm đủ TestId viết tắt

REAL_CUSTOMER_CALL_ALLOWED=NO

Baseline: `3e06c17933f7f14a40baf95ef42cc780952e508f`. Phạm vi: công cụ nghiệm thu và kiểm thử của nó.

## Lỗi và bản sửa

W-0327 phát hiện `IT-API-TERMINATE-09/10` trong W-0207 chỉ được C2 đọc thành
`IT-API-TERMINATE-09`. Test thứ hai có thể thiếu hoặc thất bại mà hồ sơ vẫn qua C2.

Parser nay mở từng hậu tố slash, giữ độ rộng số, hỗ trợ hậu tố chữ như `CT-CI-06/06b`
và chuỗi nhiều phần tử. Slash là danh sách, không tự thêm các số ở giữa. Chuỗi trộn
slash và khoảng được đọc đủ; khoảng đảo/ngưỡng quá lớn/hậu tố số sai cú pháp bị từ chối.
Nhóm shell `CT-CI-06..06h` dùng danh sách assertion cố định trong GATE_TESTS: `06`, rồi
`06b` đến `06h`. Runner selftest-pii.sh không có `06a`. Chỉ nhóm đã đăng ký cùng runner
và có cả hai đầu mút mới được mở khoảng chữ; không suy đoán test không tồn tại.
Các chặn commit, tree, kết quả cũ, hash và đủ gate giữ nguyên.

## Kiểm chứng ban đầu

- Regression mới chạy trên parser cũ: FAIL, thiếu `IT-API-TERMINATE-10` đúng lỗi đã nêu.
- Sau sửa: `acceptance-batches.mjs --self-test` PASS, 45 kiểm tra C2 (thêm 20).
- `CT-ACCEPTANCE-C2-01` và `CT-ACCEPTANCE-EVIDENCE-01`: hai gate bắt buộc; provenance có 47 kiểm tra PASS.
- Ca âm gồm thiếu kết quả, Failed, NotExecuted, thiếu traceability và hậu tố sai; ca dương đòi đủ test xanh.
- GitNexus impact: citedTestIds LOW, 2 caller trực tiếp, 5 phụ thuộc, 0 flow.
  c2SelfTest chưa có trong index; đối chiếu source xác nhận chỉ selfTest gọi trực tiếp.
  Query báo thiếu FTS; không dùng kết quả rỗng để kết luận không có phụ thuộc.

## Phần phải hoàn tất

Lượt đầu ở `a9da739` đã dừng sau khi phát hiện 12 gói dùng khoảng chữ; log được giữ ở
`.artifacts/w0328-acceptance-a9da739/`, không có acceptance manifest và không dùng để nghiệm thu.
Chốt commit chứa bản sửa, chạy toàn bộ solution rồi full gate sweep trên checkout sạch,
lưu bundle đúng SHA và sinh lại danh sách nghiệm thu. Kết quả ban đầu bên trên chưa thay thế bước đó.
Toàn quyết định ACCEPTED; hồ sơ này không cho phép gọi khách thật hoặc xác nhận M3 tích hợp.
