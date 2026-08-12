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
