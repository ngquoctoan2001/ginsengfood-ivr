# MASTER-00 - INDEX REGISTRY

## Mục Đích

MASTER-00 là chỉ mục điều phối cao nhất cho bộ tài liệu Ginsengfood. Tài liệu này khóa phạm vi toàn hệ, thứ tự PACK hiện hành, ranh giới trách nhiệm, nguyên tắc source-of-truth, dependency, handoff, evidence và release control ở cấp governance.

MASTER-00 không thiết kế chi tiết database, API, DTO, service, UI, worker, prompt AI, mapping thiết bị, dữ liệu production thật hoặc cấu hình vận hành thật. Các chi tiết đó thuộc PACK, PHASE hoặc tài liệu implementation tương ứng.

## Vai Trò Của MASTER-00

| Vai trò | Nội dung khóa | Không làm |
| --- | --- | --- |
| Registry | Danh mục MASTER, PACK, PHASE và thứ tự đọc | Không thay tài liệu chi tiết |
| Boundary | Owner, consumer, runtime boundary | Không tự định nghĩa logic runtime của từng pack |
| Dependency | Chiều phụ thuộc giữa Product, Operational và downstream | Không cho consumer tự quyết dữ liệu owner core |
| Handoff | Điều kiện để chuyển từ tài liệu sang implementation | Không tự gọi implementation complete |
| Release control | Chuẩn đọc gate, evidence, smoke, owner decision | Không tự cấp release hoặc go-live |

## Nguyên Tắc Đọc Bộ Tài Liệu

1. MASTER quyết định khung quản trị.
2. PACK quyết định rule nghiệp vụ hoặc runtime theo từng vùng.
3. PHASE chuyển PACK thành handoff triển khai theo thứ tự.
4. Evidence và smoke chỉ chứng minh kết quả khi có artifact cụ thể.
5. Owner decision là nguồn duy nhất để mở rộng rule khi tài liệu chưa đủ dữ liệu.
6. Downstream không được tự tạo source, tự hardcode hoặc tự suy luận rule nghiệp vụ.

## Pack Registry Canonical

| Pack | Tên canonical | Vai trò chính | Owner truth |
| --- | --- | --- | --- |
| PACK-01 | Operational Core | Source, raw lot, QC, production, warehouse, inventory, traceability, recall, sale lock | Operational truth |
| PACK-02 | Product Master / SKU / Recipe / Activation | Product, SKU, ingredient, recipe, formula version, activation | Product truth |
| PACK-03 | Demand / MRP / Procurement / Material Control | Demand, MRP, formula scaling, procurement, material and packaging control | Planning truth |
| PACK-04 | MISA / Accounting Handoff | Accounting checkpoint, sync, reconcile, cost handoff, MISA boundary | Accounting handoff truth |
| PACK-05 | AI Advisor Channel | Product consulting, customer memory, quote-safe handoff, safety wording | AI consumer behavior |
| PACK-06 | Facebook Channel Gateway | Meta Live, Messenger, comment routing, private handoff, channel policy | Channel gateway truth |
| PACK-07 | Ads / ROAS / Attribution | Ads cost, attribution, verified revenue, scale gate, data quality | Ads measurement truth |
| PACK-08 | MC AI Live | Daily Live Product Board, script brief, host assistant, live orchestration | Live orchestration truth |
| PACK-09 | IVR Order Confirmation | Internal SIM Gateway, call job, call result, order confirmation signal | IVR confirmation truth |
| PACK-10 | Completion / Evidence / Gateway | Evidence registry, smoke, UAT, completion control, release readiness governance | Evidence truth |

## Master Registry

| File | Vai trò | Đầu vào | Đầu ra |
| --- | --- | --- | --- |
| MASTER-00 | Chỉ mục, registry, sequence, handoff | Toàn bộ rule nền | Pack registry canonical |
| MASTER-01 | Source-of-Truth registry | Pack registry | Source owner matrix |
| MASTER-02 | Cross-pack dependency map | Source owner matrix | Dependency graph |
| MASTER-03 | Traceability ID standard | Dependency graph | ID và trace standard |
| MASTER-04 | Runtime resolution and guard control | Source + trace | Resolver/guard standard |
| MASTER-05 | Evidence, smoke, completion gate standard | Resolver/guard | Evidence/smoke standard |
| MASTER-06 | Reserved and future integration governance | Release and source rules | Activation gate cho scope tương lai |
| MASTER-07 | Release and go-live control | Evidence/smoke standard | Release review model |
| MASTER-08 | Cross-system decision log | Source baseline mới + conflict register | Owner decision IDs + fail-closed runtime stance |
| MASTER-09 | Cross-phase runtime lock addendum | MASTER-08 + source baseline mới | Runtime chain + addendum sequence + implementation block map |

## Chuỗi Dependency Cốt Lõi

```text
PACK-02 Product Master
  -> PACK-01 Operational Core
  -> PACK-03 Demand / MRP
  -> PACK-04 MISA Handoff
  -> Commerce Runtime, AI, Gateway, Ads, Live, CRM, IVR
  -> PACK-10 Evidence / Completion
```

Product truth đi trước Operational truth. Operational truth đi trước Commerce, AI, Ads, Live, Gateway, CRM và IVR. Consumer không được tự quyết dữ liệu thuộc owner core.

## Domain Coverage

| Domain | Owner | Consumer | Rule nền |
| --- | --- | --- | --- |
| Product name, SKU, recipe | PACK-02 | PACK-01, AI, Ads, Live | Active không đồng nghĩa sellable |
| Formula version, BOM | PACK-02 | PACK-01, PACK-03 | Production phải dùng snapshot |
| Raw material lot | PACK-01 | Production, MISA, trace | QC_PASS chưa phải READY_FOR_PRODUCTION |
| Production order | PACK-01 | Workshop, batch, material issue | Không chọn tay nguyên liệu ngoài snapshot |
| Batch release | PACK-01 | Warehouse, inventory, trace | Batch QC_PASS chưa phải RELEASED |
| Warehouse receipt | PACK-01 | Inventory, MISA, Commerce Runtime | Tăng tồn sau receipt confirmed |
| Inventory ledger | PACK-01 | Sellable, AI, Ads, CRM | Append-only |
| Traceability / QR | PACK-01 | Public trace, Gateway, IVR | Public whitelist-only |
| Recall / Sale Lock | PACK-01 | Mọi downstream | Thắng mọi kênh bán |
| MISA handoff | PACK-04 | Accounting | Chỉ nhận checkpoint hợp lệ |
| AI response | PACK-05 | Gateway, CRM, Live | AI là consumer, không phải owner |
| Channel routing | PACK-06 | Messenger, Live | Không lộ dữ liệu private |
| Ads scale | PACK-07 | Marketing, owner | Dựa trên verified revenue |
| Live script | PACK-08 | Live operation | Chỉ dùng board/script hợp lệ |
| IVR result | PACK-09 | Order confirmation | Chỉ là signal xác nhận đơn |
| Evidence | PACK-10 | Release review | Không thay owner decision |

## Rule Nghiệp Vụ Không Được Vi Phạm

| Rule | Ý nghĩa | Consumer bị ảnh hưởng |
| --- | --- | --- |
| Product Active không đồng nghĩa Sellable | Product đủ điều kiện nội bộ, chưa tự được bán | AI, Ads, Live, Gateway |
| SKU Active không đồng nghĩa Sellable | SKU active chưa chứng minh tồn kho, release, trace, lock | Commerce Runtime, CRM |
| Recipe Active không đồng nghĩa Sellable | Recipe active chưa có released batch | Production, AI |
| Raw Lot QC_PASS không đồng nghĩa READY_FOR_PRODUCTION | QC đạt chưa tự cho phép issue nguyên liệu | Production, MRP |
| Batch QC_PASS không đồng nghĩa RELEASED | QC batch đạt chưa tự mở nhập kho | Warehouse, inventory |
| Warehouse chỉ nhận Batch RELEASED | Thành phẩm chỉ tăng tồn sau release và receipt confirmed | Commerce Runtime |
| Inventory Ledger append-only | Không sửa lịch sử ledger | Audit, MISA, evidence |
| Recall / Sale Lock thắng downstream | Mọi kênh bán phải bị chặn theo lock | AI, Ads, Live, CRM, Gateway, IVR |

## Ranh Giới Với Commerce Và Runtime Ngoài Cụm B

Commerce Runtime, CRM, Admin UI, Security Runtime và Landing Page có thể được nhắc như downstream dependency, nhưng không được đổi mã PACK hiện hành. Khi MASTER cần nói về Commerce, dùng tên domain "Commerce Runtime", không gán sang PACK-04 vì PACK-04 hiện là MISA / Accounting Handoff.

## File Sequence Chuẩn

| Cụm | Thứ tự | Ghi chú |
| --- | --- | --- |
| MASTER | MASTER-00 đến MASTER-09 | Đọc trước PACK; MASTER-08/09 điều phối source bổ sung và conflict P0 |
| PACK | PACK-01 đến PACK-10 | Theo registry canonical |
| PHASE-01 | P1 analysis, design, limited implementation, smoke | Product truth |
| PHASE-02 | P2 analysis, design, operational handoff, smoke | Operational truth |
| APPENDICES | Forms, MISA, print, material taxonomy | Tài liệu hỗ trợ rule |

## Handoff Từ MASTER Sang PACK

Mỗi PACK phải nhận từ MASTER tối thiểu:

1. Pack ID canonical.
2. Owner boundary.
3. Source-of-Truth liên quan.
4. Dependency đầu vào và đầu ra.
5. Resolver hoặc guard bắt buộc.
6. Evidence và smoke tối thiểu.
7. Done Gate và Fail Gate cấp tài liệu.

## Missing Input Gate

Nếu thiếu dữ liệu để viết sạch:

1. Dừng tại file đang viết.
2. Ghi rõ mục thiếu.
3. Ghi câu hỏi cho owner.
4. Không tự điền placeholder.
5. Không đưa câu hỏi vào nội dung chính của tài liệu sạch.

## Done Gate Của MASTER-00

MASTER-00 đạt yêu cầu khi:

1. Registry PACK khớp thứ tự hiện hành.
2. Không còn mã PACK gây conflict.
3. Chuỗi Product -> Operational -> downstream rõ ràng.
4. Rule active, QC, release, warehouse, inventory, recall và sale lock được khóa rõ.
5. Các MASTER còn lại có vai trò riêng, không lặp chức năng.
6. PACK và PHASE có thể dùng MASTER làm nền để viết tiếp.

## Chi Tiết Governance Tương Ứng

Phần này giữ lại phạm vi chi tiết từ nguồn MASTER theo cụm cùng chủ đề, đồng thời chuẩn hóa theo registry PACK hiện hành và loại bỏ các câu trạng thái tổng không dùng trong bộ sạch.


## Master-00 — Ginsengfood Master Technical Documentation Index


## Source Of Truth / Component Boundary / Cross-Pack Traceability / Dev Handoff Control


## PHẦN 1/4 — MASTER SCOPE / PACK REGISTRY / GLOBAL SOURCE-OF-TRUTH / OWNERSHIP BOUNDARY


## 1. MỤC ĐÍCH CỦA MASTER-00

MASTER-00 là tài liệu chỉ mục kỹ thuật cấp cao nhất của hệ thống Ginsengfood.

Tài liệu này dùng để xác định:

Phạm vi toàn hệ thống.

Danh mục các pack kỹ thuật chính thức.

Ranh giới trách nhiệm giữa các hợp phần.

Nguồn dữ liệu được phép xem là Source-of-Truth.

Trạng thái release toàn hệ.

Nguyên tắc kiểm soát dependency giữa các pack.

Nguyên tắc handoff từ tài liệu sang triển khai.

Nguyên tắc traceability, evidence, smoke test và owner sign-off.

MASTER-00 không phải là tài liệu triển khai chi tiết của từng module.

MASTER-00 không mô tả sâu logic nghiệp vụ cấp màn hình, cấp API, cấp database table, cấp DTO, cấp worker hoặc cấp rule cụ thể theo từng chức năng.

Các nội dung chi tiết phải được viết trong pack chuyên trách tương ứng.

Nguyên tắc của MASTER-00:

MASTER-00 = Governance + Boundary + Source-of-Truth + Dependency + Gate + Evidence + Release ControlPack chi tiết = Implementation Rule + Runtime Contract + API/DTO + Database + UI + Test + Smoke


## 2. PHẠM VI TOÀN HỆ THỐNG GINSENGFOOD

Hệ thống Ginsengfood là hệ thống kỹ thuật đa hợp phần, phục vụ toàn bộ chuỗi vận hành sản phẩm Cháo Sâm Savigin, bao gồm:

Quản trị dữ liệu sản phẩm.

AI tư vấn bán hàng.

Channel Gateway cho Facebook / Meta Live / Messenger.

Commerce Core cho báo giá, giỏ hàng, đơn hàng, thanh toán, vận chuyển.

CRM và Member Lifecycle.

Sản xuất, nhập kho, truy xuất, thu hồi.

GS1 / barcode / QR / public trace.

Kết nối kế toán / MISA.

Landing Page và các bề mặt bán hàng.

IVR xác nhận đơn hàng.

Dashboard, monitoring, evidence, smoke test, release gate.

Hệ thống không được hiểu là một phần mềm đơn lẻ.

Hệ thống phải được hiểu là một kiến trúc đa hợp phần có nhiều pack tài liệu, nhiều service, nhiều runtime boundary và nhiều điểm kiểm soát trước khi được phép vận hành thật.


## 3. TRẠNG THÁI TOÀN HỆ BẮT BUỘC

Ý nghĩa:

Không pack con nào được tự tuyên bố production ready nếu trạng thái MASTER vẫn chưa đạt điều kiện release.


## 4. NGUYÊN TẮC SOURCE-OF-TRUTH TOÀN CỤC

Hệ thống Ginsengfood phải vận hành theo nguyên tắc Source-of-Truth rõ ràng.

Không hợp phần nào được tự suy diễn dữ liệu nghiệp vụ, giá, quyền lợi thành viên, tồn kho, thanh toán, vận chuyển, trạng thái đơn hàng hoặc trạng thái sản xuất nếu chưa nhận dữ liệu từ core tương ứng.

Nguyên tắc chung:

Không có Source-of-Truth hợp lệ -> không được ra quyết định runtime.Không có resolver hợp lệ -> không được trả lời như dữ liệu đã chắc chắn.Không có evidence hợp lệ -> không được tính là Done.Không có smoke pass -> không được release.Không có owner sign-off -> không được go-live.


## 5. SOURCE-OF-TRUTH THEO NHÓM DỮ LIỆU


### 5.1. Sản phẩm / SKU / công thức / kiến thức sản phẩm

Source-of-Truth thuộc về Product Knowledge Pack và Operational Product Master.

Bao gồm:

Tên sản phẩm công khai.

Nhóm sản phẩm.

SKU nội bộ.

Thành phần chính.

Công thức vận hành.

Trạng thái bán.

Trạng thái tồn kho có thể bán.

Nội dung tư vấn công khai.

Claim whitelist.

Claim không được nói.

Mapping giữa sản phẩm bán hàng và sản phẩm vận hành.

AI Advisor, Channel Gateway, Landing Page, CRM và bất kỳ giao diện nào có nội dung sản phẩm đều không được tự viết hoặc hardcode dữ liệu sản phẩm ngoài Product Knowledge Source-of-Truth.


### 5.2. Giá / chương trình / quyền lợi thành viên

Source-of-Truth thuộc về Commerce Pricing Core và Member Benefit Core.

Giá niêm yết.

Chương trình Giờ Vàng.

Chương trình 24/7.

Mức giảm theo chương trình.

Quyền lợi thành viên.

Điều kiện áp dụng.

Thời gian hiệu lực.

Quote freeze window.

Giá cuối cùng được phép báo cho khách.

AI Advisor, Gateway, CRM, Landing Page và Admin UI không được tự tính hoặc tự hứa giá nếu Pricing Core chưa trả dữ liệu hợp lệ.


### 5.3. Thanh toán

Source-of-Truth thuộc về Commerce Payment Core.

Chiến lược thanh toán đã chốt:

PAYMENT_STRATEGY = VNPAY + MOMO_BUSINESS + DIRECT_BANK_TRANSFER_TO_SSAVIGROUP

Trong đó:

VNPAY là cổng thanh toán trung gian chính.

MoMo Business là ví điện tử song song để tăng tỷ lệ chuyển đổi.

Chuyển khoản trực tiếp SSAVIGROUP là phương án chủ động/dự phòng.

Direct bank transfer phải đi qua Company Bank Account Registry.

Bank transfer order phải có payment_reference hoặc transfer memo để kế toán đối soát khi chưa có bank API hoặc bank API không khả dụng.

Payment không được chuyển trạng thái PAID nếu chưa có Payment Core confirmation hoặc Accounting Review confirmation.

Không hợp phần nào được hardcode thông tin tài khoản ngân hàng SSAVIGROUP ngoài payment configuration / Company Bank Account Registry đã được phê duyệt.

Áp dụng cho:

AI Advisor.

Channel Gateway.

CRM.

Admin UI.

Frontend component.

Static template.

Landing Page.

IVR.

Worker.

Script nội bộ.


### 5.4. Vận chuyển

Source-of-Truth thuộc về Commerce Shipping Core.

Phạm vi hiện tại là vận chuyển trong nước.

Không ghi nhận vận chuyển quốc tế như phạm vi đang vận hành trong MASTER-00.

Nếu sau này có nhu cầu vận chuyển quốc tế, nội dung này chỉ được xem là phần mở rộng có kiểm soát và phải có owner approval riêng.

Cấu hình ghi nhận:

FUTURE_GOVERNED_EXTENSION = INTERNATIONAL_PAYMENT_OR_SHIPPING_IF_OWNER_APPROVED_LATER

AI Advisor, Gateway hoặc CRM không được tự hứa thời gian giao hàng, phí giao hàng, COD hoặc khả năng giao hàng nếu Commerce Shipping Core chưa trả eligibility hợp lệ.


### 5.5. Đơn hàng

Source-of-Truth thuộc về Commerce Order Core.

Order draft.

Quote cart.

Order confirmation draft.

Order state machine.

Order code.

Payment state.

Shipping state.

COD state.

Cancellation state.

IVR confirmation result.

Accounting review result.

Fulfillment state.

Gateway, AI Advisor, CRM, Landing Page, IVR và Admin UI không được tự cập nhật order state ngoài Order Core.


### 5.6. Khách hàng / thành viên / CRM

Source-of-Truth thuộc về Customer Profile Core, Member Core và CRM Lifecycle Core.

Khách mới hay khách cũ.

Hạng thành viên.

Lịch sử mua hàng.

Lịch sử tư vấn.

Điều kiện duy trì hạng.

Điều kiện nâng hạng.

Chu kỳ chăm sóc.

Suppression rule.

Frequency cap.

Quiet hours.

Customer Memory.

Recommendation history.

AI Advisor chỉ được cá nhân hóa nếu có dữ liệu runtime hợp lệ.


### 5.7. Tồn kho / mở bán / giữ hàng

Source-of-Truth thuộc về Inventory Core và Commerce Availability Core.

Tồn kho có thể bán.

Tồn kho giữ hàng.

Quality hold.

Sale lock.

Batch release.

Warehouse receipt.

Lot balance.

Allocation.

Trạng thái SKU có được bán hay không.

AI Advisor, Gateway, CRM và Landing Page không được bán hoặc hứa còn hàng nếu Availability Core chưa trả kết quả hợp lệ.


### 5.8. Sản xuất / truy xuất / thu hồi

Source-of-Truth thuộc về Operational Core.

Nguồn nguyên liệu.

Raw material lot.

Incoming QC.

READY_FOR_PRODUCTION.

Production order.

Formula snapshot.

Material issue.

Batch genealogy.

QC inspection.

Inventory ledger.

Traceability.

Recall / recovery.

CAPA.

MISA sync boundary.

MASTER-00 chỉ ghi nhận boundary và dependency.

Quy tắc triển khai chi tiết thuộc về Operational Pack.


### 5.9. IVR xác nhận đơn hàng

Source-of-Truth thuộc về IVR Order Confirmation Pack và Commerce Order Core.

IVR không phải hệ thống bán hàng.

IVR không phải hệ thống CRM.

IVR không phải hệ thống tư vấn.

IVR không được tự xử lý đơn hàng ngoài Order State Machine.

Cấu hình pack chính thức:

Rule IVR đã chốt:

IVR chỉ xác nhận đơn hàng.

Internal SIM Gateway Server là mô hình mặc định.

1 SIM = 1 active outbound call.

Phím 1 = xác nhận đơn.

Phím 0 = khách hủy / không đặt đơn.

Giờ Vàng = 10 phút, 2 cuộc, cách 5 phút.

24/7 = 15 phút, 3 cuộc, lịch T0/T0+5/T0+10.

Rolling real-time queue required.

Batch after session calling prohibited.

SIM Gateway không được cập nhật order state trực tiếp.

Mọi hủy/tiếp tục xử lý đơn từ IVR phải qua Core Order State Machine.

Technical error is not customer no-answer.

IVR không dùng cho sales, CRM, marketing, tư vấn sản phẩm hoặc chăm sóc đại trà.


## 6. PACK REGISTRY TOÀN HỆ

Danh mục pack chính thức của hệ thống Ginsengfood được ghi nhận như sau.


## Pack-00 — Master Governance / Source-Of-Truth / Release Control

Mục đích

PACK-00 là lớp quản trị cao nhất của toàn hệ.

PACK-00 bao gồm:


## Master-00.

Pack registry.

Source-of-Truth registry.

Dependency graph.

Traceability standard.

