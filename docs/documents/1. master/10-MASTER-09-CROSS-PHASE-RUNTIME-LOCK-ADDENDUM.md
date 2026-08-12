# MASTER-09 - CUSTOMER / PRODUCT / COMMERCE / AI / FACEBOOK / CRM / FINANCE CROSS-PHASE RUNTIME LOCK ADDENDUM

## Mục Đích

MASTER-09 là bản điều phối sạch toàn hệ sau khi bổ sung nhóm tài liệu Facebook Ads, Live Commerce, AI Advisor Facebook Completion, Financial Baseline, SKU/BOM/G1, Operational Config, phiếu tự sinh, Member/Diamond và IVR.

Tài liệu này khóa hướng đọc, ranh giới source baseline so với technical spec, chuỗi runtime xuyên phase, thứ tự addendum cần viết và trạng thái chặn trước khi dev triển khai đầy đủ.

## Non-Scope

MASTER-09 không:

1. Viết code, API, DTO, database hoặc worker.
2. Tự thay policy decision trong MASTER-08.
3. Mở Gateway production.
4. Gọi Completion Report PASS.
5. Thực hiện Meta App setup, token, webhook thật, MISA sync thật hoặc payout thật.
6. Sửa trực tiếp giá, giờ live, formula, Page ID hoặc evidence ngoài policy refs đã khóa.

## Trạng Thái Toàn Hệ Sau Addendum

| Nhóm | Trạng thái |
| --- | --- |
| Business model clarity | STRONG |
| Source baseline coverage | STRONG_BUT_DISTRIBUTED |
| Technical spec readiness | CLEAN_FOR_LIMITED_HANDOFF |
| Direct implementation readiness | LIMITED_IMPLEMENTATION_HANDOFF_ALLOWED_WITH_EVIDENCE_GATES |
| Gateway production | BLOCKED |
| Required next gate | EVIDENCE_ACCEPTANCE + COMPLETION_REVIEW + RELEASE_SIGNOFF |

## Source Baseline Vs Technical Spec

Các tài liệu mới phải được phân loại trước khi dev đọc:

| Nhóm | Vai trò | Có phải spec code trực tiếp không |
| --- | --- | --- |
| FB-SRC | Business, Ads, Live Commerce, Page list, financial model, implementation sequence | Không. Là source baseline / owner lock. |
| OPS-SRC | Bộ khóa 5 bước, G1 formula, material taxonomy, phiếu lệnh, phiếu tự sinh | Không trực tiếp. Là operational source để viết addendum Phase 1-2. |
| AIA-FB-GATE | Completion Report, Evidence Package, Dev Brief | Không tự mở Gateway. Là gate/evidence spec. |
| PHASE/PACK chính thức | MASTER, PACK, PHASE addendum đã sạch | Có thể làm handoff dev sau khi conflict P0 được xử lý. |

Quy tắc: source baseline có quyền mô tả nghiệp vụ, nhưng implementation chỉ được bắt đầu từ PACK/PHASE/addendum sạch có policy ref và evidence gate rõ.

## Business Engine Lock

Mô hình runtime chính của hệ:

```text
Facebook Ads
  -> Live Giờ Vàng Tri Ân
  -> Comment
  -> Messenger / Commerce Hub
  -> AI Advisor
  -> QuoteSnapshot
  -> Order Verified
  -> CRM / Reorder
  -> Diamond / Finance / MISA
  -> Evidence / Completion / Release
```

Rule nền:

1. Giờ Vàng Tri Ân là revenue engine chính.
2. 24/7 là kênh hỗ trợ CRM, mua lại, khách bỏ lỡ live, reactivation và Diamond referral ngoài live.
3. Ads/ROAS chỉ dùng Order Verified / verified revenue.
4. Comment/inbox/lead/order chưa xác nhận không được tính là doanh thu verified.
5. AI Advisor không ép combo; 1 hộp là Entry Order hợp lệ, 2-3 hộp là Growth Order mục tiêu.
6. Mọi final price phải qua QuoteSnapshot.

## Cross-Phase Runtime Chain

| Step | Owner truth | Consumer | Gate bắt buộc |
| --- | --- | --- | --- |
| Product / SKU / dietary | Product Master / Formula governance | AI, Gateway, Live, Ads, CRM, Commerce | SKU active không đồng nghĩa sellable; vegan claim phải khớp ingredient/formula. |
| Formula / BOM / Operational Config | Product + Operational source | Production, MRP, MISA, Costing | G0 không dùng production; G1 chỉ production khi approved. |
| Sellable availability | Operational / Inventory / Sale Lock | Commerce, AI, Ads, Live, CRM | Recall/Sale Lock/Quality Hold thắng mọi kênh bán. |
| Pricing / Program | Commerce Pricing Core | AI, Gateway, CRM, Live, Ads | Dùng `PRICE_POLICY_V2026_06_CANONICAL_001` và `GOLDEN_HOUR_POLICY_V2026_06_CANONICAL_001`; evidence phải trace được policy ref. |
| QuoteSnapshot | Commerce Runtime | AI, Gateway, Order, CRM | Không QuoteSnapshot thì không final price. |
| Order Verified | Order / Payment / Shipping Core | Ads, ROAS, CRM, Diamond, Finance | Chỉ verified order/revenue mới đi downstream. |
| Ads / ROAS | Ads Measurement | Owner, Dashboard | Không scale nếu attribution/ROAS/order verified chưa PASS. |
| CRM / Member | CRM / Member Lifecycle Core | AI, Messenger, Outbound | Suppression, quiet hours, frequency cap, member policy priority. |
| Diamond | Diamond Referral / Commission Core | Finance, CRM, Dashboard | Referral bind + eligible order + payout policy. |
| MISA / Finance | Accounting Handoff | Finance review | Mapping ACTIVE + Finance checkpoint + evidence; không sync thật nếu Finance/MISA inputs thiếu. |
| Completion | Evidence Registry | Release review | P0 smoke/evidence + owner sign-off. |

