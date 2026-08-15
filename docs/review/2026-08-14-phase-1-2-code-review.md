# Rà soát Phase 1 + Phase 2 — IVR Order Confirmation

**Ngày:** 2026-08-14 · **Phạm vi:** `P1-1`…`P1-5`, `P2-1`…`P2-9` (W-0014…W-0024, W-0064, W-0065, W-0066)
**Loại:** rà soát độc lập, chỉ đọc — không sửa file nào
**Baseline:** `main@76792e1` · `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO`
**Sales Platform:** `NOT_VERIFIED_FROM_CURRENT_SALES_SOURCE` — source không có trong workspace

---

## 1. Tóm tắt điều hành

Chất lượng từng lát cắt cao: 264 test xanh, build 0 warning/0 error, D-02/D-05 có phòng thủ nhiều lớp, ma trận DTMF đúng, attempt policy không hard-code. Từng prompt được thực hiện nghiêm túc.

**Nhưng hệ thống chưa chạy được end-to-end trong bất kỳ cấu hình nào**, và có bốn chỗ khoá chết vĩnh viễn không có đường thoát. Đây là loại lỗi mà test-theo-lát-cắt không thể bắt được — mỗi slice tự dựng fixture riêng nên đều xanh, còn chỗ nối giữa chúng thì đứt.

| Mức | Số nhóm | Nội dung |
|---|---|---|
| **P0** | 3 | Luồng nghiệp vụ không chạy được; cổng D-05 bị vượt qua |
| **P1** | 9 | Khoá chết vĩnh viễn; lỗi nghiệp vụ chặn khách thật; cổng contract đỏ |
| **P2** | 8 | Mất dữ liệu cục bộ, test không chứng minh điều nó tuyên bố |
| **P3** | ~40 | Nợ kỹ thuật, trôi lệch tài liệu |

Rà soát bởi 37 tác nhân độc lập (18 lens + 18 phản biện đối kháng + 1 critic). 138 phát hiện thô → 9 bị bác bỏ → gộp trùng còn khoảng 30 nhóm. **Các mục P0 và P1 dưới đây đều do tôi tự kiểm chứng lại bằng lệnh chạy thật**, không chỉ dựa vào báo cáo của tác nhân.

---

## 2. Mốc đo thực nghiệm

| Hạng mục | Kết quả |
|---|---|
| `dotnet restore Ivr.sln --locked-mode` | exit 0 |
| `dotnet build Ivr.sln -c Release` | **0 warning, 0 error** |
| `dotnet test Ivr.sln -c Release` | **264 pass / 0 fail / 0 skip** |
| — Contract | 21 |
| — Unit | 164 |
| — Integration | 79 (Testcontainers chạy thật trên PostgreSQL, 33s) |
| NU1903 (SSH.NET) từ lượt rà soát Phase 0 | **đã xử lý đúng** — direct `PackageReference SSH.NET 2026.0.0`, không suppress audit |
| Working tree | sạch, trừ 2 file metadata `AGENTS.md`/`CLAUDE.md` |

---

## 3. P0 — Chặn

### E-01 · Luồng nghiệp vụ không chạy được ở chế độ MOCK: intake ghi vào RAM, scheduler đọc PostgreSQL

Trong `MOCK` — chế độ bắt buộc của V1 — intake và scheduler dùng hai kho lưu trữ **khác nhau và không kết nối với nhau**.

`src/Ivr.Infrastructure/Configuration/ServiceCollectionExtensions.cs:114-118` (nhánh MOCK):

```csharp
services.TryAddSingleton<InMemoryTaskIntakeStore>();
services.TryAddSingleton<ITaskIntakeStore>(provider =>
    provider.GetRequiredService<InMemoryTaskIntakeStore>());
services.TryAddSingleton<IEligibilityRepository>(provider =>
    provider.GetRequiredService<InMemoryTaskIntakeStore>());
```

`src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs:504` (đăng ký **vô điều kiện**, ngoài mọi nhánh mode):

```csharp
services.TryAddSingleton<IPostgresSchedulerStore, PostgresSchedulerStore>();
```

