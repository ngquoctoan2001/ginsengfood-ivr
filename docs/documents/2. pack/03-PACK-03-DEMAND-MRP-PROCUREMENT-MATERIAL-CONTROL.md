# PACK-03 - DEMAND MRP PROCUREMENT MATERIAL CONTROL

## DEMAND–MRP–FORMULA SCALING–PROCUREMENT–STOCK ALERT–MATERIAL–PACKAGING CONTROL PACK

## PHẦN 1/4 — GOVERNANCE FOUNDATION / DEMAND PRINCIPLES / PILOT-TO-G1 FORMULA SCALING / PRODUCTION DEMAND BOARD

## 1. MỤC TIÊU CỦA PACK-03 V2.0

PACK-03 V2.0 khóa lớp quản trị từ nhu cầu sản xuất, công thức neo gạo, MRP, ngưỡng tồn kho, ngưỡng loại khỏi phiếu thu mua, nguồn công ty trồng, thu mua/thu hoạch, nhập kho, readiness, cảnh báo tồn kho, hủy vật tư và kiểm soát nguyên liệu – bao bì – vật tư.

PACK-03 V2.0 không phải tài liệu code.

PACK-03 V2.0 không thiết kế API, DTO, database, UI, worker hoặc route chi tiết.

PACK-03 V2.0 là tài liệu governance/domain dùng để khóa:

- Nhu cầu sản xuất không được đi thẳng thành lệnh sản xuất.

- Pilot công thức phải lấy gạo làm nguyên liệu neo.

- Pilot thành công phải được khóa thành G1 Approved Operational Formula.

- Bộ phận sản xuất không được sửa công thức G1.

- Khi sản xuất, chỉ được nhập số kg gạo / batch anchor quantity trong phạm vi được phép.

- Hệ thống tự scale nguyên liệu theo tỷ lệ đã khóa.

- Hệ thống tự tạo Production BOM Snapshot.

- MRP tính theo BOM Snapshot, Packaging Config, Print Config và tồn kho thực tế.

- Ngưỡng tồn Nhóm A phải tự nhảy theo công thức đã scale.

- Ngưỡng vật tư Nhóm B phải tự nhảy theo sản lượng gói/hộp/thùng thực tế.

- Phiếu thu mua phải tự loại nguyên liệu/vật tư đủ hoặc vượt ngưỡng.

- Sâm Savigin là Company Source theo mùa vụ, không xử lý như nguyên liệu mua ngoài.

- Nguyên liệu/vật tư hủy phải qua Disposal & Write-off Control.

- Không có evidence, smoke, audit, owner sign-off thì không PASS.

## 2. VỊ TRÍ CỦA PACK-03 TRONG BỘ HỒ SƠ GINSENGFOOD

PACK-03 đứng giữa lớp Product Master / Recipe / Formula của PACK-02 và lớp Operational Core / Production / Warehouse / Inventory / Traceability / Recall / Sale Lock của PACK-01.

PACK-03 không thay PACK-01.

PACK-03 không thay PACK-02.

PACK-03 dùng PACK-02 để biết SKU, nguyên liệu, công thức, G1, BOM, Packaging Config, Print Config, QC/HSD/Trace/Recall Config.

PACK-03 dùng PACK-01 để biết Production Order, Material Issue, Raw Material Lot, Warehouse Receipt, Inventory Ledger, Traceability, Recall, Sale Lock và Evidence.

PACK-03 khóa cách đi từ nhu cầu sản xuất đến điều kiện được phép phát hành Production Order chính thức.

## 3. SOURCE-OF-TRUTH CỦA PACK-03 V2.0

## 3.1. Source-of-Truth nền

Nhóm nguồn | Vai trò

MASTER-00 đến MASTER-07 | Governance lõi, Source-of-Truth, Dependency, Guard, Evidence, Release Gate

PACK-01 | Operational Core, Production, Warehouse, Inventory, Trace, Recall, Sale Lock

PACK-02 | Product Master, SKU, Ingredient, Recipe, Formula Version, Product Activation

File danh mục V1.2 đã khóa rõ: Nhóm A là nguyên liệu sản xuất A1–A3, Nhóm B là bao bì/vật tư B1–B3; tỷ lệ +5% và +7% là MRP config, không hardcode; đồng thời tài liệu này dùng cho PACK-03 MRP, Stock Alert, Material Master, Formula Scaling và Procurement Suppression Threshold.

## 3.2. Source-of-Truth bắt buộc cho công thức

Công thức sản xuất không được lấy từ ghi chú rời, Excel cá nhân, tin nhắn nội bộ hoặc thao tác nhập tay không có version.

Công thức hợp lệ phải có:

- SKU.

- Formula Version.

- Formula Status.

- Ingredient canonical.

- BOM line.

- Usage role.

- Unit policy.

- Ratio policy.

- Anchor policy.

- Rounding policy.

- Evidence.

- Owner approval.

- Audit.

## 4. NGUYÊN TẮC G0 / PILOT / G1 / VERSION SAU G1

## 4.1. Định nghĩa G0

G0 là công thức gốc nghiên cứu, lịch sử nghiên cứu hoặc công thức nền thử nghiệm.

G0 không được dùng trực tiếp như công thức vận hành sản xuất chính thức nếu chưa được chuyển hóa, kiểm tra, phê duyệt và khóa thành version vận hành.

## 4.2. Định nghĩa Pilot Formula

Pilot Formula là công thức thử nghiệm có kiểm soát.

Trong giai đoạn pilot, owner/pilot được phép:

- Nhập tên nguyên liệu.

- Nhập số lượng kg/g/l/ml.

- Thêm nguyên liệu.

- Bỏ nguyên liệu.

- Thay đổi định lượng.

- Thử nghiệm mẻ nhỏ.

- Lưu snapshot.

- Ghi evidence.

- Ghi audit.

Pilot Formula chưa phải G1 nếu chưa được phê duyệt.

## 4.3. Định nghĩa G1 Approved Operational Formula

G1 Approved Operational Formula là công thức vận hành được phê duyệt sau pilot.

Khi pilot được duyệt thành G1, công thức trở thành source-of-truth cho sản xuất.

Từ thời điểm đó:

- Bộ phận sản xuất không được sửa công thức.

- Bộ phận sản xuất không được thêm nguyên liệu.

- Bộ phận sản xuất không được bỏ nguyên liệu.

- Bộ phận sản xuất không được sửa tỷ lệ.

- Bộ phận sản xuất không được sửa đơn vị.

- Bộ phận sản xuất không được sửa số lượng nguyên liệu đã được hệ thống tính.

- Bộ phận sản xuất chỉ được nhập số kg gạo / batch anchor quantity trong phạm vi được phép.

- Hệ thống tự scale công thức.

- Hệ thống tạo Production BOM Snapshot.

- Snapshot được dùng cho Production Order, MRP, Material Issue, costing, trace và audit.

File owner lock đã xác định rõ: sau khi pilot được phê duyệt thành G1, bộ phận sản xuất không có quyền sửa công thức; khi lập lệnh sản xuất chỉ được nhập số kg gạo / batch anchor quantity, hệ thống tự scale nguyên liệu theo tỷ lệ đã khóa và tạo Production BOM Snapshot.

## 4.4. Version sau G1

Mọi thay đổi sau G1 phải tạo Formula Version mới nếu thay đổi một trong các điểm sau:

- Tỷ lệ nguyên liệu.

- Thêm nguyên liệu.

- Bỏ nguyên liệu.

- Đổi đơn vị tính gốc.

- Đổi cấu trúc công thức.

- Đổi usage basis.

- Đổi anchor policy.

- Đổi rounding policy có ảnh hưởng định lượng.

- Đổi yield policy có ảnh hưởng sản lượng.

- Đổi quy trình làm ảnh hưởng BOM/MRP/costing/trace.

Không được sửa đè G1 đã từng dùng cho Production Order, Batch, Trace, Inventory hoặc Costing.

## 5. RICE ANCHOR FORMULA GOVERNANCE

## 5.1. Gạo là anchor_ingredient

Trong công thức Ginsengfood, gạo là nguyên liệu neo trung tâm.

Hệ thống phải xác định:

Trường | Ý nghĩa

anchor_ingredient_id | Nguyên liệu neo, mặc định là gạo

anchor_ingredient_name | Gạo lúa – tôm / gạo trong công thức

anchor_quantity | Số lượng gạo nhập ở pilot hoặc sản xuất

anchor_unit | kg/g theo UOM policy

formula_scope | SKU / formula version

formula_status | Pilot / Approved G1 / Retired / Superseded

owner_reference | Owner phê duyệt

evidence_reference | Evidence

audit_reference | Audit

## 5.2. Pilot nhập nguyên liệu và hệ thống tự tính tỷ lệ

Trong giai đoạn pilot, khi owner/pilot nhập:

- Tên nguyên liệu.

- Số lượng.

- Đơn vị kg/g/l/ml.

- Usage role.

- Ghi chú thử nghiệm nếu có.

Hệ thống phải:

- Chuẩn hóa material canonical.

- Chuẩn hóa đơn vị.

- Kiểm tra UOM policy.

- Kiểm tra density policy nếu cần quy đổi lít/ml sang khối lượng.

- Tính ratio_to_rice.

- Lưu Pilot Formula Snapshot.

- Ghi audit.

- Cho phép so sánh nhiều pilot batch nếu cần.

Công thức tỷ lệ:

ratio_to_rice = standardized_quantity / standardized_rice_quantity × 100%

Với nguyên liệu dạng lít/ml, nếu chưa có density config thì không ép đổi sang kg. Hệ thống lưu theo lít/kg gạo hoặc ml/kg gạo; nếu cần quy đổi khối lượng thì phải qua density/UOM config. Cơ chế này đã được khóa trong bản owner lock V1.2.

## 5.3. Pilot được phép thêm/bỏ nguyên liệu

Trong giai đoạn pilot, owner/pilot được phép thêm hoặc bỏ nguyên liệu.

Tuy nhiên, mọi thao tác phải có:

- Người thực hiện.

- Thời điểm.

- Lý do.

- Snapshot trước/sau.

- Evidence nếu có.

- Audit.

Sau khi pilot được phê duyệt thành G1, thêm/bỏ nguyên liệu không còn là thao tác pilot nữa mà là thay đổi công thức, bắt buộc tạo Formula Version mới.

## 6. G1 PRODUCTION FORMULA LOCK

## 6.1. Khi nào khóa G1

G1 chỉ được khóa khi:

- Pilot đạt yêu cầu.

- Công thức có đầy đủ nguyên liệu canonical.

- Tất cả nguyên liệu có UOM hợp lệ.

- Tỷ lệ theo gạo đã tính.

- Owner xác nhận công thức đạt.

- Evidence pilot đầy đủ.

- Rounding policy rõ.

- Yield policy rõ nếu đã có.

- Material group A1/A2/A3 rõ.

- Packaging/yield dependency rõ nếu áp dụng.

- Audit đầy đủ.

## 6.2. Dữ liệu phải lưu tại thời điểm khóa G1

Khi khóa G1, hệ thống phải lưu:

Nhóm | Dữ liệu cần lưu

Formula identity | sku_id, formula_version, formula_kind, formula_status

Anchor | anchor_ingredient_id, anchor_quantity_pilot, anchor_unit

BOM | material_id, usage_role, quantity, unit, ratio_to_rice

UOM | unit_policy, density_policy nếu có

Rounding | rounding_policy, min/max nếu có

Yield | expected_pack_output, box_output, carton_output nếu có

Governance | owner_approval, effective_date

Evidence | pilot evidence, sensory/quality evidence nếu có

Audit | created_by, approved_by, locked_at

## 6.3. Bộ phận sản xuất không được sửa G1

Sau khi G1 được khóa, bộ phận sản xuất chỉ được thao tác trong phạm vi sản xuất.

Bộ phận sản xuất được phép:

- Chọn SKU được phép sản xuất.

- Chọn G1 active theo SKU.

- Nhập anchor_rice_quantity trong phạm vi batch sizing policy.

- Xem công thức hệ thống tự tính.

- Xem Production BOM Snapshot.

- Gửi lệnh sản xuất qua approval.

- Thực hiện sản xuất theo snapshot đã duyệt.

Bộ phận sản xuất không được:

- Sửa nguyên liệu.

- Sửa tỷ lệ.

- Sửa số lượng hệ thống tính.

- Sửa đơn vị.

- Thêm nguyên liệu.

- Bỏ nguyên liệu.

- Sửa buffer MRP.

- Sửa yield policy.

- Sản xuất ngoài snapshot.

- Tạo công thức mới từ màn hình sản xuất.

## 6.4. Scale mẻ không phải sửa công thức

Nếu chỉ đổi số kg gạo, không đổi tỷ lệ, không thêm/bỏ nguyên liệu, không đổi UOM policy, thì đây là scale công thức.

Scale công thức không tạo Formula Version mới.

Scale công thức tạo:

- Calculation Snapshot.

- Production BOM Snapshot.

- MRP Requirement tương ứng.

- Material Issue Plan tương ứng.

- Costing basis tương ứng nếu áp dụng.

- Trace basis tương ứng nếu áp dụng.

## 7. PRODUCTION BOM SNAPSHOT LOCK

## 7.1. Định nghĩa

Production BOM Snapshot là bản chụp công thức đã được hệ thống tính ra cho một lệnh sản xuất cụ thể.

Snapshot này được tạo từ:

- SKU.

- G1 active.

- anchor_rice_quantity.

- ratio_to_rice.

- UOM policy.

- rounding policy.

- yield policy.

- calculation timestamp.

## 7.2. Vì sao phải có Production BOM Snapshot

Production BOM Snapshot bắt buộc để đảm bảo:

- Lệnh sản xuất không bị ảnh hưởng nếu công thức active thay đổi sau này.

- Material Issue cấp đúng số lượng.

- MRP tính đúng nhu cầu.

- Batch trace được về đúng công thức.

- Costing không bị lệch.

- QA/QC biết đúng công thức được dùng.

- Không ai sửa công thức trực tiếp trong sản xuất.

- Audit có bằng chứng tại thời điểm phát lệnh.

## 7.3. Snapshot không được sửa tay

Production BOM Snapshot không được sửa tay.

Nếu cần thay đổi vì lý do sản xuất, phải xử lý theo một trong các hướng:

Tình huống | Cách xử lý

Nhập sai anchor_rice_quantity trước khi duyệt lệnh | Hủy draft hoặc tạo lại calculation snapshot

Công thức sai | Không sửa snapshot; tạo Formula Version mới

Nguyên liệu thiếu | Không sửa công thức; xử lý qua MRP/procurement/material substitution nếu policy cho phép

Hao hụt thực tế cao | Ghi variance, không sửa G1

Cần đổi tỷ lệ | Tạo Formula Version mới

Cần đổi yield | Tạo yield policy/version mới nếu ảnh hưởng chuẩn sản xuất

## 7.4. Field App chỉ hiển thị snapshot

Field App / Shopfloor / Mobile PWA chỉ hiển thị Production BOM Snapshot đã được duyệt.

Field App không phải nơi sửa công thức.

Field App không được:

- Tạo nguyên liệu mới.

- Đổi tỷ lệ.

- Đổi số lượng công thức.

- Đổi BOM line.

- Đổi G1.

- Vượt Material Guard.

## 8. BUFFER MRP KHÔNG PHẢI CÔNG THỨC SẢN XUẤT

## 8.1. Tách công thức sản xuất và buffer kế hoạch

Công thức sản xuất là số lượng nguyên liệu được tính theo G1 và anchor_rice_quantity.

Buffer MRP là tỷ lệ cộng thêm để lập kế hoạch tồn kho, nhập/mua/chuẩn bị.

Hai lớp này không được nhập làm một.

Ví dụ:

Thành phần | Vai trò

Số lượng nguyên liệu theo G1 | Công thức sản xuất

+5% Nhóm A | Buffer MRP/stock planning

+7% B1/B2 | Buffer vật tư bao bì/packaging planning

B3 +0% mặc định | Theo tồn thực tế/Stock Policy

Tài liệu owner lock đã xác định tỷ lệ +5% và +7% là cấu hình kế hoạch để lập MRP, không hardcode trong code; buffer không được tự động sửa công thức sản xuất đã phê duyệt.

## 8.2. Quy tắc khóa

Không được:

- Cộng +5% trực tiếp vào công thức G1.

- Cộng +7% trực tiếp vào yield sản xuất.

- Ghi buffer thành định lượng sản xuất thật.

- Dùng buffer để hợp thức hóa sai lệch công thức.

- Sửa Production BOM Snapshot vì buffer.

Buffer chỉ dùng cho MRP, Stock Alert, Procurement Planning và Procurement Suppression Threshold.

## 9. DEMAND GOVERNANCE PRINCIPLES

## 9.1. Production Demand không phải Production Order

Production Demand là nhu cầu sản xuất.

Production Order là lệnh sản xuất chính thức.

Hai khái niệm này phải tách riêng.

Production Demand chỉ trả lời:

Có nhu cầu sản xuất SKU nào, số lượng bao nhiêu, lý do gì, kỳ nào, nguồn nào?

Production Order trả lời:

Nhà máy được phép sản xuất SKU nào, theo công thức nào, snapshot nào, số lượng nào, nguyên liệu nào, thời điểm nào, ai duyệt?

## 9.2. Không có đường tắt sang Production Order

Các đường đi sau bị cấm:

Đường đi bị cấm | Lý do

Sales Demand -> Production Order | Chưa qua Demand Board/MRP/Owner Approval

Dealer Order -> Production Order | Phải kiểm tra tồn kho và phần thiếu

Distributor Order -> Production Order | Không được sản xuất toàn bộ nếu kho đủ một phần

Stock Alert -> Production Order | Cảnh báo không phải quyết định sản xuất

Material Alert -> Purchase Order | Cảnh báo vật tư không phải quyết định mua

MRP Output -> Purchase Order | MRP chỉ là kết quả tính toán

Purchase Requirement -> Intake | Chưa có hàng thực tế

Intake -> QC_PASS | Hàng nhận chưa qua QC

QC_PASS -> READY_FOR_PRODUCTION | Readiness là action riêng

G1 Formula -> Production Order | Vẫn cần demand, MRP, stock, readiness và owner approval

## 9.3. Chuỗi đúng tổng quát

Chuỗi đúng của PACK-03 V2.0:

Production Demand -> Demand Board -> Pilot/G1 Formula Resolver nếu cần -> Production BOM Snapshot Draft -> MRP -> Material Requirement Board -> Procurement Suppression Check -> Procurement / Harvest Requirement -> Intake -> QC / Check -> Readiness -> Production Order Draft -> Owner Approval -> Production Order chính thức

## 10. HAI NGUỒN TẠO PRODUCTION DEMAND CHÍNH

Ginsengfood chỉ cho phép tạo Production Demand từ hai nhóm nguồn chính:

- STOCK_SALES_DEMAND

- DEALER_DISTRIBUTOR_ORDER

Nguồn khác nếu phát sinh phải đi qua Extension Governance, không tự mở nguồn demand mới.

## 11. STOCK_SALES_DEMAND

## 11.1. Định nghĩa

STOCK_SALES_DEMAND là nhu cầu sản xuất phát sinh từ tồn kho, kế hoạch bán hàng, chiến dịch kinh doanh hoặc tín hiệu vận hành bán hàng.

## 11.2. Nguồn con được phép

Nguồn con | Ý nghĩa

FINISHED_GOODS_LOW_STOCK | Tồn thành phẩm xuống thấp

SALES_PLAN | Kế hoạch bán hàng tuần/tháng

ADS_CAMPAIGN | Chiến dịch quảng cáo

GOLDEN_HOUR_PROGRAM | Phiên Giờ Vàng

CRM_CAMPAIGN | Chăm sóc/nhắc mua lại

LANDING_PAGE_CAMPAIGN | Nhu cầu từ landing page

FAST_MOVING_SKU | SKU bán nhanh

SEASONAL_COMBO_PLAN | Combo mùa vụ

MANAGEMENT_FORECAST | Dự báo điều hành

## 11.3. STOCK_SALES_DEMAND không được làm gì

STOCK_SALES_DEMAND không được:

- Tự tạo Production Order.

- Tự sửa công thức.

- Tự chọn nguyên liệu.

- Tự bỏ qua G1.

- Tự tạo BOM Snapshot.

- Tự mua nguyên liệu.

- Tự thu hoạch Sâm.

- Tự nhập kho.

- Tự pass QC.

- Tự vượt Sale Lock.

## 12. DEALER_DISTRIBUTOR_ORDER

## 12.1. Định nghĩa

DEALER_DISTRIBUTOR_ORDER là nhu cầu phát sinh từ đơn đại lý, nhà phân phối hoặc đơn sỉ có cam kết.

Đây là nguồn nhu cầu thương mại có trọng số cao, nhưng vẫn không được đi thẳng thành Production Order.

## 12.2. Nguyên tắc fulfillment-first

Dealer/Distributor Order phải kiểm tra tồn kho thành phẩm trước.

Nếu kho đủ, ưu tiên fulfillment từ kho.

Nếu kho thiếu, chỉ phần thiếu mới được tạo Production Demand.

Tình huống | Cách xử lý

Đơn 1.000 hộp, tồn khả dụng 1.200 hộp | Fulfillment từ kho, không tạo Production Demand

Đơn 1.000 hộp, tồn khả dụng 600 hộp | 600 hộp fulfillment, 400 hộp đưa vào Demand Board

SKU bị Sale Lock | Không fulfillment, không tạo demand để bán

SKU thiếu G1/BOM/config | Chặn trước MRP

Đơn lớn vượt ngưỡng tồn/thu mua | Cần Giám đốc phê duyệt, evidence, audit và MRP run

Quy tắc Giám đốc phê duyệt cho kế hoạch/đơn hàng lớn đã được khóa: phải có production plan hoặc dealer/distributor order lớn, MRP run, lý do vượt ngưỡng, evidence, audit và người phê duyệt; không được dùng override để mua sắm tùy tiện hoặc bỏ qua tồn kho đang đủ/vượt định mức.

## 13. PRODUCTION DEMAND BOARD

## 13.1. Vai trò

Production Demand Board là bảng trung tâm gom, chuẩn hóa, kiểm tra và phân loại nhu cầu sản xuất trước khi chuyển sang MRP.

Production Demand Board không phải Production Order Board.

Production Demand Board không được dùng để phát hành lệnh sản xuất.

## 13.2. Nhiệm vụ của Production Demand Board

Production Demand Board phải:

- Nhận demand từ STOCK_SALES_DEMAND.

- Nhận demand từ DEALER_DISTRIBUTOR_ORDER.

- Chuẩn hóa SKU.

- Chuẩn hóa requested quantity.

- Kiểm tra tồn thành phẩm.

- Tính shortage quantity.

- Xác định priority.

- Gắn demand reason.

- Kiểm tra Product/SKU/G1/Formula availability.

- Kiểm tra Sale Lock/Recall/Hold.

- Kiểm tra duplicate demand.

- Cho phép merge/split demand nếu hợp lệ.

- Chuyển demand đủ điều kiện sang MRP.

- Chặn demand thiếu điều kiện.

- Ghi audit và evidence.

## 13.3. Trường thông tin tối thiểu

Nhóm | Trường | Ý nghĩa

Identity | demand_id | Mã nhu cầu

Source | demand_source | STOCK_SALES_DEMAND / DEALER_DISTRIBUTOR_ORDER

Product | sku_id | SKU cần sản xuất

Quantity | requested_quantity | Số lượng nhu cầu

Time | requested_date | Ngày/kỳ cần đáp ứng

Priority | priority | Mức ưu tiên

Reason | demand_reason | Lý do nhu cầu

Sales Link | linked_sales_plan | Kế hoạch bán hàng nếu có

Dealer Link | linked_dealer_order | Đơn đại lý nếu có

Distributor Link | linked_distributor_order | Đơn nhà phân phối nếu có

Stock | current_stock | Tồn hiện tại

Stock | available_stock | Tồn khả dụng

Quantity | shortage_quantity | Số lượng thiếu

Formula | formula_status | G1 available / missing / bị chặn

Formula | formula_version | Version công thức

Planning | planning_status | Trạng thái planning

Review | owner_review_status | Trạng thái owner review

Audit | audit_reference | Dấu vết audit

Evidence | evidence_reference | Chứng cứ

## 13.4. Công thức shortage_quantity

shortage_quantity = requested_quantity - available_stock

Nếu shortage_quantity <= 0:

- Không tạo demand sản xuất mới.

- Fulfillment từ kho nếu là dealer/distributor order.

- Đóng demand với trạng thái không cần sản xuất.

- Ghi audit.

Nếu shortage_quantity > 0:

- Đưa vào Demand Board.

- Kiểm tra guard.

- Chuyển MRP nếu đạt điều kiện.

- Không phát Production Order trực tiếp.

## 14. DEMAND STATUS

## 14.1. planning_status

planning_status | Ý nghĩa

DRAFT | Demand mới ghi nhận

waiting_VALIDATION | Chờ kiểm tra dữ liệu

VALIDATED | Hợp lệ ở tầng đầu vào

STOCK_AVAILABLE | Tồn đủ, không cần sản xuất

SHORTAGE_DETECTED | Có thiếu hụt

FORMULA_CHECK_REQUIRED | Cần kiểm tra G1/formula

FORMULA_READY | G1/formula đủ điều kiện

FORMULA_BLOCKED | Thiếu hoặc lỗi formula

waiting_MRP | Chờ MRP

MRP_IN_PROGRESS | Đang tính MRP

bị chặn | Bị chặn

CANCELLED | Hủy hợp lệ

CLOSED_NO_PRODUCTION | Đóng vì không cần sản xuất

CLOSED_TRANSFERRED_TO_MRP | Đã chuyển sang MRP

CLOSED_TRANSFERRED_TO_PRODUCTION_DRAFT | Đã chuyển sang Production Order Draft

## 14.2. owner_review_status

owner_review_status | Ý nghĩa

NOT_REQUIRED_YET | Chưa cần duyệt

waiting_OWNER_REVIEW | Chờ owner review

OWNER_APPROVED_FOR_MRP | Duyệt chuyển MRP

OWNER_APPROVED_FOR_PRODUCTION_DRAFT | Duyệt tạo draft lệnh sản xuất

OWNER_REJECTED | Từ chối

OWNER_REQUESTED_REVISION | Yêu cầu chỉnh

AUTO_BLOCKED_BY_GUARD | Guard tự chặn

DIRECTOR_APPROVAL_REQUIRED | Cần Giám đốc phê duyệt

DIRECTOR_APPROVED | Giám đốc đã duyệt ngoại lệ

ESCALATED | Chuyển cấp duyệt cao hơn

## 15. DEMAND GUARD

## 15.1. Vai trò

Demand Guard kiểm tra demand trước khi được chuyển sang MRP.

Demand Guard không cho nhu cầu sai, thiếu, mơ hồ, bị khóa hoặc không đủ công thức đi tiếp.

## 15.2. Demand Guard P0

Guard | Điều kiện kiểm tra | Nếu fail

DEMAND_SOURCE_GUARD | Nguồn demand hợp lệ | bị chặn

SKU_CANONICAL_GUARD | SKU tồn tại và hợp lệ | bị chặn

PRODUCT_ACTIVE_GUARD | Product active đúng cách | bị chặn

G1_FORMULA_GUARD | Có G1 Approved Operational Formula | bị chặn

FORMULA_VERSION_GUARD | Formula version hợp lệ | bị chặn

ANCHOR_POLICY_GUARD | Nếu cần scale, phải có anchor policy | bị chặn

STOCK_AVAILABILITY_GUARD | Đọc được tồn khả dụng | bị chặn

SELLABLE_DEPENDENCY_GUARD | SKU phục vụ bán phải không bị non-sellable | bị chặn

SALE_LOCK_GUARD | Sale Lock thắng mọi demand | bị chặn

RECALL_GUARD | Recall lock thắng mọi demand | bị chặn

DUPLICATE_DEMAND_GUARD | Không tạo demand trùng | MERGE hoặc bị chặn

EVIDENCE_GUARD | Demand trọng yếu phải có evidence | waiting_REVIEW / bị chặn

OWNER_REVIEW_GUARD | Vượt ngưỡng phải có owner review | waiting_OWNER_REVIEW

DIRECTOR_APPROVAL_GUARD | Vượt ngưỡng đặc biệt phải có Giám đốc phê duyệt | DIRECTOR_APPROVAL_REQUIRED

## 15.3. Priority không được vượt guard

Dù demand là P0_CRITICAL, guard vẫn thắng.

Không được vì:

- Đơn lớn.

- Chiến dịch lớn.

- Giờ Vàng.

- Đại lý cam kết.

- Nhà phân phối yêu cầu.

- Tồn kho thấp.

mà bỏ qua SKU, G1, BOM, MRP, Sale Lock, Recall, Evidence hoặc Owner Approval.

## 16. FORMULA RESOLUTION TRƯỚC MRP

## 16.1. Khi nào cần Formula Resolution

Trước khi MRP, hệ thống phải xác định công thức được dùng.

Formula Resolution bắt buộc khi:

- Demand chuyển sang MRP.

- Production Order Draft được tạo.

- Số kg gạo / anchor quantity được nhập.

- Công thức cần scale.

- Ngưỡng tồn Nhóm A cần tính động.

- B1/B2 cần tính theo output/yield.

## 16.2. Formula Resolution trả về gì

Formula Resolution phải trả về:

Kết quả | Ý nghĩa

resolved_sku_id | SKU được sản xuất

resolved_formula_version | G1 hoặc version active

formula_status | Approved / bị chặn / missing

anchor_ingredient | Gạo

anchor_quantity_policy | Phạm vi được phép nhập

ratio_to_rice_lines | Tỷ lệ nguyên liệu theo gạo

uom_policy | Quy đổi đơn vị

rounding_policy | Làm tròn

yield_policy | Sản lượng gói/hộp/thùng nếu có

calculation_snapshot_required | Có cần snapshot

formula_guard_result | Kết quả guard

## 16.3. Không có Formula Resolution thì không MRP

Nếu không resolve được công thức, MRP phải chặn.

Các trạng thái chặn:

Tình huống | Trạng thái

SKU thiếu G1 | FORMULA_BLOCKED

Formula chưa approved | FORMULA_NOT_APPROVED

Thiếu anchor policy | ANCHOR_CONFIG_MISSING

Thiếu ratio_to_rice | FORMULA_RATIO_MISSING

Thiếu UOM policy | UOM_CONFIG_MISSING

Thiếu density policy bắt buộc | DENSITY_CONFIG_MISSING

Formula đang retired/superseded | FORMULA_INACTIVE

Formula bị quality hold | FORMULA_HOLD

## 17. DEMAND EVIDENCE

## 17.1. Evidence bắt buộc

Loại demand | Evidence tối thiểu

Stock-based demand | Stock snapshot, available stock, sellable status

Sales plan demand | Kế hoạch bán hàng

ADS demand | Campaign plan/forecast

Golden Hour demand | Kế hoạch phiên

CRM demand | CRM plan/cohort

Dealer order demand | Đơn đại lý

Distributor order demand | Đơn nhà phân phối

Bulk order demand | Cam kết số lượng/điều kiện thương mại

Formula-driven demand | G1/formula reference

Large-plan exception | Giám đốc phê duyệt, MRP run, evidence, audit

## 17.2. Không evidence thì không approved

Demand thiếu evidence không được gọi approved.

Demand thiếu evidence phải ở một trong các trạng thái:

- DRAFT.

- waiting_VALIDATION.

- waiting_OWNER_REVIEW.

- WAITING EVIDENCE.

## 18. DEMAND AUDIT

## 18.1. Audit bắt buộc

Mọi hành động trên Demand Board phải audit:

- Tạo demand.

- Sửa số lượng.

- Sửa SKU.

- Sửa requested date.

- Sửa priority.

- Gắn evidence.

- Gắn formula reference.

- Chạy Formula Resolution.

- Merge demand.

- Split demand.

- Chuyển trạng thái.

- Owner approve/reject.

- Director approve/reject.

- Chuyển sang MRP.

- Cancel.

- Close.

## 18.2. Audit không thay evidence

Audit chỉ ghi nhận thao tác.

Evidence chứng minh căn cứ nghiệp vụ.

Có audit nhưng thiếu evidence thì vẫn không được gọi PASS.

## 19. RULE TÓM TẮT PHẦN 1/4

Rule ID | Quy tắc | Mức độ

P03-FRM-P0-001 | Gạo là anchor_ingredient của công thức pilot | P0

P03-FRM-P0-002 | Pilot được nhập kg/g/l/ml và hệ thống tự tính ratio_to_rice | P0

P03-FRM-P0-003 | Pilot được thêm/bỏ nguyên liệu trước khi khóa G1 | P0

P03-FRM-P0-004 | Pilot được duyệt thì khóa thành G1 Approved Operational Formula | P0

P03-FRM-P0-005 | Sau khi khóa G1, bộ phận sản xuất không được sửa công thức | P0

P03-FRM-P0-006 | Sản xuất chỉ được nhập anchor_rice_quantity trong phạm vi policy | P0

P03-FRM-P0-007 | Scale số kg gạo không phải sửa công thức | P0

P03-FRM-P0-008 | Đổi tỷ lệ/thêm/bỏ nguyên liệu sau G1 phải tạo Formula Version mới | P0

P03-FRM-P0-009 | Production BOM Snapshot là bắt buộc | P0

P03-FRM-P0-010 | Field App chỉ hiển thị snapshot, không sửa công thức | P0

P03-FRM-P0-011 | Buffer MRP không được sửa công thức sản xuất | P0

P03-DEM-P0-001 | Production Demand không phải Production Order | P0

P03-DEM-P0-002 | Sales/Dealer/Distributor/Stock Alert không được đi thẳng Production Order | P0

P03-DEM-P0-003 | Dealer/Distributor phải fulfillment-first | P0

P03-DEM-P0-004 | Demand phải qua Demand Board | P0

P03-DEM-P0-005 | Demand muốn sang MRP phải qua Formula Resolution | P0

P03-DEM-P0-006 | Sale Lock/Recall/Hold thắng mọi demand | P0

P03-DEM-P0-007 | Demand thiếu evidence không được approved | P0

P03-DEM-P0-008 | Vượt ngưỡng đặc biệt phải có Giám đốc phê duyệt | P0

## 20. ACCEPTANCE CRITERIA CHO PHẦN 1/4

## PHẦN 1/4 được coi là đạt governance khi:

- Đã xác định vị trí PACK-03 V2.0 trong bộ hồ sơ.

- Đã khóa source-of-truth.

- Đã phân biệt G0, Pilot, G1 và version sau G1.

- Đã khóa gạo là anchor_ingredient.

- Đã khóa pilot nhập nguyên liệu và tự tính ratio_to_rice.

- Đã khóa G1 Production Formula Lock.

- Đã khóa bộ phận sản xuất không được sửa G1.

- Đã khóa scale mẻ không phải sửa công thức.

- Đã khóa Production BOM Snapshot.

- Đã khóa buffer MRP không sửa công thức sản xuất.

- Đã khóa Production Demand không phải Production Order.

- Đã khóa hai nguồn demand chính.

- Đã khóa Production Demand Board.

- Đã khóa Demand Guard.

- Đã khóa Formula Resolution trước MRP.

- Đã khóa Evidence/Audit cho demand.

- Chưa gọi Production Ready.

- Chưa gọi Release Approved.

- Chưa gọi Go-live Approved.

## 21. TRẠNG THÁI SAU PHẦN 1/4

Hạng mục | Trạng thái

PACK-03 PHẦN 1/4 Documentation | GOVERNANCE_DRAFT_COMPLETE

Source-of-Truth | LOCKED_IN_DOCUMENTATION

G0 / Pilot / G1 Boundary | LOCKED_IN_DOCUMENTATION

Rice Anchor Formula Governance | LOCKED_IN_DOCUMENTATION

G1 Production Formula Lock | LOCKED_IN_DOCUMENTATION

Production BOM Snapshot Lock | LOCKED_IN_DOCUMENTATION

Demand Principles | LOCKED_IN_DOCUMENTATION

Production Demand Board | LOCKED_IN_DOCUMENTATION

Demand Guard | LOCKED_IN_DOCUMENTATION

Formula Resolution before MRP | LOCKED_IN_DOCUMENTATION

Evidence / Audit Requirement | LOCKED_IN_DOCUMENTATION

Demand Board Runtime | waiting_IMPLEMENTATION

Formula Resolver Runtime | waiting_IMPLEMENTATION

Evidence Packet | waiting_EVIDENCE

Smoke Run | waiting_SMOKE

Owner Sign-off | waiting_OWNER_SIGNOFF

Production Ready | KHÔNG

Release Approved | KHÔNG

Go-live Approved | KHÔNG

## 22. KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 của PACK-03 V2.0 khóa nền governance quan trọng nhất cho toàn bộ chuỗi Demand -> Formula Scaling -> MRP -> Procurement -> Production.

Từ thời điểm này, Ginsengfood không cho phép sản xuất theo cảm tính, không cho phép sales/dealer/stock alert đi thẳng thành lệnh sản xuất, không cho phép bộ phận sản xuất sửa công thức G1 và không cho phép MRP chạy khi chưa resolve được công thức hợp lệ.

Kết luận khóa của PHẦN 1/4:

Pilot được phép thử nghiệm, nhưng G1 đã phê duyệt thì phải khóa.

Bộ phận sản xuất chỉ được nhập số kg gạo trong phạm vi được phép; hệ thống tự scale công thức theo tỷ lệ đã khóa và tạo Production BOM Snapshot.

Scale mẻ không phải sửa công thức.

Mọi thay đổi tỷ lệ, nguyên liệu, đơn vị hoặc cấu trúc công thức sau G1 đều phải tạo Formula Version mới và được phê duyệt riêng.

Mọi nhu cầu sản xuất phải đi qua Production Demand Board, Demand Guard, Formula Resolution, Evidence và Audit trước khi được chuyển sang MRP.

## PHẦN 2/4 — MATERIAL REQUIREMENT PLANNING / DYNAMIC THRESHOLD RESOLVER / MATERIAL REQUIREMENT BOARD / PROCUREMENT SUPPRESSION CONTROL

## 1. MỤC TIÊU CỦA PHẦN 2/4

## PHẦN 2/4 khóa lớp Material Requirement Planning — MRP theo mô hình mới của Ginsengfood.

MRP không chỉ bóc BOM và tính thiếu nguyên liệu. Trong PACK-03 V2.0, MRP phải làm đủ các việc sau:

- Đọc Demand đã qua Production Demand Board.

- Đọc Formula Resolution.

- Đọc G1 Approved Operational Formula.

- Đọc Production BOM Snapshot / Calculation Snapshot.

- Tính nhu cầu nguyên liệu Nhóm A theo công thức đã scale.

- Tính nhu cầu bao bì/vật tư Nhóm B theo sản lượng gói/hộp/thùng thực tế.

- Tách công thức sản xuất và buffer kế hoạch.

- Tính tồn khả dụng.

- Loại trừ hàng hold, recall, reject, expired, disposal waiting.

- Tính shortage.

- Tính dynamic suppression threshold.

- Xác định nguyên liệu/vật tư nào được phép đưa vào phiếu thu mua.

- Tự loại nguyên liệu/vật tư đủ hoặc vượt ngưỡng khỏi phiếu thu mua.

- Gắn evidence, audit và owner review.

- Không tự tạo Purchase Order.

- Không tự tạo Harvest Requirement chính thức nếu chưa qua owner/guard.

- Không tự phát Production Order.

MRP là lớp tính toán và kiểm soát nhu cầu nguyên vật liệu.

MRP không phải lệnh sản xuất.

MRP không phải đơn mua hàng.

MRP không phải lệnh thu hoạch.

MRP không phải phiếu nhập kho.

MRP không phải QC.

MRP không phải readiness.

## 2. NGUYÊN TẮC P0 CỦA MRP V2.0

## 2.1. MRP bắt buộc sau Demand Board và Formula Resolution

Không được chạy MRP nếu chưa có:

- Production Demand hợp lệ.

- SKU hợp lệ.

- G1 Approved Operational Formula hợp lệ.

- Formula Resolution thành công.

- Anchor/rice policy rõ.

- Production BOM Snapshot hoặc Calculation Snapshot nháp.

- Yield/Packaging Output policy nếu cần tính bao bì.

- Stock source hợp lệ.

- Evidence và audit nền.

Nếu thiếu một trong các dữ liệu trên, MRP phải chặn.

## 2.2. MRP không sửa công thức

MRP không được sửa:

- G1 Formula.

- Tỷ lệ nguyên liệu theo gạo.

- Production BOM Snapshot.

- Yield policy.

- Packaging Config.

- Print Config.

- Buffer policy.

MRP chỉ đọc dữ liệu đã được resolve và tính nhu cầu.

## 2.3. Buffer MRP không phải công thức sản xuất

Tỷ lệ +5% của Nhóm A và +7% của Nhóm B1/B2 là buffer kế hoạch/MRP, không phải định lượng công thức sản xuất. File owner lock đã xác định các tỷ lệ +5% và +7% là cấu hình kế hoạch ban đầu để lập MRP, không hardcode trong code và mọi thay đổi phải có audit/evidence/owner approval.

Do đó:

- Công thức sản xuất dùng số lượng theo G1 đã scale.

- MRP có thể cộng buffer để chuẩn bị tồn/mua/thu hoạch.

- Buffer không được ghi ngược vào công thức G1.

- Buffer không được sửa Production BOM Snapshot.

## 2.4. MRP không tự tạo Purchase Order

MRP output chỉ là kết quả tính toán và đề xuất.

MRP không được tự tạo:

- Purchase Order.

- Harvest Execution.

- Raw Material Intake.

- Packaging Intake.

- QC_PASS.

- READY_FOR_PRODUCTION.

- READY_FOR_PACKAGING.

- Production Order chính thức.

## 3. CHUỖI ĐÚNG CỦA MRP V2.0

Chuỗi đúng:

Production Demand Board -> Formula Resolution -> Production BOM Snapshot Draft -> MRP Run -> Material Requirement Board -> Dynamic Threshold Resolver -> Procurement Suppression Check -> Owner Review -> Purchase / Harvest / Packaging Requirement nếu được phép

Không được đi tắt từ:

Đường đi bị cấm | Lý do

Demand Board -> Purchase Order | Chưa qua MRP và suppression check

Formula Resolution -> Material Issue | Chưa có MRP/readiness

MRP Run -> Purchase Order | MRP không phải quyết định mua

MRP Shortage -> Purchase Order | Shortage cần owner/procurement gate

Material Alert -> Purchase Order | Alert không phải quyết định mua

Stock Alert -> Production Order | Alert không phải lệnh sản xuất

Approved Incoming -> READY_FOR_PRODUCTION | Incoming chưa intake/QC/readiness

QC_PASS -> Material Issue | QC_PASS chưa đủ nếu readiness chưa hoàn tất

## 4. INPUT BẮT BUỘC CỦA MRP

## 4.1. Input từ Production Demand Board

MRP phải nhận tối thiểu:

Input | Ý nghĩa

demand_batch_id | Nhóm demand được dùng để tính

demand_source_summary | Tổng hợp nguồn demand

sku_list | Danh sách SKU

planned_quantity_by_sku | Số lượng cần sản xuất theo SKU

shortage_quantity_by_sku | Phần thiếu sau kiểm tra tồn thành phẩm

planning_period | Kỳ sản xuất/kỳ đáp ứng

priority | Mức ưu tiên

owner_review_status | Trạng thái owner review

demand_evidence_reference | Evidence demand

demand_audit_reference | Audit demand

## 4.2. Input từ Formula Resolution

MRP bắt buộc nhận kết quả Formula Resolution:

Input | Ý nghĩa

resolved_formula_version | G1 hoặc formula version active

formula_status | Approved / bị chặn / missing

anchor_ingredient | Gạo

anchor_rice_quantity | Số kg gạo dùng để scale

ratio_to_rice_lines | Tỷ lệ nguyên liệu theo gạo

calculated_material_quantity_per_batch | Số lượng nguyên liệu đã tính cho một mẻ

uom_policy | Chính sách đơn vị

density_policy | Chính sách density nếu có chất lỏng

rounding_policy | Chính sách làm tròn

calculation_snapshot | Snapshot tính toán

production_bom_snapshot_reference | Tham chiếu BOM Snapshot

formula_guard_result | Kết quả Formula Guard

## 4.3. Input từ Product / Packaging / Print Config

MRP phải đọc:

Input | Ý nghĩa

Packaging Config | Cấu hình bao bì cấp 1/cấp 2

Print Config | Cấu hình in nếu có

pack_output_per_batch | Sản lượng gói/phần ăn/mẻ

box_output_per_batch | Sản lượng hộp/mẻ

carton_output_per_batch | Sản lượng thùng/mẻ

B1 buffer rate | Buffer bao bì cấp 1

B2 buffer rate | Buffer hộp/thùng

B3 Stock Policy | Tồn thực tế/vật tư dùng chung

trade_item_config | Hộp/thùng/trade item nếu liên quan

Nhóm B không nhảy trực tiếp theo kg gạo mà nhảy theo sản lượng chuẩn sau khi công thức/mẻ sản xuất đã được xác định. File owner lock đã khóa rõ: khi xác định được một mẻ ra bao nhiêu gói, hộp, thùng, hệ thống tự tính nhu cầu B1/B2 và ngưỡng tồn tương ứng.

## 4.4. Input từ Inventory / Warehouse / Operational Core

MRP phải đọc:

Input | Ý nghĩa

current_stock | Tồn hiện tại

available_stock | Tồn khả dụng

ready_stock | Tồn đã ready

reserved_stock | Tồn đã giữ

hold_stock | Tồn bị hold

recall_locked_stock | Tồn bị recall lock

qc_waiting_stock | Tồn chờ QC

rejected_stock | Tồn bị reject

disposal_waiting_stock | Tồn chờ hủy

expired_stock | Tồn hết hạn

approved_incoming | Incoming đã duyệt

warehouse_location | Kho/vị trí

lot_status | Trạng thái lot

readiness_status | READY_FOR_PRODUCTION / READY_FOR_PACKAGING

## 5. CÔNG THỨC MRP V2.0

## 5.1. Lớp 1 — Production Formula Requirement

Production Formula Requirement là số lượng nguyên liệu thật theo G1 đã scale.

Công thức:

production_required_quantity = anchor_rice_quantity × ratio_to_rice

Với chất lỏng, nếu lưu theo lít/kg gạo hoặc ml/kg gạo thì tính theo usage basis đã khóa, không ép đổi sang kg nếu chưa có density config.

## 5.2. Lớp 2 — MRP Gross Requirement

Gross Requirement là nhu cầu trước khi trừ tồn.

Với Nhóm A:

gross_requirement_A = production_required_quantity × planned_batch_count

Với Nhóm B1:

gross_requirement_B1 = pack_output_per_batch × planned_batch_count

Với Nhóm B2 hộp:

gross_requirement_B2_box = box_output_per_batch × planned_batch_count

Với Nhóm B2 thùng:

gross_requirement_B2_carton = carton_output_per_batch × planned_batch_count

Với B3:

gross_requirement_B3 = consumption_policy hoặc Stock Policy nếu có

## 5.3. Lớp 3 — MRP Buffer Requirement

Buffer được áp dụng theo config:

Nhóm | Buffer mặc định | Ghi chú

A1/A2/A3 | 5% | Buffer MRP, không sửa công thức

B1 | 7% | Buffer bao bì cấp 1

B2 | 7% | Buffer hộp/thùng

B3 | 0% mặc định | Theo Stock Policy thực tế

Công thức:

buffered_requirement = gross_requirement × (1 + buffer_rate)

## 5.4. Lớp 4 — Net Requirement

Net Requirement là nhu cầu ròng sau khi trừ tồn khả dụng và incoming đã duyệt.

Công thức:

net_requirement = buffered_requirement - available_stock - approved_incoming + reserved_adjustment nếu có

Nếu net_requirement <= 0:

- Không tạo nhu cầu mua/thu hoạch mới.

- Chuyển trạng thái COVERED_BY_STOCK hoặc COVERED_BY_APPROVED_INCOMING.

- Vẫn phải qua suppression check để tránh đưa vào phiếu thu mua.

Nếu net_requirement > 0:

- Tạo shortage_quantity.

- Đưa vào Material Requirement Board.

- Chưa tự tạo Purchase Order.

## 5.5. Lớp 5 — Procurement Suppression Check

Sau khi tính net requirement, hệ thống phải chạy suppression check.

Công thức khóa:

Nếu available_stock + approved_incoming >= dynamic_suppression_threshold -> loại khỏi phiếu thu mua

Nguyên tắc này đã được khóa trong file owner lock: phiếu thu mua không được chứa nguyên liệu/vật tư chỉ vì có yêu cầu nếu tồn khả dụng + incoming đã duyệt đã vượt ngưỡng kiểm soát.

## 6. MATERIAL REQUIREMENT BOARD

## BẢNG NHU CẦU NGUYÊN VẬT LIỆU

## 6.1. Vai trò

Material Requirement Board là bảng trung tâm hiển thị kết quả MRP.

Board này không phải Purchase Order.

Board này không phải phiếu thu mua chính thức.

Board này dùng để:

- Hiển thị nhu cầu theo từng material.

- Hiển thị công thức/yield làm căn cứ.

- Hiển thị gross requirement.

- Hiển thị buffer.

- Hiển thị available stock.

- Hiển thị approved incoming.

- Hiển thị net requirement.

- Hiển thị dynamic suppression threshold.

- Hiển thị trạng thái được phép mua hay bị loại khỏi phiếu thu mua.

- Hiển thị source type.

- Gắn owner review.

- Gắn evidence/audit.

## 6.2. Trường thông tin tối thiểu

Nhóm | Trường | Ý nghĩa

Identity | material_requirement_id | Mã nhu cầu vật tư

Planning | planning_period | Kỳ kế hoạch

Demand | demand_batch_id | Demand batch liên quan

Formula | formula_version | G1/version dùng tính

Formula | production_bom_snapshot_reference | Snapshot công thức sản xuất

SKU | sku_list | Danh sách SKU

Quantity | planned_quantity_by_sku | Sản lượng dự kiến

Material | material_id | Material canonical

Material | material_type | A1/A2/A3/B1/B2/B3

Requirement | production_required_quantity | Lượng theo công thức sản xuất

Requirement | gross_requirement | Nhu cầu trước buffer

Buffer | buffer_rate | Tỷ lệ buffer

Requirement | buffered_requirement | Nhu cầu sau buffer

Stock | current_stock | Tồn hiện tại

Stock | available_stock | Tồn khả dụng

Stock | ready_stock | Tồn ready

Stock | reserved_quantity | Tồn reserved

Stock | hold_quantity | Tồn hold

Stock | recall_locked_quantity | Tồn recall lock

Stock | disposal_waiting_quantity | Tồn chờ hủy

Incoming | approved_incoming | Incoming đã duyệt

Shortage | net_requirement | Nhu cầu ròng

Suppression | dynamic_suppression_threshold | Ngưỡng loại khỏi phiếu thu mua

Suppression | procurement_allowed | Có được đưa vào phiếu thu mua không

Suppression | suppression_reason | Lý do loại/chặn

Source | source_type | COMPANY_SOURCE / SUPPLIER_SOURCE / PACKAGING_SUPPLIER

Source | supplier_id | Nhà cung cấp nếu có

Source | company_source_id | Nguồn công ty nếu có

Status | status | Trạng thái

Review | owner_review_status | Owner review

Audit | audit_reference | Audit

Evidence | evidence_reference | Evidence

## 7. DYNAMIC THRESHOLD RESOLVER

## 7.1. Vai trò

Dynamic Threshold Resolver tính ngưỡng tồn kho và ngưỡng loại khỏi phiếu thu mua theo công thức, sản lượng, material group và stock policy.

Resolver này bắt buộc vì PACK-03 V2.0 không dùng ngưỡng chết cho mọi trường hợp.

Ngưỡng phải tự nhảy khi:

- Anchor rice quantity thay đổi.

- G1 Formula scale ra số lượng mới.

- calculated_material_quantity_per_batch thay đổi.

- Yield gói/hộp/thùng thay đổi.

- SKU mới được thêm.

- Planning basket thay đổi.

- Owner thay đổi Stock Policy.

- B1/B2 output thay đổi.

## 7.2. Resolver trả về gì

Output | Ý nghĩa

material_id | Material canonical

material_group | A1/A2/A3/B1/B2/B3

threshold_type | A_SPECIFIC / A_COMMON / SAVIGIN / B1 / B2 / B3

threshold_basis | Cơ sở tính

dynamic_suppression_threshold | Ngưỡng loại khỏi phiếu thu mua

calculation_source | Formula / yield / stock policy

effective_date | Ngày hiệu lực

owner_policy_reference | Chính sách owner

guard_result | Kết quả guard

audit_reference | Audit

evidence_reference | Evidence

## 7.3. Nếu không tính được threshold

Nếu resolver không tính được threshold, hệ thống không được đoán.

Các trường hợp chặn:

Thiếu dữ liệu | Trạng thái

Thiếu calculated_material_quantity_per_batch | BLOCKED_FORMULA_CALCULATION_MISSING

Thiếu ratio_to_rice | BLOCKED_FORMULA_RATIO_MISSING

Thiếu UOM conversion | BLOCKED_UOM_CONFIG_MISSING

Thiếu density policy bắt buộc | BLOCKED_DENSITY_CONFIG_MISSING

Thiếu yield output | BLOCKED_YIELD_CONFIG_MISSING

Thiếu Stock Policy B3 | BLOCKED_STOCK_POLICY_MISSING

Thiếu material group | BLOCKED_MATERIAL_GROUP_MISSING

## 8. NHÓM A — DYNAMIC THRESHOLD

## 8.1. Nhóm A là nguyên liệu sản xuất

Nhóm A gồm:

- A1 — Nguyên liệu Quân–Thần–Tá–Sứ.

- A2 — Nguyên liệu nền dinh dưỡng + rau củ chiết dịch nước hầm.

- A3 — Nguyên liệu nêm & tạo hương vị.

Nhóm A áp dụng buffer MRP mặc định 5%, nhưng buffer này không sửa công thức sản xuất.

## 8.2. A_SPECIFIC_5_BATCH_SKU

Áp dụng cho nguyên liệu đặc thù theo SKU.

Ngưỡng khóa:

dynamic_suppression_threshold = calculated_material_quantity_per_batch × 5 mẻ/SKU

Quy tắc:

Điều kiện | Hành động

available_stock + approved_incoming < dynamic_suppression_threshold | Được đưa vào danh mục xét nhập/mua nếu có MRP demand

available_stock + approved_incoming >= dynamic_suppression_threshold | Loại khỏi phiếu thu mua

Muốn mua vượt ngưỡng | Cần Giám đốc phê duyệt + MRP run + evidence + audit

Thiếu formula calculation | BLOCKED_CONFIG_MISSING

File owner lock đã khóa nhóm A đặc thù theo SKU dùng ngưỡng 5 mẻ/SKU, công thức là BOM_G1(material, SKU, mẻ 400) × 5; dưới ngưỡng mới được xét nhập/mua, bằng hoặc vượt ngưỡng thì loại khỏi phiếu thu mua.

## 8.3. A_COMMON_20_BATCH_POOL

Áp dụng cho nguyên liệu dùng chung nhiều SKU.

Ngưỡng khóa:

dynamic_suppression_threshold = aggregate calculated_material_quantity của active/planned SKU basket trong 20 mẻ chuẩn

Nguyên tắc:

- Không tính 5 mẻ riêng cho từng SKU.

- Không mua trùng vì cùng một nguyên liệu xuất hiện ở nhiều usage_role.

- Material Master chỉ có một material canonical.

- BOM line ghi usage_role để trace/costing/MRP.

- Threshold tính theo material canonical.

- Nếu tồn trên ngưỡng 20 mẻ aggregate thì loại khỏi phiếu thu mua.

File owner lock đã khóa rõ: nguyên liệu dùng chung nhiều SKU áp dụng ngưỡng tổng 20 mẻ chuẩn aggregate, không tính 5 mẻ riêng lẻ từng SKU để tránh mua trùng.

## 8.4. Sâm Savigin — SAVIGIN_STRATEGIC_RESERVE

Sâm Savigin là Company Source / vùng trồng công ty.

Sâm Savigin không đưa vào phiếu thu mua như nguyên liệu supplier.

Nguyên tắc:

- Sâm Savigin có mùa vụ/chu kỳ thu hoạch.

- Đến kỳ thu hoạch vẫn cho harvest/sơ chế/intake/QC/lot/readiness.

- Sâm sau nhập kho là Strategic Reserve Stock.

- MRP chỉ cảnh báo thiếu Sâm cho sản xuất khi ready stock dưới production safety threshold hoặc forecast thiếu.

- Không tạo Purchase Requirement cho Sâm như hàng mua ngoài.

- Không áp dụng suppression threshold kiểu supplier material để chặn harvest mùa vụ.

- Nếu tồn dự trữ cao, hệ thống đưa Owner Review, không tự hủy harvest cycle.

File owner lock đã xác định Sâm Savigin là Company Source, theo mùa vụ và tồn dự trữ chiến lược; không là Purchase Requirement, đến kỳ thu hoạch vẫn cho harvest/sơ chế/nhập kho bảo quản, chỉ cảnh báo thiếu cho sản xuất khi ready stock dưới ngưỡng an toàn hoặc forecast thiếu.

## 9. NHÓM B — DYNAMIC PACKAGING THRESHOLD

## 9.1. Nhóm B không tính trực tiếp theo kg gạo

Nhóm B gồm:

- B1 — Bao bì đóng gói cấp 1.

- B2 — Hộp/thùng đóng gói cấp 2.

- B3 — Vật tư dùng chung.

Nhóm B tính theo sản lượng thực tế của mẻ:

- pack_output_per_batch.

- box_output_per_batch.

- carton_output_per_batch.

- consumption_per_batch nếu có.

- Stock Policy.

Không tính B1/B2 trực tiếp theo kg gạo.

## 9.2. B1_SKU_15_BATCH

B1 là cuộn màng/bao bì đóng gói cấp 1 theo SKU.

Công thức động:

B1_requirement_per_batch = pack_output_per_batch × (1 + B1_buffer_rate)

Ngưỡng loại khỏi phiếu thu mua:

B1_dynamic_suppression_threshold = B1_requirement_per_batch × 15 mẻ/SKU

Nếu dùng mẻ chuẩn 400 theo baseline:

- pack_output_per_batch = 7.200 gói.

- B1 buffer = 7%.

- B1_requirement_per_batch = 7.704 gói quy đổi.

- Ngưỡng 15 mẻ = 115.560 gói quy đổi/SKU.

File owner lock đã khóa B1 theo SKU dùng ngưỡng 15 mẻ/SKU, với baseline 7.704 gói quy đổi/mẻ × 15 = 115.560 gói quy đổi/SKU.

## 9.3. B2_BOX_SKU_15_BATCH

B2 hộp là hộp sản phẩm theo SKU.

Công thức động:

B2_box_requirement_per_batch = box_output_per_batch × (1 + B2_buffer_rate)

Ngưỡng loại khỏi phiếu thu mua:

B2_box_dynamic_suppression_threshold = B2_box_requirement_per_batch × 15 mẻ/SKU

Nếu dùng mẻ chuẩn 400 theo baseline:

- box_output_per_batch = 1.800 hộp.

- B2 buffer = 7%.

- B2_box_requirement_per_batch = 1.926 hộp.

- Ngưỡng 15 mẻ = 28.890 hộp/SKU.

## 9.4. B2_CARTON_SKU_15_BATCH

B2 thùng là thùng carton theo SKU.

Công thức động:

B2_carton_requirement_per_batch = carton_output_per_batch × (1 + B2_buffer_rate)

Ngưỡng loại khỏi phiếu thu mua:

B2_carton_dynamic_suppression_threshold = B2_carton_requirement_per_batch × 15 mẻ/SKU

Nếu dùng mẻ chuẩn 400 theo baseline:

- carton_output_per_batch = 300 thùng.

- B2 buffer = 7%.

- B2_carton_requirement_per_batch = 321 thùng.

- Ngưỡng 15 mẻ = 4.815 thùng/SKU.

File owner lock đã khóa B2 hộp 1.926 hộp/mẻ × 15 = 28.890 hộp/SKU và B2 thùng 321 thùng/mẻ × 15 = 4.815 thùng/SKU.

## 9.5. B3_ACTUAL_STOCK_POLICY

B3 là vật tư dùng chung như:

- Tem dùng chung.

- Tem QC/release nếu dùng.

- Băng keo.

- Mực in date/lot/HSD.

- Ribbon/mực in mã vạch/QR.

- Decal/nhãn dùng chung.

- Dung môi/vật tư vệ sinh máy in.

- Dao cắt/keo/vật tư phụ.

B3 không tự cộng 7%.

B3 xử lý theo Stock Policy thực tế.

Nếu có consumption_per_batch thì owner cấu hình riêng.

Nếu tồn đủ hoặc vượt policy thì loại khỏi phiếu mua.

## 10. PROCUREMENT SUPPRESSION CONTROL

## 10.1. Mục tiêu

Procurement Suppression Control đảm bảo hệ thống không mua sắm chỉ vì có yêu cầu.

Hệ thống phải kiểm tra tồn kho và tự loại nguyên liệu/vật tư đủ hoặc vượt ngưỡng khỏi phiếu thu mua.

## 10.2. Công thức khóa

suppression_check_quantity = available_stock + approved_incoming

Nếu:

suppression_check_quantity >= dynamic_suppression_threshold

thì:

procurement_allowed = KHÔNG

và material bị loại khỏi phiếu thu mua.

Nếu:

suppression_check_quantity < dynamic_suppression_threshold

thì:

procurement_allowed = REVIEW_REQUIRED

và material chỉ được đưa vào danh mục xét nhập/mua nếu MRP demand và guard đạt.

## 10.3. Các trạng thái suppression

Status | Ý nghĩa | Hành động

PROCUREMENT_ALLOWED_FOR_REVIEW | Có thể đưa vào danh mục xét nhập/mua | Chờ owner/procurement review

PROCUREMENT_NOT_REQUIRED | Tồn đủ, chưa cần mua | Loại khỏi phiếu thu mua

PROCUREMENT_SUPPRESSED_BY_THRESHOLD | Vượt ngưỡng loại khỏi phiếu thu mua | Không mua thêm

PROCUREMENT_SUPPRESSED_BY_INCOMING | Incoming đã duyệt đủ | Không mua thêm

OWNER_REVIEW_REQUIRED | Cần owner review | Không tự PO

DIRECTOR_APPROVAL_REQUIRED | Vượt ngưỡng đặc biệt cần Giám đốc duyệt | Chặn nếu chưa duyệt

BLOCKED_CONFIG_MISSING | Thiếu config | Không đi tiếp

BLOCKED_BY_HOLD_RECALL_DISPOSAL | Tồn không usable hoặc bị khóa | Không tính usable

## 10.4. Vượt ngưỡng đặc biệt

Nếu có kế hoạch sản xuất lớn, đơn đại lý/nhà phân phối lớn hoặc chiến dịch đã được phê duyệt làm phát sinh nhu cầu vượt ngưỡng thông thường, việc đưa vật tư đã vượt ngưỡng vào phiếu thu mua chỉ được thực hiện khi có:

- Production Plan hoặc Dealer/Distributor Order lớn đã xác nhận.

- MRP run hợp lệ.

- Lý do vượt ngưỡng.

- Evidence.

- Audit.

- Giám đốc phê duyệt.

Không được dùng override để mua sắm tùy tiện.

Không được gọi Purchase Order chính thức nếu chưa qua Procurement Requirement, owner approval và procurement gate.

## 11. AVAILABLE STOCK CONTROL

## 11.1. Tồn hiện tại không đồng nghĩa tồn khả dụng

MRP phải phân biệt:

Khái niệm | Ý nghĩa

current_stock | Tồn hiện tại trên hệ thống

available_stock | Tồn khả dụng sau khi trừ các khóa

ready_stock | Tồn đạt readiness

reserved_stock | Tồn đã giữ

hold_stock | Tồn hold

recall_locked_stock | Tồn recall lock

qc_waiting_stock | Tồn chờ QC

rejected_stock | Tồn reject

disposal_waiting_stock | Tồn chờ hủy

expired_stock | Tồn hết hạn

damaged_stock | Tồn hư hỏng

approved_incoming | Incoming đã duyệt

## 11.2. Không được tính là usable

Không tính vào available_stock/ready_stock:

- Hàng chưa intake.

- Hàng chờ QC.

- Hàng QC_HOLD.

- Hàng QC_REJECT.

- Hàng hết hạn.

- Hàng bị hư hỏng.

- Hàng bị recall lock.

- Hàng bị sale lock nếu liên quan thành phẩm.

- Hàng đang chờ hủy.

- Hàng đã duyệt hủy.

- Hàng đã disposal executed nhưng chưa write-off.

- Hàng sai source/spec/grade.

- Hàng sai dietary guard.

- Hàng chưa READY_FOR_PRODUCTION hoặc READY_FOR_PACKAGING.

File owner lock đã khóa rằng hàng chờ hủy hoặc đã duyệt hủy không được tính vào available_stock, ready_stock hoặc suppression threshold để tránh hệ thống hiểu nhầm là còn vật tư usable.

## 12. MATERIAL REQUIREMENT STATUS

Status | Ý nghĩa | Hành động

DRAFT | Dòng nhu cầu mới tạo | Chờ tính

CALCULATING | Đang tính MRP | Chờ kết quả

CALCULATED | Đã tính xong | Chạy threshold/suppression

COVERED_BY_STOCK | Tồn khả dụng đủ | Không mua thêm

COVERED_BY_APPROVED_INCOMING | Incoming đã duyệt đủ | Theo dõi incoming

SHORTAGE_DETECTED | Có thiếu hụt | Đưa review

THRESHOLD_CHECK_REQUIRED | Cần kiểm tra ngưỡng | Chạy Dynamic Threshold Resolver

PROCUREMENT_SUPPRESSED | Bị loại khỏi phiếu thu mua | Không mua

PROCUREMENT_ALLOWED_FOR_REVIEW | Có thể xét nhập/mua | Chờ owner/procurement

waiting_OWNER_REVIEW | Chờ owner review | Không tự PO

DIRECTOR_APPROVAL_REQUIRED | Cần Giám đốc duyệt | Chặn

APPROVED_FOR_REQUIREMENT | Được duyệt tạo requirement | Chuyển bước sau

TRANSFERRED_TO_PURCHASE_REQUIREMENT | Chuyển sang yêu cầu mua | Không phải PO

TRANSFERRED_TO_HARVEST_REQUIREMENT | Chuyển sang yêu cầu thu hoạch | Không phải harvest execution

BLOCKED_CONFIG_MISSING | Thiếu config | Không đi tiếp

BLOCKED_STOCK_DATA_MISSING | Thiếu tồn kho | Không đi tiếp

BLOCKED_BY_HOLD_RECALL_DISPOSAL | Bị khóa bởi hold/recall/disposal | Không đi tiếp

CANCELLED | Hủy hợp lệ | Lưu audit

CLOSED | Đóng | Lưu kết luận

## 13. MATERIAL GUARD

## 13.1. Mục tiêu

Material Guard kiểm soát dữ liệu MRP trước khi vật tư được đưa vào requirement hoặc phiếu thu mua.

## 13.2. Material Guard P0

Guard | Điều kiện kiểm tra | Nếu fail

MRP_INPUT_GUARD | Demand batch hợp lệ | bị chặn

FORMULA_SNAPSHOT_GUARD | Có Production BOM Snapshot/Calculation Snapshot | bị chặn

MATERIAL_CANONICAL_GUARD | Material canonical | bị chặn

MATERIAL_GROUP_GUARD | Có A1/A2/A3/B1/B2/B3 | bị chặn

UOM_CONVERSION_GUARD | UOM hợp lệ | bị chặn

DENSITY_POLICY_GUARD | Chất lỏng cần density thì phải có policy | bị chặn

BUFFER_CONFIG_GUARD | Buffer có config | bị chặn

YIELD_CONFIG_GUARD | B1/B2 phải có output/yield | bị chặn

STOCK_SOURCE_GUARD | Tồn từ nguồn hợp lệ | bị chặn

AVAILABLE_STOCK_GUARD | Loại trừ hold/recall/reject/disposal | bị chặn

INCOMING_APPROVAL_GUARD | Incoming phải approved mới tính | IGNORE_INCOMING / bị chặn

SUPPRESSION_THRESHOLD_GUARD | Có dynamic threshold | bị chặn

PROCUREMENT_SUPPRESSION_GUARD | Vượt ngưỡng thì loại khỏi phiếu thu mua | SUPPRESSED

SAVIGIN_SOURCE_GUARD | Sâm là Company Source, không Purchase Requirement | ROUTE_TO_HARVEST_POLICY

OWNER_REVIEW_GUARD | Thiếu/ngoại lệ cần owner review | waiting_OWNER_REVIEW

DIRECTOR_APPROVAL_GUARD | Kế hoạch lớn vượt ngưỡng cần Giám đốc duyệt | DIRECTOR_APPROVAL_REQUIRED

## 14. NO-HARDCODE CONTROL

## 14.1. Không được hardcode

Không được hardcode:

- +5% Nhóm A.

- +7% B1/B2.

- 0% B3.

- Mẻ chuẩn 400.

- 7.200 gói.

- 1.800 hộp.

- 300 thùng.

- 115.560 gói.

- 28.890 hộp.

- 4.815 thùng.

- 5 mẻ/SKU.

- 15 mẻ/SKU.

- 20 mẻ aggregate.

- Supplier mặc định.

- Warehouse mặc định.

- UOM conversion.

- Density conversion.

- Yield output.

- Suppression threshold.

- Stock Policy.

Các số trên là config/policy, không phải code cố định.

## 14.2. Vì sao không hardcode baseline

Baseline mẻ 400 = 7.200 gói = 1.800 hộp = 300 thùng dùng làm chuẩn hiện tại, nhưng nếu sản lượng thực tế của mẻ thay đổi sau pilot thì ngưỡng B1/B2 phải tự tính lại theo output mới. File owner lock đã khóa rõ không được giữ cứng 7.200 gói, 1.800 hộp, 300 thùng nếu công thức chuẩn xác nhận sản lượng khác.

## 15. MRP EVIDENCE

MRP phải lưu tối thiểu:

Evidence | Nội dung

Demand Batch Snapshot | Demand đầu vào

Formula Resolution Evidence | G1/formula được dùng

Production BOM Snapshot | Công thức đã scale

Calculation Snapshot | Kết quả tính nguyên liệu

Yield Snapshot | Sản lượng gói/hộp/thùng

Buffer Config Snapshot | Buffer áp dụng

Stock Snapshot | Tồn kho/tồn khả dụng

Incoming Snapshot | Incoming được tính

Dynamic Threshold Snapshot | Ngưỡng tự tính

Suppression Result | Kết quả loại/cho phép mua

Exception Log | STOP_POINTS/exception

Owner Decision Evidence | Duyệt/chặn/override

Director Approval Evidence | Nếu vượt ngưỡng đặc biệt

Audit Log | Audit toàn bộ MRP

## 16. MRP AUDIT

Mọi hành động sau phải audit:

- Tạo MRP run.

- Chọn demand batch.

- Resolve formula.

- Đọc G1.

- Đọc Production BOM Snapshot.

- Đọc yield.

- Đọc stock.

- Đọc incoming.

- Tính gross requirement.

- Tính buffer requirement.

- Tính net requirement.

- Tính dynamic threshold.

- Chạy procurement suppression.

- Loại khỏi phiếu thu mua.

- Cho vào danh mục xét mua.

- Owner approve/reject.

- Director approve/reject.

- Recalculate.

- Close MRP run.

## 17. OUTPUT CỦA MRP

MRP chỉ được tạo các output sau:

Output | Ý nghĩa

MATERIAL_COVERED_BY_STOCK | Tồn đủ

MATERIAL_COVERED_BY_INCOMING | Incoming đủ

MATERIAL_SHORTAGE_DETECTED | Có thiếu

MATERIAL_PROCUREMENT_SUPPRESSED | Bị loại khỏi phiếu thu mua

MATERIAL_PROCUREMENT_REVIEW_ALLOWED | Cho xét mua/nhập

SAVIGIN_HARVEST_POLICY_REQUIRED | Sâm đi theo chính sách harvest

PACKAGING_REQUIREMENT_REVIEW_ALLOWED | Vật tư B1/B2/B3 được xét mua

CONFIG_MISSING_STOP_POINTS | Thiếu config

STOCK_DATA_MISSING_STOP_POINTS | Thiếu dữ liệu tồn

DISPOSAL_EXCLUDED_FROM_STOCK | Tồn chờ hủy đã bị loại

OWNER_REVIEW_REQUIRED | Cần owner review

DIRECTOR_APPROVAL_REQUIRED | Cần Giám đốc phê duyệt

RECALC_REQUIRED | Cần tính lại

MRP không được tự tạo Purchase Order hoặc Production Order.

## 18. RULE TÓM TẮT PHẦN 2/4

Rule ID | Quy tắc | Mức độ

P03-MRP-P0-001 | MRP bắt buộc sau Demand Board và Formula Resolution | P0

P03-MRP-P0-002 | MRP không sửa G1 hoặc Production BOM Snapshot | P0

P03-MRP-P0-003 | Buffer MRP không phải công thức sản xuất | P0

P03-MRP-P0-004 | Nhóm A tính theo calculated_material_quantity_per_batch | P0

P03-MRP-P0-005 | A đặc thù SKU dùng ngưỡng 5 mẻ/SKU | P0

P03-MRP-P0-006 | A dùng chung dùng aggregate 20 mẻ chuẩn | P0

P03-MRP-P0-007 | Sâm Savigin không đi phiếu thu mua supplier | P0

P03-MRP-P0-008 | B1/B2 tính theo output gói/hộp/thùng, không theo kg gạo | P0

P03-MRP-P0-009 | B1/B2 áp ngưỡng 15 mẻ/SKU | P0

P03-MRP-P0-010 | B3 theo Stock Policy thực tế | P0

P03-MRP-P0-011 | Nếu available + incoming >= threshold thì loại khỏi phiếu thu mua | P0

P03-MRP-P0-012 | Tồn hold/recall/reject/disposal không tính usable | P0

P03-MRP-P0-013 | Incoming chưa approved không được tính nguồn đáp ứng | P0

P03-MRP-P0-014 | Vượt ngưỡng đặc biệt cần Giám đốc phê duyệt | P0

P03-MRP-P0-015 | Không hardcode buffer/mẻ/ngưỡng/yield | P0

P03-MRP-P0-016 | MRP phải có evidence/audit | P0

P03-MRP-P0-017 | MRP output không tự thành Purchase Order | P0

## 19. ACCEPTANCE CRITERIA CHO PHẦN 2/4

## PHẦN 2/4 đạt governance khi:

- Đã khóa MRP bắt buộc sau Demand Board và Formula Resolution.

- Đã khóa MRP không sửa công thức.

- Đã khóa công thức MRP theo 5 lớp.

- Đã khóa Material Requirement Board.

- Đã khóa Dynamic Threshold Resolver.

- Đã khóa Nhóm A đặc thù 5 mẻ/SKU.

- Đã khóa Nhóm A dùng chung aggregate 20 mẻ.

- Đã khóa Sâm Savigin theo Strategic Reserve.

- Đã khóa Nhóm B tính theo output thực tế.

- Đã khóa B1/B2 15 mẻ/SKU.

- Đã khóa B3 theo Stock Policy.

- Đã khóa Procurement Suppression Control.

- Đã khóa available stock không tính hold/recall/reject/disposal.

- Đã khóa Material Guard.

- Đã khóa No-Hardcode Control.

- Đã khóa MRP Evidence.

- Đã khóa MRP Audit.

- Chưa gọi Production Ready.

- Chưa gọi Release Approved.

- Chưa gọi Go-live Approved.

## 20. TRẠNG THÁI SAU PHẦN 2/4

Hạng mục | Trạng thái

PACK-03 PHẦN 2/4 Documentation | GOVERNANCE_DRAFT_COMPLETE

MRP Principles | LOCKED_IN_DOCUMENTATION

Material Requirement Board | LOCKED_IN_DOCUMENTATION

Dynamic Threshold Resolver | LOCKED_IN_DOCUMENTATION

Procurement Suppression Control | LOCKED_IN_DOCUMENTATION

## A_SPECIFIC_5_BATCH_SKU	LOCKED_IN_DOCUMENTATION

## A_COMMON_20_BATCH_POOL	LOCKED_IN_DOCUMENTATION

## SAVIGIN_STRATEGIC_RESERVE	LOCKED_IN_DOCUMENTATION

## B1_SKU_15_BATCH	LOCKED_IN_DOCUMENTATION

## B2_BOX_SKU_15_BATCH	LOCKED_IN_DOCUMENTATION

## B2_CARTON_SKU_15_BATCH	LOCKED_IN_DOCUMENTATION

## B3_ACTUAL_STOCK_POLICY	LOCKED_IN_DOCUMENTATION

Material Guard | LOCKED_IN_DOCUMENTATION

No-Hardcode Control | LOCKED_IN_DOCUMENTATION

MRP Evidence / Audit | LOCKED_IN_DOCUMENTATION

MRP Runtime | waiting_IMPLEMENTATION

Material Requirement Runtime | waiting_IMPLEMENTATION

Threshold Resolver Runtime | waiting_IMPLEMENTATION

Procurement Suppression Runtime | waiting_IMPLEMENTATION

Evidence Packet | waiting_EVIDENCE

Smoke Run | waiting_SMOKE

Owner Sign-off | waiting_OWNER_SIGNOFF

Production Ready | KHÔNG

Release Approved | KHÔNG

Go-live Approved | KHÔNG

## 21. KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 khóa lớp MRP V2.0 của Ginsengfood theo hướng động, có kiểm soát và không mua sắm dư thừa.

Từ thời điểm này, MRP không còn chỉ là phép tính thiếu nguyên liệu. MRP phải đọc công thức G1 đã scale, đọc sản lượng thực tế của mẻ, tính buffer, kiểm tra tồn khả dụng, loại trừ hàng không usable, tính dynamic threshold và tự loại nguyên liệu/vật tư đủ hoặc vượt ngưỡng khỏi phiếu thu mua.

Kết luận khóa của PHẦN 2/4:

Ginsengfood không mua nguyên liệu chỉ vì có demand.

Ginsengfood không mua bao bì chỉ vì có kế hoạch sản xuất.

MRP phải tính đúng nhu cầu, nhưng cũng phải chặn mua thừa.

Nhóm A tự nhảy theo công thức đã scale.

Nhóm B tự nhảy theo sản lượng gói/hộp/thùng thực tế.

Sâm Savigin đi theo Company Source và Strategic Reserve, không đi như nguyên liệu mua ngoài.

Nếu tồn khả dụng + incoming đã duyệt đạt hoặc vượt ngưỡng, hệ thống phải loại khỏi phiếu thu mua, trừ trường hợp kế hoạch lớn được Giám đốc phê duyệt đầy đủ evidence và audit.

## PHẦN 3/4 — PROCUREMENT / COMPANY SOURCE / SUPPLIER SOURCE / INTAKE / QC / READINESS / DISPOSAL & WRITE-OFF CONTROL

## 1. MỤC TIÊU CỦA PHẦN 3/4

## PHẦN 3/4 khóa lớp xử lý sau MRP và Dynamic Threshold Resolver.

Sau khi MRP tính được nhu cầu nguyên vật liệu, hệ thống không được tự động mua, nhập, thu hoạch hoặc đưa nguyên liệu vào sản xuất. Hệ thống phải đi qua các lớp kiểm soát:

- Procurement Suppression Result.

- Source Routing.

- Purchase Requirement hoặc Harvest Requirement.

- Owner Review.

- Purchase / Harvest Approval.

- Supplier Delivery hoặc Company Harvest.

- Raw Material Intake / Packaging Intake.

- Incoming QC / Packaging Check.

- Lot / Readiness.

- READY_FOR_PRODUCTION hoặc READY_FOR_PACKAGING.

- Disposal & Write-off nếu nguyên liệu/vật tư bị hủy.

## PHẦN 3/4 đảm bảo:

- MRP Output không tự thành Purchase Order.

- Purchase Requirement không tự thành Purchase Order.

- Harvest Requirement không tự thành nguyên liệu sẵn sàng.

- Supplier Evidence không tự PASS QC.

- Intake không tự PASS QC.

- QC_PASS không tự đồng nghĩa READY_FOR_PRODUCTION.

- Packaging Intake không tự đồng nghĩa READY_FOR_PACKAGING.

- Hàng hủy không được xóa tay khỏi tồn kho.

- Sâm Savigin không đi theo phiếu thu mua như nguyên liệu supplier.

## 2. NGUYÊN TẮC P0 SAU MRP

## 2.1. MRP Output chỉ là kết quả tính toán

MRP Output chỉ cho biết:

- Vật tư nào đủ tồn.

- Vật tư nào thiếu.

- Vật tư nào bị loại khỏi phiếu thu mua do đủ/vượt ngưỡng.

- Vật tư nào cần owner review.

- Vật tư nào cần Giám đốc phê duyệt vì vượt ngưỡng đặc biệt.

- Vật tư nào phải đi theo Supplier Source.

- Vật tư nào phải đi theo Company Source.

- Vật tư nào phải đi theo Packaging Supplier.

- Vật tư nào bị chặn do thiếu config, hold, recall, reject, disposal.

MRP Output không phải nghiệp vụ thực tế đã xảy ra.

## 2.2. Không có đường tắt sau MRP

Các đường đi sau bị cấm:

Đường đi bị cấm | Lý do

MRP Output -> Purchase Order | MRP không phải quyết định mua

MRP Output -> Harvest Execution | Harvest phải qua Company Source / owner review

MRP Output -> Intake | Chưa có hàng thực tế

MRP Output -> QC_PASS | Chưa kiểm tra chất lượng

MRP Output -> READY_FOR_PRODUCTION | Chưa intake/QC/lot/readiness

MRP Output -> READY_FOR_PACKAGING | Chưa packaging intake/check/readiness

MRP Output -> Material Issue | Chưa có lot ready

Procurement Suppressed -> Purchase Requirement | Đã bị loại khỏi phiếu thu mua

Purchase Requirement -> Purchase Order | Cần owner/procurement approval

Supplier Evidence -> QC_PASS | Evidence supplier không thay QC nội bộ

Intake -> READY | Intake chỉ ghi nhận hàng về, không phải readiness

## 2.3. Procurement phải chịu Dynamic Threshold

Nếu MRP/Dynamic Threshold Resolver kết luận nguyên liệu/vật tư đủ hoặc vượt ngưỡng, hệ thống phải loại khỏi phiếu thu mua.

Chỉ khi có kế hoạch sản xuất lớn hoặc đơn hàng lớn đã được Giám đốc phê duyệt, có MRP run, evidence và audit, hệ thống mới được cho phép vượt ngưỡng có kiểm soát. File owner lock đã khóa nguyên tắc phiếu thu mua phải tự loại vật tư đủ/vượt ngưỡng, không cho mua sắm chỉ vì có yêu cầu.

## 3. SOURCE ROUTING SAU MRP

## 3.1. Source Routing là gì

Source Routing là bước xác định nguyên liệu/vật tư sau MRP sẽ đi theo nguồn nào:

- Company Source.

- Supplier Source.

- Packaging Supplier.

- Internal Ready Stock.

- bị chặn / Owner Review.

Source Routing không được suy đoán.

Source Routing phải dựa trên Material Master, Source Policy, Company Source Registry, Supplier Allowlist, Packaging Config và Stock Policy.

## 3.2. Source Type chính

source_type | Ý nghĩa | Áp dụng

COMPANY_SOURCE | Nguồn công ty tự trồng/tự sở hữu | Sâm Savigin

SUPPLIER_SOURCE | Nguồn mua từ nhà cung cấp | Nguyên liệu mua ngoài

PACKAGING_SUPPLIER | Nguồn mua bao bì/vật tư | B1/B2/B3

INTERNAL_READY_STOCK | Tồn nội bộ đã sẵn sàng | Material đã READY

BLOCKED_SOURCE_MISSING | Thiếu nguồn | Chặn

OWNER_REVIEW_REQUIRED | Cần owner quyết định | Trường hợp mơ hồ/ngoại lệ

## 3.3. Source Routing Guard

Guard | Điều kiện kiểm tra | Nếu fail

SOURCE_TYPE_GUARD | source_type phải hợp lệ | bị chặn

MATERIAL_SOURCE_POLICY_GUARD | Material phải có source policy | BLOCKED_CONFIG_MISSING

COMPANY_SOURCE_GUARD | Company Source phải active | bị chặn

SUPPLIER_ALLOWLIST_GUARD | Supplier được phép cung cấp material | bị chặn

PACKAGING_SOURCE_GUARD | Bao bì/vật tư phải có nguồn hợp lệ | bị chặn

SUPPRESSION_RESULT_GUARD | Vật tư bị suppressed không được tạo purchase requirement | bị chặn

DIRECTOR_APPROVAL_GUARD | Vượt ngưỡng đặc biệt phải có Giám đốc duyệt | DIRECTOR_APPROVAL_REQUIRED

## 4. PROCUREMENT REQUIREMENT CONTROL

## 4.1. Procurement Requirement là gì

Procurement Requirement là yêu cầu nghiệp vụ để xem xét mua nguyên liệu/vật tư sau khi MRP xác định thiếu thật và vượt qua suppression check.

Procurement Requirement không phải Purchase Order.

Procurement Requirement chỉ trả lời:

Có cần xem xét mua nguyên liệu/vật tư này không, số lượng bao nhiêu, vì sao, cho kế hoạch nào, nguồn nào, đã qua ngưỡng tồn chưa?

## 4.2. Điều kiện tạo Procurement Requirement

Chỉ được tạo Procurement Requirement khi:

- MRP đã chạy.

- Material Requirement Board có dòng thiếu thật.

- Dynamic Threshold Resolver đã tính ngưỡng.

- Procurement Suppression Check cho phép xét mua.

- Material không thuộc Sâm Savigin Company Source.

- Material có supplier/source hợp lệ.

- Không bị hold/recall/disposal STOP_POINTS.

- Có evidence.

- Có audit.

- Owner review đạt điều kiện nếu policy yêu cầu.

## 4.3. Procurement Requirement không được tạo khi

Không được tạo Procurement Requirement khi:

- available_stock + approved_incoming >= dynamic_suppression_threshold.

- Material bị PROCUREMENT_SUPPRESSED.

- Material là Sâm Savigin Company Source.

- Material chưa canonical.

- Material thiếu source policy.

- Supplier không nằm allowlist.

- Material đang hold/recall/reject/disposal.

- Thiếu MRP run.

- Thiếu evidence/audit.

- Kế hoạch vượt ngưỡng nhưng chưa có Giám đốc phê duyệt.

## 5. COMPANY SOURCE — SÂM SAVIGIN STRATEGIC RESERVE

## 5.1. Nguyên tắc khóa

Sâm Savigin là Company Source / vùng trồng công ty.

Sâm Savigin không đưa vào phiếu thu mua như nguyên liệu supplier.

Sâm Savigin được quản trị theo mùa vụ, harvest cycle và strategic reserve stock.

File owner lock đã khóa: Sâm Savigin là Company Source theo mùa vụ, được harvest/sơ chế/intake/QC/lot/readiness/nhập kho dự trữ chiến lược; không đưa vào phiếu thu mua như nguyên liệu supplier.

## 5.2. Chuỗi đúng cho Sâm Savigin

Chuỗi bắt buộc:

Company Source / Vùng trồng công ty -> Harvest Alert -> Harvest Requirement -> Harvest Plan -> Harvest Execution -> Sơ chế -> Raw Material Intake -> Incoming QC -> Raw Material Lot -> READY_FOR_PRODUCTION -> Strategic Reserve Stock -> Material Issue khi sản xuất

## 5.3. Sâm đến mùa vụ vẫn cho thu hoạch

Nếu vùng trồng Sâm Savigin đến kỳ thu hoạch, hệ thống phải cho phép tạo Company Source Harvest Alert / Harvest Requirement, ngay cả khi tồn Sâm hiện tại còn đủ cho sản xuất ngắn hạn.

Lý do:

- Sâm là nguồn công ty trồng.

- Có mùa vụ/chu kỳ thu hoạch.

- Không phải nguyên liệu mua ngoài có thể đặt bất cứ lúc nào.

- Cần thu hoạch đúng thời điểm kỹ thuật.

- Cần sơ chế, nhập kho và bảo quản dự trữ.

- Tồn Sâm là tồn dự trữ chiến lược, không chỉ là tồn phục vụ lệnh sản xuất hiện tại.

## 5.4. Sâm không áp dụng purchase suppression như supplier material

Sâm không bị loại khỏi “phiếu thu mua” theo logic mua ngoài, vì Sâm không đi phiếu thu mua supplier.

Tuy nhiên, Sâm vẫn phải chịu:

- Company Source Governance.

- Harvest Evidence.

- Intake.

- QC.

- Raw Material Lot.

- Readiness.

- Inventory Ledger.

- Traceability.

- Strategic Reserve Review.

- Owner Review nếu tồn quá cao hoặc aging risk.

## 5.5. Khi nào cảnh báo thiếu Sâm

Hệ thống chỉ cảnh báo thiếu Sâm cho sản xuất khi:

- Tồn Sâm READY_FOR_PRODUCTION dưới production safety threshold.

- Forecast sản xuất vượt tồn dự trữ.

- Tồn Sâm có nhưng không ready.

- Tồn Sâm bị hold.

- Tồn Sâm bị QC reject.

- Tồn Sâm bị aging/quality risk cần review.

- Tồn Sâm bị reserved cho kế hoạch khác.

File owner lock đã nêu rõ: hệ thống chỉ cảnh báo nguy cơ thiếu Sâm cho sản xuất khi tồn Sâm READY_FOR_PRODUCTION dưới production safety threshold hoặc forecast sản xuất vượt tồn dự trữ.

## 5.6. Company Source Harvest Status

Status | Ý nghĩa | Hành động

SAVIGIN_HARVEST_WINDOW_OPEN | Vùng trồng đến cửa sổ thu hoạch | Owner review

SAVIGIN_HARVEST_DUE | Đến kỳ thu hoạch | Tạo Harvest Requirement

SAVIGIN_HARVEST_OVERDUE | Quá kỳ thu hoạch | Cảnh báo ưu tiên

SAVIGIN_HARVEST_PLANNED | Đã có kế hoạch thu hoạch | Theo dõi

SAVIGIN_HARVEST_EXECUTED | Đã thu hoạch | Chuyển sơ chế/intake

SAVIGIN_INTAKE_waiting | Chờ intake | Không tính ready

SAVIGIN_QC_waiting | Chờ QC | Không tính ready

SAVIGIN_READY_FOR_PRODUCTION | Đã ready | Có thể dùng cho sản xuất

SAVIGIN_STRATEGIC_RESERVE_OK | Dự trữ đạt | Theo dõi

SAVIGIN_PRODUCTION_STOCK_LOW | Dưới ngưỡng an toàn sản xuất | Cảnh báo thiếu

SAVIGIN_RESERVE_AGING_RISK | Tồn lâu/rủi ro chất lượng | QA/owner review

## 6. SUPPLIER SOURCE — NGUYÊN LIỆU MUA NGOÀI

## 6.1. Nguyên tắc khóa

Nguyên liệu không thuộc Company Source phải đi theo Supplier Source nếu là nguyên liệu mua ngoài.

Chuỗi đúng:

Material Requirement Board -> Procurement Suppression Check -> Procurement Requirement -> Purchase Request -> Purchase Order nếu được duyệt -> Supplier Delivery -> Raw Material Intake -> Incoming QC -> Raw Material Lot -> READY_FOR_PRODUCTION

## 6.2. Supplier Allowlist

Supplier chỉ được cung cấp nguyên liệu nằm trong allowlist.

Nếu supplier không nằm trong allowlist cho material đó, hệ thống phải chặn.

Tình huống | Hành động

Supplier active và được phép cung cấp material | Cho phép tạo Procurement Requirement nếu điều kiện khác đạt

Supplier inactive | bị chặn

Supplier không được phép cung cấp material | bị chặn

Supplier thiếu evidence policy | waiting_REVIEW / bị chặn

Supplier chưa được owner duyệt | bị chặn

Supplier bị quality block | bị chặn

## 6.3. Purchase Requirement không phải Purchase Order

Purchase Requirement chỉ là yêu cầu xem xét mua.

Purchase Order chỉ được tạo khi:

- Purchase Requirement hợp lệ.

- Supplier hợp lệ.

- Quantity rõ ràng.

- Expected delivery rõ ràng.

- Điều kiện mua được duyệt nếu áp dụng.

- Owner/procurement approval đạt.

- Evidence đầy đủ.

- Audit đầy đủ.

- Không bị suppression.

- Không vi phạm threshold/guard.

## 6.4. Supplier Evidence không tự PASS QC

Supplier có thể cung cấp:

- Hình ảnh.

- Video.

- Phiếu giao hàng.

- COA nếu có.

- Chứng từ nguồn gốc.

- Chứng nhận chất lượng nếu có.

- Cam kết chất lượng.

- Delivery evidence.

Nhưng Supplier Evidence không được hiểu là:

- Hàng đã đạt QC nội bộ.

- Hàng đã nhập kho hợp lệ.

- Hàng đã READY_FOR_PRODUCTION.

- Hàng được phép cấp phát.

- Hàng được phép dùng trong sản xuất.

QC nội bộ của Ginsengfood mới là điểm quyết định.

## 7. PACKAGING SUPPLIER — NHÓM B1 / B2 / B3

## 7.1. Nguyên tắc khóa

Bao bì và vật tư không được xem là “phụ” theo nghĩa không cần quản trị.

Nhóm B ảnh hưởng trực tiếp đến:

- Đóng gói.

- In lô.

- NSX.

- HSD.

- Mã vạch.

- QR.

- Trade item.

- Public trace.

- Recall.

- Sellable condition.

## 7.2. Chuỗi đúng cho bao bì/vật tư

Packaging Requirement -> Procurement Suppression Check -> Packaging Purchase Requirement -> Purchase Request -> Purchase Order nếu được duyệt -> Supplier Delivery -> Packaging Intake -> Packaging Check / QC nếu cần -> READY_FOR_PACKAGING

## 7.3. B1 — Bao bì cấp 1

B1 phải kiểm soát:

- Đúng SKU.

- Đúng cuộn màng/bao bì cấp 1.

- Đúng quy cách.

- Đúng vật liệu.

- Đúng nhà cung cấp.

- Đúng readiness.

- Không hold.

- Không reject.

- Không obsolete.

- Không disposal waiting.

## 7.4. B2 — Hộp / thùng carton

B2 phải kiểm soát:

- Đúng SKU.

- Đúng hộp sản phẩm.

- Đúng thùng carton.

- Đúng trade item.

- Đúng quy cách 4 gói/hộp nếu policy hiện hành áp dụng.

- Đúng quy cách 6 hộp/thùng nếu policy hiện hành áp dụng.

- Đúng vùng in lô/NSX/HSD/mã vạch/QR.

- Đúng Print Config.

- Không dùng hộp/thùng SKU khác nếu chưa có approved substitution.

- Không dùng carton nếu carton config chưa bật.

## 7.5. B3 — Vật tư dùng chung

B3 gồm tem, nhãn, băng keo, mực in, ribbon, decal, dung môi, vật tư vệ sinh máy in, dao cắt, keo và vật tư phụ.

## B3:

- Không tự cộng 7%.

- Không dùng ngưỡng 15 mẻ mặc định nếu chưa cấu hình.

- Xử lý theo Stock Policy thực tế.

- Nếu có consumption_per_batch thì owner cấu hình riêng.

- Nếu tồn đủ/vượt policy thì loại khỏi phiếu mua.

## 8. RAW MATERIAL INTAKE

## 8.1. Định nghĩa

Raw Material Intake là nghiệp vụ ghi nhận nguyên liệu thực tế đã được tiếp nhận vào hệ thống.

Raw Material Intake không phải QC_PASS.

Raw Material Intake không phải READY_FOR_PRODUCTION.

Raw Material Intake chỉ xác nhận:

Có một lô nguyên liệu thực tế được tiếp nhận, cần kiểm tra và xử lý tiếp.

## 8.2. Nguồn Raw Material Intake

Raw Material Intake có thể đến từ:

Nguồn | Điều kiện

Company Harvest | Có Harvest Plan/Harvest Evidence

Supplier Delivery | Có Purchase Order hoặc delivery reference hợp lệ

Internal Transfer nếu có | Có source/destination rõ

Return/Rework nếu có | Có policy riêng

## 8.3. Trường tối thiểu

Trường | Ý nghĩa

intake_id | Mã intake

source_type | COMPANY_SOURCE / SUPPLIER_SOURCE

source_id | company_source_id hoặc supplier_id

material_id | Material canonical

received_quantity | Số lượng nhận

received_uom | Đơn vị nhận

received_date | Ngày nhận

linked_requirement_id | Purchase/Harvest Requirement

linked_purchase_order_id | Nếu có

linked_harvest_id | Nếu có

received_by | Người nhận

intake_status | Trạng thái intake

evidence_reference | Evidence

audit_reference | Audit

## 8.4. Intake Status

Status | Ý nghĩa

DRAFT | Phiếu intake mới

RECEIVED_waiting_QC | Đã nhận, chờ QC

PARTIALLY_ACCEPTED | Một phần được chấp nhận

REJECTED | Bị từ chối

RETURNED_TO_SUPPLIER | Trả supplier

CANCELLED | Hủy hợp lệ

CLOSED | Đóng

## 8.5. Intake không tự tạo READY_FOR_PRODUCTION

Sau Intake, nguyên liệu phải qua:

- Incoming QC.

- Lot creation.

- Disposition.

- Readiness check.

- READY_FOR_PRODUCTION nếu đạt.

Không được dùng hàng đã intake cho sản xuất khi chưa ready.

## 9. PACKAGING INTAKE

## 9.1. Định nghĩa

Packaging Intake là nghiệp vụ ghi nhận bao bì, hộp, thùng, tem, nhãn, vật tư in hoặc vật tư phụ đã được tiếp nhận.

Packaging Intake không phải READY_FOR_PACKAGING.

## 9.2. Packaging Intake phải kiểm soát

- packaging_material_id.

- material_group B1/B2/B3.

- SKU/trade item nếu có.

- Nhà cung cấp.

- Số lượng nhận.

- Đơn vị nhận.

- Thiết kế/spec nếu có.

- Print dependency nếu có.

- Evidence.

- Audit.

## 9.3. Packaging Check

Packaging Check có thể kiểm tra:

- Đúng SKU.

- Đúng thiết kế.

- Đúng quy cách.

- Đúng chất liệu.

- Đúng kích thước.

- Đúng số lượng.

- Đúng vùng in.

- Đúng mã in sẵn nếu có.

- Không lỗi in.

- Không rách/móp/hư.

- Không sai trade item.

- Không dùng nhầm bao bì SKU khác.

## 10. INCOMING QC

## 10.1. Incoming QC là gate bắt buộc

Incoming QC là bước kiểm tra chất lượng đầu vào.

Incoming QC thuộc Quality/Operational Core, không thuộc supplier.

## 10.2. Kết quả QC

qc_result | Ý nghĩa

QC_waiting | Chờ kiểm tra

QC_PASS | Đạt kiểm tra

QC_HOLD | Giữ lại để xem xét

QC_REJECT | Không đạt

QC_RETEST_REQUIRED | Cần kiểm tra lại

QC_PARTIAL_PASS | Một phần đạt nếu policy cho phép

## 10.3. QC_PASS không đồng nghĩa READY_FOR_PRODUCTION

QC_PASS chỉ là kết quả kiểm tra chất lượng.

READY_FOR_PRODUCTION còn cần:

- Raw Material Lot hợp lệ.

- Usable quantity > 0.

- Warehouse/location hợp lệ.

- Không hold.

- Không recall lock.

- Không expired.

- Không disposal waiting.

- Trace reference.

- Evidence.

- Audit.

- Readiness action.

## 11. RAW MATERIAL LOT

## 11.1. Vai trò

Raw Material Lot là đơn vị quản trị nguyên liệu sau intake/QC.

Material Issue phải lot-based.

Không được cấp phát nguyên liệu không có lot hợp lệ.

## 11.2. Trường tối thiểu

Trường | Ý nghĩa

raw_lot_id | Mã lô nguyên liệu

material_id | Material canonical

source_type | Company/Supplier

source_id | Nguồn

intake_id | Intake liên quan

qc_result | Kết quả QC

lot_status | Trạng thái lot

usable_quantity | Lượng usable

warehouse_id | Kho

location_id | Vị trí

received_date | Ngày nhận

expiry_date | Hạn dùng nếu có

trace_reference | Trace

evidence_reference | Evidence

audit_reference | Audit

## 11.3. Lot Status

lot_status | Ý nghĩa

waiting_QC | Chờ QC

QC_PASSED_waiting_READINESS | QC đạt, chờ readiness

READY_FOR_PRODUCTION | Sẵn sàng sản xuất

HOLD | Đang hold

REJECTED | Không đạt

EXPIRED | Hết hạn

DISPOSAL_waiting | Chờ hủy

DISPOSED | Đã hủy

CONSUMED | Đã dùng hết

RECALL_LOCKED | Bị recall lock

CANCELLED | Hủy hợp lệ

## 12. READY_FOR_PRODUCTION CONTROL

## 12.1. Định nghĩa

READY_FOR_PRODUCTION là trạng thái xác nhận raw material lot đủ điều kiện cấp cho sản xuất.

READY_FOR_PRODUCTION không tự trừ tồn.

READY_FOR_PRODUCTION không tự cấp phát.

Material Issue vẫn là nghiệp vụ riêng của PACK-01.

## 12.2. Điều kiện đạt READY_FOR_PRODUCTION

Một raw material lot chỉ được chuyển READY_FOR_PRODUCTION khi:

- material_id hợp lệ.

- source_type hợp lệ.

- source_id hợp lệ.

- intake hợp lệ.

- Incoming QC đạt.

- raw lot được tạo hợp lệ.

- usable_quantity > 0.

- warehouse/location hợp lệ.

- Không hold.

- Không recall lock.

- Không expired.

- Không disposal waiting.

- Không quality STOP_POINTS.

- Có trace reference.

- Có evidence.

- Có audit.

- Readiness owner xác nhận nếu policy yêu cầu.

## 13. READY_FOR_PACKAGING CONTROL

## 13.1. Định nghĩa

READY_FOR_PACKAGING xác nhận bao bì/vật tư đủ điều kiện dùng cho đóng gói/in.

READY_FOR_PACKAGING không tự in.

READY_FOR_PACKAGING không tự đóng gói.

READY_FOR_PACKAGING không tự tạo thành phẩm.

## 13.2. Điều kiện đạt READY_FOR_PACKAGING

Bao bì/vật tư chỉ được chuyển READY_FOR_PACKAGING khi:

- packaging_material_id hợp lệ.

- material_group đúng B1/B2/B3.

- supplier/source hợp lệ.

- intake hợp lệ.

- received_quantity xác nhận.

- Packaging Check/QC đạt nếu policy yêu cầu.

- Đúng SKU/trade item nếu là B1/B2.

- Đúng Print Config nếu phục vụ in.

- Không hold.

- Không reject.

- Không obsolete.

- Không disposal waiting.

- Không recall/quality lock.

- Có warehouse/location.

- Có evidence.

- Có audit.

- Owner/QA/Packaging owner xác nhận nếu policy yêu cầu.

## 14. DISPOSAL & WRITE-OFF CONTROL

## 14.1. Mục tiêu

Disposal & Write-off Control khóa luồng xử lý nguyên liệu, bao bì, vật tư hoặc thành phẩm phải hủy.

Hệ thống không được cho phép xóa tay tồn kho hoặc sửa trực tiếp số tồn.

File owner lock đã khóa rõ: khi nguyên liệu, bao bì hoặc vật tư phải hủy do QC reject, hết hạn, hư hỏng, sai mẫu, lỗi in, obsolete hoặc recall/quality issue, hệ thống không được xóa tay hoặc sửa tồn trực tiếp; phải đi theo luồng hủy có kiểm soát.

## 14.2. Các lý do hủy

Có thể tạo Disposal Request khi:

- QC_REJECT.

- EXPIRED.

- DAMAGED.

- OBSOLETE.

- Sai mẫu.

- Lỗi in.

- Sai SKU.

- Sai trade item.

- Recall / quality issue.

- Hư hỏng trong kho.

- Không còn phù hợp công thức/bao bì đã active.

- Owner quyết định loại bỏ sau review.

## 14.3. Chuỗi hủy bắt buộc

HOLD / QC_REJECT / EXPIRED / DAMAGED / OBSOLETE -> DISPOSAL_REQUESTED -> QA / OWNER REVIEW -> DISPOSAL_APPROVED -> DISPOSAL_EXECUTED -> INVENTORY_WRITTEN_OFF -> ACCOUNTING_SYNC_waiting nếu cần -> DISPOSAL_CLOSED

## 14.4. Trạng thái Disposal

Status | Ý nghĩa | Quy tắc xử lý

HOLD | Vật tư bị giữ | Không tính usable

QC_REJECT | Không đạt QC | Không cấp phát

EXPIRED | Hết hạn | Không dùng

DAMAGED | Hư hỏng | Không dùng

OBSOLETE | Lỗi thời/không còn phù hợp | Owner review

DISPOSAL_REQUESTED | Tạo yêu cầu hủy | Cần lý do/evidence

DISPOSAL_waiting_QA_REVIEW | Chờ QA review | Không giảm tồn

DISPOSAL_waiting_OWNER_APPROVAL | Chờ owner duyệt | Không giảm tồn

DISPOSAL_APPROVED | Được duyệt hủy | Chưa giảm tồn nếu chưa thực hiện

DISPOSAL_EXECUTED | Đã thực hiện hủy | Ghi evidence

INVENTORY_WRITTEN_OFF | Đã ghi giảm tồn | Qua Inventory Ledger

ACCOUNTING_SYNC_waiting | Chờ hạch toán nếu cần | Đi qua MISA/accounting boundary

DISPOSAL_CLOSED | Đóng hồ sơ | Tồn mới được MRP đọc

## 14.5. Hàng chờ hủy không tính usable

Các trạng thái sau không được tính vào available_stock, ready_stock hoặc suppression threshold:

- QC_REJECT.

- EXPIRED.

- DAMAGED.

- OBSOLETE.

- DISPOSAL_REQUESTED.

- DISPOSAL_APPROVED.

- DISPOSAL_EXECUTED.

- INVENTORY_WRITTEN_OFF.

- RECALL_LOCKED.

File owner lock đã khóa: hàng chờ hủy hoặc đã duyệt hủy không được tính vào available_stock, ready_stock hoặc suppression threshold để tránh hệ thống hiểu nhầm là còn vật tư usable.

## 14.6. Disposal Evidence

Disposal phải có tối thiểu:

- material_id / packaging_material_id / lot_id.

- quantity đề nghị hủy.

- lý do hủy.

- hình ảnh/video nếu policy yêu cầu.

- QC/QA reference nếu có.

- owner approval.

- người thực hiện hủy.

- thời gian hủy.

- phương thức hủy.

- inventory ledger write-off reference.

- accounting reference nếu có.

- audit log.

## 15. ACCOUNTING / MISA BOUNDARY

## 15.1. MISA không điều khiển vận hành

MISA hoặc kế toán không được:

- Tạo readiness.

- Pass QC.

- Tạo raw lot.

- Cấp phát nguyên liệu.

- Phát lệnh sản xuất.

- Sửa Inventory Ledger trực tiếp.

- Xóa tồn vật tư.

## 15.2. Khi nào cần accounting sync

Accounting/MISA boundary có thể nhận dữ liệu khi:

- Purchase Order đã phát sinh hợp lệ.

- Raw Material Intake đã xác nhận theo policy.

- Inventory Ledger có movement hợp lệ.

- Disposal write-off đã thực hiện.

- Material Issue đã xảy ra.

- Warehouse Receipt đã xác nhận.

- Chi phí cần hạch toán.

## 15.3. Disposal Accounting

Nếu nguyên liệu/vật tư hủy có giá trị kế toán, sau INVENTORY_WRITTEN_OFF phải có accounting sync hoặc accounting review theo policy.

Không được hạch toán hủy nếu chưa có Disposal Execution và Inventory Ledger Write-off.

## 16. OWNER BOUNDARY

## 16.1. Procurement Owner

Procurement Owner chịu trách nhiệm:

- Xem xét Procurement Requirement.

- Chọn supplier hợp lệ.

- Kiểm tra allowlist.

- Trình/duyệt Purchase Request.

- Theo dõi Purchase Order nếu có.

- Theo dõi supplier delivery.

- Gắn evidence/audit.

Procurement Owner không được pass QC.

## 16.2. Company Source Owner

Company Source Owner chịu trách nhiệm:

- Quản trị vùng trồng Sâm.

- Theo dõi harvest window.

- Tạo/duyệt Harvest Requirement.

- Theo dõi harvest execution.

- Gắn harvest evidence.

- Chuyển về intake.

Company Source Owner không được tự READY_FOR_PRODUCTION nếu chưa qua QC/readiness.

## 16.3. Quality Owner

Quality Owner chịu trách nhiệm:

- Incoming QC.

- QC_PASS/QC_HOLD/QC_REJECT.

- Retest nếu có.

- Hold/reject reason.

- QC evidence.

- Quality disposition.

Quality Owner không tự phát Production Order.

## 16.4. Warehouse / Intake Owner

Warehouse/Intake Owner chịu trách nhiệm:

- Nhận hàng.

- Xác nhận quantity.

- Xác nhận kho/location.

- Tạo intake.

- Tạo hoặc hỗ trợ tạo lot theo policy.

- Ghi inventory movement hợp lệ.

Warehouse không tự pass QC.

## 16.5. Packaging Owner

Packaging Owner chịu trách nhiệm:

- Kiểm tra bao bì/vật tư.

- Xác nhận đúng SKU/trade item.

- Xác nhận Print Config.

- Xác nhận READY_FOR_PACKAGING nếu policy cho phép.

- Gắn evidence/audit.

Packaging Owner không tự release batch.

## 16.6. Disposal Owner

Disposal Owner / QA / Warehouse / Accounting phối hợp để:

- Tạo Disposal Request.

- Review lý do hủy.

- Duyệt hoặc từ chối hủy.

- Thực hiện hủy.

- Ghi write-off.

- Gắn evidence.

- Gửi accounting sync nếu cần.

## 17. GUARD TÓM TẮT PHẦN 3/4

## 17.1. Procurement Guard

Guard | Điều kiện kiểm tra | Nếu fail

MATERIAL_REQUIREMENT_APPROVAL_GUARD | Material Requirement hợp lệ | bị chặn

PROCUREMENT_SUPPRESSION_GUARD | Không bị suppressed | SUPPRESSED

SOURCE_TYPE_GUARD | Có source_type hợp lệ | bị chặn

SUPPLIER_ACTIVE_GUARD | Supplier active | bị chặn

SUPPLIER_ALLOWLIST_GUARD | Supplier được phép cung cấp material | bị chặn

COMPANY_SOURCE_ACTIVE_GUARD | Company Source active | bị chặn

QUANTITY_GUARD | Quantity rõ và > 0 | bị chặn

UOM_GUARD | UOM hợp lệ | bị chặn

OWNER_APPROVAL_GUARD | Cần owner approval | waiting_OWNER_REVIEW

DIRECTOR_APPROVAL_GUARD | Vượt ngưỡng đặc biệt cần Giám đốc duyệt | DIRECTOR_APPROVAL_REQUIRED

## 17.2. Intake Guard

Guard | Điều kiện kiểm tra | Nếu fail

SOURCE_REFERENCE_GUARD | Intake có nguồn hợp lệ | bị chặn

MATERIAL_CANONICAL_GUARD | Material canonical | bị chặn

REQUIREMENT_LINK_GUARD | Có requirement/PO/harvest link nếu cần | bị chặn

QUANTITY_RECEIVED_GUARD | Quantity nhận rõ ràng | bị chặn

SUPPLIER_MATCH_GUARD | Supplier khớp PO/requirement | bị chặn

COMPANY_SOURCE_MATCH_GUARD | Company Source khớp Harvest Plan | bị chặn

EVIDENCE_GUARD | Evidence đủ | waiting_REVIEW / bị chặn

DUPLICATE_INTAKE_GUARD | Không intake trùng | bị chặn

QC_REQUIRED_GUARD | Nếu cần QC thì chuyển waiting_QC | FORCE_waiting_QC

## 17.3. Readiness Guard

Guard | Điều kiện kiểm tra | Nếu fail

QC_PASS_GUARD | QC đạt | bị chặn

LOT_CREATED_GUARD | Lot hợp lệ | bị chặn

USABLE_QUANTITY_GUARD | usable_quantity > 0 | bị chặn

LOCATION_GUARD | Có kho/location | bị chặn

HOLD_GUARD | Không hold | bị chặn

RECALL_LOCK_GUARD | Không recall lock | bị chặn

EXPIRY_GUARD | Không hết hạn | bị chặn

DISPOSAL_GUARD | Không disposal waiting/approved/executed | bị chặn

TRACE_GUARD | Có trace reference nếu cần | bị chặn

EVIDENCE_GUARD | Evidence đủ | waiting_REVIEW / bị chặn

## 17.4. Disposal Guard

Guard | Điều kiện kiểm tra | Nếu fail

DISPOSAL_REASON_GUARD | Có lý do hủy | bị chặn

MATERIAL_LOT_GUARD | Có material/lot rõ | bị chặn

QUANTITY_GUARD | Quantity hủy rõ | bị chặn

QA_REVIEW_GUARD | QA review nếu cần | waiting_QA_REVIEW

OWNER_APPROVAL_GUARD | Owner duyệt nếu cần | waiting_OWNER_APPROVAL

EXECUTION_EVIDENCE_GUARD | Có evidence hủy | bị chặn

LEDGER_WRITE_OFF_GUARD | Có Inventory Ledger write-off | BLOCKED_CLOSE

ACCOUNTING_SYNC_GUARD | Có sync/review nếu cần | waiting_ACCOUNTING_SYNC

## 18. EVIDENCE REQUIREMENT

## 18.1. Evidence cho Procurement

- Material Requirement reference.

- Dynamic threshold result.

- Suppression result.

- Procurement Requirement.

- Supplier allowlist.

- Owner approval.

- Director approval nếu vượt ngưỡng đặc biệt.

- Purchase Request / Purchase Order nếu có.

- Supplier confirmation.

- Delivery evidence.

- Audit.

## 18.2. Evidence cho Company Source / Sâm Savigin

- Company Source reference.

- Harvest window.

- Harvest Requirement.

- Harvest Plan.

- Harvest execution evidence.

- Sơ chế evidence nếu có.

- Raw Material Intake.

- Incoming QC.

- Raw lot.

- READY_FOR_PRODUCTION.

- Strategic Reserve Stock snapshot.

- Audit.

## 18.3. Evidence cho Intake / QC / Readiness

- Intake record.

- Source reference.

- Received quantity.

- QC evidence.

- Lot evidence.

- Readiness decision.

- Warehouse/location.

- Hold/reject/disposal reference nếu có.

- Audit.

## 18.4. Evidence cho Disposal

- Disposal Request.

- Lý do hủy.

- Material/lot/quantity.

- QA/Owner approval.

- Disposal execution evidence.

- Inventory Ledger write-off.

- Accounting sync nếu cần.

- Audit.

## 19. RULE TÓM TẮT PHẦN 3/4

Rule ID | Quy tắc | Mức độ

P03-PROC-P0-001 | MRP Output không tự thành Purchase Order | P0

P03-PROC-P0-002 | Procurement Requirement không tự thành Purchase Order | P0

P03-PROC-P0-003 | Vật tư bị suppressed không được đưa vào phiếu thu mua | P0

P03-PROC-P0-004 | Sâm Savigin là Company Source, không Purchase Requirement supplier | P0

P03-PROC-P0-005 | Sâm đến kỳ thu hoạch vẫn đi Harvest Requirement theo mùa vụ | P0

P03-PROC-P0-006 | Supplier phải nằm trong allowlist | P0

P03-PROC-P0-007 | Supplier Evidence không tự PASS QC | P0

P03-INT-P0-001 | Raw Material Intake không tự PASS QC | P0

P03-INT-P0-002 | Packaging Intake không tự READY_FOR_PACKAGING | P0

P03-QC-P0-001 | QC_PASS không tự READY_FOR_PRODUCTION | P0

P03-LOT-P0-001 | Material Issue phải dùng lot READY_FOR_PRODUCTION | P0

P03-PKG-P0-001 | B1/B2/B3 phải READY_FOR_PACKAGING trước đóng gói/in | P0

P03-DSP-P0-001 | Nguyên liệu/vật tư hủy không được xóa tay tồn kho | P0

P03-DSP-P0-002 | Disposal phải qua Request -> Review -> Execution -> Ledger Write-off | P0

P03-DSP-P0-003 | Hàng chờ hủy không tính usable/available/threshold | P0

P03-EVD-P0-001 | Procurement/Intake/QC/Readiness/Disposal phải có evidence/audit | P0

## 20. ACCEPTANCE CRITERIA CHO PHẦN 3/4

## PHẦN 3/4 đạt governance khi:

- Đã khóa MRP Output không tự thành Purchase Order.

- Đã khóa Procurement Requirement không tự thành Purchase Order.

- Đã khóa Source Routing.

- Đã khóa Company Source cho Sâm Savigin.

- Đã khóa Sâm Savigin Strategic Reserve.

- Đã khóa Harvest Cycle theo mùa vụ.

- Đã khóa Supplier Source.

- Đã khóa Supplier Allowlist.

- Đã khóa Packaging Supplier cho B1/B2/B3.

- Đã khóa Raw Material Intake.

- Đã khóa Packaging Intake.

- Đã khóa Incoming QC.

- Đã khóa QC_PASS không tự READY_FOR_PRODUCTION.

- Đã khóa Raw Material Lot.

- Đã khóa READY_FOR_PRODUCTION.

- Đã khóa READY_FOR_PACKAGING.

- Đã khóa Disposal & Write-off Control.

- Đã khóa hàng chờ hủy không tính usable.

- Đã khóa Accounting/MISA Boundary.

- Đã khóa Owner Boundary.

- Đã khóa Evidence/Audit.

- Chưa gọi Production Ready.

- Chưa gọi Release Approved.

- Chưa gọi Go-live Approved.

## 21. TRẠNG THÁI SAU PHẦN 3/4

Hạng mục | Trạng thái

PACK-03 PHẦN 3/4 Documentation | GOVERNANCE_DRAFT_COMPLETE

Source Routing | LOCKED_IN_DOCUMENTATION

Procurement Requirement Control | LOCKED_IN_DOCUMENTATION

Company Source / Sâm Savigin | LOCKED_IN_DOCUMENTATION

Strategic Reserve Stock | LOCKED_IN_DOCUMENTATION

Supplier Source | LOCKED_IN_DOCUMENTATION

Supplier Allowlist | LOCKED_IN_DOCUMENTATION

Packaging Supplier | LOCKED_IN_DOCUMENTATION

Raw Material Intake | LOCKED_IN_DOCUMENTATION

Packaging Intake | LOCKED_IN_DOCUMENTATION

Incoming QC | LOCKED_IN_DOCUMENTATION

Raw Material Lot | LOCKED_IN_DOCUMENTATION

## READY_FOR_PRODUCTION	LOCKED_IN_DOCUMENTATION

## READY_FOR_PACKAGING	LOCKED_IN_DOCUMENTATION

Disposal & Write-off Control | LOCKED_IN_DOCUMENTATION

Accounting/MISA Boundary | LOCKED_IN_DOCUMENTATION

Procurement Runtime | waiting_IMPLEMENTATION

Company Source / Harvest Runtime | waiting_IMPLEMENTATION

Supplier Source Runtime | waiting_IMPLEMENTATION

Intake Runtime | waiting_IMPLEMENTATION

QC / Readiness Runtime | waiting_IMPLEMENTATION

Disposal Runtime | waiting_IMPLEMENTATION

Evidence Packet | waiting_EVIDENCE

Smoke Run | waiting_SMOKE

Owner Sign-off | waiting_OWNER_SIGNOFF

Production Ready | KHÔNG

Release Approved | KHÔNG

Go-live Approved | KHÔNG

## 22. KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 khóa lớp Procurement, Source Routing, Company Source, Supplier Source, Packaging Source, Intake, QC, Readiness và Disposal & Write-off Control.

Từ thời điểm này, Ginsengfood không được mua, nhập, thu hoạch, cấp phát, đóng gói hoặc hủy vật tư theo cảm tính. Mọi nghiệp vụ đều phải qua Source-of-Truth, Guard, Owner Review, Evidence, Audit và Inventory Ledger.

Kết luận khóa của PHẦN 3/4:

MRP báo thiếu không có nghĩa là được mua ngay.

Vật tư đủ/vượt ngưỡng phải bị loại khỏi phiếu thu mua.

Sâm Savigin là nguồn công ty trồng theo mùa vụ, phải đi theo Harvest -> Sơ chế -> Intake -> QC -> Lot -> Readiness -> Strategic Reserve, không đi như nguyên liệu supplier.

Supplier Evidence không thay QC nội bộ.

Intake không thay readiness.

QC_PASS chưa đủ để cấp phát nếu chưa READY_FOR_PRODUCTION.

Bao bì/vật tư chưa READY_FOR_PACKAGING thì không được đóng gói/in.

Hàng phải hủy không được xóa tay tồn kho; bắt buộc qua Disposal Request, QA/Owner Review, Disposal Execution, Inventory Ledger Write-off, Evidence và Audit.

## PHẦN 4/4 — STOCK ALERT / MATERIAL ALERT / EXTENSION REGISTRY / EVIDENCE / SMOKE / DONE GATE / PACK-03 FINAL CONCLUSION

## 1. MỤC TIÊU CỦA PHẦN 4/4

## PHẦN 4/4 khóa lớp cảnh báo tồn kho, cảnh báo nguyên liệu – bao bì – vật tư, cơ chế mở rộng SKU/công thức/nguyên liệu/bao bì, evidence packet, smoke matrix, done gate và trạng thái cuối của PACK-03 V2.0.

## PHẦN 4/4 đảm bảo rằng:

- Stock Alert không tự tạo Production Order.

- Material Alert không tự tạo Purchase Order.

- Cảnh báo thiếu không đồng nghĩa được mua ngay.

- Tồn đủ/vượt ngưỡng phải tự loại khỏi phiếu thu mua.

- Ngưỡng Nhóm A tự nhảy theo công thức đã scale.

- Ngưỡng Nhóm B tự nhảy theo sản lượng gói/hộp/thùng thực tế.

- Sâm Savigin đi theo Company Source / Strategic Reserve, không đi như nguyên liệu mua ngoài.

- SKU mới không được thêm bằng cách sửa đè danh mục cũ.

- Công thức mới không được sửa đè G1 đã dùng.

- Nguyên liệu/vật tư hủy không được xóa tay khỏi tồn kho.

- Không evidence thì không PASS.

- Không smoke thì không PASS.

- Không owner sign-off thì không Release Approved.

- PACK-03 V2.0 đạt governance complete nhưng chưa đồng nghĩa Production Ready.

## 2. STOCK ALERT THÀNH PHẨM

## 2.1. Định nghĩa

Stock Alert thành phẩm là cơ chế cảnh báo khi tồn kho thành phẩm của SKU thay đổi đến mức cần theo dõi, lập kế hoạch sản xuất, hoặc chặn bán nếu không còn hàng sellable.

Stock Alert chỉ là tín hiệu cảnh báo.

Stock Alert không phải:

- Production Demand.

- Production Order.

- MRP.

- Product Activation.

- Sellable Approval.

- Warehouse Receipt.

- Release Approval.

- Purchase Requirement.

## 2.2. Chuỗi đúng từ Stock Alert

Chuỗi đúng:

Stock Alert -> Production Demand -> Production Demand Board -> Formula Resolution -> MRP -> Material Requirement Board -> Procurement / Harvest / Readiness Check -> Production Order Draft -> Owner Approval -> Production Order chính thức

Không được đi tắt:

Đường đi bị cấm | Lý do

Stock Alert -> Production Order | Cảnh báo tồn không phải quyết định sản xuất

Stock Alert -> MRP trực tiếp | Phải qua Production Demand Board

Stock Alert -> Purchase Order | Thiếu thành phẩm không tự đồng nghĩa mua nguyên liệu

Stock Alert -> Sellable Override | Không được vượt Sale Lock/Recall/Hold

Stock Alert -> MISA Posting | Cảnh báo tồn không phải nghiệp vụ kế toán

## 2.3. Bảng trạng thái Stock Alert

Mức cảnh báo | Mã trạng thái | Ý nghĩa | Hành động

Tồn đủ | STOCK_OK | Chưa cần sản xuất | Theo dõi

Tồn gần mức cảnh báo | STOCK_YELLOW_ALERT | Chuẩn bị kế hoạch | Tạo cảnh báo vàng

Tồn dưới mức an toàn | STOCK_RED_ALERT | Cần lập Production Demand | Đưa lên Demand Board

Nguy cơ hết trước kỳ sản xuất | STOCKOUT_RISK | Cần ưu tiên sản xuất | Tăng priority, không tự phát lệnh

Tồn bằng 0 / không sellable | STOCK_ZERO_OR_NOT_SELLABLE | Không đủ hàng bán | Kích hoạt Stock/Sellable Guard

Tồn bị hold/recall/sale lock | STOCK_LOCKED | Không được bán | Chặn Sales/CRM/ADS/Order

Tồn đủ nhưng sắp hết hạn/cần review | STOCK_AGING_REVIEW | Cần QA/owner review | Không tự bán nếu policy chặn

## 2.4. Tồn thành phẩm được tính là available khi nào

Tồn thành phẩm chỉ được tính available khi:

- Batch đã QC đạt.

- Batch đã RELEASED.

- Đã Warehouse Receipt.

- Đã vào Inventory Ledger.

- Không bị hold.

- Không bị recall.

- Không bị sale lock.

- Không bị reserved vượt mức cho đơn/chương trình khác.

- Không hết hạn hoặc cận hạn vượt policy.

- Có trace/QR/public trace nếu policy bắt buộc.

## 2.5. Stock Alert Guard

Guard | Điều kiện kiểm tra | Nếu fail

FINISHED_GOODS_SOURCE_GUARD | Tồn phải lấy từ Inventory Ledger/projection hợp lệ | bị chặn

RELEASE_STATUS_GUARD | Chỉ batch RELEASED mới tính sellable | EXCLUDE

WAREHOUSE_RECEIPT_GUARD | Chỉ hàng đã nhập kho thành phẩm mới tính available | EXCLUDE

HOLD_GUARD | Hàng hold không tính available | EXCLUDE

RECALL_GUARD | Hàng recall không tính available | bị chặn

SALE_LOCK_GUARD | Sale Lock thắng mọi kênh bán | bị chặn

RESERVED_STOCK_GUARD | Reserved stock phải trừ khỏi available | EXCLUDE

HSD_GUARD | Hàng hết hạn/cận hạn theo policy phải chặn hoặc review | REVIEW / bị chặn

TRACE_GUARD | Hàng thiếu trace nếu policy yêu cầu phải chặn | bị chặn

## 3. MATERIAL / PACKAGING STOCK ALERT

## 3.1. Định nghĩa

Material / Packaging Stock Alert là cơ chế cảnh báo tồn nguyên liệu, bao bì, hộp, thùng, tem, nhãn, vật tư in và vật tư phụ.

Material Alert không phải:

- Purchase Requirement.

- Purchase Order.

- Harvest Requirement chính thức.

- Intake.

- QC_PASS.

- READY_FOR_PRODUCTION.

- READY_FOR_PACKAGING.

- Material Issue.

## 3.2. Chuỗi đúng từ Material Alert

Chuỗi đúng:

Material Alert -> Material Requirement Board -> Dynamic Threshold Resolver -> Procurement Suppression Check -> Owner Review -> Purchase / Harvest / Packaging Requirement nếu đủ điều kiện -> Intake -> QC / Check -> Readiness

Không được đi tắt:

Đường đi bị cấm | Lý do

Material Alert -> Purchase Order | Alert không phải quyết định mua

Material Alert -> Harvest Execution | Harvest phải qua Company Source governance

Material Alert -> Intake | Chưa có hàng thực tế

Material Alert -> QC_PASS | Chưa kiểm tra

Material Alert -> READY_FOR_PRODUCTION | Chưa có lot/readiness

Material Alert -> Material Issue | Chưa đủ điều kiện cấp phát

## 3.3. Bảng trạng thái Material / Packaging Alert

Mức cảnh báo | Mã trạng thái | Ý nghĩa | Hành động

Tồn đủ | MATERIAL_OK | Chưa cần nhập thêm | Theo dõi

Tồn gần mức cảnh báo | MATERIAL_YELLOW_ALERT | Chuẩn bị kế hoạch | Tạo cảnh báo vàng

Tồn dưới mức an toàn | MATERIAL_RED_ALERT | Cần xem xét nhập/mua/thu hoạch | Đưa lên Material Requirement Board

Nguy cơ thiếu cho kỳ sản xuất tới | MATERIAL_SHORTAGE_RISK | Cần ưu tiên xử lý | Chạy MRP/owner review

Tồn bằng 0 hoặc chưa ready | MATERIAL_ZERO_OR_NOT_READY | Không đủ điều kiện cấp phát | Chặn Material Issue

Tồn đủ/vượt ngưỡng | MATERIAL_PURCHASE_NOT_REQUIRED | Không cần mua thêm | Loại khỏi phiếu thu mua

Tồn vượt ngưỡng chặn mua | MATERIAL_PURCHASE_SUPPRESSED | Không được đưa vào phiếu thu mua | Suppression

Tồn bị hold/reject/disposal | MATERIAL_NOT_USABLE | Không usable | Loại khỏi available

Sâm đến kỳ thu hoạch | SAVIGIN_HARVEST_DUE | Company Source đến mùa vụ | Harvest Review

Sâm thiếu cho sản xuất | SAVIGIN_PRODUCTION_STOCK_LOW | Ready stock dưới ngưỡng | Cảnh báo sản xuất

## 3.4. Material Alert phải kiểm soát hai chiều

Material Alert không chỉ cảnh báo thiếu.

Material Alert phải kiểm soát cả:

- Thiếu thì đưa vào Material Requirement Board.

- Đủ thì không đưa vào phiếu thu mua.

- Vượt ngưỡng thì loại khỏi phiếu thu mua.

- Không ready thì không tính usable.

- Chờ hủy thì không tính usable.

- Supplier material thì đi procurement.

- Sâm Savigin thì đi Company Source / Harvest / Strategic Reserve.

- B1/B2 thì tính theo output gói/hộp/thùng.

- B3 thì tính theo Stock Policy.

File owner lock đã khóa rõ: hệ thống phải kiểm soát hai chiều, thiếu thì cảnh báo để lập nhu cầu nhập/mua/thu hoạch; đủ hoặc vượt định mức thì tự động loại khỏi phiếu thu mua/mua sắm.

## 4. STOCK POLICY CONFIG

## 4.1. Mục tiêu

Stock Policy Config định nghĩa mức tồn, mức cảnh báo, mức an toàn, mức chặn mua, ngưỡng loại khỏi phiếu thu mua và rule xử lý cho thành phẩm, nguyên liệu, bao bì và vật tư.

Stock Policy không được hardcode trong code, UI, Excel hoặc template.

## 4.2. Stock Policy cho thành phẩm

Trường | Ý nghĩa

sku_id | SKU áp dụng

finished_goods_min_stock | Tồn tối thiểu

finished_goods_safety_stock | Tồn an toàn

yellow_threshold | Cảnh báo vàng

red_threshold | Cảnh báo đỏ

stockout_risk_rule | Rule nguy cơ hết hàng

sellable_policy | Điều kiện sellable

reserved_policy | Cách trừ reserved

hsd_policy | HSD/cận hạn

trace_policy | Trace bắt buộc nếu có

owner_reference | Owner

effective_date | Ngày hiệu lực

audit_reference | Audit

## 4.3. Stock Policy cho nguyên liệu/vật tư

Trường | Ý nghĩa

material_id | Material canonical

material_group | A1/A2/A3/B1/B2/B3

source_type | Company/Supplier/Packaging

min_stock | Tồn tối thiểu

safety_stock | Tồn an toàn

reorder_point | Điểm cần xem xét nhập/mua

max_stock_threshold | Ngưỡng tồn tối đa

suppression_threshold | Ngưỡng loại khỏi phiếu thu mua

dynamic_threshold_policy | Chính sách tự tính ngưỡng

readiness_policy | READY_FOR_PRODUCTION / READY_FOR_PACKAGING

disposal_exclusion_policy | Loại trừ hàng chờ hủy

owner_reference | Owner

effective_date | Ngày hiệu lực

audit_reference | Audit

## 4.4. Thay đổi Stock Policy

Thay đổi Stock Policy phải có:

- Lý do.

- Owner approval.

- Effective date.

- Evidence.

- Audit.

- Impact analysis.

- Smoke nếu ảnh hưởng MRP/Procurement.

- Không hồi tố sai vào lệnh sản xuất/phiếu mua đã đóng.

## 5. SKU–FORMULA–MATERIAL–PACKAGING EXTENSION REGISTRY

## 5.1. Mục tiêu

Extension Registry là cơ chế mở rộng SKU, công thức, nguyên liệu, bao bì, vật tư, stock policy và MRP config khi Ginsengfood phát triển vượt danh mục nền hiện tại.

Nguyên tắc khóa:

Không mở rộng bằng cách sửa đè danh mục cũ.

Không sửa đè G1 đã dùng.

Không tự thêm nguyên liệu/bao bì vào sản xuất nếu chưa qua registry, evidence, smoke và owner approval.

## 5.2. Không sửa đè

Khi mở rộng, không được:

- Sửa đè SKU cũ.

- Đổi nghĩa SKU cũ.

- Dùng lại SKU code cũ cho sản phẩm mới.

- Sửa đè G1 đã dùng.

- Thay material canonical cũ bằng nghĩa mới.

- Dùng lại hộp/thùng SKU khác nếu chưa approved.

- Tự active SKU mới.

- Tự đưa SKU mới vào MRP.

- Tự bán SKU mới khi chưa qua Sellable Gate.

## 5.3. Chuỗi mở rộng SKU mới

SKU mới phải đi qua:

Product Master -> Formula Candidate / Pilot -> Rice Anchor Formula -> G1 Approval -> Recipe/BOM Version -> A1/A2/A3 Mapping -> B1/B2/B3 Mapping -> Yield Policy -> Stock Policy Config -> MRP Config -> QC/HSD/Trace/Recall Config -> Product Activation Guard -> Evidence -> Smoke -> Owner Sign-off

## 5.4. Trường tối thiểu của Extension Registry

Trường | Ý nghĩa

extension_id | Mã hồ sơ mở rộng

extension_type | SKU / FORMULA / MATERIAL / PACKAGING / STOCK_POLICY / MRP_CONFIG

sku_id | SKU liên quan

formula_version | Formula version

formula_status | Pilot / Approved / Retired

anchor_ingredient | Gạo

anchor_policy_reference | Chính sách anchor

ratio_to_rice_reference | Tỷ lệ theo gạo

material_group_mapping | A1/A2/A3

packaging_group_mapping | B1/B2/B3

yield_policy_reference | Sản lượng gói/hộp/thùng

stock_policy_reference | Stock Policy

mrp_config_reference | MRP Config

qc_config_reference | QC Config

hsd_config_reference | HSD Config

trace_config_reference | Trace Config

recall_config_reference | Recall Config

owner_review_status | Owner review

evidence_reference | Evidence

smoke_reference | Smoke

activation_status | Trạng thái active

audit_reference | Audit

## 5.5. Điều kiện active SKU mới

SKU mới không được active nếu thiếu:

- Product Master.

- Formula Candidate/Pilot nếu cần.

- G1 Approved Operational Formula.

- Rice Anchor Policy.

- Ratio_to_rice lines.

- Recipe/BOM Version.

- A1/A2/A3 mapping.

- B1/B2/B3 mapping.

- Yield Policy.

- Packaging Config.

- Print Config nếu cần in.

- Stock Policy.

- MRP Config.

- QC Config.

- HSD Config.

- Trace Config.

- Recall Config.

- Owner approval.

- Evidence.

- Smoke.

## 6. FORMULA EXTENSION / VERSION CONTROL

## 6.1. Khi nào phải tạo Formula Version mới

Bắt buộc tạo Formula Version mới khi:

- Đổi tỷ lệ nguyên liệu.

- Thêm nguyên liệu.

- Bỏ nguyên liệu.

- Đổi anchor policy.

- Đổi UOM policy ảnh hưởng tính toán.

- Đổi density policy ảnh hưởng chất lỏng.

- Đổi rounding policy ảnh hưởng định lượng.

- Đổi yield policy ảnh hưởng sản lượng.

- Đổi quy trình làm ảnh hưởng định lượng/BOM/MRP.

- Đổi công thức sau khi G1 đã được dùng.

## 6.2. Scale mẻ không tạo Formula Version mới

Nếu chỉ đổi anchor_rice_quantity trong phạm vi batch policy và không đổi tỷ lệ, không thêm/bỏ nguyên liệu, thì không tạo version mới.

Hệ thống chỉ tạo:

- Calculation Snapshot.

- Production BOM Snapshot.

- MRP Requirement mới.

- Material Issue plan mới.

- Costing basis nếu áp dụng.

- Trace basis nếu áp dụng.

## 6.3. Không sửa đè G1

G1 đã dùng cho Production Order, Batch, Trace, Inventory hoặc Costing thì không được sửa đè.

Nếu công thức mới tốt hơn, phải tạo G2 hoặc version tiếp theo.

## 7. MATERIAL EXTENSION REGISTRY

## 7.1. Khi nào cần thêm material mới

Cần Material Extension Registry khi:

- SKU mới có nguyên liệu mới.

- Formula Version mới thêm nguyên liệu.

- Nguyên liệu cũ cần tách grade/spec.

- Nguyên liệu cần dietary guard riêng.

- Nguyên liệu cần source policy riêng.

- Nguyên liệu cần QC/HSD/Trace riêng.

- Supplier mới cung cấp nguyên liệu mới.

## 7.2. Điều kiện thêm material mới

Material mới không được dùng trong BOM/MRP nếu thiếu:

- material_id canonical.

- Tên chuẩn.

- Material group.

- UOM policy.

- Density policy nếu cần.

- Source policy.

- Supplier allowlist hoặc Company Source.

- QC/check policy.

- HSD/expiry policy nếu có.

- Trace requirement nếu có.

- Stock Policy.

- MRP Config.

- Owner approval.

- Evidence.

- Smoke.

## 7.3. Không dùng material mơ hồ

Không được dùng các tên mơ hồ như:

- Gia vị chung.

- Rau củ tổng hợp.

- Hải sản.

- Thịt.

- Bao bì.

- Tem.

- Nhãn.

- Phụ liệu.

- Vật tư phụ.

Nếu downstream cần MRP, QC, trace, costing hoặc procurement, phải có material_id cụ thể.

## 8. PACKAGING / PRINT MATERIAL EXTENSION REGISTRY

## 8.1. Khi nào cần thêm bao bì/vật tư mới

Cần Packaging / Print Material Extension Registry khi:

- Thêm hộp SKU mới.

- Thêm thùng carton SKU mới.

- Thêm cuộn màng cấp 1 mới.

- Thêm tem/nhãn mới.

- Thêm mực/ribbon/decal mới.

- Đổi thiết kế bao bì.

- Đổi quy cách đóng gói.

- Đổi vùng in lô/NSX/HSD/mã vạch/QR.

- Thêm trade item mới.

- Thêm carton config mới.

## 8.2. Điều kiện thêm bao bì/vật tư mới

Bao bì/vật tư mới không được dùng nếu thiếu:

- packaging_material_id.

- material_group B1/B2/B3.

- SKU/trade item áp dụng nếu có.

- Quy cách.

- Thiết kế/spec.

- Packaging Config.

- Print Config nếu cần.

- Supplier/source.

- Stock Policy.

- MRP Config.

- Check/QC policy nếu cần.

- Readiness policy.

- Owner approval.

- Evidence.

- Smoke.

## 8.3. Không dùng bao bì sai SKU

Các hành vi bị cấm:

- Dùng hộp SKU A cho SKU B.

- Dùng thùng SKU A cho SKU B nếu chưa approved substitution.

- Dùng cuộn màng sai SKU.

- Dùng vật tư in không tương thích.

- Dùng nhãn sai nội dung.

- Dùng mã vạch/QR sai trade item.

- Dùng carton khi carton config chưa bật.

- In cấp 2 khi chưa có Print Config.

## 9. EVIDENCE PACKET

## 9.1. Vai trò

Evidence Packet là bộ chứng cứ chứng minh PACK-03 đã được triển khai, kiểm tra và vận hành đúng.

Không có Evidence Packet thì không PASS.

Evidence Packet không thay smoke.

Evidence Packet không thay owner sign-off.

## 9.2. Evidence cho Demand / Formula

Evidence | Nội dung

Demand Source Evidence | Nguồn demand

Demand Board Snapshot | Demand Board

Formula Resolution Evidence | G1/formula được dùng

Rice Anchor Evidence | anchor_rice_quantity

Ratio_to_rice Snapshot | Tỷ lệ nguyên liệu theo gạo

Calculation Snapshot | Kết quả scale công thức

Production BOM Snapshot | BOM theo lệnh sản xuất

Owner Approval | Duyệt công thức / demand

Audit Log | Audit

## 9.3. Evidence cho MRP / Threshold / Suppression

Evidence | Nội dung

MRP Run Snapshot | MRP run

Material Requirement Board Snapshot | Bảng nhu cầu vật tư

Dynamic Threshold Snapshot | Ngưỡng tự tính

Stock Snapshot | Tồn kho/tồn khả dụng

Incoming Snapshot | Incoming đã duyệt

Suppression Result | Kết quả loại khỏi phiếu thu mua

Director Approval Evidence | Nếu vượt ngưỡng đặc biệt

Exception Log | STOP_POINTS/exception

Audit Log | Audit

## 9.4. Evidence cho Procurement / Harvest / Intake / Readiness

Evidence | Nội dung

Procurement Requirement | Yêu cầu mua

Supplier Allowlist | Supplier được phép cung cấp

Purchase Request / PO nếu có | Chứng từ mua

Company Source Evidence | Nguồn Sâm công ty

Harvest Requirement | Yêu cầu thu hoạch

Harvest Evidence | Chứng cứ thu hoạch

Raw Material Intake | Phiếu intake

Incoming QC Evidence | QC đầu vào

Raw Material Lot | Lot nguyên liệu

READY_FOR_PRODUCTION Evidence | Readiness nguyên liệu

Packaging Intake / Check | Bao bì/vật tư

READY_FOR_PACKAGING Evidence | Readiness bao bì

Audit Log | Audit

## 9.5. Evidence cho Disposal / Write-off

Evidence | Nội dung

Disposal Request | Yêu cầu hủy

Disposal Reason | Lý do hủy

Material/Lot/Quantity | Vật tư/lô/số lượng

QA/Owner Review | Duyệt hủy

Disposal Execution Evidence | Chứng cứ thực hiện hủy

Inventory Ledger Write-off | Ghi giảm tồn

Accounting Sync Evidence | Nếu có

Audit Log | Audit

## 10. SMOKE MATRIX PACK-03 V2.0

## 10.1. Mục tiêu smoke

Smoke Matrix kiểm tra tối thiểu để chứng minh PACK-03 V2.0 vận hành đúng governance.

Smoke không thay UAT đầy đủ.

Smoke không thay owner sign-off.

## 10.2. Smoke nhóm Demand / Formula

Smoke ID | Tình huống | Kết quả kỳ vọng

P03-V2-FRM-001 | Pilot nhập gạo làm anchor và nhập nguyên liệu kg/g/l/ml | Hệ thống chuẩn hóa UOM, tính ratio_to_rice

P03-V2-FRM-002 | Pilot thêm/bỏ nguyên liệu trước khi khóa G1 | Cho phép, có audit và Formula Candidate Snapshot

P03-V2-FRM-003 | Pilot được duyệt thành G1 | Khóa G1, lưu evidence/owner approval

P03-V2-FRM-004 | Sản xuất nhập anchor_rice_quantity mới | Tự scale nguyên liệu, tạo Calculation Snapshot

P03-V2-FRM-005 | Sản xuất cố sửa tỷ lệ sau G1 | bị chặn / yêu cầu Formula Version mới

P03-V2-FRM-006 | Chỉ scale số kg gạo, không đổi tỷ lệ | Không tạo Formula Version mới

P03-V2-FRM-007 | Liquid material thiếu density/UOM policy khi cần | BLOCKED_CONFIG_MISSING

P03-V2-FRM-008 | Field App cố sửa BOM Snapshot | bị chặn

## 10.3. Smoke nhóm Demand Board

Smoke ID | Tình huống | Kết quả kỳ vọng

P03-V2-DEM-001 | Stock Alert đỏ | Tạo Production Demand, không tạo Production Order

P03-V2-DEM-002 | Dealer Order tồn kho đủ | Fulfillment từ kho, không tạo demand sản xuất

P03-V2-DEM-003 | Dealer Order thiếu một phần | Chỉ phần thiếu vào Demand Board

P03-V2-DEM-004 | SKU bị Sale Lock | bị chặn

P03-V2-DEM-005 | SKU thiếu G1 | FORMULA_BLOCKED

P03-V2-DEM-006 | Demand thiếu evidence | Không approved

## 10.4. Smoke nhóm MRP / Dynamic Threshold / Suppression

Smoke ID | Tình huống | Kết quả kỳ vọng

P03-V2-MRP-001 | MRP từ demand hợp lệ + G1 | Tạo MRP run

P03-V2-MRP-002 | Nhóm A đặc thù tồn >= 5 mẻ/SKU | Loại khỏi phiếu thu mua

P03-V2-MRP-003 | Nhóm A đặc thù tồn < 5 mẻ/SKU | Được đưa vào xét mua nếu có demand

P03-V2-MRP-004 | Nhóm A dùng chung tồn >= aggregate 20 mẻ | Loại khỏi phiếu thu mua

P03-V2-MRP-005 | B1 film tồn >= 15 mẻ/SKU | Loại khỏi phiếu thu mua

P03-V2-MRP-006 | B2 hộp tồn >= 15 mẻ/SKU | Loại khỏi phiếu thu mua

P03-V2-MRP-007 | B2 thùng tồn >= 15 mẻ/SKU | Loại khỏi phiếu thu mua

P03-V2-MRP-008 | B3 tồn đủ Stock Policy | Loại khỏi phiếu mua

P03-V2-MRP-009 | available + incoming >= threshold | PROCUREMENT_SUPPRESSED

P03-V2-MRP-010 | Đơn hàng lớn vượt ngưỡng thiếu Giám đốc duyệt | DIRECTOR_APPROVAL_REQUIRED

P03-V2-MRP-011 | Đơn hàng lớn có Giám đốc duyệt + MRP + evidence | Cho override có kiểm soát

## 10.5. Smoke nhóm Sâm Savigin / Company Source

Smoke ID | Tình huống | Kết quả kỳ vọng

P03-V2-SVG-001 | Sâm đến kỳ thu hoạch dù tồn đủ sản xuất | Tạo Harvest Alert/Requirement, không tạo Purchase Requirement

P03-V2-SVG-002 | Sâm READY_FOR_PRODUCTION dưới production safety threshold | Cảnh báo SAVIGIN_PRODUCTION_STOCK_LOW

P03-V2-SVG-003 | Sâm thu hoạch xong chưa QC | Không tính ready

P03-V2-SVG-004 | Sâm QC_PASS nhưng chưa readiness | Không cấp phát

P03-V2-SVG-005 | Sâm ready đưa vào Strategic Reserve | Tính ready stock, trace đầy đủ

## 10.6. Smoke nhóm Procurement / Intake / Readiness

Smoke ID | Tình huống | Kết quả kỳ vọng

P03-V2-PROC-001 | MRP output thiếu supplier material | Tạo Procurement Requirement, không PO

P03-V2-PROC-002 | Supplier không allowlist | bị chặn

P03-V2-PROC-003 | Supplier Evidence đầy đủ nhưng chưa QC | Không QC_PASS

P03-V2-INT-001 | Raw Material Intake tạo xong | RECEIVED_waiting_QC

P03-V2-QC-001 | Incoming QC_PASS | Chưa tự READY_FOR_PRODUCTION

P03-V2-RDY-001 | QC_PASS + lot + readiness đủ | READY_FOR_PRODUCTION

P03-V2-PKG-001 | Packaging Intake tạo xong | Chưa tự READY_FOR_PACKAGING

P03-V2-PKG-002 | Bao bì sai SKU | bị chặn

P03-V2-PKG-003 | Hộp/thùng thiếu Print Config | BLOCKED_CONFIG_MISSING

P03-V2-PKG-004 | Packaging check đạt + đủ điều kiện | READY_FOR_PACKAGING

## 10.7. Smoke nhóm Disposal / Write-off

Smoke ID | Tình huống | Kết quả kỳ vọng

P03-V2-DSP-001 | Nguyên liệu QC_REJECT cần hủy | Tạo Disposal Request, không usable

P03-V2-DSP-002 | Vật tư EXPIRED cần hủy | Disposal Request, không tính available

P03-V2-DSP-003 | Disposal approved nhưng chưa executed | Chưa write-off, không usable

P03-V2-DSP-004 | Disposal executed | Inventory Ledger Write-off

P03-V2-DSP-005 | Disposal có giá trị kế toán | Accounting/MISA sync waiting

P03-V2-DSP-006 | User cố xóa tay tồn kho | bị chặn

## 10.8. Smoke nhóm Extension Registry

Smoke ID | Tình huống | Kết quả kỳ vọng

P03-V2-EXT-001 | SKU mới thiếu G1 | Không active

P03-V2-EXT-002 | SKU mới thiếu A1/A2/A3 mapping | Không active

P03-V2-EXT-003 | SKU mới thiếu B1/B2 mapping | Không active

P03-V2-EXT-004 | SKU mới thiếu Yield Policy | Không active

P03-V2-EXT-005 | SKU mới thiếu Stock Policy/MRP Config | Không active

P03-V2-EXT-006 | SKU mới đủ config/evidence/smoke | Được đề xuất Product Activation Guard

P03-V2-EXT-007 | Công thức G1 đã dùng bị sửa đè | bị chặn

P03-V2-EXT-008 | Công thức đổi tỷ lệ sau G1 | Yêu cầu Formula Version mới

## 11. DONE GATE PACK-03 V2.0

## 11.1. Documentation Done Gate

Điều kiện | Trạng thái cần đạt

## PHẦN 1/4 hoàn tất	REQUIRED

## PHẦN 2/4 hoàn tất	REQUIRED

## PHẦN 3/4 hoàn tất	REQUIRED

## PHẦN 4/4 hoàn tất	REQUIRED

Demand governance khóa | REQUIRED

Formula Scaling khóa | REQUIRED

G1 Formula Lock khóa | REQUIRED

MRP / Dynamic Threshold khóa | REQUIRED

Procurement Suppression khóa | REQUIRED

Company Source / Sâm khóa | REQUIRED

Supplier / Packaging Source khóa | REQUIRED

Intake / QC / Readiness khóa | REQUIRED

Disposal / Write-off khóa | REQUIRED

Extension Registry khóa | REQUIRED

Evidence / Smoke / Done Gate khóa | REQUIRED

## 11.2. Runtime Done Gate

Runtime | Trạng thái cần đạt trước production

Demand Board Runtime | IMPLEMENTED + TESTED

Formula Resolver Runtime | IMPLEMENTED + TESTED

G1 Lock Runtime | IMPLEMENTED + TESTED

Production BOM Snapshot Runtime | IMPLEMENTED + TESTED

MRP Runtime | IMPLEMENTED + TESTED

Dynamic Threshold Resolver Runtime | IMPLEMENTED + TESTED

Procurement Suppression Runtime | IMPLEMENTED + TESTED

Stock Alert Runtime | IMPLEMENTED + TESTED

Material Alert Runtime | IMPLEMENTED + TESTED

Procurement Runtime | IMPLEMENTED + TESTED

Company Source / Harvest Runtime | IMPLEMENTED + TESTED

Supplier Source Runtime | IMPLEMENTED + TESTED

Intake / QC / Readiness Runtime | IMPLEMENTED + TESTED

Disposal Runtime | IMPLEMENTED + TESTED

Extension Registry Runtime | IMPLEMENTED + TESTED

## 11.3. Evidence Done Gate

Evidence | Trạng thái cần đạt

Demand Evidence Packet | COMPLETE

Formula Evidence Packet | COMPLETE

MRP Evidence Packet | COMPLETE

Threshold / Suppression Evidence | COMPLETE

Procurement Evidence Packet | COMPLETE

Harvest Evidence Packet | COMPLETE

Intake Evidence Packet | COMPLETE

QC / Readiness Evidence Packet | COMPLETE

Disposal Evidence Packet | COMPLETE

Extension Registry Evidence Packet | COMPLETE

Audit Evidence | COMPLETE

## 11.4. Smoke Done Gate

Nhóm smoke | Trạng thái cần đạt

Demand smoke | PASS

Formula smoke | PASS

MRP smoke | PASS

Threshold / Suppression smoke | PASS

Stock Alert smoke | PASS

Material Alert smoke | PASS

Procurement smoke | PASS

Company Source / Sâm smoke | PASS

Supplier smoke | PASS

Packaging smoke | PASS

Intake / QC / Readiness smoke | PASS

Disposal smoke | PASS

Extension Registry smoke | PASS

Sale Lock / Recall / Hold smoke | PASS

## 11.5. Owner Sign-off Gate

PACK-03 V2.0 chỉ được đề xuất release khi có sign-off của:

- Product Master Owner.

- Formula / R&D Owner.

- Operational Planning Owner.

- Material Planning Owner.

- Procurement Owner.

- Company Source Owner.

- Quality Owner.

- Warehouse / Intake Owner.

- Packaging Owner.

- Accounting / MISA Owner nếu có liên quan.

- Operational Core Owner.

- Release Owner.

Nếu thiếu owner sign-off, trạng thái là waiting_OWNER_SIGNOFF.

## 12. RELEASE CONTROL

## 12.1. Không được gọi PASS khi thiếu evidence

Không được gọi PASS nếu:

- Chưa có Evidence Packet.

- Chưa chạy smoke.

- Smoke fail.

- Chưa có owner sign-off.

- Runtime chưa implement.

- Guard chưa hoạt động.

- Audit chưa có.

- Threshold chưa test.

- Suppression chưa test.

- Disposal chưa test.

- Sâm Company Source chưa test.

- Extension Registry chưa test.

## 12.2. Governance Complete không đồng nghĩa Production Ready

PACK-03 V2.0 có thể đạt Governance Complete khi tài liệu đã khóa đầy đủ.

Nhưng Production Ready cần runtime thật, dữ liệu thật, test thật, smoke thật, evidence thật và owner sign-off thật.

## 12.3. Production Ready không đồng nghĩa Release Approved

Production Ready chỉ xác nhận hệ thống có thể vận hành theo điều kiện kiểm thử.

Release Approved cần:

- Completion Gate.

- Release Review.

- Owner sign-off cuối.

- Rollback plan.

- Monitoring plan.

- Evidence đầy đủ.

- Không còn STOP_POINTS P0.

## 12.4. Release Approved không đồng nghĩa Go-live Approved

Go-live Approved là quyết định riêng theo Global Release Governance.

Không được tự đưa PACK-03 vào vận hành thật nếu chưa qua Go-live Gate.

## 13. CROSS-PACK FINAL DEPENDENCY

## 13.1. Phụ thuộc PACK-01

PACK-03 phụ thuộc PACK-01 ở:

- Production Order.

- Material Issue.

- Raw Material Lot.

- Inventory Ledger.

- Warehouse Receipt.

- Finished Goods Inventory.

- Traceability.

- Recall.

- Sale Lock.

- Evidence.

- Audit.

- MISA boundary nếu liên quan.

PACK-03 không tạo source-of-truth song song cho inventory hoặc production.

## 13.2. Phụ thuộc PACK-02

PACK-03 phụ thuộc PACK-02 ở:

- Product Master.

- SKU canonical.

- Ingredient/material canonical.

- Formula Version.

- G1 Approved Operational Formula.

- Recipe/BOM.

- Packaging Config.

- Print Config.

- QC Config.

- HSD Config.

- Trace Config.

- Recall Config.

- Product Activation Guard.

- Sellable dependency.

PACK-03 không tự thêm SKU/công thức/nguyên liệu ngoài Product Master và Extension Registry.

## 13.3. Phụ thuộc MASTER Governance

PACK-03 phải tuân thủ:

- Source-of-Truth Registry.

- Cross-Pack Dependency Map.

- Traceability ID Standard.

- Runtime Resolution & Guard Control.

- Evidence / Smoke / Completion Gate.

- Future Integration Boundary.

- Global Release Governance.

Nếu MASTER Gate chưa đạt, PACK-03 không được tự gọi Gateway PASS hoặc Go-live Approved.

## 14. IMPLEMENTATION HANDOFF BOUNDARY

## 14.1. PACK-03 không viết implementation chi tiết

PACK-03 là governance/domain pack.

PACK-03 không viết chi tiết:

- API route.

- DTO.

- Database schema.

- UI component.

- Worker job.

- Cron job.

- Permission matrix chi tiết.

- Printer/device integration.

- MISA mapping chi tiết.

Các phần này thuộc implementation pack sau.

## 14.2. Implementation sau này cần có tối thiểu

- Production Demand Board.

- Formula Resolver.

- G1 Formula Lock.

- Production BOM Snapshot.

- MRP Run.

- Material Requirement Board.

- Dynamic Threshold Resolver.

- Procurement Suppression Check.

- Stock Alert Dashboard.

- Material Alert Dashboard.

- Company Source / Harvest Board.

- Procurement Requirement Board.

- Raw Material Intake.

- Packaging Intake.

- Incoming QC.

- Readiness Board.

- Disposal / Write-off Board.

- Extension Registry.

- Evidence Packet Registry.

- Smoke / Completion Gate Dashboard.

## 15. RULE TÓM TẮT PHẦN 4/4

Rule ID | Quy tắc | Mức độ

P03-ALERT-P0-001 | Stock Alert không tự tạo Production Order | P0

P03-ALERT-P0-002 | Material Alert không tự tạo Purchase Order | P0

P03-ALERT-P0-003 | Alert phải đi qua Demand/MRP/Requirement Board | P0

P03-STK-P0-001 | Stock Policy không được hardcode | P0

P03-STK-P0-002 | Tồn hold/recall/reject/disposal không tính usable | P0

P03-EXT-P0-001 | SKU mới không được sửa đè danh mục cũ | P0

P03-EXT-P0-002 | G1 đã dùng không được sửa đè | P0

P03-EXT-P0-003 | SKU mới thiếu G1/BOM/config không được active | P0

P03-EXT-P0-004 | Material mới phải qua Material Extension Registry | P0

P03-EXT-P0-005 | Packaging mới phải qua Packaging Extension Registry | P0

P03-EVD-P0-001 | Không evidence thì không PASS | P0

P03-SMK-P0-001 | Không smoke thì không PASS | P0

P03-REL-P0-001 | Không owner sign-off thì không Release Approved | P0

P03-REL-P0-002 | Governance Complete không đồng nghĩa Production Ready | P0

P03-REL-P0-003 | Production Ready không đồng nghĩa Release Approved | P0

P03-REL-P0-004 | Release Approved không đồng nghĩa Go-live Approved | P0

## 16. ACCEPTANCE CRITERIA CHO PHẦN 4/4

## PHẦN 4/4 đạt governance khi:

- Đã khóa Stock Alert thành phẩm.

- Đã khóa Material / Packaging Alert.

- Đã khóa Alert không tự tạo Production Order/Purchase Order.

- Đã khóa Stock Policy Config.

- Đã khóa SKU–Formula–Material–Packaging Extension Registry.

- Đã khóa Formula Extension / Version Control.

- Đã khóa Material Extension Registry.

- Đã khóa Packaging / Print Material Extension Registry.

- Đã khóa Evidence Packet.

- Đã khóa Smoke Matrix V2.0.

- Đã khóa Done Gate.

- Đã khóa Release Control.

- Đã khóa Cross-Pack Final Dependency.

- Đã khóa Implementation Handoff Boundary.

- Chưa gọi Production Ready.

- Chưa gọi Release Approved.

- Chưa gọi Go-live Approved.

## 17. TRẠNG THÁI CUỐI PACK-03 V2.0

Hạng mục | Trạng thái

PACK-03 Documentation | Governance Complete

Demand Board Runtime | waiting_IMPLEMENTATION

Formula Resolver Runtime | waiting_IMPLEMENTATION

G1 Formula Lock Runtime | waiting_IMPLEMENTATION

Production BOM Snapshot Runtime | waiting_IMPLEMENTATION

MRP Runtime | waiting_IMPLEMENTATION

Dynamic Threshold Resolver Runtime | waiting_IMPLEMENTATION

Procurement Suppression Runtime | waiting_IMPLEMENTATION

Stock Alert Runtime | waiting_IMPLEMENTATION

Material Alert Runtime | waiting_IMPLEMENTATION

Procurement Runtime | waiting_IMPLEMENTATION

Company Source / Harvest Runtime | waiting_IMPLEMENTATION

Supplier Source Runtime | waiting_IMPLEMENTATION

Packaging Source Runtime | waiting_IMPLEMENTATION

Material / Packaging Intake Runtime | waiting_IMPLEMENTATION

QC / Readiness Runtime | waiting_IMPLEMENTATION

Disposal / Write-off Runtime | waiting_IMPLEMENTATION

Extension Registry Runtime | waiting_IMPLEMENTATION

Evidence Packet | waiting_EVIDENCE

Smoke Run | waiting_SMOKE

Owner Sign-off | waiting_OWNER_SIGNOFF

Production Ready | KHÔNG

Release Approved | KHÔNG

Go-live Approved | KHÔNG

## 18. PACK-03 V2.0 FINAL CONCLUSION

PACK-03 V2.0 hoàn tất vai trò khóa governance cho lớp Demand / MRP / Formula Scaling / Procurement / Stock Alert / Material & Packaging Control của Ginsengfood.

Từ thời điểm này, toàn bộ nhu cầu sản xuất phải đi qua chuỗi kiểm soát:

Production Demand -> Demand Board -> Formula Resolution -> G1 / Production BOM Snapshot -> MRP -> Dynamic Threshold Resolver -> Procurement Suppression Check -> Material Requirement Board -> Procurement / Harvest / Intake / QC / Readiness -> Production Order Draft -> Owner Approval -> Production Order chính thức

Không được đi tắt từ Sales, Dealer, Distributor, Stock Alert, Material Alert, ADS, CRM, Landing Page, AI Advisor, MC AI, Order, Payment, Shipping hoặc MISA sang Production Order.

Không được đi tắt từ MRP sang Purchase Order.

Không được đưa nguyên liệu/vật tư đủ hoặc vượt ngưỡng vào phiếu thu mua nếu không có ngoại lệ được duyệt.

Không được sửa công thức G1 đã khóa.

Không được để bộ phận sản xuất thêm/bớt nguyên liệu hoặc sửa tỷ lệ trong sản xuất.

Không được tính bao bì trực tiếp theo kg gạo nếu chưa qua yield gói/hộp/thùng.

Không được xử lý Sâm Savigin như nguyên liệu supplier.

Không được xóa tay tồn kho khi hủy nguyên liệu/vật tư.

Không được active SKU mới nếu thiếu Product Master, G1 Formula, BOM, A1/A2/A3, B1/B2/B3, Yield Policy, Stock Policy, MRP Config, QC/HSD/Trace/Recall Config, evidence, smoke và owner approval.

Kết luận khóa cuối của PACK-03 V2.0:

Ginsengfood không sản xuất theo cảm tính.

Ginsengfood không mua nguyên liệu theo cảnh báo thô.

Ginsengfood không cho sản xuất sửa công thức G1.

Ginsengfood không mua vật tư nếu tồn đã đủ hoặc vượt ngưỡng.

Ginsengfood không xem Sâm Savigin là nguyên liệu mua ngoài, mà quản trị như Company Source theo mùa vụ và tồn dự trữ chiến lược.

Ginsengfood không xóa tay hàng hủy khỏi tồn kho, mà phải qua Disposal & Write-off Control.

Mọi nhu cầu sản xuất, công thức, MRP, threshold, procurement, harvest, intake, readiness, alert, disposal và extension đều phải có Source-of-Truth, Runtime Resolver, Guard, Evidence, Audit, Smoke và Owner Sign-off.

PACK-03 V2.0 đạt Governance Complete.

Các trạng thái runtime, evidence, smoke, owner sign-off, production ready, release approved và go-live approved vẫn giữ đúng:
