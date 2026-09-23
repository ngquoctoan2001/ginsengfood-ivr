# W-0200 — GĐ 2 đợt 4 — **CANCELLED**: OpenAPI `1.0.0-draft.22` → `1.0.0` + tách hai trục phiên bản/vòng đời

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0200 chuẩn bị bump OpenAPI `1.0.0-draft.22` → `1.0.0` theo `OD-V1-02`: xoay baseline, đổi tên bản scaffold P1-1, và tách chuỗi phiên bản khỏi trạng thái phê duyệt trong `docs/api-versioning.md` (commit `ca1331a`, message ghi id cũ `W-0198`). Khi merge vào main ở `4fde987`, bản `draft.23` của W-0197 đã đổi văn bản hợp đồng, nên theo chỉ đạo owner bản phát hành bị rút: mọi file W-0200 chạm đều lấy lại nội dung phía main, và `docs/api-changelog.md` ghi `W-0200`, `CANCELLED`. Tại HEAD không còn thay đổi nào của W-0200 để kiểm; ý 'chuỗi phiên bản không phải chữ ký' về sau được W-0204 hiện thực trong `contract-freeze-verifier.mjs`.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0200 (tiêu đề ghi CANCELLED, cột Status vẫn TESTS_PASS); §9 completion record 'Work ID: W-0200'; activity A-0569, A-0570
- ca1331a (commit gốc; message ghi id cũ W-0198)
- 4fde987 (merge vào main; message: 'W-0200 lands as CANCELLED rather than being deleted')
- docs/api-changelog.md:243 ('the release was withdrawn (W-0200, CANCELLED)')
- §9 completion record W-0202, residual (3): 'W-0200 CANCELLED'
- deploy/ci/scripts/contract-freeze-verifier.mjs:333 (FREEZE-04 của W-0204 nhắc lại luận điểm của W-0200)

## Phép kiểm C2

Trạng thái khai báo:

- Chưa khai phép kiểm. Mọi thay đổi của W-0200 đã bị rút khi merge ở 4fde987, và changelog API ghi W-0200 là CANCELLED. Tại HEAD không còn gì để kiểm. Đề nghị Toàn chuyển dòng này sang CANCELLED thay vì nghiệm thu.
