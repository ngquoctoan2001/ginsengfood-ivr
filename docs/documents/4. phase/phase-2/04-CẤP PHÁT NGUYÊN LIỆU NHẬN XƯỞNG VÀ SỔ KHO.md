# 04-P2 - CẤP PHÁT NGUYÊN LIỆU NHẬN XƯỞNG VÀ SỔ KHO

Trạng thái: `MATERIAL_ISSUE_RECEIPT_LEDGER_CANONICAL`  
Mode: `LIMITED IMPLEMENTATION HANDOFF`  
Mục tiêu: khóa phiếu đề nghị cấp nguyên liệu, chấp thuận nguyên liệu, xuất kho, nhận xưởng và ledger.

## 1. Material request

Phiếu đề nghị cấp nguyên liệu sinh từ Production BOM Snapshot. Dòng phiếu phải tự hiện nguyên liệu, UOM, quantity required, material group và formula refs.

## 2. Material approval

Phiếu chấp thuận nguyên liệu khóa raw lot nào được phép đưa vào sản xuất. Lot phải `READY_FOR_PRODUCTION`; hold/quarantine/rejected/expired/damaged/disposed bị block.

## 3. Material issue

Material Issue là điểm giảm tồn nguyên liệu chính:

1. Ghi raw inventory ledger debit append-only.
2. Có idempotency key để không double debit.
3. Có actor, approval, evidence.
4. Có accounting value nếu scope kế toán cần.

## 4. Workshop receipt

Workshop receipt xác nhận xưởng nhận nguyên liệu đã issue, không giảm tồn lần hai. Chênh lệch phải có variance reason và owner approval nếu vượt policy.

## 5. Accounting handoff

Phiếu kế toán xuất nguyên liệu lấy từ material issue/ledger. Phase 2 chỉ chuẩn bị handoff/checkpoint, không sync MISA thật khi PACK-04 chưa mở.

## 6. Field phiếu đề nghị cấp nguyên liệu

| Nhóm field | Field bắt buộc | Rule |
| --- | --- | --- |
| Header | material_request_code, production_dossier_id, production_order_id, requested_by, requested_at, factory, warehouse | Không tạo rời ngoài dossier |
| Snapshot refs | snapshot_id, formula_id, formula_version, sku_id, product_name_snapshot | Tự hiện từ dossier |
| Dòng nguyên liệu | material_id, material_name_snapshot, material_group, required_qty, uom, requested_qty, note | Tự hiện từ BOM snapshot |
| Availability | available_ready_qty, allocated_qty, shortage_qty, lot_candidate_count | Tính từ raw lot ready |
| Kế toán tham chiếu | current_avg_cost, estimated_issue_value | Chỉ là dự kiến đến khi issue |
| Xác nhận | requester, warehouse, factory_manager | Lưu role/actor/time |

## 7. Field phiếu chấp thuận nguyên liệu

| Nhóm field | Field bắt buộc | Rule |
| --- | --- | --- |
| Header | material_approval_code, request_id, approver, approval_time, approval_status | Approval riêng, không auto-pass |
| Lot selection | raw_lot_id, lot_code, expiry_date, readiness_state, qc_result, usable_qty | Chỉ lot `READY_FOR_PRODUCTION` |
| Approved qty | approved_qty, shortage_qty, variance_reason | Không vượt usable qty |
| Block reason | hold/quarantine/rejected/expired/damaged/disposed/insufficient_qty | Bắt buộc ghi nếu không cấp |
| Accounting | avg_cost, approved_issue_value | Chuẩn bị cho material issue |
| Evidence | QC refs, receipt refs, approval signature | Thiếu evidence thì block |

## 8. Inventory ledger nguyên liệu

| Ledger event | Tăng/Giảm | Source | Idempotency |
| --- | --- | --- | --- |
| `RAW_RECEIPT_ACCEPTED` | Tăng tồn nguyên liệu | Phiếu nhập nguyên liệu | receipt_line_id |
| `RAW_RECEIPT_REJECTED` | Không tăng usable | QC đầu vào | receipt_line_id + qc_result |
| `RAW_LOT_DISPOSED` | Giảm/loại usable | Disposal/write-off | disposal_id |
| `MATERIAL_ISSUED_TO_PRODUCTION` | Giảm tồn nguyên liệu | Material Issue | material_issue_id |
| `MATERIAL_RETURNED_FROM_WORKSHOP` | Tăng lại nếu có trả | Return note approved | return_id |
| `MATERIAL_VARIANCE_WRITEOFF` | Giảm/ghi nhận hao hụt | Variance approval | variance_id |

Material Issue là điểm giảm tồn chính. Workshop Receipt không ghi debit lần hai.

## 9. Workshop receipt và variance

| Trường hợp | Cách xử lý |
| --- | --- |
| Nhận đủ đúng issue | Confirm receipt, không tạo ledger giảm thêm |
| Nhận thiếu trong policy | Ghi variance reason, owner review nếu policy yêu cầu |
| Nhận thiếu vượt policy | Block batch start đến khi có approval |
| Nhận thừa | Block hoặc tạo exception; không tự tăng tồn/issue |
| Hư hỏng tại xưởng | Tạo variance/write-off path, không sửa issue gốc |
| Trả lại kho | Return flow riêng, ledger tăng lại bằng return event |

## 10. MISA/accounting boundary

| Chứng từ nội bộ | Source | MISA state mặc định |
| --- | --- | --- |
| Phiếu kế toán xuất nguyên liệu | Material Issue + ledger debit | `NOT_CONFIGURED` hoặc `PENDING_OWNER_MAPPING` |
| Giá trị hao hụt cấp phát | Variance/write-off approval | `PENDING_ACCOUNTING_REVIEW` |
| Giá trị hủy/xử lý | Disposal/write-off | `PENDING_ACCOUNTING_REVIEW` |
| Giá trị nhập kho thành phẩm | Finished Goods Receipt | File 05 quản trị |

Không lưu secret MISA trong tài liệu. Không gọi API thật. Không đánh dấu synced nếu chưa có owner mapping, credential vault, dry-run evidence và approval.

## 11. Fail gates

1. Request thiếu dossier hoặc snapshot -> block.
2. Material line không có material_id/UOM/required_qty -> block.
3. Lot không `READY_FOR_PRODUCTION` -> block.
4. Approved qty vượt usable qty -> block.
5. Issue chạy lại cùng idempotency key -> no-op, không double debit.
6. Workshop receipt thiếu issue ref -> block.
7. Accounting handoff thiếu mapping -> giữ `PENDING_OWNER_MAPPING`, không sync.
---

## 6. Mapping kế toán và ranh giới MISA

# MISA MAPPING RULES

## 3. QUY TẮC NỐI MISA, KỸ THUẬT VÀ PHÂN RANH DỮ LIỆU - GINSENGFOOD

## 0. MỞ ĐẦU

### 0.1. Mục đích

Khóa phân ranh rõ giữa:

hệ vận hành Ginsengfood

lớp tính giá / chi phí nội bộ

MISA kế toán chính thức

### 0.2. Nguyên tắc cốt lõi

Hệ Ginsengfood tự tính vận hành và giá thành nội bộ. MISA hạch toán chính thức.

## 1. HỆ GINSENGFOOD TỰ TÍNH GÌ

### 1.1. Vận hành kho và sản xuất

nhập nguyên liệu

xuất nguyên liệu cho sản xuất

lô nguyên liệu

mẻ sản xuất

hao hụt

nhập kho thành phẩm

### 1.2. Chi phí nội bộ

đơn giá nhập nguyên liệu

đơn giá bình quân gia quyền

giá trị xuất nguyên liệu cho mẻ

hao hụt / hư hỏng

chi phí nhân công vận hành từ check-in / check-out

giá thành nội bộ theo mẻ / SKU

### 1.3. Kinh doanh nội bộ

doanh thu

chiết khấu

hoa hồng

đối soát đơn hàng

## 2. DỮ LIỆU NÀO LÀ KẾT QUẢ “TỰ HIỆN THEO CÔNG THỨC” VÀ ẢNH HƯỞNG KẾ TOÁN

### 2.1. Dữ liệu tự hiện theo công thức

Các dữ liệu sau trong sản xuất là dữ liệu hệ thống tự hiện:

danh sách nguyên liệu

hàm lượng

đơn vị

số lượng cần cấp

version công thức áp dụng

### 2.2. Ý nghĩa kế toán

Do nguyên liệu sản xuất là dữ liệu tự hiện theo công thức, nên hệ thống mới tính đúng được:

giá trị xuất kho

hao hụt

chi phí mẻ

giá thành nội bộ

dữ liệu đồng bộ sang MISA

### 2.3. Cấm

Nếu để người dùng tự chọn lại nguyên liệu ở bước sản xuất, dữ liệu giá thành và dữ liệu kế toán sẽ sai.

## 3. MISA HẠCH TOÁN GÌ

### 3.1. Kế toán chính thức

hạch toán nhập kho

hạch toán xuất kho

công nợ nhà cung cấp

doanh thu

chiết khấu kế toán

chi phí

báo cáo tài chính

báo cáo kế toán chính thức

### 3.2. Cấm

Không để MISA điều khiển:

lệnh sản xuất

mẻ sản xuất

công thức

traceability

recall

print logic

check-in vận hành

## 4. MISA INTEGRATION LAYER

### 4.1. Phải có lớp riêng

Lớp này phải:

nhận dữ liệu từ toàn hệ

map mã nội bộ sang mã MISA

kiểm tra điều kiện đồng bộ

gửi sang MISA

lưu trạng thái đồng bộ

### 4.2. Trạng thái đồng bộ

Chưa đồng bộ

Đã gửi MISA

Đồng bộ thành công

Đồng bộ lỗi

Chờ kế toán duyệt

## 5. DỮ LIỆU NÀO ĐỒNG BỘ SANG MISA

### 5.1. Nhập kho nguyên liệu

Đồng bộ:

phiếu nhập nguyên liệu đã duyệt

số lượng thực nhập

đơn giá nhập

thành tiền

hư hỏng loại bỏ

### 5.2. Xuất nguyên liệu cho sản xuất

Đồng bộ:

phiếu kế toán xuất nguyên liệu cho sản xuất

số lượng xuất

đơn giá bình quân

thành tiền xuất

hao hụt nếu có

### 5.3. Nhập kho thành phẩm

Đồng bộ:

lệnh nhập kho thành phẩm

số lượng nhập

giá trị thành phẩm nếu đã tính

### 5.4. Doanh thu

Đồng bộ:

dữ liệu bán hàng

doanh thu

chiết khấu

trả hàng nếu có

công nợ

### 5.5. Hoa hồng / chiết khấu

Đồng bộ khi có nhu cầu hạch toán chính thức:

hoa hồng

chiết khấu cộng tác viên

chiết khấu Diamond

khoản phải trả liên quan

## 6. PHIẾU NÀO PHẢI CÓ NGÕ KẾ TOÁN

### 6.1. Phiếu nhập nguyên liệu đầu vào

Phải có:

đơn giá nhập

thành tiền nhập

giá trị hư hỏng loại bỏ

đơn giá bình quân sau nhập

### 6.2. Phiếu chấp thuận nguyên liệu đưa vào sản xuất

Bản vận hành không cần hiện giá cho xưởng, nhưng hệ thống phải sinh:

phiếu kế toán xuất nguyên liệu cho sản xuất

đơn giá bình quân

thành tiền xuất

hao hụt

### 6.3. Lệnh nhập kho thành phẩm

Phải mở ngõ để nối:

số lượng nhập

giá trị thành phẩm

đối tượng tập hợp chi phí nếu cần

## 7. NHÂN SỰ VÀ CHI PHÍ NHÂN CÔNG

### 7.1. Check-in / check-out bắt buộc

Nhân sự tham gia mẻ phải:

check-in

check-out theo:

lệnh sản xuất

mẻ sản xuất

công đoạn

### 7.2. Dữ liệu phải giữ

mã nhân sự

họ tên

bộ phận

vai trò

thời gian vào

thời gian ra

tổng giờ làm

đơn giá công

chi phí nhân công phân bổ

### 7.3. Phân ranh

hệ Ginsengfood tự tính chi phí nhân công vận hành

chỉ đồng bộ sang MISA khi cần hạch toán chính thức

## 8. BẢNG MAPPING BẮT BUỘC

### 8.1. Mapping đối tượng

kho

nguyên liệu

thành phẩm / SKU

nhà cung cấp

khách hàng

nhân viên / bộ phận nếu cần

tài khoản kế toán

mã tập hợp chi phí

mã doanh thu / chiết khấu / hoa hồng

### 8.2. Quy tắc

Không có mapping thì không được đồng bộ chính thức sang MISA.

## 9. PASS / FAIL GATE

### 9.1. Đạt khi

hệ mình tự tính được chi phí nội bộ

nguyên liệu cho sản xuất là dữ liệu tự hiện theo công thức

MISA nhận được dữ liệu hạch toán chính thức

có mapping rõ

có trạng thái đồng bộ rõ

không lẫn logic nhà máy với logic kế toán

### 9.2. Chưa đạt khi

hệ mình không tính được giá thành nội bộ

nguyên liệu sản xuất vẫn là dữ liệu người dùng tự chọn

MISA phải điều khiển nghiệp vụ nhà máy

không có mapping

không có trạng thái sync

dữ liệu vận hành và kế toán tách rời, không nối được

## 10. KẾT LUẬN

Ba khối trên hiện đã được viết lại liền mạch và khóa cứng đầy đủ, gồm cả mục:

Ranh giới tự hiện công thức sản xuất

Quy tắc sinh mã in theo thời gian thực

Phân ranh dữ liệu với MISA

Khối này làm nền thống nhất ráp thành bản sạch cuối cho bộ:

SẢN XUẤT - NHẬP KHO - TRUY VẾT - THU HỒI CÓ LIÊN KẾT MISA để giao dev theo nhịp copy-dán / test / chạy thử.

---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P2.2C - MATERIAL ISSUE RECEIPT LEDGER

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-01 material issue and inventory ledger.
- PACK-03 material control.
- PACK-04 MISA accounting handoff boundary.
- TECH-03 Material Issue / Receipt / Ledger.

## Noi Dung Rewrite

## 35. KẾT LUẬN ĐIỀU PHỐI P2.2B

Từ thời điểm dùng bản V2.0 này, miền Production Order / Snapshot / Workshop phải tuân thủ các khóa sau:

Chọn SKU thì công thức phải tự hiện.

Công thức phải có formula code.

Công thức phải có formula version.

Công thức phải bung chi tiết từng nguyên liệu cụ thể.

Không chọn tay nguyên liệu ở Production Order.

Không sửa công thức trong Production Order.

Anchor gạo và ratio_to_rice phải đến từ Phase 1.

Nhập anchor quantity chỉ tạo scaled formula lines, không đổi formula version.

Production Order phải tạo Formula/Recipe/BOM Snapshot.

Snapshot phải bất biến.

Recipe/formula thay đổi sau đó không làm thay đổi snapshot cũ.

Production Order phải sinh Production Dossier.

Production Dossier là nguồn dữ liệu gốc cho chuỗi phiếu sau.

Workshop Handoff chỉ là bàn giao dữ liệu xuống xưởng.

Workshop Handoff không phải Material Receipt.

Production Order không Material Issue.

Production Order không debit inventory.

Production Order không tạo batch.

Production Order không nhập kho.

Production Order không sellable.

Actor/permission/audit/idempotency/evidence là bắt buộc.

Evidence Submitted chưa phải Evidence Accepted.

Global Gate vẫn bị chặn.

Tài liệu kế tiếp cần viết lại toàn bộ là:

## 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER

## TÀI LIỆU ĐẶC TẢ MATERIAL REQUEST / MATERIAL APPROVAL / MATERIAL ISSUE / MATERIAL RECEIPT / RAW INVENTORY LEDGER CHO PHASE 2

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

## PHASE-02-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER

Đây là file thứ ba trong nhóm triển khai giới hạn của Phase 2 - Operational Core.

File này phụ trách miền:

Material Request / phiếu đề nghị cấp nguyên liệu.

Material Approval / phiếu chấp thuận nguyên liệu đưa vào sản xuất.

Accounting Material Issue Handoff / phiếu kế toán xuất nguyên liệu cho sản xuất.

Material Issue / xuất nguyên liệu thực tế.

Raw Inventory Ledger Debit / ghi giảm tồn nguyên liệu.

Material Receipt / Workshop Receipt / xưởng xác nhận nhận nguyên liệu.

No second debit boundary / không giảm tồn lần hai.

Inventory Ledger append-only.

Idempotency để chống double debit.

Variance handling giữa quantity đề nghị, quantity xuất, quantity xưởng nhận.

MISA accounting boundary.

Actor / permission / audit / evidence.

Smoke/evidence cho P2.2C.

Report 15 mục cho P2.2C.

Tài liệu này thay thế toàn bộ nội dung cũ của:

## 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER.docx

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER

Phiên bản

Phase

## PHASE 2 - OPERATIONAL CORE

Workstream

## P2.2C

Loại tài liệu

Limited Implementation Handoff / Material-Issue-Receipt-Ledger

Nguồn chính

README Phase 2 V1.1 + P2.1 Technical Design + P2.2B Production Dossier

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

Phụ thuộc Phase 1

SKU / Formula / Ingredient / UOM đã chuẩn hóa

Phụ thuộc P2.2A

Raw Lot READY_FOR_PRODUCTION

Phụ thuộc P2.2B

Production Order / Production Dossier / Formula Snapshot

Cho phép code ngay bởi file này?

Không, chỉ sau khi có lệnh Limited Implementation riêng

Cho phép migration ngay bởi file này?

Không

Cho phép seed ngay bởi file này?

Không

Cho phép Agent tự suy luận nguyên liệu?

Không

Global Gate

bị chặn

Production Readiness

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để khóa điểm quan trọng nhất của tồn kho nguyên liệu:

Material Issue là điểm giảm tồn nguyên liệu chính.

Nếu P2.2C sai, các lỗi nghiêm trọng có thể xảy ra:

Nguyên liệu bị trừ tồn hai lần.

Raw lot chưa READY_FOR_PRODUCTION vẫn được cấp vào sản xuất.

Phiếu đề nghị cấp nguyên liệu cho phép chọn tay nguyên liệu ngoài công thức.

Phiếu chấp thuận nguyên liệu thêm nguyên liệu ngoài phiếu đề nghị.

Material Receipt / Workshop Receipt bị hiểu nhầm là điểm giảm tồn.

Inventory Ledger bị update/delete thay vì append-only.

Retry API tạo double debit.

Kế toán/MISA can thiệp vào trạng thái vận hành.

Batch sau này không trace đúng nguyên liệu đã xuất.

Giá thành nội bộ sai.

Traceability và Recall sai từ gốc.

Mục tiêu chính của tài liệu này:

Khóa phiếu đề nghị cấp nguyên liệu phải tự hiện nguyên liệu theo Production Dossier / Formula Snapshot.

Khóa không cho chọn tay nguyên liệu ở Material Request.

Khóa phiếu chấp thuận nguyên liệu dựa trên từ phiếu đề nghị, không thêm nguyên liệu ngoài request.

Khóa Material Issue chỉ được xuất raw lot có trạng thái READY_FOR_PRODUCTION.

Khóa Material Issue là điểm duy nhất ghi Raw Inventory Ledger Debit.

Khóa Material Receipt / Workshop Receipt chỉ xác nhận xưởng nhận nguyên liệu, không giảm tồn lần hai.

Khóa Inventory Ledger append-only.

Khóa idempotency để retry không double debit.

Khóa phiếu kế toán xuất nguyên liệu cho sản xuất chỉ là handoff/sync boundary, MISA không điều khiển vận hành.

Khóa actor/permission/audit/evidence cho toàn bộ command rủi ro.

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

Không thay file chi tiết P2.2C

Tầng 2

P2.1 Technical Design V1.1

Thiết kế workstream

Cung cấp form registry, field behavior, data inheritance

Không tự cho phép code

Tầng 3

P2.2A Source/Raw/QC/Readiness

Raw lot readiness truth

Cung cấp raw lot READY_FOR_PRODUCTION

Không issue lot chưa READY

Tầng 4

P2.2B Production Order/Snapshot/Workshop

Production dossier truth

Cung cấp Production Dossier và Formula Snapshot

Không lấy formula động thay snapshot

Tầng 5

P2.2C V1.1 này

Nguồn chính cho Material Request/Issue/Receipt/Ledger

Giao dev/Agent phân tích/triển khai giới hạn khi được mở

Không lấn sang Batch/Warehouse/Trace

Tầng 6

Phase 1 Recipe/BOM/Formula/UOM

Nguồn công thức nền

Cung cấp ingredient, UOM, ratio, formula version

Không sửa Phase 1 trong file này

Tầng 7

nguồn tổng hợp TÀI LIỆU.docx

Nguồn phụ trợ nghiệp vụ

Đối chiếu phiếu cấp nguyên liệu, issue, receipt, MISA boundary

Không code/seed trực tiếp

Tầng 8

Code hiện tại

Hiện trạng triển khai

Dùng để gap analysis

Không là source-of-truth nếu conflict

## 5. SCOPE IN - PHẠM VI BAO GỒM

Tài liệu này bao gồm:

Material Request.

Material Request line.

Material Approval.

Material Approval line.

Accounting Material Issue Handoff.

Material Issue.

Material Issue line.

Raw lot selection for issue.

READY_FOR_PRODUCTION issue guard.

Issue quantity and UOM validation.

Raw Inventory Ledger Debit.

Ledger append-only boundary.

Idempotency chống double debit.

Material Receipt / Workshop Receipt.

Receipt line.

No second debit boundary.

Received quantity / variance reason.

Variance between requested / approved / issued / received.

MISA accounting boundary.

Disposal/write-off boundary nếu nguyên liệu bị lỗi trong quá trình cấp/nhận.

Actor / permission / audit / evidence.

Smoke/evidence requirements cho P2.2C.

Report 15 mục cho P2.2C.

## 6. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM

Tài liệu này không bao gồm:

Không triển khai Source / Raw Lot / QC đầy đủ.

Không triển khai Production Order đầy đủ.

Không triển khai Production Dossier đầy đủ.

Không triển khai Batch.

Không triển khai Batch Process.

Không triển khai Batch QC.

Không triển khai Batch Release.

Không triển khai Warehouse Receipt thành phẩm.

Không triển khai Finished Goods Ledger.

Không triển khai Packaging/Print/QR runtime.

Không triển khai Traceability đầy đủ.

Không triển khai Recall / Sale Lock đầy đủ.

Không triển khai Commerce Runtime.

Không quyết định Sellable.

Không triển khai full MRP/procurement.

Không triển khai full MISA Integration.

Không tạo Official Order.

Không tạo QuoteSnapshot.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 7. ĐỊNH NGHĨA THUẬT NGỮ

Thuật ngữ

Định nghĩa

Material Request

Phiếu đề nghị cấp nguyên liệu cho sản xuất

Material Approval

Phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất

Material Issue

Lệnh xuất nguyên liệu thực tế từ kho sang sản xuất

Material Receipt / Workshop Receipt

Phiếu xưởng xác nhận đã nhận nguyên liệu

Raw Inventory Ledger Debit

Bút toán ghi giảm tồn nguyên liệu

Ledger Append-only

Ledger chỉ được ghi thêm, không update/delete bằng business flow

Double Debit

Lỗi giảm tồn nguyên liệu hai lần cho cùng nghiệp vụ

Ready Raw Lot

Raw lot có trạng thái READY_FOR_PRODUCTION

Issue Eligibility

Điều kiện raw lot được phép xuất vào sản xuất

Variance

Chênh lệch giữa số lượng đề nghị / duyệt / xuất / nhận

Accounting Handoff

Dữ liệu chuyển sang kế toán/MISA để hạch toán chính thức

MISA Boundary

Ranh giới MISA không điều khiển vận hành

Idempotency

Cơ chế chống retry tạo side effect trùng

Production Dossier

Hồ sơ sản xuất gốc sinh từ Production Order

Formula Snapshot

Bản chụp công thức dùng cho Production Dossier

## PHẦN 2/4 - MATERIAL REQUEST / MATERIAL APPROVAL / MATERIAL ISSUE MODEL

## 8. LUỒNG NGHIỆP VỤ TỔNG QUAN P2.2C

Luồng nghiệp vụ P2.2C:

Production Dossier -> Material Request -> Material Approval -> Material Issue -> Raw Inventory Ledger Debit -> Material Receipt / Workshop Receipt -> No Second Debit -> Handoff cho Batch Process

Trong đó:

Production Dossier đến từ P2.2B.

Material Request phải tự hiện nguyên liệu theo Formula Snapshot trong Production Dossier.

Material Request không được cho người dùng chọn tay nguyên liệu.

Material Approval dựa trên Material Request.

Material Approval không được thêm nguyên liệu ngoài request.

Material Issue chỉ được issue raw lot READY_FOR_PRODUCTION.

Material Issue là điểm duy nhất ghi Raw Inventory Ledger Debit.

Material Receipt / Workshop Receipt chỉ xác nhận xưởng nhận nguyên liệu.

Material Receipt / Workshop Receipt không được giảm tồn lần hai.

Inventory Ledger là append-only.

Retry không được tạo double debit.

MISA chỉ nhận handoff kế toán, không điều khiển issue/receipt.

## 9. MATERIAL REQUEST - PHIẾU ĐỀ NGHỊ CẤP NGUYÊN LIỆU

## 9.1. Nguyên tắc

nguồn tổng hợp đã khóa:

Phiếu đề nghị cấp nguyên liệu phải tự hiện nguyên liệu theo công thức.

Trong P2.2C, điều này có nghĩa:

Material Request phải lấy dữ liệu từ Production Dossier.

Production Dossier phải chứa Formula Snapshot.

Formula Snapshot phải có từng ingredient line cụ thể.

Material Request lines được sinh từ snapshot.

Người dùng không được chọn tay nguyên liệu.

Người dùng không được thêm nguyên liệu ngoài snapshot.

Người dùng không được sửa ingredient_code.

Người dùng không được sửa formula_version trong request.

Nếu cần cấp lệch/đổi nguyên liệu, phải có workflow deviation riêng được Owner phê duyệt, không được sửa tay trong request mặc định.

## 9.2. Checklist Material Request Header

Trường

Bắt buộc

Quy tắc

material_request_code

Có

Mã phiếu đề nghị

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

request_status

Có

## DRAFT / GENERATED / SUBMITTED / APPROVED / REJECTED / CANCELLED

requested_by

Có

Actor

requested_at

Có

Thời điểm

evidence_ref

Tùy policy

Evidence nếu cần

audit_log

Có

Bắt buộc

idempotency_key

Có nếu command có side effect

Chống tạo trùng request

## 9.3. Checklist Material Request Line

Trường

Bắt buộc

Quy tắc

request_line_no

Có

Thứ tự dòng

formula_snapshot_line_ref

Có

Trỏ dòng snapshot

ingredient_code

Có

dựa trên từ snapshot

ingredient_name_snapshot

Có

dựa trên

material_group_snapshot

Có

A/B/Company Source

source_type_snapshot

Có

Supplier/Company Source/Internal

requested_quantity

Có

Tính từ snapshot

requested_uom

Có

dựa trên UOM

inventory_uom_snapshot

Có

Nếu khác UOM

conversion_rule_snapshot

Có nếu cần

Không tự suy luận

requested_for_dossier

Có

Trỏ dossier

line_status

Có

## GENERATED / APPROVED / REJECTED / ISSUED / CANCELLED

variance_allowed

Có

Theo policy

note

Tùy

Không thay đổi ingredient

## 9.4. Field behavior Material Request

Field

Hành vi

production_dossier_code

Click chọn hoặc hệ thống tự sinh từ dossier đủ điều kiện

sku_code

Tự hiện

formula_code

Tự hiện

formula_version

Tự hiện

ingredient_code

Tự hiện, không chọn tay

ingredient_name

Tự hiện

requested_quantity

Tự hiện từ snapshot

requested_uom

Tự hiện

note

Nhập tay có kiểm soát

request_status

State machine quản lý

variance_reason

Chỉ nhập nếu policy cho phép

## 9.5. Fail gate Material Request

Fail nếu:

Material Request không linked Production Dossier.

Material Request không lấy Formula Snapshot.

Material Request cho chọn tay ingredient.

Material Request cho thêm ingredient ngoài snapshot.

Material Request cho sửa ingredient_code.

Material Request cho sửa formula_version.

Material Request thiếu UOM.

Material Request dùng formula hiện tại thay vì snapshot.

Material Request tự debit inventory.

Material Request tự tạo batch.

## 10. MATERIAL APPROVAL - PHIẾU CHẤP THUẬN NGUYÊN LIỆU ĐƯA VÀO SẢN XUẤT

## 10.1. Nguyên tắc

nguồn tổng hợp đã khóa:

Phiếu chấp thuận nguyên liệu đưa vào sản xuất phải dựa trên từ phiếu đề nghị cấp nguyên liệu.

Trong P2.2C, điều này có nghĩa:

Material Approval phải linked Material Request.

Approval lines phải dựa trên request lines.

Approval không được thêm nguyên liệu ngoài request.

Approval không được đổi formula snapshot.

Approval có thể approve/reject từng dòng theo policy.

Approval có thể điều chỉnh quantity trong giới hạn rule nếu được Owner xác nhận.

Mọi điều chỉnh phải có reason, actor, audit.

Approval chưa phải Material Issue.

Approval chưa giảm tồn.

## 10.2. Checklist Material Approval Header

Trường

Bắt buộc

Quy tắc

material_approval_code

Có

Mã phiếu chấp thuận

material_request_code

Có

Trỏ phiếu đề nghị

production_dossier_code

Có

dựa trên

approval_status

Có

## WAITING / APPROVED / PARTIALLY_APPROVED / REJECTED / CANCELLED

approved_by

Có nếu duyệt

Actor có quyền

approved_at

Có nếu duyệt

Thời điểm

approval_reason

Có nếu reject/adjust

Lý do

evidence_ref

Tùy policy

Evidence

audit_log

Có

Bắt buộc

idempotency_key

Có nếu command có side effect

Chống tạo trùng

## 10.3. Checklist Material Approval Line

Trường

Bắt buộc

Quy tắc

approval_line_no

Có

Thứ tự

request_line_ref

Có

Trỏ request line

ingredient_code

Có

dựa trên, không đổi

approved_quantity

Có nếu approved

Không vượt rule nếu chưa có approval đặc biệt

approved_uom

Có

dựa trên/request UOM

approval_line_status

Có

## APPROVED / REJECTED / ADJUSTED

adjustment_reason

Có nếu adjusted

Bắt buộc

approved_raw_lot_policy

Tùy

Có chọn lot ở approval hay để issue chọn theo READY

audit_log

Có

Bắt buộc

## 10.4. Fail gate Material Approval

Fail nếu:

Approval không linked request.

Approval thêm ingredient ngoài request.

Approval đổi ingredient_code.

Approval đổi formula_version.

Approval tự debit inventory.

Approval tự tạo Material Receipt.

Approval thiếu actor/audit.

Approval adjustment không có reason.

Approval cho phép quantity bất hợp lý không qua rule.

## 11. MATERIAL ISSUE - ĐIỂM GIẢM TỒN NGUYÊN LIỆU

## 11.1. Nguyên tắc

nguồn tổng hợp đã khóa:

Material Issue là điểm giảm tồn nguyên liệu.

Trong P2.2C, đây là rule P0:

Chỉ Material Issue mới ghi Raw Inventory Ledger Debit.

Material Request không ghi debit.

Material Approval không ghi debit.

Material Receipt / Workshop Receipt không ghi debit lần hai.

Retry Material Issue không được double debit.

Material Issue phải linked Production Dossier.

Material Issue phải linked Material Approval.

Material Issue phải chọn raw lot READY_FOR_PRODUCTION.

Material Issue phải có actor/permission/audit/idempotency.

Material Issue không tạo batch.

Material Issue không nhập kho thành phẩm.

Material Issue không quyết định sellable.

## 11.2. Checklist Material Issue Header

Trường

Bắt buộc

Quy tắc

material_issue_code

Có

Mã lệnh xuất nguyên liệu

material_approval_code

Có

Trỏ phiếu chấp thuận

material_request_code

Có

dựa trên

production_dossier_code

Có

dựa trên

production_order_code

Có

dựa trên

issue_status

Có

DRAFT / CONFIRMED / PARTIALLY_CONFIRMED / CANCELLED / bị chặn

issued_by

Có khi confirmed

Actor kho

issued_at

Có khi confirmed

Thời điểm

warehouse_location

Có

Kho xuất

evidence_ref

Tùy policy

Evidence

idempotency_key

Có

Bắt buộc chống double debit

audit_log

Có

Bắt buộc

accounting_handoff_status

Có nếu có handoff

WAITING / READY / SENT / FAILED / bị chặn

## 11.3. Checklist Material Issue Line

Trường

Bắt buộc

Quy tắc

issue_line_no

Có

Thứ tự

approval_line_ref

Có

Trỏ line đã duyệt

ingredient_code

Có

dựa trên

raw_lot_code

Có

Lot được xuất

raw_lot_status_snapshot

Có

Phải là READY_FOR_PRODUCTION

issued_quantity

Có

Số lượng xuất

issued_uom

Có

UOM xuất

inventory_uom_snapshot

Có

Nếu khác UOM

conversion_rule_snapshot

Có nếu cần

Không tự suy luận

requested_quantity_ref

Có

Số lượng đề nghị

approved_quantity_ref

Có

Số lượng duyệt

issue_variance_quantity

Có nếu lệch

Chênh lệch

issue_variance_reason

Có nếu lệch

Lý do

raw_ledger_debit_ref

Có khi confirmed

Trỏ ledger debit

line_status

Có

CONFIRMED / PARTIAL / CANCELLED / bị chặn

audit_log

Có

Bắt buộc

## 11.4. Guard Material Issue

Material Issue chỉ được confirm khi pass toàn bộ guard:

Guard

Điều kiện

Production Dossier valid

Dossier tồn tại, không cancelled/bị chặn

Material Approval valid

Approval approved/partially approved

Approval line valid

Có line được duyệt

Ingredient match

Ingredient issue trùng approval line

Raw lot exists

Raw lot tồn tại

Raw lot material match

Raw lot đúng ingredient

Raw lot READY

raw_lot_status = READY_FOR_PRODUCTION

Raw lot not hold

Không HOLD

Raw lot not quarantine

Không QUARANTINE

Raw lot not rejected

Không REJECTED

Raw lot not expired

Không EXPIRED

Raw lot not damaged

Không DAMAGED

Quantity available

Tồn khả dụng đủ

UOM valid

UOM và conversion hợp lệ

Actor valid

Actor có quyền xuất

Idempotency valid

Không retry tạo double debit

Evidence valid

Nếu policy yêu cầu

Audit ready

Có audit/correlation

## 11.5. Fail gate Material Issue

Fail nếu:

Issue không linked Production Dossier.

Issue không linked Approval.

Issue raw lot chưa READY_FOR_PRODUCTION.

Issue raw lot chỉ QC_PASS nhưng chưa READY.

Issue raw lot HOLD/QUARANTINE/REJECTED/EXPIRED/DAMAGED.

Issue sai ingredient so với approval line.

Issue quantity vượt approved/available mà không có rule.

Issue thiếu UOM/conversion.

Issue không tạo raw ledger debit.

Retry tạo double debit.

Issue thiếu actor/permission/audit.

Issue tự tạo batch.

Issue tự warehouse receipt.

Issue tự set sellable.

## 12. RAW INVENTORY LEDGER DEBIT

## 12.1. Nguyên tắc

Raw Inventory Ledger là nguồn ghi nhận biến động tồn nguyên liệu.

Raw Inventory Ledger phải append-only.

Material Issue thành công phải tạo ledger debit.

Không được update/delete ledger entry bằng business flow.

Nếu cần điều chỉnh, phải tạo entry điều chỉnh mới với reason/audit.

## 12.2. Checklist Raw Ledger Debit

Trường

Bắt buộc

Quy tắc

ledger_entry_code

Có

Mã ledger

ledger_type

Có

## RAW_MATERIAL

direction

Có

## DEBIT

source_command

Có

## MATERIAL_ISSUE

source_command_code

Có

material_issue_code

source_line_ref

Có

material_issue_line

production_dossier_code

Có

dựa trên

material_code

Có

Nguyên liệu

raw_lot_code

Có

Lot bị xuất

quantity

Có

Số lượng debit

uom

Có

UOM debit

inventory_uom

Có

UOM tồn kho

conversion_snapshot

Có nếu cần

Snapshot conversion

warehouse_location

Có

Kho xuất

actor

Có

Người/system thực hiện

occurred_at

Có

Thời điểm

idempotency_key

Có

Chống double debit

correlation_id

Có

Truy vết chuỗi

audit_log

Có

Bắt buộc

## 12.3. Ledger append-only rule

Không được:

Update ledger quantity.

Delete ledger entry.

Sửa direction.

Sửa source_command.

Ghi đè ledger cũ khi retry.

Dùng Material Receipt để sửa debit.

Dùng MISA để sửa ledger vận hành.

Nếu sai ledger:

Tạo correction entry.

Có reason.

Có actor.

Có audit.

Có approval nếu policy yêu cầu.

## 13. MATERIAL RECEIPT / WORKSHOP RECEIPT

## 13.1. Nguyên tắc

nguồn tổng hợp đã khóa:

Material Receipt / Workshop Receipt chỉ xác nhận xưởng nhận nguyên liệu, không giảm tồn lần hai.

Trong P2.2C, đây là rule P0:

Workshop Receipt phải linked Material Issue.

Workshop Receipt dựa trên issue lines.

Workshop Receipt ghi nhận quantity xưởng nhận.

Workshop Receipt có thể ghi nhận chênh lệch/variance.

Workshop Receipt không tạo Raw Inventory Ledger Debit.

Workshop Receipt không sửa debit của Material Issue.

Workshop Receipt không tự tạo Batch output.

Workshop Receipt không tự tạo Finished Goods.

Workshop Receipt không tự tạo Warehouse Receipt.

## 13.2. Checklist Workshop Receipt Header

Trường

Bắt buộc

Quy tắc

material_receipt_code

Có

Mã phiếu nhận

material_issue_code

Có

Trỏ issue

production_dossier_code

Có

dựa trên

receipt_status

Có

DRAFT / RECEIVED / PARTIALLY_RECEIVED / REJECTED / bị chặn

received_by

Có khi received

Actor xưởng

received_at

Có khi received

Thời điểm

workshop_location

Có nếu có

Xưởng/khu vực

evidence_ref

Tùy policy

Evidence

idempotency_key

Có

Chống duplicate receipt

audit_log

Có

Bắt buộc

## 13.3. Checklist Workshop Receipt Line

Trường

Bắt buộc

Quy tắc

receipt_line_no

Có

Thứ tự

issue_line_ref

Có

Trỏ issue line

ingredient_code

Có

dựa trên

raw_lot_code

Có

dựa trên

issued_quantity

Có

dựa trên

issued_uom

Có

dựa trên

received_quantity

Có

Xưởng nhập

received_uom

Có

UOM nhận

variance_quantity

Có nếu lệch

Chênh lệch

variance_reason

Có nếu lệch

Lý do

condition_on_receipt

Tùy

Bình thường/hỏng/thiếu

line_status

Có

## RECEIVED / PARTIAL / REJECTED / VARIANCE

audit_log

Có

Bắt buộc

## 13.4. Variance handling

Tình huống

Cách xử lý

Received quantity = issued quantity

Receipt received bình thường

Received quantity < issued quantity

Cần variance reason

Received quantity > issued quantity

Bị reject hoặc yêu cầu điều tra, không tự tạo tồn

Nguyên liệu nhận bị hỏng

Tạo variance/evidence, có thể kích hoạt disposal/review

Xưởng từ chối nhận

Receipt rejected, reason/evidence

Sai nguyên liệu

Block, điều tra, không sửa ledger issue trực tiếp

Thiếu evidence

Receipt bị chặn nếu policy yêu cầu

## 13.5. Fail gate Workshop Receipt

Fail nếu:

Receipt không linked Issue.

Receipt tự tạo raw ledger debit lần hai.

Receipt sửa ledger debit của Issue.

Receipt tạo batch output.

Receipt tạo finished goods.

Receipt tạo warehouse receipt.

Receipt nhận quantity lớn hơn issued mà không block.

Variance không có reason.

Receipt thiếu actor/audit.

Retry receipt tạo duplicate side effect.

## PHẦN 3/4 - ACCOUNTING HANDOFF / MISA BOUNDARY / DISPOSAL / CROSS-CUTTING

## 14. PHIẾU KẾ TOÁN XUẤT NGUYÊN LIỆU / ACCOUNTING HANDOFF

## 14.1. Nguyên tắc

nguồn tổng hợp đã khóa:

Phiếu kế toán xuất nguyên liệu cho sản xuất phải có ngõ MISA/kế toán.

Đồng thời:

Ginsengfood tự tính vận hành/giá thành nội bộ; MISA hạch toán chính thức.

Vì vậy, trong P2.2C:

Ginsengfood là nguồn sự thật vận hành của Material Issue.

Material Issue tạo raw inventory ledger debit vận hành.

Ginsengfood có thể tạo accounting handoff record.

MISA nhận dữ liệu qua Integration Layer riêng.

MISA không điều khiển Material Request.

MISA không điều khiển Material Approval.

MISA không điều khiển Material Issue.

MISA không sửa Raw Inventory Ledger.

MISA không xác nhận xưởng nhận nguyên liệu.

MISA không thay thế audit/evidence vận hành.

## 14.2. Accounting handoff fields

Trường

Bắt buộc

Quy tắc

accounting_handoff_code

Có

Mã handoff

material_issue_code

Có

Trỏ Issue

production_dossier_code

Có

dựa trên

material_code

Có

Nguyên liệu

raw_lot_code

Có

Lot

quantity

Có

Số lượng

uom

Có

Đơn vị

issue_date

Có

Ngày xuất

warehouse_location

Có

Kho

cost_placeholder

Tùy phase

Nếu có costing nội bộ

misa_mapping_status

Có

WAITING / READY / SENT / FAILED / bị chặn

misa_item_ref

Tùy

Mapping, không hardcode

misa_warehouse_ref

Tùy

Mapping, không hardcode

sync_attempt_count

Có nếu sync

Số lần thử

last_sync_error

Có nếu fail

Lỗi

audit_log

Có

Bắt buộc

## 14.3. MISA boundary fail gate

Fail nếu:

MISA tạo Material Issue.

MISA sửa Material Issue.

MISA sửa Raw Inventory Ledger.

MISA xác nhận Workshop Receipt.

MISA quyết định lot được issue.

MISA thay đổi Production Dossier.

MISA hardcode item/warehouse/account mapping trong logic vận hành.

MISA sync fail làm rollback Material Issue vận hành.

MISA sync success được hiểu là Issue mới hợp lệ hơn operational truth.

## 15. DISPOSAL / WRITE-OFF TRONG MIỀN MATERIAL ISSUE / RECEIPT

## 15.1. Nguyên tắc

Disposal/write-off có thể phát sinh trong P2.2C nếu:

Nguyên liệu đã issue nhưng xưởng phát hiện hư hỏng.

Nguyên liệu thiếu/hỏng khi bàn giao.

Nguyên liệu bị từ chối nhận.

Nguyên liệu bị hủy sau khi xuất kho nhưng trước khi sản xuất.

Nguyên liệu sai lot/sai chất lượng cần điều tra.

Tuy nhiên, P2.2C không được xử lý disposal bằng cách sửa ledger issue.

Nếu cần write-off, phải đi qua flow có kiểm soát:

Disposal Request -> QA/Owner Review -> Disposal Executed -> Inventory Ledger Write-off -> Evidence/Audit

## 15.2. Boundary disposal trong P2.2C

Tình huống

Xử lý

Xưởng nhận thiếu so với issue

Receipt variance + investigation

Nguyên liệu hỏng khi nhận

Receipt variance + disposal request nếu cần

Nguyên liệu sai lot

Block receipt + investigation

Nguyên liệu đã issue nhưng không dùng được

Disposal request/review/write-off

Cần điều chỉnh tồn

Không sửa ledger cũ, tạo entry correction/write-off

Cần trả lại kho

Cần return flow riêng, không tự sửa issue debit

## 15.3. Fail gate disposal trong P2.2C

Fail nếu:

Xóa ledger debit để “sửa” nguyên liệu hỏng.

Xóa tồn kho để xử lý thiếu/hỏng.

Receipt variance tự update lại Issue quantity.

Disposal không có request/review/evidence.

Write-off không có ledger entry.

Không trace được disposal về issue/lot/dossier.

MISA xử lý disposal trước khi Ginsengfood có operational record.

## 16. RAW INVENTORY BALANCE PROJECTION

## 16.1. Nguyên tắc

P2.2C có thể cập nhật hoặc chuẩn bị raw inventory balance projection nếu thuộc scope implementation.

Tuy nhiên, nguồn sự thật biến động vẫn là ledger.

Balance projection là kết quả tính từ ledger, không phải ledger.

## 16.2. Rule balance projection

Rule

Diễn giải

Ledger là nguồn gốc

Balance phải tính từ ledger entries

Projection có thể cache

Nhưng không được là source-of-truth duy nhất

Issue debit giảm projection

Khi ledger debit confirmed

Receipt không giảm lần hai

Workshop Receipt không tạo debit

Correction qua ledger

Không sửa projection thủ công

Retry không double debit

Idempotency phải giữ projection đúng

## 16.3. Fail gate projection

Fail nếu:

Balance giảm ở Material Issue và giảm tiếp ở Receipt.

Projection sửa tay không dựa ledger.

Ledger thiếu nhưng projection vẫn giảm.

Retry làm projection giảm nhiều lần.

MISA sync sửa projection vận hành.

## 17. ACTOR / PERMISSION / AUDIT / IDEMPOTENCY / EVIDENCE

## 17.1. Command rủi ro trong P2.2C

Command

Actor

Permission

Audit

Idempotency

Evidence

Generate Material Request

System/Actor

Bắt buộc

Bắt buộc

Bắt buộc

Snapshot evidence

Submit Material Request

Actor

Bắt buộc

Bắt buộc

Bắt buộc

Nếu policy yêu cầu

Approve Material Request

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Approval evidence

Reject Material Request

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Reason

Confirm Material Issue

Warehouse actor

Bắt buộc

Bắt buộc

Bắt buộc

Issue evidence

Create Raw Ledger Debit

System

Bắt buộc theo command

Bắt buộc

Bắt buộc

Ledger evidence

Confirm Workshop Receipt

Workshop actor

Bắt buộc

Bắt buộc

Bắt buộc

Receipt evidence

Record Variance

Workshop/QA actor

Bắt buộc

Bắt buộc

Bắt buộc

Variance evidence

Create Accounting Handoff

System/Accounting actor

Bắt buộc

Bắt buộc

Bắt buộc

Handoff evidence

Create Disposal Request

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Disposal evidence

## 17.2. Idempotency rule

Case

Expected

Same key + same payload Material Request

Không tạo request trùng

Same key + different payload Material Request

Conflict

Same key + same payload Material Issue

Không tạo double debit

Same key + different payload Material Issue

Conflict

Same key + same payload Workshop Receipt

Không tạo receipt trùng

Same key + different payload Workshop Receipt

Conflict

Retry ledger debit after timeout

Trả kết quả cũ hoặc no-op, không debit lần hai

Retry accounting handoff

Không tạo record kế toán trùng nếu cùng payload

## 17.3. Evidence rule

Evidence có thể ở trạng thái:

## NOT_SUBMITTED.

## SUBMITTED.

## UNDER_REVIEW.

## ACCEPTED.

## REJECTED.

bị chặn.

## SUPERSEDED.

Nếu policy yêu cầu evidence accepted, thì evidence SUBMITTED chưa được xem là pass.

Fail nếu evidence SUBMITTED bị dùng như ACCEPTED.

## 18. ENTITY / API / SERVICE BOUNDARY

## 18.1. Entity impact draft

Tài liệu này không tạo migration, nhưng implementation sau này có thể cần rà soát các nhóm entity:

Nhóm entity

Vai trò

material_request

Phiếu đề nghị cấp nguyên liệu

material_request_line

Dòng đề nghị

material_approval

Phiếu chấp thuận

material_approval_line

Dòng chấp thuận

material_issue

Lệnh xuất nguyên liệu

material_issue_line

Dòng xuất

material_receipt

Phiếu xưởng nhận

material_receipt_line

Dòng nhận

raw_inventory_ledger

Ledger tồn nguyên liệu

accounting_handoff

Handoff kế toán/MISA

material_variance

Chênh lệch xuất/nhận

disposal_request

Yêu cầu hủy nếu phát sinh

evidence_ref

Liên kết evidence

audit_log

Audit

idempotency_record

Chống retry trùng

## 18.2. API/service boundary

Nhóm

Mục tiêu

Không được

Material Request API

Tạo/đọc request từ dossier

Không chọn tay nguyên liệu

Material Approval API

Duyệt request lines

Không thêm material ngoài request

Material Issue API

Confirm issue + ledger debit

Không issue lot chưa READY

Raw Ledger Service

Ghi debit append-only

Không update/delete ledger

Workshop Receipt API

Xưởng xác nhận nhận

Không debit lần hai

Variance Service

Ghi nhận chênh lệch

Không sửa ledger cũ

Accounting Handoff Service

Chuẩn bị MISA handoff

Không để MISA mutate issue

Disposal Service

Request/review/write-off

Không xóa tay tồn

Idempotency Service

Chống double debit

Không bỏ qua khi retry

Audit Service

Ghi audit

Không bỏ audit

Evidence Service

Link evidence

Không coi submitted là accepted

PHẦN 4/4 - EXECUTION ORDER / HANDOFF / SMOKE / DONE GATE / FAIL GATE

## 19. EXECUTION ORDER P2.2C

Khi sau này được phép triển khai, P2.2C phải đi theo thứ tự:

Thứ tự

Việc cần làm

Output

Đọc README Phase 2 V1.1

Hiểu boundary Phase 2

Đọc P2.1 Technical Design V1.1

Hiểu form registry / field behavior

Đọc P2.2A V1.1

Hiểu raw lot READY_FOR_PRODUCTION

Đọc P2.2B V2.0

Hiểu Production Dossier / Snapshot

Đọc P2.2C V1.1 này

Hiểu Material Request/Issue/Receipt/Ledger

Analysis code hiện tại

Gap matrix

Owner Confirm nếu thiếu dữ liệu

Owner confirm list

Technical implementation plan

Task breakdown

Limited implementation nếu được mở

Code trong scope

10

Smoke P2.2C

Smoke result

11

Evidence submission

Evidence package

12

Report 15 mục

Handoff update

Hiện tại trong cuộc làm tài liệu này:

Chỉ viết lại tài liệu sạch, chưa mở implementation.

## 20. DEV / CODEX / COPILOT HANDOFF

## 20.1. Mode mặc định

Khi giao file này cho Agent:

## MODE: LIMITED IMPLEMENTATION CHỈ KHI ĐÃ CÓ APPROVED ANALYSIS + TECHNICAL DESIGN

Nếu chưa có approval, phải dùng:

## MODE: ANALYSIS ONLY

## 20.2. Prompt đúng cho Agent

MODE hiện tại: ANALYSIS ONLY nếu chưa có Owner/dev lead approval.

Nhiệm vụ:

Lập Gap Matrix giữa tài liệu P2.2C và code hiện tại.

Liệt kê entity/table hiện có hoặc thiếu.

Liệt kê API/DTO/service/UI/worker bị ảnh hưởng.

Liệt kê nơi Material Request đang cho chọn tay nguyên liệu nếu có.

Liệt kê nơi Material Request không lấy Production Dossier / Formula Snapshot nếu có.

Liệt kê nơi Material Approval thêm nguyên liệu ngoài request nếu có.

Liệt kê nơi Material Issue có thể issue raw lot chưa READY_FOR_PRODUCTION nếu có.

Liệt kê nơi Material Issue không tạo raw ledger debit nếu có.

Liệt kê nơi Material Receipt / Workshop Receipt tạo debit lần hai nếu có.

Liệt kê nơi Inventory Ledger có thể update/delete nếu có.

Liệt kê nơi retry có thể tạo double debit nếu có.

Liệt kê nơi MISA có thể mutate operational truth nếu có.

Liệt kê risk P0/P1/P2.

Đề xuất implementation scope.

Đề xuất smoke/evidence.

Không sửa code nếu chưa có lệnh Limited Implementation riêng.

Cấm:

Không code trực tiếp từ nguồn tổng hợp.

Không tạo migration khi chưa duyệt.

Không seed dữ liệu khi chưa Owner confirm.

Không tự tạo material lines ngoài snapshot.

Không issue raw lot chưa READY.

Không tạo double debit.

Không để Receipt debit lần hai.

Không triển khai Batch đầy đủ.

Không triển khai Warehouse đầy đủ.

Không gọi Gateway PASS.

Không gọi Production Readiness.

## 21. OWNER CONFIRM REQUIRED

Các điểm cần Owner/dev lead xác nhận trước implementation:

Nhóm

Nội dung cần xác nhận

Material Request trigger

Khi nào sinh request từ Production Dossier

Material Request lines

Có cho variance/deviation không

Approval workflow

Ai duyệt, duyệt toàn bộ hay từng line

Raw lot selection

Chọn lot ở approval hay issue

Quantity tolerance

Cho phép lệch bao nhiêu giữa request/issue/receipt

UOM conversion

Lấy rule từ Phase 1 hay snapshot

Issue warehouse

Kho xuất nguyên liệu

Issue idempotency

Key và conflict behavior

Ledger event type

Raw debit event naming

Receipt variance

Các reason chênh lệch

Workshop receipt

Có cần xưởng ACK trước batch không

Accounting handoff

Field handoff kế toán/MISA

MISA mapping

Chỉ placeholder hay đã có mapping

Disposal from issue/receipt

Khi nào kích hoạt disposal

## RBAC

Ai request/approve/issue/receive/accounting handoff/dispose

Evidence

Evidence nào cần accepted

Audit

Audit field bắt buộc

## 22. STOP CONDITIONS

Dev/Agent phải dừng nếu gặp:

Mã

Điều kiện dừng

Hành động

## P2-2C-STOP-01

Chưa có Production Dossier

Dừng, quay lại P2.2B

## P2-2C-STOP-02

Production Dossier thiếu Formula Snapshot

Dừng P0

## P2-2C-STOP-03

Material Request cho chọn tay nguyên liệu

Dừng P0

## P2-2C-STOP-04

Material Request không lấy snapshot

Dừng P0

## P2-2C-STOP-05

Material Approval thêm nguyên liệu ngoài request

Dừng P0

## P2-2C-STOP-06

Raw lot chưa READY vẫn issue được

Dừng P0

## P2-2C-STOP-07

Raw lot QC_PASS chưa READY vẫn issue được

Dừng P0

## P2-2C-STOP-08

HOLD/QUARANTINE/REJECTED vẫn issue được

Dừng P0

## P2-2C-STOP-09

Issue không tạo raw ledger debit

Dừng P0

## P2-2C-STOP-10

Receipt tạo debit lần hai

Dừng P0

## P2-2C-STOP-11

Ledger update/delete được

Dừng P0

## P2-2C-STOP-12

Retry tạo double debit

Dừng P0

## P2-2C-STOP-13

MISA mutate Material Issue/Ledger

Dừng P0

## P2-2C-STOP-14

Disposal xóa ledger/tồn kho

Dừng P0

## P2-2C-STOP-15

Command thiếu actor/audit

Dừng P0

## P2-2C-STOP-16

Evidence SUBMITTED được coi ACCEPTED

Dừng P0

## P2-2C-STOP-17

Agent chuẩn bị code khi chưa approval

Dừng

## P2-2C-STOP-18

Scope lấn sang Batch/Warehouse

Dừng, chuyển đúng file P2.2D

## P2-2C-STOP-19

Có ý định gọi Gateway PASS/Production Readiness

Dừng

## 23. SMOKE REQUIREMENTS P2.2C

Mã smoke

Nội dung

Expected

## SMK-P2-2C-01

Tạo Material Request từ Production Dossier

Request được tạo

## SMK-P2-2C-02

Material Request tự hiện nguyên liệu theo Formula Snapshot

Lines tự hiện

## SMK-P2-2C-03

User chọn tay nguyên liệu ở Material Request

Bị chặn

## SMK-P2-2C-04

Material Request thiếu Production Dossier

Bị reject

## SMK-P2-2C-05

Material Request dùng formula hiện tại thay snapshot

Bị chặn

## SMK-P2-2C-06

Material Approval linked Request

Approval được tạo

## SMK-P2-2C-07

Approval thêm ingredient ngoài request

Bị chặn

## SMK-P2-2C-08

Approval adjustment thiếu reason

Bị reject

## SMK-P2-2C-09

Issue raw lot READY_FOR_PRODUCTION

Issue được confirm

## SMK-P2-2C-10

Issue raw lot QC_PASS chưa READY

Bị reject

## SMK-P2-2C-11

Issue raw lot HOLD/QUARANTINE/REJECTED

Bị reject

## SMK-P2-2C-12

Issue sai ingredient so với approval

Bị reject

## SMK-P2-2C-13

Issue thành công

Tạo Raw Inventory Ledger Debit

## SMK-P2-2C-14

Retry Issue same key/payload

Không double debit

## SMK-P2-2C-15

Same key different payload

Conflict

## SMK-P2-2C-16

Ledger update/delete attempt

Bị chặn

## SMK-P2-2C-17

Workshop Receipt linked Issue

Receipt được tạo

## SMK-P2-2C-18

Workshop Receipt thành công

Không tạo debit lần hai

## SMK-P2-2C-19

Receipt quantity lệch issue

Cần variance reason

## SMK-P2-2C-20

Receipt quantity lớn hơn issue

Bị reject/block

## SMK-P2-2C-21

Accounting handoff

Có handoff boundary, MISA không mutate

## SMK-P2-2C-22

Disposal từ variance/hư hỏng

Có request/review/write-off flow

## SMK-P2-2C-23

Evidence required nhưng SUBMITTED

Không pass nếu policy cần ACCEPTED

## SMK-P2-2C-24

Command thiếu actor/permission

## DENY/BLOCK

## SMK-P2-2C-25

Material Issue confirmed

Không tự tạo batch/warehouse/sellable

## 24. EVIDENCE REQUIREMENTS P2.2C

Mã evidence

Nội dung evidence

## EVD-P2-2C-MATERIAL-REQUEST

Material Request linked Production Dossier

## EVD-P2-2C-AUTO-LINES

Request lines tự hiện từ Formula Snapshot

## EVD-P2-2C-NO-MANUAL-INGREDIENT

Không chọn tay nguyên liệu

## EVD-P2-2C-MATERIAL-APPROVAL

Approval dựa trên Request

## EVD-P2-2C-READY-LOT-ISSUE

Issue chỉ từ raw lot READY

EVD-P2-2C-QC-PASS-NOT-READY-BLOCK

QC_PASS chưa READY bị chặn

## EVD-P2-2C-HOLD-QUARANTINE-BLOCK

HOLD/QUARANTINE/REJECTED bị chặn

## EVD-P2-2C-RAW-LEDGER-DEBIT

Material Issue tạo Raw Ledger Debit

## EVD-P2-2C-NO-DOUBLE-DEBIT

Retry không double debit

## EVD-P2-2C-LEDGER-APPEND-ONLY

Ledger không update/delete

## EVD-P2-2C-WORKSHOP-RECEIPT

Workshop Receipt linked Issue

## EVD-P2-2C-NO-SECOND-DEBIT

Receipt không debit lần hai

## EVD-P2-2C-VARIANCE

Variance reason/evidence

## EVD-P2-2C-MISA-HANDOFF

Accounting handoff boundary

## EVD-P2-2C-DISPOSAL

Disposal/write-off nếu phát sinh

## EVD-P2-2C-AUDIT

Actor/permission/audit evidence

## EVD-P2-2C-IDEMPOTENCY

Idempotency evidence

EVD-P2-2C-GATEWAY-bị chặn

Gateway vẫn bị chặn

Evidence Submitted chưa phải Evidence Accepted.

## 25. RISK REGISTER P2.2C

Risk

Mức độ

Nguyên nhân

Kiểm soát

Material Request chọn tay nguyên liệu

## P0

Không dùng snapshot

Auto-lines from dossier

Approval thêm nguyên liệu ngoài request

## P0

Approval UI/API sai

Request inheritance guard

Issue lot chưa READY

## P0

Issue guard thiếu readiness

raw_lot_status guard

Double debit

## P0

Issue retry hoặc Receipt debit

Idempotency + no second debit

Ledger bị update/delete

## P0

Thiết kế ledger sai

Append-only rule

Receipt sửa issue debit

## P0

Nhầm receipt là correction

Variance/correction flow riêng

MISA mutate operational truth

## P0

Boundary sai

Handoff-only

UOM sai khi issue

## P0

Conversion thiếu

UOM snapshot guard

Variance không reason

## P1

Thiếu validation

Variance reason required

Disposal xóa tồn

## P0

Thiếu write-off flow

Controlled disposal

Command thiếu audit

## P1

Thiếu cross-cutting

Audit required

Evidence submitted được pass

## P0

Sai evidence state

Evidence state guard

## 26. REPORT FORMAT 15 MỤC CHO P2.2C

Mọi report P2.2C phải có đủ 15 mục:

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

Nếu thiếu 15 mục, không được chuyển sang P2.2D.

## 27. DONE GATE P2.2C

P2.2C được xem là đạt ở mức handoff/implementation report khi:

Material Request linked Production Dossier.

Material Request tự hiện nguyên liệu theo Formula Snapshot.

Material Request không cho chọn tay nguyên liệu.

Material Request không dùng formula động.

Material Approval linked Request.

Material Approval dựa trên request lines.

Material Approval không thêm ingredient ngoài request.

Material Approval adjustment có reason/audit.

Material Issue linked Approval.

Material Issue linked Production Dossier.

Material Issue chỉ issue raw lot READY_FOR_PRODUCTION.

Raw lot QC_PASS chưa READY bị chặn.

HOLD/QUARANTINE/REJECTED/EXPIRED/DAMAGED bị chặn.

Issue sai ingredient bị chặn.

Material Issue tạo Raw Inventory Ledger Debit.

Retry Issue không double debit.

Ledger append-only.

Material Receipt / Workshop Receipt linked Issue.

Workshop Receipt không debit lần hai.

Workshop Receipt variance có reason/evidence.

Accounting handoff có boundary.

MISA không mutate operational truth.

Disposal/write-off không xóa tay tồn kho.

Actor/permission/audit/idempotency/evidence được kiểm soát.

Có smoke result.

Có evidence package.

Có report 15 mục.

Không lấn sang Batch/Warehouse/Trace.

Gateway vẫn bị chặn.

Không gọi Completion Decision.

Không gọi Production Readiness.

## 28. FAIL GATE P2.2C

P2.2C fail nếu xảy ra một trong các trường hợp:

Material Request không linked Production Dossier.

Material Request không lấy Formula Snapshot.

Material Request cho chọn tay nguyên liệu.

Material Request cho thêm material ngoài snapshot.

Material Approval thêm ingredient ngoài request.

Material Approval đổi formula version.

Material Approval tự debit inventory.

Material Issue raw lot chưa READY_FOR_PRODUCTION.

Material Issue raw lot QC_PASS chưa READY.

Material Issue raw lot HOLD/QUARANTINE/REJECTED/EXPIRED/DAMAGED.

Material Issue không tạo Raw Ledger Debit.

Material Issue retry tạo double debit.

Ledger update/delete được.

Material Receipt tạo debit lần hai.

Material Receipt sửa ledger debit của Issue.

Receipt variance không có reason.

Receipt quantity lớn hơn issue nhưng vẫn pass.

MISA tạo/sửa Material Issue.

MISA sửa Raw Inventory Ledger.

MISA xác nhận Workshop Receipt.

Disposal bằng cách xóa tồn kho.

Disposal/write-off không có evidence/audit.

Command rủi ro thiếu actor.

Command rủi ro thiếu permission.

Command rủi ro thiếu audit.

Evidence Submitted bị gọi Accepted.

Lấn sang Batch/Warehouse đầy đủ.

Material Issue tự tạo Batch.

Material Issue tự tạo Warehouse Receipt.

Material Issue tự set Sellable.

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

P2.2C - Material Request / Issue / Receipt / Ledger

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
