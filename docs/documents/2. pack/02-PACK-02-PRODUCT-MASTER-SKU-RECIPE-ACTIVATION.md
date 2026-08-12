# PACK-02 - PRODUCT MASTER SKU RECIPE ACTIVATION

## SKU–INGREDIENT–RECIPE–FORMULA VERSION–OPERATIONAL CONFIG CONTROL PACK

PACK-02 sẽ khóa sâu lớp:

- SKU canonical.

- Tên sản phẩm.

- Nhóm sản phẩm.

- Phân loại vegan / mặn.

- Nguyên liệu canonical.

- Công thức G1.

- Version công thức.

- BOM snapshot.

- Operational config theo SKU.

- Packaging config.

- QC config.

- HSD config.

- Trace config.

- Recall config.

- Trade item hộp/thùng.

- GTIN/GS1 boundary.

- Product Activation dependency.

PACK-02 là cầu nối trực tiếp giữa:

20 SKU -> Nguyên liệu -> Công thức G1 -> Operational Config -> Production Order -> Packaging/Trace/Recall -> Sellable/Product Activation.

## SKU–INGREDIENT–RECIPE–FORMULA VERSION–OPERATIONAL CONFIG CONTROL PACK

## PHẦN 1/4 — PRODUCT MASTER PRINCIPLES / SKU CANONICAL / INGREDIENT CANONICAL / OWNER BOUNDARY / NO-HARDCODE RULE

## 1. MỤC TIÊU CỦA PACK-02

PACK-02 thiết lập chuẩn quản trị sản phẩm vận hành cho Ginsengfood, bao gồm Product Master, SKU, nguyên liệu, công thức, phiên bản công thức, BOM, cấu hình vận hành theo SKU và điều kiện kích hoạt sản phẩm.

PACK-02 không phải tài liệu marketing, không phải tài liệu bán hàng, không phải tài liệu giao diện và không phải tài liệu lập trình chi tiết.

PACK-02 có mục tiêu khóa các nguyên tắc nền để mọi module phía sau sử dụng cùng một sự thật sản phẩm, tránh tình trạng mỗi nơi hiểu một kiểu về sản phẩm, nguyên liệu, công thức, đóng gói, in mã, truy xuất, thu hồi và điều kiện mở bán.

## 2. PHẠM VI QUẢN TRỊ CỦA PACK-02

PACK-02 quản trị các lớp sau:

- Product Master.

- SKU canonical.

- Tên sản phẩm nội bộ và tên sản phẩm public.

- Nhóm sản phẩm.

- Phân loại vegan / mặn.

- Ingredient canonical.

- Dược liệu / nguyên liệu / phụ liệu.

- Recipe / BOM / Formula Version.

- G1 operational formula.

- G0 research-origin formula boundary.

- BOM snapshot cho sản xuất.

- Operational config theo SKU.

- Packaging config cấp 1 / cấp 2.

- Print config hộp / thùng.

- QC config.

- HSD config.

- Trace config.

- Recall config.

- Trade item hộp / thùng.

- GTIN/GS1 boundary.

- Product Activation dependency.

- Consumer dependency đối với AI Advisor, CRM, ADS, MC AI, Gateway, Landing Page, Order, Quote, Payment, Shipping, IVR và Public Trace.

## 3. NGUYÊN TẮC CỐT LÕI CỦA PRODUCT MASTER

## 3.1. Product Master là sự thật sản phẩm gốc

Product Master là nơi quản trị danh mục sản phẩm, định danh SKU, tên sản phẩm, nhóm sản phẩm, phân loại, thành phần nền, công thức đang vận hành, trạng thái cấu hình và điều kiện kích hoạt sản phẩm.

Product Master không phải nơi xác nhận hàng có thể bán cuối cùng.

Một SKU có thể tồn tại trong Product Master nhưng chưa đủ điều kiện sản xuất, chưa đủ điều kiện đóng gói, chưa đủ điều kiện in mã, chưa đủ điều kiện nhập kho và chưa đủ điều kiện bán.

## 3.2. Product Master không thay thế Operational Core

Product Master chỉ xác định sản phẩm là gì.

Operational Core xác định sản phẩm đã được sản xuất đúng, QC đúng, release đúng, nhập kho đúng, tồn kho đúng, trace đúng, không bị recall, không bị hold và không bị sale lock hay chưa.

Do đó:

- Product Master Active không đồng nghĩa sellable.

- Recipe Approved không đồng nghĩa batch RELEASED.

- Packaging Config Ready không đồng nghĩa hàng đã nhập kho.

- Print Config Ready không đồng nghĩa mã đã in đúng trên lô thật.

- Trace Config Ready không đồng nghĩa public trace được bật cho mọi lô.

- Product Activation không được vượt qua Sellable Gate của Operational Core.

## 3.3. Product Master là điều kiện cần, không phải điều kiện đủ

Một SKU chỉ được xem là có nền vận hành khi tối thiểu có đủ:

- SKU canonical.

- Tên sản phẩm chuẩn.

- Nhóm sản phẩm.

- Phân loại vegan / mặn.

- Ingredient canonical.

- Recipe / BOM version.

- Formula version.

- Operational config.

- Packaging config.

- Print config.

- QC config.

- HSD config.

- Trace config.

- Recall config.

- Trade item config nếu có cấp hộp / thùng.

- Activation status hợp lệ.

- Audit và evidence cấu hình.

Tuy nhiên, sản phẩm vẫn chưa được bán nếu batch, stock, release, recall, hold hoặc sale lock chưa đạt theo Operational Core.

## 4. 20 SKU CANONICAL LÀ DANH MỤC SẢN PHẨM NỀN

## 4.1. Nguyên tắc 20 SKU canonical

20 SKU canonical là danh mục sản phẩm nền của Ginsengfood.

Mỗi SKU trong 20 SKU phải là một bản ghi sản phẩm riêng biệt, có định danh riêng, tên riêng, nhóm riêng, phân loại riêng, công thức riêng, BOM riêng và cấu hình vận hành riêng.

Không được gom nhiều SKU thành một dòng chung để dùng xuống sản xuất, kho, truy xuất, recall, quote, order hoặc CRM.

## 4.2. Không được dùng SKU mơ hồ

Các cách gọi sau không được dùng làm SKU vận hành:

- “Dòng mùa xuân”.

- “Dòng mùa hè”.

- “Dòng chay”.

- “Dòng mặn”.

- “Dòng bổ dưỡng”.

- “Dòng chức năng”.

- “Cháo Sâm chung”.

- “Sản phẩm nền”.

- “Công thức nền”.

- “BOM chung”.

Các tên trên chỉ có thể là nhóm phân loại hoặc nhóm hiển thị, không được thay cho SKU canonical.

## 4.3. Mỗi SKU phải có block đầu công thức bắt buộc

Mỗi SKU trước khi đi vào công thức G1 phải có block đầu công thức tối thiểu gồm:

- SKU ID.

- SKU code.

- Tên sản phẩm public.

- Tên sản phẩm nội bộ.

- Nhóm sản phẩm.

- Phân loại vegan / mặn.

- Mã công thức.

- Formula version.

- Recipe / BOM version.

- Trạng thái công thức.

- Trạng thái SKU.

- Trạng thái operational config.

- Trạng thái activation.

- Owner phê duyệt.

- Ngày hiệu lực.

- Audit reference.

- Evidence reference.

Nếu thiếu một trong các thông tin bắt buộc này, SKU không được đưa vào vận hành sản xuất thường.

## 5. SKU CANONICAL STANDARD

## 5.1. SKU là đơn vị sản phẩm vận hành

SKU là đơn vị sản phẩm cụ thể được dùng xuyên suốt từ Product Master đến sản xuất, kho, truy xuất, recall, bán hàng, quote, order, CRM và báo cáo.

SKU không được hiểu là tên thương mại chung.

SKU phải đủ rõ để hệ thống biết:

- Sản phẩm nào đang được sản xuất.

- Công thức nào đang được dùng.

- Nguyên liệu nào cần issue.

- Đóng gói theo cấu hình nào.

- In mã theo template nào.

- QC theo tiêu chuẩn nào.

- HSD theo rule nào.

- Trace theo mô hình nào.

- Recall theo logic nào.

- Trade item hộp / thùng nào được áp dụng.

- Sản phẩm có được kích hoạt vận hành hay chưa.

## 5.2. Trường thông tin bắt buộc của SKU canonical

Mỗi SKU canonical phải có tối thiểu các trường quản trị sau:

Nhóm thông tin | Nội dung bắt buộc

Định danh | sku_id, sku_code, product_master_id

Tên gọi | public_product_name, internal_product_name

Phân nhóm | product_group, product_line, product_family

Chế độ ăn | dietary_type: vegan / mặn

Công thức | active_formula_version_id, active_recipe_version_id, active_bom_version_id

Trạng thái | sku_lifecycle_status, activation_status

Vận hành | operational_config_status

Đóng gói | packaging_config_status

In mã | print_config_status

QC | qc_config_status

HSD | shelf_life_config_status

Trace | trace_config_status

Recall | recall_config_status

Trade item | trade_item_config_status

Governance | owner_id, approval_status, audit_status, evidence_status

## 5.3. SKU lifecycle status

SKU phải có trạng thái vòng đời rõ ràng.

Các nhóm trạng thái quản trị tối thiểu:

- Draft.

- waiting Review.

- Approved.

- Config Ready.

- Activation Ready.

- Activated.

- Suspended.

- Retired.

Không module consumer nào được tự diễn giải trạng thái SKU.

Mọi module consumer phải đọc trạng thái thông qua Product Resolver và các guard liên quan.

## 5.4. SKU Activated không đồng nghĩa Sellable

SKU Activated chỉ có nghĩa là sản phẩm đã được phê duyệt đủ điều kiện danh mục và cấu hình sản phẩm.

SKU Activated không có nghĩa là có hàng bán.

Điều kiện sellable cuối cùng phải đi qua Operational Core, Inventory Ledger, Batch Release, Stock Gate, Recall Gate và Sale Lock Gate.

## 6. PRODUCT GROUP STANDARD

## 6.1. Product group là phân loại quản trị, không thay SKU

Product group dùng để phân loại sản phẩm theo chiến lược sản phẩm, vận hành, tư vấn, CRM hoặc báo cáo.

Product group không được dùng thay SKU trong sản xuất, kho, quote, order, truy xuất hoặc recall.

## 6.2. Product group phải được cấu hình trong Product Master

Mỗi SKU phải thuộc một nhóm sản phẩm chính thức.

Nhóm sản phẩm phải do Product Master quản trị, không được hardcode trong AI Advisor, CRM, ADS, Landing Page, Gateway, Order, Quote hoặc báo cáo.

## 6.3. Product group không được tạo quyền bán

Một SKU thuộc nhóm sản phẩm hợp lệ vẫn chưa được bán nếu:

- Chưa có recipe/BOM version.

- Chưa có operational config.

- Chưa có packaging config.

- Chưa có print config.

- Chưa có QC config.

- Chưa có HSD config.

- Chưa có trace/recall config.

- Chưa có batch RELEASED.

- Chưa có tồn kho khả dụng.

- Đang bị hold.

- Đang bị recall.

- Đang bị sale lock.

- Có conflict về QR/barcode/GTIN/GS1.

- Có conflict về audit/evidence.

## 7. VEGAN / MẶN CLASSIFICATION CONTROL

## 7.1. Phân loại vegan / mặn là P0

Phân loại vegan / mặn là dữ liệu bắt buộc của SKU.

Dữ liệu này ảnh hưởng trực tiếp đến:

- Tư vấn sản phẩm.

- Gợi ý thay thế khi hết hàng.

- Combo gia đình.

- CRM chăm sóc sau mua.

- Order.

- Quote.

- Landing Page.

- Public content.

- Complaint handling.

- Health-sensitive guard.

- Dietary restriction guard.

## 7.2. Không được suy luận vegan / mặn từ tên sản phẩm

AI Advisor, CRM, ADS, MC AI, Gateway, Landing Page, Order, Quote hoặc bất kỳ consumer nào không được tự suy luận vegan / mặn từ tên sản phẩm.

Phân loại vegan / mặn phải được đọc từ Product Master thông qua resolver.

## 7.3. Guard bắt buộc theo chế độ ăn

Nếu khách cần sản phẩm thuần chay, hệ thống chỉ được gợi ý SKU có dietary_type phù hợp.

Nếu khách ăn mặn, hệ thống có thể gợi ý SKU mặn hoặc SKU chay nếu phù hợp nhu cầu, nhưng không được tự đánh tráo mục tiêu tư vấn.

Nếu SKU hết hàng, sản phẩm thay thế phải đi qua cùng guard chế độ ăn, dị ứng, kiêng kỵ, health-sensitive và sellable.

## 8. INGREDIENT CANONICAL STANDARD

## 8.1. Ingredient canonical là sự thật nguyên liệu

Ingredient canonical là danh mục nguyên liệu chuẩn dùng trong công thức, sản xuất, QC, truy xuất và recall.

Mỗi nguyên liệu phải có định danh riêng, tên chuẩn, nhóm nguyên liệu, đơn vị tính, trạng thái sử dụng, yêu cầu QC, yêu cầu truy xuất và điều kiện sử dụng.

## 8.2. Không dùng nguyên liệu gom nhóm mơ hồ ở downstream

Các dòng gom nhóm như sau không được dùng ở downstream:

- “Nền chung”.

- “Nước hầm chung”.

- “Nêm chung”.

- “Gia vị chung”.

- “Rau củ chung”.

- “Dược liệu chung”.

- “Phụ liệu chung”.

- “Hỗn hợp nền”.

- “Base công thức”.

- “Nguyên liệu khác”.

Nếu có hỗn hợp hoặc nền công thức, phải bung thành từng nguyên liệu cụ thể trong BOM vận hành, có hàm lượng, đơn vị và vai trò rõ ràng.

## 8.3. Nguyên liệu phải có định danh cụ thể

Mỗi nguyên liệu trong BOM phải có tối thiểu:

- ingredient_id.

- ingredient_code.

- ingredient_canonical_name.

- ingredient_public_name nếu có hiển thị public.

- ingredient_type.

- source_category.

- dietary_compatibility.

- allergen_flag nếu có.

- unit_of_measure.

- usage_status.

- qc_requirement.

- trace_requirement.

- recall_relevance.

- owner approval.

- audit reference.

- evidence reference.

## 8.4. Dược liệu và nguyên liệu thực phẩm phải tách rõ

Dược liệu, nguyên liệu thực phẩm, phụ liệu, gia vị, nước nền, nguyên liệu động vật, nguyên liệu thực vật, hạt, nấm, rong biển, rau củ và nguyên liệu đặc thù phải được phân loại rõ.

Không được gộp tất cả vào một nhóm “nguyên liệu chung”.

Việc tách rõ giúp:

- Sản xuất issue đúng lot.

- QC kiểm tra đúng tiêu chuẩn.

- Trace truy ngược đúng nguồn.

- Recall xác định đúng phạm vi ảnh hưởng.

- AI tư vấn đúng sản phẩm.

- CRM không gợi ý sai chế độ ăn.

- Order không tạo đơn sai sản phẩm.

- Public trace không lộ dữ liệu không được phép.

## 9. SÂM SAVIGIN INGREDIENT BOUNDARY

## 9.1. Sâm Savigin là nguyên liệu trung tâm

Sâm Savigin / Vietnam Ocean Ginseng là nguyên liệu trung tâm của sản phẩm Ginsengfood.

Trong Ingredient Registry, Sâm Savigin phải được quản trị như một nguyên liệu canonical riêng, không được gộp vào nhóm “dược liệu chung” hoặc “nguyên liệu nền”.

## 9.2. Nguồn Sâm Savigin thuộc vùng trồng công ty

Nguồn Sâm Savigin thuộc vùng trồng của công ty.

Source Origin, vùng trồng, lô nguyên liệu, QC lot và readiness cho sản xuất phải đi qua Operational Core.

Product Master không tự xác nhận lô Sâm Savigin đủ điều kiện sản xuất.

## 9.3. Không hardcode hàm lượng Sâm Savigin trong consumer

Hàm lượng, tỷ lệ, đơn vị và vai trò của Sâm Savigin trong từng SKU phải nằm trong Recipe/BOM version được phê duyệt.

AI Advisor, CRM, Landing Page, Gateway, Order, Quote hoặc nội dung public không được tự hardcode hàm lượng nếu không lấy từ registry/resolver được phê duyệt.

## 10. RECIPE / BOM OWNER BOUNDARY Ở TẦNG NGUYÊN TẮC

## 10.1. Recipe/BOM là sự thật công thức vận hành

Recipe/BOM xác định một SKU được tạo ra từ những nguyên liệu nào, hàm lượng bao nhiêu, đơn vị nào và theo phiên bản công thức nào.

Không SKU nào được sản xuất nếu thiếu Recipe/BOM version hợp lệ.

## 10.2. G1 là công thức vận hành

G1 là công thức đang dùng cho vận hành.

G1 không được sửa đè nếu đã được dùng cho sản xuất, snapshot, batch, trace hoặc costing.

Mọi thay đổi sau G1 phải tạo formula version mới.

## 10.3. G0 là lịch sử / công thức gốc nghiên cứu

G0 là vùng lịch sử hoặc công thức gốc nghiên cứu.

G0 không được sửa đè để thay cho G1.

G0 không được dùng trực tiếp làm công thức sản xuất thường nếu chưa được chuẩn hóa thành version vận hành.

## 10.4. Không sửa công thức đã có lịch sử vận hành

Khi một công thức đã được dùng trong Production Order hoặc Batch Snapshot, không được sửa trực tiếp công thức đó.

Mọi thay đổi phải tạo version mới, có owner approval, audit, evidence và ngày hiệu lực.

## 11. OWNER BOUNDARY

## 11.1. Product Master Owner

Product Master Owner chịu trách nhiệm quản trị:

- Danh mục sản phẩm.

- SKU canonical.

- Tên sản phẩm.

- Nhóm sản phẩm.

- Phân loại vegan / mặn.

- Trạng thái SKU.

- Trạng thái activation ở tầng sản phẩm.

- Liên kết SKU với công thức/version/config.

Product Master Owner không được tự pass QC, release batch, tăng inventory hoặc xác nhận sellable cuối cùng.

## 11.2. Recipe / Formula Owner

Recipe / Formula Owner chịu trách nhiệm:

- Công thức G1.

- Formula version.

- Recipe version.

- BOM version.

- Hàm lượng nguyên liệu.

- Đơn vị tính.

- Ingredient mapping.

- Version history.

- Công thức thay thế tương lai.

Recipe / Formula Owner không được tự tạo tồn kho, không được tự mở bán và không được bypass Operational Core.

## 11.3. Operational Core Owner

Operational Core Owner chịu trách nhiệm:

- Production Order.

- BOM snapshot trong sản xuất.

- Material issue theo lot.

- Batch genealogy.

## •	QC.

- Batch Release.

- Warehouse Receipt.

- Inventory Ledger.

- Traceability.

- Recall.

- Hold.

- Sale Lock.

- Sellable Gate.

Operational Core Owner là nơi xác định sản phẩm đã đủ điều kiện vận hành thực tế hay chưa.

## 11.4. Compliance / Claim Owner

Compliance / Claim Owner chịu trách nhiệm:

- Claim được phép dùng.

- Nội dung public-safe.

- Ranh giới không nói như thuốc.

- Tên gọi public.

- Cảnh báo dị ứng / chế độ ăn nếu có.

- Public trace whitelist.

- Dữ liệu không được lộ ra ngoài.

Product Master không được tự sinh claim public ngoài whitelist được phê duyệt.

## 11.5. Consumer Module Owner

Các consumer gồm AI Advisor, CRM, ADS, MC AI, Gateway, Landing Page, Order, Quote, Payment, Shipping, IVR và Public Trace chỉ được đọc dữ liệu sản phẩm thông qua resolver/guard.

Consumer không được:

- Tự tạo SKU.

- Tự đổi tên sản phẩm.

- Tự đổi nhóm sản phẩm.

- Tự suy luận vegan / mặn.

- Tự sửa công thức.

- Tự sửa BOM.

- Tự bật sản phẩm.

- Tự xác nhận sellable.

- Tự bỏ qua sale lock.

- Tự thay thế SKU hết hàng nếu không qua guard.

- Tự hiển thị sản phẩm không hợp lệ.

## 12. PRODUCT RESOLVER PRINCIPLE

## 12.1. Product Resolver là cửa đọc dữ liệu sản phẩm

Mọi module tiêu thụ dữ liệu sản phẩm phải gọi qua Product Resolver hoặc resolver tương đương được governance phê duyệt.

Product Resolver chịu trách nhiệm trả về trạng thái sản phẩm đã được chuẩn hóa, không để consumer tự ghép dữ liệu từ nhiều nguồn.

## 12.2. Resolver không được vượt Guard

Resolver chỉ trả dữ liệu theo quyền và ngữ cảnh.

Nếu sản phẩm chưa đủ điều kiện hiển thị, chưa đủ điều kiện tư vấn, chưa đủ điều kiện quote, chưa đủ điều kiện order hoặc đang bị sale lock, resolver phải trả trạng thái bị chặn hoặc bị giới hạn.

Resolver không được trả “active” nếu sản phẩm chỉ active ở Product Master nhưng chưa đạt điều kiện operational/sellable.

