# KẾ HOẠCH HOÀN THIỆN MODULE IVR

**Ngày lập:** 05/09/2026  
**Baseline:** `2a6d2902a27d6d41a10d244ba28a5c78da1a86e2`  
**Nhân sự:** Owner (người dùng) + Codex  
**Nguyên tắc mua trunk/SIM:** thực hiện cuối cùng, sau khi software, sandbox và staging chạy ổn định.

## 1. Đích hoàn thành

**Phạm vi là backend.** Admin UI không thuộc module này — Module 3 sẽ tự làm console trên BFF/identity
của họ (Owner chốt 05/09/2026). Mọi mục dưới đây chỉ nói về API, worker, dữ liệu, telephony adapter và
bề mặt để Module 3 kết nối.

Module chỉ được coi là hoàn thiện khi đồng thời đạt các điều kiện sau:

1. 38 operation inbound và callback outbound đúng OpenAPI, có positive/negative/idempotency/auth tests.
2. Hai chương trình `GOLDEN_HOUR` và `24_7`, cùng các payment profile được chấp nhận, chạy E2E qua Module 3.
3. Scheduler, normalization, telephony dispatch, DTMF/result, callback, retry/DLQ và retention chạy liên tục, không cần thao tác tay.
4. Feature flags, kill switch, allowlist và `REAL_CUSTOMER_CALL_ALLOWED` fail-closed ở mọi môi trường.
5. Bề mặt admin API phục vụ đúng read/write/danger tier cho BFF của Module 3; danger có actor/reason.
6. Migrations hỗ trợ rolling/blue-green và rollback trong cửa sổ overlap; backup/restore và DR được chứng minh.
7. Tất cả test, quality, security, image, Kubernetes, observability, performance, chaos và progressive gates chạy trong CI bắt buộc.
8. Script/voice/privacy/retention/attempt policy/opt-out và release decision có evidence được đúng owner chấp nhận.
9. Lab với một trunk/SIM thật đạt tiêu chí; capacity đo thực tế mới quyết định số kênh/gói production.
10. Pilot, rollback rehearsal và go-live checklist đạt; chỉ khi đó mới đổi cờ cho cuộc gọi khách thật.

## 2. Quy tắc điều hành cho đội hai người

- Codex phụ trách source, test, scripts, tài liệu kỹ thuật, automation, phân tích lỗi và tạo evidence có thể tái lập.
- Owner chốt nghiệp vụ, cấp credential/môi trường, ký quyết định release, mua dịch vụ và thực hiện thao tác cần danh tính/con người.
- Chữ ký của Module 3, Legal/Privacy, Security/Platform hoặc vendor không được Codex tự thay thế; nếu Owner kiêm vai trò thì phải ghi rõ vai trò và căn cứ.
- Mỗi pha dùng một SHA bất biến; không trộn kết quả local, sandbox, staging, lab, pilot và production.
- Mỗi lỗi P0 phải có regression test. Không dùng “test xanh” để tự động suy ra production-ready.
- Giữ `REAL_CUSTOMER_CALL_ALLOWED=NO`, kill switch bật và destination allowlist rỗng cho đến pha lab.

## 3. Đường găng và thời lượng

| Pha | Thời lượng tập trung | Phụ thuộc | Kết quả bắt buộc |
|---|---:|---|---|
| P0. Sửa blocker nội bộ | 1–2 ngày | Không | Local build/run/package sạch |
| P1. Đóng API và pipeline MOCK | 2–3 ngày | P0 | 38 operation + full worker E2E |
| P2. Module 3 sandbox | 3–5 ngày | P1 + M3 endpoint/credential | Producer/callback/BFF E2E |
| P3. CI và release artifact | 2–3 ngày | P1 | Required pipeline + immutable candidate |
| P4. Staging không gọi thật | 4–6 ngày | P2–P3 + platform | Soak/perf/chaos/DR/rollback |
| P5. Policy và acceptance | 2–4 ngày | P4 + owner/signers | Go/no-go packet hoàn chỉnh |
| P6. Lab một SIM/trunk | 2–4 ngày | P5 + mua gói lab | DTMF/call outcome/capacity thật |
| P7. Scale, UAT, pilot | 4–7 ngày | P6 | Production package và pilot đạt |

