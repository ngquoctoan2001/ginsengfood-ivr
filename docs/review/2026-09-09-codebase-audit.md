# Rà soát codebase — 2026-09-09

Phạm vi: toàn bộ repo tại `main@c0e6609`. Không có mục khen. Mỗi mục đều có `file:line`.

**Quy mô thực tế**

| Hạng mục | Số liệu |
|---|---|
| C# thật (trừ migration + `.g.cs`) | 37.628 dòng / 208 file |
| Migration + generated | ~104.500 dòng |
| Script CI `.mjs` | 23.532 dòng / 51 file |
| Markdown trong `docs/` | 470.337 dòng / 464 file |
| Test | 538 `[Fact]` + 81 `[Theory]`, 0 skip |
| Index EF | 109 |
| Magic string `UPPER_SNAKE` trong `src/` | 951 lượt / 461 giá trị |

Tài liệu gấp **12,5 lần** code.

---

## 1. Bug tiềm ẩn

### B1 — MEDIUM: crash-loop tiềm ẩn nếu bất biến ngầm bị phá — ĐÃ SỬA

> **Đính chính (2026-09-09, sau khi sửa).** Bản đầu của báo cáo này xếp B1 là CRITICAL và
> khẳng định retention tạo ra row mồ côi. **Khẳng định đó sai** và đã được kiểm chứng ngược lại:
> xem "Vì sao retention không gây ra được" bên dưới. Phần phân tích hậu quả vẫn đúng; phần
> reachability thì không. Mức đúng là MEDIUM — lỗ hổng phòng thủ, không phải bug đang xảy ra.

`src/Ivr.Infrastructure/Persistence/Outbox/CallbackOutboxRepository.cs:93-162`

```csharp
await transaction.CommitAsync(cancellationToken);          // dòng 151 — commit TRƯỚC
return rows.Select(row => new CallbackOutboxMessage(
        ...
        tasks[row.TaskId].ProgramType,                      // dòng 155 — indexer, không TryGetValue
        tasks[row.TaskId].CorrelationId,
```

Cùng lỗi ở site thứ hai: `CompleteDeliveryAsync` dùng `SingleAsync` trên `ConfirmationTasks`
(dòng ~242), sau khi `ExecuteUpdateAsync` đã chạy.

**Hậu quả nếu `task_id` không resolve được** (phần này đúng):

- `tasks[row.TaskId]` ném `KeyNotFoundException` **sau khi** transaction đã commit. N row trong batch đã bị đánh dấu `SENDING` + gán lease.
- `CallbackDeliveryJobHost.cs:60-65` bắt, log, `liveness.Fault(...)`, chờ 1 giây, chạy lại.
- Query reclaim (`CallbackOutboxRepository.cs:121-123`) chọn lại đúng row đó: `delivery_status = 'SENDING' AND lease_expires_at < now`.
- Ném lại. Vô hạn. Một row chặn toàn bộ hàng đợi callback vì exception xảy ra ở bước projection, trước khi bất kỳ message nào được dispatch.

**Vì sao retention KHÔNG gây ra được** — điều tôi đã bỏ sót:

`ivr_result_callbacks.task_id` đúng là không có FK. Nhưng bất biến được giữ **bắc cầu** qua hai
FK khác, cả hai đều `RESTRICT` và cả hai đều có thật trong schema:

```
FK_ivr_result_callbacks_ivr_call_results_ivr_call_result_id   → RESTRICT
   (20260812142435_P1_2_InitialTargetV1Persistence.cs:533)
FK_ivr_call_results_ivr_confirmation_tasks_task_id            → RESTRICT
   (PersistenceModelConfiguration.cs:318-322)
```

Còn callback thì không xóa được result; còn result thì không xóa được task. Retention
(`RetentionTargetCatalog.cs:59-65`) còn khai báo rõ `dependencyBlockedSql` tôn trọng đúng chuỗi
này. Và `CallbackOutboxSnapshotFactory.cs:86-90` đặt `TaskId = job.TaskId` cùng
`IvrCallResultId = resultId` của chính job đó, nên đường ghi production luôn nhất quán.

**Cái còn lại là thật:** bất biến giữ cho hai lời gọi kia an toàn là *bắc cầu và không được phát
biểu ở đâu cả*. Không có FK nào trên `task_id`, không có check constraint, không có comment. Một
script sửa dữ liệu, một migration làm `ivr_call_result_id` nullable, hay một thay đổi đặt `TaskId`
từ nguồn khác — đều tái tạo được crash-loop trên. Chi phí của sự cố hoàn toàn không tương xứng
với xác suất tới của nó.

**Đã sửa:**

- `DequeueReadyAsync` phân loại trước khi claim: row không resolve được task sẽ được
  dead-letter (`INVALID_DEAD_LETTER` + `LastError = CALLBACK_TASK_MISSING`) **trong chính
  transaction claim**, kèm audit row và review item `OPEN` để operator nhìn thấy. Row rời hàng đợi
  thay vì quay vòng trong đó. Nó không bao giờ tồn tại ở trạng thái claimed-nhưng-vô-chủ.
- `CompleteDeliveryAsync` chuyển `SingleAsync` → `SingleOrDefaultAsync`, dùng correlation id suy
  ra từ callback id khi task đã biến mất. Bản update đã áp dụng được giữ lại thay vì rollback.
- Test hồi quy `IT-DB-OUTBOX-09` dựng đúng trạng thái đó (result hợp lệ, `task_id` trỏ vào hư
  không), khẳng định dequeue trả về rỗng thay vì ném, row thành terminal, review item được tạo, và
  **lượt poll thứ hai cũng không thấy gì** — chỗ vòng lặp từng khép lại. Đã kiểm chứng test này
  **fail trên code chưa sửa**.

---

### B2 — HIGH: lỗi hạ tầng tạm thời bị biến thành thất bại vĩnh viễn — ĐÃ SỬA

`src/Ivr.Infrastructure/Callbacks/CallbackDispatcher.cs:85-133`

```csharp
transportResult = contract switch
{
    SalesCallbackContractKind.TargetV1 => await targetTransport.SendAsync(...),   // dòng 91
    SalesCallbackContractKind.CurrentGoldenHourCompat => await currentTransport.SendAsync(...),
    _ => throw new InvalidOperationException("Callback contract is unsupported."), // dòng 99
};
}
catch (InvalidOperationException)                                                  // dòng 102
{
    transportResult = new CallbackTransportResult(
        CallbackTransportOutcome.Invalid, null,
        "CALLBACK_ADAPTER_SELECTION_REJECTED", "CALLBACK_ADAPTER_SELECTION_REJECTED");
}
```

`catch` ở dòng 102 rõ ràng được viết để bắt cái `throw` ở dòng 99. Nhưng nó bọc cả hai lời gọi `SendAsync`. Mọi `InvalidOperationException` ném ra từ bên trong transport đều bị nuốt và dán nhãn sai:

- `HttpClient` cấu hình sai hoặc `BaseAddress` null
- `InvalidOperationException` từ EF: *"The connection is already open"*, *"A second operation was started on this context"*
- `JsonSerializer` ở trạng thái không hợp lệ

Kết quả: outcome = `Invalid` (terminal, **không retry**), và `circuitBreaker.RecordSuccess()` được gọi ở dòng 130-131 vì nhánh này không phải `TransientFailure`.

→ Sự cố tạm thời → callback chết vĩnh viễn, breaker báo "khỏe", error code dẫn người điều tra đi sai hướng hoàn toàn.

Chính codebase này biết pattern đúng: `TargetV1CallbackTransport.cs:37` viết `catch (Exception exception) when (exception is JsonException or InvalidOperationException)` với phạm vi hẹp. Chỗ này quên áp dụng.

**Đã sửa:** resolve contract tách ra `TryResolveContract`, một `try` chỉ bọc đúng lời gọi
selector, không bọc gì khác. Transport có `try` riêng: `OperationCanceledException` giữ nguyên
hành vi, mọi thứ còn lại thành `TransientFailure`. Nhánh `_ =>` trả về giá trị thay vì `throw`,
nên một `SalesCallbackContractKind` thêm về sau không thể tái mở lỗi này.

Hành vi tài liệu hoá cho trường hợp thật (`docs/evidence/W-0107/vocabulary-review.md:443`) giữ
nguyên: chương trình không adapter nào phục vụ vẫn là `CALLBACK_ADAPTER_SELECTION_REJECTED` và vẫn
terminal.

Hai test: `UT-CALLBACK-TRANSPORT-INVALIDOP-14` (transport ném `InvalidOperationException` →
`RETRY_PENDING` + breaker ghi nhận transient failure) và `UT-CALLBACK-ADAPTER-REJECT-15` (chương
trình không định tuyến được → vẫn `INVALID_DEAD_LETTER`). Test đầu đã kiểm chứng **fail trên code
chưa sửa**; test sau pass ở cả hai phía, đúng vai trò chốt bảo toàn hành vi.

---

### B3 — MEDIUM: PII guard nổ nhưng không để lại dấu vết nào — ĐÃ SỬA

`src/Ivr.Api/Application/EligibilityService.cs:177-208`

```csharp
PiiGuard.EnsureSafeText(JsonSerializer.Serialize(capacity, JsonOptions));  // dòng 186
...
catch (Exception)                                                          // dòng 196
{
    return new EligibilityCapacitySnapshot(false, false, "CAPACITY-SOURCE-ERROR", ...);
}
```

`catch (Exception)` không bind biến, không log, không đếm metric. Exception bị vứt đi hoàn toàn.

`PiiGuard.EnsureSafeText` ném khi PII sắp rò rỉ ra ngoài. Sự kiện đó — đúng loại sự kiện mà cả tầng `Governance/` tồn tại để bắt — bị nuốt và trả về `CAPACITY_SOURCE_UNREADABLE`, không phân biệt được với "Postgres đang chết".

Capacity provider hỏng vĩnh viễn trông y hệt vận hành fail-closed bình thường. Không ai biết. Không có gì để grep.

Cùng vấn đề, mức nhẹ hơn (có comment biện minh nhưng vẫn không log/metric): `FeatureFlagPlatform.cs:71`, `RuntimeGateApprovals.cs:149`.

---

### B4 — MEDIUM: không có backoff, DB chết = bão log + bão connection — ĐÃ SỬA

`src/Ivr.Worker/Jobs/CallbackDeliveryJobHost.cs:37-70`
`src/Ivr.Worker/Jobs/NormalizationJobHost.cs:38-68`
`src/Ivr.Worker/Jobs/SchedulerJobHost.cs:15`

Mặc định `PollIntervalMilliseconds = 1000` (`CallbackDeliveryOptions.cs:28`, `ResultRepository.cs:23`, `SchedulerCapacity.cs:40`).

Cả ba vòng lặp: catch-all → log `Error` kèm stack trace → chờ đúng 1000 ms → thử lại. Không exponential backoff, không jitter, không circuit breaker trên chính vòng lặp (`CallbackCircuitBreaker` chỉ bảo vệ tầng HTTP egress, không bảo vệ DB).

Postgres chết 10 phút, 2 replica worker: `3 loop × 600 lần × 2 = 3.600` dòng error kèm stack + 3.600 lần mở connection. Log storm che mất chính nguyên nhân gốc, và cơn bão connection kéo dài thời gian DB phục hồi.

---

### B5 — MEDIUM: rò rỉ bộ nhớ không giới hạn ở chế độ mặc định của image — ĐÃ SỬA

`src/Ivr.Infrastructure/Idempotency/InMemoryIdempotencyStore.cs:13-16`

```csharp
private readonly ConcurrentDictionary<string, SemaphoreSlim> keyLocks = new(StringComparer.Ordinal);
private readonly ConcurrentDictionary<string, IdempotencyKeyRecord> records = new(StringComparer.Ordinal);
```

- `keyLocks`: một `SemaphoreSlim` cho **mỗi** idempotency key, không bao giờ remove, không bao giờ `Dispose`. `SemaphoreSlim` là `IDisposable`.
- `records`: không TTL, không eviction, không giới hạn kích thước. Có ghi `timeProvider.GetUtcNow()` ở dòng 54 nhưng **không có code nào đọc lại** — timestamp đó chỉ là trang trí.
- Đăng ký `Singleton` (`ServiceCollectionExtensions.cs:104-106`), tức là sống suốt đời tiến trình.

Có guard: chỉ được phép ở `MOCK` (`ServiceCollectionExtensions.cs:95-102`). Nhưng `MOCK` là mặc định của image production (`deploy/docker/Dockerfile.api`: `IVR_EXECUTION_MODE=MOCK`), tức là chính chế độ chạy soak test, E2E dài ngày và pilot. Ở đó OOM chỉ là vấn đề thời gian.

---

### B6 — MEDIUM: hai store idempotency serialize khác nhau — ĐÃ SỬA

