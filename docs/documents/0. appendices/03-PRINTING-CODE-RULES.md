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