Evidence package standard.

Smoke matrix standard.

Release governance.

Owner sign-off rule.

Global Done Gate.

Boundary

PACK-00 không thay thế pack chi tiết.

PACK-00 chỉ quyết định ranh giới, thứ tự, trạng thái và điều kiện qua cổng.

PACK-05 — AI ADVISOR CHANNEL

PACK-05 quản trị logic AI tư vấn sản phẩm Cháo Sâm Savigin.

AI Advisor Parent Logic.

Product Knowledge Resolver.

Sales conversation guard.

Product recommendation.

Public wording policy.

Claim denylist.

Quote handoff.

Order confirmation draft handoff.

Existing customer care before next sale.

AI Advisor không phải Pricing Core.

AI Advisor không phải Payment Core.

AI Advisor không phải Shipping Core.

AI Advisor không phải Inventory Core.

AI Advisor phải gọi core tương ứng trước khi báo giá, chốt đơn, hứa thanh toán, hứa giao hàng hoặc cá nhân hóa quyền lợi thành viên.


## Pack-02 — Product Knowledge / Sku Master / Claim Policy

PACK-02 quản trị toàn bộ dữ liệu sản phẩm, SKU, nhóm sản phẩm, claim công khai, claim nội bộ và rule chuyển đổi nội dung sang ngôn ngữ an toàn.

20 SKU baseline.

Tên công khai từng sản phẩm.

Nhóm A/B/C.

Thành phần.

Công dụng theo hướng thực dưỡng công khai.

Nền khoa học Sâm Savigin.

Public View / Internal View.

Product Knowledge Data Contract.

PACK-02 không tự quyết định giá, tồn kho, chương trình, quyền lợi thành viên hoặc giao hàng.


## Pack-06 — Facebook Channel Gateway

PACK-03 quản trị toàn bộ luồng tương tác qua Facebook / Meta Live / Messenger.

Comment public.

Messenger private.

Live session routing.

Page configuration.

Spam / profanity / abuse handling.

Handoff.

Price routing.

Public reply policy.

Meta app review evidence.

Gateway smoke.

UAT.

Completion report.

Gateway không được tự tính giá.

Gateway không được tự báo tồn kho.

Gateway không được tự hứa thanh toán.

Gateway không được tự hứa vận chuyển.

Gateway không được tự cập nhật order state.

Gateway phải gọi AI Advisor, Commerce Core, Payment Core, Shipping Core, Order Core và CRM Core theo đúng runtime contract.

Commerce Runtime — Quote / Order / Payment / Shipping

PACK-04 quản trị toàn bộ commerce runtime.

Quote.

Cart.

Pricing resolver.

Program resolver.

Member benefit resolver.

Payment eligibility.

Shipping eligibility.

COD.

VAT.

Order total.

Payment confirmation.

Accounting review.

Shipping tracking.

Commerce Core là nơi quyết định dữ liệu giao dịch.

AI, Gateway, CRM, Landing Page và Admin UI không được bypass Commerce Core.

CRM Runtime — Member Lifecycle

PACK-05 quản trị chăm sóc khách hàng, thành viên và vòng đời 12 tháng.

D0 / D1 / D3 / D7 / D14 / D21 / D30.

M2 đến M12.

Grace lifecycle.

Maintain tier.

Upgrade tier.

Downgrade safe handling.

Retry rule.

Suppression.

Order History.

Product Effectiveness.

Runtime member variables.

CRM không được tự bán, tự báo giá, tự hứa thanh toán hoặc tự hứa vận chuyển nếu Commerce Core chưa trả eligibility hợp lệ.

CRM không được gửi nội dung trái với Claim Policy.

PACK-01 — OPERATIONAL CORE

PACK-06 quản trị chuỗi vận hành sản xuất và hậu kiểm.

Source origin.

Raw material intake.

Raw lot readiness.

Material receipt.

Process chain.

Packaging.

Printing.

QR.

MISA integration boundary.

Operational Core không phải AI Advisor.

Operational Core không phải Channel Gateway.

Operational Core không phải Commerce Payment.

Operational Core cung cấp trạng thái sản phẩm, lô, tồn kho, release và trace cho các hợp phần liên quan theo contract.

Landing Page Runtime — consumer surface

PACK-07 quản trị các bề mặt web công khai dùng để giới thiệu, thu lead, điều hướng mua hàng và hiển thị sản phẩm.

Product public page.

Campaign page.

CTA.

Messenger handoff.

Quote entry.

Product display.

Public claim compliance.

Tracking.

Conversion event.

Landing Page không được hardcode giá cuối cùng nếu giá phụ thuộc chương trình runtime.

Landing Page không được hardcode tài khoản ngân hàng.

Landing Page không được tự hứa thanh toán hoặc vận chuyển ngoài Commerce Core.

Landing Page không được hiển thị claim ngoài Claim Policy.

PACK-08 — ADS / TRACKING / LEARNING ENGINE / CAMPAIGN SIGNAL

PACK-08 quản trị dữ liệu quảng cáo, tracking, signal, chiến dịch và học máy theo chiến dịch.

Campaign source.

Ad set.

Creative.

Tracking event.

Lead quality.

Messenger conversion.

Order conversion.

Revenue attribution.

Customer segment.

Campaign learning.

Budget evidence.

Performance dashboard.

ADS Pack không được quyết định giá, tồn kho, thanh toán, vận chuyển hoặc quyền lợi thành viên.

ADS Pack chỉ nhận signal hợp lệ từ Channel Gateway, Commerce Core, CRM Core và tracking layer.

PACK-09 — IVR ORDER CONFIRMATION

PACK-09 quản trị hệ thống gọi tự động xác nhận đơn hàng.

Cấu hình chính thức:

IVR chỉ được dùng để xác nhận đơn hàng.

IVR không được dùng cho:

Sales.

Marketing.

Tư vấn sản phẩm.

Chăm sóc đại trà.

Upsell.

Cross-sell.

Nhắc mua lại.

Gửi nội dung quảng bá.

IVR không được cập nhật trực tiếp order state.

IVR chỉ gửi kết quả xác nhận về Order Core theo contract.

Order Core mới là nơi quyết định trạng thái tiếp theo của đơn hàng.

Admin Runtime — internal operator surface

PACK-10 quản trị giao diện nội bộ cho vận hành, theo dõi, kiểm soát và xử lý nghiệp vụ.

Admin dashboard.

Operator workspace.

Order management.

Payment review.

Shipping review.

CRM review.

Product management.

Operational monitoring.

Evidence viewer.

Smoke status.

Gateway readiness.

Release checklist.

Admin UI không được chứa logic nghiệp vụ lõi theo kiểu hardcode.

Admin UI phải gọi API/service tương ứng.

Admin UI không được hardcode tài khoản ngân hàng SSAVIGROUP.

Admin UI không được tự chuyển payment thành PAID nếu chưa có Payment Core confirmation hoặc Accounting Review confirmation.


## Pack-10 — Completion / Evidence / Gateway

PACK-10 quản trị toàn bộ bằng chứng nghiệm thu.

Evidence package.

Screenshot.

API response evidence.

Database evidence.

Log evidence.

Gateway evidence.

Meta evidence.

Payment evidence.

Shipping evidence.

IVR evidence.

Smoke test evidence.

UAT evidence.

Final gate report.

Không có evidence thì không được tính Done.

Không có smoke pass thì không được release.

Completion Report chưa đủ evidence thì trạng thái phải giữ:

Security / Audit Control

Security / Audit Control quản trị bảo mật, phân quyền, audit và tuân thủ.

Role.

Permission.

Least privilege.

Audit log.

Sensitive data policy.

Public trace whitelist.

Evidence metadata.

Break-glass.

Approval.

State transition.

Idempotency.

Data retention.

Access boundary.

Credential policy.

Security Pack là yêu cầu xuyên suốt.

Không pack nào được bypass permission, audit, approval hoặc compliance rule.


## 7. COM FILE INDEX BẮT BUỘC

Trong nhóm Commerce Core, COM file index phải ghi nhận như sau:

COM-06 — PAYMENT / BANK TRANSFER / VIETQR / VNPAY / MOMO BUSINESS / COMPANY BANK ACCOUNT REGISTRY / ACCOUNTING QUEUECOM-07 — SHIPPING / TRACKING / COD / DOMESTIC SHIPPING / FALLBACK ETA

Không đưa thanh toán hoặc vận chuyển ngoài phạm vi trong nước thành phạm vi đang vận hành trong MASTER-00.

Nếu cần ghi nhận hướng mở rộng sau này, chỉ dùng:


## 8. GLOBAL RULE — PAYMENT / BANK TRANSFER / SHIPPING

Các rule sau áp dụng toàn hệ:

GLOBAL-RULE-PAYMENT-001 — Company Bank Account Registry

Direct bank transfer to SSAVIGROUP must be governed by Company Bank Account Registry.

Không hợp phần nào được tự lưu, tự nhập, tự hiển thị hoặc tự phát sinh tài khoản ngân hàng ngoài registry đã được phê duyệt.

GLOBAL-RULE-PAYMENT-002 — No Hardcoded Bank Account

No AI, Gateway, CRM, Admin UI, frontend component, or static template may hardcode company bank account information outside approved payment configuration / registry.

AI Advisor response.

Messenger template.

Live reply template.

CRM message.

Static HTML.

PDF template.

Print template.

Notification worker.

Internal script.

GLOBAL-RULE-PAYMENT-003 — Payment Reference Required

Bank transfer order must include payment_reference / transfer memo.

Mục tiêu:

Đối soát kế toán.

Giảm nhầm đơn.

Giảm nhầm khách.

Hỗ trợ khi chưa có bank API.

Hỗ trợ khi bank API không khả dụng.

Tạo bằng chứng cho Accounting Review Queue.

GLOBAL-RULE-PAYMENT-004 — No PAID Without Confirmation

No payment may be marked PAID without Payment Core confirmation or Accounting Review confirmation.

Điều này áp dụng cho mọi kênh:

VNPAY.

MoMo Business.

Direct bank transfer.

COD reconciliation nếu có.

Manual review.

GLOBAL-RULE-PAYMENT-005 — Governed Payment Channels

VNPAY and MoMo Business are governed payment channels under Commerce Payment Core.

AI, Gateway, CRM, Landing Page hoặc Admin UI không được tự hứa các phương thức này nếu Payment Core chưa trả eligibility.

GLOBAL-RULE-PAYMENT-006 — Payment Eligibility Required

AI / Gateway / CRM must not promise payment availability unless Commerce Payment Core confirms eligibility.

Nếu chưa có eligibility, hệ thống chỉ được nói theo hướng đang kiểm tra phương thức phù hợp trong luồng commerce, không được khẳng định khách chắc chắn thanh toán được bằng phương thức cụ thể.

GLOBAL-RULE-SHIPPING-001 — Shipping Eligibility Required

AI / Gateway / CRM must not promise shipping availability or ETA unless Commerce Shipping Core confirms eligibility.

Nếu chưa có eligibility, hệ thống không được tự hứa:

Có giao được hay không.

Phí ship.

Miễn phí ship.

COD có hỗ trợ hay không.

Thời gian giao dự kiến.

Đơn vị vận chuyển.

Mã tracking.


## 9. OWNERSHIP BOUNDARY TOÀN HỆ

MASTER-00 không phân chia theo người cụ thể.

MASTER-00 chỉ ghi nhận ranh giới sở hữu theo hợp phần.

Nguyên tắc:

Một nghiệp vụ lõi chỉ có một core sở hữu quyết định cuối.Các hợp phần khác chỉ được consume qua contract.Không có hợp phần phụ nào được bypass core.Không có UI nào được tự quyết định nghiệp vụ lõi.Không có AI nào được tự suy diễn dữ liệu runtime.


## 10. BẢNG OWNERSHIP BOUNDARY CẤP MASTER

Nhóm nghiệp vụ

Core sở hữu

Hợp phần được consume

Không được phép

Product Knowledge

Product Knowledge Core

AI, Gateway, CRM, Landing Page, Admin UI

Tự viết claim ngoài whitelist

Pricing

Commerce Pricing Core

Tự tính giá ngoài resolver

Member Benefit

Member Core

AI, CRM, Commerce

Tự hứa quyền lợi nếu chưa có dữ liệu runtime

Payment

Commerce Payment Core

AI, Gateway, CRM, Admin UI, Landing Page

Hardcode bank account / tự đánh dấu PAID

Shipping

Commerce Shipping Core

Tự hứa ETA / phí ship / COD

Order

Commerce Order Core

AI, Gateway, IVR, CRM, Admin UI

Tự cập nhật order state

CRM Lifecycle

CRM Core

AI, Gateway, Admin UI

Gửi vượt frequency cap / quiet hours

Inventory Availability

Inventory / Availability Core

AI, Commerce, Gateway, Landing Page

Bán khi chưa có eligibility

Production

Operational Core

Admin UI, Trace, Commerce Availability

Bypass production state

Traceability

Trace Core

Public Trace, Admin UI, Recall

Public leak dữ liệu nội bộ

Recall

Recall Core

Admin UI, Trace, CRM

Thu hồi không có snapshot

MISA

Integration Core

Accounting, Dashboard

Sync trực tiếp không qua integration layer

IVR

IVR Pack + Order Core

Commerce Order Core, Admin UI

IVR tự đổi trạng thái đơn

Evidence

Evidence Pack

Tất cả pack

Tự kết luận Done khi chưa có evidence

Security/Audit

Security Pack

Bypass permission/audit


## 11. NGUYÊN TẮC RANH GIỚI AI / GATEWAY / CRM

AI Advisor, Channel Gateway và CRM là các lớp tương tác với khách hàng, nhưng không phải core giao dịch.

Ba hợp phần này phải tuân thủ:

Không tự tính giá.Không tự hứa tồn kho.Không tự hứa thanh toán.Không tự hứa vận chuyển.Không tự cập nhật đơn hàng.Không tự xác nhận PAID.Không tự hứa quyền lợi thành viên nếu chưa có dữ liệu runtime.Không tự nói claim ngoài whitelist.Không tự gửi nội dung vượt rule CRM.

AI / Gateway / CRM chỉ được trả lời chắc chắn khi có dữ liệu từ core hợp lệ.


## 12. NGUYÊN TẮC RANH GIỚI ADMIN UI

Admin UI là giao diện vận hành nội bộ.

Admin UI không phải nơi chứa nghiệp vụ lõi.

Admin UI phải:

Gọi API/service tương ứng.

Hiển thị dữ liệu do core trả về.

Gửi command đúng contract.

Hiển thị trạng thái evidence.

Hiển thị lỗi rõ ràng.

Không hardcode tài khoản ngân hàng.

Không hardcode trạng thái thanh toán.

Không hardcode trạng thái vận chuyển.

Không tự đổi state nếu core không cho phép.


## 13. NGUYÊN TẮC RANH GIỚI LANDING PAGE

Landing Page là bề mặt công khai.

Landing Page có thể hiển thị:

Thông tin sản phẩm công khai.

Nội dung thương hiệu.

Form lead.

Nút điều hướng Messenger.

Nút bắt đầu mua hàng.

Nội dung chiến dịch nếu được phép.

Landing Page không được:

Hardcode giá cuối cùng nếu giá phụ thuộc runtime.

Hardcode tài khoản ngân hàng.

Tự hứa thanh toán.

Tự hứa vận chuyển.

Tự hứa tồn kho.

Tự hiển thị claim ngoài whitelist.

Tự tạo order chính thức khi chưa qua Commerce Core.


## 14. NGUYÊN TẮC RANH GIỚI IVR

IVR là lớp xác nhận đơn bằng cuộc gọi tự động.

IVR không được xem là kênh bán hàng.

IVR không được dùng để tư vấn.

IVR không được dùng để chăm sóc khách hàng đại trà.

IVR không được dùng để upsell.

IVR không được dùng để remarketing.

IVR chỉ nhận order confirmation task từ Order Core theo eligibility hợp lệ.

Kết quả IVR chỉ là input cho Order Core.

Order Core quyết định trạng thái tiếp theo.


## 15. NGUYÊN TẮC RANH GIỚI PAYMENT

Payment phải nằm trong Commerce Payment Core.

Các kênh thanh toán được ghi nhận:

VNPAYMOMO_BUSINESSDIRECT_BANK_TRANSFER_TO_SSAVIGROUP

Direct bank transfer bắt buộc đi qua:

Company Bank Account RegistryAccounting Review Queuepayment_reference / transfer memo

Không được đánh dấu PAID chỉ vì khách nói đã chuyển khoản.

Không được đánh dấu PAID chỉ vì có ảnh chụp giao dịch.

Không được đánh dấu PAID chỉ vì AI/Gateway/CRM nhận tin nhắn.

Chỉ được PAID khi có:

Payment Core confirmationhoặcAccounting Review confirmation


## 16. NGUYÊN TẮC RANH GIỚI SHIPPING

Shipping phải nằm trong Commerce Shipping Core.

Phạm vi vận chuyển hiện tại:

DOMESTIC SHIPPING

Không ghi vận chuyển quốc tế như phạm vi hiện hành.

Commerce Shipping Core phải quyết định:

Có hỗ trợ giao khu vực đó không.

Có COD hay không.

Fallback ETA.

Đối tác vận chuyển.

Trạng thái giao hàng.

AI / Gateway / CRM / Landing Page chỉ được hiển thị thông tin shipping sau khi core trả eligibility.


## 17. MASTER-00 KHÔNG LÀM GÌ

MASTER-00 không làm các việc sau:

Không viết code.

Không thiết kế chi tiết database table.

Không định nghĩa chi tiết từng API.

Không viết chi tiết từng DTO.

Không viết chi tiết từng component UI.

Không viết chi tiết từng màn hình.

Không viết toàn bộ kịch bản hội thoại.

Không viết toàn bộ nội dung CRM.

Không viết toàn bộ rule sản xuất.

Không viết toàn bộ rule MISA.

Không viết toàn bộ rule IVR cấp queue/worker.

Không thay thế test plan chi tiết.

Không thay thế completion report chi tiết.

Các phần đó thuộc về pack chuyên trách.


## 18. MASTER-00 BẮT BUỘC PHẢI KIỂM SOÁT GÌ

MASTER-00 bắt buộc kiểm soát:

Pack nào tồn tại.

Pack nào là hợp phần chính thức.

Pack nào phụ thuộc pack nào.

Pack nào sở hữu Source-of-Truth nào.

Hợp phần nào không được bypass core.

Trạng thái toàn hệ.

Điều kiện được tính Done.

Điều kiện được release.

Điều kiện được go-live.

Evidence bắt buộc.

Smoke bắt buộc.

Owner sign-off bắt buộc.

Global handoff sequence.

Traceability ID standard.


## 19. NGUYÊN TẮC PACK CHI TIẾT

Mỗi pack chi tiết phải có tối thiểu:

Mục tiêu.

Phạm vi.

Boundary.

Source-of-Truth.

Runtime contract.

Data contract.

API contract nếu có.

DTO nếu có.

UI rule nếu có.

Worker rule nếu có.

State machine nếu có.

Security rule.

Audit rule.

Evidence requirement.

Smoke test.

Done Gate.

Handoff note.

Release condition.

Pack chi tiết không được mâu thuẫn với MASTER-00.

Nếu pack chi tiết có nội dung vượt quá MASTER-00, nội dung đó phải được ghi nhận bằng owner decision trước khi đưa vào triển khai.


## 20. KẾT LUẬN PHẦN 1/4


## PHẦN 1/4 xác lập phạm vi cao nhất của MASTER-00.

Các điểm đã khóa trong phần này:

HẾT PHẦN 1/4


## PHẦN 2/4 — CROSS-PACK DEPENDENCY GRAPH / DOCUMENT WRITING SEQUENCE / DEV HANDOFF SEQUENCE / TRACEABILITY ID STANDARD


## 21. MỤC ĐÍCH CỦA PHẦN 2/4


## PHẦN 2/4 xác định cách các pack trong hệ thống Ginsengfood phụ thuộc lẫn nhau.

Mục tiêu của phần này là tránh tình trạng:

Viết tài liệu rời rạc.

Pack sau tự suy diễn pack trước.

AI / Gateway / CRM tự quyết định dữ liệu của Commerce Core.

Frontend tự hardcode thông tin thanh toán, vận chuyển, tài khoản ngân hàng.

IVR tự cập nhật trạng thái đơn.

Operational Core bị tách khỏi Commerce Availability.

Evidence không truy ngược được về requirement.

Dev nhận tài liệu nhưng không biết thứ tự triển khai.

Smoke test không biết phải kiểm tra chuỗi nào trước.

Nguyên tắc cốt lõi:

Một pack chỉ được triển khai khi dependency đầu vào đã rõ.Một runtime decision chỉ được thực hiện khi Source-of-Truth trả dữ liệu hợp lệ.Một Done Gate chỉ được công nhận khi có traceability và evidence.Một release chỉ được xét khi toàn bộ dependency trọng yếu đã smoke pass.


## 22. CROSS-PACK DEPENDENCY GRAPH CẤP MASTER

Hệ thống Ginsengfood phải được nhìn như một graph phụ thuộc đa hợp phần, không phải danh sách file độc lập.

Sơ đồ dependency cấp cao:

PACK-00 MASTER GOVERNANCE ↓PACK-02 PRODUCT KNOWLEDGE / SKU / CLAIM POLICY ↓Commerce Runtime / QUOTE / ORDER / PAYMENT / SHIPPING ↓PACK-05 AI Advisor Channel ↓PACK-06 Facebook Channel Gateway / META LIVE / MESSENGER ↓CRM Runtime / MEMBER LIFECYCLE ↓PACK-09 IVR ORDER CONFIRMATION ↓Admin Runtime UI / OPERATOR WORKSPACE ↓PACK-10 Evidence / SMOKE / UAT / COMPLETION REPORT ↓Security / Audit Control / PERMISSION / AUDIT / COMPLIANCE

