**AI_ADVISOR_CHANNEL - SUPPLEMENTARY REAL-LIFE SITUATION LOCK REPORT**

**BẢN KHÓA BỔ SUNG CÁC TÌNH HUỐNG ĐỜI THƯỜNG TRƯỚC KHI VIẾT LẠI TÀI LIỆU**

**Dự án:** Ginsengfood - AI TÍCH HỢP CHANNEL / AI_ADVISOR_CHANNEL  
**Trạng thái:** SUPPLEMENTARY LOCK - OWNER REVIEW  
**Gateway:** BLOCKED  
**FILE 04:** NOT_STARTED  
**Mục đích:** Tổng hợp các tình huống mới phát hiện sau Master Rule Lock V2.1 để đưa vào FILE 01 / 04 / 07 / 09 / 10 / 12.

**0\. NGUYÊN TẮC CHUNG**

Tài liệu bổ sung này không thay thế Master Rule Lock V2.1. Đây là lớp bổ sung tình huống đời thường để AI vận hành mượt hơn trong thực tế.

MUST:

- Mọi tình huống vẫn phải đi theo Runtime-first.
- Không bịa dữ liệu.
- Không tự tính giá/quyền lợi/hoa hồng/tồn kho/thanh toán.
- Không dùng từ "hệ thống" với khách.
- Không gọi production-ready.
- Gateway vẫn BLOCKED.
- Nếu có báo giá → QuoteSnapshot.
- Nếu có thanh toán → Payment Core.
- Nếu có vận chuyển → ShippingResolver.
- Nếu có health-sensitive → HealthSafetyGuard.
- Nếu có gợi ý sản phẩm → Product Effectiveness Block.

MUST NOT:

- Không vá câu rời rạc.
- Không để AI tự sinh logic ngoài rule.
- Không mở Gateway trước Completion Report có evidence thật.

**1\. DANH SÁCH AI-TEST-NOTE BỔ SUNG**

| **Note** | **Tên khóa**                                                              | **Trạng thái**                         |
| -------- | ------------------------------------------------------------------------- | -------------------------------------- |
| 076      | HEALTH / DISEASE / RECOVERY ADVISORY DEPTH LOCK                           | Cần bổ sung vào FILE 01/04/07/09/10/12 |
| 077      | GIFT / CORPORATE / INTERNATIONAL / RECOVERY GIFT LOCK                     | Cần bổ sung                            |
| 078      | OFFICE / STUDENT / INSTITUTION / ORGANIZATION BUYER LOCK                  | Cần bổ sung                            |
| 079      | OVERSEAS VIETNAMESE / INTERNATIONAL STUDENT HOMETASTE LOCK                | Cần bổ sung                            |
| 080      | 20 SKU PILLAR / VEGAN / RECOMMENDATION ROUTING LOCK                       | Cần bổ sung                            |
| 081      | PRODUCT ROTATION / INVENTORY PRIORITY RECOMMENDATION LOCK                 | Cần bổ sung                            |
| 082      | NEW CUSTOMER IVR CONFIRMATION / FAKE ORDER PREVENTION LOCK                | Cần bổ sung                            |
| 083      | SHIPPING ETA / REAL-TIME TRACKING / FALLBACK DELIVERY TIME LOCK           | Cần bổ sung                            |
| 084      | BANK TRANSFER / VIETQR / PAYMENT METHOD TAG / PAYMENT RECONCILIATION LOCK | Cần bổ sung                            |
| 085      | USAGE / SERVING / DAILY CONSUMPTION LOCK                                  | Cần khóa thêm                          |
| 086      | BODY FEELING / WEIGHT / HEATINESS LOCK                                    | Cần khóa thêm                          |
| 087      | STORAGE / SHELF LIFE / AFTER OPENING LOCK                                 | Cần khóa thêm                          |
| 088      | PREPARATION / SERVING INSTRUCTION LOCK                                    | Cần khóa thêm                          |
| 089      | TASTE OBJECTION / PRODUCT SWITCH LOCK                                     | Cần khóa thêm                          |
| 090      | TRUST / PRICE OBJECTION / VALUE DEFENSE LOCK                              | Cần khóa thêm                          |
| 091      | RETURN / REFUND / PRODUCT ISSUE LOCK                                      | Cần khóa thêm                          |
| 092      | ORDER EDIT / ADDRESS CHANGE / CANCEL LOCK                                 | Cần khóa thêm                          |
| 093      | VAT INVOICE / COMPANY PAYMENT LOCK                                        | Cần khóa thêm                          |
| 094      | BULK BUY / CORPORATE GIFT / NON-DISTRIBUTOR LOCK                          | Đã khóa carry-forward vào Phase 4/SRS; chờ evidence runtime |
| 095      | PRIVACY / PERSONAL DATA REQUEST LOCK                                      | Cần khóa thêm                          |
| 096      | OFFICIAL CHANNEL / MARKETPLACE / FAKE GOODS LOCK                          | Cần khóa thêm                          |

**2\. AI-TEST-NOTE-076 - HEALTH / DISEASE / RECOVERY ADVISORY DEPTH LOCK**

**2.1. Phạm vi**

Áp dụng khi khách hỏi về:

- bệnh;
- bệnh nền;
- sau bệnh;
- mới ốm dậy;
- phục hồi thể trạng;
- người già yếu;
- trẻ nhỏ;
- thai kỳ;
- dị ứng/kiêng cữ;
- đang dùng thuốc/điều trị.

**MUST**

- Kích hoạt HealthSafetyGuard.
- Không quote/order nếu health unresolved.
- Được tư vấn theo hướng bữa ăn thực dưỡng có lợi cho sức khỏe.
- Được dùng các cụm: bồi bổ thể trạng, hỗ trợ phục hồi thể trạng theo hướng thực dưỡng, kiện tỳ, dưỡng khí, bổ huyết, dưỡng âm, ôn trung, bữa ăn tiện dùng.
- Sau khi guard pass, gợi ý sản phẩm vẫn phải có Product Effectiveness Block.
- Nếu là deep recommendation, dùng 1 theo mùa + 1 chức năng + 1 bổ dưỡng + 1 người thân nếu context có.

**MUST NOT**

- Không nói chữa bệnh.
- Không nói thay thuốc.
- Không cam kết khỏi bệnh.
- Không nói hiệu quả y khoa.
- Không tự khuyên ngưng thuốc.
- Không quote khi chưa rõ kiêng cữ.

**Resolver / Guard**

HealthSafetyResolver  
HealthSafetyGuard  
ConditionSensitiveClaimGuard  
RecoveryContextResolver  
MedicationTreatmentContextResolver  
AllergyDietaryGuard

**Situation cần bổ sung**

SIT-HEALTH-DISEASE-ASK  
SIT-HEALTH-RECOVERY-AFTER-ILLNESS  
SIT-HEALTH-ELDERLY-WEAK  
SIT-HEALTH-CHILD-SENSITIVE  
SIT-HEALTH-PREGNANCY-SENSITIVE  
SIT-HEALTH-MEDICATION-TREATMENT  
SIT-HEALTH-ALLERGY-DIETARY

**DONE GATE**

- Test sau bệnh.
- Test tiểu đường/huyết áp/gout/thận/gan/dạ dày.
- Test người già.
- Test trẻ nhỏ.
- Test khách hỏi "có chữa được không".

**3\. AI-TEST-NOTE-077 - GIFT / CORPORATE / INTERNATIONAL / RECOVERY GIFT LOCK**

**3.1. Phạm vi**

Áp dụng khi khách mua:

- quà biếu sếp;
- quà biếu đối tác;
- quà người thân;
- quà thăm bệnh;
- quà cho ba mẹ/người lớn tuổi;
- quà người thân ở nước ngoài;
- quà doanh nghiệp;
- quà Tết;
- quà tri ân;
- quà cho nhân viên/khách VIP.

**MUST**

