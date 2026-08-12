# 15 — Open Questions

Gom câu hỏi cần xác nhận. Mỗi câu: câu hỏi · vì sao cần · ai trả lời · ảnh hưởng nếu chưa trả lời · có thể giả định tạm không.

> ✅ **Cập nhật 2026-07-02 — Module 3 & 3.1 đã trả lời** (xem [decisions-log.md](../decisions-log.md) D-01..D-14).
> **ĐÃ KHÓA:** Q-B1→D-08 · Q-F1→D-01 · Q-S1→D-02 · Q-S2→D-03 · Q-S3→D-05 · Q-O1(phần Core)→D-06 · Q-F3→D-12 · Q-D2→D-14 · (+ Q4/Q7/Q9/Q11/Q13→D-04/07/09/11/13).
> **✅ Ops-Core cũng đã trả lời (DO-01..DO-09):** blocker = sellable gate; Q-O1 phần Ops → DO-01; QO4 event → DO-04.
> **🆕 PHÁT SINH (P0):** **Q-C1** — do-not-call/opt-out là **CRM/business-platform** (không phải ops, theo DO-CORR-2) → cần hỏi Module 3.1 (CRM).
> **✅ Foundation + Telephony (IVR-owned) đã chốt (DF-*/DT-*):** Q-A1→DF-02 (OpenAPI), Q-A2→DF-01/DF-06 (RBAC/allowlist), Q-K1→DF-03 (release gate, sign-off owner), Q-T2→DT-02 (disposition), Q-P1→DT-05 (recording OFF).
> **CÒN TREO (thật sự):** **Q-C1** (CRM do-not-call — P0) · **mua SIM**: Q-T1/DT-01 (protocol), DT-04 (số SIM), Q-T6/DT-06 (caller-ID) · **Legal**: Q-D1/DF-07 (retention) · Q-F2 (KEY_9) · OD-DR-02..06 (naming/model, không chặn) · OD-10 (technical retry backoff).

## A. Business / Scope

- **Q-B1 (P0):** IVR chỉ làm **outbound order-confirmation** (theo phase-8), hay có mở **inbound** (khách gọi hotline tra cứu/đặt hàng/gặp nhân viên/tư vấn) như brief giả định?
  - Vì sao: quyết định toàn bộ scope, actors, API sales/ops, DB.
  - Ai: Product Owner / chủ đầu tư.
  - Ảnh hưởng nếu chưa: không chốt được scope; nguy cơ scope creep.
  - Giả định tạm: **CÓ** — giữ outbound-only theo docs; inbound đánh `NEED_CONFIRMATION`.

- **Q-B2 (P1):** Tên module chính thức để đặt slug specs/API/DB: `ivr-order-confirmation` (theo docs) đúng không?
  - Ai: Owner. Giả định tạm: dùng `ivr-order-confirmation`.

## B. IVR flow

- **Q-F1 (P0):** Thứ tự **IVR ↔ order_code**: IVR chạy **trước** khi sinh order_code (phase-3.1/07) hay **sau** khi có Official Order (phase-8/00)? Là **một** cơ chế hay **hai** (risk-decision ở sales vs confirmation-call ở phase-8)?
  - Vì sao: định hình toàn bộ contract task/callback & state machine.
  - Ai: Order Core owner + Sales owner. Ảnh hưởng: thiết kế sai phải làm lại. Giả định tạm: **Không** (không tự chọn).

- **Q-F2 (P1):** Có bật phím "gặp nhân viên" (`IVR_CUSTOMER_NEEDS_SUPPORT`) không, route tới đâu?
  - Ai: Ops/CSKH owner. Giả định tạm: tắt (theo baseline "future key").

- **Q-F3 (P1):** Ngưỡng trusted customer để skip IVR + risk flags buộc trusted vẫn phải gọi?
  - Ai: Risk/Customer owner. Giả định tạm: không hardcode; require IVR nếu thiếu.

## C. Sales Platform

- **Q-S1 (P0):** State machine order: tên state, tập **IVR-callable**, transition sau mỗi result type?
  - Ai: Order Core owner. Ảnh hưởng: callback/revalidate.
- **Q-S2 (P0):** Order Core **push task** hay IVR **poll**? Transport (REST/command/queue)? Ai giữ retry?
  - Ai: Order Core owner.
- **Q-S3 (P0):** Official contact projection & **dial token** (TTL, mapping token→số) cấp thế nào?
  - Ai: Customer/Commerce owner.
- **Q-S4 (P1):** `IVRRequiredDecision` là API GET hay event? Nó tạo task trực tiếp hay chỉ set cờ?
  - Ai: Sales (3.1) owner.
- **Q-S5 (P1):** Golden Hour quota release khi IVR fail/timeout: qua callback signal hay API riêng?
  - Ai: Sales (3.1) owner.
