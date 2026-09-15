# Phiếu yêu cầu — gửi Module 3 / chủ dữ liệu đơn hàng

**Chủ đề:** Chốt nghĩa của "tối đa hai lần trong 10 phút" trước khi nó thành policy chạy được
**Người gửi:** Team IVR — Order Confirmation
**Ngày lập:** `2026-09-15` · **Mốc mã:** `main@ff22227`
**Trạng thái:** `READY_TO_DISPATCH / NOT_SENT`
**Ưu tiên:** P1 cho câu 1–3 (chặn `T13` và việc chốt policy); P2 cho câu 4

> Phiếu này **không** xin gì về nhà mạng, trunk hay token. Ba câu đầu là ngữ nghĩa policy; câu 4 là
> một tham chiếu kỹ thuật. Trả lời được câu nào thì đóng câu đó.

---

## Vì sao hỏi bây giờ

Yêu cầu "gọi tối đa hai lần trong 10 phút" được mô tả bằng lời. Trong code hiện có **hai chương trình
với hai lịch khác nhau**, và **không cái nào** khớp trực tiếp với câu mô tả đó:

| Chương trình | Số customer attempt | Offset | Cửa sổ xác nhận |
| --- | --- | --- | --- |
| `GOLDEN_HOUR` | 2 | `0s`, `150s` | `300s` |
| `TWENTY_FOUR_SEVEN` | 2 | `0s`, `450s` | `900s` |

Nguồn: [`AttemptPolicyRegistries.cs`](../../src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs).

Cả hai offset đều **dưới** 10 phút, nên thoạt nhìn có vẻ đã thoả. Nhưng "10 phút" tính từ mốc nào và
ràng buộc cái gì thì ba cách đọc cho ra ba policy khác nhau, và chúng khác nhau ở chỗ **khách nghe máy
mấy lần**. IVR không tự chọn giúp được.

**Chúng tôi chưa đổi gì.** Lịch hiện hành vẫn đang chạy. Phiếu này là để chốt trước khi sửa, chứ không
phải báo cáo một thay đổi đã làm.

---

## Câu 1 — "10 phút" ràng buộc cái gì? (P1)

Chọn một:

| # | Cách đọc | Hệ quả |
| --- | --- | --- |
| A | Hai lượt gọi phải **bắt đầu** trong vòng 10 phút kể từ lượt đầu | Gần với lịch hiện tại; cửa sổ xác nhận là chuyện khác |
| B | **Toàn bộ** việc xác nhận đơn phải xong trong 10 phút | `TWENTY_FOUR_SEVEN` đang là `900s` = 15 phút → **phải đổi** |
| C | Khoảng cách giữa hai lượt **không quá** 10 phút | Cả hai chương trình hiện đã thoả; không phải đổi gì |

Và: mốc `0` tính từ **lúc M3 giao task**, hay từ **lúc IVR quay số lần đầu**? Hai mốc này lệch nhau bằng
độ trễ hàng đợi, và độ trễ đó không cố định.

## Câu 2 — Technical retry có tính vào "hai lần" không? (P1)

Đây là câu có hậu quả trực tiếp nhất tới khách hàng.

Hiện tại `technical exception` **không** được tính là customer attempt
([`DispositionMapper.cs`](../../src/Ivr.Domain/Confirmation/DispositionMapper.cs)), và trần retry kỹ thuật
mặc định là `1`. Budget quay số của token là `MaxAttempts + TechnicalRetryLimit`.

Nghĩa là **về mặt lý thuyết khách có thể nhận cuộc thứ ba**, nếu một lượt hỏng vì lý do kỹ thuật sau khi
đã đổ chuông.

| # | Quy tắc | Hệ quả |
| --- | --- | --- |
| A | "Hai lần" = hai **customer attempt**; technical retry không tính | Giữ nguyên code; khách có thể nghe chuông 3 lần |
| B | "Hai lần" = hai **cuộc gọi tới khách**, bất kể lý do | Phải chặn technical retry tạo cuộc thứ ba; đổi budget |

Nếu chọn B: **đổ chuông rồi mới hỏng** có tính không? Còn **gửi originate nhưng nhà mạng từ chối trước khi
đổ chuông** thì sao? Hai cái này khác nhau về việc khách có bị làm phiền hay không.

## Câu 3 — Giới hạn theo đơn hay theo số điện thoại? (P1)

Hiện lịch và budget gắn theo **job/task**, tức theo đơn.

Nếu một khách có **hai đơn** trong cùng khung giờ, họ nhận **bốn** cuộc. Không có gì trong hệ thống hiện
chặn điều đó.

| # | Quy tắc | Cần gì từ M3 |
| --- | --- | --- |
| A | Giới hạn theo đơn — bốn cuộc là chấp nhận được | Không cần gì |
| B | Giới hạn theo đích liên hệ — tối đa một cuộc đang hoạt động trên mỗi số | **Câu 4** |

## Câu 4 — Nếu chọn 3B: tham chiếu nào để khoá theo đích? (P2)

IVR **không được lưu số điện thoại thô** (`OD-V1-18`). Số E.164 chỉ sống trong bộ nhớ của biên telephony
đủ lâu để quay số; mọi nơi khác chỉ có tham chiếu opaque.

Để khoá "một cuộc đang hoạt động trên mỗi đích", IVR cần **một tham chiếu ổn định cho cùng một số thuê bao
qua nhiều đơn** — do M3 cấp hoặc cho phép IVR dẫn xuất.

Cần biết:

1. `phone_ref` hiện tại có **ổn định qua các đơn khác nhau của cùng một số** không, hay mỗi task một giá trị?
2. Nếu không ổn định: M3 có cấp được một `contact_ref` ổn định không?
3. Nếu không có: IVR **không** tự băm số để tạo khoá — làm vậy là dựng một định danh khách hàng mới ở phía
   IVR, đúng thứ `OD-V1-18` không muốn. Khi đó câu 3 phải chọn A.

---

## Cái gì mở ra khi có trả lời

| Câu | Mở khoá |
| --- | --- |
| 1, 2 | Chốt được policy; viết được `T13` (đếm số originate thật và số lần tới đích) |
| 2 | Quyết được budget resolve của dial token — **không đổi schema ledger**, chỉ đổi con số bên gọi |
| 3, 4 | Quyết được có cần khoá theo đích không, và khoá bằng gì |

Nếu policy phải đổi: phiên bản mới **giữ snapshot của job cũ**, và phải kiểm cả producer M3, intake,
scheduler, TTL token, normalization và callback. Không sửa hằng số riêng ở adapter.

**Ràng buộc kiến trúc không đổi:** M3 quyết đơn nào cần gọi; IVR thực thi và trả kết quả; M3 kiểm lại
trạng thái/phiên bản đơn trước khi cập nhật nghiệp vụ. `0` là yêu cầu huỷ đơn, **không** phải yêu cầu
ngừng mọi liên hệ.