Thời gian kỹ thuật tối thiểu khoảng 20–34 ngày làm việc tập trung. Thời gian lịch có thể dài hơn nếu chờ Module 3, platform, Legal hoặc vendor; không rút ngắn bằng cách bỏ gate.

## 4. P0 — Sửa ba blocker nội bộ

### P0.1 Feature-flag API — `TESTS_PASS` (05/09/2026)

- Tái hiện lỗi provider cho đủ `dev`, `staging`, `lab`, `pilot`, `prod` trên host Development/PostgreSQL chuẩn.
- Ghi log exception nội bộ an toàn để xác định store/DI/config gây lỗi; không lộ secret hay PII.
- Làm cho GET snapshot và kill-switch đọc được ở MOCK; provider hỏng vẫn trả safe default và chặn dial.
- Chuẩn hóa environment sai thành error code 4xx trong envelope, không để `ArgumentOutOfRangeException` thành 500.
- Thêm integration test chạy đúng composition root của `Ivr.Api`, không chỉ test host thay thế dependency.

**Exit đạt:** năm environment hợp lệ trả 200; input sai trả 404/`IVR_NOT_FOUND`; outage snapshot trả 409 nhưng kill switch vẫn ON và real calls false. Evidence: [`W-0190`](../evidence/W-0190/README.md).

### P0.2 Dev seed/scenario — `TESTS_PASS` (05/09/2026)

- Cấu hình `Ivr:DevTooling:SeedDirectory` mặc định an toàn cho Development hoặc resolve từ content root.
- Cập nhật lệnh chạy local chuẩn; validate sớm và báo lỗi actionable nếu thiếu file.
- Test fresh clone: prepare DB → start API → seed → dry-run mà không cần biến môi trường bí mật ngoài tài liệu.

**Exit đạt:** `pnpm dev:bootstrap` chuẩn bị DB, khởi động API MOCK ở cổng trống, trả 9/9 seed outcome (8 dry-run job + 1 restricted) và replay `SCN-001-confirm` đúng. Evidence: [`W-0191`](../evidence/W-0191/README.md).

### P0.3 Migration expand-contract — `TESTS_PASS` (05/09/2026, local/PostgreSQL)

- Thay migration drop trực tiếp bằng chuỗi: ngừng ghi/đọc → deploy phiên bản tương thích → xác nhận không còn consumer → cleanup migration ở release sau.
- Chứng minh upgrade N-1→N, N/N+1 overlap, rollback N và forward recovery trên bản sao PostgreSQL.
- Cấm destructive DDL trong pha expand bằng static/CI guard.

**Exit đạt:** `progressive-selftest.mjs` PASS; hai binary trên PostgreSQL copy qua upgrade/overlap/rollback/forward đều đạt, ghim candidate `c8dc3c4`; unit `528/528`, integration schema `5/5`. Evidence: [`W-0196`](../evidence/W-0196/README.md). Cleanup vẫn ở release sau, cần xác nhận hết consumer và đóng cửa sổ rollback; không suy ra staging/production-ready.

## 5. P1 — Đóng chức năng local/MOCK

### P1.1 Ma trận API đầy đủ

- Sinh danh sách 38 operation trực tiếp từ OpenAPI và đối chiếu endpoint runtime.
- Với mỗi operation, chạy: happy path, malformed body/query, auth thiếu/sai tier, scope sai, not-found/conflict và correlation ID.
- Với endpoint ghi, thêm retry cùng key, replay cùng payload và conflict khi đổi payload.
- Xác nhận 11 result code wire; hai mã blocked chỉ xuất hiện pre-call như đặc tả.
- Xác nhận response không chứa phone/address/payment/recording hoặc field ngoài allowlist.

**Exit đạt (local/MOCK):** [`W-0197`](../evidence/W-0197/README.md), [`JSON máy đọc`](../evidence/W-0197/api-matrix.json): **38/38 operation, 417 HTTP request**, 11 wire code, schema/allowlist/PII và retry/replay/conflict PASS. Lượt cuối matrix + replay transaction **3/3**; unit **528/528**, contract **24/24**; validator **18/18**. Evidence ghim content hash của working-tree candidate trên `d5539ba`, không phải hosted/exact-commit release proof; P1.2 crash/full-worker E2E vẫn mở.

