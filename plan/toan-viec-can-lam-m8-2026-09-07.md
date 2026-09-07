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

## Bảng trạng thái

Cập nhật mỗi lần đụng vào một mục. Quy ước:

| Ký hiệu | Nghĩa |
| --- | --- |
| ✅ `XONG` | không còn việc gì, kể cả bên ngoài |
| 🟡 `XONG PHẦN M8` | phần M8 tự làm được đã xong và đã ghim bằng test; còn chờ chữ ký/đầu vào bên ngoài |
| ⬜ `CHƯA LÀM` | chưa động tới |
| ⛔ `CHỜ NGOÀI` | M8 không làm được gì thêm cho tới khi bên ngoài trả lời |
| ➖ `ĐÃ ĐÓNG` | bỏ khỏi hàng đợi, giữ ghi nhận lịch sử |

| Mục | Trạng thái | Work ID | Chờ ai |
| --- | --- | --- | --- |
| **0.1** TTL `dial_token` ba tầng | 🟡 `XONG PHẦN M8` | `W-0208` · `fb6a613` | M3/Security — `DTK-02`/`DTK-06` |
| **0.2** Header 128 vs 200 | 🟡 `XONG PHẦN M8` | `W-0209` | nội bộ M8 — chốt lệch intake↔admin |
| **0.3** `DTMF-0` ≠ opt-out | 🟡 `XONG PHẦN M8` | `W-0210` | Product + CRM/M3 + Legal/Privacy — quorum `OD-V1-23` |
| **0.4** Docs sai về DB | ✅ `XONG` | `W-0211` | — |
| **0.5** Approval theo môi trường | 🟡 `XONG PHẦN M8` | `W-0213` | Security/Platform — có cần scope theo env không |
| **0.6** `40/50/60` occupancy | ✅ `XONG` | `W-0212` | — |
| **X1** nhánh & worktree | ⛔ `CHỜ NGOÀI` | `W-0214` | Owner — không phải nhánh rác, xem X1 |
| **X2** `docker-compose.e2e.yml` | ✅ `XONG` | `W-0214` | — |
| **X3** log window-closed | ✅ `XONG` | `W-0214` | — |
| **A1**–**A4** quản trị/chữ ký | ⛔ `CHỜ NGOÀI` | — | Owner hệ + chief auditor |
| **B5** khung giờ 21:00 | 🟡 `XONG PHẦN M8` | `W-0215` | Owner + M3 — chọn End, và nguồn `LOCK-05` |
| **B4** `RUNTIME_GATE_ADMIN` | ⬜ `CHƯA LÀM` | — | Security/Platform |
| **B6** hai lịch sử `W0122` | ⬜ `CHƯA LÀM` | — | — (cần DB đích) |
| **B10** constraint V0.3 | ⛔ `CHỜ NGOÀI` | — | Order Core — `DR-03`/`DR-04` |
| **B11** vị trí resolver | ⛔ `CHỜ NGOÀI` | — | Security — trust boundary |
| **B1** capacity | ⛔ `CHỜ NGOÀI` | — | Owner (`EXTERNAL_INTAKE_DEFERRED`) |
| **B8** TTS | ⛔ `CHỜ NGOÀI` | — | Owner/Legal/procurement |
| **B12** adapter production | ⛔ `CHỜ NGOÀI` | — | procurement (SIM) |
| **C-SESSION** (C3+C4+C7) | ⛔ `CHỜ NGOÀI` | — | M3 — `golden_hour_session_id` |
| **C-REVOKE** (C11+C12+C14) | ⛔ `CHỜ NGOÀI` | — | M3 + Owner — chọn A/B/hybrid |
| **C6+C13** contact gate | 🟡 một phần qua 0.1 | `W-0208` | M3/Security |
| **C8+C9** callback | ⛔ `CHỜ NGOÀI` | — | M3 consumer + Security credential |
| **C1 · C2 · C5 · C10** | ⬜ `CHƯA LÀM` | — | phần lớn gộp vào 0.3/0.4 |
| **D1**–**D10** | ✅ `DONE_LOCAL` | — | external evidence |
| ~~**B2**~~ softphone dirty | ➖ `ĐÃ ĐÓNG` | — | hết tiền đề |
| ~~**B9**~~ Admin UI | ➖ `ĐÃ ĐÓNG` | — | đã chuyển M3 |

