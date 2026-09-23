# W-0308 — `PD-02`: trần gọi đồng thời, và một câu cảnh báo trong runbook thành một lần từ chối khởi động

**Ngày:** `2026-09-16` · **Baseline:** `main@6029b19` · **Loại:** `1` file `.cs` production + `2` file test + runbook

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Tóm tắt

`PD-02` là việc cuối cùng còn lại trong [kế hoạch luồng gọi production](../../../plan/ivr-orther/sip-production-dial-path-plan-2026-09-16.md)
mà **không chờ ai** — `PD-01` (`W-0303`, `a2808ce`) và `PD-03` (`W-0305`, `d207526`) đã xong trước đó.

Kế hoạch liệt kê `6` gạch đầu dòng. **Bốn trong sáu đã đạt sẵn từ lượt `15/09`**, nên việc thật chỉ
là hai dòng còn đỏ. Đây là chiều ngược với `W-0307`, nơi `3/4` dòng kế hoạch **sai**; ở đây kế hoạch
**đòi ít hơn** thực tế đã có. Cả hai lần đều tới từ cùng một thói quen: đọc mã trước, tin kế hoạch sau.

| Gạch đầu dòng kế hoạch | Tình trạng khi kiểm lại | Việc `W-0308` |
| --- | --- | --- |
| Trần dùng chung nhiều worker | ✅ `IT-SCH-POOL-01/02/03`, Postgres thật | — |
| Vòng bảo trì tiến triển khi đầy | ✅ `UT-SCH-PUMP-02` | — |
| Drain lúc tắt | ✅ `UT-SCH-PUMP-07` + `SchedulerJobHost.StopAsync` | — |
| Tách `MaxCallStartsPerSecond` | ✅ option riêng + validator riêng + `UT-SCH-PUMP-03` | — |
| Bậc `1→4→8→16→32` | ❌ chỉ `32` và `8`; **`16` không có ở đâu** | `UT-SCH-PUMP-11` |
| Bậc cuối phải khớp hợp đồng | ❌ **chỉ là câu văn** | `SchedulerTrunkCapacityValidator` |

**Kết quả:** `1143/1143` toàn bộ solution (`773` unit · `338` integration · `24` contract · `8` chaos),
`0` failed `0` skipped. `8` `TestId` mới. Traceability `708` → `716`.

---

## 1. Bậc thang trần — `UT-SCH-PUMP-11`

### 1.1. Vì sao "đã có test cho `32`" là chưa đủ

Trước lượt này, tính chất trần — **giữ đúng `N`, `N+1` không bao giờ được claim, một cuộc kết thúc
mở đúng một suất** — được khẳng định ở `32` (`UT-SCH-PUMP-01`) và `8` (`UT-SCH-PUMP-02`).

Các số `1`, `2`, `4` **có xuất hiện** trong file test, nhưng là *bối cảnh* cho những test về lỗi
dispatch và drain, không phải phép kiểm trần. `16` không xuất hiện ở bất kỳ đâu trong cây.

Runbook `PD-03` lại nói:

> *"Bounds are 1–256; the software is proven at 1, 2, 8 and 32 (`UT-SCH-PUMP-01/02/04`)."*

Nên **cái thang mà người vận hành được bảo hãy leo có hai bậc không có gì đứng lên**. Câu đó đã sửa.

### 1.2. Đã làm

`[Theory]` năm bậc `1 / 4 / 8 / 16 / 32`. Hàng đợi luôn **nhiều hơn trần `8` việc**, vì với hàng đợi
ngắn hơn trần thì một pass dừng sớm trông y hệt một pass tôn trọng giới hạn.

Khẳng định đặt trên **số lần claim**, không phải số lần bị từ chối. Một reservation bị từ chối
*trước* claim thì DB không bị đụng tới; một reservation bị từ chối *sau khi* đã lấy lease sẽ để lại
một hàng `ivr_sim_channels` ở `RESERVED` với fencing generation đã tiêu, và **không gì trả nó lại
trong `RecoveryQuarantineSeconds` — mười phút** — trên đúng việc vừa đến hạn.