Kiểm chứng: `git grep -l 'InMemorySchedulerStore' -- src/` → **không có kết quả**. Toàn repo chỉ tồn tại `PostgresSchedulerStore.cs`.

**Hệ quả.** Ở MOCK, task và call-job do intake tạo nằm trong `Dictionary` của một process. Scheduler poll bảng `ivr_call_jobs` trong PostgreSQL và không bao giờ thấy chúng. Không cuộc gọi nào được lên lịch. Ngoài ra dữ liệu mất sạch khi restart.

### E-02 · Không có đường nào tạo kênh SIM — scheduler không bao giờ lease được channel

```
$ git grep -nE 'SimChannels\.Add|new SimChannelEntity|INSERT INTO ivr_sim_channels' -- src/ tests/
tests/Ivr.IntegrationTests/InternalAdminApiTests.cs:632
tests/Ivr.IntegrationTests/MockTelephonyPersistenceTests.cs:327
tests/Ivr.IntegrationTests/PostgresPersistenceTests.cs:416
tests/Ivr.IntegrationTests/SchedulerPersistenceTests.cs:451
```

**Bốn kết quả, cả bốn đều là file test.** Migration tạo bảng `ivr_sim_channels` nhưng không seed hàng nào. `SimChannelLeaseRepository` chỉ có `SELECT` và `UPDATE`. API admin của P2-8 có `disableSim`/`enableSim` — sửa hàng đã có, không tạo mới. Không có seed, không có endpoint provisioning, không có lệnh CLI.

**Hệ quả.** Trên database mới, bảng kênh SIM rỗng vĩnh viễn. Kể cả khi sửa xong E-01 thì scheduler vẫn không lease được channel nào để gọi.

> **E-01 + E-02 gộp lại:** không tồn tại cấu hình nào mà một task đi trọn vẹn từ intake → eligibility → schedule → dial → result → callback. 264 test xanh vì mỗi lát cắt tự seed fixture của riêng nó rồi kiểm lát cắt đó.

### E-03 · `PiiGuard` bỏ lọt mọi số điện thoại có dấu phân tách — cổng D-05 trên đường chạy thật

Đã báo ở lượt rà soát Phase 0 (F-06) khi rủi ro còn thấp. Sau P1-3/P2-4/P2-7/P2-9, guard này đã trở thành chốt chặn D-05 **trên đường dữ liệu sống**, nên mức độ nâng lên P0.

```
'0912341234'        MATCH
'0912 341 234'      miss
'091-234-1234'      miss
'+84 912 341 234'   miss
'0912.341.234'      miss
```

`src/Ivr.Domain/Privacy/PiiGuard.cs:21` — `(?<![0-9])(?:0|84|\+84)[0-9]{9}(?![0-9])` đòi 9 chữ số liền nhau.

**Hệ quả.** Sales gửi số ở dạng `0901 234 567` trong bất kỳ trường tự do nào (tên người nhận, ghi chú đơn, `delivery_area_short`) thì giá trị đó đi thẳng qua guard, vào DB, vào lời thoại TTS, và vào evidence. Đây chính xác là điều D-05 tồn tại để ngăn.

**Sửa:** cho phép `[\s.\-]?` giữa các nhóm chữ số, và thêm 5 case trên vào `UT-FND-PII-07`.

---

## 4. P1 — Nghiêm trọng

### E-04 · Một job trễ deadline làm ngừng dispatch của **toàn hệ thống**, vĩnh viễn

`src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs:363-378` — mỗi job miss deadline tạo một incident:

```csharp
context.CapacityIncidents.Add(new CapacityIncidentEntity
{
    Scope = "SCHEDULER_DEADLINE",
    Status = "OPEN",
    HoldNewCalls = true,
```

`PostgresSchedulerStore.cs:100-104` — truy vấn claim chặn **toàn cục**, không lọc theo program hay job:

```sql
AND NOT EXISTS (
    SELECT 1 FROM ivr_capacity_incidents incident
    WHERE incident.status = 'OPEN' AND incident.hold_new_calls IS TRUE
)
```