`PostgresIdempotencyStore.cs:105-110` đăng ký `ReadOnlySetJsonConverterFactory`:

```csharp
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
options.Converters.Add(new ReadOnlySetJsonConverterFactory());
```

`InMemoryIdempotencyStore.cs:10-11` thì không:

```csharp
private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
```

Response chứa read-only set sẽ replay đúng ở Postgres và hỏng ở in-memory. Divergence nằm ngay trong lớp bảo đảm tính đúng đắn của idempotency, và nằm đúng ở chế độ dùng để diễn tập trước khi lên production.

---

### B7 — MEDIUM: secret bị phơi ra qua chính API public của lớp bảo vệ nó — ĐÃ SỬA

`src/Ivr.Infrastructure/Auth/RotatingCredentialProvider.cs`

Doc-comment ở dòng 176-180 tuyên bố: *"an audit row that quoted the value would be the leak it exists to record."* Nhưng:

```csharp
public sealed record CredentialGeneration(
    int Generation,
    byte[] SecretBytes,          // public property, mảng là reference type
    ...);

public IReadOnlyList<CredentialGeneration> ActiveGenerations { get; }   // dòng 43-54
```

`provider.ActiveGenerations[0].SecretBytes` trả thẳng secret thô. `IReadOnlyList` chỉ bảo vệ danh sách, không bảo vệ nội dung mảng bên trong — caller còn ghi đè được.

Ba vấn đề phụ trong cùng file:

- `record` chứa `byte[]` → `Equals`/`GetHashCode` sinh tự động dùng **reference equality** cho mảng. Hai generation cùng secret so ra khác nhau.
- `Fingerprint` (dòng 172-176): SHA-256 thô cắt còn 48 bit, không salt, không iterate. Comment biện luận an toàn bằng **độ dài** (`MinimumSecretLength = 24`), nhưng brute-force phụ thuộc **entropy**. Secret 24 ký tự do người đặt vẫn bị dictionary attack ra fingerprint trong vài giây.
- `audit` list (dòng 32) tăng vô hạn, không bao giờ trim — trong khi `generations` ngay bên cạnh **có** trim (dòng 156). Property `Audit` copy toàn bộ list mỗi lần đọc.

Về claim constant-time ở dòng 100-106: vòng lặp cố tình không short-circuit, nhưng `generation.IsValidAt(now) && FixedTimeEquals(...)` vẫn short-circuit ở vế trái, và `FixedTimeEquals` trả `false` ngay khi độ dài khác nhau. Độ dài secret vẫn rò qua timing. Comment mô tả một đảm bảo mạnh hơn code thực tế.

**Đã sửa (W-0258):** `CredentialGeneration` không còn mang `byte[]` — provider giữ **SHA-256 digest** trong một type private, và chỉ cần xác minh chứ không bao giờ cần tiết lộ. So sánh giờ là hai digest 32 byte nên không còn độ dài để rò, và bỏ mảng khỏi record cũng hết luôn lỗi reference-equality. `Fingerprint` chuyển sang PBKDF2-HMAC-SHA256 210k vòng + salt tách miền; tham số chọn bằng đo (34 ms/lần, tối đa 8 lần lúc khởi động ≈ 275 ms) chứ không bằng ước lượng.

**Không sửa danh sách `audit`**, và lý do quan trọng hơn việc sửa: cắt bớt lịch sử xoay khoá làm hỏng một bản ghi tuân thủ. Một bản ghi âm thầm quên tệ hơn một bản ghi to dần. Thực tế nó giữ 1–2 mục mỗi scope suốt đời tiến trình vì không có gì xoay theo lịch. Đã ghi lý do vào code.

---

### B8 — MEDIUM: validator CI fail-closed toàn bộ nếu checkout nằm dưới symlink — ĐÃ SỬA

9 trên 10 bản `isConfined` so `realpathSync(resolved)` với `REPOSITORY_ROOT` **chưa** realpath:

- `attempt-policy-production-bundle-validator.mjs:172`
- `capacity-registry-decision-pack-validator.mjs:291`
- `d06-revalidation-evidence-validator.mjs:455`
- `external-decision-closure-validator.mjs:246`
- `target-v1-shared-e2e-report-validator.mjs:299`

Chỉ `b3-telephony-evidence-validator.mjs:872` làm đúng: `const repositoryReal = realpathSync(REPOSITORY_ROOT);`

Nếu runner checkout vào path chứa symlink — rất phổ biến: `/tmp` trên macOS, workspace symlink của GitLab runner, mount trong container — thì `realpathSync(resolved)` trả path thật còn `REPOSITORY_ROOT` là path symlink. `relative()` cho ra `../../..`, `isConfined` trả `false` cho **mọi** file hợp lệ, gate fail toàn bộ với thông báo sai hoàn toàn: *"real path escapes repository root"*.

Hỏng ở chỗ khó debug nhất: gate bảo mật báo tấn công traversal trong khi thực tế chỉ là đường dẫn checkout.

**Đã sửa (W-0258):** không vá mười bản — vá mười bản là để lại đúng cơ chế đã sinh ra lỗi. `deploy/ci/scripts/repository-path-lib.mjs` giữ `REPOSITORY_ROOT` (đã realpath) và `isConfined`; mười validator import từ đó, hai semantics phân hoá hợp nhất về dạng đúng.

Sửa mười script làm drift 11 hash được ghim, thuộc **hai vai**: pin sống (3 tệp — cập nhật) và bản ghi đóng băng dưới `W-0180`/`W-0182`/`W-0188` (**không đụng**, theo đúng luật `W-0251` dựng: một attestation đúng khi nó cũ).

Guard mới `PATH_CONFINEMENT_SINGLE_SOURCE_PASS` trong `ci-config-selftest.mjs` khẳng định lib canonicalise root **và** không script nào định nghĩa `isConfined` riêng — đã chứng minh đỏ được ở cả hai nửa.

---

### B9 — LOW: selftest xả rác ra repo root, cleanup không chạy

`deploy/ci/scripts/external-decision-response-validator.mjs:692-711`

```javascript
function runSelfTest() {
  const temporaryRoot = mkdtempSync(join(REPOSITORY_ROOT, ".w0165-selftest-"));  // 693 — NGOÀI try
  const manifest = parseAndVerifyManifest();                                     // 694 — NGOÀI try, có thể ném
  ...
  try {                                                                          // 711 — try bắt đầu ở đây
```

