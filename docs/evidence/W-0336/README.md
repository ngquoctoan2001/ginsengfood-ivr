# W-0336 — Hồ sơ trình duyệt 9 việc local còn lại

REAL_CUSTOMER_CALL_ALLOWED=NO

Baseline tài liệu: `a6a8f98cf047e59c573f75464c65d5bde195cfff`.
Candidate đã chạy test/sweep: `aaba3d2c173c1ce3b2e6dcbf899515ea5e87c979`.
Trạng thái gói chuẩn bị: **EVIDENCE_SUBMITTED**. Toàn đã duyệt chín việc trong [phiếu trình duyệt](approval-request.md#quyết-định-owner--21092026).
Tracker vẫn là nguồn trạng thái duy nhất; phiếu này không tự chuyển việc nào sang ACCEPTED.

## Kết quả

- Đã đọc lại toàn bộ README của chín việc và phần Residual ở tracker; đối chiếu từng vế với
  [bản rà đầy đủ W-0327](../W-0327/README.md) cùng các bản sửa và bằng chứng về sau.
- Consumer chạy từ checkout sạch aaba3d2 kiểm lại SHA/tree/hash, đủ project và đủ gate của bundle
  W-0330: **1171/1171 test, 42/42 gate chạy, 24 mục classified không có invocation**.
  Không có lượt chạy full suite mới ở commit tài liệu này.
- C1/C2/C4 của **9/9** việc đạt; **22 lượt tham chiếu TestId** được đọc từ README, prompt và
  attachment cùng gói theo consumer thật. Consumer C3 vẫn là **XEM**; quyết định owner được ghi riêng sau khi đọc phạm vi và Residual.
- Candidate là ancestor của baseline; các cây src/tests/tools/deploy và cấu hình solution đã
  đối chiếu không đổi giữa hai mốc. WIP của phiên khác không tham gia kết luận.
- Sửa XML summary lỗi thời trong [SimAdapters](../../../src/Ivr.Infrastructure/Telephony/SimAdapters.cs):
  W-0274 đã chốt từ vựng, W-0275/W-0278 đã triển khai enum và NONE cho dashboard. Đối chiếu sau
  bỏ riêng dòng XML comment cho kết quả giống nhau; không đổi constant hoặc hành vi.
  GitNexus impact trước sửa: LOW, 0 direct, 0 process; số này không được dùng thay việc đọc source.

[Verification](verification.json) ghi từng C1/C2/C4, đủ TestId, nguyên Residual tracker, hash tài liệu,
bundle và bản comment trước/sau. Hash SHA-256 chia nhóm tám ký tự; bỏ dấu nối để được hex chuẩn.
Các addendum trong chín hồ sơ gốc dẫn tới phiếu này, giữ nguyên kết quả và lỗi lịch sử.

## Phạm vi còn lại

- Chín việc được đề nghị cho phần local đã chứng minh, không phải phê duyệt release hay M3 thật.
  Phiếu ghi hành động/người phụ trách từng phần chưa làm; không gán người ký giả.
- Drill W-0196 vẫn chỉ chứng minh cặp ba43605 → c8dc3c4. Muốn phát hành candidate khác phải
  diễn tập đúng cặp binary đó; full test aaba3d2 không thay một lượt rollback.
- W-0207 có [W-0332](../W-0332/README.md) kiểm mới 20 vòng/330 task/11 ca phía IVR tại aaba3d2;
  producer, revalidation, BFF và chữ ký M3 thật vẫn thiếu.
- Trong nhóm 13 gốc: W-0029/W-0032/W-0052 đã được owner duyệt theo phạm vi đã ghi; chín việc ở
  phiếu này đã được owner duyệt ngày 21/09; W-0042 còn thiếu staging/OTLP/thông báo dù W-0334 đã bổ sung proof local.
- Bộ tám việc P2 trong [W-0324](../W-0324/approval-request.md) là một phiếu khác, không gộp vào đây.

Tại mốc trình 4dfd500, tổng ACCEPTED là **48**. Sau phản hồi “tôi chấp nhận”, chín việc gốc
được ghi ACCEPTED, tổng **57**; W-0336 giữ EVIDENCE_SUBMITTED. [Quyết định owner](approval-request.md#quyết-định-owner--21092026)
không sửa SHA/kết quả kiểm chứng cũ và giữ nguyên các giới hạn ngoài local.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: Gói trình duyệt phạm vi local của chín việc, đối chiếu bằng chứng và Residual; ngoài một chú thích XML trong SimAdapters.cs không đổi mã, test hay gate nên không có khẳng định phần mềm để test. Danh sách 3 tài liệu nằm trong khai báo.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.
