# Đề xuất phương án gọi điện xác nhận đơn hàng tự động

Ngày lập: **15/09/2026** · Trình lãnh đạo quyết định.

## Kết luận

**Đề xuất chọn Mobile SIP Trunk của một nhà mạng duy nhất, đăng ký 8 kênh, mở rộng sau khi có số liệu thật.**

| | **A — Mobile SIP Trunk** *(đề xuất)* | B — Gateway 4G + 32 SIM |
| --- | --- | --- |
| Hiện tên công ty khi gọi khách | **Được** | **Không có lời giải** |
| Tiền mặt bỏ ra ban đầu | **~1 triệu** | **~88 triệu** |
| Tiền mặt năm đầu | **~37 triệu** | **~135 triệu** |
| Bắt đầu nhỏ, mở rộng dần | **Được** — nâng kênh bằng phụ lục hợp đồng | **Không** — mua 32 cổng ngay từ đầu |
| Thời gian hoàn thành | **4–6 tuần** | 5–8 tuần |
| Phải nuôi thiết bị, SIM | Không | 1 hộp máy + 32 thuê bao |

### Ba lý do

**1. Yêu cầu bắt buộc là hiện tên công ty — gateway không làm được.**

Khách nhận cuộc gọi phải thấy tên công ty, không phải một dãy số lạ. Tên này do nhà mạng cấp và gắn vào số thuê bao của công ty. Gateway 4G cắm 32 SIM thường — loại SIM này không gắn được tên định danh. Nghĩa là: **bỏ 88 triệu mua thiết bị xong vẫn không đạt yêu cầu ban đầu**, dù cuộc gọi vẫn thực hiện được bình thường.

**2. Chưa ai biết giờ cao điểm sẽ có bao nhiêu cuộc — nên đừng mua trước.**

Website bán hàng chưa chạy, livestream chưa bắt đầu. Con số "32 kênh" hiện tại là ước lượng, chưa có một phiên bán hàng thật nào để đo. Mua hộp gateway 32 cổng lúc này là **đặt cọc 78 triệu vào một con số chưa ai biết đúng hay sai**.

Thuê đường truyền thì ngược lại: đăng ký 8 kênh, chạy vài phiên livestream thật, đo xem đỉnh thực tế bao nhiêu, rồi nâng lên 16 hay 32 bằng một phụ lục hợp đồng. Không mua thêm máy móc, không lắp đặt lại.

**3. Bỏ ra khoảng 1 triệu thay vì 88 triệu — và nếu sai hướng thì dừng được.**

Hợp đồng thuê kết thúc được khi không dùng nữa. Hộp gateway 78 triệu đã mua thì nằm đó, bán lại mất giá nhiều. Ở giai đoạn còn nhiều điều chưa chắc chắn, **trả tiền theo mức thực dùng an toàn hơn trả trước một lần**.

**Và chỉ dùng một nhà mạng — không chia cho hai.** Chia lưu lượng cho hai nhà mạng để hưởng giá nội mạng nghe hợp lý, nhưng tính ra thì **lỗ cho tới khi đạt 22.000 đơn/tháng**. Việc đáng làm là chọn đúng một nhà mạng ngay từ đầu — tác dụng lớn gấp gần 7 lần mà không tốn thêm đồng nào. Chi tiết ở §7.

> **Khi nào nên xem lại phương án B?** Khi có đủ cả bốn: lượng đơn lớn và ổn định đã đo được; nhà mạng xác nhận một gói SIM doanh nghiệp rẻ hơn hẳn được phép dùng cho gọi tự động; có cách hiện tên công ty, hoặc lãnh đạo quyết định bỏ yêu cầu đó; và báo giá thật cho thấy tiết kiệm đủ bù tiền thiết bị. **Hiện chưa có điều nào.**

## 1. Việc cần làm là gì

Khách đặt hàng trên website hoặc livestream → hệ thống tự gọi điện cho khách → đọc thông tin đơn → khách **bấm 1 để xác nhận, bấm 0 để hủy** → kết quả trả về hệ thống đơn hàng.

Đây là **gọi ra**, không phải tổng đài nhận cuộc gọi của khách. Số dùng để gọi xác nhận tách riêng với hotline công ty đang dùng.

**Một số từ dùng trong báo cáo:**

| Từ | Nghĩa |
| --- | --- |
| **Kênh** | Một kênh = một cuộc gọi cùng lúc. 8 kênh nghĩa là gọi được cho 8 khách cùng một thời điểm. |
| **Mobile SIP Trunk** | Thuê nhà mạng một đường thoại chạy qua Internet, nối thẳng vào tổng đài của công ty. Không cần SIM, không cần thiết bị. |
| **Gateway 4G** | Một hộp máy cắm 32 SIM đặt tại công ty, biến cuộc gọi của tổng đài thành cuộc gọi phát ra từ SIM. |
| **Tên định danh** *(Voice Brandname)* | Tên công ty hiện trên màn hình điện thoại khách thay vì dãy số lạ. Do nhà mạng cấp, gắn vào một số cụ thể. |

