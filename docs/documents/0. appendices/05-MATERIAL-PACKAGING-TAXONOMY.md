# MATERIAL PACKAGING TAXONOMY

## DANH MỤC PHÂN NHÓM NGUYÊN LIỆU / VẬT TƯ SẢN XUẤT GINSENGFOOD

Cấu trúc Nhóm A / Nhóm B + Công thức neo gạo + Ngưỡng tồn kho / Loại khỏi phiếu thu mua

Dùng cho PACK-03 MRP / Stock Alert / Material Master / Formula Scaling / Procurement Suppression Threshold

V1.2 CLEAN FINAL - Owner Lock - Rice Anchor Formula / Dynamic Threshold Locked

Nguồn dữ liệu: CÔNG THỨC VẬN HÀNH 20 SKU MỚI.docx; quy ước mẻ 400 = 7.200 gói/phần ăn = 1.800 hộp = 300 thùng. Bản này dùng làm danh mục nền để owner cấu hình công thức pilot theo gạo làm trung tâm, tỷ lệ nguyên liệu, sản lượng chuẩn, ngưỡng tồn, ngưỡng loại khỏi phiếu thu mua và hiệu chỉnh theo vận hành thực tế.

## 1. Nguyên tắc phân nhóm tổng thể

| Nhóm lớn | Nội dung | Nhóm con | +% / hao hụt | Ghi chú |
| --- | --- | --- | --- | --- |
| Nhóm A | Nguyên liệu sản xuất | A1-A3 | Cộng/trừ 5% hao hụt kế hoạch | Dùng cho BOM/MRP/Material Issue |
| Nhóm B | Bao bì, vật tư đóng gói, vật tư phụ | B1-B3 | B1/B2 hao hụt 7%; B3 không tính hao hụt | Dùng cho Packaging/Print/MRP/Stock Alert |

Nguyên tắc khóa: Các tỷ lệ +5% và +7% là cấu hình kế hoạch ban đầu để lập MRP, không hardcode trong code. Khi vận hành thật, owner có quyền điều chỉnh theo SKU, vật tư, nhà cung cấp, mùa vụ hoặc tỷ lệ hao hụt thực tế, nhưng mọi thay đổi phải có audit/evidence/owner approval.

### 1.1. Ngưỡng tồn kho và ngưỡng loại khỏi phiếu thu mua

Hệ thống phải kiểm soát hai chiều: thiếu thì cảnh báo để lập nhu cầu nhập/mua/thu hoạch; đủ hoặc vượt định mức thì tự động loại khỏi phiếu thu mua/mua sắm. Phiếu thu mua không được chứa nguyên liệu/vật tư chỉ vì có yêu cầu nếu tồn khả dụng + incoming đã duyệt đã vượt ngưỡng kiểm soát.

| Mã ngưỡng | Áp dụng | Ngưỡng khóa | Công thức nội suy | Quy tắc xử lý |
| --- | --- | --- | --- | --- |
| A_SPECIFIC_5_BATCH_SKU | Nhóm A đặc thù theo SKU | 5 mẻ/SKU | BOM_G1(material, SKU, mẻ 400) × 5 | Dưới ngưỡng mới được đưa vào danh mục xét nhập/mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua. |
| A_COMMON_20_BATCH_POOL | Nhóm A dùng chung nhiều SKU | 20 mẻ chuẩn aggregate | Σ BOM_G1(material, active SKU/planning basket, mẻ 400) trong 20 mẻ chuẩn | Không tính 5 mẻ riêng cho từng SKU; tồn trên ngưỡng 20 mẻ thì loại khỏi phiếu thu mua. |
| SAVIGIN_STRATEGIC_RESERVE | Sâm Savigin Company Source | Theo mùa vụ + tồn dự trữ chiến lược | Không là Purchase Requirement; harvest/intake/QC/lot/readiness theo chu kỳ thực tế | Đến kỳ thu hoạch vẫn cho harvest/sơ chế/nhập kho bảo quản; chỉ cảnh báo thiếu cho sản xuất khi ready stock dưới ngưỡng an toàn hoặc forecast thiếu. |
| B1_SKU_15_BATCH | Bao bì cấp 1 theo SKU | 15 mẻ/SKU | 7.704 gói quy đổi/mẻ × 15 = 115.560 gói quy đổi/SKU | Dưới ngưỡng mới được đưa vào danh mục xét mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua. |
| B2_BOX_SKU_15_BATCH | Hộp cấp 2 theo SKU | 15 mẻ/SKU | 1.926 hộp/mẻ × 15 = 28.890 hộp/SKU | Dưới ngưỡng mới được đưa vào danh mục xét mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua. |
| B2_CARTON_SKU_15_BATCH | Thùng carton theo SKU | 15 mẻ/SKU | 321 thùng/mẻ × 15 = 4.815 thùng/SKU | Dưới ngưỡng mới được đưa vào danh mục xét mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua. |
| B3_ACTUAL_STOCK_POLICY | Vật tư dùng chung B3 | Theo Stock Policy thực tế | Không tự áp +% hao hụt; nếu có consumption_per_batch thì owner cấu hình riêng | Không đưa vào phiếu mua nếu tồn đủ/vượt policy; trường hợp lớn cần Giám đốc phê duyệt. |

### 1.2. Quy tắc Giám đốc phê duyệt cho đơn hàng/kế hoạch sản xuất lớn

Trong trường hợp hệ thống nhận được kế hoạch sản xuất lớn, đơn hàng đại lý/nhà phân phối lớn hoặc chiến dịch đã được phê duyệt làm phát sinh nhu cầu vượt ngưỡng tồn thông thường, việc đưa nguyên liệu/vật tư đã vượt ngưỡng vào phiếu thu mua chỉ được thực hiện khi có Giám đốc phê duyệt.

Phải có Production Plan hoặc Dealer/Distributor Order lớn đã được xác nhận.

Phải có MRP run thể hiện nhu cầu thực tế theo SKU/BOM/Packaging Config.

Phải có lý do vượt ngưỡng, evidence, audit và người phê duyệt.

Không được dùng override để mua sắm tùy tiện hoặc bỏ qua tồn kho đang đủ/vượt định mức.

Không được gọi Purchase Order chính thức nếu chưa qua Procurement Requirement, owner approval và gate liên quan.

### 1.3. Khóa cơ chế công thức pilot lấy gạo làm trung tâm

Trong giai đoạn chưa khóa công thức chuẩn cuối cùng, hệ thống phải cho phép owner/pilot nhập công thức thử nghiệm theo nguyên tắc lấy gạo làm nguyên liệu neo trung tâm. Khi pilot nhập tên nguyên liệu và số lượng theo kg, g, lít, ml hoặc gram, hệ thống phải tự chuẩn hóa đơn vị, tự tính tỷ lệ so với gạo, lưu snapshot thử nghiệm và cho phép thêm hoặc bỏ bớt nguyên liệu trong phạm vi pilot.

Nguyên tắc khóa: gạo là anchor_ingredient. Tất cả nguyên liệu Nhóm A trong công thức pilot phải có ratio_to_rice hoặc usage_basis rõ ràng để khi thay đổi số kg gạo, hệ thống tự tính lại toàn bộ công thức.

| Thành phần kiểm soát | Quy tắc khóa | Cách hệ thống xử lý |
| --- | --- | --- |
| Anchor ingredient | Gạo lúa-tôm / gạo trong công thức là nguyên liệu neo | Owner nhập anchor_rice_quantity; hệ thống dùng làm cơ sở tính tỷ lệ |
| Nhập nguyên liệu pilot | Cho phép nhập tên nguyên liệu, số lượng, đơn vị kg/g/l/ml | Chuẩn hóa material canonical và UOM trước khi tính |
| Tự tính tỷ lệ | ratio_to_rice = standardized_quantity / standardized_rice_quantity × 100% | Hiển thị % theo gạo và lưu vào Pilot Formula Snapshot |
| Nguyên liệu dạng lít/ml | Không ép đổi sang kg nếu chưa có density config | Lưu theo lít/kg gạo hoặc ml/kg gạo; nếu cần quy đổi khối lượng phải qua density/UOM config |
| Thêm/bỏ nguyên liệu | Được phép trong giai đoạn pilot | Ghi audit; sau khi công thức đã chuẩn hóa thì thêm/bỏ là thay đổi công thức và phải tạo version mới |
| Pilot thành công | Chuyển thành công thức chuẩn được duyệt | Lưu Formula Version, ratio_to_rice, unit_policy, rounding_policy, evidence và owner approval |

### 1.4. Công thức chuẩn tự nhảy theo số kg gạo

Sau khi pilot được phê duyệt thành G1, bộ phận sản xuất không có quyền sửa công thức. Khi lập lệnh sản xuất, bộ phận sản xuất chỉ được nhập số kg gạo / batch anchor quantity trong phạm vi được phép; hệ thống tự scale toàn bộ nguyên liệu theo tỷ lệ đã khóa của G1 và tạo Production BOM Snapshot. Việc scale mẻ không phải sửa công thức. Mọi thay đổi về nguyên liệu, tỷ lệ, đơn vị hoặc cấu trúc công thức phải tạo Formula Version mới và được phê duyệt riêng.

| Tình huống | Cách tính | Kết quả quản trị |
| --- | --- | --- |
| Chỉ đổi số kg gạo | calculated_quantity = anchor_rice_quantity × ratio_to_rice | Không tạo formula version mới; tạo Calculation Snapshot/BOM Snapshot theo mẻ |
| Đổi tỷ lệ nguyên liệu | ratio_to_rice thay đổi | Bắt buộc tạo Formula Version mới, owner approval, evidence và audit |
| Thêm nguyên liệu mới | BOM line mới phát sinh | Bắt buộc qua Material Master, Formula Version, QC/MRP/Stock Policy nếu cần |
| Bỏ nguyên liệu cũ | BOM line bị loại khỏi công thức | Bắt buộc tạo Formula Version mới; không sửa đè version đã dùng |
| Đổi đơn vị tính | kg/g/l/ml phải quy đổi qua UOM policy | Không cho tính nếu thiếu UOM conversion hoặc density policy khi bắt buộc |
| Làm tròn sản xuất | Áp dụng rounding_policy theo material | Lưu số trước/sau làm tròn trong calculation snapshot |

Công thức sản xuất và buffer MRP phải tách riêng. Tỷ lệ +5% của Nhóm A là buffer kế hoạch/MRP để chuẩn bị và kiểm soát tồn kho, không được tự động sửa số lượng công thức sản xuất đã được phê duyệt.

### 1.5. Dynamic Threshold Resolver cho Nhóm A theo công thức đã scale

Ngưỡng tồn kho và ngưỡng loại khỏi phiếu thu mua của Nhóm A không được cố định bằng số chết nếu công thức còn thay đổi theo anchor gạo. Khi công thức chuẩn đã scale theo số kg gạo/mẻ, hệ thống phải dùng số lượng nguyên liệu đã tính cho một mẻ làm cơ sở tính ngưỡng tồn.

| Loại nguyên liệu Nhóm A | Cơ sở tính động | Ngưỡng loại khỏi phiếu thu mua |
| --- | --- | --- |
| Nguyên liệu đặc thù theo SKU | calculated_material_quantity_per_batch(SKU, anchor_rice_quantity) | calculated_material_quantity_per_batch × 5 mẻ/SKU |
| Nguyên liệu dùng chung nhiều SKU | Tổng calculated_material_quantity của active/planned SKU basket | aggregate nhu cầu 20 mẻ chuẩn hoặc planning basket được owner duyệt |
| Sâm Savigin | Company Source theo mùa vụ và strategic reserve stock | Không đưa vào phiếu thu mua; harvest theo mùa vụ, chỉ cảnh báo thiếu khi READY_FOR_PRODUCTION dưới ngưỡng an toàn hoặc forecast thiếu |
| Nguyên liệu chưa có tỷ lệ hoặc UOM | Không tính được threshold | BLOCKED_CONFIG_MISSING / OWNER_REVIEW_REQUIRED |

Công thức khóa: nếu available_stock + approved_incoming >= dynamic_suppression_threshold thì nguyên liệu bị loại khỏi phiếu thu mua, trừ trường hợp kế hoạch sản xuất lớn được Giám đốc phê duyệt, có MRP run, evidence và audit.

### 1.6. Dynamic Packaging Yield Resolver cho Nhóm B

Nhóm B không nhảy trực tiếp theo kg gạo, mà nhảy theo sản lượng chuẩn sau khi công thức/mẻ sản xuất đã được xác định. Khi chuẩn hóa được 1 mẻ sản xuất ra bao nhiêu gói, bao nhiêu hộp và bao nhiêu thùng, hệ thống phải tự tính nhu cầu B1/B2 và ngưỡng tồn tương ứng.

| Nhóm B | Cơ sở tính động | Công thức MRP | Ngưỡng loại khỏi phiếu thu mua |
| --- | --- | --- | --- |
| B1 film theo SKU | pack_output_per_batch | pack_output_per_batch × (1 + B1_buffer_rate) | B1_requirement_per_batch × 15 mẻ/SKU |
| B2 hộp theo SKU | box_output_per_batch | box_output_per_batch × (1 + B2_buffer_rate) | B2_box_requirement_per_batch × 15 mẻ/SKU |
| B2 thùng theo SKU | carton_output_per_batch | carton_output_per_batch × (1 + B2_buffer_rate) | B2_carton_requirement_per_batch × 15 mẻ/SKU |
| B3 vật tư dùng chung | Stock Policy hoặc consumption_per_batch nếu owner cấu hình | Theo tồn thực tế hoặc định mức đã duyệt | Theo max/suppression policy, không cộng 7% mặc định |

Nếu sản lượng thực tế của một mẻ thay đổi sau pilot, các ngưỡng B1/B2 phải tự tính lại theo output mới. Không được giữ cứng 7.200 gói, 1.800 hộp, 300 thùng nếu công thức chuẩn đã xác nhận sản lượng khác.

### 1.7. Material / Packaging Disposal & Write-off Control

Khi nguyên liệu, bao bì hoặc vật tư phải hủy do QC reject, hết hạn, hư hỏng, sai mẫu, lỗi in, obsolete hoặc recall/quality issue, hệ thống không được xóa tay hoặc sửa tồn trực tiếp. Phải đi theo luồng hủy có kiểm soát để tồn kho, MRP và kế toán không bị sai.

| Trạng thái / bước | Ý nghĩa | Quy tắc xử lý |
| --- | --- | --- |
| HOLD / QC_REJECT / EXPIRED / DAMAGED / OBSOLETE | Vật tư không được dùng | Không tính usable, không đưa vào phiếu cấp phát |
| DISPOSAL_REQUESTED | Tạo yêu cầu hủy | Cần lý do, material/lot, quantity, evidence |
| QA / OWNER REVIEW | Xem xét hủy | QA/owner duyệt hoặc trả về xử lý khác |
| DISPOSAL_APPROVED | Được duyệt hủy | Chưa giảm tồn nếu chưa thực hiện hủy |
| DISPOSAL_EXECUTED | Đã thực hiện hủy | Ghi evidence hủy, người thực hiện, thời gian |
| INVENTORY_WRITTEN_OFF | Ghi giảm tồn kho | Bắt buộc qua Inventory Ledger write-off, không delete |
| ACCOUNTING_SYNC_AWAITING_REVIEW | Chờ hạch toán nếu có giá trị kế toán | Đi qua MISA/accounting boundary |
| DISPOSAL_CLOSED | Đóng hồ sơ hủy | MRP/Stock Alert đọc tồn mới sau write-off |

Hàng chờ hủy hoặc đã duyệt hủy không được tính vào available_stock, ready_stock hoặc suppression threshold để tránh hệ thống hiểu nhầm là còn vật tư usable.

## 2. NHÓM A - NGUYÊN LIỆU SẢN XUẤT

| Mã nhóm | Tên nhóm | Phạm vi | +% hao hụt |
| --- | --- | --- | --- |
| A1 | Nguyên liệu Quân-Thần-Tá-Sứ của 20 SKU | Nguyên liệu trung tâm, nguyên liệu đặc thù từng SKU, dược liệu/nguyên liệu hỗ trợ | 5% |
| A2 | Nguyên liệu nền dinh dưỡng + rau củ chiết dịch tạo nước hầm | Gạo, rau củ, nước dừa, xương/lòng trắng trứng nếu áp dụng theo BOM | 5% |
| A3 | Nguyên liệu nêm & tạo hương vị | Muối, tiêu, tỏi, hành, rễ cần tây, mì chính, hương sâm | 5% |

Khóa tồn kho Nhóm A: nguyên liệu đặc thù theo SKU áp dụng ngưỡng 5 mẻ/SKU. Nguyên liệu dùng chung cho nhiều SKU áp dụng ngưỡng tổng 20 mẻ chuẩn aggregate, không tính 5 mẻ riêng lẻ cho từng SKU. Riêng Sâm Savigin là Company Source theo mùa vụ, không đưa vào phiếu thu mua như nguyên liệu mua ngoài.

### 2.1. Nhóm A1 - Nguyên liệu trong phần Quân-Thần-Tá-Sứ của 20 SKU

| Mã vật tư đề xuất | Tên nguyên liệu chuẩn hóa | Nguồn quản trị | Vai trò / ghi chú | +% | Quy tắc tồn kho/QC | Ngưỡng tồn / loại khỏi phiếu thu mua |
| --- | --- | --- | --- | --- | --- | --- |
| A1-001 | Sâm Savigin | Company Source / vùng trồng công ty | Nguyên liệu trung tâm trong công thức Quân-Thần-Tá-Sứ | 5% | Theo lot, QC, READY_FOR_PRODUCTION | SAVIGIN_STRATEGIC_RESERVE |
| A1-002 | Diêm mạch | Supplier Source | Nguyên liệu đặc thù SKU A1 | 5% | Theo lot, QC nếu áp dụng | A_SPECIFIC_5_BATCH_SKU |
| A1-003 | Hạt sen | Supplier Source | Nguyên liệu đặc thù SKU A1 | 5% | Theo lot, QC nếu áp dụng | A_SPECIFIC_5_BATCH_SKU |
| A1-004 | Cá Basa | Supplier Source | Nguyên liệu mặn đặc thù SKU A2 | 5% | Theo lot lạnh/tươi, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-005 | Cá hồi | Supplier Source | Nguyên liệu mặn đặc thù SKU A3/B2 | 5% | Theo lot lạnh/tươi, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-006 | Lươn đồng | Supplier Source | Nguyên liệu mặn đặc thù SKU A4 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-007 | Thịt cừu | Supplier Source | Nguyên liệu mặn đặc thù SKU A5 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-008 | Rau má | Supplier Source | Nguyên liệu đặc thù SKU B1 | 5% | Theo lot, QC nếu áp dụng | A_SPECIFIC_5_BATCH_SKU |
| A1-009 | Cá cơm | Supplier Source | Nguyên liệu mặn đặc thù SKU B3 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-010 | Thịt heo nạc | Supplier Source | Nguyên liệu mặn đặc thù SKU B4 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-011 | Da heo | Supplier Source | Nguyên liệu mặn đặc thù SKU B4 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-012 | Hàu biển | Supplier Source | Nguyên liệu mặn đặc thù SKU B5 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-013 | Gà ác | Supplier Source | Nguyên liệu mặn đặc thù SKU B6 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-014 | Bào ngư | Supplier Source | Nguyên liệu mặn đặc thù SKU C1 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-015 | Đông trùng hạ thảo | Supplier Source | Nguyên liệu đặc thù SKU C2 | 5% | Theo lot, QC nếu áp dụng | A_SPECIFIC_5_BATCH_SKU |
| A1-016 | Nấm đông cô | Supplier Source | Nguyên liệu đặc thù SKU C3 | 5% | Theo lot, QC nếu áp dụng | A_SPECIFIC_5_BATCH_SKU |
| A1-017 | Cua biển | Supplier Source | Nguyên liệu mặn đặc thù SKU C4 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-018 | Cá ngừ | Supplier Source | Nguyên liệu mặn đặc thù SKU C5 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-019 | Tôm | Supplier Source | Nguyên liệu mặn đặc thù SKU C6 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-020 | Thịt gà | Supplier Source | Nguyên liệu mặn đặc thù SKU C7 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-021 | Thịt heo | Supplier Source | Nguyên liệu mặn đặc thù SKU C8 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-022 | Thịt bò | Supplier Source | Nguyên liệu mặn đặc thù SKU C9 | 5% | Theo lot, QC bắt buộc | A_SPECIFIC_5_BATCH_SKU |
| A1-023 | Hoài sơn | Supplier Source | Dược liệu/nguyên liệu hỗ trợ trong nhiều SKU | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A1-024 | Bạch linh | Supplier Source | Dược liệu/nguyên liệu hỗ trợ trong một số SKU | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A1-025 | Kỷ tử | Supplier Source | Dược liệu/nguyên liệu hỗ trợ trong nhiều SKU | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A1-026 | Gừng / Gừng nướng | Supplier Source | Nguyên liệu tạo nền ấm/hương vị trong nhiều SKU | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A1-027 | Trần bì | Supplier Source | Nguyên liệu hỗ trợ trong một số SKU | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A1-028 | Quế chi | Supplier Source | Nguyên liệu hỗ trợ SKU A5 | 5% | Theo lot, QC nếu áp dụng | A_SPECIFIC_5_BATCH_SKU |
| A1-029 | Vừng / Vừng không vỏ | Supplier Source | Nguyên liệu đặc thù B2/B3 | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL / OWNER_CLASSIFY |
| A1-030 | Táo tàu | Supplier Source | Nguyên liệu xuất hiện ở công thức đặc thù và nước hầm | 5% | Một vật tư canonical, nhiều usage_role | A_COMMON_20_BATCH_POOL |
| A1-031 | Rong biển | Supplier Source | Nguyên liệu xuất hiện ở công thức đặc thù và nước hầm | 5% | Một vật tư canonical, nhiều usage_role | A_COMMON_20_BATCH_POOL |
| A1-032 | Đậu xanh không vỏ | Supplier Source | Nguyên liệu xuất hiện ở công thức đặc thù và nền dinh dưỡng | 5% | Một vật tư canonical, nhiều usage_role | A_COMMON_20_BATCH_POOL |

