# Module 8 — Việc còn lại

**Baseline:** `main@1804241` · **Cập nhật:** 07/09/2026 · **Dev:** Toàn

Bản này thay bản audit 07/09 làm danh sách giao việc. Sau mười một lượt khắc phục
(`W-0208`→`W-0223`), **không còn mục nào M8 tự đóng được**. Mọi thứ dưới đây chờ một người có tên,
một môi trường thật, hoặc một chữ ký — nên danh sách xếp theo **ai phải quyết**, không theo nhóm
audit nữa.

| | |
| --- | --- |
| `dotnet test Ivr.sln` | **900/900**, 0 failed, 0 skipped |
| `gate-status.mjs` | `GATE_STATUS_PASS` — 221 work item, 11 gate, 5 open decision |
| Gate offline | 50 script đã chạy; hai cái đỏ đều cần CI/Docker, không phải defect |
| Pin nguồn | 32 khớp / 0 lệch |

**Ba cổng vẫn đóng, không lượt nào mở:** `REAL_CUSTOMER_CALL_ALLOWED=NO`
(`IvrOptionsValidator.cs:49-52` từ chối boot nếu YES) · TARGET_V1 delivery thật bị
`CallbackDeliveryOptions.cs:72-78` chặn · `TARGET_CONTRACT_V1` vẫn `DRAFT`.

**Chưa kiểm, đừng suy ra từ bản này:** hosted CI · clean-checkout · server test `192.168.1.61` ·
sandbox M3 · production · mọi tài liệu ngoài repo.

### Mã cũ → mục mới

Bản trước xếp theo nhóm audit (`0.x`, `A`, `B`, `C`, `X`). Không mục nào bị bỏ; `B4` và `0.5` là
**cùng một quyết định** nên gộp làm một.

| Mã cũ | Ở đâu bây giờ |
| --- | --- |
| `0.1` · `C6` · `C13` | **2.1**, **2.2** |
| `0.2` | **7.1** — ✅ owner đã chốt |
| `0.3` · `C10` | **5.1** |
| `0.4` · `0.6` · `X2` · `X3` · `C2` · `D1`–`D10` | *Đã xong* |
| `0.5` · **`B4`** | **4.1** — cùng một quyết định |
| `A1` `A2` `A3` `A4` · `C5` | **3.1**–**3.4** |
| `B5` | **1.1** — ✅ owner đã chốt |
| `X1` | **1.2** — ✅ owner đã chốt |
| `B1` | **1.3** — ✅ owner đã chốt |
| `B8` · `B12` | **1.4**–**1.5** |
| `B6` | **8.1** |
| `B10` | **6.1** |
| `B11` | **4.2** |
| `C1` | **2.3** |
| `C-SESSION` (`C3`+`C4`+`C7`) | **2.4** |
| `C-REVOKE` (`C11`+`C12`+`C14`) | **2.5** |
| `C8` · `C9` | **2.6** |
| `B2` · `B9` | *Đã đóng* |

---

## 1 · Anh quyết

### ~~1.1 — Chọn `End` của khung giờ gọi~~ → ✅ **owner chốt 07/09, đã thi hành**

Owner chốt **`21:07:30`**. Đã đặt `End = 21:08` — xem ngay dưới vì sao không phải `21:07:30`.

| Program | T0 muộn nhất còn đủ **hai** cuộc | trước | sau |
| --- | --- | ---: | ---: |
| `TWENTY_FOUR_SEVEN` | offset `450s` | 20:52:30 | **21:00:30** |
| `GOLDEN_HOUR` | offset `150s` | 20:57:30 | **21:05:30** |

Đơn 24/7 cuối cùng nhận đúng lúc `21:00:00` giờ đủ hai cuộc — trước đây cuộc 2 rơi vào `21:07:30`
và bị từ chối.

> ⚠️ **`21:07:30` không biểu diễn được, và `21:07` sẽ hỏng thầm lặng.**
> `EndMinuteOfLocalDay` là **phút**, `Evaluate` bỏ phần giây. Tại `21:07:30` minute-of-day là
> `1267`; `1267 < 1267` là sai → gate **đóng**. Đặt `21:07` trông như tuân lệnh nhưng quyết định
> **không xảy ra**. `21:08` là giá trị nhỏ nhất thi hành được ý đó, đắt thêm 30 giây.
> Muốn đúng từng giây thì phải đổi trường sang đơn vị giây — sửa contract config cho 30 giây,
> tôi không tự làm.

`UT-SCH-WINDOW-09` **đã đỏ** khi đổi và được sửa có chủ đích — đó là lý do nó suy mốc từ hai nguồn
đã ký thay vì gõ tay. Nay nó khẳng định thêm: đơn 24/7 lúc `21:00:00` đủ hai cuộc.
IR-06 `§3.4.2` và register `OD-V1-16` đã cập nhật. · `W-0215` → `W-0220`

**`LOCK-05` (20:15–21:00) vẫn không có nguồn** — và **không** phải căn cứ cho con số này.

### ~~1.2 — Ba nhánh và bốn worktree~~ → ✅ **owner chốt 07/09** · còn một câu chưa trả lời

