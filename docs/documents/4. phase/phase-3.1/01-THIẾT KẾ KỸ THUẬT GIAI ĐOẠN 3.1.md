# 01 - THIẾT KẾ KỸ THUẬT GIAI ĐOẠN 3.1

## 1. Mode

`CHỈ THIẾT KẾ KỸ THUẬT`.

Không implement, không migration, không seed, không đổi contract khi chưa có approval. Thiết kế này là nền bắt buộc trước khi chạy các file 02 đến 07.

## 2. Mục tiêu thiết kế

Thiết kế kỹ thuật phải khóa đầy đủ:

- Runtime owner.
- Resolver/guard/engine.
- API/DTO/event contract nếu cần.
- DB/entity/job impact nếu cần.
- UI/Admin/CRM/worker impact nếu cần.
- Smoke/evidence cho từng workstream.
- P0 blocker/fail-closed behavior.
- Downstream handoff cho AI, Gateway, CRM, Live, Ads, IVR.

Thiết kế không được biến PHASE-03.1 thành nguồn thay thế PHASE-03. PHASE-03 vẫn sở hữu Commerce Runtime.

## 3. Input bắt buộc

- Report từ `00-PHAN-TICH-HIEN-TRANG-GIAI-DOAN-3-1.md`.
- 6 tài liệu nguồn trong `5. bo sung/`.
- `TÀI LIỆU TRIỂN KHAI DIAMOND HOA HỒNG, THƯỞNG MỚI.md`.
- PHASE-03 Commerce Runtime.
- PHASE-04 AI Advisor nếu thiết kế chạm AI.
- PHASE-05 Gateway nếu thiết kế chạm delivery/public-private.
- PHASE-08 IVR nếu thiết kế chạm IVR.

Nếu thiếu analysis report, ghi:

`BLOCKED_NEED_PHASE_3_1_ANALYSIS_REPORT`

## 4. Workstream thiết kế

| Workstream | File triển khai | Backlog | Nội dung |
| --- | --- | --- | --- |
| P3.1-2A | `02-THÀNH-VIÊN-GIÁ-CHƯƠNG-TRÌNH-24-7-GIỜ-VÀNG.md` | P3_1-BLG-001/002/003 | Member, price, 24/7, Golden Hour, early access, QuoteSnapshot. |
| P3.1-2B | `03-DIAMOND-GIỚI-THIỆU-HOA-HỒNG-ĐỐI-SOÁT.md` | P3_1-BLG-004 | Diamond referral, buyer benefit, commission, reversal, dispute. |
| P3.1-2C | `04-TIN-NHẮN-GIỜ-VÀNG-CRM-ĐA-KÊNH.md` | P3_1-BLG-005 | Message template, trigger, channel, email, no duplicate. |
| P3.1-2D | `05-CRM-12-THÁNG-VÒNG-ĐỜI-THÀNH-VIÊN.md` | P3_1-BLG-006 | CRM timeline, lifecycle, quiet hours, frequency, outcome. |
| P3.1-2E | `06-AI-ADVISOR-CHÍNH-SÁCH-LÕI-TÌNH-HUỐNG-THỰC-CHIẾN.md` | P3_1-BLG-007/008 | AI core policy, situation locks, content guard, product recommendation. |
| P3.1-2F | `07-THANH-TOÁN-VẬN-CHUYỂN-IVR-ĐƠN-HÀNG.md` | P3_1-BLG-009/010/011 | Bank transfer, shipping, IVR, order edit/refund/VAT. |
| P3.1-2G | `08-KIỂM-THỬ-BẰNG-CHỨNG-BÁO-CÁO-GIAI-ĐOẠN-3-1.md` | P3_1-BLG-012 | Kiểm thử/bằng chứng/báo cáo/fail gate. |

## 5. Runtime owner matrix

