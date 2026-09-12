# BÁO CÁO TIẾN ĐỘ MODULE 8 — IVR XÁC NHẬN ĐƠN HÀNG

**Kỳ báo cáo:** 06/09 → 12/09/2026 (7 ngày) · **Người thực hiện:** Nguyễn Quốc Toàn · **Mốc code:** `2613da5` (11/09 13:14)

**Trạng thái điều hành:** `RELEASE_BLOCKED — CHỜ MODULE 3`. Phần mềm chạy trọn vòng gọi ở chế độ giả lập, **986 bài kiểm thử xanh hoàn toàn**, blocker nội bộ cuối cùng của tuần trước đã đóng, và ba lỗi phát hiện sáng 12/09 cũng đã sửa xong trong ngày. Không còn việc kỹ thuật nào của riêng Module 8 chặn tiến độ; thứ đang chặn là **11 cổng bên ngoài**, gần nhất là **Module 3 chưa phản hồi phiếu chốt hợp đồng gửi ngày 10/09**. Trong kỳ có 130 lưu vào `main`, cây làm việc sạch, hai kho GitLab và GitHub đã đồng bộ, tổng lịch sử 328 lưu. **Giao diện quản trị đã bị xoá khỏi module** theo chỉ đạo 05/09 — Module 8 nay thuần backend, Module 3 tự làm màn hình.

## 1. TIẾN ĐỘ

| Hạng mục | Mức đã xây | Hiện trạng kiểm chứng tại `HEAD` |
| --- | ---: | --- |
| Nhận đơn, xác thực, chống gọi trùng | 95% | Lược đồ nhận việc đã buộc vào hợp đồng sinh tự động; một luật chống trùng duy nhất cho cả hai bề mặt |
| Vòng đời cuộc gọi, mã kết quả, lập lịch & chính sách gọi | 93% | 5 kịch bản chạy trọn vòng; lỗi kỹ thuật và số sai không bị tính là lượt gọi khách; khung giờ 08:00–21:08 theo chữ ký chủ dự án, tối đa 2 lượt, hai hàng rào thu hồi việc giữa chừng |
| Hợp đồng API & đặc tả | 92% | 38 lệnh, bản `draft.27`, cổng đóng băng hợp đồng 15/15; phiếu chốt đã phát cho Module 3 |
| Dữ liệu, nâng cấp CSDL & cổng an toàn thời gian chạy | 90% | 25 bản nâng cấp, **cổng chạy song song hai phiên bản đã xanh trong kỳ**; bảng phê duyệt trong CSDL, luật bốn mắt, công tắc khẩn luôn tắt được kể cả khi thiếu quyền |
| Trả kết quả về Module 3 | 88% | Hàng đợi gửi lại, thư mục lỗi, đủ bộ mã xác nhận; **chưa có đầu nhận thật để đối soát** |
| Giọng đọc, quyền riêng tư & tuân thủ | 84% | Ngân hàng clip A–E đã sinh đủ; bỏ tên khách khỏi lời thoại nên không phải gửi dữ liệu khách ra ngoài; xử lý yêu cầu xoá dữ liệu, chặn lộ tên, mẫu hồ sơ lưu trữ cho pháp lý — nhưng **chưa duyệt giọng và chưa có chữ ký nào** |
| Triển khai, CI, quan sát, hiệu năng & bằng chứng | 47% | 39 đầu việc CI đã khai, cổng đóng gói nay 5/5 đạt, nhưng **chưa lượt nào chạy trên máy chủ thật** — và hôm nay chứng minh đúng cái giá của việc đó (mục 4.1); mô hình dung lượng chưa hiệu chỉnh; chưa chạy tải; 200/254 dòng chưa có gói bằng chứng |
| **TỔNG THỂ (phần chức năng)** | **~85%** | Ước lượng kỹ thuật ở chế độ giả lập; **không phải 85% đã nghiệm thu**, và chưa nói gì về mức sẵn sàng gọi khách thật |

