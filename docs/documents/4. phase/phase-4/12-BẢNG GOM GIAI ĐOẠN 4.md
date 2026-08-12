# BẢNG GÔM GIAI ĐOẠN 4 - AI ADVISOR RUNTIME CANONICAL REWRITE

**Phase:** PHASE-04 - AI Advisor Runtime / AI Channel Readiness
**Ngày viết lại:** 2026-06-05
**Trạng thái tài liệu:** `CANONICAL_REWRITE_FOR_OWNER_REVIEW`
**Dev handoff status:** `DEV_IMPLEMENTATION_DOCUMENTATION_READY_WITH_RUNTIME_GATES`
**Runtime implementation status:** `NOT_IMPLEMENTED__EVIDENCE_AND_OWNER_DECISIONS_REQUIRED_AFTER_DEV`
**AI review target:** `READY_TO_HAND_TO_DEV_FOR_CONTROLLED_IMPLEMENTATION`
**Gateway:** `PRODUCTION_BLOCKED`
**Release:** `NOT_RELEASE_READY`

## 0. Khóa chống conflict nội bộ Phase 4

Bản này là canonical rewrite của toàn bộ thư mục Phase 4. Nếu file 00-08, chỉ mục 09 hoặc phụ lục 10 khác bản này về owner, runtime source, pricing, Golden Hour, CRM/Member, Diamond/Finance, Gateway, IVR, public/private, evidence hoặc readiness, áp dụng bản này và xử lý fail-closed.

Các cụm `ANALYSIS ONLY`, `TECHNICAL DESIGN ONLY`, `LIMITED IMPLEMENTATION HANDOFF`, `local-ready`, `DONE`, `READY_FOR_OWNER_REVIEW` hoặc `PASS cục bộ` trong các shard chỉ mô tả phạm vi/gate cục bộ. Chúng có thể dùng để bóc task và giao dev theo lane, nhưng không phải `IMPLEMENTATION_COMPLETE`, không phải `GATEWAY_PASS`, không phải `COMPLETION_PASS`, không phải `RELEASE_READY`, không phải `PRODUCTION_READY` và không phải Go-live.

## 1. Kết luận điều hành

Phase 4 là lớp AI Advisor Runtime. Phase này biến toàn bộ nguồn nghiệp vụ, bổ sung thực chiến, CRM/Member, Diamond/Finance, Facebook Ads/Live, Gateway và evidence gate thành một bộ contract để AI tư vấn đúng, chốt đúng, dừng đúng và handoff đúng.

Phase 4 không phải chatbot tự do. Phase 4 không phải Gateway production. Phase 4 không phải Commerce Core. Phase 4 không phải CRM worker. Phase 4 không phải Diamond payout runtime. Phase 4 không phải Ads optimizer. Phase 4 chỉ là lớp:

- Nhận ngữ cảnh khách/kênh/runtime.
- Tư vấn sản phẩm bằng Product Knowledge public-safe.
- Gợi ý combo theo nhu cầu và ProductRoleMatrix.
- Render câu trả lời sau Final Response Guard.
- Handoff quote/order/payment/shipping/CRM/Diamond/Gateway sang đúng owner.
- Ghi DecisionEnvelope, resolver trace, guard trace và evidence.

Kết luận khóa:

```text
AI Advisor = consumer / orchestrator / safe renderer
AI Advisor != source-of-truth
AI Advisor != Gateway
AI Advisor != Commerce
AI Advisor != Finance
```

## 2. Tài liệu nguồn đã nhập vào Phase 4

### 2.1. Master / Pack / Tech

