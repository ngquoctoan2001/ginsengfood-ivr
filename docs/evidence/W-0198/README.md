# W-0198 — GĐ 2 đợt 2: khung giờ được phép gọi + bản attempt policy production đã ký

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

Một bản ghi lịch sử ở mục 9 của tracker từng dùng số W-0198 cho việc P1.1 nay là W-0197. README này thuộc W-0198 hiện hành: khung giờ được phép gọi và bản attempt policy production đã ký.

## Việc đã làm

Trước lượt này hệ thống không có khái niệm giờ được phép gọi. W-0198 thêm `CallingWindow` theo giờ Việt Nam (offset cố định +07:00), chặn quay số sau bước thu hồi lease/đóng deadline và trước khi claim, từ chối cửa sổ rỗng hoặc lộn ngược lúc khởi động. Cùng lượt đăng ký bản attempt policy production đã ký `gh-247-prod-v1` theo số D-10 thành một version mới thay vì thăng hạng `mock-lab-v1`. Kiểm bằng `UT-SCH-WINDOW-01..07` và `UT-POLICY-SIGNED-01..05` (commit gốc `5cffea3`, message ghi id cũ `W-0196`). Sau đó W-0220 dời giờ đóng sang 21:08 và W-0300 gỡ hai dòng seed `approved_for_production = TRUE` khỏi schema; catalogue C# và các test trên vẫn giữ nguyên.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0198; §9 completion record 'Work ID: W-0198' (bản của Claude, ngay sau bản W-0196 của Codex); activity A-0562..A-0566
- 5cffea3 (commit gốc, chung với W-0199; message ghi id cũ W-0196)
- cc12e53 và 4fde987 (đánh số lại và merge vào main)
- src/Ivr.Infrastructure/Scheduling/CallingWindow.cs; src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs; src/Ivr.Infrastructure/Persistence/Migrations/20260905120000_W0196SignedProductionAttemptPolicy.cs
- tests/Ivr.UnitTests/Scheduling/CallingWindowTests.cs; tests/Ivr.UnitTests/Intake/SignedProductionAttemptPolicyTests.cs; tests/Ivr.UnitTests/Scheduling/DeadlineSchedulerTests.cs
- việc sau có đổi phần này: 6993e3f (W-0220, giờ đóng 21:08), 641d294 (W-0300, gỡ seed approved_for_production)

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `UT-SCH-WINDOW-01`, `UT-SCH-WINDOW-02`, `UT-SCH-WINDOW-03`, `UT-SCH-WINDOW-04`, `UT-SCH-WINDOW-05`, `UT-SCH-WINDOW-06`, `UT-SCH-WINDOW-07`, `UT-POLICY-SIGNED-01`, `UT-POLICY-SIGNED-02`, `UT-POLICY-SIGNED-03`, `UT-POLICY-SIGNED-04`, `UT-POLICY-SIGNED-05`.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0198 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Phần còn lại
của GĐ 2 thuộc các việc sau. Claude chọn việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt;
Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
