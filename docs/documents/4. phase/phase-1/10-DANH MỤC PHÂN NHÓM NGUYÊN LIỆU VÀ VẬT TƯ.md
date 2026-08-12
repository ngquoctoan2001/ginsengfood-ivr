**DANH MỤC PHÂN NHÓM NGUYÊN LIỆU / VẬT TƯ SẢN XUẤT GINSENGFOOD**

Cấu trúc Nhóm A / Nhóm B + Công thức neo gạo + Ngưỡng tồn kho / Loại khỏi phiếu thu mua

Dùng cho PACK-03 MRP / Stock Alert / Material Master / Formula Scaling / Procurement Suppression Threshold

V1.2 CLEAN FINAL - Owner Lock - Rice Anchor Formula / Dynamic Threshold Locked

Nguồn dữ liệu: CÔNG THỨC VẬN HÀNH 20 SKU MỚI.docx; quy ước mẻ 400 = 7.200 gói/phần ăn = 1.800 hộp = 300 thùng. Bản này dùng làm danh mục nền để owner cấu hình công thức pilot theo gạo làm trung tâm, tỷ lệ nguyên liệu, sản lượng chuẩn, ngưỡng tồn, ngưỡng loại khỏi phiếu thu mua và hiệu chỉnh theo vận hành thực tế.

# 1\. Nguyên tắc phân nhóm tổng thể

| **Nhóm lớn** | **Nội dung**                        | **Nhóm con** | **+% / hao hụt**                        | **Ghi chú**                              |
| ------------ | ----------------------------------- | ------------ | --------------------------------------- | ---------------------------------------- |
| Nhóm A       | Nguyên liệu sản xuất                | A1-A3        | Cộng/trừ 5% hao hụt kế hoạch            | Dùng cho BOM/MRP/Material Issue          |
| Nhóm B       | Bao bì, vật tư đóng gói, vật tư phụ | B1-B3        | B1/B2 hao hụt 7%; B3 không tính hao hụt | Dùng cho Packaging/Print/MRP/Stock Alert |

Nguyên tắc khóa: Các tỷ lệ +5% và +7% là cấu hình kế hoạch ban đầu để lập MRP, không hardcode trong code. Khi vận hành thật, owner có quyền điều chỉnh theo SKU, vật tư, nhà cung cấp, mùa vụ hoặc tỷ lệ hao hụt thực tế, nhưng mọi thay đổi phải có audit/evidence/owner approval.

## 1.1. Ngưỡng tồn kho và ngưỡng loại khỏi phiếu thu mua

Hệ thống phải kiểm soát hai chiều: thiếu thì cảnh báo để lập nhu cầu nhập/mua/thu hoạch; đủ hoặc vượt định mức thì tự động loại khỏi phiếu thu mua/mua sắm. Phiếu thu mua không được chứa nguyên liệu/vật tư chỉ vì có yêu cầu nếu tồn khả dụng + incoming đã duyệt đã vượt ngưỡng kiểm soát.

| **Mã ngưỡng**             | **Áp dụng**                 | **Ngưỡng khóa**                     | **Công thức nội suy**                                                              | **Quy tắc xử lý**                                                                                                                                   |
| ------------------------- | --------------------------- | ----------------------------------- | ---------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| A_SPECIFIC_5_BATCH_SKU    | Nhóm A đặc thù theo SKU     | 5 mẻ/SKU                            | BOM_G1(material, SKU, mẻ 400) × 5                                                  | Dưới ngưỡng mới được đưa vào danh mục xét nhập/mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua.                                              |
| A_COMMON_20_BATCH_POOL    | Nhóm A dùng chung nhiều SKU | 20 mẻ chuẩn aggregate               | Σ BOM_G1(material, active SKU/planning basket, mẻ 400) trong 20 mẻ chuẩn           | Không tính 5 mẻ riêng cho từng SKU; tồn trên ngưỡng 20 mẻ thì loại khỏi phiếu thu mua.                                                              |
| SAVIGIN_STRATEGIC_RESERVE | Sâm Savigin Company Source  | Theo mùa vụ + tồn dự trữ chiến lược | Không là Purchase Requirement; harvest/intake/QC/lot/readiness theo chu kỳ thực tế | Đến kỳ thu hoạch vẫn cho harvest/sơ chế/nhập kho bảo quản; chỉ cảnh báo thiếu cho sản xuất khi ready stock dưới ngưỡng an toàn hoặc forecast thiếu. |
| B1_SKU_15_BATCH           | Bao bì cấp 1 theo SKU       | 15 mẻ/SKU                           | 7.704 gói quy đổi/mẻ × 15 = 115.560 gói quy đổi/SKU                                | Dưới ngưỡng mới được đưa vào danh mục xét mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua.                                                   |
| B2_BOX_SKU_15_BATCH       | Hộp cấp 2 theo SKU          | 15 mẻ/SKU                           | 1.926 hộp/mẻ × 15 = 28.890 hộp/SKU                                                 | Dưới ngưỡng mới được đưa vào danh mục xét mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua.                                                   |
| B2_CARTON_SKU_15_BATCH    | Thùng carton theo SKU       | 15 mẻ/SKU                           | 321 thùng/mẻ × 15 = 4.815 thùng/SKU                                                | Dưới ngưỡng mới được đưa vào danh mục xét mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua.                                                   |
| B3_ACTUAL_STOCK_POLICY    | Vật tư dùng chung B3        | Theo Stock Policy thực tế           | Không tự áp +% hao hụt; nếu có consumption_per_batch thì owner cấu hình riêng      | Không đưa vào phiếu mua nếu tồn đủ/vượt policy; trường hợp lớn cần Giám đốc phê duyệt.                                                              |

## 1.2. Quy tắc Giám đốc phê duyệt cho đơn hàng/kế hoạch sản xuất lớn

Trong trường hợp hệ thống nhận được kế hoạch sản xuất lớn, đơn hàng đại lý/nhà phân phối lớn hoặc chiến dịch đã được phê duyệt làm phát sinh nhu cầu vượt ngưỡng tồn thông thường, việc đưa nguyên liệu/vật tư đã vượt ngưỡng vào phiếu thu mua chỉ được thực hiện khi có Giám đốc phê duyệt.

- Phải có Production Plan hoặc Dealer/Distributor Order lớn đã được xác nhận.
- Phải có MRP run thể hiện nhu cầu thực tế theo SKU/BOM/Packaging Config.
- Phải có lý do vượt ngưỡng, evidence, audit và người phê duyệt.
- Không được dùng override để mua sắm tùy tiện hoặc bỏ qua tồn kho đang đủ/vượt định mức.
- Không được gọi Purchase Order chính thức nếu chưa qua Procurement Requirement, owner approval và gate liên quan.

## 1.3. Khóa cơ chế công thức pilot lấy gạo làm trung tâm

Trong giai đoạn chưa khóa công thức chuẩn cuối cùng, hệ thống phải cho phép owner/pilot nhập công thức thử nghiệm theo nguyên tắc lấy gạo làm nguyên liệu neo trung tâm. Khi pilot nhập tên nguyên liệu và số lượng theo kg, g, lít, ml hoặc gram, hệ thống phải tự chuẩn hóa đơn vị, tự tính tỷ lệ so với gạo, lưu snapshot thử nghiệm và cho phép thêm hoặc bỏ bớt nguyên liệu trong phạm vi pilot.

**Nguyên tắc khóa: gạo là anchor_ingredient. Tất cả nguyên liệu Nhóm A trong công thức pilot phải có ratio_to_rice hoặc usage_basis rõ ràng để khi thay đổi số kg gạo, hệ thống tự tính lại toàn bộ công thức.**

