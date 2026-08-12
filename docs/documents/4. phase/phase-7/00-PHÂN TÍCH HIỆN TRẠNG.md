# P7 - Analysis Only

**PROMPT-P7 - MC AI LIVE / LIVE SCRIPT RUNTIME / LIVE SALES CONTROL ANALYSIS HANDOFF**

## Mode

ANALYSIS ONLY. Khong sua code, khong tao script production, khong bat live auto mode, khong scale Ads.

## Entry Gate

Chi bat dau Phase 7 Analysis khi:

- Phase 5 Gateway public/private, comment classifier va Messenger handoff co evidence.
- Phase 4 AI Advisor / Final Response Guard co evidence.
- Phase 3 QuoteSnapshot / Order / Payment boundary co evidence.
- Phase 6 Verified Revenue / Ads signal boundary co evidence neu dung Ads-safe live orchestration.
- Neu chua du, ghi blocker `P7-ENTRY-BLOCKED-BY-GATEWAY-AI-COMMERCE`.

**GLOBAL GATEWAY:** BLOCKED.

## 9. SRS hardening addendum - analysis baseline

### 9.1. Source hierarchy bắt buộc

Khi dev hoặc analyst bóc SRS cho Phase 7, thứ tự nguồn phải cố định như sau:

| Priority | Source | Vai trò trong Phase 7 |
|---:|---|---|
| 1 | `docs/documents/1. master/02-MASTER-01-SOURCE-OF-TRUTH.md` | Luật thắng cuối cùng về source-of-truth và không bịa runtime truth. |
| 2 | `docs/documents/1. master/05-MASTER-04-RUNTIME-RESOLUTION-GUARD.md` | Thứ tự guard/resolver: sale lock, recall, QuoteSnapshot, Gateway, evidence, owner sign-off. |
| 3 | `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md` | Cách ghi evidence, smoke, completion. |
| 4 | `docs/documents/1. master/08-MASTER-07-RELEASE-GO-LIVE-CONTROL.md` | Không được gọi Live Ready, Release Ready, Production Ready khi thiếu gate. |
| 5 | `docs/documents/2. pack/08-PACK-08-MC-AI-LIVE.md` | Business pack của MC AI Live. |
| 6 | `docs/documents/3. tech/09-TECH-08-MC-AI-LIVE-LIVE-SCRIPT-RUNTIME-HOSTING-INTELLIGENCE-LIVE-SALES-CONTROL-ADS-SAFE-LIVE-ORCHESTRATION.md` | Source kỹ thuật chính của Phase 7. |
| 7 | `docs/documents/2. pack/06-PACK-06-FACEBOOK-CHANNEL-GATEWAY.md` và `docs/documents/3. tech/07-TECH-06-FACEBOOK-GATEWAY-META-LIVE-MESSENGER-CHANNEL-DELIVERY-COMMENT-TO-MESSENGER.md` | Owner public/private, live comment, Messenger handoff, delivery, moderation. |
| 8 | `docs/documents/2. pack/07-PACK-07-ADS-ROAS-ATTRIBUTION.md` và `docs/documents/3. tech/08-TECH-07-ADS-MEASUREMENT-ATTRIBUTION-ROAS-CPA-AOV-VERIFIED-REVENUE-SCALE-GATE.md` | Owner Ads signal, attribution, Verified Revenue, ROAS, scale gate. |
| 9 | `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md` | Owner QuoteSnapshot, order, payment, verified revenue. |
| 10 | `openapi/business-platform/mc-ai-live.v1.yaml`, `schemas/live/*`, `schemas/channel/*`, `schemas/commerce/*`, `schemas/ads/*` | Contract-level object/API references; không thay thế pack/tech. |

`BẢNG GÔM PHASE 7.md` không được dùng làm nguồn analysis trong scope này theo yêu cầu hiện tại. Nếu nội dung Bảng Gôm mâu thuẫn file con, file con sau hardening này thắng trong handoff cho dev.

### 9.2. Current-state facts phải kiểm tra trước SRS

Analyst phải ghi rõ mỗi fact dưới đây là `FOUND`, `MISSING`, `CONFLICT`, hoặc `NOT_APPLICABLE`:

| Fact ID | Fact cần xác minh | Expected source/evidence |
|---|---|---|
| P7-FCT-001 | Có `LivePlan` contract và API `/v1/live/plans`. | `openapi/business-platform/mc-ai-live.v1.yaml`, `schemas/live/live-plan.schema.json` |
| P7-FCT-002 | Có `HostCue` contract và API `/v1/live/host-cues`. | `schemas/live/host-cue.schema.json` |
| P7-FCT-003 | Có `LiveRiskCue` contract và API `/v1/live/risk-cues`. | `schemas/live/live-risk-cue.schema.json` |
| P7-FCT-004 | Có `PrivateHandoffCue` contract và API `/v1/live/private-handoff-cues`. | `schemas/live/private-handoff-cue.schema.json` |
| P7-FCT-005 | Có `LiveMeasurementEvent` contract cho signal, không phải revenue. | `schemas/live/live-measurement-event.schema.json` |
| P7-FCT-006 | Comment-to-Messenger dùng contract Gateway, không đi tắt từ MC AI Live. | `schemas/channel/comment-to-messenger-handoff.schema.json` |
| P7-FCT-007 | QuoteSnapshot là nguồn giá cuối cùng. | `schemas/commerce/quote-snapshot.schema.json`, TECH-04 |
| P7-FCT-008 | Official Order không sinh từ live public comment. | `schemas/commerce/order.schema.json`, TECH-04, TECH-06 |
| P7-FCT-009 | Payment/Verified Revenue không do MC AI Live xác nhận. | `schemas/commerce/payment.schema.json`, `schemas/commerce/verified-revenue.schema.json` |
| P7-FCT-010 | Sale Lock/Recall chặn live selling. | `schemas/ops/sale-lock.schema.json`, `state-machines/ops/sale-lock-state.md` |
| P7-FCT-011 | Ads live signal chỉ là funnel/engagement cho đến khi Verified Revenue link pass. | `schemas/live/live-measurement-event.schema.json`, `schemas/ads/verified-revenue-link.schema.json` |
| P7-FCT-012 | Evidence registry có trạng thái review/accepted/rejected. | `schemas/common/evidence-registry.schema.json`, `state-machines/common/evidence-registry-state.md` |

Không được viết SRS theo giả định "sẽ có module" nếu chưa map được contract hoặc backlog yêu cầu tạo contract mới.

### 9.3. Boundary khẳng định cho SRS

1. MC AI Live là assistant cho host/operator, không phải AI Advisor tư vấn sâu cá nhân hóa.
2. MC AI Live không thay Facebook Gateway; mọi public reply, Messenger handoff, pacing, delivery, moderation phải qua Gateway boundary.
3. MC AI Live không thay Commerce Runtime; mọi final price phải từ QuoteSnapshot, mọi order phải từ official order flow.
4. MC AI Live không thay Ads Measurement; live engagement chỉ là signal, không phải revenue hoặc ROAS.
5. MC AI Live không thay Operational Core; sellable, recall, sale lock, suppression phải thắng mọi script/cue.
6. MC AI Live không thay owner review; rehearsal pass không đồng nghĩa production go-live.
7. Documentation complete của Phase 7 không được gọi `LIVE_READY`, `RELEASE_READY`, `PRODUCTION_READY`, `GLOBAL_SMOKE_PASS`, hoặc `COMPLETION_PASS`.

### 9.4. Gap taxonomy dùng chung

| Gap type | Định nghĩa | Cách ghi |
|---|---|---|
| `SOURCE_MISSING` | Không tìm thấy source/contract/evidence cần thiết. | Ghi file/path thiếu và owner cần cung cấp. |
| `CONTRACT_MISSING` | Pack/tech có yêu cầu nhưng schema/OpenAPI chưa có object tương ứng. | Ghi object cần tạo, không tự triển khai. |
| `CONTRACT_CONFLICT` | Schema/API hiện có khác yêu cầu pack/tech/master. | Ghi rõ source thắng và yêu cầu sửa contract. |
| `RUNTIME_UNKNOWN` | Không biết code/runtime hiện tại có tồn tại hay không. | Chỉ ghi analysis, không claim pass. |
| `EVIDENCE_MISSING` | Có contract nhưng chưa có smoke/evidence accepted. | Giữ `GLOBAL GATEWAY: BLOCKED`. |
| `OWNER_DECISION_REQUIRED` | Cần owner chốt scope, live page, live plan, risk policy, go-live. | Tạo decision ID. |

### 9.5. Gap matrix tối thiểu

| Backlog | Scope | Minimum SRS output |
|---|---|---|
| LIVE-BLG-001 | Live Session Context Resolver | Live session identity, page/channel binding, live plan/board/product scope, suppression, readiness state. |
| LIVE-BLG-002 | Live Script Runtime | Script source, script brief, host cue, script version, guard chain, allowed/blocked output. |
| LIVE-BLG-003 | Claim Guard | Claim source, public wording, medical/health-sensitive block, fake urgency/scarcity block. |
| LIVE-BLG-004 | Live Comment Classifier | Intent taxonomy, confidence, public/private decision, moderation risk. |
| LIVE-BLG-005 | Messenger Coordination | Gateway handoff command, handoff status, delivery log, context preservation. |
| LIVE-BLG-006 | Product/Price Boundary | QuoteSnapshot dependency, no self-price, no self-order, sellable/sale-lock/recall guard. |
| LIVE-BLG-007 | Troll/Abuse Route | Abuse/malicious route, no public debate, no sales Messenger pull for malicious actors. |
| LIVE-BLG-008 | Complaint/CSKH Route | Complaint, adverse event, payment/order quality route, human handoff. |
| LIVE-BLG-009 | Ads-safe Live Signal | Funnel signal only, no ROAS, no scale, verified revenue link after Commerce. |
| LIVE-BLG-010 | Smoke/Evidence Report | P0 smoke matrix, evidence package, final blocked/release wording. |

