# SRS-06 — Assumptions & Open Questions (v1)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p02` (seed từ `plan/ivr-orther/15-open-questions.md` + open decisions từ `05-current-docs-review.md` §11 + `phase-8/24`).
✅ **p14 đã chạy** → register chính thức: [`_review/open-decisions-register.md`](_review/open-decisions-register.md) (+ [`_review/traceability-matrix.md`](_review/traceability-matrix.md), [`_review/normalization-report.md`](_review/normalization-report.md)). File này giữ làm bản gốc; register `_review` là bản chuẩn.

## 1. Assumptions (giả định đang áp dụng)

| ID | Giả định | Cơ sở | Tác động nếu sai | Ai xác nhận |
| --- | --- | --- | --- | --- |
| AS-01 ✅ | **Attempt policy = rule mới PACK-09 V1.0** (đã KHÓA): `MAX_ATTEMPT=2` cả hai program; Giờ Vàng 5′ (A1@T0, A2@T0+2:30, expire T0+5:00); 24/7 15′ (A1@T0, A2@T0+7:30, expire T0+15:00); interval = ½ window; **`T0` = lúc Core mở window/tạo task**. | **CONFIRMED — D-10** (Module 3.1, 2026-07-02) | — (đã khóa) | ✅ Đã chốt |
| AS-02 ✅ | **Scope = outbound confirmation only** (không inbound); inbound = future scope | **CONFIRMED — D-08** (Module 3, 2026-07-02) | — | ✅ Đã chốt |
| AS-03 | **Module N = Phase N** (Module 3=Commerce … 8=IVR) | CONFIRMED docx §1,§18 | (thấp) | — |
| AS-04 ✅ | Blocker ops = **SellableStatus per-line** qua sellable gate `availability/check`; **Order Core fan-out** (ops không biết `order_id`) nhúng snapshot (+`captured_at`) + **Order Core revalidate realtime** khi callback (fail-closed). **Do-not-call/opt-out = CRM, KHÔNG phải ops; source DC-01 đã có, IR-CRM-01 còn build rich fields/Core wiring. Sale-lock = recall-triggered.** | **CONFIRMED — D-06/DO-01/DO-02/DO-03/DO-CORR-1/2/3 + DC-01** (2026-07-02) | — (đã khóa; build P1 cho rich do-not-call) | ✅ Đã chốt |
| AS-05 | Deployment = Internal SIM Gateway; provider ngoài là future | docx §10, §22 | Đổi provider → đổi adapter/webhook | Owner (Q-T3) |
| AS-06 | Call recording **OFF** mặc định | docx §17; phase-8/08 | Bật → cần consent/legal/retention | Owner+Legal (Q-P1) |
| AS-07 | KEY_9 "gặp CSKH" **NOT_ENABLED** giai đoạn đầu | docx §9, M8-OD-006 | Bật → thêm route handoff | Ops Owner (Q-F2) |
| AS-08 | Slug module = `ivr-order-confirmation`; API `/v1/ivr/order-confirmation/*`; DB prefix `ivr_` | phase-8/11,12; docx | (thấp) | Owner (Q-B2) |
| AS-09 | Sinh OpenAPI 3.1 cho contract IVR | phase-8/11 | Nếu không → chỉ mô tả bảng | Architect (Q-A1) |
| AS-10 | Result taxonomy = **superset** (md + docx) với alias chuẩn hóa | 05-review C-03 | Sai naming → lệch contract | IVR Owner (OD-DR-04) |
| AS-11 | Data model giữ **md (chi tiết)** + bổ sung `ivr_raw_call_event` từ docx | 05-review C-04 | Model sai → refactor DB | Architect (OD-DR-03) |

## 2. Open questions / Owner Decisions (P0 in đậm)