| **Thành phần kiểm soát** | **Quy tắc khóa**                                                          | **Cách hệ thống xử lý**                                                                             |
| ------------------------ | ------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| Anchor ingredient        | Gạo lúa-tôm / gạo trong công thức là nguyên liệu neo                      | Owner nhập anchor_rice_quantity; hệ thống dùng làm cơ sở tính tỷ lệ                                 |
| Nhập nguyên liệu pilot   | Cho phép nhập tên nguyên liệu, số lượng, đơn vị kg/g/l/ml                 | Chuẩn hóa material canonical và UOM trước khi tính                                                  |
| Tự tính tỷ lệ            | ratio_to_rice = standardized_quantity / standardized_rice_quantity × 100% | Hiển thị % theo gạo và lưu vào Pilot Formula Snapshot                                               |
| Nguyên liệu dạng lít/ml  | Không ép đổi sang kg nếu chưa có density config                           | Lưu theo lít/kg gạo hoặc ml/kg gạo; nếu cần quy đổi khối lượng phải qua density/UOM config          |
| Thêm/bỏ nguyên liệu      | Được phép trong giai đoạn pilot                                           | Ghi audit; sau khi công thức đã chuẩn hóa thì thêm/bỏ là thay đổi công thức và phải tạo version mới |
| Pilot thành công         | Chuyển thành công thức chuẩn được duyệt                                   | Lưu Formula Version, ratio_to_rice, unit_policy, rounding_policy, evidence và owner approval        |

## 1.4. Công thức chuẩn tự nhảy theo số kg gạo

Sau khi pilot được phê duyệt thành G1, bộ phận sản xuất không có quyền sửa công thức. Khi lập lệnh sản xuất, bộ phận sản xuất chỉ được nhập số kg gạo / batch anchor quantity trong phạm vi được phép; hệ thống tự scale toàn bộ nguyên liệu theo tỷ lệ đã khóa của G1 và tạo Production BOM Snapshot. Việc scale mẻ không phải sửa công thức. Mọi thay đổi về nguyên liệu, tỷ lệ, đơn vị hoặc cấu trúc công thức phải tạo Formula Version mới và được phê duyệt riêng.

| **Tình huống**        | **Cách tính**                                              | **Kết quả quản trị**                                                         |
| --------------------- | ---------------------------------------------------------- | ---------------------------------------------------------------------------- |
| Chỉ đổi số kg gạo     | calculated_quantity = anchor_rice_quantity × ratio_to_rice | Không tạo formula version mới; tạo Calculation Snapshot/BOM Snapshot theo mẻ |
| Đổi tỷ lệ nguyên liệu | ratio_to_rice thay đổi                                     | Bắt buộc tạo Formula Version mới, owner approval, evidence và audit          |
| Thêm nguyên liệu mới  | BOM line mới phát sinh                                     | Bắt buộc qua Material Master, Formula Version, QC/MRP/Stock Policy nếu cần   |
| Bỏ nguyên liệu cũ     | BOM line bị loại khỏi công thức                            | Bắt buộc tạo Formula Version mới; không sửa đè version đã dùng               |
| Đổi đơn vị tính       | kg/g/l/ml phải quy đổi qua UOM policy                      | Không cho tính nếu thiếu UOM conversion hoặc density policy khi bắt buộc     |
| Làm tròn sản xuất     | Áp dụng rounding_policy theo material                      | Lưu số trước/sau làm tròn trong calculation snapshot                         |

**Công thức sản xuất và buffer MRP phải tách riêng. Tỷ lệ +5% của Nhóm A là buffer kế hoạch/MRP để chuẩn bị và kiểm soát tồn kho, không được tự động sửa số lượng công thức sản xuất đã được phê duyệt.**

## 1.5. Dynamic Threshold Resolver cho Nhóm A theo công thức đã scale

Ngưỡng tồn kho và ngưỡng loại khỏi phiếu thu mua của Nhóm A không được cố định bằng số chết nếu công thức còn thay đổi theo anchor gạo. Khi công thức chuẩn đã scale theo số kg gạo/mẻ, hệ thống phải dùng số lượng nguyên liệu đã tính cho một mẻ làm cơ sở tính ngưỡng tồn.

| **Loại nguyên liệu Nhóm A**        | **Cơ sở tính động**                                               | **Ngưỡng loại khỏi phiếu thu mua**                                                                                                    |
| ---------------------------------- | ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Nguyên liệu đặc thù theo SKU       | calculated_material_quantity_per_batch(SKU, anchor_rice_quantity) | calculated_material_quantity_per_batch × 5 mẻ/SKU                                                                                     |
| Nguyên liệu dùng chung nhiều SKU   | Tổng calculated_material_quantity của active/planned SKU basket   | aggregate nhu cầu 20 mẻ chuẩn hoặc planning basket được owner duyệt                                                                   |
| Sâm Savigin                        | Company Source theo mùa vụ và strategic reserve stock             | Không đưa vào phiếu thu mua; harvest theo mùa vụ, chỉ cảnh báo thiếu khi READY_FOR_PRODUCTION dưới ngưỡng an toàn hoặc forecast thiếu |
| Nguyên liệu chưa có tỷ lệ hoặc UOM | Không tính được threshold                                         | BLOCKED_CONFIG_MISSING / OWNER_REVIEW_REQUIRED                                                                                        |

**Công thức khóa: nếu available_stock + approved_incoming >= dynamic_suppression_threshold thì nguyên liệu bị loại khỏi phiếu thu mua, trừ trường hợp kế hoạch sản xuất lớn được Giám đốc phê duyệt, có MRP run, evidence và audit.**

## 1.6. Dynamic Packaging Yield Resolver cho Nhóm B

Nhóm B không nhảy trực tiếp theo kg gạo, mà nhảy theo sản lượng chuẩn sau khi công thức/mẻ sản xuất đã được xác định. Khi chuẩn hóa được 1 mẻ sản xuất ra bao nhiêu gói, bao nhiêu hộp và bao nhiêu thùng, hệ thống phải tự tính nhu cầu B1/B2 và ngưỡng tồn tương ứng.

| **Nhóm B**           | **Cơ sở tính động**                                        | **Công thức MRP**                              | **Ngưỡng loại khỏi phiếu thu mua**                  |
| -------------------- | ---------------------------------------------------------- | ---------------------------------------------- | --------------------------------------------------- |
| B1 film theo SKU     | pack_output_per_batch                                      | pack_output_per_batch × (1 + B1_buffer_rate)   | B1_requirement_per_batch × 15 mẻ/SKU                |
| B2 hộp theo SKU      | box_output_per_batch                                       | box_output_per_batch × (1 + B2_buffer_rate)    | B2_box_requirement_per_batch × 15 mẻ/SKU            |
| B2 thùng theo SKU    | carton_output_per_batch                                    | carton_output_per_batch × (1 + B2_buffer_rate) | B2_carton_requirement_per_batch × 15 mẻ/SKU         |
| B3 vật tư dùng chung | Stock Policy hoặc consumption_per_batch nếu owner cấu hình | Theo tồn thực tế hoặc định mức đã duyệt        | Theo max/suppression policy, không cộng 7% mặc định |

**Nếu sản lượng thực tế của một mẻ thay đổi sau pilot, các ngưỡng B1/B2 phải tự tính lại theo output mới. Không được giữ cứng 7.200 gói, 1.800 hộp, 300 thùng nếu công thức chuẩn đã xác nhận sản lượng khác.**

## 1.7. Material / Packaging Disposal & Write-off Control

Khi nguyên liệu, bao bì hoặc vật tư phải hủy do QC reject, hết hạn, hư hỏng, sai mẫu, lỗi in, obsolete hoặc recall/quality issue, hệ thống không được xóa tay hoặc sửa tồn trực tiếp. Phải đi theo luồng hủy có kiểm soát để tồn kho, MRP và kế toán không bị sai.

