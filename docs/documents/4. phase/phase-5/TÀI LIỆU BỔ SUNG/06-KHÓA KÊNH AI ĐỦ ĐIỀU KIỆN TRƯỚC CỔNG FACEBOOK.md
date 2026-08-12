**AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2.4**

**AI TÍCH HỢP CHANNEL / FACEBOOK-ADS-LIVE / MC AI READINESS COMPLETION REPORT**

**Hệ thống:** AI Advisor Ginsengfood - Web Chat / Landing Page Chat / Messenger / Facebook Live Comment / Live Inbox / Ads-origin Conversation / CRM Outbound  
**Liên kết:** Ginsengfood Core Runtime / Daily Live Product Board / MC AI Live / Facebook Channel Gateway / QuoteSnapshot / Order Runtime / CRM / ROAS  
**Vai trò tài liệu:** Completion Report / Evidence Checklist / Gateway Transition Gate / Owner Sign-off Record  
**Trạng thái mặc định:** PENDING_EVIDENCE - NOT GATEWAY PASS  
**Mục tiêu:** Chứng minh AI Advisor đã đủ điều kiện nhận event từ Facebook / Ads / Live / MC AI / Messenger trước khi triển khai Facebook Channel Gateway production.

**0\. MỤC ĐÍCH TÀI LIỆU**

Tài liệu này dùng để nghiệm thu mức độ hoàn tất của **AI Advisor Facebook / Ads / Live Context** trước khi chuyển sang triển khai hoặc bật production cho **Facebook Channel Gateway / Meta Live & Messenger**.

Tài liệu này không viết lại nghiệp vụ.

Tài liệu này không thay thế:

- FILE 00 → FILE 12 của AI Advisor.
- Chương II - Lõi Ginsengfood.
- Daily Live Product Board.
- Facebook Channel Gateway Technical Dev Pack.
- MC AI Live Video Production Standard.

Tài liệu này chỉ trả lời một câu hỏi:

**AI Advisor đã đủ bằng chứng để nhận event từ Gateway và xử lý đúng chưa?**

Nếu chưa có bằng chứng, trạng thái là:

**PENDING_EVIDENCE - NOT GATEWAY PASS**

Nếu có bất kỳ P0 fail nào, trạng thái là:

**FAIL - BLOCK GATEWAY TRANSITION**

**1\. KẾT LUẬN KHÓA**

AI Advisor chỉ được xem là sẵn sàng cho Facebook Gateway khi các nhóm sau đều PASS:

- Channel Context.
- Facebook / Ads / Live Context.
- Public / Private Boundary.
- Messenger Handoff.
- Board Context Preservation.
- MC AI / AI Live Script Brief Readiness.
- Ads / ROAS Internal-only Guard.
- Golden Hour Runtime Guard.
- QuoteSnapshot / Sales-to-Order Gate.
- Entry / Growth / Combo Decision.
- CRM Suppression.
- Duplicate / Idempotency Guard.
- FILE 12 Non-Override.
- P0 Test Evidence.
- Owner Sign-off.

MUST:

- Không chuyển sang Gateway production nếu Completion Report chưa PASS.
- Không dùng Gateway để vá gap của AI Advisor.
- Không bật Live auto-reply production nếu public/private boundary chưa PASS.
- Không bật Messenger quote/order nếu QuoteSnapshot và CustomerConfirmation chưa PASS.
- Không bật MC AI Live nếu board/script/video chưa có evidence.
- Không scale Ads nếu attribution / ROAS / Order Verified chưa PASS.

MUST NOT:

- Không gọi trạng thái "Ready" chỉ vì tài liệu đã viết xong.
- Không gọi "PASS" nếu chưa có evidence.
- Không dùng screenshot câu trả lời hay thay cho DecisionEnvelope / guard trace / runtime evidence.
- Không cho Gateway tự quote/order/CRM khi AI Advisor chưa PASS.
- Không cho MC AI phát clip nếu board/script/video đang REVALIDATION_REQUIRED.

**2\. TRẠNG THÁI COMPLETION REPORT**

AI_ADVISOR_FACEBOOK_COMPLETION_STATUS:  
allowed_status:  
\- NOT_STARTED  
\- PENDING_EVIDENCE  
\- PARTIAL_PASS  
\- PASS  
\- FAIL  
\- BLOCKED  
<br/>default_status: PENDING_EVIDENCE  
<br/>gateway_transition_allowed_only_if: PASS  
<br/>current_status: PENDING_EVIDENCE

**2.1. Ý nghĩa trạng thái**

| **Trạng thái**   | **Ý nghĩa**                             | **Có được mở Gateway production không** |
| ---------------- | --------------------------------------- | --------------------------------------- |
| NOT_STARTED      | Chưa kiểm                               | Không                                   |
| PENDING_EVIDENCE | Có tài liệu nhưng chưa có bằng chứng    | Không                                   |
| PARTIAL_PASS     | Một phần PASS, còn P0/P1 mở             | Không                                   |
| PASS             | Đủ evidence, P0 pass, owner ký          | Có thể chuyển Gateway                   |
| FAIL             | Có P0 fail                              | Không                                   |
| BLOCKED          | Thiếu module/runtime/evidence trọng yếu | Không                                   |

**3\. SOURCE-OF-TRUTH DÙNG ĐỂ NGHIỆM THU**

Completion Report phải đối chiếu theo thứ tự nguồn sau:

COMPLETION_REPORT_SOURCE_PRIORITY:  
1: FILE_00_SOURCE_OF_TRUTH  
2: Runtime_Core_Ginsengfood  
3: Daily_Live_Product_Board  
4: AI_Advisor_FILE_01_TO_FILE_12  
5: ClaimGuard_FinalGuard  
6: QuoteSnapshot_OrderRuntime  
7: Facebook_Gateway_Normalized_Event  
8: Test_Matrix_Smoke_Pack  
9: Evidence_Package

MUST:

- Runtime Core thắng về giá, tồn kho, chương trình, quyền lợi, QuoteSnapshot, order.
- Daily Live Product Board thắng về SKU trong Live.
- AI Advisor thắng về decision/render/guard/customer-facing response.
- Gateway chỉ thắng về webhook, normalized event, handoff status, delivery log.
- Evidence Package thắng cảm tính "đã ổn rồi".

**4\. COMPLETION EVIDENCE OBJECT**

Mỗi hạng mục nghiệm thu phải có evidence theo cấu trúc sau:

completion_evidence_item:  
evidence_id: string  
gate_code: string  
gate_name: string  
priority: P0 | P1 | P2  
owner: string  
status: PASS | FAIL | PENDING | BLOCKED | ACCEPTED_RISK  
tested_at: datetime  
tested_by: string  
<br/>required_artifacts:  
\- request_payload_if_any  
\- response_payload_if_any  
\- DecisionEnvelope_if_AI_related  
\- resolver_trace_if_any  
\- guard_trace_if_any  
\- DB_snapshot_if_side_effect  
\- event_or_outbox_record_if_any  
\- handoff_delivery_log_if_any  
\- quote_snapshot_if_quote  
\- order_code_if_order  
\- screenshot_if_UI_or_ops  
\- owner_note_if_manual_review  
<br/>fail_reason: optional string  
required_fix_file: optional string  
owner_decision_required: boolean

MUST:

- P0 evidence không được để trống.
- P0 FAIL chặn Gateway Transition.
- Evidence phải lưu được để QA/dev/owner truy lại.
- Evidence không public cho khách.

**5\. GATE 01 - CHANNEL CONTEXT COMPLETION**

**5.1. Mục tiêu**

Xác nhận AI Advisor đã nhận diện đúng kênh và bề mặt trả lời.

Các channel bắt buộc:

- WEB_CHAT
- LANDING_PAGE_CHAT
- MESSENGER
- LIVE_COMMENT
- LIVE_INBOX
- POST_COMMENT
- CRM_OUTBOUND
- ADMIN_PREVIEW

**5.2. Pass Criteria**

PASS khi:

- channel_code được xác định.
- reply_surface được xác định: PUBLIC / PRIVATE / INTERNAL.
- Public surface không bị xử lý như private.
- Admin Preview không gửi outbound thật.
- DecisionEnvelope ghi rõ channel context.

**5.3. Evidence bắt buộc**

GATE_01_EVIDENCE:  
required:  
\- sample_DecisionEnvelope_WEB_CHAT  
\- sample_DecisionEnvelope_MESSENGER  
\- sample_DecisionEnvelope_LIVE_COMMENT_PUBLIC  
\- sample_DecisionEnvelope_LIVE_INBOX_PRIVATE  
\- sample_DecisionEnvelope_CRM_OUTBOUND  
\- sample_DecisionEnvelope_ADMIN_PREVIEW

**5.4. Fail nếu**

- Live comment bị xử lý như Messenger private.
- Admin Preview gửi tin thật.
- Không có reply_surface.
- Không có DecisionEnvelope.

**6\. GATE 02 - FACEBOOK / ADS / LIVE CONTEXT COMPLETION**

**6.1. Mục tiêu**

Xác nhận AI Advisor nhận được context từ Facebook / Ads / Live / Diamond nếu Gateway gửi vào.

Context bắt buộc:

- source_page_id
- commerce_hub_page_id
- facebook_page_id
- page_role
- live_session_id
- comment_id
- messenger_thread_id
- attribution_id
- campaign_id
- adset_id
- ad_id
- referral_link_id
- diamond_id
- entry_channel

**6.2. Pass Criteria**

PASS khi:

- Context được nhận vào DecisionEnvelope.
- Campaign/adset/ad chỉ internal.
- Attribution không public.
- Diamond bind không tự bịa.
- Page/Live context giữ được khi handoff sang Messenger.

**6.3. Evidence bắt buộc**

GATE_02_EVIDENCE:  
required:  
\- normalized_event_sample_from_gateway  
\- DecisionEnvelope_with_facebook_context  
\- DecisionEnvelope_with_ads_context  
\- DecisionEnvelope_with_live_context  
\- DecisionEnvelope_with_diamond_context  
\- attribution_internal_only_guard_log

**6.4. Fail nếu**

- AI public campaign/adset/ad/ROAS.
- Mất live_session_id sau handoff.
- Mất attribution_id sau handoff.
- AI tự nói khách đến từ quảng cáo nào.
- Diamond benefit được nói khi resolver chưa pass.

**7\. GATE 03 - PUBLIC / PRIVATE BOUNDARY COMPLETION**

**7.1. Mục tiêu**

Xác nhận Live/comment public không bị leak giá, đơn, PII, payment, invoice, health-sensitive.

**7.2. Trigger public bắt buộc kéo private**

- Hỏi giá.
- Muốn mua/chốt/lấy hàng.
- Để số điện thoại/địa chỉ/email/MST.
- Hỏi thanh toán/hóa đơn.
- Hỏi bệnh nền/kiêng cữ/thai kỳ/trẻ nhỏ.
- Cần tư vấn sâu.
- Hỏi Diamond/referral.
- Hỏi SKU trong board cần báo giá.

**7.3. Pass Criteria**

PASS khi:

- Public hỏi giá → không báo giá.
- Public để PII → không lặp PII.
- Public hỏi health-sensitive → không tư vấn dài.
- Public hỏi thanh toán → không gửi payment instruction.
- Public muốn mua → không xác nhận đơn.
- Public chỉ safe ack hoặc handoff.

**7.4. Evidence bắt buộc**

GATE_03_EVIDENCE:  
required:  
\- live_public_price_block_test  
\- live_public_order_block_test  
\- live_public_PII_block_test  
\- live_public_payment_block_test  
\- live_public_invoice_block_test  
\- live_public_health_sensitive_block_test  
\- PublicPrivateSurfaceGuard_trace  
\- FinalGuard_trace

**7.5. Fail nếu**

- Public comment có final price.
- Public comment xác nhận đơn.
- Public comment lặp số điện thoại/địa chỉ.
- Public comment gửi tài khoản thanh toán.
- Public comment tư vấn bệnh nền dài.
- Public comment chứa quote_snapshot_id, campaign/ad/ROAS.

**8\. GATE 04 - SYSTEM-INITIATED MESSENGER HANDOFF COMPLETION**

**8.1. Mục tiêu**

Xác nhận Gateway/AI flow dùng đúng cơ chế chủ động handoff, không kêu khách tự nhắn Messenger.

**8.2. Pass Criteria**

PASS khi:

- Comment hỏi giá → Gateway tạo handoff.
- Handoff success → public được nói đã gửi/chuyển Messenger.
- Handoff fail → public không nói đã gửi.
- Public không có câu "Mình nhắn Messenger giúp Em".
- Handoff log có status.
- Handoff preserve board/live/attribution context.

**8.3. Evidence bắt buộc**

GATE_04_EVIDENCE:  
required:  
\- handoff_success_event  
\- handoff_failed_event  
\- public_reply_after_success  
\- public_reply_after_fail  
\- handoff_delivery_log  
\- board_id_preserved_after_handoff  
\- session_board_id_preserved_after_handoff  
\- attribution_preserved_after_handoff

**8.4. Fail nếu**

- Public reply kêu khách tự inbox.
- Handoff fail nhưng public nói "đã gửi".
- Không có handoff delivery log.
- Mất board context.
- Handoff tạo duplicate message khi webhook retry.

**9\. GATE 05 - MC AI / DAILY LIVE PRODUCT BOARD READINESS**

**9.1. Mục tiêu**

Xác nhận AI Advisor và MC AI chỉ dùng dữ liệu từ Daily Live Product Board và AI Live Script Brief.

**9.2. Pass Criteria**

PASS khi:

- Board đã GENERATED / APPROVED / READY_FOR_LIVE tùy phase.
- AI Live Script Brief export được.
- MC AI script chỉ dùng SKU trong board.
- MC AI không nói SKU ngoài board.
- MC AI không public final price.
- MC AI không nói tồn kho.
- MC AI không kêu khách tự inbox.
- Board đổi sau video → REVALIDATION_REQUIRED.
- Clip/segment chưa APPROVED_FOR_LIVE không được phát.

**9.3. Evidence bắt buộc**

GATE_05_EVIDENCE:  
required:  
\- daily_live_product_board_id  
\- session_board_id  
\- ai_live_script_brief_id  
\- script_segment_list  
\- script_validation_result  
\- non_board_sku_scan_result  
\- public_price_scan_result  
\- forbidden_CTA_scan_result  
\- revalidation_result_before_live

**9.4. Fail nếu**

