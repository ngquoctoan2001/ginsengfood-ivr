# Module 8 — Danh sách việc cần làm (bản sạch, dùng để khắc phục)

**Baseline:** `main@9603ca5` · **Ngày chốt bản này:** 07/09/2026 · **Dev phụ trách:** Toàn

Bản này thay thế bản audit 07/09 làm **danh sách giao việc hiện hành**. Nó là hợp lưu của ba nguồn,
đã loại phần sai và phần đã lỗi thời:

| Nguồn | Vai trò | Lưu ở đâu |
| --- | --- | --- |
| Audit chief auditor 07/09 (baseline `9f13bea`) | phát hiện gốc, 37 mục | `git show 9603ca5:plan/toan-viec-can-lam-m8-2026-09-07.md` |
| Đối chiếu code của Claude (71 chú thích) | verify `file:dòng`, chạy test | [`docs/review/2026-09-07-m8-worklist-claude-annotated.md`](../docs/review/2026-09-07-m8-worklist-claude-annotated.md) |
| Đánh giá độc lập của Codex (F01–F12) | sửa cả audit lẫn chú thích Claude | [`docs/review/2026-09-07-m8-independent-full-review.md`](../docs/review/2026-09-07-m8-independent-full-review.md) |

**Đã kiểm bằng cách chạy thật tại `9603ca5`:** `dotnet test Ivr.sln` → **891/891 PASS**
(Unit 593 · Integration 266 · Contract 24 · Chaos 8), 0 failed, 0 skipped, exit 0.

**Chưa kiểm (đừng suy ra từ bản này):** hosted CI · clean-checkout · server test 192.168.1.61 ·
sandbox M3 · production · mọi tài liệu ngoài repo.

**Ba cổng vẫn đóng và bản này không mở cái nào:** `REAL_CUSTOMER_CALL_ALLOWED` vẫn `NO`
(`IvrOptionsValidator.cs:49-52` từ chối boot nếu YES) · TARGET_V1 delivery thật vẫn bị
`CallbackDeliveryOptions.cs:72-78` chặn · `TARGET_CONTRACT_V1` vẫn `DRAFT`.

---

## Nhóm 0 — Sửa hướng dẫn sai TRƯỚC khi M3 viết code

> Đây là nhóm duy nhất có deadline thật. Mỗi mục dưới đây là một chỗ **tài liệu bàn giao đang dạy
> M3 làm sai**. M3 code theo tài liệu hiện tại sẽ hỏng ở runtime, không hỏng ở compile — nên càng
> để lâu càng đắt.

### 0.1 — TTL `dial_token` mâu thuẫn xuyên **ba** tầng ⚠️ nặng nhất

`OD-V1-17` (ký 05/09) ghi *"TTL = cửa sổ xác nhận **+ 60s**"*. Code hiện tại có **ba** guard, và
chúng cộng lại buộc TTL phải **bằng đúng** window end:

| Tầng | File | Luật |
| --- | --- | --- |
| Intake | `TaskIntakeService.cs:411-414` | từ chối nếu `dial_token_expires_at` **<** `window.ExpiresAt` |
| Persistence | `PersistenceInvariantValidator.cs:120-124` | throw nếu **>** `ConfirmationWindowExpiresAt` |
| **Dispatch** | `PostgresTelephonyDispatchStore.cs:157-160` | throw *"Stored dial-token expiry exceeds the call deadline"* nếu **>** `lease.Deadline` |

Tầng dispatch là phát hiện của Codex (F01) — audit gốc và chú thích Claude **đều bỏ sót**. Hệ quả:
sửa OpenAPI + persistence theo "+60s" **vẫn chưa đủ**, dial sẽ chết ở tầng thứ ba.

IR-06 tự mâu thuẫn với chính nó: `L219` ghi `≥ window end`, `L522`/`L534` ghi `>`, `L983` mô tả
equality.