| **Trạng thái / bước**                           | **Ý nghĩa**                          | **Quy tắc xử lý**                                     |
| ----------------------------------------------- | ------------------------------------ | ----------------------------------------------------- |
| HOLD / QC_REJECT / EXPIRED / DAMAGED / OBSOLETE | Vật tư không được dùng               | Không tính usable, không đưa vào phiếu cấp phát       |
| DISPOSAL_REQUESTED                              | Tạo yêu cầu hủy                      | Cần lý do, material/lot, quantity, evidence           |
| QA / OWNER REVIEW                               | Xem xét hủy                          | QA/owner duyệt hoặc trả về xử lý khác                 |
| DISPOSAL_APPROVED                               | Được duyệt hủy                       | Chưa giảm tồn nếu chưa thực hiện hủy                  |
| DISPOSAL_EXECUTED                               | Đã thực hiện hủy                     | Ghi evidence hủy, người thực hiện, thời gian          |
| INVENTORY_WRITTEN_OFF                           | Ghi giảm tồn kho                     | Bắt buộc qua Inventory Ledger write-off, không delete |
| ACCOUNTING_SYNC_PENDING                         | Chờ hạch toán nếu có giá trị kế toán | Đi qua MISA/accounting boundary                       |
| DISPOSAL_CLOSED                                 | Đóng hồ sơ hủy                       | MRP/Stock Alert đọc tồn mới sau write-off             |

**Hàng chờ hủy hoặc đã duyệt hủy không được tính vào available_stock, ready_stock hoặc suppression threshold để tránh hệ thống hiểu nhầm là còn vật tư usable.**

# 2\. NHÓM A - NGUYÊN LIỆU SẢN XUẤT

| **Mã nhóm** | **Tên nhóm**                                                | **Phạm vi**                                                                       | **+% hao hụt** |
| ----------- | ----------------------------------------------------------- | --------------------------------------------------------------------------------- | -------------- |
| A1          | Nguyên liệu Quân-Thần-Tá-Sứ của 20 SKU                      | Nguyên liệu trung tâm, nguyên liệu đặc thù từng SKU, dược liệu/nguyên liệu hỗ trợ | 5%             |
| A2          | Nguyên liệu nền dinh dưỡng + rau củ chiết dịch tạo nước hầm | Gạo, rau củ, nước dừa, xương/lòng trắng trứng nếu áp dụng theo BOM                | 5%             |
| A3          | Nguyên liệu nêm & tạo hương vị                              | Muối, tiêu, tỏi, hành, rễ cần tây, mì chính, hương sâm                            | 5%             |

Khóa tồn kho Nhóm A: nguyên liệu đặc thù theo SKU áp dụng ngưỡng 5 mẻ/SKU. Nguyên liệu dùng chung cho nhiều SKU áp dụng ngưỡng tổng 20 mẻ chuẩn aggregate, không tính 5 mẻ riêng lẻ cho từng SKU. Riêng Sâm Savigin là Company Source theo mùa vụ, không đưa vào phiếu thu mua như nguyên liệu mua ngoài.

## 2.1. Nhóm A1 - Nguyên liệu trong phần Quân-Thần-Tá-Sứ của 20 SKU

| **Mã vật tư đề xuất** | **Tên nguyên liệu chuẩn hóa** | **Nguồn quản trị**                  | **Vai trò / ghi chú**                                       | **+%** | **Quy tắc tồn kho/QC**                 | **Ngưỡng tồn / loại khỏi phiếu thu mua** |
| --------------------- | ----------------------------- | ----------------------------------- | ----------------------------------------------------------- | ------ | -------------------------------------- | ---------------------------------------- |
| A1-001                | Sâm Savigin                   | Company Source / vùng trồng công ty | Nguyên liệu trung tâm trong công thức Quân-Thần-Tá-Sứ       | 5%     | Theo lot, QC, READY_FOR_PRODUCTION     | SAVIGIN_STRATEGIC_RESERVE                |
| A1-002                | Diêm mạch                     | Supplier Source                     | Nguyên liệu đặc thù SKU A1                                  | 5%     | Theo lot, QC nếu áp dụng               | A_SPECIFIC_5_BATCH_SKU                   |
| A1-003                | Hạt sen                       | Supplier Source                     | Nguyên liệu đặc thù SKU A1                                  | 5%     | Theo lot, QC nếu áp dụng               | A_SPECIFIC_5_BATCH_SKU                   |
| A1-004                | Cá Basa                       | Supplier Source                     | Nguyên liệu mặn đặc thù SKU A2                              | 5%     | Theo lot lạnh/tươi, QC bắt buộc        | A_SPECIFIC_5_BATCH_SKU                   |
| A1-005                | Cá hồi                        | Supplier Source                     | Nguyên liệu mặn đặc thù SKU A3/B2                           | 5%     | Theo lot lạnh/tươi, QC bắt buộc        | A_SPECIFIC_5_BATCH_SKU                   |
| A1-006                | Lươn đồng                     | Supplier Source                     | Nguyên liệu mặn đặc thù SKU A4                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-007                | Thịt cừu                      | Supplier Source                     | Nguyên liệu mặn đặc thù SKU A5                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-008                | Rau má                        | Supplier Source                     | Nguyên liệu đặc thù SKU B1                                  | 5%     | Theo lot, QC nếu áp dụng               | A_SPECIFIC_5_BATCH_SKU                   |
| A1-009                | Cá cơm                        | Supplier Source                     | Nguyên liệu mặn đặc thù SKU B3                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-010                | Thịt heo nạc                  | Supplier Source                     | Nguyên liệu mặn đặc thù SKU B4                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-011                | Da heo                        | Supplier Source                     | Nguyên liệu mặn đặc thù SKU B4                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-012                | Hàu biển                      | Supplier Source                     | Nguyên liệu mặn đặc thù SKU B5                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-013                | Gà ác                         | Supplier Source                     | Nguyên liệu mặn đặc thù SKU B6                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-014                | Bào ngư                       | Supplier Source                     | Nguyên liệu mặn đặc thù SKU C1                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-015                | Đông trùng hạ thảo            | Supplier Source                     | Nguyên liệu đặc thù SKU C2                                  | 5%     | Theo lot, QC nếu áp dụng               | A_SPECIFIC_5_BATCH_SKU                   |
| A1-016                | Nấm đông cô                   | Supplier Source                     | Nguyên liệu đặc thù SKU C3                                  | 5%     | Theo lot, QC nếu áp dụng               | A_SPECIFIC_5_BATCH_SKU                   |
| A1-017                | Cua biển                      | Supplier Source                     | Nguyên liệu mặn đặc thù SKU C4                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-018                | Cá ngừ                        | Supplier Source                     | Nguyên liệu mặn đặc thù SKU C5                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-019                | Tôm                           | Supplier Source                     | Nguyên liệu mặn đặc thù SKU C6                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-020                | Thịt gà                       | Supplier Source                     | Nguyên liệu mặn đặc thù SKU C7                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-021                | Thịt heo                      | Supplier Source                     | Nguyên liệu mặn đặc thù SKU C8                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-022                | Thịt bò                       | Supplier Source                     | Nguyên liệu mặn đặc thù SKU C9                              | 5%     | Theo lot, QC bắt buộc                  | A_SPECIFIC_5_BATCH_SKU                   |
| A1-023                | Hoài sơn                      | Supplier Source                     | Dược liệu/nguyên liệu hỗ trợ trong nhiều SKU                | 5%     | Theo lot, QC nếu áp dụng               | A_COMMON_20_BATCH_POOL                   |
| A1-024                | Bạch linh                     | Supplier Source                     | Dược liệu/nguyên liệu hỗ trợ trong một số SKU               | 5%     | Theo lot, QC nếu áp dụng               | A_COMMON_20_BATCH_POOL                   |
| A1-025                | Kỷ tử                         | Supplier Source                     | Dược liệu/nguyên liệu hỗ trợ trong nhiều SKU                | 5%     | Theo lot, QC nếu áp dụng               | A_COMMON_20_BATCH_POOL                   |
| A1-026                | Gừng / Gừng nướng             | Supplier Source                     | Nguyên liệu tạo nền ấm/hương vị trong nhiều SKU             | 5%     | Theo lot, QC nếu áp dụng               | A_COMMON_20_BATCH_POOL                   |
| A1-027                | Trần bì                       | Supplier Source                     | Nguyên liệu hỗ trợ trong một số SKU                         | 5%     | Theo lot, QC nếu áp dụng               | A_COMMON_20_BATCH_POOL                   |
| A1-028                | Quế chi                       | Supplier Source                     | Nguyên liệu hỗ trợ SKU A5                                   | 5%     | Theo lot, QC nếu áp dụng               | A_SPECIFIC_5_BATCH_SKU                   |
| A1-029                | Vừng / Vừng không vỏ          | Supplier Source                     | Nguyên liệu đặc thù B2/B3                                   | 5%     | Theo lot, QC nếu áp dụng               | A_COMMON_20_BATCH_POOL / OWNER_CLASSIFY  |
| A1-030                | Táo tàu                       | Supplier Source                     | Nguyên liệu xuất hiện ở công thức đặc thù và nước hầm       | 5%     | Một vật tư canonical, nhiều usage_role | A_COMMON_20_BATCH_POOL                   |
| A1-031                | Rong biển                     | Supplier Source                     | Nguyên liệu xuất hiện ở công thức đặc thù và nước hầm       | 5%     | Một vật tư canonical, nhiều usage_role | A_COMMON_20_BATCH_POOL                   |
| A1-032                | Đậu xanh không vỏ             | Supplier Source                     | Nguyên liệu xuất hiện ở công thức đặc thù và nền dinh dưỡng | 5%     | Một vật tư canonical, nhiều usage_role | A_COMMON_20_BATCH_POOL                   |