| Dữ liệu / quyết định | Owner truth | Consumer được phép |
| --- | --- | --- |
| Product public name, SKU, pillar, dietary, claim | Product/Product Knowledge Core | AI, CRM, Live, Gateway |
| Sellable, stock, quality hold, recall hold, sale lock | Operational + Availability Core | Commerce, AI, CRM, Live |
| Listed price, program price, final total | Pricing Core + QuoteSnapshot | AI, CRM, Gateway, Live |
| Quote expiry, VAT, shipping, COD fee | QuoteSnapshot + Commerce Core | AI, CRM, Gateway |
| Cart, Order Draft, Official Order, order_code | Order Core | AI, Gateway, IVR |
| Payment status | Payment Core + Accounting Review | AI, CRM, Gateway, Admin |
| Shipping tracking/ETA | Shipping Core | AI, CRM, Gateway |
| Member tier and benefit | Member Core | AI, CRM, Gateway |
| Diamond referral and commission | Diamond Referral Core + Verified Revenue | CRM, Ads, Admin |
| CRM eligibility, quiet hours, suppression | CRM Lifecycle Core | CRM Delivery, AI |
| Message text | Approved Template Registry | CRM Delivery |
| Health/privacy/refund/open case | Safety/Case/Privacy Core | AI, CRM, Gateway |
| IVR required/result | IVR + Order Core | Order Core, Admin |

## 6. Resolver / guard / engine registry

| Nhóm | Resolver / Guard / Engine bắt buộc |
| --- | --- |
| Identity | CustomerIdentityResolver, CustomerMemoryResolver, LastPurchaseResolver, IdentityResolutionGuard |
| Member | MembershipTierResolver, MemberLifecycleResolver, MemberMaintainResolver, MemberUpgradeResolver, MemberGracePeriodResolver, MemberDowngradeResolver |
| Rights | RuntimeRightsResolver, MemberBenefitResolver, ProgramEligibilityResolver, PolicyPriorityResolver |
| Price/Quote | ProgramPriceResolver, QuoteEngine, QuoteRightsGuard, QuoteCompletenessGuard, QuoteCartChangeGuard |
| Golden Hour | GoldenHourSessionResolver, GoldenHourEligibilityResolver, GoldenHourQuotaGuard, EarlyAccessResolver, QuotaReleaseGuard |
| Diamond | DiamondReferralResolver, DiamondBindResolver, DiamondBuyerBenefitResolver, DiamondCommissionResolver, CommissionReversalGuard, MemberDisputeEscalationResolver |
| Messaging | MessageTriggerResolver, MessageTemplateResolver, MessageTextImmutableGuard, MonthlyDuplicateGuard, CrossChannelDedupGuard, ChannelEligibilityGuard |
| CRM | CRMEligibilityGuard, MessageFatigueGuard, QuietHoursGuard, CRMOpenCaseSuppressionGuard, MemberLifecycleOutcomeLogger |
| Product recommendation | ProductPillarResolver, VeganDietaryGuard, ProductTriadFamilyAddOnResolver, ProductEffectivenessResolver, InventoryPriorityResolver, ProductRotationResolver |
| AI safety | ClaimGuard, FinalResponseGuard, HealthSafetyGuard, ConditionSensitiveClaimGuard, PublicPrivateBoundaryGuard |
| Payment | BankTransferReferenceResolver, PaymentReconciliationGuard, AccountingReviewResolver, DuplicateTransactionGuard |
| Shipping | ShippingResolver, DeliveryTrackingResolver, DeliveryRegionFallbackResolver, TrackingDataFreshnessGuard |
| IVR/Order | IVREligibilityResolver, IVRRiskScoreResolver, IVRStateTransitionGuard, OrderVerificationGuard, OrderEditEligibilityResolver |
| Evidence | DecisionEnvelopeLogger, ResolverTraceLogger, GuardTraceLogger, CRMJobLog, OutcomeLog, AuditLog |

## 7. Contract thiết kế tối thiểu

