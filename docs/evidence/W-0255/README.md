# W-0255 — Hai chỗ callback nuốt lỗi sai cách, và một đính chính

Ngày: 2026-09-09 · Baseline: `main@fc4a6d0` · Trạng thái: **TESTS_PASS**.

Xuất phát từ một lượt rà soát toàn repo ([`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md)).
Lượt này sửa hai phát hiện nặng nhất trong đường giao kết quả về Sales. Cả hai đều là lỗi **xử lý
ngoại lệ quá rộng** — bắt đúng chỗ nhưng sai phạm vi — và cả hai đều biến một sự cố tạm thời thành
mất dữ liệu nghiệp vụ vĩnh viễn.

**5 tệp đổi, +407 −58.** Không đổi chữ ký public, không đổi schema, không migration.

## 1. B2 — `InvalidOperationException` bắt trùm cả transport

`CallbackDispatcher.RunBatchAsync` có một `try` bọc chung hai việc khác hẳn nhau: chọn contract, và
gửi qua transport. Cái `catch (InvalidOperationException)` bên dưới rõ ràng được viết cho việc thứ
nhất — `SalesCallbackContractSelector.Select` khai báo hẳn ngoại lệ đó trong `<exception>` cho cặp
provider/program không hỗ trợ.

Nhưng transport cũng ném đúng type ấy: handler đã dispose, EF `"The connection is already open"`,
`JsonSerializer` ở trạng thái hỏng. Mọi sự cố hạ tầng tạm thời đó bị gán nhãn
`CALLBACK_ADAPTER_SELECTION_REJECTED` → outcome `Invalid` → **terminal, không retry, dead-letter**.

Nửa còn lại tệ hơn nửa đầu: `Invalid` không phải `TransientFailure`, nên nhánh `else` gọi
`circuitBreaker.RecordSuccess()`. Một downstream đang hỏng theo kiểu này trông **khoẻ mạnh** trên
breaker, trong khi mọi kết quả nó nợ Sales bị lặng lẽ vứt đi.

**Sửa:** `TryResolveContract` tách riêng, một `try` chỉ bọc đúng lời gọi selector và không gì khác.
Transport có `try` riêng — `OperationCanceledException` giữ nguyên hành vi, còn lại thành
`TransientFailure`. Nhánh `_ =>` trả về giá trị thay vì `throw`, nên một `SalesCallbackContractKind`
thêm về sau không thể tái mở lỗi này.

`ArgumentException` được bắt cùng `InvalidOperationException` trong `TryResolveContract` một cách
có chủ ý: selector ném nó khi `programCode` rỗng, và đó là **cùng một sự thật** — message không
định tuyến được. Để nó thoát ra sẽ bỏ dở cả batch thay vì retire một row, tức là đổi một lỗi lấy
một lỗi khác.

Hành vi tài liệu hoá cho trường hợp thật giữ nguyên: chương trình không adapter nào phục vụ vẫn là
`CALLBACK_ADAPTER_SELECTION_REJECTED` và vẫn terminal
(`docs/evidence/W-0107/vocabulary-review.md:443`).

## 2. B1 — `tasks[row.TaskId]` không guard, sau khi transaction đã commit

`CallbackOutboxRepository.DequeueReadyAsync` claim row, **commit**, rồi mới dựng message bằng
indexer `tasks[row.TaskId]`. Message lấy `ProgramCode` và `CorrelationId` từ task, không từ chính
callback row.

Nếu `task_id` không resolve được thì `KeyNotFoundException` bay ra **sau commit**. N row trong batch
đã mang `SENDING` + lease. `CallbackDeliveryJobHost` bắt, log, chờ 1 giây, chạy lại. Nhánh reclaim
của query (`SENDING AND lease_expires_at < now`) nhặt lại đúng row đó. Ném lại. Vô hạn — và exception
xảy ra ở bước projection nên nó kéo theo **cả batch**, mỗi lượt.

Cùng lỗi ở site thứ hai, phát hiện trong lúc sửa: `CompleteDeliveryAsync` dùng `SingleAsync` trên
`ConfirmationTasks`, **sau khi** `ExecuteUpdateAsync` đã áp dụng. Ném ở đó là rollback bản ghi giao
đã thành công, trả row về `SENDING` cho lượt sau nhặt lên và hỏng lại.

### 2.1 Đính chính: điều này retention KHÔNG gây ra được

Bản đầu của báo cáo rà soát xếp B1 là **CRITICAL** với lập luận rằng retention xoá
`ivr_confirmation_tasks` và bỏ lại callback mồ côi. **Lập luận đó sai**, và chính việc dựng test đã
bác bỏ nó.

`ivr_result_callbacks.task_id` đúng là không có FK. Nhưng bất biến được giữ **bắc cầu** qua hai FK
khác, cả hai `RESTRICT` và cả hai có thật trong schema:

| Ràng buộc | Nguồn |
| --- | --- |
| `FK_ivr_result_callbacks_ivr_call_results_ivr_call_result_id` | `20260812142435_P1_2_InitialTargetV1Persistence.cs:533` |
| `FK_ivr_call_results_ivr_confirmation_tasks_task_id` | `PersistenceModelConfiguration.cs:318-322` |

Còn callback thì không xoá được result; còn result thì không xoá được task.
`RetentionTargetCatalog.cs:59-65` còn khai báo `dependencyBlockedSql` tôn trọng đúng chuỗi ấy. Và
`CallbackOutboxSnapshotFactory.cs:86-90` đặt `TaskId = job.TaskId` cùng `IvrCallResultId` của chính
job đó, nên đường ghi production luôn nhất quán.

Tôi đọc đúng rằng `task_id` không có FK trực tiếp, rồi kết luận sai rằng vì thế nó mồ côi được.
Mức đúng là **MEDIUM**.

### 2.2 Vì sao vẫn sửa

Bất biến giữ cho hai lời gọi kia an toàn là **bắc cầu và không được phát biểu ở đâu cả** — không FK
trên `task_id`, không check constraint, không comment. Một script sửa dữ liệu, một migration làm
`ivr_call_result_id` nullable, hay một thay đổi đặt `TaskId` từ nguồn khác đều tái tạo được
crash-loop. Chi phí sự cố không tương xứng với xác suất tới của nó.

**Sửa:** `DequeueReadyAsync` phân loại **trước khi** claim. Row không resolve được task đi thẳng vào
`INVALID_DEAD_LETTER` + `LastError = CALLBACK_TASK_MISSING`, **trong chính transaction claim**, nên
nó không bao giờ tồn tại ở trạng thái claimed-nhưng-vô-chủ. `INVALID_DEAD_LETTER` là giá trị
check-constraint đã có sẵn cho đúng nghĩa này, nên không cần migration.

Kèm audit row và review item `OPEN`. Đây mới là phần đáng kể: retire lặng lẽ là đổi một hàng đợi
kẹt lấy một kết quả không bao giờ tới Sales mà **không ai được báo**. Review item là thứ duy nhất
operator nhìn thấy.

`CompleteDeliveryAsync` chuyển sang `SingleOrDefaultAsync`, dùng correlation id suy ra từ callback id
khi task đã biến mất. Bản update đã áp dụng được giữ lại thay vì rollback.

## 3. Test

| TestId | Khẳng định |
| --- | --- |
| `IT-DB-OUTBOX-09` | Callback có `task_id` không resolve được → dequeue trả rỗng thay vì ném; row thành terminal, không lease; review item `OPEN`; **lượt poll thứ hai cũng không thấy gì** |
| `UT-CALLBACK-TRANSPORT-INVALIDOP-14` | Transport ném `InvalidOperationException` → `RETRY_PENDING`, breaker ghi nhận transient failure |
| `UT-CALLBACK-ADAPTER-REJECT-15` | Chương trình không định tuyến được → vẫn `INVALID_DEAD_LETTER`, transport không bị chạm |

`IT-DB-OUTBOX-09` dựng trạng thái đó bằng cách cho `IvrCallResultId` trỏ vào một result **hợp lệ**
(thoả FK) trong khi `TaskId` trỏ vào hư không — tức là đi qua đúng cột không có FK. Đó là cách duy
nhất dựng được trạng thái này mà không phá schema, và nó chính là hình dạng mà một script sửa dữ
liệu sẽ tạo ra.

**Hai test hồi quy đã được kiểm chứng là fail trên code chưa sửa**, bằng cách hoàn nguyên tạm thời
từng file rồi chạy lại, trước khi ghi nhận là pass. Một test xanh ở cả hai phía không chứng minh
được gì. `UT-CALLBACK-ADAPTER-REJECT-15` pass ở cả hai phía **có chủ ý** — vai trò của nó là chốt
bảo toàn hành vi, không phải bắt lỗi.

## 4. Kết quả

```
Unit         649/649
Integration  274/274   (Testcontainers PostgreSQL 16)
Contract      24/24
Chaos          8/8
             ─────────
             955/955
```

`dotnet build Ivr.sln` — 0 warning, 0 error, với `TreatWarningsAsErrors=true`.

`docs/traceability-tests.md` sinh lại bằng `node deploy/ci/scripts/generate-test-traceability.mjs`;
`FailGateTests.TheTraceabilityTableMatchesTheSuiteItClaimsToDescribe` bắt buộc điều này với mọi
`TestId` mới.

`gitnexus detect_changes` xếp mức `critical` vì `RunBatchAsync` và `DequeueReadyAsync` nằm trên **16
execution flow**. Cả 16 đều nằm trong các suite trên, gồm chaos scenario
`WhenSalesIsDownTheResultIsHeldForBoundedRetryAndNeverReportedAsConfirmed`, nên "critical" ở đây là
phân loại theo blast radius chứ không phải cảnh báo chưa được kiểm.

Impact analysis chạy trước khi sửa, theo `CLAUDE.md`:

| Symbol | Risk | Direct callers |
| --- | --- | --- |
| `CallbackDispatcher.RunBatchAsync` | HIGH | 8 (1 production host, 6 unit test, 1 chaos) |
| `CallbackOutboxRepository.DequeueReadyAsync` | MEDIUM | 15 impacted / 7 direct |

## 5. Còn lại

Bản rà soát ghi 26 phát hiện; lượt này đóng 2. Phần còn lại còn nguyên trong
[`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md), thứ tự ưu
tiên ở §5. Ba cái đáng làm tiếp:

- **P1** — `AdminReadService.GetDashboardAsync`: 30 round trip tuần tự, `jobIds` nhúng lại làm
  subquery trong 5 query riêng → 5 semi-join full-table khi không filter.
- **B3** — `EligibilityService.GetCapacityFailClosedAsync`: `catch (Exception)` không bind, không
  log, không metric. Một cú trượt `PiiGuard` trông y hệt DB chết.
- **B4** — ba background loop retry mỗi 1000 ms không backoff, không jitter.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