### 2.2. Nhóm A2 - Nguyên liệu nền dinh dưỡng + rau củ chiết dịch tạo nước hầm

| Mã vật tư đề xuất | Tên nguyên liệu chuẩn hóa | Nguồn quản trị | Vai trò / ghi chú | +% | Quy tắc tồn kho/QC | Ngưỡng tồn / loại khỏi phiếu thu mua |
| --- | --- | --- | --- | --- | --- | --- |
| A2-001 | Gạo (lúa - tôm) | Company/Supplier Source theo registry | Nguyên liệu nền dinh dưỡng | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A2-002 | Cà rốt / Carot | Supplier Source | Nền dinh dưỡng và nước hầm | 5% | Chuẩn hóa tên thành Cà rốt | A_COMMON_20_BATCH_POOL |
| A2-003 | Bí đỏ | Supplier Source | Nguyên liệu nền dinh dưỡng | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A2-004 | Đậu xanh không vỏ | Supplier Source | Nguyên liệu nền dinh dưỡng | 5% | Có thể trùng A1 theo usage_role | A_COMMON_20_BATCH_POOL |
| A2-005 | Nấm kim châm | Supplier Source | Nguyên liệu nền dinh dưỡng | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A2-006 | Táo tàu | Supplier Source | Rau củ/chiết dịch nước hầm | 5% | Có thể trùng A1 theo usage_role | A_COMMON_20_BATCH_POOL |
| A2-007 | Củ cải trắng | Supplier Source | Rau củ/chiết dịch nước hầm | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A2-008 | Rong biển | Supplier Source | Rau củ/chiết dịch nước hầm | 5% | Có thể trùng A1 theo usage_role | A_COMMON_20_BATCH_POOL |
| A2-009 | Hành tây | Supplier Source | Rau củ/chiết dịch nước hầm | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A2-010 | Cần tây | Supplier Source | Rau củ/chiết dịch nước hầm | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A2-011 | Lá thơm | Supplier Source | Rau củ/chiết dịch nước hầm | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A2-012 | Nước dừa nguyên chất | Supplier/Company Source theo registry | Nền nước hầm | 5% | Đơn vị lít, intake/QC theo rule | A_COMMON_20_BATCH_POOL |
| A2-013 | Lòng trắng trứng | Supplier Source | Nền nước hầm | 5% | Cần guard dietary/vegan khi áp dụng | A_COMMON_20_BATCH_POOL |
| A2-014 | Xương heo xay vỡ | Supplier Source | Nền nước hầm cho SKU mặn | 5% | Cần guard dietary/vegan khi áp dụng | A_COMMON_20_BATCH_POOL |

### 2.3. Nhóm A3 - Nguyên liệu nêm & tạo hương vị

| Mã vật tư đề xuất | Tên nguyên liệu chuẩn hóa | Nguồn quản trị | Vai trò / ghi chú | +% | Quy tắc tồn kho/QC | Ngưỡng tồn / loại khỏi phiếu thu mua |
| --- | --- | --- | --- | --- | --- | --- |
| A3-001 | Muối biển rang | Supplier Source | Nêm vị | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A3-002 | Tiêu đen rang | Supplier Source | Tạo hương vị | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A3-003 | Tỏi nướng | Supplier Source | Tạo hương vị | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A3-004 | Hành lá thái khúc | Supplier Source | Tạo hương vị | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A3-005 | Rễ cần tây thái nhuyễn | Supplier Source | Tạo hương vị | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A3-006 | Mì chính | Supplier Source | Tạo vị umami | 5% | Theo lot, QC nếu áp dụng | A_COMMON_20_BATCH_POOL |
| A3-007 | Hương sâm | Supplier/Company Source theo registry | Bù hương sâm | 5% | Đơn vị ml, phải có config/QC nếu dùng | A_COMMON_20_BATCH_POOL |

Lưu ý quản trị A1/A2/A3: Một nguyên liệu có thể xuất hiện ở nhiều vai trò, ví dụ Táo tàu, Rong biển, Đậu xanh không vỏ. Khi vào Material Master chỉ nên quản trị một material canonical, nhưng BOM line phải ghi rõ usage_role theo A1/A2/A3 để trace, QC, MRP và costing không bị nhầm. Ngưỡng loại khỏi thu mua phải tính theo material canonical, không tính trùng theo usage_role.

## 3. NHÓM B - BAO BÌ / VẬT TƯ ĐÓNG GÓI / VẬT TƯ PHỤ

| Mã nhóm | Tên nhóm | Phạm vi | +% hao hụt | Cơ sở tính |
| --- | --- | --- | --- | --- |
| B1 | Bao bì đóng gói cấp 1 | Cuộn màng đóng gói cho từng SKU | 7% | Cơ sở 7.200 gói/mẻ; đề xuất 7.704 gói quy đổi/mẻ |
| B2 | Bao bì đóng gói cấp 2 | 20 loại hộp 4 gói/hộp + 20 loại thùng carton 6 hộp/thùng | 7% | Cơ sở 1.800 hộp + 300 thùng/mẻ; đề xuất 1.926 hộp + 321 thùng/mẻ |
| B3 | Vật tư khác dùng chung | Tem dùng chung, băng keo, mực in dùng chung, vật tư phụ | 0% | Không tính hao hụt kế hoạch; tính tồn kho thực tế |

Khóa tồn kho Nhóm B: do bao bì, hộp, thùng, tem/nhãn và vật tư in có thể cần đặt hàng trước 30-45 ngày, B1/B2 được kiểm soát theo ngưỡng 15 mẻ/SKU. Dưới ngưỡng mới được đưa vào danh mục xét mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua, trừ trường hợp kế hoạch sản xuất lớn được Giám đốc phê duyệt.

### 3.1. Nhóm B1 - Bao bì đóng gói cấp 1: cuộn màng đóng gói cho từng SKU

| Mã vật tư đề xuất | SKU | Tên sản phẩm | Loại vật tư | +% | Cơ sở/mẻ | Đề xuất/mẻ | Ngưỡng loại khỏi phiếu thu mua 15 mẻ/SKU |
| --- | --- | --- | --- | --- | --- | --- | --- |
| B1-A1-CS-DM-HS-FILM | A1/CS/DM/HS | Cháo Sâm - Diêm mạch & Hạt sen | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-A2-CS-BASA-FILM | A2/CS/BASA | Cháo Sâm - Cá Basa | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-A3-CS-CAHOI-FILM | A3/CS/CAHOI | Cháo Sâm - Cá hồi | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-A4-CS-LUON-FILM | A4/CS/LUON | Cháo Sâm - Lươn đồng | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-A5-CS-CUU-FILM | A5/CS/CUU | Cháo Sâm - Thịt cừu & Táo tàu | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-B1-CS-RM-DX-FILM | B1/CS/RM/ĐX | Cháo Sâm - Rau má & Đậu xanh | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-B2-CS-DHA-FILM | B2/CS/DHA | Cháo Sâm - DHA Não bộ | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-B3-CS-CACOM-FILM | B3/CS/CACOM | Cháo Sâm - Cá cơm & Vừng | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-B4-CS-COLAGEN-FILM | B4/CS/COLAGEN | Cháo Sâm - Thịt heo & Da heo | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-B5-CS-SINHLUC-FILM | B5/CS/SINHLUC | Cháo Sâm - Hàu biển | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-B6-CS-GAAC-FILM | B6/CS/GAAC | Cháo Sâm - Gà ác | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-C1-CS-BAONGU-FILM | C1/CS/BAONGU | Cháo Sâm - Bào ngư | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-C2-CS-DONGTRUNG-FILM | C2/CS/DONGTRUNG | Cháo Sâm - Đông trùng hạ thảo | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-C3-CS-NAMDONGCO-FILM | C3/CS/NAMDONGCO | Cháo Sâm - Nấm đông cô | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-C4-CS-CUABIEN-FILM | C4/CS/CUABIEN | Cháo Sâm - Cua biển | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-C5-CS-CANGU-FILM | C5/CS/CANGU | Cháo Sâm - Cá ngừ | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-C6-CS-TOM-RONGBIEN-FILM | C6/CS/TOM/RONGBIEN | Cháo Sâm - Tôm & Rong biển | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-C7-CS-THITGA-FILM | C7/CS/THITGA | Cháo Sâm - Thịt gà | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-C8-CS-THITHEO-FILM | C8/CS/THITHEO | Cháo Sâm - Thịt heo | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |
| B1-C9-CS-THITBO-FILM | C9/CS/THITBO | Cháo Sâm - Thịt bò | Cuộn màng đóng gói cấp 1 theo SKU | 7% | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | >=115.560: loại; <115.560: xét mua |

Bảng nội suy nhanh B1 theo 15 mẻ/SKU

| SKU | Tên sản phẩm | Cơ sở/mẻ | Sau hao hụt 7%/mẻ | Ngưỡng mẻ | Ngưỡng loại khỏi phiếu thu mua |
| --- | --- | --- | --- | --- | --- |
| A1/CS/DM/HS | Cháo Sâm - Diêm mạch & Hạt sen | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| A2/CS/BASA | Cháo Sâm - Cá Basa | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| A3/CS/CAHOI | Cháo Sâm - Cá hồi | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| A4/CS/LUON | Cháo Sâm - Lươn đồng | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| A5/CS/CUU | Cháo Sâm - Thịt cừu & Táo tàu | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| B1/CS/RM/ĐX | Cháo Sâm - Rau má & Đậu xanh | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| B2/CS/DHA | Cháo Sâm - DHA Não bộ | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| B3/CS/CACOM | Cháo Sâm - Cá cơm & Vừng | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| B4/CS/COLAGEN | Cháo Sâm - Thịt heo & Da heo | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| B5/CS/SINHLUC | Cháo Sâm - Hàu biển | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| B6/CS/GAAC | Cháo Sâm - Gà ác | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| C1/CS/BAONGU | Cháo Sâm - Bào ngư | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| C2/CS/DONGTRUNG | Cháo Sâm - Đông trùng hạ thảo | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| C3/CS/NAMDONGCO | Cháo Sâm - Nấm đông cô | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| C4/CS/CUABIEN | Cháo Sâm - Cua biển | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| C5/CS/CANGU | Cháo Sâm - Cá ngừ | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| C6/CS/TOM/RONGBIEN | Cháo Sâm - Tôm & Rong biển | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| C7/CS/THITGA | Cháo Sâm - Thịt gà | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| C8/CS/THITHEO | Cháo Sâm - Thịt heo | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |
| C9/CS/THITBO | Cháo Sâm - Thịt bò | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | 15 mẻ/SKU | 115.560 gói quy đổi/SKU |

### 3.2. Nhóm B2 - Bao bì đóng gói cấp 2: hộp sản phẩm và thùng carton theo từng SKU

| SKU | Tên sản phẩm | Mã hộp | Đề xuất hộp/mẻ | Ngưỡng hộp 15 mẻ | Mã thùng | Đề xuất thùng/mẻ | Ngưỡng thùng 15 mẻ | Quy tắc loại khỏi phiếu thu mua |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| A1/CS/DM/HS | Cháo Sâm - Diêm mạch & Hạt sen | B2-A1-CS-DM-HS-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-A1-CS-DM-HS-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| A2/CS/BASA | Cháo Sâm - Cá Basa | B2-A2-CS-BASA-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-A2-CS-BASA-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| A3/CS/CAHOI | Cháo Sâm - Cá hồi | B2-A3-CS-CAHOI-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-A3-CS-CAHOI-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| A4/CS/LUON | Cháo Sâm - Lươn đồng | B2-A4-CS-LUON-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-A4-CS-LUON-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| A5/CS/CUU | Cháo Sâm - Thịt cừu & Táo tàu | B2-A5-CS-CUU-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-A5-CS-CUU-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| B1/CS/RM/ĐX | Cháo Sâm - Rau má & Đậu xanh | B2-B1-CS-RM-DX-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-B1-CS-RM-DX-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| B2/CS/DHA | Cháo Sâm - DHA Não bộ | B2-B2-CS-DHA-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-B2-CS-DHA-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| B3/CS/CACOM | Cháo Sâm - Cá cơm & Vừng | B2-B3-CS-CACOM-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-B3-CS-CACOM-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| B4/CS/COLAGEN | Cháo Sâm - Thịt heo & Da heo | B2-B4-CS-COLAGEN-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-B4-CS-COLAGEN-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| B5/CS/SINHLUC | Cháo Sâm - Hàu biển | B2-B5-CS-SINHLUC-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-B5-CS-SINHLUC-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| B6/CS/GAAC | Cháo Sâm - Gà ác | B2-B6-CS-GAAC-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-B6-CS-GAAC-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| C1/CS/BAONGU | Cháo Sâm - Bào ngư | B2-C1-CS-BAONGU-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-C1-CS-BAONGU-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| C2/CS/DONGTRUNG | Cháo Sâm - Đông trùng hạ thảo | B2-C2-CS-DONGTRUNG-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-C2-CS-DONGTRUNG-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| C3/CS/NAMDONGCO | Cháo Sâm - Nấm đông cô | B2-C3-CS-NAMDONGCO-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-C3-CS-NAMDONGCO-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| C4/CS/CUABIEN | Cháo Sâm - Cua biển | B2-C4-CS-CUABIEN-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-C4-CS-CUABIEN-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| C5/CS/CANGU | Cháo Sâm - Cá ngừ | B2-C5-CS-CANGU-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-C5-CS-CANGU-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| C6/CS/TOM/RONGBIEN | Cháo Sâm - Tôm & Rong biển | B2-C6-CS-TOM-RONGBIEN-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-C6-CS-TOM-RONGBIEN-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| C7/CS/THITGA | Cháo Sâm - Thịt gà | B2-C7-CS-THITGA-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-C7-CS-THITGA-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| C8/CS/THITHEO | Cháo Sâm - Thịt heo | B2-C8-CS-THITHEO-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-C8-CS-THITHEO-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |
| C9/CS/THITBO | Cháo Sâm - Thịt bò | B2-C9-CS-THITBO-BOX | 1.926 hộp | 28.890 hộp/SKU | B2-C9-CS-THITBO-CARTON | 321 thùng | 4.815 thùng/SKU | >= ngưỡng: loại; < ngưỡng: xét mua |

Bảng nội suy nhanh B2 theo 15 mẻ/SKU

| SKU | Tên sản phẩm | Cơ sở hộp | Sau 7% hộp | Ngưỡng hộp 15 mẻ | Cơ sở thùng | Sau 7% thùng | Ngưỡng thùng 15 mẻ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| A1/CS/DM/HS | Cháo Sâm - Diêm mạch & Hạt sen | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| A2/CS/BASA | Cháo Sâm - Cá Basa | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| A3/CS/CAHOI | Cháo Sâm - Cá hồi | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| A4/CS/LUON | Cháo Sâm - Lươn đồng | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| A5/CS/CUU | Cháo Sâm - Thịt cừu & Táo tàu | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| B1/CS/RM/ĐX | Cháo Sâm - Rau má & Đậu xanh | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| B2/CS/DHA | Cháo Sâm - DHA Não bộ | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| B3/CS/CACOM | Cháo Sâm - Cá cơm & Vừng | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| B4/CS/COLAGEN | Cháo Sâm - Thịt heo & Da heo | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| B5/CS/SINHLUC | Cháo Sâm - Hàu biển | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| B6/CS/GAAC | Cháo Sâm - Gà ác | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| C1/CS/BAONGU | Cháo Sâm - Bào ngư | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| C2/CS/DONGTRUNG | Cháo Sâm - Đông trùng hạ thảo | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| C3/CS/NAMDONGCO | Cháo Sâm - Nấm đông cô | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| C4/CS/CUABIEN | Cháo Sâm - Cua biển | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| C5/CS/CANGU | Cháo Sâm - Cá ngừ | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| C6/CS/TOM/RONGBIEN | Cháo Sâm - Tôm & Rong biển | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| C7/CS/THITGA | Cháo Sâm - Thịt gà | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| C8/CS/THITHEO | Cháo Sâm - Thịt heo | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |
| C9/CS/THITBO | Cháo Sâm - Thịt bò | 1.800 hộp/mẻ | 1.926 hộp/mẻ | 28.890 hộp/SKU | 300 thùng/mẻ | 321 thùng/mẻ | 4.815 thùng/SKU |

### 3.3. Nhóm B3 - Vật tư khác dùng chung, tính theo tồn kho thực tế

| Mã vật tư đề xuất | Tên vật tư | Nhóm vật tư | +% | Nguyên tắc tính | Ngưỡng loại khỏi phiếu thu mua |
| --- | --- | --- | --- | --- | --- |
| B3-001 | Tem dùng chung | Vật tư dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-002 | Tem kiểm soát nội bộ / QC / release nếu dùng | Vật tư dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-003 | Băng keo đóng thùng | Vật tư dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-004 | Mực in date/lot/HSD dùng chung | Vật tư in dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-005 | Ribbon/mực in mã vạch/QR nếu dùng | Vật tư in dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-006 | Decal/nhãn dùng chung nếu không tách theo SKU | Vật tư in dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-007 | Dung môi/vật tư vệ sinh máy in nếu dùng | Vật tư phụ dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-008 | Dao cắt/keo/vật tư phụ đóng gói khác | Vật tư phụ dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |

4. Quy tắc tính nhanh theo mẻ 400 và ngưỡng loại khỏi phiếu thu mua

| Chỉ tiêu | Cơ sở | Áp dụng | Đề xuất/MRP | Ngưỡng loại khỏi phiếu thu mua |
| --- | --- | --- | --- | --- |
| Sản lượng gói/phần ăn | 7.200 gói/mẻ | B1 cuộn màng cấp 1 | +7% = 7.704 gói quy đổi/mẻ | 15 mẻ/SKU = 115.560 gói quy đổi/SKU |
| Sản lượng hộp | 1.800 hộp/mẻ | B2 hộp sản phẩm | +7% = 1.926 hộp/mẻ | 15 mẻ/SKU = 28.890 hộp/SKU |
| Sản lượng thùng | 300 thùng/mẻ | B2 thùng carton | +7% = 321 thùng/mẻ | 15 mẻ/SKU = 4.815 thùng/SKU |
| Nguyên liệu A đặc thù SKU | Theo BOM G1/mẻ 400 | A1/A2/A3 đặc thù 1 SKU | +5% hao hụt trong MRP | Ngưỡng loại khỏi thu mua = BOM G1/mẻ × 5 mẻ/SKU |
| Nguyên liệu A dùng chung | Theo aggregate BOM G1 | A1/A2/A3 dùng chung nhiều SKU | +5% hao hụt trong MRP | Ngưỡng loại khỏi thu mua = aggregate nhu cầu 20 mẻ chuẩn |
| Sâm Savigin | Company Source theo mùa vụ | Nguồn công ty trồng | Không là phiếu thu mua | Harvest theo mùa vụ; cảnh báo thiếu khi dưới production safety threshold hoặc forecast thiếu |
| Vật tư B3 | Theo tồn kho thực tế | Tem, băng keo, mực in, ribbon, vật tư phụ | +0% mặc định | Theo Stock Policy; không tự mua nếu tồn đủ/vượt policy |

Công thức MRP đề xuất: Nhu cầu đề xuất = Nhu cầu theo BOM/Packaging Config × (1 + tỷ lệ +%) - tồn khả dụng - incoming đã được duyệt + lượng reserve/điều chỉnh nếu có.

Công thức loại khỏi phiếu thu mua: Nếu available_stock + approved_incoming >= suppression_threshold thì material bị loại khỏi phiếu thu mua, trừ khi có kế hoạch sản xuất lớn được Giám đốc phê duyệt.

## 5. Bảng owner kiểm soát tồn kho theo nhóm

