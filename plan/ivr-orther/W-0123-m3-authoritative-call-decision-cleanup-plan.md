# W-0123 — M3 quyết định gọi, IVR chỉ thực thi: cleanup trusted-skip phía IVR

Ngày lập: `2026-08-27`

Baseline phân tích: `main@f291f449d540`

Baseline triển khai: `main@ef09a062597f8f43dad41be751ace03ef5f5973f`

Trạng thái: `TESTS_PASS` cho local implementation · `BLOCKED_EXTERNAL` cho acceptance/integration thật

Origin: `UNPLANNED` — owner làm rõ ranh giới tích hợp ngày `2026-08-27`

Quyết định owner: **Module 3 quyết định nghiệp vụ; IVR chỉ thực thi cuộc gọi.**

Prereq: `W-0118 TESTS_PASS`; IR-06 đã được viết lại theo authority boundary mới nhưng chưa đồng
bộ với code/spec còn lại.

> Đây là plan cleanup, chưa cấp quyền sửa logic production trong lượt lập kế hoạch. Blast radius
> hiện được xếp **HIGH** theo source inventory: contract + domain + persistence + generated code +
> UI + seed + tài liệu. GitNexus MCP không được expose trong phiên lập plan; Phase 0 bắt buộc chạy
> lại upstream impact cho từng symbol trước edit theo `AGENTS.md`.

---

## 0. Tiến độ triển khai

| Phase | Trạng thái | Cập nhật 2026-08-27 |
| --- | --- | --- |
| Phase 0 — authority/baseline/impact/data | `PARTIAL_PASS` | OD-18 đã khóa; baseline sạch `ef09a06`; GitNexus re-index 51.242 nodes/71.624 edges/300 flows; impact `HIGH` cho `EligibilityRules.Evaluate`, `IvrOptions`, `SchedulerCapacityMapper.RiskScore`; local không có IVR DB nên data preflight `ENV_BLOCKED`; M3 field/enum usage `OWNER_DATA_REQUIRED` |
| Phase 1 — red/mutation tests | `PASS` | Red trước cutover đúng assertion; 3 mutation đều bị bắt: reintroduce Trust, risk flags đổi eligibility, weaken call restriction |
| Phase 2 — runtime cutover | `CODE_DONE` | Domain không còn trust predicate/result; service không đọc trust; config flag và persistence skip branch đã gỡ; build 0 warning; focused 4 unit + 5 integration xanh |
| Phase 3 — contract/data compatibility | `CODE_DONE_EXTERNAL_GATES` | draft.21 deprecate/ignore non-breaking từ draft.20; codegen/hash/lint/validate/drift/CT/history-read xanh; target DB/M3 usage còn external; cumulative oasdiff draft.2 vẫn đỏ do pre-existing OD-17 `sellable_status` removal |
| Phase 4 — UI/seed/docs | `CODE_DONE` | Active spec/IR-06/seed/UI/SLO đồng bộ OD-18; evidence/report/phase-8 cũ gắn `HISTORICAL`/`SUPERSEDED`; JSON parse xanh |
| Phase 5 — verification/rollout | `LOCAL_PASS_EXTERNAL_GATES_OPEN` | .NET 801/801; admin UI 223/223 + typecheck/build; OpenAPI/docs/traceability/config/map/exact-search/diff-check xanh; GitNexus LOW/0 flow. Target DB, M3 evidence/sign-off, hosted CI còn mở |

Evidence đang tích lũy tại `docs/evidence/W-0123/README.md`.

---

## 1. Kết quả cần đạt

Sau W-0123:

1. Module 3 là nơi duy nhất quyết định khách/đơn nào cần gọi.
2. Module 3 không gửi task nếu quyết định là `NO_CALL`; Module 3 tự tiếp tục workflow của đơn đó.
3. Task hợp lệ được M3 gửi sang với `ivr_confirmation_required=true` và
   `eligibility_snapshot.decision=ELIGIBLE` được IVR xem là lệnh `CALL_REQUIRED`.
4. IVR không dùng `customer_trust_status`, `trusted_skip_allowed`,
   `trust.risk_evidence_available` hoặc empty/non-empty `risk_flags` để quyết định call/skip.
