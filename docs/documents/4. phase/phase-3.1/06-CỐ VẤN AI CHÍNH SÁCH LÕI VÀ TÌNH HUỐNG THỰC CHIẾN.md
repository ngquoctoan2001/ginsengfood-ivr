# 06 - AI ADVISOR / CHÍNH SÁCH LÕI / TÌNH HUỐNG THỰC CHIẾN

## 1. Mục tiêu

File này khóa toàn bộ AI Advisor rule phát sinh từ test thực chiến và bản bổ sung tình huống đời thường.

Đây là nguồn triển khai chính cho workstream P3.1-2E.

Sau khi dùng file này, người triển khai không được quay lại tự diễn giải rời rạc từ các note cũ. Mọi note nguồn đã được gom thành rule, resolver/guard, situation và smoke.

## 2. Source đã gom

| Source | Nội dung đã gom |
| --- | --- |
| `5. bo sung/1. BẢN TỔNG HỢP SAU TEST THỰC CHIẾN + CORE POLICY + CRM MESSAGING + MEMBER LIFECYCLE.md` | CORE-01 đến CORE-20, AI-TEST-NOTE 001-063, resolver/guard registry, situation registry, rewrite impact. |
| `5. bo sung/2. BẢN KHÓA BỔ SUNG CÁC TÌNH HUỐNG ĐỜI THƯỜNG TRƯỚC KHI VIẾT LẠI TÀI LIỆU.md` | AI-TEST-NOTE 076-096, everyday situations, payment/shipping/IVR/health/privacy/fake goods. |
| `5. bo sung/5. CRM 12-MONTH + MEMBER LIFECYCLE AUTOMATION LOCK.md` | Product triad + family add-on, member proactive amount, CRM suppression. |
| PHASE-04 AI Advisor | AI consumer boundary, Final Response Guard, Claim Guard. |
| PHASE-03 Commerce | QuoteSnapshot, Order Draft, Payment/Shipping owner truth. |

## 3. 20 core policy locks

| Mã | Rule | Cách triển khai |
| --- | --- | --- |
| CORE-01 | AI phải biết khách là ai trước khi tư vấn | CustomerIdentityResolver + CustomerMemoryResolver. |
| CORE-02 | Không dùng từ "hệ thống" với khách | FinalResponseGuard kiểm tra customer-facing wording. |
| CORE-03 | AI không tự tính giá/quyền lợi/hoa hồng | QuoteSnapshot/RuntimeRightsResolver/DiamondCommissionResolver. |
| CORE-04 | Mỗi gợi ý sản phẩm có hiệu dụng Đông phương | ProductEffectivenessResolver bắt buộc. |
| CORE-05 | Khách đã mua quay lại phải hỏi thăm trước | LastPurchaseResolver + ReturningCustomerCareGuard. |
| CORE-06 | Quote line-by-line đầy đủ | QuoteCompletenessGuard. |
| CORE-07 | Order draft là form prefill | OrderDraftPrefillFormGuard. |
| CORE-08 | Public comment chỉ kéo private/Messenger | PublicPrivateBoundaryGuard. |
| CORE-09 | Affiliate/Diamond tách đại lý/mua sỉ | DistributorIntentGuard. |
| CORE-10 | Live abuse hide/no reply/no Messenger | LiveAbuseClassifier + PurchaseBlockGuard. |
| CORE-11 | CRM message text immutable | MessageTextImmutableGuard. |
| CORE-12 | Gateway vẫn blocked | Release/Gateway guard. |
| CORE-13 | CRM 12 tháng là chăm sóc vòng đời | CRMLifecycleResolver. |
| CORE-14 | Member lifecycle proactive | MemberLifecycleResolver. |
| CORE-15 | Maintain hạng = 50% chuẩn hạng | MemberMaintainResolver. |
| CORE-16 | Recommendation mặc định 3 trụ + người thân | ProductTriadFamilyAddOnResolver. |
| CORE-17 | Priority conflict thắng sales | PriorityConflictResolver. |
| CORE-18 | Cross-channel dedup | CrossChannelDedupGuard. |
| CORE-19 | Member dispute escalation | MemberDisputeEscalationResolver. |
| CORE-20 | Member outcome log | MemberLifecycleOutcomeLogger. |

