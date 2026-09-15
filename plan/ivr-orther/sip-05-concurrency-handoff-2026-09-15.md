# SIP-05 — Điều phối đồng thời, sở hữu controller: bàn giao

**Work ID:** không có. `SIP-01…SIP-10` là mã nội bộ của
[kế hoạch Mobile SIP Trunk](mobile-sip-trunk-production-32-channels-plan-2026-09-15.md), **không phải W-ID**.
Nếu owner muốn gắn W-ID thì phải đổi tên **ba** migration — `20260915085434_AriControllerOwnership`,
`20260915114705_DialTokenResolveLedger` và `20260915122953_ProductionCallApprovalIsEnvironmentScoped` —
**trước** khi chúng ra release. Xem mục 6.

**Baseline khi bàn giao:** `main@b817fae`. Sáu commit code: `56e3213`, `6eaa64b`, `b1bf377`, `c061001`,
`eb7e781`, `b817fae`; cộng `adb9a66` (kế hoạch) và `763cf1f` (bản đầu của tài liệu này).

**Trạng thái:** **`G2_SOFTWARE_CONCURRENCY_PROVEN / POOL_BOUND_PROVEN_ON_POSTGRES / CONTROLLER_OWNERSHIP_LANDED / TOKEN_LEDGER_DURABLE / PRODUCTION_GATE_ENV_SCOPED / CONTROLLER_STATUS_OBSERVABLE / TRUNK_POOL_BLOCKED_ON_SIP_03 / TOKEN_PROTECTOR_BLOCKED_ON_PLATFORM / VENDOR_INPUT_REQUIRED / NOT_PUSHED`**

**Ngày:** `2026-09-15`

> Phần làm được mà không cần đầu vào bên ngoài **đã hết**, và đã chạy thật. Bốn việc còn lại ở mục 5
> đều **bị chặn, không phải làm dở** — và mỗi việc bị chặn bởi một thứ khác nhau: nhà mạng (pool trunk),
> Platform (nguồn khoá token), hợp đồng release (định danh bản triển khai), và một quyết định của owner
> (nút cô lập controller). Đọc mục 5 trước khi bắt tay, vì viết code cho bất kỳ việc nào trong đó lúc này
> là **bịa ra chính cái hợp đồng đang chờ**.

---

## 1. Đã làm, và tại sao

### 1.1. `56e3213` — quay số đồng thời dưới một trần có kiểm soát

Trước đây một pass của scheduler kết thúc bằng `await` chính cuộc gọi nó vừa claim, nên một pass dài
bằng một cuộc gọi. Hai hệ quả, và chỉ một liên quan tới dung lượng:

- Một worker chỉ giữ được **đúng một** cuộc dù pool có bao nhiêu kênh rảnh. `ClaimBatchSize=32` không
  hề tạo ra 32 cuộc đồng thời.
- **Defect đang sống:** `PollingJobHost` chỉ tick `WorkerLiveness` khi pass trả về, và `WorkerLiveness`
  coi loop là stale sau `max(3×poll, 30s)` = 30s ở poll mặc định. Cuộc gọi bình thường dài tới
  120s audio + 30s ring + 15s DTMF. Nghĩa là **mỗi cuộc gọi dài đều báo scheduler bị treo**, và toàn bộ
  bookkeeping đêm (quarantine lease, đóng deadline) đứng yên suốt cuộc đó.

`RunOnceAsync` giờ **reserve → claim → start → trả về**. `SchedulerDispatchPump` giữ các cuộc đang chạy.

Ba điểm thiết kế đừng đảo ngược khi refactor:

| Điểm | Lý do |
| --- | --- |
| Reserve **trước** khi claim | Một lease claim mà không chạy được để lại dòng `ivr_sim_channels` ở RESERVED, mất một fencing generation, và không ai trả lại cho tới khi `QuarantineExpiredLeasesAsync` phát hiện 10 phút sau — trên chính job vừa đến hạn. |
| Trần là **counter dưới lock**, không phải `SemaphoreSlim` | Trần phải **hạ được**. 32 → 8 phải chặn nhận mới cho tới khi active rút xuống, và không được cắt cuộc đang nối máy. Semaphore không co lại được mà không phải chọn cuộc nào để bỏ. |
| Mặc định `1`/`1` | Hành vi không đổi cho tới khi config nâng lên, theo đúng `1 → 4 → 8 → 16 → 32` của plan. |