5. IVR vẫn giữ các execution/safety gate: auth, schema, idempotency, official order/state,
   program profile, do-not-call, phone/token, window, privacy, approved script/policy và capacity.
6. `risk_flags` có thể tiếp tục phục vụ audit và ưu tiên scheduler, nhưng không được đảo quyết định
   call/skip của Module 3.
7. Không còn write path mới tạo `TASK_SKIPPED_TRUSTED_CUSTOMER`.
8. Dữ liệu lịch sử vẫn đọc được; không xoá migration cũ, baseline cũ hoặc evidence lịch sử.
9. OpenAPI, generated DTO, DB model hiện hành, admin UI, seed, SRS, integration docs và tracker
   cùng mô tả một authority boundary.

`CALL_REQUIRED`/`NO_CALL` trong plan là tên logic phía Module 3, không phải field wire mới.

---

## 2. Hiện trạng đã xác minh

### 2.1. Runtime đang có IVR-side business decision

| Thành phần | Hành vi hiện tại | Hệ quả |
| --- | --- | --- |
| `EligibilityRules.TrustResolverEvidence.CanSkip` | Tự tính skip từ feature flag + veto + risk evidence + empty `risk_flags` | IVR đang sở hữu một business predicate |
| `EligibilityRules.Evaluate` | Trả `TASK_SKIPPED_TRUSTED_CUSTOMER` trước nhánh eligible | Task M3 gửi có thể bị IVR business-skip |
| `EligibilityEvaluation.TrustedCustomerSkipped` | Mang state riêng cho nhánh skip | Domain/result model đã bị couple vào quyết định cũ |
| `EligibilityService.ReadTrustEvidence` | Parse `eligibility_snapshot.trust` và metadata customer/risk | IVR tự diễn giải evidence của M3 thành call/skip |
| `IvrOptions.ReturningCustomerSkipEnabled` | Mặc định `true` | Nhánh cũ đang armed theo config mặc định |
| `ServiceCollectionExtensions` | Đọc `IVR_RETURNING_CUSTOMER_SKIP_ENABLED`; chỉ `NO` mới tắt | Deployment config đang mang owner decision sai chỗ |
| `EligibilityRepository` | Decision skip đóng job bằng `status/queue_status=SKIPPED` | Business skip đã đi vào persistence lifecycle |

### 2.2. Contract và dữ liệu đã mang vocabulary cũ

| Lớp | Hiện trạng |
| --- | --- |
| OpenAPI `draft.20` | Công bố `customer_trust_status`, `trusted_skip_allowed`, `risk_flags` với semantics `OD-15`; response enum có `TASK_SKIPPED_TRUSTED_CUSTOMER` |
| Linked evidence schema | `trust.risk_evidence_available` được mô tả là điều kiện bắt buộc để IVR skip |
| Generated DTO | Có property `Trusted_skip_allowed` và enum `TASK_SKIPPED_TRUSTED_CUSTOMER` |
| Task intake | Allow/persist trust fields và `risk_flags` |
| Database model | Check constraints cho phép `TASK_SKIPPED_TRUSTED_CUSTOMER` và `SKIPPED` |
| Admin UI | Dịch decision/reason/status của trusted skip |
| Seed | Có task/scenario được kỳ vọng không tạo call job vì trusted skip |
| SLO | Loại trusted skip khỏi fail-closed metrics vì coi đó là business policy đúng |

### 2.3. Tài liệu đang split-brain

- `integration-requirements/06-module-3-api-handover.md` đã chuyển sang **M3 quyết định, IVR thực
  thi** và đánh dấu `IMPLEMENTATION_ALIGNMENT_REQUIRED`.
- `integration-requirements/00`, `01`, `05`; `decisions-log`; phiếu hỏi riêng `OD-15`; workflow,
  functional spec, glossary, database spec, API error codes, evidence schema, W-0118 evidence,
  báo cáo 2026-08-26 và tài liệu phase-8 vẫn mô tả IVR tự skip.