| Nhóm | +% | Ngưỡng tồn/loại khỏi thu mua | Điều kiện được đưa vào phiếu thu mua | Điều kiện loại khỏi phiếu thu mua | Ghi chú owner |
| --- | --- | --- | --- | --- | --- |
| A1/A2/A3 đặc thù SKU | 5% | 5 mẻ/SKU theo BOM G1 | available + incoming < BOM×5 | available + incoming >= BOM×5 | Nếu vượt ngưỡng nhưng có kế hoạch lớn: Giám đốc phê duyệt |
| A1/A2/A3 dùng chung | 5% | 20 mẻ chuẩn aggregate | available + incoming < aggregate 20 mẻ | available + incoming >= aggregate 20 mẻ | Không tính 5 mẻ riêng từng SKU để tránh mua trùng |
| Sâm Savigin | 5% trong MRP nếu cấp sản xuất | Strategic reserve theo mùa vụ | Không tạo phiếu thu mua; harvest theo chu kỳ | Không áp dụng purchase suppression như supplier material | Chỉ cảnh báo thiếu dưới production safety threshold/forecast thiếu |
| B1 film theo SKU | 7% | 15 mẻ/SKU = 115.560 gói quy đổi | <115.560 gói quy đổi/SKU | >=115.560 gói quy đổi/SKU | Do lead time 30-45 ngày; vẫn cần MRP/owner review |
| B2 hộp theo SKU | 7% | 15 mẻ/SKU = 28.890 hộp | <28.890 hộp/SKU | >=28.890 hộp/SKU | Không mua nếu vượt ngưỡng, trừ kế hoạch lớn được duyệt |
| B2 thùng theo SKU | 7% | 15 mẻ/SKU = 4.815 thùng | <4.815 thùng/SKU | >=4.815 thùng/SKU | Không mua nếu vượt ngưỡng, trừ kế hoạch lớn được duyệt |
| B3 dùng chung | 0% mặc định | Theo Stock Policy thực tế | Theo min/safety/reorder policy | Theo max/suppression policy | Nếu có định mức theo batch thì owner cấu hình riêng |

## 6. Khóa nguyên tắc đưa vào PACK-03

Nhóm A là nguyên liệu sản xuất, chia A1/A2/A3 và áp dụng +5% hao hụt kế hoạch trong MRP.

Nhóm A đặc thù theo SKU áp dụng ngưỡng tồn 5 mẻ/SKU; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua.

Nhóm A dùng chung nhiều SKU áp dụng ngưỡng aggregate 20 mẻ chuẩn; không tính 5 mẻ riêng lẻ từng SKU để tránh mua trùng.

Sâm Savigin là Company Source theo mùa vụ, được harvest/sơ chế/intake/QC/lot/readiness/nhập kho dự trữ chiến lược; không đưa vào phiếu thu mua như nguyên liệu supplier.

Hệ thống chỉ cảnh báo nguy cơ thiếu Sâm cho sản xuất khi tồn Sâm READY_FOR_PRODUCTION dưới production safety threshold hoặc forecast sản xuất vượt tồn dự trữ.

Nhóm B là bao bì/vật tư, chia B1/B2/B3; B1/B2 áp dụng +7%, B3 tính tồn kho thực tế và không tính hao hụt kế hoạch mặc định.

B1/B2 theo SKU áp dụng ngưỡng 15 mẻ/SKU do lead time đặt hàng 30-45 ngày.

B1 film: ngưỡng loại khỏi phiếu thu mua = 115.560 gói quy đổi/SKU.

B2 hộp: ngưỡng loại khỏi phiếu thu mua = 28.890 hộp/SKU.

B2 thùng: ngưỡng loại khỏi phiếu thu mua = 4.815 thùng/SKU.

B3 là vật tư dùng chung như tem, băng keo, mực in, ribbon, decal và vật tư phụ; xử lý theo Stock Policy thực tế.

Tỷ lệ +%, mẻ chuẩn, ngưỡng 5 mẻ, ngưỡng 15 mẻ và ngưỡng 20 mẻ là config/policy, không hardcode trong code hoặc template.

Material Master phải quản trị theo material canonical; cùng một nguyên liệu xuất hiện ở nhiều vai trò thì không tạo trùng vật tư, mà tách usage_role trên BOM line.

Phiếu thu mua phải tự loại vật tư đủ/vượt ngưỡng, không cho mua sắm chỉ vì có yêu cầu.

Kế hoạch sản xuất/đơn hàng lớn muốn vượt ngưỡng phải có Giám đốc phê duyệt, evidence, audit và MRP run hợp lệ.

Không phát hành Production Order nếu thiếu nguyên liệu/vật tư cần thiết hoặc tồn khả dụng không đạt guard.

Công thức pilot phải lấy gạo làm anchor_ingredient; khi nhập nguyên liệu kg/g/l/ml hệ thống tự chuẩn hóa đơn vị và tính ratio_to_rice.

Sau khi pilot thành công và công thức được owner duyệt, người dùng chỉ cần nhập anchor_rice_quantity thì hệ thống tự tính toàn bộ nguyên liệu theo tỷ lệ đã khóa.

Scale số kg gạo không phải đổi công thức; đổi tỷ lệ, thêm nguyên liệu hoặc bỏ nguyên liệu sau khi công thức đã khóa thì bắt buộc tạo Formula Version mới.

Ngưỡng tồn Nhóm A phải nhảy theo calculated_material_quantity_per_batch sau khi công thức được scale; không dùng số cứng nếu công thức/mẻ thay đổi.

Ngưỡng B1/B2 phải nhảy theo sản lượng chuẩn thực tế của mẻ: gói, hộp, thùng; không tính trực tiếp theo kg gạo.

Buffer +5% Nhóm A và +7% B1/B2 là buffer MRP/stock planning, không tự sửa công thức sản xuất đã phê duyệt.

Nguyên liệu/vật tư hủy phải đi qua Disposal Request, QA/Owner Review, Disposal Execution, Inventory Ledger Write-off, Evidence và Audit; không được xóa tay tồn kho.

## 7. Smoke test bắt buộc cho ngưỡng tồn kho và phiếu thu mua

| Smoke ID | Tình huống | Kết quả kỳ vọng |
| --- | --- | --- |
| MAT-SUP-001 | Nguyên liệu A đặc thù SKU tồn >= 5 mẻ/SKU | Loại khỏi phiếu thu mua |
| MAT-SUP-002 | Nguyên liệu A đặc thù SKU tồn < 5 mẻ/SKU và có MRP demand | Được đưa vào danh mục xét nhập/mua |
| MAT-SUP-003 | Nguyên liệu A dùng chung tồn >= aggregate 20 mẻ chuẩn | Loại khỏi phiếu thu mua |
| MAT-SUP-004 | Nguyên liệu A dùng chung tồn < aggregate 20 mẻ chuẩn | Được đưa vào Material Requirement Board nếu có demand |
| MAT-SUP-005 | Sâm Savigin đến kỳ thu hoạch dù tồn sản xuất còn đủ | Tạo Company Source Harvest Alert/Harvest Requirement, không tạo Purchase Requirement |
| MAT-SUP-006 | Sâm Savigin READY_FOR_PRODUCTION dưới ngưỡng an toàn sản xuất | Cảnh báo SAVIGIN_PRODUCTION_STOCK_LOW |
| MAT-SUP-007 | B1 film tồn >= 115.560 gói quy đổi/SKU | Loại khỏi phiếu thu mua |
| MAT-SUP-008 | B2 hộp tồn >= 28.890 hộp/SKU | Loại khỏi phiếu thu mua |
| MAT-SUP-009 | B2 thùng tồn >= 4.815 thùng/SKU | Loại khỏi phiếu thu mua |
| MAT-SUP-010 | B3 tồn đủ/vượt Stock Policy | Loại khỏi phiếu mua; không tự mua thêm |
| MAT-SUP-011 | Đơn hàng/kế hoạch sản xuất lớn vượt ngưỡng, chưa có Giám đốc phê duyệt | BLOCKED / OWNER_APPROVAL_REQUIRED |
| MAT-SUP-012 | Đơn hàng/kế hoạch sản xuất lớn vượt ngưỡng, có Giám đốc phê duyệt + evidence + MRP | Cho phép override có kiểm soát, không tự tạo PO nếu procurement gate chưa đạt |
| MAT-FRM-001 | Pilot nhập gạo làm anchor và nhập nguyên liệu theo kg/g/l/ml | Hệ thống chuẩn hóa UOM và tự tính ratio_to_rice cho từng BOM line |
| MAT-FRM-002 | Pilot thêm hoặc bỏ nguyên liệu trước khi khóa công thức | Cho phép thay đổi trong pilot, có audit và Formula Candidate Snapshot |
| MAT-FRM-003 | Công thức đã khóa, user nhập số kg gạo mới | Toàn bộ nguyên liệu Nhóm A tự nhảy theo ratio_to_rice và tạo Calculation Snapshot |
| MAT-FRM-004 | Chỉ scale số kg gạo, không đổi tỷ lệ | Không tạo Formula Version mới; chỉ tạo BOM/Production Snapshot theo mẻ |
| MAT-FRM-005 | Sau khi khóa công thức, user đổi tỷ lệ hoặc thêm/bỏ nguyên liệu | BLOCKED hoặc yêu cầu tạo Formula Version mới + owner approval + evidence |
| MAT-FRM-006 | Công thức/mẻ thay đổi làm calculated_material_quantity_per_batch thay đổi | Ngưỡng Nhóm A tự tính lại theo 5 mẻ/SKU hoặc aggregate 20 mẻ |
| MAT-FRM-007 | Sản lượng mẻ thay đổi số gói/hộp/thùng | Ngưỡng B1/B2 tự tính lại theo output mới và 15 mẻ/SKU |
| MAT-FRM-008 | Liquid material thiếu UOM/density policy khi cần quy đổi | BLOCKED_CONFIG_MISSING / OWNER_REVIEW_REQUIRED |
| MAT-DSP-001 | Nguyên liệu QC_REJECT hoặc EXPIRED cần hủy | Tạo Disposal Request; không tính usable; không xóa tay tồn kho |
| MAT-DSP-002 | Disposal đã được duyệt và thực hiện | Inventory Ledger write-off, accounting sync nếu cần, evidence/audit đầy đủ |

## NHÓM A - CHÁO SÂM THEO MÙA

## A1/CS/DM/HS - CHÁO SÂM - DIÊM MẠCH & HẠT SEN

Thông tin đầu công thức

SKU: A1/CS/DM/HS Tên sản phẩm: Cháo Sâm - Diêm mạch & Hạt sen Nhóm sản phẩm: Cháo sâm theo mùa Phân loại: Không thuần chay (vegan claim khóa do công thức/BOM có thành phần động vật) Mã công thức: FML-A1-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Diêm mạch | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 3 | Hạt sen | 9.00 | kg | 4.62 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Bạch linh | 5.00 | kg | 2.56 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 6 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 7 | Gừng | 0.70 | kg | 0.36 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9 | Nước dừa nguyên chất<br>Lòng trắng trứng | 73.30<br>55.00 | Lít<br>kg | 39.13<br>28.21 | Làm nền nước hầm<br>Làm nền nước hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5<br>6 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## A2/CS/BASA - CHÁO SÂM - CÁ BASA

Thông tin đầu công thức

SKU: A2/CS/BASA Tên sản phẩm: Cháo Sâm - Cá Basa Nhóm sản phẩm: Cháo sâm theo mùa Phân loại: Mặn Mã công thức: FML-A2-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Cá Basa | 22.00 | kg | 11.28 | Rửa sạch Ép lấy thịt ướp gia vị xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Bạch linh | 9.00 | kg | 4.62 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 6 | Gừng | 0.40 | kg | 0.21 | Rửa sạch nướng xay nhuyễn |
| 7 | Trần bì | 0.40 | kg | 0.21 | Rửa sạch |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Chiết dịch nước hầm |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Xương heo xay vỡ<br>Lòng trắng trứng | 55.00<br>55.00<br>21.30 | Lít<br>kg<br>kg | 28.21<br>28.21<br>10.92 | Làm nền nước hầm<br>Làm nền nước hầm<br>Làm nền nước hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## A3/CS/CAHOI - CHÁO SÂM - CÁ HỒI

Thông tin đầu công thức

SKU: A3/CS/CAHOI Tên sản phẩm: Cháo Sâm - Cá hồi Nhóm sản phẩm: Cháo sâm theo mùa Phân loại: Mặn Mã công thức: FML-A3-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Cá hồi | 15.50 | kg | 7.95 | Rửa sạch Ép lấy thịt ướp gia vị xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## A4/CS/LUON - CHÁO SÂM - LƯƠN ĐỒNG

Thông tin đầu công thức

SKU: A4/CS/LUON Tên sản phẩm: Cháo Sâm - Lươn đồng Nhóm sản phẩm: Cháo sâm theo mùa Phân loại: Mặn Mã công thức: FML-A4-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Lươn đồng | 15.50 | kg | 7.95 | Rửa sạch Ép lấy thịt ướp gia vị xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## A5/CS/CUU - CHÁO SÂM - THỊT CỪU & TÁO TÀU

Thông tin đầu công thức

SKU: A5/CS/CUU Tên sản phẩm: Cháo Sâm - Thịt cừu & Táo tàu Nhóm sản phẩm: Cháo sâm theo mùa Phân loại: Mặn Mã công thức: FML-A5-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Thịt cừu | 15.50 | kg | 7.95 | Rửa sạch xay nhuyễn ướp gia vị xào thơm |
| 3 | Táo tàu | 6.00 | kg | 3.08 | Rửa sạch |
| 4 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 6 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |
| 7 | Quế chi | 0.25 | kg | 0.13 | Rửa sạch đập vỡ |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## NHÓM B - CHÁO SÂM CHỨC NĂNG

## B1/CS/RM/ĐX - CHÁO SÂM - RAU MÁ & ĐẬU XANH

Thông tin đầu công thức

SKU: B1/CS/RM/ĐX Tên sản phẩm: Cháo Sâm - Rau má & Đậu xanh Nhóm sản phẩm: Cháo sâm chức năng Phân loại: Không thuần chay (vegan claim khóa do công thức/BOM có thành phần động vật) Mã công thức: FML-B1-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Rau má | 15.50 | kg | 7.95 | Rửa sạch xay nhuyển lọc lấy nước |
| 3 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 6 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9 | Nước dừa nguyên chất<br>Lòng trắng trứng | 73.30<br>55.00 | Lít<br>kg | 39.13<br>28.21 | Làm nền nước hầm<br>Làm nền nước hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## B2/CS/DHA - CHÁO SÂM - DHA NÃO BỘ

Thông tin đầu công thức

SKU: B2/CS/DHA Tên sản phẩm: Cháo Sâm - DHA Não bộ Nhóm sản phẩm: Cháo sâm chức năng Phân loại: Mặn Mã công thức: FML-B2-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Cá hồi | 15.50 | kg | 7.95 | Xay vừa ướp xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Vừng không võ | 6.00 | kg | 3.08 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 6 | Gừng | 0.40 | kg | 0.21 | Rửa sạch nướng xay nhuyễn |
| 7 | Trần bì | 0.40 | kg | 0.21 | Rửa sạch |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm ) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## B3/CS/CACOM - CHÁO SÂM - CÁ CƠM & VỪNG

Thông tin đầu công thức

SKU: B3/CS/CACOM Tên sản phẩm: Cháo Sâm - Cá cơm & Vừng Nhóm sản phẩm: Cháo sâm chức năng Phân loại: Mặn Mã công thức: FML-B3-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Cá cơm | 15.50 | kg | 7.95 | Rửa sạch xay nhuyễnt ướp gia vị xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Vừng | 6.00 | kg | 3.08 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 6 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | Rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## B4/CS/COLAGEN - CHÁO SÂM - THỊT HEO & DA HEO

Thông tin đầu công thức

SKU: B4/CS/COLAGEN Tên sản phẩm: Cháo Sâm - Thịt heo & Da heo Nhóm sản phẩm: Cháo sâm chức năng Phân loại: Mặn Mã công thức: FML-B4-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Thịt heo nạc | 10.50 | kg | 5.38 | Rửa sạch xay nhuyễn ướp gia vị xào thơm |
| 3 | Da heo | 5.00 | kg | 2.56 | Rửa sạch say nhuyện cùng da heo |
| 4 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 6 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## B5/CS/SINHLUC - CHÁO SÂM - HÀU BIỂN

Thông tin đầu công thức

SKU: B5/CS/SINHLUC Tên sản phẩm: Cháo Sâm - Hàu biển Nhóm sản phẩm: Cháo sâm chức năng Phân loại: Mặn Mã công thức: FML-B5-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Hàu biển | 15.50 | kg | 7.95 | Rửa sạch để ráo |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Trần bì | 0.80 | kg | 0.41 | Rửa sạch |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | Rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | Nền dinh dưỡng |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | Nền dinh dưỡng |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Nền dinh dưỡng |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Nền dinh dưỡng |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## B6/CS/GAAC - CHÁO SÂM - GÀ ÁC

Thông tin đầu công thức

SKU: B6/CS/GAAC Tên sản phẩm: Cháo Sâm - Gà ác Nhóm sản phẩm: Cháo sâm chức năng Phân loại: Mặn Mã công thức: FML-B6-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Gà ác | 15.50 | kg | 7.95 | Xay nhuyển ướp gia vị xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## NHÓM C - CHÁO SÂM BỔ DƯỠNG

## C1/CS/BAONGU - CHÁO SÂM - BÀO NGƯ

Thông tin đầu công thức

SKU: C1/CS/BAONGU Tên sản phẩm: Cháo Sâm - Bào ngư Nhóm sản phẩm: Cháo sâm bổ dưỡng Phân loại: Mặn Mã công thức: FML-C1-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu Ghi chú version: sử dụng làm nền công thức gốc G0, mở rộng trường phân loại theo rule toàn hệ, viết lại theo phân nhóm vận hành để xuất phiếu rõ thao tác, và mở rộng mì chính 1.9 kg.

Cơ sở tính tỷ lệ: 195 kg gạo nền = 100%

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % / 195 kg gạo nền | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin - Pouzolzia zeylanica (L.) Benn. | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Bào ngư - Haliotis spp. | 15.50 | kg | 7.95 | Rửa sạch thái lát ướp gia vị sào thơm |
| 3 | Hoài sơn - Dioscorea opposita | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử - Lycium barbarum | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng nướng - Zingiber officinale | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % / 195 kg gạo nền | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % / 195 kg gạo nền | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % / 195 kg gạo nền | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## C2/CS/DONGTRUNG - CHÁO SÂM - ĐÔNG TRÙNG HẠ THẢO

Thông tin đầu công thức

SKU: C2/CS/DONGTRUNG Tên sản phẩm: Cháo Sâm - Đông trùng hạ thảo Nhóm sản phẩm: Cháo sâm bổ dưỡng Phân loại: Không thuần chay (vegan claim khóa do công thức/BOM có thành phần động vật) Mã công thức: FML-C2-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Đông trùng hạ thảo | 15.50 | kg | 7.95 | Xay nhuyễn |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng nướng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9 | Nước dừa nguyên chất<br>Lòng trắng trứng | 73.30<br>55.00 | Lít<br>kg | 39.13<br>28.21 | Làm nền nước hầm<br>Làm nền nước hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## C3/CS/NAMDONGCO - CHÁO SÂM - NẤM ĐÔNG CÔ

Thông tin đầu công thức

SKU: C3/CS/NAMDONGCO Tên sản phẩm: Cháo Sâm - Nấm đông cô Nhóm sản phẩm: Cháo sâm bổ dưỡng Phân loại: Không thuần chay (vegan claim khóa do công thức/BOM có thành phần động vật) Mã công thức: FML-C3-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Nấm đông cô | 15.50 | kg | 7.95 | Rửa sạch thái lát ướp gia vị sào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9 | Nước dừa nguyên chất<br>Lòng trắng trứng | 73.30<br>55.00 | Lít<br>kg | 39.13<br>28.21 | Làm nền nước hầm<br>Làm nền nước hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## C4/CS/CUABIEN - CHÁO SÂM - CUA BIỂN

Thông tin đầu công thức

SKU: C4/CS/CUABIEN Tên sản phẩm: Cháo Sâm - Cua biển Nhóm sản phẩm: Cháo sâm bổ dưỡng Phân loại: Mặn Mã công thức: FML-C4-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Cua biển | 15.50 | kg | 7.95 | Rửa sạch ướp gia vị xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## C5/CS/CANGU - CHÁO SÂM - CÁ NGỪ

Thông tin đầu công thức

SKU: C5/CS/CANGU Tên sản phẩm: Cháo Sâm - Cá ngừ Nhóm sản phẩm: Cháo sâm bổ dưỡng Phân loại: Mặn Mã công thức: FML-C5-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Cá ngừ | 15.50 | kg | 7.95 | Ép lấy thịch ướp gia vị xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Bạch linh | 5.00 | kg | 2.56 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 6 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch |
| 7 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## C6/CS/TOM/RONGBIEN - CHÁO SÂM - TÔM & RONG BIỂN

Thông tin đầu công thức

SKU: C6/CS/TOM/RONGBIEN Tên sản phẩm: Cháo Sâm - Tôm & Rong biển Nhóm sản phẩm: Cháo sâm bổ dưỡng Phân loại: Mặn Mã công thức: FML-C6-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Tôm | 15.50 | kg | 7.95 | Xay nhuyện ướp gia vị xào thơm |
| 3 | Rong biển | 15.50 | kg | 7.95 | Rừa sạch |
| 4 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 6 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## C7/CS/THITGA - CHÁO SÂM - THỊT GÀ

Thông tin đầu công thức

SKU: C7/CS/THITGA Tên sản phẩm: Cháo Sâm - Thịt gà Nhóm sản phẩm: Cháo sâm bổ dưỡng Phân loại: Mặn Mã công thức: FML-C7-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Thịt gà | 15.50 | kg | 7.95 | Hấp đánh tơi |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## C8/CS/THITHEO - CHÁO SÂM - THỊT HEO

