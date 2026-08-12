# 01-P2 - THIẾT KẾ KỸ THUẬT VẬN HÀNH LÕI

Trạng thái: `OPERATIONAL_TECHNICAL_DESIGN_SOURCE_ABSORBED`  
Mode: `TECHNICAL DESIGN ONLY`  
Mục tiêu: thiết kế entity/service/event cho Operational Core bằng tiếng Việt, đủ để bỏ 5 file nguồn cũ.

## 1. Aggregate chính

| Aggregate | Nguồn | Quyền sở hữu |
| --- | --- | --- |
| RawReceipt | Phiếu nhập nguyên liệu | Kho/QC/Kế toán vật tư |
| RawLot | Raw receipt + QC | Kho/QC |
| ProductionDossier | Lệnh sản xuất | Nhà máy |
| ProductionBOMSnapshot | Phase 1 Formula G1 | Product/Factory |
| MaterialRequest | Dossier + BOM snapshot | Xưởng/Kho |
| MaterialIssue | Approved request + lot readiness | Kho |
| WorkshopReceipt | Material issue | Xưởng |
| Batch | Dossier + workshop receipt | Sản xuất/QC |
| PackagingRun | Batch + packaging profile | Đóng gói |
| FinishedGoodsReceipt | Released batch | Kho thành phẩm |
| TraceProjection | Batch/lot/warehouse | Traceability owner |
| AccountingHandoff | Ledger issue/receipt/write-off | Kế toán/MISA owner |

## 2. Command high-risk

Mọi command sau phải có RBAC, idempotency, correlation, audit, evidence:

1. Submit raw receipt.
2. Approve incoming QC.
3. Mark raw lot ready.
4. Create production order.
5. Approve production dossier.
6. Approve material request.
7. Issue material.
8. Receive material at workshop.
9. Start/complete batch stage.
10. Approve finished QC.
11. Release batch.
12. Receive finished goods.
13. Create recall/sale lock.
14. Create accounting handoff.

## 3. Inheritance rule

Hồ sơ sản xuất gốc là nguồn duy nhất để các phiếu sau tự kế thừa:

`production_dossier_id`, `production_order_id`, SKU, product name, formula_id, formula_version, planned quantity, batch id, actor, stage, material lines, lot refs, packaging profile, trace ids.

Không phiếu nào được tự tạo rời nếu không link về dossier hoặc raw receipt tương ứng.

## 4. Data contract chi tiết

| Contract | Field bắt buộc | Source |
| --- | --- | --- |
| `RawReceiptHeader` | receipt_id, receipt_code, supplier_id, warehouse_id, received_at, receiver, qc_actor, accounting_actor, evidence_refs | Phiếu đánh giá và nhập kho nguyên liệu |
| `RawReceiptLine` | material_id, material_name_snapshot, material_group, delivered_qty, accepted_qty, rejected_qty, uom, unit_cost, damage_value, qc_result | Phiếu nhập + QC đầu vào |
| `RawLot` | raw_lot_id, lot_code, material_id, receipt_line_id, status, usable_qty, blocked_qty, expiry_date, readiness_state | Raw receipt + QC |
| `ProductionDossier` | dossier_id, production_order_id, sku_id, product_name_snapshot, formula_id, formula_version, planned_qty, shift, factory_manager, status | Lệnh sản xuất |
| `ProductionBOMSnapshot` | snapshot_id, dossier_id, formula_version_id, material_id, required_qty, uom, ratio_to_anchor, source_formula_ref, locked_at | Formula G1 Phase 1 |
| `MaterialRequest` | request_id, dossier_id, snapshot_id, requested_by, requested_at, request_status, evidence_refs | Phiếu đề nghị cấp nguyên liệu |
| `MaterialApproval` | approval_id, request_id, approved_raw_lot_id, approved_qty, approver_roles, approval_result, reason | Phiếu chấp thuận nguyên liệu |
| `MaterialIssue` | issue_id, approval_id, warehouse_id, raw_lot_id, issued_qty, inventory_ledger_id, accounting_value, idempotency_key | Xuất kho nguyên liệu |
| `WorkshopReceipt` | workshop_receipt_id, issue_id, received_qty, variance_qty, variance_reason, received_by, received_at | Nhận xưởng |
| `Batch` | batch_id, dossier_id, batch_code, process_status, qc_status, release_status, yield_qty, rejected_qty | Theo dõi mẻ |
| `PackagingRun` | packaging_run_id, batch_id, level, package_material_id, planned_pack_qty, actual_pack_qty, print_payload_hash | Đóng gói cấp 1/2 |
| `FinishedGoodsReceipt` | fg_receipt_id, batch_id, warehouse_id, released_qty, receipt_qty, fg_ledger_id, received_at | Lệnh nhập kho thành phẩm |
| `TraceProjection` | trace_id, public_batch_code, sku_id, batch_id, raw_lot_refs, release_state, sale_lock_state, public_visibility | Trace/QR |
| `AccountingHandoff` | handoff_id, source_doc_type, source_doc_id, debit_account, credit_account, amount, misa_state, blocked_reason | Phiếu kế toán/MISA |

## 5. Phân ranh click chọn, tự hiện, nhập tay

| Ngữ cảnh | Click chọn | Tự hiện từ hệ thống | Được nhập tay |
| --- | --- | --- | --- |
| Nhập nguyên liệu | Supplier, material master, kho, vị trí | material_group, UOM chuẩn, threshold policy | qty giao, qty đạt, giá nhập, evidence, ghi chú |
| Lệnh sản xuất | SKU, ca, nhà máy, planned qty | product name, formula G1, BOM snapshot, packaging profile | ghi chú kế hoạch, reason nếu deviation |
| Phiếu đề nghị cấp nguyên liệu | dossier/order | toàn bộ material lines, required qty, UOM, formula refs | ghi chú đề nghị |
| Phiếu chấp thuận nguyên liệu | raw lot ready | available qty, QC/readiness state, ledger value estimate | approved qty, reason nếu không cấp đủ |
| Material issue | approved request | lot, qty approved, accounting value, ledger entry | issue note, evidence |
| Workshop receipt | material issue | item/lot/qty issued | received qty, variance reason |
| Batch/QC | dossier/batch | SKU, formula, process route, expected yield | actual yield, QC result, hold reason |
| Packaging/print | batch + packaging profile | label/QR payload, package material, batch code | actual pack qty, reprint reason |
| Warehouse receipt | released batch | released qty, FG SKU/batch | receipt qty nếu lệch phải có reason |

## 6. Command và event bắt buộc

| Command | Event thành công | Event block |
| --- | --- | --- |
| `SubmitRawReceipt` | `RawReceiptSubmitted` | `RawReceiptRejectedByValidation` |
| `ApproveIncomingQc` | `IncomingQcApproved` | `IncomingQcFailed` |
| `MarkRawLotReady` | `RawLotMarkedReady` | `RawLotReadinessBlocked` |
| `CreateProductionOrder` | `ProductionOrderCreated` | `ProductionOrderBlocked` |
| `ApproveProductionDossier` | `ProductionDossierApproved` | `ProductionDossierApprovalBlocked` |
| `GenerateMaterialRequest` | `MaterialRequestGenerated` | `MaterialRequestBlocked` |
| `ApproveMaterialIssue` | `MaterialIssueApproved` | `MaterialIssueApprovalBlocked` |
| `IssueMaterial` | `MaterialIssued` | `MaterialIssueBlocked` |
| `ReceiveAtWorkshop` | `WorkshopMaterialReceived` | `WorkshopReceiptBlocked` |
| `CompleteBatchStage` | `BatchStageCompleted` | `BatchStageBlocked` |
| `ApproveFinishedQc` | `FinishedQcApproved` | `FinishedQcBlocked` |
| `ReleaseBatch` | `BatchReleased` | `BatchReleaseBlocked` |
| `ReceiveFinishedGoods` | `FinishedGoodsReceived` | `FinishedGoodsReceiptBlocked` |
| `CreateRecallOrSaleLock` | `SaleLockActivated` | `SaleLockRejectedByValidation` |

## 7. Cross-cutting không được thiếu

1. Mọi command write phải có `idempotency_key`.
2. Mọi state change phải append audit log.
3. Mọi phiếu có xác nhận phải lưu role, actor, timestamp, decision và note.
4. Evidence submitted chưa phải evidence accepted.
5. Outbox/event phải chứa `correlation_id`, `causation_id`, `tenant/factory context` nếu runtime có multi-site.
6. API đọc public trace phải dùng whitelist projection, không đọc trực tiếp internal trace.
7. MISA sync state mặc định `NOT_CONFIGURED` hoặc `BLOCKED_PENDING_OWNER_INPUT`, không tự sync.
---

## PHỤ LỤC NGUỒN ĐÃ NHẬP - PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN.md

> Phụ lục này giữ nội dung nguồn cũ để không mất bảng, số liệu, field hoặc rule khi bỏ file nguồn riêng. Phần rewrite chuẩn hóa ở đầu file là nguồn chính; phụ lục dùng để trace và đối chiếu số liệu.

## 8. Quy tắc tự sinh phiếu vận hành

# AUTO GENERATED FORM RULES

## 1. QUY TẮC TOÀN BỘ PHIẾU TỰ SINH - GINSENGFOOD

## 0. MỞ ĐẦU

### 0.1. Mục đích

Tài liệu này khóa nguyên tắc:

tạo hồ sơ sản xuất gốc

tự sinh chuỗi phiếu vận hành

tự sử dụng làm nền dữ liệu giữa các phiếu

phân ranh rõ nơi nào được click chọn, nơi nào bắt buộc tự hiện

chỉ cho người dùng đánh giá, ghi chú, nhập số liệu thực tế và xác nhận

### 0.2. Phạm vi

Áp dụng cho chuỗi:

Phiếu kế hoạch sản xuất

Phiếu đánh giá và nhập kho nguyên liệu đầu vào

Lệnh sản xuất

Phiếu đề nghị cấp nguyên liệu cho sản xuất

Phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất

Phiếu check-in / check-out nhân sự theo lệnh sản xuất / mẻ / công đoạn

Phiếu theo dõi sơ chế

Phiếu kiểm chất lượng sau sấy thăng hoa

Phiếu theo dõi đóng gói cấp 1

Phiếu theo dõi đóng gói cấp 2

Phiếu QC thành phẩm

Lệnh nhập kho thành phẩm

Phiếu kế toán xuất nguyên liệu cho sản xuất

## 1. HỒ SƠ SẢN XUẤT GỐC

### 1.1. Nguyên tắc

Ngay khi mở Lệnh sản xuất, hệ thống phải sinh:

Mã hồ sơ sản xuất gốc

Mã lệnh sản xuất

SKU

Tên sản phẩm

Mã công thức

Version công thức

Ca sản xuất

Bộ phận sản xuất

Quản lý nhà máy

Số lượng kế hoạch

### 1.2. Vai trò

Hồ sơ sản xuất gốc là nguồn dữ liệu duy nhất để toàn bộ phiếu sau tự sử dụng làm nền.

### 1.3. Cấm

Không cho từng công đoạn tự tạo phiếu rời không liên kết với hồ sơ sản xuất gốc.

## 2. PHÂN RANH CỨNG GIỮA “CLICK CHỌN” VÀ “TỰ HIỆN”

### 2.1. Ranh giới 1 - Phiếu nhập nguyên liệu đầu vào

#### 2.1.1. Vai trò

Đây là nơi tiếp nhận nguyên liệu đầu vào từ bên ngoài vào kho.

#### 2.1.2. Quy tắc cứng

Tại phiếu này, nguyên liệu được click chọn từ danh mục master.

#### 2.1.3. Các trường được click chọn

Nhà cung cấp

Nguyên liệu

Nhóm nguyên liệu

Đơn vị tính

Người giao hàng

Nhân sự tiếp nhận

Kho nhập

Vị trí kho

#### 2.1.4. Các trường phải nhập

Số lượng giao thực tế

Quy cách

Kết quả đánh giá

Lý do không đạt

Đề xuất xử lý

Đơn giá nhập

Ghi chú

#### 2.1.5. Kết luận

Phiếu nhập nguyên liệu đầu vào là nơi click chọn nguyên liệu từ danh mục master.

### 2.2. Ranh giới 2 - Lệnh sản xuất và phiếu cấp nguyên liệu cho sản xuất

#### 2.2.1. Vai trò

Đây là nơi thực thi công thức sản xuất.

#### 2.2.2. Quy tắc cứng

Tại lệnh sản xuất và phiếu cấp nguyên liệu cho sản xuất, người dùng không được tự chọn nguyên liệu.

#### 2.2.3. Hệ thống phải tự hiện

Khi chọn:

SKU

Mã công thức

Version công thức

thì hệ thống phải tự hiện:

Mã nguyên liệu

Tên nguyên liệu

Nhóm nguyên liệu

Hàm lượng

Đơn vị tính

Số lượng cần cấp theo công thức

#### 2.2.4. Cấm

Không cho người dùng ở bước sản xuất thường:

tự thêm nguyên liệu

tự xóa nguyên liệu

tự đổi nguyên liệu

tự thay đổi hàm lượng chuẩn trong phiếu cấp phát thường

#### 2.2.5. Ngoại lệ duy nhất

Nếu thực tế cần thay đổi nguyên liệu hoặc hàm lượng, phải đi qua:

chỉnh version công thức hoặc

phiếu thay đổi công thức / chấp thuận thay thế nguyên liệu riêng

#### 2.2.6. Kết luận

Lệnh sản xuất và phiếu cấp nguyên liệu cho sản xuất là nơi nguyên liệu phải tự hiện theo công thức, không được tự chọn.

### 2.3. Ranh giới tự hiện công thức sản xuất

#### 2.3.1. Quy tắc cứng

Khi chọn SKU, mã công thức và version công thức trong lệnh sản xuất, hệ thống phải tự động hiển thị đầy đủ danh sách:

dược liệu

nguyên liệu

hàm lượng

đơn vị tính

#### 2.3.2. Ý nghĩa vận hành

Người vận hành không được nhìn công thức ở dạng nhóm mơ hồ. Hệ thống phải hiện đúng từng nguyên liệu cụ thể để:

sinh phiếu cấp phát

trừ kho

theo dõi lô nguyên liệu

tính giá trị xuất kho

tính giá thành mẻ

truy xuất và thu hồi

#### 2.3.3. Cấm

Không được để:

“nền chung”

“nước hầm chung”

“nêm chung”

ở dạng dòng gom nhóm trong lệnh sản xuất vận hành.

Tất cả phải bung thành:

từng nguyên liệu

từng hàm lượng

từng đơn vị

#### 2.3.4. Kết luận

Công thức sản xuất phải tự hiện đầy đủ và chi tiết tới từng nguyên liệu; không cho chọn lại nguyên liệu ở bước sản xuất thường.

## 3. DỮ LIỆU NÀO TỰ HIỆN, DỮ LIỆU NÀO CHỌN, DỮ LIỆU NÀO NHẬP

### 3.1. Tự sinh / tự hiện

Mã hồ sơ sản xuất gốc

Mã lệnh sản xuất

Tên sản phẩm

Mã công thức

Version công thức

Nhóm sản phẩm

Phân loại Vegan / Mặn

Danh sách nguyên liệu của công thức

Hàm lượng của công thức

Đơn vị tính của công thức

### 3.2. Click chọn từ danh mục

SKU

Nhà cung cấp

Nguyên liệu đầu vào

Nhân sự

Bộ phận

Kho

Vị trí kho

Ca sản xuất

Trạng thái đánh giá

### 3.3. Phải nhập

Số lượng kế hoạch

Số lượng giao thực tế

Số lượng thực nhập

Số lượng thực xuất

Số liệu đo thực tế

% độ ẩm

Ghi chú

Lý do không đạt

Đề xuất hướng xử lý

Đơn giá nhập

thông tin kế toán cần nhập tay nếu chưa tự kéo được

## 4. NGUYÊN TẮC TỰ SINH PHIẾU

### 4.1. Từ phiếu kế hoạch sản xuất

Cho phép sinh:

Lệnh sản xuất

### 4.2. Từ lệnh sản xuất

Hệ thống tự sinh nháp:

Phiếu đề nghị cấp nguyên liệu cho sản xuất

Phiếu check-in / check-out nhân sự

Phiếu theo dõi sơ chế

Phiếu kiểm chất lượng sau sấy thăng hoa

Phiếu đóng gói cấp 1

Phiếu đóng gói cấp 2

Phiếu QC thành phẩm

Lệnh nhập kho thành phẩm

### 4.3. Từ phiếu nhập nguyên liệu đầu vào

Khi nguyên liệu đạt và nhập kho thành công, hệ thống cho phép:

sử dụng lô nguyên liệu đó cho phiếu cấp phát sản xuất

gắn lô nguyên liệu vào phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất

### 4.4. Từ phiếu đề nghị cấp nguyên liệu

Hệ thống sinh:

Phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất

Phiếu kế toán xuất nguyên liệu cho sản xuất

## 5. NGUYÊN TẮC sử dụng làm nền GIỮA CÁC PHIẾU

### 5.1. Phiếu sau phải tự kéo dữ liệu từ phiếu trước

Ví dụ:

Lệnh sản xuất kéo từ kế hoạch

Phiếu đề nghị cấp nguyên liệu kéo từ công thức SKU

Phiếu chấp thuận kéo từ phiếu đề nghị cấp

Phiếu sơ chế kéo từ phiếu chấp thuận

Phiếu sấy kéo từ sơ chế

Phiếu đóng gói kéo từ phiếu sấy

Phiếu QC thành phẩm kéo từ đóng gói

Lệnh nhập kho kéo từ QC thành phẩm

### 5.2. Không nhập lại dữ liệu nền

Các trường sau không được nhập lại:

SKU

Tên sản phẩm

Công thức

Version

Danh sách nguyên liệu

Hàm lượng chuẩn

Đơn vị chuẩn

Nhân sự chính

Ca sản xuất

Bộ phận

Kho / vị trí kho

Lô nguyên liệu đã duyệt

### 5.3. Người dùng chỉ cập nhật phần vận hành

Ở mỗi phiếu sau, người dùng chỉ xử lý:

đánh giá

ghi chú

số liệu thực tế

ảnh / video

xác nhận

## 6. KHỐI XÁC NHẬN THEO TỪNG CÁ NHÂN

### 6.1. Bắt buộc

Mỗi phiếu phải có:

Vai trò

Họ tên

Bộ phận

Trạng thái xác nhận

Thời gian xác nhận

Ghi chú

### 6.2. Trạng thái xác nhận

Chưa xác nhận

Đã xác nhận

Từ chối xác nhận

### 6.3. Quy tắc chuyển bước

Không đủ xác nhận bắt buộc thì phiếu không được chuyển bước.

## 7. QUY TẮC ĐƠN VỊ THEO TỪNG PHIẾU

### 7.1. Phiếu nhập nguyên liệu đầu vào

kg

lít

ml

### 7.2. Phiếu cấp nguyên liệu cho sản xuất

kg

lít

ml

### 7.3. Phiếu theo dõi sơ chế

khay

kg

lít

### 7.4. Phiếu kiểm chất lượng sau sấy thăng hoa

khay

% độ ẩm

### 7.5. Phiếu đóng gói cấp 1

gói

lọ

hũ

### 7.6. Phiếu đóng gói cấp 2

hộp

thùng

lọ

hũ

### 7.7. Lệnh nhập kho thành phẩm

hộp

thùng

lọ

hũ

## 8. QUY TẮC BẰNG CHỨNG

### 8.1. Phiếu bắt buộc có ảnh / video

Phiếu nhập nguyên liệu đầu vào

Phiếu theo dõi sơ chế