`parseAndVerifyManifest()` là bước verify hash — ném là chuyện bình thường khi manifest lệch. Khi đó `finally` không chạy và temp dir kẹt lại ở **repo root**.

Bằng chứng đang nằm trong repo ngay lúc này:

```
.w0165-selftest-XeqCsv/  .w0165-selftest-ZzOaCn/  .w0165-selftest-cpZY4k/
.w0165-selftest-pgqCbG/  .w0165-selftest-yOq6cB/
```

Năm thư mục rỗng. Cùng lỗi ở `capacity-registry-decision-pack-validator.mjs:842` (`.w0182-selftest-`). Không pattern nào có trong `.gitignore`, nên nếu lần sau chúng chứa file thì sẽ hiện ra như untracked noise.

---

### B10 — LOW: cột chết nhưng index vẫn sống trên hot path

`IdempotencyKeyEntity.ExpiresAt` (`IvrPersistenceEntities.cs:372`) **không có writer nào** trong toàn bộ `src/`. `PostgresIdempotencyStore.cs:70-77` chỉ set `Scope`, `Key`, `PayloadHash`, `ResponseSnapshotJson`, `CreatedAt`.

Nhưng cột này có index: `PersistenceModelConfiguration.cs:566`. Index đó được bảo trì trên **mọi** insert idempotency, tức là mọi request mutating của API, để index một cột luôn `NULL`.

Retention xóa bảng này theo `created_at` (`RetentionTargetCatalog.cs:90`), không dùng `expires_at`.

---

## 2. Vấn đề performance

### P1 — Dashboard admin: 12 round trip tuần tự, 5 lần semi-join full-table — ĐÃ SỬA

> **Đính chính (2026-09-09).** Bản đầu viết "~30 round trip" và dẫn "30 `await`". Sai: con số đó
> ra từ `grep -c 'await '` trên một khoảng dòng `awk` cắt tràn sang method kế tiếp, và mỗi query
> đóng góp hai lần vì `ConfigureAwait(false)` nằm dòng riêng. Đếm theo lời gọi thực sự chạm
> database thì là **12 query + 1 lần mở context**. Vấn đề vẫn thật, chỉ không lớn bằng con số đã in.

`src/Ivr.Api/Application/AdminReadService.cs:75-200`

Vấn đề nặng hơn số lượng:

```csharp
IQueryable<string> jobIds = jobs.Select(job => job.IvrCallJobId);           // dòng 126
...
.Where(result => jobIds.Contains(result.IvrCallJobId))                      // dòng 144
.Where(attempt => jobIds.Contains(attempt.IvrCallJobId))                    // dòng 150
// + attemptTotal, countedAttempts, technicalRetries, activeAttempts        // 152-160
```

`jobIds` là `IQueryable` chưa thực thi. Nó chỉ xuất hiện 2 lần trong mã, nhưng lần thứ hai gán vào `attempts`, và `attempts` được **thực thi 4 lần** — mỗi `CountAsync`/`SumAsync` là một query riêng mang theo trọn subquery. Cộng `resultCounts` là **5 lần** subquery chạm database.

Khi gọi không filter (`program` null, không truyền khoảng ngày — chính là request mặc định khi mở dashboard), `jobs` là toàn bộ `ivr_call_jobs`, nên mỗi lần là `WHERE ivr_call_job_id IN (SELECT ivr_call_job_id FROM ivr_call_jobs)` — semi-join full-table, 5 lần.

Thêm nữa, dòng 163-165 load nguyên bảng vào bộ nhớ, không `Take`, không projection:

```csharp
List<SimChannelEntity> channels = await context.SimChannels.AsNoTracking()
    .ToListAsync(cancellationToken)
```

**Và một bug đi kèm phát hiện lúc sửa:** query đó không có `ORDER BY`, nhưng `BuildSimPanel` báo adapter mode của `channels[0]`. Query không `ORDER BY` thì không có "dòng đầu tiên" — PostgreSQL trả về thứ tự nào cũng hợp lệ, đổi theo plan và theo vacuum. Ô adapter mode trên dashboard có thể đổi giữa hai lần F5 mà không có gì thay đổi. Chưa lộ vì môi trường thật mới có một channel.

**Đã sửa (W-0257):** gộp 2 tile của `jobs` và 4 counter của `attempts` thành hai grouped aggregate → **12 query còn 8**, subquery `jobIds` **5 lần còn 2**. `SimChannels` chiếu còn 5 cột và sắp theo `sim_channel_id`. Không đặt `Take()` vì chặn số lượng ở đây làm panel đếm thiếu trong im lặng.

Rủi ro tự tạo: `GroupBy(_ => 1)` trả **không group nào** trên bảng rỗng, khác `CountAsync` trả 0. Chặn hai lớp — nullable analyzer + `TreatWarningsAsErrors` khiến nhánh null không bỏ qua được (đã kiểm chứng: gỡ `??` thì build đỏ `CS8600`/`CS8602`), và `IT-ADMIN-READ-12` chốt rằng câu trả lời đúng là số 0 chứ không phải lỗi.

---

### P2 — 109 index, trong đó nhiều index trên cột boolean

`PersistenceModelConfiguration.cs` — 109 lần `HasIndex`.

| Bảng | Số index | Cột boolean / cardinality thấp được index riêng |
|---|---|---|
| `ivr_confirmation_tasks` | 18 | `IvrConfirmationRequired`:88 (bool), `OrderState`:86, `PaymentMethodSnapshot`:87, `PhoneValidationStatus`:95, `EligibilityDecision`:97, `ProgramType`:90 |
| `ivr_call_jobs` | 12 | `Eligible`:174 (bool), `EligibilityDecision`:175, `QueueStatus`:176 |
| `ivr_call_attempts` | 14 | `IsCountedCustomerAttempt`:281 (bool), `Status`:278, `ResultStatus`:279, `Disposition`:280 |
| `ivr_call_results` | — | `IsCountedCustomerAttempt`, `IsFinalForIvr`, `HumanReviewRequired` (3 bool) |

Cộng thêm 3 shadow index cho **mỗi** bảng kế thừa `RetainedEntity`: `RetainUntil`, `LegalHoldUntil`, `AnonymizedAt`.

Btree trên boolean ở Postgres gần như vô dụng — selectivity ~50%, planner sẽ chọn seq scan — nhưng vẫn phải maintain trên mọi `INSERT`/`UPDATE`, vẫn bloat, vẫn tốn buffer cache và VACUUM.

Đây là thuế ghi đặt trực tiếp lên hot path `intake → job → attempt → result → callback`, tức là đúng đường đi của mọi cuộc gọi.

