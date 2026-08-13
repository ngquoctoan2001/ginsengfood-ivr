# IVR Master Implementation Progress Ledger

Trạng thái: `ACTIVE_SINGLE_SOURCE` · Cập nhật: `2026-08-13`
Scope: mọi planned work, unplanned work, dependency, decision, implementation, test, evidence và acceptance của IVR.

> **Không tạo tracker/backlog thứ hai.** File này là sổ tiến độ duy nhất. Không xóa lịch sử; sửa factual error bằng một Activity entry mới. Khi bảng dài, tiếp tục append, không tách file.

## 1. Operating rules

1. Work ID toàn cục dạng `W-0001`, tăng tuần tự. `NEXT_WORK_ID` ở §2 phải được tăng ngay khi cấp ID.
2. Mỗi prompt dùng Work ID đã dành ở §5. Việc phát sinh ngoài plan dùng ID kế tiếp, `Origin=UNPLANNED`, chen vào đúng thứ tự phát sinh chứ không tạo danh sách phụ.
3. Trước khi làm: ghi owner, baseline, dependency, acceptance, `IN_PROGRESS` và Activity `START`.
4. Trong khi làm: append `CHECKPOINT`, `DISCOVERY`, `DECISION_REQUEST`, `BLOCKED` hoặc `SCOPE_CHANGE`.
5. Sau khi làm: ghi files/artifacts, command+result, evidence, residual gates, next action và Activity `FINISH/HANDOFF`.
6. Không chuyển `ACCEPTED` nếu evidence chưa được reviewer/owner chấp nhận. Mock/lab/real evidence không thay thế nhau.
7. External item chỉ đóng khi có artifact thật (OpenAPI, signed decision, credential test, lab report…), không đóng vì IVR đã mock.

Status: `PLANNED`, `NOT_STARTED`, `IN_PROGRESS`, `CODE_DONE`, `TESTS_PASS`, `EVIDENCE_SUBMITTED`, `ACCEPTED`, `BLOCKED_INTERNAL`, `BLOCKED_EXTERNAL`, `DEFERRED_TARGET`, `N/A`, `CANCELLED`.

## 2. Ledger control

| Field | Value |
| --- | --- |
| `NEXT_WORK_ID` | `W-0087` |
| Last allocated | `W-0086` |
| Last activity sequence | `A-0118` |
| Contract state | `TARGET_CONTRACT_V1=DRAFT` |
| Logical repository | standalone `ginsengfood-ivr`; source root is current repository |
| Namespace | `Ivr` |
| CI / evidence root | GitLab CI / `docs/evidence/` |
| Default mode | `MOCK` |
| Real customer calls | `NO` |
| Current lab target | 1 real SIM + destination allowlist |
| Production telephony target | 32 eSIM channels, configurable |

## 3. Current gate snapshot

| Gate ID | Gate | Owner | Status | Mock/parallel path | Closure evidence |
| --- | --- | --- | --- | --- | --- |
| `G-CONTRACT` | Sales task/callback Target V1 | Sales API/Core | BLOCKED_EXTERNAL | fake Sales + WireMock | approved OpenAPI + CDC tests |
| `G-SPEECH` | privacy-safe order summary | Sales/Product/Privacy | BLOCKED_EXTERNAL | fake summaries/renderer | schema/examples/privacy approval |
| `G-DIAL` | dial-token issue/resolve | Sales/Security/Telephony | BLOCKED_EXTERNAL | fake resolver | threat model/API/tests |
| `G-AUTH` | production JWT/mTLS profile | Security/Platform | BLOCKED_EXTERNAL | mock JWT | auth profile + sandbox credential/tests |
| `G-POLICY` | production attempt policy | Product/Core | BLOCKED_EXTERNAL | candidate mock-lab-v1 | signed policy/version |
| `G-LAB-SIM` | one real SIM lab | Infra/vendor | BLOCKED_EXTERNAL | mock SIM | lab report/allowlist/kill-switch evidence |
| `G-ESIM32` | 32 eSIM capacity | Infra/procurement | BLOCKED_EXTERNAL | load simulator | procurement + measured capacity/failover |
| `G-LEGAL` | script/privacy/retention/legal | Legal/Privacy | BLOCKED_EXTERNAL | recording off/redaction | signed review |
| `G-RELEASE` | production release | Release owner | BLOCKED_EXTERNAL | none | accepted go/no-go/evidence |
| `G-GITLAB` | GitLab project/runner/registry/protected-branch (TV1-12) | Platform/Infra | BLOCKED_EXTERNAL | all controls except required independent MR approval are hosted-PASS | upgrade to Premium/Ultimate + add second reviewer + prove one required approval before merge |
| `G-PLATFORM` | secret store, K8s cluster, observability backend, warehouse, progressive-delivery controller | Platform/Infra | BLOCKED_EXTERNAL | docker-compose local | provisioned endpoints + credentials + smoke |

## 4. External request register

| Work ID | Request | Owner | Status | Exact deliverable requested | IVR fallback | Next action |
| --- | --- | --- | --- | --- | --- | --- |
| `W-0002` | Program/task producer | Sales/Product | BLOCKED_EXTERNAL | GH ONLINE + 24/7 COD matrix, callable states, required flag, task OpenAPI/tests | fake producer | send Target V1 pack |
| `W-0003` | Speech payload | Sales/Product/Privacy | BLOCKED_EXTERNAL | schema + samples + item/area rules | fake summary fixtures | request design review |
| `W-0004` | Dial token | Sales/Security/Telephony | BLOCKED_EXTERNAL | issue/resolve/TTL/one-use contract | fake resolver | request threat model/API |
| `W-0005` | Callback/revalidation | Sales API/Core | BLOCKED_EXTERNAL | target endpoint, DTO, ACK, version/idempotency/timeout tests | target WireMock + GH compat | request OpenAPI review |
| `W-0006` | Auth | Security/Platform | BLOCKED_EXTERNAL | JWT issuer/audience/scope/TTL/JWKS and mTLS decision | mock JWT | request auth profile |
| `W-0007` | Attempt policy | Product/Core | BLOCKED_EXTERNAL | approved version/max/window/offsets | candidate MOCK/LAB only | owner sign-off |
| `W-0008` | Telephony lab/32 eSIM | Infra/vendor | BLOCKED_EXTERNAL | 1 SIM lab protocol + future 32 eSIM capacity/caller-ID/cost | mock SIM/load model | procure/test |
| `W-0009` | Legal/release | Legal/Privacy/Release | BLOCKED_EXTERNAL | script, retention, legal basis, pilot/go-live approval | recording off/no customers | start review early |
| `W-0061` | **GitLab platform provisioning** (TV1-12) | Platform/Infra | BLOCKED_EXTERNAL | project/dual remote, runner/DinD, protected `main`, no-direct-push, `Pipelines must succeed`, protected variables, Registry smoke và private Pages đều PASS; GitLab Free hiển thị `Approval is optional`, project chỉ có một Owner | giữ workflow branch + MR; không ghi token/value bí mật vào evidence | nâng Premium/Ultimate, mời reviewer độc lập, require ≥1 approval cho protected branch, rồi chạy MR chứng minh blocked-before/merge-after approval |
| `W-0063` | **Platform infrastructure dependencies** | Platform/Infra | BLOCKED_EXTERNAL | container registry; K8s cluster + credentials 4 env; secret store (Vault/KMS); observability backend (Tempo/Jaeger + Prometheus + Loki hoặc APM); Grafana/Alertmanager; Argo Rollouts/Flagger; analytics warehouse; visual-regression service | docker-compose local stack | gom 8 mục `NEED_CONFIRMATION` trong P5-5/P6-1/P6-2/P7-1/P7-2/P7-4/P7-5/P10-4 |

## 5. Planned implementation register

Every row is planned work. Detailed build/test/evidence requirements live in the linked prompt and specs; actual results must be written back here.

`Origin` mặc định là `PLANNED`. Việc phát sinh ghi `UNPLANNED` hoặc `RED_TEAM_REMEDIATION` trong cột `Scope summary`.

