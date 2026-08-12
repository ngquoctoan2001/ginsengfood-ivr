# MASTER-02 - CROSS PACK DEPENDENCY

## Mục Đích

MASTER-02 khóa bản đồ phụ thuộc giữa các PACK. Tài liệu này trả lời câu hỏi: pack nào phải có trước, pack nào chỉ được đọc, pack nào được quyết định và pack nào phải dừng khi upstream chưa đủ điều kiện.

MASTER-02 không thay thế PACK chi tiết. Mọi implementation vẫn phải được viết trong PACK hoặc PHASE tương ứng.

## Nguyên Tắc Dependency

1. Dependency đi theo chiều owner core sang consumer.
2. Consumer không được tạo nguồn sự thật song song.
3. Pack downstream không được mở hành động nếu upstream chưa trả trạng thái hợp lệ.
4. Evidence dependency phải trace được về source.
5. Dependency không được hiểu là quyền sửa dữ liệu của pack khác.

## Dependency Graph Chính

```text
PACK-02 Product Master
  -> PACK-01 Operational Core
  -> PACK-03 Demand / MRP
  -> PACK-04 MISA Handoff
  -> PACK-05 AI Advisor
  -> PACK-06 Facebook Gateway
  -> PACK-07 Ads
  -> PACK-08 MC AI Live
  -> PACK-09 IVR
  -> PACK-10 Evidence / Completion
```

## Matrix Phụ Thuộc Theo PACK

| Pack | Phụ thuộc vào | Cung cấp cho | Chặn nếu thiếu |
| --- | --- | --- | --- |
| PACK-01 Operational Core | PACK-02 Product / Recipe | Warehouse, inventory, trace, recall, sale lock, MISA handoff | Product/recipe snapshot, raw lot readiness, release rule |
| PACK-02 Product Master | MASTER source rules | Operational, AI, Ads, Live | Canonical SKU, ingredient, recipe, formula version |
| PACK-03 Demand / MRP | PACK-02, PACK-01 inventory | Procurement, material planning | BOM, stock, threshold, owner approval |
| PACK-04 MISA Handoff | PACK-01 checkpoints, accounting mapping | Accounting sync, reconcile | Mapping, checkpoint, audit |
| PACK-05 AI Advisor | PACK-02, PACK-01, Commerce Runtime | Customer consulting, channel response | Public-safe product, sellable, price, stock |
| PACK-06 Facebook Gateway | PACK-05, channel policy | Messenger, Live, private handoff | App permission, policy, human takeover |
| PACK-07 Ads | PACK-06, PACK-05, Commerce verified revenue | Attribution, scale gate | Verified revenue, tracking quality |
| PACK-08 MC AI Live | PACK-02, PACK-01, PACK-05, PACK-07 | Live board, script brief | Board, script, claim guard, sale lock |
| PACK-09 IVR | Order confirmation runtime, PACK-01 sale lock | Confirmation signal | Eligible order, phone, retry policy |
| PACK-10 Completion | All PACK evidence | Release readiness | Evidence, smoke, sign-off, P0 clearance |

## Critical Dependency Rules

### Product To Operational

PACK-01 không được tạo Production Order nếu PACK-02 chưa cung cấp SKU, recipe, formula version, ingredient, UOM và BOM snapshot đủ điều kiện.

### Operational To Sellable

Downstream chỉ được xem sản phẩm có thể bán khi Operational Core trả đủ release, warehouse receipt, inventory availability, trace validity và không bị recall/sale lock.

### Operational To MISA

PACK-04 chỉ nhận accounting handoff tại checkpoint hợp lệ. MISA không quyết định production order, material issue, batch release, warehouse receipt hoặc inventory truth.

### Operational To AI / Gateway / Ads / Live

AI, Gateway, Ads và Live chỉ được dùng public-safe truth. Nếu recall/sale lock hoặc inventory unavailable, downstream phải suppress hoặc handoff theo rule.

### Evidence To Release

PACK-10 chỉ tổng hợp evidence. PACK-10 không thay owner core và không được nâng trạng thái khi thiếu evidence accepted.

## Cross-Pack Smoke Coverage

| Smoke group | Upstream | Downstream | Mục tiêu |
| --- | --- | --- | --- |
| Product to Production | PACK-02 | PACK-01 | Production chỉ dùng formula snapshot hợp lệ |
| Raw Lot to Material Issue | PACK-01 | PACK-01 | Lot chưa READY_FOR_PRODUCTION không được issue |
| Batch to Warehouse | PACK-01 | PACK-01 | Batch chưa RELEASED không được receipt |
| Inventory to AI | PACK-01 | PACK-05 | AI không tư vấn item unavailable |
| Sale Lock to Channel | PACK-01 | PACK-06 | Gateway suppress đúng scope |
| Verified Revenue to Ads | Commerce Runtime | PACK-07 | Ads không scale từ revenue chưa verified |
| Live Board to MC AI | PACK-08 | PACK-05/PACK-06 | Live script không nói ngoài board |
| Evidence to Completion | All PACK | PACK-10 | Completion chỉ review artifact cụ thể |

## Dependency Fail Conditions

Một pack phải dừng hoặc trả trạng thái không đủ điều kiện khi:

1. Thiếu Source-of-Truth.
2. Thiếu owner decision.
3. Thiếu resolver cho dữ liệu runtime.
4. Thiếu guard cho hành động rủi ro cao.
5. Thiếu audit hoặc evidence bắt buộc.
6. Dữ liệu upstream đang conflict.
7. Consumer cố sửa hoặc hardcode dữ liệu owner core.

## Handoff Matrix

| Handoff | Bên giao | Bên nhận | Nội dung tối thiểu |
| --- | --- | --- | --- |
| Product -> Operational | PACK-02 | PACK-01 | SKU, recipe, formula version, BOM, UOM |
| Operational -> MISA | PACK-01 | PACK-04 | Checkpoint, amount, item, account mapping key |
| Operational -> AI | PACK-01 | PACK-05 | Sellable, trace public, sale lock |
| AI -> Gateway | PACK-05 | PACK-06 | Response, handoff flag, safety output |
| Gateway -> Ads | PACK-06 | PACK-07 | Channel event, attribution signal |
| Operational -> IVR | PACK-01 / Order runtime | PACK-09 | Sale lock and eligibility |
| All -> Evidence | All PACK | PACK-10 | Smoke result, evidence packet, audit reference |

## Done Gate Của MASTER-02

MASTER-02 đạt yêu cầu khi:

1. Dependency Product -> Operational -> downstream rõ.
2. MISA, AI, Gateway, Ads, Live, IVR không vượt owner core.
3. Recall / Sale Lock có priority cao nhất.
4. Evidence dependency rõ.
5. PACK chi tiết có thể dùng matrix này để viết handoff và smoke.

## Chi Tiết Governance Tương Ứng

Phần này giữ lại phạm vi chi tiết từ nguồn MASTER theo cụm cùng chủ đề, đồng thời chuẩn hóa theo registry PACK hiện hành và loại bỏ các câu trạng thái tổng không dùng trong bộ sạch.


## Master-02 — Cross-Pack Dependency Map

MASTER-02 phải dùng registry trong MASTER-01 để vẽ dependency.

Nguyên tắc handoff:

Dependency phải xuất phát từ Source-of-Truth.Pack consumer phụ thuộc pack owner source.Runtime resolver tạo dependency runtime.Evidence source tạo dependency release.Security source tạo dependency protected operation.

Ví dụ:

AI Advisor phụ thuộc Product Knowledge vì cần SRC-PROD-001.AI Advisor phụ thuộc Commerce Pricing vì cần SRC-PRICE-001.Gateway phụ thuộc Payment Core nếu nói payment vì cần SRC-PAY-001.CRM phụ thuộc Suppression Resolver vì cần SRC-CRM-001.IVR phụ thuộc Order Core vì cần SRC-ORDER-003.Landing Page phụ thuộc Claim Policy vì cần SRC-CLAIM-001.Admin UI phụ thuộc Security Core vì cần SRC-SEC-001.Release phụ thuộc Evidence Registry vì cần SRC-EVD-001.

HẾT PHẦN 4/4


## Hết Master-01 — Global Source-Of-Truth Registry


## Data Authority / Runtime Resolver / Source Owner / Consumer Boundary / Evidence Control

SOURCE-DRIVEN DEPENDENCY / PACK RELATIONSHIP / BUSINESS-RULE-DRIVEN RUNTIME DEPENDENCY / RELEASE DEPENDENCY CONTROL


## PHẦN 1/4 — DEPENDENCY PRINCIPLES / BUSINESS-RULE-DRIVEN DEPENDENCY MODEL / SOURCE BOUNDARY


## 0. VAI TRÒ CỦA MASTER-02

MASTER-02 là tài liệu quản trị bản đồ phụ thuộc liên pack của hệ thống Ginsengfood.

MASTER-02 quy định cách các domain nghiệp vụ, source-of-truth, owner core, runtime resolver, consumer pack, evidence và release gate phụ thuộc lẫn nhau trong toàn bộ hệ thống.

MASTER-02 không phải tài liệu implementation chi tiết.

MASTER-02 không viết code, API, DTO, database, worker, UI hoặc template nội dung hoàn chỉnh.

Các pack chi tiết chịu trách nhiệm triển khai sâu theo boundary và dependency đã được MASTER-02 khóa.

MASTER-02 có vai trò bắt buộc trong việc ngăn dev, AI Advisor, Gateway, CRM, Landing Page, ADS, Admin UI hoặc các consumer khác tự suy luận rule kinh doanh, tự hardcode dữ liệu runtime hoặc tự vượt quyền owner core.


## 1. MỤC ĐÍCH CỦA MASTER-02

MASTER-02 khóa bản đồ phụ thuộc giữa các domain nghiệp vụ và các pack kỹ thuật trong hệ thống Ginsengfood.

Mục tiêu của MASTER-02 là đảm bảo:

Mỗi dữ liệu quan trọng đều có source-of-truth.

Mỗi quyết định nghiệp vụ đều có owner core.

Mỗi consumer chỉ được sử dụng dữ liệu trong phạm vi cho phép.

Mọi quyết định runtime phải đi qua resolver hoặc core tương ứng.

Mọi hành vi customer-facing phải qua guard phù hợp.

Mọi protected operation phải có permission, audit và evidence.

Mọi rule ảnh hưởng nhiều pack phải được khóa ở tầng dependency governance.

Mọi dữ liệu runtime có tính quyết định không được hardcode trong AI, Gateway, CRM, Landing Page, Admin UI, ADS hoặc template tĩnh.

Mọi release phải có evidence.

Gateway không được mở nếu Completion Report chưa PASS.

Production Ready không được gọi nếu chưa có smoke, evidence và owner sign-off.

Các pack chi tiết không được tự suy luận rule kinh doanh ngoài MASTER-02.

MASTER-02 giúp toàn hệ thống vận hành theo một nguyên tắc thống nhất:

Source-of-Truth quyết định dữ liệu.Owner Core quyết định nghiệp vụ.Runtime Resolver quyết định tại thời điểm thực thi.Consumer chỉ tiêu thụ trong boundary.Guard kiểm soát điều kiện an toàn.Evidence chứng minh dependency đã hoạt động.Release Gate quyết định được phép mở hay chưa.


## 2. PHẠM VI CỦA MASTER-02

MASTER-02 bao phủ dependency giữa các nhóm domain sau:

Governance / Source-of-Truth.

Product Knowledge / Claim / Brand / Scientific Proof.

Customer Identity / Customer Memory.

Segment Recommendation / Product Recommendation.

CRM 12-Month / Message Trigger / Suppression.

Member Lifecycle / Rights / Downgrade / Dispute.

Diamond / Affiliate / Entrepreneurship / Distributor Boundary.

Pricing / Program / 24/7 / Golden Hour / QuoteSnapshot.

Program / Sellable / Product Activation / Production Signal.

Payment / Bank Transfer / Accounting Review.

Shipping / Tracking / ETA / COD.

Order / OrderDraft / CustomerConfirmation / IVR.

Gateway / Public Comment / Messenger Handoff / Moderation.

Official Contact / Phone Number Registry.

Health Boundary / Complaint / Priority Conflict.

Evidence / Completion Report / Gateway Gate / Security.

MASTER-02 chỉ khóa dependency governance.

Chi tiết API, DTO, database, worker, UI, content block, message template, test case và deployment thuộc pack chi tiết.


## 3. NON-SCOPE CỦA MASTER-02

MASTER-02 không làm các việc sau:

Không viết API endpoint chi tiết.

Không viết DTO.

Không viết database schema.

Không viết migration.

Không viết service class.

Không viết worker.

Không viết UI component.

Không viết full content block.

Không viết toàn bộ tin nhắn CRM.

Không viết toàn bộ test script.

Không viết cấu hình Meta App.

Không viết Page Token.

Không viết webhook secret.

Không viết tích hợp ngân hàng chi tiết.

Không viết tích hợp đơn vị vận chuyển chi tiết.

Không viết MISA mapping chi tiết.

Không viết code để dev copy-paste.

Không thay thế pack implementation.

Không thay thế Evidence Package.

Không thay thế Completion Report.

MASTER-02 chỉ xác định:

Source nào quyết định.Owner nào chịu trách nhiệm.Consumer nào được dùng.Resolver nào bắt buộc.Guard nào bắt buộc.Thiếu gì thì bị chặn.Evidence nào phải có.Pack nào triển khai chi tiết.Done Gate nào phải đạt.


## 4. TRẠNG THÁI TOÀN HỆ

MASTER-02 là tài liệu governance.

MASTER-02 không làm thay đổi trạng thái production.

Trạng thái toàn hệ phải giữ:

Ý nghĩa:

Viết xong MASTER-02 không đồng nghĩa Gateway được mở.

Viết xong MASTER-02 không đồng nghĩa Completion Report PASS.

Viết xong MASTER-02 không đồng nghĩa hệ thống production-ready.

Viết xong MASTER-02 không đồng nghĩa release được phê duyệt.

Viết xong MASTER-02 không đồng nghĩa go-live được phê duyệt.

Mọi trạng thái PASS phải dựa trên smoke, evidence, runtime trace và owner sign-off.

Không được dùng tài liệu governance thay cho evidence triển khai.

Không được dùng câu trả lời AI nghe tốt để thay cho DecisionEnvelope, resolver trace, guard trace hoặc audit evidence.


## 5. NGUYÊN TẮC DEPENDENCY CỐT LÕI


### 5.1. Source-of-Truth quyết định dữ liệu

Mọi dữ liệu quan trọng phải có nguồn sự thật rõ ràng.

Consumer không được tự quyết định dữ liệu.

Consumer không được biến bản copy dữ liệu thành nguồn quyết định.

Consumer không được lấy dữ liệu từ prompt, template, cache cũ, file tĩnh hoặc nội dung hiển thị để ra quyết định runtime.

Mô hình đúng:

Source-of-Truth-> Owner Core / Owner Pack-> Runtime Resolver / Guard-> Consumer Pack-> Action / Display / Decision-> Evidence-> Done Gate

Mô hình sai:

Consumer-> tự tính-> tự quyết định-> tự ghi trạng thái-> tự render cho khách

Ví dụ bị cấm:

AI tự tính giá.

CRM tự gửi quyền lợi thành viên.

Gateway tự xác nhận đơn.

Landing Page hardcode miễn phí ship.

Admin UI tự set PAID.

IVR tự đổi order state.

ADS tự chọn SKU mở bán.

MC AI nói SKU ngoài board.

CRM tự tạo trigger tin nhắn.

AI tự gửi số điện thoại ngoài Official Contact Registry.


### 5.2. Owner Core quyết định, Consumer chỉ tiêu thụ

Owner Core là nơi có quyền quyết định nghiệp vụ.

Consumer chỉ được tiêu thụ kết quả đã được owner core hoặc resolver trả về.

Commerce Pricing Core quyết định giá.AI chỉ hiển thị giá từ QuoteSnapshot.

Shipping Core quyết định ETA/COD/phí ship.Landing Page chỉ hiển thị kết quả shipping được phép.

Payment Core / Accounting Review quyết định PAID.Admin UI không được tự set PAID.

Order Core quyết định trạng thái đơn.Gateway, AI, IVR không được tự đổi order state.

Member Lifecycle Core quyết định hạng, duy trì, ân hạng, hạ hạng.CRM không được tự nói khách bị hạ hạng nếu runtime chưa trả.

Official Contact Registry quyết định số điện thoại được gửi.AI không được tự nhớ hoặc tự bịa số điện thoại.


### 5.3. Runtime Resolver là bắt buộc cho quyết định tại thời điểm thực thi

Các quyết định sau bắt buộc phải qua resolver hoặc core:

Customer identity.

Customer memory.

Last purchase.

Member tier.

Member lifecycle.

Diamond referral bind.

Commission eligibility.

CRM eligibility.

Message trigger.

Message dedup.

Product recommendation.

Segment recommendation.

Product sellable status.

Program activation.

24/7 price.

Golden Hour active status.

Early access.

QuoteSnapshot.

OrderDraft.

CustomerConfirmation.

Payment eligibility.

Bank transfer instruction.

Payment state.

Shipping eligibility.

Tracking status.

Official contact.

Health guard.

Claim guard.

Handoff status.

Moderation action.

Evidence gate.

Completion Report gate.

Nếu resolver thiếu, consumer không được suy đoán.


### 5.4. Rule kinh doanh ảnh hưởng nhiều pack phải được khóa trong MASTER-02

Bất kỳ quy tắc kinh doanh nào ảnh hưởng từ hai pack trở lên phải được đưa vào MASTER-02 ở tầng dependency governance.

Customer Identity ảnh hưởng AI, CRM, Pricing, Order, Member, Diamond, Gateway.

CRM 12 tháng ảnh hưởng Customer Memory, Order History, Product Knowledge, Member Lifecycle, Messaging, CRM Suppression.

Member Lifecycle ảnh hưởng AI, CRM, Quote, Pricing, Message, Outcome Logger.

Giờ Vàng ảnh hưởng Pricing, Availability, Live, Gateway, QuoteSnapshot, IVR, CRM, ADS, Order.

Quy tắc mở bán ảnh hưởng Operational Core, Availability, Pricing, Program, Gateway, AI, Landing Page, ADS, CRM, Order và Production Planning.

Các rule này không được để ẩn trong content, phụ lục, test hoặc note kỹ thuật rời rạc.

MASTER-02 phải biết rule gốc.

Pack chi tiết mới triển khai sâu.


## 6. BUSINESS-RULE-DRIVEN DEPENDENCY MODEL

MASTER-02 sử dụng mô hình:

Business Rule-> Source-of-Truth-> Owner Core / Owner Pack-> Runtime Resolver / Guard-> Consumer Pack-> Allowed Action-> bị chặn-if-missing-> Evidence-> Done Gate

Ví dụ với Customer Memory:

Khách cũ phải hỏi thăm trước khi bán-> Customer Memory Source-> Customer Memory Core / CRM Lifecycle Core-> LastPurchaseResolver / CareBeforeNextSaleGuard-> AI Advisor / CRM-> Hỏi thăm trải nghiệm-> Thiếu last purchase thì không nhắc cụ thể-> Evidence returning customer care-> Done Gate khách cũ không bị bán ngay

Ví dụ với mở bán SKU:

SKU chỉ được mở bán khi sellable-> Sellable Availability Source / Operational Core / Pricing Source-> Commerce Availability Core-> AvailabilityResolver / ProgramActivationResolver-> AI / Gateway / CRM / Landing / ADS / Order-> Tư vấn / quote / order SKU active-> Thiếu sellable thì không bán-> Evidence availability + quote/order guard-> Done Gate không SKU not sellable nào được bán

Ví dụ với Giờ Vàng:

Giờ Vàng mở theo tồn kho và chương trình-> Program Policy Source / Availability Source-> Golden Hour Engine-> GoldenHourResolver / EarlyAccessResolver / QuoteLockResolver-> AI / Gateway / CRM / Order / IVR-> Quote Giờ Vàng trong private-> Thiếu resolver thì không nói active / không giữ giá-> Evidence GoldenHourResolver + QuoteSnapshot-> Done Gate Giờ Vàng runtime pass

Ví dụ với Message Trigger:

Tin nhắn chỉ được gửi theo trigger đã duyệt-> Message Trigger Registry-> Messaging Core / CRM Lifecycle Core-> MessageTriggerResolver / MessageTextImmutableGuard / DedupGuard-> CRM / Messenger / SMS / Email-> Gửi đúng tin, đúng kênh, đúng hạng-> Thiếu trigger thì không gửi-> Evidence message job + send log-> Done Gate không gửi sai trigger, sai hạng, trùng tháng


## 7. CUSTOMER CONTEXT FIRST PRINCIPLE

AI Advisor không được tư vấn như một chatbot không biết khách là ai.

Trước khi tư vấn có tính cá nhân hóa, báo quyền lợi, CRM, quote, Diamond, referral hoặc member lifecycle, hệ thống phải resolve:

Khách mới hay khách cũ.

Có lịch sử mua hay chưa.

Có phải member không.

Hạng thành viên hiện tại.

Có phải Diamond không.

Có đi qua link Diamond không.

Link Diamond có bind hợp lệ không.

Doanh số tích lũy.

Chu kỳ thành viên.

Có đang trong ân hạng không.

Lần gần nhất mua sản phẩm gì.

Mua cho ai.

Có open case không.

Có opt-out CRM không.

Có dispute quyền lợi không.

Nếu thiếu Customer Context:

AI không được khẳng định khách là member.AI không được nói khách là Diamond.AI không được nói quyền lợi hạng.AI không được nhắc lịch sử mua cụ thể.AI không được cá nhân hóa sâu.CRM không được gửi tin lifecycle.Quote không được áp member benefit.Diamond benefit không được áp.

Customer Context First là P0 dependency.


## 8. CUSTOMER MEMORY / CARE BEFORE NEXT SALE PRINCIPLE

Khách đã mua quay lại không được bị bán tiếp ngay.

AI phải hỏi thăm trải nghiệm trước:

Dùng có ngon không.

Có hợp không.

Có vừa ý không.

Người nhận dùng có hợp không.

Dịch vụ có tốt không.

Có hài lòng không.

Có vấn đề gì cần hỗ trợ không.

Nếu runtime có dữ liệu, AI phải nhắc đúng sản phẩm mua gần nhất.

Nếu runtime có dữ liệu người nhận, AI phải nhắc đúng người nhận.

Nếu khách phản hồi tích cực, AI mới bắt cầu sang tư vấn sản phẩm tiếp theo.

Nếu khách phản hồi chưa hài lòng, AI phải ưu tiên chăm sóc hoặc xử lý trải nghiệm, không được nhảy sang upsell.

Consumer bị cấm:

Không bán ngay khi khách cũ chỉ mới chào.

Không hỏi lại dữ liệu đã có.

Không bịa lịch sử mua.

Không bịa người nhận.

Không nhắc sản phẩm mua gần nhất nếu runtime không trả.

Không tiếp tục bán khi khách đang có open case.

Không dùng phản hồi tiêu cực làm cơ hội upsell.

Customer Memory / Care Before Next Sale là P0 dependency.


## 9. CRM 12-MONTH LIFECYCLE PRINCIPLE

CRM là năng lực chăm sóc vòng đời khách hàng.

CRM không phải tin nhắn khuyến mãi.

CRM không phải spam bán thêm.

CRM phải dựa trên:

Customer Identity.

Customer Memory.

Order History.

Product Effectiveness.

Member Lifecycle.

Event Calendar.

Suppression Policy.

Product Recommendation.

Feedback.

Consent.

Channel history.

Frequency cap.

Quiet hours.

CRM 12 tháng gồm các lớp:

D0 — Post Purchase WelcomeD1 — Usage GuideD3 — First Experience CheckD7 — Taste/Fit CheckD14 — Reorder Soft SuggestionD21 — Next Product SuggestionD30 — First Month ReviewM2 — Next Need CareM3 — Family/Gift/Seasonal CareM4 — Product EducationM5 — Growth Order CareM6 — Mid-cycle ReviewM7 — Member Lifecycle CareM8 — Gift/Family CareM9 — Maintain/Upgrade ReminderM10 — Retention Event CareM11 — Pre-cycle / WinbackM12 — Annual Review

CRM không được gửi nếu:

Khách opt-out.

Có open case.

Message fatigue exceeded.

Quiet hour fail.

Missing customer memory.

Missing Product Effectiveness khi có gợi ý sản phẩm.

Missing member lifecycle runtime khi nói hạng/quyền lợi.

ClaimGuard fail.

FinalGuard fail.

CRM 12-Month Lifecycle là P0 dependency.


## 10. MEMBER LIFECYCLE PRINCIPLE

Member không chỉ là discount.

