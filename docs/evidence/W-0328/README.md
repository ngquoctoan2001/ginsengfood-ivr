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

## Kiểm chứng cuối — hoàn tất phần sửa C2

Candidate: `1794241dfbbc919409d03db7232d8a9ad504b9f8`. Bộ thu xác minh checkout sạch trước/sau cả hai lệnh,
cùng commit/tree, hash TRX, đủ project và từng gate trong manifest của chính commit này.

| Kiểm tra | Kết quả |
| --- | --- |
| Toàn solution | 1151/1151 Passed; 4 project; 0 fail/skip |
| Full sweep | 42/42 gate chạy; 24 mục classified theo manifest |
| C2 regression | 45 PASS, tăng 20 so với trước sửa |
| Provenance/CLI regression | 47 PASS |
| Đối chiếu parser trên hồ sơ | 20 gói đọc thêm 98 lượt TestId; 0 verdict đổi do parser |
| Danh sách mới | 224 ứng viên: 73 XEM, 151 KHÔNG ĐẠT, 0 ĐẠT, 0 chưa kiểm |

Thời gian UTC: test 2026-09-21T06:25:45.504Z → 2026-09-21T06:34:08.569Z;
sweep 2026-09-21T06:34:09.199Z → 2026-09-21T06:41:03.333Z.

W-0207 nay xét đủ IT-API-TERMINATE-09 và IT-API-TERMINATE-10; cả hai xanh trong TRX.
Các lượt tham chiếu bổ sung đều được C2 đối chiếu; số XEM tăng so với W-0325 vì hai
hồ sơ W-0325/W-0327 mới vào tập ứng viên ở commit này, không do nới tiêu chí.

- [Xác minh, hash và thời gian](verification.json).
- [Đối chiếu từng ứng viên và 20 gói bị ảnh hưởng](c2-assessment.json).
- [Danh sách nghiệm thu sinh từ kết quả mới](../../release/acceptance-batches.md).
- [Bundle gốc giữ trên máy](../../../.artifacts/w0328-acceptance-1794241/acceptance-run.json).

Lượt đầu tại a9da739 dừng vì cần bổ sung khoảng assertion chữ; giữ toàn bộ log tại
.artifacts/w0328-acceptance-a9da739, không có manifest và không dùng làm bằng chứng nghiệm thu.
Lỗi MSBuild sau thời điểm dừng là do huỷ cây tiến trình đó; không phải kết quả của lượt cuối.

Phần sửa C2 đã xong. Danh sách ghim candidate trên; commit tài liệu sau không được gán lại test.
Giữ nguyên trạng thái 13 việc đã rà và các gói cũ. Toàn quyết định ACCEPTED.
Bước tiếp thực thi phần local E2E W-0207; M3, staging và quyền gọi khách vẫn chưa được xác nhận.
