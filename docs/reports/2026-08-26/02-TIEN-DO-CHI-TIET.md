# Tiến độ chi tiết — đã làm những gì

**Ngày:** 2026-08-26 · **Baseline:** `main@bdde72c`
**Nguồn:** [`prompt/_execution/prompt-execution-tracker.md`](../../../prompt/_execution/prompt-execution-tracker.md)
(§2–§5) đối chiếu với source thật.

---

## 1. Cách đọc bảng trạng thái

| Trạng thái | Nghĩa chính xác |
| --- | --- |
| `ACCEPTED` | evidence **đã được owner/reviewer chấp nhận**. Chỉ 5 work item đạt mức này |
| `EVIDENCE_SUBMITTED` | đã nộp evidence pack, **chưa** được chấp nhận |
| `TESTS_PASS` | code xong, test xanh, gate CI local xanh — **chưa** nộp/chưa được ký |
| `BLOCKED_EXTERNAL` | chờ bên ngoài; **không** phải lỗi hay chậm trễ của đội IVR |
| `DEFERRED_TARGET` | cố ý hoãn theo quyết định, không phải lỗ hổng |

> **`TESTS_PASS` ≠ `ACCEPTED` ≠ production-ready.** Đây là ba mức khác nhau và governance
> (`MASTER-05`) cấm đánh đồng. Mock evidence, lab evidence và real evidence **không thay thế nhau**.

---

## 2. Toàn cảnh 12 phase

| Phase | Nội dung | Work ID | Trạng thái |
| --- | --- | --- | --- |
| **P0** Foundation | repo/solution · CI GitLab · cross-cutting (auth/audit/idempotency/correlation) · feature flag & kill switch | W-0010..W-0013 | 1 `ACCEPTED`, 3 `TESTS_PASS` |
| **P1** Contracts & Data | OpenAPI/codegen · migration PostgreSQL · domain/DTO mapping · portal tài liệu · retention job | W-0014..W-0017, W-0064 | 1 `ACCEPTED`, 4 `TESTS_PASS` |
| **P2** Core runtime | intake · eligibility · scheduler · SIM adapter mock · DTMF normalizer · callback · script content · internal/admin API · speech port | W-0018..W-0024, W-0065, W-0066 | 9 `TESTS_PASS` |
| **P3** Admin UI | foundation · dashboard/log/detail · config/integration/roles · reporting | W-0025..W-0028 + W-0095..W-0102 | 11 `TESTS_PASS`, 1 `EVIDENCE_SUBMITTED` |
| **P4** Integration | Sales wiring · sellable gate · CRM eligibility · shared auth · notification (hoãn) · opt-out (hoãn) | W-0029..W-0034 | 4 `TESTS_PASS`, 2 `DEFERRED_TARGET` |
| **P5** Quality | unit/integration · contract/E2E · perf/security · code-review gate · a11y/i18n | W-0035..W-0039 | 5 `TESTS_PASS` |
| **P6** Observability | telemetry · dashboard/SLO/alert · chaos/gameday | W-0040..W-0042 | 3 `TESTS_PASS` |
| **P7** Deployment | Docker/Compose · Helm/K8s · CI/CD · canary · secret rotation | W-0043..W-0047 | 5 `TESTS_PASS` |
| **P8** SIM pilot | adapter vendor thật + 1 SIM lab · runbook lab | W-0048, W-0049 | 2 `BLOCKED_EXTERNAL` |
| **P9** Release ops | release gate · cutover/hypercare | W-0050, W-0051 | 2 `BLOCKED_EXTERNAL` |
| **P10** Compliance | PDPA/DSAR · governance/backup/DR · capacity/cost · analytics/BI · SLA/on-call | W-0052..W-0056 | 4 `TESTS_PASS`, 1 `BLOCKED_EXTERNAL` |
| **P11** Production closure | telephony RFQ · Sales contract closure pack · legal/retention pack · readiness board | W-0057..W-0060 | 4 `EVIDENCE_SUBMITTED` |