## 12.3. Resolver phải phân biệt các trạng thái

Resolver phải phân biệt tối thiểu:

- Product exists.

- SKU canonical approved.

- Recipe/BOM ready.

- Operational config ready.

- Product activation ready.

- Product activated.

- Batch released.

- Stock available.

- Sellable.

- On hold.

- Sale locked.

- Recalled.

- Suppressed.

- Retired.

Không được trả một trạng thái chung kiểu “available” cho mọi tình huống.

## 13. PRODUCT GUARD PRINCIPLE

## 13.1. Product Guard là lớp chặn sai nghiệp vụ

Product Guard ngăn việc sử dụng SKU sai trạng thái, sai công thức, sai cấu hình, sai chế độ ăn, sai sellable hoặc sai rule vận hành.

Product Guard áp dụng cho cả backend, Admin Web, Field App, AI Advisor, CRM, ADS, Gateway, Landing Page, Order, Quote, Payment, Shipping, IVR và Public Trace.

## 13.2. Các guard bắt buộc

Product Guard tối thiểu phải kiểm tra:

- SKU canonical exists.

- SKU approved.

- Product name approved.

- Product group approved.

- Vegan / mặn classification exists.

- Ingredient canonical complete.

- Recipe/BOM version exists.

- Formula version valid.

- Operational config complete.

- Packaging config complete.

- Print config complete.

- QC config complete.

- HSD config complete.

- Trace config complete.

- Recall config complete.

- Trade item config complete nếu có hộp/thùng.

- Activation status valid.

- Batch/stock/sellable status từ Operational Core.

- Hold/recall/sale lock status.

- Evidence and audit status.

## 13.3. Guard thắng mọi consumer

Nếu Product Guard trả về block, mọi consumer phải dừng hành động tương ứng.

Consumer không được tự downgrade lỗi thành cảnh báo nhẹ.

Consumer không được tiếp tục quote/order/sell nếu guard đã block.

## 14. NO-HARDCODE RULE

## 14.1. No-hardcode là P0

Dữ liệu sản phẩm, công thức và cấu hình vận hành không được hardcode trong consumer.

Các module sau không được hardcode dữ liệu Product Master:

- AI Advisor.

## •	CRM.

## •	ADS.

## •	MC AI.

- Gateway.

- Landing Page.

- Order.

- Quote.

- Payment.

- Shipping.

## •	IVR.

- Public Trace.

- Admin content preview.

- Static template.

- Reporting dashboard.

## 14.2. Các dữ liệu không được hardcode

Các dữ liệu sau phải đến từ registry/resolver/config được phê duyệt:

- Tên sản phẩm.

- Nhóm sản phẩm.

- Phân loại vegan / mặn.

- Ingredient list.

- Hàm lượng nguyên liệu.

- Công thức G1.

- Formula version.

- Recipe version.

- BOM version.

- Packaging config.

- Print config.

- QC config.

- HSD config.

- Trace config.

- Recall config.

- Trade item hộp/thùng.

- GTIN/GS1 nếu có.

- QR rule.

- Barcode rule.

- Product activation status.

- Sellable status.

- Hold status.

- Sale lock status.

- Recall status.

- Replacement product rule.

- Public claim.

- Public trace field.

- Evidence status.

- Smoke status.

## 14.3. Master data khác runtime data

Product Master có thể chứa dữ liệu chuẩn như tên sản phẩm, nhóm sản phẩm, phân loại, công thức và cấu hình.

Tuy nhiên consumer không được copy dữ liệu đó thành logic riêng.

Consumer phải đọc thông qua resolver để bảo đảm dữ liệu luôn theo đúng version, đúng trạng thái, đúng guard và đúng release.

## 15. FIELD APP BOUNDARY ĐỐI VỚI PRODUCT MASTER

## 15.1. Field App chỉ hiển thị theo snapshot

Field Operations Internal App / Mobile PWA chỉ được hiển thị công thức và nguyên liệu theo BOM snapshot đã được backend/domain service cấp cho lệnh sản xuất.

Field App không được cho người dùng chọn tay nguyên liệu trong sản xuất thường.

## 15.2. Field App không được sửa Product Master

Field App không được:

- Tạo SKU.

- Sửa tên sản phẩm.

- Sửa nhóm sản phẩm.

- Sửa phân loại vegan / mặn.

- Sửa ingredient canonical.

- Sửa công thức.

- Sửa BOM.

- Sửa formula version.

- Sửa packaging config.

- Sửa print config.

- Sửa QC config.

- Sửa HSD config.

- Sửa trace config.

- Sửa recall config.

- Tự bật product activation.

- Tự xác nhận sellable.

## 15.3. Field capture không thay Product approval

Ảnh, video, scan, check-in, check-out, click-confirm trong Field App là evidence capture.

Evidence capture không tự làm sản phẩm được approved, không tự làm công thức valid và không tự làm SKU sellable.

## 16. CONSUMER DEPENDENCY PRINCIPLE

## 16.1. Consumer phải phụ thuộc vào Product Master và Operational Core

Mọi consumer khi dùng SKU phải kiểm tra đồng thời:

- Product Master validity.

- Recipe/BOM validity.

- Operational config readiness.

- Product activation status.

- Operational sellable status.

- Hold/recall/sale lock status.

Không consumer nào được chỉ nhìn thấy tên sản phẩm rồi tự cho phép bán.

## 16.2. AI Advisor không được tự tạo tri thức sản phẩm ngoài registry

AI Advisor chỉ được tư vấn sản phẩm dựa trên dữ liệu được Product Resolver trả về.

AI Advisor không được tự bịa:

- Tên SKU.

- Thành phần.

- Công thức.

- Hàm lượng.

- Vegan / mặn.

- Hiệu dụng.

- Giá.

- Tồn kho.

## •	HSD.

- Quy cách đóng gói.

- Trạng thái bán.

- Sản phẩm thay thế.

- Claim khoa học.

- Public trace.

## 16.3. CRM không được gợi ý sản phẩm bị block

CRM không được gợi ý mua lại, nâng hạng, combo, quà sức khỏe hoặc sản phẩm theo mùa nếu SKU chưa đạt guard.

Nếu SKU bị hold, recall, sale lock, out-of-stock hoặc không sellable, CRM phải suppress theo rule.

## 16.4. Order / Quote không được tạo đơn từ SKU không sellable

Order và Quote không được tạo đơn nếu SKU không đạt Product Guard và Sellable Gate.

QuoteSnapshot không được tự hợp thức hóa sản phẩm không đủ điều kiện bán.

## 17. TRACEABILITY ID PRINCIPLE TRONG PACK-02

## 17.1. Product Master phải có định danh trace được

Mọi đối tượng chính trong PACK-02 phải có định danh có thể audit và trace.

Tối thiểu gồm:

- product_master_id.

- sku_id.

- ingredient_id.

- recipe_id.

- formula_version_id.

- bom_version_id.

- operational_config_id.

- packaging_config_id.

- print_config_id.

- qc_config_id.

- shelf_life_config_id.

- trace_config_id.

- recall_config_id.

- trade_item_id.

- product_activation_id.

## 17.2. Human-readable code không thay record identity

SKU code, product code, recipe code hoặc ingredient code là mã hiển thị/đối soát, không thay thế định danh hệ thống.

Không được tái sử dụng định danh đã retired cho sản phẩm khác, công thức khác hoặc nguyên liệu khác.

## 17.3. Mọi version phải audit được

Mỗi lần tạo mới, phê duyệt, kích hoạt, tạm dừng, retired hoặc thay đổi version phải có audit.

Audit tối thiểu phải biết:

- Ai thực hiện.

- Vai trò nào.

- Đối tượng nào.

- Trạng thái trước.

- Trạng thái sau.

- Lý do.

- Thời điểm.

- Evidence.

- Correlation ID.

- Approval reference nếu có.

## 18. AUDIT PRINCIPLE

## 18.1. Product Master không được chỉnh âm thầm

Mọi thay đổi liên quan đến Product Master, SKU, ingredient, recipe, BOM, formula version và operational config đều phải ghi audit.

Không được chỉnh trực tiếp dữ liệu sản phẩm mà không có dấu vết.

## 18.2. Các audit event tối thiểu

PACK-02 yêu cầu tối thiểu các nhóm audit event sau:

- Product created.

- Product updated.

- SKU created.

- SKU approved.

- SKU suspended.

- SKU retired.

- Ingredient created.

- Ingredient approved.

- Ingredient suspended.

- Recipe created.

- Formula version created.

- Formula version approved.

- BOM version approved.

- Operational config approved.

- Packaging config approved.

- Print config approved.

- QC config approved.

- HSD config approved.

- Trace config approved.

- Recall config approved.

- Trade item config approved.

- Product activation requested.

- Product activation approved.

- Product activation suspended.

- Product activation rejected.

- Guard bị chặn.

- Consumer suppressed.

- Evidence attached.

- Smoke passed.

- Smoke failed.

## 19. EVIDENCE PRINCIPLE Ở TẦNG PRODUCT MASTER

## 19.1. Evidence là điều kiện release

Không Product Master, SKU, ingredient, formula version hoặc config nào được gọi là hoàn tất nếu không có evidence tương ứng.

Evidence có thể bao gồm:

- Hồ sơ phê duyệt sản phẩm.

- Hồ sơ công thức.

- Hồ sơ BOM.

- Hồ sơ nguyên liệu.

- Hồ sơ phân loại vegan / mặn.

- Hồ sơ packaging.

- Hồ sơ print.

- Hồ sơ QC.

- Hồ sơ HSD.

- Hồ sơ trace.

- Hồ sơ recall.

- Hồ sơ trade item.

- Hồ sơ owner review.

- Smoke evidence.

## 19.2. Không evidence thì không PASS

Nếu chưa có evidence, trạng thái phải là waiting_EVIDENCE.

Không được gọi PASS dựa trên giả định, nội dung mô tả hoặc ý kiến chưa có bằng chứng.

## 20. RELEASE BOUNDARY CỦA PACK-02 PHẦN 1

## 20.1. Điều kiện đạt của PHẦN 1

PHẦN 1 được xem là đạt về mặt governance khi đã khóa được:

- Product Master là source-of-truth sản phẩm.

- Product Master không thay Operational Core.

- Product Master Active không đồng nghĩa Sellable.

- 20 SKU canonical là danh mục nền.

- Mỗi SKU là một bản ghi riêng.

- SKU không được mơ hồ.

- Mỗi SKU phải có block đầu công thức bắt buộc.

- Product group không thay SKU.

- Vegan / mặn là P0.

- Ingredient canonical là bắt buộc.

- Không dùng nguyên liệu gom nhóm mơ hồ ở downstream.

- Sâm Savigin là nguyên liệu canonical riêng.

- G1 là công thức vận hành.

- G0 là lịch sử/công thức gốc nghiên cứu.

- Mọi thay đổi sau G1 phải tạo version mới.

- Owner boundary được phân định rõ.

- Resolver / Guard là bắt buộc.

- Field App không được sửa Product Master.

- Consumer không được hardcode dữ liệu sản phẩm.

- Audit và evidence là điều kiện bắt buộc.

## 20.2. Trạng thái sau PHẦN 1

Sau PHẦN 1, PACK-02 mới khóa tầng nguyên tắc và ranh giới owner.

PACK-02 chưa được xem là hoàn tất cho đến khi hoàn thành:

- PHẦN 2/4 — Recipe / BOM / Formula Version Registry / 20 SKU G1 Structure / Snapshot Control.

- PHẦN 3/4 — Operational Config / Packaging Config / Print Config / QC Config / HSD / Trace / Recall / Trade Item Control.

- PHẦN 4/4 — Product Activation Dependency / Evidence / Smoke / Done Gate / PACK-02 Final Conclusion.

## PHẦN 2/4 — RECIPE / BOM / FORMULA VERSION REGISTRY / 20 SKU G1 STRUCTURE / SNAPSHOT CONTROL

## 21. MỤC TIÊU CỦA PHẦN 2

PHẦN 2 thiết lập chuẩn quản trị công thức, BOM, phiên bản công thức và cơ chế snapshot cho 20 SKU nền của Ginsengfood.

Mục tiêu chính là bảo đảm mỗi SKU khi đi vào sản xuất đều có công thức vận hành rõ ràng, version rõ ràng, nguyên liệu rõ ràng, hàm lượng rõ ràng, trạng thái phê duyệt rõ ràng và khả năng truy vết đầy đủ.

PHẦN 2 khóa nguyên tắc:

- Không sản xuất SKU nếu thiếu recipe/BOM version.

- Không sửa đè công thức đã dùng trong sản xuất.

- Không dùng công thức nghiên cứu G0 trực tiếp cho vận hành thường.

- Công thức vận hành hiện tại là G1.

- Mọi thay đổi sau G1 phải tạo version mới.

- BOM dùng trong sản xuất phải được snapshot.

- Field App chỉ hiển thị theo BOM snapshot.

- Không cho chọn tay nguyên liệu trong sản xuất thường.

- Không dùng dòng gom nhóm mơ hồ ở downstream.

- 20 SKU canonical phải có cấu trúc G1 đầy đủ.

## 22. RECIPE / BOM / FORMULA VERSION LÀ LỚP KIỂM SOÁT TRỌNG YẾU

Recipe, BOM và Formula Version là lớp trung tâm kết nối Product Master với sản xuất thực tế.

Product Master xác định sản phẩm là gì.

Recipe/BOM xác định sản phẩm được tạo ra từ những nguyên liệu nào, tỷ lệ nào, đơn vị nào, theo công thức nào.

Operational Core xác định công thức đó được đưa vào lệnh sản xuất nào, issue nguyên liệu nào, tạo batch nào, QC ra sao, release ra sao và nhập kho ra sao.

Nếu thiếu Recipe/BOM/Formula Version, SKU không được phép đi vào sản xuất.

## 23. ĐỊNH NGHĨA RECIPE

Recipe là bản mô tả công thức vận hành của một SKU.

Recipe xác định:

- SKU áp dụng.

- Nhóm sản phẩm.

- Phân loại vegan / mặn.

- Formula version.

- Recipe version.

- BOM version.

- Danh sách nguyên liệu.

- Hàm lượng từng nguyên liệu.

- Đơn vị tính.

- Vai trò nguyên liệu.

- Điều kiện sử dụng.

- Trạng thái phê duyệt.

- Ngày hiệu lực.

- Owner phê duyệt.

- Audit reference.

- Evidence reference.

Recipe không được là ghi chú tự do.

Recipe không được là mô tả marketing.

Recipe không được là nội dung tư vấn bán hàng.

Recipe là đối tượng vận hành có kiểm soát.

## 24. ĐỊNH NGHĨA BOM

BOM là danh mục nguyên liệu định lượng dùng để sản xuất một SKU theo một recipe/version cụ thể.

BOM phải trả lời rõ:

- Dùng nguyên liệu nào?

- Mỗi nguyên liệu dùng bao nhiêu?

- Đơn vị tính là gì?

- Nguyên liệu đó thuộc nhóm nào?

- Có cần QC trước khi issue không?

- Có cần lot tracking không?

- Có ảnh hưởng vegan / mặn không?

- Có ảnh hưởng allergen không?

- Có ảnh hưởng trace / recall không?

- Có được dùng trong version hiện tại không?

BOM không được dùng các dòng mơ hồ như “nước hầm chung”, “nêm chung”, “nền chung”, “gia vị chung”, “rau củ chung”, “dược liệu chung” nếu không bung thành từng nguyên liệu cụ thể.

## 25. ĐỊNH NGHĨA FORMULA VERSION

Formula Version là phiên bản công thức được quản trị theo vòng đời.

Mỗi formula version phải có:

- formula_version_id.

- formula_code.

- version_label.

- sku_id.

- recipe_version_id.

- bom_version_id.

- version_type.

- lifecycle_status.

- effective_from.

- effective_to nếu có.

- approval_status.

- owner_id.

- audit_reference.

- evidence_reference.

Formula Version là lớp bắt buộc để bảo vệ lịch sử sản xuất.

Không được sửa trực tiếp một formula version đã được dùng trong Production Order, Batch Snapshot, QC, trace hoặc costing.

## 26. G0 / G1 / G2 / G3 VERSION PRINCIPLE

## 26.1. G0 là công thức gốc / lịch sử nghiên cứu

G0 là vùng lịch sử hoặc công thức gốc nghiên cứu.

G0 có giá trị làm nền tham chiếu khoa học, R&D và chuẩn hóa ban đầu.

G0 không phải công thức vận hành thường nếu chưa được chuẩn hóa thành version vận hành.

G0 không được sửa đè để tạo cảm giác đang cập nhật công thức vận hành.

## 26.2. G1 là công thức vận hành hiện tại

G1 là công thức đang dùng cho vận hành chính thức.

G1 là version bắt buộc trong giai đoạn sản xuất nền.

Mỗi SKU trong 20 SKU canonical phải có cấu trúc G1 riêng.

Không SKU nào được đi vào sản xuất thường nếu chưa có G1 hợp lệ hoặc chưa có version vận hành được owner phê duyệt.

## 26.3. G2 / G3 / version sau G1

Mọi thay đổi công thức sau G1 phải tạo version mới.

Các version mới có thể là:

- Điều chỉnh hàm lượng.

- Điều chỉnh nguyên liệu.

- Thay đổi nguồn nguyên liệu.

- Thay đổi tỷ lệ.

- Thay đổi tiêu chuẩn vận hành.

- Thay đổi quy cách sản xuất.

- Thay đổi để phù hợp mùa vụ, nguồn cung hoặc cải tiến chất lượng.

Mỗi version mới phải có owner approval, audit, evidence và ngày hiệu lực.

Không được thay đổi âm thầm trên G1 đã dùng trong sản xuất.

## 27. RECIPE LIFECYCLE STATUS

Recipe phải có trạng thái vòng đời rõ ràng.

Các trạng thái tối thiểu:

- Draft.

- waiting Review.

- Approved.

- Active.

- Superseded.

- Suspended.

- Retired.

Ý nghĩa quản trị:

- Draft: chưa được dùng trong vận hành.

- waiting Review: đang chờ phê duyệt.

- Approved: đã được phê duyệt nhưng chưa chắc đã active.

- Active: version đang có hiệu lực vận hành.

- Superseded: đã được thay bằng version mới.

- Suspended: tạm dừng sử dụng.

- Retired: ngừng sử dụng vĩnh viễn.

Không module consumer nào được tự diễn giải trạng thái recipe.

## 28. BOM LIFECYCLE STATUS

BOM phải có trạng thái vòng đời riêng.

Các trạng thái tối thiểu:

- Draft.

- waiting Review.

- Approved.

- Active.

- Locked By Production Use.

- Superseded.

- Suspended.

- Retired.

Khi BOM đã được sử dụng trong Production Order, BOM đó phải được khóa lịch sử.

Không được sửa trực tiếp BOM đã bị khóa bởi sản xuất.

## 29. FORMULA VERSION LIFECYCLE STATUS

Formula Version phải có trạng thái riêng.

Các trạng thái tối thiểu:

- Research Reference.

- Draft.

- waiting Review.

- Approved.

- Active For Production.

- Locked By Production Use.

- Superseded.

- Suspended.

- Retired.

G0 thường thuộc nhóm Research Reference.

G1 thuộc nhóm Active For Production khi đã được phê duyệt vận hành.

Khi G1 đã đi vào Production Order, G1 phải có trạng thái Locked By Production Use đối với lịch sử đã phát sinh.

## 30. RECIPE / BOM / FORMULA OWNER BOUNDARY

## 30.1. Recipe Owner

Recipe Owner chịu trách nhiệm nội dung công thức.

Recipe Owner quyết định:

- Nguyên liệu nào thuộc công thức.

- Hàm lượng từng nguyên liệu.

- Vai trò từng nguyên liệu.

- Đơn vị tính.

- Phiên bản công thức.

- Ngày hiệu lực.

- Lý do thay đổi version.

Recipe Owner không được tự xác nhận batch đạt QC, không được tự nhập kho và không được tự mở bán.

## 30.2. BOM Owner

BOM Owner chịu trách nhiệm cấu trúc nguyên liệu định lượng dùng cho sản xuất.

BOM Owner phải bảo đảm:

- Không có nguyên liệu mơ hồ.

- Không thiếu hàm lượng.

- Không thiếu đơn vị.

- Không thiếu ingredient canonical.

- Không thiếu lot tracking flag nếu cần.

- Không thiếu dietary/allergen/trace/recall flag.

- Không dùng nguyên liệu chưa approved.

## 30.3. Formula Version Owner

Formula Version Owner chịu trách nhiệm version công thức.

Formula Version Owner phải bảo đảm:

- Mỗi version có định danh riêng.

- Mỗi version có trạng thái rõ.

- Mỗi version có ngày hiệu lực.

- Mỗi version có approval.

- Mỗi version có audit.

- Mỗi version có evidence.

- Version cũ không bị sửa đè sau khi đã dùng.

## 30.4. Operational Core Owner

Operational Core Owner chỉ sử dụng recipe/BOM/formula version đã được phê duyệt.

Operational Core Owner chịu trách nhiệm snapshot recipe/BOM vào Production Order.

Operational Core không được tự sửa công thức gốc.

## 31. CẤU TRÚC BẮT BUỘC CỦA MỖI G1 SKU

Mỗi SKU trong 20 SKU canonical phải có cấu trúc G1 tối thiểu như sau:

Nhóm thông tin | Nội dung bắt buộc

Định danh SKU | sku_id, sku_code

Tên sản phẩm | public_product_name, internal_product_name

Phân nhóm | product_group, product_line, product_family

Chế độ ăn | dietary_type: vegan / mặn

Công thức | formula_code, formula_version_label = G1

Recipe | recipe_id, recipe_version_id

BOM | bom_id, bom_version_id

Thành phần | ingredient lines đầy đủ

Hàm lượng | quantity, unit_of_measure

Vai trò | ingredient_role

Trạng thái | approval_status, lifecycle_status

Hiệu lực | effective_from, effective_to nếu có

Owner | recipe_owner, formula_owner

Governance | audit_reference, evidence_reference

Không SKU nào được thiếu block G1 nếu muốn đi vào sản xuất thường.

## 32. 20 SKU G1 STRUCTURE REGISTRY

## 32.1. Nguyên tắc registry

20 SKU G1 Structure Registry là bảng khóa cấu trúc công thức vận hành nền cho toàn bộ danh mục sản phẩm.

Registry này không thay thế BOM chi tiết nhưng bắt buộc xác định mỗi SKU có đủ khung công thức G1.

Mỗi SKU phải có một dòng riêng.

Không được gộp nhóm nhiều SKU vào một dòng chung.

## 32.2. Cấu trúc registry tối thiểu

STT | SKU Canonical | Tên sản phẩm public | Nhóm sản phẩm | Vegan / Mặn | Formula Version | Recipe Version | BOM Version | Status

01 | SKU-01 | {{public_product_name_01}} | {{product_group_01}} | {{dietary_type_01}} | G1 | {{recipe_version_01}} | {{bom_version_01}} | waiting_CONFIG / READY

02 | SKU-02 | {{public_product_name_02}} | {{product_group_02}} | {{dietary_type_02}} | G1 | {{recipe_version_02}} | {{bom_version_02}} | waiting_CONFIG / READY

03 | SKU-03 | {{public_product_name_03}} | {{product_group_03}} | {{dietary_type_03}} | G1 | {{recipe_version_03}} | {{bom_version_03}} | waiting_CONFIG / READY

04 | SKU-04 | {{public_product_name_04}} | {{product_group_04}} | {{dietary_type_04}} | G1 | {{recipe_version_04}} | {{bom_version_04}} | waiting_CONFIG / READY

05 | SKU-05 | {{public_product_name_05}} | {{product_group_05}} | {{dietary_type_05}} | G1 | {{recipe_version_05}} | {{bom_version_05}} | waiting_CONFIG / READY

06 | SKU-06 | {{public_product_name_06}} | {{product_group_06}} | {{dietary_type_06}} | G1 | {{recipe_version_06}} | {{bom_version_06}} | waiting_CONFIG / READY

07 | SKU-07 | {{public_product_name_07}} | {{product_group_07}} | {{dietary_type_07}} | G1 | {{recipe_version_07}} | {{bom_version_07}} | waiting_CONFIG / READY

08 | SKU-08 | {{public_product_name_08}} | {{product_group_08}} | {{dietary_type_08}} | G1 | {{recipe_version_08}} | {{bom_version_08}} | waiting_CONFIG / READY

09 | SKU-09 | {{public_product_name_09}} | {{product_group_09}} | {{dietary_type_09}} | G1 | {{recipe_version_09}} | {{bom_version_09}} | waiting_CONFIG / READY

10 | SKU-10 | {{public_product_name_10}} | {{product_group_10}} | {{dietary_type_10}} | G1 | {{recipe_version_10}} | {{bom_version_10}} | waiting_CONFIG / READY

11 | SKU-11 | {{public_product_name_11}} | {{product_group_11}} | {{dietary_type_11}} | G1 | {{recipe_version_11}} | {{bom_version_11}} | waiting_CONFIG / READY

12 | SKU-12 | {{public_product_name_12}} | {{product_group_12}} | {{dietary_type_12}} | G1 | {{recipe_version_12}} | {{bom_version_12}} | waiting_CONFIG / READY

13 | SKU-13 | {{public_product_name_13}} | {{product_group_13}} | {{dietary_type_13}} | G1 | {{recipe_version_13}} | {{bom_version_13}} | waiting_CONFIG / READY

14 | SKU-14 | {{public_product_name_14}} | {{product_group_14}} | {{dietary_type_14}} | G1 | {{recipe_version_14}} | {{bom_version_14}} | waiting_CONFIG / READY

15 | SKU-15 | {{public_product_name_15}} | {{product_group_15}} | {{dietary_type_15}} | G1 | {{recipe_version_15}} | {{bom_version_15}} | waiting_CONFIG / READY

16 | SKU-16 | {{public_product_name_16}} | {{product_group_16}} | {{dietary_type_16}} | G1 | {{recipe_version_16}} | {{bom_version_16}} | waiting_CONFIG / READY

17 | SKU-17 | {{public_product_name_17}} | {{product_group_17}} | {{dietary_type_17}} | G1 | {{recipe_version_17}} | {{bom_version_17}} | waiting_CONFIG / READY

18 | SKU-18 | {{public_product_name_18}} | {{product_group_18}} | {{dietary_type_18}} | G1 | {{recipe_version_18}} | {{bom_version_18}} | waiting_CONFIG / READY

19 | SKU-19 | {{public_product_name_19}} | {{product_group_19}} | {{dietary_type_19}} | G1 | {{recipe_version_19}} | {{bom_version_19}} | waiting_CONFIG / READY

20 | SKU-20 | {{public_product_name_20}} | {{product_group_20}} | {{dietary_type_20}} | G1 | {{recipe_version_20}} | {{bom_version_20}} | waiting_CONFIG / READY

## 33. KHÓA NGUYÊN TẮC KHÔNG DÙNG PLACEHOLDER Ở PRODUCTION RUNTIME

Các biến dạng {{public_product_name_01}}, {{recipe_version_01}}, {{bom_version_01}} trong tài liệu governance chỉ biểu thị trường bắt buộc phải có trong registry.

Khi triển khai dữ liệu thật, không được để placeholder đi vào production runtime.

Production runtime phải có dữ liệu thật đã được owner phê duyệt.

Nếu còn placeholder, trạng thái phải là waiting_CONFIG hoặc waiting_EVIDENCE, không được READY, không được ACTIVE và không được SELLABLE.

## 34. RECIPE HEADER BẮT BUỘC CHO MỖI SKU

Mỗi recipe G1 phải bắt đầu bằng Recipe Header.

Recipe Header tối thiểu gồm:

Trường | Yêu cầu

sku_id | Bắt buộc

sku_code | Bắt buộc

public_product_name | Bắt buộc

internal_product_name | Bắt buộc

product_group | Bắt buộc

dietary_type | Bắt buộc

formula_code | Bắt buộc

formula_version | Bắt buộc, G1 đối với version vận hành nền

recipe_version_id | Bắt buộc

bom_version_id | Bắt buộc

recipe_status | Bắt buộc

formula_status | Bắt buộc

effective_from | Bắt buộc

approved_by | Bắt buộc khi active

approved_at | Bắt buộc khi active

audit_reference | Bắt buộc

evidence_reference | Bắt buộc

Nếu thiếu Recipe Header, recipe không hợp lệ.

## 35. BOM LINE BẮT BUỘC CHO MỖI NGUYÊN LIỆU

Mỗi dòng BOM phải có tối thiểu:

Trường | Yêu cầu

bom_line_id | Bắt buộc

ingredient_id | Bắt buộc

ingredient_code | Bắt buộc

ingredient_canonical_name | Bắt buộc

ingredient_type | Bắt buộc

ingredient_role | Bắt buộc

quantity | Bắt buộc

unit_of_measure | Bắt buộc

loss_allowance_rule | Nếu có áp dụng

lot_tracking_required | Bắt buộc

qc_required_before_issue | Bắt buộc

dietary_relevance | Bắt buộc

allergen_relevance | Nếu có

trace_relevance | Bắt buộc

recall_relevance | Bắt buộc

status | Bắt buộc

audit_reference | Bắt buộc

Không được có BOM line không gắn ingredient_id.

Không được có BOM line chỉ ghi text mô tả.

## 36. INGREDIENT ROLE STANDARD

Mỗi nguyên liệu trong BOM phải có vai trò rõ ràng.

Các nhóm vai trò tối thiểu:

- Core Functional Ingredient.

- Main Food Ingredient.

- Supporting Food Ingredient.

- Broth / Liquid Base Ingredient.

- Seasoning Ingredient.

- Texture Ingredient.

- Nutritional Support Ingredient.

- Processing Support Ingredient.

- Packaging-Relevant Ingredient nếu có.

- Trace-Critical Ingredient.

Vai trò nguyên liệu không được dùng thay cho tên nguyên liệu.

Ví dụ “seasoning” không được là một nguyên liệu.

Phải ghi rõ đó là muối, đường, gia vị cụ thể hoặc nguyên liệu cụ thể nào.

## 37. UNIT OF MEASURE STANDARD

Mọi BOM line phải có đơn vị đo lường chuẩn.

Không được dùng đơn vị mơ hồ như:

- “Một ít”.

- “Vừa đủ”.

- “Tùy chỉnh”.

- “Theo cảm quan”.

- “Theo kinh nghiệm”.

- “Nêm vừa”.

- “Khoảng chừng”.

Nếu cần rule cảm quan trong sản xuất, rule đó phải nằm ở work instruction hoặc QC sensory guidance, không thay thế BOM định lượng.

## 38. QUANTITY CONTROL

## 38.1. Hàm lượng là bắt buộc

Mỗi nguyên liệu phải có quantity rõ ràng.

Quantity phải gắn với unit_of_measure.

Không có quantity thì BOM line không hợp lệ.

## 38.2. Không sửa quantity trực tiếp sau khi active

Nếu cần đổi hàm lượng nguyên liệu trong công thức active, phải tạo BOM version mới hoặc formula version mới theo rule phê duyệt.

Không được sửa trực tiếp quantity trong version đã active và đã dùng cho sản xuất.

## 38.3. Quantity trong snapshot là lịch sử bất biến

Khi Production Order được tạo, quantity trong BOM snapshot là lịch sử cho lệnh sản xuất đó.

Nếu sau này recipe/BOM thay đổi, Production Order cũ vẫn giữ snapshot cũ.

## 39. DIETARY CONTROL TRONG RECIPE/BOM

## 39.1. Vegan / mặn phải nhất quán

Phân loại vegan / mặn của SKU phải nhất quán với ingredient list.

Một SKU được đánh dấu vegan không được chứa nguyên liệu động vật hoặc nguyên liệu không phù hợp vegan.

Nếu ingredient thay đổi làm ảnh hưởng dietary_type, recipe/BOM phải bị block cho đến khi owner review.

## 39.2. Không để AI tự suy luận dietary

AI Advisor, CRM, ADS, Gateway, Landing Page, Order và Quote không được tự đọc ingredient name để suy luận vegan / mặn.

Dietary status phải do Product Resolver trả về.

## 39.3. Dietary conflict là lỗi P0

Nếu có xung đột giữa SKU dietary_type và BOM ingredient, SKU phải bị chặn activation.

Không được đưa SKU vào sản xuất, tư vấn, quote hoặc order khi dietary conflict chưa được xử lý.

## 40. ALLERGEN / RESTRICTION CONTROL

Mỗi ingredient phải có flag liên quan đến dị ứng hoặc hạn chế sử dụng nếu có.

Các thông tin này ảnh hưởng đến:

- AI Advisor.

## •	CRM.

- Order.

- Public content.

- Complaint handling.

- Health-sensitive guard.

- Replacement product logic.

Nếu thiếu allergen/restriction flag đối với nguyên liệu có rủi ro, SKU không được gọi là Product Knowledge Ready cho consumer.

## 41. TRACE RELEVANCE TRONG BOM

Mỗi nguyên liệu phải có trace_relevance.

Trace relevance xác định nguyên liệu có cần truy vết theo lot, nguồn, nhà cung cấp, vùng trồng, batch hoặc evidence hay không.

Nguyên liệu trace-critical không được issue nếu thiếu lot hợp lệ.

Không được dùng BOM không có trace relevance để sản xuất sản phẩm có public trace hoặc recall requirement.

## 42. RECALL RELEVANCE TRONG BOM

Mỗi nguyên liệu phải có recall_relevance.

Recall relevance giúp xác định phạm vi thu hồi khi phát hiện vấn đề.

Nếu một nguyên liệu có liên quan trực tiếp đến chất lượng, an toàn, nguồn gốc, claim, chế độ ăn hoặc truy xuất, nguyên liệu đó phải có recall relevance.

Không được để nguyên liệu quan trọng ngoài phạm vi recall.

## 43. LOT-BASED BOM CONTROL

Các nguyên liệu cần quản trị theo lot phải được đánh dấu lot_tracking_required.

Khi tạo Production Order, BOM snapshot phải đi kèm yêu cầu issue theo lot.

Không được issue nguyên liệu trace-critical theo cách tổng kho không lot.

Không được sản xuất batch không truy ngược được raw material lots.

## 44. QC REQUIRED BEFORE ISSUE

Một số nguyên liệu phải QC trước khi issue vào sản xuất.

Nếu BOM line có qc_required_before_issue = true, chỉ lot đạt trạng thái QC phù hợp và readiness phù hợp mới được issue.

Không được dùng nguyên liệu chưa đạt QC vào sản xuất thường.

## 45. RECIPE / BOM SNAPSHOT CONTROL

## 45.1. Snapshot là bắt buộc khi tạo Production Order

Khi tạo Production Order, hệ thống phải chụp snapshot recipe/BOM/version đang active tại thời điểm tạo lệnh.

Snapshot phải gắn với Production Order.

Production Order không được chỉ tham chiếu động đến recipe hiện tại mà không có snapshot.

## 45.2. Snapshot bảo vệ lịch sử sản xuất

Nếu sau này công thức thay đổi, Production Order cũ vẫn phải giữ công thức tại thời điểm sản xuất.

Không được cập nhật ngược Production Order cũ theo công thức mới.

## 45.3. Snapshot là nguồn hiển thị cho Field App

Field App chỉ được hiển thị nguyên liệu, hàm lượng và thao tác theo BOM snapshot đã được cấp cho Production Order.

Field App không được lấy dữ liệu trực tiếp từ recipe active hiện tại nếu Production Order đã có snapshot.

## 46. SNAPSHOT CONTENT MINIMUM

Một BOM snapshot trong Production Order tối thiểu phải chứa:

- production_order_id.

- sku_id.

- sku_code.

- public_product_name.

- internal_product_name.

- product_group.

- dietary_type.

- formula_version_id.

- formula_version_label.

- recipe_version_id.

- bom_version_id.

- ingredient list.

- quantity.

- unit_of_measure.

- ingredient_role.

- lot_tracking_required.

- qc_required_before_issue.

- trace_relevance.

- recall_relevance.

- snapshot_created_at.

- snapshot_created_by.

- audit_reference.

- correlation_id.

## 47. SNAPSHOT IMMUTABILITY

BOM snapshot sau khi gắn vào Production Order không được sửa trực tiếp.

Nếu cần điều chỉnh do lỗi trước khi sản xuất, phải có quy trình hủy lệnh, tạo lệnh mới hoặc correction có audit rõ ràng.

Không được chỉnh âm thầm snapshot.

Không được thay đổi snapshot sau khi đã issue nguyên liệu, bắt đầu sản xuất hoặc tạo batch.

## 48. SNAPSHOT VÀ COSTING

BOM snapshot là một trong các nguồn để phục vụ tính giá thành, đối soát nguyên liệu và liên kết kế toán.

Tuy nhiên, PACK-02 không tính giá thành và không điều khiển kế toán.

PACK-02 chỉ bảo đảm recipe/BOM snapshot có đủ dữ liệu chuẩn để các pack/module liên quan sử dụng đúng.

## 49. SNAPSHOT VÀ TRACEABILITY

BOM snapshot phải là một phần của chuỗi truy xuất.

Traceability không được chỉ dựa trên tên sản phẩm hoặc batch code.

Traceability phải biết:

- Batch được sản xuất từ Production Order nào.

- Production Order dùng SKU nào.

- SKU dùng recipe/BOM version nào.

- Recipe/BOM snapshot có nguyên liệu nào.

- Nguyên liệu được issue từ lot nào.

- Lot nguyên liệu đến từ nguồn nào.

## 50. SNAPSHOT VÀ RECALL

Recall phải dựa trên recipe/BOM snapshot và genealogy thật.

Khi có vấn đề với một nguyên liệu, hệ thống phải xác định được:

- Nguyên liệu đó nằm trong BOM nào.

- Đã được dùng trong Production Order nào.

- Đã tạo batch nào.

- Batch đó đã đóng gói ra đơn vị nào.

- Hàng đã nhập kho, bán, giao, hoặc còn tồn ở đâu.

- SKU nào phải hold, sale lock hoặc recall.

Không được recall dựa trên cảm tính hoặc danh sách sản phẩm thủ công.

## 51. KHÔNG CHO CHỌN TAY NGUYÊN LIỆU TRONG SẢN XUẤT THƯỜNG

Trong sản xuất thường, người vận hành không được chọn tay nguyên liệu thay cho BOM.

Người vận hành chỉ được xác nhận, scan, capture evidence hoặc thao tác theo BOM snapshot.

Nếu cần thay nguyên liệu, phải đi qua exception flow, owner approval, audit và version/correction control.

Không được để Field App hoặc Admin Web cho phép bypass BOM snapshot trong sản xuất thường.

## 52. RECIPE EXCEPTION CONTROL

Có thể phát sinh tình huống ngoại lệ như thiếu nguyên liệu, đổi nguồn nguyên liệu, thay thế nguyên liệu, lỗi BOM hoặc điều chỉnh công thức.

Các ngoại lệ này không được xử lý bằng cách sửa tay trong lệnh sản xuất.

Mọi ngoại lệ phải đi qua:

- Exception request.

- Reason bắt buộc.

- Owner review.

- Risk assessment.

- Approval nếu được phép.

- Audit.

- Evidence.

- Version hoặc correction path phù hợp.

- Guard kiểm tra lại trước khi tiếp tục vận hành.

Nếu ngoại lệ ảnh hưởng chất lượng, chế độ ăn, allergen, trace, recall hoặc claim, SKU/batch phải bị hold cho đến khi có quyết định hợp lệ.

## 53. FORMULA CHANGE CONTROL

## 53.1. Khi nào phải tạo version mới

Phải tạo version mới khi có thay đổi:

- Thêm nguyên liệu.

- Bỏ nguyên liệu.

- Đổi hàm lượng.

- Đổi đơn vị.

- Đổi loại nguyên liệu.

- Đổi vai trò nguyên liệu.

- Đổi dietary impact.

- Đổi allergen impact.

- Đổi trace relevance.

- Đổi recall relevance.

- Đổi QC requirement.

- Đổi tiêu chuẩn vận hành liên quan công thức.

- Đổi cấu trúc G1 thành version kế tiếp.

## 53.2. Không được gọi thay đổi lớn là chỉnh sửa nhỏ

Nếu thay đổi làm ảnh hưởng sản xuất, QC, trace, recall, claim, chế độ ăn, public content hoặc costing, đó là thay đổi có kiểm soát.

Không được ghi nhận như sửa lỗi text thông thường.

## 53.3. Effective date bắt buộc

Mỗi version mới phải có ngày hiệu lực.

Không được để nhiều formula version active cùng lúc cho cùng một SKU và cùng một ngữ cảnh vận hành, trừ khi governance cho phép phân tách rõ theo formula_kind hoặc production context.

## 54. VERSION UNIQUENESS CONTROL

Mỗi SKU không được có nhiều G1 active cùng lúc cho cùng một formula_kind.

Nếu có nhiều công thức phục vụ mục đích khác nhau, phải phân biệt rõ:

- formula_kind.

- production_context.

- effective_date.

- owner approval.

- activation rule.

- usage boundary.

Không được để dev, admin hoặc người vận hành tự chọn version theo cảm tính.

## 55. RECIPE/BOM APPROVAL GATE

Một Recipe/BOM chỉ được Approved khi đạt đủ:

- SKU canonical exists.

- SKU approved.

- Product group valid.

- Dietary type valid.

- Ingredient canonical complete.

- Không có ingredient mơ hồ.

- Quantity đầy đủ.

- Unit đầy đủ.

- Lot tracking flag đầy đủ.

- QC requirement đầy đủ.

- Trace relevance đầy đủ.

- Recall relevance đầy đủ.

- Allergen/restriction flag nếu có.

- Formula version hợp lệ.

- Owner review hoàn tất.

- Audit event được ghi.

- Evidence được gắn.

Nếu thiếu một điều kiện, Recipe/BOM không được Approved.

## 56. ACTIVE RECIPE GATE

Recipe/BOM Approved chưa đồng nghĩa Active.

Muốn Active, phải đạt thêm:

- Effective date hợp lệ.

- Không conflict với version active hiện tại.

- Operational config liên quan không bị block.

- Packaging/QC/trace/recall dependency không bị conflict.

- Owner phê duyệt activation.

- Evidence đầy đủ.

- Guard không block.