| Nhánh | So với `main` | Kết quả |
| --- | --- | --- |
| `codex/w0128-w0129-candidate` | **ahead 1** — thiết kế | ✅ **giữ** — mốc provenance của `W-0130` |
| `codex/p03-expand-contract` | ahead 0 | 🗑 xoá ref (`d5539ba` vẫn reachable từ `main`) |
| `worktree-gd0-fixes` | ahead 0 | 🗑 xoá ref (`cc12e53` vẫn reachable từ `main`) |

Nhánh `ahead 1` ra đời **chính vì** `main@2a4f45d` là commit `save` trộn 98 file, không đủ provenance
— đúng thứ mục **3.3** dưới đây than phiền. Evidence `W-0130` khai đích danh branch, đường dẫn
worktree và tree hash. Xoá nó = commit unreachable = đứt chuỗi bằng chứng.

**Owner chốt:** giữ nhánh `W-0130`, xoá hai nhánh kia. Đã làm:

| Câu hỏi | Trả lời | Đã thi hành |
| --- | --- | --- |
| 1. Còn cần mốc provenance? | **Còn** | `codex/w0128-w0129-candidate` giữ nguyên; **miễn trừ tường minh** đã ghi vào `CLAUDE.md` + `AGENTS.md` |
| 2. Xoá ref hai nhánh `ahead 0`? | **Xoá** | `codex/p03-expand-contract` và `worktree-gd0-fixes` đã xoá |
| 3. Gỡ bốn worktree? | **chưa trả lời** | **không đụng thư mục nào** |

Local branch nay còn đúng `main` + một mốc provenance. Hai worktree của nhánh đã xoá được
**detach**, không xoá thư mục — nên `git worktree list` vẫn thấy chúng ở trạng thái detached HEAD.

> **`W-0224` (08/09) — `1.2` mới chỉ dọn *local*.** Trên `origin` vẫn còn
> `codex/phase-1-2-opus-remediation` (`34340cc`, **ahead 0 / behind 173**, đã xác nhận là ancestor
> của `main`) — đã xoá. `pre-push` cho phép: hook miễn trừ local oid toàn số không, tức thao tác
> xoá. Nay **cả hai remote chỉ còn `main`**, cùng SHA `03a1922`.

> **Còn chờ:** hai thư mục `Desktop/ivr-p03-expand-contract` và `.claude/worktrees/gd0-fixes` giờ là
> worktree detached không còn nhánh. Gỡ hay giữ là câu 3, và một trong hai nằm **ngoài repo** nên
> tôi không tự xoá.

Mâu thuẫn `CLAUDE.md` ⟷ `W-0130` **đã gỡ**: luật vẫn cấm tạo nhánh và không đụng hook nào; chỉ đánh
dấu **một ref có tên** là load-bearing, kèm lý do, để lượt audit sau không báo lại là rác.
· `W-0214` → `W-0222`

### ~~1.3 — Capacity: giữ `UNCALIBRATED`~~ → ✅ **owner chốt 07/09: giữ, chờ `W-0008`**

Trạng thái không đổi — và đó là quyết định, không phải quán tính: model tự khai `UNCALIBRATED`,
`CAP-CALIB-03 PASS_UNCALIBRATED`, `CAP-DRIFT-05 PASS_DECLARED_DISAGREEMENT`,
`EXTERNAL_INTAKE_DEFERRED_BY_OWNER` giữ nguyên. `PT-CAP-02` (M8-P0-009) **đã có** — đừng giao lại
như chưa làm.

#### Nhưng hoãn thì phải hoãn cho đúng (`W-0223`)

Đi kiểm thì **checklist thoát vẫn nói "ba con số"**. `W-0212` khai báo con số thứ tư
(`specAverageCallSeconds = 35`) nhưng **không** sửa đoạn *"When W-0008 produces measurements…"*.
Ai làm `W-0008` sẽ theo checklist đó, calibrate ba số, bỏ lại số thứ tư — **tái tạo đúng thiếu sót
mà `W-0212` vừa vá** — rồi `CAP-DRIFT-05` đỏ ở một chỗ trông như bí ẩn.

Đã sửa hai thứ:

- **Checklist thoát** liệt kê đủ **bốn** số và số nào lấy giá trị gì.
- **`assertCalibratedDurationSemantics`** nay đòi `specAverageCallSeconds == modelCallSeconds` khi
  calibrated, kèm một mutation `TEST_ONLY` khẳng định calibration bỏ quên số thứ tư **bị từ chối**.

Đó cũng chính là thứ **đóng được mười giây**: uncalibrated thì không có phép đo nên không ép quan hệ
nào (chỉ ghim); calibrated thì cả bốn cùng mô tả một cuộc gọi đã đo, nên hai số của spec được **dẫn
lại từ phép đo** và khoảng chênh biến mất — **không ai phải phân xử số nào sai**.

Bất biến hành vi không đổi: `CAP-MODEL-01` 21 kênh, `CAP-SENS-02` 27 corner / `7..72`.

### 1.4 — TTS VieNeu

`gate-status` W-0122 = `BLOCKED_EXTERNAL`; `deploy/tts/`, `MODELS.lock`,
`docker-compose.vieneu-tts*.yml` không có commit nào trong `b21ec67..HEAD`.

