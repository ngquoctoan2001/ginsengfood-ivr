# R-00 — Hồ sơ hỏi mua cổng thoại di động 4G/VoLTE (bản gửi thẳng nhà cung cấp)

External work `W-0008` · quyết định `OD-V1-09`, `OD-V1-18` · gate `G-LAB-SIM` · trạng thái `OPEN` · tạo `2026-08-28`

> ⛔ **ĐỌC TRƯỚC — đây không phải hồ sơ mua "GSM gateway".**
>
> Việt Nam **tắt sóng 2G ngày 15/09/2026**. **3G chạy tới tháng 9/2028** — Cục Viễn thông đặt lộ trình dừng hẳn 3G năm 2028, không phải 2026. Thiết bị bán dưới tên "GSM gateway" (2G thuần) hết dùng được từ 15/09/2026; thiết bị 3G/WCDMA còn khoảng hai năm. Nguồn: [VNPT](https://vnpt.vn/gioi-thieu/tin-tuc/15-9-2026-he-thong-2g-se-ngung-hoat-dong-tai-viet-nam.html) (2G) · [VietnamNet](https://vietnamnet.vn/thang-9-2028-se-khai-tu-cong-nghe-3g-tai-viet-nam-2303408.html) và [Nhân Dân](https://nhandan.vn/viet-nam-se-tat-song-3g-vao-nam-2028-post819850.html) (3G).
>
> Chúng tôi chỉ xét thiết bị **4G/LTE có VoLTE**. Thiết bị 4G nhưng thoại phải rơi về CSFB (2G/3G) cũng bị loại vì lý do y hệt.

> **Cách dùng.** Gửi nguyên file này cho nhà cung cấp. Không cần đính kèm tài liệu nội bộ nào khác, không cần cắt từ tài liệu thiết kế. Mọi bảng đều có cột cuối để nhà cung cấp điền.
>
> **Mục 8 là ghi chú nội bộ** — xoá trước khi gửi nếu không muốn lộ ID công việc nội bộ.
>
> File này là bản **mở cuộc nói chuyện đầu tiên**. Bản kỹ thuật đầy đủ dùng cho vòng đàm phán sau là [R-01](R-01-vendor-requirements.md); nó không mâu thuẫn với file này, chỉ chi tiết hơn.

---

## 1. Chúng tôi cần mua gì

Một **cổng thoại di động 4G/VoLTE** (lắp SIM, nối vào tổng đài qua SIP trunk) để hệ thống tự động gọi điện xác nhận đơn hàng với khách; khách bấm phím `1` (xác nhận) hoặc `0` (huỷ).

| Hạng mục | Nội dung |
| --- | --- |
| Giai đoạn 1 — **lab** | **1 SIM**, chỉ quay số trong danh sách nội bộ được duyệt. Đây là hạng mục cần báo giá ngay. |
| Giai đoạn 2 — production | Nhiều kênh. Số kênh **chưa chốt**, sẽ quyết định sau khi đo thật ở lab. Xin báo giá theo bậc kênh để tham khảo, không phải đơn hàng. |
| Thoại | Phát file âm thanh tiếng Việt, thu một phím DTMF của khách. |
| Ghi âm | **Mặc định TẮT**, và phải **đọc ngược lại được qua API** để chứng minh nó đang tắt. |

Hai điều kiện thương mại đi kèm: thời gian giao hàng dự kiến, và có hỗ trợ kỹ thuật trong buổi lab đầu tiên hay không.

## 2. Đường tích hợp mong muốn

```
Hệ thống của chúng tôi → Asterisk (đã có sẵn) → SIP trunk → GSM Gateway → SIM → nhà mạng
```

Chỉ đoạn **`SIP trunk → GSM Gateway`** là phần mới. Phần còn lại đã chạy.

Vì vậy thiết bị **nói SIP chuẩn** được ưu tiên hơn hẳn thiết bị chỉ có SDK riêng của hãng: SIP thì nối trunk là xong, SDK riêng thì chúng tôi phải viết lại phần kết nối.

## 3. Bảy điều kiện loại trừ

Đây là tiêu chí tối thiểu. Thiết bị không đáp ứng một dòng nào trong đây thì xin nêu rõ ngay từ hồ sơ, đừng để phát hiện ở buổi lab.

| **#** | **Yêu cầu** | **Vì sao đây là điều kiện loại trừ** | **Thiết bị của quý công ty** |
| --- | --- | --- | --- |
| **0** | **Thoại chạy trên VoLTE (4G).** Nêu rõ model có module VoLTE hay chỉ có LTE data + CSFB. | 2G tắt **15/09/2026**, 3G tắt **tháng 9/2028**. Thiết bị 2G thuần chết từ 15/09/2026. Thiết bị 4G dùng CSFB **không** chết ngay: sau khi 2G tắt nó còn rơi về 3G tới 2028 — nhưng đó là mua một thiết bị có hạn dùng đã đếm ngược, cho một hệ thống dự kiến chạy quá mốc đó. Vì vậy VoLTE là bắt buộc, lý do là **horizon**, không phải "chết sau một tháng". **Đây là điều kiện đầu tiên; không đạt thì không cần trả lời các mục còn lại.** | `<điền>` |
| 1 | Có API kiểm tra sức khoẻ **từng kênh**, và **đọc ngược lại được cờ trạng thái ghi âm**. | Chính sách của chúng tôi khoá ghi âm ở trạng thái TẮT. Không đọc ngược được thì không chứng minh được nó đang tắt. | `<điền>` |
| 2 | Bảng **mã kết thúc cuộc gọi (call disposition)** phân biệt được 11 giá trị ở **mục 4** bên dưới. | Ánh xạ nhầm "khách bấm nút từ chối" thành "khách huỷ đơn" là huỷ đơn của một khách không hề yêu cầu huỷ. | `<điền>` |
| 3 | DTMF theo **RFC 2833/4733**; nêu rõ có bắt được phím **trong lúc đang phát thoại** (barge-in) không. | Không có barge-in thì cuộc gọi dài hơn, tỉ lệ khách cúp máy giữa chừng tăng. | `<điền>` |
| 4 | Một SIM tại một thời điểm chỉ mang **một** cuộc gọi — hoặc nêu rõ nếu khác. | Toàn bộ mô hình phân bổ kênh và tính năng lực của chúng tôi dựa trên giả định này. | `<điền>` |
| 5 | **Tắt được từng kênh qua API**; nêu rõ hành vi khi kênh đang có cuộc gọi. | Cần cho nút dừng khẩn cấp, và để thay SIM lỗi mà không phải dừng cả hệ thống. | `<điền>` |
| 6 | Có **CDR** kèm mã tham chiếu cuộc gọi, nối được sang mã cuộc gọi của chúng tôi. | Không nối được thì mọi tranh chấp hoá đơn đều không giải quyết được. | `<điền>` |
| 7 | **Nói SIP chuẩn**, ưu tiên hơn SDK độc quyền. | Xem mục 2. | `<điền>` |

## 4. Bảng mã kết thúc cuộc gọi — thứ chúng tôi cần nhất

Đây là **11 trạng thái mà phần mềm của chúng tôi đang chờ nhận**. Xin điền cột 4 bằng mã thô mà thiết bị thực sự trả về. Nếu thiết bị **gộp** nhiều dòng vào một mã, xin ghi rõ gộp thành mã gì — gộp không phải lúc nào cũng là vấn đề, nhưng gộp sai chỗ thì là.

| **#** | **Trạng thái chúng tôi cần phân biệt** | **Nghĩa** | **Thiết bị trả mã gì?** | **Có tính là một lần đã gọi khách** |
| --- | --- | --- | --- | --- |
| 1 | Answered | Khách nhấc máy. | `<điền>` | Có |
| 2 | RingTimeout | Đổ chuông hết giờ, không ai nghe. | `<điền>` | Có |
| 3 | Busy | Máy bận. | `<điền>` | Có |
| 4 | Rejected | **Khách chủ động bấm nút từ chối cuộc gọi.** | `<điền>` | Có |
| 5 | Unreachable | Thuê bao không liên lạc được. | `<điền>` | Không |
| 6 | InvalidDestination | Số không tồn tại / sai định dạng. | `<điền>` | Không |
| 7 | Dropped | Cuộc gọi rớt giữa chừng. | `<điền>` | Không |
| 8 | NetworkError | Lỗi mạng nhà mạng. | `<điền>` | Không |
| 9 | SimError | Lỗi SIM / lỗi kênh. | `<điền>` | Không |
| 10 | AudioError | Lỗi phát thoại. | `<điền>` | Không |
| 11 | DtmfError | Lỗi bắt phím. | `<điền>` | Không |

**Cột cuối là cột đắt tiền nhất.** Số lần được gọi mỗi khách **chưa được ký**: `T-09` còn trạng thái `OPEN`, và ngay các bản đề xuất trong đó cũng chưa thống nhất với nhau — Giờ Vàng 2 lần, còn 24/7 là **3** lần. Vendor **không cần** đáp ứng con số nào ở đây. Điều chúng tôi cần là thiết bị **trả về disposition đủ phân biệt** để bên chúng tôi tự đếm: phân biệt được "khách đã có cơ hội nghe máy" với "lỗi thiết bị/mạng" là yêu cầu thật, còn con số lần gọi là chuyện nội bộ chưa chốt. Ví dụ dưới dùng hai lần chỉ để minh hoạ cách phân loại. Dòng 1–4 là những ca khách **đã có cơ hội nghe máy thật**, nên tiêu một lượt. Dòng 5–11 là lỗi của **thiết bị hoặc mạng**, không được tính vào lượt của khách — nếu SIM hỏng hai lần mà bị ghi thành "khách không nghe máy", đơn của một khách chưa từng nghe chuông lần nào sẽ bị đóng.

**Nếu thiết bị trả mã nguyên nhân Q.850**, xin nói rõ — như vậy là đủ, không cần bảng riêng. Chúng tôi đã ánh xạ sẵn các cause sau: `16` → Answered, `17` → Busy, `18`/`19` → RingTimeout, `21` → Rejected, `1`/`3`/`20` → Unreachable, `28` → InvalidDestination, `34`/`38`/`41`/`42`/`44` → lỗi mạng.

## 5. Hai dòng phải hỏi kỹ nhất

**Dòng 4 — `Rejected` (khách bấm nút đỏ từ chối cuộc gọi).**

Khách bấm nút từ chối **không có nghĩa là "tôi muốn huỷ đơn"** — nó chỉ có nghĩa "giờ tôi không tiện nghe".

| Thiết bị xử lý thế nào | Kết luận |
| --- | --- |
| Gộp `Rejected` chung với `RingTimeout` (không nghe máy) | **Chấp nhận được.** Cả hai đều ra cùng một kết quả ở phía chúng tôi. |
| Gộp `Rejected` chung với `Answered` | **Phải biết trước khi ký.** Chúng tôi sẽ phát thoại cho một cuộc gọi không có ai nghe. |
| Trả một mã ngụ ý "khách từ chối đơn hàng" | **Phải biết trước khi ký.** Ánh xạ nhầm dòng này là huỷ đơn của khách không hề yêu cầu. |

**Hộp thư thoại — cố ý không có dòng riêng ở bảng trên.**

Phần lớn thiết bị báo hộp thư thoại là `Answered`. Nếu vậy, hệ thống sẽ đọc kịch bản cho hộp thư rồi ghi nhận "khách đã nghe" — sai hoàn toàn.

Xin trả lời rõ: thiết bị **có cờ phân biệt hộp thư thoại / AMD** không, và nếu có thì đọc cờ đó ở đâu?

## 6. Chín câu hỏi xin trả lời bằng văn bản

0. **Model này gọi thoại bằng VoLTE hay bằng CSFB?** Sau 15/09/2026 (tắt 2G) model còn gọi được không, và sau **tháng 9/2028** (tắt 3G) thì sao? Xin trả lời bằng tên model cụ thể, không trả lời bằng tên dòng sản phẩm.
1. Thiết bị trả về những **mã kết thúc cuộc gọi** nào? Xin gửi bảng đầy đủ.
2. Trong bảng đó, có phân biệt được **khách chủ động bấm từ chối** với **đổ chuông không ai nghe** không? Nếu gộp thì gộp thành mã gì?
3. Có cờ nào cho biết cuộc gọi rơi vào **hộp thư thoại** không, hay nó cũng báo là "answered"?
4. Có phân biệt **lỗi kỹ thuật** (SIM lỗi, rớt mạng, lỗi âm thanh) với **khách không nghe máy** không?
5. DTMF theo **RFC 2833/4733**? Có bắt được phím **trong lúc đang phát thoại** (barge-in) không? Thời gian chờ phím có cấu hình được không, dải bao nhiêu?
6. **Một SIM tại một thời điểm mang được mấy cuộc gọi?**
7. Có API **tắt/bật từng kênh** không? Có API đọc được **trạng thái ghi âm** để xác nhận nó đang TẮT không?
8. Có **CDR** kèm mã tham chiếu cuộc gọi để đối soát không? Lấy CDR bằng cách nào (API, file, cổng quản trị)?
9. Thiết bị nói **SIP chuẩn** hay chỉ có **SDK riêng của hãng**?

Câu **2, 3, 4** là ba câu quyết định — sai nhóm là huỷ nhầm đơn của khách. Câu **9** quyết định chi phí tích hợp.

## 7. Hồ sơ xin nộp, và điều gì bị loại ngay

**Xin nộp:** bảng mã trạng thái đầy đủ của thiết bị · tài liệu API (health, tắt/bật kênh, CDR) · xác nhận SIP hay SDK · báo giá 1 SIM cho lab kèm bậc giá nhiều kênh để tham khảo · thời gian giao hàng.

**Bị loại ngay:**

- **Chỉ hỗ trợ 2G/3G**, hoặc 4G nhưng thoại phải rơi về CSFB.
- Ghi âm bật mặc định và **không tắt được ở mức API**.
- Không có API đọc được trạng thái ghi âm để xác nhận nó đang tắt.
- Không nêu được bảng mã trạng thái, hoặc chỉ trả lời "có, đầy đủ" mà không kèm bảng.

Trả lời "có, qua tuỳ chỉnh" cho bất kỳ mục nào — xin nêu rõ tuỳ chỉnh là gì và **ai chịu chi phí**.

---

## 8. Ghi chú nội bộ (xoá trước khi gửi)

**Nguồn.** Mục 3 gộp từ `§13.2`, mục 4–5 gộp từ `§13.3` của [`docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md`](../../MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md). Mục 6 là bản câu-hỏi-hoá của cả hai.

**Đối chiếu code ngày 28/08/2026** (`5ec7342`):

| Khẳng định trong file | Kiểm chứng |
| --- | --- |
| 11 disposition, đúng thứ tự | `SimProviderDisposition` — [`ProviderPorts.cs:154`](../../../src/Ivr.Domain/Ports/ProviderPorts.cs) |
| Cột "tính lượt khách": dòng 1–4 Có, 5–11 Không | Cờ `IsCounted` trong [`DispositionMapper.cs:73`](../../../src/Ivr.Domain/Confirmation/DispositionMapper.cs) — `MapAnswered`/`MapNoAnswer` trả `true`, `InvalidPhone`/`Technical` trả `false` |
| `Rejected` → không nghe máy + cờ cần review | `MapNoAnswer("REJECTED_REVIEW_REQUIRED", humanReviewRequired: true)` — [`DispositionMapper.cs:76`](../../../src/Ivr.Domain/Confirmation/DispositionMapper.cs) |
| Danh sách Q.850 ở mục 4 | `MapHangup` — [`AsteriskAriSimGateway.cs:361`](../../../src/Ivr.Infrastructure/Telephony/AsteriskAriSimGateway.cs) |

**Sửa 28/08/2026 — mốc tắt sóng.** Bản đầu của file này (và `§13.2` gốc) viết "GSM Gateway", tức thiết bị 2G. **Đính chính lần hai, cùng ngày (`W-0135`):** lượt sửa đầu ghi 3G tắt `30/09/2026` và tự khai đã tra nguồn chính thức. Con số đó **sai hai năm** — 3G chạy tới **tháng 9/2028**. Mốc 2G `15/09/2026` tra lại thấy đúng, giữ nguyên. Nguồn: [VNPT](https://vnpt.vn/gioi-thieu/tin-tuc/15-9-2026-he-thong-2g-se-ngung-hoat-dong-tai-viet-nam.html) (2G) · [VietnamNet](https://vietnamnet.vn/thang-9-2028-se-khai-tu-cong-nghe-3g-tai-viet-nam-2303408.html) và [Nhân Dân](https://nhandan.vn/viet-nam-se-tat-song-3g-vao-nam-2028-post819850.html) (3G). Toàn bộ hồ sơ đổi sang **4G/VoLTE**, thêm điều kiện loại trừ #0 và câu hỏi #0. `§13.2` của tài liệu Module 8 **vẫn còn dùng từ "GSM" và chưa có ràng buộc VoLTE** — cần sửa nguồn, nếu không lần sau lại gộp ra bản sai.

**Không nêu trong phần gửi vendor vì chưa chốt nội bộ:** số kênh cho pilot (`M8-OD-A`, đang bị chặn bởi `M8-OD-C`); thời gian chờ DTMF (tài liệu ghi 15s, cấu hình lab đang 60s — nên chỉ hỏi *dải cấu hình*, không tuyên bố con số); attempt policy còn ở version `mock-lab-v1` chưa ký.

**Hai ca ánh xạ sai đã mở thành điểm chặn:** `M8-P0-013` (Rejected bị hiểu thành huỷ đơn), `M8-P0-014` (hộp thư thoại bị tính là khách đã nghe).

**Ba ràng buộc bất biến** ở [README §4](README.md) vẫn áp dụng nhưng cố ý **không** đưa vào phần gửi vendor: ranh giới `dial_token` là quyết định kiến trúc nội bộ (`OD-V1-18`), và `REAL_CUSTOMER_CALL_ALLOWED=NO` là chính sách của chúng tôi, không phải yêu cầu thiết bị.

**File này không đóng gate.** `W-0008`, `G-LAB-SIM`, `G-ESIM32` chỉ đóng bằng artifact thật, không bằng RFQ hay báo giá.