---

## Nhóm 0 — Sửa hướng dẫn sai TRƯỚC khi M3 viết code

> Đây là nhóm duy nhất có deadline thật. Mỗi mục dưới đây là một chỗ **tài liệu bàn giao đang dạy
> M3 làm sai**. M3 code theo tài liệu hiện tại sẽ hỏng ở runtime, không hỏng ở compile — nên càng
> để lâu càng đắt.

### 0.1 — TTL `dial_token` mâu thuẫn xuyên **ba** tầng ⚠️ nặng nhất

> **🟡 `XONG PHẦN M8`** · `W-0208` · commit `fb6a613` · 07/09
> Hành vi đã ghim (`IT-INTAKE-DB-03`), tài liệu đã sửa. **Chờ M3/Security chốt con số TTL**
> (`DTK-02`/`DTK-06`). Evidence: [`docs/evidence/W-0208`](../docs/evidence/W-0208/README.md).

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

> **🟡 `XONG PHẦN M8`** · `W-0209` · 07/09
> Ghim bằng `IT-INTAKE-HEADER-07`, IR-06 đã sửa (`§3.1.1` mới). **Còn hai việc:** chốt lệch
> intake↔admin, rồi re-pin OpenAPI cùng lượt với `W-0208`.
> Evidence: [`docs/evidence/W-0209`](../docs/evidence/W-0209/README.md).

Codex nêu một vế (F10); đọc kỹ ra **ba** con số sai, và vế thứ ba mới là vế nguy hiểm:

| | IR-06 nói | Code thi hành |
| --- | --- | --- |
| Dài tối đa | `200` | **`128`** |
| Dài tối thiểu (`Idempotency-Key`) | `8` | **`1`** — không có sàn |
| Bảng chữ cái | *không nói gì* | **chỉ `[A-Za-z0-9._:-]`** |

`+`, `/`, `=` đều bị chặn ⇒ **key base64 hỏng**, mà sinh key ngẫu nhiên bằng base64 là việc rất
thường. M3 không có cách nào biết trước — OpenAPI cũng khai `{ type: string }` trần.

**Không phải "chốt 128 hay 200".** Repo đã chốt `128` ở ba chỗ rồi
(`RequiredHeader`, `CorrelationMiddleware`, và `GeneratedCorrelationId` trong chính OAS). `200`
là con số lạc trong IR-06. Nên đây là sửa tài liệu, không phải quyết định.

**Chỗ thật sự cần quyết (mới, ngoài Codex):** cùng tên `Idempotency-Key`, hai luật trong cùng một
API — route intake ràng bảng chữ cái, route admin/internal (`InternalServiceOptions.
RequireIdempotencyKey`) thì không. Cả hai dùng chung một `$ref` trong OAS nên schema không mô tả
đúng được cả hai. Siết admin hay nới intake?

### 0.3 — `DTMF-0` bị gọi nhầm là tín hiệu opt-out

> **🟡 `XONG PHẦN M8`** · `W-0210` · 07/09
> Register đã sửa, hành vi đã ghim (`UT-OPTOUT-DTMF0-05`). **Chờ quorum `OD-V1-23`**
> (Product + CRM/M3 + Legal/Privacy). Evidence: [`docs/evidence/W-0210`](../docs/evidence/W-0210/README.md).

`OD-V1-23` (register `L64`, ký 06/09, **vẫn `QUORUM_PENDING`**) ghi *"chỉ coi là opt-out khi khách
phát tín hiệu tường minh (**DTMF-0 / handoff**)"*. Nguyên tắc "explicit-only" thì đúng và nên giữ.
**Hai ví dụ tín hiệu trong ngoặc thì sai cả hai:**

