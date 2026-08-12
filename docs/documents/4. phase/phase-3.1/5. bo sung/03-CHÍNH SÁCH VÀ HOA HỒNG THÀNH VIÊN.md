**THÀNH VIÊN, QUY TẮC GIÁ & CHƯƠNG TRÌNH GIÁ**

**MEMBER TIER / PRICE POLICY / PROGRAM 24/7 / GOLDEN HOUR TRI ÂN CORE LOCK**

**CANONICAL UPDATE 2026-06-05:** Tài liệu này đã được chuẩn hóa theo `PRICE_POLICY_V2026_06_CANONICAL_001` và `GOLDEN_HOUR_POLICY_V2026_06_CANONICAL_001`. Các giá trị runtime hiện hành: 24/7 mở bán ban đầu 9%, dưới 50 hộp/ngày 9%, từ 50 hộp/ngày trở lên 5%; Giờ Vàng 12h00-12h45 và 20h00-20h45. Các đoạn dưới đây là source active sau cleanup; mọi nhánh giá/giờ legacy chỉ còn giá trị lịch sử, không phải runtime.

**1\. Mục đích**

Tài liệu này dùng để khóa các chính sách lõi liên quan đến:

- Chu kỳ thành viên.
- Tiêu chuẩn xếp hạng thành viên.
- Quyền lợi giảm giá theo hạng thành viên.
- Quy tắc duy trì hạng thành viên.
- Quy tắc giá niêm yết, giá chương trình và giá bán.
- Quy tắc tự động mở bán / ngưng bán sản phẩm.
- Chương trình giảm giá 24/7.
- Chương trình Live Giờ Vàng Tri Ân.
- Quyền mua sớm theo hạng thành viên.
- Quyền lợi người mua qua link Diamond.
- Hoa hồng Diamond cho đơn hàng giới thiệu thành công.
- Nguyên tắc không cộng dồn quyền lợi.

**PHẦN I - CHÍNH SÁCH THÀNH VIÊN**

**2\. Chu kỳ thành viên**

Chu kỳ thành viên được tính trong **12 tháng kể từ ngày khách hàng được công nhận hạng thành viên**.

member_cycle_duration = 12 tháng  
cycle_start_date = ngày được công nhận hạng thành viên  
cycle_end_date = cycle_start_date + 12 tháng

Hết chu kỳ 12 tháng, hệ thống sẽ kiểm tra doanh số mua hàng hợp lệ để xác định:

- Thành viên được duy trì hạng hiện tại.
- Thành viên được nâng hạng.
- Thành viên bị hạ hạng.
- Thành viên được đưa vào thời gian ân hạng nếu chưa đạt điều kiện duy trì.

**3\. Tiêu chuẩn xếp hạng thành viên**

Hạng thành viên được xác định dựa trên **doanh số mua hàng hợp lệ trong chu kỳ thành viên**.

| **Hạng thành viên** | **Doanh số mua hàng hợp lệ trong chu kỳ** |
| ------------------- | ----------------------------------------- |
| Silver              | Từ 1.000.000đ đến 2.999.999đ              |
| Gold                | Từ 3.000.000đ đến 4.999.999đ              |
| Platinum            | Từ 5.000.000đ đến 7.999.999đ              |
| Diamond             | Từ 8.000.000đ trở lên                     |

**3.1. Doanh số mua hàng hợp lệ**

Doanh số mua hàng hợp lệ là doanh số từ các đơn hàng đã được xác nhận thành công theo Order Runtime.

Doanh số được tính khi đơn hàng đạt trạng thái hợp lệ, ví dụ:

ORDER_VERIFIED  
PAID  
COD_VERIFIED

Không tính vào doanh số thành viên các trường hợp:

- Đơn nháp.
- Quote chưa xác nhận.
- Đơn hủy.
- Đơn hoàn.
- Đơn lỗi.
- Đơn test.
- Đơn trùng idempotency.
- Đơn chưa qua xác nhận hợp lệ.
- Đơn bị phát hiện gian lận hoặc không hợp lệ.

**4\. Quyền lợi giảm giá theo hạng thành viên**

| **Hạng thành viên** | **Quyền lợi giảm giá** |
| ------------------- | ---------------------- |
| Silver              | Giảm 5%                |
| Gold                | Giảm 8%                |
| Platinum            | Giảm 12%               |
| Diamond             | Giảm 20%               |

Quyền lợi thành viên chỉ được áp dụng khi:

- Hạng thành viên còn hiệu lực.
- Thành viên đang trong chu kỳ hợp lệ hoặc thời gian ân hạng hợp lệ.
- Chương trình bán hàng tại thời điểm mua cho phép áp dụng quyền lợi thành viên.
- Runtime Core xác nhận quyền lợi hợp lệ tại thời điểm tạo quote.
- QuoteSnapshot ghi nhận đầy đủ quyền lợi được áp dụng.

AI, content block, nhân viên vận hành hoặc channel gateway không được tự tính quyền lợi thành viên ngoài Runtime Core.

**5\. Duy trì hạng thành viên**

Để duy trì hạng thành viên, khách hàng cần đạt tối thiểu **50% doanh số chuẩn của hạng hiện tại** trong chu kỳ thành viên.

| **Hạng hiện tại** | **Doanh số chuẩn của hạng** | **Doanh số tối thiểu để duy trì hạng** |
| ----------------- | --------------------------- | -------------------------------------- |
| Silver            | 1.000.000đ                  | 500.000đ                               |
| Gold              | 3.000.000đ                  | 1.500.000đ                             |
| Platinum          | 5.000.000đ                  | 2.500.000đ                             |
| Diamond           | 8.000.000đ                  | 4.000.000đ                             |

Nếu hết chu kỳ 12 tháng mà thành viên chưa đạt doanh số duy trì hạng, hệ thống đưa thành viên vào **thời gian ân hạng 60 ngày**.

grace_period = 60 ngày

Trong thời gian ân hạng:

- Thành viên tạm thời được giữ hạng hiện tại.
- Hệ thống tiếp tục theo dõi doanh số mua hàng hợp lệ.
- Nếu thành viên đạt điều kiện duy trì trong 60 ngày, hạng được giữ lại.
- Nếu hết 60 ngày vẫn không đạt, hệ thống thực hiện hạ hạng theo doanh số thực tế.
- Nếu doanh số trong thời gian ân hạng đủ điều kiện nâng hạng, hệ thống có thể nâng hạng theo chính sách thành viên.

**PHẦN II - QUY TẮC GIÁ**

**6\. Khái niệm giá**

**6.1. Giá niêm yết chính thức**

Giá niêm yết chính thức là giá gốc của từng SKU do Owner phê duyệt.

Giá niêm yết có thể được cập nhật sau khi có giá chính thức. Khi giá niêm yết chính thức được phê duyệt và active, hệ thống mới được dùng giá này để tính chương trình bán hàng.

listed_price_status = ACTIVE

Nếu SKU chưa có giá niêm yết chính thức active, hệ thống không được mở bán SKU đó.

**6.2. Giá chương trình**

Giá chương trình là mức giá sau khi áp dụng chương trình giảm giá hợp lệ tại thời điểm bán.

Công thức:

Giá bán chương trình = Giá niêm yết chính thức - Mức giảm chương trình hợp lệ

Ví dụ:

Giá niêm yết: 100.000đ  
Chương trình giảm 9%  
Giá bán chương trình = 100.000đ - 9% = 91.000đ

**6.3. Giá thanh toán cuối**

Giá thanh toán cuối là số tiền khách hàng phải thanh toán sau khi hệ thống áp dụng:

- Giá niêm yết chính thức.
- Chương trình giá đang active.
- Quyền lợi thành viên hợp lệ nếu được phép áp dụng.
- Quyền lợi người mua qua link Diamond nếu được phép áp dụng.
- Phí vận chuyển nếu có.
- Phí COD nếu có.
- VAT nếu có.

Công thức tổng quát:

Giá thanh toán cuối = Giá bán chương trình - Quyền lợi hợp lệ tại thời điểm quote + Phí phát sinh hợp lệ

Tất cả giá bán cuối cùng phải được lấy từ **QuoteSnapshot**. Không thành phần nào được tự tính giá bên ngoài Runtime Core.

**PHẦN III - QUY TẮC MỞ BÁN SẢN PHẨM**

**7\. Điều kiện tự động mở bán**

Một SKU chỉ được hệ thống tự động kích hoạt mở bán khi đồng thời thỏa các điều kiện sau:

- Thành phẩm đã hoàn tất quy trình sản xuất.
- Thành phẩm đã nhập kho hợp lệ.
- SKU có tồn kho khả dụng để bán.
- SKU không bị quality hold.
- SKU không bị recall hold.
- SKU không bị sale lock.
- SKU không bị channel block.
- SKU có giá niêm yết chính thức đã được phê duyệt và active.
- SKU có trạng thái được phép bán trong Runtime Core.

sellable_status = SELLABLE  
listed_price_status = ACTIVE  
stock_available > 0

Khi các điều kiện trên đạt, hệ thống được phép tự động kích hoạt mở bán SKU.

**8\. Điều kiện tự động ngưng bán**

Hệ thống tự động ngưng bán SKU khi xảy ra một trong các điều kiện sau:

- Hết hàng.
- Tồn kho khả dụng bằng 0.
- SKU bị quality hold.
- SKU bị recall hold.
- SKU bị sale lock.
- SKU bị channel block.
- Giá niêm yết bị inactive.
- SKU bị ẩn tạm thời khỏi chương trình bán hàng.
- Runtime Core xác định SKU không còn đủ điều kiện bán.

sellable_status != SELLABLE

Khi SKU không sellable, AI và hệ thống bán hàng không được:

- Báo quote cho SKU đó.
- Tạo đơn cho SKU đó.
- Public số lượng tồn kho.
- Nói "chỉ còn vài hộp" để tạo khan hiếm giả.
- Gợi ý sản phẩm thay thế nếu sản phẩm thay thế chưa qua guard.

**PHẦN IV - CHƯƠNG TRÌNH GIẢM GIÁ 24/7**

**9\. Mục đích chương trình 24/7**

Chương trình 24/7 là chương trình giá vận hành xuyên suốt toàn hệ thống Ginsengfood.

Chương trình này tự động xác định mức giảm giá theo từng SKU dựa trên **lượng bán thành công của SKU trong ngày trước đó**.

**10\. Nguyên tắc giá mở bán ban đầu**

Khi một SKU lần đầu được mở bán và chưa có lịch sử bán hàng trước đó, hệ thống áp dụng mức giảm mặc định:

Giá bán mở bán ban đầu = Giá niêm yết chính thức - 9%

Mức giảm mặc định cho SKU đầu tiên mở bán:

initial_24_7_discount = 9%

**11\. Nguyên tắc tính chương trình 24/7 hằng ngày**

Hệ thống căn cứ vào số lượng bán thành công của từng SKU trong ngày để xác định mức giảm giá cho ngày tiếp theo.

sales_count_day_D → discount_day_D+1

| **Số lượng bán thành công của SKU trong ngày** | **Mức giảm áp dụng cho ngày tiếp theo** |
| ---------------------------------------------- | --------------------------------------- |
| Dưới 50 sản phẩm/ngày                          | Giảm 9%                                 |
| Từ 50 sản phẩm/ngày trở lên                    | Giảm 5%                                 |

Tôi đề xuất khóa kỹ thuật theo mốc:

Nếu successful_sales_count < 50 → discount = 9%  
Nếu successful_sales_count >= 50 → discount = 5%

Cách này tránh khoảng trống ở mốc đúng 50 sản phẩm/ngày.

**12\. Doanh số / số lượng bán thành công dùng cho 24/7**

Số lượng bán thành công dùng để tính chương trình 24/7 là số lượng sản phẩm thuộc SKU đã được ghi nhận trong đơn hàng hợp lệ.

Chỉ tính các đơn hàng:

- Đã được xác nhận thành công.
- Đã qua checkpoint hợp lệ của Order Engine.
- Không bị hủy.
- Không bị hoàn.
- Không phải đơn test.
- Không phải đơn trùng.
- Không phải đơn lỗi.

Không tính quote, giỏ hàng, đơn nháp hoặc đơn chưa xác nhận.

**13\. Quyền lợi thành viên trong chương trình 24/7**

Khi khách hàng mua trong chương trình 24/7, thành viên được hưởng quyền lợi giảm giá theo hạng thành viên nếu Runtime Core xác nhận đủ điều kiện.

| **Hạng thành viên** | **Quyền lợi trong 24/7**  |
| ------------------- | ------------------------- |
| Silver              | Giảm 5% nếu đủ điều kiện  |
| Gold                | Giảm 8% nếu đủ điều kiện  |
| Platinum            | Giảm 12% nếu đủ điều kiện |
| Diamond             | Giảm 20% nếu đủ điều kiện |

Quyền lợi này không được tự động cộng dồn với các quyền lợi khác nếu policy không cho phép.

**14\. Người mua qua link Diamond trong chương trình 24/7**

Người mua qua link Diamond hợp lệ được hưởng quyền lợi:

buyer_from_diamond_link_24_7_discount = 5%

Mức giảm này áp dụng trên giá bán của chương trình 24/7 nếu Diamond Referral Resolver xác nhận link hợp lệ.

Điều kiện áp dụng:

- Link Diamond hợp lệ.
- Bind trạng thái hợp lệ.
- Người mua phát sinh đơn hàng qua link Diamond.
- Đơn hàng được xác nhận thành công.
- Runtime Core xác nhận được phép áp dụng quyền lợi.
- Không cộng dồn với quyền lợi khác nếu chính sách không cho phép.

**PHẦN V - CHƯƠNG TRÌNH LIVE GIỜ VÀNG TRI ÂN**

**15\. Mục đích chương trình Giờ Vàng Tri Ân**

Giờ Vàng Tri Ân là chương trình bán hàng theo phiên, được mở dựa trên tồn kho khả dụng của từng SKU.

Chương trình này dùng để:

- Tạo phiên bán hàng tập trung.
- Ưu tiên quyền mua sớm cho thành viên.
- Tạo giới hạn quota bán theo phiên.
- Tăng hiệu quả live commerce.
- Kết nối với AI tư vấn, Messenger, QuoteSnapshot và IVR xác nhận đơn.

**16\. Số phiên Giờ Vàng trong ngày**

Tối đa có **2 phiên Giờ Vàng Tri Ân mỗi ngày**.

| **Phiên** | **Thời gian chính thức** |
| --------- | ------------------------ |
| Phiên 1   | 12h00 - 12h45            |
| Phiên 2   | 20h00 - 20h45            |

**17\. Quy tắc mở phiên theo tồn kho SKU**

Giờ Vàng Tri Ân được mở theo tồn kho khả dụng của từng SKU.

| **Tồn kho khả dụng của SKU**     | **Quy tắc mở phiên**                            |
| -------------------------------- | ----------------------------------------------- |
| Dưới 300 sản phẩm/SKU            | Không mở Giờ Vàng cho SKU đó                    |
| Từ 300 đến dưới 500 sản phẩm/SKU | Mở 1 phiên/ngày: 20h00 - 20h45                  |
| Từ 500 sản phẩm/SKU trở lên      | Mở 2 phiên/ngày: 12h00 - 12h45 và 20h00 - 20h45 |

Tôi đề xuất khóa kỹ thuật bằng dấu lớn hơn hoặc bằng:

sellable_stock < 300 → không mở Giờ Vàng  
300 <= sellable_stock < 500 → mở 1 phiên tối  
sellable_stock >= 500 → mở 2 phiên

Cách này tránh khoảng trống ở đúng mốc 300 và 500 sản phẩm.

**18\. Quota bán trong Giờ Vàng**

Số lượng bán trong Giờ Vàng:

2.000 sản phẩm / phiên

Khi bán đủ số lượng chương trình theo quota, hệ thống tự động kết thúc bán hàng của phiên.

Để tránh oversell, hệ thống cần quản lý quota theo 2 lớp:

session_quota = 2.000 sản phẩm / phiên  
session_sku_quota <= sellable_stock của từng SKU

Không được cho phép bán vượt quá tồn kho khả dụng.

**19\. Kết thúc phiên Giờ Vàng**

Một phiên Giờ Vàng kết thúc khi xảy ra một trong các điều kiện sau:

- Hết thời gian phiên.
- Đủ quota 2.000 sản phẩm/phiên.
- SKU hết tồn kho khả dụng.
- SKU bị sale lock.
- SKU bị quality hold.
- SKU bị recall hold.
- Runtime Core dừng phiên theo policy vận hành.

**20\. Quy tắc chờ xác nhận qua IVR**

Sau khi khách đặt hàng trong Giờ Vàng, hệ thống áp dụng thời gian chờ xác nhận qua IVR thêm:

IVR_confirmation_extra_time = 5 phút

Ý nghĩa:

- Khách có thêm thời gian xác nhận đơn qua IVR.
- Reservation/quote/order draft không được giữ vô hạn.
- Nếu hết thời gian xác nhận mà khách không xác nhận theo rule, hệ thống xử lý expire theo chính sách Giờ Vàng.
- Hàng/quota có thể được trả lại vào pool nếu đơn không được xác nhận hợp lệ.

**PHẦN VI - QUYỀN MUA SỚM TRONG GIỜ VÀNG**

**21\. Quyền mua sớm theo hạng thành viên**

Thành viên được quyền mua trước thời điểm Giờ Vàng chính thức theo số phút sau:

| **Hạng thành viên** | **Quyền mua sớm** |
| ------------------- | ----------------- |
| Silver              | Mua trước 5 phút  |
| Gold                | Mua trước 10 phút |
| Platinum            | Mua trước 20 phút |
| Diamond             | Mua trước 30 phút |

**22\. Quyền mua sớm của người mua qua link Diamond**

Người mua qua link Diamond hợp lệ được quyền mua sớm:

buyer_from_diamond_link_early_access = 5 phút

Điều kiện áp dụng:

- Link Diamond hợp lệ.
- Người mua được ghi nhận là đến từ link Diamond.
- Bind link hợp lệ tại thời điểm vào phiên.
- Phiên Giờ Vàng đang tồn tại.
- SKU trong phiên còn đủ điều kiện bán.

**23\. Bảng thời gian mua sớm theo phiên**

**Phiên 1: 12h00 - 12h45**

| **Đối tượng**              | **Thời điểm được vào mua** |
| -------------------------- | -------------------------- |
| Diamond                    | 11h30                      |
| Platinum                   | 11h40                      |
| Gold                       | 11h50                      |
| Silver                     | 11h55                      |
| Người mua qua link Diamond | 11h55                      |
| Khách thường               | 12h00                      |

**Phiên 2: 20h00 - 20h45**

| **Đối tượng**              | **Thời điểm được vào mua** |
| -------------------------- | -------------------------- |
| Diamond                    | 19h30                      |
| Platinum                   | 19h40                      |
| Gold                       | 19h50                      |
| Silver                     | 19h55                      |
| Người mua qua link Diamond | 19h55                      |
| Khách thường               | 20h00                      |

**PHẦN VII - QUYỀN LỢI DIAMOND**

**24\. Quyền lợi người mua qua link Diamond**

Người mua qua link Diamond hợp lệ có 2 quyền lợi chính:

| **Chương trình**  | **Quyền lợi người mua qua link Diamond**                     |
| ----------------- | ------------------------------------------------------------ |
| Chương trình 24/7 | Được giảm 5% trên giá bán chương trình 24/7 nếu đủ điều kiện |
| Giờ Vàng Tri Ân   | Được quyền mua sớm 5 phút trước giờ chính thức               |

Các quyền lợi này chỉ áp dụng khi Diamond Referral Resolver xác nhận link Diamond hợp lệ.

**25\. Hoa hồng Diamond cho đơn hàng giới thiệu thành công**

Diamond được hưởng hoa hồng cho đơn hàng giới thiệu thành công qua link Diamond.

| **Chương trình phát sinh đơn** | **Hoa hồng Diamond** |
| ------------------------------ | -------------------- |
| Chương trình 24/7              | 15%                  |
| Chương trình Giờ Vàng Tri Ân   | 10%                  |

Hoa hồng chỉ được ghi nhận khi:

- Đơn hàng phát sinh qua link Diamond hợp lệ.
- Bind link hợp lệ.
- Đơn hàng được xác nhận thành công.
- Đơn hàng không bị hủy.
- Đơn hàng không bị hoàn.
- Đơn hàng không phải đơn test.
- Đơn hàng không trùng idempotency.
- Runtime Core xác nhận đơn đủ điều kiện tính hoa hồng.

**PHẦN VIII - NGUYÊN TẮC KHÔNG CỘNG DỒN QUYỀN LỢI**

**26\. Nguyên tắc chung**

Trong tất cả tình huống, quyền lợi không được tính cộng dồn tùy tiện.

Khách mua tại chương trình nào thì được hưởng quyền lợi hợp lệ của chương trình tại thời điểm đó.

Không cộng dồn nhiều quyền lợi nếu policy không cho phép.  
Không tự cộng member benefit + Diamond buyer benefit + chương trình khác ngoài runtime.  
Không cộng quyền lợi 24/7 với quyền lợi Giờ Vàng nếu chương trình không cho phép.

**27\. Nguyên tắc áp dụng theo chương trình**

**27.1. Chương trình 24/7**

Trong chương trình 24/7:

- SKU có giá chương trình 24/7 theo mức 9% hoặc 5%.
- Thành viên có thể được hưởng quyền lợi theo hạng nếu đủ điều kiện.
- Người mua qua link Diamond có thể được hưởng giảm 5% nếu đủ điều kiện.
- Nếu khách đồng thời đủ nhiều quyền lợi, hệ thống phải chọn quyền lợi theo policy priority được cấu hình.
- Không cộng dồn quyền lợi nếu policy không cho phép.

**27.2. Chương trình Giờ Vàng Tri Ân**

Trong Giờ Vàng Tri Ân:

- Thành viên được quyền mua sớm theo hạng.
- Người mua qua link Diamond được quyền mua sớm 5 phút.
- Quota phiên được kiểm soát bởi Golden Hour Engine.
- Giá bán, nếu có cấu hình giá riêng cho Giờ Vàng, phải lấy từ Runtime Core / GoldenHourResolver / ProgramPriceResolver.
- AI không được tự bịa giá Giờ Vàng.
- Không áp dụng thêm member discount hoặc Diamond buyer discount trong Giờ Vàng; quyền lợi mặc định của member/Diamond buyer trong Giờ Vàng chỉ là early access. Mọi ngoại lệ phải nằm trong QuoteSnapshot với policy version riêng.

**28\. Policy Priority khi khách có nhiều quyền lợi**

Nếu một khách hàng đồng thời đủ điều kiện nhiều quyền lợi, hệ thống phải xử lý theo policy_priority.

Ví dụ các trường hợp cần policy priority:

- Khách vừa là thành viên Gold vừa mua qua link Diamond.
- Khách Diamond tự mua trong chương trình 24/7.
- Khách member mua trong phiên Giờ Vàng.
- Khách qua link Diamond cũng là thành viên cũ.
- Khách đang trong thời gian ân hạng nhưng có quyền mua sớm.

Nguyên tắc khóa:

Runtime Core quyết định quyền lợi hợp lệ.  
AI không tự chọn quyền lợi.  
Content không tự nói khách được giảm thêm.  
QuoteSnapshot phải ghi rõ quyền lợi nào đã được áp dụng.

**PHẦN IX - QUY TẮC QUOTE / ORDER LIÊN QUAN GIÁ**

**29\. QuoteSnapshot bắt buộc**

Mọi báo giá cho khách phải được tạo từ QuoteSnapshot.

QuoteSnapshot phải ghi nhận:

- Giá niêm yết chính thức.
- Chương trình giá đang áp dụng.
- Mức giảm chương trình.
- Quyền lợi thành viên nếu có.
- Quyền lợi link Diamond nếu có.
- Phí vận chuyển nếu có.
- Phí COD nếu có.
- VAT nếu có.
- Giá thanh toán cuối.
- Thời điểm tạo quote.
- Thời điểm hết hạn quote.
- Chương trình áp dụng tại thời điểm quote.
- Attribution nếu khách đến từ Live / Ads / Diamond link.

**30\. Không được báo giá nếu thiếu dữ liệu lõi**

Không được báo giá nếu thiếu một trong các dữ liệu sau:

- SKU chưa sellable.
- SKU chưa có giá niêm yết active.
- Chưa có chương trình giá hợp lệ.
- Không xác định được quyền lợi khách hàng.
- QuoteSnapshot tạo lỗi.
- Runtime Core không trả final price.
- Khách đang ở public surface như live/comment.
- Sản phẩm hết hàng hoặc sale locked.

**PHẦN X - DONE GATE**

Tài liệu này đạt chuẩn khi:

- Đã khóa chu kỳ thành viên 12 tháng.
- Đã khóa ngưỡng doanh số Silver / Gold / Platinum / Diamond.
- Đã khóa quyền lợi giảm giá 5% / 8% / 12% / 20%.
- Đã khóa duy trì hạng bằng 50% doanh số chuẩn của hạng.
- Đã khóa thời gian ân hạng 60 ngày.
- Đã khóa giá niêm yết chỉ active khi có giá chính thức.
- Đã khóa điều kiện tự động mở bán khi có hàng và giá active.
- Đã khóa điều kiện tự động ngưng bán khi hết hàng hoặc SKU không sellable.
- Đã khóa chương trình 24/7: mở bán ban đầu giảm 9%.
- Đã khóa chương trình 24/7: dưới 50 sản phẩm/ngày giảm 9%.
- Đã khóa chương trình 24/7: từ 50 sản phẩm/ngày trở lên giảm 5%.
- Đã khóa quyền lợi thành viên trong chương trình 24/7.
- Đã khóa người mua qua link Diamond trong 24/7 được giảm 5%.
- Đã khóa Giờ Vàng tối đa 2 phiên/ngày.
- Đã khóa phiên 1: 12h00 - 12h45.
- Đã khóa phiên 2: 20h00 - 20h45.
- Đã khóa tồn kho từ 300 đến dưới 500 mở 1 phiên tối.
- Đã khóa tồn kho từ 500 trở lên mở 2 phiên.
- Đã khóa quota 2.000 sản phẩm/phiên.
- Đã khóa IVR cộng thêm 5 phút chờ xác nhận.
- Đã khóa quyền mua sớm Silver 5 phút, Gold 10 phút, Platinum 20 phút, Diamond 30 phút.
- Đã khóa người mua qua link Diamond được mua sớm 5 phút.
- Đã khóa hoa hồng Diamond 15% trong 24/7.
- Đã khóa hoa hồng Diamond 10% trong Giờ Vàng.
- Đã khóa nguyên tắc không cộng dồn quyền lợi.
- Đã khóa QuoteSnapshot là nguồn báo giá duy nhất.
- Đã khóa AI không được tự tính giá, tự bịa quyền lợi, tự bịa chương trình.