| Source | Vai trò trong Phase 4 |
| --- | --- |
| `docs/documents/1. master/01-MASTER-00-INDEX-REGISTRY.md` | Registry đọc tài liệu; bảo đảm Phase 4 không tự tách khỏi master index. |
| `docs/documents/1. master/02-MASTER-01-SOURCE-OF-TRUTH.md` | Nguyên tắc source-of-truth: AI không tạo business truth, chỉ consume owner runtime. |
| `docs/documents/1. master/03-MASTER-02-CROSS-PACK-DEPENDENCY.md` | Dependency giữa Commerce, AI Advisor, Gateway, Ads, Live, CRM, Finance/Diamond. |
| `docs/documents/1. master/05-MASTER-04-RUNTIME-RESOLUTION-GUARD.md` | Resolver/guard thứ tự bắt buộc trước khi AI trả lời hoặc handoff. |
| `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md` | Evidence/smoke/completion gate; local smoke không đồng nghĩa completion/release. |
| `docs/documents/1. master/08-MASTER-07-RELEASE-GO-LIVE-CONTROL.md` | Release control; Governance PASS không phải Production Ready. |
| `docs/documents/1. master/09-MASTER-08-CROSS-SYSTEM-DECISION-LOG.md` | Gap/impact, owner decisions, fail-closed stance. |
| `docs/documents/1. master/10-MASTER-09-CROSS-PHASE-RUNTIME-LOCK-ADDENDUM.md` | Runtime chain và thứ tự đọc toàn hệ. |
| `docs/documents/2. pack/05-PACK-05-AI-ADVISOR-CHANNEL.md` | AI Advisor business/channel governance. |
| `docs/documents/3. tech/06-TECH-05-AI-ADVISOR-RUNTIME-PRODUCT-CONSULTING-CUSTOMER-MEMORY-QUOTE-SAFE-SALES-ORDER-DRAFT-HANDOFF.md` | Technical boundary chính của AI Advisor. |
| `docs/documents/2. pack/06-PACK-06-FACEBOOK-CHANNEL-GATEWAY.md` | Downstream channel/Gateway boundary. |
| `docs/documents/3. tech/07-TECH-06-FACEBOOK-GATEWAY-META-LIVE-MESSENGER-CHANNEL-DELIVERY-COMMENT-TO-MESSENGER.md` | Gateway technical owner, không thuộc Phase 4. |
| `docs/documents/2. pack/07-PACK-07-ADS-ROAS-ATTRIBUTION.md` | Ads/ROAS boundary: chỉ đo bằng `ORDER_VERIFIED`, không dùng quote/comment/inbox làm doanh thu. |
| `docs/documents/3. tech/08-TECH-07-ADS-ROAS-PIXEL-CAPI-ATTRIBUTION-SCALE-GATE.md` | Pixel/CAPI/offline conversion/dedup; AI chỉ preserve attribution context. |
| `docs/documents/2. pack/08-PACK-08-MC-AI-LIVE.md` | MC AI Live boundary: live script/brief dùng dữ liệu ngày bán, không tự quote. |
| `docs/documents/3. tech/09-TECH-08-MC-AI-LIVE-SESSION-SCRIPT-LIVE-COMMERCE-COMMENT-ROUTING.md` | Live session/comment routing; Phase 4 chỉ quyết câu trả lời safe và handoff intent. |
| `docs/documents/2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md` | Completion/Evidence gateway; AI Channel chỉ được xét PASS khi evidence accepted. |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-PRICE-PROMOTION-ORDER-PAYMENT-SHIPPING.md` | Upstream Commerce runtime: sellable, quote, order, payment, shipping. |
| `docs/documents/3. tech/11-TECH-10-GLOBAL-SMOKE-EVIDENCE-RELEASE-GATE.md` | Global smoke/release vocabulary; Phase 4 local proof không tự gọi `GLOBAL_SMOKE_PASS`. |
| `docs/documents/3. tech/12-TECH-11-IMPLEMENTATION-ROADMAP.md` | Roadmap sequencing để không nhảy từ AI Advisor sang Gateway production. |
| `docs/documents/3. tech/13-TECH-12-PHASE-BACKLOG.md` | Backlog/owner blockers cần carry-over nếu chưa có quyết định. |

### 2.2. Upstream Phase 3 / Phase 3.1

| Source | Vai trò trong Phase 4 |
| --- | --- |
| `docs/documents/4. phase/phase-3/10-P3-ADDENDUM-CUSTOMER-TO-CASH-RUNTIME-LOCK.md` | DailySalesContext, QuoteSnapshot, order/payment/shipping/verified revenue handoff. |
| `docs/documents/4. phase/phase-3.1/TÀI LIỆU TRIỂN KHAI DIAMOND HOA HỒNG, THƯỞNG MỚI.md` | Phase 3.1 supplement index cho Member/Diamond/CRM runtime. |
| `docs/documents/4. phase/phase-3.1/10-P3_1-ADDENDUM-DIAMOND-CRM-AI-ADVISOR-RUNTIME-LOCK.md` | Cross-phase locks cho Member, Program, Diamond, CRM, Payment, Shipping, IVR. |
| `docs/documents/4. phase/phase-3.1/00-P3_1-ANALYSIS-ONLY.md` đến `08-P3_1-2G-SMOKE-EVIDENCE-REPORT.md` | Workstream handoff cho member/program/diamond/CRM/situation/payment/shipping/IVR evidence. |

### 2.3. Sáu bản bổ sung Phase 3.1

| Source | Vai trò trong Phase 4 |
| --- | --- |
| `4. phase/phase-3.1/5. bo sung/1. BẢN TỔNG HỢP SAU TEST THỰC CHIẾN + CORE POLICY + CRM MESSAGING + MEMBER LIFECYCLE.md` | Core policy, CRM, member lifecycle, QuoteSnapshot, Diamond, resolver/guard registry. |
| `4. phase/phase-3.1/5. bo sung/2. BẢN KHÓA BỔ SUNG CÁC TÌNH HUỐNG ĐỜI THƯỜNG TRƯỚC KHI VIẾT LẠI TÀI LIỆU.md` | Tình huống đời thường, health/gift/office/student/overseas/vegan/payment/shipping/IVR/privacy/fake goods. |
| `4. phase/phase-3.1/5. bo sung/3. bổ sung tin nhắn chăm sóc khác hàng.md` | Source baseline cho tin nhắn chăm sóc, Golden Hour, Diamond referral, commission. |
| `4. phase/phase-3.1/5. bo sung/4. chính sách và hoa hồng thành viên.md` | Member policy, 24/7, Giờ Vàng, Diamond commission, benefit boundaries. |
| `4. phase/phase-3.1/5. bo sung/5. CRM 12-MONTH + MEMBER LIFECYCLE AUTOMATION LOCK.md` | CRM 12 tháng, lifecycle, quiet hours, frequency cap, suppression, P0 tests. |
| `4. phase/phase-3.1/5. bo sung/6. PHỤ LỤC HỆ THỐNG TIN NHẮN.md` | Message appendix/reference; không render trực tiếp nếu chưa canonical hóa. |

### 2.4. Canonical mới

| Source | Vai trò trong Phase 4 |
| --- | --- |
| `docs/documents/6. canonical/01-CANONICAL-CRM-MEMBER-LIFECYCLE-RUNTIME.md` | CRM/member context, message registry, suppression, language guard. |
| `docs/documents/6. canonical/02-CANONICAL-FINANCE-DIAMOND-COMMISSION-PAYOUT-RUNTIME.md` | Buyer benefit vs commission, Diamond eligibility, Finance/MISA/payout boundary. |
| `docs/documents/6. canonical/03-CANONICAL-EVIDENCE-SMOKE-GATE-CUSTOMER-TO-CASH-CARE.md` | Cross-system evidence/smoke gate, status vocabulary, P0 matrix. |

### 2.5. Downstream Phase 5 / Facebook / Ads / Live

| Source | Vai trò trong Phase 4 |
| --- | --- |
| `docs/documents/4. phase/phase-5/10-P5-PHỤ LỤC KHÓA RUNTIME GATEWAY FACEBOOK VÀ NHẮN TIN.md` | Gateway boundary: delivery only, public/private, handoff, guard, P0 smoke. |
| `docs/documents/4. phase/phase-5/TÀI LIỆU BỔ SUNG/AI_ADVISOR_FACEBOOK_COMPLETION_REPORT.md` | Completion report gate; không tự PASS. |
| `docs/documents/4. phase/phase-5/TÀI LIỆU BỔ SUNG/DANH-SACH-FACEBOOK-AI.md` | Page registry baseline; cần normalize trước production. |
| `docs/documents/4. phase/phase-5/TÀI LIỆU BỔ SUNG/Khóa AI Channel đủ điều kiện trước Gateway - CHANNEL FACEBOOK.md` | AI Channel readiness trước Gateway. |
| `docs/documents/4. phase/phase-5/TÀI LIỆU BỔ SUNG/*ADS*`, `*LIVE*`, `*FINANCIAL*` | Business baseline cho Ads/Live/Finance; không dùng làm runtime quote trực tiếp. |

### 2.6. Coverage lock chống thiếu nguồn

Phase 4 được xem là đọc đủ nguồn khi tài liệu triển khai hoặc report về AI Advisor chứng minh đã kiểm ít nhất các cụm sau:

| Cụm nguồn | Bắt buộc phản ánh trong Phase 4 |
| --- | --- |
| Master source-of-truth | Không tự tính giá, tồn, order, paid, revenue, commission, ROAS, payout. |
| Master dependency | Phase 3/3.1/Core upstream thắng AI; Phase 5/Ads/Live downstream không override AI/Commerce. |
| Master runtime guard | Resolver phải chạy trước guard, guard phải pass trước render/handoff. |
| Master evidence/release | `PENDING_EVIDENCE` không được diễn giải thành `PASS`; không gọi `RELEASE_READY` khi chưa có evidence accepted. |
| Pack/Tech AI Advisor | AI là advisor/orchestrator/safe renderer, không phải autonomous sales engine. |
| Pack/Tech Commerce | `DailySalesContext`, `QuoteSnapshot`, `OrderDraft`, `CustomerConfirmation`, `order_code`, payment/shipping state là dữ liệu tiêu thụ, không phải dữ liệu AI tự tạo. |
| Pack/Tech Gateway | Gateway chỉ delivery/handoff; Phase 4 chỉ tạo decision/handoff intent và response candidate đã guard. |
| Pack/Tech Ads | Attribution context được preserve; doanh thu đo bằng `ORDER_VERIFIED` בלבד. |
| Pack/Tech Live | Live/MC AI dùng script brief/ngữ cảnh live; comment hỏi giá/mua/tư vấn sâu đi private. |
| Canonical CRM | Message registry, suppression, opt-out, quiet hour, open case, frequency cap thắng mọi template raw. |
| Canonical Finance/Diamond | Buyer benefit/commission/payout chỉ nói từ Finance/Diamond state; không tự cộng/chốt quyền lợi. |
| Canonical Evidence | Evidence item phải có source, artifact, owner, result, blocker; không có artifact thì chưa đạt. |

## 3. Conflict resolution matrix

| Conflict ID | Nội dung conflict | Cách xử lý trong Phase 4 |
| --- | --- | --- |
| P4-CONF-001 | 24/7 pricing đã resolve về `PRICE_POLICY_V2026_06_CANONICAL_001`. | AI không hardcode bất kỳ tỷ lệ nào. Chỉ nói giá/quyền lợi từ QuoteSnapshot hoặc DailySalesContext có `policy_version`. Nếu thiếu, trả `POLICY_PENDING`. |
| P4-CONF-002 | Golden Hour schedule đã resolve về `GOLDEN_HOUR_POLICY_V2026_06_CANONICAL_001`. | AI không tự nói khung giờ cố định nếu thiếu GoldenHourResolver/policy version. Message reminder chỉ gửi từ CRM/Gateway eligible event. |
| P4-CONF-003 | Member benefit và Diamond buyer benefit đã resolve: Giờ Vàng chỉ early access, không cộng discount trừ policy exception trong QuoteSnapshot. | AI không tự cộng quyền lợi. QuoteSnapshot phải có `applied_benefit_policy` và `policy_priority_decision`. |
| P4-CONF-004 | Diamond commission/payout đã resolve bằng Finance/Diamond Canonical; payout/MISA vẫn cần Finance approval + mapping active + evidence. | AI chỉ nói trạng thái commission theo Finance/Diamond state. Không nói "đã chi trả" nếu chưa `PAYOUT_APPROVED` hoặc `PAID`. |
| P4-CONF-005 | Một số source có thể ghi vegan/chay nhưng formula/ingredient có rủi ro động vật. | AI fail-closed: không claim chay/vegan nếu Product/Dietary proof chưa pass. |
| P4-CONF-006 | File tin nhắn 3 và 6 trùng/lệch và còn dùng "hệ thống". | File 3/6 là baseline. Phase 4 chỉ render qua canonical message registry + CustomerFacingLanguageGuard. |
| P4-CONF-007 | Page list Facebook trộn Page ID/Profile ID/Business ID/App ID. | Phase 4 chỉ preserve channel context. Phase 5 normalized Page Registry mới quyết định production. |
| P4-CONF-008 | Pack 09 IVR cleanup đã áp dụng theo `IVR_ATTEMPT_POLICY_V2026_06_CANONICAL_001`; real call còn cần evidence. | Phase 4 không triển khai IVR. Nếu AI nhắc order confirmation, chỉ nói trạng thái safe từ Order/IVR runtime; IVR real call blocked đến khi IVR evidence pass. |
| P4-CONF-009 | Business baseline có giá/cost/ROAS/commission cụ thể. | Các số này là internal/source baseline, không customer-facing quote. AI chỉ quote từ runtime. |
| P4-CONF-010 | Completion report/Gateway docs có thể tạo cảm giác đã sẵn sàng. | Mọi gate mặc định `PENDING_EVIDENCE`. Không có evidence accepted thì không PASS, không Release Ready, không Production Ready. |

## 4. Phase 4 source-of-truth chain

```text
Product / Formula / Public Product Knowledge
  -> Operational Sellable / Recall / Sale Lock / Public-safe Trace
  -> Commerce DailySalesContext / QuoteSnapshot / Order / Payment / Shipping / ORDER_VERIFIED
  -> Phase 3.1 Member / Program / Diamond / CRM policy runtime
  -> Phase 4 AI Advisor Runtime
  -> Phase 5 Gateway / Messenger / Live / CRM delivery
  -> Ads / ROAS / Diamond / Finance / IVR / Evidence
```

Không có đường tắt:

```text
AI prompt -> price
AI prompt -> stock
AI prompt -> member benefit
AI prompt -> Diamond commission
AI prompt -> official order
AI prompt -> paid
AI prompt -> verified revenue
AI prompt -> Gateway production
```

## 5. Phase 4 owns / does not own

| Nhóm | Phase 4 sở hữu | Phase 4 không sở hữu |
| --- | --- | --- |
| AI behavior | Intent, context resolution, recommendation, response rendering, Final Response Guard, handoff decision. | Product truth, inventory truth, price truth, payment truth, delivery truth. |
| Product advice | Product public-safe view, ProductRoleMatrix consumption, explain reason. | Formula/BOM/cost/internal SKU/QC/supplier/ingredient decision. |
| Sales support | Quote request, quote rendering from QuoteSnapshot, Order Draft rendering, CustomerConfirmation prompt. | Final price calculation, official order creation, order_code generation. |
| Payment support | Public-safe payment state response and approved instruction rendering. | Mark paid, reconcile bank transfer, hardcode bank account. |
| CRM support | Decide AI wording eligibility and outbound candidate handoff. | CRM scheduler/worker delivery, lifecycle truth, payout message truth. |
| Channel | Decide public/private-safe content and handoff intent. | Meta App, webhook, token, Page production, Messenger delivery infra. |
| Evidence | DecisionEnvelope, resolver trace, guard trace, AI evidence item. | Global release approval, owner sign-off, production readiness decision. |

## 6. Global status locks

| Scope | Status |
| --- | --- |
| Phase 4 documentation | `CANONICAL_REWRITE_FOR_OWNER_REVIEW` |
| Direct code implementation | `BLOCKED_UNTIL_ANALYSIS_AND_DESIGN_ACCEPTED` |
| AI channel completion | `PENDING_EVIDENCE` |
| Gateway production | `PRODUCTION_BLOCKED` |
| Release readiness | `NOT_RELEASE_READY` |
| Go-live | `NOT_ALLOWED` |

Các status cấm dùng sau Phase 4 nếu chưa có evidence thật:

- `GATEWAY_PASS`
- `GLOBAL_SMOKE_PASS`
- `COMPLETION_PASS`
- `RELEASE_READY`
- `PRODUCTION_READY`
- `GO_LIVE_APPROVED`

## 7. Runtime input contracts bắt buộc

### 7.1. AIAdvisorRequestEnvelope

```yaml
AIAdvisorRequestEnvelope:
  request_id: required
  correlation_id: required
  idempotency_key: required_if_side_effect_possible
  occurred_at: required
  channel_context: ChannelContext
  customer_context_ref: required_or_unknown
  incoming_text_or_event_ref: required
  requested_action:
    - consult_product
    - ask_price
    - request_quote
    - create_order_draft
    - confirm_order
    - ask_payment
    - ask_shipping_eta
    - ask_member_benefit
    - ask_diamond
    - crm_reply
    - complaint
    - privacy_request
    - human_handoff
```

### 7.2. ChannelContext

```yaml
ChannelContext:
  channel_code: web_chat | messenger | live_comment | public_comment | crm_outbound | admin_preview | unknown
  reply_surface: public | private | internal_preview
  source_page_id: optional
  commerce_hub_page_id: optional
  live_session_id: optional
  comment_id: optional
  thread_id: optional
  ads_context_ref: optional
  referral_context_ref: optional
  channel_permission_state: allowed | blocked | unknown
  production_allowed: boolean
```

Rules:

- Public surface không được chứa PII, payment, final personal quote, order state cá nhân, member tier, Diamond commission hoặc health-sensitive advice.
- Admin preview không được gửi outbound thật.
- Channel permission unknown thì không gửi response production.

### 7.3. DailySalesContext

```yaml
DailySalesContext:
  business_date: required
  channel: required
  source_page_id_if_facebook: optional
  live_session_id_if_live: optional
  active_programs: list
  program_time_window: required_if_program_active
  program_policy_version: required_if_program_active
  product_public_name: required
  public_category: required
  dietary_type: vegan | non_vegan | mixed | unknown
  target_roles: list
  listed_price_display: required_if_sellable
  program_price_display: required_if_program_eligible
  member_benefit_eligible: boolean
  diamond_benefit_eligible: boolean
  sellable_status: sellable | low_stock | sold_out | blocked | unknown
  public_stock_state: available | low | sold_out | not_sellable | unknown
  quote_eligible: boolean
  quote_hold_minutes: required_if_quote_eligible
  policy_version_refs: list
```

Fail-closed:

| Thiếu | AI phải làm |
| --- | --- |
| `sellable_status` | Không nói còn hàng. |
| `program_policy_version` | Không nói Giờ Vàng/24/7 đang active. |
| `QuoteSnapshot` | Không nói final total. |
| `product_public_name` | Không tư vấn sản phẩm đó. |
| `policy_version_refs` | Không nói quyền lợi member/Diamond. |

### 7.4. CustomerAdvisoryContext

```yaml
CustomerAdvisoryContext:
  identity:
    customer_type: guest | new_unregistered_buyer | registered_new | returning_buyer | member | diamond | bulk_buyer_candidate | corporate_gift_buyer | distributor_candidate | agency_candidate | unknown
    verified_contact_level: none | channel_only | phone_verified | account_verified
    channel_identity: required
  purchase_memory:
    has_verified_purchase: boolean
    last_purchase_date_display: optional_public_safe
    last_purchase_product_public_names: list
    last_purchase_recipient_profile: optional_public_safe
    last_purchase_purpose: optional_public_safe
    last_feedback_summary_public_safe: optional
  member_memory:
    member_tier_display: optional
    accumulated_valid_revenue_display: optional
    amount_to_next_tier_display: optional
    amount_to_maintain_tier_display: optional
    days_to_cycle_end_display: optional
    grace_status: none | active | recovered | expired | unknown
  guards:
    privacy_allowed: boolean
    opt_out_status: opted_in | opted_out | unknown
    open_case_status: none | complaint | refund | payment | privacy | quality | unknown
    dietary_allergy_flags: list
    bulk_purchase_signal: none | family_bulk | corporate_gift | distributor_or_agency | unknown
    primary_user_profile: self | spouse | parent_elderly | child | family | gift_recipient | unknown
```

Rules:

- Khách mới: không nói "lần trước".
- Khách cũ: chỉ nhắc lịch sử ở private channel và public-safe.
- Member/Diamond: chỉ nói quyền lợi từ runtime.
- Open case hoặc opt-out: dừng sales/CRM, route support/human.
- Mua nhiều/corporate gift không đồng nghĩa đại lý hoặc nhà phân phối; phải tách bằng BulkBuyIntentResolver, DistributorIntentGuard và BulkQuoteResolver trước khi báo giá/chuyển tuyến.

### 7.5. ProductRoleMatrix

```yaml
ProductRoleMatrixItem:
  product_public_name: required
  public_category: required
  pillar_role: seasonal | functional | nourishing | gift | unknown
  dietary_type: vegan | non_vegan | mixed | unknown
  suitable_for:
    - elderly
    - child
    - office_worker
    - travel
    - gift
    - overseas_vietnamese
    - student
    - international_student
    - recovery
    - family
  gender_family_addon:
    female_context: optional_public_safe
    male_context: optional_public_safe
    elderly_context: optional_public_safe
  gender_age_self_addon:
    female_self_context: optional_public_safe
    male_self_context: optional_public_safe
    elderly_calcium_mineral_context: optional_public_safe
  eastern_effectiveness_summary: required_public_safe
  taste_sensory_summary: optional
  avoid_if: list
  substitute_products: list
  claim_policy_refs: list
```

Rules:

- Không có ProductRoleMatrix thì AI không được tự đoán role của sản phẩm.
- Vegan/chay phải có dietary proof; nếu conflict thì block.
- Trẻ em, người già, thai kỳ, bệnh nền, dị ứng phải qua HealthSafetyGuard.
- Nữ hỏi cho bản thân: chỉ được đề xuất thêm nhóm dưỡng mô/sắc vóc/bồi bổ nhẹ nhàng khi ProductRoleMatrix và HealthSafetyGuard pass; không claim điều trị/nội tiết.
- Nam hỏi cho bản thân: chỉ được đề xuất thêm nhóm trợ dương/sinh lực public-safe khi ProductRoleMatrix và HealthSafetyGuard pass; không nói chữa yếu sinh lý, tăng testosterone hoặc cam kết hiệu quả.
- Người già/ba mẹ/người lớn tuổi: chỉ được đề xuất thêm nhóm canxi/khoáng hóa/chăm sóc xương khớp public-safe khi có proof; không nói chữa loãng xương hoặc thay thuốc.
- Nếu chưa rõ người dùng chính là ai, AI hỏi một câu ngắn hoặc bỏ qua add-on; không đoán giới tính/tuổi từ tên, giọng văn hoặc món đang hỏi.

### 7.6. QuoteSnapshotConsumerView

```yaml
QuoteSnapshotConsumerView:
  quote_snapshot_id: internal_ref
  quote_display_code: optional_public_safe
  quote_status: active | expired | voided | converted
  quote_created_at: required
  quote_expires_at: required
  quote_hold_minutes: required
  line_items:
    - product_public_name
    - quantity
    - listed_price_display
    - program_price_display
    - member_benefit_display
    - diamond_benefit_display
    - line_total_display
    - quote_line_reason_public_safe
  shipping_fee_display: required_or_pending
  cod_fee_display: required_or_pending
  vat_display: required_or_not_applicable
  final_total_display: required_when_complete
  applied_benefit_policy: required
  policy_version_refs: list
```

Rules:

- QuoteSnapshot là immutable source cho final price.
- Quote expired thì AI chỉ được yêu cầu revalidate.
- AI không tự thêm ship/COD/VAT/discount.
- Customer-facing không nói `QuoteSnapshot`; nói "báo giá của Mình".

### 7.7. OrderRuntimeConsumerView

```yaml
OrderRuntimeConsumerView:
  order_stage:
    - CUSTOMER_INTERESTED
    - QUOTE_REQUESTED
    - QUOTE_SNAPSHOT_CREATED
    - ORDER_DRAFT_RENDERED
    - CUSTOMER_CONFIRMED
    - ORDER_CODE_CREATED
    - PAYMENT_PENDING
    - PAYMENT_CONFIRMED
    - READY_FOR_FULFILLMENT
    - WAREHOUSE_RELEASED
    - HANDED_TO_CARRIER
    - IN_DELIVERY
    - DELIVERED
    - DELIVERY_EXCEPTION
    - CANCELLED
    - ORDER_VERIFIED
  order_code_display: required_if_order_code_created
  payment_public_state: optional
  shipping_public_state: optional
  editable_fields: optional
  next_customer_action: optional
```

Forbidden claims:

- Không nói đơn đã ghi nhận nếu chưa `ORDER_CODE_CREATED`.
- Không nói paid nếu chưa `PAYMENT_CONFIRMED`.
- Không nói doanh thu verified với khách.
- Không nói đã xuất kho/giao hàng nếu shipping runtime chưa xác nhận.

### 7.8. PaymentPublicResponsePolicy

| Payment state | AI được nói | AI không được nói |
| --- | --- | --- |
| `PAYMENT_PENDING` | "Đơn đang chờ thanh toán/xác nhận." | "Đã thanh toán." |
| `BANK_TRANSFER_PENDING` | Hướng dẫn chuyển khoản từ payment instruction approved. | Hardcode tài khoản hoặc xác nhận paid. |
| `BANK_TRANSFER_SUBMITTED_BY_CUSTOMER` | "Em đã ghi nhận thông tin Mình gửi và đang chờ xác nhận." | "Ảnh này đã đủ để hoàn tất thanh toán." |
| `PAYMENT_RECONCILING` | "Thanh toán đang được đối chiếu." | Đổ lỗi hoặc hứa thời gian không có SLA. |
| `PAID_BY_BANK_TRANSFER` | Chỉ nói đã thanh toán khi runtime trả confirmed. | Tự xác nhận từ tin nhắn khách. |
| `REFUND_PENDING` | Route support/refund policy. | Tự hứa hoàn tiền. |

### 7.9. ShippingETAResponsePolicy

| State | AI action |
| --- | --- |
| `SHIPPING_INFO_MISSING` | Hỏi thông tin tối thiểu ở private channel. |
| `SHIPPING_INFO_AVAILABLE_NO_TRACKING` | Trả ETA fallback theo vùng nếu policy cho phép. |
| `TRACKING_AVAILABLE` | Trả trạng thái vận chuyển public-safe. |
| `HANDED_TO_CARRIER` | Chỉ nói đã bàn giao vận chuyển nếu runtime xác nhận. |
| `DELIVERY_EXCEPTION` | Route support, không đổ lỗi khách/đơn vị vận chuyển. |

Rules:

- Không hỏi lại địa chỉ nếu order đã có shipping info.
- Không hứa international shipping nếu owner-approved extension chưa có.
- Không public mã đơn, địa chỉ, số điện thoại hoặc trạng thái thanh toán ở comment.

### 7.10. CRMMemberContext

Phase 4 dùng context này để render/handoff CRM-safe content, không tự chạy CRM worker.

```yaml
CRMMemberContext:
  customer_identity_status: guest | known | verified
  verified_purchase_status: boolean
  member_status: non_member | member | diamond | suspended | unknown
  member_tier_display: optional
  cycle_start_date: optional
  cycle_end_date: optional
  days_to_cycle_end: optional
  accumulated_valid_revenue_display: optional
  amount_to_next_tier_display: optional
  amount_to_maintain_tier_display: optional
  grace_status: none | active | recovered | expired | unknown
  opt_out_status: required
  open_case_status: required
  quiet_hour_status: required
  frequency_cap_status: required
```

Rules:

- CRM không phải tin nhắn khuyến mãi tùy tiện.
- CRM chỉ chạy khi trigger thật, data thật, suppression pass.
- Không bịa hạng, doanh số, số còn thiếu, ngày còn lại.
- Không gửi khi opt-out/open case/quiet hour/frequency cap/fatigue fail.

### 7.11. DiamondReferralContext

```yaml
DiamondReferralContext:
  referral_link_id: optional
  referral_bind_status: none | candidate | confirmed | rejected | pending_review
  buyer_benefit_status: none | candidate | applied_in_quote | rejected
  commission_state:
    - not_started
    - candidate
    - eligible
    - finance_review
    - payout_approved
    - paid
    - reversed
  verified_revenue_ref: required_for_commission_eligible
  finance_review_status: pending | approved | rejected | blocked
```

Rules:

- Buyer benefit không đồng nghĩa Diamond commission.
- Commission candidate không đồng nghĩa payable.
- AI không nói "hoa hồng đã ghi nhận/đã chi trả" nếu Finance state chưa pass.
- Self-purchase conflict pending thì route review.

## 8. Recommendation decision tree

Thứ tự quyết định bắt buộc:

1. Resolve channel and reply surface.
2. Resolve customer identity and privacy permission.
3. Detect open case: complaint, refund, payment, privacy, quality, fake goods.
4. Resolve purchase/member/Diamond context.
5. Detect need: self-use, parent, child, gift, office, student, overseas, travel, vegan, recovery, health-sensitive.
6. Apply health/child/pregnancy/allergy/dietary guard.
7. Resolve DailySalesContext.
8. Resolve ProductRoleMatrix.
9. Resolve GenderAgeAddOnRule nếu đã biết người dùng chính là nữ/nam/người già.
10. Resolve BulkDistributorIntentRule nếu có tín hiệu mua nhiều, quà công ty, mua sỉ, đại lý hoặc nhà phân phối.
11. Filter by sellable/sale-lock/recall/channel suppression.
12. Rank by customer need, product role, public-safe effectiveness, taste, history, channel fit.
13. Suggest 1 product or 2-3 item combo only with reason.
14. If price/buy/quote intent: request QuoteSnapshot.
15. If customer confirms: render Order Draft and CustomerConfirmation path.
16. If order/payment/shipping/status question: consume runtime state only.
17. If CRM/after-sales: check opt-out, quiet hours, frequency cap, open case.
18. Apply Final Response Guard.
19. Emit DecisionEnvelope and evidence.

Priority:

```text
health safety
> privacy / open case
> dietary / allergy
> sellable / recall / sale lock
> customer need
> product role fit
> customer history
> inventory priority
> business campaign
```

Inventory priority and campaign urgency never override safety, privacy, dietary or sellable.

## 9. Situation locks from real-life supplement

| Situation | Phase 4 behavior |
| --- | --- |
| Health concern / disease / medicine | Use health-sensitive template, no treatment claim, recommend consulting professional when needed. |
| Gift for parents/elderly | Use ProductRoleMatrix elderly/gift/public-safe effectiveness, no medical guarantee. |
| Female self-care | If the user is female and buying for herself, add dưỡng mô/sắc vóc only from ProductRoleMatrix proof; no treatment or hormone claim. |
| Male self-care | If the user is male and buying for himself, add trợ dương/sinh lực only in public-safe wording; no sexual-health diagnosis or guaranteed effect. |
| Elderly calcium/mineral | For elderly/parents context, add canxi/khoáng hóa/chăm sóc xương khớp only with proof; no osteoporosis/treatment claim. |
| Child / student | Child-sensitive guard; only approved products; no adult dosage or medical claim. |
| Office worker | Suggest convenient, light, nourishing roles if sellable and public-safe. |
| Overseas / international student / travel | Check shipping/export eligibility; do not promise international shipping if not approved. |
| Vegan / chay | Only suggest products with dietary proof; conflict -> block. |
| Inventory priority | Use only after safety/dietary/sellable; never push locked stock. |
| IVR / fake order prevention | AI only explains confirmation status from runtime; IVR call flow belongs Phase 8. |
| Shipping ETA | Use ShippingETAResponsePolicy; no repeated address request if already present. |
| Bank transfer / VietQR | Render approved payment instruction; no PAID claim before confirmation. |
| Usage / storage | Use approved product usage data; no invented instructions. |
| Return/refund | Route support/policy; no promise outside runtime. |
| Order edit / VAT | Consume editable fields and invoice/tax runtime; no public PII. |
| Bulk buy / corporate gift / distributor | Family bulk/corporate gift uses bulk quote/corporate support flow; đại lý/nhà phân phối/mua sỉ routes distributor guard/human, not normal retail quote. |
| Privacy / opt-out | Stop CRM and open privacy/support route if required. |
| Fake goods / anti-counterfeit | Route anti-counterfeit/human support; no public accusation. |

## 10. Public/private and language guard

### 10.1. Customer-facing banned terms

Customer-facing output must not contain:

- hệ thống
- SKU
- runtime
- resolver
- core
- gateway
- QuoteSnapshot
- internal
- nội bộ
- cost
- supplier
- MISA
- audit
- API
- worker
- ROAS
- CPA
- campaign id
- Page Access Token
- App Secret
- business verification internals

### 10.2. Replacement style

| Internal phrase | Customer-facing replacement |
| --- | --- |
| hệ thống ghi nhận | Ginsengfood ghi nhận / bên Em ghi nhận |
| hệ thống kiểm tra | Em kiểm tra theo thông tin hiện tại |
| SKU | tên sản phẩm công khai |
| QuoteSnapshot | báo giá của Mình |
| runtime chưa có dữ liệu | Em cần kiểm tra thêm trước khi xác nhận |
| resolver chưa pass | hiện chưa đủ điều kiện để xác nhận |
| MISA/kế toán | bộ phận phụ trách / bộ phận xác nhận |

### 10.3. Channel surface rule

| Surface | Allowed | Blocked |
| --- | --- | --- |
| Public comment/live comment | Safe acknowledgement, general product info, private handoff. | Final price, PII, payment, order status, member tier, Diamond commission, health-sensitive details. |
| Messenger/private | Personalized consult, quote/order if runtime and guard pass. | Self-calculation, overclaim, unapproved policy. |
| CRM outbound | Registry message after suppression pass. | Free-form rewrite, spam, opt-out/open case. |
| Admin preview | Internal review only. | Real outbound command. |

## 11. Final Response Guard

Every customer-facing response must pass:

```yaml
FinalResponseGuard:
  input_checks:
    - source_truth_present
    - channel_surface_valid
    - customer_privacy_allowed
    - product_public_safe
    - claim_policy_pass
    - health_safety_pass
    - sellable_or_non_sales_mode
    - quote_snapshot_required_if_price
    - order_state_required_if_order
    - payment_state_required_if_payment
    - shipping_state_required_if_shipping
    - crm_suppression_pass_if_crm
    - diamond_finance_state_pass_if_commission
    - no_banned_customer_terms
    - no_secret_or_pii_leak
  output:
    status: PASS | BLOCK | REWRITE_REQUIRED | HUMAN_HANDOFF
    guard_trace_id: required
    block_reason: required_if_not_PASS
```

Guard unavailable means `BLOCK_OR_HUMAN_HANDOFF`, not "send anyway".

## 12. Typing / pacing policy

```yaml
TypingDelayPolicy:
  enabled_if:
    - channel_supports_typing_indicator = true
    - channel_policy_allows_delay = true
    - message_is_customer_facing = true
  base_ms: 1200
  per_character_ms: 28
  min_ms: 1500
  max_ms: 9000
  long_message_chunk_threshold_chars: 650
  cancel_if_customer_sends_new_message: true
  bypass_for:
    - complaint
    - payment_issue
    - privacy_case
    - health_sensitive
    - delivery_exception
```

Rules:

- Public comment không giả lập typing nếu channel policy không cho.
- Customer sends new message -> cancel and recompute.
- Complaint/payment/privacy/health/delivery exception ưu tiên nhanh, không tạo cảm giác câu giờ.
- Log `typing_delay_decision_ms` vào evidence nếu dùng.

## 13. CRM / Member rendering lock

Phase 4 may prepare CRM response candidates, but CRM delivery belongs CRM/Gateway runtime.

Every CRM message candidate must include:

```yaml
MessageRegistryItem:
  message_code: required
  canonical_source: required
  trigger: required
  lifecycle_stage: optional
  eligible_segment: required
  required_variables: list
  fallback_if_missing_variable: required
  channel_scope: public | private | notification
  quiet_hour_policy: required
  frequency_cap: required
  suppression_guards:
    - opt_out
    - open_case
    - fatigue
    - quiet_hours
    - duplicate
  language_guard_status: required
  evidence_required: required
```

Rules:

- File 3 and file 6 message text are not direct runtime templates.
- Canonical registry wins over raw supplement text.
- If variables missing, use fallback; do not invent.
- Downgrade/grace message must be soft, no shame.
- Diamond commission message must follow Finance/Diamond state.

## 14. Finance / Diamond rendering lock

| Finance/Diamond state | AI allowed wording | AI blocked wording |
| --- | --- | --- |
| Referral candidate | "Đơn giới thiệu đang được kiểm tra theo chính sách." | "Hoa hồng đã ghi nhận." |
| Commission eligible | "Hoa hồng đủ điều kiện theo chính sách hiện hành." | "Đã chi trả." |
| Finance review | "Bộ phận phụ trách đang rà soát." | MISA/audit/accounting details. |
| Payout approved/paid | Only if runtime confirms. | Guessing payout date/amount. |
| Reversal/refund | Public-safe reason if policy allows. | Blame or internal accounting detail. |

AI must not:

- Calculate commission.
- Decide self-purchase eligibility.
- Execute payout.
- Sync MISA.
- Tell customer Finance internals.

## 15. Ads / Live / Gateway lock

### 15.1. Ads

AI can preserve internal ads context but must not:

- Public campaign/adset/ad creative IDs.
- Public CPA/ROAS/AOV.
- Treat lead/comment/inbox/quote/order draft as revenue.
- Suggest scaling based on unverified order.

Ads/ROAS only consume `ORDER_VERIFIED` / verified revenue.

### 15.2. Live / MC AI

AI can support live only when:

- Daily Live Product Board is present.
- Segment/script is approved.
- Product still sellable.
- Claim guard passes.
- Price is from QuoteSnapshot/private flow.

AI must not:

- Public final personal quote in live.
- Mention products outside board as live offer.
- Fake scarcity.
- Use unapproved MC segment.

### 15.3. Gateway

Gateway is downstream delivery.

Phase 4 must hand off:

- safe response candidate.
- guard trace.
- handoff decision.
- channel context.
- idempotency key.
- evidence refs.

Phase 4 must not:

- Configure Meta App.
- Store Page Access Token.
- Open webhook production.
- Send Gateway outbound when P5 production blocked.

## 16. AI decision and evidence objects

### 16.1. DecisionEnvelope

```yaml
DecisionEnvelope:
  decision_id: required
  request_id: required
  correlation_id: required
  channel_context_ref: required
  customer_context_ref: required_or_unknown
  intent_classification: required
  source_truth_refs:
    - product_public_view_ref
    - daily_sales_context_ref
    - quote_snapshot_ref
    - order_runtime_ref
    - crm_member_context_ref
    - diamond_context_ref
  resolver_trace_id: required
  guard_trace_id: required
  decision_type:
    - RESPOND
    - ASK_CLARIFYING_QUESTION
    - REQUEST_QUOTE
    - RENDER_QUOTE
    - RENDER_ORDER_DRAFT
    - REQUEST_CUSTOMER_CONFIRMATION
    - HANDOFF_MESSENGER
    - HANDOFF_HUMAN
    - SUPPRESS
    - BLOCK
  output_surface: public | private | internal_preview
  evidence_id: required
```

### 16.2. Evidence item

```yaml
Phase4EvidenceItem:
  evidence_id: required
  gate_code: required
  p0_status: PASS | FAIL | PENDING | BLOCKED
  tested_at: required
  tested_by: required
  source_docs: list
  fixture_ref: required
  command_or_test_ref: required
  runtime_artifacts:
    - DecisionEnvelope
    - resolver_trace
    - guard_trace
    - quote_snapshot_if_price
    - customer_confirmation_if_order
    - order_code_if_success
    - handoff_delivery_log_if_handoff
    - idempotency_key_if_side_effect
  pii_mask_status: required
  secret_scan_status: required
  owner_review_status: pending | accepted | rejected
```

Evidence cannot be a narrative statement only. It needs artifact/log/fixture/trace.

## 17. Phase 4 execution shards

| File | Role after canonical rewrite | Must include from this Bảng Gôm |
| --- | --- | --- |
| `00-PHÂN TÍCH HIỆN TRẠNG.md` | Inspect current code/docs only. | Source map, gap/conflict matrix, no implementation. |
| `01-THIẾT KẾ KỸ THUẬT.md` | Design boundaries only. | Runtime contracts, DecisionEnvelope, guard chain, evidence model. |
| `02-HỢP ĐỒNG TIÊU THỤ DỮ LIỆU THỜI GIAN CHẠY.md` | Runtime consumer handoff. | DailySalesContext, QuoteSnapshot view, source truth chain. |
| `03-BỘ NHỚ KHÁCH HÀNG VÀ BỘ GIẢI NGỮ CẢNH.md` | Customer context. | CustomerAdvisoryContext, CRMMemberContext, privacy/open-case rules. |
| `04-ĐIỀU PHỐI TƯ VẤN KIẾN THỨC SẢN PHẨM.md` | Product recommendation. | ProductRoleMatrix, decision tree, situation locks, vegan/health guards. |
| `05-TIÊU THỤ BÁO GIÁ NHÁP VÀ XÁC NHẬN ĐƠN.md` | Quote/order consumer. | QuoteSnapshot, OrderRuntimeConsumerView, payment/shipping states. |
| `06-CHẶN AN TOÀN NGÔN TỪ CÔNG KHAI VÀ TUYÊN BỐ.md` | Final guard. | Banned terms, public/private, health claim, PII/secret guard. |
| `07-CẦU NỐI BÀN GIAO KÊNH MESSENGER TRỰC TIẾP VÀ CRM.md` | Handoff bridge. | ChannelContext, Gateway boundary, typing/pacing, CRM/Live handoff. |
| `08-BÁO CÁO KIỂM THỬ KHÓI VÀ BẰNG CHỨNG.md` | Smoke/evidence/report only. | P0 gates, evidence object, status vocabulary, owner review. |
| `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md` | Practical runtime addendum. | Compact lock for dev before implementation. |
| `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` | SRS/dev handoff. | Requirement traceability, NFR, work packages, implementation guardrails. |

## 18. Phase 4 P0 gates

| Gate | Scope | PASS requires |
| --- | --- | --- |
| P4-GATE-001 | Source truth | AI never self-calculates product, stock, price, benefit, order, payment, shipping, revenue, commission or payout. |
| P4-GATE-002 | DailySalesContext | Today-selling/price/program/stock answers use DailySalesContext + QuoteSnapshot if final price. |
| P4-GATE-003 | CustomerAdvisoryContext | New/returning/member/Diamond/open-case/opt-out context resolved without privacy leak or guessing. |
| P4-GATE-004 | ProductRoleMatrix | Recommendation uses role matrix, sellable, dietary, health and claim guard. |
| P4-GATE-005 | QuoteSnapshot | Final price only from QuoteSnapshot; expired quote revalidates. |
| P4-GATE-006 | Order state | Order success only after order_code; retry idempotent. |
| P4-GATE-007 | Payment | AI never says paid before Payment Core/Accounting confirmation. |
| P4-GATE-008 | Shipping/ETA | AI only says shipping/tracking state from Delivery Runtime/fallback policy. |
| P4-GATE-009 | CRM/Member | Suppression, quiet hours, frequency cap, open case, opt-out pass before CRM candidate. |
| P4-GATE-010 | Diamond/Finance | Commission/payout wording follows Finance/Diamond state and verified revenue. |
| P4-GATE-011 | Public/private | Public comment/live never leaks final personal quote, PII, payment, member tier, order or health-sensitive details. |
| P4-GATE-012 | Language guard | No banned internal terms in customer-facing output. |
| P4-GATE-013 | Typing/pacing | Delay policy, max cap, cancel/recompute and bypass contexts evidence. |
| P4-GATE-014 | Gateway handoff | Gateway remains blocked; handoff includes safe response and trace only. |
| P4-GATE-015 | Evidence | Every P0 has evidence artifact, no PII/secret, owner review status. |
| P4-GATE-016 | Gender/age add-on | Female self, male self and elderly calcium/mineral add-ons use ProductRoleMatrix proof, HealthSafetyGuard and no treatment claim. |
| P4-GATE-017 | Bulk/distributor split | Bulk buyer, corporate gift, agency/distributor and mua sỉ intents are separated before quote/router; no automatic discount or false distributor classification. |
| P4-GATE-018 | Order success care | Customer-facing order success message renders only after order_code and uses approved post-purchase care wording. |
| P4-GATE-019 | Runtime provider evidence | DailySalesContext, ProductRoleMatrix, QuoteSnapshot, OrderDraft, ShippingState, CRMJobLog and guard trace have real provider evidence before any PASS claim. |

## 19. Smoke matrix

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P4-SMK-001 | Khách hỏi "hôm nay bán gì" | Public sellable products only, no internal SKU. |
| P4-SMK-002 | Khách hỏi giá trong Messenger | QuoteSnapshot or fail-safe. |
| P4-SMK-003 | Khách hỏi giá ở public comment/live | Safe ack + private handoff, no final price. |
| P4-SMK-004 | Golden Hour policy missing | No hardcoded time/discount; `POLICY_PENDING`. |
| P4-SMK-005 | Quote expired | Revalidation required; no order from expired quote. |
| P4-SMK-006 | Product sale lock/recall | No sale, no combo, safe alternative only if allowed. |
| P4-SMK-007 | Vegan customer with conflicting product | Block non-proof vegan recommendation. |
| P4-SMK-008 | Customer has previous purchase | Private public-safe memory use; no public leak. |
| P4-SMK-009 | New customer | No fake history/member benefit. |
| P4-SMK-010 | Member runtime missing | No invented tier/revenue/remaining amount. |
| P4-SMK-011 | Diamond referral candidate | No commission/payout wording. |
| P4-SMK-012 | Bank transfer image sent | Acknowledge review, no paid claim. |
| P4-SMK-013 | Shipping info already present | No repeated address ask; ETA from runtime/fallback. |
| P4-SMK-014 | Open complaint/refund/privacy case | Stop sales and CRM, route support/human. |
| P4-SMK-015 | Opt-out request | Suppress CRM and route privacy/consent. |
| P4-SMK-016 | CRM message includes "hệ thống" | Language guard blocks/rewrite required. |
| P4-SMK-017 | Duplicate webhook/comment | One decision/evidence, no duplicate response/order. |
| P4-SMK-018 | Live board missing | No live offer/product claim beyond safe generic. |
| P4-SMK-019 | Ads context present | Preserve internal attribution, no public ROAS/campaign. |
| P4-SMK-020 | Admin preview | No outbound command. |
| P4-SMK-021 | Customer sends new message during typing | Cancel and recompute. |
| P4-SMK-022 | Evidence missing artifact | Gate remains PENDING/FAIL, not PASS. |
| P4-SMK-023 | Female/male/elderly add-on | Adds dưỡng mô/sinh lực/canxi only for matching verified context and proof; otherwise asks once or skips. |
| P4-SMK-024 | Bulk vs distributor | Corporate gift bulk quote is not routed as distributor; distributor/mua sỉ intent routes human/0889787878 policy. |
| P4-SMK-025 | Order success wording | After order_code, AI chúc mừng/ghi nhận đơn thành công without saying paid/shipped unless runtime says so. |
| P4-SMK-026 | Runtime provider evidence missing | PENDING_EVIDENCE remains when provider traces for sales/product/quote/order/shipping/CRM/guard are missing. |

## 20. Done gate

Phase 4 chỉ được gọi `EVIDENCE_SUBMITTED_FOR_OWNER_REVIEW` khi:

1. Bảng Gôm canonical này là file đọc đầu tiên.
2. Chỉ mục bàn giao trỏ đúng về Bảng Gôm và đúng thứ tự chạy.
3. `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` được dùng để tạo SRS/RTM trước implementation.
4. File 00-08 khớp Bảng Gôm này hoặc có blocker cập nhật rõ ràng.
5. P4-GATE-001 đến P4-GATE-019 có evidence item.
6. Smoke P4-SMK-001 đến P4-SMK-026 có kết quả `PASS` / `FAIL` / `BLOCKED` / `PENDING` rõ ràng.
7. Không có P0 fail bị che thành "known limitation".
8. Owner decision từ MASTER-08 được chọn kèm policy version hoặc được carry như blocker.
9. Gateway vẫn `PRODUCTION_BLOCKED`.
10. Không tuyên bố Release/Go-live.

## 21. Fail gate

Phase 4 FAIL nếu có bất kỳ điều nào sau đây:

- AI self-calculates price, stock, benefit, ship, VAT, final total, order_code, paid, verified revenue, commission, ROAS or payout.
- AI public final quote, PII, payment instruction, personal order state, member tier or commission in public comment/live.
- AI suggests product without ProductRoleMatrix/public-safe Product Knowledge.
- AI suggests vegan/chay product without proof.
- AI ignores sale lock/recall/not sellable.
- AI says order success without order_code.
- AI says paid based on customer message or screenshot only.
- AI says shipping/export/ETA without runtime/fallback policy.
- AI sends CRM when opt-out/open case/quiet hour/frequency cap/fatigue fail.
- AI dùng raw message file 3/6 mà không qua canonical registry.
- AI gợi ý sai add-on nữ/nam/người già hoặc dùng claim điều trị cho dưỡng mô/sinh lực/canxi.
- AI gom mua nhiều/quà công ty thành đại lý, tự báo chiết khấu bulk hoặc bỏ qua DistributorIntentGuard.
- AI render câu chúc mừng đơn thành công khi chưa có order_code hoặc nói paid/shipped không có runtime.
- AI gọi P0 PASS khi thiếu provider evidence thật cho DailySalesContext, ProductRoleMatrix, QuoteSnapshot, OrderDraft, ShippingState, CRMJobLog hoặc guard trace.
- AI emits banned customer-facing terms.
- AI opens or implies Gateway production.
- Evidence is missing, unmasked, contains secret, or is not accepted but called PASS.

## 22. Owner decision carry-over

| Decision | Impact on Phase 4 if pending |
| --- | --- |
| `PRICE-POLICY-DECISION-001` | No hardcoded 24/7/GH discount; quote only from runtime. |
| `GOLDEN-HOUR-TIME-DECISION-001` | No fixed GH schedule in AI/CRM wording. |
| `VEGAN-FORMULA-DECISION-001` | No vegan claim/recommendation if proof conflict. |
| `FB-PAGE-REGISTRY-CLEAN-001` | No Gateway production or page-specific outbound assumption. |
| `GATEWAY-PRODUCTION-GATE-001` | Gateway remains blocked. |
| `MEMBER-BENEFIT-GOLDEN-HOUR-DECISION-001` | No member/GH benefit stacking unless QuoteSnapshot says so. |
| `DIAMOND-SELF-PURCHASE-DECISION-001` | Commission state pending/review if self-purchase uncertain. |
| `MISA-PAYOUT-DECISION-001` | No payout/MISA wording beyond safe pending/finance review. |
| `IVR-ATTEMPT-POLICY-DECISION-001` | No IVR real-call readiness claim. |

## 23. SRS and implementation handoff

Phase 4 đủ để giao dev phân tích sang SRS khi dev dùng đúng các file sau theo thứ tự:

1. `12-BẢNG GOM GIAI ĐOẠN 4.md`.
2. `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md`.
3. `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md`.
4. `00-PHÂN TÍCH HIỆN TRẠNG.md`.
5. `01-THIẾT KẾ KỸ THUẬT.md`.
6. File 02-08 theo work package.

Dev phải tạo SRS/RTM trước khi code. SRS tối thiểu phải map:

- `P4-SRS-FR-001` đến `P4-SRS-FR-024`.
- `P4-SRS-NFR-001` đến `P4-SRS-NFR-011`.
- `P4-GATE-001` đến `P4-GATE-019`.
- `P4-SMK-001` đến `P4-SMK-026`.
- Owner decisions còn pending.
- Evidence artifact và smoke command/test reference.

Implementation chỉ được bóc theo work package sau khi SRS được review/accepted hoặc codebase hiện hữu chứng minh map tương đương. Nếu chưa có SRS accepted, dev chỉ được làm analysis, SRS, design hoặc gap report.

## 24. Handoff to Phase 5

Phase 5 may start planning only after Phase 4 owner review accepts:

- AI safe response contract.
- Final Response Guard.
- Public/private boundary.
- Handoff decision envelope.
- Evidence package schema.
- P0 smoke plan.

Phase 5 production Gateway chỉ được xét sau khi có đủ artifact được owner accepted với trạng thái `PASS`; các dòng dưới đây là điều kiện tương lai, không phải claim hiện tại của Phase 4:

- AI Advisor Facebook Completion Report owner-accepted `PASS`.
- Evidence Package owner-accepted `PASS`.
- P0 E2E live-to-order smoke owner-accepted `PASS`.
- Page Registry normalized and production explicitly allowed.
- App permission/security review.
- Owner sign-off.
- Rollback/monitoring ready.

Until then:

```text
P4_AI_CHANNEL_PENDING_EVIDENCE
P5_GATEWAY_PRODUCTION_BLOCKED
NO_RELEASE_READY
NO_PRODUCTION_READY
```

## 25. Final status

```text
PHASE_4_CANONICAL_REWRITE_COMPLETE_FOR_OWNER_REVIEW
PHASE_4_DEV_SRS_HANDOFF_READY
SRS_REQUIRED_BEFORE_CODE_IMPLEMENTATION
AI_ADVISOR_RUNTIME_BOUNDARY_LOCKED
PENDING_OWNER_DECISIONS
PENDING_EVIDENCE
GATEWAY_PRODUCTION_BLOCKED
NOT_RELEASE_READY
NOT_PRODUCTION_READY
```