## 57. RECIPE/BOM CONSUMER BOUNDARY

Recipe/BOM là dữ liệu nhạy cảm vận hành.

Consumer không được đọc toàn bộ công thức nếu không có quyền hoặc không có nhu cầu nghiệp vụ.

Các consumer public như AI Advisor, Landing Page, Public Trace chỉ được nhận dữ liệu đã qua Public View Resolver hoặc Claim/Content Guard.

Không được lộ dữ liệu công thức nội bộ, tỷ lệ chi tiết, nguồn cung nhạy cảm, costing hoặc thông tin không public.

## 58. PRODUCT KNOWLEDGE KHÁC RECIPE/BOM

Product Knowledge dùng cho tư vấn bán hàng không đồng nghĩa Recipe/BOM.

AI Advisor có thể nói sản phẩm có thành phần chính hoặc đặc điểm public-safe theo whitelist.

AI Advisor không được thấy hoặc nói toàn bộ công thức vận hành nếu dữ liệu đó không được public.

Recipe/BOM là dữ liệu vận hành.

Product Knowledge là dữ liệu đã qua kiểm duyệt public-safe.

## 59. CLAIM CONTROL GẮN VỚI RECIPE/BOM

Nếu một claim sản phẩm dựa trên thành phần hoặc công thức, claim đó phải được kiểm soát qua Claim Owner và Public Content Guard.

Không được tự sinh claim từ BOM.

Không được vì thấy một nguyên liệu trong BOM mà tự nói công dụng điều trị, chữa bệnh hoặc cam kết hiệu quả y khoa.

Hiệu dụng thực dưỡng phương Đông nếu dùng trong tư vấn phải đi theo whitelist public-safe đã được phê duyệt.

## 60. BOM VERSION VÀ PUBLIC TRACE

Public Trace không được hiển thị BOM nội bộ đầy đủ.

Public Trace chỉ được hiển thị các trường đã whitelist.

BOM version có thể được dùng để bảo đảm truy xuất nội bộ, nhưng không đồng nghĩa toàn bộ BOM được đưa ra public.

Public Trace phải bảo vệ:

- Công thức chi tiết.

- Tỷ lệ nội bộ.

- Nhà cung cấp nhạy cảm.

- Costing.

- QC defect nội bộ.

- Root cause nội bộ.

- Người vận hành / người phê duyệt.

- Dữ liệu audit nội bộ.

## 61. RECIPE/BOM VÀ TRADE ITEM

Recipe/BOM tạo ra sản phẩm bán được về mặt công thức, nhưng không tự tạo trade item.

Trade item hộp / thùng cần cấu hình riêng ở tầng packaging và trade item control.

Một SKU có thể có nhiều cấp đóng gói.

Mỗi cấp đóng gói có thể có rule in, QR, barcode, GTIN/GS1 riêng.

Recipe/BOM không được hardcode trade item hoặc barcode.

## 62. RECIPE/BOM VÀ HSD

Recipe/BOM có thể ảnh hưởng đến HSD nhưng không tự quyết định HSD cuối cùng nếu chưa có HSD config được phê duyệt.

HSD mặc định theo SKU hoặc nhóm SKU là 12 tháng chỉ được áp dụng qua Shelf Life Config, không được hardcode trong recipe text, AI content hoặc print template.

Nếu công thức thay đổi có thể ảnh hưởng HSD, version mới phải đi qua review.

## 63. RECIPE/BOM VÀ QC CONFIG

Recipe/BOM phải cung cấp thông tin đủ để QC biết sản phẩm thuộc nhóm nào, có thành phần gì, cần kiểm gì và có rủi ro gì.

Tuy nhiên QC Config là cấu hình riêng.

Recipe/BOM không tự pass QC.

Một SKU có Recipe/BOM Active vẫn không được nhập kho nếu chưa có QC/Release Config hợp lệ và batch chưa được release.

## 64. RECIPE/BOM VÀ PACKAGING CONFIG

Recipe/BOM không thay thế Packaging Config.

Sản phẩm có công thức active nhưng thiếu Packaging Config thì không được đóng gói.

Nếu chưa có cấu hình cấp 1/cấp 2, quy cách hộp/thùng, template in và trade item mapping, SKU phải bị block ở bước packaging/printing.

## 65. RECIPE/BOM VÀ PRINT CONFIG

Recipe/BOM không điều khiển máy in.

Print Config xác định in gì, trên cấp nào, theo template nào, lấy dữ liệu từ đâu và có kiểm soát reprint hay không.

Recipe/BOM chỉ cung cấp thông tin sản phẩm cần thiết cho payload in nếu được resolver cho phép.

Không được hardcode nội dung in trong recipe.

## 66. RECIPE/BOM VÀ INVENTORY

Recipe/BOM xác định nhu cầu nguyên liệu.

Inventory xác định nguyên liệu có thật trong kho hay không, lot nào đủ điều kiện issue và số lượng khả dụng ra sao.

Không được vì BOM có nguyên liệu mà tự coi là nguyên liệu tồn kho sẵn sàng.

Material Issue phải dựa trên Inventory Ledger và trạng thái lot thật.

## 67. RECIPE/BOM VÀ SELLABLE

Recipe/BOM Active không đồng nghĩa Sellable.

Một SKU chỉ có thể đi tới sellable khi đồng thời đạt:

- Product Master valid.

- Recipe/BOM/Formula valid.

- Operational config ready.

- Packaging/Print/QC/HSD/Trace/Recall config ready.

- Batch produced.

- QC passed.

- Batch released.

- Warehouse receipt confirmed.

- Inventory available.

- Không hold.

- Không recall.

- Không sale lock.

- Sellable Gate pass.

## 68. RECIPE/BOM AUDIT

Các hành động sau phải ghi audit:

- Tạo recipe.

- Sửa recipe draft.

- Gửi recipe review.

- Approve recipe.

- Reject recipe.

- Activate recipe.

- Suspend recipe.

- Retire recipe.

- Tạo BOM.

- Sửa BOM draft.

- Approve BOM.

- Activate BOM.

- Supersede BOM.

- Tạo formula version.

- Approve formula version.

- Activate formula version.

- Tạo snapshot cho Production Order.

- Block do recipe/BOM conflict.

- Correction/exception liên quan recipe.

- Consumer suppression do recipe/BOM không hợp lệ.

## 69. RECIPE/BOM EVIDENCE

Recipe/BOM evidence tối thiểu gồm:

- Hồ sơ công thức.

- Hồ sơ nguyên liệu.

- Hồ sơ định lượng.

- Hồ sơ phê duyệt công thức.

- Hồ sơ phê duyệt BOM.

- Hồ sơ version.

- Hồ sơ dietary review.

- Hồ sơ allergen/restriction review nếu có.

- Hồ sơ trace/recall relevance.

- Hồ sơ owner approval.

- Hồ sơ snapshot test.

- Hồ sơ smoke test liên quan.

Không có evidence thì recipe/BOM không được gọi PASS.

## 70. RECIPE/BOM GUARD

Recipe/BOM Guard phải block khi:

- SKU không tồn tại.

- SKU chưa approved.

- Recipe missing.

- BOM missing.

- Formula version missing.

- Ingredient không canonical.

- Ingredient bị suspended/retired.

- Quantity missing.

- Unit missing.

- Có dòng nguyên liệu mơ hồ.

- Dietary conflict.

- Allergen/restriction missing đối với nguyên liệu rủi ro.

- Trace relevance missing.

- Recall relevance missing.

- QC requirement missing.

- Version conflict.

- Effective date conflict.

- Formula đã retired.

- BOM đã suspended.

- Snapshot missing khi tạo Production Order.

- Field App cố chọn tay nguyên liệu.

- Consumer cố dùng SKU chưa recipe-ready.

- Evidence missing.

- Audit missing.

## 71. SNAPSHOT GUARD

Snapshot Guard phải block khi:

- Production Order không có SKU hợp lệ.

- SKU chưa có active formula version.

- Recipe/BOM chưa active.

- Recipe/BOM đang bị suspended.

- Formula version bị retired.

- Có conflict giữa Product Master và BOM.

- Có dietary conflict.

- Có ingredient missing.

- Có quantity missing.

- Có unit missing.

- Có trace/recall missing.

- Snapshot không tạo được.

- Snapshot bị sửa sau khi đã locked.

- Snapshot không gắn audit.

- Snapshot không gắn correlation_id.

## 72. 20 SKU G1 DONE CONDITION

Mỗi SKU trong 20 SKU chỉ được xem là G1-ready khi đạt đủ:

- Có SKU canonical.

- Có tên sản phẩm public.

- Có tên sản phẩm nội bộ.

- Có nhóm sản phẩm.

- Có phân loại vegan / mặn.

- Có Formula Version G1.

- Có Recipe Version G1.

- Có BOM Version G1.

- Có ingredient list đầy đủ.

- Không có nguyên liệu mơ hồ.

- Có quantity đầy đủ.

- Có unit đầy đủ.

- Có ingredient role đầy đủ.

- Có lot tracking rule.

- Có QC-before-issue rule.

- Có trace relevance.

- Có recall relevance.

- Có dietary/allergen review.

- Có owner approval.

- Có audit.

- Có evidence.

- Có snapshot test.

- Không có guard block.

## 73. PHẦN 2 RELEASE BOUNDARY

PHẦN 2 chỉ khóa governance cho Recipe / BOM / Formula Version / Snapshot Control.

PHẦN 2 không xác nhận sản phẩm đã được sản xuất.

PHẦN 2 không xác nhận sản phẩm có tồn kho.

PHẦN 2 không xác nhận sản phẩm đã sellable.

PHẦN 2 không gọi Product Activation PASS nếu PHẦN 3 và PHẦN 4 chưa hoàn tất.

## 74. TRẠNG THÁI SAU PHẦN 2

Sau PHẦN 2, PACK-02 đã khóa được:

- Recipe là công thức vận hành có kiểm soát.

- BOM là danh mục nguyên liệu định lượng.

- Formula Version bảo vệ lịch sử công thức.

- G0 là lịch sử/công thức gốc nghiên cứu.

- G1 là công thức vận hành hiện tại.

- Mọi thay đổi sau G1 phải tạo version mới.

- 20 SKU phải có G1 structure riêng.

- Mỗi SKU phải có Recipe Header.

- Mỗi BOM line phải có ingredient canonical, quantity và unit.

- Không dùng nguyên liệu gom nhóm mơ hồ.

- Dietary/allergen/trace/recall relevance là bắt buộc.

- Snapshot bắt buộc khi tạo Production Order.

- Snapshot là bất biến sau khi dùng.

- Field App chỉ hiển thị theo snapshot.

- Không cho chọn tay nguyên liệu trong sản xuất thường.

- Recipe/BOM Active không đồng nghĩa Sellable.

- Audit, evidence và guard là bắt buộc.

PACK-02 vẫn chưa hoàn tất cho đến khi viết xong:

- PHẦN 3/4 — OPERATIONAL CONFIG / PACKAGING CONFIG / PRINT CONFIG / QC CONFIG / HSD / TRACE / RECALL / TRADE ITEM CONTROL.

## •	PHẦN 4/4 — PRODUCT ACTIVATION DEPENDENCY / EVIDENCE / SMOKE / DONE GATE / PACK-02 FINAL CONCLUSION.

## PHẦN 3/4 — OPERATIONAL CONFIG / PACKAGING CONFIG / PRINT CONFIG / QC CONFIG / HSD / TRACE / RECALL / TRADE ITEM CONTROL

## 75. MỤC TIÊU CỦA PHẦN 3

PHẦN 3 thiết lập chuẩn cấu hình vận hành theo SKU sau khi SKU đã có Product Master, Ingredient Canonical, Recipe, BOM và Formula Version.

PHẦN 3 bảo đảm rằng một SKU không chỉ có công thức đúng mà còn phải có đủ cấu hình để đi qua các bước vận hành thực tế:

- Cấu hình vận hành.

- Cấu hình đóng gói.

- Cấu hình in mã.

- Cấu hình QC.

- Cấu hình HSD.

- Cấu hình truy xuất.

- Cấu hình thu hồi.

- Cấu hình trade item hộp / thùng.

- Ranh giới GTIN/GS1.

- Guard chống hardcode.

- Evidence và audit cho từng cấu hình.

Một SKU có Recipe/BOM Active nhưng thiếu bất kỳ cấu hình bắt buộc nào trong PHẦN 3 thì không được đi đến Product Activation hoàn chỉnh.

## 76. OPERATIONAL CONFIG LÀ GÌ

Operational Config là bộ cấu hình xác định một SKU có đủ điều kiện đi vào chuỗi vận hành thật hay chưa.

Operational Config không phải công thức.

Operational Config không phải tồn kho.

Operational Config không phải trạng thái bán hàng.

Operational Config là cầu nối giữa Product Master và Operational Core, giúp hệ thống biết SKU đó phải được sản xuất, đóng gói, kiểm tra, in mã, truy xuất và thu hồi theo rule nào.

## 77. NGUYÊN TẮC OPERATIONAL CONFIG THEO SKU

Mỗi SKU phải có Operational Config riêng.

Không được dùng một Operational Config chung cho nhiều SKU nếu các SKU đó khác nhau về công thức, chế độ ăn, nguyên liệu, QC, HSD, đóng gói, in mã, trace hoặc recall.

Operational Config phải gắn với:

- SKU canonical.

- Formula version.

- Recipe version.

- BOM version.

- Product group.

- Dietary type.

- Packaging rule.

- Print rule.

- QC rule.

- HSD rule.

- Trace rule.

- Recall rule.

- Trade item rule.

- Owner approval.

- Audit.

- Evidence.

## 78. OPERATIONAL CONFIG KHÔNG THAY OPERATIONAL CORE

Operational Config chỉ nói SKU được cấu hình vận hành như thế nào.

Operational Core mới xác định một batch cụ thể đã được sản xuất, QC, release, nhập kho, trace, recall và sellable hay chưa.

Do đó:

- Operational Config Ready không đồng nghĩa đã có hàng.

- Operational Config Ready không đồng nghĩa batch đã RELEASED.

- Operational Config Ready không đồng nghĩa inventory available.

- Operational Config Ready không đồng nghĩa sellable.

- Operational Config Ready không được vượt Sale Lock / Stop Sale.

## 79. TRƯỜNG BẮT BUỘC CỦA OPERATIONAL CONFIG

Mỗi Operational Config tối thiểu phải có:

Nhóm thông tin | Trường bắt buộc

Định danh | operational_config_id, sku_id, sku_code

Công thức | formula_version_id, recipe_version_id, bom_version_id

Trạng thái | operational_config_status, approval_status

Đóng gói | packaging_config_id

In mã | print_config_id

QC | qc_config_id

HSD | shelf_life_config_id

Trace | trace_config_id

Recall | recall_config_id

Trade item | trade_item_config_id nếu có

Hiệu lực | effective_from, effective_to nếu có

Owner | owner_id, approved_by, approved_at

Governance | audit_reference, evidence_reference

Nếu thiếu Operational Config, SKU không được đưa vào vận hành.

## 80. OPERATIONAL CONFIG STATUS

Operational Config phải có trạng thái vòng đời rõ ràng.

Các trạng thái tối thiểu:

- Draft.

- waiting Review.

- Approved.

- Ready For Activation.

- Active.

- Suspended.

- Superseded.

- Retired.

Ý nghĩa:

- Draft: chưa đủ điều kiện dùng.

- waiting Review: đang chờ review.

- Approved: đã được duyệt cấu hình.

- Ready For Activation: đã đủ điều kiện để đưa vào Product Activation.

- Active: đang áp dụng.

- Suspended: tạm dừng.

- Superseded: đã có version thay thế.

- Retired: ngừng sử dụng.

Không module consumer nào được tự diễn giải trạng thái Operational Config.

## 81. OPERATIONAL CONFIG GUARD

Operational Config Guard phải block khi:

- SKU không tồn tại.

- SKU chưa approved.

- Recipe/BOM/Formula missing.

- Formula version không active.

- Operational Config missing.

- Packaging Config missing.

- Print Config missing.

- QC Config missing.

- HSD Config missing.

- Trace Config missing.

- Recall Config missing.

- Trade Item Config missing nếu SKU có hộp/thùng.

- Config hết hiệu lực.

- Config chưa owner approval.

- Evidence missing.

- Audit missing.

- Có conflict giữa Product Master và Operational Core.

- Có conflict về vegan / mặn.

- Có conflict về trace / recall.

- Có conflict về QR / barcode / GTIN / GS1.

Nếu Operational Config Guard block, SKU không được activation.

## 82. PACKAGING CONFIG PRINCIPLE

Packaging Config xác định SKU được đóng gói theo cấp nào, quy cách nào, đơn vị nào và liên kết với trade item nào.

Packaging Config không được hardcode trong Production Order, Field App, Print Template, Landing Page, Order hoặc Quote.

Packaging Config phải do Product Master / Packaging Owner quản trị và được Operational Core sử dụng qua resolver.

## 83. CẤU HÌNH ĐÓNG GÓI CẤP 1

Packaging Level 1 là cấp đóng gói trực tiếp liên quan đến sản phẩm sau sản xuất trước khi đi vào cấp thương mại cao hơn.

Cấu hình cấp 1 phải xác định tối thiểu:

- sku_id.

- packaging_level = L1.

- packaging_unit_name.

- packaging_material_id nếu có.

- quantity_per_unit.

- unit_of_measure.

- label_requirement.

- print_requirement.

- QC check requirement.

- trace binding requirement.

- evidence requirement.

- approved status.

- audit reference.

Cấp 1 không tự tạo tồn kho bán hàng nếu quy trình yêu cầu phải qua cấp hộp/thùng và warehouse receipt.

## 84. CẤU HÌNH ĐÓNG GÓI CẤP 2 — HỘP

Packaging Level 2 — Hộp là cấp đóng gói thương mại hoặc bán lẻ nếu được cấu hình là trade item.

Cấu hình hộp phải xác định tối thiểu:

- sku_id.

- packaging_level = L2_BOX.

- box_unit_name.

- units_per_box.

- box_material_config.

- box_label_config.

- box_print_config_id.

- QR requirement.

- barcode requirement.

- GTIN/GS1 requirement nếu áp dụng.

- lot binding rule.

- NSX binding rule.

- HSD binding rule.

- trace binding rule.

- QC check before release.

- reprint rule.

- evidence requirement.

- owner approval.

- audit reference.

Nếu thiếu Packaging Config cho hộp, SKU không được đóng gói thành hộp.

## 85. CẤU HÌNH ĐÓNG GÓI CẤP 2 — THÙNG

Packaging Level 2 — Thùng là cấp đóng gói thương mại, logistics hoặc bán sỉ nếu được cấu hình là trade item.

Cấu hình thùng phải xác định tối thiểu:

- sku_id.

- packaging_level = L2_CARTON.

- carton_unit_name.

- boxes_per_carton hoặc units_per_carton.

- carton_material_config.

- carton_label_config.

- carton_print_config_id.

- QR requirement.

- barcode requirement.

- GTIN/GS1 requirement nếu áp dụng.

- lot binding rule.

- NSX binding rule.

- HSD binding rule.

- trace binding rule.

- warehouse receiving rule.

- logistics handling rule.

- evidence requirement.

- owner approval.

- audit reference.

Nếu thiếu Packaging Config cho thùng, SKU không được đóng gói thành thùng.

## 86. PACKAGING CONFIG KHÔNG TẠO INVENTORY

Packaging Config chỉ xác định cách đóng gói.

Packaging Config không tự tăng inventory.

Inventory chỉ tăng sau Warehouse Receipt Confirmed theo Operational Core.

Việc đóng gói xong không đồng nghĩa sản phẩm đã nhập kho.

Việc in mã xong không đồng nghĩa sản phẩm đã sellable.

## 87. PACKAGING CONFIG GUARD

Packaging Config Guard phải block khi:

- SKU missing.

- SKU chưa approved.

- Operational Config missing.

- Packaging level missing.

- Quy cách đóng gói missing.

- Hộp/thùng missing trong cấu hình nhưng quy trình yêu cầu.

- Print Config missing cho cấp cần in.

- Trace binding missing.

- HSD binding missing.

- Lot binding missing.

- Trade Item mapping missing nếu cấp đóng gói là trade item.

- Evidence missing.

- Audit missing.

- Có conflict giữa cấp hộp và cấp thùng.

- Có conflict giữa packaging config và print config.

- Có conflict giữa packaging config và warehouse receiving rule.

Nếu Packaging Config Guard block, SKU không được đi tới đóng gói.

## 88. PRINT CONFIG PRINCIPLE

Print Config xác định nội dung in, cấp in, nguồn dữ liệu in, template in, rule kiểm soát in và rule reprint.

Print Config không phải công thức.

Print Config không phải barcode registry.

Print Config không phải QR generator tự do.

Print Config không được hardcode trong máy in, Field App, Admin Web, Landing Page hoặc file template tĩnh.

Máy in chỉ nhận payload đã được backend/domain service cấp.

Máy in không sinh business truth.

## 89. PRINT CONFIG CẤP 1

In cấp 1 mặc định chỉ áp dụng thông tin NSX/HSD nếu quy trình hiện hành yêu cầu.

Cấu hình in cấp 1 phải xác định:

- sku_id.

- packaging_level = L1.

- printable_fields.

- NSX source.