- Exact-reference inventory tìm thấy **71 file** ngoài archive/generated portal: `27 src`, `3 tests`,
  `10 specs`, `4 integration-requirements`, `18 docs`, `5 plan`, `3 seed`, `1 admin-ui`.
- Nhiều trong 27 source file là historical migration designer/snapshot. Không được hiểu con số 27
  là 27 file đều phải sửa.

### 2.4. Hành vi thực tế hiện nay

Evidence W-0118 ghi Module 3 chưa gửi `trust.risk_evidence_available`, nên các task hiện tại vẫn
được gọi và mang advisory `TRUST_RISK_EVIDENCE_UNAVAILABLE`. Nghĩa là nhánh skip đã implement và
được test, nhưng chưa có bằng chứng chạy trên dữ liệu thật.

Kết luận:

- IR-06 mới **không tự thay đổi runtime**.
- Code hiện tại tình cờ chưa skip dữ liệu thật không có nghĩa authority boundary đã đúng.
- Chỉ cần M3 bắt đầu gửi risk evidence theo tài liệu cũ, IVR sẽ kích hoạt business skip trái quyết
  định mới.

---

## 3. Đánh giá blast radius

### 3.1. Risk

**Risk: HIGH** — lý do:

- thay đổi domain decision nằm trên pre-dial critical path;
- có persisted enum/status/check constraints và historical rows tiềm năng;
- thay đổi server OpenAPI/generated DTO có thể ảnh hưởng consumer M3;
- `risk_flags` còn được scheduler dùng để tính priority/capacity;
- UI, analytics/SLO, seed và evidence vocabulary đang phụ thuộc decision cũ;
- hơn 15 symbol/artifact cần rà và nhiều execution surfaces phải test.

Không triển khai bằng find-and-replace hoặc xoá mọi chữ `SKIPPED`.

### 3.2. Symbol phải chạy GitNexus impact trước khi sửa

| Symbol | Lý do |
| --- | --- |
| `TrustResolverEvidence` / `CanSkip` | Predicate business cần loại khỏi active domain |
| `EligibilityRules.Evaluate` | Critical pre-dial decision flow |
| `EligibilityEvaluation` | Constructor/result shape dùng ở mọi decision branch |
| `EligibilityService.Map` / `ReadTrustEvidence` | Mapping DB evidence vào domain |
| `IvrOptions` | Xoá/deprecate runtime flag |
| `IvrApiServiceCollectionExtensions` | Configuration binding/default |
| `EligibilityRepository.PersistAsync` | State transition `SKIPPED` |
| `TaskIntakeService` | Wire metadata persistence |
| `TaskIntakeEndpoint` | `additionalProperties:false` allowlist |
| `SchedulerCapacityMapper.RiskScore` | Chứng minh `risk_flags` vẫn chỉ dùng execution priority |

Nếu impact trả `HIGH` hoặc `CRITICAL`, dừng trước edit và báo owner đúng blast radius/process.

### 3.3. Active code scope dự kiến

- `src/Ivr.Domain/Policies/EligibilityRules.cs`
- `src/Ivr.Api/Application/EligibilityService.cs`
- `src/Ivr.Infrastructure/Configuration/IvrOptions.cs`
- `src/Ivr.Infrastructure/Configuration/ServiceCollectionExtensions.cs`
- `src/Ivr.Infrastructure/Repositories/EligibilityRepository.cs`
- `src/Ivr.Api/Intake/TaskIntakeEndpoint.cs`
- `src/Ivr.Infrastructure/Intake/TaskIntakeService.cs`
- `src/Ivr.Infrastructure/Persistence/Entities/IvrPersistenceEntities.cs`
- `src/Ivr.Infrastructure/Persistence/PersistenceModelConfiguration.cs`
- `src/Ivr.Contracts/Generated/IvrServer/V1/IvrServerModels.g.cs` — chỉ regenerate, không sửa tay

Regression surfaces phải đọc/test nhưng không mặc định sửa:

- `src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs`
- `src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs`
- `src/Ivr.Infrastructure/Governance/PersonalDataInventory.cs`
- admin read/analytics/reporting paths đọc eligibility decision/status.

