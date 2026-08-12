# PACK-08 - MC AI LIVE

## AI TÍCH HỢP CHANNEL / FACEBOOK–ADS–LIVE / MC AI READINESS COMPLETION REPORT

Hệ thống: AI Advisor Ginsengfood — Web Chat / Landing Page Chat / Messenger / Facebook Live Comment / Live Inbox / Ads-origin Conversation / CRM OutboundLiên kết: Ginsengfood Core Runtime / Daily Live Product Board / MC AI Live / Facebook Channel Gateway / QuoteSnapshot / Order Runtime / CRM / ROASVai trò tài liệu: Completion Report / Evidence Checklist / Gateway Transition Gate / Owner Sign-off RecordTrạng thái mặc định: waiting_EVIDENCE — NOT GATEWAY PASSMục tiêu: Chứng minh AI Advisor đã đủ điều kiện nhận event từ Facebook / Ads / Live / MC AI / Messenger trước khi triển khai Facebook Channel Gateway production.

## 0. MỤC ĐÍCH TÀI LIỆU

Tài liệu này dùng để nghiệm thu mức độ hoàn tất của AI Advisor Facebook / Ads / Live Context trước khi chuyển sang triển khai hoặc bật production cho Facebook Channel Gateway / Meta Live & Messenger.

Tài liệu này không viết lại nghiệp vụ.

Tài liệu này không thay thế:

FILE 00 -> FILE 12 của AI Advisor.

Chương II — Lõi Ginsengfood.

Daily Live Product Board.

Facebook Channel Gateway Technical Dev Pack.

MC AI Live Video Production Standard.

Tài liệu này chỉ trả lời một câu hỏi:

AI Advisor đã đủ bằng chứng để nhận event từ Gateway và xử lý đúng chưa?

Nếu chưa có bằng chứng, trạng thái là:

waiting_EVIDENCE — NOT GATEWAY PASS

Nếu có bất kỳ P0 fail nào, trạng thái là:

## FAIL — BLOCK GATEWAY TRANSITION

## 1. KẾT LUẬN KHÓA

AI Advisor chỉ được xem là sẵn sàng cho Facebook Gateway khi các nhóm sau đều PASS:

Channel Context.

Facebook / Ads / Live Context.

Public / Private Boundary.

Messenger Handoff.

Board Context Preservation.

MC AI / AI Live Script Brief Readiness.

Ads / ROAS Internal-only Guard.

Golden Hour Runtime Guard.

QuoteSnapshot / Sales-to-Order Gate.

Entry / Growth / Combo Decision.

CRM Suppression.

Duplicate / Idempotency Guard.

FILE 12 Non-Override.

P0 Test Evidence.

Owner Sign-off.

## MUST:

Không chuyển sang Gateway production nếu Completion Report chưa PASS.

Không bật Live auto-reply production nếu public/private boundary chưa PASS.

Không bật Messenger quote/order nếu QuoteSnapshot và CustomerConfirmation chưa PASS.

Không bật MC AI Live nếu board/script/video chưa có evidence.

Không scale Ads nếu attribution / ROAS / Order Verified chưa PASS.

## MUST NOT:

Không gọi trạng thái “Ready” chỉ vì tài liệu đã viết xong.

Không gọi “PASS” nếu chưa có evidence.

Không dùng screenshot câu trả lời hay thay cho DecisionEnvelope / guard trace / runtime evidence.

Không cho Gateway tự quote/order/CRM khi AI Advisor chưa PASS.

Không cho MC AI phát clip nếu board/script/video đang Revalidation Required.

## 2. TRẠNG THÁI COMPLETION REPORT

AI_ADVISOR_FACEBOOK_COMPLETION_STATUS: allowed_status: - NOT_STARTED - waiting_EVIDENCE - PARTIAL_PASS - PASS - FAIL - bị chặn default_status: waiting_EVIDENCE gateway_transition_allowed_only_if: PASS current_status: waiting_EVIDENCE

## 2.1. Ý nghĩa trạng thái

Trạng thái

Ý nghĩa

Có được mở Gateway production không

## NOT_STARTED

Chưa kiểm

Không

waiting_EVIDENCE

Có tài liệu nhưng chưa có bằng chứng

Không

## PARTIAL_PASS

Một phần PASS, còn P0/P1 mở

Không

Đủ evidence, P0 pass, owner ký

Có thể chuyển Gateway

Có P0 fail

Không

bị chặn

Thiếu module/runtime/evidence trọng yếu

Không

## 3. SOURCE-OF-TRUTH DÙNG ĐỂ NGHIỆM THU

Completion Report phải đối chiếu theo thứ tự nguồn sau:

COMPLETION_REPORT_SOURCE_PRIORITY: 1: FILE_00_SOURCE_OF_TRUTH 2: Runtime_Core_Ginsengfood 3: Daily_Live_Product_Board 4: AI_Advisor_FILE_01_TO_FILE_12 5: ClaimGuard_FinalGuard 6: QuoteSnapshot_OrderRuntime 7: Facebook_Gateway_Normalized_Event 8: Test_Matrix_Smoke_Pack 9: Evidence_Package

## MUST:

Runtime Core thắng về giá, tồn kho, chương trình, quyền lợi, QuoteSnapshot, order.

Daily Live Product Board thắng về SKU trong Live.

AI Advisor thắng về decision/render/guard/customer-facing response.

Gateway chỉ thắng về webhook, normalized event, handoff status, delivery log.

Evidence Package thắng cảm tính “đã ổn rồi”.

## 4. COMPLETION EVIDENCE OBJECT

Mỗi hạng mục nghiệm thu phải có evidence theo cấu trúc sau:

completion_evidence_item: evidence_id: string gate_code: string gate_name: string priority: P0 | P1 | P2 owner: string status: PASS | FAIL | waiting | bị chặn | ACCEPTED_RISK tested_at: datetime tested_by: string required_artifacts: - request_payload_if_any - response_payload_if_any - DecisionEnvelope_if_AI_related - resolver_trace_if_any - guard_trace_if_any - DB_snapshot_if_side_effect - event_or_outbox_record_if_any - handoff_delivery_log_if_any - quote_snapshot_if_quote - order_code_if_order - screenshot_if_UI_or_ops - owner_note_if_manual_review fail_reason: optional string required_fix_file: optional string owner_decision_required: boolean

## MUST:

P0 evidence không được để trống.

P0 FAIL chặn Gateway Transition.

Evidence phải lưu được để QA/dev/owner truy lại.

Evidence không public cho khách.

## 5. GATE 01 — CHANNEL CONTEXT COMPLETION

## 5.1. Mục tiêu

Xác nhận AI Advisor đã nhận diện đúng kênh và bề mặt trả lời.

Các channel bắt buộc:

## WEB_CHAT

## LANDING_PAGE_CHAT

## MESSENGER

## LIVE_COMMENT

## LIVE_INBOX

## POST_COMMENT

## CRM_OUTBOUND

## ADMIN_PREVIEW

## 5.2. Pass Criteria

PASS khi:

channel_code được xác định.

reply_surface được xác định: PUBLIC / PRIVATE / INTERNAL.

Public surface không bị xử lý như private.

Admin Preview không gửi outbound thật.

DecisionEnvelope ghi rõ channel context.

## 5.3. Evidence bắt buộc

GATE_01_EVIDENCE: required: - sample_DecisionEnvelope_WEB_CHAT - sample_DecisionEnvelope_MESSENGER - sample_DecisionEnvelope_LIVE_COMMENT_PUBLIC - sample_DecisionEnvelope_LIVE_INBOX_PRIVATE - sample_DecisionEnvelope_CRM_OUTBOUND - sample_DecisionEnvelope_ADMIN_PREVIEW

## 5.4. Fail nếu

Live comment bị xử lý như Messenger private.

Admin Preview gửi tin thật.

Không có reply_surface.

Không có DecisionEnvelope.

## 6. GATE 02 — FACEBOOK / ADS / LIVE CONTEXT COMPLETION

## 6.1. Mục tiêu

Xác nhận AI Advisor nhận được context từ Facebook / Ads / Live / Diamond nếu Gateway gửi vào.

Context bắt buộc:

source_page_id

commerce_hub_page_id

facebook_page_id

page_role

live_session_id

comment_id

messenger_thread_id

attribution_id

campaign_id

adset_id

ad_id

referral_link_id

diamond_id

entry_channel

## 6.2. Pass Criteria

PASS khi:

Context được nhận vào DecisionEnvelope.

Campaign/adset/ad chỉ internal.

Attribution không public.

Diamond bind không tự bịa.

Page/Live context giữ được khi handoff sang Messenger.

## 6.3. Evidence bắt buộc