- HSD source.

- template_id.

- printer_profile_id nếu có.

- print_permission.

- print_audit_rule.

- evidence requirement.

Không được tự thêm barcode hoặc QR cấp 1 nếu chưa được cấu hình.

## 90. PRINT CONFIG CẤP 2 — HỘP

In cấp 2 trên hộp phải có cấu hình riêng.

Nội dung in trên hộp tối thiểu gồm:

- Lô.

- NSX.

- HSD.

- Mã vạch nếu áp dụng.

- QR truy xuất nếu áp dụng.

- Tên sản phẩm theo Product Master.

- Quy cách nếu được cấu hình.

- Template in hộp.

- Dữ liệu binding với batch/lot.

- Audit và evidence.

Không được hardcode lô, NSX, HSD, barcode hoặc QR vào template.

## 91. PRINT CONFIG CẤP 2 — THÙNG

In cấp 2 trên thùng phải có cấu hình riêng.

Nội dung in trên thùng tối thiểu gồm:

- Lô.

- NSX.

- HSD.

- Mã vạch nếu áp dụng.

- QR truy xuất nếu áp dụng.

- Tên sản phẩm hoặc mã hàng theo Product Master.

- Số lượng hộp/thùng hoặc quy cách nếu được cấu hình.

- Template in thùng.

- Dữ liệu binding với batch/lot.

- Audit và evidence.

Không được dùng template hộp thay cho template thùng nếu chưa được owner phê duyệt.

## 92. PRINT PAYLOAD BOUNDARY

Print Payload là dữ liệu đã được backend/domain service tạo ra cho một lần in cụ thể.

Print Payload phải gắn với:

- sku_id.

- batch_id.

- lot/batch code.

- packaging_level.

- print_config_id.

- template_id.

- printable fields.

- QR payload reference nếu có.

- barcode reference nếu có.

- printer/device nếu có.

- actor.

- timestamp.

- audit_id.

- correlation_id.

Print Payload không được sinh từ file Excel rời, ghi chú thủ công hoặc nhập tay không kiểm soát.

## 93. REPRINT CONTROL

Reprint là hành động rủi ro cao.

Reprint phải có:

- Permission.

- Reason bắt buộc.

- Object binding.

- Print payload reference.

- Người thực hiện.

- Thiết bị.

- Số lần reprint.

- Audit.

- Evidence nếu cần.

- Guard kiểm tra conflict.

Không được reprint tự do.

Không được reprint để sửa sai dữ liệu nguồn.

Nếu dữ liệu nguồn sai, phải xử lý correction ở nguồn nghiệp vụ, không sửa trực tiếp trên template in.

## 94. QR CODE BOUNDARY

QR là mã truy xuất và minh bạch.

QR không thay GTIN/GS1.

QR không thay barcode thương mại.

QR không tự xác nhận sản phẩm sellable.

QR không tự tạo public trace.

QR chỉ được public khi Trace Config, Public Trace Whitelist, Batch Release, Warehouse/Inventory state và Sale Lock/Recall Guard cho phép.

Nếu SKU chưa bật Trace Config, không được public QR trace.

## 95. BARCODE / GTIN / GS1 BOUNDARY

Barcode thương mại và GTIN/GS1 phải đi qua Trade Item Registry.

Không được hardcode GTIN/GS1.

Không được cho user nhập tay barcode thương mại.

Không được có hai barcode thương mại trên cùng một trade item.

Hộp và thùng có thể là trade item riêng, do đó có thể có định danh thương mại riêng nếu được cấu hình hợp lệ.

Một SKU không được tự sinh barcode thương mại nếu chưa có Trade Item Config.

## 96. PRINT CONFIG GUARD

Print Config Guard phải block khi:

- SKU missing.

- Batch missing.

- Packaging level missing.

- Print Config missing.

- Template missing.

- Printable field source missing.

- Lot binding missing.

- NSX binding missing.

- HSD binding missing.

- QR rule missing nếu cần QR.

- Barcode rule missing nếu cần barcode.

- GTIN/GS1 mapping missing nếu cấp in là trade item có GTIN.

- Device/printer conflict nếu có.

- Reprint không có reason.

- Reprint không có permission.

- Evidence missing.

- Audit missing.

Nếu Print Config Guard block, hệ thống không được in hoặc reprint.

## 97. QC CONFIG PRINCIPLE

QC Config xác định SKU cần kiểm gì, ngưỡng nào, thời điểm nào, ai có quyền xác nhận, evidence nào cần đính kèm và trạng thái nào được chuyển tiếp.

QC Config không tự pass QC.

QC Config chỉ cung cấp rule.

Kết quả QC thật phải được ghi nhận qua Operational Core, actor có quyền, evidence, audit và state transition hợp lệ.

## 98. QC CONFIG THEO SKU

Mỗi SKU phải có QC Config riêng hoặc QC Config theo nhóm SKU được owner phê duyệt.

QC Config phải gắn với:

- sku_id hoặc product_group.

- formula_version_id nếu QC phụ thuộc công thức.

- recipe_version_id nếu QC phụ thuộc BOM.

- qc_checklist.

- qc_threshold.

- sampling_rule nếu có.

- evidence_rule.

- pass/hold/fail rule.

- release dependency.

- owner approval.

- audit reference.

Không được dùng một checklist QC mơ hồ cho toàn bộ sản phẩm nếu các SKU có rủi ro khác nhau.

## 99. QC ĐỘ ẨM SAU SẤY

QC độ ẩm sau sấy là tiêu chí vận hành quan trọng.

Ngưỡng cấu hình hiện hành:

Kết quả độ ẩm | Trạng thái xử lý

Dưới 2% | PASS

Từ 2% đến 4% | HOLD / REVIEW

Trên 4% | FAIL

Ngưỡng này phải nằm trong QC Config hoặc registry được owner phê duyệt.

Không được hardcode trong Field App, Admin Web, AI Advisor, dashboard hoặc tài liệu vận hành rời.

Nếu owner thay đổi ngưỡng QC trong tương lai, phải thay đổi qua version/config có audit và evidence.

## 100. QC_PASS KHÔNG ĐỒNG NGHĨA RELEASED

QC_PASS chỉ là kết quả kiểm tra đạt.

QC_PASS không đồng nghĩa batch đã RELEASED.

Batch Release là action riêng, có owner, permission, audit và evidence.

SKU có QC Config đầy đủ vẫn không được nhập kho nếu batch chưa được RELEASED.

## 101. QC CONFIG TRƯỚC WAREHOUSE RECEIPT

Một batch chỉ được Warehouse Receipt khi:

- SKU valid.

- Recipe/BOM snapshot valid.

- QC Config valid.

- QC result hợp lệ.

- Batch Release hợp lệ.

- Không có hold.

- Không có recall.

- Không có sale lock ở cấp batch/SKU nếu ảnh hưởng warehouse.

- Evidence đầy đủ.

- Audit đầy đủ.

Nếu thiếu QC/release config, SKU không được nhập kho.

## 102. QC CONFIG GUARD

QC Config Guard phải block khi:

- SKU missing.

- QC Config missing.

- QC checklist missing.

- QC threshold missing.

- QC evidence rule missing.

- QC actor permission missing.

- QC result không hợp lệ.

- QC_PASS bị hiểu nhầm là RELEASED.

- Release action missing.

- Evidence missing.

- Audit missing.

- Có conflict giữa QC Config và Recipe/BOM.

- Có conflict giữa QC Config và HSD/Trace/Recall.

- Có moisture result thuộc HOLD/REVIEW nhưng hệ thống vẫn cho release.

- Có moisture result FAIL nhưng hệ thống vẫn cho release.

Nếu QC Config Guard block, batch không được release và không được nhập kho.

## 103. HSD CONFIG PRINCIPLE

HSD Config xác định hạn sử dụng của sản phẩm theo SKU hoặc nhóm SKU.

HSD Config không được hardcode trong print template, AI content, Landing Page, Order hoặc Public Trace.

HSD phải được lấy từ Shelf Life Config được owner phê duyệt.

## 104. HSD MẶC ĐỊNH 12 THÁNG

HSD mặc định theo SKU hoặc nhóm SKU là 12 tháng.

Mặc định này chỉ được áp dụng khi đã có Shelf Life Config hợp lệ.

Không được tự động in HSD 12 tháng nếu SKU chưa có Shelf Life Config.

Không được để máy in tự cộng ngày thủ công.

Không được để người vận hành nhập tay HSD nếu không có permission và audit.

## 105. NGUỒN TÍNH NSX / HSD

NSX phải lấy từ dữ liệu sản xuất hoặc batch được Operational Core xác nhận.

HSD phải được tính theo Shelf Life Config.

HSD không được lấy từ ghi chú text.

HSD không được nhập tay trực tiếp trên template in.

Nếu NSX hoặc HSD conflict, hệ thống phải block in, block warehouse receipt hoặc block sellable tùy mức độ.

## 106. HSD CONFIG VERSIONING

Shelf Life Config phải có version.

Nếu thay đổi HSD theo SKU hoặc nhóm SKU, phải tạo version mới hoặc thay đổi config có audit.

Không được sửa âm thầm HSD của SKU đã có batch sản xuất.

Batch đã sản xuất phải giữ HSD theo config tại thời điểm sản xuất/in/release tùy rule được owner phê duyệt.

## 107. HSD CONFIG GUARD

HSD Config Guard phải block khi:

- SKU missing.

- Shelf Life Config missing.

- NSX source missing.

- HSD rule missing.

- HSD calculation conflict.

- HSD vượt rule được phê duyệt.

- HSD bị sửa tay không có audit.

- HSD không khớp giữa hộp và thùng.

- HSD không khớp với batch.

- HSD conflict với trace/public trace.

- Evidence missing.

- Audit missing.

Nếu HSD Config Guard block, không được in, không được nhập kho hoặc không được sellable tùy điểm phát hiện.

## 108. TRACE CONFIG PRINCIPLE

Trace Config xác định SKU được truy xuất theo mô hình nào, cấp nào, dữ liệu nào được ghi nhận, dữ liệu nào được public và dữ liệu nào chỉ dùng nội bộ.

Trace Config phải kết nối với Operational Core.

Trace không được dựng thành chuỗi riêng rời khỏi sản xuất, batch, lot, packaging, warehouse và inventory.

## 109. TRACE CONFIG THEO SKU

Mỗi SKU phải có Trace Config riêng hoặc theo nhóm SKU được owner phê duyệt.

Trace Config phải xác định:

- trace_config_id.

- sku_id hoặc product_group.

- trace_level.

- raw material lot linkage.

- production order linkage.

- batch linkage.

- packaging linkage.

- QR linkage.

- warehouse linkage.

- public trace whitelist.

- internal trace fields.

- evidence rule.

- activation status.

- owner approval.

- audit reference.

Nếu chưa bật Trace Config, SKU không được public trace.

## 110. TRACE LEVEL CONTROL

Trace Config phải phân biệt tối thiểu:

- Internal trace.

- Public trace.

- Recall trace.

- Audit trace.

- Evidence trace.

Không được dùng một QR public để lộ toàn bộ trace nội bộ.

Không được để Public Trace thấy dữ liệu nhạy cảm như supplier detail, người vận hành, người phê duyệt, costing, MISA, QC defect, root cause nội bộ hoặc trace graph ID nội bộ.

## 111. PUBLIC TRACE WHITELIST

Public Trace chỉ được hiển thị trường đã whitelist.

Public Trace không được tự đọc toàn bộ Product Master, BOM, batch, QC hoặc audit.

Các nhóm dữ liệu có thể public phải qua owner phê duyệt.

Các nhóm dữ liệu không được public:

- Công thức chi tiết.

- Tỷ lệ nguyên liệu nội bộ.

- Supplier detail nhạy cảm.

- Costing.

- MISA.

- QC defect/loss/waste.

- Root cause nội bộ.

- Người vận hành.

- Người phê duyệt.

- Internal trace graph ID.

- Audit history nội bộ.

- Dữ liệu điều tra recall/CAPA chưa public.

## 112. TRACE CONFIG GUARD

Trace Config Guard phải block khi:

- SKU missing.

- Trace Config missing.

- Trace level missing.

- Raw material lot linkage missing.

- Production Order linkage missing.

- Batch linkage missing.

- Packaging linkage missing nếu đã đóng gói.

- QR linkage missing nếu public trace cần QR.

- Public whitelist missing.

- Internal/public boundary conflict.

- Batch chưa RELEASED nhưng public trace đang mở.

- SKU đang sale lock nhưng public trace không suppress đúng.

- Recall đang active nhưng public trace không hiển thị trạng thái phù hợp theo policy.

- Evidence missing.

- Audit missing.

Nếu Trace Config Guard block, không được public trace.

## 113. RECALL CONFIG PRINCIPLE

Recall Config xác định cách SKU tham gia quy trình hold, recall, recovery, disposition và CAPA.

Recall Config không được tách khỏi Traceability.

Recall phải dùng traceability chain thật.

Không được dựng danh sách recall thủ công nếu hệ thống có dữ liệu trace.

## 114. RECALL CONFIG THEO SKU

Mỗi SKU phải có Recall Config riêng hoặc theo nhóm SKU được owner phê duyệt.

Recall Config phải xác định:

- recall_config_id.

- sku_id hoặc product_group.

- recall_scope_rule.

- affected batch rule.

- affected lot rule.

- affected packaging rule.

- affected warehouse rule.

- affected order/customer rule nếu có.

- hold rule.

- sale lock rule.

- recovery rule.

- disposition rule.

- CAPA linkage.

- evidence rule.

- owner approval.

- audit reference.

Nếu thiếu Trace/Recall Config, SKU không được recall chuẩn.

## 115. HOLD VÀ SALE LOCK PHẢI TÁCH RIÊNG

Hold là trạng thái giữ/chặn ở nghiệp vụ vận hành, QC, kho hoặc batch.

Sale Lock / Stop Sale là trạng thái chặn bán ở các consumer.

Một SKU hoặc batch có thể bị hold nhưng chưa chắc đã cần public messaging.

Một SKU hoặc batch bị Sale Lock thì mọi kênh bán phải dừng.

Sale Lock / Stop Sale thắng:

- Sales.

## •	CRM.

## •	ADS.

## •	MC AI.

- Gateway.

- Landing Page.

- Order.

- Quote.

- Payment.

- Shipping.

## •	IVR.

- Public Trace theo rule policy.

## 116. RECALL CONFIG GUARD

Recall Config Guard phải block khi:

- SKU missing.

- Recall Config missing.

- Trace Config missing.

- Recall scope missing.

- Affected batch rule missing.

- Affected lot rule missing.

- Affected packaging rule missing.

- Sale Lock rule missing.

- Hold rule missing.

- Recovery/disposition rule missing.

- CAPA linkage missing nếu cần.

- Evidence missing.

- Audit missing.

- Recall cố chạy bằng danh sách thủ công không có trace chain.

- Consumer vẫn bán SKU/batch đang recall.

- CRM/ADS vẫn gợi ý sản phẩm đang stop-sale.

- Quote/Order vẫn tạo đơn cho SKU đang sale lock.

Nếu Recall Config Guard block, SKU không được gọi là recall-ready.

## 117. TRADE ITEM CONTROL PRINCIPLE

Trade Item là đơn vị thương mại có thể được nhận diện, đóng gói, in mã, lưu kho, bán hoặc vận chuyển theo rule.

Hộp và thùng có thể là trade item riêng.

Trade Item không được tự sinh từ SKU nếu chưa có cấu hình.

Trade Item phải được quản trị qua Trade Item Registry.

## 118. TRADE ITEM HỘP

Trade Item Hộp phải xác định:

- trade_item_id.

- sku_id.

- packaging_level = BOX.

- trade_item_name.

- unit_quantity.

- unit_of_measure.

- box_config_id.

- print_config_id.

- barcode_config_id nếu có.

- GTIN/GS1 mapping nếu có.

- QR rule nếu có.

- warehouse handling rule.

- sellable unit rule.

- owner approval.

- audit reference.

- evidence reference.

Nếu hộp là đơn vị bán lẻ, Order/Quote phải đọc đúng trade item hộp qua resolver.

## 119. TRADE ITEM THÙNG

Trade Item Thùng phải xác định:

- trade_item_id.

- sku_id.

- packaging_level = CARTON.

- trade_item_name.

- boxes_per_carton hoặc unit_quantity.

- unit_of_measure.

- carton_config_id.

- print_config_id.

- barcode_config_id nếu có.

- GTIN/GS1 mapping nếu có.

- QR rule nếu có.

- warehouse handling rule.

- logistics handling rule.

- sellable unit rule nếu thùng được bán.

- owner approval.

- audit reference.

- evidence reference.

Nếu thùng là đơn vị bán sỉ hoặc logistics, Order/Quote/Shipping phải đọc đúng trade item thùng qua resolver.

## 120. GTIN/GS1 BOUNDARY TRONG TRADE ITEM

GTIN/GS1 là định danh thương mại của trade item nếu doanh nghiệp đăng ký và cấu hình.

GTIN/GS1 không phải SKU code.

GTIN/GS1 không phải QR trace code.

GTIN/GS1 không phải batch code.

GTIN/GS1 không được hardcode.

GTIN/GS1 không được nhập tay tự do.

Một trade item chỉ được có một barcode thương mại active tại một thời điểm.

Nếu thay đổi GTIN/GS1, phải đi qua Trade Item Versioning, approval, audit và evidence.

## 121. KHÔNG CÓ HAI BARCODE THƯƠNG MẠI TRÊN CÙNG TRADE ITEM

Một trade item không được có hai barcode thương mại active.

Nếu hộp và thùng đều là trade item, mỗi cấp có thể có barcode riêng.

Nhưng không được có hai barcode thương mại cùng active cho cùng một hộp hoặc cùng một thùng.

Nếu phát hiện conflict, Print Config, Warehouse Receipt, Order, Shipping và Public Trace phải bị block hoặc suppress theo rule.

## 122. QR VÀ GTIN/GS1 KHÔNG THAY THẾ NHAU

QR dùng cho truy xuất/minh bạch.

GTIN/GS1 dùng cho định danh thương mại nếu áp dụng.

Barcode thương mại phục vụ nhận diện trade item.

Ba lớp này phải tách biệt:

Lớp mã | Vai trò

QR | Truy xuất / minh bạch

Barcode thương mại | Nhận diện trade item

GTIN/GS1 | Định danh thương mại theo chuẩn đăng ký

Không được dùng QR thay cho GTIN/GS1.

Không được dùng GTIN/GS1 thay cho trace chain.

Không được dùng barcode thương mại để xác nhận QC hoặc release batch.

## 123. TRADE ITEM GUARD

Trade Item Guard phải block khi:

- Trade item missing.

- SKU missing.

- Packaging level missing.

- Packaging Config missing.

- Print Config missing.

- Barcode rule missing nếu cần.

- GTIN/GS1 mapping conflict.

- Có hai barcode thương mại active.

- QR rule conflict.

- Unit quantity missing.

- Sellable unit rule missing nếu trade item được bán.

- Warehouse handling rule missing.

- Shipping handling rule missing nếu cần.

- Evidence missing.

- Audit missing.

Nếu Trade Item Guard block, không được in mã thương mại, không được nhập kho theo trade item, không được quote/order theo trade item đó.

## 124. PRODUCT CONFIG RESOLVER PRINCIPLE

Product Config Resolver là lớp đọc cấu hình hợp lệ cho SKU.

Resolver phải trả dữ liệu đã qua guard, không trả dữ liệu thô khiến consumer tự suy luận.

Product Config Resolver phải phân biệt:

- SKU exists.

- SKU approved.

- Recipe/BOM active.

- Operational Config ready.

- Packaging Config ready.

- Print Config ready.

- QC Config ready.

- HSD Config ready.

- Trace Config ready.

- Recall Config ready.

- Trade Item Config ready.

- Product Activation ready.

- Sellable status từ Operational Core.

- Suppression status nếu có.

Không được trả một trạng thái chung kiểu “ready” nếu chưa qua đầy đủ guard.

## 125. CONFIG VERSIONING PRINCIPLE

Mọi cấu hình trong PHẦN 3 phải có version hoặc lịch sử thay đổi có audit.

Các cấu hình phải quản trị version gồm:

- Operational Config.

- Packaging Config.

- Print Config.

- QC Config.

- Shelf Life Config.

- Trace Config.

- Recall Config.

- Trade Item Config.

- Barcode/GTIN mapping nếu có.

- QR rule nếu có.

Không được sửa đè cấu hình đã dùng trong batch, production order, print payload, warehouse receipt, trace hoặc recall.

## 126. CONFIG EFFECTIVE DATE

Mỗi config phải có ngày hiệu lực.

Nếu config mới thay thế config cũ, hệ thống phải biết:

- Config nào dùng trước ngày hiệu lực.

- Config nào dùng sau ngày hiệu lực.

- Batch nào đã dùng config cũ.

- Batch nào dùng config mới.

- Print payload nào dùng template cũ.

- Trace/recall nào phải giữ theo version cũ.

Không được để nhiều config active cùng lúc cho cùng một SKU và cùng một context nếu không có rule phân tách rõ.

## 127. CONFIG AUDIT

Các hành động sau phải ghi audit:

- Tạo Operational Config.

- Approve Operational Config.

- Suspend Operational Config.

- Tạo Packaging Config.

- Approve Packaging Config.

- Tạo Print Config.

- Approve Print Config.

- Reprint.

- Tạo QC Config.

- Thay đổi QC threshold.

- Tạo Shelf Life Config.

- Thay đổi HSD.

- Tạo Trace Config.

- Bật/tắt Public Trace.

- Tạo Recall Config.

- Kích hoạt Sale Lock rule.

- Tạo Trade Item.

- Gán GTIN/GS1.

- Thay đổi barcode mapping.

- Config Guard block.

- Consumer suppression do config conflict.

## 128. CONFIG EVIDENCE

Mỗi cấu hình phải có evidence tương ứng.

Evidence tối thiểu gồm:

- Hồ sơ duyệt Operational Config.

- Hồ sơ duyệt Packaging Config.

- Hồ sơ duyệt Print Config.

- Mẫu/template in được duyệt.

- Hồ sơ test in hộp.

- Hồ sơ test in thùng.

- Hồ sơ QC Config.

- Hồ sơ ngưỡng QC.

- Hồ sơ HSD Config.

- Hồ sơ Trace Config.

- Hồ sơ Public Trace Whitelist.

- Hồ sơ Recall Config.

- Hồ sơ Trade Item Config.

- Hồ sơ GTIN/GS1 nếu áp dụng.

- Hồ sơ smoke test.

- Owner sign-off.

Không có evidence thì không PASS.

## 129. CONSUMER SUPPRESSION THEO CONFIG

Nếu cấu hình bị thiếu hoặc bị block, các consumer phải suppress theo đúng ngữ cảnh.

Các consumer chịu ảnh hưởng gồm:

- AI Advisor.

- CRM.

- ADS.

- MC AI.

- Gateway.

- Landing Page.

- Order.

- Quote.

- Payment.

- Shipping.

- IVR.

- Public Trace.

- Admin preview.

- Reporting dashboard.

Ví dụ:

- Thiếu Trace Config thì không public trace.

- Thiếu Print Config thì không in cấp 2.

- Thiếu QC Config thì không release batch.

- Thiếu Trade Item Config thì không quote/order theo hộp/thùng.

- Có Sale Lock thì mọi kênh bán phải dừng.

- Có HSD conflict thì không in/không nhập kho/không sellable tùy điểm phát hiện.

## 130. FIELD APP BOUNDARY THEO CONFIG

Field App chỉ được hiển thị và capture theo config đã được backend/domain service cấp.

Field App không được:

- Tự chọn packaging config.

- Tự chọn print template.

- Tự sửa NSX/HSD.

- Tự sinh QR.

- Tự sinh barcode.

- Tự nhập GTIN/GS1.

- Tự pass QC.

- Tự release batch.

- Tự bật trace.

- Tự mở recall.

- Tự bỏ sale lock.

- Tự sửa trade item.

Field App chỉ là capture channel, không phải Owner Core.

## 131. ADMIN WEB BOUNDARY THEO CONFIG

Admin Web được dùng cho:

- Master data.

- Cấu hình.

- Review.

- Dashboard.

- Audit.

- Phê duyệt theo quyền.

Admin Web không được bypass backend/domain service.

Admin Web không được cập nhật trực tiếp config đã locked mà không qua approval, audit, evidence và guard.

Admin Web không được cho phép nhập tay barcode thương mại trái rule.

## 132. NO-HARDCODE RUNTIME CONFIG

Các dữ liệu sau không được hardcode:

- Kho.

- Location.

- Printer.

- Template.

- Device.

- App version.

- GTIN/GS1.

- Barcode.

- QR.

- MISA mapping.

- QC threshold.

- HSD.

- Supplier pass.

- Nhân sự.

- Role/permission.

- Evidence path.

- Smoke status.

- Sellable status.

- Recall status.

- Sale lock status.

- Operational config.

- Packaging config.

- Print config.

- Trace config.

- Trade item config.

Tất cả phải đi qua registry/config/resolver/guard được phê duyệt.

## 133. PHẦN 3 DONE CONDITION

PHẦN 3 được xem là đạt về mặt governance khi đã khóa được:

- Operational Config là bắt buộc theo SKU.

- Operational Config không thay Operational Core.

- Packaging Config cấp 1/cấp 2 là bắt buộc.

- Hộp và thùng có cấu hình riêng.

- Print Config hộp/thùng là bắt buộc nếu in cấp 2.

- Máy in không sinh business truth.

- Print Payload phải do backend/domain service cấp.

- Reprint là high-risk action.

- QR không thay GTIN/GS1.

- Barcode/GTIN/GS1 đi qua Trade Item Registry.

- Không có hai barcode thương mại trên cùng trade item.

- QC Config là bắt buộc.

- QC_PASS không đồng nghĩa RELEASED.

- QC độ ẩm sau sấy phải qua config.

- HSD mặc định 12 tháng phải qua Shelf Life Config.

- NSX/HSD không được nhập tay tùy tiện.

- Trace Config là bắt buộc nếu public trace.

- Public Trace phải whitelist-only.

- Recall Config phải dùng traceability chain.

- Hold và Sale Lock phải tách riêng.

- Sale Lock thắng mọi consumer.

- Trade Item hộp/thùng phải có registry.

- Config versioning, audit và evidence là bắt buộc.

- Consumer suppression phải kích hoạt khi config bị block.

- No-hardcode runtime config là P0.

## 134. TRẠNG THÁI SAU PHẦN 3

Sau PHẦN 3, PACK-02 đã khóa được lớp cấu hình vận hành cho SKU, bao gồm:

- Operational Config.

- Packaging Config.

- Print Config.

- QC Config.

- HSD Config.

- Trace Config.

- Recall Config.

- Trade Item Config.

- GTIN/GS1 boundary.

- QR/barcode boundary.

- Config Guard.

- Config Evidence.

- Config Audit.

- Consumer Suppression.

- No-hardcode runtime rule.

PACK-02 vẫn chưa hoàn tất cho đến khi viết xong:

## •	PHẦN 4/4 — PRODUCT ACTIVATION DEPENDENCY / EVIDENCE / SMOKE / DONE GATE / PACK-02 FINAL CONCLUSION.

## PHẦN 4/4 — PRODUCT ACTIVATION DEPENDENCY / EVIDENCE / SMOKE / DONE GATE / PACK-02 FINAL CONCLUSION

## 135. MỤC TIÊU CỦA PHẦN 4

PHẦN 4 thiết lập chuẩn Product Activation cho SKU, đồng thời khóa điều kiện evidence, smoke, done gate và release handoff của PACK-02.

Mục tiêu của PHẦN 4 là bảo đảm một SKU chỉ được kích hoạt đúng tầng, đúng điều kiện, đúng owner, đúng guard và đúng trạng thái.

Product Activation không được hiểu là mở bán tự động.

Product Activation chỉ xác nhận SKU đã đủ điều kiện cấu hình sản phẩm ở tầng Product Master / Recipe / Operational Config.

Sellable cuối cùng vẫn phải đi qua Operational Core, Batch Release, Warehouse Receipt, Inventory Ledger, Stock Gate, Recall Gate và Sale Lock Gate.

## 136. PRODUCT ACTIVATION LÀ GÌ

Product Activation là trạng thái quản trị cho biết một SKU đã được phê duyệt đủ điều kiện đi vào hệ vận hành theo phạm vi được cho phép.

Product Activation có thể áp dụng cho:

- Kích hoạt SKU trong Product Master.

- Kích hoạt công thức G1.

- Kích hoạt Recipe/BOM version.

- Kích hoạt Operational Config.

- Kích hoạt Packaging Config.

- Kích hoạt Print Config.

- Kích hoạt QC Config.

- Kích hoạt HSD Config.

- Kích hoạt Trace Config.

- Kích hoạt Recall Config.

- Kích hoạt Trade Item Config.

- Kích hoạt SKU cho sản xuất.

- Kích hoạt SKU cho consumer visibility nếu đủ điều kiện.

- Kích hoạt SKU cho quote/order chỉ khi Sellable Gate đạt.

Product Activation không được gom tất cả các ý nghĩa trên vào một cờ duy nhất.

## 137. PRODUCT ACTIVATION KHÔNG ĐỒNG NGHĨA SELLABLE

Một SKU được Product Activated chưa chắc được bán.

Một SKU chỉ được bán khi đồng thời đạt:

- SKU canonical hợp lệ.

- Recipe/BOM/Formula version hợp lệ.

- Operational Config hợp lệ.

- Packaging Config hợp lệ.

- Print Config hợp lệ nếu cần in.

- QC Config hợp lệ.

- HSD Config hợp lệ.

- Trace Config hợp lệ nếu public trace.

- Recall Config hợp lệ.

- Trade Item Config hợp lệ nếu bán theo hộp/thùng.

- Batch đã sản xuất.

- QC đạt theo rule.

- Batch đã RELEASED.

- Warehouse Receipt đã confirmed.

- Inventory Ledger có tồn khả dụng.

- Không bị hold.

- Không bị recall.

- Không bị sale lock.

- Sellable Gate của Operational Core đạt.

- Consumer Guard không block.

Nếu thiếu bất kỳ điều kiện nào, SKU không được gọi là sellable.

## 138. PRODUCT ACTIVATION LÀ ĐIỀU KIỆN CẦN

Product Activation là điều kiện cần để SKU tham gia vận hành.

Product Activation không phải điều kiện đủ để bán.

Các tầng sau đây phải được phân biệt rõ:

Tầng | Ý nghĩa | Có được bán không

Product Master Approved | SKU được công nhận trong danh mục sản phẩm | Không

Recipe/BOM Active | Công thức G1 được phép dùng | Không

Operational Config Ready | SKU đủ cấu hình vận hành | Không

Product Activated | SKU được kích hoạt trong phạm vi vận hành | Chưa chắc

Production Completed | Batch đã sản xuất xong | Không

QC_PASS | Kết quả QC đạt | Không

RELEASED | Batch được release | Chưa chắc

Warehouse Receipt Confirmed | Hàng đã nhập kho | Chưa chắc

Inventory Available | Có tồn khả dụng | Chưa chắc

Sellable Gate PASS | Đủ điều kiện bán | Có, nếu không bị consumer guard khác block

## 139. PRODUCT ACTIVATION OWNER BOUNDARY

## 139.1. Product Master Owner

Product Master Owner chịu trách nhiệm đề xuất và quản trị trạng thái activation ở tầng sản phẩm.

Product Master Owner không được tự xác nhận sellable cuối cùng.

## 139.2. Recipe / Formula Owner

Recipe / Formula Owner chịu trách nhiệm xác nhận công thức, version, BOM, ingredient và định lượng đã đủ điều kiện dùng cho activation.

Recipe / Formula Owner không được tự mở bán.

## 139.3. Operational Config Owner

Operational Config Owner chịu trách nhiệm xác nhận các cấu hình vận hành theo SKU đã đủ điều kiện.

Operational Config Owner không được tự pass production, QC, release hoặc warehouse.

## 139.4. Operational Core Owner

Operational Core Owner chịu trách nhiệm xác nhận batch thật, stock thật, trace thật, recall status thật và sellable status thật.

Operational Core Owner là nơi quyết định trạng thái vận hành thực tế của hàng hóa.

## 139.5. Compliance / Claim Owner

Compliance / Claim Owner chịu trách nhiệm xác nhận nội dung public, claim, trace public, dietary/allergen disclosure và ranh giới không lộ dữ liệu nhạy cảm.

Không được public SKU nếu claim/public content chưa qua guard.

## 139.6. Release Owner

Release Owner chịu trách nhiệm xem xét evidence, smoke, done gate và quyết định có cho phép chuyển PACK-02 sang trạng thái đủ điều kiện handoff hay không.

Release Owner không được approve nếu còn thiếu evidence, smoke hoặc audit.

## 140. PRODUCT ACTIVATION STATUS MODEL

Product Activation phải có trạng thái rõ ràng.

Các trạng thái tối thiểu:

- Not Created.

- Draft.

- waiting Review.

- Config Incomplete.

- Config Ready.

- Evidence waiting.

- Smoke waiting.

- Activation Requested.

- Activation Approved.

- Activated For Internal Operations.

- Activated For Consumer Candidate.

- Sellable Eligible.

- Sellable bị chặn.

- Suspended.

- Retired.

Không được dùng một trạng thái duy nhất kiểu “Active” để đại diện cho mọi tầng.

## 141. Ý NGHĨA CÁC TRẠNG THÁI ACTIVATION

Trạng thái | Ý nghĩa quản trị

Not Created | Chưa có hồ sơ activation cho SKU

Draft | Đang tạo hồ sơ activation

waiting Review | Đang chờ owner review

Config Incomplete | Thiếu cấu hình bắt buộc

Config Ready | Đủ cấu hình cơ bản nhưng chưa đủ evidence/smoke

Evidence waiting | Thiếu evidence

Smoke waiting | Chưa chạy smoke hoặc smoke chưa đạt

Activation Requested | Đã gửi yêu cầu kích hoạt

Activation Approved | Owner đã duyệt activation ở tầng Product

Activated For Internal Operations | Được phép xuất hiện trong vận hành nội bộ theo phạm vi

Activated For Consumer Candidate | Có thể hiển thị có điều kiện cho consumer nếu guard cho phép

Sellable Eligible | Có thể đi qua Sellable Gate khi batch/stock/recall hợp lệ

Sellable bị chặn | Bị chặn bán bởi Operational Core hoặc Consumer Guard

Suspended | Tạm dừng

Retired | Ngừng sử dụng

## 142. PRODUCT ACTIVATION DEPENDENCY CHAIN

Product Activation phải phụ thuộc theo chuỗi sau:

- Product Master Approved.

- SKU Canonical Approved.

- Ingredient Canonical Approved.

- Recipe G1 Approved.

- BOM G1 Approved.

- Formula Version G1 Active.

- Recipe/BOM Snapshot Rule Ready.

- Operational Config Ready.

- Packaging Config Ready.

- Print Config Ready nếu có in.

- QC Config Ready.

- HSD Config Ready.

- Trace Config Ready nếu có trace.

- Recall Config Ready.

- Trade Item Config Ready nếu bán theo hộp/thùng.

- GTIN/GS1 Boundary Ready nếu áp dụng.

- Public Claim / Public Content Guard Ready nếu hiển thị public.

- Evidence Packet Ready.

- Smoke Run Completed.

- Owner Sign-off.

- Release Gate Review.

Thiếu một mắt xích thì không được activation hoàn chỉnh.

## 143. PRODUCT ACTIVATION KHÔNG ĐƯỢC TỰ MỞ NẾU OPERATIONAL CORE / SELLABLE GATE CHƯA ĐẠT

Product Activation chỉ được bật ở phạm vi Product / Config khi các cấu hình sản phẩm đã đạt.

Tuy nhiên, activation không được mở ra bán nếu Operational Core chưa xác nhận:

- Batch thật.

- QC thật.

- Release thật.

- Warehouse Receipt thật.

- Inventory Ledger thật.

- Stock available thật.

- Không hold.

- Không recall.

- Không sale lock.

- Sellable Gate đạt.

Nếu Operational Core hoặc Sellable Gate chưa đạt, SKU chỉ được ở trạng thái cấu hình hoặc vận hành nội bộ, không được bán ra consumer.

## 144. PRODUCT ACTIVATION RESOLVER

Product Activation Resolver là lớp trả trạng thái kích hoạt SKU cho các consumer.

Resolver phải trả trạng thái đã qua guard, không trả dữ liệu thô.

Product Activation Resolver phải phân biệt tối thiểu:

- SKU exists.

- SKU approved.

- Product Master ready.

- Ingredient ready.

- Recipe/BOM ready.

- Formula active.

- Operational Config ready.

- Packaging Config ready.

- Print Config ready.

- QC Config ready.

- HSD Config ready.

- Trace Config ready.

- Recall Config ready.

- Trade Item Config ready.

- Product Activation status.

- Operational Sellable status.

- Consumer visibility status.

- Suppression status.

- Block reason.

- Required next action.

Không được trả một kết quả chung như “active = true” cho mọi mục đích.

## 145. PRODUCT ACTIVATION GUARD

Product Activation Guard là lớp chặn kích hoạt sai điều kiện.

Product Activation Guard phải block khi:

- SKU missing.

- SKU chưa approved.

- Tên sản phẩm chưa approved.

- Nhóm sản phẩm thiếu hoặc conflict.

- Vegan / mặn thiếu hoặc conflict.

- Ingredient Canonical thiếu.

- Ingredient bị suspended/retired.

- Recipe missing.

- BOM missing.

- Formula Version missing.

- Formula G1 chưa active.

- Có nhiều formula active conflict.

- BOM có dòng mơ hồ.

- Quantity missing.

- Unit missing.

- Snapshot rule missing.

- Operational Config missing.

- Packaging Config missing.

- Print Config missing khi cần in.

- QC Config missing.

- HSD Config missing.

- Trace Config missing nếu cần public trace.

- Recall Config missing.

- Trade Item Config missing nếu bán theo hộp/thùng.

- GTIN/GS1 conflict nếu áp dụng.

- QR/barcode conflict.

- Public Claim Guard chưa đạt.

- Evidence missing.

- Smoke missing.

- Owner sign-off missing.

- Audit missing.

- Operational Core block.

- Sellable Gate block.

- Sale Lock active.

- Recall active.

- Hold active.

- Consumer Suppression active.

Nếu guard block, consumer không được tự bỏ qua.

## 146. PRODUCT ACTIVATION VÀ CONSUMER VISIBILITY

Consumer Visibility là việc SKU được nhìn thấy bởi AI Advisor, CRM, ADS, MC AI, Gateway, Landing Page, Order, Quote, Payment, Shipping, IVR hoặc Public Trace.

Consumer Visibility không được bật chỉ vì SKU đã có Product Master.

Consumer Visibility phải phụ thuộc vào:

- Product Activation status.

- Product Knowledge Guard.

- Public Claim Guard.

- Dietary/allergen guard.

- Sellable status nếu consumer có mục đích bán.

- Trace/Public Trace status nếu consumer có mục đích truy xuất.

- Sale Lock / Recall / Hold status.

- Channel policy.

- Runtime Resolver.

## 147. ACTIVATION SCOPE THEO CONSUMER

Consumer | Điều kiện tối thiểu

AI Advisor | Product Knowledge public-safe + Sellable Guard nếu tư vấn bán

CRM | Product Activation + Sellable/No-Sale-Lock Guard

ADS | Product Activation + Campaign Eligibility + Sellable Guard

MC AI | Product Activation + Live Board + Sellable Guard

Gateway | Product Activation + Channel Policy + Suppression Guard

Landing Page | Product Activation + Public Claim Guard + Sellable Guard nếu có CTA mua

Quote | Product Activation + Trade Item + Price/Sellable Guard

Order | Product Activation + Trade Item + Inventory/Sellable Guard

Payment | Order hợp lệ, không tự kiểm SKU rời

Shipping | Order/Trade Item hợp lệ

IVR | Order Confirmation Only, không tự bán SKU

Public Trace | Trace Config + Public Whitelist + Batch/Trace Guard

## 148. PRODUCT ACTIVATION VÀ QUOTE / ORDER

Quote và Order không được tạo từ SKU chỉ mới Product Activated.

Quote và Order phải kiểm tra:

- SKU có được kích hoạt đúng scope không.

- Trade item bán là hộp hay thùng.

- SKU có sellable không.

- Có tồn khả dụng không.

- Có đang hold/recall/sale lock không.

- Giá có được resolver trả về không.

- Chính sách chương trình có hợp lệ không.

- QuoteSnapshot có đủ dữ liệu không.

- Order không vi phạm suppress rule.

Product Activation không được tự tạo quyền quote/order.

## 149. PRODUCT ACTIVATION VÀ PUBLIC TRACE

Public Trace không được tự bật khi Product Activation được duyệt.

Public Trace chỉ được bật khi:

- Trace Config ready.

- Public Trace Whitelist approved.

- Batch linkage valid.

- Packaging/QR linkage valid.

- Batch release status phù hợp.

- Recall/Sale Lock policy phù hợp.

- Dữ liệu public không chứa thông tin nội bộ nhạy cảm.

- Evidence ready.

- Audit ready.

Nếu thiếu bất kỳ điều kiện nào, QR có thể tồn tại nội bộ nhưng public trace không được mở.

## 150. PRODUCT ACTIVATION VÀ RECALL / SALE LOCK

Product Activation không được thắng Recall / Sale Lock.

Nếu SKU, batch, lot, trade item hoặc inventory bị recall/sale lock, mọi trạng thái consumer phải bị suppress theo rule.

Các kênh phải dừng:

- AI Advisor chốt đơn.

- CRM gợi ý mua.

- ADS quảng bá.

- MC AI bán trong live.

- Gateway tạo handoff bán hàng.

- Landing Page CTA mua.

- Quote.

- Order.

- Payment continuation nếu order chưa hợp lệ.

- Shipping nếu order bị block theo rule.

- IVR xác nhận đơn nếu đơn bị invalid.

- Public Trace theo policy.

## 151. PRODUCT ACTIVATION VÀ FIELD APP

Field App chỉ được thấy SKU đã được kích hoạt theo phạm vi vận hành nội bộ.

Field App không được:

- Tự kích hoạt SKU.

- Tự bật công thức.

- Tự đổi BOM.

- Tự chọn config khác.

- Tự bỏ guard.

- Tự xác nhận sellable.

- Tự bỏ sale lock.

- Tự public trace.

- Tự tạo trade item.

- Tự sửa GTIN/GS1.

Field App chỉ hiển thị/capture theo dữ liệu đã được backend/domain service cấp.

## 152. PRODUCT ACTIVATION VÀ ADMIN WEB

Admin Web có thể là giao diện để owner review, phê duyệt, xem trạng thái và theo dõi audit.

Admin Web không phải Owner Core.

Admin Web không được bypass:

- Product Guard.

- Recipe/BOM Guard.

- Operational Config Guard.

- Packaging Guard.

- Print Guard.

- QC Guard.

- HSD Guard.

- Trace Guard.

- Recall Guard.

- Trade Item Guard.

- Sellable Gate.

- Release Gate.

Mọi phê duyệt từ Admin Web phải qua backend/domain service, audit và permission.

## 153. PRODUCT ACTIVATION AUDIT

Mọi hành động liên quan Product Activation phải ghi audit.

Audit tối thiểu gồm:

- activation_id.

- sku_id.

- actor_id.

- actor_role.

- action.

- from_status.

- to_status.

- reason.

- approval_reference nếu có.

- evidence_reference.

- smoke_reference nếu có.

- correlation_id.

- created_at.

- source_channel.

- object binding.

Không được kích hoạt SKU âm thầm.

## 154. PRODUCT ACTIVATION EVIDENCE PRINCIPLE

Không có evidence thì không Product Activation PASS.

Evidence không phải lời xác nhận miệng.

Evidence phải là hồ sơ, log, biên bản, ảnh, video, file, snapshot, audit, smoke result hoặc owner sign-off có thể kiểm tra lại.

Evidence phải gắn đúng object nghiệp vụ.

Evidence không được mồ côi.

## 155. PRODUCT ACTIVATION EVIDENCE PACKET

Mỗi SKU khi activation phải có Product Activation Evidence Packet.

Mô hình chuẩn:

01 SKU / 01 Activation Packet / 01 Evidence Packet

Evidence Packet phải liên kết được với:

- product_master_id.

- sku_id.

- formula_version_id.

- recipe_version_id.

- bom_version_id.

- operational_config_id.

- packaging_config_id.

- print_config_id.

- qc_config_id.

- shelf_life_config_id.

- trace_config_id.

- recall_config_id.

- trade_item_config_id nếu có.

- activation_id.

- smoke_run_id.

- audit_id.

- owner_signoff_id.

## 156. EVIDENCE CHECKLIST CHO PRODUCT MASTER / SKU

Evidence cho Product Master / SKU tối thiểu gồm:

- Hồ sơ tạo SKU.

- Hồ sơ tên sản phẩm public.

- Hồ sơ tên sản phẩm nội bộ.

- Hồ sơ nhóm sản phẩm.

- Hồ sơ phân loại vegan / mặn.

- Hồ sơ trạng thái SKU.

- Hồ sơ owner approval.

- Audit tạo / sửa / approve SKU.

- Evidence xác nhận không trùng định danh.

- Evidence xác nhận SKU thuộc danh mục 20 SKU canonical.

## 157. EVIDENCE CHECKLIST CHO INGREDIENT

Evidence cho Ingredient tối thiểu gồm:

- Danh mục ingredient canonical.

- Hồ sơ ingredient_id / ingredient_code.

- Hồ sơ phân loại nguyên liệu.

- Hồ sơ đơn vị tính.

- Hồ sơ dietary compatibility.

- Hồ sơ allergen/restriction nếu có.

- Hồ sơ trace relevance.

- Hồ sơ recall relevance.

- Hồ sơ QC requirement.

- Hồ sơ owner approval.

- Audit tạo / sửa / approve ingredient.

## 158. EVIDENCE CHECKLIST CHO RECIPE / BOM / FORMULA

Evidence cho Recipe / BOM / Formula tối thiểu gồm:

- Hồ sơ G1 formula.

- Hồ sơ G0 reference nếu có.

- Hồ sơ Recipe Header từng SKU.

- Hồ sơ BOM từng SKU.

- Danh sách ingredient line đầy đủ.

- Hàm lượng từng nguyên liệu.

- Đơn vị từng nguyên liệu.

- Ingredient role.

- Lot tracking rule.

- QC-before-issue rule.

- Trace relevance.

- Recall relevance.

- Dietary/allergen review.

- Owner approval.

- Version history.

- Effective date.

- Audit tạo / approve / activate.

- Snapshot test evidence.

## 159. EVIDENCE CHECKLIST CHO OPERATIONAL CONFIG

Evidence cho Operational Config tối thiểu gồm:

- Hồ sơ Operational Config theo SKU.

- Hồ sơ mapping SKU với formula/recipe/BOM version.

- Hồ sơ trạng thái config.

- Hồ sơ dependency với packaging/print/QC/HSD/trace/recall/trade item.

- Hồ sơ owner approval.

- Audit approve config.

- Evidence xác nhận không hardcode runtime config.

## 160. EVIDENCE CHECKLIST CHO PACKAGING / PRINT

Evidence cho Packaging / Print tối thiểu gồm:

- Hồ sơ Packaging Config cấp 1.

- Hồ sơ Packaging Config cấp 2 hộp.

- Hồ sơ Packaging Config cấp 2 thùng.

- Hồ sơ template in cấp 1.

- Hồ sơ template in hộp.

- Hồ sơ template in thùng.

- Hồ sơ test print hộp.

- Hồ sơ test print thùng.

- Hồ sơ print payload.

- Hồ sơ reprint rule.

- Hồ sơ QR rule.

- Hồ sơ barcode rule.

- Hồ sơ GTIN/GS1 boundary nếu áp dụng.

- Audit print/reprint.

- Evidence xác nhận máy in không sinh business truth.

## 161. EVIDENCE CHECKLIST CHO QC / HSD

Evidence cho QC / HSD tối thiểu gồm:

- Hồ sơ QC Config theo SKU hoặc nhóm SKU.

- Hồ sơ checklist QC.

- Hồ sơ ngưỡng QC.

- Hồ sơ ngưỡng độ ẩm sau sấy.

- Hồ sơ pass/hold/fail rule.

- Hồ sơ Batch Release dependency.

- Hồ sơ Shelf Life Config.

- Hồ sơ HSD 12 tháng theo SKU hoặc nhóm SKU.

- Hồ sơ NSX source.

- Hồ sơ HSD calculation rule.

- Hồ sơ audit thay đổi QC/HSD.

- Evidence xác nhận QC_PASS không tự chuyển RELEASED.

## 162. EVIDENCE CHECKLIST CHO TRACE / RECALL

Evidence cho Trace / Recall tối thiểu gồm:

- Hồ sơ Trace Config.

- Hồ sơ trace level.

- Hồ sơ raw material lot linkage.

- Hồ sơ Production Order linkage.

- Hồ sơ batch linkage.

- Hồ sơ packaging linkage.

- Hồ sơ QR linkage.

- Hồ sơ Public Trace Whitelist.

- Hồ sơ Recall Config.

- Hồ sơ recall scope rule.

- Hồ sơ affected batch/lot/packaging rule.

- Hồ sơ hold rule.

- Hồ sơ sale lock rule.

- Hồ sơ recovery/disposition rule.

- Hồ sơ CAPA linkage nếu có.

- Evidence xác nhận recall dùng traceability chain, không dựng chain riêng.

## 163. EVIDENCE CHECKLIST CHO TRADE ITEM / GTIN / GS1

Evidence cho Trade Item tối thiểu gồm:

- Hồ sơ trade item hộp.

- Hồ sơ trade item thùng.

- Hồ sơ unit quantity.

- Hồ sơ sellable unit rule.

- Hồ sơ warehouse handling rule.

- Hồ sơ shipping handling rule nếu có.

- Hồ sơ barcode rule.

- Hồ sơ GTIN/GS1 mapping nếu áp dụng.

- Hồ sơ xác nhận không có hai barcode thương mại active trên cùng trade item.

- Hồ sơ owner approval.

- Audit gán/thay đổi trade item.

- Evidence xác nhận QR và GTIN/GS1 không thay thế nhau.

## 164. EVIDENCE CHECKLIST CHO CONSUMER SUPPRESSION

Evidence cho Consumer Suppression tối thiểu gồm:

- Hồ sơ suppression rule.

- Hồ sơ suppress theo SKU chưa active.

- Hồ sơ suppress theo SKU thiếu config.

- Hồ sơ suppress theo SKU out-of-stock.

- Hồ sơ suppress theo hold.

- Hồ sơ suppress theo recall.

- Hồ sơ suppress theo sale lock.

- Hồ sơ suppress theo trace gap.

- Hồ sơ suppress theo QR/barcode conflict.

- Hồ sơ suppress theo HSD conflict.

- Hồ sơ test consumer không bán khi bị block.

- Audit consumer block/suppression.

## 165. SMOKE TEST PRINCIPLE CHO PACK-02

Smoke test của PACK-02 phải chứng minh rằng Product Master, SKU, Ingredient, Recipe, BOM, Formula Version, Config, Activation và Consumer Suppression hoạt động đúng ở tầng governance/domain.

Smoke test không được chỉ kiểm tra giao diện.

Smoke test không được chỉ kiểm tra dữ liệu mẫu.

Smoke test phải chứng minh guard, resolver, audit, evidence và dependency hoạt động đúng.

Không smoke thì không PASS.

## 166. PACK-02 SMOKE RUN ID

Mỗi lần chạy smoke cho PACK-02 phải có product_master_smoke_run_id.

Smoke run phải gắn với:

- Danh sách SKU được test.

- Product Master version.

- Recipe/BOM version.

- Config version.

- Activation version.

- Actor.

- Environment.

- Time.

- Evidence packet.

- Result.

- Failure reason nếu có.

- Owner review.

Không được ghi nhận smoke bằng text rời không có định danh.

## 167. PACK-02 SMOKE MATRIX

Smoke ID | Tên smoke | Mục tiêu

PM-SMK-001 | SKU Canonical Registry | Kiểm tra 20 SKU có bản ghi riêng, không gom nhóm

PM-SMK-002 | Product Name / Group / Dietary | Kiểm tra tên, nhóm, vegan/mặn được lấy từ Product Master

PM-SMK-003 | Ingredient Canonical | Kiểm tra ingredient có định danh, phân loại, unit, trace/recall flag

PM-SMK-004 | INGREDIENT AMBIGUITY CHECK | Kiểm tra không có “nền chung”, “nêm chung”, “nước hầm chung” ở downstream

PM-SMK-005 | G0 / G1 Formula Boundary | Kiểm tra G0 là lịch sử, G1 là công thức vận hành

PM-SMK-006 | Recipe Header Per SKU | Kiểm tra mỗi SKU có Recipe Header đầy đủ

PM-SMK-007 | BOM Line Completeness | Kiểm tra mỗi BOM line có ingredient_id, quantity, unit, role

PM-SMK-008 | Formula Version Control | Kiểm tra thay đổi sau G1 tạo version mới, không sửa đè

PM-SMK-009 | BOM Snapshot | Kiểm tra Production Order tạo BOM snapshot bất biến

PM-SMK-010 | Field App Snapshot View | Kiểm tra Field App chỉ hiển thị theo snapshot, không cho chọn tay nguyên liệu

PM-SMK-011 | Operational Config Ready | Kiểm tra SKU thiếu Operational Config bị block

PM-SMK-012 | Packaging Config L1/L2 | Kiểm tra SKU thiếu Packaging Config bị block

PM-SMK-013 | Print Config Box/Carton | Kiểm tra thiếu print config hộp/thùng thì không in cấp 2

PM-SMK-014 | QC / HSD Config | Kiểm tra QC threshold, QC_PASS != RELEASED, HSD theo config

PM-SMK-015 | Trace / Public Trace Config | Kiểm tra chưa bật Trace Config thì không public trace

PM-SMK-016 | Recall Config | Kiểm tra thiếu Recall Config thì không recall-ready

PM-SMK-017 | Trade Item / GTIN / GS1 | Kiểm tra hộp/thùng trade item, không trùng barcode thương mại

PM-SMK-018 | Product Activation Guard | Kiểm tra activation bị block nếu thiếu config/evidence/smoke

PM-SMK-019 | Sellable Dependency | Kiểm tra Product Activation không vượt Sellable Gate

PM-SMK-020 | Consumer Suppression | Kiểm tra AI/CRM/ADS/Order/Quote/Public Trace bị suppress khi SKU bị block

PM-SMK-021 | No-Hardcode Runtime Data | Kiểm tra không hardcode tên, config, HSD, QC, GTIN, QR, sellable, sale lock

PM-SMK-022 | Evidence Packet Close-out | Kiểm tra mỗi SKU có Activation Evidence Packet

PM-SMK-023 | Audit Completeness | Kiểm tra mọi approve/activate/block đều có audit

PM-SMK-024 | Final PACK-02 Close-out | Kiểm tra toàn bộ chain SKU -> Recipe -> Config -> Activation -> Suppression

## 168. PM-SMK-001 — SKU CANONICAL REGISTRY

Mục tiêu: kiểm tra 20 SKU canonical là danh mục nền riêng biệt.

Điều kiện đạt:

- Có đủ 20 SKU.

- Mỗi SKU có sku_id riêng.

- Mỗi SKU có sku_code riêng.

- Mỗi SKU có tên public.

- Mỗi SKU có tên internal.

- Mỗi SKU có nhóm sản phẩm.

- Mỗi SKU có phân loại vegan / mặn.

- Không có dòng SKU gom nhóm.

- Không có SKU mơ hồ.

- Có audit và evidence.

Fail nếu:

- Thiếu SKU.

- Trùng SKU.

- Gom nhiều SKU vào một dòng.

- Dùng nhóm sản phẩm thay SKU.

- Thiếu owner approval.

## 169. PM-SMK-002 — PRODUCT NAME / GROUP / DIETARY

Mục tiêu: kiểm tra tên, nhóm và phân loại chế độ ăn được quản trị tập trung.

Điều kiện đạt:

- Tên sản phẩm public lấy từ Product Master.

- Tên sản phẩm internal lấy từ Product Master.

- Nhóm sản phẩm lấy từ Product Master.

- Vegan / mặn lấy từ Product Master.

- Consumer không tự suy luận vegan / mặn.

- Có guard chặn dietary conflict.

Fail nếu:

- AI Advisor tự đoán vegan / mặn.

- Landing Page hardcode tên sản phẩm.

- CRM dùng nhóm sản phẩm thay SKU.

- Order nhận SKU không có dietary_type.

## 170. PM-SMK-003 — INGREDIENT CANONICAL

Mục tiêu: kiểm tra nguyên liệu có canonical registry.

Điều kiện đạt:

- Mỗi nguyên liệu có ingredient_id.

- Có ingredient_code.

- Có canonical name.

- Có ingredient_type.

- Có unit_of_measure.

- Có dietary compatibility.

- Có allergen/restriction nếu có.

- Có trace relevance.

- Có recall relevance.

- Có QC requirement.

- Có audit/evidence.

Fail nếu:

- BOM line chỉ là text.

- Nguyên liệu thiếu định danh.

- Nguyên liệu bị suspended vẫn dùng.

- Nguyên liệu thiếu trace/recall flag.

## 171. PM-SMK-004 — INGREDIENT AMBIGUITY CHECK

Mục tiêu: kiểm tra không có dòng nguyên liệu gom nhóm mơ hồ ở downstream.

Các dòng phải bị chặn:

- Nền chung.

- Nước hầm chung.

- Nêm chung.

- Gia vị chung.

- Rau củ chung.

- Dược liệu chung.

- Phụ liệu chung.

- Hỗn hợp nền.

- Base công thức.

- Nguyên liệu khác.

Điều kiện đạt:

- Mọi dòng gom nhóm đều được bung thành nguyên liệu cụ thể.

- Có quantity.

- Có unit.

- Có ingredient_id.

- Có trace/recall relevance.

Fail nếu BOM sản xuất còn dòng mơ hồ.

## 172. PM-SMK-005 — G0 / G1 FORMULA BOUNDARY

Mục tiêu: kiểm tra G0 và G1 được tách đúng.

Điều kiện đạt:

- G0 là research/history reference.

- G1 là công thức vận hành.

- Không dùng G0 trực tiếp cho sản xuất thường.

- G1 có owner approval.

- G1 có effective date.

- G1 có audit.

- G1 có evidence.

- Thay đổi sau G1 tạo version mới.

Fail nếu:

- Sửa G0 để thay công thức vận hành.

- Sửa đè G1 đã dùng.

- Không có version history.

- Không có evidence.

## 173. PM-SMK-006 — RECIPE HEADER PER SKU

Mục tiêu: kiểm tra mỗi SKU có Recipe Header đầy đủ.

Điều kiện đạt:

- sku_id.

- sku_code.

- public_product_name.

- internal_product_name.

- product_group.

- dietary_type.

- formula_code.

- formula_version.

- recipe_version_id.

- bom_version_id.

- recipe_status.

- formula_status.

- effective_from.

- approved_by.

- approved_at.

- audit_reference.

- evidence_reference.

Fail nếu thiếu Recipe Header hoặc dùng Recipe Header chung cho nhiều SKU.

## 174. PM-SMK-007 — BOM LINE COMPLETENESS

Mục tiêu: kiểm tra từng BOM line đủ thông tin vận hành.

Điều kiện đạt:

- bom_line_id.

- ingredient_id.

- ingredient_code.

- ingredient_canonical_name.

- ingredient_type.

- ingredient_role.

- quantity.

- unit_of_measure.

- lot_tracking_required.

- qc_required_before_issue.

- dietary_relevance.

- allergen_relevance nếu có.

- trace_relevance.

- recall_relevance.

- status.

- audit_reference.

Fail nếu BOM line thiếu quantity/unit hoặc không gắn ingredient_id.

## 175. PM-SMK-008 — FORMULA VERSION CONTROL

Mục tiêu: kiểm tra version công thức không bị sửa đè.

Điều kiện đạt:

- G1 đã active không bị sửa trực tiếp sau khi dùng.

- Thay đổi quantity tạo version mới.

- Thêm/bỏ ingredient tạo version mới.

- Đổi dietary/allergen/trace/recall flag tạo version mới.

- Version mới có owner approval.

- Version mới có audit/evidence.

- Effective date rõ ràng.

Fail nếu dev/admin sửa trực tiếp công thức active đã có lịch sử sản xuất.

## 176. PM-SMK-009 — BOM SNAPSHOT

Mục tiêu: kiểm tra BOM snapshot khi tạo Production Order.

Điều kiện đạt:

- Production Order có BOM snapshot.

- Snapshot gắn sku_id.

- Snapshot gắn formula_version_id.

- Snapshot gắn recipe_version_id.

- Snapshot gắn bom_version_id.

- Snapshot có ingredient list.

- Snapshot có quantity/unit.

- Snapshot có audit/correlation_id.

- Snapshot bất biến sau khi tạo.

- Công thức mới không cập nhật ngược Production Order cũ.

Fail nếu Production Order chỉ tham chiếu động recipe active hiện tại.

## 177. PM-SMK-010 — FIELD APP SNAPSHOT VIEW

Mục tiêu: kiểm tra Field App chỉ hiển thị theo BOM snapshot.

Điều kiện đạt:

- Field App hiển thị ingredient từ snapshot.

- Field App hiển thị quantity từ snapshot.

- Field App không cho chọn tay nguyên liệu.

- Field App không cho sửa công thức.

- Field App không cho đổi BOM.

- Field App không cho bypass guard.

- Capture evidence gắn đúng object.

Fail nếu Field App cho người vận hành chọn tay nguyên liệu trong sản xuất thường.

## 178. PM-SMK-011 — OPERATIONAL CONFIG READY

Mục tiêu: kiểm tra SKU thiếu Operational Config bị block.

Điều kiện đạt:

- SKU có Operational Config.

- Operational Config gắn formula/recipe/BOM version.

- Operational Config gắn packaging/print/QC/HSD/trace/recall/trade item.

- Config có owner approval.

- Config có audit/evidence.

- Resolver trả đúng trạng thái.

Fail nếu SKU có recipe nhưng vẫn được activation khi thiếu Operational Config.

## 179. PM-SMK-012 — PACKAGING CONFIG L1/L2

Mục tiêu: kiểm tra Packaging Config cấp 1/cấp 2.

Điều kiện đạt:

- Có Packaging Config cấp 1 nếu quy trình yêu cầu.

- Có Packaging Config hộp.

- Có Packaging Config thùng nếu dùng thùng.

- Có lot/NSX/HSD/trace binding.

- Có trade item mapping nếu hộp/thùng là trade item.

- Có audit/evidence.

Fail nếu SKU được đóng gói khi thiếu Packaging Config.

## 180. PM-SMK-013 — PRINT CONFIG BOX / CARTON

Mục tiêu: kiểm tra Print Config hộp/thùng.

Điều kiện đạt:

- Có template hộp.

- Có template thùng.

- Có print payload.

- Có lot binding.

- Có NSX/HSD source.

- Có QR rule nếu áp dụng.

- Có barcode rule nếu áp dụng.

- Có reprint rule.

- Có audit/evidence.

Fail nếu in cấp 2 khi thiếu Print Config hoặc hardcode dữ liệu trên template.

