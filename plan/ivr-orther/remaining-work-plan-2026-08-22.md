# Kế hoạch phần việc còn lại — IVR Order Confirmation

Trạng thái: `PROPOSED` · Lập: `2026-08-22` · Người lập: Claude (Opus 5) theo yêu cầu owner
Phương pháp: đọc trực tiếp source `src/**`, `admin-ui/**`, `specs/**`, `docs/**`,
`prompt/_execution/prompt-execution-tracker.md`; **không** suy luận từ báo cáo.

> **File này không phải tracker thứ hai.** Nguồn tiến độ duy nhất vẫn là
> [`prompt/_execution/prompt-execution-tracker.md`](../../prompt/_execution/prompt-execution-tracker.md).
> File này chỉ trả lời một câu hỏi: **còn thiếu gì, và nên làm theo thứ tự nào.**
> Mỗi hạng mục ở §4 khi bắt đầu mới cấp Work ID thật từ `NEXT_WORK_ID`; ở đây dùng
> mã tạm `G-xx` để không đụng bộ đếm đang được ba luồng khác dùng.

---

## 0. Cách đọc file này

| Nhóm | Ý nghĩa | Ai gỡ được |
| --- | --- | --- |
| **A** | Thiếu thật, **IVR tự làm được ngay**, không chờ ai | Dev IVR |
| **B** | Thiếu vì **cổng ngoài** chưa đóng | Sales / Infra / Legal / Owner |
| **C** | **Cố ý hoãn** — đã có quyết định, không phải lỗ hổng | — |
| **D** | **Trôi tài liệu / vệ sinh** — nhỏ nhưng gây hiểu sai | Dev IVR |

Mỗi hạng mục nhóm A có: bằng chứng (file\:dòng), tại sao là lỗ hổng, việc phải làm,
tiêu chí nghiệm thu, và phụ thuộc.

---

## 1. Kết luận một trang

**Phần khung đã xong, phần "khách nghe được đơn của chính mình" thì chưa.**

Đối chiếu contract và spec cho ra kết quả tốt hơn mong đợi ở lớp API và màn hình:

- **34/34 path** trong [`ivr-order-confirmation.v1.yaml`](../../specs/api/openapi/ivr-order-confirmation.v1.yaml)
  đều có handler thật trong `src/Ivr.Api`. Không endpoint nào bị khai mà chưa dựng.
- **8/8 màn** trong `specs/ui/` đều có route trong `admin-ui/src/app/(console)/`, cộng
  6 route phát sinh từ W-0105 (`/accounts`, `/accounts/[id]`, `/profile`, `/login`, `/queue`, `/reports/export`).
- **0 `TODO` / `FIXME` / `NotImplementedException`** trong toàn bộ `src/` và `admin-ui/src/`.
- Readiness probe đã là fail-closed thật (`503` khi DB unreachable/schema_behind hoặc
  callback circuit hở) — [`IvrReadinessProbe.cs:16`](../../src/Ivr.Api/Health/IvrReadinessProbe.cs).

Nhưng có **một lỗ hổng chức năng lớn** và **ba lỗ hổng vừa**, tất cả đều tự làm được:

| # | Thiếu | Vì sao quan trọng |
| --- | --- | --- |
| **A1** | **Đường ống ghép audio động chưa có.** Cuộc gọi hiện phát **đúng một file WAV cố định**, không chứa dữ liệu đơn của khách | Đây là **lý do tồn tại** của hệ thống. Không có nó thì không thể pilot |
| **A2** | **Script lifecycle có domain nhưng không có API và không có màn hình** | Không ai duyệt được kịch bản từ console ⇒ cổng Legal (`G-LEGAL`) không có đường thi hành |
| **A3** | **Không có màn hình feature-flag / runtime gate** | `OD-V1-20` vừa cấp quyền `IVR_RUNTIME_GATE_ADMIN` cho Admin, nhưng quyền đó không có nút nào để bấm |
| **A4** | **Không có cách cắt ngang cuộc đang gọi** | Kill switch chặn cuộc **mới**; cuộc đang nói chuyện với khách thì không dừng được |

Ba lỗ hổng A2/A3/A4 đều là **an toàn vận hành**, không phải tính năng phụ. A1 là
đường găng của toàn bộ lịch pilot.

---

## 2. Luồng đang chạy song song — **KHÔNG làm lại**

Rà `git status` và transcript các phiên đang mở tại thời điểm lập file. **Ba luồng đang
sửa code**, và bốn file dưới đây đang có thay đổi chưa commit. Bất kỳ ai nhận việc từ
plan này **phải tránh chạm vào chúng** cho tới khi luồng tương ứng commit xong.

| Luồng | Đang làm | File đang giữ | Đụng vào plan này ở đâu |
| --- | --- | --- | --- |
| **L1 — `OD-V1-20` quyền Admin** | Thêm `IVR_FLAG_READ` + `IVR_RUNTIME_GATE_ADMIN` vào role `Admin`, cập nhật spec/i18n/test | `src/Ivr.Api/Auth/IvrRoles.cs`, `admin-ui/src/i18n/vi.json`, `tests/component/back-office.test.tsx`, `tests/e2e/back-office-screens.test.ts`, `tests/e2e/console-auth-stub.ts`, `tests/unit/contract-drift.test.ts` | **A3 phụ thuộc luồng này.** A3 chỉ dựng **màn hình**; tuyệt đối không sửa `IvrRoles.cs` |
| **L2 — Bố cục sidebar** | Đưa khối tài khoản/đăng xuất xuống đáy sidebar | `admin-ui/src/components/shell/**` (chưa commit tại thời điểm rà) | A3 thêm route mới vào nav ⇒ **chờ L2 commit** rồi mới thêm mục nav |
| **L3 — Hậu kiểm W-0107 + audio lab** | Soát 6 giá trị enum "không có nhãn", phát hiện 2 field spec khai `type: string` (không `enum:`) nên guard không thấy; đồng thời vá culture số thập phân trong script audio lab | `admin-ui/src/i18n/enums.vi.json` (khả năng), `deploy/lab/Convert-LabVoiceAudio.ps1`, `docs/evidence/W-0106/phase-4-lab-runbook.md` | **A6/A7 đụng W-0106.** Chờ L3 xong mới sửa file lab |

**Việc đã xong rồi, đừng đưa lại vào plan** (đã kiểm chứng trong source, không phải theo lời kể):

