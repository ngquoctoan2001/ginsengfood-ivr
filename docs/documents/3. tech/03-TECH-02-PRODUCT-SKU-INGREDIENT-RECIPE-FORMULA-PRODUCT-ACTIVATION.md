# TECH-02 - PRODUCT SKU INGREDIENT RECIPE FORMULA PRODUCT ACTIVATION

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

## PRODUCT MASTER / SKU MASTER / INGREDIENT MASTER / RECIPE VERSION / FORMULA GOVERNANCE / PRODUCT ACTIVATION GATE

## PHẦN 1/4 - PRODUCT DOMAIN PRINCIPLES / SOURCE-OF-TRUTH / PRODUCT-SKU-INGREDIENT-RECIPE BOUNDARY / ACTIVATION NO-BYPASS RULE

## 1. MỤC ĐÍCH CỦA TECH-02

TECH-02 là tài liệu kỹ thuật nền cho toàn bộ lớp sản phẩm của Ginsengfood.

TECH-02 khóa cách hệ thống quản lý:

Product Master.

SKU Master.

Ingredient Master.

Recipe.

Formula Version.

Formula Kind.

Product Activation.

Product Knowledge Runtime.

Product Scope Registry.

Sellable Gate Dependency.

Product-to-Commerce Dependency.

Product-to-Operational Dependency.

Product-to-AI/Live/Ads Dependency.

TECH-02 không viết code.
TECH-02 không thiết kế API chi tiết.
TECH-02 không thiết kế database chi tiết.
TECH-02 không thiết kế UI chi tiết.
TECH-02 không thay PACK-02.
TECH-02 không thay PACK-01 về tồn kho/sale lock/recall.
TECH-02 không thay Commerce Runtime về giá/quote/order/payment.
TECH-02 không thay PACK-05 về AI Advisor.

Vai trò đúng của TECH-02 là chuyển các rule sản phẩm đã khóa trong MASTER/PACK sang lớp technical implementation specification để dev biết phải thiết kế module sản phẩm theo boundary nào.

## 2. VAI TRÒ CỦA TECH-02 TRONG TOÀN HỆ GINSENGFOOD

## 2.1. TECH-02 là upstream của nhiều hệ thống

TECH-02 là upstream bắt buộc của:

Downstream

Phụ thuộc TECH-02

Operational Core

Cần SKU, ingredient, recipe, formula snapshot

Demand / MRP

Cần BOM, formula, ingredient, scaling rule

Commerce Runtime

Cần SKU active, product activation, product identity

AI Advisor

Cần Product Knowledge approved, public product name

Facebook Gateway

Cần product scope khi reply/handoff

Ads Measurement

Cần SKU/product mapping để đo doanh thu

MC AI Live

Cần Product Scope, script sản phẩm được cue

## IVR

Không trực tiếp tạo sản phẩm, nhưng order xác nhận phải gắn SKU hợp lệ

Public Trace

Cần product/SKU public identity

MISA / Accounting

Cần mapping SKU, nguyên liệu, thành phẩm nếu scope có

Nếu TECH-02 sai, các module downstream có thể sai hàng loạt:

AI tư vấn sai sản phẩm.

Commerce bán SKU chưa active.

MRP tính sai nguyên liệu.

Production dùng sai công thức.

Ads đo sai SKU.

Live cue sản phẩm chưa được phép.

Public Trace hiển thị sai thông tin.

Order/warehouse/accounting lệch mapping.

## 2.2. Product domain không phải chỉ là danh sách sản phẩm

Trong Ginsengfood, Product/SKU không phải chỉ là bảng tên sản phẩm.

Product domain phải kiểm soát:

Sản phẩm thương mại là gì.

SKU vận hành là gì.

SKU nào thuộc nhóm mùa/chức năng/bổ dưỡng nếu được khóa trong master.

Nguyên liệu nào được dùng.

Công thức nào được dùng.

Công thức thuộc formula kind nào.

Phiên bản công thức nào đang active.

Công thức nào dùng cho pilot.

Công thức nào dùng cho sản xuất.

SKU nào được active.

SKU nào chưa được bán.

SKU nào có product knowledge được AI dùng.

SKU nào được live cue.

SKU nào được đo Ads.

SKU nào được mở quote/order.

SKU nào bị chặn bởi sellable/sale lock/recall/stock/price.

## 3. TECH-02 SOURCE-OF-TRUTH

## 3.1. Nguồn chính của TECH-02

TECH-02 dựa trên các nguồn:

MASTER governance đã khóa.

PACK-02 - Product Master / SKU / Ingredient / Recipe / Formula Version / Product Activation.

PACK-01 - Operational Core / Warehouse / Inventory / Traceability / Recall / Sale Lock.

PACK-03 - Demand / MRP / Formula Scaling / Procurement / Material & Packaging Control.

Commerce Runtime - Quote / Cart / Order / Payment / Shipping.

PACK-05 - AI Advisor / Product Knowledge / Claim Guard.

PACK-08 - MC AI Live / Product Scope / Script Registry.

PACK-10 - Completion / Evidence / Gateway / Release Readiness.

## DEV EXECUTION COMMAND PACK.

## TECH-00 - Technical Implementation Master Plan.

## TECH-01 - Foundation / RBAC / Audit / Idempotency / Evidence Registry.

Owner decision mới nếu phát sinh thay đổi.

## 3.2. Nguồn không được dùng làm sự thật sản phẩm

Không được dùng các nguồn sau làm product source-of-truth:

Tài liệu cũ của kỹ sư trước.

Code cũ.

Product seed cũ chưa owner xác nhận.

UI cũ.

File Excel cũ chưa khóa.

Nội dung quảng cáo cũ.

Prompt AI cũ.

Nội dung live cũ.

Dashboard cũ.

Cách gọi tên sản phẩm tự phát.

Tên SKU trong comment/chat chưa được master hóa.

Bảng giá không đi qua Commerce Runtime.

Nếu có khác biệt giữa legacy và tài liệu mới:

NEW SOURCE-OF-TRUTH WINS
LEGACY_REFERENCE_ONLY
OWNER_REVIEW_REQUIRED nếu còn nghi ngờ

## 4. PRODUCT DOMAIN CORE MODULES

TECH-02 quản trị 8 nhóm module chính.

## 4.1. Product Master Module

Product Master quản lý danh tính sản phẩm ở cấp thương mại.

Product Master trả lời:

Sản phẩm tên gì.

Tên public dùng cho khách là gì.

Sản phẩm thuộc nhóm nào.

Sản phẩm có mô tả gì.

Sản phẩm có trạng thái gì.

Sản phẩm có được phép xuất hiện trong AI/Commerce/Live/Ads không.

Sản phẩm có liên kết SKU nào.

Sản phẩm có product knowledge approved chưa.

Sản phẩm có được đưa vào release scope không.

Product Master không tự quyết tồn kho.
Product Master không tự quyết giá.
Product Master không tự quyết order.
Product Master không tự quyết batch release.
Product Master không tự quyết sale lock/recall.

## 4.2. SKU Master Module

SKU Master quản lý đơn vị bán/vận hành cụ thể.

SKU Master trả lời:

SKU code nội bộ là gì.

SKU public name là gì.

SKU thuộc product nào.

SKU thuộc nhóm sản phẩm nào.

SKU có được active không.

SKU dùng formula kind nào.

SKU dùng recipe/formula version nào.

SKU có packaging/trade item nào.

SKU có được quote/order không.

SKU có được AI tư vấn không.

SKU có được MC Live cue không.

SKU có được Ads đo không.

SKU Master không được dùng trực tiếp với khách nếu tên đó là mã nội bộ.
Khách chỉ được thấy public product name/public SKU name đã approved.

## 4.3. Ingredient Master Module

Ingredient Master quản lý nguyên liệu và vật tư tham gia sản xuất.

Ingredient Master trả lời:

Nguyên liệu/vật tư tên gì.

Mã nguyên liệu là gì.

Nhóm nguyên liệu là gì.

Nguyên liệu có lot-managed không.

Nguyên liệu là purchased hay company source nếu rule liên quan.

Nguyên liệu dùng trong recipe nào.

Nguyên liệu có yêu cầu QC không.

Nguyên liệu có yêu cầu supplier/source không.

Nguyên liệu có thuộc nhóm đặc thù hoặc dùng chung không.

Nguyên liệu có được MRP/procurement xét không.

Ingredient Master không tự quyết mua hàng.
MRP/Procurement do PACK-03 kiểm soát.
Raw lot readiness do Operational Core kiểm soát.

## 4.4. Recipe Module

Recipe quản lý công thức sản phẩm.

Recipe trả lời:

SKU nào dùng công thức nào.

Formula kind là gì.

Version là gì.

Thành phần gồm những ingredient nào.

Nhóm recipe line là gì.

Quantity/UOM/usage basis/prep note là gì nếu scope khóa.

Công thức có active không.

Công thức có được dùng cho production order không.

Công thức có cần snapshot không.

Công thức có bị khóa lịch sử không.

Recipe không được bị ghi đè lịch sử.
Production Order phải dùng snapshot, không phụ thuộc live recipe thay đổi sau đó.

## 4.5. Formula Version Module

Formula Version quản lý phiên bản công thức.

Mục tiêu:

Không ghi đè công thức cũ.

Cho phép coexist nhiều phiên bản nếu owner khóa.

Cho phép G1/G2 hoặc future formula version tồn tại có kiểm soát.

Biết phiên bản nào active.

Biết phiên bản nào dùng cho pilot.

Biết phiên bản nào dùng cho production.

Biết version nào đã retired.

Biết order/batch nào đã dùng version nào.

Formula Version là yêu cầu bắt buộc để truy xuất lịch sử sản xuất chính xác.

## 4.6. Product Activation Module

Product Activation quản lý trạng thái sản phẩm/SKU được kích hoạt.

Product Activation trả lời:

Product đã đủ thông tin chưa.

SKU đã đủ thông tin chưa.

Recipe/formula đã active chưa.

Product knowledge đã approved chưa.

Product có được hiển thị nội bộ không.

Product có được đưa vào AI không.

Product có được đưa vào Commerce không.

Product có được đưa vào Live không.

Product có được đưa vào Ads không.

Product Activation không đồng nghĩa Sellable.

Đây là điểm khóa rất quan trọng.

## 4.7. Product Knowledge Runtime Module

Product Knowledge Runtime cung cấp dữ liệu sản phẩm cho AI Advisor, Live script, Landing Page, Website, Messenger hoặc nội dung tư vấn nếu scope có.

Product Knowledge Runtime phải quản lý:

Public product name.

Public-safe description.

Claim whitelist.

Ingredient public description.

Usage guidance nếu approved.

Dietary flags nếu có.

Seasonal/functional/nourishing grouping nếu approved.

Public-safe benefit language.

Product positioning.

Product replacement relationship nếu có.

Content version.

Approval status.

AI không được tự viết knowledge ngoài nguồn approved.

## 4.8. Product Scope Registry Module

Product Scope Registry xác định sản phẩm nào được phép dùng trong từng bối cảnh:

Commerce.

AI Advisor.

Facebook Messenger.

Live.

Ads.

Landing Page.

CRM nếu có.

IVR nếu order confirmation cần tên sản phẩm.

Public Trace.

Ví dụ:

SKU active nhưng chưa nằm trong Live Product Scope thì MC AI Live không được cue.

SKU active nhưng chưa nằm trong AI Product Scope thì AI không được chủ động tư vấn.

SKU active nhưng bị sale lock thì Commerce/AI/Ads/Live phải dừng bán theo rule upstream.

## 5. PRODUCT / SKU / INGREDIENT / RECIPE BOUNDARY

## 5.1. Product khác SKU

Product là danh tính thương mại/cấp sản phẩm.
SKU là đơn vị vận hành/bán hàng cụ thể.

Một product có thể có một hoặc nhiều SKU tùy mô hình.
Một SKU phải thuộc product rõ ràng.

Không được trộn lẫn Product và SKU khiến:

AI gọi sai tên.

Commerce quote sai đơn vị.

Ads đo sai doanh thu.

Warehouse nhập sai SKU.

MISA mapping sai.

Public Trace hiển thị sai.

## 5.2. SKU khác Trade Item

SKU không đồng nghĩa Trade Item/GTIN/Barcode.

Trade Item là đơn vị thương mại có thể gắn mã vạch/GTIN ở cấp hộp/thùng nếu scope có.

Nguyên tắc:

SKU là sản phẩm bán/vận hành.

Trade Item là đơn vị đóng gói/thương mại cụ thể.

GTIN/GS1 nếu có phải quản lý riêng.

Không dùng barcode thay SKU master.

Không tạo barcode thương mại song song tùy tiện.

PACKET/BOX/CARTON nếu scope có phải theo packaging boundary.

Chi tiết packaging/GTIN sẽ đi sâu ở TECH-03/TECH-04 hoặc tài liệu packaging tương ứng, nhưng TECH-02 phải khóa ranh giới.

## 5.3. Ingredient khác Raw Lot

Ingredient là danh mục nguyên liệu.
Raw Lot là lô nguyên liệu cụ thể đã tiếp nhận/QC/ready trong Operational Core.

Ingredient Master không tự tạo Raw Lot.
Raw Lot không được tạo tùy tiện ngoài quy trình intake/QC/readiness.

Ví dụ:

HRB_SAM_SAVIGIN là ingredient.

Một lô Sâm Savigin thu hoạch/nguyên liệu cụ thể là raw lot.

Ingredient dùng trong recipe.

Raw lot dùng trong material issue/production.

## 5.4. Recipe khác Production Snapshot

Recipe là công thức master/version.
Production Snapshot là bản chụp công thức tại thời điểm tạo lệnh sản xuất.

Không được để production order đọc live recipe rồi thay đổi lịch sử khi recipe cập nhật.

Nguyên tắc:

Recipe thay đổi không được làm thay đổi production order/batch đã tạo trước đó.

## 6. PRODUCT ACTIVATION KHÁC SELLABLE

## 6.1. Product Active không đồng nghĩa được bán

Đây là rule P0.

Product/SKU Active chỉ có nghĩa sản phẩm đã đủ điều kiện master để được hệ thống xem xét trong các flow tương ứng.

Sellable cần thêm các điều kiện khác, ví dụ:

SKU active.

Product active.

Recipe/formula active nếu required.

Price/listed price active nếu bán.

Batch/finished goods đã RELEASED nếu cần tồn thật.

Warehouse receipt confirmed nếu cần stock.

Stock available > 0 nếu scope yêu cầu.

Không quality hold.

Không recall hold.

Không sale lock.

Không channel hold.

Không program/sellable policy chặn.

Commerce Runtime cho phép quote/order.

Vì vậy:

## ACTIVE != SELLABLE

## 6.2. Product Activation là upstream của Sellable, không thay Sellable

Product Activation cung cấp điều kiện ban đầu.
Sellable Gate nằm ở giao điểm Product + Operational + Commerce.

Nếu Product active nhưng:

chưa có hàng;

batch chưa release;

đang recall;

đang sale lock;

chưa có giá;

chưa mở kênh bán;

thì vẫn không được bán.

## 6.3. AI / Facebook / Live / Ads phải tôn trọng Sellable

Downstream bắt buộc:

AI không được chốt SKU không sellable.

Facebook Gateway không được routing order SKU không sellable.

MC AI Live không được cue bán SKU không sellable.

Ads không được scale SKU không sellable.

Commerce không được quote/order SKU không sellable.

CRM không được upsell SKU không sellable.

Landing Page không được mở mua SKU không sellable nếu scope có.

