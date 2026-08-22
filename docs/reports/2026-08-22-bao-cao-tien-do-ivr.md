# BÁO CÁO TIẾN ĐỘ — HỆ THỐNG IVR XÁC NHẬN ĐƠN HÀNG

**Kỳ báo cáo:** 12/08 → 22/08/2026 (11 ngày) · **Người thực hiện:** Nguyễn Quốc Toàn · **Trạng thái an toàn:** đang chạy bằng **dữ liệu giả**, **chưa gọi khách hàng thật**

> *IVR = hệ thống tự động gọi điện cho khách xác nhận đơn: khách bấm phím 1 (đồng ý) / 0 (huỷ), hệ thống ghi kết quả và trả về hệ thống bán hàng.*

## 1. TIẾN ĐỘ

### 1.1 · Mức hoàn thành theo từng hạng mục

| Hạng mục | % xong | Hiện trạng |
| --- | --- | --- |
| Cơ sở dữ liệu (DB) | **90%** | 17 bảng, nhật ký chống sửa, tự xoá dữ liệu hết hạn |
| Backend — lõi xử lý cuộc gọi | **85%** | Nhận đơn → xếp lịch → gọi → nhận phím → trả kết quả |
| Giao diện quản trị (Frontend) | **85%** | 18 màn hình Việt/Anh, 176 bài kiểm thử xanh |
| Giám sát vận hành | **85%** | Bảng theo dõi, cảnh báo, diễn tập sự cố |
| Kiểm thử chất lượng (QA) | **80%** | 650 bài kiểm thử tự động, phủ ~94% mã nguồn |
| Thiết kế giao diện (UI) | **80%** | Đã chuẩn hoá bảng màu, bố cục thống nhất toàn hệ thống |
| Đăng nhập & phân quyền | **80%** | 2 vai trò Quản trị/Vận hành; còn chờ lưu 158 file vào kho |
| Đóng gói & triển khai | **75%** | Đóng gói máy chủ, phát hành từng bước, quay lui được |
| Trải nghiệm sử dụng (UX) | **75%** | Đã kiểm khả năng tiếp cận và song ngữ |
| Kết nối tổng đài | **55%** | Phần mềm xong, đã gọi thật trong lab; **chờ SIM + thiết bị** |
| Giọng nói & kịch bản gọi | **45%** | 1 giọng đã duyệt; còn 3 giọng vùng miền + cách đọc số tiền |
| Hồ sơ pháp lý & nghiệm thu | **40%** | Hồ sơ đã soạn xong, **chờ chữ ký** |
| Nối hệ thống bán hàng | **35%** | Xong với bản giả lập; **chờ API thật từ đội Sales** |
| **TỔNG THỂ** | **~80%** | Chỉ tính tới mức tự kiểm đạt, **chưa tính phần nghiệm thu** |

### 1.2 · Con số tổng quan

**105 hạng mục công việc:** 64 kiểm thử đạt (chờ nghiệm thu) · 19 đã nộp bằng chứng chờ ký · **15 bị chặn bởi bên ngoài** · 5 đã nghiệm thu · 2 cố ý hoãn (tính năng gửi SMS, bản 1 không làm). **Khối lượng:** 68 lần lưu công việc · ~185.000 dòng nội dung · 650 bài kiểm thử xanh · phủ ~94% mã nguồn · 72 bộ hồ sơ bằng chứng · 390 file tài liệu · 18 màn hình.

## 2. NHỮNG VIỆC ĐÃ LÀM

| Ngày | Lưu | Nhóm việc | Kết quả |
| --- | --- | --- | --- |
| 12/08 | 11 | Dựng nền móng | Nạp 386 file tài liệu; dựng khung phần mềm 5 lớp; dây chuyền kiểm tra chất lượng tự động 6 chốt; công tắc dừng khẩn cấp; cơ sở dữ liệu 17 bảng |
| 13/08 | 27 | Máy chủ thật + lõi vận hành | Dựng máy chủ kiểm thử riêng, khoá nhánh mã nguồn chính. Xong phần nhận đơn, kiểm điều kiện, xếp lịch gọi, quản lý kịch bản gọi |
| 14/08 | 11 | Hoàn tất lõi + sửa lỗi | Xong đọc phím bấm, trả kết quả về Sales, 13 chức năng quản trị nội bộ. Tự rà soát và **tự tìm ra 23 lỗi**, sửa hết trong ngày |
| 15/08 | 2 | Màn hình quản trị | Bảng điều khiển, nhật ký cuộc gọi, chi tiết cuộc gọi, cấu hình, báo cáo, phân quyền + chuẩn thiết kế thống nhất |
| 18–19/08 | 4 | Tích hợp, kiểm thử, giám sát | Nối hệ thống bán hàng (giả lập), kiểm tồn kho trước khi gọi, kiểm hiệu năng/bảo mật/quyền riêng tư, bảng giám sát và cảnh báo |
| 20/08 | 6 | Đóng gói triển khai | Đóng gói máy chủ đám mây, phát hành từng bước quay lui được, sao lưu & khôi phục thảm hoạ. Đóng đợt kiểm toán toàn hệ thống |
| 20–22/08 | 5 | Tổng đài thử nghiệm + giọng nói | Dựng tổng đài miễn phí, **gọi ra máy thật, nghe được giọng đọc đơn, bấm 1/0 ghi đúng kết quả**. Sếp chọn giọng C, chốt lời chào → **đã nghiệm thu** |
| 22/08 | *chưa lưu* | Đăng nhập & phân quyền | Tài khoản/mật khẩu thật, 2 vai trò, quản lý tài khoản, thu hồi phiên. Kiểm thử đạt 474 + 176 bài |