### 3.4. Không sửa lịch sử

Không chỉnh:

- migration cũ và `*.Designer.cs` cũ;
- OpenAPI baseline cũ;
- `docs/evidence/W-0118` như thể W-0118 chưa từng xảy ra;
- tracker activity/decision lịch sử;
- archived questions/source documents.

Lịch sử phải được giữ và gắn nhãn **superseded/legacy**, không rewrite quá khứ.

---

## 4. Chính sách compatibility để “clean” mà không phá rollback

### 4.1. Active behavior

- Xoá active `CanSkip` branch và trust-skip advisories.
- Không phát sinh `TASK_SKIPPED_TRUSTED_CUSTOMER` mới.
- Mọi task M3 đã gửi và pass execution/safety gates đi tiếp tới call scheduling.
- `call_restriction`/voice restriction vẫn block; đây là safety veto, không phải customer-risk
  classification.
- `eligibility_snapshot.decision`, `source_version`, `captured_at`, `source_available` và
  `blockers` vẫn được kiểm để payload không tự mâu thuẫn.

### 4.2. Wire contract

Current contract là `1.0.0-draft.20`. W-0123 dự kiến tạo reviewed draft tiếp theo, không sửa
baseline.

| Field/value | Target |
| --- | --- |
| `trusted_skip_allowed` | Remove khỏi active target nếu M3 xác nhận chưa consume; nếu đã consume thì deprecate + ignore một compatibility window |
| `trust.risk_evidence_available` | Remove khỏi active decision schema; nếu payload cũ còn gửi thì chỉ audit/ignored |
| `customer_trust_status` | Không được tham gia decision; deprecate/remove theo privacy + consumer gate |
| `risk_flags` | Giữ nếu scheduler/audit cần; mô tả rõ không quyết định call/skip |
| `TASK_SKIPPED_TRUSTED_CUSTOMER` | Server không emit; remove khỏi active enum nếu CDC cho phép, nếu chưa thì giữ `deprecated/legacy` một version |

Mọi enum shrink/field removal phải chạy pinned `oasdiff`, consumer contract test và M3 sign-off.
`TARGET_CONTRACT_V1=DRAFT` không cho phép silent breaking change.

### 4.3. Persistence/history

- Trước migration chạy data preflight đếm historical skip rows.
- Không drop cột/check value trong lượt cutover đầu nếu còn rolling-deploy/rollback reader.
- Application ngừng đọc `TrustedSkipAllowed` cho decision ngay Phase 2.
- DB có thể tiếp tục cho phép legacy value để đọc historical rows; application và test khóa **no
  new writes**.
- `SKIPPED` là generic lifecycle value và còn retention test sử dụng; không xoá nếu chưa chứng minh
  nó chỉ thuộc trusted skip.
- Physical column/value removal, nếu cần, chỉ sau retention + rollback window và có thể tách Work ID
  riêng.

### 4.4. UI/history

- Active UI không trình bày trusted skip như hành vi hiện hành.
- Nếu historical rows còn tồn tại, giữ translation dạng `Lịch sử — bỏ qua theo chính sách cũ`.
- Không để unknown raw enum làm hỏng report/export cũ.

---

## 5. Kế hoạch triển khai

### Phase 0 — Freeze authority, candidate và impact

| # | Việc | Gate |
| --- | --- | --- |
| 0.1 | Ghi owner decision mới vào decisions log, supersede **placement** của `OD-15`; không phủ nhận business outcome | Decision có owner/date/scope |
| 0.2 | Khóa immutable baseline/candidate; bảo vệ WIP W-0122 và mọi file dirty ngoài W-0123 | Hash + `git status` snapshot |
| 0.3 | Chạy GitNexus upstream impact cho bảng symbol §3.2 | Không symbol nào bị edit trước impact |
| 0.4 | Chạy `gitnexus_detect_changes(scope=unstaged)` để tách WIP hiện hữu khỏi candidate W-0123 | Affected flows được ghi riêng |
| 0.5 | Inventory exact references gồm source/test/spec/UI/seed/docs/generated portal | Không broad delete |
| 0.6 | Data preflight ở DB mục tiêu | Counts được lưu evidence; không suy từ fixture |
| 0.7 | M3 xác nhận producer chưa/đang gửi 3 trust fields và đang consume decision enum nào | `OWNER_DATA_REQUIRED` đóng |