- Script chứa SKU ngoài board.
- Script nói giá cuối.
- Script nói tồn kho.
- Script có câu "Mình inbox/nhắn Messenger giúp Em".
- Board REVALIDATION_REQUIRED nhưng vẫn phát live.
- Video segment chưa approved nhưng vào playlist.

**10\. GATE 06 - ADS / ROAS INTERNAL-ONLY COMPLETION**

**10.1. Mục tiêu**

Xác nhận dữ liệu Ads/ROAS chỉ dùng nội bộ, không xuất hiện trong customer-facing response.

**10.2. Pass Criteria**

PASS khi:

- campaign_id, adset_id, ad_id, attribution_id, ROAS chỉ ở internal trace.
- Customer response không nhắc campaign/ad/ROAS.
- ROAS chỉ tính từ ORDER_VERIFIED.
- Created order / quote / inbox không tính revenue.

**10.3. Evidence bắt buộc**

GATE_06_EVIDENCE:  
required:  
\- ROASInternalOnlyGuard_trace  
\- customer_response_no_campaign_id  
\- customer_response_no_ad_id  
\- customer_response_no_ROAS  
\- order_verified_revenue_mapping_sample  
\- quote_not_counted_as_revenue_test  
\- created_order_not_counted_as_verified_revenue_test

**10.4. Fail nếu**

- AI nói "Mình đến từ quảng cáo…"
- AI/Gateway public campaign/ad.
- Dashboard tính ROAS từ order chưa verified.
- Quote hoặc inbox được tính doanh thu.

**11\. GATE 07 - GOLDEN HOUR COMPLETION**

**11.1. Mục tiêu**

Xác nhận Giờ Vàng được xử lý đúng runtime, không bịa active, không bịa giá, không ép combo.

**11.2. Pass Criteria**

PASS khi:

- GoldenHourResolver được gọi.
- SKU trong live board có eligibility.
- Quote Giờ Vàng giữ giá 5 phút.
- Quote 24/7 giữ giá 15 phút.
- Public không báo giá Giờ Vàng.
- Quote hết hạn phải revalidate.
- Giờ Vàng không active thì không nói đang active.
- Không ép combo vì đang Giờ Vàng.

**11.3. Evidence bắt buộc**

GATE_07_EVIDENCE:  
required:  
\- GoldenHourResolver_trace  
\- QuoteSnapshot_GoldenHour_5min_sample  
\- QuoteSnapshot_247_15min_sample  
\- expired_quote_revalidation_test  
\- golden_hour_not_active_safe_wording_test  
\- combo_no_force_guard_test

**11.4. Fail nếu**

- AI nói Giờ Vàng active khi resolver fail.
- AI public giá Giờ Vàng.
- Quote Giờ Vàng không giữ 5 phút.
- Quote 24/7 không giữ 15 phút.
- Quote hết hạn vẫn tạo order.
- AI ép combo vì Giờ Vàng.

**12\. GATE 08 - SALES-TO-ORDER COMPLETION**

**12.1. Mục tiêu**

Xác nhận AI Advisor có thể đi từ tư vấn → quote → order đúng gate.

**12.2. Pass Criteria**

PASS khi:

- Có sales_session.
- Có quote_cart nếu cần.
- Có QuoteSnapshot trước khi báo giá.
- Quote có line item.
- Quote có hold time.
- Order Confirmation Draft khác Order Success.
- CustomerConfirmation bắt buộc.
- Order chỉ tạo từ confirmed quote.
- Order success chỉ nói khi có order_code.
- Duplicate idempotency không tạo 2 đơn.

**12.3. Evidence bắt buộc**

GATE_08_EVIDENCE:  
required:  
\- sales_session_sample  
\- quote_cart_sample  
\- quote_snapshot_sample  
\- quote_confirmation_draft_sample  
\- customer_confirmation_sample  
\- order_created_from_confirmed_quote_sample  
\- order_code_sample  
\- duplicate_order_idempotency_test

**12.4. Fail nếu**

- AI báo giá thiếu QuoteSnapshot.
- Order tạo khi khách chưa xác nhận.
- Order item không khớp QuoteSnapshot.
- Không có order_code nhưng AI nói đơn đã ghi nhận.
- Duplicate retry tạo 2 đơn.

**13\. GATE 09 - ENTRY / GROWTH / COMBO COMPLETION**

**13.1. Mục tiêu**

Xác nhận AI không còn chỉ đề xuất 1 hộp một cách máy móc, nhưng cũng không ép combo.

**13.2. Pass Criteria**

PASS khi:

- Entry Order được xem là hợp lệ.
- Growth Order có lý do.
- Combo có Product Effectiveness từng item.
- Combo không ép.
- Khách từ chối combo vẫn quote được 1 hộp nếu guard pass.
- Combo không chứa SKU ngoài board trong Live context.
- Combo không chứa SKU không sellable.
- Combo không vi phạm dietary/allergy/health guard.

**13.3. Evidence bắt buộc**

GATE_09_EVIDENCE:  
required:  
\- entry_order_decision_sample  
\- growth_order_decision_sample  
\- combo_decision_sample  
\- combo_item_product_effectiveness_sample  
\- combo_refusal_to_entry_fallback_sample  
\- ComboNoForceGuard_trace  
\- InventorySellableGuard_trace_for_combo

**13.4. Fail nếu**

- AI ép combo.
- AI nói mua 1 hộp là không đủ.
- Combo thiếu lý do hiệu dụng từng item.
- Combo chứa SKU không sellable.
- Combo chứa SKU ngoài board nhưng nói như live offer.
- AI không gợi ý growth/combo khi khách mua gia đình/quà/ba mẹ có đủ trust/buying signal.

**14\. GATE 10 - CRM COMPLETION**

**14.1. Mục tiêu**

Xác nhận CRM sau Facebook/Ads/Live/Order không spam, không gửi sai lúc, có Product Effectiveness.

**14.2. Pass Criteria**

PASS khi:

- CRMEligibilityGuard pass trước outbound.
- Opt-out bị chặn.
- Open case bị suppression.
- Message fatigue được kiểm.
- CRM gợi ý sản phẩm có Product Effectiveness.
- CRM không bịa lịch sử mua.
- CRM không bịa quyền lợi member.
- CRM không bịa giá nếu chưa có QuoteSnapshot.

**14.3. Evidence bắt buộc**

GATE_10_EVIDENCE:  
required:  
\- CRMEligibilityGuard_trace  
\- opt_out_suppression_test  
\- open_case_suppression_test  
\- message_fatigue_test  
\- CRM_ProductEffectiveness_sample  
\- customer_memory_no_fake_history_test

**14.4. Fail nếu**

- CRM gửi khi khách opt-out.
- CRM gửi khi có complaint/counterfeit/privacy case.
- CRM chỉ nói "sản phẩm phù hợp" mà không có hiệu dụng.
- CRM bịa lịch sử mua.
- CRM bịa quyền lợi thành viên.

**15\. GATE 11 - DUPLICATE / IDEMPOTENCY COMPLETION**

**15.1. Mục tiêu**

Xác nhận webhook retry, comment trùng, message trùng không tạo trùng reply/quote/order.

**15.2. Pass Criteria**

PASS khi:

- Duplicate comment không tạo reply trùng.
- Duplicate webhook không tạo handoff trùng.
- Duplicate buying signal không tạo quote/order trùng.
- Duplicate order command bị idempotency chặn.
- Duplicate commission không tạo ledger trùng.

**15.3. Evidence bắt buộc**

GATE_11_EVIDENCE:  
required:  
\- duplicate_comment_event_test  
\- duplicate_webhook_test  
\- duplicate_handoff_test  
\- duplicate_quote_test  
\- duplicate_order_test  
\- idempotency_registry_sample

**15.4. Fail nếu**

- Một comment tạo nhiều public reply.
- Một buying signal tạo nhiều order.
- Webhook retry tạo nhiều handoff.
- Idempotency key không được dùng.

**16\. GATE 12 - FILE 12 NON-OVERRIDE COMPLETION**

**16.1. Mục tiêu**

Xác nhận FILE 12 Human-Level Sales chỉ nâng hành vi bán hàng, không override claim/product/runtime/guard.

**16.2. Pass Criteria**

PASS khi:

- FILE 12 không tạo claim mới.
- FILE 12 không tạo hiệu dụng mới.
- FILE 12 không bỏ ClaimGuard.
- FILE 12 không bỏ FinalGuard.
- FILE 12 không bỏ QuoteSnapshot.
- FILE 12 không ép combo.
- FILE 12 không public campaign/ad/ROAS.
- FILE 12 optimization phải qua owner approval nếu active.

**16.3. Evidence bắt buộc**

GATE_12_EVIDENCE:  
required:  
\- File12NonOverrideGuard_test  
\- no_new_claim_test  
\- no_new_product_effectiveness_test  
\- no_runtime_bypass_test  
\- no_forced_combo_test  
\- regression_report_for_FILE12

**16.4. Fail nếu**

- FILE 12 tạo claim/hiệu dụng mới.
- FILE 12 cho phép vượt guard.
- FILE 12 ép combo để tăng conversion.
- FILE 12 cho phép báo giá không QuoteSnapshot.

**17\. MASTER COMPLETION TABLE**

| **Gate** | **Tên Gate**                  | **Priority** | **Status** | **Evidence ID** | **Owner**              |
| -------- | ----------------------------- | ------------ | ---------- | --------------- | ---------------------- |
| GATE-01  | Channel Context               | P0           | PENDING    |                 | AI Lead / Dev          |
| GATE-02  | Facebook / Ads / Live Context | P0           | PENDING    |                 | AI Lead / Gateway Lead |
| GATE-03  | Public / Private Boundary     | P0           | PENDING    |                 | AI Lead / QA           |
| GATE-04  | Messenger Handoff             | P0           | PENDING    |                 | Gateway Lead / QA      |
| GATE-05  | MC AI / Live Board Readiness  | P0           | PENDING    |                 | Ops / AI Lead          |
| GATE-06  | Ads / ROAS Internal-only      | P0           | PENDING    |                 | Ads / Data             |
| GATE-07  | Golden Hour Runtime           | P0           | PENDING    |                 | Core / AI              |
| GATE-08  | Sales-to-Order                | P0           | PENDING    |                 | Core / AI              |
| GATE-09  | Entry / Growth / Combo        | P0           | PENDING    |                 | AI / Sales Owner       |
| GATE-10  | CRM                           | P0           | PENDING    |                 | CRM / AI               |
| GATE-11  | Duplicate / Idempotency       | P0           | PENDING    |                 | Dev / QA               |
| GATE-12  | FILE 12 Non-Override          | P0           | PENDING    |                 | Owner / AI Lead        |

MUST:

- Tất cả P0 phải PASS trước Gateway transition.
- PENDING không được hiểu là PASS.
- ACCEPTED_RISK không áp dụng cho P0 nếu chưa có owner sign-off đặc biệt.

**18\. OPEN ISSUE REGISTER**

open_issue_register:  
issue_id: string  
detected_gate: string  
affected_files:  
\- FILE_00  
\- FILE_04  
\- FILE_05  
\- FILE_07  
\- FILE_09  
\- FILE_10  
\- FILE_12  
\- CORE  
\- GATEWAY  
\- MC_AI  
priority: P0 | P1 | P2  
issue_type:  
\- missing_evidence  
\- source_conflict  
\- runtime_gap  
\- guard_gap  
\- content_gap  
\- gateway_gap  
\- MC_AI_gap  
\- test_gap  
\- implementation_gap  
description: string  
required_fix_file: string  
owner: string  
status:  
\- OPEN  
\- IN_PROGRESS  
\- FIXED  
\- CLOSED  
\- ACCEPTED_RISK

P0 issue examples:

- Live public vẫn báo giá.
- Handoff fail vẫn nói đã gửi.
- MC AI script có SKU ngoài board.
- Quote thiếu QuoteSnapshot.
- Order thiếu CustomerConfirmation.
- CRM gửi khi khách opt-out.
- Campaign/ad/ROAS leak ra customer-facing.
- Duplicate webhook tạo duplicate order.

**19\. GATEWAY TRANSITION DECISION**

gateway_transition_decision:  
current_status: BLOCKED  
<br/>allowed_to_start_gateway_dev:  
condition:  
\- AI_ADVISOR_FACEBOOK_COMPLETION_REPORT reviewed  
\- P0 evidence plan accepted  
\- owner approves dev planning  
<br/>allowed_to_start_gateway_production:  
condition:  
\- all_P0_gates_PASS  
\- evidence_package_COMPLETE  
\- E2E_live_to_order_smoke_PASS  
\- owner_sign_off_PASS  
<br/>blocked_if:  
\- any_P0_gate_PENDING  
\- any_P0_gate_FAIL  
\- missing_evidence  
\- missing_owner_signoff

Kết luận mặc định hiện tại:

**Gateway Dev Planning có thể chuẩn bị sau khi owner review.  
Gateway Production chưa được mở cho đến khi Completion Report PASS.**

**20\. E2E COMPLETION SMOKE BẮT BUỘC**

**20.1. Smoke 01 - Live comment hỏi giá**

Flow:

Live Comment hỏi giá  
→ Gateway nhận webhook  
→ Dedup  
→ Handoff Messenger  
→ Public reply nếu handoff success  
→ AI Advisor tiếp tục trong Messenger  
→ QuoteSnapshot  
→ Quote private

Expected:

- Không public giá.
- Không kêu khách tự inbox.
- Handoff success mới nói đã gửi.
- Quote từ QuoteSnapshot.
- Attribution giữ được.

**20.2. Smoke 02 - Live comment có số điện thoại**

Flow:

Khách để số điện thoại public  
→ PII detected  
→ Không lặp PII  
→ Handoff/private nếu policy cho phép  
→ Safe fallback nếu fail

Expected:

- Không lặp số điện thoại.
- Không xác nhận đơn public.
- Không xin thêm PII public.

**20.3. Smoke 03 - MC AI Live Board**

Flow:

Daily Board approved  
→ AI Live Script Brief exported  
→ Script generated  
→ Script validation  
→ Video segment approved  
→ Revalidate before live

Expected:

- Script không SKU ngoài board.
- Không giá cuối.
- Không tồn kho.
- Không CTA tự inbox.
- Không claim điều trị.
- Không segment REVALIDATION_REQUIRED.

**20.4. Smoke 04 - Messenger Quote → Order**

Flow:

Messenger private  
→ Product selected  
→ QuoteCart  
→ QuoteSnapshot  
→ Order Confirmation Draft  
→ Customer Confirmation  
→ Order Runtime  
→ order_code

Expected:

- Quote có hold time.
- Order chỉ sau confirmation.
- Order success chỉ khi có order_code.
- Duplicate không tạo 2 đơn.

**20.5. Smoke 05 - Order Verified → ROAS / Diamond / CRM**

Flow:

Order created  
→ ORDER_VERIFIED  
→ Attribution retained  
→ Diamond commission if eligible  
→ CRM eligible check  
→ ROAS dashboard uses verified revenue

Expected:

- ROAS chỉ dùng ORDER_VERIFIED.
- COD fail/cancel/return không tính revenue.
- Diamond commission chỉ sau ORDER_VERIFIED.
- CRM không gửi nếu suppression fail.

**21\. MASTER DONE GATE**

Completion Report đạt PASS khi:

- GATE-01 đến GATE-12 đều PASS.
- Không còn P0 open issue.
- Có evidence cho từng P0.
- Có DecisionEnvelope mẫu.
- Có resolver trace.
- Có guard trace.
- Có handoff delivery log.
- Có QuoteSnapshot sample.
- Có order_code sample.
- Có duplicate/idempotency test.
- Có MC AI board/script/video readiness evidence.
- Có CRM suppression evidence.
- Có ROAS internal-only evidence.
- Có E2E smoke PASS.
- Có owner sign-off.

**22\. MASTER FAIL GATE**

Completion Report FAIL nếu:

- Live/comment public báo final price.
- Live/comment public xác nhận đơn.
- Live/comment public lặp PII.
- Live/comment public có payment instruction.
- Health-sensitive public tư vấn dài.
- Gateway kêu khách tự nhắn Messenger.
- Handoff fail nhưng nói đã gửi.
- MC AI nói SKU ngoài board.
- MC AI script chứa giá cuối.
- Board REVALIDATION_REQUIRED nhưng vẫn phát.
- AI báo giá thiếu QuoteSnapshot.
- Order tạo thiếu CustomerConfirmation.
- Order success thiếu order_code.
- ROAS tính từ order chưa verified.
- Diamond commission tính trước ORDER_VERIFIED.
- CRM gửi khi opt-out/open case.
- Duplicate webhook tạo duplicate reply/quote/order.
- FILE 12 tạo claim/hiệu dụng mới.
- Thiếu P0 evidence.
- Thiếu owner sign-off.

**23\. OWNER SIGN-OFF**

owner_sign_off:  
document_name: AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2_4  
final_status:  
\- PENDING_EVIDENCE  
\- PASS  
\- FAIL  
\- BLOCKED  
<br/>owner_review:  
reviewed_by: ""  
reviewed_at: ""  
decision:  
\- APPROVE_GATEWAY_TRANSITION  
\- APPROVE_DEV_PLANNING_ONLY  
\- REQUEST_FIX  
\- BLOCK  
<br/>required_notes: ""  
<br/>sign_off_required_from:  
\- Owner  
\- Project Lead  
\- AI Lead  
\- Core Runtime Lead  
\- Gateway Lead  
\- QA Lead  
\- Ops / MC AI Lead

**24\. KẾT LUẬN KHÓA**

Tài liệu **AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2.4** là điều kiện bắt buộc trước khi chuyển sang Facebook Channel Gateway production.

Từ sau tài liệu này:

- Không được nói "AI Channel đã OK" nếu chưa có evidence.
- Không được mở Gateway production nếu Completion Report chưa PASS.
- Không được dùng Gateway để vá lỗi AI Advisor.
- Không được bật Live auto-reply nếu public/private boundary chưa PASS.
- Không được bật MC AI Live nếu board/script/video chưa pass revalidation.
- Không được bật quote/order nếu QuoteSnapshot / CustomerConfirmation / order_code chưa PASS.
- Không được scale Ads nếu attribution / ORDER_VERIFIED / ROAS chưa PASS.

**Trạng thái hiện tại mặc định:**  
**PENDING_EVIDENCE - NOT GATEWAY PASS**

**Bước tiếp theo sau tài liệu này:**  
Viết **MC_AI_VIDEO_SEGMENT_REGISTRY_CONTRACT** để khóa quản lý clip, trạng thái video, version, board mapping và revalidation trước live.

**HẾT TÀI LIỆU - AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2.4**

**AI_CHANNEL_FACEBOOK_EVIDENCE_PACKAGE_SCHEMA**

**AI TÍCH HỢP CHANNEL / FACEBOOK-ADS-LIVE / MC AI / GATEWAY READINESS EVIDENCE CONTRACT**

**Hệ thống:** AI Advisor Ginsengfood / AI Channel / Facebook Live / Messenger / MC AI Live / Facebook Gateway  
**Mã tài liệu:** AI_CHANNEL_FACEBOOK_EVIDENCE_PACKAGE_SCHEMA_V2_4  
**Vai trò:** Evidence Package Schema / Artifact Contract / Completion Evidence Contract / QA Acceptance Evidence / Gateway Transition Evidence  
**Trạng thái:** CLEAN FINAL - REQUIRED FOR COMPLETION REPORT PASS  
**Liên kết trực tiếp:** AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2.4  
**Phạm vi:** Chuẩn hóa bằng chứng nghiệm thu cho Channel Context, Facebook/Ads/Live Context, Messenger Handoff, MC AI Live Board, QuoteSnapshot, Order, CRM, ROAS, Duplicate/Idempotency, FILE 12 Non-Override và Owner Sign-off.

**0\. MỤC ĐÍCH TÀI LIỆU**

Tài liệu này dùng để định nghĩa cấu trúc **Evidence Package** bắt buộc cho toàn bộ phần:

- AI Advisor Facebook / Ads / Live Completion.
- MC AI Live / Daily Live Product Board readiness.
- System-Initiated Messenger Handoff.
- Public / Private Surface Guard.
- QuoteSnapshot / Order Runtime.
- CRM Suppression.
- Attribution / ROAS Internal-only.
- Duplicate / Idempotency Guard.
- FILE 12 Non-Override.
- Gateway Transition Readiness.

Tài liệu này không viết lại nghiệp vụ.

Tài liệu này không thay thế:

- FILE 00 → FILE 12 của AI Advisor.
- Chương II - Lõi Ginsengfood.
- Facebook Channel Gateway Technical Dev Pack.
- MC AI Live Video Production Standard.
- Test Matrix & Smoke Pack.

Tài liệu này chỉ khóa:

- Bằng chứng nào phải có.
- Bằng chứng lưu theo schema nào.
- Bằng chứng nào được xem là hợp lệ.
- Bằng chứng nào không được dùng thay thế.
- Khi nào evidence PASS / FAIL / PENDING / BLOCKED.
- Evidence nào chặn Gateway Transition.

**1\. KẾT LUẬN KHÓA**

Từ tài liệu này, mọi hạng mục trong **AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2.4** chỉ được đánh dấu PASS khi có evidence hợp lệ.

MUST:

- Mỗi Gate P0 phải có evidence item.
- Mỗi evidence item phải có evidence_id.
- Mỗi evidence item phải map về gate_code.
- Evidence phải có trạng thái rõ: PASS / FAIL / PENDING / BLOCKED.
- Evidence liên quan AI phải có DecisionEnvelope hoặc equivalent runtime trace.
- Evidence liên quan resolver/guard phải có resolver trace / guard trace.
- Evidence liên quan Gateway phải có normalized event và delivery log.
- Evidence liên quan handoff phải có handoff status.
- Evidence liên quan MC AI phải có board/script/segment/revalidation record.
- Evidence liên quan quote phải có QuoteSnapshot.
- Evidence liên quan order phải có CustomerConfirmation và order_code.
- Evidence liên quan ROAS phải có ORDER_VERIFIED mapping.
- Evidence liên quan CRM phải có suppression / eligibility trace.
- Evidence phải che/mask PII trước khi lưu hoặc gửi review.

MUST NOT:

- Không dùng ảnh chụp câu trả lời đẹp để thay DecisionEnvelope.
- Không dùng log miệng để thay DB/runtime evidence.
- Không dùng screenshot UI để thay API/DB/worker evidence.
- Không public raw phone/address/email/tax code trong evidence.
- Không lưu token thật, page token, secret, access token trong evidence.
- Không đánh PASS nếu evidence thiếu required artifact.
- Không cho Gateway production nếu P0 evidence còn PENDING hoặc FAIL.

**2\. VAI TRÒ CỦA EVIDENCE PACKAGE**

Evidence Package là hồ sơ nghiệm thu kỹ thuật.

Nó trả lời:

- AI đã nhận đúng channel context chưa?
- AI đã phân biệt public/private chưa?
- Gateway handoff có đúng success/fail chưa?
- Handoff có giữ board/live/attribution context không?
- MC AI script có dùng đúng board không?
- MC AI video segment có approved/revalidated không?
- AI có báo giá bằng QuoteSnapshot không?
- Order có CustomerConfirmation và order_code không?
- ROAS có tính bằng ORDER_VERIFIED không?
- CRM có suppression không?
- Duplicate webhook có bị chặn không?
- FILE 12 có override claim/runtime không?
- Owner có bằng chứng để ký PASS không?

Evidence Package là điều kiện để Completion Report chuyển từ:

PENDING_EVIDENCE

sang:

PASS

**3\. ROOT OBJECT - EVIDENCE PACKAGE**

evidence_package:  
package_id: string  
package_code: string  
package_name: string  
package_version: "V2.4"  
<br/>related_completion_report:  
report_code: "AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2_4"  
report_version: "V2.4"  
<br/>package_scope:  
\- AI_CHANNEL  
\- FACEBOOK_LIVE  
\- ADS_CONTEXT  
\- MESSENGER_HANDOFF  
\- MC_AI_LIVE  
\- DAILY_LIVE_PRODUCT_BOARD  
\- QUOTE_SNAPSHOT  
\- ORDER_RUNTIME  
\- CRM  
\- ROAS  
\- GATEWAY_TRANSITION  
<br/>environment:  
\- LOCAL  
\- STAGING  
\- PILOT  
\- PRODUCTION  
<br/>status:  
\- DRAFT  
\- COLLECTING_EVIDENCE  
\- READY_FOR_QA_REVIEW  
\- QA_REVIEWING  
\- PASS  
\- FAIL  
\- BLOCKED  
\- RETIRED  
<br/>created_by: string  
created_at: datetime  
updated_at: datetime  
<br/>owner_review_required: true  
qa_review_required: true  
gateway_transition_allowed: false  
<br/>summary:  
total_gates: number  
total_p0: number  
p0_pass: number  
p0_fail: number  
p0_pending: number  
p0_blocked: number  
open_issue_count: number  
<br/>evidence_items:  
\- evidence_item

MUST:

- package_id là duy nhất.
- package_version phải khớp version tài liệu.
- environment phải rõ.
- Production không được dùng evidence từ local/dev làm PASS cuối.
- gateway_transition_allowed = true chỉ khi tất cả P0 PASS và owner sign-off PASS.

**4\. EVIDENCE ITEM BASE SCHEMA**

evidence_item:  
evidence_id: string  
evidence_code: string  
gate_code: string  
gate_name: string  
<br/>priority:  
\- P0  
\- P1  
\- P2  
<br/>evidence_type:  
\- DECISION_ENVELOPE  
\- RESOLVER_TRACE  
\- GUARD_TRACE  
\- FINAL_GUARD_TRACE  
\- NORMALIZED_EVENT  
\- HANDOFF_DELIVERY_LOG  
\- BOARD_CONTEXT  
\- SCRIPT_BRIEF  
\- MC_AI_SEGMENT  
\- VIDEO_REVALIDATION  
\- QUOTE_SNAPSHOT  
\- ORDER_RUNTIME  
\- CRM_SUPPRESSION  
\- ROAS_MAPPING  
\- DUPLICATE_IDEMPOTENCY  
\- ADMIN_UI_SCREEN  
\- API_REQUEST_RESPONSE  
\- DB_SNAPSHOT  
\- WORKER_RUN  
\- SCREENSHOT  
\- OWNER_APPROVAL  
\- QA_NOTE  
<br/>status:  
\- PASS  
\- FAIL  
\- PENDING  
\- BLOCKED  
\- ACCEPTED_RISK  
<br/>tested_at: datetime  
tested_by: string  
reviewed_by: optional string  
reviewed_at: optional datetime  
<br/>source_environment:  
\- LOCAL  
\- STAGING  
\- PILOT  
\- PRODUCTION  
<br/>related_ids:  
correlation_id: optional string  
idempotency_key: optional string  
message_id: optional string  
conversation_id: optional string  
board_id: optional string  
session_board_id: optional string  
brief_id: optional string  
segment_id: optional string  
handoff_id: optional string  
quote_snapshot_id: optional string  
order_code: optional string  
sales_session_id: optional string  
attribution_id: optional string  
<br/>artifact_refs:  
\- artifact_ref  
<br/>pii_safety:  
pii_present: boolean  
pii_masked: boolean  
raw_pii_removed: boolean  
reviewer_can_access_raw_pii: false  
<br/>validation_result:  
required_artifacts_present: boolean  
schema_valid: boolean  
guard_passed: boolean  
no_forbidden_leak: boolean  
<br/>fail_reason: optional string  
required_fix_file: optional string  
required_fix_module: optional string  
owner_decision_required: boolean

MUST:

- P0 evidence item không được thiếu artifact_refs.
- P0 evidence item không được để status = ACCEPTED_RISK nếu chưa có owner exception.
- Nếu pii_present = true, thì pii_masked = true và raw_pii_removed = true.
- correlation_id bắt buộc nếu evidence liên quan flow runtime.
- idempotency_key bắt buộc nếu evidence liên quan command/state change.
- Evidence FAIL phải có fail_reason.

**5\. ARTIFACT REFERENCE SCHEMA**

artifact_ref:  
artifact_id: string  
artifact_type:  
\- JSON_PAYLOAD  
\- API_REQUEST  
\- API_RESPONSE  
\- DB_RECORD  
\- LOG_RECORD  
\- DECISION_ENVELOPE  
\- RESOLVER_TRACE  
\- GUARD_TRACE  
\- SCREENSHOT  
\- VIDEO_SEGMENT  
\- SCRIPT_FILE  
\- BOARD_EXPORT  
\- CSV_EXPORT  
\- PDF_EXPORT  
\- ADMIN_SCREEN  
\- TEST_REPORT  
<br/>artifact_name: string  
artifact_location: string  
storage_policy:  
\- INTERNAL_ONLY  
\- QA_REVIEW_ONLY  
\- OWNER_REVIEW  
\- PRODUCTION_AUDIT  
<br/>privacy_class:  
\- PUBLIC_SAFE  
\- INTERNAL  
\- SENSITIVE  
\- SECRET_REF_ONLY  
<br/>hash: optional string  
created_at: datetime  
created_by: string

MUST:

- Không lưu token thật trong artifact.
- Secret chỉ được lưu bằng SECRET_REF_ONLY.
- Evidence có raw PII phải lưu trong khu vực kiểm soát, không đưa vào bản review rộng.
- File video segment có thể lưu path/ref, không cần embed toàn bộ video vào báo cáo.

**6\. REQUIRED GATE EVIDENCE MAP**

**6.1. GATE-01 - Channel Context Evidence**

GATE_01_CHANNEL_CONTEXT_EVIDENCE:  
required_evidence_types:  
\- DECISION_ENVELOPE  
\- API_REQUEST_RESPONSE  
\- GUARD_TRACE  
<br/>required_samples:  
\- WEB_CHAT_PRIVATE  
\- LANDING_PAGE_CHAT_PRIVATE  
\- MESSENGER_PRIVATE  
\- LIVE_COMMENT_PUBLIC  
\- POST_COMMENT_PUBLIC  
\- LIVE_INBOX_PRIVATE  
\- CRM_OUTBOUND_PRIVATE  
\- ADMIN_PREVIEW_INTERNAL  
<br/>required_fields:  
\- channel_code  
\- reply_surface  
\- outbound_allowed  
\- channel_policy_status  
\- selected_action  
\- blocked_actions

PASS khi:

- Tất cả channel mẫu có DecisionEnvelope.
- Live comment có reply_surface = PUBLIC.
- Messenger/Live Inbox có reply_surface = PRIVATE.
- Admin Preview có reply_surface = INTERNAL.

FAIL nếu:

- Live comment bị gắn PRIVATE.
- Admin Preview có outbound thật.
- Thiếu reply_surface.

**6.2. GATE-02 - Facebook / Ads / Live Context Evidence**

GATE_02_FACEBOOK_ADS_LIVE_CONTEXT_EVIDENCE:  
required_evidence_types:  
\- NORMALIZED_EVENT  
\- DECISION_ENVELOPE  
\- RESOLVER_TRACE  
\- GUARD_TRACE  
<br/>required_fields:  
\- source_page_id  
\- commerce_hub_page_id  
\- page_role  
\- live_session_id  
\- comment_id  
\- messenger_thread_id_if_private  
\- attribution_id_if_ads  
\- campaign_id_internal_if_ads  
\- adset_id_internal_if_ads  
\- ad_id_internal_if_ads  
\- referral_link_id_if_diamond  
\- entry_channel  
<br/>forbidden_customer_facing:  
\- campaign_id  
\- adset_id  
\- ad_id  
\- attribution_id  
\- ROAS  
\- page_token_ref  
\- psid

PASS khi:

- Context được lưu trong DecisionEnvelope.
- Ads attribution internal-only.
- Live context không mất khi vào Messenger.
- Diamond context chỉ dùng khi resolver pass.

FAIL nếu:

- Customer-facing response có campaign/ad/ROAS.
- Handoff làm mất live_session_id.
- Diamond bind bị bịa.

**6.3. GATE-03 - Public / Private Boundary Evidence**

GATE_03_PUBLIC_PRIVATE_BOUNDARY_EVIDENCE:  
required_evidence_types:  
\- DECISION_ENVELOPE  
\- GUARD_TRACE  
\- FINAL_GUARD_TRACE  
\- CONTENT_RENDER_RESULT  
<br/>required_test_cases:  
\- live_public_price_signal  
\- live_public_order_signal  
\- live_public_PII_signal  
\- live_public_payment_signal  
\- live_public_invoice_signal  
\- live_public_health_sensitive_signal  
<br/>blocked_public_fields:  
\- final_quote_price  
\- listed_price_current  
\- quote_snapshot_id  
\- order_code  
\- raw_phone  
\- raw_address  
\- payment_instruction  
\- health_sensitive_detail

PASS khi:

- Public price bị chặn.
- Public order confirmation bị chặn.
- PII không lặp lại.
- Payment không public.
- Health-sensitive không tư vấn dài public.

FAIL nếu:

- Public response chứa final price.
- Public response xác nhận đơn.
- Public response lặp PII.

**6.4. GATE-04 - Messenger Handoff Evidence**

GATE_04_MESSENGER_HANDOFF_EVIDENCE:  
required_evidence_types:  
\- NORMALIZED_EVENT  
\- HANDOFF_DELIVERY_LOG  
\- PUBLIC_REPLY_LOG  
\- DECISION_ENVELOPE  
\- DB_SNAPSHOT  
<br/>required_cases:  
\- handoff_success_price_signal  
\- handoff_success_order_signal  
\- handoff_success_PII_signal  
\- handoff_success_health_signal  
\- handoff_failed_safe_fallback  
\- duplicate_handoff_blocked  
<br/>required_fields:  
\- handoff_id  
\- handoff_status  
\- public_reply_status  
\- private_reply_status  
\- board_id_if_live  
\- session_board_id_if_live  
\- live_session_id_if_live  
\- attribution_id_if_ads  
\- delivery_log_id

PASS khi:

- Handoff success mới nói "đã gửi".
- Handoff fail dùng fallback.
- Không có CTA yêu cầu khách tự inbox.
- Handoff giữ board/live/attribution.

FAIL nếu:

- Handoff fail mà nói "đã gửi".
- Public reply có "Mình inbox/nhắn Messenger".
- Không có delivery log.

**6.5. GATE-05 - MC AI / Live Board Evidence**

GATE_05_MC_AI_LIVE_BOARD_EVIDENCE:  
required_evidence_types:  
\- BOARD_CONTEXT  
\- SCRIPT_BRIEF  
\- MC_AI_SEGMENT  
\- VIDEO_REVALIDATION  
\- ADMIN_UI_SCREEN  
\- TEST_REPORT  
<br/>required_artifacts:  
\- daily_live_product_board_export  
\- live_session_product_board_export  
\- ai_live_script_brief_export  
\- script_segment_registry  
\- video_segment_registry  
\- revalidation_report  
\- approved_playlist_report  
<br/>required_fields:  
\- board_id  
\- session_board_id  
\- brief_id  
\- segment_id  
\- segment_code  
\- video_status  
\- revalidation_status  
\- approved_for_live

PASS khi:

- Script chỉ chứa SKU trong board.
- Segment SKU map đúng board line.
- Không segment nào REVALIDATION_REQUIRED.
- Chỉ clip APPROVED_FOR_LIVE vào playlist.
- Không giá cuối/tồn kho/CTA tự inbox/claim điều trị.

FAIL nếu:

- Script có SKU ngoài board.
- Clip có SKU bị remove.
- Clip chưa approved nhưng phát.
- Board thay đổi nhưng không revalidate.

**6.6. GATE-06 - Ads / ROAS Internal-only Evidence**

GATE_06_ROAS_INTERNAL_ONLY_EVIDENCE:  
required_evidence_types:  
\- GUARD_TRACE  
\- ROAS_MAPPING  
\- DB_SNAPSHOT  
\- DASHBOARD_SAMPLE  
\- CONTENT_RENDER_RESULT  
<br/>required_cases:  
\- customer_response_no_campaign  
\- customer_response_no_ad  
\- customer_response_no_ROAS  
\- quote_not_counted_as_revenue  
\- created_order_not_counted_as_verified_revenue  
\- ORDER_VERIFIED_counted_as_revenue  
\- COD_FAILED_excluded  
\- CANCELLED_excluded  
\- RETURNED_excluded

PASS khi:

- Customer response không có Ads/ROAS internal fields.
- ROAS chỉ tính ORDER_VERIFIED.
- COD fail/cancel/return bị loại khỏi revenue.

FAIL nếu:

- Dashboard tính created order là revenue verified.
- AI public campaign/ad.
- Quote/inbox được tính doanh thu.

**6.7. GATE-07 - Golden Hour Evidence**

GATE_07_GOLDEN_HOUR_EVIDENCE:  
required_evidence_types:  
\- RESOLVER_TRACE  
\- QUOTE_SNAPSHOT  
\- GUARD_TRACE  
\- DB_SNAPSHOT  
\- TEST_REPORT  
<br/>required_cases:  
\- GoldenHourResolver_ACTIVE  
\- GoldenHourResolver_NOT_ACTIVE  
\- quote_hold_5_minutes  
\- program_24_7_hold_15_minutes  
\- expired_quote_revalidation  
\- fake_urgency_blocked  
\- combo_no_force_during_golden_hour

PASS khi:

- Golden Hour active do resolver trả.
- Quote Giờ Vàng giữ 5 phút.
- Quote 24/7 giữ 15 phút.
- Quote hết hạn revalidate.
- Không ép combo vì Giờ Vàng.

FAIL nếu:

- AI bịa Giờ Vàng active.
- Quote hết hạn vẫn tạo order.
- AI public giá Giờ Vàng.

**6.8. GATE-08 - Sales-to-Order Evidence**

GATE_08_SALES_TO_ORDER_EVIDENCE:  
required_evidence_types:  
\- DECISION_ENVELOPE  
\- QUOTE_SNAPSHOT  
\- ORDER_RUNTIME  
\- DB_SNAPSHOT  
\- API_REQUEST_RESPONSE  
\- DUPLICATE_IDEMPOTENCY  
<br/>required_artifacts:  
\- advisor_sales_session_record  
\- quote_cart_record  
\- quote_snapshot_record  
\- order_confirmation_draft_record  
\- customer_confirmation_record  
\- order_created_from_confirmed_quote_record  
\- order_code_record  
\- idempotency_test_report

PASS khi:

- AI có sales_session.
- Quote có QuoteSnapshot.
- Order confirmation draft không bị gọi là order success.
- Order chỉ tạo sau CustomerConfirmation.
- Order success có order_code.
- Retry không tạo duplicate order.

FAIL nếu:

- AI báo giá thiếu QuoteSnapshot.
- Order tạo thiếu CustomerConfirmation.
- Không có order_code nhưng nói đơn đã ghi nhận.

**6.9. GATE-09 - Entry / Growth / Combo Evidence**

GATE_09_ENTRY_GROWTH_COMBO_EVIDENCE:  
required_evidence_types:  
\- DECISION_ENVELOPE  
\- RESOLVER_TRACE  
\- GUARD_TRACE  
\- CONTENT_RENDER_RESULT  
\- QUOTE_SNAPSHOT_IF_QUOTE  
<br/>required_cases:  
\- entry_order_one_box_allowed  
\- growth_order_family  
\- growth_order_gift  
\- combo_parents  
\- combo_family  
\- combo_refusal_fallback_to_entry  
\- combo_no_force_guard  
\- combo_items_product_effectiveness

PASS khi:

- Entry Order hợp lệ.
- Growth Order có reason.
- Combo có Product Effectiveness từng item.
- Khách từ chối combo vẫn được mua 1 hộp nếu guard pass.
- Không ép combo.

FAIL nếu:

- AI nói "phải mua combo".
- AI xem 1 hộp là thất bại.
- Combo thiếu Product Effectiveness.
- Combo chứa SKU không sellable.

**6.10. GATE-10 - CRM Evidence**

GATE_10_CRM_EVIDENCE:  
required_evidence_types:  
\- CRM_SUPPRESSION  
\- DECISION_ENVELOPE  
\- GUARD_TRACE  
\- CONTENT_RENDER_RESULT  
\- CUSTOMER_MEMORY_SAMPLE  
<br/>required_cases:  
\- CRM_eligible_customer  
\- opt_out_suppressed  
\- open_case_suppressed  
\- message_fatigue_suppressed  
\- CRM_with_ProductEffectiveness  
\- no_fake_purchase_history

PASS khi:

- CRMEligibilityGuard chạy.
- Opt-out bị chặn.
- Open case bị chặn.
- CRM có Product Effectiveness khi gợi ý sản phẩm.
- Không bịa lịch sử mua.

FAIL nếu:

- CRM gửi khi khách opt-out.
- CRM gửi khi có case chất lượng/hàng giả/privacy/payment.
- CRM gợi ý sản phẩm hời hợt không có hiệu dụng.

**6.11. GATE-11 - Duplicate / Idempotency Evidence**

GATE_11_DUPLICATE_IDEMPOTENCY_EVIDENCE:  
required_evidence_types:  
\- DUPLICATE_IDEMPOTENCY  
\- NORMALIZED_EVENT  
\- DB_SNAPSHOT  
\- TEST_REPORT  
<br/>required_cases:  
\- duplicate_comment_no_second_public_reply  
\- duplicate_webhook_no_second_handoff  
\- duplicate_quote_command_no_second_quote  
\- duplicate_order_command_no_second_order  
\- duplicate_commission_command_no_second_ledger

PASS khi:

- Một comment không tạo nhiều public reply.
- Một webhook retry không tạo nhiều handoff.
- Một confirmed quote không tạo nhiều order.
- Commission không double-ledger.

FAIL nếu:

- Duplicate event tạo duplicate reply/quote/order/commission.

**6.12. GATE-12 - FILE 12 Non-Override Evidence**

GATE_12_FILE12_NON_OVERRIDE_EVIDENCE:  
required_evidence_types:  
\- GUARD_TRACE  
\- TEST_REPORT  
\- CONTENT_RENDER_RESULT  
\- OWNER_APPROVAL  
<br/>required_cases:  
\- no_new_claim  
\- no_new_effectiveness  
\- no_runtime_bypass  
\- no_guard_bypass  
\- no_forced_combo  
\- no_public_campaign_ROAS  
\- regression_pass

PASS khi:

- FILE 12 không tạo claim mới.
- FILE 12 không tạo hiệu dụng mới.
- FILE 12 không bỏ guard.
- FILE 12 không ép combo.
- Regression pass.

FAIL nếu:

- FILE 12 vượt FILE 00→11.
- FILE 12 dùng conversion để bỏ safety/claim/runtime.

**7\. ARTIFACT TYPE CONTRACTS**

**7.1. DecisionEnvelope Evidence**

decision_envelope_evidence:  
artifact_type: DECISION_ENVELOPE  
<br/>required_fields:  
\- envelope_id  
\- correlation_id  
\- idempotency_key  
\- message_id  
\- conversation_id  
\- channel_code  
\- reply_surface  
\- selected_action  
\- blocked_actions  
\- resolvers_called  
\- guards_evaluated  
\- final_guard_status  
\- side_effects_created  
\- side_effects_blocked  
<br/>conditional_required_fields:  
if_live:  
\- live_session_id  
\- board_id  
\- session_board_id  
<br/>if_ads:  
\- attribution_id  
\- campaign_id_internal  
\- ad_id_internal  
<br/>if_quote:  
\- sales_session_id  
\- quote_snapshot_id  
<br/>if_order:  
\- customer_confirmation_status  
\- order_code

MUST NOT:

- Không để DecisionEnvelope raw xuất cho khách.
- Không chứa raw PII không mask trong review package.

**7.2. Resolver Trace Evidence**

resolver_trace_evidence:  
artifact_type: RESOLVER_TRACE  
<br/>required_fields:  
\- resolver_name  
\- resolver_status  
\- input_summary  
\- output_summary  
\- fail_reason_code_if_any  
\- executed_at  
\- correlation_id  
<br/>important_resolvers:  
\- ChannelContextResolver  
\- FacebookContextResolver  
\- LiveSessionResolver  
\- AdsAttributionResolver  
\- MessengerHandoffResolver  
\- ProductKnowledgeResolver  
\- ProgramPriceResolver  
\- GoldenHourResolver  
\- MemberRightsResolver  
\- DiamondReferralResolver  
\- QuoteSnapshotResolver  
\- OrderRuntimeResolver  
\- CRMEligibilityResolver

MUST:

- Resolver output có cấu trúc.
- Không lưu token/secret/raw PII trong resolver trace rộng.

**7.3. Guard Trace Evidence**

guard_trace_evidence:  
artifact_type: GUARD_TRACE  
<br/>required_fields:  
\- guard_name  
\- guard_status  
\- checked_rules  
\- blocked_fields  
\- blocked_actions  
\- public_safe_reason_if_customer_facing  
\- internal_reason_code  
\- executed_at  
\- correlation_id  
<br/>important_guards:  
\- PublicPrivateSurfaceGuard  
\- PrivacyPIIGuard  
\- ClaimGuard  
\- FinalGuard  
\- ROASInternalOnlyGuard  
\- QuoteSnapshotGuard  
\- PriceLockGuard  
\- CustomerConfirmationGuard  
\- OrderCodeGuard  
\- ComboNoForceGuard  
\- CRMEligibilityGuard  
\- OpenCaseSuppressionGuard  
\- MetaDuplicateEventGuard

MUST:

- Guard fail phải tạo blocked_actions.
- Guard internal reason không public cho khách.

**7.4. Normalized Event Evidence**

normalized_event_evidence:  
artifact_type: NORMALIZED_EVENT  
<br/>required_fields:  
\- normalized_event_id  
\- source_platform  
\- source_page_id  
\- event_type  
\- received_at  
\- idempotency_key  
\- correlation_id  
\- raw_payload_ref  
\- payload_signature_status  
\- dedup_status  
<br/>conditional_required_fields:  
if_live_comment:  
\- live_session_id  
\- comment_id  
\- board_id  
\- session_board_id  
<br/>if_ads:  
\- attribution_id  
\- campaign_id_internal  
\- ad_id_internal  
<br/>if_handoff:  
\- handoff_required  
\- handoff_reason

MUST:

- raw_payload_ref internal only.
- Signature status phải rõ.
- Dedup status phải rõ.

**7.5. Handoff Evidence**

handoff_evidence:  
artifact_type: HANDOFF_DELIVERY_LOG  
<br/>required_fields:  
\- handoff_id  
\- source_event_id  
\- handoff_trigger  
\- handoff_status  
\- public_reply_status  
\- private_reply_status  
\- delivery_log_id  
\- sent_at  
\- fail_reason_code_if_any  
<br/>required_context:  
\- board_id_if_live  
\- session_board_id_if_live  
\- live_session_id_if_live  
\- attribution_id_if_ads  
\- conversation_id  
\- messenger_thread_id_if_created  
<br/>validation:  
say_sent_allowed_only_if:  
\- handoff_status = SENT  
\- public_reply_status = SENT

MUST:

- Handoff fail không được có "đã gửi Messenger".
- Handoff success phải có delivery log.

**7.6. MC AI Segment Evidence**

mc_ai_segment_evidence:  
artifact_type: MC_AI_SEGMENT  
<br/>required_fields:  
\- segment_id  
\- board_id  
\- session_board_id  
\- brief_id  
\- segment_code  
\- segment_group  
\- segment_role  
\- script_version  
\- video_file_ref  
\- video_status  
\- revalidation_status  
\- approved_for_live  
\- approved_by  
\- approved_at  
<br/>conditional_required_fields:  
if_sku_segment:  
\- sku_id  
\- product_public_name  
\- live_board_sku_line_id  
<br/>validation_checks:  
\- sku_in_board  
\- no_final_price  
\- no_stock_quantity  
\- no_forbidden_CTA  
\- no_treatment_claim  
\- no_campaign_ROAS

MUST:

- Clip SKU phải map với board line.
- Clip REVALIDATION_REQUIRED không được phát.
- Clip chưa APPROVED_FOR_LIVE không được vào playlist.

**7.7. QuoteSnapshot Evidence**

quote_snapshot_evidence:  
artifact_type: QUOTE_SNAPSHOT  
<br/>required_fields:  
\- quote_snapshot_id  
\- sales_session_id  
\- quote_status  
\- product_lines  
\- final_payable  
\- quote_hold_minutes  
\- quote_created_at  
\- quote_expires_at  
\- applied_benefits  
\- blocked_benefits_if_any  
<br/>conditional_required_fields:  
if_golden_hour:  
\- quote_hold_minutes = 5  
<br/>if_program_24_7:  
\- quote_hold_minutes = 15  
<br/>if_member:  
\- member_tier  
\- member_benefit  
<br/>if_diamond:  
\- diamond_bind_status  
\- diamond_benefit_if_applied  
<br/>if_ads_or_live:  
\- attribution_snapshot  
\- live_session_id_if_live

MUST:

- Final price chỉ từ QuoteSnapshot.
- QuoteSnapshot ID không customer-facing.
- Quote hết hạn phải có revalidation evidence nếu tạo order sau đó.

**7.8. Order Runtime Evidence**

order_runtime_evidence:  
artifact_type: ORDER_RUNTIME  
<br/>required_fields:  
\- order_code  
\- quote_snapshot_id  
\- sales_session_id  
\- customer_confirmation_status  
\- order_status  
\- created_at  
\- idempotency_key  
<br/>required_links:  
\- quote_snapshot_record  
\- order_confirmation_draft_record  
\- customer_confirmation_record  
\- order_items_from_quote_snapshot  
<br/>validation:  
order_success_allowed_only_if:  
\- order_code present  
\- order_status created_or_confirmed_by_runtime

MUST:

- Không có order_code thì không render order success.
- Order item không nhập lại từ AI text.

**7.9. CRM Suppression Evidence**

crm_suppression_evidence:  
artifact_type: CRM_SUPPRESSION  
<br/>required_fields:  
\- customer_id_or_guest_ref  
\- consent_status  
\- crm_eligibility_status  
\- suppression_reasons  
\- open_case_status  
\- message_fatigue_status  
\- selected_action  
\- outbound_created  
<br/>validation:  
outbound_created_must_be_false_if:  
\- consent_status = OPTED_OUT  
\- open_case_status in \[QUALITY, COUNTERFEIT, PRIVACY, PAYMENT\]  
\- message_fatigue_status = EXCEEDED

MUST:

- CRM không gửi khi suppression fail.
- CRM gợi ý sản phẩm phải có Product Effectiveness evidence.

**7.10. ROAS Mapping Evidence**

roas_mapping_evidence:  
artifact_type: ROAS_MAPPING  
<br/>required_fields:  
\- order_code  
\- order_status  
\- verified_at  
\- revenue_value  
\- attribution_id  
\- campaign_id_internal  
\- ad_id_internal  
\- live_session_id_if_live  
\- dashboard_projection_id  
<br/>validation:  
revenue_counted_only_if:  
\- order_status = ORDER_VERIFIED  
<br/>revenue_excluded_if:  
\- COD_FAILED  
\- CANCELLED  
\- RETURNED  
\- TEST_ORDER  
\- DUPLICATE_ORDER

MUST:

- ROAS không tính quote/inbox/comment.
- ROAS không public cho khách.

**8\. EVIDENCE FILE NAMING STANDARD**

evidence_file_naming:  
pattern: "YYYYMMDD_ENV_GATECODE_EVIDENCETYPE_SEQUENCE_VERSION"  
<br/>examples:  
\- "20260513_STAGING_GATE03_GUARDTRACE_001_V01.json"  
\- "20260513_STAGING_GATE04_HANDOFFLOG_001_V01.json"  
\- "20260513_STAGING_GATE05_MCAI_SEGMENT_001_V01.json"  
\- "20260513_STAGING_GATE08_QUOTESNAPSHOT_001_V01.json"  
\- "20260513_STAGING_GATE11_IDEMPOTENCY_001_V01.json"

MUST NOT:

- Không đặt tên file "test ok".
- Không đặt "final final".
- Không đặt tên chứa số điện thoại khách.
- Không đặt tên chứa token/secret.

**9\. EVIDENCE PRIVACY & SECURITY**

**9.1. PII Masking**

pii_masking_policy:  
phone:  
display: "09\*\*\*\*\*\*123"  
address:  
display: "masked_address"  
email:  
display: "u\*\*\*@domain.com"  
tax_code:  
display: "masked_tax_code"  
psid:  
display: "hashed_psid"

MUST:

- PII phải mask trước khi đưa vào owner/QA package rộng.
- Raw PII chỉ giữ trong system storage theo quyền.
- Evidence không được lặp lại PII public comment.

**9.2. Secret Handling**

Forbidden in evidence:

- Page access token thật.
- App secret.
- Verify token.
- Webhook secret.
- Payment secret.
- Bank credential.
- API key.
- Cookie/session token.

MUST:

- Chỉ lưu secret_ref.
- Screenshot không được lộ token.

**10\. EVIDENCE VALIDATION RULES**

evidence_validation_rules:  
EV-P0-001:  
name: "P0 evidence required"  
fail_if:  
\- priority = P0  
\- artifact_refs empty  
<br/>EV-P0-002:  
name: "DecisionEnvelope required for AI decision"  
fail_if:  
\- evidence_type relates_to_AI  
\- DecisionEnvelope missing  
<br/>EV-P0-003:  
name: "Guard trace required for blocked response"  
fail_if:  
\- selected_action = BLOCK_RESPONSE  
\- guard_trace missing  
<br/>EV-P0-004:  
name: "Handoff log required"  
fail_if:  
\- gate_code = GATE-04  
\- handoff_delivery_log missing  
<br/>EV-P0-005:  
name: "QuoteSnapshot required for price"  
fail_if:  
\- final_price_rendered = true  
\- quote_snapshot missing  
<br/>EV-P0-006:  
name: "Order code required for order success"  
fail_if:  
\- order_success_rendered = true  
\- order_code missing  
<br/>EV-P0-007:  
name: "MC AI segment approval required"  
fail_if:  
\- segment_in_playlist = true  
\- approved_for_live != true  
<br/>EV-P0-008:  
name: "Revalidation required before live"  
fail_if:  
\- board_or_segment_changed = true  
\- revalidation_status != PASS  
<br/>EV-P0-009:  
name: "No raw PII in review package"  
fail_if:  
\- pii_present = true  
\- pii_masked = false  
<br/>EV-P0-010:  
name: "No secret in evidence"  
fail_if:  
\- artifact_contains_secret = true

**11\. PACKAGE REVIEW WORKFLOW**

evidence_package_review_workflow:  
DRAFT:  
next:  
\- COLLECTING_EVIDENCE  
<br/>COLLECTING_EVIDENCE:  
next:  
\- READY_FOR_QA_REVIEW  
\- BLOCKED  
<br/>READY_FOR_QA_REVIEW:  
next:  
\- QA_REVIEWING  
<br/>QA_REVIEWING:  
next:  
\- PASS  
\- FAIL  
\- BLOCKED  
<br/>PASS:  
next:  
\- OWNER_SIGN_OFF  
<br/>FAIL:  
next:  
\- FIX_REQUIRED  
<br/>BLOCKED:  
next:  
\- FIX_REQUIRED  
<br/>OWNER_SIGN_OFF:  
next:  
\- GATEWAY_TRANSITION_ALLOWED  
\- REQUEST_FIX

MUST:

- QA không được PASS nếu thiếu P0 evidence.
- Owner không ký nếu còn P0 FAIL/PENDING.
- Gateway Transition chỉ mở sau Owner Sign-off.

**12\. EVIDENCE PACKAGE SUMMARY TABLE**

| **Gate** | **Evidence bắt buộc**               | **PASS khi**                    |
| -------- | ----------------------------------- | ------------------------------- |
| GATE-01  | DecisionEnvelope channel samples    | Nhận đúng channel/surface       |
| GATE-02  | Normalized event + ads/live context | Context internal đầy đủ         |
| GATE-03  | Guard trace public/private          | Public không leak giá/PII/order |
| GATE-04  | Handoff delivery log                | Handoff success/fail đúng       |
| GATE-05  | Board/script/segment evidence       | MC AI dùng đúng board           |
| GATE-06  | ROAS mapping                        | ROAS chỉ từ ORDER_VERIFIED      |
| GATE-07  | GoldenHourResolver + QuoteSnapshot  | GH/24-7 hold đúng               |
| GATE-08  | Sales session / quote / order       | Quote/order đúng gate           |
| GATE-09  | Entry/Growth/Combo traces           | Không ép combo, có hiệu dụng    |
| GATE-10  | CRM suppression                     | CRM không spam/sai lúc          |
| GATE-11  | Idempotency report                  | Không duplicate                 |
| GATE-12  | FILE 12 regression                  | Không override                  |

**13\. MASTER DONE GATE**

Evidence Package Schema đạt chuẩn khi:

- Có root package schema.
- Có evidence item schema.
- Có artifact ref schema.
- Có required evidence map cho GATE-01 → GATE-12.
- Có artifact type contract.
- Có DecisionEnvelope evidence contract.
- Có resolver trace contract.
- Có guard trace contract.
- Có Gateway normalized event evidence.
- Có handoff evidence.
- Có MC AI segment evidence.
- Có QuoteSnapshot evidence.
- Có Order Runtime evidence.
- Có CRM suppression evidence.
- Có ROAS mapping evidence.
- Có duplicate/idempotency evidence.
- Có privacy/PII masking policy.
- Có secret handling policy.
- Có validation rules.
- Có review workflow.
- Có fail gate rõ.
- Có owner sign-off linkage.

**14\. MASTER FAIL GATE**

Evidence Package FAIL nếu:

- P0 evidence thiếu artifact.
- Không có DecisionEnvelope cho AI decision.
- Không có guard trace cho public/private block.
- Không có handoff delivery log.
- Không có board/script/segment evidence cho MC AI.
- Không có QuoteSnapshot cho báo giá.
- Không có CustomerConfirmation / order_code cho order success.
- Không có ORDER_VERIFIED mapping cho ROAS.
- Không có CRM suppression evidence.
- Không có idempotency evidence.
- Evidence chứa raw PII chưa mask.
- Evidence chứa token/secret.
- Screenshot được dùng thay runtime evidence.
- Owner ký khi còn P0 FAIL/PENDING.

**15\. OWNER SIGN-OFF LINKAGE**

owner_sign_off_linkage:  
evidence_package_id: string  
completion_report_id: string  
<br/>sign_off_allowed_only_if:  
\- all_P0_evidence_status_PASS  
\- no_P0_open_issue  
\- evidence_schema_valid  
\- QA_review_PASS  
\- privacy_review_PASS_if_PII  
\- security_review_PASS_if_gateway_related  
<br/>sign_off_roles:  
\- Owner  
\- Project Lead  
\- AI Lead  
\- Core Runtime Lead  
\- Gateway Lead  
\- QA Lead  
\- Ops_MC_AI_Lead

MUST:

- Owner sign-off phải gắn với evidence_package_id.
- Không ký PASS bằng miệng.
- Không ký PASS khi evidence còn thiếu.

**16\. KẾT LUẬN KHÓA**

Tài liệu **AI_CHANNEL_FACEBOOK_EVIDENCE_PACKAGE_SCHEMA_V2.4** là chuẩn bắt buộc để thu thập, lưu, kiểm tra và ký duyệt evidence cho:

- AI Advisor Facebook Completion Report.
- MC AI Live readiness.
- Facebook Gateway transition.
- Live-to-Order E2E smoke.
- Owner Sign-off.

Từ sau tài liệu này:

- Completion Report không được PASS nếu thiếu evidence.
- Gateway không được production nếu Evidence Package chưa PASS.
- MC AI không được live nếu thiếu board/script/video/revalidation evidence.
- AI không được quote nếu thiếu QuoteSnapshot evidence.
- Order không được success nếu thiếu order_code evidence.
- ROAS không được tính nếu thiếu ORDER_VERIFIED evidence.
- CRM không được gửi nếu thiếu eligibility/suppression evidence.
- Không có P0 evidence thì không release.

**Trạng thái mặc định sau khi tạo schema:**  
READY_FOR_EVIDENCE_COLLECTION

**Bước tiếp theo:**  
Viết **MC_AI_VIDEO_SEGMENT_REGISTRY_CONTRACT** để khóa object quản lý clip, trạng thái video, version, board mapping và revalidation trước live.

**HẾT TÀI LIỆU - AI_CHANNEL_FACEBOOK_EVIDENCE_PACKAGE_SCHEMA_V2.4**

**AI_CHANNEL_DEV_IMPLEMENTATION_BRIEF_V2.4**

**AI TÍCH HỢP CHANNEL / FACEBOOK-ADS-LIVE / MC AI / MESSENGER / QUOTE / ORDER / CRM DEV BRIEF**

**Hệ thống:** AI Advisor Ginsengfood - AI Channel Runtime  
**Liên kết:** Ginsengfood Core Runtime / Daily Live Product Board / MC AI Live / Facebook Channel Gateway / Messenger / QuoteSnapshot / Order Runtime / CRM / ROAS  
**Vai trò tài liệu:** Dev Implementation Brief / Task Breakdown Guide / Module-API-Resolver-Guard-Test Map / Gateway Prerequisite Brief  
**Trạng thái:** CLEAN FINAL - READY FOR DEV TASK BREAKDOWN  
**Không phải:** Source-of-truth nghiệp vụ mới / tài liệu Core mới / tài liệu Facebook Gateway production / tài liệu code copy-paste  
**Vị trí:** Sau AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2.4 và AI_CHANNEL_FACEBOOK_EVIDENCE_PACKAGE_SCHEMA_V2.4; trước FB-09 - FACEBOOK GATEWAY DEV IMPLEMENTATION BACKLOG.

**0\. MỤC ĐÍCH TÀI LIỆU**

Tài liệu này dùng để chuyển toàn bộ phần **AI Tích hợp Channel / Facebook-Ads-Live / MC AI readiness** thành danh mục triển khai kỹ thuật cho dev.

Dev đọc tài liệu này phải biết:

- Module nào cần triển khai.
- Resolver nào cần có.
- Guard nào cần có.
- API nào cần tạo hoặc tích hợp.
- DB/log/evidence nào cần lưu.
- Worker/job nào cần chạy.
- Admin/preview nào cần hỗ trợ.
- Test P0 nào bắt buộc pass.
- Evidence nào cần xuất.
- Khi nào mới được chuyển sang Facebook Gateway production.

Tài liệu này không yêu cầu dev copy code.

Một hệ thống AI Channel thực tế không thể xây bằng cách copy vài đoạn code AI rồi dán vào. Dev phải triển khai theo kiến trúc thật gồm API, DTO, DB, resolver, guard, orchestrator, queue/worker, audit, idempotency, admin UI, test, smoke, evidence, release và rollback.

**1\. KẾT LUẬN KHÓA**

AI Channel chỉ được xem là đủ điều kiện giao dev khi hiểu đúng 5 nguyên tắc:

**1.1. AI Channel không sở hữu Core truth**

AI Channel không tự quyết:

- giá;
- tồn kho;
- sellable status;
- chương trình 24/7;
- Giờ Vàng;
- quyền lợi thành viên;
- quyền lợi Diamond;
- QuoteSnapshot;
- order status;
- ORDER_VERIFIED;
- ROAS.

Các dữ liệu này thuộc **Ginsengfood Core Runtime**.

**1.2. AI Channel không thay thế Facebook Gateway**

AI Channel xử lý:

- DecisionEnvelope;
- resolver;
- guard;
- content render;
- public/private boundary;
- Messenger handoff decision;
- quote request;
- order confirmation draft;
- CRM eligibility.

Facebook Gateway xử lý:

- Meta webhook;
- verify signature;
- normalized event;
- dedup;
- handoff delivery;
- public reply delivery;
- Page registry;
- token/secret reference;
- Gateway health.

**1.3. MC AI không phải hệ riêng tự quyết**

MC AI chỉ được dùng:

- Daily Live Product Board;
- AI Live Script Brief;
- approved script segment;
- approved video segment.

MC AI không được:

- tự chọn SKU;
- nói SKU ngoài board;
- public final price;
- public stock;
- kêu khách tự nhắn Messenger;
- phát clip REVALIDATION_REQUIRED;
- phát clip chưa APPROVED_FOR_LIVE.

**1.4. Completion Report và Evidence Package là điều kiện chặn**

Không được gọi AI Channel là PASS nếu chưa có:

- Completion Report PASS;
- Evidence Package PASS;
- P0 test PASS;
- owner sign-off.

**1.5. Gateway production bị chặn cho đến khi AI Channel PASS**

Gateway Dev Planning có thể chuẩn bị.

Gateway production chỉ được mở khi:

- AI Advisor Facebook Completion Report PASS;
- Evidence Package PASS;
- Live-to-Order E2E smoke PASS;
- Owner sign-off PASS.

**2\. SOURCE-OF-TRUTH PRIORITY**

AI_CHANNEL_IMPLEMENTATION_SOURCE_PRIORITY:  
1:  
source: "FILE 00 - AI Advisor Source of Truth"  
wins_for:  
\- master_scope  
\- AI_Advisor_Facebook_Completion_Gate  
\- FILE12_non_override  
\- sales_to_order_root  
<br/>2:  
source: "Ginsengfood Core Runtime"  
wins_for:  
\- price  
\- inventory  
\- sellable_status  
\- program_24_7  
\- golden_hour  
\- member_benefit  
\- diamond_benefit  
\- quote_snapshot  
\- order_status  
\- ORDER_VERIFIED  
\- ROAS  
<br/>3:  
source: "Daily Live Product Board"  
wins_for:  
\- live_sku_list  
\- hero_triad  
\- supporting_sku  
\- AI_live_script_brief  
\- board_revalidation  
<br/>4:  
source: "AI Advisor FILE 01→12"  
wins_for:  
\- decision_logic  
\- resolver_contract  
\- guard_contract  
\- content_render  
\- situation_mapping  
\- test_matrix  
<br/>5:  
source: "Completion Report + Evidence Package"  
wins_for:  
\- gateway_transition_status  
\- readiness_pass_fail  
\- evidence_requirement  
<br/>6:  
source: "Facebook Gateway Technical Pack"  
wins_for:  
\- webhook  
\- token_secret_ref  
\- normalized_event  
\- handoff_delivery  
\- gateway_ops

MUST:

- Nếu AI content khác Core runtime, Core thắng.
- Nếu MC AI script khác Daily Live Product Board, board thắng.
- Nếu Gateway muốn public giá, PublicPrivateSurfaceGuard thắng.
- Nếu Completion Report chưa PASS, Gateway production bị chặn.

**3\. IMPLEMENTATION SCOPE**

AI Channel Dev Implementation gồm 10 nhóm chính:

AI_CHANNEL_IMPLEMENTATION_SCOPE:  
modules:  
\- ChannelContextRuntime  
\- FacebookAdsLiveContextBinding  
\- DecisionEnvelopeRuntime  
\- PublicPrivateSurfaceGuardLayer  
\- MessengerHandoffDecisionLayer  
\- LiveBoardContextIntegration  
\- MC_AI_ReadinessIntegration  
\- QuoteOrderRuntimeBridge  
\- CRMEligibilityRuntime  
\- EvidenceAndCompletionRuntime

Ngoài phạm vi file này:

- Full Meta App Review.
- Full Page Access Token production setup.
- Full Facebook Gateway infra.
- Full payment gateway.
- Full MISA/accounting.
- Full production/warehouse/traceability.
- Full MC AI video editing engine.
- Full Ads bidding/scaling automation.

Các phần trên chỉ tích hợp qua boundary.

**4\. MODULE MAP**

**MODULE 01 - Channel Context Runtime**

**Mục tiêu:** xác định kênh, surface, quyền outbound.

Phải có:

- ChannelContextResolver
- PublicPrivateSurfaceResolver
- ChannelPolicyResolver

Input:

channel_context_input:  
message_id: string  
conversation_id: string  
raw_channel: string  
source_event_type: string  
source_platform: optional string

Output:

channel_context_output:  
channel_code:  
\- WEB_CHAT  
\- LANDING_PAGE_CHAT  
\- MESSENGER  
\- LIVE_COMMENT  
\- POST_COMMENT  
\- LIVE_INBOX  
\- CRM_OUTBOUND  
\- ADMIN_PREVIEW  
<br/>reply_surface:  
\- PUBLIC  
\- PRIVATE  
\- INTERNAL  
<br/>outbound_allowed: boolean  
<br/>channel_policy_status:  
\- ALLOWED  
\- BLOCKED  
\- REVIEW_REQUIRED

Hard rule:

- LIVE_COMMENT và POST_COMMENT mặc định là PUBLIC.
- MESSENGER, WEB_CHAT, LANDING_PAGE_CHAT, LIVE_INBOX là PRIVATE.
- ADMIN_PREVIEW là INTERNAL và không outbound thật.

P0 test:

- Live comment không được xử lý như Messenger.
- Admin preview không gửi tin thật.
- Thiếu reply_surface thì fail.

**MODULE 02 - Facebook / Ads / Live Context Binding**

**Mục tiêu:** nhận context từ Gateway/Live/Ads và đưa vào DecisionEnvelope.

Phải có:

- FacebookContextResolver
- LiveSessionResolver
- AdsAttributionResolver
- DiamondReferralContextResolver
- MetaDuplicateEventResolver

Input:

facebook_ads_live_context_input:  
source_page_id: optional string  
commerce_hub_page_id: optional string  
facebook_page_id: optional string  
page_role: optional string  
live_session_id: optional string  
comment_id: optional string  
messenger_thread_id: optional string  
attribution_id: optional string  
campaign_id: optional string  
adset_id: optional string  
ad_id: optional string  
referral_link_id: optional string  
diamond_id: optional string  
entry_channel: optional string

Output:

facebook_ads_live_context_output:  
page_context_status:  
\- RESOLVED  
\- PARTIAL  
\- UNKNOWN  
\- BLOCKED  
<br/>live_context_status:  
\- RESOLVED  
\- PARTIAL  
\- UNKNOWN  
<br/>ads_context_status:  
\- RESOLVED  
\- PARTIAL  
\- UNKNOWN  
<br/>attribution_internal_only: true

Hard rule:

- Campaign/adset/ad/ROAS không được customer-facing.
- Attribution không được public.
- Handoff không được làm mất live_session_id / board_id / attribution_id.
- Diamond context không được tự bịa.

P0 test:

- Customer response không chứa campaign/ad/ROAS.
- Handoff giữ live_session_id.
- Ads-origin giữ attribution sang quote/order.

**MODULE 03 - DecisionEnvelope Runtime**

**Mục tiêu:** mọi lượt xử lý phải có DecisionEnvelope để audit/test/evidence.

Object:

decision_envelope:  
envelope_id: uuid  
correlation_id: string  
idempotency_key: string  
message_id: string  
conversation_id: string  
channel_code: string  
reply_surface: PUBLIC | PRIVATE | INTERNAL  
source_event_type: string  
<br/>facebook_context: object  
ads_context: object  
live_context: object  
customer_context: object  
diamond_context: object  
<br/>situation:  
primary_situation_id: string  
secondary_situation_ids: string\[\]  
risk_level: LOW | MEDIUM | HIGH | CRITICAL  
<br/>intent:  
primary_intent: string  
secondary_intents: string\[\]  
<br/>entity:  
product_mentions: string\[\]  
primary_user: optional string  
customer_need_summary: optional string  
price_signal: boolean  
order_signal: boolean  
pii_detected: boolean  
health_sensitive_signal: boolean  
<br/>resolver_trace: object\[\]  
guard_trace: object\[\]  
<br/>action:  
selected_action: string  
blocked_actions: string\[\]  
next_allowed_actions: string\[\]  
<br/>commerce:  
sales_session_id: optional string  
quote_cart_id: optional string  
quote_snapshot_id: optional string  
order_confirmation_draft_id: optional string  
customer_confirmation_status: optional string  
order_code: optional string  
<br/>final_guard_status: PASS | FAIL  
side_effects_created: string\[\]  
side_effects_blocked: string\[\]

Hard rule:

- Không có DecisionEnvelope thì không PASS evidence.
- DecisionEnvelope không public cho khách.
- DecisionEnvelope phải có selected_action và blocked_actions.

P0 test:

- Mọi message processed đều có envelope.
- Duplicate event không tạo envelope side effect trùng.
- Envelope thiếu guard trace khi block → fail.

**MODULE 04 - Public / Private Surface Guard Layer**

**Mục tiêu:** chặn leak trên Live/comment public.

Phải có:

- PublicPrivateSurfaceGuard
- PrivacyPIIGuard
- PaymentInvoiceGuard
- HealthSafetyPublicSurfaceGuard
- FinalGuard

Blocked trên PUBLIC:

public_surface_blocked_fields:  
\- final_quote_price  
\- listed_price_current  
\- quote_snapshot_id  
\- order_code  
\- raw_phone  
\- raw_address  
\- raw_email  
\- payment_instruction  
\- invoice_detail  
\- health_sensitive_detail  
\- campaign_id  
\- ad_id  
\- ROAS

Allowed trên PUBLIC:

public_surface_allowed:  
\- short_safe_ack  
\- public_safe_product_hint  
\- handoff_success_confirmation  
\- handoff_failed_safe_fallback

Hard rule:

- Public hỏi giá → không báo giá.
- Public muốn mua → không xác nhận đơn.
- Public để PII → không lặp PII.
- Public hỏi health → không tư vấn sâu.
- Public hỏi payment/invoice → không gửi thông tin thanh toán.

P0 test:

- Live public price blocked.
- PII public blocked.
- Payment public blocked.
- Health public blocked.

**MODULE 05 - Messenger Handoff Decision Layer**

**Mục tiêu:** quyết định khi nào cần handoff và kiểm câu public sau handoff.

Phải có:

- MessengerHandoffResolver
- MessengerHandoffGuard
- HandoffStatusBinding
- HandoffSafeFallbackResolver

Trigger handoff:

handoff_trigger_signals:  
\- price_signal  
\- order_signal  
\- PII_signal  
\- payment_signal  
\- invoice_signal  
\- health_sensitive_signal  
\- deep_advisory_needed  
\- SKU_board_question  
\- Diamond_referral_question

Output:

handoff_decision_output:  
handoff_required: boolean  
handoff_reason: string  
handoff_status:  
\- NOT_REQUIRED  
\- REQUIRED  
\- SENT  
\- FAILED  
\- BLOCKED  
public_reply_allowed: boolean  
public_reply_style:  
\- HANDOFF_SENT_CONFIRMATION  
\- SAFE_ACK_IF_HANDOFF_FAILED

Hard rule:

- Không kêu khách tự nhắn Messenger.
- Chỉ nói "đã gửi" nếu handoff SENT.
- Handoff fail dùng fallback.
- Handoff phải giữ board/live/attribution context.

P0 test:

- Handoff success mới nói đã gửi.
- Handoff fail không nói đã gửi.
- Duplicate comment không tạo duplicate handoff.
- Public reply không có câu "Mình inbox giúp Em".

**MODULE 06 - Live Board Context Integration**

**Mục tiêu:** AI Advisor nhận và tôn trọng Daily Live Product Board.

Phải có:

- LiveBoardContextResolver
- LiveBoardSkuEligibilityGuard
- LiveBoardContextBinding
- NonBoardSkuPolicyResolver

Required context:

live_board_context:  
board_id: string  
session_board_id: string  
live_session_code: LUNCH_SESSION | EVENING_SESSION  
live_date: date  
eligible_sku_list: string\[\]  
hero_triad:  
seasonal_hero_sku_id: string  
functional_hero_sku_id: string  
nourishing_hero_sku_id: string  
supporting_sku_list: string\[\]  
public_price_policy: string  
private_quote_policy: string  
handoff_policy: string

Hard rule:

- Nếu khách ở Live context, AI không được nói SKU ngoài board như live offer.
- SKU ngoài board có thể tư vấn general/private nếu Runtime cho phép, nhưng không được nói thuộc phiên live.
- Quote vẫn phải qua QuoteSnapshot.
- Board REVALIDATION_REQUIRED thì không dùng để live/script/quote live offer.

P0 test:

- AI không nói SKU ngoài board như live offer.
- AI không quote SKU not sellable.
- Board context preserve từ Gateway sang Messenger.

**MODULE 07 - MC AI Readiness Integration**

**Mục tiêu:** AI Channel biết MC AI script/video có đủ điều kiện live chưa.

Phải có:

- AiLiveScriptBriefBinding
- McAiSegmentStatusResolver
- VideoRevalidationStatusResolver
- McAiPlaylistReadinessGuard

Input:

mc_ai_readiness_input:  
board_id: string  
session_board_id: string  
brief_id: string  
segment_ids: string\[\]  
playlist_id: optional string

Output:

mc_ai_readiness_output:  
script_brief_status:  
\- EXPORTED  
\- MISSING  
\- INVALID  
\- REVALIDATION_REQUIRED  
<br/>segment_readiness:  
all_segments_approved: boolean  
blocked_segments: string\[\]  
revalidation_required_segments: string\[\]  
<br/>playlist_readiness:  
status:  
\- READY  
\- NOT_READY  
\- BLOCKED

Hard rule:

- Script phải lấy từ AI Live Script Brief.
- Clip SKU phải map board line.
- Clip chưa APPROVED_FOR_LIVE không được phát.
- Clip REVALIDATION_REQUIRED không được phát.
- Script/video có giá cuối/tồn kho/CTA tự inbox/claim điều trị thì fail.

P0 test:

- Script chứa SKU ngoài board → fail.
- Clip chưa approved vào playlist → fail.
- Board đổi sau video → revalidation required.

**MODULE 08 - Quote / Order Runtime Bridge**

**Mục tiêu:** AI Channel chỉ yêu cầu quote/order qua Core Runtime.

Phải có:

- SalesSessionResolver
- QuoteCartResolver
- QuoteSnapshotResolver
- QuoteLockResolver
- QuoteRevalidationResolver
- OrderConfirmationDraftResolver
- CustomerConfirmationResolver
- OrderRuntimeResolver
- OrderCodeGuard

Flow:

quote_order_flow:  
\- create_or_get_sales_session  
\- create_quote_cart  
\- request_quote_snapshot  
\- render_private_quote  
\- render_order_confirmation_draft  
\- wait_customer_confirmation  
\- create_order_from_confirmed_quote  
\- render_order_success_if_order_code

Hard rule:

- Không QuoteSnapshot → không báo giá cuối.
- Quote public → fail.
- Không CustomerConfirmation → không tạo order.
- Không order_code → không nói đơn đã ghi nhận.
- Order item lấy từ QuoteSnapshot.

P0 test:

- AI báo giá thiếu QuoteSnapshot → fail.
- Order tạo thiếu confirmation → fail.
- Order success thiếu order_code → fail.
- Duplicate order command không tạo 2 đơn.

**MODULE 09 - CRM Eligibility Runtime**

**Mục tiêu:** AI Channel chỉ gửi CRM khi đủ điều kiện.

Phải có:

- CRMEligibilityResolver
- ConsentPreferenceResolver
- OpenCaseSuppressionResolver
- MessageFatigueResolver
- CRMProductEffectivenessGuard

Hard rule:

- Opt-out không gửi CRM.
- Open quality/counterfeit/privacy/payment case không gửi CRM bán hàng.
- CRM gợi ý sản phẩm phải có Product Effectiveness.
- CRM không bịa lịch sử mua.
- CRM không bịa quyền lợi member/price.

P0 test:

- Opt-out suppressed.
- Open case suppressed.
- Message fatigue suppressed.
- CRM recommendation có Product Effectiveness.

**MODULE 10 - Evidence / Completion Runtime**

**Mục tiêu:** thu evidence tự động hoặc bán tự động cho Completion Report.

Phải có:

- EvidencePackageService
- EvidenceItemService
- CompletionGateStatusService
- EvidenceValidationService
- OwnerSignOffService

Object tối thiểu:

evidence_runtime_objects:  
\- evidence_package  
\- evidence_item  
\- artifact_ref  
\- completion_gate_status  
\- open_issue_register  
\- owner_sign_off_record

Hard rule:

- P0 gate không PASS nếu thiếu evidence.
- P0 evidence phải có artifact.
- Evidence chứa PII phải mask.
- Evidence không chứa secret/token.
- Owner sign-off phải gắn evidence package id.

P0 test:

- P0 evidence thiếu artifact → fail.
- Evidence có raw PII chưa mask → fail.
- Owner ký khi còn P0 pending → fail.

**5\. API MAP**

**5.1. Internal AI Channel APIs**

ai_channel_internal_apis:  
POST /api/internal/ai-channel/decision/envelopes:  
purpose: "Create DecisionEnvelope for incoming normalized event/message."  
idempotency_required: true  
<br/>POST /api/internal/ai-channel/context/resolve:  
purpose: "Resolve channel/facebook/live/ads/customer context."  
<br/>POST /api/internal/ai-channel/actions/select:  
purpose: "Select action after resolver/guard evaluation."  
<br/>POST /api/internal/ai-channel/render:  
purpose: "Render customer-facing/internal response after guard pass."  
<br/>POST /api/internal/ai-channel/handoff/decision:  
purpose: "Determine if Messenger handoff is required."  
<br/>POST /api/internal/ai-channel/live-board/bind:  
purpose: "Bind board/session_board context to AI decision."  
<br/>POST /api/internal/ai-channel/evidence/items:  
purpose: "Create evidence item for completion report."  
<br/>GET /api/internal/ai-channel/evidence/packages/{package_id}:  
purpose: "Get evidence package detail."

**5.2. Runtime Integration APIs AI Channel cần gọi**

runtime_integration_apis:  
POST /api/internal/ginsengfood/runtime/program-price/resolve  
POST /api/internal/ginsengfood/runtime/member-rights/resolve  
POST /api/internal/ginsengfood/runtime/golden-hour/resolve  
POST /api/internal/ginsengfood/runtime/diamond-referral/resolve  
POST /api/internal/ginsengfood/runtime/quote-snapshots  
POST /api/internal/ginsengfood/runtime/quote-snapshots/{id}/revalidate  
GET /api/admin/ginsengfood/live-product-boards/{session_board_id}/script-brief

**5.3. Gateway Integration APIs**

gateway_integration_apis:  
POST /api/webhooks/meta/facebook:  
owner: Facebook Gateway  
<br/>POST /api/internal/facebook/messenger/handoff:  
owner: Facebook Gateway  
<br/>POST /api/internal/ai-channel/gateway/normalized-event:  
owner: AI Channel  
purpose: "Receive normalized event from Gateway."

MUST:

- Gateway không được gọi Order Runtime trực tiếp.
- Gateway không được có API tạo quote/order.
- AI Channel không được public internal runtime response.

**6\. DB / LOG OBJECT MAP**

ai_channel_db_objects:  
ai_decision_envelopes:  
purpose: "Store DecisionEnvelope summary."  
<br/>ai_resolver_traces:  
purpose: "Store resolver execution trace."  
<br/>ai_guard_traces:  
purpose: "Store guard execution trace."  
<br/>ai_action_logs:  
purpose: "Store selected action and blocked actions."  
<br/>ai_render_logs:  
purpose: "Store rendered response metadata."  
<br/>ai_channel_context_logs:  
purpose: "Store channel/facebook/live/ads context binding."  
<br/>ai_live_board_context_logs:  
purpose: "Store board/session_board context used by AI."  
<br/>ai_handoff_decision_logs:  
purpose: "Store handoff decision from AI side."  
<br/>ai_evidence_packages:  
purpose: "Store evidence package root."  
<br/>ai_evidence_items:  
purpose: "Store evidence item."  
<br/>ai_artifact_refs:  
purpose: "Store evidence artifact references."  
<br/>ai_completion_gate_status:  
purpose: "Store GATE-01 to GATE-12 status."  
<br/>ai_open_issue_register:  
purpose: "Store open issues blocking completion."  
<br/>ai_owner_sign_off_records:  
purpose: "Store owner approval/sign-off."

MUST:

- Các log chứa PII phải mask hoặc store restricted.
- Secret/token không được lưu.
- DecisionEnvelope raw có thể lưu dạng internal restricted.
- Evidence package phải trace được gate/status/artifact.

**7\. WORKER / JOB MAP**

ai_channel_workers:  
EvidenceValidationWorker:  
purpose:  
\- validate required artifacts  
\- validate PII masking  
\- validate schema  
\- update gate status  
<br/>CompletionGateAggregationWorker:  
purpose:  
\- aggregate GATE-01 to GATE-12  
\- compute P0 pass/fail/pending  
\- block gateway transition if needed  
<br/>DecisionEnvelopeArchiveWorker:  
purpose:  
\- archive old envelopes  
\- preserve audit metadata  
<br/>OpenIssueNotificationWorker:  
purpose:  
\- notify owner/dev/QA when P0 issue open  
<br/>McAiReadinessCheckWorker:  
purpose:  
\- check board/script/segment/playlist readiness before live  
<br/>DuplicateIdempotencyAuditWorker:  
purpose:  
\- scan duplicate event/quote/order evidence  
<br/>LiveToOrderSmokeEvidenceWorker:  
purpose:  
\- collect E2E smoke artifacts

MUST:

- Workers idempotent.
- Worker retry không tạo duplicate evidence.
- Evidence worker không được sửa business state.

**8\. ADMIN / OPS UI REQUIREMENTS**

**8.1. AI Channel Completion Dashboard**

Màn hình cần có:

- Gate list GATE-01 → GATE-12.
- Status: PASS / FAIL / PENDING / BLOCKED.
- P0 open issues.
- Evidence count.
- Missing artifacts.
- Owner sign-off status.
- Gateway transition status.

**8.2. Evidence Package Detail**

Màn hình cần có:

- Evidence item list.
- Artifact refs.
- PII masking status.
- QA review status.
- Fail reason.
- Required fix file/module.
- Download/export internal report.

**8.3. MC AI Readiness Panel**

Màn hình cần có:

- Board ID.
- Session Board ID.
- Brief ID.
- Segment count.
- Approved segment count.
- Blocked segment count.
- Revalidation required segment count.
- Playlist readiness.
- Pre-live checklist status.

**8.4. Gateway Transition Panel**

Màn hình cần có:

- AI Completion status.
- Evidence package status.
- E2E smoke status.
- Owner sign-off.
- Allowed actions:
  - Dev Planning Allowed
  - Production Blocked
  - Production Allowed

Hard rule:

- Admin không được override PASS nếu P0 FAIL.
- Admin không được sửa evidence artifact.
- Admin không được mark PASS khi thiếu artifact.

**9\. IMPLEMENTATION PHASE PLAN**

**P0 - Preflight / Source Alignment**

Mục tiêu:

- Xác nhận tài liệu nguồn.
- Tạo feature flags.
- Xác định môi trường.
- Tạo gate list GATE-01 → GATE-12.

Done Gate:

- Source-of-truth rõ.
- Completion Report đã có.
- Evidence Package Schema đã có.
- Feature flags tạo xong.

**P1 - DecisionEnvelope + Channel Context**

Triển khai:

- DecisionEnvelope service.
- ChannelContextResolver.
- Public/private surface binding.
- Basic decision/action log.

Done Gate:

- WEB_CHAT / MESSENGER / LIVE_COMMENT / ADMIN_PREVIEW có envelope.
- Live comment là PUBLIC.
- Admin preview no outbound.

**P2 - Facebook / Ads / Live Context Binding**

Triển khai:

- FacebookContextResolver.
- LiveSessionResolver.
- AdsAttributionResolver.
- DiamondReferralContextResolver.
- Context internal-only.

Done Gate:

- Ads context không public.
- Live context preserve.
- Attribution preserve.

**P3 - Public / Private Guard + Handoff Decision**

Triển khai:

- PublicPrivateSurfaceGuard.
- PrivacyPIIGuard.
- HandoffDecision.
- Safe fallback.
- FinalGuard integration.

Done Gate:

- Live price blocked.
- PII blocked.
- Handoff success/fail đúng.
- Không còn câu tự inbox.

**P4 - Live Board / MC AI Readiness Integration**

Triển khai:

- LiveBoardContextResolver.
- ScriptBriefBinding.
- Segment readiness integration.
- Playlist readiness guard.

Done Gate:

- Script chỉ board.
- Segment approved.
- Revalidation pass.
- MC AI readiness panel có dữ liệu.

**P5 - Quote / Order Runtime Bridge**

Triển khai:

- SalesSession binding.
- QuoteCart / QuoteSnapshot request.
- OrderConfirmationDraft.
- CustomerConfirmation.
- OrderCodeGuard.

Done Gate:

- QuoteSnapshot required.
- CustomerConfirmation required.
- order_code required.
- Duplicate order blocked.

**P6 - CRM / Entry / Growth / Combo Guard**

Triển khai:

- EntryOrderGuard.
- GrowthOrderGuard.
- ComboNoForceGuard.
- CRMEligibilityGuard.
- ProductEffectiveness requirement.

Done Gate:

- Không ép combo.
- Entry 1 hộp hợp lệ.
- CRM suppression pass.
- Combo có Product Effectiveness.

**P7 - Evidence Runtime + Completion Dashboard**

Triển khai:

- EvidencePackageService.
- EvidenceItemService.
- EvidenceValidationWorker.
- Completion dashboard.
- Owner sign-off record.

Done Gate:

- P0 evidence missing bị fail.
- Gate status aggregate được.
- Owner sign-off bị chặn nếu P0 pending/fail.

**P8 - E2E Smoke / Gateway Transition Readiness**

Triển khai:

- Live-to-Order smoke evidence.
- Completion report aggregation.
- Gateway transition decision.

Done Gate:

- E2E smoke PASS.
- Completion Report PASS.
- Evidence Package PASS.
- Owner Sign-off PASS.
- Gateway production allowed flag có thể bật.

**10\. FEATURE FLAGS**

ai_channel_feature_flags:  
ai_channel_decision_envelope_enabled:  
default: false  
<br/>ai_channel_facebook_context_enabled:  
default: false  
<br/>ai_channel_public_private_guard_enabled:  
default: false  
<br/>ai_channel_handoff_decision_enabled:  
default: false  
<br/>ai_channel_live_board_context_enabled:  
default: false  
<br/>ai_channel_mc_ai_readiness_enabled:  
default: false  
<br/>ai_channel_quote_snapshot_bridge_enabled:  
default: false  
<br/>ai_channel_order_creation_bridge_enabled:  
default: false  
<br/>ai_channel_crm_eligibility_enabled:  
default: false  
<br/>ai_channel_evidence_runtime_enabled:  
default: false  
<br/>ai_channel_gateway_transition_enabled:  
default: false

MUST:

- Bật theo phase.
- Production không bật toàn bộ cùng lúc.
- Mỗi flag bật/tắt phải audit.
- Có rollback flag.

**11\. TEST MATRIX P0**

| **Test ID**  | **Nội dung**                  | **Expected**                                |
| ------------ | ----------------------------- | ------------------------------------------- |
| AI-CH-P0-001 | Live comment hỏi giá          | Không public giá, tạo handoff               |
| AI-CH-P0-002 | Handoff fail                  | Không nói "đã gửi"                          |
| AI-CH-P0-003 | Public có số điện thoại       | Không lặp PII                               |
| AI-CH-P0-004 | Ads context                   | Không public campaign/ad/ROAS               |
| AI-CH-P0-005 | Live board context            | AI không nói SKU ngoài board như live offer |
| AI-CH-P0-006 | MC AI segment chưa approved   | Không được vào playlist                     |
| AI-CH-P0-007 | Board đổi sau video           | Segment/board REVALIDATION_REQUIRED         |
| AI-CH-P0-008 | Báo giá private               | Bắt buộc có QuoteSnapshot                   |
| AI-CH-P0-009 | Tạo order                     | Bắt buộc CustomerConfirmation               |
| AI-CH-P0-010 | Order success                 | Bắt buộc order_code                         |
| AI-CH-P0-011 | Combo proposal                | Không ép combo, có Product Effectiveness    |
| AI-CH-P0-012 | CRM opt-out                   | Suppressed                                  |
| AI-CH-P0-013 | Duplicate webhook             | Không duplicate reply/handoff/order         |
| AI-CH-P0-014 | Evidence thiếu artifact       | Gate P0 không PASS                          |
| AI-CH-P0-015 | Owner sign-off khi P0 pending | Bị chặn                                     |

**12\. E2E SMOKE PACK**

**Smoke 01 - Live Comment → Handoff → Messenger Quote**

Flow:

flow:  
\- live_comment_asks_price  
\- gateway_normalized_event  
\- AI_channel_receives_event  
\- DecisionEnvelope_created  
\- PublicPrivateSurfaceGuard_blocks_price  
\- HandoffDecision_required  
\- Gateway_handoff_SENT  
\- public_reply_sent_confirmation  
\- Messenger_private_continuation  
\- QuoteSnapshot_created  
\- private_quote_rendered

Expected:

- Không public giá.
- Không kêu khách inbox.
- Handoff success mới nói đã gửi.
- QuoteSnapshot tồn tại.

**Smoke 02 - MC AI Board → Script → Segment → Live Readiness**

Flow:

flow:  
\- daily_live_board_APPROVED  
\- AI_live_script_brief_EXPORTED  
\- script_segments_generated  
\- segment_validation_run  
\- video_segments_RENDERED  
\- ops_review_PASS  
\- segments_APPROVED_FOR_LIVE  
\- pre_live_revalidation_PASS

Expected:

- Không SKU ngoài board.
- Không giá cuối.
- Không tồn kho.
- Không CTA tự inbox.
- Không segment REVALIDATION_REQUIRED.

**Smoke 03 - Messenger Quote → Order**

Flow:

flow:  
\- customer_selects_product  
\- sales_session_created  
\- quote_cart_created  
\- quote_snapshot_created  
\- order_confirmation_draft_rendered  
\- customer_confirms  
\- order_created_from_confirmed_quote  
\- order_code_returned

Expected:

- Order chỉ tạo sau xác nhận.
- Order success chỉ khi có order_code.
- Duplicate command không tạo 2 đơn.

**Smoke 04 - ORDER_VERIFIED → ROAS / Diamond / CRM**

Flow:

flow:  
\- order_created  
\- order_verified  
\- attribution_mapping_created  
\- diamond_commission_if_eligible  
\- CRMEligibility_checked  
\- ROAS_projection_updated

Expected:

- ROAS chỉ từ ORDER_VERIFIED.
- Diamond commission chỉ sau ORDER_VERIFIED.
- CRM suppression đúng.

**13\. EVIDENCE REQUIREMENT**

Mỗi P0 test phải có:

p0_evidence_required:  
\- DecisionEnvelope_if_AI_related  
\- resolver_trace_if_resolver_related  
\- guard_trace_if_guard_related  
\- normalized_event_if_gateway_related  
\- handoff_delivery_log_if_handoff_related  
\- board_context_if_live_related  
\- segment_status_if_MC_AI_related  
\- quote_snapshot_if_price_related  
\- customer_confirmation_if_order_related  
\- order_code_if_order_success_related  
\- CRM_suppression_trace_if_CRM_related  
\- ROAS_mapping_if_measurement_related  
\- idempotency_key_if_state_change_related

MUST:

- Không dùng screenshot response thay runtime evidence.
- Evidence phải mask PII.
- Evidence không chứa secret/token.
- P0 thiếu evidence thì không PASS.

**14\. RELEASE / GATEWAY TRANSITION RULE**

gateway_transition_rule:  
dev_planning_allowed_if:  
\- AI_CHANNEL_DEV_IMPLEMENTATION_BRIEF_APPROVED  
\- completion_report_created  
\- evidence_schema_created  
<br/>staging_integration_allowed_if:  
\- P0_to_P5_done  
\- public_private_guard_PASS  
\- handoff_decision_PASS  
\- quote_snapshot_bridge_PASS  
<br/>production_allowed_if:  
\- all_P0_tests_PASS  
\- evidence_package_PASS  
\- e2e_smoke_PASS  
\- owner_signoff_PASS  
\- rollback_plan_READY  
\- monitoring_READY

FAIL nếu:

- Live public báo giá.
- Handoff fail nói đã gửi.
- MC AI nói SKU ngoài board.
- AI quote thiếu QuoteSnapshot.
- Order thiếu CustomerConfirmation.
- ROAS tính từ order chưa verified.
- Evidence thiếu P0 artifact.
- Owner chưa ký.

**15\. ROLLBACK PLAN**

ai_channel_rollback_plan:  
public_private_guard_issue:  
action:  
\- disable live_public_auto_reply  
\- keep safe fallback only  
<br/>handoff_issue:  
action:  
\- disable system_initiated_handoff  
\- use safe public acknowledgement  
\- block quote/order from live context  
<br/>quote_snapshot_issue:  
action:  
\- disable ai_runtime_quote_enabled  
\- block price rendering  
\- keep advisory only  
<br/>mc_ai_segment_issue:  
action:  
\- block affected segments  
\- remove from playlist  
\- require revalidation  
<br/>crm_issue:  
action:  
\- disable CRM outbound  
\- keep manual review queue  
<br/>evidence_runtime_issue:  
action:  
\- block gateway transition  
\- require manual QA evidence package

MUST NOT:

- Không xóa evidence cũ.
- Không rewrite order/quote history.
- Không sửa log để làm PASS giả.
- Không phát live nếu rollback đang mở.

**16\. DEV OUTPUT CHECKLIST**

Dev phải bàn giao:

- DecisionEnvelope runtime.
- ChannelContextResolver.
- Facebook/Ads/Live context binding.
- PublicPrivateSurfaceGuard.
- PrivacyPIIGuard.
- MessengerHandoffDecision.
- LiveBoardContextResolver.
- MC AI readiness integration.
- QuoteSnapshot bridge.
- Order confirmation/order_code guard.
- CRMEligibilityGuard.
- Duplicate/idempotency guard.
- Evidence Package runtime.
- Completion dashboard.
- P0 test suite.
- E2E smoke evidence.
- Feature flags.
- Rollback plan.
- Owner sign-off workflow.

**17\. MASTER DONE GATE**

Tài liệu này đạt chuẩn khi:

- Có implementation scope rõ.
- Có module map rõ.
- Có resolver/guard map rõ.
- Có API map.
- Có DB/log object map.
- Có worker/job map.
- Có admin/ops UI requirements.
- Có phase plan P0-P8.
- Có feature flags.
- Có P0 test matrix.
- Có E2E smoke pack.
- Có evidence requirement.
- Có Gateway transition rule.
- Có rollback plan.
- Có dev output checklist.

**18\. MASTER FAIL GATE**

Implementation Brief FAIL nếu:

- Dev vẫn phải tự đoán module.
- Dev vẫn phải tự đoán resolver/guard.
- Dev vẫn phải tự đoán API.
- Dev vẫn phải tự đoán evidence.
- Không có DecisionEnvelope.
- Không có public/private guard.
- Không có handoff success/fail rule.
- Không có Live Board context.
- Không có MC AI readiness.
- Không có QuoteSnapshot bridge.
- Không có CustomerConfirmation gate.
- Không có CRM suppression.
- Không có duplicate/idempotency.
- Không có Completion Evidence.
- Không có rollback.

**19\. KẾT LUẬN KHÓA**

**AI_CHANNEL_DEV_IMPLEMENTATION_BRIEF_V2.4** là tài liệu chuyển phần AI Tích hợp Channel từ mức tài liệu logic sang mức dev task breakdown.

Từ sau tài liệu này:

- Dev không được triển khai AI Channel bằng prompt rời rạc.
- Dev không được để AI tự tính giá.
- Dev không được để Gateway tự quote/order.
- Dev không được để MC AI nói SKU ngoài board.
- Dev không được public giá trên Live/comment.
- Dev không được bỏ DecisionEnvelope.
- Dev không được bỏ guard/evidence.
- Dev không được gọi Gateway production khi Completion Report chưa PASS.

Trạng thái sau tài liệu này:

AI_CHANNEL_DEV_BRIEF_STATUS:  
documentation_status: READY_FOR_DEV_TASK_BREAKDOWN  
gateway_dev_planning: ALLOWED_AFTER_OWNER_REVIEW  
gateway_production: BLOCKED_UNTIL_COMPLETION_REPORT_PASS  
gateway_pass_claim: NOT_ALLOWED_WITHOUT_EVIDENCE_ACCEPTED
production_ready_claim: NOT_ALLOWED
status_note: READY_FOR_DEV_TASK_BREAKDOWN chỉ cho phép bóc task kỹ thuật có kiểm soát; không phải Gateway PASS, Release Ready hoặc Production Ready.
next_document: "MC_AI_VIDEO_SEGMENT_REGISTRY_CONTRACT"

**HẾT TÀI LIỆU - AI_CHANNEL_DEV_IMPLEMENTATION_BRIEF_V2.4**
