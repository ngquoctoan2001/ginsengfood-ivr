**HỆ THỐNG TIN NHẮN & GIAO TIẾP NGỮ CẢNH PHẦN BỔ SUNG**

Giữ nguyên câu chữ, chỉ sắp xếp lại theo cấu trúc hệ thống.

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

# HỆ THỐNG TIN NHẮN GIỜ VÀNG - SOURCE BASELINE

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

Là thành viên {{current_tier_display_name}}, mình được quyền mua sản phẩm trước {{current_tier_early_access_minutes}} so với thời điểm mở phiên chính thức.

Hãy chuẩn bị sẵn sàng để tăng cơ hội mua thành công.

Xem ngay tại đây: {{golden_hour_landing_link}}.

Trân trọng ạ.

# 2\. THÔNG BÁO QUYỀN VÀO SỚM (CHỈ GỬI CHO NGƯỜI CÓ QUYỀN)

Em chào Mình ạ.

Phiên Giờ Vàng sẽ bắt đầu lúc {{golden_hour_start_time}}.

Theo hạng {{current_tier_display_name}},Mình được quyền vào sớm {{current_tier_early_access_minutes}} trước khi phiên mở chính thức.

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

Là thành viên {{current_tier_display_name}}, Mình đang được quyền mua sơm ….. phút.

Nhanh tay đặt mua trước khi hết số lượng.

Xem ngay tại đây: {{golden_hour_landing_link}}.

Trân trọng ạ.

# GHI CHÚ QUAN TRỌNG ĐỂ CHÈN VÀO TÀI LIỆU

- AI xung với khách hàng là Em, gọi khách hàng là Mình.
- Không được tự tính quyền mua sớm.
- Quyền vào sớm chỉ hiển thị nếu hệ thống xác nhận người đó có quyền.
- Không được thay bằng số điện thoại nếu không có họ tên.
- Không được tự suy đoán hạng thành viên.

# 7\. DIAMOND_REFERRAL - GIỚI THIỆU THÀNH CÔNG

### 7.1️. Bản triển khai

Ginsengfood kính chúc mừng Mình ạ.

Mình vừa có khách hàng mua hàng thành công thông qua đường link giới thiệu của mình.

Là thành viên {{current_tier_display_name}}, Mình đang hưởng chính sách dành riêng cho hạng DIAMOND.  
Mức hoa hồng hiện tại của Mình là {{commission_percent}}.

Tiếp tục chia sẻ đường link của Mình để gia tăng giá trị tích lũy.  
Đường link giới thiệu của Mình: {{referral_link}}.

Xem chính sách DIAMOND tại đây: {{policy_diamond_link}}.

Trân trọng ạ.

### 7.2️. Biến sử dụng

{{full_name}}  
{{current_tier_display_name}}  
{{commission_percent}}  
{{referral_link}}  
{{policy_diamond_link}}

### 7.3️. Snapshot bắt buộc

· diamond_snapshot  
· tier_snapshot  
· link_snapshot  
· policy_version_id

### 7.4️. Điều kiện gửi

Trigger: DIAMOND_REFERRAL  
Chỉ áp dụng cho member có tier_name = DIAMOND  
Không gửi cho hạng khác

# 8\. DIAMOND_COMMISSION - THÔNG BÁO HOA HỒNG

### 8.1️. Bản triển khai

Ginsengfood kính chào Mình ạ

Hoa hồng mới phát sinh từ hoạt động giới thiệu của Mình.

Mức hoa hồng hiện tại theo chính sách là {{commission_percent}}.

Mình có thể theo dõi chi tiết hoa hồng và chính sách DIAMOND tại đây: {{policy_diamond_link}}.

Tiếp tục phát triển hệ thống khách hàng của Mình để gia tăng giá trị tích lũy dài hạn và nhận thu nhập hàng tháng.

Trân trọng ạ.

### 8.2️. Biến sử dụng

{{full_name}}  
{{commission_percent}}  
{{policy_diamond_link}}

### 8.3️. Snapshot bắt buộc

· diamond_snapshot  
· link_snapshot  
· policy_version_id

### 8.4️. Điều kiện gửi

Trigger: DIAMOND_COMMISSION  
Chỉ gửi khi commission được ghi nhận thành công.

**9\. Tin nhắn Tết**

Kích hoạt: campaign_scheduler_event

Ginsengfood kính chúc {{full_name}} năm mới an khang thịnh vượng, thành công và hạnh phúc. Trân trọng cảm ơn Mình đã đồng hành cùng Ginsengfood trong suốt thời gian qua.

Tổng giám đốc Tập đoàn Ssavigroup  
TS. Phù Tường Nguyên Dũng

Gửi lúc 7h00 phút mùng 1 tết AL

**10\. Tin nhắn chúc mừng sinh nhật:**

Ginsengfood trân trọng chúc mừng {{full_name}} sinh nhật thật ấm áp, hạnh phúc bên gia đình và thành công hơn nữa trong cuộc sống

Trân trọng

Gửi lúc 9h00 phút ngày sinh nhật

**11\. TIN NHẮN TẠO CẢM XÚC**

**11.1. Guest:**

- Guest: đây là trường hợp người chưa từng biết đến sản phẩm Cháo sâm của Ginsengfood do vậy cần khai thác nguồn khách Guest từ các nguồn sau:  
   \- Qua quảng cáo mà có  
   \- Tệp khách hàng trước đây
- Dữ liệu khách hàng được chia sẽ và thu thập được

**Tin nhắn tạo cảm xúc cho Guest:**

- Tin nhắn:
- Em chào Mình ạ, Ginsengfood trân trọng giới thiệu đến Mình 20 sản phẩm Cháo Sâm, đây là hệ sản phẩm khoa học chất lượng, bổ dưỡng và tiện lợi. cùng với quyền lợi hấp dẫn khi đăng ký thành viên. Xem chính sách thành viên và mua bên dưới (Xem Ngay)
- Tin nhắn:

Ginsengfood trân trọng giới thiệu đến Mình 3 nhóm Cháo Sâm gồm Cháo Sâm theo mùa, Cháo Sâm chức năng và Cháo Sâm bổ dưỡng với 20 loại "Giữ trọn tinh hoa, Ngon như Mẹ nấu" Xem chính sách thành viên và mua bên dưới (Xem Ngay)

- Tin nhắn:

Cháo Sâm được nghiên cứu, ứng dụng theo nền tảng y thực đồng nguyên hiện đại kết hợp hài kinh nghiệm hơn 4.000 năm của nền y học phương đông, ứng dụng công nghệ sấy thăng hoa mang đến cho Mình sản phẩm giá trị bổ dưỡng cao, độc đáo trên thị trường hiện nay. Xem chính sách thành viên và mua bên dưới (Xem Ngay)

- Tin nhắn:

Mình hãy đăng ký thành viên của Ginsengfoof hôm nay và mua sản phẩm để kích hoạt đặc quyền ưu đãi giảm lên đến 20% cho các đơn hàng và quyền mua sớm trước giờ vàng tri ân đến 30 phút. Xem chính sách thành viên và mua bên dưới ạ(Xem Ngay)

**11.2. NEW:**

**Tin nhắn cảm xúc cho** New

**\* New có doanh số nhưng chưa đăng ký**

- Tin nhắn:

Chúc mừng {{full_name}} Hiện doanh số tích lũy của Mình là ….. VNĐ. Chỉ với đơn hàng …… VNĐ, đồng thời đăng ký thành viên để kích hoạt quyền lợi {{current_tier_display_name}} tất cả các đơn hàng cùa Mình được giảm lên đến {{max_discount_percent}} cho chương trình 24/7 và quyền mua sớm {{current_tier_early_access_minutes}} trước chương trình giờ vàng mỗi ngày. Trân trọng ạ (Mua ngay)

- Tin nhắn:

Chúc mừng {{full_name}} Hiện doanh số tích lũy của Mình là ….. VNĐ. Tương đương với {{current_tier_display_name}} Hãy đăng ký để kích hoạt quyền lợi {{current_tier_display_name}} được giảm lên đến {{max_discount_percent}} cho tất cả đơn hàng khi mua sản phẩm tại Ginsengfood chương trình 24/7 và quyền mua sớm {{current_tier_early_access_minutes}} trước chương trình giờ vàng mỗi ngày. Trân trọng ạ

**11.2.2. Tin nhắn cảm xúc cho New**

**\* New đã đăng ký nhưng chưa đạt Sliver chưa được hưởng quyền lợi**

- Tin nhắn:

Chúc mừng {{full_name}} hãy mua đơn hàng với số tiền…… VNĐ để kích hoạt quyền lợi {{current_tier_display_name}}, đặc quyền giảm lên đến {{max_discount_percent}} khi mua sản phẩm tại Ginsengfood chương trình 24/7 và quyền mua sớm {{current_tier_early_access_minutes}} trước chương trình giờ vàng mỗi ngày. Trân trọng ạ

- Tin nhắn:

Để trở thành {{current_tier_display_name}}, tăng đặc quyền nhận quyền giảm lên đến {{max_discount_percent}} và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng, Mình chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Để trở thành {{current_tier_display_name}}, tăng quyền lợi giảm lên đến {{max_discount_percent}} chương trình 24/7 và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng Mình chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Để trở thành {{current_tier_display_name}} tăng quyền lợi giảm lên đến {{max_discount_percent}} chương trình 24/7 và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng Mình chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Em giới thiệu Mình Cháo Sâm Ginsengfood là sản phẩm dược nghiên cứu đựa trên nền tảng y thực đồng nguyên hiện đại kết hợp hài hòa cùng giá trị hơn 4.000 năm y học phương đông và ứng dụng công nghệ hiện đại sấy thăng hoa mang đến cho bạn sản phẩm Cháo Sâm bổ dưỡng, Giữ trọn tinh hoa, Ngon như Mẹ nấu (Mua ngay)

- Tin nhắn:

Em chào mình ạ. Hãy chọn cho Mình và gia đình những sản phẩm tuyệt vời của Ginsengfood đồng thời giúp Ssavigroup lan tỏa giá trị sản phẩm khoa học, chất lượng bổ dưỡng, tinh túy đến với cộng đồng đó là Mình đang cùng Ssavigroup vì tương lai tươi sáng cho cho người dân nghèo ven biển (Mua ngay)

- Tin nhắn:

Ginsengfood trân trọng cảm ơn Mình đã đồng hành cùng Ginsengfood, Mình có sẵn sàng cùng Ginsengfood viết lên khát vọng Việt Nam và góp phần vì cuộc sống tốt đẹp hơn cho người dân nghèo vùng cát biển không?. Hãy xem chính sách khởi nghiệp. (Xem ngay)

- Tin nhắn:

Khởi nghiệp cùng Ginsengfood hôm nay tự hào cho tương lai. Nếu có thể Mình hãy cùng lan tỏa giá trị những sản phẩm khoa học từ sâm trồng trên cát biển quý giá, chăm sóc sức khỏe cộng đồng, cùng Ginsengfood thực hiện khát vọng vì cuộc sống tốt đẹp hơn cho người dân vùng cát biển. (Xem chính sách khởi nghiệp. (Xem ngay)

**12\. SLIVER**

\- **Tin nhắn cảm xúc cho** SILVER

- Tin nhắn:

Chúc mừng {{full_name}}…. Bạn đang là thành viên {{current_tier_display_name}} (Ngay từ bây giờ tất cả đơn hàng Bạn được giảm lên đến {{max_discount_percent}} khi mua sản phẩm tại Ginsengfood chương trình 24/7 và quyền mua sớm {{current_tier_early_access_minutes}} trước chương trình giờ vàng mỗi ngày. Trân trọng cảm ơn Bạn

- Tin nhắn:

Để trở thành {{current_tier_display_name}}, tăng đặc quyền nhận quyền giảm lên đến {{max_discount_percent}} và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng, bạn chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Để trở thành {{current_tier_display_name}}, tăng quyền lợi giảm lên đến {{max_discount_percent}} chương trình 24/7 và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng Mình chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Để trở thành {{current_tier_display_name}} tăng quyền lợi giảm lên đến {{max_discount_percent}} chương trình 24/7 và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng Mình chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Em chào Mình ạ, Cháo Sâm Ginsengfood là sản phẩm dược nghiên cứu đựa trên nền tảng y thực đồng nguyên hiện đại kết hợp hài hòa cùng giá trị hơn 4.000 năm y học phương đông và ứng dụng công nghệ hiện đại sấy thăng hoa mang đến cho Mình sản phẩm Cháo Sâm bổ dưỡng, Giữ trọn tinh hoa, Ngon như Mẹ nấu (Mua ngay)

- Tin nhắn:

Em chào mình ạ, hãy chọn cho Mình và gia đình những sản phẩm tuyệt vời của Ginsengfood đồng thời giúp chúng tôi lan tỏa giá trị sản phẩm khoa học, chất lượng bổ dưỡng, tinh túy đến với cộng đồng đó là Mình đang cùng Ssavigroup vì tương lai tươi sáng cho cho người dân nghèo ven biển (Mua ngay)

- Tin nhắn:

Ginsengfood trân trọng cảm ơn Mình đã đồng hành cùng Ginsengfood, Mình có sẵn sàng cùng Ginsengfood viết lên khát vọng Việt Nam và góp phần vì cuộc sống tốt đẹp hơn cho người dân nghèo vùng cát biển không?. Hãy xem chính sách khởi nghiệp. (Xem ngay)

- Tin nhắn:

Khởi nghiệp cùng Ginsengfood hôm nay tự hào cho tương lai. Nếu có thể Mình hãy cùng lan tỏa giá trị những sản phẩm khoa học từ sâm trồng trên cát biển quý giá, chăm sóc sức khỏe cộng đồng, cùng Ginsengfood thực hiện khát vọng vì cuộc sống tốt đẹp hơn cho người dân vùng cát biển. (Xem chính sách khởi nghiệp. (Xem ngay)

**13\. GOLD:**

\- **Tin nhắn cảm xúc cho** GOLD

- Tin nhắn:

Chúc mừng {{full_name}}…. Mình đang là thành viên {{current_tier_display_name}} (Từ bây giờ, tất cả đơn hàng Bạn được giảm lên đến {{max_discount_percent}} khi mua sản phẩm tại Ginsengfood chương trình 24/7 và quyền mua sớm {{current_tier_early_access_minutes}} trước chương trình giờ vàng mỗi ngày. Hãy mua thêm để tích lũy thăng hạn và nhật đặc quyền cao hơn nhé (Mua ngay)

- Tin nhắn:

Để trở thành {{current_tier_display_name}}, nhận quyền lợi giảm lên đến {{max_discount_percent}} khi mua sản phẩm chương trình 24/7 và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng bạn chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Để trở thành {{current_tier_display_name}}, nhận quyền lợi giảm lên đến {{max_discount_percent}} khi mua sản phẩm chương trình 24/7 và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng bạn chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Cháo Sâm là sản phẩm được nghiên cứu dựa trên nền tảng y thực đồng nguyên hiện đại kết hợp hài hòa cùng y học phương đông và ứng dụng công nghệ hiện đại sấy thăng hoa mang đến cho bạn sản phẩm Cháo Sâm bổ dưỡng, Giữ trọn tinh hoa, Ngon như Mẹ nấu (Mua ngay)

- Tin nhắn:

Ginsengfood trân trọng cảm ơn Mình đã đồng hành cùng Ginsengfood, Mình có sẵn sàng cùng viết lên khát vọng Việt Nam và góp phần vì cuộc sống tốt đẹp hơn cho người dân nghèo vùng cát biển không? Hãy trở thành Nhà đồng hành khởi nghiệp làm giàu và tự hào cùng Ginsengfood nhé. Xem chính sách khởi nghiệp (Xem ngay)

- Tin nhắn:

Hãy cùng Ginsengfood lan tỏa giá trị sản phẩm khoa học chất lượng tinh túy bổ dưõng đến với cộng đồng vì góp phần cuộc sống tốt đẹp hơn cho người dân nghèo ven biển bằng cách đăng ký trở thành Nhà khởi nghiệp cùng Ginsengfood nhé. Xem chính sách khởi nghiệp (Xem ngay)

- Tin nhắn:

Khởi nghiệp cùng Ginsengfood hôm nay tự hào cho tương lai. Mình hãy cùng lan tỏa giá trị những sản phẩm khoa học từ sâm trồng trên cát biển quý giá, chăm sóc sức khỏe cộng đồng, cùng Ginsengfood thực hiện khát vọng vì cuộc sống tốt đẹp hơn cho người dân vùng cát biển. Trân trọng cảm ơn. Xem chính sách khởi nghiệp (Xem ngay)

**14\. PLATINUM:**

\- **Tin nhắn cảm xúc cho** PLATINUM

- Tin nhắn:

Chúc mừng {{full_name}} Mình đang là {{current_tier_display_name}}, nhận quyền lợi theo hạng thành viên giảm lên đến {{max_discount_percent}} chương trình 24/7 và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng Mình chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Để trở thành {{current_tier_display_name}} nhà đồng hành khởi nghiệp cùng Ginsengfood, nhận quyền lợi giảm lên đến {{max_discount_percent}} chương trình 14/7 và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng Mình chỉ cần mua thêm ……. VND (Mua ngay)

- Tin nhắn:

Cháo Sâm của Ginsengfood được nghiên cứu dựa trên nền tảng y thực đồng nguyên hiện đại, kết hợp hài hòa kinh nghiệm hơn 4.000 năm của nền y học phương đông đồng thời ứng dụng công nghệ hiện đại sấy thăng hoa. Ginsenfood mang đến cho Mình sản phẩm Cháo Sâm bổ dưỡng, Giữ trọn tinh hoa, Ngon như Mẹ nấu (Mua ngay)

- Tin nhắn:

Cảm ơn {{full_name}} đã ủng hộ sản phẩn của Ginsengfood. Mình không chỉ chọn cho mình và gia đình những sản phẩm khoa học chất lượng bỗ dưỡng, lành mạnh hãy giúp Ginsengfood lan tỏa giá trị sản phẩm đến với cộng đồng Trân trọng cảm ơn ạ. (Mua ngay)

- Tin nhắn:

Hãy cùng Ginsengfood xây dựng nền nông nghiệp giá trị cao trên vùng cát biển vì dược sống tốt đẹp hơn cho người dân nghèo ven biển, vì sức khỏe cộng đồng vì sức khỏe bằng cách đăng ký trở thành Nhà khởi nghiệp cùng Ginsengfood nhé (Xem ngay)

- Tin nhắn:

Khởi nghiệp cùng Ginsengfood hôm nay tự hào cho tương lai. Hãy cùng lan tỏa giá trị những sản phẩm khoa học từ sâm trồng trên cát biển quý giá, chăm sóc sức khỏe cộng đồng, cùng Ginsengfood thực hiện khát vọng vì cuộc sống tốt đẹp hơn cho người dân vùng cát biển. khởi nghiệp cùng Ginsengfood nhé (Xem ngay)

**15\. DIAMOND:**

\- **Tin nhắn cảm xúc cho** DIAMOND

- Tin nhắn:

Giờ Vàng sẽ bắt đầu {{golden_hour_start_time}}. {{full_name}} được quyền mua trước sau … phút nữa. Ưu tiên thời điểm chốt đơn hợp lệ. Hãy chia sẽ link của Mình để được nhận chiết khấu hoa hồng nhé {{referral_link}}

.

- Tin nhắn:

Hãy gửi link thành viên của Mình cho người mua hàng, Mình sẽ được chiết khấu {{commission_percent}} chương trình 24/7 và {{commission_percent}}trong chương trình giờ vàng {{referral_link}}.

- Tin nhắn:

Mình hãy nói với người Mình muốn giới thiệu mua hàng (Với đặc quyền của Tôi khi Anh/Chị mua bất kỳ sản phẩm nào từ link của Tôi, Anh/Chị sẽ được giảm lên đến {{max_discount_percent}} trong chương trình 24/7 của Ginsengfood) và quyền mua sớm {{current_tier_early_access_minutes}} chương trình giờ vàng {{referral_link}}.

- Tin nhắn:

Chúc mừng {{full_name}} Mình đang là {{current_tier_display_name}}, nhận quyền lợi giảm lên đến {{max_discount_percent}} khi mua sản phẩm chương trình 24/7 và đặc quyền mua sớm {{current_tier_early_access_minutes}} trước giờ vàng (Mua ngay)

- Tin nhắn:

Bây giờ Mình là {{current_tier_display_name}}, Nhà đồng hành Khởi nghiệp cùng Ginsengfood. Mình được hưởng nhiều đặc quyền và cơ hội kinh doanh mang đến thu nhập cao nhưng giá trị lớn hơn chính là Mình đang cùng Tập đoàn Ssavigroup cùng nhau vì sức khỏe cộng đồng, vì một tương lai tươi sáng hơn cho người dân vùng cát biển và khát vọng quốc gia dẫn đầu bằng các sản phẩm từ sâm Trân trọng chào mừng và cảm ơn Mình nhé {{policy_diamond_link}}

- Tin nhắn:

Trong chương trình giờ vàng, Mình hãy nói với người Mình muốn giới thiệu mua hàng (Với đặc quyền của Tôi Anh/Chị mua hàng tại link của tôi sẽ được quyền mua sớm {{current_tier_early_access_minutes}}, số lượng sản phẩm chỉ có {{quota_per_sku}} Anh/Chị không phải tranh suất với mọi người. {{referral_link}}

- Tin nhắn:

Tại giao diện Diamond Mình ngoài theo dõi doanh thu, hoa hồng thì bạn có thể vào các kho bài viết, kho video, kho hình ảnh lấy ảnh, nội dung để đăng trên mạng xã hội bán hàng và nhận chiết khấu nhé. {{policy_diamond_link}}

- Tin nhắn:

Hiện tại TikTok, Shopee không cho gắn link do vậy Mình đừng bao giờ gắn link vào 2 nền tảng này nhé, trừ khi Mình tạo giỏ hàng trên các nền tảng đó… Mình nhớ nhé.

- Tin nhắn:

Chỉ cần Mình vào Ginsengfood Mình sẽ thấy mục nội dung bài viết, video, hình ảnh trên thanh công cụ. Mình có thể tải nội dung, hình ảnh video xuống để đăng bài trên Facebook, Zalo, Instagram bán hàng và nhận chiết khấu hoa hồng nhé {{policy_diamond_link}}

- Tin nhắn:

Chào buổi sáng thật nhiều năng lượng và hạnh phúc thành công nhé, Ginsengfood trân trọng cảm ơn Mình đã đồng hành cùng Ginsengfood, chúng ta sẽ cùng nhau viết lên khát vọng Việt Nam và góp phần vì cuộc sống tốt đẹp hơn cho người dân nghèo vùng cát biển.

- Tin nhắn:

Mỗi đơn hàng Mình giới thiệu thành công mang đến giá trị rất lớn không chỉ mang đến sức khỏe cho cộng đồng mà Mình đang cùng Tập đoàn Ssavigroup, doanh nghiệp khoa học Việt Nam xây dựng nền nông nghiệp giá trị cao trên vùng cát biển và góp phần vì cuộc sống tốt đẹp hơn cho người dân nơi đây. Trân trọng cảm ơn ạ.

- Tin nhắn:

Nền tảng y thực đồng nguyên hiện đại kết hợp hài hòa cùng giá trị hơn 4.000 năm của nền y học phương đông và ứng dụng công nghệ hiện đại sấy thăng hoa mang đến cho bạn sản phẩm Cháo Sâm bổ dưỡng, Giữ trọn tinh hoa, Ngon như Mẹ nấu. (mua ngay)

- Tin nhắn: Hãy cùng Ginsengfood lan tỏa giá trị sản phẩm khoa học, đến với cộng đồng vì sức khỏe và tương lai tươi sáng cho dân tộc Việt Nam, vì hạnh phúc và cuộc sống tốt đẹp hơn cho người dân nghèo ven biển. Xem chính sách khởi nghiệp cùng Ginsengfood nhé (Xem ngay)

……………………………………………….

Tin nhắn chăm sóc khách hàng sau mua  
mục tiêu là chăm sóc khác khàng đã mua trở thành khách hàng trung thành ( thành viên), biến thành viên thành nhà đồng khành khởi nghiệp

Quy tắc chăm sóc phải dựa vào các quy định như sau:

Với khách mới và New:

- hỏi thăm dùng ngon không, có cần tư vấn gì không
- hỏi trải nghiệm của khách về phục vụ có tốt không
- nhắc mua tiếp
- giới thiệu sản phẩm khác, sản phẩm mới
- nhắc giờ vàng
- nhắc đắng ký thành viên nếu chưa đăng ký
- nhắc thăng hạng và kịch hoạt quyền lợi cao hơn
- nhắc giờ vàng sắp diễn ra, và đang diễn ra

Với Diamond:

- nhắc giờ vàng sắp diễn ra,
- Nhắc gửi link cho khách mua tạo doanh thu
- Nhắc đăng bài giới thiệu
- Hướng dẫn lấy tài nguyên đăng bài

Thực hiện: đề nghị CHAT GPT Pro viết chi tiết phần chăm sóc sau mua và khóa kỹ thuật phù hợp với quy tắc giao tiếp chăm sóc khách hàng đỉnh cao mà những nền tảng lớn đang dùng, nội tạo cảm xúc gắn kết để người dùng luôn cảm thấy được quan tâm, trân trọng dể cho họ luôn hạnh phúc với Ginsengfood. Tầng xuất gửi tin không gian thời gian phù hợp để tạo cảm xúc tốt và tối ưu hóa CRM tốt nhát