**Quy mô thay đổi trong kỳ:** 130 lưu (07/09: 20 · 08/09: 31 · 09/09: 49 · 10/09: 24 · 11/09: 6) · 532 file · `+45.913 / −30.486` dòng · 77 hạng mục `W-0203`→`W-0279`.

## 2. NHỮNG VIỆC ĐÃ LÀM

| Ngày | Lưu | Kết quả chính |
| --- | ---: | --- |
| 07/09 | 20 | Dựng hồ sơ cấu hình chạy trọn vòng có tiêm lỗi; cổng chặn trạng thái nháp bị sửa thành chữ ký; chạy lại toàn bộ 50 bài tự kiểm và sửa bảng theo dõi đã chết từ trước; nhân hai quyết định đã ký thành mốc giờ cắt của bộ lập lịch; **sửa ba chỗ bàn giao nói sai với Module 3** (luật tiêu đề, mã xác nhận, token quay số không lưu được) |
| 08/09 | 31 | Ngân hàng giọng đọc: ghi lại 12 đoạn cố định, chốt bộ ghép câu, phát số tiền từ clip thay vì đọc máy; **chủ dự án dời khung giờ gọi sang 21:08** để đơn 21 giờ vẫn đủ hai lượt; một luật chống gọi trùng duy nhất; dọn nhánh lạc, đồng bộ hai kho |
| 09/09 | 49 | **Xoá hẳn giao diện quản trị**, giữ đúng phần Module 3 cần; phát hành hợp đồng `draft.24`; ký luật hết hạn token quay số; dựng hai hàng rào thu hồi việc; chặn lộ tên khách; sửa hai chỗ nuốt kết quả do bắt lỗi quá rộng; ba đường fail-closed nay nói rõ lý do; giảm bảng điều khiển từ 12 xuống 8 truy vấn; xoá 942 MB rác kho |
| 10–11/09 | 30 | **Phát phiếu chốt một lần cho Module 3** kèm hợp đồng `draft.25`; **chủ dự án nghiệm thu 35 hạng mục**, số đã nghiệm thu tăng 8 → 43; gộp bốn vòng lặp worker thành một; buộc lược đồ nhận việc vào hợp đồng sinh tự động; mẫu hồ sơ lưu trữ cho pháp lý; bỏ 440 lệnh thừa. **11/09 (6 lưu):** đưa hợp đồng lên `draft.27`, thống nhất từ vựng chế độ chạy, sửa chuỗi băm bị đọc nhầm thành số điện thoại |

## 3. KẾT NỐI VỚI MODULE 3 — **ĐANG CHỜ HỌ TRẢ LỜI**

**Tài liệu đã gửi xong, phía mình không còn nợ gì.** Ngày 10/09 đã phát trọn bộ: bản bàn giao API `IR-06`, **phiếu chốt một lần `IR-07`**, hợp đồng OpenAPI và sổ thay đổi giữa các bản.

| Nội dung | Trạng thái |
| --- | --- |
| Bàn giao API `IR-06` + hợp đồng 38 lệnh | ✅ Đã gửi 10/09, kèm cảnh báo 3 thay đổi phá vỡ tương thích |
| **Phiếu chốt `IR-07` — 21 mục phải trả lời**: bên gửi việc 7 · trả kết quả 8 · bề mặt quản trị 5 · môi trường 1 | 🟡 **Đã gửi 10/09 · Module 3 chưa phản hồi** |
| Môi trường thử để Module 3 gọi vào | ❌ Chưa dựng — nằm trong kế hoạch tuần tới, không chờ phiếu |