| Tín hiệu `OD-V1-23` nêu | Thực tế trong code |
| --- | --- |
| `DTMF-0` | **phím hủy đơn.** Lời thoại khóa cứng: *"bấm phím 0 để hủy"*; `DispositionMapper` → `IvrCustomerCancelled` + `RevalidateAndCancelCustomerRequest` |
| `handoff` / phím 9 | **ngoài scope** (`specs/01-context-and-scope.md:34`); `TargetV1SpeechPolicy.ValidateTemplate` **ném lỗi** với template chứa "phím 9" |

Và M8-08 §4.2 — **chính gói mà `OD-V1-23` dẫn làm closure evidence** — ghi thẳng: *"DTMF 1 là xác
nhận đơn, DTMF 0 là yêu cầu huỷ đơn. **Không tái dùng hai phím này cho opt-out**."* §4.3: *"Current
V1 không có explicit opt-out signal."*

⚠️ **Vì sao nặng hơn một lỗi tài liệu:** khách bấm 0 chỉ được nghe *"bấm phím 0 để hủy"*. Đọc thao
tác đó thành lệnh cấm liên hệ vĩnh viễn là **lấy consent khách chưa từng cho, từ một câu khách chưa
từng nghe**. Nếu CRM/M3 build theo register, khách hủy **một** đơn sẽ bị chặn gọi **mãi mãi**.

**Việc còn lại:** V1 **không có** tín hiệu opt-out tường minh nào — thêm một cái cần wording/script
mới + signal source + proof + Legal/Privacy ký riêng (M8-08 §4.3). Đây là việc của quorum, không
phải của M8. Hằng số `2/3` giữ nguyên `TEST_ONLY_CANDIDATE`, không wire (M8-08 §4.7).

### 0.4 — Tài liệu specs nói sai về chính DB của mình

- `specs/database/03-enums-and-status.md` §4 ghi *"Result type — **11** giá trị"* + *"bốn nơi …
  **đang khớp**"*. Thực tế `ck_ivr_call_results_result_type` chỉ có **9** (W-0172 đã bỏ
  `IVR_OPERATIONAL_BLOCKED`/`IVR_POLICY_BLOCKED`). Ba tập con hợp lệ: **11** vocabulary wire ·
  **9** runtime · **6** final callback · **5** counted.
- IR-06 `L8`/`L25`/`L914`/`L1129` còn hướng dẫn lấy **draft.22**; manifest hiện **draft.23**.
- IR-06 §4A.7 nói bảng console không còn trong DB — nhưng `P03` giữ/tạo lại bảng compatibility.

**Lưu ý:** FREEZE PASS chỉ xác nhận invariant mà gate kiểm (pins, field inventory, draft state,
ACK matrix) — **không** chứng minh mọi câu trong docs khớp code. Ba lỗi dưới đây đều lọt qua
`CONTRACT_FREEZE=PASS`.

#### Đã làm (W-0211, 07/09) — ✅ đóng

Cả ba đều là docs, không cần chữ ký ai, và **không cần test mới**: bốn con số `11/9/6` đã được
`UT-RESULT-CONTRACT-01` khẳng định sẵn từ `W-0172` — vấn đề chỉ là tài liệu nói khác code, và giờ
tài liệu trỏ thẳng vào test đó làm neo.

- **`specs/database/03-enums-and-status.md` §4** — thay *"11 giá trị / bốn nơi đang khớp"* bằng bảng
  **bốn tập lồng nhau**: vocabulary `11` · runtime `9` · final callback `6` · counted `5`, kèm nơi
  thi hành từng tập và câu hỏi nó trả lời. Bốn nơi đó mang `11/9/6/11` và **không được phép** khớp
  nhau — đó mới là thiết kế. Ghi rõ vì sao `OPERATIONAL_BLOCKED`/`POLICY_BLOCKED` là quyết định
  *trước* cuộc gọi.
- **IR-06 `draft.22` → `draft.23`** ở 5 chỗ (`L8`, `L25`, tiêu đề bảng, `§4A.7`, checklist `L1203`),
  thêm dòng changelog `.22→.23` còn thiếu. Tiện thể: chính bản `.22→.23` là nơi route feature-flag
  nhận `minLength:1/maxLength:128/pattern` cho `x-correlation-id` — **cùng luật mà route intake vẫn
  chưa khai** (0.2), nên đã liên kết chéo hai mục.