Thông tin đầu công thức

SKU: C8/CS/THITHEO Tên sản phẩm: Cháo Sâm - Thịt heo Nhóm sản phẩm: Cháo sâm bổ dưỡng Phân loại: Mặn Mã công thức: FML-C8-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Thịt heo | 15.50 | kg | 7.95 | Xoay nhuyện ướp gia vị xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

## C9/CS/THITBO - CHÁO SÂM - THỊT BÒ

Thông tin đầu công thức

SKU: C9/CS/THITBO Tên sản phẩm: Cháo Sâm - Thịt bò Nhóm sản phẩm: Cháo sâm bổ dưỡng Phân loại: Mặn Mã công thức: FML-C9-G1 Version: G1 Trạng thái: Công thức cập nhật vận hành / sản xuất mẫu

Phần 1 - Công thức: Quần - Thần - Tá - Sứ

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Sâm Savigin | 9.00 | kg | 4.62 | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2 | Thịt bò | 15.50 | kg | 7.95 | Xay nhuyện ướp gia vị xào thơm |
| 3 | Hoài sơn | 15.50 | kg | 7.95 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 4 | Kỷ tử | 3.00 | kg | 1.54 | Rửa sạch |
| 5 | Gừng | 0.80 | kg | 0.41 | Rửa sạch nướng xay nhuyễn |

Phần 2 - Nguyên liệu nền dinh dưỡng

| STT | Nguyên liệu | Số lượng / mẻ 400 | Đơn vị | Tỷ lệ % | Ghi chú vận hành |
| --- | --- | --- | --- | --- | --- |
| 1 | Gạo (lúa - tôm) | 195.00 | kg | 100.00 | rửa sạch để ráo rang vàng |
| 2 | Cà rốt | 35.00 | kg | 17.95 | rửa sạch thái hạt lựu |
| 3 | Bí đỏ | 33.00 | kg | 16.92 | rửa sạch thái hạt lựu |
| 4 | Đậu xanh không vỏ | 29.50 | kg | 15.13 | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5 | Nấm kim châm | 18.50 | kg | 9.49 | Rửa sạch để ráo cắt khúc 20-30 mm |

Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng thái khúc | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây chẻ 4 | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br>9<br>10 | Nước dừa nguyên chất<br>Lòng trắng trứng<br>Xương heo xay vỡ | 55.00<br>21.30<br>55.00 | Lít<br>Kg<br>kg | 28.21<br>10.92<br>28.21 | Nền dịch hầm<br>Nền dịch hầm<br>Nền dịch hầm |

Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)

| STT | Nguyên liệu | Mẻ/400 | Đơn vị | %/195 kg gạo nền | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Muối biển rang | 12.84 | kg | 6.58 | Nêm vị |
| 2 | Tiêu đen rang | 1.32 | kg | 0.68 | Tạo hương vị |
| 3 | Tỏi nướng | 0.72 | kg | 0.37 | Tạo hương vị |
| 4 | Hành lá thái khúc | 2.19 | kg | 1.12 | Tạo hương vị |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | kg | 0.64 | Tạo hương vị |
| 6<br>7 | Mì chính<br>Hương sâm | 1.90<br>10 | Kg<br>ml | 0.97<br>0.000.5 | Tạo vị umami<br>Bù hương sâm |

BƯỚC 1

## 20 SKU ↔ NGUYÊN LIỆU / BOM MAP - GINSENGFOOD

## 0. MỞ ĐẦU TÀI LIỆU

### 0.1. Mục đích tài liệu

Tài liệu này dùng để khóa quan hệ giữa:

20 SKU canonical

danh mục nguyên liệu canonical

công thức đầu vào cho sản xuất mẫu

Tài liệu này chỉ dùng để khóa:

SKU nào dùng nguyên liệu nào

nguyên liệu nào là bắt buộc

nguyên liệu nào thuộc phần dược liệu

nguyên liệu nào thuộc phần nguyên liệu

version BOM / recipe nào đang áp dụng

cách map từ SKU sang danh sách nguyên liệu để cấp phát và sản xuất

### 0.2. Vai trò tài liệu

Đây là mắt xích nối giữa:

## BỘ 20 SKU CANONICAL - GINSENGFOOD

## BỘ NGUYÊN LIỆU CANONICAL - GINSENGFOOD

## CÔNG THỨC ĐẦU VÀO CHO SẢN XUẤT MẪU

### 0.3. Ranh giới tài liệu

Tài liệu này không giữ:

giá nguyên liệu

tồn kho

lô nguyên liệu

QC checklist

packaging rule

in mã

trace / recall rule chi tiết

Những phần đó đi ở bước sau.

## 1. NGUYÊN TẮC CHUNG

### 1.1. Một SKU phải map được ra nguyên liệu

Mỗi SKU phải map được ra:

danh sách dược liệu

danh sách nguyên liệu

version công thức đang áp dụng

mô tả sơ chế theo ghi chú

### 1.2. Không cho suy luận thủ công

Dev và vận hành không được tự suy luận nguyên liệu từ tên sản phẩm. Phải có map rõ ràng.

### 1.3. BOM / Recipe phải theo version

Mỗi SKU phải có:

mã công thức

version công thức

trạng thái công thức

### 1.4. Chọn SKU là ra BOM/Recipe

Khi chọn SKU trong lệnh sản xuất, hệ thống phải dùng chính map này để tự hiện công thức.

## 2. CẤU TRÚC MAP BẮT BUỘC

### 2.1. Thông tin đầu map

SKU

Tên sản phẩm

Mã công thức

Version

Trạng thái công thức

### 2.2. Phần 1 - Dược liệu

Mỗi dòng gồm:

STT

Mã nguyên liệu

Tên dược liệu

Hàm lượng

Đơn vị

Bắt buộc / không bắt buộc

Ghi chú

### 2.3. Phần 2 - Nguyên liệu

Mỗi dòng gồm:

STT

Mã nguyên liệu

Tên nguyên liệu

Hàm lượng

Đơn vị

Bắt buộc / không bắt buộc

Ghi chú

## 3. DANH SÁCH 20 SKU ↔ BOM MAP

### 3.1. Nhóm A - Theo mùa

## A1/CS/DM/HS

## A2/CS/BASA

## A3/CS/CAHOI

## A4/CS/LUON

## A5/CS/CUU

### 3.2. Nhóm B - Chức năng

## B1/CS/RM/ĐX

## B2/CS/DHA

## B3/CS/CACOM

## B4/CS/COLAGEN

## B5/CS/SINHLUC

## B6/CS/GAAC

### 3.3. Nhóm C - Bổ dưỡng

## C1/CS/BAONGU

## C2/CS/DONGTRUNG

## C3/CS/NAMDONGCO

## C4/CS/CUABIEN

## C5/CS/CANGU

## C6/CS/TOM/RONGBIEN

## C7/CS/THITGA

## C8/CS/THITHEO

## C9/CS/THITBO

## 4. QUY TẮC CỨNG

### 4.1. Không SKU nào được sản xuất nếu chưa có BOM/Recipe map

### 4.2. Không cho vận hành chọn lại nguyên liệu ở bước sản xuất

### 4.3. Mọi thay đổi nguyên liệu phải đi qua version công thức

### 4.4. Mã nguyên liệu phải lấy từ danh mục nguyên liệu canonical

### 4.5. Mọi cấp phát nguyên liệu cho sản xuất phải bám đúng map này

## 5. PASS / FAIL GATE

### 5.1. Đạt khi

đủ 20 SKU có BOM/Recipe map

mỗi SKU map được ra danh sách nguyên liệu

map phân tách được dược liệu / nguyên liệu

có version công thức

hệ thống dùng được map này để tự hiện công thức

### 5.2. Chưa đạt khi

thiếu SKU

thiếu nguyên liệu

không có version

vẫn phải suy luận tay từ tên SKU

BƯỚC 2

## 20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD

## 0. MỞ ĐẦU TÀI LIỆU

### 0.1. Mục đích tài liệu

Tài liệu này dùng để khóa cấu hình vận hành của từng SKU trong Operational Domain.

### 0.2. Vai trò tài liệu

Đây là file nối từ:

SKU

BOM / recipe

packaging

QC

release

trace

recall

### 0.3. Ranh giới tài liệu

Không giữ:

logic tài chính

doanh thu

KPI

MISA sync

## 1. CẤU TRÚC CONFIG MỖI SKU

Mỗi SKU phải có tối thiểu:

### 1.1. Thông tin công thức

mã công thức

version

trạng thái áp dụng

### 1.2. Quy cách đóng gói cấp 1

đơn vị cấp 1:

gói / lọ / hũ

nội dung in cấp 1

số lượng / quy cách cấp 1 nếu đã khóa

### 1.3. Quy cách đóng gói cấp 2

đơn vị cấp 2:

hộp / thùng / lọ / hũ

nội dung in cấp 2

mã vạch / QR rule

### 1.4. QC Config

QC checkpoint nào

bắt buộc % độ ẩm hay không

tiêu chí đạt / không đạt chính

### 1.5. Release Config

điều kiện cho nhập kho thành phẩm

điều kiện chặn release

### 1.6. Trace Config

public trace có / không

internal trace có / không

cấp truy vết theo hộp / thùng

### 1.7. Recall Config

SKU có thuộc diện recall áp dụng không

mức truy vết yêu cầu

## 2. QUY TẮC CHUNG

### 2.1. Operational không owner SKU master

Operational chỉ owner config vận hành theo SKU, không owner master SKU.

### 2.2. Không SKU nào được đưa vào sản xuất nếu thiếu operational config

### 2.3. Không SKU nào được in mã cấp 2 nếu thiếu packaging + trace config

### 2.4. Không SKU nào được nhập kho nếu thiếu QC/release config

## 3. PASS / FAIL GATE

### 3.1. Đạt khi

mỗi SKU có operational config

config đủ packaging / QC / release / trace / recall

không phải đoán bằng tay khi vận hành

### 3.2. Chưa đạt khi

SKU có BOM nhưng thiếu config vận hành

đóng gói / QC / trace vẫn phải đoán thủ công

thiếu rule in mã / trace / recall

BƯỚC 3

## QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC

## 0. MỞ ĐẦU

### 0.1. Mục đích

Khóa cách quản lý version công thức để sau này tối ưu mà không loạn.

### 0.2. Vai trò

Đây là file giữ cho:

công thức gốc nghiên cứu

công thức sản xuất mẫu

công thức áp dụng chính thức không bị ghi đè lẫn nhau.

## 1. NGUYÊN TẮC VERSION

### 1.1. Công thức gốc nghiên cứu

Là baseline đầu tiên, lấy từ đề tài nghiên cứu.

### 1.2. Công thức sản xuất mẫu

Là version dùng cho pilot / test mẻ thực tế.

### 1.3. Công thức đang áp dụng

Là version được duyệt để đưa vào sản xuất vận hành thường.

### 1.4. Công thức ngưng áp dụng

Version cũ vẫn phải lưu để truy xuất lịch sử.

## 2. KHI NÀO PHẢI TẠO VERSION MỚI

Phải tạo version mới khi có thay đổi về:

nguyên liệu

hàm lượng

đơn vị

bước kỹ thuật

ghi chú kỹ thuật ảnh hưởng vận hành

quy cách áp dụng theo mẻ

Không được sửa đè version cũ.

## 3. PHÊ DUYỆT THAY ĐỔI CÔNG THỨC

Mỗi thay đổi công thức phải có:

người đề xuất

lý do thay đổi

phạm vi thay đổi

version mới

ngày hiệu lực

người phê duyệt

mẻ đầu tiên áp dụng

## 4. TRẠNG THÁI VERSION

Tối thiểu:

DRAFT

PILOT

APPROVED

ACTIVE

## DEPRECATED

## 5. QUY TẮC CỨNG

### 5.1. Không cho sản xuất bằng version chưa được phép

### 5.2. Không cho sửa đè version cũ

### 5.3. Mỗi mẻ phải ghi nhận đang dùng version nào

### 5.4. Mọi phiếu sản xuất phải tự kéo version đang áp dụng

## 6. PASS / FAIL GATE

### 6.1. Đạt khi

mỗi thay đổi tạo version mới

có phê duyệt

truy được mẻ dùng version nào

### 6.2. Chưa đạt khi

sửa đè công thức cũ

không biết mẻ nào dùng version nào

thay đổi mà không có phê duyệt

BƯỚC 4

## TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP

## 0. MỞ ĐẦU

### 0.1. Mục đích

Khóa chuỗi dữ liệu truy vết thực tế từ nguyên liệu đến thành phẩm.

### 0.2. Vai trò

Đây là mắt xích nối:

lô nguyên liệu

mẻ sản xuất

batch thành phẩm

đơn vị đóng gói

QR / barcode

trace / recall

## 1. CHUỖI DỮ LIỆU BẮT BUỘC

### 1.1. Nguyên liệu đầu vào

mã nguyên liệu

lô nguyên liệu

### 1.2. Xuất cho sản xuất

lô nguyên liệu nào đi vào mẻ nào

### 1.3. Mẻ sản xuất

mẻ nào

SKU nào

version công thức nào

### 1.4. Batch thành phẩm

batch / lô sản xuất được sinh từ mẻ nào

### 1.5. Đóng gói cấp 1

gói / lọ / hũ thuộc mẻ / batch nào

### 1.6. Đóng gói cấp 2

hộp / thùng thuộc mẻ / batch nào

### 1.7. Mã vạch / QR

QR cấp hộp map tới record batch / trace nào

barcode map tới đơn vị đóng gói nào

## 2. RULE THEO CẤP ĐÓNG GÓI

### 2.1. Cấp 1

Chỉ mang:

MFG

EXP

### 2.2. Cấp 2

Mang:

batch / lot

MFG

EXP

barcode

QR

## 3. PUBLIC TRACE VS INTERNAL TRACE

### 3.1. Public trace

Hiển thị dữ liệu cho người dùng cuối, không lộ dữ liệu nội bộ nhạy cảm.

### 3.2. Internal trace

Dùng cho:

QC

nhà máy

kho

truy nguyên nguyên liệu

recall

## 4. RECALL DATA CHAIN

Recall phải lần ngược được:

hộp / thùng

QR / barcode

batch

mẻ sản xuất

lô nguyên liệu

nhà cung cấp / source nếu cần

## 5. PASS / FAIL GATE

### 5.1. Đạt khi

truy được từ batch về lô nguyên liệu

truy được từ hộp/thùng về batch

QR cấp 2 map đúng dữ liệu

recall lần ngược được chuỗi

### 5.2. Chưa đạt khi

batch không gắn với lô nguyên liệu

QR không gắn với batch thật

hộp / thùng không truy được về mẻ

BƯỚC 5

## BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH

## 0. MỞ ĐẦU

### 0.1. Mục đích

Khóa lớp đặc tả để dev code chính xác.

### 0.2. Vai trò

Dùng cho:

backend

frontend

QA

vận hành

## 1. MỖI PHIẾU PHẢI CÓ 4 LỚP ĐẶC TẢ

### 1.1. Màn hình

route

title

phần thông tin chung

bảng chi tiết

khối bằng chứng

khối xác nhận

### 1.2. Route / API

create

view

update

approve / reject

next step

### 1.3. Action

ai được bấm

khi nào được bấm

điều kiện chuyển trạng thái

### 1.4. Trạng thái

draft

submitted

approved

rejected

completed

canceled (tùy phiếu)

## 2. CÁC MÀN HÌNH / ROUTE / ACTION TỐI THIỂU CẦN ĐẶC TẢ

### 2.1. Phiếu nhập nguyên liệu đầu vào

### 2.2. Lệnh sản xuất

### 2.3. Phiếu đề nghị cấp nguyên liệu cho sản xuất

### 2.4. Phiếu chấp thuận nguyên liệu đưa vào sản xuất

### 2.5. Phiếu check-in / check-out nhân sự

### 2.6. Phiếu theo dõi sơ chế

### 2.7. Phiếu kiểm chất lượng sau sấy thăng hoa

### 2.8. Phiếu đóng gói cấp 1

### 2.9. Phiếu đóng gói cấp 2

### 2.10. Phiếu QC thành phẩm

### 2.11. Lệnh nhập kho thành phẩm

### 2.12. Phiếu kế toán xuất nguyên liệu cho sản xuất

## 3. QUY TẮC CỨNG CHO DEV

### 3.1. Phiếu nhập nguyên liệu = click chọn

### 3.2. Lệnh sản xuất / cấp phát = tự hiện công thức

### 3.3. Mọi phiếu sau = tự sử dụng làm nền dữ liệu từ hồ sơ gốc

### 3.4. Mọi phiếu phải có khối xác nhận cá nhân

### 3.5. Mọi phiếu phải đúng đơn vị theo công đoạn

### 3.6. Không cho hardcode đơn vị hoặc nguyên liệu sai rule

## 4. PASS / FAIL GATE

### 4.1. Đạt khi

mỗi phiếu có màn hình rõ

route rõ

action rõ

trạng thái rõ

điều kiện chuyển bước rõ

### 4.2. Chưa đạt khi

dev phải tự đoán form

route không rõ

action không rõ

trạng thái không rõ

## 20 SKU ↔ NGUYÊN LIỆU / BOM MAP - GINSENGFOOD

## 0. MỞ ĐẦU TÀI LIỆU

### 0.1. Mục đích tài liệu

Tài liệu này dùng để khóa BOM / Recipe Map giữa:

20 SKU canonical

danh mục nguyên liệu canonical

công thức đầu vào cho sản xuất mẫu

Tài liệu này chỉ dùng để khóa:

SKU nào dùng nguyên liệu nào

nguyên liệu nào thuộc Dược liệu

nguyên liệu nào thuộc Nguyên liệu

hàm lượng ban đầu của từng thành phần

đơn vị tính

mã công thức

version công thức

trạng thái công thức

### 0.2. Vai trò tài liệu

Tài liệu này là mắt xích nối giữa:

## BỘ 20 SKU CANONICAL - GINSENGFOOD

## BỘ NGUYÊN LIỆU CANONICAL - GINSENGFOOD

CÔNG THỨC ĐẦU VÀO CHO SẢN XUẤT MẪU cho 20 SKU cháo sâm - GINSENGFOOD

### 0.3. Ranh giới tài liệu

Tài liệu này không giữ:

giá nguyên liệu

tồn kho

lô nguyên liệu

QC checklist

release rule

packaging rule

trace / recall rule chi tiết

logic kế toán / MISA

Những phần đó đi ở bước sau.

### 0.4. Nguồn khóa dùng để viết tài liệu

Nguồn đối chiếu trực tiếp của tài liệu này là:

file danh mục 20 SKU đã chuẩn hóa vegan / mặn

file danh mục nguyên liệu đầu vào 2 nhóm

file đề tài dữ liệu đầu vào nguyên liệu làm baseline công thức G0

## 1. NGUYÊN TẮC CHUNG

### 1.1. Một SKU phải map được ra BOM / Recipe

Mỗi SKU phải map được ra:

mã công thức

version công thức

danh sách dược liệu

danh sách nguyên liệu

hàm lượng

đơn vị tính

### 1.2. Không cho suy luận thủ công

Dev, QA và vận hành không được suy luận nguyên liệu từ tên sản phẩm.

Hệ thống phải đọc đúng từ BOM / Recipe Map này.

### 1.3. Chọn SKU là ra công thức

Khi chọn SKU trong lệnh sản xuất, hệ thống phải tự động hiện:

mã công thức

version công thức

toàn bộ dược liệu

toàn bộ nguyên liệu

hàm lượng

đơn vị tính

### 1.4. Không cho chọn lại nguyên liệu ở bước sản xuất

Danh sách nguyên liệu dùng cho sản xuất là dữ liệu tự hiện theo:

SKU

mã công thức

version công thức

Người vận hành không được tự thêm, tự xóa, tự thay đổi danh sách nguyên liệu ở bước sản xuất thường.

### 1.5. Version công thức

BOM / Recipe Map phải đi theo version.

Bản trong tài liệu này là:

Version G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

### 1.6. Quy tắc thay đổi về sau

Nếu có thay đổi:

nguyên liệu

hàm lượng

đơn vị

ghi chú kỹ thuật

thì tạo version mới, không sửa đè version G0.

## 2. CẤU TRÚC CHUẨN CHO MỖI BOM / RECIPE MAP

### 2.1. Thông tin đầu

SKU

Tên sản phẩm

Mã công thức

Version

Trạng thái

### 2.2. Phần 1 - Dược liệu

Mỗi dòng gồm:

STT

Mã nguyên liệu

Tên nguyên liệu

Hàm lượng

Đơn vị

Ghi chú

### 2.3. Phần 2 - Nguyên liệu

Mỗi dòng gồm:

STT

Mã nguyên liệu

Tên nguyên liệu

Hàm lượng

Đơn vị

Ghi chú

## 3. NHÓM A - CHÁO SÂM THEO MÙA

### 3.1. A1/CS/DM/HS - CHÁO SÂM - DIÊM MẠCH & HẠT SEN

Thông tin đầu

## SKU: A1/CS/DM/HS

Tên sản phẩm: Cháo Sâm - Diêm mạch & Hạt sen