**Bốn việc khác nhau, đừng gộp** (Codex F12): owner **chọn giọng** (manifest 3 giọng
`OWNER_ACCEPTED` còn khớp) ≠ **nghe mối nối / 6 cuộc thử** ≠ **target hardware** ≠ **release
approval**. Provenance gate vẫn báo `LEGAL,INTERNAL_MIRROR`.

> **Quyết:** theo gate từng artifact. Không mở lại phần đã ký khi binding không đổi; không dùng
> artifact chọn giọng để đóng lab/production.

#### `W-0225` (08/09) — đi kiểm hai blocker thì một cái **không phải gate**

Quyết định *"theo gate từng artifact"* buộc phải hỏi: hai blocker có thật là gate không? Một cái
không. `MODELS.lock` có hai cổng cạnh nhau, cùng hình dạng, được đối xử **hoàn toàn khác**:

| | `legal_gate` | `internal_mirror_gate` |
| --- | --- | --- |
| guard trong `validate()` | **có** — throw | **không** |
| đòi thẩm quyền / người ký / ngày / reference | **cả bốn** | — |
| ràng vào artifact | — | **không** |
| mutation test | có | **không** |
| **điều kiện mở** | 5 trường đúng thẩm quyền | **`status !== "PASS"`** |

Chạy thật, trước khi sửa:

```text
internal_mirror_gate = {"status": "PASS"}      ← ba chữ
→ release_blockers=LEGAL                        ← INTERNAL_MIRROR biến mất
→ internal_mirror_uri still null on 13/13 artifacts
```

Đúng hình dạng `legal_gate` từng có ngày `28/08`, trước khi `2a4f45d` tự ký `PASS` và `TODAY-03`
phải khôi phục. Bài học đó chỉ được áp cho **một** trong hai cổng.

Luật đúng **đã tồn tại** — `verify-model.py` đòi *"production requires an exact internal mirror"* —
nhưng nằm trong nhánh `--mode production`, mà **CI chỉ chạy gate Node** (`apk add nodejs`) và image
không ship `deploy/tts/scripts/`. Cùng lớp lỗi `7.1`: một luật, hai bản, một bản lỏng hơn.

**Đã sửa** đối xứng ở cả hai file: đòi hồ sơ quyết định (`decided_by` · `approval_reference` ·
`decided_on`) **và** mọi artifact phải có `internal_mirror_uri` + `internal_mirror_digest` thật.
Gate nay **10 mutation** (trước 8); probe cũ nay trả `exit 1 · internal mirror approval invalid`.

> ⚠️ **Cố ý không chỉ định `decision_authority`.** `LEGAL_PRIVACY` tồn tại vì owner **không được**
> tự ký review pháp lý. Cổng mirror thì lý do trong lock ghi thiếu *"**owner-approved** internal
> artifact or OCI mirror URI/digest"* — owner **là** thẩm quyền đúng. Đòi hồ sơ, không đòi danh tính.

> ⚠️ **Bẫy cho người dựng mirror:** `expectedArtifactSetSha256` phủ cả hai trường mirror, nên điền
> giá trị thật **sẽ** làm gate đỏ `artifact provenance fingerprint drift` — đúng lúc làm đúng. Đã ghi
> comment tại chỗ. Re-pin cùng lượt, theo dây chuyền `W-0126`.

**Hai blocker không đổi trạng thái.** `LEGAL` vẫn cần review licence, `INTERNAL_MIRROR` vẫn cần
Platform. Lượt này chỉ làm cái thứ hai **không mở được bằng ba chữ**. · `W-0225`

#### `W-0226` (08/09) — hai phiếu để gửi, đã khôi phục và cập nhật

Hai phiếu hỏi cho đúng hai blocker này **đã tồn tại từ `28/08`** và bị lượt dọn `04/09` (`8ed62e9`)
gỡ mất, dù cả hai `NOT_SENT` và chưa từng bị supersede. `W-0122/README` thì ghi intake *"đi qua
`W-0185`"* — nhưng W-0185 là **biểu nhận** (`LEGAL_PRIVACY_APPROVAL` → `artifact_ref` + `sha256`),
không chứa 13 cặp URI/digest cũng không chứa `L1`–`L7`. Thay câu hỏi bằng biểu nhận thì bên kia
không còn gì để trả lời.

| Phiếu | Hỏi gì | Chặn |
| --- | --- | --- |
| [questions-to-legal-od-voice-07](ivr-orther/questions-to-legal-od-voice-07.md) | `L1`–`L7`: licence không có file LICENSE, training data, quyền 3 preset, attribution, nghĩa vụ khi bỏ SaaS, retention | `legal_gate` |
| [questions-to-platform-w0122-infrastructure](ivr-orther/questions-to-platform-w0122-infrastructure.md) | `INF-A` mirror 13 artifact · `INF-B` target hardware · `INF-C` `OD-VOICE-08` media sink | `internal_mirror_gate` + 2 gate khác |
| [questions-to-security-w0122-cve-disposition](ivr-orther/questions-to-security-w0122-cve-disposition.md) | `SEC-A` ký disposition 16 finding **hoặc** `SEC-B` đổi base image | `RELEASE` (`W-0227`) |
| [today-03-tts-handoff-pack](ivr-orther/today-03-tts-handoff-pack-2026-08-29.md) | gói routing cho cả ba, kèm bảng 6 cuộc MicroSIP và retention/rollback drill | — (`W-0227`) |