Member Lifecycle gồm:

Chu kỳ 12 tháng.

Doanh số hợp lệ.

Silver / Gold / Platinum / Diamond.

Quyền lợi theo hạng.

Duy trì hạng.

Nâng hạng.

Ân hạng.

Hạ hạng.

Tranh chấp hạng.

Outcome logger.

CRM chăm sóc thành viên.

Hệ thống phải resolve:

member_tier_displayaccumulated_valid_revenue_displayamount_to_maintain_tier_displayamount_to_next_tier_displaynext_member_tier_displaydays_to_cycle_end_displaygrace_statusdays_left_in_grace_display

AI không được:

Tự tính hạng.

Tự tính doanh số.

Tự tính số còn thiếu.

Tự nói ngày còn lại.

Tự hạ hạng.

Tự nâng hạng.

Tự bù quyền lợi.

Tự xử lý tranh chấp.

Hạ hạng phải nói nhẹ, không làm khách xấu hổ.

Member dispute phải mở case, không tranh luận.

Member Lifecycle là P0 dependency.


## 11. PRODUCT RECOMMENDATION PRINCIPLE

AI không được gợi ý sản phẩm ngẫu nhiên.

AI không được dùng cùng một bộ sản phẩm cho mọi khách.

AI không được chỉ gợi ý một sản phẩm nếu ngữ cảnh yêu cầu deep recommendation.

Gợi ý sâu mặc định phải theo:

01 sản phẩm theo mùa01 sản phẩm chức năng01 sản phẩm bổ dưỡng01 sản phẩm chăm sóc người thân theo giới/ngữ cảnh gia đình

Mỗi sản phẩm phải có:

Product public name.

Eastern effectiveness summary.

Hero ingredients.

Ingredient synergy effect.

Suitable context.

Safety boundary.

AI phải biết tư vấn theo:

Cha.

Mẹ.

Chồng.

Vợ.

Con.

Người lớn tuổi.

Gia đình.

Văn phòng.

Sinh viên.

Du học sinh.

Việt kiều.

Công ty.

Trường học.

Bệnh viện / khu chăm sóc.

Quà biếu.

Công ty mua cho nhân viên.

Đối tác.

HR / Admin / Procurement.

Người bận rộn.

Người ở xa.

Product Recommendation phải qua Product Knowledge, Claim Policy, Availability, Pricing và Order Core khi chuyển sang quote/order.

Product Recommendation là P0 dependency.


## 12. SEGMENT RECOMMENDATION PRINCIPLE

Segment Recommendation là dependency chính thức.

AI, CRM, ADS và Landing Page phải phân biệt các segment:

Cá nhân dùng hằng ngày.

Cơ quan / đoàn thể.

Quà sếp.

Quà đối tác.

Quà thăm bệnh.

Quà doanh nghiệp.

Người đi công tác.

Khách mua số lượng lớn.

Khách mua gửi cho người thân.

Segment Recommendation phải đi qua:

SegmentRecommendationResolverProductKnowledgeResolverClaimPolicyResolverAvailabilityResolverPricingResolverShippingEligibilityResolver nếu có giao hàngOrderDraftResolver nếu chuyển sang đơn

Không được mặc định công ty / trường học / bệnh viện là đại lý.

Không được hứa giao quốc tế nếu Shipping / Export Eligibility chưa pass.

Không được quote số lượng lớn nếu chưa có Bulk / Corporate Policy.

Segment Recommendation là P0 dependency.


## 13. DIAMOND / AFFILIATE / DISTRIBUTOR BOUNDARY PRINCIPLE

MASTER-02 khóa ranh giới:

Diamond / Affiliate / Khởi nghiệp!= Đại lý!= Nhà phân phối!= Mua sỉ!= Công ty mua quà!= Khách mua lẻ

Affiliate / khởi nghiệp gắn với Diamond.

Đại lý / nhà phân phối / mua sỉ đi theo Partner / Distributor / Wholesale Policy.

Gộp Diamond với đại lý.

Gộp khách mua số lượng lớn với nhà phân phối.

Hứa hoa hồng nếu commission runtime chưa xác nhận.

Tự nói khách đủ điều kiện khởi nghiệp nếu chưa có runtime.

Tự gửi sai số điện thoại.

Tự tính hoa hồng.

Public commission/payout khi không đúng surface.

Tự xử lý dispute hoa hồng.

Diamond / Affiliate / Distributor Boundary là P0 dependency.


## 14. MESSAGE TRIGGER / IMMUTABLE TEXT PRINCIPLE

Tin nhắn không được sinh tự do.

Mỗi tin nhắn phải có:

Trigger.

Đối tượng.

Hạng thành viên nếu liên quan.

Kênh gửi.

Điều kiện gửi.

Điều kiện chặn.

Nội dung đã duyệt.

Snapshot biến.

Dedup.

Log.

Không được:

Tự tạo trigger mới.

Gửi sai hạng.

Gửi trùng trong tháng nếu rule cấm.

Rewrite/paraphrase nội dung đã duyệt.

Gửi trên kênh chưa có lịch sử tương tác hoặc chưa được phép.

Gửi khi opt-out.

Gửi khi open case.

Gửi khi fatigue fail.

Gửi ngoài quiet hours nếu policy không cho.

Gửi email sai event.

Message Trigger / Immutable Text là P0 dependency.


## 15. PUBLIC COMMENT / MESSENGER HANDOFF PRINCIPLE

Public comment / live comment là public surface.

Báo final price.

Quote line-by-line.

Xác nhận đơn.

Lặp số điện thoại.

Xin địa chỉ.

Gửi tài khoản.

Tư vấn bệnh dài.

Gửi invoice detail.

Public Diamond commission.

Chốt đơn public.

Các tình huống phải kéo vào Messenger / private nếu policy cho phép:

Hỏi giá.

Muốn mua.

Chốt / lấy hàng.

Để số điện thoại.

Để địa chỉ.

Hỏi thanh toán.

Hỏi hóa đơn.

Hỏi bệnh / kiêng cữ / sức khỏe nhạy cảm.

Hỏi Diamond / referral.

Cần tư vấn sâu.

Handoff success mới được nói đã gửi/chuyển Messenger.

Handoff fail không được nói đã gửi.

Không được kêu khách tự nhắn Messenger nếu flow cho phép hệ thống chủ động handoff.

Public Comment / Messenger Handoff là P0 dependency.


## 16. HEALTH BOUNDARY PRINCIPLE

Nếu khách không nêu bệnh:

AI không được tự hỏi:

Mình có bệnh gì không?

Có kiêng cữ gì không?

Đang điều trị gì không?

Có dùng thuốc gì không?

Không được kéo hội thoại thực dưỡng thông thường sang y khoa.

Nếu khách nêu bệnh hoặc tình trạng sức khỏe:

Bật HealthSafetyGuard.

Không quote/order nếu health unresolved.

Tư vấn theo hướng bữa ăn thực dưỡng.

Không nói chữa bệnh.

Không thay thuốc.

Không cam kết khỏi bệnh.

Không khuyên ngưng thuốc.

Không tư vấn chi tiết ở public comment.

Health Boundary là P0 dependency.


## 17. BRAND SOUL / SCIENTIFIC EVIDENCE PRINCIPLE

MASTER-02 khóa brand và khoa học như dependency.

Các trục bắt buộc:

“Ngon như Mẹ nấu — thương ngay từ vị đầu tiên”.

Sâm Savigin / Vietnam Ocean Ginseng.

Sản phẩm khoa học.

Bài báo khoa học quốc tế nếu Evidence Registry approved.

Y thực đồng nguyên kết hợp kiến thức ẩm thực phương Đông.

Không thuốc hóa.

Không bịa link/kết luận khoa học.

Không hạ thấp bằng chứng khoa học đã có thành “tài liệu nội bộ”.

Không bán củ Sâm Savigin / giống Sâm Savigin nếu đã khóa không bán.

Không dùng claim khoa học để hứa điều trị.

Không dùng mã SKU nội bộ với khách.

Brand Soul / Scientific Evidence là P0 dependency.


## 18. OFFICIAL CONTACT REGISTRY PRINCIPLE

Số điện thoại không được là text tự do trong AI.

MASTER-02 khóa Official Contact Registry.

Các contact purpose phải có source:

Tham quan / sắp lịch / đón tiếp.

Liên hệ công ty.

Gặp / trao đổi với lãnh đạo.

Xin số cá nhân lãnh đạo.

Đại lý.

Nhà phân phối.

Hợp tác kinh doanh.

Mua số lượng nhiều.

Affiliate / Diamond / khởi nghiệp.

CSKH / khiếu nại nếu có.

Nếu thiếu OfficialContactResolver:

AI không được gửi số điện thoại.Gateway không được public số điện thoại.CRM không được gửi contact.Landing Page không được hardcode contact.

Official Contact Registry là P0 dependency.


## 19. PROGRAM / SELLABLE / PRODUCT ACTIVATION PRINCIPLE

Quy tắc mở bán là P0 domain riêng.

Không được để rule mở bán nằm rải rác trong Pricing / Availability / Program.

MASTER-02 khóa domain:

PROGRAM / SELLABLE / PRODUCT ACTIVATION DEPENDENCY

Domain này gồm 6 lớp.


### 19.1. Base Sellable Gate

SKU chỉ được mở bán khi:

Thành phẩm đã hoàn tất sản xuất.Thành phẩm đã nhập kho hợp lệ.stock_available > 0.sellable_status = SELLABLE.listed_price_status = ACTIVE.Không quality hold.Không recall hold.Không sale lock.Không channel block.Runtime Core cho phép bán.

Nếu thiếu bất kỳ điều kiện nào, SKU không được mở bán.


### 19.2. Auto Stop Sale Gate

SKU tự động ngưng bán khi:

Hết hàng.stock_available = 0.quality hold.recall hold.sale lock.channel block.listed_price inactive.product hidden.sellable_status != SELLABLE.

Khi SKU không sellable:

Không báo quote.Không tạo đơn.Không public tồn kho.Không nói “chỉ còn vài hộp”.Không gợi ý thay thế nếu sản phẩm thay thế chưa qua guard.


### 19.3. Program 24/7 Activation

24/7 phải theo ProgramPolicyResolver:

SKU mở bán lần đầu -> giảm 9%.successful_sales_count < 50/ngày -> giảm 9% ngày tiếp theo.successful_sales_count >= 50/ngày -> giảm 5% ngày tiếp theo.

Chỉ tính đơn hợp lệ.

Không tính:

Quote.

Giỏ hàng.

Đơn nháp.

Đơn chưa xác nhận.

Đơn hủy.

Đơn hoàn.

Đơn test.

Đơn lỗi.

Đơn trùng.


### 19.4. Golden Hour Activation

Giờ Vàng phải theo GoldenHourResolver và Availability:

sellable_stock < 300 -> không mở Giờ Vàng.300 <= sellable_stock < 500 -> mở 1 phiên tối.sellable_stock >= 500 -> mở 2 phiên/ngày.session_quota = 2.000 sản phẩm / phiên.Không bán vượt tồn kho khả dụng.

Phiên Giờ Vàng chỉ được kích hoạt nếu:

SKU sellable.

Có stock_available hợp lệ.

Có listed price active.

Có program eligibility.

Không quality hold.

Không recall hold.

Không sale lock.

Không channel block.

Runtime Core cho phép bán.


### 19.5. Channel / Board / AI Activation

AI, Gateway, CRM, Landing Page, ADS, MC AI chỉ được nói hoặc bán SKU khi SKU:

Active trong chương trình/kênh.

Sellable.

Có giá active.

Có content/claim public-safe.

Không ngoài board live nếu đang ở live board.

Không ngoài program active list.

Không bị hold/recall/lock.

MC AI không được nói SKU ngoài Daily Live Product Board.

ADS không được chạy SKU ngoài active list nếu campaign là campaign bán hàng theo chương trình.

CRM không được gửi SKU không active nếu message gắn chương trình.

Landing Page không được hardcode danh sách chương trình.

Gateway không được tự chọn SKU chương trình.


### 19.6. Sales / Stock Movement -> Production Planning Signal

Dữ liệu bán hàng và tồn kho phải tạo tín hiệu sản xuất:

SKU bán nhanh.SKU tồn cao.SKU active trong 24/7.SKU active trong Giờ Vàng.SKU có tốc độ giảm tồn cao.SKU cần sản xuất ghi thêm.SKU cần đẩy bán trước khi sản xuất mới.

Production Planning Signal không tự động thành Production Order nếu chưa qua Operational Core / Owner Review.

Program / Sellable / Product Activation là P0 dependency.


## 20. PAYMENT / BANK TRANSFER PRINCIPLE

Payment phải đi qua Payment Core và Accounting Review.

Khi khách chọn chuyển khoản:

payment_method = BANK_TRANSFERpayment_channel = VIETQR_OR_BANK_ACCOUNTpayment_status = BANK_TRANSFER_WAITINGaccounting_review_status = WAITING_BANK_TRANSFERpayment_reference = unique_payment_referencepayment_verification_source = WAITING_RECONCILIATION

AI chỉ được nói đã thanh toán khi Payment Core trả PAID hoặc PAID_BY_BANK_TRANSFER.

Ảnh giao dịch không tự động bằng đã thanh toán.

Khách nói “đã chuyển khoản” không tự động bằng PAID.

Kế toán xác nhận thủ công phải có audit.

Payment / Bank Transfer là P0 dependency.


## 21. SHIPPING / TRACKING PRINCIPLE

Shipping phải theo Shipping Core.

Hệ thống chỉ được nói:

ETA.

COD.

Phí ship.

Tracking.

Carrier status.

Fallback delivery time.

khi Shipping Core hoặc resolver tương ứng trả kết quả.

Nếu có tracking realtime -> trả trạng thái/link.

Nếu không có tracking -> fallback theo vùng nếu policy cho phép.

Không hỏi lại địa chỉ nếu order đã có shipping info.

Không bịa trạng thái vận chuyển.

Không cam kết ngày nhận tuyệt đối nếu chưa có carrier tracking.

Shipping / Tracking là P0 dependency.


## 22. PRIORITY CONFLICT / SUPPRESSION PRINCIPLE

Các case sau thắng CRM / member sales / upsell:

ComplaintRefundPrivacyPayment disputeDelivery issueHealth reviewCounterfeitQuality caseMember disputeCommission dispute

Nếu có open case:

Không gửi CRM bán hàng.

Không nhắc duy trì/nâng hạng.

Không upsell.

Không gửi Diamond commission promo.

Không tranh luận.

Không tự sửa hạng/quyền lợi.

Mở case / human review.

Priority Conflict / Suppression là P0 dependency.


## 23. CROSS-CHANNEL DEDUP / OUTCOME LOGGER PRINCIPLE

Không gửi cùng message family đa kênh trong 12 giờ.

Không gửi cùng trigger nhiều lần.

Không gửi lại message đã gửi trong tháng nếu rule không cho.

Member lifecycle outcome phải log:

MAINTAIN_SUCCESSMAINTAIN_RISKGRACE_STARTEDGRACE_RECOVEREDDOWNGRADEDUPGRADE_SUCCESSMEMBER_RETAINEDMEMBER_WINBACKMEMBER_DISPUTE_OPENEDMEMBER_CRM_SUPPRESSED

Outcome dùng cho dashboard / BI / learning có kiểm soát.

Outcome không public.

Outcome không dùng để ép khách.

Cross-channel Dedup / Outcome Logger là P0 dependency.


## 24. LIVE MODERATION / BRAND RISK PRINCIPLE

Comment chửi thề, troll, brand attack, phá live phải được xử lý qua moderation.

Nguyên tắc:

Hide / no reply / no Messenger khi là abuse rõ.

Không kéo troll vào Messenger.

Không đôi co.

Không quote/order.

Không mở CRM.

Không auto blacklist khi là complaint thật.

Nếu có mã đơn/mã lô/bằng chứng -> chuyển CSKH/case.

Có fuzzy matching tiếng Việt.

Có moderation evidence.

Live Moderation / Brand Risk là P0 dependency.


## 25. COMPLETION REPORT / GATEWAY RELEASE PRINCIPLE

Completion Report là release gate, không phải tài liệu tham khảo.

Completion Report mặc định:

Gateway chỉ được xem xét production khi:

GATE-01 đến GATE-12 đều PASS.

Không còn P0 open issue.

Có evidence cho từng P0.

Có DecisionEnvelope.

Có resolver trace.

Có guard trace.

Có handoff delivery log.

Có QuoteSnapshot.

Có order_code.

Có duplicate/idempotency test.

Có CRM suppression evidence.

Có ROAS internal-only evidence.

Có E2E smoke PASS.

Có owner sign-off.

Không được gọi “Ready” chỉ vì tài liệu đã viết xong.

Không được gọi PASS nếu chưa có evidence.

Completion Report / Gateway Release là P0 dependency.


## 26. SOURCE BOUNDARY MODEL

MASTER-02 sử dụng 4 lớp source boundary.


### 26.1. Governance Source

Governance Source quyết định rule cấp cao.

Bao gồm:


## Master-00.


## Master-01.


## Master-02.

Completion Report.

Evidence Standard.

Smoke Matrix.

Owner decision log.

Governance Source không thay thế runtime.

Governance Source khóa rule, boundary, condition, dependency, done gate.


### 26.2. Runtime Decision Source

Runtime Decision Source quyết định trạng thái tại thời điểm chạy.

Pricing.

Program.

Availability.

Payment.

Shipping.

Order State.

IVR.

CRM Eligibility.

Message Trigger.

Open Case.

Handoff Status.

Completion Evidence.

Consumer không được tự tính thay Runtime Decision Source.


### 26.3. Approved Content / Public-Safe Source

Approved Content Source quyết định câu chữ được render.

Product Knowledge.

Claim Policy.

Brand Voice.

Scientific Evidence.

Approved Publication Link.

Message Text Immutable Source.

Official Contact Registry.

Content Block Registry.

Content không quyết định runtime.

Content chỉ render khi runtime/guard pass.


### 26.4. Evidence Source

Evidence Source quyết định release.

Evidence item.

DecisionEnvelope.

Resolver trace.

Guard trace.

QuoteSnapshot sample.

OrderDraft sample.

OrderCode sample.

Handoff delivery log.

CRM job log.

Message send log.

Dedup log.

Outcome log.

Completion gate status.

Owner sign-off.

Thiếu evidence thì không release.


## 27. NO-HARDCODE BOUNDARY

Không được hardcode dữ liệu quyết định trong AI, Gateway, CRM, Landing Page, Admin UI, ADS, MC AI, content block, frontend component hoặc static template.

Không được hardcode:

Giá.

Chương trình.

Quyền lợi thành viên.

Hoa hồng Diamond.

Buyer link Diamond.

Hạng thành viên.

Số còn thiếu để nâng/duy trì hạng.

Ngày còn lại trong chu kỳ.

Grace period.

CRM trigger.

Message text.

Message send channel.

Số điện thoại chính thức.

Link bài báo khoa học.

Payment reference.

Bank account.

Shipping ETA.

SKU active trong 24/7.

SKU active trong Giờ Vàng.

Live board SKU.

Completion PASS.

Gateway PASS.

Production Ready.

IVR result interpretation.

Order state.

PAID state.

Recall / hold status.

Public trace field.

Permission.

Audit bypass.

No-hardcode không có nghĩa không được hiển thị dữ liệu.

No-hardcode có nghĩa dữ liệu hiển thị phải đến từ source / resolver / core được phép.


## 28. FALLBACK PRINCIPLE

Fallback không phải suy đoán.

Fallback chỉ được dùng khi:

Có policy cho phép.

Không tạo cam kết sai.

Không thay thế source-of-truth.

Không thay thế runtime resolver.

Không bypass guard.

Không ảnh hưởng giá, đơn, thanh toán, shipping, hạng, quyền lợi, claim, order state hoặc release gate.

Không được fallback bằng suy đoán cho:

Diamond benefit.

Commission.

PAID.

Sellable status.

Product active trong chương trình.

IVR state.

Health-sensitive recommendation.

Scientific evidence.

Nếu thiếu dữ liệu, phải chặn hoặc hỏi thêm trong phạm vi an toàn.


## 29. EVIDENCE PRINCIPLE

Mọi dependency P0 phải có evidence.

Evidence có thể gồm:

Runtime result.

OrderCode.

Payment record.

Bank transfer queue.

Accounting review audit.

Shipping tracking record.

Moderation action log.

Completion gate evidence.

Không dùng:

Lời xác nhận miệng.

Screenshot câu trả lời hay.

Nội dung tài liệu đã viết.

Demo rời rạc.

Log thiếu source.

Test không có trace.

UI nhìn có vẻ chạy.

để thay thế evidence P0.


## 30. PART 1 DONE GATE


## PHẦN 1/4 đạt khi:

Đã xác định vai trò MASTER-02 là Cross-Pack Dependency Governance.

Đã xác định MASTER-02 không viết implementation chi tiết.

Đã khóa Source-of-Truth quyết định dữ liệu.

Đã khóa Owner Core quyết định nghiệp vụ.

Đã khóa Consumer chỉ tiêu thụ trong boundary.

Đã khóa Runtime Resolver cho quyết định runtime.

Đã khóa Business-rule-driven dependency model.

Đã khóa Customer Context First.

Đã khóa Customer Memory / Care Before Sale.

Đã khóa CRM 12 tháng là dependency chính thức.

Đã khóa Member Lifecycle là dependency chính thức.

Đã khóa Product Recommendation 3 trụ + người thân.

Đã khóa Segment Recommendation.

Đã khóa Diamond / Affiliate / Distributor separation.

Đã khóa Message Trigger / Immutable Text.

Đã khóa Public Comment -> Messenger Handoff.

Đã khóa Health Boundary.

Đã khóa Brand Soul / Scientific Evidence.

Đã khóa Official Contact Registry.

Đã khóa Program / Sellable / Product Activation là P0 domain riêng.

Đã khóa Payment / Bank Transfer.

Đã khóa Shipping / Tracking.

Đã khóa Priority Conflict.

Đã khóa Dedup / Outcome Logger.

Đã khóa Live Moderation.

Đã khóa Completion Report / Gateway Release Gate.

Đã khóa Source Boundary Model.

Đã khóa No-hardcode Boundary.

Đã khóa Fallback Principle.

Đã khóa Evidence Principle.

Không gọi Gateway PASS.

Không gọi Production Ready.

Không cho consumer tự suy luận rule kinh doanh.


## 31. TRẠNG THÁI SAU PHẦN 1/4


## 32. PHẦN TIẾP THEO

Phần tiếp theo:


## PHẦN 2/4 — CROSS-PACK DEPENDENCY REGISTRY BY BUSINESS DOMAIN / OWNER CORE / CONSUMER PACK / bị chặn-IF-MISSING RULE


## PHẦN 2/4 sẽ khóa registry dependency theo 16 domain:


## 01. Governance / Source-of-Truth02. Product Knowledge / Claim / Brand / Scientific Proof03. Customer Identity / Customer Memory04. Segment Recommendation / Product Recommendation05. CRM 12-Month / Message Trigger / Suppression06. Member Lifecycle / Rights / Downgrade / Dispute07. Diamond / Affiliate / Entrepreneurship / Distributor Boundary08. Pricing / Program / 24/7 / Golden Hour / QuoteSnapshot09. Program / Sellable / Product Activation / Production Signal10. Payment / Bank Transfer / Accounting Review11. Shipping / Tracking / ETA / COD12. Order / OrderDraft / CustomerConfirmation / IVR13. Gateway / Public Comment / Messenger Handoff / Moderation14. Official Contact / Phone Number Registry15. Health Boundary / Complaint / Priority Conflict16. Evidence / Completion Report / Gateway Gate / Security


## 0. MỤC ĐÍCH CỦA PHẦN 2/4


## PHẦN 2/4 khóa registry dependency theo từng business domain của hệ thống Ginsengfood.

Mỗi domain trong phần này xác định rõ:

Source-of-Truth nào quyết định.

Owner Core / Owner Pack nào chịu trách nhiệm.

Consumer Pack nào được phép tiêu thụ.

Consumer được làm gì.

Consumer bị cấm làm gì.

Runtime Resolver / Guard nào bắt buộc.

Thiếu dữ liệu thì phải chặn gì.

Evidence nào phải có.

Done Gate nào bắt buộc.

Pack chi tiết nào triển khai sâu.


## PHẦN 2/4 không viết API, DTO, DB, UI, worker, code hoặc template chi tiết.


## PHẦN 2/4 chỉ khóa dependency governance để các pack chi tiết không tự suy luận rule kinh doanh.


## 1. CHUẨN ĐỌC MỖI DOMAIN DEPENDENCY

Mỗi domain dependency trong MASTER-02 phải được đọc theo cấu trúc:

Domain IDDomain NameBusiness PurposeSource IDOwner Core / Owner PackConsumer PackDependency TypeRequired Resolver / GuardAllowed UseForbidden UseBlocked If MissingEvidence RequiredDone GatePack Detail Mapping


### 1.1. Domain ID

Domain ID dùng để định danh nhóm dependency ở tầng governance.

DEP-DOM-01-GOVDEP-DOM-02-PROD-CLAIM-BRANDDEP-DOM-03-CUSTOMERDEP-DOM-04-RECOMMENDATION

Domain ID không phải API endpoint.

Domain ID không phải database table.

Domain ID không phải service class.

Domain ID là mã governance để trace rule, source, pack và evidence.


### 1.2. Source ID

Source ID là nguồn sự thật của dữ liệu hoặc rule.

Source ID có thể thuộc:

Master governance.

Runtime Core.

Customer Profile.

CRM.

Order.

Gateway.

Evidence.

Security.

Consumer không được tự tạo Source ID mới.

Nếu một domain cần Source ID chưa có trong MASTER-01, MASTER-02 phải khai báo source đó ở tầng governance để pack chi tiết triển khai sau.


### 1.3. Owner Core / Owner Pack

Owner Core là nơi có quyền quyết định dữ liệu hoặc trạng thái.

Customer Profile Core quyết định customer identity.

Customer Memory Core quyết định lịch sử mua và trải nghiệm khách.

Member Lifecycle Core quyết định nâng hạng, duy trì, ân hạng, hạ hạng.

Commerce Pricing Core quyết định giá.

Program Policy Core quyết định 24/7 và Giờ Vàng.

Commerce Availability Core quyết định sellable.

Payment Core quyết định thanh toán.

Shipping Core quyết định giao hàng.

Order Core quyết định order state.

Gateway Pack quyết định normalized event / handoff status.

Evidence Registry quyết định completion evidence.

Security Pack quyết định permission/audit.

Consumer không được vượt quyền owner.


### 1.4. Consumer Pack

Consumer Pack là pack được phép tiêu thụ dữ liệu từ owner.

Consumer có thể là:

AI Advisor.

Messaging.

Landing Page.

ADS.

MC AI.

Admin UI.

Order Core.

Payment Core.

Shipping Core.

MISA.

Evidence Pack.

BI / Dashboard.

Consumer chỉ được dùng dữ liệu trong phạm vi allowed use.

Consumer không được biến dữ liệu tiêu thụ thành source-of-truth mới.


### 1.5. bị chặn-if-missing

Mỗi domain P0 phải có bị chặn-if-missing.

Thiếu source -> không khẳng định.Thiếu resolver -> không hứa.Thiếu guard -> không hành động.Thiếu evidence -> không release.Thiếu permission -> không protected action.Thiếu owner approval -> không active scope.


## 2. DOMAIN REGISTRY OVERVIEW

MASTER-02 khóa 16 business domain dependency chính:


## 3. DEP-DOM-01-GOV — GOVERNANCE / SOURCE-OF-TRUTH DEPENDENCY


### 3.1. Business Purpose

Domain này khóa nguồn quản trị cấp cao của toàn hệ thống.

Mục tiêu là đảm bảo mọi pack chi tiết đều bám theo source-of-truth, không tạo nguồn song song, không tự suy luận rule và không bypass governance.


### 3.2. Source ID

SRC-GOV-001 — MASTER GOVERNANCE SOURCESRC-EVD-001 — EVIDENCE REGISTRY SOURCESRC-SEC-001 — SECURITY / PERMISSION / AUDIT SOURCESRC-COMP-001 — COMPLETION REPORT SOURCE


### 3.3. Owner Core / Owner Pack

Master Governance OwnerEvidence Registry OwnerSecurity / Permission / Audit OwnerRelease Governance OwnerOwner Sign-off Authority


### 3.4. Consumer Pack

All packsAI AdvisorGatewayCRMCommerce CoreOperational CorePayment CoreShipping CoreOrder CoreAdmin UILanding PageADSMC AIEvidence PackCompletion Report


### 3.5. Dependency Type


## Governancesource_Of_Truthrelease_Gateevidencesecurity


### 3.6. Required Resolver / Guard

SourceOfTruthResolverGovernanceConflictResolverEvidenceRegistryResolverReleaseGateResolverOwnerSignOffResolverSecurityPermissionResolverAuditResolver


### 3.7. Allowed Use

Consumer được dùng Governance Source để:

Xác định source-of-truth.

Xác định owner core.

Xác định consumer boundary.

Xác định rule nào là P0.

Xác định dependency nào chặn release.

Xác định evidence bắt buộc.

Xác định pack nào được phép triển khai chi tiết.

Xác định trạng thái Gateway / Completion / Production / Release / Go-live.


### 3.8. Forbidden Use

Consumer không được:

Tạo source-of-truth song song.

Tự sửa rule governance.

Tự gọi dependency PASS khi thiếu evidence.

Tự mở Gateway.

Tự gọi production-ready.

Tự bỏ qua source boundary.

Tự dùng tài liệu content thay runtime.

Tự dùng screenshot thay evidence.

Tự bypass permission/audit.


### 3.9. bị chặn If Missing

Nếu thiếu Governance Source:

Pack chi tiết không được tự quyết rule.

Không được release.

Không được Gateway PASS.

Không được Production Ready.

Không được Go-live.

Nếu thiếu Evidence Registry:

Release giữ NOT APPROVED.

Gateway giữ bị chặn.


### 3.10. Evidence Required

EVD-GOV-001 — Source-of-truth mappingEVD-GOV-002 — Owner core mappingEVD-GOV-003 — Consumer boundary mappingEVD-GOV-004 — Governance conflict resolutionEVD-GOV-005 — Release gate statusEVD-GOV-006 — Owner sign-off record


### 3.11. Done Gate

PASS khi mọi pack chi tiết đều trace được rule về source-of-truth, không có source song song, không có consumer vượt quyền owner, và mọi release state dựa trên evidence.


### 3.12. Pack Detail Mapping

MASTER-00MASTER-01MASTER-02Evidence PackSecurity PackCompletion ReportRelease Governance


## 4. DEP-DOM-02-PROD-CLAIM-BRAND — PRODUCT KNOWLEDGE / CLAIM / BRAND / SCIENTIFIC PROOF DEPENDENCY


### 4.1. Business Purpose

Domain này khóa tri thức sản phẩm, claim, brand voice, bằng chứng khoa học và Product Effectiveness.

Mục tiêu là để AI, CRM, Landing Page, ADS, Gateway và MC AI tư vấn đúng sản phẩm, đúng hồn thương hiệu, đúng claim, không thuốc hóa và không bịa bằng chứng khoa học.


### 4.2. Source ID

SRC-PROD-001 — PRODUCT KNOWLEDGE MASTERSRC-CLAIM-001 — CLAIM WHITELIST SOURCESRC-BRAND-001 — BRAND VOICE SOURCESRC-SCI-001 — SCIENTIFIC EVIDENCE SOURCESRC-PUB-001 — APPROVED PUBLICATION LINK SOURCESRC-EVD-001 — EVIDENCE REGISTRY SOURCE


### 4.3. Owner Core / Owner Pack

Product Knowledge PackClaim Policy PackBrand Voice PackScientific Evidence RegistryApproved Public Link RegistryContent Approval Pack


### 4.4. Consumer Pack

AI AdvisorGatewayCRMLanding PageADSMC AIContent BlocksPublic Trace if applicableAdmin Preview


### 4.5. Dependency Type

PRODUCT_KNOWLEDGECLAIM_GUARDBRAND_VOICESCIENTIFIC_PROOFPUBLIC_SAFECONTENT_RENDER


### 4.6. Required Resolver / Guard

ProductKnowledgeResolverProductEffectivenessResolverProductPublicNameGuardClaimPolicyResolverClaimGuardBrandVoiceResolverScientificEvidenceResolverApprovedPublicationLinkResolverSaviginScienceClaimGuardFinalGuard


### 4.7. Allowed Use

Consumer được dùng domain này để:

Tư vấn đúng sản phẩm.

Render tên sản phẩm public đầy đủ.

Giải thích Product Effectiveness.

Giữ trục “Ngon như Mẹ nấu — thương ngay từ vị đầu tiên”.

Trình bày Sâm Savigin / Vietnam Ocean Ginseng đúng bằng chứng.

Nói về sản phẩm khoa học khi evidence approved.

Gửi link bài báo khoa học nếu ApprovedPublicationLinkResolver pass.

Dùng y thực đồng nguyên kết hợp kiến thức ẩm thực phương Đông trong phạm vi public-safe.

Render content CRM, combo, proposal, quote line reason nếu đủ Product Effectiveness.


### 4.8. Forbidden Use

Tự tạo claim mới.

Tự tạo hiệu dụng mới.

Tự bịa bằng chứng khoa học.

Tự bịa link bài báo.

Tự nói sản phẩm chữa bệnh.

Tự diễn giải nghiên cứu thành claim điều trị.

Gọi sản phẩm bằng mã SKU nội bộ với khách.

Gọi tên sản phẩm rút gọn trong quote/order/CRM.

Bỏ Product Effectiveness khi gợi ý sản phẩm.

Bán củ Sâm Savigin hoặc giống Sâm Savigin nếu không thuộc scope bán.

Dùng “Ngon như Mẹ nấu” như tagline rời rạc, không gắn với bữa ăn/chăm sóc/gia đình.

Hạ claim khoa học đã approved thành “tài liệu nội bộ”.


### 4.9. bị chặn If Missing

Nếu thiếu ProductKnowledgeResolver:

AI không được gợi ý SKU cụ thể.

CRM không được gửi product recommendation.

Landing Page không được publish product block.

ADS không được chạy creative SKU cụ thể.

Nếu thiếu ClaimPolicyResolver:

Không được nói hiệu dụng cụ thể.

Không được render claim health/nourishment.

Không được chạy ADS claim.

Nếu thiếu ScientificEvidenceResolver:

Không được nói có bài báo khoa học cụ thể.

Không được gửi link bài báo.

Không được bịa tên bài/DOI/link/kết luận.

Nếu thiếu ProductEffectiveness:

Không được render CRM recommendation.

Không được render combo/growth proposal.

Không được render quote line reason nếu block yêu cầu.


### 4.10. Evidence Required

EVD-PROD-001 — Product public name fullEVD-PROD-002 — Product Effectiveness presentEVD-PROD-003 — ClaimGuard passEVD-PROD-004 — No treatment claimEVD-PROD-005 — Scientific evidence uses approved sourceEVD-PROD-006 — Approved publication link resolver usedEVD-PROD-007 — No internal SKU customer-facingEVD-PROD-008 — Brand voice rendered correctly


### 4.11. Done Gate

PASS khi mọi nội dung sản phẩm, claim, brand, science, CRM, combo, quote line và public wording đều lấy từ Product Knowledge / Claim / Brand / Scientific Evidence source, không bịa, không thuốc hóa, không hardcode và không dùng mã nội bộ với khách.


### 4.12. Pack Detail Mapping

FILE 02 — Product KnowledgeFILE 03 — Product Knowledge Schema / ViewsFILE 06 — Claim Whitelist / SafetyFILE 07 — Common Content BlocksFILE 08 — SKU ContentFILE 09 — Test MatrixFILE 10 — Situation RegistryFILE 12 — Sales BehaviorEvidence Registry


## 5. DEP-DOM-03-CUSTOMER — CUSTOMER IDENTITY / CUSTOMER MEMORY DEPENDENCY


### 5.1. Business Purpose

Domain này khóa việc AI, CRM, Pricing, Order, Diamond, Gateway và Messaging phải biết khách là ai trước khi tư vấn, báo quyền lợi, CRM, quote hoặc chốt đơn.


### 5.2. Source ID

SRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCESRC-CUSTMEM-001 — CUSTOMER MEMORY SOURCESRC-MEMBER-001 — MEMBER BENEFIT SOURCESRC-CRM-001 — CRM LIFECYCLE SOURCESRC-ORDER-003 — ORDER STATE MACHINE SOURCESRC-PRICE-001 — COMMERCE PRICING SOURCESRC-ORDER-001 — QUOTE SNAPSHOT SOURCESRC-CASE-001 — CUSTOMER CASE / COMPLAINT SOURCE


### 5.3. Owner Core / Owner Pack

Customer Profile CoreCustomer Memory CoreMember Benefit CoreOrder CoreCRM Lifecycle CoreCase Management CoreCommerce Pricing Core


### 5.4. Consumer Pack

AI AdvisorCRMGatewayCommerce PricingOrder CoreMessagingAdmin UIEvidence Pack


### 5.5. Dependency Type

CUSTOMER_IDENTITYCUSTOMER_MEMORYPERSONALIZATIONCARE_BEFORE_SALECRM_CONTEXTQUOTE_CONTEXT


### 5.6. Required Resolver / Guard

CustomerIdentityResolverCustomerStageResolverCustomerMemoryResolverLastPurchaseResolverLastPurchasedProductResolverRecipientMemoryResolverCustomerExperienceResolverProductFeedbackResolverServiceSatisfactionResolverCareBeforeNextSaleGuardOpenCaseStatusResolverOpenCaseBeforeSaleGuardCustomerConsentResolver


### 5.7. Allowed Use

Xác định khách mới hay khách cũ.

Xác định khách có phải member không.

Xác định lịch sử mua.

Xác định lần gần nhất mua sản phẩm gì.

Xác định mua cho ai nếu có dữ liệu.

Hỏi thăm trải nghiệm trước khi bán tiếp.

Cá nhân hóa CRM.

Cá nhân hóa tư vấn.

Xác định khách có open case không.

Chặn CRM/bán hàng khi có open case.

Gắn Customer Context vào quote/order nếu hợp lệ.


### 5.8. Forbidden Use

Bịa khách là khách cũ.

Bịa khách là member.

Bịa hạng khách.

Bịa lịch sử mua.

Bịa sản phẩm đã mua.

Bịa người nhận.

Bỏ qua hỏi thăm khách cũ.

Bán tiếp khi khách đang có open case.

Dùng lịch sử chat không xác thực thay Customer Memory.

Gửi CRM như khách cũ nếu CustomerMemoryResolver không trả dữ liệu.


### 5.9. bị chặn If Missing

Nếu thiếu CustomerIdentityResolver:

Không cá nhân hóa chắc chắn.

Không nói khách là member.

Không nói khách là Diamond.

Không áp quyền lợi member.

Không kích hoạt Diamond benefit.

Nếu thiếu LastPurchaseResolver:

Không nói “lần trước Mình mua…”.

Chỉ được hỏi thăm chung.

Nếu thiếu OpenCaseStatusResolver trong ngữ cảnh nhạy cảm:

Không member lifecycle sales.

Nếu khách phản hồi chưa hài lòng:

Không chốt đơn tiếp.

Chuyển sang CSKH / case flow.


### 5.10. Evidence Required

EVD-CUST-001 — New customer resolvedEVD-CUST-002 — Returning customer resolvedEVD-CUST-003 — Last purchase resolvedEVD-CUST-004 — Last recipient resolved if source existsEVD-CUST-005 — Care-before-sale renderedEVD-CUST-006 — Negative feedback opens care flowEVD-CUST-007 — Open case blocks salesEVD-CUST-008 — Unknown customer does not receive fake personalization


### 5.11. Done Gate

PASS khi AI/CRM/Gateway biết khách là ai trước khi cá nhân hóa; khách cũ được hỏi thăm trước khi bán tiếp; không bịa lịch sử; open case chặn bán hàng; Customer Memory được dùng đúng source.


### 5.12. Pack Detail Mapping

FILE 01 — Parent LogicFILE 04 — Resolver / GuardFILE 07 — Customer care wordingFILE 09 — Customer memory testsFILE 10 — Customer situationsFILE 12 — Human-level sales behaviorCRM / Customer Memory PackCase Management Pack


## 6. DEP-DOM-04-RECOMMENDATION — SEGMENT RECOMMENDATION / PRODUCT RECOMMENDATION DEPENDENCY


### 6.1. Business Purpose

Domain này khóa logic gợi ý sản phẩm theo nhu cầu, người nhận, segment, 3 trụ sản phẩm, gia đình, quà tặng, khách hàng tổ chức và Product Effectiveness.


### 6.2. Source ID

SRC-PROD-001 — PRODUCT KNOWLEDGE MASTERSRC-CLAIM-001 — CLAIM WHITELIST SOURCESRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCESRC-CUSTMEM-001 — CUSTOMER MEMORY SOURCESRC-CRM-001 — CRM LIFECYCLE SOURCESRC-AVAIL-001 — SELLABLE AVAILABILITY SOURCESRC-PRICE-001 — COMMERCE PRICING SOURCESRC-ORDER-001 — QUOTE SNAPSHOT SOURCESRC-SHIP-001 — SHIPPING ELIGIBILITY SOURCESRC-ADS-001 — ADS TRACKING SIGNAL SOURCE


### 6.3. Owner Core / Owner Pack

Product Knowledge PackAI Advisor Logic PackCRM Lifecycle CoreCustomer Memory CoreClaim Policy PackCommerce Availability CoreCommerce Pricing CoreShipping CoreADS Context Pack


### 6.4. Consumer Pack

AI AdvisorCRMGatewayLanding PageADSMC AI if live board context allowsOrder Core if converted to quote/order


### 6.5. Dependency Type

PRODUCT_RECOMMENDATIONSEGMENT_RECOMMENDATIONPRODUCT_TRIADFAMILY_ADDONGIFT_RECOMMENDATIONPUBLIC_SAFESALES_GUARD


### 6.6. Required Resolver / Guard

ProductKnowledgeResolverProductEffectivenessResolverProductPillarResolverThreePillarRecommendationResolverProductTriadFamilyAddOnResolverSegmentRecommendationResolverOfficeUseCaseResolverStudentUseCaseResolverInstitutionBuyerResolverCorporateProcurementResolverOverseasVietnameseContextResolverInternationalStudentContextResolverGiftIntentResolverGiftRecipientResolverBulkGiftRoutingGuardPillarAvailabilityFallbackResolverAvailabilityResolverPricingResolverQuoteSnapshotResolverClaimGuard


### 6.7. Allowed Use

Gợi ý sản phẩm theo nhu cầu khách.

Gợi ý 3 trụ: theo mùa, chức năng, bổ dưỡng.

Gợi ý thêm sản phẩm chăm sóc người thân nếu context có.

Gợi ý theo cha/mẹ/chồng/vợ/con/người lớn tuổi.

Gợi ý theo văn phòng/sinh viên/du học sinh/Việt kiều.

Gợi ý theo công ty/trường học/bệnh viện/cơ quan.

Gợi ý quà sếp/quà đối tác/quà doanh nghiệp/quà thăm bệnh.

Gợi ý sản phẩm theo Product Effectiveness.

Chuyển sang quote khi khách chọn sản phẩm và Pricing/Quote pass.

Chuyển sang OrderDraft khi khách có buying signal và đủ dữ liệu.


### 6.8. Forbidden Use

Gợi ý sản phẩm ngẫu nhiên.

Dùng cùng bộ sản phẩm cho mọi khách.

Gợi ý SKU mặn cho khách thuần chay.

Gợi ý SKU dị ứng với khách có allergy signal.

Gợi ý SKU health-sensitive khi health unresolved.

Gợi ý SKU not sellable nếu có ý định bán.

Báo giá khi thiếu QuoteSnapshot.

Gọi combo là ưu đãi giá nếu Pricing chưa trả.

Ép combo.

Nói “đổi món để không ngán”.

Mặc định công ty/trường học là đại lý.

Hứa giao quốc tế nếu Shipping/Export chưa pass.

Dùng ADS signal để ép SKU trái nhu cầu khách.


### 6.9. bị chặn If Missing

Không gợi ý SKU cụ thể.

Không render proposal.

Nếu thiếu ProductPillarResolver:

Không gợi ý 3 trụ.

Không phân loại seasonal/functional/nourishing.

Nếu thiếu SegmentRecommendationResolver:

Không cá nhân hóa sâu theo segment.

Chỉ được hỏi thêm ngắn.

Nếu thiếu Availability:

Không chốt bán.

Không mở quote.

Chỉ được tư vấn định hướng nếu an toàn.

Nếu thiếu Pricing/Quote:

Không báo giá.

Không gọi combo giá.


### 6.10. Evidence Required

EVD-RECO-001 — Seasonal product selectedEVD-RECO-002 — Functional product selectedEVD-RECO-003 — Nourishing product selectedEVD-RECO-004 — Family add-on selected when context existsEVD-RECO-005 — Office scenarioEVD-RECO-006 — Student scenarioEVD-RECO-007 — Overseas Vietnamese scenarioEVD-RECO-008 — Corporate gift scenarioEVD-RECO-009 — Hospital/care contextEVD-RECO-010 — Vegan guardEVD-RECO-011 — Allergy guardEVD-RECO-012 — Availability guardEVD-RECO-013 — Product Effectiveness per item


### 6.11. Done Gate

PASS khi AI/CRM/ADS/Landing Page gợi ý đúng sản phẩm theo Product Knowledge, segment, 3 trụ, người thân, claim, availability và pricing; không random, không ép combo, không chọn SKU sai guard.


### 6.12. Pack Detail Mapping

FILE 01 — Recommendation decisionFILE 02 — Product KnowledgeFILE 03 — Product schema/viewsFILE 04 — Resolvers/guardsFILE 07 — Proposal blocksFILE 08 — SKU contentFILE 09 — TestsFILE 10 — SituationsFILE 12 — Sales behaviorCRM / ADS / Landing Page Pack


## 7. DEP-DOM-05-CRM-MESSAGE — CRM 12-MONTH / MESSAGE TRIGGER / SUPPRESSION DEPENDENCY


### 7.1. Business Purpose

Domain này khóa CRM 12 tháng, message trigger, immutable text, đa kênh, dedup, suppression, quiet hours, frequency cap và CRM evidence.


### 7.2. Source ID

SRC-CRM-001 — CRM LIFECYCLE SOURCESRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCESRC-CUSTMEM-001 — CUSTOMER MEMORY SOURCESRC-ORDER-003 — ORDER STATE MACHINE SOURCESRC-PROD-001 — PRODUCT KNOWLEDGE MASTERSRC-MEMBER-001 — MEMBER BENEFIT SOURCESRC-MSG-001 — MESSAGE TRIGGER REGISTRY SOURCESRC-MSG-002 — MESSAGE TEXT IMMUTABLE SOURCESRC-MSG-003 — CHANNEL SEND POLICY SOURCESRC-SUPPRESS-001 — CRM SUPPRESSION POLICY SOURCESRC-DEDUPE-001 — CROSS-CHANNEL DEDUP SOURCE


### 7.3. Owner Core / Owner Pack

CRM Lifecycle CoreMessaging CoreCustomer Memory CoreMember Lifecycle CoreProduct Knowledge PackCRM Suppression CoreChannel Send Policy CoreDedup Core


### 7.4. Consumer Pack

CRMAI AdvisorMessagingMessengerZalo / SMS / Email / Instagram if configuredMember LifecycleEvidence PackBI / Dashboard


### 7.5. Dependency Type

CRM_LIFECYCLEMESSAGE_TRIGGERIMMUTABLE_TEXTMULTI_CHANNELSUPPRESSIONDEDUPOUTCOME_LOG


### 7.6. Required Resolver / Guard

CRMLifecycleResolverCRMEligibilityGuardCustomerMemoryResolverOrderHistoryResolverCRMProductRecommendationResolverProductEffectivenessResolverMessageTriggerResolverMessageTemplateResolverMessageTextImmutableGuardMessageTriggerOnlyGuardTierMessageEligibilityGuardMonthlyDuplicateGuardCrossChannelDedupGuardChannelPreferenceResolverQuietHoursGuardMessageFatigueGuardFrequencyCapGuardEmailRuleGuardCRMJobLoggerMessageSendLogger


### 7.7. Allowed Use

CRM/Messaging được dùng domain này để:

Chăm sóc khách sau mua.

Hướng dẫn sử dụng.

Hỏi trải nghiệm.

Gợi ý mua lại.

Gợi ý sản phẩm kế tiếp.

Chăm sóc theo mùa.

Chăm sóc member lifecycle.

Nhắc duy trì/nâng hạng nếu runtime cho phép.

Gửi tin theo trigger đã duyệt.

Gửi đúng kênh có lịch sử tương tác/xác thực.

Ghi log send / suppress / outcome.


### 7.8. Forbidden Use

CRM/Messaging không được:

Gửi khi message fatigue exceeded.

Gửi sai trigger.

Tự tạo trigger.

Rewrite message text đã duyệt.

Gửi trùng nội dung trong tháng nếu rule cấm.

