# Kế hoạch hoàn thiện IVR: Mobile SIP Trunk và 32 cuộc gọi đồng thời

Ngày lập: **15/09/2026**. Trạng thái: **PLANNED — mới lập kế hoạch, chưa triển khai các thay đổi bên dưới**.

Rà soát lần 2: **15/09/2026 — đã hiệu chỉnh kế hoạch**, chưa sửa code/config runtime. Kết luận: hướng Mobile SIP Trunk trực tiếp phù hợp phạm vi đã chọn; bản đầu còn thiếu các ràng buộc và phép thử được ghi tại mục 11. Mốc “hai lần trong 10 phút” trong trao đổi mới được đối chiếu riêng tại mục 3.1, không mặc nhiên coi là hành vi code đã có.

Baseline đã đọc: `main@2667f4247a0d323647e1b11f7a27578da812b400`. Các nhận xét hiện trạng dưới đây dựa trên source tại baseline này; phải kiểm tra lại HEAD và phần việc đang làm dở trước khi thực hiện.

**Mục tiêu:** hoàn thiện đường gọi từ IVR của mình qua Asterisk và **một nhà mạng Mobile SIP Trunk**, nhận đúng phản hồi phím `1/0`, trả kết quả về Module 3 và chứng minh tối đa **32 cuộc gọi ra đồng thời** trên tuyến đã đăng ký.

Tài liệu này là kế hoạch kỹ thuật riêng. [Báo cáo so sánh để trình sếp](C:/Users/Administrator/Desktop/ivr/docs/reports/2026-09-15-so-sanh-mobile-sip-trunk-va-gateway-32-sim.md) giữ nguyên; giá dịch vụ, phương án gateway 32 SIM và thủ tục tên định danh nằm trong báo cáo đó.

## 1. Phạm vi cần hoàn thành

| Trong đợt này | Để giai đoạn sau / ngoài phạm vi |
| --- | --- |
| Một nhà mạng: VinaPhone, MobiFone hoặc Viettel; một cấu hình trunk và số gọi ra được nhà mạng cấp quyền | Nhiều nhà mạng, tra cứu mạng đích và định tuyến tối ưu cước |
| Gọi ra xác nhận đơn do M3 giao; phát lời thoại; nhận `1/0`; trả callback | Hotline gọi vào, IVR bấm phím chuyển nhân viên, mua thêm nền tảng Auto Call |
| Hoàn thiện adapter, đích quay số, cấu hình, điều phối, phục hồi và kiểm thử 32 phiên | Gateway vật lý 4G/VoLTE, 32 SIM, tích hợp modem |
| Dùng lại intake, policy, lời thoại/audio, chuẩn hóa kết quả và callback hiện có | Viết lại hệ thống IVR, thay nhà cung cấp giọng hoặc xây console mới |
| Kiểm chứng tên công ty hiển thị theo dịch vụ Voice Brandname đã mở | Tự gán chữ trong SIP để coi như đã có Brandname |

Phương án này **không mua gói Tổng đài ảo + API Auto Call của PA**. Nhà cung cấp chỉ cung cấp tuyến SIP, số/Brandname và dung lượng đã thỏa thuận; IVR/Asterisk của mình thực hiện gọi và thu DTMF. Nếu nhà cung cấp chỉ đưa API nhận link audio để họ gọi hộ, đó là phương án tích hợp khác, không thay vào SIP-03 như một bộ thông tin SIP.

**32 kênh ở đây là 32 suất gọi đồng thời của trunk**, không phải 32 SIM. Số gọi ra có được dùng cho đủ 32 phiên hay không phải nằm trong xác nhận dịch vụ. Phải đếm cả phiên đang thiết lập/đổ chuông khi chúng chiếm tài nguyên; không chỉ đếm khách đang nói chuyện.

## 2. Hiện trạng đã xác minh và đầu ra cần đạt

| Hiện trạng tại baseline | Việc phải làm và bằng chứng đóng |
| --- | --- |
| `AsteriskAriSimGateway.DialAsync` chỉ nhận alias lab, dựng endpoint từ alias và dùng caller ID lab. [Source](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/AsteriskAriSimGateway.cs:82) | Bổ sung đường production nhận quyền quay số cho từng task, resolve số trong biên telephony, chọn đúng trunk/số gọi ra; test hai task tới hai đích khác nhau. |
| DI chỉ nối Asterisk khi `asteriskLab`; ngoài MOCK/lab dùng dispatcher chưa khả dụng. [Source](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs:538) | Đăng ký đủ resolver, bảo vệ token, gateway, dispatcher và cấu hình production; kiểm thử khởi tạo toàn bộ host cho từng mode. |
| Dispatcher chỉ sẵn sàng ở lab và truyền cứng `ExecutionMode.LabRealSim` vào lời thoại. [Source](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/AsteriskSchedulerDispatchGateway.cs:28) | Dispatcher production dùng đúng mode và approval của production; không lấy approval lab để phát cho khách. |
| `SchedulerRuntime.RunOnceAsync` đợi hết `DispatchAsync`; host polling cũng đợi lượt đó xong. [Scheduler](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/SchedulerRuntime.cs:134), [host](C:/Users/Administrator/Desktop/ivr/src/Ivr.Worker/Jobs/PollingJobHost.cs) | Tách việc nhận việc và xử lý cuộc gọi; chạy đồng thời có giới hạn, tiếp tục bảo trì lease/deadline/liveness khi đang gọi. |
| DB đã có khóa cấp phát, lease token và fencing generation; lab chỉ cấp một channel. [Store](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs), [provisioner](C:/Users/Administrator/Desktop/ivr/src/Ivr.Worker/Jobs/AsteriskLabChannelProvisioner.cs) | Dùng lại cơ chế DB, thêm pool kênh logic của trunk và phục hồi có đối soát. `ClaimBatchSize=32` không tự tạo ra 32 cuộc đồng thời. |
| Validator hiện từ chối `RealCustomerCallAllowed=true`; `DispatchGate` kiểm allowlist lab cho mọi nhánh gọi không phải MOCK. [Validator](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Configuration/IvrOptionsValidator.cs:53), [gate](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/FeatureFlags/DispatchGate.cs:55) | Hoàn thiện quy trình kích hoạt production có bằng chứng phê duyệt và kiểm tra đích theo mode; không chỉ xóa điều kiện chặn hoặc bật biến môi trường. |
| Vault lab băm token không thể giải ngược và trả cùng alias; ledger resolve hiện nằm trong bộ nhớ. [Vault](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/LabDialTokenVault.cs), [ledger](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/DialTokenResolveLedger.cs) | Có đường lấy số thật được phép và bộ đếm resolve bền vững qua restart/nhiều tiến trình; không dùng fingerprint lab làm token production. |
| Trạng thái cuộc gọi ARI nằm trong `ConcurrentDictionary`; mất kết nối sự kiện có thể làm trạng thái trong IVR khác trạng thái còn tồn tại ở Asterisk. [Gateway](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/AsteriskAriSimGateway.cs) | Lưu liên kết attempt–channel trước originate; đối soát sau lỗi, không giải phóng suất rồi gọi lại khi chưa biết cuộc cũ còn tồn tại hay không. |
| `LoadAsync` kiểm revoke trước bước tạo audio; dispatcher còn resolve/render/synthesize rồi mới dial. Khoảng kiểm quyền → quay số có thể kéo dài theo xử lý audio. [Store](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/PostgresTelephonyDispatchStore.cs:154), [dispatcher](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/AsteriskSchedulerDispatchGateway.cs:94) | Kiểm lại task/lease/deadline/quyền gọi sát originate sau audio; test revoke trong lúc audio đang bị giữ, không chỉ revoke trước claim. |
| Health lab chỉ ping ARI và trả trạng thái recording từ options. [Health](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/AsteriskAriSimGateway.cs:295) | Có health tuyến SIP/media/định danh và kiểm cấu hình recording thực tế; ARI sống chưa chứng minh nhà mạng nhận cuộc gọi. |

Đây là các khoảng trống kỹ thuật, chưa phải kết quả chạy production. Lần lập kế hoạch này chỉ đọc source/tài liệu, không gọi nhà mạng, không chạy tải và không kích hoạt cờ gọi khách thật.

