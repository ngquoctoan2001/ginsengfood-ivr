# W-0020 / P2-3 — Policy Registry, Deadline Scheduler and Channel Leases

Status: `TESTS_PASS`. Evidence này chứng minh implementation local, planner `MOCK` và transaction trên PostgreSQL disposable. Nó không phải bằng chứng 1 SIM vật lý, 32 eSIM, gateway/telco, khách thật hoặc production.

## Phạm vi đã hoàn thành

- `DeadlineScheduler` xếp hàng deterministic theo đúng khóa: deadline → program priority → due offset → risk → creation → stable job/attempt tie-breaker.
- Schedule được sinh từ snapshot policy của từng job; candidate `mock-lab-v1` vẫn là fixture `MOCK/LAB`, trong khi policy 3-attempt thay thế chạy được mà không đổi schema/code constant.
- `PostgresAttemptPolicyRegistryWriter` chỉ tạo version mới, từ chối overwrite, kiểm execution-mode approval và ghi append-only audit chứa snapshot hash.
- Candidate/unapproved policy không thể được đăng ký hoặc resolve cho `PRODUCTION_REAL`.
- `SchedulerEligibilityCapacityProvider` thay hoàn toàn `MockEligibilityCapacityProvider`/`FailClosedEligibilityCapacityProvider`; cả MOCK và non-MOCK đều dùng phép tính scheduler theo queue, deadline, duration và pool channel.
- `MockChannelCount` là config: cùng thuật toán đã mô phỏng 1 channel và 32 channel. Không có literal 32 trong scheduler runtime.
- PostgreSQL claim khóa job và SIM bằng `FOR UPDATE SKIP LOCKED`, tạo dispatch lease + attempt trong một transaction; hai worker không thể cùng claim một job/channel.
- Scheduler chỉ claim attempt khi configured offset đã tới và deadline chưa qua; final result hoặc active attempt chặn claim tiếp theo.
- Attempt mới claim vẫn `is_counted_customer_attempt=false`; chỉ P2-4/P2-5 được đánh dấu counted sau khi có call disposition hợp lệ. Technical retry dùng counter/bound riêng.
- Lease hết TTL không được tái sử dụng mù. Recovery chuyển channel sang `QUARANTINED`, tăng fencing generation, giữ job/attempt ở admin-reconciliation state và ghi audit.
- Deadline miss không im lặng: tạo `ivr_capacity_incidents`, final `IVR_CAPACITY_EXCEPTION`, đóng job và giữ `is_counted_customer_attempt=false`.
- Worker đã có `SchedulerJobHost` và runtime loop. `Ivr:Scheduler:Enabled=false` mặc định; default `UnavailableSchedulerDispatchGateway.IsReady=false` nên chưa thể claim/dial cho tới P2-4 đăng ký mock SIM adapter an toàn.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` không đổi. P2-3 không thêm dial-token resolver, `ISimGateway` implementation hay external egress.

## Cấu hình

```json
{
  "Ivr": {
    "Scheduler": {
      "Enabled": false,
      "MockChannelCount": 1,
      "ExpectedCallDurationSeconds": 60,
      "LeaseDurationSeconds": 120,
      "RecoveryQuarantineSeconds": 600,
      "TechnicalRetryLimit": 1,
      "ClaimBatchSize": 64,
      "PollIntervalMilliseconds": 1000
    }
  }
}
```

Để mô phỏng target software 32 eSIM, chỉ đổi `Ivr__Scheduler__MockChannelCount=32`; đây không phải physical/vendor capacity evidence.

## Timing assertions

| Case | Assertion |
| --- | --- |
| candidate GH | attempts tại `T0` và `T0+150s`; expiry `T0+300s` |
| alternate approved | attempts tại `T0`, `T0+45s`, `T0+105s`; expiry `T0+180s` |
| clock boundary | `starts_at == due_at` được phép; `starts_at == expiry` bị từ chối |
| one channel | 2 dispatch 60s chạy lần lượt tại `T0`, `T0+60s` trong window 120s |
| 32 channels | 32 job due cùng `T0` được xếp trên 32 channel, không miss |
| overload | 3 job × 60s / 1 channel / 90s window → job cuối miss, không kéo dài window |

## Tests

| Test ID | Bằng chứng | Kết quả |
| --- | --- | --- |
| `UT-SCH-POLICY-01` | candidate và alternate 3-attempt dùng schedule data-driven | `PASS` |
| `UT-SCH-ORDER-02` | đủ 7 deterministic ordering keys | `PASS` |
| `UT-SCH-CLOCK-03` | due boundary pass, expiry boundary fail | `PASS` |
| `UT-SCH-CAPACITY-04` | 1/32-channel simulation | `PASS` |
| `UT-SCH-CAPACITY-05` | overload tạo missed-deadline plan | `PASS` |
| `UT-SCH-RETRY-06` | final stop; technical retry counter riêng | `PASS` |
| `UT-SCH-CONFIG-07` | bounds cho channel/duration/lease/quarantine/retry/batch/poll | `PASS` |
| `UT-SCH-RUNTIME-08` | disabled/no-gateway không claim | `PASS` |
| `UT-SCH-RUNTIME-09` | ready gateway nhận đúng một atomic lease | `PASS` |
| `IT-SCH-CLAIM-01` | duplicate workers → 1 attempt + 1 lease + 1 audit | `PASS` |
| `IT-SCH-RECOVERY-02` | expired lease → quarantine + fence increment + no blind reclaim | `PASS` |
| `IT-SCH-DEADLINE-03` | missed deadline → incident + final non-counted result | `PASS` |
| `IT-SCH-DEADLINE-08` | active attempt đã claim trước expiry không bị gắn nhầm capacity miss | `PASS` |
| `IT-SCH-FINAL-04` | final result chặn attempt 2 dù offset đã tới | `PASS` |
| `IT-POLICY-AUDIT-05` | alternate version resolve PROD và audit; duplicate version reject | `PASS` |
| `IT-POLICY-PROD-06` | candidate production registration fail-closed | `PASS` |
| `IT-SCH-CAPACITY-07` | DI non-MOCK dùng scheduler capacity thật | `PASS` |
| `IT-ELIG-SCHED-09` | scheduler capacity unavailable → admin hold, zero attempt | `PASS` |

Focused result: scheduler unit `9/9`; PostgreSQL scheduler/capacity integration `9/9`.

## Local gates — 2026-08-13

```text
dotnet format Ivr.sln --no-restore --verify-no-changes
PASS