PACK-01 Operational Core và PACK-07 Ads / ROAS / Attribution không đứng ngoài chuỗi này.

Hai pack này liên kết theo dependency song song:

PACK-01 Operational Core -> cung cấp release / inventory / trace / recall / MISA boundary -> phụ thuộc vào Security / Audit Control / Audit -> cung cấp dữ liệu cho Commerce Availability Runtime -> cung cấp dữ liệu cho Public Trace Runtime / Landing Page -> cung cấp evidence cho PACK-10 Evidence -> PACK-07 Ads / ROAS / Attribution / TRACKING / LEARNING ENGINE -> nhận signal từ PACK-06 Facebook Channel Gateway -> nhận conversion từ Commerce Runtime -> nhận customer lifecycle event từ CRM Runtime -> không được quyết định giá / tồn kho / payment / shipping


## 23. NGUYÊN TẮC ĐỌC DEPENDENCY GRAPH

Dependency graph phải được hiểu theo 4 chiều:


### 23.1. Chiều Source-of-Truth

Pack nào sở hữu dữ liệu gốc thì pack đó quyết định.

Ví dụ:

Giá thuộc Commerce Pricing Core.

Phương thức thanh toán thuộc Commerce Payment Core.

ETA / COD / phí ship thuộc Commerce Shipping Core.

Order state thuộc Commerce Order Core.

Nội dung claim thuộc Product Knowledge / Claim Policy.

Khách hàng và quyền lợi thành viên thuộc Customer / Member Core.

Release batch và tồn kho có thể bán thuộc Operational / Inventory Core.

Kết quả IVR chỉ là dữ liệu đầu vào cho Order Core, không phải quyết định cuối.


### 23.2. Chiều Runtime Contract

Pack không sở hữu dữ liệu phải gọi contract.

AI muốn báo giá phải gọi Pricing Resolver.

AI muốn báo quyền lợi thành viên phải gọi Member Benefit Resolver.

Gateway muốn mở luồng thanh toán phải gọi Payment Eligibility.

CRM muốn nhắc mua lại phải gọi Suppression / Frequency Cap / Customer Memory.

IVR muốn gọi xác nhận đơn phải nhận task hợp lệ từ Order Core.

Landing Page muốn hiển thị CTA mua hàng phải gọi Commerce Entry Contract.


### 23.3. Chiều Evidence

Mọi kết quả Done phải có bằng chứng.

API response.

Screenshot UI.

Log worker.

Record trạng thái.

Order state transition.

IVR call result.

Gateway webhook log.

CRM suppression log.

Public trace response key list.

Smoke test result.

Không có evidence thì không được tính là hoàn thành.


### 23.4. Chiều Release Gate

Không pack nào được đi tới release nếu dependency trọng yếu chưa pass.

Gateway không được mở nếu AI Advisor chưa có quote handoff an toàn.

AI Advisor không được quote nếu Pricing / Member / Availability chưa rõ.

Order không được xác nhận nếu payment / shipping rule chưa rõ.

IVR không được gọi nếu Order Core chưa cấp confirmation task.

Admin UI không được vận hành nếu permission / audit chưa có.

Landing Page không được chạy chiến dịch nếu public claim chưa qua whitelist.

CRM không được chạy automation nếu suppression / quiet hours / frequency cap chưa có.


## 24. DEPENDENCY MATRIX THEO PACK

Pack

Phụ thuộc đầu vào

Cung cấp đầu ra

Không được bypass


## Pack-00 Master

Owner decision, system scope

Pack registry, gate, boundary, traceability

Không thay pack chi tiết

PACK-05 AI Advisor

Product Knowledge, Pricing, Member, Inventory, Payment, Shipping, CRM Memory

Tư vấn, proposal, quote handoff, order draft handoff

Không tự tính giá, không tự hứa payment/shipping

PACK-02 Product Knowledge

Brand, SKU, claim policy, product data

Public View, Internal View, SKU knowledge, claim whitelist

Không quyết định giá/tồn kho

PACK-06 Facebook Channel Gateway

AI Advisor, Commerce Core, CRM, Meta config

Comment/Messenger routing, gateway evidence

Không tự quote, không tự tạo order

Commerce Runtime

Product, Pricing, Member, Payment, Shipping, Inventory

Quote, Cart, Order, Payment, Shipping, COD, VAT

Không để UI/AI bypass order/payment state

CRM Runtime

Customer, Order History, Member Core, Product Knowledge

Lifecycle message, care flow, repurchase signal

Không gửi vượt suppression/frequency cap

PACK-01 Operational Core

Source, recipe, production, QC, inventory, trace

Release, lot, trace, recall, MISA boundary

Không để commerce bán lô chưa release

Landing Page Runtime

Product Public View, Claim Policy, Commerce Entry

Public display, lead, CTA, tracking

Không hardcode price/bank/shipping

PACK-07 Ads / ROAS / Attribution

Campaign config, Gateway signal, Commerce conversion

Tracking, attribution, learning signal

Không quyết định commerce data

PACK-09 IVR

Order Core confirmation task, customer phone eligibility

IVR result event

Không tự đổi order state

Admin Runtime UI

API/service contracts, permission, audit

Operator workspace, review screen, dashboard

Không chứa logic nghiệp vụ hardcode

PACK-10 Evidence

Log, screenshot, API, DB, smoke

Completion report, UAT package, release evidence

Không tự pass nếu thiếu bằng chứng

Role model, audit policy, compliance

Permission, audit, access boundary

Không pack nào được bypass


## 25. DEPENDENCY TRỌNG YẾU GIỮA AI / GATEWAY / COMMERCE

Đây là dependency quan trọng nhất trong toàn bộ chuỗi bán hàng.

AI Advisor, Channel Gateway và CRM đều là lớp tương tác.

Commerce Core mới là lớp quyết định giao dịch.

Chuỗi đúng:

Customer Message -> Channel Gateway -> AI Advisor -> Product Knowledge Resolver -> Commerce Pricing Resolver -> Member Benefit Resolver -> Availability Resolver -> Payment Eligibility Resolver -> Shipping Eligibility Resolver -> Quote Cart -> Order Confirmation Draft -> Customer Confirmation -> Order Core -> Payment Core -> Shipping Core -> Evidence

Không được đi theo chuỗi sai:

Customer Message -> AI tự báo giá -> AI tự hứa chuyển khoản -> AI tự gửi số tài khoản -> AI tự hứa giao hàng -> AI tự xác nhận đã thanh toán -> AI tự tạo đơn chính thức

Chuỗi sai trên bị cấm ở cấp MASTER.


## 26. DEPENDENCY THANH TOÁN TRONG COMMERCE CORE

Thanh toán thuộc Commerce Payment Core.

Chiến lược thanh toán:

Dependency thanh toán:

Order Confirmation Draft -> Payment Eligibility -> Payment Method Selection -> Payment Instruction -> Payment Confirmation -> Accounting Review nếu cần -> Order Payment State

Đối với chuyển khoản trực tiếp:

Direct Bank Transfer -> Company Bank Account Registry -> payment_reference / transfer memo -> Customer transfer -> Payment evidence -> Accounting Review Queue -> Accounting Review confirmation -> Payment Core marks PAID

Không được có nhánh:

AI / Gateway / CRM / Admin UI / frontend / static template -> hardcode bank account -> customer transfers -> order marked PAID automatically

Nhánh này bị cấm.


## 27. DEPENDENCY VẬN CHUYỂN TRONG COMMERCE CORE

Vận chuyển hiện tại là vận chuyển trong nước.

Dependency vận chuyển:

Order Confirmation Draft -> Shipping Eligibility -> Domestic Shipping Rule -> COD Eligibility nếu có -> Shipping Fee -> Fallback ETA -> Customer Confirmation -> Shipping Task -> Tracking

AI / Gateway / CRM không được tự hứa:

Giao được hay không.

Bao lâu nhận hàng.

Có miễn phí ship hay không.

Phí ship là bao nhiêu.

Đơn vị vận chuyển nào.

Tracking khi chưa có dữ liệu.

Điều kiện bắt buộc:

Shipping answer requires Commerce Shipping Core eligibility.


## 28. DEPENDENCY IVR ORDER CONFIRMATION

PACK-09 là hợp phần chính thức, nhưng chỉ thuộc phạm vi xác nhận đơn hàng.

Order Confirmation Draft -> Customer submits confirmation -> Order Core creates confirmation-required task -> IVR Queue receives task -> Internal SIM Gateway Server places outbound call -> Customer presses 1 or 0 -> IVR records result event -> Order Core evaluates result -> Order State Machine decides next state

IVR call result -> SIM Gateway directly updates order state

Nhánh sai này bị cấm.

Quy tắc kiểm soát:

IVR chỉ nhận task từ Order Core.

IVR không tự tạo order.

IVR không tự hủy order.

IVR không tự xác nhận order.

IVR không tự đánh dấu khách không nghe máy nếu lỗi là lỗi kỹ thuật.


## 29. DEPENDENCY OPERATIONAL CORE VỚI COMMERCE

Commerce muốn bán được sản phẩm phải dựa vào Operational / Inventory / Availability.

Production Batch -> QC Inspection -> Batch Release -> Warehouse Receipt -> Inventory Ledger -> Sellable Availability -> Commerce Availability Resolver -> AI / Gateway / Landing Page may offer product

Không được bán theo chuỗi sai:

SKU exists in product catalog -> AI says available -> Customer orders

SKU có trong catalog không đồng nghĩa với SKU đang bán được.

Điều kiện bán phải dựa trên availability hợp lệ.


## 30. DEPENDENCY PRODUCT KNOWLEDGE VỚI CLAIM POLICY

Mọi nội dung AI, Landing Page, CRM, ADS và Gateway đều phải lấy dữ liệu từ Product Knowledge và Claim Policy.

Product Knowledge Master -> Public View -> Claim Whitelist -> Channel Template -> AI Response -> CRM Message -> Landing Page Content -> ADS Content

Writer / AI / Dev -> tự viết claim -> đưa lên public

Mọi claim công khai phải nằm trong whitelist.


## 31. DOCUMENT WRITING SEQUENCE TOÀN HỆ

Tài liệu phải được viết theo thứ tự để tránh pack sau thiếu đầu vào.

Thứ tự chuẩn:

BƯỚC 01 — MASTER-00BƯỚC 02 — PACK-02 Product Knowledge / SKU / Claim PolicyBƯỚC 03 — Commerce Runtime CoreBƯỚC 04 — PACK-05 AI Advisor CoreBƯỚC 05 — PACK-06 Facebook Channel GatewayBƯỚC 06 — CRM Runtime / Member LifecycleBƯỚC 07 — PACK-09 IVR Order ConfirmationBƯỚC 08 — PACK-01 Operational Core linkageBƯỚC 09 — Landing Page Runtime / Public WebBƯỚC 10 — PACK-07 Ads / ROAS / Attribution / TrackingBƯỚC 11 — Admin Runtime UIBƯỚC 12 — PACK-10 Evidence / Smoke / UAT / Completion ReportBƯỚC 13 — Security / Audit Control / Permission / Audit / Compliance cross-check

Lưu ý:

Security / Audit Control là pack xuyên suốt.

Security / Audit Control có thể được viết song song theo từng phần, nhưng không pack nào được bỏ qua security / permission / audit / compliance.


## 32. LÝ DO PACK-02 PHẢI ĐI TRƯỚC AI ADVISOR

AI Advisor không thể tư vấn nếu chưa có Product Knowledge.

PACK-02 phải đi trước PACK-05 vì AI cần:

Điểm nổi bật.

Claim được nói.

Cách nói an toàn.

Public View.

Internal View.

SKU mapping.

Recommendation basis.

Nếu thiếu PACK-02, AI sẽ có nguy cơ:

Tự đặt tên sản phẩm.

Nói sai claim.

Nói quá công dụng.

Lẫn SKU nội bộ với tên công khai.

Gợi ý sai sản phẩm.

Không nhất quán giữa live, Messenger, CRM và Landing Page.


## 33. LÝ DO PACK-04 PHẢI ĐI TRƯỚC CHANNEL GATEWAY VÀ CRM

Channel Gateway và CRM đều liên quan đến quote, order, payment, shipping.

PACK-04 phải đi trước PACK-03 và PACK-05 vì cần khóa:

Quote Cart.

Order Confirmation Draft.

Pricing Resolver.

Program Resolver.

Member Benefit Resolver.

Payment Eligibility.

Shipping Eligibility.

Final payable amount.

Payment reference.

Order State Machine.

Accounting Review Queue.

Domestic Shipping Rule.

Nếu thiếu PACK-04, Gateway / CRM rất dễ tự hứa:

Giá.

Chương trình.

Phương thức thanh toán.

Chuyển khoản.

Thời gian giao hàng.

Trạng thái đơn.

Các hành vi này bị cấm ở cấp MASTER.


## 34. LÝ DO PACK-09 IVR PHẢI ĐI SAU PACK-04

IVR không thể đứng trước Order Core.

PACK-09 phải đi sau PACK-04 vì IVR cần:

Order Core confirmation task.

Customer phone eligibility.

IVR call window.

Cancellation handling.

No-answer handling.

Technical-error handling.

Evidence event.

Admin monitoring.

Nếu không có PACK-04, IVR có nguy cơ trở thành một hệ thống gọi rời rạc, không gắn đúng trạng thái đơn hàng.

Điều này bị cấm.


## 35. LÝ DO PACK-10 Evidence PHẢI ĐI SAU CÁC PACK CHỨC NĂNG

Evidence Pack không thể viết trống rời khỏi chức năng thực tế.

PACK-10 phải dựa trên:

AI quote evidence.

CRM evidence.

Operational evidence.

Admin UI evidence.

Security evidence.

Smoke evidence.

Tuy nhiên, tiêu chuẩn evidence phải được biết từ đầu để dev ghi log, chụp bằng chứng và thiết kế test đúng.

Vì vậy:

Evidence standard biết từ MASTER-00.Evidence detail viết trong PACK-10.Evidence collection diễn ra trong suốt quá trình triển khai.Evidence approval nằm ở release gate.


## 36. DOCUMENT WRITING RULE CHO MỖI PACK

Mỗi pack chi tiết phải viết theo cấu trúc tối thiểu:


## 1. Purpose2. Scope3. Non-scope4. Source-of-Truth5. Dependency6. Boundary7. Runtime Contract8. Data Contract9. State / Flow10. Security / Permission / Audit11. Error Handling12. Evidence Requirement13. Smoke Test14. Done Gate15. Handoff Checklist

Nếu pack có UI, ghi thêm:

UI ScreenUI StateUI PermissionUI Empty StateUI Error StateUI Evidence

Nếu pack có worker, ghi thêm:

Worker TriggerQueue RuleRetry RuleIdempotency RuleDead-letter RuleMonitoring Rule

Nếu pack có payment, ghi thêm:

Payment EligibilityPayment InstructionPayment ConfirmationAccounting ReviewPayment ReferenceCompany Bank Account RegistryNo Hardcoded Bank Account Gate

Nếu pack có shipping, ghi thêm:

Domestic Shipping EligibilityCOD EligibilityShipping FeeFallback ETATrackingShipping Evidence

Nếu pack có IVR, ghi thêm:

Call EligibilitySIM Capacity RuleRolling QueueCall WindowDTMF ResultTechnical Error HandlingOrder Core State Machine BoundaryIVR Evidence


## 37. DEV HANDOFF SEQUENCE TOÀN HỆ

Handoff từ tài liệu sang triển khai phải đi theo chuỗi kiểm soát.

Không giao dev bằng mô tả chung chung.

Chuỗi handoff chuẩn:

STEP 01 — MASTER boundary reviewedSTEP 02 — Pack scope confirmedSTEP 03 — Source-of-Truth confirmedSTEP 04 — Dependency input confirmedSTEP 05 — Runtime contract confirmedSTEP 06 — API / DTO / DB requirement confirmedSTEP 07 — UI / worker requirement confirmedSTEP 08 — Security / permission / audit confirmedSTEP 09 — Evidence requirement confirmedSTEP 10 — Smoke test case confirmedSTEP 11 — Implementation startsSTEP 12 — Local test evidence collectedSTEP 13 — Integrated smoke evidence collectedSTEP 14 — Completion report preparedSTEP 15 — Owner sign-off requested

Không được đưa pack vào triển khai nếu chưa rõ:

Pack nhận dữ liệu từ đâu.

Pack trả dữ liệu cho ai.

Pack có quyền quyết định gì.

Pack không được quyết định gì.

Pack có trạng thái nào.

Pack có API nào.

Pack có queue/worker nào.

Pack có evidence nào.

Pack pass smoke bằng cách nào.


## 38. DEV HANDOFF THEO CẤP ĐỘ


### 38.1. Cấp MASTER

Handoff cấp MASTER trả lời:

Hệ thống gồm những pack nào?

Pack nào là core?

Pack nào là consumer?

Pack nào sở hữu Source-of-Truth?

Pack nào phụ thuộc pack nào?

Trạng thái release là gì?

Gate nào đang chặn go-live?

Evidence nào bắt buộc?


### 38.2. Cấp PACK

Handoff cấp PACK trả lời:

Pack này làm gì?

Không làm gì?

Dữ liệu gốc lấy từ đâu?

API/DTO nào cần có?

State machine nào cần có?

Permission nào cần có?

Audit gì cần ghi?

Smoke test nào cần pass?

Evidence nào cần nộp?


### 38.3. Cấp MODULE

Handoff cấp MODULE trả lời:

Module này thuộc pack nào?

Module nhận input gì?

Module tạo output gì?

Module có error case nào?

Module có idempotency không?

Module có retry không?

Module có audit không?

Module có permission không?

Module có UI không?

Module có worker không?


### 38.4. Cấp TASK

Handoff cấp TASK trả lời:

Làm file nào?

Thêm endpoint nào?

Thêm DTO nào?

Thêm table/migration nào nếu cần?

Thêm component nào?

Thêm test nào?

Chạy lệnh kiểm tra nào?

Evidence lưu ở đâu?

Done Gate của task là gì?


## 39. DEV HANDOFF KHÔNG ĐƯỢC LÀM GÌ

Không được handoff theo kiểu:

Dev tự hiểu thêm.Dev tự suy luận.Dev tự chọn trạng thái.Dev tự chọn cách tính giá.Dev tự chọn phương thức thanh toán.Dev tự hardcode số tài khoản.Dev tự hứa shipping.Dev tự đổi order state.Dev tự bỏ qua evidence.Dev tự bỏ qua smoke.

Handoff đúng phải giúp dev biết rõ:

Cái gì được làm.Cái gì không được làm.Dữ liệu lấy từ đâu.Kết quả trả về đâu.Điều kiện pass là gì.Bằng chứng cần nộp là gì.


## 40. TRACEABILITY ID STANDARD TOÀN HỆ

Traceability ID dùng để liên kết:

Requirement.

Business rule.

API.

DTO.

Database.

UI.

Test.

Evidence.

Smoke.

Release gate.

Không có traceability ID thì không thể kiểm soát Done Gate.


## 41. CẤU TRÚC TRACEABILITY ID

Chuẩn ID toàn hệ:

<PACK>-<TYPE>-<NUMBER>

PACK là mã pack.

TYPE là loại item.

NUMBER là số thứ tự 3 chữ số.

COM-REQ-001COM-RULE-001COM-API-001COM-DTO-001COM-DB-001COM-UI-001COM-WORKER-001COM-TEST-001COM-EVID-001COM-GATE-001


## 42. PACK PREFIX CHUẨN

Prefix

MAS

AIA

PKM

CHG

COM

CRM Runtime / Member Lifecycle

CRM

OPS

LDP

PACK-07 Ads / ROAS / Attribution / Tracking

ADS

PACK-09 IVR Order Confirmation

ADM

PACK-10 Evidence / Smoke / UAT

EVD

Security / Audit Control / Audit / Compliance

SEC


## 43. TYPE PREFIX CHUẨN

Type

Ý nghĩa

REQ

Requirement

RULE

Business / operational rule

SRC

Source-of-Truth

DEP

Dependency

API

API contract

DTO

DTO / payload

DB

Database requirement

UI

UI requirement

WRK

Worker / queue requirement

EVT

Event

STATE

State machine

ERR

Error handling

Security / permission

AUD

Audit

TEST

Test case

SMK

Smoke case

EVID


## Gate

Done Gate / Release Gate

DEC

Owner decision


## 44. VÍ DỤ TRACEABILITY ID THEO PACK


### 44.1. Commerce Payment

COM-REQ-001 — Commerce Core must support governed payment methods.COM-RULE-001 — No payment may be marked PAID without Payment Core or Accounting Review confirmation.COM-SRC-001 — Company Bank Account Registry is the Source-of-Truth for direct bank transfer account information.COM-API-001 — Payment Eligibility API.COM-DTO-001 — Payment Method Eligibility Response.COM-DB-001 — Payment reference field.COM-UI-001 — Payment Review Queue screen.COM-TEST-001 — Direct bank transfer requires payment_reference.COM-EVID-001 — Accounting Review confirmation evidence.COM-GATE-001 — Payment Done Gate.


### 44.2. Commerce Shipping

COM-REQ-020 — Commerce Shipping Core must return domestic shipping eligibility.COM-RULE-020 — AI/Gateway/CRM must not promise shipping availability or ETA without Shipping Core eligibility.COM-API-020 — Shipping Eligibility API.COM-DTO-020 — Shipping Eligibility Response.COM-TEST-020 — Missing address blocks ETA promise.COM-EVID-020 — Shipping eligibility API response evidence.COM-GATE-020 — Shipping Done Gate.