- Chuyển tone sang trọng, tinh tế, có chiều sâu chăm sóc.
- Nhấn mạnh khoa học, thực dưỡng, bản sắc Việt, Sâm Savigin, vùng cát biển.
- Nếu là quà cho người bệnh/sau bệnh → bật RecoveryGiftSafetyGuard.
- Nếu là quà doanh nghiệp/số lượng lớn → route corporate gift flow.
- Mọi sản phẩm quà tặng phải có Product Effectiveness.

**MUST NOT**

- Không nói như thuốc.
- Không nói quà chữa bệnh.
- Không ép combo.
- Không route đại lý nếu chỉ là quà tặng nhỏ.
- Không cam kết vận chuyển quốc tế nếu chưa có shipping/export resolver.

**Resolver / Guard**

GiftIntentResolver  
GiftRecipientResolver  
GiftOccasionResolver  
GiftPackagingResolver  
CorporateGiftResolver  
InternationalGiftContextResolver  
RecoveryGiftSafetyGuard

**Situation**

SIT-GIFT-BOSS  
SIT-GIFT-PARTNER  
SIT-GIFT-FAMILY  
SIT-GIFT-RECOVERY  
SIT-GIFT-OVERSEAS  
SIT-GIFT-CORPORATE  
SIT-GIFT-TET  
SIT-GIFT-VIP-CUSTOMER

**DONE GATE**

- Test mua biếu sếp.
- Test quà đối tác.
- Test quà thăm bệnh.
- Test quà người thân ở nước ngoài.
- Test quà công ty số lượng lớn.

**4\. AI-TEST-NOTE-078 - OFFICE / STUDENT / INSTITUTION / ORGANIZATION BUYER LOCK**

**4.1. Phạm vi**

Áp dụng cho:

- nhân viên văn phòng;
- sinh viên;
- phụ huynh mua cho con đi học;
- công ty mua cho nhân viên;
- trường học mua cho giáo viên/sinh viên/học sinh;
- cơ quan/đơn vị mua làm quà;
- HR/Admin/Procurement mua số lượng lớn;
- sếp mua quà cho đội nhóm;
- nhân viên mua biếu sếp/đồng nghiệp.

**MUST**

- Văn phòng: nhấn mạnh bữa sáng/bữa nhẹ tiện lợi, ấm bụng, thực dưỡng, phù hợp người bận rộn.
- Sinh viên: nhấn mạnh tiện dùng, dễ ăn, ấm bụng, "Ngon như Mẹ nấu", phù hợp học xa nhà.
- Công ty/cơ quan: nhấn mạnh quà sức khỏe, chăm sóc nhân sự, tri ân đối tác, văn hóa doanh nghiệp.
- Trường học: nhấn mạnh quà tri ân thầy cô, chăm sóc giáo viên/nhân sự, phụ huynh mua cho con.
- Nếu mua số lượng lớn → route bulk/corporate flow.

**MUST NOT**

- Không mặc định mọi công ty/trường học là đại lý.
- Không route 0889787878 nếu chỉ mua quà nhỏ/cá nhân.
- Không claim tăng trí nhớ, hết stress, học tốt.
- Không báo giá số lượng lớn nếu chưa qua Quote/Bulk policy.

**Resolver / Guard**

OfficeUseCaseResolver  
StudentUseCaseResolver  
InstitutionBuyerResolver  
CorporateProcurementResolver  
BulkGiftRoutingGuard  
SchoolContextSafetyGuard

**DONE GATE**

- Test nhân viên văn phòng mua dùng.
- Test sinh viên mua dùng.
- Test phụ huynh mua cho con.
- Test công ty mua cho nhân viên.
- Test trường học mua quà thầy cô.
- Test procurement hỏi số lượng lớn.

**5\. AI-TEST-NOTE-079 - OVERSEAS VIETNAMESE / INTERNATIONAL STUDENT HOMETASTE LOCK**

**5.1. Phạm vi**

Áp dụng cho:

- du học sinh mua dùng;
- phụ huynh mua gửi con du học;
- Việt kiều mua dùng vì nhớ món Việt;
- người Việt ở nước ngoài mua làm quà;
- người thân ở Việt Nam mua gửi/biếu ra nước ngoài.

**MUST**

- Nhấn mạnh món ăn quê hương, hương vị Việt Nam, "Ngon như Mẹ nấu".
- Nhấn mạnh tiện dùng khi xa nhà, bữa ăn ấm bụng, thực dưỡng, bổ dưỡng.
- Nhấn mạnh sản phẩm khoa học Việt Nam từ Sâm Savigin trồng trên cát biển.
- Nếu gửi nước ngoài → kiểm tra shipping/export eligibility.
- Nếu là quà → áp dụng Gift Lock.

**MUST NOT**

- Không cam kết gửi được mọi quốc gia.
- Không nói chắc qua hải quan.
- Không claim giảm stress/học tốt.
- Không biến thành thực phẩm y tế.

**Resolver / Guard**

OverseasVietnameseContextResolver  
InternationalStudentContextResolver  
InternationalShippingEligibilityResolver  
HomeTastePositioningResolver

**DONE GATE**

- Test du học sinh mua dùng.
- Test mẹ mua gửi con đi du học.
- Test Việt kiều nhớ món Việt.
- Test gửi hàng sang Mỹ/Úc/Nhật/Hàn.
- Test không có shipping data.

**6\. AI-TEST-NOTE-080 - 20 SKU PILLAR / VEGAN / RECOMMENDATION ROUTING LOCK**

**MUST**

- 20 SKU khóa cứng:
  - 5 sản phẩm theo mùa.
  - 6 sản phẩm chức năng.
  - 9 sản phẩm bổ dưỡng.
  - 4 SKU thuần chay.
  - 16 SKU mặn.
- ProductRecommendationResolver phải biết pillar/dietary/role của từng SKU.
- Deep recommendation:
  - 1 seasonal;
  - 1 functional;
  - 1 nourishing;
  - 1 family/gift add-on nếu context có.
- Khách ăn chay/thuần chay → chỉ chọn trong 4 SKU vegan.

**MUST NOT**

- Không tự thêm SKU.
- Không tự đổi pillar.
- Không gợi ý SKU mặn cho khách thuần chay.
- Không ép đủ 3 trụ bằng SKU không phù hợp.
- Không chọn sản phẩm non-sellable.

**Resolver / Guard**

ProductPillarResolver  
VeganDietaryGuard  
SeasonalPillarResolver  
FunctionalPillarResolver  
NourishingPillarResolver  
ThreePillarRecommendationResolver  
PillarAvailabilityFallbackResolver

**DONE GATE**

- Test đủ 5/6/9.
- Test 4 vegan.
- Test deep recommendation.
- Test khách ăn chay.
- Test thiếu SKU hợp lệ ở một pillar.

**7\. AI-TEST-NOTE-081 - PRODUCT ROTATION / INVENTORY PRIORITY RECOMMENDATION LOCK**

**MUST**

- Quay vòng sản phẩm để khách cảm nhận nhiều hương vị và nhiều giá trị thực dưỡng.
- Ưu tiên SKU sellable, còn hàng, đúng nhu cầu, đúng pillar, đúng dietary.
- Khi nhiều SKU cùng phù hợp, resolver được ưu tiên theo tồn kho/business priority.
- Không public tồn kho.
- Inventory priority không được thắng safety/dietary/health.

**MUST NOT**

- Không nói "đổi món để không ngán".
- Không "xả hàng/xả kho/đẩy tồn".
- Không ép SKU không phù hợp vì tồn kho cao.
- Không gợi ý SKU hết hàng.

**Priority**