## 7. FORMULA GOVERNANCE PRINCIPLE

## 7.1. G1 / G2 / future formula phải có governance

Hệ thống phải hỗ trợ công thức theo version/kind có kiểm soát.

Các nguyên tắc:

Công thức không được ghi đè lịch sử.

Formula kind phải rõ.

Formula version phải rõ.

Active version phải rõ.

Production Order phải snapshot formula.

Batch phải truy ngược được formula version đã dùng.

Future formula không phá dữ liệu cũ.

Pilot formula không bị dùng nhầm production nếu chưa active đúng scope.

Production formula không bị chỉnh sau khi batch đã tạo.

Owner approval cần thiết khi activate hoặc thay đổi formula production.

## 7.2. G1 là baseline go-live nếu đã khóa trong PACK

Theo các pack đã khóa trước đó, G1 là baseline cho go-live/pilot/production tùy phạm vi đã xác định trong tài liệu nguồn. TECH-02 không tự đổi lại logic này.

Nếu sau này cần G2/G3:

Tạo version mới.

Không ghi đè G1.

Có owner approval.

Có test/smoke/evidence.

Có effectivity rule.

Có production snapshot.

Có rollback/retire rule nếu cần.

## 8. RECIPE LINE GROUP PRINCIPLE

## 8.1. Recipe line phải theo nhóm đã khóa

Recipe line không được nhập tự do không nhóm.

Theo các rule đã khóa trong pack, recipe phải tôn trọng nhóm công thức được chuẩn hóa. Ở tầng TECH-02, dev phải thiết kế để recipe line có group rõ ràng, phục vụ:

Kiểm tra completeness.

Formula validation.

## MRP.

Production snapshot.

Traceability.

Costing/MISA nếu scope có.

AI/product knowledge nếu cần diễn giải public-safe.

## 8.2. Recipe group không dùng làm claim public trực tiếp

Recipe group là cấu trúc kỹ thuật/vận hành.

Không được lấy recipe group nội bộ để nói trực tiếp với khách nếu chưa được Product Knowledge / Claim Guard public hóa.

Ví dụ:

Ingredient nội bộ có thể dùng cho công thức.

Public product knowledge phải dùng ngôn ngữ an toàn, dễ hiểu, không lộ công thức chi tiết nếu không được phép.

## 9. PRODUCT KNOWLEDGE PRINCIPLE

## 9.1. Product Knowledge là nguồn cho AI và nội dung

Product Knowledge phải được xem là nguồn approved cho:

AI Advisor.

Website/Landing Page nếu scope có.

Facebook Messenger.

Live script.

CRM nếu scope có.

Public product description.

Không để AI tự bịa sản phẩm.

## 9.2. Product Knowledge phải phân public/internal

Product Knowledge cần có ít nhất hai lớp:

Layer

Nội dung

Internal Knowledge

Dữ liệu nội bộ, technical, formula, operation, claim source

Public Knowledge

Nội dung được phép nói với khách

Public Knowledge phải qua claim guard nếu có yếu tố sức khỏe/khoa học.

Không public:

Công thức chi tiết nếu không được phép.

Tỷ lệ nội bộ nếu policy không public.

Costing.

Supplier detail.

QC defect/loss.

MISA/accounting.

Personnel/internal approval.

Internal SKU nếu khách không cần biết.

## 10. PRODUCT SCOPE PRINCIPLE

## 10.1. Product Scope theo kênh

Không phải sản phẩm active là được dùng ở mọi kênh.

Cần Product Scope cho:

Commerce.

AI Advisor.

Facebook.

Live.

Ads.

Landing Page.

## CRM.

Public Trace.

IVR order confirmation.

Ví dụ:

SKU có thể active nội bộ nhưng chưa mở bán.

SKU có thể mở bán Messenger nhưng chưa đưa vào Live.

SKU có thể dùng trong Ads measurement nhưng chưa scale.

SKU có thể public trace nhưng không hiển thị đầy đủ knowledge.

## 10.2. Product Scope phải có owner decision khi production

Khi đưa sản phẩm vào production scope:

Sản phẩm phải active.

Sellable condition phải clear nếu bán.

Product knowledge phải approved nếu AI/Live/Web dùng.

Price/program phải clear nếu Commerce dùng.

Channel scope phải clear nếu Facebook/Live/Ads dùng.

Owner decision phải có nếu scope rủi ro.

## 11. PRODUCT MODULE BOUNDARY

## 11.1. TECH-02 được phép làm

TECH-02 cho phép module Product thực hiện:

Quản lý product.

Quản lý SKU.

Quản lý ingredient master.

Quản lý recipe.

Quản lý formula version.

Quản lý product activation.

Quản lý product knowledge approved source.

Quản lý product scope registry.

Cung cấp product/SKU/recipe data cho downstream.

Cung cấp dependency cho Operational/Commerce/AI/Ads/Live.

## 11.2. TECH-02 không được làm

TECH-02 không được:

Quyết định tồn kho thật.

Quyết định batch release.

Quyết định warehouse receipt.

Quyết định sale lock/recall.

Quyết định giá cuối cùng.

Quyết định quote/order.

Quyết định payment.

Quyết định verified revenue.

Quyết định Ads scale.

Quyết định IVR result/order status.

Quyết định claim public nếu chưa qua Claim Guard.

Quyết định production ready cho downstream.

## 12. PRODUCT P0 RULES

Các rule sau là P0:

SKU chưa active vẫn bán được.

Product active bị hiểu nhầm là sellable.

SKU không có recipe/formula required vẫn production.

Recipe version ghi đè lịch sử.

Production order dùng live recipe thay vì snapshot.

Formula pilot bị dùng nhầm production.

Product knowledge chưa approved nhưng AI dùng để tư vấn.

AI dùng tên SKU nội bộ với khách.

Live cue sản phẩm ngoài Product Scope.

Ads scale sản phẩm không sellable.

Commerce quote SKU không active/sellable.

Product/SKU bị recall/sale lock vẫn được bán.

Ingredient master sai làm MRP/production dùng sai nguyên liệu.

Public product knowledge lộ công thức/nội bộ không được phép.

Product scope production không có owner decision.

Nếu có P0:

## 13. TECH-02 DEPENDENCY MAP

## 13.1. Upstream của TECH-02

TECH-02 phụ thuộc:

## TECH-00 - Technical Implementation Master Plan.

## TECH-01 - Foundation/RBAC/Audit/Evidence.

PACK-02 - Product/SKU/Recipe source-of-truth.

PACK-01 - Sellable/Sale Lock/Recall dependency.

PACK-03 - Formula Scaling/MRP dependency.

Commerce Runtime dependency.

PACK-05 - AI Product Knowledge dependency.

PACK-08 - Live Product Scope dependency.

## 13.2. Downstream của TECH-02

TECH-02 là upstream cho:

## TECH-03 - Operational Core.

## TECH-04 - Commerce Runtime.

## TECH-05 - AI Advisor.

## TECH-06 - Facebook Gateway.

## TECH-07 - Ads Measurement.

## TECH-08 - MC AI Live.

## TECH-09 - IVR.

## TECH-10 - Evidence/Release Handoff.

Nếu TECH-02 chưa clear, downstream production scope liên quan sản phẩm phải bị chặn.

## 14. TECH-02 IMPLEMENTATION READINESS

## 14.1. Điều kiện được bắt đầu triển khai TECH-02

Dev chỉ được bắt đầu triển khai TECH-02 khi có:

TECH-00 complete.

TECH-01 complete ở mức tài liệu.

Product source-of-truth rõ.

Product/SKU baseline rõ.

Ingredient baseline rõ.

Recipe/formula baseline rõ.

Product activation rule rõ.

Product knowledge rule rõ.

Product scope rule rõ.

Evidence/smoke plan cho Product domain.

Owner cho phép bắt đầu Wave 1.

## 14.2. TECH-02 chưa production ready

Ngay cả khi TECH-02 tài liệu hoàn tất, trạng thái vẫn là:

## 15. TECH-02 PHẦN 1 DONE GATE

## PHẦN 1/4 được xem là hoàn tất khi đã khóa:

Mục đích TECH-02.

Vai trò TECH-02 trong toàn hệ.

Source-of-truth.

Product core modules.

Product/SKU/Ingredient/Recipe boundary.

Product Activation khác Sellable.

Formula governance.

Recipe line group principle.

Product Knowledge principle.

Product Scope principle.

Product module boundary.

Product P0 rules.

Dependency map.

Implementation readiness.

## 16. TECH-02 PHẦN 1 FAIL GATE

## PHẦN 1/4 fail nếu đội kỹ thuật:

Xem Product/SKU chỉ là danh sách tên sản phẩm.

Trộn Product và SKU.

Trộn SKU và Trade Item.

Trộn Ingredient và Raw Lot.

Trộn Recipe và Production Snapshot.

Xem Product Active là Sellable.

Cho SKU chưa active vào Commerce quote/order.

Cho AI dùng product knowledge chưa approved.

Cho Live cue sản phẩm ngoài scope.

Cho Ads scale sản phẩm không sellable.

Ghi đè recipe version.

Không có formula governance.

Không có product scope theo kênh.

Dùng legacy product list làm source-of-truth.

Gọi TECH-02 documentation complete là production ready.

Nếu có lỗi:

## 17. KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 của TECH-02 đã khóa nguyên tắc nền cho Product / SKU / Ingredient / Recipe / Formula / Product Activation.

Kết luận bắt buộc:

Product domain là upstream của Operational, Commerce, AI, Facebook, Ads, Live và IVR.

Product không chỉ là danh sách sản phẩm.

Product khác SKU.

SKU khác Trade Item.

Ingredient khác Raw Lot.

Recipe khác Production Snapshot.

Product Active không đồng nghĩa Sellable.

Recipe/formula phải có version governance.

Formula không được ghi đè lịch sử.

Production Order phải dùng snapshot công thức.

Product Knowledge phải là nguồn approved cho AI/Live/Web.

Product Scope phải theo kênh và theo release scope.

Commerce không được quote/order SKU chưa active/sellable.

AI không được tư vấn/chốt SKU chưa approved hoặc không sellable.

Ads/Live không được dùng sản phẩm ngoài scope.

Documentation Complete không phải Production Ready.

Trạng thái sau PHẦN 1/4:

## PHẦN 2/4 - PRODUCT MASTER / SKU MASTER / INGREDIENT MASTER / RECIPE / FORMULA VERSION / PRODUCT KNOWLEDGE / PRODUCT SCOPE MODULE CONTRACT

## 18. MỤC ĐÍCH CỦA PHẦN 2/4

## PHẦN 2/4 khóa Module Contract cho các module sản phẩm nền của Ginsengfood.

Nếu PHẦN 1/4 đã khóa nguyên tắc:

Product Active không đồng nghĩa Sellable.
Recipe không được ghi đè lịch sử.
Product Knowledge phải approved trước khi AI dùng.
Product Scope phải rõ trước khi Commerce / AI / Ads / Live dùng.

Thì PHẦN 2/4 xác định rõ từng module phải làm gì, không được làm gì, phụ thuộc gì, cung cấp gì cho downstream và các P0 điểm chặn nào phải chặn release.

Các module được khóa trong PHẦN 2/4 gồm:

Product Master Module.

SKU Master Module.

Ingredient Master Module.

Recipe Module.

Formula Version Module.

Product Activation Module.

Product Knowledge Runtime Module.

Product Scope Registry Module.

Product Expansion / Future SKU Governance.

## 19. PRODUCT MASTER MODULE CONTRACT

## 19.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W1-PRODUCT-MASTER-001

Module Name

Product Master Module

Source Pack

## PACK-02

Wave

Wave 1

Application Group

Backend Core / Admin Web

Upstream

Foundation / RBAC / Audit

Downstream

SKU, Commerce, AI, Live, Ads, Public Trace

Default Status

## WAITING_IMPLEMENTATION

Production Status

KHONG

## 19.2. Module Purpose

Product Master Module quản lý danh tính sản phẩm ở cấp thương mại.

Module này trả lời:

Sản phẩm là gì.

Tên sản phẩm public là gì.

Sản phẩm thuộc nhóm nào.

Sản phẩm có mô tả public/internal nào.

Sản phẩm có được quản lý trong hệ thống không.

Sản phẩm có liên kết SKU nào.

Sản phẩm có product knowledge approved chưa.

Sản phẩm có được đưa vào Commerce / AI / Live / Ads không.

Sản phẩm có còn hiệu lực không.

Sản phẩm có nằm trong release scope nào không.

## 19.3. Scope In

Product Master được phép:

Tạo danh mục product.

Cập nhật thông tin product.

Quản lý public name.

Quản lý internal name nếu cần.

Quản lý product group/category.

Quản lý product lifecycle.

Liên kết product với SKU.

Liên kết product với Product Knowledge.

Liên kết product với Product Scope.

Ghi audit thay đổi product.

Ghi trạng thái product cho downstream đọc.

## 19.4. Scope Out / Must Not Do

Product Master không được:

Quyết định SKU sellable.

Quyết định tồn kho.

Quyết định batch release.

Quyết định sale lock/recall.

Quyết định giá bán.

Quyết định quote/order.

Quyết định payment.

Quyết định verified revenue.

Quyết định Ads scale.

Quyết định IVR order status.

Tự public claim sức khỏe chưa được duyệt.

Ghi đè recipe/formula history.

## 19.5. Product Status Model

Product nên có các trạng thái nền:

Product Status

Ý nghĩa

## DRAFT

Đang nháp

## INTERNAL_REVIEW

Đang review nội bộ

## APPROVED

Được duyệt master

## ACTIVE

Được kích hoạt trong master

## INACTIVE

Không còn active

## RETIRED

Ngừng sử dụng

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

bị chặn

Bị chặn do thiếu dữ liệu/P0

Lưu ý:

Product ACTIVE chưa phải SELLABLE.

## 19.6. Product Master P0 điểm chặn

Các lỗi sau là P0:

Product không có public name approved nhưng AI/Live dùng với khách.

Product inactive vẫn được đưa vào Commerce.

Product retired vẫn được Ads scale.

Product thiếu SKU mapping nhưng vẫn bán.

Product bị owner review required nhưng vẫn release.

Product dùng mô tả public chưa qua claim/safety nếu có yếu tố sức khỏe.

Product legacy chưa xác nhận nhưng được dùng làm source-of-truth.

Product status bị sửa không audit.

## 20. SKU MASTER MODULE CONTRACT

## 20.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W1-SKU-MASTER-001

Module Name

SKU Master Module

Source Pack

## PACK-02

Wave

Wave 1

Application Group

Backend Core / Admin Web

Upstream

Product Master, Foundation

Downstream

Recipe, Operational, Commerce, AI, Ads, Live, MISA if scope

Default Status

## WAITING_IMPLEMENTATION

Production Status

KHONG

## 20.2. Module Purpose

SKU Master quản lý đơn vị sản phẩm cụ thể để vận hành, sản xuất, bán hàng, đo lường và truy xuất.

SKU Master trả lời:

SKU thuộc product nào.

SKU có mã nội bộ nào.

SKU có tên public nào.

SKU thuộc nhóm sản phẩm nào.

SKU có active không.

SKU dùng formula kind nào.

SKU liên kết recipe/formula version nào.

SKU có package/trade item nào nếu scope có.

SKU có được đưa vào Commerce không.

SKU có được AI tư vấn không.

SKU có được Live cue không.

SKU có được Ads measurement không.

## 20.3. 20 SKU Baseline và khả năng mở rộng