**Việc:** chốt một con số TTL với M3/Security → sửa **đồng thời** OAS + intake + persistence +
dispatch + IR-06 + CDC/boundary test. Không sửa lẻ từng tầng.
**Xong khi:** một task TTL = window+60s đi hết intake → persist → dispatch không exception.

#### Đã làm (W-0208, 07/09) — phần không cần chữ ký

Con số TTL vẫn chờ M3/Security. Nhưng phần đang gây hại thì không chờ được: tài liệu bàn giao đang
**dạy M3 gửi giá trị sẽ ném exception**. Đã sửa:

- **`IT-INTAKE-DB-03`** — test ghim cả ba biên trên Postgres thật: equality → nhận · `+60s`
  (đúng con số `OD-V1-17`) → qua contact gate rồi **throw ở persistence** · `−60s` → từ chối sạch
  tại intake kèm `DIAL_TOKEN_EXPIRES_BEFORE_WINDOW`. Test **cố ý sẽ đỏ** khi TTL đổi — sửa có chủ
  đích, đừng xóa. Suite: 550 → **551** tagged.
- **IR-06** — `L219` (`≥` → equality), `L522` (`phải lớn hơn` → equality; đây là câu sai nguy hiểm
  nhất, nó bảo M3 gửi đúng thứ bị từ chối), `L534` (checklist M3 tick → `=`), `§6` (thêm guard
  dispatch). Thêm **`§3.4.1`**: bảng ba tầng + cảnh báo `OD-V1-17` chưa thi hành được.
- **register `OD-V1-17`** — thêm dòng `Sửa 2026-09-07 (W-0208)`: vế TTL chưa thi hành được, đã pin
  bằng test. **Không** đổi trạng thái dòng — đó là việc của chief auditor (A2).

**Chưa làm, có chủ đích:** OpenAPI. `dial_token_expires_at` vẫn là `{ type: string, format:
date-time }` không mô tả ràng buộc. Sửa YAML sẽ bump hash đã ghim và cần re-pin — mà re-pin là hành
động có review (W-0204). Đúng thứ tự là gộp nó vào lượt sửa cùng lúc khi TTL được chốt.
Ghi chú của chính freeze verifier: *"Owners approve `06-module-3-api-handover.md`, not the YAML."*

### 0.2 — Ràng buộc header lệch: tài liệu nói 200, code chặn ở 128

`integration-requirements/06-module-3-api-handover.md:157-158` dạy M3:
`Idempotency-Key: <8-200 chars>`, `X-Correlation-Id: <1-200 chars>`.
`TaskIntakeEndpoint.cs:126` chỉ nhận `value.Length is > 0 and <= 128` (+ charset an toàn).
OpenAPI khai `type: string`, không nêu ràng buộc nào.

Phát hiện của Codex (F10). M3 sinh key 129–200 ký tự từ tài liệu → **400 IVR_MALFORMED_REQUEST**.

**Việc:** chốt một con số (128 hay 200), sửa cả ba nơi: IR-06, OAS (`maxLength`), runtime.

### 0.3 — `DTMF-0` bị gọi nhầm là tín hiệu opt-out

`OD-V1-23` (register `L64`) ghi *"chỉ coi là opt-out khi khách phát tín hiệu tường minh
(**DTMF-0** / handoff)"*. Nhưng `DispositionMapper.cs:126-136` map `DTMF "0"` →
**`IvrCustomerCancelled`** + `RevalidateAndCancelCustomerRequest`, reason `CUSTOMER_PRESSED_0` —
tức **hủy một đơn**, không phải "đừng gọi tôi nữa". M8-08 `L65` yêu cầu explicit-only.

Phát hiện của Codex (F04). Nếu M3 đọc register và build theo, khách hủy một đơn sẽ bị cấm liên hệ
vĩnh viễn.

**Việc:** sửa `OD-V1-23`; định nghĩa một tín hiệu opt-out riêng và cho các owner ký (M8-08 approval
table vẫn thiếu chữ ký, OD-V1-23 vẫn `QUORUM_PENDING`).