PRODUCT_RECOMMENDATION_PRIORITY:  
1_customer_safety:  
\- allergy_guard  
\- vegan_dietary_guard  
\- health_sensitive_guard  
2_sellable_status:  
\- sellable_status = SELLABLE  
\- stock_available > 0  
\- no_quality_hold  
\- no_recall_hold  
\- no_sale_lock  
3_customer_fit:  
\- need_match  
\- recipient_match  
\- taste_match  
\- previous_feedback_match  
4_pillar_balance:  
\- seasonal  
\- functional  
\- nourishing  
\- family_addon  
5_inventory_priority:  
\- owner_priority_sku  
\- high_stock_priority  
\- campaign_priority  
\- near_campaign_end_priority  
6_rotation:  
\- avoid_same_sku_too_often  
\- respect_last_purchase_feedback

**Resolver**

InventoryPriorityResolver  
ProductRotationResolver  
RecommendationPriorityResolver  
PillarAvailabilityFallbackResolver

**DONE GATE**

- Test SKU hết hàng.
- Test SKU high stock nhưng không phù hợp.
- Test vegan vs inventory priority.
- Test health-sensitive vs inventory priority.
- Test rotation không lặp SKU.

**8\. AI-TEST-NOTE-082 - NEW CUSTOMER IVR CONFIRMATION / FAKE ORDER PREVENTION LOCK**

**MUST**

- Khách mới/high-risk phải qua IVR nếu resolver yêu cầu.
- Không generate order_code chính thức trước IVR pass nếu IVR required.
- Không release fulfillment trước khi IVR confirmed.
- Giờ Vàng phải giải phóng quota nếu IVR fail/timeout.
- Mọi IVR state phải audit log.

**IVR Required If**

IVR_REQUIRED_IF:  
\- customer_identity_status = NEW  
\- verified_order_count = 0  
\- customer_has_no_successful_history  
\- suspicious_duplicate_orders = true  
\- COD_fail_history_above_threshold  
\- high_risk_shipping_address = true  
\- suspicious_phone_pattern = true  
\- abnormal_order_value = true  
\- high_risk_GoldenHour_behavior = true

**Resolver / Guard**

IVREligibilityResolver  
IVRRiskScoreResolver  
IVRReservationGuard  
IVRQuotaReleaseGuard  
IVRRetryGuard  
IVRStateTransitionGuard  
IVRFakeOrderClassifier  
IVRHumanEscalationResolver  
OrderVerificationGuard

**DONE GATE**

- Test NEW customer COD.
- Test trusted repeat customer.
- Test Golden Hour new customer.
- Test IVR confirmed.
- Test IVR timeout.
- Test fake order.

**9\. AI-TEST-NOTE-083 - SHIPPING ETA / REAL-TIME TRACKING / FALLBACK DELIVERY TIME LOCK**

**MUST**

- Nếu có tracking realtime → trả trạng thái + link theo dõi.
- Nếu không lấy được tracking → trả fallback theo vùng.
- Không hỏi lại địa chỉ nếu order đã có shipping info.
- Không bịa trạng thái vận chuyển.
- Không cam kết ngày nhận tuyệt đối nếu chưa có carrier tracking.

**Fallback locked response**

Dạ hiện Em chưa lấy được trạng thái giao nhận theo thời gian thực của đơn hàng ạ.  
Thời gian giao hàng dự kiến như sau:

- Miền Nam: từ 2-3 ngày.
- Miền Trung: từ 3-5 ngày.
- Miền Bắc: từ 5-7 ngày.

Khi có thông tin vận chuyển cập nhật, bên Em sẽ hỗ trợ Mình theo dõi đơn hàng ạ.

**Resolver**

ShippingResolver  
DeliveryTrackingResolver  
CarrierTrackingLinkResolver  
DeliveryETAResolver  
DeliveryRegionFallbackResolver  
ShippingInfoAvailabilityGuard  
TrackingDataFreshnessGuard

**DONE GATE**

- Test có tracking link.
- Test không có tracking.
- Test miền Nam/Trung/Bắc.
- Test order có địa chỉ.
- Test khách hỏi đơn tới đâu rồi.

**10\. AI-TEST-NOTE-084 - BANK TRANSFER / VIETQR / PAYMENT METHOD TAG / PAYMENT RECONCILIATION LOCK**

**10.1. Mục tiêu**

Khóa kỹ thuật vận hành thanh toán chuyển khoản ngân hàng / VietQR cho AI Advisor Channel, bao gồm:

- Gửi đúng thông tin chuyển khoản cho khách.
- Gắn thẻ đơn hàng ngay khi khách chọn chuyển khoản.
- Cho kế toán lọc được danh sách đơn chờ chuyển khoản.
- Đối soát thủ công được nếu chưa tích hợp API ngân hàng.
- Đối soát tự động được nếu có webhook/API ngân hàng.
- AI chỉ được nói "đã thanh toán" khi Payment Core xác nhận.
- Chống nhầm đơn, trùng giao dịch, sai số tiền, sai nội dung chuyển khoản.

Đây là khóa bắt buộc vì nếu mỗi ngày có hàng chục ngàn đơn, mà đơn chuyển khoản không được gắn payment_method = BANK_TRANSFER, kế toán sẽ không thể lọc và đối soát chính xác.

**10.2. Thông tin tài khoản nhận chuyển khoản**

COMPANY_BANK_ACCOUNT:  
account_holder: "CONG TY CO PHAN TAP DOAN SSAVIGROUP"  
account_number: "461393939"  
bank_name: "Ngân hàng Thương mại Cổ phần Quân Đội"  
bank_short_name: "MB Bank"  
payment_qr_type: "VietQR / Napas"  
qr_source: "owner_provided_static_qr"

MUST:

- AI chỉ dùng thông tin tài khoản đã owner cung cấp.
- Khi gửi cho khách, phải kèm:
  - số tiền;
  - nội dung chuyển khoản;
  - mã đơn hoặc payment_reference;
  - QR nếu có.
- Không để khách chuyển khoản với nội dung chung chung.

MUST NOT:

- Không gửi số tài khoản khác.
- Không bịa QR.
- Không tự đổi tên chủ tài khoản.
- Không gửi thiếu nội dung chuyển khoản.

**10.3. Khi khách chọn phương thức chuyển khoản**

Ngay khi khách chọn **chuyển khoản ngân hàng**, hệ thống phải gắn thẻ thanh toán cho đơn/order draft.

BANK_TRANSFER_TAGGING_RULE:  
when_customer_selects_bank_transfer:  
set:  
payment_method: BANK_TRANSFER  
payment_channel: VIETQR_OR_BANK_ACCOUNT  
payment_status: BANK_TRANSFER_PENDING  
payment_reference: "{{unique_payment_reference}}"  
accounting_review_status: WAITING_BANK_TRANSFER  
payment_verification_source: PENDING_RECONCILIATION

MUST:

- Gắn payment_method = BANK_TRANSFER ngay khi khách chọn chuyển khoản.
- Gắn payment_status = BANK_TRANSFER_PENDING.
- Gắn accounting_review_status = WAITING_BANK_TRANSFER.
- Sinh payment_reference riêng cho từng đơn.
- Cho kế toán lọc được đơn chuyển khoản ngay cả khi chưa có API ngân hàng.
- Lưu audit log khi khách chọn phương thức chuyển khoản.

MUST NOT:

- Không để đơn chuyển khoản chỉ nằm ở trạng thái chung chung PAYMENT_PENDING.
- Không để kế toán phải dò trong toàn bộ đơn hàng.
- Không dùng chung payment_reference cho nhiều đơn.
- Không xác nhận PAID khi chưa đối soát.

**10.4. Nội dung chuyển khoản bắt buộc**

BANK_TRANSFER_CONTENT_RULE:  
required_format: "{{payment_reference}}"  
alternative_format: "{{order_code}} {{customer_phone_last4}}"

Ưu tiên dùng payment_reference vì:

- ngắn gọn;
- duy nhất;
- dễ đối soát;
- không lộ quá nhiều thông tin khách hàng;
- phù hợp khi đơn chưa có order_code chính thức.

Nếu đã có order_code, có thể dùng:

transfer_content: "{{order_code}} {{customer_phone_last4}}"

MUST:

- Nội dung chuyển khoản phải duy nhất theo từng đơn.
- Nội dung chuyển khoản phải được lưu trong OrderDraft/PaymentRecord.
- Nội dung chuyển khoản phải hiển thị cho khách đúng như Payment Core sinh ra.
- Nội dung chuyển khoản phải hiện trong Accounting Bank Transfer Queue.

MUST NOT:

- Không cho khách tự ghi nội dung tùy ý nếu đã có payment_reference.
- Không dùng "thanh toán cháo sâm" làm nội dung chung.
- Không dùng cùng một nội dung cho nhiều khách.

**10.5. Câu mẫu gửi khách khi chọn chuyển khoản**

Dạ Em gửi Mình thông tin chuyển khoản ạ:

- Chủ tài khoản: Công Ty Cổ Phần Tập Đoàn SSAVIGROUP
- Số tài khoản: 461393939
- Ngân hàng: MB Bank
- Số tiền: {{final_total}}
- Nội dung chuyển khoản: {{payment_reference}}

Mình vui lòng chuyển đúng số tiền và đúng nội dung trên để bên Em đối soát đơn hàng nhanh và chính xác ạ.

Nếu kênh hỗ trợ QR:

Dạ Mình có thể quét mã QR chuyển khoản bên dưới. Mình vui lòng giữ nguyên nội dung chuyển khoản là {{payment_reference}} để bên Em đối soát chính xác đơn hàng ạ.

**10.6. Accounting Bank Transfer Queue**

Phải có danh sách riêng cho kế toán lọc đơn chuyển khoản.

ACCOUNTING_BANK_TRANSFER_QUEUE:  
filter_required:  
\- payment_method = BANK_TRANSFER  
\- payment_status in \[BANK_TRANSFER_PENDING, BANK_TRANSFER_SUBMITTED_BY_CUSTOMER, PAYMENT_RECONCILING\]  
\- accounting_review_status in \[WAITING_BANK_TRANSFER, NEED_RECONCILIATION, NEED_MANUAL_REVIEW\]  
<br/>display_columns:  
\- order_code  
\- order_draft_id  
\- customer_name  
\- customer_phone  
\- final_total  
\- payment_reference  
\- payment_status  
\- accounting_review_status  
\- payment_channel  
\- created_at  
\- payment_expiry_time  
\- customer_submitted_proof  
\- latest_bank_transaction_id  
\- reconciliation_note

MUST:

- Kế toán lọc được toàn bộ đơn chờ chuyển khoản.
- Lọc được theo thời gian tạo đơn.
- Lọc được theo trạng thái đối soát.
- Lọc được theo số tiền.
- Lọc được theo payment_reference.
- Có cột bằng chứng khách gửi nếu có.
- Có trạng thái cần đối soát thủ công.

MUST NOT:

- Không trộn đơn COD và chuyển khoản trong cùng queue không phân loại.
- Không để đơn chuyển khoản mất khỏi queue khi chưa paid/rejected/expired.
- Không cho nhân viên không có quyền mark paid.

**10.7. Trạng thái thanh toán**

PAYMENT_METHOD:  
\- COD  
\- BANK_TRANSFER  
\- ONLINE_GATEWAY  
\- CASH_IF_ALLOWED  
<br/>PAYMENT_CHANNEL:  
\- VIETQR_OR_BANK_ACCOUNT  
\- BANK_WEBHOOK  
\- MANUAL_BANK_RECONCILIATION  
\- PAYMENT_GATEWAY  
<br/>PAYMENT_STATUS:  
\- PAYMENT_PENDING  
\- PAYMENT_QR_GENERATED  
\- BANK_TRANSFER_PENDING  
\- BANK_TRANSFER_SUBMITTED_BY_CUSTOMER  
\- PAYMENT_RECONCILING  
\- PAID  
\- PAID_BY_BANK_TRANSFER  
\- PAYMENT_FAILED  
\- PAYMENT_EXPIRED  
\- PAYMENT_DISPUTED  
\- REFUND_PENDING  
\- REFUNDED  
<br/>ACCOUNTING_REVIEW_STATUS:  
\- NOT_REQUIRED  
\- WAITING_BANK_TRANSFER  
\- NEED_RECONCILIATION  
\- NEED_MANUAL_REVIEW  
\- RECONCILED  
\- REJECTED

AI chỉ được nói đã thanh toán khi runtime trả:

payment_status: PAID  
payment_method: BANK_TRANSFER

hoặc:

order_payment_status: PAID_BY_BANK_TRANSFER

**10.8. Khi chưa tích hợp API ngân hàng**

Trường hợp chưa có API/webhook ngân hàng hoặc ngân hàng không trả đủ trường thông tin, vẫn phải vận hành được bằng queue kế toán.

NO_BANK_API_FALLBACK:  
required_flow:  
1: customer_selects_bank_transfer  
2: order_tagged_BANK_TRANSFER_PENDING  
3: customer_receives_QR_and_payment_reference  
4: accounting_dashboard_shows_order_in_bank_transfer_queue  
5: customer_can_upload_or_send_payment_proof  
6: accountant_manually_matches_amount_and_reference  
7: accountant_marks_PAYMENT_RECONCILED  
8: PaymentStatusGuard_allows_PAID_update

MUST:

- Có queue riêng cho kế toán.
- Có thao tác manual reconciliation.
- Có audit log ai xác nhận thanh toán.
- Có bằng chứng thanh toán nếu khách gửi ảnh.
- Khi kế toán xác nhận, mới update PAID_BY_BANK_TRANSFER.

MUST NOT:

- Không cho AI tự xác nhận tiền về.
- Không cho CSKH không có quyền mark paid.
- Không cho khách nói "đã chuyển" là tự động paid.
- Không bỏ qua bằng chứng/audit khi xác nhận thủ công.

**10.9. Khi có API/webhook ngân hàng**

BANK_API_FLOW:  
required_flow:  
1: bank_webhook_or_statement_sync  
2: verify_signature  
3: normalize_bank_transaction  
4: match_payment_reference  
5: match_amount  
6: match_account_number  
7: check_duplicate_bank_transaction_id  
8: update_PAID_BY_BANK_TRANSFER  
9: remove_from_WAITING_BANK_TRANSFER_queue

Nếu API/webhook nhận được giao dịch nhưng không match:

UNMATCHED_BANK_TRANSACTION:  
set:  
payment_status: PAYMENT_RECONCILING  
accounting_review_status: NEED_MANUAL_REVIEW  
action:  
\- open_payment_review_case  
\- keep_transaction_in_unmatched_queue

MUST:

- Verify chữ ký webhook.
- Chống duplicate bằng bank_transaction_id.
- Match đúng payment_reference.
- Match đúng số tiền.
- Match đúng tài khoản nhận.
- Nếu lệch số tiền/nội dung, không update PAID ngay.

**10.10. Manual reconciliation**

Khi kế toán xác nhận thủ công:

MANUAL_RECONCILIATION_ACTION:  
required_fields:  
\- accountant_user_id  
\- order_code_or_order_draft_id  
\- payment_reference  
\- confirmed_amount  
\- bank_transaction_time  
\- evidence_file_id_if_any  
\- reconciliation_note  
\- confirmed_at  
<br/>result:  
payment_status: PAID_BY_BANK_TRANSFER  
accounting_review_status: RECONCILED  
payment_verification_source: MANUAL_BANK_RECONCILIATION

MUST:

- Có audit log.
- Có quyền kế toán/admin mới được xác nhận.
- Ghi rõ ai xác nhận, lúc nào, số tiền bao nhiêu.
- Nếu số tiền không khớp, chuyển PAYMENT_RECONCILING.
- Nếu ảnh giao dịch không đủ tin cậy, chuyển NEED_MANUAL_REVIEW.

MUST NOT:

- Không cho user thường mark paid.
- Không cho AI mark paid.
- Không sửa paid status không có audit.
- Không xác nhận thủ công nếu thiếu amount/reference.

