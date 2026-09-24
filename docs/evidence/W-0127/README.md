# W-0127 — Rút ngắn khoảng cách tới sáu gate còn treo của `W-0122`

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0127 không đóng gate nào của W-0122 mà rút mỗi gate còn treo xuống một hành động: dựng thật profile nghe thử rồi dừng sạch, thêm deploy/lab/New-W0122VoiceAcceptance.ps1 để Owner sinh manifest chấp nhận giọng thay vì viết tay, quét lại Trivy trên image đã rebuild (13 HIGH/3 CRITICAL/0 fixable) kèm đo reachability, và viết ba phiếu hỏi Legal/Security/Platform. Kiểm chứng là các lượt chạy thủ công ghi trong commit df0a26a và mục A-0378 (probe W0122_AUDITION_PROFILE_READY, script có 3 luồng từ chối và 1 luồng thành công qua đúng gate CI); cùng ngày Owner đã dùng chính script này để sinh manifest được ký (A-0379). Ba phiếu hỏi đã được gộp vào plan/ivr-orther/00-CHUA-XONG.md ở W-0297 nên bản đầy đủ chỉ còn trong lịch sử git.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- df0a26a
- deploy/lab/New-W0122VoiceAcceptance.ps1
- docs/evidence/W-0122/voice-audition-runbook.md
- docs/evidence/W-0122/security-performance.md
- docs/evidence/W-0122/README.md
- docs/evidence/W-0122/voice-acceptance-manifest.json
- plan/ivr-orther/00-CHUA-XONG.md
- 2d6033b

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: W-0127 chỉ viết phiếu hỏi, ghi lượt dựng thử profile nghe và lượt quét lại Trivy, và thêm một script lab sinh manifest cho Owner; không sửa src/, tests/ hay gate CI nên không có claim phần mềm để test tự động. Danh sách 3 tài liệu nằm trong khai báo.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0127 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