### 9.6. Conflict ladder

Nếu có conflict giữa tài liệu, áp dụng thứ tự thắng:

1. Master source-of-truth và runtime guard.
2. Completion/evidence/release gate.
3. TECH-08/PACK-08 trong phạm vi MC AI Live.
4. TECH-06/PACK-06 khi liên quan Gateway/public/private/Messenger.
5. TECH-04 khi liên quan QuoteSnapshot/order/payment/revenue.
6. TECH-07 khi liên quan Ads/ROAS/scale.
7. Schema/OpenAPI hiện có.
8. File Phase 7 con.
9. Tài liệu business bổ sung/reference-only.

Không được dùng wording marketing hoặc live script mẫu để override source-of-truth kỹ thuật.

### 9.7. Output format cho analyst/dev

Mỗi report Phase 7 phải có:

1. `scope`.
2. `sources_checked`.
3. `contracts_checked`.
4. `runtime_current_state`.
5. `gap_matrix`.
6. `conflict_matrix`.
7. `required_contract_changes`.
8. `required_api_changes`.
9. `required_smoke`.
10. `required_evidence`.
11. `owner_decisions`.
12. `blocked_status`.

Mẫu verdict:

```yaml
phase: P7
verdict: ANALYSIS_REPORT_ONLY
live_ready: false
release_ready: false
production_ready: false
global_gateway: BLOCKED
reason:
  - evidence_not_accepted
  - owner_signoff_missing
  - global_gateway_not_open
```

### 9.8. No-go claims

Các câu sau bị cấm trong mọi output Phase 7 nếu chưa có evidence accepted và owner decision:

- `Phase 7 PASS`
- `MC AI Live production ready`
- `Live Ready`
- `Release Ready`
- `Global Smoke Pass`
- `Gateway PASS`
- `Có thể chạy live thật`
- `Có thể scale Ads từ live`
- `Có thể chốt đơn trên live`

Nếu cần viết trạng thái tích cực, chỉ được dùng: `READY_FOR_DEV_TASK_BREAKDOWN`, `ANALYSIS_COMPLETE_FOR_HANDOFF`, hoặc `EVIDENCE_SUBMITTED_ONLY`, kèm `GLOBAL GATEWAY: BLOCKED`.

## Source Of Truth

- `docs/documents/3. tech/09-TECH-08-MC-AI-LIVE-LIVE-SCRIPT-RUNTIME-HOSTING-INTELLIGENCE-LIVE-SALES-CONTROL-ADS-SAFE-LIVE-ORCHESTRATION.md`
- `docs/documents/2. pack/08-PACK-08-MC-AI-LIVE.md`
- `docs/documents/3. tech/07-TECH-06-FACEBOOK-GATEWAY-META-LIVE-MESSENGER-CHANNEL-DELIVERY-COMMENT-TO-MESSENGER.md`
- `docs/documents/3. tech/08-TECH-07-ADS-MEASUREMENT-ATTRIBUTION-ROAS-CPA-AOV-VERIFIED-REVENUE-SCALE-GATE.md`
- `docs/documents/3. tech/06-TECH-05-AI-ADVISOR-RUNTIME-PRODUCT-CONSULTING-CUSTOMER-MEMORY-QUOTE-SAFE-SALES-ORDER-DRAFT-HANDOFF.md`
- `docs/documents/3. tech/13-TECH-12-PHASE-BACKLOG-PACK-DEV-TASK-BREAKDOWN-SOURCE-TO-BACKLOG-MATRIX.md`

Chua thay source-of-truth `docs/docs-ginsengfood-project/` trong workspace hien tai. Neu sau nay file do xuat hien, phai doi chieu lai truoc khi sua backend that.

## Scope Can Phan Tich

1. Live Session Context Resolver.
2. Live Script Runtime.
3. Script Guard / Claim Guard.
4. Live Comment Classifier.
5. Comment-to-Messenger coordination.
6. Product/Price Boundary in Live.
7. Troll / abuse / malicious handling.
8. Complaint / CSKH / quality route.
9. Ads-safe live signal boundary.
10. MC AI Live smoke/evidence pack.

## Khong Duoc Lam

- MC AI Live khong tu bao gia.
- MC AI Live khong tu tao order.
- MC AI Live khong tu xac nhan payment/revenue.
- Live signal khong duoc dung lam ROAS/revenue.
- Script chua guard khong duoc dung.
- Claim chua approved khong duoc noi.
- Public live khong duoc lo private/internal data.

## Output Bat Buoc

- Current-state map cho live/session/script/comment/gateway/ads integration.
- Gap matrix LIVE-BLG-001 -> LIVE-BLG-010.
- Conflict matrix voi TECH-04/05/06/07/08.
- Script/claim/privacy risk register.
- Proposed smoke matrix.
- Owner decision required.

## Done Gate

Analysis chi ket thuc khi co report ro evidence/missing evidence. Khong goi live-ready hoac release-ready.