- **🆕 Q-C1 (P0):** Nguồn/endpoint **do-not-call / opt-out / call-restriction** (blocker thương mại) — CRM/business-platform cấp thế nào? Snapshot trong task hay Order Core query? Format?
  - Vì sao: Ops-Core xác nhận đây KHÔNG thuộc ops (DO-CORR-2); IVR/Core phải chặn khách opt-out trước khi gọi. Ảnh hưởng: gọi nhầm khách từ chối = vi phạm compliance.
  - Ai: CRM / business-platform (Module 3.1) owner. Giả định tạm: task kèm cờ `call_restriction`/`opt_out` do Order Core hợp nhất từ CRM.

## D. Ops-Core

- **Q-O1 (P0):** Blocker (sale-lock/recall/suppression): IVR nhận **snapshot qua task** hay gọi ops **realtime**? Ai gọi realtime lúc revalidate (Core hay IVR)? SLA?
  - Ai: Ops + Order Core owner.
- **Q-O2 (P1):** Availability cho revalidate lấy từ ops-core hay commerce?
  - Ai: Ops/Commerce owner.
- **Q-O3 (P1):** Có event push khi lock/recall activate giữa cuộc gọi (để hold) không?
  - Ai: Ops owner.

## E. Telephony / Provider

- **Q-T1 (P0):** Production SIM Gateway hardware/API protocol?
  - Ai: IVR Infra + Owner. Ảnh hưởng: adapter design, gọi thật.
- **Q-T2 (P0):** Mapping tín hiệu SIM thật (busy/rejected/unreachable/dropped) → no-answer vs technical?
  - Ai: IVR Infra + Owner. Ảnh hưởng: tránh FAIL technical≠no-answer.
- **Q-T3 (P2):** Có dùng provider ngoài (cloud IVR/SIP/brandname) không, hay giữ internal SIM?
  - Ai: Owner. Giả định tạm: internal SIM (mặc định).

## F. Security / Privacy

- **Q-P1 (P1):** Bật call recording không? Consent/legal basis?
  - Ai: Owner + Legal. Giả định tạm: OFF.
- **Q-P2 (P1):** Trường PII nào được hiển thị admin UI / đọc trong script (ngoài masked)?
  - Ai: Privacy owner. Giả định tạm: chỉ masked, không full.

## G. Data

- **Q-D1 (P1):** Retention duration từng loại: call log, DTMF evidence, recording, admin audit, raw phone/token?
  - Ai: Owner. Giả định tạm: TTL ngắn nhất; audit theo foundation.
- **Q-D2 (P1):** IVR có được ghi call-note vào hồ sơ khách (CRM) không, hay chỉ ghi audit nội bộ?
  - Ai: CRM owner. Giả định tạm: chỉ audit nội bộ (phase-8 cấm CRM đại trà).

## H. API

- **Q-A1 (P0):** Có duyệt sinh OpenAPI 3.1 `ivr-order-confirmation.v1.yaml` trong `specs/srs/api/openapi/` không?
  - Ai: Owner/Architect. Giả định tạm: có (theo phase-8/11).
- **Q-A2 (P0):** Service identity allowlist (ai được tạo task / gọi admin) cấu hình ở đâu?
  - Ai: Foundation owner.

## I. Database

- **Q-DB1 (P1):** Repo dùng RDBMS nào & convention (đặt tên, migration tool)? Có outbox chuẩn để tái dùng không?
  - Ai: Foundation/Architect. Giả định tạm: theo convention repo; không tự tạo broker mới.

## J. UI / Admin

- **Q-U1 (P1):** Admin/Ops console dùng nền tảng nào (tái dùng admin hiện có?)? RBAC permission `IVR_*` tạo ở đâu?
  - Ai: Ops/Foundation owner.

## K. Testing / Deployment

- **Q-K1 (P0):** Release gate model & điều kiện mở `REAL_CUSTOMER_CALL_ALLOWED`; ai sign-off?
  - Ai: Release owner. Giả định tạm: theo phase-8/09 (evidence+smoke+owner sign-off).
- **Q-K2 (P1):** Có môi trường test tích hợp với sales/ops không, hay chỉ mock?
  - Ai: Platform owner. Giả định tạm: mock trước, tích hợp sau.
- **Q-K3 (P1):** Pilot real customer scope (khi nào, bao nhiêu, tiêu chí)?
  - Ai: Owner.

## Tổng hợp câu hỏi P0 cần trả lời trước khi sinh specs sâu
- ✅ **ĐÃ KHÓA (Module 3/3.1):** Q-B1, Q-F1, Q-S1, Q-S2, Q-S3, Q-O1(phần Core).
- ⏳ **CÒN TREO (P0):** Q-O1(phần Ops = QO1–QO3), Q-T1, Q-T2, Q-A1, Q-A2, Q-K1.
