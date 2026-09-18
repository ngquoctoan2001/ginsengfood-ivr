# Phiếu xin quyết định — gửi Sếp

**Chủ đề:** Bốn việc Module 8 không tự quyết được — ba việc cần tiền, một việc cần người
**Người gửi:** Toàn — Module 8 (IVR gọi xác nhận đơn hàng)
**Ngày lập:** `2026-09-17` · **Mốc mã:** `main@fbe6edb`
**Trạng thái:** `READY_TO_DISPATCH / NOT_SENT`

> Phiếu này **không có thuật ngữ kỹ thuật nào cần tra**. Mỗi mục có: cần gì · để làm gì · nếu không
> có thì sao. Trả lời được mục nào thì đóng mục đó, không cần chờ đủ bốn.
>
> **Phần code đã xong.** Tính tới hôm nay, mọi việc lập trình không phụ thuộc bên ngoài đều đã làm
> xong. Bốn mục dưới đây là bốn chỗ duy nhất mà thêm ngày công của em **không** làm nó nhúc nhích.

---

## Vì sao phiếu này gộp bốn thứ vào một

Trước đây em soạn `6` phiếu riêng, gửi cho **Platform**, **Security** và **Legal**. Rà lại thì
**`5/6` phiếu không có người nhận** — công ty mình không có ba phòng ban đó. Em đã đi nhầm đường
khá lâu: viết phiếu đúng chuẩn, nhưng gửi cho những phòng ban không tồn tại.

Ta đã có tiền lệ xử lý đúng: ngày `10/09`, với quyết định `OD-V1-11` (ghi âm cuộc gọi, thời hạn lưu),
Sếp đã chốt *"không có đội Legal, owner tự nhận rủi ro, ghi vào văn bản"*. Phiếu này làm đúng như thế
cho phần còn lại — đưa thẳng về người có thẩm quyền thật, là Sếp.

**Ba phiếu về VieNeu** (máy chạy, lỗ hổng, bản quyền) không gửi riêng nữa — mục 5 cuối phiếu ghi
chúng nay nằm ở đâu.

---

## Mục 1 — Chỗ chạy thử trước khi chạy thật *(cần tiền)*

### Cần gì

| # | Thứ cần mua/cấp | Để làm gì |
| --- | --- | --- |
| 1 | Một chỗ chạy máy chủ thử (cụm Kubernetes, `2` khu vực: `dev` và `staging`) | Nơi cài phần mềm lên chạy thử như thật |
| 2 | Một cơ sở dữ liệu **riêng** cho bản thử | Sẽ bị xoá và nạp lại nhiều lần — tuyệt đối không dùng chung với dữ liệu thật |
| 3 | Một nơi giữ mật khẩu/khoá | Hiện các giá trị này đang là **giá trị giả ghi thẳng trong file**, chỉ hợp cho máy cá nhân |
| 4 | Một tên miền + chứng thư bảo mật cho bản thử | Để Module 3 gọi vào được và chứng minh được đường truyền có mã hoá |
| 5 | Một bảng theo dõi + nơi nhận cảnh báo | Đo hàng đợi, độ trễ, tỉ lệ lỗi. Chưa có nơi nhận thì không đặt được ngưỡng báo động |

### Phần em đã tự làm xong, **không** cần mua

Cấu hình triển khai, chặn kết nối mặc định, chạy không quyền cao nhất, giới hạn tài nguyên, kiểm tra
sống, dọn dữ liệu định kỳ — **đã viết và đã kiểm trên một cụm tạm dựng trên máy em, đạt hết**. Thiếu
đúng một thứ: **chỗ để chạy thật**.

### Nếu không có thì sao

Không đo được hiệu năng, không diễn tập được khôi phục sự cố, không chạy liên tục `24–72` giờ được.
Nghĩa là tới ngày gọi khách thật, **lần chạy thật đầu tiên chính là lần chạy thử đầu tiên**.

---

## Mục 2 — Khoá để mở số điện thoại khách *(cần tiền, nhiều khả năng rất ít)*

### Việc đang là thế nào

Số điện thoại khách **không** được lưu thẳng trong hệ thống của em — đúng như thiết kế bảo mật.
Nó được cất dưới dạng mã hoá, và chỉ được mở ra **đúng một lần, ngay lúc quay số**, rồi biến mất
khỏi bộ nhớ. Không ghi vào cơ sở dữ liệu, không ghi vào nhật ký, không gửi đi đâu.

Để mở được, cần một **khoá**. Hiện hệ thống có `4` cách giữ khoá và **không cách nào dùng được cho
chạy thật** — chúng đều là bản giả cho phòng thí nghiệm.

### Cần Sếp quyết `3` điều

| # | Câu hỏi | Vì sao phải hỏi |
| --- | --- | --- |
| 1 | Dùng dịch vụ giữ khoá nào | Em **không tự chọn** — chọn sai là tự quyết một thứ tốn tiền định kỳ |
| 2 | Đổi khoá định kỳ kiểu gì | Số đã mã hoá bằng khoá cũ vẫn phải mở được sau khi đổi |
| 3 | Mất khoá thì sao | Mất khoá = **không đơn nào gọi được nữa**. Cần biết đây là chấp nhận được, hay phải có bản dự phòng cất riêng |

### Nếu không có thì sao

Đường quay số thật **đã dựng xong** (`16/09`) nhưng sẽ dừng ngay bước cuối: có số mã hoá, không mở
được. Phần mềm đã được viết để **từ chối chạy** nếu thiếu khoá — cố ý, để không bao giờ có chuyện
quay một số mà hệ thống không bảo vệ nổi.

---

## Mục 3 — Một người thứ hai để duyệt *(cần người, không cần tiền)*

### Cần gì

Một tài khoản thứ hai có quyền duyệt trên kho mã nguồn — **một người khác em**.

### Vì sao không tự lo được

Hai hạng mục an toàn của hệ thống đòi **bốn mắt**: người đề xuất và người duyệt phải là **hai người
khác nhau**. Đây là ràng buộc em tự đặt ra và tự ép trong code, vì nó bảo vệ đúng thao tác nguy hiểm
nhất — **tắt công tắc dừng khẩn cấp** và **mở rộng danh sách số được phép gọi**.

Em **không tự gỡ ràng buộc này**, dù gỡ được. Bật công tắc dừng thì một người là đủ (chiều **giảm**
rủi ro); tắt nó thì phải hai người (chiều **tăng** rủi ro). Một mình em thì vế thứ hai không thực
hiện được — **chữ ký không tạo ra người**.

### Nếu không có thì sao

Mọi thứ khác xanh, riêng cổng kiểm này không đóng được. Và tới lúc cần tắt khẩn cấp thật, thao tác
đó sẽ bị chính hệ thống chặn lại.

---

## Mục 4 — Quyền dùng giọng đọc VieNeu *(cần một chữ ký)*

### Việc đang là thế nào

Giọng đọc của hệ thống là **VieNeu-TTS**, một phần mềm đọc tiếng Việt **cài trên máy chủ công ty**.
Không thuê dịch vụ đám mây nào, nên nội dung đơn không rời hệ thống. Kịch bản có `12` đoạn cố định
do VieNeu đọc sẵn bằng ba giọng Sếp đã duyệt ngày `28/08`; món hàng, tổng tiền và nơi giao do VieNeu
đọc ngay lúc gọi.

Trang phát hành của model ghi giấy phép **Apache-2.0** (cho phép dùng thương mại), nhưng đúng bản mà
em đã ghim lại **không kèm file giấy phép**. Công ty không có bộ phận pháp chế, nên cách xử lý giống
`OD-V1-11` ngày `10/09`: **người có thẩm quyền ký nhận rủi ro bằng văn bản**.

### Cần Sếp làm gì

Ký rủi ro `4` trong chữ ký `S2` của [vướng mắc `17/09`](vuong-mac-va-quyet-dinh-2026-09-17.md):
*quyền dùng thương mại model VieNeu và `12` đoạn đã render.* Chưa ký thì VieNeu vẫn chạy được ở lab
nhưng không bật ở production.

---

## Mục 5 — Ba phiếu VieNeu cũ: nay nằm ở đâu

VieNeu là giọng đọc duy nhất của hệ thống, nên ba phiếu hỏi về nó vẫn còn nguyên đối tượng. Em không
gửi riêng ba phiếu cho những phòng ban không tồn tại; câu hỏi của chúng đi về đúng chỗ Sếp đang quyết:

| Phiếu cũ | Hỏi gì | Nay ở đâu |
| --- | --- | --- |
| `platform-w0122` | Máy chủ đủ sức chạy VieNeu + kho bản cài nội bộ | `S5` — chỗ chạy thử |
| `security-w0122-cve` | `16` lỗ hổng bảo mật trong bản cài VieNeu | `S2` rủi ro `3` — em thử đổi bản cài nền trước; làm được thì rủi ro này biến mất |
| `legal-od-voice-07` | Bản quyền model VieNeu và `12` đoạn đã render | `S2` rủi ro `4` — Mục 4 ở trên |

---

## Tóm tắt một bảng

| Mục | Loại | Em tự làm được không | Chặn cái gì |
| --- | --- | --- | --- |
| `1` Chỗ chạy thử | 💰 Tiền | ❌ | Toàn bộ việc đo hiệu năng và diễn tập sự cố |
| `2` Khoá mở số | 💰 Tiền | ❌ | Bước cuối của đường quay số thật |
| `3` Người duyệt thứ hai | 👤 Người | ❌ | Cổng kiểm an toàn + thao tác tắt khẩn cấp |
| `4` Quyền dùng VieNeu | ⚖️ Rủi ro | ❌ Sếp ký | Bật VieNeu ở production |

**Không mục nào trong phiếu này liên quan tới nhà mạng.** Phần nhà mạng là một luồng riêng, đang chờ
báo giá, và hai luồng **không phụ thuộc nhau**.

---

## Nếu Sếp cần thêm số liệu để quyết

Mục `1` và `2` là quyết định chi tiền, nên nếu cần con số cụ thể (cấu hình tối thiểu, dung lượng,
lưu lượng ước tính) — Sếp nêu **cần số gì**, em đo và gửi trong ngày. Em đã có sẵn số đo thời gian
chạy của từng nhóm việc; cái em **không** có là báo giá, vì chọn nhà cung cấp là quyết định của Sếp
chứ không phải của em.
