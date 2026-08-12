# P5 - CHỈ MỤC BÀN GIAO

**Phase:** PHASE-05 - Facebook Channel Gateway / Meta Live / Messenger Delivery  
**Ngày viết lại:** 2026-06-05  
**Trạng thái:** `REWRITTEN_FROM_FACEBOOK_ADS_LIVE_BASELINE`  
**Gateway:** `PRODUCTION_BLOCKED`  
**Release:** `NOT_RELEASE_READY`

## 1. Đọc file nào trước

1. `BẢNG GÔM GIAI ĐOẠN 5.md`
2. `10-P5-PHỤ LỤC KHÓA RUNTIME GATEWAY FACEBOOK VÀ NHẮN TIN.md`
3. `00-P5-PHAN-TICH-HIEN-TRANG.md`
4. `01-P5-THIẾT KẾ KỸ THUẬT.md`
5. `02-P5-2A-ĐỊNH DANH KÊNH VÀ NGỮ CẢNH PAGE.md`
6. `03-P5-2B-BÌNH LUẬN CÔNG KHAI VÀ BÀN GIAO MESSENGER.md`
7. `04-P5-2C-NGỮ CẢNH MESSENGER VÀ CHẶN PHẢN HỒI CUỐI.md`
8. `05-P5-2D-CHỈ BÁO ĐANG GÕ VÀ NHỊP ĐỘ PHẢN HỒI.md`
9. `06-P5-2E-GIỚI HẠN TẦN SUẤT CHỐNG SPAM VÀ KIỂM DUYỆT.md`
10. `07-P5-2F-CHẶN GỬI TỪ CHỐI NHẬN VÀ BÀN GIAO NGƯỜI THẬT.md`
11. `08-P5-2G-BÁO CÁO KIỂM THỬ KHÓI VÀ BẰNG CHỨNG GATEWAY.md`

## 2. Vai trò Phase 5

Phase 5 là Facebook Channel Gateway: lớp hạ tầng kênh cho Meta webhook, Page Registry, event normalization, live comment, Messenger handoff, delivery pacing, rate-limit, suppression, attribution preservation và evidence.

Phase 5 không phải AI Advisor, không phải Commerce, không phải Payment, không phải CRM worker, không phải Ads optimizer và không phải Diamond payout runtime.

## 3. Entry gate

Gateway production chỉ được xét khi:

- AI Advisor Facebook Completion Report `PASS`.
- Evidence Package `PASS`.
- GATE-01 đến GATE-12 của AI Channel đều PASS.
- E2E live-to-order smoke PASS.
- Owner sign-off PASS.
- Rollback plan và monitoring READY.

Nếu chưa đủ, trạng thái bắt buộc:

`P5_GATEWAY_PRODUCTION_BLOCKED_BY_AI_COMPLETION_EVIDENCE`

## 4. Source-of-truth

Nguồn Phase 5 tối thiểu:

| Nhóm | File |
| --- | --- |
| Master source truth | `docs/documents/1. master/02-MASTER-01-SOURCE-OF-TRUTH.md` |
| Master dependency | `docs/documents/1. master/03-MASTER-02-CROSS-PACK-DEPENDENCY.md` |
| Master runtime guard | `docs/documents/1. master/05-MASTER-04-RUNTIME-RESOLUTION-GUARD.md` |
| Master evidence gate | `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md` |
| Master release control | `docs/documents/1. master/08-MASTER-07-RELEASE-GO-LIVE-CONTROL.md` |
| Master decision log | `docs/documents/1. master/09-MASTER-08-CROSS-SYSTEM-DECISION-LOG.md` |
| Master runtime addendum | `docs/documents/1. master/10-MASTER-09-CROSS-PHASE-RUNTIME-LOCK-ADDENDUM.md` |
| Gateway pack | `docs/documents/2. pack/06-PACK-06-FACEBOOK-CHANNEL-GATEWAY.md` |
| AI Advisor pack | `docs/documents/2. pack/05-PACK-05-AI-ADVISOR-CHANNEL.md` |
| Ads pack | `docs/documents/2. pack/07-PACK-07-ADS-ROAS-ATTRIBUTION.md` |
| Live pack | `docs/documents/2. pack/08-PACK-08-MC-AI-LIVE.md` |
| Completion evidence pack | `docs/documents/2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md` |
| Commerce tech | `docs/documents/3. tech/05-TECH-04-COMMERCE-PRICE-PROMOTION-ORDER-PAYMENT-SHIPPING.md` |
| AI Advisor tech | `docs/documents/3. tech/06-TECH-05-AI-ADVISOR-RUNTIME-PRODUCT-CONSULTING-CUSTOMER-MEMORY-QUOTE-SAFE-SALES-ORDER-DRAFT-HANDOFF.md` |
| Gateway tech | `docs/documents/3. tech/07-TECH-06-FACEBOOK-GATEWAY-META-LIVE-MESSENGER-CHANNEL-DELIVERY-COMMENT-TO-MESSENGER.md` |
| Ads tech | `docs/documents/3. tech/08-TECH-07-ADS-ROAS-PIXEL-CAPI-ATTRIBUTION-SCALE-GATE.md` |
| Live tech | `docs/documents/3. tech/09-TECH-08-MC-AI-LIVE-SESSION-SCRIPT-LIVE-COMMERCE-COMMENT-ROUTING.md` |
| Global evidence tech | `docs/documents/3. tech/11-TECH-10-GLOBAL-SMOKE-EVIDENCE-RELEASE-GATE.md` |
| Phase 4 AI lock | `docs/documents/4. phase/phase-4/BẢNG GÔM GIAI ĐOẠN 4.md` |
| CRM canonical | `docs/documents/6. canonical/01-CANONICAL-CRM-MEMBER-LIFECYCLE-RUNTIME.md` |
| Finance/Diamond canonical | `docs/documents/6. canonical/02-CANONICAL-FINANCE-DIAMOND-COMMISSION-PAYOUT-RUNTIME.md` |
| Evidence canonical | `docs/documents/6. canonical/03-CANONICAL-EVIDENCE-SMOKE-GATE-CUSTOMER-TO-CASH-CARE.md` |