dotnet build Ivr.sln -c Release --no-restore -p:RunAnalyzers=true
PASS — 0 warnings / 0 errors

dotnet test Ivr.sln -c Release --no-build --no-restore --collect:"XPlat Code Coverage"
contract 21 + unit 93 + integration 56 = 170/170 PASS

merged coverage
TOTAL_LINE_COVERAGE=94.68% COVERED=19729 VALID=20838 REPORTS=3

dotnet ef migrations has-pending-model-changes --project src/Ivr.Infrastructure/Ivr.Infrastructure.csproj --no-build --configuration Release
No changes have been made to the model since the last migration.

CI config, OpenAPI lint/parse/schema/hash/drift/negative and API docs
PASS — 2 OpenAPI files; 9 canonical tasks; 12 schema negatives rejected;
13 domain negatives schema-valid; 11 portal artifacts

admin-ui lint + production build; NuGet/npm HIGH; Docker Compose
PASS — 0 vulnerability finding

Gitleaks 8.30.0 working tree + Git history
PASS — 35.16 MB working tree; 31 commits / 21.28 MB; no leaks

PII selftest + docs/evidence scan
PASS — CT-CI-06..06h; 23 text files; 2 binary files skipped

official Markdown map
PASS — 414 files / 375 resolved links / 0 unresolved
```

## Artifacts

- `src/Ivr.Domain/Scheduling/DeadlineScheduler.cs`
- `src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs`
- `src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs`
- `src/Ivr.Infrastructure/Scheduling/SchedulerRuntime.cs`
- `src/Ivr.Infrastructure/Intake/AttemptPolicyRegistryWriter.cs`
- `src/Ivr.Infrastructure/Persistence/Channels/SimChannelLeaseRepository.cs` — **đính chính `2026-09-04` (`W-0171`): file/thư mục này không tồn tại.** Lease/fencing của SIM channel nằm ở `PostgresSchedulerStore.cs` và `PersistenceModelConfiguration.cs`
- `src/Ivr.Api/Application/EligibilityService.cs`
- `src/Ivr.Worker/Jobs/SchedulerJobHost.cs`
- `tests/Ivr.UnitTests/Scheduling/DeadlineSchedulerTests.cs`
- `tests/Ivr.IntegrationTests/SchedulerPersistenceTests.cs`

## Commit và remote handoff

- Implementation commit: `d23ab984627d270c335172261c836aa8af78497a` trên `main`.
- GitHub `main` đã fast-forward tới đúng commit trên và remote ref đã được xác minh.
- GitLab `origin/main` vẫn ở `5544395ecbc62c31e8a3f78857f65d275e97b5a1`: direct push bị pre-receive hook từ chối vì protected branch. Không hạ protection và không tạo branch/MR theo chỉ đạo workflow một nhánh của IVR owner.
- GitNexus staged review: `CRITICAL`, 20 file/188 symbol/35 flow; mức rộng này đến từ thay capacity provider và thêm scheduler xuyên Domain/API/Infrastructure/Worker. Cycle Configuration↔Scheduling mới phát hiện đã được loại bỏ; cycle check cuối chỉ còn cycle baseline `RuntimeGateDefaults`↔`PersistenceModelConfiguration`.

## Residual gates

- `Ivr:Scheduler:Enabled` giữ `false` và dispatch gateway giữ unavailable cho tới P2-4 cung cấp speech/dial-token/mock SIM adapter. Đây là fail-closed activation boundary, không phải real-call permission.
- P2-4 phải consume `SchedulerDispatchLease`, giữ fencing generation trên mọi gateway command và chỉ chuyển `is_counted_customer_attempt=true` sau disposition hợp lệ.
- Candidate attempt policy vẫn chỉ `MOCK/LAB`; final owner-approved production policy (`W-0007`/`OD-V1-08`) còn `BLOCKED_EXTERNAL`.
- 1 SIM lab, destination allowlist, modem/carrier disposition, 32 eSIM procurement/config/failover/capacity và caller-ID đều `NOT_RUN`.
- Production throughput không được suy ra từ mô phỏng software; phải đo vendor/gateway thật.
- Target Sales contract/auth/data, legal/privacy/security và release sign-off còn mở. Không có customer call hoặc order mutation trong W-0020.
- Protected GitLab `main` có thể tiếp tục từ chối direct push theo workflow một nhánh do owner yêu cầu; không tự hạ protection và không tạo branch/MR.