## Facebook Hub-Spoke Runtime Lock

Mô hình Facebook phải là:

```text
Traffic / Audience / Live Spoke Page
  -> preserve source_page_id
  -> redirect / handoff về Commerce Hub
  -> Messenger / AI / Quote / Order tại Commerce Hub
  -> attribution giữ source_page_id + commerce_hub_page_id
```

Rule bắt buộc:

1. `Ginsengfood - Cháo Sâm Mỗi Ngày` là Commerce Hub theo source page list hiện tại.
2. Page phụ không có chatbot chốt đơn riêng, không có CRM riêng và không tự quote/order.
3. Messenger/Quote/Order phải lưu `commerce_hub_page_id`.
4. Attribution phải lưu `source_page_id`.
5. Không để nhiều Page cùng nhắn lặp cho một khách.
6. Không dùng raw Page list để bật production; phải qua FB-PAGE-REGISTRY-CLEAN-001.

## Addendum Workstream Chính Thức

| Artifact | Vị trí đề xuất | Nội dung khóa | Input chặn |
| --- | --- | --- | --- |
| COMMERCE-ADDENDUM | Phase 3 | 24/7, Giờ Vàng, Quote Hold, VAT, ship, COD, Diamond candidate, Order Verified | `PRICE_POLICY_V2026_06_CANONICAL_001`, `GOLDEN_HOUR_POLICY_V2026_06_CANONICAL_001`, evidence trace |
| AI-ADVISOR-ADDENDUM | Phase 4 | Customer360, Product Triad, Vegan Guard, segment routing, QuoteSnapshot-only pricing, no SKU wording | `DIETARY_POLICY_V2026_06_CANONICAL_001`, Commerce Addendum |
| FACEBOOK-GATEWAY FB-00 -> FB-08 | Phase 5 | README, Page Registry, webhook normalization, Live/Handoff, Messenger routing, Ads attribution, Diamond referral, ROAS dashboard, security/app review/test/pilot/release | `FB_PAGE_REGISTRY_LOCK_V2026_06_CANONICAL_001`, Gateway Completion P0 evidence |
| ADS-ADDENDUM | Phase 6 | Verified revenue only, CPA/ROAS, scale gate, dashboard evidence | Commerce Order Verified + Ads attribution evidence |
| MC-AI-LIVE-ADDENDUM | Phase 7 | Daily Live Board, script, segment evidence, claim guard, no non-board SKU | Product/Claim/Vegan/GoldenHour policy |
| OPERATIONAL-CONFIG-ADDENDUM | Phase 1-2 | G1/G0, BOM/Recipe Map, packaging, print, QC, release, trace, recall, no manual ingredient selection | `FORMULA_POLICY_V2026_06_CANONICAL_001`, `DIETARY_POLICY_V2026_06_CANONICAL_001` |
| FINANCE-CANONICAL-REGISTRY | Phase 3.1 / PACK-04 | Diamond commission, PIT, payout, verified revenue, MISA handoff | `DIAMOND_SELF_PURCHASE_POLICY_V2026_06_CANONICAL_001`, `MISA_PAYOUT_POLICY_V2026_06_CANONICAL_001` |
| EVIDENCE-SMOKE-MATRIX | PACK-10 / TECH-10 | P0 smoke/evidence for QuoteSnapshot, Order Verified, Gateway, CRM suppression, duplicate/idempotency, ROAS, release sign-off | All P0 policy refs and affected addendums |
| IVR-ADDENDUM | Phase 8 / PACK-09 | Final Golden Hour schedule alignment, attempt policy, queue/capacity, Order Core callback | `GOLDEN_HOUR_POLICY_V2026_06_CANONICAL_001`, `IVR_ATTEMPT_POLICY_V2026_06_CANONICAL_001` |

## Clean Handoff Sequence

```text
1. MASTER-08 System-wide Gap / Impact Matrix + Policy Decision Log
2. MASTER-09 Cross-Phase Runtime Lock Addendum
3. Phase 3 Customer-to-Cash Runtime Addendum
4. Phase 4 AI Advisor Practical Runtime Addendum
5. Phase 5 Facebook Gateway / Messaging Delivery Addendum
6. CRM / Member Lifecycle Canonical
7. Finance / Diamond Commission / Payout Canonical
8. IVR Attempt Policy Cleanup Decision
9. Evidence / Smoke Gate toàn hệ
10. Completion Report evidence review
11. Evidence acceptance / release sign-off
12. Release / Go-live review
```

