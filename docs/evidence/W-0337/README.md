# W-0337 — Hoàn tất hồ sơ P2/P3 để owner quyết định

REAL_CUSTOMER_CALL_ALLOWED=NO

Ngày 21/09/2026. Baseline hồ sơ: **53ed4c7ad366deeb959c764c61a748490bf2fab7**.
Trạng thái: **EVIDENCE_SUBMITTED**. [Phiếu quyết định](approval-request.md) gồm **9 việc P2 đề nghị
ACCEPTED trong phạm vi local/MOCK** và **4 việc P3 đề nghị CANCELLED vì UI đã ngừng thuộc IVR**.
Tất cả quyết định đang PENDING; không tự chuyển trạng thái. Tổng ACCEPTED giữ 57.

## Kiểm chứng

Candidate runtime: **aaba3d2c173c1ce3b2e6dcbf899515ea5e87c979**, tree
406a786cb02cd2d382657db841eecce29e6c0db5. Consumer chạy từ checkout sạch, đọc Git tại đúng SHA.

- Bundle W-0330 được kiểm lại hash/provenance: **1171/1171 test, 42/42 gate chạy**, 24 entry được
  manifest phân loại không có invocation riêng. Không chạy lại full suite cho commit tài liệu này.
- Chín P2 đạt C1/C2/C4, **142 lượt tham chiếu TestId** hợp lệ, gồm shell assertion được ánh xạ
  sang gate bắt buộc. Đây không phải 142 test độc lập hay một lượt test mới. C3 vẫn là XEM.
- W-0019 xét 24 TestId hiện hành; hai ID lịch sử được retire theo OD-17/OD-18, với 11 test thay thế.
  Đã xác minh ba pin quyết định duy nhất: SHA là ancestor, file/hash và trích đoạn khớp.
- Cả bốn UI P3 bị C2 từ chối đúng lý do scope đã ngừng. Source admin-ui không tồn tại tại candidate;
  gói đề nghị CANCELLED theo W-0253, không dùng test backend để chứng nhận UI.
- Phụ lục đối chiếu README, attachment cùng gói, prompt, mọi class trong source và kết quả TRX.
  Kết quả 13 dòng khớp CLI acceptance-batches thật trên cùng candidate.
- Giữa candidate và baseline, các cây src/tests/tools/deploy cùng cấu hình solution chỉ khác
  XML comment SimAdapters của W-0336; phần ngoài XML giống nhau. Shared WIP TTS/lab bị loại khỏi proof.

[verification.json](verification.json) lưu verdict từng việc, đầy đủ TestId, nguyên Residual,
hash tài liệu và pin quyết định. SHA-256 được chia nhóm tám ký tự; bỏ dấu nối để lấy hex chuẩn.
Snapshot CLI và helper tại `.artifacts/w0337-p2p3-closeout/`; bundle gốc tại
`.artifacts/w0330-acceptance-aaba3d2/acceptance-run.json`.

Bộ phụ lục tạm lúc đầu chưa nạp sourceClasses nên không tìm đúng hai class TTS trong một file.
TRX có cả hai test Passed; helper đã nạp class từ source đúng cách CLI dùng, rồi đối chiếu lại toàn
bộ 13 dòng với CLI thật. Không sửa công cụ C2, traceability hoặc kết quả test để bỏ qua lỗi này.

## Rà phạm vi và phần còn lại

Đã đọc hồ sơ P2, các báo cáo test đính kèm, retirement W-0019, bốn closeout P3 và toàn bộ Residual
đã lưu trong [W-0322](../W-0322/README.md). [W-0324](../W-0324/README.md) đã xử lý lỗi C2 và năm
hồ sơ thiếu; [W-0328](../W-0328/README.md) bổ sung parser slash/range. W-0337 kết hợp các kết quả
để Toàn xét dứt điểm cả P2/P3, không mở thêm một backlog trạng thái.

- P2: mock/runtime local, không nhận real M3/issuer/capacity SIM, script production hoặc TTS trên máy đích.
- W-0019 đủ đề nghị theo authority hiện hành: M3 quyết định có gọi; không phục hồi trust-skip hoặc
  sellable predicate cũ. Các điều kiện kỹ thuật và an toàn của IVR vẫn được kiểm.
- W-0066 chỉ xét port/fake/cache/privacy. Các thay đổi/hồ sơ model-image, hàng chờ TTS và S5 đang
  nằm trong WIP riêng, không được nghiệm thu thông qua W-0066 tại aaba3d2.
- P3: giữ lịch sử source/test/capture, enum labels/spec phục vụ M3 và backend đang sống. Toàn
  xác nhận đầu mối M3 khi cần bàn giao; chưa có xác nhận người nhận hoặc thông điệp gửi trong lượt này.
- Tám việc P2 của phiếu W-0324 được giữ nguyên phạm vi; W-0019 được bổ sung thành việc thứ chín.
  Quyết định chín việc W-0336 vừa qua không bao gồm nhóm P2/P3 này.

Hành động và người phụ trách từng Residual nằm trong [phiếu quyết định](approval-request.md).
Không phát hiện phần runtime local bắt buộc phải triển khai thêm để trình các phạm vi này.