## 2.2. Nhóm A2 - Nguyên liệu nền dinh dưỡng + rau củ chiết dịch tạo nước hầm

| **Mã vật tư đề xuất** | **Tên nguyên liệu chuẩn hóa** | **Nguồn quản trị**                    | **Vai trò / ghi chú**      | **+%** | **Quy tắc tồn kho/QC**              | **Ngưỡng tồn / loại khỏi phiếu thu mua** |
| --------------------- | ----------------------------- | ------------------------------------- | -------------------------- | ------ | ----------------------------------- | ---------------------------------------- |
| A2-001                | Gạo (lúa - tôm)               | Company/Supplier Source theo registry | Nguyên liệu nền dinh dưỡng | 5%     | Theo lot, QC nếu áp dụng            | A_COMMON_20_BATCH_POOL                   |
| A2-002                | Cà rốt / Carot                | Supplier Source                       | Nền dinh dưỡng và nước hầm | 5%     | Chuẩn hóa tên thành Cà rốt          | A_COMMON_20_BATCH_POOL                   |
| A2-003                | Bí đỏ                         | Supplier Source                       | Nguyên liệu nền dinh dưỡng | 5%     | Theo lot, QC nếu áp dụng            | A_COMMON_20_BATCH_POOL                   |
| A2-004                | Đậu xanh không vỏ             | Supplier Source                       | Nguyên liệu nền dinh dưỡng | 5%     | Có thể trùng A1 theo usage_role     | A_COMMON_20_BATCH_POOL                   |
| A2-005                | Nấm kim châm                  | Supplier Source                       | Nguyên liệu nền dinh dưỡng | 5%     | Theo lot, QC nếu áp dụng            | A_COMMON_20_BATCH_POOL                   |
| A2-006                | Táo tàu                       | Supplier Source                       | Rau củ/chiết dịch nước hầm | 5%     | Có thể trùng A1 theo usage_role     | A_COMMON_20_BATCH_POOL                   |
| A2-007                | Củ cải trắng                  | Supplier Source                       | Rau củ/chiết dịch nước hầm | 5%     | Theo lot, QC nếu áp dụng            | A_COMMON_20_BATCH_POOL                   |
| A2-008                | Rong biển                     | Supplier Source                       | Rau củ/chiết dịch nước hầm | 5%     | Có thể trùng A1 theo usage_role     | A_COMMON_20_BATCH_POOL                   |
| A2-009                | Hành tây                      | Supplier Source                       | Rau củ/chiết dịch nước hầm | 5%     | Theo lot, QC nếu áp dụng            | A_COMMON_20_BATCH_POOL                   |
| A2-010                | Cần tây                       | Supplier Source                       | Rau củ/chiết dịch nước hầm | 5%     | Theo lot, QC nếu áp dụng            | A_COMMON_20_BATCH_POOL                   |
| A2-011                | Lá thơm                       | Supplier Source                       | Rau củ/chiết dịch nước hầm | 5%     | Theo lot, QC nếu áp dụng            | A_COMMON_20_BATCH_POOL                   |
| A2-012                | Nước dừa nguyên chất          | Supplier/Company Source theo registry | Nền nước hầm               | 5%     | Đơn vị lít, intake/QC theo rule     | A_COMMON_20_BATCH_POOL                   |
| A2-013                | Lòng trắng trứng              | Supplier Source                       | Nền nước hầm               | 5%     | Cần guard dietary/vegan khi áp dụng | A_COMMON_20_BATCH_POOL                   |
| A2-014                | Xương heo xay vỡ              | Supplier Source                       | Nền nước hầm cho SKU mặn   | 5%     | Cần guard dietary/vegan khi áp dụng | A_COMMON_20_BATCH_POOL                   |

## 2.3. Nhóm A3 - Nguyên liệu nêm & tạo hương vị

| **Mã vật tư đề xuất** | **Tên nguyên liệu chuẩn hóa** | **Nguồn quản trị**                    | **Vai trò / ghi chú** | **+%** | **Quy tắc tồn kho/QC**                | **Ngưỡng tồn / loại khỏi phiếu thu mua** |
| --------------------- | ----------------------------- | ------------------------------------- | --------------------- | ------ | ------------------------------------- | ---------------------------------------- |
| A3-001                | Muối biển rang                | Supplier Source                       | Nêm vị                | 5%     | Theo lot, QC nếu áp dụng              | A_COMMON_20_BATCH_POOL                   |
| A3-002                | Tiêu đen rang                 | Supplier Source                       | Tạo hương vị          | 5%     | Theo lot, QC nếu áp dụng              | A_COMMON_20_BATCH_POOL                   |
| A3-003                | Tỏi nướng                     | Supplier Source                       | Tạo hương vị          | 5%     | Theo lot, QC nếu áp dụng              | A_COMMON_20_BATCH_POOL                   |
| A3-004                | Hành lá thái khúc             | Supplier Source                       | Tạo hương vị          | 5%     | Theo lot, QC nếu áp dụng              | A_COMMON_20_BATCH_POOL                   |
| A3-005                | Rễ cần tây thái nhuyễn        | Supplier Source                       | Tạo hương vị          | 5%     | Theo lot, QC nếu áp dụng              | A_COMMON_20_BATCH_POOL                   |
| A3-006                | Mì chính                      | Supplier Source                       | Tạo vị umami          | 5%     | Theo lot, QC nếu áp dụng              | A_COMMON_20_BATCH_POOL                   |
| A3-007                | Hương sâm                     | Supplier/Company Source theo registry | Bù hương sâm          | 5%     | Đơn vị ml, phải có config/QC nếu dùng | A_COMMON_20_BATCH_POOL                   |

Lưu ý quản trị A1/A2/A3: Một nguyên liệu có thể xuất hiện ở nhiều vai trò, ví dụ Táo tàu, Rong biển, Đậu xanh không vỏ. Khi vào Material Master chỉ nên quản trị một material canonical, nhưng BOM line phải ghi rõ usage_role theo A1/A2/A3 để trace, QC, MRP và costing không bị nhầm. Ngưỡng loại khỏi thu mua phải tính theo material canonical, không tính trùng theo usage_role.

# 3\. NHÓM B - BAO BÌ / VẬT TƯ ĐÓNG GÓI / VẬT TƯ PHỤ

| **Mã nhóm** | **Tên nhóm**           | **Phạm vi**                                              | **+% hao hụt** | **Cơ sở tính**                                                   |
| ----------- | ---------------------- | -------------------------------------------------------- | -------------- | ---------------------------------------------------------------- |
| B1          | Bao bì đóng gói cấp 1  | Cuộn màng đóng gói cho từng SKU                          | 7%             | Cơ sở 7.200 gói/mẻ; đề xuất 7.704 gói quy đổi/mẻ                 |
| B2          | Bao bì đóng gói cấp 2  | 20 loại hộp 4 gói/hộp + 20 loại thùng carton 6 hộp/thùng | 7%             | Cơ sở 1.800 hộp + 300 thùng/mẻ; đề xuất 1.926 hộp + 321 thùng/mẻ |
| B3          | Vật tư khác dùng chung | Tem dùng chung, băng keo, mực in dùng chung, vật tư phụ  | 0%             | Không tính hao hụt kế hoạch; tính tồn kho thực tế                |

Khóa tồn kho Nhóm B: do bao bì, hộp, thùng, tem/nhãn và vật tư in có thể cần đặt hàng trước 30-45 ngày, B1/B2 được kiểm soát theo ngưỡng 15 mẻ/SKU. Dưới ngưỡng mới được đưa vào danh mục xét mua; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua, trừ trường hợp kế hoạch sản xuất lớn được Giám đốc phê duyệt.

## 3.1. Nhóm B1 - Bao bì đóng gói cấp 1: cuộn màng đóng gói cho từng SKU

| **Mã vật tư đề xuất**      | **SKU**            | **Tên sản phẩm**               | **Loại vật tư**                   | **+%** | **Cơ sở/mẻ** | **Đề xuất/mẻ**       | **Ngưỡng loại khỏi phiếu thu mua 15 mẻ/SKU** |
| -------------------------- | ------------------ | ------------------------------ | --------------------------------- | ------ | ------------ | -------------------- | -------------------------------------------- |
| B1-A1-CS-DM-HS-FILM        | A1/CS/DM/HS        | Cháo Sâm - Diêm mạch & Hạt sen | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-A2-CS-BASA-FILM         | A2/CS/BASA         | Cháo Sâm - Cá Basa             | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-A3-CS-CAHOI-FILM        | A3/CS/CAHOI        | Cháo Sâm - Cá hồi              | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-A4-CS-LUON-FILM         | A4/CS/LUON         | Cháo Sâm - Lươn đồng           | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-A5-CS-CUU-FILM          | A5/CS/CUU          | Cháo Sâm - Thịt cừu & Táo tàu  | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-B1-CS-RM-DX-FILM        | B1/CS/RM/ĐX        | Cháo Sâm - Rau má & Đậu xanh   | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-B2-CS-DHA-FILM          | B2/CS/DHA          | Cháo Sâm - DHA Não bộ          | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-B3-CS-CACOM-FILM        | B3/CS/CACOM        | Cháo Sâm - Cá cơm & Vừng       | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-B4-CS-COLAGEN-FILM      | B4/CS/COLAGEN      | Cháo Sâm - Thịt heo & Da heo   | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-B5-CS-SINHLUC-FILM      | B5/CS/SINHLUC      | Cháo Sâm - Hàu biển            | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-B6-CS-GAAC-FILM         | B6/CS/GAAC         | Cháo Sâm - Gà ác               | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-C1-CS-BAONGU-FILM       | C1/CS/BAONGU       | Cháo Sâm - Bào ngư             | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-C2-CS-DONGTRUNG-FILM    | C2/CS/DONGTRUNG    | Cháo Sâm - Đông trùng hạ thảo  | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-C3-CS-NAMDONGCO-FILM    | C3/CS/NAMDONGCO    | Cháo Sâm - Nấm đông cô         | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-C4-CS-CUABIEN-FILM      | C4/CS/CUABIEN      | Cháo Sâm - Cua biển            | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-C5-CS-CANGU-FILM        | C5/CS/CANGU        | Cháo Sâm - Cá ngừ              | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-C6-CS-TOM-RONGBIEN-FILM | C6/CS/TOM/RONGBIEN | Cháo Sâm - Tôm & Rong biển     | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-C7-CS-THITGA-FILM       | C7/CS/THITGA       | Cháo Sâm - Thịt gà             | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-C8-CS-THITHEO-FILM      | C8/CS/THITHEO      | Cháo Sâm - Thịt heo            | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |
| B1-C9-CS-THITBO-FILM       | C9/CS/THITBO       | Cháo Sâm - Thịt bò             | Cuộn màng đóng gói cấp 1 theo SKU | 7%     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ | \>=115.560: loại; <115.560: xét mua          |

### Bảng nội suy nhanh B1 theo 15 mẻ/SKU

| **SKU**            | **Tên sản phẩm**               | **Cơ sở/mẻ** | **Sau hao hụt 7%/mẻ** | **Ngưỡng mẻ** | **Ngưỡng loại khỏi phiếu thu mua** |
| ------------------ | ------------------------------ | ------------ | --------------------- | ------------- | ---------------------------------- |
| A1/CS/DM/HS        | Cháo Sâm - Diêm mạch & Hạt sen | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| A2/CS/BASA         | Cháo Sâm - Cá Basa             | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| A3/CS/CAHOI        | Cháo Sâm - Cá hồi              | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| A4/CS/LUON         | Cháo Sâm - Lươn đồng           | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| A5/CS/CUU          | Cháo Sâm - Thịt cừu & Táo tàu  | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| B1/CS/RM/ĐX        | Cháo Sâm - Rau má & Đậu xanh   | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| B2/CS/DHA          | Cháo Sâm - DHA Não bộ          | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| B3/CS/CACOM        | Cháo Sâm - Cá cơm & Vừng       | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| B4/CS/COLAGEN      | Cháo Sâm - Thịt heo & Da heo   | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| B5/CS/SINHLUC      | Cháo Sâm - Hàu biển            | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| B6/CS/GAAC         | Cháo Sâm - Gà ác               | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| C1/CS/BAONGU       | Cháo Sâm - Bào ngư             | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| C2/CS/DONGTRUNG    | Cháo Sâm - Đông trùng hạ thảo  | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| C3/CS/NAMDONGCO    | Cháo Sâm - Nấm đông cô         | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| C4/CS/CUABIEN      | Cháo Sâm - Cua biển            | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| C5/CS/CANGU        | Cháo Sâm - Cá ngừ              | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| C6/CS/TOM/RONGBIEN | Cháo Sâm - Tôm & Rong biển     | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| C7/CS/THITGA       | Cháo Sâm - Thịt gà             | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| C8/CS/THITHEO      | Cháo Sâm - Thịt heo            | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |
| C9/CS/THITBO       | Cháo Sâm - Thịt bò             | 7.200 gói/mẻ | 7.704 gói quy đổi/mẻ  | 15 mẻ/SKU     | 115.560 gói quy đổi/SKU            |