TECH-02 khóa nguyên tắc:

20 SKU là baseline go-live nếu đã được pack/master khóa.

20 SKU không phải giới hạn vĩnh viễn.

Hệ thống phải chừa khả năng mở rộng lên 40-50 SKU hoặc nhiều hơn.

Thêm SKU mới phải đi qua Product/SKU/Recipe/Product Knowledge/Product Scope/Activation/Evidence/Owner Decision.

Không được thêm SKU mới bằng cách hardcode vào AI, UI, Ads hoặc Live script.

SKU mới chưa qua activation và scope thì downstream không được dùng production.

## 20.4. SKU Status Model

SKU Status

Ý nghĩa

## DRAFT

Nháp

## MASTER_READY

Dữ liệu master đủ

## RECIPE_WAITING

Chưa đủ recipe/formula

## KNOWLEDGE_WAITING

Chưa có product knowledge approved nếu AI/Live cần

## ACTIVE

Được kích hoạt master

## INACTIVE

Không active

## RETIRED

Ngừng sử dụng

bị chặn

Bị chặn

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

Lưu ý:

SKU ACTIVE chưa phải SELLABLE.

## 20.5. Scope In

SKU Master được phép:

Tạo SKU.

Cập nhật SKU.

Gắn SKU với product.

Gắn SKU với recipe/formula.

Gắn SKU với package/trade item nếu scope có.

Gắn SKU với Product Knowledge.

Gắn SKU với Product Scope.

Quản lý SKU lifecycle.

Cung cấp SKU identity cho downstream.

Audit mọi thay đổi SKU quan trọng.

## 20.6. Scope Out / Must Not Do

SKU Master không được:

Tự quyết sellable.

Tự quyết stock available.

Tự quyết price.

Tự quyết order.

Tự quyết payment.

Tự quyết Ads scale.

Tự quyết batch release.

Tự quyết recall/sale lock.

Tự quyết public claim.

Cho khách thấy mã SKU nội bộ nếu chưa public approved.

## 20.7. SKU P0 điểm chặn

Các lỗi sau là P0:

SKU chưa active vẫn quote/order được.

SKU active bị hiểu là sellable.

SKU không có product mapping.

SKU không có recipe/formula required nhưng production được.

SKU bị retired vẫn xuất hiện ở AI/Commerce/Live.

SKU bị sale lock/recall vẫn được bán.

SKU public name chưa approved nhưng AI dùng với khách.

SKU mới được thêm mà không có owner decision.

SKU mapping MISA/Accounting nếu scope cần nhưng chưa clear vẫn dùng production.

SKU status thay đổi không audit.

## 21. INGREDIENT MASTER MODULE CONTRACT

## 21.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W1-INGREDIENT-MASTER-001

Module Name

Ingredient Master Module

Source Pack

## PACK-02 / PACK-03

Wave

Wave 1

Application Group

Backend Core / Admin Web

Upstream

Foundation

Downstream

Recipe, MRP, Operational, Procurement, Traceability

Default Status

## WAITING_IMPLEMENTATION

Production Status

KHONG

## 21.2. Module Purpose

Ingredient Master quản lý danh mục nguyên liệu/vật tư phục vụ công thức, MRP, thu mua, nhập nguyên liệu, sản xuất và traceability.

Ingredient Master trả lời:

Nguyên liệu là gì.

Mã nguyên liệu là gì.

Nguyên liệu thuộc nhóm nào.

Nguyên liệu có lot-managed không.

Nguyên liệu là purchased hay company source nếu rule yêu cầu.

Nguyên liệu có supplier/source mapping không.

Nguyên liệu dùng trong recipe nào.

Nguyên liệu có QC requirement không.

Nguyên liệu có procurement threshold không.

Nguyên liệu có strategic reserve policy không.

## 21.3. Ingredient Special Locks

Các ingredient đã được khóa trong bộ tài liệu nền phải được xem là ingredient quan trọng, không được gộp sai:

HRB_SAM_SAVIGIN - Sâm Savigin / Company Source / strategic reserve policy nếu áp dụng.

ING_MI_CHINH - nguyên liệu riêng, không tự thay thế/gộp nếu đã khóa.

ING_THIT_HEO_NAC - nguyên liệu riêng, lot-managed, không gộp với nhóm thịt khác nếu đã khóa.

Các mã trên là ví dụ khóa theo pack/memory đã chốt; dev không được tự đổi tên/mã nếu không có owner decision.

## 21.4. Ingredient khác Raw Lot

Ingredient Master không phải Raw Lot.

Khái niệm

Ý nghĩa

Ingredient

Danh mục nguyên liệu

Raw Lot

Lô nguyên liệu cụ thể đã intake/QC/readiness

Supplier Mapping

Nhà cung cấp được phép cung cấp ingredient

Source Origin

Nguồn/vùng trồng nếu company source/self-grown

Inventory Balance

Tồn khả dụng theo lot/kho

Không được dùng Ingredient Master để thay quy trình Raw Intake / QC / Readiness.

## 21.5. Ingredient Status Model

Ingredient Status

Ý nghĩa

## DRAFT

Nháp

## ACTIVE

Được dùng trong recipe/MRP

## INACTIVE

Không dùng

bị chặn

Bị chặn

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## RETIRED

Ngừng sử dụng

## 21.6. Scope In

Ingredient Master được phép:

Tạo ingredient.

Cập nhật ingredient.

Phân nhóm ingredient.

Quản lý UOM nền.

Gắn lot-managed flag.

Gắn purchased/company source flag nếu scope có.

Gắn supplier/source dependency.

Gắn QC requirement.

Gắn MRP/procurement policy reference.

Cung cấp ingredient cho recipe/MRP/Operational.

## 21.7. Scope Out / Must Not Do

Ingredient Master không được:

Tự tạo raw lot.

Tự mark raw lot ready.

Tự quyết mua hàng.

Tự quyết harvest.

Tự quyết inventory balance.

Tự issue material.

Tự substitute ingredient khi recipe không cho.

Tự bỏ QC requirement.

Tự expose supplier/costing public.

Tự sửa ingredient lịch sử làm sai recipe/batch cũ.

## 21.8. Ingredient P0 điểm chặn

Các lỗi sau là P0:

Ingredient bị gộp sai làm recipe sai.

Ingredient lot-managed nhưng vận hành như non-lot.

Company source Sâm Savigin bị xử lý như purchased commodity nếu trái rule.

Ingredient inactive vẫn dùng trong recipe active.

Ingredient thiếu UOM/usage basis nhưng MRP/recipe dùng.

Ingredient supplier/source required nhưng bị bỏ qua.

Ingredient QC required nhưng intake không kiểm.

Ingredient public leak supplier/costing/internal note.

Ingredient status thay đổi không audit.

Ingredient bị thay thế trong recipe mà không tạo formula version mới.

## 22. RECIPE MODULE CONTRACT

## 22.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W1-RECIPE-001

Module Name

Recipe Module

Source Pack

## PACK-02 / PACK-03

Wave

Wave 1

Application Group

Backend Core / Admin Web

Upstream

SKU Master, Ingredient Master, Foundation

Downstream

Formula Version, Operational, MRP, Production Snapshot

Default Status

## WAITING_IMPLEMENTATION

Production Status

KHONG

## 22.2. Module Purpose

Recipe Module quản lý công thức của SKU theo cấu trúc có version, có group, có ingredient line, có quantity/UOM và có governance.

Recipe trả lời:

SKU nào dùng recipe nào.

Recipe thuộc formula kind nào.

Recipe version nào.

Recipe gồm những ingredient nào.

Ingredient thuộc group nào.

Quantity/UOM là gì.

Prep/usage basis là gì nếu scope cần.

Recipe active hay không.

Recipe có được production dùng không.

Recipe có được MRP dùng không.

## 22.3. Recipe Line Group Lock

Recipe line phải tôn trọng đúng 4 group đã khóa:

## SPECIAL_SKU_COMPONENT

## NUTRITION_BASE

## BROTH_EXTRACT

## SEASONING_FLAVOR

Không được thêm group tùy tiện nếu chưa có owner decision.
Không được nhập line recipe không có group.
Không được đưa ingredient vào sai group nếu ảnh hưởng MRP/production/snapshot.

## 22.4. Recipe Structure Principle

Một recipe phải đủ các thành phần quản trị:

## SKU.

Formula kind.

Formula version.

Recipe line group.

Ingredient.

Quantity.

## UOM.

Usage basis.

Prep note nếu cần.

Active status.

Approval status.

Effective rule nếu có.

Audit history.

## 22.5. Recipe Status Model

Recipe Status

Ý nghĩa

## DRAFT

Nháp

## UNDER_REVIEW

Đang review

## APPROVED

Được duyệt

## ACTIVE

Được dùng theo scope

## RETIRED

Ngừng dùng

bị chặn

Bị chặn

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## 22.6. Scope In

Recipe Module được phép:

Tạo recipe draft.

Thêm recipe line.

Gắn ingredient vào recipe.

Gắn recipe line group.

Gắn quantity/UOM.

Gắn formula kind/version.

Gửi review/approve.

Activate recipe theo owner decision.

Retire recipe theo rule.

Cung cấp recipe cho Formula Version/MRP/Production Snapshot.

## 22.7. Scope Out / Must Not Do

Recipe Module không được:

Ghi đè recipe active đã dùng trong production lịch sử.

Cho production đọc live recipe mà không snapshot.

Cho SKU production khi recipe chưa active.

Cho recipe line không group.

Cho ingredient inactive vào recipe active.

Tự substitute ingredient.

Tự thay UOM không audit.

Public công thức chi tiết nếu chưa được phép.

Dùng recipe draft cho production.

Bỏ qua owner approval khi activate production recipe.

## 22.8. Recipe P0 điểm chặn

Các lỗi sau là P0:

Recipe active bị sửa trực tiếp làm thay đổi lịch sử.

Production order không snapshot recipe.

Recipe draft dùng cho production.

Recipe thiếu line group.

Recipe thiếu ingredient/quantity/UOM.

Recipe dùng ingredient inactive.

Recipe version không rõ.

G1/G2 formula kind bị nhầm.

Recipe group ngoài 4 group khóa nhưng không có owner decision.

Recipe public leak công thức chi tiết chưa được phép.

## 23. FORMULA VERSION MODULE CONTRACT

## 23.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W1-FORMULA-VERSION-001

Module Name

Formula Version Module

Source Pack

## PACK-02 / PACK-03

Wave

Wave 1

Application Group

Backend Core / Admin Web

Upstream

Recipe Module, Foundation

Downstream

MRP, Production Order, Batch Traceability

Default Status

## WAITING_IMPLEMENTATION

Production Status

KHONG

## 23.2. Module Purpose

Formula Version Module bảo đảm công thức có version, có formula kind, có lifecycle, có audit và không phá lịch sử.

Module này trả lời:

Formula kind là gì.

Version nào đang active.

Version nào dùng pilot.

Version nào dùng production.

Version nào retired.

Version nào dùng cho SKU nào.

Production order/batch nào đã dùng version nào.

Có cho phép coexist nhiều formula kind không.

Khi nào cần owner approval để activate.

Khi nào cần rollback/retire version.

## 23.3. Formula Kind Governance

Các formula kind phải được quản lý rõ.

Theo các rule đã khóa trong dự án:

G1 PILOT_PERCENT_BASED là baseline pilot/go-live theo phạm vi đã chốt.

G2 FIXED_QUANTITY_BATCH là hướng production fixed-batch nếu đã được kích hoạt theo owner decision.

G1/G2 có thể coexist theo (sku_id, formula_kind) nếu pack đã khóa.

G0/research/baseline tokens không được dùng operationally nếu đã bị cấm trong pack.

Future formula kind chỉ được thêm bằng owner decision.

TECH-02 không tự đổi các rule này.
Dev phải map đúng theo source-of-truth mới nhất.

## 23.4. Formula Version Status Model

Formula Version Status

Ý nghĩa

## DRAFT

Nháp

## UNDER_REVIEW

Đang review

## APPROVED

Được duyệt

## ACTIVE_OPERATIONAL

Được dùng operational theo scope

## ACTIVE_PILOT

Được dùng pilot theo scope

## RETIRED

Ngừng dùng

bị chặn

Bị chặn

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## 23.5. Coexistence Rule

Nếu hệ thống cho phép nhiều formula kind tồn tại cho cùng SKU, phải có rule:

Không conflict active version.

Không dùng nhầm pilot cho production.

Không dùng nhầm production cho pilot.

Production Order phải snapshot đúng formula kind/version.

Batch trace phải truy ngược đúng formula kind/version.

MRP phải biết đang tính theo formula kind nào.

UI/Admin phải hiển thị rõ để người vận hành không chọn nhầm.

## 23.6. Scope In

Formula Version Module được phép:

Tạo version mới.

Review version.

Approve version.

Activate version theo scope.

Retire version.

Cung cấp active version cho Production/MRP.

Cung cấp version history cho trace/audit.

Chặn version conflict.

Chặn forbidden formula kind.

Ghi audit mọi thay đổi.

## 23.7. Scope Out / Must Not Do

Formula Version Module không được:

Ghi đè version cũ.

Xóa version đã dùng trong production.

Cho active conflict không owner decision.

Cho forbidden G0/research token dùng operational.

Cho production order dùng version không approved.

Tự update batch cũ khi formula mới active.

Tự đổi formula kind của production order đã snapshot.

Public formula chi tiết trái policy.

Bỏ qua audit khi activate/retire.

Bỏ qua evidence khi release formula production.

## 23.8. Formula Version P0 điểm chặn

Các lỗi sau là P0:

Formula version ghi đè lịch sử.

Production Order dùng formula live thay vì snapshot.

Formula kind forbidden được dùng operational.

Active conflict giữa versions không rule.

G1/G2 bị nhầm scope.

Version chưa approved vẫn production.

Batch không trace được formula version.

MRP dùng sai formula kind.

Formula activation không audit.

Owner approval thiếu khi active production formula.

## 24. PRODUCT ACTIVATION MODULE CONTRACT

## 24.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W1-PRODUCT-ACTIVATION-001

Module Name

Product Activation Module

Source Pack

## PACK-02 / PACK-10

Wave

Wave 1

Application Group

Backend Core / Admin Web

Upstream

Product, SKU, Recipe, Formula, Product Knowledge, Foundation

Downstream

Commerce, AI, Live, Ads, Public Trace

Default Status

## WAITING_IMPLEMENTATION

Production Status

KHONG

## 24.2. Module Purpose

Product Activation Module quyết định Product/SKU có đủ điều kiện master để đưa vào các downstream scope hay chưa.

Activation không phải Sellable.

Activation trả lời:

Product master đủ chưa.

SKU master đủ chưa.

Recipe/formula required đủ chưa.

Product knowledge required approved chưa.

Product scope required rõ chưa.

Owner decision có chưa.

Có P0 điểm chặn không.

Có evidence required không.

Product/SKU được active ở scope nào.

Product/SKU chưa được active ở scope nào.

## 24.3. Activation Scope

Activation phải theo scope:

Activation Scope

Ý nghĩa

## INTERNAL_MASTER

Đủ master nội bộ

## OPERATIONAL_READY

Đủ để dùng trong production planning

## COMMERCE_CANDIDATE

Đủ điều kiện master để Commerce xem xét

## AI_KNOWLEDGE_READY

Đủ knowledge approved để AI dùng

## LIVE_SCOPE_READY

Đủ để đưa vào live scope

## ADS_MEASUREMENT_READY

Đủ mapping để đo Ads