### P1.2 Full worker pipeline

- Tạo profile `LocalMockE2E`: scheduler, normalization, callback và retention bật; telephony/TTS vẫn fake deterministic.
- Chạy chuỗi intake → claim → script/TTS → mock DTMF/outcome → normalize → callback fake → retention.
- Bao phủ answer/no-answer/busy/timeout/invalid DTMF, retryable/non-retryable ACK, 429 Retry-After, stale/revoked và kill switch giữa claim/dial.
- Kiểm tra crash recovery, lease expiry, duplicate delivery, DLQ/replay và concurrency.

**Exit đạt (local/MOCK):** [`W-0203`](../evidence/W-0203/README.md),
[`JSON máy đọc`](../evidence/W-0203/local-mock-e2e.json): **100/100 vòng, 1110 task, 0 failure**;
0 task có hai final result, 0 task có hai callback, 0 kết quả không-phải-khách bị đếm là customer
attempt. Sổ giao nhận lấy từ journal của bên nhận: **1085 callback id, 23 lần giao lại, 0 lần không
nhất quán**. Mỗi scenario khai trước số dial được phép nên "retry có trần" là khẳng định kiểm được.
Bảy pha tiêm lỗi PASS: worker bị giết giữa cuộc gọi (thu hồi lease, **không quay số lại**), lease
hết hạn, kill switch bật/tắt, operator cắt cuộc đang gọi (ra technical, không tính customer
attempt), callback outage rồi phục hồi, dead-letter rồi replay, hai worker song song.

Ba khoảng trống mở phát hiện trong lượt này, ghi trong evidence chứ không lấp: **F-1 (HIGH)** 10
request `/eligibility-checks` đồng thời → 9 trả HTTP 500 (`40001` dưới envelope SERIALIZABLE của
idempotency store) — harness cần 99 lần retry để đi hết 100 vòng, và bản vá phải chứng minh từng
call site an toàn khi chạy lại nên thuộc một việc riêng; **F-2** dead letter chỉ replay được bằng
SQL tay; **F-3** MOCK không có kill switch runtime. Đây là working-tree candidate local/MOCK, không
phải hosted CI, không phải staging soak.

## 6. P2 — Tích hợp Module 3 khi chưa có telephony thật

### P2.1 Chốt contract

- Owner và phía Module 3 duyệt 22 field intake, 13+1 field callback, enums, versioning và compatibility window.
- Chốt producer theo `decision`, cặp program × payment, freshness/revoke semantics và `golden_hour_session_id` nếu được duyệt.
- Chốt attempt policy version, opt-out boundary, `dial_token` opaque và profile auth/rotation.
- Freeze OpenAPI version, generated client và contract hash; thay trạng thái draft chỉ sau chữ ký hợp lệ.

**Trạng thái (06/09):** cơ chế đóng băng **đạt** — [`W-0204`](../evidence/W-0204/README.md):
`contract-freeze-verifier` 5 kiểm, selftest đột biến **11/11**, generated client nay được ghim
hash, closure pack thôi chép lại pin (bản cũ gọi tên baseline `draft.18` không còn tồn tại), bảng
22/13+1 field trong IR-06 nay được gate so khớp từng tên với spec, và trạng thái `DRAFT` không còn
đổi được bằng cách sửa một chuỗi. Bề mặt wire để duyệt gom về một trang sinh tự động:
[`target-v1-field-inventory.md`](../contracts/target-v1-field-inventory.md).

**Chữ ký thì chưa.** Owner đã ký đóng `OD-V1-01..08` và `OD-V1-12..18` ngày 05/09 — nhiều hơn những
gì IR-05 còn mô tả — nhưng **Module 3 chưa đồng ký bề mặt và chưa giao producer SHA/OpenAPI/CDC**,
mà closure evidence của chính các dòng đó liệt kê producer artifact là điều kiện đóng. Vì vậy
`TARGET_CONTRACT_V1` giữ `DRAFT`, và đó là trạng thái đúng chứ không phải nợ.