## 4. AI-TEST-NOTE 001-063 gom rule

| Nhóm note | Nội dung khóa | Resolver/Guard |
| --- | --- | --- |
| 001-002 | Public comment mơ hồ và handoff private | CommentIntentResolver, MessengerHandoffGuard |
| 003-010 | Giá, multi-SKU quote, proposal, order draft | QuoteEngine, ActiveProposalContextResolver, OrderDraftCompletenessGuard |
| 011-024 | Không hỏi vòng, combo gia đình, free ship upsell, VAT, quote expiry | QuestionBudgetGuard, SharedUserFitResolver, QuoteCompletenessGuard |
| 025-026 | Diamond buyer benefit wording | DiamondBuyerBenefitResolver, QuoteRightsGuard |
| 027-036 | Sâm Savigin science/contact/address/visit/agency/privacy | ScientificEvidenceResolver, SaviginScienceClaimGuard, OfficialContactRoutingGuard |
| 037-044 | Khách cũ, member rights, quote path, prefill order draft | LastPurchaseResolver, MemberBenefitResolver, OrderDraftPrefillFormGuard |
| 045-048 | Abuse/troll/profanity/brand attack | ProfanityNormalizer, BrandAttackClassifier, TrollSpamBurstGuard |
| 049-053 | Affiliate vs đại lý, runtime-first policy | DistributorIntentGuard, DiamondReferralResolver |
| 054-063 | CRM immutable, trigger-only, no duplicate, fatigue | MessageTriggerResolver, MessageTextImmutableGuard, MonthlyDuplicateGuard |

## 5. AI-TEST-NOTE 076-096 gom rule

| Note | Tên khóa | Must | Must Not | Resolver/Guard |
| --- | --- | --- | --- | --- |
| 076 | Health / disease / recovery | Health guard, thực dưỡng, không quote khi unresolved | Không chữa bệnh/thay thuốc/cam kết khỏi | HealthSafetyGuard, AllergyDietaryGuard |
| 077 | Gift / corporate / international / recovery gift | Tone sang trọng, quà đúng recipient, gift safety | Không hứa gửi quốc tế khi chưa eligible | GiftIntentResolver, CorporateGiftResolver |
| 078 | Office / student / institution | Văn phòng/sinh viên/công ty/trường học đúng context | Không mặc định đại lý | OfficeUseCaseResolver, InstitutionBuyerResolver |
| 079 | Overseas Vietnamese / international student | Hometaste, xa nhà, shipping eligibility | Không cam kết hải quan/quốc tế | InternationalShippingEligibilityResolver |
| 080 | 20 SKU pillar / vegan | 5 seasonal, 6 functional, 9 nourishing, 4 vegan | Không gợi ý SKU mặn cho vegan | ProductPillarResolver, VeganDietaryGuard |
| 081 | Product rotation / inventory priority | Ưu tiên sellable/fit/pillar/inventory | Không xả hàng/đẩy tồn | InventoryPriorityResolver |
| 082 | New customer IVR / fake order | IVR required nếu high-risk | Không order_code trước IVR pass | IVREligibilityResolver |
| 083 | Shipping ETA/tracking | Tracking realtime hoặc fallback vùng | Không bịa trạng thái | DeliveryTrackingResolver |
| 084 | Bank transfer/VietQR | payment_reference, accounting queue | Không PAID thiếu reconcile | PaymentReconciliationGuard |
| 085 | Usage/serving | Bữa ăn thực dưỡng, dùng đều đặn | Không nói liều dùng/hiệu quả X ngày | UsageContextResolver |
| 086 | Body feeling/weight/heatiness | Trả lời theo khẩu phần/thành phần | Không claim giảm/tăng cân | WeightConcernGuard |
| 087 | Storage/shelf-life | Bảo quản khô ráo, lot/HSD từ data | Không bịa HSD | ShelfLifeResolver |
| 088 | Preparation/serving | Theo hướng dẫn sản phẩm | Không tự tạo công thức pha | ProductUsageDataResolver |
| 089 | Taste objection/product switch | Ghi nhận, gợi ý SKU thay thế đúng fit | Không phản bác/đổ lỗi | TasteFeedbackResolver |
| 090 | Trust/price objection | Giải thích giá trị, khoa học, nguyên liệu | Không tự giảm giá | PriceObjectionResolver |
| 091 | Return/refund/product issue | Open case, ảnh/mã đơn/mã lô | Không kết luận lỗi SX khi chưa QA | ReturnRefundResolver |
| 092 | Order edit/address/cancel | Check order state, revalidate quote | Không sửa order locked sai policy | OrderEditEligibilityResolver |
| 093 | VAT/company payment | Thu thập đủ MST/company/email/order | Không xuất thiếu data | TaxInfoValidationGuard |
| 094 | Bulk/corporate gift | Tách bulk gift với distributor | Không tự báo chiết khấu | BulkBuyIntentResolver |
| 095 | Privacy/data request | Opt-out, privacy case, suppress CRM | Không public PII | PrivacyRequestResolver |
| 096 | Official channel/fake goods | Trace QR, CSKH/legal route | Không kết luận khi thiếu evidence | CounterfeitSuspicionGuard |