`src/Ivr.Api/Application/InternalAdminApiService.cs:323-333` — đường resolve **duy nhất** lọc theo `Scope == "ADMIN_QUEUE_PAUSE"`, nên không bao giờ khớp `SCHEDULER_DEADLINE`. Tệ hơn, `:312-321` còn chủ động ném lỗi nếu tồn tại incident hold scope khác:

```csharp
if (blocked) { throw IvrErrors.OperationalBlocked(
    "Queue resume is blocked by a non-admin capacity incident."); }
```

`EligibilityRepository.cs:173-189` tạo incident scope `ELIGIBILITY_DEADLINE` với cùng vấn đề.

**Hệ quả.** Một đơn trễ hạn → toàn bộ hàng đợi dừng → `resume-queue` bị chặn → `technical-retry` bị chặn (`:699-706`). Chỉ khôi phục được bằng `UPDATE` tay trong database. Đây là outage toàn hệ thống kích hoạt bởi một sự kiện hoàn toàn bình thường.

### E-05 · Kênh SIM bị quarantine vĩnh viễn — công suất giảm dần về 0

`PostgresSchedulerStore.cs:272-275` đặt `Status = "QUARANTINED"` và `QuarantineUntil = detectedAt + quarantineDuration`.

Nhưng mọi truy vấn claim lại lọc `quarantine_until IS NULL`, **không** lọc `<= now`:

- `PostgresSchedulerStore.cs:131-132`
- `SimChannelLeaseRepository.cs:66-67`

Chỗ duy nhất gán lại `QuarantineUntil = null` là `PostgresTelephonyDispatchStore.cs:251-253`, chỉ chạy khi một cuộc gọi **hoàn tất trên chính channel đó** — mà channel đã quarantine thì không lease được nữa, nên không bao giờ tới được nhánh này. Admin cũng bị chặn: `InternalAdminApiService.cs:580-589` từ chối enable khi `Status == "QUARANTINED"` hoặc `FailCount > 0`.

**Hệ quả.** Mỗi lease hết hạn (worker crash, GC pause, deploy) đốt vĩnh viễn một kênh SIM. Công suất chỉ giảm, không bao giờ hồi. `quarantineDuration` được truyền vào nhưng không có tác dụng gì.

### E-06 · Job trong `HELD_ADMIN_REVIEW` chết im lặng — Sales không bao giờ nhận kết quả

`PostgresSchedulerStore.cs:251-256` đưa job vào `HELD_ADMIN_REVIEW` sau lease recovery. `PostgresSchedulerStore.cs:319` — sweeper deadline chỉ quét 3 trạng thái:

```sql
AND job.status IN ('READY_FOR_SCHEDULER', 'DISPATCH_LEASED', 'DRY_RUN')
```

`ResultRepository.cs:338-346` đưa job hết technical retry vào cùng trạng thái đó, cũng bị loại y hệt.

**Hệ quả.** Worker crash giữa cuộc gọi → job treo → hết confirmation window → không final result, không incident, không bản ghi trong `ivr_result_callbacks`. Đơn kẹt vô hạn ở phía Sales và không ai nhìn thấy. Vi phạm trực tiếp `P2-3` §2.6: *"Capacity miss creates incident/result; never silently expires"*.

### E-07 · `TASK_HELD_ADMIN_REVIEW` không tạo ReviewItem — admin không có việc để làm

`EligibilityRepository.cs:275-278` chỉ đổi status, không tạo `ReviewItemEntity`. So sánh: `ResultRepository.cs:188` **có** tạo. `InternalAdminApiService.ReviewAsync` (`:496-500`) tra theo `ReviewItemId` và ném `NotFound` nếu không có.

`TASK_HELD_ADMIN_REVIEW` là nhánh mặc định của **9 tình huống fail-closed** trong `EligibilityRules.cs` (`:154`, `:165`, `:181`, `:199`, `:246`, `:308`, `:325`, `:335`, `:369`). Tất cả rơi vào một trạng thái hold không có cơ chế xử lý và không có transition ra.