| Contract | Fields bắt buộc |
| --- | --- |
| MemberLifecycleSnapshot | member_id, tier, cycle_start, cycle_end, eligible_revenue, amount_to_maintain, amount_to_next_tier, grace_status, days_left |
| MemberBenefitDecision | member_id, tier, program_type, benefit_percent, allow_apply, deny_reason, policy_version |
| ProgramPriceDecision | sku_id, listed_price, program_type, program_discount, program_price, effective_date, source |
| GoldenHourSessionDecision | session_id, sku_id, stock, quota, start_time, end_time, early_access_window, status |
| DiamondReferralSnapshot | referral_link_id, owner_member_id, owner_tier, bind_status, policy_version |
| DiamondCommissionDecision | source_order_id, verified_revenue_id, commission_rate, commission_amount, status, reversal_status |
| MessageTemplate | template_id, message_family, approved_text_hash, variables, policy_version, active_status |
| MessageDeliveryDecision | customer_id, channel, trigger_id, template_id, allow_send, deny_reason, dedup_key |
| CRMMemberLifecycleEvent | event_id, customer_id, member_id, milestone, branch, outcome, next_action |
| ProductRecommendationDecision | customer_id, context, selected_items, pillar_map, effect_summary, safety_boundary |
| BankTransferPaymentReference | payment_reference, order_id, amount, expires_at, status, accounting_status |
| ShippingETAResult | order_id, tracking_status, tracking_link, region, fallback_eta, freshness_status |
| IVRRequiredDecision | order_draft_id, required, risk_reasons, attempt_policy, quota_release_policy |
| DecisionEnvelope | decision_id, source_ref, input_hash, result, reason_code, correlation_id, created_at |

## 8. API/DTO/event impact review

Thiết kế phải nêu rõ nếu cần:

- Endpoint read-only cho member/benefit/golden hour decision.
- Endpoint hoặc internal contract cho Diamond commission.
- Event cho member lifecycle milestone.
- Event cho CRM message trigger.
- Event cho bank transfer selected/reconciled.
- Event cho IVR required/confirmed/timeout.
- DTO cho QuoteSnapshot benefit breakdown.
- DTO cho delivery decision/suppression.

Không được thêm API nếu chỉ để AI/Gateway tự tính nghiệp vụ. API phải trả decision từ owner runtime.

## 9. Database/job impact review

Thiết kế phải kiểm tra có cần:

- Member cycle/tier history.
- Member revenue ledger.
- Benefit policy version.
- Golden Hour session/quota.
- Diamond referral bind.
- Commission ledger/reversal.
- Message template/trigger registry.
- Delivery dedup log.
- CRM lifecycle event/outcome log.
- Payment reference/accounting review queue.
- Shipping tracking cache/fallback.
- IVR task/state log.

Nếu chưa chắc chắn, ghi `PROPOSED_ONLY - NEED_OWNER_REVIEW`.

## 10. UI/Admin impact review

Thiết kế phải kiểm tra:

- Admin xem member tier/lifecycle.
- Admin xem Diamond commission/dispute.
- Admin quản template message đã duyệt.
- Admin xem CRM delivery/suppression.
- Accounting queue cho bank transfer.
- Shipping status/tracking/fallback.
- IVR monitoring nếu thuộc scope.
- Evidence viewer/audit trail.

Admin UI không được hardcode policy, không tự set PAID, không sửa commission nếu thiếu audit/reason.

## 11. P0 blocker design

Thiết kế FAIL nếu:

- Không có owner truth cho price/benefit/commission/payment/shipping.
- Không có Scope Out cho từng workstream.
- Không có fail-closed behavior khi runtime thiếu.
- Cho phép hardcode policy.
- Cho phép message text mutable.
- Cho phép AI tự tính hoặc tự hứa nghiệp vụ.
- Cho phép payment PAID thiếu confirmation.
- Cho phép commission thiếu Verified Revenue.
- Cho phép Gateway mở hoặc release claim.

## 12. Evidence và smoke design

Mỗi workstream phải có:

- Smoke happy path.
- Smoke fail path.
- Smoke missing runtime.
- Smoke privacy/safety nếu có dữ liệu khách.
- Evidence log.
- Decision envelope.
- Correlation/audit reference.
- Masking note nếu có PII/payment.

## 13. Done gate

File 01 DONE khi:

- Workstream matrix đầy đủ.
- Owner matrix đầy đủ.
- Resolver/guard registry đầy đủ.
- Contract impact rõ.
- DB/API/UI/job impact rõ hoặc blocker rõ.
- P0 blocker và smoke/evidence đủ.
- Không implementation.
- Không release claim.

## 14. Final status

`GIAI ĐOẠN 3.1 THIẾT KẾ KỸ THUẬT HOÀN TẤT - CHƯA IMPLEMENT - GATEWAY BLOCKED`
