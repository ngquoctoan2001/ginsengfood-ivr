# W-0123 — M3 authoritative call decision cleanup evidence

Ngày: `2026-08-27`

Trạng thái: `TESTS_PASS` local · `BLOCKED_EXTERNAL` cho acceptance/integration thật

Execution baseline: `main@ef09a062597f8f43dad41be751ace03ef5f5973f`

Authority: `OD-18` — **Module 3 quyết định nghiệp vụ; IVR chỉ thực thi cuộc gọi.**

## 1. Phase 0

### 1.1. Candidate và WIP isolation

- Worktree sạch tại baseline trước khi re-index.
- `W-0122` vẫn `IN_PROGRESS`, nhưng phần thay đổi đã được commit tới baseline; W-0123 không sửa
  vendor/TTS/Compose/audio artifact của W-0122.
- `npx gitnexus analyze` tạo diff count-only ở `AGENTS.md` và `CLAUDE.md`; hai file này được theo
  dõi riêng, không phải runtime cleanup.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ nguyên.

### 1.2. GitNexus

Index sau refresh:

- `51,242` nodes;
- `71,624` edges;
- `453` clusters;
- `300` flows.

Upstream impact trước symbol edit:

| Symbol | Risk | Blast radius đáng chú ý |
| --- | --- | --- |
| `EligibilityRules.Evaluate` | `HIGH` | 32 affected, 17 direct; domain/governance/integration/application |
| `IvrOptions` | `HIGH` | 25 affected, 10 direct; Intake/Configuration/Telephony/tests |
| `SchedulerCapacityMapper.RiskScore` | `HIGH` | 5 affected, 2 direct; process `CalculateAsync` |
| `IvrApiServiceCollectionExtensions` | `MEDIUM` | 17 affected, 9 direct |
| `EligibilityService.Map` | `LOW` | 16 affected, 1 direct |
| `EligibilityService.ReadTrustEvidence` | `LOW` | 16 affected, 1 direct |
| `PostgresEligibilityRepository.PersistAsync` | `LOW` | 16 affected, 1 direct |
| `TaskIntakeService` | `LOW` | 14 affected, 4 direct |
| `TaskIntakeEndpoint` | `LOW` | 28 affected, 4 direct documentation consumers |
| `TrustResolverEvidence`, `CanSkip`, `EligibilityEvaluation` | `LOW` theo graph | Graph không map record/property consumer đầy đủ; direct source/test inventory được dùng bổ sung |

Risk tổng giữ `HIGH`. Không sửa `SchedulerCapacityMapper.RiskScore`; `risk_flags` tiếp tục chỉ phục
vụ capacity/scheduler priority.

### 1.3. Data và producer/consumer evidence

| Gate | Trạng thái | Evidence |
| --- | --- | --- |
| Local IVR DB | `ENV_BLOCKED` | `docker ps` chỉ có PostgreSQL của `local-information-platform`; không giả định đó là IVR DB |
| Target/staging/production counts | `ENV_BLOCKED` | Không có endpoint/credential/environment được đặt trong scope |
| M3 field usage | `OWNER_DATA_REQUIRED` | Chưa có commit/OpenAPI/runtime capture phía M3 |
| M3 enum consumption | `OWNER_DATA_REQUIRED` | Chưa có CDC hoặc consumer test từ M3 |

Vì hai gate ngoài repo chưa đóng, implementation chọn nhánh tương thích an toàn:

1. Runtime IVR ngừng đọc trust metadata để quyết định call/skip.
2. Wire field cũ được deprecate/ignore thay vì remove ngay.
3. Persisted enum/cột/status lịch sử tiếp tục đọc được; không migration drop.
4. Runtime mới không emit/write `TASK_SKIPPED_TRUSTED_CUSTOMER`.

## 2. Phase 1 — red/mutation tests

Trạng thái: `PASS`.

| Test | Kỳ vọng mới | Kết quả trên code cũ |
| --- | --- | --- |
| `UT-M3-AUTHORITY-01` | Trust metadata đầy đủ không được đảo M3 call decision | `FAIL` tại `Assert.True(evaluation.Eligible)`; actual `false` |
| `IT-M3-AUTHORITY-05/06` | Valid task phải eligible/queued và không write skip | `FAIL` tại `Assert.True(result.Eligible)`; actual `false` |

Hai lệnh đều compile thành công rồi fail đúng assertion authority. Đây là red evidence; không phải
regression xanh và chưa chứng minh runtime đã sửa.

Mutation evidence sau cutover:

| Mutation tạm thời | Gate bị đỏ |
| --- | --- |
| Thêm lại property `EligibilitySnapshot.Trust` | `UT-M3-AUTHORITY-02` fail tại `Assert.Null(GetProperty("Trust"))` |
| Cho non-empty `risk_flags` đặt `IvrConfirmationRequired=false` | `IT-M3-AUTHORITY-07` fail 2/4 case tại `Eligible=true` |
| Làm yếu `voice.Restricted` gate | `UT-M3-AUTHORITY-03` và `UT-ELIG-DNC-02` cùng fail, actual `Eligible=true` |

Mỗi mutation được revert ngay sau lượt chạy; rebuild sau revert thành công.

## 3. Phase 2 — runtime

Trạng thái: `CODE_DONE`.

Đã gỡ khỏi active runtime:

- `TrustResolverEvidence` và `CanSkip`;
- `EligibilitySnapshot.Trust`;
- `EligibilityEvaluation.TrustedCustomerSkipped`;
- `EligibilityService.ReadTrustEvidence` và dependency options;
- `IvrOptions.ReturningCustomerSkipEnabled` cùng env binding;
- nhánh `EligibilityPersistence.ApplyJobState` ghi `SKIPPED` vì trusted customer;
- trust-skip advisories/reason generation.

Đã giữ nguyên:

- source eligibility và transactional voice/do-not-call fail-closed;
- phone/token/window/capacity gates;
- `SchedulerCapacityMapper.RiskScore(task.RiskFlagsJson)` ở cả eligibility capacity provider và
  scheduler calculation.

Focused verification:

- `dotnet build Ivr.sln --no-restore`: PASS, 0 warning/0 error;
- authority unit: 4/4 PASS;
- authority integration: 5/5 PASS trên PostgreSQL Testcontainers.

## 4. Phase 3 — contract/data compatibility

Trạng thái: `CODE_DONE_EXTERNAL_GATES`.

Implementation:

- OpenAPI `1.0.0-draft.20 → draft.21`;
- `customer_trust_status` và `trusted_skip_allowed`: `deprecated`, `LEGACY_READ`, active runtime
  ignore;
- `risk_flags`: audit/scheduler priority only, không eligibility;
- `TASK_SKIPPED_TRUSTED_CUSTOMER`: giữ trong wire enum cho generated-client compatibility nhưng
  ghi rõ runtime draft.21 không emit;
- linked evidence `trust` object: deprecated/ignored;
- NSwag regenerate, manifest hash và human diff re-pin;
- DB constraint/cột cũ giữ cho history/rollback; không migration/drop;
- `IT-M3-AUTHORITY-08` chèn rồi đọc lại historical skip row qua PostgreSQL constraint hiện hành.

Local gates:

| Gate | Kết quả |
| --- | --- |
| `CT-M3-AUTHORITY-08/09` | 2/2 PASS |
| `IT-M3-AUTHORITY-08` | 1/1 PASS trên PostgreSQL Testcontainers |
| Redocly lint | PASS, 0 warning |
| parser/fixtures/negative | PASS: 2 files, task schemas 9, schema negatives 12, domain negatives 13 |
| manifest drift | `OPENAPI_HASHES_PINNED=3`, human diff current |
| pinned oasdiff `draft.20 → draft.21` | exit `0`, no breaking changes |
| pinned oasdiff cumulative `draft.2 → draft.21 --fail-on WARN` | exit `1`: pre-existing OD-17 removal `sellable_status`; W-0123 không thêm breaking change |

External residuals không được giả xanh:

- target DB counts: `ENV_BLOCKED`;
- M3 field/enum usage: `OWNER_DATA_REQUIRED`;
- cumulative OD-17 oasdiff baseline decision: `PREEXISTING_GATE_FAILURE`.

## 5. Phase 4 — UI/seed/docs

Trạng thái: `CODE_DONE`.

Active sources đã đồng bộ:

- workflow/functional/API/database/glossary ghi rõ M3 quyết định, IVR chỉ thực thi;
- IR-01/05/06 đóng nghĩa vụ trust/risk-evidence cũ và ghi IVR local alignment;
- seed task/customer không còn trust wire field; `SCN-010-m3-authoritative-call` chứng minh task M3
  gửi được thực thi, đồng bộ consumer ở admin seed page;
- UI đổi nhãn enum/status cũ thành “Lịch sử — ... không còn phát sinh”;
- SLO xem new trusted-skip emission là regression, chỉ loại historical row khỏi numerator;
- W-0118, báo cáo 2026-08-26, phase-8 và tech/backlog cũ được gắn
  `HISTORICAL_EVIDENCE`/`SUPERSEDED`; không sửa số đo quá khứ;
- API changelog nâng current tới `draft.21` và ghi rõ cumulative OD-17 warning.