Bậc `1` là một case riêng chứ không phải hình thức: đó là mặc định mọi deployment khởi đầu, và là
bậc duy nhất mà *"giữ một, từ chối cái thứ hai"* và *"mở một rồi dừng"* là cùng một quan sát.

### 1.3. Mutation

| Đột biến | Kết quả |
| --- | --- |
| `SchedulerDispatchPump.cs:117` `active >= max` → `active > max` | **`5/5` bậc đỏ** |

---

## 2. Trần phải khớp hợp đồng — `SchedulerTrunkCapacityValidator`

### 2.1. Ba con số, không cái nào so với cái nào

| Con số | Ở đâu | Nghĩa |
| --- | --- | --- |
| `SipTrunk:ContractedChannels` | `SipTrunkOptions.cs:80` | nhà mạng bán bao nhiêu |
| `Scheduler:MaxConcurrentDispatches` | `SchedulerCapacity.cs:55` | **một tiến trình** giữ bao nhiêu |
| số hàng `ivr_sim_channels` | trong DB | pool mọi worker dùng chung |

Trần hiệu dụng là **nhỏ nhất trong ba**. Trước lượt này **không có gì so chúng với nhau** — và cả
hai tài liệu đều đã tự nói ra điều đó:

- Doc-comment của `ContractedChannels`: tách khỏi trần scheduler *"so that the two can be compared"*.
- Runbook `PD-03`, câu cuối mục *When the contract changes*: *"nothing warns you when they disagree
  — which is worth a gate of its own and does not have one yet."*

Một bên tuyên bố ý định so sánh, một bên nói thẳng là chưa có. `W-0308` làm cái gate đó.

### 2.2. Vì sao là **từ chối khởi động** chứ không phải một dòng log

Cấu hình vượt hợp đồng thì các cuộc dôi ra bị **nhà mạng** từ chối, và từ chối phía nhà mạng đến
dưới dạng **lỗi mạng chập chờn**: đổ cho tuyến, nặng nhất đúng lúc tải cao nhất — tức là lúc khó
tái lập nhất. Và nó **qua được mọi test không có nhà mạng**, nghĩa là qua được tất cả.

### 2.3. Ranh giới — cần, **chưa đủ**, và nói rõ

`MaxConcurrentDispatches` là **mỗi tiến trình**; hợp đồng là **toàn hệ thống**. Hai pod cùng đặt
`32` trên hợp đồng `32` thì **từng pod đều qua**. Thứ giữ ranh giới toàn hệ thống vẫn là số hàng
`ivr_sim_channels` dưới `SKIP LOCKED` — một cái bảng, dùng chung, và **không validator cấu hình nào
nhìn thấy được**; đó cũng chính là lý do nó mới là trần có thẩm quyền.

Kiểm tra này bắt **đúng một lỗi**: một worker bị bảo giữ nhiều hơn số đã mua. Đã ghi nguyên văn
giới hạn này vào doc-comment và vào runbook, để không ai đọc nó thành nhiều hơn thế.

### 2.4. Vì sao là validator **thứ hai**, không phải thêm luật vào cái đang có

`gitnexus_impact` trên `SchedulerOptionsValidator`: **MEDIUM**, `40` ký hiệu, `4` trực tiếp,
`0` execution flow. Đây là kiểm tra duy nhất đọc **hai** section cùng lúc, nên tách ra giữ cho
validator mà `40` ký hiệu chạm tới **không bị sửa một dòng nào**.

### 2.5. Mutation

| Đột biến | Kết quả |
| --- | --- |
| `>` → `>=` ở so sánh trần | `2/7` đỏ — đúng hai khẳng định biên |
| Xóa lối thoát `if (!trunk.Enabled) return []` | `1/7` đỏ (`UT-SCH-TRUNKCAP-01`) |

Lối thoát cho trunk chưa bật là **bắt buộc**, không phải tiện tay: lab và MOCK chưa từng cấu hình
hợp đồng, nên một phép so với số `0` kết quả sẽ **từ chối khởi động mọi máy dev trong dự án**.