### ✅ Đã KHÓA (Module 3/3.1 trả lời 2026-07-02 — xem `plan/ivr-orther/decisions-log.md`)
- ✅ **OD-DR-01 → D-10 (RESOLVED):** Attempt policy = rule mới (2 cuộc cả hai program).
- ✅ **Q-F1 → D-01/DS-01 (RESOLVED):** order_code cấp khi tạo Official Order; state chờ IVR = **`CONFIRMING`** (DS-01, không phải mock CONFIRMATION_REQUIRED); fulfillment khóa (shipment cần CONFIRMED). 🆕 **COD-only** (DS-01).
- ✅ **Q-S1 → D-02/DS-01 (RESOLVED):** Core trả `order_state` + COD gate; `is_ivr_callable` nếu có là derived convenience; transition do Core. `order_version` là target IR-SALES-OC1 (DS-04).
- ✅ **Q-S2 → D-03 (RESOLVED):** push sync `POST .../tasks` + Idempotency/Correlation; Core retry bounded.
- ✅ **Q-S3 → D-05 (RESOLVED):** dial_token qua OfficialContactResolver, token vault ở SIM adapter, TTL ≤ window, one-use/attempt.
- ✅ **Q-O1 (phần Core) → D-06 (RESOLVED):** Core revalidate blocker realtime; phần Ops còn treo QO1–QO3.
- ✅ **Q-B1 → D-08 (RESOLVED):** outbound-only; inbound = future.
- ✅ **Q-F3 → D-12 (RESOLVED):** không hardcode ngưỡng; điều kiện skip + danh sách risk-flag buộc gọi.
- ✅ **Q-D2 → D-14 (RESOLVED):** IVR audit-only, không CRM; CRM nhận event sau Core decision.
- ✅ **Q4/Q7/Q9/Q11/Q13 → D-04/D-07/D-09/D-11/D-13 (RESOLVED).**

### Còn treo — Từ p01 (05-current-docs-review §11)
- OD-DR-02 (P1): ID scheme chính (`IVR-*` vs `M8-*`) + ánh xạ chéo.
- OD-DR-03 (P1): Data-object model (md vs docx).
- OD-DR-04 (P1): Chuẩn hóa result taxonomy (superset + alias).
- OD-DR-05 (P1): Invalid phone → cancel hay admin review.
- OD-DR-06 (P1): Version canonical (docx V0.2 vs md).

### Từ phase-8/24 (residual open decisions)
- OD-08 (P0): Ngưỡng trusted customer skip + risk flags buộc trusted vẫn gọi.
- OD-09 (P0): Tiêu chí permanent invalid phone.
- OD-10 (P1): Technical retry count/backoff.
- OD-11 (P0): Mapping tín hiệu SIM thật (busy/rejected/unreachable/dropped → no-answer vs technical).
- OD-12 (P1): Recording enabled + retention.
- OD-13 (P1): Retention duration từng loại dữ liệu.
- OD-14 (P0): Production SIM gateway protocol.
- OD-15 (P1): Pilot real customer scope.
- OD-16 (P1): Notification template sau khi Core hủy/expire.

### ✅ Câu hỏi mới đã trả lời (từ đính chính Ops-Core 2026-07-02)
- ✅ **Q-C1 → DC-01 (RESOLVED):** nguồn/endpoint **do-not-call / opt-out / call-restriction** là CRM/Customer Identity `crm-ads-eligibility` PHONE_CALL. Hết chặn P0; còn **IR-CRM-01 P1** để extend response + Core wiring `call_restriction`.

### Câu hỏi tích hợp P0 CÒN TREO (không thuộc Module 3/3.1)
- Q-O1 (phần Ops): Blocker status API/SLA + snapshot freshness → **QO1–QO3** (`questions-to-ops-core.md`).
- Q-A2 (P0): Service identity allowlist → Foundation.
- Q-K1 (P0): Release gate model & sign-off → Release owner.
- Q-T1/Q-T2 (P0): SIM protocol & disposition mapping → Infra/Telephony.
- (Q-F1, Q-S1, Q-S2, Q-S3, Q-B1, Q-F3, Q-D2 → đã KHÓA ở mục trên.)

## 3. Quy tắc xử lý
- KHÔNG đóng open decision bằng suy luận; chỉ đóng khi owner sign-off (ghi `specs/decisions/`).
- Các file specs dùng giả định phải trích ID assumption tương ứng (VD: attempt policy → AS-01).
- p14 rà soát: mọi assumption phải có trạng thái (open/confirmed) và mọi P0 open decision phải có owner + hạn.