---

## 3. Chi tiết từng phase

### P0 · Nền tảng (W-0010 → W-0013)

| Work | Đã giao gì |
| --- | --- |
| **W-0010** `ACCEPTED` | `Ivr.sln` 5 project + test project; `docker-compose.dev.yml`; README; port range riêng (`5005`/`3005`/`55433`) để chạy chung máy với `ginsengfood-ops-core` |
| **W-0011** `TESTS_PASS` | `.gitlab-ci.yml` + 13 file include; MR template; CODEOWNERS; lockfile; **pipeline hosted đã từng chạy PASS** (`#2756517379`, 12 job / 98 test / Pages) |
| **W-0012** `TESTS_PASS` | mock JWT · PII guard/masker · idempotency store · correlation context xuyên suốt · error envelope ổn định · audit append-only · evidence store · allowlist Order Core |
| **W-0013** `TESTS_PASS` | 3 mode `MOCK`/`LAB_REAL_SIM`/`PRODUCTION_REAL`; provider flag; **kill switch bất đối xứng theo chiều an toàn** (bật được lúc sự cố, không "immutable" khoá cứng); guardrail cấu hình; mutation cờ có audit + idempotency |

### P1 · Hợp đồng & dữ liệu (W-0014 → W-0017, W-0064)

| Work | Đã giao gì |
| --- | --- |
| **W-0014** `TESTS_PASS` | 2 OpenAPI (IVR server + Sales callback target); codegen NSwag ghim version; client Golden Hour compat ghim **riêng**; drift gate ghim SHA-256 |
| **W-0015** `TESTS_PASS` | EF model 17 bảng ban đầu + migration; Up/Down SQL; Testcontainers; outbox + channel lease/fencing; audit/idempotency/flag đều persistent |
| **W-0016** `TESTS_PASS` | domain immutable + value object; provider port + fake tất định; anti-corruption mapper target ↔ current; privacy guard |
| **W-0017** `ACCEPTED` | portal Redoc tĩnh; guide versioning/integration/changelog; oasdiff ghim; GitLab Pages riêng tư |
| **W-0064** `TESTS_PASS` | `IRetentionJob` + policy provider + target catalog + telemetry + host worker + entrypoint run-once |

### P2 · Lõi vận hành (W-0018 → W-0024, W-0065, W-0066)

Đây là phần "ruột" của hệ thống. Toàn bộ 9 work item đều `TESTS_PASS`.

| Work | Đã giao gì | Con số |
| --- | --- | --- |
| **W-0024** (`P2-7`, chạy **trước** intake) | vòng đời kịch bản + biến an toàn + migration/seed | 117/117 test |
| **W-0018** (`P2-1`) | intake cho cả hai program/payment; atomicity đồng thời trên PostgreSQL | 144/144, coverage 95,26% |
| **W-0019** (`P2-2`) | eligibility fail-closed; DNC; capacity; atomic task/job/outbox/audit/evidence | 152/152, coverage 94,71% |
| **W-0020** (`P2-3`) | policy registry có version + audit; deadline scheduler tất định; claim/lease/fencing/quarantine | — |
| **W-0021** (`P2-4`) | port SIM vendor-neutral; renderer tiếng Việt đã duyệt; vault fingerprint + expiry/replay/allowlist; dispatch adapter có fencing | 186/186, coverage 94,55% |
| **W-0022** (`P2-5`) | **một** disposition mapper duy nhất; ngữ nghĩa counted/final; đúng 1 retry kỹ thuật có giới hạn | 209/209, coverage 94,61% |
| **W-0023** (`P2-6`) | outbox callback nguyên tử; dispatcher Target V1; adapter GH compat **tách biệt**; retry/circuit/readiness | 33/33 focused |
| **W-0065** (`P2-8`) | 6 lifecycle service-only + 7 admin operation có permission; response typed/masked; idempotency; atomic action/audit | 13 operation |
| **W-0066** (`P2-9`) | port TTS + model; fake tất định; skeleton external; seam privacy/cache/retention | 281/281 sau remediation |