Không được đảo thứ tự bằng cách code Gateway production trước khi policy ref, QuoteSnapshot, Order Verified, CRM suppression, duplicate/idempotency và evidence gate chưa rõ.

## Phase-Specific Runtime Locks

### Phase 1-2 / Operational

1. Không SKU nào được sản xuất nếu thiếu BOM/Recipe Map approved.
2. Production Order sinh production dossier gốc.
3. Các phiếu sau tự kế thừa dữ liệu từ dossier/BOM snapshot.
4. Lệnh sản xuất và phiếu cấp nguyên liệu không được chọn tay nguyên liệu ngoài snapshot.
5. Mọi thay đổi nguyên liệu đi qua formula version mới.
6. Vegan SKU phải có ingredient/formula proof nhất quán trước khi AI/label/claim dùng.

### Phase 3 / Commerce

1. Commerce owns pricing, program, quote, order, payment, shipping, verified revenue.
2. QuoteSnapshot là nguồn sự thật cho final price.
3. Quote Hold: Giờ Vàng 5 phút, 24/7 15 phút, áp dụng theo selected policy refs và phải trace trong QuoteSnapshot.
4. Order Verified là điều kiện để Ads, CRM, Diamond và Finance dùng doanh thu.
5. Không cộng dồn quyền lợi nếu policy priority chưa rõ.

### Phase 4 / AI Advisor

1. AI là consumer, không phải owner của giá/tồn kho/claim/payment/shipping/order.
2. AI chỉ báo final price từ QuoteSnapshot.
3. AI phải fail-closed nếu vegan/dietary conflict chưa xử lý.
4. AI không dùng mã SKU nội bộ trong customer-facing wording.
5. AI không ép combo; gợi ý Growth Order phải có lý do.

### Phase 5 / Facebook Gateway

1. Gateway chỉ nhận/normalize/dedup route event, không tự quote/order/CRM.
2. Public comment không được lộ final price, PII, payment, health-sensitive detail.
3. Messenger handoff phải preserve context và attribution.
4. Page token/secret không ghi trong tài liệu/evidence.
5. Gateway production chỉ mở sau Completion Report PASS.

### Phase 6-7 / Ads + Live

1. Ads chỉ scale theo verified revenue.
2. ROAS/CPA dashboard là internal-only evidence, không thay owner sign-off.
3. Daily Live Board quyết định SKU/script live.
4. MC AI Live không nói SKU ngoài board hoặc ngoài claim guard.

### Phase 8 / IVR

1. IVR chỉ xác nhận đơn hàng.
2. IVR không bán hàng, không CRM, không tư vấn sản phẩm.
3. SIM Gateway không cập nhật order trực tiếp.
4. Attempt policy hiện hành dùng `IVR_ATTEMPT_POLICY_V2026_06_CANONICAL_001` và Golden Hour schedule đã chọn trong MASTER-08.
5. Real customer call vẫn bị chặn nếu thiếu smoke/evidence/security/owner sign-off.

## Evidence Matrix Bắt Buộc

| Evidence group | Bắt buộc chứng minh |
| --- | --- |
| Decision evidence | P0 policy ref có selected option, effective context và trace evidence. |
| Quote evidence | QuoteSnapshot có price/program/member/shipping/tax/final và expiry. |
| Order evidence | OrderDraft, CustomerConfirmation, order_code, verified state. |
| Gateway evidence | Channel context, public/private boundary, Messenger handoff, duplicate/idempotency. |
| CRM evidence | Suppression, quiet hours, frequency cap, opt-out/open case block. |
| Ads evidence | Order Verified -> ROAS/CPA, no lead/comment revenue. |
| Diamond evidence | Referral bind, eligible order, commission policy, payout boundary. |
| Operational evidence | G1 approval, BOM snapshot, no outside-BOM issue, vegan guard if applicable. |
| IVR evidence | Attempt policy, callback, no direct order update, technical error not counted. |

## Final Done Gate

MASTER-09 đạt yêu cầu khi:

1. Source baseline và technical spec được tách rõ.
2. Runtime chain từ Product/Operational đến Finance/Evidence không còn mơ hồ.
3. P0 conflicts đã được resolve trong MASTER-08 bằng policy refs hoặc fail-closed evidence gate.
4. Phase addendum sequence rõ cho dev.
5. Gateway production vẫn BLOCKED cho đến khi evidence thật PASS.
6. Không có câu nào gọi toàn hệ Production Ready.

## Final Lock

Trạng thái sau MASTER-09:

```text
SYSTEM DOCUMENT STATUS:
STRONG BUSINESS + TECHNICAL FOUNDATION

DIRECT IMPLEMENTATION STATUS:
LIMITED HANDOFF ALLOWED WITH EVIDENCE GATES

GLOBAL GATEWAY:
BLOCKED

NEXT REQUIRED WORK:
RUNTIME EVIDENCE
COMPLETION REPORT REVIEW
RELEASE SIGNOFF
```