Nguồn bổ sung được nhập:

- Business & Ads Financial Baseline Lock.
- AI Advisor Facebook Completion Report.
- Facebook Page List / Owner Decisions.
- Ads Cost Model / Live Commerce Scale Model.
- Pricing / Golden Hour / Diamond Model.
- Financial Analysis.
- AI Channel đủ điều kiện trước Gateway.
- Trình tự triển khai tiếp theo.

File `01. FILE FACEBOOK ADS & LIVE COMMERCE OPERATING MODEL.md` hiện rỗng.

## 5. Execution order

| Bước | File | Scope | Done condition |
| --- | --- | --- | --- |
| 0 | `BẢNG GÔM GIAI ĐOẠN 5.md` | Reading hub | Hiểu toàn bộ Gateway boundary và business/financial/channel locks. |
| 1 | `10-P5-PHỤ LỤC KHÓA RUNTIME GATEWAY FACEBOOK VÀ NHẮN TIN.md` | Runtime lock | Gateway không tự quote/order/CRM/commission, production blocked. |
| 2 | `00-P5-PHAN-TICH-HIEN-TRANG.md` | Analysis | Current state/gap/risk/owner decisions, không sửa code. |
| 3 | `01-P5-THIẾT KẾ KỸ THUẬT.md` | Design | API/DB/event/feature flag design. |
| 4 | `02-P5-2A...` | Page Registry | Meta App/Page/Token governance. |
| 5 | `03-P5-2B...` | Webhook/live/comment | Normalized event, public/private, handoff. |
| 6 | `04-P5-2C...` | Messenger/AI routing | Thread binding, Final Response Guard, quote/order handoff. |
| 7 | `05-P5-2D...` | Delivery | Typing, pacing, retry, delivery log. |
| 8 | `06-P5-2E...` | Security/moderation | Rate-limit, anti-spam, dedup, App Review checklist. |
| 9 | `07-P5-2F...` | Suppression/attribution | Opt-out, CRM, human handoff, Ads/ROAS/Diamond guard. |
| 10 | `08-P5-2G...` | Smoke/evidence | P0 matrix, E2E smoke, dashboard, pilot, rollback. |

## 6. Core rules

- Gateway chỉ nhận/normalize/route/deliver; không tự tạo business truth.
- Không có Final Response Guard PASS thì không delivery.
- Không có QuoteSnapshot thì không final price.
- Không có CustomerConfirmation/order_code thì không order success.
- Public comment/live không được leak price/PII/payment/order/health-sensitive.
- Handoff success mới được nói đã chuyển Messenger.
- ORDER_VERIFIED là event revenue chính.
- Diamond commission chỉ sau valid referral bind và Order Verified, không tính trong Gateway.
- Completion Report chưa PASS thì Gateway production vẫn BLOCKED.

## 7. Pilot/scale rule

Pilot 7-14 ngày, không scale mạnh ngay.

Chỉ scale khi:

- Không leak PII.
- Không spam.
- Handoff đúng success/fail.
- AOV >= 2 hộp/đơn.
- CPA nằm trong target.
- ORDER_VERIFIED tracking đúng.
- ROAS đo thật và cao hơn break-even.
- COD fail/cancel/return trong ngưỡng hoặc owner accepted risk rõ.

## 8. Status cuối

`PHASE_5_REWRITTEN_AS_FACEBOOK_CHANNEL_GATEWAY_TECHNICAL_DEV_PACK`

`GATEWAY_PRODUCTION_BLOCKED`

`NOT_RELEASE_READY`

## 9. SRS hardening addendum - Dev handoff map

### 9.1. Reading order for dev

| Order | File | Purpose |
| --- | --- | --- |
| 1 | `09-P5-CHỈ MỤC BÀN GIAO.md` | Scope, reading order, boundary, status vocabulary. |
| 2 | `00-P5-PHAN-TICH-HIEN-TRANG.md` | Analysis checklist and conflict taxonomy. |
| 3 | `10-P5-PHỤ LỤC KHÓA RUNTIME GATEWAY FACEBOOK VÀ NHẮN TIN.md` | Runtime invariants and production lock. |
| 4 | `01-P5-THIẾT KẾ KỸ THUẬT.md` | Overall module/API/DB/state contract. |
| 5 | `02-P5-2A-ĐỊNH DANH KÊNH VÀ NGỮ CẢNH PAGE.md` | Page Registry and permission model. |
| 6 | `03-P5-2B-BÌNH LUẬN CÔNG KHAI VÀ BÀN GIAO MESSENGER.md` | Public comment and handoff. |
| 7 | `04-P5-2C-NGỮ CẢNH MESSENGER VÀ CHẶN PHẢN HỒI CUỐI.md` | Messenger and Final Response Guard. |
| 8 | `05-P5-2D-CHỈ BÁO ĐANG GÕ VÀ NHỊP ĐỘ PHẢN HỒI.md` | Delivery pacing and retry. |
| 9 | `06-P5-2E-GIỚI HẠN TẦN SUẤT CHỐNG SPAM VÀ KIỂM DUYỆT.md` | Rate limit, moderation, security. |
| 10 | `07-P5-2F-CHẶN GỬI TỪ CHỐI NHẬN VÀ BÀN GIAO NGƯỜI THẬT.md` | Suppression, opt-out, human handoff, attribution. |
| 11 | `08-P5-2G-BÁO CÁO KIỂM THỬ KHÓI VÀ BẰNG CHỨNG GATEWAY.md` | Smoke/evidence gate. |

### 9.2. Traceability matrix

| Requirement group | Owner file | Upstream/downstream guard |
| --- | --- | --- |
| P5-SRS-PAGE | 02-P5-2A | PACK-06, TECH-06, schemas/channel. |
| P5-SRS-WEBHOOK | 03-P5-2B, 06-P5-2E | TECH-06, idempotency standard, evidence registry. |
| P5-SRS-PUBLIC | 03-P5-2B | PACK-05 public comment rules, TECH-06. |
| P5-SRS-MESSENGER | 04-P5-2C, 05-P5-2D | PACK-05/TECH-05 AI guard, TECH-06. |
| P5-SRS-DELIVERY | 05-P5-2D, 06-P5-2E | Platform policy, rate-limit, App Review. |
| P5-SRS-SUPPRESSION | 07-P5-2F | Canonical CRM, sale-lock/recall state machines. |
| P5-SRS-ATTRIBUTION | 07-P5-2F, 08-P5-2G | PACK-07/Ads schemas, Finance/Diamond canonical. |
| P5-SRS-EVIDENCE | 08-P5-2G | Master evidence gate, canonical evidence. |
| P5-SRS-RELEASE | 09-P5, 10-P5 | Master release/go-live control. |

### 9.3. Backlog decomposition

```yaml
phase_5_backlog:
  epics:
    - P5-E01 Page Registry and Meta App Governance
    - P5-E02 Webhook Intake and Event Normalization
    - P5-E03 Public Comment Policy and Comment-to-Messenger
    - P5-E04 Messenger Session and AI Guard Consumer
    - P5-E05 Delivery Pacing, Retry, Rate Limit
    - P5-E06 Moderation, Security, App Review
    - P5-E07 Suppression, Human Handoff, Attribution
    - P5-E08 Evidence, Smoke, Release Gate
  cross_cutting:
    - idempotency
    - redaction
    - RBAC
    - audit/evidence
    - kill-switch
    - owner decision registry
```

### 9.4. Required dev outputs

Dev handoff is incomplete unless implementation analysis produces:

1. SRS requirement list with IDs mapped to P5 files.
2. OpenAPI/internal API delta.
3. DB migration plan.
4. Event/schema payloads.
5. State machine diagrams or transition tables.
6. Service/module ownership.
7. Test matrix by P0 smoke ID.
8. Seed/config plan for Page Registry without real secrets.
9. Evidence collection plan.
10. Release blocker register.

### 9.5. Scope lock

Dev must not turn Phase 5 into:

- AI Advisor rewrite.
- Commerce quote/order/payment service.
- CRM lifecycle engine.
- Ads optimizer/ROAS source.
- Diamond commission/payout runtime.
- Generic chatbot webhook.

Final status remains `GATEWAY_PRODUCTION_BLOCKED` and `NOT_RELEASE_READY` until evidence gates explicitly move it.