GATE_02_EVIDENCE: required: - normalized_event_sample_from_gateway - DecisionEnvelope_with_facebook_context - DecisionEnvelope_with_ads_context - DecisionEnvelope_with_live_context - DecisionEnvelope_with_diamond_context - attribution_internal_only_guard_log

## 6.4. Fail nếu

AI public campaign/adset/ad/ROAS.

Mất live_session_id sau handoff.

Mất attribution_id sau handoff.

AI tự nói khách đến từ quảng cáo nào.

Diamond benefit được nói khi resolver chưa pass.

## 7. GATE 03 — PUBLIC / PRIVATE BOUNDARY COMPLETION

## 7.1. Mục tiêu

Xác nhận Live/comment public không bị leak giá, đơn, PII, payment, invoice, health-sensitive.

## 7.2. Trigger public bắt buộc kéo private

Hỏi giá.

Muốn mua/chốt/lấy hàng.

Để số điện thoại/địa chỉ/email/MST.

Hỏi thanh toán/hóa đơn.

Hỏi bệnh nền/kiêng cữ/thai kỳ/trẻ nhỏ.

Cần tư vấn sâu.

Hỏi Diamond/referral.

Hỏi SKU trong board cần báo giá.

## 7.3. Pass Criteria

PASS khi:

Public hỏi giá -> không báo giá.

Public để PII -> không lặp PII.

Public hỏi health-sensitive -> không tư vấn dài.

Public hỏi thanh toán -> không gửi payment instruction.

Public muốn mua -> không xác nhận đơn.

Public chỉ safe ack hoặc handoff.

## 7.4. Evidence bắt buộc

GATE_03_EVIDENCE: required: - live_public_price_block_test - live_public_order_block_test - live_public_PII_block_test - live_public_payment_block_test - live_public_invoice_block_test - live_public_health_sensitive_block_test - PublicPrivateSurfaceGuard_trace - FinalGuard_trace

## 7.5. Fail nếu

Public comment có final price.

Public comment xác nhận đơn.

Public comment lặp số điện thoại/địa chỉ.

Public comment gửi tài khoản thanh toán.

Public comment tư vấn bệnh nền dài.

Public comment chứa quote_snapshot_id, campaign/ad/ROAS.

## 8. GATE 04 — SYSTEM-INITIATED MESSENGER HANDOFF COMPLETION

## 8.1. Mục tiêu

Xác nhận Gateway/AI flow dùng đúng cơ chế chủ động handoff, không kêu khách tự nhắn Messenger.

## 8.2. Pass Criteria

PASS khi:

Comment hỏi giá -> Gateway tạo handoff.

Handoff success -> public được nói đã gửi/chuyển Messenger.

Handoff fail -> public không nói đã gửi.

Public không có câu “Mình nhắn Messenger giúp Em”.

Handoff log có status.

Handoff preserve board/live/attribution context.

## 8.3. Evidence bắt buộc

GATE_04_EVIDENCE: required: - handoff_success_event - handoff_failed_event - public_reply_after_success - public_reply_after_fail - handoff_delivery_log - board_id_preserved_after_handoff - session_board_id_preserved_after_handoff - attribution_preserved_after_handoff

## 8.4. Fail nếu

Public reply kêu khách tự inbox.

Handoff fail nhưng public nói “đã gửi”.

Không có handoff delivery log.

Mất board context.

Handoff tạo duplicate message khi webhook retry.

## 9. GATE 05 — MC AI / DAILY LIVE PRODUCT BOARD READINESS

## 9.1. Mục tiêu

Xác nhận AI Advisor và MC AI chỉ dùng dữ liệu từ Daily Live Product Board và AI Live Script Brief.

## 9.2. Pass Criteria

PASS khi:

Board đã GENERATED / APPROVED / Ready For Live tùy phase.

AI Live Script Brief export được.

MC AI script chỉ dùng SKU trong board.

MC AI không nói SKU ngoài board.

MC AI không public final price.

MC AI không nói tồn kho.

MC AI không kêu khách tự inbox.

Board đổi sau video -> Revalidation Required.

Clip/segment chưa Approved For Live không được phát.

## 9.3. Evidence bắt buộc

GATE_05_EVIDENCE: required: - daily_live_product_board_id - session_board_id - ai_live_script_brief_id - script_segment_list - script_validation_result - non_board_sku_scan_result - public_price_scan_result - forbidden_CTA_scan_result - revalidation_result_before_live

## 9.4. Fail nếu

Script chứa SKU ngoài board.

Script nói giá cuối.

Script nói tồn kho.

Script có câu “Mình inbox/nhắn Messenger giúp Em”.

Board Revalidation Required nhưng vẫn phát live.

Video segment chưa approved nhưng vào playlist.

## 10. GATE 06 — ADS / ROAS INTERNAL-ONLY COMPLETION

## 10.1. Mục tiêu

Xác nhận dữ liệu Ads/ROAS chỉ dùng nội bộ, không xuất hiện trong customer-facing response.

## 10.2. Pass Criteria

PASS khi:

campaign_id, adset_id, ad_id, attribution_id, ROAS chỉ ở internal trace.

Customer response không nhắc campaign/ad/ROAS.

ROAS chỉ tính từ Order Verified.

Created order / quote / inbox không tính revenue.

## 10.3. Evidence bắt buộc

GATE_06_EVIDENCE: required: - ROASInternalOnlyGuard_trace - customer_response_no_campaign_id - customer_response_no_ad_id - customer_response_no_ROAS - order_verified_revenue_mapping_sample - quote_not_counted_as_revenue_test - created_order_not_counted_as_verified_revenue_test

## 10.4. Fail nếu

AI nói “Mình đến từ quảng cáo…”

AI/Gateway public campaign/ad.

Dashboard tính ROAS từ order chưa verified.

Quote hoặc inbox được tính doanh thu.

## 11. GATE 07 — GOLDEN HOUR COMPLETION

## 11.1. Mục tiêu

Xác nhận Giờ Vàng được xử lý đúng runtime, không bịa active, không bịa giá, không ép combo.

## 11.2. Pass Criteria

PASS khi:

GoldenHourResolver được gọi.

SKU trong live board có eligibility.

Quote Giờ Vàng giữ giá 5 phút.

Quote 24/7 giữ giá 15 phút.

Public không báo giá Giờ Vàng.

Quote hết hạn phải revalidate.

Giờ Vàng không active thì không nói đang active.

Không ép combo vì đang Giờ Vàng.

## 11.3. Evidence bắt buộc

GATE_07_EVIDENCE: required: - GoldenHourResolver_trace - QuoteSnapshot_GoldenHour_5min_sample - QuoteSnapshot_247_15min_sample - expired_quote_revalidation_test - golden_hour_not_active_safe_wording_test - combo_no_force_guard_test

## 11.4. Fail nếu

AI nói Giờ Vàng active khi resolver fail.

AI public giá Giờ Vàng.

Quote Giờ Vàng không giữ 5 phút.

Quote 24/7 không giữ 15 phút.

Quote hết hạn vẫn tạo order.

AI ép combo vì Giờ Vàng.

## 12. GATE 08 — SALES-TO-ORDER COMPLETION

## 12.1. Mục tiêu

Xác nhận AI Advisor có thể đi từ tư vấn -> quote -> order đúng gate.

## 12.2. Pass Criteria

PASS khi:

Có sales_session.

Có quote_cart nếu cần.

Có QuoteSnapshot trước khi báo giá.

Quote có line item.

Quote có hold time.

Order Confirmation Draft khác Order Success.

CustomerConfirmation bắt buộc.

Order chỉ tạo từ confirmed quote.

Order success chỉ nói khi có order_code.

Duplicate idempotency không tạo 2 đơn.

## 12.3. Evidence bắt buộc

GATE_08_EVIDENCE: required: - sales_session_sample - quote_cart_sample - quote_snapshot_sample - quote_confirmation_draft_sample - customer_confirmation_sample - order_created_from_confirmed_quote_sample - order_code_sample - duplicate_order_idempotency_test

## 12.4. Fail nếu

AI báo giá thiếu QuoteSnapshot.

Order tạo khi khách chưa xác nhận.

Order item không khớp QuoteSnapshot.

Không có order_code nhưng AI nói đơn đã ghi nhận.

Duplicate retry tạo 2 đơn.

## 13. GATE 09 — ENTRY / GROWTH / COMBO COMPLETION

## 13.1. Mục tiêu

Xác nhận AI không còn chỉ đề xuất 1 hộp một cách máy móc, nhưng cũng không ép combo.

## 13.2. Pass Criteria

PASS khi:

Entry Order được xem là hợp lệ.

Growth Order có lý do.

Combo có Product Effectiveness từng item.

Combo không ép.

Khách từ chối combo vẫn quote được 1 hộp nếu guard pass.

Combo không chứa SKU ngoài board trong Live context.

Combo không chứa SKU không sellable.