- **IR-06 §4A.7** — dòng *"Bảng tài khoản console … đã xoá"* đúng ở tầng API, **sai ở tầng DB**:
  `W0122.Up()` nay rỗng và `P03` chạy `CREATE TABLE IF NOT EXISTS` cho `ivr_console_accounts` +
  `ivr_console_sessions`. Hai bảng **vẫn tồn tại**, cố ý, cho cửa sổ rollback helm. Đã tách "gỡ khỏi
  runtime/API" khỏi "xoá khỏi DB" để ai soi DB không tưởng là tàn dư bỏ quên.

**Không đụng OpenAPI** — `ResultType` ở đó **đã đúng**: enum 11 giá trị kèm `description` giải thích
IVR không phát hai mã pre-call. Chỉ `specs/database` sai.

### 0.5 — `environment` scope một loại approval, và trơ với hai loại kia

> **🟡 `XONG PHẦN M8`** · `W-0213` · 07/09
> Đã ghim (`IT-GATE-APPROVAL-10`) và ghi vào code. **Chờ Security/Platform** trả lời: admin gate có
> **cần** scope theo môi trường không. Evidence: [`docs/evidence/W-0213`](../docs/evidence/W-0213/README.md).

⚠️ **Sửa chính tiêu đề cũ của mục này.** Bản trước viết *"ngữ nghĩa approval theo môi trường **chưa
có thật trong code**"* — **quá đà**, và tôi đã tự khái quát từ một câu Codex viết đúng phạm vi hơn.
Ngữ nghĩa đó **có thật**, cho một trong ba loại:

| Loại approval | Đọc bởi | Lọc environment? |
| --- | --- | --- |
| `FEATURE_FLAG_CHANGE` | `PostgresFourEyesApprovalVerifier.VerifyAsync` | **Có, hai lớp** — `AND environment = {3}` trong SQL, **và** `RuntimeGateFingerprint.Of()` băm `snapshot.Environment` làm trường đầu tiên |
| `RUNTIME_GATE_ADMIN` | `RuntimeGateApprovalReader.AnyLiveAsync` | **Không** — chỉ kind + revoked + expires |
| `PRODUCTION_CALL` | `AnyLiveAsync` | **Không** (nhưng không migration nào seed) |

Nên approval cho một thay đổi cờ ở lab **không thể** dùng lại cho production, kể cả khi ai đó xài
lại `approval_reference` — fingerprint đã khác trước khi tới predicate cột.

**Cái thật sự là bẫy:** cột `environment` được **ghi** cho cả ba loại nhưng chỉ được **đọc** cho một
loại, và không có gì trong schema nói điều đó. Ai chèn một row `RUNTIME_GATE_ADMIN` với
`environment='lab'` sẽ tưởng mình đã giới hạn — không hề.

Thêm một tầng nữa mà `W-0213` phát hiện khi test đỏ lần đầu: **không sửa được cột trên row đã cấp**.
Trigger append-only từ chối thẳng (`P0001: a granted runtime gate approval is immutable; only
revocation may change`). Nên "thu hẹp" một admin grant sau khi cấp là **bất khả**; chỉ có revoke rồi
cấp lại — mà cấp lại cũng chẳng giới hạn được gì.

**Việc còn lại (Security/Platform):** admin gate có **cần** scope theo môi trường không? Nếu có thì
phải sửa **cả contract + reader + test**, không chỉ đổi giá trị cột. Nếu không thì giữ nguyên và
ghi rõ vào `OD-V1-20` rằng cột này cố ý trơ cho hai loại đó.

### 0.6 — `40/50/60` không phải ba cách viết của một con số

`40s`/`60s` là **occupancy**; `50s` là **full cycle đã cộng cooldown**. Yêu cầu "đưa cả ba về một
số" (có trong audit gốc) sẽ sai đơn vị hoặc cộng cooldown hai lần. `CAP-DRIFT-05` vừa pass đúng với
trạng thái `DECLARED_DISAGREEMENT`.

Phát hiện của Codex (F05).

#### Đã làm (W-0212, 07/09) — ✅ đóng

Đi kiểm thì **code và `docs/capacity-model.md` §4a đã đúng sẵn từ `W-0132`** — cả hai đều nói rõ
occupancy vs full cycle và cảnh báo đừng làm ba số bằng nhau. Gate cũng nói thẳng:
`CAP-DRIFT-05 PASS_DECLARED_DISAGREEMENT — … They disagree **by design**`. Chỗ sai duy nhất là câu
chữ trong bản audit gốc, mà bản sạch này đã sửa từ `cf4bd4a`.

**Nhưng đọc kỹ ra con số thứ tư.** Spec V0.3 §14 viết một **cặp**, không phải riêng chu kỳ:

| Spec ghi | Giá trị |
| --- | --- |
| `AVERAGE_CALL_DURATION` | **35s** |
| `CONSERVATIVE_CALL_CYCLE` | **50s** |
| SIM cooldown | **5s** |

`W-0132` gom `40/50/60` và **bỏ sót `35`** — ba trong bốn con số cùng họ được gate canh, một con số
thì không. Và ba con số của **chính spec** không khớp nhau: `35 + 5 = 40`, **không phải 50**.
Mười giây chênh không có nguồn.

Đã làm: khai báo `specAverageCallSeconds: 35` vào `CALL_DURATION_ASSUMPTIONS`, ghim nó trong
`CAP-DRIFT-05`, sửa comment sai của gate (*"The spec never writes 50s down"* — spec **có** viết, cả
hai số), và cập nhật §4a thành bốn con số. **Không đổi một giá trị nào**, không ép quan hệ nào giữa
`35` và `50` — chọn đáp án cho mười giây đó là tuyên bố một phép đo, đúng thứ constant này sinh ra
để không làm. `W-0008` giải quyết.

Bất biến hành vi giữ nguyên: `CAP-MODEL-01` 21 kênh, `CAP-SENS-02` 27 corner / `7..72`.

**Lưu ý cho lúc calibrate:** luật hiện hành đặt chu kỳ spec `= occupancy + cooldown` (`+5`), còn cặp
lịch sử của spec dùng `+15`. Khi có số đo, `50` sẽ đổi — hệ quả đã biết, không phải drift.

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

> **🟡 `XONG PHẦN M8`** · `W-0215` · 07/09
> Đã ghim tương tác bằng `UT-SCH-WINDOW-09` và công bố hai mốc cắt cho M3 (IR-06 §3.4.2).
> **Chờ owner chọn `End`**, và chờ **nguồn cho `LOCK-05`**.
> Evidence: [`docs/evidence/W-0215`](../docs/evidence/W-0215/README.md).

#### Con số thật, suy ra từ hai chữ ký chứ không phải gõ tay

`OD-V1-16` ký khung giờ đóng lúc `21:00`. `OD-V1-08` ký attempt 2 ở `T0+150s` (GH) và `T0+450s`
(24/7). **Tích của hai quyết định đó là một mốc cắt chưa ai viết ra:**

| Program | Offset attempt 2 | Mốc cắt (T0 muộn nhất còn đủ 2 cuộc) |
| --- | ---: | ---: |
| `TWENTY_FOUR_SEVEN` | `450s` | **20:52:30** |
| `GOLDEN_HOUR` | `150s` | **20:57:30** |

⚠️ **Chương trình tên "24/7" lại có mốc cắt sớm hơn** — sớm hơn Giờ Vàng 5 phút. Tên đó nói về lúc
Sales **nhận đơn**, không phải lúc IVR **được gọi**; khung giờ không theo program, nên attempt 2 dài
450s phải vượt cùng một mốc 21:00.