Phiếu kiểm chất lượng sau sấy thăng hoa

Phiếu đóng gói cấp 1

Phiếu đóng gói cấp 2

Lệnh nhập kho thành phẩm

### 8.2. Dữ liệu bằng chứng

ảnh

video

file đính kèm

biên bản liên quan

## 9. PASS / FAIL GATE

### 9.1. Đạt khi

nguyên liệu đầu vào được click chọn đúng ở phiếu nhập

nguyên liệu cho sản xuất tự hiện đúng theo công thức

công thức tự hiện chi tiết tới từng nguyên liệu

phiếu sau tự sử dụng làm nền dữ liệu

người dùng không phải lập lại phiếu từ đầu

đủ xác nhận theo từng cá nhân

đúng đơn vị theo từng công đoạn

### 9.2. Chưa đạt khi

lệnh sản xuất còn cho chọn tay nguyên liệu

phiếu cấp phát còn cho sửa tay danh sách nguyên liệu

công thức vẫn hiện ở dạng nhóm mơ hồ

phải nhập lại dữ liệu nền

thiếu khối xác nhận

sai đơn vị vận hành

thiếu % độ ẩm ở phiếu sau sấy

## 9. Quy tắc in mã, kế toán và MISA trong vận hành lõi

**2\. QUY TẮC IN MÃ SẢN PHẨM 2 GIAI ĐOẠN - GINSENGFOOD**

**0\. MỞ ĐẦU**

**0.1. Mục đích**

Khóa nguyên tắc in mã theo 2 cấp đóng gói:

- cấp 1
- cấp 2

**0.2. Nguyên tắc cốt lõi**

**Hệ thống Ginsengfood sinh dữ liệu in. Máy in chỉ nhận payload để in.**

Máy in không được tự sinh:

- mã sản xuất
- mã lô
- ngày sản xuất
- hạn dùng
- barcode
- QR

**1\. QUY TẮC SINH MÃ TRUNG TÂM**

**1.1. Hệ thống phải sinh**

- batch / lô sản xuất
- ngày sản xuất
- hạn dùng
- barcode data
- QR token / QR payload
- print payload theo cấp đóng gói

**1.2. Máy in chỉ làm**

- nhận payload
- in
- trả trạng thái:
  - đã nhận
  - đã in
  - lỗi in
  - dừng line
  - reprint callback

**1.3. Cấm**

Không cho máy in tự sinh mã nghiệp vụ làm source of truth.

**2\. QUY TẮC SINH DỮ LIỆU IN THEO THỜI GIAN THỰC**

**2.1. Điều kiện để hệ thống tự sinh dữ liệu in**

Khi hệ thống đã biết:

- SKU đang sản xuất
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- Mẻ sản xuất
- Công đoạn đóng gói
- Cấp đóng gói
- Template in đang áp dụng

thì hệ thống phải **tự sinh dữ liệu in theo thời gian thực**.

**2.2. Ý nghĩa "tự sinh thời gian thực"**

"Tự sinh thời gian thực" nghĩa là:

- hệ thống chủ động tạo dữ liệu in ngay tại thời điểm line đang chạy
- không để người vận hành nhập tay mã
- không để máy in tự nghĩ ra mã nghiệp vụ

**2.3. Người vận hành chỉ làm**

- xác nhận line đang chạy lệnh nào
- xác nhận cấp đóng gói nào
- chọn máy in
- bấm bắt đầu in
- theo dõi lỗi / reprint theo quyền

**3\. GIAI ĐOẠN IN CẤP 1**

**3.1. Đơn vị áp dụng**

- gói
- chai
- lọ
- hũ

**3.2. Nội dung in cấp 1**

Chỉ in:

- ngày sản xuất
- hạn dùng

**3.3. Mục tiêu**

- tối giản
- nhanh
- ít lỗi
- không làm nặng line đóng gói cấp 1

**4\. GIAI ĐOẠN IN CẤP 2**

**4.1. Đơn vị áp dụng**

- hộp

**4.2. Nội dung in cấp 2**

Phải in:

- lô sản xuất
- ngày sản xuất
- hạn dùng
- mã vạch
- mã QR

**4.3. Quy tắc sinh dữ liệu in cấp 2**

Khi line cấp 2 đang chạy, hệ thống phải:

- tự xác định SKU
- tự xác định batch/lô
- tự xác định MFG/EXP
- tự sinh barcode
- tự sinh QR
- tự cấp số thứ tự in nếu policy cần
- tự gửi payload xuống máy in

**4.4. Cấm**

Không cho người vận hành nhập tay:

- mã lô
- barcode
- QR
- ngày sản xuất
- hạn dùng  
   trừ trường hợp override được phê duyệt riêng.

**5\. LIÊN KẾT GIỮA CÔNG THỨC, LỆNH SẢN XUẤT VÀ IN MÃ**

**5.1. Dữ liệu in không tách rời công thức**

Dữ liệu in phải sinh từ:

- SKU
- công thức đang áp dụng
- batch/lô thật
- hồ sơ sản xuất gốc
- công đoạn đóng gói thực tế

**5.2. Không có bước chọn lại nguyên liệu để in**

Người vận hành không được chọn lại nguyên liệu để sinh dữ liệu in.  
Dữ liệu in chỉ bám vào:

- lệnh đang chạy
- batch đang chạy
- sản phẩm đang đóng gói

**5.3. Kết luận**

**Công thức tự hiện đúng thì dữ liệu in mới đúng.**  
Nếu công thức không bị chọn tay lại ở bước sản xuất, hệ thống mới có thể sinh mã in đúng, sạch và truy được.

**6\. QUY TẮC TEMPLATE IN**

**6.1. Template cấp 1**

Mỗi SKU phải biết:

- cấp 1 dùng đơn vị gì
- template cấp 1 là gì
- chỉ in MFG/HSD

**6.2. Template cấp 2**

Mỗi SKU phải biết:

- cấp 2 dùng đơn vị gì
- template cấp 2 là gì
- in lô/MFG/HSD/barcode/QR

**6.3. Mapping theo SKU**

Mỗi SKU phải có bảng mapping:

| **SKU** | **Đơn vị cấp 1** | **Nội dung in cấp 1** | **Đơn vị cấp 2** | **Nội dung in cấp 2** |
| ------- | ---------------- | --------------------- | ---------------- | --------------------- |

**7\. REPRINT**

**7.1. Cấp 1**

Reprint có kiểm soát, mức kiểm soát nhẹ hơn.

**7.2. Cấp 2**

Reprint phải bị kiểm soát chặt vì liên quan:

- batch
- barcode
- QR
- traceability
- recall

**7.3. Bắt buộc**

- có lý do reprint
- có người phê duyệt
- có log reprint
- không cho máy in tự ý reprint ngoài hệ thống

**8\. LOG IN BẮT BUỘC**

**8.1. Log cấp 1**

- mã print job
- SKU
- đơn vị in
- MFG/HSD
- thời gian gửi lệnh
- trạng thái in

**8.2. Log cấp 2**

- mã print job
- SKU
- batch/lô
- MFG/HSD
- barcode
- QR
- thời gian gửi lệnh
- trạng thái in
- reprint nếu có

**9\. PASS / FAIL GATE**

**9.1. Đạt khi**

- cấp 1 chỉ in MFG/HSD
- cấp 2 in đủ lô/MFG/HSD/barcode/QR
- hệ thống sinh toàn bộ dữ liệu in theo thời gian thực
- máy in chỉ in và trả trạng thái
- có log đầy đủ
- reprint bị kiểm soát
- dữ liệu in bám đúng batch/lô và lệnh sản xuất thật

**9.2. Chưa đạt khi**

- máy in tự sinh mã nghiệp vụ
- không có log in
- không có log reprint
- barcode/QR không gắn với batch thật
- không phân biệt cấp 1 và cấp 2
- người vận hành phải nhập tay mã in cấp 2

**3\. QUY TẮC NỐI MISA, KỸ THUẬT VÀ PHÂN RANH DỮ LIỆU - GINSENGFOOD**

**0\. MỞ ĐẦU**

**0.1. Mục đích**

Khóa phân ranh rõ giữa:

- hệ vận hành Ginsengfood
- lớp tính giá / chi phí nội bộ
- MISA kế toán chính thức

**0.2. Nguyên tắc cốt lõi**

**Hệ Ginsengfood tự tính vận hành và giá thành nội bộ. MISA hạch toán chính thức.**

**1\. HỆ GINSENGFOOD TỰ TÍNH GÌ**

**1.1. Vận hành kho và sản xuất**

- nhập nguyên liệu
- xuất nguyên liệu cho sản xuất
- lô nguyên liệu
- mẻ sản xuất
- hao hụt
- nhập kho thành phẩm

**1.2. Chi phí nội bộ**

- đơn giá nhập nguyên liệu
- đơn giá bình quân gia quyền
- giá trị xuất nguyên liệu cho mẻ
- hao hụt / hư hỏng
- chi phí nhân công vận hành từ check-in / check-out
- giá thành nội bộ theo mẻ / SKU

**1.3. Kinh doanh nội bộ**

- doanh thu
- chiết khấu
- hoa hồng
- đối soát đơn hàng

**2\. DỮ LIỆU NÀO LÀ KẾT QUẢ "TỰ HIỆN THEO CÔNG THỨC" VÀ ẢNH HƯỞNG KẾ TOÁN**

**2.1. Dữ liệu tự hiện theo công thức**

Các dữ liệu sau trong sản xuất là dữ liệu hệ thống tự hiện:

- danh sách nguyên liệu
- hàm lượng
- đơn vị
- số lượng cần cấp
- version công thức áp dụng

**2.2. Ý nghĩa kế toán**

Do nguyên liệu sản xuất là dữ liệu tự hiện theo công thức, nên hệ thống mới tính đúng được:

- giá trị xuất kho
- hao hụt
- chi phí mẻ
- giá thành nội bộ
- dữ liệu đồng bộ sang MISA

**2.3. Cấm**

Nếu để người dùng tự chọn lại nguyên liệu ở bước sản xuất, dữ liệu giá thành và dữ liệu kế toán sẽ sai.

**3\. MISA HẠCH TOÁN GÌ**

**3.1. Kế toán chính thức**

- hạch toán nhập kho
- hạch toán xuất kho
- công nợ nhà cung cấp
- doanh thu
- chiết khấu kế toán
- chi phí
- báo cáo tài chính
- báo cáo kế toán chính thức

**3.2. Cấm**

Không để MISA điều khiển:

- lệnh sản xuất
- mẻ sản xuất
- công thức
- traceability
- recall
- print logic
- check-in vận hành

**4\. MISA INTEGRATION LAYER**

**4.1. Phải có lớp riêng**

Lớp này phải:

- nhận dữ liệu từ toàn hệ
- map mã nội bộ sang mã MISA
- kiểm tra điều kiện đồng bộ
- gửi sang MISA
- lưu trạng thái đồng bộ

**4.2. Trạng thái đồng bộ**

- Chưa đồng bộ
- Đã gửi MISA
- Đồng bộ thành công
- Đồng bộ lỗi
- Chờ kế toán duyệt

**5\. DỮ LIỆU NÀO ĐỒNG BỘ SANG MISA**

**5.1. Nhập kho nguyên liệu**

Đồng bộ:

- phiếu nhập nguyên liệu đã duyệt
- số lượng thực nhập
- đơn giá nhập
- thành tiền
- hư hỏng loại bỏ

**5.2. Xuất nguyên liệu cho sản xuất**

Đồng bộ:

- phiếu kế toán xuất nguyên liệu cho sản xuất
- số lượng xuất
- đơn giá bình quân
- thành tiền xuất
- hao hụt nếu có

**5.3. Nhập kho thành phẩm**

Đồng bộ:

- lệnh nhập kho thành phẩm
- số lượng nhập
- giá trị thành phẩm nếu đã tính

**5.4. Doanh thu**

Đồng bộ:

- dữ liệu bán hàng
- doanh thu
- chiết khấu
- trả hàng nếu có
- công nợ

**5.5. Hoa hồng / chiết khấu**

Đồng bộ khi có nhu cầu hạch toán chính thức:

- hoa hồng
- chiết khấu cộng tác viên
- chiết khấu Diamond
- khoản phải trả liên quan

**6\. PHIẾU NÀO PHẢI CÓ NGÕ KẾ TOÁN**

**6.1. Phiếu nhập nguyên liệu đầu vào**

Phải có:

- đơn giá nhập
- thành tiền nhập
- giá trị hư hỏng loại bỏ
- đơn giá bình quân sau nhập

**6.2. Phiếu chấp thuận nguyên liệu đưa vào sản xuất**

Bản vận hành không cần hiện giá cho xưởng, nhưng hệ thống phải sinh:

- phiếu kế toán xuất nguyên liệu cho sản xuất
- đơn giá bình quân
- thành tiền xuất
- hao hụt

**6.3. Lệnh nhập kho thành phẩm**

Phải mở ngõ để nối:

- số lượng nhập
- giá trị thành phẩm
- đối tượng tập hợp chi phí nếu cần

**7\. NHÂN SỰ VÀ CHI PHÍ NHÂN CÔNG**

**7.1. Check-in / check-out bắt buộc**

Nhân sự tham gia mẻ phải:

- check-in
- check-out  
   theo:
- lệnh sản xuất
- mẻ sản xuất
- công đoạn

**7.2. Dữ liệu phải giữ**

- mã nhân sự
- họ tên
- bộ phận
- vai trò
- thời gian vào
- thời gian ra
- tổng giờ làm
- đơn giá công
- chi phí nhân công phân bổ

**7.3. Phân ranh**

- hệ Ginsengfood tự tính chi phí nhân công vận hành
- chỉ đồng bộ sang MISA khi cần hạch toán chính thức

**8\. BẢNG MAPPING BẮT BUỘC**

**8.1. Mapping đối tượng**

- kho
- nguyên liệu
- thành phẩm / SKU
- nhà cung cấp
- khách hàng
- nhân viên / bộ phận nếu cần
- tài khoản kế toán
- mã tập hợp chi phí
- mã doanh thu / chiết khấu / hoa hồng

**8.2. Quy tắc**

Không có mapping thì không được đồng bộ chính thức sang MISA.

**9\. PASS / FAIL GATE**

**9.1. Đạt khi**

- hệ mình tự tính được chi phí nội bộ
- nguyên liệu cho sản xuất là dữ liệu tự hiện theo công thức
- MISA nhận được dữ liệu hạch toán chính thức
- có mapping rõ
- có trạng thái đồng bộ rõ
- không lẫn logic nhà máy với logic kế toán

**9.2. Chưa đạt khi**

- hệ mình không tính được giá thành nội bộ
- nguyên liệu sản xuất vẫn là dữ liệu người dùng tự chọn
- MISA phải điều khiển nghiệp vụ nhà máy
- không có mapping
- không có trạng thái sync
- dữ liệu vận hành và kế toán tách rời, không nối được

**10\. KẾT LUẬN**

Ba khối trên hiện đã được viết lại **liền mạch và khóa cứng đầy đủ**, gồm cả mục:

- **Ranh giới tự hiện công thức sản xuất**
- **Quy tắc sinh mã in theo thời gian thực**
- **Phân ranh dữ liệu với MISA**

Khối này làm nền thống nhất ráp thành bản sạch cuối cho bộ:

**SẢN XUẤT - NHẬP KHO - TRUY VẾT - THU HỒI CÓ LIÊN KẾT MISA**  
để giao dev theo nhịp copy-dán / test / chạy thử.

---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 01-P2-1-TECHNICAL-DESIGN-ONLY.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P2.1 - TECHNICAL DESIGN ONLY

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-01 Operational Core.
- TECH-03 Operational Core technical design.
- PACK-03 Demand / MRP / Procurement boundary.
- PACK-04 MISA Accounting Handoff boundary.

## Noi Dung Rewrite

## 01-P2-1-TECHNICAL-DESIGN-ONLY

## TÀI LIỆU THIẾT KẾ KỸ THUẬT PHASE 2 - OPERATIONAL CORE

Global Gate Status: bị chặn
Completion Status: WAITING IMPLEMENTATION EVIDENCE
Evidence Status: NOT ACCEPTED
Production Readiness: KHONG
Release Readiness: KHONG
Go-live Decision: KHONG
Default Agent Mode: TECHNICAL DESIGN ONLY

## PHẦN 1/4 - PHASE MARKER / PURPOSE / SOURCE-OF-TRUTH / SCOPE

## 1. PHASE MARKER

Tài liệu này thuộc nhóm:

## PHASE-02-P2-1-TECHNICAL-DESIGN-ONLY

Đây là file thiết kế kỹ thuật điều phối cho toàn bộ Phase 2 - Operational Core.

Tài liệu này đứng sau:

## 00-P2-ANALYSIS-ONLY.docx

Tài liệu này đứng trước các file triển khai giới hạn:

## 02-P2-2A-SOURCE-RAW-LOT-QC-READINESS.docx

## 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP.docx

## 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER.docx

## 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY.docx

## 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK.docx

## 07-P2-2F-SMOKE-EVIDENCE-REPORT.docx

Tài liệu này không phải file code.

Tài liệu này không cho phép migration.

Tài liệu này không cho phép seed.

Tài liệu này không cho phép dev/Codex/Copilot sửa code ngay.

Tài liệu này chỉ dùng để chuyển kết quả phân tích của 00-P2-ANALYSIS-ONLY thành:

Workstream Matrix.

Operational Form / Command Registry.

Field Behavior Matrix.

Data Inheritance Matrix.

Dependency Matrix.

Technical Boundary Matrix.

Implementation Sequence.

Smoke Plan.

Evidence Plan.

Stop Condition Matrix.

Owner Confirm Matrix.

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## 01-P2-1-TECHNICAL-DESIGN-ONLY

Phiên bản

Phase

## PHASE 2 - OPERATIONAL CORE

Loại tài liệu

Technical Design Only

Nguồn chính

README Phase 2 V1.1 + P2 Analysis

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

Cho phép code?

Không

Cho phép migration?

Không

Cho phép seed?

Không

Cho phép sửa UI/API/service?

Không

Cho phép Agent tự suy luận nghiệp vụ?

Không

Mode mặc định

## TECHNICAL DESIGN ONLY

Global Gate

bị chặn

Production Readiness

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để làm lớp thiết kế kỹ thuật trung gian giữa:

Analysis Only -> Limited Implementation

Trong Phase 2, nếu đi thẳng từ README hoặc Analysis sang code thì rất dễ sai vì Operational Core có nhiều chuỗi nghiệp vụ phụ thuộc nhau:

Raw lot QC_PASS chưa được phép issue nếu chưa READY_FOR_PRODUCTION.

Production Order phải snapshot formula/version/ingredient/UOM.

Production Order phải sinh hồ sơ sản xuất gốc.

Hồ sơ sản xuất gốc là nguồn dữ liệu duy nhất cho chuỗi phiếu sau.

Material Request không được cho chọn tay nguyên liệu.

Material Issue là điểm giảm tồn nguyên liệu chính.

Material Receipt / Workshop Receipt không được giảm tồn lần hai.

Batch QC_PASS chưa phải RELEASED.

Warehouse chỉ được nhận batch đã RELEASED.

Print cấp 1/cấp 2 phải theo payload do Ginsengfood sinh.

Máy in không được tự sinh mã.

MISA không được điều khiển vận hành.

Disposal/write-off không được xóa tay tồn kho.

Sale Lock phải thắng Commerce/AI/Gateway/Ads/Live/CRM/IVR.

Mục tiêu của tài liệu này là khóa thiết kế kỹ thuật ở mức đủ rõ để các file P2.2A-P2.2F viết tiếp không bị lệch.

Tài liệu này không thay thế file implementation chi tiết.

## 4. SOURCE-OF-TRUTH - THỨ BẬC NGUỒN SỰ THẬT

Tầng

Nguồn

Vai trò

Được dùng như thế nào

Không được làm

Tầng 0

MASTER Governance

Governance cao nhất

Giữ nguyên tắc no-bypass, no-parallel-source, no-PASS-without-evidence

Không ghi đè bằng thiết kế cục bộ

Tầng 1

README Phase 2 V1.1

Điều phối Phase 2

Xác định scope, file order, boundary

Không thay file chi tiết

Tầng 2

## 00-P2-ANALYSIS-ONLY

Phân tích hiện trạng

Là input cho design

Không tự chuyển thành code

Tầng 3

P2.1 Technical Design này

Thiết kế workstream và boundary

Là nguồn điều phối kỹ thuật cho P2.2A-P2.2F

Không tự cho phép implementation

Tầng 4

nguồn tổng hợp

Nguồn phụ trợ nghiệp vụ

Dùng để kiểm tra chuỗi phiếu, dựa trên, MISA, print, disposal

Không code trực tiếp

Tầng 5

Phase 1 clean docs

Product/SKU/Recipe/Formula source

Cung cấp SKU, formula, version, ingredient, UOM

Không tự sửa Phase 1 trong Phase 2

Tầng 6

Code hiện tại

Hiện trạng kỹ thuật

Dùng để gap analysis

Không là source-of-truth nếu conflict với tài liệu khóa

## 5. SCOPE IN - PHẠM VI BAO GỒM

Tài liệu này bao gồm thiết kế điều phối cho:

Workstream Phase 2.

Chuỗi phiếu/lệnh vận hành.

Tự sinh phiếu.

dựa trên dữ liệu giữa phiếu.

Click chọn / tự hiện / nhập tay.

Production Order.

Production Dossier.

Formula Snapshot.

Material Request.

Material Approval.

Material Issue.

Material Receipt / Workshop Receipt.

Raw Inventory Ledger.

Batch.

Batch Process.

Batch QC.

Batch Release.

Warehouse Receipt.

Finished Goods Ledger.

Packaging / đóng gói cấp 1 / cấp 2.

Print payload cấp 1 / cấp 2.

Reprint control.

Traceability.

## QR.

Public Trace.

Recall / Hold / Sale Lock.

MISA boundary.

Sâm Savigin Company Source / Strategic Reserve boundary.

Disposal / Write-off.

Actor / Permission / Audit / Idempotency / Evidence.

Smoke / Evidence Plan.

Report 15 mục.

Stop Conditions.

## 6. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM

Tài liệu này không bao gồm:

Không viết code.

Không tạo migration.

Không tạo seed.

Không sửa API.

Không sửa DTO.

Không sửa service.

Không sửa repository.

Không sửa UI.

Không sửa worker.

Không triển khai Production Order thật.

Không triển khai Material Issue thật.

Không triển khai Warehouse Receipt thật.

Không triển khai Print Runtime thật.

Không triển khai MISA Integration thật.

Không triển khai Commerce Runtime.

Không quyết định Sellable.

Không tạo QuoteSnapshot.

Không tạo Order/Payment.

Không triển khai AI Advisor.

Không triển khai Facebook Gateway.

Không triển khai Ads/Live/CRM/IVR.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 7. NGUYÊN TẮC THIẾT KẾ KỸ THUẬT

Mã

Nguyên tắc

Diễn giải

## TD-01

Thiết kế trước, code sau

Không nhảy từ analysis sang implementation

## TD-02

Một nghiệp vụ có một nguồn sự thật

Không tạo source-of-truth song song

## TD-03

Phiếu sau dựa trên phiếu trước

Không bắt nhập lại dữ liệu đã có

## TD-04

Production Dossier là root

Chuỗi sau sản xuất lấy dữ liệu từ hồ sơ gốc

## TD-05

Material Issue là điểm debit chính

Không double debit ở receipt

## TD-06

Ledger append-only

Không update/delete ledger bằng business flow

## TD-07

Status phải tách rõ

QC_PASS không tự READY/RELEASED

## TD-08

MISA không điều khiển vận hành

Chỉ là accounting sync layer

## TD-09

Printer không sinh mã

Chỉ nhận payload từ Ginsengfood

## TD-10

Evidence Submitted chưa phải Accepted

Không gọi PASS sai

## TD-11

Global Gate vẫn bị chặn

Phase 2 không tự mở Gateway

## TD-12

No hardcode operational rules

Không hardcode SKU, formula, threshold, status

## TD-13

Fail-safe khi thiếu dependency

Thiếu data/runtime thì block, không suy đoán

## TD-14

Actor/permission/audit/idempotency bắt buộc

Mọi command rủi ro phải kiểm soát

## PHẦN 2/4 - WORKSTREAM MATRIX / FORM REGISTRY / FIELD BEHAVIOR / DATA INHERITANCE

## 8. WORKSTREAM MATRIX PHASE 2

Workstream

File

Mục tiêu

Đầu vào chính

Đầu ra chính

Không được làm

## P2.2A

## 02-P2-2A-SOURCE-RAW-LOT-QC-READINESS

Source / Raw / Lot / QC / Readiness

Material master, source, receipt evidence

Raw lot, QC result, readiness status

Không Production Order, không Material Issue đầy đủ

## P2.2B

## 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP

Production Order / Dossier / Snapshot / Workshop

SKU, Formula G1, UOM, production demand

Production Order, Production Dossier, Formula Snapshot

Không debit inventory, không batch output

## P2.2C

## 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER

Material Request / Approval / Issue / Receipt / Ledger

Production Dossier, Raw Lot READY

Material Request, Approval, Issue, Receipt, Raw Ledger Debit

Không Batch/Warehouse/Finished Goods

## P2.2D

## 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY

Batch / QC / Release / Warehouse / Inventory / Packaging

Production Dossier, Issue/Receipt, batch process

Batch, QC, Release, Warehouse Receipt, FG Ledger Credit

Không Commerce Sellable, không Public Trace full

## P2.2E

## 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK

Trace / QR / Public Trace / Recall / Sale Lock

Raw genealogy, batch, warehouse, packaging, print data

Trace projection, QR, Public Trace, Recall, Sale Lock

Không Commerce/AI/Gateway full

## P2.2F

## 07-P2-2F-SMOKE-EVIDENCE-REPORT

Smoke / Evidence / Report

Reports P2.2A-P2.2E

Smoke Matrix, Evidence Register, Risk Register

Không thêm feature

## 9. OPERATIONAL FORM / COMMAND REGISTRY

Bảng dưới đây là registry thiết kế kỹ thuật cho các phiếu/lệnh tối thiểu. Khi viết file chi tiết, có thể gom UI, nhưng không được bỏ checkpoint hệ thống.

Mã

Phiếu/lệnh

Chủ sở hữu nghiệp vụ

Nguồn dữ liệu

Cơ chế sinh

File chi tiết

Evidence bắt buộc

## OP-FORM-01

Phiếu tiếp nhận/đánh giá nguyên liệu đầu vào

Kho / QC

Material master, source/supplier

Tạo khi có nguyên liệu vào

## P2.2A

Chứng từ, ảnh, COA nếu có

## OP-FORM-02

Phiếu nhập nguyên liệu đầu vào / Raw Receipt

Kho

## OP-FORM-01

Tự sinh hoặc tạo từ tiếp nhận

## P2.2A

Receipt evidence

## OP-FORM-03

Raw Lot

Kho / QC

Raw Receipt

Tự sinh từ receipt

## P2.2A

Lot code, UOM, quantity

## OP-FORM-04

Phiếu QC nguyên liệu đầu vào

## QC

Raw Lot

Tự sinh khi lot chờ QC

## P2.2A

QC result, evidence

## OP-FORM-05

Readiness Decision

QA / Owner theo policy

Raw Lot + QC

Tạo khi xét READY_FOR_PRODUCTION

## P2.2A

Readiness evidence/audit

## OP-FORM-06

Lệnh sản xuất

Sản xuất

SKU + Formula active

Tạo theo kế hoạch/nhu cầu

## P2.2B

Production order evidence

## OP-FORM-07

Hồ sơ sản xuất gốc

Hệ thống / Sản xuất

Production Order

Tự sinh từ Production Order

## P2.2B

Dossier snapshot

## OP-FORM-08

Phiếu đề nghị cấp nguyên liệu

Sản xuất/Kho

Production Dossier

Tự sinh từ formula snapshot

## P2.2C

Request lines

## OP-FORM-09

Phiếu chấp thuận nguyên liệu đưa vào sản xuất

Kho/QA/Owner

Material Request

dựa trên request

## P2.2C

Approval evidence

## OP-FORM-10

Phiếu kế toán xuất nguyên liệu cho sản xuất

Kế toán/Kho

Material Issue

Tự sinh handoff/sync boundary

P2.2C / MISA phase

Accounting handoff evidence

## OP-FORM-11

Material Issue

Kho

Approval + Ready raw lot

Command ghi debit

## P2.2C

Raw ledger debit

## OP-FORM-12

Material Receipt / Workshop Receipt

Xưởng

Material Issue

Xưởng xác nhận nhận

## P2.2C

Receipt/no-second-debit evidence

## OP-FORM-13

Phiếu sơ chế

Sản xuất

Production Dossier / Batch

Tự dựa trên dossier

## P2.2D

Process evidence

## OP-FORM-14

Phiếu QC sau sấy thăng hoa

## QC

Batch/process step

Tự dựa trên batch

## P2.2D

QC evidence

## OP-FORM-15

Phiếu đóng gói cấp 1

Sản xuất/Đóng gói

Batch + packaging profile

Tự dựa trên batch

## P2.2D

Print level 1 evidence

## OP-FORM-16

Phiếu đóng gói cấp 2

Sản xuất/Đóng gói

Batch + QR/barcode config

Tự dựa trên batch

## P2.2D / P2.2E

Print level 2 evidence

## OP-FORM-17

Phiếu QC thành phẩm

## QC

Batch / packaging output

Tự dựa trên batch

## P2.2D

Finished QC evidence

## OP-FORM-18

Lệnh nhập kho thành phẩm

Kho

Batch RELEASED

Chỉ khi release pass

## P2.2D

Warehouse receipt evidence

## OP-FORM-19

Print Job

Hệ thống / Máy in

Ginsengfood payload

Tự sinh payload

## P2.2E

Print status evidence

## OP-FORM-20

Reprint Request

Authorized role

Original print job

Tạo khi cần in lại

## P2.2E

Reason/permission/audit

## OP-FORM-21

Recall Case

QA/Owner

Batch/QR/SKU impact

Tạo khi có sự cố

## P2.2E

Recall evidence

## OP-FORM-22

Sale Lock / Hold Command

Authorized role

Recall/QA/Owner decision

Tạo khi cần stop-sale

## P2.2E

Suppression evidence

## OP-FORM-23

Disposal Request

Kho/QC/Owner

Lot/batch/item

Tạo khi hàng hỏng/hết hạn/reject

## P2.2A/C/D

Disposal evidence

## OP-FORM-24

Disposal Executed / Write-off

Authorized role

Approved disposal

Command ghi write-off ledger

## P2.2A/C/D

Ledger write-off evidence

## 10. FIELD BEHAVIOR MATRIX - CLICK CHỌN / TỰ HIỆN / NHẬP TAY

Màn hình/phiếu

Được click chọn

Phải tự hiện

Được nhập tay

Tuyệt đối không được

Raw Receipt

Material từ master, source/supplier nếu có

Canonical name, UOM, source type

Quantity, chứng từ, evidence

Nhập material text tự do

Incoming QC

Raw lot chờ QC

Material, lot, receipt, quantity/UOM

QC result, QC note, evidence

Đổi material/lot gốc

Readiness

Raw lot QC_PASS

QC result, lot status, hold/quarantine flag

Decision reason nếu cần

Mark READY khi hold/reject/quarantine

Production Order

## SKU

Formula code, version, formula lines, UOM

Anchor quantity/sản lượng kế hoạch/ngày kế hoạch

Chọn tay nguyên liệu

Production Dossier

Không tạo tay rời

SKU, formula snapshot, batch/dossier code

Chỉ cập nhật field được phép

Đổi formula snapshot tùy tiện

Material Request

Không chọn tay nguyên liệu

Nguyên liệu theo formula snapshot

Note/variance request nếu rule cho phép

Thêm nguyên liệu ngoài snapshot

Material Approval

Request cần duyệt

Request lines, quantity, UOM

Approve/reject/reason

Thêm material mới không có request

Material Issue

Ready raw lot phù hợp request

Material, lot, UOM, approved quantity

Issued quantity trong giới hạn, reason nếu lệch

Issue lot chưa READY

Workshop Receipt

Material Issue

Issue lines, issued quantity

Received quantity, variance reason

Debit tồn kho lần hai

Batch Process

Batch từ dossier

SKU, formula snapshot, issue genealogy

Process result, actual output

Tạo batch không linked dossier

Batch QC

Batch

Batch data, process evidence

QC result, evidence

QC_PASS tự release

Batch Release

Batch QC_PASS

QC result, hold/reject flags

Release decision/reason

Release batch QC_FAIL/HOLD

Warehouse Receipt

Batch RELEASED

Batch, output quantity, UOM

Received FG quantity, reason nếu lệch

Nhập kho batch chưa RELEASED

Packaging cấp 1

Batch + profile

MFG/HSD, level 1 template

Actual packaging quantity

In QR/barcode nếu chưa rule

Packaging cấp 2

Batch + profile

Batch/MFG/HSD/barcode/QR payload

Actual packaging quantity

Máy in tự sinh mã

Reprint

Original print job

Payload snapshot

Reason, disposition

Reprint không audit

Public Trace

QR valid

Public whitelist fields

Không áp dụng

Lộ field nội bộ

Disposal

Lot/batch/item

Current balance, item snapshot

Reason, evidence

Xóa tồn kho trực tiếp

## 11. DATA INHERITANCE MATRIX - dựa trên DỮ LIỆU GIỮA CÁC PHIẾU

Nguồn

Đích dựa trên

Trường dựa trên bắt buộc

Không được nhập lại

Material Master

Raw Receipt

material_code, canonical_name, UOM, source_type

Tên material tự do

Raw Receipt

Raw Lot

receipt_code, material_code, quantity, UOM, source

Material/UOM gốc

Raw Lot

Incoming QC

lot_code, material, received quantity

Lot/material

Incoming QC

Readiness

QC result, evidence, hold/reject/quarantine

QC result gốc

SKU/Formula

Production Order

formula_code, version, formula kind, lines

Formula text tự do

Production Order

Production Dossier

SKU, formula snapshot, planned quantity, actor, time

SKU/formula

Production Dossier

Material Request

Ingredient lines, quantity, UOM, batch/dossier ref

Nguyên liệu

Material Request

Material Approval

Request lines, quantity, UOM

Thêm material ngoài request

Material Approval

Material Issue

Approved material lines, quantity, UOM

Dòng cấp phát

Material Issue

Workshop Receipt

Issued lines, raw lot, quantity, UOM

Debit ledger

Production Dossier

Batch

SKU, formula snapshot, material genealogy

SKU/formula

Batch

Process/QC

Batch code, SKU, dossier, process context

Batch identity

Batch Release

Warehouse Receipt

Batch released, output quantity, UOM

Batch status

Warehouse Receipt

FG Ledger

FG item, batch, quantity, UOM

Ledger direction

Batch/Warehouse

Traceability

genealogy raw -> production -> batch -> warehouse

Trace genealogy

QR/Trace

Public Trace

Whitelist projection

Internal fields

Recall/Sale Lock

Downstream Suppression

Target SKU/batch/QR, lock status

Suppression logic

## 12. STATE BOUNDARY MATRIX

State / Event

Không đồng nghĩa với

Gate kế tiếp

Raw Lot RECEIVED

## QC_PASS

Incoming QC

Raw Lot QC_PASS

## READY_FOR_PRODUCTION

Readiness gate

## READY_FOR_PRODUCTION

Issued

Material Issue command

Production Order CREATED

Material issued

Material Request/Issue

Production Dossier CREATED

Batch released

Batch process/QC/release

Material Request CREATED

Inventory debit

Material Issue

Material Issue CONFIRMED

Workshop received

Workshop Receipt

Workshop Receipt CONFIRMED

Batch output

Batch process

Batch PROCESS_COMPLETED

## QC_PASS

Batch QC

Batch QC_PASS

## RELEASED

Batch Release

Batch RELEASED

Sellable

Warehouse/Commerce later

Warehouse Receipt CONFIRMED

Sellable

Commerce Sellable Gate later

## QR GENERATED

Public-valid

QR status + trace + public policy

Evidence SUBMITTED

Evidence ACCEPTED

Owner/Reviewer review

Smoke PASSED local

Global Smoke Pass

Global release gate later

## PHẦN 3/4 - TECHNICAL DESIGN BOUNDARIES / CROSS-CUTTING CONTROL

## 13. TECHNICAL DESIGN DELIVERABLES

File P2.1 này yêu cầu khi dev/Agent làm technical design phải tạo các deliverables sau, nhưng chưa code:

Deliverable

Nội dung

Workstream Matrix

Mapping P2.2A-P2.2F

Entity Impact Draft

Danh sách entity/bảng có thể cần nhưng chưa migration

API Impact Draft

Danh sách API có thể cần nhưng chưa tạo

DTO Impact Draft

Danh sách DTO có thể cần nhưng chưa tạo

Service Impact Draft

Danh sách service/domain service có thể cần

State Machine Draft

Trạng thái và transition cần thiết

Form/Command Registry

Registry phiếu/lệnh vận hành

Field Behavior Matrix

Click chọn/tự hiện/nhập tay

Data Inheritance Matrix

Dữ liệu dựa trên giữa phiếu

Ledger Design Boundary

Debit/credit/append-only/idempotency

MISA Boundary Matrix

Handoff/sync/accounting boundary

Print Boundary Matrix

Print cấp 1/cấp 2/reprint/printer payload

Disposal Boundary Matrix

Request/review/execute/write-off/evidence

Smoke Plan Matrix

Smoke cần chạy theo từng workstream

Evidence Plan Matrix

Evidence phải gom

Stop Condition Matrix

Điều kiện phải dừng

Owner Confirm Matrix

Dữ liệu/decision cần Owner xác nhận

## 14. ENTITY / DATABASE DESIGN BOUNDARY

Tài liệu này không tạo migration, nhưng technical design phải rà soát khả năng cần các nhóm dữ liệu sau:

Nhóm entity

Mục tiêu

Ghi chú

source_origin

Nguồn nguyên liệu

Không dùng cho MISA điều khiển

raw_material_receipt

Phiếu nhập nguyên liệu

Chọn material từ master

raw_lot

Lot nguyên liệu

QC/readiness riêng

incoming_qc

QC đầu vào

QC_PASS không tự READY

raw_lot_readiness

Quyết định readiness

Có evidence/audit

production_order

Lệnh sản xuất

Chọn SKU, snapshot formula

production_dossier

Hồ sơ sản xuất gốc

Root data

formula_snapshot

Snapshot công thức

Không phụ thuộc động

material_request

Đề nghị cấp nguyên liệu

Tự hiện từ snapshot

material_approval

Chấp thuận cấp nguyên liệu

dựa trên request

material_issue

Xuất nguyên liệu

Debit chính

material_receipt

Xưởng nhận nguyên liệu

No second debit

inventory_ledger

Ledger tồn kho

Append-only

production_batch

Batch sản xuất

Linked dossier

batch_process_step

Sơ chế/sấy/đóng gói

dựa trên batch

batch_qc

QC batch/thành phẩm

QC_PASS chưa RELEASED

batch_release

Release batch

Gate riêng

warehouse_receipt

Nhập kho thành phẩm

Chỉ batch RELEASED

finished_goods_ledger

Ledger thành phẩm

Credit khi warehouse confirmed

print_job

Job in

Payload từ Ginsengfood

reprint_request

Yêu cầu in lại

Reason/permission/audit

trace_genealogy

Truy xuất nội bộ

Raw -> production -> batch -> warehouse

qr_code

## QR

Status lifecycle

public_trace_projection

Public trace whitelist

Không lộ nội bộ

recall_case

Thu hồi

Impact analysis

sale_lock

Stop-sale

Downstream suppression

disposal_request

Yêu cầu hủy

Không xóa tay

disposal_execution

Thực hiện hủy/write-off

Ledger write-off

## 15. API / DTO / SERVICE DESIGN BOUNDARY

Tài liệu này không viết API, nhưng technical design phải xác định nhóm API/DTO/service có thể cần.

Layer

Nhóm

Design requirement

## API

Raw receipt

Tạo/đọc receipt, không nhập material tự do

## API

Raw QC/readiness

Tách QC_PASS và READY_FOR_PRODUCTION

## API

Production order

Tạo order, snapshot formula

## API

Production dossier

Đọc root dossier

## API

Material request/approval

Tự hiện/dựa trên lines

## API

Material issue

Guard ready lot, idempotency, ledger debit

## API

Material receipt

Confirm receipt, no second debit

## API

Batch/QC/release

QC_PASS riêng, release riêng

## API

Warehouse receipt

Chỉ released batch, FG credit

## API

Print/reprint

Payload, status, reason/audit

## API

Trace/public trace

Internal genealogy, public whitelist

## API

Recall/sale lock

Case, impact, suppression

## API

Disposal/write-off

Request/review/execute/ledger

## DTO

Public trace DTO

Whitelist-only

## DTO

Admin/internal DTO

Có quyền, không public

Service

State guard

Chặn transition sai

Service

Ledger writer

Append-only + idempotency

Service

Snapshot service

Freeze formula

Service

Print payload service

Ginsengfood sinh mã

Service

MISA handoff service

Chỉ sync/handoff

Service

Sale lock service

Downstream suppression boundary

## 16. CROSS-CUTTING CONTROL MATRIX

Control

Bắt buộc ở đâu

Rule

Actor

Mọi command

Phải biết ai thực hiện

Permission

Mọi action rủi ro

Không có quyền thì DENY

Audit

Create/update/status/command

Ghi action/result/correlation

Idempotency

Command có side effect

Same key same payload không tạo side effect lần hai

Evidence

QC/release/readiness/disposal/reprint/sale lock

Submitted chưa pass nếu policy cần accepted

State guard

Raw lot, production, issue, batch, warehouse, QR, sale lock

Không transition sai

Ledger append-only

Raw ledger, FG ledger, write-off

Không update/delete

Stale state guard

Command theo version/status

Nếu state cũ thì conflict/retry

Correlation ID

Cross-form chain

Truy vết production dossier

Fail-safe

Runtime dependency unavailable

Block, không suy đoán

Public/private boundary

Public trace, QR

Whitelist-only

Downstream suppression

Sale lock/recall

Chặn Commerce/AI/Gateway/Ads/Live/CRM/IVR

## 17. MISA BOUNDARY MATRIX

Hạng mục

Ginsengfood sở hữu

MISA sở hữu

Không được

Lệnh sản xuất

Có

Không

MISA tạo/sửa production order

Công thức/formula

Có

Không

MISA sửa formula

Batch

Có

Không

MISA release batch

Material Issue operational

Có

Không

MISA quyết định cấp nguyên liệu

Phiếu kế toán xuất nguyên liệu

Ginsengfood tạo handoff

MISA hạch toán chính thức

MISA làm thay operational issue

Inventory operational truth

Ginsengfood

Không

MISA ghi đè ledger vận hành

Traceability

Ginsengfood

Không

MISA điều khiển trace

Recall/Sale Lock

Ginsengfood

Không

MISA mở khóa sale lock

Print payload

Ginsengfood

Không

MISA/máy in sinh mã

Accounting posting

Handoff/sync

Có

Ginsengfood tự gọi hạch toán chính thức nếu chưa MISA phase

Thiết kế Phase 2 chỉ khóa boundary MISA.

Không triển khai full MISA Integration trong file này.

## 18. PRINT / QR / REPRINT BOUNDARY MATRIX

Nhóm

Rule

Print payload

Ginsengfood sinh dữ liệu in

Máy in

Chỉ nhận payload, in, trả trạng thái

Máy in không được

Tự sinh mã sản xuất, mã lô, MFG, HSD, barcode, QR

Print cấp 1

Chỉ in MFG/HSD theo nguồn tổng hợp

Print cấp 2

In batch/MFG/HSD/barcode/QR

QR status

GENERATED / PRINTED / FAILED / VOID / bị chặn

QR public-valid

Chỉ khi status và trace/public policy hợp lệ

Reprint

Phải có original_print_id, reason, permission, audit

Reprint evidence

Payload snapshot, reason, old label disposition nếu có

Public Trace

Whitelist-only

## 19. DISPOSAL / WRITE-OFF BOUNDARY MATRIX

Bước

Rule

Disposal Request

Tạo khi nguyên liệu/vật tư/thành phẩm cần hủy

QA/Owner Review

Bắt buộc theo policy

Disposal Approved

Chỉ khi có role hợp lệ

Disposal Executed

Thực hiện hủy có actor/time/evidence

Inventory Ledger Write-off

Ghi ledger write-off append-only

Evidence

Ảnh, biên bản, chứng từ

Audit

Bắt buộc

Không được

Xóa tay tồn kho

Áp dụng cho:

Raw material reject.

Raw material expired.

Raw material damaged.

Packaging damaged.

Finished goods rejected.

Batch failed.

Recalled goods if disposal applies.

## 20. PROCUREMENT / MRP SUPPRESSION BOUNDARY

Phase 2 không triển khai full MRP/procurement nếu chưa mở phase riêng.

Tuy nhiên, vì nguồn tổng hợp có quy tắc ngưỡng tồn kho, Phase 2 phải giữ boundary để không thiết kế sai về sau.

Nhóm

Rule boundary

Nhóm A đặc thù SKU

Threshold 5 mẻ/SKU

Nhóm A dùng chung

Aggregate 20 mẻ chuẩn

Sâm Savigin

Company Source / strategic reserve, không supplier purchase thường

B1 film

15 mẻ/SKU, 115.560 gói quy đổi/SKU

B2 hộp

15 mẻ/SKU, 28.890 hộp/SKU

B2 thùng

15 mẻ/SKU, 4.815 thùng/SKU

available_stock + approved_incoming >= threshold

Loại khỏi phiếu thu mua

Ngoại lệ

Kế hoạch sản xuất lớn có Giám đốc phê duyệt/evidence/audit

Không được:

Hardcode threshold trong service.

Tự tạo purchase request trong Phase 2.

Áp dụng purchase suppression thường cho Sâm Savigin.

Sửa formula sản xuất bằng buffer/threshold.

## 21. SÂM SAVIGIN COMPANY SOURCE / STRATEGIC RESERVE

Thiết kế Phase 2 phải khóa riêng Sâm Savigin:

Rule

Diễn giải

Source type

## COMPANY_SOURCE

Không supplier material thường

Không đưa vào phiếu thu mua supplier

Có harvest/intake route

Đến mùa vụ có harvest/sơ chế/intake/QC

Có raw lot/QC/readiness

Vẫn phải qua QC và READY_FOR_PRODUCTION

Strategic reserve

Tồn kho dự trữ chiến lược

MRP cảnh báo thiếu

Khi READY stock dưới safety threshold

Disposal

Hỏng/hết hạn vẫn qua write-off flow

PHẦN 4/4 - EXECUTION ORDER / HANDOFF / SMOKE / DONE GATE / FAIL GATE

## 22. EXECUTION ORDER SAU P2.1

Sau khi tài liệu P2.1 này hoàn tất, thứ tự đúng là:

Thứ tự

File tiếp theo

Mục tiêu

## 02-P2-2A-SOURCE-RAW-LOT-QC-READINESS

Viết lại/triển khai Source/Raw/QC/Readiness boundary

## 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP

Viết lại/triển khai Production Order/Dossier/Snapshot

## 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER

Viết lại/triển khai Material Issue/Receipt/Ledger

## 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY

Viết lại/triển khai Batch/QC/Release/Warehouse/Packaging

## 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK

Viết lại/triển khai Trace/QR/Recall/Sale Lock

## 07-P2-2F-SMOKE-EVIDENCE-REPORT

Viết lại smoke/evidence/report Phase 2

Hiện tại, với yêu cầu đang làm tài liệu:

Chỉ viết lại tài liệu sạch, chưa mở Limited Implementation.

## 23. DEV / CODEX / COPILOT HANDOFF

## 23.1. Mode mặc định

Khi giao file này cho Agent, mode là:

## TECHNICAL DESIGN ONLY

Agent không được sửa code.

## 23.2. Prompt đúng cho Agent

Nhiệm vụ:

Lập Workstream Matrix cho P2.2A-P2.2F.

Lập Operational Form / Command Registry.

Lập Field Behavior Matrix.

Lập Data Inheritance Matrix.

Lập Entity Impact Draft.

Lập API/DTO/Service Impact Draft.

Lập State Machine Draft.

Lập Ledger Boundary.

Lập MISA Boundary.

Lập Print Boundary.

Lập Disposal Boundary.

Lập Smoke Plan.

Lập Evidence Plan.

Lập Owner Confirm Required.

Lập Stop Condition Matrix.

Cấm:

Không sửa code.

Không tạo migration.

Không tạo seed.

Không tạo API.

Không tạo DTO.

Không tạo UI.

Không tạo worker.

Không code trực tiếp từ nguồn tổng hợp.

Không gọi Gateway PASS.

Không gọi Production Readiness.

## 24. OWNER CONFIRM REQUIRED

Các điểm cần Owner/dev lead xác nhận trước khi mở implementation:

Nhóm

Nội dung cần xác nhận

Operational forms

Danh sách phiếu/lệnh cuối cùng, có gom UI hay không

Role/permission

Ai được tạo/duyệt/issue/receive/release/print/reprint/dispose

Readiness policy

Điều kiện READY_FOR_PRODUCTION

Production Dossier

Mã hồ sơ, field bắt buộc, snapshot scope

Formula snapshot

Field snapshot bắt buộc từ Phase 1

Material Request

Cho phép variance không, mức nào, ai duyệt

Material Issue

Idempotency, debit rule, raw lot selection

Material Receipt

Variance reason, no second debit

Batch process

Các bước sơ chế/sấy/đóng gói cần tách phiếu hay gom

Batch Release

Điều kiện release, actor, evidence

Warehouse Receipt

Điều kiện nhập kho thành phẩm

Print cấp 1/cấp 2

Template, payload, printer status

Reprint

Reason code, approval, old label disposition

Public Trace

Whitelist field

Recall/Sale Lock

Target scope, downstream suppression

## MISA

Handoff/sync boundary, không full integration

Disposal

Reason, approval, write-off evidence

Sâm Savigin

Company Source / harvest / reserve rule

MRP threshold

Chỉ boundary, chưa full procurement

## 25. STOP CONDITIONS

Dev/Agent phải dừng nếu gặp:

Mã

Điều kiện dừng

Hành động

## P2-TD-STOP-01

Chưa có Analysis Report

Không design tiếp

## P2-TD-STOP-02

Chưa rõ form registry

Owner confirm

## P2-TD-STOP-03

Chưa rõ field click/tự hiện/nhập tay

Owner confirm

## P2-TD-STOP-04

Chưa rõ Production Dossier

Owner confirm

## P2-TD-STOP-05

Chưa rõ Formula Snapshot

Quay lại Phase 1/P2.2B

## P2-TD-STOP-06

Chưa rõ Material Issue debit rule

Owner/dev lead confirm

## P2-TD-STOP-07

Có thiết kế double debit

Dừng P0

## P2-TD-STOP-08

Có thiết kế ledger update/delete

Dừng P0

## P2-TD-STOP-09

QC_PASS đang bị dùng làm READY/RELEASED

Dừng P0

## P2-TD-STOP-10

MISA đang điều khiển vận hành

Dừng P0

## P2-TD-STOP-11

Máy in đang tự sinh mã

Dừng P0

## P2-TD-STOP-12

Disposal đang xóa tay tồn kho

Dừng P0

## P2-TD-STOP-13

Phase 2 tự quyết sellable

Dừng P0

## P2-TD-STOP-14

Agent chuẩn bị code khi mode là design

Dừng

## P2-TD-STOP-15

Conflict với README Phase 2 V1.1

Lập conflict report

## 26. SMOKE PLAN MATRIX

Nhóm smoke

File carry chi tiết

Mục tiêu

Raw/QC/Readiness

## P2.2A, P2.2F

QC_PASS không tự READY, chỉ READY mới issue

Sâm Strategic Reserve

## P2.2A, P2.2F

Sâm là Company Source

Production Dossier

## P2.2B, P2.2F

Production Order sinh root dossier

Formula Snapshot

## P2.2B, P2.2F

Snapshot không đổi khi formula thay đổi

No Manual Ingredient

## P2.2B/C, P2.2F

Không chọn tay nguyên liệu

Material Issue Debit

## P2.2C, P2.2F

Debit một lần

Material Receipt No Debit

## P2.2C, P2.2F

Receipt không giảm tồn lần hai

Ledger Append-only

## P2.2C/D, P2.2F

Không update/delete ledger

Batch QC/Release

## P2.2D, P2.2F

QC_PASS không tự RELEASED

Warehouse Receipt

## P2.2D, P2.2F

Chỉ nhận batch RELEASED

Print Level 1/2

## P2.2D/E, P2.2F

Payload đúng cấp

Reprint

## P2.2E, P2.2F

Có reason/permission/audit

Public Trace

## P2.2E, P2.2F

Whitelist-only

QR status

## P2.2E, P2.2F

VOID/FAILED không public-valid

Recall/Sale Lock

## P2.2E, P2.2F

Downstream suppression

MISA Boundary

## P2.2C, P2.2F

MISA không mutate operational truth

Disposal/Write-off

## P2.2A/C/D, P2.2F

Không xóa tay tồn kho

Cross-cutting

## P2.2F

Actor/permission/audit/idempotency/evidence

## 27. EVIDENCE PLAN MATRIX

Evidence

Nội dung

## EVD-P2-TD-WORKSTREAM

Workstream Matrix P2.2A-P2.2F

## EVD-P2-TD-FORM-REGISTRY

Operational Form / Command Registry

## EVD-P2-TD-FIELD-BEHAVIOR

Field Behavior Matrix

## EVD-P2-TD-DATA-INHERITANCE

Data Inheritance Matrix

## EVD-P2-TD-STATE-BOUNDARY

State Boundary Matrix

## EVD-P2-TD-ENTITY-IMPACT

Entity Impact Draft

## EVD-P2-TD-API-IMPACT

API/DTO/Service Impact Draft

## EVD-P2-TD-MISA-BOUNDARY

MISA Boundary Matrix

## EVD-P2-TD-PRINT-BOUNDARY

Print / Reprint Boundary Matrix

## EVD-P2-TD-DISPOSAL

Disposal / Write-off Boundary Matrix

## EVD-P2-TD-SMOKE-PLAN

Smoke Plan Matrix

## EVD-P2-TD-OWNER-CONFIRM

Owner Confirm Matrix

## EVD-P2-TD-STOP-CONDITION

Stop Condition Matrix

EVD-P2-TD-GATEWAY-bị chặn

Gateway still bị chặn evidence

Evidence Submitted chưa phải Evidence Accepted.

## 28. REPORT FORMAT 15 MỤC

Mọi report sau P2.1 phải có đủ 15 mục:

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Kết quả backend build.

Kết quả frontend build.

Kết quả cleanup process.

Cập nhật Markdown/tài liệu.

Kết quả database migration/update nếu áp dụng.

Kết quả seed validation nếu áp dụng.

Rủi ro còn lại.

Cập nhật handoff.

Đề xuất tiếp theo.

Nếu report thiếu 15 mục, không được chuyển prompt tiếp theo.

## 29. DONE GATE

File P2.1 được xem là đạt khi:

Đã khóa vai trò Technical Design Only.

Đã khóa không code/migration/seed.

Đã có Workstream Matrix.

Đã có Operational Form / Command Registry.

Đã có Field Behavior Matrix.

Đã có Data Inheritance Matrix.

Đã có State Boundary Matrix.

Đã có Entity Impact Draft.

Đã có API/DTO/Service Impact Boundary.

Đã khóa Production Dossier là root.

Đã khóa Formula Snapshot.

Đã khóa No Manual Ingredient Selection.

Đã khóa Material Issue là debit point.

Đã khóa Material Receipt no second debit.

Đã khóa Ledger append-only.

Đã khóa Batch QC_PASS != RELEASED.

Đã khóa Warehouse only released batch.

Đã khóa Print cấp 1/cấp 2.

Đã khóa Printer no code generation.

Đã khóa Reprint control.

Đã khóa Public Trace whitelist-only.

Đã khóa QR VOID/FAILED not public-valid.

Đã khóa Recall/Sale Lock downstream suppression.

Đã khóa MISA boundary.

Đã khóa Disposal/write-off.

Đã khóa Sâm Savigin strategic reserve.

Đã khóa MRP/procurement suppression boundary.

Đã có Smoke Plan Matrix.

Đã có Evidence Plan Matrix.

Đã có Owner Confirm Matrix.

Đã có Stop Conditions.

Đã khóa report 15 mục.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 30. FAIL GATE

File P2.1 fail nếu xảy ra một trong các trường hợp sau:

Chuyển từ technical design sang code.

Tạo migration.

Tạo seed.

Thiết kế thiếu Operational Form Registry.

Thiết kế thiếu Field Behavior Matrix.

Thiết kế cho phép nhập tay nguyên liệu ở Production Order.

Thiết kế cho phép nhập tay nguyên liệu ở Material Request.

Thiết kế không có Production Dossier.

Thiết kế không có Formula Snapshot.

Thiết kế double debit ở Material Receipt.

Thiết kế ledger update/delete.

Thiết kế QC_PASS = READY_FOR_PRODUCTION.

Thiết kế Batch QC_PASS = RELEASED.

Thiết kế Warehouse nhận batch chưa RELEASED.

Thiết kế Warehouse Receipt tự set Sellable.

Thiết kế printer tự sinh mã.

Thiết kế reprint không reason/audit.

Thiết kế MISA điều khiển production/formula/batch/trace/print.

Thiết kế disposal bằng xóa tay tồn kho.

Thiết kế Public Trace không whitelist.

Thiết kế QR VOID/FAILED public-valid.

Thiết kế Sale Lock không chặn downstream.

Thiết kế Phase 2 lấn sang Commerce sellable.

Thiết kế full MRP/procurement trong Phase 2 khi chưa mở phase.

Evidence Submitted bị gọi Accepted.

Gateway bị mở.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

Hạng mục

Trạng thái

Documentation review

## READY FOR OWNER REVIEW

Phase

## PHASE 2 - OPERATIONAL CORE

File type

## TECHNICAL DESIGN ONLY

Implementation authorization

KHONG

Code authorization

KHONG

Migration authorization

KHONG

Seed authorization

KHONG

Limited implementation authorization

KHONG

Evidence status

NOT ACCEPTED mặc định

Gateway status

bị chặn

Global Smoke Pass

KHONG

Completion Decision

KHONG

Release Readiness

KHONG

Production Readiness

KHONG

Go-live Decision

KHONG

Tài liệu kế tiếp
