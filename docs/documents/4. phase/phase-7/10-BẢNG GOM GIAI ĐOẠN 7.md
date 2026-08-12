# Phase 7 - Consolidated Handoff Index

**PROMPT-P7 - MC AI LIVE / LIVE SCRIPT RUNTIME / LIVE SALES CONTROL ANALYSIS HANDOFF**

## Mode

ANALYSIS ONLY. Khong sua code, khong tao script production, khong bat live auto mode, khong scale Ads.

## AI Review Boundary

Reviewer AI chỉ chấm tài liệu này theo tiêu chí đủ để giao dev chạy lane phân tích, thiết kế, bóc task và triển khai có kiểm soát. `ANALYSIS ONLY`, `GLOBAL GATEWAY: BLOCKED`, `NOT_LIVE_READY`, `NOT_RELEASE_READY` hoặc yêu cầu evidence không phải lỗi dev-readiness; đó là guard để không nhầm tài liệu thiết kế với live/go-live approval.

```text
DEV_IMPLEMENTATION_DOCUMENTATION_READY: YES_FOR_CONTROLLED_TASK_BREAKDOWN
PRODUCTION_READY: NO
RUNTIME_EVIDENCE_REQUIRED_AFTER_DEV: YES
```

## Entry Gate

Chi bat dau Phase 7 Analysis khi:

- Phase 5 Gateway public/private, comment classifier va Messenger handoff co evidence.
- Phase 4 AI Advisor / Final Response Guard co evidence.
- Phase 3 QuoteSnapshot / Order / Payment boundary co evidence.
- Phase 6 Verified Revenue / Ads signal boundary co evidence neu dung Ads-safe live orchestration.
- Neu chua du, ghi blocker `P7-ENTRY-BLOCKED-BY-GATEWAY-AI-COMMERCE`.

**GLOBAL GATEWAY:** BLOCKED.

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

# P7.1 - Technical Design Only

**PROMPT-P7.1 - MC AI LIVE TECHNICAL DESIGN HANDOFF**

## Mode

TECHNICAL DESIGN ONLY. Khong implement live runtime, khong bat live auto-reply, khong tao script production.

## Input Bat Buoc

- P7 Analysis report.
- P5 Gateway evidence.
- P4 AI/Final Response Guard evidence.
- P3 Commerce evidence.
- P6 Ads signal/Verified Revenue boundary evidence neu dung ads-safe orchestration.

Neu thieu Gateway evidence, ghi `BLOCKED_NEED_PHASE5_GATEWAY_EVIDENCE`.

## Design Boundary

MC AI Live la live orchestration assistant. No chi:

- Resolve live session context.
- Goi y script/nhiep noi/CTA.
- Goi y public-safe comment handling.
- Coordinate Messenger handoff qua Gateway.
- Respect Product Knowledge / Claim Guard / Final Response Guard.
- Respect Commerce QuoteSnapshot / Order / Payment boundary.
- Respect Ads signal boundary.

MC AI Live khong:

- Tu tinh gia/discount/ton kho.
- Tu tao order/payment/revenue.
- Tu noi claim y te chua approved.
- Tu dung live signal lam revenue/ROAS.

## Workstreams

| Slice | Backlog          | Muc tieu                                    |
| ----- | ---------------- | ------------------------------------------- |
| P7.2A | LIVE-BLG-001     | Live Session Context Resolver               |
| P7.2B | LIVE-BLG-002/003 | Live Script Runtime + Script/Claim Guard    |
| P7.2C | LIVE-BLG-004/005 | Comment Classifier + Messenger Coordination |
| P7.2D | LIVE-BLG-006     | Product/Price Boundary In Live              |
| P7.2E | LIVE-BLG-007/008 | Troll/Abuse + Complaint/CSKH Route          |
| P7.2F | LIVE-BLG-009     | Ads-safe Live Signal Boundary               |
| P7.2G | LIVE-BLG-010     | Smoke / Evidence Pack                       |

## Contract Can Khoa

- Live session context.
- Live product board reference.
- Script runtime request/response.
- Claim guard decision.
- Comment classification result.
- Messenger handoff command.
- Commerce quote boundary.
- Ads-safe signal event.
- Suppression decision.
- Evidence record.

## API/DTO Impact

Moi API/DTO/event/OpenAPI thay doi phai ghi FE/operator console/channel/ads dashboard impact.

## P0 Design Blockers

- Live tu bao gia.
- Live tu tao order/payment/revenue.
- Script chua guard van duoc dung.
- Claim chua approved van noi.
- Live signal dung lam ROAS.
- Troll/malicious keo Messenger sai policy.
- Complaint that bi xu nhu troll.

## Final Status

**P7 TECHNICAL DESIGN HANDOFF COMPLETED - NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P7.2A - Live Session Context Resolver

**BACKLOG:** LIVE-BLG-001  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Khoa live session context de MC AI Live biet live nao, page nao, product board nao, host/operator nao va suppression nao dang active.

## Entry Gate

- P7.1 design accepted.
- P5 Gateway channel/page/session evidence pass.
- Live board/source accepted.

## Implementation Scope

- Live session identity.
- Page/channel binding.
- Live product board binding.
- Host/operator context.
- Suppression context.
- Evidence logging.

## Not Allowed

- Hardcode live session/product board.
- Dung live session unknown de tao script.
- Bo qua recall/sale lock/suppression.
- Public internal board/private data.

## Acceptance Criteria

- Moi live command co live_session_id/page/channel/context.
- Unknown session bi reject.
- Product board co source/evidence.
- Suppression state duoc resolve truoc script/comment suggestion.

## Smoke

| Smoke ID      | Scenario           | Expected                 |
| ------------- | ------------------ | ------------------------ |
| P7.2A-SMK-001 | Known live session | Context resolved         |
| P7.2A-SMK-002 | Unknown session    | Reject/quarantine        |
| P7.2A-SMK-003 | Missing board      | No script generation     |
| P7.2A-SMK-004 | Sale lock active   | Sales suggestion blocked |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P7.2B - Live Script Runtime / Claim Guard

**BACKLOG:** LIVE-BLG-002 / LIVE-BLG-003  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Tao/giai quyet script live tu approved source va bat buoc qua Script Guard / Claim Guard truoc khi host/operator dung.

## Entry Gate

- LIVE-BLG-001 pass.
- AIA-BLG-002 Product Knowledge / Claim Guard pass.
- Approved script/product knowledge source available.

## Implementation Scope

- Script runtime resolver.
- Product story generation from approved public-safe data.
- Claim Guard enforcement.
- Script version/evidence.
- Guard fail -> no use/human review.

## Not Allowed

- Script tu noi san pham nhu thuoc/chua benh.
- Claim chua approved van vao script.
- Script hardcode gia/discount/ton kho.
- Script bypass guard vi dang live.

## Acceptance Criteria

- Script co source/version.
- Script pass Claim Guard truoc khi hien thi/dung.
- Guard fail co reason va no delivery/use.
- Product story khong lo recipe/BOM/supplier/private data.

## Smoke

| Smoke ID      | Scenario                   | Expected         |
| ------------- | -------------------------- | ---------------- |
| P7.2B-SMK-001 | Approved product story     | Script pass      |
| P7.2B-SMK-002 | Medical treatment claim    | Guard fail       |
| P7.2B-SMK-003 | Internal formula in script | Guard fail       |
| P7.2B-SMK-004 | Hardcoded price            | Guard fail/block |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P7.2C - Live Comment Classifier / Messenger Coordination

**BACKLOG:** LIVE-BLG-004 / LIVE-BLG-005  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Classify comment live va coordinate Gateway de public reply an toan, handoff Messenger khi can.

## Entry Gate

- GW-BLG-003/004 pass.
- GW-BLG-008 classifier/moderation evidence pass.
- LIVE-BLG-001 pass.

## Implementation Scope

- Live comment classifier.
- Intent categories: price, buy, consult, troll, complaint, spam, generic.
- Public response suggestion.
- Messenger handoff command via Gateway.
- Evidence for classification.

## Not Allowed

- MC AI Live tu gui Messenger bypass Gateway.
- Public private info/order/payment.
- Pull troll/malicious to Messenger sales.
- Treat complaint as troll.

## Acceptance Criteria

- Price/buy/deep consult -> public-safe + Messenger route.
- Troll/malicious -> moderation.
- Complaint -> CSKH/human route.
- All handoff commands go through Gateway contract.

## Smoke

| Smoke ID      | Scenario          | Expected                                       |
| ------------- | ----------------- | ---------------------------------------------- |
| P7.2C-SMK-001 | "Gia bao nhieu"   | Route Messenger, no public final price         |
| P7.2C-SMK-002 | "Mua 1 hop"       | Route order draft path via Gateway/AI/Commerce |
| P7.2C-SMK-003 | Troll comment     | Moderation                                     |
| P7.2C-SMK-004 | Complaint quality | CSKH/human                                     |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P7.2D - Product / Price Boundary In Live

**BACKLOG:** LIVE-BLG-006  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Dam bao MC AI Live khong tu bao gia, khong tu xac nhan ton kho, khong tu tao quote/order; moi gia cuoi phai tu Commerce QuoteSnapshot.

## Entry Gate

- COM-BLG-002 QuoteSnapshot pass.
- P4 AI QuoteSnapshot consumption pass.
- P5 Gateway public/private handoff pass.

## Implementation Scope

- Live quote boundary guard.
- Product sellable/suppression awareness.
- Public-safe price wording.
- QuoteSnapshot request/handoff path via Gateway/AI/Commerce.
- Evidence for no self-price.

## Not Allowed

- Noi final price tu script/memory.
- Tu tinh discount/member benefit/live offer.
- Tu xac nhan con hang khi sellable unknown.
- Tu tao order_code/payment/revenue.

## Acceptance Criteria

- No QuoteSnapshot -> no final price.
- Live comment hoi gia -> route private/quote runtime.
- Expired QuoteSnapshot -> refresh request.
- Sale lock/recall -> no sales CTA.

## Smoke

| Smoke ID      | Scenario                      | Expected                   |
| ------------- | ----------------------------- | -------------------------- |
| P7.2D-SMK-001 | Host asks price without quote | No final price             |
| P7.2D-SMK-002 | Active QuoteSnapshot          | Display allowed per policy |
| P7.2D-SMK-003 | Expired QuoteSnapshot         | Request refresh            |
| P7.2D-SMK-004 | SKU recall/sale lock          | No sales CTA               |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P7.2E - Troll / Abuse / Complaint Route

**BACKLOG:** LIVE-BLG-007 / LIVE-BLG-008  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Tach troll/abuse/malicious khoi complaint that va route dung moderation/CSKH/human.

## Entry Gate

- GW-BLG-008 pass.
- P7.2C classifier evidence available.
- Human/CSKH route accepted.

## Implementation Scope

- Troll/abuse classifier.
- Complaint classifier.
- Quality/CSKH handoff.
- Moderator suggestion.
- Evidence with masked samples.

## Not Allowed

- Complaint that bi xoa/hide nhu troll.
- Troll/malicious bi keo vao Messenger sales.
- MC AI Live tranh luan cong khai lam tang risk.
- Lo thong tin don hang/payment cua khach tren public.

## Acceptance Criteria

- Troll -> moderation/safe ignore/hide by policy.
- Complaint -> CSKH/human with evidence.
- Severe issue -> escalation.
- Response wording public-safe.

## Smoke

| Smoke ID      | Scenario                | Expected                        |
| ------------- | ----------------------- | ------------------------------- |
| P7.2E-SMK-001 | Abuse/troll             | Moderation route                |
| P7.2E-SMK-002 | Complaint hang loi      | CSKH route                      |
| P7.2E-SMK-003 | Payment/order complaint | Private/human, no public detail |
| P7.2E-SMK-004 | Malicious link          | Block/quarantine                |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P7.2E - Troll / Abuse / Complaint Route

**BACKLOG:** LIVE-BLG-007 / LIVE-BLG-008  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Tach troll/abuse/malicious khoi complaint that va route dung moderation/CSKH/human.

## Entry Gate

- GW-BLG-008 pass.
- P7.2C classifier evidence available.
- Human/CSKH route accepted.

## Implementation Scope

- Troll/abuse classifier.
- Complaint classifier.
- Quality/CSKH handoff.
- Moderator suggestion.
- Evidence with masked samples.

## Not Allowed

- Complaint that bi xoa/hide nhu troll.
- Troll/malicious bi keo vao Messenger sales.
- MC AI Live tranh luan cong khai lam tang risk.
- Lo thong tin don hang/payment cua khach tren public.

## Acceptance Criteria

- Troll -> moderation/safe ignore/hide by policy.
- Complaint -> CSKH/human with evidence.
- Severe issue -> escalation.
- Response wording public-safe.

## Smoke

| Smoke ID      | Scenario                | Expected                        |
| ------------- | ----------------------- | ------------------------------- |
| P7.2E-SMK-001 | Abuse/troll             | Moderation route                |
| P7.2E-SMK-002 | Complaint hang loi      | CSKH route                      |
| P7.2E-SMK-003 | Payment/order complaint | Private/human, no public detail |
| P7.2E-SMK-004 | Malicious link          | Block/quarantine                |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P7.2F - Ads-safe Live Signal Boundary

**BACKLOG:** LIVE-BLG-009  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Dam bao live signal chi la signal/funnel/engagement, khong bi dung lam revenue/ROAS hoac scale decision khi chua qua Phase 6.

## Entry Gate

- ADS-BLG-001 event taxonomy pass.
- ADS-BLG-004 Verified Revenue consumption pass neu lien quan ROAS.
- LIVE-BLG-001/004 pass.

Neu thieu ADS-BLG-004, ghi `LIVE-BLG-009-BLOCKED-BY-ADS-VERIFIED-REVENUE`.

## Implementation Scope

- Live signal event mapping.
- Ads-safe label.
- Revenue=false guard.
- Signal export to Ads Measurement.
- Evidence for no ROAS from live signal.

## Not Allowed

- Dung comment/view/engagement live lam revenue.
- Dung live order intent lam ROAS.
- Auto-scale ads tu live hype.
- Hide suppression/data quality fail.

## Acceptance Criteria

- Live signal exported as funnel/engagement only.
- Revenue metric only from Commerce Verified Revenue.
- Ads dashboard flags live signal confidence.
- Scale decision remains under Phase 6 Gate.

## Smoke

| Smoke ID      | Scenario                 | Expected                     |
| ------------- | ------------------------ | ---------------------------- |
| P7.2F-SMK-001 | Live comment high intent | Signal only, not revenue     |
| P7.2F-SMK-002 | Live view spike          | No ROAS                      |
| P7.2F-SMK-003 | Live quote created       | Funnel signal only           |
| P7.2F-SMK-004 | Verified revenue arrives | Revenue via Ads Phase 6 only |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P7.2G - MC AI Live Smoke / Evidence Report

**BACKLOG:** LIVE-BLG-010  
**MODE:** SMOKE / EVIDENCE / REPORT ONLY  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Chay smoke cho Phase 7, gom evidence, lap report 14 muc. Khong sua code, khong bat live production trong file nay.

## Entry Gate

- LIVE-BLG-001 -> LIVE-BLG-009 da co report.
- P5 Gateway evidence available.
- P4 Final Response Guard evidence available.
- P3 Commerce boundary evidence available.
- P6 Ads boundary evidence available neu dung live signal.

## Smoke Matrix Toi Thieu

| Smoke ID    | Scenario                        | Expected                           |
| ----------- | ------------------------------- | ---------------------------------- |
| P7-GSMK-001 | Known live session              | Context resolved                   |
| P7-GSMK-002 | Unknown live session            | No script                          |
| P7-GSMK-003 | Approved script                 | Guard pass                         |
| P7-GSMK-004 | Unapproved claim                | Guard fail                         |
| P7-GSMK-005 | Internal/private data in script | Guard fail                         |
| P7-GSMK-006 | Public price comment            | Route Messenger/QuoteSnapshot path |
| P7-GSMK-007 | No QuoteSnapshot                | No final price                     |
| P7-GSMK-008 | Active recall/sale lock         | No sales CTA                       |
| P7-GSMK-009 | Troll comment                   | Moderation                         |
| P7-GSMK-010 | Real complaint                  | CSKH/human                         |
| P7-GSMK-011 | Live signal spike               | Not revenue/ROAS                   |
| P7-GSMK-012 | Quote created in live           | Funnel signal only                 |
| P7-GSMK-013 | Payment WAITING                 | Not paid/revenue                   |
| P7-GSMK-014 | Runtime unavailable             | Fail-safe/human handoff            |

## Evidence Bat Buoc

- Live session context sample.
- Script source/version.
- Claim Guard trace.
- Comment classification examples.
- Messenger handoff evidence.
- QuoteSnapshot boundary evidence.
- Moderation/complaint evidence.
- Ads-safe live signal evidence.
- Owner review request.

