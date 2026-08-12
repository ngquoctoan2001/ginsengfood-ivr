# 05-P2 - MẺ SẢN XUẤT QC RELEASE KHO VÀ TỒN

Trạng thái: `BATCH_QC_RELEASE_WAREHOUSE_CANONICAL`  
Mode: `LIMITED IMPLEMENTATION HANDOFF`  
Mục tiêu: khóa batch execution, QC, packaging, release, warehouse receipt và finished goods inventory.

## 1. Batch lifecycle

```text
DRAFT -> IN_PRODUCTION -> PREPROCESSING -> FREEZE_DRY -> POST_DRY_QC -> PACKAGING_L1 -> PACKAGING_L2 -> FINISHED_QC -> RELEASE_PENDING -> RELEASED -> WAREHOUSE_RECEIVED
```

Fail/hold path:

```text
QC_HOLD / QC_REJECTED / REWORK_REQUIRED / DISPOSAL_REQUESTED / WRITTEN_OFF
```

## 2. QC boundary

1. QC sau sấy thăng hoa bắt buộc có % độ ẩm và evidence.
2. QC thành phẩm bắt buộc trước release.
3. `QC_PASS` không tự là `RELEASED`.
4. Batch release cần QA/owner approval.

## 3. Packaging L1/L2

Packaging L1 theo gói/lọ/hũ và print cấp 1. Packaging L2 theo hộp/thùng/lọ/hũ và print cấp 2/QR/barcode/batch. Reprint phải có reason, actor, old/new code, count và audit.

## 4. Warehouse receipt

Warehouse chỉ nhận batch `RELEASED`. Finished goods inventory chỉ credit sau warehouse receipt hợp lệ; không sửa tồn trực tiếp.

## 5. Batch lifecycle chi tiết

| State | Ý nghĩa | Gate vào state |
| --- | --- | --- |
| `CREATED` | Batch sinh từ dossier | Dossier approved |
| `MATERIAL_READY` | Nguyên liệu đã issue và xưởng nhận | Workshop receipt confirmed |
| `IN_PROCESS` | Bắt đầu sản xuất | Nhân sự/stage mở, material ready |
| `PRE_PROCESS_COMPLETED` | Sơ chế hoàn tất | Có actual qty/hao hụt/evidence |
| `FREEZE_DRYING` | Đang sấy thăng hoa | Stage start |
| `FREEZE_DRYING_COMPLETED` | Sấy xong | Có completion evidence |
| `POST_DRY_QC_WAITING` | Chờ QC sau sấy | Stage done |
| `POST_DRY_QC_PASSED` | QC sau sấy đạt | QC approval |
| `PACKAGING_LEVEL_1` | Đóng gói cấp 1 | Packaging material ready |
| `PACKAGING_LEVEL_2` | Đóng gói cấp 2 | Cấp 1 completed |
| `FINISHED_QC_WAITING` | Chờ QC thành phẩm | Packaging completed |
| `QC_PASSED_WAITING_RELEASE` | QC thành phẩm đạt, chờ release | QC approval |
| `RELEASED` | Được release | Release approval |
| `HOLD/REJECTED/DISPOSAL_WAITING/DISPOSED/CANCELLED` | Nhánh chặn/xử lý | Owner/QC decision |

## 6. Phiếu theo dõi sơ chế

| Field | Rule |
| --- | --- |
| dossier_id, batch_id, stage_id | Bắt buộc |
| material lines | Tự hiện từ issue/workshop receipt |
| planned_qty/actual_qty | actual được nhập, planned không sửa tay |
| loss_qty/loss_rate | Nếu vượt policy phải owner approval |
| operator check-in/out | Liên kết phiếu nhân sự |
| evidence_refs | Ảnh, cân, biên bản, note |

## 7. QC sau sấy và QC thành phẩm

| QC gate | Input | Output |
| --- | --- | --- |
| Sau sấy thăng hoa | batch, stage, sample refs, actual yield | `POST_DRY_QC_PASSED`, `HOLD` hoặc `REJECTED` |
| Trước đóng gói | post-dry state, packaging readiness | Cho phép hoặc block packaging |
| Thành phẩm | package qty, label/QR payload, sample refs | `QC_PASSED_WAITING_RELEASE`, hold hoặc reject |
| Release | finished QC pass + approval | `RELEASED` |

QC pass không tự release. Release là command riêng có audit và owner/QC approval.

## 8. Đóng gói cấp 1/cấp 2

| Cấp | Dữ liệu tự hiện | Dữ liệu nhập thực tế |
| --- | --- | --- |
| Cấp 1 | SKU, batch, package material B1, planned gói/lọ/hũ, print payload cấp 1 | actual pack qty, reject qty, machine/operator, reprint reason |
| Cấp 2 | hộp B2, thùng carton B2, tem/ribbon/mực B3, planned hộp/thùng | actual box/carton qty, damaged packaging, label check result |

Output chuẩn nguồn: mẻ 400 = 7.200 gói/phần ăn = 1.800 hộp = 300 thùng. Buffer mua/threshold B1/B2 không được dùng để sửa output thực tế của batch.

## 9. Print/reprint boundary

Print payload phải lấy từ dossier/batch/packaging profile:

1. SKU/product name public-approved.
2. Batch public code.
3. Production/expiry date nếu được duyệt.
4. QR/public trace URL/code.
5. Packaging level.
6. Template version.
7. Payload hash.

Reprint bắt buộc có reason, actor, old_print_id, new_print_id, count, payload hash, approval nếu vượt policy.

## 10. Warehouse receipt và finished goods ledger

| Event | Rule |
| --- | --- |
| `BatchReleased` | Chưa tăng kho thành phẩm |
| `FinishedGoodsReceiptCreated` | Chỉ tạo từ released batch |
| `FinishedGoodsReceiptConfirmed` | Tăng tồn thành phẩm |
| `FinishedGoodsHoldActivated` | Chặn sellable/trace public nếu cần |
| `FinishedGoodsWriteOff` | Giảm tồn với reason/evidence |

Finished goods inventory phải lưu SKU, batch, package level, quantity, warehouse, location, release state, sale lock state và trace refs.

## 11. Fail gates

1. Batch không được start nếu material chưa nhận xưởng.
2. Packaging không được start nếu post-dry QC chưa pass.
3. Release không được chạy nếu finished QC chưa pass.
4. Warehouse không được nhận batch chưa released.
5. Inventory không được tăng ở QC pass.
6. Reprint không reason/audit -> block.
7. Sale lock active -> không mark sellable dù kho có tồn.
---

## 5. Quy tắc in mã sản phẩm, QR, barcode và reprint

# PRINTING CODE RULES

## 2. QUY TẮC IN MÃ SẢN PHẨM 2 GIAI ĐOẠN - GINSENGFOOD

## 0. MỞ ĐẦU

### 0.1. Mục đích

Khóa nguyên tắc in mã theo 2 cấp đóng gói:

cấp 1

cấp 2

### 0.2. Nguyên tắc cốt lõi

Hệ thống Ginsengfood sinh dữ liệu in. Máy in chỉ nhận payload để in.

Máy in không được tự sinh:

mã sản xuất

mã lô

ngày sản xuất

hạn dùng

barcode

QR

## 1. QUY TẮC SINH MÃ TRUNG TÂM

### 1.1. Hệ thống phải sinh

batch / lô sản xuất

ngày sản xuất

hạn dùng

barcode data

QR token / QR payload

print payload theo cấp đóng gói

### 1.2. Máy in chỉ làm

nhận payload

in

trả trạng thái:

đã nhận

đã in

lỗi in

dừng line

reprint callback

### 1.3. Cấm

Không cho máy in tự sinh mã nghiệp vụ làm source of truth.

## 2. QUY TẮC SINH DỮ LIỆU IN THEO THỜI GIAN THỰC

### 2.1. Điều kiện để hệ thống tự sinh dữ liệu in

Khi hệ thống đã biết:

SKU đang sản xuất

Mã hồ sơ sản xuất

Mã lệnh sản xuất

Mẻ sản xuất

Công đoạn đóng gói

Cấp đóng gói

Template in đang áp dụng

thì hệ thống phải tự sinh dữ liệu in theo thời gian thực.

### 2.2. Ý nghĩa “tự sinh thời gian thực”

“Tự sinh thời gian thực” nghĩa là:

hệ thống chủ động tạo dữ liệu in ngay tại thời điểm line đang chạy

không để người vận hành nhập tay mã

không để máy in tự nghĩ ra mã nghiệp vụ

### 2.3. Người vận hành chỉ làm

xác nhận line đang chạy lệnh nào

xác nhận cấp đóng gói nào

chọn máy in

bấm bắt đầu in

theo dõi lỗi / reprint theo quyền

## 3. GIAI ĐOẠN IN CẤP 1

### 3.1. Đơn vị áp dụng

gói

chai

lọ

hũ

### 3.2. Nội dung in cấp 1

Chỉ in:

ngày sản xuất

hạn dùng

### 3.3. Mục tiêu

tối giản

nhanh

ít lỗi

không làm nặng line đóng gói cấp 1

## 4. GIAI ĐOẠN IN CẤP 2

### 4.1. Đơn vị áp dụng

hộp

### 4.2. Nội dung in cấp 2

Phải in:

lô sản xuất

ngày sản xuất

hạn dùng

mã vạch

mã QR

### 4.3. Quy tắc sinh dữ liệu in cấp 2

Khi line cấp 2 đang chạy, hệ thống phải:

tự xác định SKU

tự xác định batch/lô

tự xác định MFG/EXP

tự sinh barcode

tự sinh QR

tự cấp số thứ tự in nếu policy cần

tự gửi payload xuống máy in

### 4.4. Cấm

Không cho người vận hành nhập tay:

mã lô

barcode

QR

ngày sản xuất

hạn dùng trừ trường hợp override được phê duyệt riêng.

## 5. LIÊN KẾT GIỮA CÔNG THỨC, LỆNH SẢN XUẤT VÀ IN MÃ

### 5.1. Dữ liệu in không tách rời công thức

Dữ liệu in phải sinh từ:

SKU

công thức đang áp dụng

batch/lô thật

hồ sơ sản xuất gốc

công đoạn đóng gói thực tế

### 5.2. Không có bước chọn lại nguyên liệu để in

Người vận hành không được chọn lại nguyên liệu để sinh dữ liệu in. Dữ liệu in chỉ bám vào:

lệnh đang chạy

batch đang chạy

sản phẩm đang đóng gói

### 5.3. Kết luận

Công thức tự hiện đúng thì dữ liệu in mới đúng. Nếu công thức không bị chọn tay lại ở bước sản xuất, hệ thống mới có thể sinh mã in đúng, sạch và truy được.

## 6. QUY TẮC TEMPLATE IN

### 6.1. Template cấp 1

Mỗi SKU phải biết:

cấp 1 dùng đơn vị gì

template cấp 1 là gì

chỉ in MFG/HSD

### 6.2. Template cấp 2

Mỗi SKU phải biết:

cấp 2 dùng đơn vị gì

template cấp 2 là gì

in lô/MFG/HSD/barcode/QR

### 6.3. Mapping theo SKU

Mỗi SKU phải có bảng mapping:

| SKU | Đơn vị cấp 1 | Nội dung in cấp 1 | Đơn vị cấp 2 | Nội dung in cấp 2 |
| --- | --- | --- | --- | --- |

## 7. REPRINT

### 7.1. Cấp 1

Reprint có kiểm soát, mức kiểm soát nhẹ hơn.

### 7.2. Cấp 2

Reprint phải bị kiểm soát chặt vì liên quan:

batch

barcode

QR

traceability

recall

### 7.3. Bắt buộc

có lý do reprint

có người phê duyệt

có log reprint

không cho máy in tự ý reprint ngoài hệ thống

## 8. LOG IN BẮT BUỘC

### 8.1. Log cấp 1

mã print job

SKU

đơn vị in

MFG/HSD

thời gian gửi lệnh

trạng thái in

### 8.2. Log cấp 2

mã print job

SKU

batch/lô

MFG/HSD

barcode

QR

thời gian gửi lệnh

trạng thái in

reprint nếu có

## 9. PASS / FAIL GATE

### 9.1. Đạt khi

cấp 1 chỉ in MFG/HSD

cấp 2 in đủ lô/MFG/HSD/barcode/QR

hệ thống sinh toàn bộ dữ liệu in theo thời gian thực

máy in chỉ in và trả trạng thái

có log đầy đủ

reprint bị kiểm soát

dữ liệu in bám đúng batch/lô và lệnh sản xuất thật

### 9.2. Chưa đạt khi

máy in tự sinh mã nghiệp vụ

không có log in

không có log reprint

barcode/QR không gắn với batch thật

không phân biệt cấp 1 và cấp 2

người vận hành phải nhập tay mã in cấp 2

---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P2.2D - BATCH QC RELEASE WAREHOUSE INVENTORY

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-01 Batch / QC / Release / Warehouse / Inventory.
- PACK-04 MISA handoff after warehouse receipt confirmed.
- TECH-03 Batch QC / Release / Warehouse.
- Batch QC_PASS khong dong nghia RELEASED.

## Noi Dung Rewrite

## 30. KẾT LUẬN ĐIỀU PHỐI P2.2C

Từ thời điểm dùng bản V1.1 này, miền Material Request / Material Issue / Material Receipt / Ledger phải tuân thủ các khóa sau:

Material Request phải lấy dữ liệu từ Production Dossier.

Material Request phải tự hiện nguyên liệu theo Formula Snapshot.

Không chọn tay nguyên liệu ở Material Request.

Material Approval phải dựa trên từ Material Request.

Material Approval không được thêm nguyên liệu ngoài request.

Material Issue chỉ được xuất raw lot READY_FOR_PRODUCTION.

Raw lot QC_PASS chưa đủ để issue.

HOLD/QUARANTINE/REJECTED/EXPIRED/DAMAGED không được issue.

Material Issue là điểm giảm tồn nguyên liệu chính.

Raw Inventory Ledger Debit chỉ được tạo tại Material Issue.

Material Receipt / Workshop Receipt không giảm tồn lần hai.

Material Receipt chỉ xác nhận xưởng nhận nguyên liệu.

Ledger phải append-only.

Retry không được double debit.

Variance phải có reason/evidence.

Phiếu kế toán xuất nguyên liệu chỉ là handoff kế toán/MISA.

MISA không điều khiển Material Issue/Receipt/Ledger.

Disposal/write-off không được xóa tay tồn kho.

Actor/permission/audit/idempotency/evidence là bắt buộc.

Evidence Submitted chưa phải Evidence Accepted.

Global Gate vẫn bị chặn.

Tài liệu kế tiếp cần viết lại toàn bộ là:

## 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY

## TÀI LIỆU ĐẶC TẢ BATCH / PROCESS / QC / RELEASE / WAREHOUSE / INVENTORY / PACKAGING CHO PHASE 2

Global Gate Status: bị chặn
Completion Status: WAITING IMPLEMENTATION EVIDENCE
Evidence Status: NOT ACCEPTED
Production Readiness: KHONG
Release Readiness: KHONG
Go-live Decision: KHONG
Default Agent Mode: LIMITED IMPLEMENTATION CHỈ KHI ĐÃ QUA ANALYSIS + TECHNICAL DESIGN + OWNER APPROVAL

## PHẦN 1/4 - PHASE MARKER / PURPOSE / SOURCE-OF-TRUTH / SCOPE

## 1. PHASE MARKER

Tài liệu này thuộc nhóm:

## PHASE-02-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY

Đây là file thứ tư trong nhóm triển khai giới hạn của Phase 2 - Operational Core.

File này phụ trách miền:

Production Batch.

Batch process state.

Check-in/check-out nhân sự sản xuất nếu thuộc phạm vi vận hành.

Phiếu sơ chế.

Phiếu sấy thăng hoa.

Phiếu QC sau sấy thăng hoa.

Phiếu đóng gói cấp 1.

Phiếu đóng gói cấp 2.

Phiếu QC thành phẩm.

Batch QC.

Batch Release Gate.

Finished Goods Warehouse Receipt.

Finished Goods Ledger Credit.

Finished Goods Balance Projection.

Packaging output.

Print level 1 boundary.

Print level 2 boundary.

No sellable boundary.

No double credit boundary.

Disposal/write-off cho batch/thành phẩm/bao bì hỏng.

Actor / permission / audit / idempotency / evidence.

Smoke/evidence cho P2.2D.

Report 15 mục cho P2.2D.

Tài liệu này thay thế toàn bộ nội dung cũ của:

## 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY.docx

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY

Phiên bản

Phase

## PHASE 2 - OPERATIONAL CORE

Workstream

## P2.2D

Loại tài liệu

Limited Implementation Handoff / Batch-QC-Release-Warehouse-Inventory

Nguồn chính

README Phase 2 V1.1 + P2.1 Technical Design + P2.2B/P2.2C

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

Phụ thuộc Phase 1

SKU / Formula / Packaging Profile / UOM đã chuẩn hóa

Phụ thuộc P2.2B

Production Order / Production Dossier / Formula Snapshot

Phụ thuộc P2.2C

Material Issue / Workshop Receipt / Raw Ledger Debit

Cho phép code ngay bởi file này?

Không, chỉ sau khi có lệnh Limited Implementation riêng

Cho phép migration ngay bởi file này?

Không

Cho phép seed ngay bởi file này?

Không

Cho phép Agent tự suy luận nghiệp vụ?

Không

Global Gate

bị chặn

Production Readiness

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để khóa toàn bộ chuỗi vận hành sau khi nguyên liệu đã được cấp và xưởng đã xác nhận nhận nguyên liệu.

Đây là miền biến dữ liệu từ:

Production Dossier + Material Issue + Workshop Receipt

thành:

Production Batch -> Batch Process -> Batch QC -> Batch Release -> Warehouse Receipt -> Finished Goods Ledger

Nếu P2.2D sai, hệ thống có thể phát sinh các lỗi rất nghiêm trọng:

Batch không trace được về Production Dossier.

Batch không biết sản xuất theo formula version nào.

QC_PASS bị hiểu sai thành RELEASED.

Batch chưa release vẫn được nhập kho.

Warehouse Receipt tạo tồn thành phẩm sai.

Retry nhập kho tạo double credit.

Thành phẩm nhập kho tự được xem là sellable.

Print cấp 1/cấp 2 in sai dữ liệu.

Máy in hoặc operator tự sinh mã thay hệ thống.

Batch hỏng/reject bị xóa tay thay vì disposal/write-off.

Traceability và Recall sau này không xác định đúng batch.

Commerce/AI/Gateway/Ads/Live/CRM có thể bán nhầm hàng chưa được release.

Mục tiêu chính của tài liệu này:

Khóa Batch phải linked Production Dossier.

Khóa Batch phải dựa trên SKU, formula snapshot, material genealogy từ Production Dossier.

Khóa Batch Process phải dựa trên dữ liệu, không nhập lại SKU/formula/material.

Khóa sơ chế, sấy, QC sau sấy, đóng gói cấp 1, đóng gói cấp 2, QC thành phẩm phải theo chuỗi state rõ.

Khóa Batch QC_PASS chưa đồng nghĩa RELEASED.

Khóa Batch Release là gate riêng.

Khóa Warehouse chỉ được nhận batch đã RELEASED.

Khóa Finished Goods Ledger Credit chỉ phát sinh khi Warehouse Receipt confirmed.

Khóa Warehouse Receipt không tự set Sellable.

Khóa print cấp 1 chỉ in MFG/HSD.

Khóa print cấp 2 in batch/MFG/HSD/barcode/QR.

Khóa print payload do Ginsengfood sinh; máy in không tự sinh mã.

Khóa disposal/write-off cho batch/thành phẩm/bao bì lỗi, không xóa tay tồn kho.

Khóa actor/permission/audit/idempotency/evidence cho toàn bộ command rủi ro.

## 4. SOURCE-OF-TRUTH - THỨ BẬC NGUỒN SỰ THẬT

Tầng

Nguồn

Vai trò

Được dùng như thế nào

Không được làm

Tầng 0

MASTER Governance

Governance cao nhất

Giữ no-bypass, no-parallel-source, no-PASS-without-evidence

Không ghi đè bằng implementation cục bộ

Tầng 1

README Phase 2 V1.1

Điều phối Phase 2

Xác định scope, boundary, execution order

Không thay file chi tiết P2.2D

Tầng 2

P2.1 Technical Design V1.1

Thiết kế workstream

Cung cấp form registry, field behavior, data inheritance

Không tự cho phép code

Tầng 3

P2.2B Production Order / Dossier / Snapshot

Production root truth

Cung cấp Production Dossier và Formula Snapshot

Không lấy SKU/formula động thay dossier

Tầng 4

P2.2C Material Issue / Receipt / Ledger

Material movement truth

Cung cấp material issued, workshop receipt, raw ledger debit

Không tự tạo issue/debit trong P2.2D

Tầng 5

P2.2D V2.0 này

Nguồn chính cho Batch/QC/Release/Warehouse/Inventory

Giao dev/Agent phân tích/triển khai giới hạn khi được mở

Không lấn sang P2.2E/Commerce

Tầng 6

Phase 1 Packaging Profile / SKU / UOM

Nguồn cấu hình nền

Cung cấp packaging profile, UOM, SKU identity

Không sửa Phase 1 trong file này

Tầng 7

nguồn tổng hợp TÀI LIỆU.docx

Nguồn phụ trợ nghiệp vụ

Đối chiếu sơ chế/sấy/đóng gói/QC/nhập kho/print

Không code/seed trực tiếp

Tầng 8

Code hiện tại

Hiện trạng triển khai

Dùng để gap analysis

Không là source-of-truth nếu conflict

## 5. SCOPE IN - PHẠM VI BAO GỒM

Tài liệu này bao gồm:

Production Batch.

Batch code.

Batch linked Production Dossier.

Batch lifecycle.

Batch process state.

Workshop received prerequisite.

Check-in/check-out nhân sự nếu thuộc scope.

Phiếu sơ chế.

Phiếu sấy thăng hoa.

Phiếu QC sau sấy thăng hoa.

Phiếu đóng gói cấp 1.

Phiếu đóng gói cấp 2.

Phiếu QC thành phẩm.

Batch QC.

Batch Release Gate.

Batch release evidence.

Finished Goods Warehouse Receipt.

Warehouse receipt guard.

Finished Goods Ledger Credit.

Finished Goods Balance Projection.

No double credit.

Packaging profile consumption.

Packaging output tracking.

Print level 1 boundary.

Print level 2 boundary.

Print payload boundary.

Rejected / damaged / failed batch handling.

Disposal / write-off for batch/FG/packaging.

MISA accounting boundary for finished goods inventory.

Actor / permission / audit / idempotency / evidence.

Smoke/evidence requirements cho P2.2D.

Report 15 mục cho P2.2D.

## 6. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM

Tài liệu này không bao gồm:

Không triển khai Source / Raw Lot / QC đầu vào đầy đủ.

Không triển khai Production Order đầy đủ.

Không triển khai Formula Snapshot đầy đủ.

Không triển khai Material Request/Issue/Receipt đầy đủ.

Không triển khai Raw Inventory Ledger Debit.

Không triển khai Traceability đầy đủ.

Không triển khai QR/Public Trace đầy đủ.

Không triển khai Recall / Sale Lock đầy đủ.

Không triển khai Commerce Runtime.

Không quyết định Sellable.

Không tạo giá bán.

Không tạo QuoteSnapshot.

Không tạo Cart/Order/Payment.

Không triển khai AI Advisor runtime.

Không triển khai Facebook Gateway.

Không triển khai Ads/Live/CRM/IVR.

Không triển khai full MISA Integration.

Không cho MISA điều khiển batch/release/warehouse.

Không triển khai full MRP/procurement.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 7. ĐỊNH NGHĨA THUẬT NGỮ

Thuật ngữ

Định nghĩa

Production Batch

Mẻ sản xuất được tạo từ Production Dossier

Batch Process

Chuỗi công đoạn xử lý batch như sơ chế, sấy, đóng gói

Freeze-dry QC

QC sau công đoạn sấy thăng hoa

Packaging Level 1

Đóng gói cấp 1, theo nguồn tổng hợp chỉ in MFG/HSD

Packaging Level 2

Đóng gói cấp 2, in batch/MFG/HSD/barcode/QR

Batch QC

Kiểm tra chất lượng batch/thành phẩm

## QC_PASS

Batch đạt QC, nhưng chưa đồng nghĩa RELEASED

Batch Release

Quyết định phê duyệt batch để nhập kho thành phẩm

Warehouse Receipt

Lệnh nhập kho thành phẩm

Finished Goods Ledger Credit

Bút toán ghi tăng tồn thành phẩm

No Double Credit

Không ghi tăng tồn thành phẩm hai lần khi retry

Sellable

Được phép bán, thuộc Commerce Runtime, không thuộc P2.2D

Print Payload

Dữ liệu in do Ginsengfood sinh

Rejected Batch

Batch không đạt, không được release

Disposal / Write-off

Quy trình hủy/ghi giảm có kiểm soát

## PHẦN 2/4 - BATCH / PROCESS / PACKAGING / QC MODEL

## 8. LUỒNG NGHIỆP VỤ TỔNG QUAN P2.2D

Luồng nghiệp vụ P2.2D:

Production Dossier -> Workshop Receipt Confirmed -> Production Batch -> Batch Process -> Sơ chế -> Sấy thăng hoa -> QC sau sấy -> Đóng gói cấp 1 -> Đóng gói cấp 2 -> QC thành phẩm -> Batch QC -> Batch Release -> Warehouse Receipt -> Finished Goods Ledger Credit

Trong đó:

Batch phải linked Production Dossier.

Batch không được tạo trôi nổi.

Batch phải dựa trên SKU, formula version, material genealogy.

Batch Process phải dựa trên dữ liệu từ batch/dossier.

Các phiếu sơ chế/sấy/đóng gói/QC không được nhập lại SKU/formula.

Batch QC_PASS chưa phải RELEASED.

Batch Release là bước riêng.

Warehouse chỉ nhận batch đã RELEASED.

Finished Goods Ledger chỉ credit khi Warehouse Receipt confirmed.

Warehouse Receipt không tự set Sellable.

Print cấp 1/cấp 2 chỉ dùng payload do Ginsengfood sinh.

P2.2D không public trace, không QR full, không sale lock full; các phần đó thuộc P2.2E.

## 9. PRODUCTION BATCH MODEL

## 9.1. Nguyên tắc

Production Batch là mẻ sản xuất phát sinh từ Production Dossier.

Batch không được tạo nếu:

Không có Production Dossier.

Production Dossier bị cancelled/bị chặn.

Production Dossier thiếu formula snapshot.

Production Dossier chưa có material issue/receipt prerequisite nếu policy yêu cầu.

Actor không có quyền.

Evidence/approval chưa đủ nếu policy yêu cầu.

## 9.2. Checklist Production Batch Header

Trường

Bắt buộc

Quy tắc

batch_code

Có

Mã batch duy nhất

production_dossier_code

Có

Trỏ hồ sơ sản xuất gốc

production_order_code

Có

dựa trên từ dossier

sku_code

Có

dựa trên

sku_name_snapshot

Có

dựa trên

formula_code

Có

dựa trên snapshot

formula_version

Có

dựa trên snapshot

formula_snapshot_ref

Có

Trỏ snapshot

material_issue_ref

Có nếu policy yêu cầu

Trỏ issue đã cấp

workshop_receipt_ref

Có nếu policy yêu cầu

Trỏ xác nhận xưởng nhận

planned_output_quantity

Có

Sản lượng dự kiến

planned_output_uom

Có

Đơn vị

actual_output_quantity

Tùy theo process

Cập nhật sau

batch_status

Có

Theo lifecycle

production_site

Tùy

Xưởng/khu vực

started_at

Tùy

Khi bắt đầu

completed_at

Tùy

Khi hoàn tất

created_by

Có

Actor

created_at

Có

Thời điểm

audit_log

Có

Bắt buộc

idempotency_key

Có nếu command có side effect

Chống tạo trùng batch

## 9.3. Batch Status Lifecycle

Status

Ý nghĩa

Ghi chú

## CREATED

Batch đã tạo

Chưa xử lý

## MATERIAL_READY

Nguyên liệu sẵn sàng theo prerequisite

Không đồng nghĩa đã sản xuất xong

## IN_PROCESS

Đang sản xuất

Có thể có nhiều công đoạn

## PRE_PROCESS_COMPLETED

Sơ chế hoàn tất

Nếu tách bước

## FREEZE_DRYING

Đang sấy thăng hoa

Nếu có bước

## FREEZE_DRYING_COMPLETED

Sấy hoàn tất

Chưa QC sau sấy

## POST_DRY_QC_WAITING

Chờ QC sau sấy

Không release

## POST_DRY_QC_PASSED

QC sau sấy đạt

Chưa finished QC/release

## PACKAGING_LEVEL_1

Đang đóng gói cấp 1

Chưa release

## PACKAGING_LEVEL_2

Đang đóng gói cấp 2

Chưa release

## FINISHED_QC_WAITING

Chờ QC thành phẩm

Chưa release

## QC_PASSED_WAITING_RELEASE

QC đạt, chờ release

Chưa nhập kho

## RELEASED

Đã phê duyệt release

Có thể nhập kho

## HOLD

Tạm giữ

Không nhập kho

## REJECTED

Không đạt

Không nhập kho

## DISPOSAL_WAITING

Chờ hủy/write-off

Không nhập kho

## DISPOSED

Đã hủy/write-off

Không nhập kho

## CANCELLED

Hủy

Không tiếp tục

bị chặn

Bị chặn

Cần xử lý

## 9.4. Fail gate Batch

Fail nếu:

Batch không linked Production Dossier.

Batch không linked formula snapshot.

Batch không linked SKU.

Batch tự nhập SKU/formula khác dossier.

Batch tạo khi dossier cancelled/bị chặn.

Batch tạo khi chưa có prerequisite material theo policy.

Batch tự QC_PASS.

Batch tự RELEASED.

Batch tự Warehouse Receipt.

Batch tự Sellable.

## 10. BATCH PROCESS / SƠ CHẾ / SẤY THĂNG HOA

## 10.1. Nguyên tắc

Batch Process là chuỗi công đoạn xử lý batch.

Các công đoạn phải dựa trên dữ liệu từ Production Batch và Production Dossier.

Không được nhập lại:

## SKU.

Formula code.

Formula version.

Ingredient list.

Batch identity.

Production dossier identity.

## 10.2. Process Step Registry

Step

Tên công đoạn

Nguồn dữ liệu

Được nhập tay

Không được

## PROCESS-01

Check-in nhân sự/kíp sản xuất nếu áp dụng

Batch / user

Actor/time

Không thay batch

## PROCESS-02

Sơ chế

Batch / dossier

Kết quả sơ chế, actual quantity, loss reason

Không đổi formula

## PROCESS-03

Sấy thăng hoa

Batch / dossier

Thời gian, điều kiện, output sau sấy

Không tự QC_PASS

## PROCESS-04

QC sau sấy

Batch / process

QC result/evidence

Không tự RELEASED

## PROCESS-05

Đóng gói cấp 1

Batch / packaging profile

Actual quantity/loss reason

Không tự sinh QR/barcode

## PROCESS-06

Đóng gói cấp 2

Batch / packaging profile

Actual quantity/loss reason

Không để máy in tự sinh mã

## PROCESS-07

QC thành phẩm

Batch / packaging output

QC result/evidence

Không tự warehouse receipt

## PROCESS-08

Release review

Batch QC

Release decision

Không auto release từ QC_PASS nếu chưa gate

## 10.3. Checklist Batch Process Step

Trường

Bắt buộc

Quy tắc

process_step_code

Có

Mã công đoạn

batch_code

Có

Trỏ batch

production_dossier_code

Có

dựa trên

step_type

Có

## PRE_PROCESS / FREEZE_DRY / POST_DRY_QC / PACKAGING_L1 / PACKAGING_L2 / FINISHED_QC

step_status

Có

## WAITING / IN_PROGRESS / COMPLETED / FAILED / HOLD

input_quantity

Tùy

Nếu cần

output_quantity

Tùy

Nếu cần

loss_quantity

Tùy

Nếu có hao hụt

loss_reason

Có nếu có loss

Bắt buộc nếu loss

started_by

Có nếu start

Actor

started_at

Có nếu start

Thời điểm

completed_by

Có nếu complete

Actor

completed_at

Có nếu complete

Thời điểm

evidence_ref

Có nếu policy yêu cầu

Evidence

audit_log

Có

Bắt buộc

## 10.4. Fail gate Batch Process

Fail nếu:

Công đoạn không linked batch.

Công đoạn nhập lại SKU/formula.

Sấy hoàn tất tự QC_PASS.

QC sau sấy tự RELEASED.

Process step thiếu actor/audit.

Loss không có reason nếu policy yêu cầu.

Batch process tự nhập kho thành phẩm.

Batch process tự set sellable.

## 11. PACKAGING LEVEL 1 / LEVEL 2

## 11.1. Nguyên tắc

nguồn tổng hợp đã khóa:

In mã sản phẩm chia 2 cấp.

Cấp 1 chỉ in MFG/HSD.

Cấp 2 in batch/MFG/HSD/barcode/QR.

Hệ thống Ginsengfood sinh dữ liệu in.

Máy in chỉ nhận payload, in và trả trạng thái.

Máy in không được tự sinh mã sản xuất, mã lô, ngày sản xuất, hạn dùng, barcode, QR.

Trong P2.2D, phạm vi chính là packaging và print boundary.

Chi tiết QR/Public Trace/Recall/Sale Lock sẽ được khóa sâu hơn ở P2.2E.

## 11.2. Packaging Profile Consumption

P2.2D tiêu thụ Packaging Profile từ Phase 1.

Packaging profile phải có:

SKU code.

Level 1 unit.

B1 material nếu áp dụng.

Units per box nếu có hộp.

Boxes per carton nếu có thùng.

Label template level 1 reference.

Label template level 2 reference.

Barcode required flag.

QR required flag.

Owner confirmed.

## 11.3. Packaging Level 1

Trường

Bắt buộc

Quy tắc

packaging_l1_code

Có

Mã đóng gói cấp 1

batch_code

Có

Trỏ batch

packaging_profile_code

Có

dựa trên profile

mfg_date

Có

Do Ginsengfood sinh

hsd_date

Có

Do Ginsengfood sinh

level_1_unit_quantity

Có

Số gói/đơn vị cấp 1

label_template_ref

Có

Template cấp 1

print_payload_ref

Có nếu in

Payload cấp 1

actual_output_quantity

Có

Số lượng thực tế

reject_quantity

Tùy

Nếu có

reject_reason

Có nếu reject

Bắt buộc

packaging_status

Có

## WAITING / IN_PROGRESS / COMPLETED / FAILED / HOLD

actor

Có

Người thực hiện

audit_log

Có

Bắt buộc

Rule cấp 1:

Print cấp 1 chỉ in MFG/HSD.

Nếu sau này Owner muốn in thêm trường ở cấp 1, phải có tài liệu clean update riêng, không được để máy in hoặc dev tự thêm.

## 11.4. Packaging Level 2

Trường

Bắt buộc

Quy tắc

packaging_l2_code

Có

Mã đóng gói cấp 2

batch_code

Có

Trỏ batch

packaging_profile_code

Có

dựa trên profile

batch_code_print

Có

Do Ginsengfood sinh

mfg_date

Có

Do Ginsengfood sinh

hsd_date

Có

Do Ginsengfood sinh

barcode_value

Có nếu required

Do Ginsengfood sinh

qr_value

Có nếu required

Do Ginsengfood sinh hoặc QR service sau này

label_template_ref

Có

Template cấp 2

print_payload_ref

Có nếu in

Payload cấp 2

box_quantity

Có nếu đóng hộp

Theo profile

carton_quantity

Có nếu đóng thùng

Theo profile

actual_output_quantity

Có

Số lượng thực tế

reject_quantity

Tùy

Nếu có

reject_reason

Có nếu reject

Bắt buộc

packaging_status

Có

## WAITING / IN_PROGRESS / COMPLETED / FAILED / HOLD

actor

Có

Người thực hiện

audit_log

Có

Bắt buộc

Rule cấp 2:

Print cấp 2 in batch/MFG/HSD/barcode/QR.

## 11.5. Print Boundary trong P2.2D

P2.2D không triển khai full QR/Public Trace, nhưng phải giữ print boundary:

Nhóm

Rule

Payload

Ginsengfood sinh

Máy in

Chỉ nhận payload, in, trả trạng thái

Cấp 1

Chỉ MFG/HSD

Cấp 2

Batch/MFG/HSD/barcode/QR

Barcode

Không để máy in tự sinh

## QR

Không để máy in tự sinh

Reprint

Không xử lý sâu ở P2.2D, nhưng phải có boundary chuyển P2.2E

Failed print

Không public-valid

Void print

Không public-valid

## 11.6. Fail gate Packaging / Print

Fail nếu:

Packaging không linked batch.

Packaging nhập lại SKU/formula.

Print cấp 1 in sai rule MFG/HSD.

Print cấp 2 thiếu batch/MFG/HSD/barcode/QR.

Máy in tự sinh MFG/HSD.

Máy in tự sinh barcode.

Máy in tự sinh QR.

Packaging completed tự release batch.

Packaging completed tự warehouse receipt.

Packaging completed tự sellable.

## 12. BATCH QC / FINISHED QC

## 12.1. Nguyên tắc

Batch QC là bước kiểm chất lượng batch/thành phẩm.

Batch QC có thể PASS, FAIL, HOLD, RETEST, REJECT.

Nhưng:

Batch QC_PASS không đồng nghĩa RELEASED.

QC_PASS chỉ có nghĩa batch có thể được xét release.

Release là gate riêng.

## 12.2. Checklist Batch QC

Trường

Bắt buộc

Quy tắc

batch_qc_code

Có

Mã QC batch

batch_code

Có

Trỏ batch

production_dossier_code

Có

dựa trên

qc_stage

Có

## POST_DRY_QC / FINISHED_QC / OTHER

qc_result

Có

PASS / FAIL / HOLD / RETEST / REJECT

qc_checked_by

Có

Actor QC

qc_checked_at

Có

Thời điểm

qc_criteria

Có nếu policy yêu cầu

Tiêu chí

qc_evidence

Có nếu policy yêu cầu

Evidence

accepted_quantity

Có nếu pass

Số lượng đạt

rejected_quantity

Có nếu fail/reject

Số lượng không đạt

qc_note

Tùy

Ghi chú

audit_log

Có

Bắt buộc

## 12.3. QC Result Mapping

QC Result

Batch status sau QC

Được nhập kho không?

## QC_PASSED_WAITING_RELEASE

Không, cần release

HOLD / REJECTED theo rule

Không

## HOLD

## HOLD

Không

## RETEST

## QC_RETEST_REQUIRED

Không

## REJECT

## REJECTED

Không

## PARTIAL_PASS

Cần split/accepted quantity theo rule

Chỉ phần được release nếu policy cho phép

## 12.4. Fail gate Batch QC

Fail nếu:

Batch QC_PASS tự set RELEASED.

Batch QC_PASS tự Warehouse Receipt.

Batch QC_FAIL vẫn release được.

Batch HOLD vẫn release được.

Batch QC thiếu actor.

Batch QC thiếu evidence khi policy yêu cầu.

Batch QC tự tạo Finished Goods Ledger Credit.

QC result bị dùng làm sellable.

## PHẦN 3/4 - BATCH RELEASE / WAREHOUSE / FG LEDGER / DISPOSAL / MISA

## 13. BATCH RELEASE GATE

## 13.1. Nguyên tắc

Batch Release là quyết định riêng, tách khỏi Batch QC.

Một batch chỉ được RELEASED nếu:

Batch tồn tại.

Batch linked Production Dossier.

Batch có process evidence theo policy.

Batch không HOLD.

Batch không REJECTED.

Batch không CANCELLED.

Batch không DISPOSAL_WAITING.

Required evidence đã ACCEPTED nếu policy yêu cầu.

Actor có quyền release.

Audit được ghi.

Idempotency guard pass.

## 13.2. Checklist Batch Release

Trường

Bắt buộc

Quy tắc

batch_release_code

Có

Mã release

batch_code

Có

Trỏ batch

production_dossier_code

Có

dựa trên

qc_record_ref

Có

Trỏ QC PASS

release_decision

Có

RELEASED / REJECTED / HOLD / bị chặn

release_reason

Có nếu hold/reject/block

Lý do

released_quantity

Có nếu released

Số lượng release

released_uom

Có

Đơn vị

released_by

Có

Actor có quyền

released_at

Có

Thời điểm

evidence_status

Có nếu policy yêu cầu

## ACCEPTED / SUBMITTED / MISSING

audit_log

Có

Bắt buộc

idempotency_key

Có

Chống duplicate release

## 13.3. Release không được làm

Batch Release không được:

Tự tạo Warehouse Receipt nếu chưa có workflow riêng.

Tự tạo Finished Goods Ledger Credit.

Tự set SKU Sellable.

Tự tạo QR public-valid.

Tự tạo Commerce stock.

Tự tạo Order/Revenue.

Tự bypass Recall/Sale Lock.

## 13.4. Fail gate Batch Release

Fail nếu:

Batch QC_PASS tự RELEASED.

Release batch QC_FAIL.

Release batch HOLD/REJECTED.

Release thiếu actor/permission.

Release thiếu evidence accepted nếu policy yêu cầu.

Release tạo Finished Goods Ledger Credit trực tiếp.

Release tự set Sellable.

## 14. WAREHOUSE RECEIPT THÀNH PHẨM

## 14.1. Nguyên tắc

Warehouse Receipt là bước nhập kho thành phẩm.

Warehouse chỉ được nhận:

Batch RELEASED

Không được nhận:

Batch QC_PASS nhưng chưa release.

Batch HOLD.

Batch REJECTED.

Batch CANCELLED.

Batch DISPOSAL_WAITING.

Batch thiếu release evidence.

## 14.2. Checklist Finished Goods Warehouse Receipt

Trường

Bắt buộc

Quy tắc

fg_warehouse_receipt_code

Có

Mã phiếu nhập kho thành phẩm

batch_code

Có

Trỏ batch

batch_release_code

Có

Trỏ release

production_dossier_code

Có

dựa trên

sku_code

Có

dựa trên

sku_name_snapshot

Có

dựa trên

received_quantity

Có

Số lượng nhập

received_uom

Có

Đơn vị

warehouse_location

Có

Kho thành phẩm

receipt_status

Có

DRAFT / CONFIRMED / CANCELLED / bị chặn

received_by

Có khi confirmed

Actor kho

received_at

Có khi confirmed

Thời điểm

evidence_ref

Tùy policy

Evidence

idempotency_key

Có

Chống double credit

audit_log

Có

Bắt buộc

## 14.3. Warehouse Receipt Guard

Warehouse Receipt chỉ confirmed khi pass:

Guard

Điều kiện

Batch exists

Batch tồn tại

Batch released

batch_status = RELEASED

Release record valid

Có batch_release_code

Not hold/rejected/cancelled

Batch không bị chặn

Quantity valid

Quantity hợp lệ

UOM valid

UOM hợp lệ

Actor valid

Actor có quyền nhập kho

Idempotency valid

Không double credit

Evidence valid

Nếu policy yêu cầu

Audit ready

Có audit/correlation

## 14.4. Warehouse Receipt không được làm

Warehouse Receipt không được:

Nhận batch chưa release.

Tự release batch.

Tự sửa QC result.

Tự sửa production dossier.

Tự set SKU Sellable.

Tự public trace.

Tự tạo Commerce stock.

Tự tạo order/revenue.

Tự bypass recall/sale lock.

Tự sửa ledger cũ.

## 15. FINISHED GOODS LEDGER CREDIT

## 15.1. Nguyên tắc

Finished Goods Ledger Credit chỉ được tạo khi:

Warehouse Receipt confirmed

Không tạo FG ledger credit khi:

Batch created.

Batch process completed.

Batch QC_PASS.

Batch RELEASED nhưng chưa warehouse receipt.

Packaging completed.

Print completed.

QR generated.

## 15.2. Checklist FG Ledger Credit

Trường

Bắt buộc

Quy tắc

fg_ledger_entry_code

Có

Mã ledger

ledger_type

Có

## FINISHED_GOODS

direction

Có

## CREDIT

source_command

Có

## WAREHOUSE_RECEIPT

source_command_code

Có

fg_warehouse_receipt_code

batch_code

Có

Trỏ batch

sku_code

Có

Thành phẩm

quantity

Có

Số lượng credit

uom

Có

Đơn vị

warehouse_location

Có

Kho thành phẩm

actor

Có

Người/system

occurred_at

Có

Thời điểm

idempotency_key

Có

Chống double credit

correlation_id

Có

Truy vết

audit_log

Có

Bắt buộc

## 15.3. No Double Credit

Case

Expected

Retry Warehouse Receipt same key/same payload

Không tạo credit lần hai

Same key/different payload

Conflict

Warehouse Receipt failed

Không tạo credit

Warehouse Receipt draft

Không tạo credit

Warehouse Receipt cancelled

Không tạo credit

Batch release retry

Không tạo FG credit nếu chưa warehouse receipt

Manual ledger credit không source command

Bị chặn

## 15.4. Fail gate FG Ledger

Fail nếu:

Batch QC_PASS tạo FG credit.

Batch RELEASED tạo FG credit trước warehouse receipt.

Packaging completed tạo FG credit.

Warehouse Receipt retry tạo double credit.

Ledger update/delete được.

FG Ledger credit tự set sellable.

MISA tạo/sửa FG ledger vận hành.

## 16. FINISHED GOODS BALANCE PROJECTION

## 16.1. Nguyên tắc

Finished Goods Balance Projection là kết quả tính từ Finished Goods Ledger, không phải nguồn sự thật duy nhất.

Nguồn biến động là ledger.

Projection có thể dùng để hiển thị tồn kho, nhưng không tự quyết sellable.

## 16.2. Projection rule

Rule

Diễn giải

Projection tính từ ledger

Không sửa tay tùy tiện

Warehouse Receipt confirmed tạo credit

Tăng projection

Correction qua ledger

Không sửa projection trực tiếp

Projection không tự sellable

Sellable thuộc Phase 3

Projection không bỏ qua sale lock

Phase sau phải kiểm sale lock

Projection không thay release

Chỉ là tồn kho, không là quality release

## 16.3. Fail gate projection

Fail nếu:

Projection tăng trước warehouse receipt.

Projection sửa tay không dựa ledger.

Projection tự set sellable.

Projection bỏ qua recall/sale lock.

MISA sync sửa projection vận hành.

## 17. DISPOSAL / WRITE-OFF CHO BATCH / THÀNH PHẨM / BAO BÌ

## 17.1. Nguyên tắc

Nếu batch, thành phẩm hoặc bao bì bị hỏng, lỗi, reject, hết hạn, không đạt QC, hoặc cần hủy, không được xóa tay tồn kho hoặc batch.

Flow bắt buộc:

Disposal Request -> QA/Owner Review -> Disposal Approved/Rejected -> Disposal Executed -> Inventory Ledger Write-off -> Evidence/Audit

## 17.2. Tình huống kích hoạt disposal

Tình huống

Kích hoạt disposal?

Batch QC_FAIL

Có thể, theo decision

Batch REJECTED

Có

Thành phẩm lỗi sau đóng gói

Có

Bao bì hỏng trong đóng gói

Có nếu thuộc inventory

Hàng thành phẩm hết hạn

Có

Hàng thành phẩm bị recall và cần hủy

Có

Batch HOLD

Chưa, nếu chưa có quyết định hủy

Batch cần retest

Chưa, nếu chưa reject

## 17.3. Checklist disposal

Trường

Bắt buộc

Quy tắc

disposal_request_code

Có

Mã yêu cầu

item_type

Có

## BATCH / FINISHED_GOODS / PACKAGING

batch_code

Có nếu liên quan batch

Trỏ batch

sku_code

Có nếu thành phẩm

Trỏ SKU

quantity

Có

Số lượng hủy

uom

Có

Đơn vị

reason_code

Có

Lý do

requested_by

Có

Actor

qa_review_status

Có

## WAITING / APPROVED / REJECTED

owner_approval_status

Có nếu policy yêu cầu

## WAITING / APPROVED / REJECTED

executed_by

Có khi thực hiện

Actor

executed_at

Có khi thực hiện

Thời điểm

ledger_writeoff_ref

Có sau write-off

Ledger

evidence_ref

Có

Evidence

audit_log

Có

Bắt buộc

## 17.4. Fail gate disposal

Fail nếu:

Batch reject bị xóa.

Thành phẩm lỗi bị xóa khỏi tồn.

Bao bì hỏng bị xóa tay.

Disposal không có request.

Disposal không có QA/Owner review.

Disposal không có evidence.

Disposal không có write-off ledger.

Disposal làm mất trace batch.

Disposal bị MISA điều khiển trước operational record.

## 18. MISA / ACCOUNTING BOUNDARY

## 18.1. Nguyên tắc

P2.2D có thể tạo dữ liệu nhập kho thành phẩm và giá thành nội bộ để làm nền cho kế toán sau này.

Nhưng:

Ginsengfood sở hữu operational truth.

MISA hạch toán chính thức thông qua Integration Layer riêng.

MISA không điều khiển batch.

MISA không release batch.

MISA không confirm warehouse receipt vận hành.

MISA không tạo/sửa Finished Goods Ledger vận hành.

MISA không quyết định sellable.

MISA không sinh QR/print payload.

## 18.2. Accounting handoff fields

Trường

Có thể chuẩn bị

Không được

warehouse_receipt_ref

Có

Không để MISA tạo receipt

finished_goods_ledger_ref

Có

Không để MISA sửa ledger

sku_code

Có

Không dùng MISA để đổi SKU

batch_code

Có

Không dùng MISA để đổi batch

quantity/uom

Có

Không để MISA sửa quantity vận hành

cost_placeholder

Có nếu cần

Không hạch toán chính thức nếu chưa MISA phase

misa_mapping_status

Có

Không hardcode mapping

sync_status

Có

Sync fail không làm mất operational truth

## 18.3. Fail gate MISA

Fail nếu:

MISA release batch.

MISA confirm warehouse receipt.

MISA tạo FG ledger credit.

MISA sửa batch status.

MISA sửa warehouse receipt quantity.

MISA tự set sellable.

MISA tạo print/QR.

MISA sync fail rollback operational receipt.

PHẦN 4/4 - CROSS-CUTTING / HANDOFF / SMOKE / DONE GATE / FAIL GATE

## 19. ACTOR / PERMISSION / AUDIT / IDEMPOTENCY / EVIDENCE

## 19.1. Command rủi ro trong P2.2D

Command

Actor

Permission

Audit

Idempotency

Evidence

Create Batch

Bắt buộc

Bắt buộc

Bắt buộc

Bắt buộc

Dossier evidence

Start Batch Process

Bắt buộc

Bắt buộc

Bắt buộc

Bắt buộc

Process evidence nếu cần

Complete Process Step

Bắt buộc

Bắt buộc

Bắt buộc

Bắt buộc

Output/loss evidence

Submit Post-dry QC

QC actor

Bắt buộc

Bắt buộc

Bắt buộc

QC evidence

Complete Packaging L1

Actor đóng gói

Bắt buộc

Bắt buộc

Bắt buộc

Packaging evidence

Complete Packaging L2

Actor đóng gói

Bắt buộc

Bắt buộc

Bắt buộc

Packaging evidence

Submit Finished QC

QC actor

Bắt buộc

Bắt buộc

Bắt buộc

QC evidence

Release Batch

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Release evidence

Confirm Warehouse Receipt

Warehouse actor

Bắt buộc

Bắt buộc

Bắt buộc

Receipt evidence

Create FG Ledger Credit

System

Theo command

Bắt buộc

Bắt buộc

Ledger evidence

Create Disposal Request

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Disposal evidence

Execute Disposal/Write-off

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Write-off evidence

## 19.2. Idempotency rule

Case

Expected

Same key + same payload Create Batch

Không tạo batch trùng

Same key + different payload Create Batch

Conflict

Retry Batch Release same key

Không release trùng

Retry Warehouse Receipt same key

Không tạo double credit

Retry Packaging Completion same key

Không duplicate output

Retry Disposal same key

Không write-off trùng

Retry FG Ledger Credit after timeout

Trả kết quả cũ/no-op, không credit lần hai

## 19.3. Evidence rule

Nếu policy yêu cầu evidence accepted, thì:

Evidence SUBMITTED chưa được xem là pass.

Evidence UNDER_REVIEW chưa được xem là pass.

Evidence MISSING phải block.

Evidence REJECTED phải fail/retest.

Evidence ACCEPTED mới được dùng cho gate tương ứng.

Fail nếu evidence SUBMITTED bị dùng như ACCEPTED.

## 20. ENTITY / API / SERVICE BOUNDARY

## 20.1. Entity impact draft

Tài liệu này không tạo migration, nhưng implementation sau này có thể cần rà soát các nhóm entity:

Nhóm entity

Vai trò

production_batch

Batch sản xuất

batch_status_history

Lịch sử trạng thái batch

batch_process_step

Công đoạn sản xuất

batch_personnel_log

Check-in/check-out nếu có

post_dry_qc

QC sau sấy

packaging_l1

Đóng gói cấp 1

packaging_l2

Đóng gói cấp 2

finished_qc

QC thành phẩm

batch_release

Release batch

fg_warehouse_receipt

Phiếu nhập kho thành phẩm

finished_goods_ledger

Ledger thành phẩm

fg_balance_projection

Projection tồn thành phẩm

print_payload_ref

Liên kết payload in

batch_disposal_request

Disposal batch/FG

evidence_ref

Evidence

audit_log

Audit

idempotency_record

Chống retry trùng

## 20.2. API/service boundary

Nhóm

Mục tiêu

Không được

Batch API

Tạo/đọc batch linked dossier

Không tạo batch trôi nổi

Batch Process API

Ghi công đoạn

Không đổi SKU/formula

Batch QC API

Ghi QC result

Không tự release

Batch Release API

Release nếu pass guard

Không warehouse receipt trực tiếp nếu chưa workflow

Warehouse Receipt API

Nhập kho batch released

Không nhận batch QC_PASS chưa release

FG Ledger Service

Ghi credit append-only

Không double credit

Packaging Service

Đóng gói cấp 1/2

Không tự sinh mã ngoài payload

Print Payload Service

Chuẩn bị payload

Không để printer sinh mã

Disposal Service

Request/review/write-off

Không xóa tay tồn

MISA Handoff Service

Chuẩn bị accounting handoff

Không để MISA mutate operational truth

Audit Service

Ghi audit

Không bỏ audit

Idempotency Service

Chống duplicate side effect

Không bỏ qua retry

## 21. EXECUTION ORDER P2.2D

Khi sau này được phép triển khai, P2.2D phải đi theo thứ tự:

Thứ tự

Việc cần làm

Output

Đọc README Phase 2 V1.1

Hiểu boundary Phase 2

Đọc P2.1 Technical Design V1.1

Hiểu form registry / field behavior

Đọc P2.2B V2.0

Hiểu Production Dossier / Snapshot

Đọc P2.2C V1.1

Hiểu Material Issue / Workshop Receipt

Đọc P2.2D V2.0 này

Hiểu Batch/QC/Release/Warehouse

Analysis code hiện tại

Gap matrix

Owner Confirm nếu thiếu dữ liệu

Owner confirm list

Technical implementation plan

Task breakdown

Limited implementation nếu được mở

Code trong scope

10

Smoke P2.2D

Smoke result

11

Evidence submission

Evidence package

12

Report 15 mục

Handoff update

Hiện tại trong cuộc làm tài liệu này:

Chỉ viết lại tài liệu sạch, chưa mở implementation.

## 22. DEV / CODEX / COPILOT HANDOFF

## 22.1. Mode mặc định

Khi giao file này cho Agent:

## MODE: LIMITED IMPLEMENTATION CHỈ KHI ĐÃ CÓ APPROVED ANALYSIS + TECHNICAL DESIGN

Nếu chưa có approval, phải dùng:

## MODE: ANALYSIS ONLY

## 22.2. Prompt đúng cho Agent

MODE hiện tại: ANALYSIS ONLY nếu chưa có Owner/dev lead approval.

Nhiệm vụ:

Lập Gap Matrix giữa tài liệu P2.2D và code hiện tại.

Liệt kê entity/table hiện có hoặc thiếu.

Liệt kê API/DTO/service/UI/worker bị ảnh hưởng.

Liệt kê nơi Batch không linked Production Dossier nếu có.

Liệt kê nơi Batch tự tạo từ Production Order không qua prerequisite nếu có.

Liệt kê nơi Batch QC_PASS đang bị dùng làm RELEASED nếu có.

Liệt kê nơi Warehouse Receipt nhận Batch chưa RELEASED nếu có.

Liệt kê nơi Finished Goods Ledger credit được tạo trước Warehouse Receipt confirmed nếu có.

Liệt kê nơi Warehouse Receipt tự set Sellable nếu có.

Liệt kê nơi retry Warehouse Receipt có thể double credit nếu có.

Liệt kê nơi print cấp 1/cấp 2 sai rule nếu có.

Liệt kê nơi máy in tự sinh mã nếu có.

Liệt kê nơi disposal batch/FG/packaging bị xóa tay nếu có.

Liệt kê nơi MISA mutate batch/warehouse/FG ledger nếu có.

Liệt kê risk P0/P1/P2.

Đề xuất implementation scope.

Đề xuất smoke/evidence.

Không sửa code nếu chưa có lệnh Limited Implementation riêng.

Cấm:

Không code trực tiếp từ nguồn tổng hợp.

Không tạo migration khi chưa duyệt.

Không seed dữ liệu khi chưa Owner confirm.

Không tự tạo batch trôi nổi.

Không auto release từ QC_PASS.

Không warehouse receipt batch chưa release.

Không double credit.

Không tự set Sellable.

Không triển khai Traceability/QR/Recall/Sale Lock đầy đủ.

Không gọi Gateway PASS.

Không gọi Production Readiness.

## 23. OWNER CONFIRM REQUIRED

Các điểm cần Owner/dev lead xác nhận trước implementation:

Nhóm

Nội dung cần xác nhận

Batch code

Quy tắc mã batch

Batch creation prerequisite

Có cần Workshop Receipt confirmed trước khi tạo batch không

Process steps

Có tách sơ chế/sấy/QC/đóng gói/QC thành phẩm thành phiếu riêng không

Personnel check-in/out

Có cần quản lý nhân sự theo batch không

Freeze-dry QC

Tiêu chí QC sau sấy

Finished QC

Tiêu chí QC thành phẩm

Batch release actor

Ai được release batch

Release evidence

Evidence nào phải accepted

Warehouse receipt actor

Ai được nhập kho thành phẩm

FG ledger

Ledger event naming

Packaging profile

Cách áp dụng units_per_box/boxes_per_carton

Print level 1

Template và field MFG/HSD

Print level 2

Template và field batch/MFG/HSD/barcode/QR

Printer status

Status WAITING/PRINTED/FAILED/VOID

Rejected batch

Cách xử lý batch reject/hold/retest

Disposal

Reason/approval/write-off

MISA handoff

Field accounting handoff cho thành phẩm

## RBAC

Ai được process/QC/release/warehouse/print/dispose

Idempotency

Command nào cần key

Audit

Audit field bắt buộc

## 24. STOP CONDITIONS

Dev/Agent phải dừng nếu gặp:

Mã

Điều kiện dừng

Hành động

## P2-2D-STOP-01

Chưa có Production Dossier

Dừng, quay lại P2.2B

## P2-2D-STOP-02

Chưa có Material Issue/Workshop Receipt prerequisite nếu policy yêu cầu

Dừng, quay lại P2.2C

## P2-2D-STOP-03

Batch không linked Production Dossier

Dừng P0

## P2-2D-STOP-04

Batch nhập lại SKU/formula khác dossier

Dừng P0

## P2-2D-STOP-05

Batch QC_PASS tự RELEASED

Dừng P0

## P2-2D-STOP-06

Batch QC_FAIL/HOLD vẫn release

Dừng P0

## P2-2D-STOP-07

Warehouse Receipt nhận batch chưa RELEASED

Dừng P0

## P2-2D-STOP-08

FG Ledger credit trước Warehouse Receipt confirmed

Dừng P0

## P2-2D-STOP-09

Retry Warehouse Receipt double credit

Dừng P0

## P2-2D-STOP-10

Warehouse Receipt tự set Sellable

Dừng P0

## P2-2D-STOP-11

Print cấp 1 sai rule MFG/HSD

Dừng P0

## P2-2D-STOP-12

Print cấp 2 thiếu batch/MFG/HSD/barcode/QR

Dừng P0

## P2-2D-STOP-13

Máy in tự sinh mã

Dừng P0

## P2-2D-STOP-14

MISA mutate batch/release/warehouse/FG ledger

Dừng P0

## P2-2D-STOP-15

Disposal xóa tay batch/FG/packaging

Dừng P0

## P2-2D-STOP-16

Command thiếu actor/audit

Dừng P0

## P2-2D-STOP-17

Evidence SUBMITTED được coi ACCEPTED

Dừng P0

## P2-2D-STOP-18

Agent chuẩn bị code khi chưa approval

Dừng

## P2-2D-STOP-19

Scope lấn sang Traceability/QR/Recall/Sale Lock đầy đủ

Dừng, chuyển đúng file P2.2E

## P2-2D-STOP-20

Có ý định gọi Gateway PASS/Production Readiness

Dừng

## 25. SMOKE REQUIREMENTS P2.2D

Mã smoke

Nội dung

Expected

## SMK-P2-2D-01

Tạo Batch từ Production Dossier hợp lệ

Batch được tạo

## SMK-P2-2D-02

Tạo Batch không có Production Dossier

Bị reject

## SMK-P2-2D-03

Batch dựa trên SKU/formula snapshot

Dữ liệu tự hiện

## SMK-P2-2D-04

Batch nhập SKU/formula khác dossier

Bị chặn

## SMK-P2-2D-05

Batch process sơ chế

Step linked batch

## SMK-P2-2D-06

Batch process sấy thăng hoa

Step linked batch, chưa QC_PASS

## SMK-P2-2D-07

QC sau sấy PASS

Chưa RELEASED

## SMK-P2-2D-08

Đóng gói cấp 1

Chỉ payload MFG/HSD

## SMK-P2-2D-09

Đóng gói cấp 2

Payload batch/MFG/HSD/barcode/QR

## SMK-P2-2D-10

Máy in tự sinh mã

Bị chặn

## SMK-P2-2D-11

Batch QC PASS

Batch QC_PASSED_WAITING_RELEASE

## SMK-P2-2D-12

Batch QC FAIL/HOLD/REJECT

Không release được

## SMK-P2-2D-13

Release batch QC_PASS + guard pass

Batch RELEASED

## SMK-P2-2D-14

Warehouse Receipt batch QC_PASS chưa RELEASED

Bị reject

## SMK-P2-2D-15

Warehouse Receipt batch RELEASED

Receipt confirmed

## SMK-P2-2D-16

Warehouse Receipt confirmed

Tạo FG Ledger Credit

## SMK-P2-2D-17

Retry Warehouse Receipt same key/payload

Không double credit

## SMK-P2-2D-18

Same key different payload

Conflict

## SMK-P2-2D-19

Warehouse Receipt confirmed

Không tự set Sellable

## SMK-P2-2D-20

FG Ledger update/delete attempt

Bị chặn

## SMK-P2-2D-21

Disposal batch/FG reject

Có request/review/write-off

## SMK-P2-2D-22

Xóa tay batch/FG/packaging

Bị chặn

## SMK-P2-2D-23

MISA mutate batch/warehouse/ledger

Bị chặn

## SMK-P2-2D-24

Evidence required nhưng SUBMITTED

Không pass nếu policy cần ACCEPTED

## SMK-P2-2D-25

Command thiếu actor/permission

## DENY/BLOCK

## 26. EVIDENCE REQUIREMENTS P2.2D

Mã evidence

Nội dung evidence

## EVD-P2-2D-BATCH

Batch linked Production Dossier

## EVD-P2-2D-BATCH-INHERITANCE

Batch dựa trên SKU/formula/material genealogy

## EVD-P2-2D-PROCESS

Sơ chế/sấy/process step evidence

## EVD-P2-2D-POST-DRY-QC

QC sau sấy evidence

## EVD-P2-2D-PACKAGING-L1

Packaging/print level 1 payload

## EVD-P2-2D-PACKAGING-L2

Packaging/print level 2 payload

## EVD-P2-2D-PRINTER-BOUNDARY

Máy in không tự sinh mã

EVD-P2-2D-BATCH-QC-PASS-NOT-RELEASED

Batch QC_PASS chưa RELEASED

## EVD-P2-2D-BATCH-RELEASE

Release gate evidence

## EVD-P2-2D-WAREHOUSE-ONLY-RELEASED

Warehouse chỉ nhận batch RELEASED

## EVD-P2-2D-FG-LEDGER-CREDIT

Warehouse confirmed tạo FG Ledger Credit

## EVD-P2-2D-NO-DOUBLE-CREDIT

Retry không double credit

## EVD-P2-2D-NO-SELLABLE

Warehouse/FG inventory không tự sellable

## EVD-P2-2D-DISPOSAL

Disposal/write-off batch/FG/packaging

## EVD-P2-2D-MISA-BOUNDARY

MISA không mutate operational truth

## EVD-P2-2D-AUDIT

Actor/permission/audit evidence

## EVD-P2-2D-IDEMPOTENCY

Idempotency evidence

EVD-P2-2D-GATEWAY-bị chặn

Gateway vẫn bị chặn

Evidence Submitted chưa phải Evidence Accepted.

## 27. RISK REGISTER P2.2D

Risk

Mức độ

Nguyên nhân

Kiểm soát

Batch không linked dossier

## P0

Tạo batch trôi nổi

Batch creation guard

Batch QC_PASS = RELEASED

## P0

Gộp QC và release

Tách QC/release state

Warehouse nhận batch chưa release

## P0

Warehouse guard thiếu

Release validation

FG credit trước warehouse receipt

## P0

Ledger trigger sai

Credit only on confirmed receipt

Double credit

## P0

Retry thiếu idempotency

Idempotency key

Warehouse tự sellable

## P0

Lấn sang Commerce

No sellable boundary

Print cấp 1/cấp 2 sai

## P0

Print boundary thiếu

Payload rules

Máy in tự sinh mã

## P0

Printer integration sai

Ginsengfood payload only

Disposal xóa tay

## P0

Thiếu write-off flow

Disposal boundary

MISA mutate operational truth

## P0

Boundary sai

Handoff-only

Missing audit

## P1

Command thiếu actor/correlation

Audit required

Evidence submitted được pass

## P0

Sai evidence state

Evidence state guard

## 28. REPORT FORMAT 15 MỤC CHO P2.2D

Mọi report P2.2D phải có đủ 15 mục:

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

Nếu thiếu 15 mục, không được chuyển sang P2.2E.

## 29. DONE GATE P2.2D

P2.2D được xem là đạt ở mức handoff/implementation report khi:

Batch linked Production Dossier.

Batch dựa trên SKU/formula snapshot/material genealogy.

Batch không nhập lại SKU/formula khác dossier.

Batch process steps linked batch.

Sơ chế/sấy/đóng gói/QC dựa trên batch/dossier.

QC sau sấy không tự release.

Packaging cấp 1 đúng boundary MFG/HSD.

Packaging cấp 2 đúng boundary batch/MFG/HSD/barcode/QR.

Máy in không tự sinh mã.

Batch QC_PASS chưa RELEASED.

Batch Release là gate riêng.

Batch QC_FAIL/HOLD/REJECTED không release được.

Warehouse chỉ nhận batch RELEASED.

Warehouse Receipt confirmed tạo FG Ledger Credit.

Retry Warehouse Receipt không double credit.

FG Ledger append-only.

Warehouse Receipt không tự set Sellable.

Finished Goods Projection không tự sellable.

Disposal/write-off không xóa tay batch/FG/packaging.

MISA không mutate batch/release/warehouse/ledger.

Actor/permission/audit/idempotency/evidence được kiểm soát.

Có smoke result.

Có evidence package.

Có report 15 mục.

Không lấn sang Traceability/QR/Recall/Sale Lock đầy đủ.

Không lấn sang Commerce.

Gateway vẫn bị chặn.

Không gọi Completion Decision.

Không gọi Production Readiness.

## 30. FAIL GATE P2.2D

P2.2D fail nếu xảy ra một trong các trường hợp:

Batch không linked Production Dossier.

Batch không linked formula snapshot.

Batch nhập lại SKU/formula khác dossier.

Batch tạo khi prerequisite material/workshop chưa đạt theo policy.

Batch process tự sửa formula.

Batch process thiếu actor/audit.

Batch QC_PASS tự RELEASED.

Batch QC_FAIL/HOLD/REJECTED vẫn release được.

Batch Release thiếu actor/permission/evidence.

Batch Release tự tạo FG Ledger Credit.

Warehouse Receipt nhận batch chưa RELEASED.

Warehouse Receipt tự release batch.

Warehouse Receipt tạo FG Ledger Credit trước confirmed.

Warehouse Receipt retry tạo double credit.

Finished Goods Ledger update/delete được.

Warehouse Receipt tự set Sellable.

FG balance projection tự set sellable.

Print cấp 1 in sai rule.

Print cấp 2 thiếu batch/MFG/HSD/barcode/QR.

Máy in tự sinh mã.

Packaging completed tự sellable.

Disposal bằng cách xóa batch/FG/packaging.

Disposal không evidence/audit.

MISA release batch.

MISA confirm warehouse receipt.

MISA sửa FG ledger.

Evidence Submitted bị gọi Accepted.

Lấn sang QR/Public Trace/Recall/Sale Lock đầy đủ.

Lấn sang Commerce Runtime.

Gọi Gateway PASS.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

Hạng mục

Trạng thái

Documentation review

## READY FOR OWNER REVIEW

Phase

## PHASE 2 - OPERATIONAL CORE

Workstream

P2.2D - Batch / QC / Release / Warehouse / Inventory

Implementation authorization by this document

KHONG

Code authorization by this document

KHONG

Migration authorization by this document

KHONG

Seed authorization by this document

KHONG

Limited implementation authorization

Chỉ khi Owner/dev lead mở riêng

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
