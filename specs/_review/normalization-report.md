# REVIEW — Normalization Report

## Round 5 — Target V1 realignment (2026-08-12)

Source/code review superseded several earlier conclusions. Active plan/spec/prompt now use `docs/contracts/target-v1-closure-pack/README.md` cùng hai OpenAPI hiện hành:

- scope is Golden Hour ONLINE plus 24/7 COD, not global COD-only;
- D-10 timings are configurable candidate values for MOCK/LAB, not production-locked;
- target callback is Sales-owned generic `/api/v1/internal/orders/{orderId}/ivr-result-callbacks`; Golden Hour endpoint is compatibility-only;
- speech summary with items/total/short area is a P0 upstream dependency;
- no-answer waits for Sales timeout; V1 notification is disabled;
- dev is mock-first, then one real SIM lab, with 32 eSIM target later;
- target auth and several Sales contracts remain open.

Earlier Round 1–4 text below is retained as historical evidence and is **not authoritative where it conflicts with Round 5**.

Trạng thái: `REVIEW` · Sinh bởi: `p14-review-and-normalize-specs.md` (prompt sinh tài liệu đã nghỉ hưu 2026-09-04; còn trong git history)
Phạm vi: rà soát toàn bộ `specs/srs/*` + `integration-requirements/*` + `seed/*`. Chuẩn đánh giá: `00-AI-EVALUATION-DEV-READINESS.md`, `MASTER-05`.

## 1. Nhất quán attempt policy (D-10) — ✅ PASS
Kiểm mọi nơi trích attempt policy đã về **rule mới** (2 cuộc cả hai; GH 5′/window 300/spacing 150; 24-7 15′/window 900/spacing 450; `T0`=lúc Core mở window):
`functional/03`, `functional/00-index`, `workflows/00-index`, `workflows/03`, `glossary`, `06-assumptions` (AS-01✅), `05-current-docs-review` (C-01/C-02✅), `api/05`, `database/02` (CHECK), `seed/ivr-tasks`, `testing/*`, `architecture/07`. → **không còn 2/10 & 3/15** (trừ dòng ghi lịch sử conflict đã đánh dấu superseded).

## 2. Order state là "đục" (D-02) — ✅ PASS
Không file nào cho IVR sở hữu/suy diễn/ghi order state. Current snapshot dùng `order_state` đục + COD gate; `is_ivr_callable` nếu có chỉ là derived convenience; `order_version` là target/nullable IR-SALES-OC1. Xác nhận **không endpoint/flow nào IVR update order state** (api/00-02, database/00, architecture/02).

## 3. Blocker model (DO-*) — ✅ PASS
Nhất quán: sellable gate per-line, Order Core fan-out (ops không biết order_id), IVR không gọi ops trực tiếp, do-not-call = CRM (không ops), sale-lock=recall-triggered. Kiểm `functional/02`, `data/03`, `api/05/06/08`, `architecture/03/05`, `integration-requirements/02`, `seed/inventory`.

## 4. Mâu thuẫn C-01..C-08 (từ 05-current-docs-review) — trạng thái
| # | Trạng thái |
| --- | --- |
| C-01 attempt policy | ✅ RESOLVED D-10 |
| C-02 DB 24/7 max | ✅ RESOLVED D-10 (max=2) |
| C-03 result taxonomy naming | ✅ Chuẩn hóa superset (functional/05, api/06, database/03) — OD-DR-04 khép |
| C-04 data model | ✅ Giữ md chi tiết + thêm `ivr_raw_call_event` (OD-DR-03) |
| C-05 ID scheme | ⏳ OD-DR-02 mở — dùng kép + bảng ánh xạ (testing/09) |
| C-06 SIM cooldown | ✅ 5s (docx) |
| C-07 capacity hệ số | ✅ AVG35/cycle50 (docx) |
| C-08 invalid phone | ✅ DT-02 (INVALID_PHONE_FINAL) + admin review policy |

## 5. Thuật ngữ (glossary) — ✅ PASS (lưu ý nhỏ)
Thuật ngữ dùng nhất quán với `04-glossary`. Đã bổ sung "Sellable gate/SellableStatus", "T0", làm rõ "Suppression" (2 nghĩa) và "Sale Lock" (recall-triggered). `TODO nhẹ`: thống nhất viết `24/7` vs `24-7` (hiện dùng lẫn) — cosmetic, không chặn.