| Work ID | Prompt | Scope summary | Prereq | Status | Owner | Artifacts/MR | Tests/evidence | Residual/next |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `W-0001` | Planning realignment | Target V1 plan/spec/prompt/tracker/OpenAPI | docs/code review | EVIDENCE_SUBMITTED | Codex + IVR owner | Target V1 draft, two OpenAPI files, 51-prompt register, fake seed | JSON/YAML/schema/ref/link/tracker/diff checks pass | technical defaults confirmed; owner may accept evidence separately |
| `W-0010` | `P0-1` | repo/solution bootstrap | technical defaults confirmed 2026-08-12; baseline frozen at `5c6f39e` | ACCEPTED | Codex (explicit IVR owner authorization) | `Ivr.sln`; `src/**`; `tests/**`; `admin-ui/**`; `docker-compose.dev.yml`; `README.md`; `docs/evidence/W-0010/` | .NET build 0 warning/0 error; 3/3 test pass; format 0/39; UI lint/build pass; Postgres healthy; probes 3/3; browser clean + screenshot; doc links 0 unresolved; GitNexus LOW/0 process/0 cycle | P0-1 closed; MOCK only; GitLab CI next at W-0011; real Sales/SIM/lab/production remain NOT_RUN and outside this acceptance |
| `W-0011` | `P0-2` | GitLab CI/quality baseline | W-0010 ACCEPTED | TESTS_PASS | Codex | `.gitlab-ci.yml`; `deploy/ci/**`; MR template; CODEOWNERS; lockfiles; `docs/evidence/W-0011/`; `docs/evidence/W-0061/` | MR pipelines `#2756409438`/`#2756495155` PASS; protected-main pipeline `#2756517379` 12 jobs/98 tests/Pages PASS; Registry job `15872915564` PASS | CI + hosted enforcement complete except required independent approval; giữ TESTS_PASS tới khi Premium/Ultimate + second reviewer proof đóng W-0061 |
| `W-0012` | `P0-3` | config/auth/audit/idempotency/correlation | W-0010 ACCEPTED; P0-2 local gates TESTS_PASS | TESTS_PASS | Codex | `src/Ivr.Api/{Auth,Foundation,Middleware}/`; `src/Ivr.Domain/{Errors,Privacy}/`; `src/Ivr.Infrastructure/{Audit,Correlation,Evidence,Idempotency}/`; `docs/evidence/W-0012/` | build 0/0; 14/14 implemented tests; P0-3 11/11; coverage 91.99%; format/UI/OpenAPI/config/security/PII/Compose + later hosted GitLab quality pipelines PASS; GitNexus staged CRITICAL breadth reviewed, 0 cycle | local MOCK implementation complete; P1-2 owns persistence migrations, P4-4 owns production auth; no Sales/SIM/real call; W-0061 only approval enforcement remains |
| `W-0013` | `P0-4` | mode/provider flags + kill switches | W-0012 TESTS_PASS | TESTS_PASS | Codex | `src/Ivr.Infrastructure/FeatureFlags/**`; API/admin/Worker wiring; EF model; OpenAPI; `docs/evidence/W-0013/` | Release 0/0; 27/27 tests, five-run stability; P0-4 13/13 including all 10 required; coverage 87.50%; format/UI/OpenAPI/config/security/PII/Compose + later hosted pipelines PASS; GitNexus staged CRITICAL breadth reviewed, 0 cycle | local MOCK complete; OD-V1-20 pending/fail-closed; P1-2 owns migration/persistent mutation; W-0061 only approval enforcement remains |
| `W-0014` | `P1-1` | both OpenAPI/codegen/contract scaffold | W-0010..12 TESTS_PASS; baseline `c78a407` | TESTS_PASS | Codex | pinned NSwag/codegen; generated IVR DTOs + Target Sales client; verified current-compat fixture/client; drift/hash gate; fake Sales mappings; `docs/contracts/**`; `docs/evidence/W-0014/` | Release 0/0; 55/55 tests; coverage 75.57%; regeneration stable; OpenAPI/config/security/privacy + later hosted pipelines PASS; GitNexus staged MEDIUM 42 file/452 symbol/4 generated-client flow/0 cycle | Contract remains TARGET_DRAFT; current compat runtime-disabled; W-0002/W-0005/W-0006 external; W-0061 only approval enforcement remains |
| `W-0015` | `P1-2` | PostgreSQL/EF migrations, versioned policy/speech snapshots | W-0012 TESTS_PASS; baseline `5d2301e` | TESTS_PASS | Codex | EF 17-table model/migration; Up/Down SQL; Testcontainers; persistent P0-4 flag/audit/idempotency transaction; outbox + channel lease/fencing; `docs/evidence/W-0015/` | clean/recreate/rollback + 6/6 PostgreSQL tests; full 61/61; later hosted DinD/Testcontainers and quality pipelines PASS | local P0-4 persistence gap closed; DF-07/KMS/backup-staging-prod remain; W-0061 only approval enforcement remains; no real Sales/SIM/call |
| `W-0016` | `P1-3` | domain/DTO/provider ports/privacy guards | W-0014,W-0015 | TESTS_PASS | Codex | immutable domain/value objects; provider ports + deterministic fakes; target/current anti-corruption mappers; privacy guards; tests; `docs/evidence/W-0016/README.md` | locked restore/build/format PASS; 93/93 full tests; later hosted pipelines 98/98; GitNexus staged HIGH 292 symbol/22 file/12 expected flow | local MOCK complete; Target V1/policy approvals and real Sales/SIM/customer calls remain external/NOT_RUN; W-0061 only GitLab approval enforcement remains |
| `W-0017` | `P1-4` | API docs/versioning/drift portal | W-0014 | ACCEPTED | Codex (explicit IVR owner authorization) | static Redoc portal; versioning/integration/changelog guides; pinned oasdiff; private GitLab Pages; `docs/evidence/W-0017/` | local CT-DOC-01/02 + UT-DOC-PII-03; pipeline `#2756517379` 12 jobs/98 tests; Pages job `15873355825` + deploy PASS; anonymous access redirects to auth | non-production docs scope closed; Target V1 remains DRAFT; no Sales/SIM/customer call; W-0061 approval gate remains independent |
| `W-0018` | `P2-1` | task intake for both program/payment paths | W-0014..16,W-0024 | TESTS_PASS | Codex | `docs/evidence/W-0018/README.md`; 144/144; PostgreSQL concurrent atomicity `1/1/1/1/1`; coverage 95.26% | local MOCK + disposable PostgreSQL; full build/OpenAPI/docs/UI/security/privacy gates PASS | owner/reviewer acceptance, real Sales/auth, LAB/PROD script/key/SIM and P2-2 eligibility remain open |
| `W-0019` | `P2-2` | eligibility/blockers/fail-closed | W-0018 | TESTS_PASS | Codex | rules/service/repository; atomic task/job/outbox/capacity/audit/evidence; `docs/evidence/W-0019/` | 4/4 required unit + capacity/MOCK/DNC/fail-closed integration 4/4; full 152/152; coverage 94.71%; Release 0/0; all local gates PASS | P2-3 owns real capacity/scheduler; no direct Ops/CRM; trust-skip off; real Sales/SIM/LAB/PROD NOT_RUN |
| `W-0020` | `P2-3` | policy registry/scheduler/channel leases | W-0019 | NOT_STARTED |  |  |  |  |
| `W-0021` | `P2-4` | speech + dial-token + mock SIM adapter | W-0020 | NOT_STARTED |  |  |  |  |
| `W-0022` | `P2-5` | DTMF/disposition normalizer | W-0021 | NOT_STARTED |  |  |  |  |
| `W-0023` | `P2-6` | target callback/outbox + GH compat | W-0022 | NOT_STARTED |  |  |  |  |
| `W-0024` | `P2-7` | script/content approval and safe variables (chạy TRƯỚC P2-1) | W-0016 | TESTS_PASS | Codex | `src/Ivr.Domain/Scripts/**`; `src/Ivr.Infrastructure/Scripts/**`; P2-7 migration/seed; specs/tests/evidence W-0024 | 117/117; coverage 94.71%; EF/model/format/build/OpenAPI/docs/UI/security/PII PASS | GitHub main pushed; GitLab main push BLOCKED_EXTERNAL by protected-branch rule; MOCK fixture only; LAB/real Sales/PROD NOT_RUN; OD-V1-15 + W-0003 remain open |
| `W-0025` | `P3-1` | Next.js/RBAC/API client/mode banners | W-0012,W-0018,W-0065 | NOT_STARTED |  |  |  |  |
| `W-0026` | `P3-2` | dashboard/log/detail masked evidence | W-0025,W-0065 | NOT_STARTED |  |  |  |  |
| `W-0027` | `P3-3` | config/integration/provider/channel/roles UI | W-0026,W-0065 | NOT_STARTED |  |  |  |  |
| `W-0028` | `P3-4` | privacy-safe reporting/analytics | W-0026,W-0055 | NOT_STARTED |  |  |  |  |
| `W-0029` | `P4-1` | real Sales provider wiring and CDC | W-0023,W-0002..6 | BLOCKED_EXTERNAL |  |  |  | build adapters with mocks first |
| `W-0030` | `P4-2` | Sales-owned blocker contract validation | W-0019 | NOT_STARTED |  |  |  | IVR does not directly own ops transition |
| `W-0031` | `P4-3` | CRM/voice restriction provider wiring | W-0019 | NOT_STARTED |  |  |  |  |
| `W-0032` | `P4-4` | production service auth/audit federation | W-0012,W-0006 | BLOCKED_EXTERNAL |  |  |  | mock JWT first |
| `W-0033` | `P4-5` | notification disabled/no-op boundary | W-0031 | DEFERRED_TARGET |  |  |  | no customer message in V1 |
| `W-0034` | `P4-6` | optional opt-out feedback review loop | W-0031 | DEFERRED_TARGET |  |  |  | cannot alter consent automatically |
| `W-0035` | `P5-1` | unit/integration/Testcontainers suite | W-0018..24,W-0065,W-0066 | NOT_STARTED |  |  |  |  |
| `W-0036` | `P5-2` | OpenAPI/CDC/E2E both modes/programs | W-0018..24,W-0025..27,W-0065,W-0066 | NOT_STARTED |  |  |  |  |
| `W-0037` | `P5-3` | performance/security/privacy/mode-isolation | W-0018..24,W-0065,W-0066,W-0030,W-0031 | NOT_STARTED |  |  |  |  |
| `W-0038` | `P5-4` | code review/static/security gates | W-0011 | NOT_STARTED |  |  |  |  |
| `W-0039` | `P5-5` | UI accessibility/i18n/visual QA | W-0025..28 | NOT_STARTED |  |  |  |  |
| `W-0040` | `P6-1` | redacted telemetry/tracing/metrics | W-0018..24,W-0065,W-0066 | NOT_STARTED |  |  |  | bao gồm provider/TTS telemetry của P2-9 |
| `W-0041` | `P6-2` | dashboards/SLO/alerts/readiness | W-0040 | NOT_STARTED |  |  |  |  |
| `W-0042` | `P6-3` | chaos/fail-closed/recovery exercises | W-0041,W-0030,W-0031 | NOT_STARTED |  |  |  |  |
| `W-0043` | `P7-1` | Docker/Compose incl fake Sales/mock SIM/JWT | W-0018..24,W-0025..27,W-0065,W-0066 | NOT_STARTED |  |  |  |  |
| `W-0044` | `P7-2` | Helm/K8s/modes/netpol/retention | W-0043,W-0041,W-0064 | NOT_STARTED |  |  |  |  |
| `W-0045` | `P7-3` | CI/CD/promotions/evidence | W-0044,W-0038 | NOT_STARTED |  |  |  |  |
| `W-0046` | `P7-4` | canary/rollback/flag ramp | W-0045 | NOT_STARTED |  |  |  |  |
| `W-0047` | `P7-5` | secret/token/cert rotation | W-0044,W-0032 | NOT_STARTED |  |  |  |  |
| `W-0048` | `P8-1` | real vendor adapter + **1 SIM lab** | W-0021,W-0066,W-0008 | BLOCKED_EXTERNAL |  |  |  | allowlist only |
| `W-0049` | `P8-2` | lab runbook/evidence, no customers | W-0048,W-0045 | BLOCKED_EXTERNAL |  |  |  | keep REAL_CUSTOMER_CALL_ALLOWED=NO |
| `W-0050` | `P9-1` | production release gate execution | W-0029,W-0032,W-0049,W-0059,W-0060 + mọi gate §3 | BLOCKED_EXTERNAL |  |  |  | 32 eSIM/legal/sign-off |
| `W-0051` | `P9-2` | cutover/rollback/ops/hypercare | W-0050,W-0064 | BLOCKED_EXTERNAL |  |  |  |  |
| `W-0052` | `P10-1` | privacy/legal/script/DSAR | W-0012,W-0031,W-0064 | NOT_STARTED |  |  |  | owner/legal inputs |
| `W-0053` | `P10-2` | data governance/backup/DR | W-0015,W-0044,W-0064 | NOT_STARTED |  |  |  |  |
| `W-0054` | `P10-3` | 1/32-channel capacity and cost model | W-0041,W-0037 | NOT_STARTED |  |  |  | measured data needed |
| `W-0055` | `P10-4` | analytics pipeline | W-0040 | NOT_STARTED |  |  |  |  |
| `W-0056` | `P10-5` | SLA/error budget/on-call | W-0041,W-0051 | BLOCKED_EXTERNAL |  |  |  | chờ P9-2 production ops runbook/hypercare; không suy ra on-call maturity từ mock |
| `W-0057` | `P11-1` | telephony RFQ + one-SIM lab + 32-eSIM requirements | — (chạy song song từ đầu; không phụ thuộc code) | NOT_STARTED |  |  |  | owns W-0008 closure |
| `W-0058` | `P11-2` | Sales/auth contract closure pack | W-0014 | NOT_STARTED |  |  |  | owns W-0002..7 closure |
| `W-0059` | `P11-3` | legal/retention/script/release package | W-0052,W-0053 | NOT_STARTED |  |  |  | owns W-0009 inputs |
| `W-0060` | `P11-4` | readiness board/evidence/go-no-go | — (liên tục; đọc tracker §3/§4/§5) | NOT_STARTED |  |  |  | mirrors, does not replace this ledger |
| `W-0062` | Red-team remediation | Sửa tài liệu/contract/prompt/fixture theo red-team findings (Origin=`RED_TEAM_REMEDIATION`) | W-0001 | EVIDENCE_SUBMITTED | Claude + IVR owner | governance restore, DB/OpenAPI/seed realign, 3 prompt mới, tracker | link/JSON/YAML/OpenAPI/seed-schema/prompt-graph checks — xem §9 | không đóng external gate nào; chờ owner review |
| `W-0064` | `P1-5` | retention job + data lifecycle (`IRetentionJob`) | W-0015 | TESTS_PASS | Codex | branch `codex/w0064-p1-5-retention`; Domain/Infrastructure/Worker; EF migration; DB-05; `docs/evidence/W-0064/` | 7/7 focused; full 105/105; coverage 93.70%; migration apply/rollback/recreate; format/config/OpenAPI/UI/security/PII/Compose PASS | owner/reviewer acceptance pending; production periods vẫn `OWNER_DECISION_REQUIRED` theo DF-07/`OD-V1-11`; `REAL_CUSTOMER_CALL_ALLOWED=NO` |
| `W-0065` | `P2-8` | IVR internal & admin API (13 operation còn thiếu) | W-0013,W-0018,W-0020,W-0022,W-0023 | NOT_STARTED |  |  |  | prereq của W-0025/W-0026/W-0027/W-0036; quyền `IVR_RUNTIME_GATE_ADMIN` chờ `OD-V1-20` |
| `W-0066` | `P2-9` | speech/TTS provider port + adapter skeleton | W-0021,W-0024,W-0064 | NOT_STARTED |  |  |  | prereq của W-0048; vendor TTS chờ `OD-V1-19` |
| `W-0067` | Fix: PII regex control char | Ký tự điều khiển `0x08` lọt vào regex PII của `pii_scan` (P0-2) làm pattern vô hiệu (Origin=`RED_TEAM_REMEDIATION`) | W-0062 | EVIDENCE_SUBMITTED | Claude | `P0-2` §6.2 | 0x08 = 0 toàn repo; 5/5 pattern test pass | pattern chuyển sang `deploy/ci/pii-patterns.txt`, tạo file thật ở W-0011 |
| `W-0068` | Fix: kill-switch immutability | “Immutable trong PRODUCTION_REAL” khiến **không bật được** kill switch khi sự cố — sửa thành bất đối xứng theo chiều an toàn (Origin=`RED_TEAM_REMEDIATION`) | W-0062 | EVIDENCE_SUBMITTED | Claude | `P0-4` §6.6-6.7, `specs/api/03-admin-api.md` | thêm `IT-FLAG-EMERGENCY-10`, sửa `IT-FLAG-PRODGUARD-07`/`-09` | quyền `IVR_RUNTIME_GATE_ADMIN` vẫn chờ `OD-V1-20` |
| `W-0069` | Fix: P2-1 ↔ P2-7 dependency | Bỏ prompt hư cấu `P2-7a`/`P2-7b`; `P2-7` chạy trước `P2-1` (Origin=`RED_TEAM_REMEDIATION`) | W-0062 | EVIDENCE_SUBMITTED | Claude | `P2-1`, `P2-7`, `prompt/00-index.md`, tracker §5 | cycle check 54 nodes = 0 cycle; `W-0024←W-0016`, `W-0018←W-0014..16,W-0024` | — |
| `W-0070` | Fix: nối W-0064/65/66 downstream | 3 Work ID mới chưa là prereq của bất kỳ downstream nào (Origin=`RED_TEAM_REMEDIATION`) | W-0062 | EVIDENCE_SUBMITTED | Claude | tracker §5 (9 row) + Meta của P3-1/2/3, P7-2, P10-1, P10-2, P8-1 | cycle check = 0; W-0036 free-text prereq đổi thành Work ID cụ thể | — |
| `W-0071` | Fix: cross-table CHECK | PostgreSQL không cho `CHECK` tham chiếu bảng khác; thêm cột denormalize `max_attempts_snapshot` + same-row CHECK (Origin=`RED_TEAM_REMEDIATION`) | W-0062 | EVIDENCE_SUBMITTED | Claude | `specs/database/02-tables.md` §3, `04-indexes.md` §4 | ghi rõ 3 cơ chế hợp lệ (snapshot+CHECK / trigger / app) | `P1-2` phải chọn cơ chế và ghi trong migration |
| `W-0072` | Fix: `order_state` + policy `delivery_area_short` | `order_state` required ở OpenAPI nhưng thiếu ở plan §5/IR-01; pattern `^[^0-9]*$` loại nhầm `"Quận 7"` hợp lệ (Origin=`RED_TEAM_REMEDIATION`) | W-0062 | EVIDENCE_SUBMITTED | Claude | OpenAPI, target-contract §5/§6, IR-01, `specs/data/05`, seed | required 22/22 khớp 3 tài liệu; pattern 7/7 case pass; seed 9/9 valid | whitelist vẫn chờ `OD-V1-15` |
| `W-0073` | Fix: PII gate case + artifact topology | Pattern địa chỉ chỉ khớp chữ thường (sót `Đường`, `SỐ NHÀ`); `grep -i` KHÔNG fold được `Đ`↔`đ` (đo: 1/3 dòng ở mọi locale). `pii_scan` chưa khai `needs`/`dependencies` nên không thấy artifact job khác ⇒ xanh giả (Origin=`RED_TEAM_REMEDIATION`) | W-0067 | EVIDENCE_SUBMITTED | Claude | `P0-2` §6.2/§7/§8/§9 | 19/19 pattern case pass; thêm `CT-CI-06b` (chữ HOA), `CT-CI-06c` (artifact liên job), `CT-CI-08` (mọi job artifact có trong `needs`) | `deploy/ci/pii-patterns.txt` tạo thật ở W-0011 |
| `W-0074` | Fix: dependency wildcard sweep | `W-0065` thiếu `W-0013` (P0-4 flags), `W-0066` thiếu `W-0064` (IRetentionJob); 13 row có Meta `P*-\*` hoặc prereq free-text lệch tracker (Origin=`RED_TEAM_REMEDIATION`) | W-0070 | EVIDENCE_SUBMITTED | Claude | tracker §5 (14 row) + Meta 9 prompt | kiểm tra ban đầu không thấy cycle | reviewer phát hiện hai direct dependency còn lệch; follow-up W-0077 |
| `W-0075` | Doc-map regeneration | Map phải do **mapper chính thức** `markdown-doc-reader` sinh; generator tự viết không tái lập được semantics (đo: 192/188/305/333/485/513/556 link — không giá trị nào ra 368) (Origin=`RED_TEAM_REMEDIATION`) | W-0062 | EVIDENCE_SUBMITTED | Codex | `.codex-doc-memory/markdown-doc-map.{json,md}` | official mapper: `384 files / 368 resolved / 0 unresolved / 1 duplicate / 16 anomalies / 21 orphans` | artifact đã đồng bộ; chờ owner review/acceptance, không suy ra contract/release readiness |
| `W-0076` | Fix: PII pattern locale-independence | Bracket expression đa byte (`[Đđ]`, `[ốỐ]`, `[àÀ]`) **vỡ dưới `LC_ALL=C`** — chỉ bắt 3/8 dòng, và `Ngõ` khớp nhầm do trùng byte `0xC3`. Container CI tối giản thường ở `LC_ALL=C` ⇒ gate lại xanh giả theo cách khác (Origin=`RED_TEAM_REMEDIATION`) | W-0073 | EVIDENCE_SUBMITTED | Claude + Codex | `P0-2` §6.2/§7/§8/§9 | Pattern trích thẳng từ prompt, BusyBox `grep -nE`: **49/49 PII, 0/5 false-positive** ở `C`, `C.UTF-8`, `POSIX`; thêm `CT-CI-06d/e/f`, gồm `ngách/NGÁCH/NGACH` và mixed-case | `deploy/ci/pii-patterns.txt` + hosted CT-CI tạo/chạy thật ở W-0011; current evidence là static prompt test |
| `W-0077` | Fix: direct dependency Meta/tracker drift | `P6-1` khai `P2-1..P2-9` nhưng W-0040 thiếu W-0066; `P10-5` khai/dùng trực tiếp `P9-2` nhưng W-0056 thiếu W-0051. Hai Work ID đều vắng cả direct dependency lẫn transitive closure (Origin=`RED_TEAM_REMEDIATION`) | W-0074 | EVIDENCE_SUBMITTED | Codex | tracker §5 + Meta/body của `P6-1`, `P10-5` | 54/54 prompt: 0 direct Meta/tracker mismatch; graph 54 node, 0 cycle; W-0040 có W-0066, W-0056 có W-0051 | W-0056 chuyển `BLOCKED_EXTERNAL` đúng với P9-2; không đóng external gate |
| `W-0078` | NuGet security gate remediation | `Origin=UNPLANNED` · discovered 2026-08-13 while closing W-0016 · Testcontainers 4.13.0 transitively resolves vulnerable SSH.NET 2025.1.0 · priority P0 · affects build/dependency gates | W-0015,W-0016 | TESTS_PASS | Codex | direct `SSH.NET 2026.0.0` pin in integration-test project + refreshed lockfile | advisory range `<=2025.1.0`; locked restore/build PASS 0/0; NuGet High audit 0; full 20/20 integration and 93/93 regression PASS | retain pin until Testcontainers declares a patched dependency; no production SSH/SIM/Sales execution |
| `W-0079` | P0-2 CI semantic fail-closed remediation | `Origin=RED_TEAM_REMEDIATION` · chặn CT-CI-02/03 xanh giả do mọi non-zero exit; validate schema/severity của NuGet vulnerability JSON | W-0011,W-0078 | TESTS_PASS | Codex | `Ivr.CiPolicy`; 6 vulnerability fixtures; semantic self-test; GitLab job wiring | CT-CI-02/03/09 PASS; intended marker vs typo-path controls; clean/High/empty/malformed/unknown cases; actual NuGet High gate PASS | local implementation complete; hosted GitLab evidence vẫn `NOT_RUN` dưới W-0061 |
| `W-0080` | P0-2 PII artifact coverage remediation | `Origin=RED_TEAM_REMEDIATION` · scanner phải phủ mọi text artifact, gồm `.sql` và file không extension; missing/zero-text target fail closed | W-0011,W-0076 | TESTS_PASS | Codex | `scan-pii.sh`; `selftest-pii.sh`; CI config guard; P0-2/CI README | CT-CI-06h PASS; SQL/extensionless rejected; missing/binary-only exit 2; final tree scan PASS 95 text/1 binary | local implementation complete; hosted artifact topology vẫn `NOT_RUN` dưới W-0061 |
| `W-0081` | Stable error catalog parity remediation | `Origin=RED_TEAM_REMEDIATION` · đồng bộ `IVR_PII_POLICY_VIOLATION` giữa API-06, OpenAPI, domain source và HTTP mapping; chặn drift | W-0012,W-0014 | TESTS_PASS | Codex | API-06; P0-3; domain errors/factory; 422 mapping; CI parity guard | CT-CI-10 exact-set 16/16 PASS; PII error envelope returns 422; OpenAPI lint/parse/drift PASS | local catalog complete; Target V1 remains `DRAFT` and owner acceptance is separate |
| `W-0082` | Error envelope boundary remediation | `Origin=RED_TEAM_REMEDIATION` · error middleware phải bao auth/allowlist; writer fail safely khi response đã start | W-0012,W-0081 | TESTS_PASS | Codex | middleware order; response writer started guard/log/abort; integration harness/tests | IT-FND-ERR-12/13 + full cross-cutting 7/7; full solution 96/96; coverage 91.50%; build 0/0 | HIGH blast radius regression passed locally; production runtime proof remains outside this work |
| `W-0083` | Source project dependency guard remediation | `Origin=RED_TEAM_REMEDIATION` · thay assembly-only check bằng exact `src/*.csproj` reference matrix | W-0010,W-0014 | TESTS_PASS | Codex | `ArchitectureDependencyTests` reads every source `.csproj` and exact approved direct refs | UT-BOOT-03 1/1; unit 54/54; full solution 96/96 | local guard complete; any new source project/reference must update reviewed matrix |
| `W-0084` | PostgreSQL audit append-only proof | `Origin=RED_TEAM_REMEDIATION` · chứng minh trực tiếp trigger chặn cả UPDATE và DELETE, bản ghi không đổi | W-0015 | TESTS_PASS | Codex | PostgreSQL Testcontainers `IT-DB-AUDIT-07` | direct UPDATE and DELETE both SQLSTATE `P0001`/append-only; row unchanged; integration 23/23; full 96/96 | local PostgreSQL proof complete; no staging/production database mutation performed |
| `W-0085` | Linux ProjectReference path portability | `Origin=UNPLANNED` · discovered 2026-08-13 from hosted GitLab job `15870797229` · Windows-style `ProjectReference` separators made UT-BOOT-03 fail only on Linux · priority P0 · affects hosted build gate | W-0083,W-0061 | ACCEPTED | Codex (explicit IVR owner authorization) | cross-platform project-name resolver + Windows/Unix separator regression; `docs/evidence/W-0085/` | local 98/98; hosted pipelines `#2756119982` và self-hosted `#2756183002` đều 9/9 jobs + 98/98 tests PASS | source defect closed; W-0061 remains separately open only for required independent MR approval |
| `W-0086` | Shallow-clone Gitleaks fingerprint remediation | `Origin=UNPLANNED` · MR `!3` job `15873689410` exposed immutable planning-prose false positive at depth boundary; persistent runner later retained orphan amended commits | W-0011,W-0061 | ACCEPTED | Codex | exact historical fingerprint plus validated `${CI_COMMIT_SHA:-HEAD}` Gitleaks history scope; `docs/evidence/W-0086/` | local current-HEAD 23 commits/20.45 MB/no leaks; MR pipeline `#2756668648` 9/9 jobs + 98 tests PASS; security `15874408908` and privacy `15874408909` PASS | scanner remains fail-closed; only pipeline commit ancestry is authoritative, not stale runner refs |