Phiếu được thiết kế để **chốt trong đúng một vòng**: mỗi mục đã có sẵn vị trí của Module 8 kèm lý do, Module 3 chỉ cần đánh `ĐỒNG Ý` hoặc ghi giá trị khác, **không điền coi như đồng ý**. **Module 3 đã hẹn phản hồi trong tuần 14–19/09.** Chừng nào chưa nhận phiếu đã điền thì 5 cổng `G-CONTRACT`, `G-SPEECH`, `G-DIAL`, `G-AUTH`, `G-POLICY` không đóng được và việc đấu nối hai chiều chưa bắt đầu được — **đây là đường găng duy nhất của module lúc này**.

## 4. KẾT QUẢ KIỂM CHỨNG

| Nhóm | Kết quả chạy lại tại `2613da5` + `W-0280` (ba bản vá ngày 12/09) |
| --- | --- |
| Kiểm thử tự động | **986 đạt / 0 hỏng**: đơn vị 670 · tích hợp với CSDL thật 284 · hợp đồng 24 · hỗn loạn 8. Tuần trước còn 2 hỏng — **đã sạch**. Bài thứ 986 là bài canh lỗi kho số liệu viết hôm nay. Đối soát kiểm thử ↔ yêu cầu: 613 mã bài kiểm, tuần trước 508 |
| Chạy trọn vòng gọi ở chế độ giả lập | **PASS 5/5** khi chạy trong khung giờ: xác nhận, huỷ, số sai, bấm sai phím, lỗi kỹ thuật đều ra đúng kết quả; lỗi kỹ thuật và số sai không tính là lượt gọi khách; 3/5 kết quả vào hàng đợi trả về. Lượt chạy lúc 07:54 đỏ toàn bộ **nhưng là đúng thiết kế** — bộ lập lịch từ chối quay số vì chưa tới 08:00, khung giờ chủ dự án chốt ngày 08/09. Ghi nhận: kịch bản diễn tập nay phụ thuộc giờ trong ngày, chưa ghim được đồng hồ |
| Nâng cấp CSDL · hợp đồng · xây · tài liệu · CI | Nâng cấp chạy song song hai phiên bản **PASS** — blocker nội bộ cuối cùng của tuần trước đã đóng. Đóng băng hợp đồng 15/15; kiểm tra hợp lệ, đối chiếu trôi, bắt bản hỏng: đạt cả ba; biên dịch 0 cảnh báo / 0 lỗi; tài liệu và cấu hình CI đạt. **Kubernetes PASS** trên cụm thật dựng tạm: chặn kết nối ra ngoài danh sách cho phép, worker sống qua 90 giây mất CSDL với 0 lần khởi động lại, xoay token gối đầu 6/6 |
| **Cổng kiểm bản đóng gói** (*container image* — thứ thật sự đem lên máy chủ chạy) | ✅ **5/5 ĐẠT sau ba bản vá hôm nay**: đóng gói không chạy quyền cao nhất · tự báo còn sống không cần CSDL · dựng cụm chạy thử, bên bán hàng giả lập không ra được Internet · quét lỗ hổng sạch và vẫn biết báo đỏ với bản cố tình hỏng · kê khai thành phần 31 và 97 thư viện. Sửa xong **lộ tiếp hai việc bị che khuất**, một đã vá, một cần chú quyết — mục 4.1 |
| **Việc tổng hợp số liệu** | ✅ **ĐÃ SỬA trong ngày** — lỗi nằm trên đường chạy thật, không phải bộ giả lập: hàm chuẩn hoá quy mọi phím khác `0`/`1` thành chữ `INVALID` và không bấm gì thành `NO_INPUT`, hai chuỗi đó tràn cột `varchar(1)` của kho số liệu. Vì cả lô nạp trong một giao dịch nên **một khách bấm nhầm phím làm hỏng cả lô và mọi lô sau, vĩnh viễn** — số liệu đứng im mà mọi test vẫn xanh. Nay chỉ nhận đúng một phím thật, còn lại ghi rỗng; thông tin không mất vì đã nằm ở cột loại kết quả. Có bài kiểm canh lỗi, đã kiểm chứng đỏ khi gỡ vá |
| Quét bảo mật | Lần quét 07/09 sạch, có ghi nhận 55 cảnh báo giả đã rà; máy này chưa cài công cụ nên **không chạy lại được tại chỗ** |
| Trạng thái kiểm soát chính thức | **NẤC 0 / NO-GO**. 43/255 hạng mục đã nghiệm thu, tuần trước 8 · 18 chặn bởi bên ngoài · **11 cổng ngoài còn mở** · quyết định còn mở giảm 4 → **2**. ⚠️ Bảng này **đếm thiếu 13 hạng mục**: `W-0267`–`W-0279` đã commit và có gói bằng chứng nhưng không có dòng nào trong sổ tiến độ. Cờ gọi khách thật vẫn **NO** ở cả bốn môi trường; **chưa một cuộc gọi nào tới số khách hàng thật** |

