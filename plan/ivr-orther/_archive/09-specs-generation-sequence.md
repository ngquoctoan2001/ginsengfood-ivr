# 09 — Specs Generation Sequence

Thứ tự sinh specs sau khi plan được duyệt. Mỗi bước: input → output → prompt → dependency → checklist → rủi ro.

## Bước 1 — Docs review
- Input: toàn bộ phase-8/PACK-09/TECH-09/MASTER; plan/01,03,07.
- Output: `specs/srs/05-current-docs-review.md` + inventory final + mapping docs→specs.
- Prompt: **p01**. Dependency: plan duyệt.
- Checklist: mọi phase-8 doc có mapping; file tham chiếu thiếu được liệt kê.
- Rủi ro: bỏ sót lệch version .docx vs md.

## Bước 2 — Context / Scope / Glossary
- Input: docs-review; phase-8/00,01,02,03.
- Output: `specs/srs/01,02,03,04,06`.
- Prompt: **p02**. Dependency: bước 1.
- Checklist: scope IN/OUT khớp phase-8/00; glossary ≥20; actors phân loại.
- Rủi ro: mở nhầm scope inbound.

## Bước 3 — Functional requirements
- Input: context/scope; phase-8/00,03,04,05,06,07,13,14,22,23; TECH-09.
- Output: `specs/srs/functional/*`.
- Prompt: **p03**. Dependency: bước 2.
- Checklist: giữ mã P0; traceability FR→nguồn; residual decisions liệt kê.
- Rủi ro: mất mã ID gốc; suy diễn quyết định treo.

## Bước 4 — Workflows
- Input: functional; phase-8/14,05,07,23.
- Output: `specs/srs/workflows/*` (+ state machines).
- Prompt: **p04**. Dependency: bước 3.
- Checklist: 8 luồng + state machines; race Sale Lock rõ; Golden Hour/24-7 đúng số.
- Rủi ro: state machine lệch phase-8/07,/12.

## Bước 5 — API specs
- Input: functional, workflows; phase-8/11,04,06,07; TECH-01; plan/11,12.
- Output: `specs/srs/api/*` (+ openapi nếu duyệt).
- Prompt: **p05**. Dependency: bước 3,4.
- Checklist: không endpoint update order; error map đủ; idempotency rõ; external needs trỏ integration.
- Rủi ro: bịa API sales/ops đã tồn tại.

## Bước 6 — Data mapping
- Input: functional, API; MASTER-01,03; phase-8/02,04,07,08,12; plan/04,10,13.
- Output: `specs/srs/data/*`.
- Prompt: **p06**. Dependency: bước 5.
- Checklist: mọi trường task có ownership; PII policy; missing data gắn priority.
- Rủi ro: gán IVR làm owner order/payment.

## Bước 7 — Database design
- Input: data, functional, workflows/state; phase-8/12,13; MASTER-03; TECH-01.
- Output: `specs/srs/database/*`.
- Prompt: **p07**. Dependency: bước 6.
- Checklist: ERD 9–10 bảng; constraint attempt/program; index scheduler+idempotency; retention/PII; migration gates.
- Rủi ro: cột lưu order state như source-of-truth; cột raw phone bắt buộc.

## Bước 8 — Architecture
- Input: API, data, workflows; phase-8/10,13,16,17,18; MASTER-04; TECH-00.
- Output: `specs/srs/architecture/*` (+ `modules/`, `non-functional/`).
- Prompt: **p08**. Dependency: bước 5,6.
- Checklist: context/boundary/integration/deploy/resilience/observability; failure matrix khớp phase-8/02.
- Rủi ro: chọn cloud provider mặc định (future decision).

## Bước 9 — Integration requirements
- Input: API, data; plan/10,11,12; phase-8/17,02; phase-3.1/07.
- Output: `integration-requirements/*` (root).
- Prompt: **p09**. Dependency: bước 5,6.
- Checklist: sales/ops/telephony/shared có file; mỗi need có priority/owner/mock; tension order_code nêu.
- Rủi ro: thiết kế thay hệ sales/ops.

## Bước 10 — Seed data
- Input: database, data, workflows; plan/13; phase-8/09,12.
- Output: `seed/*` (root).
- Prompt: **p10**. Dependency: bước 7.
- Checklist: đủ domain + tình huống; README gỡ mock; không PII thật.
- Rủi ro: seed vào production; bật recording/real SIM.

## Bước 11 — Testing specs
- Input: toàn bộ specs; phase-8/09,19; MASTER-05; TECH-10.
- Output: `specs/srs/testing/*`.
- Prompt: **p11**. Dependency: bước 3–9.
- Checklist: 7 loại test + acceptance + smoke matrix; P0 negative đủ; release gate rõ.
- Rủi ro: test gọi khách thật; tuyên bố production-ready.

## Bước 12 — UI specs
- Input: functional (admin), API admin; phase-8/08,11; TECH-01.
- Output: `specs/srs/ui/*`.
- Prompt: **p12**. Dependency: bước 3,5.
- Checklist: các màn theo brief; privacy-safe; permission map.
- Rủi ro: UI cho phép force order/bypass blocker.

## Bước 13 — Review / Normalize (chạy lặp)
- Input: toàn bộ specs; plan/07,10,14,15; phase-8/25; AI-EVALUATION.
- Output: `specs/srs/_review/*`; cập nhật `06-assumptions`.
- Prompt: **p14**. Dependency: sau mỗi vòng.
- Checklist: traceability matrix; mâu thuẫn + hướng xử lý; open decisions cập nhật.
- Rủi ro: tự "đóng" quyết định nghiệp vụ.

## Bước 14 — Final prompt library
- Input: specs ổn định; TECH-13/11/12; plan/16.
- Output: `prompt/*` (root).
- Prompt: **p13**. Dependency: bước 1–13 ổn định.
- Checklist: mỗi prompt có traceability + evidence expected.
- Rủi ro: chạy sớm khi specs chưa ổn định.

## Ghi chú
- p13 (prompt library) đứng cuối dù đánh số nhỏ — chỉ chạy khi specs qua p14 ổn định.
- Sau p14 nếu còn open decision P0 → dừng, đưa lên owner trước khi sang code.