## 6. Tension phase-3.1 vs phase-8 (order_code) — ✅ RESOLVED
D-01/DS-01: order_code khi tạo Official Order; state chờ IVR = **`CONFIRMING`** (COD-only, DS-01); fulfillment gated (shipment cần CONFIRMED). Nêu ở `01-context`, `data/*`, `integration-requirements/00`, `10-gap-analysis`.

## 7. Mọi FR/P0 có test + evidence? — ✅ PASS (còn treo integration)
- 10 P0 (00-index testing) đều có test + smoke (09) + evidence.
- ✅ **DG-03 đã trả DS-01..05**; integration/contract test đầy đủ đã tách target/deferred cho OC1/OC2/OC3.

## 8. Trùng lặp / lỗ hổng
- Không phát hiện requirement mâu thuẫn nội bộ.
- `non-functional/` và `modules/` (đề xuất ở structure) chưa tách file riêng — NFR nằm trong `architecture/04-06`, module boundaries trong `architecture/02`. Chấp nhận; nếu muốn tách, làm ở vòng sau (không chặn).

## 9. Dev-readiness (00-AI-EVALUATION) — đánh giá
- Source path rõ, requirement rõ, owner/boundary rõ, privacy rõ, evidence/fail-safe rõ, test expected rõ → **đủ để dev-review** theo chuẩn.
- **KHÔNG** tuyên bố production-ready. Còn open decisions (§open-decisions-register) chặn gọi khách thật.

## Kết luận (Round 1)
Specs **nhất quán và đủ chín để dev-review + bắt đầu implement (dry-run/mock)**. Các điểm mở đều là owner-decision/procurement/legal, đã gom ở `open-decisions-register.md`, không phải mâu thuẫn thiết kế.

---

## Round 2 (2026-07-02, sau p13 + linter api/06)
Phạm vi mới kiểm: `prompt/*` (dev library) và `api/06 §1b/§1c` (stable code catalog).

### R2.1 Stable code catalog (api/06 §1c) — ✅ PASS + lưu ý
- 15 `code` ổn định (`IVR_*`) + response model 200-decision vs 4xx-envelope (§1b) **nhất quán** với `testing/04` (CT-ERR-01..04 đã tham chiếu §1b/§1c) và intake taxonomy (`api/02`, `database/03`).
- ✅ Đã siết OpenAPI `ErrorEnvelope.code` bằng schema `ErrorCode` enum 15 mã ổn định từ `api/06 §1c`; không còn là `string` tự do.

### R2.2 Dev prompt library (`prompt/*`) — ✅ PASS
- 10 prompt (foundation + M8.2A–H) — mỗi prompt có requirement ID + source spec path + test + evidence + forbidden. Kiểm chéo: test IDs (IT-*/UT-*/CT-*/SEC-*/PT-*/E2E-*) và smoke (M8-P0-*/SMK-*) prompt trích **đều tồn tại** trong `testing/*`.
- Governance nhất quán: mọi prompt giữ `REAL_CUSTOMER_CALL_ALLOWED=NO`, no order-update (D-02), no bypass blocker, foundation RBAC/audit/idempotency/evidence.
- `Lưu ý ID scheme (OD-DR-02 vẫn mở)`: `prompt/00-index` (câu chữ generator) nhắc dạng `IVR-xx-FR-xxx`, nhưng các prompt thực (02–09) dùng đúng ID thật `FR-IVR-<domain>-nnn`. Không lệch trong nội dung; chỉ là dual-scheme chưa chốt (OD-DR-02).

### R2.3 Lỗi đã sửa (round 2)
- ✅ `architecture/01-system-context` diagram: sửa cạnh sai `OC -- IVRRequired --> OC` (self-loop) → `CRM -- IVRRequired event order.ivr_required_decisioned (D-09) --> OC` (đúng: Sales/CRM 3.1 phát event, Order Core consume — D-09).

### R2.4 Không phát sinh mâu thuẫn mới
- Không thêm endpoint/flow nào cho IVR update order state.
- Attempt policy D-10, blocker model DO-*, PII D-05 vẫn nhất quán sau khi thêm `prompt/`.

**Kết luận Round 2:** `prompt/` + `_review/` khép vòng specs; toàn bộ p01–p14 nhất quán. Vẫn **KHÔNG** production-ready — tại thời điểm Round 2 còn chờ Q-C1/DG-03; Round 3/register mới đã resolve hai mục này, P0 còn mua SIM + DF-03.

---

## Round 3 (2026-07-03, sau khi Order Core trả DG-03 → DS-01..05)
Phạm vi: đồng bộ `testing/*` + `workflows/*` theo thực tế Core (DS-01..05), tách rõ **target vs implemented**.

### R3.1 COD-only gate (DS-01) — ✅ đã phủ test
- Thêm `IT-05a` (state ≠ CONFIRMING → reject) và `IT-05b` (payment ≠ COD → reject `STATE_NOT_CALLABLE`) ở `testing/03`. FR gốc `FR-IVR-INTAKE-003` đã yêu cầu `CONFIRMING`+`COD`. → **IVR chỉ đơn COD** có test.

### R3.2 Race-guard order_version = target/deferred (DS-04) — ✅ đánh dấu
- `testing/03 IT-08` và `testing/04 CT-CB-02`: đổi sang **target/deferred** (Core chưa expose `order_version`, chưa check `order_version_seen_by_ivr`). Bảo vệ stale **đang chạy** = state+COD+sellable recheck.
- `workflows/06`: thêm caveat + 2 dòng matrix (state rời CONFIRMING → 422; mất COD → 422); nhánh version-mismatch = target (IR-SALES-OC1).

### R3.3 Callback codes = target (DS-03) — ✅ đánh dấu
- `testing/04 §3` + `workflows/09` (đã có) + `api/05` (đã có): Core hiện **`200/422`**, bộ `CALLBACK_*` là target (IR-SALES-OC2).

### R3.4 No-answer KHÔNG auto-transition (DS-02) — ✅ đính chính
- `workflows/03`: caveat + note diagram — no-answer-final **không** hủy order; order chờ `timeout→EXPIRED`. `recommended=..._CANCEL_NO_ANSWER` = advisory; explicit transition = target (**IR-SALES-OC3**, mới thêm vào register).

### R3.5 Trusted-skip DISABLED (DC-06) — ✅ đính chính
- `workflows/07`: caveat — `CustomerTrustResolver` chưa build → **default require-IVR** (fail-safe D-12); flow skip là target (P3.2), không chặn gọi thật.

### R3.6 Không phát sinh mâu thuẫn mới
- Fail-gate `testing/08` (IVR không xử lý payment) vẫn đúng: IVR chỉ **đọc** cờ COD snapshot làm intake-gate, không xử lý payment.
- Callback contract đã tách current/target: current không bắt buộc `order_version_seen_by_ivr`; target variant (IR-SALES-OC1) mới bắt buộc field này.

**Kết luận Round 3:** testing + workflows đã khớp thực tế Core; mọi khoảng cách "target vs implemented" được gắn nhãn + trỏ IR-SALES-OC1/OC2/OC3 (build) — **không** phải mâu thuẫn thiết kế. Vẫn **KHÔNG** production-ready.

---

## Round 4 (2026-07-06, specs-vs-current-docs completion)
Phạm vi: rà soát lại `specs/*` theo open-decisions-register hiện tại sau khi callback OpenAPI tách current/target.

### R4.1 Task/callback race-guard current/target — ✅ CLOSED
- `api/openapi`, `api/02/05/07/08`, `functional/01/05`, `database/00/02/03/04/06`, `data/00/01/02`, `testing/02/04`, `workflows/01/06/09` đã thống nhất: current task dùng `order_state=CONFIRMING` + `payment_method_snapshot=COD`, không require `order_version`; current callback dùng `200`/`422` + recheck state/COD/sellable.
- `order_version`, `order_version_seen_by_ivr` và semantic `CALLBACK_*` response codes là target IR-SALES-OC1/OC2.

### R4.2 Error envelope enum — ✅ CLOSED
- OpenAPI `ErrorEnvelope.code` đã ref `ErrorCode` enum 15 mã ổn định, khớp `api/06 §1c` và `testing/04 CT-ERR-*`.

### R4.3 Remaining gaps — không phải conflict
- P0 gọi thật vẫn còn DT-01 (SIM gateway/procurement) và DF-03 (release sign-off).
- P1/P2 target còn IR-SALES-OC1/OC2/OC3, DO-02, DF-07, DT-02/04/06, IR-CRM-01, DC-05/06; tất cả đã có owner/gate trong `open-decisions-register.md`.

**Kết luận Round 4:** specs hiện đúng/đủ cho dev-review và dry-run/mock; các gap còn lại là build/procurement/legal target đã đăng ký, không còn conflict nội bộ chặn spec.
