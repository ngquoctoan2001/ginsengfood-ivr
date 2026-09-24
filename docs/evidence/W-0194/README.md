# W-0194 — Owner sign-off 19 quyết định OD-V1/OD-VOICE

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

Ngày 05/09/2026 owner ký một lượt 19 trong 23 quyết định OD-V1/OD-VOICE đang mở theo phương án Claude soạn sau khi đối chiếu từng dòng với code đang chạy; chữ ký ghi vào `specs/_review/open-decisions-register.md` và gói ký (nay còn bản rút gọn ở `plan/ivr-orther/00-DA-XONG.md#od-v1-signoff`, bản đầy đủ trong lịch sử git). Cùng lượt thêm cột `Current` cho bảng bốn cột để `gate-status.mjs` không đọc nhầm cột Gate làm trạng thái. Kiểm bằng `gate-status.mjs --write`: số quyết định mở giảm 23 → 4 và `GATE_STATUS_PASS`; không đổi code, test, OpenAPI hay migration.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0194; §9 completion record 'Work ID: W-0194'; activity A-0554, A-0555
- 4baad09 (commit gốc; message ghi id cũ w-0191)
- 7dc9a8c (đánh số lại w-0191 → W-0194)
- ba43605 (merge worktree-gd0-fixes vào main)
- specs/_review/open-decisions-register.md (header dòng 3-12; cột Current của bảng 'P0 — lab/production calls', dòng 52-54)
- plan/ivr-orther/00-DA-XONG.md#od-v1-signoff (bản rút gọn 5 dòng do W-0297 viết)
- 7dc9a8c:plan/ivr-orther/od-v1-signoff-2026-09-05.md (gói ký đầy đủ 112 dòng; bị xoá ở 257cbef / W-0297)
- docs/reports/05-09-bao-cao-tien-do-module-8-ivr.md (mục 5 và mục 8)

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: Việc ký quyết định: ghi chữ ký owner cho 19 dòng OD-V1/OD-VOICE vào register và gói ký, thêm cột Current cho bảng bốn cột; không đổi code, test hay gate. Danh sách 2 tài liệu nằm trong khai báo.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0194 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. OD-V1-11 và
OD-V1-21 đóng ở W-0266; OD-V1-09 và OD-V1-10 chờ số đo thật. Claude chọn việc theo tiêu chí phiếu
W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
