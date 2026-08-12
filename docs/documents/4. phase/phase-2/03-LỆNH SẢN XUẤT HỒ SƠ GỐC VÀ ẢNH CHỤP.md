# 03-P2 - LỆNH SẢN XUẤT HỒ SƠ GỐC VÀ SNAPSHOT

Trạng thái: `PRODUCTION_ORDER_DOSSIER_SNAPSHOT_CANONICAL`  
Mode: `LIMITED IMPLEMENTATION HANDOFF`  
Mục tiêu: khóa lệnh sản xuất, hồ sơ sản xuất gốc và snapshot công thức từ Phase 1.

## 1. Lệnh sản xuất

Khi mở lệnh sản xuất, hệ thống phải sinh:

1. Mã hồ sơ sản xuất gốc.
2. Mã lệnh sản xuất.
3. SKU, tên sản phẩm, nhóm, phân loại.
4. Mã công thức, version công thức.
5. Ca sản xuất, bộ phận, quản lý nhà máy.
6. Số lượng kế hoạch.
7. Production BOM Snapshot đầy đủ từng nguyên liệu.

## 2. Không chọn tay nguyên liệu

Tại lệnh sản xuất và phiếu cấp nguyên liệu, người dùng không được tự thêm, xóa, đổi nguyên liệu hoặc thay hàm lượng chuẩn. Thay đổi phải qua Formula Version mới hoặc phiếu thay đổi/approval riêng.

## 3. Snapshot bất biến

Production BOM Snapshot lưu:

1. Formula version tại thời điểm approve lệnh.
2. Full ingredient lines, UOM, quantity, ratio, rounding.
3. Anchor calculation nếu đổi quy mô mẻ.
4. Packaging/output profile.
5. Evidence và owner approval refs.

Formula thay đổi sau đó không sửa snapshot của lệnh đã approve.

## 4. Field lệnh sản xuất

| Nhóm field | Field bắt buộc | Nguồn |
| --- | --- | --- |
| Header | production_order_code, production_dossier_code, created_at, created_by, factory, shift, department | Nhập/chọn khi tạo lệnh |
| Product/SKU | sku_id, sku_code, product_name_snapshot, product_group, classification_state | Phase 1 SKU master |
| Formula | formula_id, formula_version, formula_status, source_formula_ref | Phase 1 Formula G1 |
| Kế hoạch | planned_batch_qty, planned_output_gói, planned_output_hộp, planned_output_thùng, start_date, due_date | Người lập lệnh + output profile |
| BOM snapshot | material lines, required_qty, uom, ratio_to_anchor, rounding_policy, material_group | Formula resolver |
| Packaging snapshot | B1/B2/B3 material refs, planned package qty, print payload template | Packaging profile |
| Approval | manager, factory_owner, status, approval_time, evidence_refs | Workflow approval |

## 5. Production dossier là root

Production dossier phải là root cho toàn bộ phiếu sau:

1. Phiếu đề nghị cấp nguyên liệu.
2. Phiếu chấp thuận nguyên liệu đưa vào sản xuất.
3. Phiếu kế toán xuất nguyên liệu.
4. Phiếu check-in/check-out nhân sự.
5. Phiếu theo dõi sơ chế.
6. Phiếu kiểm chất lượng sau sấy thăng hoa.
7. Phiếu đóng gói cấp 1.
8. Phiếu đóng gói cấp 2.
9. Phiếu QC thành phẩm.
10. Lệnh nhập kho thành phẩm.
11. Trace/internal genealogy.
12. Public QR projection sau release.

Không phiếu nào trong chuỗi này được tạo rời không có `production_dossier_id`.

## 6. Production BOM Snapshot schema

| Field | Rule |
| --- | --- |
| `snapshot_id` | Sinh một lần khi approve dossier |
| `snapshot_version` | Tăng khi có approved deviation; không sửa snapshot cũ |
| `formula_id/formula_version` | Lấy từ Phase 1; G0/research không được dùng production |
| `material_id` | Canonical material, không alias |
| `material_name_snapshot` | Lưu tên tại thời điểm snapshot |
| `line_group` | A1/A2/A3/B1/B2/B3 hoặc group production tương ứng |
| `required_qty` | Tính theo planned qty và ratio |
| `uom` | UOM chuẩn đã pass conversion/density nếu cần |
| `ratio_to_anchor` | Bắt buộc với line scale theo gạo |
| `is_required` | Bắt buộc/không bắt buộc theo formula |
| `dietary_claim_guard` | Fail-closed nếu claim vegan có line động vật |
| `source_ref` | Ref công thức G1 và source appendix |
| `locked_by/locked_at` | Người và thời điểm khóa snapshot |

## 7. Trạng thái lệnh và dossier

```text
DRAFT -> SUBMITTED -> APPROVED -> MATERIAL_REQUEST_GENERATED -> IN_EXECUTION -> COMPLETED
```

Fail/hold path:

```text
BLOCKED / REJECTED / CANCELLED / SUPERSEDED_BY_APPROVED_DEVIATION
```

`APPROVED` chỉ có nghĩa dossier và snapshot đã khóa. Nó không có nghĩa đã xuất kho, đã sản xuất, đã QC, đã release hoặc đã bán được.

## 8. Tự sinh phiếu sau khi approve dossier

| Phiếu tự sinh | Trigger | Dữ liệu tự hiện |
| --- | --- | --- |
| Phiếu đề nghị cấp nguyên liệu | Dossier approved | SKU, formula, snapshot lines, required qty, UOM |
| Phiếu chấp thuận nguyên liệu | Material request submitted | Approved lot candidate, QC/readiness, available qty |
| Phiếu check-in/check-out | Batch/stage opened | dossier, batch, stage, shift |
| Phiếu theo dõi sơ chế | Batch stage started | material lines, process route, expected qty |
| Phiếu QC sau sấy | Freeze-dry completed | batch, stage, sample refs |
| Phiếu đóng gói cấp 1/2 | QC pass stage | packaging profile, batch code, print payload |
| Phiếu QC thành phẩm | Packaging completed | finished lot, package qty |
| Lệnh nhập kho thành phẩm | Batch released | released qty, warehouse, SKU/batch |

## 9. Không được làm

1. Không cho production user chọn tay nguyên liệu ở lệnh sản xuất.
2. Không sửa công thức trong lệnh đã approve.
3. Không dùng formula hiện tại để thay snapshot cũ.
4. Không sinh material request nếu snapshot thiếu UOM/quantity/material_id.
5. Không issue nguyên liệu từ lệnh sản xuất; issue chỉ ở file 04.
6. Không tạo batch từ lệnh nếu material chưa được issue/nhận xưởng theo gate.
7. Không gọi lệnh sản xuất là production ready hoặc sellable.
---

## PHỤ LỤC NGUỒN ĐÃ NHẬP - BẢNG PHIẾU LỆNH SẢN XUẤT.md

> Phụ lục này giữ nội dung nguồn cũ để không mất bảng, số liệu, field hoặc rule khi bỏ file nguồn riêng. Phần rewrite chuẩn hóa ở đầu file là nguồn chính; phụ lục dùng để trace và đối chiếu số liệu.

## 10. Operational forms - phiếu và lệnh vận hành bắt buộc

# OPERATIONAL FORMS

## I. DANH SÁCH TỐI THIỂU NHỮNG PHIẾU / LỆNH BẮT BUỘC PHẢI LÀM MẪU TRƯỚC

1. Phiếu đánh giá và nhập kho nguyên liệu đầu vào

Bắt buộc vì:

là cửa vào của nguyên liệu

có đánh giá đạt / không đạt

có giá nhập

có ngõ kế toán

có lô nguyên liệu

2. Lệnh sản xuất

Bắt buộc vì:

là nơi sinh hồ sơ sản xuất gốc

chọn SKU là ra công thức

kéo theo toàn bộ chuỗi phiếu sau

3. Phiếu đề nghị cấp nguyên liệu cho sản xuất

Bắt buộc vì:

là cầu nối giữa công thức và kho

là nơi hệ thống tự hiện nguyên liệu theo công thức

là nền để sinh xuất kho cho sản xuất

4. Phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất

Bắt buộc vì:

khóa lô nào thực sự được dùng cho mẻ

có xác nhận 3 bên

có lớp kế toán xuất nguyên liệu

5. Phiếu check-in / check-out nhân sự theo lệnh sản xuất / mẻ / công đoạn

Bắt buộc vì:

là đầu vào tính chi phí nhân công vận hành

6. Phiếu theo dõi sơ chế

Bắt buộc vì:

theo dõi khay / kg / lít

ghi hao hụt sơ chế

là dữ liệu chuyển tiếp sang sấy

7. Phiếu kiểm chất lượng sau sấy thăng hoa

Bắt buộc vì:

có % độ ẩm

là cổng chất lượng quan trọng trước đóng gói

8. Phiếu theo dõi đóng gói cấp 1

Bắt buộc vì:

theo dõi đơn vị gói / lọ / hũ

gắn với in cấp 1

9. Phiếu theo dõi đóng gói cấp 2

Bắt buộc vì:

theo dõi hộp / thùng / lọ / hũ

gắn với in mã cấp 2, QR, barcode, batch

10. Phiếu QC thành phẩm

Bắt buộc vì:

chốt đạt / không đạt trước nhập kho

11. Lệnh nhập kho thành phẩm

Bắt buộc vì:

là điểm kích hoạt tồn kho thành phẩm

nối với kho + truy vết + MISA

12. Phiếu kế toán xuất nguyên liệu cho sản xuất

Bắt buộc vì:

bản xưởng không hiện giá

nhưng kế toán phải có chứng từ giá trị để hạch toán

## II. KẾT LUẬN PHẠM VI MẪU

Nếu làm trước 12 mẫu này thì đã đủ để:

dựng luồng vận hành chuẩn

dựng dữ liệu cho code

QA test end-to-end

vận hành hình dung rõ

Phiếu kế hoạch sản xuất có thể làm sau nếu cần, vì về mặt code lõi, nhiều nơi có thể bắt đầu từ Lệnh sản xuất. Nhưng nếu bạn muốn quy trình đầy đủ hơn, có thể thêm nó ở vòng sau.

## III. BỘ MẪU TOÀN BỘ - NGUỒN DỮ LIỆU CHO CODE

phần này mình viết theo format thống nhất:

A. Thông tin chung

B. Dữ liệu chi tiết

C. Bằng chứng / đánh giá

D. Xác nhận tham gia

E. Kết luận / điều kiện chuyển bước

Mình viết theo đúng các rule đã chốt:

nơi nào click chọn

nơi nào tự hiện

nơi nào nhập

đơn vị nào dùng theo từng công đoạn

chỗ nào có giá / không có giá

## 1. PHIẾU ĐÁNH GIÁ VÀ NHẬP KHO NGUYÊN LIỆU ĐẦU VÀO

A. Thông tin chung

Mã phiếu

Ngày nhập

Giờ nhập

Địa điểm nhận hàng

Đại diện SSAVIGROUP

Người lập phiếu

Bộ phận tiếp nhận

B. Thông tin nhà cung cấp

Mã nhà cung cấp

Tên nhà cung cấp

Địa chỉ

Số điện thoại

Người giao hàng

Số điện thoại người giao

Phương tiện giao hàng

Biển số xe

Ghi chú giao hàng

C. Danh sách nguyên liệu nhập

Nguyên liệu tại phiếu này = click chọn từ danh mục master

| STT | Mã NL | Tên nguyên liệu | Nhóm nguyên liệu | Chủng loại / mô tả | Số lượng giao | Đơn vị | Quy cách | Tình trạng ban đầu | Kết quả đánh giá | Lý do không đạt | Đề xuất xử lý | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

Đơn vị chỉ được dùng:

kg

lít

ml

D. Khối hạch toán nhập kho

| STT | Mã NL | Số lượng giao | Số lượng đạt nhập | Số lượng không đạt / hư hỏng | Đơn vị | Đơn giá nhập | Thành tiền nhập | Chi phí mua phân bổ | Giá trị hư hỏng loại bỏ | Đơn giá bình quân sau nhập | Ghi chú kế toán |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

E. Bằng chứng

Ảnh toàn bộ lô hàng

Ảnh từng nguyên liệu

Video kiểm hàng

Hóa đơn / phiếu giao hàng

Biên bản xử lý hàng không đạt

F. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Đại diện SSAVIGROUP |   |   |   |   |   |
| Người giao hàng |   |   |   |   |   |
| Kho |   |   |   |   |   |
| QC đầu vào |   |   |   |   |   |
| Kế toán kho / vật tư |   |   |   |   |   |

G. Kết luận

Cho nhập kho toàn bộ

Cho nhập kho một phần

Không cho nhập kho

## 2. LỆNH SẢN XUẤT

A. Thông tin chung

Mã hồ sơ sản xuất gốc

Mã lệnh sản xuất

Ngày lập

SKU

Tên sản phẩm

Mã công thức

Version công thức

Nhóm sản phẩm

Phân loại Vegan / Mặn

Số lượng kế hoạch

Đơn vị kế hoạch

Ca sản xuất

Bộ phận sản xuất

Quản lý nhà máy

Ghi chú

B. Dữ liệu công thức tự hiện

Tại lệnh sản xuất = không được chọn tay nguyên liệu Hệ thống tự hiện theo SKU + công thức + version

Phần 1 - Dược liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |

Phần 2 - Nguyên liệu

| STT | Mã NL | Tên nguyên liệu | Hàm lượng | Đơn vị | Ghi chú |
| --- | --- | --- | --- | --- | --- |

C. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Người lập lệnh |   |   |   |   |   |
| Quản lý nhà máy |   |   |   |   |   |
| Bộ phận sản xuất |   |   |   |   |   |

D. Kết luận

Mở lệnh sản xuất

Chưa mở lệnh sản xuất

## 3. PHIẾU ĐỀ NGHỊ CẤP NGUYÊN LIỆU CHO SẢN XUẤT

A. Thông tin chung

Mã phiếu

Mã hồ sơ sản xuất

Mã lệnh sản xuất

SKU

Tên sản phẩm

Mã công thức

Version

Kho xuất

Người đề nghị

Ngày đề nghị

B. Danh sách nguyên liệu cần cấp

Tự hiện từ công thức, không chọn tay

| STT | Mã NL | Tên nguyên liệu | Nhóm nguyên liệu | Hàm lượng công thức | Đơn vị | Số lượng yêu cầu cấp | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- |

Đơn vị chỉ được dùng:

kg

lít

ml

C. Giá trị tham chiếu quản trị / kế toán

| STT | Mã NL | Số lượng yêu cầu cấp | Đơn vị | Đơn giá bình quân hiện hành | Giá trị dự kiến xuất | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- |

D. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Người đề nghị |   |   |   |   |   |
| Kho |   |   |   |   |   |
| Quản lý nhà máy |   |   |   |   |   |

E. Kết luận

Đồng ý cấp phát

Cấp thiếu

Chưa đồng ý cấp phát

## 4. PHIẾU CHẤP THUẬN NGUYÊN LIỆU ĐƯỢC PHÉP ĐƯA VÀO SẢN XUẤT

A. Thông tin chung

Mã phiếu

Mã hồ sơ sản xuất

Mã lệnh sản xuất

SKU

Tên sản phẩm

Mã công thức

Version

Ngày chấp thuận

B. Danh sách nguyên liệu được duyệt

Danh sách này tự sử dụng làm nền từ phiếu đề nghị cấp phát

| STT | Mã NL | Tên nguyên liệu | Lô nguyên liệu | Số lượng duyệt xuất | Đơn vị | Kết quả chấp thuận | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- |

Đơn vị chỉ được dùng:

kg

lít

ml

C. Phiếu kế toán xuất đi kèm

| STT | Mã NL | Lô nguyên liệu | Số lượng duyệt xuất | Đơn vị | Đơn giá bình quân | Thành tiền xuất cho sản xuất | Hao hụt cấp phát | Giá trị hao hụt | Ghi chú kế toán |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

D. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Quản lý nhà máy |   |   |   |   |   |
| Kho |   |   |   |   |   |
| QC |   |   |   |   |   |
| Kế toán vật tư / kho |   |   |   |   |   |

E. Kết luận

Cho phép đưa vào sản xuất

Chưa cho phép đưa vào sản xuất

## 5. PHIẾU CHECK-IN / CHECK-OUT NHÂN SỰ THEO LỆNH / MẺ / CÔNG ĐOẠN

A. Thông tin chung

Mã phiếu

Mã hồ sơ sản xuất

Mã lệnh sản xuất

Mẻ sản xuất

SKU

Tên sản phẩm

Công đoạn

Ca sản xuất

Ngày làm việc

B. Danh sách nhân sự tham gia

| STT | Mã nhân sự | Họ tên | Bộ phận | Chức danh | Đơn giá công / giờ | Check-in | Check-out | Tổng giờ làm | Chi phí nhân công phân bổ | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

C. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Tổ trưởng / phụ trách công đoạn |   |   |   |   |   |
| Quản lý bộ phận |   |   |   |   |   |

D. Kết luận

Hoàn tất công đoạn

Chưa hoàn tất công đoạn

## 6. PHIẾU THEO DÕI SƠ CHẾ

A. Thông tin chung

Mã phiếu

Mã hồ sơ sản xuất

Mã lệnh sản xuất

SKU

Tên sản phẩm

Mẻ sản xuất

Người phụ trách

Thời gian bắt đầu

Thời gian kết thúc

B. Kết quả theo dõi sơ chế

| STT | Loại bán thành phẩm / lô sơ chế | Số lượng đầu vào | Đơn vị | Số lượng sau sơ chế | Đơn vị | Hao hụt | Đơn vị | Đánh giá | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

Đơn vị được dùng:

khay

kg

lít

C. Bằng chứng

Ảnh sơ chế

Video sơ chế

D. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Người thực hiện |   |   |   |   |   |
| Tổ trưởng / phụ trách công đoạn |   |   |   |   |   |
| Quản lý bộ phận |   |   |   |   |   |

E. Kết luận

Đạt

Không đạt

Cần xử lý thêm

## 7. PHIẾU KIỂM CHẤT LƯỢNG SAU SẤY THĂNG HOA

A. Thông tin chung

Mã phiếu

Mã hồ sơ sản xuất

Mã lệnh sản xuất

SKU

Tên sản phẩm

Mã công thức

Version

Mẻ sản xuất

Ngày kiểm tra

B. Chỉ tiêu kiểm tra

| STT | Chỉ tiêu | Kết quả | Đơn vị | Đánh giá | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| 1 | Số lượng bán thành phẩm kiểm tra |   | khay |   |   |
| 2 | Tỷ lệ % độ ẩm sản phẩm |   | % | Đạt / Không đạt |   |
| 3 | Màu sắc cảm quan |   |   | Đạt / Không đạt |   |
| 4 | Cấu trúc sau sấy |   |   | Đạt / Không đạt |   |
| 5 | Mùi |   |   | Đạt / Không đạt |   |
| 6 | Trạng thái bề mặt |   |   | Đạt / Không đạt |   |

C. Bằng chứng

Ảnh sau sấy

Video kiểm tra

File đo độ ẩm nếu có

D. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| QC |   |   |   |   |   |
| Quản lý nhà máy |   |   |   |   |   |
| Bộ phận phụ trách công đoạn |   |   |   |   |   |

E. Kết luận

Đạt cho đóng gói

Không đạt

Chờ xử lý

## 8. PHIẾU THEO DÕI ĐÓNG GÓI CẤP 1

A. Thông tin chung

Mã phiếu

Mã hồ sơ sản xuất

Mã lệnh sản xuất

SKU

Tên sản phẩm

Mẻ sản xuất

Người phụ trách

B. Kết quả đóng gói cấp 1

| STT | Số lượng đầu vào | Đơn vị | Số lượng đạt | Đơn vị | Số lượng không đạt | Đơn vị | Lý do không đạt | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |

Đơn vị được dùng:

gói

lọ

hũ

C. Bằng chứng

Ảnh

Video

D. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Người thực hiện |   |   |   |   |   |
| Tổ trưởng / phụ trách |   |   |   |   |   |
| Quản lý bộ phận |   |   |   |   |   |

E. Kết luận

Đạt

Không đạt

Cần xử lý thêm

## 9. PHIẾU THEO DÕI ĐÓNG GÓI CẤP 2

A. Thông tin chung

Mã phiếu

Mã hồ sơ sản xuất

Mã lệnh sản xuất

SKU

Tên sản phẩm

Mẻ sản xuất

Người phụ trách

B. Kết quả đóng gói cấp 2

| STT | Số lượng đầu vào | Đơn vị | Số lượng đầu ra | Đơn vị | Số lượng không đạt | Đơn vị | Lý do không đạt | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |

Đơn vị được dùng:

hộp

thùng

lọ

hũ

C. Bằng chứng

Ảnh

Video

D. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Người thực hiện |   |   |   |   |   |
| Tổ trưởng / phụ trách |   |   |   |   |   |
| Quản lý bộ phận |   |   |   |   |   |

E. Kết luận

Đạt

Không đạt

Cần xử lý thêm

## 10. PHIẾU QC THÀNH PHẨM

A. Thông tin chung

Mã phiếu

Mã hồ sơ sản xuất

Mã lệnh sản xuất

SKU

Tên sản phẩm

Mẻ sản xuất

Ngày kiểm tra

B. Kết quả QC

| STT | Số lượng kiểm tra | Đơn vị | Số lượng đạt | Đơn vị | Số lượng không đạt | Đơn vị | Tỷ lệ đạt | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |

Đơn vị phù hợp:

gói

lọ

hũ

hộp

thùng (tùy điểm QC thực tế)

C. Bằng chứng

Ảnh

Video

File kiểm tra

D. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| QC |   |   |   |   |   |
| Quản lý nhà máy |   |   |   |   |   |
| Bộ phận liên quan |   |   |   |   |   |

E. Kết luận

Đạt nhập kho

Không đạt

Chờ xử lý

## 11. LỆNH NHẬP KHO THÀNH PHẨM

A. Thông tin chung

Mã lệnh nhập kho

Mã hồ sơ sản xuất

Mã lệnh sản xuất

SKU

Tên sản phẩm

Kho nhập

Vị trí kho

Người nhập

Thời gian nhập

B. Kết quả nhập kho

| STT | Số lượng nhập | Đơn vị | Ghi chú |
| --- | --- | --- | --- |

Đơn vị được dùng:

hộp

thùng

lọ

hũ

C. Kiểm soát nhập kho

| STT | Chỉ tiêu | Kết quả | Ghi chú |
| --- | --- | --- | --- |
| 1 | QC thành phẩm đạt | Có / Không |   |
| 2 | Quét mã nhập kho | Có / Không |   |
| 3 | Ảnh / video nhập kho | Có / Không |   |

D. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Kho |   |   |   |   |   |
| QC |   |   |   |   |   |
| Quản lý nhà máy |   |   |   |   |   |

E. Kết luận

Cho nhập kho

Chưa cho nhập kho

## 12. PHIẾU KẾ TOÁN XUẤT NGUYÊN LIỆU CHO SẢN XUẤT

A. Thông tin chung

Mã phiếu kế toán

Mã hồ sơ sản xuất

Mã lệnh sản xuất

SKU

Tên sản phẩm

Mẻ sản xuất

Ngày hạch toán

B. Bảng hạch toán

| STT | Mã NL | Tên nguyên liệu | Lô nguyên liệu | Số lượng xuất | Đơn vị | Đơn giá bình quân | Thành tiền xuất | Hao hụt | Giá trị hao hụt | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

Đơn vị được dùng:

kg

lít

ml

C. Xác nhận tham gia

| Vai trò | Họ tên | Bộ phận | Trạng thái xác nhận | Thời gian xác nhận | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Kế toán vật tư / kho |   |   |   |   |   |
| Kho |   |   |   |   |   |
| Quản lý nhà máy |   |   |   |   |   |

D. Kết luận

Hạch toán hoàn tất

Chưa hoàn tất

---

## PHỤ LỤC NGUỒN ĐÃ NHẬP - CÔNG THỨC VẬN HÀNH 20 SKU MỚI.MD

## 11. Công thức vận hành 20 SKU dùng cho snapshot

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

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P2.2B - PRODUCTION ORDER SNAPSHOT WORKSHOP

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-01 production order and BOM snapshot.
- PACK-02 Recipe / BOM / Formula Version.
- TECH-03 Production Order / Snapshot / Workshop.
- Production snapshot khong duoc sua nguoc recipe active.

## Noi Dung Rewrite

## 31. KẾT LUẬN ĐIỀU PHỐI P2.2A

Từ thời điểm dùng bản V1.1 này, miền Source / Raw Lot / QC / Readiness phải tuân thủ các khóa sau:

Phiếu nhập nguyên liệu đầu vào là nơi được click chọn nguyên liệu từ master.

Không nhập material text tự do.

Raw receipt sinh raw lot có nguồn, material, UOM, quantity rõ.

Incoming QC không đồng nghĩa readiness.

QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.

READY_FOR_PRODUCTION là gate riêng.

Chỉ lot READY_FOR_PRODUCTION mới đủ điều kiện cấp sản xuất.

Lot HOLD/QUARANTINE/REJECTED/EXPIRED/DAMAGED không được issue.

Sâm Savigin là Company Source / strategic reserve.

Sâm vẫn phải qua intake/QC/readiness.

Sâm không là supplier material thường.

Disposal/write-off không được xóa tay tồn kho.

MISA không điều khiển raw receipt/raw lot/QC/readiness.

Actor/permission/audit/idempotency/evidence là bắt buộc.

P2.2A không lấn sang Production Order đầy đủ.

P2.2A không lấn sang Material Issue đầy đủ.

Evidence Submitted chưa phải Evidence Accepted.

Global Gate vẫn bị chặn.

Tài liệu kế tiếp cần viết lại toàn bộ là:

## 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP

## TÀI LIỆU ĐẶC TẢ PRODUCTION ORDER / PRODUCTION DOSSIER / FORMULA SNAPSHOT / WORKSHOP HANDOFF CHO PHASE 2

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

## PHASE-02-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP

Đây là file thứ hai trong nhóm triển khai giới hạn của Phase 2 - Operational Core.

File này phụ trách miền:

Production demand intake boundary.

Production order.

Production order state lifecycle.

Production dossier / hồ sơ sản xuất gốc.

Formula snapshot.

Recipe snapshot.

BOM snapshot.

SKU + formula code + formula version auto-display.

Anchor gạo / scaled formula lines dựa trên từ Phase 1.

No manual ingredient selection.

Workshop handoff.

Production order immutability boundary.

Production order không tự Material Issue.

Production order không tự Batch.

Production order không tự Inventory Debit.

Production order không tự Sellable.

Actor / permission / audit / idempotency / evidence.

Smoke/evidence cho P2.2B.

Report 15 mục cho P2.2B.

Tài liệu này thay thế toàn bộ nội dung cũ của:

## 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP.docx

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP

Phiên bản

Phase

## PHASE 2 - OPERATIONAL CORE

Workstream

## P2.2B

Loại tài liệu

Limited Implementation Handoff / Production Order-Snapshot-Workshop

Nguồn chính

README Phase 2 V1.1 + P2.1 Technical Design

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

Phụ thuộc Phase 1

Product/SKU/Recipe/Formula Version/G1/UOM/Ingredient đã chuẩn hóa

Phụ thuộc P2.2A

Raw Lot / QC / Readiness foundation

Cho phép code ngay bởi file này?

Không, chỉ sau khi có lệnh Limited Implementation riêng

Cho phép migration ngay bởi file này?

Không

Cho phép seed ngay bởi file này?

Không

Cho phép Agent tự suy luận công thức?

Không

Global Gate

bị chặn

Production Readiness

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để khóa miền lệnh sản xuất và hồ sơ sản xuất gốc của Operational Core.

Đây là điểm nối trực tiếp giữa:

Phase 1: Product / SKU / Recipe / Formula Version.

Phase 2: Production / Material Issue / Batch / Warehouse / Traceability.

Nếu P2.2B sai, toàn bộ chuỗi vận hành sau đó sẽ sai theo:

Phiếu đề nghị cấp nguyên liệu có thể tự chọn tay nguyên liệu.

Material Issue có thể xuất sai nguyên liệu.

Batch có thể không trace được về công thức gốc.

QC thành phẩm không biết sản xuất theo version nào.

Print/QR có thể sai batch hoặc sai hạn dùng.

Traceability không dựng được genealogy từ SKU -> formula -> raw lot -> batch.

Recall không xác định được batch/lot bị ảnh hưởng.

Warehouse có thể nhập kho thành phẩm không đúng nguồn.

Commerce/AI sau này có thể bán sản phẩm không có operational truth.

Mục tiêu chính của tài liệu này:

Khóa khi chọn SKU trong lệnh sản xuất, hệ thống phải tự hiện công thức, mã công thức và formula version.

Khóa công thức phải bung chi tiết từng nguyên liệu cụ thể, không hiển thị nhóm mơ hồ.

Khóa không cho người dùng chọn tay nguyên liệu trong Production Order.

Khóa Production Order phải tạo Formula/Recipe/BOM Snapshot.

Khóa Production Order phải sinh Production Dossier / hồ sơ sản xuất gốc.

Khóa Production Dossier là nguồn dữ liệu duy nhất cho chuỗi phiếu sau.

Khóa snapshot phải bất biến, không bị thay đổi khi recipe/formula hiện tại thay đổi.

Khóa Workshop Handoff chỉ nhận dữ liệu dựa trên từ Production Dossier.

Khóa P2.2B không được tự Material Issue, không trừ tồn kho, không tạo batch output, không nhập kho, không sellable.

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

Không thay file chi tiết P2.2B

Tầng 2

P2.1 Technical Design V1.1

Thiết kế workstream

Cung cấp form registry, field behavior, data inheritance

Không tự cho phép code

Tầng 3

P2.2B V2.0 này

Nguồn chính cho Production Order / Snapshot / Workshop

Giao dev/Agent phân tích/triển khai giới hạn khi được mở

Không lấn sang P2.2C/D/E

Tầng 4

Phase 1 P1.2B Recipe/BOM/Formula Version

Nguồn công thức

Cung cấp formula code, version, ingredient, UOM, ratio/scaling

Không sửa Phase 1 trong file này

Tầng 5

Phase 1 P1.2D Seed Governance

Nguồn seed/master data

Cung cấp seed rule và no dirty data

Không seed mới trong P2.2B

Tầng 6

P2.2A Source/Raw/QC/Readiness

Nguồn raw lot readiness

Cung cấp readiness boundary cho material sau này

Không issue material trong P2.2B

Tầng 7

nguồn tổng hợp TÀI LIỆU.docx

Nguồn phụ trợ nghiệp vụ

Đối chiếu lệnh sản xuất, hồ sơ gốc, tự hiện công thức, không chọn tay nguyên liệu

Không code/seed trực tiếp

Tầng 8

Code hiện tại

Hiện trạng triển khai

Dùng để gap analysis

Không là source-of-truth nếu conflict

## 5. SCOPE IN - PHẠM VI BAO GỒM

Tài liệu này bao gồm:

Production demand intake boundary.

Production order creation.

Production order validation.

Production order lifecycle.

SKU selection.

Formula code auto-display.

Formula version auto-display.

Formula kind auto-display.

Recipe/BOM/Formula snapshot.

Ingredient line snapshot.

Quantity/UOM snapshot.

Anchor gạo / anchor quantity snapshot nếu công thức anchor-based.

Scaled formula lines snapshot.

Production dossier generation.

Production dossier code.

Production dossier root data boundary.

Workshop handoff.

No manual ingredient selection.

Snapshot immutability.

Snapshot historical correctness.

Production order dependency on Phase 1.

Production order dependency on P2.2A readiness boundary.

Actor / permission / audit / idempotency / evidence.

Smoke/evidence requirements cho P2.2B.

Report 15 mục cho P2.2B.

## 6. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM

Tài liệu này không bao gồm:

Không triển khai Material Request đầy đủ.

Không triển khai Material Approval đầy đủ.

Không triển khai Material Issue đầy đủ.

Không ghi raw inventory ledger debit.

Không triển khai Material Receipt / Workshop Receipt đầy đủ.

Không triển khai Production Batch đầy đủ.

Không triển khai Batch Process đầy đủ.

Không triển khai Batch QC.

Không triển khai Batch Release.

Không triển khai Warehouse Receipt.

Không triển khai Finished Goods Ledger.

Không triển khai Print/QR runtime.

Không triển khai Traceability đầy đủ.

Không triển khai Recall / Sale Lock đầy đủ.

Không triển khai Commerce Runtime.

Không quyết định Sellable.

Không triển khai full MRP/procurement.

Không triển khai MISA Integration thật.

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

Production Demand

Nhu cầu sản xuất đến từ kế hoạch nội bộ, forecast hoặc yêu cầu đã được duyệt

Production Order

Lệnh sản xuất chính thức trong Operational Core

Production Dossier

Hồ sơ sản xuất gốc sinh từ Production Order

Formula Snapshot

Bản chụp công thức tại thời điểm tạo Production Order

Recipe Snapshot

Bản chụp recipe tại thời điểm tạo Production Order

BOM Snapshot

Bản chụp định mức nguyên liệu/vật tư tại thời điểm tạo Production Order

Workshop Handoff

Bàn giao dữ liệu sản xuất xuống xưởng

Anchor Quantity

Lượng nguyên liệu mốc, theo Phase 1 là kg gạo nếu công thức anchor-based

Scaled Formula Lines

Các dòng nguyên liệu đã được tính theo anchor quantity

No Manual Ingredient Selection

Không cho người dùng thêm/bớt/chọn tay nguyên liệu trong lệnh sản xuất

Snapshot Immutability

Snapshot không đổi sau khi Production Order đã tạo

Root Production File

Cách gọi khác của Production Dossier / hồ sơ sản xuất gốc

Downstream Forms

Các phiếu sau Production Dossier: Material Request, Issue, Batch, QC, Packaging, Traceability

## PHẦN 2/4 - PRODUCTION ORDER / FORMULA SNAPSHOT / PRODUCTION DOSSIER MODEL

## 8. LUỒNG NGHIỆP VỤ TỔNG QUAN P2.2B

Luồng nghiệp vụ P2.2B:

Production Demand / Plan -> Select SKU -> Auto-load Formula Code + Formula Version + Formula Lines -> Enter Planned Quantity / Anchor Quantity -> Generate Scaled Formula Lines -> Create Production Order -> Create Formula/Recipe/BOM Snapshot -> Generate Production Dossier -> Workshop Handoff

Trong đó:

SKU phải đến từ Product/SKU master đã chuẩn hóa ở Phase 1.

Formula phải là formula active/approved theo Phase 1.

Formula phải có formula code và formula version.

Nếu công thức là anchor-based, gạo anchor và ratio_to_rice phải đến từ Phase 1.

Người dùng không được chọn tay nguyên liệu.

Người dùng chỉ được nhập các trường sản xuất hợp lệ như sản lượng kế hoạch, anchor quantity, ngày dự kiến, ghi chú.

Hệ thống tự bung công thức thành từng dòng nguyên liệu cụ thể.

Production Order phải snapshot toàn bộ dữ liệu công thức.

Production Dossier phải được sinh từ Production Order.

Các phiếu sau phải dựa trên Production Dossier.

## 9. PRODUCTION DEMAND INTAKE BOUNDARY

## 9.1. Nguyên tắc

P2.2B có thể nhận nhu cầu sản xuất từ:

Kế hoạch sản xuất nội bộ.

Forecast sản xuất.

Nhu cầu từ kinh doanh đã được duyệt.

Kế hoạch sản xuất lớn có Owner/Giám đốc phê duyệt.

Lệnh sản xuất thủ công do người có quyền tạo.

Tuy nhiên, P2.2B không được để demand tự động phát sinh Production Order nếu chưa qua resolver/guard.

## 9.2. Demand không được tự tạo Production Order nếu thiếu guard

Không được tự tạo Production Order nếu:

SKU chưa active/allowed.

SKU chưa có formula active.

Formula thiếu version.

Formula thiếu ingredient lines.

Formula thiếu UOM.

Formula chưa Owner confirm.

Packaging/production dependency chưa đủ nếu policy yêu cầu.

Actor không có quyền.

Evidence/approval chưa đủ nếu là kế hoạch đặc biệt.

Demand đến từ Commerce/AI/Gateway chưa qua owner-approved production planning rule.

## 9.3. Checklist production demand

Trường

Bắt buộc

Quy tắc

demand_ref

Có nếu có nguồn demand

Trỏ tới kế hoạch hoặc yêu cầu

demand_type

Có

## PLAN / FORECAST / OWNER_REQUEST / SALES_APPROVED / OTHER

sku_code

Có

SKU cần sản xuất

planned_quantity

Có

Sản lượng kế hoạch

planned_uom

Có

Đơn vị sản lượng

priority

Tùy

Bình thường/cao/khẩn

approval_ref

Có nếu policy yêu cầu

Kế hoạch lớn cần approval

requested_by

Có

Actor

requested_at

Có

Thời điểm

status

Có

DRAFT / APPROVED_FOR_ORDER / bị chặn

## 10. PRODUCTION ORDER MODEL

## 10.1. Nguyên tắc

Production Order là lệnh sản xuất chính thức.

Production Order không phải:

Material Issue.

Batch output.

Warehouse receipt.

Sellable decision.

Commerce order.

Production Order chỉ mở chuỗi sản xuất và tạo hồ sơ sản xuất gốc.

## 10.2. Checklist Production Order Header

Trường

Bắt buộc

Quy tắc

production_order_code

Có

Mã lệnh sản xuất duy nhất

production_demand_ref

Tùy

Nguồn nhu cầu nếu có

sku_code

Có

Chọn từ SKU master

sku_name_snapshot

Có

Snapshot tên SKU

product_line_snapshot

Có

Dòng sản phẩm

formula_code

Có

Tự hiện từ SKU

formula_version

Có

Tự hiện từ formula active

formula_kind

Có

## ANCHOR_BASED / FIXED_BATCH

planned_quantity

Có

Sản lượng kế hoạch

planned_uom

Có

Đơn vị sản lượng

anchor_ingredient_code

Có nếu anchor-based

Gạo anchor

anchor_quantity

Có nếu anchor-based

Lượng gạo dùng làm mốc

anchor_uom

Có nếu anchor-based

UOM của gạo

planned_start_date

Có

Ngày bắt đầu dự kiến

planned_end_date

Tùy

Ngày kết thúc dự kiến

production_site

Tùy

Xưởng/khu vực

responsible_user

Có

Người phụ trách

status

Có

Theo lifecycle

created_by

Có

Actor

created_at

Có

Thời điểm

audit_required

Có

Bắt buộc

idempotency_key

Có nếu command create có side effect

Chống tạo trùng

## 10.3. Production Order Status Lifecycle

Status

Ý nghĩa

Cho phép làm gì tiếp

## DRAFT

Nháp

Chỉnh thông tin chưa snapshot chính thức

## READY_FOR_REVIEW

Chờ review

Chờ duyệt

## APPROVED_FOR_PRODUCTION

Được duyệt sản xuất

Có thể sinh dossier/snapshot

## SNAPSHOT_CREATED

Đã tạo snapshot

Không sửa formula gốc trong order

## DOSSIER_CREATED

Đã sinh hồ sơ sản xuất gốc

Có thể handoff xuống xưởng

## WORKSHOP_HANDOFF_READY

Sẵn sàng bàn giao xưởng

P2.2C có thể tạo Material Request sau này

## WORKSHOP_HANDOFF_SENT

Đã bàn giao xưởng

Chờ các bước sau

## CANCELLED

Hủy

Không tạo request/issue

bị chặn

Bị chặn

Cần xử lý điểm chặn

## 10.4. Status không đồng nghĩa

Status/Event

Không đồng nghĩa với

Production Order CREATED

Material đã issue

Production Order APPROVED

Raw lot đã READY

Production Order SNAPSHOT_CREATED

Nguyên liệu đã xuất kho

Production Dossier CREATED

Batch đã tạo

Workshop Handoff Sent

Xưởng đã nhận nguyên liệu

Production Order Completed

Batch RELEASED

Production Order Approved

SKU sellable

## 11. SKU SELECTION / FORMULA AUTO-DISPLAY

## 11.1. Nguyên tắc

Khi người dùng chọn SKU trong Production Order:

Hệ thống phải tự hiện:

Tên SKU.

Recipe code.

Formula code.

Formula version.

Formula kind.

Ingredient lines.

UOM từng dòng.

Anchor ingredient nếu có.

Ratio_to_rice nếu công thức anchor-based.

Formula approval metadata nếu cần.

Packaging profile reference nếu policy cần cho bước sau.

Không được để người dùng nhập công thức bằng tay.

Không được để người dùng chọn nguyên liệu bằng tay.

## 11.2. Field behavior khi chọn SKU

Field

Hành vi

sku_code

Click chọn từ SKU master

sku_name

Tự hiện

recipe_code

Tự hiện

formula_code

Tự hiện

formula_version

Tự hiện

formula_kind

Tự hiện

formula_status

Tự hiện

anchor_ingredient

Tự hiện nếu anchor-based

formula_lines

Tự bung chi tiết

ingredient_code

Tự hiện, không chọn tay

ingredient_name

Tự hiện

formula_uom

Tự hiện

ratio_to_rice

Tự hiện nếu có

quantity_at_anchor

Tự tính

planned_quantity

Người dùng nhập trong giới hạn rule

anchor_quantity

Người dùng nhập nếu formula anchor-based

note

Người dùng nhập có kiểm soát

## 11.3. Fail gate SKU/formula auto-display

Fail nếu:

Chọn SKU nhưng không tự hiện formula code.

Chọn SKU nhưng không tự hiện formula version.

Chọn SKU nhưng formula thiếu lines.

Formula line chỉ hiển thị nhóm mơ hồ.

Cho người dùng chọn tay ingredient.

Cho người dùng đổi formula line trực tiếp trong Production Order.

Cho người dùng đổi ratio_to_rice trong Production Order.

Cho người dùng dùng formula chưa approved/active nếu policy chặn.

Cho Production Order tạo khi SKU không có formula active.

## 12. FORMULA / RECIPE / BOM SNAPSHOT

## 12.1. Nguyên tắc

Production Order phải snapshot công thức tại thời điểm tạo lệnh.

Snapshot là bản lịch sử bất biến, phục vụ:

Material Request.

Material Issue.

Batch genealogy.

## QC.

Warehouse.

Traceability.

Recall.

Costing nội bộ.

MISA handoff sau này nếu cần.

Nếu Recipe/Formula/BOM hiện tại thay đổi sau khi Production Order đã tạo, snapshot cũ không được thay đổi theo.

## 12.2. Snapshot tối thiểu phải có

Nhóm snapshot

Trường bắt buộc

SKU snapshot

sku_code, sku_name, product_line

Recipe snapshot

recipe_code, recipe_name, recipe_status

Formula snapshot

formula_code, formula_version, formula_kind, formula_status

Approval snapshot

approved_by, approved_at, effective_from nếu có

Anchor snapshot

anchor_ingredient_code, anchor_name, anchor_quantity, anchor_uom

Formula line snapshot

line_no, ingredient_code, ingredient_name, group, UOM, ratio/quantity

Scaled line snapshot

planned/scaled quantity, UOM, rounding result

Material source snapshot

material_group, source_type nếu cần

Packaging ref snapshot

packaging_profile_ref nếu cần

Audit snapshot

snapshot_actor, snapshot_created_at, correlation_id

## 12.3. Formula Snapshot Line

Trường

Bắt buộc

Quy tắc

snapshot_line_no

Có

Thứ tự dòng snapshot

original_formula_line_ref

Có

Trỏ line gốc nếu có

ingredient_code

Có

Nguyên liệu cụ thể

ingredient_name_snapshot

Có

Tên tại thời điểm snapshot

material_group_snapshot

Có

A/B/Company Source

source_type_snapshot

Có

Supplier/Company Source/Internal

formula_uom

Có

UOM công thức

inventory_uom_snapshot

Có

UOM tồn kho

ratio_to_rice_snapshot

Có nếu anchor-based

Tỷ lệ tại thời điểm snapshot

base_quantity_snapshot

Có nếu có

Quantity gốc

scaled_quantity

Có

Quantity theo planned/anchor quantity

rounding_policy_snapshot

Có

Làm tròn theo policy

is_anchor_ingredient

Có

TRUE với gạo anchor

is_active_line_snapshot

Có

Theo formula gốc tại thời điểm snapshot

## 12.4. Snapshot immutability

Sau khi snapshot đã tạo:

Không được:

Cập nhật snapshot theo formula mới.

Đổi formula version trong order cũ.

Đổi ingredient line snapshot.

Đổi ratio_to_rice snapshot.

Đổi UOM snapshot.

Đổi scaled quantity nếu không có workflow deviation được duyệt.

Xóa snapshot để tạo lại không audit.

Dùng recipe hiện tại thay snapshot cũ khi tạo Material Request.

Nếu cần thay đổi sản xuất sau khi snapshot:

Phải có workflow điều chỉnh.

Phải có reason.

Phải có approval.

Phải có audit.

Có thể tạo production order mới hoặc revised snapshot theo policy riêng.

Không được sửa lén snapshot cũ.

## 13. PRODUCTION DOSSIER - HỒ SƠ SẢN XUẤT GỐC

## 13.1. Nguyên tắc

nguồn tổng hợp đã khóa:

Lệnh sản xuất sinh hồ sơ sản xuất gốc.

Hồ sơ sản xuất gốc là nguồn dữ liệu duy nhất cho chuỗi phiếu sau.

Vì vậy, P2.2B phải đảm bảo sau khi Production Order được duyệt/snapshot hợp lệ, hệ thống tạo Production Dossier.

Production Dossier là root data cho:

Material Request.

Material Approval.

Material Issue.

Workshop Receipt.

Batch.

Batch Process.

Batch QC.

Batch Release.

Warehouse Receipt.

Packaging.

Print.

Traceability.

Recall.

## 13.2. Checklist Production Dossier

Trường

Bắt buộc

Quy tắc

production_dossier_code

Có

Mã hồ sơ sản xuất gốc

production_order_code

Có

Trỏ Production Order

sku_code

Có

dựa trên order

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

planned_quantity

Có

dựa trên order

planned_uom

Có

dựa trên order

anchor_quantity

Có nếu anchor-based

dựa trên order

scaled_formula_lines_ref

Có

dựa trên snapshot

production_site

Tùy

dựa trên/nhập theo rule

responsible_user

Có

Người phụ trách

dossier_status

Có

Theo lifecycle

created_by

Có

Actor

created_at

Có

Thời điểm

audit_log

Có

Bắt buộc

## 13.3. Production Dossier Status

Status

Ý nghĩa

## CREATED

Hồ sơ đã tạo

## READY_FOR_MATERIAL_REQUEST

Đủ điều kiện sinh phiếu đề nghị cấp nguyên liệu

## MATERIAL_REQUESTED

Đã tạo phiếu đề nghị cấp nguyên liệu

## MATERIAL_APPROVED

Nguyên liệu đã được chấp thuận

## MATERIAL_ISSUED

Nguyên liệu đã xuất kho

## WORKSHOP_RECEIVED

Xưởng đã nhận nguyên liệu

## IN_PROCESS

Đang sản xuất

## BATCH_CREATED

Batch đã tạo

Hồ sơ đóng

## CANCELLED

Hủy

bị chặn

Bị chặn

P2.2B chỉ cần thiết kế/tạo nền đến mức:

## CREATED.

## READY_FOR_MATERIAL_REQUEST.

WORKSHOP_HANDOFF_READY nếu thuộc scope.

Các trạng thái sau thuộc P2.2C/P2.2D.

## 13.4. Production Dossier không được làm

Production Dossier không được:

Tự tạo Material Issue.

Tự debit inventory.

Tự tạo Batch output.

Tự release batch.

Tự nhập kho thành phẩm.

Tự set SKU sellable.

Tự sinh QR public.

Tự tạo Official Order.

Tự tạo revenue.

## 14. KHONG MANUAL INGREDIENT SELECTION

## 14.1. Nguyên tắc

nguồn tổng hợp đã khóa:

Không được cho người dùng chọn tay nguyên liệu ở lệnh sản xuất.

Không được cho người dùng chọn tay nguyên liệu ở phiếu cấp nguyên liệu.

Phiếu đề nghị cấp nguyên liệu phải tự hiện nguyên liệu theo công thức.

Trong P2.2B, rule này được áp dụng trực tiếp cho Production Order và Production Dossier.

## 14.2. Người dùng được nhập gì trong Production Order

Người dùng có thể nhập:

SKU cần sản xuất.

Sản lượng kế hoạch.

Anchor quantity nếu công thức anchor-based.

Ngày bắt đầu dự kiến.

Ngày kết thúc dự kiến.

Xưởng/khu vực sản xuất nếu có.

Người phụ trách.

Ghi chú sản xuất.

Lý do sản xuất.

Evidence/approval nếu policy yêu cầu.

## 14.3. Người dùng không được nhập gì trong Production Order

Người dùng không được nhập/chọn/sửa:

Ingredient list.

Ingredient code.

Ingredient name.

Formula version thủ công nếu đã có default active formula.

Ratio_to_rice.

UOM công thức.

BOM line.

Material group.

Source type.

Formula approval metadata.

Formula snapshot line sau khi snapshot đã tạo.

Nếu cần đổi nguyên liệu hoặc tỷ lệ, phải quay lại Formula Version governance của Phase 1.

## 15. WORKSHOP HANDOFF

## 15.1. Nguyên tắc

Workshop Handoff là bước bàn giao hồ sơ sản xuất xuống xưởng.

Workshop Handoff không phải Material Receipt.

Workshop Handoff không có nghĩa xưởng đã nhận nguyên liệu.

Workshop Handoff chỉ có nghĩa:

Production Dossier đã sẵn sàng cho xưởng xem.

Formula snapshot đã cố định.

Nguyên liệu cần cấp đã được xác định ở mức snapshot.

Các bước tiếp theo có thể tạo Material Request ở P2.2C.

## 15.2. Workshop Handoff Package

Thành phần

Bắt buộc

Nguồn

production_dossier_code

Có

Production Dossier

production_order_code

Có

Production Order

sku_code

Có

Production Order

sku_name

Có

Snapshot

formula_code

Có

Snapshot

formula_version

Có

Snapshot

scaled_formula_lines

Có

Formula Snapshot

planned_quantity

Có

Production Order

anchor_quantity

Có nếu anchor-based

Production Order

production_note

Tùy

Production Order

responsible_user

Có

Production Order

workshop_target

Tùy

Xưởng/khu vực

handoff_status

Có

CREATED/SENT/ACKNOWLEDGED nếu có

audit_log

Có

Bắt buộc

## 15.3. Workshop Handoff không được làm

Workshop Handoff không được:

Debit nguyên liệu.

Xác nhận xưởng đã nhận nguyên liệu.

Tạo material ledger.

Tạo batch output.

Tạo QC result.

Tạo release.

Tạo warehouse receipt.

Tự tạo print/QR.

Tự set sellable.

Material Receipt / Workshop Receipt thuộc P2.2C, không phải P2.2B.

## PHẦN 3/4 - STATE / VALIDATION / CROSS-CUTTING / DEPENDENCY

## 16. DEPENDENCY MATRIX

Dependency

Cần từ đâu

Nếu thiếu thì sao

SKU canonical

Phase 1 Product/SKU

Không tạo Production Order

Formula active

Phase 1 Recipe/Formula

Không tạo Production Order

Formula version

Phase 1 P1.2B

Không tạo snapshot

Formula lines

Phase 1 P1.2B

Không tạo snapshot

Ingredient canonical

Phase 1 Ingredient

Không tạo snapshot

UOM/conversion

Phase 1 UOM

Không scale/không snapshot

Anchor gạo

Phase 1 Formula

Không scale anchor-based

Ratio_to_rice

Phase 1 Formula

Không scale

Packaging profile

Phase 1 seed/profile nếu policy cần

Chỉ warning/block theo policy

Raw lot readiness

## P2.2A

Chưa dùng để issue trong P2.2B, nhưng là dependency cho P2.2C

Actor/permission

Foundation/RBAC

Không cho command

Audit/idempotency

Foundation

Không cho command side effect

Evidence policy

Foundation/Evidence Registry

Không pass review nếu thiếu

## 17. VALIDATION RULES PRODUCTION ORDER

Production Order chỉ được tạo khi pass các validation sau:

Validation

Rule

SKU exists

SKU phải tồn tại trong master

SKU allowed

SKU không inactive/bị chặn

Formula exists

SKU phải có formula active hoặc approved theo policy

Formula version exists

Formula phải có version

Formula lines exist

Formula phải có line cụ thể

No group-only formula

Formula không chỉ có nhóm mơ hồ

UOM valid

Mọi line phải có UOM

Anchor valid

Anchor gạo phải có nếu anchor-based

Ratio valid

ratio_to_rice phải có nếu anchor-based

Planned quantity valid

Sản lượng kế hoạch hợp lệ

Anchor quantity valid

Anchor quantity hợp lệ nếu anchor-based

Actor valid

Actor có quyền tạo lệnh

Idempotency valid

Không tạo trùng command

Evidence valid

Evidence/approval đủ nếu policy yêu cầu

## 18. STATE TRANSITION GUARD

From

To

Điều kiện

## DRAFT

## READY_FOR_REVIEW

Có SKU, planned quantity, formula preview

## READY_FOR_REVIEW

## APPROVED_FOR_PRODUCTION

Actor có quyền duyệt

## APPROVED_FOR_PRODUCTION

## SNAPSHOT_CREATED

Formula active hợp lệ và snapshot thành công

## SNAPSHOT_CREATED

## DOSSIER_CREATED

Dossier tạo thành công

## DOSSIER_CREATED

## WORKSHOP_HANDOFF_READY

Dossier đủ dữ liệu để handoff

## WORKSHOP_HANDOFF_READY

## WORKSHOP_HANDOFF_SENT

Actor/system gửi handoff

Any non-final

## CANCELLED

Actor có quyền, chưa có downstream irreversible command hoặc có rule hủy

Any

bị chặn

Thiếu dependency, conflict, stale state, invalid formula

## 19. IMMUTABILITY / HISTORICAL CORRECTNESS

## 19.1. Nguyên tắc

Production Order và Production Dossier phải bảo vệ lịch sử.

Khi Production Order đã snapshot:

Formula hiện tại có đổi thì snapshot cũ không đổi.

Ingredient master có đổi tên thì snapshot cũ giữ tên snapshot.

UOM master có đổi thì snapshot cũ giữ UOM snapshot.

Formula version mới không ghi đè version cũ.

Material Request sau này phải dùng snapshot, không dùng formula hiện tại.

## 19.2. Các trường bất biến sau snapshot

Trường

Có được sửa sau snapshot không?

sku_code

Không

formula_code

Không

formula_version

Không

formula_kind

Không

ingredient_code snapshot

Không

ingredient_name_snapshot

Không

ratio_to_rice_snapshot

Không

formula_uom snapshot

Không

scaled_quantity snapshot

Không, trừ workflow deviation được duyệt

anchor_ingredient_code

Không

anchor_quantity

Không, trừ tạo lại order/revision theo policy

rounding_policy_snapshot

Không

## 20. ACTOR / PERMISSION / AUDIT / IDEMPOTENCY / EVIDENCE

## 20.1. Command rủi ro trong P2.2B

Command

Actor

Permission

Audit

Idempotency

Evidence

Create Production Order

Bắt buộc

Bắt buộc

Bắt buộc

Bắt buộc

Nếu policy yêu cầu

Approve Production Order

Bắt buộc

Bắt buộc

Bắt buộc

Bắt buộc

Approval evidence

Create Snapshot

System/Actor

Bắt buộc

Bắt buộc

Bắt buộc

Snapshot evidence

Generate Production Dossier

System/Actor

Bắt buộc

Bắt buộc

Bắt buộc

Dossier evidence

Send Workshop Handoff

Bắt buộc

Bắt buộc

Bắt buộc

Bắt buộc

Handoff evidence

Cancel Production Order

Bắt buộc

Bắt buộc

Bắt buộc

Bắt buộc

Cancel reason

Block Production Order

System/Actor

Bắt buộc

Bắt buộc

Bắt buộc

Block reason

## 20.2. Cross-cutting fail gate

Fail nếu:

Create Production Order thiếu actor.

Create Production Order thiếu permission.

Create Production Order thiếu audit.

Retry tạo duplicate Production Order.

Same idempotency key khác payload không conflict.

Snapshot không có audit.

Dossier không có audit.

Evidence SUBMITTED được coi là ACCEPTED khi policy yêu cầu accepted.

Hệ thống suy đoán formula khi dependency unavailable.

Hệ thống tự chọn formula/ingredient khi dữ liệu thiếu.

## 21. ENTITY / API / SERVICE BOUNDARY

## 21.1. Entity impact draft

Tài liệu này không tạo migration, nhưng implementation sau này có thể cần rà soát các nhóm entity:

Nhóm entity

Vai trò

production_demand

Nhu cầu/kế hoạch sản xuất nếu có

production_order

Lệnh sản xuất

production_order_status_history

Lịch sử trạng thái

formula_snapshot

Snapshot công thức

formula_snapshot_line

Dòng snapshot công thức

recipe_snapshot

Snapshot recipe

bom_snapshot_line

Dòng BOM snapshot

production_dossier

Hồ sơ sản xuất gốc

production_dossier_status_history

Lịch sử hồ sơ

workshop_handoff

Bàn giao xưởng

workshop_handoff_line

Dữ liệu bàn giao nếu cần

production_order_evidence

Evidence

production_order_audit

Audit

idempotency_record

Chống retry trùng

## 21.2. API/service boundary

Nhóm

Mục tiêu

Không được

Production Order API

Tạo/đọc/duyệt/hủy order

Không issue material

Formula Preview API

Tự hiện formula khi chọn SKU

Không sửa formula

Snapshot Service

Tạo snapshot bất biến

Không đọc formula động sau snapshot

Dossier Service

Tạo Production Dossier

Không tạo batch output

Workshop Handoff API

Gửi dữ liệu xuống xưởng

Không receipt nguyên liệu

Validation Service

Guard SKU/formula/UOM/actor

Không tự suy luận dữ liệu thiếu

Audit Service

Ghi audit

Không bỏ audit

Idempotency Service

Chống duplicate order/snapshot

Không double side effect

Evidence Service

Link evidence

Không coi submitted là accepted

## 22. MISA / ACCOUNTING BOUNDARY TRONG P2.2B

P2.2B không triển khai full MISA Integration.

Production Order có thể là nguồn dữ liệu tham chiếu sau này cho kế toán/giá thành nội bộ, nhưng:

MISA không tạo Production Order.

MISA không sửa Production Order.

MISA không sửa Formula Snapshot.

MISA không tạo Production Dossier.

MISA không hủy Production Dossier.

MISA không quyết định xưởng sản xuất.

MISA không quyết định nguyên liệu được cấp.

MISA không làm thay Material Request/Issue.

Ginsengfood là nguồn vận hành.

MISA chỉ nhận dữ liệu qua integration layer riêng ở phase phù hợp.

## 23. MRP / PROCUREMENT BOUNDARY TRONG P2.2B

P2.2B không triển khai full MRP/procurement.

Nếu Production Order xuất phát từ kế hoạch sản xuất lớn, hệ thống có thể cần ghi nhận:

Demand source.

Production plan reference.

Owner/Giám đốc approval.

Evidence.

Audit.

Nhưng P2.2B không được:

Tự tạo purchase request.

Tự bỏ qua threshold.

Tự override procurement suppression.

Tự quyết định mua nguyên liệu.

Tự sửa công thức theo buffer.

Tự cộng buffer vào formula snapshot nếu không có rule được duyệt.

PHẦN 4/4 - EXECUTION ORDER / HANDOFF / SMOKE / DONE GATE / FAIL GATE

## 24. EXECUTION ORDER P2.2B

Khi sau này được phép triển khai, P2.2B phải đi theo thứ tự:

Thứ tự

Việc cần làm

Output

Đọc README Phase 2 V1.1

Hiểu boundary Phase 2

Đọc P2.1 Technical Design V1.1

Hiểu form registry / field behavior

Đọc Phase 1 P1.2B Formula Version

Hiểu formula/version/anchor/ratio

Đọc P2.2B V2.0 này

Hiểu Production Order/Snapshot/Dossier

Analysis code hiện tại

Gap matrix

Owner Confirm nếu thiếu dữ liệu

Owner confirm list

Technical implementation plan

Task breakdown

Limited implementation nếu được mở

Code trong scope

Smoke P2.2B

Smoke result

10

Evidence submission

Evidence package

11

Report 15 mục

Handoff update

Hiện tại trong cuộc làm tài liệu này:

Chỉ viết lại tài liệu sạch, chưa mở implementation.

## 25. DEV / CODEX / COPILOT HANDOFF

## 25.1. Mode mặc định

Khi giao file này cho Agent:

## MODE: LIMITED IMPLEMENTATION CHỈ KHI ĐÃ CÓ APPROVED ANALYSIS + TECHNICAL DESIGN

Nếu chưa có approval, phải dùng:

## MODE: ANALYSIS ONLY

## 25.2. Prompt đúng cho Agent

MODE hiện tại: ANALYSIS ONLY nếu chưa có Owner/dev lead approval.

Nhiệm vụ:

Lập Gap Matrix giữa tài liệu P2.2B và code hiện tại.

Liệt kê entity/table hiện có hoặc thiếu.

Liệt kê API/DTO/service/UI/worker bị ảnh hưởng.

Liệt kê nơi Production Order đang thiếu formula snapshot nếu có.

Liệt kê nơi Production Order cho chọn tay nguyên liệu nếu có.

Liệt kê nơi Material Request/Issue đang lấy formula động thay vì snapshot nếu có.

Liệt kê nơi snapshot bị cập nhật ngược khi formula thay đổi nếu có.

Liệt kê nơi Production Order tự tạo Material Issue/Batch/Inventory nếu có.

Liệt kê nơi Workshop Handoff bị hiểu nhầm là Workshop Receipt nếu có.

Liệt kê risk P0/P1/P2.

Đề xuất implementation scope.

Đề xuất smoke/evidence.

Không sửa code nếu chưa có lệnh Limited Implementation riêng.

Cấm:

Không code trực tiếp từ nguồn tổng hợp.

Không tạo migration khi chưa duyệt.

Không seed dữ liệu khi chưa Owner confirm.

Không tự tạo formula.

Không tự suy luận ratio_to_rice.

Không tự cho chọn tay nguyên liệu.

Không triển khai Material Issue đầy đủ.

Không triển khai Batch đầy đủ.

Không gọi Gateway PASS.

Không gọi Production Readiness.

## 26. OWNER CONFIRM REQUIRED

Các điểm cần Owner/dev lead xác nhận trước implementation:

Nhóm

Nội dung cần xác nhận

Production Order code

Quy tắc mã lệnh sản xuất

Production Dossier code

Quy tắc mã hồ sơ sản xuất gốc

Production demand source

Nguồn demand nào được phép tạo lệnh

Production approval

Ai duyệt lệnh sản xuất

SKU selection

SKU nào được phép tạo lệnh

Formula selection

Chọn default active formula hay cho chọn version theo quyền

Formula snapshot

Field snapshot bắt buộc

Anchor quantity

Cách nhập kg gạo/sản lượng nếu anchor-based

Rounding

Lấy rounding policy từ Phase 1

Workshop handoff

Có cần ACK xưởng không

Cancellation

Khi nào được hủy Production Order

Revision

Khi nào phải tạo order mới / snapshot mới

Material Request trigger

Khi nào P2.2C được tạo request từ dossier

Evidence

Evidence/approval nào bắt buộc

## RBAC

Ai được tạo/duyệt/handoff/hủy/block

Idempotency

Command nào cần key

Audit

Audit field bắt buộc

## 27. STOP CONDITIONS

Dev/Agent phải dừng nếu gặp:

Mã

Điều kiện dừng

Hành động

## P2-2B-STOP-01

SKU chưa canonical

Dừng, quay lại Phase 1

## P2-2B-STOP-02

SKU không có formula active

Dừng, Owner confirm

## P2-2B-STOP-03

Formula thiếu version

Dừng P0

## P2-2B-STOP-04

Formula thiếu ingredient lines

Dừng P0

## P2-2B-STOP-05

Formula line chỉ có group mơ hồ

Dừng P0

## P2-2B-STOP-06

UOM thiếu conversion

Dừng, quay lại Phase 1

## P2-2B-STOP-07

Anchor gạo thiếu nếu anchor-based

Dừng P0

## P2-2B-STOP-08

ratio_to_rice thiếu nếu anchor-based

Dừng P0

## P2-2B-STOP-09

Production Order cho chọn tay nguyên liệu

Dừng P0

## P2-2B-STOP-10

Production Order không snapshot formula

Dừng P0

## P2-2B-STOP-11

Snapshot bị cập nhật khi formula đổi

Dừng P0

## P2-2B-STOP-12

Không có Production Dossier

Dừng P0

## P2-2B-STOP-13

Phiếu sau không lấy Production Dossier làm root

Dừng P0

## P2-2B-STOP-14

Production Order tự Material Issue/debit inventory

Dừng P0

## P2-2B-STOP-15

Workshop Handoff debit/receipt nguyên liệu

Dừng P0

## P2-2B-STOP-16

Command thiếu actor/audit

Dừng P0

## P2-2B-STOP-17

Agent chuẩn bị code khi chưa approval

Dừng

## P2-2B-STOP-18

Scope lấn sang Material Issue/Batch/Warehouse

Dừng, chuyển đúng file P2.2C/D

## P2-2B-STOP-19

Có ý định gọi Gateway PASS/Production Readiness

Dừng

## 28. SMOKE REQUIREMENTS P2.2B

Mã smoke

Nội dung

Expected

## SMK-P2-2B-01

Chọn SKU hợp lệ

Formula code/version tự hiện

## SMK-P2-2B-02

Chọn SKU không có formula active

Bị reject/block

## SMK-P2-2B-03

Formula thiếu version

Bị reject

## SMK-P2-2B-04

Formula thiếu ingredient lines

Bị reject

## SMK-P2-2B-05

Formula line chỉ có group mơ hồ

Bị reject

## SMK-P2-2B-06

Formula anchor-based có anchor gạo

Anchor tự hiện

## SMK-P2-2B-07

Nhập anchor quantity

Scaled formula lines được tính

## SMK-P2-2B-08

Production Order tạo thành công

Order được tạo, chưa issue material

## SMK-P2-2B-09

Production Order snapshot

Có SKU/formula/version/ingredient/quantity/UOM

## SMK-P2-2B-10

Recipe/formula đổi sau order

Snapshot cũ không đổi

## SMK-P2-2B-11

User chọn tay nguyên liệu trong order

Bị chặn

## SMK-P2-2B-12

User sửa ratio_to_rice trong order

Bị chặn

## SMK-P2-2B-13

Production Order sinh Production Dossier

Dossier được tạo

## SMK-P2-2B-14

Production Dossier thiếu snapshot

Bị chặn

## SMK-P2-2B-15

Workshop Handoff

Handoff package có dossier/snapshot

## SMK-P2-2B-16

Workshop Handoff không debit inventory

Không có raw ledger debit

## SMK-P2-2B-17

Production Order không tự tạo batch

Không có batch output

## SMK-P2-2B-18

Retry create order same key/payload

Không duplicate

## SMK-P2-2B-19

Same idempotency key different payload

Conflict

## SMK-P2-2B-20

Command thiếu actor/permission

## DENY/BLOCK

## SMK-P2-2B-21

Evidence required nhưng SUBMITTED

Không pass nếu policy cần ACCEPTED

## SMK-P2-2B-22

Production Order active

Không tự set Sellable

## 29. EVIDENCE REQUIREMENTS P2.2B

Mã evidence

Nội dung evidence

## EVD-P2-2B-SKU-FORMULA-AUTO

Chọn SKU tự hiện formula code/version

## EVD-P2-2B-FORMULA-LINES

Formula lines cụ thể, không group mơ hồ

## EVD-P2-2B-ANCHOR-SCALING

Anchor quantity tạo scaled formula lines

## EVD-P2-2B-PRODUCTION-ORDER

Production Order được tạo đúng

## EVD-P2-2B-SNAPSHOT

Formula/Recipe/BOM Snapshot

## EVD-P2-2B-SNAPSHOT-IMMUTABLE

Snapshot cũ không đổi khi formula đổi

## EVD-P2-2B-NO-MANUAL-INGREDIENT

Không chọn tay nguyên liệu

## EVD-P2-2B-DOSSIER

Production Dossier được sinh

## EVD-P2-2B-DOSSIER-ROOT

Phiếu sau dùng dossier làm root

## EVD-P2-2B-WORKSHOP-HANDOFF

Workshop Handoff package

## EVD-P2-2B-NO-DEBIT

Production Order/Handoff không debit inventory

## EVD-P2-2B-NO-BATCH

Production Order không tự tạo batch

## EVD-P2-2B-AUDIT

Actor/permission/audit evidence

## EVD-P2-2B-IDEMPOTENCY

Retry không tạo duplicate

EVD-P2-2B-GATEWAY-bị chặn

Gateway vẫn bị chặn

Evidence Submitted chưa phải Evidence Accepted.

## 30. RISK REGISTER P2.2B

Risk

Mức độ

Nguyên nhân

Kiểm soát

Production Order không snapshot formula

## P0

Dùng formula động

Snapshot service + smoke

Chọn tay nguyên liệu

## P0

UI/API cho manual ingredient

No manual ingredient guard

Formula thiếu version

## P0

Phase 1 chưa đủ

Stop condition

Formula line mơ hồ

## P0

Recipe chỉ có group

Phase 1/P1.2B dependency

Snapshot bị cập nhật ngược

## P0

Không lưu immutable snapshot

Snapshot immutability

Không có Production Dossier

## P0

Order không sinh root data

Dossier gate

Phiếu sau không dựa trên dossier

## P0

Data inheritance sai

Dossier root rule

Production Order tự issue material

## P0

Lấn sang P2.2C

Scope guard

Workshop Handoff bị hiểu là Receipt

## P0

Nhầm handoff/receipt

Handoff boundary

Retry tạo duplicate order

## P1

Thiếu idempotency

Idempotency key

Missing audit

## P1

Command thiếu actor/correlation

Audit required

Sellable bị set từ order

## P0

Lấn sang Commerce

No sellable boundary

## 31. REPORT FORMAT 15 MỤC CHO P2.2B

Mọi report P2.2B phải có đủ 15 mục:

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

Nếu thiếu 15 mục, không được chuyển sang P2.2C.

## 32. DONE GATE P2.2B

P2.2B được xem là đạt ở mức handoff/implementation report khi:

Chọn SKU tự hiện formula code.

Chọn SKU tự hiện formula version.

Chọn SKU tự hiện formula kind.

Formula lines được bung chi tiết từng nguyên liệu.

Không có formula line nhóm mơ hồ.

Không cho chọn tay nguyên liệu.

Không cho sửa ratio_to_rice trong Production Order.

Anchor gạo tự hiện nếu formula anchor-based.

Anchor quantity tạo scaled formula lines.

Production Order được tạo đúng validation.

Production Order không tự Material Issue.

Production Order không tự debit inventory.

Production Order không tự tạo batch.

Production Order không tự set Sellable.

Formula/Recipe/BOM Snapshot được tạo.

Snapshot bất biến sau khi formula đổi.

Production Dossier được sinh.

Production Dossier là root cho phiếu sau.

Workshop Handoff package có đủ dossier/snapshot.

Workshop Handoff không phải Workshop Receipt.

Actor/permission/audit/idempotency/evidence được kiểm soát.

Có smoke result.

Có evidence package.

Có report 15 mục.

Không lấn sang P2.2C/P2.2D/P2.2E.

Gateway vẫn bị chặn.

Không gọi Completion Decision.

Không gọi Production Readiness.

## 33. FAIL GATE P2.2B

P2.2B fail nếu xảy ra một trong các trường hợp:

Production Order tạo khi SKU không có formula active.

Formula thiếu version vẫn tạo order.

Formula thiếu lines vẫn tạo order.

Formula line chỉ có group mơ hồ.

Production Order cho chọn tay nguyên liệu.

Production Order cho sửa formula line.

Production Order cho sửa ratio_to_rice.

Production Order không snapshot formula.

Snapshot thiếu ingredient/quantity/UOM.

Snapshot bị cập nhật ngược khi formula đổi.

Không sinh Production Dossier.

Production Dossier không linked Production Order.

Phiếu sau không dùng Production Dossier làm root.

Production Order tự tạo Material Issue.

Production Order tự debit inventory.

Production Order tự tạo Batch.

Workshop Handoff debit inventory.

Workshop Handoff bị dùng như Material Receipt.

Production Order tự set Sellable.

Command rủi ro thiếu actor.

Command rủi ro thiếu permission.

Command rủi ro thiếu audit.

Retry tạo duplicate side effect.

Evidence Submitted bị gọi Accepted.

Lấn sang Material Issue đầy đủ.

Lấn sang Batch/Warehouse.

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

P2.2B - Production Order / Snapshot / Workshop

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