### 0.4 — Tài liệu specs nói sai về chính DB của mình

- `specs/database/03-enums-and-status.md` §4 ghi *"Result type — **11** giá trị"* + *"bốn nơi …
  **đang khớp**"*. Thực tế `ck_ivr_call_results_result_type` chỉ có **9** (W-0172 đã bỏ
  `IVR_OPERATIONAL_BLOCKED`/`IVR_POLICY_BLOCKED`). Ba tập con hợp lệ: **11** vocabulary wire ·
  **9** runtime · **6** final callback · **5** counted.
- IR-06 `L8`/`L25`/`L914`/`L1129` còn hướng dẫn lấy **draft.22**; manifest hiện **draft.23**.
- IR-06 §4A.7 nói bảng console không còn trong DB — nhưng `P03` giữ/tạo lại bảng compatibility.

**Việc:** sửa docs theo tập con, **không** ép mọi tầng về cùng một số.
**Lưu ý:** FREEZE PASS chỉ xác nhận invariant mà gate kiểm (pins, field inventory, draft state,
ACK matrix) — **không** chứng minh mọi câu trong docs khớp code.

### 0.5 — Ngữ nghĩa "approval theo môi trường" chưa có thật trong code

`RuntimeGateApprovals.cs:95-120` (`RuntimeGateApprovalReader.AnyLiveAsync`) — SQL chỉ lọc
`approval_kind` + `revoked_at IS NULL` + `expires_at`. **Không lọc environment, không lọc actor.**
Nên đề xuất "chỉ seed row cho dev/lab" **không tạo ra giới hạn môi trường nào** trong cùng một DB.

Phát hiện của Codex (F02). Xem thêm mục B4 bên dưới.

### 0.6 — `40/50/60` không phải ba cách viết của một con số

`40s`/`60s` là **occupancy**; `50s` là **full cycle đã cộng cooldown**. Yêu cầu "đưa cả ba về một
số" (có trong audit gốc) sẽ sai đơn vị hoặc cộng cooldown hai lần. `CAP-DRIFT-05` vừa pass đúng với
trạng thái `DECLARED_DISAGREEMENT`.

Phát hiện của Codex (F05). **Việc:** sửa câu chữ trong worklist/capacity docs, **không** sửa số.

---

## Nhóm A — Quản trị và chữ ký (không phải việc code)

### A1 — 19 quyết định OD-V1 ký trong một lượt, có dòng thuộc owner ngoài M8

Sự thật đã verify: `od-v1-signoff-2026-09-05.md:3-4` ghi *"Người ký: IVR owner
(`marketingssv2024@gmail.com`) · Người soạn phương án: Claude"*; register dòng `19-25` (OD-V1-01..07)
đều `✅ CLOSED 2026-09-05` **trong khi cột Owner là Sales/Security/Core**; code dựng ngay lên đó
(`AttemptPolicyRegistries.cs:52-75`, `W0195`, `W0196`, `appsettings ×4`).

**Ba điều phải tách bạch** (Codex F08 — cả audit lẫn Claude đều lệch một vế):

1. **Có thật** một bản ghi owner chọn phương án cho 19 dòng.
2. **Chưa chứng minh** quyền đồng ký của các owner được liệt kê (Sales/Security/Core).
   Email trong commit **không chứng minh cũng không bác bỏ** thẩm quyền của email người ký —
   câu của Claude khẳng định danh tính cũng **không phải bằng chứng độc lập**.
3. **"Có chữ production" ≠ "production đã mở"** — runtime vẫn chặn real call lúc boot, production
   gateway chưa tồn tại, TargetV1 delivery thật vẫn bị options validator chặn.

**Việc:** reconcile **từng quyết định một** theo scope + approval reference.
**Không** rollback đồng loạt 19 dòng về OPEN — không đủ cơ sở, và sẽ phá cả những dòng thuộc đúng
thẩm quyền M8.

### A2 — Tách trạng thái trong register cho dòng có owner ngoài M8

**Việc:** với OD-V1-01/02/03/05/07 — đổi `CLOSED` thành `M8_POSITION_SIGNED / <owner> NOT_RECEIVED`.
Lý do: M3 đọc register hiện tại sẽ hiểu là đã chốt toàn hệ.

### A3 — Nợ hồ sơ của chính gate release

- **8** work item `TESTS_PASS` + `evidence: null`: W-0194, W-0195, W-0198, W-0199, W-0200, W-0201,
  W-0202 (audit tìm được 7) **+ W-0205** (mới, sau baseline).
- **W-0206 không có dòng nào trong `gate-status.yaml`** (`grep -c "W-0206"` = 0) dù `4d0c761` đã
  merge — work item vô hình với bảng điều khiển.
- **32** commit subject `save`/`sa ve` trên lịch sử reachable từ HEAD (đã gồm HEAD `9603ca5`).

### A4 — 0/5 batch dispatch: không pack nào rời repo M8

Nút thắt thật của cả nhóm C: mọi mục đều dừng ở *"đã đóng gói, chờ gửi"*. Đây là việc **chief
auditor phải kéo về**, không phải việc dev.
**Lưu ý:** `0/5` là trạng thái ledger — **không** chứng minh mọi trao đổi ngoài repo đều bằng 0.

---

## Nhóm B — Code M8 tự làm được

### B5 — Khung giờ gọi cắt 21:00 chồng lên vòng đời task ⭐ rẻ nhất, làm trước

`CallingWindow.cs:31-43` (Enabled=true, UTC+420, 08:00–21:00) · `:121-122` open chỉ khi
`minuteOfDay < End` · `SchedulerRuntime.cs:93-96` ngoài giờ không claim dial.

Ví dụ tái hiện: task GH `T0=20:59`, window 5 phút, attempt 2 tại `21:01:30` — **nằm trong window của
task nhưng ngoài giờ gọi**. `CallingWindowTests.cs` hiện chỉ kiểm hàm thời gian/config
(`:41` = `[InlineData(21, 0, false)]`), **không có test nào cho task vắt qua mốc đóng**.

**Việc:** thêm test lifecycle cho task vắt mốc → rồi mới chốt tham số với owner.
**Cảnh báo:** "1 tham số + 1 test" là **dự đoán**, không phải estimate đã chứng minh (Codex F09).
`LOCK-05 (20:15–21:00)` **không có nguồn nào trong repo M8** — phải xin M3/owner trước khi lấy 21:05.

### B4 — Seed `RUNTIME_GATE_ADMIN` environment NULL

`W0195:142-162` seed `approval_kind=RUNTIME_GATE_ADMIN`, `environment=NULL`, approver `ivr-owner`.
`:138-141` xác nhận **không seed `PRODUCTION_CALL`**.

**Sửa mức độ nghiêm trọng** (Codex F02 — chú thích "ĐÚNG TUYỆT ĐỐI" của Claude là quá mạnh):
`FeatureFlagAdminService.cs:74-79` **từ chối mọi risk increase tại production** *sau* bước four-eyes.
Có thêm approval row **cũng không** làm HTTP API cho phép tắt kill switch hay mở allowlist ở prod.
Đây **không** phải bằng chứng đang có đường gọi production vượt gate.

**Việc:** chốt scope approval. Nếu thật sự cần phân môi trường thì phải sửa **cả contract + reader
+ test** (xem 0.5), không chỉ đổi giá trị `environment` trên row. Giữ nguyên khả năng giảm rủi ro
khẩn cấp một người (`unconditionalRiskReduction`).

### B6 — Hai lịch sử schema của `W0122`

`W0122DropConsoleAccounts.cs` đã đổi `Up()` thành no-op **giữ nguyên migration ID** (8 thêm/147 xóa);
`P03PreserveConsoleCompatibility.cs:21-71` `CREATE TABLE IF NOT EXISTS`. DB chạy bản cũ có bảng đã
drop; DB mới thì không — `__EFMigrationsHistory` không phân biệt được.

**Sửa phạm vi** (Codex F03 — audit gốc coi runbook/test là việc chưa làm, sai):
`docs/database/expand-contract.md:21-32` **đã mô tả đủ 4 tình huống** (chưa drop / đã drop / phục hồi
từ pre-drop backup / partial schema). `ExpandContractMigrationTests` có 2 case và **nằm trong suite
integration vừa pass**.

**Việc còn lại (hẹp hơn nhiều):** inventory + backup + rehearsal trên **database đích thật**.
Repair schema **không** phục hồi account/session đã xóa.
**Bỏ:** hai migration trùng timestamp `20260905120000` khác full ID nên vẫn có thứ tự xác định —
chưa có bằng chứng là lỗi dependency (rút lại cảnh báo của Claude).

### B10 — Constraint DB khóa hành vi V0.3 trong khi V0.2 nói khác

`PersistenceModelConfiguration.cs:396-405` `ck_ivr_call_results_action_matches_type`:
no-answer → chỉ `NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`; window expired → chỉ `REVALIDATE_AND_EXPIRE_
CONFIRMATION`/`REVALIDATE_AND_HOLD_ADMIN_REVIEW`. **Không sửa được bằng config** — đổi hành vi phải
ra migration mới (không sửa W0172).

**Việc:** chốt semantics với Core (DR-03/DR-04); giữ mapping hiện hành tới khi contract đổi.

### B11 — Vị trí resolver `dial_token → E.164`

`ProviderPorts.cs:17-32` `DialAuthorization` gọi `OpaqueReferenceGuard.EnsureNotRawPhone`;
`Identifiers.cs:133-151` ném lỗi với chuỗi 10-12 chữ số bắt đầu `0`/`84`.

**Sửa kết luận** (Codex F06 — câu của Claude *"code làm OD-V1-18 không thi hành được"* là **quá
mạnh**): guard cấm raw phone đi qua **type đó**. Một adapter hoàn toàn có thể nhận opaque handle rồi
resolve nội bộ mà **không** đưa số thô vào `DialAuthorization` — đúng như API-04 §2 mô tả.

**Việc:** reconcile authority/threat model giữa M8-10, OD-V1-18 và spec V0.3 (mâu thuẫn tài liệu là
thật). **Không** đề xuất bỏ privacy guard để làm tài liệu đúng.

### B1 — Capacity model

`SchedulerCapacity.cs:29` `ExpectedCallDurationSeconds = 60`; model tự khai `UNCALIBRATED`.
`PT-CAP-02` (M8-P0-009) **đã có** — đừng giao lại như chưa làm.
`W-0189`/worklist 03/09 ghi **`EXTERNAL_INTAKE_DEFERRED_BY_OWNER`** — chưa có 4 nhóm input **không**
đồng nghĩa phải tự mở lại dispatch/provider intake.

**Việc:** giữ `UNCALIBRATED`; chỉ mở calibration khi owner mở lại **và** đủ dữ liệu. Xem 0.6.

### B8 — TTS self-hosted VieNeu

`gate-status.yaml` W-0122 = `BLOCKED_EXTERNAL`. `deploy/tts/`, `docker-compose.vieneu-tts*.yml`,
`MODELS.lock`, `docker-compose.softphone.yml` **không có commit nào** trong `b21ec67..HEAD`.

**Tách rõ bốn việc khác nhau** (Codex F12): owner **chọn giọng** (manifest 3 giọng `OWNER_ACCEPTED`
còn khớp) ≠ **nghe mối nối/6 cuộc thử** ≠ **target hardware** ≠ **release approval**.
Provenance gate vẫn báo `LEGAL,INTERNAL_MIRROR`.

**Việc:** theo gate từng artifact. Không mở lại phần đã ký khi binding không đổi; cũng không dùng
artifact chọn giọng để đóng lab/production.

### B12 — Telephony adapter production

DI chỉ có mock / lab / `UnavailableSchedulerDispatchGateway` (`SchedulerCapacity.cs:567-568`).
**Việc:** vendor/custody/network + lab. **Không** coi Asterisk softphone là bằng chứng real-SIM.
Đây là **chờ mua SIM**, không phải nợ code.

---

## Nhóm C — Seam với M3 (gộp theo thiết kế chung)

> **Nền tảng đã verify và rất vững:** `git diff --stat b21ec67..HEAD` trên `Contracts/Generated`,
> `TaskIntakeEndpoint.cs`, `TaskIntakeService.cs`, `PostgresSchedulerStore.cs`, `EligibilityRules.cs`
> = **rỗng**. Schema task/callback trong OAS không đổi một byte. **M3 viết producer/consumer được
> ngay** — mặt cắt đã đứng yên 62 commit và W-0204 có cơ chế đóng băng giữ nó đứng yên.

### C-SESSION — gộp C3 + C4 + C7 thành một thiết kế

Cùng một bài toán: không có `golden_hour_session_id` từ M3. Hiện `SessionId` chỉ tồn tại trên
`CapacityIncidentEntity` với giá trị tự sinh (`SCHED-`, `MOCK-SCHED-`, `ADMIN-QUEUE-`).

**Việc:** ký `golden_hour_session_id` → producer mapping → store (nullable, additive) → CDC → enforce.
**Không** map đè lên `session_id` nội bộ (giá trị tự sinh vẫn trace về job, giữ nguyên).
**Cảnh báo:** required `program_code=GH` là **breaking** dù store nullable là additive — đừng hứa
toàn bộ non-breaking.

### C-REVOKE — gộp C11 + C12 + C14 thành một lifecycle design

Cùng một lỗ hổng: recall/hủy đơn bật giữa confirmation window thì attempt 2 vẫn quay. Claim query
(`PostgresSchedulerStore.cs`) không đọc lại `sale_lock`/`recall`/`order_state`; không có endpoint
revoke. `W-0111 terminate` chỉ là admin cắt cuộc đang gọi. ACK trên callback **không** thay được
một command.

**Việc:** chọn A (chấp nhận trade-off + M3 bắt buộc D-06) / B (M3 phát revoke) / hybrid.
Nếu chọn B **phải fence tới tận trước dial** — technical lease generation hiện có **không phải**
order-revocation generation.

### C6 + C13 — Contact gate và TTL

Xem **0.1** (TTL ba tầng) và **0.2**. Phần còn lại của C13: production issuer/resolver custody chưa
có (chỉ `MockDialTokenVault`/`LabDialTokenVault`). Ledger W-0199 đã có task binding + ceiling + audit
— nhưng đó là **local ledger, không phải distributed replay proof**.
`phone_validation_status`: optional trong OAS, runtime bắt `== "VALID"` (422). Đổi thành required
enum là **thay đổi contract cần xét compatibility**, không phải sửa hiển nhiên.

### C8 + C9 — Callback ra M3

Shape đã implement và **W-0207 đã tự sửa** một mâu thuẫn ACK nguy hiểm: ma trận yêu cầu
`DUPLICATE_ACCEPTED` trên **409**, nhưng `CallbackAck409` chỉ mang `REJECTED_STALE`/
`IDEMPOTENCY_CONFLICT` ⇒ transport trả `CALLBACK_ACK_INVALID` ⇒ **`INVALID_DEAD_LETTER` terminal**.
Nếu M3 xây theo tờ cũ thì **mọi replay chính xác chết lặng**. Đã sửa về `[200]` + gate `FREEZE-06`.

Đường gửi thật vẫn tắt **có chủ đích** (`appsettings.json:85-88` Enabled=false;
`CallbackDeliveryOptions.cs:72-78` fail-closed trên W-0006/OD-V1-07).
**Việc:** M3 consumer + auth + shared E2E. Đủ auth/sandbox/approval rồi mới mở delivery thật.

### C1 · C2 · C5 · C10 — còn lại

- **C1** enum/matrix đúng; IR-06 §3.10 R3 **đã ghi nguồn business** cho hai cặp → không phải phát
  hiện business hoàn toàn mới. Việc: ký wire mapping/producer.
- **C2** xem **0.4**.
- **C5** có pack, thiếu external approval. Xem A4.
- **C10** xem **0.3**. Bỏ tiêu chí cũ *"2 lần rejected → do-not-call"* (M8-08 yêu cầu explicit-only).

---

## Nhóm D — Đã đạt, đừng làm lại

`D1`–`D10` giữ nguyên trạng thái `DONE_LOCAL`, kèm đúng giới hạn: **local đã chứng minh, external
chưa**. Chi tiết bằng chứng ở bản audit gốc và bảng 37 mục của Codex §3.

Hai điều chỉnh:

- **D1** — core intake/auth có thật, nhưng **chưa gọi là "docs/contract sạch"** được: xem 0.2 (header
  drift). Middleware có auth thực dù endpoint khai `AllowAnonymous`.
- **D8** — **891/891 local** đã xác nhận. Các số `481`/`466`/`535` trong ô cũ là **lịch sử có ngày**,
  không phải mâu thuẫn số học. `550` tagged declarations ≠ `891` executed cases (Theory mở nhiều
  case) — hai số **không thay nhau được**. Còn lại: hosted CI, UI/clean-checkout, external evidence.

---

## Nhóm X — Việc không có trong bản audit gốc

### X1 — 4 nhánh và 4 worktree, trong khi luật repo chỉ cho `main`

Bản audit lặp câu *"không nhánh nào — repo chỉ có main"* ở **16 hàng** B/C. Sai, và đã sai từ lúc
viết. Thực tế:

| Nhánh | Tip | So với `main` |
| --- | --- | --- |
| `codex/p03-expand-contract` | `d5539ba` 05/09 | ahead 0 / behind 18 → đã merge, chỉ chờ xóa |
| `worktree-gd0-fixes` | `cc12e53` 05/09 | ahead 0 / behind 15 → đã merge, chỉ chờ xóa |
| `codex/w0128-w0129-candidate` | `1fa0150` 28/08 | **ahead 1** / behind 73 |
| `origin/codex/phase-1-2-opus-remediation` | remote | — |

Cộng **4 worktree** ngoài cây chính.

**Cảnh báo (Codex F11):** commit riêng của `w0128-w0129-candidate` chạm **92 file**, không chỉ
`admin-ui/` như Claude viết. Behind 73 commit. **Đừng merge mù vì thấy "ahead 1".**

**Việc:** owner quyết từng nhánh. `CLAUDE.md` ghi *"Where a stray branch already exists, merge it
into `main` and delete it"* — nhưng với nhánh ahead 1/behind 73 thì phải review nội dung trước.

### X2 — `docker-compose.e2e.yml` vẫn dính bẫy khung giờ

`W-0203` đã thêm `appsettings.Profile.LocalMockE2E.json:64-69` đặt `CallingWindow` **0..1440** với lý
do viết thẳng trong file: *"a rehearsal whose result depends on what time it was started is not a
rehearsal"*. Nhưng đó là harness `tools/dev/Invoke-LocalMockE2E.mjs`.

`docker-compose.e2e.yml` **commit cuối `7195ba8` ngày 20/08**, không set `CallingWindow` → stack
smoke chạy ngoài 08:00–21:00 sẽ FAIL lại y hệt.

**Việc:** port cấu hình window của LocalMockE2E sang compose.
**Không kết luận thay server:** kết quả local mới **không** xóa được lịch sử smoke trên server tại
baseline khác; chưa đọc config/log triển khai thật thì chưa xác nhận nguyên nhân tuyệt đối (F09).

### X3 — Worker không log lý do khi cửa sổ giờ đóng

`SchedulerJobHost.cs:42-51` chỉ log khi có quarantine/closed/claimed.

**Sửa lập luận của Claude (F09):** quarantine và deadline-closing chạy **trước** hour gate, nên **vẫn
có thể có log** khi có việc. Khoảng trống đúng là: **worker không nói rõ "window closed, opens at …"
khi idle**.

**Việc:** thêm một dòng log/metric khi window đóng.
**Hệ quả sản phẩm cần chốt với M3:** task tới ngoài giờ gọi mà confirmation window ngắn hơn khoảng
chờ tới 08:00 thì **luôn** hết hạn không gọi.

---

## Đã đóng — bỏ khỏi hàng đợi

### ~~B2 — 4 file softphone lab dirty~~ → **hết tiền đề**

`git status --porcelain` = **rỗng**. 3 file tracked/unmodified. File thứ tư
`deploy/lab/Start-AndroidSoftphoneLab.ps1` **không tồn tại** ở bất kỳ checkout nào trên máy (đã kiểm
`main` + `.claude/worktrees/gd0-fixes` + `ivr-p03-expand-contract` + `ivr-w0128-w0129-candidate`).
Worklist 03/09 đã đóng đúng: `CLEAN_CLOSED_NA` — *"các file thuộc clone khác"*.

> ⚠️ Đề xuất *"commit vào nhánh lab"* trong bản audit gốc **vi phạm `CLAUDE.md`** và từ `9603ca5` sẽ
> **bị git hook chặn** (`.githooks/reference-transaction`, `.githooks/pre-push`). Đừng làm.

### ~~B9 — Admin UI / Monitoring~~ → **đã chuyển owner**

Worklist 03/09 đóng implementation local và chuyển operator UI/BFF cho M3.
API hiện trả tối đa **20 incident OPEN** trên dashboard; call detail có `evidence_refs`/`audit_refs`
— đủ cho handoff đã ghi nhận, **nhưng không tương đương** endpoint xem toàn bộ lịch sử incident/audit.

**Việc (nếu có):** M3 làm UI/BFF/authz. Nếu muốn lịch sử đầy đủ thì cần use case + contract riêng —
**không** tự dựng thêm hai route chỉ để giữ B9 ở nhóm code lõi, và **không** tính công việc UI của M3
vào tỷ lệ code M8 còn thiếu.

---

## Ghi chú về bản audit gốc

Giữ lại làm lịch sử, **không** dùng làm danh sách giao việc. Ba điểm bản này đã sửa:

1. **Phần trăm 29%/71% tái lập được** — B: `(0,80+0,65)/5` = 29% · C: 10 giá trị `/13` ≈ 30% ·
   tổng `(1,45+3,85)/18` ≈ 29%. Phản biện của Claude ở đây **sai**: 4 giá trị "thiếu" nằm trong các
   ô `▸ Cập nhật 07/09` của từng hàng, không phải trong danh sách tóm tắt đầu file. Dù vậy đây vẫn
   là **trung bình ước lượng chủ quan** của một danh sách cũ có mục trùng/đã đóng/chờ external —
   **không đo phần trăm module hay năng suất**.
2. **"3 phiếu questions-to-\*"** (dòng 119) là 3 phiếu **TTS** (legal/security/platform), **không**
   mâu thuẫn với "10 phiếu" ở dòng 91. Phiếu `questions-to-module-3-od18-authority.md` còn sống là
   phiếu khác — Claude gán nhầm. Bản audit **không sai** ở chỗ này.
3. **Không tìm thấy file 30/08 trong Git** chưa bác bỏ được một tài liệu tồn tại ngoài repo
   (audit đến từ phiên `D:\9 module`). Câu "nền sai" của Claude là **overreach**.

Số dòng `file:dòng` trong bản gốc chính xác cao — kiểm ~60 tham chiếu, chỉ 3 chỗ lệch
(`appsettings.json:10`→`:18` · `CallingWindow.cs:33-44`→`:31-43` ·
`TaskIntakeService.cs:406-409`→`:411-414`).