**Trần này là process-local và KHÔNG phải trần toàn hệ thống.** Trần thật là pool `ivr_sim_channels`.

### 1.2. `6eaa64b` — giảm tải sau các lần dispatch lỗi liên tiếp

Đây là **regression do chính commit trước tạo ra**: dispatch lỗi từng ném lên `PollingJobHost` và
`LoopBackoff` giãn vòng lặp. Khi cuộc gọi sống lâu hơn pass thì không còn đường đó nữa, nên một trunk
từ chối mọi originate sẽ bị quay số ở tốc độ claim đầy đủ suốt thời gian nó hỏng.

Lịch giãn **dùng lại chính `LoopBackoff`**, không phát minh cái mới — `Compute` và `ApplyJitter` đều
static và thuần, nên tái dùng được đúng đường cong, đúng trần 30s, đúng half-jitter. **Không có ngưỡng
nào cần ai quyết định.**

Shed được kiểm **trước** trần đồng thời và CPS: khi tuyến đang hỏng thì "còn mấy suất rảnh" là câu hỏi
sai. Hệ quả: **một cuộc lỗi chặn luôn phần còn lại của pass đó**. Một cuộc thành công **xoá sạch streak**
(giống `LoopBackoff.RecordSuccess`) — pump không đọc được SIP cause code nên không phân biệt được tuyến
chết với vài số sai, nhưng nó thấy được có gì đó đang thông hay không.

### 1.3. `b1bf377` — cổng sở hữu ARI controller

Asterisk giao application cho socket nào kết nối sau cùng và chỉ báo cho bên thua **sau khi** đã lấy.
Không có cách nào bảo nó từ chối. Nên hai worker cùng tin mình nên quay số **không** va vào một cái lock —
một bên im lặng mất event stream của những cuộc nó tưởng đang chạy, bên kia bắt đầu cuộc mới.

**Quy tắc cả thiết kế này tồn tại để bảo vệ: hết lease KHÔNG phải là chỗ trống.** Node mất kết nối DB
nhưng vẫn giữ socket ARI báo cáo y hệt node đã chết.

| Tình huống | Kết quả | Cần người? |
| --- | --- | --- |
| Holder drain xong rồi release | Worker sau lấy tự do | Không |
| Lease hết hạn | `AwaitingIsolation` | **Có** |
| Lấy sau khi cô lập | `AwaitingReconciliation` — sở hữu nhưng **chưa được gọi** | **Có** |

Controller đã bị cô lập **không bao giờ lấy lại được** — mà nó lại là bên hay đòi nhất, vì nó không hề
biết có chuyện gì.

Bốn invariant nằm ở DB. Quan trọng nhất: **không thể cô lập ẩn danh** (`ck_..._isolation_is_attributed`).
`IT-SCH-CTRL-06` chứng minh bằng cách đi vòng qua C# và bị DB từ chối.

**Quyết định có chủ đích: V1 không có endpoint để cô lập.** Profile là một replica, hậu quả sai là hai
controller cùng gọi một tập khách; thao tác DB có chủ đích hợp với mức rủi ro đó hơn một cái nút. Nếu
định làm endpoint, xem mục 5.3 trước.

**Vì sao cổng này chịu lực:** socket ARI mở **lazily bên trong `DialAsync`**
([AsteriskAriSimGateway.cs:91](../../src/Ivr.Infrastructure/Telephony/AsteriskAriSimGateway.cs:91)),
không mở lúc khởi động. Pod không được quay số thì không bao giờ mở socket cạnh tranh. Đừng đổi sang
kết nối sớm mà không đọc lại toàn bộ mục này.

### 1.4. `c061001` — drain chạy được, và trần chung có bằng chứng

**Defect trong chính cái vừa ship:** `terminationGracePeriodSeconds` không được đặt ở đâu trong chart,
nên worker chạy trên mặc định 30s của Kubernetes — ngắn hơn một cuộc gọi và đúng bằng drain. SIGKILL rơi
giữa drain: cúp máy giữa chừng với khách, **và** bỏ qua bước trả quyền ARI (chỉ chạy sau drain hoàn tất).
Pod kế tiếp phải chờ hết lease rồi cần người cô lập một scope mà chủ của nó chỉ bị giết quá sớm —
**mỗi lần deploy trùng cuộc gọi**.

- `DispatchDrainSeconds` 30 → **180**, `terminationGracePeriodSeconds` = **210**.
- `ControllerLeaseSeconds` 60 → **240**, và validator **kiểm chéo** `ControllerLeaseSeconds >= DispatchDrainSeconds`.
  Cả hai hợp lệ khi đứng riêng nhưng sai khi đi cùng: trong lúc drain không có gì renew grant vì vòng lặp
  đã dừng.
- Helm worker chuyển sang `Recreate`, và hai ghi chú scaling cũ được sửa — chúng giải thích replica thừa
  là "tranh lease", **nói nhẹ hơn thực tế**.

### 1.5. `eb7e781` — trần dial-token sống qua restart (SIP-02, nửa dưới)

`OD-V1-17` thay "one use per attempt" bằng một **trần**, và tính chất mua được là: token rò rỉ vẫn không
quay số nhiều hơn policy cho phép. **Tính chất đó thực ra chưa hề được giữ.** Ledger thực thi nó là
`ConcurrentDictionary` nằm trong vault instance của tiến trình, và chính class tự ghi rằng ràng buộc thật
đến từ "SIM vault" — vault đó chưa bao giờ được xây. Nên restart là xoá bộ đếm, và hai worker mỗi bên giữ
một bộ riêng: token được phép quay hai lần thì quay được **hai lần mỗi tiến trình**, vô hạn, miễn là tiến
trình cứ restart.

`ivr_dial_token_resolves` giữ **một dòng cho mỗi (token, attempt)** thay vì một bộ đếm, vì cả ba luật đều
đọc ra được từ các dòng còn bộ đếm chỉ trả lời được một: số lần là số dòng, binding là task trên dòng sớm
nhất, replay là dòng đã có sẵn — và primary key biến vế cuối thành invariant của DB chứ không phải việc
ứng dụng phải nhớ kiểm.

**Không lưu token.** Khoá là SHA-256 của thứ vault tiết lộ. LAB vault vốn đã đưa ra fingerprint không đảo
ngược được, nên ở đó đây là lớp bảo hiểm thừa — nó hết thừa đúng lúc một vault **đảo ngược được** giá trị
của chính nó sở hữu ledger này, tức mục tiêu của phần token production.

Thứ tự từ chối lấy nguyên từ bản in-memory, **không suy lại**: hết hạn → thiếu trần → binding → replay →
trần. Replay trước trần không phải ngẫu nhiên: một attempt lặp phải đọc ra là replay kể cả khi budget đã
cạn, vì một bên là bug của caller còn bên kia là policy đang làm đúng việc.

`LabDialTokenVault` nhận ledger là **dependency bắt buộc, không có default**. Default sẽ khiến một bản
triển khai cấu hình sai lặng lẽ nhận trần per-process mà không test nào phát hiện — **đúng cách cái cũ
sống sót lâu đến vậy**.

**MOCK giữ ledger in-memory.** Vault của nó là fake tất định mà mọi đích đều là cùng một chuỗi allowlist,
nên trần ở đó là chuyện tất định của test chứ không phải bảo vệ khách hàng.

### 1.6. `b817fae` — gate production theo môi trường, và trạng thái controller nhìn được

**Gate.** `PostgresProductionCallGate` chỉ hỏi "có approval `PRODUCTION_CALL` nào còn hiệu lực không",
nên **một chữ ký mở mọi bản triển khai** đọc chung database — approval cho pilot cấp phép luôn cho
production, trong khi cột `environment` đã nằm sẵn trên dòng đó từ `W-0195` mà không ai dùng.
`DispatchGate` **vốn đã cầm** environment và chỉ đơn giản không truyền xuống.

Giữ nguyên độ thô của hai kind kia: `RUNTIME_GATE_ADMIN` bỏ qua environment **có chủ đích**
(`IT-GATE-APPROVAL-10` pin điều đó), `FEATURE_FLAG_CHANGE` được siết bằng fingerprint four-eyes của
từng thay đổi. `PRODUCTION_CALL` **không có cả hai** — approval chính *là* quyết định.