Combo không vi phạm dietary/allergy/health guard.

## 13.3. Evidence bắt buộc

GATE_09_EVIDENCE: required: - entry_order_decision_sample - growth_order_decision_sample - combo_decision_sample - combo_item_product_effectiveness_sample - combo_refusal_to_entry_fallback_sample - ComboNoForceGuard_trace - InventorySellableGuard_trace_for_combo

## 13.4. Fail nếu

AI ép combo.

AI nói mua 1 hộp là không đủ.

Combo thiếu lý do hiệu dụng từng item.

Combo chứa SKU không sellable.

Combo chứa SKU ngoài board nhưng nói như live offer.

AI không gợi ý growth/combo khi khách mua gia đình/quà/ba mẹ có đủ trust/buying signal.

## 14. GATE 10 — CRM COMPLETION

## 14.1. Mục tiêu

Xác nhận CRM sau Facebook/Ads/Live/Order không spam, không gửi sai lúc, có Product Effectiveness.

## 14.2. Pass Criteria

PASS khi:

CRMEligibilityGuard pass trước outbound.

Opt-out bị chặn.

Open case bị suppression.

Message fatigue được kiểm.

CRM gợi ý sản phẩm có Product Effectiveness.

CRM không bịa lịch sử mua.

CRM không bịa quyền lợi member.

CRM không bịa giá nếu chưa có QuoteSnapshot.

## 14.3. Evidence bắt buộc

GATE_10_EVIDENCE: required: - CRMEligibilityGuard_trace - opt_out_suppression_test - open_case_suppression_test - message_fatigue_test - CRM_ProductEffectiveness_sample - customer_memory_no_fake_history_test

## 14.4. Fail nếu

CRM gửi khi khách opt-out.

CRM gửi khi có complaint/counterfeit/privacy case.

CRM chỉ nói “sản phẩm phù hợp” mà không có hiệu dụng.

CRM bịa lịch sử mua.

CRM bịa quyền lợi thành viên.

## 15. GATE 11 — DUPLICATE / IDEMPOTENCY COMPLETION

## 15.1. Mục tiêu

Xác nhận webhook retry, comment trùng, message trùng không tạo trùng reply/quote/order.

## 15.2. Pass Criteria

PASS khi:

Duplicate comment không tạo reply trùng.

Duplicate webhook không tạo handoff trùng.

Duplicate buying signal không tạo quote/order trùng.

Duplicate order command bị idempotency chặn.

Duplicate commission không tạo ledger trùng.

## 15.3. Evidence bắt buộc

GATE_11_EVIDENCE: required: - duplicate_comment_event_test - duplicate_webhook_test - duplicate_handoff_test - duplicate_quote_test - duplicate_order_test - idempotency_registry_sample

## 15.4. Fail nếu

Một comment tạo nhiều public reply.

Một buying signal tạo nhiều order.

Webhook retry tạo nhiều handoff.

Idempotency key không được dùng.

## 16. GATE 12 — FILE 12 NON-OVERRIDE COMPLETION

## 16.1. Mục tiêu

Xác nhận FILE 12 Human-Level Sales chỉ nâng hành vi bán hàng, không override claim/product/runtime/guard.

## 16.2. Pass Criteria

PASS khi:

FILE 12 không tạo claim mới.

FILE 12 không tạo hiệu dụng mới.

FILE 12 không bỏ ClaimGuard.

FILE 12 không bỏ FinalGuard.

FILE 12 không bỏ QuoteSnapshot.

FILE 12 không ép combo.

FILE 12 không public campaign/ad/ROAS.

FILE 12 optimization phải qua owner approval nếu active.

## 16.3. Evidence bắt buộc

GATE_12_EVIDENCE: required: - File12NonOverrideGuard_test - no_new_claim_test - no_new_product_effectiveness_test - no_runtime_bypass_test - no_forced_combo_test - regression_report_for_FILE12

## 16.4. Fail nếu

FILE 12 tạo claim/hiệu dụng mới.

FILE 12 cho phép vượt guard.

FILE 12 ép combo để tăng conversion.

FILE 12 cho phép báo giá không QuoteSnapshot.

## 17. MASTER COMPLETION TABLE

Gate

Tên Gate

Priority

Status

Evidence ID

Owner

## GATE-01

Channel Context

## P0

waiting

AI Lead / Dev

## GATE-02

Facebook / Ads / Live Context

## P0

waiting

AI Lead / Gateway Lead

## GATE-03

Public / Private Boundary

## P0

waiting

AI Lead / QA

## GATE-04

Messenger Handoff

## P0

waiting

Gateway Lead / QA

## GATE-05

MC AI / Live Board Readiness

## P0

waiting

Ops / AI Lead

## GATE-06

Ads / ROAS Internal-only

## P0

waiting

Ads / Data

## GATE-07

Golden Hour Runtime

## P0

waiting

Core / AI

## GATE-08

Sales-to-Order

## P0

waiting

Core / AI

## GATE-09

Entry / Growth / Combo

## P0

waiting

AI / Sales Owner

## GATE-10

## CRM

## P0

waiting

## CRM / AI

## GATE-11

Duplicate / Idempotency

## P0

waiting

Dev / QA

## GATE-12

FILE 12 Non-Override

## P0

waiting

Owner / AI Lead

## MUST:

Tất cả P0 phải PASS trước Gateway transition.

waiting không được hiểu là PASS.

ACCEPTED_RISK không áp dụng cho P0 nếu chưa có owner sign-off đặc biệt.

## 18. OPEN ISSUE REGISTER

open_issue_register: issue_id: string detected_gate: string affected_files: - FILE_00 - FILE_04 - FILE_05 - FILE_07 - FILE_09 - FILE_10 - FILE_12 - CORE - GATEWAY - MC_AI priority: P0 | P1 | P2 issue_type: - missing_evidence - source_conflict - runtime_gap - guard_gap - content_gap - gateway_gap - MC_AI_gap - test_gap - implementation_gap description: string required_fix_file: string owner: string status: - OPEN - IN_PROGRESS - FIXED - CLOSED - ACCEPTED_RISK

P0 issue examples:

Live public vẫn báo giá.

Handoff fail vẫn nói đã gửi.

MC AI script có SKU ngoài board.

Quote thiếu QuoteSnapshot.

Order thiếu CustomerConfirmation.

CRM gửi khi khách opt-out.

Campaign/ad/ROAS leak ra customer-facing.

Duplicate webhook tạo duplicate order.

## 19. GATEWAY TRANSITION DECISION

gateway_transition_decision: current_status: bị chặn allowed_to_start_gateway_dev: condition: - AI_ADVISOR_FACEBOOK_COMPLETION_REPORT reviewed - P0 evidence plan accepted - owner approves dev planning allowed_to_start_gateway_production: condition: - all_P0_gates_PASS - evidence_package_COMPLETE - E2E_live_to_order_smoke_PASS - owner_sign_off_PASS blocked_if: - any_P0_gate_waiting - any_P0_gate_FAIL - missing_evidence - missing_owner_signoff

Kết luận mặc định hiện tại:

Gateway Dev Planning có thể chuẩn bị sau khi owner review.Gateway Production chưa được mở cho đến khi Completion Report PASS.

## 20. E2E COMPLETION SMOKE BẮT BUỘC

## 20.1. Smoke 01 — Live comment hỏi giá

Flow:

Live Comment hỏi giá-> Gateway nhận webhook-> Dedup-> Handoff Messenger-> Public reply nếu handoff success-> AI Advisor tiếp tục trong Messenger-> QuoteSnapshot-> Quote private

Expected:

Không public giá.

Không kêu khách tự inbox.

Handoff success mới nói đã gửi.

Quote từ QuoteSnapshot.

Attribution giữ được.

## 20.2. Smoke 02 — Live comment có số điện thoại

Flow:

Khách để số điện thoại public-> PII detected-> Không lặp PII-> Handoff/private nếu policy cho phép-> Safe fallback nếu fail

Expected:

Không lặp số điện thoại.

Không xác nhận đơn public.

Không xin thêm PII public.

## 20.3. Smoke 03 — MC AI Live Board

Flow:

Daily Board approved-> AI Live Script Brief exported-> Script generated-> Script validation-> Video segment approved-> Revalidate before live

Expected:

Script không SKU ngoài board.

Không giá cuối.

Không tồn kho.

Không CTA tự inbox.

Không claim điều trị.

Không segment Revalidation Required.

## 20.4. Smoke 04 — Messenger Quote -> Order

Flow:

Messenger private-> Product selected-> QuoteCart-> QuoteSnapshot-> Order Confirmation Draft-> Customer Confirmation-> Order Runtime-> order_code

Expected:

Quote có hold time.

Order chỉ sau confirmation.

Order success chỉ khi có order_code.

Duplicate không tạo 2 đơn.