## 6. Product recommendation priority

Thứ tự ưu tiên:

1. Customer safety:
   - allergy.
   - vegan.
   - health-sensitive.
2. Sellable status:
   - sellable.
   - stock > 0.
   - no quality hold.
   - no recall hold.
   - no sale lock.
3. Customer fit:
   - need.
   - recipient.
   - taste.
   - previous feedback.
4. Pillar balance:
   - seasonal.
   - functional.
   - nourishing.
   - family add-on.
5. Inventory priority:
   - owner priority.
   - high stock.
   - campaign priority.
6. Rotation:
   - tránh lặp SKU quá thường xuyên.
   - tôn trọng feedback trước.

Inventory priority không thắng safety/dietary/health.

## 7. Product pillar rule

20 SKU phải có mapping canonical:

- 5 sản phẩm theo mùa.
- 6 sản phẩm chức năng.
- 9 sản phẩm bổ dưỡng.
- 4 SKU thuần chay.
- 16 SKU mặn.

AI không tự thêm SKU, không đổi pillar, không chọn SKU mặn cho khách thuần chay, không ép đủ 3 trụ nếu pillar không có SKU sellable phù hợp.

## 8. Public/private rule

Public comment chỉ được:

- Chào/handoff.
- Kéo Messenger/private nếu intent đủ điều kiện.
- Không xin PII.
- Không báo final quote cá nhân.
- Không chốt đơn.
- Không nói payment/order/private data.

Troll/abuse/malicious không kéo Messenger như lead bán hàng.

Complaint thật phải route CSKH, không xử như troll.

## 9. Health/safety wording

Được nói:

- Bồi bổ thể trạng.
- Hỗ trợ phục hồi thể trạng theo hướng thực dưỡng.
- Kiện tỳ, dưỡng khí, bổ huyết, dưỡng âm, ôn trung nếu claim whitelist cho phép.
- Bữa ăn tiện dùng.

Không được nói:

- Chữa bệnh.
- Thay thuốc.
- Cam kết khỏi.
- Hiệu quả y khoa.
- Ngưng thuốc.
- Quote/order khi kiêng cữ/dị ứng chưa rõ.

## 10. Trust/price objection wording

Khi khách hỏi "sao đắt", "có thật không", "lừa đảo không":

- Không công kích khách.
- Không phòng thủ.
- Giải thích bằng khoa học, Sâm Savigin, nguyên liệu, sấy thăng hoa, y thực đồng nguyên.
- Nếu có bằng chứng/mã đơn/khiếu nại, route complaint/CSKH.
- Không tự giảm giá ngoài runtime.

## 11. Resolver/Guard matrix