Gửi cùng message family đa kênh trong dedup window.

Bịa quyền lợi member.

Bịa giá nếu thiếu QuoteSnapshot.

Gửi product recommendation thiếu Product Effectiveness.


### 7.9. bị chặn If Missing

Nếu thiếu CRMEligibilityGuard:

Không gửi CRM.

Không tạo CRM job.

Nếu thiếu MessageTriggerResolver:

Không gửi tin.

Không tạo message event.

Nếu thiếu MessageTextImmutableSource:

Không render message.

Không để AI tự viết lại.

Nếu thiếu ProductEffectiveness khi có product recommendation:

Không gửi recommendation.

Nếu thiếu MemberLifecycleRuntime khi nói hạng/quyền lợi:

Không gửi member lifecycle message.


### 7.10. Evidence Required

EVD-CRM-001 — CRM lifecycle triggerEVD-CRM-002 — CRM eligibility passEVD-CRM-003 — Opt-out suppressionEVD-CRM-004 — Open case suppressionEVD-CRM-005 — Quiet hour respectedEVD-CRM-006 — Frequency cap respectedEVD-CRM-007 — Message text not rewrittenEVD-CRM-008 — Correct tier messageEVD-CRM-009 — Same-month duplicate blockedEVD-CRM-010 — Cross-channel dedupEVD-CRM-011 — Product Effectiveness presentEVD-CRM-012 — Message send log


### 7.11. Done Gate

PASS khi CRM vận hành như chăm sóc vòng đời 12 tháng, không spam, không bịa, không gửi sai trigger/hạng/kênh, có suppression, có dedup, có Product Effectiveness và có log/evidence.


### 7.12. Pack Detail Mapping

FILE 01 — CRM decisionFILE 04 — CRM/message resolvers/guardsFILE 07 — CRM/message blocksFILE 09 — TestsFILE 10 — SituationsFILE 12 — CRM behaviorCRM PackMessaging PackEvidence Pack


## 8. DEP-DOM-06-MEMBER — MEMBER LIFECYCLE / RIGHTS / DOWNGRADE / DISPUTE DEPENDENCY


### 8.1. Business Purpose

Domain này khóa vòng đời thành viên, quyền lợi, nâng hạng, duy trì, ân hạng, hạ hạng, tranh chấp và outcome logging.


### 8.2. Source ID

SRC-MEMBER-001 — MEMBER BENEFIT SOURCESRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCESRC-CUSTMEM-001 — CUSTOMER MEMORY SOURCESRC-CRM-001 — CRM LIFECYCLE SOURCESRC-ORDER-003 — ORDER STATE MACHINE SOURCESRC-PRICE-001 — COMMERCE PRICING SOURCESRC-ORDER-001 — QUOTE SNAPSHOT SOURCESRC-CASE-001 — CUSTOMER CASE / COMPLAINT SOURCESRC-OUTCOME-001 — BUSINESS OUTCOME LOGGER SOURCE


### 8.3. Owner Core / Owner Pack

Member Lifecycle CoreMember Benefit CoreCustomer Profile CoreOrder CoreCRM Lifecycle CoreCommerce Pricing CoreCase Management CoreOutcome Logger


### 8.4. Consumer Pack

AI AdvisorCRMCommerce PricingOrder CoreMessagingAdmin UIBI / DashboardEvidence Pack


### 8.5. Dependency Type

MEMBER_LIFECYCLEMEMBER_RIGHTSTIER_UPGRADETIER_MAINTAINGRACE_PERIODDOWNGRADEDISPUTEOUTCOME_LOG


### 8.6. Required Resolver / Guard

MemberLifecycleResolverRuntimeRightsResolverMemberMaintainResolverMemberUpgradeResolverMemberGracePeriodResolverMemberDowngradeResolverMemberDisputeEscalationResolverMemberLifecycleOutcomeLoggerQuoteBenefitResolverCRMEligibilityGuardOpenCaseSuppressionGuard


### 8.7. Allowed Use

Nói hạng thành viên nếu runtime xác nhận.

Nói quyền lợi hạng nếu runtime xác nhận.

Nói doanh số tích lũy hợp lệ nếu runtime xác nhận.

Nói số còn thiếu để duy trì/nâng hạng nếu runtime xác nhận.

Nói ngày còn lại trong chu kỳ nếu runtime xác nhận.

Nói grace period nếu runtime xác nhận.

Chăm sóc duy trì/nâng hạng.

Hạ hạng an toàn nếu runtime xác nhận.

Áp quyền lợi vào QuoteSnapshot nếu Pricing Core cho phép.

Ghi outcome lifecycle.


### 8.8. Forbidden Use

Tự áp discount.

Tự cộng dồn quyền lợi.

Gửi member message khi thiếu runtime.

Dọa khách hạ hạng.

Tranh luận khi khách dispute.

Tiếp tục upsell khi đang member dispute.


### 8.9. bị chặn If Missing

Nếu thiếu MemberLifecycleResolver:

Không nói maintain/upgrade/downgrade.

Không gửi member lifecycle CRM.

Không nói số còn thiếu.

Không nói grace period.

Nếu thiếu QuoteSnapshot:

Không nói quyền lợi đã áp vào giá cuối cùng.

Nếu có member dispute:

Không tự sửa hạng.

Mở case.

Chặn CRM bán hàng nếu dispute open.


### 8.10. Evidence Required

EVD-MEMBER-001 — Member tier resolvedEVD-MEMBER-002 — Maintain amount resolvedEVD-MEMBER-003 — Upgrade amount resolvedEVD-MEMBER-004 — Grace period resolvedEVD-MEMBER-005 — Downgrade safe wordingEVD-MEMBER-006 — Quote benefit from runtimeEVD-MEMBER-007 — Member dispute opens caseEVD-MEMBER-008 — Outcome loggedEVD-MEMBER-009 — CRM suppressed during dispute


### 8.11. Done Gate

PASS khi member lifecycle không còn là discount text, mà là runtime-managed lifecycle có hạng, doanh số, duy trì, nâng hạng, ân hạng, hạ hạng, dispute, CRM và outcome evidence.


### 8.12. Pack Detail Mapping

FILE 01 — Member lifecycle decisionFILE 04 — Member resolvers/guardsFILE 07 — Member content blocksFILE 09 — TestsFILE 10 — SituationsFILE 12 — Member care behaviorCRM / Messaging PackBI / Outcome Logger


## 9. DEP-DOM-07-DIAMOND-PARTNER — DIAMOND / AFFILIATE / ENTREPRENEURSHIP / DISTRIBUTOR BOUNDARY DEPENDENCY


### 9.1. Business Purpose

Domain này khóa ranh giới giữa Diamond, affiliate, nhà đồng hành khởi nghiệp, đại lý, nhà phân phối, mua sỉ, công ty mua quà và khách mua lẻ.


### 9.2. Source ID

SRC-MEMBER-001 — MEMBER BENEFIT SOURCESRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCESRC-DIAMOND-001 — DIAMOND REFERRAL / AFFILIATE SOURCESRC-PARTNER-001 — PARTNER / DISTRIBUTOR / WHOLESALE POLICY SOURCESRC-CONTACT-001 — OFFICIAL CONTACT REGISTRY SOURCESRC-PRICE-001 — COMMERCE PRICING SOURCESRC-ORDER-003 — ORDER STATE MACHINE SOURCESRC-MISA-001 — MISA / ACCOUNTING INTEGRATION SOURCE


### 9.3. Owner Core / Owner Pack

Diamond Referral CoreAffiliate / Entrepreneurship Policy CorePartner / Distributor Policy CoreMember Lifecycle CoreCommerce Pricing CoreOrder CoreAccounting / Commission CoreOfficial Contact Registry


### 9.4. Consumer Pack

AI AdvisorCRMGatewayCommerce PricingOrder CoreAccounting / CommissionAdmin UIMessagingEvidence Pack


### 9.5. Dependency Type

DIAMOND_REFERRALAFFILIATEENTREPRENEURSHIPCOMMISSIONPARTNER_ROUTINGDISTRIBUTOR_BOUNDARYWHOLESALE_BOUNDARYOFFICIAL_CONTACT


### 9.6. Required Resolver / Guard

DiamondReferralResolverDiamondLinkAttributionResolverDiamondCommissionResolverAffiliateEligibilityResolverEntrepreneurshipEligibilityResolverDistributorRoutingResolverWholesaleRoutingResolverPartnerPolicyResolverOfficialContactResolverCommissionEligibilityResolverOrderVerifiedResolverSelfPurchaseGuard


### 9.7. Allowed Use

Xác định khách là Diamond.

Xác định link Diamond hợp lệ.

Xác định buyer từ link Diamond.

Xác định quyền lợi buyer từ link Diamond.

Xác định hoa hồng Diamond.

Trả lời chính sách khởi nghiệp nếu đủ điều kiện.

Route đại lý/nhà phân phối/mua sỉ đúng nguồn.

Route công ty mua quà đúng flow.

Gửi số liên hệ chính thức nếu Contact Registry cho phép.

Ghi commission nếu order đủ điều kiện.


### 9.8. Forbidden Use

Tự nói khách đủ điều kiện khởi nghiệp khi chưa có runtime.

Tự ghi commission.

Public commission/payout sai surface.

Chặn mua thường khi Diamond bind invalid.

Tự gửi số điện thoại ngoài registry.

Tự xử lý commission dispute.


### 9.9. bị chặn If Missing

Nếu thiếu DiamondReferralResolver:

Không nói quyền lợi Diamond.

Không nói buyer từ link Diamond.

Không ghi commission.

Nếu thiếu CommissionEligibilityResolver:

Không nói hoa hồng phát sinh.

Không gửi commission message.

Không tạo commission ledger.

Nếu thiếu PartnerPolicyResolver:

Không báo chính sách đại lý/sỉ.

Chỉ route contact nếu OfficialContactResolver pass.


### 9.10. Evidence Required

EVD-DIAMOND-001 — Diamond resolvedEVD-DIAMOND-002 — Diamond link bind passEVD-DIAMOND-003 — Invalid bind still allows normal purchaseEVD-DIAMOND-004 — Commission only after eligible orderEVD-DIAMOND-005 — Self-purchase bị chặn if policy requiresEVD-DIAMOND-006 — Affiliate vs distributor separatedEVD-DIAMOND-007 — Wholesale routed correctlyEVD-DIAMOND-008 — Official contact returned correctly


### 9.11. Done Gate

PASS khi AI/CRM/Commerce/Accounting phân biệt đúng Diamond, affiliate, nhà đồng hành khởi nghiệp, đại lý, nhà phân phối, mua sỉ và công ty mua quà; không hardcode hoa hồng/quyền lợi/số điện thoại.


### 9.12. Pack Detail Mapping

FILE 01 — Routing decisionFILE 04 — Diamond/partner resolversFILE 07 — Diamond/partner wordingFILE 09 — TestsFILE 10 — SituationsFILE 12 — Affiliate behaviorAccounting / Commission PackOfficial Contact Registry


## 10. DEP-DOM-08-PRICING-PROGRAM — PRICING / PROGRAM / 24-7 / GOLDEN HOUR / QUOTESNAPSHOT DEPENDENCY


### 10.1. Business Purpose

Domain này khóa giá, chương trình 24/7, Giờ Vàng, quyền mua sớm, policy priority, QuoteSnapshot và thời hạn giữ giá.


### 10.2. Source ID

SRC-PRICE-001 — COMMERCE PRICING SOURCESRC-PRICE-002 — PROGRAM POLICY SOURCESRC-MEMBER-001 — MEMBER BENEFIT SOURCESRC-DIAMOND-001 — DIAMOND REFERRAL / AFFILIATE SOURCESRC-AVAIL-001 — SELLABLE AVAILABILITY SOURCESRC-ORDER-001 — QUOTE SNAPSHOT SOURCESRC-IVR-001 — IVR ORDER CONFIRMATION SOURCESRC-OPS-001 — OPERATIONAL CORE SOURCE


### 10.3. Owner Core / Owner Pack

Commerce Pricing CoreProgram Policy Core24/7 Program EngineGolden Hour EngineMember Benefit CoreDiamond Referral CoreCommerce Availability CoreOrder CoreIVR Confirmation Core


### 10.4. Consumer Pack

AI AdvisorGatewayCRMLanding PageADSOrder CoreIVRAdmin UIEvidence Pack


### 10.5. Dependency Type

PRICINGPROGRAM_POLICY24_7_PROGRAMGOLDEN_HOUREARLY_ACCESSQUOTE_SNAPSHOTQUOTE_LOCKPOLICY_PRIORITY


### 10.6. Required Resolver / Guard

PricingResolverProgramPolicyResolverProgram24_7ResolverGoldenHourResolverGoldenHourSessionResolverEarlyAccessResolverQuoteSnapshotResolverQuoteLockResolverProgramPriceResolverSuccessfulSalesCountResolverAvailabilityResolverQuotaResolverPolicyPriorityResolverPriceLockGuard


### 10.7. Allowed Use

Báo giá khi có QuoteSnapshot.

Hiển thị giá niêm yết nếu runtime trả.

Hiển thị giá chương trình nếu runtime trả.

Hiển thị quyền lợi member nếu QuoteSnapshot áp.

Hiển thị buyer link Diamond benefit nếu QuoteSnapshot áp.

Nói Giờ Vàng active nếu GoldenHourResolver pass.

Nói quyền mua sớm nếu EarlyAccessResolver pass.

Nói quote hold time nếu QuoteLockResolver pass.

Tạo order từ quote còn hiệu lực.


### 10.8. Forbidden Use

Tự tính giá.

Tự tính discount.

Tự cộng member benefit.

Tự cộng Diamond benefit.

Tự nói Giờ Vàng active.

Tự nói quyền mua sớm.

Tự kéo dài quote.

Tự báo final total thiếu QuoteSnapshot.

Public final price ở live/comment.

Tạo order từ quote hết hạn.

Tự chọn policy priority.

Tính doanh số 24/7 từ quote/giỏ/đơn nháp.


### 10.9. bị chặn If Missing

Nếu thiếu PricingResolver:

Không render final price.

Không tạo OrderDraft.

Nếu thiếu GoldenHourResolver:

Không nói Giờ Vàng active.

Không nói giá Giờ Vàng.

Không nói quyền mua sớm.

Nếu thiếu QuoteLockResolver:

Không nói giữ giá 5 phút/15 phút.

Nếu quote expired:

Không tạo order.

Revalidate quote.


### 10.10. Evidence Required

EVD-PRICE-001 — Pricing resolver pass
EVD-PRICE-002 — QuoteSnapshot sample
EVD-PRICE-003 — 24/7 initial 9%
EVD-PRICE-004 — 24/7 <50 => 9%
EVD-PRICE-005 — 24/7 >=50 => 5%
EVD-PRICE-006 — Golden Hour active resolver
EVD-PRICE-007 — Early access by tier
EVD-PRICE-008 — Quote hold 5 min Golden Hour
EVD-PRICE-009 — Quote hold 15 min 24/7
EVD-PRICE-010 — Expired quote revalidation
EVD-PRICE-011 — Policy priority applied


### 10.11. Done Gate

PASS khi mọi giá/chương trình/quyền lợi/quote/hold time/policy priority đều từ runtime source, không hardcode, không public giá sai surface, không order từ quote hết hạn.


### 10.12. Pack Detail Mapping

Commerce Pricing CoreProgram Policy CoreOrder CoreAI Advisor FILE 04 / 07 / 09 / 10GatewayCRMADSEvidence Pack


## 11. DEP-DOM-09-ACTIVATION — PROGRAM / SELLABLE / PRODUCT ACTIVATION / PRODUCTION SIGNAL DEPENDENCY


### 11.1. Business Purpose

Domain này khóa quy tắc mở bán, ngưng bán, kích hoạt sản phẩm chương trình, kiểm soát SKU active theo kênh và tạo tín hiệu kế hoạch sản xuất.


### 11.2. Source ID

SRC-OPS-001 — OPERATIONAL CORE SOURCESRC-AVAIL-001 — SELLABLE AVAILABILITY SOURCESRC-PRICE-001 — COMMERCE PRICING SOURCESRC-PRICE-002 — PROGRAM POLICY SOURCESRC-PROD-001 — PRODUCT KNOWLEDGE MASTERSRC-CLAIM-001 — CLAIM WHITELIST SOURCESRC-ORDER-003 — ORDER STATE MACHINE SOURCESRC-ADS-001 — ADS TRACKING SIGNAL SOURCE

Cần sử dụng source ghi thêm:

SRC-PROD-ACT-001 — PRODUCT ACTIVATION SOURCESRC-PROD-PLAN-001 — PRODUCTION PLANNING SIGNAL SOURCESRC-LIVE-BOARD-001 — DAILY LIVE PRODUCT BOARD SOURCE


### 11.3. Owner Core / Owner Pack

Operational CoreCommerce Availability CoreCommerce Pricing CoreProgram Policy CoreProduct Knowledge PackClaim Policy PackDaily Live Product Board OwnerProduction Planning CoreOrder Core


### 11.4. Consumer Pack

AI AdvisorGatewayCRMLanding PageADSMC AIOrder CoreAdmin UIProduction PlanningEvidence Pack


### 11.5. Dependency Type

SELLABLE_GATEAUTO_OPEN_SALEAUTO_STOP_SALEPROGRAM_ACTIVATIONCHANNEL_ACTIVATIONLIVE_BOARD_ACTIVATIONPRODUCTION_SIGNAL


### 11.6. Required Resolver / Guard

AvailabilityResolverSellableStatusResolverOperationalReleaseResolverWarehouseReceiptResolverListedPriceStatusResolverQualityHoldResolverRecallHoldResolverSaleLockResolverChannelBlockResolverProgramActivationResolverGoldenHourActivationResolverDailyLiveProductBoardResolverMCBoardValidationGuardProductContentReadinessResolverClaimPolicyResolverSalesVelocityResolverStockMovementResolverProductionPlanningSignalResolverOwnerReviewResolver


### 11.7. Allowed Use

Biết SKU có được mở bán không.

Biết SKU có phải ngưng bán không.

Biết SKU có active trong 24/7 không.

Biết SKU có active trong Giờ Vàng không.

Biết SKU có thuộc board live không.

Bật/tắt CTA mua hàng.

Chặn quote/order SKU not sellable.

Chặn ADS/CRM/Landing/MC AI nói SKU ngoài active list.

Ghi tín hiệu bán/tồn kho cho Production Planning.

Gợi ý sản phẩm thay thế nếu replacement pass guard.


### 11.8. Forbidden Use

Bán SKU chưa nhập kho hợp lệ.

Bán SKU chưa có giá active.

Bán SKU stock_available <= 0.

Bán SKU quality hold.

Bán SKU recall hold.

Bán SKU sale lock.

Bán SKU channel block.

Public số lượng tồn kho.

Nói “chỉ còn vài hộp” tạo khan hiếm giả.

Gợi ý SKU not sellable.

Landing Page hardcode active SKU.

Production signal tự thành Production Order.


### 11.9. bị chặn If Missing

Nếu thiếu AvailabilityResolver:

Không quote.

Nếu thiếu ListedPriceStatusResolver:

Không mở bán.

Nếu thiếu ProgramActivationResolver:

Không nói SKU thuộc chương trình.

Không gửi CRM chương trình.

Không chạy ADS chương trình.

Nếu thiếu DailyLiveProductBoardResolver:

MC AI không nói SKU live.

Gateway không xử lý offer theo board.

ADS/Live không dùng board-specific SKU.

Nếu thiếu ProductionPlanningSignalResolver:

Không tạo tín hiệu sản xuất.

Không tự tạo production order.


### 11.10. Evidence Required

EVD-ACT-001 — SKU sellable gate passEVD-ACT-002 — SKU auto stop when out of stockEVD-ACT-003 — Quality hold blocks saleEVD-ACT-004 — Recall hold blocks saleEVD-ACT-005 — Listed price active requiredEVD-ACT-006 — 24/7 active SKU listEVD-ACT-007 — Golden Hour active SKU listEVD-ACT-008 — Daily Live Board SKU validationEVD-ACT-009 — MC AI no non-board SKUEVD-ACT-010 — ADS only active SKUEVD-ACT-011 — CRM only active SKUEVD-ACT-012 — Production planning signal logged


### 11.11. Done Gate

PASS khi SKU chỉ được bán nếu sellable, giá active, stock hợp lệ, không hold/lock; chương trình chỉ dùng active list; AI/Gateway/CRM/Landing/ADS/MC AI không nói SKU ngoài active boundary; dữ liệu bán/tồn kho tạo Production Planning Signal nhưng không tự tạo production order.


### 11.12. Pack Detail Mapping

Operational CoreCommerce Availability CoreProgram Policy CoreProduct KnowledgeDaily Live Product BoardMC AI PackADS PackCRM PackOrder CoreProduction Planning PackFILE 04 / FILE 07 / FILE 09 / FILE 10 / FILE 12


## 12. DEP-DOM-10-PAYMENT — PAYMENT / BANK TRANSFER / ACCOUNTING REVIEW DEPENDENCY


### 12.1. Business Purpose

Domain này khóa thanh toán, chuyển khoản ngân hàng, VietQR, payment reference, queue kế toán, đối soát thủ công/tự động và PAID state.


### 12.2. Source ID

SRC-PAY-001 — PAYMENT ELIGIBILITY SOURCESRC-PAY-002 — PAYMENT STATE SOURCESRC-BANK-001 — COMPANY BANK ACCOUNT REGISTRY SOURCESRC-ORDER-003 — ORDER STATE MACHINE SOURCESRC-MISA-001 — MISA / ACCOUNTING INTEGRATION SOURCESRC-EVD-001 — EVIDENCE REGISTRY SOURCE


### 12.3. Owner Core / Owner Pack

Payment CoreCompany Bank Account RegistryAccounting Review CoreOrder CoreMISA / Accounting IntegrationAdmin UI


### 12.4. Consumer Pack

AI AdvisorGatewayCRMLanding PageOrder CoreAdmin UIAccounting QueueEvidence Pack


### 12.5. Dependency Type

PAYMENT_ELIGIBILITYBANK_TRANSFERVIETQRPAYMENT_REFERENCEACCOUNTING_REVIEWPAYMENT_STATEPAID_CONTROL


### 12.6. Required Resolver / Guard

PaymentEligibilityResolverPaymentInstructionResolverBankAccountRegistryResolverBankTransferTaggingResolverPaymentReferenceResolverAccountingBankTransferQueueResolverManualReconciliationResolverBankWebhookResolverPaymentProofResolverPaymentStateResolverOrderPaymentLinkageResolverPaymentStatusGuardAccountingPermissionGuardAuditResolver


### 12.7. Allowed Use

Hiển thị phương thức thanh toán hợp lệ.

Gửi payment instruction trong private nếu Payment Core cho phép.

Gắn payment_method khi khách chọn chuyển khoản.

Sinh payment_reference.

Đưa đơn vào Accounting Bank Transfer Queue.

Nhận proof giao dịch nếu policy cho phép.

Đối soát thủ công qua kế toán.

Đối soát tự động qua webhook nếu có.

Cập nhật PAID khi Payment Core / Accounting Review xác nhận.


### 12.8. Forbidden Use

Hardcode bank account.

Hardcode QR.

Gửi bank instruction thiếu payment_reference.

Để khách tự ghi nội dung chuyển khoản tùy ý nếu đã có reference.

AI xác nhận PAID.

Gateway xác nhận PAID.

CRM xác nhận PAID.

Admin UI set PAID nếu thiếu permission/audit.

Xem ảnh giao dịch là đã paid.

Xem khách nói “đã chuyển” là paid.

Trộn đơn COD và bank transfer trong queue không phân loại.


### 12.9. bị chặn If Missing

Nếu thiếu PaymentEligibilityResolver:

Không hứa phương thức thanh toán.

Nếu thiếu BankRegistry:

Không gửi bank account.

Không render VietQR.

Nếu thiếu payment_reference:

Không gửi hướng dẫn chuyển khoản.

Nếu thiếu AccountingReview:

Không PAID_BY_BANK_TRANSFER.

Không release fulfillment nếu payment required.

Nếu khách nói đã chuyển khoản nhưng Payment Core chưa xác nhận:

Không nói đã thanh toán.

Ghi nhận chờ đối soát.

Yêu cầu proof nếu policy cho phép.


### 12.10. Evidence Required

EVD-PAY-001 — Payment eligibility passEVD-PAY-002 — Bank transfer taggedEVD-PAY-003 — Unique payment_referenceEVD-PAY-004 — Bank instruction from registryEVD-PAY-005 — Accounting queue recordEVD-PAY-006 — Payment proof attachedEVD-PAY-007 — Manual reconciliation auditEVD-PAY-008 — Bank webhook reconciliation if availableEVD-PAY-009 — AI no paid before Core confirmsEVD-PAY-010 — PAID_BY_BANK_TRANSFER after confirmation