Cần đo `pg_stat_user_indexes.idx_scan` trên môi trường thật trước khi drop — nhưng với boolean thì kết quả gần như chắc chắn là 0.

---

### P3 — DSAR: 8 round trip tuần tự + nguy cơ nổ giới hạn tham số — ĐÃ SỬA

`src/Ivr.Infrastructure/Governance/DsarService.cs:100-148`

`orderIds` (dòng 111-117) rồi `jobIds` (dòng 124-129) được materialize về client, sau đó **gửi ngược lại** làm `IN (...)` trong 4 query: `CallAttempts`, `CallResults`, `ResultCallbacks`, `AnalyticsFacts`. Postgres giới hạn 65.535 tham số cho một statement.

8 count query chạy tuần tự, không transaction, không snapshot → báo cáo DSAR không nhất quán nếu retention chạy song song. Người yêu cầu nhận được một bức ảnh ghép từ 8 thời điểm khác nhau.

`EraseAsync` (dòng 155-185): đếm `matched` rồi update `redacted` ở hai statement rời, không cùng transaction. Hai con số ghi vào audit row có thể mâu thuẫn với nhau, và audit row là thứ tồn tại để giải quyết tranh chấp.

---

### P4 — 4/5 background loop không test được nhịp — ĐÃ SỬA

Chỉ `AnalyticsEtlJobHost.cs:36` truyền `timeProvider` vào `PeriodicTimer`:

```csharp
using PeriodicTimer timer = new(period, timeProvider);
```

Bốn cái còn lại dùng đồng hồ thật: `CallbackDeliveryJobHost.cs:26`, `NormalizationJobHost.cs:27`, `SchedulerJobHost.cs:15`, `IvrHeartbeat.cs:25`. Test nhịp của chúng buộc phải sleep thật, hoặc không test được. Trong khi `TimeProvider` đã được đăng ký DI sẵn (`ServiceCollectionExtensions.cs:84`).

---

## 3. Code smell

### S1 — God class kèm interface 16 method — ĐÃ SỬA KHỚP NỐI

`src/Ivr.Api/Application/InternalAdminApiService.cs` — 1.265 dòng, file `.cs` lớn nhất không phải generated.

> **Đính chính (W-0263).** **15** method, không phải 16.

**Đã sửa (W-0263):** tách thành `IIvrLifecycleApiService` (6) và `IIvrAdminOperationsService` (9), đúng hai nhóm consumer đã tồn tại — khác biệt nhìn thấy trong chữ ký: mọi method nhóm sau nhận `actorId`, không method nào nhóm đầu nhận. `DevToolingApiService` dùng 2/9 thay vì phụ thuộc cả 15. Composition root cho 6 service khác chuyển sang file riêng.

**Class giữ nguyên một khối, cố ý:** 15 method chia nhau wrapper idempotency, context factory và helper validation; tách class sẽ nhân đôi chúng hoặc cần một type thứ ba để giữ. 1.266 → 1.188 dòng. Cái đã sửa là **khớp nối**, không phải kích thước.

`IInternalAdminApiService` (dòng 22-39) có **15 method**, trộn lẫn: eligibility, call job, attempt, result, callback, pause/resume queue, enable/disable channel, technical retry, admin review, terminate call, terminate all. Mọi consumer phải phụ thuộc vào cả 16 dù chỉ dùng một.

File này còn kiêm luôn DI registration cho **6 service khác** (dòng 1200-1240):

```csharp
services.AddSingleton<IAdminReadService, AdminReadService>();
services.AddSingleton<IAdminConfigReadService, AdminConfigReadService>();
services.AddSingleton<IAnalyticsReadService, AnalyticsReadService>();
services.AddSingleton<IScriptLifecycleApiService, ScriptLifecycleApiService>();
services.AddSingleton<SeedCatalog>();
services.AddSingleton<IDevToolingApiService, DevToolingApiService>();
```

Composition root nằm nhầm chỗ: muốn tìm nơi đăng ký `AnalyticsReadService` thì phải đoán ra nó nằm trong file của `InternalAdminApiService`.

---

### S2 — 951 magic string, 461 giá trị riêng biệt — trong khi hằng số ĐÃ tồn tại — ĐÓNG MỘT PHẦN

Hằng số có sẵn, literal vẫn viết tay ngay bên cạnh.

`IvrOptions.LabRealSimExecutionMode = "LAB_REAL_SIM"` (`IvrOptions.cs:14`), nhưng `"LAB_REAL_SIM"` viết thô ở:
- `InternalAdminApiService.cs:871`
- `ScriptLifecycleApiService.cs:129`
- `AttemptPolicyRegistries.cs:107`
- `AttemptPolicyRegistryWriter.cs:117`
- `SpeechServiceCollectionExtensions.cs:149`

`IvrErrorCodes.OperationalBlocked = "IVR_OPERATIONAL_BLOCKED"` (`IvrErrorCodes.cs:23`), nhưng `DispositionMapper.cs:55` vẫn viết literal.

Nặng hơn — **cùng một enum tồn tại ở ba dạng độc lập**:
1. C# enum `ExecutionMode`
2. `const string` trong `IvrOptions`
3. enum sinh từ OpenAPI trong `IvrServerModels.g.cs:3356`

Với bảng ánh xạ `switch` viết tay lặp lại ở ít nhất 3 nơi (`AttemptPolicyRegistries.cs:107`, `AttemptPolicyRegistryWriter.cs:117`, `ScriptLifecycleApiService.cs:129`). Thêm một execution mode = ba chỗ có thể quên, không chỗ nào báo lỗi biên dịch.

Và raw SQL trong `PostgresSchedulerStore.cs:379-395` hardcode lại cùng tập trạng thái ở ngôn ngữ khác:

```sql
AND job.status IN ('READY_FOR_SCHEDULER', 'DISPATCH_LEASED', 'DRY_RUN', 'HELD_ADMIN_REVIEW')
```

trong khi C# gán `job.Status = "HELD_ADMIN_REVIEW"` bằng literal riêng.

> **Đính chính (W-0261).** Tôi viết rằng đổi tên một trạng thái sẽ hỏng **âm thầm**. Sai ở phía ghi: có `CHECK constraint` trên các cột status (`ck_ivr_call_jobs_status` liệt kê 30 giá trị), nên một literal sai bị database từ chối lúc INSERT/UPDATE. Đã đo cả phía đọc — trích 100 giá trị CHECK cho phép, đối chiếu 15 token `UPPER_SNAKE` trong vị từ raw SQL của `src/`: **không token nào sai**. Hai token bị probe gắn cờ nằm trên cột không có CHECK, tức false positive. **Không có bug đang tồn tại.** Mức đúng của S2 là chi phí bảo trì, không phải lỗi đang chạy — đã hạ xuống LOW.