### 44.3. AI Advisor

AIA-REQ-001 — AI Advisor must resolve product before quoting.AIA-RULE-001 — AI must not quote price without Pricing Resolver result.AIA-RULE-002 — AI must not promise payment method without Payment Eligibility.AIA-RULE-003 — AI must not promise shipping ETA without Shipping Eligibility.AIA-TEST-001 — Customer asks price without product; AI routes to product clarification.AIA-EVID-001 — Conversation transcript evidence.AIA-GATE-001 — AI Quote Safety Done Gate.


### 44.4. Channel Gateway

CHG-REQ-001 — Gateway must route public price intent to Messenger flow.CHG-RULE-001 — Gateway must not expose final quote in public comment.CHG-RULE-002 — Gateway must not hardcode bank account information.CHG-TEST-001 — Public comment asking price triggers private handoff.CHG-EVID-001 — Meta webhook log and Messenger transcript evidence.CHG-GATE-001 — Gateway Production Gate.


### 44.5. IVR

IVR-REQ-001 — IVR must support order confirmation only.IVR-RULE-001 — IVR result must go through Order Core State Machine.IVR-RULE-002 — 1 SIM handles 1 outbound call at a time.IVR-RULE-003 — Technical error is not customer no-answer.IVR-WRK-001 — Rolling real-time call queue.IVR-TEST-001 — Customer presses 1 and Order Core evaluates confirmation.IVR-TEST-002 — Customer presses 0 and Order Core evaluates cancellation.IVR-EVID-001 — IVR call log and Order Core transition evidence.IVR-GATE-001 — IVR Confirmation Done Gate.


### 44.6. Operational Core

OPS-REQ-001 — Released batch is required before warehouse sellable availability.OPS-RULE-001 — QC_PASS is not RELEASED.OPS-RULE-002 — READY_FOR_PRODUCTION is distinct from QC_PASS.OPS-TEST-001 — Warehouse receipt blocks unreleased batch.OPS-EVID-001 — Batch release and warehouse receipt evidence.OPS-GATE-001 — Operational Sellable Availability Gate.


## 45. TRACEABILITY MATRIX CHUẨN

Mỗi pack phải có traceability matrix theo cấu trúc:

Requirement ID

Rule ID

Source ID

API/DTO/DB/UI/Worker ID

Test ID

Evidence ID

Gate ID

COM-REQ-001

COM-RULE-001

COM-SRC-001

COM-API-001 / COM-DTO-001 / COM-DB-001

COM-TEST-001

COM-EVID-001


## Com-Gate-001

Requirement có rule.

Rule có source.

Source có implementation contract.

Contract có test.

Test có evidence.

Evidence gắn gate.

Gate mới được xét Done.


## 46. TRACEABILITY BẮT BUỘC CHO GLOBAL RULE

Các global rule trong MASTER-00 phải có ID riêng.

Các rule này phải được map xuống pack chi tiết.


## 47. TRACEABILITY GIỮA MASTER VÀ PACK CHI TIẾT

MASTER chỉ ghi rule cấp cao.

Pack chi tiết phải nhận rule và triển khai thành contract cụ thể.

MAS-RULE-002 -> COM-RULE-BANK-001 -> CHG-RULE-PAYMENT-001 -> AIA-RULE-PAYMENT-001 -> CRM-RULE-PAYMENT-001 -> ADM-RULE-PAYMENT-001 -> LDP-RULE-PAYMENT-001 -> COM-TEST-BANK-001 -> EVD-EVID-BANK-001

Ví dụ với shipping:

MAS-RULE-007 -> COM-RULE-SHIP-001 -> AIA-RULE-SHIP-001 -> CHG-RULE-SHIP-001 -> CRM-RULE-SHIP-001 -> LDP-RULE-SHIP-001 -> COM-TEST-SHIP-001 -> EVD-EVID-SHIP-001

Ví dụ với IVR:

MAS-RULE-008 -> IVR-REQ-001 -> IVR-RULE-001 -> IVR-WRK-001 -> IVR-TEST-001 -> IVR-EVID-001 -> IVR-GATE-001


## 48. PACK SEQUENCE CHO COMMERCE CORE

Commerce Runtime phải được tách theo COM file index.

Tối thiểu gồm:

COM-01 — COMMERCE CORE OVERVIEW / SOURCE-OF-TRUTH / BOUNDARYCOM-02 — PRICING / PROGRAM / QUOTE FREEZE / FINAL PRICECOM-03 — CART / ORDER CONFIRMATION DRAFT / ORDER STATE MACHINECOM-04 — MEMBER BENEFIT / CUSTOMER PRICE IDENTITY / ELIGIBILITYCOM-05 — INVENTORY AVAILABILITY / SALE LOCK / QUALITY HOLDCOM-06 — PAYMENT / BANK TRANSFER / VIETQR / VNPAY / MOMO BUSINESS / COMPANY BANK ACCOUNT REGISTRY / ACCOUNTING QUEUECOM-07 — SHIPPING / TRACKING / COD / DOMESTIC SHIPPING / FALLBACK ETACOM-08 — VAT / INVOICE / TOTAL PAYABLE / ACCOUNTING HANDOFFCOM-09 — COMMERCE API / DTO / EVENT CONTRACTCOM-10 — COMMERCE TEST / SMOKE / DONE GATE

COM-06 và COM-07 là hai file bắt buộc vì liên quan trực tiếp đến:

Không tự đánh dấu PAID.

Không tự hứa phương thức thanh toán.

Không tự hứa vận chuyển.

Không tự hứa ETA.

Điều kiện đối soát kế toán.

Điều kiện chốt đơn thật.


## 49. PACK SEQUENCE CHO IVR ORDER CONFIRMATION

PACK-09 IVR Order Confirmation phải được viết theo thứ tự:

IVR-01 — IVR PURPOSE / NON-SCOPE / ORDER CONFIRMATION ONLYIVR-02 — INTERNAL SIM GATEWAY SERVER DEPLOYMENT MODELIVR-03 — ORDER CORE CONFIRMATION TASK CONTRACTIVR-04 — REAL-TIME ROLLING QUEUE / SIM CAPACITY / CALL WINDOWIVR-05 — DTMF RESULT / CUSTOMER CONFIRM / CUSTOMER CANCELIVR-06 — TECHNICAL ERROR / NO-ANSWER / RETRY / TIMEOUT POLICYIVR-07 — ORDER STATE MACHINE BOUNDARYIVR-08 — ADMIN MONITORING / CALL LOG / EVIDENCEIVR-09 — IVR SMOKE TEST / DONE GATE

Trong đó các điểm bắt buộc:

1 SIM = 1 outbound call at a time.Giờ Vàng = 10 phút, 2 cuộc, cách 5 phút.24/7 = 15 phút, 3 cuộc, lịch T0/T0+5/T0+10.Rolling real-time queue required.Batch after session calling prohibited.SIM Gateway must not update order state directly.Technical error is not customer no-answer.


## 50. DEV HANDOFF CHECKLIST CẤP PACK

Mỗi pack khi giao sang triển khai phải có checklist:

[ ] Pack purpose rõ ràng[ ] Scope rõ ràng[ ] Non-scope rõ ràng[ ] Source-of-Truth rõ ràng[ ] Dependency input rõ ràng[ ] Dependency output rõ ràng[ ] Runtime contract rõ ràng[ ] API/DTO nếu có[ ] DB/migration nếu có[ ] UI nếu có[ ] Worker/queue nếu có[ ] State machine nếu có[ ] Security/permission/audit rõ ràng[ ] Error handling rõ ràng[ ] Evidence requirement rõ ràng[ ] Smoke test rõ ràng[ ] Done Gate rõ ràng

Nếu thiếu một trong các mục trọng yếu, pack chưa đạt điều kiện triển khai chính thức.


## 51. DEV HANDOFF CHECKLIST RIÊNG CHO PAYMENT

Payment là nhóm có rủi ro cao, phải có checklist riêng.

[ ] Payment strategy đã khóa: VNPAY + MOMO_BUSINESS + DIRECT_BANK_TRANSFER_TO_SSAVIGROUP[ ] VNPAY nằm dưới Commerce Payment Core[ ] MoMo Business nằm dưới Commerce Payment Core[ ] Direct bank transfer nằm dưới Company Bank Account Registry[ ] Không hardcode tài khoản ngân hàng ở AI/Gateway/CRM/Admin UI/frontend/static template[ ] Bank transfer order có payment_reference / transfer memo[ ] Có Accounting Review Queue[ ] Không PAID nếu chưa có Payment Core confirmation hoặc Accounting Review confirmation[ ] Có payment evidence[ ] Có payment smoke test


## 52. DEV HANDOFF CHECKLIST RIÊNG CHO SHIPPING

Shipping là nhóm ảnh hưởng trực tiếp quote và chốt đơn.

[ ] Shipping scope hiện tại là domestic shipping[ ] Có Shipping Eligibility Resolver[ ] Có COD Eligibility nếu COD được dùng[ ] Có Shipping Fee Rule[ ] Có Free Shipping Rule nếu chương trình có hỗ trợ[ ] Có Fallback ETA Rule[ ] AI/Gateway/CRM không tự hứa ETA nếu chưa có eligibility[ ] Landing Page không hardcode ETA[ ] Admin UI chỉ hiển thị dữ liệu từ Shipping Core[ ] Có shipping evidence[ ] Có shipping smoke test


## 53. DEV HANDOFF CHECKLIST RIÊNG CHO IVR

IVR là nhóm có liên quan trực tiếp tới xác nhận đơn.

[ ] IVR purpose = ORDER_CONFIRMATION_ONLY[ ] Deployment model = INTERNAL_SIM_GATEWAY_SERVER[ ] 1 SIM = 1 outbound call at a time[ ] Giờ Vàng call window = 10 phút[ ] Giờ Vàng retry = 2 cuộc, cách 5 phút[ ] 24/7 call window = 15 phút[ ] 24/7 retry = 3 cuộc, lịch T0/T0+5/T0+10[ ] Rolling real-time queue required[ ] Batch after session calling prohibited[ ] Phím 1 = xác nhận đơn[ ] Phím 0 = khách hủy / không đặt đơn[ ] SIM Gateway không cập nhật order state trực tiếp[ ] IVR result đi qua Core Order State Machine[ ] Technical error không được tính là customer no-answer[ ] IVR không dùng cho sales / CRM / marketing / tư vấn / chăm sóc đại trà[ ] Có IVR call log evidence[ ] Có Order Core transition evidence[ ] Có IVR smoke test


## 54. DEV HANDOFF CHECKLIST RIÊNG CHO AI / GATEWAY / CRM

[ ] Không quote nếu chưa xác định SKU/sản phẩm[ ] Không quote nếu Pricing Resolver chưa trả kết quả[ ] Không quote member price nếu chưa xác định customer_price_identity[ ] Không hứa quyền lợi thành viên nếu Member Core chưa trả dữ liệu[ ] Không hứa payment nếu Payment Core chưa trả eligibility[ ] Không hứa shipping/ETA nếu Shipping Core chưa trả eligibility[ ] Không bán nếu Availability Core chưa trả sellable[ ] Không nói claim ngoài whitelist[ ] Không public final quote ở comment nếu policy yêu cầu vào Messenger[ ] Không gửi nội dung CRM vượt quiet hours / frequency cap / suppression[ ] Có transcript evidence[ ] Có resolver response evidence[ ] Có smoke test thực chiến


## 55. DEV HANDOFF CHECKLIST RIÊNG CHO ADMIN UI

[ ] Admin UI gọi API/service, không hardcode nghiệp vụ lõi[ ] Không hardcode tài khoản ngân hàng[ ] Không hardcode trạng thái PAID[ ] Không hardcode trạng thái shipping[ ] Có permission guard[ ] Có audit event[ ] Có empty state[ ] Có error state[ ] Có evidence view nếu liên quan nghiệm thu[ ] Có smoke test theo role[ ] Có screenshot evidence


## 56. DEV HANDOFF CHECKLIST RIÊNG CHO OPERATIONAL CORE

[ ] QC_PASS không được xem là RELEASED[ ] Raw material QC_PASS không đồng nghĩa READY_FOR_PRODUCTION[ ] Material issue là điểm trừ kho nguyên liệu[ ] Material receipt không trừ kho lần hai[ ] Warehouse receipt chỉ sau batch release[ ] Inventory ledger append-only[ ] Public trace whitelist-only[ ] Recall có snapshot[ ] MISA đi qua integration layer[ ] Có operational smoke test[ ] Có ledger evidence[ ] Có trace evidence


## 57. RELEASE DEPENDENCY MAP

Release toàn hệ chỉ được xét khi các dependency sau có evidence:

Product Knowledge -> AI AdvisorPricing -> QuoteMember -> Final PriceAvailability -> SellablePayment -> Order PaymentShipping -> Order FulfillmentOrder Core -> IVR ConfirmationGateway -> Messenger / Meta FlowCRM -> Lifecycle / SuppressionOperational -> Inventory / TraceAdmin UI -> Operator ControlSecurity -> Permission / AuditEvidence -> Completion ReportSmoke -> Release Gate

Nếu một dependency trọng yếu chưa có evidence, toàn hệ giữ trạng thái:


## 58. SMOKE TRACEABILITY CHUẨN

Mỗi smoke test phải trace được ngược về requirement.

Cấu trúc smoke trace:

SMOKE CASE -> Requirement ID -> Rule ID -> API/DTO/DB/UI/Worker ID -> Test Evidence -> Gate Result

Ví dụ payment smoke:

COM-SMK-006 -> COM-REQ-006 -> MAS-RULE-001 / MAS-RULE-002 / MAS-RULE-003 / MAS-RULE-004 -> COM-API-PAYMENT-ELIGIBILITY -> COM-DB-PAYMENT-REFERENCE -> COM-EVID-PAYMENT-TRANSFER-001 -> COM-GATE-PAYMENT

Ví dụ shipping smoke:

COM-SMK-007 -> COM-REQ-007 -> MAS-RULE-007 -> COM-API-SHIPPING-ELIGIBILITY -> COM-EVID-SHIPPING-001 -> COM-GATE-SHIPPING

Ví dụ IVR smoke:

IVR-SMK-001 -> IVR-REQ-001 -> MAS-RULE-008 / MAS-RULE-009 -> IVR-WRK-QUEUE-001 -> IVR-EVID-CALL-001 -> IVR-GATE-001


## 59. DOCUMENT-TO-DEV-TO-EVIDENCE CONTROL

Toàn bộ hệ thống phải theo chuỗi:

MASTER -> Pack Document -> Requirement -> Rule -> Contract -> Implementation -> Test -> Smoke -> Evidence -> Completion Report -> Release Gate -> Owner Sign-off

Không được đảo chuỗi thành:

Code trước -> Sau đó mới đoán tài liệu -> Sau đó mới tìm rule -> Sau đó mới kiểm tra evidence

Cách làm sai này khiến hệ thống dễ lệch kiến trúc, khó nghiệm thu và khó kiểm soát rủi ro production.


## 60. NGUYÊN TẮC KHÓA XUNG ĐỘT GIỮA PACK

Nếu hai pack có nội dung khác nhau về cùng một rule, xử lý theo thứ tự ưu tiên:


## 1. MASTER-00 Global Rule2. Source-of-Truth Pack3. Core Runtime Contract4. Pack Detail Rule5. UI / Content / Template6. Test / Evidence interpretation

Nếu AI template nói có thể gửi số tài khoản trực tiếp, nhưng MASTER và Commerce Payment nói không được hardcode tài khoản ngân hàng, thì AI template sai.

Nếu Gateway muốn tự báo ETA nhưng Commerce Shipping chưa trả eligibility, Gateway sai.

Nếu IVR muốn tự hủy đơn sau phím 0, nhưng Order Core yêu cầu đi qua Order State Machine, IVR sai.

Nếu Landing Page muốn hiển thị giá cố định nhưng Pricing Core yêu cầu runtime price, Landing Page sai.


## 61. NGUYÊN TẮC KHI PACK CHƯA ĐỦ DỮ LIỆU

Khi pack chưa đủ dữ liệu, không được tự điền bằng suy đoán.

Cách xử lý đúng:

Ghi rõ missing input.Ghi rõ pack nào phải cung cấp.Ghi rõ điểm chặn là gì.Không đưa vào release.Không cho pass Done Gate.Không cho go-live.

Chưa có Payment Eligibility -> AI không được hứa VNPAY/MoMo/chuyển khoản.

Chưa có Company Bank Account Registry -> không được hiển thị tài khoản ngân hàng.

Chưa có Shipping Eligibility -> không được hứa ETA.

Chưa có Order Core confirmation task -> IVR không được gọi.

Chưa có Claim Whitelist -> Landing Page không được public nội dung claim.

Chưa có Availability -> AI không được chốt bán SKU.


## 62. KẾT LUẬN PHẦN 2/4


## PHẦN 2/4 đã xác lập:

Cross-pack dependency graph.Document writing sequence.Dev handoff sequence.Traceability ID standard.Dependency matrix theo pack.Commerce dependency cho payment và shipping.IVR dependency với Order Core.Operational dependency với Commerce Availability.Traceability matrix chuẩn.Smoke traceability chuẩn.Handoff checklist theo nhóm rủi ro.Release dependency map.Quy tắc xử lý xung đột giữa pack.

Các điểm khóa quan trọng:

AI / Gateway / CRM là lớp tương tác, không phải Commerce Core.Commerce Core quyết định quote, order, payment, shipping.Payment phải đi qua Commerce Payment Core.Direct bank transfer phải đi qua Company Bank Account Registry.Không hardcode tài khoản ngân hàng ở bất kỳ lớp tương tác hoặc UI nào.Không PAID nếu thiếu Payment Core confirmation hoặc Accounting Review confirmation.Shipping hiện tại là domestic shipping.Không hứa shipping/ETA nếu thiếu Shipping Core eligibility.IVR chỉ xác nhận đơn và không tự cập nhật order state.Operational Core cung cấp release / inventory / trace cho Commerce Availability.Không có traceability thì không có Done Gate.Không có evidence thì không có Completion Report pass.Không có smoke pass thì không có release.

Trạng thái toàn hệ tiếp tục giữ:

HẾT PHẦN 2/4


## PHẦN 3/4 — GLOBAL RELEASE GOVERNANCE / EVIDENCE PACKAGE / SMOKE MATRIX / MONITORING / ROLLBACK / OWNER SIGN-OFF


## 63. MỤC ĐÍCH CỦA PHẦN 3/4


## PHẦN 3/4 xác định cơ chế quản trị release toàn hệ cho Ginsengfood.

Mục tiêu là bảo đảm mọi hợp phần trước khi đưa vào vận hành thật đều phải có:

Điều kiện release rõ ràng.

Bằng chứng nghiệm thu rõ ràng.

Smoke test bắt buộc.

Monitoring bắt buộc.

Rollback / recovery plan rõ ràng.

Owner sign-off rõ ràng.

Không pack nào được tự tuyên bố hoàn thành nếu thiếu evidence.


## 64. GLOBAL RELEASE GOVERNANCE

Global Release Governance là lớp kiểm soát cuối cùng trước khi một pack hoặc toàn hệ được đưa vào vận hành thật.

Không có evidence -> không Done.Không có smoke pass -> không Release.Không có monitoring -> không Production Ready.Không có rollback plan -> không Go-live.Không có owner sign-off -> không được mở vận hành thật.

Release Governance áp dụng cho tất cả pack:


## Pack-00 Master.

PACK-05 AI Advisor.

PACK-02 Product Knowledge.

PACK-06 Facebook Channel Gateway.

Commerce Runtime.

CRM Runtime / Member Lifecycle.

PACK-01 Operational Core.

Landing Page Runtime.

PACK-07 Ads / ROAS / Attribution / Tracking.

PACK-09 IVR Order Confirmation.

Admin Runtime UI.

PACK-10 Evidence / Smoke / UAT.

Security / Audit Control / Permission / Audit / Compliance.

Không pack nào được miễn trừ evidence và smoke test.


## 65. RELEASE GOVERNANCE PRINCIPLES


### 65.1. Principle 1 — Source-of-Truth First

Mọi release phải chứng minh dữ liệu được lấy từ Source-of-Truth đúng.

Giá phải đến từ Commerce Pricing Core.

Quyền lợi thành viên phải đến từ Member Core.

Payment method phải đến từ Commerce Payment Core.

Bank account phải đến từ Company Bank Account Registry.

Shipping ETA phải đến từ Commerce Shipping Core.

Order state phải đến từ Commerce Order Core.

Product claim phải đến từ Claim Policy.

IVR result phải đi qua Order State Machine.

Inventory sellable phải đến từ Inventory / Availability Core.


### 65.2. Principle 2 — Boundary Must Be Enforced

Mọi release phải chứng minh boundary không bị bypass.

AI không tự tính giá.

Gateway không tự quote public.

CRM không tự hứa payment / shipping.

Admin UI không tự hardcode trạng thái PAID.

Landing Page không hardcode tài khoản ngân hàng.

IVR không tự cập nhật order state.

Operational Core không để Commerce bán batch chưa release.

Public trace không lộ dữ liệu nội bộ.


### 65.3. Principle 3 — Evidence Must Be Concrete

Evidence không được là mô tả miệng.

Evidence phải là bằng chứng có thể kiểm tra lại.

Log.

Database row.

Audit event.

Worker log.

Queue log.

Webhook log.

Accounting review confirmation.

Shipping eligibility response.

IVR call log.