## PUBLIC_TRACE_READY

Đủ public trace identity

## FULL_PRODUCT_ACTIVE

Đủ master theo scope owner duyệt

Không được gộp tất cả thành một flag “active” mơ hồ.

## 24.4. Activation Gate

Product/SKU chỉ được active theo scope khi đạt:

Product master approved.

SKU master approved.

Recipe/formula active nếu scope cần.

Product knowledge approved nếu AI/Live/Public cần.

Product scope approved nếu channel cần.

No P0 điểm chặn.

Owner decision nếu production scope.

Audit record.

Evidence accepted nếu release scope yêu cầu.

## 24.5. Activation vs Sellable

Activation Module phải luôn trả rõ:

Sellable do downstream gate tổng hợp từ:

Product/SKU active.

Price active.

Stock available.

Batch release.

Warehouse receipt.

No quality hold.

No recall.

No sale lock.

Channel/program policy.

Commerce Runtime.

## 24.6. Product Activation P0 điểm chặn

Các lỗi sau là P0:

Một flag active duy nhất bị dùng thay mọi scope.

Product active bị hiểu là sellable.

SKU chưa đủ recipe/formula vẫn active operational.

Product knowledge chưa approved vẫn active AI.

Live scope chưa approved vẫn cue live.

Ads measurement không có SKU mapping vẫn dùng.

Public trace identity chưa approved vẫn public.

Activation không audit.

Activation thiếu owner decision cho production scope.

Activation bypass P0 điểm chặn.

## 25. PRODUCT KNOWLEDGE RUNTIME MODULE CONTRACT

## 25.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W1-PRODUCT-KNOWLEDGE-001

Module Name

Product Knowledge Runtime Module

Source Pack

## PACK-02 / PACK-06

Wave

Wave 1 / Wave 4 dependency

Application Group

Backend Core / AI Runtime

Upstream

Product, SKU, Ingredient, Claim Guard, Foundation

Downstream

AI Advisor, Website, Landing Page, Messenger, Live, CRM if scope

Default Status

## WAITING_IMPLEMENTATION

Production Status

KHONG

## 25.2. Module Purpose

Product Knowledge Runtime cung cấp tri thức sản phẩm đã được duyệt cho AI, Live, Web/Landing Page, Messenger và các nội dung tư vấn.

Module này ngăn AI bịa sản phẩm, bịa claim, bịa công dụng, bịa thành phần hoặc dùng dữ liệu nội bộ chưa public.

## 25.3. Knowledge Layer

Product Knowledge phải phân lớp:

Layer

Ý nghĩa

## INTERNAL_PRODUCT_KNOWLEDGE

Dữ liệu nội bộ, source, formula summary, technical note

## PUBLIC_PRODUCT_KNOWLEDGE

Nội dung được phép nói với khách

## AI_RENDER_READY_KNOWLEDGE

Nội dung/biến được AI có thể render

## LIVE_SCRIPT_READY_KNOWLEDGE

Nội dung được phép cue live

## CLAIM_REVIEW_REQUIRED

Nội dung cần claim reviewer

## BLOCKED_PUBLIC

Không được public

## 25.4. Public Knowledge Requirement

Public Knowledge phải có:

Public product name.

Public-safe description.

Ingredient public summary.

Product positioning.

Dietary flags nếu có.

Usage guidance nếu approved.

Public-safe benefit phrase.

Claim whitelist mapping.

Forbidden claim list nếu có.

Version.

Approval status.

Effective scope.

## 25.5. Scope In

Product Knowledge Runtime được phép:

Quản lý tri thức sản phẩm approved.

Cung cấp dữ liệu cho AI.

Cung cấp dữ liệu cho Live script.

Cung cấp dữ liệu cho public product display nếu scope có.

Gắn claim whitelist.

Gắn dietary/public-safe flags.

Gắn replacement suggestion metadata nếu có.

Version hóa nội dung.

Audit approval/change.

Chặn downstream nếu knowledge chưa approved.

## 25.6. Scope Out / Must Not Do

Product Knowledge Runtime không được:

Tự tạo claim chưa được duyệt.

Tự nói sản phẩm như thuốc.

Public công thức chi tiết nếu không được phép.

Public supplier/costing/QC defect/internal note.

Cho AI dùng internal knowledge để render cho khách.

Cho Live cue claim chưa approved.

Cho nội dung hết hiệu lực vẫn dùng production.

Cho product knowledge không gắn SKU/product source.

Bỏ qua claim guard.

Dùng content cũ/legacy làm source nếu chưa approved.

## 25.7. Product Knowledge P0 điểm chặn

Các lỗi sau là P0:

AI dùng knowledge chưa approved.

Live cue knowledge chưa approved.

Public claim vượt whitelist.

Product được mô tả như thuốc/chữa bệnh.

Public lộ công thức chi tiết/nội bộ.

Public lộ supplier/costing/QC/internal.

Knowledge không version.

Knowledge bị sửa không audit.

Knowledge cũ bị dùng sau khi superseded.

Product Knowledge không map đúng SKU/product.

## 26. PRODUCT SCOPE REGISTRY MODULE CONTRACT

## 26.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W1-PRODUCT-SCOPE-001

Module Name

Product Scope Registry Module

Source Pack

## PACK-02 / PACK-05 / PACK-06 / PACK-07 / PACK-08

Wave

Wave 1 dependency for Wave 4-8

Application Group

Backend Core / Channel Runtime

Upstream

Product/SKU/Activation/Sellable dependency

Downstream

Commerce, AI, Facebook, Ads, Live, Public Trace

Default Status

## WAITING_IMPLEMENTATION

Production Status

KHONG

## 26.2. Module Purpose

Product Scope Registry xác định Product/SKU được phép dùng trong bối cảnh nào.

Không phải SKU active là được dùng ở mọi nơi.

Product Scope Registry trả lời:

SKU này có được Commerce quote/order không.

SKU này có được AI tư vấn không.

SKU này có được Facebook Gateway đề cập không.

SKU này có được Live cue không.

SKU này có được Ads đo/scale không.

SKU này có được Public Trace không.

SKU này có được Landing Page/Web display không.

SKU này có bị excluded khỏi release scope không.

SKU này có cần owner review không.

## 26.3. Scope Types

Scope Type

Ý nghĩa

## COMMERCE_SCOPE

Được Commerce xem xét quote/order nếu sellable clear

## AI_ADVISOR_SCOPE

Được AI tư vấn/render nếu knowledge approved

## FACEBOOK_SCOPE

Được Gateway nhắc/handoff theo policy

## ADS_MEASUREMENT_SCOPE

Được đo event/revenue mapping

## ADS_SCALE_SCOPE

Được scale Ads nếu Data Quality/Revenue/Owner pass

## LIVE_CUE_SCOPE

Được MC AI Live cue

## PUBLIC_TRACE_SCOPE

Được public trace identity

## LANDING_PAGE_SCOPE

Được hiển thị landing/page

## CRM_SCOPE

Được dùng CRM nếu có

## EXCLUDED_SCOPE

Loại khỏi scope hiện tại

## 26.4. Scope Decision Rule

Một Product/SKU chỉ được đưa vào scope khi:

Product/SKU active theo activation scope tương ứng.

Product Knowledge approved nếu cần public/AI/Live.

Sellable clear nếu scope là bán hàng.

Price/program clear nếu scope là Commerce.

Stock/Batch/Sale Lock clear nếu scope bán hàng.

Claim Guard clear nếu scope có claim.

Channel policy clear nếu scope Facebook/Live/Ads.

Owner decision có nếu production scope.

Evidence accepted nếu release scope.

Audit record created.

## 26.5. Scope Out / Must Not Do

Product Scope Registry không được:

Đưa SKU chưa active vào production scope.

Đưa SKU không sellable vào sales scope.

Đưa SKU chưa knowledge approved vào AI scope.

Đưa SKU chưa owner approve vào Live production scope.

Đưa SKU không verified revenue mapping vào Ads official reporting.

Đưa SKU sale lock/recall vào Commerce/AI/Ads/Live sales scope.

Dùng scope cũ sau khi product status thay đổi.

Bỏ qua release evidence.

Bỏ qua audit.

Gộp scope thành một flag mơ hồ.

## 26.6. Product Scope P0 điểm chặn

Các lỗi sau là P0:

Live cue SKU ngoài Live Scope.

AI tư vấn SKU ngoài AI Scope.

Commerce quote SKU ngoài Commerce Scope.

Ads scale SKU ngoài Ads Scale Scope.

Public Trace hiển thị SKU ngoài Public Trace Scope.

SKU bị sale lock/recall vẫn nằm sales scope.

Scope thay đổi không audit.

Scope production không owner decision.

Scope bị hardcode trong prompt/script/UI.

Một flag scope dùng cho mọi kênh gây nhầm lẫn.

## 27. PRODUCT EXPANSION / FUTURE SKU GOVERNANCE

## 27.1. Mục tiêu

Ginsengfood phải có khả năng mở rộng SKU trong tương lai mà không phá dữ liệu cũ.

Thêm SKU mới phải đi qua governance.

Không được thêm SKU mới bằng cách:

Thêm hardcode vào AI.

Thêm trong frontend dropdown.

Thêm trong Ads dashboard.

Thêm trong Live script thủ công.

Thêm trong seed không owner decision.

Clone SKU cũ rồi sửa tên mà không công thức/knowledge/scope.

## 27.2. Future SKU Onboarding Flow

Một SKU mới phải đi qua:

Product master created.

SKU master created.

Ingredient impact reviewed.

Recipe/formula created.

Formula version approved.

Product knowledge drafted.

Claim/public wording reviewed.

Product activation by scope.

Product scope registry configured.

Commerce dependency reviewed.

Operational dependency reviewed.

AI/Live/Ads dependency reviewed.

Evidence submitted.

Smoke passed.

Owner decision.

Release scope defined.

## 27.3. Future SKU P0 điểm chặn

Các lỗi sau là P0:

SKU mới không có recipe/formula.

SKU mới không có product knowledge approved nhưng AI dùng.

SKU mới không có product scope nhưng Live cue.

SKU mới không có price/sellable nhưng Commerce quote.

SKU mới không có MRP/ingredient mapping nhưng production planning dùng.

SKU mới không có trace/public identity nhưng Public Trace dùng.

SKU mới không có evidence/smoke nhưng production release.

SKU mới làm phá recipe version cũ.

SKU mới dùng ingredient mới chưa approved.

SKU mới không owner decision.

## 28. PRODUCT MODULE PERMISSION / AUDIT REQUIREMENT

## 28.1. Permission bắt buộc

Các action Product cần permission:

Action

Permission baseline

Create product

product.create

Update product

product.update

Activate product

product.activate

Retire product

product.retire

Create SKU

sku.create

Update SKU

sku.update

Activate SKU

sku.activate

Create ingredient

ingredient.create

Update ingredient

ingredient.update

Create recipe

recipe.create

Update recipe

recipe.update

Approve recipe

recipe.approve

Activate formula

formula.activate

Retire formula

formula.retire

Approve product knowledge

product_knowledge.approve

Update product scope

product_scope.update

Approve production scope

product_scope.production_approve

## 28.2. Audit bắt buộc

Audit bắt buộc cho:

Product create/update/activate/retire.

SKU create/update/activate/retire.

Ingredient create/update/retire.

Recipe create/update/approve/activate.

Formula version create/approve/activate/retire.

Product Knowledge approve/reject/update.

Product Scope update/approve/exclude.

Product Activation status change.

Owner decision.

P0 điểm chặn clear.

## 28.3. Separation of Duties

Nên tách:

Người tạo recipe và người approve recipe.

Người tạo Product Knowledge và người approve claim/public wording.

Người đề xuất Product Scope và người approve production scope.

Người tạo formula version và người activate production formula.

Người submit evidence và người accept evidence.

Nếu chưa tách được, phải ghi:

## OWNER_REVIEW_REQUIRED

## 29. PRODUCT MODULE SMOKE PREVIEW

## PHẦN 4/4 sẽ khóa smoke matrix đầy đủ, nhưng PHẦN 2/4 cần preview các smoke bắt buộc:

Smoke ID

Scenario

Expected Result

## PROD-SMK-001

Product ACTIVE nhưng không SKU

Không được Commerce quote

## PROD-SMK-002

SKU ACTIVE nhưng chưa sellable

Commerce/AI không được chốt bán

## PROD-SMK-003

Recipe draft

Không được production dùng

## PROD-SMK-004

Formula version mới active

Batch cũ không bị đổi lịch sử

## PROD-SMK-005

Product Knowledge chưa approved

AI/Live không được dùng

## PROD-SMK-006

Product Scope chưa Live approved

MC AI Live không cue

## PROD-SMK-007

Ingredient inactive

Recipe active không được dùng ingredient đó

## PROD-SMK-008

SKU sale lock

Commerce/AI/Ads/Live bị chặn

## PROD-SMK-009

Thêm SKU mới

Phải đi qua onboarding flow

## PROD-SMK-010

Scope production không owner decision

Bị chặn

## 30. TECH-02 PHẦN 2 DONE GATE

## PHẦN 2/4 được xem là hoàn tất khi đã khóa:

Product Master Module Contract.

SKU Master Module Contract.

Ingredient Master Module Contract.

Recipe Module Contract.

Formula Version Module Contract.

Product Activation Module Contract.

Product Knowledge Runtime Module Contract.

Product Scope Registry Module Contract.

Future SKU Governance.

Permission/Audit requirements.

P0 điểm chặn theo từng module.

Product smoke preview.

## 31. TECH-02 PHẦN 2 FAIL GATE

## PHẦN 2/4 fail nếu đội kỹ thuật:

Viết Product/SKU module không có contract.

Không tách Product và SKU.

Không tách Ingredient và Raw Lot.

Không tách Recipe và Formula Version.

Không tách Product Activation và Sellable.

Không có Product Knowledge approval.

Không có Product Scope theo kênh.

Không có Formula Version governance.

Không có Recipe Line Group lock.

Không có permission/audit cho Product changes.

Không có owner decision cho production scope.

Không có P0 điểm chặn list.

Không có smoke preview.

Dùng legacy seed/product list làm truth.

Gọi Product Active là được bán.

Nếu có lỗi:

## 32. KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 của TECH-02 đã khóa Module Contract cho Product / SKU / Ingredient / Recipe / Formula / Product Activation / Product Knowledge / Product Scope.

Kết luận bắt buộc:

Product Master là danh tính sản phẩm thương mại, không phải tồn kho/giá/order.

SKU Master là đơn vị vận hành/bán hàng cụ thể, nhưng SKU active chưa phải sellable.

Ingredient Master là danh mục nguyên liệu, không phải Raw Lot.

Recipe phải có group, ingredient, quantity, UOM, formula kind/version.

Recipe line phải theo 4 group đã khóa.

Formula Version không được ghi đè lịch sử.

G1/G2/future formula phải có governance.

Product Activation phải theo scope, không dùng một flag mơ hồ.

Product Knowledge phải approved trước khi AI/Live/Web dùng.

Product Scope phải theo kênh: Commerce, AI, Facebook, Ads, Live, Public Trace, CRM.

Future SKU phải đi qua onboarding flow và owner decision.

Permission/audit là bắt buộc với mọi thay đổi Product/SKU/Recipe/Formula/Scope.

P0 điểm chặn Product phải chặn downstream.

Documentation Complete không phải Production Ready.

Trạng thái sau PHẦN 2/4:

## PHẦN 3/4 - PRODUCT LIFECYCLE / STATE MODEL / COMMAND-QUERY BOUNDARY / RUNTIME HANDOFF / DOWNSTREAM DEPENDENCY CONTROL

## 33. MỤC ĐÍCH CỦA PHẦN 3/4

## PHẦN 3/4 khóa cách Product domain vận hành như một hệ thống có lifecycle, state, guard, command, query, audit và dependency rõ ràng.

## PHẦN 3/4 trả lời:

Product đi qua các trạng thái nào.

SKU đi qua các trạng thái nào.

Ingredient đi qua các trạng thái nào.

Recipe / Formula Version đi qua lifecycle nào.

Product Activation được xử lý theo scope nào.

Product Knowledge được approve và publish thế nào.

Product Scope được bàn giao cho Commerce / AI / Ads / Live / Public Trace thế nào.

Command nào được phép thay đổi dữ liệu.

Query nào chỉ được đọc dữ liệu.

Runtime nào là nguồn cho downstream.

Khi nào downstream bị bị chặn.

Khi nào owner phải review.

Khi nào Product domain được Release Ready.

Khi nào Product domain vẫn phải giữ WAITING_EVIDENCE / bị chặn.

## PHẦN 3/4 không viết code.
PHẦN 3/4 không thiết kế API chi tiết.
PHẦN 3/4 không thiết kế database chi tiết.
PHẦN 3/4 không thiết kế UI chi tiết.

## PHẦN 3/4 chỉ khóa technical decision để dev triển khai đúng module Product.

## 34. PRODUCT DOMAIN STATE PRINCIPLE

## 34.1. Product domain không phải dữ liệu tĩnh

Product / SKU / Ingredient / Recipe không được xem là bảng danh mục tĩnh.

Trong Ginsengfood, Product domain có ảnh hưởng trực tiếp đến:

Sản xuất.

## MRP.

Thu mua nguyên liệu.

Công thức.

Batch.

Kho.

Truy xuất.

Báo giá.

Đơn hàng.

AI tư vấn.

Live script.

Ads measurement.

Public trace.

MISA/accounting nếu scope có.

Vì vậy mọi thay đổi Product/SKU/Recipe/Formula phải có:

Permission.

Guard.

Audit.

Version nếu ảnh hưởng lịch sử.

Evidence nếu release scope.

Owner decision nếu production scope.

Downstream impact review.

## 34.2. State phải rõ, không dùng boolean mơ hồ

Không được thiết kế kiểu chỉ có:

isActive = true/false

isSellable = true/false

isPublished = true/false

và dùng một flag cho nhiều mục đích.

Thay vào đó phải phân biệt:

Product master status.

SKU master status.

Recipe status.

Formula version status.

Product knowledge status.

Product activation scope.

Product scope by channel.

Sellable status.

Release status.

Evidence status.

Một sản phẩm có thể:

active ở master;

chưa sellable;

có knowledge approved;

chưa mở AI scope;

có recipe active;

chưa có tồn kho;

chưa mở Commerce scope;

bị sale lock;

vẫn được internal trace nhưng không được public sales.

Do đó state phải đủ chi tiết.

## 35. PRODUCT LIFECYCLE MODEL

## 35.1. Product lifecycle chuẩn

Product nên đi qua lifecycle:

## DRAFT.

## INTERNAL_REVIEW.

## APPROVED.

## ACTIVE.

## SCOPE_CONFIGURED.

## RELEASE_CANDIDATE.

## RELEASED_FOR_SCOPE.

## INACTIVE.

## RETIRED.

bị chặn.

## OWNER_REVIEW_REQUIRED.

## 35.2. Ý nghĩa trạng thái Product

Product State

Ý nghĩa

## DRAFT

Product mới tạo, chưa đủ dữ liệu

## INTERNAL_REVIEW

Đang review nội bộ

## APPROVED

Product master đã được duyệt

## ACTIVE

Product được kích hoạt ở master

## SCOPE_CONFIGURED

Đã cấu hình scope sử dụng

## RELEASE_CANDIDATE

Đủ điều kiện trình release theo scope

## RELEASED_FOR_SCOPE

Đã được owner/release decision cho scope cụ thể

## INACTIVE

Tạm không dùng

## RETIRED

Ngừng sử dụng

bị chặn

Bị chặn do P0 hoặc thiếu dữ liệu

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## 35.3. Product state transition rule

From State

To State

Điều kiện

## DRAFT

## INTERNAL_REVIEW

Đủ dữ liệu tối thiểu

## INTERNAL_REVIEW

## APPROVED

Reviewer/owner duyệt master

## APPROVED

## ACTIVE

Product đủ master, audit, permission

## ACTIVE

## SCOPE_CONFIGURED

Có scope theo kênh/module

## SCOPE_CONFIGURED

## RELEASE_CANDIDATE

Có evidence/smoke liên quan scope

## RELEASE_CANDIDATE

## RELEASED_FOR_SCOPE

Owner release decision

## ACTIVE

## INACTIVE

Owner/permission hợp lệ

## ACTIVE / INACTIVE

## RETIRED

Có owner decision nếu đã từng production

Any

bị chặn

Có P0 điểm chặn

Any

## OWNER_REVIEW_REQUIRED

Có conflict/dependency chưa rõ

## 35.4. Product lifecycle P0

Các lỗi sau là P0:

Product DRAFT được downstream dùng production.

Product chưa APPROVED nhưng ACTIVE.

Product ACTIVE nhưng không audit.

Product RETIRED vẫn được quote/order.

Product bị chặn vẫn xuất hiện trong AI/Live/Commerce.

Product RELEASED_FOR_SCOPE nhưng không có owner decision.

Product dùng legacy source chưa review.

Product chuyển trạng thái không audit.

## 36. SKU LIFECYCLE MODEL

## 36.1. SKU lifecycle chuẩn

SKU nên đi qua lifecycle:

## DRAFT.

## MASTER_READY.

## RECIPE_REQUIRED.

## RECIPE_READY.

## KNOWLEDGE_REQUIRED.

## KNOWLEDGE_READY.

## SCOPE_WAITING.

## ACTIVE.

## RELEASE_CANDIDATE.

## RELEASED_FOR_SCOPE.

## INACTIVE.

## RETIRED.

bị chặn.

## OWNER_REVIEW_REQUIRED.

## 36.2. Ý nghĩa SKU state

SKU State

Ý nghĩa

## DRAFT

SKU mới, thiếu dữ liệu

## MASTER_READY

SKU đủ master data

## RECIPE_REQUIRED

SKU cần recipe/formula nhưng chưa đủ

## RECIPE_READY

Recipe/formula đã đủ theo scope

## KNOWLEDGE_REQUIRED

SKU cần knowledge để AI/Live/Public dùng

## KNOWLEDGE_READY

Product knowledge đã approved

## SCOPE_WAITING

Chưa được gán scope

## ACTIVE

SKU active ở master

## RELEASE_CANDIDATE

Đủ điều kiện trình release theo scope

## RELEASED_FOR_SCOPE

Được release cho scope cụ thể

## INACTIVE

Không active

## RETIRED

Ngừng dùng

bị chặn

Bị chặn

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## 36.3. SKU state transition guard

SKU chỉ được chuyển ACTIVE khi:

Product parent đã APPROVED / ACTIVE.

SKU master data đủ.

SKU code/public identity rõ.

Recipe/formula đủ nếu scope yêu cầu.

Product knowledge đủ nếu AI/Live/Public scope yêu cầu.

Không có P0 điểm chặn.

Permission hợp lệ.

Audit được ghi.

SKU chỉ được RELEASED_FOR_SCOPE khi:

Scope rõ.

Evidence accepted.

Smoke pass nếu scope production.

Owner decision có nếu production scope.

Downstream dependency clear.

## 36.4. SKU lifecycle P0

Các lỗi sau là P0:

SKU DRAFT được quote.

SKU chưa RECIPE_READY nhưng production dùng.

SKU chưa KNOWLEDGE_READY nhưng AI tư vấn.

SKU ACTIVE được hiểu nhầm là SELLABLE.

SKU RETIRED vẫn được Ads/Live dùng.

SKU RELEASED_FOR_SCOPE nhưng không có evidence.

SKU scope thay đổi không audit.

SKU public name chưa approved nhưng hiển thị cho khách.

## 37. INGREDIENT LIFECYCLE MODEL

## 37.1. Ingredient lifecycle chuẩn

Ingredient nên đi qua lifecycle:

## DRAFT.

## INTERNAL_REVIEW.

## APPROVED.

## ACTIVE.

bị chặn.

## INACTIVE.

## RETIRED.

## OWNER_REVIEW_REQUIRED.

## 37.2. Ingredient state guard

Ingredient chỉ được ACTIVE khi:

Tên/mã ingredient rõ.

UOM rõ.

Nhóm ingredient rõ.

Lot-managed flag rõ nếu cần.

Purchased/company source flag rõ nếu cần.

QC requirement rõ nếu cần.

Supplier/source dependency rõ nếu cần.

Procurement/MRP policy reference rõ nếu scope có.

Không có P0 điểm chặn.

Audit đầy đủ.

## 37.3. Ingredient impact review

Trước khi sửa Ingredient đang dùng trong recipe active, cần impact review:

Recipe nào đang dùng.

SKU nào bị ảnh hưởng.

Production Order nào có thể dùng.

MRP nào bị ảnh hưởng.

Raw lot / inventory nào liên quan.

Traceability nào bị ảnh hưởng.

Public knowledge có bị ảnh hưởng không.

Supplier/source policy có bị ảnh hưởng không.

Nếu ảnh hưởng production hoặc recipe active:

## OWNER_REVIEW_REQUIRED

## 37.4. Ingredient lifecycle P0

Các lỗi sau là P0:

Ingredient inactive vẫn dùng trong recipe active.

Ingredient changed làm sai recipe lịch sử.

Ingredient lot-managed nhưng không quản lý lot.

Ingredient company source bị xử lý như purchased nếu trái rule.

Ingredient supplier/source required nhưng bị bỏ qua.

Ingredient QC required nhưng không có guard.

Ingredient đổi UOM làm sai MRP/production.

Ingredient thay đổi không audit.

## 38. RECIPE LIFECYCLE MODEL

## 38.1. Recipe lifecycle chuẩn

Recipe nên đi qua:

## DRAFT.

## INTERNAL_REVIEW.

## VALIDATION_WAITING.

## VALIDATED.

## APPROVED.

## ACTIVE_FOR_SCOPE.

## LOCKED_BY_USAGE.

## RETIRED.

bị chặn.

## OWNER_REVIEW_REQUIRED.

## 38.2. Recipe validation rule

Recipe phải được validate trước khi APPROVED.

Validation tối thiểu:

SKU hợp lệ.

Formula kind hợp lệ.

Formula version hợp lệ.

Có đầy đủ recipe line.

Mỗi recipe line có group.

Group thuộc 4 group đã khóa.

Ingredient active.

Quantity hợp lệ.

UOM hợp lệ.

Không có forbidden token.

Không conflict active formula.

Không thiếu ingredient bắt buộc.

Không có P0 điểm chặn.

## 38.3. Recipe active rule

Recipe chỉ được ACTIVE_FOR_SCOPE khi:

Recipe APPROVED.

Formula Version hợp lệ.

Scope rõ: pilot / production / MRP / other.

Owner decision có nếu production.

Audit có.

Evidence có nếu release scope.

No conflict với active version hiện tại.

## 38.4. LOCKED_BY_USAGE

Khi Recipe đã được dùng trong Production Order hoặc Batch:

Recipe phải được LOCKED_BY_USAGE cho lịch sử đó.

Không được sửa trực tiếp recipe đã dùng để làm thay đổi lịch sử.

Muốn thay đổi phải:

Tạo version mới.

Review.

Approve.

Activate theo scope.

Không ảnh hưởng production/batch cũ.

## 38.5. Recipe lifecycle P0

Các lỗi sau là P0:

Recipe DRAFT được production dùng.

Recipe chưa VALIDATED nhưng APPROVED.

Recipe chưa APPROVED nhưng ACTIVE.

Recipe ACTIVE bị sửa trực tiếp.

Recipe đã dùng production nhưng không locked.

Recipe không có group theo 4 group khóa.

Ingredient inactive trong active recipe.

Recipe version conflict.

Recipe dùng forbidden formula kind/token.

Owner approval thiếu cho production recipe.

## 39. FORMULA VERSION LIFECYCLE MODEL

## 39.1. Formula version lifecycle chuẩn

Formula Version nên đi qua:

## DRAFT.

## REVIEW_WAITING.

## VALIDATED.

## APPROVED.

## ACTIVE_PILOT.

## ACTIVE_OPERATIONAL.

## LOCKED_BY_USAGE.

## RETIRED.

## SUPERSEDED.

bị chặn.

## OWNER_REVIEW_REQUIRED.

## 39.2. Active pilot và active operational phải tách rõ

Không được trộn:

## ACTIVE_PILOT

## ACTIVE_OPERATIONAL

Pilot formula không tự động được dùng cho production operational.

Production formula không tự động thay thế pilot nếu không có scope.

## 39.3. Formula activation guard

Formula chỉ được activate khi:

Recipe valid.

Formula kind hợp lệ.

Version rõ.

SKU rõ.

No conflict active rule.

Owner approval nếu scope production.

Evidence nếu release scope.

Audit.

Downstream impact review.

No P0 điểm chặn.

## 39.4. Formula version P0

Các lỗi sau là P0:

Formula version ghi đè nguồn đã có.

Active conflict không rule.

Pilot dùng nhầm production.

Production dùng nhầm pilot.

Forbidden token operational.

Formula active không owner decision.

Production Order không snapshot formula.

Batch không trace được formula version.

Formula activation không audit.

MRP dùng sai formula version.

## 40. PRODUCT KNOWLEDGE LIFECYCLE MODEL

## 40.1. Product Knowledge lifecycle

Product Knowledge nên đi qua:

## DRAFT.

## CONTENT_REVIEW.

## CLAIM_REVIEW_REQUIRED.

## CLAIM_REVIEWED.

## PUBLIC_SAFE_APPROVED.

## AI_RENDER_READY.

## LIVE_SCRIPT_READY.

## PUBLISHED_FOR_SCOPE.

## SUPERSEDED.

## RETIRED.

bị chặn.

## OWNER_REVIEW_REQUIRED.

## 40.2. Product Knowledge approval guard

Product Knowledge chỉ được AI/Live/Public dùng khi:

Gắn product/SKU rõ.

Có public name approved.

Có public-safe description.

Claim health/science đã review nếu có.

Không dùng language điều trị/chữa bệnh/thay thuốc.

Không lộ công thức chi tiết nếu policy cấm.

Không lộ supplier/costing/internal note.

Version rõ.

Scope rõ.

Audit approval.

## 40.3. Product Knowledge P0

Các lỗi sau là P0:

AI dùng DRAFT knowledge.

Live dùng content chưa claim review.

Public wording nói như thuốc.

Public wording lộ công thức/cost/supplier/internal.

Knowledge không gắn SKU/product.

Knowledge superseded vẫn dùng.

Knowledge không version.

Knowledge sửa không audit.

Claim reviewer chưa duyệt nhưng production.

Owner review required nhưng vẫn publish.

## 41. PRODUCT SCOPE LIFECYCLE MODEL

## 41.1. Product Scope lifecycle

Product Scope nên đi qua:

## SCOPE_DRAFT.

## INTERNAL_REVIEW.

## DEPENDENCY_CHECK.

## OWNER_REVIEW_REQUIRED.

## APPROVED_FOR_SCOPE.

