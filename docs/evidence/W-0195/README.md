# W-0195 — GĐ 2 đợt 1: ba cổng runtime thực thi + sửa bất đối xứng kill switch

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0195 thay ba lớp `Pending*` trả cứng `false` bằng ba hiện thực đọc bảng phê duyệt append-only `ivr_runtime_gate_approvals` (migration `20260905034908_W0195RuntimeGateApprovals`), với bốn mắt, fingerprint, hạn dùng và tính bất biến cưỡng chế ở tầng DB, và sửa một lỗi thật: `MutateAsync` hỏi cổng phê duyệt trước khi xét thay đổi là gì nên khi cổng chưa cấp thì API không bật được kill switch, trái W-0068. Kiểm bằng `IT-GATE-APPROVAL-02..09` chạy trên PostgreSQL thật cùng `UT-FLAG-KILLSWITCH-12` và `IT-FLAG-OWNERGATE-12` cho hai chiều bất đối xứng kill switch (commit gốc `7be49a8`, message ghi id cũ `w-0192`). Hai phần của lượt này đã bị việc sau đảo: W-0301 thu hồi dòng phê duyệt quản trị không gắn môi trường mà migration seed, và W-0299 trả mặc định whitelist lời thoại về NO.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0195; §9 completion record 'Work ID: W-0195'; activity A-0556, A-0557
- 7be49a8 (commit gốc; message ghi id cũ w-0192)
- 7dc9a8c (đánh số lại; đổi tên migration W0192 → W0195)
- ba43605 (merge vào main)
- src/Ivr.Infrastructure/FeatureFlags/RuntimeGateApprovals.cs, RuntimeGateApprovalEntity.cs, FeatureFlagAdminService.cs
- src/Ivr.Infrastructure/Persistence/Migrations/20260905034908_W0195RuntimeGateApprovals.cs
- tests/Ivr.IntegrationTests/RuntimeGateApprovalTests.cs; tests/Ivr.UnitTests/FeatureFlagPlatformTests.cs; tests/Ivr.IntegrationTests/FeatureFlagApiTests.cs
- docs/reports/05-09-bao-cao-tien-do-module-8-ivr.md (mục 3 và 4)
- việc sau có đổi phần này: 4bd633c (W-0213), b817fae (SIP-04), e72ca84 (W-0299), 1651e8f (W-0301)

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `IT-GATE-APPROVAL-02`, `IT-GATE-APPROVAL-03`, `IT-GATE-APPROVAL-04`, `IT-GATE-APPROVAL-05`, `IT-GATE-APPROVAL-06`, `IT-GATE-APPROVAL-07`, `IT-GATE-APPROVAL-08`, `IT-GATE-APPROVAL-09`, `UT-FLAG-KILLSWITCH-12`, `IT-FLAG-OWNERGATE-12`.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0195 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Phần còn lại
của GĐ 2 thuộc các việc sau. Claude chọn việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt;
Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