### E-08 · Cổng `call_restriction` ở intake đã bị gỡ — regression từ P2-2

Baseline P2-1 (`git show 85c2b63:src/Ivr.Infrastructure/Intake/TaskIntakeService.cs`) **có** gate:

```csharp
if (source.Call_restriction)
{
    return Rejected(source, TaskIntakeDecisions.BlockedOperational, "CALL_RESTRICTION_ACTIVE");
}
```

Hiện tại `TaskIntakeService.cs:448-467` không còn nhắc `Call_restriction`; nó chỉ còn được ghi vào entity ở `:587`.

Seed `seed/sales-target-v1.sample.json:542-547` khai `NEG-DOMAIN-RESTRICTION-01` phải cho `TASK_BLOCKED_OPERATIONAL`. Thực tế trả `TASK_ACCEPTED_DRY_RUN_ONLY`.

**Test đang khoá hành vi sai:** `TaskIntakeServiceTests.cs:104-118` và `TaskIntakePersistenceTests.cs:139` cùng assert `AcceptedDryRunOnly`. Cả hai đều PASS.

**Hệ quả.** Khách đã đăng ký không muốn nhận cuộc gọi vẫn được nhận task. Rủi ro tuân thủ, không chỉ rủi ro kỹ thuật.

### E-09 · `technicalRetry` bị từ chối 100% — so sánh sai hằng số

`src/Ivr.Domain/Policies/EligibilityRules.cs:6`:

```csharp
public const string Eligible = "ELIGIBLE_FOR_IVR";
```

`src/Ivr.Api/Application/InternalAdminApiService.cs:690`:

```csharp
|| !string.Equals(task.EligibilityDecision, "ELIGIBLE", StringComparison.Ordinal))
```

`EligibilityRepository.cs:201` ghi `task.EligibilityDecision = persisted.Decision`, tức luôn là `"ELIGIBLE_FOR_IVR"`. So sánh `Ordinal` với `"ELIGIBLE"` **không bao giờ khớp** → điều kiện luôn đúng → luôn ném `PolicyMismatch`.

**Hệ quả.** Một trong 13 operation bắt buộc của P2-8 không dùng được dưới bất kỳ cấu hình nào. Test happy-path của nó không tồn tại nên không ai phát hiện.

### E-10 · `PiiGuard` chặn nhầm tên sản phẩm của chính công ty

```
'Sâm cao cấp Hàn Quốc'          CHẶN
'Nấm linh chi cao cấp 500g'     CHẶN
'Hồng sâm cao cấp'              cho qua
```

`PiiGuard.cs:23` — `(?:duong|so nha|ngo|hem|ngach|thon|ap)\s+[A-Za-z0-9]` thiếu word boundary, nên `ấp` trong **"cao cấp"** + khoảng trắng + chữ cái khớp luật địa chỉ.

**Hệ quả.** Đây là công ty thực phẩm sâm. "cao cấp" là một trong những từ phổ biến nhất trong tên sản phẩm. Mọi đơn chứa `cao cấp <chữ>` sẽ bị guard chặn không đọc được thành lời thoại. Chung gốc với E-03 nhưng hướng ngược lại — cùng một dòng regex vừa quá lỏng vừa quá chặt.

**Sửa:** thêm `\b` vào nhánh không dấu.

### E-11 · `ShortDeliveryArea` từ chối định dạng khu vực giao hàng phổ biến nhất Việt Nam

`src/Ivr.Domain/Confirmation/PrivacySafeSpeech.cs:105-118` — chuỗi `"thanh pho "` chỉ được cắt khi nó là **tiền tố**:

```csharp
string markerScanText = normalized.StartsWith("thanh pho ", StringComparison.Ordinal)
    ? normalized["thanh pho ".Length..]
    : normalized;
```

| Giá trị | Kết quả |
|---|---|
| `Thành phố Thủ Đức` | ✅ qua (tiền tố được cắt) |
| `Quận 1, Thành phố Hồ Chí Minh` | ❌ **bị từ chối** (`"pho "` nằm giữa chuỗi) |
| `Hà Nội, Thành phố Hà Nội` | ❌ **bị từ chối** |