## 3. Những quyết định đã có phải giữ đúng

1. **M3 quyết định đơn cần gọi.** IVR thực thi và gửi kết quả; M3 kiểm lại trạng thái/phiên bản đơn trước khi cập nhật nghiệp vụ. `0` là yêu cầu hủy đơn, không phải yêu cầu ngừng mọi liên hệ. [Hồ sơ M3](C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/questions-to-module-3-od18-authority.md), [register hiện hành](C:/Users/Administrator/Desktop/ivr/specs/_review/open-decisions-register.md).
2. **Resolver nằm trong IVR, bên trong adapter telephony.** Số E.164 chỉ sống trong bộ nhớ phục vụ quay số; không ghi DB ứng dụng, log, evidence hay callback. `DialAuthorization` ở các biên còn lại vẫn là tham chiếu opaque. Không chuyển cả danh bạ cho nhà mạng để họ tự giải token. Đây là quyết định `OD-V1-18` đã ký, không mở lại ở kế hoạch này. [Hợp đồng adapter](C:/Users/Administrator/Desktop/ivr/specs/api/04-sim-adapter-contract.md), [port hiện tại](C:/Users/Administrator/Desktop/ivr/src/Ivr.Domain/Ports/ProviderPorts.cs).
3. **Token dùng lại có giới hạn**, gắn task, mỗi lần resolve ứng với attempt khác nhau; không tự thêm API cấp lại hoặc đổi thành mảng token. TTL **bằng đúng cuối cửa sổ xác nhận** theo quyết định ngày 09/09, thay mốc `+60s` cũ. [Quyết định TTL](C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/00-DA-XONG.md#od-v1-17-ttl).
4. **Không sửa lén policy của các job đã được nhận.** Đối chiếu yêu cầu mới tại mục 3.1 trước khi chọn cấu hình đích; thay policy phải có phiên bản và hợp đồng M3 tương ứng. Khung giờ hiện hành đã sửa giờ kết thúc thành `21:08` ngày 07/09; không lấy lại `21:00` từ hồ sơ cũ. Recording vẫn tắt; approval lời thoại giữ đúng phạm vi. [Register](C:/Users/Administrator/Desktop/ivr/specs/_review/open-decisions-register.md), [calling window](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/CallingWindow.cs).
5. **Chữ ký và khả năng chạy là hai điều phải kiểm riêng.** Register ngày 10/09 ghi owner đã nhận quyết định chính sách nội dung, nhưng kiểm tra ba actor trong code production vẫn còn. Kế hoạch không tự tạo approver, không lách kiểm tra; cần làm rõ cách thực thi quyết định hiện hành trước mốc gọi khách. Đây không phải yêu cầu ký lại tất cả quyết định đã đóng. [Register](C:/Users/Administrator/Desktop/ivr/specs/_review/open-decisions-register.md:38), [kiểm tra script](C:/Users/Administrator/Desktop/ivr/src/Ivr.Domain/Scripts/ScriptContentContracts.cs).

### 3.1. Đối chiếu yêu cầu “tối đa hai lần trong 10 phút”

**Yêu cầu người dùng vừa mô tả:** chỉ gọi ra đọc/xác nhận đúng đơn; tối đa hai lần trong 10 phút; nhiều kênh phục vụ nhiều khách đồng thời. Bản plan trước chỉ dẫn policy cũ nên chưa thể chứng minh yêu cầu này đã khớp runtime.

| Khía cạnh | Source tại baseline | Việc phải ghi rõ trước khi hoàn tất SIP-01 |
| --- | --- | --- |
| Lịch gọi | Golden Hour: hai customer attempt, offset `0/150s`, cửa sổ `300s`. 24/7: hai customer attempt, offset `0/450s`, cửa sổ `900s`. [Policy](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs:55) | “10 phút” là giới hạn thời điểm bắt đầu hai lượt hay thời hạn xác nhận toàn đơn; lấy mốc từ lúc nào và khoảng cách hai lần là bao nhiêu. Không tự đổi cả hai chương trình thành TTL 600s chỉ từ cách nói tóm tắt. |
| Cách đếm hai lần | Technical exception không tính customer attempt; trần retry kỹ thuật mặc định là 1, resolver budget dùng `MaxAttempts + TechnicalRetryLimit`. [Normalizer](C:/Users/Administrator/Desktop/ivr/src/Ivr.Domain/Confirmation/DispositionMapper.cs:192), [dispatch context](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/PostgresTelephonyDispatchStore.cs:189) | Phân biệt customer attempt, yêu cầu originate, lần đổ chuông và lượt resolve token. Để giữ lời hứa “tối đa hai cuộc”, không cho retry kỹ thuật tạo cuộc thứ ba tới khách; thao tác chưa hề gửi originate không tự được coi là một lần đã gọi. |
| Phạm vi giới hạn | Lịch/budget hiện gắn job/task. | Ghi rõ giới hạn theo đơn và cơ chế tránh nhiều đơn cùng số bị gọi chồng. Đề xuất một cuộc đang hoạt động trên mỗi đích liên hệ; dùng tham chiếu ổn định do M3 cung cấp/được phép, không lưu raw phone để khóa. Không tự gộp đơn hoặc phát sinh kết quả cho đơn bị chờ. |

Đây là **điểm cần đối chiếu hợp đồng**, không phải kết luận hệ thống hiện đã vi phạm “hai lần/10 phút”. Hai offset hiện tại đều dưới 10 phút, nhưng hàng đợi chậm và retry kỹ thuật có thể làm cách vận hành thực tế khác lời mô tả. Nếu yêu cầu đích thay policy, phiên bản mới phải giữ snapshot job cũ và được kiểm cả producer M3, intake, scheduler, token TTL, normalization và callback; không sửa hằng số riêng ở adapter.

Sau phản hồi hợp lệ `1/0`, hoặc khi task đã bị thu hồi/đóng, không tạo lần gọi tiếp. Timeout HTTP sau originate chưa rõ kết quả phải giữ chỗ trong budget quay số cho tới khi đối soát, không được tính như “chưa gọi” để thử lại.

## 4. Thiết kế triển khai đề xuất

### 4.1. Luồng đích

```mermaid
sequenceDiagram
    participant M3 as Module 3
    participant IVR as IVR / Scheduler
    participant TEL as Adapter telephony trong IVR
    participant AST as Asterisk
    participant NET as Nhà mạng Mobile SIP Trunk
    participant KH as Điện thoại đích
    M3->>IVR: Task xác nhận đơn + dial_token + thời hạn
    IVR->>IVR: Kiểm task, policy, giờ gọi và quyền gọi
    IVR->>IVR: Chuẩn bị audio trong hàng đợi có giới hạn
    IVR->>IVR: Giữ một suất trống trong pool tối đa 32
    IVR->>IVR: Kiểm lại task, lease, deadline, quyền gọi và budget
    IVR->>TEL: Attempt + lease + quyền quay số opaque
    TEL->>TEL: Kiểm token, resolve E.164 trong bộ nhớ
    TEL->>AST: ARI originate với channel ID đã ghi nhận
    AST->>NET: Yêu cầu gọi qua trunk và số đã đăng ký
    NET->>KH: Đổ chuông; tên hiển thị theo dịch vụ đã mở
    KH-->>AST: Trả lời cuộc gọi qua nhà mạng
    AST-->>TEL: Sự kiện đã kết nối
    TEL->>AST: Phát audio đã chuẩn bị/được duyệt
    AST->>KH: Nội dung đơn và hướng dẫn bấm 1 hoặc 0
    KH-->>AST: Phím bấm truyền qua nhà mạng
    AST-->>TEL: DTMF gắn đúng channel / attempt
    TEL->>AST: Kết thúc cuộc gọi
    AST-->>TEL: Xác nhận channel đã kết thúc
    TEL-->>IVR: Disposition, phím bấm và mốc thời gian
    IVR->>IVR: Lưu kết quả; trả suất sau xác nhận/cooldown
    IVR->>M3: Callback qua outbox, retry theo hợp đồng
    M3-->>IVR: ACK sau kiểm tra và xử lý nghiệp vụ
```

Sơ đồ là nhánh thành công. Mất mạng, timeout, đơn bị thu hồi, sai phím và callback thất bại đi theo policy/nhánh phục hồi; không suy đoán rằng khách đã hủy hoặc không nghe máy chỉ từ một lỗi HTTP.

### 4.2. Cấu trúc code

- Giữ adapter lab riêng. Thêm profile Mobile SIP Trunk và adapter production; chỉ tách phần ARI dùng chung khi có test hồi quy bảo vệ hành vi lab. Tên class/file mới chốt lúc triển khai, không cần đổi toàn bộ tên `ISimGateway` và bảng `ivr_sim_channels` chỉ vì chuyển sang trunk.
- Thêm resolver production và bảo vệ token bằng khóa do Platform quản lý. Bên trong biên telephony, quyền opaque được đổi thành số dùng tạm thời để dựng địa chỉ quay số. Nhà mạng nhận số điện thoại qua SIP; họ không tự hiểu `dial_token` của ứng dụng.
- Lưu bền vững `task_id`, `attempt_id`, ID channel, lease/fencing và trạng thái dispatch. Không lưu URI chứa số, SIP body, mật khẩu, nguyên token hoặc nội dung đơn vào evidence.
- Dùng lại renderer/audio service và callback outbox. Kiểm tra approval/media có sẵn cho production; không coi việc TTS đã hoạt động ở lab là đã nghiệm thu bộ lời thoại production.

### 4.3. Chạy 32 cuộc và sở hữu ARI

**Đề xuất V1: một controller ARI đang hoạt động cho một Asterisk application, xử lý tối đa 32 tác vụ cuộc gọi bất đồng bộ.** Worker thứ hai không được mở thêm event socket cho cùng application khi chưa có cơ chế bàn giao. Asterisk quy định socket mới có thể thay socket đang giữ cùng application; vì vậy tăng replica tùy ý có thể làm mất quyền nhận sự kiện. [Tài liệu chính thức về ApplicationReplaced](https://docs.asterisk.org/Certified-Asterisk_22.8_Documentation/API_Documentation/Asterisk_REST_Interface/Asterisk_REST_Data_Models/#applicationreplaced).

- Khóa sở hữu controller trong DB là điều kiện nhận việc, **không phải hàng rào được Asterisk tự hiểu**. V1 dùng một controller được phép điều khiển, không tự chuyển quyền chỉ vì lease cũ hết hạn. Nếu tiến trình/node cũ chưa xác nhận đã dừng hoặc mất quyền truy cập ARI, instance thay thế giữ trạng thái chờ; phải cô lập quyền điều khiển cũ rồi đối soát mới mở cuộc mới. Đây là đánh đổi thời gian phục hồi để tránh hai controller cùng gọi.
- Profile triển khai controller đặt một replica, tắt autoscale cho phần giữ ARI, nâng cấp theo trình tự dừng/drain rồi khởi động lại. Khi dùng Deployment có thể chọn `Recreate`, nhưng nó chỉ giúp thứ tự nâng cấp, không tự bảo đảm một process sống duy nhất khi mất node/force delete. Cơ chế kiểm quyền/đối soát phía ứng dụng vẫn bắt buộc. [Kubernetes Deployment strategy](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#recreate-deployment).
- Pool DB tối đa 32 suất theo trunk, cộng giới hạn tác vụ cục bộ. Một semaphore trong từng process không thay thế giới hạn toàn hệ thống.
- Hạn mức triển khai ban đầu là **1**, tăng có kiểm chứng `1 → 4 → 8 → 16 → 32`. Cấu hình không vượt số phiên nhà mạng thực sự cấp.
- Tách `MaxConcurrentCalls` và `MaxCallStartsPerSecond` (CPS). Có 32 suất trống không có nghĩa được tạo 32 cuộc trong cùng một giây.
- Chỉ claim khi có chỗ thực thi; không giữ lease cho một hàng đợi trong RAM chờ quá lâu. Task được giám sát, lỗi được thu nhận, scope/DbContext riêng theo cuộc; không dùng vòng `Task.Run` bỏ mặc kết quả.
- Tách vòng bảo trì lease/deadline/heartbeat khỏi thời gian phát lời thoại và chờ DTMF. Áp lực audio, DB hoặc ARI phải làm chậm việc nhận cuộc mới thay vì làm sập worker.
- Pool logic phải dành riêng cho đường gọi này hoặc trừ phần dùng chung mà nhà mạng xác nhận. Một trunk hết quota, sai xác thực hoặc bị chặn phải ngừng nhận cuộc trên cả pool; không quay vòng qua 32 suất logic như thể đó là 32 tuyến độc lập.

### 4.4. Không gọi trùng khi mất kết nối

ARI hỗ trợ truyền `channelId` khi originate và truy vấn channel đang tồn tại. Kế hoạch dùng ID được tạo/lưu trước khi gửi để phục vụ đối soát; HTTP thành công chỉ xác nhận thao tác API, không tự chứng minh khách đã nghe. Với originate dùng `app`, Asterisk chuyển channel vào Stasis khi được trả lời; vẫn phải kiểm chứng nhánh đổ chuông/early media trên tuyến. [ARI Channels API — Asterisk 22](https://docs.asterisk.org/Asterisk_22_Documentation/API_Documentation/Asterisk_REST_Interface/Channels_REST_API/).

- Timeout sau gửi originate là **chưa rõ cuộc gọi đã tạo hay chưa**. Tra channel đã biết và đối soát trước khi quyết định; không tự tạo attempt mới để thử lại ngay. `404`/không thấy channel chỉ nói channel không còn tồn tại lúc kiểm, không chứng minh cuộc gọi chưa từng được tạo. ID channel là khóa đối chiếu, không thay ledger chống gọi trùng bền vững.
- Gia hạn lease phải kiểm đúng lease token/fencing. Hết lease không cho phép tái sử dụng suất ngay khi Asterisk có thể còn giữ cuộc cũ.
- Mất event socket, controller mất quyền hoặc Asterisk restart: dừng nhận cuộc mới, đánh dấu cần đối soát, đọc trạng thái thực tế rồi kết thúc/khôi phục theo quy tắc đã kiểm thử.
- Nếu không đủ bằng chứng cuộc cũ đã hết, giữ suất cách ly và đưa attempt vào review. Không hứa bảo đảm “đúng một lần” qua mọi sự cố mạng; mục tiêu là không tự gọi lại khi kết quả lần trước chưa rõ.
- Phân biệt timeout của token để **bắt đầu** cuộc mới và thời gian xử lý cuộc đang chạy. Không sửa TTL đã ký để che lỗi lease.

### 4.5. Thứ tự audio, quyền gọi và thời hạn

Chuẩn bị audio bằng worker/queue có giới hạn trước khi chiếm suất trunk, hoặc chứng minh cách giữ lease khi chuẩn bị không làm mất dung lượng. Audio phải gắn đúng task/summary/script hash; cache hết hạn/xóa file chỉ sau khi không còn playback đang dùng. Không lấy cùng một audio đơn thử rồi coi là đã thử tải sinh audio của 32 đơn khác nhau. [Synthesis/cache](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Speech/SpeechSynthesisService.cs:114).

Sát originate kiểm lại: controller ownership, lease/fencing, task chưa revoke/đóng, giờ gọi, hạn bắt đầu cuộc, script còn hợp lệ, đích đúng quyền, quota/CPS và budget. Sau resolve có thể có độ trễ mạng nên kiểm hạn/quyền lại trước gửi. Chỉ giữ số đích trong bộ nhớ của biên telephony cho thời gian cần thiết; không giữ transaction DB xuyên qua HTTP/SIP.

Revoke/kill switch đến sau lần kiểm cuối nhưng trước khi Asterisk nhận lệnh vẫn có một khoảng đua. Định nghĩa điểm quyết định, đo độ trễ và kiểm cơ chế hủy cuộc đang dở; không hứa “thu hồi là không thể có thêm chuông” trong mọi thời điểm. Poll termination phải hoạt động khi đang đổ chuông, đang phát audio và chờ DTMF, không chỉ lúc chờ phím. [Giới hạn revoke đã ghi nhận](C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/00-CHUA-XONG.md#m8-17).

Thời gian giữ lease phải bao phủ quá trình chuẩn bị còn nằm trong lease, đổ chuông, phát hết audio, chờ DTMF và kết thúc/đối soát; hoặc có gia hạn đã kiểm thử. Mặc định hiện có audio tối đa 120s, ring timeout 30s, DTMF 15s trong khi lease 120s, nên không được coi cấu hình mặc định đủ cho mọi cuộc. Thêm thời hạn cứng cho cuộc đang chạy và cho từng thao tác mạng; quá hạn thì kết thúc/đối soát, không treo suất vô hạn.

### 4.6. Dữ liệu mới và triển khai có thể quay lại

- Lưu liên kết attempt–Asterisk instance–channel ID–trunk–lease generation, ý định dispatch và các mốc đã gửi/đã nghe/đã kết thúc/đã đối soát. Một attempt chỉ được phát một ý định originate hợp lệ; lập khóa duy nhất và cập nhật trạng thái nguyên tử. Chuẩn hóa sự kiện theo ID ổn định, không tạo kết quả/callback cuối trùng khi phát lại event.
- Ledger token có budget/task binding bền vững. Dùng mã hóa có xác thực, phân tách mục đích và key version; kiểm rotation, mất khóa và xóa theo retention. Token lab đã băm không thể chuyển thành token production: tách dữ liệu/môi trường và chỉ nhận task mới hợp lệ, không “nâng cấp” task lab để gọi khách.
- Migration bổ sung theo cách tương thích; không sửa migration lịch sử hoặc backfill số điện thoại. Diễn tập nâng schema và chạy lại bản binary trước với dial bị khóa; không giả định down migration là đường quay lại an toàn.
- Khi rollback: khóa cuộc mới → drain/kết thúc có xác nhận hoặc giữ cách ly → thu hồi quyền controller cũ → quay lại image/config đã xác minh → đối soát các attempt/outbox còn lại. Không tự fallback sang MOCK/LAB trên task production; lỗi callback M3 chỉ retry callback, không quay lại gọi khách để lấy kết quả lần nữa.

## 5. Danh sách công việc thực hiện

Các mã `SIP-01…SIP-10` chỉ dùng trong tài liệu này, không phải W-ID mới. Toàn bộ hiện là **PLANNED**. Chỉ cập nhật tracker chính sau khi có code/bằng chứng tương ứng.

### SIP-01 — Chốt cấu hình đầu vào và hợp đồng nối tuyến

**Làm ngay:** lập bảng cấu hình không chứa secret; chốt một provider profile, sơ đồ mạng, các đích thử được phép và người phụ trách từng đầu vào ở mục 6. Ghi phiên bản Asterisk triển khai thực tế; Dockerfile lab hiện pin `22.10.1`, không tự đổi phiên bản cùng đợt này. Chốt cách thử cô lập mà không mở gọi từ môi trường dev/staging.

**Đầu ra:** hợp đồng tích hợp trunk; mẫu config với giá trị placeholder; hợp đồng resolver token và nguồn trả số được phép. Nếu chưa có nhà mạng, dùng SIP peer thử trong mạng cô lập để phát triển, ghi rõ đây là giả lập tuyến.

**Bổ sung sau rà soát:** ghi kết quả đối chiếu mục 3.1 và lập tệp tham số nghiệm thu gồm số kênh/CPS được cấp, giới hạn theo đơn/đích, thời hạn bắt đầu và kết thúc cuộc, số đơn ở đỉnh tải, độ trễ hàng đợi/audio/DTMF, ngưỡng lỗi và hạn mức cước thử. Các ngưỡng phải điền trước bài chạy tương ứng; không để trống rồi chọn ngưỡng theo kết quả đã thấy. Đầu mối nhà mạng xác nhận kịch bản chỉ gọi ra xác nhận đơn, xử lý chặn nhầm và ngày test, không chỉ trả lời “CSKH thì OK”.

**Đạt khi:** không còn lẫn trách nhiệm Auto Call với nhà mạng; mỗi thông số thiếu có người cung cấp và trạng thái rõ. Sẵn sàng viết code dùng giao diện giả lập; chưa được coi là đã có tuyến thật.

### SIP-02 — Resolver và token production

**Phụ thuộc:** SIP-01 về hợp đồng token; có thể viết test double trước khi có endpoint/khóa thật.

**Làm:** triển khai `IOpaqueValueProtector` dùng khóa quản lý; nối intake → token đã bảo vệ → resolver trong biên telephony. Bổ sung ledger DB nguyên tử cho task binding, expiry, attempt đã resolve và tổng budget. Giữ TTL equality và trần theo policy hiện hành; retry HTTP của resolver không được vô tình tiêu thụ thêm một lượt hoặc tạo thêm cuộc gọi.

**Scope đọc/sửa dự kiến:** [bảo vệ token](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Persistence/Security/IOpaqueValueProtector.cs), [ledger hiện tại](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/DialTokenResolveLedger.cs), [dispatch store](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/PostgresTelephonyDispatchStore.cs), migration và DI liên quan.

**Đạt khi:** hai task/token khác nhau quay tới đúng hai đích thử khác nhau; một token luôn bị ràng vào đúng task/contact được cấp. Sai task, hết hạn, quá trần, replay bị từ chối; restart và hai tiến trình tranh cùng token không làm reset/tăng budget. Phân biệt retry transport của cùng thao tác resolve với cấp quyền quay số mới; hết hạn/unknown không được hoàn budget để gọi lại mù. Số, token và mapping không xuất hiện trong log/trace/evidence, kể cả HTTP lỗi và ARI error body. Kiểm cấu hình log/CDR phía Asterisk vì dữ liệu báo hiệu cũng chứa số đích; không copy CDR thô vào evidence ứng dụng.

### SIP-03 — Profile trunk, adapter và nhánh DI production

**Phụ thuộc:** SIP-01; dùng resolver giả lập cho tới khi SIP-02 hoàn tất.

**Làm:** thêm options cho trunk endpoint, outbound identity được phép, mode/environment và transport; dựng endpoint quay số theo tài liệu nhà mạng, kiểm tra/chuẩn hóa số ngay trong adapter. Bổ sung gateway và dispatcher production; truyền mode đúng vào renderer/synthesis. Đăng ký đầy đủ các dependency, không rơi về `UnavailableSchedulerDispatchGateway` khi cấu hình production hợp lệ.

**Scope:** [ARI options](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/AsteriskAriOptions.cs), [ARI gateway](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/AsteriskAriSimGateway.cs), [dispatcher](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/AsteriskSchedulerDispatchGateway.cs), [DI](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs).

**Đạt khi:** test composition root cho MOCK/lab/production chọn đúng implementation; cấu hình thiếu/sai bị từ chối với mã lỗi an toàn; đích hoặc trunk ngoài cấu hình không được quay số. Bài lab `LAB-A` hiện có vẫn giữ nguyên phạm vi và hành vi.

### SIP-04 — Quy trình cho phép gọi production

**Phụ thuộc:** SIP-03; có thể xây test quyết định cho phép/từ chối trước khi có giấy tờ nhà mạng.

**Làm:** nối nhất quán `IvrOptionsValidator`, effective feature flags, `DispatchGate`, `IProductionCallGate` và kiểm tra Helm. Thay chặn tuyệt đối bằng cơ chế khởi tạo/kích hoạt theo release được duyệt, có audit; biến môi trường đơn lẻ không đủ cho phép gọi. Giữ admin mutation thông thường không được tự bật quyền gọi khách.

**Làm rõ đích theo mode:** lab vẫn chỉ alias thử đã duyệt; production cần token/task hợp lệ, policy, quyền release và ràng buộc đích thử/pilot được cấp. Không thêm mọi khách vào `LabDestinationAllowlist` hoặc gỡ kiểm tra cho mọi mode. Tách kiểm tra sẵn sàng cấu hình khỏi quyết định được phép gọi tại thời điểm dispatch; kiểm lại sát lệnh originate sau khi chuẩn bị audio.

**Scope:** [validator](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Configuration/IvrOptionsValidator.cs), [guardrails](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/FeatureFlags/FeatureFlagGuardrails.cs), [dispatch gate](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/FeatureFlags/DispatchGate.cs), [approval store](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/FeatureFlags/RuntimeGateApprovals.cs), [Helm helpers](C:/Users/Administrator/Desktop/ivr/deploy/helm/ivr/templates/_helpers.tpl).

**Đạt khi:** tại lần kiểm quyền cuối trước originate, thiếu/thu hồi approval, kill switch bật, sai môi trường, task bị thu hồi hoặc script chưa được duyệt đều chặn quay số. Thay đổi đến sau lần kiểm này xử lý theo mục 4.5. Approval phải áp đúng phạm vi môi trường/candidate theo hợp đồng release, không dùng một record còn hiệu lực bất kỳ để mở mọi bản triển khai. Có test host và chart cùng một ma trận, không chỉ test riêng một method.

**Cụ thể hóa:** `PostgresProductionCallGate` hiện truy vấn một approval còn hiệu lực theo loại, chưa nhận environment/candidate để đối chiếu. SIP-04 phải bổ sung hợp đồng/phạm vi đọc này; “đã có IProductionCallGate” chưa đóng việc. Kiểm từ chối quay số tại điểm kiểm quyền cuối; race đến sau điểm đó kiểm theo termination ở mục 4.5. Test chặn một task không được làm mất lượt quay của task khác hay tự kích hoạt vòng retry kỹ thuật.

### SIP-05 — Điều phối đồng thời và pool kênh trunk

**Phụ thuộc:** SIP-03 cho dispatch contract; có thể kiểm thử bằng dispatcher có rào đồng bộ, chưa cần nhà mạng.

**Làm:** xây bộ điều phối tác vụ có giới hạn theo mục 4.3; cấp pool 32 suất logic có provider/trunk ownership, migration/provisioning chạy lại không nhân đôi pool. Thêm controller ownership, hạn mức CPS và giảm tải. Dùng transaction/lease/fencing hiện có; bảo đảm một job không có hai attempt active do nhiều vòng scheduler tranh nhau.

**Scope:** [runtime](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/SchedulerRuntime.cs), [capacity và options](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs), [Postgres store](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs), [scheduler host](C:/Users/Administrator/Desktop/ivr/src/Ivr.Worker/Jobs/SchedulerJobHost.cs), [polling host](C:/Users/Administrator/Desktop/ivr/src/Ivr.Worker/Jobs/PollingJobHost.cs), provisioner trunk mới.

**Đạt khi:** bài test giữ 32 tác vụ cùng đang chạy; tác vụ 33 chờ và được chạy khi có suất trống. Hai worker tranh việc vẫn không vượt trần chung/không originate hai lần cho một attempt. Maintenance/liveness tiếp tục tiến triển trong lúc 32 cuộc đang chờ phím.

**Bổ sung:** bài “hai worker” phải chứng minh chỉ một bên được giữ ARI, bên còn lại chờ; không coi hai kết nối cùng application là cấu hình hỗ trợ. Kiểm task đến hạn khi đang đầy 32 suất được xử lý theo deadline, không gọi bù sau hạn; giảm hạn mức 32 xuống 8 thì chặn nhận thêm cho đến khi số đang giữ phù hợp, không hủy tùy tiện các cuộc hiện có. Cập nhật [Deployment worker](C:/Users/Administrator/Desktop/ivr/deploy/helm/ivr/templates/deployment-worker.yaml) và [scaling](C:/Users/Administrator/Desktop/ivr/deploy/helm/ivr/templates/scaling.yaml) theo quyền sở hữu controller đã chọn.

### SIP-06 — Vòng đời cuộc gọi, gia hạn và phục hồi

**Phụ thuộc:** SIP-02/03/05.

**Làm:** persist channel ID trước originate; map sự kiện theo attempt/channel/fencing; xử lý sự kiện lặp, đến trễ, DTMF đến trong lúc phát, socket đóng và `ApplicationReplaced`. Bổ sung lease renewal và đối soát channel đang sống khi worker restart. Không đánh đồng đóng WebSocket với nhà mạng đã cúp cuộc gọi.

**Shutdown:** ngừng claim, drain có thời hạn, yêu cầu kết thúc phần còn lại, xác nhận hoặc cách ly trước khi trả suất. Kill switch chặn cuộc mới; việc chấm dứt cuộc đang chạy theo cơ chế termination hiện có và phải đo thời gian hiệu lực.

**Scope:** gateway/dispatcher, [telephony dispatch store](C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/PostgresTelephonyDispatchStore.cs), scheduler store, event persistence và shutdown host.

**Đạt khi:** lỗi tại từng điểm “đã gửi originate/chưa nhận response”, “đã nghe/chưa ghi active”, “đã cúp/chưa ghi kết quả” không dẫn đến gọi lại mù hoặc thất lạc suất. Lease cũ không hoàn tất thay lease mới. Unknown đi review; có đường phục hồi có audit để trả suất sau khi chứng minh cuộc cũ đã hết.

**Bổ sung:** `HangupAsync`/`FailAsync` không được trả suất chỉ vì HTTP DELETE đã trả về hoặc catch bỏ qua lỗi hangup. Kiểm channel thực sự đã hết, hoặc giữ quarantine kèm liên kết cuộc cũ; kết quả nghiệp vụ và trạng thái giải phóng tài nguyên là hai điều cần đối soát riêng. Tránh normalizer sinh lần gọi mới trong lúc lần cũ chưa rõ. Mất node mà process cũ vẫn tới ARI được là bài thử bắt buộc của cơ chế bàn giao quyền, không chỉ test restart sạch.

### SIP-07 — Cấu hình Asterisk và nối một cuộc qua nhà mạng

**Chia hai mốc để lấy phản hồi nhà mạng sớm:**

- **SIP-07a — thử kết nối riêng một cuộc:** sau SIP-01 và khi có tuyến/đích thử được phép; dùng cấu hình/harness cô lập có đúng một suất, audio giả và đích cố định. Có thể kiểm signaling, nghe và DTMF trước khi xong resolver production hoặc 32 kênh. Không nới guard của `LAB-A` hiện tại, không đưa khách thật vào bài thử này.
- **SIP-07b — chạy xuyên suốt đường ứng dụng:** sau SIP-02/03/04/05/06 và SIP-07a; task thử đi qua dispatcher mới, lưu kết quả và callback. Bằng chứng 07a không thay 07b hoặc nghiệm thu M3.

**Làm:** tạo cấu hình triển khai riêng cho trunk, gồm PJSIP transport/auth/endpoint/AOR/identify theo cách nhà mạng cấp; firewall, public IP/NAT hoặc đường riêng, RTP, codec và DTMF. Kiểm đường audio mà Asterisk đọc được, không chỉ file tồn tại trong container worker. Đưa secret qua cơ chế quản lý secret, không commit tài khoản.

**Thử:** lần lượt gọi đích nội bộ được phép, nghe rõ nội dung, bấm `1/0`, không nghe, bận, từ chối, sai/không có phím và đích không hợp lệ. Đối chiếu mã SIP/hangup thực tế trước khi khóa bảng disposition. Kiểm early media không bị ghi thành nghe máy. Tên công ty phải kiểm trên điện thoại đích theo phạm vi hỗ trợ mà nhà mạng xác nhận.

**Đạt khi:** một cuộc xuyên suốt từ task thử tới callback ACK; nhật ký đã che số liên kết được attempt với cuộc của nhà mạng. Thiếu Brandname thì ghi rõ “thoại đã thông, tên hiển thị chưa đạt”, chưa đóng yêu cầu tên công ty.

**Kiểm tuyến bổ sung:** ARI auth/WebSocket, reachability SIP và đăng ký nếu nhà mạng dùng registration, media/RTP hai chiều, đầu số gọi ra và trạng thái chặn/quota. Codec/DTMF theo thỏa thuận thực tế; không mặc định cứ ping ARI thành công là route tốt. Chặn originate tới endpoint/đích ngoài phạm vi, không bật chuyển cuộc/ghi âm ngoài yêu cầu. Lỗi auth/quota/chặn tuyến phải dừng toàn trunk và báo vận hành; không suy thành “khách từ chối” hoặc đổi số để gọi tiếp. SIP status cần đối chiếu ngữ cảnh nhà mạng, không tự gắn mọi mã lỗi vào bảng kết quả khách hàng.

### SIP-08 — M3 và môi trường triển khai

**Phụ thuộc:** bắt đầu song song với SIP-02; đóng sau SIP-07b và đầu vào M3/Platform.

**Làm:** nối intake/callback theo contract hiện hành, xác thực, idempotency, deadline, token source và revocation. Xác minh M3 không giao đơn không cần gọi; ACK stale/đơn đã thay đổi không làm IVR cập nhật đơn thay M3. Kiểm audio approval và cấu hình production thực sự được nạp trong image chạy.

**Môi trường:** dev/staging giữ giới hạn gọi hiện hành; chạy tải mô phỏng trong mạng cô lập. Tuyến thật chỉ mở trong môi trường và đích đã được phép. Không đổi tên môi trường để né guard.

**Đạt khi:** M3 thật ở môi trường kiểm thử gửi task, IVR trả callback và M3 xử lý/ACK đúng cả `1`, `0`, stale, revoke và callback retry. Có SHA hai phía và bằng chứng runtime; M3 giả lập chỉ đóng test nội bộ.

### SIP-09 — Chứng minh 32 phiên, chất lượng và chi phí

**Phụ thuộc:** SIP-05/06/07/08 và nhà mạng cấp đủ dung lượng thử.

**Làm:** chạy ma trận mục 7, tăng `1/4/8/16/32`, có ít nhất một lượt giữ **32 cuộc đã kết nối chồng lấn liên tục ≥30 giây**, nhận DTMF riêng từng cuộc; lặp ba đợt có kiểm soát. Cấu hình lời thoại/timeout của bài thử phải cho phép giữ đủ thời gian này; không âm thầm đổi policy nghiệp vụ production.

**Sau đó:** chạy tải hỗn hợp tối thiểu hai giờ trên đích test được phép, gồm các trạng thái trả lời/không trả lời/lỗi đã thiết kế; tải tuân thủ CPS và giới hạn cước được thống nhất trước khi thực hiện. Đây là thời lượng kiểm thử đề xuất, không phải bằng chứng sẵn có hay SLA.

**Thiết kế bài tải:** để có 30s chồng lấn, cuộc trả lời đầu phải được giữ ít nhất `30s + khoảng cách thời điểm trả lời đầu/cuối` thực đo. CPS thấp làm thời gian này dài hơn; đừng dùng timeout lab 15s rồi kết luận trunk không đủ 32 kênh. Tách thử giữ kênh bằng audio test khỏi thử 32 nội dung đơn khác nhau trên pipeline audio thật; thử cả cache nóng/lạnh, mạng đích của ba nhà mạng, một chiều audio, DTMF trễ/mất và tuyến bị chặn.

**Đạt khi:** bằng chứng IVR, Asterisk và bản đối soát nhà mạng khớp; không vượt 32, không gọi trùng, không lẫn audio/phím/đơn; các sai lệch được giải thích. Đo thời gian chiếm suất thực tế để đánh giá đáp ứng hạn đơn, không suy từ 32 thành số đơn/giờ đã cam kết.

### SIP-10 — Bàn giao, pilot và mở vận hành

**Phụ thuộc:** SIP-01…09 đạt trong phạm vi tương ứng; các điều kiện release thực tế được giải quyết.

**Làm:** bàn giao cấu hình đã che secret, dashboard, cảnh báo, đối soát cước, quy trình xử lý unknown, restart, giảm dung lượng và kill switch. Pilot khách thật bắt đầu với hạn mức nhỏ được owner duyệt; tăng sau khi kiểm chất lượng, callback và cước. Mặc định không mở 32 ngay trong lần đầu phục vụ khách.

**Đạt khi:** có người vận hành, cách dừng, bằng chứng diễn tập khôi phục, phê duyệt release áp đúng bản triển khai và phạm vi pilot; danh sách tồn đọng nêu rõ tác động. Đóng ba khoảng trống mục 2 không tự đóng mọi release gate khác của dự án.

**Điều kiện dừng pilot:** có cuộc sai đích, trộn audio/DTMF giữa đơn, vượt trần gọi đã chốt, gọi trùng hoặc lộ dữ liệu thì khóa phát cuộc mới ngay; lỗi quota/chặn tuyến cũng dừng trunk. Ngưỡng chất lượng/độ trễ/cước lấy từ tệp tham số nghiệm thu SIP-01. Runbook phải chỉ rõ người xử lý unknown, thời gian phục hồi mục tiêu và cách rollback mục 4.6; không dùng nhiều số mới để tránh chặn của nhà mạng.

## 6. Đầu vào bên ngoài và phần làm được trong lúc chờ

| Đầu vào / nơi cung cấp | Cần cụ thể những gì | Chặn việc nào; có thể làm trước gì |
| --- | --- | --- |
| Nhà mạng / đơn vị cung cấp trunk | Xác nhận chỉ mua kết nối cho Asterisk mình quản lý; nhà mạng, số gọi ra, 32 phiên ra trên số đó, CPS, quota, tài liệu cấu hình và kỹ thuật viên test | Chặn SIP-07/09; SIP-02…06 vẫn làm với peer cô lập |
| Nhà mạng / đơn vị cung cấp trunk | Xác nhận sử dụng chỉ gọi ra xác nhận đơn, quy tắc gọi lại đã đối chiếu mục 3.1, chính sách cảnh báo/chặn và đầu mối xử lý chặn nhầm; có hỗ trợ đúng mô hình trunk trực tiếp hay chỉ API gọi hộ | Chặn chốt gói và mở lưu lượng thực; không suy từ “mua nhiều kênh” ra được miễn chống cuộc gọi rác |
| Nhà mạng / đầu mối hồ sơ công ty | Trạng thái hồ sơ, ngày sẵn sàng test thoại, ngày mở tên định danh, mạng/thiết bị được hỗ trợ hiển thị | Chặn nghiệm thu Brandname và lịch tuyến thật; không chặn viết code |
| Nhà mạng / kỹ thuật mạng | IP/FQDN và port SIP/RTP; IP-auth hay đăng ký; outbound proxy; codec; DTMF; định dạng số; caller ID/PAI được phép | Chặn chốt config thật; viết schema/validator và cấu hình mẫu trước |
| M3 / chủ dữ liệu | Nguồn phát và resolve token, endpoint hoặc cơ chế trao đổi đã chốt, quyền truy cập, TTL/binding, bộ task thử và callback ACK | Chặn resolver số thật và E2E; không yêu cầu nhà mạng tự xây vault thay M3 |
| Platform | Host/network/secret store/khóa token, PostgreSQL, nơi audio được Asterisk đọc, giám sát và đích triển khai | Chặn deploy tuyến thật; chuẩn bị image/config và bài kiểm tra cô lập trước |
| Owner / người đang giữ thẩm quyền | Đích thử, phạm vi pilot, lời thoại được duyệt, bằng chứng release; giải quyết ràng buộc kỹ thuật approval còn mở | Chặn gọi vượt phạm vi đã được phép; không chặn lập kế hoạch hoặc code có outbound thật bị khóa |
| Nhà mạng / kế toán | Phí khởi tạo, duy trì, số/Brandname/phiên, giá phút và block, VAT, cam kết tối thiểu, cách lấy đối soát và hạn mức test | Chặn chốt ngân sách test tải/vận hành; thêm bộ đo và mẫu đối soát trước |

Mốc cấp giấy không đồng nghĩa mốc nhà mạng đã mở đủ số, Brandname, IP và 32 phiên. Lấy **ngày có tuyến test sử dụng được** làm mốc phối hợp; không tự cộng một tháng chờ cố định vào lịch phát triển.

## 7. Ma trận kiểm thử và tiêu chí nghiệm thu

| Mã | Phép thử | Điều kiện đạt |
| --- | --- | --- |
| T01 — Composition | Khởi tạo host với MOCK, lab, production hợp lệ và các cấu hình thiếu/sai | Đúng adapter/resolver/mode; sai cấu hình không có originate; lab không mở rộng ngoài ý muốn |
| T02 — Token | TTL tại biên, sai task, replay, hết budget, restart, hai tiến trình tranh resolve | Bộ đếm nguyên tử, lý do từ chối đúng, không rò số/token; không cần reissue API |
| T03 — Call flow | Hai đơn khác nhau, lời thoại khác nhau, lần lượt phím `1/0` | Đúng đích, đúng audio, đúng attempt, đúng callback; không dùng alias lab cho khách |
| T04 — Signaling | Ringing/early media/answer/busy/reject/timeout/sai số | Mapping dựa trên quan sát tuyến; không biến reject thành hủy đơn hoặc lỗi mạng thành số sai |
| T05 — DTMF/audio | Phím sớm/lặp/sai/không phím, cúp lúc phát, thiếu file/audio lỗi | Theo policy đã chốt; không lẫn phiên, không xác nhận khi chưa có phản hồi hợp lệ |
| T06 — 32 nội bộ | 32 tác vụ bị giữ đồng thời, thêm tác vụ 33 và nhiều worker tranh việc | Active chung ≤32; đạt thực tế 32; số 33 chờ; maintenance không đứng; không gọi trùng |
| T07 — 32 qua trunk | 32 đích hoặc test endpoint nhà mạng được phép, giữ chồng lấn ≥30 giây; ba đợt | Chứng minh 32 cuộc outbound đã kết nối, DTMF riêng; không dùng 32 SIP dialog/Local leg nội bộ để thay số cuộc khách |
| T08 — Khôi phục | HTTP timeout sau originate, mất WebSocket, restart worker/Asterisk, DB mất kết nối, lease hết | Không gọi lại khi chưa rõ; không trả suất đang còn cuộc; stale fence bị từ chối; unknown có đường đối soát |
| T09 — Gate/termination | Bật kill switch khi đang đầy tải; thu hồi task/approval; sai env/script | Không phát thêm cuộc mới sau điểm kiểm quyền; xử lý cuộc đang chạy đúng hợp đồng; có đo độ trễ |
| T10 — M3 | Intake lặp, callback timeout/retry, stale/revoke, hai phản hồi cạnh tranh | M3 chỉ thực hiện một chuyển trạng thái hợp lệ; outbox hết hoặc có lỗi giải thích được, không mất kết quả |
| T11 — Soak/capacity | Tải hỗn hợp hai giờ, theo CPS; đo audio, DB, RAM/CPU, độ trễ hàng đợi và deadline | Không leak suất/tăng bộ nhớ không giới hạn; đạt các ngưỡng vận hành chốt trước bài chạy; báo rõ đơn trễ hạn |
| T12 — Identity/cước | Đối soát số gọi ra, tên công ty và các cuộc có cước | Tên đúng trong phạm vi nhà mạng cam kết; cuộc tính cước và block đối chiếu được; sai lệch có nguyên nhân |
| T13 — Giới hạn gọi | Hai lượt theo hợp đồng mục 3.1, technical retry, response đã có, nhiều đơn cùng đích, restart sau originate | Đếm số originate thực tế và số lần tới đích bên cạnh counted attempt; không có cuộc thứ ba ngoài giới hạn đã chốt, không gọi tiếp đơn đã kết thúc |
| T14 — Race/thời hạn | Giữ xử lý audio rồi revoke/bật kill switch/đổi approval; đóng giờ gọi hoặc hết TTL trước originate; cuộc dài hơn lease ban đầu | Không originate nếu bị chặn ở lần kiểm cuối; cuộc đã gửi được termination/reconcile; lease renewal không cho bắt đầu cuộc mới ngoài hạn |
| T15 — Bàn giao controller/rollback | Nâng cấp, force-delete hoặc mất node khi process cũ vẫn truy cập ARI; quay lại image cũ sau migration | Controller mới không gọi khi chưa cô lập bên cũ và đối soát; giữ trần pool và các attempt/outbox, không chuyển task thật sang lab |
| T16 — Lỗi toàn tuyến | ARI ping tốt nhưng SIP/RTP lỗi, hết quota hoặc nhà mạng chặn; callback M3 lỗi kéo dài | Đúng scope lỗi: dừng trunk hoặc retry callback tương ứng; không thử lại lần lượt qua 32 suất, không gọi lại khách vì mất ACK |

Các ngưỡng chất lượng/độ trễ của T07/T09/T11/T14 phải được chốt trong SIP-01 và gắn với run ID. Bài chức năng có mục tiêu không gọi sai đích, không trộn kết quả, không vượt trần và không gọi trùng; tỷ lệ nghe máy của khách thật không dùng để phán lỗi phần mềm nếu chưa tách nguyên nhân.

T06 chạy nội bộ chỉ chứng minh phần mềm đồng thời. **T07 mới đóng yêu cầu dung lượng tuyến thật**; nếu nhà mạng mới cấp một phiên thì ghi `VENDOR_INPUT_REQUIRED`, không sửa số trong báo cáo để thành 32.

Mỗi lượt kiểm thử lưu: run ID, SHA code/image/config đã che dữ liệu nhạy cảm, môi trường, dung lượng được cấp, thời gian bắt đầu/kết thúc, các attempt/channel ID opaque, đỉnh phiên, số thành công/lỗi/chờ, kết quả assertion và danh sách tồn đọng. Khi đối soát nguồn có số điện thoại, xử lý tại biên được phép rồi xuất bản đã loại số/token; không chép raw SIP/CDR vào evidence.

## 8. Thứ tự thực hiện và ước lượng

```text
SIP-01 → SIP-02 + SIP-03 → SIP-04
                 SIP-03 → SIP-05 → SIP-06
SIP-01 + tuyến test/đích được phép → SIP-07a (thử một cuộc riêng, làm sớm)
SIP-02…06 + SIP-07a → SIP-07b (xuyên suốt ứng dụng)
SIP-08 bắt đầu sớm với M3/Platform, đóng sau E2E
SIP-05/06/07b/08 + đủ 32 phiên → SIP-09 → SIP-10
```

Ước lượng ban đầu cùng phạm vi với báo cáo trình sếp là **19–30 ngày công**, một người làm chính quen repo, có đầu mối nhà mạng/M3/Platform hỗ trợ. **Sau rà soát, khoảng này chưa được ước lượng lại theo các phần bổ sung; không dùng làm lịch cam kết.** SIP-01 phải phân công và tính lại các việc về policy, bàn giao controller, schema/rollback, ngưỡng nghiệm thu; chỉ trừ phần đã có bằng chứng, không lấy số test lab làm công việc production đã xong.

| Nhóm công | Phạm vi kế hoạch tương ứng | Ngày công sơ bộ |
| --- | --- | --- |
| Đích gọi/token, adapter và tích hợp gate | SIP-02/03/04 | 3–5 |
| Điều phối đồng thời, lease, ARI và phục hồi | SIP-05/06 | 3–5 |
| M3 và môi trường triển khai | SIP-08 | 4–6 |
| Chuẩn bị/nối một trunk | SIP-01/07 | 2–3 |
| Giám sát và đối soát cuộc gọi/cước | Phần vận hành SIP-09/10 | 2–3 |
| Thử tải, lỗi, chạy liên tục và nghiệm thu | Phần kiểm thử SIP-09 | 4–6 |
| Tài liệu và bàn giao | Phần bàn giao SIP-10 | 1–2 |
| **Tổng** | **Không cộng lại lần nữa theo từng SIP-ID** | **19–30** |

Hai hàng đầu từng được cộng thành **6–10 ngày công cho code lõi**; sau lần rà soát này, chưa có cơ sở dùng con số đó như thời hạn chắc chắn đóng ba khoảng trống. Cần bóc lại phạm vi controller fencing, ledger bền vững, policy mới nếu có và activation workflow. Xây mới dịch vụ token phía M3 hoặc sửa quy trình release lớn phải được tính riêng, không ép vào con số cũ. Báo cáo trình sếp không bị sửa trong lượt review này.

Lịch giao thực tế phụ thuộc ngày xong code, ngày có tuyến, M3/Platform và phạm vi gọi được duyệt. Làm hồ sơ/kết nối/M3 song song với code; khi có đầu vào sớm thì giảm thời gian chờ, không bỏ bớt nghiệm thu.

## 9. Mốc báo cáo tiến độ và điều kiện đóng việc

| Mốc | Chỉ được báo đạt khi |
| --- | --- |
| **G1 — Đường production đã có code** | SIP-02/03/04 và T01…03/T13/T14 nội bộ đạt theo policy đã đối chiếu; đúng nhánh và gate vẫn chặn khi thiếu quyền |
| **G2 — Phần mềm chạy 32 tác vụ đồng thời** | SIP-05/06, T06/T08/T15 nội bộ đạt; có bằng chứng phục hồi và không vượt trần |
| **G3 — Một cuộc xuyên suốt qua nhà mạng** | SIP-07b đạt trên đích được phép; signaling/audio/DTMF/callback được đối chiếu, Brandname ghi trạng thái riêng. 07a chỉ là kết quả thử kết nối ban đầu |
| **G4 — 32 cuộc qua trunk đã kiểm chứng** | SIP-08/09 đạt, đủ T07 và các test liên quan T09…16; có nguồn đối soát từ tuyến và tệp ngưỡng đã chốt |
| **G5 — Sẵn sàng pilot khách thật** | SIP-10, quyền release, bộ lời thoại, phạm vi đích, điều kiện dừng và vận hành đã đủ; không còn blocker thuộc phạm vi pilot |

Checklist kết thúc toàn kế hoạch:

- [ ] Đích gọi được lấy đúng từ token của từng task trong biên telephony; không còn phụ thuộc `LAB-A` trên đường production.
- [ ] Adapter production có cấu hình/DI/gate đầy đủ; không cần mua nền tảng Auto Call khác để thay nghiệp vụ đã có.
- [ ] Có 32 cuộc qua trunk chồng lấn được đo, cuộc 33 chờ; không trùng đơn, không lẫn DTMF/audio và có giới hạn CPS.
- [ ] Lease, channel và callback phục hồi được sau lỗi; trường hợp chưa rõ được cách ly, không tự gọi lại mù.
- [ ] M3 nhận và xử lý kết quả đúng contract; IVR không cập nhật trạng thái đơn thay M3.
- [ ] Số gọi ra và tên công ty đạt phạm vi dịch vụ đã đăng ký; đối soát được cước thử.
- [ ] Có runbook, ngưỡng cảnh báo, quy trình dừng/khôi phục và bằng chứng release của đúng bản triển khai.
- [ ] Đã đối chiếu “hai lần/10 phút”, retry kỹ thuật và nhiều đơn cùng đích với M3/policy; đếm được số cuộc thực tế tới khách.
- [ ] Đã diễn tập mất controller, lỗi toàn trunk và rollback có dữ liệu đang dở; không để bên cũ và mới cùng originate.

## 10. Cách bắt đầu triển khai từ kế hoạch này

1. Kiểm lại `git status`/HEAD và công việc đang làm dở; bảo toàn báo cáo trình sếp. Repo chỉ làm trên `main`, không tạo branch/worktree nhánh mới và không thay hook.
2. Thực hiện SIP-01; đồng thời chuẩn bị test contract cho token và adapter. Không đợi giấy định danh mới bắt đầu phần mềm.
3. Trước mỗi thay đổi function/class/method, chạy GitNexus impact theo `AGENTS.md` và báo phạm vi ảnh hưởng; trước commit chạy detect changes theo cùng quy định. Lần lập kế hoạch này không có tool GitNexus callable và không sửa symbol nào; lúc triển khai phải xử lý khả năng truy cập công cụ theo quy định repo, không coi việc đọc source hôm nay là kết quả impact analysis.
4. Triển khai từng nhóm nhỏ theo mục 5, chạy unit/integration/contract hoặc chaos phù hợp với thay đổi. Dùng các suite hiện có: [telephony lab](C:/Users/Administrator/Desktop/ivr/tests/Ivr.UnitTests/Telephony/AsteriskLabTelephonyTests.cs), [token](C:/Users/Administrator/Desktop/ivr/tests/Ivr.UnitTests/Telephony/DialTokenResolveLedgerTests.cs), [scheduler persistence](C:/Users/Administrator/Desktop/ivr/tests/Ivr.IntegrationTests/SchedulerPersistenceTests.cs), [gate approval](C:/Users/Administrator/Desktop/ivr/tests/Ivr.IntegrationTests/RuntimeGateApprovalTests.cs), [termination](C:/Users/Administrator/Desktop/ivr/tests/chaos/CallTerminationScenario.cs); thêm test vào đúng lớp thay vì chỉ kiểm cấu hình có giá trị 32.
5. Ghi evidence và trạng thái theo mục 9 sau mỗi mốc. Thiếu tuyến, giấy tờ hoặc quyền môi trường thì giữ `VENDOR_INPUT_REQUIRED`/`BLOCKED_EXTERNAL` cho mốc phụ thuộc; tiếp tục phần code/kiểm thử cô lập còn làm được.

**Ưu tiên triển khai:** hoàn thiện Mobile SIP Trunk một nhà mạng trên nền IVR hiện có; kết nối nhỏ để lấy dữ liệu signaling thật, sau đó chứng minh điều phối và dung lượng 32 phiên trước khi mở rộng pilot.

## 11. Kết quả rà soát lần 2 — 15/09/2026

Đã đọc toàn bộ plan, đối chiếu source và quyết định liên quan tại baseline đầu tài liệu. Các dòng dưới là **lỗi/thiếu trong kế hoạch đã được hiệu chỉnh**, không phải bằng chứng code đã được sửa hoặc release đã đạt.

| Mức | Điểm cần sửa trong bản đầu | Hiệu chỉnh ở bản này |
| --- | --- | --- |
| P1 | Chưa đối chiếu mô tả hai lần/10 phút; customer attempt khác số cuộc thực và retry có budget riêng | Mục 3.1, SIP-01, T13 và checklist đóng việc |
| P1 | Khóa DB chưa đủ ngăn controller cũ tiếp tục tác động ARI; thiếu trình tự deploy và rollback | Mục 4.3/4.6, SIP-05/06/10, T15 |
| P1 | Revoke/deadline có thể đổi trong khi tạo audio; lease mặc định ngắn hơn tổng thời lượng có thể xảy ra | Mục 4.5, SIP-04/06, T14; nêu rõ race còn lại thay vì hứa ngăn tuyệt đối |
| P1 | Ping ARI chưa chứng minh tuyến khỏe; chưa có dừng toàn pool khi auth/quota/chặn tuyến lỗi | Mục 2/4.3, SIP-07/10, T16; bổ sung xác nhận kịch bản gọi ra với nhà mạng |
| P2 | Tiêu chí “token đúng gọi hai đích” có thể hiểu sai là một token đổi đích; vòng đời token/schema chưa đủ cụ thể | SIP-02 làm rõ hai task/token khác nhau; mục 4.6 bổ sung khóa, rotation, tách dữ liệu lab |
| P2 | Thử một cuộc bị xếp sau toàn bộ phần đồng thời/phục hồi; tên mốc M3 dễ lẫn Module 3 | Tách SIP-07a làm sớm/07b E2E; đổi tên mốc thành G1…G5 |
| P2 | Bài 32 cuộc thiếu cách giữ đủ thời gian theo CPS, tải audio khác nhau và ngưỡng đo trước chạy | SIP-01/09 và T11/T14; không dùng một file test để chứng minh toàn pipeline |
| P2 | Ước lượng 6–10 ngày code và 19–30 ngày tổng còn quá sơ bộ cho các việc vừa làm rõ | Mục 8 giữ nguồn số ban đầu, đánh dấu phải lập lại ước lượng sau SIP-01 |

Phạm vi chỉnh sửa của lượt này: **chỉ file plan này**. Không chỉnh báo cáo so sánh, tracker, source, runtime flags, migration hay dữ liệu. Không thực hiện cuộc gọi, test tải hoặc deploy; kiểm tra lần này là review tài liệu và đối chiếu source.
