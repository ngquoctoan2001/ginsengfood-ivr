# 07 — Source-of-Truth Build Plan  ⭐ (file quan trọng nhất)

Kế hoạch xây dựng bộ `specs/srs` chính thức làm **source-of-truth mới** cho module IVR.

## 1. Vì sao cần source-of-truth mới?

- CONFIRMED: Baseline phase-8 rất chín nhưng đang ở dạng **văn bản phase-based, tiếng Việt, rải theo 27 file** (SRS 00–09, SDS 10–20, gap/trace 24–26) + 1 `.docx` version V0.2. Nó là *nguồn tham chiếu*, nhưng không phải cấu trúc `specs/srs` chuẩn để dev handoff.
- CONFIRMED: Governance hệ thống yêu cầu mỗi module có source-of-truth rõ ràng, traceable, với evidence/smoke/gate (MASTER-01, MASTER-05). Cần một bộ specs được **chuẩn hóa cấu trúc, khử trùng lặp, đóng open decisions, và gắn traceability ID + evidence** để đưa vào dev.
- `RISK`: Nếu để nguyên phase-8 làm "source", nhiều mâu thuẫn/nhầm version (V0.2 docx vs md) và open decisions sẽ trôi vào code.

## 2. Docs cũ được dùng như reference thế nào?

- Nguyên tắc: **Distill, không rewrite tùy tiện.** Giữ nguyên semantic contract, mã ID (`IVRxx-FR-xxx`, `IVRxx-P0-xxx`), tên bảng `ivr_*`, endpoint `/v1/ivr/order-confirmation/*`, contract `IvrConfirmationTaskV1` / `IvrConfirmationResultCallbackV1`.
- Mỗi file specs mới phải có bảng **"Nguồn tham chiếu"** trỏ về path phase-8/PACK/TECH/MASTER tương ứng (như phase-8 docs đang làm).
- `docs/documents/*` **không bị xóa/sửa**. Nếu specs mới khác baseline, ghi vào `specs/decisions/` (ADR) + `specs/srs/05-current-docs-review.md`.
- Đối chiếu `MODULE_8_...V0.2_CLEAN_FINAL.docx` với md; nếu lệch → ghi decision, chọn version chuẩn (`NEED_CONFIRMATION` owner).

## 3. Thứ tự sinh specs (tóm tắt; chi tiết [09](09-specs-generation-sequence.md))

docs-review (p01) → context/scope/glossary (p02) → functional (p03) → workflows (p04) → API (p05) → data-mapping (p06) → database (p07) → architecture (p08) → integration-requirements (p09) → seed (p10) → testing (p11) → UI (p12) → review/normalize (p14) → (khi ổn định) prompt library (p13).

## 4. File nào sinh trước / phụ thuộc gì

| Sinh trước | Là input cho |
| --- | --- |
| docs-review | tất cả |
| context/scope/glossary | functional, workflows, data |
| functional | workflows, API, data, testing, UI |
| workflows | API, database (state machine), testing |
| API + data-mapping | database, architecture, integration-requirements, UI |
| database | seed |
| tất cả specs | testing, review/normalize |
| specs ổn định | prompt library (p13) |

## 5. Tiêu chuẩn đánh giá specs "đạt yêu cầu"

Một file specs đạt khi:
1. Có bảng "Nguồn tham chiếu" (path docs) cho mọi khẳng định chính.
2. Dùng nhãn `CONFIRMED/ASSUMPTION/NEED_CONFIRMATION/TODO/GAP/RISK`.
3. Mọi requirement có ID traceable + acceptance hint + (khi tới p11) test/evidence.
4. Không có quyết định nghiệp vụ do implementer tự suy diễn — chỗ chưa rõ ghi `Owner Decision Required`.
5. Tôn trọng P0 boundary: IVR không update order state, không Quote/Cart/Draft, không payment, không tự notification, tách technical≠no-answer, không override blocker.
6. Đạt chuẩn dev-readiness theo `docs/documents/00-AI-EVALUATION-DEV-READINESS.md` (KHÔNG tuyên bố production-ready).

## 6. Quy tắc xử lý mâu thuẫn giữa docs cũ và specs mới

- Ưu tiên: **MASTER (governance) > PACK/TECH (nguồn pack/tech) > phase-8 md > .docx V0.2 > suy luận**. Nguồn: MASTER-01 (source-of-truth hierarchy).
- Nếu phase-8 md và .docx lệch → ghi vào `specs/decisions/`, chọn theo owner (`NEED_CONFIRMATION`).
- Nếu phase-3.1 (IVR trước order_code) mâu thuẫn phase-8 (IVR sau Official Order) → **KHÔNG tự chọn**; ghi `Owner Decision Required`, chờ Order Core/Sales owner. Đây là mâu thuẫn P0.
- Mọi lần override baseline phải tạo 1 ADR trong `specs/decisions/`.

## 7. Quy tắc đánh dấu assumptions / open questions

- Assumptions gom ở `specs/srs/06-assumptions-and-open-questions.md`, mỗi assumption có: nội dung, cơ sở, tác động nếu sai, ai xác nhận.
- Open decisions (từ phase-8/24, /25) gom ở `specs/srs/_review/open-decisions-register.md` (do p14 duy trì).
- Không "đóng" một open decision bằng suy luận; chỉ đóng khi có owner sign-off (ghi trong `specs/decisions/`).

## 8. Quy tắc review trước khi triển khai code

- Chạy p14 (review/normalize) sau mỗi vòng; sinh traceability-matrix + normalization-report.
- Điều kiện để chuyển sang code (p13 prompt library rồi mới code): (a) không P0 nào thiếu test/evidence; (b) các Owner Decision P0 (thứ tự order_code, order state set, SIM protocol, recording, retention) đã được trả lời tối thiểu; (c) integration-requirements đã gửi và có phản hồi từ sales/ops; (d) release gate model rõ (`REAL_CUSTOMER_CALL_ALLOWED=NO` cho tới khi pass).
- Code chỉ bắt đầu sau khi plan + specs được owner duyệt và prompt library (p13) sẵn sàng — ngoài phạm vi giai đoạn này.

## 9. Nguyên tắc chống "scope creep"

- Giữ scope = **outbound order-confirmation** theo phase-8. Mọi tính năng inbound (lookup, order-by-phone, gặp nhân viên, tư vấn) chỉ vào specs sau khi có `Owner Decision Required` mở scope + tài liệu nguồn. Ghi rõ ở `specs/srs/01-context-and-scope.md`.