Smoke test report.

UAT checklist.

Monitoring dashboard.


### 65.4. Principle 4 — Smoke Must Cover Real Chain

Smoke test không chỉ kiểm tra từng màn hình rời rạc.

Smoke phải kiểm tra chuỗi nghiệp vụ thật.

Customer asks price -> AI resolves product -> Pricing resolver returns price -> Member resolver returns customer price identity -> Availability resolver confirms sellable -> Payment eligibility returns allowed methods -> Shipping eligibility returns domestic shipping / COD / ETA -> Quote Cart generated -> Order Confirmation Draft generated -> Customer confirms -> Order Core creates order -> IVR confirms if required -> Payment handled -> Shipping handled -> Evidence captured


### 65.5. Principle 5 — Production Ready Is Not Self-Declared

Production readiness phải được xác định qua:

Requirement completed.

Contract completed.

Test completed.

Smoke completed.

Evidence completed.

Monitoring completed.

Rollback completed.

Security completed.

Owner sign-off completed.


## 66. GLOBAL RELEASE STATE MODEL

Trạng thái release toàn hệ gồm các mức:

Tại thời điểm MASTER-00 này, toàn hệ đang giữ:

Không được nhảy trạng thái từ document sang go-live.

Document -> Implementation -> Test -> Smoke -> Evidence -> Review -> Release Approval -> Go-live Approval


## 67. GLOBAL RELEASE GATES

Toàn hệ phải đi qua các gate sau:

GATE-01 — Documentation GateGATE-02 — Source-of-Truth GateGATE-03 — Dependency GateGATE-04 — Contract GateGATE-05 — Implementation GateGATE-06 — Security / Permission / Audit GateGATE-07 — Smoke GateGATE-08 — Evidence GateGATE-09 — Monitoring GateGATE-10 — Rollback GateGATE-11 — Owner Sign-off GateGATE-12 — Go-live Gate

Nếu một gate trọng yếu chưa pass, trạng thái toàn hệ tiếp tục là:


## 68. GATE-01 — DOCUMENTATION GATE

Documentation Gate kiểm tra tài liệu đã đủ để triển khai chưa.

Điều kiện pass:

Pack có mục tiêu rõ ràng.

Scope rõ ràng.

Non-scope rõ ràng.

Boundary rõ ràng.

Source-of-Truth rõ ràng.

Dependency rõ ràng.

Runtime contract rõ ràng.

Security rule rõ ràng.

Evidence requirement rõ ràng.

Smoke test rõ ràng.

Done Gate rõ ràng.

Điều kiện fail:

Pack viết mơ hồ.

Pack để dev tự suy luận.

Pack thiếu Source-of-Truth.

Pack thiếu boundary.

Pack thiếu evidence.

Pack thiếu smoke.

Pack mâu thuẫn MASTER-00.


## 69. GATE-02 — SOURCE-OF-TRUTH GATE

Source-of-Truth Gate kiểm tra pack có lấy dữ liệu từ đúng core không.

Các câu hỏi bắt buộc:

Dữ liệu này thuộc core nào?Pack này có quyền quyết định dữ liệu đó không?Nếu không có quyền, pack gọi resolver/API nào?Nếu resolver lỗi thì xử lý ra sao?Có evidence chứng minh dữ liệu đến từ Source-of-Truth không?

Ví dụ bắt buộc:

AI báo giá phải có Pricing Resolver evidence.

AI báo quyền lợi thành viên phải có Member Resolver evidence.

AI / Gateway / CRM nói phương thức thanh toán phải có Payment Eligibility evidence.

AI / Gateway / CRM nói ETA phải có Shipping Eligibility evidence.

Direct bank transfer phải có Company Bank Account Registry evidence.

Payment PAID phải có Payment Core confirmation hoặc Accounting Review confirmation evidence.

IVR call result phải có Order Core transition evidence.


## 70. GATE-03 — DEPENDENCY GATE

Dependency Gate kiểm tra pack có đủ đầu vào để chạy không.

Dependency input rõ.

Dependency output rõ.

Upstream pack sẵn sàng.

Downstream consumer rõ.

Error case rõ.

Không có dependency mơ hồ.

Không bypass core.

Gateway phụ thuộc AI Advisor và Commerce Core.AI Advisor phụ thuộc Product Knowledge, Pricing, Member, Availability, Payment, Shipping.IVR phụ thuộc Order Core.CRM phụ thuộc Customer Memory, Member Core, Order History, Product Knowledge.Landing Page phụ thuộc Product Public View và Commerce Entry.

Nếu dependency chưa có evidence, pack chưa được release.


## 71. GATE-04 — CONTRACT GATE

Contract Gate kiểm tra các contract đã đủ rõ để triển khai chưa.

Contract bao gồm:

API contract.

DTO contract.

Event contract.

Queue contract.

Worker contract.

UI state contract.

Error contract.

Audit contract.

Permission contract.

Evidence contract.

Payment Eligibility Response phải trả allowed_methods, blocked_reason, evidence_id nếu có.Shipping Eligibility Response phải trả domestic_shipping_supported, COD eligibility, fee, fallback ETA.IVR Result Event phải trả order_id, call_attempt_id, result, technical_status, timestamp.Quote Cart phải có price source, program source, member benefit source, freeze window.Order Confirmation Draft phải có order lines, total payable, payment options, shipping options.

Nếu contract chưa rõ, không được triển khai bằng suy đoán.


## 72. GATE-05 — IMPLEMENTATION GATE

Implementation Gate kiểm tra phần đã làm có đúng tài liệu không.

Đúng pack scope.

Đúng Source-of-Truth.

Đúng API/DTO.

Đúng database/migration nếu có.

Đúng UI/worker nếu có.

Đúng state machine.

Đúng security/audit.

Đúng error handling.

Có test cơ bản.

Có log phục vụ evidence.

Code chạy nhưng sai boundary.

UI hiển thị đúng nhìn bề ngoài nhưng hardcode logic.

AI trả lời đúng một tình huống nhưng không gọi resolver.

Gateway routing đúng nhưng không có webhook evidence.

Payment hiển thị được nhưng hardcode bank account.

Shipping trả lời được nhưng không qua Shipping Core.

IVR gọi được nhưng tự đổi order state.


## 73. GATE-06 — SECURITY / PERMISSION / AUDIT GATE

Security / Permission / Audit Gate là gate bắt buộc trước release.

Có role/permission rõ ràng.

API có kiểm tra quyền.

Admin UI có kiểm tra quyền.

High-risk action có audit.

State transition có audit.

Payment review có audit.

Accounting review có audit.

IVR result có audit/log.

Manual override nếu có phải có reason.

Không lộ dữ liệu nhạy cảm ra public.

Public trace whitelist-only.

Không hardcode credential hoặc bank account.

Các hành vi không được phép:

Frontend tự che nút nhưng backend không kiểm quyền.Admin UI tự cho đổi PAID không có audit.Gateway lưu token hoặc secret trong static config public.Landing Page hardcode bank account.AI template chứa số tài khoản ngân hàng.IVR Gateway tự đổi trạng thái đơn.Public trace trả dữ liệu supplier/personnel/accounting/internal cause.


## 74. GATE-07 — SMOKE GATE

Smoke Gate kiểm tra chuỗi nghiệp vụ tối thiểu có chạy đúng không.

Smoke Gate không thay thế toàn bộ test, nhưng là điều kiện release.

Mỗi smoke case phải có:

Smoke ID.

Requirement ID.

Rule ID.

Pack liên quan.

Dữ liệu đầu vào.

Các bước chạy.

Kết quả mong đợi.

Trạng thái pass/fail.

Người/nhóm phê duyệt theo governance, không ghi cá nhân cụ thể trong MASTER.

Smoke không có evidence thì không được tính pass.


## 75. GATE-08 — EVIDENCE GATE

Evidence Gate kiểm tra bằng chứng có đủ, đúng và truy ngược được không.

Evidence gắn ID.

Evidence gắn requirement/rule/test/smoke.

Evidence có timestamp.

Evidence có môi trường chạy.

Evidence có dữ liệu đầu vào.

Evidence có kết quả đầu ra.

Evidence có log hoặc screenshot/API response/DB row.

Evidence lưu được để kiểm tra lại.

Evidence không chứa secret, password, token, bank credential ngoài phạm vi cho phép.

Evidence không lộ dữ liệu khách hàng ra public.

Nếu evidence thiếu, giữ:


## 76. GATE-09 — MONITORING GATE

Monitoring Gate kiểm tra hệ thống có được theo dõi khi vận hành không.

Các nhóm monitoring bắt buộc:

API health.

Database health.

Queue health.

Worker health.

Gateway webhook health.

Messenger handoff health.

Payment eligibility health.

Payment confirmation health.

Accounting review queue health.

Shipping eligibility health.

Order state transition health.

IVR queue health.

SIM Gateway health.

IVR call success/failure.

CRM suppression/frequency cap.

Inventory availability resolver.

Public trace API.

MISA sync nếu áp dụng.

Evidence storage.

Error rate.

Retry / dead-letter.

Không có monitoring thì không được kết luận production ready.


## 77. GATE-10 — ROLLBACK GATE

Rollback Gate kiểm tra phương án xử lý khi release lỗi.

Rollback không chỉ là quay lại code cũ.

Rollback phải bao gồm:

Code rollback.

Config rollback.

Feature flag.

Queue pause.

Worker pause.

Gateway pause.

Payment method disable.

Shipping method disable.

IVR queue stop.

CRM automation pause.

Landing Page CTA disable.

ADS traffic pause.

Admin UI emergency notice.

Data correction policy nếu có lỗi dữ liệu.

Audit record cho hành động khẩn cấp.

Nếu không có rollback plan, không được go-live.


## 78. GATE-11 — OWNER SIGN-OFF GATE

Owner Sign-off Gate xác nhận pack hoặc toàn hệ được phê duyệt theo governance.

MASTER-00 không ghi người cụ thể.

Sign-off phải theo vai trò sở hữu hợp phần, không theo tên cá nhân trong tài liệu MASTER.

Các nhóm sign-off tối thiểu:

Product / Brand sign-off.

Commerce sign-off.

Payment / Accounting sign-off.

Shipping / Fulfillment sign-off.

AI Advisor sign-off.

Channel Gateway sign-off.

CRM sign-off.

Operational Core sign-off.

Security / Audit sign-off.

Evidence / QA sign-off.

Release sign-off.

Nếu thiếu sign-off trọng yếu, trạng thái giữ:


## 79. GATE-12 — GO-LIVE GATE

Go-live Gate là cổng cuối cùng.

Điều kiện tối thiểu:

Chỉ khi tất cả điều kiện pass mới được xét:

Tại thời điểm tài liệu này, các trạng thái trên vẫn là:

EVIDENCE PACKAGE


## 80. GLOBAL EVIDENCE PACKAGE STANDARD

Evidence Package là bộ bằng chứng bắt buộc để chứng minh pack đã được triển khai và kiểm thử đúng.

Mỗi evidence item phải có:

evidence_idpack_idrequirement_idrule_idtest_idsmoke_id nếu cóenvironmenttimestampinput_dataoutput_resultartifact_typeartifact_locationreview_statusnotes

Artifact type có thể là:

Database snapshot.

State transition record.

Payment provider response.

Accounting review record.

Shipping provider response.

Public trace response.

Smoke report.

UAT report.

Monitoring dashboard snapshot.


## 81. EVIDENCE PACKAGE STRUCTURE

Cấu trúc thư mục evidence khuyến nghị ở cấp tài liệu:

Trong mỗi pack:

requirements/rules/api/dto/db/ui/worker/events/audit/smoke/uat/monitoring/rollback/sign-off/

Mục tiêu là khi kiểm tra một evidence, có thể truy ngược ngay về requirement và gate.


## 82. EVIDENCE STATUS MODEL

Mỗi evidence item có trạng thái:

MISSINGCOLLECTEDNEEDS_REVIEWACCEPTEDREJECTEDRETEST_REQUIRED

MISSING: chưa có bằng chứng.

COLLECTED: đã có bằng chứng nhưng chưa xem xét.

NEEDS_REVIEW: cần kiểm tra thêm.

ACCEPTED: được chấp nhận.

REJECTED: bị từ chối.

RETEST_REQUIRED: phải test lại.


## 83. EVIDENCE TỐI THIỂU CHO PACK-05 AI Advisor

PACK-01 cần evidence tối thiểu:

Product resolution transcript.

Pricing resolver response.

Member resolver response.

Availability resolver response.

Payment eligibility response nếu AI nói phương thức thanh toán.

Shipping eligibility response nếu AI nói ETA/phí ship/COD.

Quote Cart handoff.

Order Confirmation Draft handoff.

Claim whitelist compliance.

Existing customer care flow.

New customer order form flow.

OOS alternative recommendation flow.

Abuse/profanity handling nếu qua live/channel.

AI response transcript.

Smoke test transcript.

Không có resolver evidence thì AI response không được xem là hợp lệ cho release.


## 84. EVIDENCE TỐI THIỂU CHO PACK-02 PRODUCT KNOWLEDGE

PACK-02 cần evidence tối thiểu:

20 SKU baseline list.

Public product name mapping.

Internal SKU mapping.

Product group mapping.

Public View sample.

Internal View sample.

Product Knowledge API/seed evidence nếu có.

Cross-check giữa Product Knowledge và AI response.

Landing Page content compliance.

CRM content compliance.

ADS content compliance.


## 85. EVIDENCE TỐI THIỂU CHO PACK-06 Facebook Channel Gateway

PACK-03 cần evidence tối thiểu:

Meta webhook receive log.

Comment public routing evidence.

Messenger handoff evidence.

Public price intent handling.

Messenger private flow evidence.

Abuse/profanity hidden/flagged evidence.

No public final quote evidence.

No hardcoded bank account evidence.

Gateway error handling evidence.

Page configuration evidence.

Rate limit / spam control evidence.

UAT screenshot.

Gateway smoke report.

Gateway không được mở nếu thiếu evidence trọng yếu.

Trạng thái tiếp tục:


## 86. EVIDENCE TỐI THIỂU CHO Commerce Runtime

PACK-04 cần evidence tối thiểu:

Quote Cart API response.

Pricing Resolver response.

Program Resolver response.

Member Benefit Resolver response.

Availability Resolver response.

Order Confirmation Draft response.

Order State Machine transition.

Payment Eligibility response.

VNPAY eligibility evidence.

MoMo Business eligibility evidence.

Direct bank transfer eligibility evidence.

Company Bank Account Registry evidence.

payment_reference / transfer memo evidence.

Accounting Review Queue evidence.

Payment Core confirmation evidence.

Shipping Eligibility response.

COD eligibility evidence nếu COD dùng.

Domestic shipping fee evidence.

Fallback ETA evidence.

VAT / total payable evidence nếu áp dụng.

Order final state evidence.

Audit log evidence.

Đặc biệt bắt buộc:

Không có Company Bank Account Registry evidence -> không pass Direct Bank Transfer.Không có payment_reference / transfer memo evidence -> không pass Bank Transfer Order.Không có Payment Core hoặc Accounting Review confirmation -> không được PAID.Không có Shipping Eligibility evidence -> không được hứa ETA/shipping.


## 87. EVIDENCE TỐI THIỂU CHO CRM Runtime / MEMBER LIFECYCLE

PACK-05 cần evidence tối thiểu:

Customer Memory read evidence.

Order History evidence.

Member tier evidence.

Member benefit evidence.

Lifecycle event evidence.

Quiet hours enforcement.

Frequency cap enforcement.

Suppression enforcement.

Grace lifecycle evidence.

Maintain / upgrade / downgrade safe handling.

Product recommendation based on purchase history.

Existing customer care before next sale evidence.

CRM message transcript.

Opt-out / suppression handling nếu có.

CRM smoke report.

CRM không được gửi nội dung nếu thiếu suppression/frequency cap/quiet hours evidence.


## 88. EVIDENCE TỐI THIỂU CHO PACK-01 Operational Core

PACK-06 cần evidence tối thiểu:

Source origin evidence.

Raw material receipt evidence.

Incoming QC evidence.

Raw lot READY_FOR_PRODUCTION evidence.

Production order snapshot.

Material issue evidence.

Inventory ledger debit.

Material receipt evidence.

Process chain evidence.

Packaging job evidence.

QR/print evidence.

QC inspection evidence.

Batch release evidence.

Warehouse receipt evidence.

Inventory ledger credit.

Sellable availability evidence.

Recall snapshot.

CAPA evidence nếu có.

MISA integration dry-run / sync evidence nếu áp dụng.

Operational smoke report.

Các hard lock bắt buộc:

QC_PASS != RELEASED.Raw material QC_PASS != READY_FOR_PRODUCTION.Warehouse receipt requires released batch.Inventory ledger append-only.Public trace whitelist-only.


## 89. EVIDENCE TỐI THIỂU CHO Landing Page Runtime

PACK-07 cần evidence tối thiểu:

Product public content screenshot.

CTA flow evidence.

No hardcoded final runtime price evidence nếu giá phụ thuộc chương trình.

No unsupported shipping promise evidence.

Tracking event evidence.

Form submission evidence nếu có.

Error/empty state evidence.

Mobile display evidence.

Landing Page smoke report.


## 90. EVIDENCE TỐI THIỂU CHO PACK-07 Ads / ROAS / Attribution / TRACKING

PACK-08 cần evidence tối thiểu:

Campaign tracking event.

Creative mapping.

Landing Page / Gateway attribution.

Messenger conversion signal.

Order conversion signal.

Revenue attribution nếu có.

No claim violation evidence.

No hardcoded payment/shipping promise evidence.

Event payload evidence.

Tracking dashboard evidence.

ADS smoke report.

ADS không được quyết định commerce data.

ADS chỉ nhận signal hợp lệ.


## 91. EVIDENCE TỐI THIỂU CHO PACK-09 IVR ORDER CONFIRMATION

PACK-09 cần evidence tối thiểu:

Order Core confirmation task evidence.

IVR queue entry.

SIM capacity evidence.

1 SIM = 1 active outbound call evidence.

Giờ Vàng call window evidence.

24/7 call window evidence.

Retry timing evidence.

Rolling real-time queue evidence.

No batch-after-session evidence.

DTMF phím 1 evidence.

DTMF phím 0 evidence.

Technical error evidence.

No-answer evidence.

IVR result event.

Order Core State Machine transition evidence.

Admin monitoring screenshot.

IVR smoke report.

Điểm bắt buộc:

SIM Gateway không được cập nhật order state trực tiếp.Technical error không được tính là customer no-answer.IVR chỉ dùng để xác nhận đơn hàng.IVR không dùng cho sales / CRM / marketing / tư vấn / chăm sóc đại trà.

Trạng thái của PACK-09 tại MASTER-00:


## 92. EVIDENCE TỐI THIỂU CHO Admin Runtime UI

PACK-10 cần evidence tối thiểu:

Login / permission evidence.

Role-based menu evidence.

API-backed data display.

No hardcoded payment state.

No hardcoded bank account.

Payment review screen.

Accounting queue screen.

Shipping review screen.

Order state screen.

IVR monitoring screen nếu có.

Gateway status screen nếu có.

Evidence viewer screen.

Error state screenshot.

Empty state screenshot.

Admin UI smoke report.

Admin UI chỉ là operator workspace, không phải core nghiệp vụ.


## 93. EVIDENCE TỐI THIỂU CHO PACK-10 Evidence / SMOKE / UAT

Evidence registry.

Smoke matrix.

Test report.

Completion Report.

Evidence status dashboard.

Missing evidence list.

Rejected evidence list.

Retest required list.

Release readiness summary.

Final gate checklist.

Nếu còn evidence trọng yếu bị thiếu:


## 94. EVIDENCE TỐI THIỂU CHO Security / Audit Control / AUDIT / COMPLIANCE

Security / Audit Control cần evidence tối thiểu:

Role/permission matrix.

API permission test.

UI permission test.

Audit log sample.

State transition log sample.

Payment review audit.

Accounting review audit.

IVR result audit/log.

CRM suppression audit.

Public trace data leak test.

Sensitive data handling evidence.

Credential handling evidence.

No hardcoded secret evidence.

Security smoke report.


## Smoke Matrix


## 95. GLOBAL SMOKE MATRIX STANDARD

Smoke Matrix cấp MASTER phải bao phủ các chuỗi nghiệp vụ trọng yếu.

smoke_idpack_scopescenariopreconditionstepsexpected_resultrequired_evidenceblocking_levelstatus

Blocking level:


## P0_Stop_Pointp1_Release_Stop_Pointp2_Review_Required

P0_STOP_POINT không được waive cho go-live.


## 96. MASTER SMOKE MATRIX — P0

Smoke ID

Chuỗi kiểm tra

Pack liên quan

Expected result

Blocking

MAS-SMK-001

Product -> AI consult

PACK-01, PACK-02

AI dùng đúng Product Knowledge và Claim Policy

P0

MAS-SMK-002

AI quote runtime

PACK-01, PACK-04

AI không quote nếu thiếu Pricing/Member/Availability

MAS-SMK-003

Payment eligibility

PACK-04

VNPAY/MoMo/Bank Transfer chỉ hiện khi Payment Core cho phép

MAS-SMK-004

Bank transfer governance

PACK-04, PACK-10, Security / Audit Control

Bank account lấy từ Company Bank Account Registry, có payment_reference

MAS-SMK-005

No PAID without confirmation

Payment không PAID nếu thiếu Payment Core/Accounting Review confirmation

MAS-SMK-006

Shipping eligibility

PACK-04, PACK-01, PACK-03, PACK-05

AI/Gateway/CRM không hứa ETA nếu Shipping Core chưa trả eligibility