> ⚠️ **Chỗ rẽ chưa ai chốt, ghi rõ trong cả hai phiếu.** `OD-V1-19` ký `05/09` — *"không vendor TTS
> lúc chạy; thu giọng người thật"* — **đã vào code**: template `v3-test-approved` không còn
> `{{customer_display_name}}`. Còn ba placeholder động; `total_amount` và `delivery_area` thu trước
> được, **`items_spoken` thì chưa ai trả lời**. Nếu thu trước được thì mục `INF-A` (mirror 201 MiB
> weights) **không còn cần**.
>
> Nhưng `L3` **vẫn cần dù đường nào**: 12 đoạn cố định hiện có là audio **do VieNeu render**, Owner
> ký `28/08`. *"Được chạy model không"* và *"được dùng audio model đã tạo ra không"* là hai câu khác
> nhau. · `W-0226`

Chỗ rẽ đó chạm **cả ba** phiếu, mỗi phiếu một kiểu — bảng đầy đủ ở `today-03` phụ lục `P.2`:

| Phiếu | Nếu **bỏ** TTS lúc chạy |
| --- | --- |
| Legal | `L1`–`L4` phụ thuộc; **`L3` vẫn cần** (12 đoạn hiện có do VieNeu render); `L5`–`L7` cần dù đường nào |
| Security | 16 finding thuộc base image `ivr-tts` ⇒ **image không lên production ⇒ không cần disposition** |
| Platform | `INF-A` không còn cần; `INF-B` và `INF-C` vẫn cần |

> ⚠️ **Đừng ký `SEC-A` trước khi chốt.** Một disposition có thời hạn cho image không bao giờ deploy
> là nợ giấy tờ, và sẽ bị đọc như bằng chứng image đó đã được duyệt. · `W-0227`

### 1.5 — Mua SIM gateway

DI chỉ có mock / lab / `UnavailableSchedulerDispatchGateway` (`SchedulerCapacity.cs:567-568`).
Đây là **chờ mua**, không phải nợ code. Không coi Asterisk softphone là bằng chứng real-SIM.

---

## 2 · M3 (± Security / Product)

> **Nền tảng rất vững, kiểm lại được:** `git diff --stat b21ec67..HEAD` trên `Contracts/Generated`,
> `TaskIntakeEndpoint.cs`, `TaskIntakeService.cs`, `PostgresSchedulerStore.cs`, `EligibilityRules.cs`
> = **rỗng**. Schema task/callback trong OAS không đổi một byte. **M3 viết producer/consumer được
> ngay** — mặt cắt đứng yên 62 commit và `W-0204` có cơ chế đóng băng giữ nó đứng yên.

### 2.1 — Chốt con số TTL `dial_token` ⚠️ nặng nhất · *M3 + Security*

`OD-V1-17` ký *"TTL = cửa sổ + 60s"*. **Ba** guard đang chạy buộc nó **bằng đúng** cửa sổ:

| Tầng | Luật |
| --- | --- |
| Intake `TaskIntakeService.cs:411-414` | từ chối nếu **<** `window.ExpiresAt` |
| Persistence `PersistenceInvariantValidator.cs:120-124` | throw nếu **>** |
| Dispatch `PostgresTelephonyDispatchStore.cs:157-160` | throw nếu **>** `lease.Deadline` |

Task dựng đúng theo quyết định đã ký sẽ **qua intake rồi ném exception ở persistence**.

> **Quyết `DTK-02`/`DTK-06`:** chốt số, rồi sửa **cùng lúc** OAS + intake + persistence + dispatch +
> IR-06 + CDC. Sửa lẻ hai tầng thì cuộc gọi vẫn chết ở tầng thứ ba.

Đã ghim `IT-INTAKE-DB-03`; IR-06 `§3.4.1` mô tả đủ ba tầng; register `OD-V1-17` có dòng correction.
**OpenAPI cố ý chưa sửa** — bump hash ghim cần re-pin có review, gộp vào lượt sửa khi chốt số.
· `W-0208`

### 2.2 — Contact gate: custody của bên cấp token · *M3 + Security*

Production issuer/resolver chưa có — chỉ `MockDialTokenVault`/`LabDialTokenVault`. Ledger `W-0199`
có task binding + ceiling + audit, nhưng đó là **local ledger, không phải distributed replay proof**.

`phone_validation_status`: optional trong OAS, runtime bắt `== "VALID"` (422). Đổi thành required
enum là **thay đổi contract cần xét compatibility**, không phải sửa hiển nhiên.

### 2.3 — Ký wire mapping `program_code` · *M3 + Product*

Phần M8 xong: IR-06 `§3.10 R3` có bảng cặp `program × payment`, hành vi từ chối tường minh
(`GOLDEN_HOUR + COD` → `400 IVR_MALFORMED_REQUEST`, loại tại schema), và **nguồn business** Flow
04/05 đóng `27/08`. Checklist `L1205` tự ghi *"Đã đóng 27/08 … chỉ còn ký wire mapping"*.

Phần "đổi mô tả `ProgramCode` thành tham chiếu registry M3" chờ M3 công bố registry — chưa tồn tại.