### P3 · Console vận hành (W-0025 → W-0028 + 8 work phát sinh)

Điểm đáng chú ý: **8 work item phát sinh ngoài kế hoạch** trong phase này, trong đó **4 là
`RED_TEAM_REMEDIATION`** — tức là do rà soát đối kháng phát hiện, không phải do đổi yêu cầu.

| Work | Loại | Đã giao gì |
| --- | --- | --- |
| **W-0095** | UNPLANNED | 3 read endpoint thiếu cho dashboard/call log/detail |
| **W-0025** (`P3-1`) | PLANNED | BFF shell · session · RBAC · i18n · error envelope · PII foundation |
| **W-0026** (`P3-2`) | PLANNED | dashboard/log/detail có mask; 83 test |
| **W-0096** | UNPLANNED | 3 read endpoint cho scripts/integration-status/review-items |
| **W-0027** (`P3-3`) | PLANNED | config/integration/review/seed/roles; 102 test |
| **W-0097** | UNPLANNED | pass thiết kế Minimalism/Swiss + token 2 theme + kiểm tương phản |
| **W-0098** | UNPLANNED | 4 read endpoint analytics |
| **W-0028** (`P3-4`) | PLANNED | báo cáo + biểu đồ xu hướng + breakdown + xuất CSV + banner độ tươi |
| **W-0099** | RED_TEAM | spec khai 2 action `Disable/Enable SIM` nhưng **không màn nào** có control — quyền đã seed mà vô dụng. Thêm `GET /sim-channels` + control |
| **W-0100** | RED_TEAM | 6 nhóm lỗi vệ sinh: guard drift **thiếu 10 endpoint**, test có assertion rỗng, dead code, filter không có control, action không báo thành công, doc drift |
| **W-0101** | RED_TEAM | rà theo **spec UI** (không theo prompt): 4 tile không có field phía sau; filter ngày không gửi được dù API nhận; `sellable_status[]` bị thu về mỗi timestamp |
| **W-0102** | UNPLANNED | chụp bằng chứng §10 của cả 4 prompt Phase 3 từ stack thật |

### P4 · Tích hợp (W-0029 → W-0034)

| Work | Trạng thái | Đã giao gì |
| --- | --- | --- |
| **W-0029** (`P4-1`) | `TESTS_PASS` | wiring Sales provider + CDC; card `ORDER_CORE` quan sát thật; 337/337 |
| **W-0030** (`P4-2`) | `TESTS_PASS` | schema `eligibility-snapshot.v1` + hash bằng chứng + migration; 315/315 |
| **W-0031** (`P4-3`) | `TESTS_PASS` | `VoiceContactEvidence` + `TrustResolverEvidence` (do-not-call + trust); 324/324 |
| **W-0032** (`P4-4`) | `TESTS_PASS` | `ServiceIdentity` + allowlist + audit federation; 336/336 |
| **W-0033** (`P4-5`) | `DEFERRED_TARGET` | **chứng minh IVR không gửi notification** — đây là mục tiêu, không phải thiếu sót |
| **W-0034** (`P4-6`) | `DEFERRED_TARGET` | opt-out feedback loop: chỉ đề xuất cho người duyệt, **không tự đổi consent** |

### P5 · Chất lượng (W-0035 → W-0039)

| Work | Đã giao gì |
| --- | --- |
| **W-0035** | `IT-FAILGATE-01..08` — 8 fail-gate; sinh bảng truy vết test tự động; nâng ngưỡng coverage CI 60% → **80%** |
| **W-0036** | lane CI contract/E2E riêng; 2 luồng E2E đầy đủ; `DUPLICATE_ACCEPTED` |
| **W-0037** | perf/security/privacy/mode-isolation: capacity 1 kênh/8 job và 4 kênh/24 job; `PT-FAILCLOSED-03`; `SEC-PII-04` |
| **W-0038** | quality gate + review checklist + reviewer guide + kiểm truy vết MR |
| **W-0039** | a11y/i18n/visual QA; lane CI riêng |