MAS-SMK-007

Gateway public price intent

PACK-01, PACK-03, PACK-04

Public comment không lộ final quote, kéo đúng private flow

MAS-SMK-008

Order Confirmation Draft

Draft đủ sản phẩm, giá, ship, payment, total, xác nhận

MAS-SMK-009

IVR confirmation

PACK-04, PACK-09

IVR gọi đúng task, phím 1/0 đi qua Order State Machine

MAS-SMK-010

IVR technical error

PACK-09

Technical error không tính customer no-answer

MAS-SMK-011

CRM suppression

PACK-05

CRM không gửi vượt quiet hours/frequency cap/suppression

MAS-SMK-012

Operational sellable availability

PACK-04, PACK-06

Chỉ bán batch đã release và có warehouse availability

MAS-SMK-013

Public trace safety

PACK-06, Security / Audit Control

Public trace whitelist-only, không lộ dữ liệu nội bộ

MAS-SMK-014

Admin permission/audit

PACK-10, Security / Audit Control

Action trọng yếu có permission và audit

MAS-SMK-015

Completion Report evidence

PACK-10

Không pass nếu evidence thiếu


## 97. PAYMENT SMOKE MATRIX

Scenario

COM-SMK-PAY-001

Customer chọn VNPAY

Payment Core xác nhận eligibility trước khi hiển thị

Payment Eligibility response

COM-SMK-PAY-002

Customer chọn MoMo Business

COM-SMK-PAY-003

Customer chọn chuyển khoản

Bank account lấy từ Company Bank Account Registry

Registry response

COM-SMK-PAY-004

Bank transfer order

Có payment_reference / transfer memo

Order/payment record

COM-SMK-PAY-005

Khách gửi ảnh chuyển khoản

Không tự PAID nếu chưa review/confirmation

Payment state evidence

COM-SMK-PAY-006

Accounting Review xác nhận

Payment Core chuyển PAID qua rule hợp lệ

Audit + state transition

COM-SMK-PAY-007

AI/Gateway/CRM nói payment

Chỉ nói khi Payment Core trả eligibility

Transcript + resolver evidence

COM-SMK-PAY-008

Static template scan

Không có hardcoded bank account

Scan report


## 98. SHIPPING SMOKE MATRIX

COM-SMK-SHIP-001

Có địa chỉ trong nước hợp lệ

Shipping Core trả domestic eligibility

API response

COM-SMK-SHIP-002

Thiếu địa chỉ

AI không hứa ETA

Transcript

COM-SMK-SHIP-003

COD được hỗ trợ

COD chỉ hiển thị khi eligible

Shipping response

COM-SMK-SHIP-004

Free shipping rule

Miễn phí ship chỉ khi rule trả đúng

Quote evidence

COM-SMK-SHIP-005

Fallback ETA

ETA đến từ Shipping Core

COM-SMK-SHIP-006

Gateway trả lời giao hàng

Không tự hứa nếu thiếu eligibility

Gateway transcript

COM-SMK-SHIP-007

Landing Page hiển thị shipping

Không hardcode ETA/phí ship

UI screenshot


## 99. IVR SMOKE MATRIX

IVR-SMK-001

Order Core tạo confirmation task

IVR nhận task hợp lệ

Queue log

IVR-SMK-002

Giờ Vàng call window

2 cuộc trong 10 phút, cách 5 phút

Call log

IVR-SMK-003

24/7 call window

3 cuộc trong 15 phút, lịch T0/T0+5/T0+10

IVR-SMK-004

1 SIM nhiều task

SIM chỉ xử lý 1 active outbound call

SIM log

IVR-SMK-005

Khách bấm phím 1

IVR gửi result, Order Core xử lý xác nhận

Result event + state transition

IVR-SMK-006

Khách bấm phím 0

IVR gửi result, Order Core xử lý hủy/không đặt

IVR-SMK-007

Lỗi kỹ thuật

Không ghi customer no-answer

Error log

IVR-SMK-008

Queue sau phiên

Không batch gọi sau session

Queue evidence

IVR-SMK-009

IVR sales misuse

Không dùng IVR cho sales/CRM/marketing

Policy test

IVR-SMK-010

SIM Gateway direct update

Không có quyền cập nhật order state trực tiếp

Permission/API evidence


## 100. GATEWAY SMOKE MATRIX

CHG-SMK-001

Public comment hỏi giá

Không trả final quote public

Comment log

CHG-SMK-002

Public comment mơ hồ

Route đúng sang Messenger/private flow

Webhook + transcript

CHG-SMK-003

Messenger hỏi giá SKU rõ

AI gọi Pricing/Member/Availability

Resolver evidence

CHG-SMK-004

Messenger hỏi thanh toán

Gateway/AI chỉ trả theo Payment Eligibility

CHG-SMK-005

Messenger hỏi giao hàng

Gateway/AI chỉ trả theo Shipping Eligibility

CHG-SMK-006

Abuse/profanity

Không trả public, flag/ẩn theo policy

Moderation log

CHG-SMK-007

Gateway error

Có error log, không tạo đơn sai

Error evidence

CHG-SMK-008

Meta webhook

Nhận/gửi đúng event

Webhook log


## 101. AI ADVISOR SMOKE MATRIX

AIA-SMK-001

Khách hỏi sản phẩm

AI dùng Product Knowledge

AIA-SMK-002

Khách hỏi giá chưa rõ SKU

AI không quote, kéo xác định sản phẩm

AIA-SMK-003

Khách hỏi giá SKU rõ

AIA-SMK-004

Khách là member

AI dùng đúng member tier/benefit runtime

AIA-SMK-005

Khách chốt đơn

AI tạo Order Confirmation Draft, không tạo order chính thức trước xác nhận

Draft evidence

AIA-SMK-006

Khách hỏi payment

AI không hứa nếu Payment Core chưa trả eligibility

AIA-SMK-007

Khách hỏi ETA

AI không hứa nếu Shipping Core chưa trả eligibility

AIA-SMK-008

Khách cũ quay lại

AI hỏi thăm trải nghiệm trước khi bán tiếp

AIA-SMK-009

Sản phẩm hết hàng

AI gợi ý thay thế theo rule, không bán SKU hết hàng

Transcript + availability

AIA-SMK-010

Claim sensitive

AI không nói chữa bệnh/điều trị/thay thuốc


## 102. OPERATIONAL SMOKE MATRIX

OPS-SMK-001

Raw material intake

Tạo receipt/lot đúng rule

DB/API evidence

OPS-SMK-002

Incoming QC pass

QC_PASS không đồng nghĩa READY_FOR_PRODUCTION

State evidence

OPS-SMK-003

Mark ready

Raw lot READY_FOR_PRODUCTION mới được issue

State transition

OPS-SMK-004

Production order

Snapshot công thức bất biến

Snapshot evidence

OPS-SMK-005

Material issue

Chỉ issue mới trừ kho

Ledger evidence

OPS-SMK-006

Material receipt

Không trừ kho lần hai

OPS-SMK-007

Batch QC pass

QC_PASS không đồng nghĩa RELEASED

OPS-SMK-008

Batch release

Warehouse receipt chỉ sau release

Release evidence

OPS-SMK-009

Public trace

Whitelist-only

OPS-SMK-010

Có snapshot, không overwrite

MONITORING


## 103. GLOBAL MONITORING STANDARD

Monitoring phải giúp phát hiện lỗi vận hành trước khi gây ảnh hưởng lớn.

Monitoring không chỉ theo dõi server sống/chết.

Monitoring phải theo dõi:

Runtime flow.

Business state.

Queue.

Payment.

Shipping.

Gateway.

Security.

SLA/SLO nếu có.


## 104. MONITORING CATEGORIES


### 104.1. System Health

API uptime.

Database connection.

Cache nếu có.

Queue connection.

Worker status.

Storage status.

External provider status.

CPU/RAM/disk.

Latency.


### 104.2. Commerce Monitoring

Quote success rate.

Pricing resolver failure.

Member resolver failure.

Availability resolver failure.

Order draft creation failure.

Order state transition failure.

Payment eligibility failure.

Payment confirmation delay.

Accounting Review Queue backlog.

Shipping eligibility failure.

COD eligibility failure.

Order stuck state.


### 104.3. Payment Monitoring

Bắt buộc theo dõi:

VNPAY eligibility success/fail.

MoMo Business eligibility success/fail.

Direct bank transfer instruction generated.

Missing payment_reference.

Bank transfer chưa đủ điều kiện review count.

Accounting Review Queue aging.

Payment confirmation mismatch.

Payment marked PAID event.

Manual review action.

Payment error.

Suspicious duplicate payment reference.

Cảnh báo P0:

Payment marked PAID without valid confirmation.Bank account rendered from non-registry source.Order missing payment_reference for direct bank transfer.


### 104.4. Shipping Monitoring

Shipping eligibility success/fail.

Missing address.

COD eligibility fail.

Shipping fee calculation error.

Free shipping rule mismatch.

Fallback ETA missing.

Tracking creation failure.

Order stuck before shipping.

Shipping provider error.

AI/Gateway/CRM promised ETA without Shipping Eligibility evidence.Order shipped without valid shipping method.COD promised but not eligible.


### 104.5. IVR Monitoring

IVR queue length.

Task waiting time.

SIM online/offline.

SIM busy count.

Active outbound call count.

Call attempt count.

Call success rate.

DTMF 1 count.

DTMF 0 count.

No-answer count.

Technical error count.

Retry schedule.

Orders exceeding confirmation window.

IVR result delivery to Order Core.

Order Core state transition after IVR result.

SIM Gateway directly updates order state.Technical error counted as customer no-answer.Batch calling after session detected.1 SIM has more than 1 active outbound call.IVR used outside order confirmation purpose.


### 104.6. Gateway Monitoring

Webhook receive rate.

Webhook fail rate.

Messenger handoff success.

Public comment reply success.

Abuse/profanity detection.

Hidden comment count.

bị chặn/troll flag count.

AI response latency.

Resolver failure in Messenger.

Gateway retry.

Gateway dead-letter.

Meta API error.

Final quote exposed in public comment.Gateway sends bank account hardcoded text.Gateway promises payment/shipping without core eligibility.Gateway creates order state directly.


### 104.7. CRM Monitoring

Scheduled lifecycle events.

Suppressed messages.

Quiet hours blocks.

Frequency cap blocks.

Retry count.

Opt-out.

Member lifecycle message.

Customer Memory read failure.

Order History read failure.

Recommendation resolver failure.

Message send failure.

CRM sends during quiet hours.CRM sends above frequency cap.CRM promises payment/shipping without eligibility.CRM sends claim outside whitelist.


### 104.8. Operational Monitoring

Raw material lot state.

QC chưa đủ điều kiện.

READY_FOR_PRODUCTION lot.

Production order status.

Material issue status.

Inventory ledger error.

Batch QC status.

Batch release status.

Warehouse receipt status.

Sellable inventory.

Public trace availability.

Recall open cases.

MISA sync status nếu áp dụng.

Warehouse receipt without released batch.Inventory ledger mutation instead of append-only.Public trace exposes internal fields.Commerce sells non-released/non-sellable batch.


## 105. MONITORING EVIDENCE

Mỗi nhóm monitoring phải có evidence:

Dashboard screenshot.

Alert rule config.

Sample alert.

Error log sample.

Recovery log sample.

Queue backlog screenshot.

Worker health screenshot.

Provider status sample.

Smoke monitoring snapshot.

Không có monitoring evidence thì không pass Monitoring Gate.

ROLLBACK / RECOVERY


## 106. GLOBAL ROLLBACK STANDARD

Rollback là cơ chế đưa hệ thống về trạng thái an toàn khi phát hiện lỗi sau release.

Rollback không được làm mất audit.

Rollback không được sửa lịch sử giao dịch đã ghi nhận theo cách phá vỡ traceability.

Rollback phải ưu tiên:

Dừng tác động mới.Giữ nguyên bằng chứng.Ngăn lỗi lan rộng.Khôi phục luồng an toàn.Ghi audit.Thông báo đúng nhóm vận hành.


## 107. ROLLBACK LEVELS

Các mức rollback:

LEVEL 1 — Feature disableLEVEL 2 — Queue pauseLEVEL 3 — Worker pauseLEVEL 4 — Provider disableLEVEL 5 — Gateway blockLEVEL 6 — Release rollbackLEVEL 7 — Emergency operational hold

Feature disable: tắt một tính năng.

Queue pause: dừng hàng đợi xử lý.

Worker pause: dừng worker.

Provider disable: tắt provider như payment/shipping/IVR.

Gateway block: chặn gateway nhận/đẩy event.

Release rollback: quay về bản triển khai trước.

Emergency operational hold: dừng nghiệp vụ rủi ro cao.


## 108. PAYMENT ROLLBACK

Rollback payment áp dụng khi có lỗi:

Hiển thị sai phương thức thanh toán.

Sai payment reference.

Payment bị đánh dấu PAID sai.

VNPAY lỗi.

MoMo Business lỗi.

Accounting Review Queue lỗi.

Hành động an toàn:

Disable affected payment method.Stop showing affected payment instruction.Pause automatic payment confirmation if unsafe.Route chưa đủ điều kiện payments to Accounting Review Queue.Keep audit log.Notify Admin UI.Collect evidence.Retest before enabling again.

Đặc biệt:

Nếu phát hiện hardcoded bank account ngoài registry: -> Block immediately. -> Remove rendered source. -> Force Registry-only rendering. -> Scan AI/Gateway/CRM/Admin UI/frontend/static template. -> Collect evidence.


## 109. SHIPPING ROLLBACK

Rollback shipping áp dụng khi có lỗi:

ETA sai.

COD eligibility sai.

Phí ship sai.

Free shipping rule sai.

Provider lỗi.

Tracking lỗi.

Disable affected shipping promise.Fallback to manual shipping review.Block AI/Gateway/CRM from promising ETA.Keep orders in review state if needed.Collect shipping evidence.Retest Shipping Eligibility.

Không được tiếp tục hứa ETA khi Shipping Core lỗi.


## 110. IVR ROLLBACK

Rollback IVR áp dụng khi có lỗi:

SIM Gateway lỗi.

DTMF sai.

Technical error bị tính nhầm no-answer.

Queue gọi trễ sau session.

1 SIM xử lý nhiều outbound call.

IVR result không gửi được về Order Core.

IVR tự cập nhật order state.

Pause IVR queue.Stop outbound call worker.Keep chưa đủ điều kiện orders in Order Core review state.Do not cancel orders solely due to IVR technical failure.Route to manual confirmation if needed.Collect call log and queue evidence.Retest IVR smoke.

IVR rollback không được làm mất Order Core audit.IVR rollback không được tự hủy đơn.IVR rollback không được đánh khách là không nghe máy nếu lỗi thuộc kỹ thuật.


## 111. GATEWAY ROLLBACK

Rollback Gateway áp dụng khi:

Public comment trả sai nội dung.

Final quote bị lộ public.

Gateway gửi sai Messenger flow.

Gateway lỗi webhook.

Gateway gửi hardcoded bank account.

Gateway hứa shipping/payment sai.

Abuse/profanity handling sai.

Disable public auto-reply if needed.Keep Messenger manual fallback.Pause risky templates.Block final quote public rendering.Remove unsafe payment/shipping wording.Collect webhook/transcript evidence.Retest gateway smoke.


## 112. AI ADVISOR ROLLBACK

Rollback AI Advisor áp dụng khi:

AI nói sai claim.

AI quote sai giá.

AI bán SKU không sellable.

AI hứa payment/shipping khi chưa có eligibility.

AI bỏ qua customer memory quan trọng.

AI tạo order chính thức trước khi khách xác nhận.

AI nói nội dung trái policy.

Disable affected response path.Force resolver-required mode.Block unsafe templates.Route to human/manual review for high-risk intent.Collect conversation evidence.Retest AI Advisor smoke.


## 113. CRM ROLLBACK

Rollback CRM áp dụng khi:

Gửi vượt frequency cap.

Gửi ngoài quiet hours.

Gửi sai quyền lợi thành viên.

Gửi sai sản phẩm.

Gửi claim ngoài whitelist.

Gửi payment/shipping promise sai.

Không tôn trọng suppression.

Pause affected campaign.Apply suppression safety.Review Customer Memory and Member Core dependency.Collect message logs.Retest CRM lifecycle smoke.


## 114. OPERATIONAL ROLLBACK / RECOVERY

Operational rollback cần đặc biệt thận trọng vì liên quan ledger, trace, production, recall.

Không xóa lịch sử.Không mutate ledger append-only.Không overwrite trace/recall snapshot.Không sửa batch release trái audit.Không mở bán batch chưa đủ điều kiện.

Đưa batch vào hold nếu cần.

Dừng sellable availability nếu có rủi ro.

Tạo correction record thay vì sửa lịch sử.

Tạo audit event.

Tạo recall / recovery workflow nếu ảnh hưởng sản phẩm đã bán.

Đồng bộ lại Commerce Availability.

Thu thập evidence.


## 115. ADMIN UI ROLLBACK

Rollback Admin UI áp dụng khi:

UI cho thao tác vượt quyền.

UI hiển thị dữ liệu sai.

UI hardcode bank account.

UI cho đổi payment state sai.

UI thiếu audit action.

UI gọi sai API.

Disable affected screen/action.Keep backend permission strict.Hide unsafe control.Force API validation.Collect screenshot and audit evidence.Retest role permission smoke.

OWNER SIGN-OFF


## 116. OWNER SIGN-OFF STANDARD

Owner Sign-off là xác nhận chính thức theo vai trò sở hữu hợp phần.

MASTER-00 không ghi tên người cụ thể.

Sign-off phải ghi theo nhóm trách nhiệm:

signoff_idpack_idscopegateevidence_packagesmoke_statusknown_risksapproval_statusapproval_timestampnotes

Trạng thái sign-off:

WAITINGAPPROVEDREJECTEDCONDITIONALLY_APPROVEDRETEST_REQUIRED

Nếu còn chưa đủ điều kiện, REJECTED hoặc RETEST_REQUIRED ở gate trọng yếu, không được go-live.


## 117. SIGN-OFF MATRIX CẤP MASTER

Sign-off ID

Nhóm sign-off

Phạm vi

Điều kiện

MAS-SIGN-001

Master Governance

Pack registry, gate, boundary

MASTER đầy đủ và không mâu thuẫn

MAS-SIGN-002

Product / Claim

Product Knowledge, claim public

Claim whitelist pass

MAS-SIGN-003

AI Advisor

AI consult, quote handoff

AI smoke + transcript evidence pass

MAS-SIGN-004

Channel Gateway

Meta Live/Messenger

Gateway smoke + webhook evidence pass

MAS-SIGN-005

Commerce

Quote, order, payment, shipping

Commerce smoke pass

MAS-SIGN-006

Payment / Accounting

VNPAY, MoMo, bank transfer, review

Payment evidence pass

MAS-SIGN-007

Shipping / Fulfillment

Domestic shipping, COD, ETA

Shipping evidence pass

MAS-SIGN-008

CRM / Member

Lifecycle, member rights, suppression

CRM smoke pass

MAS-SIGN-009

Order confirmation only

IVR smoke pass

MAS-SIGN-010

Operational

Production, inventory, trace, recall

Operational smoke pass

MAS-SIGN-011

Admin UI

Operator workspace

Permission/UI smoke pass

MAS-SIGN-012

Security / Audit

Permission, audit, compliance

Security evidence pass

MAS-SIGN-013

Evidence / QA

Completion report

Evidence Gate pass

MAS-SIGN-014

Release

Final release

All gates pass


## 118. SIGN-OFF KHÔNG ĐƯỢC LÀM GÌ

Sign-off không được dựa trên:

Cảm tính.

Demo miệng.

Ảnh chụp không gắn test ID.

Test rời rạc không trace requirement.

Code chạy một lần trên local.

Không có audit.

Không có smoke.

Không có evidence.

Không có rollback.

Không có monitoring.

Sign-off đúng phải dựa trên:

Requirement -> Rule -> Contract -> Implementation -> Test -> Smoke -> Evidence -> Gate -> Sign-off


## 119. COMPLETION REPORT GOVERNANCE

Completion Report là tài liệu tổng hợp trạng thái hoàn thành.

Completion Report phải thể hiện:

Pack nào đã làm.

Pack nào chưa làm.

Gate nào pass.

Gate nào fail.

Evidence nào đủ.

Evidence nào thiếu.

Smoke nào pass.

Smoke nào fail.

Known risk.

điểm chặn.

Retest required.

Release recommendation.

Go-live recommendation.

Tại thời điểm MASTER-00 này:

Do đó Completion Report chưa được dùng để tuyên bố go-live.


## 120. COMPLETION REPORT MINIMUM STRUCTURE

Completion Report tối thiểu phải có:


## 1. Executive Release Status2. Pack Completion Matrix3. Gate Status Matrix4. Smoke Test Summary5. Evidence Package Summary6. Security / Audit Summary7. Payment Evidence Summary8. Shipping Evidence Summary9. Gateway Evidence Summary10. IVR Evidence Summary11. CRM Evidence Summary12. Operational Evidence Summary13. Admin UI Evidence Summary14. Monitoring Readiness15. Rollback Readiness16. Known Stop Points17. Retest Required18. Owner Sign-off Summary19. Final Recommendation


## 121. RELEASE DECISION RULE

Release decision chỉ có 3 kết quả:

RELEASE_REJECTED: chưa đủ điều kiện, phải xử lý lại.

RELEASE_CONDITIONALLY_HELD: có thể tiếp tục xử lý kỹ thuật nhưng chưa được go-live.

Tại thời điểm MASTER-00:


## 122. GO-LIVE DECISION RULE

Go-live decision chỉ có 2 kết quả:

All P0 smoke pass.

All P0 evidence accepted.

Security gate pass.

Payment gate pass.

Shipping gate pass.

Gateway gate pass.

IVR gate pass nếu IVR nằm trong luồng release.

Monitoring gate pass.

Rollback gate pass.

Owner sign-off complete.


## 123. GLOBAL điểm chặn LIST

Các lỗi sau là điểm chặn toàn hệ:

AI tự quote không qua Pricing Resolver.AI/Gateway/CRM hứa payment khi chưa có Payment Eligibility.AI/Gateway/CRM hứa shipping/ETA khi chưa có Shipping Eligibility.Hardcoded bank account xuất hiện ở AI/Gateway/CRM/Admin UI/frontend/static template.Bank transfer order thiếu payment_reference / transfer memo.Payment bị PAID khi chưa có Payment Core confirmation hoặc Accounting Review confirmation.Gateway lộ final quote ở public comment nếu policy cấm.CRM gửi vượt quiet hours / frequency cap / suppression.IVR dùng ngoài mục đích xác nhận đơn hàng.IVR tự cập nhật order state.Technical error bị tính là customer no-answer.Batch after session calling trong IVR.Commerce bán SKU/batch không sellable.Warehouse receipt cho batch chưa release.Public trace lộ dữ liệu nội bộ.Admin UI cho thao tác vượt quyền.Thiếu audit ở hành động trọng yếu.Completion Report thiếu evidence P0.Smoke P0 fail.Không có rollback plan.Không có monitoring.Thiếu owner sign-off trọng yếu.

Nếu có bất kỳ điểm chặn nào, trạng thái giữ:


## 124. RELEASE READINESS SUMMARY FORMAT

Mẫu tóm tắt readiness cấp MASTER:

Tại thời điểm hiện tại:


## 125. RELEASE GOVERNANCE FOR FUTURE GOVERNED EXTENSION

Nếu sau này có mở rộng ngoài phạm vi hiện tại, phải đi qua governed extension.

Ghi nhận duy nhất ở MASTER-00:

Điều kiện tối thiểu nếu mở rộng sau này:

Owner approval.

Legal/compliance review.

Payment provider capability evidence.

Shipping provider capability evidence.

Tax/accounting review.

Customer communication rule.

Pricing/VAT/fee rule.

Risk handling.

Monitoring.

Rollback.

Phần này không phải phạm vi đang vận hành.

Phạm vi hiện tại vẫn là:

Thanh toán trong nước.Vận chuyển trong nước.


## 126. KẾT LUẬN PHẦN 3/4


## PHẦN 3/4 đã xác lập cơ chế kiểm soát release toàn hệ:

Global Release Governance.Release Gate model.Evidence Package standard.Evidence status model.Smoke Matrix P0.Payment smoke.Shipping smoke.IVR smoke.Gateway smoke.AI Advisor smoke.Operational smoke.Monitoring standard.Rollback standard.Owner Sign-off standard.Completion Report governance.Release decision rule.Go-live decision rule.Global điểm chặn list.

Không có evidence thì không Done.Không có smoke pass thì không Release.Không có monitoring thì không Production Ready.Không có rollback thì không Go-live.Không có owner sign-off thì không mở vận hành thật.Direct bank transfer phải qua Company Bank Account Registry.Không hardcode tài khoản ngân hàng ở AI/Gateway/CRM/Admin UI/frontend/static template.Bank transfer order phải có payment_reference / transfer memo.Không PAID nếu thiếu Payment Core confirmation hoặc Accounting Review confirmation.AI/Gateway/CRM không được hứa payment nếu Payment Core chưa trả eligibility.AI/Gateway/CRM không được hứa shipping/ETA nếu Shipping Core chưa trả eligibility.IVR chỉ xác nhận đơn hàng.IVR không tự cập nhật order state.Technical error không phải customer no-answer.Shipping hiện tại là domestic shipping.Payment hiện tại là VNPAY + MoMo Business + Direct Bank Transfer to SSAVIGROUP.

HẾT PHẦN 3/4


## PHẦN 4/4 — FINAL MASTER DONE GATE / FILE SEQUENCE / PACK SEQUENCE / GLOBAL HANDOFF CONCLUSION


## 127. MỤC ĐÍCH CỦA PHẦN 4/4


## PHẦN 4/4 là phần khóa cuối của MASTER-00.

Phần này dùng để xác định:

Done Gate cuối cùng của MASTER-00.

Điều kiện để từng pack được xem là đủ điều kiện triển khai.

Điều kiện để từng pack được xem là đủ điều kiện nghiệm thu.

Thứ tự file cần viết trong toàn bộ hệ thống.

Thứ tự pack cần triển khai.

Điều kiện handoff từ tài liệu sang triển khai.

Kết luận trạng thái toàn hệ.

Kết luận chưa được release / chưa được go-live nếu thiếu evidence.

MASTER-00 không thay thế tài liệu chi tiết của từng pack.

MASTER-00 chỉ khóa:

Pack nào tồn tại.Pack nào phụ thuộc pack nào.Pack nào sở hữu Source-of-Truth.Pack nào không được bypass core.Pack nào cần evidence.Pack nào cần smoke.Pack nào đang bị chặn release.Điều kiện nào phải pass trước khi go-live.


## 128. FINAL MASTER DONE GATE LÀ GÌ

Final Master Done Gate là cổng kiểm tra cuối cùng để xác định MASTER-00 đã hoàn thành vai trò governance cấp cao hay chưa.

MASTER-00 chỉ được xem là Done khi:

Pack registry đầy đủ.

Source-of-Truth toàn hệ rõ ràng.

Boundary giữa các hợp phần rõ ràng.

Dependency graph rõ ràng.

Document writing sequence rõ ràng.

Dev handoff sequence rõ ràng.

Traceability ID standard rõ ràng.

Release governance rõ ràng.

Evidence package standard rõ ràng.

Smoke matrix rõ ràng.

Monitoring standard rõ ràng.

Rollback standard rõ ràng.

Owner sign-off standard rõ ràng.

File sequence rõ ràng.

Pack sequence rõ ràng.

Global điểm chặn list rõ ràng.

Trạng thái toàn hệ được giữ đúng.

MASTER-00 Done không đồng nghĩa toàn hệ Production Ready.

MASTER-00 Done chỉ có nghĩa là lớp governance đã đủ để các pack chi tiết tiếp tục được viết và triển khai theo đúng trật tự.


## 129. FINAL MASTER DONE GATE CHECKLIST

MASTER-00 đạt Done Gate nếu tất cả mục sau được đánh dấu pass:

Nếu một mục trọng yếu fail, MASTER-00 chưa đạt Done Gate.


## 130. MASTER DONE KHÔNG ĐỒNG NGHĨA VỚI GO-LIVE

Cần phân biệt rõ:

MASTER-00 Done != Pack Done != Implementation Done != Smoke Pass != Evidence Accepted != Production Ready != Release Approved != Go-live Approved

MASTER-00 Done chỉ cho phép đi tiếp sang:

Viết pack chi tiết.

Chốt runtime contract.

Chốt API/DTO/DB/UI/worker rule.

Tạo test matrix.

Tạo evidence checklist.

Handoff triển khai theo từng phase.

MASTER-00 không được dùng để tuyên bố hệ thống đã hoàn thành.


## 131. FINAL MASTER STATUS

Trạng thái toàn hệ tại cuối MASTER-00 bắt buộc giữ nguyên:

Gateway chưa được mở vận hành thật.

Completion Report chưa đủ evidence.

Hệ thống chưa production ready.

Chưa được release.

Chưa được go-live.

Không pack con nào được ghi trạng thái cao hơn trạng thái toàn hệ nếu chưa có evidence và sign-off.

FILE SEQUENCE TOÀN HỆ


## 132. NGUYÊN TẮC FILE SEQUENCE

File sequence là thứ tự viết và quản trị tài liệu để tránh lệch kiến trúc.

MASTER đi trước.Pack source-of-truth đi trước pack consumer.Commerce core đi trước AI quote / Gateway / CRM / IVR.Evidence và smoke phải đi cùng từng pack.Security / audit là yêu cầu xuyên suốt.

Không viết file theo cảm hứng.

Không viết file con trước khi chưa biết nó phụ thuộc file nào.

Không để dev tự suy luận phần thiếu.


## 133. MASTER FILE SEQUENCE

Nhóm MASTER gồm:

MASTER-00 — GINSENGFOOD MASTER TECHNICAL DOCUMENTATION INDEXMASTER-01 — GLOBAL SOURCE-OF-TRUTH REGISTRYMASTER-02 — CROSS-PACK DEPENDENCY MAPMASTER-03 — TRACEABILITY ID STANDARDMASTER-04 — RELEASE GOVERNANCE / DONE GATE STANDARDMASTER-05 — EVIDENCE PACKAGE STANDARDMASTER-06 — SMOKE MATRIX STANDARDMASTER-07 — QUẢN TRỊ PACK DỰ PHÒNG / RANH GIỚI TÍCH HỢP TƯƠNG LAI

Trong đó MASTER-07 dùng để quản trị các hợp phần chưa triển khai chính thức hoặc các hướng mở rộng tương lai có kiểm soát.

Với phạm vi hiện tại, thanh toán và vận chuyển ngoài phạm vi trong nước không phải active scope.

Nếu cần ghi nhận, chỉ ghi:


## 134. PRODUCT KNOWLEDGE FILE SEQUENCE

PACK-02 Product Knowledge cần có tối thiểu:

PKM-00 — PRODUCT KNOWLEDGE README / SOURCE-OF-TRUTHPKM-01 — SKU MASTER 20 SKUPKM-02 — PRODUCT PUBLIC VIEWPKM-03 — PRODUCT INTERNAL VIEWPKM-04 — CLAIM WHITELISTPKM-05 — CLAIM DENYLISTPKM-06 — PUBLIC WORDING POLICYPKM-07 — PRODUCT KNOWLEDGE API / DATA CONTRACTPKM-08 — SKU SEED MANIFESTPKM-09 — PRODUCT KNOWLEDGE TEST / SMOKE / DONE GATE

PACK-02 là đầu vào bắt buộc cho:

ADS.

Public content.

Claim compliance.


## 135. COMMERCE CORE FILE SEQUENCE

Commerce Runtime cần có tối thiểu:

COM-06 và COM-07 là file bắt buộc.

COM-06 phải khóa:

Direct bank transfer to SSAVIGROUP.

Company Bank Account Registry.

VietQR nếu dùng cho chuyển khoản.

payment_reference / transfer memo.

Payment Core confirmation.

No PAID without confirmation.

COM-07 phải khóa:

Domestic shipping.

Shipping fee.

Free shipping nếu có rule.

AI/Gateway/CRM không hứa ETA nếu thiếu eligibility.


## 136. AI ADVISOR FILE SEQUENCE

PACK-05 AI Advisor cần có tối thiểu:

AIA-00 — AI ADVISOR README / BOUNDARY / SOURCE-OF-TRUTHAIA-01 — AI ADVISOR PARENT LOGICAIA-02 — PRODUCT CONSULTING FLOWAIA-03 — PRICE / QUOTE / MEMBER / AVAILABILITY RESOLVER USAGEAIA-04 — PAYMENT / SHIPPING ELIGIBILITY RESPONSE RULEAIA-05 — ORDER CONFIRMATION DRAFT HANDOFFAIA-06 — CUSTOMER MEMORY / EXISTING CUSTOMER CAREAIA-07 — OOS ALTERNATIVE RECOMMENDATIONAIA-08 — CLAIM SAFETY / PUBLIC WORDINGAIA-09 — LIVE / MESSENGER RESPONSE BLOCKAIA-10 — AI ADVISOR TEST / SMOKE / DONE GATE

AI Advisor không được tự sở hữu giá, payment, shipping, tồn kho hoặc order state.

AI Advisor chỉ được sử dụng kết quả từ core tương ứng.


## 137. CHANNEL GATEWAY FILE SEQUENCE

PACK-06 Facebook Channel Gateway cần có tối thiểu:

CHG-00 — CHANNEL GATEWAY README / META LIVE / MESSENGER BOUNDARYCHG-01 — PAGE / APP / WEBHOOK CONFIGURATIONCHG-02 — PUBLIC COMMENT POLICYCHG-03 — MESSENGER PRIVATE FLOWCHG-04 — PRICE INTENT ROUTINGCHG-05 — PAYMENT / SHIPPING PROMISE GUARDCHG-06 — ABUSE / PROFANITY / BRAND ATTACK HANDLINGCHG-07 — HANDOFF TO AI / COMMERCE / CRMCHG-08 — META APP REVIEW / UAT / EVIDENCECHG-09 — GATEWAY SMOKE / COMPLETION REPORTCHG-10 — GATEWAY DONE GATE

Gateway không được:

Public final quote nếu policy yêu cầu private flow.

Hứa payment nếu Payment Core chưa trả eligibility.

Hứa shipping/ETA nếu Shipping Core chưa trả eligibility.

Tự tạo hoặc tự đổi order state.


## 138. CRM / MEMBER LIFECYCLE FILE SEQUENCE

CRM Runtime cần có tối thiểu:

CRM-00 — CRM / MEMBER LIFECYCLE READMECRM-01 — CUSTOMER MEMORY CONTRACTCRM-02 — ORDER HISTORY / PRODUCT EFFECTIVENESS USAGECRM-03 — MEMBER TIER / BENEFIT / MAINTAIN / UPGRADE / GRACECRM-04 — 12-MONTH LIFECYCLE EVENT REGISTRYCRM-05 — QUIET HOURS / FREQUENCY CAP / SUPPRESSIONCRM-06 — POST-PURCHASE CARECRM-07 — REPURCHASE / FAMILY CARE / PRODUCT RECOMMENDATIONCRM-08 — PAYMENT / SHIPPING PROMISE GUARDCRM-09 — CRM TEST / SMOKE / DONE GATE

CRM không được gửi nội dung vượt:


## 139. OPERATIONAL CORE FILE SEQUENCE

PACK-01 Operational Core cần có tối thiểu:

OPS-00 — OPERATIONAL CORE README / SOURCE-OF-TRUTHOPS-01 — SOURCE ORIGIN / RAW MATERIAL INTAKEOPS-02 — INCOMING QC / RAW LOT READINESSOPS-03 — SKU / INGREDIENT / RECIPE / FORMULA SNAPSHOTOPS-04 — PRODUCTION ORDER / PROCESS CHAINOPS-05 — MATERIAL REQUEST / ISSUE / RECEIPTOPS-06 — PACKAGING / PRINTING / QROPS-07 — QC INSPECTION / BATCH RELEASEOPS-08 — WAREHOUSE / INVENTORY LEDGER / SELLABLE AVAILABILITYOPS-09 — TRACEABILITY / PUBLIC TRACEOPS-10 — RECALL / RECOVERY / CAPAOPS-11 — MISA INTEGRATION BOUNDARYOPS-12 — OPERATIONAL TEST / SMOKE / DONE GATE

Operational Core phải bảo đảm:

QC_PASS không đồng nghĩa RELEASED.

Raw lot QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.

Warehouse receipt chỉ sau batch release.

Inventory ledger append-only.

Recall có snapshot.

MISA đi qua integration layer.


## 140. LANDING PAGE FILE SEQUENCE

Landing Page Runtime cần có tối thiểu:

LDP-00 — LANDING PAGE README / PUBLIC BOUNDARYLDP-01 — PRODUCT PUBLIC DISPLAYLDP-02 — CLAIM COMPLIANCELDP-03 — CTA / MESSENGER HANDOFFLDP-04 — COMMERCE ENTRY CONTRACTLDP-05 — TRACKING / CONVERSION EVENTLDP-06 — PAYMENT / SHIPPING DISPLAY GUARDLDP-07 — LANDING PAGE TEST / SMOKE / DONE GATE

Tự hứa phương thức thanh toán.

Tự hứa vận chuyển/ETA.

Nói claim ngoài whitelist.


## 141. ADS / TRACKING FILE SEQUENCE

PACK-07 Ads / ROAS / Attribution cần có tối thiểu:

ADS-00 — ADS / TRACKING READMEADS-01 — CAMPAIGN / CREATIVE / SOURCE STRUCTUREADS-02 — EVENT TRACKING CONTRACTADS-03 — MESSENGER / GATEWAY SIGNALADS-04 — ORDER CONVERSION SIGNALADS-05 — REVENUE ATTRIBUTIONADS-06 — CLAIM / PAYMENT / SHIPPING GUARDADS-07 — ADS DASHBOARD / LEARNING SIGNALADS-08 — ADS TEST / SMOKE / DONE GATE

ADS chỉ nhận signal.

ADS không được quyết định:

Tồn kho.

Order state.


## 142. IVR ORDER CONFIRMATION FILE SEQUENCE

PACK-09 IVR Order Confirmation cần có tối thiểu:

PACK-09 status:

Rule khóa:

IVR chỉ xác nhận đơn hàng.1 SIM = 1 active outbound call.Phím 1 = xác nhận đơn.Phím 0 = khách hủy / không đặt đơn.Giờ Vàng = 10 phút, 2 cuộc, cách 5 phút.24/7 = 15 phút, 3 cuộc, lịch T0/T0+5/T0+10.Rolling real-time queue required.Batch after session calling prohibited.SIM Gateway không được cập nhật order state trực tiếp.Mọi hủy/tiếp tục xử lý đơn từ IVR phải qua Core Order State Machine.Technical error is not customer no-answer.IVR không dùng cho sales, CRM, marketing, tư vấn sản phẩm hoặc chăm sóc đại trà.


## 143. ADMIN UI FILE SEQUENCE

Admin Runtime UI cần có tối thiểu:

ADM-00 — ADMIN UI README / OPERATOR WORKSPACE BOUNDARYADM-01 — ROLE-BASED MENU / PERMISSION UIADM-02 — ORDER MANAGEMENT SCREENADM-03 — PAYMENT REVIEW / ACCOUNTING QUEUE SCREENADM-04 — SHIPPING REVIEW / TRACKING SCREENADM-05 — CRM / MEMBER REVIEW SCREENADM-06 — GATEWAY MONITORING SCREENADM-07 — IVR MONITORING SCREENADM-08 — OPERATIONAL DASHBOARDADM-09 — EVIDENCE VIEWERADM-10 — ADMIN UI TEST / SMOKE / DONE GATE

Admin UI phải gọi API/service.

Admin UI không được hardcode nghiệp vụ lõi.

Admin UI không được hardcode tài khoản ngân hàng.

Admin UI không được tự đánh dấu PAID nếu thiếu confirmation hợp lệ.


## 144. EVIDENCE / SMOKE / UAT FILE SEQUENCE

PACK-10 Evidence cần có tối thiểu:

EVD-00 — EVIDENCE / SMOKE / UAT READMEEVD-01 — EVIDENCE REGISTRYEVD-02 — SMOKE MATRIXEVD-03 — UAT CHECKLISTEVD-04 — COMPLETION REPORT TEMPLATEEVD-05 — PAYMENT EVIDENCE PACKAGEEVD-06 — SHIPPING EVIDENCE PACKAGEEVD-07 — GATEWAY EVIDENCE PACKAGEEVD-08 — AI ADVISOR EVIDENCE PACKAGEEVD-09 — IVR EVIDENCE PACKAGEEVD-10 — OPERATIONAL EVIDENCE PACKAGEEVD-11 — SECURITY / AUDIT EVIDENCE PACKAGEEVD-12 — FINAL RELEASE EVIDENCE SUMMARY

Nếu evidence trọng yếu còn thiếu:


## 145. SECURITY / PERMISSION / AUDIT FILE SEQUENCE

Security / Audit Control cần có tối thiểu:

SEC-00 — SECURITY / PERMISSION / AUDIT READMESEC-01 — ROLE / PERMISSION MODELSEC-02 — BACKEND PERMISSION ENFORCEMENTSEC-03 — ADMIN UI PERMISSION ENFORCEMENTSEC-04 — AUDIT LOG / STATE TRANSITION LOGSEC-05 — HIGH-RISK ACTION APPROVALSEC-06 — PAYMENT / ACCOUNTING REVIEW AUDITSEC-07 — IVR / GATEWAY / CRM AUDITSEC-08 — PUBLIC TRACE DATA SAFETYSEC-09 — CREDENTIAL / SECRET / BANK ACCOUNT SAFETYSEC-10 — SECURITY TEST / SMOKE / DONE GATE

Không pack nào được bypass security / permission / audit.

PACK SEQUENCE TOÀN HỆ


## 146. NGUYÊN TẮC PACK SEQUENCE

Pack sequence là thứ tự ưu tiên triển khai theo dependency.

Không nên triển khai từ giao diện trước khi core chưa rõ.

Không nên triển khai AI quote trước khi Commerce Pricing chưa rõ.

Không nên triển khai IVR trước khi Order Core chưa rõ.

Không nên triển khai Gateway production trước khi Evidence và Smoke chưa pass.

Thứ tự đúng:

Governance -> Product -> Commerce -> AI -> Gateway -> CRM -> IVR -> Operational Linkage -> Landing Page -> ADS -> Admin UI -> Evidence -> Security Cross-check -> Release Review


## 147. PACK SEQUENCE CHÍNH THỨC

Thứ tự pack chính thức:


## 01. PACK-00 — MASTER GOVERNANCE / SOURCE-OF-TRUTH / RELEASE CONTROL02. PACK-02 — PRODUCT KNOWLEDGE / SKU MASTER / CLAIM POLICY03. Commerce Runtime — Quote / Order / Payment / Shipping

Security / Audit Control phải được kiểm tra xuyên suốt từng pack, không đợi cuối mới làm.


## 148. PACK-00 DONE GATE

PACK-00 pass khi:

PACK-00 Done không làm thay Pack chi tiết.


## 149. PACK-02 DONE GATE

PACK-02 pass khi:

[ ] 20 SKU baseline đầy đủ.[ ] Tên sản phẩm công khai đầy đủ.[ ] SKU nội bộ map đúng.[ ] Nhóm sản phẩm rõ.[ ] Public View đầy đủ.[ ] Internal View đầy đủ.[ ] Claim whitelist đầy đủ.[ ] Claim denylist đầy đủ.[ ] Public wording policy đầy đủ.[ ] AI / Gateway / CRM / Landing Page dùng được Product Knowledge.[ ] Product Knowledge smoke pass.[ ] Evidence accepted.

Nếu thiếu Product Knowledge, AI Advisor không được quote/tư vấn sâu.


## 150. PACK-04 DONE GATE

PACK-04 pass khi:

[ ] Pricing Resolver hoạt động.[ ] Program Resolver hoạt động.[ ] Member Benefit Resolver hoạt động.[ ] Availability Resolver hoạt động.[ ] Quote Cart hoạt động.[ ] Order Confirmation Draft hoạt động.[ ] Order State Machine hoạt động.[ ] Payment Eligibility hoạt động.[ ] VNPAY nằm dưới Payment Core.[ ] MoMo Business nằm dưới Payment Core.[ ] Direct Bank Transfer nằm dưới Company Bank Account Registry.[ ] Bank transfer có payment_reference / transfer memo.[ ] Accounting Review Queue hoạt động.[ ] Không PAID nếu thiếu confirmation hợp lệ.[ ] Shipping Eligibility hoạt động.[ ] Domestic shipping được khóa.[ ] COD / fee / fallback ETA rõ nếu áp dụng.[ ] Commerce API/DTO/Event Contract rõ.[ ] Commerce smoke pass.[ ] Evidence accepted.

PACK-04 chưa pass thì AI / Gateway / CRM / IVR không được vận hành thật.


## 151. PACK-01 DONE GATE

PACK-01 pass khi:

[ ] AI dùng Product Knowledge đúng.[ ] AI không nói claim ngoài whitelist.[ ] AI xác định đúng sản phẩm/SKU trước khi quote.[ ] AI gọi Pricing Resolver trước khi báo giá.[ ] AI gọi Member Resolver trước khi báo giá member.[ ] AI gọi Availability Resolver trước khi chốt bán.[ ] AI gọi Payment Eligibility trước khi nói phương thức thanh toán.[ ] AI gọi Shipping Eligibility trước khi nói ETA/phí ship/COD.[ ] AI tạo Order Confirmation Draft, không tạo order chính thức trước xác nhận.[ ] AI chăm sóc khách cũ trước khi bán tiếp.[ ] AI xử lý OOS đúng rule.[ ] AI smoke pass.[ ] Conversation evidence accepted.


## 152. PACK-03 DONE GATE

PACK-03 pass khi:

[ ] Meta webhook hoạt động.[ ] Public comment policy hoạt động.[ ] Messenger handoff hoạt động.[ ] Public price intent không lộ final quote.[ ] Gateway gọi AI/Commerce đúng contract.[ ] Gateway không hardcode bank account.[ ] Gateway không hứa payment nếu thiếu Payment Eligibility.[ ] Gateway không hứa shipping/ETA nếu thiếu Shipping Eligibility.[ ] Abuse/profanity/brand attack handling hoạt động.[ ] Gateway monitoring có evidence.[ ] Gateway smoke pass.[ ] Gateway UAT evidence accepted.

Nếu chưa pass, trạng thái vẫn là:


## 153. PACK-05 DONE GATE

PACK-05 pass khi:

[ ] Customer Memory hoạt động.[ ] Order History hoạt động.[ ] Member lifecycle hoạt động.[ ] 12-month event registry rõ.[ ] Quiet hours hoạt động.[ ] Frequency cap hoạt động.[ ] Suppression hoạt động.[ ] Grace lifecycle rõ.[ ] Existing customer care before next sale hoạt động.[ ] CRM không hứa payment/shipping nếu thiếu eligibility.[ ] CRM không nói claim ngoài whitelist.[ ] CRM smoke pass.[ ] Evidence accepted.


## 154. PACK-09 DONE GATE

PACK-09 pass khi:

[ ] IVR purpose = ORDER_CONFIRMATION_ONLY.[ ] Deployment model = INTERNAL_SIM_GATEWAY_SERVER.[ ] Order Core confirmation task contract hoạt động.[ ] Rolling real-time queue hoạt động.[ ] 1 SIM = 1 active outbound call.[ ] Giờ Vàng = 10 phút, 2 cuộc, cách 5 phút.[ ] 24/7 = 15 phút, 3 cuộc, lịch T0/T0+5/T0+10.[ ] Phím 1 = xác nhận đơn.[ ] Phím 0 = khách hủy / không đặt đơn.[ ] Technical error không tính là customer no-answer.[ ] Batch after session calling bị chặn.[ ] SIM Gateway không cập nhật order state trực tiếp.[ ] IVR result đi qua Core Order State Machine.[ ] IVR không dùng cho sales/CRM/marketing/tư vấn/chăm sóc đại trà.[ ] IVR monitoring có evidence.[ ] IVR smoke pass.[ ] Evidence accepted.

Nếu chưa có đầy đủ evidence:


## 155. PACK-06 DONE GATE

PACK-06 pass khi:

[ ] Source origin rõ.[ ] Raw material intake hoạt động.[ ] Incoming QC hoạt động.[ ] Raw lot READY_FOR_PRODUCTION rõ.[ ] Formula snapshot bất biến.[ ] Production order hoạt động.[ ] Material issue trừ kho đúng.[ ] Material receipt không trừ kho lần hai.[ ] QC_PASS không đồng nghĩa RELEASED.[ ] Warehouse receipt chỉ sau batch release.[ ] Inventory ledger append-only.[ ] Sellable availability đúng.[ ] Public trace whitelist-only.[ ] Recall có snapshot.[ ] MISA integration boundary đúng nếu áp dụng.[ ] Operational smoke pass.[ ] Evidence accepted.


## 156. PACK-07 DONE GATE

PACK-07 pass khi:

[ ] Landing Page dùng Product Public View.[ ] Claim compliance pass.[ ] CTA đúng flow.[ ] Messenger handoff đúng.[ ] Commerce Entry đúng contract.[ ] Không hardcode final runtime price.[ ] Không hardcode bank account.[ ] Không hứa payment nếu thiếu Payment Eligibility.[ ] Không hứa shipping/ETA nếu thiếu Shipping Eligibility.[ ] Tracking event hoạt động.[ ] Landing Page smoke pass.[ ] Evidence accepted.


## 157. PACK-08 DONE GATE

PACK-08 pass khi:

[ ] Campaign tracking rõ.[ ] Event tracking contract rõ.[ ] Gateway signal nhận đúng.[ ] Order conversion signal nhận đúng.[ ] Revenue attribution nếu có hoạt động đúng.[ ] Claim compliance pass.[ ] Không quyết định giá/tồn kho/payment/shipping.[ ] ADS dashboard có evidence.[ ] ADS smoke pass.[ ] Evidence accepted.


## 158. PACK-10 DONE GATE

PACK-10 pass khi:

[ ] Admin UI gọi API/service đúng.[ ] Role-based menu hoạt động.[ ] Backend permission không bị bypass.[ ] Order management đúng contract.[ ] Payment Review / Accounting Queue đúng contract.[ ] Shipping review đúng contract.[ ] IVR monitoring đúng nếu có.[ ] Gateway monitoring đúng nếu có.[ ] Evidence viewer hoạt động.[ ] Không hardcode bank account.[ ] Không hardcode PAID state.[ ] Audit log ghi đúng.[ ] Admin UI smoke pass.[ ] Screenshot evidence accepted.


## 159. PACK-10 DONE GATE

[ ] Evidence registry đầy đủ.[ ] Smoke matrix đầy đủ.[ ] UAT checklist đầy đủ.[ ] Completion Report đầy đủ.[ ] Payment evidence accepted.[ ] Shipping evidence accepted.[ ] Gateway evidence accepted.[ ] AI Advisor evidence accepted.[ ] IVR evidence accepted nếu IVR thuộc release scope.[ ] Operational evidence accepted.[ ] Security evidence accepted.[ ] Missing evidence list không còn P0 điểm chặn.[ ] Final Release Evidence Summary hoàn tất.

Nếu chưa đủ:


## 160. Security / Audit Control DONE GATE

Security / Audit Control pass khi:

[ ] Role / permission model rõ.[ ] Backend permission enforcement pass.[ ] Admin UI permission enforcement pass.[ ] Audit log hoạt động.[ ] State transition log hoạt động.[ ] High-risk action có approval/reason nếu cần.[ ] Payment / Accounting Review audit pass.[ ] IVR / Gateway / CRM audit pass.[ ] Public trace data safety pass.[ ] Credential / secret / bank account safety pass.[ ] No hardcoded bank account evidence accepted.[ ] Security smoke pass.[ ] Evidence accepted.

Không có Security Gate thì không được release.

GLOBAL HANDOFF CONTROL


## 161. GLOBAL HANDOFF LÀ GÌ

Global handoff là quá trình chuyển từ tài liệu governance sang triển khai có kiểm soát.

Handoff không phải là “đưa tài liệu rồi tự làm”.

Handoff phải bảo đảm:

Dev biết pack nào làm trước.

Dev biết dữ liệu lấy từ đâu.

Dev biết không được bypass core nào.

Dev biết API/DTO/DB/UI/worker nào cần có.

Dev biết evidence nào phải nộp.

Dev biết smoke nào phải pass.

Dev biết trạng thái nào đang điểm chặn.

Dev biết chưa được go-live nếu thiếu evidence.


## 162. GLOBAL HANDOFF PACKAGE

Một Global Handoff Package tối thiểu gồm:


## 1. MASTER-002. Pack registry3. Pack sequence4. File sequence5. Source-of-Truth registry6. Dependency graph7. Traceability ID standard8. Handoff checklist9. Smoke matrix10. Evidence checklist11. Release gate checklist12. Known điểm chặn list13. Current global status

Không được handoff nếu thiếu:

Dependency.

Gate.


## 163. HANDOFF THEO PHASE

Khuyến nghị handoff theo phase:

PHASE 01 — MASTER / GOVERNANCE FOUNDATIONPHASE 02 — PRODUCT KNOWLEDGE FOUNDATIONPHASE 03 — COMMERCE CORE FOUNDATIONPHASE 04 — AI ADVISOR RUNTIMEPHASE 05 — CHANNEL GATEWAYPHASE 06 — CRM / MEMBER LIFECYCLEPHASE 07 — IVR ORDER CONFIRMATIONPHASE 08 — OPERATIONAL CORE LINKAGEPHASE 09 — LANDING PAGE / PUBLIC WEBPHASE 10 — ADS / TRACKING SIGNALPHASE 11 — ADMIN UIPHASE 12 — EVIDENCE / SMOKE / UATPHASE 13 — SECURITY / AUDIT CROSS-CHECKPHASE 14 — RELEASE REVIEW

Mỗi phase phải có:

Input document.

Scope.

Non-scope.

API/DTO/DB/UI/worker contract nếu có.


## 164. HANDOFF RULE CHO PAYMENT

Payment là nhóm không được giao mơ hồ.

Handoff Payment phải nói rõ:

Payment strategy = VNPAY + MOMO_BUSINESS + DIRECT_BANK_TRANSFER_TO_SSAVIGROUP.VNPAY là governed payment channel dưới Commerce Payment Core.MoMo Business là governed payment channel dưới Commerce Payment Core.Direct Bank Transfer phải qua Company Bank Account Registry.Bank transfer order phải có payment_reference / transfer memo.Không hardcode bank account ở AI/Gateway/CRM/Admin UI/frontend/static template.Không PAID nếu thiếu Payment Core confirmation hoặc Accounting Review confirmation.Accounting Review Queue là bắt buộc cho trường hợp cần đối soát.

Handoff payment thiếu các điểm trên là chưa đạt.


## 165. HANDOFF RULE CHO SHIPPING

Shipping là nhóm phải giao theo eligibility.

Handoff Shipping phải nói rõ:

Shipping hiện tại = domestic shipping.Shipping Eligibility Core quyết định có giao được hay không.COD Eligibility nếu COD được dùng.Shipping fee do Shipping Core trả.Free shipping nếu có phải do rule trả.Fallback ETA do Shipping Core trả.AI/Gateway/CRM/Landing Page không được tự hứa ETA/phí ship/COD.

Không ghi international shipping như active scope.

Nếu sau này mở rộng, đi theo:


## 166. HANDOFF RULE CHO IVR

Handoff IVR phải nói rõ:

IVR chỉ xác nhận đơn hàng.Không dùng IVR cho sales / CRM / marketing / tư vấn / chăm sóc đại trà.Internal SIM Gateway Server là mô hình mặc định.1 SIM = 1 active outbound call.Phím 1 = xác nhận đơn.Phím 0 = khách hủy / không đặt đơn.Giờ Vàng = 10 phút, 2 cuộc, cách 5 phút.24/7 = 15 phút, 3 cuộc, lịch T0/T0+5/T0+10.Rolling real-time queue required.Batch after session calling prohibited.SIM Gateway không cập nhật order state trực tiếp.IVR result phải đi qua Core Order State Machine.Technical error không phải customer no-answer.

Nếu thiếu Order Core State Machine, IVR chưa được triển khai vận hành thật.


## 167. HANDOFF RULE CHO AI / GATEWAY / CRM

Handoff cho AI / Gateway / CRM phải nói rõ:

Không tự tính giá.Không tự báo giá nếu chưa xác định sản phẩm/SKU.Không tự hứa member benefit nếu chưa có Member Core.Không tự hứa payment nếu chưa có Payment Eligibility.Không tự hứa shipping/ETA nếu chưa có Shipping Eligibility.Không tự bán nếu chưa có Availability.Không hardcode bank account.Không tự đánh dấu PAID.Không tạo order chính thức trước xác nhận.Không nói claim ngoài whitelist.Không gửi CRM vượt quiet hours/frequency cap/suppression.


## 168. HANDOFF RULE CHO ADMIN UI

Handoff Admin UI phải nói rõ:

Admin UI là operator workspace.Admin UI không phải core nghiệp vụ.Admin UI phải gọi API/service.Admin UI không được hardcode logic lõi.Admin UI không hardcode tài khoản ngân hàng.Admin UI không tự đổi PAID nếu core chưa cho phép.Admin UI phải có permission guard.Admin UI phải có audit.Admin UI phải có empty/error state.Admin UI phải có screenshot evidence.


## 169. HANDOFF RULE CHO OPERATIONAL CORE

Handoff Operational Core phải nói rõ:

QC_PASS không đồng nghĩa RELEASED.Raw lot QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.Material issue là điểm trừ kho nguyên liệu.Material receipt không trừ kho lần hai.Warehouse receipt chỉ sau batch release.Inventory ledger append-only.Public trace whitelist-only.Recall có snapshot.MISA đi qua integration layer.Commerce chỉ bán sellable availability hợp lệ.


## Final Global Done Gate


## 170. FINAL GLOBAL DONE GATE CHO TOÀN HỆ

Toàn hệ chỉ được xem là Done khi:

[ ] MASTER-00 Done.[ ] Tất cả pack chi tiết Done.[ ] Tất cả Source-of-Truth được xác nhận.[ ] Tất cả dependency trọng yếu pass.[ ] Tất cả runtime contract pass.[ ] Tất cả API/DTO/DB/UI/worker cần thiết pass.[ ] Security / permission / audit pass.[ ] All P0 smoke pass.[ ] All P0 evidence accepted.[ ] Monitoring pass.[ ] Rollback pass.[ ] Completion Report accepted.[ ] Owner Sign-off complete.[ ] Release approved.[ ] Go-live approved.

Tại thời điểm này, toàn hệ chưa đạt Final Global Done Gate.


## 171. FINAL GLOBAL điểm chặn

Các điểm chặn còn phải giữ ở cấp MASTER cho đến khi có evidence:

Không được ghi ngược lại nếu chưa có:

Completion Report accepted.

P0 smoke pass.

Evidence accepted.

Monitoring pass.

Rollback pass.

Owner sign-off.


## 172. FINAL GLOBAL RELEASE CONDITION

Toàn hệ chỉ được chuyển sang release khi tất cả điều kiện sau pass:

Nếu một gate fail:


## 173. FINAL GO-LIVE CONDITION

Go-live chỉ được phép khi:

Khi chưa đủ, trạng thái bắt buộc:


## Final Master Conclusion


## 174. KẾT LUẬN MASTER-00

MASTER-00 đã xác định khung quản trị cấp cao cho hệ thống Ginsengfood.

MASTER-00 khóa các điểm nền tảng:

Hệ thống Ginsengfood là hệ thống kỹ thuật đa hợp phần.MASTER-00 là governance document cấp cao nhất.MASTER-00 không thay thế pack chi tiết.Mọi pack phải tuân thủ Source-of-Truth, boundary, dependency, evidence và gate.AI / Gateway / CRM không được tự quyết định dữ liệu commerce.Commerce Core sở hữu quote, order, payment, shipping.Operational Core sở hữu sản xuất, release, inventory, trace, recall.Security / Audit là yêu cầu xuyên suốt.Evidence là điều kiện bắt buộc.Smoke là điều kiện bắt buộc.Owner sign-off là điều kiện bắt buộc.


## 175. KẾT LUẬN RIÊNG VỀ PAYMENT

Payment được khóa theo chiến lược:

Direct bank transfer to SSAVIGROUP là phương án chủ động/dự phòng.

Direct bank transfer phải qua Company Bank Account Registry.

Bank transfer order phải có payment_reference / transfer memo.

Không hardcode tài khoản ngân hàng ở AI/Gateway/CRM/Admin UI/frontend/static template.

Không payment nào được PAID nếu thiếu Payment Core confirmation hoặc Accounting Review confirmation.

AI/Gateway/CRM không được tự hứa payment nếu Payment Core chưa trả eligibility.


## 176. KẾT LUẬN RIÊNG VỀ SHIPPING

Shipping được khóa theo phạm vi hiện tại:

Commerce Shipping Core quyết định shipping eligibility.

Shipping Core quyết định COD nếu có.

Shipping Core quyết định phí ship.

Shipping Core quyết định fallback ETA.

AI/Gateway/CRM không được tự hứa vận chuyển.

AI/Gateway/CRM không được tự hứa ETA.

Landing Page không được hardcode ETA/phí ship.

Nếu sau này có mở rộng:


## 177. KẾT LUẬN RIÊNG VỀ IVR

PACK-09 được ghi nhận là hợp phần chính thức:

IVR không dùng cho:

Remarketing.

Mọi kết quả IVR phải đi qua Core Order State Machine.

Technical error không được tính là customer no-answer.


## 178. KẾT LUẬN RIÊNG VỀ GATEWAY

Gateway vẫn đang bị chặn.

Trạng thái:

Gateway chỉ được mở khi:

Public comment policy pass.

Messenger handoff pass.

Price intent routing pass.

No public final quote pass.

Payment promise guard pass.

Shipping promise guard pass.

Abuse/profanity handling pass.

Meta webhook evidence accepted.

Gateway smoke pass.

Completion Report evidence accepted.


## 179. KẾT LUẬN RIÊNG VỀ COMPLETION REPORT

Completion Report chưa được xem là hoàn tất.

Completion Report chỉ được chấp nhận khi:

Evidence registry đầy đủ.

Smoke matrix đầy đủ.

P0 evidence accepted.

Known điểm chặn được xử lý.

Retest nếu có đã hoàn tất.

Monitoring evidence có.

Rollback evidence có.

Owner sign-off đủ.


## 180. KẾT LUẬN RELEASE

Không được dùng MASTER-00 để tuyên bố hệ thống đã vận hành thật.

MASTER-00 chỉ là bản khóa governance để toàn bộ pack tiếp theo viết và triển khai đúng trật tự.


## 181. FINAL HANDOFF CONCLUSION

Từ MASTER-00, toàn hệ được phép đi tiếp theo hướng:

Viết pack chi tiết theo file sequence.Triển khai theo pack sequence.Gắn traceability ID cho requirement/rule/API/DTO/DB/UI/worker/test/evidence.Chạy smoke theo P0 matrix.Thu thập evidence theo pack.Rà security / permission / audit.Lập Completion Report.Review release.Xin owner sign-off.Chỉ go-live khi toàn bộ gate pass.

Không được đi theo hướng:

Code trước rồi mới tìm rule.UI tự hardcode logic.AI tự tính giá.Gateway tự hứa thanh toán.CRM tự hứa vận chuyển.IVR tự đổi order state.Admin UI tự đánh dấu PAID.Landing Page hardcode bank account.Completion Report pass khi thiếu evidence.Go-live khi smoke chưa pass.


## 182. FINAL MASTER LOCK

MASTER-00 khóa trạng thái cuối:


## 183. FINAL MASTER DONE GATE RESULT

Kết quả ở cấp MASTER:

Điều này có nghĩa:

MASTER-00 đã đủ vai trò chỉ mục và governance.

Các pack chi tiết vẫn phải được viết/kiểm tra riêng.

Hệ thống vẫn chưa được phép production.

Gateway vẫn chưa được phép mở.

Completion Report vẫn phải ghi thêm evidence.

Go-live chưa được phê duyệt.

HẾT PHẦN 4/4


## Hết Master-00 — Ginsengfood Master Technical Documentation Index
