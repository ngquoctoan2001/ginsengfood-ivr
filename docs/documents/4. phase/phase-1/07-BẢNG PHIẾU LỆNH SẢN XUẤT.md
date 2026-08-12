**I. DANH SÁCH TỐI THIỂU NHỮNG PHIẾU / LỆNH BẮT BUỘC PHẢI LÀM MẪU TRƯỚC**

**1\. Phiếu đánh giá và nhập kho nguyên liệu đầu vào**

Bắt buộc vì:

- là cửa vào của nguyên liệu
- có đánh giá đạt / không đạt
- có giá nhập
- có ngõ kế toán
- có lô nguyên liệu

**2\. Lệnh sản xuất**

Bắt buộc vì:

- là nơi sinh hồ sơ sản xuất gốc
- chọn SKU là ra công thức
- kéo theo toàn bộ chuỗi phiếu sau

**3\. Phiếu đề nghị cấp nguyên liệu cho sản xuất**

Bắt buộc vì:

- là cầu nối giữa công thức và kho
- là nơi hệ thống tự hiện nguyên liệu theo công thức
- là nền để sinh xuất kho cho sản xuất

**4\. Phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất**

Bắt buộc vì:

- khóa lô nào thực sự được dùng cho mẻ
- có xác nhận 3 bên
- có lớp kế toán xuất nguyên liệu

**5\. Phiếu check-in / check-out nhân sự theo lệnh sản xuất / mẻ / công đoạn**

Bắt buộc vì:

- là đầu vào tính chi phí nhân công vận hành

**6\. Phiếu theo dõi sơ chế**

Bắt buộc vì:

- theo dõi khay / kg / lít
- ghi hao hụt sơ chế
- là dữ liệu chuyển tiếp sang sấy

**7\. Phiếu kiểm chất lượng sau sấy thăng hoa**

Bắt buộc vì:

- có % độ ẩm
- là cổng chất lượng quan trọng trước đóng gói

**8\. Phiếu theo dõi đóng gói cấp 1**

Bắt buộc vì:

- theo dõi đơn vị gói / lọ / hũ
- gắn với in cấp 1

**9\. Phiếu theo dõi đóng gói cấp 2**

Bắt buộc vì:

- theo dõi hộp / thùng / lọ / hũ
- gắn với in mã cấp 2, QR, barcode, batch

**10\. Phiếu QC thành phẩm**

Bắt buộc vì:

- chốt đạt / không đạt trước nhập kho

**11\. Lệnh nhập kho thành phẩm**

Bắt buộc vì:

- là điểm kích hoạt tồn kho thành phẩm
- nối với kho + truy vết + MISA

**12\. Phiếu kế toán xuất nguyên liệu cho sản xuất**

Bắt buộc vì:

- bản xưởng không hiện giá
- nhưng kế toán phải có chứng từ giá trị để hạch toán

**II. KẾT LUẬN PHẠM VI MẪU**

Nếu làm trước **12 mẫu này** thì đã đủ để:

- dựng luồng vận hành chuẩn
- dựng dữ liệu cho code
- QA test end-to-end
- vận hành hình dung rõ

**Phiếu kế hoạch sản xuất** có thể làm sau nếu cần, vì về mặt code lõi, nhiều nơi có thể bắt đầu từ **Lệnh sản xuất**. Nhưng nếu bạn muốn quy trình đầy đủ hơn, có thể thêm nó ở vòng sau.

**III. BỘ MẪU TOÀN BỘ - NGUỒN DỮ LIỆU CHO CODE**

Dưới đây mình viết theo format thống nhất:

- **A. Thông tin chung**
- **B. Dữ liệu chi tiết**
- **C. Bằng chứng / đánh giá**
- **D. Xác nhận tham gia**
- **E. Kết luận / điều kiện chuyển bước**

Mình viết theo đúng các rule đã chốt:

- nơi nào click chọn
- nơi nào tự hiện
- nơi nào nhập
- đơn vị nào dùng theo từng công đoạn
- chỗ nào có giá / không có giá

**1\. PHIẾU ĐÁNH GIÁ VÀ NHẬP KHO NGUYÊN LIỆU ĐẦU VÀO**

**A. Thông tin chung**

- Mã phiếu
- Ngày nhập
- Giờ nhập
- Địa điểm nhận hàng
- Đại diện SSAVIGROUP
- Người lập phiếu
- Bộ phận tiếp nhận

**B. Thông tin nhà cung cấp**

- Mã nhà cung cấp
- Tên nhà cung cấp
- Địa chỉ
- Số điện thoại
- Người giao hàng
- Số điện thoại người giao
- Phương tiện giao hàng
- Biển số xe
- Ghi chú giao hàng

**C. Danh sách nguyên liệu nhập**

**Nguyên liệu tại phiếu này = click chọn từ danh mục master**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Nhóm nguyên liệu** | **Chủng loại / mô tả** | **Số lượng giao** | **Đơn vị** | **Quy cách** | **Tình trạng ban đầu** | **Kết quả đánh giá** | **Lý do không đạt** | **Đề xuất xử lý** | **Ghi chú** |
| ------- | --------- | ------------------- | -------------------- | ---------------------- | ----------------- | ---------- | ------------ | ---------------------- | -------------------- | ------------------- | ----------------- | ----------- |

**Đơn vị chỉ được dùng:**

- kg
- lít
- ml

**D. Khối hạch toán nhập kho**

| **STT** | **Mã NL** | **Số lượng giao** | **Số lượng đạt nhập** | **Số lượng không đạt / hư hỏng** | **Đơn vị** | **Đơn giá nhập** | **Thành tiền nhập** | **Chi phí mua phân bổ** | **Giá trị hư hỏng loại bỏ** | **Đơn giá bình quân sau nhập** | **Ghi chú kế toán** |
| ------- | --------- | ----------------- | --------------------- | -------------------------------- | ---------- | ---------------- | ------------------- | ----------------------- | --------------------------- | ------------------------------ | ------------------- |

**E. Bằng chứng**

- Ảnh toàn bộ lô hàng
- Ảnh từng nguyên liệu
- Video kiểm hàng
- Hóa đơn / phiếu giao hàng
- Biên bản xử lý hàng không đạt

**F. Xác nhận tham gia**

| **Vai trò**          | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| -------------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Đại diện SSAVIGROUP  |            |             |                         |                        |             |
| Người giao hàng      |            |             |                         |                        |             |
| Kho                  |            |             |                         |                        |             |
| QC đầu vào           |            |             |                         |                        |             |
| Kế toán kho / vật tư |            |             |                         |                        |             |

**G. Kết luận**

- Cho nhập kho toàn bộ
- Cho nhập kho một phần
- Không cho nhập kho

**2\. LỆNH SẢN XUẤT**

**A. Thông tin chung**

- Mã hồ sơ sản xuất gốc
- Mã lệnh sản xuất
- Ngày lập
- SKU
- Tên sản phẩm
- Mã công thức
- Version công thức
- Nhóm sản phẩm
- Phân loại Vegan / Mặn
- Số lượng kế hoạch
- Đơn vị kế hoạch
- Ca sản xuất
- Bộ phận sản xuất
- Quản lý nhà máy
- Ghi chú

**B. Dữ liệu công thức tự hiện**

**Tại lệnh sản xuất = không được chọn tay nguyên liệu**  
**Hệ thống tự hiện theo SKU + công thức + version**

**Phần 1 - Dược liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |

**Phần 2 - Nguyên liệu**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Hàm lượng** | **Đơn vị** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------- | ---------- | ----------- |

**C. Xác nhận tham gia**

| **Vai trò**      | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| ---------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Người lập lệnh   |            |             |                         |                        |             |
| Quản lý nhà máy  |            |             |                         |                        |             |
| Bộ phận sản xuất |            |             |                         |                        |             |

**D. Kết luận**

- Mở lệnh sản xuất
- Chưa mở lệnh sản xuất

**3\. PHIẾU ĐỀ NGHỊ CẤP NGUYÊN LIỆU CHO SẢN XUẤT**

**A. Thông tin chung**

- Mã phiếu
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Mã công thức
- Version
- Kho xuất
- Người đề nghị
- Ngày đề nghị

**B. Danh sách nguyên liệu cần cấp**

**Tự hiện từ công thức, không chọn tay**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Nhóm nguyên liệu** | **Hàm lượng công thức** | **Đơn vị** | **Số lượng yêu cầu cấp** | **Ghi chú** |
| ------- | --------- | ------------------- | -------------------- | ----------------------- | ---------- | ------------------------ | ----------- |

**Đơn vị chỉ được dùng:**

- kg
- lít
- ml

**C. Giá trị tham chiếu quản trị / kế toán**

| **STT** | **Mã NL** | **Số lượng yêu cầu cấp** | **Đơn vị** | **Đơn giá bình quân hiện hành** | **Giá trị dự kiến xuất** | **Ghi chú** |
| ------- | --------- | ------------------------ | ---------- | ------------------------------- | ------------------------ | ----------- |

**D. Xác nhận tham gia**

| **Vai trò**     | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| --------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Người đề nghị   |            |             |                         |                        |             |
| Kho             |            |             |                         |                        |             |
| Quản lý nhà máy |            |             |                         |                        |             |

**E. Kết luận**

- Đồng ý cấp phát
- Cấp thiếu
- Chưa đồng ý cấp phát

**4\. PHIẾU CHẤP THUẬN NGUYÊN LIỆU ĐƯỢC PHÉP ĐƯA VÀO SẢN XUẤT**

**A. Thông tin chung**

- Mã phiếu
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Mã công thức
- Version
- Ngày chấp thuận

**B. Danh sách nguyên liệu được duyệt**

**Danh sách này tự kế thừa từ phiếu đề nghị cấp phát**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Lô nguyên liệu** | **Số lượng duyệt xuất** | **Đơn vị** | **Kết quả chấp thuận** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------------ | ----------------------- | ---------- | ---------------------- | ----------- |

**Đơn vị chỉ được dùng:**

- kg
- lít
- ml

**C. Phiếu kế toán xuất đi kèm**

| **STT** | **Mã NL** | **Lô nguyên liệu** | **Số lượng duyệt xuất** | **Đơn vị** | **Đơn giá bình quân** | **Thành tiền xuất cho sản xuất** | **Hao hụt cấp phát** | **Giá trị hao hụt** | **Ghi chú kế toán** |
| ------- | --------- | ------------------ | ----------------------- | ---------- | --------------------- | -------------------------------- | -------------------- | ------------------- | ------------------- |

**D. Xác nhận tham gia**

| **Vai trò**          | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| -------------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Quản lý nhà máy      |            |             |                         |                        |             |
| Kho                  |            |             |                         |                        |             |
| QC                   |            |             |                         |                        |             |
| Kế toán vật tư / kho |            |             |                         |                        |             |

**E. Kết luận**

- Cho phép đưa vào sản xuất
- Chưa cho phép đưa vào sản xuất

**5\. PHIẾU CHECK-IN / CHECK-OUT NHÂN SỰ THEO LỆNH / MẺ / CÔNG ĐOẠN**

**A. Thông tin chung**

- Mã phiếu
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- Mẻ sản xuất
- SKU
- Tên sản phẩm
- Công đoạn
- Ca sản xuất
- Ngày làm việc

**B. Danh sách nhân sự tham gia**

| **STT** | **Mã nhân sự** | **Họ tên** | **Bộ phận** | **Chức danh** | **Đơn giá công / giờ** | **Check-in** | **Check-out** | **Tổng giờ làm** | **Chi phí nhân công phân bổ** | **Ghi chú** |
| ------- | -------------- | ---------- | ----------- | ------------- | ---------------------- | ------------ | ------------- | ---------------- | ----------------------------- | ----------- |

**C. Xác nhận tham gia**

| **Vai trò**                     | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| ------------------------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Tổ trưởng / phụ trách công đoạn |            |             |                         |                        |             |
| Quản lý bộ phận                 |            |             |                         |                        |             |

**D. Kết luận**

- Hoàn tất công đoạn
- Chưa hoàn tất công đoạn

**6\. PHIẾU THEO DÕI SƠ CHẾ**

**A. Thông tin chung**

- Mã phiếu
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Mẻ sản xuất
- Người phụ trách
- Thời gian bắt đầu
- Thời gian kết thúc

**B. Kết quả theo dõi sơ chế**

| **STT** | **Loại bán thành phẩm / lô sơ chế** | **Số lượng đầu vào** | **Đơn vị** | **Số lượng sau sơ chế** | **Đơn vị** | **Hao hụt** | **Đơn vị** | **Đánh giá** | **Ghi chú** |
| ------- | ----------------------------------- | -------------------- | ---------- | ----------------------- | ---------- | ----------- | ---------- | ------------ | ----------- |

**Đơn vị được dùng:**

- khay
- kg
- lít

**C. Bằng chứng**

- Ảnh sơ chế
- Video sơ chế

**D. Xác nhận tham gia**

| **Vai trò**                     | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| ------------------------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Người thực hiện                 |            |             |                         |                        |             |
| Tổ trưởng / phụ trách công đoạn |            |             |                         |                        |             |
| Quản lý bộ phận                 |            |             |                         |                        |             |

**E. Kết luận**

- Đạt
- Không đạt
- Cần xử lý thêm

**7\. PHIẾU KIỂM CHẤT LƯỢNG SAU SẤY THĂNG HOA**

**A. Thông tin chung**

- Mã phiếu
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Mã công thức
- Version
- Mẻ sản xuất
- Ngày kiểm tra

**B. Chỉ tiêu kiểm tra**

| **STT** | **Chỉ tiêu**                     | **Kết quả** | **Đơn vị** | **Đánh giá**    | **Ghi chú** |
| ------- | -------------------------------- | ----------- | ---------- | --------------- | ----------- |
| 1       | Số lượng bán thành phẩm kiểm tra |             | khay       |                 |             |
| 2       | Tỷ lệ % độ ẩm sản phẩm           |             | %          | Đạt / Không đạt |             |
| 3       | Màu sắc cảm quan                 |             |            | Đạt / Không đạt |             |
| 4       | Cấu trúc sau sấy                 |             |            | Đạt / Không đạt |             |
| 5       | Mùi                              |             |            | Đạt / Không đạt |             |
| 6       | Trạng thái bề mặt                |             |            | Đạt / Không đạt |             |

**C. Bằng chứng**

- Ảnh sau sấy
- Video kiểm tra
- File đo độ ẩm nếu có

**D. Xác nhận tham gia**

| **Vai trò**                 | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| --------------------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| QC                          |            |             |                         |                        |             |
| Quản lý nhà máy             |            |             |                         |                        |             |
| Bộ phận phụ trách công đoạn |            |             |                         |                        |             |

**E. Kết luận**

- Đạt cho đóng gói
- Không đạt
- Chờ xử lý

**8\. PHIẾU THEO DÕI ĐÓNG GÓI CẤP 1**

**A. Thông tin chung**

- Mã phiếu
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Mẻ sản xuất
- Người phụ trách

**B. Kết quả đóng gói cấp 1**

| **STT** | **Số lượng đầu vào** | **Đơn vị** | **Số lượng đạt** | **Đơn vị** | **Số lượng không đạt** | **Đơn vị** | **Lý do không đạt** | **Ghi chú** |
| ------- | -------------------- | ---------- | ---------------- | ---------- | ---------------------- | ---------- | ------------------- | ----------- |

**Đơn vị được dùng:**

- gói
- lọ
- hũ

**C. Bằng chứng**

- Ảnh
- Video

**D. Xác nhận tham gia**

| **Vai trò**           | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| --------------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Người thực hiện       |            |             |                         |                        |             |
| Tổ trưởng / phụ trách |            |             |                         |                        |             |
| Quản lý bộ phận       |            |             |                         |                        |             |

**E. Kết luận**

- Đạt
- Không đạt
- Cần xử lý thêm

**9\. PHIẾU THEO DÕI ĐÓNG GÓI CẤP 2**

**A. Thông tin chung**

- Mã phiếu
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Mẻ sản xuất
- Người phụ trách

**B. Kết quả đóng gói cấp 2**

| **STT** | **Số lượng đầu vào** | **Đơn vị** | **Số lượng đầu ra** | **Đơn vị** | **Số lượng không đạt** | **Đơn vị** | **Lý do không đạt** | **Ghi chú** |
| ------- | -------------------- | ---------- | ------------------- | ---------- | ---------------------- | ---------- | ------------------- | ----------- |

**Đơn vị được dùng:**

- hộp
- thùng
- lọ
- hũ

**C. Bằng chứng**

- Ảnh
- Video

**D. Xác nhận tham gia**

| **Vai trò**           | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| --------------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Người thực hiện       |            |             |                         |                        |             |
| Tổ trưởng / phụ trách |            |             |                         |                        |             |
| Quản lý bộ phận       |            |             |                         |                        |             |

**E. Kết luận**

- Đạt
- Không đạt
- Cần xử lý thêm

**10\. PHIẾU QC THÀNH PHẨM**

**A. Thông tin chung**

- Mã phiếu
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Mẻ sản xuất
- Ngày kiểm tra

**B. Kết quả QC**

| **STT** | **Số lượng kiểm tra** | **Đơn vị** | **Số lượng đạt** | **Đơn vị** | **Số lượng không đạt** | **Đơn vị** | **Tỷ lệ đạt** | **Ghi chú** |
| ------- | --------------------- | ---------- | ---------------- | ---------- | ---------------------- | ---------- | ------------- | ----------- |

**Đơn vị phù hợp:**

- gói
- lọ
- hũ
- hộp
- thùng  
   (tùy điểm QC thực tế)

**C. Bằng chứng**

- Ảnh
- Video
- File kiểm tra

**D. Xác nhận tham gia**

| **Vai trò**       | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| ----------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| QC                |            |             |                         |                        |             |
| Quản lý nhà máy   |            |             |                         |                        |             |
| Bộ phận liên quan |            |             |                         |                        |             |

**E. Kết luận**

- Đạt nhập kho
- Không đạt
- Chờ xử lý

**11\. LỆNH NHẬP KHO THÀNH PHẨM**

**A. Thông tin chung**

- Mã lệnh nhập kho
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Kho nhập
- Vị trí kho
- Người nhập
- Thời gian nhập

**B. Kết quả nhập kho**

| **STT** | **Số lượng nhập** | **Đơn vị** | **Ghi chú** |
| ------- | ----------------- | ---------- | ----------- |

**Đơn vị được dùng:**

- hộp
- thùng
- lọ
- hũ

**C. Kiểm soát nhập kho**

| **STT** | **Chỉ tiêu**         | **Kết quả** | **Ghi chú** |
| ------- | -------------------- | ----------- | ----------- |
| 1       | QC thành phẩm đạt    | Có / Không  |             |
| 2       | Quét mã nhập kho     | Có / Không  |             |
| 3       | Ảnh / video nhập kho | Có / Không  |             |

**D. Xác nhận tham gia**

| **Vai trò**     | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| --------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Kho             |            |             |                         |                        |             |
| QC              |            |             |                         |                        |             |
| Quản lý nhà máy |            |             |                         |                        |             |

**E. Kết luận**

- Cho nhập kho
- Chưa cho nhập kho

**12\. PHIẾU KẾ TOÁN XUẤT NGUYÊN LIỆU CHO SẢN XUẤT**

**A. Thông tin chung**

- Mã phiếu kế toán
- Mã hồ sơ sản xuất
- Mã lệnh sản xuất
- SKU
- Tên sản phẩm
- Mẻ sản xuất
- Ngày hạch toán

**B. Bảng hạch toán**

| **STT** | **Mã NL** | **Tên nguyên liệu** | **Lô nguyên liệu** | **Số lượng xuất** | **Đơn vị** | **Đơn giá bình quân** | **Thành tiền xuất** | **Hao hụt** | **Giá trị hao hụt** | **Ghi chú** |
| ------- | --------- | ------------------- | ------------------ | ----------------- | ---------- | --------------------- | ------------------- | ----------- | ------------------- | ----------- |

**Đơn vị được dùng:**

- kg
- lít
- ml

**C. Xác nhận tham gia**

| **Vai trò**          | **Họ tên** | **Bộ phận** | **Trạng thái xác nhận** | **Thời gian xác nhận** | **Ghi chú** |
| -------------------- | ---------- | ----------- | ----------------------- | ---------------------- | ----------- |
| Kế toán vật tư / kho |            |             |                         |                        |             |
| Kho                  |            |             |                         |                        |             |
| Quản lý nhà máy      |            |             |                         |                        |             |

**D. Kết luận**

- Hạch toán hoàn tất
- Chưa hoàn tất