Không bắt đầu Phase 1 trong shared dirty checkout nếu W-0122 vẫn thay đổi cùng file/config. Phải chờ
handoff hoặc freeze một candidate tách biệt.

### Phase 1 — Red tests cho authority boundary mới

Viết test đỏ trước khi sửa runtime:

| Test ID đề xuất | Chứng minh |
| --- | --- |
| `UT-M3-AUTHORITY-01` | `risk_flags=[]/non-empty`, trust evidence present/absent cho cùng task không làm đổi call/skip |
| `UT-M3-AUTHORITY-02` | Không decision nào từ trust metadata tạo `TASK_SKIPPED_TRUSTED_CUSTOMER` |
| `UT-M3-AUTHORITY-03` | `call_restriction=true` vẫn block trước dial |
| `UT-M3-AUTHORITY-04` | Missing/blocked/stale source eligibility vẫn fail-closed kỹ thuật |
| `IT-M3-AUTHORITY-05` | Valid M3 task đi từ `HELD_ELIGIBILITY` tới eligible/queued bất kể trust metadata |
| `IT-M3-AUTHORITY-06` | Không có new DB row/job mang skip decision/status sau evaluation |
| `IT-M3-AUTHORITY-07` | Risk flags vẫn ảnh hưởng scheduler priority theo contract hiện có, nhưng không eligibility decision |
| `CT-M3-AUTHORITY-08` | OpenAPI không công bố IVR-side trusted-skip như active behavior |
| `CT-M3-AUTHORITY-09` | Payload/response compatibility branch đúng sign-off M3 ở Phase 0 |

Mutation evidence bắt buộc:

1. Reinsert `if (trust.CanSkip)` → test authority phải đỏ.
2. Cho `risk_flags` empty/non-empty đổi eligibility → test phải đỏ.
3. Bỏ `call_restriction` gate → governance test phải đỏ.

Existing tests `UT-ELIG-TRUST-03/16/17/18/19`, `IT-ELIG-TRUST-14/15` phải được thay thế hoặc
retire có traceability; không xoá im lặng để lấy xanh.

### Phase 2 — Runtime cutover

| # | Việc | Gate |
| --- | --- | --- |
| 2.1 | Bỏ `ReturningCustomerSkipEnabled` và env binding khỏi active runtime | Config absence test + no hidden default |
| 2.2 | Bỏ `TrustResolverEvidence.CanSkip` khỏi decision graph | Authority tests xanh |
| 2.3 | Bỏ `Trust` khỏi active `EligibilitySnapshot` nếu impact cho phép | Constructor/caller compile sạch |
| 2.4 | Bỏ `TrustedCustomerSkipped` khỏi active evaluation model | Repository không còn skip branch |
| 2.5 | Bỏ trust-skip advisories/reason generation | Metrics vocabulary không còn active emit |
| 2.6 | `EligibilityRepository` không còn transition sang `SKIPPED` vì trusted customer | No-new-write integration test |
| 2.7 | Giữ voice restriction/contact/window/privacy/capacity gates nguyên thứ tự | Regression + mutation pass |
| 2.8 | Giữ `risk_flags` scheduling path hoặc thay thế bằng explicit priority metadata nếu owner mở rộng | Không vô tình đổi capacity ordering |

Không đổi callback ownership: IVR vẫn trả signal, M3 vẫn revalidate và sở hữu state transition.

### Phase 3 — Contract, generated code và data compatibility