**Hệ quả.** `Quận X, Thành phố Hồ Chí Minh` là cách viết chuẩn của khu vực giao hàng ở thị trường lớn nhất. Task chứa nó sẽ bị từ chối ngay từ intake.

### E-12 · Circuit breaker kẹt half-open vĩnh viễn, readiness vẫn báo READY

`src/Ivr.Infrastructure/Callbacks/CallbackDeliveryModels.cs:69-87` — `TryEnter()` đặt `_halfOpenProbeInProgress = true`. Cờ này chỉ được xoá trong `RecordSuccess()` hoặc `RecordTransientFailure()`.

`CallbackDispatcher.cs:68-102` — khối `try` chỉ bắt `InvalidOperationException` (`:83`); `RecordSuccess`/`RecordTransientFailure` nằm **sau** khối try.

Đường thoát cụ thể: `TargetV1CallbackTransport.cs:122` gọi `ReadFromJsonAsync<CallbackAck200>` — ném `NotSupportedException` khi Sales/LB trả HTTP 200 với `Content-Type: text/html`. `SendAsync` chỉ bắt `OperationCanceledException`, `HttpRequestException`, `JsonException`.

`CallbackDeliveryJobHost.cs:39-42` bắt `Exception` rồi tiếp tục vòng lặp → process không chết, breaker latch vĩnh viễn.

`CallbackDeliveryModels.cs:117-122` — `Snapshot()` tính `open` từ `_openUntil` mà **không** xét `_halfOpenProbeInProgress`, nên readiness trả `READY` trong khi `TryEnter()` chặn 100%.

**Hệ quả.** Một response HTML từ ingress làm ngừng vĩnh viễn toàn bộ callback về Sales, và health check nói mọi thứ bình thường.

---

## 5. P1 — Cổng contract và quản trị

### E-13 · Cổng changelog/breaking-change đang đỏ trên `main`; changelog công bố sai sự thật

Chạy đúng image mà CI pin (`tufin/oasdiff:v1.26.1`):

```
oasdiff breaking specs/api/openapi/baselines/ivr-order-confirmation.v1.0.0.yaml \
                 specs/api/openapi/ivr-order-confirmation.v1.yaml --fail-on WARN
→ 143 changes: 63 error, 80 warning
```

Nhưng `docs/api/changelog/ivr-order-confirmation.md:1-3` đã commit:

```
# API Changelog 1.0.0 vs. 1.0.0

No changes detected
```

Nguyên nhân: commit `251d276` (P2-8) sửa `ivr-order-confirmation.v1.yaml` (+342 dòng) nhưng không tái sinh changelog và không xoay baseline. `git log -- docs/api/changelog/ivr-order-confirmation.md` chỉ có duy nhất commit P1-4.

Job `deploy/ci/docs.gitlab-ci.yml:15-24` có `allow_failure: false` và so byte-for-byte ở dòng `:19`, rồi `oasdiff breaking --fail-on WARN` ở `:22` — cả hai đứng **trước** `selftest-oasdiff.sh` (`:24` = `CT-DOC-02`), nên test bắt buộc đó không bao giờ chạy tới.

Thêm nữa: 63 breaking change được ship dưới nguyên `info.version: 1.0.0`.

**Hệ quả.** Pipeline đỏ ở stage `validate`. `docs/api-changelog.md:18` đang nói với Order Core/Ops/CRM rằng contract không đổi, trong khi có 63 breaking change. Đội tích hợp không biết phải sửa client.

### E-14 · Evidence W-0061 khẳng định `main` được bảo vệ, thực tế có 19 commit push thẳng

`docs/evidence/W-0061/README.md:72` ghi `| Allowed to push and merge | No one |`, `:179` ghi `| Protected default branch | PASS |`, `:181` `| Pipelines must succeed | PASS |`.

Đo hôm nay: `refs/remotes/origin/main` = `refs/heads/main` = `76792e1`. `git log 5544395..main` → **19 commit, 0 merge commit**, toàn bộ push thẳng.