**Đã sửa một phần (W-0261):** ba bảng ánh xạ `ExecutionMode → string` gộp về một `ExecutionModes.ToWireValue`; 11 literal `"LAB_REAL_SIM"`/`"PRODUCTION_REAL"` thay bằng hằng số; `ARCH-CONST-01` chặn tái phát.

**`"MOCK"` cố ý không đụng:** nó có **hai chủ sở hữu** — `IvrOptions.MockExecutionMode` và `FeatureFlagCatalog.MockSimProvider` — cho hai thứ khác nhau tình cờ viết giống nhau. 22 site, và thay tất cả sẽ sai ở khoảng một nửa. Cần owner tách tên trước.

---

### S3 — 5 background host copy-paste nguyên khối, kể cả comment — ĐÃ SỬA

> **Đính chính (W-0263).** Tôi viết "5 host" và đếm `RetentionJobHost` vào đó. **Sai** — nó chạy một lần rồi thoát, không timer, không liveness registration, không vòng lặp thất bại, nên không có pattern nào để trùng lặp. Số đúng là **bốn**.

**Đã sửa (W-0263):** skeleton chuyển vào `PollingJobHost`; 4 host từ 476 còn 337 dòng cộng 133 dòng base — gần như hoà về dòng, nhưng vòng lặp chỉ còn **một** câu trả lời cho "khi nào backoff, tick trước hay sau, exception nào kết thúc vòng lặp". Bốn test `UT-WORKER-LOOP-01..04` là lần đầu skeleton này được unit test.

Comment này từng xuất hiện **nguyên văn** ở 3 file (`AnalyticsEtlJobHost.cs`, `CallbackDeliveryJobHost.cs`, `NormalizationJobHost.cs`):

> *"Registered even though it will not run, so the report can tell a loop that was turned OFF from a loop that was never wired: the first is a decision, the second is a defect, and only one of them is worth a restart."*

Cùng skeleton lặp 5 lần: disabled check → `RegisterDisabled` → `PeriodicTimer` → `Register` → `try`/`catch`/`Tick`/`Fault` → `WaitForNextTickAsync`.

Và lặp không nhất quán — hai biến thể cú pháp cho cùng một ngữ nghĩa:
- `AnalyticsEtlJobHost.cs:43-84`: `do { ... } while (await timer.WaitForNextTickAsync(...))`
- 3 file kia: `while (!stoppingToken.IsCancellationRequested) { ... if (!await timer.WaitForNextTickAsync(...)) break; }`

Đây phải là một base class hoặc một helper nhận `Func<CancellationToken, Task>`.

---

### S4 — Helper bảo mật copy-paste 8–18 lần và đã phân hóa thành 2 semantics — PHẦN BẢO MẬT ĐÃ ĐÓNG

51 script trong `deploy/ci/scripts/`, chỉ **8** import module dùng chung.

Số bản sao của cùng một hàm:

| Hàm | Số bản |
|---|---|
| `sha256` | 18 |
| `assertExactKeys` | 13 |
| `clone` | 12 |
| `assert` | 12 |
| `assertString` | 11 |
| `isConfined` | 10 |
| `rejectDuplicateJsonKeys` | 8 |
| `readStrictJson` | 6 |

`isConfined` — hàm chặn path traversal — đã tách thành hai semantics khác nhau:

```javascript
// Bản A (5 bản): attempt-policy:161, capacity-registry:275, d06:443,
//                dial-token:206, target-v1-shared:292
return rel !== "" && !rel.startsWith("..") && !isAbsolute(rel);

// Bản B (3 bản): external-decision-response:139, external-decision-routing:98,
//                opt-out-suppression:340
return rel !== "" && rel !== ".." && !rel.startsWith(`..${sep}`) && !isAbsolute(rel);
```

Bản A quá chặt: từ chối cả thư mục hợp lệ có tên bắt đầu bằng `..`. Bản B đúng.

Cả hai đều an toàn về traversal, nhưng đây là **helper bảo mật**. Sửa một bug ở một bản không lan sang 9 bản còn lại — và B8 ở trên chính là ví dụ: bug realpath được sửa đúng ở `b3-telephony-evidence-validator.mjs` và không bao giờ tới 9 bản kia.

---

### S5 — Schema wire được định nghĩa hai lần bằng tay — ĐÃ BUỘC VÀO NHAU (W-0264)

`src/Ivr.Api/Intake/TaskIntakeEndpoint.cs:411-472` — 6 `HashSet<string>` chứa ~50 tên field viết tay:

```csharp
private static readonly HashSet<string> RequiredTaskProperties =
[
    "contract_version", "task_id", "order_id", "order_code", "order_version",
    "order_state", "payment_method_snapshot", "ivr_confirmation_required",
    ... // 23 field
];
```

Trùng lặp hoàn toàn với contract sinh tự động trong `src/Ivr.Contracts/Generated/IvrServer/V1/IvrServerModels.g.cs` (`[JsonPropertyName("program_type")]`...).

Thêm một field vào OpenAPI spec → regenerate contract → endpoint **vẫn từ chối** field đó vì allowlist viết tay chưa được cập nhật. Không test nào bắt được, vì cả hai định nghĩa đều tự nhất quán với chính nó.

**Đã sửa (W-0264):** năm test parity đọc allowlist của endpoint bằng reflection rồi đối chiếu với contract sinh tự động — bắt drift ở **cả hai chiều**, đã kiểm chứng bằng cách làm chúng đỏ thật.

**Không suy allowlist ra từ contract**, vì làm thế đổi thứ intake chấp nhận. Đo cho thấy đúng **một** chênh lệch thật: `phone_validation_status` nằm trong `required` của spec (`ivr-order-confirmation.v1.yaml:1193`) nhưng endpoint nhận task thiếu nó. Bắt buộc nó sẽ từ chối body hôm nay đang thành công — breaking change trên API service-to-service với Module 3 ở đầu kia. Khai báo như ngoại lệ **được assert**, chờ owner.

Phép đo phải chạy **ba lần** mới đáng tin; hai lần đầu sai do regex của chính tôi. Nếu hành động theo lần hai, tôi đã gỡ `pronunciation_hints` — một field **có** trong spec (dòng 1172) và **được `PrivacySafeSpeech` trong Domain đọc**.

---

### S6 — ~~434.000 dòng tài liệu không liên quan nằm trong repo code~~ — RÚT LẠI

> **Phát hiện này sai. Rút lại (W-0262), không phải hoãn.**
>
> Tôi lấy bốn file lớn nhất theo dòng, thấy Facebook gateway / ads ROAS / MC AI live-sales /
> commerce runtime, rồi kết luận cả 179 file là "không thuộc module IVR". Đó là suy rộng từ đầu một
> danh sách đã sắp theo kích thước, và nó sai.

Đo lại cho đủ:

| | |
| --- | ---: |
| Tài liệu **đặt tên riêng cho IVR** | **8 file / 17.810 dòng** |
| Tài liệu **có nhắc IVR** | **105 / 179** |

Trong đó có `2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` (7.385 dòng) và
`3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-...md` (8.337 dòng) — đặc tả
nghiệp vụ của **chính module này**.

Và nó không phải rác vô chủ. `prompt/README-governance.md:20` tuyên bố thẳng:

> `docs/documents/` **là business source để truy nguyên**; nó không tự chứng minh implementation
> hiện tại. Nếu Target V1 khác business source, delta phải được ghi ở
> `specs/_review/open-decisions-register.md` kèm owner, không được im lặng.

Bảy bản ghi evidence trích dẫn tài liệu cụ thể trong đó — `W-0057`, `W-0058`, `W-0136`, `W-0151`,
`W-0217`, `W-0238`, `W-0247`. `W-0247` còn ghi *"chỗ duy nhất trong toàn bộ `docs/documents/` nêu
`golden_hour_session_id`"*.

**"Tách sang repo riêng" như tôi đề xuất sẽ cắt đứt chuỗi truy nguyên mà governance yêu cầu và làm
mồ côi bảy trích dẫn trong evidence.** Đúng loại thiệt hại mà `W-0251`/`W-0252` tồn tại để tránh.

Cái còn đúng, và chỉ có thế: 434.464 dòng thật sự làm chậm `clone`, `grep -r` và mỗi lần index — đó
là cái giá của việc giữ corpus truy nguyên, không phải một khoản lãng phí để cắt. Tên thư mục có dấu
cách (`2. pack`, `3. tech`) cũng thật, nhưng gate sweep xanh nên hiện chưa có gì vỡ vì nó.

---

### S7 — Worktree rác nằm bên trong working tree — ĐÃ SỬA

```
$ git worktree list
C:/.../ivr                                              c0e6609 [main]
C:/.../ivr/.claude/worktrees/gd0-fixes                  cc12e53 (detached HEAD)
C:/.../ivr-p03-expand-contract                          d5539ba (detached HEAD) prunable
C:/.../ivr-p03-expand-contract/ci-artifacts/.../previous-source  ba43605 (detached) prunable
C:/.../ivr-w0128-w0129-candidate                        1fa0150 [codex/...] prunable
```

`.claude/worktrees/gd0-fixes` là **bản sao nguyên repo** nằm bên trong cây làm việc chính, ở commit cũ `cc12e53`, kèm `node_modules`, `.next`, `third_party/vieneu-tts`, và toàn bộ `admin-ui` + `deploy/docker/Dockerfile.ui` mà commit `c0e6609` vừa xóa khỏi `main`. Code zombie chờ được hồi sinh nhầm — và nó là thứ mà `find`/`grep` đệ quy tìm thấy trước.

Ba worktree `prunable` vẫn còn đăng ký. Cái đáng chú ý nhất:

```
ivr-p03-expand-contract/ci-artifacts/expand-contract/9c8c99f6.../previous-source
```

Một worktree được đăng ký **bên trong `ci-artifacts/`** — thư mục đã gitignore (`.gitignore:11`) và bị xóa định kỳ. Đó chính là lý do nó thành `prunable`. Đăng ký worktree vào thư mục artifact là footgun kiến trúc, không phải tai nạn một lần.

`git worktree prune` quá hạn từ lâu. Lưu ý: branch `codex/w0128-w0129-candidate` phải giữ theo `CLAUDE.md`; prune chỉ xóa **bản ghi worktree**, không xóa ref — nhưng cần kiểm tra lại sau khi chạy.

---

## 4. Chưa clean code

### C1 — `ConfigureAwait` nửa vời

442 chỗ dùng `ConfigureAwait(false)` trong `src/`. Nhưng:

- `PostgresIdempotencyStore.cs`: **0** chỗ
- `CallbackOutboxRepository.cs`: **0** chỗ trên 18 `await`

Hoặc là quy ước, hoặc là không. Nửa vời thì reviewer không phân biệt được chỗ thiếu là cố ý hay quên, và cả hai file này đều nằm trên đường ghi quan trọng nhất.

### C2 — `.gitignore` trùng lặp — ĐÃ SỬA MỘT PHẦN, và đính chính

`.claude/worktrees/` khai báo **hai lần**: dòng 16 và dòng 61, kèm hai block comment gần như y hệt. **Đã xoá bản không ghi work-id (W-0262).**

> **Đính chính.** Tôi cũng gọi các entry `admin-ui` là "lạc hậu". **Sai.** Chúng là **guard**, đúng theo tiền lệ repo tự viết cho `deployment-ui.yaml` — template đó được giữ cố ý vì xoá nó biến `ui.enabled` thành no-op im lặng. Cùng lập luận: nếu ai dựng lại `admin-ui`, các dòng ignore giữ nó ngoài git và ngoài image. Xoá đi là đổi một guard lấy vẻ gọn gàng. **Giữ nguyên.**

### C3 — Ba quy ước temp dir trong cùng một họ script

| Quy ước | Ví dụ |
|---|---|
| `tmpdir()` | `contract-freeze-selftest.mjs:43`, `capacity-data-intake-validator.mjs:1595` |
| `.artifacts/` | `d06-revalidation-evidence-validator.mjs:905`, `dial-token-production-bundle-validator.mjs:1183`, `external-decision-closure-validator.mjs:841` |
| `REPOSITORY_ROOT` | `external-decision-response-validator.mjs:693`, `capacity-registry-decision-pack-validator.mjs:842` |

Cái thứ ba là nguyên nhân trực tiếp của B9.

### C4 — `Dockerfile.api`: comment nói ngược với code

```dockerfile
# The chiseled images define a non-root app user (UID 1654). Stated explicitly rather than
# inherited, so IT-IMG-BUILD-01 can assert it and a base change cannot quietly drop it.
USER $APP_UID
```

`$APP_UID` **chính là** biến kế thừa từ base image. Muốn "stated explicitly" thì phải viết `USER 1654`. Comment mô tả một đảm bảo mà dòng code bên dưới không cung cấp.

### C5 — Bốn API service đều `Singleton` nhưng không có gì bảo vệ ràng buộc đó

`InternalAdminApiService`, `AdminReadService`, `AdminConfigReadService`, `AnalyticsReadService` đều `AddSingleton` (`InternalAdminApiService.cs:1200-1204`). Hiện tại an toàn vì tất cả dùng `IDbContextFactory` thay vì inject `DbContext`.

Nhưng không có gì — không comment, không analyzer, không test — ngăn người tiếp theo thêm một field mutable hoặc inject `IvrDbContext` trực tiếp vào một trong bốn class đó. Khi điều đó xảy ra, lỗi sẽ là race condition dưới tải, không phải lỗi biên dịch.

---

## 5. Thứ tự xử lý đề xuất

| # | Vấn đề | Mức | Chi phí sửa |
|---|---|---|---|
| ~~B2~~ | ~~`InvalidOperationException` quá rộng~~ | ~~HIGH~~ | **đã sửa — W-0255** |
| ~~B1~~ | ~~Crash-loop tiềm ẩn ở outbox~~ | ~~MEDIUM~~ | **đã sửa — W-0255** |
| ~~B3~~ | ~~PII guard bị nuốt im lặng~~ | ~~MEDIUM~~ | **đã sửa — W-0256** |
| ~~B4~~ | ~~Không backoff khi DB chết~~ | ~~MEDIUM~~ | **đã sửa — W-0256** |
| ~~B6~~ | ~~Hai store serialize khác nhau~~ | ~~MEDIUM~~ | **đã sửa — W-0256** |
| ~~P4~~ | ~~4/5 loop không test được nhịp~~ | ~~LOW~~ | **đã sửa — W-0256** (phụ phẩm của B4) |
| ~~P1~~ | ~~Dashboard 12 round trip + 5 semi-join~~ | ~~HIGH~~ | **đã sửa — W-0257** |
| ~~B7~~ | ~~Secret phơi qua `ActiveGenerations`~~ | ~~MEDIUM~~ | **đã sửa — W-0258** |
| ~~B8~~ | ~~Validator vỡ dưới symlink~~ | ~~MEDIUM~~ | **đã sửa — W-0258** |
| ~~B5~~ | ~~Rò rỉ bộ nhớ ở MOCK~~ | ~~MEDIUM~~ | **đã sửa — W-0259** |
| P2 | 109 index, nhiều cái trên boolean | MEDIUM | trung bình — đo `pg_stat_user_indexes` trước |
| ~~S4~~ | ~~Helper bảo mật copy-paste 18 lần~~ | ~~MEDIUM~~ | **phần bảo mật đã đóng — W-0258/W-0259/W-0260**; phần còn lại thuần kỹ thuật, census guard giữ không tăng |
| S2 | 951 magic string | LOW | **một phần — W-0261**; `"MOCK"` có hai chủ sở hữu, cần owner tách tên |
| ~~P3~~ | ~~DSAR 8 round trip, nổ tham số~~ | ~~LOW~~ | **đã sửa — W-0261** |
| ~~S7~~ | ~~Worktree rác + bản sao repo~~ | ~~LOW~~ | **đã sửa — W-0262** (942 MB thu hồi) |
| ~~S6~~ | ~~434k dòng docs không liên quan~~ | — | **RÚT LẠI — phát hiện sai (W-0262)** |
| ~~S1~~ | ~~God class + interface 15 method~~ | ~~LOW~~ | **đã sửa — W-0263** |
| ~~S3~~ | ~~4 background host copy-paste~~ | ~~LOW~~ | **đã sửa — W-0263** |
| ~~S5~~ | ~~Schema wire định nghĩa hai lần~~ | ~~LOW~~ | **đã buộc vào nhau — W-0264**; 1 chênh lệch chờ owner |
| B9, B10, C1, C3–C5 | — | LOW | — |

**18/26 đã đóng, 1 rút lại vì sai. Không còn mục HIGH nào.**

---

## 6. Nhật ký sửa

**2026-09-09 — B1 và B2 đã sửa.**

| File | Thay đổi |
|---|---|
| `src/Ivr.Infrastructure/Persistence/Outbox/CallbackOutboxRepository.cs` | Phân loại orphan trước khi claim; `DeadLetterOrphanAsync`; `SingleAsync` → `SingleOrDefaultAsync` |
| `src/Ivr.Infrastructure/Callbacks/CallbackDispatcher.cs` | `TryResolveContract` tách khỏi transport; `catch` thu hẹp phạm vi |
| `tests/Ivr.IntegrationTests/PostgresPersistenceTests.cs` | `IT-DB-OUTBOX-09` |
| `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `UT-CALLBACK-TRANSPORT-INVALIDOP-14`, `UT-CALLBACK-ADAPTER-REJECT-15` |
| `docs/traceability-tests.md` | Sinh lại (gate `FailGateTests` bắt buộc) |

Không đổi chữ ký public, không đổi schema, không migration.

Kết quả: **955/955 test xanh** — 649 unit, 274 integration (Testcontainers PostgreSQL 16), 24
contract, 8 chaos. Build sạch với `TreatWarningsAsErrors=true`.

Hai test hồi quy đã được kiểm chứng là **fail trên code chưa sửa** rồi mới ghi nhận là pass — một
test xanh ở cả hai phía không chứng minh được gì.

`gitnexus detect_changes` xếp thay đổi ở mức `critical` vì `RunBatchAsync` và `DequeueReadyAsync`
nằm trên 16 execution flow. Cả 16 flow đó đều nằm trong các suite đã chạy ở trên, gồm cả chaos
scenario `WhenSalesIsDownTheResultIsHeldForBoundedRetryAndNeverReportedAsConfirmed`.

---

*Rà soát thực hiện bằng đọc mã trực tiếp. Mọi khẳng định đều đã được xác minh trong repo tại
`main@c0e6609`; các suy đoán không kiểm chứng được đã bị loại bỏ. Riêng B1, khẳng định ban đầu về
reachability đã bị chính quá trình sửa bác bỏ và được đính chính tại chỗ thay vì lặng lẽ gỡ xuống.*