| # | Việc | Gate |
| --- | --- | --- |
| 3.1 | Bump reviewed OpenAPI draft, cập nhật descriptions/deprecation/removal theo M3 sign-off | Redocly + parser pass |
| 3.2 | Cập nhật linked eligibility evidence schema | Không còn claim IVR tự phân loại khách |
| 3.3 | Regenerate `IvrServerModels.g.cs`; không sửa tay | Codegen drift clean |
| 3.4 | Update manifest/hash/changelog/docs portal | Pinned hashes và portal selftest pass |
| 3.5 | Chạy pinned `oasdiff` và ghi rõ breaking/non-breaking | Không che WARN |
| 3.6 | Update intake allowlist/persistence cho field đã remove/deprecate | Compat branch có test |
| 3.7 | Data preflight + forward migration nếu thật sự cần | Rolling-deploy/rollback tests pass |
| 3.8 | Giữ historical DB enum/read path nếu còn row hoặc rollback cần | Admin/report đọc được history |

### Phase 4 — UI, seed và documentation cleanup

Active sources cần đồng bộ tối thiểu:

- `specs/workflows/07-trusted-skip.md`: đổi thành tombstone/superseded hoặc thay bằng workflow M3
  authoritative decision; không để reader hiểu IVR còn skip.
- `specs/functional/02-eligibility-and-blockers.md`: thay `FR-IVR-ELIG-007` bằng rule không
  re-decide business.
- `specs/api/06-error-codes.md`: trust advisories thành legacy/non-emitted hoặc remove active.
- `specs/api/evidence/eligibility-snapshot.v1.schema.json`: trust metadata không quyết định skip.
- `specs/database/02-tables.md`, `specs/04-glossary.md`, workflow indexes.
- `integration-requirements/00`, `01`, `05`; giữ IR-06 làm authority handover.
- `plan/ivr-orther/decisions-log.md`: thêm quyết định mới, mark `OD-15` placement superseded.
- `questions-to-module-3-od15-risk-evidence.md`: close/superseded; M3 không còn nghĩa vụ field đó.
- `seed/ivr-tasks.sample.json`, `seed/call-scenarios.sample.json`, `seed/customers.sample.json`:
  không còn fixture kỳ vọng IVR trusted-skip.
- `admin-ui/src/i18n/enums.vi.json`: active vs legacy wording.
- `docs/slo.md`: loại special-case trust-skip khỏi active SLO.
- báo cáo 2026-08-26/W-0118/evidence cũ: giữ lịch sử, thêm superseded note/backlink; không sửa số đo cũ.
- docs portal/generated inventory: regenerate từ OpenAPI, không sửa tay.

Exact-search exit gate:

- Mọi occurrence còn lại của `TASK_SKIPPED_TRUSTED_CUSTOMER`, `trusted_skip_allowed`,
  `risk_evidence_available`, `OD-15` phải thuộc một trong ba nhãn: `LEGACY_READ`,
  `HISTORICAL_EVIDENCE`, `SUPERSEDED`.
- Không occurrence active nào mô tả IVR tự quyết định khách cũ/khách mới.

### Phase 5 — Verification và rollout

Chạy tuần tự khi DLL lock có thể xảy ra:

1. Focused unit/domain/governance tests.
2. Focused integration/persistence tests.
3. `dotnet build Ivr.sln`.
4. `dotnet test Ivr.sln`.
5. `pwsh -File deploy/ci/scripts/regenerate-openapi.ps1` và xác nhận generated diff đúng phạm vi.
6. `npm --prefix deploy/ci run openapi:lint`.
7. `npm --prefix deploy/ci run openapi:validate`.
8. `npm --prefix deploy/ci run test:openapi-negative`.
9. `npm --prefix deploy/ci run openapi:drift` hoặc reviewed-draft flow có owner authority.
10. Pinned `oasdiff breaking` + changelog selftest.
11. `npm --prefix deploy/ci run docs:build` và `test:docs`.
12. `npm --prefix deploy/ci run traceability:write`, review diff, rồi `test:traceability`.
13. `npm --prefix deploy/ci run test:config`.
14. `npm --prefix admin-ui run typecheck`, `test`, `build` nếu UI vocabulary thay đổi.
    Dùng `npm` chứ không `pnpm`: `deploy/ci/ui-qa.gitlab-ci.yml` cài bằng `npm ci` và lockfile
    được commit là `package-lock.json`. Gọi `pnpm` sẽ bootstrap lockfile/workspace riêng và
    lệch khỏi chính gate đang cần tái lập (`W-0124` F6).