**Hệ quả nặng nhất không phải "mất một cuộc gọi".** Kết quả gửi Sales **đổi loại**: hết attempt →
`IVR_NO_ANSWER_FINAL` (`NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`); chưa hết attempt mà window hết →
`IVR_CONFIRMATION_WINDOW_EXPIRED` (`REVALIDATE_AND_EXPIRE_CONFIRMATION` / `HOLD_ADMIN_REVIEW`).
**Cùng một hành vi khách hàng, hai kết quả khác nhau, quyết bởi đồng hồ treo tường.**

**Cảnh báo giữ nguyên:** "1 tham số + 1 test" là **dự đoán**, không phải estimate đã chứng minh
(Codex F09). Và `LOCK-05 (20:15–21:00)` **vẫn không có nguồn nào trong repo M8** — nếu owner chọn
`End ≥ 21:05` thì phải kèm nguồn, đừng lấy con số từ một bản audit.

**Việc còn lại — owner + M3:**

1. Chọn `End`. Nếu muốn đơn đặt tới đúng 21:00 vẫn đủ 2 cuộc thì `End ≥ 21:07:30` (theo 24/7), chứ
   không phải `21:05`.
2. Hoặc M3 ngừng phát task từ `20:52:30` (24/7) / `20:57:30` (GH).
3. Dù chọn gì: `UT-SCH-WINDOW-09` suy mốc cắt từ policy + window, nên nó **sẽ đỏ** khi một trong hai
   đổi — sửa có chủ đích.

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

**Cập nhật `W-0213`:** đề xuất *"gate seed theo environment (chỉ dev/lab)"* trong bản audit gốc
**không thi hành được** theo hai cách độc lập — reader không đọc cột đó (0.5), và trigger append-only
từ chối sửa cột trên row đã cấp. Đề xuất *"thêm test khẳng định prod không có RUNTIME_GATE_ADMIN sau
migration"* cũng sai tiền đề: row seed mang `environment=NULL` và áp mọi môi trường theo thiết kế,
nên không có trạng thái "prod không có" để khẳng định. Cái thay thế được là `IT-GATE-APPROVAL-10`:
ghim rằng cột trơ, để không ai tưởng nó là một công tắc.

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
**Thêm vào phạm vi B1 từ `W-0212`:** khi `W-0008` có số đo, phải giải quyết luôn **mười giây** chênh
trong chính spec (`AVERAGE_CALL_DURATION 35s` + cooldown `5s` ≠ `CONSERVATIVE_CALL_CYCLE 50s`) —
giờ đã được khai báo và ghim, nhưng chưa ai giải thích được.

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

> **⛔ `CHỜ NGOÀI`** · `W-0214` · 07/09 · **không xóa gì cả — xem lý do bên dưới**

| Nhánh | Tip | So với `main` | Là gì |
| --- | --- | --- | --- |
| `codex/w0128-w0129-candidate` | `1fa0150` 28/08 | **ahead 1** / behind 80 | **mốc bằng chứng cố ý** của `W-0130` |
| `codex/p03-expand-contract` | `d5539ba` 05/09 | ahead 0 / behind 25 | đã merge; còn được dẫn trong evidence `W-0197` |
| `worktree-gd0-fixes` | `cc12e53` 05/09 | ahead 0 / behind 22 | đã merge vào `main` tại `ba43605` |
| `origin/codex/phase-1-2-opus-remediation` | remote | — | chưa kiểm |

⚠️ **Sửa chính mục này (`W-0214`).** Bản trước viết *"ahead 0 → đã merge, **chỉ chờ xóa**"* và trích
`CLAUDE.md` *"merge it into `main` and delete it"*. **Luật đó nói về nhánh rác. Đây không phải nhánh
rác.**

`docs/evidence/W-0130/README.md` dựng `codex/w0128-w0129-candidate` **có chủ đích** làm mốc
provenance — tracker ghi lý do: *"`main@2a4f45d` là mixed `save` 98 file nên **không đủ
provenance**"*. File evidence khai đích danh cả **branch** lẫn **đường dẫn worktree**
`Desktop/ivr-w0128-w0129-candidate`, cùng tree hash. `ahead 1` là **thiết kế**, không phải việc chưa
dọn. Xóa nhánh = commit thành unreachable = **đứt chuỗi bằng chứng mà `W-0130` sinh ra để giữ**.

Hai nhánh `ahead 0` thì xóa ref không mất commit nào (đều reachable từ `main`), nhưng cả hai đều
đang được tài liệu dẫn tên, và **cả ba đều đang được checkout trong worktree** — muốn xóa nhánh thì
phải gỡ worktree trước, tức **xoá thư mục**, trong đó hai thư mục nằm **ngoài repo này** trên
Desktop.

**Việc — owner quyết, ba câu hỏi tách rời:**

1. `codex/w0128-w0129-candidate` có còn cần làm mốc provenance không? Nếu còn → **giữ nguyên**, và
   sửa `CLAUDE.md`/worklist để nhánh này được miễn trừ tường minh thay vì mỗi lần audit lại bị báo
   là rác.
2. Hai nhánh `ahead 0` có xóa ref không? (An toàn về commit; chỉ làm tài liệu dẫn tên bị treo.)
3. Bốn worktree — trong đó `Desktop/ivr-p03-expand-contract` và `Desktop/ivr-w0128-w0129-candidate`
   nằm ngoài repo — có gỡ không? **Tôi không tự xoá thư mục ngoài repo.**

### X2 — `docker-compose.e2e.yml` vẫn dính bẫy khung giờ

`W-0203` đã thêm `appsettings.Profile.LocalMockE2E.json:64-69` đặt `CallingWindow` **0..1440** với lý
do viết thẳng trong file: *"a rehearsal whose result depends on what time it was started is not a
rehearsal"*. Nhưng đó là harness `tools/dev/Invoke-LocalMockE2E.mjs`.

`docker-compose.e2e.yml` **commit cuối `7195ba8` ngày 20/08**, không set `CallingWindow` → stack
smoke chạy ngoài 08:00–21:00 sẽ FAIL lại y hệt.

> **✅ `XONG`** · `W-0214` · 07/09
> Đã port sang `docker-compose.e2e.yml`: `CallingWindow` **Enabled=true**, `0..1440`, UTC+420.
> Giữ **bật** chứ không tắt — tắt thì thôi không còn kiểm cái code ra quyết định nữa.
> Evidence: [`docs/evidence/W-0214`](../docs/evidence/W-0214/README.md).

**Không kết luận thay server:** kết quả local **không** xóa được lịch sử smoke trên server tại
baseline khác; chưa đọc config/log triển khai thật thì chưa xác nhận nguyên nhân tuyệt đối (F09).

### X3 — Worker không log lý do khi cửa sổ giờ đóng

`SchedulerJobHost.cs:42-51` chỉ log khi có quarantine/closed/claimed.

**Sửa lập luận của Claude (F09):** quarantine và deadline-closing chạy **trước** hour gate, nên **vẫn
có thể có log** khi có việc. Khoảng trống đúng là: **worker không nói rõ "window closed, opens at …"
khi idle**.

> **✅ `XONG`** · `W-0214` · 07/09
> `SchedulerRunResult` nay mang thêm `CallingWindowOpensAt`; `SchedulerJobHost` log **hai chiều
> chuyển trạng thái** kèm giờ mở lại. Chỉ log lúc **đổi trạng thái**, không log mỗi vòng — poll
> 100ms dưới profile `LocalMockE2E` thì một dòng mỗi vòng sẽ chôn vùi chính cái đêm nó cần giải
> thích. Ghim bằng `UT-SCH-WINDOW-07` (đóng → có `OpensAt`) và `UT-SCH-WINDOW-08` (mở → `null`).

Ghi chú: `CallingWindowDecision.Describe()` đã có sẵn câu chữ đúng từ `W-0198` nhưng **không có
caller nào trong `src/`** — chỉ một test gọi. Thông điệp đã tồn tại, chỉ chưa ai phát ra.

**Hệ quả sản phẩm cần chốt với M3 (chưa đóng):** task tới ngoài giờ gọi mà confirmation window ngắn
hơn khoảng chờ tới 08:00 thì **luôn** hết hạn không gọi. Log mới làm nó **nhìn thấy được**, không
làm nó hết là vấn đề.

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