- ✅ Đăng nhập username/password + session opaque + 2 role + CRUD account (W-0105) — đã commit.
- ✅ Việt hóa dữ liệu: `enums.vi.json` **39 họ**, `EnumLabel.tsx`, `lib/i18n/enum.ts` (W-0107) — đã có trong cây làm việc.
- ✅ Phân miền 34 tỉnh + đọc số tiền/số lượng bằng chữ: `DeliveryRegionResolver`,
  `VietnameseNumberSpeller`, `VietnameseTextNormalizer` (W-0106 GĐ 2) — đã có.
- ✅ Định tuyến giọng theo miền + cache tách theo giọng + telemetry (W-0106 GĐ 3) — đã có.
- ✅ Trường `voice_region` trên màn chi tiết cuộc gọi (W-0106 GĐ 5) — đã có.
- ✅ Job retention, ETL analytics, outbox callback, scheduler, normalizer — đều có host thật trong `src/Ivr.Worker/Jobs/`.

---

## 3. Hiện trạng đã xác minh

### 3.1 Backend — đủ so với contract

| Nhóm | Số path | Trạng thái |
| --- | --- | --- |
| Intake | 1 (`/tasks`) | ✅ [`TaskIntakeEndpoint.cs:18`](../../src/Ivr.Api/Intake/TaskIntakeEndpoint.cs) |
| Internal lifecycle | 6 | ✅ [`InternalLifecycleEndpoints.cs`](../../src/Ivr.Api/Internal/InternalLifecycleEndpoints.cs) |
| Admin read | 11 | ✅ [`IvrAdminEndpoints.cs`](../../src/Ivr.Api/Admin/IvrAdminEndpoints.cs) |
| Admin mutation | 6 | ✅ cùng file |
| Feature flag | 3 | ✅ [`FeatureFlagEndpoint.cs`](../../src/Ivr.Api/Admin/FeatureFlagEndpoint.cs) |
| Auth + account | 10 | ✅ W-0105 |
| Health | 3 | ✅ [`HealthEndpointRouteBuilderExtensions.cs`](../../src/Ivr.Api/Health/HealthEndpointRouteBuilderExtensions.cs) |

Lệnh kiểm chứng ở §9.

### 3.2 Frontend — đủ màn, thiếu **hành động ghi**

| Màn spec | Route | Ghi được chưa |
| --- | --- | --- |
| UI-01 Dashboard | `/dashboard` | đọc — đúng spec |
| UI-02 Call log | `/calls` | đọc — đúng spec |
| UI-03 Call detail | `/calls/[id]` | ✅ có technical retry + admin review |
| UI-04 Script config | `/config` | ❌ **chỉ đọc** — spec khai **7 action ghi**, xem A2 |
| UI-05 Integration status | `/integration` | đọc — đúng spec (spec ghi rõ view-only) |
| UI-06 Review & retry | `/review` | ✅ đủ action |
| UI-07 Seed/mock | `/seed` | ❌ **chỉ đọc** — spec khai 3 action, xem A5 |
| UI-08 Role/permission | `/roles` | đọc + `/accounts` CRUD — đúng spec |
| *(không có spec màn)* | — | ❌ **thiếu hẳn màn feature-flag**, xem A3 |

### 3.3 Kiểm thử

`tests/Ivr.UnitTests` 34 file · `tests/Ivr.IntegrationTests` 38 file ·
`tests/Ivr.ContractTests` 2 file · `tests/chaos` 8 file · `admin-ui/tests` 21 file test.
Con số pass gần nhất ghi ở tracker `A-0322`: .NET `675/675`, admin-ui `200/200`.

---

## 4. Nhóm A — thiếu thật, tự làm được

### A1 · Chuỗi ghép audio động (fixed + biến thiên)

> **Ưu tiên: 🔴 CAO NHẤT.** Đây là việc then chốt của pilot.
>
> ✅ **ĐÃ TRIỂN KHAI `2026-08-22` — `W-0108`, trạng thái `CODE_DONE`.**
> Bằng chứng: [`docs/evidence/W-0108/README.md`](../../docs/evidence/W-0108/README.md).
> 4/5 tiêu chí nghiệm thu đã đạt bằng test tự động; tiêu chí thứ 5 (nghe thử MicroSIP) chờ 12
> file MP3 mà chỉ owner render được. Integration/contract/chaos **chưa chạy** — 3 process dev
> đang khoá DLL.

**Bằng chứng.**

- [`StaticFileTtsProvider.cs`](../../src/Ivr.Infrastructure/Speech/StaticFileTtsProvider.cs)
  trả về **đúng một** `mediaReference` + `durationSeconds` cho mỗi `VoiceId`. Nghĩa là
  toàn bộ cuộc gọi phát **một file cố định**.
- [`ConfigurableExternalTtsProvider.cs:18`](../../src/Ivr.Infrastructure/Speech/ConfigurableExternalTtsProvider.cs)
  ghi rõ *"Configuration seam reserved for P8-1. It intentionally contains no vendor
  protocol or SDK"* và luôn ném `TTS_NOT_CONFIGURED`.
- [`W-0106 plan §4.6`](W-0106-regional-voice-routing-plan.md) tự ghi: *"W-0106 **chưa**
  implement hybrid. Hybrid là work item riêng, gắn với `P8-1`/`OD-V1-19`."*

**Vì sao là lỗ hổng.** `VietnameseOrderScriptRenderer` **đã sinh đúng câu** cho từng đơn
(tên khách, mã đơn, món, tổng tiền bằng chữ, vùng giao). Nhưng không có gì biến câu đó
thành **âm thanh**. Cuộc gọi lab hiện tại khách nghe một bản thu chung — hay để kiểm
DTMF và disposition, nhưng **không chứng minh được** khách nghe đúng đơn của mình. Mọi
nghiệm thu "gọi thật thành công" trước khi có A1 đều đang nghiệm thu một thứ khác.

**Việc phải làm.**

1. **Tách script thành đoạn.** Mở rộng `SpeechScript` thành danh sách `SpeechSegment`
   với hai loại: `Fixed` (khóa theo id đoạn + miền) và `Dynamic` (khóa theo nội dung).
   Theo đo đạc §4.6 của W-0106: 203/300 ký tự là cố định (68%).