`environment IS NULL` **khớp không gì cả**, không phải wildcard — wildcard sẽ giữ nguyên đúng hành vi
đang bị bỏ. Migration cũng **từ chối lưu** dòng `PRODUCTION_CALL` không ghi môi trường, đóng nốt nửa
lặng lẽ hơn của cùng một bug: nếu chỉ sửa query thì dòng đó vẫn insert được và **âm thầm không làm gì**,
để người ký tin rằng họ đã cấp phép.

> Migration này **fail to** trên database đã có sẵn một dòng như vậy. Không migration nào seed kind này,
> nên một dòng như thế là quyền gọi khách thật mà không ai giới hạn phạm vi — đáng để deploy dừng lại và
> có người nhìn vào, hơn là lặng lẽ `NOT VALID` rồi mang đi tiếp.

**Chưa làm phần candidate, và là bị chặn chứ không bỏ qua.** Plan đòi environment **và** candidate, nhưng
**không có gì trong codebase định danh một bản triển khai** — `IvrTelemetry.Version` là hằng `"1.0.0"`.
Chưa có hợp đồng release để gắn approval vào; bịa ra ở đây là bịa ra chính hợp đồng đó.

**Bề mặt vận hành.** `/healthz` giờ mang trạng thái ARI controller. Không có nút cô lập là **quyết định**
(mục 1.3); không **nhìn thấy** được trạng thái là **thiếu sót** — và là cái tệ hơn, vì worker kẹt ở
`AwaitingIsolation` chờ ai đó để ý mà không có gì báo cho ai cả.

Nằm ở body và **cố ý không** ở status code. Worker không được quay số là đang chạy đúng thiết kế; fail
probe sẽ restart nó, mà restart không cấp được application và còn làm rơi các cuộc nó đang drain — đúng
lập luận endpoint này vốn đã dùng cho `Idle`. **Đọc trạng thái quan sát được lần cuối, không query** —
probe mà chạm DB sẽ fail đúng lúc DB fail. Trước pass đầu tiên thì **vắng mặt**, không bịa.



---

## 2. Bằng chứng

| Kiểm tra | Kết quả |
| --- | --- |
| Build toàn solution | 0 warning, 0 error (`TreatWarningsAsErrors`) |
| Unit | **713/713** |
| Integration (Testcontainers PostgreSQL) | **315/315** |
| `IT-DB-MIGRATE-01` | drop về `InitialDatabase` rồi dựng lại → **cả hai** migration mới rollback được |
| CI selftest | `ci-config-selftest`, `capacity-selftest` pass |
| **LocalMockE2E, 2 worker + đủ lỗi** | **PASSED**, 5 vòng / 50 task / 0 failure |

LocalMockE2E chạy ở baseline `c061001`, tức **trước** `eb7e781` và `b817fae`. Hai commit đó đổi nhánh DI
của **lab** và đường `PRODUCTION_REAL`; harness chạy MOCK nên `DispatchGate` trả về ở nhánh `MOCK_MODE`
trước khi chạm gate, và vault MOCK giữ ledger in-memory. Rất có khả năng không ảnh hưởng — nhưng **chưa
chạy lại ở HEAD**, ghi ra để không ai đọc bảng này thành "đã chạy ở HEAD".

Ba dòng đáng giá nhất từ lần chạy thật:

- `recovery_required=1 held=1 redialled=0` — worker bị giết giữa cuộc, lease hết hạn, **không gọi lại mù**
- Kill switch bật: **0 attempt sau 100 lượt scheduler poll** — vòng lặp vẫn quay trong khi không quay số
- Kill switch thả: mọi task bị giữ hoàn tất **đúng một lần**

Lệnh tái lập (xem mục 4 về port và policy):

```bash
node tools/dev/Invoke-LocalMockE2E.mjs --rounds 5 --workers 2 --policy gh-247-prod-v1 --api-port 5117 --sales-port 18187
```

**Không render được Helm** ở máy này (không cài `helm`). Thay đổi chart được review chứ chưa render;
`k8s-selftest.mjs` phủ ở CI.

---

## 3. Test đã thêm

| Mã | Chứng minh |
| --- | --- |
| `UT-SCH-PUMP-01…04` | 32 giữ đồng thời, cuộc 33 **không hề được claim**; bảo trì vẫn chạy khi đầy; CPS tách khỏi trần; hạ trần không cắt cuộc đang chạy |
| `UT-SCH-PUMP-05…07` | Lỗi báo đúng một lần và trả suất; claim ném lỗi không rò suất; drain báo đúng sự thật |
| `UT-SCH-PUMP-08…10` | Một cuộc lỗi shed phần còn lại của pass; một cuộc thông xoá shed; shed tự hết hạn |
| `UT-SCH-CTRL-07` | Runtime **tuân** cổng sở hữu — state machine đúng mà không ai hỏi thì vô dụng |
| `UT-SCH-CONFIG-08` | Lease ngắn hơn drain bị từ chối |
| `IT-SCH-CTRL-01…06` | Máy trạng thái sở hữu trên Postgres thật, gồm cả DB từ chối cô lập ẩn danh |
| `IT-SCH-POOL-01…03` | Trần chung: một worker không vượt pool; **hai worker chạy cùng lúc không vượt pool giữa hai bên**; pool cạn là chờ chứ không lỗi |
| `IT-GATE-APPROVAL-11…12` | Approval production-call **chỉ mở đúng môi trường nó ghi**; DB từ chối approval không ghi môi trường |
| `UT-WRK-CTRL-01…06` | Trạng thái controller hiện ra ở `/healthz`, **không** làm hỏng probe; vắng mặt chứ không bịa trước pass đầu |
| `IT-TOKEN-DURABLE-01…06` | Trần token **sống qua restart**; hai tiến trình tranh cùng token **dùng chung một budget**; binding và replay vượt qua ranh giới tiến trình; hết hạn/thiếu trần không ghi gì; bảng chỉ chứa hash |

`IT-SCH-POOL-02` cố ý **không** assert tỉ lệ chia giữa hai worker — 5/3, 8/0, 4/4 đều là kết quả đúng
của một race; pin một cái là pin lịch chạy của test host chứ không phải tính chất của hệ thống.

---

## 4. Bẫy môi trường đã gặp (để lần sau khỏi mất thì giờ)

1. **Port 5015 bị chiếm bởi dự án khác.** `Ginsengfood.Operational.Api.dll` từ
   `C:/Projects/ginsengfood-ops-core/` listen trên 5015. Harness không bind được, request đi vào API cũ
   đó và trả `401`. **Đừng giết tiến trình đó** — nó không thuộc repo này. Dùng `--api-port` / `--sales-port`.
2. **DB dev đăng ký `gh-247-prod-v1`, không phải `mock-lab-v1`** mặc định của harness. Dùng `--policy`.
3. Harness ghi đè `docs/evidence/W-0203/local-mock-e2e.json`. Đó là evidence của **W-0203**, không phải
   của việc này — đã `git checkout` trả lại. Lần sau chạy xong nhớ kiểm `git status`.
4. `npx gitnexus analyze` sửa số symbol trong `AGENTS.md`/`CLAUDE.md` và chạm 6 file
   `.claude/skills/gitnexus/*` (chỉ line-ending, nội dung không đổi).

---

## 5. Làm tiếp cái gì

### 5.1. Nửa trên của SIP-02 — production protector, **chờ Platform**

Nửa dưới (ledger bền vững) **đã xong** ở `eb7e781` — xem mục 1.5. Còn lại:

| Nửa | Trạng thái |
| --- | --- |
| ~~Ledger bền vững (RAM → DB)~~ | ✅ `eb7e781` |
| Production protector (khóa quản lý) | **Chờ Platform chốt nguồn khóa** |

`IOpaqueValueProtector` hiện chỉ có `MockOnly`, `Unavailable`, `LabDialTokenVault`, `MockDialTokenVault`.
Cần mã hoá có xác thực, tách mục đích, key version, và kiểm rotation/mất khoá/xoá theo retention.
**Không tự chọn nguồn khoá** — hỏi Platform.

### 5.2. Phần **candidate** của SIP-04 — chờ hợp đồng release

Nửa environment **đã xong** ở `b817fae` (mục 1.6). Plan đòi environment **và** candidate; phần candidate
chưa làm được vì **không có gì trong codebase định danh một bản triển khai** — `IvrTelemetry.Version` là
hằng `"1.0.0"`, và không có image digest / release ref nào được nối tới app.

Cần chốt trước: **cái gì định danh một bản triển khai** để approval gắn vào (digest? release ref? cả hai?),
và ai cấp nó vào runtime. Đó là một quyết định hợp đồng release, không phải một dòng code.

**Dòng chặn ở [`IvrOptionsValidator.cs:53`](../../src/Ivr.Infrastructure/Configuration/IvrOptionsValidator.cs:53)
vẫn giữ nguyên** và không được đụng tới ở bước này.

### 5.3. Hành động vận hành cho controller — còn thiếu nút, không còn thiếu mắt

Quy trình thủ công đã có: [`docs/operations/ari-controller-ownership.md`](../../docs/operations/ari-controller-ownership.md).
SQL trong đó **đã chạy thật** trên DB dev trong transaction rồi rollback, gồm cả ca cô lập ẩn danh bị
ràng buộc từ chối.


`b817fae` đã đưa trạng thái lên `/healthz` (mục 1.6), nên phần **nhìn** đã xong.

Còn lại là phần **bấm**: `PostgresAriControllerOwnership.IsolateAsync` và `ConfirmReconciledAsync` đã có,
có audit, nhưng chỉ gọi được từ code — người vận hành phải thao tác thẳng vào DB. V1 cố ý như vậy (mục
1.3). Nếu owner đổi ý thì đây là chỗ làm, và phải đi kèm four-eyes chứ không phải một endpoint trần.

### 5.4. Chặn bởi SIP-03 / nhà mạng

**Pool 32 kênh trunk.** [`AsteriskAriOptionsValidator`](../../src/Ivr.Infrastructure/Telephony/AsteriskAriOptions.cs:103)
pin lab vào **đúng** một kênh: *"The free softphone profile is pinned to its lab alias, channel and ARI
adapter."* Plan nói thẳng không được nới guard `LAB-A`. Pool có kích thước thuộc production trunk profile.
Thêm `trunk_id` bây giờ là bịa ra hình dạng của một profile chưa ai báo giá.

Cũng chặn: dừng toàn trunk khi lỗi auth/quota (SIP-07, T16) — cần `trunk_id` ở trên.

---

## 6. Việc cần owner quyết

1. **Ba migration không mang W-ID:** `20260915085434_AriControllerOwnership`,
   `20260915114705_DialTokenResolveLedger` và `20260915122953_ProductionCallApprovalIsEnvironmentScoped`.
   Plan nói `SIP-01…10` không phải W-ID và bịa một cái sẽ đặt tham chiếu chết vào schema. Đổi tên
   **trước** khi ra release nếu muốn.
2. **V1 không có endpoint cô lập** — xác nhận giữ nguyên, hay muốn làm endpoint (xem 5.3).
3. **MOCK vẫn dùng ledger in-memory** trong khi lab/production dùng bản bền vững — xác nhận lý do ở mục
   1.5 là chấp nhận được.
4. **Định danh bản triển khai** cho phần candidate của SIP-04 (mục 5.2) — cần quyết định hợp đồng release.
5. **Chưa push.** Sáu commit code nằm trên `main` local.

## 7. Đang chờ bên ngoài

| Bên | Cần gì | Chặn việc nào |
| --- | --- | --- |
| Ba nhà mạng | Đặc biệt: **bán trunk cho Asterisk mình, hay chỉ bán API gọi hộ?** Nếu là vế sau thì SIP-03 là **kiến trúc khác**, không phải config khác | SIP-03 → pool trunk, SIP-07, SIP-09 |
| M3 | `§3.1` — **[phiếu đã soạn](questions-to-module-3-call-limit-2026-09-15.md)**, chưa gửi | T13; con số `MaxResolves` bên gọi (không đổi schema ledger) |
| Platform | Nguồn khoá token, định danh bản triển khai, xác nhận profile worker — **[phiếu đã soạn](questions-to-platform-key-source-and-release-identity-2026-09-15.md)**, chưa gửi | Mục 5.1, mục 5.2; deploy tuyến thật |