Mã công thức: FML-A1-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg | Dược liệu chủ đạo |
| 2 | DL-002 | Hoài sơn | 15.5 | kg | Dược liệu |
| 3 | DL-003 | Bạch linh | 5.0 | kg | Dược liệu |
| 4 | DL-004 | Kỷ tử | 3.0 | kg | Dược liệu |
| 5 | DL-005 | Gừng nướng | 0.7 | kg | Dược liệu |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-001 | Diêm mạch | 15.5 | kg | Thành phần chính |
| 2 | NL-002 | Hạt sen | 9 | kg | Thành phần chính |
| 3 | NL-003 | Gạo lúa - tôm | 195 | kg | Nền dinh dưỡng |
| 4 | NL-004 | Cà rốt | 35 | kg | Nền / nước hầm |
| 5 | NL-005 | Bí đỏ | 33 | kg | Nền dinh dưỡng |
| 6 | NL-006 | Đậu xanh không vỏ | 29.5 | kg | Nền dinh dưỡng |
| 7 | NL-007 | Nấm kim châm | 18.5 | kg | Nền dinh dưỡng |
| 8 | NL-008 | Táo tàu | 4.4 | kg | Nước hầm |
| 9 | NL-009 | Củ cải trắng | 49.2 | kg | Nước hầm |
| 10 | NL-010 | Rong biển kombu / wakame | 7 | kg | Nước hầm |
| 11 | NL-011 | Hành tây | 21.3 | kg | Nước hầm |
| 12 | NL-012 | Nước dừa nguyên chất | 55 | lít | Nước hầm |
| 13 | NL-013 | Muối biển rang | 12.84 | kg | Nêm |
| 14 | NL-014 | Tiêu đen rang | 1.32 | kg | Nêm |
| 15 | NL-015 | Tỏi nướng | 0.72 | kg | Nêm |
| 16 | NL-016 | Hành lá | 2.19 | kg | Nêm |
| 17 | NL-017 | Rễ cần tây | 1.25 | kg | Nêm |

### 3.2. A2/CS/BASA - CHÁO SÂM - CÁ BASA

Thông tin đầu

## SKU: A2/CS/BASA

Tên sản phẩm: Cháo Sâm - Cá Basa

Mã công thức: FML-A2-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg | Dược liệu chủ đạo |
| 2 | DL-002 | Hoài sơn | 15.5 | kg | Dược liệu |
| 3 | DL-003 | Bạch linh | 9 | kg | Dược liệu |
| 4 | DL-004 | Kỷ tử | 3.0 | kg | Dược liệu |
| 5 | DL-005 | Gừng nướng | 0.4 | kg | Dược liệu |
| 6 | DL-006 | Trần bì | 0.4 | kg | Dược liệu |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-018 | Cá Basa | 22 | kg | Thành phần chính |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg | Nền dinh dưỡng |
| 3 | NL-004 | Cà rốt | 35 | kg | Nền / nước hầm |
| 4 | NL-005 | Bí đỏ | 33 | kg | Nền dinh dưỡng |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg | Nền dinh dưỡng |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg | Nền dinh dưỡng |
| 7 | NL-008 | Táo tàu | 4.4 | kg | Nước hầm |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg | Nước hầm |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg | Nước hầm |
| 10 | NL-011 | Hành tây | 21.3 | kg | Nước hầm |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít | Nước hầm |
| 12 | NL-013 | Muối biển rang | 12.84 | kg | Nêm |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg | Nêm |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg | Nêm |
| 15 | NL-016 | Hành lá | 2.19 | kg | Nêm |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg | Nêm |

### 3.3. A3/CS/CAHOI - CHÁO SÂM - CÁ HỒI

Thông tin đầu

## SKU: A3/CS/CAHOI

Tên sản phẩm: Cháo Sâm - Cá hồi

Mã công thức: FML-A3-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-019 | Cá hồi | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 3.4. A4/CS/LUON - CHÁO SÂM - LƯƠN ĐỒNG

Thông tin đầu

## SKU: A4/CS/LUON

Tên sản phẩm: Cháo Sâm - Lươn đồng

Mã công thức: FML-A4-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-020 | Lươn đồng | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 3.5. A5/CS/CUU - CHÁO SÂM - THỊT CỪU & TÁO TÀU

Thông tin đầu

## SKU: A5/CS/CUU

Tên sản phẩm: Cháo Sâm - Thịt cừu & Táo tàu

Mã công thức: FML-A5-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |
| 5 | DL-007 | Quế chi | 0.25 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-021 | Thịt cừu | 15.5 | kg |   |
| 2 | NL-008 | Táo tàu | 6.0 | kg | Thành phần chính |
| 3 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 4 | NL-004 | Cà rốt | 35 | kg |   |
| 5 | NL-005 | Bí đỏ | 33 | kg |   |
| 6 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 7 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

## 4. NHÓM B - CHÁO SÂM CHỨC NĂNG

### 4.1. B1/CS/RM/ĐX - CHÁO SÂM - RAU MÁ & ĐẬU XANH

Thông tin đầu

## SKU: B1/CS/RM/ĐX

Tên sản phẩm: Cháo Sâm - Rau má & Đậu xanh

Mã công thức: FML-B1-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-022 | Rau má | 15.5 | kg |   |
| 2 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 3 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 4 | NL-004 | Cà rốt | 35 | kg |   |
| 5 | NL-005 | Bí đỏ | 33 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 4.2. B2/CS/DHA - CHÁO SÂM - DHA NÃO BỘ

Thông tin đầu

## SKU: B2/CS/DHA

Tên sản phẩm: Cháo Sâm - DHA Não bộ

Mã công thức: FML-B2-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.4 | kg |   |
| 5 | DL-006 | Trần bì | 0.4 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-019 | Cá hồi | 15.5 | kg |   |
| 2 | NL-023 | Vừng | 6.0 | kg |   |
| 3 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 4 | NL-004 | Cà rốt | 35 | kg |   |
| 5 | NL-005 | Bí đỏ | 33 | kg |   |
| 6 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 7 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 8 | NL-008 | Táo tàu | 4.4 | kg |   |
| 9 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 10 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 11 | NL-011 | Hành tây | 21.3 | kg |   |
| 12 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 13 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 14 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 15 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 16 | NL-016 | Hành lá | 2.19 | kg |   |
| 17 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 4.3. B3/CS/CACOM - CHÁO SÂM - CÁ CƠM & VỪNG

Thông tin đầu

## SKU: B3/CS/CACOM

Tên sản phẩm: Cháo Sâm - Cá cơm & Vừng

Mã công thức: FML-B3-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-024 | Cá cơm | 15.5 | kg |   |
| 2 | NL-023 | Vừng | 6.0 | kg |   |
| 3 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 4 | NL-004 | Cà rốt | 35 | kg |   |
| 5 | NL-005 | Bí đỏ | 33 | kg |   |
| 6 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 7 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 8 | NL-008 | Táo tàu | 4.4 | kg |   |
| 9 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 10 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 11 | NL-011 | Hành tây | 21.3 | kg |   |
| 12 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 13 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 14 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 15 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 16 | NL-016 | Hành lá | 2.19 | kg |   |
| 17 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 4.4. B4/CS/COLAGEN - CHÁO SÂM - THỊT HEO & DA HEO

Thông tin đầu

## SKU: B4/CS/COLAGEN

Tên sản phẩm: Cháo Sâm - Thịt heo & Da heo

Mã công thức: FML-B4-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-025 | Thịt heo nạc | 10.5 | kg |   |
| 2 | NL-026 | Da heo | 5.0 | kg |   |
| 3 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 4 | NL-004 | Cà rốt | 35 | kg |   |
| 5 | NL-005 | Bí đỏ | 33 | kg |   |
| 6 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 7 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 8 | NL-008 | Táo tàu | 4.4 | kg |   |
| 9 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 10 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 11 | NL-011 | Hành tây | 21.3 | kg |   |
| 12 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 13 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 14 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 15 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 16 | NL-016 | Hành lá | 2.19 | kg |   |
| 17 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 4.5. B5/CS/SINHLUC - CHÁO SÂM - HÀU BIỂN

Thông tin đầu

## SKU: B5/CS/SINHLUC

Tên sản phẩm: Cháo Sâm - Hàu biển

Mã công thức: FML-B5-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-006 | Trần bì | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-027 | Hàu biển | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 4.6. B6/CS/GAAC - CHÁO SÂM - GÀ ÁC

Thông tin đầu

## SKU: B6/CS/GAAC

Tên sản phẩm: Cháo Sâm - Gà ác

Mã công thức: FML-B6-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-028 | Gà ác | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

## 5. NHÓM C - CHÁO SÂM BỔ DƯỠNG

### 5.1. C1/CS/BAONGU - CHÁO SÂM - BÀO NGƯ

Thông tin đầu

## SKU: C1/CS/BAONGU

Tên sản phẩm: Cháo Sâm - Bào ngư

Mã công thức: FML-C1-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-029 | Bào ngư | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 5.2. C2/CS/DONGTRUNG - CHÁO SÂM - ĐÔNG TRÙNG HẠ THẢO

Thông tin đầu

## SKU: C2/CS/DONGTRUNG

Tên sản phẩm: Cháo Sâm - Đông trùng hạ thảo

Mã công thức: FML-C2-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-008 | Đông trùng hạ thảo | 15.5 | kg |   |
| 3 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 4 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 5 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 2 | NL-004 | Cà rốt | 35 | kg |   |
| 3 | NL-005 | Bí đỏ | 33 | kg |   |
| 4 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 5 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 6 | NL-008 | Táo tàu | 4.4 | kg |   |
| 7 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 8 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 9 | NL-011 | Hành tây | 21.3 | kg |   |
| 10 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 11 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 12 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 13 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 14 | NL-016 | Hành lá | 2.19 | kg |   |
| 15 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 5.3. C3/CS/NAMDONGCO - CHÁO SÂM - NẤM ĐÔNG CÔ

Thông tin đầu

## SKU: C3/CS/NAMDONGCO

Tên sản phẩm: Cháo Sâm - Nấm đông cô

Mã công thức: FML-C3-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-030 | Nấm đông cô | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 5.4. C4/CS/CUABIEN - CHÁO SÂM - CUA BIỂN

Thông tin đầu

## SKU: C4/CS/CUABIEN

Tên sản phẩm: Cháo Sâm - Cua biển

Mã công thức: FML-C4-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-031 | Cua biển | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 5.5. C5/CS/CANGU - CHÁO SÂM - CÁ NGỪ

Thông tin đầu

## SKU: C5/CS/CANGU

Tên sản phẩm: Cháo Sâm - Cá ngừ

Mã công thức: FML-C5-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-003 | Bạch linh | 5.0 | kg |   |
| 4 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 5 | DL-009 | Táo tàu | 4.4 | kg |   |
| 6 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-032 | Cá ngừ | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu dùng nước hầm | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 5.6. C6/CS/TOM/RONGBIEN - CHÁO SÂM - TÔM & RONG BIỂN

Thông tin đầu

## SKU: C6/CS/TOM/RONGBIEN

Tên sản phẩm: Cháo Sâm - Tôm & Rong biển

Mã công thức: FML-C6-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-033 | Tôm | 15.5 | kg |   |
| 2 | NL-034 | Rong biển | 15.5 | kg |   |
| 3 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 4 | NL-004 | Cà rốt | 35 | kg |   |
| 5 | NL-005 | Bí đỏ | 33 | kg |   |
| 6 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 7 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 8 | NL-008 | Táo tàu | 4.4 | kg |   |
| 9 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 10 | NL-010 | Rong biển kombu / wakame dùng nước hầm | 7 | kg |   |
| 11 | NL-011 | Hành tây | 21.3 | kg |   |
| 12 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 13 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 14 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 15 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 16 | NL-016 | Hành lá | 2.19 | kg |   |
| 17 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 5.7. C7/CS/THITGA - CHÁO SÂM - THỊT GÀ

Thông tin đầu

## SKU: C7/CS/THITGA

Tên sản phẩm: Cháo Sâm - Thịt gà

Mã công thức: FML-C7-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-035 | Thịt gà | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 5.8. C8/CS/THITHEO - CHÁO SÂM - THỊT HEO

Thông tin đầu

## SKU: C8/CS/THITHEO

Tên sản phẩm: Cháo Sâm - Thịt heo

Mã công thức: FML-C8-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-036 | Thịt heo | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

### 5.9. C9/CS/THITBO - CHÁO SÂM - THỊT BÒ

Thông tin đầu

## SKU: C9/CS/THITBO

Tên sản phẩm: Cháo Sâm - Thịt bò

Mã công thức: FML-C9-G0

Version: G0

Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | DL-001 | Sâm Savigin | 9 | kg |   |
| 2 | DL-002 | Hoài sơn | 15.5 | kg |   |
| 3 | DL-004 | Kỷ tử | 3.0 | kg |   |
| 4 | DL-005 | Gừng nướng | 0.8 | kg |   |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | NL-037 | Thịt bò | 15.5 | kg |   |
| 2 | NL-003 | Gạo lúa - tôm | 195 | kg |   |
| 3 | NL-004 | Cà rốt | 35 | kg |   |
| 4 | NL-005 | Bí đỏ | 33 | kg |   |
| 5 | NL-006 | Đậu xanh không vỏ | 29.5 | kg |   |
| 6 | NL-007 | Nấm kim châm | 18.5 | kg |   |
| 7 | NL-008 | Táo tàu | 4.4 | kg |   |
| 8 | NL-009 | Củ cải trắng | 49.2 | kg |   |
| 9 | NL-010 | Rong biển kombu / wakame | 7 | kg |   |
| 10 | NL-011 | Hành tây | 21.3 | kg |   |
| 11 | NL-012 | Nước dừa nguyên chất | 55 | lít |   |
| 12 | NL-013 | Muối biển rang | 12.84 | kg |   |
| 13 | NL-014 | Tiêu đen rang | 1.32 | kg |   |
| 14 | NL-015 | Tỏi nướng | 0.72 | kg |   |
| 15 | NL-016 | Hành lá | 2.19 | kg |   |
| 16 | NL-017 | Rễ cần tây | 1.25 | kg |   |

## 6. QUY TẮC CỨNG

### 6.1. Không SKU nào được sản xuất nếu chưa có BOM / Recipe Map

### 6.2. Không cho vận hành chọn lại nguyên liệu ở bước sản xuất

### 6.3. Mọi thay đổi nguyên liệu hoặc hàm lượng phải đi qua version công thức

### 6.4. Tất cả nguyên liệu phải lấy từ danh mục nguyên liệu canonical

### 6.5. Mọi phiếu cấp nguyên liệu cho sản xuất phải bám đúng map này

### 6.6. Tất cả nguyên liệu phải hiện đầy đủ từng dòng, không dùng dòng gom nhóm mơ hồ

## 7. PASS / FAIL GATE

### 7.1. Đạt khi

đủ 20 SKU có BOM / Recipe Map

mỗi SKU map được ra danh sách dược liệu và nguyên liệu

có hàm lượng và đơn vị

có mã công thức và version

hệ thống dùng được map này để tự hiện công thức

dùng được để sinh phiếu đề nghị cấp nguyên liệu cho sản xuất

### 7.2. Chưa đạt khi

thiếu SKU

thiếu nguyên liệu

thiếu hàm lượng

thiếu version

vẫn phải suy luận tay từ tên SKU

vẫn dùng dòng gom nhóm thay vì nguyên liệu cụ thể

bản “20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD” theo cùng format sạch, Word-ready.

Bản này nối từ:

## BỘ 20 SKU CANONICAL - GINSENGFOOD

## 20 SKU ↔ NGUYÊN LIỆU / BOM MAP - GINSENGFOOD

bộ quy tắc phiếu tự sinh

quy tắc in mã 2 giai đoạn

quy tắc kết nối MISA toàn hệ

Mục đích của file này là khóa cho từng SKU:

đang dùng công thức nào

đóng gói cấp 1 / cấp 2 thế nào

in gì ở từng cấp

QC nào bắt buộc

điều kiện release nào

trace / recall áp dụng ra sao

đơn vị vận hành chính là gì

Đây là file rất quan trọng, vì nếu không có nó thì dù có SKU, nguyên liệu và BOM, đội dev vẫn phải tự suy luận cách vận hành từng SKU trong Operational.

## 20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD

## 0. MỞ ĐẦU TÀI LIỆU

### 0.1. Mục đích tài liệu

Tài liệu này dùng để khóa cấu hình vận hành theo từng SKU cho Operational Domain của Ginsengfood.

Tài liệu này chỉ dùng để khóa:

công thức đang áp dụng theo SKU

quy cách đóng gói cấp 1

quy cách đóng gói cấp 2

nội dung in cấp 1

nội dung in cấp 2

QC checkpoint bắt buộc

điều kiện release

traceability rule

recall applicability

đơn vị vận hành chính theo từng công đoạn

### 0.2. Vai trò tài liệu

Đây là mắt xích nối giữa:

SKU Master

BOM / Recipe Map

Packaging

QC / Release

Traceability

Recall

MISA Integration

### 0.3. Ranh giới tài liệu

Tài liệu này không giữ:

master SKU

danh mục nguyên liệu

chi tiết BOM từng dòng nguyên liệu

phiếu vận hành chi tiết

code API / code UI

logic kế toán chi tiết

Tài liệu này chỉ giữ operational config theo SKU.

### 0.4. Nguyên tắc dùng tài liệu

Operational Domain không owner master SKU. Operational chỉ owner:

config vận hành theo SKU

dữ liệu vận hành phát sinh từ SKU

Mọi SKU trong Operational phải đọc từ bộ SKU canonical đã khóa trước.

## 1. NGUYÊN TẮC CHUNG

### 1.1. Một SKU phải có đủ operational config mới được vận hành

Một SKU chỉ được xem là sẵn sàng vận hành khi có đủ:

BOM / Recipe version đang áp dụng

config đóng gói cấp 1

config đóng gói cấp 2

config in mã

config QC

config release

config trace

config recall

### 1.2. Không SKU nào được sản xuất nếu thiếu config

Nếu thiếu một trong các cấu hình trên, SKU đó chưa được đưa vào luồng vận hành chính thức.

### 1.3. Operational config phải theo version

Mỗi SKU phải ghi rõ:

version công thức đang áp dụng

version operational config đang áp dụng nếu có

ngày hiệu lực

### 1.4. Mọi thay đổi config phải đi qua approval

Nếu thay đổi:

đơn vị đóng gói

nội dung in

QC checkpoint

điều kiện release

trace rule

recall rule

thì phải tạo version mới hoặc bản config mới, không sửa đè bản đã dùng cho mẻ cũ.

## 2. CẤU TRÚC CHUẨN CHO MỖI SKU OPERATIONAL CONFIG

### 2.1. Thông tin đầu

SKU

Tên sản phẩm

Nhóm sản phẩm

Phân loại Vegan / Mặn

Mã công thức đang áp dụng

Version công thức

Trạng thái operational config

### 2.2. Packaging Config

đơn vị cấp 1

đơn vị cấp 2

quy cách gom cấp 1 → cấp 2

quy cách gom cấp 2 → nhập kho

### 2.3. Print Config

nội dung in cấp 1

nội dung in cấp 2

template in cấp 1

template in cấp 2

rule reprint

### 2.4. QC / Release Config

checkpoint QC bắt buộc

có bắt buộc % độ ẩm sau sấy hay không

điều kiện đạt để vào đóng gói

điều kiện đạt để nhập kho

### 2.5. Trace / Recall Config

public trace có / không

internal trace có / không

đơn vị truy vết chính

recall applicability

cấp dữ liệu dùng cho recall

## 3. BỘ QUY TẮC CẤU HÌNH CHUNG ÁP DỤNG CHO 20 SKU

### 3.1. Công thức

Dùng công thức version G0 làm baseline ban đầu

Khi tối ưu, tạo version mới

Lệnh sản xuất luôn kéo version đang áp dụng

### 3.2. Đóng gói cấp 1

Đơn vị được phép:

gói

lọ

hũ

### 3.3. Đóng gói cấp 2

Đơn vị được phép:

hộp

thùng

lọ

hũ

### 3.4. In cấp 1

Chỉ in:

ngày sản xuất

hạn dùng

### 3.5. In cấp 2

Phải in:

lô sản xuất

ngày sản xuất

hạn dùng

mã vạch

mã QR

### 3.6. QC sau sấy

Bắt buộc có:

số lượng theo khay

% độ ẩm

### 3.7. Nhập kho thành phẩm

Đơn vị nhập kho:

hộp

thùng

lọ

hũ

### 3.8. Traceability

Public trace và internal trace phải tách rõ. Cấp 2 là cấp mang dữ liệu truy vết chính.

### 3.9. Recall

Recall phải lần ngược được:

đơn vị cấp 2

batch / lot

mẻ sản xuất

lô nguyên liệu

## 4. NHÓM A - CHÁO SÂM THEO MÙA

### 4.1. A1/CS/DM/HS - CHÁO SÂM - DIÊM MẠCH & HẠT SEN

Thông tin đầu

## SKU: A1/CS/DM/HS

Tên sản phẩm: Cháo Sâm - Diêm mạch & Hạt sen

Nhóm sản phẩm: Theo mùa

Phân loại: Không thuần chay (vegan claim khóa do công thức/BOM có thành phần động vật)

