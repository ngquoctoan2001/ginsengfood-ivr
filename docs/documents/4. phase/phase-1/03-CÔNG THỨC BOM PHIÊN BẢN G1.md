# 03-P1 - CÔNG THỨC BOM PHIÊN BẢN G1

Trạng thái: `FORMULA_G1_BOM_CANONICAL`  
Mode: `LIMITED IMPLEMENTATION HANDOFF`  
Mục tiêu: khóa Recipe/BOM/Formula Version cho 20 SKU, bao gồm toàn bộ công thức G1 từ nguồn cũ.

## 1. Formula baseline

Mỗi SKU phải có:

1. `formula_id` dạng `FML-<SKU_GROUP>-G1`.
2. `formula_version = G1`.
3. `formula_status = APPROVED_BASELINE` hoặc `OWNER_REVIEW_REQUIRED` nếu chưa có approval.
4. `anchor_material = Gạo (lúa - tôm)` khi công thức scale theo gạo.
5. Full line nguyên liệu cụ thể.
6. Output profile mẻ 400 hoặc output profile owner-approved mới.

## 2. Cấu trúc BOM bắt buộc

| Phần | Nội dung | Runtime requirement |
| --- | --- | --- |
| Phần 1 | Quân - Thần - Tá - Sứ | Từng nguyên liệu cụ thể, không dòng gom |
| Phần 2 | Nguyên liệu nền dinh dưỡng | Có ratio/UOM, thường neo theo gạo |
| Phần 3 | Rau củ chiết dịch/nước hầm | Tách variant thuần chay và variant mặn |
| Phần 4 | Nêm và tạo hương vị | Từng material, UOM kg/ml, không gom nhóm |
| B1/B2/B3 | Bao bì/vật tư | Theo packaging profile, không trộn vào formula dinh dưỡng |

## 3. Rule version

1. G0/pilot chỉ dùng thử nghiệm.
2. G1 là baseline vận hành sau owner approval.
3. Thay đổi nguyên liệu, tỷ lệ, UOM, quy cách sơ chế, output profile hoặc packaging profile phải tạo version mới.
4. Đổi số kg gạo trong phạm vi scale cho phép chỉ tạo calculation snapshot, không tạo formula version mới.
5. Formula đã dùng trong production order không được sửa đè.

## 4. Chuẩn hóa conflict công thức vegan

Nguồn G1 hiện có phần nước hầm dùng chung và có dòng động vật. Phase 1 chuẩn hóa chính thức:

1. A1/CS/DM/HS, B1/CS/RM/ĐX, C2/CS/DONGTRUNG và C3/CS/NAMDONGCO đang bị khóa vegan claim vì công thức G1 có `lòng trắng trứng`.
2. Không mặc định bất kỳ SKU G1 nào là `Thuần chay` nếu formula line set còn `lòng trắng trứng`, `xương heo`, thịt/cá/hải sản.
3. Muốn mở lại claim thuần chay phải tạo formula/broth version mới sạch động vật, lưu trace source, có owner approval và effective date.
4. SKU mặn được dùng broth variant mặn nếu source formula yêu cầu.
5. Nếu implementation phát hiện line động vật trong formula đang mang vegan claim, `VeganFormulaGuard = BLOCK`.
6. Không sửa thầm nguồn định lượng; phải tạo normalized formula line set và lưu trace source.

## 5. Production consumer

Phase 2 chỉ consume công thức qua `ProductionBOMSnapshot`, gồm:

1. SKU + formula version.
2. Planned quantity + anchor calculation.
3. Full material lines.
4. Required quantity, UOM, rounding policy.
5. Packaging/output profile.
6. Evidence/owner approval refs.

Production user không được chọn tay nguyên liệu.

## 6. Formula header schema

| Field | Rule |
| --- | --- |
| `formula_id` | `FML-<SKU_SHORT>-G1`, duy nhất theo SKU/version |
| `sku_id` | Trỏ SKU canonical P1-02 |
| `formula_version` | G1 cho nguồn vận hành 20 SKU mới |
| `formula_status` | `OWNER_REVIEW_REQUIRED`, `APPROVED_BASELINE`, `SUPERSEDED`, `BLOCKED` |
| `effective_from/effective_to` | Chỉ set khi owner approve |
| `anchor_material_id` | Gạo (lúa - tôm) nếu scale theo gạo |
| `output_profile_ref` | Mẻ 400 hoặc profile owner-approved |
| `source_document_ref` | `CÔNG THỨC VẬN HÀNH 20 SKU MỚI.MD` và `BỘ KHÓA 5 BƯỚC.md` |
| `owner_approval_ref` | Bắt buộc trước production-active |

## 7. Formula line schema

| Field | Rule |
| --- | --- |
| `formula_line_id` | Duy nhất trong formula version |
| `line_group` | `A1_QTTS`, `A2_BASE`, `A2_BROTH`, `A3_SEASONING`, `B1_PACK`, `B2_PACK`, `B3_AUX` |
| `material_id` | Canonical, không alias |
| `material_name_snapshot` | Lưu tên nguồn để trace |
| `quantity` | Số lượng/mẻ hoặc quantity theo anchor |
| `uom` | kg/lít/ml/gói/hộp/thùng theo allowlist |
| `ratio_to_anchor` | Bắt buộc nếu line scale theo gạo |
| `preprocess_note` | Rửa/ngâm/hấp/sấy/nướng/cắt/thái... từ nguồn |
| `is_required` | true nếu bắt buộc trong BOM |
| `dietary_risk_flag` | true nếu line động vật hoặc ảnh hưởng claim |
| `source_row_ref` | Ref file/section/table nguồn |

## 8. Bộ 20 formula G1 phải có

| Nhóm | SKU | Formula |
| --- | --- | --- |
| A | A1/CS/DM/HS, A2/CS/BASA, A3/CS/CAHOI, A4/CS/LUON, A5/CS/CUU | FML-A1-G1 đến FML-A5-G1 |
| B | B1/CS/RM/ĐX, B2/CS/DHA, B3/CS/CACOM, B4/CS/COLAGEN, B5/CS/SINHLUC, B6/CS/GAAC | FML-B1-G1 đến FML-B6-G1 |
| C | C1/CS/BAONGU, C2/CS/DONGTRUNG, C3/CS/NAMDONGCO, C4/CS/CUABIEN, C5/CS/CANGU, C6/CS/TOM/RONGBIEN, C7/CS/THITGA, C8/CS/THITHEO, C9/CS/THITBO | FML-C1-G1 đến FML-C9-G1 |

## 9. Anchor rice scaling

Nguồn đang dùng mẻ 400 với gạo `(lúa - tôm)` làm anchor 195 kg ở nhiều công thức. Scaling policy:

1. Scale chỉ tạo calculation snapshot, không sửa formula version.
2. Không scale nếu line thiếu ratio/UOM.
3. Không ép lít/ml sang kg nếu thiếu density.
4. Rounding phải lưu trong snapshot.
5. Phase 2 Production BOM Snapshot phải lưu planned quantity + calculated quantity.

## 10. Nhóm công thức

| Phần | Line group | Runtime use |
| --- | --- | --- |
| Quân - Thần - Tá - Sứ | A1_QTTS | Dược liệu/đặc thù SKU |
| Nguyên liệu nền dinh dưỡng | A2_BASE | Gạo, rau củ, nền |
| Rau củ chiết dịch/nước hầm | A2_BROTH | Broth/dietary guard |
| Nêm và tạo hương vị | A3_SEASONING | Nêm vị/hương |
| Bao bì/vật tư | B1/B2/B3 | Packaging profile, không trộn vào nutrition formula |

## 11. Deviation và versioning

| Thay đổi | Cách xử lý |
| --- | --- |
| Đổi nguyên liệu | Formula version mới |
| Đổi tỷ lệ/hàm lượng | Formula version mới |
| Đổi UOM/conversion | Owner approval + version nếu ảnh hưởng quantity |
| Đổi sơ chế ảnh hưởng chất lượng | Formula version mới hoặc process profile version |
| Đổi output packaging | Packaging profile version |
| Chỉ đổi planned batch size theo anchor | Calculation snapshot, không đổi G1 |

## 12. Snapshot handoff cho Phase 2

P1-03 phải cung cấp cho P2-03:

1. Header formula.
2. Full line formula.
3. Material canonical refs.
4. UOM và conversion.
5. Ratio/anchor.
6. Packaging/output profile.
7. Dietary claim guard result.
8. Source refs và owner approval refs.
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

## PHỤ LỤC NGUỒN ĐÃ NHẬP - CÔNG THỨC VẬN HÀNH 20 SKU MỚI.MD

> Phụ lục này giữ nguyên văn nguồn cũ để không mất bảng, số liệu, field hoặc rule khi bỏ file nguồn riêng. Phần rewrite chuẩn hóa ở đầu file là nguồn chính; phụ lục dùng để trace và đối chiếu số liệu.

**NHÓM A - CHÁO SÂM THEO MÙA**

**A1/CS/DM/HS - CHÁO SÂM - DIÊM MẠCH & HẠT SEN**

**Thông tin đầu công thức**

**SKU:** A1/CS/DM/HS  
**Tên sản phẩm:** Cháo Sâm - Diêm mạch & Hạt sen  
**Nhóm sản phẩm:** Cháo sâm theo mùa  
**Phân loại:** Không thuần chay (vegan claim khóa do có lòng trắng trứng trong công thức)  
**Mã công thức:** FML-A1-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Diêm mạch       | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 3       | Hạt sen         | 9.00                  | kg         | 4.62        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 5       | Bạch linh       | 5.00                  | kg         | 2.56        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 6       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 7       | Gừng            | 0.70                  | kg         | 0.36        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT**    | **Nguyên liệu**                              | **Mẻ/400**         | **Đơn vị**    | **%/195 kg gạo nền** | **Ghi chú**                              |
| ---------- | -------------------------------------------- | ------------------ | ------------- | -------------------- | ---------------------------------------- |
| 1          | Táo tàu                                      | 4.40               | kg            | 2.26                 | Rửa sạch để ráo                          |
| 2          | Carot                                        | 49.20              | kg            | 25.23                | Rửa sạch Thái nhuyễn                     |
| 3          | Củ cải trắng                                 | 49.20              | kg            | 25.23                | Rửa sạch Thái nhuyễn                     |
| 4          | Rong biển                                    | 7.00               | kg            | 3.59                 | Rửa sạch để ráo                          |
| 5          | Hành tây                                     | 21.30              | kg            | 10.92                | Rửa sạch nướng thái nhuyễn               |
| 6          | Cần tây                                      | 21.30              | kg            | 10.92                | Rửa sạch Thái nhuyễn                     |
| 7          | Lá thơm                                      | 1.32               | kg            | 0.68                 | Rửa sạch Thái nhuyễn                     |
| 8<br><br>9 | Nước dừa nguyên chất<br><br>Lòng trắng trứng | 73.30<br><br>55.00 | Lít<br><br>kg | 39.13<br><br>28.21   | Làm nền nước hầm<br><br>Làm nền nước hầm |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5<br><br>6 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**A2/CS/BASA - CHÁO SÂM - CÁ BASA**

**Thông tin đầu công thức**

**SKU:** A2/CS/BASA  
**Tên sản phẩm:** Cháo Sâm - Cá Basa  
**Nhóm sản phẩm:** Cháo sâm theo mùa  
**Phân loại:** Mặn  
**Mã công thức:** FML-A2-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Cá Basa         | 22.00                 | kg         | 11.28       | Rửa sạch Ép lấy thịt ướp gia vị xào thơm                                                           |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Bạch linh       | 9.00                  | kg         | 4.62        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 5       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 6       | Gừng            | 0.40                  | kg         | 0.21        | Rửa sạch nướng xay nhuyễn                                                                          |
| 7       | Trần bì         | 0.40                  | kg         | 0.21        | Rửa sạch                                                                                           |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT**              | **Nguyên liệu**                                                      | **Mẻ/400**                      | **Đơn vị**              | **%/195 kg gạo nền**            | **Ghi chú**                                                      |
| -------------------- | -------------------------------------------------------------------- | ------------------------------- | ----------------------- | ------------------------------- | ---------------------------------------------------------------- |
| 1                    | Táo tàu                                                              | 4.40                            | kg                      | 2.26                            | Rửa sạch để ráo                                                  |
| 2                    | Carot                                                                | 49.20                           | kg                      | 25.23                           | Rửa sạch Thái nhuyễn                                             |
| 3                    | Củ cải trắng thái khúc                                               | 49.20                           | kg                      | 25.23                           | Rửa sạch Thái nhuyễn                                             |
| 4                    | Rong biển                                                            | 7.00                            | kg                      | 3.59                            | Chiết dịch nước hầm                                              |
| 5                    | Hành tây chẻ 4                                                       | 21.30                           | kg                      | 10.92                           | Rửa sạch nướng thái nhuyễn                                       |
| 6                    | Cần tây                                                              | 21.30                           | kg                      | 10.92                           | Rửa sạch Thái nhuyễn                                             |
|                      |                                                                      |                                 |                         |                                 |                                                                  |
| 7                    | Lá thơm                                                              | 1.32                            | kg                      | 0.68                            | Rửa sạch Thái nhuyễn                                             |
|                      |                                                                      |                                 |                         |                                 |                                                                  |
| 8<br><br>9<br><br>10 | Nước dừa nguyên chất<br><br>Xương heo xay vỡ<br><br>Lòng trắng trứng | 55.00<br><br>55.00<br><br>21.30 | Lít<br><br>kg<br><br>kg | 28.21<br><br>28.21<br><br>10.92 | Làm nền nước hầm<br><br>Làm nền nước hầm<br><br>Làm nền nước hầm |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**A3/CS/CAHOI - CHÁO SÂM - CÁ HỒI**

**Thông tin đầu công thức**

**SKU:** A3/CS/CAHOI  
**Tên sản phẩm:** Cháo Sâm - Cá hồi  
**Nhóm sản phẩm:** Cháo sâm theo mùa  
**Phân loại:** Mặn  
**Mã công thức:** FML-A3-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Cá hồi          | 15.50                 | kg         | 7.95        | Rửa sạch Ép lấy thịt ướp gia vị xào thơm                                                           |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | **Mẻ/400** | **Đơn vị** | | | | | | | | **%/195 kg gạo nền** | | | | | | **Ghi chú** | | | | | | | | |
| --- | | --- | --- | --- | | | | | | | | --- | | | | | | --- | | | | | | | | |
| 1 | | Táo tàu | | | | 4.40 | | | | | | | | kg | | | | | 2.26 | | | | Rửa sạch để ráo | | | | | |
| 2 | | Carot | | | | 49.20 | kg | | | | | | | 25.23 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | | 49.20 | kg | | | | | | | 25.23 | | | | | | | | Rửa sạch Thái nhuyễn | | | | | |
| 4 | | Rong biển | | | | 7.00 | | kg | | | | | | | | 3.59 | | | | | | | | Rửa sạch để ráo | | | | | |
| 5 | | Hành tây chẻ 4 | | | | 21.30 | | | kg | | | | | | | | 10.92 | | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | |
| 6 | | Cần tây | | | 21.30 | | | | | | kg | | | | 10.92 | | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | |
| 7 | Lá thơm | | | | 1.32 | | | | | kg | | | | 0.68 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**A4/CS/LUON - CHÁO SÂM - LƯƠN ĐỒNG**

**Thông tin đầu công thức**

**SKU:** A4/CS/LUON  
**Tên sản phẩm:** Cháo Sâm - Lươn đồng  
**Nhóm sản phẩm:** Cháo sâm theo mùa  
**Phân loại:** Mặn  
**Mã công thức:** FML-A4-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Lươn đồng       | 15.50                 | kg         | 7.95        | Rửa sạch Ép lấy thịt ướp gia vị xào thơm                                                           |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**A5/CS/CUU - CHÁO SÂM - THỊT CỪU & TÁO TÀU**

**Thông tin đầu công thức**

**SKU:** A5/CS/CUU  
**Tên sản phẩm:** Cháo Sâm - Thịt cừu & Táo tàu  
**Nhóm sản phẩm:** Cháo sâm theo mùa  
**Phân loại:** Mặn  
**Mã công thức:** FML-A5-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Thịt cừu        | 15.50                 | kg         | 7.95        | Rửa sạch xay nhuyễn ướp gia vị xào thơm                                                            |
| 3       | Táo tàu         | 6.00                  | kg         | 3.08        | Rửa sạch                                                                                           |
| 4       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 5       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 6       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |
| 7       | Quế chi         | 0.25                  | kg         | 0.13        | Rửa sạch đập vỡ                                                                                    |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | | **Mẻ/400** | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | | --- | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | 4.40 | | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | 49.20 | | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | 49.20 | | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | 7.00 | | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | 21.30 | | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**NHÓM B - CHÁO SÂM CHỨC NĂNG**

**B1/CS/RM/ĐX - CHÁO SÂM - RAU MÁ & ĐẬU XANH**

**Thông tin đầu công thức**

**SKU:** B1/CS/RM/ĐX  
**Tên sản phẩm:** Cháo Sâm - Rau má & Đậu xanh  
**Nhóm sản phẩm:** Cháo sâm chức năng  
**Phân loại:** Không thuần chay (vegan claim khóa do có lòng trắng trứng trong công thức)  
**Mã công thức:** FML-B1-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | ----------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin       | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Rau má            | 15.50                 | kg         | 7.95        | Rửa sạch xay nhuyển lọc lấy nước                                                                   |
| 3       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Hoài sơn          | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 5       | Kỷ tử             | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 6       | Gừng              | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**              |
| ------- | --------------- | --------------------- | ---------- | ----------- | --------------------------------- |
| 1       | Gạo (lúa - tôm) | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng         |
| 2       | Cà rốt          | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu             |
| 3       | Bí đỏ           | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu             |
| 4       | Nấm kim châm    | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | **Nguyên liệu** | **Mẻ/400** | **Đơn vị** | **%/195 kg gạo nền** | **Ghi chú** | |
| --- | --- | --- | --- | --- | --- | |
| 1 | Táo tàu | 4.40 | kg | 2.26 | Rửa sạch để ráo |
| 2 | Carot | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 3 | Củ cải trắng | 49.20 | kg | 25.23 | Rửa sạch Thái nhuyễn |
| 4 | Rong biển | 7.00 | kg | 3.59 | Rửa sạch để ráo |
| 5 | Hành tây | 21.30 | kg | 10.92 | Rửa sạch nướng thái nhuyễn |
| 6 | Cần tây | 21.30 | kg | 10.92 | Rửa sạch Thái nhuyễn |
| 7 | Lá thơm | 1.32 | kg | 0.68 | Rửa sạch Thái nhuyễn |
| 8<br><br>9 | Nước dừa nguyên chất<br><br>Lòng trắng trứng | 73.30<br><br>55.00 | Lít<br><br>kg | 39.13<br><br>28.21 | Làm nền nước hầm<br><br>Làm nền nước hầm |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**B2/CS/DHA - CHÁO SÂM - DHA NÃO BỘ**

**Thông tin đầu công thức**

**SKU:** B2/CS/DHA  
**Tên sản phẩm:** Cháo Sâm - DHA Não bộ  
**Nhóm sản phẩm:** Cháo sâm chức năng  
**Phân loại:** Mặn  
**Mã công thức:** FML-B2-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Cá hồi          | 15.50                 | kg         | 7.95        | Xay vừa ướp xào thơm                                                                               |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Vừng không võ   | 6.00                  | kg         | 3.08        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 5       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 6       | Gừng            | 0.40                  | kg         | 0.21        | Rửa sạch nướng xay nhuyễn                                                                          |
| 7       | Trần bì         | 0.40                  | kg         | 0.21        | Rửa sạch                                                                                           |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm )  | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**B3/CS/CACOM - CHÁO SÂM - CÁ CƠM & VỪNG**

**Thông tin đầu công thức**

**SKU:** B3/CS/CACOM  
**Tên sản phẩm:** Cháo Sâm - Cá cơm & Vừng  
**Nhóm sản phẩm:** Cháo sâm chức năng  
**Phân loại:** Mặn  
**Mã công thức:** FML-B3-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Cá cơm          | 15.50                 | kg         | 7.95        | Rửa sạch xay nhuyễnt ướp gia vị xào thơm                                                           |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Vừng            | 6.00                  | kg         | 3.08        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 5       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 6       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | Rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**B4/CS/COLAGEN - CHÁO SÂM - THỊT HEO & DA HEO**

**Thông tin đầu công thức**

**SKU:** B4/CS/COLAGEN  
**Tên sản phẩm:** Cháo Sâm - Thịt heo & Da heo  
**Nhóm sản phẩm:** Cháo sâm chức năng  
**Phân loại:** Mặn  
**Mã công thức:** FML-B4-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Thịt heo nạc    | 10.50                 | kg         | 5.38        | Rửa sạch xay nhuyễn ướp gia vị xào thơm                                                            |
| 3       | Da heo          | 5.00                  | kg         | 2.56        | Rửa sạch say nhuyện cùng da heo                                                                    |
| 4       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 5       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 6       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**B5/CS/SINHLUC - CHÁO SÂM - HÀU BIỂN**

**Thông tin đầu công thức**

**SKU:** B5/CS/SINHLUC  
**Tên sản phẩm:** Cháo Sâm - Hàu biển  
**Nhóm sản phẩm:** Cháo sâm chức năng  
**Phân loại:** Mặn  
**Mã công thức:** FML-B5-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Hàu biển        | 15.50                 | kg         | 7.95        | Rửa sạch để ráo                                                                                    |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Trần bì         | 0.80                  | kg         | 0.41        | Rửa sạch                                                                                           |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**      |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------- |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | Rửa sạch để ráo rang vàng |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | Nền dinh dưỡng            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | Nền dinh dưỡng            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Nền dinh dưỡng            |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Nền dinh dưỡng            |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**B6/CS/GAAC - CHÁO SÂM - GÀ ÁC**

**Thông tin đầu công thức**

**SKU:** B6/CS/GAAC  
**Tên sản phẩm:** Cháo Sâm - Gà ác  
**Nhóm sản phẩm:** Cháo sâm chức năng  
**Phân loại:** Mặn  
**Mã công thức:** FML-B6-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Gà ác           | 15.50                 | kg         | 7.95        | Xay nhuyển ướp gia vị xào thơm                                                                     |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**NHÓM C - CHÁO SÂM BỔ DƯỠNG**

**C1/CS/BAONGU - CHÁO SÂM - BÀO NGƯ**

**Thông tin đầu công thức**

**SKU:** C1/CS/BAONGU  
**Tên sản phẩm:** Cháo Sâm - Bào ngư  
**Nhóm sản phẩm:** Cháo sâm bổ dưỡng  
**Phân loại:** Mặn  
**Mã công thức:** FML-C1-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu  
**Ghi chú version:** Kế thừa công thức gốc G0, bổ sung trường phân loại theo rule toàn hệ, viết lại theo phân nhóm vận hành để xuất phiếu rõ thao tác, và bổ sung **mì chính 1.9 kg**.

**Cơ sở tính tỷ lệ:** 195 kg gạo nền = 100%

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu**                                | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ % / 195 kg gạo nền** | **Ghi chú vận hành**                                                                               |
| ------- | ---------------------------------------------- | --------------------- | ---------- | ---------------------------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin - _Pouzolzia zeylanica_ (L.) Benn. | 9.00                  | kg         | 4.62                         | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Bào ngư - _Haliotis spp._                      | 15.50                 | kg         | 7.95                         | Rửa sạch thái lát ướp gia vị sào thơm                                                              |
| 3       | Hoài sơn - _Dioscorea opposita_                | 15.50                 | kg         | 7.95                         | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử - _Lycium barbarum_                      | 3.00                  | kg         | 1.54                         | Rửa sạch                                                                                           |
| 5       | Gừng nướng - _Zingiber officinale_             | 0.80                  | kg         | 0.41                         | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ % / 195 kg gạo nền** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ---------------------------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00                       | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95                        | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92                        | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13                        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49                         | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | **Số lượng / mẻ 400** | | | | | | | | **Đơn vị** | | **Tỷ lệ % / 195 kg gạo nền** | | | | | | | | | | | | | **Ghi chú vận hành** |
| --- | | --- | --- | | | | | | | | --- | | --- | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | kg | | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | kg | | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | kg | | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | kg | | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | | 21.30 | | | | | | kg | | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | | 1.32 | | | | | kg | | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | Lít<br><br>Kg<br><br>kg | | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | | | | **Đơn vị** | **Tỷ lệ % / 195 kg gạo nền** | | **Ghi chú vận hành** | |
| --- | --- | --- | | | | --- | --- | | --- | |
| 1 | Muối biển rang | 12.84 | | | | kg | 6.58 | | Nêm vị | |
| 2 | Tiêu đen rang | 1.32 | | | | kg | 0.68 | | Tạo hương vị | |
| 3 | Tỏi nướng | 0.72 | | | | kg | 0.37 | | Tạo hương vị | |
| 4 | Hành lá thái khúc | 2.19 | | | | kg | 1.12 | | Tạo hương vị | |
| 5 | Rễ cần tây thái nhuyễn | 1.25 | | | | kg | 0.64 | | Tạo hương vị | |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5 | | | Tạo vị umami<br><br>Bù hương sâm | |
| | | | | | | | | | | |

**C2/CS/DONGTRUNG - CHÁO SÂM - ĐÔNG TRÙNG HẠ THẢO**

**Thông tin đầu công thức**

**SKU:** C2/CS/DONGTRUNG  
**Tên sản phẩm:** Cháo Sâm - Đông trùng hạ thảo  
**Nhóm sản phẩm:** Cháo sâm bổ dưỡng  
**Phân loại:** Không thuần chay (vegan claim khóa do có lòng trắng trứng trong công thức)  
**Mã công thức:** FML-C2-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu**    | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | ------------------ | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin        | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Đông trùng hạ thảo | 15.50                 | kg         | 7.95        | Xay nhuyễn                                                                                         |
| 3       | Hoài sơn           | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử              | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Gừng nướng         | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT**    | **Nguyên liệu**                              | **Mẻ/400**         | **Đơn vị**    | **%/195 kg gạo nền** | **Ghi chú**                              |
| ---------- | -------------------------------------------- | ------------------ | ------------- | -------------------- | ---------------------------------------- |
| 1          | Táo tàu                                      | 4.40               | kg            | 2.26                 | Rửa sạch để ráo                          |
| 2          | Carot                                        | 49.20              | kg            | 25.23                | Rửa sạch Thái nhuyễn                     |
| 3          | Củ cải trắng                                 | 49.20              | kg            | 25.23                | Rửa sạch Thái nhuyễn                     |
| 4          | Rong biển                                    | 7.00               | kg            | 3.59                 | Rửa sạch để ráo                          |
| 5          | Hành tây                                     | 21.30              | kg            | 10.92                | Rửa sạch nướng thái nhuyễn               |
| 6          | Cần tây                                      | 21.30              | kg            | 10.92                | Rửa sạch Thái nhuyễn                     |
| 7          | Lá thơm                                      | 1.32               | kg            | 0.68                 | Rửa sạch Thái nhuyễn                     |
| 8<br><br>9 | Nước dừa nguyên chất<br><br>Lòng trắng trứng | 73.30<br><br>55.00 | Lít<br><br>kg | 39.13<br><br>28.21   | Làm nền nước hầm<br><br>Làm nền nước hầm |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**C3/CS/NAMDONGCO - CHÁO SÂM - NẤM ĐÔNG CÔ**

**Thông tin đầu công thức**

**SKU:** C3/CS/NAMDONGCO  
**Tên sản phẩm:** Cháo Sâm - Nấm đông cô  
**Nhóm sản phẩm:** Cháo sâm bổ dưỡng  
**Phân loại:** Không thuần chay (vegan claim khóa do có lòng trắng trứng trong công thức)  
**Mã công thức:** FML-C3-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Nấm đông cô     | 15.50                 | kg         | 7.95        | Rửa sạch thái lát ướp gia vị sào thơm                                                              |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT**    | **Nguyên liệu**                              | **Mẻ/400**         | **Đơn vị**    | **%/195 kg gạo nền** | **Ghi chú**                              |
| ---------- | -------------------------------------------- | ------------------ | ------------- | -------------------- | ---------------------------------------- |
| 1          | Táo tàu                                      | 4.40               | kg            | 2.26                 | Rửa sạch để ráo                          |
| 2          | Carot                                        | 49.20              | kg            | 25.23                | Rửa sạch Thái nhuyễn                     |
| 3          | Củ cải trắng                                 | 49.20              | kg            | 25.23                | Rửa sạch Thái nhuyễn                     |
| 4          | Rong biển                                    | 7.00               | kg            | 3.59                 | Rửa sạch để ráo                          |
| 5          | Hành tây                                     | 21.30              | kg            | 10.92                | Rửa sạch nướng thái nhuyễn               |
| 6          | Cần tây                                      | 21.30              | kg            | 10.92                | Rửa sạch Thái nhuyễn                     |
| 7          | Lá thơm                                      | 1.32               | kg            | 0.68                 | Rửa sạch Thái nhuyễn                     |
| 8<br><br>9 | Nước dừa nguyên chất<br><br>Lòng trắng trứng | 73.30<br><br>55.00 | Lít<br><br>kg | 39.13<br><br>28.21   | Làm nền nước hầm<br><br>Làm nền nước hầm |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**C4/CS/CUABIEN - CHÁO SÂM - CUA BIỂN**

**Thông tin đầu công thức**

**SKU:** C4/CS/CUABIEN  
**Tên sản phẩm:** Cháo Sâm - Cua biển  
**Nhóm sản phẩm:** Cháo sâm bổ dưỡng  
**Phân loại:** Mặn  
**Mã công thức:** FML-C4-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Cua biển        | 15.50                 | kg         | 7.95        | Rửa sạch ướp gia vị xào thơm                                                                       |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**C5/CS/CANGU - CHÁO SÂM - CÁ NGỪ**

**Thông tin đầu công thức**

**SKU:** C5/CS/CANGU  
**Tên sản phẩm:** Cháo Sâm - Cá ngừ  
**Nhóm sản phẩm:** Cháo sâm bổ dưỡng  
**Phân loại:** Mặn  
**Mã công thức:** FML-C5-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Cá ngừ          | 15.50                 | kg         | 7.95        | Ép lấy thịch ướp gia vị xào thơm                                                                   |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Bạch linh       | 5.00                  | kg         | 2.56        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 5       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 6       | Táo tàu         | 4.40                  | kg         | 2.26        | Rửa sạch                                                                                           |
| 7       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | | **Mẻ/400** | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | **Ghi chú** | | | | | | | |
| --- | | --- | | | --- | --- | | | | | | --- | | | | | | | --- | | | | | | | |
| 1 | | Táo tàu | | 4.40 | | | | | | | | | | kg | | | | 2.26 | | | | | Rửa sạch để ráo | | | | | |
| 2 | | Carot | | 49.20 | | | kg | | | | | | | 25.23 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | 49.20 | | | kg | | | | | | | 25.23 | | | | | | | | Rửa sạch Thái nhuyễn | | | | | |
| 4 | | Rong biển | | 7.00 | | | | kg | | | | | | | | 3.59 | | | | | | | | Rửa sạch để ráo | | | | | |
| 5 | | Hành tây chẻ 4 | | 21.30 | | | | | kg | | | | | | | | 10.92 | | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**C6/CS/TOM/RONGBIEN - CHÁO SÂM - TÔM & RONG BIỂN**

**Thông tin đầu công thức**

**SKU:** C6/CS/TOM/RONGBIEN  
**Tên sản phẩm:** Cháo Sâm - Tôm & Rong biển  
**Nhóm sản phẩm:** Cháo sâm bổ dưỡng  
**Phân loại:** Mặn  
**Mã công thức:** FML-C6-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Tôm             | 15.50                 | kg         | 7.95        | Xay nhuyện ướp gia vị xào thơm                                                                     |
| 3       | Rong biển       | 15.50                 | kg         | 7.95        | Rừa sạch                                                                                           |
| 4       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 5       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 6       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | | **Nguyên liệu** | | | **Mẻ/400** | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | | --- | | | --- | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**C7/CS/THITGA - CHÁO SÂM - THỊT GÀ**

**Thông tin đầu công thức**

**SKU:** C7/CS/THITGA  
**Tên sản phẩm:** Cháo Sâm - Thịt gà  
**Nhóm sản phẩm:** Cháo sâm bổ dưỡng  
**Phân loại:** Mặn  
**Mã công thức:** FML-C7-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Thịt gà         | 15.50                 | kg         | 7.95        | Hấp đánh tơi                                                                                       |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**C8/CS/THITHEO - CHÁO SÂM - THỊT HEO**

**Thông tin đầu công thức**

**SKU:** C8/CS/THITHEO  
**Tên sản phẩm:** Cháo Sâm - Thịt heo  
**Nhóm sản phẩm:** Cháo sâm bổ dưỡng  
**Phân loại:** Mặn  
**Mã công thức:** FML-C8-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Thịt heo        | 15.50                 | kg         | 7.95        | Xoay nhuyện ướp gia vị xào thơm                                                                    |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- | --- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |     |
|            |                           |                |              |                      |                                  |

**C9/CS/THITBO - CHÁO SÂM - THỊT BÒ**

**Thông tin đầu công thức**

**SKU:** C9/CS/THITBO  
**Tên sản phẩm:** Cháo Sâm - Thịt bò  
**Nhóm sản phẩm:** Cháo sâm bổ dưỡng  
**Phân loại:** Mặn  
**Mã công thức:** FML-C9-G1  
**Version:** G1  
**Trạng thái:** Công thức cập nhật vận hành / sản xuất mẫu

**Phần 1 - Công thức: Quần - Thần - Tá - Sứ**

| **STT** | **Nguyên liệu** | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                                                                               |
| ------- | --------------- | --------------------- | ---------- | ----------- | -------------------------------------------------------------------------------------------------- |
| 1       | Sâm Savigin     | 9.00                  | kg         | 4.62        | Rửa sạch Thái lác mỏng 1mm, hấp nhiệt 120 độ 10 phút, ủ nhiệt độ 55 độ c , độ ẩm 65% thời gian 24h |
| 2       | Thịt bò         | 15.50                 | kg         | 7.95        | Xay nhuyện ướp gia vị xào thơm                                                                     |
| 3       | Hoài sơn        | 15.50                 | kg         | 7.95        | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút                                                   |
| 4       | Kỷ tử           | 3.00                  | kg         | 1.54        | Rửa sạch                                                                                           |
| 5       | Gừng            | 0.80                  | kg         | 0.41        | Rửa sạch nướng xay nhuyễn                                                                          |

**Phần 2 - Nguyên liệu nền dinh dưỡng**

| **STT** | **Nguyên liệu**   | **Số lượng / mẻ 400** | **Đơn vị** | **Tỷ lệ %** | **Ghi chú vận hành**                             |
| ------- | ----------------- | --------------------- | ---------- | ----------- | ------------------------------------------------ |
| 1       | Gạo (lúa - tôm)   | 195.00                | kg         | 100.00      | rửa sạch để ráo rang vàng                        |
| 2       | Cà rốt            | 35.00                 | kg         | 17.95       | rửa sạch thái hạt lựu                            |
| 3       | Bí đỏ             | 33.00                 | kg         | 16.92       | rửa sạch thái hạt lựu                            |
| 4       | Đậu xanh không vỏ | 29.50                 | kg         | 15.13       | Rửa sạch ngâm 2h, hấp nhiệt độ 120 trong 30 phút |
| 5       | Nấm kim châm      | 18.50                 | kg         | 9.49        | Rửa sạch để ráo cắt khúc 20-30 mm                |

**Phần 3 - Rau củ chiết dịch tạo nước hầm (dùng chung)**

| **STT** | | **Nguyên liệu** | | **Mẻ/400** | | **Đơn vị** | | | | | | **%/195 kg gạo nền** | | | | | | | | | | | | | | **Ghi chú** |
| --- | | --- | | --- | | --- | | | | | | --- | | | | | | | | | | | | | | --- |
| 1 | | Táo tàu | | | 4.40 | | | | | | | | | kg | | | | 2.26 | | | | Rửa sạch để ráo | | | | | | |
| 2 | | Carot | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | |
| 3 | | Củ cải trắng thái khúc | | | 49.20 | | kg | | | | | | | 25.23 | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 4 | | Rong biển | | | 7.00 | | | kg | | | | | | | | 3.59 | | | | | | | Rửa sạch để ráo | | | | | | |
| 5 | | Hành tây chẻ 4 | | | 21.30 | | | | kg | | | | | | | | 10.92 | | | | | | | Rửa sạch nướng thái nhuyễn | | | | | | |
| 6 | | Cần tây | 21.30 | | | | | | | | kg | | | | 10.92 | | | | | | | | | | Rửa sạch Thái nhuyễn | | | | | | |
| 7 | Lá thơm | | 1.32 | | | | | | | kg | | | | 0.68 | | | | | | Rửa sạch Thái nhuyễn | | | | | | | | | | | | | |
| 8<br><br>9<br><br>10 | | Nước dừa nguyên chất<br><br>Lòng trắng trứng<br><br>Xương heo xay vỡ | | | 55.00<br><br>21.30<br><br>55.00 | | | | | | | | Lít<br><br>Kg<br><br>kg | 28.21<br><br>10.92<br><br>28.21 | | | | | Nền dịch hầm<br><br>Nền dịch hầm<br><br>Nền dịch hầm | | | | | | | | | | | | | |

**Phần 4 - Nguyên liệu nêm & tạo hương vị (dùng chung)**

| **STT**    | **Nguyên liệu**           | **Mẻ/400**     | **Đơn vị**   | **%/195 kg gạo nền** | **Ghi chú**                      |
| ---------- | ------------------------- | -------------- | ------------ | -------------------- | -------------------------------- |
| 1          | Muối biển rang            | 12.84          | kg           | 6.58                 | Nêm vị                           |
| 2          | Tiêu đen rang             | 1.32           | kg           | 0.68                 | Tạo hương vị                     |
| 3          | Tỏi nướng                 | 0.72           | kg           | 0.37                 | Tạo hương vị                     |
| 4          | Hành lá thái khúc         | 2.19           | kg           | 1.12                 | Tạo hương vị                     |
| 5          | Rễ cần tây thái nhuyễn    | 1.25           | kg           | 0.64                 | Tạo hương vị                     |
| 6<br><br>7 | Mì chính<br><br>Hương sâm | 1.90<br><br>10 | Kg<br><br>ml | 0.97<br><br>0.000.5  | Tạo vị umami<br><br>Bù hương sâm |

---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 03-P1-2B-RECIPE-BOM-FORMULA-VERSION.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P1.2B - RECIPE BOM FORMULA VERSION

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-02 Recipe / BOM / Formula Version.
- PACK-03 Demand / MRP / Procurement / Material Control.
- TECH-02 Formula and BOM boundary.
- Recipe Active khong dong nghia batch RELEASED.

## Noi Dung Rewrite

## 03-P1-2B-RECIPE-BOM-FORMULA-VERSION

## TÀI LIỆU ĐẶC TẢ RECIPE / BOM / FORMULA VERSION CHO PHASE 1

Global Gate Status: bị chặn
Completion Status: WAITING IMPLEMENTATION EVIDENCE
Production Readiness: KHONG
Go-live Decision: KHONG
Default Agent Mode: ANALYSIS ONLY

## PHẦN 1/4 - PHASE MARKER / PURPOSE / SOURCE-OF-TRUTH / SCOPE

## 1. PHASE MARKER

Tài liệu này thuộc nhóm:

## PHASE-01-P1-2B-RECIPE-BOM-FORMULA-VERSION

Đây là file đặc tả lõi cho miền:

Recipe.

## BOM.

Formula.

Formula Version.

G1 Operational Formula.

Anchor gạo.

Ratio_to_rice.

Formula scaling.

Formula snapshot requirement cho Phase 2.

Quy tắc không sửa công thức đã khóa.

Quy tắc tạo version mới khi thay đổi công thức.

Quy tắc phân biệt công thức sản xuất và buffer planning.

Tài liệu này thay thế toàn bộ nội dung cũ của:

## 03-P1-2B-RECIPE-BOM-FORMULA-VERSION.docx

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## 03-P1-2B-RECIPE-BOM-FORMULA-VERSION

Phiên bản

Phase

Phase 1

Module

Recipe / BOM / Formula Version

Loại tài liệu

Đặc tả nghiệp vụ - kỹ thuật, chưa phải code

Nguồn chính

Phase 1 clean documentation

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

File điều phối trước

## README-PHASE-01-HANDOFF-INDEX V1.1

Checklist liên quan

## MASTER-DATA-NORMALIZATION-CHECKLIST-PHASE-01-02

Cho phép code ngay?

Không

Cho phép seed ngay?

Không

Cho phép tạo migration ngay?

Không

Mode mặc định cho Agent

## ANALYSIS ONLY

Gateway Status

bị chặn

Production Readiness

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để khóa toàn bộ logic Recipe/BOM/Formula cho Phase 1 theo đúng nghiệp vụ đã được mở rộng từ nguồn tổng hợp.

Mục tiêu chính:

Chuẩn hóa cách định nghĩa công thức sản xuất cho 20 SKU baseline.

Khóa công thức G1 là công thức vận hành nền cho go-live sau này, nhưng chưa tự gọi Production Readiness.

Khóa nguyên tắc gạo là anchor ingredient trong công thức pilot/G1.

Khóa nguyên tắc khi nhập số kg gạo, hệ thống tự scale toàn bộ nguyên liệu theo ratio_to_rice.

Khóa nguyên tắc scale kg gạo không phải là đổi công thức.

Khóa nguyên tắc nếu đổi tỷ lệ, thêm nguyên liệu, bỏ nguyên liệu sau khi G1 đã khóa thì bắt buộc tạo Formula Version mới.

Khóa việc công thức phải bung chi tiết từng nguyên liệu cụ thể, không để nhóm mơ hồ.

Khóa UOM và conversion là điều kiện bắt buộc trước khi seed formula.

Khóa buffer +5% nhóm A và +7% nhóm B chỉ là buffer planning, không được tự sửa công thức sản xuất đã phê duyệt.

Khóa việc Phase 2 Production Order phải tiêu thụ công thức bằng snapshot, không chọn tay nguyên liệu.

Khóa smoke/evidence cần có để chứng minh Recipe/BOM/Formula đúng trước khi giao implementation.

## 4. SOURCE-OF-TRUTH - NGUỒN SỰ THẬT

Tầng

Nguồn

Vai trò

Được dùng như thế nào

Không được dùng để làm gì

Tầng 0

MASTER Governance

Quy tắc quản trị cao nhất

Giữ nguyên tắc no-bypass, no-parallel-source, no-PASS-without-evidence

Không ghi đè bằng công thức tự suy luận

Tầng 1

## README-PHASE-01-HANDOFF-INDEX V1.1

Điều phối Phase 1

Biết phạm vi và gate của Phase 1

Không thay thế file P1.2B chi tiết

Tầng 2

Tài liệu P1.2B V2.0 này

Nguồn chính cho Recipe/BOM/Formula

Giao dev/Agent phân tích miền công thức

Không tự cho phép code/seed

Tầng 3

## MASTER-DATA-NORMALIZATION-CHECKLIST

Kiểm soát dữ liệu trước seed/code

Chốt SKU, ingredient, UOM, formula, packaging cần Owner confirm

Không thay thế Owner confirmation

Tầng 4

nguồn tổng hợp TÀI LIỆU.docx

Nguồn phụ trợ nghiệp vụ

Dùng để xác định rule cần đưa vào tài liệu sạch

Không code/seed trực tiếp

Tầng 5

Code hiện tại

Hiện trạng triển khai

Dùng để gap analysis

Không là source-of-truth nếu conflict với tài liệu này

## 5. SCOPE IN - PHẠM VI BAO GỒM

Tài liệu này bao gồm:

Recipe master.

Recipe header.

Recipe line group.

Recipe line.

BOM sản xuất.

Formula header.

Formula line.

Formula version.

Formula kind.

G1 operational formula.

G0/research boundary.

G2/G3 future version boundary.

Anchor ingredient là gạo.

Ratio_to_rice.

Formula scaling theo kg gạo.

Formula snapshot requirement.

Formula approval status.

Formula activation status.

Formula change control.

UOM dùng trong công thức.

UOM conversion requirement.

Rounding policy.

Buffer planning +5% nhóm A.

Buffer planning +7% nhóm B.

No manual ingredient selection trong Production Order và Material Request của Phase 2.

Seed requirement cho formula.

Smoke/evidence cho formula.

## 6. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM

Tài liệu này không bao gồm:

Không triển khai code.

Không tạo migration.

Không tạo seed script.

Không thiết kế API chi tiết.

Không thiết kế DTO chi tiết.

Không thiết kế UI chi tiết.

Không triển khai Production Order thật.

Không triển khai Material Issue thật.

Không triển khai Warehouse Ledger thật.

Không triển khai MRP/procurement đầy đủ.

Không triển khai MISA integration.

Không triển khai print/QR/traceability runtime.

Không quyết định SKU sellable.

Không quyết định batch release.

Không quyết định Production Readiness.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Go-live Decision.

Tài liệu này chỉ khóa nền Recipe/BOM/Formula để các file sau tiêu thụ đúng.

## 7. ĐỊNH NGHĨA THUẬT NGỮ

Thuật ngữ

Định nghĩa

Recipe

Công thức sản phẩm ở cấp nghiệp vụ, gắn với SKU

## BOM

Định mức nguyên liệu/vật tư cần cho sản xuất, được rút ra từ công thức

Formula

Phiên bản công thức có thể được duyệt và dùng vận hành

Formula Version

Mã phiên bản của công thức, ví dụ G1, G2

## G0

Công thức nghiên cứu/thử nghiệm, không được dùng làm operational formula

## G1

Công thức vận hành nền được khóa cho baseline/go-live sau này

## G2/G3

Phiên bản công thức tương lai khi có thay đổi sau G1

Anchor Ingredient

Nguyên liệu mốc dùng để scale công thức; trong nguồn tổng hợp khóa là gạo

Ratio_to_rice

Tỷ lệ của từng nguyên liệu so với lượng gạo anchor

Formula Scaling

Tính lại lượng nguyên liệu khi thay đổi kg gạo

Formula Change

Thay đổi tỷ lệ/thêm/bỏ/thay nguyên liệu làm phát sinh version mới

Planning Buffer

Hệ số dự phòng cho MRP/stock planning, không phải công thức sản xuất

Formula Snapshot

Bản chụp công thức tại thời điểm tạo lệnh sản xuất

Active Operational

Trạng thái công thức được phép tiêu thụ cho vận hành theo gate

Owner Confirm

Xác nhận của chủ dự án/chủ nghiệp vụ trước seed/code

## PHẦN 2/4 - RECIPE / BOM / FORMULA MODEL / ANCHOR GẠO / SCALING

## 8. NGUYÊN TẮC TỔNG QUAN RECIPE / BOM / FORMULA

## 8.1. Recipe phải gắn với SKU

Mỗi recipe vận hành phải thuộc về một SKU cụ thể.

Không cho phép recipe trôi nổi không có SKU.

Không cho phép một recipe dùng cho nhiều SKU nếu chưa có rule chia sẻ được Owner xác nhận.

## 8.2. Formula phải có version

Mọi công thức dùng cho vận hành phải có:

Formula code.

Formula version.

SKU code.

Formula status.

Approved status.

Active status.

Effective date.

Formula lines.

Owner confirmation.

Không có version thì không được dùng sản xuất.

Không có approved status thì không được active operational.

Không có formula lines cụ thể thì không được seed.

## 8.3. Recipe line phải là nguyên liệu cụ thể

Công thức không được chỉ ghi:

Nhóm đạm.

Nhóm rau củ.

Nhóm gia vị.

Nhóm bổ dưỡng.

Nhóm bao bì.

Nhóm nguyên liệu phụ.

Mỗi dòng công thức phải có:

Ingredient code.

Ingredient canonical name.

## UOM.

Ratio hoặc quantity.

Formula line group.

Status.

Owner confirmation.

Recipe line group chỉ dùng để tổ chức công thức, không được thay thế nguyên liệu cụ thể.

## 8.4. BOM là đầu ra từ formula, không phải nhập tay độc lập

BOM sản xuất phải được sinh hoặc xác định từ formula đã khóa.

Không cho phép dev tạo một danh sách BOM độc lập, tách rời formula.

Không cho phép người dùng tự nhập tay BOM ở bước sản xuất nếu công thức đã có.

BOM dùng cho Phase 2 phải đến từ formula snapshot.

## 9. CẤU TRÚC RECIPE HEADER

Recipe header là lớp định danh công thức sản phẩm ở cấp SKU.

Trường nghiệp vụ

Bắt buộc

Quy tắc

recipe_code

Có

Mã recipe duy nhất

sku_code

Có

SKU sở hữu recipe

sku_name_snapshot

Có

Snapshot tên SKU tại thời điểm tạo recipe

recipe_name

Có

Tên recipe nội bộ

recipe_purpose

Có

Baseline / Pilot / Operational / Research

recipe_status

Có

## DRAFT / OWNER_REVIEW / APPROVED / ACTIVE / INACTIVE

formula_kind

Có

ANCHOR_BASED / FIXED_BATCH hoặc loại được Owner khóa

default_formula_version

Có nếu active

Version mặc định khi tạo production order

owner_confirmed

Có

Bắt buộc trước seed/code

created_by

Có

Actor tạo

approved_by

Có nếu approved

Actor duyệt

approved_at

Có nếu approved

Thời điểm duyệt

effective_from

Có nếu active

Ngày hiệu lực

effective_to

Tùy

Ngày hết hiệu lực nếu thay version

audit_required

Có

Bắt buộc

## 10. CẤU TRÚC FORMULA HEADER

Formula header là lớp khóa phiên bản công thức.

Trường nghiệp vụ

Bắt buộc

Quy tắc

formula_code

Có

Mã công thức duy nhất

recipe_code

Có

Trỏ về recipe

sku_code

Có

SKU áp dụng

formula_version

Có

G1/G2/G3 hoặc version được khóa

formula_name

Có

Tên công thức

formula_kind

Có

ANCHOR_BASED hoặc FIXED_BATCH

formula_status

Có

## DRAFT / OWNER_REVIEW / APPROVED_SEED_BASELINE / ACTIVE_OPERATIONAL / SUPERSEDED / INACTIVE

is_active_operational

Có

TRUE duy nhất theo rule active

anchor_ingredient_code

Có với ANCHOR_BASED

Gạo anchor

anchor_uom

Có với ANCHOR_BASED

Đơn vị của gạo anchor

standard_anchor_quantity

Có

Ví dụ kg gạo chuẩn nếu Owner xác nhận

standard_batch_size

Có nếu FIXED_BATCH

Mẻ chuẩn nếu dùng fixed batch

rounding_policy

Có

Quy tắc làm tròn khi scale

loss_buffer_policy_ref

Tùy

Chỉ trỏ planning buffer, không sửa formula

approved_by

Có khi approved

Người duyệt

approved_at

Có khi approved

Thời điểm duyệt

effective_from

Có khi active

Ngày hiệu lực

change_reason

Có nếu version mới

Lý do thay đổi

previous_formula_version

Có nếu G2+

Version trước đó

owner_confirmed

Có

Bắt buộc trước seed/code

audit_required

Có

Bắt buộc

## 11. CẤU TRÚC FORMULA LINE

Formula line là từng dòng nguyên liệu cụ thể.

Trường nghiệp vụ

Bắt buộc

Quy tắc

formula_code

Có

Trỏ về formula header

formula_version

Có

Version cụ thể

line_no

Có

Thứ tự dòng, không trùng

line_group_code

Có

Nhóm dòng công thức

ingredient_code

Có

Nguyên liệu cụ thể

ingredient_name_snapshot

Có

Snapshot tên nguyên liệu

ingredient_source_type_snapshot

Có

Supplier / Company Source / Internal

material_group_snapshot

Có

Nhóm A/B/Company Source

formula_uom

Có

Đơn vị trong công thức

inventory_uom_snapshot

Có

Đơn vị tồn kho tương ứng

uom_conversion_required

Có

TRUE nếu cần quy đổi

ratio_to_rice

Có với ANCHOR_BASED

Tỷ lệ so với gạo

quantity_at_standard_anchor

Có nếu có anchor chuẩn

Số lượng tại mức gạo chuẩn

quantity_per_standard_batch

Có nếu FIXED_BATCH

Số lượng theo mẻ chuẩn

is_anchor_ingredient

Có

TRUE duy nhất với gạo

is_active_line

Có

TRUE nếu dòng còn hiệu lực

is_optional

Có

G1 operational không nên optional nếu chưa có rule

owner_confirmed

Có

Bắt buộc trước seed

audit_required

Có

Bắt buộc

## 12. RECIPE LINE GROUP

## 12.1. Mục tiêu của line group

Recipe line group dùng để phân nhóm nguyên liệu cho dễ quản trị, kiểm tra và hiển thị.

Line group không phải là nguyên liệu.

Line group không được thay thế ingredient code.

Mỗi dòng công thức vẫn phải có ingredient cụ thể.

## 12.2. Quy tắc line group

Quy tắc

Diễn giải

Mỗi công thức G1 phải có đủ nhóm bắt buộc theo seed spec đã khóa

Không thiếu nhóm nếu rule yêu cầu 4 nhóm

Group chỉ dùng tổ chức

Không dùng group làm dòng nguyên liệu

Ingredient phải nằm trong group hợp lệ

Không để ingredient không group nếu rule yêu cầu

Group code phải từ master/config

Không nhập tay tự do

Group không quyết định UOM

UOM nằm ở ingredient/formula line

Group không quyết định quantity

Quantity/ratio nằm ở formula line

## 12.3. Fail gate line group

Fail nếu:

Công thức chỉ có group nhưng không có ingredient.

Một group chứa mô tả chung chung không thể issue nguyên liệu.

Formula line không có ingredient_code.

Ingredient không thuộc group nào trong khi G1 yêu cầu đủ nhóm.

Dev hardcode group trong service thay vì dùng master/config.

## 13. FORMULA KIND

## 13.1. Các loại formula được chấp nhận trong tài liệu này

Formula kind

Ý nghĩa

Dùng khi nào

Điều kiện bắt buộc

## ANCHOR_BASED

Công thức scale theo nguyên liệu mốc

nguồn tổng hợp khóa gạo làm anchor

Có anchor gạo và ratio_to_rice

## FIXED_BATCH

Công thức theo mẻ chuẩn cố định

Dùng nếu Owner khóa batch size cụ thể

Có quantity_per_standard_batch

## RESEARCH_ONLY

Công thức nghiên cứu

Không dùng vận hành

Không được active operational

## 13.2. Công thức G1 ưu tiên ANCHOR_BASED theo nguồn tổng hợp

nguồn tổng hợp đã khóa:

Pilot formula lấy gạo làm anchor ingredient.

Khi nhập số kg gạo, hệ thống tự scale toàn bộ nguyên liệu theo ratio_to_rice.

Scale số kg gạo không phải đổi công thức.

Vì vậy, trong Phase 1, file này khóa hướng G1 như sau:

G1 operational formula phải hỗ trợ anchor-based scaling theo gạo.

Nếu một SKU có công thức fixed batch riêng, phải được Owner xác nhận rõ và không được làm mờ rule anchor gạo.

## 14. ANCHOR GẠO

## 14.1. Nguyên tắc

Gạo là nguyên liệu mốc để scale công thức.

Khi người dùng nhập số kg gạo trong bối cảnh sản xuất, hệ thống phải tự tính ra lượng các nguyên liệu còn lại theo ratio_to_rice.

Gạo anchor phải là một ingredient canonical trong master.

Không được dùng chữ “gạo” dạng text tự do nếu chưa có ingredient_code.

## 14.2. Checklist anchor gạo

Trường

Bắt buộc

Quy tắc

anchor_ingredient_code

Có

Mã ingredient của gạo

anchor_ingredient_name_snapshot

Có

Tên gạo tại thời điểm formula approved

anchor_uom

Có

Đơn vị tính, ví dụ kg nếu Owner xác nhận

is_anchor_ingredient

Có

TRUE duy nhất trong formula

anchor_precision

Có

Số lẻ cho phép

anchor_min_quantity

Tùy

Nếu có giới hạn vận hành

anchor_max_quantity

Tùy

Nếu có giới hạn vận hành

owner_confirmed

Có

Bắt buộc

## 14.3. Fail gate anchor

Fail nếu:

Formula G1 không có anchor.

Có nhiều hơn một anchor trong cùng formula.

Anchor không phải ingredient canonical.

Anchor thiếu UOM.

Anchor không phải gạo nhưng không có Owner confirm.

Anchor bị nhập tay ở Production Order thay vì lấy từ formula.

## 15. RATIO_TO_RICE

## 15.1. Định nghĩa

ratio_to_rice là tỷ lệ của từng nguyên liệu so với lượng gạo anchor.

Khi gạo thay đổi, lượng nguyên liệu khác được tính theo tỷ lệ này.

Ví dụ về nguyên tắc, không phải dữ liệu thật:

Nếu gạo là 10 kg và nguyên liệu X có ratio_to_rice = 0,05 thì nguyên liệu X được tính theo tỷ lệ 5% so với gạo.

Tài liệu này không khóa con số thật. Con số thật phải do Owner xác nhận trong checklist chuẩn hóa.

## 15.2. Quy tắc bắt buộc

Quy tắc

Diễn giải

Mỗi dòng nguyên liệu cần scale phải có ratio_to_rice

Không để null

Ratio_to_rice phải theo UOM đã chuẩn hóa

Không dùng UOM mơ hồ

Ratio_to_rice thuộc formula version

Version khác có thể có ratio khác

Ratio_to_rice không được sửa trong G1 active

Nếu sửa phải tạo version mới

Ratio_to_rice phải Owner confirm

Agent không được tự suy luận

Ratio_to_rice phải test scaling

Smoke bắt buộc

## 15.3. Fail gate ratio

Fail nếu:

Thiếu ratio_to_rice.

Ratio_to_rice âm.

Ratio_to_rice không phù hợp UOM.

Ratio_to_rice tự suy luận từ text nguồn tổng hợp.

Sửa ratio_to_rice trong G1 mà không tạo version mới.

Công thức scale được dù thiếu ratio.

## 16. FORMULA SCALING THEO KG GẠO

## 16.1. Nguyên tắc chính

Khi nhập số kg gạo:

Hệ thống lấy formula version đang active.

Hệ thống lấy anchor gạo.

Hệ thống đọc ratio_to_rice của từng dòng nguyên liệu.

Hệ thống tính quantity cần dùng cho từng nguyên liệu.

Hệ thống áp dụng rounding policy.

Hệ thống tạo danh sách scaled formula lines.

Danh sách này được Phase 2 dùng để tạo production dossier/material request.

## 16.2. Scaling không làm thay đổi formula version

Thay đổi số kg gạo chỉ thay đổi sản lượng/mẻ sản xuất, không làm thay đổi công thức.

Các hành động sau không tạo formula version mới:

Hành động

Có tạo version mới không?

Ghi chú

Nhập 10 kg gạo thay vì 5 kg

Không

Chỉ scale sản lượng

Nhập 20 kg gạo theo cùng ratio

Không

Chỉ scale

Làm tròn theo rounding policy đã khóa

Không

Nếu policy không đổi

Tính nhu cầu nguyên liệu cho production order

Không

Dùng snapshot

Tính nhu cầu planning có buffer

Không

Buffer thuộc planning, không phải formula

## 16.3. Các hành động bắt buộc tạo version mới

Hành động

Có tạo version mới không?

Lý do

Đổi tỷ lệ giữa các nguyên liệu

Có

Thay đổi công thức

Đổi ratio_to_rice

Có

Thay đổi công thức

Thêm nguyên liệu

Có

Thay đổi công thức

Bỏ nguyên liệu

Có

Thay đổi công thức

Thay nguyên liệu này bằng nguyên liệu khác

Có

Thay đổi công thức

Đổi anchor ingredient

Có

Thay đổi logic scale

Đổi UOM làm thay đổi định lượng thực tế

Có

Ảnh hưởng công thức

Đổi nhóm công thức làm thay đổi line bắt buộc

Có

Ảnh hưởng formula structure

## 16.4. Rounding policy

Formula scaling phải có rounding policy.

Nội dung

Quy tắc

Rounding policy phải được khóa ở formula header hoặc config liên quan

Không để dev tự làm tròn tùy ý

Rounding phải nhất quán giữa preview, production order và material request

Không được mỗi nơi một kết quả

Rounding không được làm thay đổi ratio gốc

Chỉ là biểu diễn/tính lượng dùng

Nếu rounding policy thay đổi làm ảnh hưởng sản xuất

Cần Owner review

## 17. UOM TRONG FORMULA

## 17.1. Nguyên tắc

Mỗi formula line phải có UOM.

Không được có dòng công thức thiếu đơn vị tính.

Không được dùng UOM text tự do.

UOM phải đến từ master.

Nếu formula_uom khác inventory_uom, phải có conversion rule.

## 17.2. Nhóm UOM cần hỗ trợ

Nhóm

Ví dụ

Dùng cho

Khối lượng

kg, g

Gạo, nguyên liệu khô, thành phần sản xuất

Thể tích

l, ml

Nguyên liệu lỏng nếu có

Đơn vị đếm

pcs, gói

Vật tư hoặc đơn vị đặc biệt

Đóng gói

hộp, thùng, carton

Packaging profile, không thay dòng nguyên liệu sản xuất nếu chưa xác nhận

## 17.3. Fail gate UOM

Fail nếu:

Formula line thiếu UOM.

UOM không nằm trong master.

UOM không có conversion khi cần.

Cùng ingredient trong công thức dùng nhiều UOM không có rule.

Ratio_to_rice dùng UOM không quy đổi được.

Dev hardcode UOM trong code.

## PHẦN 3/4 - G1 LOCK / VERSIONING / BUFFER / SNAPSHOT / PHASE 2 CONSUMPTION

## 18. G1 OPERATIONAL FORMULA LOCK

## 18.1. Định nghĩa G1

G1 là công thức vận hành nền được Owner xác nhận để làm baseline cho sản xuất sau này.

G1 không phải công thức nghiên cứu.

G1 không được sửa tùy tiện sau khi đã khóa.

G1 là nguồn để Phase 2 Production Order tự hiện công thức và tạo formula snapshot.

## 18.2. Điều kiện để một formula được xem là G1 hợp lệ

Điều kiện

Bắt buộc

Có sku_code

Có

Có formula_code

Có

Có formula_version = G1 hoặc version được Owner khóa tương ứng

Có

Có formula_status hợp lệ

Có

Có anchor gạo nếu anchor-based

Có

Có ratio_to_rice cho từng line cần scale

Có

Có formula lines cụ thể

Có

Có UOM cho từng line

Có

Có ingredient canonical

Có

Có Owner confirm

Có

Có approved_by / approved_at nếu active operational

Có

Có audit trail

Có

## 18.3. G0 / Research boundary

G0 hoặc công thức nghiên cứu không được dùng làm operational formula.

Không được:

Seed G0 thành active operational.

Dùng G0 để tạo production order.

Dùng G0 để tạo material request.

Dùng G0 để mở bán.

Dùng G0 để tính giá thành chính thức.

Nếu cần lưu G0, phải ở trạng thái research/draft, không được tiêu thụ bởi production.

## 18.4. G2/G3 future version

G2/G3 chỉ phát sinh khi có thay đổi sau G1.

Mỗi version mới phải có:

Version code.

Previous version.

Change reason.

Change summary.

Changed lines.

Owner approval.

Effective date.

Audit.

Rule không ảnh hưởng hồ sơ sản xuất cũ.

## 19. FORMULA VERSIONING RULES

## 19.1. Nguyên tắc versioning

Formula versioning bảo vệ lịch sử sản xuất.

Khi một production order đã snapshot G1, sau này dù G2 được tạo, hồ sơ sản xuất cũ vẫn phải giữ G1 snapshot.

Không được cập nhật ngược công thức cũ làm thay đổi lịch sử.

## 19.2. Những thay đổi không tạo version mới

Thay đổi

Có tạo version mới không?

Ghi chú

Thay đổi số kg gạo để scale sản lượng

Không

Chỉ là quantity input

Tính nhu cầu cho mẻ lớn/mẻ nhỏ

Không

Dựa cùng ratio

Làm tròn theo policy đã có

Không

Không đổi policy

Tính planning buffer

Không

Không phải formula

Đổi trạng thái draft chưa approved trong quá trình soạn

Không bắt buộc

Tùy workflow, miễn chưa active

Sửa lỗi chính tả mô tả không ảnh hưởng công thức

Không

Cần audit nếu đã approved

## 19.3. Những thay đổi bắt buộc tạo version mới

Thay đổi

Có tạo version mới không?

Ghi chú

Thay đổi ratio_to_rice

Có

Thay đổi bản chất công thức

Thêm nguyên liệu

Có

Thay đổi BOM

Bỏ nguyên liệu

Có

Thay đổi BOM

Thay nguyên liệu

Có

Thay đổi BOM/trace

Đổi anchor gạo sang anchor khác

Có

Thay đổi logic công thức

Đổi UOM làm thay đổi định lượng

Có

Ảnh hưởng sản xuất

Đổi quantity_per_batch trong fixed batch

Có

Thay đổi mẻ chuẩn

Đổi nhóm dòng bắt buộc ảnh hưởng nguyên liệu

Có

Thay đổi cấu trúc công thức

Thay đổi quy trình làm thay đổi nguyên liệu sử dụng

Có nếu ảnh hưởng formula

Owner confirm

## 20. BUFFER +5% / +7% KHÔNG ĐƯỢC SỬA FORMULA

## 20.1. Quy tắc từ nguồn tổng hợp

nguồn tổng hợp đã khóa:

Buffer +5% Nhóm A.

Buffer +7% Nhóm B.

Buffer là buffer cho MRP/stock planning.

Buffer không được tự sửa công thức sản xuất đã phê duyệt.

## 20.2. Phân biệt formula và planning

Hạng mục

Formula sản xuất

Planning buffer

Mục đích

Định mức công thức đã duyệt

Dự phòng mua hàng/tồn kho

Có nằm trong G1 formula không?

Có, nếu là nguyên liệu thật

Không

Có làm thay đổi recipe line không?

Có nếu thay đổi nguyên liệu/tỷ lệ

Không

Có dùng để issue nguyên liệu mặc định không?

Theo production/material rule

Không tự động nếu chưa duyệt

Có tạo formula version mới không?

Có nếu đổi formula

Không nếu chỉ đổi planning policy

Ai xác nhận

Owner công thức

Owner planning/MRP

## 20.3. Fail gate buffer

Fail nếu:

Dev cộng +5% vào formula line nhóm A.

Dev cộng +7% vào formula line nhóm B.

Buffer làm thay đổi G1 quantity.

Buffer làm thay đổi ratio_to_rice.

Material request dùng buffer như công thức sản xuất khi chưa có rule phê duyệt.

Agent hiểu buffer là thành phần công thức.

## 21. FORMULA SNAPSHOT CHO PHASE 2

## 21.1. Mục tiêu snapshot

Phase 2 sẽ tạo Production Order và Production Dossier.

Khi tạo lệnh sản xuất, hệ thống phải snapshot công thức tại thời điểm đó.

Snapshot bảo vệ lịch sử:

## SKU.

Formula code.

Formula version.

Ingredient list.

Ratio_to_rice.

## UOM.

Quantity đã scale.

Rounding.

Owner approval metadata.

## 21.2. Dữ liệu snapshot tối thiểu

Trường snapshot

Bắt buộc

Ghi chú

sku_code

Có

SKU sản xuất

sku_name_snapshot

Có

Tên tại thời điểm sản xuất

formula_code

Có

Mã công thức

formula_version

Có

Version đang dùng

formula_status_snapshot

Có

Trạng thái tại thời điểm snapshot

anchor_ingredient_code

Có

Gạo anchor

anchor_quantity

Có

Số kg gạo nhập

formula_lines_snapshot

Có

Toàn bộ dòng công thức

ratio_to_rice_snapshot

Có

Tỷ lệ tại thời điểm snapshot

uom_snapshot

Có

Đơn vị tại thời điểm snapshot

scaled_quantity

Có

Số lượng đã tính

rounding_policy_snapshot

Có

Quy tắc làm tròn

approved_by_snapshot

Có nếu active

Người duyệt công thức

approved_at_snapshot

Có nếu active

Thời điểm duyệt

snapshot_created_at

Có

Thời điểm tạo snapshot

snapshot_actor

Có

Người/hệ thống tạo snapshot

## 21.3. Phase 2 không được sửa snapshot ngược

Sau khi Production Order đã tạo snapshot:

Không được sửa snapshot theo version mới.

Không được tự động đổi G1 snapshot sang G2.

Không được thay nguyên liệu trong snapshot nếu không có version/workflow đặc biệt.

Không được để Material Request chọn tay nguyên liệu khác snapshot.

Không được để Material Issue cấp nguyên liệu ngoài snapshot nếu không có approved deviation.

## 22. KHONG MANUAL INGREDIENT SELECTION - KHÔNG CHỌN TAY NGUYÊN LIỆU Ở SẢN XUẤT

## 22.1. Nguyên tắc

nguồn tổng hợp đã khóa:

Không được cho người dùng chọn tay nguyên liệu ở lệnh sản xuất.

Không được cho người dùng chọn tay nguyên liệu ở phiếu cấp nguyên liệu.

Phiếu đề nghị cấp nguyên liệu phải tự hiện nguyên liệu theo công thức.

Phiếu chấp thuận nguyên liệu đưa vào sản xuất phải dựa trên từ phiếu đề nghị.

Phase 1 phải chuẩn bị formula đủ rõ để Phase 2 thực hiện được rule này.

## 22.2. Điều kiện để Phase 2 không cần chọn tay nguyên liệu

Phase 1 phải cung cấp:

SKU có formula active.

Formula có version.

Formula có line cụ thể.

Formula line có ingredient code.

Formula line có UOM.

Formula line có ratio_to_rice hoặc quantity.

Formula có snapshot-ready structure.

Formula không dùng nhóm mơ hồ.

Ingredient canonical đã chuẩn hóa.

UOM conversion đã chuẩn hóa.

Nếu Phase 1 thiếu các dữ liệu này, Phase 2 sẽ buộc phải cho chọn tay, gây sai governance.

## 23. SEED REQUIREMENT CHO FORMULA

## 23.1. Điều kiện trước khi seed formula

Không được seed formula nếu chưa có:

SKU canonical.

Ingredient canonical.

Ingredient alias mapping.

UOM master.

UOM conversion.

Recipe line group.

Formula header.

Formula lines.

Anchor gạo.

Ratio_to_rice.

Formula version.

Formula status.

Owner confirmation.

Seed idempotency rule.

## 23.2. Seed không được làm

Seed formula không được:

Tự set SKU sellable.

Tự set SKU released.

Tự set SKU production-ready.

Tự tạo production order.

Tự tạo raw lot.

Tự tạo material issue.

Tự mở inventory.

Tự tạo MISA mapping thật.

Tự tạo print payload.

Tự gọi Gateway PASS.

PHẦN 4/4 - EXECUTION ORDER / HANDOFF / SMOKE / DONE GATE / FAIL GATE

## 24. EXECUTION ORDER - THỨ TỰ XỬ LÝ FILE P1.2B

## 24.1. Thứ tự đọc tài liệu

Thứ tự

Tài liệu

Mục đích

## README-PHASE-01-HANDOFF-INDEX V1.1

Hiểu Phase 1 và boundary

## MASTER-DATA-NORMALIZATION-CHECKLIST

Biết dữ liệu cần chuẩn hóa

## 02-P1-2A-PRODUCT-SKU-INGREDIENT-UOM

Đối chiếu SKU/Ingredient/UOM

## 03-P1-2B-RECIPE-BOM-FORMULA-VERSION V2.0

Đọc file formula này

## 05-P1-2D-SKU-EXTENSION-SEED-GOVERNANCE

Đối chiếu seed formula sau khi viết lại

## 06-P1-2E-SMOKE-EVIDENCE-REPORT

Đối chiếu smoke/evidence sau khi viết lại

## 24.2. Thứ tự phân tích trước code

Khi giao cho dev/Codex/Copilot, phải theo thứ tự:

Documentation Reading.

Analysis Only.

Gap Matrix.

Owner Confirm Required.

Technical Design Only.

Limited Implementation, chỉ khi có lệnh riêng.

Smoke/Evidence Submission.

Owner Review.

## 25. DEV / CODEX / COPILOT HANDOFF

## 25.1. Mode mặc định

Khi giao file này cho Agent, mode mặc định là:

## ANALYSIS ONLY

Agent không được code ngay.

## 25.2. Prompt giao Agent đúng

Nội dung giao việc đúng:

Nhiệm vụ:

Lập Gap Matrix giữa tài liệu này và code hiện tại.

Liệt kê dữ liệu formula còn thiếu.

Liệt kê SKU/ingredient/UOM nào cần Owner confirm.

Liệt kê impacted files.

Liệt kê impacted database/migration nếu có.

Liệt kê impacted API/DTO/service/UI/worker nếu có.

Liệt kê risk P0/P1/P2.

Đề xuất implementation scope.

Đề xuất smoke/evidence.

Không sửa code.

Cấm:

Không tạo migration.

Không tạo seed.

Không sửa code.

Không tự suy luận formula.

Không tự suy luận ratio_to_rice.

Không tự hardcode UOM.

Không dùng nguồn tổng hợp để code trực tiếp.

Không gọi Completion Decision / Gateway PASS / Production Readiness.

## 25.3. Output Agent bắt buộc

Mục

Nội dung

Scope đã đọc

File, phase, module, mode

Rule P0

Formula version, anchor gạo, ratio_to_rice, no dirty seed

Boundary

No code, no seed, no manual ingredient

Gap Matrix

Rule tài liệu vs code hiện tại

Impacted Files

File có thể bị ảnh hưởng

DB/Migration Impact

Bảng/object có thể bị ảnh hưởng nếu có

API/DTO/Service/UI/Worker Impact

Layer có thể bị ảnh hưởng

Owner Confirm Required

Dữ liệu còn thiếu/mơ hồ

Risk Register

## P0/P1/P2

Proposed Implementation Scope

Đề xuất phạm vi sửa sau này

Proposed Smoke/Evidence

Test/evidence cần nộp

## 26. OWNER CONFIRM REQUIRED

Các dữ liệu sau bắt buộc Owner xác nhận trước khi seed/code:

Nhóm

Dữ liệu cần xác nhận

Vì sao chặn

## SKU

SKU nào có G1 formula

Tránh gắn nhầm công thức

Formula code

Mã công thức từng SKU

Tránh trùng/nhầm công thức

Formula version

G1/G2 hoặc version chính thức

Tránh sản xuất từ version mơ hồ

Formula status

Draft/Approved/Active

Tránh dùng công thức chưa duyệt

Anchor gạo

Ingredient code của gạo

Tránh scale sai

Anchor UOM

kg/g hoặc đơn vị khác

Tránh sai quantity

Ratio_to_rice

Tỷ lệ từng nguyên liệu

Là lõi tính nguyên liệu

Ingredient lines

Danh sách nguyên liệu cụ thể

Tránh công thức nhóm mơ hồ

## UOM

Đơn vị từng dòng

Tránh sai quy đổi

Rounding

Cách làm tròn

Tránh lệch nguyên liệu

Buffer policy

+5%/+7% chỉ planning

Tránh sửa formula

Version change rule

Khi nào tạo version mới

Tránh sửa G1 tại chỗ

Approval actor

Người duyệt công thức

Tránh active không có trách nhiệm

Effective date

Ngày hiệu lực

Tránh dùng nhầm version

## 27. SMOKE REQUIREMENTS CHO P1.2B

Chi tiết smoke sẽ được đưa vào file P1.2E, nhưng P1.2B phải khóa các smoke tối thiểu sau:

Mã smoke

Nội dung

## SMK-P1-2B-01

Formula G1 có formula_code, sku_code, version, status

## SMK-P1-2B-02

Mỗi G1 có đúng một anchor gạo

## SMK-P1-2B-03

Anchor gạo là ingredient canonical

## SMK-P1-2B-04

Mỗi formula line có ingredient_code cụ thể

## SMK-P1-2B-05

Không có formula line chỉ ghi group mơ hồ

## SMK-P1-2B-06

Mỗi line có UOM hợp lệ

## SMK-P1-2B-07

UOM khác nhau có conversion

## SMK-P1-2B-08

Mỗi line cần scale có ratio_to_rice

## SMK-P1-2B-09

Nhập kg gạo scale đúng toàn bộ line

## SMK-P1-2B-10

Đổi kg gạo không tạo formula version mới

## SMK-P1-2B-11

Đổi ratio_to_rice tạo formula version mới

## SMK-P1-2B-12

Thêm nguyên liệu tạo formula version mới

## SMK-P1-2B-13

Bỏ nguyên liệu tạo formula version mới

## SMK-P1-2B-14

Buffer +5%/+7% không làm thay đổi formula

## SMK-P1-2B-15

Formula snapshot giữ nguyên khi có version mới

## SMK-P1-2B-16

Seed formula idempotent

## SMK-P1-2B-17

Không seed formula chưa Owner confirm

## SMK-P1-2B-18

G0/research không active operational

## 28. EVIDENCE REQUIREMENTS

Mã evidence

Nội dung evidence

## EVD-P1-2B-FORMULA-HEADER

Danh sách formula header đã chuẩn hóa

## EVD-P1-2B-FORMULA-LINES

Danh sách formula line có ingredient cụ thể

## EVD-P1-2B-ANCHOR-RICE

Bằng chứng mỗi G1 có gạo anchor

## EVD-P1-2B-RATIO

Bảng ratio_to_rice

## EVD-P1-2B-UOM

Bằng chứng UOM hợp lệ và conversion

## EVD-P1-2B-SCALING

Kết quả test scale theo kg gạo

## EVD-P1-2B-VERSIONING

Test đổi tỷ lệ/thêm/bỏ nguyên liệu tạo version mới

## EVD-P1-2B-BUFFER

Bằng chứng buffer không sửa formula

## EVD-P1-2B-SNAPSHOT

Bằng chứng snapshot formula không bị đổi ngược

## EVD-P1-2B-OWNER-CONFIRM

Biên bản/chốt Owner confirm formula

## EVD-P1-2B-SEED-IDEMPOTENCY

Log seed idempotency nếu có seed sau này

## EVD-P1-2B-G0-BLOCK

Bằng chứng G0 không active operational

Lưu ý:

Evidence Submitted chưa phải Evidence Accepted.

Không được gọi PASS nếu evidence chưa được Owner/Reviewer chấp nhận.

## 29. RISK REGISTER P1.2B

Risk

Mức độ

Nguyên nhân

Cách kiểm soát

Công thức thiếu version

## P0

Formula chưa chuẩn hóa

Bắt buộc formula_version

G1 thiếu anchor gạo

## P0

Chưa map ingredient anchor

Owner confirm

Ratio_to_rice thiếu

## P0

Công thức chưa đủ dữ liệu

Chặn seed/code

Công thức dùng nhóm mơ hồ

## P0

Không có ingredient cụ thể

Bắt buộc line detail

UOM sai

## P0

Thiếu conversion

UOM checklist

Sửa G1 trực tiếp

## P0

Thiếu versioning guard

Formula version rule

Scale kg gạo bị hiểu là đổi công thức

## P1

Nhầm scaling với versioning

Rule scaling

Buffer cộng vào formula

## P0

Nhầm planning với production formula

Buffer boundary

Phase 2 chọn tay nguyên liệu

## P0

Formula thiếu detail

Snapshot-ready formula

Seed formula bẩn

## P0

Chưa Owner confirm

No dirty seed gate

G0 active operational

## P0

Nhầm research và production

G0 block

Snapshot bị cập nhật ngược

## P0

Không lưu snapshot

Snapshot requirement

## 30. DONE GATE

File P1.2B được xem là đạt khi:

Đã khóa Recipe phải gắn SKU.

Đã khóa Formula phải có version.

Đã khóa G1 là operational formula baseline.

Đã khóa G0/research không được active operational.

Đã khóa G2/G3 là version tương lai khi thay đổi công thức.

Đã khóa gạo là anchor ingredient.

Đã khóa mỗi formula G1 có đúng một anchor.

Đã khóa ratio_to_rice cho từng line cần scale.

Đã khóa formula scaling theo kg gạo.

Đã khóa scale kg gạo không tạo version mới.

Đã khóa đổi tỷ lệ/thêm/bỏ nguyên liệu phải tạo version mới.

Đã khóa formula line phải là nguyên liệu cụ thể.

Đã khóa line group không thay thế ingredient.

Đã khóa UOM và conversion requirement.

Đã khóa rounding policy.

Đã khóa buffer +5%/+7% chỉ là planning.

Đã khóa buffer không sửa công thức sản xuất.

Đã khóa formula snapshot cho Phase 2.

Đã khóa Phase 2 không chọn tay nguyên liệu.

Đã khóa seed formula chỉ sau Owner confirm.

Đã khóa Dev/Codex/Copilot handoff ở Analysis Only.

Đã khóa smoke/evidence cần có.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Production Readiness.

## 31. FAIL GATE

File P1.2B fail nếu xảy ra một trong các trường hợp sau:

Formula không có version.

Formula G1 không có anchor gạo.

Có nhiều anchor trong một formula.

Anchor không phải ingredient canonical.

Formula line thiếu ingredient_code.

Formula line chỉ ghi group mơ hồ.

Formula line thiếu UOM.

UOM thiếu conversion khi cần.

Thiếu ratio_to_rice.

Agent tự suy luận ratio_to_rice.

Đổi kg gạo bị tạo version mới.

Đổi tỷ lệ nhưng không tạo version mới.

Thêm nguyên liệu nhưng không tạo version mới.

Bỏ nguyên liệu nhưng không tạo version mới.

Sửa G1 active trực tiếp.

G0/research được active operational.

Buffer +5%/+7% bị cộng vào formula.

Formula snapshot không lưu.

Production Order cho chọn tay nguyên liệu.

Material Request cho chọn tay nguyên liệu.

Seed formula chưa Owner confirm.

Seed formula tự set sellable/released/production-ready.

Code trực tiếp từ nguồn tổng hợp.

Dev/Codex/Copilot sửa code khi đang Analysis Only.

Gọi Evidence Submitted là Evidence Accepted.

Gọi Completion Decision.

Gọi Gateway PASS.

Gọi Production Readiness.

Gọi Go-live Decision.

Hạng mục

Trạng thái

Documentation review

## READY FOR OWNER REVIEW

Phase

## PHASE 1

Module

Recipe / BOM / Formula Version

Implementation authorization

KHONG

Code authorization

KHONG

Migration authorization

KHONG

Seed authorization

KHONG, cho đến khi Owner confirm và seed governance đạt

Limited implementation authorization

KHONG

Gateway status

bị chặn

Completion status

## WAITING IMPLEMENTATION EVIDENCE

Evidence status

## NOT ACCEPTED

Release status

Production Readiness

KHONG

Go-live Decision

KHONG

Tài liệu kế tiếp

## 33. KẾT LUẬN ĐIỀU PHỐI P1.2B

Từ thời điểm dùng bản V2.0 này, toàn bộ Recipe/BOM/Formula trong Phase 1 phải tuân thủ các khóa sau:

Công thức phải gắn SKU.

Công thức phải có version.

G1 là công thức vận hành nền, không sửa trực tiếp sau khi khóa.

Gạo là anchor ingredient.

Ratio_to_rice là lõi để scale nguyên liệu.

Nhập kg gạo chỉ scale sản lượng, không đổi công thức.

Đổi tỷ lệ/thêm/bỏ nguyên liệu thì bắt buộc tạo Formula Version mới.

Công thức phải bung từng nguyên liệu cụ thể, không dùng nhóm mơ hồ.

UOM và conversion phải chuẩn hóa trước seed/code.

Buffer +5%/+7% là planning buffer, không sửa formula.

Phase 2 chỉ được tiêu thụ formula qua snapshot.

Production Order và Material Request không được chọn tay nguyên liệu.

Seed formula chỉ được thực hiện sau Owner confirm.

Không code trực tiếp từ nguồn tổng hợp.

Không gọi Gateway PASS hoặc Production Readiness.

Tài liệu kế tiếp cần viết lại toàn bộ là:
