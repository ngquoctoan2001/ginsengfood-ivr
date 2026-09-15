# SIP-05 — Điều phối đồng thời, sở hữu controller: bàn giao

**Work ID:** không có. `SIP-01…SIP-10` là mã nội bộ của
[kế hoạch Mobile SIP Trunk](mobile-sip-trunk-production-32-channels-plan-2026-09-15.md), **không phải W-ID**.
Nếu owner muốn gắn W-ID thì phải đổi tên **hai** migration — `20260915085434_AriControllerOwnership` và
`20260915114705_DialTokenResolveLedger` — **trước** khi chúng ra release. Xem mục 6.

**Baseline khi bàn giao:** `main@eb7e781`. Năm commit code: `56e3213`, `6eaa64b`, `b1bf377`, `c061001`,
`eb7e781`; cộng `adb9a66` (kế hoạch) và `763cf1f` (bản đầu của tài liệu này).

**Trạng thái:** **`G2_SOFTWARE_CONCURRENCY_PROVEN / POOL_BOUND_PROVEN_ON_POSTGRES / CONTROLLER_OWNERSHIP_LANDED / TOKEN_LEDGER_DURABLE / TRUNK_POOL_BLOCKED_ON_SIP_03 / TOKEN_PROTECTOR_BLOCKED_ON_PLATFORM / VENDOR_INPUT_REQUIRED / NOT_PUSHED`**

**Ngày:** `2026-09-15`

> Phần SIP-05 làm được mà không cần nhà mạng đã xong và chạy thật. Phần còn lại của SIP-05 **bị chặn**,
> không phải làm dở: pool 32 kênh thuộc production trunk profile (SIP-03) và profile đó chờ nhà mạng
> trả lời. Đọc mục 5 trước khi làm tiếp — trong đó có hai việc đang mở mà **không** cần nhà mạng.

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

---

## 2. Bằng chứng

| Kiểm tra | Kết quả |
| --- | --- |
| Build toàn solution | 0 warning, 0 error (`TreatWarningsAsErrors`) |
| Unit | **702/702** |
| Integration (Testcontainers PostgreSQL) | **313/313** |
| `IT-DB-MIGRATE-01` | drop về `InitialDatabase` rồi dựng lại → **cả hai** migration mới rollback được |
| CI selftest | `ci-config-selftest`, `capacity-selftest` pass |
| **LocalMockE2E, 2 worker + đủ lỗi** | **PASSED**, 5 vòng / 50 task / 0 failure |

LocalMockE2E chạy ở baseline `c061001` (trước `eb7e781`). `eb7e781` chỉ đổi nhánh DI của **lab**, còn
harness chạy MOCK nên không đi qua đường đó — nhưng **chưa chạy lại sau `eb7e781`**, ghi ra để không ai
đọc bảng này thành "đã chạy ở HEAD".

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

### 5.2. Siết `PostgresProductionCallGate` — **không bị chặn, ưu tiên cao nhất hiện tại**

[`RuntimeGateApprovals.cs:181`](../../src/Ivr.Infrastructure/FeatureFlags/RuntimeGateApprovals.cs:181):
`IsApprovedAsync(CancellationToken)` không nhận environment/candidate, nên **một approval bất kỳ còn hạn
mở được mọi bản triển khai**. Plan chỉ đích danh việc này ở SIP-04.

Sửa nó là **siết cổng lại**, không phải mở ra — nên không dính nguyên tắc "đừng đụng dòng chặn ở
[`IvrOptionsValidator.cs:53`](../../src/Ivr.Infrastructure/Configuration/IvrOptionsValidator.cs:53) sớm".
**Dòng chặn đó vẫn giữ nguyên.**

### 5.3. Bề mặt vận hành cho trạng thái controller — thiếu sót thật

Hiện người vận hành không **nhìn thấy** được scope đang ở trạng thái nào ngoài một dòng log
(`EventId 2319`). Chưa có trong health/admin read. Việc không cho *bấm nút* là có chủ đích; việc không
cho *nhìn* thì không.

`PostgresAriControllerOwnership.IsolateAsync` và `ConfirmReconciledAsync` đã có, có audit, chỉ chưa có
đường gọi từ ngoài.

### 5.4. Chặn bởi SIP-03 / nhà mạng

**Pool 32 kênh trunk.** [`AsteriskAriOptionsValidator`](../../src/Ivr.Infrastructure/Telephony/AsteriskAriOptions.cs:103)
pin lab vào **đúng** một kênh: *"The free softphone profile is pinned to its lab alias, channel and ARI
adapter."* Plan nói thẳng không được nới guard `LAB-A`. Pool có kích thước thuộc production trunk profile.
Thêm `trunk_id` bây giờ là bịa ra hình dạng của một profile chưa ai báo giá.

Cũng chặn: dừng toàn trunk khi lỗi auth/quota (SIP-07, T16) — cần `trunk_id` ở trên.

---

## 6. Việc cần owner quyết

1. **Hai migration không mang W-ID:** `20260915085434_AriControllerOwnership` và
   `20260915114705_DialTokenResolveLedger`. Plan nói `SIP-01…10` không phải W-ID và bịa một cái sẽ đặt
   tham chiếu chết vào schema. Đổi tên **trước** khi ra release nếu muốn.
2. **V1 không có endpoint cô lập** — xác nhận giữ nguyên, hay muốn làm endpoint (xem 5.3).
3. **MOCK vẫn dùng ledger in-memory** trong khi lab/production dùng bản bền vững — xác nhận lý do ở mục
   1.5 là chấp nhận được.
4. **Chưa push.** Năm commit code nằm trên `main` local.

## 7. Đang chờ bên ngoài

| Bên | Cần gì | Chặn việc nào |
| --- | --- | --- |
| Ba nhà mạng | Đặc biệt: **bán trunk cho Asterisk mình, hay chỉ bán API gọi hộ?** Nếu là vế sau thì SIP-03 là **kiến trúc khác**, không phải config khác | SIP-03 → pool trunk, SIP-07, SIP-09 |
| M3 | `§3.1`: technical retry có tính vào "hai lần" không; khóa theo đơn hay theo đích | T13; con số `MaxResolves` bên gọi (không đổi schema ledger) |
| Platform | Xác nhận ARI controller = 1 replica, tắt autoscale; **nguồn khoá cho token protector** | Nửa trên SIP-02 (mục 5.1); deploy tuyến thật |