### 12.11. Done Gate

PASS khi thanh toán chỉ đi qua Payment Core / Bank Registry / Accounting Review; chuyển khoản có payment_reference và queue kế toán; không PAID nếu chưa có xác nhận hợp lệ; AI/Gateway/CRM không xác nhận thanh toán thay core.


### 12.12. Pack Detail Mapping

COM-06 — Payment / Bank Transfer / Accounting QueuePayment CoreAccounting ReviewOrder CoreAdmin UIFILE 04 / FILE 07 / FILE 09 / FILE 10Evidence PackSecurity Pack


## 13. DEP-DOM-11-SHIPPING — SHIPPING / TRACKING / ETA / COD DEPENDENCY


### 13.1. Business Purpose

Domain này khóa vận chuyển, COD, phí ship, tracking, ETA realtime và fallback thời gian giao hàng.


### 13.2. Source ID

SRC-SHIP-001 — SHIPPING ELIGIBILITY SOURCESRC-SHIP-002 — SHIPPING STATE SOURCESRC-ORDER-003 — ORDER STATE MACHINE SOURCESRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCE


### 13.3. Owner Core / Owner Pack

Shipping CoreOrder CoreDelivery Tracking CoreCarrier IntegrationCRM Lifecycle Core


### 13.4. Consumer Pack

AI AdvisorGatewayCRMLanding PageAdmin UIOrder CoreEvidence Pack


### 13.5. Dependency Type

SHIPPING_ELIGIBILITYCOD_ELIGIBILITYSHIPPING_FEETRACKINGETAREGION_FALLBACKSHIPPING_STATE


### 13.6. Required Resolver / Guard

ShippingResolverShippingEligibilityResolverCODEligibilityResolverShippingFeeResolverShippingStateResolverDeliveryTrackingResolverCarrierTrackingLinkResolverDeliveryETAResolverDeliveryRegionFallbackResolverShippingInfoAvailabilityGuardTrackingDataFreshnessGuard


### 13.7. Allowed Use

Nói có ship hay không.

Nói COD nếu eligible.

Nói phí ship nếu resolver trả.

Nói ETA nếu resolver trả.

Gửi tracking link nếu có.

Dùng fallback vùng nếu không có tracking realtime và policy cho phép.

Cập nhật CRM shipping state nếu Shipping State cho phép.


### 13.8. Forbidden Use

Tự hứa ETA.

Tự hứa miễn ship.

Tự hứa COD.

Tự tính phí ship.

Tự bịa tracking status.

Tự bịa carrier link.

Hỏi lại địa chỉ nếu order có shipping info.

Hứa giao quốc tế nếu không thuộc active scope.

Dùng Landing Page text thay Shipping Core.

Dùng CRM template cũ làm shipping source.


### 13.9. bị chặn If Missing

Nếu thiếu ShippingEligibilityResolver:

Không hứa ship/ETA/COD/phí ship.

Nếu thiếu ShippingStateResolver:

Không nói trạng thái giao hàng cụ thể.

Nếu thiếu Tracking realtime:

Dùng fallback vùng nếu policy cho phép.

Không bịa trạng thái carrier.

Nếu thiếu shipping info:

Hỏi thêm trong private.

Không quote ETA chắc chắn.


### 13.10. Evidence Required

EVD-SHIP-001 — Shipping eligibilityEVD-SHIP-002 — COD eligibilityEVD-SHIP-003 — Shipping fee from coreEVD-SHIP-004 — Tracking link returnedEVD-SHIP-005 — Fallback ETA by regionEVD-SHIP-006 — No fake tracking statusEVD-SHIP-007 — No address re-ask when order has infoEVD-SHIP-008 — MAS-SMK-006 pass


### 13.11. Done Gate

PASS khi shipping/ETA/COD/tracking chỉ đến từ Shipping Core hoặc fallback được phép; không hardcode, không bịa trạng thái, không hỏi lại dữ liệu đã có.


### 13.12. Pack Detail Mapping

COM-07 — Shipping / Tracking / COD / ETAShipping CoreOrder CoreCRM PackFILE 04 / FILE 07 / FILE 09 / FILE 10Evidence Pack


## 14. DEP-DOM-12-ORDER-IVR — ORDER / ORDERDRAFT / CUSTOMERCONFIRMATION / IVR DEPENDENCY


### 14.1. Business Purpose

Domain này khóa Sales-to-Order, QuoteCart, QuoteSnapshot, OrderConfirmationDraft, CustomerConfirmation, OrderCreation, order_code, idempotency và IVR confirmation.


### 14.2. Source ID

SRC-ORDER-001 — QUOTE SNAPSHOT SOURCESRC-ORDER-002 — ORDER CONFIRMATION DRAFT SOURCESRC-ORDER-003 — ORDER STATE MACHINE SOURCESRC-IVR-001 — IVR ORDER CONFIRMATION SOURCESRC-PAY-001 — PAYMENT ELIGIBILITY SOURCESRC-SHIP-001 — SHIPPING ELIGIBILITY SOURCESRC-AVAIL-001 — SELLABLE AVAILABILITY SOURCE


### 14.3. Owner Core / Owner Pack

Order CoreQuoteSnapshot CorePayment CoreShipping CoreCommerce Availability CoreIVR Order Confirmation PackAI Advisor Sales-to-Order Logic


### 14.4. Consumer Pack

AI AdvisorGatewayCRMOrder CorePayment CoreShipping CoreIVRAdmin UIEvidence Pack


### 14.5. Dependency Type

SALES_TO_ORDERQUOTE_CARTQUOTE_SNAPSHOTORDER_DRAFTCUSTOMER_CONFIRMATIONORDER_CREATIONORDER_STATEIVR_CONFIRMATIONIDEMPOTENCY


### 14.6. Required Resolver / Guard

QuoteCartResolverQuoteSnapshotResolverQuoteLockResolverOrderDraftResolverCustomerConfirmationResolverOrderCreationResolverOrderCodeGuardCartChangeRevalidationResolverCustomerPrefillResolverPaymentEligibilityResolverShippingEligibilityResolverInventorySellableGuardOrderStateResolverIVREligibilityResolverIVRConfirmationResolverIVRQuotaReleaseGuardIdempotencyResolverAuditResolver


### 14.7. Allowed Use

Mở QuoteCart khi đủ điều kiện.

Tạo QuoteSnapshot.

Render quote.

Render OrderConfirmationDraft.

Prefill thông tin khách cũ nếu policy cho phép.

Chờ CustomerConfirmation.

Nói Order Success khi có order_code.

Gửi IVR confirmation khi resolver yêu cầu.

Ghi audit/idempotency.


### 14.8. Forbidden Use

Tạo order khi khách chỉ nói “mua” nhưng chưa xác nhận draft.

Báo giá thiếu QuoteSnapshot.

Render Order Success thiếu order_code.

Tạo order public comment.

Tự tạo order_code.

Tự đổi order state.

SIM Gateway cập nhật order trực tiếp.

Duplicate retry tạo nhiều order.

OrderDraft bị render như Order Success.


### 14.9. bị chặn If Missing

Nếu thiếu OrderDraft:

Không xin xác nhận đơn.

Không nói đơn sẵn sàng.

Nếu thiếu CustomerConfirmation:

Không tạo order chính thức.

Nếu thiếu order_code:

Không nói đơn đã ghi nhận.

Nếu IVR required nhưng chưa pass:

Không release fulfillment nếu policy yêu cầu.

Giữ order ở trạng thái chờ xác nhận.


### 14.10. Evidence Required

EVD-ORDER-001 — QuoteCart sampleEVD-ORDER-002 — QuoteSnapshot sampleEVD-ORDER-003 — OrderDraft formEVD-ORDER-004 — Prefill old customerEVD-ORDER-005 — CustomerConfirmationEVD-ORDER-006 — Order created from confirmed quoteEVD-ORDER-007 — order_codeEVD-ORDER-008 — Expired quote revalidationEVD-ORDER-009 — Duplicate idempotencyEVD-ORDER-010 — IVR result handled by Order CoreEVD-ORDER-011 — IVR technical error not no-answer


### 14.11. Done Gate

PASS khi order flow đi đúng tư vấn -> QuoteSnapshot -> OrderDraft -> CustomerConfirmation -> OrderCreation -> order_code; IVR chỉ gửi result, Order Core quyết định state; duplicate không tạo nhiều đơn.


### 14.12. Pack Detail Mapping

Order CoreQuoteSnapshot CoreIVR PackPayment CoreShipping CoreAI Advisor FILE 04 / FILE 07 / FILE 09 / FILE 10 / FILE 12GatewayCRMEvidence Pack


## 15. DEP-DOM-13-GATEWAY-HANDOFF-MODERATION — GATEWAY / PUBLIC COMMENT / MESSENGER HANDOFF / MODERATION DEPENDENCY


### 15.1. Business Purpose

Domain này khóa xử lý channel, public/private boundary, live/comment, Messenger handoff, handoff success/fail, duplicate event, attribution preservation và live moderation.


### 15.2. Source ID

SRC-CHANNEL-001 — CHANNEL POLICY SOURCESRC-HANDOFF-001 — MESSENGER HANDOFF SOURCESRC-META-001 — META POLICY / GATEWAY EVENT SOURCESRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCESRC-ORDER-001 — QUOTE SNAPSHOT SOURCESRC-SEC-001 — SECURITY / PERMISSION / AUDIT SOURCESRC-EVD-001 — EVIDENCE REGISTRY SOURCESRC-MOD-001 — LIVE MODERATION POLICY SOURCESRC-BRANDRISK-001 — BRAND RISK CLASSIFICATION SOURCE


### 15.3. Owner Core / Owner Pack

Facebook Channel Gateway PackAI Advisor Channel Policy PackMessenger Handoff RuntimeMeta Event GatewayAI Advisor OrchestratorGateway Moderation CoreEvidence Pack


### 15.4. Consumer Pack

GatewayAI AdvisorCRMOrder CorePricing CoreMessengerAdmin Moderation UIEvidence Pack


### 15.5. Dependency Type

CHANNEL_CONTEXTPUBLIC_PRIVATE_BOUNDARYMESSENGER_HANDOFFHANDOFF_DELIVERYATTRIBUTION_PRESERVATIONDUPLICATE_EVENTLIVE_MODERATIONBRAND_RISK


### 15.6. Required Resolver / Guard

ChannelContextResolverPublicPrivateSurfaceResolverMessengerHandoffResolverHandoffDeliveryStatusResolverLiveCommentIntentResolverPrivacyPIIGuardMetaDuplicateEventResolverAttributionContextResolverHandoffContextRestorationResolverLiveAbuseClassifierProfanityNormalizerBrandAttackClassifierTrollSpamBurstGuardComplaintEvidenceGuardModerationActionResolverNoMessengerHandoffForAbuseGuardFinalGuard


### 15.7. Allowed Use

Gateway/AI được dùng domain này để:

Nhận public comment/live comment.

Phân loại intent.

Chặn public price/order/PII/payment/health detail.

Chủ động handoff Messenger nếu policy cho phép.

Nói đã gửi/chuyển Messenger khi handoff success.

Dùng fallback an toàn khi handoff fail.

Giữ context page/live/comment/ads/diamond nếu có.

Dedup webhook/comment/message.

Hide/no reply/no Messenger khi abuse rõ.

Route complaint thật vào case.


### 15.8. Forbidden Use

Gateway/AI không được:

Public final price.

Public order confirmation.

Public payment instruction.

Public invoice detail.

Repeat PII.

Tư vấn health-sensitive dài ở public.

Kêu khách tự inbox nếu system handoff có thể chạy.

Nói đã gửi Messenger khi handoff fail.

Gateway tự quote/order/CRM.

Handoff duplicate do webhook retry.

Kéo troll/abuse vào Messenger.

Đôi co với brand attack.

Chặn nhầm complaint thật có evidence.


### 15.9. bị chặn If Missing

Nếu thiếu Channel Policy:

Không tự chọn public/private.

Chỉ safe acknowledgement hoặc no-action.

Nếu thiếu HandoffDeliveryStatus:

Không nói đã gửi Messenger.

Nếu thiếu Attribution preservation:

Không tính attribution live/ads/referral.

Evidence fail.

Nếu abuse classification rõ:

Không handoff.

Hide/no reply theo policy.


### 15.10. Evidence Required

EVD-GW-001 — Public price blockedEVD-GW-002 — Public order blockedEVD-GW-003 — PII not repeatedEVD-GW-004 — Payment public blockedEVD-GW-005 — Health public blockedEVD-GW-006 — Handoff success eventEVD-GW-007 — Handoff failed eventEVD-GW-008 — Public reply after successEVD-GW-009 — Public reply after failEVD-GW-010 — Context preservedEVD-GW-011 — Duplicate webhook no duplicate replyEVD-GW-012 — Abuse no MessengerEVD-GW-013 — Real complaint routed


### 15.11. Done Gate

PASS khi public comment/live chỉ là cổng điều hướng an toàn; tư vấn sâu, giá, quote, order, payment, shipping, health, Diamond đều chuyển private đúng policy; abuse không vào Messenger; context và evidence được giữ.


### 15.12. Pack Detail Mapping

Gateway PackAI Advisor FILE 04FILE 07 — CB-LIVE / CB-HANDOFF / moderation wordingFILE 09 — TestsFILE 10 — Live/comment situationsCompletion ReportEvidence PackSecurity Pack


## 16. DEP-DOM-14-CONTACT — OFFICIAL CONTACT / PHONE NUMBER REGISTRY DEPENDENCY


### 16.1. Business Purpose

Domain này khóa số điện thoại chính thức, mục đích gửi, kênh gửi và ranh giới không cung cấp số cá nhân hoặc số ngoài registry.


### 16.2. Source ID

SRC-CONTACT-001 — OFFICIAL CONTACT REGISTRY SOURCESRC-CONTACT-002 — CONTACT PURPOSE POLICY SOURCESRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCESRC-CRM-001 — CRM LIFECYCLE SOURCESRC-SEC-001 — SECURITY / PERMISSION / AUDIT SOURCE


### 16.3. Owner Core / Owner Pack

Official Contact RegistryContact Purpose Policy PackCustomer Service RoutingPartner / Distributor RoutingExecutive Privacy Policy


### 16.4. Consumer Pack

AI AdvisorGatewayCRMLanding PageMessengerPublic CommentAdmin UI


### 16.5. Dependency Type

OFFICIAL_CONTACTCONTACT_PURPOSEPHONE_NUMBER_USAGEEXECUTIVE_PRIVACYPARTNER_ROUTINGPUBLIC_PRIVATE_CONTACT


### 16.6. Required Resolver / Guard

OfficialContactResolverContactPurposeResolverVisitRequestRoutingGuardExecutivePrivacyGuardDistributorContactRoutingGuardPartnerContactResolverAffiliateVsDistributorRoutingGuardContactSurfaceGuardFinalGuard


### 16.7. Allowed Use

Gửi số điện thoại chính thức theo đúng mục đích.

Route tham quan / sắp lịch.

Route gặp công ty / lãnh đạo qua kênh chính thức.

Chặn xin số cá nhân lãnh đạo.

Route đại lý / nhà phân phối / mua sỉ.

Phân biệt affiliate/Diamond với distributor.

Gửi contact trên surface được phép.


### 16.8. Forbidden Use

Tự nhớ số điện thoại.

Tự bịa số điện thoại.

Gửi số cá nhân lãnh đạo.

Gửi số không đúng mục đích.

Route affiliate sang đại lý khi khách hỏi khởi nghiệp Diamond.

Route khách mua quà nhỏ sang wholesale.

Hardcode số trong Landing Page/CRM/template nếu không qua registry.

Public contact nếu ContactSurfaceGuard không cho.


### 16.9. bị chặn If Missing

Không gửi số điện thoại.

Dùng fallback an toàn nếu được duyệt.

Nếu thiếu ContactPurposeResolver:

Hỏi thêm ngắn.

Không route tùy tiện.

Nếu khách xin số cá nhân lãnh đạo:

Không cung cấp.

Route kênh chính thức.


### 16.10. Evidence Required

EVD-CONTACT-001 — Visit request returns approved contactEVD-CONTACT-002 — Company/leader contact returns approved contactEVD-CONTACT-003 — Personal leader phone blockedEVD-CONTACT-004 — Distributor/wholesale contact routeEVD-CONTACT-005 — Affiliate not misroutedEVD-CONTACT-006 — No unapproved phone numberEVD-CONTACT-007 — Contact surface allowed


### 16.11. Done Gate

PASS khi mọi số điện thoại customer-facing đều đến từ Official Contact Registry, đúng mục đích, đúng kênh, không hardcode, không lộ số cá nhân, không route sai affiliate/distributor.


### 16.12. Pack Detail Mapping

Official Contact RegistryFILE 04 — Contact resolvers/guardsFILE 07 — Contact wordingFILE 09 — TestsFILE 10 — Contact situationsGateway / CRM / Landing Page


## 17. DEP-DOM-15-HEALTH-CASE-CONFLICT — HEALTH BOUNDARY / COMPLAINT / PRIORITY CONFLICT DEPENDENCY


### 17.1. Business Purpose

Domain này khóa ranh giới health-sensitive, disease mention, recovery, allergy/dietary, complaint, refund, privacy, counterfeit, member dispute và priority conflict.


### 17.2. Source ID

SRC-CLAIM-001 — CLAIM WHITELIST SOURCESRC-PROD-001 — PRODUCT KNOWLEDGE MASTERSRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCESRC-CASE-001 — CUSTOMER CASE / COMPLAINT SOURCESRC-SUPPRESS-001 — CRM SUPPRESSION POLICY SOURCESRC-PAY-002 — PAYMENT STATE SOURCESRC-RECALL-001 — RECALL / RECOVERY SOURCESRC-SEC-001 — SECURITY / PERMISSION / AUDIT SOURCE


### 17.3. Owner Core / Owner Pack

Claim Policy PackHealth Safety PackProduct Knowledge PackCase Management CoreCRM Suppression CoreCustomer Service CorePayment Dispute CorePrivacy Request CoreCounterfeit / Quality Case CoreMember Dispute Core


### 17.4. Consumer Pack

AI AdvisorGatewayCRMLanding PageADSOrder CoreMessagingAdmin UIEvidence Pack


### 17.5. Dependency Type

HEALTH_INTENTDISEASE_BOUNDARYRECOVERY_CONTEXTALLERGY_DIETARYCLAIM_BOUNDARYCOMPLAINTPRIVACYPAYMENT_DISPUTECOUNTERFEITPRIORITY_CONFLICTSUPPRESSION


### 17.6. Required Resolver / Guard

HealthIntentResolverDiseaseMentionResolverHealthSafetyResolverHealthSafetyGuardConditionSensitiveClaimGuardRecoveryContextResolverMedicationTreatmentContextResolverAllergyDietaryGuardPregnancySensitiveGuardChildSensitiveGuardPriorityConflictResolverOpenCaseStatusResolverOpenCaseSuppressionGuardCRMOpenCaseSuppressionGuardRefundDisputeGuardPrivacyRequestGuardPaymentDisputeGuardDeliveryIssueGuardHealthReviewSuppressionGuardCounterfeitCaseGuardMemberDisputeEscalationResolverFinalGuard


### 17.7. Allowed Use

Nhận diện health-sensitive.

Không tự hỏi bệnh khi khách không nêu.

Kích hoạt guard khi khách nêu bệnh.

Tư vấn theo thực dưỡng, không thuốc hóa.

Chặn quote/order nếu health unresolved.

Chặn CRM khi complaint/open case.

Chặn upsell khi refund/payment/privacy/counterfeit dispute.

Mở case/human review khi cần.

Route complaint thật.

Không xử lý hàng nghi giả như đổi trả thường khi chưa xác minh.


### 17.8. Forbidden Use

Chủ động hỏi bệnh khi khách không nêu.

Chủ động hỏi kiêng cữ khi không có tín hiệu liên quan.

Chẩn đoán.

Nói chữa bệnh.

Nói thay thuốc.

Cam kết khỏi bệnh.

Quote khi health unresolved.

Upsell khi complaint open.

Gửi CRM member/upgrade khi dispute open.

Tự kết luận hàng giả.

Tự sửa hạng/quyền lợi khi dispute.

Tự bỏ suppression để bán tiếp.


### 17.9. bị chặn If Missing

Nếu thiếu HealthSafetyGuard trong tình huống health-sensitive:

Không order.

Không tư vấn sản phẩm cụ thể nếu cần làm rõ.

Nếu thiếu OpenCaseStatusResolver trong tình huống case-sensitive:

Không member lifecycle promotion.

Nếu có complaint/refund/privacy/payment/counterfeit/member dispute:

Priority conflict chặn sales.

Mở case/human review.


### 17.10. Evidence Required

EVD-HEALTH-001 — No unsolicited disease questionEVD-HEALTH-002 — Disease mention triggers guardEVD-HEALTH-003 — Health unresolved blocks quoteEVD-HEALTH-004 — No treatment claimEVD-HEALTH-005 — Public health detail blockedEVD-CASE-001 — Complaint suppresses CRMEVD-CASE-002 — Refund suppresses upsellEVD-CASE-003 — Privacy request suppresses CRMEVD-CASE-004 — Payment dispute suppresses salesEVD-CASE-005 — Counterfeit opens caseEVD-CASE-006 — Member dispute opens case


### 17.11. Done Gate

PASS khi health-sensitive, complaint, refund, privacy, payment dispute, counterfeit and member dispute always override sales/CRM/member promotion; no diagnosis, no treatment claim, no upsell during open case.


### 17.12. Pack Detail Mapping

FILE 04 — Resolvers/guardsFILE 06 — Claim PolicyFILE 07 — Health/case wordingFILE 09 — TestsFILE 10 — SituationsFILE 12 — Safe sales behaviorCase Management PackCRM Suppression PackEvidence Pack


## 18. DEP-DOM-16-EVIDENCE-SECURITY — EVIDENCE / COMPLETION REPORT / GATEWAY GATE / SECURITY DEPENDENCY


### 18.1. Business Purpose

Domain này khóa evidence, Completion Report, Gateway transition, production readiness, security, permission, audit, owner sign-off và release gate.


### 18.2. Source ID

SRC-EVD-001 — EVIDENCE REGISTRY SOURCESRC-GOV-001 — MASTER GOVERNANCE SOURCESRC-SEC-001 — SECURITY / PERMISSION / AUDIT SOURCESRC-COMP-001 — AI ADVISOR FACEBOOK COMPLETION REPORT SOURCESRC-GATEWAY-001 — FACEBOOK CHANNEL GATEWAY READINESS SOURCESRC-MC-001 — MC AI / DAILY LIVE BOARD READINESS SOURCE


### 18.3. Owner Core / Owner Pack

Evidence RegistryCompletion Report OwnerGateway Readiness OwnerSecurity / Permission / Audit PackRelease GovernanceQAOwner Sign-off AuthorityMC AI / Daily Live Board Owner


### 18.4. Consumer Pack

Release GovernanceGateway PackAI AdvisorMC AIADS / ROASCRMOrder CorePayment CoreShipping CoreAdmin UIEvidence PackOwner Review


### 18.5. Dependency Type


### 18.6. Required Resolver / Guard

CompletionGateResolverEvidenceRegistryResolverGatewayTransitionResolverP0GateStatusResolverDecisionEnvelopeEvidenceResolverResolverTraceEvidenceResolverGuardTraceEvidenceResolverHandoffDeliveryLogResolverQuoteSnapshotEvidenceResolverOrderCodeEvidenceResolverCRMSuppressionEvidenceResolverDuplicateIdempotencyEvidenceResolverSecurityPermissionResolverAuditResolverOwnerSignOffResolver


### 18.7. Allowed Use

Release Governance được dùng domain này để:

Đánh giá Completion Report.

Đánh giá Gateway readiness.

Đánh giá production readiness.

Kiểm evidence P0.

Kiểm DecisionEnvelope.

Kiểm resolver trace.

Kiểm guard trace.

Kiểm handoff delivery log.

Kiểm QuoteSnapshot.

Kiểm order_code.

Kiểm CRM suppression.

Kiểm duplicate/idempotency.

Kiểm security/permission/audit.

Kiểm owner sign-off.


### 18.8. Forbidden Use

Gọi PASS khi thiếu evidence.

Gọi Ready vì tài liệu đã viết.

Dùng screenshot câu trả lời hay thay evidence.

Dùng demo miệng thay trace.

Mở Gateway khi P0 chưa đủ điều kiện/fail.

Gọi Production Ready khi Completion Report chưa đủ điều kiện.

Cho protected action thiếu permission.

Bỏ audit.

Cho Admin UI bypass backend permission.

Cho Gateway tự quote/order/CRM.

Cho MC AI live khi board/script/video chưa pass.


### 18.9. bị chặn If Missing

Nếu bất kỳ P0 gate chưa đủ điều kiện/FAIL:

Nếu thiếu owner sign-off:

Không release.

Không go-live.

Không Gateway transition.

Nếu thiếu permission/audit:

Protected action bị chặn.

Release evidence fail.


### 18.10. Evidence Required

EVD-COMP-001 — DecisionEnvelope sampleEVD-COMP-002 — Resolver traceEVD-COMP-003 — Guard traceEVD-COMP-004 — Handoff delivery logEVD-COMP-005 — QuoteSnapshot sampleEVD-COMP-006 — OrderDraft sampleEVD-COMP-007 — CustomerConfirmation sampleEVD-COMP-008 — order_code sampleEVD-COMP-009 — CRM suppression evidenceEVD-COMP-010 — Duplicate/idempotency testEVD-COMP-011 — MC AI board/script evidenceEVD-COMP-012 — ROAS internal-only evidenceEVD-COMP-013 — Security/permission/audit evidenceEVD-COMP-014 — Owner sign-off


### 18.11. Done Gate

PASS khi Completion Report có đủ evidence thật cho tất cả P0 gate, không còn P0 open issue, protected actions có permission/audit, Gateway transition được owner sign-off; nếu thiếu, Gateway và Production vẫn bị chặn.


### 18.12. Pack Detail Mapping

Completion ReportEvidence PackGateway PackSecurity PackAI Advisor FILE 04 / FILE 05 / FILE 09MC AI / Daily Live Board PackCRM PackOrder CorePayment CoreShipping CoreQA / Release Governance


## 19. CROSS-DOMAIN PRIORITY ORDER

Khi nhiều domain cùng áp dụng, hệ thống xử lý theo thứ tự ưu tiên:


## 01. Security / Permission / Audit02. Completion / Release Gate03. Priority Conflict / Open Case / Complaint / Privacy / Payment Dispute / Health Review04. Customer Identity / Customer Memory05. Health / Allergy / Dietary Safety06. Product Knowledge / Claim / Brand / Scientific Proof07. Sellable / Availability / Program Activation08. Pricing / Program / QuoteSnapshot09. Payment / Shipping / Order10. CRM / Messaging / Member Lifecycle11. Recommendation / Segment / Growth / Combo12. ADS / Gateway / Landing / Public Display13. BI / Outcome / Learning Signal

Safety và case conflict thắng sales.

Availability thắng Product Recommendation.

Claim Policy thắng Product Knowledge wording nếu có rủi ro.

QuoteSnapshot thắng AI/CRM/Gateway text.

Payment Core thắng ảnh giao dịch/khách nói đã chuyển.

Shipping Core thắng template/landing copy.

Order Core thắng Gateway/AI/IVR raw result.

Evidence Registry thắng lời xác nhận miệng.

Gateway không được vượt AI Advisor / Runtime Core.

CRM không được vượt suppression.


## 20. PART 2 DONE GATE


## PHẦN 2/4 đạt khi:

Đã khóa đủ 16 business domain dependency.

Mỗi domain có Source ID.

Mỗi domain có Owner Core / Owner Pack.

Mỗi domain có Consumer Pack.

Mỗi domain có Required Resolver / Guard.

Mỗi domain có Allowed Use.

Mỗi domain có Forbidden Use.

Mỗi domain có bị chặn-if-missing.

Mỗi domain có Evidence Required.

Mỗi domain có Done Gate.

Mỗi domain có Pack Detail Mapping.

Quy tắc mở bán là domain riêng.

CRM 12 tháng là domain chính thức.

Member Lifecycle là domain chính thức.

Message Trigger / Immutable Text là domain chính thức.

Gateway / Handoff / Moderation là domain chính thức.

Completion Report / Evidence / Security là release domain chính thức.

Không domain nào cho phép consumer tự suy luận rule kinh doanh.

Không domain nào cho phép hardcode runtime data.

Gateway vẫn bị chặn.


## 21. TRẠNG THÁI SAU PHẦN 2/4


## 22. PHẦN TIẾP THEO


## PHẦN 3/4 — RUNTIME CONTROL / CONFLICT RESOLUTION / SUPPRESSION / FALLBACK / NO-HARDCODE ENFORCEMENT


## PHẦN 3/4 sẽ khóa:


## 1. Runtime result status model2. Cross-domain conflict resolution3. Priority conflict / open case suppression4. Customer identity conflict5. Customer memory conflict6. Member / Diamond / commission conflict7. Recommendation vs Availability conflict8. Program activation vs stock conflict9. Payment proof vs Payment Core conflict10. Shipping tracking vs fallback ETA conflict11. Health intent vs sales intent conflict12. Brand science vs Claim Policy conflict13. Public comment vs Messenger private conflict14. Message trigger vs CRM content conflict15. Completion Report vs missing evidence conflict16. Expanded no-hardcode enforcement17. Safe fallback rules18. Future extension boundary


## 0. MỤC ĐÍCH CỦA PHẦN 3/4


## PHẦN 3/4 khóa cách hệ thống xử lý khi dữ liệu runtime chưa đủ, source mâu thuẫn, resolver lỗi, consumer muốn hành động nhưng thiếu quyền, hoặc rule kinh doanh bị xung đột giữa nhiều pack.


## PHẦN 3/4 trả lời các câu hỏi:

Runtime resolver phải trả trạng thái như thế nào.

Consumer được hành động khi runtime trả kết quả gì.

Khi source thiếu thì AI / CRM / Gateway / Landing Page / Admin UI phải dừng ở đâu.

Khi rule xung đột thì source nào thắng.

Khi khách có open case thì CRM và member sales bị chặn thế nào.

Khi customer identity chưa rõ thì có được cá nhân hóa không.

Khi customer memory thiếu thì có được nói lịch sử mua không.

Khi member lifecycle thiếu runtime thì có được nói nâng/hạ hạng không.

Khi Diamond link chưa bind thì có được nói quyền lợi Diamond không.

Khi sản phẩm chưa sellable thì có được quote/order không.

Khi payment chưa xác nhận thì có được nói PAID không.

Khi shipping chưa có tracking thì có được hứa ETA không.

Khi public comment hỏi giá thì có được báo giá không.

Khi thiếu evidence thì có được Gateway PASS không.

Khi fallback được dùng và khi nào fallback bị cấm.

Dữ liệu nào tuyệt đối không được hardcode.

Domain nào phải giữ bị chặn nếu missing source, missing resolver hoặc missing evidence.


## PHẦN 3/4 không viết implementation chi tiết.


## PHẦN 3/4 không viết API, DTO, DB, worker, UI hoặc code.


## 1. RUNTIME RESULT STATUS MODEL

Mọi Runtime Resolver trong hệ thống Ginsengfood phải trả trạng thái rõ ràng.

Consumer không được tự diễn giải kết quả mơ hồ.

Các trạng thái runtime chuẩn:


### 1.1. RESOLVED

RESOLVED nghĩa là resolver đã trả kết quả hợp lệ.

Consumer chỉ được sử dụng kết quả trong phạm vi allowed use.

CustomerIdentityResolver trả khách là Diamond.

QuoteSnapshotResolver trả quote hợp lệ.

AvailabilityResolver trả SKU sellable.

GoldenHourResolver trả phiên active.

PaymentStateResolver trả PAID.

ShippingStateResolver trả tracking hợp lệ.

Consumer được hành động nhưng vẫn phải qua guard tương ứng.


### 1.2. NOT_ELIGIBLE

NOT_ELIGIBLE nghĩa là có dữ liệu nhưng không đủ điều kiện.

Khách chưa đủ hạng Diamond.

SKU không đủ điều kiện Giờ Vàng.

Khách không đủ quyền mua sớm.

Order không đủ điều kiện commission.

Kênh không đủ điều kiện gửi CRM.

Địa chỉ không đủ điều kiện COD.

Consumer phải trả lời theo hướng không áp dụng / chưa đủ điều kiện, không được tự override.


### 1.3. bị chặn

bị chặn nghĩa là rule chặn hành động.

SKU bị recall hold.

Khách có open complaint case.

Payment chưa được xác nhận.

Health-sensitive chưa qua guard.

Public comment không được báo giá.

Completion Report chưa đủ evidence.

Consumer phải dừng hành động bị chặn.

Không được dùng fallback để vượt bị chặn.


### 1.4. MISSING_SOURCE

MISSING_SOURCE nghĩa là thiếu nguồn sự thật.

Không có Product Knowledge.

Không có Customer Memory.

Không có Member Lifecycle source.

Không có Official Contact Registry.

Không có Scientific Evidence source.

Không có Message Trigger source.

Consumer không được khẳng định.

Consumer không được bịa.

Consumer không được lấy dữ liệu từ cache cũ hoặc prompt.


### 1.5. RESOLVER_TIMEOUT

RESOLVER_TIMEOUT nghĩa là resolver không trả kết quả trong giới hạn.

Consumer không được hiểu timeout là “không có quyền” hoặc “không đủ điều kiện”.

Timeout không đồng nghĩa NOT_ELIGIBLE.

Timeout không đồng nghĩa bị chặn.

Timeout nghĩa là chưa có dữ liệu chắc chắn.

Consumer chỉ được dùng fallback an toàn nếu fallback đó không tạo cam kết nghiệp vụ.


### 1.6. CONFLICT

CONFLICT nghĩa là có hai hoặc nhiều source trả kết quả mâu thuẫn.

Product Knowledge nói SKU có thể tư vấn, nhưng Availability nói not sellable.

Member Core nói Gold, Customer Profile cache nói Silver.

CRM muốn gửi tin, nhưng OpenCaseSuppressionGuard chặn.

Landing Page nói sản phẩm đang mở bán, ProgramActivationResolver nói không active.

Admin UI thấy ảnh chuyển khoản, Payment Core chưa PAID.

MC AI script có SKU ngoài Daily Live Product Board.

Khi CONFLICT, consumer phải dừng quyết định và dùng conflict resolution order.


### 1.7. STALE

STALE nghĩa là dữ liệu đã hết hạn hoặc không còn hiệu lực.

QuoteSnapshot hết hạn.

Golden Hour session đã kết thúc.

Handoff context quá hạn.

Member benefit snapshot cũ.

CRM trigger đã hết window.

Tracking status quá cũ.

Consumer không được dùng dữ liệu stale để chốt, quote, order hoặc gửi CRM.


### 1.8. WAITING_REVIEW

WAITING_REVIEW nghĩa là cần human/admin/owner/accounting/CSKH review.

Payment proof cần kế toán đối soát.

Member dispute cần review.

Counterfeit case cần QA/CSKH.

Corporate / distributor request cần routing.

Claim khoa học chưa approved.

Production Planning Signal cần owner review.

Consumer không được tự kết luận.

Gateway chưa đủ handoff delivery log.

CRM chưa có suppression evidence.

Quote/order flow chưa có sample.

Security action thiếu audit evidence.

Không được production-ready.


### 1.10. PERMISSION_DENIED

PERMISSION_DENIED nghĩa là actor không có quyền.

Admin UI, backend, worker, Gateway, CRM, AI đều phải tôn trọng.

Không được bypass bằng UI.

Không được bypass bằng prompt.

Không được bypass bằng role text.


### 1.11. AUDIT_REQUIRED

AUDIT_REQUIRED nghĩa là action cần audit/reason/correlation nhưng chưa đủ.

Manual payment confirmation.

Member correction.

Hold / release / recall.

Refund approval.

Override.

Admin correction.

Nếu audit thiếu, action không được xem là hợp lệ.


### 1.12. OWNER_DECISION_REQUIRED

OWNER_DECISION_REQUIRED nghĩa là cần owner quyết định.

Áp dụng cho:

Future extension.

International shipping/payment.

New partner policy.

New commission policy.

New claim.

New product activation rule.

Production order from planning signal.

Consumer không được tự kích hoạt.


## 2. RUNTIME ACTION CONTROL

Mỗi consumer chỉ được hành động khi runtime trả trạng thái phù hợp.


### 2.1. AI Advisor

AI Advisor chỉ được:

Tư vấn sản phẩm khi Product Knowledge resolved.

Tư vấn hiệu dụng khi ClaimGuard pass.

Cá nhân hóa khi Customer Identity resolved.

Nhắc lịch sử mua khi Customer Memory resolved.

Hỏi thăm khách cũ khi LastPurchase resolved hoặc hỏi chung nếu chưa có.

Báo giá khi QuoteSnapshot resolved.

Nói quyền lợi member khi RuntimeRights resolved.

Nói Diamond benefit khi DiamondBind resolved.

Tạo OrderDraft khi quote còn hiệu lực.

AI Advisor không được:

Tự tạo order.

Tự xác nhận PAID.

Tự nói shipping ETA.

Tự nói khách là Diamond.

Tự nói khách mua qua link Diamond.

Tự nói khách từng mua sản phẩm cụ thể khi thiếu memory.

Tự hỏi bệnh khi khách không nêu bệnh.

Tự nói Gateway/Completion đã PASS.


### 2.2. CRM / Messaging

CRM chỉ được gửi khi:

CRMEligibilityGuard pass.

Customer identity resolved.

MessageTrigger resolved.

Message text source available.

Customer consent/channel policy pass.

Open case suppression pass.

Frequency cap pass.

Quiet hours pass.

Dedup pass.

Product Effectiveness present nếu gợi ý sản phẩm.

CRM không được:

Rewrite message.

Bịa quyền lợi.

Gửi member lifecycle khi thiếu runtime.

Gửi giá khi thiếu QuoteSnapshot.


### 2.3. Gateway

Gateway chỉ được:

Nhận normalized event.

Phân loại channel/surface.

Dedup webhook/comment.

Public safe reply.

Ghi handoff status.

Preserve page/live/comment/attribution context.

Chuyển context sang AI Advisor / Order / CRM trong boundary.

Gateway không được:

Tự quote.

Tự order.

Tự CRM.

Tự báo giá public.

Tự xác nhận đơn public.

Tự gửi payment instruction.

Tự lặp PII.

Tự nói đã gửi Messenger khi handoff fail.

Tự kéo abuse/troll vào Messenger.

Tự mở Gateway production khi Completion Report chưa PASS.


### 2.4. Landing Page / ADS / MC AI

Landing Page, ADS, MC AI chỉ được dùng SKU khi:

SKU active theo chương trình/kênh.

Có Product Knowledge.

Có Claim Policy.

Có price/program nếu hiển thị giá.

Có board nếu live/MC AI.

Không hold/recall/lock.

Không ngoài active list.

Hardcode SKU active.

Hardcode giá.

Hardcode shipping.

Hardcode payment.

Nói SKU ngoài board.

Public tồn kho.

Tạo khan hiếm giả.


### 2.5. Admin UI

Admin UI chỉ được:

Hiển thị dữ liệu từ core.

Gửi command đến core.

Thực hiện protected action khi permission pass.

Ghi audit khi action yêu cầu.

Hiển thị internal fields nếu RBAC pass.

Admin UI không được:

Tự set PAID.

Tự sửa member tier.

Tự release hold.

Tự override quote.

Tự xác nhận IVR.

Tự xác nhận payment khi thiếu accounting review.

Dùng UI state làm source-of-truth.


## 3. CROSS-DOMAIN CONFLICT RESOLUTION ORDER

Khi có conflict giữa nhiều domain, hệ thống xử lý theo thứ tự ưu tiên:


## 4. CUSTOMER IDENTITY CONFLICT CONTROL


### 4.1. Conflict examples

Customer identity conflict xảy ra khi:

Customer Profile nói khách là Gold nhưng Member Lifecycle nói khách đã lên Platinum.

AI memory nói khách cũ nhưng Customer Profile không xác nhận.

Khách nói “tôi là Diamond” nhưng runtime không xác nhận.

Khách vào qua link Diamond nhưng bind status không hợp lệ.

CRM nói khách là member nhưng Order History không có doanh số hợp lệ.

Admin UI cache còn dữ liệu cũ.


### 4.2. Resolution rule

Runtime Customer Identity thắng:

CustomerIdentityResolver-> MemberLifecycleResolver-> CustomerMemoryResolver-> OrderHistoryResolver-> Consumer display

Khách tự khai không thay thế runtime.

Chat history không thay thế Customer Profile.

CRM cache không thay thế Member Lifecycle.


### 4.3. bị chặn actions

Nếu identity conflict:

Không nói hạng.

Không báo quyền lợi Diamond.

Không cá nhân hóa lịch sử mua cụ thể.

Không tạo QuoteSnapshot có benefit cho đến khi runtime resolve.


## 5. CUSTOMER MEMORY CONFLICT CONTROL


### 5.1. Conflict examples

Customer memory conflict xảy ra khi:

Khách nói từng mua nhưng Order History không có.

Customer Memory nói mua A, Order History nói mua B.

CRM nói mua cho mẹ, Recipient Memory không có.

AI muốn hỏi “dùng dòng đó có hợp không” nhưng chưa biết dòng nào.

Khách phản hồi không hài lòng nhưng CRM muốn gửi mua lại.


### 5.2. Resolution rule

Order History và Customer Memory verified thắng lời suy đoán.

Nếu thiếu LastPurchase:

AI hỏi thăm chung.

Không nhắc tên sản phẩm cụ thể.

Nếu có negative feedback:

Service recovery thắng sales.

Không quote/order mới nếu vấn đề chưa xử lý.


### 5.3. bị chặn actions

Nếu customer memory conflict:

Không nói “lần trước Mình mua…” nếu chưa chắc.

Không nói “Mình mua cho mẹ…” nếu chưa có recipient source.

Không gửi CRM mua lại theo sản phẩm cũ.

Không member upsell khi trải nghiệm cũ chưa được xử lý.


## 6. MEMBER LIFECYCLE CONFLICT CONTROL


### 6.1. Conflict examples

Member lifecycle conflict xảy ra khi:

Member Benefit nói Gold nhưng Member Lifecycle đang grace.

CRM muốn gửi nâng hạng nhưng member runtime thiếu amount.

Khách dispute doanh số tích lũy.

QuoteSnapshot chưa áp member benefit nhưng AI nói đã giảm theo hạng.

Khách đang grace nhưng CRM gửi hạ hạng ngay.

Khách hết grace nhưng downgrade chưa được runtime xác nhận.


### 6.2. Resolution rule

Member Lifecycle Core thắng mọi consumer text.

QuoteSnapshot thắng lời AI/CRM về giá cuối.

Member dispute mở case, không tranh luận.


### 6.3. bị chặn actions

Không nói duy trì/nâng/hạ hạng.

Không nói số tiền còn thiếu.

Không nói ngày còn lại.

Không downgrade.

Không upgrade.

Nếu dispute open:

Không gửi CRM duy trì/nâng hạng.

Không nói “dữ liệu đúng rồi”.


## 7. DIAMOND / AFFILIATE / COMMISSION CONFLICT CONTROL


### 7.1. Conflict examples

Khách nói mua qua link Diamond nhưng bind fail.

Attribution có referral_link_id nhưng order không verified.

Khách vừa là member vừa mua qua link Diamond.

Diamond tự mua qua link của mình.

CRM muốn gửi commission nhưng order chưa đủ điều kiện.

Khách hỏi đại lý nhưng intent thực tế là Diamond affiliate.

Khách mua số lượng lớn nhưng không phải nhà phân phối.


### 7.2. Resolution rule

DiamondReferralResolver và CommissionEligibilityResolver quyết định.

OrderVerifiedResolver quyết định đơn có tính commission không.

PartnerPolicyResolver quyết định đại lý/nhà phân phối/mua sỉ.

OfficialContactResolver quyết định số liên hệ.


### 7.3. bị chặn actions

Nếu Diamond bind missing/fail:

Không tính commission.

Vẫn cho mua thường nếu sản phẩm sellable.

Nếu commission eligibility missing:

Không gửi Diamond commission message.

Không tạo payout/ledger.

Nếu partner intent chưa rõ:

Không route nhầm.

Hỏi thêm ngắn hoặc dùng Official Contact Registry nếu đủ intent.


## 8. RECOMMENDATION VS AVAILABILITY CONFLICT CONTROL


### 8.1. Conflict examples

Product Knowledge nói sản phẩm phù hợp nhưng Availability nói hết hàng.

Segment Recommendation chọn SKU nhưng SKU bị quality hold.

Product Triad chọn đủ 3 trụ nhưng một SKU not sellable.

CRM muốn gợi ý sản phẩm đã mua lại nhưng SKU đang sale lock.

ADS đang chạy SKU nhưng Product Activation đã tắt.


### 8.2. Resolution rule

Availability / Sellable Status thắng Product Recommendation.

Product Recommendation chỉ là đề xuất.

Sellable quyết định có được bán hay không.


### 8.3. bị chặn actions

Nếu SKU not sellable:

Không chốt.

Không ADS bán hàng.

Không Landing CTA mua.

Không CRM chương trình.

Chỉ gợi ý thay thế nếu replacement pass guard.


## 9. PROGRAM ACTIVATION VS STOCK CONFLICT CONTROL


### 9.1. Conflict examples

Program Policy muốn mở Giờ Vàng nhưng stock < 300.

Landing Page hiển thị SKU Giờ Vàng nhưng GoldenHourActivationResolver không active.

ADS chạy SKU 24/7 nhưng ProgramActivationResolver tắt.

MC AI script có SKU ngoài board.

24/7 discount tính từ đơn chưa verified.

Golden Hour session đã hết nhưng quote vẫn dùng giá cũ.


### 9.2. Resolution rule

ProgramActivationResolver + AvailabilityResolver + QuoteSnapshot thắng mọi display.

Daily Live Product Board thắng MC AI script.

QuoteLockResolver quyết định quote còn hạn.

SuccessfulSalesCountResolver chỉ tính đơn hợp lệ.


### 9.3. bị chặn actions

Không Landing active list.

Nếu stock không đủ:

Không mở Giờ Vàng.

Không quote Giờ Vàng.

Không tạo khan hiếm giả.

Nếu board mismatch:

MC AI không được phát script.

Segment phải REVALIDATION_REQUIRED.


## 10. QUOTE / ORDER CONFLICT CONTROL


### 10.1. Conflict examples

AI báo giá nhưng QuoteSnapshot thiếu.

QuoteSnapshot hết hạn nhưng khách xác nhận.

Customer sửa số lượng sau khi quote.

OrderDraft không khớp QuoteSnapshot.

Khách nói “chốt” ở public comment.

Duplicate webhook tạo order trùng.

IVR result gửi sau khi order đã expired.


### 10.2. Resolution rule

QuoteSnapshot thắng AI text.

OrderDraft phải dựa trên quote còn hiệu lực.

CustomerConfirmation bắt buộc.

IdempotencyResolver chặn duplicate.


### 10.3. bị chặn actions

Không OrderDraft.

Không order chính thức.

Không Order Success.

Không post-purchase CRM.


## 11. PAYMENT PROOF VS PAYMENT CORE CONFLICT CONTROL


### 11.1. Conflict examples

Khách gửi ảnh chuyển khoản nhưng Payment Core chưa xác nhận.

Admin thấy giao dịch nhưng chưa đối soát.

CRM muốn gửi “đã thanh toán” nhưng payment chưa đủ điều kiện.

Order state muốn fulfillment nhưng bank transfer đang WAITING_BANK_TRANSFER.

Bank webhook fail nhưng khách nói đã chuyển.


### 11.2. Resolution rule

Payment Core / Accounting Review thắng ảnh giao dịch, lời khách và UI display.

PAID chỉ hợp lệ khi PaymentStateResolver trả PAID hoặc Accounting Review xác nhận theo rule.


### 11.3. bị chặn actions

Nếu chưa Payment Core confirmation:

Không PAID.

Không gửi payment success CRM.

Ghi chờ đối soát.

Không gửi bank instruction.

Không tạo VietQR.


## 12. SHIPPING TRACKING VS FALLBACK ETA CONFLICT CONTROL


### 12.1. Conflict examples

Carrier tracking chưa có nhưng AI muốn nói ngày nhận.

CRM gửi ETA cũ nhưng Shipping Core có status mới.

Order đã có địa chỉ nhưng AI hỏi lại.

Landing Page hardcode “2–3 ngày”.

Carrier status stale.


### 12.2. Resolution rule

ShippingStateResolver thắng CRM/template/Landing Page.

CarrierTrackingLinkResolver thắng fallback.

Fallback chỉ dùng khi policy cho phép và không có tracking realtime.


### 12.3. bị chặn actions

Nếu thiếu ShippingEligibility:

Nếu thiếu tracking:

Không bịa trạng thái.

Nếu order có shipping info:

Không hỏi lại địa chỉ.


## 13. HEALTH INTENT VS SALES INTENT CONFLICT CONTROL


### 13.1. Conflict examples

Khách nói bệnh nhưng cũng muốn mua ngay.

Khách nói mua cho người đang điều trị.

Khách hỏi sản phẩm có chữa bệnh không.

Live comment hỏi bệnh nền.

AI muốn quote nhưng HealthSafetyGuard chưa pass.


### 13.2. Resolution rule

HealthSafetyGuard thắng Sales Intent.

Claim Policy thắng Product Recommendation.

Private surface thắng public surface khi health-sensitive.


### 13.3. bị chặn actions

Nếu health unresolved:

Không claim cụ thể nếu thiếu ClaimGuard.

Hỏi thêm trong private nếu cần.

Public comment phải handoff.

Không tự hỏi bệnh.

Không tự hỏi kiêng cữ.

Tư vấn theo bữa ăn thực dưỡng bình thường.


## 14. BRAND SCIENCE VS CLAIM POLICY CONFLICT CONTROL


### 14.1. Conflict examples

Product Knowledge có bằng chứng khoa học nhưng Claim Policy không cho wording mạnh.

AI muốn nói bài báo khoa học nhưng ApprovedPublicationLinkResolver thiếu.

ADS muốn dùng claim mạnh để tăng conversion.

CRM diễn giải nghiên cứu thành hiệu quả điều trị.

Landing Page nói “chữa” dựa trên thành phần.


### 14.2. Resolution rule

Claim Policy thắng mọi wording public.

Scientific Evidence Source quyết định bằng chứng.

ApprovedPublicationLinkResolver quyết định link public.

Brand Voice không được tạo claim y tế.


### 14.3. bị chặn actions

Không nói có bài báo cụ thể.

Không gửi link cụ thể.

Nếu thiếu ClaimGuard:

Không diễn giải hiệu dụng cụ thể.

Nếu có risk treatment claim:

Chặn render.

Chuyển wording thực dưỡng/public-safe.


## 15. MESSAGE TRIGGER VS CRM CONTENT CONFLICT CONTROL


### 15.1. Conflict examples

CRM muốn gửi tin nhưng trigger chưa có.

Message text đã duyệt nhưng AI paraphrase lại.

Khách đã nhận message cùng tháng.

Member tier chưa resolved nhưng message theo hạng muốn gửi.

Email được dùng sai event.

Quiet hour đang active.


### 15.2. Resolution rule

MessageTriggerRegistry thắng CRM content.

MessageTextImmutableSource thắng AI wording.

TierMessageEligibilityGuard thắng CRM personalization.

QuietHoursGuard / FrequencyCapGuard / DedupGuard thắng send intent.


### 15.3. bị chặn actions

Nếu thiếu trigger:

Nếu thiếu message text source:

Không rewrite.

Nếu dedup fail:

Không gửi.

Log suppression.


## 16. PUBLIC COMMENT VS MESSENGER PRIVATE CONFLICT CONTROL


### 16.1. Conflict examples

Public comment hỏi giá.

Public comment để số điện thoại.

Public comment muốn mua.

Public comment hỏi bệnh.

Public comment hỏi thanh toán/hóa đơn.

Public comment hỏi Diamond/commission.

Handoff fail nhưng template nói đã gửi.


### 16.2. Resolution rule

PublicPrivateSurfaceGuard thắng content intent.

MessengerHandoffResolver quyết định có handoff được không.

HandoffDeliveryStatusResolver quyết định có được nói “đã gửi” không.


### 16.3. bị chặn actions

Public surface không được:

Xác nhận order.

Lặp PII.

Gửi payment instruction.

Tư vấn health-sensitive dài.

Gửi commission/referral detail.

Nếu handoff fail:

Không nói đã gửi.

Dùng fallback an toàn.


## 17. LIVE MODERATION VS SALES CONFLICT CONTROL


### 17.1. Conflict examples

Comment chửi thề.

Comment phá live.

Brand attack.

Spam liên tục.

Comment có vẻ troll nhưng kèm mã đơn/mã lô.

Comment khiếu nại thật.


### 17.2. Resolution rule

LiveAbuseClassifier / ComplaintEvidenceGuard quyết định.

Abuse rõ -> no sales, no Messenger, moderation action.

Complaint thật -> case/CSKH, không bỏ qua.


### 17.3. bị chặn actions

Nếu abuse rõ:

Không CRM.

Không handoff Messenger.

Nếu complaint thật có evidence:

Không hide tùy tiện.

Route case.


## 18. COMPLETION REPORT VS MISSING EVIDENCE CONFLICT CONTROL


### 18.1. Conflict examples

Tài liệu đã viết nhưng evidence thiếu.

AI trả lời tốt một lần nhưng chưa có DecisionEnvelope.

UI chạy demo nhưng không có DB/audit trace.

Gateway handoff hoạt động nhưng không có delivery log.

Quote hiển thị nhưng không có QuoteSnapshot.

Order success hiển thị nhưng không có order_code sample.

CRM suppression nói đã có nhưng không có log.

Owner chưa sign-off.


### 18.2. Resolution rule

Evidence Registry thắng cảm tính “đã ổn”.

CompletionGateResolver quyết định PASS/chưa đủ điều kiện/FAIL.

OwnerSignOffResolver quyết định release approval.


### 18.3. bị chặn actions

Nếu thiếu P0 evidence:

Completion Report không PASS.

Gateway không mở.

Không được thay evidence bằng:

Demo miệng.

Tài liệu đã viết.

Log không có trace.


## 19. SUPPRESSION CONTROL

Suppression là cơ chế chặn hành động khi điều kiện không an toàn hoặc không phù hợp.

Suppression không phải lỗi.

Suppression là rule bảo vệ khách, thương hiệu và hệ thống.


### 19.1. CRM Suppression

CRM bị chặn nếu:

Opt-out.

Open complaint.

Refund case.

Payment dispute.

Privacy request.

Delivery issue.

Health review.

Counterfeit/quality case.

Member dispute.

Commission dispute.

Missing Product Effectiveness.

Missing runtime member data.

Missing trigger.

Dedup fail.


### 19.2. Sales Suppression

Sales bị chặn nếu:

SKU not sellable.

Health unresolved.

Counterfeit case.

Quote expired.

Missing QuoteSnapshot.

Missing CustomerConfirmation.

Public surface.

Permission denied.


### 19.3. Member Lifecycle Suppression

Member lifecycle message bị chặn nếu:

Member runtime missing.

Open case.

Amount data missing.

Tier data missing.

Grace status missing.

Message frequency exceeded.


### 19.4. Program Suppression

Program communication bị chặn nếu:

Program inactive.

SKU not active.

Stock below threshold.

Golden Hour ended.

Missing early access resolver.

Missing program eligibility.

Missing board validation.


### 19.5. Handoff Suppression

Messenger handoff bị chặn hoặc fallback nếu:

Handoff delivery fail.

Meta policy not allowed.

Duplicate event.

Abuse comment.

Missing channel context.

Missing customer/session context.

Missing page permission.


## 20. FALLBACK CONTROL

Fallback chỉ được dùng khi không tạo cam kết sai.

Fallback không được thay thế source-of-truth.

Fallback không được thay resolver.

Fallback không được bypass guard.


### 20.1. Fallback được phép

Fallback được phép trong các tình huống:

Thiếu Product Knowledge chi tiết nhưng có thể hỏi thêm nhu cầu.

Thiếu Customer Memory cụ thể nhưng có thể hỏi thăm chung.

Handoff fail nhưng có thể trả public safe fallback.

Tracking thiếu nhưng có thể nói fallback vùng nếu policy cho phép.

Missing health detail nhưng có thể hỏi thêm trong private.

Missing segment nhưng có thể hỏi thêm một câu ngắn.

Missing official contact purpose nhưng có thể hỏi rõ mục đích liên hệ.


### 20.2. Fallback bị cấm

Final price.

Member rights.

Payment instruction.

Shipping ETA chính xác.

COD eligibility.

Program active list.

Golden Hour active.

IVR result.

Official phone number.

Scientific publication link.


### 20.3. Fallback wording principle

Fallback customer-facing phải:

Không nói lỗi kỹ thuật.

Không lộ resolver/guard.

Không nói “hệ thống lỗi”.

Không bịa dữ liệu.

Không cam kết sai.

Không làm khách hoang mang.

Không làm mất vai trò tư vấn.

Không chuyển trách nhiệm cho khách nếu kênh có thể xử lý private.


## 21. NO-HARDCODE ENFORCEMENT

Không được hardcode dữ liệu quyết định trong:

AI prompt.

Gateway template.

CRM template.

ADS config.

MC AI script.

Static content.

Frontend component.

Worker logic.

Test seed dùng nhầm production.


### 21.1. No-hardcode runtime data

Discount.

Member benefit.

Program active status.

Golden Hour session.

Quote hold time nếu không từ resolver.

Final total.

Stock.

Shipping fee.

IVR interpretation.

Customer tier.

Customer revenue.

Amount to next tier.

Official phone.

Publication link.


### 21.2. No-hardcode message and content

Không được hardcode hoặc rewrite:

Message text approved.

Tier-specific message.

Birthday message.

Golden Hour message.

Diamond commission message.

Policy link.

Scientific evidence wording nếu source yêu cầu.

Official contact wording.

Claim-sensitive wording.

Content block chỉ render từ biến đã approved.


### 21.3. No-hardcode business decision

SKU 24/7 active list.

SKU Golden Hour active list.

Daily Live Product Board.

Product triad selection.

Affiliate eligibility.

Distributor routing.

Suppression.

Outcome.

Các quyết định này phải qua resolver.


## 22. CACHE / STALE DATA CONTROL

Cache không được trở thành source-of-truth.

Cache chỉ dùng nếu:

Có TTL.

Có version.

Có source reference.

Không ảnh hưởng P0 runtime decision.

Không stale.

Dữ liệu stale phải bị chặn cho:

Price.

Diamond bind.

Shipping state.

Evidence status.

Nếu stale:

RevalidatehoặcBlockhoặcWAITING_REVIEW

Không dùng stale để chốt.


## 23. IDEMPOTENCY / DUPLICATE CONTROL

Idempotency bắt buộc cho:

Webhook.

Comment event.

Messenger handoff.

Quote creation.

Order creation.

Payment callback.

Bank webhook.

CRM message job.

Commission event.

Member lifecycle job.

Production planning signal.

Duplicate không được tạo:

Nhiều public reply.

Nhiều Messenger handoff.

Nhiều QuoteSnapshot.

Nhiều order.

Nhiều CRM message.

Nhiều commission.

Nhiều outcome log sai.

Nhiều IVR state update.


## 24. FUTURE EXTENSION CONTROL

Future extension không được hiểu là active scope.

Các nội dung sau chỉ được kích hoạt khi có owner approval và source governance mới:

International shipping.

International payment.

New distributor policy.

New commission model.

New affiliate tier.

New health claim.

New CRM channel.

New IVR sales/CRM use case.

New product category.

Nếu chưa owner-approved:

AI không được hứa.

CRM không được gửi.

Landing Page không được hiển thị.

Gateway không được route như active.

Dev không được tự bật feature.

ADS không được chạy offer.


## 25. PART 3 DONE GATE


## PHẦN 3/4 đạt khi:

Runtime result status model đã khóa.

AI action control đã khóa.

CRM action control đã khóa.

Gateway action control đã khóa.

Landing/ADS/MC AI control đã khóa.

Admin UI control đã khóa.

Cross-domain priority order đã khóa.

Customer identity conflict đã khóa.

Customer memory conflict đã khóa.

Member lifecycle conflict đã khóa.

Diamond/affiliate/commission conflict đã khóa.

Recommendation vs Availability conflict đã khóa.

Program activation vs stock conflict đã khóa.

Quote/order conflict đã khóa.

Payment proof vs Payment Core conflict đã khóa.

Shipping tracking vs fallback conflict đã khóa.

Health intent vs sales intent conflict đã khóa.

Brand science vs Claim Policy conflict đã khóa.

Message trigger vs CRM content conflict đã khóa.

Public comment vs Messenger private conflict đã khóa.

Live moderation vs sales conflict đã khóa.

Completion vs missing evidence conflict đã khóa.

Suppression control đã khóa.

Fallback control đã khóa.

No-hardcode enforcement đã khóa.

Cache/stale data control đã khóa.

Idempotency/duplicate control đã khóa.

Future extension control đã khóa.

Không consumer nào được tự suy luận.

Không fallback nào được dùng để bypass P0 runtime source.

Không evidence thì không release.


## 26. TRẠNG THÁI SAU PHẦN 3/4


## 27. PHẦN TIẾP THEO


## PHẦN 4/4 — FINAL DONE GATE / SMOKE MAPPING / RELEASE CONTROL / MASTER-02 FINAL CONCLUSION


## PHẦN 4/4 sẽ khóa:


## 0. MỤC ĐÍCH CỦA PHẦN 4/4


## PHẦN 4/4 khóa điều kiện hoàn tất cuối cùng của MASTER-02.

Nếu PHẦN 1/4 khóa nguyên tắc dependency, PHẦN 2/4 khóa registry theo business domain, PHẦN 3/4 khóa runtime control, conflict, suppression, fallback và no-hardcode, thì PHẦN 4/4 khóa:

Done Gate cuối cho từng domain.

Smoke mapping bắt buộc.

Evidence mapping bắt buộc.

Điều kiện Completion Report.

Điều kiện Gateway transition.

Điều kiện Production Ready.

Điều kiện Release / Go-live.

Pack handoff.

Trạng thái cuối của MASTER-02.

Kết luận final của MASTER-02.


## PHẦN 4/4 không xác nhận hệ thống đã triển khai xong.


## PHẦN 4/4 không xác nhận Gateway được mở.


## PHẦN 4/4 không xác nhận production-ready.


## PHẦN 4/4 chỉ xác nhận MASTER-02 đã hoàn tất vai trò Cross-Pack Dependency Governance nếu các domain, source, owner, consumer, resolver, bị chặn-if-missing, evidence và done gate đã được khóa đầy đủ.


## 1. FINAL DONE GATE MODEL


### 1.1. Gate 1 — Business Domain Coverage

MASTER-02 phải bao phủ đủ 16 domain chính:


### 1.2. Gate 2 — Source ID Coverage

Mỗi domain phải có Source ID rõ ràng.

Source ID phải xác định được:

Source nào là nguồn sự thật.

Source nào là runtime decision.

Source nào là approved content.

Source nào là evidence.

Source nào là release gate.

Không được để domain dùng source mơ hồ như:

AI tự biếtCRM tự biếtDev tự xử lýFrontend tự tínhLanding Page tự hiển thịAdmin tự xác nhậnGateway tự route

Nếu source không rõ, domain không đạt Done Gate.


### 1.3. Gate 3 — Owner Core Coverage

Mỗi domain phải có Owner Core / Owner Pack rõ ràng.

Owner Core quyết định nghiệp vụ.

Customer Profile Core quyết định customer identity.Customer Memory Core quyết định lịch sử mua.Member Lifecycle Core quyết định hạng, duy trì, ân hạng, hạ hạng.Commerce Pricing Core quyết định giá.Program Policy Core quyết định 24/7 và Giờ Vàng.Commerce Availability Core quyết định sellable.Payment Core quyết định thanh toán.Shipping Core quyết định vận chuyển.Order Core quyết định order state.Gateway Pack quyết định normalized event / handoff status.Evidence Registry quyết định evidence.Security Pack quyết định permission / audit.

Nếu owner không rõ, domain không đạt Done Gate.


### 1.4. Gate 4 — Consumer Boundary Coverage

Mỗi domain phải khóa consumer được làm gì và không được làm gì.

Các consumer chính:

AI AdvisorGatewayCRMMessagingLanding PageADSMC AIAdmin UIOrder CorePayment CoreShipping CoreEvidence PackCompletion ReportBI / Dashboard

Nếu consumer có thể tự tính, tự quyết, tự ghi trạng thái hoặc tự bypass owner core, domain không đạt Done Gate.


### 1.5. Gate 5 — Runtime Resolver / Guard Coverage

Mỗi domain P0 phải có resolver / guard bắt buộc.

Các quyết định sau không được chạy nếu thiếu resolver/guard:

Diamond/referral.

Product sellable.

Public/private handoff.

Health boundary.

Moderation.

Permission/audit.

Nếu domain có runtime decision nhưng không có resolver/guard, domain không đạt Done Gate.


### 1.6. Gate 6 — bị chặn-if-missing Coverage

Mỗi domain phải có bị chặn-if-missing.

Nếu domain thiếu bị chặn-if-missing, domain không đạt Done Gate.


### 1.7. Gate 7 — Evidence Requirement Coverage

Mỗi domain P0 phải có evidence requirement.

Evidence không được là cảm tính.

Evidence không được chỉ là câu trả lời nghe đúng.

Evidence phải có trace, log, snapshot, audit hoặc record phù hợp.

Các evidence bắt buộc có thể gồm:

DecisionEnvelopeResolver traceGuard traceQuoteSnapshotOrderDraftCustomerConfirmationorder_codeHandoff delivery logCRM job logMessage send logDedup logOutcome logPayment referenceAccounting review auditShipping tracking recordModeration action logCompletion gate evidenceOwner sign-off

Nếu domain P0 không có evidence requirement, domain không đạt Done Gate.


### 1.8. Gate 8 — No-hardcode Coverage

MASTER-02 phải chặn hardcode trong toàn hệ.


### 1.9. Gate 9 — Release State Integrity

MASTER-02 phải giữ đúng trạng thái toàn hệ:

MASTER-02 không được tự chuyển trạng thái hệ thống.


### 1.10. Gate 10 — Pack Handoff Coverage

MASTER-02 phải chỉ rõ pack chi tiết chịu trách nhiệm triển khai sâu.

Không được để dev tự đoán.

Mỗi domain phải map về các pack như:

FILE 01 — Parent LogicFILE 04 — Resolver / Guard / Runtime ContractFILE 05 — Implementation LayerFILE 07 — Common Content BlocksFILE 09 — Test MatrixFILE 10 — Situation RegistryFILE 12 — Human-Level Sales BehaviorCommerce Pricing CorePayment CoreShipping CoreOrder CoreGateway PackCRM / Messaging PackEvidence PackSecurity PackOperational CoreProduction Planning Pack

Nếu domain không có pack handoff, domain không đạt Done Gate.


## 2. FINAL DONE GATE THEO 16 DOMAIN


### 2.1. DEP-DOM-01-GOV — Governance / Source-of-Truth

PASS khi:

Source-of-Truth rõ.

Owner core rõ.

Consumer boundary rõ.

Runtime source không bị consumer override.

Evidence source rõ.

Release state không bị tự chuyển.

Protected operations đi qua security/audit.

Completion Report không PASS nếu thiếu evidence.

Gateway vẫn bị chặn nếu P0 evidence thiếu.

Không có source song song.

FAIL nếu:

Pack chi tiết tự tạo source.

Dev tự suy luận rule.

Gateway tự mở.

Completion tự PASS.


### 2.2. DEP-DOM-02-PROD-CLAIM-BRAND — Product / Claim / Brand / Science

Product Knowledge là source chính.

Claim Whitelist kiểm soát public wording.

Brand Voice giữ đúng “Ngon như Mẹ nấu — thương ngay từ vị đầu tiên”.

Scientific Evidence chỉ dùng khi approved.

Publication link chỉ từ approved resolver.

Product Effectiveness có trong tư vấn, CRM, combo, proposal khi yêu cầu.

Không thuốc hóa sản phẩm.

Không bán nguyên liệu/cây giống nếu không thuộc scope.

AI tự tạo claim.

Landing Page publish claim ngoài whitelist.

CRM gợi ý sản phẩm thiếu Product Effectiveness.

ADS dùng claim điều trị.

AI nói bài báo khoa học khi thiếu source.

Sản phẩm bị gọi bằng SKU nội bộ.


### 2.3. DEP-DOM-03-CUSTOMER — Customer Identity / Customer Memory

Customer identity resolved trước khi cá nhân hóa.

Customer stage rõ: new / returning / member / Diamond.

Customer memory không bị bịa.

Last purchase chỉ nói khi resolver trả.

Last recipient chỉ nói khi source có.

Khách cũ được hỏi thăm trước khi bán tiếp.

Negative feedback mở care flow.

Open case chặn sales/CRM.

Customer history không lấy từ chat không xác thực.

Quote benefit không áp nếu identity chưa rõ.

AI bịa khách cũ.

AI bịa hạng.

AI bịa lịch sử mua.

AI bán ngay khi khách cũ quay lại.

CRM gửi lifecycle khi chưa xác định khách.

Open case vẫn upsell.


### 2.4. DEP-DOM-04-RECOMMENDATION — Segment / Product Recommendation

Product recommendation đi qua Product Knowledge.

Deep recommendation có 3 trụ: mùa / chức năng / bổ dưỡng.

Family add-on hoạt động theo ngữ cảnh.

Segment recommendation hỗ trợ văn phòng, sinh viên, du học sinh, Việt kiều, công ty, trường học, bệnh viện, cơ quan, quà tặng.

Product Effectiveness có từng item.

Dietary/allergy/health guard pass.

Availability pass nếu có ý định bán.

QuoteSnapshot có nếu báo giá.

Không ép combo.

Không dùng một bộ gợi ý cho mọi khách.

AI gợi ý ngẫu nhiên.

Gợi ý sai chay/mặn.

Gợi ý sản phẩm dị ứng.

Gợi ý không có Product Effectiveness.

Báo giá khi thiếu quote.

Segment công ty bị route nhầm đại lý.


### 2.5. DEP-DOM-05-CRM-MESSAGE — CRM 12-Month / Message Trigger / Suppression

CRM là lifecycle 12 tháng.

CRM trigger rõ.

Message text immutable.

Không tự tạo trigger.

Không gửi sai hạng.

Không gửi trùng trong tháng nếu rule cấm.

Cross-channel dedup hoạt động.

Opt-out chặn gửi.

Open case chặn gửi.

Product recommendation trong CRM có Product Effectiveness.

CRM job log có evidence.

Message send log có evidence.

CRM gửi khi opt-out.

CRM gửi khi complaint open.

AI rewrite message text.

Gửi trùng đa kênh.

CRM bịa lịch sử mua.

CRM bịa quyền lợi.


### 2.6. DEP-DOM-06-MEMBER — Member Lifecycle / Rights / Downgrade / Dispute

Member tier từ runtime.

Doanh số tích lũy từ runtime.

Amount to maintain từ runtime.

Amount to next tier từ runtime.

Days to cycle end từ runtime.

Grace status từ runtime.

Upgrade / maintain / downgrade từ Member Lifecycle Core.

Downgrade safe tone.

Member dispute mở case.

Quote benefit chỉ áp khi QuoteSnapshot ghi nhận.

Member outcome được log.

CRM member lifecycle bị suppress khi open case.

AI tự tính hạng.

CRM tự nói số còn thiếu.

AI tự hạ hạng.

AI dọa khách hạ hạng.

Member dispute vẫn upsell.

Quote hiển thị benefit không có runtime.


### 2.7. DEP-DOM-07-DIAMOND-PARTNER — Diamond / Affiliate / Distributor Boundary

Diamond identity từ runtime.

Diamond link bind pass mới áp quyền lợi.

Buyer link invalid vẫn mua thường được nếu SKU sellable.

Commission chỉ ghi khi order eligible.

Commission không hardcode.

Self-purchase guard áp dụng nếu policy yêu cầu.

Affiliate/Diamond tách khỏi đại lý/nhà phân phối.

Distributor/wholesale route đúng policy.

Official contact đúng.

Commission dispute mở case.

AI gộp Diamond với đại lý.

AI hardcode hoa hồng.

AI nói quyền lợi Diamond khi bind chưa pass.

Commission tính trước order eligible.

Đại lý/mua sỉ bị xử lý như khách lẻ.

Diamond hỏi khởi nghiệp bị trả lời vòng vo.


### 2.8. DEP-DOM-08-PRICING-PROGRAM — Pricing / 24-7 / Golden Hour / QuoteSnapshot

Giá từ Pricing Core.

24/7 từ Program24_7Resolver.

Giờ Vàng từ GoldenHourResolver.

Early access từ EarlyAccessResolver.

QuoteSnapshot có trước báo giá.

Quote hold time từ QuoteLockResolver.

Policy priority từ Runtime Core.

Không cộng dồn quyền lợi tùy tiện.

Quote expired phải revalidate.

Public comment không báo giá.

Order không tạo từ quote hết hạn.

AI tự nói Giờ Vàng active.

AI tự nói quyền mua sớm.

CRM gửi giá thiếu quote.

Quote hết hạn vẫn tạo order.

Discount 24/7 tính từ đơn chưa hợp lệ.

Policy priority bị consumer tự chọn.


### 2.9. DEP-DOM-09-ACTIVATION — Program / Sellable / Product Activation / Production Signal

SKU chỉ mở bán khi sellable.

Thành phẩm nhập kho hợp lệ.

stock_available > 0.

listed_price_status = ACTIVE.

SKU not sellable tự ngưng bán.

24/7 active list từ resolver.

Golden Hour active list từ resolver.

Daily Live Product Board được validate.

MC AI không nói SKU ngoài board.

ADS/CRM/Landing không hardcode active SKU.

Sales / stock movement tạo Production Planning Signal.

Production signal không tự thành production order.

Bán SKU hết hàng.

Bán SKU hold/recall.

Báo quote SKU không sellable.

Landing Page hardcode Giờ Vàng.

ADS chạy SKU không active.

Production order tự tạo từ signal chưa review.


### 2.10. DEP-DOM-10-PAYMENT — Payment / Bank Transfer / Accounting Review

Payment method từ Payment Core.

Bank account từ Bank Registry.

Bank transfer có payment_reference.

VietQR nếu có phải từ Payment Instruction.

Bank transfer vào Accounting Queue.

Payment proof không tự PAID.

Manual reconciliation có audit.

Bank webhook nếu có được trace.

PAID chỉ khi Payment Core / Accounting Review xác nhận.

AI/Gateway/CRM không xác nhận thanh toán thay core.

Thiếu payment_reference.

AI nói đã thanh toán vì khách gửi ảnh.

Admin UI set PAID không audit.

Accounting queue thiếu.

Fulfillment chạy khi payment chưa đủ điều kiện và policy yêu cầu paid.


### 2.11. DEP-DOM-11-SHIPPING — Shipping / Tracking / ETA / COD

Shipping eligibility từ Shipping Core.

COD eligibility từ Shipping Core.

Shipping fee từ resolver.

Tracking từ carrier/core.

ETA từ resolver.

Fallback vùng chỉ dùng khi policy cho phép.

Không bịa carrier status.

Không hứa giao quốc tế nếu không active scope.

MAS-SMK-006 pass.

AI tự hứa ETA.

CRM gửi tracking giả.

Hỏi lại địa chỉ đã có.

Hứa COD khi chưa eligible.

Shipping fee từ frontend text.


### 2.12. DEP-DOM-12-ORDER-IVR — Order / OrderDraft / CustomerConfirmation / IVR

QuoteCart được tạo đúng.

QuoteSnapshot có trước quote.

OrderDraft là form xác nhận.

Khách cũ có prefill nếu policy cho phép.

Order tạo từ quote còn hiệu lực.

Order Success chỉ khi có order_code.

Duplicate không tạo nhiều order.

IVR chỉ nhận queue từ Order Core.

IVR gửi result về Order Core.

IVR không đổi order state.

Technical error không tính là no-answer.

AI tạo order khi khách chưa xác nhận.

Public comment tạo order.

Order success thiếu order_code.

Quote expired vẫn order.

SIM Gateway tự cập nhật order.

Duplicate webhook tạo nhiều order.


### 2.13. DEP-DOM-13-GATEWAY-HANDOFF-MODERATION — Gateway / Public Comment / Handoff / Moderation

Channel context resolved.

Public/private surface resolved.

Public hỏi giá không báo giá.

Public muốn mua không xác nhận đơn.

Public để PII không lặp PII.

Public payment/invoice không gửi instruction.

Public health không tư vấn dài.

Handoff success mới nói đã gửi.

Handoff fail không nói đã gửi.

Không kêu khách tự inbox nếu system handoff có thể chạy.

Context page/live/comment/attribution được giữ.

Duplicate webhook không tạo duplicate reply.

Abuse rõ không handoff.

Complaint thật route case.

Public lặp PII.

Handoff fail vẫn nói đã gửi.

Comment troll được kéo Messenger.

Mất attribution sau handoff.


### 2.14. DEP-DOM-14-CONTACT — Official Contact / Phone Number Registry

Số điện thoại từ Official Contact Registry.

Contact purpose resolved.

Visit request route đúng.

Company/leader contact route đúng.

Personal leader phone bị chặn.

Distributor/wholesale route đúng.

Affiliate không route nhầm distributor.

Contact surface pass.

Không có số ngoài registry.

Không hardcode contact trong template/static page.

AI tự bịa số.

AI gửi số cá nhân lãnh đạo.

Đại lý/affiliate route sai.

Landing Page hardcode contact.

Contact không đúng mục đích.


### 2.15. DEP-DOM-15-HEALTH-CASE-CONFLICT — Health / Complaint / Priority Conflict

Khách không nêu bệnh thì AI không tự hỏi bệnh.

Khách nêu bệnh thì HealthSafetyGuard chạy.

Health unresolved chặn quote/order.

Không treatment claim.

Public health-sensitive kéo private.

Allergy/dietary guard hoạt động.

Complaint chặn CRM.

Refund chặn upsell.

Privacy request chặn CRM.

Payment dispute chặn sales.

Counterfeit mở case.

Priority conflict thắng sales.

AI tự hỏi bệnh khi khách không nêu.

AI quote khi health unresolved.

AI nói chữa bệnh.

Counterfeit xử lý như đổi trả thường khi chưa xác minh.


### 2.16. DEP-DOM-16-EVIDENCE-SECURITY — Evidence / Completion / Gateway / Security

Evidence Registry đầy đủ.

Completion gates có status.

DecisionEnvelope sample có.

Resolver trace có.

Guard trace có.

Handoff delivery log có.

QuoteSnapshot sample có.

OrderDraft sample có.

CustomerConfirmation sample có.

order_code sample có.

CRM suppression evidence có.

Duplicate/idempotency evidence có.

MC AI board/script evidence có nếu scope có.

ROAS internal-only evidence có nếu scope có.

Permission/audit evidence có.

Owner sign-off có.

Gateway transition chỉ khi P0 pass.

Completion PASS thiếu evidence.

Gateway mở khi P0 chưa đủ điều kiện/fail.

Screenshot thay DecisionEnvelope.

Protected action thiếu permission/audit.


## 3. SMOKE MAPPING BẮT BUỘC

MASTER-02 khóa các smoke group bắt buộc sau.


### 3.1. MAS-SMK-001 — Source-of-Truth / Owner / Consumer Boundary

Mục tiêu

Chứng minh consumer không tự tạo source-of-truth.

Domain liên quan

DEP-DOM-01Tất cả domain còn lại

Expected Result

Mọi consumer đọc từ owner core / resolver.

Fail nếu

AI tự tính.

CRM tự gửi quyền lợi.

Gateway tự order.

UI tự set state.

Frontend hardcode runtime data.


### 3.2. MAS-SMK-002 — Product / Claim / Brand / Science

Chứng minh tư vấn sản phẩm đúng Product Knowledge, Claim Policy, Brand Voice và Scientific Evidence.

Product public name đúng.

Product Effectiveness có.

ClaimGuard pass.

Science proof dùng approved source.

“Ngon như Mẹ nấu — thương ngay từ vị đầu tiên” dùng đúng ngữ cảnh.

Claim điều trị.

Bịa bài báo.

Thiếu Product Effectiveness.

Dùng SKU nội bộ.

Hạ thấp science proof thành tài liệu nội bộ.


### 3.3. MAS-SMK-003 — Customer Identity / Customer Memory

Chứng minh AI biết khách là ai và khách cũ được hỏi thăm trước khi bán tiếp.

New customer không bị cá nhân hóa giả.

Returning customer được care before sale.

Last purchase đúng.

Last recipient đúng nếu có.

Open case chặn sales.

Bán ngay cho khách cũ.

Nhắc sai sản phẩm cũ.

Upsell khi khách chưa hài lòng.


### 3.4. MAS-SMK-004 — CRM 12-Month / Message Trigger / Suppression

Chứng minh CRM lifecycle, message trigger và suppression hoạt động đúng.

CRM D0/D7/D21/M7/M12 đúng trigger.

Message text không bị rewrite.

Đúng hạng thành viên.

Không gửi trùng tháng.

Opt-out suppress.

Open case suppress.

Quiet hour respected.

Product Effectiveness có trong recommendation.

CRM gửi sai trigger.

CRM gửi sai hạng.

CRM bịa history.

CRM gửi khi opt-out/open case.

CRM thiếu Product Effectiveness.


### 3.5. MAS-SMK-005 — Member Lifecycle / Rights / Downgrade / Dispute

Chứng minh member lifecycle chạy theo runtime.

Member tier đúng.

Maintain amount đúng.

Upgrade amount đúng.

Grace period đúng.

Downgrade safe.

Dispute opens case.

Outcome logged.

AI tự nói còn thiếu.

Hạ hạng dọa khách.

Dispute vẫn upsell.

Quote benefit không có runtime.


### 3.6. MAS-SMK-006 — Recommendation / Segment / Product Triad

Chứng minh gợi ý sản phẩm theo 3 trụ, người thân và segment.

Có seasonal product.

Có functional product.

Có nourishing product.

Có family add-on nếu context có.

Office/student/overseas/company/school/hospital scenarios pass.

Product Effectiveness per item.

Availability guard pass.

Gợi ý ngẫu nhiên.

Cùng một bộ sản phẩm cho mọi khách.

Không có 3 trụ.

Không claim guard.


### 3.7. MAS-SMK-007 — Diamond / Affiliate / Distributor / Wholesale

Chứng minh Diamond, affiliate, distributor, wholesale được phân luồng đúng.

Diamond resolved.

Diamond bind pass.

Invalid bind vẫn mua thường được.

Commission only after eligible order.

Affiliate không nhầm distributor.

Distributor/wholesale contact route đúng.

No hardcoded commission.

Hardcode hoa hồng.

Tính commission trước order eligible.

Diamond hỏi affiliate bị route đại lý.

Mua số lượng lớn tự xem là distributor.


### 3.8. MAS-SMK-008 — Pricing / 24-7 / Golden Hour / QuoteSnapshot

Chứng minh giá và chương trình đi qua runtime.

24/7 initial 9%.

24/7 <50 sales -> 9%.

24/7 >=50 sales -> 5%.

Golden Hour active từ resolver.

Early access đúng hạng.

Quote hold 5 phút Giờ Vàng.

Quote hold 15 phút 24/7.

Quote expired revalidate.

Public không báo giá.

Quote thiếu snapshot.

Giờ Vàng active bị bịa.

Quote hết hạn vẫn order.


### 3.9. MAS-SMK-009 — Program / Sellable / Product Activation / Production Signal

Chứng minh quy tắc mở bán, ngưng bán, active SKU và production signal.

Sellable gate pass.

Out of stock auto stop.

Quality hold blocks sale.

Recall hold blocks sale.

Listed price active required.

24/7 active SKU list from resolver.

Golden Hour active SKU list from resolver.

Daily Live Board validation.

MC AI no non-board SKU.

Production planning signal logged.

Bán SKU not sellable.

Báo quote SKU hold.

Landing hardcode active list.

Production order tự tạo từ signal.


### 3.10. MAS-SMK-010 — Payment / Bank Transfer / Accounting Review

Chứng minh payment đi qua Payment Core và Accounting Review.

Payment eligibility pass.

Bank account from registry.

Unique payment_reference.

Bank transfer tagged.

Accounting queue record.

Payment proof attached if allowed.

Manual reconciliation audit.

AI no paid before Core confirms.

AI nói PAID vì khách gửi ảnh.

Admin set PAID thiếu audit.

Payment queue thiếu.


### 3.11. MAS-SMK-011 — Shipping / Tracking / ETA / COD

Chứng minh shipping theo Shipping Core.

Shipping eligibility pass.

COD eligibility pass.

Shipping fee from core.

Tracking link returned.

Fallback ETA by region if allowed.

No fake tracking status.

No address re-ask when order has info.

Landing hardcode free ship.


### 3.12. MAS-SMK-012 — Order / OrderDraft / CustomerConfirmation / IVR

Chứng minh sales-to-order đi đúng gate.

QuoteCart sample.

OrderDraft form.

Prefill old customer if policy allows.

CustomerConfirmation sample.

Order created from confirmed quote.

order_code sample.

Duplicate idempotency.

IVR result handled by Order Core.

Technical error not no-answer.

Order tạo khi chưa confirmation.

Duplicate tạo nhiều đơn.


### 3.13. MAS-SMK-013 — Gateway / Handoff / Moderation

Chứng minh public/private boundary, handoff và moderation hoạt động đúng.

Public price bị chặn.

Public order bị chặn.

PII not repeated.

Payment public bị chặn.

Health public bị chặn.

Handoff success event.

Handoff failed event.

Public reply after success/fail đúng.

Context preserved.

Duplicate webhook no duplicate reply.

Abuse no Messenger.

Real complaint routed.

Comment troll vào Messenger.

Gateway tự quote/order.

Mất attribution.


### 3.14. MAS-SMK-014 — Official Contact Registry

Chứng minh mọi số điện thoại theo registry.

Visit request returns approved contact.

Company/leader contact returns approved contact.

Distributor/wholesale route correct.

Affiliate not misrouted.

No unapproved phone.

Số cá nhân lãnh đạo bị lộ.

Contact hardcoded.

Route sai mục đích.


### 3.15. MAS-SMK-015 — Health / Complaint / Priority Conflict

Chứng minh health boundary và priority conflict thắng sales.

No unsolicited disease question.

Disease mention triggers guard.

Health unresolved blocks quote.

No treatment claim.

Public health detail bị chặn.

Complaint suppresses CRM.

Payment dispute suppresses sales.

Counterfeit opens case.

Member dispute opens case.

AI hỏi bệnh khi khách không nêu.

Counterfeit xử lý như đổi trả thường.


### 3.16. MAS-SMK-016 — Evidence / Completion / Gateway / Security

Chứng minh Completion Report, Gateway Gate, evidence và security đạt điều kiện release.

DecisionEnvelope sample.

CRM suppression evidence.

Duplicate/idempotency evidence.

Security/permission/audit evidence.

Screenshot thay evidence.

Protected action thiếu audit.


## 4. EVIDENCE MAPPING BẮT BUỘC

Mọi smoke P0 phải có evidence item.

Evidence item tối thiểu gồm:

evidence_iddomain_idsmoke_idpriorityownerstatustested_attested_byrequest_payload_if_anyresponse_payload_if_anyDecisionEnvelope_if_AI_relatedresolver_trace_if_anyguard_trace_if_anyDB_snapshot_if_side_effectevent_or_outbox_record_if_anyhandoff_delivery_log_if_anyquote_snapshot_if_quoteorder_code_if_orderscreenshot_if_UI_or_opsowner_note_if_manual_reviewfail_reason_if_anyrequired_fix_file_if_anyowner_decision_required

Không được đánh PASS nếu thiếu evidence bắt buộc.


## 5. COMPLETION REPORT CONDITION

Completion Report chỉ được PASS khi:

Tất cả P0 domain smoke PASS.

Có DecisionEnvelope mẫu.

Có QuoteSnapshot sample.

Có OrderDraft sample.

Có CustomerConfirmation sample.

Có order_code sample.

Có payment/accounting evidence nếu payment thuộc scope.

Có shipping evidence nếu shipping thuộc scope.

Có MC AI board/script/video evidence nếu MC AI thuộc scope.

Có ROAS internal-only evidence nếu ADS/ROAS thuộc scope.

Có security/permission/audit evidence.

Nếu thiếu bất kỳ P0 evidence nào:

Nếu có bất kỳ P0 fail:


## 6. GATEWAY TRANSITION CONDITION

Gateway production chỉ được xem xét khi:

Gateway production bị chặn nếu:

Any P0 gate chưa đủ điều kiện.

Any P0 gate FAIL.

Missing evidence.

Missing owner sign-off.

Public/private boundary fail.

Handoff fail evidence missing.

QuoteSnapshot missing.

CustomerConfirmation missing.

CRM suppression missing.

Duplicate/idempotency fail.

Security/audit evidence missing.

MC AI board/script fail nếu thuộc scope.

ROAS internal-only fail nếu thuộc scope.

Gateway không được dùng để che lấp gap của AI Advisor.

Gateway không được tự quote/order/CRM.


## 7. PRODUCTION READY CONDITION

Production Ready chỉ được xem xét khi:

MASTER governance pass.

Pack implementation pass.

Runtime resolvers hoạt động.

Guards hoạt động.

P0 smoke pass.

Evidence package complete.

Completion Report PASS.

Gateway transition approved nếu có Gateway.

Security / permission / audit pass.

Payment pass nếu thuộc scope.

Shipping pass nếu thuộc scope.

Order pass.

CRM pass.

Member lifecycle pass.

Product activation pass.

Official contact pass.

Health / priority conflict pass.

Release approved.

Go-live approved.

Tại thời điểm MASTER-02 hoàn tất governance:


## 8. RELEASE / GO-LIVE BOUNDARY

MASTER-02 hoàn tất không đồng nghĩa release.

Release chỉ được duyệt khi:

Pack implementation hoàn tất.

Evidence đầy đủ.

Security pass.

UAT/smoke pass.

Release governance approve.

Go-live chỉ được duyệt khi:

Production checklist pass.

Rollback plan có.

Monitoring có.

Incident handling có.

Owner go-live sign-off.

Nếu thiếu một trong các điều kiện trên:


## 9. PACK HANDOFF

MASTER-02 handoff cho các pack chi tiết như sau.


### 9.1. FILE 01 — Parent Logic / Global Decision Engine

FILE 01 chịu trách nhiệm triển khai logic cha cho:

Customer Context First.

Care Before Sale.

Segment Recommendation.

CRM lifecycle decision.

Member lifecycle decision.

Diamond/affiliate/distributor routing.

Program context.

Public/private boundary.

Priority conflict.


### 9.2. FILE 04 — Resolver / Guard / Runtime Contract

FILE 04 chịu trách nhiệm định nghĩa:

Input/output resolver.

Guard contract.

Runtime status.

Error/fallback contract.

Owner core boundary.

Các resolver P0 trong MASTER-02 phải có contract ở FILE 04.


### 9.3. FILE 07 — Common Content Block Registry

FILE 07 chịu trách nhiệm render:

Public-safe wording.

Messenger handoff wording.

Customer care wording.

CRM wording.

Member lifecycle wording.

Diamond/affiliate wording.

Health-safe wording.

Quote/Order wording.

Payment/Shipping wording.

FILE 07 không tự quyết định nghiệp vụ.

FILE 07 chỉ render sau khi resolver/guard pass.


### 9.4. FILE 09 — Test Matrix / Done Gate

FILE 09 chịu trách nhiệm test:

16 domain smoke.

P0 negative path.

Resolver missing.

Guard fail.

Quote/order/payment/shipping.

CRM suppression.

Gateway handoff.

Evidence mapping.


### 9.5. FILE 10 — Situation Registry

FILE 10 chịu trách nhiệm mapping real-life situations:

Khách mới.

Khách cũ.

Member.

Diamond.

Affiliate.

Đại lý/nhà phân phối.

Văn phòng/sinh viên/du học sinh/Việt kiều.

Công ty/trường học/bệnh viện.

Live abuse.

Health-sensitive.

Complaint.

Program / Giờ Vàng / 24/7.


### 9.6. FILE 12 — Human-Level Sales Behavior

FILE 12 chịu trách nhiệm hành vi bán hàng:

Hỏi thăm khách cũ.

Không bán thô.

Gợi ý 3 trụ + người thân.

Entry order không phải thất bại.

Growth order có lý do.

Member care proactive.

Hạ hạng an toàn.

Diamond trả lời trực tiếp.

Affiliate/distributor phân tách.

Segment selling.

Không dùng outcome để ép khách.

FILE 12 không được override Product Knowledge, Claim, Pricing, Availability, QuoteSnapshot, Order, Payment, Shipping hoặc Security.


### 9.7. Commerce / Payment / Shipping / Order / Gateway / Evidence Packs

Các pack kỹ thuật chịu trách nhiệm:

Commerce Pricing Core — giá, chương trình, quote pricingCommerce Availability Core — sellable, active product, stop saleOperational Core — sản xuất, nhập kho, hold, recallPayment Core / COM-06 — payment, bank transfer, accounting queueShipping Core / COM-07 — shipping, tracking, ETA, CODOrder Core — quote, draft, confirmation, order stateIVR Pack — order confirmation onlyGateway Pack — Meta event, handoff, dedup, moderationCRM / Messaging Pack — lifecycle, trigger, send, suppression, dedupEvidence Pack — evidence item, completion gateSecurity Pack — permission, audit, protected operation

MASTER-02 hoàn tất ở tầng governance khi:


## PHẦN 1/4 completed.


## PHẦN 2/4 completed.


## PHẦN 3/4 completed.


## PHẦN 4/4 completed.

16 business domain đã khóa.

Done Gate đã khóa.

Smoke mapping đã khóa.

Evidence mapping đã khóa.

Release boundary đã khóa.

Pack handoff đã khóa.

Trạng thái MASTER-02:

Trạng thái toàn hệ sau MASTER-02:

Lý do:

MASTER-02 là governance document.

MASTER-02 không thay thế implementation, smoke, evidence, completion report, release approval hoặc go-live approval.


## 11. MASTER-02 FINAL CONCLUSION

MASTER-02 khóa bản đồ phụ thuộc liên pack của hệ thống Ginsengfood theo mô hình:

Business Rule-> Source-of-Truth-> Owner Core-> Runtime Resolver / Guard-> Consumer Pack-> Allowed Action-> bị chặn-if-missing-> Evidence-> Done Gate

MASTER-02 xác định rằng hệ thống Ginsengfood không được vận hành bằng các đoạn logic rời rạc, template hardcode hoặc AI tự suy luận.

Mọi hành vi quan trọng phải đi qua source, resolver, guard và evidence.

Các nguyên tắc cuối cùng:

Source-of-Truth quyết định dữ liệu.

Runtime Resolver quyết định tại thời điểm thực thi.

Consumer chỉ tiêu thụ trong boundary.

Customer Context First là bắt buộc.

Customer Memory là bắt buộc cho khách cũ.

CRM 12 tháng là dependency chính thức.

Member Lifecycle là dependency chính thức.

Diamond / Affiliate / Distributor phải tách rõ.

Product Recommendation phải theo Product Knowledge, segment, 3 trụ và Product Effectiveness.

Message Trigger và Immutable Text phải được kiểm soát.

Public Comment chỉ là cổng handoff, không phải nơi chốt.

Health Boundary phải an toàn và không thuốc hóa.

Brand Soul và Scientific Evidence phải đúng source.

Official Contact phải qua registry.

Payment phải qua Payment Core / Accounting Review.

Shipping phải qua Shipping Core.

Order phải qua QuoteSnapshot / OrderDraft / CustomerConfirmation / OrderCode.

IVR chỉ xác nhận đơn, không đổi order state.

Priority Conflict thắng sales.

Dedup và Outcome Logger là bắt buộc.

Live Moderation bảo vệ live và thương hiệu.

Evidence là điều kiện của release.

Không hardcode dữ liệu runtime.

Không fallback bằng suy đoán.

Không future extension nếu chưa owner-approved.

Không pack nào được tự suy luận ngoài MASTER-02.


## 12. MASTER-02 FINAL LOCK

Kể từ bản này, MASTER-02 được khóa làm tài liệu governance chính thức cho Cross-Pack Dependency Map.

Tên bản:

Phạm vi khóa:

Dependency PrinciplesBusiness-rule-driven Dependency ModelSource BoundaryCross-pack Dependency Registry by Business DomainOwner Core / Consumer PackBlocked-if-missing RuleRuntime ControlConflict ResolutionSuppressionFallbackNo-hardcode EnforcementSmoke MappingEvidence MappingRelease ControlPack HandoffFinal Conclusion

Trạng thái khóa:


## 13. CẬP NHẬT HANDOFF

Đã hoàn thành toàn bộ:

Gồm đủ 4 phần:


## PHẦN 1/4 — DEPENDENCY PRINCIPLES / BUSINESS-RULE-DRIVEN DEPENDENCY MODEL / SOURCE BOUNDARYPHẦN 2/4 — CROSS-PACK DEPENDENCY REGISTRY BY BUSINESS DOMAIN / OWNER CORE / CONSUMER PACK / bị chặn-IF-MISSING RULEPHẦN 3/4 — RUNTIME CONTROL / CONFLICT RESOLUTION / SUPPRESSION / FALLBACK / NO-HARDCODE ENFORCEMENTPHẦN 4/4 — FINAL DONE GATE / SMOKE MAPPING / RELEASE CONTROL / MASTER-02 FINAL CONCLUSION

Trạng thái cuối:
