# W-0218 — Kiểm lại bốn mục `C1`/`C2`/`C5`/`C10` từng bị gộp một dòng "chưa làm"

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

Bốn mục C1, C2, C5, C10 của worklist 07/09 bị gộp chung một dòng 'chưa làm' với ghi chú 'phần lớn gộp vào 0.3/0.4'. W-0218 kiểm từng mục trên tài liệu và code và kết luận cả bốn đã xong phần của M8: C1 có nguồn business và bảng cặp program × payment trong IR-06 §3.10; C2 có correction ở IR-06 §4.3 và cầu nối m8-05 §3.1; C5 đủ pack nhưng chưa gửi ra ngoài; tiêu chí cũ của C10 đã rút. Worklist `plan/toan-viec-can-lam-m8-2026-09-07.md` được tách dòng đó thành bốn và viết lại mục C kèm bằng chứng (commit `bcb79a4`), không đổi code hay test. Lượt này còn phát hiện câu lạc ở `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md:472`: câu đó ghi `result_type` có 11 giá trị trong khi DB cho đúng 6. Câu này cố ý không tự sửa vì thẩm quyền thuộc chief auditor/Owner.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0218 (không có activity riêng)
- bcb79a4 (commit duy nhất)
- plan/toan-viec-can-lam-m8-2026-09-07.md (bản tại bcb79a4; kết luận nay nằm ở mục 2.3, 3.4, 3.5, 5.1 và bảng 'Đã xong')
- docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md:472 (câu lạc '11 giá trị')
- integration-requirements/06-module-3-api-handover.md (IR-06 §3.10, §4.3)
- 39a6243 (W-0224 ghi lý do không viết gói evidence cho W-0218/W-0219)

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: Việc rà soát tài liệu: kiểm lại bốn mục C1/C2/C5/C10 trên tài liệu và code rồi viết lại worklist kèm bằng chứng; không đổi code, test hay gate. Danh sách 1 tài liệu nằm trong khai báo.