Parse gate: `ivr-tasks.sample.json`, `call-scenarios.sample.json`, `customers.sample.json`,
`enums.vi.json` và eligibility evidence schema đều parse JSON thành công. Scenario ID mới khớp
giữa seed và `SeedMockPage`; seed không còn occurrence trust/skip cũ.

## 6. Phase 5 — verification

Trạng thái: `LOCAL_PASS_EXTERNAL_GATES_OPEN`.

### 6.1. Full local gates

| Gate | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln --no-restore` | PASS, 0 warning/0 error |
| `dotnet test Ivr.sln --no-build --no-restore` | PASS 801/801: unit 496, integration 273, contract 24, chaos 8 |
| NSwag regenerate | PASS; generated code current, không có drift ngoài expected draft.21 docs |
| OpenAPI lint/validate/negative/drift | PASS; 2 files, schemas 9, negatives 12+13, hashes 3, human diff current |
| pinned oasdiff self-test | `CT-DOC-02 PASS`; CRLF chỉ normalize trong container `/tmp`, không sửa script |
| pinned oasdiff cumulative | exit 1, chỉ warning OD-17 `sellable_status` removal — `PREEXISTING_GATE_FAILURE` |
| docs build/self-test | PASS; portal 12 artifact, PII/boundary/link/CI topology xanh |
| traceability | regenerate/review rồi `TEST_TRACEABILITY_CURRENT=484` |
| config self-test | PASS toàn bộ CT-CI/config/cache/image/codegen checks |
| admin UI | typecheck PASS; Vitest 223/223; Next production build PASS |
| JSON seeds/schema | 5/5 parse PASS |
| Markdown map | 581 files, 643 resolved links; global backlog cũ còn 207 unresolved links, không gọi là đã sửa |
| exact classification | 73 files matched old terms, `UNCLASSIFIED_FILES=0`; active runtime branch/seed skip searches = 0 |
| `git diff --check` | PASS; chỉ hiện line-ending conversion warnings, không whitespace error |
| GitNexus detect changes | risk `LOW`; 75 files/151 symbols/0 affected process |

Full test đầu tiên đỏ duy nhất tại `UT-TRACE-01` vì bảng generated chưa chứa test mới; sau
`traceability:write` và review diff, full rerun xanh 801/801. Admin gate đầu tiên cũng phát hiện ba
unused symbol có sẵn và một E2E fixture tự đòi `READY_503` nhưng không tạo state đó. Impact trước
cleanup đều `LOW`/0 process; sau minimal cleanup, ba admin gates xanh. Hai file pnpm bootstrap tự
tạo ngoài baseline đã được loại khỏi worktree.

### 6.2. Residual gates

| Gate | Trạng thái | Lý do |
| --- | --- | --- |
| M3 producer/consumer usage + sign-off | `OWNER_DATA_REQUIRED` | Chưa có commit/OpenAPI/runtime capture hoặc chữ ký M3. Phiếu để M3 trả lời: `plan/ivr-orther/questions-to-module-3-od18-authority.md` (`W-0125`) |
| Target/staging/production DB preflight | `ENV_BLOCKED` | Không có endpoint/credential; local chỉ có DB project khác. Query đã sẵn và được CI chạy: `tools/ops/od18-legacy-skip-preflight.sql` (`W-0125`) |
| Cumulative oasdiff baseline | `PREEXISTING_GATE_FAILURE` | OD-17 đã remove `sellable_status` từ draft.20 |
| Hosted GitLab CI | `NOT_RUN` | Không push/không có pipeline trong authority lượt này |
| Real customer call | `NO` | Không cần cho W-0123 và không được mở bởi work này |

Không suy production readiness từ local tests. Hosted GitLab CI, target DB preflight và real
customer calls không nằm trong evidence hiện tại.

> **Cập nhật `2026-08-27` sau lượt rà soát.** `W-0124` đã đóng dòng
> `Cumulative oasdiff baseline` ở trên: baseline so sánh được xoay `draft.2 → draft.20`, nên
> `PREEXISTING_GATE_FAILURE` không còn. Ba dòng residual còn lại **không** được đóng bởi `W-0124`.
> Con số portal `12 artifact` ở §6.1 cũng thuộc lượt chạy `W-0123`; `W-0124` thêm một báo cáo
> chuyển tiếp đóng băng nên chạy lại hôm nay sẽ ra `13`. Số cũ được giữ nguyên vì nó đúng tại thời
> điểm đo. Xem [`docs/evidence/W-0124/README.md`](../W-0124/README.md).

### 6.3. TODAY-04 target-DB preflight update — 2026-08-29

Trạng thái hiện tại:
**`COMPLETE_AS_BLOCKED — PREFLIGHT_READY / OWNER_DATA_REQUIRED / TARGET_DB_NOT_RUN`**.

- [`od18-legacy-skip-preflight.sql`](../../../tools/ops/od18-legacy-skip-preflight.sql) giờ xuất
  migration inventory, legacy schema/constraint inventory và data counts; SHA-256
  `203c5fd173384cc0c09e51b115ff841fdf40eb91b8cd6510d7a962c84961dd7a`.
- Static check: PowerShell parser `PASS`; SQL có 18/18 câu `SELECT`, 0 non-SELECT.
- `IT-M3-AUTHORITY-13`: `PASS` 1/1 trên migrated PostgreSQL test schema của working tree hiện tại;
  đây không phải immutable release candidate.
- Target preflight vẫn **không chạy**: máy kiểm tra không có `psql`, `Get-Secret`, env/file secret,
  target endpoint, credential hoặc authority/ticket. Container PostgreSQL local không có authority
  xác nhận là target IVR nên không được dùng làm target.
- Owner Module 8 đã xác nhận blocker; xác nhận này không thay target authority/evidence.

Handoff đầy đủ:
[`today-04-target-db-preflight-handoff-2026-08-29.md`](../../../plan/ivr-orther/today-04-target-db-preflight-handoff-2026-08-29.md).

## 7. Thay đổi ngoài phạm vi mang trong cùng diff

Ba thay đổi dưới đây **không** thuộc `OD-18` nhưng nằm trong cùng worktree. Chúng được ghi ở đây để
ranh giới commit là một quyết định được viết ra chứ không phải một tai nạn của thứ tự làm việc
(`W-0124` F4).

| Thay đổi | Vì sao có mặt | Vì sao không revert |
| --- | --- | --- |
| `admin-ui/src/app/(console)/calls/[ivrCallJobId]/page.tsx` — gỡ `DataTable`, `Column`, `flag()` | Admin gate chặn vì unused symbol | Đã chết sẵn tại `HEAD` (kiểm bằng `git show HEAD:…` — chỉ còn dòng import và định nghĩa, không call site). Revert là trả lại một gate đỏ |
| `admin-ui/tests/e2e/back-office-screens.test.ts` — fixture `READY_503` | Fixture tự đòi `READY_503` nhưng không tạo state đó | Revert là trả lại một test khẳng định điều nó không dựng |
| `AGENTS.md`, `CLAUDE.md` — số đếm GitNexus | `npx gitnexus analyze` ghi lại count sau re-index | Count cũ nay sai; giữ lại là để tài liệu nói sai về chính index |

Ranh giới commit — ba commit, **theo thứ tự này**:

| # | Nội dung | Vì sao ở vị trí này |
| --- | --- | --- |
| 1 | `admin-ui` dead code + fixture `READY_503` | Nợ gate có sẵn từ `HEAD`. Phải đi **trước**: nếu commit công việc đứng một mình mà chưa có nó thì `ui_qa` lint vẫn đỏ vì ba unused symbol, và commit đó sẽ không bao giờ xanh khi bisect tới |
| 2 | `AGENTS.md`, `CLAUDE.md` — số đếm GitNexus | Độc lập với cả hai; tách ra để một commit chỉ đổi con số không lẫn vào diff có nghĩa |
| 3 | `W-0123` + `W-0124` | Công việc thật |

`W-0123` và `W-0124` **đi chung một commit**, không tách thêm. Không phải vì tiện: `W-0124` sửa
đúng những file `W-0123` vừa viết lại — `TaskIntakeService.cs`, `EligibilityRulesTests.cs`,
`docs/slo.md`, `06-error-codes.md`, tracker, portal sinh tự động và bảng traceability, khoảng 12
file lồng nhau ở mức hunk. Tách chúng cần phẫu thuật từng hunk và sẽ đẻ ra một commit trung gian
**chưa từng được build hay test** — đánh đổi một lịch sử đẹp lấy một commit không ai biết có xanh
không. Hai Work ID vẫn tách bạch ở nơi chúng thật sự cần tách: tracker, plan và evidence riêng.

```bash
git add "admin-ui/src/app/(console)/calls" admin-ui/tests/e2e/back-office-screens.test.ts
```

```bash
git add AGENTS.md CLAUDE.md
```

```bash
git add -A
```

Chỉ trạng thái **cây cuối cùng** được xác minh xanh (`804/804` + admin `223/223` + toàn bộ gate).
Hai commit đầu không được chạy gate riêng lẻ — nói rõ ở đây thay vì để người đọc suy ra là chúng đã
được kiểm.