2. **Pre-render đoạn cố định.** 4 đoạn × 3 miền = 12 file, ghim SHA-256 vào
   `manifest.txt`, nhúng vào image Asterisk. Chi phí runtime = 0, và **nội dung đơn
   không rời mạng nội bộ** ở phần này.
3. **Provider cho đoạn biến thiên.** Hiện thực `ConfigurableExternalTtsProvider` thật
   (HTTP + credential từ secret provider, không hard-code vendor). `AudioCache` đã ghép
   sẵn `summaryHash` + `VoiceId` nên cache hoạt động **không cần sửa** — cần test
   chứng minh, không được giả định.
4. **Phát danh sách.** `AsteriskAriSimGateway` hiện phát một media; phải phát **playlist**
   theo thứ tự đoạn, và fail-closed nếu **bất kỳ** đoạn nào thiếu (tuyệt đối không phát
   thiếu đoạn rồi coi là thành công).
5. **Đóng luôn A7** (số thập phân) trong cùng đợt — xem A7.

**Tiêu chí nghiệm thu.**

- Hai đơn khác nhau ⇒ hai chuỗi audio khác nhau, kiểm bằng hash danh sách media.
- Thiếu một đoạn ⇒ ném lỗi có mã, **không** phát cuộc gọi thiếu nội dung.
- Cache ấm: đơn thứ hai cùng nội dung ⇒ **0** lần gọi vendor (đếm bằng telemetry).
- Gọi thử MicroSIP 3 miền: nghe đúng tên/món/số tiền của đơn tương ứng, Bắc đọc
  "nghìn", Trung/Nam đọc "ngàn".
- Test: đoạn cố định thiếu file ⇒ fail-start, không fail lúc đang gọi.

**Phụ thuộc.** Phần **đoạn cố định** làm được ngay. Phần **đoạn biến thiên** cần
`OD-VOICE-01` (mua gói) — nhưng viết adapter + test bằng fake provider thì không chờ.

**Ước lượng.** 4–6 ngày công (2 cho segment model + playlist, 2 cho provider + cache, 1–2 cho lab evidence).

---

### A2 · Script lifecycle: API + ràng buộc quyền + màn hình

> ✅ **ĐÃ TRIỂN KHAI `2026-08-23` — `W-0109`, trạng thái `TESTS_PASS`.**
> Bằng chứng: [`docs/evidence/W-0109/README.md`](../../docs/evidence/W-0109/README.md).
> Không cần role thứ ba: four-eyes đã có sẵn theo `ActorId`, thi hành ở hai chỗ độc lập.
> Màn read-only là **quyết định có chủ đích của W-0096**, nên A2 là đảo ngược quyết định đó
> chứ không phải vá một lỗ hổng.

> **Ưu tiên: 🔴 CAO.** Không có nó thì cổng Legal không có đường thi hành.

**Bằng chứng.**

- Domain **đã có đủ**: [`ScriptContentContracts.cs:26-32`](../../src/Ivr.Domain/Scripts/ScriptContentContracts.cs)
  khai 7 quyền `ivr.script.edit` … `ivr.script.retire`;
  [`InMemoryScriptRegistry.cs:151`](../../src/Ivr.Infrastructure/Scripts/InMemoryScriptRegistry.cs)
  có `ApproveAsync`, dòng `198` có `RetireAsync`, kèm luật "creator không tự duyệt".
- Nhưng `grep -rn "ivr\.script\." src/Ivr.Api admin-ui/src specs/api/openapi` ⇒ **0 hit**.
- [`IvrPermissions.cs:7-19`](../../src/Ivr.Api/Auth/IvrPermissions.cs) có 13 quyền, **không
  quyền nào là `ivr.script.*`** ⇒ không role nào gọi được các hàm trên.
- OpenAPI chỉ có `GET /scripts`. Màn `/config` gắn callout `config-read-only`.
- [`specs/ui/04-ivr-menu-config.md`](../../specs/ui/04-ivr-menu-config.md) §Actions khai
  **7 action ghi**: create draft, submit review, approve MOCK/LAB/CONTENT/PRIVACY_LEGAL, retire.

**Vì sao là lỗ hổng.** Có một cỗ máy lifecycle hoàn chỉnh **không có công tắc**. Hệ quả
cụ thể: W-0106 sinh script `v3-test-approved`, và cổng `G-LEGAL` yêu cầu Legal/Privacy
ký duyệt nội dung — nhưng **không có đường nào** để chữ ký đó đi vào hệ thống ngoài sửa
tay dữ liệu. Sửa tay thì mất audit, mất `creator ≠ approver`, mất luôn ý nghĩa của cổng.

**Cập nhật `2026-08-22` sau khi đọc kỹ source — hai điểm sửa lại chính phân tích trên.**

1. **`OD-SCRIPT-01` đã được trả lời sẵn trong code, không cần quyết định mới.** Four-eyes ràng
   buộc theo `ActorId` chứ không theo role, và được thi hành ở **hai** chỗ độc lập:
   `InMemoryScriptRegistry.EnsureApprovalAllowed` chặn lúc ghi (creator ≠ approver, và Content
   approver ≠ Privacy/Legal approver), còn `ScriptApprovalPolicy.ProductionAllows` kiểm lại lúc
   đọc. Không cần role thứ ba. Hệ quả vận hành phải nói rõ: four-eyes = **hai tài khoản console
   khác nhau**, và hệ thống **không phân biệt được** một người Pháp chế với một Admin bất kỳ —
   muốn phân biệt thì đó là role thứ ba và là một work item khác.

2. **Màn `/config` read-only là quyết định có chủ đích, không phải sót.**
   [`AdminConfigReadService.cs`](../../src/Ivr.Api/Application/AdminConfigReadService.cs) ghi
   nguyên văn: *"Script lifecycle transitions stay in `IScriptContentManager` and are not
   exposed: approval is an owner action governed by `OD-V1-15`, not a console button."* Bản rà
   soát ban đầu gọi đây là "cỗ máy lifecycle không có công tắc" — đúng về hiện trạng, **sai về
   nguyên nhân**. A2 là **đảo ngược một quyết định đã ghi**, nên cần owner gật đầu tường minh
   chứ không phải vá một lỗ hổng.

   Hai cách đọc đều hợp lệ: (a) mở 5 endpoint + màn hình, để chữ ký đi qua khuôn admin mutation
   có audit/`reason`/four-eyes sẵn có; (b) giữ ngoài console, chữ ký đi qua một công cụ vận hành
   riêng có audit riêng. Lập luận cho (a): hôm nay đường duy nhất là **sửa tay dữ liệu**, mà sửa
   tay thì mất cả ba thứ đó.