15. Data/rolling-deploy compatibility tests.
16. `gitnexus_detect_changes()` trước commit.
17. Hosted GitLab CI; local green không thay thế runner evidence.

Rollout posture:

- Không bật real customer call để nghiệm thu W-0123.
- Deploy canary phải quan sát zero new trusted-skip decision, technical holds không tăng bất thường,
  task accepted → scheduled/callback flow không tụt.
- Rollback bằng previous image/config phải đọc được dữ liệu mới và cũ.

---

## 6. Data preflight và observability

SQL/probe cần chạy trên từng môi trường trước migration:

```sql
SELECT count(*)
FROM ivr_confirmation_tasks
WHERE eligibility_decision = 'TASK_SKIPPED_TRUSTED_CUSTOMER';

SELECT count(*)
FROM ivr_call_jobs
WHERE eligibility_decision = 'TASK_SKIPPED_TRUSTED_CUSTOMER'
   OR status = 'SKIPPED'
   OR queue_status = 'SKIPPED';

SELECT count(*)
FROM ivr_confirmation_tasks
WHERE trusted_skip_allowed IS NOT NULL;
```

Không chạy delete/update dữ liệu từ plan. Nếu count khác zero:

- giữ read compatibility;
- xác định row là fixture/lab/real;
- không rewrite evidence lịch sử;
- tách migration/archive decision có owner approval.

Observability sau cutover:

| Tín hiệu | Kỳ vọng |
| --- | --- |
| New `TASK_SKIPPED_TRUSTED_CUSTOMER` | `0` |
| New `TRUST_*` advisory | `0` sau vocabulary cutover |
| Accepted task chuyển sang eligible/queued | Không giảm do trust metadata |
| `call_restriction`/voice restriction block | Không giảm |
| Privacy/contact/window/capacity holds | Không regression ngoài baseline |
| Scheduler ordering theo `risk_flags` | Giữ nguyên nếu Phase 2.8 không mở rộng |
| Callback delivery | Không đổi taxonomy/ownership |

---

## 7. Acceptance matrix

| Gate | Bằng chứng bắt buộc | Trạng thái lúc lập plan |
| --- | --- | --- |
| Owner authority | Decision mới ghi rõ M3 decides, IVR executes | `OWNER_CONFIRMED`, chưa sync decisions log |
| GitNexus impact | Upstream impact từng symbol + detect changes | `NOT_RUN` — tool unavailable trong lượt plan |
| WIP isolation | Immutable candidate không trộn W-0122/IR-06 WIP ngoài scope | `NOT_RUN` |
| M3 producer/consumer data | Field/enum usage thực tế được M3 xác nhận | `OWNER_DATA_REQUIRED` |
| Red/mutation tests | Reinsert skip làm test đỏ; safety veto mutation đỏ | `NOT_RUN` |
| Runtime cleanup | Zero active business-skip branch/config | `NOT_RUN` |
| No-new-write | Zero new skip decision/status rows | `NOT_RUN` |
| Historical read | Old rows render/audit được hoặc count=0 có evidence | `NOT_RUN` |
| Contract | Reviewed draft + lint/validate/codegen/hash/oasdiff/CDC | `NOT_RUN` |
| Persistence | Data preflight + rolling deploy/rollback | `NOT_RUN` |
| Full .NET | Build/test full solution | `NOT_RUN` |
| Admin UI | typecheck/test/build nếu vocabulary đổi | `NOT_RUN` |
| Docs | Exact-search classification + docs/traceability/config gates | `NOT_RUN` |
| Hosted CI | GitLab pipeline green | `NOT_RUN` |
| Real customer call | Không cần và không được mở bởi W-0123 | `NO` |

Không nâng `PLANNED → CODE_DONE/TESTS_PASS/ACCEPTED` từ selected tests hoặc doc-only diff.

---

## 8. Rủi ro và biện pháp

| ID | Rủi ro | Mức | Biện pháp |
| --- | --- | --- | --- |
| R1 | Xoá skip nhưng vô tình xoá luôn do-not-call | Critical | Governance + mutation test riêng cho `call_restriction`/voice restriction |
| R2 | Xoá `risk_flags` làm đổi scheduler priority/capacity | High | Giữ field hoặc mở decision riêng; regression ordering |
| R3 | Enum/field removal làm M3 client vỡ | High | M3 field usage proof + CDC + oasdiff + compatibility window |
| R4 | Drop DB value/cột làm rollback code cũ fail | High | No-drop cutover đầu; rolling deploy tests; data preflight |
| R5 | Rewrite migrations/evidence làm mất audit history | High | Immutable-history rule |
| R6 | Admin/report không đọc được historical `SKIPPED` | Medium | Legacy translation/read test |
| R7 | Shared WIP W-0122 bị stage/commit cùng | High | Candidate isolation + selective staging + detect changes |
| R8 | IR-06 mới được hiểu là implementation đã xong | High | Giữ `IMPLEMENTATION_ALIGNMENT_REQUIRED` tới khi gates đóng |
| R9 | Full regression xanh local nhưng hosted CI không chạy | Medium | Giữ `NOT_RUN` tới pipeline thật |

---

## 9. Ranh giới bắt buộc

- Không sửa code trong lượt chỉ duyệt plan.
- Không chạy `git add .`, không stage/commit WIP ngoài W-0123.
- Không sửa historical migration/designer/baseline/evidence để làm exact search “đẹp”.
- Không xoá `risk_flags` khi scheduler còn consume.
- Không xoá `call_restriction`/voice restriction hoặc làm yếu fail-closed safety.
- Không đổi callback semantics/state ownership của Module 3.
- Không gọi task `ACCEPTED` là order confirmed.
- Không drop DB column/enum dựa trên fixture; cần target-environment count.
- Không gọi contract change non-breaking khi chưa chạy pinned `oasdiff`.
- Không gọi W-0123 production-ready từ MOCK/lab/local-only evidence.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` không đổi.

---

## 10. Open decisions trước implementation

| ID | Câu hỏi | Owner | Điều kiện đóng |
| --- | --- | --- | --- |
| `OD18-C1` | M3 hiện có gửi/consume `customer_trust_status`, `trusted_skip_allowed`, `risk_evidence_available` hoặc skip decision không? | Module 3 | Trả lời gắn version/commit/OpenAPI |
| `OD18-C2` | `customer_trust_status` còn cần audit hay xoá khỏi active payload theo data minimization? | M3 + Privacy + M8 | Use-case và retention được ký |
| `OD18-C3` | Giữ `risk_flags` làm scheduler priority hay thay bằng explicit priority field? | M3 + M8 + Ops | Không để IVR suy business call/skip |
| `OD18-C4` | Remove enum ngay trong draft tiếp theo hay deprecate một compatibility window? | M3 + M8 contract owners | CDC/oasdiff + rollout plan |
| `OD18-C5` | Historical skip rows có tồn tại ngoài fixture/lab không? | DB owner | Query evidence từng môi trường |

---

## 11. Thứ tự thực hiện sau khi owner duyệt plan

1. Đóng Phase 0: tracker/decision/baseline/WIP isolation/GitNexus/M3 data answers.
2. Viết red + mutation tests Phase 1.
3. Thực hiện runtime cutover Phase 2, không động contract ngoài test cần thiết.
4. Chốt compatibility branch rồi sửa OpenAPI/generated/persistence Phase 3.
5. Đồng bộ UI/seed/tài liệu Phase 4.
6. Chạy toàn bộ gate Phase 5, data preflight và hosted CI.
7. Chỉ sau evidence review mới đề nghị `TESTS_PASS`; owner quyết `ACCEPTED`.

Điểm dừng ngay lập tức:

- GitNexus trả `HIGH/CRITICAL` ngoài phạm vi đã báo;
- M3 đang consume field/enum dự kiến remove;
- DB có historical rows không có read/rollback plan;
- W-0122 hoặc WIP khác đang edit cùng symbol/file;
- safety gate do-not-call/privacy/contact/capacity bị suy yếu;
- oasdiff báo breaking change chưa được owner ký.