Mã công thức áp dụng: FML-A1-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Quy cách gom cấp 1 → cấp 2: theo cấu hình đóng gói

Quy cách gom cấp 2 → nhập kho: theo hộp / thùng

Print Config

In cấp 1: ngày sản xuất, hạn dùng

In cấp 2: lô sản xuất, ngày sản xuất, hạn dùng, barcode, QR

Reprint cấp 2: có kiểm soát

QC / Release Config

Bắt buộc phiếu theo dõi sơ chế

Bắt buộc kiểm chất lượng sau sấy

Bắt buộc % độ ẩm

Bắt buộc QC thành phẩm trước nhập kho

Trace / Recall Config

Public trace: Có

Internal trace: Có

Đơn vị truy vết chính: hộp

Recall applicability: Có

### 4.2. A2/CS/BASA - CHÁO SÂM - CÁ BASA

Thông tin đầu

## SKU: A2/CS/BASA

Tên sản phẩm: Cháo Sâm - Cá Basa

Nhóm sản phẩm: Theo mùa

Phân loại: Mặn

Mã công thức áp dụng: FML-A2-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Có kiểm sau sấy

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 4.3. A3/CS/CAHOI - CHÁO SÂM - CÁ HỒI

Thông tin đầu

## SKU: A3/CS/CAHOI

Tên sản phẩm: Cháo Sâm - Cá hồi

Nhóm sản phẩm: Theo mùa

Phân loại: Mặn

Mã công thức áp dụng: FML-A3-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Bắt buộc kiểm sau sấy

Bắt buộc % độ ẩm

QC thành phẩm trước nhập kho

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 4.4. A4/CS/LUON - CHÁO SÂM - LƯƠN ĐỒNG

Thông tin đầu

## SKU: A4/CS/LUON

Tên sản phẩm: Cháo Sâm - Lươn đồng

Nhóm sản phẩm: Theo mùa

Phân loại: Mặn

Mã công thức áp dụng: FML-A4-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Có kiểm sau sấy

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 4.5. A5/CS/CUU - CHÁO SÂM - THỊT CỪU & TÁO TÀU

Thông tin đầu

## SKU: A5/CS/CUU

Tên sản phẩm: Cháo Sâm - Thịt cừu & Táo tàu

Nhóm sản phẩm: Theo mùa

Phân loại: Mặn

Mã công thức áp dụng: FML-A5-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Có kiểm sau sấy

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

## 5. NHÓM B - CHÁO SÂM CHỨC NĂNG

### 5.1. B1/CS/RM/ĐX - CHÁO SÂM - RAU MÁ & ĐẬU XANH

Thông tin đầu

## SKU: B1/CS/RM/ĐX

Tên sản phẩm: Cháo Sâm - Rau má & Đậu xanh

Nhóm sản phẩm: Chức năng

Phân loại: Không thuần chay (vegan claim khóa do công thức/BOM có thành phần động vật)

Mã công thức áp dụng: FML-B1-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 5.2. B2/CS/DHA - CHÁO SÂM - DHA NÃO BỘ

Thông tin đầu

## SKU: B2/CS/DHA

Tên sản phẩm: Cháo Sâm - DHA Não bộ

Nhóm sản phẩm: Chức năng

Phân loại: Mặn

Mã công thức áp dụng: FML-B2-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 5.3. B3/CS/CACOM - CHÁO SÂM - CÁ CƠM & VỪNG

Thông tin đầu

## SKU: B3/CS/CACOM

Tên sản phẩm: Cháo Sâm - Cá cơm & Vừng

Nhóm sản phẩm: Chức năng

Phân loại: Mặn

Mã công thức áp dụng: FML-B3-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 5.4. B4/CS/COLAGEN - CHÁO SÂM - THỊT HEO & DA HEO

Thông tin đầu

## SKU: B4/CS/COLAGEN

Tên sản phẩm: Cháo Sâm - Thịt heo & Da heo

Nhóm sản phẩm: Chức năng

Phân loại: Mặn

Mã công thức áp dụng: FML-B4-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 5.5. B5/CS/SINHLUC - CHÁO SÂM - HÀU BIỂN

Thông tin đầu

## SKU: B5/CS/SINHLUC

Tên sản phẩm: Cháo Sâm - Hàu biển

Nhóm sản phẩm: Chức năng

Phân loại: Mặn

Mã công thức áp dụng: FML-B5-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 5.6. B6/CS/GAAC - CHÁO SÂM - GÀ ÁC

Thông tin đầu

## SKU: B6/CS/GAAC

Tên sản phẩm: Cháo Sâm - Gà ác

Nhóm sản phẩm: Chức năng

Phân loại: Mặn

Mã công thức áp dụng: FML-B6-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

## 6. NHÓM C - CHÁO SÂM BỔ DƯỠNG

### 6.1. C1/CS/BAONGU - CHÁO SÂM - BÀO NGƯ

Thông tin đầu

## SKU: C1/CS/BAONGU

Tên sản phẩm: Cháo Sâm - Bào ngư

Nhóm sản phẩm: Bổ dưỡng

Phân loại: Mặn

Mã công thức áp dụng: FML-C1-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 6.2. C2/CS/DONGTRUNG - CHÁO SÂM - ĐÔNG TRÙNG HẠ THẢO

Thông tin đầu

## SKU: C2/CS/DONGTRUNG

Tên sản phẩm: Cháo Sâm - Đông trùng hạ thảo

Nhóm sản phẩm: Bổ dưỡng

Phân loại: Không thuần chay (vegan claim khóa do công thức/BOM có thành phần động vật)

Mã công thức áp dụng: FML-C2-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 6.3. C3/CS/NAMDONGCO - CHÁO SÂM - NẤM ĐÔNG CÔ

Thông tin đầu

## SKU: C3/CS/NAMDONGCO

Tên sản phẩm: Cháo Sâm - Nấm đông cô

Nhóm sản phẩm: Bổ dưỡng

Phân loại: Không thuần chay (vegan claim khóa do công thức/BOM có thành phần động vật)

Mã công thức áp dụng: FML-C3-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 6.4. C4/CS/CUABIEN - CHÁO SÂM - CUA BIỂN

Thông tin đầu

## SKU: C4/CS/CUABIEN

Tên sản phẩm: Cháo Sâm - Cua biển

Nhóm sản phẩm: Bổ dưỡng

Phân loại: Mặn

Mã công thức áp dụng: FML-C4-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 6.5. C5/CS/CANGU - CHÁO SÂM - CÁ NGỪ

Thông tin đầu

## SKU: C5/CS/CANGU

Tên sản phẩm: Cháo Sâm - Cá ngừ

Nhóm sản phẩm: Bổ dưỡng

Phân loại: Mặn

Mã công thức áp dụng: FML-C5-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 6.6. C6/CS/TOM/RONGBIEN - CHÁO SÂM - TÔM & RONG BIỂN

Thông tin đầu

## SKU: C6/CS/TOM/RONGBIEN

Tên sản phẩm: Cháo Sâm - Tôm & Rong biển

Nhóm sản phẩm: Bổ dưỡng

Phân loại: Mặn

Mã công thức áp dụng: FML-C6-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 6.7. C7/CS/THITGA - CHÁO SÂM - THỊT GÀ

Thông tin đầu

## SKU: C7/CS/THITGA

Tên sản phẩm: Cháo Sâm - Thịt gà

Nhóm sản phẩm: Bổ dưỡng

Phân loại: Mặn

Mã công thức áp dụng: FML-C7-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 6.8. C8/CS/THITHEO - CHÁO SÂM - THỊT HEO

Thông tin đầu

## SKU: C8/CS/THITHEO

Tên sản phẩm: Cháo Sâm - Thịt heo

Nhóm sản phẩm: Bổ dưỡng

Phân loại: Mặn

Mã công thức áp dụng: FML-C8-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

### 6.9. C9/CS/THITBO - CHÁO SÂM - THỊT BÒ

Thông tin đầu

## SKU: C9/CS/THITBO

Tên sản phẩm: Cháo Sâm - Thịt bò

Nhóm sản phẩm: Bổ dưỡng

Phân loại: Mặn

Mã công thức áp dụng: FML-C9-G0

Version công thức: G0

Trạng thái config: ACTIVE_PILOT

Packaging Config

Đơn vị cấp 1: gói

Đơn vị cấp 2: hộp / thùng

Print Config

Cấp 1: NSX, HSD

Cấp 2: Lô, NSX, HSD, barcode, QR

QC / Release Config

Có sơ chế

Bắt buộc % độ ẩm

QC thành phẩm bắt buộc

Trace / Recall Config

Public trace: Có

Internal trace: Có

Recall applicability: Có

## 7. QUY TẮC CỨNG

### 7.1. Không SKU nào được đưa vào sản xuất nếu thiếu operational config

### 7.2. Không SKU nào được đóng gói nếu thiếu packaging config

### 7.3. Không SKU nào được in mã cấp 2 nếu thiếu print config cấp 2

### 7.4. Không SKU nào được nhập kho nếu thiếu QC / release config

### 7.5. Không SKU nào được public trace nếu chưa bật trace config

### 7.6. Không SKU nào được recall chuẩn nếu thiếu trace / recall config

### 7.7. Operational config không được sửa đè âm thầm; mọi thay đổi phải có version hoặc bản config mới

## 8. PASS / FAIL GATE

### 8.1. Đạt khi

đủ 20 SKU có operational config

mỗi SKU có:

công thức áp dụng

packaging cấp 1 / cấp 2

print rule

QC / release rule

trace / recall rule

không phải đoán thủ công khi vận hành

hệ thống dùng được config này để điều phối production → packaging → QC → nhập kho → trace → recall

### 8.2. Chưa đạt khi

SKU có BOM nhưng thiếu operational config

đóng gói / in mã / QC / trace vẫn phải đoán tay

thiếu rule cấp 1 / cấp 2

thiếu recall applicability

thiếu version hoặc trạng thái config

## 9. KẾT LUẬN

Từ đây, 20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD được khóa ở mức:

mỗi SKU có cấu hình vận hành riêng

packaging / print / QC / release / trace / recall đã có rule nền

đủ để đi tiếp sang:

Quy tắc version công thức / phê duyệt thay đổi công thức

Trace data chain / batch-lot-QR-box-case map

Bộ đặc tả màn hình / route / action cho toàn bộ phiếu vận hành

bản “QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC - GINSENGFOOD” theo format sạch, Word-ready.

File này là mắt xích rất quan trọng, vì nếu không khóa chỗ này thì sau khi chạy vài mẻ thực tế, công thức sẽ rất dễ bị sửa chồng, mất lịch sử, không biết mẻ nào dùng version nào, và lúc đó truy lỗi, tối ưu cảm quan, tính giá thành hay recall đều rất khó. Với hệ thống thật, không thể chỉ copy-paste vài dòng code AI rồi mong tự quản được lịch sử công thức; phải khóa rõ version, approval, hiệu lực và dấu vết thay đổi.

## QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC - GINSENGFOOD

## 0. MỞ ĐẦU TÀI LIỆU

### 0.1. Mục đích tài liệu

Tài liệu này dùng để khóa quy tắc:

tạo version công thức

phân loại công thức gốc / pilot / áp dụng chính thức

phê duyệt thay đổi công thức

ghi nhận hiệu lực theo thời gian và theo mẻ sản xuất

truy xuất lịch sử thay đổi công thức

### 0.2. Vai trò tài liệu

Tài liệu này là nguồn chuẩn để:

đội R&D / kỹ thuật công thức đề xuất thay đổi

quản lý nhà máy phê duyệt dùng trong sản xuất

QC kiểm theo đúng version

kho cấp nguyên liệu theo đúng version

kế toán tính chi phí theo đúng version

dev dựng đúng logic version / approval / trace lịch sử

### 0.3. Ranh giới tài liệu

Tài liệu này không thay:

bộ 20 SKU canonical

danh mục nguyên liệu canonical

BOM / Recipe Map hiện hành

Operational Config Map

bộ phiếu vận hành

Tài liệu này chỉ giữ rule version và approval thay đổi công thức.

### 0.4. Nguyên tắc nền

công thức nghiên cứu trong đề tài là baseline ban đầu

sản xuất thực tế có thể phải tối ưu dần

mọi thay đổi phải đi qua version mới

không sửa đè version cũ

mỗi mẻ phải truy được mình đã dùng version nào

## 1. KẾT LUẬN KHÓA

### 1.1. Kết luận tổng quát

Mọi công thức trong Ginsengfood phải được quản lý theo version.

Không cho phép:

sửa trực tiếp công thức đang dùng mà không tạo version mới

thay đổi nguyên liệu / hàm lượng trong lệnh sản xuất thường

mất lịch sử version cũ

### 1.2. Kết luận về version

Mỗi công thức phải có tối thiểu:

mã công thức

version

trạng thái version

ngày hiệu lực

người tạo

người phê duyệt

lý do thay đổi

phạm vi thay đổi

### 1.3. Kết luận về phê duyệt

Không version nào được đưa vào sản xuất thường nếu chưa được phê duyệt đúng vai trò.

### 1.4. Kết luận về truy xuất

Mỗi mẻ sản xuất phải ghi được:

SKU

mã công thức

version công thức

thời điểm áp dụng

## 2. PHÂN LOẠI VERSION CÔNG THỨC

### 2.1. Version gốc nghiên cứu

Là version baseline đầu tiên, lấy trực tiếp từ đề tài nghiên cứu.

Vai trò:

làm chuẩn gốc ban đầu

làm mốc đối chiếu khi tối ưu về sau

Quy ước khuyến nghị:

G0

### 2.2. Version pilot / sản xuất mẫu

Là version được dùng để chạy thử thực tế tại nhà máy, chưa phải bản áp dụng đại trà.

Vai trò:

kiểm tra cảm quan

kiểm tra vận hành

kiểm tra hao hụt

kiểm tra ổn định sấy / đóng gói

kiểm tra tính khả thi sản xuất công nghiệp

Quy ước khuyến nghị:

## P1, P2, P3...

### 2.3. Version áp dụng chính thức

Là version đã qua đánh giá và được phép dùng cho vận hành sản xuất thường.

Vai trò:

dùng cho lệnh sản xuất chuẩn

dùng để cấp phát nguyên liệu

dùng cho QC / release / trace / recall

Quy ước khuyến nghị:

## R1, R2, R3...

### 2.4. Version ngưng áp dụng

Là version cũ không còn dùng cho sản xuất mới nhưng vẫn phải lưu để:

truy lịch sử

đối chiếu mẻ cũ

phục vụ kiểm tra / recall / phân tích lỗi

## 3. TRẠNG THÁI VERSION BẮT BUỘC

### 3.1. DRAFT

Mới tạo, chưa được dùng.

### 3.2. REVIEW_AWAITING_REVIEW

Đã gửi duyệt, chờ xem xét.

### 3.3. PILOT_APPROVED

Được phép chạy pilot / sản xuất mẫu.

### 3.4. ACTIVE

Được phép dùng cho sản xuất thường.

### 3.5. REJECTED

Bị từ chối, không được dùng.

### 3.6. DEPRECATED

Ngưng áp dụng, giữ để tra cứu lịch sử.

## 4. KHI NÀO BẮT BUỘC TẠO VERSION MỚI

Phải tạo version mới khi có thay đổi về:

### 4.1. Thành phần nguyên liệu

thêm nguyên liệu

bỏ nguyên liệu

thay nguyên liệu này bằng nguyên liệu khác

### 4.2. Hàm lượng

tăng hàm lượng

giảm hàm lượng

đổi định mức theo mẻ

### 4.3. Đơn vị hoặc quy cách áp dụng

đổi kg sang lít / ml khi bản chất công thức thay đổi

đổi quy cách sử dụng trong sản xuất

### 4.4. Ghi chú kỹ thuật ảnh hưởng vận hành

đổi cách sơ chế

đổi cách phối trộn

đổi thứ tự cho nguyên liệu vào

đổi rule sấy / hoàn nguyên nếu gắn trực tiếp với công thức

### 4.5. Thay đổi ảnh hưởng cảm quan hoặc chất lượng

thay đổi để ngon hơn

thay đổi để ổn định hơn

thay đổi để giảm hao hụt

thay đổi để phù hợp sản xuất công nghiệp

## 5. NHỮNG GÌ KHÔNG ĐƯỢC SỬA ĐÈ

Không được sửa đè trực tiếp trên version đã có hiệu lực đối với các trường:

danh sách nguyên liệu

hàm lượng

đơn vị

ghi chú kỹ thuật cốt lõi

logic áp dụng theo mẻ

trạng thái version cũ nếu đã từng được dùng

Nếu có thay đổi, phải tạo record version mới.

## 6. THÔNG TIN BẮT BUỘC CỦA MỖI VERSION

Mỗi version công thức phải có tối thiểu:

SKU

tên sản phẩm

mã công thức

version

loại version:

Gốc nghiên cứu

Pilot

Chính thức

trạng thái

ngày tạo

người tạo

ngày gửi duyệt

người duyệt

ngày hiệu lực

lý do thay đổi

mô tả thay đổi

version nguồn tham chiếu

ghi chú kỹ thuật

## 7. QUY TẮC PHÊ DUYỆT THAY ĐỔI CÔNG THỨC

### 7.1. Người được đề xuất thay đổi

Tối thiểu có thể gồm:

R&D

kỹ thuật sản xuất

quản lý nhà máy

QC

người được phân quyền

### 7.2. Nội dung bắt buộc khi đề xuất

Mỗi đề xuất phải có:

SKU nào

version hiện tại là gì

version đề xuất là gì

nguyên liệu nào thay đổi

hàm lượng nào thay đổi

lý do thay đổi

mục tiêu thay đổi:

cảm quan

ổn định

hao hụt

khả năng sản xuất

chi phí

đề xuất áp dụng từ mẻ nào

### 7.3. Người phê duyệt bắt buộc

Tối thiểu phải có:

quản lý nhà máy

bộ phận kỹ thuật / R&D nếu có

QC hoặc người chịu trách nhiệm chất lượng

Nếu version dùng cho sản xuất thường, không chỉ pilot, thì phải đủ các xác nhận bắt buộc.

### 7.4. Điều kiện phê duyệt version pilot

Version pilot chỉ được duyệt khi:

có lý do rõ ràng

có thay đổi được mô tả rõ

không mâu thuẫn với danh mục nguyên liệu

không phá rule đóng gói / QC / trace

có thể truy ngược về version nguồn

### 7.5. Điều kiện phê duyệt version ACTIVE

Version ACTIVE chỉ được duyệt khi:

đã có kết quả pilot hoặc kiểm chứng thực tế

đã có đánh giá cảm quan / chất lượng

đã có xác nhận từ quản lý nhà máy

đã có xác nhận từ QC

đã chốt được dùng từ mẻ nào / ngày nào

## 8. QUY TẮC ÁP DỤNG VERSION VÀO SẢN XUẤT

### 8.1. Lệnh sản xuất chỉ được kéo version hợp lệ

Khi chọn SKU, hệ thống chỉ được tự hiện:

version ở trạng thái ACTIVE hoặc

version PILOT_APPROVED nếu lệnh đó được đánh dấu chạy pilot

### 8.2. Không cho chọn version DRAFT / REJECTED / DEPRECATED

Các version này không được hiện như lựa chọn hợp lệ trong lệnh sản xuất thường.

### 8.3. Mỗi mẻ phải ghi rõ version đã dùng

Mọi mẻ sản xuất phải lưu:

SKU

mã công thức

version công thức

ngày áp dụng

người mở lệnh

### 8.4. Một mẻ chỉ dùng một version

Một mẻ sản xuất không được trộn nhiều version công thức trong cùng một batch vận hành thường, trừ khi có phiếu ngoại lệ được phê duyệt riêng.

## 9. TRUY XUẤT LỊCH SỬ VERSION

### 9.1. Hệ thống phải truy được version nguồn

Mỗi version mới phải biết mình được tạo từ:

version nào trước đó

### 9.2. Hệ thống phải truy được mẻ đã dùng version nào

Khi xem một mẻ, phải biết:

dùng version nào

ai duyệt version đó

thay đổi gì so với version trước

### 9.3. Hệ thống phải truy được toàn bộ lịch sử thay đổi

Phải nhìn được chuỗi như:

## G0 → P1 → P2 → R1 → R2 ...

## 10. QUY TẮC NGOẠI LỆ

### 10.1. Thay đổi khẩn cấp

Nếu vì lý do:

nguyên liệu thiếu

nguyên liệu không đạt

cần thay thế khẩn cấp

thì phải đi qua một phiếu thay đổi / thay thế nguyên liệu riêng, không sửa tay ngay trên lệnh sản xuất.

### 10.2. Mọi ngoại lệ đều phải để lại dấu vết

Bắt buộc có:

người đề xuất

người duyệt

lý do

phạm vi áp dụng

hiệu lực cho mẻ nào

## 11. QUY TẮC LIÊN KẾT VỚI CÁC KHỐI KHÁC

### 11.1. Liên kết với BOM / Recipe Map

Mỗi version công thức phải trỏ được về đúng BOM / Recipe Map tương ứng.

### 11.2. Liên kết với phiếu vận hành

Lệnh sản xuất, phiếu cấp phát, QC, sấy, đóng gói, nhập kho phải đều nhìn thấy version đang dùng.

### 11.3. Liên kết với Operational Config Map