| Nhóm | Bắt buộc |
| --- | --- |
| Identity/context | CustomerIdentityResolver, CustomerMemoryResolver, LastPurchaseResolver |
| Product | ProductKnowledgeResolver, ProductEffectivenessResolver, ProductPillarResolver, VeganDietaryGuard |
| Quote/order | QuoteEngine, QuoteCompletenessGuard, OrderDraftPrefillFormGuard |
| Safety | ClaimGuard, HealthSafetyGuard, ConditionSensitiveClaimGuard, FinalResponseGuard |
| Public/private | PublicPrivateBoundaryGuard, MessengerHandoffGuard |
| Moderation | LiveAbuseClassifier, ProfanityNormalizer, BrandAttackClassifier, TrollSpamBurstGuard |
| CRM | MessageTextImmutableGuard, CRMOpenCaseSuppressionGuard, CrossChannelDedupGuard |
| Cases | ReturnRefundResolver, PrivacyRequestResolver, CounterfeitSuspicionGuard |
| Payment/shipping | PaymentReconciliationGuard, ShippingResolver, DeliveryTrackingResolver |

## 12. Situation registry tối thiểu

Phải có situation ID cho:

- Live low-intent engagement.
- Public comment to private.
- Multi-SKU price inquiry.
- Returning customer care.
- Diamond 24/7 buyer benefit.
- Diamond quote summary.
- Sâm Savigin science/contact/visit/agency.
- Live abuse/no reply.
- Affiliate new/member/Diamond.
- CRM Golden Hour.
- CRM Diamond referral/commission.
- CRM lifecycle maintain/upgrade/grace/downgrade.
- Health disease/recovery.
- Gift/corporate/international.
- Office/student/institution.
- Overseas/hometaste.
- Vegan/pillar.
- Inventory priority.
- IVR high-risk.
- Bank transfer.
- Shipping ETA.
- Usage/storage/preparation.
- Taste objection.
- Trust/price objection.
- Return/refund.
- Order edit/cancel.
- VAT/company.
- Privacy.
- Fake goods.

Mỗi situation map:

`intent -> resolver -> guard -> action -> content/template -> smoke -> evidence`.

## 13. P0 blocker

- AI tự tính giá/quyền lợi/hoa hồng.
- AI dùng từ "hệ thống" customer-facing.
- AI quote thiếu QuoteSnapshot.
- AI order thiếu draft/confirmation.
- AI claim chữa bệnh/thay thuốc.
- AI gợi ý SKU non-sellable.
- AI bỏ qua vegan/health/allergy.
- AI public final quote/PII.
- AI kéo troll/malicious vào Messenger.
- AI tiếp tục bán khi complaint/privacy/refund/payment/health open.
- AI rewrite message locked text.

## 14. Smoke tối thiểu

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P3_1-2E-001 | Khách cũ hỏi mua tiếp | Hỏi thăm trải nghiệm trước khi bán. |
| P3_1-2E-002 | Public comment hỏi giá | Route private, không public final quote. |
| P3_1-2E-003 | Khách hỏi tiểu đường/huyết áp | Health guard, không claim chữa bệnh. |
| P3_1-2E-004 | Khách ăn chay | Chỉ gợi ý SKU vegan sellable. |
| P3_1-2E-005 | SKU high stock nhưng không fit | Không gợi ý vì inventory không thắng fit. |
| P3_1-2E-006 | Troll/chửi thề | Hide/no reply/no Messenger nếu malicious. |
| P3_1-2E-007 | Complaint rách gói | Open case, không bán tiếp. |
| P3_1-2E-008 | Order edit sau bàn giao | Route delivery support. |
| P3_1-2E-009 | Privacy opt-out | Suppress CRM. |
| P3_1-2E-010 | Fake goods/QR lạ | Trace/CSKH/legal, không kết luận thiếu evidence. |

## 15. Evidence bắt buộc

- Situation registry.
- Resolver trace.
- Guard trace.
- Final response sample.
- Product recommendation decision.
- QuoteSnapshot ref nếu có giá.
- Case/suppression log nếu có open case.
- Smoke conversation evidence.

## 16. Done gate

Workstream 06 DONE khi:

- CORE-01 đến CORE-20 đã map.
- AI-TEST-NOTE 001-063 đã gom.
- AI-TEST-NOTE 076-096 đã gom.
- Product recommendation priority đã khóa.
- Situation registry có đủ các nhóm.
- P0 smoke có evidence.
- Không release claim.

## 17. Final status

`P3.1-2E HOÀN TẤT BÀN GIAO - CHỜ TRIỂN KHAI/BẰNG CHỨNG - GATEWAY BLOCKED`