### P6 · Quan sát (W-0040 → W-0042)

| Work | Đã giao gì |
| --- | --- |
| **W-0040** | telemetry redacted + tracing + metric; **`/health/ready` fail-closed thật** (503 khi DB không tới được / schema lệch / mạch callback mở) |
| **W-0041** | dashboard 7 panel; 5 alert rule + 3 file promtool test; `docs/slo.md` với `runbook_url` trỏ đúng mục |
| **W-0042** | project chaos mới: Toxiproxy + 5 scenario + blast-radius guard + gameday report |

### P7 · Triển khai (W-0043 → W-0047)

| Work | Đã giao gì |
| --- | --- |
| **W-0043** | 6 Dockerfile + `.dockerignore` + compose mở rộng (api/worker/ui/migrate/fake-sales/otel) |
| **W-0044** | Helm chart đầy đủ: 3 deployment, service/SA, HPA+PDB, **3 NetworkPolicy**, migrate hook, retention CronJob; values 4 môi trường |
| **W-0045** | publish + scan + digest; `deploy_dev`/`deploy_staging`; `promote_lab`/`promote_prod`/`rollback_prod` (**đều `when: manual` + `allow_failure: false`**) |
| **W-0046** | Argo Rollouts: canary API, blue-green worker, analysis theo SLO |
| **W-0047** | `RotatingCredentialProvider` — **code production thật**: dual-key overlap, emergency revoke, audit **không** ghi giá trị; inventory + runbook |

### P10 · Tuân thủ & trưởng thành (W-0052 → W-0056)

| Work | Trạng thái | Đã giao gì |
| --- | --- | --- |
| **W-0052** | `TESTS_PASS` | `PersonalDataInventory` + `DsarService`; hằng số SQL **dùng chung** giữa retention ↔ DSAR (không hai bản) |
| **W-0053** | `TESTS_PASS` | `DataClassification`; script backup/restore/prune; `failover.sh`; DR selftest trong CI |
| **W-0054** | `TESTS_PASS` | mô hình capacity 1 → 32 kênh + cost model; selftest CI `allow_failure: false` |
| **W-0055** | `TESTS_PASS` | warehouse schema `analytics` (7 bảng); ETL job; KPI fold; retention hook; **pipeline chỉ đọc** |
| **W-0056** | `BLOCKED_EXTERNAL` | SLA/error budget/on-call — chờ `P9-2`; **không suy ra on-call maturity từ mock** |

### P11 · Đóng gói production (W-0057 → W-0060) — cả 4 đều `EVIDENCE_SUBMITTED`

| Work | Đã giao gì |
| --- | --- |
| **W-0057** | `docs/contracts/telephony-procurement-pack/` — R-01 yêu cầu vendor, R-02 gói lab, R-03 gói 32 eSIM, R-04 scorecard, R-05 năng lực TTS/audio, + biểu mẫu nghiệm thu lab |
| **W-0058** | `docs/contracts/target-v1-closure-pack/` — **9 ticket T-01..T-09** phủ 13 quyết định `OD-V1-*`, mỗi ticket đủ 9 mục DoD |
| **W-0059** | `docs/compliance/` — data inventory, retention options, PDPA legal-basis pack; 2 quyết định `DF-07`/`DT-05` đều gắn `LEGAL_SIGNOFF_REQUIRED`; gói ký `DF-03` |
| **W-0060** | `gate-status.mjs` sinh `gate-status.yaml` máy đọc được + `readiness-board.md`; CI **đối chiếu** và đỏ nếu lệch tracker |

---

## 4. Việc phát sinh ngoài kế hoạch — 40 work item