Hai mục thuộc P2.1 mà trước lượt này **không tài liệu nào đang giữ** đã được hỏi và owner ký ngày
06/09: `OD-V1-22` **compatibility window ≥ 90 ngày** (đồng hồ chạy khi successor vừa được duyệt vừa
có ở non-prod) và `OD-V1-23` **opt-out explicit-only V1**. Hệ quả của mục sau cần theo dõi: hằng số
`2/3` đang chạy (suy opt-out từ nhiều lần `Rejected`) **không có authority**, nên từ nay là một
khoảng trống chứ không phải một quy tắc. `OD-V1-23` mới là vị trí owner — Legal/Privacy và CRM/M3
còn phải đồng ký trước khi có code opt-out. `golden_hour_session_id` vẫn cấm đổi code/OpenAPI/DB
trước chữ ký M3.

**Còn lại để đóng P2.1:** chữ ký bề mặt của Module 3 (IR-06 §10) và producer artifact — SHA/OpenAPI/
CDC — cộng credential sandbox. Không mục nào trong số đó IVR tự làm được.

### P2.2 Sandbox E2E

- M3 phát task call-ready cho cả hai chương trình; IVR không tự quyết định nghiệp vụ Order Core.
- Callback fake telephony về M3 với toàn bộ outcome; M3 revalidate trước khi đổi trạng thái đơn.
- Kiểm đủ ACK `ACCEPTED`, duplicate/idempotent, stale, blocked, malformed/unsupported content và Retry-After.
- Test rotation token có overlap, rate limit, timeout, DNS/TLS/network outage và replay sau phục hồi.
- BFF M3 gọi các API admin theo đúng read/write/danger tier; danger có actor/reason/four-eyes khi áp dụng.

**Exit:** hai chiều producer/callback và bề mặt admin đều có sandbox evidence trên cùng contract hash.

**Trạng thái (06/09) — nửa IVR đạt, chưa đạt exit:** [`W-0207`](../evidence/W-0207/README.md).
M3 chưa xong luồng bán hàng nên không cấp được sandbox; chờ là thời gian chết, nên đảo chiều phụ
thuộc và chứng minh trước nửa IVR trên đúng ma trận M3 sẽ được mời đồng ký:
**`shared_e2e_ivr_side` 11/11 `TV1-E2E` DEMONSTRATED**, run `20 vòng / 330 task / 0 failure`, 0
final trùng, 0 callback trùng; journal bên nhận `314 callback id / 26 giao lại / 0 không nhất quán`.

**Một mâu thuẫn hợp đồng đã được tìm ra và sửa, không cần M3.** Tờ ma trận `W-0174` yêu cầu M3 trả
`DUPLICATE_ACCEPTED` trên **HTTP 409** (và `BLOCKED_BY_CORE`/`REVIEW_REQUIRED` trên 200 **hoặc**
409), trong khi hợp đồng đã ghim chỉ cho các mã đó ở **200**. Không phải lỗi nhỏ: transport đọc body
409 theo `CallbackAck409`, không parse được thì trả `CALLBACK_ACK_INVALID` và dispatcher biến thành
**`INVALID_DEAD_LETTER` — terminal, không retry**. M3 xây theo tờ đó sẽ **dead-letter mọi lần replay
chính xác**, im lặng. Hệ quả được chứng minh bằng `UT-CALLBACK-TARGET-ACK-CROSS-01` trước khi sửa;
`FREEZE-06` chặn lớp lỗi này tái diễn (selftest 14/14).

**Còn thiếu để đóng P2.2:** producer của M3 phát task call-ready, M3 revalidate rồi đổi trạng thái
đơn, và BFF của M3 gọi admin API — **không quan sát được từ phía IVR bằng bất kỳ cách nào**. Chữ ký
thật cần năm vai (`M8_OWNER`/`M3_OWNER`/`SECURITY`/`PLATFORM`/`RELEASE_OWNER`); template vẫn
`NOT_READY`. Rotation token/rate limit/DNS-TLS mới ở mức fake — bằng chứng thật cần credential
sandbox (`OQ-AUTH-01`).

## 7. P3 — CI và candidate phát hành