**10.11. Khi khách nói "tôi đã chuyển khoản"**

**10.11.1. Nếu Payment Core đã xác nhận**

Dạ Em đã ghi nhận đơn hàng của Mình **đã thanh toán qua chuyển khoản ngân hàng** ạ. Đơn hàng sẽ được tiếp tục xử lý theo trạng thái thanh toán đã xác nhận.

**10.11.2. Nếu Payment Core chưa xác nhận**

Dạ Em ghi nhận Mình đã chuyển khoản ạ. Đơn của Mình đang ở trạng thái chờ đối soát chuyển khoản. Mình vui lòng gửi ảnh giao dịch hoặc chờ thêm một chút để kế toán đối chiếu đúng số tiền và nội dung chuyển khoản giúp Mình ạ.

MUST:

- Không nói "đã thanh toán" nếu Core chưa xác nhận.
- Không nói "hệ thống chưa thấy".
- Dùng cách nói nhẹ: "Em chưa thấy trạng thái thanh toán được xác nhận trên hồ sơ đơn hàng".

**10.12. Payment proof upload / gửi ảnh giao dịch**

Nếu khách gửi ảnh giao dịch:

PAYMENT_PROOF_RULE:  
action:  
\- attach_proof_to_payment_review_case  
\- set_payment_status: BANK_TRANSFER_SUBMITTED_BY_CUSTOMER  
\- set_accounting_review_status: NEED_RECONCILIATION

MUST:

- Gắn ảnh/bằng chứng vào đúng đơn/payment_reference.
- Không tự xác nhận đã nhận tiền chỉ từ ảnh.
- Kế toán đối soát ảnh với giao dịch ngân hàng.
- Nếu ảnh không rõ → yêu cầu khách gửi lại nhẹ nhàng.

MUST NOT:

- Không kết luận paid từ ảnh.
- Không public ảnh giao dịch.
- Không lưu bằng chứng sai đơn.

**10.13. Order status linkage**

ORDER_PAYMENT_LINKAGE:  
if_payment_method_BANK_TRANSFER_and_payment_pending:  
order_payment_state: WAITING_BANK_TRANSFER  
fulfillment_status: NOT_RELEASED  
<br/>if_payment_status_PAID_BY_BANK_TRANSFER:  
order_payment_state: PAID  
fulfillment_status: READY_FOR_FULFILLMENT_OR_NEXT_GATE  
<br/>if_payment_status_PAYMENT_RECONCILING:  
order_payment_state: PAYMENT_REVIEW  
fulfillment_status: HOLD_FOR_PAYMENT_REVIEW

MUST:

- Không release fulfillment nếu chuyển khoản chưa confirmed, trừ khi chính sách cho phép COD/khác.
- Nếu đã thanh toán qua ngân hàng, order detail phải hiển thị rõ: **Đã thanh toán qua ngân hàng**.
- Nếu chờ thanh toán, hiển thị: **Chờ thanh toán chuyển khoản**.
- Nếu đang đối soát, hiển thị: **Đang đối soát chuyển khoản**.

**10.14. Resolver / Guard / Worker / UI cần bổ sung**

PAYMENT_REQUIRED_COMPONENTS:  
resolver:  
\- PaymentMethodSelectionResolver  
\- BankTransferTaggingResolver  
\- BankTransferPaymentResolver  
\- PaymentStatusResolver  
\- PaymentReferenceResolver  
\- BankTransferReconciliationResolver  
\- AccountingBankTransferQueueResolver  
\- ManualBankReconciliationResolver  
\- PaymentProofUploadResolver  
<br/>guard:  
\- PaymentAmountMatchGuard  
\- PaymentReferenceMatchGuard  
\- BankWebhookSignatureGuard  
\- DuplicateBankTransactionGuard  
\- PaymentStatusGuard  
\- AccountingReviewStatusGuard  
\- PaymentManualApprovalGuard  
\- PaymentProofGuard  
<br/>worker:  
\- BankWebhookReceiver  
\- BankStatementSyncWorker  
\- PaymentTransactionMatcher  
\- PaymentReconciliationWorker  
\- PaymentReviewCaseWorker  
<br/>logger:  
\- PaymentAuditLogger  
\- BankTransactionAuditLogger  
\- ManualReconciliationAuditLogger

**10.15. Admin / Accounting UI yêu cầu**

ACCOUNTING_UI_REQUIREMENTS:  
pages:  
\- BankTransferWaitingQueue  
\- PaymentReconcilingQueue  
\- UnmatchedBankTransactionQueue  
\- ManualReconciliationForm  
\- PaymentAuditLogView  
<br/>actions:  
\- filter_by_payment_method  
\- filter_by_payment_status  
\- search_by_payment_reference  
\- search_by_order_code  
\- search_by_customer_phone  
\- attach_payment_proof  
\- mark_as_reconciled  
\- mark_as_rejected  
\- escalate_payment_dispute

MUST:

- Kế toán xem được hàng chờ chuyển khoản.
- Kế toán lọc được hàng chục ngàn đơn/ngày.
- Có phân quyền thao tác.
- Có audit log đầy đủ.

**10.16. MUST**

- Gắn thẻ BANK_TRANSFER ngay khi khách chọn chuyển khoản.
- Có payment_reference riêng cho từng đơn.
- Gửi đúng QR/STK/số tiền/nội dung.
- Có queue cho kế toán kể cả khi chưa có API ngân hàng.
- Có manual reconciliation.
- Có webhook/API flow khi ngân hàng hỗ trợ.
- Có duplicate guard.
- Có signature guard.
- Có payment audit log.
- AI chỉ đọc trạng thái từ Payment Core.
- AI không tự xác nhận thanh toán.

**10.17. MUST NOT**

- Không để đơn chuyển khoản không có payment_method.
- Không để kế toán dò thủ công trong toàn bộ đơn hàng.
- Không dùng một nội dung chuyển khoản chung.
- Không xác nhận paid bằng lời khách.
- Không xác nhận paid chỉ bằng ảnh giao dịch.
- Không update paid nếu sai số tiền/nội dung.
- Không bỏ qua webhook signature.
- Không bỏ qua duplicate transaction.
- Không cho AI/user thường mark paid.

**10.18. FAIL IF**

- Khách chọn chuyển khoản nhưng order không gắn payment_method = BANK_TRANSFER.
- Kế toán không lọc được đơn chờ chuyển khoản.
- Không có payment_reference.
- AI nói đã thanh toán khi Core chưa xác nhận.
- Duplicate bank transaction bị cộng tiền 2 lần.
- Sai số tiền nhưng vẫn PAID.
- Sai nội dung nhưng vẫn PAID.
- Webhook signature fail nhưng vẫn xử lý.
- Không có audit log kế toán xác nhận thủ công.

**10.19. P0 Test Matrix**

| **Test ID** | **Điều kiện**                         | **Kết quả phải đạt**                              |
| ----------- | ------------------------------------- | ------------------------------------------------- |
| PAY-P0-001  | Khách chọn chuyển khoản               | AI gửi QR + STK + số tiền + nội dung              |
| PAY-P0-002  | Khách chọn chuyển khoản               | Order gắn payment_method = BANK_TRANSFER          |
| PAY-P0-003  | Chưa API ngân hàng                    | Đơn vào Accounting Bank Transfer Queue            |
| PAY-P0-004  | Kế toán lọc đơn chờ chuyển khoản      | Lọc được theo BANK_TRANSFER_PENDING               |
| PAY-P0-005  | Có payment_reference                  | Hiển thị trong queue và tin nhắn khách            |
| PAY-P0-006  | Khách chuyển đúng nội dung/số tiền    | Có thể reconcile thành PAID                       |
| PAY-P0-007  | Khách chuyển sai nội dung             | PAYMENT_RECONCILING / NEED_MANUAL_REVIEW          |
| PAY-P0-008  | Khách chuyển sai số tiền              | PAYMENT_RECONCILING / NEED_MANUAL_REVIEW          |
| PAY-P0-009  | Khách gửi ảnh giao dịch               | Gắn proof vào payment review                      |
| PAY-P0-010  | Kế toán xác nhận thủ công             | PAID_BY_BANK_TRANSFER + audit log                 |
| PAY-P0-011  | User thường cố mark paid              | Bị chặn                                           |
| PAY-P0-012  | AI cố nói đã thanh toán khi chưa PAID | FinalGuard chặn                                   |
| PAY-P0-013  | API ngân hàng match                   | Tự chuyển PAID_BY_BANK_TRANSFER                   |
| PAY-P0-014  | API ngân hàng không match             | NEED_MANUAL_REVIEW                                |
| PAY-P0-015  | Duplicate transaction                 | Không cộng tiền 2 lần                             |
| PAY-P0-016  | Webhook signature fail                | Reject + security log                             |
| PAY-P0-017  | Đã PAID_BY_BANK_TRANSFER              | Order detail hiển thị đã thanh toán qua ngân hàng |
| PAY-P0-018  | BANK_TRANSFER_PENDING                 | Order detail hiển thị chờ thanh toán chuyển khoản |

**10.20. Situation Registry cần bổ sung**

PAYMENT_SITUATION_REGISTRY:  
\- SIT-PAYMENT-BANK-TRANSFER-SELECTED  
\- SIT-PAYMENT-BANK-TRANSFER-WAITING  
\- SIT-PAYMENT-CUSTOMER-SAYS-TRANSFERRED  
\- SIT-PAYMENT-PROOF-UPLOADED  
\- SIT-PAYMENT-MANUAL-RECONCILIATION  
\- SIT-PAYMENT-PAID-BY-BANK  
\- SIT-PAYMENT-WRONG-AMOUNT  
\- SIT-PAYMENT-WRONG-REFERENCE  
\- SIT-PAYMENT-DUPLICATE-TRANSACTION  
\- SIT-PAYMENT-UNMATCHED-BANK-TRANSACTION

**10.21. FILE Impact**

| **File** | **Nội dung cần bổ sung**                                                                                                             |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| FILE 04  | Payment method tagging, bank transfer resolver, accounting queue contract, manual reconciliation guard, payment status state machine |
| FILE 05  | Bank webhook, statement sync worker, transaction matcher, payment review case, accounting UI/API                                     |
| FILE 07  | Câu mẫu chuyển khoản, chờ đối soát, đã thanh toán, chưa xác nhận                                                                     |
| FILE 09  | P0 test payment tagging, accounting filter, reconciliation, duplicate, signature                                                     |
| FILE 10  | Situation registry payment                                                                                                           |
| FILE 12  | Human-level payment support, không làm khách lo, không kết luận sai                                                                  |
| FILE 03  | Payment schema fields: payment_method, payment_reference, accounting_review_status, proof metadata                                   |
| FILE 06  | Payment privacy/security, không lộ bằng chứng giao dịch                                                                              |

**10.22. DONE GATE**

Block này đạt khi:

- Khách chọn chuyển khoản thì order/draft được gắn payment_method = BANK_TRANSFER.
- Có payment_reference duy nhất.
- AI gửi đúng QR/STK/số tiền/nội dung chuyển khoản.
- Có Accounting Bank Transfer Queue.
- Kế toán lọc được đơn chờ chuyển khoản.
- Có manual reconciliation khi chưa có API ngân hàng.
- Có webhook/API reconciliation khi đã tích hợp ngân hàng.
- Có trạng thái BANK_TRANSFER_PENDING, PAYMENT_RECONCILING, PAID_BY_BANK_TRANSFER.
- AI không tự xác nhận đã thanh toán.
- Kế toán xác nhận thủ công có audit log.
- Duplicate transaction bị chặn.
- Sai số tiền/nội dung không được tự PAID.
- Order detail hiển thị đúng: **Chờ thanh toán chuyển khoản / Đang đối soát chuyển khoản / Đã thanh toán qua ngân hàng**.

**Kết luận khóa:**  
Đây là khóa bắt buộc cho thanh toán chuyển khoản quy mô lớn. Nếu không gắn thẻ BANK_TRANSFER ngay từ lúc khách chọn chuyển khoản, kế toán sẽ không thể lọc và đối soát khi có hàng chục ngàn đơn/ngày.

**11\. AI-TEST-NOTE-085 - USAGE / SERVING / DAILY CONSUMPTION LOCK**

**Phạm vi**

Khách hỏi:

- ăn lúc nào;
- ăn mấy gói;
- dùng bao lâu;
- có dùng hằng ngày không;
- sáng/tối có dùng được không.

**MUST**

- Trả lời theo hướng bữa ăn thực dưỡng, bữa sáng, bữa nhẹ, bữa tối nhẹ.
- Không dùng từ "liều dùng".
- Không cam kết hiệu quả sau X ngày.
- Nếu trẻ em/người già/health-sensitive → bật guard phù hợp.
- Gợi ý dùng đều đặn như một bữa ăn, không như thuốc.

**MUST NOT**

- Không nói uống/uống thuốc.
- Không nói "ngày mấy liều".
- Không cam kết phục hồi/sức khỏe sau bao lâu.
- Không khuyên thay bữa y tế nếu có bệnh nền.

**Resolver / Guard**

UsageContextResolver  
ServingSuggestionResolver  
DailyConsumptionGuard  
HealthSensitiveServingGuard

**DONE GATE**

- Test "ăn lúc nào".
- Test "mấy gói/ngày".
- Test "dùng bao lâu có hiệu quả".
- Test trẻ em/người già hỏi dùng.

**12\. AI-TEST-NOTE-086 - BODY FEELING / WEIGHT / HEATINESS LOCK**

**Phạm vi**

Khách hỏi:

- ăn có nóng không;
- có mập không;
- có tăng cân không;
- có đầy bụng không;
- có khó tiêu không.

**MUST**

- Trả lời theo thành phần, khẩu phần, bữa ăn thực dưỡng.
- Nếu khách quan tâm cân nặng → tư vấn chọn dòng nhẹ, khẩu phần phù hợp.
- Nếu khách sợ nóng → gợi ý dòng thanh nhẹ/thanh bổ nếu SKU phù hợp.
- Không claim giảm cân/tăng cân.

**MUST NOT**

- Không nói chắc "không mập".
- Không nói "giảm cân".
- Không nói "không nóng 100%".
- Không chẩn đoán cơ địa.

**Resolver**

BodyFeelingConcernResolver  
WeightConcernGuard  
HeatinessConcernResolver  
DigestiveComfortResolver

**DONE GATE**

- Test "ăn có nóng không".
- Test "có mập không".
- Test "đầy bụng không".
- Test khách muốn dòng nhẹ bụng.

**13\. AI-TEST-NOTE-087 - STORAGE / SHELF LIFE / AFTER OPENING LOCK**

**Phạm vi**

Khách hỏi:

- bảo quản thế nào;
- hạn dùng bao lâu;
- mở ra để được bao lâu;
- bị ẩm/vón/có mùi lạ.

**MUST**

- Hướng dẫn bảo quản nơi khô ráo, tránh nắng, tránh ẩm, đóng kín.
- Nếu đã mở gói → nên dùng ngay theo hướng dẫn sản phẩm.
- Nếu có dấu hiệu bất thường → không dùng, chụp ảnh, gửi CSKH.
- HSD phải lấy từ packaging/lot data nếu khách hỏi lô cụ thể.

**MUST NOT**

- Không bịa HSD.
- Không khuyên dùng nếu có mùi lạ/vón bất thường.
- Không nói lỗi sản xuất khi chưa QA xác minh.

**Resolver / Guard**

StorageInstructionResolver  
ShelfLifeResolver  
OpenedPackageGuidanceResolver  
QualitySignalGuard  
LotExpiryResolver