## RELEASED_FOR_SCOPE.

## HOLD.

## EXCLUDED_FROM_SCOPE.

## RETIRED.

bị chặn.

## 41.2. Product Scope dependency check

Trước khi APPROVED_FOR_SCOPE, phải check:

Scope

Dependency

## COMMERCE_SCOPE

SKU active, price runtime, sellable dependency

## AI_ADVISOR_SCOPE

Product knowledge approved, claim guard

## FACEBOOK_SCOPE

AI/Commerce boundary nếu có handoff

## ADS_MEASUREMENT_SCOPE

SKU/order/revenue mapping

## ADS_SCALE_SCOPE

Data quality, verified revenue, sellable/fulfillment

## LIVE_CUE_SCOPE

Product knowledge, live script, claim guard, sellable

## PUBLIC_TRACE_SCOPE

Public identity, trace whitelist

## CRM_SCOPE

Product knowledge, customer memory policy

## IVR_SCOPE

Order product naming, no sales behavior

## 41.3. Product Scope P0

Các lỗi sau là P0:

Scope production không dependency check.

Scope không owner decision.

Scope thay đổi không audit.

Product sale lock/recall vẫn in sales scope.

Live cue scope mở khi claim guard chưa pass.

AI scope mở khi knowledge chưa approved.

Commerce scope mở khi SKU chưa active/sellable dependency.

Ads scale scope mở khi verified revenue/data quality chưa pass.

Public trace scope mở khi whitelist chưa clear.

Scope old config vẫn dùng sau product status change.

## 42. PRODUCT COMMAND BOUNDARY

## 42.1. Product command là gì

Command là hành động thay đổi dữ liệu/state Product domain.

Các command Product cần được kiểm soát:

## CREATE_PRODUCT.

## UPDATE_PRODUCT.

## APPROVE_PRODUCT.

## ACTIVATE_PRODUCT.

## RETIRE_PRODUCT.

## CREATE_SKU.

## UPDATE_SKU.

## ACTIVATE_SKU.

## RETIRE_SKU.

## CREATE_INGREDIENT.

## UPDATE_INGREDIENT.

## RETIRE_INGREDIENT.

## CREATE_RECIPE.

## UPDATE_RECIPE.

## VALIDATE_RECIPE.

## APPROVE_RECIPE.

## ACTIVATE_RECIPE.

## CREATE_FORMULA_VERSION.

## APPROVE_FORMULA_VERSION.

## ACTIVATE_FORMULA_VERSION.

## RETIRE_FORMULA_VERSION.

## SUBMIT_PRODUCT_KNOWLEDGE.

## APPROVE_PRODUCT_KNOWLEDGE.

## UPDATE_PRODUCT_SCOPE.

## APPROVE_PRODUCT_SCOPE.

## EXCLUDE_PRODUCT_SCOPE.

## 42.2. Product command requirement

Mọi command Product phải có:

Actor.

Permission.

Scope.

Guard.

Audit.

Reason nếu rủi ro.

Owner decision nếu production scope.

Evidence nếu release scope.

Idempotency nếu command có nguy cơ submit lặp.

Downstream impact nếu thay đổi active/scope/formula.

## 42.3. Product command idempotency

Các command sau nên có idempotency:

## CREATE_PRODUCT.

## CREATE_SKU.

## CREATE_RECIPE.

## CREATE_FORMULA_VERSION.

## ACTIVATE_SKU.

## ACTIVATE_FORMULA_VERSION.

## APPROVE_PRODUCT_SCOPE.

## APPROVE_PRODUCT_KNOWLEDGE.

## EXCLUDE_PRODUCT_SCOPE.

## RETIRE_SKU.

Mục tiêu: tránh tạo trùng product/SKU/recipe/version/scope khi retry.

## 43. PRODUCT QUERY BOUNDARY

## 43.1. Product query là gì

Query là thao tác đọc dữ liệu Product domain.

Query không được thay đổi state.

Các query chính:

## GET_PRODUCT.

## GET_PRODUCT_PUBLIC_VIEW.

## GET_PRODUCT_INTERNAL_VIEW.

## GET_SKU.

## GET_SKU_FOR_COMMERCE.

## GET_SKU_FOR_OPERATIONAL.

## GET_INGREDIENT.

## GET_RECIPE_VERSION.

## GET_ACTIVE_FORMULA_FOR_SCOPE.

## GET_PRODUCT_KNOWLEDGE_FOR_AI.

## GET_PRODUCT_SCOPE_FOR_CHANNEL.

## GET_PRODUCT_TRACE_PUBLIC_IDENTITY.

## GET_PRODUCT_RELEASE_STATUS.

## 43.2. Query phải tôn trọng data boundary

Ví dụ:

AI query chỉ được lấy AI_RENDER_READY knowledge.

Live query chỉ được lấy LIVE_SCRIPT_READY knowledge.

Public Trace query chỉ lấy PUBLIC_TRACE_READY fields.

Ads query lấy SKU mapping, không lấy formula detail.

Commerce query lấy SKU active/sellable dependency, không tự đọc internal formula.

Supplier query không được xem sản phẩm/scope ngoài quyền.

Public query không lấy internal notes.

## 44. PRODUCT RUNTIME / RESOLVER BOUNDARY

## 44.1. Product runtime là gì

Product Runtime cung cấp dữ liệu sản phẩm đã approved cho downstream theo scope.

Runtime không phải dữ liệu draft.
Runtime không phải legacy seed.
Runtime không phải prompt AI.
Runtime không phải file Excel ngoài hệ thống.

Product Runtime chỉ được cung cấp dữ liệu đã:

Approved.

Active theo scope.

Không bị bị chặn.

Không expired.

Không superseded.

Có audit/version.

## 44.2. Runtime views theo consumer

Consumer

Runtime View

Operational

SKU + active formula + ingredient recipe snapshot source

## MRP

SKU + formula + ingredient/UOM/scaling basis

Commerce

SKU identity + activation/sellable dependency + public name

AI Advisor

Product knowledge public-safe + resolver variables

Facebook Gateway

Product public name + safe handoff metadata

Ads Measurement

SKU/product mapping + campaign product scope

MC AI Live

Product scope + live script ready knowledge

Public Trace

Public identity + whitelist fields

MISA/Accounting

SKU/material mapping if scope

## 44.3. Runtime unavailable rule

Nếu Product Runtime không có dữ liệu hợp lệ:

AI không được bịa sản phẩm.

Commerce không được quote SKU đó.

Live không được cue SKU đó.

Ads không được scale SKU đó.

Public Trace không được public SKU đó.

Operational không được production theo formula đó.

MRP không được tính theo recipe đó.

Kết quả phải là bị chặn / OWNER_REVIEW_REQUIRED / DATA_UNAVAILABLE theo scope.

## 45. PRODUCT DOWNSTREAM HANDOFF MODEL

## 45.1. Handoff sang Operational Core

Product -> Operational cần cung cấp:

SKU identity.

Active formula/version.

Ingredient list.

Recipe line group.

UOM/quantity.

Snapshot requirement.

Packaging/trade item reference nếu scope có.

Product/SKU trace identity.

Formula kind.

Activation status.

Operational không được tự sửa Product/Recipe master.

## 45.2. Handoff sang MRP / Procurement

Product -> MRP cần cung cấp:

## SKU.

Formula kind/version.

Ingredient BOM.

Quantity/UOM.

Ingredient group.

Shared/special ingredient classification nếu scope có.

Strategic reserve flags nếu ingredient liên quan.

Procurement suppression policy reference.

Formula scaling basis.

Future SKU impact.

MRP không được tự sửa recipe.

## 45.3. Handoff sang Commerce Runtime

Product -> Commerce cần cung cấp:

Product public identity.

SKU public identity.

SKU active status.

Commerce scope.

Product activation status.

Product scope status.

Sellable dependency status.

Price dependency placeholder.

Product restriction flags nếu có.

Product unavailable reason nếu bị chặn.

Commerce mới quyết quote/order theo price/sellable/payment/shipping rule.

## 45.4. Handoff sang AI Advisor

Product -> AI cần cung cấp:

Public product name.

Product knowledge approved.

Public-safe benefit language.

Dietary flags.

Ingredient public summary.

Claim whitelist mapping.

Forbidden claim.

Product replacement metadata nếu có.

AI scope.

Runtime variable mapping.

AI không được dùng internal product data để render public nếu chưa approved.

## 45.5. Handoff sang Facebook Gateway

Product -> Facebook Gateway cần cung cấp:

Public product name.

Channel-safe product metadata.

Product scope for Facebook.

Private handoff metadata.

No public price unless Commerce Runtime authorizes by flow.

Health-sensitive guard indicator nếu liên quan.

Product unavailable status nếu cần.

Gateway không tự tạo product content.

## 45.6. Handoff sang Ads Measurement

Product -> Ads cần cung cấp:

Product/SKU mapping.

Ads measurement scope.

Campaign product scope.

SKU-to-order mapping reference.

SKU-to-revenue mapping reference.

Product status.

Excluded product list.

Sale lock/recall dependency.

Ads không được đo/scale SKU không scope.

## 45.7. Handoff sang MC AI Live

Product -> MC AI Live cần cung cấp:

Live Product Scope.

Live-safe product name.

Live script ready knowledge.

Claim guard status.

Product sellable dependency.

Product unavailable reason.

Replacement suggestion nếu scope có.

Forbidden topic/claim.

Risk flag.

MC AI Live không cue sản phẩm ngoài Live Product Scope.

## 45.8. Handoff sang Public Trace

Product -> Public Trace cần cung cấp:

Public product identity.

Public SKU/trade item identity nếu allowed.

Public-safe product description.

Trace whitelist mapping.

Fields allowed.

Fields bị chặn.

Version of public text.

Public trace scope.

Public Trace không đọc internal product/recipe/ingredient detail trực tiếp.

## 46. PRODUCT CHANGE IMPACT CONTROL

## 46.1. Khi nào cần impact review

Cần Product Impact Review khi thay đổi:

Product public name.

SKU public name.

SKU status.

Ingredient master.

Ingredient UOM.

Recipe line.

Quantity/UOM.

Formula version.

Product Knowledge.

Product Scope.

Product Activation.

Product group/category.

Public trace identity.

Trade item/GTIN reference nếu scope có.

## 46.2. Impact review phải xét downstream

Impact review phải xét:

Downstream

Câu hỏi

Operational

Có ảnh hưởng production order/batch không?

## MRP

Có ảnh hưởng BOM/procurement không?

Commerce

Có ảnh hưởng quote/order không?

## AI

Có ảnh hưởng câu tư vấn không?

Facebook

Có ảnh hưởng public/private reply không?

Ads

Có ảnh hưởng attribution/revenue mapping không?

Live

Có ảnh hưởng script/cue không?

Public Trace

Có ảnh hưởng public fields không?

## MISA

Có ảnh hưởng mapping kế toán không?

Evidence/Release

Có cần retest/release lại không?

## 46.3. Product change P0

Các thay đổi sau nếu không impact review là P0:

Recipe/formula thay đổi.

SKU public identity thay đổi.

Product Knowledge claim thay đổi.

Product Scope production thay đổi.

Ingredient UOM thay đổi.

Product Activation production thay đổi.

Commerce scope thay đổi.

Live scope thay đổi.

Public trace identity thay đổi.

MISA/accounting mapping thay đổi nếu scope có.

## 47. PRODUCT OWNER DECISION MODEL

## 47.1. Khi nào cần owner decision

Owner decision bắt buộc khi:

Activate product production scope.

Activate SKU production scope.

Activate formula production.

Add new SKU beyond baseline.

Retire SKU đã từng production.

Change product public identity.

Change formula version used in production.

Change Product Knowledge public claim.

Open AI scope.

Open Live scope.

Open Ads scale scope.

Open Commerce scope.

Publish Public Trace identity.

Accept risk/waiver liên quan Product P0.

## 47.2. Owner Decision Record

Mỗi owner decision phải có:

Decision ID.

Product/SKU.

Scope.

Formula/knowledge/scope liên quan.

Evidence liên quan.

Smoke liên quan.

P0 status.

Dependency status.

Decision result.

Effective date.

Conditions.

Audit.

## 48. PRODUCT EXCEPTION HANDLING

## 48.1. Product runtime unavailable

Nếu Product Runtime không có dữ liệu:

Consumer

Behavior

## AI

Không tư vấn/chốt SKU, trả safe fallback hoặc handoff

Commerce

Không quote/order SKU

Live

Không cue SKU

Ads

Không scale/report official theo SKU đó

Public Trace

Không public

Operational

Không production theo formula thiếu

## MRP

Không tính BOM thiếu

## 48.2. Formula conflict

Nếu formula conflict:

Block activation.

Mark OWNER_REVIEW_REQUIRED.

Prevent production order from using conflicted formula.

Require evidence/retest after fix.

Audit all decisions.

## 48.3. Product Knowledge claim conflict

Nếu claim conflict:

Block AI_RENDER_READY.

Block LIVE_SCRIPT_READY.

Mark CLAIM_REVIEW_REQUIRED.

Prevent public publish.

Require claim reviewer decision.

Audit.

## 49. PRODUCT RELEASE STATUS MODEL

## 49.1. Product release status theo scope

Product/SKU phải có release status theo scope:

Scope

Status

Master

## NOT_STARTED / SPEC_COMPLETE / ACTIVE

Operational

bị chặn / READY / RELEASED

Commerce

bị chặn / RELEASE_CANDIDATE / RELEASED

## AI

bị chặn / KNOWLEDGE_READY / RELEASED

Facebook

bị chặn / SCOPE_READY / RELEASED

Ads Measurement

bị chặn / MAPPED / RELEASED

Ads Scale

bị chặn / OWNER_REVIEW_REQUIRED / RELEASED

Live

bị chặn / SCRIPT_READY / RELEASED

Public Trace

bị chặn / WHITELIST_READY / RELEASED

Không dùng một status chung cho mọi scope.

## 49.2. Product release status P0

Các lỗi sau là P0:

Release status chung gây hiểu nhầm.

Product released for AI bị hiểu là released for Commerce.

Product released for Ads measurement bị hiểu là Ads scale.

Product released for Live rehearsal bị hiểu là Live production.

SKU master active bị hiểu là stock sellable.

Scope release không audit.

Scope release không owner decision.

## 50. TECH-02 PHẦN 3 DONE GATE

## PHẦN 3/4 được xem là hoàn tất khi đã khóa:

Product lifecycle model.

SKU lifecycle model.

Ingredient lifecycle model.

Recipe lifecycle model.

Formula Version lifecycle model.

Product Knowledge lifecycle model.

Product Scope lifecycle model.

Product command boundary.

Product query boundary.

Runtime/resolver boundary.

Downstream handoff model.

Product change impact control.

Owner decision model.

Exception handling.

Product release status model.

## 51. TECH-02 PHẦN 3 FAIL GATE

## PHẦN 3/4 fail nếu đội kỹ thuật:

Dùng một flag active cho mọi scope.

Không có state model cho Product/SKU/Recipe/Formula.

Không có command boundary.

Không có query boundary.

Query public đọc internal data.

AI dùng internal knowledge chưa public approved.

Product runtime unavailable nhưng downstream vẫn bịa dữ liệu.

Product scope không phân channel.

Formula conflict nhưng vẫn production.

Recipe active bị sửa trực tiếp.

Product change không impact review.

Owner decision thiếu cho scope production.

Product release status không theo scope.

Documentation complete bị gọi release ready.

Nếu có lỗi:

## 52. KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 của TECH-02 đã khóa lifecycle, state model, command-query boundary, runtime handoff và downstream dependency control cho Product domain.

Kết luận bắt buộc:

Product/SKU/Ingredient/Recipe/Formula không phải dữ liệu tĩnh.

Product state phải rõ, không dùng boolean mơ hồ.

SKU active không đồng nghĩa sellable.

Recipe active không được sửa trực tiếp sau khi đã production dùng.

Formula Version phải có lifecycle và không ghi đè lịch sử.

Product Knowledge phải có lifecycle approval trước AI/Live/Public.

Product Scope phải theo channel/scope.

Product command phải có permission, guard, audit, owner decision nếu production.

Product query không được thay đổi state.

Runtime chỉ cung cấp dữ liệu approved theo scope.

Runtime unavailable thì downstream phải block, không được bịa dữ liệu.

Downstream handoff phải có dữ liệu đúng scope.

Product change quan trọng phải impact review.

Owner decision bắt buộc cho production scope.

Product release status phải theo scope.

Documentation Complete không phải Production Ready.

Trạng thái sau PHẦN 3/4:

## PHẦN 4/4 - FINAL PRODUCT SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-02 FINAL CONCLUSION

## 53. MỤC ĐÍCH CỦA PHẦN 4/4

## PHẦN 4/4 là phần khóa cuối của TECH-02.

Nếu PHẦN 1/4 đã khóa nguyên tắc Product Domain, PHẦN 2/4 đã khóa Module Contract, PHẦN 3/4 đã khóa Lifecycle / State / Command / Query / Runtime Handoff, thì PHẦN 4/4 khóa:

Final Product Smoke Matrix.

Product Evidence Package.

Product Done Gate.

Product Fail Gate.

Product Release Handoff Package.

Product Owner Review Points.

Product Security / Data / Public Boundary Review.

Product Downstream Readiness Rule.

TECH-02 Final Conclusion.

## PHẦN 4/4 không viết code.
PHẦN 4/4 không thiết kế API chi tiết.
PHẦN 4/4 không thiết kế database chi tiết.
PHẦN 4/4 không thiết kế UI chi tiết.

## PHẦN 4/4 chỉ khóa điều kiện để Product domain được xem là đủ đặc tả kỹ thuật và sau này đủ điều kiện cho Operational / Commerce / AI / Ads / Live / IVR phụ thuộc.

## 54. PRODUCT FINAL SMOKE PRINCIPLE

## 54.1. Product Smoke là gì

Product Smoke là bộ kiểm tra chứng minh Product / SKU / Ingredient / Recipe / Formula / Product Activation vận hành đúng trước khi các module downstream sử dụng.

Product Smoke không phải chỉ xem danh sách sản phẩm hiện trên UI.
Product Smoke không phải chỉ kiểm tra tạo được SKU.
Product Smoke không phải chỉ import được file danh mục.
Product Smoke không phải chỉ hiển thị được sản phẩm trên màn hình.

Product Smoke phải chứng minh:

Product và SKU được phân biệt đúng.

Ingredient và Raw Lot được phân biệt đúng.

Recipe và Production Snapshot được phân biệt đúng.

Product Active không bị hiểu nhầm là Sellable.

Formula Version không ghi đè lịch sử.

Product Knowledge chưa approved không được AI/Live/Public dùng.

Product Scope chưa approved không được đưa vào Commerce/AI/Ads/Live.

SKU bị sale lock/recall/not sellable không được downstream bán.

Product changes có audit.

Scope production có owner decision.

Downstream bị bị chặn khi Product domain chưa clear.

## 54.2. Smoke phải test cả bị chặn path

Product Smoke phải test:

Happy path.

Missing product data path.

Missing SKU path.

Missing recipe path.

Recipe draft path.

Formula conflict path.

Product knowledge not approved path.

Product scope not approved path.

Product active but not sellable path.

Sale lock/recall path.

Future SKU onboarding path.

Owner review required path.

Nếu chỉ test product tạo thành công:

## PRODUCT_SMOKE_STATUS = INCOMPLETE

## 55. FINAL PRODUCT SMOKE MATRIX

## 55.1. Product Master Smoke

Smoke ID

Scenario

Expected Result

## PROD-MST-SMK-001

Tạo product draft

Product ở trạng thái DRAFT, chưa downstream production

## PROD-MST-SMK-002

Product thiếu public name approved

AI/Live/Public không được dùng

## PROD-MST-SMK-003

Product APPROVED -> ACTIVE

Có permission, guard, audit

## PROD-MST-SMK-004

Product ACTIVE nhưng chưa scope

Downstream Commerce/AI/Live/Ads bị bị chặn

## PROD-MST-SMK-005

Product RETIRED

Không được quote/order/cue live/scale ads

## PROD-MST-SMK-006

Product bị chặn

Downstream sales scope bị chặn

## PROD-MST-SMK-007

Product status change

Audit created

## PROD-MST-SMK-008

Product legacy chưa review

## OWNER_REVIEW_REQUIRED

## 55.2. SKU Master Smoke

Smoke ID

Scenario

Expected Result

## SKU-SMK-001

SKU tạo mới

SKU ở trạng thái DRAFT

## SKU-SMK-002

SKU không có product mapping

Không được active

## SKU-SMK-003

SKU thiếu recipe required

Không được operational/production scope

## SKU-SMK-004

SKU thiếu product knowledge

Không được AI/Live scope

## SKU-SMK-005

SKU ACTIVE nhưng chưa sellable

Commerce/AI không được chốt bán

## SKU-SMK-006

## SKU RETIRED

Không xuất hiện production scope

## SKU-SMK-007

SKU public name chưa approved

Không render với khách

## SKU-SMK-008

SKU thêm mới ngoài baseline

Phải qua onboarding + owner decision

## 55.3. Ingredient Master Smoke

Smoke ID

Scenario

Expected Result

## ING-SMK-001

Tạo ingredient active

Ingredient có mã, nhóm, UOM, audit

## ING-SMK-002

Ingredient inactive dùng trong recipe active

bị chặn

## ING-SMK-003

Ingredient lot-managed nhưng thiếu lot policy

bị chặn for operational use

## ING-SMK-004

Ingredient company source bị dùng như purchased trái rule

bị chặn / Owner Review

## ING-SMK-005

Ingredient QC required nhưng intake bỏ qua

bị chặn downstream

## ING-SMK-006

Ingredient UOM thay đổi

Impact review required

## ING-SMK-007

Ingredient thay thế trong recipe active

New formula version required

## ING-SMK-008

Ingredient public leak supplier/costing

bị chặn

## 55.4. Recipe Smoke

Smoke ID

Scenario

Expected Result

## RCP-SMK-001

Recipe draft

Không được production dùng

## RCP-SMK-002

Recipe thiếu line group

Validation failed

## RCP-SMK-003

Recipe line group ngoài 4 group khóa

bị chặn unless owner decision

## RCP-SMK-004

Recipe thiếu ingredient/quantity/UOM

Validation failed

## RCP-SMK-005

Recipe dùng ingredient inactive

bị chặn

## RCP-SMK-006

Recipe approved

Audit created

## RCP-SMK-007

Recipe active for production

Owner decision required

## RCP-SMK-008

Recipe active đã dùng production bị sửa

bị chặn; new version required

## RCP-SMK-009

Recipe public leak formula detail

bị chặn

## RCP-SMK-010

Recipe used by Production Order

Snapshot required

## 55.5. Formula Version Smoke

Smoke ID

Scenario

Expected Result

## FML-SMK-001

Tạo formula version mới

Version mới không ghi đè version cũ

## FML-SMK-002

Activate formula pilot

ACTIVE_PILOT only

## FML-SMK-003

Activate formula operational

Owner decision + audit required

## FML-SMK-004

Forbidden formula kind/token operational

bị chặn

## FML-SMK-005

G1/G2 conflict

Owner review / bị chặn

## FML-SMK-006

Production Order dùng formula

Snapshot kind/version

## FML-SMK-007

Batch trace formula

Truy được formula version

## FML-SMK-008

Retire formula used in history

History preserved

## FML-SMK-009

Formula activation without audit

## FML-SMK-010

MRP reads wrong formula kind

bị chặn

## 55.6. Product Activation Smoke

Smoke ID

Scenario

Expected Result

## ACT-SMK-001

Product master approved

Có thể xét activation, chưa sellable

## ACT-SMK-002

SKU active nhưng chưa price/stock/batch

Not sellable

## ACT-SMK-003

AI Knowledge chưa approved

AI scope bị chặn

## ACT-SMK-004

Live scope chưa approved

Live cue bị chặn

## ACT-SMK-005

Ads measurement mapping thiếu

Ads official mapping bị chặn

## ACT-SMK-006

Product active flag dùng thay all scope

## ACT-SMK-007

Activation production không owner decision

bị chặn

## ACT-SMK-008

Activation status change

Audit created

## ACT-SMK-009

P0 điểm chặn open

Activation bị chặn

## ACT-SMK-010

Scope excluded

Downstream must respect excluded scope

## 55.7. Product Knowledge Smoke

Smoke ID

Scenario

Expected Result

## PK-SMK-001

Knowledge draft

AI/Live/Public bị chặn

## PK-SMK-002

Public-safe approved

AI can use within scope

## PK-SMK-003

Claim review required

AI/Live bị chặn until review

## PK-SMK-004

Claim vượt whitelist

Rejected

## PK-SMK-005

Content nói như thuốc

Rejected

## PK-SMK-006

Content lộ công thức/cost/supplier/internal

Rejected

## PK-SMK-007

Knowledge superseded

Old version not used production

## PK-SMK-008

Knowledge version missing

Rejected

## PK-SMK-009

AI query internal knowledge for public render

bị chặn

## PK-SMK-010

Knowledge approval

Audit created

## 55.8. Product Scope Smoke

Smoke ID

Scenario

Expected Result

## PSC-SMK-001

Add SKU to Commerce Scope

Requires activation + sellable dependency

## PSC-SMK-002

Add SKU to AI Scope

Requires knowledge approved

## PSC-SMK-003

Add SKU to Live Scope

Requires live-safe knowledge + owner decision

## PSC-SMK-004

Add SKU to Ads Scale Scope

Requires data quality/verified revenue/owner decision

## PSC-SMK-005

Sale lock active

SKU removed/bị chặn from sales scope

## PSC-SMK-006

Recall active

SKU/batch sales scope bị chặn

## PSC-SMK-007

Scope changed

Audit created

## PSC-SMK-008

Scope production without evidence

bị chặn

## PSC-SMK-009

One flag used for all scopes

## PSC-SMK-010

Excluded scope

Downstream must block usage

## 55.9. Downstream Handoff Smoke

Smoke ID

Scenario

Expected Result

## DSH-SMK-001

Product -> Operational

SKU + active formula + recipe snapshot source available

## DSH-SMK-002

Product -> MRP

BOM/ingredient/UOM/formula kind available

## DSH-SMK-003

Product -> Commerce

SKU identity + activation + sellable dependency available

## DSH-SMK-004

Product -> AI

AI_RENDER_READY knowledge only

## DSH-SMK-005

Product -> Facebook

Channel-safe public metadata only

## DSH-SMK-006

Product -> Ads

SKU/product/order/revenue mapping reference

## DSH-SMK-007

Product -> MC AI Live

Live Product Scope + script-ready knowledge

## DSH-SMK-008

Product -> Public Trace

Whitelist public identity only

## DSH-SMK-009

Runtime unavailable

Downstream bị chặn, no fake data

## DSH-SMK-010

Product P0 open

Downstream product dependency bị chặn

## 56. PRODUCT EVIDENCE PACKAGE

## 56.1. Product Evidence Package là gì

Product Evidence Package là gói bằng chứng chứng minh Product domain đã được triển khai, test, smoke và review đủ để downstream phụ thuộc theo scope.

Product Evidence Package không đồng nghĩa Product Production Ready.
Product Evidence Package là điều kiện bắt buộc để xét Product Release Ready.

## 56.2. Product Evidence Package bắt buộc gồm

Evidence Group

Nội dung

## EV-PROD-MASTER

Product Master evidence

## EV-SKU-MASTER

SKU Master evidence

## EV-ING-MASTER

Ingredient Master evidence

## EV-RECIPE

Recipe validation/version evidence

## EV-FORMULA

Formula version/coexistence evidence

## EV-ACTIVATION

Product/SKU activation evidence

## EV-KNOWLEDGE

Product Knowledge approval evidence

## EV-SCOPE

Product Scope Registry evidence

## EV-HANDOFF

Downstream runtime handoff evidence

## EV-PERMISSION

Permission/audit evidence for Product actions

## EV-P0

Product P0 điểm chặn fix/retest evidence

## EV-OWNER

Owner decision evidence

## EV-SECURITY

Public/internal data boundary evidence

## 56.3. Evidence tối thiểu theo nhóm

Evidence

Phải chứng minh

## EV-PROD-MASTER

Product có lifecycle, public name approved, status audit

## EV-SKU-MASTER

SKU mapping đúng product, active chưa bị hiểu là sellable

## EV-ING-MASTER

Ingredient có group/UOM/lot/source/QC flags đúng

## EV-RECIPE

Recipe đủ group/ingredient/quantity/UOM/version

## EV-FORMULA

Formula không ghi đè lịch sử, snapshot được

## EV-ACTIVATION

Activation theo scope, không thay sellable

## EV-KNOWLEDGE

AI/Live/Public chỉ dùng approved knowledge

## EV-SCOPE

Commerce/AI/Ads/Live/Public scope tách rõ

## EV-HANDOFF

Downstream nhận đúng runtime view

## EV-OWNER

Production scope có owner decision

## 56.4. Product Evidence bị reject nếu

Evidence bị REJECTED nếu:

Không gắn Product/SKU/module rõ.

Không gắn source rule.

Không rõ environment/version.

Không chứng minh active khác sellable.

Không chứng minh formula version không ghi đè.

Không chứng minh AI/Live dùng approved knowledge.

Không chứng minh Product Scope theo kênh.

Không có reviewer.

Có P0 mở.

Evidence dùng legacy data chưa owner review.

Evidence chỉ là ảnh UI không có smoke/log/audit liên quan.

Evidence không gắn release scope.

## 57. PRODUCT DEV DELIVERY CHECKLIST

## 57.1. Checklist dev phải nộp sau TECH-02

Checklist Item

Required

Product Module Map

Có

Product Master Spec

Có

SKU Master Spec

Có

Ingredient Master Spec

Có

Recipe Spec

Có

Formula Version Spec

Có

Product Activation Spec

Có

Product Knowledge Spec

Có

Product Scope Spec

Có

Future SKU Onboarding Spec

Có

Product Command Boundary

Có

Product Query Boundary

Có

Product Runtime/Handoff Map

Có

Product Permission Matrix

Có

Product Audit Plan

Có

Product P0 điểm chặn Baseline

Có

Product Smoke Plan

Có

Product Evidence Package Plan

Có

Product Owner Review Questions

Có

Product Downstream Dependency Report

Có

## 57.2. Không có checklist thì downstream bị chặn

Nếu thiếu Product Dev Delivery Checklist:

## 58. PRODUCT DONE GATE

## 58.1. Product Specification Done Gate

TECH-02 được xem là Specification Complete khi có đủ:

Product domain principles.

Source-of-truth.

Product/SKU/Ingredient/Recipe boundary.

Product Activation vs Sellable rule.

Formula governance.

Recipe line group lock.

Product Knowledge rule.

Product Scope rule.

Module contracts.

Lifecycle/state model.

