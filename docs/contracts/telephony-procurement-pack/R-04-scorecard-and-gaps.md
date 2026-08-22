# R-04 — Scorecard nhà cung cấp, gap register và điều khoản hợp đồng

External work `W-0008` · gate `G-LAB-SIM`, `G-ESIM32` · trạng thái `OPEN`

Owner: **Procurement** (quyết định mua), **Infra** (chấm điểm kỹ thuật).

Due: dùng **trước khi chọn nhà cung cấp**. Ngày cam kết của owner: `<owner điền>`.

> Đây là **công cụ để Procurement quyết định**, không phải một quyết định. IVR không chọn nhà cung cấp.

## 1. Hạng mục loại trừ — chấm điểm sau, loại trước

Bốn điều kiện dưới đây không có trọng số. Không đạt là loại, bất kể điểm các mục khác.

| # | Điều kiện | Nguồn ràng buộc | Kiểm ở đâu |
| --- | --- | --- | --- |
| 1 | Recording tắt được ở mức API **và** đọc ngược lại được qua health | `DT-05` | [R-01](R-01-vendor-requirements.md) §11 |
| 2 | Ánh xạ được đủ 11 disposition, đặc biệt phân biệt `Rejected` với `Answered` | `DT-02` | [R-01](R-01-vendor-requirements.md) §7 |
| 3 | Có API kiểm tra sức khoẻ từng kênh và tắt được từng kênh | `DT-04` | [R-01](R-01-vendor-requirements.md) §8 |
| 4 | Không buộc IVR phải giữ mapping `dial_token → số thật` | `D-05` | [R-01](R-01-vendor-requirements.md) §4 |

Điều kiện 4 có thể thoả bằng một thành phần resolver tin cậy chạy ngoài IVR — nhưng **phải có văn bản của nhà cung cấp**, và Security phải chấp nhận, vì đó chính là `OD-V1-18`.

## 2. Scorecard có trọng số

Thang điểm mỗi mục: `0` không đáp ứng · `1` đáp ứng một phần, cần tuỳ chỉnh · `2` đáp ứng · `3` đáp ứng và có bằng chứng đo được.

| # | Tiêu chí | Trọng số | Vì sao trọng số này | A | B | C |
| --- | --- | ---: | --- | --- | --- | --- |
| 1 | Độ tin cậy DTMF trên mạng di động Việt Nam | 5 | Bắt sai một phím là ghi sai ý khách. Không có cách sửa sau. | | | |
| 2 | Ánh xạ disposition đầy đủ và ổn định | 5 | Ánh xạ sai `Rejected` → huỷ đơn khách không yêu cầu. | | | |
| 3 | Chất lượng thoại và hỗ trợ codec | 4 | Khách không nghe rõ thì cả luồng vô nghĩa. | | | |
| 4 | Health API + tắt/bật kênh | 4 | Không có thì `DT-04` không enforce được, kênh hỏng kéo cả hệ thống. | | | |
| 5 | Caller ID nhất quán và không bị gắn nhãn spam | 4 | Ảnh hưởng trực tiếp tỉ lệ nghe máy — chính là KPI của dự án. | | | |
| 6 | Ranh giới `dial_token` không buộc IVR giữ số | 4 | Ràng buộc privacy; giải pháp thay thế tốn kiến trúc. | | | |
| 7 | Năng lực mở rộng nhiều kênh + quota rõ ràng | 3 | Quyết định trần dung lượng production. | | | |
| 8 | Chi phí mỗi phút và mỗi kênh | 3 | Quyết định dự án có kinh tế không. | | | |
| 9 | CDR nối được sang `attempt_id` | 3 | Không có thì không đối soát được hoá đơn. | | | |
| 10 | SDK/tích hợp với .NET trên Linux | 2 | Thiếu thì phải nuôi một lớp đệm. | | | |
| 11 | Sandbox/môi trường test riêng | 2 | Thiếu thì mọi thử nghiệm đều chạm mạng thật. | | | |
| 12 | SLA, hỗ trợ, thời gian phản hồi sự cố | 2 | | | | |
| 13 | Chính sách phiên bản API và báo trước breaking change | 2 | | | | |
| 14 | Dữ liệu lưu trong nước / có DPA | 2 | Ảnh hưởng PDPA. | | | |
| 15 | TTS tiếng Việt sẵn có | 1 | Tiện nhưng thay thế được — xem [R-05](R-05-tts-audio-capability.md). | | | |
| 16 | Barge-in | 1 | Cải thiện trải nghiệm, không chặn. | | | |
| | **Tổng có trọng số** | **47** | | | | |

Ba trọng số cao nhất đều là **độ chính xác**, không phải hiệu năng hay giá. Lý do: sai một cuộc gọi ở đây là sai một đơn hàng của một khách hàng thật, và IVR không có cơ chế nào để phát hiện mình vừa ghi nhầm ý khách.

**Điểm trên giấy tối đa là `2`.** Chỉ số đo được từ lab hoặc từ một khách hàng tham chiếu mới cho `3`.

## 3. Gap register

Ghi mọi khoảng cách giữa yêu cầu và những gì nhà cung cấp đáp ứng. Một dòng cho một khoảng cách.

| # | Khoảng cách | Nhà cung cấp | Mức độ | Cách bù | Ai chịu chi phí | Trạng thái |
| --- | --- | --- | --- | --- | --- | --- |
| G-01 | `<điền>` | `<điền>` | chặn / cao / trung bình / thấp | `<điền>` | `<điền>` | `<điền>` |

Ba khoảng cách đã biết trước, không phụ thuộc nhà cung cấp nào:

| # | Khoảng cách | Mức độ | Ghi chú |
| --- | --- | --- | --- |
| G-A | Chưa sửa được `labDestinationAllowlist` / `globalDialKillSwitch` qua console. Permission `OD-V1-20` **đã cấp cho `Admin`** 2026-08-22, nhưng `PendingRuntimeGateAuthorization` vẫn chặn mọi mutation (`409 IVR_OPERATIONAL_BLOCKED`) | **chặn** | Còn thiếu **hai** thứ: chữ ký four-eyes, và một `IRuntimeGateAuthorization` duyệt thật. Chặn lịch lab, không chặn RFQ. Xem [R-02](R-02-lab-package.md) §5. |
| G-B | Chưa chọn TTS (`OD-V1-19`) | **chặn** | `PlayAsync` chưa có nguồn audio. Xem [R-05](R-05-tts-audio-capability.md). |
| G-C | Chưa chốt vị trí resolve `dial_token → E.164` (`OD-V1-18`) | **chặn** | Cần văn bản nhà cung cấp **và** quyết định Security. |

## 4. Điều khoản hợp đồng cần có

Những điều khoản này bảo vệ chống đúng các rủi ro mà kiến trúc IVR nhạy cảm với:

| # | Điều khoản | Chống rủi ro gì |
| --- | --- | --- |
| 1 | Báo trước tối thiểu 90 ngày cho breaking change ở API | Đối xứng với chính sách của IVR ở [`docs/api-versioning.md`](../../api-versioning.md) |
| 2 | **Danh sách trạng thái disposition thô là một phần của hợp đồng**; thêm/đổi/bỏ phải báo trước | Đây là rủi ro âm thầm nhất: thêm một trạng thái thô mới, IVR rơi vào nhánh mặc định và phân loại sai kết quả cuộc gọi |
| 3 | Cam kết recording tắt được, có thể kiểm chứng | `DT-05` |
| 4 | Cam kết không lưu số đã quay quá thời hạn thoả thuận; xoá theo yêu cầu | PDPA |
| 5 | SLA có cách đo và có bồi thường | |
| 6 | Quyền chấm dứt khi không đạt SLA nhiều kỳ liên tiếp | |
| 7 | Lấy được CDR lịch sử khi chấm dứt | Đối soát và kiểm toán |
| 8 | Không tự động đổi quota/ngưỡng mà không báo | Quota đổi giữa giờ cao điểm là sự cố production |
| 9 | Giá cố định trong kỳ tối thiểu; công thức điều chỉnh minh bạch | |
| 10 | Có môi trường test không tính cước hoặc tính riêng | Tránh mọi thử nghiệm đều chạm mạng thật |

Điều khoản 2 là điều khoản mà đội kỹ thuật hay quên nhất và tốn nhất khi thiếu: nó là phiên bản telephony của cùng vấn đề mà [T-08](../target-v1-closure-pack/T-08-openapi-compat-cdc.md) mô tả cho Sales — quản trị hợp đồng một chiều thì bên kia đổi lúc nào mình biết lúc đó.

## 5. Closure artifact

- [ ] **Scorecard §2 đã chấm** cho ít nhất 2 nhà cung cấp, có người chấm và ngày.
- [ ] **Gap register §3 đã điền**, mọi dòng mức "chặn" có cách bù và người chịu chi phí.
- [ ] **Điều khoản §4 đã đưa vào bản thảo hợp đồng**, ghi rõ điều nào nhà cung cấp từ chối.
- [ ] **Quyết định chọn nhà cung cấp** do Procurement ký — IVR không ký.

Scorecard điền xong **không** đóng `W-0008`. Nó chỉ cho phép bước sang mua sắm.
