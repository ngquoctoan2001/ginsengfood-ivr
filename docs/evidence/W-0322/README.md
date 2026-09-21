# W-0322 — Rà nghiệm thu 13 việc P2/P3

Ngày: `2026-09-21` · Người rà: Codex · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Kết luận: 8 việc P2 đủ để đề nghị nghiệm thu trong phạm vi phần mềm/MOCK;
W-0019 thiếu đối chiếu chính thức cho hai TestId đã retire; 4 việc P3 thiếu hồ sơ kết thúc
phạm vi đã chuyển sang Module 3.** Không đề nghị nghiệm thu
UI như sản phẩm còn tồn tại. Không phát hiện việc phải bổ sung runtime trong **phạm vi MOCK
của chín việc P2** để trình owner; các việc tích hợp thật, production và UI M3 vẫn còn riêng.

Đây là báo cáo rà soát, không phải chữ ký nghiệm thu. Giữ nguyên trạng thái của cả 13 việc.
Không sửa bằng chứng lịch sử, không triển khai các hành động đề xuất trong báo cáo này.

## 1. Mốc và cách kiểm

- Mốc đọc source, tracker, bằng chứng và lịch sử: `423b5732062b6ee77c6d8e875473683e4c41ed75`.
- Gói đã chạy toàn solution rồi full sweep: `bd9ea5c64b6a6b4a1a1f34c7043f80cfea25e08f`,
  `1151/1151` test, `42/42` gate chạy, `22` mục được manifest loại khỏi sweep.
  Xem [W-0319 §6](../W-0319/README.md#6-kiểm-chứng-tại-commit-xác-định--21092026).
- Đã kiểm lại gói bằng consumer trên chính checkout `bd9ea5c`. SHA-256 manifest:
  `5521d7d6b48d522797a823a4f4fc9068e8aa0f2416a6382ac85f5e2f72521bf4`.
  Gói nằm ở `.artifacts/w0319-acceptance-bd9ea5c/`; không nằm trong Git.
- Giữa hai commit chỉ đổi README W-0319, danh sách nghiệm thu và tracker. Source, test,
  manifest gate và 13 gói gốc không đổi. **Test chứng minh `bd9ea5c`, không được đổi nhãn
  thành lượt chạy tại `423b573` hay commit của báo cáo này.** Không chạy lại solution trong lượt rà.
- Đọc 13 README, các file đính kèm của chúng, 13 prompt và **nguyên cột Residual**, không dùng
  câu bị cắt trong danh sách tự động. Đối chiếu quyết định, commit và code sau mỗi việc.
- [Bảng đối chiếu máy đọc được](test-crosscheck.json) giữ nguyên Residual, hash Git blob của
  từng file bằng chứng, TestId hiện hành và ánh xạ class/method sang TRX. Mọi định nghĩa đã chọn
  phải có kết quả Passed; đọc mọi dòng traceability trùng TestId, không chỉ dòng cuối.
  Đây là phụ lục kiểm tra, không thay `acceptance-run.json`.
- [Đối chiếu commit](commit-crosscheck.txt) giữ SHA đầy đủ, ngày, subject của 23 commit được viện dẫn;
  đã kiểm tất cả nằm trong lịch sử dẫn đến mốc rà.
- Loại khỏi kết luận WIP W-0320/W-0321 và các file chưa commit khác. GitNexus đã cập nhật chỉ mục;
  query vẫn cảnh báo thiếu FTS, nên dùng context/source/Git để xác minh, không suy từ graph.

Phân loại áp vào **phạm vi hiện còn được giao cho IVR**. “Thiếu bằng chứng” của P3 là thiếu
biên bản kết thúc từng work/chuyển giao tương ứng; bằng chứng việc xoá UI đã có ở W-0253.
Nó không có nghĩa phải dựng lại UI để có test xanh. Nếu cần nghiệm thu sản phẩm UI của M3,
phải có commit, test và chủ thể nghiệm thu bên M3; repo này không chứng minh được phần đó.

## 2. Danh sách quyết định

| Việc | Phạm vi | Phân loại | Hành động ngay / người phụ trách |
| --- | --- | --- | --- |
| W-0018 / P2-1 | Intake | Đủ để đề nghị nghiệm thu | Owner M8 duyệt phần mềm/MOCK; giữ intake producer/auth thật trong gói tích hợp M3 |
| W-0019 / P2-2 | Eligibility | Thiếu bằng chứng | Codex lập addendum chính thức cho hai TestId đã retire và test thay thế; sửa C2 để xét đúng retirement trước khi trình Owner M8 |
| W-0020 / P2-3 | Scheduler/policy | Đủ để đề nghị nghiệm thu | Owner M8 duyệt phần local; giữ phép đo SIM/capacity ở W-0008/W-0048 |
| W-0021 / P2-4 | Mock SIM dispatch | Đủ để đề nghị nghiệm thu | Owner M8 duyệt mock adapter; không dùng kết quả này nghiệm thu nhà mạng |
| W-0022 / P2-5 | Normalizer | Đủ để đề nghị nghiệm thu | Owner M8 duyệt mapping local; bảng tín hiệu nhà mạng cần lượt đo riêng |
| W-0023 / P2-6 | Callback/outbox | Đủ để đề nghị nghiệm thu | Owner M8 duyệt fake Target/GH compatibility; M3 trả endpoint/ACK/auth và chạy E2E chung |
| W-0024 / P2-7 | Script lifecycle/privacy | Đủ để đề nghị nghiệm thu | Owner M8 duyệt lifecycle MOCK; duyệt script production phải đáp ứng luật actor hiện hành |
| W-0065 / P2-8 | Internal/admin API | Đủ để đề nghị nghiệm thu | Owner M8 duyệt API local; M3 sở hữu identity/role→tier/BFF và tích hợp thật |
| W-0066 / P2-9 | TTS boundary/cache/privacy | Đủ để đề nghị nghiệm thu | Owner M8 duyệt port/fake/cache; kiểm chứng image/model VieNeu hiện hành là việc riêng |
| W-0025 / P3-1 | UI foundation cũ | Thiếu bằng chứng | Codex lập dòng đóng scope theo W-0253; Owner M8 duyệt cách kết thúc; M3 nhận foundation/identity |
| W-0026 / P3-2 | Dashboard/call log cũ | Thiếu bằng chứng | Codex chuyển yêu cầu còn áp dụng sang phiếu M3, ghi rõ auto-refresh bắt buộc/CSV tuỳ chọn; Owner M8 duyệt đóng scope |
| W-0027 / P3-3 | Config/integration UI cũ | Thiếu bằng chứng | Codex ghi phần thay thế và ngoại lệ replay/role; Owner M8 duyệt đóng scope; M3 xác nhận UI thay thế |
| W-0028 / P3-4 | Reporting UI cũ | Thiếu bằng chứng | Codex tách UI đã xoá khỏi BI backend còn sống; Owner M8 duyệt đóng scope; M3 cung cấp UI/E2E nếu nhận delivery |

Tổng: **8 / 5 / 0** theo thứ tự ba loại user yêu cầu. Số `0` không có nghĩa dự án đã xong;
nó chỉ nói không tìm thấy phần runtime MOCK bắt buộc còn thiếu trong chín work P2 này.
Không có cá nhân M3 nhận việc được xác nhận trong nguồn đã đọc: “Owner M3” là vai trò cần
xác định khi bàn giao, không phải tên người được tự gán hay bằng chứng đã gửi/đã nhận.
Owner M8/release owner ở đây là **Toàn**, theo tracker §1. Chỉ owner quyết định ACCEPTED.

## 3. Phát hiện ảnh hưởng việc nghiệm thu

### F1 — Cao: C2 bỏ lọt test UI đã xoá

[citedTestIds](../../../deploy/ci/scripts/acceptance-batches.mjs) chỉ nhận ID đã biết hoặc tiền tố
còn tồn tại trong traceability. Khi cả nhóm UI biến mất, `UT-UI-*` và `E2E-UI-*` bị bỏ qua.
[judge](../../../deploy/ci/scripts/acceptance-batches.mjs) coi không có ID là C2 đạt nếu gói chạy hợp lệ.
Tái hiện trực tiếp với 13 README và traceability tại mốc rà:

| Work P3 | ID mà công cụ thực sự xét | Điều còn thiếu |
| --- | --- | --- |
| W-0025 | 0 | 5 test foundation UI trong prompt/evidence không được xét |
| W-0026 | Chỉ `IT-ADMIN-READ-01` | 4 test UI bắt buộc không được xét; backend read không chứng minh UI |
| W-0027 | 0 | 4 UT UI và E2E review thay replay không được xét |
| W-0028 | Chỉ 4 `BI-*` | 5 test reporting UI không được xét; BI backend không chứng minh UI |

`c0e6609` (09/09, W-0253) xoá `admin-ui/` và CI UI theo quyết định owner.
[W-0253](../W-0253/README.md) và [prompt index](../../../prompt/00-index.md#phase-3--nextjs-admin-đã-xoá-sản-phẩm-giữ-prompt-làm-lịch-sử)
đã ghi console thuộc M3; bốn dòng tracker vẫn TESTS_PASS, nên tiếp tục lọt vào lô P3.
**XEM không chứng minh bốn sản phẩm UI hiện còn và chạy được.**

Hành động triển khai riêng, **Codex phụ trách**: sửa extractor nhận diện TestId không phụ thuộc
tập tiền tố còn sống; phân biệt test đã retire có quyết định với ID gõ sai/mất không rõ lý do;
không cho “không nêu TestId” thay bằng chứng kiểm thử bắt buộc; kiểm nguồn test đính kèm được
khai báo tường minh và phân biệt runner .NET/UI/shell. Thêm regression cho nhóm tiền tố bị xoá,
README chỉ có backend test nhưng work là UI, README không có ID nhưng có test-report liên kết,
và quyết định retire hợp lệ. Sau sửa, thu gói tại commit mới rồi sinh danh sách mới.
Đây là phát hiện của lượt audit, chưa sửa source trong W-0322; không làm mất hiệu lực việc W-0319
đã kiểm đúng SHA/hash và đủ gate của manifest.

### F2 — Vừa: ghi chú lịch sử đang trộn ba thứ khác nhau

Các câu “chờ P2-2/P2-4/P2-6”, “OD-V1-15/19 còn mở”, “chưa có pipeline P10-4”,
“chưa cấp runtime-gate permission” đã có thay đổi sau đó. Ngược lại, Target DRAFT,
chưa có producer/consumer M3 và chưa đo nhà mạng/capacity thật vẫn còn.
[Register](../../../specs/_review/open-decisions-register.md) ở `9dc5479` (W-0304) sửa rõ:
OD-V1-01/02/03/05 chỉ là **M8_POSITION_SIGNED / M3_NOT_RECEIVED**.
Không dùng chữ ký M8 thay chữ ký M3.

Các giới hạn như không chuyển trạng thái đơn, không SMS, recording OFF, fail-closed khi chưa
cấu hình, dùng dữ liệu fake là chủ ý thiết kế; không coi chúng là TODO cần mở cho hết Residual.

## 4. Đối chiếu từng việc

### W-0018 — P2-1 intake

**Đủ để đề nghị nghiệm thu phần mềm/MOCK.** [Bằng chứng gốc](../W-0018/README.md).

**Residual nguyên văn tại mốc rà:**

> owner/reviewer acceptance, real Sales/auth, LAB/PROD script/key/SIM and P2-2 eligibility remain open

- “P2-2 eligibility” đã hết hiệu lực: `6e0f9d3` bổ sung eligibility; `d23ab98` bổ sung capacity
  scheduler. [TaskIntakeService](../../../src/Ivr.Infrastructure/Intake/TaskIntakeService.cs)
  và [EligibilityService](../../../src/Ivr.Api/Application/EligibilityService.cs) hiện có luồng tương ứng.
- Auth/business matrix phía M8 đã có quyết định; auth credential/producer M3 thật vẫn chưa có.
  `92091a1` và `ffa0284` tiếp tục siết intake ngoài giờ và TTL token, không mở real-call gate.
- Hai TestId DB trong README và nhóm intake hiện hành đã đối chiếu TRX ở phụ lục. `CT-CI-06`
  trong log cũ là shell gate PII, không phải .NET TestId mất: full sweep có `selftest-pii.sh` PASS.
- **Còn làm:** Owner M8 xét duyệt local. Owner M3 cung cấp producer revision/payload/contract;
  Owner M8 phụ trách đầu mối credential/key và điều kiện lab, IVR dev chạy shared E2E khi đủ đầu vào.
  LAB/PROD là việc ngoài phạm vi nghiệm thu intake MOCK.

### W-0019 — P2-2 eligibility

**Thiếu bằng chứng đối chiếu test chính thức để đạt C2 nghiêm ngặt.** [Bằng chứng gốc](../W-0019/README.md).

**Residual nguyên văn tại mốc rà:**

> P2-3 owns real capacity/scheduler; no direct Ops/CRM; trust-skip off; real Sales/SIM/LAB/PROD NOT_RUN

- Capacity placeholder đã được P2-3 thay ở `d23ab98`; còn [SchedulerEligibilityCapacityProvider](../../../src/Ivr.Api/Application/EligibilityService.cs).
- “trust-skip off” không còn mô tả đúng: `6760ba6` (W-0123/W-0124, OD-18) bỏ quyền IVR quyết
  định skip; M3 quyết định có gọi. [Prompt P2-2](../../../prompt/phase-2-core-runtime/P2-2-eligibility-blockers.md)
  đánh dấu phần cũ superseded. Sellable check cũng không còn là predicate độc lập của IVR.
- README cũ nêu `UT-ELIG-BLOCK-01` và `UT-ELIG-TRUST-03`; hai ID không còn hiện hành.
  Đây là hành vi được thay có quyết định, không yêu cầu phục hồi test cho logic đã bỏ.
  Sáu ID còn sống cùng bộ `UT/IT/CT-M3-AUTHORITY-*` đã đối chiếu TRX; xem
  [W-0123](../W-0123/README.md), [W-0124](../W-0124/README.md),
  [EligibilityRules](../../../src/Ivr.Domain/Policies/EligibilityRules.cs). Tuy nhiên README vẫn nêu
  hai ID cũ như PASS và C2 đang bỏ qua chúng. Chưa có bản đối chiếu chính thức gắn từng ID cũ
  với quyết định retire và nhóm test thay thế được bộ kiểm tra xét. Không gọi trường hợp này
  là đạt C2 chỉ vì source mới có test xanh.
- **Việc thiếu / owner:** Codex bổ sung addendum vào gói W-0019, giữ nguyên kết quả lịch sử,
  ghi rõ ID nào là lịch sử/superseded, source/commit thay thế và kết quả test tại commit được xét;
  sửa C2 theo F1, kiểm lại rồi trình Owner M8. Báo cáo này đã nêu căn cứ nhưng chưa sửa gói cũ
  hoặc bộ kiểm tra. Không cần implement lại sellable predicate/trust-skip.
- Không đọc Ops/CRM trực tiếp vẫn là ranh giới đúng. **Còn việc tích hợp riêng:** Owner M8 duyệt scope mới;
  Owner M3 cung cấp dữ liệu eligibility/voice-contact có nguồn, revision và shared test;
  IVR dev kiểm fail-closed bằng payload thật trong sandbox khi được cấp. SIM/LAB/PROD vẫn riêng.

### W-0020 — P2-3 scheduler/attempt policy

**Đủ để đề nghị nghiệm thu local.** [Bằng chứng gốc](../W-0020/README.md).

**Residual nguyên văn tại mốc rà:**

> scheduler flag off + dispatch gateway unavailable until P2-4; policy owner/LAB/1 SIM/32 eSIM/PROD NOT_RUN; REAL_CUSTOMER_CALL_ALLOWED=NO

- “gateway unavailable until P2-4” đã được `ec459a3` giải quyết. Cấu hình MOCK đăng ký
  `MockSchedulerDispatchGateway` trong [SchedulerCapacity](../../../src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs).
  Nhánh unavailable vẫn tồn tại để chặn cấu hình chưa đủ, không phải chưa implement gateway.
- “policy owner” đã hết hiệu lực ở phía M8: `5cffea3` thêm policy ký `gh-247-prod-v1`,
  `6993e3f` cập nhật cuối giờ gọi 21:08; [AttemptPolicyRegistries](../../../src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs)
  giữ riêng policy candidate và owner-approved. 18 ID gốc + 5 ID signed-policy đều Passed.
- Flag off mặc định là cấu hình an toàn. 1 SIM/32 eSIM mô phỏng không chứng minh capacity thật.
- **Còn làm:** Owner M8 duyệt local; Owner M8 cung cấp hạ tầng/lịch đo và đầu mối nhà mạng;
  IVR dev chạy W-0048/W-0008, lưu thời lượng, DTMF/disposition và tải thực;
  Owner M3 xác nhận policy/version/cutover của producer. Không cần bật cờ để nghiệm thu slice này.

### W-0021 — P2-4 mock SIM adapter

**Đủ để đề nghị nghiệm thu mock adapter.** [Bằng chứng gốc](../W-0021/README.md).

**Residual nguyên văn tại mốc rà:**

> MOCK-only; no live TTS/real provider/egress/customer call; REAL_CUSTOMER_CALL_ALLOWED=NO

- Toàn câu là giới hạn phạm vi gốc, không phải lỗi thiếu của P2-4. 16 TestId còn sống đều Passed.
- Phụ thuộc đọc tiếng đã tiến triển: `c93dace` tạo TTS boundary/cache; `9af20d3` giới hạn bộ đọc
  thật về VieNeu loopback. Điều đó không biến mock dispatch thành bằng chứng nhà mạng.
- **Còn làm:** Owner M8 duyệt mock; phần adapter thật do IVR dev kiểm với đầu vào/hạ tầng
  Owner M8 và nhà cung cấp cung cấp trong W-0008/W-0048. Giữ no-egress/real-call NO trong MOCK.

### W-0022 — P2-5 normalizer

**Đủ để đề nghị nghiệm thu mapping/persist local.** [Bằng chứng gốc](../W-0022/README.md).

**Residual nguyên văn tại mốc rà:**

> MOCK/local only; normalization disabled by default; production retry/vendor mapping owner approval; P2-6 callback pending

- “P2-6 callback pending” đã đóng bằng `2412cf6`: snapshot/outbox/dispatcher và fake Target
  có mặt. 15 TestId normalization đã đối chiếu Passed, gồm persist/concurrency và technical
  không tính thành customer no-answer.
- Chính sách attempt phía M8 có bản ký sau ở `5cffea3`; điều này không ký hộ mapping tín hiệu
  của modem/nhà mạng. OD-V1-09 vẫn HALF_SIGNED ở phần cần đo thật.
- Normalization disabled mặc định và MOCK/local là giới hạn triển khai, không thiếu hàm.
- **Còn làm:** Owner M8 duyệt local. IVR dev + đầu mối nhà mạng do Owner M8 chỉ định thu
  disposition/DTMF thật và đối chiếu DT-02; ghi mọi sai khác trước khi bật môi trường lab thật.

### W-0023 — P2-6 callback/outbox

**Đủ để đề nghị nghiệm thu fake Target và GH compatibility.** [Bằng chứng gốc](../W-0023/README.md).

**Residual nguyên văn tại mốc rà:**

> local/MOCK only; callbacks disabled; Target contract DRAFT; W-0005/W-0006, real Sales/auth/LAB/PROD remain blocked; no notification

- README không liệt kê TestId trực tiếp, nên không dựa vào C2 “không nêu TestId”. Lượt rà này
  bổ sung ánh xạ toàn bộ `UT-CALLBACK-*`/`IT-CALLBACK-*` hiện hành sang TRX, gồm ACK, retry
  giữ body/key, 24/7 không lọt GH compatibility và atomic outbox; chi tiết trong phụ lục.
- [CallbackDeliveryOptions](../../../src/Ivr.Infrastructure/Callbacks/CallbackDeliveryOptions.cs)
  vẫn disabled mặc định và từ chối enabled real TARGET_V1; đây là chặn có chủ đích khi auth thật
  chưa được nối. “Target DRAFT”, W-0005/W-0006 và chưa shared E2E vẫn có hiệu lực.
- OD-V1-07 đã chốt profile JWT; **profile được chọn không đồng nghĩa adapter/auth thật đã
  triển khai và kiểm chứng**. Không notification là yêu cầu giữ nguyên.
- **Còn làm ngoài P2 MOCK:** Owner M3 cung cấp consumer revision/OpenAPI, endpoint sandbox,
  ACK/version/idempotency cases; Owner M8 phụ trách credential/JWKS/network; IVR dev triển khai
  và kiểm chứng auth thật ở work tích hợp tương ứng, chạy shared E2E rồi mới xét mở delivery.

### W-0024 — P2-7 script/content

**Đủ để đề nghị nghiệm thu lifecycle/privacy MOCK.** [README](../W-0024/README.md),
[fixture](../W-0024/approved-mock-fixture.json), [privacy report](../W-0024/privacy-test-report.md).

**Residual nguyên văn tại mốc rà:**

> GitHub main pushed; GitLab main push BLOCKED_EXTERNAL by protected-branch rule; MOCK fixture only; LAB/real Sales/PROD NOT_RUN; OD-V1-15 + W-0003 remain open

- OD-V1-15 đã CLOSED theo chữ ký 05/09 (`4baad09`, record W-0194); không còn đợi chọn whitelist.
  W-0003 vẫn cần schema/sample producer M3 thật; hai việc không đồng nhất.
- GitLab push-block cũ không còn là mô tả hiện hành: OD-V1-21 đã ghi sửa pushurl ở W-0121,
  hạ tầng đã có và owner chốt giới hạn approval tại `a3688c7`. Không kiểm remote trực tiếp trong
  lượt rà này, cũng không dùng các thông tin đó chứng nhận hosted CI của `bd9ea5c`.
- Preview có tên khách là fixture lịch sử; `f7c9be9` (W-0104) đã nghiệm thu lời chào chung.
  Không chép fixture cũ làm script hiện hành. 10 TestId gốc có kết quả Passed.
- **Còn làm:** Owner M8 duyệt local; Owner M3 cung cấp speech-safe payload thực.
  Owner M8 bố trí actor duyệt script production hoặc ban hành quyết định đổi luật rõ ràng;
  code hiện vẫn đòi ba actor khác nhau. OD-V1-11 CLOSED phần chính sách không tự tháo luật này.
  IVR dev chỉ kiểm/thi hành luật đã được chốt; không giả danh người duyệt còn thiếu.

### W-0065 — P2-8 internal/admin API

**Đủ để đề nghị nghiệm thu API local/MOCK.** [Bằng chứng gốc và các sample](../W-0065/README.md).

**Residual nguyên văn tại mốc rà:**

> local/MOCK only; Target contract DRAFT; hosted GitLab/reviewer/real Sales-auth/SIM-eSIM/LAB/PROD `NOT_RUN`; `REAL_CUSTOMER_CALL_ALLOWED=NO`; no order transition/Sales write/SMS/call thật

- 10 TestId bắt buộc về authz/idempotency/audit/PII/queue/retry/review/SIM/OAS đã đối chiếu Passed.
  Sample 403, 409 và audit là dữ liệu tổng hợp lịch sử; coverage 88,80% là số lịch sử đã sửa,
  không phải đo coverage lại hôm nay.
- `a09f062` bỏ console account khỏi hợp đồng/code; `0dcb145` chuyển console thành reference,
  sau đó `c0e6609` xoá UI. API vẫn còn; role/identity thuộc M3, IVR enforce service tier.
  Không suy việc UI bị xoá thành API bị xoá.
- Target DRAFT, credential thật và shared E2E vẫn mở. Hosted GitLab của đúng mốc test vẫn
  chưa có trong gói local này; P2-8 cho phép ghi NOT_RUN. Không order transition/Sales write/SMS
  là giới hạn đúng, không phải chức năng cần bổ sung.
- **Còn làm:** Owner M8 duyệt local; Owner M3 ký role→tier/BFF/actor propagation và cung cấp
  consumer test; Owner M8 cấp secret reference/đầu mối hạ tầng; IVR dev chạy shared authz/E2E
  và đính hosted pipeline đúng SHA khi thực hiện work tích hợp/release.

### W-0066 — P2-9 TTS boundary

**Đủ để đề nghị nghiệm thu port/fake/cache/privacy.** [README](../W-0066/README.md),
[test-report](../W-0066/test-report.md), [metadata audio fake](../W-0066/audio-metadata.json).

**Residual nguyên văn tại mốc rà:**

> prereq của W-0048; `OD-V1-15`, `OD-V1-19`, `W-0008` vẫn mở; lab/vendor/pronunciation NOT_RUN; MOCK no-egress, real calls NO

- Bộ sinh chỉ đọc README nên tìm được 0 ID. Đọc test-report liên kết cho đủ **9 ID**, cả 9
  đã đối chiếu source class/method với TRX Passed; không kết luận TTS thiếu test.
- OD-V1-15 đã chốt whitelist; OD-V1-19/OD-VOICE-01 đã chốt VieNeu tự host ngày 17/09 và
  `9af20d3` (W-0315) ép loopback cho lab lẫn production. Không còn việc “chọn vendor”.
- “pronunciation NOT_RUN” không thể áp trùm mọi bằng chứng: [manifest giọng W-0122](../W-0122/voice-acceptance-manifest.json)
  đã có owner nghe/chọn giọng. Nhưng nó không chứng minh image/model runtime hiện hành đọc
  được toàn câu và qua kênh đích; còn phải chạy đúng bài acceptance tương ứng.
- W-0008 và điều kiện lab thật vẫn riêng. MOCK no-egress/real-call NO vẫn đúng.
- **Còn làm ngoài P2 MOCK:** IVR dev hoàn tất smoke với model ghim và nghe qua media path hiện
  hành; Owner M8 ký phần nghe/chấp nhận CVE/quyền model, cung cấp máy đích và kho nội bộ theo
  S2/S5. W-0320/W-0321 đang WIP không được dùng như kết quả đã đạt ở mốc này.
  Không đem coverage-summary 94,7% cũ làm coverage hiện tại; test-report đã ghi bản sửa 88,80%.

### W-0025 — P3-1 UI foundation

**Thiếu bằng chứng kết thúc work theo scope mới; không đề nghị ACCEPTED cho UI hiện hành.**
[README và capture lịch sử](../W-0025/README.md).

**Residual nguyên văn tại mốc rà:**

> MOCK-only; component library vẫn `NEED_CONFIRMATION` (chưa chọn thư viện nào); SSO/JWT thật `BLOCKED_EXTERNAL` (G-AUTH/W-0006) và client fail-closed ngoài MOCK; `IVR_RUNTIME_GATE_ADMIN` không cấp cho role nào (OD-V1-20); hosted GitLab evidence `NOT_RUN`; màn nghiệp vụ thuộc W-0026/W-0027; visual/a11y QA thuộc W-0039; không có nút chuyển trạng thái đơn (D-02); `REAL_CUSTOMER_CALL_ALLOWED=NO`

- Component library, SSO/session frontend và visual/a11y không còn là backlog triển khai UI của
  IVR sau `0dcb145` rồi `c0e6609`; M3 sở hữu console/identity. W-0026/W-0027 từng bổ sung màn,
  sau đó cùng bị retire; không tiếp tục coi chúng là prerequisite chưa được viết.
- “runtime permission không cấp role nào” đã sai từ `bed051c`; sau đổi authority còn phải đọc
  service tier hiện hành. OD-V1-20 đã chốt, nhưng bốn mắt chưa tự được đáp ứng.
- Capture 15/08 chứng minh redirect/cookie/RBAC/correlation và fake stack lúc đó; **không phải
  screenshot/visual/a11y, không phải bằng chứng UI tại `bd9ea5c`**. Cả 5 test UI gốc đã retire.
- MOCK-only, D-02 và real-call NO là giới hạn giữ. Hosted pipeline mới không chứng minh UI đã xoá.
- **Việc thiếu / owner:** Codex lập biên bản W-0025 liên kết quyết định W-0253, code/test retired
  và phần chuyển M3; Owner M8 quyết định trạng thái kết thúc (đề nghị CANCELLED vì deliverable
  bị bỏ, hoặc N/A nếu quy ước owner chọn). Owner M3 xác nhận nhận foundation/identity và đưa
  commit/E2E riêng nếu muốn nghiệm thu UI M3. Không sửa bằng chứng cũ để tạo chữ ký mới.

### W-0026 — P3-2 dashboard/log/detail

**Thiếu bằng chứng kết thúc work/chuyển giao UI.** [README và capture](../W-0026/README.md).

**Residual nguyên văn tại mốc rà:**

> không có control chuyển trạng thái đơn (D-02); chỉ dữ liệu đã che (D-05); KPI đọc từ API; `cost_per_confirmed_order` KHÔNG hiển thị (chưa có cost model, W-0054); CSV export + auto-refresh chưa làm; component library vẫn `NEED_CONFIRMATION`; visual/a11y QA thuộc W-0039; màn còn lại thuộc W-0027

- D-02/D-05 và KPI đọc API là yêu cầu đúng. W-0054 đã có mô hình local, nhưng **chưa có giá
  và instrument cost_per_confirmed_order**; “chưa có cost model” cũ chỉ đúng một phần.
- CSV là **tuỳ chọn** trong P3-2 §6.2; auto-refresh nằm trong build step §6.1 và khi triển khai
  cũ còn thiếu. Không gộp hai mục thành cùng một lỗi bắt buộc.
- Màn W-0027 đã từng làm; component library/a11y/auto-refresh không còn là việc sửa UI trong
  IVR sau W-0253. Capture còn giữ callback 422 đúng trạng thái, masked phone và role matrix
  lịch sử; `IT-ADMIN-READ-01` Passed chỉ chứng minh API, không thay 4 UI test gốc đã xoá.
- **Việc thiếu / owner:** Codex ghi closeout W-0026 với quyết định retire và phiếu M3 phân biệt
  auto-refresh/CSV/cost; Owner M8 duyệt trạng thái kết thúc. Owner M3 xác nhận scope dashboard,
  cung cấp commit/UI tests/visual-a11y của họ khi thực hiện. Owner M8 cung cấp đầu vào giá cho
  work cost riêng; không bịa giá để điền KPI.

### W-0027 — P3-3 config/integration/roles

**Thiếu bằng chứng kết thúc work/chuyển giao UI.** [README và capture](../W-0027/README.md).

**Residual nguyên văn tại mốc rà:**

> read-only toàn bộ; không có control đổi adapter mode/REAL; KEY_9 NOT_ENABLED; OD-V1-15 hiển thị khoá; fail-closed chỉ **gắn nhãn**, KHÔNG verify (chờ W-0040); `E2E-UI-REPLAY-05` thay bằng `E2E-UI-REVIEW-05` vì `specs/ui/06` không có replay và API không có operation đó; gán quyền không thuộc IVR (DF-01)

- “read-only toàn bộ” không còn đúng ở những bản UI về sau: `37d84dc` bổ sung script lifecycle,
  `83979d6` runtime gates, `f1e8bc9` seed/scenario. Tuy nhiên toàn console sau đó bị xoá.
- OD-V1-15 đã chốt; W-0040 đã triển khai readiness 503 của tiến trình. Điều đó **không chứng
  minh mọi dependency ngoài đã được probe** và không biến badge stub cũ thành integration test.
- KEY_9 NOT_ENABLED, không bật REAL từ UI và M3 quản lý quyền là ranh giới đúng.
  Thay `E2E-UI-REPLAY-05` bằng REVIEW là ngoại lệ có giải thích trong README, không phải chứng
  minh replay đã làm. Bốn UT UI và E2E review hiện không còn. Không tự mở lại replay trong IVR.
- **Việc thiếu / owner:** Codex ghi riêng phần đã được work sau thay thế, phần retire và ngoại lệ
  replay/role vào closeout W-0027. Owner M8 duyệt kết thúc; Owner M3 chốt yêu cầu config/seed/
  integration/roles theo API hiện hành và cung cấp E2E/permission evidence cho sản phẩm của họ.

### W-0028 — P3-4 reporting/analytics UI

**Thiếu bằng chứng kết thúc work/chuyển giao UI.** [README và capture](../W-0028/README.md).

**Residual nguyên văn tại mốc rà:**

> đọc-only tuyệt đối: không control dispatch/đổi trạng thái đơn (D-02), không ghi Order Core/CRM/evidence (D-14) — ghi duy nhất là audit export của chính IVR; UI **chỉ format**, mọi KPI do API tính (P3-4 §4); banner nói thẳng nguồn là đọc vận hành chứ CHƯA có pipeline P10-4; drill-down dừng ở bucket tổng hợp, không có link sang call detail; không chart library, chart `aria-hidden` + bảng số song song; RBAC-denied verify bằng API trả 403 vì **không seed actor nào thiếu `IVR_QUEUE_VIEW`**; `cost_per_confirmed_order` vẫn chưa có (W-0054); a11y/visual QA thuộc W-0039; hosted GitLab `NOT_RUN`

- “CHƯA có pipeline P10-4” đã cũ: [W-0055](../W-0055/README.md) triển khai ETL/schema analytics;
  [AnalyticsReadService.LoadAsync](../../../src/Ivr.Api/Application/AnalyticsReadService.cs)
  chọn warehouse khi có fact, vẫn có fallback operational và tự khai nguồn. `8f149da` còn sửa
  lỗi analytics khi chạy thật. Không được suy warehouse **đã deploy/chạy trên cluster**.
- 4 TestId BI Passed trong C2 là backend. 5 TestId UI đã retire cùng `c0e6609`.
  Source còn comment cũ về warehouse; kết luận ở đây dựa nhánh LoadAsync đang thi hành.
- Read-only, KPI từ API, audit export riêng và drill-down aggregate là giới hạn privacy đúng.
  Không link call detail không tự là lỗi: P3-4 cho phép dừng ở aggregate/masked bucket.
  Chart aria-hidden + bảng song song là lựa chọn thiết kế; không thay kiểm a11y thực.
- Cost metric vẫn thiếu đầu vào như W-0026; RBAC-denied bằng API không phải ảnh UI denied;
  visual/a11y và hosted UI lịch sử chưa chứng minh sản phẩm M3 hiện tại.
- **Việc thiếu / owner:** Codex viết closeout W-0028 tách UI retired khỏi BI backend còn sống;
  Owner M8 duyệt kết thúc. Owner M3 cung cấp commit/UI E2E có user bị từ chối quyền,
  freshness/source banner, export/privacy và a11y khi nhận delivery. Owner M8/IVR dev giữ
  các đầu vào cost và triển khai ETL thật ở work tương ứng, không đóng bằng bảng BI local.

## 5. Thứ tự thực hiện sau báo cáo

1. **Owner M8:** xét 8 đề nghị P2 theo phạm vi local/MOCK; nếu đồng ý, ghi quyết định riêng cho
   từng ID rồi mới chuyển ACCEPTED. Các gate M3/lab/production giữ nguyên.
2. **Codex + Owner M8:** hoàn tất closeout 4 P3 theo quyết định W-0253; chọn trạng thái terminal
   đúng với scope đã bỏ và lập bản bàn giao cho M3. Không cần một lượt UI test mới trong IVR.
3. **Codex:** bổ sung đối chiếu hai test retired của W-0019; sửa lỗi C2 ở F1, thêm regression
   rồi kiểm chứng tại commit mới và sinh lại danh sách. Khi C2 đã xét đúng gói mới, trình W-0019.
4. **Owner M8 + đầu mối M3:** xác nhận người nhận và đầu vào tích hợp; triển khai các việc ngoài
   P2 MOCK theo gói tương ứng. Báo cáo này không gửi thông điệp hay bàn giao ra ngoài.

## 6. Kiểm tra chính báo cáo

13 ID duy nhất, 9 P2 + 4 P3; Residual chép từ Git ở SHA đã chốt, giữ nguyên toàn bộ.
Các bằng chứng đính kèm được hash từ Git blob; phụ lục kiểm các định nghĩa test hiện hành
với đủ bốn TRX đã xác minh. CT-CI-06 được đối chiếu shell gate; test bị retire được giải thích
thay vì đếm như lỗi test runtime. Kiểm 38 liên kết nội bộ, mirror tracker và diff: PASS.
GitNexus staged check: LOW, 0 affected process. Gói mới qua PII scan.
Mirror của snapshot chỉ chứa audit này có 308 work; checkout chung có 310 vì giữ WIP W-0320/W-0321.
Không có thay đổi source/test/manifest, không thay 13 trạng thái, không gọi khách thật.

## Phiếu hiện hành — W-0337, 21/09/2026

[W-0337](../W-0337/approval-request.md) đã kiểm lại cả P2/P3 tại aaba3d2 sạch: 1171 test,
42 gate, chín P2 đạt C1/C2/C4 với142 lượt TestId; ba pin quyết định hợp lệ. W-0019 đã đủ
retirement/replacement và được trình thành việc P2 thứ chín. Bốn UI vẫn được đề nghị CANCELLED
theo W-0253, không phải ACCEPTED sản phẩm UI. Toàn chưa quyết định hai nhóm này; các số và
phân loại phía trên là lịch sử tại mốc đã ghi. Tổng ACCEPTED hiện57; không thay trạng thái13 việc.