3. **Hạ tầng backend đã đủ, A2 thuần phần bề mặt.** `IScriptContentManager` đã đăng ký DI cho
   **cả hai** đường MOCK (`InMemoryScriptRegistry`) và bền vững (`PostgresScriptRegistry`), và cả
   hai đều hiện thực đủ 4 method lifecycle. Ước lượng 3–4 ngày giữ nguyên.

**Việc phải làm.**

1. ~~Quyết định `OD-SCRIPT-01`~~ — đã trả lời sẵn trong code, xem điểm 1 ở trên. Việc còn lại là
   owner gật đầu cho điểm 2: có đảo ngược quyết định read-only của W-0096 hay không.
2. 5 endpoint mới dưới `/scripts`: `POST /scripts` (draft), `POST /scripts/{id}:submit`,
   `POST /scripts/{id}:approve`, `POST /scripts/{id}:retire`, `GET /scripts/{id}`.
   Cùng khuôn với admin mutation hiện có: `reason` + `X-Actor-Id` + `Idempotency-Key` +
   audit + `no_policy_bypass=true`.
3. Nối `ivr.script.*` vào `IvrPermissions`/`IvrRoles` theo quyết định ở bước 1.
4. OpenAPI `draft.12 → draft.13`, regenerate DTO, re-pin `contract-manifest.json`, oasdiff.
5. Màn `/config` chuyển từ read-only sang có action bar, giữ nguyên hai chốt cứng của spec:
   **không** thêm biến ngoài whitelist, **không** bật `KEY_9`.

**Tiêu chí nghiệm thu.**

- Creator submit rồi tự approve ⇒ `403`, có test.
- Content approver = Privacy/Legal approver ⇒ bị chặn, có test.
- Approve khi `ProductionTargetV1FieldsApproved=false` ⇒ **không** được coi là production-ready
  (giữ đúng ràng buộc `OD-V1-15`).
- Retired version ⇒ fail-closed ở mọi mode.
- Mọi action có bản ghi trong `ivr_audit_log` với before/after.

**Ước lượng.** 3–4 ngày công + 1 quyết định owner.

---

### A3 · Màn hình feature-flag / runtime gate

> ✅ **ĐÃ TRIỂN KHAI `2026-08-23` — `W-0110`, trạng thái `TESTS_PASS`.**
> Bằng chứng: [`docs/evidence/W-0110/README.md`](../../docs/evidence/W-0110/README.md).
> ⚠️ Đọc §6b của evidence trước khi dùng: `PendingRuntimeGateAuthorization.IsApprovedAsync`
> trả `false` vô điều kiện, nên **mọi** thay đổi cờ hiện trả `409 IVR_OPERATIONAL_BLOCKED`.
> Màn hình đã xong; đường phê duyệt phía sau thì chưa.

> **Ưu tiên: 🟠 CAO.** Quyền vừa được cấp mà không có nút để bấm.

**Bằng chứng.**

- [`specs/ui/08-role-permission-ui.md`](../../specs/ui/08-role-permission-ui.md) §3 ghi
  nguyên văn: *"Đọc feature flag / kill switch (API, **chưa có màn riêng**)"*.
- `grep -rn "featureFlag\|feature-flag" admin-ui/src` ⇒ chỉ có một chip **chỉ đọc**
  trên `/integration` (`integration.killSwitch`).
- Backend đã đủ: 3 endpoint trong `FeatureFlagEndpoint.cs`, quyền `FlagRead` +
  `RuntimeGateAdmin` đã tồn tại trong `IvrPermissions`.

**Vì sao là lỗ hổng.** `OD-V1-20` (luồng L1 đang thi hành) chuyển ràng buộc P0 từ
*"không ai bấm được"* sang *"mỗi lần bấm phải có four-eyes + actor khớp + audit"*. Cách
duy nhất hiện nay để bấm là gọi API tay bằng `curl` — tức là **đúng thứ không có
four-eyes và không có UI nào nhắc người bấm về chiều rủi ro**. Màn hình này không phải
tiện lợi, nó là nơi luật bất đối xứng của `specs/api/03` §"Runtime-gate controls" được
nhìn thấy.

**Việc phải làm.**

1. Route `/flags` (Admin-only, server-side guard như mọi route khác).
2. Panel đọc: snapshot typed theo environment + kết quả `:/kill-switch` (revision +
   trạng thái effective). Không đọc được ⇒ hiển thị **ON** (fail-closed), không hiển thị trống.
3. Form mutation, **phân biệt rõ hai chiều**:
   - *Giảm rủi ro* (bật kill switch, thu hẹp allowlist, đặt `realCustomerCallAllowed=false`):
     chỉ cần `reason`.
   - *Tăng rủi ro* (tắt kill switch, mở rộng allowlist, `realCustomerCallAllowed=true`):
     bắt buộc four-eyes; ở `PRODUCTION_REAL` thì **chặn hẳn ở UI** và chỉ dẫn sang deployment.
4. Hiển thị cảnh báo khi actor định mở allowlist tới đích mà chính họ sắp gọi (luật đã
   có trong spec, chưa có chỗ nào thi hành ở UI).
5. Thêm mục nav — **sau khi L2 commit xong** để tránh xung đột.

**Tiêu chí nghiệm thu.**

- Operator vào `/flags` ⇒ 403, không render dữ liệu (không chỉ ẩn nút).
- Mutation tăng rủi ro thiếu four-eyes ⇒ UI chặn **và** API trả lỗi (chứng minh cả hai lớp).
- `REAL_CUSTOMER_CALL_ALLOWED` vẫn `false` ở cả 4 environment sau mọi test.
- E2E: bật kill switch từ UI ⇒ audit log có bản ghi với actor khớp subject.

**Phụ thuộc.** L1 phải commit trước (không sửa `IvrRoles.cs` từ luồng này). L2 nên commit trước bước 5.

**Ước lượng.** 2–3 ngày công.

---

### A4 · Cắt ngang cuộc gọi đang diễn ra

> ✅ **ĐÃ TRIỂN KHAI `2026-08-23` — `W-0111`, trạng thái `TESTS_PASS`.**
> Bằng chứng: [`docs/evidence/W-0111/README.md`](../../docs/evidence/W-0111/README.md).
> Cắt qua CSDL vì `Ivr.Api` không đăng ký `ISimGateway`; độ trễ = chu kỳ poll (mặc định
> `500 ms`). Cắt hàng loạt tách thành nút riêng, **không** gộp vào kill switch.

> **Ưu tiên: 🟠 CAO — chốt an toàn trước pilot.**

**Bằng chứng.**

- [`docs/release/readiness-board.md`](../../docs/release/readiness-board.md) §6 ghi:
  *"cắt ngang cuộc đang gọi | **không có cơ chế nào**"*.
- `HangupAsync` **có tồn tại** nhưng chỉ ở trong vòng lặp dispatch:
  [`AsteriskSchedulerDispatchGateway.cs:149`](../../src/Ivr.Infrastructure/Telephony/AsteriskSchedulerDispatchGateway.cs)
  và `MockTelephonyDispatchGateway.cs:272`. Không endpoint nào gọi tới.
- [`specs/api/03-admin-api.md`](../../specs/api/03-admin-api.md) §2a xác nhận có chủ ý:
  *"Queue pause … chỉ chặn claim mới; active lease/call không bị cancel"*.

**Vì sao là lỗ hổng.** Kill switch và queue pause đều dừng cuộc **sắp gọi**. Nếu phát
hiện script sai, giọng sai, hay gọi nhầm số **trong lúc đang nói chuyện với khách**,
không có gì dừng được — phải đợi khách cúp máy. Với `REAL_CUSTOMER_CALL_ALLOWED=NO` thì
đây là rủi ro lý thuyết; ngày bật cờ đó lên, nó thành rủi ro thật, và lúc đó mới làm là muộn.

**Việc phải làm.**

1. `POST /call-jobs/{ivrCallJobId}:terminate` — quyền mới (đề xuất `IVR_CALL_TERMINATE`,
   cấp cho cả Admin và Operator vì đây là chiều **giảm** rủi ro), `reason` bắt buộc, audit.
2. Ngữ nghĩa phải chốt và test: cuộc bị cắt ghi **`IVR_TECHNICAL_EXCEPTION`**,
   `customer_attempt_counted=false`. Cắt ngang **không phải** là khách từ chối.
3. Nút trên `/calls/[id]` chỉ hiện khi job đang ở trạng thái có lease active.
4. Nối vào kill switch: bật kill switch ⇒ tùy chọn "cắt mọi cuộc đang chạy" (mặc định
   **không** tự cắt — cắt hàng loạt phải là hành động tường minh có `reason` riêng).

**Tiêu chí nghiệm thu.**

- Cắt cuộc đang chạy ⇒ lease được giải phóng, channel về `IDLE`, không kẹt fencing token.
- Kết quả ghi là technical exception, `customer_attempt_counted=false`, có test.
- Cắt cuộc đã kết thúc ⇒ `409`, không tạo bản ghi rác.
- Chaos test: cắt đúng lúc đang chuyển trạng thái ⇒ không mất/ghi trùng kết quả.

**Ước lượng.** 2–3 ngày công.

---

### A5 · Seed loader / scenario runner / integration-status profile (UI-07)

> ✅ **ĐÃ TRIỂN KHAI `2026-08-23` — `W-0112`, trạng thái `TESTS_PASS`.**
> Bằng chứng: [`docs/evidence/W-0112/README.md`](../../docs/evidence/W-0112/README.md).
> Ba điểm khác bản mô tả, đã ghi ngược vào `specs/ui/07`: quyền riêng `IVR_DEV_TOOLING` (không
> dùng lại quyền SIM); production `404` vì **không đăng ký route**; "áp profile" chỉ thi hành
> được `SIM_GATEWAY`, 4/5 phụ thuộc còn lại là khai báo vì IVR không thăm dò chúng.
> ⚠️ File mẫu `sales-target-v1.sample.json` ghi mốc thời gian tuyệt đối đã hết hạn — loader dời
> cửa sổ từng tác vụ về hiện tại; chạy lại **không** làm mới cửa sổ.

> **Ưu tiên: 🟡 TRUNG BÌNH — nhưng có giá trị ngay cho các buổi nghiệm thu tháng 9.**

**Bằng chứng.** [`admin-ui/src/i18n/vi.json:245`](../../admin-ui/src/i18n/vi.json) ghi
*"Chưa có API nạp seed hay chạy scenario. Việc này hiện thực hiện qua CLI/SQL"*, trong
khi [`specs/ui/07-seed-mock-management.md`](../../specs/ui/07-seed-mock-management.md)
khai 3 action: đổi adapter_mode, chạy scenario dry-run, áp integration-status profile.

**Vì sao đáng làm.** Lịch tháng 9 có 2 đợt nghiệm thu và 1 đợt chạy thử toàn tuyến. Mỗi
lần dựng lại tình huống demo bằng SQL tay là một lần có thể dựng sai, và người nghiệm thu
không tự bấm lại được. Có scenario runner thì buổi nghiệm thu tự chạy được, và bằng chứng
tái lập được.

**Việc phải làm.** 3 endpoint **chỉ non-prod**, chặn cứng bằng environment guard chứ không
chỉ bằng permission: `POST /dev/seed:load`, `POST /dev/scenarios/{id}:dry-run`,
`POST /dev/integration-profiles/{id}:apply`. Ở `PRODUCTION_REAL` phải **404**, không phải 403
(không tiết lộ là có tồn tại). Giữ nguyên khóa `adapter_mode=REAL` cho tới khi `DT-01` + `DF-03` đóng.

**Tiêu chí nghiệm thu.** Gọi ở production ⇒ 404, có test. Dry-run **không** phát cuộc gọi
nào (đếm bằng telemetry). Seed vào prod ⇒ không thể, có test.

**Ước lượng.** 2–3 ngày công.

---

### A6 · Lưu lại giọng **đã thực sự phát**, không suy lại lúc đọc

> ✅ **ĐÃ TRIỂN KHAI `2026-08-23` — `W-0113`, trạng thái `TESTS_PASS`.**
> Bằng chứng: [`docs/evidence/W-0113/README.md`](../../docs/evidence/W-0113/README.md).
> Ba cột `voice_id`/`voice_region`/`voice_region_resolved` ghi tại dispatch **và** vào audit log.
> Contract đọc cột đã lưu, chỉ fallback suy-lại cho bản ghi cũ, kèm `voice_region_source` =
> `RECORDED`/`DERIVED`; màn chi tiết cảnh báo bản suy lại **không dùng để ký nghiệm thu**.
> ⚠️ Không sửa dữ liệu cũ — evidence lab đã sinh trước đây vẫn mang số suy lại.
> `SpeakingRate` cũng suy lại được sai như vậy nhưng nằm ngoài phạm vi A6.

> **Ưu tiên: 🟡 TRUNG BÌNH — phải xong trước khi dùng evidence lab để ký.**

**Bằng chứng.** [`W-0106 plan §5`](W-0106-regional-voice-routing-plan.md) ghi rõ:
*"`voice_region` là hàm của dữ liệu đã lưu, **không phải bản ghi audit của giọng đã phát**
… một lần đổi config giữa lúc gọi và lúc đọc sẽ làm hai thứ lệch nhau."*

**Vì sao là lỗ hổng.** Evidence lab sẽ được dùng để owner ký nhận giọng. Nếu con số trên
màn hình là **suy lại** chứ không phải **ghi lại**, thì một lần đổi config sau đó làm
toàn bộ evidence cũ nói sai. Đây là lỗi âm thầm: không có gì đỏ, chỉ có số sai.

**Việc phải làm.** Migration thêm `voice_id` + `voice_region` vào bảng attempt, ghi tại
thời điểm dispatch; `voice_region` trên contract đọc từ cột đã lưu, chỉ fallback sang
suy-lại cho bản ghi cũ (và **đánh dấu rõ** là suy lại).

**Ước lượng.** 1–2 ngày công. **Phụ thuộc:** chờ L3 commit xong phần W-0106.

---

### A7 · Số thập phân trong số lượng (`2,5 kg`)

> **Ưu tiên: 🟡 TRUNG BÌNH — gộp vào A1.**
>
> ✅ **ĐÃ LÀM cùng A1 (`W-0108`).** `VietnameseNumberSpeller.SpellQuantity` đọc phần thập phân
> **từng chữ số** (`0,25` → "không phẩy hai năm"). `OD-SCRIPT-02` được thi hành theo đúng đề
> xuất "đọc thành chữ"; nếu owner muốn làm tròn thì đó là một thay đổi ngược lại, không phải
> việc còn thiếu.

**Bằng chứng.** [`W-0106 plan §6 GĐ 2`](W-0106-regional-voice-routing-plan.md):
*"số lượng **thập phân** (`2,5 kg`) vẫn giữ dạng chữ số … concatenative không ghép được
số thập phân từ clip thu sẵn. Cần quyết định trước khi buổi thu âm diễn ra."*

**Việc phải làm.** Mở rộng `VietnameseNumberSpeller` đọc phần thập phân ("hai phẩy năm"),
hoặc chốt quy tắc làm tròn ở tầng renderer. Đây là **quyết định nghiệp vụ** (`OD-SCRIPT-02`
ở §8), không phải lựa chọn kỹ thuật — làm tròn số lượng hàng bán là chuyện của business.

**Ước lượng.** 0,5 ngày sau khi có quyết định.

---

### A8 · Cổng "code mới chịu được schema cũ"

> ✅ **ĐÃ TRIỂN KHAI `2026-08-24` — `W-0114`, trạng thái `TESTS_PASS`.**
> Bằng chứng: [`docs/evidence/W-0114/README.md`](../../docs/evidence/W-0114/README.md).
> Job CI riêng `schema_compat_gate`, `allow_failure: false`, có trong `requiredJobs` của
> `ci-config-selftest` nên xoá job là pipeline đỏ.
> ⚠️ **Phạm vi rộng hơn đề bài, có chủ ý.** Chart chạy migration bằng Job `pre-upgrade`, nên
> chiều mà `helm rollback --atomic` thật sự đi qua là **code cũ trên schema mới** — không phải
> chiều A8 nêu. Cổng làm **cả hai**: `IT-SCHEMA-NEWCODE-01/02` (binary bản ship trên schema N-1)
> và `UT-SCHEMA-BACKCOMPAT-01` (7 dạng thao tác migration release trước không sống nổi).
> ℹ️ `rollback.md` §3 tự nhận *"chưa có test nào ép"* — câu đó **hết đúng từ `W-0046`**:
> `IT-MIGRATE-03` đã quét mã nguồn migration từ đó. `W-0114` mở rộng nó sang mô hình thao tác có
> kiểu (thêm 3 dạng, phân biệt nới rộng/thu hẹp `AlterColumn`) và **giữ cả hai** — bản quét văn
> bản chạy trong image node nên đỏ sớm hơn. Câu trong `rollback.md` đã được sửa lại.
> Không sửa một dòng production nào; danh sách miễn trừ rỗng là **kết quả đo**, không phải
> mặc định bỏ qua.
> Cổng đọc *hình dạng* migration, **không** đọc dữ liệu hiện có — phần đó là A9.

> **Ưu tiên: 🟡 TRUNG BÌNH.**

**Bằng chứng.** [`docs/owner-decisions-open.md`](../../docs/owner-decisions-open.md) §"Không
nằm trong danh sách này": *"migration 'code mới chịu được schema cũ' — chưa có cổng; là
**việc kỹ thuật** còn lại, không phải quyết định."*

**Việc phải làm.** Job CI chạy binary mới trên schema của migration **trước đó** và yêu cầu
smoke pass. Đây là điều kiện để rolling deploy không gãy giữa chừng — mà `helm rollback --atomic`
đã được khai trong readiness board là cơ chế rollback chính.

**Ước lượng.** 1–2 ngày công.

---

### A9 · `CHECK` constraint cho 16 bảng còn thiếu

Đã được W-0107 tách ra tường minh (`OD-L10N-04`). Giá trị: mở rộng vùng phủ của guard
`IT-L10N-DBENUM-04` và chặn dữ liệu rác ở tầng DB. **Ưu tiên: 🟢 THẤP**, làm khi rảnh.
Ước lượng 1–2 ngày (chủ yếu là migration + kiểm dữ liệu hiện có có vi phạm không).

---

### A10 · Việt hóa phần văn xuôi backend còn hoãn (`OD-L10N-02b`)

