**PHỤ LỤC**

**HỆ THỐNG TIN NHẮN & GIAO TIẾP NGỮ CẢNH**

**(Bản khóa - Thuần Việt - Không tự diễn giải - Không tạo thêm Trigger ngoài danh sách)**

Nội dung được chuẩn hóa từ tài liệu bạn cung cấp

TIN NHẮN

và giữ nguyên câu chữ, chỉ sắp xếp lại theo cấu trúc hệ thống.

**I. NGUYÊN TẮC CHUNG**

- Tin nhắn là lớp giao tiếp theo hành vi người dùng.
- Mỗi tin nhắn phải gắn với một sự kiện kích hoạt cụ thể.
- Không được tự tạo thêm sự kiện ngoài danh sách quy định.
- Không gửi sai hạng thành viên.
- Không gửi trùng nội dung trong cùng tháng dương lịch.
- Nội dung tin nhắn không được thay đổi câu chữ khi triển khai kỹ thuật.

**II. PHÂN LOẠI TIN NHẮN**

Hệ thống định nghĩa các loại tin nhắn:

- Tin nhắn sinh nhật
- Tin nhắn chiến dịch Tết
- Nhắc Giờ Vàng
- Nhắc nâng hạng
- Chúc mừng nâng hạng
- Hết hàng trong phiên
- Mua thành công
- Mời khởi nghiệp

**III. SỰ KIỆN KÍCH HOẠT (BẮT BUỘC)**

Mỗi loại tin nhắn phải gắn với đúng sự kiện sau:

| **Loại tin nhắn**   | **Sự kiện kích hoạt**            |
| ------------------- | -------------------------------- |
|                     |                                  |
| Tết                 | campaign_scheduler_event         |
| NEW-01              | user_visit_without_register      |
| Nhắc T-10           | golden_hour_start_time - 10 phút |
| Nhắc T-5            | golden_hour_start_time - 5 phút  |
| NEW-03              | spent_total_update_event         |
| Chúc mừng nâng hạng | tier_change_event                |
| Hết hàng phiên      | sku_remaining_in_session = 0     |
| Mua thành công      | order_status = delivered         |

Không cho phép tạo thêm trigger phụ.

**IV. NỘI DUNG TIN NHẮN**

# PHỤ LỤC HỆ THỐNG TIN NHẮN GIỜ VÀNG - CANONICAL APPENDIX

Áp dụng cho:

- Khách chưa đăng ký (Guest)
- Khách đã mua nhưng chưa đăng ký tài khoản
- Thành viên NEW
- Thành viên các hạng SILVER, GOLD, PLATINUM, DIAMOND

Hệ thống sẽ tự xác định ai có quyền mua sớm theo hạng thành viên.

# 1\. THÔNG BÁO TRƯỚC GIỜ VÀNG

## 1.1. Dành cho khách chưa đăng ký hoặc chỉ có số điện thoại

Em chào Mình ạ.

Phiên Giờ Vàng sẽ bắt đầu lúc {{golden_hour_start_time}}.

Mình có thể tham gia mua sản phẩm khi phiên mở chính thức.

Đăng ký tài khoản để kích hoạt hạng thành viên và hưởng thêm quyền lợi theo chính sách hệ thống.

Xem danh sách sản phẩm Giờ Vàng tại đây: {{golden_hour_landing_link}}.

Trân trọng ạ.

## 1.2. Dành cho thành viên đã đăng ký

Em chào Mình ạ.

Phiên Giờ Vàng sẽ bắt đầu lúc {{golden_hour_start_time}}.

Là thành viên {{current_tier_display_name}}, Mình có quyền mua sản phẩm theo chính sách hạng thành viên của mình.

Nếu hạng của bạn có quyền vào sớm, bạn được vào trước {{current_tier_early_access_minutes}} so với thời điểm mở phiên chính thức.

Hãy chuẩn bị sẵn sàng để tăng cơ hội mua thành công.

Xem ngay tại đây: {{golden_hour_landing_link}}.

Trân trọng ạ.

# 2\. THÔNG BÁO QUYỀN VÀO SỚM (CHỈ GỬI CHO NGƯỜI CÓ QUYỀN)

Em chào Mình ạ.

Phiên Giờ Vàng sẽ bắt đầu lúc {{golden_hour_start_time}}.

Theo hạng {{current_tier_display_name}}, Mình được quyền vào sớm {{current_tier_early_access_minutes}} trước khi phiên mở chính thức.

Hãy truy cập sớm để gia tăng cơ hội mua thành công.

Xem ngay tại đây: {{golden_hour_landing_link}}.

Trân trọng ạ.

# 3\. THÔNG BÁO BẮT ĐẦU GIỜ VÀNG

## 3.1. Dành cho khách chưa đăng ký

Em chào Mình ạ.

Phiên Giờ Vàng đã bắt đầu lúc {{golden_hour_start_time}}.

Mình có thể tham gia mua ngay khi phiên mở.

Đăng ký tài khoản để kích hoạt hạng thành viên và hưởng chính sách ưu đãi dài hạn.

Xem ngay tại đây: {{golden_hour_landing_link}}.

Trân trọng ạ.

## 3.2. Dành cho thành viên đã đăng ký

Em chào Mình ạ.

Phiên Giờ Vàng đã bắt đầu lúc {{golden_hour_start_time}}.

Là thành viên {{current_tier_display_name}}, Mình đang được hưởng quyền mua sớm …. phút.

Nhanh tay đặt mua trước khi hết số lượng.

Xem ngay tại đây: {{golden_hour_landing_link}}.

Trân trọng ạ.

# 4\. NHẮC KHI PHIÊN ĐANG DIỄN RA

## 4.1. Dành cho khách chưa đăng ký

Em chào Mình ạ.

Phiên Giờ Vàng đang diễn ra.

Nhanh tay đặt mua trước khi hết số lượng.

Xem ngay tại đây: {{golden_hour_landing_link}}.

Trân trọng ạ.

## 4.2. Dành cho thành viên đã đăng ký

Em chào Mình ạ

Phiên Giờ Vàng đang diễn ra.

Nhanh tay đặt mua trước khi hết số lượng.

Xem ngay tại đây: {{golden_hour_landing_link}}.

Trân trọng ạ.

# GHI CHÚ QUAN TRỌNG ĐỂ CHÈN VÀO TÀI LIỆU

- Gọi khác là mình AI phải sưng Em khi nói chuyện.
- Không được tự tính quyền mua sớm.
- Quyền vào sớm chỉ hiển thị nếu hệ thống xác nhận người đó có quyền.
- Không được thay bằng số điện thoại nếu không có họ tên.
- Không được tự suy đoán hạng thành viên.

# 6\. BIRTHDAY_REWARD - CHÚC MỪNG SINH NHẬT

### 1️. Bản triển khai

Ginsengfood kính chúc {{full_name}} một ngày sinh nhật an vui và nhiều năng lượng.

Nhân dịp sinh nhật của bạn, hệ thống gửi đến bạn ưu đãi theo hạng thành viên {{current_tier_display_name}}.

Là thành viên {{current_tier_display_name}}, bạn đang được hưởng mức giảm lên đến {{current_tier_discount_percent}} trong chương trình 24/7 và quyền mua sớm {{current_tier_early_access_minutes}} trong Giờ Vàng.

Hãy theo dõi các phiên Giờ Vàng để không bỏ lỡ ưu đãi lên đến {{max_discount_percent}}, mỗi SKU chỉ có {{quota_per_sku}} sản phẩm.

Xem chi tiết quyền lợi của bạn tại đây: {{policy_member_link}}.

Trân trọng.

### 2️. Biến sử dụng

{{full_name}}  
{{current_tier_display_name}}  
{{current_tier_discount_percent}}  
{{current_tier_early_access_minutes}}  
{{max_discount_percent}}  
{{quota_per_sku}}  
{{policy_member_link}}

### 3️. Snapshot bắt buộc

· tier_snapshot  
· golden_hour_policy_snapshot  
· policy_version_id  
· link_snapshot

### 4️. Điều kiện gửi

Trigger: BIRTHDAY_REWARD  
Chỉ gửi 1 lần trong năm.  
Email được phép gửi theo Multi-Channel Lock.

# C.7. DIAMOND_REFERRAL - GIỚI THIỆU THÀNH CÔNG

### 1️. Bản triển khai

Ginsengfood kính chúc mừng {{full_name}}.

Bạn vừa có khách hàng đăng ký hoặc mua hàng thành công thông qua đường link giới thiệu của bạn.

Là thành viên {{current_tier_display_name}}, bạn đang hưởng chính sách dành riêng cho hạng DIAMOND.  
Mức hoa hồng hiện tại của bạn là {{commission_percent}}.

Tiếp tục chia sẻ đường link của bạn để gia tăng giá trị tích lũy.  
Đường link giới thiệu của bạn: {{referral_link}}.

Xem chính sách DIAMOND tại đây: {{policy_diamond_link}}.

Trân trọng.

### 2️. Biến sử dụng

{{full_name}}  
{{current_tier_display_name}}  
{{commission_percent}}  
{{referral_link}}  
{{policy_diamond_link}}

### 3️. Snapshot bắt buộc

· diamond_snapshot  
· tier_snapshot  
· link_snapshot  
· policy_version_id

### 4️. Điều kiện gửi

Trigger: DIAMOND_REFERRAL  
Chỉ áp dụng cho member có tier_name = DIAMOND  
Không gửi cho hạng khác

# C.8. DIAMOND_COMMISSION - THÔNG BÁO HOA HỒNG

### 1️. Bản triển khai

Ginsengfood kính chào {{full_name}}.

Hệ thống ghi nhận hoa hồng mới phát sinh từ hoạt động giới thiệu của bạn.

Mức hoa hồng hiện tại theo chính sách là {{commission_percent}}.

Bạn có thể theo dõi chi tiết hoa hồng và chính sách DIAMOND tại đây: {{policy_diamond_link}}.

Tiếp tục phát triển hệ thống khách hàng của bạn để gia tăng giá trị tích lũy dài hạn.

Trân trọng.

### 2️. Biến sử dụng

{{full_name}}  
{{commission_percent}}  
{{policy_diamond_link}}

### 3️. Snapshot bắt buộc

· diamond_snapshot  
· link_snapshot  
· policy_version_id

### 4️. Điều kiện gửi

Trigger: DIAMOND_COMMISSION  
Chỉ gửi khi commission được ghi nhận thành công.

**1\. Tin nhắn Tết**

Kích hoạt: campaign_scheduler_event

Ginsengfood kính chúc họ và tên ……năm mới an khang thịnh vượng, thành công và hạnh phúc, nhân dịp xuân về Ginsengfood, Trân trọng cảm ơn Bạn đã đồng hành cùng ginsengfood trong suốt thời gian qua.

Trân trọng cảm ơn  
Tổng giám đốc Tập đoàn Ssavigroup  
TS. Phù Tường Nguyên Dũng

Gửi lúc 7h00 phút mùng 1 tết AL

- **Tin nhắn chúc mừng sinh nhật:**

"Ginsengfood trân trọng chúc mừng \[Họ và tên\] sinh nhật thật ấm áp, hạnh phúc bên gia đình và thành công hơn nữa trong cuộc sống

Trân trọng"

Gửi lúc 9h00 phút ngày sinh nhật

- **Guest:**

- Guest: đây là trường hợp người chưa từng biết đến sản phẩm Cháo sâm của Ginsengfood do vậy cần khai thác nguồn khách Guest từ các nguồn sau:  
   \- Qua quảng cáo tự vào  
   \- Tệp khách hàng trước đây, dữ liệu khách hàng được chia sẽ và thu thập được

**Tin nhắn cho Guest:**

- Tin nhắn 1: Em chào Mình ạ, Ginsengfood trân trọng giới thiệu 20 sản phẩm Cháo Sâm, đây là hệ sản phẩm khoa học vô cùng chất lượng, bổ dưỡng và tiện lợi. cùng với quyền lợi hấp dẫn khi đăng ký thành viên. Xem chính sách thành viên và mua bên dưới (Xem Ngay)
- \- Tin nhắn 2: Ginsengfood trân trọng giới thiệu đến Mình 3 nhóm Cháo Sâm gồm Cháo Sâm theo mùa, Cháo Sâm chức năng và Cháo Sâm bổ dưỡng với 20 loại bổ dưỡng Giữ trọn tinh hoa, Ngon như Mẹ nấu Xem chính sách thành viên và mua bên dưới (Xem Ngay)
- \- Tin nhắn 3: Cháo Sâm được nghiên cứu, ứng dụng theo nền tảng y thực đồng nguyên hiện đại kết hợp hài hòa cùng giá trị hơn 4.000 năm của nền y học phương đông và ứng dụng công nghệ hiện đại sấy thăng hoa mang đến cho Anh/Chị sản phẩm bổ dưỡng, độc đáo nhất trên thị trường hiện nay Xem chính sách thành viên và mua bên dưới (Xem Ngay)
- \- Tin nhắn 4: Dạ em chào ạ, Mình hãy đăng ký thành của Ginsengfoof hôm nay để hưởng nhiều chính sách ưu đãi giảm lên đến 20% cho các đơn hàng và quyền mua sớm trước giờ vàng tri ân đến 30 phút. Xem chính sách thành viên và mua bên dưới (Xem Ngay)
- \- Tin nhắn 5: Giờ Vàng sẽ bắt đầu sau ….. phút. Mình hãy đăng ký thành viên và tích lũy điểm để trở thành nhà đồng hành được giảm lên đến 20% tất cả các đơn hàng, được quyền mua sớm trước giờ vàng lên đến 30 phút và cơ hội kinh doanh cùng Ginsengfood. Xem chính sách thành viên và mua bên dưới (Xem Ngay)
- \- Tin nhắn 6: Giờ vàng Tri ân Số lượng có hạn, vui đừng bỏ lỡ". Hãy cùng Ginsengfood lan tỏa giá trị sản phẩm khoa học, đến với cộng đồng vì sức khỏe và tương lai tươi sáng cho dân tộc Việt Nam, vì hạnh phúc và cuộc sống tốt đẹp hơn cho người dân nghèo ven biển. Xem chính sách thành viên và mua bên dưới (Xem Ngay)
- NEW-01
- **Tin nhắn cho New:**

Dùng cho khách đã có lịch sử mua hàng

**Tin nhắn trước giờ vàng chính thức:**

**Nội dung gửi:**

- Giờ Vàng sẽ bắt đầu sau ….. phút. Số lượng có hạn. Hãy chuẩn bị sẵn sàng nhé ạ."
- Em chào mình ạ, Giờ vàng tri ân của ginsengfoof còn ….. phút nữa sẽ diễn ra Số lượng có hạn, vui đừng bỏ lỡ nhé"

**Thời gian gửi phiên tối (phiên 2):**

- Gửi trước giờ vàng 15 phút và 3 phút trước khi giờ vàng diễn ra

**Tin nhắn khởi nghiệp**

**NEW-02**

- Hãy đăng ký thành viên và tích lũy điểm để trở Nhà đồng hành khởi nghiệp Ginsengfood. Chúng ta sẽ cùng lan tỏa giá trị từ Sâm quý trồng trên cát biển và vì cuộc sống tốt đẹp hơn cho người dân nghề trên vùng cát biển

Đặc quyền giảm đến 20% khi mua san phẩm và cơ hội kinh doanh cùng ginsengfood ." Xem chính sách khởi nghiệp (xem ngay)

Trình tự gửi tin nhắn khởi nghiệp:

Chỉ gửi ngay sau khi mua hàng,

Tháng đầu: Gửi 1 tin vào các ngày chủ nhật lúc 8h sáng. Tháng thứ 2 và các tháng tiếp theo 2 tin/ tháng vào 8h sáng chủ nhật cho đến khi New đăng ký thành viên. Không gửi nữa, lúc đó sẽ chuyển sang chế độ chăm sóc **_SILVER_**

**NEW-03**

**Trường hợp 1: Nếu khách New đã có doanh số nhưng chưa đạt tiêu chuẩn thành viên**

**Gửi tin nhắn:**

- Hãy vui lòng đăng ký thành viên Ginsengfood và trích lũy để thăng hạng, nhận đặc quyền giảm lên đến 20% "theo chính sách thành viên" + quyền mua sớm từ 5 phút đến 30 phút trước Giờ Vàng tri ân mỗi ngày."

**\- Gửi khi New vừa rời khỏi ginsengfood**

**Trường Hợp 1: khách New có doanh số nhưng chưa đạt tiêu chuẩn thành viên**

**Gửi tin nhắn:**

- Bạn đã đồng hành cùng Ginsengfood với tổng giá trị {{spent_total}} Chỉ còn …….. VNĐ bạn sẽ đạt hạng thành viên ……. Hãy kích hoạt hạng thành viên để nhận đặc quyền giảm lên đến ….% "theo chính sách thành viên"khi mua sản phẩm tại Ginsengfood

**Trường Hợp 2: khách New có doanh số đạt thứ hạng thành viên nhưng chưa đăng ký, chưa kích hoạt thành viên**

**Gửi tin nhắn chăm sóc khách khàng:**

- Bạn đã đồng hành cùng Ginsengfood với tổng giá trị {{spent_total}} tương ứng với hạng thành viên ………..  
   Hãy kích hoạt hạng thành viên để nhận đặc quyền giảm lên đến ………% và quyền mua sớm trong Giờ Vàng tri ân mỗi ngày nhé

**Tin nhắn khởi nghiệp**

- _Khởi nghiệp cùng Ginsengfood hôm nay từ hào cho ngày mai_

_Cùng lan tỏa giá trị những sản phẩm khoa học từ sâm trồng trên cát biển quý giá chăm sóc sức khỏe cộng đồng, cùng Ginsengfood thực hiện khát vọng vì cuộc sống tốt đẹp hơn cho người dân vùng cát biển. (Xem chính sách khởi nghiêp bên dưới)_

**_Chú ý:_**

- _Hệ thống phải bắt được thông tin New (đã có lịch sử mua hàng, mà chưa đăng ký, bắt được trường doanh số bao nhiêu …..VNĐ và còn bao nhiêu ….. VNĐ để đạt hạng thành viên gì ………_
- _Hệ thống phải bắt được doanh số thành viên đã mua tương ứng với hạnh thành viên nào để yêu cầu đăng ký nhằm để New được hưởng đặc quyền theo hạng thành viên từ đó biến New trở thành thành viên trung thành và cơ hội đồng hành khởi nghiệpcùng Ginsengfood_
- (xem ngay) phải gắn link để New đọc được chính sách thành viên và chính sách khởi nghiệp

**_SILVER_**

**Thời gian gửi phiên tối (phiên 2):**

**Nội dung gửi:**

- Giờ Vàng sẽ bắt đầu sau ….. phút nữa.  
   Ưu tiên thời điểm chốt đơn hợp lệ.  
   **_Mình là SILVER_** được quyền mua trước sau ….. phút nữa

Hãy chuẩn bị sẵn sàng nhé."

- Giờ vàng chỉ còn ….. phút nữa số lượng có hạn, mình vui đừng bỏ lỡ nhé"

**Thời gian gửi:**

- **Gửi trước giờ vàng 20 phút và trước 4 Phút khi giờ vàng diễn ra**

**Tin nhắn chăm sóc thành viên:**

- _Chúc mừng họ và tên…. Bạn đang là thành viên SILVER (Ngay từ bây giờ tất cả đơn hàng Bạn được_ giảm _lên đến ….% khi mua sản phẩm tại Ginsengfood chương trình 24/7 và quyền mua sớm ….. phút trước chương trình giờ vàng mỗi ngày._
- _Để trở thành GOLD, tăng đặc quyền nhận_ quyền giảm _lên đến ….% và đặc quyền mua sớm …… phút trước giờ vàng, bạn chỉ cần mua thêm ……. VND_
- _Để trở thành PLATINUM, tăng_ quyền lợi giảm _lên đến ….% chương trình 24/7 và đặc quyền mua sớm …. phút trước giờ vàng Mình chỉ cần mua thêm ……. VND_
- _Để trở thành DIAMOND tăng_ quyền lợi giảm _lên đến …..% chương trình 24/7 và đặc quyền mua sớm …. phút trước giờ vàng Mình chỉ cần mua thêm ……. VND_
- _Khởi nghiệp cùng Ginsengfood hôm nay tự hào cho tương lai._

_Cùng lan tỏa giá trị những sản phẩm khoa học từ sâm trồng trên cát biển quý giá, chăm sóc sức khỏe cộng đồng, cùng Ginsengfood thực hiện khát vọng vì cuộc sống tốt đẹp hơn cho người dân vùng cát biển. (Xem chính sách khởi nghiêp bên dưới)_

**_GOLD_:**

**Thời gian gửi phiên tối (phiên 2):**

- "Giờ Vàng sẽ bắt đầu sau …. phút.  
   Ưu tiên thời điểm chốt đơn hợp lệ.  
   Mình là Gold được quyền mua trước sau …..phút nữa, Số lượng có hạn hãy chuẩn bị sẵn sàng nhé."

**Tin nhắn ngay giờ vàng:**

2\. "Giờ Vàng sẽ bắt đầu sau …. phút.  
phút hãy tận dụng đặc quyền này nhé." Số lượng có hạn, vui đừng bỏ lỡ"

- **Gửi trước giờ vàng 15 phút và trước 6 phút khi giờ vàng diễn ra**

**Tin nhắn chăm sóc thành viên:**

- _Chúc mừng họ và tên…. Bạn đang là thành viên GOLD (Từ bây giờ, tất cả đơn hàng Mình được giảm lên đến ……% khi mua sản phẩm tại Ginsengfood chương trình 24/7 và quyền mua sớm ….. phút trước chương trình giờ vàng mỗi ngày._
- _Để trở thành PLATINUM, nhận quyền lợi giảm lên đến …..% khi mua sản phẩm chương trình 24/7 và đặc quyền mua sớm …. phút trước giờ vàng Mình chỉ cần mua thêm ……. VND_
- _Để trở thành DIAMOND, nhận quyền lợi giảm lên đến …..% khi mua sản phẩm chương trình 24/7 và đặc quyền mua sớm …..phút trước giờ vàng Mình chỉ cần mua thêm ……. VND_
- _Khởi nghiệp cùng Ginsengfood hôm nay tự hào cho tương lai._

_Cùng lan tỏa giá trị những sản phẩm khoa học từ sâm trồng trên cát biển quý giá, chăm sóc sức khỏe cộng đồng, cùng Ginsengfood thực hiện khát vọng vì cuộc sống tốt đẹp hơn cho người dân vùng cát biển. (Xem chính sách khởi nghiêp bên dưới)_

**_PLATINUM_**

**Thời gian gửi phiên tối (phiên 2):**

- "Giờ Vàng sẽ bắt đầu sau …. phút.  
   Ưu tiên thời điểm chốt đơn hợp lệ. Mình là Platinum được quyền mua trước sau …. phút nữa

Số lượng có hạn hãy chuẩn bị sẵn sàng nhé."

**Tin nhắn trước giờ vàng 8 phút:**

2\. Em chào ạ, Giờ Vàng sẽ bắt đầu sau ….. phút nữa, Mình hãy tận dụng đặc quyền này nhé." Số lượng có hạn, vui đừng bỏ lỡ"

- **Gửi trước giờ vàng 25 phút và trước 8 phút khi giờ vàng diễn ra**

**Tin nhắn:**

- _Chúc mừng họ và tên…. Bạn đang là PLATINUM, nhận_ quyền lợi theo hạng thành viên giảm _lên đến …..% chương trình 24/7 và đặc quyền mua sớm ….. phút trước giờ vàng bạn chỉ cần mua thêm ……. VND_
- _Để trở thành DIAMOND, nhận_ quyền lợi giảm _lên đến ….% và đặc quyền mua sớm … phút trước giờ vàng bạn chỉ cần mua thêm ……. VND_
- _Khởi nghiệp cùng Ginsengfood hôm nay tự hào cho tương lai._

_Cùng lan tỏa giá trị những sản phẩm khoa học từ sâm trồng trên cát biển quý giá, chăm sóc sức khỏe cộng đồng, cùng Ginsengfood thực hiện khát vọng vì cuộc sống tốt đẹp hơn cho người dân vùng cát biển. (Xem chính sách khởi nghiêp bên dưới)_

**DIAMOND nhà đồng hành khởi nghiệp**

**Thời gian gửi phiên tối (phiên 2):**

- Dạ giờ Vàng tri ân của ginsengoof sẽ bắt đầu sau …. phút nữa.

Mình hiện là Diamond được quyền mua sớm …. phút, hãy chuẩn bị sẵn sàng nhé. Trân trọng

**Tin nhắn trước giờ vàng 10 phút:**

2\. " dạ Giờ Vàng sẽ bắt đầu sau …. phút nữa, mình hãy tận dụng chương trình giờ vàng tri ân này nhé." Số lượng có hạn, vui đừng bỏ lỡ"

- **Gửi trước giờ vàng 35 phút và trước 10 phút khi giờ vàng diễn ra**

**Tin nhắn:**

- _Chúc mừng họ và tên…. Mình đang là DIAMOND, nhận_ quyền lợi giảm _lên đến ….% khi mua sản phẩm chương trình 24/7 và đặc quyền mua sớm …. phút trước giờ vàng ạ_
- _Bây giờ Mình đã đạt hạng thành viên DIAMOND, mình có thể trở thành nhà đồng hành Khởi nghiệp cùng Ginsengfood ngay lúc này ạ.  
   Mình được hưởng nhiều đặc quyền và cơ hội kinh doanh mang đến thu nhập cao nhưng giá trị lớn hơn chính là Chúng ta đang cùng nhau vì sức khỏe cộng đồng, vì một tương lai tươi sáng hơn cho người dân vùng cát biển và khát vọng quốc gia dẫn đầu bằng các sản phẩm từ sâm (link chính sách thành viên)_

**Chu kỳ gửi:**

- Gửi lần lượt từ thứ 2 - CN hàng tuần
- Mỗi tuần chỉ gửi 1 lần vào 1 ngày theo thứ tự thứ 2 - CN".

**Cách gửi tin nhắn:**

Gửi lần lượt từ tin trước và ngay khi diễn ra giờ vàng

Chú ý: Gửi tin nhắn cho người dùng vào trước chương trình tối phiên 2

Chỉ gửi: vào nền tảng đã có lịch sử nhắn tin, trao đổi trước đó

Hạng thành viên nào gửi nội dung của hạng thành viên đó và thời gin gửi được mô tả ở trên

**GHI CHÚ PHẦN TIN NHẮN:**

Tất cả các nội dung tin nhắn phải căn cứ vào các hạng thành viên để gửi tin nhắn.

Hệ thống phải biết được đang nói chuyện với ai, họ là khách mới hay hạng thành viên gì, doanh số tích lũy bao nhiêu, quyền lợi thế nào và phải bắt được các biến của lõi ginsengfoof để trả lời thật chính xác, tránh nói sai, bịa, trùng tin nhiều lần

Công cụ gửi tin nhắn gồm các nền tảng theo thứ tự ưu tiên như sau:

- - - Zalo
      - Messenger
      - SMS
      - Instagram
      - Email

Email là kênh bắt buộc cho event ORDER_SUCCESS nếu user có đăng ký email hợp lệ.  
Các nội dung khác không bắt buộc gửi email.

**Chú ý:**

- - - - Không gửi hết toàn bộ nền tảng gửi tin, chỉ gủi cho các nền tảng có lịch sử đã ghi nhận trao tổi trước đây và chỉ gửi 1 lần/tháng cho 1 nội dung tin nhắn - Email chỉ gửi nội dung thành viên không gửi tin nhắn hết hết hàng, chỉ gửi tin nhắn chúc mừng mua hàng thành công - Trường hợp New chỉ gửi cho New đã có lịch sử mua hàng thành công

Thông tin khai báo

- Thành viên từ **_SILVER_ - _PLATINUM_**

- Họ và tên "bắt buột"
- Số điện thoại "bắt buột"
- Địa chỉ: "bắt buột"
- Ngày tháng năm sinh: " bắt buột"
- Email: "không bắt buột"

- **_Thành viên DIAMOND nhà đồng hành khởi nghiệp_**
- Họ và tên "bắt buột"
- Số điện thoại "bắt buột"
- Địa chỉ: "bắt buột"
- Số tài khoản …… ngân hàng ……………"bắt buột"
- Email: "không bắt buột"
- Ngày tháng năm sinh "bắt buột"

# Chương V - Tin Nhắn

## 1\. Quy tắc Email (thay toàn bộ đoạn cũ liên quan Email)

Email là kênh bắt buộc cho event ORDER_SUCCESS nếu user có đăng ký email hợp lệ.  
Các nội dung khác không bắt buộc gửi email.  
Nếu user không đăng ký email thì không áp dụng kênh email.

Không tồn tại rule thứ hai song song.

**2\. BIẾN ĐỘNG TRONG NỘI DUNG**

Hệ thống phải hỗ trợ biến:

- {{full_name}}
- {{spent_total}}
- {{tier_name}}
- {{amount_needed}}
- {{discount_percent}}
- {{early_minutes}}
- {{voucher_amount}}
- {{cash_amount}}

Biến phải được xử lý trước khi gửi.

**3\. QUY TẮC GỬI ĐA KÊNH**

**3.1. Thứ tự ưu tiên**

- Zalo
- Messenger
- SMS
- Instagram
- Email
- Email

**3.2. Điều kiện gửi**

Chỉ gửi trên nền tảng người dùng đã từng tương tác hoặc xác thực.

Không gửi nền tảng chưa có lịch sử.

**3.3. Không gửi trùng trong cùng tháng dương lịch**

- Tính theo múi giờ Việt Nam
- Từ 00:00 ngày 01 đến 23:59 ngày cuối tháng
- Không tính rolling 30 ngày

**3.4. Quy định riêng Email**

Email chỉ gửi với:

- ORDER_SUCCESS
- BIRTHDAY_REWARD
- TIER_CONGRATS

Không gửi email cho:

- SKU_SOLD_OUT
- Nội dung quảng bá khác

**VI. GIỚI HẠN GỬI**

**Chu kỳ gửi:**

- Gửi lần lượt từ thứ 2 - CN hàng tuần
- Mỗi tuần chỉ gửi 1 lần vào 1 ngày theo thứ tự thứ 2 - CN".

**Cách gửi tin nhắn:**

Gửi lần lượt từ tin trước và ngay khi diễn ra giờ vàng

Chú ý: Gửi tin nhắn cho người dùng vào trước chương trình tối phiên 2

Hạng thành viên nào gửi nội dung của hạng thành viên đó và thời gin gửi được mô tả ở trên

**VII. NHẬT KÝ BẮT BUỘC**

Hệ thống phải lưu:

- user_id
- loại tin nhắn
- kênh gửi
- thời gian gửi
- trạng thái
- lỗi nếu có

**VIII. ĐIỀU KIỆN DỮ LIỆU**

Chỉ gửi khi có ít nhất một trong các thông tin:

- Số điện thoại
- Zalo
- Messenger
- Email

Nếu NEW chưa có thông tin → không gửi.

Hệ thống phải có biểu mẫu thu thập:

- Họ tên
- Số điện thoại
- Email
- Ngày sinh
- Địa chỉ
- Thông tin tài khoản ngân hàng

Dữ liệu này phục vụ:

- Giao hàng
- Chăm sóc khách hàng
- Tri ân sinh nhật và lễ Tết

**IX. CẤM**

- Không tạo thêm trigger ngoài danh sách
- Không gửi sai hạng
- Không gửi trùng tháng
- Không gửi email tràn lan
- Không sửa nội dung câu chữ

**Chương XI:**

- Trigger
- Rate limit
- Biến động
- Điều kiện gửi
- Nhật ký
- Quy định email