- Đưa restore/build, 799+ test, OpenAPI, security, image, K8s, progressive và evidence validators vào required pipeline.
- Bật dependency cache có lockfile; cấm job bỏ qua test hoặc dùng artifact không đúng SHA.
- Sinh SBOM, vulnerability report, Gitleaks report, image digest và provenance manifest.
- Chạy coverage thật, đặt threshold cho domain/API/callback/gates; mọi critical path cần negative tests.
- Sửa trạng thái GitLab approval hoặc ghi quyết định governance thay thế; không tuyên bố gate đạt khi plan yêu cầu reviewer thứ hai nhưng tài khoản/gói chưa hỗ trợ.
- Freeze release candidate bằng commit SHA + image digests + OpenAPI hash; test clean checkout.

**Exit:** merge bị chặn khi bất kỳ required job fail; candidate tái tạo được từ clean runner.

## 8. P4 — Staging hoàn chỉnh nhưng vẫn không gọi thật

### P4.1 Platform/deploy

- Provision PostgreSQL, registry, secrets manager, DNS/TLS, M3 endpoints và observability stack.
- Deploy API/Worker/Migrate bằng Helm; NetworkPolicy default-deny, non-root/read-only FS, resource limits và PodDisruptionBudget.
- Diễn tập rolling, blue-green, failed migration, rollback và token rotation.

### P4.2 Observability/SLO

- Thu metrics: intake/reject, queue age, claim latency, channel utilization, call outcome, callback retry/DLQ, TTS latency/error và retention lag.
- Trace xuyên M3 → IVR → mock telephony → callback bằng correlation/trace ID privacy-safe.
- Dashboard và alert cho backlog, callback outage, provider unreadable, kill switch, DB saturation và no-progress worker.
- Chốt SLI/SLO, alert threshold, on-call/runbook và cách xác nhận phục hồi.

### P4.3 Performance/soak/chaos/DR

- Load theo low/base/peak/burst; đo throughput, p95/p99, DB pool và channel demand với fake gateway.
- Soak liên tục 24 giờ trước, 72 giờ cho candidate; không tăng backlog vô hạn hoặc rò bộ nhớ/connection.
- Chaos DB/network/API/worker/pod; xác nhận không duplicate outcome và không dial khi state không chắc chắn.
- Backup/restore có timestamp; chạy multi-node/multi-AZ failover và kiểm encryption at rest.
- Chứng minh RPO/RTO mục tiêu bằng restore thật, không dùng kết quả single-host thay thế.

**Exit:** staging candidate xanh 72 giờ; rollback và DR rehearsal đạt; chưa bật cuộc gọi thật.

## 9. P5 — Policy, privacy và release acceptance

- Owner chốt attempt window/max attempt/cooldown/timezone/holiday và version pin.
- Duyệt script cho cả hai chương trình; cố định prompt, DTMF mapping, fallback và lời thông báo phù hợp.
- Audition giọng theo vùng/thiết bị; chốt voice ID, tốc độ, pronunciation, cache/provenance và fallback file.
- Chốt opt-out/DTMF-0 handoff; suppression chỉ xảy ra theo contract rõ ràng và có audit.
- Legal/Privacy chốt dữ liệu được lưu, retention từng bảng/log/evidence, DSAR/delete và recording OFF.
- Security chốt auth, secret rotation, threat model dial-token, allowlist và incident response.
- Hoàn thiện runbook deploy/rollback, provider outage, callback backlog, kill switch, restore và communication.
- Tổ chức go/no-go trên đúng candidate; mọi mục `NOT_RUN`, `BLOCKED_EXTERNAL`, `OWNER_DECISION_REQUIRED` phải được đóng hoặc có quyết định hoãn minh bạch.

**Exit:** software/staging đạt và packet được các owner thực tế chấp nhận; gate lab/procurement vẫn mở có chủ đích. Đây là điểm duy nhất cho phép chuyển sang mua lab trunk/SIM.

## 10. Cổng quyết định mua trunk/SIM

Chỉ mua khi tất cả câu sau đều **Có**:

- P0–P5 đã đạt trên cùng release candidate.
- Module 3 sandbox E2E hai chiều đạt; auth/rotation/callback không còn draft.
- 72 giờ staging soak, performance, chaos, rollback và DR đạt.
- Script/voice/privacy/Legal/Security/release acceptance hoàn tất.
- Kill switch, allowlist số nội bộ, cost cap và vendor stop procedure đã test bằng mock.
- Capacity model có traffic thực tế đủ để ước lượng; hiện self-test chỉ cho dải 7–72 kênh nên chưa đủ căn cứ mua 32.