Mâu thuẫn trong chính activity log cùng ngày: `A-0135` ghi *"GitLab từ chối vì protected main"* → `A-0140` ghi *"push và xác minh exact ref ở cả GitHub lẫn GitLab"*. Giữa hai dòng không có entry nào ghi ai đã đổi protected-branch setting, khi nào, vì sao.

**Hệ quả.** Toàn bộ P1-5 và Phase 2 (9 Work ID) không có Merge Request nào, nên checklist traceability bắt buộc của `README-governance` §5 chưa từng được thực hiện lần nào. Và một mục evidence đã ký đang mô tả sai trạng thái hệ thống.

---

## 6. P2 — Đáng kể

| ID | Vấn đề | Vị trí |
|---|---|---|
| E-15 | Idempotency và mutation nghiệp vụ commit ở **hai transaction khác nhau** — crash giữa hai lần commit làm mất chốt chống trùng | `InternalAdminApiService` (P2-8) |
| E-16 | `CompleteDeliveryAsync` kiểm `lease_token` ở `SELECT` nhưng `UPDATE` không mang điều kiện lease — TOCTOU, worker cũ ghi đè kết quả worker mới | `CallbackOutboxRepository` (P2-6) |
| E-17 | Retention: catalog `task_metadata` không khai phụ thuộc `ivr_task_intake_outbox`, `DELETE` cha có thể vi phạm FK hoặc bỏ lại orphan | P1-5 |
| E-18 | `UT-RET-CONFIG-01` không assert `NOT_CONFIGURED` — nhánh fail-closed quan trọng nhất của job xoá dữ liệu chưa từng được kiểm chứng | P1-5 |
| E-19 | `domain_negative` trong seed chưa bao giờ được chạy qua implementation; contract test tuyên bố "covers all" nhưng chỉ đọc JSON | P2-1 |
| E-20 | Idempotency scope theo `task_id` khiến mọi từ chối **tạm thời** (`HELD_POLICY_MISSING`, `BLOCKED_OPERATIONAL`) bị đóng băng — Sales gửi lại vẫn nhận quyết định cũ | P2-1 |
| E-21 | `IT-API-PII-05` tuyên bố quét PII trên response + log của 13 endpoint, thực tế chỉ assert response của **1** endpoint (`GET /queue`, DTO thuần bộ đếm) và không quét log dòng nào | P2-8 |
| E-22 | Thiếu `Idempotency-Key` trả `IVR_MALFORMED_REQUEST` (400) thay vì `IVR_MISSING_TRACE` (422); `IVR_MALFORMED_REQUEST` được trả với **hai** HTTP status khác nhau tuỳ endpoint | P2-1 / P2-8 |
| E-23 | `IT-API-SIM-09`, `IT-API-QUEUE-08`, `IT-TTS-MODE-09`, `UT-TEL-SAFETY-06`, `UT-TTS-PII-04` — mỗi test không kiểm được ít nhất một vế mà §8 yêu cầu; `UT-TEL-SAFETY-06` chứng minh "MOCK không egress" bằng reflection lên tên field | P2-4 / P2-8 / P2-9 |

---

## 7. P3 — Nợ kỹ thuật và trôi lệch

**Test không chứng minh điều nó tuyên bố.** 8 assert negative của P2-7 dùng `Assert.ThrowsAny<Exception>` (pass với bất kỳ exception nào, kể cả `NullReferenceException`). `EveryProviderPortHasDeterministicFake` không hề kiểm tính tất định. `TargetTaskMapperUsesRegistryAndDoesNotMapPhoneFields` chứng minh bằng assertion rỗng. `IsCountedCustomerAttempt` là hằng số `false` ở mọi nhánh nên assertion về nó là tautology. "Concurrent one-channel safety" được chứng minh bằng test **tuần tự**. `CT-DOC-02` báo PASS sai vì `grep` bắt trúng chữ "breaking" trong **tên file** fixture.

**Coverage bị thổi phồng.** 57% dòng đo được là file EF migration sinh tự động. Con số 94–95% trong mọi evidence P1/P2 không phản ánh độ phủ code nghiệp vụ. `W-0065`/`W-0066` còn mâu thuẫn số học: valid lines tăng 2.943 trong khi chỉ thêm 1.125 dòng.

**Trôi lệch contract.** `specs/api/06-error-codes.md` §3 thiếu `IVR_POLICY_BLOCKED` (6 nguồn khác đều có). `IVR_CONTACT_INVALID` là mã chết — công bố ở spec, OpenAPI và code nhưng không điểm nào phát ra. `intakeTask` khai 200/403/409/422 nhưng handler trả cả 400 và 401. Response serialize `null` cho field OpenAPI khai `type: string`. Bề mặt callback Sales bị nhân bản trong OpenAPI internal với enum ACK 6 giá trị mâu thuẫn spec. `P2-8` khai "14 operation" trong khi file có 17 `operationId`.

**Cấu hình chưa khai báo.** `IVR_INTERNAL_SERVICE_TOKEN` — secret bắt buộc cho 6 endpoint internal — không có trong `README`, `.gitlab-ci.yml`, `docker-compose.dev.yml` hay `appsettings*`. `TokenAudience` không nằm trong validator; giá trị rỗng làm mỗi lượt gửi ném `ArgumentException`.

**Khoá cấu hình cũ.** `P1-4`, `P2-2`, `P2-5` vẫn khai governance bằng `IVR_ADAPTER_MODE` (đã thay bằng `IVR_EXECUTION_MODE`). Cùng lỗi đã báo ở lượt Phase 0 (F-15) cho 4 prompt P0.

**Vòng đời/tài nguyên.** `CallbackDispatcher` và `CurrentGoldenHourCallbackTransport` đăng ký Singleton nhưng giữ typed `HttpClient`. `AudioCache` bọc factory trong `Lazy` bắt giữ `CancellationToken` của caller **đầu tiên** — caller khác huỷ sẽ làm hỏng entry dùng chung. Ba `ConcurrentDictionary` trong singleton MOCK chỉ thêm, không bao giờ dọn. `RetentionJobHost` gọi `StopApplication()` trong `finally`, giết luôn scheduler/normalizer/callback vì chung host.

**Code chết.** `SimChannelLeaseRepository` được khai là artifact của P2-3 và đăng ký DI nhưng scheduler không dùng. `CurrentGoldenHourCompatMapper` được unit-test nhưng không dùng ở runtime; transport tự nhân bản logic. Nhánh đọc ngược trạng thái recording (DT-05) là code chết vì `FakeSimGateway.CheckHealthAsync` không bao giờ trả trạng thái đó.

**Evidence.** `W-0019` thiếu 2/3 block sample mà §10 yêu cầu. `W-0023` dẫn tên bảng và khoá cấu hình không tồn tại. `W-0064` khai `executionMode: MOCK_TEST_DB` trong khi fixture thật chạy `LAB_REAL_SIM`. Evidence `W-0014`/`W-0015`/`W-0016` bị sửa lùi để đổi từ "hosted `NOT_RUN`" thành "hosted PASS". `W-0017` được nâng `ACCEPTED` bằng self-review, "explicit IVR owner authorization" chỉ tồn tại trong lời của chính tác nhân đã thực hiện — **cùng vấn đề đã nêu với `W-0010` ở lượt trước**. Phạm vi quét PII và Gitleaks bị thu hẹp dần qua các work item. Row `W-0018` ở tracker §5 bị lệch cột.

---

## 8. Đã bác bỏ — đừng sửa

9 phát hiện không qua được vòng phản biện. Đáng chú ý:

- **"Attempt policy bị hard-code"** — sai. Giá trị 150s/450s nằm trong `CandidateAttemptPolicies` gắn nhãn `AttemptPolicyApproval.CandidateMockLabOnly`, và `PostgresAttemptPolicyRegistry.ResolveAsync` nhận `executionMode` làm tham số. Đúng hình dạng prompt yêu cầu.
- **"Snapshot task có thể sửa được"** — sai. `PersistenceInvariantValidator:33-52` chặn ở tầng EF cho 14 cột contract/policy/speech, cộng trigger DB.
- **"IVR ghi order state"** — sai. `order_state` chỉ xuất hiện như trường **đọc** trong client generated.