## 6. Unplanned work insertion template

Copy a row into §5 using `NEXT_WORK_ID`, then increment control immediately:

**Dùng ĐÚNG cột của §5** (trước đây template có cột lệch khiến `Priority` rơi vào ô `Status`):

| Work ID | Prompt | Scope summary | Prereq | Status | Owner | Artifacts/MR | Tests/evidence | Residual/next |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `W-XXXX` | — hoặc `PN-M` | `Origin=UNPLANNED` · discovered by/date · problem/scope · priority P0/P1/P2 · affected gate | Work ID phụ thuộc | một trong Status vocabulary §1 | named owner | links | commands/artifacts/reviewer | residual/next |

Never reuse or renumber an issued ID, even if cancelled.

## 7. Activity log (append-only)

| Seq | Timestamp | Work ID | Event | Detail | Actor | Evidence/result |
| --- | --- | --- | --- | --- | --- | --- |
| `A-0001` | 2026-08-12 | `W-0001` | START | Rà soát và realign plan/spec/prompt theo Sales dev answers và owner direction | Codex | baseline review |
| `A-0002` | 2026-08-12 | `W-0001` | CHECKPOINT | Target V1 draft, plan/spec/integration/OpenAPI/governance/tracker đang được cập nhật; validation chưa chạy | Codex | working tree |
| `A-0003` | 2026-08-12 | `W-0001` | HANDOFF | Hoàn tất realignment tài liệu; chưa khóa contract và chưa triển khai runtime | Codex | JSON 14/14; YAML 2/2; local refs pass; target seeds 2/2; Markdown links 0 unresolved; prompt IDs 51/51; git diff check pass |
| `A-0004` | 2026-08-12 | `W-0010` | DECISION | IVR dev xác nhận standalone ginsengfood-ivr tại repository hiện tại, namespace Ivr, GitHub Actions, docs/evidence, PostgreSQL outbox/IVR-owned schema, committed OpenAPI codegen và local MOCK | IVR dev | defaults-and-confirmations.md; P0-1 remains NOT_STARTED |
| `A-0005` | 2026-08-12 | `W-0011` | DECISION | IVR dev đổi CI provider sang GitLab CI và không dùng GitHub Actions; quyết định này supersede phần CI của A-0004 | IVR dev | defaults/P0-2/P5-4/P7-3 realigned; W-0011 remains NOT_STARTED |
| `A-0006` | 2026-08-12 | `W-0062` | START | Red-team remediation tài liệu. Baseline: `HEAD=b3a93aac90099169c1bc5df0afa6b216fa50a43c`, branch `main`, remote `https://github.com/ngquoctoan2001/ivr.git`, 13 file uncommitted trước khi bắt đầu | Claude | git status/diff/remote ghi trong §9 |
| `A-0007` | 2026-08-12 | `W-0062` | DISCOVERY | Governance §3 layout / §4 coding standards / §6 ladder đã bị **xóa** ở commit `b3a93aa` (không phải lỗi đánh số); 11 citation trong 7 prompt trỏ vào nội dung không còn tồn tại, khiến DoD của P0-1 không kiểm chứng được | Claude | `git show ab7de4d:prompt/README-governance.md` |
| `A-0008` | 2026-08-12 | `W-0061` | BLOCKED | TV1-12 khóa GitLab CI nhưng remote duy nhất là GitHub. Provisioning GitLab project/runner/registry/protected-branch được cấp Work ID riêng và gate `G-GITLAB`; `.gitlab-ci.yml` **không** được tạo trong lượt tài liệu này | Claude | `git remote -v`; P0-2 hosted evidence = `NOT_RUN` |
| `A-0009` | 2026-08-12 | `W-0062` | HANDOFF | Hoàn tất remediation tài liệu: governance khôi phục + đánh số, 32 prompt được gắn Work ID, DB gỡ hard-code attempt policy + thêm lease/fencing + 5 bảng foundation, OpenAPI đồng bộ enum/required/codegen, seed Target V1 mở rộng 9 task + tách negative theo lớp, 3 prompt mới (P1-5/P2-8/P2-9), 9 open decision mới `OD-V1-13..21` | Claude | validation trong §9; không đóng external gate nào |
| `A-0010` | 2026-08-12 | `W-0067` | FINISH | Gỡ 2 byte `0x08` khỏi regex PII trong `P0-2`; chuyển pattern sang file `deploy/ci/pii-patterns.txt` để escape không bị hỏng lần nữa | Claude | 0x08 = 0 toàn repo; 5/5 pattern test (bắt MSISDN/địa chỉ/dial_token, không bắt `560000 VND` và `Quận 7`) |
| `A-0011` | 2026-08-12 | `W-0068` | FINISH | Sửa mâu thuẫn kill switch: BẬT luôn được ở mọi env (chiều giảm rủi ro), TẮT/mở rộng allowlist mới cần four-eyes + deployment; không đọc được trạng thái ⇒ coi như ON | Claude | `P0-4` §6 bảng bất đối xứng; `IT-FLAG-EMERGENCY-10` mới |
| `A-0012` | 2026-08-12 | `W-0069` | FINISH | Bỏ `P2-7a`/`P2-7b` hư cấu; `P2-7` (`W-0024`) chuyển lên trước `P2-1` (`W-0018`) | Claude | cycle check 54 nodes = none |
| `A-0013` | 2026-08-12 | `W-0070` | FINISH | Nối `W-0064`→W-0044/51/52/53, `W-0065`→W-0025/26/27/36, `W-0066`→W-0048 ở cả tracker và Meta prompt | Claude | 12 row tracker + 7 prompt Meta; cycle = none |
| `A-0014` | 2026-08-12 | `W-0071` | FINISH | Thay CHECK liên bảng bằng cột `max_attempts_snapshot` + same-row CHECK; ghi rõ giới hạn `CHECK` của PostgreSQL và 3 cơ chế thay thế | Claude | `02-tables.md` §3, `04-indexes.md` §4 |
| `A-0015` | 2026-08-12 | `W-0072` | FINISH | Thêm `order_state` vào plan §5 + IR-01; thay pattern `delivery_area_short` quá chặt bằng pattern chỉ chặn số nhà | Claude | required 22/22 khớp OpenAPI/plan/IR; 7/7 pattern case; seed 9/9 valid |
| `A-0016` | 2026-08-12 | `W-0062` | CHECKPOINT | Regenerate `.codex-doc-memory/markdown-doc-map.*` sau khi thêm 3 prompt mới và sửa link | Claude | map đồng bộ với cây file thực tế |
| `A-0017` | 2026-08-12 | `W-0062` | HANDOFF | Đóng 6 nhóm lỗi còn lại của lượt remediation; chạy lại toàn bộ validation | Claude | xem §9 completion record W-0062 (bổ sung) |
| `A-0018` | 2026-08-12 | `W-0073` | FINISH | Pattern địa chỉ chuyển sang lớp ký tự tường minh (`[Đđ][ưƯ][ờỜ][Nn][Gg]`…) vì `grep -i` không fold được tiếng Việt; quy định topology `needs`/`dependencies` cho `pii_scan`; thêm `pii-patterns.txt` vào Output Artifacts | Claude | 19/19 case pass (gồm `Đường`, `SỐ NHÀ`, `NGÕ`, `DIAL_TOKEN`; không false-positive `560000 VND`/`Quận 7`) |
| `A-0019` | 2026-08-12 | `W-0074` | FINISH | Nối `W-0013`→`W-0065`, `W-0064`→`W-0066`; thay 13 prereq wildcard/free-text bằng Work ID cụ thể ở cả tracker lẫn Meta prompt | Claude | 0 row còn lỏng; cycle = none |
| `A-0020` | 2026-08-12 | `W-0075` | BLOCKED | Hoàn tác doc-map tự sinh (semantics khác mapper chính thức: 192 vs 368 link). Map revert về HEAD và ở trạng thái STALE tới khi mapper chính thức chạy | Claude | acceptance đã đo: 384/368/0/1/16 |
| `A-0021` | 2026-08-12 | `W-0062` | HANDOFF | Đóng 3 finding còn lại; chạy lại toàn bộ validation | Claude | xem báo cáo |
| `A-0022` | 2026-08-12 | `W-0076` | DISCOVERY | Pattern lớp ký tự của W-0073 vẫn hỏng: dưới `LC_ALL=C` chỉ bắt 3/8 dòng PII, sót `đường`/`Đường`/`ĐƯỜNG`/`SỐ NHÀ`; `Ngõ` khớp do trùng byte `0xC3` chứ không đúng ngữ nghĩa | Claude | `LC_ALL=C grep -nEf` trên fixture 8 dòng |
| `A-0023` | 2026-08-12 | `W-0076` | FINISH | Chuyển sang **alternation literal** (chuỗi byte UTF-8 nguyên vẹn) cho mọi ký tự có dấu; giữ lớp ký tự ASCII cho `dial_token`; job bắt buộc đặt `LC_ALL=C.UTF-8` tường minh | Claude | 8/8 PII + 0/0 false-positive ở 4 locale, pattern trích thẳng từ prompt |
| `A-0024` | 2026-08-12 | `W-0074` | CHECKPOINT | Xác minh lại hai dependency đã nối ở W-0074 (`W-0065←W-0013`, `W-0066←W-0064`); transitive closure cho thấy **0** phụ thuộc trực tiếp còn thiếu trên toàn bộ 54 prompt ⇒ **không cấp Work ID mới** cho hạng mục này | Claude | closure check 54 node |
| `A-0025` | 2026-08-12 | `W-0077` | START | Reviewer tái hiện hai dependency Meta/tracker còn lệch: W-0066 không nằm trong closure của W-0040 và W-0051 không nằm trong closure của W-0056; cấp Work ID riêng để sửa factual error của A-0024 | Codex | baseline `ff6734e7bb54819a3ab2cade5b798e374f7540dc`; direct + transitive dependency check |
| `A-0026` | 2026-08-12 | `W-0076` | DISCOVERY | Alternation theo cả cụm vẫn bỏ lọt `ngách`, `NGÁCH`, `NGACH` và hoa/thường trộn (`ĐưỜnG`, `Số NHÀ`, `nGáCh`) dù đã hết lỗi bracket đa byte | Codex | BusyBox `grep -nE` từ pattern trích thẳng P0-2 |
| `A-0027` | 2026-08-12 | `W-0076` | FINISH | Chuyển cụm địa chỉ sang alternation literal theo từng ký tự; bổ sung biến thể có dấu/không dấu và `CT-CI-06f` cho mixed-case | Codex | 49/49 PII + 0/5 false-positive ở `C`, `C.UTF-8`, `POSIX`; engine BusyBox 1.37.0 |
| `A-0028` | 2026-08-12 | `W-0077` | FINISH | Thêm W-0066 vào W-0040; thêm W-0051 vào W-0056 và chuyển W-0056 sang `BLOCKED_EXTERNAL` để khớp prerequisite P9-2 | Codex | 54/54 prompt, 0 direct Meta/tracker mismatch; graph 54 node, 0 cycle |
| `A-0029` | 2026-08-12 | `W-0075` | FINISH | Chạy official `markdown-doc-reader` mapper vào `.codex-doc-memory`; thay map stale 381/365 bằng artifact khớp cây tài liệu hiện tại | Codex | 384 files / 368 resolved / 0 unresolved / 1 duplicate / 16 anomalies / 21 orphans |
| `A-0030` | 2026-08-12 | `W-0075` | CHECKPOINT | Trong lượt final validation, một GitNexus index refresh ngoài phạm vi đã đồng thời sửa AGENTS.md, CLAUDE.md và 6 skill file; Codex không tạo/hoàn nguyên/stage các thay đổi này. Map được regenerate trên cây hiện tại; owner phải tách scope khi review/commit | Codex | baseline đầu lượt sạch; concurrent files có timestamp 15:33:04 và `.gitnexus/meta.json` indexedAt 08:33:03Z tại HEAD ff6734e |
| `A-0031` | 2026-08-12 | `W-0075` | CHECKPOINT | IVR dev chủ động chạy lại GitNexus và yêu cầu tiếp tục; giữ bộ AGENTS/CLAUDE/skill generated làm trạng thái có chủ đích, sau đó regenerate official Markdown map và rerun validation | IVR dev + Codex | GitNexus indexedAt 08:46:44Z, lastCommit ff6734e, 390 files / 36578 nodes / 36756 edges / 0 processes |
| `A-0032` | 2026-08-12 | `W-0010` | START | Bắt đầu P0-1 repo/solution bootstrap từ baseline đã freeze; dựng .NET 10 + PostgreSQL/EF Core + Next.js strict + Docker Compose hoàn toàn MOCK | Codex | baseline `5c6f39e892b2c6d85c56065f8c10dcdba2ba8770`, branch main, working tree clean; .NET SDK 10.0.201, Node 24.14.1, npm 11.11.0, Docker Engine 29.6.2/Compose 5.3.1 |
| `A-0033` | 2026-08-12 | `W-0010` | CHECKPOINT | Hoàn tất scaffold/source/tests/admin UI/Compose; xử lý analyzer logging bằng source-generated LoggerMessage và tách cổng Postgres IVR sang `55433` do `5432`, `55431`, `55432` đã bận | Codex | build 0 warning/0 error; tests 3/3; Postgres `healthy`, `127.0.0.1:55433` reachable |
| `A-0034` | 2026-08-12 | `W-0010` | FINISH/HANDOFF | Nộp evidence P0-1 ở trạng thái MOCK; chưa tự ACCEPTED và không tạo GitLab CI, business entity, Sales/SIM integration hay real-call path | Codex | `docs/evidence/W-0010/`; UI lint/build pass; health live/ready/startup 200 JSON; screenshot + console 0 issue; format/diff pass; owner review + GitNexus change review pending |
| `A-0035` | 2026-08-12 | `W-0010` | VALIDATION/HANDOFF | Lượt rà soát cuối đồng bộ launch URL `127.0.0.1:5088`, loại asset/README Next.js template sai, chạy lại build/test/UI/runtime và dọn tiến trình/container kiểm thử có kiểm soát | Codex | build 0/0; 3/3 test pass; UI lint/build pass; launch-profile probes 3/3 HTTP 200; official doc map 388 files/368 resolved/0 unresolved; Postgres container stopped, volume preserved |
| `A-0036` | 2026-08-12 | `W-0010` | ACCEPTANCE | Theo ủy quyền explicit của IVR owner, Codex tự review và chấp nhận P0-1 local bootstrap; acceptance không mở rộng sang real integration/telephony/production | Codex | GitNexus re-index 36,696 nodes/36,886 edges; status up-to-date; change risk LOW, 0 affected process; 0 circular import; direct diff review không có blocking finding |
| `A-0037` | 2026-08-12 | `W-0011` | START | Bắt đầu P0-2 từ P0-1 commit `85cefa7`; dựng GitLab-only quality baseline và negative self-tests, giữ mock/no-real-call invariants | Codex | branch main; GitNexus up-to-date; remote duy nhất `origin=https://github.com/ngquoctoan2001/ivr.git`; không có GitLab CI env nên W-0061 hosted evidence tiếp tục BLOCKED_EXTERNAL |
| `A-0038` | 2026-08-12 | `W-0011` | DISCOVERY | Exact security job phát hiện floating image `dotnet/sdk:10.0` resolve SDK 10.0.400, không tương thích `global.json` khóa 10.0.201; pin cả 3 job .NET về image 10.0.201 và thêm config regression check | Codex | lần đầu exit 1 trước restore; image tag 10.0.201 tồn tại; lượt chạy lại exact Linux security script exit 0 |
| `A-0039` | 2026-08-12 | `W-0011` | CHECKPOINT | Hoàn tất 6 quality gate và negative self-test CT-CI-01..08 ở local/container; OpenAPI có 10 advisory warning nhưng 0 lint/parser/ref/schema error | Codex | build 0 warning/0 error; 3/3 test; merged coverage 95.77% (68/71 unique lines); format 0/43; UI build; NuGet/npm/Gitleaks/PII/Compose PASS; official Markdown map 391 files/369 resolved/0 unresolved; `docs/evidence/W-0011/` |
| `A-0040` | 2026-08-12 | `W-0011` | FINISH/HANDOFF | Chốt P0-2 ở mức local/config `TESTS_PASS`; không nâng `ACCEPTED` vì chưa có GitLab project/runner/MR/protected-setting evidence theo DoD | Codex | implementation và evidence hoàn tất; W-0061/G-GITLAB tiếp tục BLOCKED_EXTERNAL; real Sales/SIM/customer call vẫn NOT_RUN |
| `A-0041` | 2026-08-12 | `W-0011` | DISCOVERY | Self-review sau handoff phát hiện phép cộng root count của nhiều Cobertura report đếm trùng source line; sửa policy thành merge theo package/class/line và thêm 2 report bổ sung nhau để chứng minh deduplicate | Codex | GitNexus impact LOW, 1 caller/0 process; merge fixture 100%; actual merged coverage 95.77% (68/71 unique lines) |
| `A-0042` | 2026-08-12 | `W-0011` | DISCOVERY | PII scanner ban đầu echo nguyên dòng vi phạm và `deploy/ci/node_modules` chưa được ignore; đổi log thành file:line:[REDACTED], thêm regression assertion, tổng quát hóa ignore và giữ coverage fixtures trackable | Codex | CT-CI-06* PASS với log redacted; evidence/artifact scan 22 files PASS; untracked scope giảm còn đúng 49 file P0-2 |
| `A-0043` | 2026-08-12 | `W-0011` | FINISH/HANDOFF | Rerun gate sau self-review; workflow giới hạn đúng MR, default-branch push và manual web (schedule không còn lọt); sẵn sàng dedicated P0-2 commit ở `TESTS_PASS` | Codex | config/OpenAPI/.NET/UI/security/PII/doc-map/GitNexus/diff gates PASS; hosted W-0061 tiếp tục BLOCKED_EXTERNAL |
| `A-0044` | 2026-08-12 | `W-0012` | START | Bắt đầu P0-3 cross-cutting foundation từ dedicated P0-2 commit; triển khai config validation, correlation, error envelope, RBAC, Order Core allowlist, idempotency, append-only audit, evidence và PII guard | Codex | baseline `0c2f692`, branch main, clean working tree trước official Markdown map; GitNexus up-to-date; MOCK/REAL_CUSTOMER_CALL_ALLOWED=NO; W-0061 không chặn local implementation |
| `A-0045` | 2026-08-12 | `W-0012` | CHECKPOINT | Hoàn tất source và test P0-3; bổ sung test riêng cho config/evidence ngoài 8 ID bắt buộc; bắt đầu đóng evidence và change review | Codex | Release build 0/0; 13/13 implemented tests; P0-3 10/10; merged coverage 91.46% (546/597); format/UI/OpenAPI/config/NuGet/npm/Compose/Gitleaks/PII PASS; hosted GitLab vẫn NOT_RUN |
| `A-0046` | 2026-08-12 | `W-0012` | FINISH/HANDOFF | Chốt P0-3 local MOCK ở `TESTS_PASS`; toàn bộ primitive, test và evidence đã hoàn tất nhưng không nâng `ACCEPTED` vì DoD yêu cầu chạy trong GitLab CI còn bị W-0061 chặn bên ngoài | Codex | official doc map 392 files/369 resolved/0 unresolved; GitNexus 37,086 nodes/37,561 edges/23 flows, entrypoint risk LOW, detect change LOW, 0 affected process, 0 cycle; Sales/SIM/lab/production NOT_RUN |
| `A-0047` | 2026-08-12 | `W-0012` | DISCOVERY/FIX | Staged change review báo CRITICAL do breadth 56 file/21 flow; review từng flow phát hiện PII guard chưa chứng minh mixed-case Unicode và correlation/audit/evidence metadata chưa guard đủ. Mở rộng case-folding + guard toàn metadata, correlation nghi PII được thay ID mới, thêm regression rồi rerun | Codex | `EnsureSafeText` impact HIGH: 4 direct/2 process nên được cảnh báo và kiểm thử đầy đủ; các middleware/store còn lại LOW; focused P0-3 10/10 PASS; full gate rerun pending |
| `A-0048` | 2026-08-12 | `W-0012` | VALIDATION/HANDOFF | Khóa context làm nguồn correlation outbound duy nhất, chặn PII trong idempotency key/snapshot, snapshot bất biến error details/catalog; hoàn tất full rerun sau remediation staged review | Codex | locked restore/format/build PASS 0/0; 14/14 implemented tests; P0-3 11/11; merged coverage 91.99% (563/612); W-0061 hosted GitLab vẫn NOT_RUN/BLOCKED_EXTERNAL |
| `A-0049` | 2026-08-12 | `W-0012` | CHANGE_REVIEW | Re-index và review staged scope sau mọi remediation. `detect_changes` vẫn CRITICAL do breadth foundation (56 file/228 symbol/25 flow); đối chiếu từng flow đều thuộc scope mới, impact cụ thể cao nhất PII guard HIGH đã có regression; không có caller/process ngoài dự kiến hay cycle | Codex | GitNexus 37,093 nodes/37,596 edges/27 flows; staged diff check PASS; 0 circular import; cảnh báo CRITICAL được giữ nguyên trong evidence, không hạ mức giả tạo |
| `A-0050` | 2026-08-12 | `W-0013` | START | Bắt đầu P0-4 feature-flag/dynamic-config/kill-switch/admin platform từ dedicated P0-3 commit; giữ mọi real-customer gate fail-closed | Codex | baseline `1c08cf0`, branch main, clean working tree trước official Markdown map; GitNexus up-to-date; OD-V1-20 chưa approved; W-0061 tiếp tục BLOCKED_EXTERNAL |
| `A-0051` | 2026-08-12 | `W-0013` | DISCOVERY | Source priority phát hiện P0-3 còn nhận mode lịch sử `LAB` thay vì canonical `LAB_REAL_SIM`; DB-02 §8 giao P0-4 entity/store nhưng migration cho P1-2, supersede dòng output migration trong prompt; runtime-gate permission OD-V1-20 phải mặc định fail-closed | Codex | governance §1/§2/§6; DB-02 §8; API-03 runtime-gate; sẽ sửa mode, không tạo migration sớm, test authorization approval chỉ bằng fake test-scoped |
| `A-0052` | 2026-08-12 | `W-0013` | IMPLEMENTATION/VALIDATION | Hoàn tất typed flag/config store, cache/refresh, centralized dispatch/kill gate, asymmetric audited admin API, four-eyes/self-authorization guard, EF model/45 safe seeds, Worker/API DI và OpenAPI; self-review sửa replay DTO và bảo đảm kill-on không bị cấu hình hỏng sẵn chặn | Codex | locked restore/build/format PASS 0/0; full suite 27/27 PASS ba lượt liên tiếp; all 10 required + 3 extra P0-4 IDs PASS; coverage 87.61%; UI/OpenAPI/config/NuGet/npm/Compose/Gitleaks/PII PASS; GitNexus final staged review pending |
| `A-0053` | 2026-08-12 | `W-0013` | REGRESSION/FIX | Full-suite stress tái hiện flake P0-3: GUID correlation sinh ngẫu nhiên đôi lúc giống chuỗi số bị PII guard chặn, gây HTTP 500; thay generator bằng prefix + tám nhóm hex 4 ký tự và thêm 1.000-case regression vào test PII hiện hữu | Codex | impact CorrelationContext/CorrelationMiddleware LOW (interface/DI lower-bound); Release 0/0; 27/27 PASS năm lượt liên tiếp; final coverage 87.50% (1183/1352); không tạo Work ID mới vì remediation trực tiếp cần để P0-4 full gate ổn định |
| `A-0054` | 2026-08-12 | `W-0013` | CHANGE_REVIEW/HANDOFF | Re-index và review staged scope sau remediation; giữ nguyên cảnh báo CRITICAL do breadth vertical platform, đối chiếu focused impact và process family trước commit riêng P0-4 | Codex | GitNexus 37,400 nodes/38,474 edges/62 flows; staged 42 file/279 symbol/49 flow CRITICAL (gồm 2 metadata file GitNexus); focused cao nhất InMemoryFeatureFlagStore MEDIUM với 11 test dependants, các entrypoint còn lại LOW; 0 cycle; không thấy consumer ngoài API/Worker/test/spec dự kiến |
| `A-0055` | 2026-08-12 | `W-0014` | START/DISCOVERY | Bắt đầu P1-1 từ dedicated P0-4 commit; đọc đủ governance/Target V1/API/OpenAPI/IR và kiểm chứng current Golden Hour trực tiếp trên Sales baseline; giữ target/current tách biệt và không nâng DRAFT thành approved | Codex | IVR baseline `c78a407`, Sales baseline `a3aad246`; current DTO 7 field + enum 4 giá trị + `X-Internal-Token`; official Markdown map start 393 file/369 resolved/0 unresolved; MOCK only, W-0002/W-0005/W-0006/W-0061 vẫn BLOCKED_EXTERNAL |
| `A-0056` | 2026-08-12 | `W-0014` | IMPLEMENTATION | Thêm NSwag codegen/pin/drift, generated IVR/Target contracts, current Golden Hour compatibility source-verified, typed provider/mode guard, fake Sales catalog và test contract; không trộn current/Target semantics | Codex | generated hashes ổn định; OpenAPI 2/2, exact matrix + 10 negative; current compat schema và Target-field rejection; contract 19, unit 22 |
| `A-0057` | 2026-08-12 | `W-0014` | VALIDATION/FIX | Full gate tìm thấy integration fixture LAB còn dùng mock SIM và PII scanner hiểu nhầm tên method Cobertura `get_Dial_token`; sửa fixture theo canonical LAB+VENDOR và regex chỉ nhận token khi có phép gán, thêm CT-CI-06g | Codex | integration 14/14; full 55/55; coverage 75.57%; PII self-test + 60 artifact PASS; Gitleaks/NuGet/npm/UI/Compose/OpenAPI/config PASS |
| `A-0058` | 2026-08-12 | `W-0014` | CHANGE_REVIEW/HANDOFF | Hoàn tất evidence, official Markdown map và GitNexus review; scoped change không có production flow/cycle ngoài contract/config/test dự kiến; chốt local MOCK ở TESTS_PASS và giữ external gates mở | Codex | map 396 file/369 resolved/0 unresolved; GitNexus 37,854 nodes/39,085 edges/66 flows; staged MEDIUM 42 file/452 symbol/4 generated-client flow; focused clients/selector/validator LOW, test fixture MEDIUM 5 caller; cycle 0 |
| `A-0059` | 2026-08-12 | `W-0015` | START/DISCOVERY | Bắt đầu P1-2 từ dedicated P1-1 commit; đọc đủ governance/Target V1/DB/data/both OAS, phân loại source cột và chốt PostgreSQL thật + no-hard-code policy; mở lại persistence P0-4 để đóng bằng transaction DB | Codex | baseline `5d2301e`; map baseline 396 file/369 resolved/0 unresolved; phát hiện DB-06 §3 còn prose stale max=2/300/150/900/450 trái Target/DB-04/P1-2 nên sẽ realign; MOCK only, DF-07 và W-0061 vẫn external |
| `A-0060` | 2026-08-12 | `W-0015` | IMPLEMENTATION | Dựng EF model/migration 17 bảng, policy/speech snapshot, opaque encrypted token hook, immutable callback outbox, SIM channel lease/fencing, persistent idempotency/audit/evidence và transaction feature-flag atomic; thêm DIND pin cho GitLab Testcontainers | Codex | migration Up 17 table/94 index/6 trigger; feature flags 45 safe seed; exact candidate timing không nằm trong CHECK; Down script tách riêng và ghi rõ total data loss; W-0061 runner privileged vẫn external |
| `A-0061` | 2026-08-12 | `W-0015` | VALIDATION/FIX | Testcontainers PostgreSQL thật bắt replay `IReadOnlySet` không deserialize và sửa serializer; sửa callback negative SQL; realign partial unique index để technical retry cùng attempt number không bị tính/chặn; thêm duplicate/raw-phone/PII/offset regression | Codex | dedicated PostgreSQL 6/6 PASS qua migrate/down/recreate, canonical seed, policy bounds, attempt trigger, atomic flag/audit/idempotency rollback/replay/conflict, lease concurrency/fencing và outbox exactly-once/immutability |
| `A-0062` | 2026-08-12 | `W-0015` | VALIDATION | Hoàn tất full local gate và evidence; đóng phần persistence P0-4 ở mức local TESTS_PASS, giữ toàn bộ external/runtime/production gate đúng trạng thái mở | Codex | Release 0/0; 61/61; coverage 90.64% (6765/7464); format/UI/OpenAPI/config/NuGet/npm/Compose/Gitleaks/PII PASS; official map 397 file/369 resolved/0 unresolved; GitNexus final review pending trong cùng work item |
| `A-0063` | 2026-08-12 | `W-0015` | SELF_REVIEW/FIX | Rà lại DB-02 và runtime invariant: bổ sung callback result status/state + index còn thiếu, cho phép nhiều raw provider event/attempt, siết task/policy/speech snapshot immutable, health gate kiểm tra bảng audit thật, release SIM xóa active job; tái sinh migration đầu tiên để model/snapshot/SQL đồng nhất | Codex | `has-pending-model-changes` none; dedicated PostgreSQL 6/6 PASS sau remediation; migration mới 17 table/94 index/6 trigger; final full gate/coverage/GitNexus sẽ rerun trước commit |
| `A-0064` | 2026-08-12 | `W-0015` | CHANGE_REVIEW/HANDOFF | Review staged scope và cycle sau full rerun; chốt P1-2 local ở TESTS_PASS, giữ W-0061/DF-07/KMS/backup-staging-production mở đúng trạng thái | Codex | GitNexus staged MEDIUM 40 file/35 indexed symbol/4 feature-flag read flow/0 cycle; symbol persistence mới chưa có trong index nên số graph là lower-bound, direct source + 6 PostgreSQL/61 full tests là bằng chứng chính |
| `A-0065` | 2026-08-13 | `W-0016` | START | Bắt đầu P1-3: domain/DTO/provider ports/privacy guards; giữ W-0061 là lane Platform độc lập | Codex | baseline `38eaecad2b4ce99aa14b12f708f5db1dd5fda5e9`; GitNexus pre-edit: `PiiGuard` LOW/0 caller, `SalesCallbackContractSelector` LOW/4 direct import, không có HIGH/CRITICAL; persistence P1-2 chưa có trong index nên dùng direct source làm authoritative |
| `A-0066` | 2026-08-13 | `W-0016` | BLOCKED | Đã thêm domain/ports/fakes/anti-corruption mappers/tests; không đóng P1-3 vì mọi lệnh `dotnet`/`git` và GitNexus detect_changes đều bị managed sandbox chặn process con | Codex | static delimiter/generated-enum/privacy audit PASS; repository graph đã parse symbol mới 38,679 node/40,898 edge/112 flow nhưng global registry write EPERM; focused impact `CallResultSnapshot` HIGH/8 nên giữ nguyên tới full tests; chi tiết `docs/evidence/W-0016/README.md` |
| `A-0067` | 2026-08-13 | `W-0016` | DOCUMENTATION | Ghi evidence trung thực cho scope đã triển khai và blocker; chạy lại official Markdown map sau khi thêm W-0016 evidence | Codex | map 398 Markdown file/369 resolved edge/0 unresolved link; không dùng static evidence thay cho build/test/commit còn NOT_RUN |
| `A-0068` | 2026-08-13 | `W-0016` | CHANGE_REVIEW | Refresh repository-local GitNexus graph sau code review và chạy required `detect_changes(compare, main)` | Codex | graph 38,695 node/40,916 edge/113 flow; focused HIGH: `AttemptOffsets` 10 impact/3 flow, `CallResultSnapshot` 8 impact/1 flow/3 module; remaining focused symbols LOW; `detect_changes` vẫn NOT_RUN vì internal `spawnSync git EPERM` |
| `A-0069` | 2026-08-13 | `W-0078` | START/DISCOVERY | Khi rerun full-solution build để đóng W-0016, NuGet phát hiện advisory High mới công bố ở SSH.NET 2025.1.0 do Testcontainers 4.13.0 kéo gián tiếp; cấp Work ID phát sinh và pin bản vá trực tiếp thay vì hạ quality gate | Codex | dependency chain xác nhận trong `tests/Ivr.IntegrationTests/packages.lock.json`; advisory affected `<=2025.1.0`, patched `2026.0.0`; Testcontainers.PostgreSql latest vẫn 4.13.0 |
| `A-0070` | 2026-08-13 | `W-0016` | UNBLOCKED/VALIDATION | Môi trường mới cho phép chạy `dotnet` và `git`; sửa analyzer/ambiguity bằng impact-first review và GitNexus rename, sau đó chạy toàn bộ closure gates | Codex | Release build 0 warning/0 error; format 0/138; 54/54 unit và 93/93 full tests; coverage 90.99%; UI/OpenAPI/config/Compose/security/PII PASS |
| `A-0071` | 2026-08-13 | `W-0078` | FINISH/HANDOFF | Pin SSH.NET bản vá trực tiếp vì Testcontainers chưa có release mới; giữ warning-as-error, không suppress advisory | Codex | locked restore PASS; NuGet vulnerability list rỗng; build 0/0; 20/20 integration + 93/93 regression; npm audit và Gitleaks PASS |
| `A-0072` | 2026-08-13 | `W-0016` | CHANGE_REVIEW/HANDOFF | Chốt P1-3 local MOCK ở TESTS_PASS sau khi rerun đủ build/test/coverage/security và staged change review; không nâng ACCEPTED hoặc production readiness | Codex | GitNexus graph 38,700 node/40,925 edge/113 flow; staged HIGH breadth 292 changed symbol/22 indexed file/12 expected speech-privacy-policy-mapping flow; 93/93 regression + 90.99% coverage là bằng chứng chính; W-0061 và mọi real Sales/SIM/customer gate vẫn mở |
| `A-0073` | 2026-08-13 | `W-0061` | EXTERNAL_PROGRESS/FAIL | GitLab project và remote `origin` đã có; push `main@3c0aa13` tạo pipeline hosted đầu tiên nhưng GitLab từ chối cấu hình trước khi sinh job vì `.dotnet_cache.cache.key.files` có 3 mục, vượt giới hạn 2 | IVR dev + Codex | project `nqt20102001/ginsengfood-ivr`; pipeline `#2755964245`; 0 job; exact error được lưu tại `docs/evidence/W-0061/README.md`; runner/protected branch/registry vẫn chưa có proof |
| `A-0074` | 2026-08-13 | `W-0061` | REMEDIATION/VALIDATION | Giảm .NET cache key còn `global.json` + `dotnet-tools.json`; thêm self-test tổng quát chặn mọi `cache:key:files` rỗng hoặc quá 2 mục; chuẩn bị commit/push riêng để GitLab tạo pipeline mới | Codex | impact LOW, blast radius 3 job kế thừa cache; `npm --prefix deploy/ci run test:config` PASS; `gitlab-ci-local --list` ENV_BLOCKED do máy Windows không có `/bin/bash`; hosted rerun và enforcement vẫn `NOT_RUN` |
| `A-0075` | 2026-08-13 | `W-0079` | START | Cấp remediation CI fail-closed; khóa baseline `a94b858`; parser policy và CT-CI-02/03 phải xác minh đúng semantic failure thay vì chấp nhận mọi exit khác 0 | Codex | GitNexus impact `RunVulnerabilities` LOW; direct source/fixture/CI inspection |
| `A-0076` | 2026-08-13 | `W-0080` | START | Cấp remediation coverage scanner cho mọi text evidence/artifact và zero-scan fail closed | Codex | direct scanner/self-test inspection; official Markdown map 399/369/0 |
| `A-0077` | 2026-08-13 | `W-0081` | START | Cấp remediation canonical 16-code catalog, chọn giữ `IVR_PII_POLICY_VIOLATION` vì Target contract, seed, IR-01 và P2-9 đã dùng | Codex | GitNexus impact `IvrErrorCodes`/`IvrErrors` LOW; OpenAPI/source/API-06 diff confirmed |
| `A-0078` | 2026-08-13 | `W-0082` | START | Cấp remediation error boundary và response-started behavior; cảnh báo blast radius trước sửa | Codex | GitNexus HIGH: writer 4 direct callers/4 process; status mapper HIGH via writer |
| `A-0079` | 2026-08-13 | `W-0083` | START | Cấp remediation exact source-project dependency matrix | Codex | GitNexus impact architecture test LOW; current 5 source projects/direct refs enumerated |
| `A-0080` | 2026-08-13 | `W-0084` | START | Cấp direct PostgreSQL UPDATE/DELETE proof cho append-only audit trigger | Codex | GitNexus impact PostgreSQL persistence tests LOW; migration trigger confirmed |
| `A-0081` | 2026-08-13 | `W-0079` | FINISH/HANDOFF | Harden vulnerability JSON schema/severity và thay negative gate bằng semantic self-test xác minh đúng test/coverage failure, kèm typo-path controls | Codex | CT-CI-02/03/09 PASS; actual NuGet High gate PASS; malformed/empty/unknown fixtures fail closed |
| `A-0082` | 2026-08-13 | `W-0080` | FINISH/HANDOFF | Bỏ extension whitelist, quét mọi regular text artifact; missing target và zero-text target fail closed; binary skip có counter | Codex | CT-CI-06h PASS; final scan `files=95 skipped_binary=1` |
| `A-0083` | 2026-08-13 | `W-0081` | FINISH/HANDOFF | Chốt canonical 16-code catalog với `IVR_PII_POLICY_VIOLATION`, 422 mapping/factory và exact-set drift guard | Codex | CT-CI-10 PASS; OpenAPI lint/parse/drift PASS; 422 integration regression PASS |
| `A-0084` | 2026-08-13 | `W-0082` | FINISH/HANDOFF | Đưa error envelope ra trước auth/allowlist; writer log+abort thay vì rewrite response đã start; thêm auth-stage/redaction/started-response matrix | Codex | cross-cutting 7/7; full solution 96/96; coverage 91.50%; build 0 warning/0 error |
| `A-0085` | 2026-08-13 | `W-0083` | FINISH/HANDOFF | UT-BOOT-03 đọc toàn bộ 5 source project và enforce exact direct-reference matrix | Codex | focused 1/1; unit 54/54; full solution 96/96 |
| `A-0086` | 2026-08-13 | `W-0084` | FINISH/HANDOFF | Testcontainers gửi UPDATE và DELETE trực tiếp vào `ivr_audit_log`, xác minh trigger `P0001` và bản ghi nguyên vẹn | Codex | focused 1/1; integration 23/23; full solution 96/96; không chạm staging/production |
| `A-0087` | 2026-08-13 | `W-0079..W-0084` | CHANGE_REVIEW/HANDOFF | Chạy GitNexus `detect-changes --scope all`; combined dirty tree gồm remediation này, generated GitNexus WIP và P1-4 concurrent work nên breadth là upper bound, không gán toàn bộ cho bundle này | Codex | CRITICAL aggregate 33 indexed files/96 symbols/31 flows; expected error-writer flows hiện diện; build 0/0, full 96/96, coverage 91.50%, focused matrices và privacy/config gates PASS |
| `A-0088` | 2026-08-13 | `W-0017` | START/DISCOVERY | Bắt đầu P1-4 từ baseline P1-3; chọn Redocly đã pin sẵn và oasdiff v1.26.1; giữ current Golden Hour tách khỏi Target V1 và Pages fail-closed tới khi access control được xác minh | Codex | baseline `a94b858`; GitNexus docs/CI impact LOW hoặc unindexed, 0 runtime process; MOCK/non-production only |
| `A-0089` | 2026-08-13 | `W-0017` | IMPLEMENTATION/VALIDATION | Dựng portal tĩnh 11 artifact, integration/versioning/changelog docs, manifest hash, baseline + breaking fixture và 3 GitLab jobs; self-review đổi Pages thành build thẳng `public/` và arm oasdiff bằng `--fail-on WARN` | Codex | CT-DOC-01/02 và UT-DOC-PII-03 PASS; Target/current boundary + link + CI topology PASS; oasdiff 2 baseline no-change; visual render PASS |
| `A-0090` | 2026-08-13 | `W-0017` | CHANGE_REVIEW/HANDOFF | Chốt P1-4 local ở TESTS_PASS; giữ hosted Pages, runner và access-control proof mở dưới W-0061, không suy ra contract approval hay production readiness | Codex | build 0/0; full 96/96; UI lint/build; OpenAPI/config/Compose/NuGet/npm/Gitleaks/PII PASS; official map 405 file/372 resolved/0 unresolved; GitNexus staged LOW 33 file/10 doc symbol/0 process; evidence `docs/evidence/W-0017/` |
| `A-0091` | 2026-08-13 | `W-0085` | START/DISCOVERY | Hosted Linux runner tái hiện UT-BOOT-03: `Path.GetFullPath` trên Linux không coi dấu gạch chéo ngược trong MSBuild Include là separator, nên actual project names còn nguyên `..\` | Codex | job `15870797229`, commit `2b1a4d4`; GitNexus focused impact LOW/0 caller/0 process |
| `A-0092` | 2026-08-13 | `W-0061` | EXTERNAL_PROGRESS | Hosted pipeline chứng minh project, checkout, SaaS Linux runner, Docker executor, DIND service, cache và artifact upload hoạt động; chưa đóng gate vì build job đỏ và settings/protection proof còn thiếu | IVR dev + Codex | runner `green-8.saas-linux-small-amd64`; job `15870797229`; artifacts/JUnit/Cobertura upload 201; root cause tracked W-0085 |
| `A-0093` | 2026-08-13 | `W-0085` | VALIDATION/HANDOFF | Chuẩn hóa cả separator Windows/Unix trước khi resolve ProjectReference và thêm regression riêng; chốt local/Linux sạch ở TESTS_PASS, chờ hosted rerun | Codex | impact LOW/0 process; Windows 3/3; Linux focused 3/3 + unit 56/56; full local 98/98; build 0/0; format/config PASS; `docs/evidence/W-0085/` |
| `A-0094` | 2026-08-13 | `W-0061` | START/EXTERNAL_PROGRESS | Provision self-hosted Docker executor trên Windows + Docker Desktop Linux containers; giữ nguyên `ops-core-win`, tạo project runner riêng cho IVR và Things, khóa theo project và chỉ nhận tag `ginsengfood-docker` | Codex + IVR owner | IVR runner `#55115499` online; Things runner `#55115556` online; GitLab Runner 19.2.0; config global `concurrent=3`, mỗi runner `limit=1`, `request_concurrency=2`; pipeline tagged/DinD proof đang chờ commit |
| `A-0095` | 2026-08-13 | `W-0061`,`W-0085` | VALIDATION/HANDOFF | IVR self-hosted pipeline xanh toàn bộ và đóng lỗi portability W-0085; Things cũng chứng minh runner Docker riêng. Phần runner/DIND của W-0061 hoàn tất, nhưng gate vẫn BLOCKED_EXTERNAL vì platform settings còn thiếu | Codex + IVR owner | IVR `#2756183002`: 9/9 jobs, 98/98 tests, coverage 91.5%, 19m37s, queue 3s, runner `#55115499`, jobs `15871330726/15871330732/15871330733`; Things `#2756187683`: G02 PASS, 12m59s, queue 3s, runner `#55115556` |
| `A-0096` | 2026-08-13 | `W-0061` | PLATFORM_ENFORCEMENT | Bảo vệ `main` (merge Maintainers, push No one, force-push off), bật `Pipelines must succeed`, giữ skipped pipeline không được coi thành công, tạo protected variables và khóa Pages `Only Project Members`; direct-push negative test bị pre-receive hook từ chối đúng thiết kế | Codex + IVR owner | `git push origin HEAD:main` rejected; variables `IVR_W0061_PROTECTED_PROBE` protected/masked/hidden và `API_DOCS_PUBLISH_NONPROD` protected; không lưu secret value |
| `A-0097` | 2026-08-13 | `W-0061` | HOSTED_VALIDATION/DISCOVERY | MR `!1` pipeline `#2756409438` xanh và chỉ auto-merge sau checks; Registry smoke `15872915564` PASS. Main pipeline `#2756451810` giữ lại bằng chứng lỗi thật: Pages script PASS nhưng deploy thiếu root `public/`; mở MR `!2` sửa output path + regression guard | Codex | MR `!1`; merge `b8044096`; Registry repo `ginsengfood-ivr/w0061-proof`; exact Pages warning `public: no matching files`; fix commit `4f3ce5f` |
| `A-0098` | 2026-08-13 | `W-0061`,`W-0017` | VALIDATION/HANDOFF | MR `!2` pipeline `#2756495155` xanh; protected-main pipeline `#2756517379` PASS 12 jobs/98 tests, Pages job/deploy xanh, private portal chặn anonymous. W-0017 được ACCEPTED; W-0061 còn duy nhất approval rule bắt buộc vì GitLab Free + một member | Codex + IVR owner | merge `ca10ebb4`; Pages job `15873355825`, `API_DOCS_OUTPUT=public`, 12 artifacts, HTTP 201; URL `https://ginsengfood-ivr-0332fa.gitlab.io/`; anonymous 302 to GitLab auth; UI MR=`Approval is optional` |
| `A-0099` | 2026-08-13 | `W-0086`,`W-0061` | DISCOVERY | MR `!3` pipeline `#2756568239` qua validate/build/98 tests nhưng security job đỏ; fresh depth-20 clone tái hiện đúng một Gitleaks false positive trong planning prose của immutable commit `b3a93aa`, trong khi full-history local scan xanh | Codex | job `15873689410`; fingerprint `b3a93aac...:prompt/phase-0-foundation/P0-2-ci-baseline-quality-gates.md:generic-api-key:75`; không có credential thật |
| `A-0100` | 2026-08-13 | `W-0086` | REMEDIATION/VALIDATION | Thêm đúng fingerprint commit/file/rule/line vào `.gitleaksignore`; không exempt file, không nới regex/rule, giữ CT-CI-04 fake-PAT negative test | Codex | remediated depth-20 scan PASS: 20 commits, 19.93 MB, no leaks; config PASS; hosted rerun còn bắt buộc trước ACCEPTED |
| `A-0101` | 2026-08-13 | `W-0086` | HOSTED_DISCOVERY/REMEDIATION | Pipeline `#2756604515` xác nhận fingerprint lịch sử đã được ignore, nhưng câu mô tả finding trong chính evidence W-0086 lại tạo meta false positive mới trên synthetic merge ref; sửa wording và amend commit thay vì thêm exception nối tiếp | Codex | security job `15873949053`; exact `refs/merge-requests/3/merge` depth-20 reproduction: 21 commits, one redacted match in `docs/evidence/W-0086/README.md:19`; `pii_scan` PASS |
| `A-0102` | 2026-08-13 | `W-0086` | HOSTED_DISCOVERY/REMEDIATION | Pipeline `#2756636651` vẫn thấy orphan commit cũ trong persistent runner worktree dù fresh clone của exact synthetic merge ref xanh; khóa Git history scan vào commit pipeline đã validate thay vì quét mọi local ref | Codex | job `15874176742`; fresh `refs/merge-requests/3/merge` PASS 21 commits/19.93 MB/no leaks; GitNexus blast radius LOW, 0 affected processes; thêm config regression guard cho `${CI_COMMIT_SHA:-HEAD}` + `--log-opts` |
| `A-0103` | 2026-08-13 | `W-0086`,`W-0061` | VALIDATION/HANDOFF | Final remediation pipeline xanh; chuyển W-0086 sang ACCEPTED và cho phép MR `!3` tiếp tục merge-check. W-0061 không đổi verdict, chỉ còn approval rule bắt buộc là BLOCKED_EXTERNAL | Codex | pipeline `#2756668648` PASS 9/9 jobs, 98 tests, 11m57s; security `15874408908` PASS 20 commits/19.91 MB/no leaks; privacy `15874408909` PASS |
| `A-0104` | 2026-08-13 | `W-0064` | START | Bắt đầu P1-5 retention/data lifecycle từ protected-main merge; giữ default dry-run, fail-closed khi thiếu period, legal hold thắng retention và audit/evidence accepted không bị purge | Codex | baseline `5544395`; branch `codex/w0064-p1-5-retention`; `REAL_CUSTOMER_CALL_ALLOWED=NO`; GitNexus refresh đang chạy trước symbol edits |
| `A-0105` | 2026-08-13 | `W-0064` | IMPLEMENTATION/VALIDATION | Dựng catalog 9 data class, default dry-run/fail-closed config, legal hold, child-first batch bằng `SKIP LOCKED`, checkpoint/resume, audit/metric/age alert, one-pass worker host và EF migration; self-review bổ sung trigger redaction một chiều + rollback trigger cũ và bảo vệ accepted evidence | Codex | UT-RET 1/1 + IT-RET 6/6 PASS trên PostgreSQL Testcontainers; IT-DB-MIGRATE-01 PASS; EF no pending model changes |
| `A-0106` | 2026-08-13 | `W-0064` | CHANGE_REVIEW/HANDOFF | Chốt P1-5 local ở `TESTS_PASS`; DB-05 phủ đủ 18 bảng, evidence aggregate không PII; giữ period production trống để Legal/Privacy quyết định và không nâng thành ACCEPTED | Codex | locked restore; build 0/0; format PASS; full 105/105; coverage 93.70%; CI config/OpenAPI/docs/UI/npm/NuGet/Compose/Gitleaks/PII PASS; official map 409 file/374 link/0 unresolved; evidence `docs/evidence/W-0064/` |
| `A-0107` | 2026-08-13 | `W-0024` | START | Bắt đầu P2-7 trực tiếp trên `main`: versioned script lifecycle, mode-specific approval, Target V1 Vietnamese renderer, PostgreSQL persistence và MOCK test-approved seed; không tạo branch/MR theo chỉ đạo IVR owner | Codex | baseline `458d0af`; prereq W-0016 satisfied; GitNexus pre-edit: IvrDbContext CRITICAL (18 symbols/8 flows), model/DI LOW; full shared-DbContext regression bắt buộc; REAL_CUSTOMER_CALL_ALLOWED=NO |
| `A-0108` | 2026-08-13 | `W-0024` | IMPLEMENTATION/VALIDATION | Hoàn thiện immutable script lifecycle, exact mode approval, RBAC/four-eyes/audit, Target V1 whitelist/renderer/preview/hash, MOCK seed và PostgreSQL trigger; focused remediation sửa EF navigation state và short-area `Thành phố` nhưng vẫn chặn full address | Codex | UT-SCRIPT 10/10; IT-SCRIPT + IT-DB-MIGRATE 3/3; EF no pending model changes; pre-edit ShortDeliveryArea impact LOW (10 symbols/2 flows) |
| `A-0109` | 2026-08-13 | `W-0024` | CHANGE_REVIEW/HANDOFF | Chốt P2-7 local ở `TESTS_PASS`; full shared-DbContext regression và privacy/security gate xanh, giữ MOCK/LAB/PROD tách biệt và không nâng synthetic fixture thành production approval | Codex | build 0/0; format PASS; 117/117; coverage 94.71%; OpenAPI/docs/UI/NuGet/npm/Compose/Gitleaks/PII PASS; official map 411 file/375 resolved/0 unresolved; evidence `docs/evidence/W-0024/` |
| `A-0110` | 2026-08-13 | `W-0024` | COMMIT/PUSH_HANDOFF | Commit implementation P2-7 trực tiếp trên `main`; GitHub fast-forward thành công, GitLab từ chối vì protected `main` vẫn cấu hình No one can push. Không tự hạ protection và không tạo branch/MR trái chỉ đạo owner | Codex | implementation commit `e911fc1`; GitHub `main` pushed; GitLab exact error `You are not allowed to push code to protected branches`; origin/main remains behind local main |
| `A-0111` | 2026-08-13 | `W-0018` | START | Bắt đầu P2-1 trực tiếp trên `main`: endpoint Target V1, ordered validation, exact script approval, atomic task/job/outbox/audit, idempotent replay/conflict và fake scenarios cho cả hai program path; không tạo branch/MR theo chỉ đạo IVR owner | Codex | baseline `addc423`; prereq W-0014/W-0015/W-0016/W-0024 satisfied; GitNexus refreshed at baseline; mapper/DI LOW, `ConfirmationTaskEntity` HIGH (44 symbols), `IvrDbContext` CRITICAL (18 symbols/8 flows); full PostgreSQL regression bắt buộc; `REAL_CUSTOMER_CALL_ALLOWED=NO` |
| `A-0112` | 2026-08-13 | `W-0018` | IMPLEMENTATION/VALIDATION | Dựng Target V1 intake endpoint, strict schema/canonical idempotency, ordered domain gates, exact attempt/script lookup, MOCK dry-run, atomic PostgreSQL task/job/outbox/audit/idempotency và fake coverage cho cả hai program path | Codex | contract 21 + unit 80 + integration 43 = 144/144; PostgreSQL 8 concurrent → `1/1/1/1/1`; reject → zero task/job/outbox; coverage 95.26%; EF no pending changes |
| `A-0113` | 2026-08-13 | `W-0018` | SELF_REVIEW/REMEDIATION | Chặn task identifier không public-safe trước khi dùng làm idempotency/audit scope; thu hẹp schema-error catch để lỗi runtime/service không bị ngụy trang thành malformed request; chốt evidence ở MOCK ceiling `TESTS_PASS` | Codex | locked restore; format; build 0/0; CI config/OpenAPI/docs/UI/NuGet/npm/Compose/PII PASS; official map 412/375/0; baseline Gitleaks history 27 commits/no leaks; final GitNexus/commit history scan còn chạy trước commit |
| `A-0114` | 2026-08-13 | `W-0018` | CHANGE_REVIEW/HANDOFF | Staged review chỉ gồm P2-1, loại AGENTS.md/CLAUDE.md metadata WIP; GitNexus xác nhận CRITICAL breadth đúng với intake API + shared persistence, nên giữ verdict `TESTS_PASS` và dựa trên full API/PostgreSQL/regression proof | Codex | staged 32 files/206 indexed symbols/57 flows; build 0/0; 144/144; coverage 95.26%; EF no drift; focused 13/13 + PostgreSQL 3/3; diff-check PASS |
| `A-0115` | 2026-08-13 | `W-0018` | COMMIT/PUSH_HANDOFF | Commit P2-1 trực tiếp trên `main`; post-commit Gitleaks sạch; GitHub fast-forward thành công, GitLab từ chối vì protected `main` vẫn cấu hình No one can push. Không hạ protection và không tạo branch/MR trái chỉ đạo owner | Codex | implementation `85c2b63`; Gitleaks 28 commits/21.19 MB/no leaks; GitHub remote ref verified exact; GitLab remains `5544395` with pre-receive rejection |
| `A-0116` | 2026-08-13 | `W-0019` | START/DISCOVERY | Bắt đầu P2-2 trực tiếp trên `main`: pure eligibility rules, fail-closed sellable/voice restriction/contact/window/capacity, trust-skip disabled và atomic task/job/capacity/audit/evidence update; không gọi Ops/CRM trực tiếp | Codex | baseline `8751d3f`; P2-1 prereq TESTS_PASS; official map 412/375/0; P2-1 `Accepted` impact HIGH 13 symbols/3 flows, DI LOW; REAL_CUSTOMER_CALL_ALLOWED=NO |
| `A-0117` | 2026-08-13 | `W-0019` | IMPLEMENTATION | Chuyển accepted intake về `PENDING_ELIGIBILITY`; dựng ordered pure rules và service chỉ đọc stored snapshot; persistence khóa theo task và cập nhật task/job/outbox/reason/evidence/audit/capacity incident atomically; MOCK eligible vẫn no-egress | Codex | `EligibilityRules`, `EligibilityService`, in-memory/PostgreSQL repository; trust-skip application flag hard-off; default non-MOCK capacity fail-closed; không có direct Ops/CRM client |
| `A-0118` | 2026-08-13 | `W-0019` | VALIDATION/HANDOFF | Chốt P2-2 local ở `TESTS_PASS`: required blocker/DNC/trust/fail-closed/capacity cases xanh; bổ sung MOCK eligible hold, stored restriction-before-capacity và capacity-evidence fail-closed proof; full regression, coverage, EF, API/docs/UI/security/privacy/map đều xanh | Codex | focused unit 4/4 + integration 4/4; full 152/152; coverage 94.71%; Release 0 warning/0 error; EF no drift; Gitleaks no leaks; PII PASS; map 413/375/0; real calls NO |

## 8. Per-work completion record template

Append one block when a work item reaches a handoff/status boundary:

```text
Work ID:
Baseline/commit:
Scope completed:
Files/artifacts:
Commands and exact results:
Tests/evidence:
Review/acceptance by:
Mock-only evidence:
Lab evidence:
Real integration evidence:
Production evidence:
Residual blockers/risks:
Next allowed Work ID(s):
Final status:
```

## 9. Completion records

```text
Work ID: W-0001
Baseline/commit: Sales PhucApu@a3aad246d986fbc273cf41aaa93eec6659669656; IVR main@ab7de4d59eb04eb9f172385a1ffa4d25023064e5; documentation working tree 2026-08-12
Scope completed: realign Target V1 plan, specs, API contracts, integration requests, prompts, execution governance and the single progress ledger
Files/artifacts: plan/ivr-orther/target-contract-v1-draft.md; specs/api/openapi/*.yaml; seed/sales-target-v1.sample.json; prompt/00-index.md; prompt/RUNBOOK-execute-prompts.md; this ledger
Commands and exact results: JSON parse 14/14; YAML parse 2/2; OpenAPI local refs PASS; target seed schema 2/2; Markdown files 381 and unresolved links 0; prompt IDs 51/51 with no tracker omissions; W-0001..W-0060 no gaps; git diff --check PASS
Tests/evidence: documentation/contract static validation only; no runtime implementation, build, real API, telephony or production evidence
Review/acceptance by: pending IVR owner and Sales/Core reviewers
Mock-only evidence: canonical fake Sales fixtures prepared for GOLDEN_HOUR/ONLINE and TWENTY_FOUR_SEVEN/COD
Lab evidence: NOT_RUN; requires approved destination allowlist and one real SIM
Real integration evidence: NOT_RUN; blocked by W-0002..W-0008
Production evidence: NOT_RUN; requires 32 eSIM readiness and all release gates
Residual blockers/risks: Target Contract V1 remains DRAFT; attempt policy, Sales endpoints/payload, dial-token, auth, legal and telephony inputs remain external
Next allowed Work ID(s): W-0010; W-0057 and W-0058 may start early in parallel when owners are available
Final status: EVIDENCE_SUBMITTED
```

```text
Work ID: W-0062
Origin: RED_TEAM_REMEDIATION
Baseline/commit: HEAD b3a93aac90099169c1bc5df0afa6b216fa50a43c on main; remote origin https://github.com/ngquoctoan2001/ivr.git; 13 files uncommitted at start (.codex-doc-memory x2, decisions-log, prompt/00-index, README-governance, _TEMPLATE, defaults-and-confirmations, tracker, P0-1, P0-2, P5-4, P7-3, traceability-matrix)
Findings addressed: N-03/F-08 governance deleted content; F-09 compose command; F-20 missing Work IDs; F-01/A-07 DB attempt-policy hard-code; F-13/A-06 DB<->OpenAPI required inversion; F-14 lease/fencing absent; F-04/A-08 seed scenarios unrunnable; F-05/A-04 negative fixtures wrong layer; F-11/A-02 ResultType divergence; F-03/A-05 task required-field drift; F-16 delivery_area_short unenforceable; N-02/F-02(F1) admin API has no prompt; F-06/M-13/H-04 no TTS prompt; F2-03 IRetentionJob missing; N-08/F2-01 COD-only test; N-04/E-06 speech whitelist; N-05/E-03 dial-token reuse; N-06/E-04 runtime-gate RBAC; N-07/E-05 token-vault ownership; F-10/M-01 CI fragments; F2-02 mode key drift; E-07 X-Permissions; E-08 JWT scopes; E-09 recording OFF enforcement; E-10 PII CI gate; F-07 Phase-4 DoD; F-03(F1) P2-1<->P2-7 cycle; F-04(F1) missing EF tables; F-06(F1) health/ready ownership; F2-08 P9-1 outputs; F2-09 P9-2 blast radius; F2-07/M-09 platform deps + evidence root; M-04 Work ID binding; G-04 unplanned template columns
Scope completed: documentation/contract/prompt/fixture remediation only
Files/artifacts: see report; 3 new prompts (P1-5/W-0064, P2-8/W-0065, P2-9/W-0066); 9 new open decisions OD-V1-13..21; 2 new external rows W-0061/W-0063; 2 new gates G-GITLAB/G-PLATFORM
Commands and exact results: recorded in the remediation report; OpenAPI 2/2 parse with 88 refs and 0 broken; Target V1 seed 9/9 tasks valid; 7/7 schema_negative reject at schema layer; 7/7 domain_negative reach the domain layer; prompt IDs 54/54 unique with 54/54 Work IDs; 0 dependency cycles; git diff --check PASS
Tests/evidence: documentation/contract static validation only. NO runtime implementation, NO build, NO real API, NO telephony, NO production evidence
Review/acceptance by: pending IVR owner
Mock-only evidence: canonical fake Sales fixtures extended to 9 tasks + 12 callback scenarios for both programs
Lab evidence: NOT_RUN
Real integration evidence: NOT_RUN
Production evidence: NOT_RUN
Residual blockers/risks: TARGET_CONTRACT_V1 stays DRAFT. OD-V1-01..21 all open. W-0002..W-0009, W-0061, W-0063 all BLOCKED_EXTERNAL. Working tree is uncommitted and MUST be frozen/reviewed by the owner before W-0010 starts (no commit performed by this remediation)
Next allowed Work ID(s): W-0010 after the owner freezes the baseline; W-0057/W-0058/W-0061/W-0063 may be requested in parallel
Final status: EVIDENCE_SUBMITTED
```

```text
Work ID: W-0062 (bổ sung — vòng sửa lỗi thứ hai)
Origin: RED_TEAM_REMEDIATION
Follow-up Work IDs: W-0067, W-0068, W-0069, W-0070, W-0071, W-0072
Findings addressed (6 nhóm còn lại sau vòng một):
  1. W-0067 — ký tự điều khiển 0x08 lọt vào regex PII của job pii_scan (P0-2) làm pattern vô hiệu
  2. W-0068 — "immutable trong PRODUCTION_REAL" mâu thuẫn với chức năng kill switch (không bật được khi sự cố)
  3. W-0069 — dependency P2-1 <-> P2-7 vẫn treo vì tham chiếu prompt hư cấu P2-7a/P2-7b
  4. W-0070 — W-0064/W-0065/W-0066 không phải prereq của bất kỳ downstream nào
  5. W-0071 — CHECK "attempt_number <= ivr_call_jobs.max_attempts" không hợp lệ trong PostgreSQL (cross-table)
  6. W-0072 — order_state required ở OpenAPI nhưng thiếu ở plan §5/IR-01; pattern delivery_area_short loại nhầm "Quận 7"
Commands and exact results: xem báo cáo; 0x08=0 toàn repo; pattern PII 5/5 case; delivery_area_short 7/7 case;
  OpenAPI 2/2 parse, 88 refs, 0 broken; seed 9/9 valid; schema_negative 7/7 đúng lớp; domain_negative 10/10 đúng lớp;
  required 22/22 khớp OpenAPI/plan §5/IR-01; prompt 54/54 unique ID + Work ID + Forbidden + DoD; 0 dependency cycle;
  Markdown 0 unresolved; git diff --check PASS; 0 commit
Tests/evidence: documentation/contract static validation only. NO runtime, NO build, NO real API, NO telephony
Review/acceptance by: pending IVR owner
Residual blockers/risks: không đổi so với vòng một — TARGET_CONTRACT_V1 vẫn DRAFT; OD-V1-01..21 mở;
  W-0002..W-0009, W-0061, W-0063 BLOCKED_EXTERNAL; baseline vẫn chưa commit (BASELINE_FREEZE_REQUIRED)
Final status: EVIDENCE_SUBMITTED
```

```text
Work ID: W-0075
Origin: RED_TEAM_REMEDIATION
Baseline/commit: HEAD ff6734e7bb54819a3ab2cade5b798e374f7540dc on main; clean working tree at start
Scope completed: regenerate repository Markdown map bằng đúng official markdown-doc-reader mapper, không dùng generator tự viết
Files/artifacts: .codex-doc-memory/markdown-doc-map.md; .codex-doc-memory/markdown-doc-map.json
Commands and exact results: node md_doc_map.js <repo> --out <repo>/.codex-doc-memory; 384 Markdown files; 368 links resolved; 0 unresolved; 1 duplicate title; 16 encoding/name anomalies; 21 orphan candidates
Tests/evidence: official generated artifacts + summary counts; final JSON/OpenAPI/seed/prompt/dependency/control-char/diff validation PASS
Review/acceptance by: pending IVR owner
Mock-only evidence: N/A — documentation index only
Lab evidence: NOT_RUN
Real integration evidence: NOT_RUN
Production evidence: NOT_RUN
Residual blockers/risks: duplicate/anomaly/orphan inventories vẫn hiện hữu trong map và không tự động đồng nghĩa defect; cần review riêng nếu owner muốn dọn cấu trúc. IVR dev đã chủ động rerun GitNexus và giữ generated changes làm input hiện tại; khi commit nên tách GitNexus-generated scope khỏi remediation docs. Không đóng contract/external/release gate
Next allowed Work ID(s): W-0010 chỉ sau baseline owner review/freeze; external W-0057/W-0058/W-0061/W-0063 có thể chạy theo owner
Final status: EVIDENCE_SUBMITTED
```

```text
Work ID: W-0076 (review follow-up)
Origin: RED_TEAM_REMEDIATION
Baseline/commit: HEAD ff6734e7bb54819a3ab2cade5b798e374f7540dc on main
Scope completed: đóng false-negative còn lại của PII pattern cho ngách có dấu/không dấu, uppercase và hoa/thường trộn mà vẫn không dùng bracket expression đa byte
Files/artifacts: prompt/phase-0-foundation/P0-2-ci-baseline-quality-gates.md; tracker
Commands and exact results: trích 6 pattern thẳng từ prompt; BusyBox 1.37.0 grep -nE; C=49/49 PII + 0/5 false-positive; C.UTF-8=49/49 + 0/5; POSIX=49/49 + 0/5
Tests/evidence: CT-CI-06b thêm NGÁCH; CT-CI-06e thêm NGACH; CT-CI-06f thêm mixed-case ĐưỜnG/Số NHÀ/nGáCh/HẻM
Review/acceptance by: pending IVR owner
Mock-only evidence: static prompt/regex fixture only; deploy/ci/pii-patterns.txt và GitLab job chưa tồn tại
Lab evidence: NOT_RUN
Real integration evidence: NOT_RUN
Production evidence: NOT_RUN
Residual blockers/risks: phải implement file pattern, artifact topology và CT-CI-06* thật ở W-0011; hosted GitLab evidence vẫn BLOCKED_EXTERNAL bởi W-0061
Next allowed Work ID(s): W-0011 sau W-0010
Final status: EVIDENCE_SUBMITTED
```

```text
Work ID: W-0077
Origin: RED_TEAM_REMEDIATION
Baseline/commit: HEAD ff6734e7bb54819a3ab2cade5b798e374f7540dc on main
Scope completed: đồng bộ hai direct prerequisite còn lệch giữa prompt Meta và canonical tracker
Files/artifacts: prompt/_execution/prompt-execution-tracker.md; source Meta/body được đối chiếu ở P6-1 và P10-5, không cần sửa source prompt
Commands and exact results: parse 54 canonical prompt + tracker; direct Meta/tracker mismatch=0; dependency graph=54 node; cycle=0; W-0040 contains W-0066; W-0056 contains W-0051
Tests/evidence: deterministic static dependency/graph validation
Review/acceptance by: pending IVR owner
Mock-only evidence: N/A — planning dependency correction only
Lab evidence: NOT_RUN
Real integration evidence: NOT_RUN
Production evidence: NOT_RUN
Residual blockers/risks: W-0056 chuyển BLOCKED_EXTERNAL vì P9-2/W-0051 phụ thuộc production release path; không được chạy on-call maturity như thể ops runbook đã có
Next allowed Work ID(s): W-0078 chưa cấp; W-0010 vẫn là implementation work đầu tiên sau baseline freeze
Final status: EVIDENCE_SUBMITTED
```

```text
Work ID: W-0010
Prompt: P0-1
Baseline/commit: baseline main@5c6f39e892b2c6d85c56065f8c10dcdba2ba8770; clean working tree at START; this completion record is included in the dedicated P0-1 commit
Scope completed: standalone .NET 10 solution bootstrap; API health probes; empty worker heartbeat; empty EF Core/Npgsql context; strict Next.js admin placeholder; PostgreSQL 16 and inert mock-provider Compose placeholders; root tooling and runbook
Files/artifacts: Ivr.sln; Directory.Build.props; .editorconfig; global.json; .gitignore; src/**; tests/**; admin-ui/**; docker-compose.dev.yml; README.md; docs/evidence/W-0010/**
Commands and exact results: dotnet restore PASS; dotnet build PASS 0 warnings/0 errors; dotnet test PASS 3/3 implemented tests; dotnet format verify PASS 0/39 changed; npm lint PASS; npm build PASS; npm audit 0 vulnerabilities; Compose config PASS; Postgres healthy and localhost port 55433 reachable; three API probes HTTP 200 JSON; browser console 0 warning/error; official Markdown mapper 388 files/368 resolved/0 unresolved; GitNexus staged 59 files/79 symbols, LOW risk/0 process/0 cycle; git diff --check PASS
Tests/evidence: UT-BOOT-01 PASS; IT-BOOT-02 PASS for live/ready/startup; UT-BOOT-03 PASS; screenshot docs/evidence/W-0010/admin-ui-mock-mode.png; detailed command evidence docs/evidence/W-0010/README.md
Review/acceptance by: ACCEPTED 2026-08-12 by Codex self-review under explicit IVR owner authorization; scope limited to P0-1 local bootstrap
Mock-only evidence: complete for P0-1; IVR_EXECUTION_MODE=MOCK; SALES_PROVIDER=FAKE_TARGET_V1; SIM_PROVIDER=MOCK; REAL_CUSTOMER_CALL_ALLOWED=NO
Lab evidence: NOT_RUN; one real SIM belongs W-0048/W-0049 and requires allowlist/vendor decisions
Real integration evidence: NOT_RUN; no Sales API/auth/SIM adapter connected
Production evidence: NOT_RUN; no staging, 32 eSIM, protected GitLab pipeline, or release evidence
Residual blockers/risks: readiness is an always-200 bootstrap placeholder until W-0040; GitLab CI belongs W-0011; private registry remains open but non-blocking; no real Sales/SIM/lab/production evidence is implied by this acceptance
Next allowed Work ID(s): W-0011; W-0057/W-0058 remain separately eligible when their owners and external inputs are available
Final status: ACCEPTED
```

```text
Work ID: W-0011
Prompt: P0-2
Baseline/commit: baseline main@85cefa7 (accepted P0-1); this record is included in the dedicated P0-2 commit
Scope completed: GitLab-only workflow routing; locked .NET/UI builds; JUnit/Cobertura and 60% coverage gate; .NET format/analyzers; OpenAPI lint/parse/ref/schema checks; NuGet/npm vulnerability policy; Gitleaks; locale-stable PII scan with upstream artifact topology; MR template; CODEOWNERS routing; local/hosted runbook
Files/artifacts: .gitlab-ci.yml; deploy/ci/**; .gitlab/merge_request_templates/Default.md; CODEOWNERS; .gitleaks.toml; .gitleaksignore; .redocly.yaml; committed NuGet lockfiles; docs/evidence/W-0011/README.md
Commands and exact results: config CT-CI-05/07/08 PASS; OpenAPI 2/2 parse, 9 valid target tasks, 7/7 schema negatives rejected, 10/10 domain negatives schema-valid; Release build 0 warnings/0 errors; 3/3 implemented tests; merged coverage 95.77% (68/71 unique lines, 3 reports) >= 60%; format 0/43; UI lint/build PASS; NuGet/npm High policy PASS and npm 0 vulnerabilities; exact Linux security script exit 0; PII selftests and evidence/artifact scan PASS; Compose config PASS; Gitleaks dir scan no leaks; official Markdown map 391 files/369 resolved/0 unresolved
Tests/evidence: CT-CI-01 through CT-CI-08 PASS locally, including expected non-zero for invalid OpenAPI, failing xUnit, 50% coverage, fake GitHub PAT, and downloaded-artifact PII; detailed evidence at docs/evidence/W-0011/README.md
Review/acceptance by: Codex self-review; local/config status only. Prompt DoD limits this evidence to TESTS_PASS until hosted GitLab evidence exists
Mock-only evidence: complete for P0-2 quality baseline; IVR remains MOCK and REAL_CUSTOMER_CALL_ALLOWED=NO
Lab evidence: NOT_RUN; no SIM or phone call exercised
Real integration evidence: NOT_RUN; no Sales API/auth/provider connected
Production evidence: NOT_RUN; no GitLab hosted MR/runner/protected settings/registry, staging, eSIM, or release proof
Residual blockers/risks: W-0061/G-GITLAB BLOCKED_EXTERNAL because remote remains GitHub-only and no GitLab platform access is available; CODEOWNERS paths are planned placeholders until Platform provisions/verifies groups and enforcement; 10 OpenAPI advisory warnings remain visible for P1-1 hardening
Next allowed Work ID(s): W-0061 should be closed next for hosted CI proof; W-0012/P0-3 is dependency-eligible from W-0010 and may proceed while W-0061 remains explicit
Final status: TESTS_PASS
```

```text
Work ID: W-0018 / P2-1
Baseline/commit: baseline main@addc423; implementation commit 85c2b63b6b386fcc7311a8c6c64385dacad5b31f; this handoff is finalized in a follow-up documentation commit
Scope completed: authenticated Target V1 intake; strict contract/schema and canonical idempotency; ordered official-order/matrix/policy/contact/speech/eligibility/script/mode gates; exact duplicate replay/conflict; MOCK dry-run; atomic PostgreSQL task/job/intake-outbox/audit/idempotency; immutable snapshots; retention order; canonical fake scenarios for both program paths
Files/artifacts: src/Ivr.Api/Intake/**; src/Ivr.Infrastructure/Intake/**; persistence entity/context/config/security/retention changes; migration 20260813111817_P2_1_TaskIntake; seed/sales-target-v1.sample.json; database specs; TaskIntake unit/integration/contract tests; docs/evidence/W-0018/README.md
Commands and exact results: locked restore PASS; Release build 0 warnings/0 errors; format PASS; contract 21 + unit 80 + integration 43 = 144/144; merged coverage 95.26% (18289/19200, 3 reports); EF no pending model changes; CI config/OpenAPI/docs/UI/NuGet/npm/Compose/PII PASS; official map 412 files/375 resolved/0 unresolved; implementation-commit Gitleaks history 28 commits/21.19 MB/no leaks
Tests/evidence: 13 focused service cases; 10 API cases; 2 PostgreSQL cases; 2 contract cases; 8 concurrent requests persist exactly task/job/outbox/idempotency/audit = 1/1/1/1/1; restricted task persists zero task/job/outbox; exact response/error and audit allowlist at docs/evidence/W-0018/README.md
Review/acceptance by: Codex self-review under explicit IVR owner authorization; status limited to TESTS_PASS until owner/reviewer accepts and external integrations are evidenced
Mock-only evidence: complete for both canonical Target V1 program/payment paths using fake Sales fixtures, MOCK-approved script and one-way test protector; REAL_CUSTOMER_CALL_ALLOWED=NO; outbox remains HELD_MOCK
Lab evidence: NOT_RUN; no physical SIM/eSIM, modem, destination call or customer interaction
Real integration evidence: NOT_RUN; no Sales endpoint/service auth/CDC, production key provider or carrier invoked
Production evidence: NOT_RUN; no production script/privacy approval, key management, deployment or customer call
Remote handoff: GitHub main fast-forwarded to 85c2b63 and remote ref verified exact; GitLab origin/main remains 5544395 because protected main rejects direct push; no MR was created
Residual blockers/risks: P2-2 owns eligibility/blocker orchestration; Sales API/auth/data and real opaque protector are still required; LAB/PROD script approval and SIM/eSIM configuration remain open; protected GitLab main rejects direct push under the owner-mandated single-main workflow
GitNexus review: pre-edit mapper/DI LOW, ConfirmationTaskEntity HIGH (44 symbols), IvrDbContext CRITICAL (18 symbols/8 flows); final staged detect-changes CRITICAL with 32 files, 206 indexed symbols and 57 intake/persistence flows; full API/PostgreSQL/regression proof is required and passed
Next allowed Work ID(s): W-0019/P2-2 eligibility/blockers/fail-closed is the recommended next implementation
Final status: TESTS_PASS
```

```text
Work ID: W-0024 / P2-7
Baseline/commit: main@458d0af (P1-5); implementation commit e911fc1; no branch/MR by explicit IVR owner instruction
Scope completed: immutable DRAFT/IN_REVIEW/APPROVED/RETIRED script lifecycle; explicit RBAC, actor/reason/audit and four-eyes; IScriptRegistry exact mode gate; Target V1 whitelist; deterministic Vietnamese preview with one/many/collapse/VND/short area/1-0; PUBLIC-SAFE snapshot and hashes; in-memory MOCK registry; PostgreSQL registry, migration, mode seed and database lifecycle/append-only guards
Files/artifacts: src/Ivr.Domain/Scripts/**; src/Ivr.Infrastructure/Scripts/**; IvrDbContext/model/migration; seed/ivr-menu.sample.json; API/Worker safe config; tests/Ivr.UnitTests/Scripts/**; tests/Ivr.IntegrationTests/Scripts/**; specs functional/data/database/UI; docs/evidence/W-0024/**; official Markdown map
Commands and exact results: locked restore PASS; Release build 0 warning/0 error; format PASS; EF no pending model changes; focused UT-SCRIPT 10/10 and PostgreSQL script/migration 3/3; full contract 19 + unit 67 + integration 31 = 117/117; merged coverage 94.71% (14435/15241, 3 reports); CI config, OpenAPI lint/parse/schema/hash/negative, API docs, UI lint/build, NuGet/npm HIGH, Compose, Gitleaks and locale-stable PII PASS; official map 411 files/375 resolved/0 unresolved
Tests/evidence: UT-SCRIPT-SEED-01/LIFECYCLE-02/PROD-GATE-03/TEMPLATE-GUARD-04/INPUT-GUARD-05/RENDER-GOLDEN-06/RENDER-COLLAPSE-07 and IT-SCRIPT-SEED-07/PERSISTENCE-08 plus IT-DB-MIGRATE-01 PASS; synthetic fixture, exact Vietnamese golden preview and privacy report at docs/evidence/W-0024/
Review/acceptance by: Codex self-review under explicit IVR owner authorization; final status limited to TESTS_PASS because Product + Privacy/Legal owner decision and LAB/production evidence remain open
Mock-only evidence: complete; SCRIPT-ORDER-CONFIRM:v1-test-approved has MOCK_TEST only; fake data does not close W-0003
Lab evidence: NOT_RUN; no physical SIM/eSIM, modem, vendor TTS/audio, allowlisted destination or real call
Real integration evidence: NOT_RUN; no Sales endpoint/auth/provider or customer data connected
Production evidence: NOT_RUN; Content + Privacy/Legal approval not seeded; ProductionTargetV1FieldsApproved=NO; REAL_CUSTOMER_CALL_ALLOWED=NO; no deployment
Remote handoff: GitHub main fast-forwarded to e911fc1; GitLab origin/main push is BLOCKED_EXTERNAL because protected main disallows direct push; no MR was created
Residual blockers/risks: OD-V1-15 is OWNER_DECISION_REQUIRED for reading items/short area in PROD; W-0003 external Sales contract/data remains open; recording OFF; no notification/SMS/A-B behavior; hosted pipeline intentionally skipped under current main-only/no-MR workflow
GitNexus review: pre-edit IvrDbContext CRITICAL (18 symbols/8 flows), ShortDeliveryArea LOW (10 symbols/2 flows), model/DI LOW; unstaged tracked review HIGH with 21 files/37 symbols/6 existing flows and new unindexed symbols as graph lower bound; full PostgreSQL and 117-test proof covers shared-context blast radius; final staged detect recorded immediately before commit
Next allowed Work ID(s): W-0018/P2-1 task intake is now dependency-eligible and is the recommended next implementation; it must fail closed with IVR_SCRIPT_NOT_APPROVED when this registry does not resolve exact template/version/mode
Final status: TESTS_PASS
```

```text
Work ID: W-0064 / P1-5
Baseline/commit: main@5544395; dedicated P1-5 commit created after this record
Scope completed: Domain retention port/report; config-backed nine-class policy; fail-closed NOT_CONFIGURED behavior; DELETE/ANONYMIZE catalog; legal hold; accepted-evidence/audit protection; child-first short PostgreSQL batches; resumable checkpoints; privacy-safe audit/metrics/age alert; one-pass Worker host; EF migration and rollback-safe snapshot trigger; full DB-05 matrix
Files/artifacts: src/Ivr.Domain/Retention/**; src/Ivr.Infrastructure/Retention/**; src/Ivr.Infrastructure/Persistence/**; src/Ivr.Worker/Jobs/RetentionJobHost.cs and safe appsettings; tests/Ivr.UnitTests/RetentionPolicyTests.cs; tests/Ivr.IntegrationTests/Retention/**; specs/database/05-retention-and-privacy.md; docs/evidence/W-0064/**
Commands and exact results: locked restore PASS; Release build 0 warning/0 error; format PASS; EF no pending model changes; migration apply/rollback/recreate PASS with 18 IVR tables; focused unit 1/1 + PostgreSQL integration 6/6; full contract 19 + unit 57 + integration 29 = 105/105; merged coverage 93.70% (10887/11619); CI config, OpenAPI lint/parse/schema/hash/negative, docs portal tests, UI lint/build, both npm audits, NuGet High, Compose, Gitleaks and locale-stable PII PASS
Tests/evidence: UT-RET-CONFIG-01 and IT-RET-DRYRUN-02/DELETE-03/HOLD-04/AUDIT-05/RESUME-06/PII-07 all PASS; sanitized dry-run/real-run aggregate reports and matrix at docs/evidence/W-0064/
Review/acceptance by: Codex self-review under explicit IVR owner authorization; status limited to TESTS_PASS until owner/reviewer accepts and Legal/Privacy closes DF-07 periods
Mock-only evidence: complete for P1-5 behavior using config test periods and disposable real PostgreSQL; repository defaults remain host disabled, DryRun=true and PeriodDays empty
Lab evidence: NOT_RUN; no physical SIM/eSIM, modem, destination call or customer interaction
Real integration evidence: NOT_RUN; no Sales endpoint/auth/provider invoked and no production data processed
Production evidence: NOT_RUN; no production period, schedule, deployment or legal approval; REAL_CUSTOMER_CALL_ALLOWED=NO
Residual blockers/risks: DF-07/OD-V1-11 is OWNER_DECISION_REQUIRED for all production periods; audit/accepted evidence policy changes require separate decision; P7-2 owns CronJob schedule; hosted CI/MR evidence intentionally handled outside this local prompt
GitNexus review: staged HIGH breadth with 31 files, 33 indexed symbols and 7 existing persistence flows; pre-edit impact was CRITICAL for RetainedEntity/IvrDbContext and MEDIUM for model/evidence configuration, so changes stayed additive and full PostgreSQL/regression proof is authoritative; untracked new retention symbols are a graph lower bound
Next allowed Work ID(s): W-0024/P2-7 remains the recommended next implementation; W-0066/P2-9 is dependency-eligible after W-0064 and may follow the planned order
Final status: TESTS_PASS
```

```text
Work ID: W-0012
Prompt: P0-3
Baseline/commit: baseline main@0c2f692 (dedicated P0-2 commit); this record is included in the dedicated P0-3 commit
Scope completed: fail-fast configuration; correlation context/middleware/outbound propagation; exact 15-code error catalog/envelope; seven-permission RBAC with MOCK-only header adapter and non-MOCK fail-close; Order Core source/token allowlist; in-memory MOCK idempotency, append-only audit and evidence registries; admin reason guard; PII mask/guard; DI and ordered API pipeline
Files/artifacts: src/Ivr.Api/{Auth,Foundation,Middleware}/; src/Ivr.Domain/{Errors,Privacy}/; src/Ivr.Infrastructure/{Audit,Configuration,Correlation,Evidence,Idempotency}/; tests/Ivr.UnitTests/CrossCuttingFoundationTests.cs; tests/Ivr.IntegrationTests/{CrossCuttingFoundationTests,FoundationApiTestApplication}.cs; README.md; docs/evidence/W-0012/README.md
Commands and exact results: locked restore PASS; Release build 0 warnings/0 errors; 14/14 implemented tests; merged coverage 91.99% (563/612, 3 reports) >= 60%; format PASS; UI lint/build + two npm audits PASS; CI config CT-CI-05/07/08 PASS; OpenAPI parse/schema/negative PASS with 10 advisory warnings and 0 errors; NuGet High/Critical PASS; Compose PASS; Gitleaks no leaks; PII selftest/current scan PASS; official Markdown map 392 files/369 resolved/0 unresolved; GitNexus staged CRITICAL breadth (56 files/228 symbols/25 expected new flows), 0 circular import
Tests/evidence: all eight required IDs UT-FND-IDEMP-01/CORR-02/RBAC-03/RBAC-08/ALLOW-04/ERR-05/AUDIT-06/PII-07 PASS; extra UT-FND-CONFIG-09, UT-FND-EVID-10 and UT-FND-ERRCAT-11 PASS; normalized safe 403/409/500 samples and detailed proof at docs/evidence/W-0012/README.md
Review/acceptance by: Codex self-review under explicit IVR owner authorization; final status limited to TESTS_PASS because hosted GitLab execution required by DoD is unavailable
Mock-only evidence: complete for P0-3 local implementation; IVR_EXECUTION_MODE=MOCK; SALES_PROVIDER=FAKE_TARGET_V1; SIM_PROVIDER=MOCK; REAL_CUSTOMER_CALL_ALLOWED=NO; no customer call path
Lab evidence: NOT_RUN; no physical SIM/device/provider exercised
Real integration evidence: NOT_RUN; Order Core route uses an isolated test host/fake token only; no Sales API/auth connected
Production evidence: NOT_RUN; non-MOCK authentication deliberately fails closed; no GitLab hosted pipeline, persistent foundation tables, staging, eSIM, or release proof
Residual blockers/risks: W-0061/G-GITLAB remains BLOCKED_EXTERNAL; P1-2 owns PostgreSQL mappings/migrations; P4-4 owns production JWT/service-auth federation; P7 owns production secret-store selection; 10 pre-existing OpenAPI advisory warnings remain for P1-1
Next allowed Work ID(s): W-0013/P0-4 is the recommended next local implementation; W-0061 should continue in parallel; W-0015/P1-2 is also dependency-eligible after this local foundation
Final status: TESTS_PASS
```

```text
Work ID: W-0013
Prompt: P0-4
Baseline/commit: baseline main@1c08cf0 (dedicated P0-3 commit); this record is included in the dedicated P0-4 commit
Scope completed: canonical typed feature flags/dynamic config; safe seeds/cache/refresh; fresh-read kill and centralized dispatch gate; audited atomic MOCK store; PostgreSQL read adapter and EF model; asymmetric admin mutation with explicit permissions, actor binding, reason, idempotency, four-eyes and self-authorization guard; API/Worker DI; OpenAPI
Files/artifacts: src/Ivr.Infrastructure/FeatureFlags/**; src/Ivr.Api/Admin/FeatureFlagEndpoint.cs; src/Ivr.Api/Auth/**; src/Ivr.Infrastructure/Persistence/IvrDbContext.cs; src/Ivr.Worker/Program.cs; tests/Ivr.UnitTests/FeatureFlagPlatformTests.cs; tests/Ivr.IntegrationTests/FeatureFlagApi*.cs; specs/api/{03-admin-api.md,openapi/ivr-order-confirmation.v1.yaml}; docs/evidence/W-0013/README.md
Commands and exact results: locked restore PASS; Release build/analyzers 0 warnings/0 errors; format PASS; full tests 27/27 PASS and repeated five consecutive times after correlation remediation; merged coverage 87.50% (1183/1352, 3 reports) >= 60%; UI lint/build and npm audit PASS; CI config CT-CI-05/07/08 PASS; OpenAPI parse/schema/negative PASS with 10 advisory warnings/0 errors; NuGet High/Critical, two npm audits, Compose, Gitleaks and PII gates PASS
Tests/evidence: all required UT-FLAG-DEFAULT-01/GUARD-02/AUDIT-03/AUTHZ-05/ALLOWLIST-06 and IT-FLAG-PRODGUARD-07/EMERGENCY-10/KILLSWITCH-08/FAILCLOSED-09/KILL-04 PASS; extra UT-FLAG-MODEL-11, IT-FLAG-OWNERGATE-12 and IT-FLAG-IDEMP-13 PASS; details docs/evidence/W-0013/README.md
Review/acceptance by: Codex self-review under explicit IVR owner authorization; local status limited to TESTS_PASS because OD-V1-20, P1-2 persistence and hosted GitLab evidence are open
Mock-only evidence: complete for P0-4 local implementation; process mode MOCK, fake Sales, mock SIM, real-customer permission NO; approved permission/four-eyes providers exist only as test-scoped fakes
Lab evidence: NOT_RUN; no physical SIM, vendor adapter or destination call exercised
Real integration evidence: NOT_RUN; no Sales API/auth/provider connected
Production evidence: NOT_RUN; runtime admin authorization defaults denied; persistent mutation and production release gate fail closed
Residual blockers/risks: OD-V1-20 owner approval pending; P1-2 owns physical migration and persistent atomic command transaction; W-0061 hosted GitLab remains BLOCKED_EXTERNAL; production identity/approval provider later; 10 pre-existing OpenAPI advisory warnings remain for P1-1
GitNexus review: refreshed 37,400 nodes/38,474 edges/62 flows; staged CRITICAL breadth (42 files/279 symbols/49 expected platform flows, including two generated index-count metadata files); focused maximum MEDIUM for the in-memory store with 11 test dependants, remaining platform entrypoints LOW; 0 cycle
Next allowed Work ID(s): W-0014/P1-1 is the recommended next prompt; W-0015/P1-2 is also dependency-eligible and closes P0-4 persistence; W-0061 continues in parallel
Final status: TESTS_PASS
```

```text
Work ID: W-0014 / P1-1
Baseline/commit: main@c78a407466e0f49847c83e0cea665582b80f6b1a; dedicated P1-1 commit created after this record
Scope completed: pinned/deterministic OpenAPI codegen; generated IVR server DTO and Target Sales client; isolated current Golden Hour DTO/client from verified Sales SHA; typed provider/mode guard; fake Sales mappings; drift/report CI gates; contract tests and evidence
Files/artifacts: dotnet-tools.json; specs/api/openapi/contract-manifest.json; specs/api/compat/current-golden-hour-callback.a3aad246.schema.json; src/Ivr.Contracts/Generated/**; src/Ivr.Contracts/Sales/**; tests/Ivr.ContractTests/**; docs/contracts/**; docs/evidence/W-0014/README.md
Commands and exact results: locked restore PASS; NSwag 14.7.1 regeneration stable; Release build/analyzers 0 warnings/0 errors; format 0/108; full tests 55/55; merged coverage 75.57% (1404/1858); UI lint/build and both npm audits PASS; CI config CT-CI-05/07/08/codegen PASS; OpenAPI lint 0 warnings, parse/schema/hash/report/negative PASS; NuGet High/Critical, Compose, Gitleaks and PII PASS
Tests/evidence: contract 19/19, unit 22/22, integration 14/14; target all four semantic 200 ACKs and typed 409/422/429/500/503 errors; exact two program/payment rows; 10 negative schema cases; target/current type separation; current exact route/header/DTO; details docs/evidence/W-0014/README.md
Review/acceptance by: Codex self-review under explicit IVR owner authorization; local status limited to TESTS_PASS because Target V1 and hosted GitLab gates remain externally open
Mock-only evidence: complete for scaffold, fake Sales and startup selection; IVR_EXECUTION_MODE=MOCK, SALES_PROVIDER=FAKE_TARGET_V1, SIM_PROVIDER=MOCK, REAL_CUSTOMER_CALL_ALLOWED=NO
Lab evidence: NOT_RUN; no physical SIM/vendor adapter/destination call
Real integration evidence: NOT_RUN; Sales source was read at pinned SHA but no API/auth/sandbox was invoked
Production evidence: NOT_RUN; no production URL/credential/provider/customer call; current compatibility remains runtime-disabled
Residual blockers/risks: TARGET_CONTRACT_V1=DRAFT; W-0002/W-0005/W-0006 Sales contract/auth/CDC and W-0061 hosted GitLab remain BLOCKED_EXTERNAL; P1-2 owns PostgreSQL migrations and persistent P0-4 transaction
GitNexus review: refreshed 37,854 nodes/39,085 edges/66 flows; staged MEDIUM with 42 files/452 symbols/4 generated Target-client flows; focused contract clients/selector/validator LOW, integration fixture MEDIUM with five test callers only; circular-import check 0
Next allowed Work ID(s): W-0015/P1-2 starts next and closes physical persistence assigned by P0-4; W-0061 continues in parallel
Final status: TESTS_PASS
```

```text
Work ID: W-0015 / P1-2
Baseline/commit: main@5d2301ecb9d70702924c817d2cb99859325a6b4e; dedicated P1-2 commit created after this record
Scope completed: PostgreSQL/EF Core 17-table persistence; versioned policy/speech snapshots; invariant constraints/triggers; opaque protected dial-token hook; callback outbox immutable payload + SKIP LOCKED lease; config-driven SIM channel lease/fencing/health/quarantine; persistent foundation idempotency/audit/evidence; atomic PostgreSQL feature-flag/audit/idempotency transaction; GitLab DIND Testcontainers gate
Files/artifacts: src/Ivr.Infrastructure/{Persistence,Idempotency,Audit,Evidence,FeatureFlags}/**; tests/Ivr.IntegrationTests/PostgresPersistenceTests.cs; dotnet-tools.json; deploy/ci/**; specs/database/06-migration-plan.md; docs/evidence/W-0015/{README.md,migration-up.sql,migration-down.sql}
Commands and exact results: locked restore PASS; dotnet-ef 10.0.11 tool restore/list and no-pending-model PASS; Release build/analyzers 0 warnings/0 errors; format PASS; PostgreSQL suite 6/6; full tests 61/61; merged coverage 90.64% (6765/7464); UI lint/build and npm audit PASS; CI config including TESTCONTAINERS_DIND PASS; OpenAPI lint 0 warnings, parse/schema/hash/negative PASS; NuGet High/Critical, Compose, Gitleaks and locale-stable PII PASS; official map 397 file/369 resolved/0 unresolved
Tests/evidence: IT-DB-MIGRATE-01, TASK-02, ATTEMPT-03, FLAG-04, LEASE-05 and OUTBOX-06 all PASS on postgres:16-alpine; migration Up 17 tables/94 indexes/6 triggers and Down 17 drops; zero forbidden candidate constraints; details docs/evidence/W-0015/README.md
Review/acceptance by: Codex self-review under explicit IVR owner authorization; local status limited to TESTS_PASS because hosted GitLab/runtime/production gates remain open
Mock-only evidence: complete for local application paths; Testcontainers exercises a disposable real PostgreSQL engine only; no Sales/SIM/customer egress
Lab evidence: NOT_RUN; no physical SIM/eSIM/vendor adapter/destination call
Real integration evidence: NOT_RUN; no Sales endpoint/auth/CDC invoked
Production evidence: NOT_RUN; no KMS/key rotation, retention purge, backup/restore drill, staging migration, protected GitLab pipeline or production rollback
Residual blockers/risks: W-0061/G-GITLAB BLOCKED_EXTERNAL and runner must support privileged DIND; DF-07 retention/legal hold unresolved; production protector/KMS and backup/staging/release approval open; TARGET_CONTRACT_V1 remains DRAFT; migration Down is destructive total data loss and test-only
GitNexus review: staged MEDIUM with 40 files, 35 indexed symbols and four existing feature-flag read flows; zero circular imports; new persistence symbols are not yet indexed, so graph counts are lower-bound and direct source/PostgreSQL/full regression review is authoritative
Next allowed Work ID(s): W-0016/P1-3 is the recommended next implementation; W-0017/P1-4 may follow/parallelize after scope review; W-0061 continues with Platform
Final status: TESTS_PASS
```

```text
Work ID: W-0017 / P1-4
Baseline/commit: main@a94b858 (P1-3 plus GitLab cache-key remediation); dedicated P1-4 commit created after this record
Scope completed: deterministic Redoc developer portal; 11 committed render artifacts; Target/current boundary page; integration/versioning/changelog guides; source-hash manifest; pinned oasdiff changelog and breaking gate; root-included GitLab verification/diff/fail-closed Pages jobs
Files/artifacts: docs/api/**; docs/api-changelog.md; docs/api-versioning.md; docs/integration-guide.md; specs/api/openapi/baselines/**; specs/api/openapi/changelog-baseline.json; deploy/ci/docs.gitlab-ci.yml; deploy/ci/scripts/{build-api-docs.mjs,docs-selftest.mjs,generate-oasdiff-changelog.sh,selftest-oasdiff.sh}; docs/evidence/W-0017/**
Commands and exact results: docs render 11; CT-DOC-01/02 and UT-DOC-PII-03 PASS; Target/current boundary, local links and GitLab docs topology PASS; oasdiff v1.26.1 two initial baselines no change; OpenAPI lint/parse/schema/hash/negative PASS; locked restore/format/build PASS 0 warning/0 error; contract 19/19, unit 54/54, integration 23/23 = 96/96; UI lint/build; both npm audits 0; NuGet High policy, Gitleaks, PII and Compose PASS; official map 405 file/372 resolved/0 unresolved
Tests/evidence: generated drift fail demo; removed-operation breaking fixture; privacy-safe source examples; rendered portal screenshot and exact results at docs/evidence/W-0017/README.md
Review/acceptance by: Codex self-review under explicit IVR owner authorization; ACCEPTED after hosted GitLab Pages, runner and access-control evidence passed
Mock-only evidence: complete for P1-4; portal is marked NON-PRODUCTION ONLY; Target contracts remain DRAFT; examples use masked/synthetic values
Lab evidence: NOT_RUN; no physical SIM/eSIM, device or destination call
Real integration evidence: NOT_RUN; no Sales endpoint/auth/CDC/provider invoked
Production evidence: NOT_RUN; protected `API_DOCS_PUBLISH_NONPROD=YES` publishes only the private non-production portal and no production portal is created
Hosted evidence: protected-main pipeline `#2756517379` PASS 12 jobs/98 tests; Pages job `15873355825` generated 11 portal artifacts, uploaded root `public/` with HTTP 201 and deployed `https://ginsengfood-ivr-0332fa.gitlab.io/`; anonymous access redirects 302 to GitLab auth
Residual blockers/risks: Sales contract approval remains external; W-0061 remains independently BLOCKED_EXTERNAL only for required independent MR approval enforcement on GitLab Premium/Ultimate with a second reviewer
GitNexus review: staged LOW with 33 files, 10 indexed documentation symbols and 0 affected IVR execution processes; direct generator/CI source and deterministic self-tests cover new unindexed files
Next allowed Work ID(s): W-0024/P2-7 is required before W-0018/P2-1 and is the recommended next local implementation; W-0061 continues in parallel
Final status: ACCEPTED
```

```text
Work ID: W-0085
Prompt: unplanned hosted-CI remediation after P1-4
Baseline/commit: main@2b1a4d4; dedicated fix commit created after this record
Scope completed: make exact source-project dependency guard portable across Windows and Linux MSBuild path separators; add explicit two-separator regression
Files/artifacts: tests/Ivr.UnitTests/ArchitectureDependencyTests.cs; docs/evidence/W-0085/README.md; prompt/_execution/prompt-execution-tracker.md
Commands and exact results: Windows focused 3/3; clean Linux SDK focused 3/3 and full unit 56/56; locked restore; Release build 0 warning/0 error; full local contract 19 + unit 56 + integration 23 = 98/98; format and CI config PASS
Tests/evidence: hosted failure job 15870797229 at 2b1a4d4 plus disposable Linux reproduction/fix proof in docs/evidence/W-0085/README.md
Review/acceptance by: Codex self-review; status limited to TESTS_PASS until hosted pipeline rerun is green
Mock-only evidence: N/A to runtime behavior; this changes only a static architecture test
Lab evidence: NOT_RUN; no SIM/device/customer call
Real integration evidence: NOT_RUN; no Sales/provider endpoint invoked
Production evidence: NOT_RUN; no deployment
Residual blockers/risks: W-0061 remains BLOCKED_EXTERNAL; new push must prove hosted Linux job and entire pipeline green, then branch/settings evidence remains
Next allowed Work ID(s): verify the new GitLab pipeline, then W-0024/P2-7
Final status: TESTS_PASS
```

```text
Work ID: W-0019 / P2-2
Baseline/commit: baseline main@8751d3f; dedicated implementation commit pending final change review; no branch/MR by explicit IVR owner instruction
Scope completed: ordered stored-snapshot eligibility gate; official/state/matrix reassert; per-line sellable/blocker fail-closed; PHONE_CALL restriction separated from SMS opt-out; contact/token/window; late capacity check; trust-skip hard-off; eligible/block/hold/capacity states; atomic task/job/outbox/reason/evidence/audit/capacity persistence; MOCK no-egress
Files/artifacts: src/Ivr.Domain/Policies/EligibilityRules.cs; src/Ivr.Api/Application/EligibilityService.cs; src/Ivr.Infrastructure/Repositories/EligibilityRepository.cs; intake/DI wiring; unit/integration tests; docs/evidence/W-0019/README.md; official Markdown map
Commands and exact results: locked restore PASS; Release analyzer build 0 warning/0 error; format PASS; contract 21 + unit 84 + integration 47 = 152/152; merged coverage 94.71% (18870/19925, 3 reports); EF no pending model; CI config/OpenAPI/docs/UI/NuGet/npm/Compose/Gitleaks/PII PASS; official map 413 files/375 resolved/0 unresolved
Tests/evidence: UT-ELIG-BLOCK-01/DNC-02/TRUST-03/FAILCLOSED-04 PASS; IT-ELIG-CAP-05 creates held incident with zero attempt; IT-ELIG-MOCK-06 proves eligible MOCK remains DRY_RUN/HELD_MOCK with no outbox publish/attempt; IT-ELIG-DNC-07 proves stored restriction blocks before capacity; IT-ELIG-FAILCLOSED-08 proves missing capacity evidence holds; stable reasons and signal evidence at docs/evidence/W-0019/
Review/acceptance by: Codex self-review under explicit IVR owner authorization; status limited to TESTS_PASS until owner/reviewer accepts and external data/LAB/production evidence exists
Mock-only evidence: complete; fake capacity provider and synthetic snapshots only; REAL_CUSTOMER_CALL_ALLOWED=NO; no real adapter, destination or customer call
Lab evidence: NOT_RUN; no physical SIM/eSIM, modem, destination allowlist or carrier path
Real integration evidence: NOT_RUN; no Sales/Order Core endpoint/auth/CDC and no direct Ops/CRM endpoint invoked
Production evidence: NOT_RUN; default non-MOCK capacity fails closed; no production capacity provider, scheduler, trust resolver, deployment or approval
Residual blockers/risks: P2-3 owns real scheduler/channel capacity and must replace the fail-closed non-MOCK provider; Sales Target V1/auth/data, LAB/PROD script/key/SIM and trust resolver remain external/open; protected GitLab main may reject direct push under owner-mandated single-main workflow
GitNexus review: pre-edit P2-1 Accepted HIGH (13 symbols/3 flows), InMemoryTaskIntakeStore MEDIUM (38 lower-bound consumers), EligibilityRules Evaluate HIGH (8 symbols/1 flow), foundation DI LOW; final staged detect-changes CRITICAL with 16 files/111 symbols/38 intake-persistence flows; cycle check shows only the pre-existing RuntimeGateDefaults↔PersistenceModelConfiguration cycle outside staged scope
Next allowed Work ID(s): W-0020/P2-3 policy registry, scheduler and channel leases is the recommended next implementation
Final status: TESTS_PASS
```