## 3.2. Nhóm B2 - Bao bì đóng gói cấp 2: hộp sản phẩm và thùng carton theo từng SKU

| **SKU**            | **Tên sản phẩm**               | **Mã hộp**                | **Đề xuất hộp/mẻ** | **Ngưỡng hộp 15 mẻ** | **Mã thùng**                 | **Đề xuất thùng/mẻ** | **Ngưỡng thùng 15 mẻ** | **Quy tắc loại khỏi phiếu thu mua** |
| ------------------ | ------------------------------ | ------------------------- | ------------------ | -------------------- | ---------------------------- | -------------------- | ---------------------- | ----------------------------------- |
| A1/CS/DM/HS        | Cháo Sâm - Diêm mạch & Hạt sen | B2-A1-CS-DM-HS-BOX        | 1.926 hộp          | 28.890 hộp/SKU       | B2-A1-CS-DM-HS-CARTON        | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| A2/CS/BASA         | Cháo Sâm - Cá Basa             | B2-A2-CS-BASA-BOX         | 1.926 hộp          | 28.890 hộp/SKU       | B2-A2-CS-BASA-CARTON         | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| A3/CS/CAHOI        | Cháo Sâm - Cá hồi              | B2-A3-CS-CAHOI-BOX        | 1.926 hộp          | 28.890 hộp/SKU       | B2-A3-CS-CAHOI-CARTON        | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| A4/CS/LUON         | Cháo Sâm - Lươn đồng           | B2-A4-CS-LUON-BOX         | 1.926 hộp          | 28.890 hộp/SKU       | B2-A4-CS-LUON-CARTON         | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| A5/CS/CUU          | Cháo Sâm - Thịt cừu & Táo tàu  | B2-A5-CS-CUU-BOX          | 1.926 hộp          | 28.890 hộp/SKU       | B2-A5-CS-CUU-CARTON          | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| B1/CS/RM/ĐX        | Cháo Sâm - Rau má & Đậu xanh   | B2-B1-CS-RM-DX-BOX        | 1.926 hộp          | 28.890 hộp/SKU       | B2-B1-CS-RM-DX-CARTON        | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| B2/CS/DHA          | Cháo Sâm - DHA Não bộ          | B2-B2-CS-DHA-BOX          | 1.926 hộp          | 28.890 hộp/SKU       | B2-B2-CS-DHA-CARTON          | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| B3/CS/CACOM        | Cháo Sâm - Cá cơm & Vừng       | B2-B3-CS-CACOM-BOX        | 1.926 hộp          | 28.890 hộp/SKU       | B2-B3-CS-CACOM-CARTON        | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| B4/CS/COLAGEN      | Cháo Sâm - Thịt heo & Da heo   | B2-B4-CS-COLAGEN-BOX      | 1.926 hộp          | 28.890 hộp/SKU       | B2-B4-CS-COLAGEN-CARTON      | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| B5/CS/SINHLUC      | Cháo Sâm - Hàu biển            | B2-B5-CS-SINHLUC-BOX      | 1.926 hộp          | 28.890 hộp/SKU       | B2-B5-CS-SINHLUC-CARTON      | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| B6/CS/GAAC         | Cháo Sâm - Gà ác               | B2-B6-CS-GAAC-BOX         | 1.926 hộp          | 28.890 hộp/SKU       | B2-B6-CS-GAAC-CARTON         | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| C1/CS/BAONGU       | Cháo Sâm - Bào ngư             | B2-C1-CS-BAONGU-BOX       | 1.926 hộp          | 28.890 hộp/SKU       | B2-C1-CS-BAONGU-CARTON       | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| C2/CS/DONGTRUNG    | Cháo Sâm - Đông trùng hạ thảo  | B2-C2-CS-DONGTRUNG-BOX    | 1.926 hộp          | 28.890 hộp/SKU       | B2-C2-CS-DONGTRUNG-CARTON    | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| C3/CS/NAMDONGCO    | Cháo Sâm - Nấm đông cô         | B2-C3-CS-NAMDONGCO-BOX    | 1.926 hộp          | 28.890 hộp/SKU       | B2-C3-CS-NAMDONGCO-CARTON    | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| C4/CS/CUABIEN      | Cháo Sâm - Cua biển            | B2-C4-CS-CUABIEN-BOX      | 1.926 hộp          | 28.890 hộp/SKU       | B2-C4-CS-CUABIEN-CARTON      | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| C5/CS/CANGU        | Cháo Sâm - Cá ngừ              | B2-C5-CS-CANGU-BOX        | 1.926 hộp          | 28.890 hộp/SKU       | B2-C5-CS-CANGU-CARTON        | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| C6/CS/TOM/RONGBIEN | Cháo Sâm - Tôm & Rong biển     | B2-C6-CS-TOM-RONGBIEN-BOX | 1.926 hộp          | 28.890 hộp/SKU       | B2-C6-CS-TOM-RONGBIEN-CARTON | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| C7/CS/THITGA       | Cháo Sâm - Thịt gà             | B2-C7-CS-THITGA-BOX       | 1.926 hộp          | 28.890 hộp/SKU       | B2-C7-CS-THITGA-CARTON       | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| C8/CS/THITHEO      | Cháo Sâm - Thịt heo            | B2-C8-CS-THITHEO-BOX      | 1.926 hộp          | 28.890 hộp/SKU       | B2-C8-CS-THITHEO-CARTON      | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |
| C9/CS/THITBO       | Cháo Sâm - Thịt bò             | B2-C9-CS-THITBO-BOX       | 1.926 hộp          | 28.890 hộp/SKU       | B2-C9-CS-THITBO-CARTON       | 321 thùng            | 4.815 thùng/SKU        | \>= ngưỡng: loại; < ngưỡng: xét mua |

### Bảng nội suy nhanh B2 theo 15 mẻ/SKU

| **SKU**            | **Tên sản phẩm**               | **Cơ sở hộp** | **Sau 7% hộp** | **Ngưỡng hộp 15 mẻ** | **Cơ sở thùng** | **Sau 7% thùng** | **Ngưỡng thùng 15 mẻ** |
| ------------------ | ------------------------------ | ------------- | -------------- | -------------------- | --------------- | ---------------- | ---------------------- |
| A1/CS/DM/HS        | Cháo Sâm - Diêm mạch & Hạt sen | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| A2/CS/BASA         | Cháo Sâm - Cá Basa             | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| A3/CS/CAHOI        | Cháo Sâm - Cá hồi              | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| A4/CS/LUON         | Cháo Sâm - Lươn đồng           | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| A5/CS/CUU          | Cháo Sâm - Thịt cừu & Táo tàu  | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| B1/CS/RM/ĐX        | Cháo Sâm - Rau má & Đậu xanh   | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| B2/CS/DHA          | Cháo Sâm - DHA Não bộ          | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| B3/CS/CACOM        | Cháo Sâm - Cá cơm & Vừng       | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| B4/CS/COLAGEN      | Cháo Sâm - Thịt heo & Da heo   | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| B5/CS/SINHLUC      | Cháo Sâm - Hàu biển            | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| B6/CS/GAAC         | Cháo Sâm - Gà ác               | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| C1/CS/BAONGU       | Cháo Sâm - Bào ngư             | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| C2/CS/DONGTRUNG    | Cháo Sâm - Đông trùng hạ thảo  | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| C3/CS/NAMDONGCO    | Cháo Sâm - Nấm đông cô         | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| C4/CS/CUABIEN      | Cháo Sâm - Cua biển            | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| C5/CS/CANGU        | Cháo Sâm - Cá ngừ              | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| C6/CS/TOM/RONGBIEN | Cháo Sâm - Tôm & Rong biển     | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| C7/CS/THITGA       | Cháo Sâm - Thịt gà             | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| C8/CS/THITHEO      | Cháo Sâm - Thịt heo            | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |
| C9/CS/THITBO       | Cháo Sâm - Thịt bò             | 1.800 hộp/mẻ  | 1.926 hộp/mẻ   | 28.890 hộp/SKU       | 300 thùng/mẻ    | 321 thùng/mẻ     | 4.815 thùng/SKU        |

## 3.3. Nhóm B3 - Vật tư khác dùng chung, tính theo tồn kho thực tế

| **Mã vật tư đề xuất** | **Tên vật tư**                                | **Nhóm vật tư**       | **+%**             | **Nguyên tắc tính**  | **Ngưỡng loại khỏi phiếu thu mua**      |
| --------------------- | --------------------------------------------- | --------------------- | ------------------ | -------------------- | --------------------------------------- |
| B3-001                | Tem dùng chung                                | Vật tư dùng chung     | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-002                | Tem kiểm soát nội bộ / QC / release nếu dùng  | Vật tư dùng chung     | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-003                | Băng keo đóng thùng                           | Vật tư dùng chung     | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-004                | Mực in date/lot/HSD dùng chung                | Vật tư in dùng chung  | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-005                | Ribbon/mực in mã vạch/QR nếu dùng             | Vật tư in dùng chung  | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-006                | Decal/nhãn dùng chung nếu không tách theo SKU | Vật tư in dùng chung  | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-007                | Dung môi/vật tư vệ sinh máy in nếu dùng       | Vật tư phụ dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |
| B3-008                | Dao cắt/keo/vật tư phụ đóng gói khác          | Vật tư phụ dùng chung | Không tính hao hụt | Tính tồn kho thực tế | Theo Stock Policy; đủ/vượt policy: loại |

# 4\. Quy tắc tính nhanh theo mẻ 400 và ngưỡng loại khỏi phiếu thu mua

| **Chỉ tiêu**              | **Cơ sở**                  | **Áp dụng**                               | **Đề xuất/MRP**            | **Ngưỡng loại khỏi phiếu thu mua**                                                           |
| ------------------------- | -------------------------- | ----------------------------------------- | -------------------------- | -------------------------------------------------------------------------------------------- |
| Sản lượng gói/phần ăn     | 7.200 gói/mẻ               | B1 cuộn màng cấp 1                        | +7% = 7.704 gói quy đổi/mẻ | 15 mẻ/SKU = 115.560 gói quy đổi/SKU                                                          |
| Sản lượng hộp             | 1.800 hộp/mẻ               | B2 hộp sản phẩm                           | +7% = 1.926 hộp/mẻ         | 15 mẻ/SKU = 28.890 hộp/SKU                                                                   |
| Sản lượng thùng           | 300 thùng/mẻ               | B2 thùng carton                           | +7% = 321 thùng/mẻ         | 15 mẻ/SKU = 4.815 thùng/SKU                                                                  |
| Nguyên liệu A đặc thù SKU | Theo BOM G1/mẻ 400         | A1/A2/A3 đặc thù 1 SKU                    | +5% hao hụt trong MRP      | Ngưỡng loại khỏi thu mua = BOM G1/mẻ × 5 mẻ/SKU                                              |
| Nguyên liệu A dùng chung  | Theo aggregate BOM G1      | A1/A2/A3 dùng chung nhiều SKU             | +5% hao hụt trong MRP      | Ngưỡng loại khỏi thu mua = aggregate nhu cầu 20 mẻ chuẩn                                     |
| Sâm Savigin               | Company Source theo mùa vụ | Nguồn công ty trồng                       | Không là phiếu thu mua     | Harvest theo mùa vụ; cảnh báo thiếu khi dưới production safety threshold hoặc forecast thiếu |
| Vật tư B3                 | Theo tồn kho thực tế       | Tem, băng keo, mực in, ribbon, vật tư phụ | +0% mặc định               | Theo Stock Policy; không tự mua nếu tồn đủ/vượt policy                                       |

Công thức MRP đề xuất: Nhu cầu đề xuất = Nhu cầu theo BOM/Packaging Config × (1 + tỷ lệ +%) - tồn khả dụng - incoming đã được duyệt + lượng reserve/điều chỉnh nếu có.

Công thức loại khỏi phiếu thu mua: Nếu available_stock + approved_incoming >= suppression_threshold thì material bị loại khỏi phiếu thu mua, trừ khi có kế hoạch sản xuất lớn được Giám đốc phê duyệt.

# 5\. Bảng owner kiểm soát tồn kho theo nhóm

| **Nhóm**             | **+%**                        | **Ngưỡng tồn/loại khỏi thu mua** | **Điều kiện được đưa vào phiếu thu mua**     | **Điều kiện loại khỏi phiếu thu mua**                    | **Ghi chú owner**                                                  |
| -------------------- | ----------------------------- | -------------------------------- | -------------------------------------------- | -------------------------------------------------------- | ------------------------------------------------------------------ |
| A1/A2/A3 đặc thù SKU | 5%                            | 5 mẻ/SKU theo BOM G1             | available + incoming < BOM×5                 | available + incoming >= BOM×5                            | Nếu vượt ngưỡng nhưng có kế hoạch lớn: Giám đốc phê duyệt          |
| A1/A2/A3 dùng chung  | 5%                            | 20 mẻ chuẩn aggregate            | available + incoming < aggregate 20 mẻ       | available + incoming >= aggregate 20 mẻ                  | Không tính 5 mẻ riêng từng SKU để tránh mua trùng                  |
| Sâm Savigin          | 5% trong MRP nếu cấp sản xuất | Strategic reserve theo mùa vụ    | Không tạo phiếu thu mua; harvest theo chu kỳ | Không áp dụng purchase suppression như supplier material | Chỉ cảnh báo thiếu dưới production safety threshold/forecast thiếu |
| B1 film theo SKU     | 7%                            | 15 mẻ/SKU = 115.560 gói quy đổi  | <115.560 gói quy đổi/SKU                     | \>=115.560 gói quy đổi/SKU                               | Do lead time 30-45 ngày; vẫn cần MRP/owner review                  |
| B2 hộp theo SKU      | 7%                            | 15 mẻ/SKU = 28.890 hộp           | <28.890 hộp/SKU                              | \>=28.890 hộp/SKU                                        | Không mua nếu vượt ngưỡng, trừ kế hoạch lớn được duyệt             |
| B2 thùng theo SKU    | 7%                            | 15 mẻ/SKU = 4.815 thùng          | <4.815 thùng/SKU                             | \>=4.815 thùng/SKU                                       | Không mua nếu vượt ngưỡng, trừ kế hoạch lớn được duyệt             |
| B3 dùng chung        | 0% mặc định                   | Theo Stock Policy thực tế        | Theo min/safety/reorder policy               | Theo max/suppression policy                              | Nếu có định mức theo batch thì owner cấu hình riêng                |

# 6\. Khóa nguyên tắc đưa vào PACK-03

- Nhóm A là nguyên liệu sản xuất, chia A1/A2/A3 và áp dụng +5% hao hụt kế hoạch trong MRP.
- Nhóm A đặc thù theo SKU áp dụng ngưỡng tồn 5 mẻ/SKU; bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua.
- Nhóm A dùng chung nhiều SKU áp dụng ngưỡng aggregate 20 mẻ chuẩn; không tính 5 mẻ riêng lẻ từng SKU để tránh mua trùng.
- Sâm Savigin là Company Source theo mùa vụ, được harvest/sơ chế/intake/QC/lot/readiness/nhập kho dự trữ chiến lược; không đưa vào phiếu thu mua như nguyên liệu supplier.
- Hệ thống chỉ cảnh báo nguy cơ thiếu Sâm cho sản xuất khi tồn Sâm READY_FOR_PRODUCTION dưới production safety threshold hoặc forecast sản xuất vượt tồn dự trữ.
- Nhóm B là bao bì/vật tư, chia B1/B2/B3; B1/B2 áp dụng +7%, B3 tính tồn kho thực tế và không tính hao hụt kế hoạch mặc định.
- B1/B2 theo SKU áp dụng ngưỡng 15 mẻ/SKU do lead time đặt hàng 30-45 ngày.
- B1 film: ngưỡng loại khỏi phiếu thu mua = 115.560 gói quy đổi/SKU.
- B2 hộp: ngưỡng loại khỏi phiếu thu mua = 28.890 hộp/SKU.
- B2 thùng: ngưỡng loại khỏi phiếu thu mua = 4.815 thùng/SKU.
- B3 là vật tư dùng chung như tem, băng keo, mực in, ribbon, decal và vật tư phụ; xử lý theo Stock Policy thực tế.
- Tỷ lệ +%, mẻ chuẩn, ngưỡng 5 mẻ, ngưỡng 15 mẻ và ngưỡng 20 mẻ là config/policy, không hardcode trong code hoặc template.
- Material Master phải quản trị theo material canonical; cùng một nguyên liệu xuất hiện ở nhiều vai trò thì không tạo trùng vật tư, mà tách usage_role trên BOM line.
- Phiếu thu mua phải tự loại vật tư đủ/vượt ngưỡng, không cho mua sắm chỉ vì có yêu cầu.
- Kế hoạch sản xuất/đơn hàng lớn muốn vượt ngưỡng phải có Giám đốc phê duyệt, evidence, audit và MRP run hợp lệ.
- Không phát hành Production Order nếu thiếu nguyên liệu/vật tư cần thiết hoặc tồn khả dụng không đạt guard.
- Công thức pilot phải lấy gạo làm anchor_ingredient; khi nhập nguyên liệu kg/g/l/ml hệ thống tự chuẩn hóa đơn vị và tính ratio_to_rice.
- Sau khi pilot thành công và công thức được owner duyệt, người dùng chỉ cần nhập anchor_rice_quantity thì hệ thống tự tính toàn bộ nguyên liệu theo tỷ lệ đã khóa.
- Scale số kg gạo không phải đổi công thức; đổi tỷ lệ, thêm nguyên liệu hoặc bỏ nguyên liệu sau khi công thức đã khóa thì bắt buộc tạo Formula Version mới.
- Ngưỡng tồn Nhóm A phải nhảy theo calculated_material_quantity_per_batch sau khi công thức được scale; không dùng số cứng nếu công thức/mẻ thay đổi.
- Ngưỡng B1/B2 phải nhảy theo sản lượng chuẩn thực tế của mẻ: gói, hộp, thùng; không tính trực tiếp theo kg gạo.
- Buffer +5% Nhóm A và +7% B1/B2 là buffer MRP/stock planning, không tự sửa công thức sản xuất đã phê duyệt.
- Nguyên liệu/vật tư hủy phải đi qua Disposal Request, QA/Owner Review, Disposal Execution, Inventory Ledger Write-off, Evidence và Audit; không được xóa tay tồn kho.

# 7\. Smoke test bắt buộc cho ngưỡng tồn kho và phiếu thu mua

| **Smoke ID** | **Tình huống**                                                                     | **Kết quả kỳ vọng**                                                                  |
| ------------ | ---------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| MAT-SUP-001  | Nguyên liệu A đặc thù SKU tồn >= 5 mẻ/SKU                                          | Loại khỏi phiếu thu mua                                                              |
| MAT-SUP-002  | Nguyên liệu A đặc thù SKU tồn < 5 mẻ/SKU và có MRP demand                          | Được đưa vào danh mục xét nhập/mua                                                   |
| MAT-SUP-003  | Nguyên liệu A dùng chung tồn >= aggregate 20 mẻ chuẩn                              | Loại khỏi phiếu thu mua                                                              |
| MAT-SUP-004  | Nguyên liệu A dùng chung tồn < aggregate 20 mẻ chuẩn                               | Được đưa vào Material Requirement Board nếu có demand                                |
| MAT-SUP-005  | Sâm Savigin đến kỳ thu hoạch dù tồn sản xuất còn đủ                                | Tạo Company Source Harvest Alert/Harvest Requirement, không tạo Purchase Requirement |
| MAT-SUP-006  | Sâm Savigin READY_FOR_PRODUCTION dưới ngưỡng an toàn sản xuất                      | Cảnh báo SAVIGIN_PRODUCTION_STOCK_LOW                                                |
| MAT-SUP-007  | B1 film tồn >= 115.560 gói quy đổi/SKU                                             | Loại khỏi phiếu thu mua                                                              |
| MAT-SUP-008  | B2 hộp tồn >= 28.890 hộp/SKU                                                       | Loại khỏi phiếu thu mua                                                              |
| MAT-SUP-009  | B2 thùng tồn >= 4.815 thùng/SKU                                                    | Loại khỏi phiếu thu mua                                                              |
| MAT-SUP-010  | B3 tồn đủ/vượt Stock Policy                                                        | Loại khỏi phiếu mua; không tự mua thêm                                               |
| MAT-SUP-011  | Đơn hàng/kế hoạch sản xuất lớn vượt ngưỡng, chưa có Giám đốc phê duyệt             | BLOCKED / OWNER_APPROVAL_REQUIRED                                                    |
| MAT-SUP-012  | Đơn hàng/kế hoạch sản xuất lớn vượt ngưỡng, có Giám đốc phê duyệt + evidence + MRP | Cho phép override có kiểm soát, không tự tạo PO nếu procurement gate chưa đạt        |
| MAT-FRM-001  | Pilot nhập gạo làm anchor và nhập nguyên liệu theo kg/g/l/ml                       | Hệ thống chuẩn hóa UOM và tự tính ratio_to_rice cho từng BOM line                    |
| MAT-FRM-002  | Pilot thêm hoặc bỏ nguyên liệu trước khi khóa công thức                            | Cho phép thay đổi trong pilot, có audit và Formula Candidate Snapshot                |
| MAT-FRM-003  | Công thức đã khóa, user nhập số kg gạo mới                                         | Toàn bộ nguyên liệu Nhóm A tự nhảy theo ratio_to_rice và tạo Calculation Snapshot    |
| MAT-FRM-004  | Chỉ scale số kg gạo, không đổi tỷ lệ                                               | Không tạo Formula Version mới; chỉ tạo BOM/Production Snapshot theo mẻ               |
| MAT-FRM-005  | Sau khi khóa công thức, user đổi tỷ lệ hoặc thêm/bỏ nguyên liệu                    | BLOCKED hoặc yêu cầu tạo Formula Version mới + owner approval + evidence             |
| MAT-FRM-006  | Công thức/mẻ thay đổi làm calculated_material_quantity_per_batch thay đổi          | Ngưỡng Nhóm A tự tính lại theo 5 mẻ/SKU hoặc aggregate 20 mẻ                         |
| MAT-FRM-007  | Sản lượng mẻ thay đổi số gói/hộp/thùng                                             | Ngưỡng B1/B2 tự tính lại theo output mới và 15 mẻ/SKU                                |
| MAT-FRM-008  | Liquid material thiếu UOM/density policy khi cần quy đổi                           | BLOCKED_CONFIG_MISSING / OWNER_REVIEW_REQUIRED                                       |
| MAT-DSP-001  | Nguyên liệu QC_REJECT hoặc EXPIRED cần hủy                                         | Tạo Disposal Request; không tính usable; không xóa tay tồn kho                       |
| MAT-DSP-002  | Disposal đã được duyệt và thực hiện                                                | Inventory Ledger write-off, accounting sync nếu cần, evidence/audit đầy đủ           |