Nếu version công thức làm thay đổi:

packaging

in mã

QC

trace

recall

thì phải cập nhật version operational config tương ứng, không được để lệch.

### 11.4. Liên kết với traceability

Version công thức phải là một phần của trace nội bộ để sau này biết mẻ đó sản xuất theo version nào.

## 12. KHỐI THÔNG TIN MẪU CHO MỘT VERSION

### 12.1. Thông tin đầu

SKU:

Tên sản phẩm:

Mã công thức:

Version:

Loại version:

Trạng thái:

### 12.2. Version nguồn

Version trước đó:

Ngày tạo:

Người tạo:

### 12.3. Lý do thay đổi

Mục tiêu thay đổi:

Mô tả thay đổi:

Phạm vi thay đổi:

### 12.4. Phê duyệt

Người gửi duyệt:

Ngày gửi duyệt:

Người phê duyệt:

Ngày phê duyệt:

Kết luận:

Pilot approved

Active

Rejected

### 12.5. Hiệu lực

Áp dụng từ ngày:

Áp dụng từ mẻ:

Ghi chú triển khai:

## 13. QUY TẮC CỨNG

### 13.1. Không sửa đè version cũ

### 13.2. Không sản xuất bằng version chưa được phép

### 13.3. Không cho đổi nguyên liệu trong lệnh sản xuất thường

### 13.4. Mọi thay đổi công thức phải đi qua version mới

### 13.5. Mọi mẻ phải truy được về version đã dùng

### 13.6. Version pilot và version active phải phân biệt rõ

### 13.7. Nếu thay đổi công thức ảnh hưởng đóng gói / QC / trace thì phải cập nhật config liên quan

## 14. PASS / FAIL GATE

### 14.1. Đạt khi

có version rõ ràng

có trạng thái version rõ ràng

không sửa đè version cũ

có luồng đề xuất / duyệt / hiệu lực

mẻ sản xuất truy được version

hệ thống chỉ cho dùng version hợp lệ

### 14.2. Chưa đạt khi

sửa trực tiếp công thức cũ

không biết version nào đang dùng

mẻ không truy được về version

không có người duyệt

version chưa approved vẫn đi vào sản xuất

## 15. KẾT LUẬN

Từ đây, QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC - GINSENGFOOD được khóa ở mức:

có baseline gốc

có pilot

có active

có deprecated

có approval

có hiệu lực

có truy xuất lịch sử

có liên kết sang BOM / Operational / Trace

Đây là nền để đi tiếp sang:

## TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP

## BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH

bản “TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP - GINSENGFOOD” theo format sạch, Word-ready.

File này là mắt xích khóa rất quan trọng giữa:

nguyên liệu đầu vào

lô nguyên liệu

lệnh sản xuất / mẻ sản xuất

batch thành phẩm

đóng gói cấp 1

đóng gói cấp 2

mã vạch / QR

truy vết nội bộ

truy vết công khai

thu hồi sản phẩm

Nếu không khóa khối này, về sau hệ thống có thể vẫn nhập kho, vẫn in QR, vẫn bán hàng, nhưng sẽ không truy ngược được rõ:

hộp nào thuộc batch nào

batch nào dùng lô nguyên liệu nào

thùng nào chứa hộp nào

QR nào đại diện cho đơn vị nào

recall phải chặn ở cấp nào

## TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP - GINSENGFOOD

## 0. MỞ ĐẦU TÀI LIỆU

### 0.1. Mục đích tài liệu

Tài liệu này dùng để khóa chuỗi dữ liệu truy vết của Ginsengfood từ đầu vào nguyên liệu đến thành phẩm đóng gói và thu hồi.

Tài liệu này chỉ dùng để khóa:

lot nguyên liệu

lệnh sản xuất / mẻ sản xuất

batch thành phẩm

đơn vị đóng gói cấp 1

đơn vị đóng gói cấp 2

barcode

QR

chuỗi dữ liệu phục vụ internal trace

chuỗi dữ liệu phục vụ public trace

chuỗi dữ liệu phục vụ recall

### 0.2. Vai trò tài liệu

Tài liệu này là mắt xích nối giữa:

phiếu nhập nguyên liệu đầu vào

phiếu chấp thuận nguyên liệu đưa vào sản xuất

lệnh sản xuất

quy tắc in mã 2 giai đoạn

Operational Config Map

Recall engine

### 0.3. Ranh giới tài liệu

Tài liệu này không giữ:

công thức chi tiết từng SKU

giá kế toán

KPI / thưởng / thuế

UI code

API code

Tài liệu này chỉ giữ chuỗi dữ liệu truy vết và map đơn vị truy vết.

### 0.4. Nguyên tắc nền

nguyên liệu đầu vào phải có lô nguyên liệu

lệnh sản xuất phải có mẻ sản xuất

thành phẩm phải có batch / lô sản xuất

cấp đóng gói 2 phải có barcode / QR

dữ liệu truy vết phải lần ngược và lần xuôi được

thu hồi phải reuse chính chuỗi này, không dựng chuỗi song song

## 1. KẾT LUẬN KHÓA

### 1.1. Kết luận tổng quát

Chuỗi truy vết chuẩn của Ginsengfood phải đi theo tuyến:

Lot nguyên liệu → Phiếu chấp thuận nguyên liệu đưa vào sản xuất → Lệnh sản xuất / mẻ sản xuất → Batch thành phẩm → Đóng gói cấp 1 → Đóng gói cấp 2 → Barcode / QR → Kho thành phẩm / giao hàng / truy vết / recall

### 1.2. Cấp truy vết chính

Cấp truy vết chính cho khách hàng và truy xuất thương mại nên là:

đơn vị đóng gói cấp 2 ví dụ: hộp

### 1.3. Cấp truy vết nội bộ

Cấp truy vết nội bộ phải đi sâu được tới:

batch thành phẩm

mẻ sản xuất

lô nguyên liệu đầu vào

### 1.4. Cấp thu hồi

Recall phải khóa được tối thiểu ở các cấp:

batch

hộp

thùng

Tùy tình huống có thể mở rộng sâu hơn.

## 2. ĐỊNH NGHĨA CÁC ĐƠN VỊ TRONG CHUỖI

### 2.1. Lot nguyên liệu

Là lô nguyên liệu đầu vào nhập kho.

Mỗi lot nguyên liệu phải gắn với:

mã nguyên liệu

nhà cung cấp

ngày nhập

kết quả đánh giá đầu vào

vị trí kho

mã lô nguyên liệu

### 2.2. Mẻ sản xuất

Là một lần vận hành sản xuất theo một lệnh sản xuất và một công thức/version cụ thể.

Mỗi mẻ sản xuất phải gắn với:

mã hồ sơ sản xuất

mã lệnh sản xuất

SKU

công thức

version công thức

thời gian sản xuất

nhân sự / ca sản xuất

### 2.3. Batch thành phẩm

Là lô thành phẩm sinh ra từ một mẻ sản xuất.

Mỗi batch thành phẩm phải gắn với:

mã batch

mẻ sản xuất nguồn

SKU

ngày sản xuất

hạn dùng

trạng thái QC / release

### 2.4. Đơn vị đóng gói cấp 1

Là đơn vị nhỏ trực tiếp chứa sản phẩm, ví dụ:

gói

lọ

hũ

### 2.5. Đơn vị đóng gói cấp 2

Là đơn vị truy vết chính, ví dụ:

hộp

thùng

hoặc lọ / hũ nếu một SKU không có lớp đóng gói ngoài

### 2.6. Barcode

Là mã vạch dùng cho:

kho

bán hàng

kiểm soát đóng gói

đối soát đơn vị sản phẩm

### 2.7. QR

Là mã QR dùng cho:

public trace

internal trace

recall linkage

xác thực lô / batch nếu cần

## 3. CHUỖI DỮ LIỆU TRUY VẾT BẮT BUỘC

### 3.1. Từ nguyên liệu vào sản xuất

Mỗi lot nguyên liệu phải truy được tới:

phiếu nhập nguyên liệu đầu vào

phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất

lệnh sản xuất / mẻ sản xuất đã dùng lot đó

Quy tắc

Một nguyên liệu không chỉ “có trong kho”, mà phải biết:

lô nào

đã được duyệt vào mẻ nào

### 3.2. Từ mẻ sản xuất sang batch thành phẩm

Mỗi mẻ sản xuất phải sinh ra hoặc gắn với:

một hoặc nhiều batch thành phẩm

Quy tắc

Mỗi batch thành phẩm phải truy ngược được về:

mẻ sản xuất nào

lệnh sản xuất nào

công thức version nào

các lô nguyên liệu nào đã đi vào

### 3.3. Từ batch sang đơn vị đóng gói cấp 1

Mỗi đơn vị cấp 1 phải biết:

thuộc batch nào

Quy tắc

Nếu không truy từng đơn vị cấp 1 chi tiết, ít nhất vẫn phải map được:

số lượng cấp 1 sinh ra từ batch đó

### 3.4. Từ cấp 1 sang cấp 2

Mỗi hộp / thùng phải biết:

chứa đơn vị cấp 1 nào

hoặc ít nhất chứa số lượng cấp 1 thuộc batch nào

Quy tắc

Cấp 2 là đơn vị truy vết chính, nên bắt buộc phải gắn được với batch.

### 3.5. Từ cấp 2 sang barcode / QR

Mỗi đơn vị cấp 2 phải có:

barcode

QR

ngày sản xuất

hạn dùng

batch / lô sản xuất

Quy tắc

Không được có barcode / QR “trôi nổi” không map được về batch.

## 4. BẢN ĐỒ LIÊN KẾT BẮT BUỘC

### 4.1. Lot nguyên liệu → Mẻ sản xuất

| Trường | Bắt buộc |
| --- | --- |
| lot_material_code | Có |
| raw_material_code | Có |
| supplier_id | Có |
| production_order_id | Có |
| production_batch_id / production_run_id | Có |

### 4.2. Mẻ sản xuất → Batch thành phẩm

| Trường | Bắt buộc |
| --- | --- |
| production_batch_id / run_id | Có |
| finished_batch_code | Có |
| sku_code | Có |
| formula_code | Có |
| formula_version | Có |

### 4.3. Batch thành phẩm → Đóng gói cấp 1

| Trường | Bắt buộc |
| --- | --- |
| finished_batch_code | Có |
| pack_level_1_unit_type | Có |
| pack_level_1_qty | Có |

### 4.4. Batch thành phẩm / cấp 1 → Đóng gói cấp 2

| Trường | Bắt buộc |
| --- | --- |
| finished_batch_code | Có |
| pack_level_2_unit_type | Có |
| box_code / case_code nếu có | Nên có |
| pack_level_2_qty | Có |

### 4.5. Cấp 2 → Barcode / QR

| Trường | Bắt buộc |
| --- | --- |
| package_level_2_id | Có |
| barcode_value | Có |
| qr_token | Có |
| mfg_date | Có |
| exp_date | Có |
| batch_code | Có |

## 5. QUY TẮC IN MÃ VÀ SINH MÃ TRUY VẾT

### 5.1. Đóng gói cấp 1

Cấp 1 chỉ in:

ngày sản xuất

hạn dùng

Quy tắc

Cấp 1 không phải cấp truy vết chính cho khách hàng.

### 5.2. Đóng gói cấp 2

Cấp 2 phải in:

lô sản xuất / batch

ngày sản xuất

hạn dùng

barcode

QR

Quy tắc

Đây là cấp truy vết chính.

### 5.3. Sinh mã thời gian thực

Khi line đóng gói cấp 2 đang chạy, hệ thống phải tự sinh theo thời gian thực:

batch / lot

MFG

EXP

barcode

QR

Cấm

không cho máy in tự sinh mã nghiệp vụ

không cho người vận hành nhập tay mã lô / QR / barcode trong luồng chuẩn

## 6. INTERNAL TRACE VÀ PUBLIC TRACE

### 6.1. Internal trace

Internal trace phải tra được:

đơn vị cấp 2

batch thành phẩm

mẻ sản xuất

version công thức

các lot nguyên liệu đầu vào

nhà cung cấp nếu cần

nhân sự / công đoạn nếu cần

### 6.2. Public trace

Public trace chỉ nên hiển thị:

SKU / tên sản phẩm

batch / lô

ngày sản xuất

hạn dùng

trạng thái xác thực

thông tin minh bạch được phép công bố

Cấm

Không đưa lên public trace:

giá

nhân sự nội bộ

chi tiết hao hụt

nhà cung cấp nhạy cảm

dữ liệu kế toán

## 7. QUY TẮC THU HỒI (RECALL)

### 7.1. Recall phải dùng chính chuỗi trace này

Không được dựng chuỗi thu hồi riêng.

Recall phải lần ngược được:

QR / barcode

hộp / thùng

batch thành phẩm

mẻ sản xuất

lot nguyên liệu

### 7.2. Cấp khóa recall tối thiểu

Phải khóa được ít nhất:

theo batch

theo hộp

theo thùng

### 7.3. Điều kiện recall

Khi có recall, hệ thống phải xác định được:

những đơn vị cấp 2 nào bị ảnh hưởng

batch nào bị ảnh hưởng

mẻ nào bị ảnh hưởng

nguyên liệu / lot nào liên quan

## 8. QUY TẮC KHO VÀ NHẬP KHO THÀNH PHẨM

### 8.1. Nhập kho thành phẩm chỉ sau QC đạt

Đơn vị nhập kho thành phẩm phải gắn được với:

batch

đơn vị cấp 2

barcode / QR nếu đã sinh

### 8.2. Đơn vị kho chính

Đơn vị kho thành phẩm nên theo:

hộp

thùng

lọ

hũ

tùy loại sản phẩm

### 8.3. Quy tắc quét mã nhập kho

Khi nhập kho thành phẩm, hệ thống phải có khả năng:

quét barcode / QR

xác định batch tương ứng

ghi nhận vị trí kho

## 9. KHỐI DỮ LIỆU BẮT BUỘC CHO DEV

### 9.1. Lot Material Registry

Mỗi lot nguyên liệu phải có:

lot_id

raw_material_code

supplier_id

received_at

qc_status

warehouse_location

### 9.2. Production Run / Batch Registry

Mỗi mẻ / batch phải có:

production_run_id

production_order_id

sku_code

formula_code

formula_version

finished_batch_code

mfg_date

exp_date

### 9.3. Packaging Registry

Mỗi đơn vị cấp 2 phải có:

package_level_2_id

finished_batch_code

packaging_type

barcode_value

qr_token

mfg_date

exp_date

### 9.4. Trace Registry

Phải có registry để map:

qr_token

barcode_value

package_level_2_id

finished_batch_code

production_run_id

lot_material_ids liên quan

## 10. QUY TẮC CỨNG

### 10.1. Không có batch thì không có QR cấp 2

### 10.2. Không có lot nguyên liệu thì batch không được coi là trace-complete

### 10.3. Không có map cấp 2 → batch thì không được public trace

### 10.4. Không có trace chain hoàn chỉnh thì không được coi là recall-ready

### 10.5. Không cho barcode / QR tồn tại mà không truy ngược được về batch

## 11. PASS / FAIL GATE

### 11.1. Đạt khi

truy được từ lot nguyên liệu → mẻ → batch → cấp 2 → QR

truy được từ QR → cấp 2 → batch → mẻ → lot nguyên liệu

public trace và internal trace tách rõ

recall dùng được chính chuỗi này

nhập kho thành phẩm gắn được batch và đơn vị cấp 2

### 11.2. Chưa đạt khi

lot nguyên liệu không nối được với mẻ

batch không nối được với cấp đóng gói

QR không nối được với batch

recall phải dựng chuỗi riêng

public trace lẫn dữ liệu nội bộ

## 12. KẾT LUẬN

Từ đây, TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP - GINSENGFOOD được khóa ở mức:

có lot nguyên liệu

có mẻ sản xuất

có batch thành phẩm

có map sang cấp đóng gói

có barcode / QR cấp 2

có internal trace

có public trace

recall reuse chính chuỗi này

Đây là nền để đi tiếp sang:

## BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH

bản “BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH - GINSENGFOOD” theo format sạch, Word-ready.

Bản này bám đúng các nguyên tắc đã khóa:

danh mục nguyên liệu đầu vào quản lý theo 2 nhóm: Sâm và dược liệu / Nguyên liệu

lệnh sản xuất và phiếu cấp nguyên liệu cho sản xuất phải tự hiện công thức, không cho chọn lại nguyên liệu tay

phiếu nhập nguyên liệu đầu vào là nơi click chọn nguyên liệu từ danh mục

đơn vị theo công đoạn đã chốt:

nhập / cấp phát: kg / lít / ml

sơ chế: khay / kg / lít

sấy: khay + % độ ẩm

đóng gói cấp 1: gói / lọ / hũ

đóng gói cấp 2: hộp / thùng / lọ / hũ

đóng gói cấp 2 là nơi hệ thống tự sinh dữ liệu in theo thời gian thực

MISA dùng một ngõ tích hợp chung cho toàn hệ, không điều khiển logic nhà máy hay logic KPI gốc

## BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH - GINSENGFOOD

## 0. MỞ ĐẦU TÀI LIỆU

### 0.1. Mục đích tài liệu

Tài liệu này dùng để khóa lớp đặc tả thực thi cho dev và QA, gồm:

màn hình nào phải có

route nào phải có

action nào phải có

trường nào tự hiện / click chọn / nhập tay

điều kiện chuyển bước

trạng thái của từng phiếu

quyền thao tác của từng vai trò

### 0.2. Vai trò tài liệu

Tài liệu này là nguồn để:

dev backend dựng route / controller / service / trạng thái

dev frontend dựng form / bảng / nút / modal / khối xác nhận

QA test đúng luồng

vận hành hiểu form sẽ dùng như thế nào

### 0.3. Ranh giới tài liệu

Tài liệu này không thay:

bộ phiếu mẫu vận hành

BOM / Recipe Map

Operational Config Map

Trace Data Chain

MISA Integration Rule

Tài liệu này chỉ đặc tả:

màn hình

route

action

trạng thái

quyền thao tác

## 1. QUY TẮC CHUNG CHO TẤT CẢ MÀN HÌNH

### 1.1. Phân loại trường dữ liệu

Mỗi màn hình phải chia trường thành 3 loại rõ ràng:

#### 1.1.1. Trường tự hiện

Hệ thống tự sinh / tự kéo, không cho gõ tay.

#### 1.1.2. Trường click chọn

Chọn từ danh mục master, không gõ tự do.

#### 1.1.3. Trường phải nhập

Dùng để nhập số liệu thực tế, đánh giá, ghi chú.

### 1.2. Quy tắc sử dụng làm nền dữ liệu

Mọi phiếu sau phải tự sử dụng làm nền dữ liệu từ:

hồ sơ sản xuất gốc

phiếu trước đó trong chuỗi

Không cho người dùng lập lại từ đầu.

### 1.3. Quy tắc action

Mỗi action phải có đủ:

người được bấm

điều kiện được bấm

trạng thái trước khi bấm

trạng thái sau khi bấm

log người bấm

thời gian bấm

### 1.4. Quy tắc xác nhận

Mọi phiếu phải có khối xác nhận theo từng cá nhân:

Vai trò

Họ tên

Bộ phận

Trạng thái xác nhận

Thời gian xác nhận

Ghi chú

### 1.5. Quy tắc trạng thái

Mỗi phiếu phải có tối thiểu bộ trạng thái:

DRAFT

## SUBMITTED

## APPROVED / REJECTED

## COMPLETED

CANCELED

Có thể tinh chỉnh theo từng phiếu.

## 2. MÀN HÌNH 01 - PHIẾU NHẬP NGUYÊN LIỆU ĐẦU VÀO

### 2.1. Route màn hình

GET /admin/operational/raw-material-inbound

GET /admin/operational/raw-material-inbound/{id}

GET /admin/operational/raw-material-inbound/create

### 2.2. Route API / action

POST /api/admin/operational/raw-material-inbound

POST /api/admin/operational/raw-material-inbound/{id}/submit

POST /api/admin/operational/raw-material-inbound/{id}/approve

POST /api/admin/operational/raw-material-inbound/{id}/reject

POST /api/admin/operational/raw-material-inbound/{id}/post-accounting

POST /api/admin/operational/raw-material-inbound/{id}/attach-evidence

### 2.3. Trường tự hiện

Mã phiếu

Ngày lập mặc định

Người lập mặc định

Bộ phận tiếp nhận mặc định nếu policy có

### 2.4. Trường click chọn

Nhà cung cấp

Nguyên liệu

Nhóm nguyên liệu

Người giao hàng nếu có danh mục

Kho nhập

Vị trí kho

Nhân sự QC

Kế toán kho / vật tư

### 2.5. Trường nhập tay

Số lượng giao

Quy cách

Tình trạng ban đầu

Kết quả đánh giá

Lý do không đạt

Đề xuất xử lý

Đơn giá nhập

Ghi chú

### 2.6. Bảng chi tiết

Cột bắt buộc:

| Cột |
| --- |
| Mã nguyên liệu |
| Tên nguyên liệu |
| Nhóm nguyên liệu |
| Chủng loại / mô tả |
| Số lượng giao |
| Đơn vị |
| Quy cách |
| Tình trạng ban đầu |
| Kết quả đánh giá |
| Lý do không đạt |
| Đề xuất xử lý |
| Đơn giá nhập |
| Thành tiền |
| Ghi chú |