## 20.5. Smoke 05 — Order Verified -> ROAS / Diamond / CRM

Flow:

Order created-> Order Verified-> Attribution retained-> Diamond commission if eligible-> CRM eligible check-> ROAS dashboard uses verified revenue

Expected:

ROAS chỉ dùng Order Verified.

COD fail/cancel/return không tính revenue.

Diamond commission chỉ sau Order Verified.

CRM không gửi nếu suppression fail.

## 21. MASTER DONE GATE

Completion Report đạt PASS khi:

GATE-01 đến GATE-12 đều PASS.

Không còn P0 open issue.

Có evidence cho từng P0.

Có DecisionEnvelope mẫu.

Có resolver trace.

Có guard trace.

Có handoff delivery log.

Có QuoteSnapshot sample.

Có order_code sample.

Có duplicate/idempotency test.

Có MC AI board/script/video readiness evidence.

Có CRM suppression evidence.

Có ROAS internal-only evidence.

Có E2E smoke PASS.

Có owner sign-off.

## 22. MASTER FAIL GATE

Completion Report FAIL nếu:

Live/comment public báo final price.

Live/comment public xác nhận đơn.

Live/comment public lặp PII.

Live/comment public có payment instruction.

Health-sensitive public tư vấn dài.

Gateway kêu khách tự nhắn Messenger.

Handoff fail nhưng nói đã gửi.

MC AI nói SKU ngoài board.

MC AI script chứa giá cuối.

Board Revalidation Required nhưng vẫn phát.

AI báo giá thiếu QuoteSnapshot.

Order tạo thiếu CustomerConfirmation.

Order success thiếu order_code.

ROAS tính từ order chưa verified.

Diamond commission tính trước Order Verified.

CRM gửi khi opt-out/open case.

Duplicate webhook tạo duplicate reply/quote/order.

FILE 12 tạo claim/hiệu dụng mới.

Thiếu P0 evidence.

Thiếu owner sign-off.

## 23. OWNER SIGN-OFF

owner_sign_off: document_name: AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2_4 final_status: - waiting_EVIDENCE - PASS - FAIL - bị chặn owner_review: reviewed_by: "" reviewed_at: "" decision: - APPROVE_GATEWAY_TRANSITION - APPROVE_DEV_PLANNING_ONLY - REQUEST_FIX - BLOCK required_notes: "" sign_off_required_from: - Owner - Project Lead - AI Lead - Core Runtime Lead - Gateway Lead - QA Lead - Ops / MC AI Lead

## 24. KẾT LUẬN KHÓA

Tài liệu AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2.4 là điều kiện bắt buộc trước khi chuyển sang Facebook Channel Gateway production.

Từ sau tài liệu này:

Không được nói “AI Channel đã OK” nếu chưa có evidence.

Không được mở Gateway production nếu Completion Report chưa PASS.

Không được bật Live auto-reply nếu public/private boundary chưa PASS.

Không được bật MC AI Live nếu board/script/video chưa pass revalidation.

Không được bật quote/order nếu QuoteSnapshot / CustomerConfirmation / order_code chưa PASS.

Không được scale Ads nếu attribution / Order Verified / ROAS chưa PASS.

Trạng thái hiện tại mặc định:waiting_EVIDENCE — NOT GATEWAY PASS

Bước tiếp theo sau tài liệu này:Viết MC_AI_VIDEO_SEGMENT_REGISTRY_CONTRACT để khóa quản lý clip, trạng thái video, version, board mapping và revalidation trước live.

## HẾT TÀI LIỆU — AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2.4

## AI_CHANNEL_FACEBOOK_EVIDENCE_PACKAGE_SCHEMA

## AI TÍCH HỢP CHANNEL / FACEBOOK–ADS–LIVE / MC AI / GATEWAY READINESS EVIDENCE CONTRACT

## 0. MỤC ĐÍCH TÀI LIỆU

Tài liệu này dùng để định nghĩa cấu trúc Evidence Package bắt buộc cho toàn bộ phần:

AI Advisor Facebook / Ads / Live Completion.

MC AI Live / Daily Live Product Board readiness.

System-Initiated Messenger Handoff.

Public / Private Surface Guard.

QuoteSnapshot / Order Runtime.

CRM Suppression.

Attribution / ROAS Internal-only.

Duplicate / Idempotency Guard.

FILE 12 Non-Override.

Gateway Transition Readiness.

Tài liệu này không viết lại nghiệp vụ.

Tài liệu này không thay thế:

FILE 00 -> FILE 12 của AI Advisor.

Chương II — Lõi Ginsengfood.

Facebook Channel Gateway Technical Dev Pack.

MC AI Live Video Production Standard.

Test Matrix & Smoke Pack.

Tài liệu này chỉ khóa:

Bằng chứng nào phải có.

Bằng chứng lưu theo schema nào.

Bằng chứng nào được xem là hợp lệ.

Bằng chứng nào không được dùng thay thế.

Khi nào evidence PASS / FAIL / waiting / bị chặn.

Evidence nào chặn Gateway Transition.

## 1. KẾT LUẬN KHÓA

Từ tài liệu này, mọi hạng mục trong AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2.4 chỉ được đánh dấu PASS khi có evidence hợp lệ.

## MUST:

Mỗi Gate P0 phải có evidence item.

Mỗi evidence item phải có evidence_id.

Mỗi evidence item phải map về gate_code.

Evidence phải có trạng thái rõ: PASS / FAIL / waiting / bị chặn.

Evidence liên quan AI phải có DecisionEnvelope hoặc equivalent runtime trace.

Evidence liên quan resolver/guard phải có resolver trace / guard trace.

Evidence liên quan Gateway phải có normalized event và delivery log.

Evidence liên quan handoff phải có handoff status.

Evidence liên quan MC AI phải có board/script/segment/revalidation record.

Evidence liên quan quote phải có QuoteSnapshot.

Evidence liên quan order phải có CustomerConfirmation và order_code.

Evidence liên quan ROAS phải có Order Verified mapping.

Evidence liên quan CRM phải có suppression / eligibility trace.

Evidence phải che/mask PII trước khi lưu hoặc gửi review.

## MUST NOT:

Không dùng ảnh chụp câu trả lời đẹp để thay DecisionEnvelope.

Không dùng log miệng để thay DB/runtime evidence.

Không dùng screenshot UI để thay API/DB/worker evidence.

Không public raw phone/address/email/tax code trong evidence.

Không lưu token thật, page token, secret, access token trong evidence.

Không đánh PASS nếu evidence thiếu required artifact.

Không cho Gateway production nếu P0 evidence còn waiting hoặc FAIL.

## 2. VAI TRÒ CỦA EVIDENCE PACKAGE

Evidence Package là hồ sơ nghiệm thu kỹ thuật.

Nó trả lời:

AI đã nhận đúng channel context chưa?

AI đã phân biệt public/private chưa?

Gateway handoff có đúng success/fail chưa?

Handoff có giữ board/live/attribution context không?

MC AI script có dùng đúng board không?

MC AI video segment có approved/revalidated không?

AI có báo giá bằng QuoteSnapshot không?

Order có CustomerConfirmation và order_code không?

ROAS có tính bằng Order Verified không?

CRM có suppression không?

Duplicate webhook có bị chặn không?

FILE 12 có override claim/runtime không?

Owner có bằng chứng để ký PASS không?

Evidence Package là điều kiện để Completion Report chuyển từ:

waiting_EVIDENCE

sang:

## 3. ROOT OBJECT — EVIDENCE PACKAGE

evidence_package: package_id: string package_code: string package_name: string package_version: "V2.4" related_completion_report: report_code: "AI_ADVISOR_FACEBOOK_COMPLETION_REPORT_V2_4" report_version: "V2.4" package_scope: - AI_CHANNEL - FACEBOOK_LIVE - ADS_CONTEXT - MESSENGER_HANDOFF - MC_AI_LIVE - DAILY_LIVE_PRODUCT_BOARD - QUOTE_SNAPSHOT - ORDER_RUNTIME - CRM - ROAS - GATEWAY_TRANSITION environment: - LOCAL - STAGING - PILOT - PRODUCTION status: - DRAFT - COLLECTING_EVIDENCE - READY_FOR_QA_REVIEW - QA_REVIEWING - PASS - FAIL - bị chặn - RETIRED created_by: string created_at: datetime updated_at: datetime owner_review_required: true qa_review_required: true gateway_transition_allowed: false summary: total_gates: number total_p0: number p0_pass: number p0_fail: number p0_waiting: number p0_blocked: number open_issue_count: number evidence_items: - evidence_item

## MUST:

package_id là duy nhất.

package_version phải khớp version tài liệu.

environment phải rõ.

Production không được dùng evidence từ local/dev làm PASS cuối.

gateway_transition_allowed = true chỉ khi tất cả P0 PASS và owner sign-off PASS.

## 4. EVIDENCE ITEM BASE SCHEMA

evidence_item: evidence_id: string evidence_code: string gate_code: string gate_name: string priority: - P0 - P1 - P2 evidence_type: - DECISION_ENVELOPE - RESOLVER_TRACE - GUARD_TRACE - FINAL_GUARD_TRACE - NORMALIZED_EVENT - HANDOFF_DELIVERY_LOG - BOARD_CONTEXT - SCRIPT_BRIEF - MC_AI_SEGMENT - VIDEO_REVALIDATION - QUOTE_SNAPSHOT - ORDER_RUNTIME - CRM_SUPPRESSION - ROAS_MAPPING - DUPLICATE_IDEMPOTENCY - ADMIN_UI_SCREEN - API_REQUEST_RESPONSE - DB_SNAPSHOT - WORKER_RUN - SCREENSHOT - OWNER_APPROVAL - QA_NOTE status: - PASS - FAIL - waiting - bị chặn - ACCEPTED_RISK tested_at: datetime tested_by: string reviewed_by: optional string reviewed_at: optional datetime source_environment: - LOCAL - STAGING - PILOT - PRODUCTION related_ids: correlation_id: optional string idempotency_key: optional string message_id: optional string conversation_id: optional string board_id: optional string session_board_id: optional string brief_id: optional string segment_id: optional string handoff_id: optional string quote_snapshot_id: optional string order_code: optional string sales_session_id: optional string attribution_id: optional string artifact_refs: - artifact_ref pii_safety: pii_present: boolean pii_masked: boolean raw_pii_removed: boolean reviewer_can_access_raw_pii: false validation_result: required_artifacts_present: boolean schema_valid: boolean guard_passed: boolean no_forbidden_leak: boolean fail_reason: optional string required_fix_file: optional string required_fix_module: optional string owner_decision_required: boolean

## MUST:

P0 evidence item không được thiếu artifact_refs.

Nếu pii_present = true, thì pii_masked = true và raw_pii_removed = true.

correlation_id bắt buộc nếu evidence liên quan flow runtime.

idempotency_key bắt buộc nếu evidence liên quan command/state change.

Evidence FAIL phải có fail_reason.

## 5. ARTIFACT REFERENCE SCHEMA

artifact_ref: artifact_id: string artifact_type: - JSON_PAYLOAD - API_REQUEST - API_RESPONSE - DB_RECORD - LOG_RECORD - DECISION_ENVELOPE - RESOLVER_TRACE - GUARD_TRACE - SCREENSHOT - VIDEO_SEGMENT - SCRIPT_FILE - BOARD_EXPORT - CSV_EXPORT - PDF_EXPORT - ADMIN_SCREEN - TEST_REPORT artifact_name: string artifact_location: string storage_policy: - INTERNAL_ONLY - QA_REVIEW_ONLY - OWNER_REVIEW - PRODUCTION_AUDIT privacy_class: - PUBLIC_SAFE - INTERNAL - SENSITIVE - SECRET_REF_ONLY hash: optional string created_at: datetime created_by: string

## MUST:

Không lưu token thật trong artifact.

Secret chỉ được lưu bằng SECRET_REF_ONLY.

Evidence có raw PII phải lưu trong khu vực kiểm soát, không đưa vào bản review rộng.

File video segment có thể lưu path/ref, không cần embed toàn bộ video vào báo cáo.

## 6. REQUIRED GATE EVIDENCE MAP

## 6.1. GATE-01 — Channel Context Evidence

GATE_01_CHANNEL_CONTEXT_EVIDENCE: required_evidence_types: - DECISION_ENVELOPE - API_REQUEST_RESPONSE - GUARD_TRACE required_samples: - WEB_CHAT_PRIVATE - LANDING_PAGE_CHAT_PRIVATE - MESSENGER_PRIVATE - LIVE_COMMENT_PUBLIC - POST_COMMENT_PUBLIC - LIVE_INBOX_PRIVATE - CRM_OUTBOUND_PRIVATE - ADMIN_PREVIEW_INTERNAL required_fields: - channel_code - reply_surface - outbound_allowed - channel_policy_status - selected_action - blocked_actions

PASS khi:

Tất cả channel mẫu có DecisionEnvelope.

Live comment có reply_surface = PUBLIC.

Messenger/Live Inbox có reply_surface = PRIVATE.

Admin Preview có reply_surface = INTERNAL.

FAIL nếu:

Live comment bị gắn PRIVATE.

Admin Preview có outbound thật.

Thiếu reply_surface.

## 6.2. GATE-02 — Facebook / Ads / Live Context Evidence

GATE_02_FACEBOOK_ADS_LIVE_CONTEXT_EVIDENCE: required_evidence_types: - NORMALIZED_EVENT - DECISION_ENVELOPE - RESOLVER_TRACE - GUARD_TRACE required_fields: - source_page_id - commerce_hub_page_id - page_role - live_session_id - comment_id - messenger_thread_id_if_private - attribution_id_if_ads - campaign_id_internal_if_ads - adset_id_internal_if_ads - ad_id_internal_if_ads - referral_link_id_if_diamond - entry_channel forbidden_customer_facing: - campaign_id - adset_id - ad_id - attribution_id - ROAS - page_token_ref - psid

PASS khi:

Context được lưu trong DecisionEnvelope.

Ads attribution internal-only.

Live context không mất khi vào Messenger.

Diamond context chỉ dùng khi resolver pass.

FAIL nếu:

Customer-facing response có campaign/ad/ROAS.

Handoff làm mất live_session_id.

Diamond bind bị bịa.

## 6.3. GATE-03 — Public / Private Boundary Evidence

GATE_03_PUBLIC_PRIVATE_BOUNDARY_EVIDENCE: required_evidence_types: - DECISION_ENVELOPE - GUARD_TRACE - FINAL_GUARD_TRACE - CONTENT_RENDER_RESULT required_test_cases: - live_public_price_signal - live_public_order_signal - live_public_PII_signal - live_public_payment_signal - live_public_invoice_signal - live_public_health_sensitive_signal blocked_public_fields: - final_quote_price - listed_price_current - quote_snapshot_id - order_code - raw_phone - raw_address - payment_instruction - health_sensitive_detail

PASS khi:

Public price bị chặn.

Public order confirmation bị chặn.

PII không lặp lại.

Payment không public.

Health-sensitive không tư vấn dài public.

FAIL nếu:

Public response chứa final price.

Public response xác nhận đơn.

Public response lặp PII.

## 6.4. GATE-04 — Messenger Handoff Evidence

GATE_04_MESSENGER_HANDOFF_EVIDENCE: required_evidence_types: - NORMALIZED_EVENT - HANDOFF_DELIVERY_LOG - PUBLIC_REPLY_LOG - DECISION_ENVELOPE - DB_SNAPSHOT required_cases: - handoff_success_price_signal - handoff_success_order_signal - handoff_success_PII_signal - handoff_success_health_signal - handoff_failed_safe_fallback - duplicate_handoff_blocked required_fields: - handoff_id - handoff_status - public_reply_status - private_reply_status - board_id_if_live - session_board_id_if_live - live_session_id_if_live - attribution_id_if_ads - delivery_log_id

PASS khi:

Handoff success mới nói “đã gửi”.

Handoff fail dùng fallback.

Không có CTA yêu cầu khách tự inbox.

Handoff giữ board/live/attribution.

FAIL nếu:

Handoff fail mà nói “đã gửi”.

Public reply có “Mình inbox/nhắn Messenger”.

Không có delivery log.

## 6.5. GATE-05 — MC AI / Live Board Evidence

GATE_05_MC_AI_LIVE_BOARD_EVIDENCE: required_evidence_types: - BOARD_CONTEXT - SCRIPT_BRIEF - MC_AI_SEGMENT - VIDEO_REVALIDATION - ADMIN_UI_SCREEN - TEST_REPORT required_artifacts: - daily_live_product_board_export - live_session_product_board_export - ai_live_script_brief_export - script_segment_registry - video_segment_registry - revalidation_report - approved_playlist_report required_fields: - board_id - session_board_id - brief_id - segment_id - segment_code - video_status - revalidation_status - Approved For Live

PASS khi:

Script chỉ chứa SKU trong board.

Segment SKU map đúng board line.

Không segment nào Revalidation Required.

Chỉ clip Approved For Live vào playlist.

Không giá cuối/tồn kho/CTA tự inbox/claim điều trị.

FAIL nếu:

Script có SKU ngoài board.

Clip có SKU bị remove.

Clip chưa approved nhưng phát.

Board thay đổi nhưng không revalidate.

## 6.6. GATE-06 — Ads / ROAS Internal-only Evidence