### 2.4 — `golden_hour_session_id` (gộp C3 + C4 + C7)

Không có session id từ M3. Hiện `SessionId` chỉ tồn tại trên `CapacityIncidentEntity` với giá trị tự
sinh (`SCHED-`, `MOCK-SCHED-`, `ADMIN-QUEUE-`).

> **Việc:** ký field → producer mapping → store (nullable, additive) → CDC → enforce.
> **Không** map đè lên `session_id` nội bộ — giá trị tự sinh vẫn trace về job, giữ nguyên.
> **Cảnh báo:** required cho `program_code=GH` là **breaking** dù store nullable là additive; đừng
> hứa toàn bộ non-breaking.

### 2.5 — Thu hồi task giữa window (gộp C11 + C12 + C14)

Recall/hủy đơn bật giữa confirmation window thì attempt 2 **vẫn quay**. Claim query không đọc lại
`sale_lock`/`recall`/`order_state`; không có endpoint revoke. `W-0111 terminate` chỉ là admin cắt
cuộc đang gọi. ACK trên callback **không** thay được một command.

> **Chọn:** A (chấp nhận trade-off + M3 bắt buộc D-06) / B (M3 phát revoke) / hybrid.
> Nếu B thì **phải fence tới tận trước dial** — technical lease generation hiện có **không phải**
> order-revocation generation.

### 2.6 — Consumer callback + credential

Shape đã implement. `W-0207` tự tìm và sửa một mâu thuẫn ACK nguy hiểm: ma trận yêu cầu
`DUPLICATE_ACCEPTED` trên **409** trong khi `CallbackAck409` chỉ mang `REJECTED_STALE`/
`IDEMPOTENCY_CONFLICT` ⇒ `CALLBACK_ACK_INVALID` ⇒ **`INVALID_DEAD_LETTER` terminal**. M3 xây theo tờ
cũ thì **mọi replay chính xác chết lặng**. Đã sửa về `[200]` + gate `FREEZE-06`.

Đường gửi thật vẫn tắt **có chủ đích** (`appsettings.json:85-88` `Enabled=false`;
`CallbackDeliveryOptions.cs:72-78` fail-closed trên `W-0006`/`OD-V1-07`).

> **Việc:** M3 dựng consumer + auth + shared E2E. Đủ auth/sandbox/approval rồi mới mở delivery thật.

---

## 3 · Chief auditor

### 3.1 — Reconcile 19 quyết định `OD-V1` ký một lượt

`od-v1-signoff-2026-09-05.md:3-4`: *"Người ký: IVR owner · Người soạn phương án: Claude"*. Register
dòng `19-25` (OD-V1-01..07) đều `✅ CLOSED 2026-09-05` **trong khi cột Owner là Sales/Security/Core**.
Code dựng ngay lên đó (`AttemptPolicyRegistries.cs:52-75`, `W0195`, `W0196`, `appsettings ×4`).

**Ba điều phải tách bạch** (Codex F08 — cả audit lẫn tôi đều lệch một vế):

1. **Có thật** một bản ghi owner chọn phương án cho 19 dòng.
2. **Chưa chứng minh** quyền đồng ký của các owner được liệt kê. Email trong commit **không chứng
   minh cũng không bác bỏ** thẩm quyền của email người ký — câu tôi khẳng định danh tính cũng
   **không phải bằng chứng độc lập**.
3. **"Có chữ production" ≠ "production đã mở"** — runtime vẫn chặn real call lúc boot, production
   gateway chưa tồn tại, TargetV1 delivery vẫn bị options validator chặn.

> **Việc:** reconcile **từng quyết định một** theo scope + approval reference.
> **Không** rollback đồng loạt 19 dòng về OPEN — không đủ cơ sở, và sẽ phá cả dòng thuộc đúng thẩm
> quyền M8.

### 3.2 — Tách trạng thái register cho dòng có owner ngoài M8

> **Việc:** OD-V1-01/02/03/05/07 — đổi `CLOSED` thành `M8_POSITION_SIGNED / <owner> NOT_RECEIVED`.
> M3 đọc register hiện tại sẽ hiểu là đã chốt toàn hệ.

### 3.3 — Nợ hồ sơ của chính gate release

- ~~**52 work item** `evidence: null` là evidence chưa từng viết~~ → **claim sai, đã rút**
  (`W-0224`). Con số nay là **55**, và **không dòng nào là defect**.
  `gate-status.mjs:478-491` **cố ý** tách hai loại:

  | Loại | Số dòng `null` | Luật |
  | --- | ---: | --- |
  | prompt-backed (`^P\d+-\d+$`) | **4** | DoD đòi evidence pack §10 → **assertion cứng**. Cả bốn (`W-0049` `W-0050` `W-0051` `W-0056`) đều `BLOCKED_EXTERNAL` — evidence **chưa thể** tồn tại |
  | remediation `UNPLANNED` | **51** | không có prompt nên **không có yêu cầu §10**; evidence nằm ngay trong ô tracker |

  Vi phạm thật: **0**. Số dòng loại hai được đếm ra `rows_without_evidence_pack: 167` để
  **không** bị đọc thành thiếu. Chính comment trong script cảnh báo điều này: bản đầu của check đòi
  thư mục cho **mọi** dòng và gắn cờ 20 mục remediation — *"a rule that would have been satisfied
  by creating 20 empty directories, which is the opposite of what it is for."*

  ⚠️ **Đừng "trả nợ" mục này bằng cách tạo thư mục evidence.** Đó đúng là thứ check từ chối làm.