`detail` dạng telemetry (`provider=MOCK; channels 3/4 enabled`) và `event.effect` của
`CAPACITY_INCIDENT`. W-0107 khuyến nghị **giữ nguyên** vì đây là dòng chẩn đoán `key=value`,
dịch thì mất khả năng grep. **Ưu tiên: 🟢 THẤP** — và khuyến nghị là **không làm**, chỉ
thêm cột phụ đã dịch nếu owner muốn. Cần đổi contract (`draft.13`) nếu làm.

---

## 5. Nhóm B — chặn bởi bên ngoài (không tự gỡ được)

Giữ nguyên như tracker và [`readiness-board.md`](../../docs/release/readiness-board.md); liệt kê
ở đây để plan đọc được độc lập. **15 work item** đang `BLOCKED_EXTERNAL`, **21 quyết định
`OD-V1-*`** còn mở.

| Cổng | Chặn cái gì | Ai đóng | Việc IVR chuẩn bị sẵn |
| --- | --- | --- | --- |
| `G-LAB-SIM` | W-0048, W-0049 | Infra + vendor | Đường ống lab đã xong, chờ SIM + GSM gateway |
| `G-CONTRACT` | Sales task/callback Target V1 | Sales API/Core | fake provider + WireMock + CDC sẵn sàng |
| `G-SPEECH` | `privacy_safe_order_summary` | Sales/Product/Privacy | DTO + validator + renderer đã có |
| `G-DIAL` | dial-token issue/resolve | Sales/Security/Telephony | resolver port + mock vault đã có |
| `G-AUTH` | JWT/mTLS production | Security/Platform | mock JWT + negative test đã có |
| `G-POLICY` | attempt policy D-10 | Product/Core | policy registry versioned đã có |
| `G-ESIM32` | 32 eSIM capacity | Infra/procurement | capacity simulator đã có |
| `G-LEGAL` | script/retention/do-not-call | Legal/Privacy | **cần A2 mới có đường thi hành** |
| `G-RELEASE` | DF-03 go/no-go | Release owner | evidence pack đã nộp |
| `G-GITLAB` | W-0061 | Platform | cần Premium/Ultimate + reviewer thứ hai |
| `G-PLATFORM` | endpoint + credential thật | Platform/Infra | — |

**Riêng W-0106 còn 2 việc chỉ owner làm được:**

- `4.1` — render 3 file MP3 từ ElevenLabs (cần phiên đăng nhập của owner).
- `1.7` — mua gói Starter `$6` để có commercial license ⇒ đóng `OD-VOICE-01`.
- `1.5`/`OD-VOICE-05` — owner nghe và ký; **chưa ký thì trần trạng thái W-0106 là `TESTS_PASS`**.

---

## 6. Nhóm C — cố ý hoãn, **không phải lỗ hổng**

| Mục | Trạng thái | Căn cứ |
| --- | --- | --- |
| Gửi SMS/notification | `DEFERRED_TARGET` (W-0033) | V1 không gửi; `v1NotificationEnabled=false` immutable |
| Vòng phản hồi opt-out | `DEFERRED_TARGET` (W-0034) | `P4-6`, ngoài phạm vi V1 |
| Phím `9` | `NOT_ENABLED` | AS-07, UI không cho bật |
| Ghi âm cuộc gọi | OFF mặc định | DT-05 |
| Ngôn ngữ thứ hai trong console | không làm | `DTS-03`: console **chỉ tiếng Việt** |
| Dịch `order_state` | không dịch | `NT-3` — trạng thái đơn thuộc Order Core |
| Dịch CSV/audit/evidence | không dịch | `NT-5` — giữ mã gốc để đối soát |

---

## 7. Nhóm D — trôi tài liệu, sửa nhanh

| # | Vấn đề | Bằng chứng | Sửa gì |
| --- | --- | --- | --- |
| D1 | README nói `/health/ready` **luôn trả 200** và "không phải tín hiệu fail-closed cho tới W-0040" | [`README.md:69`](../../README.md) vs [`IvrReadinessProbe.cs:16`](../../src/Ivr.Api/Health/IvrReadinessProbe.cs) trả `503` thật | Cập nhật README — W-0040 đã xong |
| D2 | README mô tả `admin-ui` chỉ có dashboard/call log/detail (P3-2) | README §Components vs 14 route thật | Cập nhật danh sách màn |
| D3 | `specs/api/03` §0 khai `/accounts/{accountId}:delete` | OpenAPI và code đều dùng `DELETE /accounts/{accountId}` | Sửa spec cho khớp |
| D4 | `specs/api/03` §1 ghi `IVR_RUNTIME_GATE_ADMIN` *(OD-V1-20 pending)* và §"Báo cáo" ghi "Admin có 11 quyền" | `OD-V1-20` đã chốt `2026-08-22`, Admin nay có **13** quyền | ⚠️ **Luồng L1 đang làm** — kiểm lại sau khi L1 commit, chỉ sửa phần còn sót |

---

## 8. Quyết định cần owner chốt trước khi code

| ID | Câu hỏi | Chặn hạng mục | Đề xuất |
| --- | --- | --- | --- |
| `OD-SCRIPT-01` | Content approver và Privacy/Legal approver phải là **hai người khác nhau**, nhưng hệ thống chỉ có 2 role. Thêm role thứ ba, hay ràng buộc theo `accountId`? | **A2** | **Ràng buộc theo `accountId`.** Thêm role thứ ba làm phình ma trận RBAC vừa mới khóa ở W-0105; ràng buộc "approver ≠ approver trước đó" thi hành được ngay trong `EnsureApprovalAllowed` |
| `OD-SCRIPT-02` | Số lượng thập phân: đọc "hai phẩy năm" hay làm tròn? | **A7**, và **A1** nếu chọn thu âm người thật | **Đọc thành chữ.** Làm tròn số lượng hàng bán là đổi thông tin đơn — không phải việc của tầng đọc |
| `OD-CALL-01` | Cắt ngang cuộc gọi: cấp cho cả Operator hay chỉ Admin? | **A4** | **Cả hai.** Đây là chiều giảm rủi ro; bắt Operator đi tìm Admin trong lúc cuộc gọi đang chạy là thiết kế sai |
| `OD-VOICE-01` | Nguồn giọng production (đang mở từ W-0106) | **A1** phần biến thiên | ElevenLabs Starter `$6` — đã phân tích ở W-0106 §7.1 |

---

