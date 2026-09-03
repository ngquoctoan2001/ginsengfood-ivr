# W-0034 — Evidence: Opt-out feedback loop (`P4-6`)

Ngày: `2026-08-18` · Correction hiện hành: `W-0148 / 2026-09-03`

Trạng thái đúng: **`DEFERRED_TARGET / COMPONENT_TESTS_PASS / RUNTIME_NOT_WIRED / CONTRACT_REQUIRED`**

> **CORRECTION W-0148:** tài liệu cũ đã nói quá mức bằng chứng. Result `Rejected` tạo generic
> review item, nhưng không feed `OptOutSuppressionPolicy`; proposer chỉ được test bằng direct
> constructor, không có DI/runtime caller. Không có signal count store, CRM sender/ACK, reversal hay
> task-block E2E. Nội dung lịch sử bên dưới đã được sửa theo current source.

## 1. Câu hỏi duy nhất mà slice này trả lời

Đây là chỗ duy nhất IVR nói về **khả năng được liên hệ trong tương lai** của một khách hàng. Nên mọi test ở đây thực chất hỏi cùng một câu:

> Một khách hàng có thể bị chặn gọi mà chưa hề yêu cầu điều đó không?

Câu trả lời phải là không, ở mọi nhánh.

## 2. Result review đã có; opt-out capture chưa có

`DT-02` yêu cầu `rejected` → `NO_ANSWER` (có tính lượt) + cờ review, **không** phải cancel và
**không** phải opt-out. `DispositionMapper` làm đúng; `ResultRepository` tạo một item
`IVR_CALL_RESULT / OPEN` khi cờ bật.

Đó chỉ là capture cho operational review. Không có code lấy các review item này, gom theo contact,
đếm signal rồi gọi policy/proposer. Vì vậy §6.1 opt-out capture **chưa hoạt động**.

`UT-OPTOUT-CAP-01` chỉ khóa mapping `Rejected != cancel`; nó không chứng minh feedback-loop runtime.

## 3. Threshold `2/3` là scaffold chưa được owner ký

Một lần từ chối cuộc gọi **không phải** opt-out. Người ta từ chối vì đang lái xe, đang họp, hoặc không nhận ra số lạ. Chặn dựa trên một tín hiệu sẽ âm thầm gỡ những khách chưa hề yêu cầu được gỡ.

`OptOutThresholdPolicy.AbsoluteFloor = 2` và default `3` đúng là tồn tại trong code. Nhưng repo
không có owner decision về counting window, subject key, dedupe hay Legal approval cho việc suy intent
từ `Rejected`. Đây là local scaffold, **không phải production policy** và không được wire chỉ vì test xanh.

Nhánh `adminConfirmed` trong pure function cũng không chứng minh admin workflow. Current admin action
chỉ resolve item `OPEN`; proposal dùng status `PENDING_CRM`, nên không có approve/reject action phù
hợp. M8-08 đề xuất explicit-only V1; threshold inference nếu cần phải là contract V2 riêng.

## 4. IVR đề xuất, không chặn

`DO-CORR-2`: registry do-not-call thuộc **CRM**. Ba chỗ điều đó được biến thành thứ kiểm chứng được thay vì một câu trong tài liệu:

| Cơ chế | Nội dung |
| --- | --- |
| `SuppressionProposalStatus` | Chỉ có `PENDING_CRM` và `ACCEPTED_BY_CRM`. **Không có `SUPPRESSED`** — một trạng thái nghĩa "IVR đã chặn" là trạng thái IVR không có quyền ghi |
| `SuppressionDecision.SuppressedLocally` | Hằng `false`, để bất biến thành một giá trị assert được |
| Audit row | Mang thẳng `suppressed_by_ivr: false` và `registry_owner: "CRM"`, nên người đọc log sau này không thể nhầm một đề xuất đang xếp hàng với một lệnh chặn đã có hiệu lực |

`SuppressionChannel` là enum **đúng một phần tử** (`PhoneCall`, `DC-02`). IVR quan sát cuộc gọi thoại và không gì khác, nên nó không có tư cách nói gì về SMS hay marketing — và không có giá trị enum nào để nói.

## 5. Queue-only persistence tồn tại, nhưng chưa phải workflow

`QueueOnlySuppressionProposer` có thể ghi `ivr_review_items` với
`SourceType = IVR_OPTOUT_PROPOSAL` khi được gọi trực tiếp. Nhưng:

- không có runtime caller hoặc DI registration;
- không có signal aggregator/repository;
- không có CRM sender hoặc ACK handler;
- không gì set `ACCEPTED_BY_CRM`;
- generic admin mutation từ chối mọi status khác `OPEN`, trong khi proposal là `PENDING_CRM`.

Proposal có thể xuất hiện trong read queue, nhưng không thể từ đó suy ra “admin xác nhận hoặc loại”.
Lifecycle hiện tại chưa khép và phải thiết kế lại theo CRM contract.

**Không thêm `AddHttpClient` nào.** CRM chưa có endpoint nhận propose; thêm một client bây giờ là dựng bề mặt egress không có đối tác, và `UT-ARCH-NO-OPS-EGRESS-05` sẽ đỏ — đúng ra phải đỏ.

## 6. Test — phủ đủ §8

| Test | Khẳng định |
| --- | --- |
| `UT-OPTOUT-CAP-01` | `rejected` → NO_ANSWER có tính lượt + cờ review; **không bao giờ** là cancel; không chứng minh opt-out capture |
| `UT-OPTOUT-THRESH-02` | Pure function trả Hold/Propose với count do test truyền; không chứng minh count/store/policy production |
| `IT-OPTOUT-PROPOSE-03` | Direct-constructed proposer persist/audit idempotently; không chứng minh runtime caller, CRM send/ACK hay admin approval |
| `UT-OPTOUT-CHANNEL-04` | Mọi quyết định đều là `PHONE_CALL`; enum chỉ có một giá trị; `SuppressedLocally` là false |
| `IT-OPTOUT-FAILSAFE-04` | Hold là **thực sự trơ**: không dòng queue, không dòng audit — một hệ thống im lặng không chặn ai cả |

`IT-OPTOUT-FAILSAFE-04` là test tôi thêm ngoài §8. Lý do: fail-safe được mô tả là "propose lỗi → giữ ở queue, không chặn nhầm", nhưng nhánh nguy hiểm hơn là nhánh **không làm gì** — nếu "hold" vô tình ghi một dòng gì đó, một người đọc sau này có thể diễn giải nó thành một quyết định. Test khẳng định hold không để lại dấu vết nào.

## 7. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` (historical 2026-08-18) | **345/345** (22 contract + 195 unit + 128 integration), +5; chỉ là historical component proof |
| `dotnet build -warnaserror` | 0 warning / 0 error |

## 8. Cái này KHÔNG chứng minh

- **Không có capture-to-proposal runtime flow.** Policy/proposer không được orchestration gọi.
- **Không đóng vòng với CRM.** Không sender/ACK/reject/reverse; không gì chuyển `PENDING_CRM`.
- **IVR không giữ registry do-not-call.** Không trạng thái nào, không bảng nào biểu diễn được "đã chặn".
- **Không có admin approval workflow cho proposal.** Generic mutation chỉ xử lý item `OPEN`.
- **Không có proof task sau bị chặn.** M3/CRM read contract vẫn chưa build.
- **Threshold `2/3` không phải policy đã được Product/CRM/Legal ký.**
- **`W-0034` giữ `DEFERRED_TARGET`.** Vòng lặp chỉ thật sự khép khi CRM nhận đề xuất.