Command/query boundary.

Runtime/handoff model.

Product change impact control.

Owner decision points.

Final smoke matrix.

Evidence package.

Done Gate / Fail Gate.

Release handoff rule.

Final conclusion.

## 58.2. Product Implementation Done Gate

Product Implementation Done khi:

Product Master implemented.

SKU Master implemented.

Ingredient Master implemented.

Recipe Module implemented.

Formula Version implemented.

Product Activation implemented.

Product Knowledge implemented.

Product Scope implemented.

Product command permission/audit implemented.

Product runtime/handoff implemented.

Implementation evidence submitted.

Implementation evidence accepted.

Nếu mới code xong nhưng chưa test/smoke/evidence:

## PRODUCT_STATUS = IMPLEMENTATION_COMPLETE_ONLY

## 58.3. Product Test Done Gate

Product Test Done khi:

Product state tests pass.

SKU state tests pass.

Ingredient tests pass.

Recipe validation tests pass.

Formula version tests pass.

Activation scope tests pass.

Product Knowledge approval tests pass.

Product Scope tests pass.

Product downstream handoff tests pass.

Product permission/audit tests pass.

Test evidence accepted.

## 58.4. Product Smoke Done Gate

Product Smoke Done khi:

Product Master smoke PASS.

SKU Master smoke PASS.

Ingredient smoke PASS.

Recipe smoke PASS.

Formula Version smoke PASS.

Product Activation smoke PASS.

Product Knowledge smoke PASS.

Product Scope smoke PASS.

Downstream handoff smoke PASS.

Product P0 điểm chặn cleared.

Product smoke evidence accepted.

## 58.5. Product Release Ready Gate

Product chỉ được Release Ready khi:

Specification complete.

Implementation complete.

Test complete.

Smoke pass.

Evidence accepted.

Product P0 cleared.

Foundation dependency clear.

Owner review complete.

Product release scope rõ.

Downstream dependency status rõ.

No stale evidence.

No unresolved Product conflict.

Nếu thiếu một điều kiện:

## 58.6. Product Production Ready Gate

Product chỉ được Production Ready theo scope khi:

Production scope owner decision có hiệu lực.

Product/SKU scope rõ.

Product data approved.

Formula/version approved.

Product Knowledge approved nếu public/AI/Live.

Product Scope approved.

Foundation security/audit clear.

Monitoring/incident owner ready nếu production.

No P0 điểm chặn.

No release hold.

Nếu thiếu một điều kiện:

## 59. PRODUCT FAIL GATE

## 59.1. Product Fail Gate tổng thể

Product domain phải FAIL nếu có một trong các lỗi:

Product và SKU bị trộn lẫn.

SKU và Trade Item bị trộn lẫn.

Ingredient và Raw Lot bị trộn lẫn.

Recipe và Production Snapshot bị trộn lẫn.

Product Active bị hiểu là Sellable.

SKU chưa active vẫn quote/order được.

Recipe draft được production dùng.

Recipe active bị sửa trực tiếp.

Formula version ghi đè lịch sử.

Formula pilot bị dùng nhầm production.

Formula forbidden token dùng operational.

Product Knowledge chưa approved nhưng AI/Live dùng.

Claim public vượt whitelist.

Product Scope không theo kênh.

Live cue SKU ngoài scope.

Ads scale SKU không scope hoặc không sellable.

Product/SKU sale lock/recall vẫn được bán.

Product status thay đổi không audit.

Product production scope không owner decision.

Evidence thiếu hoặc chưa accepted.

## 59.2. Fail Gate theo nhóm

Nhóm

Fail Condition

Impact

Product Master

Product inactive/retired vẫn downstream

Product scope bị chặn

SKU Master

SKU active hiểu là sellable

Commerce/AI bị chặn

Ingredient

Ingredient inactive dùng trong active recipe

Recipe/MRP bị chặn

Recipe

Recipe draft/invalid dùng production

Operational bị chặn

Formula

Formula version overwrite

Trace/history invalid

Activation

One flag active for all scopes

Release bị chặn

Knowledge

Unapproved knowledge used public

AI/Live/Public bị chặn

Scope

No channel-specific scope

Downstream bị chặn

Handoff

Runtime unavailable but downstream proceeds

## P0

Evidence

No accepted evidence

Release bị chặn

## 60. PRODUCT RELEASE HANDOFF PACKAGE

## 60.1. Product Release Handoff là gì

Product Release Handoff Package là gói bàn giao từ dev/QA/tech lead sang owner để xét Product Release Ready.

Nó không tự tạo Release Approved.
Nó chỉ là căn cứ để owner review.

## 60.2. Product Release Handoff Package bắt buộc có

Mục

Nội dung

Product Release Package ID

Mã gói

Scope

Product/SKU/channel/scope được xét

Source Documents

MASTER/PACK/TECH liên quan

Product List

Product trong scope

SKU List

SKU trong scope

Ingredient Impact

Ingredient liên quan

Recipe / Formula Summary

Formula kind/version trong scope

Product Knowledge Status

Approved / WAITING / bị chặn

Product Scope Status

Commerce/AI/Ads/Live/Public

Activation Status

Active by scope

Sellable Dependency Note

Product active chưa phải sellable

Downstream Handoff Summary

Operational/Commerce/AI/Ads/Live

Smoke Summary

Kết quả smoke

Evidence List

Evidence liên quan

P0 điểm chặn Status

OPEN / CLEARED

Owner Questions

Các điểm cần quyết

Excluded Scope

Scope chưa release

Risk Register

Rủi ro

Proposed Status

bị chặn / OWNER_REVIEW_REQUIRED / RELEASE_READY

## 60.3. Không có Release Handoff thì không Release Ready

Nếu thiếu Product Release Handoff Package:

## 61. PRODUCT OWNER REVIEW POINTS

Owner cần review:

Danh sách Product baseline.

Danh sách SKU baseline.

SKU nào thuộc release scope đầu tiên.

SKU nào excluded.

Formula kind/version nào dùng production.

G1/G2/future formula scope.

Recipe line group có đúng không.

Ingredient đặc thù có đúng không.

Product Knowledge public wording có approved không.

Claim public-safe có đạt không.

Product Scope cho Commerce/AI/Ads/Live/Public.

Future SKU onboarding rule.

Product public name.

Product retired/inactive policy.

Downstream dependency status.

Nếu owner chưa review các điểm production:

OWNER_REVIEW_REQUIRED
PRODUCT_RELEASE_READY = KHONG

## 62. PRODUCT SECURITY / DATA / PUBLIC BOUNDARY REVIEW

## 62.1. Product security/data review cần kiểm tra

Public product fields.

Internal product fields.

Internal SKU code exposure.

Ingredient detail exposure.

Recipe/formula detail exposure.

Supplier/source detail exposure.

Cost/accounting exposure.

Product claim exposure.

Public trace identity exposure.

AI/Live/Web content boundary.

Legacy data usage.

Data export permission.

Product audit history.

Product status change permission.

## 62.2. Public boundary rule

Public được thấy:

Public product name.

Public-safe description.

Public-safe ingredient summary.

Usage guidance nếu approved.

Public-safe benefit language.

Public trace whitelist fields nếu scope có.

Public không được thấy:

Internal SKU code nếu không public approved.

Formula chi tiết.

Recipe line detail nếu không allowed.

Supplier internal detail.

Costing.

MISA/accounting mapping.

QC defect/loss/internal note.

Operator/approver.

Internal owner decision.

Evidence nội bộ.

## 63. PRODUCT DOWNSTREAM READINESS RULE

## 63.1. Operational readiness

Operational chỉ được phụ thuộc Product khi:

SKU active theo operational scope.

Recipe/formula active theo scope.

Ingredient master active.

Formula version clear.

Production snapshot source clear.

Product P0 clear.

Evidence accepted.

Nếu không:

## 63.2. Commerce readiness

Commerce chỉ được phụ thuộc Product khi:

SKU active theo Commerce candidate scope.

Product/SKU public identity clear.

Product scope Commerce approved.

Sellable dependency rõ.

Product không sale lock/recall theo upstream.

Price dependency sẽ do Commerce kiểm.

Product P0 clear.

Nếu không:

## 63.3. AI readiness

AI chỉ được phụ thuộc Product khi:

Product Knowledge AI_RENDER_READY.

Public product name approved.

Claim guard mapping clear.

AI product scope approved.

Product not bị chặn.

Product unavailable reason rõ nếu không bán.

Evidence accepted.

Nếu không:

## 63.4. Ads readiness

Ads chỉ được phụ thuộc Product khi:

SKU/product mapping clear.

Ads measurement scope approved.

Revenue mapping dependency clear.

Product not bị chặn.

Sellable/fulfillment dependency clear nếu scale.

Owner decision for scale scope.

Nếu không:

## 63.5. MC AI Live readiness

MC AI Live chỉ được phụ thuộc Product khi:

Live Product Scope approved.

Live script ready knowledge approved.

Claim guard clear.

Product sellable dependency clear nếu live bán hàng.

Product not sale locked/recall.

Owner live scope decision.

Nếu không:

## 63.6. Public Trace readiness

Public Trace chỉ được phụ thuộc Product khi:

Public product identity approved.

Public trace scope approved.

Whitelist fields clear.

No internal leak.

Trace dependency clear.

Evidence accepted.

Nếu không:

## 64. PRODUCT MONITORING / INCIDENT BASELINE

## 64.1. Monitoring tối thiểu

Product production scope cần theo dõi:

Product status changes.

SKU status changes.

Recipe/formula activation.

Product Knowledge publish.

Product Scope changes.

Product public name changes.

Product deactivation/retirement.

Scope excluded/added.

Product P0 điểm chặn created/reopened.

Downstream request bị chặn due to product dependency.

AI attempted to use unapproved knowledge.

Live attempted to cue out-of-scope SKU.

Commerce attempted to quote bị chặn SKU.

Ads attempted to scale bị chặn SKU.

## 64.2. Product incident

Các incident Product cần xử lý ngay:

SKU chưa active được bán.

Active bị hiểu là sellable.

AI nói product knowledge chưa approved.

Live cue SKU ngoài scope.

Ads scale SKU bị bị chặn.

Formula version conflict.

Recipe bị sửa sau khi production dùng.

Public lộ formula/internal detail.

Product scope thay đổi không audit.

Product P0 bị clear không evidence.

Nếu incident chưa phân loại:

## 65. TECH-02 FINAL ACCEPTANCE CRITERIA

TECH-02 được xem là Documentation Complete khi có đủ:

Product Domain Principles.

Product Source-of-Truth.

Product/SKU/Ingredient/Recipe Boundary.

Product Activation vs Sellable Rule.

Formula Governance.

Recipe Line Group Lock.

Product Knowledge Principle.

Product Scope Principle.

Product Master Module Contract.

SKU Master Module Contract.

Ingredient Master Module Contract.

Recipe Module Contract.

Formula Version Module Contract.

Product Activation Module Contract.

Product Knowledge Runtime Module Contract.

Product Scope Registry Module Contract.

Product Expansion / Future SKU Governance.

Lifecycle/State Model.

Command/Query Boundary.

Runtime Handoff Model.

Downstream Dependency Control.

Product Impact Review.

Owner Decision Model.

Exception Handling.

Product Release Status Model.

Final Smoke Matrix.

Product Evidence Package.

Product Done Gate.

Product Fail Gate.

Product Release Handoff Package.

Owner Review Points.

Public/Internal Data Boundary.

Downstream Readiness Rule.

Monitoring / Incident Baseline.

Final Conclusion.

Sau khi TECH-02 hoàn tất tài liệu, trạng thái đúng là:

Status Group

Value

## TECH-02_DOCUMENTATION_STATUS

## COMPLETE

## PRODUCT_SPEC_STATUS

## COMPLETE

## SKU_SPEC_STATUS

## COMPLETE

## INGREDIENT_SPEC_STATUS

## COMPLETE

## RECIPE_SPEC_STATUS

## COMPLETE

## FORMULA_VERSION_SPEC_STATUS

## COMPLETE

## PRODUCT_ACTIVATION_SPEC_STATUS

## COMPLETE

## PRODUCT_KNOWLEDGE_SPEC_STATUS

## COMPLETE

## PRODUCT_SCOPE_SPEC_STATUS

## COMPLETE

## PRODUCT_IMPLEMENTATION_STATUS

## WAITING

## PRODUCT_TEST_STATUS

## WAITING

## PRODUCT_SMOKE_STATUS

## WAITING

## PRODUCT_EVIDENCE_STATUS

## WAITING

## PRODUCT_RELEASE_READY

KHONG

## PRODUCT_RELEASE_DECISION_ACCEPTED

KHONG

## PRODUCT_PRODUCTION_READINESS

KHONG

## DOWNSTREAM_PRODUCT_DEPENDENCY

## BLOCKED_UNTIL_PRODUCT_DOMAIN_CLEAR

## GO_LIVE_DECISION_ACCEPTED

KHONG

## 67. TECH-02 FINAL CONCLUSION

TECH-02 đã khóa:

Product domain là upstream của Operational, Commerce, AI, Facebook, Ads, Live, Public Trace và IVR.

Product không chỉ là danh sách tên sản phẩm.

Product khác SKU.

SKU khác Trade Item.

Ingredient khác Raw Lot.

Recipe khác Production Snapshot.

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa được bán.

Recipe phải có group, ingredient, quantity, UOM, formula kind và version.

Recipe line phải tôn trọng 4 group đã khóa.

Formula Version không được ghi đè lịch sử.

G1/G2/future formula phải có governance.

Production Order phải dùng formula snapshot.

Product Knowledge phải approved trước khi AI/Live/Public dùng.

Public Product Knowledge phải tách khỏi Internal Knowledge.

Product Scope phải theo kênh và theo release scope.

Một flag active duy nhất không được dùng thay mọi scope.

Future SKU phải đi qua onboarding flow.

Product changes quan trọng phải có impact review.

Product command phải có permission, guard, audit, evidence nếu production scope.

Product runtime chỉ cung cấp dữ liệu approved theo scope.

Runtime unavailable thì downstream phải block, không được bịa dữ liệu.

Downstream Product handoff phải theo đúng view: Operational, MRP, Commerce, AI, Facebook, Ads, Live, Public Trace.

Product public data không được lộ công thức, supplier, costing, QC/internal note, personnel, evidence nội bộ.

Product Smoke Matrix là bắt buộc.

Product Evidence Package là bắt buộc.

Product Release Handoff Package là bắt buộc.

Downstream production bị bị chặn nếu Product domain chưa clear.

Documentation Complete không phải Production Ready.

Kết luận cuối:

TECH-02 hoàn tất về mặt tài liệu kỹ thuật Product domain.
TECH-02 chưa đồng nghĩa Product domain đã được triển khai.
TECH-02 chưa đồng nghĩa Product Release Ready.
TECH-02 chưa đồng nghĩa Product Production Ready.
Sau TECH-02, bước tiếp theo là viết TECH-03 - Operational Core / Production / Warehouse / Inventory / Traceability / Recall / Sale Lock theo cùng chuẩn TECH-00, TECH-01 và TECH-02.
Dev chỉ được dùng TECH-02 để lập Product Module Map, SKU Matrix, Ingredient Matrix, Recipe/Formula Matrix, Product Knowledge Plan, Product Scope Plan, Smoke Plan, Evidence Plan và Gap Report trước khi code sâu.

Trạng thái cuối bắt buộc:

## PRODUCT / SKU / INGREDIENT / RECIPE / FORMULA / PRODUCT ACTIVATION
