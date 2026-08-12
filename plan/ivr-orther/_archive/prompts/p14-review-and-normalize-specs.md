# P14 — Review & Normalize Specs

## Tên nhiệm vụ
Review, chuẩn hóa, phát hiện mâu thuẫn, kiểm tra thiếu sót trong toàn bộ specs.

## Bối cảnh
Đây là prompt "gatekeeper" chạy sau mỗi vòng sinh specs và lặp lại. Mục tiêu: đảm bảo specs nhất quán, traceable, không mâu thuẫn với baseline docs và với nhau, tuân mô hình evidence/gate.

## Input cần đọc
- Toàn bộ `specs/srs/*`, `integration-requirements/*`, `seed/*`
- `plan/ivr-orther/07-source-of-truth-build-plan.md` (tiêu chuẩn đạt), `10-integration-gap-analysis.md`, `14-risk-register.md`, `15-open-questions.md`
- `docs/documents/4. phase/phase-8/25-MA TRẬN TRUY VẾT SRS IVR.md`
- `docs/documents/00-AI-EVALUATION-DEV-READINESS.md`

## Output cần tạo
- `specs/srs/_review/normalization-report.md` (mâu thuẫn, trùng lặp, thuật ngữ lệch, mã ID trùng)
- `specs/srs/_review/traceability-matrix.md` (requirement → source doc → spec file → test/evidence)
- `specs/srs/_review/open-decisions-register.md` (gom `Owner Decision Required` còn lại)
- Cập nhật `specs/srs/06-assumptions-and-open-questions.md`

## Quy tắc
- Kiểm nhất quán thuật ngữ với glossary.
- Kiểm mọi FR/P0 có test + evidence.
- Kiểm không có endpoint/flow cho IVR update order state.
- Kiểm tension phase-3.1 (IVR trước order_code) vs phase-8 (IVR sau Official Order) đã được nêu và có hướng xử lý.
- Đánh giá dev-readiness theo `00-AI-EVALUATION-DEV-READINESS.md` (KHÔNG tuyên bố production-ready).

## Checklist hoàn thành
- [ ] Traceability matrix đầy đủ.
- [ ] Danh sách mâu thuẫn + hướng xử lý.
- [ ] Open decisions register cập nhật.
- [ ] Không P0 nào thiếu test/evidence.

## Điều cấm
- KHÔNG tự "giải quyết" mâu thuẫn nghiệp vụ bằng suy diễn — ghi `Owner Decision Required`.
- KHÔNG tuyên bố specs đã "final/production-ready".

## Báo cáo cuối
1. Số mâu thuẫn phát hiện + đã xử lý.
2. Số open decisions còn lại.
3. Đánh giá dev-readiness (đủ/thiếu gì).
4. Vòng lặp tiếp theo nên chạy prompt nào.