---

## 3. Luồng cấu hình — `UT-SCH-CFG-01`

Kế hoạch nói *"nâng `MaxConcurrentDispatches` **qua cấu hình**"*. Mọi test khác dựng
`SchedulerOptions` **trực tiếp**, nên chứng minh được pump và không chứng minh gì về lối mà
giá trị thật sự đi trong một deployment.

Bốn option `SIP-05` được bind **từng cái một bằng tay** (`section.GetValue`), không phải `Bind`.
Một dòng thiếu trong lượt thêm đó trông **y hệt** một deployment chưa được cấu hình lại: worker
khởi động, báo healthy, và giữ đúng một cuộc.

| Đột biến | Kết quả |
| --- | --- |
| Xóa dòng bind `DispatchDrainSeconds` | đỏ — `Expected: 200 / Actual: 180` (mặc định lọt qua) |

### 3.1. Việc cố ý **không** làm: thêm bốn khóa vào `appsettings.json`

`Ivr:Scheduler` trong cả hai `appsettings.json` dừng ở `PollIntervalMilliseconds`; bốn option
`SIP-05` không có mặt. Tôi **không thêm** chúng vào, và đây là lựa chọn chứ không phải bỏ sót:

- Mặc định C# đã là một nguồn sự thật. Chép `1` vào thêm hai file JSON tạo thêm **hai chỗ nữa để
  lệch** — đúng loại lỗi mà chính lượt này đang sửa ở chỗ khác.
- Runbook `PD-03` đã ghi tên khóa và nói chúng thuộc *deployed config*, không phải file mặc định.
- Phát hiện được hay không **không còn là cơ chế an toàn** nữa: từ nay giá trị sai bị từ chối ở
  khởi động, bất kể nó tới từ đâu.

---

## 4. Ghi nhận ngoài phạm vi

`deploy/helm/ivr/values.yaml:42` có comment nói `terminationGracePeriodSeconds: 210` phải cao hơn
`Ivr__Scheduler__DispatchDrainSeconds` (`180`) — nhưng helm **không đặt** biến đó, nên hai số chỉ
được nối với nhau bằng **một dòng comment**. Cùng lớp lỗi với mục `2`, nhưng nằm ở lớp triển khai
(Platform đang chặn), nên **ghi lại chứ không sửa trong lượt này**.

---

## 5. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Toàn bộ solution | **`1143/1143`**, `0` failed, `0` skipped |
| — unit | `773` (`760` → `+13` case, `+8` `TestId`) |
| — integration · contract · chaos | `338` · `24` · `8` — **không đổi** |
| `TestId` mới | `UT-SCH-PUMP-11`, `UT-SCH-TRUNKCAP-01..06`, `UT-SCH-CFG-01` |
| Traceability | `708` → **`716`** |
| `gitnexus_impact` `AddIvrScheduling` | **LOW**, `0` impacted, `0` process |
| `gitnexus_impact` `SchedulerOptionsValidator` | **MEDIUM**, `40` ký hiệu — **không sửa**, thêm class mới bên cạnh |
| Contract / OAS | **không đụng** — `0` operation mới, `0` field mới, không `oasdiff`, không re-pin manifest |
| Mutation | `4` đột biến, **cả `4` đều bị bắt** |

Không đổi hành vi mặc định: `MaxConcurrentDispatches` vẫn là `1`, và khi trunk chưa bật thì kiểm
tra mới **không khẳng định gì**. Một deployment lab hay MOCK khởi động **đúng như trước**.

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0308 thuộc nhóm A2 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm: Nâng trần gọi
đồng thời. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: giới hạn toàn hệ thống do bảng kênh giữ; values.yaml thuộc
Platform. Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector
tại `ca4f442` (1200/1200 test, sweep 43/43), không coi commit tài liệu là một lượt test/sweep mới.
REAL_CUSTOMER_CALL_ALLOWED=NO.