### 4.1 Sửa một lỗi, lộ ra hai — cả hai đều đáng hơn lỗi ban đầu

1. **Nửa cổng này đã chết 15 ngày.** Cổng gãy ở dòng in kết quả nên nửa sau chưa từng chạy; vá xong mới lộ ra. Nó gọi các lệnh quản trị bằng cơ chế xác thực theo header mà `W-0128` đã xoá từ 28/08; hệ thống nay phân quyền theo hạng token. Máy chủ không từ chối — nó **bỏ qua** header cũ, nên mọi lệnh trả về 401. **Đã sửa và chạy lại đạt**: script dùng token đúng hạng kèm người thực hiện và lý do, ba token quản trị được bổ sung vào cấu hình dựng cụm. Đã cho script dùng token đúng hạng kèm người thực hiện và lý do, và bổ sung ba token quản trị vào cấu hình dựng cụm. **Đây là bằng chứng cụ thể cho mục C1–C2**: không có máy chủ CI chạy tự động thì một cổng kiểm mục nát hai tuần mà không ai hay.
2. **Một khác biệt nhãn kết quả, cần chủ dự án quyết.** Bài diễn tập tạm dừng hàng đợi cho cửa sổ xác nhận hết hạn mà không gọi. Phần an toàn **đúng**: lệnh tạm dừng giữ được, không cuộc gọi nào xảy ra. Nhưng hệ thống gắn nhãn `IVR_CONFIRMATION_WINDOW_EXPIRED`, còn bài diễn tập đợi `IVR_CAPACITY_EXCEPTION`. Đặc tả ghi "capacity không xử lý kịp" thì phải là nhãn sau. Hai nhãn dẫn tới **hai hành động khác nhau** mà Module 3 phải làm với đơn, nên không tự sửa: cần chốt "tạm dừng có tính là cạn năng lực không".

## 5. KẾ HOẠCH HOÀN THÀNH

Năm làn. **Làn A là việc của mình, chạy được ngay từ 14/09.** Làn B **phụ thuộc Module 3 trả phiếu** — họ hẹn trả trong tuần 14–19/09; mục ghi `sau B1` chỉ khởi động được khi nhận phiếu, nên **mốc làn B tính từ ngày nhận, không tính từ 14/09**. Làn C–E phụ thuộc các bên ngoài.

