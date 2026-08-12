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