Đây là phần thường bị bỏ sót khi báo cáo theo phase. **35 trong 119 work item không có prompt** —
chúng phát sinh trong lúc làm, và phần lớn là **kết quả của rà soát đối kháng**.

### 4.1 · Nhóm `RED_TEAM_REMEDIATION` — 26 work item

Các lỗi này đều thuộc loại **"cổng xanh giả"**: gate CI báo PASS trong khi thực ra nó không kiểm
được gì. Đó là kiểu hỏng nguy hiểm nhất, vì nó không im lặng — nó **nói dối**.

| Work | Lỗi được sửa |
| --- | --- |
| **W-0067** | ký tự điều khiển `0x08` lọt vào regex PII làm **toàn bộ pattern vô hiệu** |
| **W-0073** | pattern địa chỉ chỉ khớp chữ thường; `grep -i` **không** fold được `Đ`↔`đ` (đo: 1/3 dòng ở mọi locale) |
| **W-0076** | bracket expression đa byte (`[Đđ]`, `[ốỐ]`) **vỡ dưới `LC_ALL=C`** — chỉ bắt 3/8 dòng; container CI tối giản thường ở `LC_ALL=C` ⇒ gate xanh giả theo cách khác |
| **W-0079** | CI coi **mọi** non-zero exit là fail ⇒ CT-CI-02/03 xanh giả; nay validate schema/severity của JSON vulnerability |
| **W-0080** | scanner PII không phủ `.sql` và file không extension; target thiếu/rỗng nay fail closed |
| **W-0068** | "kill switch immutable trong PRODUCTION_REAL" khiến **không bật được kill switch khi sự cố** — sửa thành bất đối xứng theo chiều an toàn |
| **W-0071** | PostgreSQL không cho `CHECK` tham chiếu bảng khác — thêm cột denormalize + same-row CHECK |
| **W-0072** | pattern `^[^0-9]*$` **loại nhầm `"Quận 7"`** hợp lệ |
| **W-0081/82** | error catalog lệch giữa spec/OpenAPI/domain; middleware error chưa bao auth/allowlist |
| **W-0083** | architecture guard chỉ kiểm assembly → thay bằng ma trận reference `.csproj` chính xác |
| **W-0084** | chứng minh **trực tiếp** trigger audit chặn cả UPDATE lẫn DELETE (SQLSTATE `P0001`) |
| **W-0087** | **E-01/E-02**: MOCK API/worker tách persistence + không có provisioning SIM ⇒ luồng intake→dispatch **không chạy được** |
| **W-0088** | **E-04..E-07**: sự cố của một job làm **treo toàn bộ intake**; quarantine/deadline/admin-review **không có lối ra hợp lệ** |
| **W-0089** | **E-03/E-08/E-10/E-11/E-20/E-22**: bypass phone tách rời, dương tính giả địa chỉ, thiếu call restriction, đóng băng replay tạm thời |
| **W-0090** | **E-09/E-12/E-15/E-16**: canonical decision drift, breaker half-open bị latch, idempotency transaction bị tách, TOCTOU trên callback lease |
| **W-0091** | **E-17..E-19/E-21/E-23**: legal-hold FK guard, chứng minh job `NOT_CONFIGURED`, seed canonical chạy được |
| **W-0092** | **E-13**: changelog/baseline/version cũ, drift bề mặt error/nullable/callback, thiếu khai báo internal token |
| **W-0093** | **E-14**: lịch sử protected-main và vài báo cáo đã ký **không còn hậu thuẫn kết luận của chính chúng** |
| **W-0094** | vòng đời singleton typed-client, cache dùng chung bị cancel, dictionary MOCK không giới hạn, `StopApplication` toàn worker |
| **W-0069/70/74/77** | sửa đồ thị phụ thuộc giữa các prompt/work item (cycle check 54 node = 0 cycle) |
| **W-0075** | doc-map phải do mapper chính thức sinh; generator tự viết không tái lập được semantics |

### 4.2 · Nhóm `UNPLANNED` do owner yêu cầu — 14 work item

| Work | Yêu cầu |
| --- | --- |
| **W-0104** `ACCEPTED` | preflight Asterisk/softphone **miễn phí** — kiểm toàn tuyến scheduler→gate→ARI→audio→DTMF trước khi tốn tiền SIM |
| **W-0105** | thay MOCK directory bằng **account/password + session opaque thu hồi được**; đúng 2 role |
| **W-0106** | 3 giọng nữ Bắc/Trung/Nam theo **34 đơn vị hành chính mới**; sửa lỗi đọc tiền/số lượng bằng chữ số |
| **W-0107** | Việt hoá **dữ liệu** (41 điểm trên 8 màn vẫn render enum thô); từ điển 39 họ/212 giá trị |
| **W-0108** | **ghép audio động** — trước đó một cuộc gọi phát đúng một file, nên bản thu LAB không chứng minh được khách nghe đúng đơn của mình |
| **W-0109** | vòng đời kịch bản: API + quyền + màn hình — trước đó **lối duy nhất để chữ ký Pháp chế vào hệ thống là sửa tay dữ liệu** |
| **W-0110** | màn cổng runtime — trước đó lối duy nhất để bấm là `curl`, tức là không có four-eyes ở tầng người dùng |
| **W-0111** | **cắt ngang cuộc gọi đang diễn ra** — trước đó script sai đang phát cho khách thì phải đợi khách cúp máy |
| **W-0112** | seed loader / scenario runner / integration profile — trước đó mỗi buổi nghiệm thu phải dựng lại demo bằng SQL tay |
| **W-0113** | ghi lại **giọng đã thực sự phát** thay vì suy lại lúc đọc |
| **W-0114** | cổng rolling deploy **hai chiều** |
| **W-0115** | 16 `CHECK` constraint enum trên 8 bảng ở tầng DB |
| **W-0116** | Việt hoá telemetry bằng **trường phụ** `detail_vi` — giữ `detail` thô để grep log |
| **W-0117** | hygiene sau `draft.17`: 14 lỗi lint OpenAPI → 0; đồng bộ README/spec/readiness board với source |
| **W-0118** | **`OD-15` — không gọi IVR cho khách cũ**; rút nghĩa vụ của Sales từ "build scoring engine" xuống **đúng một field** |

### 4.3 · Nhóm sự cố hạ tầng — 4 work item

| Work | Sự cố |
| --- | --- |
| **W-0078** | Testcontainers 4.13.0 kéo theo SSH.NET 2025.1.0 có lỗ hổng ⇒ ghim trực tiếp `SSH.NET 2026.0.0` |
| **W-0085** `ACCEPTED` | `ProjectReference` dùng dấu phân cách kiểu Windows ⇒ `UT-BOOT-03` **chỉ fail trên Linux** |
| **W-0086** `ACCEPTED` | shallow clone làm Gitleaks báo dương tính giả ở biên depth |
| **W-0061** `BLOCKED_EXTERNAL` | GitLab platform provisioning |

---

## 5. Nhóm A của kế hoạch 22/08 — đã đóng hết

Kế hoạch [`remaining-work-plan-2026-08-22.md`](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)
liệt kê 10 hạng mục "thiếu thật, IVR tự làm được ngay". **Cả 10 đều đã đạt `TESTS_PASS`
trong 4 ngày (22 → 25/08):**

| Hạng mục | Work ID | Trạng thái |
| --- | --- | --- |
| A1 · chuỗi ghép audio động | `W-0108` | ✅ `TESTS_PASS` |
| A2 · vòng đời kịch bản | `W-0109` | ✅ `TESTS_PASS` |
| A3 · màn cổng runtime | `W-0110` | ✅ `TESTS_PASS` |
| A4 · cắt ngang cuộc gọi | `W-0111` | ✅ `TESTS_PASS` |
| A5 · seed/scenario tooling | `W-0112` | ✅ `TESTS_PASS` |
| A6 · ghi giọng đã phát | `W-0113` | ✅ `TESTS_PASS` |
| A7 · số lượng thập phân | `W-0108` | ✅ gộp vào W-0108 |
| A8 · cổng schema compatibility | `W-0114` | ✅ `TESTS_PASS` |
| A9 · `CHECK` constraint DB | `W-0115` | ✅ `TESTS_PASS` |
| A10 · `detail_vi` | `W-0116` | ✅ `TESTS_PASS` |

Ước lượng ban đầu là **17–24 ngày công**. Thực tế đóng trong 4 ngày lịch.

**Nhóm D (trôi tài liệu) cũng đã đóng** qua `W-0117`: README, `specs/api/03`, readiness board và
kế hoạch 22/08 đã được đối soát lại với source hiện tại.

---

## 6. Những gì `TESTS_PASS` **chưa** chứng minh

Đây là danh sách phải đọc kèm mọi con số ở trên:

| Chưa chứng minh | Vì sao |
| --- | --- |
| **Chưa ai nghe audio** | 12 file MP3 đoạn cố định **chưa tồn tại**. Mọi khẳng định về audio là về *chuỗi xử lý*, không về *âm thanh* |
| **Chưa gọi qua SIM/carrier thật** | mới có softphone MicroSIP qua Asterisk. Một softphone không suy ra được một mạng di động |
| **Chưa chạy trên Sales thật** | mọi test tích hợp chạy với fake provider + WireMock |
| **Chưa có credential sandbox** | không có credential ⇒ **không chạy được một test tích hợp thật nào** |
| **Chưa deploy lần nào** | `helm rollback --atomic` đã cấu hình nhưng **chưa lượt deploy nào từng chạy** |
| **Hosted CI đang `NOT_RUN`** | `remote.origin.pushurl` trỏ GitHub trong khi `remote.origin.url` fetch từ GitLab ⇒ mọi gate hiện **chỉ chạy local**. *(Cập nhật `2026-08-27` — `W-0121` đã sửa lối đẩy: `origin` nay push tới cả GitLab lẫn GitHub. Vẫn `NOT_RUN` vì chưa push lượt nào.)* |
| **Chưa có chữ ký nào của Legal/Privacy** | `DF-07` retention và `DT-05` recording đều `LEGAL_SIGNOFF_REQUIRED` |
| **`OD-15` chưa skip ai** | Module 3 chưa gửi `trust.risk_evidence_available` ⇒ **mọi task vẫn được gọi** |
| **Four-eyes chưa đủ chữ ký** | `OD-V1-20` mới có chữ ký owner module IVR; chữ ký Security/Platform + Release owner **vẫn trống** |

---

## 7. Một điểm cần nói rõ về `OD-V1-20`

Tracker ghi `OD-V1-20` là `ACCEPTED`, nhưng hệ quả thực tế **hẹp hơn tên gọi**, và điều này đáng
được nêu trong báo cáo tiến độ vì dễ bị đọc nhầm thành "đã mở được cổng runtime":

- Admin **qua được** tầng permission của `POST /feature-flags/{env}`.
- Nhưng `FeatureFlagAdminService.MutateAsync` gọi `IRuntimeGateAuthorization.IsApprovedAsync()`
  **trước tiên**, và bản duy nhất đăng ký ngoài test là `PendingRuntimeGateAuthorization` →
  **luôn trả `false`**.
- Kết quả: POST trả `409 IVR_OPERATIONAL_BLOCKED` thay vì `403 IVR_FORBIDDEN_CALLER` —
  **đổi kiểu từ chối, không mở cổng**.
- Thay đổi có hiệu lực thật: hai GET flag/kill-switch nay trả `200` cho Admin.
- Muốn thật sự đổi được cờ phải **thay `PendingRuntimeGateAuthorization`** — chưa có bản duyệt nào
  trong production code. Gap `G-A` của lab **chưa đóng**.