## 181. PM-SMK-014 — QC / HSD CONFIG

Mục tiêu: kiểm tra QC và HSD không hardcode.

Điều kiện đạt:

- Có QC Config.

- Có QC threshold.

- Có ngưỡng độ ẩm sau sấy trong config.

- QC_PASS không tự RELEASED.

- Có Shelf Life Config.

- HSD 12 tháng đi qua config.

- NSX/HSD lấy từ source hợp lệ.

- Có audit/evidence.

Fail nếu HSD được máy in tự cộng ngày hoặc QC_PASS tự chuyển batch RELEASED.

## 182. PM-SMK-015 — TRACE / PUBLIC TRACE CONFIG

Mục tiêu: kiểm tra Trace Config và Public Trace Whitelist.

Điều kiện đạt:

- Có Trace Config.

- Có raw material lot linkage.

- Có Production Order linkage.

- Có batch linkage.

- Có packaging/QR linkage.

- Có Public Trace Whitelist.

- Public Trace không lộ dữ liệu nhạy cảm.

- Có audit/evidence.

Fail nếu QR public trace mở khi Trace Config chưa ready.

## 183. PM-SMK-016 — RECALL CONFIG

Mục tiêu: kiểm tra Recall Config dùng traceability chain.

Điều kiện đạt:

- Có Recall Config.

- Có recall scope rule.

- Có affected batch rule.

- Có affected lot rule.

- Có affected packaging rule.

- Có hold rule.

- Có sale lock rule.

- Có recovery/disposition rule.

- Recall dựa trên trace chain thật.

- Có audit/evidence.

Fail nếu recall chạy bằng danh sách thủ công không gắn trace chain.

## 184. PM-SMK-017 — TRADE ITEM / GTIN / GS1

Mục tiêu: kiểm tra trade item hộp/thùng và GTIN/GS1 boundary.

Điều kiện đạt:

- Hộp có trade item nếu bán theo hộp.

- Thùng có trade item nếu bán/vận chuyển theo thùng.

- Có unit quantity.

- Có barcode rule.

- Có GTIN/GS1 mapping nếu áp dụng.

- Không có hai barcode thương mại active trên cùng trade item.

- QR không thay GTIN/GS1.

- Có audit/evidence.

Fail nếu user nhập tay barcode thương mại hoặc trùng barcode active.

## 185. PM-SMK-018 — PRODUCT ACTIVATION GUARD

Mục tiêu: kiểm tra Product Activation không vượt guard.

Điều kiện đạt:

- SKU thiếu config bị block.

- SKU thiếu evidence bị block.

- SKU thiếu smoke bị block.

- SKU thiếu owner sign-off bị block.

- SKU có recipe conflict bị block.

- SKU có trace/recall conflict bị block.

- SKU có trade item conflict bị block.

- Activation status thể hiện đúng block reason.

Fail nếu SKU được Activated dù thiếu điều kiện bắt buộc.

## 186. PM-SMK-019 — SELLABLE DEPENDENCY

Mục tiêu: kiểm tra Product Activation không vượt Sellable Gate.

Điều kiện đạt:

- SKU Product Activated nhưng chưa có batch thì không sellable.

- SKU có batch nhưng chưa QC thì không sellable.

- SKU QC_PASS nhưng chưa RELEASED thì không sellable.

- SKU RELEASED nhưng chưa Warehouse Receipt thì không sellable.

- SKU có tồn nhưng đang hold thì không sellable.

- SKU có tồn nhưng recall thì không sellable.

- SKU có tồn nhưng sale lock thì không sellable.

- Sellable Gate là điều kiện cuối.

Fail nếu Quote/Order bán SKU chỉ vì Product Activated.

## 187. PM-SMK-020 — CONSUMER SUPPRESSION

Mục tiêu: kiểm tra consumer bị suppress đúng khi SKU block.

Điều kiện đạt:

- AI Advisor không tư vấn chốt SKU bị block.

- CRM không gợi ý mua SKU bị block.

- ADS không scale SKU bị block.

- MC AI không bán SKU bị block.

- Gateway không tạo quote/order SKU bị block.

- Landing Page không CTA mua SKU bị block.

- Order/Quote không nhận SKU bị block.

- Public Trace không mở sai policy.

- IVR không xác nhận đơn invalid.

Fail nếu consumer vẫn bán hoặc gợi ý SKU bị Sale Lock / Recall / Config Missing.

## 188. PM-SMK-021 — NO-HARDCODE RUNTIME DATA

Mục tiêu: kiểm tra không hardcode runtime data.

Các dữ liệu không được hardcode:

- Tên sản phẩm.

- Nhóm sản phẩm.

- Vegan / mặn.

- Ingredient.

- Quantity.

- Unit.

- Recipe/BOM version.

- Operational Config.

- Packaging Config.

- Print Config.

- QC threshold.

- HSD.

- Trace Config.

- Recall Config.

- Trade Item.

- GTIN/GS1.

- QR.

- Barcode.

- Sellable status.

- Recall status.

- Sale lock status.

Fail nếu consumer hoặc template chứa dữ liệu cố định không qua resolver/config.

## 189. PM-SMK-022 — EVIDENCE PACKET CLOSE-OUT

Mục tiêu: kiểm tra Evidence Packet đủ và gắn đúng object.

Điều kiện đạt:

- Mỗi SKU có Evidence Packet.

- Evidence Packet gắn activation_id.

- Evidence Packet gắn sku_id.

- Evidence Packet gắn recipe/BOM/formula.

- Evidence Packet gắn config.

- Evidence Packet gắn smoke run.

- Evidence Packet gắn audit.

- Evidence Packet gắn owner sign-off.

Fail nếu evidence nằm rời, không gắn object hoặc không truy lại được.

## 190. PM-SMK-023 — AUDIT COMPLETENESS

Mục tiêu: kiểm tra mọi thay đổi đều có audit.

Điều kiện đạt:

- Tạo SKU có audit.

- Approve SKU có audit.

- Tạo ingredient có audit.

- Approve recipe/BOM có audit.

- Activate formula có audit.

- Approve config có audit.

- Product Activation có audit.

- Guard block có audit.

- Consumer suppression có audit.

- Owner sign-off có audit.

Fail nếu có thay đổi không có audit.

## 191. PM-SMK-024 — FINAL PACK-02 CLOSE-OUT

Mục tiêu: kiểm tra toàn bộ chain PACK-02 hoàn chỉnh.

Chuỗi smoke tối thiểu:

SKU Canonical -> Ingredient Canonical -> Recipe G1 -> BOM G1 -> Formula Version -> BOM Snapshot -> Operational Config -> Packaging Config -> Print Config -> QC Config -> HSD Config -> Trace Config -> Recall Config -> Trade Item -> Product Activation -> Sellable Dependency -> Consumer Suppression -> Evidence Packet -> Owner Review.

Fail nếu bất kỳ mắt xích nào thiếu evidence, audit, smoke hoặc guard.

## 192. DONE GATE CỦA PACK-02

PACK-02 chỉ được xem là hoàn tất về mặt governance khi đạt đủ các điều kiện sau:

- 20 SKU canonical được xác định là danh mục nền.

- Mỗi SKU có tên sản phẩm public.

- Mỗi SKU có tên sản phẩm nội bộ.

- Mỗi SKU có nhóm sản phẩm.

- Mỗi SKU có phân loại vegan / mặn.

- Ingredient Canonical được khóa nguyên tắc.

- Không dùng ingredient mơ hồ ở downstream.

- Sâm Savigin là ingredient canonical riêng.

- G0 và G1 được tách rõ.

- G1 là công thức vận hành.

- Mọi thay đổi sau G1 phải tạo version mới.

- Mỗi SKU có Recipe Header.

- Mỗi SKU có BOM G1.

- BOM line đủ ingredient, quantity, unit, role.

- Recipe/BOM Snapshot là bắt buộc.

- Field App chỉ hiển thị theo snapshot.

- Operational Config theo SKU là bắt buộc.

- Packaging Config cấp 1/cấp 2 là bắt buộc.

- Print Config hộp/thùng là bắt buộc nếu in cấp 2.

- QC Config là bắt buộc.

- HSD Config là bắt buộc.

- Trace Config là bắt buộc nếu public trace.

- Recall Config là bắt buộc.

- Trade Item Config là bắt buộc nếu bán theo hộp/thùng.

- GTIN/GS1 boundary được khóa.

- QR không thay GTIN/GS1.

- Product Activation không đồng nghĩa Sellable.

- Product Activation không được vượt Operational Core / Sellable Gate.

- Consumer Suppression được khóa.

- No-hardcode rule được khóa.

- Resolver / Guard / Audit / Evidence / Smoke được khóa.

- PACK-02 Smoke Matrix được xác định.

- PACK-02 Final Status được xác định.

## 193. IMPLEMENTATION DONE GATE

PACK-02 chưa được gọi implementation done nếu chưa có:

- Product Master runtime.

- SKU registry runtime.

- Ingredient registry runtime.

- Recipe/BOM registry runtime.

- Formula version runtime.

- BOM snapshot runtime.

- Operational Config runtime.

- Packaging Config runtime.

- Print Config runtime.

- QC Config runtime.

- HSD Config runtime.

- Trace Config runtime.

- Recall Config runtime.

- Trade Item runtime.

- Product Activation runtime.

- Product Resolver runtime.

- Product Guard runtime.

- Consumer Suppression runtime.

- Audit runtime.

- Evidence runtime.

- Smoke run thật.

- Owner sign-off thật.

Nếu các mục này chưa triển khai, trạng thái implementation phải là waiting_IMPLEMENTATION.

## 194. EVIDENCE DONE GATE

PACK-02 chưa được gọi evidence done nếu chưa có:

- Evidence Packet cho 20 SKU.

- Evidence cho Product Master.

- Evidence cho Ingredient.

- Evidence cho Recipe/BOM.

- Evidence cho Formula Version.

- Evidence cho Snapshot.

- Evidence cho Operational Config.

- Evidence cho Packaging Config.

- Evidence cho Print Config.

- Evidence cho QC Config.

- Evidence cho HSD Config.

- Evidence cho Trace Config.

- Evidence cho Recall Config.

- Evidence cho Trade Item Config.

- Evidence cho Product Activation.

- Evidence cho Consumer Suppression.

- Evidence cho No-Hardcode Check.

- Evidence cho Smoke Matrix.

- Evidence cho Owner Review.

- Evidence cho Release Gate.

Nếu thiếu evidence, trạng thái phải là waiting_EVIDENCE.

## 195. SMOKE DONE GATE

PACK-02 chưa được gọi smoke done nếu chưa chạy đủ:

- PM-SMK-001.

- PM-SMK-002.

- PM-SMK-003.

- PM-SMK-004.

- PM-SMK-005.

- PM-SMK-006.

- PM-SMK-007.

- PM-SMK-008.

- PM-SMK-009.

- PM-SMK-010.

- PM-SMK-011.

- PM-SMK-012.

- PM-SMK-013.

- PM-SMK-014.

- PM-SMK-015.

- PM-SMK-016.

- PM-SMK-017.

- PM-SMK-018.

- PM-SMK-019.

- PM-SMK-020.

- PM-SMK-021.

- PM-SMK-022.

- PM-SMK-023.

- PM-SMK-024.

Nếu smoke chưa chạy hoặc chưa đạt, trạng thái phải là waiting_SMOKE.

## 196. OWNER SIGN-OFF DONE GATE

PACK-02 chưa được owner sign-off nếu chưa có xác nhận của các owner liên quan:

- Product Master Owner.

- Recipe / Formula Owner.

- Ingredient Owner.

- Operational Config Owner.

- Packaging Owner.

- Print Owner.

- QC Owner.

- HSD Owner.

- Traceability Owner.

- Recall Owner.

- Trade Item Owner.

- Compliance / Claim Owner.

- Operational Core Owner.

- Release Owner.

Không owner sign-off thì không Release Approved.

## 197. RELEASE HANDOFF CỦA PACK-02

PACK-02 chỉ được handoff sang implementation khi tài liệu governance đã đủ rõ.

Release handoff không đồng nghĩa production ready.

Release handoff chỉ cho phép đội triển khai dùng PACK-02 làm nguồn quy tắc để thiết kế implementation.

Khi handoff, phải nêu rõ:

- PACK-02 Documentation Status.

- Implementation Status.

- Evidence Status.

- Smoke Status.

- Owner Sign-off Status.

- Production Ready Status.

- Release Approved Status.

- Go-live Approved Status.

- STOP_POINTS còn lại.

- Pack dependency với PACK-01.

## 198. PACK-02 PHỤ THUỘC PACK-01

PACK-02 phụ thuộc PACK-01 ở các điểm:

- Production Order.

- BOM Snapshot.

- Material Issue.

- Batch Genealogy.

- Packaging Execution.

- Print Payload.

- QC Result.

- Batch Release.

- Warehouse Receipt.

- Inventory Ledger.

- Traceability.

- Recall.

- Sale Lock.

- Sellable Gate.

- Evidence Packet.

- Smoke Run.

PACK-02 không thay thế PACK-01.

PACK-02 cung cấp sự thật sản phẩm và cấu hình sản phẩm.

PACK-01 cung cấp sự thật vận hành và sellable thực tế.

## 199. PACK-02 ẢNH HƯỞNG ĐẾN CÁC PACK / MODULE CONSUMER

PACK-02 là source-of-truth sản phẩm cho các consumer sau:

- AI Advisor.

- CRM.

- ADS.

- MC AI.

- Gateway.

- Landing Page.

- Quote.

- Order.

- Payment.

- Shipping.

- IVR.

- Public Trace.

- Reporting.

- MISA Integration Boundary.

- Admin Web.

- Field App.

Các consumer không được tự tạo bản sao rule kinh doanh.

Các consumer phải đọc qua resolver/guard phù hợp.

## 200. NO-HARDCODE FINAL LOCK

PACK-02 khóa no-hardcode ở mức P0.

Không được hardcode:

- SKU.

- Product name.

- Product group.

- Dietary type.

- Ingredient.

- Formula.

- Recipe.

- BOM.

- Quantity.

- Unit.

- Operational Config.

- Packaging Config.

- Print Config.

- QC threshold.

- HSD.

- Trace Config.

- Recall Config.

- Trade Item.

- GTIN/GS1.

- QR.

- Barcode.

- Activation status.

- Sellable status.

- Hold status.

- Recall status.

- Sale Lock status.

- Evidence status.

- Smoke status.

Mọi dữ liệu phải đi qua registry, config, resolver, guard, audit và evidence.

## 201. PACK-02 FAILURE CONDITIONS

PACK-02 phải được xem là fail nếu xảy ra một trong các lỗi sau:

- SKU không có canonical registry.

- Có SKU mơ hồ.

- Có SKU gom nhóm.

- Tên sản phẩm hardcode ở consumer.

- Vegan / mặn được consumer tự suy luận.

- Ingredient thiếu định danh.

- Ingredient gom nhóm mơ hồ ở BOM downstream.

- Recipe/BOM thiếu version.

- G1 bị sửa đè.

- G0 bị dùng trực tiếp cho sản xuất thường.

- BOM line thiếu quantity hoặc unit.

- Không có snapshot cho Production Order.

- Field App cho chọn tay nguyên liệu.

- SKU thiếu Operational Config vẫn activation.

- SKU thiếu Packaging Config vẫn đóng gói.

- SKU thiếu Print Config vẫn in cấp 2.

- SKU thiếu QC Config vẫn release.

- SKU thiếu HSD Config vẫn in HSD.

- SKU thiếu Trace Config vẫn public trace.

- SKU thiếu Recall Config vẫn recall bằng tay.

- Hộp/thùng thiếu Trade Item Config nhưng vẫn quote/order.

- GTIN/GS1 bị hardcode.

- Có hai barcode thương mại active trên cùng trade item.

- Product Activation bị hiểu là sellable.

- Quote/Order bán SKU chưa qua Sellable Gate.

- Consumer không suppress SKU bị sale lock/recall.

- Evidence missing nhưng vẫn PASS.

- Smoke missing nhưng vẫn PASS.

- Audit missing nhưng vẫn approve.

- Owner sign-off missing nhưng vẫn Release Approved.

## 202. PACK-02 FINAL STATUS

Sau khi hoàn tất 4 phần tài liệu, trạng thái governance của PACK-02 được xác định như sau:

Hạng mục | Trạng thái

PACK-02 Documentation | Governance Complete

Product Master Implementation | waiting_IMPLEMENTATION

SKU Registry Implementation | waiting_IMPLEMENTATION

Ingredient Registry Implementation | waiting_IMPLEMENTATION

Recipe/BOM Runtime | waiting_IMPLEMENTATION

Formula Version Runtime | waiting_IMPLEMENTATION

Operational Config Runtime | waiting_IMPLEMENTATION

Packaging Config Runtime | waiting_IMPLEMENTATION

Print Config Runtime | waiting_IMPLEMENTATION

QC/HSD Config Runtime | waiting_IMPLEMENTATION

Trace/Recall Config Runtime | waiting_IMPLEMENTATION

Trade Item Runtime | waiting_IMPLEMENTATION

Product Activation Runtime | waiting_IMPLEMENTATION

Resolver / Guard Runtime | waiting_IMPLEMENTATION

Evidence Packet | waiting_EVIDENCE

Smoke Run | waiting_SMOKE

Owner Sign-off | waiting_OWNER_SIGNOFF

Production Ready | KHÔNG

Release Approved | KHÔNG

Go-live Approved | KHÔNG

## 203. PACK-02 FINAL GOVERNANCE LOCK

PACK-02 khóa các nguyên tắc cuối cùng sau:

- 20 SKU canonical là danh mục sản phẩm nền.

- Mỗi SKU là một bản ghi riêng.

- Không dùng nhóm sản phẩm thay SKU.

- Không dùng tên gọi mơ hồ trong vận hành.

- Vegan / mặn là dữ liệu P0.

- Ingredient canonical là bắt buộc.

- Sâm Savigin là ingredient canonical riêng.

- Không dùng dòng nguyên liệu gom nhóm mơ hồ ở downstream.

- G0 là lịch sử / công thức gốc nghiên cứu.

- G1 là công thức vận hành hiện tại.

- Mọi thay đổi sau G1 phải tạo version mới.

- Không sửa đè công thức đã dùng.

- Không sản xuất SKU nếu thiếu Recipe/BOM version.

- Không tạo Production Order nếu thiếu BOM snapshot.

- Field App chỉ hiển thị/capture theo BOM snapshot.

- Field App không được chọn tay nguyên liệu trong sản xuất thường.

- Không đưa SKU vào vận hành nếu thiếu Operational Config.

- Không đóng gói SKU nếu thiếu Packaging Config.

- Không in mã cấp 2 nếu thiếu Print Config.

- Không nhập kho nếu thiếu QC/Release Config.

- Không public trace nếu thiếu Trace Config.

- Không recall chuẩn nếu thiếu Trace/Recall Config.

- Hộp/thùng có thể là trade item riêng.

- GTIN/GS1 phải qua Trade Item Registry.

- QR không thay GTIN/GS1.

- Không có hai barcode thương mại active trên cùng trade item.

- HSD 12 tháng phải qua Shelf Life Config.

- QC threshold phải qua QC Config.

- Product Activation không đồng nghĩa Sellable.

- Product Activation không được tự mở nếu Operational Core / Sellable Gate chưa đạt.

- Sellable cuối cùng thuộc Operational Core.

- Sale Lock / Recall / Hold thắng Product Activation.

- Consumer phải suppress khi SKU bị block.

- No-hardcode là P0.

- Resolver / Guard / Audit / Evidence / Smoke / Owner Sign-off là bắt buộc.

## 204. PACK-02 FINAL CONCLUSION

PACK-02 thiết lập lớp sự thật sản phẩm và cấu hình sản phẩm cho Ginsengfood.

PACK-02 xác định rõ một sản phẩm không chỉ là tên thương mại, mà là một chuỗi quản trị gồm SKU canonical, ingredient canonical, công thức G1, recipe, BOM, formula version, snapshot, operational config, packaging config, print config, QC config, HSD config, trace config, recall config, trade item và activation dependency.

PACK-02 cũng khóa ranh giới quan trọng: Product Master / SKU / Recipe không thay thế Operational Sellable.

Một SKU có thể hoàn chỉnh về Product Master nhưng vẫn chưa được bán nếu chưa có batch thật, QC thật, release thật, warehouse receipt thật, inventory thật, trace thật và không bị hold/recall/sale lock.

PACK-02 hoàn tất tầng governance cho Product Master / SKU / Ingredient / Recipe / Formula Version / Product Activation.

- Documentation Governance: Governance Complete.

- Runtime Implementation: waiting_IMPLEMENTATION.

- Evidence: waiting_EVIDENCE.

- Smoke: waiting_SMOKE.

- Owner Sign-off: waiting_OWNER_SIGNOFF.

PACK-02 được phép chuyển sang bước tài liệu tiếp theo theo trình tự pack đã khóa, nhưng không được dùng để tuyên bố production ready, release approved hoặc go-live approved nếu chưa có implementation, evidence, smoke và owner sign-off thật.

## HẾT PHẦN 4/4

## SKU–INGREDIENT–RECIPE–FORMULA VERSION–OPERATIONAL CONFIG CONTROL PACK