GATE_06_ROAS_INTERNAL_ONLY_EVIDENCE: required_evidence_types: - GUARD_TRACE - ROAS_MAPPING - DB_SNAPSHOT - DASHBOARD_SAMPLE - CONTENT_RENDER_RESULT required_cases: - customer_response_no_campaign - customer_response_no_ad - customer_response_no_ROAS - quote_not_counted_as_revenue - created_order_not_counted_as_verified_revenue - ORDER_VERIFIED_counted_as_revenue - COD_FAILED_excluded - CANCELLED_excluded - RETURNED_excluded

PASS khi:

Customer response không có Ads/ROAS internal fields.

ROAS chỉ tính Order Verified.

COD fail/cancel/return bị loại khỏi revenue.

FAIL nếu:

Dashboard tính created order là revenue verified.

AI public campaign/ad.

Quote/inbox được tính doanh thu.

## 6.7. GATE-07 — Golden Hour Evidence

GATE_07_GOLDEN_HOUR_EVIDENCE: required_evidence_types: - RESOLVER_TRACE - QUOTE_SNAPSHOT - GUARD_TRACE - DB_SNAPSHOT - TEST_REPORT required_cases: - GoldenHourResolver_ACTIVE - GoldenHourResolver_NOT_ACTIVE - quote_hold_5_minutes - program_24_7_hold_15_minutes - expired_quote_revalidation - fake_urgency_blocked - combo_no_force_during_golden_hour

PASS khi:

Golden Hour active do resolver trả.

Quote Giờ Vàng giữ 5 phút.

Quote 24/7 giữ 15 phút.

Quote hết hạn revalidate.

Không ép combo vì Giờ Vàng.

FAIL nếu:

AI bịa Giờ Vàng active.

Quote hết hạn vẫn tạo order.

AI public giá Giờ Vàng.

## 6.8. GATE-08 — Sales-to-Order Evidence

GATE_08_SALES_TO_ORDER_EVIDENCE: required_evidence_types: - DECISION_ENVELOPE - QUOTE_SNAPSHOT - ORDER_RUNTIME - DB_SNAPSHOT - API_REQUEST_RESPONSE - DUPLICATE_IDEMPOTENCY required_artifacts: - advisor_sales_session_record - quote_cart_record - quote_snapshot_record - order_confirmation_draft_record - customer_confirmation_record - order_created_from_confirmed_quote_record - order_code_record - idempotency_test_report

PASS khi:

AI có sales_session.

Quote có QuoteSnapshot.

Order confirmation draft không bị gọi là order success.

Order chỉ tạo sau CustomerConfirmation.

Order success có order_code.

Retry không tạo duplicate order.

FAIL nếu:

AI báo giá thiếu QuoteSnapshot.

Order tạo thiếu CustomerConfirmation.

Không có order_code nhưng nói đơn đã ghi nhận.

## 6.9. GATE-09 — Entry / Growth / Combo Evidence

GATE_09_ENTRY_GROWTH_COMBO_EVIDENCE: required_evidence_types: - DECISION_ENVELOPE - RESOLVER_TRACE - GUARD_TRACE - CONTENT_RENDER_RESULT - QUOTE_SNAPSHOT_IF_QUOTE required_cases: - entry_order_one_box_allowed - growth_order_family - growth_order_gift - combo_parents - combo_family - combo_refusal_fallback_to_entry - combo_no_force_guard - combo_items_product_effectiveness

PASS khi:

Entry Order hợp lệ.

Growth Order có reason.

Combo có Product Effectiveness từng item.

Khách từ chối combo vẫn được mua 1 hộp nếu guard pass.

Không ép combo.

FAIL nếu:

AI nói “phải mua combo”.

AI xem 1 hộp là thất bại.

Combo thiếu Product Effectiveness.

Combo chứa SKU không sellable.

## 6.10. GATE-10 — CRM Evidence

GATE_10_CRM_EVIDENCE: required_evidence_types: - CRM_SUPPRESSION - DECISION_ENVELOPE - GUARD_TRACE - CONTENT_RENDER_RESULT - CUSTOMER_MEMORY_SAMPLE required_cases: - CRM_eligible_customer - opt_out_suppressed - open_case_suppressed - message_fatigue_suppressed - CRM_with_ProductEffectiveness - no_fake_purchase_history

PASS khi:

CRMEligibilityGuard chạy.

Opt-out bị chặn.

Open case bị chặn.

CRM có Product Effectiveness khi gợi ý sản phẩm.

Không bịa lịch sử mua.

FAIL nếu:

CRM gửi khi khách opt-out.

CRM gửi khi có case chất lượng/hàng giả/privacy/payment.

CRM gợi ý sản phẩm hời hợt không có hiệu dụng.

## 6.11. GATE-11 — Duplicate / Idempotency Evidence

GATE_11_DUPLICATE_IDEMPOTENCY_EVIDENCE: required_evidence_types: - DUPLICATE_IDEMPOTENCY - NORMALIZED_EVENT - DB_SNAPSHOT - TEST_REPORT required_cases: - duplicate_comment_no_second_public_reply - duplicate_webhook_no_second_handoff - duplicate_quote_command_no_second_quote - duplicate_order_command_no_second_order - duplicate_commission_command_no_second_ledger

PASS khi:

Một comment không tạo nhiều public reply.

Một webhook retry không tạo nhiều handoff.

Một confirmed quote không tạo nhiều order.

Commission không double-ledger.

FAIL nếu:

Duplicate event tạo duplicate reply/quote/order/commission.

## 6.12. GATE-12 — FILE 12 Non-Override Evidence

GATE_12_FILE12_NON_OVERRIDE_EVIDENCE: required_evidence_types: - GUARD_TRACE - TEST_REPORT - CONTENT_RENDER_RESULT - OWNER_APPROVAL required_cases: - no_new_claim - no_new_effectiveness - no_runtime_bypass - no_guard_bypass - no_forced_combo - no_public_campaign_ROAS - regression_pass

PASS khi:

FILE 12 không tạo claim mới.

FILE 12 không tạo hiệu dụng mới.

FILE 12 không bỏ guard.

FILE 12 không ép combo.

Regression pass.

FAIL nếu:

FILE 12 vượt FILE 00->11.

FILE 12 dùng conversion để bỏ safety/claim/runtime.

## 7. ARTIFACT TYPE CONTRACTS

## 7.1. DecisionEnvelope Evidence

decision_envelope_evidence: artifact_type: DECISION_ENVELOPE required_fields: - envelope_id - correlation_id - idempotency_key - message_id - conversation_id - channel_code - reply_surface - selected_action - blocked_actions - resolvers_called - guards_evaluated - final_guard_status - side_effects_created - side_effects_blocked conditional_required_fields: if_live: - live_session_id - board_id - session_board_id if_ads: - attribution_id - campaign_id_internal - ad_id_internal if_quote: - sales_session_id - quote_snapshot_id if_order: - customer_confirmation_status - order_code

## MUST NOT:

Không để DecisionEnvelope raw xuất cho khách.

Không chứa raw PII không mask trong review package.

## 7.2. Resolver Trace Evidence

resolver_trace_evidence: artifact_type: RESOLVER_TRACE required_fields: - resolver_name - resolver_status - input_summary - output_summary - fail_reason_code_if_any - executed_at - correlation_id important_resolvers: - ChannelContextResolver - FacebookContextResolver - LiveSessionResolver - AdsAttributionResolver - MessengerHandoffResolver - ProductKnowledgeResolver - ProgramPriceResolver - GoldenHourResolver - MemberRightsResolver - DiamondReferralResolver - QuoteSnapshotResolver - OrderRuntimeResolver - CRMEligibilityResolver

## MUST:

Resolver output có cấu trúc.

Không lưu token/secret/raw PII trong resolver trace rộng.

## 7.3. Guard Trace Evidence

guard_trace_evidence: artifact_type: GUARD_TRACE required_fields: - guard_name - guard_status - checked_rules - blocked_fields - blocked_actions - public_safe_reason_if_customer_facing - internal_reason_code - executed_at - correlation_id important_guards: - PublicPrivateSurfaceGuard - PrivacyPIIGuard - ClaimGuard - FinalGuard - ROASInternalOnlyGuard - QuoteSnapshotGuard - PriceLockGuard - CustomerConfirmationGuard - OrderCodeGuard - ComboNoForceGuard - CRMEligibilityGuard - OpenCaseSuppressionGuard - MetaDuplicateEventGuard

## MUST:

Guard fail phải tạo blocked_actions.

Guard internal reason không public cho khách.

## 7.4. Normalized Event Evidence

normalized_event_evidence: artifact_type: NORMALIZED_EVENT required_fields: - normalized_event_id - source_platform - source_page_id - event_type - received_at - idempotency_key - correlation_id - raw_payload_ref - payload_signature_status - dedup_status conditional_required_fields: if_live_comment: - live_session_id - comment_id - board_id - session_board_id if_ads: - attribution_id - campaign_id_internal - ad_id_internal if_handoff: - handoff_required - handoff_reason

