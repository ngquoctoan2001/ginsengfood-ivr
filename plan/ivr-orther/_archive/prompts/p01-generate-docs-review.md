# P01 — Generate Docs Review

## Tên nhiệm vụ
Sinh tài liệu review docs hiện có và mapping docs cũ → specs mới cho IVR.

## Bối cảnh
Module IVR (working name `ivr-orther`; tên đúng trong docs: **IVR Order Confirmation / PACK-09 / phase-8**) đã có một bộ tài liệu baseline rất đầy đủ trong `docs/documents/4. phase/phase-8/` (files 00–26), cùng `PACK-09` và `TECH-09`. Giai đoạn specs sẽ KHÔNG viết lại từ đầu mà **chưng cất (distill) + chuẩn hóa** baseline này thành source-of-truth mới trong `specs/srs`.

## Input cần đọc
- `plan/ivr-orther/01-reading-inventory.md`
- `plan/ivr-orther/03-ivr-related-findings.md`
- `plan/ivr-orther/07-source-of-truth-build-plan.md`
- Toàn bộ `docs/documents/4. phase/phase-8/` (00–26)
- `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`
- `docs/documents/3. tech/10-TECH-09-...IVR-ORDER-CONFIRMATION...md`
- `docs/documents/1. master/01-MASTER-00-INDEX-REGISTRY.md`, `02-MASTER-01-SOURCE-OF-TRUTH.md`

## Output cần tạo
- `specs/srs/05-current-docs-review.md` — review từng nhóm docs, đánh giá độ chín (SRS_BASELINE / SDS_BASELINE / analysis), độ tin cậy, và điểm mâu thuẫn.
- Bảng **mapping docs cũ → file specs mới** (mỗi phase-8 doc ánh xạ tới file specs nào sẽ sinh).
- Bảng **inventory tài liệu final** (bản chốt của reading-inventory, có trạng thái đã đọc chi tiết).
- Danh sách các **file được tham chiếu nhưng không tồn tại** (ví dụ `docs/source-map.md`, `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md`) → ghi `GAP`.

## Quy tắc
- Mỗi dòng mapping phải trích path nguồn.
- Đánh dấu `CONFIRMED/ASSUMPTION/GAP` cho mỗi kết luận.
- Không đánh giá "production ready".
- Giữ nguyên semantic contract của baseline; chỉ chuẩn hóa cấu trúc.

## Checklist hoàn thành
- [ ] Mọi file phase-8 (00–26) đã có dòng trong mapping.
- [ ] PACK-09, TECH-09 đã được review.
- [ ] Đã liệt kê file tham chiếu bị thiếu.
- [ ] Đã nêu mọi mâu thuẫn giữa các doc (nếu có).
- [ ] `specs/srs/05-current-docs-review.md` tồn tại và có nhãn trạng thái.

## Điều cấm
- KHÔNG tạo file specs khác ngoài `05-current-docs-review.md` và các bảng mapping/inventory.
- KHÔNG xóa/sửa docs cũ.
- KHÔNG kết luận docs cũ là "sai" nếu chỉ là chưa rõ — dùng `NEED_CONFIRMATION`.

## Báo cáo cuối (sau khi chạy)
1. Số docs đã review.
2. Số mapping đã lập.
3. Số file tham chiếu bị thiếu.
4. Danh sách mâu thuẫn phát hiện.
5. Docs nào đủ chín để làm source, docs nào cần bổ sung.