## 9. Thứ tự triển khai đề xuất

Xếp theo **đường găng của pilot**, không theo độ dễ.

### Đợt 1 — Chốt an toàn (tuần 24–30/08)

Làm được ngay, không chờ quyết định nào, và là điều kiện để dám bật cờ gọi thật.

| # | Việc | Ngày công | Chờ ai |
| --- | --- | --- | --- |
| A4 | Cắt ngang cuộc đang gọi | 2–3 | `OD-CALL-01` (nhỏ) |
| A3 | Màn feature-flag / runtime gate | 2–3 | **L1 commit xong** |
| D1–D3 | Sửa trôi tài liệu | 0,5 | — |

> Đợt này đóng đúng cái mà `readiness-board` §6 đang bỏ trống: kill switch có UI, và
> cuộc gọi đang chạy có phanh.

### Đợt 2 — Khách nghe được đơn của chính mình (tuần 01–13/09)

| # | Việc | Ngày công | Chờ ai |
| --- | --- | --- | --- |
| A1 | Đường ống ghép audio (segment + playlist + cache) | 4–6 | phần biến thiên chờ `OD-VOICE-01` |
| A7 | Số thập phân | 0,5 | `OD-SCRIPT-02` |
| A6 | Lưu giọng đã phát | 1–2 | L3 commit xong |

> Đây là đường găng. Không có A1 thì buổi "chạy thử toàn tuyến" tuần 14–20/09 chỉ chứng
> minh được đường truyền, không chứng minh được nội dung.

### Đợt 3 — Đường thi hành cho cổng Legal (tuần 07–20/09)

| # | Việc | Ngày công | Chờ ai |
| --- | --- | --- | --- |
| A2 | Script lifecycle API + quyền + màn hình | 3–4 | `OD-SCRIPT-01` |
| A5 | Seed loader / scenario runner | 2–3 | — |

> A2 nên xong **trước** khi Legal ngồi vào bàn duyệt kịch bản (mốc 21/09 trong báo cáo
> tiến độ), nếu không chữ ký của họ không có chỗ nào để đi vào.

### Đợt 4 — Vệ sinh kỹ thuật (khi có khoảng trống)

| # | Việc | Ngày công |
| --- | --- | --- |
| A8 | Cổng schema forward-compat | 1–2 |
| A9 | `CHECK` constraint 16 bảng | 1–2 |
| A10 | (khuyến nghị **không làm**) văn xuôi telemetry | — |

**Tổng nhóm A: khoảng 17–24 ngày công**, chưa tính thời gian owner quyết định và chưa
tính bất kỳ cổng ngoài nào.

---

## 10. Rủi ro của chính kế hoạch này

| # | Rủi ro | Mức | Giảm thiểu |
| --- | --- | --- | --- |
| R1 | **Đụng file với 3 luồng đang chạy** ⇒ mất việc hoặc merge hỏng | **CAO** | §2 liệt kê chính xác file đang giữ. A3 chờ L1; A6 chờ L3; nav chờ L2 |
| R2 | A1 bị hiểu là "đã xong vì đã gọi được trong lab" | **CAO** | Lab hiện phát **một file cố định**. Tiêu chí nghiệm thu A1 yêu cầu **hai đơn khác nhau ⇒ hai chuỗi audio khác nhau** — đó là phép thử phân biệt được hai thứ |
| R3 | A2 làm xong nhưng không ai duyệt vì chưa chốt `OD-SCRIPT-01` | TRUNG BÌNH | Hỏi quyết định **trước** đợt 3, không hỏi lúc đang code |
| R4 | Làm A3 rồi mới phát hiện L1 đổi tên permission | TRUNG BÌNH | Đọc lại `IvrPermissions.cs` ngay trước khi bắt đầu A3 |
| R5 | Ước lượng ngày công lệch vì mỗi hạng mục kéo theo OpenAPI → DTO → drift baseline | TRUNG BÌNH | A2 và A10 đã tính chuỗi `draft.13`; A1/A3/A4 **không** đổi contract trừ A4 thêm 1 operation |
| R6 | Plan này bị dùng như tracker thứ hai | TRUNG BÌNH | Ghi ở đầu file. Khi bắt đầu một hạng mục, cấp Work ID thật và chuyển trạng thái **ở tracker**, không ở đây |

---

## 11. Phụ lục — lệnh kiểm chứng

Toàn bộ khẳng định trong file này tái lập được bằng các lệnh sau.

Đối chiếu path OpenAPI với handler thật:

```bash
grep -oE "^  /[^:]*" specs/api/openapi/ivr-order-confirmation.v1.yaml | sed 's/^  //' | sort -u
```

Liệt kê mọi route đã đăng ký trong API:

```bash
grep -rn "MapGet\|MapPost\|MapPut\|MapPatch\|MapDelete" src/Ivr.Api --include=*.cs
```

Chứng minh quyền script chưa được nối vào bất kỳ đâu:

```bash
grep -rn "ivr\.script\." src/Ivr.Api admin-ui/src specs/api/openapi
```

Chứng minh không có màn feature-flag:

```bash
grep -rn "feature-flag\|featureFlag" admin-ui/src
```

Chứng minh không có `TODO`/`NotImplementedException` trong mã sản phẩm:

```bash
grep -rn "TODO\|FIXME\|NotImplementedException" src/ admin-ui/src/ --include=*.cs --include=*.ts --include=*.tsx
```

Xem các luồng đang giữ file (chạy trước khi bắt đầu bất kỳ hạng mục nào):

```bash
git status --porcelain
```

---

## 12. Nguồn đã đọc

`prompt/_execution/prompt-execution-tracker.md` (§2, §3, §5, `A-0321`, `A-0322`) ·
`plan/ivr-orther/00-index.md` · `production-blockers-plan.md` ·
`W-0105`/`W-0106`/`W-0107` plan · `specs/_review/open-decisions-register.md` ·
`specs/api/03-admin-api.md` · `specs/api/openapi/ivr-order-confirmation.v1.yaml` ·
`specs/functional/01..08` · `specs/ui/00..08` · `docs/release/readiness-board.md` ·
`docs/owner-decisions-open.md` · `docs/reports/2026-08-22-bao-cao-tien-do-ivr.md` ·
`src/**` (224 file `.cs`) · `admin-ui/src/**` · `git log` · `git status` ·
transcript ba phiên đang chạy.