Nếu một câu trả lời là **Không**, tiếp tục dùng simulator; không mua gói dài hạn.

## 11. P6 — Lab với gói nhỏ nhất/1 SIM

- Chọn gói lab ngắn hạn, một kênh hoặc một SIM; không cam kết production package.
- Tích hợp adapter vendor trong profile `LAB_REAL_SIM`; credential ở secrets manager, không vào repo.
- Allowlist duy nhất số Owner; kill switch mặc định bật; đặt spend/minute/day cap.
- Test originate, answer, busy/no-answer, DTMF 0/1/2/invalid/timeout, hangup, caller ID, Unicode/TTS và callback outcome.
- Test mất sóng, SIM lock/balance, provider 429/5xx, timeout, duplicate event và restart adapter.
- Đo setup time, call duration, concurrent channel, rate limit, delivery quality và chi phí thực.
- Không gọi khách thật; không dùng recording; lưu evidence privacy-safe.

**Exit:** lab report đạt, không dial ngoài allowlist, kill switch dừng được ngay và số đo capacity/cost tin cậy.

## 12. P7 — Scale, UAT, pilot và go-live

### P7.1 Quyết định gói production

- Tính số kênh từ peak arrivals, p95 duration, retries, headroom và failover; so sánh quote vendor.
- Chỉ mua 32 eSIM nếu số đo chứng minh nhu cầu; nếu thấp hơn thì mua nhỏ hơn, nếu cao hơn phải thiết kế lại capacity.
- Chốt redundancy, SIM rotation, balance monitoring, caller ID, quota và SLA/escalation vendor.

### P7.2 UAT/pilot

- UAT bằng số nội bộ của Owner và bộ đơn sandbox đã biết kết quả.
- Pilot khách thật chỉ sau go/no-go: cohort nhỏ, giờ giới hạn, concurrency thấp, live dashboard và người trực kill switch.
- Theo dõi confirm/error/opt-out/complaint/callback lag/cost; định nghĩa ngưỡng auto-stop trước khi chạy.
- Diễn tập rollback ngay trước pilot; lưu decision/evidence theo SHA và config revision.

### P7.3 Go-live/hypercare

- Tăng tải theo bậc, không bật toàn bộ một lần; mỗi bậc cần quan sát ổn định trước khi tăng.
- Hypercare tối thiểu 3–7 ngày; review hằng ngày outcome, retry, complaint, capacity, cost và incident.
- Sau ổn định mới đóng temporary flags, tài liệu vận hành, ownership và lịch DR/rotation định kỳ.

**Exit cuối:** các tiêu chí mục 1 đạt, gate chính thức không còn blocker bắt buộc và Owner ký release cho đúng candidate.

## 13. Bảng kiểm không được bỏ sót

| Nhóm | Evidence cuối |
|---|---|
| Contract | OpenAPI signed, generated client, CDC hai chiều, compatibility matrix |
| API | 38/38 matrix, auth tiers, idempotency, stable errors, privacy assertions |
| Worker | E2E outcomes, retries/DLQ, crash/lease recovery, no-duplicate proof |
| Data | expand-contract, backup/restore, retention/DSAR, multi-AZ DR |
| Telephony/TTS | mock soak, voice approval, one-SIM lab, cost/capacity report |
| Security | threat model, secret rotation, scans, NetworkPolicy, incident test |
| Operations | dashboards/alerts/SLO, runbooks, rollback, on-call readiness |
| Release | exact SHA/digests/hashes, required CI, accepted go/no-go, pilot report |

## 14. Thứ tự thi công ngay

1. P0.1–P0.3 và P1.1–P1.2 đã đạt ở local/MOCK.
2. Đóng `F-1` (HTTP 500 khi ghi song song với idempotency key khác nhau) trước khi mở P2: Module 3
   sẽ gửi song song, nên đây là blocker thật cho sandbox chứ không phải nợ kỹ thuật.
3. Chạy lại toàn bộ suite, image self-test, progressive và K8s.
4. Chốt/triển khai sandbox Module 3 song song với required CI.
5. Dựng staging, chạy 24–72 giờ và đóng policy/evidence.
6. Qua cổng mua mới lấy một gói lab tối thiểu; qua lab mới quyết định scale/pilot.