## 3. DỰ KIẾN HOÀN THÀNH

| Tuần | Thời gian | Nội dung | Kết quả bàn giao |
| --- | --- | --- | --- |
| **T4/8** | 24–30/08 | Lưu phần đăng nhập; làm bộ đọc số tiền thành chữ; dựng 3 giọng vùng miền Bắc/Trung/Nam | Xong phần tự làm, sẵn sàng nghiệm thu |
| **T1/9** | 01–06/09 | Nghiệm thu đợt 1: lõi xử lý, cơ sở dữ liệu, giao diện quản trị. Lắp SIM + thiết bị, chạy thử cuộc gọi thật | Biên bản nghiệm thu đợt 1 |
| **T2/9** | 07–13/09 | Nghiệm thu đợt 2: kiểm thử, giám sát, triển khai. Nối API thật của hệ thống bán hàng | Biên bản đợt 2 + kết nối thật |
| **T3/9** | 14–20/09 | Chạy thử toàn tuyến: đơn thật từ Sales → gọi qua SIM thật → trả kết quả. Sửa lỗi phát sinh | Báo cáo chạy thử toàn tuyến |
| **T4/9** | 21–27/09 | Hoàn tất hồ sơ pháp lý, bảo mật, kịch bản gọi; ký duyệt phát hành | Hồ sơ ký duyệt đầy đủ |
| **T5/9** | 28–30/09 | Bàn giao, chạy chính thức có giám sát | **Hệ thống vận hành thật** |

**Điều kiện để giữ được lịch này:** SIM + thiết bị gọi có trước **01/09**; đội Sales bàn giao API thật trước **07/09**; pháp chế duyệt kịch bản trước **21/09**. Chậm khâu nào, các tuần sau lùi theo đúng khâu đó.

## 4. KHÓ KHĂN & CẦN GÌ ĐỂ HOÀN THIỆN

### 4.1 · Ba việc cần sếp quyết ngay

| Ưu tiên | Việc cần làm | Ai làm | Hạn |
| --- | --- | --- | --- |
| 🔴 1 | Cấp **1 SIM test + 1 thiết bị GSM gateway**, duyệt 2–3 số nội bộ để gọi thử | Sếp / Hạ tầng | trước 01/09 |
| 🔴 2 | Thúc đội Sales điền **phiếu đầu vào** (đã soạn sẵn, chỉ cần điền) | Sếp | trước 07/09 |
| 🔴 3 | Chỉ định **người nghiệm thu** và lịch nghiệm thu theo đợt | Sếp | trước 01/09 |
| 🟠 4 | Duyệt ngân sách mua gói **giọng đọc có bản quyền thương mại** | Sếp | trước 07/09 |
| 🟠 5 | Pháp chế duyệt kịch bản gọi + thời gian lưu dữ liệu | Pháp chế | trước 21/09 |
| 🟡 6 | Cấp máy chủ thật (hoặc chọn hạ tầng đơn giản hơn) | Hạ tầng | trước 14/09 |

### 4.2 · Vướng mắc đang chặn

| Vướng mắc | Mô tả | Hậu quả nếu không gỡ |
| --- | --- | --- |
| **Chưa có SIM & thiết bị gọi** | Hiện chỉ gọi được trong phòng lab bằng phần mềm miễn phí | Không chứng minh được hệ thống gọi khách được |
| **Chưa có API thật của Sales** | Đang chạy với hệ thống bán hàng giả lập | Không nhận được đơn thật, không trả kết quả thật |
| **Thiếu người nghiệm thu** | 100/105 hạng mục "tự kiểm thì đạt" nhưng chưa ai ký nhận | Không hạng mục nào được công nhận hoàn thành |
| **Bản quyền giọng đọc** | Giọng bản miễn phí **không được dùng thương mại**; hết hạn 31/12/2026 | Rủi ro pháp lý khi chạy thật |
| **Cách đọc số tiền chưa khớp** | Hệ thống sinh "560.000 đồng" dạng chữ số, audio đã duyệt đọc thành chữ | Khách nghe sai/khó hiểu số tiền |
| **Chỉ có 1 người làm** | Không có người review độc lập | Rủi ro lỗi lọt lưới |

### 4.3 · Rủi ro đã có sẵn biện pháp phòng

| Rủi ro | Biện pháp đã làm |
| --- | --- |
| Gọi nhầm khách thật khi chưa được phép | Khoá cứng nhiều lớp, chỉ gọi số trong danh sách trắng, có công tắc dừng khẩn cấp |
| Lộ số điện thoại / thông tin khách | Che số ở mọi nhật ký, màn hình, báo cáo; có bộ quét tự động chặn rò rỉ |
| Nhầm "lỗi kỹ thuật" thành "khách không nghe máy" | Đã tách riêng; lỗi kỹ thuật không bị tính là một lần gọi khách |

## 5. KẾT LUẬN

| Câu hỏi | Trả lời |
| --- | --- |
| Phần mềm làm được chưa? | **Rồi** — đã gọi, đọc, nhận phím, ghi kết quả trong phòng thử nghiệm. Tổng thể **~80%** |
| Còn thiếu gì? | SIM + thiết bị gọi, API thật của Sales, chữ ký nghiệm thu và pháp lý |
| Bao giờ vận hành thật? | **Cuối tháng 9/2026**, nếu 3 việc ở mục 4.1 được quyết đúng hạn |
| Cần sếp làm gì ngay? | Cấp SIM + thiết bị · thúc Sales trả phiếu đầu vào · chỉ định người nghiệm thu |

*Lập ngày 22/08/2026 · Số liệu đếm trực tiếp từ 68 lần lưu công việc trong kho mã nguồn và sổ tiến độ nội bộ, không ước lượng.*
