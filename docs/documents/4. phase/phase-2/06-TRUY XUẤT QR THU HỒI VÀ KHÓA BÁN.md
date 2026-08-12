# 06-P2 - TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN

> ⚠️ HISTORICAL / SUPERSEDED (owner decision OD-M10-004, 2026-06-22): Thứ tự đặt **Packaging Level 2 + Print SAU Warehouse Receipt** trong tài liệu này KHÔNG còn là canonical. Thứ tự canonical: **Production process → Packaging/Printing/QR → Finished QC → Batch Release → Warehouse FG Receipt** (xem `docs/software-specs/modules/10_PACKAGING_PRINTING.md` OD-M10-004 và `docs/software-specs/workflows/05_CANONICAL_OPERATIONAL_FLOW.md`). Phần còn lại của tài liệu (QR lifecycle, recall, sale-lock, public trace whitelist) vẫn dùng để tham khảo lịch sử.

Trạng thái: `TRACEABILITY_QR_RECALL_SALE_LOCK_CANONICAL`  
Mode: `LIMITED IMPLEMENTATION HANDOFF`  
Mục tiêu: khóa trace chain, QR/public trace, recall và sale lock để downstream không bán SKU/lô bị chặn.

## 1. Trace chain

```text
SKU -> Formula Version -> Production Order -> Production Dossier -> BOM Snapshot -> Raw Lot -> Material Issue -> Batch -> Packaging -> Warehouse Receipt -> Finished Goods Inventory -> Public Trace Projection
```

## 2. Public trace vs internal trace

Public trace chỉ hiển thị whitelist: sản phẩm, batch public code, ngày sản xuất/HSD nếu được duyệt, trạng thái an toàn public. Không lộ supplier, raw lot nội bộ, QC note nội bộ, costing, accounting/MISA, actor private, internal IDs.

Internal trace giữ đủ raw lot, QC, material issue, batch, packaging, warehouse, recall/sale lock, accounting refs.

## 3. Recall/sale lock

Recall hoặc sale lock có thể theo SKU, batch, raw lot, channel hoặc program. Khi active, downstream commerce/AI/ads/live/CRM phải dừng bán/quote/order/promote SKU/lô liên quan.

## 4. Print và reprint

Print cấp 1/2 phải dùng payload từ dossier/batch/packaging profile. Reprint không được thiếu reason/audit/evidence.

## 5. Internal trace data model

| Node | Bắt buộc lưu | Không được thiếu |
| --- | --- | --- |
| SKU/Product | sku_id, product_name_snapshot, product_group, classification_state | public name đã duyệt |
| Formula | formula_id, formula_version, snapshot_id | G1/source refs |
| Raw Lot | raw_lot_id, material_id, lot_code, QC/readiness state | receipt_line_id |
| Material Issue | issue_id, approved_qty, issue_qty, ledger_id | idempotency key |
| Workshop Receipt | receipt_id, received_qty, variance_reason | issue_id |
| Batch | batch_id, batch_code, process/qc/release state | dossier_id |
| Packaging | packaging_run_id, package material refs, print payload hash | level 1/2 refs |
| Finished Goods | fg_receipt_id, warehouse, quantity, location | release state |
| Sale Lock/Recall | lock_id, scope, reason, effective_at, released_by | unblock approval |

Trace nội bộ phải nối được cả hai chiều: từ batch lên raw lot/source và từ raw lot xuống mọi batch/finished goods bị ảnh hưởng.

## 6. Public QR projection

Public QR chỉ được expose whitelist:

1. Tên sản phẩm public-approved.
2. Public batch code hoặc mã truy xuất đã mask.
3. Ngày sản xuất/HSD nếu owner duyệt hiển thị.
4. Trạng thái an toàn public: `VALID`, `ON_HOLD`, `RECALLED`, `EXPIRED`, `NOT_FOUND`.
5. Thông điệp recall/sale lock public nếu có.
6. Hotline/contact hoặc URL chính thức nếu được cấu hình.

Không expose supplier, raw lot nội bộ, giá vốn, MISA refs, actor nội bộ, QC note nội bộ, full material list nhạy cảm hoặc internal IDs.

## 7. Recall và sale lock scope

| Scope | Ví dụ | Downstream phải chặn |
| --- | --- | --- |
| SKU | Toàn bộ SKU A2 | Quote/order/promote SKU |
| Batch | Batch cụ thể | Sellable tồn batch đó |
| Raw lot | Nguyên liệu/lô đầu vào lỗi | Mọi batch dùng raw lot |
| Warehouse/location | Kho/vị trí bị hold | Xuất bán từ location |
| Channel | Kênh online/đại lý/live | Publish/promote/order trên channel |
| Date range/program | Lô trong chương trình | Campaign/order/ads tương ứng |

Sale lock active phải override mọi trạng thái release/warehouse/sellable khác.

## 8. Recall workflow

```text
Draft Recall -> Impact Analysis -> Owner Approval -> Lock Activated -> Downstream Notified -> Public Projection Updated -> Resolution/Close
```

Không cho close recall nếu còn affected finished goods chưa xử lý, downstream chưa acknowledge hoặc public projection chưa cập nhật.

## 9. Sale lock state

| State | Ý nghĩa |
| --- | --- |
| `NONE` | Không có lock |
| `PENDING_APPROVAL` | Đang chờ duyệt |
| `ACTIVE` | Đang chặn bán/quote/order/promote |
| `PARTIAL_RELEASED` | Mở một phần scope có approval |
| `RESOLVED` | Đã xử lý xong |
| `CANCELLED` | Hủy lock trước khi active |

Mở khóa phải có owner approval, reason, evidence và audit; không tự mở khi batch hết hold.

## 10. Evidence tối thiểu

| Evidence ID | Nội dung | Expected |
| --- | --- | --- |
| `EVD-P2-TRACE-LINEAGE` | SKU -> formula -> raw lot -> issue -> batch -> FG | Nối đủ node |
| `EVD-P2-PUBLIC-QR` | QR public whitelist | Không lộ internal data |
| `EVD-P2-RECALL-IMPACT` | Raw lot recall tìm batch affected | Không bỏ sót batch/warehouse |
| `EVD-P2-SALE-LOCK` | Lock active chặn bán | Downstream quote/order/promote block |
| `EVD-P2-REPRINT-AUDIT` | Reprint có reason/hash/audit | Không in lại silent |
| `EVD-P2-UNLOCK` | Unlock có approval | Không auto unlock |
---

## PHỤ LỤC NGUỒN ĐÃ NHẬP - BỘ KHÓA 5 BƯỚC.md

> Phụ lục này giữ nguyên văn nguồn cũ để không mất bảng, số liệu, field hoặc rule khi bỏ file nguồn riêng. Phần rewrite chuẩn hóa ở đầu file là nguồn chính; phụ lục dùng để trace và đối chiếu số liệu.

**BƯỚC 1**

**20 SKU ↔ NGUYÊN LIỆU / BOM MAP - GINSENGFOOD**

**0\. MỞ ĐẦU TÀI LIỆU**

**0.1. Mục đích tài liệu**

Tài liệu này dùng để khóa quan hệ giữa:

- **20 SKU canonical**
- **danh mục nguyên liệu canonical**
- **công thức đầu vào cho sản xuất mẫu**

Tài liệu này chỉ dùng để khóa:

- SKU nào dùng nguyên liệu nào
- nguyên liệu nào là bắt buộc
- nguyên liệu nào thuộc phần dược liệu
- nguyên liệu nào thuộc phần nguyên liệu
- version BOM / recipe nào đang áp dụng
- cách map từ SKU sang danh sách nguyên liệu để cấp phát và sản xuất

**0.2. Vai trò tài liệu**

Đây là mắt xích nối giữa:

- **BỘ 20 SKU CANONICAL - GINSENGFOOD**
- **BỘ NGUYÊN LIỆU CANONICAL - GINSENGFOOD**
- **CÔNG THỨC ĐẦU VÀO CHO SẢN XUẤT MẪU**

**0.3. Ranh giới tài liệu**

Tài liệu này không giữ:

- giá nguyên liệu
- tồn kho
- lô nguyên liệu
- QC checklist
- packaging rule
- in mã
- trace / recall rule chi tiết

Những phần đó đi ở bước sau.

**1\. NGUYÊN TẮC CHUNG**

**1.1. Một SKU phải map được ra nguyên liệu**

Mỗi SKU phải map được ra:

- danh sách dược liệu
- danh sách nguyên liệu
- version công thức đang áp dụng
- mô tả sơ chế theo ghi chú

**1.2. Không cho suy luận thủ công**

Dev và vận hành không được tự suy luận nguyên liệu từ tên sản phẩm.  
Phải có map rõ ràng.

**1.3. BOM / Recipe phải theo version**

Mỗi SKU phải có:

- mã công thức
- version công thức
- trạng thái công thức

**1.4. Chọn SKU là ra BOM/Recipe**

Khi chọn SKU trong lệnh sản xuất, hệ thống phải dùng chính map này để tự hiện công thức.

**2\. CẤU TRÚC MAP BẮT BUỘC**

**2.1. Thông tin đầu map**

- SKU
- Tên sản phẩm
- Mã công thức
- Version
- Trạng thái công thức

**2.2. Phần 1 - Dược liệu**

Mỗi dòng gồm:

- STT
- Mã nguyên liệu
- Tên dược liệu
- Hàm lượng
- Đơn vị
- Bắt buộc / không bắt buộc
- Ghi chú

**2.3. Phần 2 - Nguyên liệu**

Mỗi dòng gồm:

- STT
- Mã nguyên liệu
- Tên nguyên liệu
- Hàm lượng
- Đơn vị
- Bắt buộc / không bắt buộc
- Ghi chú

**3\. DANH SÁCH 20 SKU ↔ BOM MAP**

**3.1. Nhóm A - Theo mùa**

- A1/CS/DM/HS
- A2/CS/BASA
- A3/CS/CAHOI
- A4/CS/LUON
- A5/CS/CUU

**3.2. Nhóm B - Chức năng**

- B1/CS/RM/ĐX
- B2/CS/DHA
- B3/CS/CACOM
- B4/CS/COLAGEN
- B5/CS/SINHLUC
- B6/CS/GAAC

**3.3. Nhóm C - Bổ dưỡng**

- C1/CS/BAONGU
- C2/CS/DONGTRUNG
- C3/CS/NAMDONGCO
- C4/CS/CUABIEN
- C5/CS/CANGU
- C6/CS/TOM/RONGBIEN
- C7/CS/THITGA
- C8/CS/THITHEO
- C9/CS/THITBO

**4\. QUY TẮC CỨNG**

**4.1. Không SKU nào được sản xuất nếu chưa có BOM/Recipe map**

**4.2. Không cho vận hành chọn lại nguyên liệu ở bước sản xuất**

**4.3. Mọi thay đổi nguyên liệu phải đi qua version công thức**

**4.4. Mã nguyên liệu phải lấy từ danh mục nguyên liệu canonical**

**4.5. Mọi cấp phát nguyên liệu cho sản xuất phải bám đúng map này**

**5\. PASS / FAIL GATE**

**5.1. Đạt khi**

- đủ 20 SKU có BOM/Recipe map
- mỗi SKU map được ra danh sách nguyên liệu
- map phân tách được dược liệu / nguyên liệu
- có version công thức
- hệ thống dùng được map này để tự hiện công thức

**5.2. Chưa đạt khi**

- thiếu SKU
- thiếu nguyên liệu
- không có version
- vẫn phải suy luận tay từ tên SKU

**BƯỚC 2**

**20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD**

**0\. MỞ ĐẦU TÀI LIỆU**

**0.1. Mục đích tài liệu**

Tài liệu này dùng để khóa **cấu hình vận hành** của từng SKU trong Operational Domain.

**0.2. Vai trò tài liệu**

Đây là file nối từ:

- SKU
- BOM / recipe
- packaging
- QC
- release
- trace
- recall

**0.3. Ranh giới tài liệu**

Không giữ:

- logic tài chính
- doanh thu
- KPI
- MISA sync

**1\. CẤU TRÚC CONFIG MỖI SKU**

Mỗi SKU phải có tối thiểu:

**1.1. Thông tin công thức**

- mã công thức
- version
- trạng thái áp dụng

**1.2. Quy cách đóng gói cấp 1**

- đơn vị cấp 1:
  - gói / lọ / hũ
- nội dung in cấp 1
- số lượng / quy cách cấp 1 nếu đã khóa

**1.3. Quy cách đóng gói cấp 2**

- đơn vị cấp 2:
  - hộp / thùng / lọ / hũ
- nội dung in cấp 2
- mã vạch / QR rule

**1.4. QC Config**

- QC checkpoint nào
- bắt buộc % độ ẩm hay không
- tiêu chí đạt / không đạt chính

**1.5. Release Config**

- điều kiện cho nhập kho thành phẩm
- điều kiện chặn release

**1.6. Trace Config**

- public trace có / không
- internal trace có / không
- cấp truy vết theo hộp / thùng

**1.7. Recall Config**

- SKU có thuộc diện recall áp dụng không
- mức truy vết yêu cầu

**2\. QUY TẮC CHUNG**

**2.1. Operational không owner SKU master**

Operational chỉ owner **config vận hành theo SKU**, không owner master SKU.

**2.2. Không SKU nào được đưa vào sản xuất nếu thiếu operational config**

**2.3. Không SKU nào được in mã cấp 2 nếu thiếu packaging + trace config**

**2.4. Không SKU nào được nhập kho nếu thiếu QC/release config**

**3\. PASS / FAIL GATE**

**3.1. Đạt khi**

- mỗi SKU có operational config
- config đủ packaging / QC / release / trace / recall
- không phải đoán bằng tay khi vận hành

**3.2. Chưa đạt khi**

- SKU có BOM nhưng thiếu config vận hành
- đóng gói / QC / trace vẫn phải đoán thủ công
- thiếu rule in mã / trace / recall

**BƯỚC 3**

**QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC**

**0\. MỞ ĐẦU**

**0.1. Mục đích**

Khóa cách quản lý version công thức để sau này tối ưu mà không loạn.

**0.2. Vai trò**

Đây là file giữ cho:

- công thức gốc nghiên cứu
- công thức sản xuất mẫu
- công thức áp dụng chính thức  
   không bị ghi đè lẫn nhau.

**1\. NGUYÊN TẮC VERSION**

**1.1. Công thức gốc nghiên cứu**

Là baseline đầu tiên, lấy từ đề tài nghiên cứu.

**1.2. Công thức sản xuất mẫu**

Là version dùng cho pilot / test mẻ thực tế.

**1.3. Công thức đang áp dụng**

Là version được duyệt để đưa vào sản xuất vận hành thường.

**1.4. Công thức ngưng áp dụng**

Version cũ vẫn phải lưu để truy xuất lịch sử.

**2\. KHI NÀO PHẢI TẠO VERSION MỚI**

Phải tạo version mới khi có thay đổi về:

- nguyên liệu
- hàm lượng
- đơn vị
- bước kỹ thuật
- ghi chú kỹ thuật ảnh hưởng vận hành
- quy cách áp dụng theo mẻ

Không được sửa đè version cũ.

**3\. PHÊ DUYỆT THAY ĐỔI CÔNG THỨC**

Mỗi thay đổi công thức phải có:

- người đề xuất
- lý do thay đổi
- phạm vi thay đổi
- version mới
- ngày hiệu lực
- người phê duyệt
- mẻ đầu tiên áp dụng

**4\. TRẠNG THÁI VERSION**

Tối thiểu:

- DRAFT
- PILOT
- APPROVED
- ACTIVE
- DEPRECATED

**5\. QUY TẮC CỨNG**

**5.1. Không cho sản xuất bằng version chưa được phép**

**5.2. Không cho sửa đè version cũ**

**5.3. Mỗi mẻ phải ghi nhận đang dùng version nào**

**5.4. Mọi phiếu sản xuất phải tự kéo version đang áp dụng**

**6\. PASS / FAIL GATE**

**6.1. Đạt khi**

- mỗi thay đổi tạo version mới
- có phê duyệt
- truy được mẻ dùng version nào

**6.2. Chưa đạt khi**

- sửa đè công thức cũ
- không biết mẻ nào dùng version nào
- thay đổi mà không có phê duyệt

**BƯỚC 4**

**TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP**

**0\. MỞ ĐẦU**

**0.1. Mục đích**

Khóa chuỗi dữ liệu truy vết thực tế từ nguyên liệu đến thành phẩm.

**0.2. Vai trò**

Đây là mắt xích nối:

- lô nguyên liệu
- mẻ sản xuất
- batch thành phẩm
- đơn vị đóng gói
- QR / barcode
- trace / recall

**1\. CHUỖI DỮ LIỆU BẮT BUỘC**

**1.1. Nguyên liệu đầu vào**

- mã nguyên liệu
- lô nguyên liệu

**1.2. Xuất cho sản xuất**

- lô nguyên liệu nào đi vào mẻ nào

**1.3. Mẻ sản xuất**

- mẻ nào
- SKU nào
- version công thức nào

**1.4. Batch thành phẩm**

- batch / lô sản xuất được sinh từ mẻ nào

**1.5. Đóng gói cấp 1**

- gói / lọ / hũ thuộc mẻ / batch nào

**1.6. Đóng gói cấp 2**

- hộp / thùng thuộc mẻ / batch nào

**1.7. Mã vạch / QR**

- QR cấp hộp map tới record batch / trace nào
- barcode map tới đơn vị đóng gói nào

**2\. RULE THEO CẤP ĐÓNG GÓI**

**2.1. Cấp 1**

Chỉ mang:

- MFG
- EXP

**2.2. Cấp 2**

Mang:

- batch / lot
- MFG
- EXP
- barcode
- QR

**3\. PUBLIC TRACE VS INTERNAL TRACE**

**3.1. Public trace**

Hiển thị dữ liệu cho người dùng cuối, không lộ dữ liệu nội bộ nhạy cảm.

**3.2. Internal trace**

Dùng cho:

- QC
- nhà máy
- kho
- truy nguyên nguyên liệu
- recall

**4\. RECALL DATA CHAIN**

Recall phải lần ngược được:

- hộp / thùng
- QR / barcode
- batch
- mẻ sản xuất
- lô nguyên liệu
- nhà cung cấp / source nếu cần

**5\. PASS / FAIL GATE**

**5.1. Đạt khi**

- truy được từ batch về lô nguyên liệu
- truy được từ hộp/thùng về batch
- QR cấp 2 map đúng dữ liệu
- recall lần ngược được chuỗi

**5.2. Chưa đạt khi**

- batch không gắn với lô nguyên liệu
- QR không gắn với batch thật
- hộp / thùng không truy được về mẻ

**BƯỚC 5**

**BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH**

**0\. MỞ ĐẦU**

**0.1. Mục đích**

Khóa lớp đặc tả để dev code chính xác.

**0.2. Vai trò**

Dùng cho:

- backend
- frontend
- QA
- vận hành

**1\. MỖI PHIẾU PHẢI CÓ 4 LỚP ĐẶC TẢ**

**1.1. Màn hình**

- route
- title
- phần thông tin chung
- bảng chi tiết
- khối bằng chứng
- khối xác nhận

**1.2. Route / API**

- create
- view
- update
- approve / reject
- next step

**1.3. Action**

- ai được bấm
- khi nào được bấm
- điều kiện chuyển trạng thái

**1.4. Trạng thái**

- draft
- submitted
- approved
- rejected
- completed
- canceled  
   (tùy phiếu)

**2\. CÁC MÀN HÌNH / ROUTE / ACTION TỐI THIỂU CẦN ĐẶC TẢ**

**2.1. Phiếu nhập nguyên liệu đầu vào**

**2.2. Lệnh sản xuất**

**2.3. Phiếu đề nghị cấp nguyên liệu cho sản xuất**

**2.4. Phiếu chấp thuận nguyên liệu đưa vào sản xuất**

**2.5. Phiếu check-in / check-out nhân sự**

**2.6. Phiếu theo dõi sơ chế**

**2.7. Phiếu kiểm chất lượng sau sấy thăng hoa**

**2.8. Phiếu đóng gói cấp 1**

**2.9. Phiếu đóng gói cấp 2**

**2.10. Phiếu QC thành phẩm**

**2.11. Lệnh nhập kho thành phẩm**

**2.12. Phiếu kế toán xuất nguyên liệu cho sản xuất**

**3\. QUY TẮC CỨNG CHO DEV**

**3.1. Phiếu nhập nguyên liệu = click chọn**

**3.2. Lệnh sản xuất / cấp phát = tự hiện công thức**

**3.3. Mọi phiếu sau = tự kế thừa dữ liệu từ hồ sơ gốc**

**3.4. Mọi phiếu phải có khối xác nhận cá nhân**

**3.5. Mọi phiếu phải đúng đơn vị theo công đoạn**

**3.6. Không cho hardcode đơn vị hoặc nguyên liệu sai rule**

**4\. PASS / FAIL GATE**

**4.1. Đạt khi**

- mỗi phiếu có màn hình rõ
- route rõ
- action rõ
- trạng thái rõ
- điều kiện chuyển bước rõ

**4.2. Chưa đạt khi**

- dev phải tự đoán form
- route không rõ
- action không rõ
- trạng thái không rõ

**20 SKU ↔ NGUYÊN LIỆU / BOM MAP - GINSENGFOOD**

**0\. MỞ ĐẦU TÀI LIỆU**

**0.1. Mục đích tài liệu**

Tài liệu này dùng để khóa **BOM / Recipe Map** giữa:

- **20 SKU canonical**
- **danh mục nguyên liệu canonical**
- **công thức đầu vào cho sản xuất mẫu**

Tài liệu này chỉ dùng để khóa:

- SKU nào dùng nguyên liệu nào
- nguyên liệu nào thuộc **Dược liệu**
- nguyên liệu nào thuộc **Nguyên liệu**
- hàm lượng ban đầu của từng thành phần
- đơn vị tính
- mã công thức
- version công thức
- trạng thái công thức

**0.2. Vai trò tài liệu**

Tài liệu này là mắt xích nối giữa:

- **BỘ 20 SKU CANONICAL - GINSENGFOOD**
- **BỘ NGUYÊN LIỆU CANONICAL - GINSENGFOOD**
- **CÔNG THỨC ĐẦU VÀO CHO SẢN XUẤT MẪU cho 20 SKU cháo sâm - GINSENGFOOD**

**0.3. Ranh giới tài liệu**

Tài liệu này **không giữ**:

- giá nguyên liệu
- tồn kho
- lô nguyên liệu
- QC checklist
- release rule
- packaging rule
- trace / recall rule chi tiết
- logic kế toán / MISA

Những phần đó đi ở bước sau.

**0.4. Nguồn khóa dùng để viết tài liệu**

Nguồn đối chiếu trực tiếp của tài liệu này là:

- file danh mục 20 SKU đã chuẩn hóa vegan / mặn
- file danh mục nguyên liệu đầu vào 2 nhóm
- file đề tài dữ liệu đầu vào nguyên liệu làm baseline công thức G0

**1\. NGUYÊN TẮC CHUNG**

**1.1. Một SKU phải map được ra BOM / Recipe**

Mỗi SKU phải map được ra:

- mã công thức
- version công thức
- danh sách dược liệu
- danh sách nguyên liệu
- hàm lượng
- đơn vị tính

**1.2. Không cho suy luận thủ công**

Dev, QA và vận hành không được suy luận nguyên liệu từ tên sản phẩm.

Hệ thống phải đọc đúng từ BOM / Recipe Map này.

**1.3. Chọn SKU là ra công thức**

Khi chọn SKU trong lệnh sản xuất, hệ thống phải tự động hiện:

- mã công thức
- version công thức
- toàn bộ dược liệu
- toàn bộ nguyên liệu
- hàm lượng
- đơn vị tính

**1.4. Không cho chọn lại nguyên liệu ở bước sản xuất**

Danh sách nguyên liệu dùng cho sản xuất là dữ liệu **tự hiện** theo:

- SKU
- mã công thức
- version công thức

Người vận hành không được tự thêm, tự xóa, tự thay đổi danh sách nguyên liệu ở bước sản xuất thường.

**1.5. Version công thức**

BOM / Recipe Map phải đi theo version.

Bản trong tài liệu này là:

- **Version G0**
- **Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu**

Canonical cleanup 2026-06-05: `G0` chỉ là history/research/pilot baseline, không được dùng để sinh production order, material issue hoặc BOM snapshot production. `G1` là operational/pilot candidate và chỉ thành production-active khi có owner approval, effective date và evidence accepted theo PACK-02.

**1.6. Quy tắc thay đổi về sau**

Nếu có thay đổi:

- nguyên liệu
- hàm lượng
- đơn vị
- ghi chú kỹ thuật

thì tạo **version mới**, không sửa đè version G0.

**2\. CẤU TRÚC CHUẨN CHO MỖI BOM / RECIPE MAP**

**2.1. Thông tin đầu**

- SKU
- Tên sản phẩm
- Mã công thức
- Version
- Trạng thái

**2.2. Phần 1 - Dược liệu**

Mỗi dòng gồm:

- STT
- Mã nguyên liệu
- Tên nguyên liệu
- Hàm lượng
- Đơn vị
- Ghi chú

**2.3. Phần 2 - Nguyên liệu**

Mỗi dòng gồm:

- STT
- Mã nguyên liệu
- Tên nguyên liệu
- Hàm lượng
- Đơn vị
- Ghi chú

**3\. NHÓM A - CHÁO SÂM THEO MÙA**

**3.1. A1/CS/DM/HS - CHÁO SÂM - DIÊM MẠCH & HẠT SEN**

**Thông tin đầu**

- SKU: A1/CS/DM/HS
- Tên sản phẩm: Cháo Sâm - Diêm mạch & Hạt sen
- Mã công thức: FML-A1-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú**       |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         | Dược liệu chủ đạo |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         | Dược liệu         |
| 3       | DL-003    | Bạch linh           | 5.0           | kg         | Dược liệu         |
| 4       | DL-004    | Kỷ tử               | 3.0           | kg         | Dược liệu         |
| 5       | DL-005    | Gừng nướng          | 0.7           | kg         | Dược liệu         |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú**      |
| ------- | --------- | ------------------------ | ------------- | ---------- | ---------------- |
| 1       | NL-001    | Diêm mạch                | 15.5          | kg         | Thành phần chính |
| 2       | NL-002    | Hạt sen                  | 9             | kg         | Thành phần chính |
| 3       | NL-003    | Gạo lúa - tôm            | 195           | kg         | Nền dinh dưỡng   |
| 4       | NL-004    | Cà rốt                   | 35            | kg         | Nền / nước hầm   |
| 5       | NL-005    | Bí đỏ                    | 33            | kg         | Nền dinh dưỡng   |
| 6       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         | Nền dinh dưỡng   |
| 7       | NL-007    | Nấm kim châm             | 18.5          | kg         | Nền dinh dưỡng   |
| 8       | NL-008    | Táo tàu                  | 4.4           | kg         | Nước hầm         |
| 9       | NL-009    | Củ cải trắng             | 49.2          | kg         | Nước hầm         |
| 10      | NL-010    | Rong biển kombu / wakame | 7             | kg         | Nước hầm         |
| 11      | NL-011    | Hành tây                 | 21.3          | kg         | Nước hầm         |
| 12      | NL-012    | Nước dừa nguyên chất     | 55            | lít        | Nước hầm         |
| 13      | NL-013    | Muối biển rang           | 12.84         | kg         | Nêm              |
| 14      | NL-014    | Tiêu đen rang            | 1.32          | kg         | Nêm              |
| 15      | NL-015    | Tỏi nướng                | 0.72          | kg         | Nêm              |
| 16      | NL-016    | Hành lá                  | 2.19          | kg         | Nêm              |
| 17      | NL-017    | Rễ cần tây               | 1.25          | kg         | Nêm              |

**3.2. A2/CS/BASA - CHÁO SÂM - CÁ BASA**

**Thông tin đầu**

- SKU: A2/CS/BASA
- Tên sản phẩm: Cháo Sâm - Cá Basa
- Mã công thức: FML-A2-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú**       |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         | Dược liệu chủ đạo |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         | Dược liệu         |
| 3       | DL-003    | Bạch linh           | 9             | kg         | Dược liệu         |
| 4       | DL-004    | Kỷ tử               | 3.0           | kg         | Dược liệu         |
| 5       | DL-005    | Gừng nướng          | 0.4           | kg         | Dược liệu         |
| 6       | DL-006    | Trần bì             | 0.4           | kg         | Dược liệu         |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú**      |
| ------- | --------- | ------------------------ | ------------- | ---------- | ---------------- |
| 1       | NL-018    | Cá Basa                  | 22            | kg         | Thành phần chính |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         | Nền dinh dưỡng   |
| 3       | NL-004    | Cà rốt                   | 35            | kg         | Nền / nước hầm   |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         | Nền dinh dưỡng   |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         | Nền dinh dưỡng   |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         | Nền dinh dưỡng   |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         | Nước hầm         |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         | Nước hầm         |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         | Nước hầm         |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         | Nước hầm         |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        | Nước hầm         |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         | Nêm              |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         | Nêm              |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         | Nêm              |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         | Nêm              |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         | Nêm              |

**3.3. A3/CS/CAHOI - CHÁO SÂM - CÁ HỒI**

**Thông tin đầu**

- SKU: A3/CS/CAHOI
- Tên sản phẩm: Cháo Sâm - Cá hồi
- Mã công thức: FML-A3-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-019    | Cá hồi                   | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**3.4. A4/CS/LUON - CHÁO SÂM - LƯƠN ĐỒNG**

**Thông tin đầu**

- SKU: A4/CS/LUON
- Tên sản phẩm: Cháo Sâm - Lươn đồng
- Mã công thức: FML-A4-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-020    | Lươn đồng                | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**3.5. A5/CS/CUU - CHÁO SÂM - THỊT CỪU & TÁO TÀU**

**Thông tin đầu**

- SKU: A5/CS/CUU
- Tên sản phẩm: Cháo Sâm - Thịt cừu & Táo tàu
- Mã công thức: FML-A5-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |
| 5       | DL-007    | Quế chi             | 0.25          | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú**      |
| ------- | --------- | ------------------------ | ------------- | ---------- | ---------------- |
| 1       | NL-021    | Thịt cừu                 | 15.5          | kg         |                  |
| 2       | NL-008    | Táo tàu                  | 6.0           | kg         | Thành phần chính |
| 3       | NL-003    | Gạo lúa - tôm            | 195           | kg         |                  |
| 4       | NL-004    | Cà rốt                   | 35            | kg         |                  |
| 5       | NL-005    | Bí đỏ                    | 33            | kg         |                  |
| 6       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |                  |
| 7       | NL-007    | Nấm kim châm             | 18.5          | kg         |                  |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |                  |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |                  |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |                  |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |                  |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |                  |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |                  |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |                  |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |                  |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |                  |

**4\. NHÓM B - CHÁO SÂM CHỨC NĂNG**

**4.1. B1/CS/RM/ĐX - CHÁO SÂM - RAU MÁ & ĐẬU XANH**

**Thông tin đầu**

- SKU: B1/CS/RM/ĐX
- Tên sản phẩm: Cháo Sâm - Rau má & Đậu xanh
- Mã công thức: FML-B1-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-022    | Rau má                   | 15.5          | kg         |             |
| 2       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 3       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 4       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 5       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**4.2. B2/CS/DHA - CHÁO SÂM - DHA NÃO BỘ**

**Thông tin đầu**

- SKU: B2/CS/DHA
- Tên sản phẩm: Cháo Sâm - DHA Não bộ
- Mã công thức: FML-B2-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.4           | kg         |             |
| 5       | DL-006    | Trần bì             | 0.4           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-019    | Cá hồi                   | 15.5          | kg         |             |
| 2       | NL-023    | Vừng                     | 6.0           | kg         |             |
| 3       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 4       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 5       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 6       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 7       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 8       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 9       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 10      | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 11      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 12      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 13      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 14      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 15      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 16      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 17      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**4.3. B3/CS/CACOM - CHÁO SÂM - CÁ CƠM & VỪNG**

**Thông tin đầu**

- SKU: B3/CS/CACOM
- Tên sản phẩm: Cháo Sâm - Cá cơm & Vừng
- Mã công thức: FML-B3-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-024    | Cá cơm                   | 15.5          | kg         |             |
| 2       | NL-023    | Vừng                     | 6.0           | kg         |             |
| 3       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 4       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 5       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 6       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 7       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 8       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 9       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 10      | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 11      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 12      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 13      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 14      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 15      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 16      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 17      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**4.4. B4/CS/COLAGEN - CHÁO SÂM - THỊT HEO & DA HEO**

**Thông tin đầu**

- SKU: B4/CS/COLAGEN
- Tên sản phẩm: Cháo Sâm - Thịt heo & Da heo
- Mã công thức: FML-B4-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-025    | Thịt heo nạc             | 10.5          | kg         |             |
| 2       | NL-026    | Da heo                   | 5.0           | kg         |             |
| 3       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 4       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 5       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 6       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 7       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 8       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 9       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 10      | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 11      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 12      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 13      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 14      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 15      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 16      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 17      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**4.5. B5/CS/SINHLUC - CHÁO SÂM - HÀU BIỂN**

**Thông tin đầu**

- SKU: B5/CS/SINHLUC
- Tên sản phẩm: Cháo Sâm - Hàu biển
- Mã công thức: FML-B5-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-006    | Trần bì             | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-027    | Hàu biển                 | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**4.6. B6/CS/GAAC - CHÁO SÂM - GÀ ÁC**

**Thông tin đầu**

- SKU: B6/CS/GAAC
- Tên sản phẩm: Cháo Sâm - Gà ác
- Mã công thức: FML-B6-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-028    | Gà ác                    | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**5\. NHÓM C - CHÁO SÂM BỔ DƯỠNG**

**5.1. C1/CS/BAONGU - CHÁO SÂM - BÀO NGƯ**

**Thông tin đầu**

- SKU: C1/CS/BAONGU
- Tên sản phẩm: Cháo Sâm - Bào ngư
- Mã công thức: FML-C1-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-029    | Bào ngư                  | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**5.2. C2/CS/DONGTRUNG - CHÁO SÂM - ĐÔNG TRÙNG HẠ THẢO**

**Thông tin đầu**

- SKU: C2/CS/DONGTRUNG
- Tên sản phẩm: Cháo Sâm - Đông trùng hạ thảo
- Mã công thức: FML-C2-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-008    | Đông trùng hạ thảo  | 15.5          | kg         |             |
| 3       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 4       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 5       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 2       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 3       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 4       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 5       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 6       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 7       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 8       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 9       | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 10      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 11      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 12      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 13      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 14      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 15      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**5.3. C3/CS/NAMDONGCO - CHÁO SÂM - NẤM ĐÔNG CÔ**

**Thông tin đầu**

- SKU: C3/CS/NAMDONGCO
- Tên sản phẩm: Cháo Sâm - Nấm đông cô
- Mã công thức: FML-C3-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-030    | Nấm đông cô              | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**5.4. C4/CS/CUABIEN - CHÁO SÂM - CUA BIỂN**

**Thông tin đầu**

- SKU: C4/CS/CUABIEN
- Tên sản phẩm: Cháo Sâm - Cua biển
- Mã công thức: FML-C4-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-031    | Cua biển                 | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**5.5. C5/CS/CANGU - CHÁO SÂM - CÁ NGỪ**

**Thông tin đầu**

- SKU: C5/CS/CANGU
- Tên sản phẩm: Cháo Sâm - Cá ngừ
- Mã công thức: FML-C5-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-003    | Bạch linh           | 5.0           | kg         |             |
| 4       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 5       | DL-009    | Táo tàu             | 4.4           | kg         |             |
| 6       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-032    | Cá ngừ                   | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu dùng nước hầm    | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**5.6. C6/CS/TOM/RONGBIEN - CHÁO SÂM - TÔM & RONG BIỂN**

**Thông tin đầu**

- SKU: C6/CS/TOM/RONGBIEN
- Tên sản phẩm: Cháo Sâm - Tôm & Rong biển
- Mã công thức: FML-C6-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**                    | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | -------------------------------------- | ------------- | ---------- | ----------- |
| 1       | NL-033    | Tôm                                    | 15.5          | kg         |             |
| 2       | NL-034    | Rong biển                              | 15.5          | kg         |             |
| 3       | NL-003    | Gạo lúa - tôm                          | 195           | kg         |             |
| 4       | NL-004    | Cà rốt                                 | 35            | kg         |             |
| 5       | NL-005    | Bí đỏ                                  | 33            | kg         |             |
| 6       | NL-006    | Đậu xanh không vỏ                      | 29.5          | kg         |             |
| 7       | NL-007    | Nấm kim châm                           | 18.5          | kg         |             |
| 8       | NL-008    | Táo tàu                                | 4.4           | kg         |             |
| 9       | NL-009    | Củ cải trắng                           | 49.2          | kg         |             |
| 10      | NL-010    | Rong biển kombu / wakame dùng nước hầm | 7             | kg         |             |
| 11      | NL-011    | Hành tây                               | 21.3          | kg         |             |
| 12      | NL-012    | Nước dừa nguyên chất                   | 55            | lít        |             |
| 13      | NL-013    | Muối biển rang                         | 12.84         | kg         |             |
| 14      | NL-014    | Tiêu đen rang                          | 1.32          | kg         |             |
| 15      | NL-015    | Tỏi nướng                              | 0.72          | kg         |             |
| 16      | NL-016    | Hành lá                                | 2.19          | kg         |             |
| 17      | NL-017    | Rễ cần tây                             | 1.25          | kg         |             |

**5.7. C7/CS/THITGA - CHÁO SÂM - THỊT GÀ**

**Thông tin đầu**

- SKU: C7/CS/THITGA
- Tên sản phẩm: Cháo Sâm - Thịt gà
- Mã công thức: FML-C7-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-035    | Thịt gà                  | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**5.8. C8/CS/THITHEO - CHÁO SÂM - THỊT HEO**

**Thông tin đầu**

- SKU: C8/CS/THITHEO
- Tên sản phẩm: Cháo Sâm - Thịt heo
- Mã công thức: FML-C8-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-036    | Thịt heo                 | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**5.9. C9/CS/THITBO - CHÁO SÂM - THỊT BÒ**

**Thông tin đầu**

- SKU: C9/CS/THITBO
- Tên sản phẩm: Cháo Sâm - Thịt bò
- Mã công thức: FML-C9-G0
- Version: G0
- Trạng thái: Công thức gốc nghiên cứu / sản xuất mẫu

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |
| 1       | DL-001    | Sâm Savigin         | 9             | kg         |             |
| 2       | DL-002    | Hoài sơn            | 15.5          | kg         |             |
| 3       | DL-004    | Kỷ tử               | 3.0           | kg         |             |
| 4       | DL-005    | Gừng nướng          | 0.8           | kg         |             |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu**      | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------------ | ------------- | ---------- | ----------- |
| 1       | NL-037    | Thịt bò                  | 15.5          | kg         |             |
| 2       | NL-003    | Gạo lúa - tôm            | 195           | kg         |             |
| 3       | NL-004    | Cà rốt                   | 35            | kg         |             |
| 4       | NL-005    | Bí đỏ                    | 33            | kg         |             |
| 5       | NL-006    | Đậu xanh không vỏ        | 29.5          | kg         |             |
| 6       | NL-007    | Nấm kim châm             | 18.5          | kg         |             |
| 7       | NL-008    | Táo tàu                  | 4.4           | kg         |             |
| 8       | NL-009    | Củ cải trắng             | 49.2          | kg         |             |
| 9       | NL-010    | Rong biển kombu / wakame | 7             | kg         |             |
| 10      | NL-011    | Hành tây                 | 21.3          | kg         |             |
| 11      | NL-012    | Nước dừa nguyên chất     | 55            | lít        |             |
| 12      | NL-013    | Muối biển rang           | 12.84         | kg         |             |
| 13      | NL-014    | Tiêu đen rang            | 1.32          | kg         |             |
| 14      | NL-015    | Tỏi nướng                | 0.72          | kg         |             |
| 15      | NL-016    | Hành lá                  | 2.19          | kg         |             |
| 16      | NL-017    | Rễ cần tây               | 1.25          | kg         |             |

**6\. QUY TẮC CỨNG**

**6.1. Không SKU nào được sản xuất nếu chưa có BOM / Recipe Map**

**6.2. Không cho vận hành chọn lại nguyên liệu ở bước sản xuất**

**6.3. Mọi thay đổi nguyên liệu hoặc hàm lượng phải đi qua version công thức**

**6.4. Tất cả nguyên liệu phải lấy từ danh mục nguyên liệu canonical**

**6.5. Mọi phiếu cấp nguyên liệu cho sản xuất phải bám đúng map này**

**6.6. Tất cả nguyên liệu phải hiện đầy đủ từng dòng, không dùng dòng gom nhóm mơ hồ**

**7\. PASS / FAIL GATE**

**7.1. Đạt khi**

- đủ 20 SKU có BOM / Recipe Map
- mỗi SKU map được ra danh sách dược liệu và nguyên liệu
- có hàm lượng và đơn vị
- có mã công thức và version
- hệ thống dùng được map này để tự hiện công thức
- dùng được để sinh phiếu đề nghị cấp nguyên liệu cho sản xuất

**7.2. Chưa đạt khi**

- thiếu SKU
- thiếu nguyên liệu
- thiếu hàm lượng
- thiếu version
- vẫn phải suy luận tay từ tên SKU
- vẫn dùng dòng gom nhóm thay vì nguyên liệu cụ thể

bản **"20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD"** theo cùng format sạch, Word-ready.

Bản này nối từ:

- **BỘ 20 SKU CANONICAL - GINSENGFOOD**
- **20 SKU ↔ NGUYÊN LIỆU / BOM MAP - GINSENGFOOD**
- **bộ quy tắc phiếu tự sinh**
- **quy tắc in mã 2 giai đoạn**
- **quy tắc kết nối MISA toàn hệ**

Mục đích của file này là khóa cho từng SKU:

- đang dùng công thức nào
- đóng gói cấp 1 / cấp 2 thế nào
- in gì ở từng cấp
- QC nào bắt buộc
- điều kiện release nào
- trace / recall áp dụng ra sao
- đơn vị vận hành chính là gì

Đây là file rất quan trọng, vì nếu không có nó thì dù có SKU, nguyên liệu và BOM, đội dev vẫn phải tự suy luận cách vận hành từng SKU trong Operational.

**20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD**

**0\. MỞ ĐẦU TÀI LIỆU**

**0.1. Mục đích tài liệu**

Tài liệu này dùng để khóa **cấu hình vận hành theo từng SKU** cho Operational Domain của Ginsengfood.

Tài liệu này chỉ dùng để khóa:

- công thức đang áp dụng theo SKU
- quy cách đóng gói cấp 1
- quy cách đóng gói cấp 2
- nội dung in cấp 1
- nội dung in cấp 2
- QC checkpoint bắt buộc
- điều kiện release
- traceability rule
- recall applicability
- đơn vị vận hành chính theo từng công đoạn

**0.2. Vai trò tài liệu**

Đây là mắt xích nối giữa:

- **SKU Master**
- **BOM / Recipe Map**
- **Packaging**
- **QC / Release**
- **Traceability**
- **Recall**
- **MISA Integration**

**0.3. Ranh giới tài liệu**

Tài liệu này **không giữ**:

- master SKU
- danh mục nguyên liệu
- chi tiết BOM từng dòng nguyên liệu
- phiếu vận hành chi tiết
- code API / code UI
- logic kế toán chi tiết

Tài liệu này chỉ giữ **operational config theo SKU**.

**0.4. Nguyên tắc dùng tài liệu**

Operational Domain không owner master SKU.  
Operational chỉ owner:

- config vận hành theo SKU
- dữ liệu vận hành phát sinh từ SKU

Mọi SKU trong Operational phải đọc từ bộ SKU canonical đã khóa trước.

**1\. NGUYÊN TẮC CHUNG**

**1.1. Một SKU phải có đủ operational config mới được vận hành**

Một SKU chỉ được xem là **sẵn sàng vận hành** khi có đủ:

- BOM / Recipe version đang áp dụng
- config đóng gói cấp 1
- config đóng gói cấp 2
- config in mã
- config QC
- config release
- config trace
- config recall

**1.2. Không SKU nào được sản xuất nếu thiếu config**

Nếu thiếu một trong các cấu hình trên, SKU đó chưa được đưa vào luồng vận hành chính thức.

**1.3. Operational config phải theo version**

Mỗi SKU phải ghi rõ:

- version công thức đang áp dụng
- version operational config đang áp dụng nếu có
- ngày hiệu lực

**1.4. Mọi thay đổi config phải đi qua approval**

Nếu thay đổi:

- đơn vị đóng gói
- nội dung in
- QC checkpoint
- điều kiện release
- trace rule
- recall rule

thì phải tạo version mới hoặc bản config mới, không sửa đè bản đã dùng cho mẻ cũ.

**2\. CẤU TRÚC CHUẨN CHO MỖI SKU OPERATIONAL CONFIG**

**2.1. Thông tin đầu**

- SKU
- Tên sản phẩm
- Nhóm sản phẩm
- Phân loại Vegan / Mặn
- Mã công thức đang áp dụng
- Version công thức
- Trạng thái operational config

**2.2. Packaging Config**

- đơn vị cấp 1
- đơn vị cấp 2
- quy cách gom cấp 1 → cấp 2
- quy cách gom cấp 2 → nhập kho

**2.3. Print Config**

- nội dung in cấp 1
- nội dung in cấp 2
- template in cấp 1
- template in cấp 2
- rule reprint

**2.4. QC / Release Config**

- checkpoint QC bắt buộc
- có bắt buộc % độ ẩm sau sấy hay không
- điều kiện đạt để vào đóng gói
- điều kiện đạt để nhập kho

**2.5. Trace / Recall Config**

- public trace có / không
- internal trace có / không
- đơn vị truy vết chính
- recall applicability
- cấp dữ liệu dùng cho recall

**3\. BỘ QUY TẮC CẤU HÌNH CHUNG ÁP DỤNG CHO 20 SKU**

**3.1. Công thức**

- Dùng công thức version G0 làm baseline nghiên cứu ban đầu, không dùng trực tiếp cho production
- Khi tối ưu, tạo version mới
- Lệnh sản xuất luôn kéo version đang áp dụng

**3.2. Đóng gói cấp 1**

Đơn vị được phép:

- gói
- lọ
- hũ

**3.3. Đóng gói cấp 2**

Đơn vị được phép:

- hộp
- thùng
- lọ
- hũ

**3.4. In cấp 1**

Chỉ in:

- ngày sản xuất
- hạn dùng

**3.5. In cấp 2**

Phải in:

- lô sản xuất
- ngày sản xuất
- hạn dùng
- mã vạch
- mã QR

**3.6. QC sau sấy**

Bắt buộc có:

- số lượng theo khay
- % độ ẩm

**3.7. Nhập kho thành phẩm**

Đơn vị nhập kho:

- hộp
- thùng
- lọ
- hũ

**3.8. Traceability**

Public trace và internal trace phải tách rõ.  
Cấp 2 là cấp mang dữ liệu truy vết chính.

**3.9. Recall**

Recall phải lần ngược được:

- đơn vị cấp 2
- batch / lot
- mẻ sản xuất
- lô nguyên liệu

**4\. NHÓM A - CHÁO SÂM THEO MÙA**

**4.1. A1/CS/DM/HS - CHÁO SÂM - DIÊM MẠCH & HẠT SEN**

**Thông tin đầu**

- SKU: A1/CS/DM/HS
- Tên sản phẩm: Cháo Sâm - Diêm mạch & Hạt sen
- Nhóm sản phẩm: Theo mùa
- Phân loại: Không thuần chay (vegan claim khóa do công thức G0/G1 có thành phần động vật)
- Mã công thức áp dụng: FML-A1-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng
- Quy cách gom cấp 1 → cấp 2: theo cấu hình đóng gói
- Quy cách gom cấp 2 → nhập kho: theo hộp / thùng

**Print Config**

- In cấp 1: ngày sản xuất, hạn dùng
- In cấp 2: lô sản xuất, ngày sản xuất, hạn dùng, barcode, QR
- Reprint cấp 2: có kiểm soát

**QC / Release Config**

- Bắt buộc phiếu theo dõi sơ chế
- Bắt buộc kiểm chất lượng sau sấy
- Bắt buộc % độ ẩm
- Bắt buộc QC thành phẩm trước nhập kho

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Đơn vị truy vết chính: hộp
- Recall applicability: Có

**4.2. A2/CS/BASA - CHÁO SÂM - CÁ BASA**

**Thông tin đầu**

- SKU: A2/CS/BASA
- Tên sản phẩm: Cháo Sâm - Cá Basa
- Nhóm sản phẩm: Theo mùa
- Phân loại: Mặn
- Mã công thức áp dụng: FML-A2-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Có kiểm sau sấy
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**4.3. A3/CS/CAHOI - CHÁO SÂM - CÁ HỒI**

**Thông tin đầu**

- SKU: A3/CS/CAHOI
- Tên sản phẩm: Cháo Sâm - Cá hồi
- Nhóm sản phẩm: Theo mùa
- Phân loại: Mặn
- Mã công thức áp dụng: FML-A3-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Bắt buộc kiểm sau sấy
- Bắt buộc % độ ẩm
- QC thành phẩm trước nhập kho

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**4.4. A4/CS/LUON - CHÁO SÂM - LƯƠN ĐỒNG**

**Thông tin đầu**

- SKU: A4/CS/LUON
- Tên sản phẩm: Cháo Sâm - Lươn đồng
- Nhóm sản phẩm: Theo mùa
- Phân loại: Mặn
- Mã công thức áp dụng: FML-A4-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Có kiểm sau sấy
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**4.5. A5/CS/CUU - CHÁO SÂM - THỊT CỪU & TÁO TÀU**

**Thông tin đầu**

- SKU: A5/CS/CUU
- Tên sản phẩm: Cháo Sâm - Thịt cừu & Táo tàu
- Nhóm sản phẩm: Theo mùa
- Phân loại: Mặn
- Mã công thức áp dụng: FML-A5-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Có kiểm sau sấy
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**5\. NHÓM B - CHÁO SÂM CHỨC NĂNG**

**5.1. B1/CS/RM/ĐX - CHÁO SÂM - RAU MÁ & ĐẬU XANH**

**Thông tin đầu**

- SKU: B1/CS/RM/ĐX
- Tên sản phẩm: Cháo Sâm - Rau má & Đậu xanh
- Nhóm sản phẩm: Chức năng
- Phân loại: Không thuần chay (vegan claim khóa do công thức G0/G1 có thành phần động vật)
- Mã công thức áp dụng: FML-B1-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**5.2. B2/CS/DHA - CHÁO SÂM - DHA NÃO BỘ**

**Thông tin đầu**

- SKU: B2/CS/DHA
- Tên sản phẩm: Cháo Sâm - DHA Não bộ
- Nhóm sản phẩm: Chức năng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-B2-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**5.3. B3/CS/CACOM - CHÁO SÂM - CÁ CƠM & VỪNG**

**Thông tin đầu**

- SKU: B3/CS/CACOM
- Tên sản phẩm: Cháo Sâm - Cá cơm & Vừng
- Nhóm sản phẩm: Chức năng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-B3-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**5.4. B4/CS/COLAGEN - CHÁO SÂM - THỊT HEO & DA HEO**

**Thông tin đầu**

- SKU: B4/CS/COLAGEN
- Tên sản phẩm: Cháo Sâm - Thịt heo & Da heo
- Nhóm sản phẩm: Chức năng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-B4-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**5.5. B5/CS/SINHLUC - CHÁO SÂM - HÀU BIỂN**

**Thông tin đầu**

- SKU: B5/CS/SINHLUC
- Tên sản phẩm: Cháo Sâm - Hàu biển
- Nhóm sản phẩm: Chức năng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-B5-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**5.6. B6/CS/GAAC - CHÁO SÂM - GÀ ÁC**

**Thông tin đầu**

- SKU: B6/CS/GAAC
- Tên sản phẩm: Cháo Sâm - Gà ác
- Nhóm sản phẩm: Chức năng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-B6-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**6\. NHÓM C - CHÁO SÂM BỔ DƯỠNG**

**6.1. C1/CS/BAONGU - CHÁO SÂM - BÀO NGƯ**

**Thông tin đầu**

- SKU: C1/CS/BAONGU
- Tên sản phẩm: Cháo Sâm - Bào ngư
- Nhóm sản phẩm: Bổ dưỡng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-C1-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**6.2. C2/CS/DONGTRUNG - CHÁO SÂM - ĐÔNG TRÙNG HẠ THẢO**

**Thông tin đầu**

- SKU: C2/CS/DONGTRUNG
- Tên sản phẩm: Cháo Sâm - Đông trùng hạ thảo
- Nhóm sản phẩm: Bổ dưỡng
- Phân loại: Không thuần chay (vegan claim khóa do công thức G0/G1 có thành phần động vật)
- Mã công thức áp dụng: FML-C2-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**6.3. C3/CS/NAMDONGCO - CHÁO SÂM - NẤM ĐÔNG CÔ**

**Thông tin đầu**

- SKU: C3/CS/NAMDONGCO
- Tên sản phẩm: Cháo Sâm - Nấm đông cô
- Nhóm sản phẩm: Bổ dưỡng
- Phân loại: Không thuần chay (vegan claim khóa do công thức G0/G1 có thành phần động vật)
- Mã công thức áp dụng: FML-C3-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**6.4. C4/CS/CUABIEN - CHÁO SÂM - CUA BIỂN**

**Thông tin đầu**

- SKU: C4/CS/CUABIEN
- Tên sản phẩm: Cháo Sâm - Cua biển
- Nhóm sản phẩm: Bổ dưỡng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-C4-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**6.5. C5/CS/CANGU - CHÁO SÂM - CÁ NGỪ**

**Thông tin đầu**

- SKU: C5/CS/CANGU
- Tên sản phẩm: Cháo Sâm - Cá ngừ
- Nhóm sản phẩm: Bổ dưỡng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-C5-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**6.6. C6/CS/TOM/RONGBIEN - CHÁO SÂM - TÔM & RONG BIỂN**

**Thông tin đầu**

- SKU: C6/CS/TOM/RONGBIEN
- Tên sản phẩm: Cháo Sâm - Tôm & Rong biển
- Nhóm sản phẩm: Bổ dưỡng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-C6-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**6.7. C7/CS/THITGA - CHÁO SÂM - THỊT GÀ**

**Thông tin đầu**

- SKU: C7/CS/THITGA
- Tên sản phẩm: Cháo Sâm - Thịt gà
- Nhóm sản phẩm: Bổ dưỡng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-C7-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**6.8. C8/CS/THITHEO - CHÁO SÂM - THỊT HEO**

**Thông tin đầu**

- SKU: C8/CS/THITHEO
- Tên sản phẩm: Cháo Sâm - Thịt heo
- Nhóm sản phẩm: Bổ dưỡng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-C8-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**6.9. C9/CS/THITBO - CHÁO SÂM - THỊT BÒ**

**Thông tin đầu**

- SKU: C9/CS/THITBO
- Tên sản phẩm: Cháo Sâm - Thịt bò
- Nhóm sản phẩm: Bổ dưỡng
- Phân loại: Mặn
- Mã công thức áp dụng: FML-C9-G0
- Version công thức: G0
- Trạng thái config: ACTIVE_PILOT

**Packaging Config**

- Đơn vị cấp 1: gói
- Đơn vị cấp 2: hộp / thùng

**Print Config**

- Cấp 1: NSX, HSD
- Cấp 2: Lô, NSX, HSD, barcode, QR

**QC / Release Config**

- Có sơ chế
- Bắt buộc % độ ẩm
- QC thành phẩm bắt buộc

**Trace / Recall Config**

- Public trace: Có
- Internal trace: Có
- Recall applicability: Có

**7\. QUY TẮC CỨNG**

**7.1. Không SKU nào được đưa vào sản xuất nếu thiếu operational config**

**7.2. Không SKU nào được đóng gói nếu thiếu packaging config**

**7.3. Không SKU nào được in mã cấp 2 nếu thiếu print config cấp 2**

**7.4. Không SKU nào được nhập kho nếu thiếu QC / release config**

**7.5. Không SKU nào được public trace nếu chưa bật trace config**

**7.6. Không SKU nào được recall chuẩn nếu thiếu trace / recall config**

**7.7. Operational config không được sửa đè âm thầm; mọi thay đổi phải có version hoặc bản config mới**

**8\. PASS / FAIL GATE**

**8.1. Đạt khi**

- đủ 20 SKU có operational config
- mỗi SKU có:
  - công thức áp dụng
  - packaging cấp 1 / cấp 2
  - print rule
  - QC / release rule
  - trace / recall rule
- không phải đoán thủ công khi vận hành
- hệ thống dùng được config này để điều phối production → packaging → QC → nhập kho → trace → recall

**8.2. Chưa đạt khi**

- SKU có BOM nhưng thiếu operational config
- đóng gói / in mã / QC / trace vẫn phải đoán tay
- thiếu rule cấp 1 / cấp 2
- thiếu recall applicability
- thiếu version hoặc trạng thái config

**9\. KẾT LUẬN**

Từ đây, **20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD** được khóa ở mức:

- mỗi SKU có cấu hình vận hành riêng
- packaging / print / QC / release / trace / recall đã có rule nền
- đủ để đi tiếp sang:
  - **Quy tắc version công thức / phê duyệt thay đổi công thức**
  - **Trace data chain / batch-lot-QR-box-case map**
  - **Bộ đặc tả màn hình / route / action cho toàn bộ phiếu vận hành**

bản **"QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC - GINSENGFOOD"** theo format sạch, Word-ready.

File này là mắt xích rất quan trọng, vì nếu không khóa chỗ này thì sau khi chạy vài mẻ thực tế, công thức sẽ rất dễ bị sửa chồng, mất lịch sử, không biết mẻ nào dùng version nào, và lúc đó truy lỗi, tối ưu cảm quan, tính giá thành hay recall đều rất khó. Với hệ thống thật, không thể chỉ copy-paste vài dòng code AI rồi mong tự quản được lịch sử công thức; phải khóa rõ version, approval, hiệu lực và dấu vết thay đổi.

**QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC - GINSENGFOOD**

**0\. MỞ ĐẦU TÀI LIỆU**

**0.1. Mục đích tài liệu**

Tài liệu này dùng để khóa quy tắc:

- tạo version công thức
- phân loại công thức gốc / pilot / áp dụng chính thức
- phê duyệt thay đổi công thức
- ghi nhận hiệu lực theo thời gian và theo mẻ sản xuất
- truy xuất lịch sử thay đổi công thức

**0.2. Vai trò tài liệu**

Tài liệu này là nguồn chuẩn để:

- đội R&D / kỹ thuật công thức đề xuất thay đổi
- quản lý nhà máy phê duyệt dùng trong sản xuất
- QC kiểm theo đúng version
- kho cấp nguyên liệu theo đúng version
- kế toán tính chi phí theo đúng version
- dev dựng đúng logic version / approval / trace lịch sử

**0.3. Ranh giới tài liệu**

Tài liệu này không thay:

- bộ 20 SKU canonical
- danh mục nguyên liệu canonical
- BOM / Recipe Map hiện hành
- Operational Config Map
- bộ phiếu vận hành

Tài liệu này chỉ giữ **rule version và approval thay đổi công thức**.

**0.4. Nguyên tắc nền**

- công thức nghiên cứu trong đề tài là **baseline ban đầu**
- sản xuất thực tế có thể phải tối ưu dần
- mọi thay đổi phải đi qua **version mới**
- không sửa đè version cũ
- mỗi mẻ phải truy được mình đã dùng version nào

**1\. KẾT LUẬN KHÓA**

**1.1. Kết luận tổng quát**

Mọi công thức trong Ginsengfood phải được quản lý theo version.

Không cho phép:

- sửa trực tiếp công thức đang dùng mà không tạo version mới
- thay đổi nguyên liệu / hàm lượng trong lệnh sản xuất thường
- mất lịch sử version cũ

**1.2. Kết luận về version**

Mỗi công thức phải có tối thiểu:

- mã công thức
- version
- trạng thái version
- ngày hiệu lực
- người tạo
- người phê duyệt
- lý do thay đổi
- phạm vi thay đổi

**1.3. Kết luận về phê duyệt**

Không version nào được đưa vào sản xuất thường nếu chưa được phê duyệt đúng vai trò.

**1.4. Kết luận về truy xuất**

Mỗi mẻ sản xuất phải ghi được:

- SKU
- mã công thức
- version công thức
- thời điểm áp dụng

**2\. PHÂN LOẠI VERSION CÔNG THỨC**

**2.1. Version gốc nghiên cứu**

Là version baseline đầu tiên, lấy trực tiếp từ đề tài nghiên cứu.

Vai trò:

- làm chuẩn gốc ban đầu
- làm mốc đối chiếu khi tối ưu về sau

Quy ước khuyến nghị:

- G0

**2.2. Version pilot / sản xuất mẫu**

Là version được dùng để chạy thử thực tế tại nhà máy, chưa phải bản áp dụng đại trà.

Vai trò:

- kiểm tra cảm quan
- kiểm tra vận hành
- kiểm tra hao hụt
- kiểm tra ổn định sấy / đóng gói
- kiểm tra tính khả thi sản xuất công nghiệp

Quy ước khuyến nghị:

- P1, P2, P3...

**2.3. Version áp dụng chính thức**

Là version đã qua đánh giá và được phép dùng cho vận hành sản xuất thường.

Vai trò:

- dùng cho lệnh sản xuất chuẩn
- dùng để cấp phát nguyên liệu
- dùng cho QC / release / trace / recall

Quy ước khuyến nghị:

- R1, R2, R3...

**2.4. Version ngưng áp dụng**

Là version cũ không còn dùng cho sản xuất mới nhưng vẫn phải lưu để:

- truy lịch sử
- đối chiếu mẻ cũ
- phục vụ kiểm tra / recall / phân tích lỗi

**3\. TRẠNG THÁI VERSION BẮT BUỘC**

**3.1. DRAFT**

Mới tạo, chưa được dùng.

**3.2. REVIEW_PENDING**

Đã gửi duyệt, chờ xem xét.

**3.3. PILOT_APPROVED**

Được phép chạy pilot / sản xuất mẫu.

**3.4. ACTIVE**

Được phép dùng cho sản xuất thường.

**3.5. REJECTED**

Bị từ chối, không được dùng.

**3.6. DEPRECATED**

Ngưng áp dụng, giữ để tra cứu lịch sử.

**4\. KHI NÀO BẮT BUỘC TẠO VERSION MỚI**

Phải tạo version mới khi có thay đổi về:

**4.1. Thành phần nguyên liệu**

- thêm nguyên liệu
- bỏ nguyên liệu
- thay nguyên liệu này bằng nguyên liệu khác

**4.2. Hàm lượng**

- tăng hàm lượng
- giảm hàm lượng
- đổi định mức theo mẻ

**4.3. Đơn vị hoặc quy cách áp dụng**

- đổi kg sang lít / ml khi bản chất công thức thay đổi
- đổi quy cách sử dụng trong sản xuất

**4.4. Ghi chú kỹ thuật ảnh hưởng vận hành**

- đổi cách sơ chế
- đổi cách phối trộn
- đổi thứ tự cho nguyên liệu vào
- đổi rule sấy / hoàn nguyên nếu gắn trực tiếp với công thức

**4.5. Thay đổi ảnh hưởng cảm quan hoặc chất lượng**

- thay đổi để ngon hơn
- thay đổi để ổn định hơn
- thay đổi để giảm hao hụt
- thay đổi để phù hợp sản xuất công nghiệp

**5\. NHỮNG GÌ KHÔNG ĐƯỢC SỬA ĐÈ**

Không được sửa đè trực tiếp trên version đã có hiệu lực đối với các trường:

- danh sách nguyên liệu
- hàm lượng
- đơn vị
- ghi chú kỹ thuật cốt lõi
- logic áp dụng theo mẻ
- trạng thái version cũ nếu đã từng được dùng

Nếu có thay đổi, phải tạo record version mới.

**6\. THÔNG TIN BẮT BUỘC CỦA MỖI VERSION**

Mỗi version công thức phải có tối thiểu:

- SKU
- tên sản phẩm
- mã công thức
- version
- loại version:
  - Gốc nghiên cứu
  - Pilot
  - Chính thức
- trạng thái
- ngày tạo
- người tạo
- ngày gửi duyệt
- người duyệt
- ngày hiệu lực
- lý do thay đổi
- mô tả thay đổi
- version nguồn tham chiếu
- ghi chú kỹ thuật

**7\. QUY TẮC PHÊ DUYỆT THAY ĐỔI CÔNG THỨC**

**7.1. Người được đề xuất thay đổi**

Tối thiểu có thể gồm:

- R&D
- kỹ thuật sản xuất
- quản lý nhà máy
- QC
- người được phân quyền

**7.2. Nội dung bắt buộc khi đề xuất**

Mỗi đề xuất phải có:

- SKU nào
- version hiện tại là gì
- version đề xuất là gì
- nguyên liệu nào thay đổi
- hàm lượng nào thay đổi
- lý do thay đổi
- mục tiêu thay đổi:
  - cảm quan
  - ổn định
  - hao hụt
  - khả năng sản xuất
  - chi phí
- đề xuất áp dụng từ mẻ nào

**7.3. Người phê duyệt bắt buộc**

Tối thiểu phải có:

- quản lý nhà máy
- bộ phận kỹ thuật / R&D nếu có
- QC hoặc người chịu trách nhiệm chất lượng

Nếu version dùng cho sản xuất thường, không chỉ pilot, thì phải đủ các xác nhận bắt buộc.

**7.4. Điều kiện phê duyệt version pilot**

Version pilot chỉ được duyệt khi:

- có lý do rõ ràng
- có thay đổi được mô tả rõ
- không mâu thuẫn với danh mục nguyên liệu
- không phá rule đóng gói / QC / trace
- có thể truy ngược về version nguồn

**7.5. Điều kiện phê duyệt version ACTIVE**

Version ACTIVE chỉ được duyệt khi:

- đã có kết quả pilot hoặc kiểm chứng thực tế
- đã có đánh giá cảm quan / chất lượng
- đã có xác nhận từ quản lý nhà máy
- đã có xác nhận từ QC
- đã chốt được dùng từ mẻ nào / ngày nào

**8\. QUY TẮC ÁP DỤNG VERSION VÀO SẢN XUẤT**

**8.1. Lệnh sản xuất chỉ được kéo version hợp lệ**

Khi chọn SKU, hệ thống chỉ được tự hiện:

- version ở trạng thái ACTIVE  
   hoặc
- version PILOT_APPROVED nếu lệnh đó được đánh dấu chạy pilot

**8.2. Không cho chọn version DRAFT / REJECTED / DEPRECATED**

Các version này không được hiện như lựa chọn hợp lệ trong lệnh sản xuất thường.

**8.3. Mỗi mẻ phải ghi rõ version đã dùng**

Mọi mẻ sản xuất phải lưu:

- SKU
- mã công thức
- version công thức
- ngày áp dụng
- người mở lệnh

**8.4. Một mẻ chỉ dùng một version**

Một mẻ sản xuất không được trộn nhiều version công thức trong cùng một batch vận hành thường, trừ khi có phiếu ngoại lệ được phê duyệt riêng.

**9\. TRUY XUẤT LỊCH SỬ VERSION**

**9.1. Hệ thống phải truy được version nguồn**

Mỗi version mới phải biết mình được tạo từ:

- version nào trước đó

**9.2. Hệ thống phải truy được mẻ đã dùng version nào**

Khi xem một mẻ, phải biết:

- dùng version nào
- ai duyệt version đó
- thay đổi gì so với version trước

**9.3. Hệ thống phải truy được toàn bộ lịch sử thay đổi**

Phải nhìn được chuỗi như:

- G0 → P1 → P2 → R1 → R2 ...

**10\. QUY TẮC NGOẠI LỆ**

**10.1. Thay đổi khẩn cấp**

Nếu vì lý do:

- nguyên liệu thiếu
- nguyên liệu không đạt
- cần thay thế khẩn cấp

thì phải đi qua một phiếu thay đổi / thay thế nguyên liệu riêng, không sửa tay ngay trên lệnh sản xuất.

**10.2. Mọi ngoại lệ đều phải để lại dấu vết**

Bắt buộc có:

- người đề xuất
- người duyệt
- lý do
- phạm vi áp dụng
- hiệu lực cho mẻ nào

**11\. QUY TẮC LIÊN KẾT VỚI CÁC KHỐI KHÁC**

**11.1. Liên kết với BOM / Recipe Map**

Mỗi version công thức phải trỏ được về đúng BOM / Recipe Map tương ứng.

**11.2. Liên kết với phiếu vận hành**

Lệnh sản xuất, phiếu cấp phát, QC, sấy, đóng gói, nhập kho phải đều nhìn thấy version đang dùng.

**11.3. Liên kết với Operational Config Map**

Nếu version công thức làm thay đổi:

- packaging
- in mã
- QC
- trace
- recall

thì phải cập nhật version operational config tương ứng, không được để lệch.

**11.4. Liên kết với traceability**

Version công thức phải là một phần của trace nội bộ để sau này biết mẻ đó sản xuất theo version nào.

**12\. KHỐI THÔNG TIN MẪU CHO MỘT VERSION**

**12.1. Thông tin đầu**

- SKU:
- Tên sản phẩm:
- Mã công thức:
- Version:
- Loại version:
- Trạng thái:

**12.2. Version nguồn**

- Version trước đó:
- Ngày tạo:
- Người tạo:

**12.3. Lý do thay đổi**

- Mục tiêu thay đổi:
- Mô tả thay đổi:
- Phạm vi thay đổi:

**12.4. Phê duyệt**

- Người gửi duyệt:
- Ngày gửi duyệt:
- Người phê duyệt:
- Ngày phê duyệt:
- Kết luận:
  - Pilot approved
  - Active
  - Rejected

**12.5. Hiệu lực**

- Áp dụng từ ngày:
- Áp dụng từ mẻ:
- Ghi chú triển khai:

**13\. QUY TẮC CỨNG**

**13.1. Không sửa đè version cũ**

**13.2. Không sản xuất bằng version chưa được phép**

**13.3. Không cho đổi nguyên liệu trong lệnh sản xuất thường**

**13.4. Mọi thay đổi công thức phải đi qua version mới**

**13.5. Mọi mẻ phải truy được về version đã dùng**

**13.6. Version pilot và version active phải phân biệt rõ**

**13.7. Nếu thay đổi công thức ảnh hưởng đóng gói / QC / trace thì phải cập nhật config liên quan**

**14\. PASS / FAIL GATE**

**14.1. Đạt khi**

- có version rõ ràng
- có trạng thái version rõ ràng
- không sửa đè version cũ
- có luồng đề xuất / duyệt / hiệu lực
- mẻ sản xuất truy được version
- hệ thống chỉ cho dùng version hợp lệ

**14.2. Chưa đạt khi**

- sửa trực tiếp công thức cũ
- không biết version nào đang dùng
- mẻ không truy được về version
- không có người duyệt
- version chưa approved vẫn đi vào sản xuất

**15\. KẾT LUẬN**

Từ đây, **QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC - GINSENGFOOD** được khóa ở mức:

- có baseline gốc
- có pilot
- có active
- có deprecated
- có approval
- có hiệu lực
- có truy xuất lịch sử
- có liên kết sang BOM / Operational / Trace

Đây là nền để đi tiếp sang:

- **TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP**
- **BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH**

bản **"TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP - GINSENGFOOD"** theo format sạch, Word-ready.

File này là mắt xích khóa rất quan trọng giữa:

- nguyên liệu đầu vào
- lô nguyên liệu
- lệnh sản xuất / mẻ sản xuất
- batch thành phẩm
- đóng gói cấp 1
- đóng gói cấp 2
- mã vạch / QR
- truy vết nội bộ
- truy vết công khai
- thu hồi sản phẩm

Nếu không khóa khối này, về sau hệ thống có thể vẫn nhập kho, vẫn in QR, vẫn bán hàng, nhưng sẽ không truy ngược được rõ:

- hộp nào thuộc batch nào
- batch nào dùng lô nguyên liệu nào
- thùng nào chứa hộp nào
- QR nào đại diện cho đơn vị nào
- recall phải chặn ở cấp nào

**TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP - GINSENGFOOD**

**0\. MỞ ĐẦU TÀI LIỆU**

**0.1. Mục đích tài liệu**

Tài liệu này dùng để khóa **chuỗi dữ liệu truy vết** của Ginsengfood từ đầu vào nguyên liệu đến thành phẩm đóng gói và thu hồi.

Tài liệu này chỉ dùng để khóa:

- lot nguyên liệu
- lệnh sản xuất / mẻ sản xuất
- batch thành phẩm
- đơn vị đóng gói cấp 1
- đơn vị đóng gói cấp 2
- barcode
- QR
- chuỗi dữ liệu phục vụ internal trace
- chuỗi dữ liệu phục vụ public trace
- chuỗi dữ liệu phục vụ recall

**0.2. Vai trò tài liệu**

Tài liệu này là mắt xích nối giữa:

- **phiếu nhập nguyên liệu đầu vào**
- **phiếu chấp thuận nguyên liệu đưa vào sản xuất**
- **lệnh sản xuất**
- **quy tắc in mã 2 giai đoạn**
- **Operational Config Map**
- **Recall engine**

**0.3. Ranh giới tài liệu**

Tài liệu này không giữ:

- công thức chi tiết từng SKU
- giá kế toán
- KPI / thưởng / thuế
- UI code
- API code

Tài liệu này chỉ giữ **chuỗi dữ liệu truy vết và map đơn vị truy vết**.

**0.4. Nguyên tắc nền**

- nguyên liệu đầu vào phải có **lô nguyên liệu**
- lệnh sản xuất phải có **mẻ sản xuất**
- thành phẩm phải có **batch / lô sản xuất**
- cấp đóng gói 2 phải có **barcode / QR**
- dữ liệu truy vết phải lần ngược và lần xuôi được
- thu hồi phải reuse chính chuỗi này, không dựng chuỗi song song

**1\. KẾT LUẬN KHÓA**

**1.1. Kết luận tổng quát**

Chuỗi truy vết chuẩn của Ginsengfood phải đi theo tuyến:

**Lot nguyên liệu**  
→ **Phiếu chấp thuận nguyên liệu đưa vào sản xuất**  
→ **Lệnh sản xuất / mẻ sản xuất**  
→ **Batch thành phẩm**  
→ **Đóng gói cấp 1**  
→ **Đóng gói cấp 2**  
→ **Barcode / QR**  
→ **Kho thành phẩm / giao hàng / truy vết / recall**

**1.2. Cấp truy vết chính**

Cấp truy vết chính cho khách hàng và truy xuất thương mại nên là:

- **đơn vị đóng gói cấp 2**  
   ví dụ: **hộp**

**1.3. Cấp truy vết nội bộ**

Cấp truy vết nội bộ phải đi sâu được tới:

- batch thành phẩm
- mẻ sản xuất
- lô nguyên liệu đầu vào

**1.4. Cấp thu hồi**

Recall phải khóa được tối thiểu ở các cấp:

- batch
- hộp
- thùng

Tùy tình huống có thể mở rộng sâu hơn.

**2\. ĐỊNH NGHĨA CÁC ĐƠN VỊ TRONG CHUỖI**

**2.1. Lot nguyên liệu**

Là lô nguyên liệu đầu vào nhập kho.

Mỗi lot nguyên liệu phải gắn với:

- mã nguyên liệu
- nhà cung cấp
- ngày nhập
- kết quả đánh giá đầu vào
- vị trí kho
- mã lô nguyên liệu

**2.2. Mẻ sản xuất**

Là một lần vận hành sản xuất theo một lệnh sản xuất và một công thức/version cụ thể.

Mỗi mẻ sản xuất phải gắn với:

- mã hồ sơ sản xuất
- mã lệnh sản xuất
- SKU
- công thức
- version công thức
- thời gian sản xuất
- nhân sự / ca sản xuất

**2.3. Batch thành phẩm**

Là lô thành phẩm sinh ra từ một mẻ sản xuất.

Mỗi batch thành phẩm phải gắn với:

- mã batch
- mẻ sản xuất nguồn
- SKU
- ngày sản xuất
- hạn dùng
- trạng thái QC / release

**2.4. Đơn vị đóng gói cấp 1**

Là đơn vị nhỏ trực tiếp chứa sản phẩm, ví dụ:

- gói
- lọ
- hũ

**2.5. Đơn vị đóng gói cấp 2**

Là đơn vị truy vết chính, ví dụ:

- hộp
- thùng
- hoặc lọ / hũ nếu một SKU không có lớp đóng gói ngoài

**2.6. Barcode**

Là mã vạch dùng cho:

- kho
- bán hàng
- kiểm soát đóng gói
- đối soát đơn vị sản phẩm

**2.7. QR**

Là mã QR dùng cho:

- public trace
- internal trace
- recall linkage
- xác thực lô / batch nếu cần

**3\. CHUỖI DỮ LIỆU TRUY VẾT BẮT BUỘC**

**3.1. Từ nguyên liệu vào sản xuất**

Mỗi lot nguyên liệu phải truy được tới:

- phiếu nhập nguyên liệu đầu vào
- phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất
- lệnh sản xuất / mẻ sản xuất đã dùng lot đó

**Quy tắc**

Một nguyên liệu không chỉ "có trong kho", mà phải biết:

- lô nào
- đã được duyệt vào mẻ nào

**3.2. Từ mẻ sản xuất sang batch thành phẩm**

Mỗi mẻ sản xuất phải sinh ra hoặc gắn với:

- một hoặc nhiều batch thành phẩm

**Quy tắc**

Mỗi batch thành phẩm phải truy ngược được về:

- mẻ sản xuất nào
- lệnh sản xuất nào
- công thức version nào
- các lô nguyên liệu nào đã đi vào

**3.3. Từ batch sang đơn vị đóng gói cấp 1**

Mỗi đơn vị cấp 1 phải biết:

- thuộc batch nào

**Quy tắc**

Nếu không truy từng đơn vị cấp 1 chi tiết, ít nhất vẫn phải map được:

- số lượng cấp 1 sinh ra từ batch đó

**3.4. Từ cấp 1 sang cấp 2**

Mỗi hộp / thùng phải biết:

- chứa đơn vị cấp 1 nào
- hoặc ít nhất chứa số lượng cấp 1 thuộc batch nào

**Quy tắc**

Cấp 2 là đơn vị truy vết chính, nên bắt buộc phải gắn được với batch.

**3.5. Từ cấp 2 sang barcode / QR**

Mỗi đơn vị cấp 2 phải có:

- barcode
- QR
- ngày sản xuất
- hạn dùng
- batch / lô sản xuất

**Quy tắc**

Không được có barcode / QR "trôi nổi" không map được về batch.

**4\. BẢN ĐỒ LIÊN KẾT BẮT BUỘC**

**4.1. Lot nguyên liệu → Mẻ sản xuất**

| **Trường**                              | **Bắt buộc** |
| --------------------------------------- | ------------ |
| lot_material_code                       | Có           |
| raw_material_code                       | Có           |
| supplier_id                             | Có           |
| production_order_id                     | Có           |
| production_batch_id / production_run_id | Có           |

**4.2. Mẻ sản xuất → Batch thành phẩm**

| **Trường**                   | **Bắt buộc** |
| ---------------------------- | ------------ |
| production_batch_id / run_id | Có           |
| finished_batch_code          | Có           |
| sku_code                     | Có           |
| formula_code                 | Có           |
| formula_version              | Có           |

**4.3. Batch thành phẩm → Đóng gói cấp 1**

| **Trường**             | **Bắt buộc** |
| ---------------------- | ------------ |
| finished_batch_code    | Có           |
| pack_level_1_unit_type | Có           |
| pack_level_1_qty       | Có           |

**4.4. Batch thành phẩm / cấp 1 → Đóng gói cấp 2**

| **Trường**                  | **Bắt buộc** |
| --------------------------- | ------------ |
| finished_batch_code         | Có           |
| pack_level_2_unit_type      | Có           |
| box_code / case_code nếu có | Nên có       |
| pack_level_2_qty            | Có           |

**4.5. Cấp 2 → Barcode / QR**

| **Trường**         | **Bắt buộc** |
| ------------------ | ------------ |
| package_level_2_id | Có           |
| barcode_value      | Có           |
| qr_token           | Có           |
| mfg_date           | Có           |
| exp_date           | Có           |
| batch_code         | Có           |

**5\. QUY TẮC IN MÃ VÀ SINH MÃ TRUY VẾT**

**5.1. Đóng gói cấp 1**

Cấp 1 chỉ in:

- ngày sản xuất
- hạn dùng

**Quy tắc**

Cấp 1 không phải cấp truy vết chính cho khách hàng.

**5.2. Đóng gói cấp 2**

Cấp 2 phải in:

- lô sản xuất / batch
- ngày sản xuất
- hạn dùng
- barcode
- QR

**Quy tắc**

Đây là cấp truy vết chính.

**5.3. Sinh mã thời gian thực**

Khi line đóng gói cấp 2 đang chạy, hệ thống phải tự sinh theo thời gian thực:

- batch / lot
- MFG
- EXP
- barcode
- QR

**Cấm**

- không cho máy in tự sinh mã nghiệp vụ
- không cho người vận hành nhập tay mã lô / QR / barcode trong luồng chuẩn

**6\. INTERNAL TRACE VÀ PUBLIC TRACE**

**6.1. Internal trace**

Internal trace phải tra được:

- đơn vị cấp 2
- batch thành phẩm
- mẻ sản xuất
- version công thức
- các lot nguyên liệu đầu vào
- nhà cung cấp nếu cần
- nhân sự / công đoạn nếu cần

**6.2. Public trace**

Public trace chỉ nên hiển thị:

- SKU / tên sản phẩm
- batch / lô
- ngày sản xuất
- hạn dùng
- trạng thái xác thực
- thông tin minh bạch được phép công bố

**Cấm**

Không đưa lên public trace:

- giá
- nhân sự nội bộ
- chi tiết hao hụt
- nhà cung cấp nhạy cảm
- dữ liệu kế toán

**7\. QUY TẮC THU HỒI (RECALL)**

**7.1. Recall phải dùng chính chuỗi trace này**

Không được dựng chuỗi thu hồi riêng.

Recall phải lần ngược được:

- QR / barcode
- hộp / thùng
- batch thành phẩm
- mẻ sản xuất
- lot nguyên liệu

**7.2. Cấp khóa recall tối thiểu**

Phải khóa được ít nhất:

- theo batch
- theo hộp
- theo thùng

**7.3. Điều kiện recall**

Khi có recall, hệ thống phải xác định được:

- những đơn vị cấp 2 nào bị ảnh hưởng
- batch nào bị ảnh hưởng
- mẻ nào bị ảnh hưởng
- nguyên liệu / lot nào liên quan

**8\. QUY TẮC KHO VÀ NHẬP KHO THÀNH PHẨM**

**8.1. Nhập kho thành phẩm chỉ sau QC đạt**

Đơn vị nhập kho thành phẩm phải gắn được với:

- batch
- đơn vị cấp 2
- barcode / QR nếu đã sinh

**8.2. Đơn vị kho chính**

Đơn vị kho thành phẩm nên theo:

- hộp
- thùng
- lọ
- hũ

tùy loại sản phẩm

**8.3. Quy tắc quét mã nhập kho**

Khi nhập kho thành phẩm, hệ thống phải có khả năng:

- quét barcode / QR
- xác định batch tương ứng
- ghi nhận vị trí kho

**9\. KHỐI DỮ LIỆU BẮT BUỘC CHO DEV**

**9.1. Lot Material Registry**

Mỗi lot nguyên liệu phải có:

- lot_id
- raw_material_code
- supplier_id
- received_at
- qc_status
- warehouse_location

**9.2. Production Run / Batch Registry**

Mỗi mẻ / batch phải có:

- production_run_id
- production_order_id
- sku_code
- formula_code
- formula_version
- finished_batch_code
- mfg_date
- exp_date

**9.3. Packaging Registry**

Mỗi đơn vị cấp 2 phải có:

- package_level_2_id
- finished_batch_code
- packaging_type
- barcode_value
- qr_token
- mfg_date
- exp_date

**9.4. Trace Registry**

Phải có registry để map:

- qr_token
- barcode_value
- package_level_2_id
- finished_batch_code
- production_run_id
- lot_material_ids liên quan

**10\. QUY TẮC CỨNG**

**10.1. Không có batch thì không có QR cấp 2**

**10.2. Không có lot nguyên liệu thì batch không được coi là trace-complete**

**10.3. Không có map cấp 2 → batch thì không được public trace**

**10.4. Không có trace chain hoàn chỉnh thì không được coi là recall-ready**

**10.5. Không cho barcode / QR tồn tại mà không truy ngược được về batch**

**11\. PASS / FAIL GATE**

**11.1. Đạt khi**

- truy được từ lot nguyên liệu → mẻ → batch → cấp 2 → QR
- truy được từ QR → cấp 2 → batch → mẻ → lot nguyên liệu
- public trace và internal trace tách rõ
- recall dùng được chính chuỗi này
- nhập kho thành phẩm gắn được batch và đơn vị cấp 2

**11.2. Chưa đạt khi**

- lot nguyên liệu không nối được với mẻ
- batch không nối được với cấp đóng gói
- QR không nối được với batch
- recall phải dựng chuỗi riêng
- public trace lẫn dữ liệu nội bộ

**12\. KẾT LUẬN**

Từ đây, **TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP - GINSENGFOOD** được khóa ở mức:

- có lot nguyên liệu
- có mẻ sản xuất
- có batch thành phẩm
- có map sang cấp đóng gói
- có barcode / QR cấp 2
- có internal trace
- có public trace
- recall reuse chính chuỗi này

Đây là nền để đi tiếp sang:

- **BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH**

bản **"BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH - GINSENGFOOD"** theo format sạch, Word-ready.

Bản này bám đúng các nguyên tắc đã khóa:

- danh mục nguyên liệu đầu vào quản lý theo 2 nhóm: **Sâm và dược liệu / Nguyên liệu**
- lệnh sản xuất và phiếu cấp nguyên liệu cho sản xuất phải **tự hiện công thức**, không cho chọn lại nguyên liệu tay
- phiếu nhập nguyên liệu đầu vào là nơi **click chọn** nguyên liệu từ danh mục
- đơn vị theo công đoạn đã chốt:
  - nhập / cấp phát: kg / lít / ml
  - sơ chế: khay / kg / lít
  - sấy: khay + % độ ẩm
  - đóng gói cấp 1: gói / lọ / hũ
  - đóng gói cấp 2: hộp / thùng / lọ / hũ
- đóng gói cấp 2 là nơi hệ thống **tự sinh dữ liệu in theo thời gian thực**
- MISA dùng một ngõ tích hợp chung cho toàn hệ, không điều khiển logic nhà máy hay logic KPI gốc

# BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH - GINSENGFOOD

## 0\. MỞ ĐẦU TÀI LIỆU

### 0.1. Mục đích tài liệu

Tài liệu này dùng để khóa lớp đặc tả thực thi cho dev và QA, gồm:

- màn hình nào phải có
- route nào phải có
- action nào phải có
- trường nào tự hiện / click chọn / nhập tay
- điều kiện chuyển bước
- trạng thái của từng phiếu
- quyền thao tác của từng vai trò

### 0.2. Vai trò tài liệu

Tài liệu này là nguồn để:

- **dev backend** dựng route / controller / service / trạng thái
- **dev frontend** dựng form / bảng / nút / modal / khối xác nhận
- **QA** test đúng luồng
- **vận hành** hiểu form sẽ dùng như thế nào

### 0.3. Ranh giới tài liệu

Tài liệu này không thay:

- bộ phiếu mẫu vận hành
- BOM / Recipe Map
- Operational Config Map
- Trace Data Chain
- MISA Integration Rule

Tài liệu này chỉ đặc tả:

- **màn hình**
- **route**
- **action**
- **trạng thái**
- **quyền thao tác**

# 1\. QUY TẮC CHUNG CHO TẤT CẢ MÀN HÌNH

## 1.1. Phân loại trường dữ liệu

Mỗi màn hình phải chia trường thành 3 loại rõ ràng:

### 1.1.1. Trường tự hiện

Hệ thống tự sinh / tự kéo, không cho gõ tay.

### 1.1.2. Trường click chọn

Chọn từ danh mục master, không gõ tự do.

### 1.1.3. Trường phải nhập

Dùng để nhập số liệu thực tế, đánh giá, ghi chú.

## 1.2. Quy tắc kế thừa dữ liệu

Mọi phiếu sau phải tự kế thừa dữ liệu từ:

- hồ sơ sản xuất gốc
- phiếu trước đó trong chuỗi

Không cho người dùng lập lại từ đầu.

## 1.3. Quy tắc action

Mỗi action phải có đủ:

- người được bấm
- điều kiện được bấm
- trạng thái trước khi bấm
- trạng thái sau khi bấm
- log người bấm
- thời gian bấm

## 1.4. Quy tắc xác nhận

Mọi phiếu phải có khối xác nhận theo từng cá nhân:

- Vai trò
- Họ tên
- Bộ phận
- Trạng thái xác nhận
- Thời gian xác nhận
- Ghi chú

## 1.5. Quy tắc trạng thái

Mỗi phiếu phải có tối thiểu bộ trạng thái:

- DRAFT
- SUBMITTED
- APPROVED / REJECTED
- COMPLETED
- CANCELED

Có thể tinh chỉnh theo từng phiếu.

# 2\. MÀN HÌNH 01 - PHIẾU NHẬP NGUYÊN LIỆU ĐẦU VÀO

## 2.1. Route màn hình

- GET /admin/operational/raw-material-inbound
- GET /admin/operational/raw-material-inbound/{id}
- GET /admin/operational/raw-material-inbound/create

## 2.2. Route API / action

- POST /api/admin/operational/raw-material-inbound
- POST /api/admin/operational/raw-material-inbound/{id}/submit
- POST /api/admin/operational/raw-material-inbound/{id}/approve
- POST /api/admin/operational/raw-material-inbound/{id}/reject
- POST /api/admin/operational/raw-material-inbound/{id}/post-accounting
- POST /api/admin/operational/raw-material-inbound/{id}/attach-evidence

## 2.3. Trường tự hiện

- Mã phiếu
- Ngày lập mặc định
- Người lập mặc định
- Bộ phận tiếp nhận mặc định nếu policy có

## 2.4. Trường click chọn

- Nhà cung cấp
- Nguyên liệu
- Nhóm nguyên liệu
- Người giao hàng nếu có danh mục
- Kho nhập
- Vị trí kho
- Nhân sự QC
- Kế toán kho / vật tư

## 2.5. Trường nhập tay

- Số lượng giao
- Quy cách
- Tình trạng ban đầu
- Kết quả đánh giá
- Lý do không đạt
- Đề xuất xử lý
- Đơn giá nhập
- Ghi chú

## 2.6. Bảng chi tiết

Cột bắt buộc:

| **Cột**            |
| ------------------ |
| Mã nguyên liệu     |
| Tên nguyên liệu    |
| Nhóm nguyên liệu   |
| Chủng loại / mô tả |
| Số lượng giao      |
| Đơn vị             |
| Quy cách           |
| Tình trạng ban đầu |
| Kết quả đánh giá   |
| Lý do không đạt    |
| Đề xuất xử lý      |
| Đơn giá nhập       |
| Thành tiền         |
| Ghi chú            |

## 2.7. Action chính

- Lưu nháp
- Gửi duyệt
- Duyệt nhập kho
- Từ chối
- Ghi nhận kế toán
- Đính ảnh / video

## 2.8. Trạng thái

- DRAFT
- SUBMITTED
- PARTIALLY_APPROVED
- APPROVED
- REJECTED
- ACCOUNTED

## 2.9. Vai trò được thao tác

- Kho
- QC đầu vào
- Kế toán kho / vật tư
- Quản lý liên quan

# 3\. MÀN HÌNH 02 - LỆNH SẢN XUẤT

## 3.1. Route màn hình

- GET /admin/operational/production-orders
- GET /admin/operational/production-orders/{id}
- GET /admin/operational/production-orders/create

## 3.2. Route API / action

- POST /api/admin/operational/production-orders
- POST /api/admin/operational/production-orders/{id}/open
- POST /api/admin/operational/production-orders/{id}/cancel
- POST /api/admin/operational/production-orders/{id}/close

## 3.3. Trường tự hiện

- Mã hồ sơ sản xuất gốc
- Mã lệnh sản xuất
- Tên sản phẩm
- Mã công thức
- Version công thức
- Nhóm sản phẩm
- Phân loại Vegan / Mặn
- Danh sách nguyên liệu theo công thức
- Hàm lượng
- Đơn vị

## 3.4. Trường click chọn

- SKU
- Ca sản xuất
- Bộ phận sản xuất
- Quản lý nhà máy
- Loại lệnh:
  - Pilot
  - Sản xuất thường

## 3.5. Trường nhập tay

- Số lượng kế hoạch
- Ghi chú lệnh

## 3.6. Bảng công thức tự hiện

### Phần 1 - Dược liệu

### Phần 2 - Nguyên liệu

**Cấm chọn tay nguyên liệu.**

## 3.7. Action chính

- Lưu nháp
- Mở lệnh
- Hủy lệnh
- Đóng lệnh

## 3.8. Trạng thái

- DRAFT
- OPEN
- CANCELED
- CLOSED

## 3.9. Vai trò được thao tác

- Kế hoạch sản xuất
- Quản lý nhà máy
- Bộ phận sản xuất

# 4\. MÀN HÌNH 03 - PHIẾU ĐỀ NGHỊ CẤP NGUYÊN LIỆU CHO SẢN XUẤT

## 4.1. Route màn hình

- GET /admin/operational/material-issue-requests
- GET /admin/operational/material-issue-requests/{id}

## 4.2. Route API / action

- POST /api/admin/operational/material-issue-requests/generate-from-order
- POST /api/admin/operational/material-issue-requests/{id}/submit
- POST /api/admin/operational/material-issue-requests/{id}/approve
- POST /api/admin/operational/material-issue-requests/{id}/reject

## 4.3. Trường tự hiện

- Hồ sơ sản xuất
- Lệnh sản xuất
- SKU
- Tên sản phẩm
- Công thức
- Version
- Danh sách nguyên liệu
- Hàm lượng
- Đơn vị
- Giá trị tham chiếu kế toán

## 4.4. Trường click chọn

- Kho xuất
- Người đề nghị
- Quản lý nhà máy

## 4.5. Trường nhập tay

- Ghi chú

## 4.6. Bảng chi tiết

| **Cột**                     |
| --------------------------- |
| Mã nguyên liệu              |
| Tên nguyên liệu             |
| Nhóm nguyên liệu            |
| Hàm lượng công thức         |
| Đơn vị                      |
| Số lượng yêu cầu cấp        |
| Đơn giá bình quân hiện hành |
| Giá trị dự kiến xuất        |
| Ghi chú                     |

## 4.7. Action chính

- Tự sinh từ lệnh sản xuất
- Gửi duyệt
- Duyệt
- Từ chối

## 4.8. Trạng thái

- GENERATED
- SUBMITTED
- APPROVED
- REJECTED

## 4.9. Vai trò được thao tác

- Bộ phận sản xuất
- Kho
- Quản lý nhà máy

# 5\. MÀN HÌNH 04 - PHIẾU CHẤP THUẬN NGUYÊN LIỆU ĐƯỢC PHÉP ĐƯA VÀO SẢN XUẤT

## 5.1. Route màn hình

- GET /admin/operational/material-issue-approvals
- GET /admin/operational/material-issue-approvals/{id}

## 5.2. Route API / action

- POST /api/admin/operational/material-issue-approvals/generate-from-request
- POST /api/admin/operational/material-issue-approvals/{id}/approve
- POST /api/admin/operational/material-issue-approvals/{id}/reject
- POST /api/admin/operational/material-issue-approvals/{id}/post-accounting

## 5.3. Trường tự hiện

- Hồ sơ sản xuất
- Lệnh sản xuất
- SKU
- Tên sản phẩm
- Công thức
- Version
- Danh sách nguyên liệu đề nghị cấp
- Giá trị hạch toán dự kiến

## 5.4. Trường click chọn

- Lô nguyên liệu cấp cho từng dòng
- Người phê duyệt theo vai trò

## 5.5. Trường nhập tay

- Số lượng duyệt thực tế nếu có chênh lệch
- Ghi chú
- Lý do từ chối

## 5.6. Bảng chi tiết

| **Cột**             |
| ------------------- |
| Mã nguyên liệu      |
| Tên nguyên liệu     |
| Lô nguyên liệu      |
| Số lượng duyệt xuất |
| Đơn vị              |
| Kết quả chấp thuận  |
| Đơn giá bình quân   |
| Thành tiền xuất     |
| Hao hụt cấp phát    |
| Giá trị hao hụt     |
| Ghi chú             |

## 5.7. Action chính

- Duyệt
- Từ chối
- Ghi nhận kế toán

## 5.8. Trạng thái

- GENERATED
- APPROVED
- REJECTED
- ACCOUNTED

## 5.9. Vai trò được thao tác

- Quản lý nhà máy
- Kho
- QC
- Kế toán vật tư / kho

# 6\. MÀN HÌNH 05 - CHECK-IN / CHECK-OUT NHÂN SỰ THEO LỆNH / MẺ / CÔNG ĐOẠN

## 6.1. Route màn hình

- GET /admin/operational/workforce-checkin
- GET /admin/operational/workforce-checkin/{id}

## 6.2. Route API / action

- POST /api/admin/operational/workforce-checkin
- POST /api/admin/operational/workforce-checkin/bulk-checkin
- POST /api/admin/operational/workforce-checkin/bulk-checkout
- POST /api/admin/operational/workforce-checkin/{id}/confirm

## 6.3. Trường tự hiện

- Lệnh sản xuất
- Hồ sơ sản xuất
- Mẻ sản xuất
- SKU
- Tên sản phẩm
- Công đoạn

## 6.4. Trường click chọn

- Nhân sự
- Bộ phận
- Chức danh
- Công đoạn
- Ca sản xuất

## 6.5. Trường nhập tay

- Ghi chú
- đơn giá công nếu chưa tự kéo được

## 6.6. Bảng chi tiết

| **Cột**                   |
| ------------------------- |
| Mã nhân sự                |
| Họ tên                    |
| Bộ phận                   |
| Chức danh                 |
| Đơn giá công / giờ        |
| Check-in                  |
| Check-out                 |
| Tổng giờ làm              |
| Chi phí nhân công phân bổ |
| Ghi chú                   |

## 6.7. Action chính

- Check-in
- Check-out
- Check-in nhiều người
- Check-out nhiều người
- Xác nhận hoàn tất công đoạn

## 6.8. Trạng thái

- OPEN
- IN_PROGRESS
- CHECKED_OUT
- CONFIRMED

## 6.9. Vai trò được thao tác

- Tổ trưởng / phụ trách công đoạn
- Quản lý bộ phận

# 7\. MÀN HÌNH 06 - PHIẾU THEO DÕI SƠ CHẾ

## 7.1. Route màn hình

- GET /admin/operational/preprocess-tracking
- GET /admin/operational/preprocess-tracking/{id}

## 7.2. Route API / action

- POST /api/admin/operational/preprocess-tracking/generate
- POST /api/admin/operational/preprocess-tracking/{id}/submit
- POST /api/admin/operational/preprocess-tracking/{id}/approve
- POST /api/admin/operational/preprocess-tracking/{id}/attach-evidence

## 7.3. Trường tự hiện

- Hồ sơ sản xuất
- Lệnh sản xuất
- SKU
- Tên sản phẩm
- Mẻ sản xuất
- Danh sách nguyên liệu đã được duyệt vào sản xuất

## 7.4. Trường click chọn

- Người thực hiện
- Tổ trưởng / phụ trách công đoạn
- Quản lý bộ phận

## 7.5. Trường nhập tay

- Số lượng đầu vào
- Số lượng sau sơ chế
- Hao hụt
- Đánh giá
- Ghi chú

## 7.6. Bảng chi tiết

| **Cột**                         |
| ------------------------------- |
| Loại bán thành phẩm / lô sơ chế |
| Số lượng đầu vào                |
| Đơn vị                          |
| Số lượng sau sơ chế             |
| Đơn vị                          |
| Hao hụt                         |
| Đơn vị                          |
| Đánh giá                        |
| Ghi chú                         |

**Đơn vị được phép:** khay / kg / lít

## 7.7. Action chính

- Lưu nháp
- Gửi duyệt
- Duyệt
- Đính bằng chứng

## 7.8. Trạng thái

- GENERATED
- DRAFT
- SUBMITTED
- APPROVED
- REJECTED

## 7.9. Vai trò được thao tác

- Bộ phận sơ chế
- Tổ trưởng
- Quản lý bộ phận

# 8\. MÀN HÌNH 07 - PHIẾU KIỂM CHẤT LƯỢNG SAU SẤY THĂNG HOA

## 8.1. Route màn hình

- GET /admin/operational/freeze-dry-qc
- GET /admin/operational/freeze-dry-qc/{id}

## 8.2. Route API / action

- POST /api/admin/operational/freeze-dry-qc/generate
- POST /api/admin/operational/freeze-dry-qc/{id}/submit
- POST /api/admin/operational/freeze-dry-qc/{id}/approve
- POST /api/admin/operational/freeze-dry-qc/{id}/reject

## 8.3. Trường tự hiện

- Hồ sơ sản xuất
- Lệnh sản xuất
- SKU
- Tên sản phẩm
- Công thức
- Version
- Mẻ sản xuất

## 8.4. Trường click chọn

- QC
- Quản lý nhà máy
- Bộ phận phụ trách công đoạn

## 8.5. Trường nhập tay

- số lượng khay kiểm tra
- % độ ẩm
- đánh giá cảm quan
- ghi chú
- quyết định đạt / không đạt

## 8.6. Bảng chỉ tiêu

| **Cột**  |
| -------- |
| Chỉ tiêu |
| Kết quả  |
| Đơn vị   |
| Đánh giá |
| Ghi chú  |

**Bắt buộc có:** % độ ẩm

## 8.7. Action chính

- Lưu nháp
- Gửi duyệt
- Duyệt cho đóng gói
- Từ chối / chờ xử lý

## 8.8. Trạng thái

- GENERATED
- DRAFT
- SUBMITTED
- APPROVED_FOR_PACKING
- REJECTED
- ON_HOLD

## 8.9. Vai trò được thao tác

- QC
- Quản lý nhà máy
- Bộ phận phụ trách công đoạn

# 9\. MÀN HÌNH 08 - PHIẾU THEO DÕI ĐÓNG GÓI CẤP 1

## 9.1. Route màn hình

- GET /admin/operational/packing-level-1
- GET /admin/operational/packing-level-1/{id}

## 9.2. Route API / action

- POST /api/admin/operational/packing-level-1/generate
- POST /api/admin/operational/packing-level-1/{id}/start
- POST /api/admin/operational/packing-level-1/{id}/complete
- POST /api/admin/operational/packing-level-1/{id}/attach-evidence

## 9.3. Trường tự hiện

- Hồ sơ sản xuất
- Lệnh sản xuất
- SKU
- Tên sản phẩm
- Mẻ sản xuất
- Đơn vị cấp 1
- rule in cấp 1

## 9.4. Trường click chọn

- Người thực hiện
- Tổ trưởng
- Quản lý bộ phận
- Máy in cấp 1 nếu có

## 9.5. Trường nhập tay

- số lượng đầu vào
- số lượng đạt
- số lượng không đạt
- lý do không đạt
- ghi chú

## 9.6. Bảng chi tiết

| **Cột**            |
| ------------------ |
| Số lượng đầu vào   |
| Đơn vị             |
| Số lượng đạt       |
| Đơn vị             |
| Số lượng không đạt |
| Đơn vị             |
| Lý do không đạt    |
| Ghi chú            |

**Đơn vị được phép:** gói / lọ / hũ

## 9.7. Action chính

- Bắt đầu đóng gói
- Hoàn tất đóng gói
- Đính ảnh / video

## 9.8. Trạng thái

- GENERATED
- READY
- IN_PROGRESS
- COMPLETED
- ON_HOLD

## 9.9. Vai trò được thao tác

- Bộ phận đóng gói cấp 1
- Tổ trưởng
- Quản lý bộ phận

# 10\. MÀN HÌNH 09 - PHIẾU THEO DÕI ĐÓNG GÓI CẤP 2

## 10.1. Route màn hình

- GET /admin/operational/packing-level-2
- GET /admin/operational/packing-level-2/{id}

## 10.2. Route API / action

- POST /api/admin/operational/packing-level-2/generate
- POST /api/admin/operational/packing-level-2/{id}/start
- POST /api/admin/operational/packing-level-2/{id}/print-start
- POST /api/admin/operational/packing-level-2/{id}/print-reprint
- POST /api/admin/operational/packing-level-2/{id}/complete

## 10.3. Trường tự hiện

- Hồ sơ sản xuất
- Lệnh sản xuất
- SKU
- Tên sản phẩm
- Mẻ sản xuất
- Đơn vị cấp 2
- batch / lô
- nội dung in cấp 2
- barcode / QR rule

## 10.4. Trường click chọn

- Người thực hiện
- Tổ trưởng
- Quản lý bộ phận
- Máy in cấp 2

## 10.5. Trường nhập tay

- số lượng đầu vào
- số lượng đầu ra
- số lượng không đạt
- lý do không đạt
- ghi chú

## 10.6. Bảng chi tiết

| **Cột**            |
| ------------------ |
| Số lượng đầu vào   |
| Đơn vị             |
| Số lượng đầu ra    |
| Đơn vị             |
| Số lượng không đạt |
| Đơn vị             |
| Lý do không đạt    |
| Ghi chú            |

**Đơn vị được phép:** hộp / thùng / lọ / hũ

## 10.7. Action chính

- Bắt đầu đóng gói cấp 2
- Bắt đầu in cấp 2
- Reprint có kiểm soát
- Hoàn tất đóng gói cấp 2

## 10.8. Trạng thái

- GENERATED
- READY
- IN_PROGRESS
- PRINTING
- COMPLETED
- PRINT_ERROR
- ON_HOLD

## 10.9. Vai trò được thao tác

- Bộ phận đóng gói cấp 2
- Tổ trưởng
- Quản lý bộ phận
- người được quyền reprint

# 11\. MÀN HÌNH 10 - PHIẾU QC THÀNH PHẨM

## 11.1. Route màn hình

- GET /admin/operational/finished-goods-qc
- GET /admin/operational/finished-goods-qc/{id}

## 11.2. Route API / action

- POST /api/admin/operational/finished-goods-qc/generate
- POST /api/admin/operational/finished-goods-qc/{id}/submit
- POST /api/admin/operational/finished-goods-qc/{id}/approve
- POST /api/admin/operational/finished-goods-qc/{id}/reject

## 11.3. Trường tự hiện

- Hồ sơ sản xuất
- Lệnh sản xuất
- SKU
- Tên sản phẩm
- Mẻ sản xuất
- rule QC thành phẩm
- đơn vị đóng gói liên quan

## 11.4. Trường click chọn

- QC
- Quản lý nhà máy
- Bộ phận liên quan

## 11.5. Trường nhập tay

- số lượng kiểm tra
- số lượng đạt
- số lượng không đạt
- tỷ lệ đạt
- ghi chú

## 11.6. Bảng chi tiết

| **Cột**            |
| ------------------ |
| Số lượng kiểm tra  |
| Đơn vị             |
| Số lượng đạt       |
| Đơn vị             |
| Số lượng không đạt |
| Đơn vị             |
| Tỷ lệ đạt          |
| Ghi chú            |

**Đơn vị được phép:** gói / lọ / hũ / hộp / thùng

## 11.7. Action chính

- Lưu nháp
- Gửi duyệt
- Duyệt nhập kho
- Từ chối / chờ xử lý

## 11.8. Trạng thái

- GENERATED
- DRAFT
- SUBMITTED
- APPROVED_FOR_WAREHOUSE
- REJECTED
- ON_HOLD

## 11.9. Vai trò được thao tác

- QC
- Quản lý nhà máy
- Bộ phận liên quan

# 12\. MÀN HÌNH 11 - LỆNH NHẬP KHO THÀNH PHẨM

## 12.1. Route màn hình

- GET /admin/operational/finished-goods-receipt
- GET /admin/operational/finished-goods-receipt/{id}

## 12.2. Route API / action

- POST /api/admin/operational/finished-goods-receipt/generate
- POST /api/admin/operational/finished-goods-receipt/{id}/scan
- POST /api/admin/operational/finished-goods-receipt/{id}/confirm
- POST /api/admin/operational/finished-goods-receipt/{id}/post-accounting

## 12.3. Trường tự hiện

- Hồ sơ sản xuất
- Lệnh sản xuất
- SKU
- Tên sản phẩm
- số lượng được phép nhập kho từ QC
- batch / lô
- barcode / QR nếu có
- kho nhập gợi ý nếu policy có

## 12.4. Trường click chọn

- Kho nhập
- Vị trí kho
- Người nhập
- QC
- Quản lý nhà máy

## 12.5. Trường nhập tay

- số lượng nhập thực tế
- ghi chú

## 12.6. Bảng chi tiết

| **Cột**       |
| ------------- |
| Số lượng nhập |
| Đơn vị        |
| Ghi chú       |

**Đơn vị được phép:** hộp / thùng / lọ / hũ

## 12.7. Action chính

- Quét mã nhập kho
- Xác nhận nhập kho
- Ghi nhận kế toán nếu có

## 12.8. Trạng thái

- GENERATED
- READY_TO_RECEIVE
- SCANNED
- RECEIVED
- ACCOUNTED

## 12.9. Vai trò được thao tác

- Kho
- QC
- Quản lý nhà máy
- Kế toán kho nếu có ngõ kế toán

# 13\. MÀN HÌNH 12 - PHIẾU KẾ TOÁN XUẤT NGUYÊN LIỆU CHO SẢN XUẤT

## 13.1. Route màn hình

- GET /admin/operational/material-issue-accounting
- GET /admin/operational/material-issue-accounting/{id}

## 13.2. Route API / action

- POST /api/admin/operational/material-issue-accounting/generate
- POST /api/admin/operational/material-issue-accounting/{id}/post
- POST /api/admin/operational/material-issue-accounting/{id}/sync-misa

## 13.3. Trường tự hiện

- Hồ sơ sản xuất
- Lệnh sản xuất
- SKU
- Tên sản phẩm
- mẻ sản xuất
- danh sách nguyên liệu đã duyệt cấp phát
- đơn giá bình quân
- thành tiền xuất
- hao hụt

## 13.4. Trường click chọn

- Kế toán vật tư / kho
- Kho
- Quản lý nhà máy

## 13.5. Trường nhập tay

- ghi chú kế toán
- điều chỉnh được phép theo policy nếu có

## 13.6. Bảng chi tiết

| **Cột**           |
| ----------------- |
| Mã nguyên liệu    |
| Tên nguyên liệu   |
| Lô nguyên liệu    |
| Số lượng xuất     |
| Đơn vị            |
| Đơn giá bình quân |
| Thành tiền xuất   |
| Hao hụt           |
| Giá trị hao hụt   |
| Ghi chú           |

**Đơn vị được phép:** kg / lít / ml

## 13.7. Action chính

- Ghi nhận phiếu kế toán
- Sync MISA
- Retry sync
- Xem trạng thái sync

## 13.8. Trạng thái

- GENERATED
- POSTED
- SYNC_PENDING
- SYNC_SUCCESS
- SYNC_ERROR

## 13.9. Vai trò được thao tác

- Kế toán vật tư / kho
- Kho
- Quản lý nhà máy
- người có quyền retry sync

# 14\. QUY TẮC CỨNG CHO DEV

## 14.1. Phiếu nhập nguyên liệu = click chọn nguyên liệu từ danh mục

## 14.2. Lệnh sản xuất / cấp phát = tự hiện công thức, không cho chọn lại nguyên liệu

## 14.3. Mọi phiếu sau = tự kế thừa dữ liệu từ hồ sơ gốc

## 14.4. Mọi phiếu phải có khối xác nhận cá nhân

## 14.5. Mọi phiếu phải đúng đơn vị theo công đoạn

## 14.6. Đóng gói cấp 2 phải có action in / reprint gắn với dữ liệu in thời gian thực

## 14.7. Phiếu kế toán xuất nguyên liệu là màn riêng cho kế toán / sync MISA, không đưa logic giá ra form vận hành xưởng

# 15\. PASS / FAIL GATE

## 15.1. Đạt khi

- đủ 12 màn hình / route / action lõi
- mỗi phiếu có trường tự hiện / click chọn / nhập tay rõ ràng
- lệnh sản xuất kéo đúng công thức
- phiếu cấp phát không cho chọn tay nguyên liệu
- đơn vị theo công đoạn đúng
- action in cấp 2 rõ
- action sync MISA rõ
- QA test được end-to-end

## 15.2. Chưa đạt khi

- dev phải tự đoán form
- phiếu không rõ trường nào auto / chọn / nhập
- nguyên liệu sản xuất vẫn cho chọn tay
- route / action không đủ
- trạng thái không rõ
- role thao tác không rõ

## 16\. KẾT LUẬN

Từ đây, **BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH - GINSENGFOOD** đã đủ để làm nền kỹ thuật giao dev:

- rõ màn hình
- rõ route
- rõ action
- rõ trường
- rõ trạng thái
- rõ role thao tác

Đến đây, 5 bước bạn chốt đã đi đủ:

- **20 SKU ↔ NGUYÊN LIỆU / BOM MAP - GINSENGFOOD**
- **20 SKU ↔ OPERATIONAL CONFIG MAP - GINSENGFOOD**
- **QUY TẮC VERSION CÔNG THỨC / PHÊ DUYỆT THAY ĐỔI CÔNG THỨC**
- **TRACE DATA CHAIN / BATCH-LOT-QR-BOX-CASE MAP**
- **BỘ ĐẶC TẢ MÀN HÌNH / ROUTE / ACTION CHO TOÀN BỘ PHIẾU VẬN HÀNH**

---

## PHỤ LỤC NGUỒN ĐÃ NHẬP - PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN.md

> Phụ lục này giữ nguyên văn nguồn cũ để không mất bảng, số liệu, field hoặc rule khi bỏ file nguồn riêng. Phần rewrite chuẩn hóa ở đầu file là nguồn chính; phụ lục dùng để trace và đối chiếu số liệu.

# 1\. QUY TẮC TOÀN BỘ PHIẾU TỰ SINH - GINSENGFOOD

**0\. MỞ ĐẦU**

**0.1. Mục đích**

Tài liệu này khóa nguyên tắc:

- tạo hồ sơ sản xuất gốc
- tự sinh chuỗi phiếu vận hành
- tự kế thừa dữ liệu giữa các phiếu
- phân ranh rõ nơi nào được click chọn, nơi nào bắt buộc tự hiện
- chỉ cho người dùng đánh giá, ghi chú, nhập số liệu thực tế và xác nhận

**0.2. Phạm vi**

Áp dụng cho chuỗi:

- Phiếu kế hoạch sản xuất
- Phiếu đánh giá và nhập kho nguyên liệu đầu vào
- Lệnh sản xuất
- Phiếu đề nghị cấp nguyên liệu cho sản xuất
- Phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất
- Phiếu check-in / check-out nhân sự theo lệnh sản xuất / mẻ / công đoạn
- Phiếu theo dõi sơ chế
- Phiếu kiểm chất lượng sau sấy thăng hoa
- Phiếu theo dõi đóng gói cấp 1
- Phiếu theo dõi đóng gói cấp 2
- Phiếu QC thành phẩm
- Lệnh nhập kho thành phẩm
- Phiếu kế toán xuất nguyên liệu cho sản xuất

**1\. HỒ SƠ SẢN XUẤT GỐC**

**1.1. Nguyên tắc**

Ngay khi mở **Lệnh sản xuất**, hệ thống phải sinh:

- Mã hồ sơ sản xuất gốc
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Mã công thức
- Version công thức
- Ca sản xuất
- Bộ phận sản xuất
- Quản lý nhà máy
- Số lượng kế hoạch

**1.2. Vai trò**

Hồ sơ sản xuất gốc là nguồn dữ liệu duy nhất để toàn bộ phiếu sau tự kế thừa.

**1.3. Cấm**

Không cho từng công đoạn tự tạo phiếu rời không liên kết với hồ sơ sản xuất gốc.

**2\. PHÂN RANH CỨNG GIỮA "CLICK CHỌN" VÀ "TỰ HIỆN"**

**2.1. Ranh giới 1 - Phiếu nhập nguyên liệu đầu vào**

**2.1.1. Vai trò**

Đây là nơi tiếp nhận nguyên liệu đầu vào từ bên ngoài vào kho.

**2.1.2. Quy tắc cứng**

Tại phiếu này, **nguyên liệu được click chọn từ danh mục master**.

**2.1.3. Các trường được click chọn**

- Nhà cung cấp
- Nguyên liệu
- Nhóm nguyên liệu
- Đơn vị tính
- Người giao hàng
- Nhân sự tiếp nhận
- Kho nhập
- Vị trí kho

**2.1.4. Các trường phải nhập**

- Số lượng giao thực tế
- Quy cách
- Kết quả đánh giá
- Lý do không đạt
- Đề xuất xử lý
- Đơn giá nhập
- Ghi chú

**2.1.5. Kết luận**

**Phiếu nhập nguyên liệu đầu vào là nơi click chọn nguyên liệu từ danh mục master.**

**2.2. Ranh giới 2 - Lệnh sản xuất và phiếu cấp nguyên liệu cho sản xuất**

**2.2.1. Vai trò**

Đây là nơi thực thi công thức sản xuất.

**2.2.2. Quy tắc cứng**

Tại lệnh sản xuất và phiếu cấp nguyên liệu cho sản xuất, **người dùng không được tự chọn nguyên liệu**.

**2.2.3. Hệ thống phải tự hiện**

Khi chọn:

- SKU
- Mã công thức
- Version công thức

thì hệ thống phải tự hiện:

- Mã nguyên liệu
- Tên nguyên liệu
- Nhóm nguyên liệu
- Hàm lượng
- Đơn vị tính
- Số lượng cần cấp theo công thức

**2.2.4. Cấm**

Không cho người dùng ở bước sản xuất thường:

- tự thêm nguyên liệu
- tự xóa nguyên liệu
- tự đổi nguyên liệu
- tự thay đổi hàm lượng chuẩn trong phiếu cấp phát thường

**2.2.5. Ngoại lệ duy nhất**

Nếu thực tế cần thay đổi nguyên liệu hoặc hàm lượng, phải đi qua:

- chỉnh version công thức  
   hoặc
- phiếu thay đổi công thức / chấp thuận thay thế nguyên liệu riêng

**2.2.6. Kết luận**

**Lệnh sản xuất và phiếu cấp nguyên liệu cho sản xuất là nơi nguyên liệu phải tự hiện theo công thức, không được tự chọn.**

**2.3. Ranh giới tự hiện công thức sản xuất**

**2.3.1. Quy tắc cứng**

Khi chọn SKU, mã công thức và version công thức trong lệnh sản xuất, hệ thống phải tự động hiển thị đầy đủ danh sách:

- dược liệu
- nguyên liệu
- hàm lượng
- đơn vị tính

**2.3.2. Ý nghĩa vận hành**

Người vận hành không được nhìn công thức ở dạng nhóm mơ hồ.  
Hệ thống phải hiện đúng **từng nguyên liệu cụ thể** để:

- sinh phiếu cấp phát
- trừ kho
- theo dõi lô nguyên liệu
- tính giá trị xuất kho
- tính giá thành mẻ
- truy xuất và thu hồi

**2.3.3. Cấm**

Không được để:

- "nền chung"
- "nước hầm chung"
- "nêm chung"

ở dạng dòng gom nhóm trong lệnh sản xuất vận hành.

Tất cả phải bung thành:

- từng nguyên liệu
- từng hàm lượng
- từng đơn vị

**2.3.4. Kết luận**

**Công thức sản xuất phải tự hiện đầy đủ và chi tiết tới từng nguyên liệu; không cho chọn lại nguyên liệu ở bước sản xuất thường.**

**3\. DỮ LIỆU NÀO TỰ HIỆN, DỮ LIỆU NÀO CHỌN, DỮ LIỆU NÀO NHẬP**

**3.1. Tự sinh / tự hiện**

- Mã hồ sơ sản xuất gốc
- Mã lệnh sản xuất
- Tên sản phẩm
- Mã công thức
- Version công thức
- Nhóm sản phẩm
- Phân loại Vegan / Mặn
- Danh sách nguyên liệu của công thức
- Hàm lượng của công thức
- Đơn vị tính của công thức

**3.2. Click chọn từ danh mục**

- SKU
- Nhà cung cấp
- Nguyên liệu đầu vào
- Nhân sự
- Bộ phận
- Kho
- Vị trí kho
- Ca sản xuất
- Trạng thái đánh giá

**3.3. Phải nhập**

- Số lượng kế hoạch
- Số lượng giao thực tế
- Số lượng thực nhập
- Số lượng thực xuất
- Số liệu đo thực tế
- % độ ẩm
- Ghi chú
- Lý do không đạt
- Đề xuất hướng xử lý
- Đơn giá nhập
- thông tin kế toán cần nhập tay nếu chưa tự kéo được

**4\. NGUYÊN TẮC TỰ SINH PHIẾU**

**4.1. Từ phiếu kế hoạch sản xuất**

Cho phép sinh:

- Lệnh sản xuất

**4.2. Từ lệnh sản xuất**

Hệ thống tự sinh nháp:

- Phiếu đề nghị cấp nguyên liệu cho sản xuất
- Phiếu check-in / check-out nhân sự
- Phiếu theo dõi sơ chế
- Phiếu kiểm chất lượng sau sấy thăng hoa
- Phiếu đóng gói cấp 1
- Phiếu đóng gói cấp 2
- Phiếu QC thành phẩm
- Lệnh nhập kho thành phẩm

**4.3. Từ phiếu nhập nguyên liệu đầu vào**

Khi nguyên liệu đạt và nhập kho thành công, hệ thống cho phép:

- sử dụng lô nguyên liệu đó cho phiếu cấp phát sản xuất
- gắn lô nguyên liệu vào phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất

**4.4. Từ phiếu đề nghị cấp nguyên liệu**

Hệ thống sinh:

- Phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất
- Phiếu kế toán xuất nguyên liệu cho sản xuất

**5\. NGUYÊN TẮC KẾ THỪA GIỮA CÁC PHIẾU**

**5.1. Phiếu sau phải tự kéo dữ liệu từ phiếu trước**

Ví dụ:

- Lệnh sản xuất kéo từ kế hoạch
- Phiếu đề nghị cấp nguyên liệu kéo từ công thức SKU
- Phiếu chấp thuận kéo từ phiếu đề nghị cấp
- Phiếu sơ chế kéo từ phiếu chấp thuận
- Phiếu sấy kéo từ sơ chế
- Phiếu đóng gói kéo từ phiếu sấy
- Phiếu QC thành phẩm kéo từ đóng gói
- Lệnh nhập kho kéo từ QC thành phẩm

**5.2. Không nhập lại dữ liệu nền**

Các trường sau không được nhập lại:

- SKU
- Tên sản phẩm
- Công thức
- Version
- Danh sách nguyên liệu
- Hàm lượng chuẩn
- Đơn vị chuẩn
- Nhân sự chính
- Ca sản xuất
- Bộ phận
- Kho / vị trí kho
- Lô nguyên liệu đã duyệt

**5.3. Người dùng chỉ cập nhật phần vận hành**

Ở mỗi phiếu sau, người dùng chỉ xử lý:

- đánh giá
- ghi chú
- số liệu thực tế
- ảnh / video
- xác nhận

**6\. KHỐI XÁC NHẬN THEO TỪNG CÁ NHÂN**

**6.1. Bắt buộc**

Mỗi phiếu phải có:

- Vai trò
- Họ tên
- Bộ phận
- Trạng thái xác nhận
- Thời gian xác nhận
- Ghi chú

**6.2. Trạng thái xác nhận**

- Chưa xác nhận
- Đã xác nhận
- Từ chối xác nhận

**6.3. Quy tắc chuyển bước**

Không đủ xác nhận bắt buộc thì phiếu không được chuyển bước.

**7\. QUY TẮC ĐƠN VỊ THEO TỪNG PHIẾU**

**7.1. Phiếu nhập nguyên liệu đầu vào**

- kg
- lít
- ml

**7.2. Phiếu cấp nguyên liệu cho sản xuất**

- kg
- lít
- ml

**7.3. Phiếu theo dõi sơ chế**

- khay
- kg
- lít

**7.4. Phiếu kiểm chất lượng sau sấy thăng hoa**

- khay
- % độ ẩm

**7.5. Phiếu đóng gói cấp 1**

- gói
- lọ
- hũ

**7.6. Phiếu đóng gói cấp 2**

- hộp
- thùng
- lọ
- hũ

**7.7. Lệnh nhập kho thành phẩm**

- hộp
- thùng
- lọ
- hũ

**8\. QUY TẮC BẰNG CHỨNG**

**8.1. Phiếu bắt buộc có ảnh / video**

- Phiếu nhập nguyên liệu đầu vào
- Phiếu theo dõi sơ chế
- Phiếu kiểm chất lượng sau sấy thăng hoa
- Phiếu đóng gói cấp 1
- Phiếu đóng gói cấp 2
- Lệnh nhập kho thành phẩm

**8.2. Dữ liệu bằng chứng**

- ảnh
- video
- file đính kèm
- biên bản liên quan

**9\. PASS / FAIL GATE**

**9.1. Đạt khi**

- nguyên liệu đầu vào được click chọn đúng ở phiếu nhập
- nguyên liệu cho sản xuất tự hiện đúng theo công thức
- công thức tự hiện chi tiết tới từng nguyên liệu
- phiếu sau tự kế thừa dữ liệu
- người dùng không phải lập lại phiếu từ đầu
- đủ xác nhận theo từng cá nhân
- đúng đơn vị theo từng công đoạn

**9.2. Chưa đạt khi**

- lệnh sản xuất còn cho chọn tay nguyên liệu
- phiếu cấp phát còn cho sửa tay danh sách nguyên liệu
- công thức vẫn hiện ở dạng nhóm mơ hồ
- phải nhập lại dữ liệu nền
- thiếu khối xác nhận
- sai đơn vị vận hành
- thiếu % độ ẩm ở phiếu sau sấy

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

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P2.2E - TRACEABILITY QR RECALL SALE LOCK

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-01 Traceability / QR / Recall / Sale Lock.
- TECH-03 Traceability / Recall / Sale Lock.
- PACK-10 Evidence / Smoke Gateway.
- Recall / Sale Lock thang downstream.

## Noi Dung Rewrite

## 32. KẾT LUẬN ĐIỀU PHỐI P2.2D

Từ thời điểm dùng bản V2.0 này, miền Batch / QC / Release / Warehouse / Inventory phải tuân thủ các khóa sau:

Batch phải linked Production Dossier.

Batch phải dựa trên SKU, formula snapshot, material genealogy.

Batch không được tạo trôi nổi.

Sơ chế, sấy, QC sau sấy, đóng gói cấp 1, đóng gói cấp 2, QC thành phẩm phải dựa trên batch/dossier.

Batch QC_PASS không đồng nghĩa RELEASED.

Batch Release là gate riêng.

Batch HOLD/REJECTED/QC_FAIL không được release.

Warehouse chỉ nhận batch RELEASED.

Finished Goods Ledger Credit chỉ tạo khi Warehouse Receipt confirmed.

Retry Warehouse Receipt không được double credit.

Warehouse Receipt không được tự set Sellable.

Finished Goods Inventory không tự Sellable.

Print cấp 1 chỉ in MFG/HSD.

Print cấp 2 in batch/MFG/HSD/barcode/QR.

Máy in chỉ nhận payload, không tự sinh mã.

Disposal/write-off không được xóa tay batch/thành phẩm/bao bì.

MISA không điều khiển batch/release/warehouse/ledger.

Actor/permission/audit/idempotency/evidence là bắt buộc.

Evidence Submitted chưa phải Evidence Accepted.

Global Gate vẫn bị chặn.

Tài liệu kế tiếp cần viết lại toàn bộ là:

## 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK

## TÀI LIỆU ĐẶC TẢ TRACEABILITY / QR / PUBLIC TRACE / RECALL / HOLD / SALE LOCK CHO PHASE 2

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

## PHASE-02-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK

Đây là file thứ năm trong nhóm triển khai giới hạn của Phase 2 - Operational Core.

File này phụ trách miền:

Internal traceability.

Internal genealogy.

Trace projection.

Trace gap detection.

QR foundation.

Barcode foundation.

QR lifecycle.

QR public-valid guard.

Print payload trace boundary.

Reprint control.

Public Trace whitelist DTO.

Public Trace private/internal field suppression.

Recall Case.

Recall Impact Analysis.

Hold.

Sale Lock.

Downstream suppression boundary.

Commerce/AI/Gateway/Ads/Live/CRM/IVR suppression boundary.

Actor / permission / audit / idempotency / evidence.

Smoke/evidence cho P2.2E.

Report 15 mục cho P2.2E.

Tài liệu này thay thế toàn bộ nội dung cũ của:

## 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK.docx

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK

Phiên bản

Phase

## PHASE 2 - OPERATIONAL CORE

Workstream

## P2.2E

Loại tài liệu

Limited Implementation Handoff / Traceability-QR-Recall-SaleLock

Nguồn chính

README Phase 2 V1.1 + P2.1 Technical Design + P2.2B/C/D

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

Phụ thuộc Phase 1

SKU / Formula / Packaging Profile / UOM / Public-safe field policy

Phụ thuộc P2.2B

Production Dossier / Formula Snapshot

Phụ thuộc P2.2C

Material Issue / Raw Lot / Ledger

Phụ thuộc P2.2D

Batch / Release / Warehouse Receipt / Packaging / Print boundary

Cho phép code ngay bởi file này?

Không, chỉ sau khi có lệnh Limited Implementation riêng

Cho phép migration ngay bởi file này?

Không

Cho phép seed ngay bởi file này?

Không

Cho phép Agent tự suy luận public trace field?

Không

Global Gate

bị chặn

Production Readiness

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để khóa lớp truy xuất, QR, public trace, recall và sale lock của Operational Core.

Đây là lớp bảo vệ cuối của Phase 2 trước khi dữ liệu vận hành được các hệ sau tiêu thụ, gồm:

Commerce Runtime.

AI Advisor Runtime.

Facebook Gateway.

Ads Measurement.

MC AI Live.

## CRM.

## IVR.

Public Trace cho khách hàng.

Nếu P2.2E sai, hệ thống có thể phát sinh các lỗi nghiêm trọng:

Public Trace lộ dữ liệu nội bộ.

QR lỗi vẫn tra cứu hợp lệ.

QR VOID/FAILED vẫn public-valid.

Máy in hoặc printer tự sinh mã ngoài hệ thống.

Reprint không có kiểm soát, gây rủi ro tem/nhãn giả.

Traceability không dựng được genealogy từ raw lot -> production -> batch -> warehouse.

Recall không xác định được batch/QR/SKU bị ảnh hưởng.

Sale Lock không chặn Commerce/AI/Gateway/Ads/Live/CRM/IVR.

Sản phẩm bị recall vẫn được chốt đơn.

Sản phẩm bị hold vẫn được quảng cáo/livestream/tư vấn.

Internal defect/loss/cost/personnel/audit bị lộ ra public.

Phase 3 có thể dùng sai operational truth để mở bán.

Mục tiêu chính của tài liệu này:

Khóa traceability phải bám theo Production Dossier và genealogy nội bộ.

Khóa QR cấp 2 phải gắn batch/MFG/HSD/barcode/QR theo payload do Ginsengfood sinh.

Khóa máy in chỉ nhận payload, in và trả trạng thái.

Khóa máy in không được tự sinh batch, MFG, HSD, barcode, QR.

Khóa QR VOID/FAILED/bị chặn không public-valid.

Khóa Public Trace whitelist-only.

Khóa Public Trace không lộ private/internal field.

Khóa Reprint phải có lý do, quyền, log và audit.

Khóa Recall Case và Impact Analysis.

Khóa Hold / Sale Lock.

Khóa Sale Lock thắng Commerce/AI/Gateway/Ads/Live/CRM/IVR.

Khóa actor/permission/audit/idempotency/evidence cho mọi command rủi ro.

Khóa Evidence Submitted chưa phải Evidence Accepted.

Khóa Global Gate vẫn bị chặn.

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

Không thay file chi tiết P2.2E

Tầng 2

P2.1 Technical Design V1.1

Thiết kế workstream

Cung cấp form registry, field behavior, data inheritance

Không tự cho phép code

Tầng 3

P2.2B Production Order / Dossier / Snapshot

Production root truth

Cung cấp Production Dossier và Formula Snapshot

Không dựng trace nếu thiếu dossier

Tầng 4

P2.2C Material Issue / Receipt / Ledger

Raw material movement truth

Cung cấp raw lot/material issue genealogy

Không tự tạo issue trong P2.2E

Tầng 5

P2.2D Batch / Release / Warehouse / Packaging

Finished goods truth

Cung cấp batch, warehouse receipt, packaging/print boundary

Không tự release/warehouse trong P2.2E

Tầng 6

P2.2E V1.1 này

Nguồn chính cho Trace/QR/Recall/Sale Lock

Giao dev/Agent phân tích/triển khai giới hạn khi được mở

Không lấn sang Commerce/AI/Gateway full

Tầng 7

Phase 1 SKU/Formula/Packaging Profile

Dữ liệu nền

Cung cấp SKU, packaging profile, public-safe reference

Không sửa Phase 1 trong file này

Tầng 8

nguồn tổng hợp TÀI LIỆU.docx

Nguồn phụ trợ nghiệp vụ

Đối chiếu QR cấp 2, print payload, reprint, recall/sale lock

Không code/seed trực tiếp

Tầng 9

Code hiện tại

Hiện trạng triển khai

Dùng để gap analysis

Không là source-of-truth nếu conflict

## 5. SCOPE IN - PHẠM VI BAO GỒM

Tài liệu này bao gồm:

Internal traceability.

Internal genealogy link.

Raw lot -> Production Dossier -> Material Issue -> Batch -> Warehouse genealogy.

Trace projection.

Trace gap detection.

QR generation boundary.

Barcode boundary.

QR status lifecycle.

QR public-valid guard.

Print payload trace boundary.

Print job status boundary.

Reprint request.

Reprint approval.

Reprint payload snapshot.

Public Trace DTO.

Public Trace whitelist fields.

Public Trace private/internal field suppression.

Recall Case.

Recall Impact Analysis.

Recall affected scope.

Hold.

Sale Lock.

Stop-sale suppression.

Downstream suppression hooks.

Commerce suppression boundary.

AI Advisor suppression boundary.

Facebook Gateway suppression boundary.

Ads/Live/CRM/IVR suppression boundary.

MISA boundary for trace/recall/sale lock.

Actor / permission / audit / idempotency / evidence.

Smoke/evidence requirements cho P2.2E.

Report 15 mục cho P2.2E.

## 6. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM

Tài liệu này không bao gồm:

Không triển khai Source / Raw Lot / QC đầu vào đầy đủ.

Không triển khai Production Order đầy đủ.

Không triển khai Formula Snapshot đầy đủ.

Không triển khai Material Request/Issue/Receipt đầy đủ.

Không triển khai Batch/Release/Warehouse đầy đủ.

Không quyết định Commerce Sellable Gate.

Không tạo giá bán.

Không tạo QuoteSnapshot.

Không tạo Cart.

Không tạo Official Order.

Không xử lý Payment.

Không xử lý Verified Revenue.

Không triển khai AI Advisor runtime đầy đủ.

Không triển khai Facebook Gateway runtime đầy đủ.

Không triển khai Ads Measurement đầy đủ.

Không triển khai MC AI Live đầy đủ.

Không triển khai CRM automation đầy đủ.

Không triển khai IVR đầy đủ.

Không triển khai full MISA Integration.

Không cho MISA điều khiển trace/recall/sale lock.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 7. ĐỊNH NGHĨA THUẬT NGỮ

Thuật ngữ

Định nghĩa

Internal Traceability

Truy xuất nội bộ đầy đủ, gồm raw lot, production, batch, warehouse

Genealogy

Dòng phả hệ dữ liệu từ nguyên liệu đến thành phẩm

Trace Projection

Bản chiếu dữ liệu truy xuất được tổng hợp từ genealogy

Public Trace

Dữ liệu truy xuất an toàn được phép hiển thị cho khách hàng

Public Trace DTO

Cấu trúc dữ liệu whitelist-only cho public

## QR

Mã truy xuất trên sản phẩm/bao bì

Barcode

Mã thương mại hoặc mã định danh đóng gói theo cấu hình

Public-valid QR

QR đủ điều kiện cho khách tra cứu public

## QR VOID

QR bị vô hiệu hóa

## QR FAILED

QR tạo/in lỗi

QR bị chặn

QR bị chặn vì trace/recall/sale lock/policy

Print Payload

Dữ liệu in do Ginsengfood sinh

Reprint

In lại nhãn/tem/QR/barcode

Recall

Thu hồi sản phẩm/batch/QR/SKU theo case

Hold

Tạm giữ, chưa cho vận hành/bán

Sale Lock

Chặn bán/chặn chốt/chặn downstream

Downstream Suppression

Chặn Commerce/AI/Gateway/Ads/Live/CRM/IVR tiêu thụ sản phẩm bị lock

Whitelist-only

Chỉ trả field được phép public

Private/Internal Field

Dữ liệu nội bộ không được public

## PHẦN 2/4 - TRACEABILITY / QR / BARCODE / PUBLIC TRACE MODEL

## 8. LUỒNG NGHIỆP VỤ TỔNG QUAN P2.2E

Luồng nghiệp vụ P2.2E:

Production Dossier -> Material Issue Genealogy -> Batch -> Batch Release -> Warehouse Receipt -> Packaging Level 2 -> Print Payload -> QR / Barcode -> Trace Projection -> Public Trace Whitelist -> Recall / Hold / Sale Lock -> Downstream Suppression

Trong đó:

Traceability phải bám theo Production Dossier.

Production Dossier là root production file.

Material Issue cung cấp raw lot genealogy.

Batch dựa trên Production Dossier và material genealogy.

Warehouse Receipt confirmed tạo finished goods truth.

Packaging Level 2 là nơi QR/barcode có ý nghĩa public-facing theo rule.

Print payload phải do Ginsengfood sinh.

QR/barcode không được do máy in tự sinh.

QR chỉ public-valid khi status, trace projection và public trace policy hợp lệ.

Public Trace chỉ trả whitelist field.

Recall/Sale Lock chặn downstream.

## 9. INTERNAL TRACEABILITY / GENEALOGY

## 9.1. Nguyên tắc

Internal Traceability phải dựng được chuỗi:

Source Origin -> Raw Receipt -> Raw Lot -> Incoming QC -> Readiness -> Material Issue -> Production Dossier -> Batch -> Batch Process -> Batch QC -> Batch Release -> Warehouse Receipt -> Packaging -> QR/Barcode -> Public Trace

Traceability không được chỉ bắt đầu từ batch.

Traceability không được chỉ bắt đầu từ QR.

Traceability phải có khả năng truy ngược về:

Nguyên liệu.

Raw lot.

QC đầu vào.

Readiness.

Production order.

Formula version.

Material issue.

Batch.

QC batch/thành phẩm.

Release.

Warehouse receipt.

Packaging/print/QR.

## 9.2. Checklist Internal Genealogy Link

Trường

Bắt buộc

Quy tắc

genealogy_link_code

Có

Mã link

source_type

Có

## RAW_LOT / MATERIAL_ISSUE / PRODUCTION_DOSSIER / BATCH / WAREHOUSE / QR

source_ref

Có

Mã nguồn

target_type

Có

Loại đích

target_ref

Có

Mã đích

relationship_type

Có

## USED_IN / PRODUCED_BY / PACKAGED_AS / PRINTED_AS / STORED_AS

sku_code

Có nếu liên quan thành phẩm

## SKU

batch_code

Có nếu liên quan batch

Batch

production_dossier_code

Có

Root

raw_lot_code

Có nếu liên quan raw

Raw lot

quantity

Tùy

Nếu cần trace định lượng

uom

Tùy

Nếu có quantity

created_by

Có

Actor/system

created_at

Có

Thời điểm

audit_log

Có

Bắt buộc

## 9.3. Trace gap detection

Trace gap phải được flag nếu thiếu:

Production Dossier.

Formula Snapshot.

Material Issue.

Raw lot genealogy.

Batch link.

Batch release.

Warehouse receipt.

Packaging link.

QR link.

Public trace projection.

Required evidence.

Nếu trace gap chưa xử lý:

QR không được public-valid nếu policy yêu cầu đầy đủ trace.

Public Trace không được trả dữ liệu mơ hồ.

Downstream không được dùng trace đó làm bằng chứng bán hàng.

## 9.4. Fail gate internal traceability

Fail nếu:

Batch không trace được về Production Dossier.

Batch không trace được về material issue/raw lot khi policy yêu cầu.

QR không trace được về batch.

Public Trace trả dữ liệu khi trace gap nghiêm trọng.

Trace projection tự suy đoán missing genealogy.

Trace sửa lịch sử production snapshot.

Trace lộ internal evidence/private field ra public.

## 10. TRACE PROJECTION

## 10.1. Nguyên tắc

Trace Projection là bản tổng hợp dữ liệu truy xuất từ các nguồn nội bộ.

Trace Projection không phải source-of-truth gốc.

Source-of-truth gốc vẫn là:

Raw receipt.

Raw lot.

QC/readiness.

Production Dossier.

Material Issue.

Batch.

Batch QC/release.

Warehouse Receipt.

Packaging/print.

Recall/Sale Lock.

Projection có thể được rebuild khi nguồn thay đổi.

Projection phải có status.

## 10.2. Checklist Trace Projection

Trường

Bắt buộc

Quy tắc

trace_projection_code

Có

Mã projection

batch_code

Có

Batch

sku_code

Có

## SKU

production_dossier_code

Có

Root

warehouse_receipt_code

Có nếu public

Thành phẩm đã nhập kho

qr_code_ref

Có nếu QR

QR liên quan

projection_status

Có

DRAFT / VALID / PARTIAL / bị chặn / SUPERSEDED

trace_gap_flag

Có

## TRUE/FALSE

public_trace_eligible

Có

## TRUE/FALSE

recall_lock_flag

Có

## TRUE/FALSE

sale_lock_flag

Có

## TRUE/FALSE

generated_at

Có

Thời điểm

generated_by

Có

System/actor

audit_log

Có

Bắt buộc

## 10.3. Projection status

Status

Ý nghĩa

Public trace được không?

## DRAFT

Chưa hoàn thiện

Không

## VALID

Đủ dữ liệu theo policy

Có thể

## PARTIAL

Thiếu một phần dữ liệu

Không hoặc chỉ public hạn chế nếu Owner policy cho phép

bị chặn

Bị chặn vì trace gap/recall/sale lock

Không

## SUPERSEDED

Bị thay bằng projection mới

Không

## ERROR

Lỗi build projection

Không

## 11. QR / BARCODE FOUNDATION

## 11.1. Nguyên tắc

nguồn tổng hợp đã khóa:

In mã sản phẩm chia 2 cấp.

Cấp 1 chỉ in MFG/HSD.

Cấp 2 in batch/MFG/HSD/barcode/QR.

Ginsengfood sinh dữ liệu in.

Máy in chỉ nhận payload, in và trả trạng thái.

Máy in không được tự sinh mã sản xuất, mã lô, ngày sản xuất, hạn dùng, barcode, QR.

Trong P2.2E, QR/barcode phải được hiểu là dữ liệu trace/sale/public-risk, không chỉ là dữ liệu in.

## 11.2. QR chỉ thuộc public trace khi đủ điều kiện

QR chỉ được public-valid nếu:

QR được sinh bởi Ginsengfood.

QR linked batch hợp lệ.

QR linked packaging level 2 nếu rule yêu cầu.

Batch đã RELEASED.

Warehouse Receipt confirmed nếu policy yêu cầu.

Trace Projection hợp lệ.

Public Trace DTO an toàn.

QR status hợp lệ.

Không VOID.

Không FAILED.

Không bị chặn.

Không recall lock.

Không sale lock theo policy.

Không thiếu required evidence nếu policy yêu cầu.

## 11.3. QR status lifecycle

Status

Ý nghĩa

Public-valid?

## DRAFT

QR chuẩn bị tạo

Không

## GENERATED

QR đã sinh

Chưa chắc

## PRINTED

QR đã in

Chưa chắc

## ACTIVE_PUBLIC

Đủ điều kiện public trace

Có

## FAILED

Sinh/in lỗi

Không

## VOID

Vô hiệu hóa

Không

bị chặn

Bị chặn vì policy/trace/recall/sale lock

Không

## SUPERSEDED

Bị thay thế

Không

## REPRINTED

Đã in lại

Chỉ public nếu replacement hợp lệ

## RECALLED

Bị recall

Không hoặc chỉ hiển thị thông báo recall theo policy

## SALE_LOCKED

Bị stop-sale

Không dùng để bán/chốt

## 11.4. Checklist QR

Trường

Bắt buộc

Quy tắc

qr_code

Có

Mã QR duy nhất

qr_status

Có

Theo lifecycle

batch_code

Có

Trỏ batch

sku_code

Có

Trỏ SKU

packaging_level

Có

Thường là LEVEL_2 theo nguồn tổng hợp

print_payload_ref

Có

Payload do Ginsengfood sinh

public_trace_projection_ref

Có nếu public

Trace projection

barcode_ref

Có nếu có barcode

Barcode liên quan

mfg_date_snapshot

Có

Snapshot

hsd_date_snapshot

Có

Snapshot

public_valid_flag

Có

## TRUE/FALSE

recall_lock_flag

Có

## TRUE/FALSE

sale_lock_flag

Có

## TRUE/FALSE

created_by

Có

Actor/system

created_at

Có

Thời điểm

audit_log

Có

Bắt buộc

## 11.5. Barcode boundary

Barcode phải đến từ:

Product/packaging master.

Trade item / packaging profile.

Payload do Ginsengfood sinh.

Approved barcode/GTIN config nếu có.

Barcode không được:

Do máy in tự sinh.

Do operator nhập tay tùy tiện.

Thay đổi theo từng lần in nếu là barcode thương mại cố định.

Dùng sai cấp packaging.

Public nếu bị VOID/bị chặn theo QR/trace policy.

## 11.6. Fail gate QR / Barcode

Fail nếu:

QR không linked batch.

QR không linked trace projection.

QR VOID vẫn public-valid.

QR FAILED vẫn public-valid.

QR bị chặn vẫn public-valid.

QR linked batch chưa release nhưng public-valid.

QR linked trace gap nghiêm trọng nhưng public-valid.

Máy in tự sinh QR.

Máy in tự sinh barcode.

Operator nhập tay QR/barcode.

Barcode cấp 2 không bám packaging profile.

QR sale-locked vẫn cho Commerce/AI/Gateway dùng để bán.

## 12. PRINT PAYLOAD / PRINTER BOUNDARY

## 12.1. Nguyên tắc

P2.2E khóa kỹ hơn về payload in:

Ginsengfood sinh payload. Máy in chỉ nhận payload, in và trả trạng thái.

Không được để máy in tự sinh:

Mã sản xuất.

Batch code.

## MFG.

## HSD.

Barcode.

## QR.

Trace URL.

Public trace token.

Recall/Sale Lock status.

## 12.2. Print payload cấp 1

Theo nguồn tổng hợp:

Cấp 1 chỉ in MFG/HSD.

Checklist:

Trường

Bắt buộc

Quy tắc

print_payload_code

Có

Mã payload

print_level

Có

## LEVEL_1

batch_code_ref

Có để trace nội bộ

Có thể không in public nếu rule chỉ MFG/HSD

mfg_date

Có

Do Ginsengfood sinh

hsd_date

Có

Do Ginsengfood sinh

label_template_ref

Có

Template cấp 1

printer_target

Có

Máy in

payload_status

Có

## WAITING / SENT / PRINTED / FAILED / VOID

audit_log

Có

Bắt buộc

## 12.3. Print payload cấp 2

Theo nguồn tổng hợp:

Cấp 2 in batch/MFG/HSD/barcode/QR.

Checklist:

Trường

Bắt buộc

Quy tắc

print_payload_code

Có

Mã payload

print_level

Có

## LEVEL_2

batch_code

Có

Do Ginsengfood sinh

sku_code

Có

## SKU

mfg_date

Có

Do Ginsengfood sinh

hsd_date

Có

Do Ginsengfood sinh

barcode_value

Có nếu required

Do Ginsengfood sinh từ config

qr_code

Có nếu required

Do Ginsengfood sinh

trace_projection_ref

Có nếu QR

Trace

label_template_ref

Có

Template cấp 2

printer_target

Có

Máy in

payload_status

Có

## WAITING / SENT / PRINTED / FAILED / VOID

audit_log

Có

Bắt buộc

## 12.4. Printer status

Status

Ý nghĩa

Hành động

## WAITING

Chưa gửi máy in

Chưa in

## SENT

Đã gửi payload

Chờ phản hồi

## PRINTED

Máy in báo đã in

Có thể chuyển QR printed

## FAILED

Máy in báo lỗi

QR/print không public-valid

## VOID

Payload bị vô hiệu

Không public-valid

## RETRY_WAITING

Chờ gửi lại

Không public-valid nếu chưa in

## REPRINT_REQUIRED

Cần in lại

Chuyển reprint flow

## 12.5. Fail gate printer

Fail nếu:

Máy in tự sinh mã.

Máy in tự sinh batch.

Máy in tự sinh MFG/HSD.

Máy in tự sinh barcode.

Máy in tự sinh QR.

Payload FAILED nhưng QR public-valid.

Payload VOID nhưng QR public-valid.

Print cấp 1 in sai rule.

Print cấp 2 thiếu field bắt buộc.

Không có audit payload.

## 13. REPRINT CONTROL

## 13.1. Nguyên tắc

nguồn tổng hợp đã khóa:

Reprint phải có lý do, quyền, log và audit.

Reprint là hành động rủi ro cao vì có thể tạo thêm nhãn/QR/barcode vật lý ngoài thị trường.

Do đó, reprint không được tự do.

Reprint phải có:

Original print job.

Reprint request.

Reprint reason.

Authorized actor.

Approval nếu policy yêu cầu.

Payload snapshot.

Old label disposition nếu có.

Audit.

Evidence.

## 13.2. Checklist Reprint Request

Trường

Bắt buộc

Quy tắc

reprint_request_code

Có

Mã yêu cầu in lại

original_print_payload_ref

Có

Payload gốc

original_qr_code

Có nếu có QR

QR gốc

batch_code

Có

Batch

reason_code

Có

Lý do

reason_note

Có nếu policy yêu cầu

Ghi chú

requested_by

Có

Actor

requested_at

Có

Thời điểm

approval_status

Có

## WAITING / APPROVED / REJECTED

approved_by

Có nếu approved

Actor có quyền

approved_at

Có nếu approved

Thời điểm

reprint_payload_ref

Có nếu approved

Payload in lại

old_label_disposition

Có nếu có

Thu hồi/hủy/ghi nhận

evidence_ref

Có nếu policy yêu cầu

Evidence

audit_log

Có

Bắt buộc

## 13.3. Reprint status

Status

Ý nghĩa

## REQUESTED

Đã yêu cầu

## WAITING_APPROVAL

Chờ duyệt

## APPROVED

Đã duyệt

## REJECTED

Bị từ chối

## REPRINT_PAYLOAD_CREATED

Đã tạo payload mới

## REPRINTED

Đã in lại

## VOIDED_ORIGINAL

QR/label gốc đã void nếu policy yêu cầu

Đóng case

## 13.4. Fail gate reprint

Fail nếu:

Reprint không có original print reference.

Reprint không có reason.

Reprint không có permission.

Reprint không có audit.

Reprint tự tạo QR mới không linked original.

Reprint không xử lý old label disposition nếu policy yêu cầu.

Reprint bypass recall/sale lock.

Reprint làm QR VOID/FAILED thành public-valid.

Reprint không có evidence khi policy yêu cầu.

## 14. PUBLIC TRACE WHITELIST

## 14.1. Nguyên tắc

Public Trace chỉ được trả dữ liệu whitelist.

Public Trace không phải internal trace.

Public Trace không được lộ:

Supplier private detail.

Internal source contract.

Cost data.

Internal formula ratio.

Full BOM detail.

Internal ingredient quantity.

Internal QC defect detail.

Internal reject reason chi tiết.

Internal loss/variance.

Personnel/user info.

Worker note.

Admin note.

MISA/accounting data.

Evidence private data.

Recall internal note.

Raw material internal lot metadata không public.

Audit log nội bộ.

Idempotency key.

Internal correlation ID nếu policy không cho public.

Any private/internal field.

## 14.2. Public Trace whitelist đề xuất

Public Trace có thể gồm các nhóm field an toàn sau, nhưng vẫn cần Owner/Public Policy xác nhận:

Nhóm field

Có thể public

Ghi chú

Product

Tên sản phẩm public

Không dùng internal SKU code nếu không public

Brand

Ginsengfood / SSAVIGROUP

Theo chính sách

Batch display

Mã batch public nếu được phép

Không lộ internal IDs nếu policy chặn

## MFG

Ngày sản xuất

Theo label

## HSD

Hạn sử dụng

Theo label

QR status public

Hợp lệ / không hợp lệ / bị thu hồi

Diễn đạt public-safe

Trace summary

Đã được sản xuất theo quy trình truy xuất

Không lộ BOM

Quality status public

Đã qua kiểm soát chất lượng theo chính sách

Không lộ defect nội bộ

Recall notice

Có nếu bị recall

Nội dung public-safe

Sale lock notice

Có nếu stop-sale

Nội dung public-safe

Customer guidance

Hướng dẫn liên hệ kênh chính thức

Không lộ nội bộ

Public certification/ref

Nếu có và được approved

Chỉ evidence public-approved

## 14.3. Public Trace DTO boundary

Rule

Diễn giải

DTO public riêng

Không dùng Admin/Internal DTO

Whitelist-only

Field không có trong whitelist thì không trả

No raw JSON debug

Không lộ cấu trúc nội bộ

No internal IDs

Không lộ ID nội bộ nếu không approved

No cost/accounting

Không lộ giá thành/MISA

No full formula

Không lộ BOM/ratio/quantity

No private evidence

Không lộ ảnh/chứng từ nội bộ

No personnel

Không lộ người vận hành

No audit

Không lộ audit/correlation/idempotency

Safe recall message

Nếu recall, chỉ thông tin public-safe

## 14.4. Fail gate Public Trace

Fail nếu:

Public Trace dùng Admin DTO.

Public Trace trả full internal trace.

Public Trace lộ supplier private detail.

Public Trace lộ formula/BOM/ratio.

Public Trace lộ cost/MISA.

Public Trace lộ QC defect nội bộ.

Public Trace lộ evidence private.

Public Trace lộ audit/idempotency/correlation.

Public Trace trả QR VOID/FAILED như hợp lệ.

Public Trace không có whitelist policy.

## PHẦN 3/4 - RECALL / HOLD / SALE LOCK / DOWNSTREAM SUPPRESSION / MISA BOUNDARY

## 15. RECALL CASE

## 15.1. Nguyên tắc

Recall Case là nghiệp vụ thu hồi hoặc kiểm soát rủi ro liên quan đến sản phẩm, batch, QR, SKU hoặc lô nguyên liệu.

Recall không được là ghi chú tự do không có trạng thái.

Recall phải có:

Case code.

Scope.

Reason.

Impact analysis.

Actor/permission.

Evidence.

Status.

Downstream suppression nếu cần.

Audit.

## 15.2. Recall scope

Recall có thể áp dụng theo:

Scope

Ý nghĩa

## RAW_LOT

Thu hồi/kiểm soát theo lô nguyên liệu

## PRODUCTION_DOSSIER

Theo hồ sơ sản xuất

## BATCH

Theo batch

## SKU

Theo SKU

## QR

Theo QR

## WAREHOUSE_RECEIPT

Theo nhập kho

## CUSTOMER_SHIPMENT

Theo lô đã giao, thuộc phase sau

## MIXED_SCOPE

Nhiều scope cùng lúc

## 15.3. Checklist Recall Case

Trường

Bắt buộc

Quy tắc

recall_case_code

Có

Mã case

recall_scope

Có

## RAW_LOT / BATCH / SKU / QR / MIXED

target_refs

Có

Danh sách target

recall_reason_code

Có

Lý do

recall_reason_note

Có nếu policy yêu cầu

Ghi chú

severity

Có

## LOW / MEDIUM / HIGH / CRITICAL

status

Có

OPEN / INVESTIGATING / IMPACT_ANALYZED / LOCKED / CLOSED / CANCELLED

opened_by

Có

Actor

opened_at

Có

Thời điểm

evidence_ref

Có

Evidence

owner_review_status

Có nếu policy yêu cầu

## WAITING / APPROVED / REJECTED

audit_log

Có

Bắt buộc

## 16. RECALL IMPACT ANALYSIS

## 16.1. Nguyên tắc

Recall Impact Analysis phải xác định các đối tượng bị ảnh hưởng.

Nếu recall từ raw lot, phải truy ra:

Material Issue.

Production Dossier.

Batch.

Warehouse Receipt.

## QR.

## SKU.

Sale lock target.

Downstream suppression target.

Nếu recall từ batch, phải truy ra:

## QR.

Warehouse Receipt.

## SKU.

Public Trace.

Sale lock target.

Downstream suppression.

## 16.2. Checklist Recall Impact

Trường

Bắt buộc

Quy tắc

impact_analysis_code

Có

Mã phân tích

recall_case_code

Có

Trỏ case

input_target

Có

Target ban đầu

impacted_raw_lots

Tùy scope

Danh sách

impacted_dossiers

Tùy scope

Danh sách

impacted_batches

Có nếu batch impacted

Danh sách

impacted_skus

Có nếu SKU impacted

Danh sách

impacted_qrs

Có nếu QR impacted

Danh sách

impacted_warehouse_receipts

Tùy

Danh sách

trace_gap_flag

Có

## TRUE/FALSE

recommended_hold

Có

## TRUE/FALSE

recommended_sale_lock

Có

## TRUE/FALSE

recommended_public_notice

Có nếu cần

## TRUE/FALSE

analyzed_by

Có

Actor/system

analyzed_at

Có

Thời điểm

audit_log

Có

Bắt buộc

## 16.3. Fail gate Recall Impact

Fail nếu:

Recall case không có impact analysis.

Recall từ raw lot không truy được batch.

Recall từ batch không truy được QR.

Trace gap không được flag.

Recall không tạo recommended hold/sale lock khi cần.

Recall chỉ là ghi chú không có state.

Recall không audit.

Recall không chặn downstream khi đã sale lock.

## 17. HOLD

## 17.1. Nguyên tắc

Hold là trạng thái tạm giữ.

Hold có thể áp dụng cho:

Raw lot.

Production Dossier.

Batch.

Warehouse stock.

## QR.

## SKU.

Packaging/print payload.

Trace projection.

Hold không nhất thiết là recall, nhưng nếu hold ảnh hưởng bán hàng thì phải có suppression boundary.

## 17.2. Checklist Hold

Trường

Bắt buộc

Quy tắc

hold_code

Có

Mã hold

hold_scope

Có

## RAW_LOT / BATCH / SKU / QR / WAREHOUSE / OTHER

target_ref

Có

Đối tượng bị hold

hold_reason_code

Có

Lý do

hold_status

Có

## ACTIVE / RELEASED / CANCELLED

hold_started_by

Có

Actor

hold_started_at

Có

Thời điểm

hold_released_by

Có nếu release

Actor

hold_released_at

Có nếu release

Thời điểm

evidence_ref

Có nếu policy yêu cầu

Evidence

audit_log

Có

Bắt buộc

## 17.3. Hold effect

Target hold

Effect

Raw lot hold

Không issue

Batch hold

Không release / không warehouse nếu chưa release

Warehouse stock hold

Không đưa vào sellable stock

QR hold

Không public-valid hoặc public warning theo policy

SKU hold

Chặn downstream selling

Trace projection hold

Public trace bị block hoặc limited

## 18. SALE LOCK

## 18.1. Nguyên tắc

Sale Lock là trạng thái chặn bán.

Sale Lock phải thắng mọi downstream channel:

Commerce.

AI Advisor.

Facebook Gateway.

Ads.

MC AI Live.

## CRM.

## IVR.

Website.

Landing page.

Any order/quote/selling channel.

Nếu Sale Lock active, downstream không được:

Báo SKU sellable.

Tư vấn chốt mua.

Tạo quote.

Tạo cart.

Tạo order.

Xác nhận đơn.

Đẩy quảng cáo.

Livestream chốt SKU.

Gửi CRM thúc mua.

IVR xác nhận đơn chứa SKU/batch/QR bị lock.

## 18.2. Sale Lock scope

Sale Lock có thể áp dụng theo:

Scope

Ý nghĩa

## SKU

Chặn toàn SKU

## BATCH

Chặn batch cụ thể

## QR

Chặn QR cụ thể

## WAREHOUSE_STOCK

Chặn tồn kho cụ thể

## PRODUCT_LINE

Chặn dòng sản phẩm

## RAW_LOT_IMPACT

Chặn thành phẩm bị ảnh hưởng bởi raw lot

## MIXED

Nhiều scope

## 18.3. Checklist Sale Lock

Trường

Bắt buộc

Quy tắc

sale_lock_code

Có

Mã sale lock

lock_scope

Có

## SKU / BATCH / QR / WAREHOUSE / MIXED

target_refs

Có

Danh sách target

lock_reason_code

Có

Lý do

lock_status

Có

## ACTIVE / RELEASED / CANCELLED

lock_source

Có

## RECALL / QA_HOLD / OWNER_DECISION / SYSTEM_GUARD

started_by

Có

Actor/system

started_at

Có

Thời điểm

released_by

Có nếu release

Actor

released_at

Có nếu release

Thời điểm

release_reason

Có nếu release

Lý do mở khóa

evidence_ref

Có

Evidence

downstream_suppression_status

Có

## ACTIVE / WAITING / FAILED / RELEASED

audit_log

Có

Bắt buộc

## 18.4. Sale Lock release

Không được release Sale Lock nếu:

Recall case chưa đóng.

QA/Owner chưa phê duyệt.

Evidence còn missing/rejected.

Trace gap chưa xử lý.

Public notice chưa hoàn tất nếu policy yêu cầu.

Actor không có quyền.

Audit không ghi.

Downstream suppression chưa được cập nhật theo rule.

## 18.5. Fail gate Sale Lock

Fail nếu:

Sale Lock active nhưng Commerce vẫn sellable.

Sale Lock active nhưng AI vẫn tư vấn chốt.

Sale Lock active nhưng Gateway vẫn tạo đơn.

Sale Lock active nhưng Ads vẫn scale sản phẩm.

Sale Lock active nhưng CRM vẫn gửi thúc mua.

Sale Lock active nhưng IVR vẫn xác nhận đơn.

Sale Lock release không có approval/evidence.

Sale Lock không audit.

Sale Lock không linked recall/hold/source reason.

Sale Lock bị MISA hoặc downstream tự mở.

## 19. DOWNSTREAM SUPPRESSION BOUNDARY

## 19.1. Nguyên tắc

Downstream suppression là boundary để các hệ sau không bán/chốt/truyền thông sai sản phẩm bị recall/hold/sale lock.

P2.2E không triển khai toàn bộ downstream, nhưng phải tạo sự thật chặn bán để Phase 3+ tiêu thụ.

## 19.2. Suppression targets

Downstream

Bị chặn khi Sale Lock active

Commerce Runtime

Không sellable, không quote, không cart, không order

AI Advisor

Không tư vấn chốt mua, không tạo order draft

Facebook Gateway

Không kéo chốt mua, không Messenger order flow cho SKU bị lock

Ads Measurement

Không scale, không ROAS promote SKU bị lock

MC AI Live

Không giới thiệu/chốt SKU bị lock

## CRM

Không gửi chăm sóc thúc mua SKU bị lock

## IVR

Không xác nhận đơn chứa SKU/batch/QR bị lock

Website/Landing

Không hiển thị mua ngay nếu lock

Public Trace

Hiển thị warning/bị chặn theo public policy

## 19.3. Suppression output cho phase sau

P2.2E phải cung cấp ít nhất:

lock_scope.

target_refs.

lock_status.

lock_reason.

effective_from.

effective_to nếu có.

downstream_suppression_status.

public_trace_policy.

audit/correlation.

evidence status.

Không được để phase sau tự suy luận Sale Lock từ ghi chú.

## 20. MISA BOUNDARY TRONG TRACE / RECALL / SALE LOCK

## 20.1. Nguyên tắc

MISA không điều khiển traceability, recall hoặc sale lock.

MISA không được:

Tạo recall case.

Đóng recall case.

Tạo sale lock.

Release sale lock.

Sửa QR status.

Sửa public trace.

Sửa trace genealogy.

Sửa batch/warehouse/ledger.

Tự mở bán sản phẩm bị lock.

Ginsengfood Operational Core là nguồn vận hành.

MISA chỉ là kế toán chính thức qua integration layer riêng.

## 20.2. Fail gate MISA boundary

Fail nếu:

MISA sửa QR status.

MISA sửa recall/sale lock.

MISA release hold.

MISA public trace data.

MISA làm mất operational lock.

MISA sync fail làm unlock sản phẩm.

MISA accounting status được dùng làm sellable truth.

PHẦN 4/4 - CROSS-CUTTING / HANDOFF / SMOKE / DONE GATE / FAIL GATE

## 21. ACTOR / PERMISSION / AUDIT / IDEMPOTENCY / EVIDENCE

## 21.1. Command rủi ro trong P2.2E

Command

Actor

Permission

Audit

Idempotency

Evidence

Build Trace Projection

System/Actor

Bắt buộc

Bắt buộc

Bắt buộc

Trace evidence

Generate QR

System/Actor

Bắt buộc

Bắt buộc

Bắt buộc

Payload evidence

Mark QR Printed

Printer callback/System

Bắt buộc

Bắt buộc

Bắt buộc

Print result

Void QR

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Reason/evidence

Create Reprint Request

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Reason/evidence

Approve Reprint

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Approval evidence

Create Recall Case

QA/Owner actor

Bắt buộc

Bắt buộc

Bắt buộc

Recall evidence

Run Recall Impact Analysis

System/QA

Bắt buộc

Bắt buộc

Bắt buộc

Impact evidence

Apply Hold

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Hold evidence

Apply Sale Lock

Authorized actor/System guard

Bắt buộc

Bắt buộc

Bắt buộc

Lock evidence

Release Sale Lock

Authorized actor

Bắt buộc

Bắt buộc

Bắt buộc

Release evidence

Update Downstream Suppression

System

Bắt buộc

Bắt buộc

Bắt buộc

Suppression evidence

## 21.2. Idempotency rule

Case

Expected

Same key + same payload Generate QR

Không tạo QR trùng

Same key + different payload Generate QR

Conflict

Retry print callback same payload

Không đổi trạng thái sai

Retry Void QR same payload

No-op hoặc trả kết quả cũ

Same key different payload Void QR

Conflict

Retry Reprint request

Không tạo nhiều request trùng

Retry Sale Lock same payload

Không tạo lock trùng

Retry Release Sale Lock

Không release nhiều lần gây audit sai

Retry Downstream Suppression

Không tạo trạng thái mâu thuẫn

## 21.3. Evidence rule

Nếu policy yêu cầu evidence accepted, thì:

Evidence SUBMITTED chưa được xem là pass.

Evidence UNDER_REVIEW chưa được xem là pass.

Evidence MISSING phải block.

Evidence REJECTED phải fail/retest.

Evidence ACCEPTED mới được dùng cho gate tương ứng.

Fail nếu evidence SUBMITTED bị dùng như ACCEPTED.

## 22. ENTITY / API / SERVICE BOUNDARY

## 22.1. Entity impact draft

Tài liệu này không tạo migration, nhưng implementation sau này có thể cần rà soát các nhóm entity:

Nhóm entity

Vai trò

trace_genealogy

Internal genealogy

trace_projection

Bản chiếu truy xuất

trace_gap

Ghi nhận thiếu chuỗi trace

qr_code

QR lifecycle

barcode_ref

Barcode reference

print_payload

Payload in

print_job

Job in

reprint_request

Yêu cầu in lại

public_trace_projection

DTO/projection public-safe

recall_case

Case thu hồi

recall_impact_analysis

Phân tích ảnh hưởng

hold_record

Hold

sale_lock

Sale lock

downstream_suppression

Suppression boundary

public_trace_access_log

Log truy xuất public nếu có

evidence_ref

Evidence

audit_log

Audit

idempotency_record

Chống retry trùng

## 22.2. API/service boundary

Nhóm

Mục tiêu

Không được

Trace API

Đọc internal genealogy

Không public internal data

Trace Projection Service

Build projection

Không suy đoán missing link

## QR API

Generate/void/status QR

Không public-valid QR lỗi

Barcode Service

Resolve barcode

Không tự sinh sai config

Print Payload Service

Sinh payload

Không để printer sinh mã

Reprint API

Request/approve/reprint

Không reprint không reason/audit

Public Trace API

Trả whitelist DTO

Không dùng Admin DTO

Recall API

Create/manage recall case

Không chỉ ghi note tự do

Impact Analysis Service

Tìm affected scope

Không bỏ trace gap

Sale Lock API

Apply/release lock

Không unlock không approval

Downstream Suppression Service

Cung cấp lock truth

Không tự triển khai full Commerce/AI

Audit Service

Ghi audit

Không bỏ audit

Idempotency Service

Chống duplicate side effect

Không bỏ qua retry

Evidence Service

Link evidence

Không coi submitted là accepted

## 23. EXECUTION ORDER P2.2E

Khi sau này được phép triển khai, P2.2E phải đi theo thứ tự:

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

Hiểu Material Issue / Raw Lot genealogy

Đọc P2.2D V2.0

Hiểu Batch / Release / Warehouse / Packaging

Đọc P2.2E V1.1 này

Hiểu Trace / QR / Recall / Sale Lock

Analysis code hiện tại

Gap matrix

Owner Confirm nếu thiếu dữ liệu

Owner confirm list

Technical implementation plan

Task breakdown

10

Limited implementation nếu được mở

Code trong scope

11

Smoke P2.2E

Smoke result

12

Evidence submission

Evidence package

13

Report 15 mục

Handoff update

Hiện tại trong cuộc làm tài liệu này:

Chỉ viết lại tài liệu sạch, chưa mở implementation.

## 24. DEV / CODEX / COPILOT HANDOFF

## 24.1. Mode mặc định

Khi giao file này cho Agent:

## MODE: LIMITED IMPLEMENTATION CHỈ KHI ĐÃ CÓ APPROVED ANALYSIS + TECHNICAL DESIGN

Nếu chưa có approval, phải dùng:

## MODE: ANALYSIS ONLY

## 24.2. Prompt đúng cho Agent

MODE hiện tại: ANALYSIS ONLY nếu chưa có Owner/dev lead approval.

Nhiệm vụ:

Lập Gap Matrix giữa tài liệu P2.2E và code hiện tại.

Liệt kê entity/table hiện có hoặc thiếu.

Liệt kê API/DTO/service/UI/worker bị ảnh hưởng.

Liệt kê nơi trace genealogy thiếu Production Dossier nếu có.

Liệt kê nơi QR không linked batch/trace projection nếu có.

Liệt kê nơi QR VOID/FAILED vẫn public-valid nếu có.

Liệt kê nơi Public Trace dùng Admin/Internal DTO nếu có.

Liệt kê private/internal fields có nguy cơ lộ public nếu có.

Liệt kê nơi printer tự sinh QR/barcode/MFG/HSD nếu có.

Liệt kê nơi reprint không reason/permission/audit nếu có.

Liệt kê nơi recall không có impact analysis nếu có.

Liệt kê nơi sale lock không chặn Commerce/AI/Gateway/Ads/Live/CRM/IVR nếu có.

Liệt kê nơi MISA/downstream có thể mutate trace/recall/sale lock nếu có.

Liệt kê risk P0/P1/P2.

Đề xuất implementation scope.

Đề xuất smoke/evidence.

Không sửa code nếu chưa có lệnh Limited Implementation riêng.

Cấm:

Không code trực tiếp từ nguồn tổng hợp.

Không tạo migration khi chưa duyệt.

Không seed dữ liệu khi chưa Owner confirm.

Không public internal trace.

Không dùng Admin DTO cho Public Trace.

Không để QR VOID/FAILED public-valid.

Không để printer tự sinh mã.

Không reprint không audit.

Không để Sale Lock bị downstream bypass.

Không triển khai Commerce/AI/Gateway đầy đủ.

Không gọi Gateway PASS.

Không gọi Production Readiness.

## 25. OWNER CONFIRM REQUIRED

Các điểm cần Owner/dev lead xác nhận trước implementation:

Nhóm

Nội dung cần xác nhận

Public Trace whitelist

Field nào được public

Public Trace wording

Cách hiển thị khi QR hợp lệ/không hợp lệ/recall

Internal/private fields

Field nào tuyệt đối không public

QR code format

Quy tắc mã QR/token/URL

Barcode config

Barcode/GTIN/trade item nếu áp dụng

QR public-valid policy

Điều kiện QR public-valid

Trace gap policy

Missing genealogy thì block hay limited public

Print payload

Field payload cấp 1/cấp 2

Printer status

Status chuẩn

Reprint reason

Danh sách lý do in lại

Reprint approval

Ai được approve

Old label disposition

Xử lý tem/QR cũ

Recall scope

## RAW_LOT/BATCH/SKU/QR/MIXED

Recall severity

## LOW/MEDIUM/HIGH/CRITICAL

Sale Lock scope

## SKU/BATCH/QR/WAREHOUSE/MIXED

Downstream suppression

Channel nào bị chặn và cách tiêu thụ

Sale Lock release

Điều kiện mở khóa

Public notice

Khi nào public trace hiển thị thông báo recall/lock

## RBAC

Ai được tạo QR, void, reprint, recall, lock, unlock

Idempotency

Command nào cần key

Audit

Audit field bắt buộc

Evidence

Evidence nào cần accepted

## 26. STOP CONDITIONS

Dev/Agent phải dừng nếu gặp:

Mã

Điều kiện dừng

Hành động

## P2-2E-STOP-01

Chưa có Production Dossier / Batch / Warehouse dependency

Dừng, quay lại P2.2B/D

## P2-2E-STOP-02

QR không linked batch

Dừng P0

## P2-2E-STOP-03

QR không linked trace projection

Dừng P0

## P2-2E-STOP-04

QR VOID/FAILED public-valid

Dừng P0

## P2-2E-STOP-05

Public Trace dùng Admin/Internal DTO

Dừng P0

## P2-2E-STOP-06

Public Trace lộ private/internal field

Dừng P0

## P2-2E-STOP-07

Trace projection tự suy đoán missing genealogy

Dừng P0

## P2-2E-STOP-08

Máy in tự sinh mã

Dừng P0

## P2-2E-STOP-09

Reprint không reason/permission/audit

Dừng P0

## P2-2E-STOP-10

Recall case không impact analysis

Dừng P0

## P2-2E-STOP-11

Sale Lock active nhưng downstream vẫn bán/chốt

Dừng P0

## P2-2E-STOP-12

Sale Lock release không approval/evidence

Dừng P0

## P2-2E-STOP-13

MISA mutate trace/recall/sale lock

Dừng P0

## P2-2E-STOP-14

Evidence SUBMITTED được coi ACCEPTED

Dừng P0

## P2-2E-STOP-15

Agent chuẩn bị code khi chưa approval

Dừng

## P2-2E-STOP-16

Scope lấn sang Commerce/AI/Gateway đầy đủ

Dừng, chuyển đúng phase

## P2-2E-STOP-17

Có ý định gọi Gateway PASS/Production Readiness

Dừng

## 27. SMOKE REQUIREMENTS P2.2E

Mã smoke

Nội dung

Expected

## SMK-P2-2E-01

Build internal genealogy

Raw lot -> issue -> dossier -> batch -> warehouse

## SMK-P2-2E-02

Missing genealogy link

Trace gap flagged

## SMK-P2-2E-03

Build trace projection hợp lệ

Projection VALID nếu đủ dữ liệu

## SMK-P2-2E-04

Trace projection thiếu dữ liệu

Projection PARTIAL/bị chặn

## SMK-P2-2E-05

Generate QR linked batch

QR được tạo và linked

## SMK-P2-2E-06

QR linked trace projection

Có trace_projection_ref

## SMK-P2-2E-07

## QR VOID

Public Trace không valid

## SMK-P2-2E-08

## QR FAILED

Public Trace không valid

## SMK-P2-2E-09

QR bị chặn

Public Trace không valid

## SMK-P2-2E-10

Print payload cấp 1

Chỉ MFG/HSD

## SMK-P2-2E-11

Print payload cấp 2

Batch/MFG/HSD/barcode/QR

## SMK-P2-2E-12

Printer tự sinh mã

Bị chặn

## SMK-P2-2E-13

Reprint request hợp lệ

Có reason/permission/audit

## SMK-P2-2E-14

Reprint không reason

Bị reject

## SMK-P2-2E-15

Public Trace response

Whitelist-only

## SMK-P2-2E-16

Public Trace private field attempt

Không lộ

## SMK-P2-2E-17

Public Trace dùng Admin DTO

Bị chặn

## SMK-P2-2E-18

Recall Case OPEN

Case được tạo

## SMK-P2-2E-19

Recall Impact Analysis

Xác định impacted batch/QR/SKU

## SMK-P2-2E-20

Recall từ raw lot

Truy ra affected batch/QR nếu có

## SMK-P2-2E-21

Hold active

Target bị chặn theo scope

## SMK-P2-2E-22

Sale Lock active

Target bị suppression

## SMK-P2-2E-23

Sale Lock active downstream

Commerce/AI/Gateway/Ads/Live/CRM/IVR bị chặn ở boundary

## SMK-P2-2E-24

Sale Lock release thiếu approval

Bị reject

## SMK-P2-2E-25

MISA mutate trace/recall/sale lock

Bị chặn

## SMK-P2-2E-26

Evidence required nhưng SUBMITTED

Không pass nếu policy cần ACCEPTED

## SMK-P2-2E-27

Command thiếu actor/permission

## DENY/BLOCK

## SMK-P2-2E-28

Retry same key/payload

Không tạo duplicate side effect

## SMK-P2-2E-29

Same key different payload

Conflict

## 28. EVIDENCE REQUIREMENTS P2.2E

Mã evidence

Nội dung evidence

## EVD-P2-2E-GENEALOGY

Internal genealogy raw -> production -> batch -> warehouse

## EVD-P2-2E-TRACE-GAP

Trace gap detection

## EVD-P2-2E-TRACE-PROJECTION

Trace projection status

## EVD-P2-2E-QR-LINK

QR linked batch/trace

## EVD-P2-2E-QR-VOID-FAILED

QR VOID/FAILED không public-valid

## EVD-P2-2E-BARCODE

Barcode boundary/config

## EVD-P2-2E-PRINT-L1

Print payload cấp 1

## EVD-P2-2E-PRINT-L2

Print payload cấp 2

## EVD-P2-2E-PRINTER-BOUNDARY

Máy in không tự sinh mã

## EVD-P2-2E-REPRINT

Reprint reason/permission/audit

## EVD-P2-2E-PUBLIC-WHITELIST

Public Trace whitelist-only

## EVD-P2-2E-NO-PRIVATE-FIELD

Không lộ private/internal field

## EVD-P2-2E-RECALL-CASE

Recall case

## EVD-P2-2E-RECALL-IMPACT

Recall impact analysis

## EVD-P2-2E-HOLD

Hold target evidence

## EVD-P2-2E-SALE-LOCK

Sale Lock evidence

## EVD-P2-2E-DOWNSTREAM-SUPPRESSION

Downstream suppression boundary

## EVD-P2-2E-MISA-BOUNDARY

MISA không mutate trace/recall/sale lock

## EVD-P2-2E-AUDIT

Actor/permission/audit evidence

## EVD-P2-2E-IDEMPOTENCY

Idempotency evidence

EVD-P2-2E-GATEWAY-bị chặn

Gateway vẫn bị chặn

Evidence Submitted chưa phải Evidence Accepted.

## 29. RISK REGISTER P2.2E

Risk

Mức độ

Nguyên nhân

Kiểm soát

Public Trace lộ dữ liệu nội bộ

## P0

Dùng Admin DTO hoặc không whitelist

Public DTO whitelist-only

QR VOID/FAILED public-valid

## P0

QR guard sai

QR status guard

QR không linked batch/trace

## P0

Trace foundation thiếu

QR link validation

Trace projection suy đoán missing data

## P0

Fail-safe thiếu

Trace gap detection

Máy in tự sinh mã

## P0

Printer integration sai

Payload-only rule

Reprint không kiểm soát

## P0

Thiếu approval/audit

Reprint flow

Recall không impact analysis

## P0

Recall chỉ là note

Impact analysis required

Sale Lock bị downstream bypass

## P0

Suppression boundary thiếu

Sale lock service/output

Sale Lock release không approval

## P0

Unlock sai quyền

Release guard

MISA mutate recall/sale lock

## P0

Boundary sai

MISA no-control

Evidence submitted được pass

## P0

Sai evidence state

Evidence guard

Missing audit

## P1

Command thiếu actor/correlation

Audit required

## 30. REPORT FORMAT 15 MỤC CHO P2.2E

Mọi report P2.2E phải có đủ 15 mục:

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

Nếu thiếu 15 mục, không được chuyển sang P2.2F.

## 31. DONE GATE P2.2E

P2.2E được xem là đạt ở mức handoff/implementation report khi:

Internal genealogy dựng được raw lot -> material issue -> production dossier -> batch -> warehouse.

Trace gap được flag.

Trace projection có status rõ.

QR linked batch.

QR linked trace projection.

QR VOID/FAILED/bị chặn không public-valid.

Barcode bám packaging/trade item config.

Print payload cấp 1 đúng MFG/HSD.

Print payload cấp 2 đúng batch/MFG/HSD/barcode/QR.

Máy in không tự sinh mã.

Reprint có reason/permission/log/audit.

Public Trace dùng DTO riêng.

Public Trace whitelist-only.

Public Trace không lộ private/internal field.

Recall Case có state.

Recall Impact Analysis xác định impacted target.

Hold có scope/status/audit.

Sale Lock có scope/status/audit/evidence.

Sale Lock active tạo downstream suppression boundary.

Commerce/AI/Gateway/Ads/Live/CRM/IVR không được bypass Sale Lock.

Sale Lock release có approval/evidence.

MISA không mutate trace/recall/sale lock.

Actor/permission/audit/idempotency/evidence được kiểm soát.

Có smoke result.

Có evidence package.

Có report 15 mục.

Không lấn sang Commerce/AI/Gateway full.

Gateway vẫn bị chặn.

Không gọi Completion Decision.

Không gọi Production Readiness.

## 32. FAIL GATE P2.2E

P2.2E fail nếu xảy ra một trong các trường hợp:

QR không linked batch.

QR không linked trace projection.

QR VOID vẫn public-valid.

QR FAILED vẫn public-valid.

QR bị chặn vẫn public-valid.

Public Trace dùng Admin/Internal DTO.

Public Trace lộ supplier private detail.

Public Trace lộ formula/BOM/ratio.

Public Trace lộ cost/MISA/accounting data.

Public Trace lộ QC defect nội bộ.

Public Trace lộ evidence private.

Public Trace lộ personnel/admin note/audit/idempotency/correlation.

Trace projection tự suy đoán missing genealogy.

Batch không trace được về Production Dossier.

QR không trace được về Warehouse Receipt nếu policy yêu cầu.

Máy in tự sinh batch/MFG/HSD/barcode/QR.

Print payload FAILED/VOID vẫn public-valid.

Reprint không có reason.

Reprint không có permission.

Reprint không có audit.

Reprint tạo QR mới không linked original.

Recall Case không có impact analysis.

Recall không tạo hold/sale lock khi cần.

Sale Lock active nhưng Commerce vẫn sellable.

Sale Lock active nhưng AI vẫn tư vấn/chốt.

Sale Lock active nhưng Gateway vẫn tạo order flow.

Sale Lock active nhưng Ads/Live/CRM vẫn thúc mua.

Sale Lock active nhưng IVR vẫn xác nhận đơn.

Sale Lock release không có approval/evidence.

MISA mutate trace/recall/sale lock.

Evidence Submitted bị gọi Accepted.

Lấn sang Commerce Runtime đầy đủ.

Lấn sang AI/Gateway runtime đầy đủ.

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

P2.2E - Traceability / QR / Recall / Sale Lock

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