## 2. Hai phương án khác nhau chỗ nào

| | A — Mobile SIP Trunk | B — Gateway 4G + 32 SIM |
| --- | --- | --- |
| **Cách hoạt động** | Thuê nhà mạng đường thoại qua Internet | Mua hộp máy, cắm 32 SIM, đặt tại công ty |
| **Hiện tên công ty** | Đăng ký với nhà mạng là có | Không gắn được vào SIM thường |
| **Số gọi ra** | Một số công ty, dùng cho mọi cuộc | 32 SIM là 32 số khác nhau |
| **Thiết bị phải mua** | Không | Hộp gateway, 32 SIM, anten, bộ lưu điện |
| **Muốn tăng kênh** | Báo nhà mạng, ký phụ lục | Mua thêm cổng hoặc thêm hộp máy |
| **Muốn giảm quy mô** | Giảm kênh theo hợp đồng | Thiết bị đã mua vẫn nằm đó |
| **Thiết bị viễn thông phải nuôi** | Không có — nhà mạng lo toàn bộ đường thoại | Hộp gateway và 32 SIM: điện, sóng, hạn dùng SIM, sửa chữa |
| **Nếu hỏng** | Nhà mạng chịu trách nhiệm theo hợp đồng | Hộp hỏng là mất toàn bộ 32 kênh |

**Phương án B có lợi thế gì không?** Có một: nếu gọi khối lượng rất lớn và mua được gói SIM doanh nghiệp giá rẻ, cước mỗi phút có thể thấp hơn. Nhưng theo giá công khai hiện tra được thì **SIM thường đang đắt hơn trunk** (1.300đ so với 880đ mỗi phút), nên lợi thế này chưa tồn tại trên giấy tờ.

## 3. Cần bao nhiêu kênh

Chưa chạy website và livestream thì chưa đo được đỉnh thật. Bảng dưới để **hình dung quy mô**, không phải con số chốt.

Giả định: mỗi cuộc chiếm một kênh khoảng 50 giây (quay số, đổ chuông, nói chuyện, kết thúc); gọi trong 12 tiếng mỗi ngày; giờ cao điểm đông gấp 4 lần trung bình.

| Đơn/tháng | Đơn/ngày | **Kênh cần** |
| --- | --- | --- |
| 10.000 | 333 | **2** |
| 30.000 | 1.000 | **6** |
| 60.000 | 2.000 | **12** |
| 100.000 | 3.333 | **19** |
| 170.000 | 5.667 | **32** |

**32 kênh là cỡ phục vụ khoảng 170.000 đơn mỗi tháng.** Đó là quy mô rất lớn so với giai đoạn khởi động.

**Hai điều làm giảm số kênh cần thiết:**

- **Đơn xác nhận không bắt buộc gọi ngay lập tức.** Livestream tạo 500 đơn trong 30 phút thì không cần 500 kênh — hệ thống xếp hàng và gọi rải ra trong khung giờ cho phép. Chỉ khi khách cần xác nhận trong vài phút mới cần nhiều kênh.
- **Có thể nâng kênh sau.** Đăng ký 8 kênh, chạy 2–3 phiên livestream thật, xem đỉnh bao nhiêu rồi quyết.

### Lộ trình đề xuất

| Giai đoạn | Kênh | Khi nào chuyển |
| --- | --- | --- |
| Chạy thử | **4** | Ngay khi nhà mạng mở đường |
| Vận hành thật | **8** | Sau khi chạy thử đạt chất lượng |
| Mở rộng | **16** | Khi đỉnh đo được chạm 70% của 8 kênh |
| Cao điểm | **32** | Khi đỉnh đo được chạm 70% của 16 kênh |

Phần mềm chỉ làm **một lần** cho mọi mức — số kênh là một tham số cấu hình. Nâng từ 8 lên 32 không phải viết lại gì.

> **Ba câu phải ghi vào hợp đồng ngay từ đầu:** giá của từng mức kênh 4/8/16/32; **nâng kênh mất bao nhiêu ngày**; có bị phạt khi giảm kênh xuống không. Không chốt ba điều này thì lợi thế "mở rộng dễ" chỉ là lời hứa.

## 4. Chi phí

### 4.1. Phân biệt hai loại chi phí

Báo cáo tách rõ hai thứ hay bị cộng nhầm vào nhau:

- **Tiền mặt phải chi:** thực sự chuyển ra khỏi công ty — cước nhà mạng, thiết bị phải mua, lệ phí. Máy chủ công ty đã có nên không nằm trong nhóm này.
- **Công nội bộ:** thời gian của đội kỹ thuật. Đội đang làm sẵn trong công ty, **không phát sinh hóa đơn**, nên để riêng và tính bằng ngày.

Các con số dưới đây là **tiền mặt**. Phần công nội bộ nằm ở §5.

### 4.2. Tiền bỏ ra ban đầu

**A — Mobile SIP Trunk**

| Khoản | Số tiền | Căn cứ |
| --- | --- | --- |
| Lệ phí tên định danh | **200.000đ** | Mức chính thức theo Nghị định 91/2020 |
| Đăng ký số và gán tên tại nhà mạng | **~800.000đ** | Tham chiếu Nhân Hòa: 60.000đ đăng ký số + 500.000đ gán tên |
| Địa chỉ Internet cố định | **0đ** | **Đã có sẵn** — gói VNPT Fiber Xtra2 công ty đang dùng đã kèm 1 địa chỉ cố định. Đã kiểm tra và xác nhận |
| Đặt cọc hoặc phí hỗ trợ tích hợp | **chưa biết** | Phải hỏi nhà mạng — đã có trong bộ câu hỏi ở §9 |
| **Tổng ước tính** | **~1 triệu** | Chưa gồm đặt cọc nếu nhà mạng yêu cầu |

**B — Gateway 32 SIM**

| Khoản | Số tiền | Căn cứ |
| --- | --- | --- |
| Hộp gateway 32 cổng | **78,1 triệu** | Giá niêm yết Dinstar UC2000-VG-32T-B |
| 32 bộ SIM | **1,6 triệu** | 50.000đ/bộ × 32, giá bán lẻ |
| Bộ lưu điện, anten, phụ kiện | **~5 triệu** | Giả định — chưa có báo giá |
| Công lắp đặt của nhà cung cấp | **~3 triệu** | Giả định — chưa có báo giá |
| **Tổng ước tính** | **~88 triệu** | Trong đó 79,7 triệu là giá niêm yết, 8 triệu là giả định |

**Chênh lệch ngay từ đầu: gần 87 triệu.**

Công ty đã có sẵn máy chủ và đường mạng, nên phương án A gần như không phải mua gì — chỉ trả lệ phí giấy tờ và phí mở dịch vụ. Phần lớn chi phí của phương án B là **một món đồ phải mua đứt trước khi biết có cần đến nó hay không**.

### 4.3. Chi phí hàng tháng theo ba mức

Chưa vận hành nên chưa có số thật. Ba kịch bản để lãnh đạo thấy khoảng dao động:

| | **Khởi động**<br>3.000 đơn/tháng | **Dự kiến**<br>10.000 đơn/tháng | **Cao điểm**<br>30.000 đơn/tháng |
| --- | --- | --- | --- |
| Số phút gọi | 1.260 phút | 4.200 phút | 12.600 phút |
| **A — Mobile SIP Trunk** | **1,7 triệu** | **4,3 triệu** | **11,7 triệu** |
| *trong đó tiền thoại* | *1,1 triệu* | *3,7 triệu* | *11,1 triệu* |
| **B — Gateway 32 SIM** | **2,0 triệu** | **5,9 triệu** | **16,8 triệu** |
| *trong đó tiền thoại* | *1,6 triệu* | *5,5 triệu* | *16,4 triệu* |

**Công ty đã có máy chủ riêng**, nên không phát sinh tiền thuê máy chủ, và phần tạo giọng đọc cũng chạy trên máy của mình. Hai bảng trên chỉ cộng thêm **300.000đ/tháng** cho tiền điện và sao lưu. Phương án B cộng thêm tiền điện cho hộp gateway.

Cách tính số phút: mỗi đơn gọi trung bình 1,2 lần; 70% số lần gọi có khách bắt máy và bị tính cước; mỗi cuộc tính 30 giây.

> **Bốn điều cần kiểm trên máy chủ hiện có.** Dùng máy của công ty tiết kiệm được tiền thuê, nhưng đổi lại công ty tự chịu trách nhiệm vận hành:
>
> 1. **Địa chỉ Internet cố định — đã có, không tốn thêm tiền.** Nhà mạng xác thực đường thoại bằng cách ghi sẵn địa chỉ Internet của công ty vào danh sách cho phép, nên địa chỉ đó bắt buộc phải cố định. Gói **VNPT Fiber Xtra2** công ty đang dùng đã kèm sẵn một địa chỉ cố định — đã kiểm tra và xác nhận. **Việc còn lại chỉ là báo địa chỉ này cho nhà mạng khi đăng ký.**
> 2. **Phải mở đường cho âm thanh đi vào.** Máy chủ nằm sau thiết bị định tuyến của công ty. Nếu không cấu hình đúng, cuộc gọi vẫn đổ chuông và khách vẫn bắt máy, nhưng **hai bên không nghe thấy nhau** — đây là lỗi thường gặp nhất khi đấu tổng đài nội bộ ra nhà mạng. Là việc cấu hình, **không tốn tiền**, đã nằm trong phần ngày công ở Mục 5.
> 3. **Cấu hình máy còn đủ không.** Máy phải chạy cùng lúc tổng đài, cơ sở dữ liệu và phần tạo giọng đọc. Cần kiểm tra phần còn trống trước khi chốt.
> 4. **Máy hỏng thì xử lý thế nào.** Máy của công ty thì công ty tự lo thay thế. Cần chuẩn bị trước: bản sao cấu hình, linh kiện hoặc máy dự phòng, và người xử lý khi sự cố xảy ra ngoài giờ.

### 4.4. Năm đầu tiên

Thực tế lượng đơn sẽ tăng dần, không cao ngay từ tháng đầu. Giả sử **6 tháng đầu ở mức khởi động, 6 tháng sau ở mức dự kiến**:

| | A — Mobile SIP Trunk | B — Gateway 32 SIM |
| --- | --- | --- |
| Chi phí vận hành 12 tháng | 36,0 triệu | 47,4 triệu |
| Tiền bỏ ra ban đầu | ~1 triệu | 87,7 triệu |
| **Tổng năm đầu** | **~37 triệu** | **~135 triệu** |

**Chênh lệch năm đầu: khoảng 98 triệu.**

Nếu lượng đơn tăng nhanh và duy trì mức cao điểm suốt cả năm: A khoảng **141 triệu**, B khoảng **289 triệu**.

### 4.5. Ba điều có thể làm số tiền thay đổi

1. **Cách nhà mạng tính cước.** Nếu cuộc 30 giây bị tính tròn thành 60 giây, tiền thoại tăng gấp đôi. Phải hỏi rõ tính theo giây hay làm tròn phút.
2. **Phí hiện tên tính riêng.** Một số nhà mạng thu thêm phí cho mỗi cuộc có hiện tên công ty. Phải hỏi phí này đã nằm trong cước phút chưa.
3. **Giá gọi ngoại mạng.** Khách dùng mạng khác với mạng công ty đăng ký thì cước cao hơn — thường gần gấp đôi. Đây là khoản lớn nhất có thể tối ưu, và cách làm là **chọn đúng nhà mạng ngay từ đầu** (§7), không phải đăng ký thêm nhà mạng.

*Giá tham chiếu đang dùng: 700đ/phút nội mạng và 1.000đ/phút ngoại mạng cho phương án A — là mức đặt vào mô hình dựa trên các bảng giá công khai, **chưa có nhà mạng nào cam kết**. Giá SIM thường 1.180–1.380đ/phút lấy từ [MobiCard](https://5g.mobifone.vn/dich-vu-di-dong/loai-thue-bao/mobicard-3). Giá thiết bị 78,1 triệu là mức [niêm yết Dinstar UC2000-VG-32T-B](https://namlong.vn/dinstar/tong-dai/thiet-bi-gsm-gateway-32-sim-dinstar-uc2000-vg-32t-b-28198.html). Con số chính thức phải chờ báo giá ký.*

## 5. Việc còn lại và thời gian

Hệ thống đã có sẵn phần lớn: nhận đơn cần gọi, xếp lịch gọi, đọc nội dung đơn bằng giọng nói, nhận phím 1/0 của khách, trả kết quả về hệ thống đơn hàng. Đã chạy thử thành công trên máy nội bộ.

Phần chưa xong là **nối ra mạng viễn thông thật** và **gọi nhiều cuộc cùng lúc**.

### 5.1. Phần chung — phương án nào cũng phải làm

| Việc | Ngày công |
| --- | --- |
| Hoàn thiện đường gọi ra thật và cơ chế bảo vệ số điện thoại khách | 3–5 |
| Làm phần gọi nhiều cuộc cùng lúc và tự phục hồi khi lỗi | 3–5 |
| Nối với hệ thống đơn hàng thật và dựng môi trường vận hành | 4–6 |
| Tài liệu vận hành và bàn giao | 1–2 |
| **Cộng** | **11–18** |

Phần này chiếm quá nửa khối lượng. **Mua gateway không rút ngắn được phần này.**

### 5.2. Phần riêng

| A — Mobile SIP Trunk | Ngày | B — Gateway 32 SIM | Ngày |
| --- | --- | --- | --- |
| Cấu hình đường thoại với nhà mạng | 2–3 | Cấu hình thiết bị, 32 cổng, 32 SIM, anten | 4–7 |
| Theo dõi đường truyền và đối chiếu hóa đơn | 2–3 | Theo dõi 32 SIM: số dư, hạn dùng, sóng, lỗi cổng | 4–6 |
| Chạy thử tải, thử lỗi, nghiệm thu | 4–6 | Chạy thử tải, thử lỗi, nghiệm thu, thêm lỗi phần cứng | 5–8 |
| **Cộng** | **8–12** | **Cộng** | **13–21** |

### 5.3. Tổng

| | A — Mobile SIP Trunk | B — Gateway 32 SIM |
| --- | --- | --- |
| **Tổng ngày công** | **19–30 ngày** | **24–39 ngày** |
| **Quy ra thời gian** | **~4–6 tuần** | **~5–8 tuần** |
| Gọi thử được cuộc đầu tiên | 2–4 ngày sau khi có đường | 3–5 ngày sau khi có thiết bị và SIM |

Đây là **thời gian làm việc của đội kỹ thuật nội bộ**, không phải hóa đơn thuê ngoài. Nếu phải thuê ngoài với giá 1,5 triệu/ngày thì tương đương 28,5–45 triệu cho A và 36–58,5 triệu cho B.

Nếu chạy thử 4–8 kênh trước thay vì nghiệm thu đủ 32 ngay, phần thử tải giảm khoảng 2 ngày ở cả hai phương án.

### 5.4. Thời gian chờ bên ngoài — tính riêng

| A — Mobile SIP Trunk | B — Gateway 32 SIM |
| --- | --- |
| Xin giấy tên định danh: **1 ngày làm việc** kể từ khi hồ sơ hợp lệ (§8) | Đặt và nhận thiết bị: **chưa có lịch giao** |
| Nhà mạng mở đường và gán tên: **phải yêu cầu ngày cụ thể** | Hòa mạng 32 SIM và kích hoạt: **chưa xác nhận gói nào được phép gọi tự động** |
| | **Vẫn phải xin giấy tên định danh** nếu muốn hiện tên — không tiết kiệm được bước này |

Các đầu vào từ hệ thống đơn hàng và bộ phận hạ tầng là **đường găng chung của cả hai phương án**. Mua gateway không làm chúng đến sớm hơn.

Phần chuẩn bị chạy song song được, nên **không cộng dồn ngày chờ với ngày làm việc**.

## 6. Rủi ro và cách xử lý

| Rủi ro | Phương án A | Phương án B |
| --- | --- | --- |
| **Không hiện được tên công ty** | Có giấy chưa chắc hiện ở mọi mạng — phải thử trên điện thoại thật từng nhà mạng và ghi phạm vi vào hợp đồng | **Chưa có giải pháp.** Không xử lý được thì không đạt yêu cầu ban đầu |
| **Chi phí vượt dự toán** | Phí hiện tên tính theo cuộc, cam kết tối thiểu, cách làm tròn cước. Hỏi rõ trước khi ký, đặt hạn mức cảnh báo | Hết ưu đãi từng SIM, phút mua không dùng hết, phải theo dõi 32 thuê bao riêng |
| **Dừng toàn bộ hệ thống** | Nhà mạng hoặc đường Internet hỏng. Có cảnh báo, xếp đơn vào hàng chờ gọi lại, và yêu cầu nhà mạng cam kết thời gian khắc phục trong hợp đồng | Mất điện, hộp máy treo, sóng yếu — mất cả 32 kênh. Cần bộ lưu điện và máy dự phòng |
| **Khách bấm phím mà máy không nhận** | Do cấu hình âm thanh. Thử kỹ trước khi chạy thật | Qua thêm một lớp chuyển đổi nên khả năng sai cao hơn. Yêu cầu nhà cung cấp thử trước |
| **Hiểu sai kết quả cuộc gọi** | Máy bận, hộp thư thoại có thể bị hiểu nhầm là khách đã nghe. Đối chiếu với bảng kê của nhà mạng; **chỉ phím hợp lệ mới tính là phản hồi** | Cùng rủi ro, thêm trạng thái báo lỗi của thiết bị. Không hủy đơn vì SIM gặp sự cố |
| **Người vận hành** | Cần người trực và đầu mối nhà mạng | Cần thêm người xử lý tại chỗ khi thiết bị hỏng |

*Đánh giá để lập kế hoạch, chưa phải thống kê sự cố đã đo.*

## 7. Chỉ dùng một nhà mạng — và cách chọn đúng

**Đề xuất: đăng ký một nhà mạng duy nhất và giữ như vậy. Không chia lưu lượng cho hai nhà mạng.**

Phương án hai nhà mạng nghe hợp lý — khách mạng nào thì gọi bằng số mạng đó để hưởng giá nội mạng. Nhưng khi tính ra tiền thì **không đủ bù chi phí phát sinh**.

### 7.1. Tại sao hai nhà mạng không đáng

**Khoản tiết kiệm rất nhỏ.** Thêm nhà mạng thứ hai chỉ kéo đơn giá bình quân từ 880đ xuống 820đ mỗi phút — tiết kiệm **60đ/phút**:

| Mức vận hành | Tiết kiệm được |
| --- | --- |
| Khởi động — 3.000 đơn/tháng | 76.000đ/tháng |
| Dự kiến — 10.000 đơn/tháng | 252.000đ/tháng |
| Cao điểm — 30.000 đơn/tháng | 756.000đ/tháng |

**Chi phí phát sinh lại lớn hơn:**

| Khoản | Số tiền |
| --- | --- |
| Duy trì số và tên định danh thứ hai | 300.000đ/tháng |
| Công kỹ thuật làm phần chọn nhà mạng (~4–6 ngày, ~6 triệu), chia đều 24 tháng | 250.000đ/tháng |
| Phí tra cứu xem khách thuộc mạng nào | Chưa có báo giá |
| **Cộng, chưa tính phí tra cứu** | **550.000đ/tháng** |

**Kết quả lãi lỗ:**

| Mức vận hành | Lãi / lỗ mỗi tháng |
| --- | --- |
| Khởi động — 3.000 đơn | **−474.000đ — lỗ** |
| Dự kiến — 10.000 đơn | **−298.000đ — lỗ** |
| Cao điểm — 30.000 đơn | +206.000đ |

**Phải đạt khoảng 22.000 đơn/tháng mới hòa vốn.** Và ngay cả ở mức 30.000 đơn — mức cao nhất đang dự kiến — khoản lãi chỉ **206.000đ/tháng**, tức khoảng 2,5 triệu một năm. Không đáng để đổi lấy hai hợp đồng, hai bộ hồ sơ tên định danh, hai bảng kê cước phải đối chiếu, và hai đầu mối kỹ thuật phải làm việc mỗi khi có sự cố.

### 7.2. Chọn đúng nhà mạng quan trọng gấp nhiều lần

Đây mới là chỗ tiết kiệm thật. Cước ngoại mạng chênh nhau rất nhiều giữa các nhà mạng, nên **đăng ký đúng nhà mạng mà phần lớn khách đang dùng** có tác dụng lớn hơn hẳn việc thêm nhà mạng thứ hai.

Ví dụ minh họa, giả sử cơ cấu khách là 50% Viettel, 25% VinaPhone, 20% MobiFone, 5% mạng khác:

| Đăng ký ở | Tỷ lệ khách cùng mạng | Đơn giá bình quân |
| --- | --- | --- |
| **Viettel** | 50% | **800đ/phút** |
| MobiFone | 20% | 1.204đ/phút |

| So sánh | Tiết kiệm |
| --- | --- |
| **Chọn đúng nhà mạng** | **404đ/phút** |
| Thêm nhà mạng thứ hai | 60đ/phút |

**Chọn đúng nhà mạng quan trọng gấp gần 7 lần — và không tốn thêm một đồng nào**, vì vẫn chỉ là một hợp đồng.

Ở mức 10.000 đơn/tháng: chọn đúng nhà mạng tiết kiệm **1,7 triệu/tháng**, trong khi thêm nhà mạng thứ hai chỉ được 252.000đ — mà còn lỗ sau khi trừ chi phí.

*Giá tham chiếu lấy từ [bảng giá Callio](https://callio.vn/bang-gia/): Viettel 650đ nội mạng / 950đ ngoại mạng; MobiFone 700đ / 1.330đ. Đây là giá của đơn vị bán lại, không phải báo giá trực tiếp của nhà mạng. Cơ cấu khách là giả định minh họa — phải đếm từ dữ liệu đơn hàng thật.*

### 7.3. Dồn lưu lượng vào một hợp đồng còn giúp ép giá

Chia đôi lưu lượng cho hai nhà mạng nghĩa là cam kết với mỗi bên chỉ một nửa. Nhà mạng tính giá theo bậc sản lượng, nên **chia nhỏ ra thì bậc giá của cả hai đều xấu đi**. Dồn toàn bộ vào một hợp đồng cho vị thế đàm phán tốt hơn và có thể xin được mức thấp hơn bảng giá niêm yết.

### 7.4. Cách chọn nhà mạng — bốn bước

1. **Đếm cơ cấu mạng của khách.** Lấy 1.000–2.000 số điện thoại khách gần nhất, đếm theo đầu số. Đầu số không chính xác tuyệt đối vì khách chuyển mạng vẫn giữ số, nhưng đủ để ước lượng.
2. **Xin báo giá cùng một cấu hình** từ cả VinaPhone, MobiFone và Viettel.
3. **Tính tổng cước ước tính cho từng bên:**

```text
Đơn giá bình quân = (tỷ lệ khách cùng mạng × giá nội mạng)
                  + (tỷ lệ khách mạng khác × giá ngoại mạng)

Tổng mỗi tháng    = số phút × đơn giá bình quân + phí cố định
```

4. **Chọn bên có tổng thấp nhất.** **Không chọn theo giá nội mạng rẻ nhất** — nếu chỉ 20% khách ở mạng đó thì giá nội mạng rẻ gần như vô nghĩa.

### 7.5. Khi nào xem lại quyết định này

Chỉ xem lại khi có **đồng thời** cả ba: lượng đơn vượt 30.000/tháng và ổn định; đã đo được cơ cấu mạng thật của khách; và có báo giá cho thấy khoản tiết kiệm đủ bù chi phí duy trì, công kỹ thuật và phí tra cứu. Trước lúc đó, một nhà mạng là phương án đơn giản hơn, rẻ hơn và dễ vận hành hơn.

## 8. Thủ tục xin tên định danh — không mất một tháng

Theo Nghị định **91/2020/NĐ-CP**, tên định danh do **Cục An toàn thông tin** cấp, nộp hồ sơ trực tuyến tại [tendinhdanh.ais.gov.vn](https://tendinhdanh.ais.gov.vn):

| | |
| --- | --- |
| Thời hạn xử lý | **01 ngày làm việc** kể từ khi nhận đủ hồ sơ hợp lệ |
| Lệ phí cấp mới | **200.000đ** |
| Lệ phí sửa đổi, cấp lại | **100.000đ** |
| Quy cách tên | Tối đa 11 ký tự liền, chữ Latin và số |

Mốc 01 ngày **chỉ tính phần xét duyệt hồ sơ**. Chưa gồm thời gian chuẩn bị giấy tờ, và **thời gian nhà mạng mở dịch vụ sau khi đã có giấy** — đây thường mới là phần dài. Có giấy rồi vẫn phải đăng ký dịch vụ tại nhà mạng để kích hoạt.

**Không đợi có giấy mới bắt đầu làm phần mềm** — hai việc chạy song song được.

## 9. Việc cần làm ngay

| # | Việc | Ai làm |
| --- | --- | --- |
| 1 | **Đếm cơ cấu mạng của khách** — lấy 1.000–2.000 số điện thoại khách gần nhất, đếm theo đầu số | Bộ phận đơn hàng |
| 2 | Xin báo giá cùng một cấu hình từ VinaPhone, MobiFone, Viettel | Chủ dự án |
| 3 | **Tính tổng cước từng nhà mạng theo công thức §7.4, chọn bên thấp nhất và chốt luôn** | Chủ dự án |
| 4 | Chuẩn bị hồ sơ và nộp xin tên định danh | Chủ dự án |
| 5 | Lấy số đơn thực tế và phân bố theo giờ để chốt số kênh đăng ký | Chủ dự án + bộ phận đơn hàng |
| 6 | Bắt đầu hoàn thiện phần mềm — **không cần chờ giấy tờ** | Đội kỹ thuật |
| 7 | Kiểm tra máy chủ hiện có còn đủ chỗ chạy không; chuẩn bị mở đường cho âm thanh (địa chỉ Internet cố định đã có sẵn) | Bộ phận hạ tầng |
| 8 | Có đường thoại rồi thì chạy thử nhỏ, cập nhật lại thời gian và ngân sách bằng số đo thật | Đội kỹ thuật |

**Việc 1–2 làm trước, vì kết quả quyết định chọn nhà mạng nào. Việc 4, 6, 7 chạy song song được.**

### Câu hỏi gửi nhà mạng

Gửi cùng bộ câu hỏi này cho cả ba nhà mạng để so sánh trên cùng một mặt bằng.

Bối cảnh: *"Công ty có tổng đài riêng, chỉ gọi ra xác nhận đơn hàng, khách bấm 1 hoặc 0. Dùng một nhà mạng duy nhất, bắt đầu 8 kênh đồng thời và hiển thị tên công ty."*

1. **Cước ngoại mạng là bao nhiêu?** Đây là khoản lớn nhất, vì phần lớn khách sẽ ở mạng khác. Xin giá riêng cho từng hướng đích.
2. **Giá của từng mức kênh 4 / 8 / 16 / 32? Nâng kênh mất mấy ngày, cần ký lại hợp đồng hay chỉ phụ lục? Giảm kênh có bị phạt không?**
3. **Có giảm giá theo sản lượng không?** Công ty dồn toàn bộ lưu lượng vào một hợp đồng — xin bậc giá theo số phút cam kết mỗi tháng.
4. Tên công ty có hiện với khách VinaPhone, MobiFone, Viettel và các mạng khác không? Phí từng hướng? Trường hợp nào không hiện?
5. Phí hiện tên nằm trong cước phút hay tính riêng theo từng cuộc?
6. **Công ty đã có một địa chỉ Internet cố định (VNPT Fiber Xtra2). Nhà mạng xác thực đường thoại bằng địa chỉ này được không, hay bắt buộc dùng cách khác?** Nếu đăng ký bằng địa chỉ thì khai báo mất bao lâu, sau này đổi địa chỉ thì xử lý thế nào?
7. Báo giá trọn gói: cấp số, đường truyền theo mức kênh, tên định danh, cước nội mạng và ngoại mạng, cách làm tròn cước, cam kết tối thiểu, thuế, thời hạn báo giá.
8. Cho xin mẫu bảng kê cước và lịch gọi thử nội bộ. Có giới hạn riêng nào cho gọi xác nhận đơn tự động không?

## 10. Giới hạn của báo cáo

- **Chưa liên hệ nhà mạng, chưa có báo giá chính thức, chưa gọi thử qua mạng viễn thông thật.** Mọi con số ở §4 phải cập nhật khi có báo giá.
- Bảng số kênh ở §3 dựa trên giả định, **chưa có số đo thật** vì website và livestream chưa vận hành. Phải đo lại sau vài phiên bán hàng.
- Phương án B **chưa có cách hiện tên công ty**, nên chi phí của nó chưa gồm khoản đáp ứng yêu cầu này — hai cột chi phí không phải hai giải pháp tương đương hoàn toàn.
- Báo cáo chỉ đề xuất; chưa thay đổi hệ thống, chưa bật gọi khách thật, chưa đăng ký dịch vụ nào.

---

<details>
<summary><b>Phụ lục kỹ thuật — dành cho đội triển khai</b></summary>

Đối chiếu mã nguồn tại `main@2667f424`. Các khoảng trống phải đóng, **chung cho cả hai phương án**:

| Khoảng trống | Chi tiết |
| --- | --- |
| Đích gọi ghim vào lab | Adapter chỉ nhận alias `LAB-A`, caller ID `IVR-LAB`; đích khác bị từ chối |
| Quyền gọi khách thật đang tắt | Ngoài chế độ lab, hệ thống chọn `UnavailableSchedulerDispatchGateway`. Bật biến môi trường không tạo ra adapter production |
| Chưa có nơi giải mã số thật | Token hiện đổi thành alias lab; cần hoàn thiện resolver tại ranh giới telephony đã thống nhất với M3/Security |
| 32 dòng kênh trong DB ≠ 32 cuộc đồng thời | `SchedulerRuntime` nhận một lease rồi `await DispatchAsync`; `PollingJobHost` chờ hết lượt. Cần điều phối đồng thời có giới hạn, sở hữu sự kiện ARI, xử lý restart và fencing |
| Provisioner lab chỉ tạo một kênh | `SIM-ASTERISK-001` không phải inventory 32 cổng hay hợp đồng 32 phiên |
| Chưa có môi trường đích | Image đã build/publish (W-0292) nhưng deploy dev còn chặn vì thiếu kết nối cluster; staging chưa chạy |

**Điều kiện nghiệm thu N kênh:** chạy tăng dần `1 → 4 → 8 → 16 → 32`; giữ N cuộc đã kết nối chồng lấn ≥30 giây; nhận DTMF riêng từng cuộc; thử bận, không nghe, từ chối, bấm sớm, bấm sai, cúp giữa chừng; thử mất kết nối và restart giữa cuộc gọi — không tự gọi lại khi chưa rõ cuộc cũ đã kết thúc; đo **CPS** (số cuộc tạo mới mỗi giây) riêng với số cuộc đồng thời.

**Mạng và địa chỉ:** IVR deploy trên `192.168.1.61` (máy thứ hai `.60`), sau NAT của router công ty. Địa chỉ Internet công cộng đo được ngày 15/09/2026 là `14.224.139.101`, reverse DNS trả về `static.vnpt.vn` — khớp với gói VNPT Fiber Xtra2 có kèm IP tĩnh. Khi cấu hình Asterisk phải khai `external_media_address` và `external_signaling_address` bằng địa chỉ công cộng này, đồng thời mở port SIP (5060/5061) và dải RTP về `.61`; nếu bỏ qua thì cuộc gọi kết nối được nhưng không có tiếng.

**Lưu ý tên gọi:** các tên `SIM`, `eSIM`, bảng `ivr_sim_channels` trong mã nguồn và tài liệu cũ **không** chứng minh dự án đã chốt kiến trúc vật lý hay đã mua SIM.

*Nguồn code: [tiếp nhận task](../../src/Ivr.Infrastructure/Intake/TaskIntakeService.cs), [scheduler](../../src/Ivr.Infrastructure/Scheduling/SchedulerRuntime.cs), [quay số](../../src/Ivr.Infrastructure/Telephony/AsteriskAriSimGateway.cs), [đăng ký adapter](../../src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs), [chuẩn hóa 0/1](../../src/Ivr.Domain/Confirmation/DispositionMapper.cs). Kế hoạch triển khai chi tiết: [mobile-sip-trunk-production-32-channels-plan](../../plan/ivr-orther/mobile-sip-trunk-production-32-channels-plan-2026-09-15.md). Trạng thái báo giá trong repo vẫn là `NO_QUOTE` — xem [cost-model](../cost-model.md).*

</details>