---

## 9. Những phần làm tốt

| Hạng mục | Kết quả |
|---|---|
| Ma trận DTMF | `"1" → IvrConfirmed`, `"0" → IvrCustomerCancelled` — không đảo. Lỗi kỹ thuật `IsCounted=false`, no-answer `IsCounted=true`, số hỏng `IsCounted=false` + final + human review |
| D-05 phòng thủ nhiều lớp | `OpaqueReferenceGuard.EnsureNotRawPhone` (domain) + `LooksLikeRawPhone` (intake) + `PersistenceInvariantValidator` chặn theo tên cột (EF) |
| D-02 | Không có đường ghi order state ngược sang Sales |
| Immutability | Snapshot contract/policy/speech bất biến ở cả EF lẫn trigger PostgreSQL; audit có `trg_ivr_audit_log_append_only` |
| Attempt policy | Versioned, có approval level, resolve theo execution mode |
| Seed | Token tổng hợp (`dial-token-fake-gh-0001`), 0 số điện thoại thật trong `seed/` và `docs/evidence/` |
| Test suite | 264 pass, 0 skip; integration chạy thật trên PostgreSQL |
| Build | 0 warning với `TreatWarningsAsErrors=true` và analyzer bật |
| NU1903 | Sửa sạch bằng direct `PackageReference`, không suppress |

---

## 10. Thứ tự đề nghị

| Bước | Việc | Vì sao ở vị trí này |
|---|---|---|
| 1 | **E-01, E-02** — cho luồng chạy được | Không có bước này thì mọi thứ khác chỉ là lý thuyết. Cần một `InMemorySchedulerStore` (hoặc cho MOCK dùng PostgreSQL) **và** một đường provisioning kênh SIM |
| 2 | **E-04, E-05, E-06, E-07** — bốn chỗ khoá chết | Tất cả sẽ xuất hiện ngay trong lần chạy end-to-end đầu tiên. Sửa cùng lúc vì chung một chủ đề: trạng thái vào được nhưng không ra được |
| 3 | **E-03, E-10, E-11** — ba lỗi trên cùng một dòng regex | Cùng gốc, sửa một lần. Chặn khách thật và làm thủng D-05 |
| 4 | **E-08, E-09** — hai regression khoá bởi test sai | Sửa code **và** sửa test đang khoá hành vi sai |
| 5 | **E-13** — tái sinh changelog, xoay baseline, bump version | Đang làm đỏ pipeline và nói sai với đội tích hợp |
| 6 | **E-14** — làm rõ trạng thái protected branch | Quyết định của owner. Nên chốt trước khi nói tới nghiệm thu |
| 7 | E-12, E-15…E-23 | Gộp thành một work item dọn dẹp |
| 8 | Nhóm P3 | Phân bổ vào phase tương ứng |

---

## 11. Phạm vi và phương pháp

Chỉ Phase 1 và Phase 2. Phase 0 đã rà soát ở lượt trước; chỉ nhắc khi P1/P2 chạm vào một cam kết của nó.

18 tác nhân rà soát song song theo 18 lăng kính (12 theo prompt, 6 cắt ngang: bảo mật/privacy, tính đúng đắn phân tán, chất lượng test, độ trung thực evidence, trôi lệch contract, chất lượng .NET). Mỗi tác nhân được một tác nhân độc lập phản biện với nhiệm vụ **bác bỏ**. Cộng một tác nhân bổ khuyết tìm vùng chưa ai soi. 138 phát hiện thô → 9 bị bác bỏ → gộp trùng còn khoảng 30 nhóm.

Toàn bộ mục P0 và P1 được tôi tự kiểm chứng lại bằng lệnh chạy thật trước khi đưa vào báo cáo này.

**Không có file nào bị sửa trong lượt rà soát.** File này là sản phẩm duy nhất được tạo ra.

Sales Platform: `NOT_VERIFIED_FROM_CURRENT_SALES_SOURCE`.