- **32 commit** subject `save`/`sa ve` reachable từ HEAD. ← **mục còn thật của `3.3`**
- ~~`W-0206` vô hình với bảng điều khiển~~ → **đã ghi hồi tố** (`W-0217`).

### 3.4 — Dispatch: 0/5 batch, không pack nào rời repo M8

Nút thắt thật của cả nhóm seam: mọi mục dừng ở *"đã đóng gói, chờ gửi"*.

Pack đủ: `today-01` 11 sheet gồm `S-11` (errata VoLTE + procurement), `m8-12` dispatch matrix kèm
SHA-256 từng artifact, `m8-13` message kit, năm batch `D-01..D-05` đã định tuyến, template nhận chữ
ký sẵn. Tự khai: **`External dispatch: NOT_PERFORMED`**.

> **Lưu ý:** `0/5` là trạng thái ledger — **không** chứng minh mọi trao đổi ngoài repo đều bằng 0.

### 3.5 — Một câu lạc trong spec, tôi không tự sửa

`docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md:472` — bảng field callback — ghi `result_type` là
*"Một trong **11** giá trị ở §16"*. Ràng buộc thật `ck_ivr_result_callbacks_result_status` cho
**đúng 6**: `IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, `IVR_NO_ANSWER_FINAL`,
`IVR_CONFIRMATION_WINDOW_EXPIRED`, `IVR_INVALID_PHONE_FINAL`, `IVR_CAPACITY_EXCEPTION`.

Cùng lớp lỗi `W-0211` đã sửa cho `specs/database`, nhưng lần này nằm trong **spec** — tài liệu mà
bản audit ghi *"luật folder cấm sửa"* và là bên thắng khi mâu thuẫn. Ai dựng consumer từ spec thay
vì IR-06 sẽ chờ 11 mã, **5 mã không bao giờ tới**. IR-06 `§4.3` thì đúng.

---

## 4 · Security / Platform

### 4.1 — Admin gate có cần scope theo môi trường không

Cột `environment` được **ghi** cho ba loại approval, **đọc** cho một:

| Loại | Đọc bởi | Lọc environment? |
| --- | --- | --- |
| `FEATURE_FLAG_CHANGE` | `PostgresFourEyesApprovalVerifier` | **Có, hai lớp** — predicate cột **và** fingerprint băm `Environment` |
| `RUNTIME_GATE_ADMIN` | `RuntimeGateApprovalReader.AnyLiveAsync` | Không |
| `PRODUCTION_CALL` | `AnyLiveAsync` | Không (không migration nào seed) |

Ai chèn row `RUNTIME_GATE_ADMIN` với `environment='lab'` sẽ tưởng đã giới hạn — không hề. Và **không
sửa được cột trên row đã cấp**: trigger append-only từ chối thẳng (`only revocation may change`).

**Mức nguy hiểm thấp hơn audit ngụ ý:** `FeatureFlagAdminService:74-79` từ chối **vô điều kiện** mọi
risk increase ở prod, đứng *sau* four-eyes; không seed `PRODUCTION_CALL`; mỗi thay đổi cụ thể vẫn bị
buộc vào đúng env qua fingerprint. Đây là **bẫy schema/tài liệu**, không phải đường vòng qua gate.

> **Quyết:** cần scope → sửa **cả contract + reader + test**, không chỉ đổi giá trị cột; giữ được
> `unconditionalRiskReduction`. Không cần → ghi thẳng vào `OD-V1-20` rằng cột cố ý trơ.

Đã ghim `IT-GATE-APPROVAL-10`, đã ghi vào doc của `RuntimeGateApprovalKinds`. · `W-0213`

> Hai đề xuất của audit gốc **không thi hành được**, đã retire: *"seed theo environment"* hỏng theo
> hai cách độc lập; *"test khẳng định prod không có `RUNTIME_GATE_ADMIN`"* sai tiền đề — row seed
> mang `environment=NULL` và áp mọi env theo thiết kế.

### 4.2 — Vị trí resolver `dial_token → E.164`

`ProviderPorts.cs:17-32` `DialAuthorization` gọi `OpaqueReferenceGuard.EnsureNotRawPhone`;
`Identifiers.cs:133-151` ném lỗi với chuỗi 10-12 chữ số bắt đầu `0`/`84`.

**Sửa kết luận** (Codex F06 — câu *"code làm OD-V1-18 không thi hành được"* là **quá mạnh**): guard
cấm raw phone đi qua **type đó**. Một adapter hoàn toàn có thể nhận opaque handle rồi resolve nội bộ
mà **không** đưa số thô vào `DialAuthorization` — đúng như API-04 §2 mô tả.

> **Việc:** reconcile authority/threat model giữa `M8-10`, `OD-V1-18` và spec V0.3.
> **Không** đề xuất bỏ privacy guard để làm tài liệu đúng.

---

## 5 · Product + CRM/M3 + Legal/Privacy

### 5.1 — Quorum `OD-V1-23` (opt-out)

Vị trí owner — *explicit-only* — **đúng và nên giữ**. Hai ví dụ tín hiệu trong ngoặc thì sai cả hai,
đã sửa trong register:

| `OD-V1-23` nêu | Thực tế |
| --- | --- |
| `DTMF-0` | **phím hủy đơn** — lời thoại khóa cứng *"bấm phím 0 để hủy"* |
| `handoff` / phím 9 | **ngoài scope**; `TargetV1SpeechPolicy.ValidateTemplate` **ném lỗi** với template chứa "phím 9" |

M8-08 §4.2 — **chính gói mà `OD-V1-23` dẫn làm closure evidence** — ghi thẳng *"Không tái dùng hai
phím này cho opt-out"*.

⚠️ Khách bấm 0 chỉ được nghe *"bấm phím 0 để hủy"*. Đọc thao tác đó thành lệnh cấm liên hệ vĩnh viễn
là **lấy consent khách chưa từng cho**.

> **Việc:** V1 **không còn phím trống** (`1`/`0` có nghĩa, `9` bị cấm). Thêm tín hiệu opt-out thật
> cần wording mới + signal source + proof + chữ ký Legal/Privacy riêng.

Tiêu chí cũ *"2 lần rejected → do-not-call"* **đã rút** — chỗ duy nhất còn nhắc là `m8-08:55`, và
nhắc để **phủ định**. Hằng số `2/3` là `TEST_ONLY_CANDIDATE`, **0 caller runtime**.
Đã ghim `UT-OPTOUT-DTMF0-05`. · `W-0210`

---

## 6 · Order Core

### 6.1 — `DR-03` / `DR-04`: hành vi no-answer và window-expired

`PersistenceModelConfiguration.cs:396-405` `ck_ivr_call_results_action_matches_type`: no-answer →
chỉ `NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`; window expired → chỉ `REVALIDATE_AND_EXPIRE_CONFIRMATION` /
`REVALIDATE_AND_HOLD_ADMIN_REVIEW`.

**Không sửa được bằng config** — đổi hành vi phải ra migration mới (không sửa `W0172`).

> **Việc:** chốt semantics với Core; giữ mapping hiện hành tới khi contract đổi.

---

## 7 · Nội bộ M8 — ✅ đã chốt

### ~~7.1 — `Idempotency-Key`: siết admin hay nới intake~~ → ✅ **owner chốt 07/09: siết admin**

Trước: cùng một tên header, **hai luật**, và cùng **một** `$ref` OpenAPI nên schema không thể mô tả
đúng cả hai. Key base64 bị từ chối ở cửa intake, nhận ở cửa admin.

Nay: một luật, ở **một chỗ** — `TraceHeaderSyntax` (`1-128`, `[A-Za-z0-9._:-]`, `PiiGuard`). Cả ba
nơi cùng gọi: intake, internal guard, correlation middleware. Ba bản sao của cùng một predicate
chính là cách chúng trôi khỏi nhau lần đầu, nên bản sửa bền là gộp chứ không phải thêm bản sao thứ
tư.

Ghim `IT-API-IDEMP-04`: base64 → `400`, `129` ký tự → `400`, `idem-<guid>` → `200`. Không test nào
trong repo phụ thuộc hành vi lỏng cũ, và mọi key đang dùng đều nằm trong bảng chữ cái.
· `W-0209` → `W-0221`

> ⚠️ **Đây là lần siết, và nó chạm 17 call site** (gitnexus `HIGH`, 2 execution flow). Rẻ đúng lúc
> này vì **BFF của M3 chưa tồn tại** và admin UI đã ra khỏi phạm vi — muộn hơn thì đắt hơn.

#### Còn lại: một lượt phát hành contract, không phải sửa code

Từ `W-0221` thì OpenAPI **mô tả được** cả hai route bằng một `$ref` — trước đó thì không. Nhưng sửa
nó là phát hành contract:

| | |
| --- | --- |
| siết header bắt buộc | **breaking** theo oasdiff |
| kéo theo | bump draft · sinh lại client · changelog |
| re-pin hash OAS | **5 nơi** (`contract-manifest`, `dial-token` validator, `portal-manifest`, `openapi-contract-diff`, `target-v1-field-inventory`) |

**Nên gộp cùng lượt sửa TTL (`2.1`) thành một lần phát hành thay vì hai.** Đó là lý do duy nhất còn
lại để hoãn — lý do cũ (*chưa quyết siết hay nới*) đã hết.

---

## 8 · Cần môi trường thật

### 8.1 — Hai lịch sử schema của `W0122`

`W0122DropConsoleAccounts.Up()` nay rỗng, **giữ nguyên migration ID**; `P03` chạy
`CREATE TABLE IF NOT EXISTS` cho `ivr_console_accounts` + `ivr_console_sessions`. DB chạy bản cũ có
bảng đã drop; DB mới thì không — `__EFMigrationsHistory` không phân biệt được.

**Phạm vi hẹp hơn audit nói** (Codex F03): `docs/database/expand-contract.md:21-32` **đã mô tả đủ 4
tình huống**, và `ExpandContractMigrationTests` có 2 case nằm trong suite integration vừa pass.

> **Việc còn lại:** inventory + backup + rehearsal trên **database đích thật**. Repair schema
> **không** phục hồi account/session đã xóa.

---

## Đã xong — không mở lại

Chi tiết ở evidence từng mục; đừng dựng lại phân tích từ đầu.

| Mục | Đã làm gì | Work ID |
| --- | --- | --- |
| **Docs sai về DB** | `specs/database` §4: *"11 giá trị, bốn nơi đang khớp"* → bảng **bốn tập** `11/9/6/5`. IR-06 `draft.22`→`.23` ở 5 chỗ. §4A.7 tách "gỡ khỏi API" khỏi "xoá khỏi DB" | [`W-0211`](../docs/evidence/W-0211/README.md) |
| **`40/50/60` occupancy** | Khai báo con số **thứ tư** (`35s` spec) mà `W-0132` bỏ sót, ghim trong `CAP-DRIFT-05`; sửa comment sai *"spec never writes 50s down"* | [`W-0212`](../docs/evidence/W-0212/README.md) |
| **Bẫy khung giờ trong compose** | `docker-compose.e2e.yml` nhận `CallingWindow` `0..1440`, **giữ Enabled=true** — tắt gate thì smoke thôi phủ đoạn code ra quyết định | [`W-0214`](../docs/evidence/W-0214/README.md) |
| **Log window-closed** | `SchedulerRunResult` mang `CallingWindowOpensAt`; host log **hai chiều chuyển trạng thái**, không log mỗi vòng | [`W-0214`](../docs/evidence/W-0214/README.md) |
| **Result taxonomy** | `m8-05 §3.1` bắc cầu sang tên business source — **không phải đổi tên 1:1**: `PACK-09` đặt tên theo lượt+lý do, runtime theo kết quả, ba tên gộp thành một code + `reason` | [`W-0217`](../docs/evidence/W-0217/README.md) |
| **Bảng điều khiển release** | `gate-status.mjs` **đã chết từ `W-0207`**, không ai chạy nên không ai biết. Sửa 7 status ghép, escape cột, sinh lại `--write`; board lệch 9 work item | [`W-0216`](../docs/evidence/W-0216/README.md) |
| **Pin nguồn validator** | 4 validator ghim hash IR-06 mà tôi sửa IR-06 4 lần không re-pin; quét lại toàn bộ → **32 khớp / 0 lệch** | [`W-0216`](../docs/evidence/W-0216/README.md) |
| **`W-0206` vô hình** | Có commit `4d0c761`, không có dòng ledger. Ghi hồi tố — id **đã được phát**, ghi xuống mới là tuân luật | [`W-0217`](../docs/evidence/W-0217/README.md) |
| **`D1`–`D10`** | Giữ `DONE_LOCAL`: local đã chứng minh, external chưa. `D1` chưa gọi là "docs sạch" được (xem **7.1**). `D8`: `550` tagged declarations ≠ `897` executed cases — hai số không thay nhau | — |

### Đã đóng — bỏ khỏi hàng đợi

- ~~**B2** 4 file softphone lab dirty~~ — **hết tiền đề**. `git status` rỗng; 3 file tracked/
  unmodified; file thứ tư **không tồn tại ở bất kỳ checkout nào**. Worklist 03/09 đã đóng đúng.
  ⚠️ Đề xuất *"commit vào nhánh lab"* của audit **vi phạm `CLAUDE.md`** và bị git hook chặn.
- ~~**B9** Admin UI~~ — **đã chuyển M3**. Worklist 03/09 đóng implementation local. Không tự dựng
  thêm route chỉ để giữ mục này ở nhóm code lõi.

---

## Nguồn và ghi chú

| Nguồn | Vai trò |
| --- | --- |
| Audit chief auditor 07/09 (`9f13bea`) | phát hiện gốc, 37 mục · `git show 9603ca5:plan/toan-viec-can-lam-m8-2026-09-07.md` |
| [Đối chiếu code + 71 chú thích](../docs/review/2026-09-07-m8-worklist-claude-annotated.md) | verify `file:dòng`, chạy test |
| [Đánh giá độc lập của Codex (F01–F12)](../docs/review/2026-09-07-m8-independent-full-review.md) | sửa cả audit lẫn chú thích |

**Bốn chỗ bản audit nói sai, đã sửa và đừng dựng lại:** phần trăm `29%/71%` **tái lập được** (bốn giá
trị "thiếu" nằm trong ô từng hàng — phản biện của tôi sai) · *"3 phiếu questions-to-\*"* là 3 phiếu
**TTS**, không mâu thuẫn với "10 phiếu" · *"không có file 30/08 trong Git"* chưa bác bỏ được tài liệu
ngoài repo · *"repo chỉ có main"* lặp **16 hàng** trong khi có 4 nhánh và 4 worktree.

Số dòng `file:dòng` trong bản gốc chính xác cao — kiểm ~60 tham chiếu, chỉ 3 chỗ lệch.

**Ba luật rút ra từ `W-0216`, để lượt sau không lặp:** `gate-status.yaml` và `readiness-board.md`
sinh bằng `--write`, không gõ tay · cột Status §5 nhận **một** token trong 12, sắc thái để cột
Residual · sửa một file được ghim thì **re-pin cùng lượt**.