## Report 14 Muc

1. Scope and environment.
2. Source-of-truth checked.
3. Phase entry evidence.
4. Live session context findings.
5. Script runtime findings.
6. Claim Guard findings.
7. Comment classifier findings.
8. Messenger coordination findings.
9. Product/price boundary findings.
10. Troll/complaint route findings.
11. Ads-safe signal findings.
12. Smoke result matrix.
13. Blocker/risk register.
14. Owner decision request.

## Final Status

**PHASE 7 VALIDATION REPORT ONLY**  
**EVIDENCE SUBMITTED ONLY**  
**NOT LIVE READY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P7 - README HANDOFF INDEX

## Update 2026-05-22 - Phase 7 Plan

Dung thu muc chinh thuc: `docs/documents/4. phase/phase-7/`.

Phase 7 la MC AI Live / Live Script Runtime / Live Sales Control / Ads-safe Live Orchestration. MC AI Live la assistant cho host/operator, khong phai source-of-truth ve san pham, gia, ton, order, payment, revenue hay ROAS.

## Entry Gate

Chi duoc bat dau Phase 7 khi:

- Phase 5 Gateway public/private, comment classifier va Messenger handoff co evidence.
- Phase 4 AI Advisor / Final Response Guard co evidence.
- Phase 3 QuoteSnapshot / Order / Payment boundary co evidence.
- Phase 6 Ads signal / Verified Revenue boundary co evidence neu dung ads-safe live orchestration.

Neu chua du, ghi `P7-ENTRY-BLOCKED-BY-GATEWAY-AI-COMMERCE`.

**Global Gateway:** `BLOCKED` den TECH-10.

## Source Of Truth

- TECH-08 MC AI Live.
- PACK-08 MC AI Live.
- TECH-06 Facebook Gateway.
- TECH-07 Ads Measurement.
- TECH-05 AI Advisor.
- TECH-04 Commerce Runtime.
- TECH-11/12 phase governance/backlog.

## Execution Order

1. `00-P7-ANALYSIS-ONLY.md`: inspect live/script/comment/runtime code that.
2. `01-P7-1-TECHNICAL-DESIGN-ONLY.md`: khoa MC AI Live contract.
3. `02-P7-2A-LIVE-SESSION-CONTEXT-RESOLVER.md`: LIVE-BLG-001.
4. `03-P7-2B-LIVE-SCRIPT-RUNTIME-CLAIM-GUARD.md`: LIVE-BLG-002/003.
5. `04-P7-2C-LIVE-COMMENT-CLASSIFIER-MESSENGER-COORDINATION.md`: LIVE-BLG-004/005.
6. `05-P7-2D-PRODUCT-PRICE-BOUNDARY-IN-LIVE.md`: LIVE-BLG-006.
7. `06-P7-2E-TROLL-COMPLAINT-MODERATION-ROUTE.md`: LIVE-BLG-007/008.
8. `07-P7-2F-ADS-SAFE-LIVE-SIGNAL-BOUNDARY.md`: LIVE-BLG-009.
9. `08-P7-2G-SMOKE-EVIDENCE-REPORT.md`: LIVE-BLG-010.

## Core Rules

- MC AI Live khong tu bao gia, khong tu tinh discount, khong tu xac nhan ton.
- MC AI Live khong tu tao order/order_code.
- MC AI Live khong tu xac nhan payment/revenue.
- No QuoteSnapshot -> no final price.
- Script/claim phai qua guard.
- Public/private boundary do Gateway enforce.
- Live signal khong phai revenue/ROAS.
- Sale Lock / Recall / Suppression thang live script/CTA.

## P0 Blockers

- MC AI Live tu bao gia.
- MC AI Live tu tao order.
- MC AI Live tu xac nhan payment/revenue.
- Live signal dung lam ROAS.
- Script chua guard van dung.
- Claim chua approved van noi.
- Troll/malicious keo Messenger sai policy.
- Complaint that bi xu nhu troll.

## Downstream

Phase 8 IVR chi nen bat dau sau khi Commerce Official Order boundary va channel handoff du evidence. Phase 7 smoke pass khong mo Gateway va khong goi production readiness.

**NOT LIVE READY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**