**DONE GATE**

- Test hỏi bảo quản.
- Test hỏi HSD.
- Test mở gói rồi để lâu.
- Test sản phẩm vón/mùi lạ.

**14\. AI-TEST-NOTE-088 - PREPARATION / SERVING INSTRUCTION LOCK**

**Phạm vi**

Khách hỏi:

- cách pha;
- cách nấu;
- có cần đun không;
- dùng nước bao nhiêu;
- chờ bao lâu;
- ăn nóng/lạnh được không.

**MUST**

- Hướng dẫn theo serving instruction đã được owner-approved.
- Không bịa lượng nước/thời gian nếu chưa có data.
- Nếu SKU có hướng dẫn riêng → lấy theo Product Usage Data.
- Ưu tiên trải nghiệm nóng/ấm để giữ cảm giác bữa ăn ngon.

**MUST NOT**

- Không tự tạo công thức pha.
- Không nói cách dùng làm sai chất lượng.
- Không khuyên trộn thành phần lạ nếu chưa có hướng dẫn.

**Resolver**

PreparationInstructionResolver  
ServingTemperatureResolver  
ProductUsageDataResolver  
InstructionExactnessGuard

**DONE GATE**

- Test "pha thế nào".
- Test "cần nấu không".
- Test "nước bao nhiêu".
- Test "ăn lạnh được không".

**15\. AI-TEST-NOTE-089 - TASTE OBJECTION / PRODUCT SWITCH LOCK**

**Phạm vi**

Khách nói:

- không hợp vị;
- nhạt/quá đậm;
- không thích mùi;
- bé không thích;
- người già khó ăn;
- muốn đổi dòng khác.

**MUST**

- Không phản bác khách.
- Ghi nhận cảm nhận.
- Hỏi nhẹ khách thích vị thanh/đậm/béo/thơm/dễ ăn hơn nếu cần.
- Gợi ý SKU thay thế theo khẩu vị + Product Effectiveness.
- Nếu nghi lỗi chất lượng → chuyển complaint/QA.

**MUST NOT**

- Không nói khách dùng sai.
- Không ép mua lại cùng SKU.
- Không nói "đổi món để không ngán".
- Không đổi sang SKU mặn nếu khách ăn chay.

**Resolver**

TasteFeedbackResolver  
ProductSwitchResolver  
TasteProfileMatcher  
NegativeFeedbackSuppressionGuard  
QualityComplaintDetector

**DONE GATE**

- Test chê nhạt.
- Test chê mùi.
- Test bé không thích.
- Test người già khó ăn.
- Test nghi lỗi chất lượng.

**16\. AI-TEST-NOTE-090 - TRUST / PRICE OBJECTION / VALUE DEFENSE LOCK**

**Phạm vi**

Khách hỏi:

- có thật không;
- có lừa đảo không;
- sao đắt vậy;
- khác cháo thường chỗ nào;
- có gì đáng tiền.

**MUST**

- Giải thích bằng khoa học, Sâm Savigin, nguyên liệu, công nghệ sấy thăng hoa, y thực đồng nguyên.
- Không đôi co.
- Không hạ giá trị sản phẩm.
- Nếu khách nghi lừa đảo có bằng chứng/mã đơn → route complaint/CSKH.
- Nếu chỉ phản đối giá → giải thích giá trị và gợi ý phương án phù hợp hơn.

**MUST NOT**

- Không công kích khách.
- Không trả lời phòng thủ.
- Không nói quá claim.
- Không tự giảm giá ngoài runtime.

**Resolver**

TrustObjectionResolver  
PriceObjectionResolver  
ValueDefenseContentResolver  
ComplaintEvidenceGuard

**DONE GATE**

- Test "sao đắt vậy".
- Test "có lừa đảo không".
- Test "khác cháo thường gì".
- Test khách nghi ngờ thương hiệu.

**17\. AI-TEST-NOTE-091 - RETURN / REFUND / PRODUCT ISSUE LOCK**

**Phạm vi**

Khách hỏi:

- đổi trả;
- hoàn tiền;
- giao sai;
- thiếu hàng;
- rách gói;
- vón cục;
- mùi lạ;
- muốn đổi vị.

**MUST**

- Xin lỗi, tiếp nhận, xin ảnh/mã đơn/mã lô nếu cần.
- Phân loại: delivery issue / product quality / wrong item / missing item / taste preference.
- Nếu chính hãng và lỗi trong chính sách → xử lý đổi/trả/hoàn theo policy.
- Nếu nghi hàng giả → route counterfeit/CSKH/legal, không xử lý như lỗi thường.
- Sau xử lý, cảm ơn khách đã phản hồi.

**MUST NOT**

- Không đổ lỗi khách.
- Không kết luận thu hồi/lỗi sản xuất khi chưa QA.
- Không từ chối cứng nếu chưa kiểm tra.
- Không tiếp tục bán khi complaint mở.

**Resolver**

ReturnRefundResolver  
ProductIssueClassifier  
OrderEvidenceResolver  
TraceQRCodeResolver  
QualityCaseRouter  
CounterfeitSuspicionGuard

**DONE GATE**

- Test rách gói.
- Test thiếu hàng.
- Test giao sai.
- Test mùi lạ.
- Test hoàn tiền.
- Test nghi hàng giả.

**18\. AI-TEST-NOTE-092 - ORDER EDIT / ADDRESS CHANGE / CANCEL LOCK**

**Phạm vi**

Khách muốn:

- sửa đơn;
- đổi địa chỉ;
- đổi số điện thoại;
- thêm/bớt sản phẩm;
- hủy đơn;
- đổi phương thức thanh toán.

**MUST**

- Kiểm tra order state.
- Nếu chưa bàn giao vận chuyển → cho sửa theo policy, revalidate quote/order draft.
- Nếu đã bàn giao vận chuyển → route delivery support.
- Nếu đã thanh toán → cần payment/refund policy.
- Nếu Giờ Vàng/quota → revalidate quota.
- Mọi thay đổi phải audit.

**MUST NOT**

- Không sửa đơn nếu order locked mà không có policy.
- Không nói sửa thành công nếu chưa update.
- Không giữ giá cũ nếu quote hết hạn.
- Không hủy đơn đã fulfill nếu không theo policy.

**Resolver**

OrderEditEligibilityResolver  
AddressChangeResolver  
CancelOrderResolver  
QuoteRevalidationGuard  
FulfillmentStateGuard  
PaymentRefundGuard

**DONE GATE**

- Test đổi địa chỉ trước giao.
- Test đổi địa chỉ sau bàn giao.
- Test thêm SKU.
- Test hủy đơn.
- Test đã thanh toán muốn hủy.

**19\. AI-TEST-NOTE-093 - VAT INVOICE / COMPANY PAYMENT LOCK**

**Phạm vi**

Khách hỏi:

- xuất hóa đơn VAT;
- công ty thanh toán;
- lấy hóa đơn đỏ;
- cần MST;
- gửi hóa đơn qua email.

**MUST**

- Thu thập đúng thông tin xuất hóa đơn:
  - tên công ty;
  - mã số thuế;
  - địa chỉ;
  - email nhận hóa đơn;
  - mã đơn.
- Không xuất hóa đơn nếu thiếu dữ liệu.
- VAT hiển thị rõ trong quote/order nếu áp dụng.
- Công ty thanh toán chuyển khoản phải đi Payment Core.

**MUST NOT**

- Không bịa VAT.
- Không nói "VAT nếu áp dụng".
- Không xuất sai MST.
- Không tự sửa hóa đơn nếu không có policy.

**Resolver**

VATInvoiceRequestResolver  
CompanyPaymentResolver  
TaxInfoValidationGuard  
InvoiceIssuanceGuard

**DONE GATE**

- Test khách cần hóa đơn.
- Test thiếu MST.
- Test sai thông tin công ty.
- Test công ty chuyển khoản.
- Test yêu cầu sửa hóa đơn.

**20\. AI-TEST-NOTE-094 - BULK BUY / CORPORATE GIFT / NON-DISTRIBUTOR LOCK**

**Phạm vi**

Khách mua số lượng lớn nhưng không phải đại lý:

- mua cho công ty;
- mua làm quà Tết;
- mua cho nhân viên;
- mua cho trường/cơ quan;
- mua nhóm văn phòng;
- mua biếu đối tác.

**MUST**

- Phân biệt bulk gift với đại lý/nhà phân phối.
- Nếu khách hỏi đại lý/mua sỉ/phân phối → 0889787878.
- Nếu mua quà số lượng lớn → route corporate gift flow, không mặc định là đại lý.
- Báo giá số lượng lớn phải qua Quote/Bulk policy.
- Có thể cần human/corporate support.

**MUST NOT**

- Không tự báo chiết khấu số lượng lớn.
- Không tự hứa branding riêng.
- Không gom mọi đơn lớn vào đại lý.
- Không bỏ qua payment/invoice/shipping.

**Resolver**

BulkBuyIntentResolver  
CorporateGiftIntentResolver  
DistributorIntentGuard  
BulkQuoteResolver  
CorporateSupportRouter

**PHASE 4 CARRY-FORWARD**

Phase 4 consume các resolver này qua `BulkDistributorIntentRule`, `P4-SRS-FR-022`, `P4-GATE-017`, `P4-SMK-024` và `P4-2C-SMK-014`. File này là source tình huống đời thường; câu khách vẫn phải render qua Phase 4 guard/registry, không render trực tiếp raw wording.

**DONE GATE**

- Test công ty mua 100 phần quà.
- Test trường mua cho thầy cô.
- Test khách hỏi đại lý.
- Test khách mua nhiều cho gia đình.
- Test cần báo giá doanh nghiệp.

**21\. AI-TEST-NOTE-095 - PRIVACY / PERSONAL DATA REQUEST LOCK**

**Phạm vi**

Khách hỏi:

- xóa thông tin;
- sửa số điện thoại/địa chỉ;
- không muốn nhận tin;
- thông tin có bị lộ không;
- yêu cầu không lưu dữ liệu.

**MUST**

- Không public PII.
- Cho opt-out CRM.
- Route privacy request.
- Nếu yêu cầu xóa/sửa dữ liệu → open privacy case.
- CRM suppress ngay khi opt-out.
- Không dùng lại dữ liệu đã opt-out cho marketing.

**MUST NOT**

- Không đọc số điện thoại/địa chỉ đầy đủ trong public.
- Không tranh luận khi khách muốn dừng nhận tin.
- Không tiếp tục gửi CRM khi opt-out.
- Không xác nhận xóa dữ liệu nếu chưa xử lý.

**Resolver**

PrivacyRequestResolver  
PIIExposureGuard  
OptOutResolver  
DataCorrectionCaseRouter  
CRMConsentGuard

**DONE GATE**

- Test "đừng nhắn nữa".
- Test "xóa thông tin tôi".
- Test "đổi số điện thoại".
- Test public có PII.
- Test CRM after opt-out.

**22\. AI-TEST-NOTE-096 - OFFICIAL CHANNEL / MARKETPLACE / FAKE GOODS LOCK**

**Phạm vi**

Khách hỏi:

- mua trên Shopee/TikTok/Lazada được không;
- đại lý ngoài;
- hàng thật/hàng giả;
- thấy chỗ khác bán rẻ hơn;
- check mã QR.

**MUST**

- Ưu tiên kênh chính thức.
- Không xác nhận đại lý ngoài nếu không có dữ liệu.
- Nếu khách nghi hàng giả → yêu cầu ảnh bao bì/QR/mã lô/nơi mua/hóa đơn.
- Không xử lý hàng chưa xác thực như lỗi đổi trả thông thường.
- Nếu QR/mã lô không thuộc kênh chính thức → route counterfeit/legal/CSKH.

**MUST NOT**

- Không nói mọi kênh ngoài đều giả nếu chưa xác minh.
- Không xác nhận chính hãng nếu chưa trace.
- Không bán hàng cho account có dấu hiệu malicious.
- Không đưa kết luận pháp lý public.

**Resolver**

OfficialChannelResolver  
MarketplacePolicyResolver  
TraceQRCodeResolver  
CounterfeitSuspicionGuard  
ExternalSellerVerificationResolver

**DONE GATE**

- Test "Shopee có không".
- Test "TikTok có không".
- Test "chỗ kia rẻ hơn".
- Test check QR.
- Test nghi hàng giả.

**23\. FILE IMPACT MATRIX**

| **File** | **Nội dung bổ sung**                                                                                                                                                                                                                                                          |
| -------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FILE 01  | Thêm toàn bộ situation locks 076-096 vào Parent Logic                                                                                                                                                                                                                         |
| FILE 04  | Resolver/Guard/Runtime Contract cho health, gift, office/student, overseas, pillar/vegan, inventory, IVR, shipping, payment method tagging, Accounting Bank Transfer Queue, manual bank reconciliation, payment, usage, storage, return, order edit, VAT, privacy, fake goods |
| FILE 07  | Content blocks customer-facing cho từng nhóm tình huống                                                                                                                                                                                                                       |
| FILE 09  | P0 Test Matrix cho 076-096                                                                                                                                                                                                                                                    |
| FILE 10  | Situation Registry map intent → resolver → guard → action → content → test                                                                                                                                                                                                    |
| FILE 12  | Human-level behavior: bán mềm, chăm sóc sâu, xử lý phản đối, complaint, member, payment, delivery                                                                                                                                                                             |
| FILE 02  | Bổ sung SKU role theo health/gift/office/student/overseas/pillar/vegan                                                                                                                                                                                                        |
| FILE 03  | Schema bổ sung runtime fields                                                                                                                                                                                                                                                 |
| FILE 05  | Worker/API cho IVR, payment, shipping, CRM, privacy, refund                                                                                                                                                                                                                   |
| FILE 06  | Claim/safety/privacy/health/fake goods                                                                                                                                                                                                                                        |
| FILE 08  | SKU content theo tình huống bán hàng đời thường                                                                                                                                                                                                                               |

**24\. DONE GATE CHO BẢN BỔ SUNG**

Bản bổ sung này đạt khi:

- Đã ghi nhận đủ AI-TEST-NOTE-076 → 096.
- Mỗi note có phạm vi, MUST, MUST NOT, resolver/guard, done gate.
- Không còn tình huống đời thường lớn bị bỏ trống.
- Có FILE Impact rõ.
- Tất cả nhóm liên quan giá/thanh toán/vận chuyển/đơn hàng đều đi qua Runtime Core.
- Tất cả nhóm liên quan bệnh/sức khỏe đi qua HealthSafetyGuard.
- Tất cả nhóm liên quan sản phẩm đi qua Product Effectiveness Block.
- Tất cả nhóm liên quan thanh toán đi qua Payment Core.
- Tất cả nhóm liên quan vận chuyển đi qua ShippingResolver.
- Gateway vẫn BLOCKED.

**25\. THỨ TỰ SAU BẢN BỔ SUNG**

- Owner rà bản bổ sung này.
- Nếu còn tình huống đời thường mới, tiếp tục test và thêm note.
- Khi owner chốt dừng test, gộp bản này vào Master Rule Lock V2.1.
- Viết lại FILE 01.
- Viết FILE 04.
- Viết FILE 07.
- Viết FILE 10.
- Viết FILE 09.
- Viết FILE 12.
- Rà FILE 02/03/05/06/08/11/Appendix.

**Kết luận:** Bản bổ sung này là lớp khóa tình huống đời thường trước khi viết lại tài liệu AI TÍCH HỢP CHANNEL. Trạng thái vẫn là: **Gateway BLOCKED / FILE 04 NOT_STARTED / Production Readiness BLOCKED**.
