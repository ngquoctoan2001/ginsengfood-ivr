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
