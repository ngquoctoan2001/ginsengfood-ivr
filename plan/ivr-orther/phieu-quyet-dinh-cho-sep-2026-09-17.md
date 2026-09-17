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

**Ba phiếu em đã tự đóng, không cần Sếp trả lời** (lý do ở mục 5 cuối phiếu).

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

## Mục 4 — Quyền dùng `12` đoạn ghi âm đang có *(cần một câu quyết định)*

### Việc đang là thế nào

Kịch bản cuộc gọi có `12` đoạn cố định. Chúng hiện là **audio do một phần mềm đọc máy tạo ra**
(VieNeu-TTS), Sếp đã duyệt ngày `28/08`.

Ngày `05/09`, quyết định `OD-V1-19` chốt **bỏ phần mềm đọc máy lúc gọi**, chuyển sang **thu giọng
người thật**. Nhưng ở đây có một chỗ dễ bỏ sót, và em muốn nói thẳng ra:

> **"Có được chạy phần mềm đó không" và "có được dùng audio mà nó đã tạo ra" là hai câu khác nhau.**
> Dù phần mềm không còn chạy lúc gọi, `12` đoạn hiện có **vẫn là sản phẩm của nó**.

### Hai lối ra — em đề xuất lối thứ hai

| | Cách làm | Ưu | Nhược |
| --- | --- | --- | --- |
| **A** | Sếp nhận rủi ro bằng văn bản, y như đã làm với `OD-V1-11` ngày `10/09` | Nhanh, `0` đồng, `0` ngày công | Vẫn là một rủi ro pháp lý treo, dù nhỏ |
| **B** ✅ | **Thu lại `12` đoạn bằng giọng người** | Câu hỏi **biến mất hoàn toàn**, không phải nhận rủi ro gì | Tốn một buổi ngồi thu |

**Vì sao em đề xuất `B`:** việc thu giọng người **đằng nào cũng nằm trong danh sách phải làm** — nó
là hệ quả của chính `OD-V1-19` mà ta đã ký. Làm nó sớm thì mục này tự đóng, không cần Sếp ký nhận
rủi ro nào cả. Đây là việc của em, không tốn tiền, chỉ tốn thời gian ngồi thu.

---

## Mục 5 — Ba phiếu em đã tự đóng, ghi lại để Sếp biết em đã bỏ cái gì

Ba phiếu dưới đây đều hỏi về **một hướng kỹ thuật đã bị bỏ**: tự cài phần mềm đọc máy lên máy chủ
công ty. Hướng đó chết từ ngày `05/09` khi `OD-V1-19` chốt dùng giọng thu sẵn.

| Phiếu cũ | Hỏi gì | Vì sao đóng |
| --- | --- | --- |
| `platform-w0122` | Xin máy chủ để cài phần mềm đọc máy | Bản cài đó **không lên máy chủ thật** — cấu hình sản xuất đặt sẵn `tắt` |
| `security-w0122-cve` | Xin ý kiến về `16` lỗ hổng bảo mật của bản cài đó | Cùng lý do — bản cài không chạy thì `16` lỗ hổng không chạm tới hệ thống thật |
| `legal-od-voice-07` phần `1`,`2`,`4` | Xin ý kiến bản quyền để **chạy** phần mềm đó | Cùng lý do — không chạy nữa |

Phần **duy nhất** của ba phiếu đó còn sống là câu hỏi về `12` đoạn audio — đã tách ra thành **Mục 4**
ở trên.

---

## Tóm tắt một bảng

| Mục | Loại | Em tự làm được không | Chặn cái gì |
| --- | --- | --- | --- |
| `1` Chỗ chạy thử | 💰 Tiền | ❌ | Toàn bộ việc đo hiệu năng và diễn tập sự cố |
| `2` Khoá mở số | 💰 Tiền | ❌ | Bước cuối của đường quay số thật |
| `3` Người duyệt thứ hai | 👤 Người | ❌ | Cổng kiểm an toàn + thao tác tắt khẩn cấp |
| `4` Quyền `12` đoạn audio | ⚖️ Rủi ro | ✅ **nếu chọn cách B** | Không chặn gì gấp |

**Không mục nào trong phiếu này liên quan tới nhà mạng.** Phần nhà mạng là một luồng riêng, đang chờ
báo giá, và hai luồng **không phụ thuộc nhau**.

---

## Nếu Sếp cần thêm số liệu để quyết

Mục `1` và `2` là quyết định chi tiền, nên nếu cần con số cụ thể (cấu hình tối thiểu, dung lượng,
lưu lượng ước tính) — Sếp nêu **cần số gì**, em đo và gửi trong ngày. Em đã có sẵn số đo thời gian
chạy của từng nhóm việc; cái em **không** có là báo giá, vì chọn nhà cung cấp là quyết định của Sếp
chứ không phải của em.