## MUST:

raw_payload_ref internal only.

Signature status phải rõ.

Dedup status phải rõ.

## 7.5. Handoff Evidence

## MUST:

Handoff fail không được có “đã gửi Messenger”.

Handoff success phải có delivery log.

## 7.6. MC AI Segment Evidence

mc_ai_segment_evidence: artifact_type: MC_AI_SEGMENT required_fields: - segment_id - board_id - session_board_id - brief_id - segment_code - segment_group - segment_role - script_version - video_file_ref - video_status - revalidation_status - Approved For Live - approved_by - approved_at conditional_required_fields: if_sku_segment: - sku_id - product_public_name - live_board_sku_line_id validation_checks: - sku_in_board - no_final_price - no_stock_quantity - no_forbidden_CTA - no_treatment_claim - no_campaign_ROAS

## MUST:

Clip SKU phải map với board line.

Clip Revalidation Required không được phát.

Clip chưa Approved For Live không được vào playlist.

## 7.7. QuoteSnapshot Evidence

quote_snapshot_evidence: artifact_type: QUOTE_SNAPSHOT required_fields: - quote_snapshot_id - sales_session_id - quote_status - product_lines - final_payable - quote_hold_minutes - quote_created_at - quote_expires_at - applied_benefits - blocked_benefits_if_any conditional_required_fields: if_golden_hour: - quote_hold_minutes = 5 if_program_24_7: - quote_hold_minutes = 15 if_member: - member_tier - member_benefit if_diamond: - diamond_bind_status - diamond_benefit_if_applied if_ads_or_live: - attribution_snapshot - live_session_id_if_live

## MUST:

Final price chỉ từ QuoteSnapshot.

QuoteSnapshot ID không customer-facing.

Quote hết hạn phải có revalidation evidence nếu tạo order sau đó.

## 7.8. Order Runtime Evidence

order_runtime_evidence: artifact_type: ORDER_RUNTIME required_fields: - order_code - quote_snapshot_id - sales_session_id - customer_confirmation_status - order_status - created_at - idempotency_key required_links: - quote_snapshot_record - order_confirmation_draft_record - customer_confirmation_record - order_items_from_quote_snapshot validation: order_success_allowed_only_if: - order_code present - order_status created_or_confirmed_by_runtime

## MUST:

Không có order_code thì không render order success.

Order item không nhập lại từ AI text.

## 7.9. CRM Suppression Evidence

## MUST:

CRM không gửi khi suppression fail.

CRM gợi ý sản phẩm phải có Product Effectiveness evidence.

## 7.10. ROAS Mapping Evidence

## MUST:

ROAS không tính quote/inbox/comment.

ROAS không public cho khách.

## 8. EVIDENCE FILE NAMING STANDARD

evidence_file_naming: pattern: "YYYYMMDD_ENV_GATECODE_EVIDENCETYPE_SEQUENCE_VERSION" examples: - "20260513_STAGING_GATE03_GUARDTRACE_001_V01.json" - "20260513_STAGING_GATE04_HANDOFFLOG_001_V01.json" - "20260513_STAGING_GATE05_MCAI_SEGMENT_001_V01.json" - "20260513_STAGING_GATE08_QUOTESNAPSHOT_001_V01.json" - "20260513_STAGING_GATE11_IDEMPOTENCY_001_V01.json"

## MUST NOT:

Không đặt tên file “test ok”.

Không đặt “final final”.

Không đặt tên chứa số điện thoại khách.

Không đặt tên chứa token/secret.

## 9. EVIDENCE PRIVACY & SECURITY

## 9.1. PII Masking

pii_masking_policy: phone: display: "09******123" address: display: "masked_address" email: display: "u***@domain.com" tax_code: display: "masked_tax_code" psid: display: "hashed_psid"

## MUST:

PII phải mask trước khi đưa vào owner/QA package rộng.

Raw PII chỉ giữ trong system storage theo quyền.

Evidence không được lặp lại PII public comment.

## 9.2. Secret Handling

Forbidden in evidence:

Page access token thật.

App secret.

Verify token.

Webhook secret.

Payment secret.

Bank credential.

API key.

Cookie/session token.

## MUST:

Chỉ lưu secret_ref.

Screenshot không được lộ token.

## 10. EVIDENCE VALIDATION RULES

## 11. PACKAGE REVIEW WORKFLOW

evidence_package_review_workflow: DRAFT: next: - COLLECTING_EVIDENCE COLLECTING_EVIDENCE: next: - READY_FOR_QA_REVIEW - bị chặn READY_FOR_QA_REVIEW: next: - QA_REVIEWING QA_REVIEWING: next: - PASS - FAIL - bị chặn PASS: next: - OWNER_SIGN_OFF FAIL: next: - FIX_REQUIRED bị chặn: next: - FIX_REQUIRED OWNER_SIGN_OFF: next: - GATEWAY_TRANSITION_ALLOWED - REQUEST_FIX

## MUST:

QA không được PASS nếu thiếu P0 evidence.

Owner không ký nếu còn P0 FAIL/waiting.

Gateway Transition chỉ mở sau Owner Sign-off.

## 12. EVIDENCE PACKAGE SUMMARY TABLE

Gate

Evidence bắt buộc

PASS khi

## GATE-01

DecisionEnvelope channel samples

Nhận đúng channel/surface

## GATE-02

Normalized event + ads/live context

Context internal đầy đủ

## GATE-03

Guard trace public/private

Public không leak giá/PII/order

## GATE-04

Handoff delivery log

Handoff success/fail đúng

## GATE-05

Board/script/segment evidence

MC AI dùng đúng board

## GATE-06

ROAS mapping

ROAS chỉ từ Order Verified

## GATE-07

GoldenHourResolver + QuoteSnapshot

GH/24-7 hold đúng

## GATE-08

Sales session / quote / order

Quote/order đúng gate

## GATE-09

Entry/Growth/Combo traces

Không ép combo, có hiệu dụng

## GATE-10

CRM suppression

CRM không spam/sai lúc

## GATE-11

Idempotency report

Không duplicate

## GATE-12

FILE 12 regression

Không override

## 13. MASTER DONE GATE

Evidence Package Schema đạt chuẩn khi:

Có root package schema.

Có evidence item schema.

Có artifact ref schema.

Có required evidence map cho GATE-01 -> GATE-12.

Có artifact type contract.

Có DecisionEnvelope evidence contract.

Có resolver trace contract.

Có guard trace contract.

Có Gateway normalized event evidence.

Có handoff evidence.

Có MC AI segment evidence.

Có QuoteSnapshot evidence.

Có Order Runtime evidence.

Có CRM suppression evidence.

Có ROAS mapping evidence.

Có duplicate/idempotency evidence.

Có privacy/PII masking policy.

Có secret handling policy.

Có validation rules.

Có review workflow.

Có fail gate rõ.

Có owner sign-off linkage.

## 14. MASTER FAIL GATE

Evidence Package FAIL nếu:

P0 evidence thiếu artifact.

Không có DecisionEnvelope cho AI decision.

Không có guard trace cho public/private block.

Không có handoff delivery log.

Không có board/script/segment evidence cho MC AI.

Không có QuoteSnapshot cho báo giá.

Không có CustomerConfirmation / order_code cho order success.

Không có Order Verified mapping cho ROAS.

Không có CRM suppression evidence.

Không có idempotency evidence.

Evidence chứa raw PII chưa mask.

Evidence chứa token/secret.

Screenshot được dùng thay runtime evidence.

Owner ký khi còn P0 FAIL/waiting.

## 15. OWNER SIGN-OFF LINKAGE

owner_sign_off_linkage: evidence_package_id: string completion_report_id: string sign_off_allowed_only_if: - all_P0_evidence_status_PASS - no_P0_open_issue - evidence_schema_valid - QA_review_PASS - privacy_review_PASS_if_PII - security_review_PASS_if_gateway_related sign_off_roles: - Owner - Project Lead - AI Lead - Core Runtime Lead - Gateway Lead - QA Lead - Ops_MC_AI_Lead

## MUST:

Owner sign-off phải gắn với evidence_package_id.

Không ký PASS bằng miệng.

Không ký PASS khi evidence còn thiếu.

## 16. KẾT LUẬN KHÓA

Tài liệu AI_CHANNEL_FACEBOOK_EVIDENCE_PACKAGE_SCHEMA_V2.4 là chuẩn bắt buộc để thu thập, lưu, kiểm tra và ký duyệt evidence cho:

AI Advisor Facebook Completion Report.

MC AI Live readiness.

Facebook Gateway transition.

Live-to-Order E2E smoke.

Owner Sign-off.

