# IVR Master Implementation Progress Ledger

Trạng thái: `ACTIVE_SINGLE_SOURCE` · Cập nhật: `2026-08-12`
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
| `NEXT_WORK_ID` | `W-0078` |
| Last allocated | `W-0077` |
| Last activity sequence | `A-0036` |
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
| `G-GITLAB` | GitLab project/runner/registry/protected-branch (TV1-12) | Platform/Infra | BLOCKED_EXTERNAL | local YAML/rules render only | GitLab project URL + remote verify + runner identity + hosted MR pipeline + protected-branch export + registry push/pull |
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
| `W-0061` | **GitLab platform provisioning** (TV1-12) | Platform/Infra | BLOCKED_EXTERNAL | tạo/mirror GitLab project + xác nhận remote; GitLab Runner + tags/capabilities; Container Registry; protected default branch; MR approvals; "Pipelines must succeed"; masked/protected CI/CD variables | local rules render + `gitlab-ci-local` (renderer, KHÔNG phải hosted proof) | gửi request Platform; P0-2 hosted evidence giữ `NOT_RUN` tới khi đóng |
| `W-0063` | **Platform infrastructure dependencies** | Platform/Infra | BLOCKED_EXTERNAL | container registry; K8s cluster + credentials 4 env; secret store (Vault/KMS); observability backend (Tempo/Jaeger + Prometheus + Loki hoặc APM); Grafana/Alertmanager; Argo Rollouts/Flagger; analytics warehouse; visual-regression service | docker-compose local stack | gom 8 mục `NEED_CONFIRMATION` trong P5-5/P6-1/P6-2/P7-1/P7-2/P7-4/P7-5/P10-4 |

## 5. Planned implementation register

Every row is planned work. Detailed build/test/evidence requirements live in the linked prompt and specs; actual results must be written back here.

`Origin` mặc định là `PLANNED`. Việc phát sinh ghi `UNPLANNED` hoặc `RED_TEAM_REMEDIATION` trong cột `Scope summary`.

| Work ID | Prompt | Scope summary | Prereq | Status | Owner | Artifacts/MR | Tests/evidence | Residual/next |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `W-0001` | Planning realignment | Target V1 plan/spec/prompt/tracker/OpenAPI | docs/code review | EVIDENCE_SUBMITTED | Codex + IVR owner | Target V1 draft, two OpenAPI files, 51-prompt register, fake seed | JSON/YAML/schema/ref/link/tracker/diff checks pass | technical defaults confirmed; owner may accept evidence separately |
| `W-0010` | `P0-1` | repo/solution bootstrap | technical defaults confirmed 2026-08-12; baseline frozen at `5c6f39e` | ACCEPTED | Codex (explicit IVR owner authorization) | `Ivr.sln`; `src/**`; `tests/**`; `admin-ui/**`; `docker-compose.dev.yml`; `README.md`; `docs/evidence/W-0010/` | .NET build 0 warning/0 error; 3/3 test pass; format 0/39; UI lint/build pass; Postgres healthy; probes 3/3; browser clean + screenshot; doc links 0 unresolved; GitNexus LOW/0 process/0 cycle | P0-1 closed; MOCK only; GitLab CI next at W-0011; real Sales/SIM/lab/production remain NOT_RUN and outside this acceptance |
| `W-0011` | `P0-2` | GitLab CI/quality baseline | W-0010 | NOT_STARTED |  |  |  |  |
| `W-0012` | `P0-3` | config/auth/audit/idempotency/correlation | W-0010 | NOT_STARTED |  |  |  |  |
| `W-0013` | `P0-4` | mode/provider flags + kill switches | W-0012 | NOT_STARTED |  |  |  |  |
| `W-0014` | `P1-1` | both OpenAPI/codegen/contract scaffold | W-0010..12 | NOT_STARTED |  |  |  |  |
| `W-0015` | `P1-2` | PostgreSQL/EF migrations, versioned policy/speech snapshots | W-0012 | NOT_STARTED |  |  |  |  |
| `W-0016` | `P1-3` | domain/DTO/provider ports/privacy guards | W-0014,W-0015 | NOT_STARTED |  |  |  |  |
| `W-0017` | `P1-4` | API docs/versioning/drift portal | W-0014 | NOT_STARTED |  |  |  |  |
| `W-0018` | `P2-1` | task intake for both program/payment paths | W-0014..16,W-0024 | NOT_STARTED |  |  |  |  |
| `W-0019` | `P2-2` | eligibility/blockers/fail-closed | W-0018 | NOT_STARTED |  |  |  |  |
| `W-0020` | `P2-3` | policy registry/scheduler/channel leases | W-0019 | NOT_STARTED |  |  |  |  |
| `W-0021` | `P2-4` | speech + dial-token + mock SIM adapter | W-0020 | NOT_STARTED |  |  |  |  |
| `W-0022` | `P2-5` | DTMF/disposition normalizer | W-0021 | NOT_STARTED |  |  |  |  |
| `W-0023` | `P2-6` | target callback/outbox + GH compat | W-0022 | NOT_STARTED |  |  |  |  |
| `W-0024` | `P2-7` | script/content approval and safe variables (chạy TRƯỚC P2-1) | W-0016 | NOT_STARTED |  |  |  |  |
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
| `W-0064` | `P1-5` | retention job + data lifecycle (`IRetentionJob`) | W-0015 | NOT_STARTED |  |  |  | prereq của W-0044/W-0051/W-0052/W-0053; retention period thật chờ DF-07/`OD-V1-11` |
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