### 2.7. Action chính

Lưu nháp

Gửi duyệt

Duyệt nhập kho

Từ chối

Ghi nhận kế toán

Đính ảnh / video

### 2.8. Trạng thái

DRAFT

## SUBMITTED

## PARTIALLY_APPROVED

APPROVED

REJECTED

## ACCOUNTED

### 2.9. Vai trò được thao tác

Kho

QC đầu vào

Kế toán kho / vật tư

Quản lý liên quan

## 3. MÀN HÌNH 02 - LỆNH SẢN XUẤT

### 3.1. Route màn hình

GET /admin/operational/production-orders

GET /admin/operational/production-orders/{id}

GET /admin/operational/production-orders/create

### 3.2. Route API / action

POST /api/admin/operational/production-orders

POST /api/admin/operational/production-orders/{id}/open

POST /api/admin/operational/production-orders/{id}/cancel

POST /api/admin/operational/production-orders/{id}/close

### 3.3. Trường tự hiện

Mã hồ sơ sản xuất gốc

Mã lệnh sản xuất

Tên sản phẩm

Mã công thức

Version công thức

Nhóm sản phẩm

Phân loại Vegan / Mặn

Danh sách nguyên liệu theo công thức

Hàm lượng

Đơn vị

### 3.4. Trường click chọn

SKU

Ca sản xuất

Bộ phận sản xuất

Quản lý nhà máy

Loại lệnh:

Pilot

Sản xuất thường

### 3.5. Trường nhập tay

Số lượng kế hoạch

Ghi chú lệnh

### 3.6. Bảng công thức tự hiện

Phần 1 - Dược liệu

Phần 2 - Nguyên liệu

Cấm chọn tay nguyên liệu.

### 3.7. Action chính

Lưu nháp

Mở lệnh

Hủy lệnh

Đóng lệnh

### 3.8. Trạng thái

DRAFT

OPEN

CANCELED

CLOSED

### 3.9. Vai trò được thao tác

Kế hoạch sản xuất

Quản lý nhà máy

Bộ phận sản xuất

## 4. MÀN HÌNH 03 - PHIẾU ĐỀ NGHỊ CẤP NGUYÊN LIỆU CHO SẢN XUẤT

### 4.1. Route màn hình

GET /admin/operational/material-issue-requests

GET /admin/operational/material-issue-requests/{id}

### 4.2. Route API / action

POST /api/admin/operational/material-issue-requests/generate-from-order

POST /api/admin/operational/material-issue-requests/{id}/submit

POST /api/admin/operational/material-issue-requests/{id}/approve

POST /api/admin/operational/material-issue-requests/{id}/reject

### 4.3. Trường tự hiện

Hồ sơ sản xuất

Lệnh sản xuất

SKU

Tên sản phẩm

Công thức

Version

Danh sách nguyên liệu

Hàm lượng

Đơn vị

Giá trị tham chiếu kế toán

### 4.4. Trường click chọn

Kho xuất

Người đề nghị

Quản lý nhà máy

### 4.5. Trường nhập tay

Ghi chú

### 4.6. Bảng chi tiết

| Cột |
| --- |
| Mã nguyên liệu |
| Tên nguyên liệu |
| Nhóm nguyên liệu |
| Hàm lượng công thức |
| Đơn vị |
| Số lượng yêu cầu cấp |
| Đơn giá bình quân hiện hành |
| Giá trị dự kiến xuất |
| Ghi chú |

### 4.7. Action chính

Tự sinh từ lệnh sản xuất

Gửi duyệt

Duyệt

Từ chối

### 4.8. Trạng thái

## GENERATED

## SUBMITTED

APPROVED

REJECTED

### 4.9. Vai trò được thao tác

Bộ phận sản xuất

Kho

Quản lý nhà máy

## 5. MÀN HÌNH 04 - PHIẾU CHẤP THUẬN NGUYÊN LIỆU ĐƯỢC PHÉP ĐƯA VÀO SẢN XUẤT

### 5.1. Route màn hình

GET /admin/operational/material-issue-approvals

GET /admin/operational/material-issue-approvals/{id}

### 5.2. Route API / action

POST /api/admin/operational/material-issue-approvals/generate-from-request

POST /api/admin/operational/material-issue-approvals/{id}/approve

POST /api/admin/operational/material-issue-approvals/{id}/reject

POST /api/admin/operational/material-issue-approvals/{id}/post-accounting

### 5.3. Trường tự hiện

Hồ sơ sản xuất

Lệnh sản xuất

SKU

Tên sản phẩm

Công thức

Version

Danh sách nguyên liệu đề nghị cấp

Giá trị hạch toán dự kiến

### 5.4. Trường click chọn

Lô nguyên liệu cấp cho từng dòng

Người phê duyệt theo vai trò

### 5.5. Trường nhập tay

Số lượng duyệt thực tế nếu có chênh lệch

Ghi chú

Lý do từ chối

### 5.6. Bảng chi tiết

| Cột |
| --- |
| Mã nguyên liệu |
| Tên nguyên liệu |
| Lô nguyên liệu |
| Số lượng duyệt xuất |
| Đơn vị |
| Kết quả chấp thuận |
| Đơn giá bình quân |
| Thành tiền xuất |
| Hao hụt cấp phát |
| Giá trị hao hụt |
| Ghi chú |

### 5.7. Action chính

Duyệt

Từ chối

Ghi nhận kế toán

### 5.8. Trạng thái

## GENERATED

APPROVED

REJECTED

## ACCOUNTED

### 5.9. Vai trò được thao tác

Quản lý nhà máy

Kho

QC

Kế toán vật tư / kho

## 6. MÀN HÌNH 05 - CHECK-IN / CHECK-OUT NHÂN SỰ THEO LỆNH / MẺ / CÔNG ĐOẠN

### 6.1. Route màn hình

GET /admin/operational/workforce-checkin

GET /admin/operational/workforce-checkin/{id}

### 6.2. Route API / action

POST /api/admin/operational/workforce-checkin

POST /api/admin/operational/workforce-checkin/bulk-checkin

POST /api/admin/operational/workforce-checkin/bulk-checkout

POST /api/admin/operational/workforce-checkin/{id}/confirm

### 6.3. Trường tự hiện

Lệnh sản xuất

Hồ sơ sản xuất

Mẻ sản xuất

SKU

Tên sản phẩm

Công đoạn

### 6.4. Trường click chọn

Nhân sự

Bộ phận

Chức danh

Công đoạn

Ca sản xuất

### 6.5. Trường nhập tay

Ghi chú

đơn giá công nếu chưa tự kéo được

### 6.6. Bảng chi tiết

| Cột |
| --- |
| Mã nhân sự |
| Họ tên |
| Bộ phận |
| Chức danh |
| Đơn giá công / giờ |
| Check-in |
| Check-out |
| Tổng giờ làm |
| Chi phí nhân công phân bổ |
| Ghi chú |

### 6.7. Action chính

Check-in

Check-out

Check-in nhiều người

Check-out nhiều người

Xác nhận hoàn tất công đoạn

### 6.8. Trạng thái

OPEN

## IN_PROGRESS

## CHECKED_OUT

## CONFIRMED

### 6.9. Vai trò được thao tác

Tổ trưởng / phụ trách công đoạn

Quản lý bộ phận

## 7. MÀN HÌNH 06 - PHIẾU THEO DÕI SƠ CHẾ

### 7.1. Route màn hình

GET /admin/operational/preprocess-tracking

GET /admin/operational/preprocess-tracking/{id}

### 7.2. Route API / action

POST /api/admin/operational/preprocess-tracking/generate

POST /api/admin/operational/preprocess-tracking/{id}/submit

POST /api/admin/operational/preprocess-tracking/{id}/approve

POST /api/admin/operational/preprocess-tracking/{id}/attach-evidence

### 7.3. Trường tự hiện

Hồ sơ sản xuất

Lệnh sản xuất

SKU

Tên sản phẩm

Mẻ sản xuất

Danh sách nguyên liệu đã được duyệt vào sản xuất

### 7.4. Trường click chọn

Người thực hiện

Tổ trưởng / phụ trách công đoạn

Quản lý bộ phận

### 7.5. Trường nhập tay

Số lượng đầu vào

Số lượng sau sơ chế

Hao hụt

Đánh giá

Ghi chú

### 7.6. Bảng chi tiết

| Cột |
| --- |
| Loại bán thành phẩm / lô sơ chế |
| Số lượng đầu vào |
| Đơn vị |
| Số lượng sau sơ chế |
| Đơn vị |
| Hao hụt |
| Đơn vị |
| Đánh giá |
| Ghi chú |

Đơn vị được phép: khay / kg / lít

### 7.7. Action chính

Lưu nháp

Gửi duyệt

Duyệt

Đính bằng chứng

### 7.8. Trạng thái

## GENERATED

DRAFT

## SUBMITTED

APPROVED

REJECTED

### 7.9. Vai trò được thao tác

Bộ phận sơ chế

Tổ trưởng

Quản lý bộ phận

## 8. MÀN HÌNH 07 - PHIẾU KIỂM CHẤT LƯỢNG SAU SẤY THĂNG HOA

### 8.1. Route màn hình

GET /admin/operational/freeze-dry-qc

GET /admin/operational/freeze-dry-qc/{id}

### 8.2. Route API / action

POST /api/admin/operational/freeze-dry-qc/generate

POST /api/admin/operational/freeze-dry-qc/{id}/submit

POST /api/admin/operational/freeze-dry-qc/{id}/approve

POST /api/admin/operational/freeze-dry-qc/{id}/reject

### 8.3. Trường tự hiện

Hồ sơ sản xuất

Lệnh sản xuất

SKU

Tên sản phẩm

Công thức

Version

Mẻ sản xuất

### 8.4. Trường click chọn

QC

Quản lý nhà máy

Bộ phận phụ trách công đoạn

### 8.5. Trường nhập tay

số lượng khay kiểm tra

% độ ẩm

đánh giá cảm quan

ghi chú

quyết định đạt / không đạt

### 8.6. Bảng chỉ tiêu

| Cột |
| --- |
| Chỉ tiêu |
| Kết quả |
| Đơn vị |
| Đánh giá |
| Ghi chú |

Bắt buộc có: % độ ẩm

### 8.7. Action chính

Lưu nháp

Gửi duyệt

Duyệt cho đóng gói

Từ chối / chờ xử lý

### 8.8. Trạng thái

## GENERATED

DRAFT

## SUBMITTED

## APPROVED_FOR_PACKING

REJECTED

ON_HOLD

### 8.9. Vai trò được thao tác

QC

Quản lý nhà máy

Bộ phận phụ trách công đoạn

## 9. MÀN HÌNH 08 - PHIẾU THEO DÕI ĐÓNG GÓI CẤP 1

### 9.1. Route màn hình

GET /admin/operational/packing-level-1

GET /admin/operational/packing-level-1/{id}

### 9.2. Route API / action

POST /api/admin/operational/packing-level-1/generate

POST /api/admin/operational/packing-level-1/{id}/start

POST /api/admin/operational/packing-level-1/{id}/complete

POST /api/admin/operational/packing-level-1/{id}/attach-evidence

### 9.3. Trường tự hiện

Hồ sơ sản xuất

Lệnh sản xuất

SKU

Tên sản phẩm

Mẻ sản xuất

Đơn vị cấp 1

rule in cấp 1

### 9.4. Trường click chọn

Người thực hiện

Tổ trưởng

Quản lý bộ phận

Máy in cấp 1 nếu có

### 9.5. Trường nhập tay

số lượng đầu vào

số lượng đạt

số lượng không đạt

lý do không đạt

ghi chú

### 9.6. Bảng chi tiết

| Cột |
| --- |
| Số lượng đầu vào |
| Đơn vị |
| Số lượng đạt |
| Đơn vị |
| Số lượng không đạt |
| Đơn vị |
| Lý do không đạt |
| Ghi chú |

Đơn vị được phép: gói / lọ / hũ

### 9.7. Action chính

Bắt đầu đóng gói

Hoàn tất đóng gói

Đính ảnh / video

### 9.8. Trạng thái

## GENERATED

READY

## IN_PROGRESS

## COMPLETED

ON_HOLD

### 9.9. Vai trò được thao tác

Bộ phận đóng gói cấp 1

Tổ trưởng

Quản lý bộ phận

## 10. MÀN HÌNH 09 - PHIẾU THEO DÕI ĐÓNG GÓI CẤP 2

### 10.1. Route màn hình

GET /admin/operational/packing-level-2

GET /admin/operational/packing-level-2/{id}

### 10.2. Route API / action

POST /api/admin/operational/packing-level-2/generate

POST /api/admin/operational/packing-level-2/{id}/start

POST /api/admin/operational/packing-level-2/{id}/print-start

POST /api/admin/operational/packing-level-2/{id}/print-reprint

POST /api/admin/operational/packing-level-2/{id}/complete

### 10.3. Trường tự hiện

Hồ sơ sản xuất

Lệnh sản xuất

SKU

Tên sản phẩm

Mẻ sản xuất

Đơn vị cấp 2

batch / lô

nội dung in cấp 2

barcode / QR rule

### 10.4. Trường click chọn

Người thực hiện

Tổ trưởng

Quản lý bộ phận

Máy in cấp 2

### 10.5. Trường nhập tay

số lượng đầu vào

số lượng đầu ra

số lượng không đạt

lý do không đạt

ghi chú

### 10.6. Bảng chi tiết

| Cột |
| --- |
| Số lượng đầu vào |
| Đơn vị |
| Số lượng đầu ra |
| Đơn vị |
| Số lượng không đạt |
| Đơn vị |
| Lý do không đạt |
| Ghi chú |

Đơn vị được phép: hộp / thùng / lọ / hũ

### 10.7. Action chính

Bắt đầu đóng gói cấp 2

Bắt đầu in cấp 2

Reprint có kiểm soát

Hoàn tất đóng gói cấp 2

### 10.8. Trạng thái

## GENERATED

READY

## IN_PROGRESS

PRINTING

## COMPLETED

## PRINT_ERROR

ON_HOLD

### 10.9. Vai trò được thao tác

Bộ phận đóng gói cấp 2

Tổ trưởng

Quản lý bộ phận

người được quyền reprint

## 11. MÀN HÌNH 10 - PHIẾU QC THÀNH PHẨM

### 11.1. Route màn hình

GET /admin/operational/finished-goods-qc

GET /admin/operational/finished-goods-qc/{id}

### 11.2. Route API / action

POST /api/admin/operational/finished-goods-qc/generate

POST /api/admin/operational/finished-goods-qc/{id}/submit

POST /api/admin/operational/finished-goods-qc/{id}/approve

POST /api/admin/operational/finished-goods-qc/{id}/reject

### 11.3. Trường tự hiện

Hồ sơ sản xuất

Lệnh sản xuất

SKU

Tên sản phẩm

Mẻ sản xuất

rule QC thành phẩm

đơn vị đóng gói liên quan

### 11.4. Trường click chọn

QC

Quản lý nhà máy

Bộ phận liên quan

### 11.5. Trường nhập tay

số lượng kiểm tra

số lượng đạt

số lượng không đạt

tỷ lệ đạt

ghi chú

### 11.6. Bảng chi tiết

| Cột |
| --- |
| Số lượng kiểm tra |
| Đơn vị |
| Số lượng đạt |
| Đơn vị |
| Số lượng không đạt |
| Đơn vị |
| Tỷ lệ đạt |
| Ghi chú |

Đơn vị được phép: gói / lọ / hũ / hộp / thùng

### 11.7. Action chính

Lưu nháp

Gửi duyệt

Duyệt nhập kho

Từ chối / chờ xử lý

### 11.8. Trạng thái

## GENERATED

DRAFT

## SUBMITTED

## APPROVED_FOR_WAREHOUSE

REJECTED

ON_HOLD

### 11.9. Vai trò được thao tác

QC

Quản lý nhà máy

Bộ phận liên quan

## 12. MÀN HÌNH 11 - LỆNH NHẬP KHO THÀNH PHẨM

### 12.1. Route màn hình

GET /admin/operational/finished-goods-receipt

GET /admin/operational/finished-goods-receipt/{id}

### 12.2. Route API / action

POST /api/admin/operational/finished-goods-receipt/generate

POST /api/admin/operational/finished-goods-receipt/{id}/scan

POST /api/admin/operational/finished-goods-receipt/{id}/confirm

POST /api/admin/operational/finished-goods-receipt/{id}/post-accounting

### 12.3. Trường tự hiện

Hồ sơ sản xuất

Lệnh sản xuất

SKU

Tên sản phẩm

số lượng được phép nhập kho từ QC

batch / lô

barcode / QR nếu có

kho nhập gợi ý nếu policy có

### 12.4. Trường click chọn

Kho nhập

Vị trí kho

Người nhập

QC

Quản lý nhà máy

### 12.5. Trường nhập tay

số lượng nhập thực tế

ghi chú

### 12.6. Bảng chi tiết

| Cột |
| --- |
| Số lượng nhập |
| Đơn vị |
| Ghi chú |

Đơn vị được phép: hộp / thùng / lọ / hũ

### 12.7. Action chính

Quét mã nhập kho

Xác nhận nhập kho

Ghi nhận kế toán nếu có

### 12.8. Trạng thái

## GENERATED

## READY_TO_RECEIVE

SCANNED

RECEIVED

## ACCOUNTED

### 12.9. Vai trò được thao tác

Kho

QC

Quản lý nhà máy

Kế toán kho nếu có ngõ kế toán

## 13. MÀN HÌNH 12 - PHIẾU KẾ TOÁN XUẤT NGUYÊN LIỆU CHO SẢN XUẤT

### 13.1. Route màn hình

GET /admin/operational/material-issue-accounting

GET /admin/operational/material-issue-accounting/{id}

### 13.2. Route API / action

POST /api/admin/operational/material-issue-accounting/generate

POST /api/admin/operational/material-issue-accounting/{id}/post

POST /api/admin/operational/material-issue-accounting/{id}/sync-misa

### 13.3. Trường tự hiện

Hồ sơ sản xuất

Lệnh sản xuất

SKU

Tên sản phẩm

mẻ sản xuất

danh sách nguyên liệu đã duyệt cấp phát

đơn giá bình quân

thành tiền xuất

hao hụt

### 13.4. Trường click chọn

Kế toán vật tư / kho

Kho

Quản lý nhà máy

### 13.5. Trường nhập tay

ghi chú kế toán

điều chỉnh được phép theo policy nếu có

### 13.6. Bảng chi tiết

| Cột |
| --- |
| Mã nguyên liệu |
| Tên nguyên liệu |
| Lô nguyên liệu |
| Số lượng xuất |
| Đơn vị |
| Đơn giá bình quân |
| Thành tiền xuất |
| Hao hụt |
| Giá trị hao hụt |
| Ghi chú |

Đơn vị được phép: kg / lít / ml

### 13.7. Action chính

Ghi nhận phiếu kế toán

Sync MISA

Retry sync

Xem trạng thái sync

### 13.8. Trạng thái

## GENERATED

POSTED

## SYNC_AWAITING_REVIEW

## SYNC_SUCCESS

## SYNC_ERROR

### 13.9. Vai trò được thao tác

Kế toán vật tư / kho

Kho

Quản lý nhà máy

người có quyền retry sync

## 14. QUY TẮC CỨNG CHO DEV

### 14.1. Phiếu nhập nguyên liệu = click chọn nguyên liệu từ danh mục

### 14.2. Lệnh sản xuất / cấp phát = tự hiện công thức, không cho chọn lại nguyên liệu

### 14.3. Mọi phiếu sau = tự sử dụng làm nền dữ liệu từ hồ sơ gốc

### 14.4. Mọi phiếu phải có khối xác nhận cá nhân

### 14.5. Mọi phiếu phải đúng đơn vị theo công đoạn

### 14.6. Đóng gói cấp 2 phải có action in / reprint gắn với dữ liệu in thời gian thực

### 14.7. Phiếu kế toán xuất nguyên liệu là màn riêng cho kế toán / sync MISA, không đưa logic giá ra form vận hành xưởng

## 15. PASS / FAIL GATE

### 15.1. Đạt khi

đủ 12 màn hình / route / action lõi

mỗi phiếu có trường tự hiện / click chọn / nhập tay rõ ràng

lệnh sản xuất kéo đúng công thức

phiếu cấp phát không cho chọn tay nguyên liệu

đơn vị theo công đoạn đúng

action in cấp 2 rõ

action sync MISA rõ

QA test được end-to-end

### 15.2. Chưa đạt khi

dev phải tự đoán form

phiếu không rõ trường nào auto / chọn / nhập

nguyên liệu sản xuất vẫn cho chọn tay

route / action không đủ

trạng thái không rõ

role thao tác không rõ

## 16. KẾT LUẬN

Từ đây, BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH - GINSENGFOOD đã đủ để làm nền kỹ thuật giao dev:

rõ màn hình

rõ route

rõ action

rõ trường

rõ trạng thái

rõ role thao tác

Đến đây, 5 bước bạn chốt đã đi đủ:

## 20 SKU ↔ NGUYÊN LIỆU / BOM MAP - GINSENGFOOD

## 20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD

## QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC

## TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP

## BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH
