# PACK-01 - OPERATIONAL CORE

## PRODUCTION–WAREHOUSE–INVENTORY–TRACE–RECALL–SELLABLE CONTROL PACK

## PHẦN 1/4 — OPERATIONAL CORE PRINCIPLES / FIELD OPERATIONS APP / OWNER BOUNDARY / PRODUCT TRUTH MODEL

## 0. DOCUMENT CONTROL

## 0.1. Mục tiêu tài liệu

PACK-01 là tài liệu kỹ thuật chính thức dùng để kiểm soát phân hệ Operational Core của hệ thống Ginsengfood.

Tài liệu này khóa các nguyên tắc vận hành cốt lõi cho toàn bộ chuỗi:

- Nguồn nguyên liệu.

- Nhập nguyên liệu đầu vào.

- QC nguyên liệu.

- Lot nguyên liệu.

- Lệnh sản xuất.

- Cấp phát nguyên liệu theo lot.

- Thực thi sản xuất.

- Batch thành phẩm.

- Đóng gói cấp 1.

- Đóng gói cấp 2 hộp/thùng.

- In mã.

- QC sau sấy.

- QC thành phẩm.

- Release batch.

- Nhập kho thành phẩm.

- Tồn kho.

- Truy xuất.

- Thu hồi.

- Sale Lock / Stop Sale.

- Kết nối hạch toán MISA.

- Evidence hiện trường.

- Smoke vận hành.

- App nội bộ hiện trường.

PACK-01 là tài liệu nền để xác định khi nào một sản phẩm thật sự đủ điều kiện đi tiếp sang bán hàng, tư vấn AI, CRM, ADS, MC AI, Gateway, Order, Payment và Shipping.

## 0.2. Vai trò của tài liệu

PACK-01 không phải tài liệu code.

PACK-01 không mô tả chi tiết:

- Migration SQL.

- DTO.

- Controller.

- Service implementation.

- UI component.

- Worker implementation.

- Prompt AI.

- Cấu hình thiết bị thật.

- Mapping MISA thật.

- GTIN/GS1 thật.

- Danh sách nhân sự thật.

PACK-01 dùng để khóa:

- Owner.

- Boundary.

- Source-of-truth.

- Field Operations App boundary.

- Product truth model.

- Operational flow.

- Registry bắt buộc.

- Resolver bắt buộc.

- Guard bắt buộc.

- Evidence bắt buộc.

- Smoke bắt buộc.

- No-hardcode rule.

- Done Gate.

- Release handoff.

## 0.3. Nguyên tắc tài liệu sạch

Tài liệu này được trình bày như một tài liệu chính thức độc lập.

Trong PACK-01:

- Không nhắc quá trình rà soát.

- Không để dev tự suy luận nghiệp vụ.

- Không để consumer tự suy luận dữ liệu vận hành.

- Không dùng placeholder như dữ liệu thật.

- Không gọi PASS khi chưa có evidence thật.

## 1. CORE SCOPE

## 1.1. Phạm vi phân hệ

PACK-01 kiểm soát các domain sau:

- Source Origin.

- Supplier / Company Source.

- Raw Material Intake.

- Raw Material QC.

- Raw Material Lot Readiness.

- Recipe / BOM Snapshot.

- Production Order.

- Material Issue.

- Material Receipt.

- Production Execution.

- Batch.

- Personnel Check-in / Check-out.

- Field Operations Internal App / Mobile PWA.

- Packaging Level 1.

- Packaging Level 2 — Hộp / Thùng.

- Printing.

- QC sau sấy.

- QC thành phẩm.

- Batch Release.

- Warehouse Receipt.

- Inventory Ledger.

- Traceability.

- Recall.

- Sale Lock / Stop Sale.

- MISA Integration Boundary.

- Operational Evidence Packet.

- Operational Smoke Run.

## 1.2. Operational Core là owner của sự thật vận hành

Operational Core là nơi tạo và giữ sự thật vận hành của sản phẩm.

Operational Core trả lời các câu hỏi:

- Nguyên liệu đến từ đâu?

- Nguyên liệu thuộc lot nào?

- Lot đã QC chưa?

- Lot có được phép đưa vào sản xuất không?

- SKU dùng công thức/version nào?

- Nguyên liệu nào được cấp cho mẻ nào?

- Batch thành phẩm được tạo từ mẻ nào?

- Batch có truy được về raw material lot không?

- Batch đã QC chưa?

- Batch đã release chưa?

- Batch đã nhập kho chưa?

- Tồn kho khả dụng là bao nhiêu?

- Hộp/thùng đã in mã nào?

- QR/barcode map về batch nào?

- Sản phẩm có bị hold, recall, sale lock không?

- Sản phẩm có đủ điều kiện bán không?

Không domain bán hàng nào được tự quyết định các câu hỏi trên.

## 1.3. Field Operations App là phạm vi bắt buộc

Sản xuất, QC, đóng gói, nhập kho, truy xuất và thu hồi là hành động hiện trường.

Do đó, PACK-01 bắt buộc có lớp:

## FIELD OPERATIONS INTERNAL APP / MOBILE PWA

Lớp này dùng cho:

- Chụp ảnh.

- Quay video.

- Upload file.

- Scan QR/barcode.

- Check-in/check-out.

- Click xác nhận.

- Ghi nhận số liệu thực tế.

- Ghi nhận % độ ẩm.

- Ghi nhận hao hụt.

- Ghi lỗi in.

- Gửi yêu cầu reprint.

- Xác nhận công đoạn.

- Đồng bộ offline.

- Ghi mobile event audit.

- Gắn evidence vào phiếu/lệnh/batch/lot.

PACK-01 không được hiểu là chỉ triển khai Admin Web.

## 1.4. Admin Web và Field App phải tách vai trò

Kênh | Vai trò chính | Không được làm

Admin Web | Quản trị master data, cấu hình, review, phê duyệt, dashboard, audit, báo cáo | Không thay thế thao tác hiện trường bắt buộc

Field Operations App / Mobile PWA | Capture thao tác hiện trường, ảnh/video, scan, check-in, click-confirm, evidence, offline sync | Không owner business truth, không bypass backend

Device / Printer | Nhận payload, in, trả trạng thái, heartbeat, báo lỗi thiết bị | Không sinh business truth

Backend Operational Core | Validate, mutate state, enforce guard, ghi audit, tạo source-of-truth | Không bỏ qua permission/evidence/audit

## 2. CURRENT OPERATIONAL LOCKS

## 2.1. In cấp 2 trên hộp và thùng

In cấp 2 áp dụng cho:

- Hộp.

- Thùng.

Cả hộp và thùng đều phải in tối thiểu:

- Lô / batch.

- NSX.

- HSD.

- Mã vạch.

- QR.

Nếu hộp và thùng là trade item thương mại, mỗi cấp phải có GTIN/GS1 riêng khi Trade Item Registry chính thức sẵn sàng.

Quy tắc bắt buộc:

- Hệ thống sinh dữ liệu in.

- Máy in chỉ nhận payload.

- Người vận hành không nhập tay lô/NSX/HSD/barcode/QR ở flow thường.

- QR là mã truy xuất/minh bạch.

- Barcode thương mại phải lấy từ Trade Item Registry nếu GTIN/GS1 đã active.

- Không có hai barcode thương mại trên cùng một trade item.

## 2.2. Kho / location thật ban đầu

Kho thật ban đầu:

Kho Bến Tre

Quy tắc:

- Kho Bến Tre là warehouse thật ban đầu.

- Kho Bến Tre phải được quản trị qua Warehouse Registry.

- Không hardcode Kho Bến Tre trong code, UI, AI, CRM, ADS, Gateway hoặc template.

- Location cụ thể bên trong kho cấu hình theo Warehouse Location Registry.

- Warehouse receipt, inventory ledger và stock balance phải gắn warehouse_id / location_id.

- Khi mở thêm kho, chỉ cập nhật registry, không sửa business logic.

## 2.3. Máy in / template in thật

Trạng thái hiện tại:

RESERVED / waiting_DEVICE

Quy tắc:

- Chừa ngõ kết nối khi có thiết bị.

- Chừa ngõ cấu hình template in.

- Không hardcode model máy in.

- Không hardcode template in.

- Hệ thống sinh print payload.

- Máy in chỉ nhận payload và trả trạng thái.

- Printer callback không được pass QC.

- Printer callback không được release batch.

- Printer callback không được nhập kho.

- Printer callback không được tạo inventory.

## 2.4. GTIN/GS1 hộp / thùng

Trạng thái hiện tại:

RESERVED / waiting_REGISTRY

Quy tắc:

- Hộp là trade item có thể có GTIN/GS1 riêng.

- Thùng là trade item có thể có GTIN/GS1 riêng.

- GTIN/GS1 phải lấy từ Trade Item Registry.

- Không cho user nhập tay barcode thương mại.

- Không sinh barcode nội bộ thứ hai trên cùng trade item.

- QR không thay thế GTIN/GS1.

- Trade item chưa có GTIN/GS1 hợp lệ thì chưa đủ điều kiện thương mại hóa ở cấp đó nếu kênh bán yêu cầu barcode thương mại.

## 2.5. Mapping MISA thật

Trạng thái hiện tại:

RESERVED / waiting_MAPPING

Các mapping cần chừa ngõ:

- Kho.

- Location nếu cần.

- Nguyên liệu.

- SKU.

- Thành phẩm.

- Nhà cung cấp.

- Tài khoản kế toán.

- Mã chi phí.

- Mã tập hợp chi phí.

- Nhân sự/bộ phận nếu cần.

- Doanh thu/chiết khấu/hoa hồng nếu thuộc scope sau.

Quy tắc:

- MISA dùng một integration layer chung.

- Không module nào sync riêng lẻ.

- Không có mapping thì không sync chính thức.

- Sync phải có status / retry / error / reconcile.

- MISA không điều khiển lệnh sản xuất.

- MISA không điều khiển mẻ sản xuất.

- MISA không điều khiển công thức.

- MISA không điều khiển traceability.

- MISA không điều khiển recall.

- MISA không điều khiển print logic.

- MISA không điều khiển check-in vận hành.

## 2.6. QC threshold độ ẩm

Ngưỡng QC sau sấy thăng hoa:

Độ ẩm | Trạng thái | Hành động

< 2% | PASS | Được xét đi tiếp

2%–4% | HOLD / REVIEW | QC review, chưa downstream tự động

> 4% | FAIL | Chặn downstream

Quy tắc:

- Độ ẩm dưới 2% được xem là đạt.

- Độ ẩm từ 2% đến 4% không tự động đạt.

- Độ ẩm trên 4% không đạt.

- QC_PASS không tự release batch.

- Batch chỉ nhập kho sau khi có Batch Release hợp lệ.

## 2.7. HSD

HSD mặc định:

12 tháng

Quy tắc:

- HSD có thể theo từng SKU.

- HSD có thể theo nhóm SKU.

- Nếu chưa có khác biệt, mặc định dùng 12 tháng.

- HSD sinh từ NSX/MFG date + shelf-life config.

- Không nhập tay HSD trong flow thường.

- Override HSD là high-risk action, phải có reason, approval và audit.

## 2.8. Source zone / source origin / supplier

Vùng trồng Sâm Savigin:

Nguồn công ty

Quy tắc:

- Vùng trồng Sâm là nguồn nội bộ của công ty.

- Không xử lý vùng trồng Sâm như supplier ngoài thông thường.

- Supplier nguyên liệu khác tích hợp Supplier Portal khi đủ điều kiện.

- Supplier đủ điều kiện được cấp tài khoản để nhập ảnh/video/thông tin vùng trồng.

- Supplier upload evidence không tự động PASS.

- Source Verification Owner phải review và xác nhận.

## 2.9. Nhân sự / role / permission thật

Quy tắc:

- Không hardcode tên người.

- Không hardcode role.

- Không hardcode permission.

- Role / Permission Registry phải kiểm soát high-risk action.

- Mỗi phiếu/lệnh quan trọng phải có xác nhận cá nhân.

- Không đủ xác nhận bắt buộc thì không chuyển bước.

## 3. OPERATIONAL CORE PRINCIPLES

## 3.1. Operational Core là nguồn sự thật vận hành

Operational Core là nguồn sự thật cho:

- Nguyên liệu đầu vào.

- Source origin.

- Supplier/company source.

- Raw material lot.

- QC nguyên liệu.

- Lot readiness.

- Công thức/version dùng cho sản xuất.

- Lệnh sản xuất.

- BOM snapshot.

- Cấp phát nguyên liệu theo lot.

- Thực thi sản xuất.

- Batch thành phẩm.

- Đóng gói cấp 1.

- Đóng gói cấp 2 hộp/thùng.

- Print payload.

- QC sau sấy.

- QC thành phẩm.

- Batch release.

- Warehouse receipt.

- Inventory ledger.

- Traceability.

- Recall.

- Sale Lock / Stop Sale.

- Evidence hiện trường.

AI, CRM, ADS, MC AI, Gateway, Landing Page, Order, Payment, Shipping không được tự tạo sự thật vận hành.

## 3.2. Product Truth phải dựa trên record, không dựa trên lời nói

Product Truth không được xác định bằng:

- Nhân sự nói đã làm xong.

- Ảnh/video không gắn phiếu.

- Excel rời.

- MISA đã ghi nhận.

- Máy in đã in.

- QC nói miệng.

- Kho đã scan nhưng chưa confirm.

- AI nói còn hàng.

- ADS đang chạy.

- CRM đang gửi.

- MC AI đang live.

- Owner chat nhưng chưa có sign-off/evidence.

Product Truth phải có:

- Record.

- Trace ID.

- Audit.

- Evidence.

- Owner/role.

- Timestamp.

- Correlation ID.

## 3.3. Không có đường tắt từ sản xuất sang bán hàng

Các trạng thái sau không đồng nghĩa sellable:

- Production complete.

- Execution complete.

- Batch created.

- Packaging completed.

- Printing completed.

- QC_PASS.

- Batch RELEASED nhưng chưa nhập kho.

- Warehouse scan nhưng chưa confirm.

- MISA sync success.

- AI/CRM/ADS/MC AI có nhu cầu bán.

Luồng đúng:

Production Execution -> Batch Create -> QC Inspection -> Batch Release -> Warehouse Receipt Confirmed -> Inventory Ledger -> Stock Available -> Trace Ready -> Recall Ready -> Sellable Gate

## 3.4. Inventory chỉ tăng tại Warehouse Receipt Confirmed

Inventory không được tăng tại:

- Production complete.

- Batch create.

- Packaging complete.

- Printing complete.

- QC pass.

- Scan inbound.

- MISA sync.

- AI demand.

- CRM demand.

- ADS demand.

- Manual note.

Inventory chỉ tăng sau:

Warehouse Receipt Confirmed

và phải ghi qua:

Inventory Ledger

## 3.5. QC_PASS không đồng nghĩa RELEASED

QC_PASS là kết quả kiểm tra.

RELEASED là quyết định giải phóng batch.

Hai trạng thái này phải tách.

Batch chỉ được nhập kho khi:

- QC phù hợp.

- Disposition phù hợp.

- Batch Release record tồn tại.

- Release action có role/permission/audit.

- Không hold/reject/block.

## 3.6. Traceability là nền của Recall

Recall không được dựng chain riêng.

Recall phải dùng traceability chain:

Raw Material Lot -> Material Issue -> Production Execution -> Batch -> Packaging L2 Hộp/Thùng -> Print Payload -> QR Registry -> Warehouse -> Allocation/Shipment/Exposure -> Recall

Nếu trace chain đứt, recall readiness chưa đạt.

## 3.7. Sale Lock thắng mọi hành vi bán hàng

Sale Lock / Stop Sale thắng:

- AI Advisor.

- CRM.

- ADS.

- MC AI.

- Gateway.

- Landing Page.

- Order.

- Quote.

- Payment request.

- Shipping allocation.

- Promotion.

- Member benefit.

- Diamond benefit.

- Revenue target.

Khi Sale Lock active, Consumer phải suppress.

## 4. FIELD OPERATIONS APP PRINCIPLES

## 4.1. Field App là kênh thao tác hiện trường bắt buộc

Các nghiệp vụ hiện trường phải có Field Operations App / Mobile PWA hoặc giao diện hiện trường tương đương:

- Nhận nguyên liệu đầu vào.

- Chụp ảnh/video nguyên liệu.

- QC nguyên liệu.

- Ghi lot nguyên liệu.

- Check-in/check-out nhân sự.

- Xác nhận cấp nguyên liệu.

- Xác nhận nhận nguyên liệu tại xưởng.

- Theo dõi sơ chế.

- Ghi hao hụt.

- QC sau sấy.

- Ghi % độ ẩm.

- Đóng gói cấp 1.

- Đóng gói cấp 2 hộp/thùng.

- Scan/kiểm tra mã in.

- Ghi lỗi in.

- Gửi yêu cầu reprint.

- QC thành phẩm.

- Scan nhập kho.

- Click confirm nhập kho.

- Scan truy vết nội bộ.

- Ghi evidence recall/recovery/disposition.

- Xác nhận hoàn tất công đoạn.

## 4.2. Field App là capture channel, không phải Owner Core

Field App chỉ được:

- Capture dữ liệu hiện trường.

- Gửi command.

- Upload media.

- Scan QR/barcode.

- Ghi nhận check-in/check-out.

- Ghi nhận số liệu thực tế.

- Gửi click-confirm.

- Sync offline event.

- Hiển thị dữ liệu backend trả về.

- Hiển thị cảnh báo guard/sale lock.

Field App không được:

- Tự pass QC.

- Tự release batch.

- Tự tăng inventory.

- Tự sửa công thức.

- Tự sửa batch.

- Tự tạo inventory.

- Tự xác nhận MISA sync.

- Tự bỏ qua sale lock.

- Tự tạo QR/barcode.

- Tự xác định sellable.

- Tự sửa HSD.

- Tự sửa GTIN/GS1.

- Tự bypass backend validation.

## 4.3. Backend mới là nơi validate và mutate state

Mọi command từ Field App phải đi qua:

- Backend API.

- Domain service.

- Resolver.

- Guard.

- Permission.

- Idempotency.

- Audit.

- State transition.

- Evidence binding.

- Event/outbox nếu cần.

Field App không được mutate state trực tiếp vào database.

Field App không được bỏ qua service validation.

## 4.4. Mọi action hiện trường phải có actor / role / device / object

Mỗi action từ Field App phải có tối thiểu:

- actor_id.

- actor_name_snapshot nếu cần.

- role_code.

- department_code.

- device_id.

- device_code.

- device_uuid.

- app_version.

- platform.

- action_time.

- object_type.

- object_id / object_code.

- action_type.

- client_event_id.

- idempotency_key.

- audit_id.

- correlation_id.

Nếu thuộc sản xuất, phải thêm:

- production_order_id.

- production_profile_id nếu có.

- production_execution_id nếu có.

- batch_id nếu có.

- work_step_code nếu có.

Nếu thuộc nguyên liệu, phải thêm:

- raw_material_receipt_id.

- raw_material_lot_id.

- raw_material_qc_id nếu có.

Nếu thuộc đóng gói/in, phải thêm:

- packaging_job_id.

- print_job_id.

- print_payload_id nếu có.

Nếu thuộc kho, phải thêm:

- warehouse_receipt_id.

- warehouse_id.

- warehouse_location_id.

Nếu thuộc recall, phải thêm:

- recall_case_id.

- hold_id nếu có.

- sale_lock_id nếu có.

- recovery_item_id nếu có.

- disposition_id nếu có.

## 4.5. Device identity là bắt buộc

Mọi request từ Field App phải có tối thiểu:

- X-Request-Id.

- X-Device-Code.

- X-Device-UUID.

- X-App-Version.

- X-Platform.

- X-Offline-Mode.

- X-Client-Event-Id.

- Idempotency-Key.

Thiết bị chưa đăng ký phải bị chặn.

Thiết bị bị revoke phải bị chặn.

Thiết bị dùng app version không còn được phép vận hành phải bị chặn hoặc yêu cầu cập nhật.

## 4.6. Offline queue là bắt buộc

Field App phải hỗ trợ offline queue cho:

- Phiếu hiện trường.

- Check-in/check-out.

- QC result.

- Scan.

- Media evidence.

- Packaging result.

- Print error.

- Warehouse scan.

- Recall evidence.

- Click-confirm được phép offline theo policy.

Mỗi offline event phải có:

- client_event_id.

- local_created_at.

- device_id.

- actor_id.

- action_type.

- object_type.

- object_id.

- payload_hash.

- idempotency_key.

- sync_status.

- retry_count.

- last_error.

- audit_ref khi sync thành công.

Retry không được sinh duplicate:

- duplicate receipt;

- duplicate QC;

- duplicate media;

- duplicate confirmation;

- duplicate warehouse receipt;

- duplicate recall action;

- duplicate print/reprint action.

## 4.7. Media evidence không được mồ côi

Ảnh/video/file hiện trường phải gắn object.

Media không được upload kiểu rời.

Media evidence phải gắn ít nhất một trong các object sau:

- raw_material_receipt_id.

- raw_material_receipt_item_id.

- raw_material_lot_id.

- raw_material_qc_id.

- production_order_id.

- production_execution_id.

- process_event_id.

- packaging_job_id.

- print_job_id.

- print_error_id.

- qc_inspection_id.

- warehouse_receipt_id.

- trace_result_id.

- recall_case_id.

- recovery_item_id.

- disposition_id.

- capa_id nếu có.

Media không gắn object không được dùng làm evidence PASS.

## 4.8. Scan không đồng nghĩa confirm

Scan là capture/verify.

Scan không được tự đổi state nghiệp vụ.

Các rule bắt buộc:

- Scan inbound không đồng nghĩa nhập kho.

- Scan mã in không đồng nghĩa QC pass.

- Scan QR không đồng nghĩa trace complete.

- Scan thùng/hộp không đồng nghĩa sellable.

- Scan nguyên liệu không đồng nghĩa lot ready.

- Scan hàng thu hồi không đồng nghĩa recall close.

Muốn đổi state phải có command riêng:

- Confirm receipt.

- Accept QC.

- Release batch.

- Complete packaging.

- Confirm warehouse receipt.

- Activate hold.

- Activate sale lock.

- Close recall.

- Mark recovery.

## 4.9. Click-confirm là command có permission và audit

Click xác nhận không phải thao tác UI đơn giản.

Các action sau là high-risk command:

- Confirm raw material intake.

- Submit QC.

- Accept QC.

- Hold QC.

- Reject QC.

- Approve material for production.

- Start execution.

- Pause execution.

- Resume execution.

- Complete execution.

- Complete packaging.

- Start print.

- Reprint.

- Accept finished goods QC.

- Release batch.

- Confirm warehouse receipt.

- Inventory adjustment.

- Activate hold.

- Activate sale lock.

- Close recall.

- Close with residual risk.

- Override / break-glass.

Mỗi high-risk command phải có:

- permission.

- idempotency_key.

- actor_id.

- role_code.

- device_id nếu từ app.

- reason nếu cần.

- audit.

- state transition log.

- evidence nếu thuộc scope.

## 5. FIELD APP USE CASE REGISTRY

## 5.1. Raw Material Intake

Field App phải hỗ trợ:

- Chọn supplier/source từ registry.

- Chọn nguyên liệu từ master.

- Nhập số lượng giao.

- Chụp ảnh/video nguyên liệu.

- Upload hóa đơn/phiếu giao hàng nếu có.

- Ghi tình trạng ban đầu.

- Ghi kết quả đánh giá.

- Ghi lý do không đạt.

- Click xác nhận theo role.

- Sync offline nếu mất mạng.

Không được nhập tay tên nguyên liệu thay cho master.

## 5.2. Raw Material QC / Lot Readiness

Field App phải hỗ trợ:

- Xem lot cần QC.

- Ghi kết quả QC.

- Chụp ảnh/video defect.

- Accept / Hold / Reject theo permission.

- Ghi reason khi hold/reject.

- Gắn evidence vào lot/QC.

- Không cho lot chưa ready vào material issue.

## 5.3. Production Order / Material Issue

Field App phải hỗ trợ:

- Xem lệnh sản xuất.

- Xem BOM snapshot.

- Xem danh sách nguyên liệu tự hiện.

- Scan/check lot nguyên liệu.

- Xác nhận nhận nguyên liệu tại xưởng.

- Ghi chênh lệch nếu có.

- Không cho chọn tay nguyên liệu ngoài BOM.

- Không cho issue lot chưa QC_PASS + READY_FOR_PRODUCTION.

## 5.4. Personnel Check-in / Check-out

Field App phải hỗ trợ:

- Check-in nhân sự.

- Check-out nhân sự.

- Gắn production_order_id.

- Gắn batch/mẻ nếu có.

- Gắn công đoạn.

- Gắn ca.

- Ghi tổng giờ làm.

- Ghi role.

- Xác nhận tổ trưởng/quản lý.

- Audit.

Check-in/check-out không được là ghi chú text.

## 5.5. Production Execution

Field App phải hỗ trợ:

- Start execution.

- Pause execution.

- Resume execution.

- Complete execution.

- Ghi process event.

- Ghi output.

- Ghi loss.

- Ghi incident.

- Chụp evidence nếu có lỗi.

- Không tự sinh batch nếu backend guard không cho phép.

## 5.6. QC sau sấy

Field App phải hỗ trợ:

- Ghi số lượng bán thành phẩm kiểm tra.

- Ghi % độ ẩm.

- Ghi cảm quan.

- Chụp ảnh/video sau sấy.

- Đính file đo độ ẩm nếu có.

- Duyệt cho đóng gói / từ chối / hold theo permission.

- Chặn downstream nếu độ ẩm từ 2% đến 4% hoặc trên 4%.

## 5.7. Packaging cấp 1

Field App phải hỗ trợ:

- Xem packaging job cấp 1.

- Xem batch.

- Xem rule in cấp 1.

- Ghi số lượng đầu vào.

- Ghi số lượng đạt.

- Ghi số lượng không đạt.

- Ghi lý do không đạt.

- Chụp ảnh/video.

- Complete packaging theo permission.

- Không tạo inventory.

## 5.8. Packaging cấp 2 — Hộp / Thùng

Field App phải hỗ trợ:

- Xem packaging job cấp 2.

- Phân biệt hộp và thùng.

- Xem batch/lô.

- Xem nội dung in cấp 2.

- Xem barcode/QR rule.

- Chọn máy in từ registry nếu có thiết bị.

- Start print.

- Scan kiểm tra mã in.

- Ghi print error.

- Gửi reprint request có reason.

- Complete packaging.

- Không nhập tay lô/NSX/HSD/barcode/QR.

- Không tạo inventory.

## 5.9. Finished Goods QC

Field App phải hỗ trợ:

- Xem batch/packaging context.

- Ghi số lượng kiểm tra.

- Ghi số lượng đạt.

- Ghi số lượng không đạt.

- Ghi tỷ lệ đạt.

- Chụp evidence.

- Submit / approve / reject theo permission.

- Không tự release batch.

## 5.10. Warehouse Receipt

Field App phải hỗ trợ:

- Scan inbound.

- Xem batch RELEASED.

- Xem warehouse/location.

- Ghi số lượng nhận.

- Chụp ảnh/video nhập kho.

- Click confirm receipt theo permission.

- Không tăng inventory khi chỉ scan.

- Inventory chỉ tăng sau confirm receipt backend PASS.

## 5.11. Traceability / Recall

Field App phải hỗ trợ:

- Scan QR/barcode.

- Resolve trace nội bộ theo quyền.

- Chụp evidence sản phẩm/lô/batch.

- Ghi recovery item.

- Ghi disposition evidence.

- Ghi contact/recovery result nếu thuộc scope.

- Gắn recall_case_id.

- Không close recall nếu thiếu owner approval.

- Không gỡ Sale Lock nếu thiếu recovery evidence/sign-off.

## 6. OWNER BOUNDARY MODEL

## 6.1. Owner Boundary tổng quan

Domain | Owner Core | Consumer

SKU canonical | Product Master Owner | Operational Core

Formula / BOM | Recipe Owner | Production

Operational Config | Operational Owner | Production, Packaging, QC, Trace

Field Operations App | Field App Owner | Operational domains

Device / Mobile Identity | Device Owner | Field App, Audit

Source Origin | Source Origin Owner | Raw Intake

Raw Material Lot | Raw Material Owner | Production, Trace, Recall

Production Order | Production Owner | Material Issue, Execution

Material Issue | Warehouse / Production Owner | Execution, Genealogy

Production Execution | Production Owner | Batch

Batch | Production / QC Owner | Packaging, QC, Warehouse, Trace

Packaging | Packaging Owner | Printing, QC

Printing | Printing Owner | Trace, QC

QC | QC Owner | Release

Batch Release | QC / Operational Owner | Warehouse

Warehouse Receipt | Warehouse Owner | Inventory

Inventory Ledger | Inventory Owner | Sellable Gate

Traceability | Traceability Owner | Recall

Recall | Recall Owner | Sale Lock

Sale Lock / Stop Sale | Recall / Risk Owner | AI, CRM, ADS, Gateway, MC AI, Order

MISA Integration | MISA Integration Owner | Accounting

Evidence / Smoke | QA / Evidence Owner | Release Gate

## 6.2. Field App Owner không thay Domain Owner

Field App Owner chịu trách nhiệm:

- App channel.

- Offline queue.

- Device binding.

- Media capture.

- Sync health.

- Mobile event audit.

- User experience hiện trường.

Field App Owner không thay:

- QC Owner.

- Production Owner.

- Warehouse Owner.

- Inventory Owner.

- Recall Owner.

- MISA Owner.

- Recipe Owner.

- Product Master Owner.

App chỉ gửi action đến domain owner tương ứng.

## 6.3. Consumer không được override Operational Truth

Các Consumer không được override:

- Stock.

- Sellable.

- Batch status.

- Release status.

- Recall status.

- Sale Lock.

- HSD.

- GTIN/GS1.

- QR registry.

- Warehouse receipt.

- Inventory ledger.

- MISA sync status.

- Evidence status.

- Smoke status.

Consumer muốn dùng dữ liệu phải đọc qua resolver/projection hợp lệ.

Projection không thay source-of-truth.

## 7. PRODUCT TRUTH MODEL

## 7.1. Product Truth là gì

Product Truth là trạng thái thật của sản phẩm trong chuỗi vận hành.

Product Truth trả lời:

- SKU nào?

- Công thức/version nào?

- Nguyên liệu lot nào?

- Mẻ nào?

- Batch nào?

- Đã QC chưa?

- Đã release chưa?

- Đã đóng gói chưa?

- Hộp/thùng đã in gì?

- QR/barcode map vào đâu?

- Đã nhập kho chưa?

- Inventory ledger ghi nhận chưa?

- Stock available bao nhiêu?

- Có bị hold không?

- Có bị recall không?

- Có bị sale lock không?

- Có đủ trace không?

- Có đủ điều kiện sellable không?

## 7.2. Product Truth Chain chuẩn

Chuỗi Product Truth chuẩn:

Source Zone -> Source Origin -> Raw Material Intake -> Incoming QC -> Raw Material Lot -> READY_FOR_PRODUCTION -> Production Order -> BOM Snapshot -> Material Issue -> Material Receipt -> Execution -> Batch -> Packaging Level 1 -> Packaging Level 2 Hộp/Thùng -> Print Payload -> QC sau sấy -> QC thành phẩm -> Batch Release -> Warehouse Receipt Confirmed -> Inventory Ledger -> Stock Available -> Trace Ready -> Recall Ready -> Sellable Gate

Không bước nào được tự ý bỏ qua.

## 7.3. Field Evidence là một phần của Product Truth

Với các hành động hiện trường, Product Truth không đủ nếu thiếu Field Evidence.

Ví dụ:

- Nguyên liệu nhập phải có phiếu/evidence nếu policy yêu cầu.

- QC phải có kết quả và người xác nhận.

- Sau sấy phải có % độ ẩm.

- Đóng gói phải có số lượng thực tế.

- In phải có payload/log.

- Nhập kho phải có confirm.

- Recall phải có evidence/recovery/disposition.

- Smoke phải có evidence packet.

Ảnh/video/scan/check/click-confirm từ Field App phải vào Operational Evidence Packet.

## 7.4. Product Truth Status tối thiểu

Status | Ý nghĩa

NOT_STARTED | Chưa phát sinh vận hành

SOURCE_READY | Nguồn đã sẵn sàng

RAW_LOT_READY | Lot nguyên liệu sẵn sàng

PRODUCTION_OPENED | Đã mở lệnh sản xuất

MATERIAL_ISSUED | Đã cấp nguyên liệu theo lot

EXECUTION_STARTED | Đã bắt đầu sản xuất

EXECUTION_COMPLETED | Đã hoàn tất sản xuất

BATCH_CREATED | Đã sinh batch

PACKAGING_L1_COMPLETED | Đã đóng gói cấp 1

PACKAGING_L2_COMPLETED | Đã đóng gói cấp 2 hộp/thùng

PRINT_COMPLETED | Đã in

QC_PASS | Đạt QC

RELEASED | Batch đã release

WAREHOUSE_RECEIVED | Đã nhập kho

INVENTORY_AVAILABLE | Có tồn khả dụng

TRACE_READY | Sẵn sàng truy xuất

RECALL_READY | Sẵn sàng thu hồi

SELLABLE | Đủ điều kiện bán

ON_HOLD | Đang hold

SALE_LOCKED | Đang khóa bán

RECALLED | Thu hồi

STOP_SALE | Dừng bán

## 8. REGISTRY LOCKS

## 8.1. Warehouse Registry

Field | Value

warehouse_runtime_default | Kho Bến Tre

registry_required | CÓ

hardcode_allowed | KHÔNG

future_expansion | CÓ

## 8.2. Field App / Device Registry

Field | Value

internal_ops_app_required | CÓ

app_type_initial | Mobile PWA / Internal App

offline_queue_required | CÓ

device_registry_required | CÓ

mobile_event_audit_required | CÓ

hardcode_device_allowed | KHÔNG

## 8.3. Printer / Template Registry

Field | Value

printer_real_device_status | waiting_DEVICE

template_real_status | waiting_TEMPLATE

integration_port | RESERVED

hardcode_allowed | KHÔNG

## 8.4. GTIN/GS1 Registry

Field | Value

retail_box_gtin | RESERVED

carton_gtin | RESERVED

owner_registry | Trade Item Registry

hardcode_allowed | KHÔNG

## 8.5. MISA Mapping Registry

Field | Value

warehouse_mapping | RESERVED

material_mapping | RESERVED

sku_mapping | RESERVED

account_mapping | RESERVED

cost_center_mapping | RESERVED

integration_status | RESERVED

hardcode_allowed | KHÔNG

## 8.6. QC Moisture Threshold Registry

Field | Value

moisture_pass | < 2%

moisture_review_hold | 2%–4%

moisture_fail | > 4%

owner | QC Owner

hardcode_allowed | KHÔNG

## 8.7. Shelf-life Registry

Field | Value

default_shelf_life | 12 tháng

by_sku_allowed | CÓ

by_group_allowed | CÓ

hardcode_allowed | KHÔNG

## 8.8. Supplier Portal Registry

Field | Value

Sâm source | Company-owned growing source

supplier_portal | RESERVED

supplier_pass | CONDITIONAL

evidence_upload | REQUIRED WHEN ACTIVE

verification_required | CÓ

## 8.9. Personnel / Role / Permission Registry

Field | Value

real_personnel_data | waiting_FACTORY_OPERATION

role_registry | REQUIRED

permission_registry | REQUIRED

hardcode_person_name | KHÔNG

## 9. NO-HARDCODE CONTROL

## 9.1. Không hardcode runtime data

Không được hardcode:

- Kho Bến Tre.

- Location thật.

- Máy in thật.

- Template in.

- Device.

- App version.

- GTIN/GS1.

- Barcode.

- QR.

- MISA mapping.

- Tài khoản kế toán.

- Mã chi phí.

- QC threshold.

- HSD.

- Supplier pass.

- Nhân sự thật.

- Role/permission.

- Evidence path.

- Smoke status.

- Product sellable.

- Recall status.

- Sale lock status.

## 9.2. Hardcode là STOP_POINTS

PACK-01 BLOCK nếu phát hiện:

- Kho Bến Tre hardcode trong service/UI.

- GTIN hardcode trong template.

- HSD hardcode không qua shelf-life registry.

- Độ ẩm hardcode không qua QC config.

- MISA account hardcode trong module.

- Máy in hardcode trong worker.

- Nhân sự hardcode trong flow.

- App cho bypass backend validation.

- App sync không có idempotency.

- Evidence lưu không gắn object.

## 10. PHẦN 1/4 DONE GATE

## PHẦN 1/4 được xem là đạt governance scope khi đã khóa đủ:

- PACK-01 là Operational Core Pack, không phải code.

- Field Operations Internal App / Mobile PWA là P0 boundary.

- Admin Web và Field App tách vai trò.

- Field App là capture channel, không phải Owner Core.

- Backend/domain service mới validate và mutate state.

- Offline queue là bắt buộc.

- Device identity là bắt buộc.

- Media evidence không được mồ côi.

- Scan không đồng nghĩa confirm.

- Click-confirm high-risk phải có permission/idempotency/audit.

- Check-in/check-out là action thật, không phải ghi chú.

- Product Truth Model đã rõ.

- Field Evidence là một phần của Product Truth.

- Owner Boundary đã rõ.

- Không đi tắt từ production sang sellable.

- Inventory chỉ tăng sau warehouse receipt confirmed.

- QC_PASS không đồng nghĩa RELEASED.

- In cấp 2 áp dụng cho hộp và thùng.

- Hệ thống sinh dữ liệu in, máy in chỉ nhận payload.

- Kho Bến Tre được ghi nhận là kho thật ban đầu nhưng không hardcode.

- Máy in/template chừa ngõ kết nối.

- GTIN/GS1 hộp/thùng chừa ngõ tích hợp.

- MISA mapping chừa ngõ kết nối.

- QC độ ẩm: <2% PASS, 2%–4% HOLD/REVIEW, >4% FAIL.

- HSD mặc định 12 tháng.

- Vùng trồng Sâm là của công ty.

- Supplier Portal/evidence chừa ngõ khi nhà cung cấp đủ điều kiện.

- Evidence Pack và Smoke Log dùng phương án tinh gọn.

- No-hardcode rule đã khóa.

## 11. PHẦN 1/4 FINAL CONCLUSION

## PHẦN 1/4 của PACK-01 khóa nền vận hành lõi của Ginsengfood theo hướng thực chiến hiện trường.

PACK-01 không được hiểu là tài liệu chỉ cho Admin Web. Mọi hành động sản xuất, QC, đóng gói, in, nhập kho, truy vết và thu hồi phải có Field Operations Internal App / Mobile PWA hoặc giao diện hiện trường tương đương để capture đúng ảnh/video/scan/check-in/click-confirm/evidence.

Chuỗi nguyên tắc bắt buộc:

Operational Core tạo sự thật sản phẩm -> Field App capture evidence hiện trường -> Backend validate/mutate state -> Guard kiểm soát điều kiện -> Audit ghi nhận -> Evidence Packet gom bằng chứng -> Smoke chứng minh xuyên chuỗi -> Sellable/Recall/Sale Lock quyết định downstream

Không có đường tắt từ app hiện trường sang business truth.

Không có đường tắt từ scan sang confirm.

Không có đường tắt từ ảnh/video sang evidence PASS nếu không gắn object.

Không có đường tắt từ QC_PASS sang RELEASED.

Không có đường tắt từ in mã sang inventory.

Không có đường tắt từ MISA sang operational truth.

Không có đường tắt từ AI/CRM/ADS/MC AI/Gateway sang sellable.

PACK-01 PHẦN 1/4 là nền để viết tiếp:

## PHẦN 2/4 — PRODUCTION / FIELD OPS APP / WAREHOUSE / INVENTORY / TRACEABILITY / RECALL / SALE LOCK DOMAIN REGISTRY

## PHẦN 2/4 — PRODUCTION / FIELD OPS APP / WAREHOUSE / INVENTORY / TRACEABILITY / RECALL / SALE LOCK DOMAIN REGISTRY

## 12. DOMAIN REGISTRY PRINCIPLES

## 12.1. Mục tiêu Domain Registry

Domain Registry là bảng khóa nghiệp vụ chính thức của PACK-01.

Domain Registry dùng để xác định rõ:

- Domain nào là Owner Core.

- Domain nào là Consumer.

- Source-of-truth nằm ở đâu.

- Resolver nào bắt buộc.

- Guard nào bắt buộc.

- Trace ID nào bắt buộc.

- Evidence nào bắt buộc.

- Field Operations App tham gia ở điểm nào.

- Admin Web tham gia ở điểm nào.

- Thiết bị/printer tham gia ở điểm nào.

- bị chặn-if-missing rule là gì.

- Domain nào được phép mutate state.

- Domain nào chỉ được capture/query/display.

Không domain nào được tự mở rộng boundary ngoài phạm vi đã khóa trong Domain Registry.

## 12.2. Nguyên tắc một domain không làm thay domain khác

Mỗi domain chỉ làm đúng phần mình owner.

Các nguyên tắc bắt buộc:

- Field App không owner business truth.

- Admin Web không thay thao tác hiện trường bắt buộc.

- Printer không tạo inventory.

- Printing không pass QC.

- QC_PASS không tự release batch.

- Batch Release không tự nhập kho.

- Warehouse scan không tự tăng tồn.

- Inventory projection không thay Inventory Ledger.

- Traceability không dựng lại công thức.

- Recall không dựng chain riêng.

- Sale Lock không thay Recall Case.

- MISA không điều khiển nghiệp vụ nhà máy.

- AI/CRM/ADS/MC AI/Gateway không quyết định product truth.

## 12.3. Mọi domain P0 phải có Field Evidence khi phát sinh hiện trường

Các domain có hành động hiện trường bắt buộc phải gắn với Field Operations App / Mobile PWA hoặc giao diện hiện trường tương đương.

Các hành động sau không được chỉ ghi nhận sau trên Admin Web:

- Nhận nguyên liệu.

- QC nguyên liệu.

- Check-in/check-out.

- Cấp phát nguyên liệu.

- Nhận nguyên liệu tại xưởng.

- Theo dõi sơ chế.

- QC sau sấy.

- Đóng gói cấp 1.

- Đóng gói cấp 2 hộp/thùng.

- In/kiểm tra mã in.

- QC thành phẩm.

- Scan nhập kho.

- Confirm nhập kho.

- Scan truy xuất.

- Thu hồi/recovery/disposition.

Nếu action hiện trường không có actor, role, device, object binding, timestamp, audit và correlation ID thì chưa đủ điều kiện làm evidence vận hành.

## 12.4. Standard Domain Record

Mỗi domain trong PACK-01 phải có cấu trúc:

Field | Bắt buộc | Ý nghĩa

domain_code | Có | Mã domain

domain_name | Có | Tên domain

owner_core | Có | Owner chịu trách nhiệm chính

consumer_scope | Có | Domain tiêu thụ dữ liệu

source_of_truth | Có | Nguồn sự thật

field_app_required | Có/Không | Có bắt buộc Field App không

admin_web_required | Có/Không | Có Admin Web không

device_required | Có/Không | Có thiết bị/printer/scan không

required_resolver | Có | Resolver bắt buộc

required_guard | Có | Guard bắt buộc

required_trace_ids | Có | Trace ID bắt buộc

required_evidence | Có | Evidence bắt buộc

blocked_if_missing | Có | Điều kiện BLOCK

audit_required | Có | Có audit hay không

no_hardcode_required | Có | Có cấm hardcode không

## 12.5. Domain Default Decision

Mọi domain trong PACK-01 mặc định là:

BLOCK / waiting_EVIDENCE

Domain chỉ chuyển sang PASS khi có đủ:

- Owner rõ.

- Source-of-truth rõ.

- Resolver hoạt động.

- Guard hoạt động.

- Trace ID đầy đủ.

- Field Evidence nếu có thao tác hiện trường.

- Audit đầy đủ.

- Smoke pass.

- Evidence accepted.

- Không có STOP_POINTS P0.

## 13. DOMAIN REGISTRY OVERVIEW

Domain Code | Domain Name | Owner Core | Field App Required | Source-of-Truth

OP-SRC | Source Origin / Company Source / Supplier Source | Source Origin Owner | Có nếu capture evidence hiện trường | Source Origin Registry

OP-RMI | Raw Material Intake | Raw Material Owner | Có | Raw Material Receipt

OP-RMQ | Raw Material QC / Lot Readiness | QC / Raw Material Owner | Có | Raw Material Lot + QC Record

OP-FLD | Field Operations Internal App / Mobile PWA | Field App Owner | Có | Mobile Event Audit / Backend Operational Records

OP-RCP | Recipe / BOM Snapshot | Recipe Owner | Không bắt buộc | Recipe / BOM Version

OP-PRD | Production Order | Production Owner | Có cho hiện trường | Production Order

OP-MTI | Material Issue / Material Receipt | Warehouse / Production Owner | Có | Material Issue by Lot

OP-EXE | Production Execution | Production Owner | Có | Execution Record

OP-BAT | Batch | Production / QC Owner | Có theo execution/QC | Batch Record

OP-PKG | Packaging Level 1 / Level 2 Box-Carton | Packaging Owner | Có | Packaging Job

OP-PRN | Printing / Print Payload / Reprint | Printing Owner | Có | Print Payload / Print Log

OP-QCR | QC / Batch Release | QC Owner | Có cho QC, Admin/Owner cho release | QC Inspection + Batch Release

OP-WHR | Warehouse Receipt | Warehouse Owner | Có | Warehouse Receipt Confirmed

OP-INV | Inventory Ledger / Stock Balance | Inventory Owner | Không mutate trực tiếp từ app | Inventory Ledger

OP-TRC | Traceability | Traceability Owner | Có cho scan/check | Trace Chain / Genealogy

OP-RCL | Recall / Recovery / Disposition / CAPA | Recall Owner | Có | Recall Case + Impact Snapshot

OP-SLK | Sale Lock / Stop Sale | Recall / Risk Owner | Có cho cảnh báo/suppress, Owner command cho lock | Sale Lock Registry

OP-MIS | MISA Integration Boundary | MISA Integration Owner | Không | MISA Sync Event / Reconcile

OP-EVD | Evidence / Smoke | QA / Evidence Owner | Có | Evidence Packet / Smoke Log

## 14. OP-SRC — SOURCE ORIGIN / COMPANY SOURCE / SUPPLIER SOURCE DOMAIN

## 14.1. Domain Purpose

OP-SRC kiểm soát nguồn gốc nguyên liệu trước khi nguyên liệu đi vào chuỗi vận hành.

Domain này xác định:

- Source zone.

- Source origin.

- Nguồn công ty.

- Supplier source.

- Evidence nguồn.

- Verification nguồn.

- Supplier Portal nếu có.

Vùng trồng Sâm Savigin là nguồn nội bộ của công ty.

Supplier bên ngoài chỉ được nhập evidence khi đủ điều kiện và được cấp quyền.

## 14.2. Owner Core

- Source Origin Owner.

- Raw Material Owner.

- QC Owner.

- Supplier Collaboration Owner.

- Evidence Owner.

## 14.3. Source-of-Truth

- Source Zone Registry.

- Source Origin Registry.

- Company Source Registry.

- Supplier Registry.

- Source Evidence.

- Source Verification Record.

## 14.4. Field App Boundary

Field App được dùng để:

- Chụp ảnh vùng trồng/nguyên liệu khi cần.

- Quay video evidence nếu cần.

- Upload evidence hiện trường.

- Gắn evidence vào source_origin_id hoặc supplier_id.

- Sync offline evidence.

Field App không được tự verify source.

Supplier Portal không được tự làm source PASS.

Source Verification Owner mới được xác nhận.

## 14.5. Required Resolver

- SourceZoneResolver.

- SourceOriginResolver.

- CompanySourceResolver.

- SupplierResolver.

- SupplierEligibilityResolver.

- SourceEvidenceResolver.

- SourceVerificationResolver.

## 14.6. Required Guard

- SourceOriginRequiredGuard.

- CompanySourceBoundaryGuard.

- SupplierPortalEligibilityGuard.

- SourceEvidenceGuard.

- SourceVerificationGuard.

- NoFreeTextSourceGuard.

- NoSupplierAutoPassGuard.

## 14.7. Required Trace IDs

- source_zone_id.

- source_origin_id.

- company_source_id nếu là nguồn công ty.

- supplier_id nếu là supplier ngoài.

- source_evidence_id.

- source_verification_id.

- audit_id.

- correlation_id.

## 14.8. bị chặn If Missing

OP-SRC BLOCK nếu:

- Không có source zone.

- Không có source origin.

- Nguyên liệu không bind được nguồn.

- Supplier chưa đủ điều kiện nhưng được nhập evidence.

- Supplier evidence tự động PASS.

- Vùng trồng Sâm bị xử lý như supplier ngoài.

- Evidence không gắn object.

- Không có verification.

- Không có audit.

## 15. OP-RMI — RAW MATERIAL INTAKE DOMAIN

## 15.1. Domain Purpose

OP-RMI kiểm soát phiếu đánh giá và nhập kho nguyên liệu đầu vào.

Domain này ghi nhận:

- Nhà cung cấp hoặc nguồn công ty.

- Source zone.

- Source origin.

- Nguyên liệu canonical.

- Số lượng giao.

- Số lượng đạt.

- Số lượng không đạt.

- Đơn vị.

- Đơn giá nếu thuộc ngõ kế toán.

- Evidence ảnh/video/file.

- Kết luận nhập kho.

- Cơ sở sinh lot nguyên liệu.

## 15.2. Owner Core

- Raw Material Owner.

- Warehouse Owner.

- QC Owner.

- Accounting Boundary Owner nếu có giá nhập.

- Evidence Owner.

## 15.3. Source-of-Truth

- Raw Material Receipt.

- Raw Material Receipt Item.

- Raw Material Master.

- Supplier/Company Source Record.

- Intake Evidence.

- Intake Accounting Data nếu có.

## 15.4. Field App Boundary

Field App bắt buộc hỗ trợ:

- Chọn supplier/source từ registry.

- Chọn nguyên liệu từ master.

- Nhập số lượng giao.

- Ghi tình trạng ban đầu.

- Chụp ảnh/video nguyên liệu.

- Upload phiếu giao hàng/hóa đơn nếu có.

- Ghi kết quả đánh giá.

- Ghi lý do không đạt.

- Click xác nhận theo role.

- Sync offline nếu mất mạng.

Field App không được cho nhập tay tên nguyên liệu thay cho master.

## 15.5. Required Resolver

- RawMaterialResolver.

- SupplierResolver.

- CompanySourceResolver.

- SourceOriginResolver.

- IntakeReceiptResolver.

- IntakeEvidenceResolver.

- IntakeAccountingResolver.

## 15.6. Required Guard

- RawMaterialMasterGuard.

- NoFreeTextMaterialGuard.

- IntakeQuantityGuard.

- SourceBindingGuard.

- SupplierStatusGuard.

- FieldEvidenceRequiredGuard.

- IntakeAccountingBoundaryGuard.

- IdempotencyGuard.

## 15.7. Required Trace IDs

- raw_material_receipt_id.

- raw_material_receipt_item_id.

- raw_material_id.

- supplier_id hoặc company_source_id.

- source_zone_id.

- source_origin_id.

- evidence_item_id.

- device_id nếu từ Field App.

- actor_id.

- audit_id.

- correlation_id.

## 15.8. bị chặn If Missing

OP-RMI BLOCK nếu:

- Nguyên liệu không chọn từ master.

- Tên nguyên liệu nhập tay tự do.

- Thiếu nguồn.

- Thiếu supplier/company source hợp lệ.

- Số lượng giao không hợp lệ.

- Không tách số lượng đạt / không đạt.

- Media evidence mồ côi.

- Confirm intake không có role/device/audit.

- Không sinh được lot.

- Không có audit.

## 16. OP-RMQ — RAW MATERIAL QC / LOT READINESS DOMAIN

## 16.1. Domain Purpose

OP-RMQ kiểm soát QC nguyên liệu và trạng thái sẵn sàng đưa vào sản xuất.

Lot nguyên liệu chỉ được issue cho sản xuất khi:

- Có raw material lot.

- Có QC record.

- QC_PASS.

- READY_FOR_PRODUCTION.

- Không hold.

- Không reject.

- Không expired.

- Không bị chặn.

- Có số lượng khả dụng.

- Trace được về nguồn.

## 16.2. Owner Core

- QC Owner.

- Raw Material Owner.

- Warehouse Owner.

- Production Owner với vai trò Consumer.

## 16.3. Source-of-Truth

- Raw Material Lot.

- Raw Material QC Record.

- Lot Readiness Status.

- Lot Hold/Reject Status.

- Lot Evidence.

## 16.4. Field App Boundary

Field App bắt buộc hỗ trợ:

- Xem lot chờ QC.

- Ghi kết quả QC.

- Chụp ảnh/video defect.

- Upload file kiểm tra nếu có.

- Accept / Hold / Reject theo permission.

- Ghi reason khi hold/reject.

- Gắn evidence vào lot/QC.

- Sync offline theo policy.

Field App không được tự chuyển lot READY_FOR_PRODUCTION nếu backend guard không PASS.

## 16.5. Required Resolver

- RawMaterialLotResolver.

- RawMaterialQCResolver.

- LotReadinessResolver.

- LotAvailabilityResolver.

- LotHoldResolver.

- LotExpiryResolver.

- LotEvidenceResolver.

## 16.6. Required Guard

- LotQCGuard.

- ReadyForProductionGuard.

- LotHoldGuard.

- LotRejectGuard.

- LotExpiryGuard.

- LotQuantityGuard.

- MaterialIssueEligibilityGuard.

- FieldQCPermissionGuard.

## 16.7. Required Trace IDs

- raw_material_lot_id.

- raw_material_qc_id.

- lot_readiness_event_id.

- evidence_item_id.

- device_id nếu từ Field App.

- actor_id.

- audit_id.

- correlation_id.

## 16.8. bị chặn If Missing

OP-RMQ BLOCK nếu:

- Lot không có QC record.

- QC chưa PASS.

- Lot chưa READY_FOR_PRODUCTION.

- Lot bị hold.

- Lot bị reject.

- Lot expired.

- Lot không trace được nguồn.

- Lot không có số lượng khả dụng.

- Field App accept/hold/reject không có permission.

- Production vẫn issue được lot chưa đủ điều kiện.

## 17. OP-FLD — FIELD OPERATIONS INTERNAL APP / MOBILE PWA DOMAIN

## 17.1. Domain Purpose

OP-FLD kiểm soát kênh thao tác hiện trường.

Domain này đảm bảo mọi hành động hiện trường được capture đúng:

- Actor.

- Role.

- Device.

- App version.

- Object binding.

- Timestamp.

- Evidence.

- Offline event.

- Idempotency.

- Audit.

- Correlation ID.

OP-FLD không owner business truth.

OP-FLD chỉ là capture/sync/action channel.

## 17.2. Owner Core

- Field App Owner.

- Device Owner.

- Security/Permission Owner.

- Operational Owner.

- Evidence Owner.

- QA Owner.

## 17.3. Source-of-Truth

- Mobile Event Queue.

- Mobile Event Audit.

- Device Registry.

- Device Auth Session.

- Media Evidence Record.

- Backend Operational Records.

- Idempotency Registry.

- Audit Log.

## 17.4. Required Resolver

- DeviceResolver.

- DeviceSessionResolver.

- ActorResolver.

- RolePermissionResolver.

- ObjectBindingResolver.

- OfflineEventResolver.

- IdempotencyResolver.

- MediaEvidenceResolver.

- FieldActionResolver.

- AppVersionResolver.

## 17.5. Required Guard

- RegisteredDeviceGuard.

- DeviceSessionGuard.

- AppVersionGuard.

- ActorRoleGuard.

- PermissionGuard.

- ObjectBindingGuard.

- IdempotencyGuard.

- OfflineSyncGuard.

- NoOrphanMediaGuard.

- NoBypassBackendGuard.

- HighRiskActionGuard.

## 17.6. Required Trace IDs

- mobile_event_id.

- client_event_id.

- device_id.

- device_session_id.

- actor_id.

- role_id.

- object_id.

- evidence_item_id nếu có.

- idempotency_key.

- audit_id.

- correlation_id.

## 17.7. Required Field App Actions

Field App phải hỗ trợ nhóm action:

- Capture image.

- Capture video.

- Upload file.

- Scan QR/barcode.

- Check-in.

- Check-out.

- Submit field form.

- Click-confirm.

- Submit QC result.

- Submit print error.

- Submit reprint request.

- Submit warehouse scan.

- Submit recall evidence.

- Offline sync.

- Retry failed sync.

## 17.8. bị chặn If Missing

OP-FLD BLOCK nếu:

- Không có device identity.

- Device chưa đăng ký.

- App version không hợp lệ.

- Không có actor/role.

- Không có permission cho high-risk action.

- Media không gắn object.

- Offline retry sinh duplicate.

- App mutate database trực tiếp.

- App bypass backend validation.

- Scan tự đổi state.

- Click-confirm không audit.

- Không có mobile event audit.

## 18. OP-RCP — RECIPE / BOM SNAPSHOT DOMAIN

## 18.1. Domain Purpose

OP-RCP kiểm soát công thức/version/BOM được dùng trong sản xuất.

Khi chọn SKU, hệ thống phải tự hiện:

- Tên sản phẩm.

- Nhóm sản phẩm.

- Phân loại vegan / mặn.

- Mã công thức.

- Version công thức.

- Danh sách dược liệu.

- Danh sách nguyên liệu.

- Hàm lượng.

- Đơn vị.

- Ghi chú vận hành nếu có.

Người vận hành không được chọn lại nguyên liệu trong sản xuất thường.

## 18.2. Owner Core

- Recipe Owner.

- Product Master Owner.

- Operational Config Owner.

- Production Owner với vai trò Consumer.

## 18.3. Source-of-Truth

- SKU Canonical Registry.

- Recipe / BOM Version.

- Formula Status Registry.

- Operational Config Map.

- Formula Approval Record.

## 18.4. Field App Boundary

Field App chỉ được hiển thị BOM snapshot và ghi nhận thao tác hiện trường.

Field App không được:

- Sửa công thức.

- Thêm nguyên liệu.

- Xóa nguyên liệu.

- Đổi hàm lượng chuẩn.

- Đổi version.

- Dùng nguyên liệu ngoài BOM.

## 18.5. Required Resolver

- SKUCanonicalResolver.

- ActiveFormulaResolver.

- BOMResolver.

- FormulaVersionResolver.

- OperationalConfigResolver.

- FormulaApprovalResolver.

## 18.6. Required Guard

- FormulaActiveGuard.

- BOMCompletenessGuard.

- NoManualMaterialSelectionGuard.

- FormulaVersionGuard.

- OperationalConfigGuard.

- FormulaChangeApprovalGuard.

## 18.7. Required Trace IDs

- sku_id.

- formula_id.

- formula_version_id.

- bom_snapshot_id.

- operational_config_id.

- formula_approval_id nếu có.

- audit_id.

- correlation_id.

## 18.8. bị chặn If Missing

OP-RCP BLOCK nếu:

- SKU không có công thức.

- Công thức thiếu version.

- Công thức thiếu nhóm sản phẩm.

- Công thức thiếu phân loại vegan/mặn.

- BOM thiếu nguyên liệu chi tiết.

- Có dòng gom nhóm mơ hồ ở downstream.

- Field App hoặc Admin Web cho chọn tay nguyên liệu ở sản xuất thường.

- Thay đổi công thức nhưng không tạo version mới.

- Lệnh sản xuất không snapshot BOM.

- Không audit được version đang dùng.

## 19. OP-PRD — PRODUCTION ORDER DOMAIN

## 19.1. Domain Purpose

OP-PRD kiểm soát lệnh sản xuất và hồ sơ sản xuất gốc.

Production Order là điểm sinh dữ liệu gốc cho toàn bộ phiếu downstream.

Production Order phải tự kéo:

- SKU.

- Tên sản phẩm.

- Nhóm sản phẩm.

- Phân loại vegan / mặn.

- Mã công thức.

- Version.

- BOM snapshot.

- Số lượng kế hoạch.

- Ca sản xuất.

- Bộ phận sản xuất.

- Quản lý nhà máy.

## 19.2. Owner Core

- Production Owner.

- Recipe Owner.

- Factory Manager.

- QA/QC Owner với vai trò downstream gate.

- Warehouse Owner với vai trò material issue consumer.

## 19.3. Source-of-Truth

- Production Order.

- Production Profile.

- BOM Snapshot.

- Production Order Item.

- Production Order Status.

- Production Sign-off.

## 19.4. Field App Boundary

Field App được dùng để:

- Xem lệnh sản xuất.

- Xem BOM snapshot.

- Xem công đoạn.

- Check-in/check-out.

- Start/Pause/Resume/Complete execution theo permission.

- Ghi process event.

- Capture evidence.

Field App không được tạo/sửa BOM snapshot.

## 19.5. Required Resolver

- ProductionOrderResolver.

- ProductionProfileResolver.

- BOMSnapshotResolver.

- ProductionOrderStatusResolver.

- ProductionRoleSignoffResolver.

- ProductionPlanningSignalResolver nếu có.

## 19.6. Required Guard

- ProductionOrderCreationGuard.

- BOMSnapshotGuard.

- ProductionOpenGuard.

- NoManualMaterialSelectionGuard.

- RoleSignoffGuard.

- ProductionPlanningOwnerReviewGuard.

- FieldCommandPermissionGuard.

## 19.7. Required Trace IDs

- production_order_id.

- production_profile_id.

- bom_snapshot_id.

- sku_id.

- formula_version_id.

- production_signoff_id.

- device_id nếu action từ app.

- audit_id.

- correlation_id.

## 19.8. bị chặn If Missing

OP-PRD BLOCK nếu:

- Không có production profile.

- Không có BOM snapshot.

- Không có formula version.

- Lệnh sản xuất không tự hiện công thức.

- Người dùng chọn tay nguyên liệu.

- Không có role ký xác nhận.

- Sản xuất bắt đầu khi chưa có nguyên liệu được chấp thuận.

- Production Planning Signal tự thành Production Order mà không qua Owner Review.

- Field App start execution không có permission/audit.

- Không audit được lệnh sản xuất.

## 20. OP-MTI — MATERIAL ISSUE / MATERIAL RECEIPT DOMAIN

## 20.1. Domain Purpose

OP-MTI kiểm soát cấp phát và nhận nguyên liệu cho sản xuất.

Material Issue là cầu nối giữa:

- BOM snapshot.

- Raw material lot.

- Production order.

- Phiếu đề nghị cấp nguyên liệu.

- Phiếu chấp thuận nguyên liệu.

- Xưởng nhận nguyên liệu.

- Batch genealogy.

- Phiếu kế toán xuất nguyên liệu nếu thuộc scope.

## 20.2. Owner Core

- Warehouse Owner.

- Production Owner.

- QC Owner.

- Accounting Boundary Owner nếu có phiếu kế toán xuất nguyên liệu.

## 20.3. Source-of-Truth

- Material Issue Record.

- Material Receipt Confirmation.

- Production Order Item / BOM Snapshot.

- Raw Material Lot.

- Material Approval Record.

- Accounting Issue Record nếu có.

## 20.4. Field App Boundary

Field App bắt buộc hỗ trợ:

- Xem danh sách nguyên liệu tự hiện từ BOM.

- Scan/check lot nguyên liệu.

- Xác nhận nhận nguyên liệu tại xưởng.

- Ghi chênh lệch nếu có.

- Chụp ảnh/video nếu có sự cố.

- Sync offline theo policy.

Field App không được:

- Issue nguyên liệu ngoài BOM.

- Chọn tay nguyên liệu.

- Dùng lot chưa QC_PASS + READY_FOR_PRODUCTION.

- Xác nhận nhận nguyên liệu nếu backend guard BLOCK.

## 20.5. Required Resolver

- MaterialIssueResolver.

- MaterialReceiptResolver.

- ProductionOrderItemResolver.

- RawMaterialLotResolver.

- LotReadinessResolver.

- MaterialApprovalResolver.

- MaterialAccountingIssueResolver.

## 20.6. Required Guard

- BOMLineGuard.

- LotReadyForProductionGuard.

- LotQuantityGuard.

- NoOutsideBOMIssueGuard.

- MaterialApprovalGuard.

- AccountingBoundaryGuard.

- IdempotencyGuard.

- FieldReceiptConfirmationGuard.

## 20.7. Required Trace IDs

- material_issue_id.

- material_receipt_id.

- production_order_id.

- production_order_item_id.

- raw_material_lot_id.

- material_approval_id.

- accounting_issue_id nếu có.

- device_id nếu từ Field App.

- audit_id.

- correlation_id.

## 20.8. bị chặn If Missing

OP-MTI BLOCK nếu:

- Nguyên liệu không thuộc BOM snapshot.

- Lot chưa QC_PASS.

- Lot chưa READY_FOR_PRODUCTION.

- Lot bị hold/reject/expired.

- Không đủ số lượng khả dụng.

- Không có phiếu chấp thuận nguyên liệu.

- Issue không lot-based.

- Field App xác nhận nhận nguyên liệu không có actor/device/audit.

- Phiếu kế toán xuất nguyên liệu bị gộp sai với phiếu xưởng.

- Batch sau này không truy ngược được raw material lot.

## 21. OP-EXE — PRODUCTION EXECUTION DOMAIN

## 21.1. Domain Purpose

OP-EXE kiểm soát thực thi sản xuất.

Domain này ghi nhận:

- Start.

- Pause.

- Resume.

- Complete.

- Process event.

- Actual input.

- Actual output.

- Loss.

- Incident.

- Handover.

- Check-in/check-out nhân sự.

Execution không được chỉ lưu trạng thái cuối.

Execution phải có log sự kiện phục vụ audit, costing, trace và recall.

## 21.2. Owner Core

- Production Owner.

- Factory Manager.

- Shift Leader / Process Owner.

- QA/QC Owner với vai trò downstream.

- Costing Owner với vai trò consumer.

## 21.3. Source-of-Truth

- Production Execution Record.

- Production Process Event.

- Personnel Check-in / Check-out Record.

- Process Evidence.

- Actual Output / Loss Record.

## 21.4. Field App Boundary

Field App bắt buộc hỗ trợ:

- Start execution.

- Pause execution.

- Resume execution.

- Complete execution.

- Ghi process event.

- Ghi output.

- Ghi loss.

- Ghi incident.

- Chụp ảnh/video nếu có lỗi.

- Sync offline theo policy.

Field App không được tự tạo batch nếu backend guard không cho phép.

## 21.5. Required Resolver

- ProductionExecutionResolver.

- ProcessEventResolver.

- ActualOutputResolver.

- LossResolver.

- IncidentResolver.

- PersonnelCheckInResolver.

## 21.6. Required Guard

- ExecutionStartGuard.

- ExecutionStateGuard.

- ProcessEventAuditGuard.

- OutputQuantityGuard.

- LossReasonGuard.

- PersonnelSignoffGuard.

- FieldExecutionPermissionGuard.

## 21.7. Required Trace IDs

- production_execution_id.

- production_order_id.

- work_order_id nếu có.

- process_event_id.

- personnel_check_event_id nếu có.

- output_record_id.

- device_id nếu từ Field App.

- audit_id.

- correlation_id.

## 21.8. bị chặn If Missing

OP-EXE BLOCK nếu:

- Execution không gắn production order.

- Không ghi process event.

- Pause/resume/loss/incident không có log.

- Actual output không rõ.

- Loss không có reason.

- Produced/accepted/loss bị trộn.

- Check-in/check-out là ghi chú text.

- Field App complete execution không có permission/audit.

- Batch phải tự đoán dữ liệu từ execution.

## 22. OP-BAT — BATCH DOMAIN

## 22.1. Domain Purpose

OP-BAT kiểm soát batch thành phẩm sau sản xuất và trước kho.

Batch là trung tâm cho:

- Packaging.

- Printing.

- QC thành phẩm.

- Release.

- Warehouse receipt.

- Inventory.

- Traceability.

- Recall.

- Public trace.

- MISA checkpoint nếu có.

## 22.2. Owner Core

- Production Owner.

- QC Owner.

- Batch Release Owner.

- Traceability Owner với vai trò consumer.

- Warehouse Owner với vai trò consumer.

## 22.3. Source-of-Truth

- Batch Record.

- Batch Material Usage.

- Batch State Transition Log.

- Batch Genealogy Link.

- Batch Quantity Summary như projection.

Batch Quantity Summary không thay source-of-truth nếu conflict với batch/execution.

## 22.4. Field App Boundary

Field App có thể capture:

- Output thực tế.

- Loss.

- Incident.

- Evidence công đoạn.

- Check trạng thái batch.

Field App không được:

- Sửa batch trực tiếp.

- Gán batch vào lot thủ công trái issue log.

- Chuyển batch QC_PASS.

- Release batch.

- Nhập kho batch.

## 22.5. Required Resolver

- BatchResolver.

- BatchGenealogyResolver.

- BatchMaterialUsageResolver.

- BatchQuantityResolver.

- BatchStatusResolver.

- BatchExpiryResolver.

- BatchHoldResolver.

## 22.6. Required Guard

- BatchCreationGuard.

- BatchCodeUniqueGuard.

- BatchGenealogyGuard.

- BatchQuantityGuard.

- BatchQCStatusGuard.

- BatchReleaseStatusGuard.

- BatchHoldGuard.

- FieldBatchMutationGuard.

## 22.7. Required Trace IDs

- batch_id.

- batch_code.

- production_order_id.

- production_execution_id.

- raw_material_lot_id list.

- batch_material_usage_id.

- formula_version_id.

- audit_id.

- correlation_id.

## 22.8. bị chặn If Missing

OP-BAT BLOCK nếu:

- Batch không gắn production order.

- Batch không gắn execution.

- Batch không truy được raw material lot.

- Batch không có formula version.

- Batch không có MFG/HSD.

- Batch mới tạo nhưng bị coi như RELEASED.

- QC_PASS bị coi như RELEASED.

- Batch bị hold nhưng vẫn downstream.

- Không có state transition log.

- Field App sửa batch state trực tiếp.

## 23. OP-PKG — PACKAGING LEVEL 1 / LEVEL 2 BOX-CARTON DOMAIN

## 23.1. Domain Purpose

OP-PKG kiểm soát đóng gói cấp 1 và cấp 2.

Packaging Level 1:

- Gói / lọ / hũ.

- Chỉ in NSX/HSD.

- Không tạo inventory.

- Không release batch.

- Không phải trace chính nếu policy không xác định là trade item.

Packaging Level 2:

- Hộp.

- Thùng.

- Có lô/batch.

- Có NSX.

- Có HSD.

- Có mã vạch.

- Có QR.

- Là cấp truy vết chính.

- Có thể là trade item thương mại.

- Phải map GTIN/GS1 khi registry sẵn sàng.

## 23.2. Owner Core

- Packaging Owner.

- Production Owner.

- Printing Owner.

- Traceability Owner.

- Trade Item Owner nếu cấp đóng gói là trade item.

- QC Owner với vai trò downstream.

## 23.3. Source-of-Truth

- Packaging Spec.

- Packaging Job.

- Packaging Job Unit.

- Batch.

- Trade Item Config.

- GTIN/GS1 Registry nếu có.

- Packaging Evidence.

## 23.4. Field App Boundary

Field App bắt buộc hỗ trợ:

- Xem packaging job.

- Xem batch.

- Phân biệt cấp 1/cấp 2.

- Phân biệt hộp/thùng.

- Ghi số lượng đầu vào.

- Ghi số lượng đạt.

- Ghi số lượng không đạt.

- Ghi lý do không đạt.

- Chụp ảnh/video.

- Complete packaging theo permission.

- Gửi print request nếu thuộc cấp in.

- Sync offline theo policy.

Field App không được tạo inventory khi complete packaging.

## 23.5. Required Resolver

- PackagingSpecResolver.

- PackagingJobResolver.

- PackagingLevelResolver.

- BatchResolver.

- TradeItemResolver.

- GTINResolver.

- PackagingEvidenceResolver.

## 23.6. Required Guard

- BatchRequiredForPackagingGuard.

- PackagingLevelGuard.

- PackagingSpecGuard.

- Level1PrintBoundaryGuard.

- Level2BoxCartonGuard.

- TradeItemGTINGuard.

- NoInventoryFromPackagingGuard.

- PackagingEvidenceGuard.

- FieldPackagingPermissionGuard.

## 23.7. Required Trace IDs

- packaging_job_id.

- packaging_level.

- batch_id.

- trade_item_id nếu có.

- gtin_id nếu có.

- evidence_item_id.

- device_id nếu từ Field App.

- audit_id.

- correlation_id.

## 23.8. bị chặn If Missing

OP-PKG BLOCK nếu:

- Packaging job không gắn batch.

- Packaging level không rõ.

- Hộp/thùng không phân biệt.

- Cấp 1 in dữ liệu vượt boundary.

- Cấp 2 không in đủ lô/NSX/HSD/barcode/QR.

- GTIN/GS1 bị nhập tay.

- QR bị dùng thay barcode thương mại.

- Packaging complete làm tăng inventory.

- Packaging complete làm pass QC.

- Packaging complete làm release batch.

- Field App cho nhập tay lô/barcode/QR.

## 24. OP-PRN — PRINTING / PRINT PAYLOAD / REPRINT DOMAIN

## 24.1. Domain Purpose

OP-PRN kiểm soát dữ liệu in, máy in, template, print payload, print log, print error và reprint.

Printing là technical boundary.

Hệ thống sinh payload.

Máy in chỉ nhận payload và trả trạng thái.

## 24.2. Owner Core

- Printing Owner.

- Packaging Owner.

- Device/Printer Owner.

- Traceability Owner.

- Trade Item Owner nếu liên quan GTIN/GS1.

- QA/QC Owner với vai trò kiểm chứng.

## 24.3. Source-of-Truth

- Print Template Registry.

- Print Device Registry.

- Print Job.

- Print Payload.

- Print Execution Log.

- Print Error Log.

- Reprint Log.

- QR Registry.

- GTIN/GS1 Registry nếu có.

## 24.4. Field App Boundary

Field App được dùng để:

- Chọn packaging job.

- Chọn máy in từ registry.

- Start print.

- Scan kiểm tra mã in.

- Ghi print error.

- Chụp ảnh/video lỗi in.

- Gửi reprint request có reason.

- Theo dõi trạng thái in.

Field App không được:

- Nhập tay batch/lô.

- Nhập tay NSX/HSD.

- Nhập tay barcode.

- Nhập tay QR.

- Tự tạo print payload.

- Tự xác nhận print PASS nếu backend/printer log không có.

## 24.5. Required Resolver

- PrintTemplateResolver.

- PrintDeviceResolver.

- PrintPayloadResolver.

- BatchPrintContextResolver.

- QRTokenResolver.

- GTINResolver.

- ReprintPolicyResolver.

- PrintLogResolver.

## 24.6. Required Guard

- PrintTemplateGuard.

- PrintDeviceStatusGuard.

- SystemGeneratedPayloadGuard.

- NoManualPrintDataGuard.

- ReprintControlGuard.

- PrinterNoBusinessTruthGuard.

- QRBatchMappingGuard.

- GTINTradeItemGuard.

- FieldPrintActionGuard.

## 24.7. Required Trace IDs

- print_job_id.

- print_payload_id.

- batch_id.

- packaging_job_id.

- qr_token_id.

- gtin_id nếu có.

- reprint_event_id nếu có.

- print_error_id nếu có.

- device_id.

- audit_id.

- correlation_id.

## 24.8. bị chặn If Missing

OP-PRN BLOCK nếu:

- Máy in tự sinh batch/lô/NSX/HSD/barcode/QR.

- Field App cho nhập tay mã in cấp 2.

- Không có print payload.

- Không có print log.

- Không có reprint log.

- Reprint không reason/approval.

- QR không map batch thật.

- GTIN không lấy từ trade item config.

- Cùng trade item có hai barcode thương mại.

- Printing làm thay inventory/QC/release.

## 25. OP-QCR — QC / BATCH RELEASE DOMAIN

## 25.1. Domain Purpose

OP-QCR kiểm soát QC sau sấy, QC thành phẩm và hành động release batch.

QC_PASS không đồng nghĩa RELEASED.

Release là action riêng.

## 25.2. Owner Core

- QC Owner.

- Batch Release Owner.

- Factory Manager.

- Warehouse Owner với vai trò downstream consumer.

- Recall Owner nếu QC phát sinh incident.

## 25.3. Source-of-Truth

- QC Checklist Template.

- QC Inspection.

- QC Inspection Item.

- QC Scan Verification.

- QC Media.

- Batch Disposition.

- Batch Release Record.

- Batch State Transition Log.

## 25.4. Field App Boundary

Field App bắt buộc hỗ trợ:

- Ghi số lượng kiểm tra.

- Ghi % độ ẩm.

- Ghi cảm quan.

- Chụp ảnh/video.

- Upload file đo nếu có.

- Submit QC result.

- Accept/Hold/Reject theo permission.

- Ghi reason khi hold/reject.

- Scan verification nếu cần.

Field App không được tự release batch.

Batch release phải là action riêng có owner/permission/audit.

## 25.5. Required Resolver

- QCChecklistResolver.

- QCInspectionResolver.

- MoistureThresholdResolver.

- BatchDispositionResolver.

- BatchReleaseResolver.

- BatchHoldResolver.

- QCIncidentResolver.

## 25.6. Required Guard

- QCRequiredItemGuard.

- MoistureThresholdGuard.

- QCResultGuard.

- BatchReleaseEligibilityGuard.

- QCNotAutoReleaseGuard.

- BatchRejectGuard.

- BatchHoldGuard.

- TransitionLogGuard.

- FieldQCPermissionGuard.

## 25.7. Required Trace IDs

- qc_inspection_id.

- qc_item_id.

- batch_id.

- disposition_id.

- batch_release_id.

- state_transition_id.

- moisture_measurement_id nếu có.

- evidence_item_id.

- device_id nếu từ Field App.

- audit_id.

- correlation_id.

## 25.8. bị chặn If Missing

OP-QCR BLOCK nếu:

- Không có QC inspection.

- Thiếu độ ẩm ở bước sau sấy.

- Độ ẩm >4% nhưng vẫn PASS.

- Độ ẩm 2%–4% nhưng tự động PASS.

- QC_PASS tự release.

- Rejected batch vẫn release được.

- Hold batch vẫn downstream.

- Release không có record riêng.

- Field App release batch trực tiếp.

- Không có transition log.

## 26. OP-WHR — WAREHOUSE RECEIPT DOMAIN

## 26.1. Domain Purpose

OP-WHR kiểm soát nhập kho thành phẩm.

Warehouse Receipt Confirmed là điểm kích hoạt tồn kho thành phẩm.

Scan inbound không đồng nghĩa confirm receipt.

Chỉ batch RELEASED mới được nhập kho.

## 26.2. Owner Core

- Warehouse Owner.

- Inventory Owner.

- QC Owner với vai trò xác nhận batch released.

- Operational Owner.

- MISA Integration Owner nếu sync nhập kho.

## 26.3. Source-of-Truth

- Warehouse Receipt.

- Warehouse Receipt Item.

- Warehouse Registry.

- Location Registry.

- Batch Release Record.

- Receipt Confirmation Record.

- Warehouse Evidence.

Kho thật ban đầu: Kho Bến Tre.

Kho này phải là registry value, không hardcode.

## 26.4. Field App Boundary

Field App bắt buộc hỗ trợ:

- Scan inbound.

- Xem batch RELEASED.

- Xem warehouse/location.

- Ghi số lượng nhận.

- Chụp ảnh/video nhập kho.

- Click confirm receipt theo permission.

- Sync offline nếu policy cho phép.

Field App không được:

- Tăng inventory khi chỉ scan.

- Confirm receipt nếu batch chưa RELEASED.

- Confirm receipt nếu không đủ permission.

- Confirm receipt nếu thiếu warehouse/location.

- Bypass backend guard.

## 26.5. Required Resolver

- WarehouseResolver.

- LocationResolver.

- BatchReleaseResolver.

- WarehouseReceiptResolver.

- ReceiptConfirmationResolver.

- WarehouseEvidenceResolver.

- MISAWarehouseMappingResolver nếu sync.

## 26.6. Required Guard

- ReleasedBatchOnlyReceiptGuard.

- ScanNotConfirmGuard.

- ReceiptConfirmationGuard.

- WarehouseLocationGuard.

- ReceiptQuantityGuard.

- ReceiptEvidenceGuard.

- WarehouseNoHardcodeGuard.

- FieldReceiptPermissionGuard.

- MISAMappingGuard nếu sync.

## 26.7. Required Trace IDs

- warehouse_receipt_id.

- warehouse_receipt_item_id.

- batch_id.

- batch_release_id.

- warehouse_id.

- warehouse_location_id.

- receipt_confirmation_id.

- device_id nếu từ Field App.

- misa_sync_event_id nếu có.

- audit_id.

- correlation_id.

## 26.8. bị chặn If Missing

OP-WHR BLOCK nếu:

- Batch chưa RELEASED.

- QC_PASS bị dùng thay release.

- Chỉ scan nhưng chưa confirm.

- Không có warehouse/location.

- Kho Bến Tre bị hardcode thay vì registry.

- Số lượng nhập không rõ.

- Không có receipt confirmation.

- Warehouse receipt làm sai inventory boundary.

- Field App confirm receipt không có permission/device/audit.

- Không có MISA mapping nếu cần sync chính thức.

## 27. OP-INV — INVENTORY LEDGER / STOCK BALANCE DOMAIN

## 27.1. Domain Purpose

OP-INV kiểm soát tồn kho và biến động tồn kho.

Inventory chỉ thay đổi qua:

- Warehouse Receipt Confirmed.

- Approved Adjustment.

- Allocation / Issue theo policy.

- Return / Recovery theo policy.

- Recall disposition theo policy.

Inventory Ledger là source-of-truth của biến động tồn kho.

## 27.2. Owner Core

- Inventory Owner.

- Warehouse Owner.

- Operational Owner.

- Recall Owner nếu có hold/recovery.

- Order/Allocation Consumer nếu có bán hàng.

## 27.3. Source-of-Truth

- Inventory Ledger.

- Inventory Lot Balance.

- Warehouse Receipt Confirmed.

- Allocation Record.

- Adjustment Approval.

- Hold/Sale Lock Registry nếu ảnh hưởng availability.

## 27.4. Field App Boundary

Field App có thể:

- Scan inbound.

- Capture stock evidence.

- Gửi yêu cầu adjustment nếu có policy.

- Capture recovery/return evidence.

Field App không được:

- Update inventory trực tiếp.

- Sửa ledger.

- Tự tạo stock available.

- Tự gỡ stock bị chặn.

- Tự bỏ recall hold.

- Tự xác định sellable.

## 27.5. Required Resolver

- InventoryLedgerResolver.

- InventoryBalanceResolver.

- StockAvailableResolver.

- StockReservedResolver.

- StockBlockedResolver.

- WarehouseLocationResolver.

- InventoryAdjustmentResolver.

- InventoryHoldResolver.

## 27.6. Required Guard

- LedgerAppendOnlyGuard.

- NoDirectInventoryUpdateGuard.

- NoNegativeInventoryGuard.

- StockAvailabilityGuard.

- StockReservationGuard.

- StockBlockedGuard.

- AdjustmentReasonGuard.

- InventoryProjectionGuard.

- FieldInventoryMutationGuard.

## 27.7. Required Trace IDs

- inventory_ledger_id.

- inventory_lot_balance_id.

- batch_id.

- warehouse_receipt_id.

- warehouse_id.

- warehouse_location_id.

- allocation_id nếu có.

- adjustment_id nếu có.

- audit_id.

- correlation_id.

## 27.8. bị chặn If Missing

OP-INV BLOCK nếu:

- Inventory tăng từ packaging.

- Inventory tăng từ QC.

- Inventory tăng từ printing.

- Inventory tăng từ scan inbound chưa confirm.

- Field App update inventory trực tiếp.

- Ledger sửa trực tiếp.

- Ledger không append-only.

- Stock âm.

- Stock available không khớp ledger/balance.

- Consumer tự suy luận tồn kho.

## 28. OP-TRC — TRACEABILITY DOMAIN

## 28.1. Domain Purpose

OP-TRC kiểm soát truy xuất nội bộ và public trace.

Traceability phải hỗ trợ:

- Backward trace.

- Forward trace.

- Internal trace.

- Public trace.

- Genealogy tree.

- Timeline.

- Batch-to-lot trace.

- QR-to-batch trace.

- Batch-to-exposure trace.

- Recall impact trace.

## 28.2. Owner Core

- Traceability Owner.

- Production Owner.

- Warehouse Owner.

- Inventory Owner.

- Recall Owner với vai trò Consumer.

- Public Trace Owner.

## 28.3. Source-of-Truth

- Trace Link.

- Batch Genealogy Link.

- QR Registry.

- Trace Search Index.

- Internal Trace View.

- Public Trace View.

- Shipment/Exposure Link nếu có.

- Trace Policy.

## 28.4. Field App Boundary

Field App phải hỗ trợ:

- Scan QR/barcode.

- Resolve trace nội bộ theo quyền.

- Capture ảnh/video trace evidence.

- Gắn trace_result_id.

- Gửi trace issue nếu phát hiện lỗi.

- Sync offline theo policy.

Field App không được:

- Tự sửa trace chain.

- Tự public trace.

- Tự gỡ trace gap.

- Tự map QR sang batch khác.

## 28.5. Required Resolver

- TraceSearchResolver.

- BatchGenealogyResolver.

- QRRegistryResolver.

- InternalTraceResolver.

- PublicTraceResolver.

- TracePolicyResolver.

- ExposureResolver.

- TraceGapResolver.

## 28.6. Required Guard

- TraceCompletenessGuard.

- QRBatchMappingGuard.

- PublicTraceWhitelistGuard.

- InternalDataLeakGuard.

- TraceGapGuard.

- RecallTraceReuseGuard.

- LotBatchBasedTraceGuard.

- FieldTracePermissionGuard.

## 28.7. Required Trace IDs

- trace_link_id.

- batch_genealogy_link_id.

- qr_token_id.

- batch_id.

- raw_material_lot_id.

- packaging_job_id.

- print_payload_id.

- warehouse_receipt_id.

- shipment_batch_link_id nếu có.

- customer_exposure_id nếu có.

- device_id nếu từ Field App.

- audit_id.

- correlation_id.

## 28.8. bị chặn If Missing

OP-TRC BLOCK nếu:

- Batch không truy ngược được raw material lot.

- QR không map batch thật.

- Cấp 2 hộp/thùng không gắn trace.

- Public trace lộ internal fields.

- Internal trace thiếu genealogy.

- Trace theo tên sản phẩm thay vì lot/batch.

- Recall phải dựng chain riêng.

- Field App tự sửa trace.

- Không audit được trace action.

## 29. OP-RCL — RECALL / RECOVERY / DISPOSITION / CAPA DOMAIN

## 29.1. Domain Purpose

OP-RCL kiểm soát thu hồi, impact, recovery, disposition, CAPA và close case.

Recall phải:

- Dùng traceability chain.

- Chặn flow thật.

- Nhìn thấy inventory impact.

- Nhìn thấy order/shipment impact nếu có.

- Nhìn thấy customer exposure impact nếu có.

- Recovery được.

- Disposition được.

- Có CAPA.

- Có timeline.

- Có evidence.

- Đóng case rõ, kể cả residual risk.

## 29.2. Owner Core

- Recall Owner.

- QC Owner.

- Risk Owner.

- Warehouse Owner.

- Customer Service Owner nếu có customer exposure.

- Legal/Compliance Owner nếu sự cố nghiêm trọng.

- Final Business Owner nếu cần.

## 29.3. Source-of-Truth

- Recall Case.

- Recall Case Batch.

- Hold Registry.

- Sale Lock Registry.

- Recall Inventory Impact.

- Recall Order Impact.

- Recall Customer Impact.

- Recall Contact Task.

- Recovery Item.

- Disposition Record.

- Recall Timeline.

- Root Cause / CAPA.

- Recall Impact Summary.

- Recovery Summary.

## 29.4. Field App Boundary

Field App phải hỗ trợ:

- Scan QR/batch khi thu hồi.

- Chụp ảnh/video sản phẩm thu hồi.

- Ghi recovery item.

- Ghi disposition evidence.

- Ghi contact/recovery result nếu thuộc scope.

- Gắn recall_case_id.

- Sync offline nếu thu hồi ngoài kho.

Field App không được:

- Tự close recall.

- Tự gỡ Sale Lock.

- Tự sửa impact.

- Tự xóa recovery record.

- Tự close with residual risk.

## 29.5. Required Resolver

- RecallCaseResolver.

- RecallImpactResolver.

- HoldResolver.

- SaleLockResolver.

- InventoryImpactResolver.

- OrderImpactResolver.

- CustomerExposureResolver.

- RecoveryResolver.

- DispositionResolver.

- CAPAResolver.

- ResidualRiskResolver.

## 29.6. Required Guard

- RecallTraceReuseGuard.

- RecallImpactCompletenessGuard.

- HoldActivationGuard.

- SaleLockActivationGuard.

- RecoveryGuard.

- DispositionGuard.

- CAPAGuard.

- ResidualRiskGuard.

- RecallCloseGuard.

- FieldRecallActionGuard.

## 29.7. Required Trace IDs

- recall_case_id.

- recall_case_batch_id.

- hold_id.

- sale_lock_id.

- inventory_impact_id.

- order_impact_id.

- customer_impact_id.

- recovery_item_id.

- disposition_id.

- capa_id.

- recall_timeline_id.

- device_id nếu từ Field App.

- audit_id.

- correlation_id.

## 29.8. bị chặn If Missing

OP-RCL BLOCK nếu:

- Recall dựng chain riêng.

- Không xác định batch/scope.

- Không có inventory impact.

- Không có order/shipment impact khi có dữ liệu.

- Không có customer exposure impact khi có dữ liệu.

- Hold và sale lock bị trộn.

- Recovery/disposition/CAPA không là object riêng.

- Close with residual risk không explicit.

- Field App close recall không có owner approval.

- Recall không chặn flow thật.

## 30. OP-SLK — SALE LOCK / STOP SALE DOMAIN

## 30.1. Domain Purpose

OP-SLK kiểm soát khóa bán theo SKU, batch, lot, trade item, channel, program hoặc campaign.

Sale Lock chặn downstream:

- AI Advisor.

- Gateway.

- CRM.

- ADS.

- MC AI.

- Landing Page.

- Order.

- Quote.

- Allocation.

- Shipping nếu scope yêu cầu.

## 30.2. Owner Core

- Recall Owner.

- Risk Owner.

- Operational Owner.

- Product Activation Owner.

- Channel Owner với vai trò Consumer.

- Final Business Owner nếu scope nghiêm trọng.

## 30.3. Source-of-Truth

- Sale Lock Registry.

- Hold Registry.

- Recall Case.

- Stop Sale Event.

- Suppression Event.

- Product Activation Status.

- Channel Block Status.

## 30.4. Field App Boundary

Field App phải:

- Hiển thị cảnh báo Sale Lock tại hiện trường.

- Chặn thao tác bị ảnh hưởng theo guard.

- Không cho confirm xuất/nhập nếu lock block.

- Không cho tiếp tục thao tác batch bị stop-sale nếu policy chặn.

- Ghi nhận evidence khi phát hiện lỗi liên quan lock.

Field App không được:

- Gỡ Sale Lock.

- Bỏ qua Sale Lock.

- Thao tác tiếp trên batch bị lock nếu guard BLOCK.

- Tự quyết định reopen.

## 30.5. Required Resolver

- SaleLockResolver.

- StopSaleResolver.

- ProductActivationResolver.

- ChannelBlockResolver.

- ConsumerSuppressionResolver.

- RecallCaseResolver.

- HoldStatusResolver.

## 30.6. Required Guard

- SaleLockGuard.

- StopSaleGuard.

- ProductActivationGuard.

- ChannelBlockGuard.

- ConsumerSuppressionGuard.

- ReopenAfterLockGuard.

- NoConsumerBypassGuard.

- FieldSaleLockGuard.

## 30.7. Required Trace IDs

- sale_lock_id.

- stop_sale_event_id.

- recall_case_id nếu có.

- affected_sku_id.

- affected_batch_id nếu có.

- affected_trade_item_id nếu có.

- affected_channel_id nếu có.

- suppression_event_id.

- reopen_event_id nếu có.

- audit_id.

- correlation_id.

## 30.8. bị chặn If Missing

OP-SLK BLOCK nếu:

- SKU/batch bị recall vẫn bán được.

- Sale Lock không chặn AI/CRM/ADS/MC AI/Gateway/Landing Page.

- Sale Lock không chặn Order nếu scope yêu cầu.

- Field App bỏ qua Sale Lock.

- Không xác định được scope lock.

- Không có suppression event.

- Mở bán lại không có recovery evidence.

- Không có owner sign-off khi cần.

- Không audit được lock/unlock.

## 31. OP-MIS — MISA INTEGRATION BOUNDARY DOMAIN

## 31.1. Domain Purpose

OP-MIS kiểm soát ngõ hạch toán chính thức giữa Operational Core và MISA.

Operational Core owner truth nghiệp vụ.

MISA owner accounting record chính thức.

MISA không điều khiển nghiệp vụ nguồn.

## 31.2. Owner Core

- MISA Integration Owner.

- Accounting Owner.

- Warehouse Owner.

- Production Owner.

- Inventory Owner.

- Finance Owner.

## 31.3. Source-of-Truth

- Operational Business Checkpoint.

- MISA Mapping Registry.

- MISA Sync Event.

- MISA Sync Log.

- MISA Reconcile Record.

- Accounting Review nếu cần.

## 31.4. Field App Boundary

Field App không sync MISA trực tiếp.

Field App chỉ capture dữ liệu hiện trường.

MISA Integration Layer chỉ nhận dữ liệu đã qua Operational checkpoint đủ điều kiện.

## 31.5. Required Resolver

- MISAMappingResolver.

- MISASyncEligibilityResolver.

- MISAEventResolver.

- MISAStatusResolver.

- MISAReconcileResolver.

- AccountingCheckpointResolver.

## 31.6. Required Guard

- MISAMappingRequiredGuard.

- OneMISAIntegrationLayerGuard.

- NoDirectModuleSyncGuard.

- MISASyncStatusGuard.

- MISAReconcileGuard.

- MISANoBusinessTruthGuard.

## 31.7. Required Checkpoints

MISA sync chừa ngõ cho:

- Intake approved.

- Phiếu kế toán xuất nguyên liệu cho sản xuất.

- Warehouse receipt confirmed.

- Sync retry/error.

- Reconcile.

## 31.8. Required Trace IDs

- misa_mapping_id.

- misa_sync_event_id.

- misa_sync_log_id.

- misa_reconcile_record_id.

- operational_checkpoint_id.

- accounting_reference_id nếu có.

- audit_id.

- correlation_id.

## 31.9. bị chặn If Missing

OP-MIS BLOCK nếu:

- Không có mapping.

- Mỗi module sync MISA một kiểu.

- MISA điều khiển business truth.

- Field App sync MISA trực tiếp.

- Không có sync status.

- Không có retry.

- Không có error log.

- Không có reconcile.

- Không trace được operational checkpoint.

- Không audit được sync.

## 32. OP-EVD — EVIDENCE / SMOKE DOMAIN

## 32.1. Domain Purpose

OP-EVD kiểm soát bằng chứng và smoke vận hành.

PACK-01 dùng mô hình:

01 Production Run / 01 Batch / 01 Operational Evidence Packet

và

01 operational_smoke_run_id cho mỗi lần smoke xuyên chuỗi

## 32.2. Owner Core

- QA Owner.

- Evidence Owner.

- Operational Owner.

- Release Owner.

- Domain Owner tương ứng từng bước.

## 32.3. Source-of-Truth

- Operational Evidence Packet.

- Evidence Item.

- Mobile Event Audit.

- Device Evidence.

- Print Log.

- QC Evidence.

- Warehouse Evidence.

- Trace Evidence.

- Recall Evidence.

- MISA Sync Evidence.

- Smoke Run.

- Smoke Step Result.

- Owner Sign-off.

## 32.4. Field App Boundary

Field App là nguồn capture evidence hiện trường chính.

Evidence từ Field App phải có:

- actor_id.

- role_code.

- device_id.

- app_version.

- object binding.

- timestamp.

- sync_status.

- audit_id.

- correlation_id.

Media không gắn object không được dùng PASS.

Offline media chưa sync không được dùng PASS.

## 32.5. Required Resolver

- EvidencePacketResolver.

- EvidenceItemResolver.

- FieldEvidenceResolver.

- MobileEventAuditResolver.

- SmokeRunResolver.

- SmokeStepResolver.

- OwnerSignoffResolver.

- EvidenceStatusResolver.

## 32.6. Required Guard

- EvidenceCompletenessGuard.

- EvidenceAcceptedGuard.

- FieldEvidenceObjectBindingGuard.

- MobileEventAuditGuard.

- SmokePassGuard.

- NegativePathEvidenceGuard.

- OwnerSignoffGuard.

- NoPartialSmokePassGuard.

## 32.7. Required Trace IDs

- evidence_packet_id.

- evidence_item_id.

- mobile_event_id nếu từ Field App.

- operational_smoke_run_id.

- smoke_step_id.

- batch_id.

- production_order_id.

- owner_signoff_id.

- audit_id.

- correlation_id.

## 32.8. bị chặn If Missing

OP-EVD BLOCK nếu:

- Không có evidence packet.

- Evidence chưa ACCEPTED.

- Field evidence không gắn object.

- Mobile event không audit.

- Smoke chưa chạy.

- Smoke chỉ partial.

- Negative path thiếu.

- Không có batch/production run trace.

- Không có owner sign-off.

- Không chứng minh được recall/sale lock chặn flow thật.

## 33. CROSS-DOMAIN FLOW REGISTRY

## 33.1. Flow 01 — Source To Raw Lot

Chuỗi:

Source Zone -> Source Origin -> Company/Supplier Source -> Field Evidence -> Intake -> QC -> Raw Material Lot -> READY_FOR_PRODUCTION

bị chặn nếu:

- Thiếu source.

- Thiếu evidence nếu policy yêu cầu.

- Supplier evidence chưa verified.

- QC chưa PASS.

- Lot chưa READY.

- Field evidence không gắn object.

## 33.2. Flow 02 — Raw Lot To Production

Chuỗi:

Production Order -> BOM Snapshot -> Material Request -> Material Approval -> Field Scan/Check Lot -> Material Issue by Lot -> Material Receipt Confirmation

bị chặn nếu:

- Lệnh sản xuất không snapshot công thức.

- User chọn tay nguyên liệu.

- Lot chưa READY_FOR_PRODUCTION.

- Issue không lot-based.

- Không có phiếu chấp thuận nguyên liệu.

- Field App confirm nhận nguyên liệu không có actor/device/audit.

## 33.3. Flow 03 — Production To Batch

Chuỗi:

Material Receipt -> Field Execution Start -> Process Event -> Output/Loss -> Execution Complete -> Batch Create -> Batch Material Usage

bị chặn nếu:

- Execution không có order.

- Process event không audit.

- Batch không genealogy.

- Produced/accepted/loss bị trộn.

- Batch mới tạo bị coi như released.

- Field App mutate batch trực tiếp.

## 33.4. Flow 04 — Batch To Packaging / Print

Chuỗi:

Batch -> Packaging Level 1 -> Packaging Level 2 Hộp/Thùng -> Field Print Action -> Print Payload -> Print Log -> QR/Barcode Registry

bị chặn nếu:

- Packaging không batch-based.

- Cấp 1/cấp 2 lẫn nhau.

- Hộp/thùng không rõ.

- User nhập tay barcode/QR.

- Printer tự sinh business data.

- Reprint không reason/audit.

- Field App bỏ qua print payload.

## 33.5. Flow 05 — QC / Release / Warehouse

Chuỗi:

Field QC -> QC Inspection -> QC Result -> Batch Disposition -> Batch Release -> Field Scan Inbound -> Warehouse Receipt Confirmed -> Inventory Ledger

bị chặn nếu:

- QC_PASS tự release.

- Batch rejected vẫn release.

- Batch chưa RELEASED vẫn nhập kho.

- Scan inbound bị coi là confirm.

- Inventory tăng trước receipt confirmed.

- Ledger không append-only.

## 33.6. Flow 06 — Inventory To Sellable

Chuỗi:

Inventory Ledger -> Inventory Balance -> Stock Available -> Hold/Sale Lock Check -> Product Activation / Sellable Gate

bị chặn nếu:

- stock_available <= 0.

- quality hold.

- recall hold.

- sale lock.

- channel block.

- price inactive nếu thuộc commerce scope.

- Consumer tự suy luận stock.

## 33.7. Flow 07 — Trace To Recall

Chuỗi:

Trace Search -> Field Scan -> Genealogy -> Exposure Resolution -> Recall Case -> Hold -> Sale Lock -> Impact Snapshot -> Field Recovery Evidence -> Disposition -> CAPA -> Close

bị chặn nếu:

- Trace chain đứt.

- Recall dựng chain riêng.

- Impact thiếu.

- Hold/sale lock trộn nhau.

- Recovery/disposition/CAPA thiếu.

- Field App close recall thiếu owner approval.

## 33.8. Flow 08 — Operational To MISA

Chuỗi:

Operational Business Checkpoint -> MISA Mapping -> MISA Integration Layer -> Sync Status -> Retry/Error -> Reconcile

bị chặn nếu:

- Mapping thiếu.

- Sync riêng từng module.

- Không có sync status.

- Không có retry/error/reconcile.

- MISA điều khiển nghiệp vụ nguồn.

- Field App sync MISA trực tiếp.

## 34. DOMAIN STATUS REGISTRY

## 34.1. Standard Domain Status

Status | Ý nghĩa

NOT_CONFIGURED | Chưa cấu hình

CONFIGURED | Đã có config

READY_FOR_TEST | Sẵn sàng test

TESTING | Đang test

PASS_GOVERNANCE | Đạt tầng tài liệu/governance

waiting_EVIDENCE | Chờ evidence

waiting_SMOKE | Chờ smoke

bị chặn | Bị chặn

READY_FOR_HANDOVER_CANDIDATE | Có thể xét handover

Ready For Handover | Đủ handover trong scope

Production Ready Candidate | Ứng viên production ready

LIVE | Đang vận hành

PAUSED | Tạm dừng

ROLLBACK_REQUIRED | Cần rollback

RETIRED | Ngưng dùng

## 34.2. Default Domain Status For PACK-01

Domain | Default Status

Source Origin | waiting_EVIDENCE

Raw Material Intake | waiting_EVIDENCE

Raw Material QC / Lot Readiness | waiting_EVIDENCE

Field Operations App | waiting_EVIDENCE

Recipe / BOM Snapshot | waiting_EVIDENCE

Production Order | waiting_EVIDENCE

Material Issue / Receipt | waiting_EVIDENCE

Production Execution | waiting_EVIDENCE

Batch | waiting_EVIDENCE

Packaging | waiting_EVIDENCE

Printing | waiting_EVIDENCE

QC / Batch Release | waiting_EVIDENCE

Warehouse Receipt | waiting_EVIDENCE

Inventory | waiting_EVIDENCE

Traceability | waiting_EVIDENCE

Recall | waiting_EVIDENCE

Sale Lock / Stop Sale | waiting_EVIDENCE

MISA Integration Boundary | waiting_EVIDENCE

Evidence / Smoke | waiting_EVIDENCE

Không domain nào được gọi PASS nếu chưa có evidence và smoke tương ứng.

## 35. DOMAIN CONFLICT RULES

## 35.1. Conflict Priority

Khi có xung đột, thứ tự ưu tiên là:

- Recall / Safety.

- Sale Lock / Stop Sale.

- QC Hold / Reject.

- Batch Release Status.

- Warehouse Receipt Confirmed.

- Inventory Ledger.

- Product Activation / Sellable.

- Order / Allocation.

- AI / CRM / ADS / MC AI / Gateway.

- MISA Sync.

- Reporting / Dashboard.

## 35.2. Critical Conflict Rules

- QC_PASS nhưng chưa RELEASED -> không nhập kho.

- RELEASED nhưng chưa warehouse receipt confirmed -> chưa tăng tồn.

- Scan inbound nhưng chưa confirm -> chưa tăng tồn.

- Có tồn nhưng Sale Lock active -> không sellable.

- Có trace nhưng public policy chưa bật -> không public trace.

- Có supplier evidence nhưng chưa verification -> chưa source PASS.

- Có print payload nhưng batch hold -> không sellable.

- Có MISA sync nhưng operational truth sai -> xử lý operational truth trước.

- Có ADS demand nhưng stock/sellable không PASS -> không scale/không bán.

- Có order demand nhưng recall active -> chặn order mới theo scope.

- Có MC AI live board nhưng Sale Lock active -> MC AI phải dừng SKU.

- Field App gửi action hợp lệ nhưng backend guard BLOCK -> backend decision thắng.

## 36. PHẦN 2/4 DONE GATE

## PHẦN 2/4 được xem là đạt governance scope khi đã khóa đủ:

- Domain Registry Principles.

- Standard Domain Record.

- Domain Registry Overview.

- Source Origin Domain.

- Raw Material Intake Domain.

- Raw Material QC / Lot Readiness Domain.

- Field Operations Internal App / Mobile PWA Domain.

- Recipe / BOM Snapshot Domain.

- Production Order Domain.

- Material Issue / Material Receipt Domain.

- Production Execution Domain.

- Batch Domain.

- Packaging Level 1 / Level 2 Hộp-Thùng Domain.

- Printing / Print Payload / Reprint Domain.

- QC / Batch Release Domain.

- Warehouse Receipt Domain.

- Inventory Ledger / Stock Balance Domain.

- Traceability Domain.

- Recall / Recovery / Disposition / CAPA Domain.

- Sale Lock / Stop Sale Domain.

- MISA Integration Boundary Domain.

- Evidence / Smoke Domain.

- Cross-Domain Flow Registry.

- Domain Status Registry.

- Domain Conflict Rules.

- Field App không owner business truth.

- Scan không đồng nghĩa confirm.

- Media evidence không mồ côi.

- Offline queue / device identity / idempotency là bắt buộc.

- No-hardcode rule cho toàn bộ domain.

## 37. PHẦN 2/4 FINAL CONCLUSION

## PHẦN 2/4 khóa Domain Registry cho toàn bộ Operational Core.

Từ điểm này trở đi, mọi nghiệp vụ sản xuất / kho / tồn kho / truy xuất / thu hồi / sale lock phải đi đúng registry:

Source Origin -> Raw Intake -> Lot QC / Readiness -> Field Operations App Evidence -> Recipe / BOM Snapshot -> Production Order -> Material Issue -> Execution -> Batch -> Packaging Hộp/Thùng -> Printing -> QC -> Release -> Warehouse Receipt -> Inventory Ledger -> Traceability -> Recall -> Sale Lock / Stop Sale -> MISA Boundary -> Evidence / Smoke

Không domain nào được tự làm thay domain khác.

Không Field App nào được owner business truth.

Không Admin Web nào được thay thế capture hiện trường bắt buộc.

Không Consumer nào được tự quyết định sự thật vận hành.

Không có đường tắt từ app sang database.

Không có đường tắt từ scan sang confirm.

Không có đường tắt từ QC_PASS sang RELEASED.

Không có đường tắt từ printing sang inventory.

Không có đường tắt từ warehouse scan sang inventory.

Không có đường tắt từ MISA sang operational truth.

Không có đường tắt từ AI/CRM/ADS/MC AI/Gateway sang sellable.

PACK-01 PHẦN 2/4 là nền để viết tiếp:

## PHẦN 3/4 — SELLABLE GATE / STOCK GATE / RECALL STOP-SALE / FIELD SUPPRESSION / CONSUMER SUPPRESSION CONTROL

## PHẦN 3/4 — SELLABLE GATE / STOCK GATE / RECALL STOP-SALE / FIELD SUPPRESSION / CONSUMER SUPPRESSION CONTROL

## 38. SELLABLE GATE CONTROL PRINCIPLES

## 38.1. Mục tiêu Sellable Gate

Sellable Gate là cổng quyết định một SKU, batch, hộp, thùng hoặc channel có được phép đi vào bán hàng hay không.

Sellable Gate không phải là quyết định của:

- AI Advisor.

- CRM.

- ADS.

- MC AI.

- Gateway.

- Landing Page.

- Order.

- Sales operator.

- Field Operations App.

- Admin Web đơn lẻ.

Sellable Gate là kết quả tổng hợp từ Operational Core, bao gồm:

- Batch thật.

- QC hợp lệ.

- Batch Release hợp lệ.

- Warehouse Receipt Confirmed.

- Inventory Ledger hợp lệ.

- Stock Available > 0.

- HSD hợp lệ.

- Traceability hợp lệ.

- Không Quality Hold.

- Không Recall Hold.

- Không Sale Lock.

- Không Stop Sale.

- Trade Item hộp/thùng đủ điều kiện.

- Channel không bị block.

- Evidence không có STOP_POINTS P0.

## 38.2. Sellable Gate mặc định là BLOCK

Mặc định mọi SKU / batch / trade item là:

## NOT SELLABLE

Chỉ được chuyển thành SELLABLE khi tất cả điều kiện P0 cùng PASS.

Không có dữ liệu thì không suy luận là bán được.

Không có stock thì không bán.

Không rõ batch thì không bán.

Không rõ HSD thì không bán.

Không rõ recall/sale lock thì không bán.

Không rõ trade item hộp/thùng thì không bán theo cấp hộp/thùng.

Không rõ channel eligibility thì không bán trên channel đó.

## 38.3. Operational Sellable và Commercial Sellable phải tách riêng

PACK-01 chỉ khóa Operational Sellable.

Lớp | Ý nghĩa | Owner

Operational Sellable | Sản phẩm đủ điều kiện vận hành: batch, QC, release, kho, tồn, trace, recall, sale lock | Operational Core

Commercial Sellable | Sản phẩm đủ điều kiện thương mại: giá, chương trình, channel, payment, shipping, order | Commerce / Pricing / Channel / Order Pack

Nếu Operational Sellable = BLOCK thì Commercial Sellable bắt buộc BLOCK.

Commercial Pack không được tự mở bán khi Operational Sellable chưa PASS.

## 38.4. Các trạng thái không được hiểu là Sellable

Các trạng thái sau không được hiểu là sản phẩm đã bán được:

- Production Order đã mở.

- Production Execution đã hoàn tất.

- Batch đã tạo.

- Packaging đã hoàn tất.

- Printing đã hoàn tất.

- QC_PASS.

- Batch RELEASED nhưng chưa nhập kho.

- Warehouse scan nhưng chưa confirm.

- MISA sync success.

- Có ảnh/video hiện trường.

- Có phiếu nhưng chưa đủ sign-off.

- Có AI/CRM/ADS demand.

- Có Live Board.

- Có Landing Page.

- Có Order Draft.

- Có khách hỏi mua.

Chuỗi tối thiểu để xét sellable:

## 39. BASE SELLABLE GATE

## 39.1. Điều kiện Base Sellable Gate

Một SKU / batch / hộp / thùng chỉ được Operational Sellable PASS khi đồng thời đạt:

- SKU canonical hợp lệ.

- Công thức/version hợp lệ.

- Operational config hợp lệ.

- Batch thật tồn tại.

- Batch trace được raw material lots.

- Batch đã qua QC thành phẩm.

- Batch đã RELEASED.

- Thành phẩm đã Warehouse Receipt Confirmed.

- Inventory Ledger đã ghi nhận.

- Stock available > 0.

- HSD còn hiệu lực.

- Không quality hold.

- Không recall hold.

- Không sale lock.

- Không stop sale.

- Không channel block.

- Hộp/thùng đủ packaging config.

- Hộp/thùng đủ print config.

- Hộp/thùng có lô/NSX/HSD/barcode/QR nếu là cấp in 2.

- Traceability đủ điều kiện.

- Public trace đủ policy nếu channel yêu cầu.

- Evidence không có STOP_POINTS.

- Smoke negative path không fail với SKU/batch đó.

## 39.2. Base Sellable Gate BLOCK khi

Sellable Gate bắt buộc BLOCK nếu có một trong các lỗi:

- SKU không hợp lệ.

- Công thức/version không hợp lệ.

- Thiếu operational config.

- Batch không tồn tại.

- Batch không trace được raw material lots.

- Batch chưa QC.

- Batch QC_HOLD.

- Batch QC_REJECT.

- Batch QC_PASS nhưng chưa RELEASED.

- Batch RELEASED nhưng chưa Warehouse Receipt Confirmed.

- Inventory Ledger chưa ghi nhận.

- Stock available = 0.

- HSD thiếu.

- HSD hết hạn.

- Quality Hold active.

- Recall Hold active.

- Sale Lock active.

- Stop Sale active.

- Channel Block active.

- Hộp/thùng chưa đủ print config.

- Hộp/thùng thiếu QR/barcode/lô/NSX/HSD.

- Trace gap nghiêm trọng.

- Public trace policy chưa đủ nhưng channel yêu cầu public trace.

- Consumer cố bán vượt guard.

## 40. SELLABLE GATE REQUIRED INPUTS / RESOLVER / GUARD

## 40.1. Required Inputs

Sellable Gate phải nhận đủ các input sau:

- sku_id.

- sku_code.

- formula_version_id.

- operational_config_id.

- batch_id.

- batch_code.

- batch_release_id.

- warehouse_receipt_id.

- warehouse_id.

- warehouse_location_id.

- inventory_ledger_id.

- stock_balance_snapshot_id.

- mfg_date.

- hsd / expiry_date.

- qc_status.

- release_status.

- quality_hold_status.

- recall_hold_status.

- sale_lock_status.

- stop_sale_status.

- trace_status.

- recall_ready_status.

- trade_item_id nếu bán theo hộp/thùng.

- gtin_id nếu trade item đã có GTIN/GS1.

- qr_registry_id nếu dùng QR.

- channel_id nếu xét theo kênh.

- evidence_packet_id nếu thuộc release/readiness review.

- operational_smoke_run_id nếu thuộc release/readiness review.

## 40.2. Required Resolver

Sellable Gate bắt buộc gọi hoặc nhận kết quả từ:

- SKUCanonicalResolver.

- FormulaVersionResolver.

- OperationalConfigResolver.

- BatchResolver.

- BatchGenealogyResolver.

- QCStatusResolver.

- BatchReleaseResolver.

- WarehouseReceiptResolver.

- InventoryLedgerResolver.

- StockAvailableResolver.

- ShelfLifeResolver.

- QualityHoldResolver.

- RecallHoldResolver.

- SaleLockResolver.

- StopSaleResolver.

- TraceabilityResolver.

- RecallReadinessResolver.

- TradeItemResolver.

- GTINResolver nếu có.

- QRRegistryResolver nếu có.

- ChannelEligibilityResolver.

- EvidencePacketResolver nếu xét release.

- SmokeRunResolver nếu xét release.

## 40.3. Required Guard

Sellable Gate bắt buộc có các guard:

- SKUActiveGuard.

- FormulaVersionGuard.

- OperationalConfigGuard.

- BatchExistsGuard.

- BatchGenealogyGuard.

- BatchQCGuard.

- BatchReleasedGuard.

- WarehouseReceiptConfirmedGuard.

- InventoryLedgerGuard.

- StockAvailableGuard.

- ShelfLifeGuard.

- QualityHoldGuard.

- RecallHoldGuard.

- SaleLockGuard.

- StopSaleGuard.

- TraceReadyGuard.

- RecallReadyGuard.

- TradeItemReadyGuard.

- GTINReadyGuard nếu trade item yêu cầu.

- QRReadyGuard nếu trace policy yêu cầu.

- ChannelEligibilityGuard.

- NoConsumerBypassGuard.

## 40.4. Required Decision Output

Sellable Gate phải trả decision record tối thiểu:

Field | Ý nghĩa

sellable_decision_id | Mã quyết định

sku_id | SKU

batch_id | Batch

trade_item_id | Hộp/thùng nếu có

warehouse_id | Kho

warehouse_location_id | Location

channel_id | Channel nếu có

operational_sellable_status | PASS / BLOCK

stock_status | PASS / BLOCK

hsd_status | VALID / EXPIRED / MISSING

qc_status | QC state

release_status | Release state

trace_status | Trace state

recall_status | CLEAR / HOLD / RECALL

sale_lock_status | CLEAR / LOCKED

decision | SELLABLE / bị chặn / REVIEW_REQUIRED

block_reasons | Danh sách lý do block

valid_from | Thời điểm hiệu lực

valid_until | Hết hiệu lực nếu có

resolver_snapshot_id | Snapshot resolver

audit_id | Audit

correlation_id | Correlation

## 41. STOCK GATE CONTROL

## 41.1. Mục tiêu Stock Gate

Stock Gate quyết định sản phẩm có tồn khả dụng để bán hay không.

Stock Gate không chỉ nhìn vào số lượng tồn tổng.

Stock Gate phải phân biệt:

- Stock on hand.

- Stock available.

- Stock reserved.

- Stock bị chặn.

- Stock on hold.

- Stock recall locked.

- Stock allocated.

- Stock expired.

- Stock quarantine.

- Channel stock.

- Trade item stock theo hộp/thùng.

- Warehouse/location stock.

## 41.2. Stock Availability Model

Stock Available được hiểu theo logic governance:

Stock Available = Stock On Hand - Stock Reserved - Stock bị chặn - Stock On Hold - Stock Allocated - Stock Recall Locked - Stock Expired

Đây là công thức logic nghiệp vụ.

Cách tính thật phải do Inventory Resolver thực hiện theo:

- Inventory Ledger.

- Inventory Lot Balance.

- Warehouse/location.

- Batch.

- Trade item.

- Channel policy.

- Recall/sale lock policy.

- Allocation policy.

Không Consumer nào được tự tính stock available.

## 41.3. Stock Gate PASS khi

Stock Gate PASS khi:

- Inventory Ledger tồn tại.

- Inventory Balance khớp Ledger.

- stock_available > 0.

- Stock thuộc warehouse/location hợp lệ.

- Stock không bị hold.

- Stock không bị recall.

- Stock không bị sale lock.

- Stock không expired.

- Stock không quarantine.

- Stock không bị channel block.

- Stock không bị allocation hết.

- Không có negative inventory risk.

- Không có ledger conflict.

- Không có unresolved adjustment.

- Không có recall impact chưa xử lý.

## 41.4. Stock Gate BLOCK khi

Stock Gate BLOCK nếu:

- stock_available = 0.

- Stock âm.

- Stock không khớp ledger.

- Batch chưa nhập kho.

- Warehouse Receipt chưa confirm.

- Stock bị quality hold.

- Stock bị recall hold.

- Stock bị sale lock.

- Stock bị stop sale.

- Stock bị channel block.

- Stock expired.

- Stock quarantine.

- Stock đã allocated hết.

- Stock chưa rõ warehouse/location.

- Inventory projection conflict với ledger.

- Consumer tự suy luận còn hàng.

- Field App cố confirm xuất/nhập vượt guard.

- Admin Web sửa tồn trực tiếp không qua approved adjustment.

## 41.5. Kho Bến Tre trong Stock Gate

Kho thật ban đầu là:

Kho Bến Tre

Quy tắc:

- Kho Bến Tre phải được đọc từ Warehouse Registry.

- Không hardcode tên kho trong code.

- Không hardcode tên kho trong UI.

- Không hardcode tên kho trong AI/CRM/ADS/Gateway.

- Stock phải tính theo warehouse_id / location_id.

- Không cộng gộp tồn nhiều kho nếu channel chỉ bán từ một kho cụ thể.

- Nếu mở thêm kho, Stock Gate phải resolve lại theo registry.

## 42. TRADE ITEM SELLABLE CONTROL — HỘP / THÙNG

## 42.1. Mục tiêu Trade Item Sellable

Trade Item Sellable kiểm soát điều kiện bán theo cấp thương mại.

Trong PACK-01, cấp in 2 gồm:

- Hộp.

- Thùng.

Hộp và thùng có thể là hai trade item khác nhau.

Mỗi trade item có thể có:

- Packaging config riêng.

- Print config riêng.

- GTIN/GS1 riêng.

- Barcode thương mại riêng.

- QR trace policy riêng.

- HSD hiển thị.

- Batch/lô in trên nhãn.

- Stock unit riêng.

- Sellable gate riêng.

## 42.2. Trade Item Readiness

Một trade item hộp/thùng chỉ ready khi:

- Bind đúng SKU canonical.

- Bind đúng batch.

- Bind đúng packaging level.

- Có packaging config.

- Có print config.

- Có HSD.

- Có batch/lô.

- Có QR nếu trace policy yêu cầu.

- Có GTIN/GS1 nếu barcode thương mại đã active.

- Không có hai barcode thương mại trên cùng trade item.

- QR không thay GTIN/GS1.

- Print payload do backend sinh.

- Reprint nếu có đã kiểm soát.

- Trace được về batch thật.

- Recall được theo batch/trade item.

- Stock available theo đúng đơn vị bán.

## 42.3. Hộp Sellable Gate

Hộp chỉ được sellable khi:

- Packaging cấp hộp hoàn tất.

- In cấp 2 trên hộp có đủ:

o | lô/batch,

o | NSX,

o | HSD,

o | mã vạch,

o | QR.

- QR map đúng batch.

- Barcode lấy từ Trade Item Registry nếu GTIN/GS1 đã active.

- Không nhập tay barcode.

- Không có reprint lỗi chưa xử lý.

- Batch đã RELEASED.

- Hộp đã nhập kho theo đơn vị tồn phù hợp nếu bán theo hộp.

- Stock available theo hộp > 0.

- Không recall.

- Không sale lock.

- Không channel block.

## 42.4. Thùng Sellable Gate

Thùng chỉ được sellable khi:

- Packaging cấp thùng hoàn tất.

- In cấp 2 trên thùng có đủ:

o | lô/batch,

o | NSX,

o | HSD,

o | mã vạch,

o | QR.

- QR map đúng batch hoặc đúng cấu trúc thùng theo policy.

- Barcode lấy từ Trade Item Registry nếu GTIN/GS1 thùng đã active.

- Không nhập tay barcode thùng.

- Không in barcode hộp cho thùng nếu thùng có trade item riêng.

- Không có reprint lỗi chưa xử lý.

- Batch đã RELEASED.

- Thùng đã nhập kho theo đơn vị tồn phù hợp nếu bán theo thùng.

- Stock available theo thùng > 0.

- Không recall.

- Không sale lock.

- Không channel block.

## 42.5. Trade Item BLOCK khi

Trade item hộp/thùng BLOCK nếu:

- Chưa có packaging config.

- Chưa có print config.

- In thiếu lô.

- In thiếu NSX.

- In thiếu HSD.

- In thiếu barcode.

- In thiếu QR.

- QR không map batch thật.

- Barcode nhập tay.

- GTIN/GS1 chưa active nhưng vẫn thương mại hóa bằng barcode.

- Một trade item có hai barcode thương mại.

- QR bị dùng thay barcode thương mại.

- HSD thiếu hoặc sai.

- Batch chưa RELEASED.

- Chưa Warehouse Receipt Confirmed.

- Stock available = 0.

- Recall active.

- Sale Lock active.

- Traceability chưa đủ.

## 43. HSD / SHELF-LIFE GATE

## 43.1. Mục tiêu Shelf-life Gate

HSD là điều kiện bắt buộc của Sellable Gate.

Mặc định ban đầu:

HSD 12 tháng

Cách quản trị:

- Theo từng SKU nếu có cấu hình riêng.

- Theo nhóm SKU nếu Owner quyết định dùng chung.

- Mặc định dùng 12 tháng nếu chưa có khác biệt.

## 43.2. Quy tắc sinh HSD

HSD phải được sinh theo:

NSX / MFG Date + Shelf-life Config

Không cho người vận hành nhập tay HSD trong flow thường.

Nếu cần sửa HSD, phải đi qua:

- reason;

- approval;

- audit;

- evidence;

- role permission;

- state transition log.

## 43.3. Shelf-life Gate PASS khi

Shelf-life Gate PASS khi:

- Batch có MFG date.

- SKU/nhóm SKU có shelf-life config.

- HSD được sinh đúng.

- HSD được in đúng trên hộp/thùng.

- HSD chưa hết hạn.

- Không có conflict giữa batch record và print payload.

- HSD trace được nguồn tính.

## 43.4. Shelf-life Gate BLOCK khi

Shelf-life Gate BLOCK nếu:

- Thiếu MFG date.

- Thiếu shelf-life config.

- HSD không sinh được.

- HSD bị nhập tay không audit.

- HSD trên print payload khác batch record.

- HSD hết hạn.

- HSD bị sửa không có approval.

- HSD không trace được nguồn tính.

## 44. QC QUALITY GATE — ĐỘ ẨM SAU SẤY

## 44.1. Mục tiêu Quality Gate

QC Quality Gate kiểm soát chất lượng sau sấy thăng hoa và QC thành phẩm trước release/warehouse/sellable.

Chỉ tiêu độ ẩm là chỉ tiêu bắt buộc trong QC sau sấy.

## 44.2. Moisture Threshold

Ngưỡng vận hành:

Độ ẩm sau sấy | Decision | Hành động

< 2% | PASS | Có thể đi tiếp theo gate QC

2%–4% | HOLD / REVIEW | QC review, chưa downstream tự động

> 4% | FAIL | Chặn downstream

## 44.3. Quality Gate PASS khi

Quality Gate PASS khi:

- Có phiếu QC sau sấy.

- Có kết quả đo độ ẩm.

- Độ ẩm < 2%.

- Có người QC xác nhận.

- Có evidence nếu policy yêu cầu.

- Có audit.

- Không có defect nghiêm trọng.

- Batch đủ điều kiện chuyển bước tiếp theo.

## 44.4. Quality Gate HOLD khi

Quality Gate HOLD khi:

- Độ ẩm từ 2% đến 4%.

- Có chỉ tiêu cảm quan cần review.

- Có defect chưa kết luận.

- Có chênh lệch số liệu cần kiểm tra.

- QC yêu cầu kiểm lại.

HOLD không được hiểu là PASS.

HOLD phải chặn release, warehouse, sellable và consumer downstream.

## 44.5. Quality Gate FAIL khi

Quality Gate FAIL khi:

- Độ ẩm > 4%.

- Batch không đạt cảm quan nghiêm trọng.

- Batch có defect không thể khắc phục.

- Evidence xác nhận không đạt.

- QC reject.

- Owner quyết định reject.

FAIL phải chặn toàn bộ downstream.

## 45. RECALL STOP-SALE GATE

## 45.1. Mục tiêu Recall Stop-Sale Gate

Recall Stop-Sale Gate đảm bảo khi có rủi ro chất lượng, truy xuất, an toàn, khiếu nại nghiêm trọng, lỗi in, lỗi HSD, lỗi QR/barcode hoặc quyết định Owner, hệ thống có thể dừng bán ngay trong scope ảnh hưởng.

Stop Sale có thể áp dụng theo:

- SKU.

- Batch.

- Raw material lot.

- Trade item hộp.

- Trade item thùng.

- Warehouse.

- Location.

- Channel.

- Program.

- ADS campaign.

- Live session.

- Customer segment.

- Time window.

- Full product line.

## 45.2. Recall Stop-Sale Triggers

Recall Stop-Sale Gate được kích hoạt khi có một trong các trigger:

- QC phát hiện lỗi nghiêm trọng.

- Batch hold.

- Batch reject.

- Recall case mở.

- Trace gap nghiêm trọng.

- QR map sai batch.

- Print error nghiêm trọng.

- HSD sai.

- GTIN/barcode sai trade item.

- Inventory ledger conflict.

- Supplier/source evidence bị reject.

- Customer complaint có evidence batch/QR.

- Counterfeit suspicion.

- Owner quyết định stop sale.

- Regulatory/compliance risk.

- Field App phát hiện lỗi hiện trường thuộc nhóm P0.

- Offline evidence sync trễ nhưng ảnh hưởng batch đã bán/đang bán.

- App/device ghi nhận conflict giữa mã scan và batch thật.

## 45.3. Hold và Sale Lock phải tách riêng

Cơ chế | Mục tiêu | Tác động

Hold | Giữ nội bộ batch/lot/stock để kiểm tra | Chặn warehouse/internal movement tùy policy

Sale Lock | Khóa bán downstream | Chặn AI/CRM/ADS/MC AI/Gateway/Landing/Order

Stop Sale | Dừng bán theo scope rộng hơn | Chặn thương mại hóa

Recall | Case xử lý thu hồi/impact/recovery/CAPA | Kích hoạt hold, sale lock, impact

Recall có thể kích hoạt Hold và Sale Lock.

Sale Lock không thay thế Recall Case.

Hold không thay thế Sale Lock.

## 45.4. Recall Stop-Sale Required Inputs

Recall Stop-Sale Gate cần tối thiểu:

- trigger_type.

- affected_scope.

- affected_sku_id nếu có.

- affected_batch_id nếu có.

- affected_lot_id nếu có.

- affected_trade_item_id nếu có.

- affected_warehouse_id nếu có.

- affected_channel_id nếu có.

- recall_case_id nếu có.

- hold_id nếu có.

- sale_lock_id.

- stop_sale_event_id.

- impact_snapshot_id.

- evidence_id.

- field_event_id nếu phát sinh từ Field App.

- owner_decision_id nếu cần.

- audit_id.

- correlation_id.

## 45.5. Recall Stop-Sale PASS khi

Recall Stop-Sale Gate PASS khi chứng minh được:

- Scope bị ảnh hưởng xác định rõ.

- Hold kích hoạt đúng nếu cần.

- Sale Lock kích hoạt đúng.

- Field App bị chặn đúng khi thao tác vào scope lock.

- Consumer suppression hoạt động.

- AI không quote SKU/batch bị lock.

- CRM không gửi gợi ý SKU/batch bị lock.

- ADS không chạy/scale SKU bị lock.

- MC AI không nói SKU bị lock.

- Gateway không kéo chốt SKU bị lock.

- Landing Page không CTA mua SKU bị lock.

- Order không tạo mới với SKU bị lock.

- Allocation không xuất hàng nếu scope chặn.

- Trace được impact.

- Recovery/reopen phải có evidence.

- Audit đầy đủ.

## 45.6. Recall Stop-Sale BLOCK khi

Recall Stop-Sale Gate BLOCK nếu:

- Không xác định được scope.

- Không có sale_lock_id.

- Field App vẫn cho thao tác scope bị lock.

- Consumer không nhận suppression.

- AI vẫn quote.

- CRM vẫn gửi.

- ADS vẫn chạy.

- MC AI vẫn nói.

- Gateway vẫn kéo chốt.

- Landing Page vẫn mở CTA.

- Order vẫn tạo mới.

- Stock vẫn sellable.

- Recall không dùng trace chain.

- Recovery chưa có evidence nhưng đã mở bán lại.

- Không có audit.

## 46. FIELD SUPPRESSION CONTROL

## 46.1. Mục tiêu Field Suppression

Field Suppression Control đảm bảo Field Operations App không thể tiếp tục thao tác sai khi Operational Core trả BLOCK.

Field App phải bị suppress khi:

- Batch bị hold.

- Batch bị recall.

- Batch bị sale lock.

- Stock bị block.

- QC failed.

- Trace gap nghiêm trọng.

- HSD conflict.

- QR/barcode conflict.

- Device không hợp lệ.

- App version không hợp lệ.

- User không có permission.

- Object đang ở state không cho thao tác.

- Offline event conflict.

- Idempotency conflict.

- Evidence không hợp lệ.

## 46.2. Field Suppression Decision Types

Decision | Ý nghĩa

SUPPRESS_FIELD_ACTION | Chặn thao tác hiện trường

SUPPRESS_FIELD_CONFIRM | Chặn click-confirm

SUPPRESS_FIELD_SCAN_CONFIRM | Chặn scan được hiểu thành confirm

SUPPRESS_FIELD_MEDIA_PASS | Chặn media dùng làm evidence PASS

SUPPRESS_FIELD_REPRINT | Chặn reprint

SUPPRESS_FIELD_RECEIPT_CONFIRM | Chặn confirm nhập kho

SUPPRESS_FIELD_QC_ACCEPT | Chặn accept QC

SUPPRESS_FIELD_RELEASE | Chặn release từ field nếu không hợp lệ

SUPPRESS_FIELD_RECALL_CLOSE | Chặn close recall

FIELD_REVIEW_REQUIRED | Yêu cầu review

FIELD_HANDOFF_REQUIRED | Chuyển người có thẩm quyền

## 46.3. Field App bị chặn khi Sale Lock active

Khi Sale Lock active, Field App phải:

- Hiển thị cảnh báo lock.

- Chặn thao tác làm sản phẩm đi tiếp sang bán hàng.

- Chặn confirm xuất/nhập nếu lock ảnh hưởng warehouse movement.

- Chặn complete packaging nếu policy yêu cầu hold.

- Chặn reprint nếu batch đang bị lock.

- Chặn scan-confirm cho mục đích sellable.

- Cho phép capture evidence phục vụ recall/recovery nếu policy cho phép.

- Ghi audit mọi attempt bị chặn.

Field App không được bỏ qua Sale Lock vì người vận hành đang ở hiện trường.

## 46.4. Field App bị chặn khi QC HOLD / FAIL

Khi QC HOLD hoặc FAIL:

Field App phải chặn:

- Complete downstream step nếu policy chặn.

- Batch release.

- Warehouse receipt.

- Inventory activation.

- Sellable action.

- Print/reprint nếu QC status không cho phép.

- Handoff sang bán hàng.

Field App vẫn có thể cho phép:

- Capture thêm evidence.

- Ghi lý do.

- Submit review.

- Thực hiện rework nếu policy cho phép.

- Gửi owner review.

## 46.5. Field App bị chặn khi Device/App không hợp lệ

Field App BLOCK nếu:

- Thiết bị chưa đăng ký.

- Thiết bị bị revoke.

- Device session hết hạn.

- App version không còn được phép.

- Thiếu device UUID.

- Thiếu client_event_id.

- Thiếu idempotency_key.

- Request không có actor.

- Actor không có role.

- Role không có permission.

Không đủ device/actor/permission thì action không được ghi state.

## 46.6. Field App bị chặn khi Offline Conflict

Offline event bị BLOCK hoặc REVIEW_REQUIRED nếu:

- Object state đã thay đổi trong lúc offline.

- Batch đã recall trước khi event sync.

- Sale Lock đã active trước khi event sync.

- Payload hash thay đổi với cùng idempotency key.

- Event trùng đã xử lý.

- Media sync trễ nhưng object đã đóng.

- QC action đến muộn sau release.

- Warehouse confirm đến muộn sau adjustment/recall.

- Device clock lệch nghiêm trọng.

- Event không thể reconcile.

Không được vì offline mà tự overwrite truth mới hơn.

## 46.7. Field Suppression Event

Mỗi lần Field App bị suppress phải ghi event:

- field_suppression_event_id.

- field_event_id.

- device_id.

- actor_id.

- role_id.

- object_type.

- object_id.

- suppression_reason.

- guard_decision_id.

- attempted_action.

- decision.

- occurred_at.

- audit_id.

- correlation_id.

## 47. CONSUMER SUPPRESSION CONTROL

## 47.1. Mục tiêu Consumer Suppression

Consumer Suppression Control đảm bảo mọi hệ thống tiêu thụ dữ liệu Operational Core phải dừng hành động khi Operational Core trả BLOCK.

Consumer gồm:

- AI Advisor.

- Gateway.

- CRM.

- ADS.

- MC AI.

- Landing Page.

- Order.

- Quote.

- Payment.

- Shipping.

- IVR.

- Public Trace.

- Reporting/Dashboard.

Consumer không được tự xử lý rule kinh doanh.

Consumer chỉ đọc decision từ resolver/guard.

## 47.2. Suppression Decision Types

Decision | Ý nghĩa

SUPPRESS_SELLING | Dừng bán

SUPPRESS_QUOTE | Dừng báo giá

SUPPRESS_ORDER | Dừng tạo đơn

SUPPRESS_ADS | Dừng ADS

SUPPRESS_CRM | Dừng CRM message

SUPPRESS_LIVE_SCRIPT | Dừng MC AI nói SKU

SUPPRESS_GATEWAY_CTA | Dừng kéo chốt qua Gateway

SUPPRESS_LANDING_CTA | Dừng CTA mua

SUPPRESS_IVR_CALL | Dừng gọi xác nhận nếu đơn chưa đủ điều kiện

SUPPRESS_ALLOCATION | Dừng allocation/xuất hàng

SUPPRESS_PUBLIC_TRACE | Dừng/ẩn public trace nếu policy vi phạm

REVIEW_REQUIRED | Chuyển Owner review

HANDOFF_REQUIRED | Chuyển người phụ trách xử lý

## 47.3. AI Advisor Suppression

AI Advisor phải suppress khi:

- SKU không operational sellable.

- Stock Gate BLOCK.

- Batch chưa release.

- Warehouse receipt chưa confirm.

- HSD không đạt.

- Recall active.

- Sale Lock active.

- Quality hold active.

- Trade item chưa ready.

- Trace gap nghiêm trọng.

- Price/Program pack chưa resolve nếu cần quote.

- Order Gate chưa đủ nếu cần chốt.

AI Advisor không được:

- Báo còn hàng khi Stock Gate chưa PASS.

- Báo giá khi Sellable Gate BLOCK.

- Tạo order draft cho SKU bị lock.

- Nói đang bán khi Product Activation/Sellable chưa PASS.

- Gợi ý thay thế bằng SKU cũng bị lock.

## 47.4. CRM Suppression

CRM phải suppress khi:

- SKU bị stop sale.

- SKU hết hàng.

- SKU bị recall/sale lock.

- Khách có complaint mở.

- Customer opt-out.

- Trigger thiếu.

- Frequency cap vượt.

- Customer identity chưa resolve.

- CRM message gợi ý SKU không sellable.

- CRM đang conflict với after-sales/complaint.

CRM không được biến recall/complaint thành cơ hội bán tiếp.

## 47.5. ADS Suppression

ADS phải suppress khi:

- SKU không active/sellable.

- Stock Gate BLOCK.

- Recall active.

- Sale Lock active.

- HSD issue.

- Trade item hộp/thùng chưa ready.

- GTIN/GS1 trade item chưa đủ điều kiện nếu campaign yêu cầu thương mại hóa barcode.

- Landing Page CTA bị block.

- Order/Payment/Shipping chưa ready nhưng campaign dẫn chốt đơn.

- Verified revenue/data quality không đủ cho scale.

ADS không được tự scale khi thiếu verified revenue, data quality, pilot evidence và owner approval.

## 47.6. MC AI Suppression

MC AI phải suppress khi:

- SKU không nằm trong Daily Live Product Board.

- mc_ai_script_brief_id không hợp lệ.

- SKU bị stock block.

- SKU bị recall/sale lock.

- Program runtime không hợp lệ.

- Stock Guard không PASS.

- Fake urgency guard BLOCK.

- Live moderation yêu cầu dừng.

- Trade item chưa ready.

- Hộp/thùng bị print/trace lỗi.

MC AI không được:

- Nói SKU ngoài board.

- Nói còn hàng khi Stock Guard chưa PASS.

- Nói giá nếu thiếu QuoteSnapshot/Program Runtime.

- Tạo khan hiếm giả.

- Tiếp tục nói SKU đã stop sale.

## 47.7. Gateway Suppression

Gateway phải suppress khi:

- Public comment có nguy cơ báo giá/chốt đơn trái policy.

- SKU bị stop sale.

- Handoff không thành công.

- PII có nguy cơ public.

- Payment instruction không được public.

- Health/complaint cần handoff.

- Troll/abuse cần moderation.

- Gateway Completion Gate chưa PASS.

Gateway không được nói đã gửi Messenger nếu handoff fail.

Gateway không được kéo chốt SKU bị lock.

## 47.8. Landing Page Suppression

Landing Page phải suppress hoặc điều chỉnh CTA khi:

- SKU không sellable.

- Stock available = 0.

- SKU bị sale lock.

- SKU bị recall.

- Price inactive.

- Trade item chưa ready.

- HSD/trace issue.

- Payment/shipping/order gate chưa đủ nếu landing có checkout.

Landing Page không được hiển thị CTA mua nếu Operational Sellable BLOCK.

## 47.9. Order / Quote Suppression

Order / Quote phải suppress khi:

- Sellable Gate BLOCK.

- Stock Gate BLOCK.

- SKU bị sale lock.

- Batch/stock không đủ.

- QuoteSnapshot không tạo được.

- CustomerConfirmation thiếu.

- Payment/shipping chưa đủ điều kiện.

- Duplicate order risk.

- Recall active trước confirmation.

Order không được tạo nếu thiếu CustomerConfirmation.

Quote không được phát hành nếu thiếu Product/Sellable/Stock/Price runtime hợp lệ.

## 47.10. IVR Suppression

IVR phải suppress khi:

- Order chưa đủ điều kiện xác nhận.

- SKU trong đơn bị sale lock trước xác nhận.

- Order bị hold.

- Payment issue cần Accounting Review.

- Khách không đủ điều kiện gọi theo policy.

- Phone chưa verified.

- Call frequency vượt ngưỡng.

- IVR flow có nguy cơ upsell/sales/payment confirmation.

IVR chỉ được ORDER_CONFIRMATION_ONLY.

## 47.11. Payment / Shipping Suppression

Payment phải suppress khi:

- Order chưa hợp lệ.

- CustomerConfirmation thiếu.

- Order bị stop sale/hold.

- Payment method chưa resolve.

- Accounting Review cần thiết nhưng chưa có.

- PaymentReference bị hiểu nhầm PAID.

Shipping phải suppress khi:

- Order chưa hợp lệ.

- Warehouse/stock chưa allocation được.

- Shipping eligibility chưa resolve.

- COD chưa eligible.

- ETA/tracking chưa có nguồn.

- Sale Lock ảnh hưởng order/stock.

## 47.12. Public Trace Suppression

Public Trace phải suppress hoặc trả safe response khi:

- QR VOID.

- QR FAILED.

- QR không map batch.

- Batch bị recall theo policy cần ẩn/đổi thông điệp.

- Public whitelist chưa cấu hình.

- Có nguy cơ lộ internal data.

- Trace gap nghiêm trọng.

- QR thuộc cấp không public.

Public trace không được lộ:

- supplier private detail;

- costing;

- internal QC defect note;

- MISA data;

- internal IDs;

- personnel data;

- customer/order private data.

## 48. SUPPRESSION EVENT REGISTRY

## 48.1. Mục tiêu Suppression Event

Mỗi lần Field App hoặc Consumer bị suppress phải có suppression event.

Suppression không được xảy ra âm thầm.

## 48.2. Required Suppression Event Fields

Mỗi suppression event phải có:

- suppression_event_id.

- suppression_type.

- trigger_domain.

- trigger_reason.

- affected_sku_id nếu có.

- affected_batch_id nếu có.

- affected_trade_item_id nếu có.

- affected_channel_id nếu có.

- affected_consumer hoặc affected_field_app_action.

- suppression_decision.

- suppression_started_at.

- suppression_ended_at nếu đã gỡ.

- source_resolver_snapshot_id.

- source_guard_decision_id.

- evidence_id nếu có.

- audit_id.

- correlation_id.

## 48.3. Suppression Status

Status | Ý nghĩa

ACTIVE | Đang suppress

RESOLVED | Đã gỡ

EXPIRED | Hết hiệu lực theo thời gian

SUPERSEDED | Bị thay bằng suppression mới

MANUAL_REVIEW | Chờ Owner review

ERROR | Lỗi cần xử lý

## 48.4. Suppression Reopen Rule

Field App hoặc Consumer chỉ được mở lại khi:

- Trigger gốc đã resolved.

- Recovery evidence ACCEPTED nếu liên quan recall/quality.

- Sellable Gate PASS lại.

- Stock Gate PASS lại.

- Field Guard PASS lại nếu là app hiện trường.

- Channel Gate PASS lại nếu là consumer.

- Owner sign-off nếu scope P0.

- Suppression event được close có audit.

- Smoke negative path được chạy lại nếu thuộc release gate.

## 49. RUNTIME DECISION CONTRACT — SELLABLE / STOCK / STOP-SALE / FIELD SUPPRESSION

## 49.1. Mục tiêu Runtime Decision Contract

Runtime Decision Contract giúp các domain đọc cùng một kết quả, tránh mỗi hệ hiểu một kiểu.

Field App không được tự tính Sellable.

Consumer không được tự tính Stock.

Consumer không được tự xác định Stop Sale.

## 49.2. Sellable Runtime Decision

Output tối thiểu:

Field | Ý nghĩa

decision_id | Mã quyết định

sku_id | SKU

batch_scope | Batch cụ thể hoặc all eligible batches

trade_item_scope | Hộp/thùng nếu áp dụng

operational_sellable_status | PASS/BLOCK

stock_status | PASS/BLOCK

recall_status | CLEAR/HOLD/RECALL

sale_lock_status | CLEAR/LOCKED

hsd_status | VALID/EXPIRED/MISSING

trace_status | READY/bị chặn

block_reasons | Danh sách lý do block

valid_until | Hết hiệu lực nếu có

audit_id | Audit

correlation_id | Trace

## 49.3. Stock Runtime Decision

Output tối thiểu:

Field | Ý nghĩa

stock_decision_id | Mã quyết định tồn

sku_id | SKU

warehouse_id | Kho

location_id | Location

batch_id | Batch

trade_item_id | Hộp/thùng nếu có

stock_on_hand | Tồn tổng

stock_reserved | Đã giữ

stock_blocked | Bị block

stock_available | Khả dụng

stock_status | PASS/BLOCK

block_reason | Lý do

audit_id | Audit

correlation_id | Trace

## 49.4. Stop-Sale Runtime Decision

Output tối thiểu:

Field | Ý nghĩa

stop_sale_decision_id | Mã quyết định stop-sale

scope_type | SKU/batch/lot/trade item/channel/program

affected_scope | Phạm vi ảnh hưởng

trigger | Nguyên nhân

recall_case_id | Nếu có

sale_lock_id | Nếu có

suppression_required | Có/không

affected_consumers | Danh sách consumer

affected_field_actions | Danh sách field action bị chặn

decision | STOP/REVIEW/CLEAR

owner_required | Có/không

audit_id | Audit

correlation_id | Trace

## 49.5. Field Suppression Runtime Decision

Output tối thiểu:

Field | Ý nghĩa

field_suppression_decision_id | Mã quyết định

field_event_id | Event từ app

device_id | Thiết bị

actor_id | Người thao tác

role_id | Vai trò

object_type | Object

object_id | ID object

attempted_action | Action

guard_result | PASS/BLOCK/REVIEW

suppression_decision | Decision

reason | Lý do

retry_allowed | Có/không

handoff_required | Có/không

audit_id | Audit

correlation_id | Trace

## 50. SPECIAL GATES FOR CURRENT OPERATIONAL LOCKS

## 50.1. Kho Bến Tre Gate

Kho Bến Tre Gate PASS khi:

- Kho Bến Tre tồn tại trong Warehouse Registry.

- Có location tối thiểu.

- Warehouse receipt ghi đúng warehouse/location.

- Inventory ledger ghi đúng warehouse/location.

- Stock query đọc đúng warehouse/location.

- Không hardcode tên kho.

- Có audit.

BLOCK nếu:

- Kho Bến Tre chỉ là text hardcode.

- Không có warehouse_id.

- Không có location.

- Inventory không theo location.

- Consumer tự nói hàng ở Kho Bến Tre mà không có resolver.

## 50.2. Field App Gate

Field App Gate PASS khi:

- Có device registry.

- Có device identity.

- Có actor/role binding.

- Có permission guard.

- Có offline queue.

- Có idempotency.

- Có mobile event audit.

- Có media object binding.

- Có no-bypass backend guard.

- Có negative test cho duplicate/offline/conflict.

BLOCK nếu:

- Device chưa đăng ký vẫn thao tác được.

- App sync không audit.

- App action không idempotency.

- Media mồ côi vẫn evidence PASS.

- Scan tự confirm.

- Field App mutate database trực tiếp.

- Field App bỏ qua Sale Lock.

## 50.3. Printer waiting Integration Gate

Printer Gate ở trạng thái hiện tại là:

RESERVED / waiting_DEVICE

PASS ở tầng PACK khi:

- Có boundary rõ.

- Có registry chừa ngõ.

- Có print payload model.

- Có log model.

- Có reprint rule.

- Có no-hardcode rule.

- Có adapter boundary.

Không được gọi device production-ready khi chưa có thiết bị/evidence thật.

## 50.4. GTIN/GS1 Reserved Gate

GTIN/GS1 Gate ở trạng thái hiện tại là:

RESERVED / waiting_REGISTRY

PASS ở tầng PACK khi:

- Có Trade Item Registry boundary.

- Hộp/thùng được mô hình hóa là trade item riêng nếu cần.

- GTIN/GS1 chừa ngõ theo từng trade item.

- Không hardcode GTIN.

- Không nhập tay barcode.

- QR không thay barcode thương mại.

Commercialization PASS chỉ được gọi khi GTIN/GS1 thật có evidence.

## 50.5. MISA Mapping Reserved Gate

MISA Mapping Gate ở trạng thái hiện tại là:

RESERVED / waiting_MAPPING

PASS ở tầng PACK khi:

- Mapping boundary rõ.

- Dữ liệu cần mapping đã liệt kê.

- MISA Integration Layer là một ngõ chung.

- Không module nào sync riêng.

- Sync status/retry/error/reconcile đã khóa.

- Không hardcode tài khoản kế toán/mã chi phí.

MISA Production Sync PASS chỉ được gọi khi mapping thật có evidence.

## 50.6. Supplier Portal Reserved Gate

Supplier Portal Gate ở trạng thái hiện tại là:

## CONDITIONAL / RESERVED

PASS ở tầng PACK khi:

- Supplier đủ điều kiện mới được cấp tài khoản.

- Supplier upload ảnh/video/thông tin vùng trồng.

- Supplier evidence không tự PASS.

- Source Verification Owner review.

- Supplier không sửa operational truth.

- Có audit.

Supplier Portal runtime chỉ active khi có supplier registry, account, permission và evidence flow.

## 51. NEGATIVE PATH MATRIX — SELLABLE / STOCK / STOP-SALE / FIELD APP

## 51.1. Negative Path bắt buộc

Case | Input lỗi | Kết quả bắt buộc

NP-SELL-001 | Batch chưa RELEASED | Sellable BLOCK

NP-SELL-002 | Warehouse receipt chưa confirm | Stock BLOCK

NP-SELL-003 | Inventory available = 0 | Sellable BLOCK

NP-SELL-004 | HSD expired | Sellable BLOCK

NP-SELL-005 | QC độ ẩm >4% | QC FAIL, downstream BLOCK

NP-SELL-006 | QC độ ẩm 2%–4% | HOLD/REVIEW, downstream BLOCK

NP-SELL-007 | Sale Lock active | Field + Consumer suppression

NP-SELL-008 | Recall active | Stop Sale / suppression

NP-SELL-009 | Hộp thiếu QR | Trade item BLOCK nếu trace required

NP-SELL-010 | Thùng dùng barcode hộp sai policy | Trade item BLOCK

NP-SELL-011 | Printer tự sinh mã | Printing BLOCK

NP-SELL-012 | Consumer tự bán SKU bị chặn | Consumer FAIL / Incident

NP-SELL-013 | Supplier evidence chưa verified | Source BLOCK

NP-SELL-014 | MISA mapping thiếu | MISA Sync BLOCK

NP-SELL-015 | Kho Bến Tre hardcode | No-hardcode FAIL

NP-FLD-001 | App không có device_id | Field action BLOCK

NP-FLD-002 | Device chưa đăng ký | Field action BLOCK

NP-FLD-003 | App không có actor/role | Field action BLOCK

NP-FLD-004 | Upload ảnh không gắn object | Evidence REJECT

NP-FLD-005 | Offline retry trùng idempotency | Không sinh duplicate

NP-FLD-006 | Scan inbound nhưng chưa confirm | Inventory không tăng

NP-FLD-007 | App nhập tay QR/barcode cấp 2 | BLOCK

NP-FLD-008 | App pass QC độ ẩm >4% | BLOCK

NP-FLD-009 | App release batch khi chưa đủ điều kiện | BLOCK

NP-FLD-010 | App confirm receipt với batch chưa RELEASED | BLOCK

NP-FLD-011 | App thao tác batch đang Sale Lock | BLOCK / REVIEW

NP-FLD-012 | Offline event cũ conflict state mới | REVIEW_REQUIRED

NP-FLD-013 | App version hết hiệu lực | BLOCK / UPDATE_REQUIRED

## 51.2. Negative Path PASS Rule

Negative Path chỉ PASS khi chứng minh được:

- Lỗi bị chặn đúng.

- Field App không bypass.

- Consumer không bypass.

- Có block reason.

- Có audit.

- Có evidence.

- Không phát sinh side effect sai.

- Không tự nâng trạng thái nghiệp vụ.

- Không ghi duplicate.

- Không làm bẩn source-of-truth.

## 52. SELLABLE / STOCK / STOP-SALE / FIELD SUPPRESSION SMOKE

## 52.1. Smoke 01 — Sellable Happy Path

Chuỗi:

- Batch đã RELEASED.

- Warehouse Receipt Confirmed.

- Inventory Ledger ghi nhận.

- Stock Available > 0.

- HSD valid.

- Trace Ready.

- No Recall.

- No Sale Lock.

- Trade item hộp/thùng ready.

- Sellable PASS.

PASS khi Consumer được phép đọc sellable status nhưng vẫn phải qua channel/price/order gate tương ứng.

## 52.2. Smoke 02 — Stock Zero Path

Chuỗi:

- Batch có tồn tổng.

- Reserved/bị chặn/allocated làm stock_available = 0.

- Stock Gate BLOCK.

- Field App không thể confirm action làm tăng sellable.

- AI không báo còn hàng.

- ADS không scale.

- CRM không gửi gợi ý mua.

- Landing Page không CTA mua.

PASS khi Field App và Consumer bị suppress đúng.

## 52.3. Smoke 03 — Recall Stop-Sale Path

Chuỗi:

- Mở recall case.

- Attach affected batch.

- Activate hold.

- Activate sale lock.

- Generate impact snapshot.

- Suppress Field App action liên quan.

- Suppress AI/CRM/ADS/MC AI/Gateway/Landing/Order.

- Attempt quote/order bị chặn.

- Recovery chưa có thì chưa reopen.

PASS khi SKU/batch bị recall không thể bán hoặc đi tiếp sai ở bất kỳ Field App/Consumer nào trong scope.

## 52.4. Smoke 04 — Hộp / Thùng Trade Item Path

Chuỗi:

- Batch ready.

- Packaging cấp 2 hộp.

- In hộp đủ lô/NSX/HSD/barcode/QR.

- Packaging cấp 2 thùng.

- In thùng đủ lô/NSX/HSD/barcode/QR.

- QR map đúng batch.

- GTIN/GS1 lấy từ registry nếu active.

- Không barcode thứ hai trên cùng trade item.

- Trade item sellable PASS nếu đủ stock.

PASS khi hộp và thùng được phân biệt đúng cấp thương mại.

## 52.5. Smoke 05 — QC Moisture Block Path

Chuỗi:

- Batch sau sấy có độ ẩm 4.5%.

- Field App submit QC.

- QC Guard trả FAIL.

- Batch không vào release.

- Warehouse receipt bị chặn.

- Inventory không tăng.

- Sellable BLOCK.

- Field App và Consumer suppression.

PASS khi độ ẩm >4% chặn đúng toàn bộ downstream.

## 52.6. Smoke 06 — Field App Offline Conflict Path

Chuỗi:

- Field App tạo offline event.

- Trong thời gian offline, batch bị Sale Lock.

- App sync event sau khi online.

- Backend phát hiện state conflict.

- Event chuyển REVIEW_REQUIRED hoặc BLOCK.

- Không mutate state sai.

- Có audit.

PASS khi offline event không overwrite truth mới hơn.

## 52.7. Smoke 07 — Scan Does Not Confirm Path

Chuỗi:

- Field App scan inbound batch.

- Chưa bấm confirm receipt.

- Inventory Ledger không thay đổi.

- Stock Available không tăng.

- Sellable không đổi.

- Confirm receipt sau đó mới tạo ledger.

PASS khi scan không tự tăng tồn.

## 53. EVIDENCE REQUIRED FOR PHẦN 3/4

## 53.1. Evidence tối thiểu

## PHẦN 3/4 cần các evidence sau khi triển khai thật:

- Sellable decision evidence.

- Stock available evidence.

- Inventory ledger evidence.

- HSD generation evidence.

- QC moisture evidence.

- Batch release evidence.

- Warehouse receipt evidence.

- Recall stop-sale evidence.

- Sale lock suppression evidence.

- Field App suppression evidence.

- Device identity evidence.

- Offline sync evidence.

- Media object binding evidence.

- AI suppression evidence.

- CRM suppression evidence.

- ADS suppression evidence.

- MC AI suppression evidence.

- Gateway suppression evidence.

- Landing Page suppression evidence.

- Order suppression evidence.

- Trade item hộp/thùng print evidence.

- GTIN/GS1 registry evidence nếu active.

- Kho Bến Tre registry evidence.

- No-hardcode evidence.

## 53.2. Evidence Packet Mapping

Các evidence trên phải được gom vào:

Operational Evidence Packet

Mỗi evidence item phải có:

- evidence_item_id.

- step.

- object_code.

- owner.

- status.

- file/log/reference.

- device_id nếu từ Field App.

- actor_id nếu từ Field App.

- sync_status nếu từ offline.

- audit_id.

- correlation_id.

## 54. PHẦN 3/4 DONE GATE

## PHẦN 3/4 được xem là đạt governance scope khi đã khóa đủ:

- Sellable Gate Control Principles.

- Base Sellable Gate.

- Operational Sellable và Commercial Sellable boundary.

- Sellable Required Inputs.

- Sellable Resolver / Guard.

- Stock Gate Control.

- Stock Availability Model.

- Kho Bến Tre Stock Boundary.

- Trade Item Sellable Control cho hộp/thùng.

- Hộp Sellable Gate.

- Thùng Sellable Gate.

- HSD / Shelf-life Gate 12 tháng.

- QC Quality Gate độ ẩm <2%, 2%–4%, >4%.

- Recall Stop-Sale Gate.

- Hold và Sale Lock tách riêng.

- Field Suppression Control.

- Consumer Suppression Control.

- Suppression Event Registry.

- Runtime Decision Contract.

- Special Gates cho Kho Bến Tre, Field App, Printer, GTIN/GS1, MISA Mapping, Supplier Portal.

- Negative Path Matrix.

- Sellable / Stock / Stop-Sale / Field Suppression Smoke.

- Evidence Required.

- No Field App Bypass rule.

- No Consumer Bypass rule.

- No-hardcode rule.

## 55. PHẦN 3/4 FINAL CONCLUSION

## PHẦN 3/4 khóa lớp Sellable Gate, Stock Gate, Recall Stop-Sale, Field Suppression và Consumer Suppression cho Operational Core.

Từ điểm này trở đi, SKU / batch / hộp / thùng chỉ được phép đi vào bán hàng khi:

Nếu một trong các điều kiện P0 không đạt, hệ thống phải:

- BLOCK sellable.

- SUPPRESS Field App action liên quan.

- SUPPRESS Consumer.

- Ghi suppression event.

- Ghi audit.

- Không cho Field App bypass.

- Không cho AI/CRM/ADS/MC AI/Gateway/Landing/Order bypass.

- Chỉ mở lại khi có evidence, smoke và owner decision nếu thuộc scope P0.

Không có đường tắt từ field action sang sellable.

Không có đường tắt từ scan sang confirm.

Không có đường tắt từ tồn kho sang bán hàng.

Không có đường tắt từ batch sang sellable.

Không có đường tắt từ print sang thương mại hóa.

Không có đường tắt từ CRM/ADS/live demand sang stock.

Không có đường tắt từ recall sang mở bán lại.

PACK-01 PHẦN 3/4 là nền để viết tiếp:

## PHẦN 4/4 — FIELD EVIDENCE / OPERATIONAL DONE GATE / SMOKE / RELEASE HANDOFF / PACK-01 FINAL CONCLUSION

## PHẦN 4/4 — FIELD EVIDENCE / OPERATIONAL DONE GATE / SMOKE / RELEASE HANDOFF / PACK-01 FINAL CONCLUSION

## 56. FIELD EVIDENCE CONTROL PRINCIPLES

## 56.1. Mục tiêu Field Evidence

Field Evidence là bằng chứng hiện trường được ghi nhận trong quá trình vận hành thực tế.

Field Evidence dùng để chứng minh:

- Ai thao tác.

- Thao tác ở đâu.

- Thao tác lúc nào.

- Thao tác trên đối tượng nào.

- Thiết bị nào ghi nhận.

- Hình ảnh/video/file nào đi kèm.

- Dữ liệu có được đồng bộ đúng không.

- Có bị offline conflict không.

- Có audit không.

- Có đủ điều kiện dùng làm evidence PASS không.

Field Evidence không phải dữ liệu trang trí.

Field Evidence là một phần của Product Truth.

Không có Field Evidence đúng chuẩn thì các thao tác hiện trường không đủ điều kiện dùng để gọi PASS trong smoke, handover hoặc release readiness.

## 56.2. Field Evidence Source

Field Evidence có thể phát sinh từ:

- Field Operations Internal App / Mobile PWA.

- Thiết bị quét QR/barcode.

- Máy ảnh/camera trong app.

- Video capture trong app.

- File upload hiện trường.

- Print device callback.

- Device heartbeat.

- Mobile offline event.

- Check-in/check-out.

- Click-confirm.

- QC measurement.

- Warehouse scan.

- Recall/recovery capture.

Field Evidence không được phát sinh dưới dạng file rời không gắn object.

## 56.3. Field Evidence bắt buộc gắn object

Mỗi Field Evidence phải gắn với ít nhất một object nghiệp vụ.

Các object hợp lệ gồm:

- source_origin_id.

- raw_material_receipt_id.

- raw_material_receipt_item_id.

- raw_material_lot_id.

- raw_material_qc_id.

- production_order_id.

- production_profile_id.

- material_issue_id.

- material_receipt_id.

- production_execution_id.

- process_event_id.

- batch_id.

- packaging_job_id.

- print_job_id.

- print_payload_id.

- print_error_id.

- qc_inspection_id.

- batch_release_id.

- warehouse_receipt_id.

- inventory_ledger_id.

- trace_result_id.

- recall_case_id.

- recovery_item_id.

- disposition_id.

- capa_id.

Evidence không gắn object được xem là ORPHAN_EVIDENCE và không được dùng để gọi PASS.

## 56.4. Field Evidence bắt buộc có actor / role / device

Mỗi Field Evidence phải có tối thiểu:

- actor_id.

- actor_name_snapshot nếu cần.

- role_code.

- department_code nếu có.

- device_id.

- device_code.

- device_uuid.

- app_version.

- platform.

- captured_at.

- synced_at nếu có offline sync.

- object_type.

- object_id.

- evidence_type.

- evidence_status.

- audit_id.

- correlation_id.

Nếu thiếu actor, role hoặc device thì evidence không đủ điều kiện dùng làm bằng chứng vận hành.

## 56.5. Field Evidence Status

Status | Ý nghĩa | Được dùng PASS không

CAPTURED_LOCAL | Đã ghi tại thiết bị, chưa sync | Không

SYNC_waiting | Đang chờ đồng bộ | Không

SYNCED | Đã đồng bộ | Chưa

UNDER_REVIEW | Đang review | Không

ACCEPTED | Được chấp nhận | Có

REJECTED | Không đạt | Không

CONFLICT | Có xung đột | Không

VOID | Hủy | Không

SUPERSEDED | Bị thay bằng evidence mới | Không

Chỉ Field Evidence có trạng thái ACCEPTED mới được dùng cho Done Gate.

## 57. OPERATIONAL EVIDENCE PACKET

## 57.1. Mục tiêu Operational Evidence Packet

Operational Evidence Packet là gói bằng chứng tổng hợp cho một production run, một batch hoặc một smoke run.

Mục tiêu là gom bằng chứng theo chuỗi vận hành, tránh để ảnh/video/log/phiếu nằm rời rạc, không trace được.

Mô hình chuẩn:

01 Production Run / 01 Batch / 01 Operational Evidence Packet

## 57.2. Cấu trúc Operational Evidence Packet

Mỗi Operational Evidence Packet phải có:

- evidence_packet_id.

- evidence_packet_code.

- production_order_id.

- production_profile_id nếu có.

- batch_id.

- sku_id.

- formula_version_id.

- warehouse_id nếu đã nhập kho.

- operational_smoke_run_id nếu thuộc smoke.

- packet_status.

- created_by.

- created_at.

- reviewed_by.

- reviewed_at.

- owner_signoff_id nếu dùng release/handover.

- audit_id.

- correlation_id.

## 57.3. Evidence Packet Content

Operational Evidence Packet tối thiểu phải gom các nhóm bằng chứng:

Nhóm evidence | Nội dung bắt buộc

Source Evidence | Source zone, source origin, company/supplier evidence, verification

Raw Intake Evidence | Phiếu nhập nguyên liệu, ảnh/video, số lượng, kết luận

Raw QC Evidence | QC nguyên liệu, lot status, readiness

Recipe Evidence | SKU, công thức, version, BOM snapshot

Material Issue Evidence | Phiếu đề nghị cấp nguyên liệu, chấp thuận nguyên liệu, issue theo lot

Material Receipt Evidence | Xưởng xác nhận nhận nguyên liệu

Production Evidence | Lệnh sản xuất, execution event, output, loss, incident

Personnel Evidence | Check-in/check-out theo mẻ/công đoạn nếu dùng

Packaging L1 Evidence | Đóng gói cấp 1, số lượng, ảnh/video

Packaging L2 Evidence | Đóng gói hộp/thùng, số lượng, ảnh/video

Printing Evidence | Print payload, print log, print error, reprint log nếu có

QC After Drying Evidence | % độ ẩm, cảm quan, ảnh/video/file đo nếu có

Finished QC Evidence | QC thành phẩm, accept/hold/reject

Release Evidence | Batch release record

Warehouse Evidence | Scan inbound, confirm receipt, ảnh/video nhập kho

Inventory Evidence | Inventory ledger, lot balance, stock available

Trace Evidence | QR/barcode -> batch -> mẻ -> lot nguyên liệu

Recall Evidence | Recall negative smoke, hold, sale lock, recovery nếu có

MISA Evidence | Sync status, retry, error, reconcile nếu thuộc scope

No-Hardcode Evidence | Registry/config thay vì hardcode

Field App Evidence | Device, actor, role, offline sync, mobile audit

## 57.4. Evidence Packet PASS Conditions

Operational Evidence Packet chỉ PASS khi:

- Có đầy đủ nhóm evidence theo scope.

- Evidence hiện trường đã sync thành công.

- Media evidence gắn đúng object.

- High-risk action có permission/audit.

- Offline event không conflict.

- Không có orphan evidence.

- Không có rejected evidence P0.

- Không có trace gap P0.

- Không có state transition thiếu audit.

- Owner/QA review chấp nhận.

## 57.5. Evidence Packet BLOCK Conditions

Operational Evidence Packet BLOCK nếu:

- Thiếu Field Evidence cho thao tác hiện trường bắt buộc.

- Ảnh/video không gắn object.

- Evidence chưa sync.

- Evidence conflict.

- Evidence bị reject.

- QC không có bằng chứng.

- Batch release không có audit.

- Warehouse receipt không có confirm evidence.

- Inventory ledger không trace được.

- Recall/sale lock không có negative evidence.

- MISA sync không có status khi thuộc scope.

- Device/actor/role thiếu.

- Evidence bị sửa không audit.

## 58. FIELD APP EVIDENCE RULES

## 58.1. Media Evidence Rule

Ảnh/video/file hiện trường phải có:

- media_id.

- object_type.

- object_id.

- evidence_packet_id.

- media_type.

- storage_ref.

- captured_by.

- captured_at.

- device_id.

- app_version.

- sync_status.

- review_status.

- audit_id.

- correlation_id.

Không cho phép media mồ côi.

Không cho phép media không biết thuộc phiếu/lệnh/batch/lot nào.

## 58.2. Check-in / Check-out Evidence Rule

Check-in/check-out phải có:

- personnel_event_id.

- actor_id.

- role_code.

- production_order_id.

- batch_id hoặc production_execution_id nếu có.

- work_step_code.

- shift_code.

- check_in_at.

- check_out_at.

- total_work_time.

- device_id.

- supervisor_confirm_id nếu cần.

- audit_id.

- correlation_id.

Check-in/check-out không được là ghi chú text.

## 58.3. Scan Evidence Rule

Scan evidence phải có:

- scan_event_id.

- scan_type.

- scan_value.

- expected_object_type.

- resolved_object_id.

- verification_result.

- device_id.

- actor_id.

- scanned_at.

- audit_id.

- correlation_id.

Scan chỉ xác minh.

Scan không tự confirm.

Scan không tự tăng tồn.

Scan không tự pass QC.

Scan không tự release batch.

## 58.4. Click-confirm Evidence Rule

Click-confirm phải có:

- confirmation_event_id.

- action_type.

- object_type.

- object_id.

- actor_id.

- role_code.

- permission_result.

- device_id nếu từ Field App.

- idempotency_key.

- reason nếu required.

- from_state.

- to_state.

- confirmed_at.

- audit_id.

- correlation_id.

High-risk click-confirm thiếu permission hoặc audit thì không hợp lệ.

## 58.5. Offline Evidence Rule

Offline event phải có:

- client_event_id.

- device_id.

- actor_id.

- local_created_at.

- sync_started_at.

- synced_at.

- action_type.

- object_type.

- object_id.

- payload_hash.

- idempotency_key.

- retry_count.

- sync_status.

- conflict_status.

- audit_id khi sync thành công.

Offline event không được overwrite truth mới hơn.

Offline event conflict phải chuyển REVIEW_REQUIRED hoặc BLOCK.

## 59. OPERATIONAL SMOKE STANDARD

## 59.1. Mục tiêu Smoke

Operational Smoke chứng minh chuỗi vận hành chạy đúng xuyên suốt.

Smoke không chỉ test từng màn hình.

Smoke không chỉ test API rời.

Smoke phải chứng minh:

- Dữ liệu đi đúng chuỗi.

- State chuyển đúng.

- Field App capture đúng.

- Backend guard chặn đúng.

- Evidence gắn đúng object.

- Inventory không tăng sai.

- Trace không đứt.

- Recall/Sale Lock chặn flow thật.

- MISA không đi tắt.

- Consumer không bypass.

## 59.2. Smoke Run Structure

Mỗi smoke run phải có:

- operational_smoke_run_id.

- smoke_run_code.

- smoke_name.

- scope.

- production_order_id.

- batch_id.

- sku_id.

- formula_version_id.

- warehouse_id.

- evidence_packet_id.

- started_by.

- started_at.

- completed_at.

- smoke_status.

- positive_path_status.

- negative_path_status.

- STOP_POINTS_list.

- owner_review_status.

- audit_id.

- correlation_id.

## 59.3. Smoke Step Structure

Mỗi smoke step phải có:

- smoke_step_id.

- operational_smoke_run_id.

- step_code.

- step_name.

- domain_code.

- input_object_id.

- expected_result.

- actual_result.

- pass_fail_status.

- evidence_item_id.

- field_event_id nếu phát sinh từ Field App.

- audit_id.

- correlation_id.

## 60. REQUIRED SMOKE MATRIX

## 60.1. OP-SMK-001 — Source -> Intake -> Lot -> QC

Mục tiêu:

Chứng minh nguyên liệu có nguồn, intake hợp lệ, lot được sinh đúng và QC kiểm soát readiness.

Positive path:

- Source hợp lệ.

- Intake được tạo.

- Nguyên liệu chọn từ master.

- Field App chụp evidence.

- Receipt confirmed.

- Lot được sinh.

- QC pass.

- Lot READY_FOR_PRODUCTION.

Negative path:

- Không có source -> intake BLOCK.

- Nguyên liệu nhập tay -> BLOCK.

- QC reject -> lot không issue được.

- Evidence mồ côi -> evidence REJECT.

## 60.2. OP-SMK-002 — SKU -> BOM -> Production Order

Mục tiêu:

Chứng minh chọn SKU tự hiện công thức và sinh hồ sơ sản xuất gốc.

Positive path:

- Chọn SKU.

- Công thức/version tự hiện.

- BOM snapshot được tạo.

- Nhóm sản phẩm và phân loại tự hiện.

- Production order mở đúng.

Negative path:

- SKU thiếu công thức -> BLOCK.

- User chọn tay nguyên liệu -> BLOCK.

- Công thức thiếu version -> BLOCK.

- BOM không snapshot -> BLOCK.

## 60.3. OP-SMK-003 — Material Issue By Lot

Mục tiêu:

Chứng minh chỉ lot đạt điều kiện mới được cấp cho sản xuất.

Positive path:

- Production order có BOM snapshot.

- Lot QC_PASS.

- Lot READY_FOR_PRODUCTION.

- Material issue theo lot.

- Xưởng xác nhận nhận nguyên liệu qua Field App.

Negative path:

- Lot chưa QC_PASS -> BLOCK.

- Lot chưa READY_FOR_PRODUCTION -> BLOCK.

- Lot ngoài BOM -> BLOCK.

- Field App confirm không permission -> BLOCK.

## 60.4. OP-SMK-004 — Execution -> Batch -> Genealogy

Mục tiêu:

Chứng minh batch sinh ra có genealogy về raw material lot.

Positive path:

- Start execution.

- Ghi process event.

- Ghi output/loss.

- Complete execution.

- Batch được tạo.

- Batch truy về material lots.

Negative path:

- Execution không gắn production order -> BLOCK.

- Batch không genealogy -> FAIL.

- Produced/accepted/loss bị trộn -> FAIL.

- Field App mutate batch trực tiếp -> BLOCK.

## 60.5. OP-SMK-005 — Packaging Level 1

Mục tiêu:

Chứng minh đóng gói cấp 1 đúng boundary.

Positive path:

- Packaging L1 gắn batch.

- In cấp 1 chỉ NSX/HSD.

- Field App ghi số lượng.

- Evidence ảnh/video gắn packaging job.

- Complete packaging L1.

Negative path:

- L1 in QR/barcode trái policy -> FAIL.

- Packaging complete tăng inventory -> FAIL.

- Evidence không gắn object -> REJECT.

## 60.6. OP-SMK-006 — Packaging Level 2 Hộp / Thùng + Print

Mục tiêu:

Chứng minh hộp và thùng đều in cấp 2 đúng rule.

Positive path:

- Packaging L2 hộp.

- Hộp in lô/NSX/HSD/barcode/QR.

- Packaging L2 thùng.

- Thùng in lô/NSX/HSD/barcode/QR.

- Print payload do backend sinh.

- Field App scan kiểm tra mã.

- Print log ghi nhận.

- QR map đúng batch.

Negative path:

- Hộp thiếu QR -> BLOCK.

- Thùng thiếu barcode -> BLOCK.

- Field App nhập tay barcode -> BLOCK.

- Printer tự sinh mã -> BLOCK.

- Một trade item có hai barcode -> FAIL.

- QR thay GTIN/GS1 -> FAIL.

## 60.7. OP-SMK-007 — QC Moisture / Finished QC

Mục tiêu:

Chứng minh QC sau sấy và QC thành phẩm hoạt động đúng.

Positive path:

- Field App ghi % độ ẩm.

- Độ ẩm <2%.

- QC pass.

- Finished QC pass.

- Evidence đầy đủ.

Negative path:

- Độ ẩm 2%–4% -> HOLD/REVIEW.

- Độ ẩm >4% -> FAIL.

- Field App cố pass QC >4% -> BLOCK.

- QC_PASS tự release -> FAIL.

## 60.8. OP-SMK-008 — Batch Release

Mục tiêu:

Chứng minh release là action riêng.

Positive path:

- Batch QC_PASS.

- Owner có quyền release.

- Release command chạy.

- Batch RELEASED.

- Transition log ghi nhận.

Negative path:

- QC_PASS tự thành RELEASED -> FAIL.

- Batch QC_HOLD release được -> FAIL.

- Batch QC_REJECT release được -> FAIL.

- Release thiếu audit -> FAIL.

## 60.9. OP-SMK-009 — Warehouse Receipt

Mục tiêu:

Chứng minh chỉ batch RELEASED mới được nhập kho.

Positive path:

- Batch RELEASED.

- Field App scan inbound.

- Warehouse/location hợp lệ.

- Confirm receipt theo permission.

- Receipt confirmed.

Negative path:

- Batch chưa RELEASED -> receipt BLOCK.

- Scan inbound tự tăng inventory -> FAIL.

- Confirm receipt thiếu device/actor/audit -> BLOCK.

- Kho hardcode -> FAIL.

## 60.10. OP-SMK-010 — Inventory Ledger

Mục tiêu:

Chứng minh inventory chỉ tăng sau receipt confirmed.

Positive path:

- Warehouse receipt confirmed.

- Inventory ledger ghi entry.

- Lot balance cập nhật.

- Stock available tính đúng.

Negative path:

- Packaging tăng inventory -> FAIL.

- Printing tăng inventory -> FAIL.

- QC tăng inventory -> FAIL.

- Ledger sửa trực tiếp -> FAIL.

- Stock âm -> FAIL.

## 60.11. OP-SMK-011 — Trace Backward / Forward

Mục tiêu:

Chứng minh truy xuất đủ chiều.

Positive path:

- Scan QR.

- Resolve batch.

- Batch truy về production order.

- Production truy về raw material lots.

- Batch truy xuôi về warehouse/inventory.

- Public trace không lộ dữ liệu nội bộ.

Negative path:

- QR không map batch -> BLOCK.

- Trace chain đứt -> FAIL.

- Public trace lộ internal data -> FAIL.

- Field App tự sửa trace -> BLOCK.

## 60.12. OP-SMK-012 — Recall / Hold / Sale Lock

Mục tiêu:

Chứng minh recall chặn flow thật.

Positive path:

- Mở recall case.

- Attach batch.

- Activate hold.

- Activate sale lock.

- Impact snapshot tạo.

- Field App bị chặn action sai scope.

- Consumer bị suppress.

Negative path:

- Recall dựng chain riêng -> FAIL.

- Sale lock không chặn AI/CRM/ADS/MC AI/Gateway -> FAIL.

- Field App vẫn thao tác batch lock -> FAIL.

- Recovery chưa evidence mà reopen -> FAIL.

## 60.13. OP-SMK-013 — Field App Offline Conflict

Mục tiêu:

Chứng minh offline sync không làm bẩn truth.

Positive path:

- App ghi event offline.

- Sync lại khi online.

- Backend nhận event.

- Idempotency xử lý đúng.

- Audit ghi nhận.

Negative path:

- Offline event trùng -> không duplicate.

- Payload hash đổi cùng idempotency key -> conflict.

- Batch bị recall trước khi sync -> REVIEW_REQUIRED/BLOCK.

- Event cũ overwrite state mới -> FAIL.

## 60.14. OP-SMK-014 — MISA Sync Boundary

Mục tiêu:

Chứng minh MISA chỉ nhận checkpoint đủ điều kiện.

Positive path:

- Operational checkpoint hợp lệ.

- Mapping tồn tại.

- Sync event tạo.

- Status ghi nhận.

- Retry/error/reconcile hoạt động.

Negative path:

- Thiếu mapping -> sync BLOCK.

- Field App sync MISA trực tiếp -> BLOCK.

- Module sync riêng -> FAIL.

- MISA điều khiển operational truth -> FAIL.

## 60.15. OP-SMK-015 — No-Hardcode Runtime Data

Mục tiêu:

Chứng minh dữ liệu runtime đi qua registry/resolver.

Positive path:

- Kho Bến Tre đọc từ Warehouse Registry.

- QC threshold đọc từ config.

- HSD đọc từ Shelf-life Registry.

- GTIN/GS1 đọc từ Trade Item Registry nếu active.

- MISA mapping đọc từ registry.

- Device đọc từ Device Registry.

Negative path:

- Kho hardcode -> FAIL.

- GTIN hardcode -> FAIL.

- HSD hardcode -> FAIL.

- QC threshold hardcode trong UI -> FAIL.

- MISA account hardcode -> FAIL.

## 60.16. OP-SMK-016 — Final E2E Close-out

Mục tiêu:

Chạy toàn chuỗi Operational Core.

Chuỗi bắt buộc:

Source -> Intake -> QC Lot -> Production Order -> Material Issue -> Execution -> Batch -> Packaging L1 -> Packaging L2 Hộp/Thùng -> Print -> QC -> Release -> Warehouse Receipt -> Inventory Ledger -> Trace -> Recall Stop-Sale Negative Test -> MISA Sync Status -> Evidence Packet -> Owner Review

PASS khi:

- Positive path pass.

- Negative path pass.

- Evidence packet đủ.

- Field evidence accepted.

- No-hardcode pass.

- Owner review pass.

- Không có P0 STOP_POINTS.

## 61. OPERATIONAL DONE GATE

## 61.1. Mục tiêu Operational Done Gate

Operational Done Gate xác định PACK-01 đã đủ điều kiện chuyển sang:

- Dev handoff.

- QA handoff.

- Operational rehearsal.

- Evidence review.

- Smoke review.

- Release readiness review.

Operational Done Gate không đồng nghĩa Production Ready.

## 61.2. Done Gate Matrix

Gate Code | Gate Name | PASS khi | BLOCK khi

OP-DG-001 | Governance Completeness Gate | PACK-01 đủ 4 phần, rule rõ | Còn rule P0 mơ hồ

OP-DG-002 | Field App Gate | App có device, offline, audit, evidence binding | App bypass backend hoặc media mồ côi

OP-DG-003 | Source / Intake Gate | Source -> Intake -> Lot -> QC chạy đúng | Lot không trace source hoặc QC thiếu

OP-DG-004 | Recipe / BOM Gate | SKU tự hiện công thức/version/BOM | Vẫn chọn tay nguyên liệu

OP-DG-005 | Production Gate | Order -> Issue -> Execution -> Batch đúng | Batch không genealogy

OP-DG-006 | Packaging / Printing Gate | L1/L2 hộp/thùng in đúng, log đúng | Printing tạo truth hoặc in sai cấp

OP-DG-007 | QC / Release Gate | QC_PASS tách RELEASED | QC_PASS tự release

OP-DG-008 | Warehouse / Inventory Gate | RELEASED mới receipt, ledger append-only | Inventory tăng sai boundary

OP-DG-009 | Traceability Gate | QR/batch/lot trace đủ | Trace chain đứt

OP-DG-010 | Recall / Sale Lock Gate | Recall dùng trace, lock chặn flow thật | Recall không chặn bán

OP-DG-011 | MISA Boundary Gate | Một integration layer, status/retry/reconcile | Module sync riêng

OP-DG-012 | Evidence Gate | Evidence Packet đủ, ACCEPTED | Evidence thiếu/chưa accepted

OP-DG-013 | Smoke Gate | E2E smoke pass, negative path pass | Chỉ pass test rời

OP-DG-014 | No-Hardcode Gate | Dữ liệu runtime qua registry/resolver | Runtime data hardcode

OP-DG-015 | Handover Gate | Dev/QA/Ops/MISA handoff đủ | Đội triển khai còn phải tự đoán

OP-DG-016 | Owner Sign-off Gate | Owner sign-off đúng scope | Review chưa ký hoặc không trace

## 61.3. Done Gate Decision States

State | Ý nghĩa

NOT_STARTED | Chưa đánh giá

UNDER_REVIEW | Đang rà

waiting_IMPLEMENTATION | Chưa triển khai

waiting_EVIDENCE | Thiếu evidence

waiting_SMOKE | Thiếu smoke

waiting_OWNER_SIGNOFF | Thiếu owner sign-off

bị chặn | Có STOP_POINTS

Governance Complete | Đủ tài liệu governance

READY_FOR_HANDOVER_CANDIDATE | Có thể xét handover

Ready For Handover | Đủ điều kiện bàn giao

Production Ready Candidate | Ứng viên production ready

REJECTED | Không đạt

SUPERSEDED | Bị thay thế

## 61.4. Default Status

Sau khi tài liệu hoàn tất, trạng thái mặc định là:

Hạng mục | Default State

PACK-01 Documentation | Governance Complete

Implementation | waiting_IMPLEMENTATION

Field App Evidence | waiting_EVIDENCE

Evidence Packet | waiting_EVIDENCE

Smoke Run | waiting_SMOKE

Owner Sign-off | waiting_OWNER_SIGNOFF

Ready For Handover | waiting_HANDOVER_REVIEW

Production Ready | KHÔNG

Release Approved | KHÔNG

Go-live Approved | KHÔNG

## 62. RELEASE HANDOFF STANDARD

## 62.1. Mục tiêu Release Handoff

Release Handoff dùng để chuyển PACK-01 sang các đội triển khai, kiểm thử và vận hành thử.

Handoff không có nghĩa là production release.

Handoff chỉ xác nhận các đội đã nhận đủ:

- Scope.

- Owner.

- Boundary.

- Source-of-truth.

- Domain Registry.

- Field App rules.

- Phase order.

- Test matrix.

- Smoke matrix.

- Evidence requirements.

- STOP_POINTS rules.

## 62.2. Handoff Recipients

PACK-01 phải handoff cho:

- Dev Lead.

- Backend Team.

- Frontend/Admin Team.

- Field App / Mobile PWA Team.

- QA Team.

- Operational Owner.

- Factory/Production Owner.

- Warehouse Owner.

- QC Owner.

- Traceability/Recall Owner.

- Accounting/MISA Owner.

- Security/Permission Owner.

- Release Owner.

- Business Owner.

## 62.3. Dev Handoff Package

Dev Handoff Package phải gồm:

- PACK-01 bản sạch.

- Domain Registry.

- Field Operations App Domain.

- Device / Offline / Idempotency rules.

- Media Evidence rules.

- Phase order.

- DB owner theo module.

- API/worker/admin owner theo module.

- Route family.

- State/action boundary.

- High-risk action list.

- No-hardcode rules.

- Registry/resolver list.

- Done Gate matrix.

- Smoke matrix.

- Negative path matrix.

- Evidence packet structure.

- Known placeholder registry.

- Known STOP_POINTS register.

Không giao dev bằng đoạn code rời.

Không để dev tự đoán owner/boundary/flow.

## 62.4. QA Handoff Package

QA Handoff Package phải gồm:

- Test matrix.

- Smoke matrix OP-SMK-001 -> OP-SMK-016.

- Negative path matrix.

- Field App negative path.

- Evidence acceptance rule.

- Device identity checklist.

- Offline sync checklist.

- Media object binding checklist.

- Trace ID checklist.

- Audit checklist.

- No-hardcode checklist.

- Recall / Sale Lock consumer suppression checklist.

- MISA sync boundary checklist.

- Trade item hộp/thùng checklist.

- QC moisture checklist.

- HSD checklist.

QA không được chỉ test màn hình.

QA phải test state transition, owner boundary, side effect, audit, field evidence và smoke xuyên chuỗi.

## 62.5. Operational Handoff Package

Operational Handoff Package phải gồm:

- Chuỗi phiếu vận hành.

- Quy tắc auto-fill / click chọn / nhập tay.

- Field App workflow.

- Check-in/check-out.

- Scan rule.

- Click-confirm rule.

- Role ký xác nhận.

- Quy tắc đơn vị theo công đoạn.

- Quy tắc evidence ảnh/video/file.

- Quy tắc QC độ ẩm.

- Quy tắc in cấp 1 / cấp 2.

- Quy tắc xử lý lỗi in / reprint.

- Quy tắc warehouse receipt.

- Quy tắc trace.

- Quy tắc recall / sale lock.

- Quy tắc phối hợp với kế toán/MISA.

## 62.6. Field App Handoff Package

Field App Handoff Package phải gồm:

- Use case hiện trường.

- Device header chuẩn.

- Offline queue rules.

- Sync rules.

- Idempotency rules.

- Media capture rules.

- Object binding rules.

- Scan rules.

- Click-confirm rules.

- High-risk action rules.

- Sale Lock warning rules.

- Error/retry/conflict rules.

- Mobile event audit rules.

- Device registry rules.

- App version control rules.

## 62.7. MISA / Accounting Handoff Package

MISA / Accounting Handoff Package phải gồm:

- Mapping cần chuẩn bị.

- Checkpoint sync.

- Sync status.

- Retry/error/reconcile rule.

- Phiếu kế toán xuất nguyên liệu.

- Nhập kho nguyên liệu.

- Nhập kho thành phẩm.

- Giá trị xuất nguyên liệu.

- Hao hụt.

- Nhân công nếu sync.

- Cost center nếu dùng.

- Không để MISA điều khiển nghiệp vụ nhà máy.

## 63. PLACEHOLDER REGISTRY CONTROL

## 63.1. Mục tiêu Placeholder Registry

Các dữ liệu thật chưa có phải được quản trị bằng placeholder có kiểm soát.

Placeholder không được public.

Placeholder không được hiểu là active runtime.

Placeholder không được dùng để gọi PASS.

## 63.2. Placeholder Registry bắt buộc

Registry | Trạng thái | Rule

Warehouse Registry | Kho Bến Tre là kho thật ban đầu | Không hardcode

Location Registry | Chờ chi tiết location | Phải cấu hình trước receipt thật

Field App Registry | Required | Không bypass backend

Device Registry | Required | Device chưa đăng ký bị chặn

Printer Registry | waiting_DEVICE | Chừa ngõ kết nối

Print Template Registry | waiting_TEMPLATE | Chừa ngõ cấu hình

Trade Item Registry | Hộp/thùng cần registry | Không suy luận từ tên

GTIN/GS1 Registry | RESERVED | Không hardcode / không nhập tay

MISA Mapping Registry | RESERVED | Không sync chính thức nếu thiếu mapping

QC Threshold Registry | <2% PASS, 2%–4% HOLD, >4% FAIL | Không hardcode trong UI

Shelf-life Registry | 12 tháng | Có thể theo SKU/nhóm SKU

Supplier Portal Registry | CONDITIONAL | Supplier đủ điều kiện mới được cấp quyền

Personnel Registry | waiting_FACTORY_OPERATION | Không hardcode tên người

Role / Permission Registry | REQUIRED | High-risk action phải có permission

Evidence Registry | REQUIRED | Chỉ ACCEPTED mới PASS

## 63.3. Placeholder BLOCK Rule

Placeholder bị xem là BLOCK nếu:

- Được dùng như dữ liệu thật.

- Được public ra khách hàng.

- Được dùng để sync MISA chính thức.

- Được dùng để gọi Production Ready.

- Được hardcode trong code/UI/template.

- Không có owner.

- Không có trạng thái.

- Không có kế hoạch thay bằng dữ liệu thật.

- Field App dùng placeholder để thao tác production thật.

- Device placeholder được dùng như device thật.

## 64. OPERATIONAL RELEASE READINESS REVIEW

## 64.1. Mục tiêu Release Readiness Review

Operational Release Readiness Review là bước rà cuối trước khi PACK-01 được đưa vào hàng chờ xét Production Ready Candidate.

Review này xác định:

- Domain nào đã triển khai.

- Domain nào chỉ governance.

- Domain nào waiting registry.

- Domain nào waiting device.

- Domain nào waiting Field App smoke.

- Domain nào waiting MISA mapping.

- Domain nào waiting GTIN/GS1.

- Domain nào waiting evidence.

- Domain nào waiting smoke.

- Domain nào waiting owner sign-off.

- Domain nào bị STOP_POINTS.

## 64.2. Release Readiness Checklist

Nhóm | Điều kiện

Governance | PACK-01 đủ 4 phần, không còn rule mơ hồ

Master Data | SKU/BOM/version/config/trace map rõ

Field App | Device, offline, idempotency, media binding, audit rõ

Source | Source zone/source origin/supplier/company source rõ

Production | Lệnh sản xuất tự hiện công thức, issue lot-based

Packaging | L1/L2 rõ, hộp/thùng in đúng

Printing | Hệ thống sinh payload, máy in chỉ nhận payload

QC | Độ ẩm và QC thành phẩm có threshold/state

Release | QC_PASS tách RELEASED

Warehouse | Kho Bến Tre registry, receipt confirmed

Inventory | Ledger append-only, stock available resolve được

Trace | Backward/forward/internal/public trace rõ

Recall | Hold/sale lock/impact/recovery/CAPA rõ

MISA | Một integration layer, mapping/status/retry/reconcile

Evidence | Operational Evidence Packet

Smoke | E2E smoke + negative smoke

Security | Permission/audit cho high-risk action

Handoff | Dev/QA/Ops/Field App/MISA nhận đủ gói

Owner | Owner sign-off theo scope

## 64.3. Production Ready Candidate Rule

PACK-01 chỉ được đưa vào trạng thái Production Ready Candidate khi:

- Implementation scope hoàn tất.

- Field App evidence ACCEPTED nếu có thao tác hiện trường.

- Evidence Packet ACCEPTED.

- E2E smoke PASS.

- Negative smoke PASS.

- No-hardcode PASS.

- Device / offline / idempotency PASS.

- Security / permission / audit PASS.

- Trace / Recall PASS.

- MISA boundary PASS nếu thuộc scope.

- Owner sign-off đủ.

- Không còn STOP_POINTS P0.

Nếu thiếu một điều kiện P0, trạng thái vẫn là waiting_EVIDENCE hoặc bị chặn.

## 65. OPERATIONAL ROLLBACK / RECOVERY HANDOFF

## 65.1. Mục tiêu Rollback / Recovery

Rollback / Recovery dùng để dừng, cô lập và xử lý lỗi vận hành mà không phá source-of-truth.

Rollback không phải xóa dữ liệu.

Rollback là:

- Dừng hành động sai.

- Cô lập scope lỗi.

- Ghi incident.

- Ghi evidence.

- Chạy recovery.

- Owner review.

- Re-run smoke nếu cần.

- Mở lại có điều kiện.

## 65.2. Rollback Trigger

Rollback/recovery bắt buộc khi:

- Nguyên liệu không trace được source.

- Lot reject/hold vẫn issue được.

- Lệnh sản xuất chọn tay nguyên liệu.

- BOM snapshot sai version.

- Batch không genealogy về lot.

- Field App sync duplicate state.

- Field App bypass backend validation.

- Field App media mồ côi được dùng PASS.

- In cấp 2 sai batch/QR/barcode.

- Máy in tự sinh mã nghiệp vụ.

- QC_PASS tự release.

- Batch chưa release vẫn nhập kho.

- Inventory tăng sai boundary.

- Trace chain đứt.

- Recall không chặn flow thật.

- Sale Lock không suppress Field App/Consumer.

- MISA sync thiếu mapping.

- High-risk action không audit.

- Hardcode runtime data.

- Evidence giả hoặc không trace được.

## 65.3. Recovery Requirements

Mỗi recovery phải có:

- recovery_case_id.

- trigger.

- affected_scope.

- owner.

- containment action.

- correction action.

- evidence.

- audit.

- re-test result.

- smoke rerun nếu P0.

- owner sign-off.

- closure status.

Không đóng recovery nếu chưa xác nhận không còn side effect sai.

## 66. FINAL OPERATIONAL DONE GATE

## 66.1. PACK-01 Governance Done Gate

PACK-01 đạt GOVERNANCE COMPLETE khi:

- PHẦN 1/4 khóa Operational Core Principles / Field Operations App / Owner Boundary / Product Truth Model.

- PHẦN 2/4 khóa Domain Registry.

- PHẦN 3/4 khóa Sellable Gate / Stock Gate / Recall Stop-Sale / Field Suppression / Consumer Suppression.

- PHẦN 4/4 khóa Field Evidence / Operational Done Gate / Smoke / Release Handoff.

- Không còn rule P0 mơ hồ.

- Không còn owner mơ hồ.

- Không còn boundary chồng chéo.

- Không còn Field App bypass.

- Không còn Consumer tự suy luận truth vận hành.

- Không còn đường tắt từ production/printing/QC/scan/MISA sang inventory/sellable.

## 66.2. PACK-01 Ready For Handover Gate

PACK-01 chỉ được gọi READY FOR HANDOVER khi có đủ:

- Dev Handoff Package.

- QA Handoff Package.

- Operational Handoff Package.

- Field App Handoff Package.

- MISA/Accounting Handoff Package.

- Test matrix.

- Smoke matrix.

- Evidence packet structure.

- Known placeholder registry.

- Known STOP_POINTS register.

- Owner review.

- Handover sign-off.

READY FOR HANDOVER không đồng nghĩa Production Ready.

## 66.3. PACK-01 Production Ready Candidate Gate

PACK-01 chỉ được gọi Production Ready Candidate khi:

- Implementation có evidence.

- Field App smoke pass.

- E2E smoke pass.

- Negative path pass.

- Evidence ACCEPTED.

- Owner sign-off đầy đủ.

- Security/permission/audit pass.

- Trace/Recall/MISA smoke pass đúng scope.

- No-hardcode evidence pass.

- Không còn P0 STOP_POINTS.

- Completion Gate liên quan đủ điều kiện xét.

## 66.4. PACK-01 Production Ready STOP_POINTS

PACK-01 bắt buộc BLOCK nếu còn một trong các lỗi:

- Lệnh sản xuất vẫn chọn tay nguyên liệu.

- Công thức không tự hiện đầy đủ.

- BOM không snapshot.

- Lot chưa QC_PASS + READY vẫn issue được.

- Batch không truy được raw material lot.

- Field App không có device identity.

- Field App không có offline/idempotency.

- Field App media mồ côi.

- Field App bypass backend.

- Field App scan làm thay confirm.

- QC_PASS tự release.

- Batch chưa RELEASED vẫn nhập kho.

- Inventory tăng trước warehouse receipt confirmed.

- Ledger không append-only.

- In cấp 2 thiếu lô/NSX/HSD/barcode/QR.

- Hộp/thùng không phân biệt trade item.

- GTIN/GS1 hardcode hoặc nhập tay sai rule.

- QR thay barcode thương mại.

- Máy in tự sinh business truth.

- Reprint không reason/approval/audit.

- Trace chain đứt.

- Public trace lộ dữ liệu nội bộ.

- Recall dựng chain riêng.

- Hold và Sale Lock trộn nhau.

- Sale Lock không suppress Field App.

- Sale Lock không suppress Consumer.

- MISA sync không có mapping/status/retry/reconcile.

- MISA điều khiển nghiệp vụ nhà máy.

- High-risk action không permission/audit.

- Evidence thiếu hoặc chưa accepted.

- Smoke chỉ chạy test rời.

- Owner sign-off thiếu.

- Runtime data bị hardcode.

## 67. PACK-01 FINAL STATUS MATRIX

Hạng mục | Trạng thái sau khi hoàn tất tài liệu

PACK-01 Documentation | Governance Complete

Operational Implementation | waiting_IMPLEMENTATION

Field App Implementation | waiting_IMPLEMENTATION

Evidence Packet | waiting_EVIDENCE

Field Evidence | waiting_EVIDENCE

Smoke Run | waiting_SMOKE

Owner Sign-off | waiting_OWNER_SIGNOFF

Ready For Handover | waiting_HANDOVER_REVIEW

Production Ready | KHÔNG

Release Approved | KHÔNG

Go-live Approved | KHÔNG

## 68. PHẦN 4/4 DONE GATE

## PHẦN 4/4 được xem là đạt governance scope khi đã khóa đủ:

- Field Evidence Control Principles.

- Operational Evidence Packet.

- Evidence Packet PASS/BLOCK Conditions.

- Field App Evidence Rules.

- Media Evidence Rule.

- Check-in / Check-out Evidence Rule.

- Scan Evidence Rule.

- Click-confirm Evidence Rule.

- Offline Evidence Rule.

- Operational Smoke Standard.

- Smoke Run Structure.

- Smoke Step Structure.

- Required Smoke Matrix OP-SMK-001 -> OP-SMK-016.

- Field App Offline Conflict Smoke.

- Scan Does Not Confirm Smoke.

- Operational Done Gate.

- Done Gate Matrix.

- Release Handoff Standard.

- Dev Handoff Package.

- QA Handoff Package.

- Operational Handoff Package.

- Field App Handoff Package.

- MISA / Accounting Handoff Package.

- Placeholder Registry Control.

- Release Readiness Review.

- Rollback / Recovery Handoff.

- Final Operational Done Gate.

- Production Ready STOP_POINTS.

- Final Status Matrix.

## 69. PACK-01 FINAL CONCLUSION

PACK-01 khóa hoàn chỉnh lớp Operational Core cho hệ thống Ginsengfood.

Operational Core được xác định là owner của sự thật vận hành gồm:

- Source Origin.

- Raw Material Intake.

- Raw Material QC.

- Raw Material Lot.

- Recipe / BOM Snapshot.

- Production Order.

- Material Issue.

- Material Receipt.

- Production Execution.

- Batch.

- Field Operations Internal App.

- Personnel Check-in / Check-out.

- Packaging Level 1.

- Packaging Level 2 hộp/thùng.

- Print Payload.

- QC sau sấy.

- QC thành phẩm.

- Batch Release.

- Warehouse Receipt.

- Inventory Ledger.

- Traceability.

- Recall.

- Sale Lock / Stop Sale.

- MISA Integration Boundary.

- Evidence / Smoke.

PACK-01 khóa rõ các nguyên tắc không được vi phạm:

- Không chọn tay nguyên liệu trong sản xuất thường.

- Không sửa đè version công thức.

- Không dùng dòng gom nhóm mơ hồ ở downstream.

- Không issue nguyên liệu ngoài BOM snapshot.

- Không dùng lot chưa QC_PASS + READY_FOR_PRODUCTION.

- Không để Field App owner business truth.

- Không để Field App bypass backend validation.

- Không để media evidence mồ côi.

- Không để scan tự confirm.

- Không để offline retry sinh duplicate.

- Không để QC_PASS tự thành RELEASED.

- Không nhập kho batch chưa RELEASED.

- Không để inventory tăng trước warehouse receipt confirmed.

- Không để printing tạo inventory/QC/release.

- Không để máy in tự sinh business truth.

- Không để QR thay barcode thương mại.

- Không để cùng trade item có hai barcode thương mại.

- Không để recall dựng chain riêng.

- Không để sale lock không chặn Field App.

- Không để sale lock không chặn Consumer.

- Không để MISA điều khiển nghiệp vụ nhà máy.

- Không để AI/CRM/ADS/MC AI/Gateway tự suy luận product truth.

- Không gọi PASS khi chưa có evidence/smoke/owner sign-off.

PACK-01 cũng khóa các dữ liệu vận hành ban đầu:

- In cấp 2 áp dụng cho hộp và thùng.

- Hộp/thùng in lô, NSX, HSD, mã vạch, QR.

- Kho thật ban đầu là Kho Bến Tre nhưng phải qua Warehouse Registry.

- Máy in/template thật chừa ngõ kết nối.

- GTIN/GS1 hộp/thùng chừa ngõ tích hợp theo Trade Item Registry.

- Mapping MISA chừa ngõ kết nối.

- QC độ ẩm: dưới 2% PASS, 2%–4% HOLD/REVIEW, trên 4% FAIL.

- HSD mặc định 12 tháng theo SKU hoặc nhóm SKU.

- Vùng trồng Sâm là nguồn công ty.

- Supplier Portal chừa ngõ cho nhà cung cấp đủ điều kiện nhập ảnh/video/thông tin vùng trồng.

- Evidence Pack và Smoke Log dùng phương án tinh gọn theo Operational Evidence Packet và operational_smoke_run_id.

PACK-01 hoàn thành ở tầng governance, chưa đồng nghĩa Production Ready.

## OPERATIONAL CORE / PRODUCTION / WAREHOUSE / INVENTORY / TRACEABILITY / RECALL / SALE LOCK

IMPLEMENTATION_STATUS: waiting_IMPLEMENTATION

FIELD_APP_STATUS: waiting_IMPLEMENTATION

FIELD_EVIDENCE_STATUS: waiting_EVIDENCE

EVIDENCE_STATUS: waiting_EVIDENCE

SMOKE_STATUS: waiting_SMOKE

## 70. ĐỀ XUẤT TIẾP THEO

Bước tiếp theo nên chuyển sang:
