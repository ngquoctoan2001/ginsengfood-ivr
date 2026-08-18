# W-0034 — Evidence: Opt-out feedback loop (`P4-6`)

Ngày: `2026-08-18` · Trạng thái đạt được: `DEFERRED_TARGET` giữ nguyên, phần capture/rule/propose đạt `TESTS_PASS`

## 1. Câu hỏi duy nhất mà slice này trả lời

Đây là chỗ duy nhất IVR nói về **khả năng được liên hệ trong tương lai** của một khách hàng. Nên mọi test ở đây thực chất hỏi cùng một câu:

> Một khách hàng có thể bị chặn gọi mà chưa hề yêu cầu điều đó không?

Câu trả lời phải là không, ở mọi nhánh.

## 2. Capture đã có sẵn — và nó đã đúng

`DT-02` yêu cầu `rejected` → `NO_ANSWER` (có tính lượt) + cờ review, **không** phải cancel. `DispositionMapper.cs:76` đã làm đúng, và `ResultRepository.cs:188` đã tạo review item khi cờ bật.

Nghĩa là §6.1 (capture) đã hoạt động từ P2-5. Slice này thêm hai thứ còn thiếu: **luật ngưỡng** và **hàng đợi đề xuất**.

`UT-OPTOUT-CAP-01` khoá lại phần đã có, vì nó là nền của toàn bộ vòng lặp: đọc "từ chối cuộc gọi" thành "huỷ đơn" sẽ huỷ những đơn khách chưa bao giờ yêu cầu huỷ.

## 3. Sàn hai tín hiệu nằm trong code, không nằm trong cấu hình

Một lần từ chối cuộc gọi **không phải** opt-out. Người ta từ chối vì đang lái xe, đang họp, hoặc không nhận ra số lạ. Chặn dựa trên một tín hiệu sẽ âm thầm gỡ những khách chưa hề yêu cầu được gỡ.

Nên `OptOutThresholdPolicy.AbsoluteFloor = 2` là **hằng số trong code**, và mọi ngưỡng cấu hình thấp hơn bị ném lỗi. Ngưỡng mặc định là 3.

Điểm tinh tế hơn: **admin cũng không được hành động trên một tín hiệu.** Một người xác nhận một cuộc gọi bị từ chối là đang xác nhận một **suy luận**, không phải một yêu cầu — và audit trail sẽ ghi lại một quyết định mà khách hàng chưa bao giờ đưa ra. Admin có thể hành động **sớm hơn ngưỡng** (từ 2 tín hiệu), đó là mục đích của hàng đợi review; nhưng không sớm hơn sàn.

## 4. IVR đề xuất, không chặn

`DO-CORR-2`: registry do-not-call thuộc **CRM**. Ba chỗ điều đó được biến thành thứ kiểm chứng được thay vì một câu trong tài liệu:

| Cơ chế | Nội dung |
| --- | --- |
| `SuppressionProposalStatus` | Chỉ có `PENDING_CRM` và `ACCEPTED_BY_CRM`. **Không có `SUPPRESSED`** — một trạng thái nghĩa "IVR đã chặn" là trạng thái IVR không có quyền ghi |
| `SuppressionDecision.SuppressedLocally` | Hằng `false`, để bất biến thành một giá trị assert được |
| Audit row | Mang thẳng `suppressed_by_ivr: false` và `registry_owner: "CRM"`, nên người đọc log sau này không thể nhầm một đề xuất đang xếp hàng với một lệnh chặn đã có hiệu lực |

`SuppressionChannel` là enum **đúng một phần tử** (`PhoneCall`, `DC-02`). IVR quan sát cuộc gọi thoại và không gì khác, nên nó không có tư cách nói gì về SMS hay marketing — và không có giá trị enum nào để nói.

## 5. Hàng đợi dùng lại `ivr_review_items`, không thêm bảng

`P4-6` §7 gợi ý một migration riêng. Tôi dùng lại `ivr_review_items` với `SourceType = IVR_OPTOUT_PROPOSAL`, vì:

- một đề xuất **đã là** một khái niệm review-queue: admin cần thấy nó, xác nhận hoặc loại nó, và nó cần cùng chính sách retention;
- bảng song song sẽ nhân đôi vòng đời và **chẻ bề mặt admin làm hai**;
- console review queue lọc theo `status` chứ không theo `source_type`, nên đề xuất hiện ra ngay **không cần màn hình mới** (§6.4).

Đề xuất resolve về **không có call job** — đúng như vậy: một đề xuất nói về một liên hệ, không nói về một cuộc gọi cụ thể. Code resolve đã degrade sạch sang null, không ném.

**Không thêm `AddHttpClient` nào.** CRM chưa có endpoint nhận propose; thêm một client bây giờ là dựng bề mặt egress không có đối tác, và `UT-ARCH-NO-OPS-EGRESS-05` sẽ đỏ — đúng ra phải đỏ.

## 6. Test — phủ đủ §8

| Test | Khẳng định |
| --- | --- |
| `UT-OPTOUT-CAP-01` | `rejected` → NO_ANSWER có tính lượt + cờ review; **không bao giờ** là cancel |
| `UT-OPTOUT-THRESH-02` | 1 tín hiệu (kể cả admin xác nhận) → hold; 2 → hold; 3 → propose; admin ở mức 2 → propose có ghi rõ là người; ngưỡng cấu hình < 2 → ném lỗi |
| `IT-OPTOUT-PROPOSE-03` | Đề xuất được xếp hàng bền vững + audit; **không dòng nào** mang trạng thái suppress; audit khai `suppressed_by_ivr: false`, `registry_owner: CRM`; hiện trong review queue; đề xuất lại **không** nhân bản |
| `UT-OPTOUT-CHANNEL-04` | Mọi quyết định đều là `PHONE_CALL`; enum chỉ có một giá trị; `SuppressedLocally` là false |
| `IT-OPTOUT-FAILSAFE-04` | Hold là **thực sự trơ**: không dòng queue, không dòng audit — một hệ thống im lặng không chặn ai cả |

`IT-OPTOUT-FAILSAFE-04` là test tôi thêm ngoài §8. Lý do: fail-safe được mô tả là "propose lỗi → giữ ở queue, không chặn nhầm", nhưng nhánh nguy hiểm hơn là nhánh **không làm gì** — nếu "hold" vô tình ghi một dòng gì đó, một người đọc sau này có thể diễn giải nó thành một quyết định. Test khẳng định hold không để lại dấu vết nào.

## 7. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **345/345** (22 contract + 195 unit + 128 integration), +5 |
| `dotnet build -warnaserror` | 0 warning / 0 error |

## 8. Cái này KHÔNG chứng minh

- **Không đóng vòng với CRM.** CRM chưa có endpoint nhận propose; đề xuất nằm ở `PENDING_CRM` và không có gì chuyển nó đi. Đó là external work, không phải nợ của IVR.
- **IVR không giữ registry do-not-call.** Không trạng thái nào, không bảng nào biểu diễn được "đã chặn".
- **Không auto-suppress từ một tín hiệu**, kể cả khi admin xác nhận.
- **Không có màn hình console riêng cho đề xuất.** Nó hiện như một dòng review chung, không có link call job. Thêm affordance riêng là việc của một slice UI sau, khi CRM đã nhận propose và vòng lặp có ý nghĩa vận hành.
- **`W-0034` giữ `DEFERRED_TARGET`.** Vòng lặp chỉ thật sự khép khi CRM nhận đề xuất.