| Làn | # | Gói việc | Xong |
| --- | ---: | --- | --- |
| A | A1 | ✅ **Xong 12/09**: ba lỗi đã sửa (dòng in kết quả cổng đóng gói · cột `dtmf_key` tràn · nửa cổng gọi bằng cơ chế xác thực đã xoá), kèm bài kiểm canh lỗi kho số liệu. **Còn lại**: chốt nhãn kết quả ở mục 4.1 rồi chạy lại cổng cho xanh hết | 14/09 |
| A | A2 | **Ma trận đủ 38 lệnh API**: đường thuận, sai định dạng, thiếu quyền, sai hạng quyền, không tìm thấy, xung đột, gọi lại cùng khoá, đổi nội dung cùng khoá, mã đối soát; khẳng định không lộ số điện thoại, địa chỉ, thanh toán | 15/09 |
| A | A3 | **Chạy liên tục ≥100 vòng** có tiêm lỗi: không nghe máy nhiều lượt, chết giữa chừng, hết hạn giữ việc, gửi trùng, hàng lỗi và phát lại; chứng minh không mất và không nhân đôi kết quả; **ghim đồng hồ** để diễn tập chạy được ngoài khung giờ gọi | 16/09 |
| A | A4 | Bù gói bằng chứng cho các dòng đã xanh test nhưng chưa có hồ sơ (200/254), ưu tiên 57 dòng thuộc kế hoạch gốc | 17/09 |
| A | A5 | Chạy lại **toàn bộ 50 bài tự kiểm + 39 đầu việc CI** trên đúng một mốc mã, ghim kết quả làm ứng viên phát hành | 18/09 |
| **B** | **B1** | **Nhận phiếu `IR-07` đã điền — 21 mục.** Module 3 hẹn trả trong tuần 14–19/09 | **chờ M3** |
| **B** | B2 | Dựng **môi trường thử để Module 3 gọi vào** (địa chỉ riêng, tài khoản dịch vụ có hạn mức, bộ ví dụ từng tình huống, cách dọn dữ liệu) và **đầu nhận kết quả giả lập** đúng hợp đồng. *Không chờ phiếu* | 17/09 |
| **B** | B3 | Áp các mục Module 3 chọn khác vào hợp đồng và mã nguồn; phát hành bản hợp đồng kế tiếp; sinh lại bộ mã gọi cho họ | sau B1 |
| **B** | B4 | **Đấu nối hai chiều**: Module 3 đẩy việc → IVR gọi giả lập → trả kết quả về Module 3 → Module 3 đổi trạng thái đơn | sau B1 |
| **B** | B5 | Đối soát đủ bộ mã xác nhận: chấp nhận, trùng, lỗi thời, bị chặn, sai định dạng, quá hạn mức, và thử lại sau khi mất kết nối | sau B1 |
| **B** | B6 | Ký đóng 5 cổng `G-CONTRACT`, `G-SPEECH`, `G-DIAL`, `G-AUTH`, `G-POLICY` | sau B4 |
| C | C1 | Nâng gói GitLab + người rà soát thứ hai, bật lượt chạy bắt buộc trước khi nhập mã (`G-GITLAB`); cấp máy chủ staging, CSDL, kho ảnh, quản lý bí mật, DNS/TLS (`G-PLATFORM`). **Hôm nay cho thấy vì sao đây là việc gấp, không phải việc cuối** | cần Hạ tầng |
| C | C2 | Triển khai staging bằng Helm; chạy khói; diễn tập cuộn dần, xanh–lam, nâng cấp hỏng và quay lui | sau C1 |
| C | C3 | Bảng theo dõi, cảnh báo, sổ tay xử lý sự cố, phân người trực, nối vết xuyên Module 3 → IVR → tổng đài; sao lưu, diễn tập khôi phục có mốc thời gian, đo RPO/RTO, khôi phục nhiều vùng, mã hoá ổ đĩa | sau C1 |
| C | C4 | Đo hiệu năng (tải thấp, nền, đỉnh, dồn cục; p95/p99; số kênh cần; bể kết nối CSDL) rồi **chạy liên tục 24–72 giờ** trên staging | sau C1 |
| C | C5 | Quét bảo mật thời gian chạy: ảnh, thư viện, bí mật; kiểm thử thâm nhập có xác thực; xử lý phát hiện | 18/09 |
| D | D1 | Duyệt kịch bản thoại hai chương trình; nghe duyệt giọng theo vùng và thiết bị; chốt tốc độ, cách đọc số, đoạn dự phòng | cần Sản phẩm |
| D | D2 | Pháp lý ký hồ sơ lưu trữ, xoá theo yêu cầu, tắt ghi âm — đóng `G-LEGAL` | cần Pháp lý |
| D | D3 | Chốt ranh giới từ chối nhận cuộc gọi và bàn giao phím 0, ký chính sách số lần gọi/khung giờ/ngày nghỉ (Sản phẩm); ký hồ sơ xác thực, xoay vòng bí mật, mô hình đe doạ token quay số, quy trình xử lý sự cố (An ninh) | cần Sản phẩm · An ninh |
| E | E1 | Chốt hợp đồng nhà mạng: số cố định + brandname; chặn mọi số ngoài danh sách cho phép; đặt trần chi phí | cần chủ dự án |
| E | E2 | **Thử một SIM thật**: gọi ra, bắt máy, máy bận, không nghe, bấm 0/1/phím sai/hết giờ, cúp máy, hiển thị số gọi, tiếng Việt có dấu; và các ca hỏng: mất sóng, khoá SIM, hết tiền, nhà mạng từ chối, quá hạn mức, sự kiện trùng, khởi động lại bộ nối | sau E1 |
| E | E3 | **Đo dung lượng thật** → hiệu chỉnh mô hình, đóng hai quyết định còn mở cuối cùng `OD-V1-09` và `OD-V1-10`; rồi quyết quy mô SIM cho vận hành **từ số đo**, không mặc định 32, kèm dự phòng, xoay SIM, theo dõi số dư, cam kết dịch vụ | sau E2 |
| E | E4 | Nghiệm thu bằng số nội bộ trên bộ đơn đã biết trước kết quả; diễn tập quay lui; rồi thí điểm nhóm khách nhỏ: giới hạn giờ, ít kênh, bảng theo dõi trực tiếp, người trực công tắc khẩn, ngưỡng tự dừng đặt trước | sau E3 |
| E | E5 | Biên bản go/no-go trên đúng ứng viên phát hành; chủ dự án ký; **bật cờ gọi khách thật**; trực tăng cường 3–7 ngày | sau E4 |

## 6. ĐIỀU KIỆN GIỮ ĐƯỢC LỊCH

| Cần có | Ai lo | Trước ngày | Nếu chậm |
| --- | --- | --- | --- |
| **Phiếu chốt `IR-07` điền xong, 21 mục** | **Module 3** | **19/09** | Làn B đứng từ B3; 5 cổng hợp đồng không đóng được; toàn bộ mốc sau lùi theo |
| Máy chủ staging, kho ảnh, bí mật, DNS/TLS **và** nâng gói GitLab + người rà soát thứ hai | Hạ tầng | 16/09 | Làn C lùi nguyên khối; không đo được hiệu năng; không có lượt chạy bắt buộc nên ứng viên phát hành không tái tạo được — và cổng kiểm tiếp tục mục nát âm thầm như mục 4.1 |
| Chữ ký kịch bản thoại, giọng đọc, pháp lý, an ninh | Sản phẩm · Pháp lý · An ninh | 18/09 | Không mở được cổng mua SIM, làn E không khởi động |
| Duyệt mua gói thử một SIM | Chủ dự án | sau khi làn A–D xanh | Chỉ chứng minh được trên giả lập, không bao giờ đo được dung lượng thật |

*Hai quyết định cuối `OD-V1-09` và `OD-V1-10` không thể ký bằng bàn giấy — chúng cần số đo từ SIM thật nên chỉ đóng được sau E3. Lập ngày 12/09/2026 từ cây mã tại `2613da5`, 130 lưu trong kỳ, bộ kiểm thử và các bài tự kiểm chạy lại tại đúng mốc này, bảng kiểm soát phát hành sinh lại từ sổ tiến độ. Mục nào không chạy lại được thì đã ghi rõ lý do, không suy từ kết quả cũ.*