Từ sau tài liệu này:

Completion Report không được PASS nếu thiếu evidence.

Gateway không được production nếu Evidence Package chưa PASS.

MC AI không được live nếu thiếu board/script/video/revalidation evidence.

AI không được quote nếu thiếu QuoteSnapshot evidence.

Order không được success nếu thiếu order_code evidence.

ROAS không được tính nếu thiếu Order Verified evidence.

CRM không được gửi nếu thiếu eligibility/suppression evidence.

Không có P0 evidence thì không release.

Trạng thái mặc định sau khi tạo schema:READY_FOR_EVIDENCE_COLLECTION

Bước tiếp theo:Viết MC_AI_VIDEO_SEGMENT_REGISTRY_CONTRACT để khóa object quản lý clip, trạng thái video, version, board mapping và revalidation trước live.

## HẾT TÀI LIỆU — AI_CHANNEL_FACEBOOK_EVIDENCE_PACKAGE_SCHEMA_V2.4

## 17. THÔNG TIN MÌNH CẦN CUNG CẤP CHO DEV

Mình chuẩn bị giúp các thông tin sau:

## 17.1. Danh sách Page thật

Page Name

Page ID

Vai trò

Bật live?

Bật Messenger?

Bật CRM?

Ái Vy Phạm

61557082440623

page CSKH

có

không

không

Hương Xuân

61585392260203

page live

có

không

không

Huyền Trân

61585211732304

page test

không

không

không

Sâm Biển Việt Nam

102437252680397

page chạy ads

có

không

không

Ginsengfood - Cháo Sâm Mỗi Ngày

101044219306135

page chính

có

có

có

Ginsengfood - Thực Dưỡng Gia Đình

161431274523980

page phụ

không

không

không

Ginsengfood - Chăm Sóc Ba Mẹ

105347285574705

page phụ

không

không

không

Ginsengfood - Món Chay Thực Dưỡng

395032681300769

page phụ

không

không

không

Ginsengfood - Quà Tặng Sức Khỏe

330237488279968

page phụ

không

không

không

## 17.2. Meta Business / App

## 17.2.1. Ginsengfood - Cháo Sâm Mỗi Ngày

Trường

Giá trị

Business ID

101044219306135

App ID

## CHƯA TẠO — DEV TẠO TRONG META FOR DEVELOPERS

App Name

DỰ KIẾN: Ginsengfood AI Advisor Channel Gateway

Admin tài khoản Meta

https://www.facebook.com/phutuongnguyendung/

Technical contact

https://www.facebook.com/phutuongnguyendung/

## 17.2.2. Ginsengfood - Thực Dưỡng Gia Đình

Trường

Giá trị

Business ID

161431274523980

App ID

## CHƯA TẠO — DEV TẠO TRONG META FOR DEVELOPERS

App Name

DỰ KIẾN: Ginsengfood AI Advisor Channel Gateway

Admin tài khoản Meta

https://www.facebook.com/phutuongnguyendung/

Technical contact

https://www.facebook.com/phutuongnguyendung/

## 17.2.3. Ginsengfood - Chăm Sóc Ba Mẹ

Trường

Giá trị

Business ID

105347285574705

App ID

## CHƯA TẠO — DEV TẠO TRONG META FOR DEVELOPERS

App Name

DỰ KIẾN: Ginsengfood AI Advisor Channel Gateway

Admin tài khoản Meta

https://www.facebook.com/phutuongnguyendung/

Technical contact

https://www.facebook.com/phutuongnguyendung/

## 17.2.4. Ginsengfood - Món Chay Thực Dưỡng

Trường

Giá trị

Business ID

395032681300769

App ID

## CHƯA TẠO — DEV TẠO TRONG META FOR DEVELOPERS

App Name

DỰ KIẾN: Ginsengfood AI Advisor Channel Gateway

Admin tài khoản Meta

https://www.facebook.com/phutuongnguyendung/

Technical contact

https://www.facebook.com/phutuongnguyendung/

## 17.2.5. Ginsengfood - Quà Tặng Sức Khỏe

Trường

Giá trị

Business ID

395032681300769

App ID

## CHƯA TẠO — DEV TẠO TRONG META FOR DEVELOPERS

App Name

DỰ KIẾN: Ginsengfood AI Advisor Channel Gateway

Admin tài khoản Meta

https://www.facebook.com/phutuongnguyendung/

Technical contact

https://www.facebook.com/phutuongnguyendung/

## 17.2.6. Ái Vy Phạm

Trường

Giá trị

Profile ID

61557082440623

App ID

## CHƯA TẠO — DEV TẠO TRONG META FOR DEVELOPERS

App Name

DỰ KIẾN: Ginsengfood AI Advisor Channel Gateway

Admin tài khoản Meta

https://www.facebook.com/profile.php?id=61557082440623

Technical contact

https://www.facebook.com/profile.php?id=61557082440623

## 17.2.7. Hương Xuân

Trường

Giá trị

Profile ID

61585392260203

App ID

## CHƯA TẠO — DEV TẠO TRONG META FOR DEVELOPERS

App Name

DỰ KIẾN: Ginsengfood AI Advisor Channel Gateway

Admin tài khoản Meta

https://www.facebook.com/profile.php?id=61585392260203

Technical contact

https://www.facebook.com/profile.php?id=61585392260203

## 17.2.8. Huyền Trân

Trường

Giá trị

Profile ID

61585211732304

App ID

## CHƯA TẠO — DEV TẠO TRONG META FOR DEVELOPERS

App Name

DỰ KIẾN: Ginsengfood AI Advisor Channel Gateway

Admin tài khoản Meta

https://www.facebook.com/profile.php?id=61585211732304#

Technical contact

https://www.facebook.com/profile.php?id=61585211732304#

## 17.2.9. Sâm Biển Việt Nam

Trường

Giá trị

Profile ID

102437252680397

App ID

## CHƯA TẠO — DEV TẠO TRONG META FOR DEVELOPERS

App Name

DỰ KIẾN: Ginsengfood AI Advisor Channel Gateway

Admin tài khoản Meta

https://www.facebook.com/phutuongnguyendung

Technical contact

https://www.facebook.com/phutuongnguyendung

## 17.3. Quyết định vận hành

Câu hỏi

Owner chốt

Page nào là Page chính thức?

Ginsengfood - Cháo Sâm Mỗi Ngày

Page nào dùng live?

Ái Vy Phạm

Hương Xuân

Sâm Biển Việt Nam

Ginsengfood - Cháo Sâm Mỗi Ngày

Page nào chỉ test?

Huyền Trân

Page nào được AI auto reply?

Ginsengfood - Cháo Sâm Mỗi Ngày

Page nào được CRM?

Ginsengfood - Cháo Sâm Mỗi Ngày

CRM tối đa bao nhiêu tin/ngày/khách?

Public reply trên live: 1 lần / comment

Private reply từ comment: 1 lần / comment đủ điều kiện

Messenger trả lời khách đang nhắn: Theo hội thoại, không spam

CRM chăm sóc sau mua: Tối đa 1 tin / ngày / khách

CRM gợi ý mua lại/đổi vị: Tối đa 1 tin / 3 ngày / khách

Win-back khách lâu chưa mua: Tối đa 1 tin / 7 ngày / khách

Campaign khuyến mãi: Tối đa 1 campaign / ngày / khách, nếu có consent và policy cho phép

Quiet hour từ mấy giờ đến mấy giờ?

22h00 – 08h00

Khi khách chửi/lừa đảo thì AI trả lời hay human?

- Nhẹ (“Có lừa đảo không?”, “Sao mắc vậy?”): AI trả lời trust-safe, không tranh luận

- Trung bình (“Tôi không tin, gửi bằng chứng đi”): AI gửi bằng chứng/link approved nếu có, hoặc kéo Messenger

- Nặng (chửi thề, vu khống, spam lặp, kích động): AI không đôi co, chuyển human/moderation, có thể ẩn/báo admin theo policy

Khi khách hỏi bệnh nền thì AI hỏi tiếp hay chuyển người thật?

Theo hệ thống AI Advisor Ginsengfood đã khóa:

AI được hỏi tiếp bằng mẫu health-sensitive nếu tình huống chưa quá phức tạp. Không chốt đơn ngay. Không chọn SKU ngay khi chưa biết kiêng cữ.

Nhưng AI phải chuyển người thật nếu:

khách đang nằm viện/sau mổ nặng;

phụ nữ mang thai hỏi chi tiết;

trẻ nhỏ;

đang dùng thuốc chống đông;

suy thận/lọc thận;

dị ứng nặng;

bác sĩ có dặn kiêng nhiều thành phần;

khách hỏi thay thuốc/chữa bệnh;

khách cần cam kết y khoa;

AI không đủ dữ liệu để lọc an toàn.
