# M8-11 — Attempt-policy production decision pack

Work: `W-0151` · ngày audit: `2026-09-03`

Trạng thái:
`EVIDENCE_SUBMITTED / NUMERIC_SOURCE_CONFLICT_UNRESOLVED / CURRENT_WIRE_MISMATCH_FAIL_CLOSED / REGISTRY_LIFECYCLE_AND_RUNTIME_FLAG_DRIFT_FOUND / M3_ATTEMPT_POLICY_PRODUCER_NOT_FOUND / PRODUCTION_POLICY_NOT_APPROVED / EXTERNAL_SIGNATURES_REQUIRED / CODE_NOT_AUTHORIZED`

Owner quyết định bắt buộc: **Product + Order Core + Module 3**. Module 8, Platform và Release
tham gia ở các dòng kỹ thuật tương ứng nhưng không được ký thay ba owner này.

## 1. Kết luận audit

1. `mock-lab-v1` là candidate, không phải tên/version có thể đổi nhãn để dùng production.
2. Chưa có bộ số production được ký. Hai nhóm nguồn đang xung đột:
   - `D-10` và code candidate: Golden Hour `2 / [0,150] / 300s`; 24/7
     `2 / [0,450] / 900s`.
   - Hai tài liệu phase-8 business: Golden Hour `2 / [0,300] / 600s`; 24/7
     `3 / [0,300,600] / 900s`.
3. Wire hiện đã có quy tắc rõ: Module 3 gửi **version + snapshot**; IVR resolve registry theo
   `(version, program, execution_mode)` và so exact attempts, offsets, duration. Lệch thì không tạo
   job và trả `409 IVR_POLICY_MISMATCH`. Claim cũ “chưa có quy tắc trên wire” là sai.
4. Task/job đã nhận lưu snapshot policy bất biến; scheduler dùng snapshot đó. Đổi registry sau này
   không tự đổi lịch của job đang chạy.
5. Registry hiện mới đủ cho dev/lab primitive, chưa đủ làm production governance store: ghi theo
   từng program, không có bundle hash/approval reference/signer/effective-retire state; không có
   four-eyes verifier; database chỉ chặn update và kiểm bounds cơ bản.
6. `TechnicalRetryLimit=1` là config scheduler riêng, không nằm trong versioned attempt policy.
   Technical exception không đốt customer attempt và có thể requeue cùng attempt number; backoff
   production chưa được owner ký.
7. Runtime feature flag chặn đúng hai literal candidate (`mock-lab-v1`, `UNAPPROVED`) nhưng không
   resolve registry và không so `attemptPolicyVersion` với policy snapshot của job. Đây là hai lớp
   gate tách rời, chưa phải một active-policy contract nhất quán.
8. Không thấy timezone/quiet-hours/holiday policy trong wire, registry hoặc scheduler. Không được
   tự suy rằng M3 đã tránh giờ cấm hay IVR sẽ tự dời lịch.
9. Snapshot Module 3 `PhucApu@a3aad246d986fbc273cf41aaa93eec6659669656` không có producer
   code/schema cho `attempt_policy_version`, `max_customer_attempts`,
   `attempt_offsets_seconds` hoặc confirmation-window Target V1. Kết luận hẹp:
   `M3_ATTEMPT_POLICY_PRODUCER_NOT_FOUND` trên snapshot này; không khẳng định hệ thống khác không có.

Vì vậy, không có thay đổi runtime nào được phép trong W-0151. Production vẫn fail-closed và
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 2. Evidence hiện hành

### 2.1. Candidate và seed không đồng nhất phạm vi

| Nguồn | Golden Hour | 24/7 | Phạm vi thực tế |
| --- | --- | --- | --- |
| `src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs` | `2 / [0,150] / 300s` | `2 / [0,450] / 900s` | enum `CandidateMockLabOnly`; in-memory/dev loader có thể đăng ký MOCK + LAB |
| `deploy/docker/dev-seed/seed.sql` | cùng bộ số | cùng bộ số | chỉ `allowed_execution_modes=["MOCK"]`, `approved=false` |
| `deploy/lab/seed.sql` | `1 / [0] / 300s` | `1 / [0] / 300s` | version riêng `lab-softphone-v1`, chỉ `LAB_REAL_SIM`, `approved=false` |

Do đó câu “`mock-lab-v1` đang chạy cho cả MOCK/LAB” chỉ đúng với khả năng của code/dev loader,
không đúng với hai default database seed. Không seed nào là production approval.

### 2.2. Hai bộ số đang xung đột

| Thông số | `D-10` + `mock-lab-v1` | Phase-8 business | Chưa được quyết định |
| --- | ---: | ---: | --- |
| Golden Hour attempts | 2 | 2 | không |
| Golden Hour offsets | `[0,150]` | `[0,300]` | **có** |
| Golden Hour window | 300s | 600s | **có** |
| 24/7 attempts | 2 | 3 | **có** |
| 24/7 offsets | `[0,450]` | `[0,300,600]` | **có** |
| 24/7 window | 900s | 900s | không |

Nguồn business cần owner sửa hoặc gắn supersession banner sau khi ký; Module 8 không tự sửa
`docs/documents/`. `D-10` được giữ như lịch sử/candidate, không được gọi là production approval.

### 2.3. Wire và snapshot hiện tại

- OpenAPI bắt buộc window start/end, version, max attempts và offsets.
- `src/Ivr.Infrastructure/Intake/TaskIntakeService.cs` resolve registry
  trước; unknown/disallowed được giữ `TASK_HELD_POLICY_MISSING`; exact snapshot mismatch trả
  `409 IVR_POLICY_MISMATCH` và không tạo job.
- Khi accepted, service ghi version/max/offsets/window và precomputed schedule vào cả task/job.
  Validator + database trigger ngăn update các snapshot đó.
- OpenAPI schema tự nó chưa biểu đạt đủ: `offsets.count=max`, offset đầu `0`, tăng nghiêm ngặt,
  mọi offset nhỏ hơn window. Những invariant này hiện do domain/EF path giữ, không phải schema wire.

### 2.4. Registry/write path hiện tại

`PostgresAttemptPolicyRegistry` yêu cầu exact execution mode trong JSON; production còn cần
`approved_for_production=true` để tạo `OwnerApproved`. Writer dùng serializable transaction và ghi
audit hash, nhưng audit chỉ nhận `actor/reason/correlation` do caller cung cấp.

Các thiếu hụt trước production:

- không có signed approval reference, signer roles, four-eyes verification;
- không có `effective_at`, `retired_at`, active state hoặc rollout environment;
- key là `(version, program)`, writer đăng ký từng row, không atomic cho bundle hai program;
- không có bundle hash để chứng minh cùng một version nghĩa là cùng một ma trận ở mọi môi trường;
- database chỉ CHECK `max 1..10` và window dương; direct SQL insert có thể bỏ qua full JSON/offset
  validation của EF;
- update bị trigger từ chối, nhưng chưa có delete-protection/lifecycle contract;
- caller production được quản trị chưa tồn tại; caller code hiện thấy là dev seed loader.

### 2.5. Scheduler, counting và technical retry

Scheduler đếm chỉ các attempt có `is_counted_customer_attempt=true`, chọn schedule slot theo counted
progress, không claim sau expiry, sau final result hoặc khi còn active attempt. Disposition business
như no-answer/busy/rejected/no-input/wrong-input được tính; invalid phone final và technical exception
không tính customer attempt.

Technical exception hiện:

- dùng `SchedulerOptions.TechnicalRetryLimit`, default `1`, range `0..10`;
- retry cùng customer attempt number; customer-counted progress chưa đổi nên slot lịch đã đến hạn;
- requeue ngay theo poll/channel/cooldown/gate hiện hành, không có versioned backoff;
- hết retry limit thì giữ admin review; manual retry cũng dùng limit config này.

Vì vậy “2 lần gọi khách” không đồng nghĩa “tối đa 2 lần dial kỹ thuật”. Capacity/call-token sizing
phải bao gồm technical retries sau khi ATP-05 được ký.

### 2.6. Runtime gate drift

Intake xác thực policy A trong registry và snapshot A vào job. Trước dial, `DispatchGate` xác thực
feature-flag snapshot nhưng không nhận policy của job, không resolve registry và không so flag B với
A. Việc B chỉ cần khác literal candidate là chưa đủ chứng minh B được owner duyệt. Hai lựa chọn hợp
lệ phải được ký ở ATP-11; current behavior không được tuyên bố production-safe.

## 3. Decision matrix bắt buộc `ATP-01..ATP-15`

| ID | Quyết định phải ký | Owner bắt buộc | Artifact đóng |
| --- | --- | --- | --- |
| `ATP-01` | Ai là business authority; nguồn nào supersede nguồn nào; ai có quyền approve/change | Product + Order Core + M3 | signed authority statement + source references |
| `ATP-02` | Tên version production mới; version scope theo bundle hay program; bundle/hash atomic | Product + Order Core + M3 + M8 | immutable version + canonical bundle JSON/hash |
| `ATP-03` | Với từng program: max customer attempts, offsets, window; định nghĩa exact `T0` và clock-skew | Product + Order Core + M3 | signed two-program matrix |
| `ATP-04` | Taxonomy nào đốt customer attempt, terminal ở attempt/window nào, no-answer/invalid-input/busy/reject | Product + Order Core + M3 | counted/terminal disposition table |
| `ATP-05` | Technical retry limit, backoff, attempt-number semantics, manual retry và quan hệ với window | Product + Order Core + M3 + Platform | signed technical-retry policy |
| `ATP-06` | Timezone, quiet hours, holiday/blackout, window cắt qua giờ cấm; M3 lọc hay IVR defer/truncate | Product + Order Core + M3 | signed temporal policy + examples |
| `ATP-07` | Giữ wire version+snapshot hay đổi contract; source-of-truth registry; exact mismatch vẫn `409` hay khác | M3 + M8 + Product | signed wire schema/semantics |
| `ATP-08` | Producer tạo T0/version/snapshot ở bước nào; distribution ordering và CDC | M3 + Order Core | producer SHA + OpenAPI/schema + CDC tests |
| `ATP-09` | Registry writer/reader authority, four-eyes, signer evidence, DB validations, delete protection | Platform + M8 + Release | governed registration design + negative tests |
| `ATP-10` | `effective_at`, cutover, retire, propagation; cách giữ job đang chạy trên snapshot cũ | Product + M3 + M8 + Release | cutover/rollback runbook |
| `ATP-11` | Feature flag phải exact-match job policy, hay bỏ field dư và dùng signed active allowlist | Platform + M8 + Release | one coherent pre-dial rule + tests |
| `ATP-12` | Unknown, disallowed, partial bundle, unavailable registry, mismatch và config drift xử lý thế nào | M3 + M8 + Product | state/error/retry matrix |
| `ATP-13` | Policy mới tác động capacity, SIM pool, dial-token TTL/reuse/reissue và rate limits thế nào | Product + Platform + Telephony + M3 | recalibrated capacity/token evidence |
| `ATP-14` | Audit fields, metrics theo version/program, retention, PII-safe evidence và approval trace | Product + Platform + Privacy + M8 | audit/metric/retention schema |
| `ATP-15` | Environments, canary, shared E2E, release gates, rollback trigger và rollback authority | Product + M3 + Platform + Release + M8 | signed rollout packet + shared test report |

Không đóng một dòng bằng “OK”, test MOCK/LAB, hoặc commit chỉ phía IVR. ATP-01..08 cần chữ ký của
ba owner Product, Order Core, M3 theo đúng phạm vi; các owner bổ sung không thay thế họ.

## 4. Mẫu policy bundle để owner điền

```yaml
decision_id: <owner-decision-id>
policy_version: <new-production-version-never-mock-lab-v1>
bundle_schema_version: attempt-policy-bundle.v1
effective_at: <UTC instant>
retire_at: <UTC instant or null>
t0_definition: <exact producer event>
timezone_and_quiet_hours: <signed rule>
programs:
  GOLDEN_HOUR:
    max_customer_attempts: <int>
    attempt_offsets_seconds: [<int>, ...]
    confirmation_window_seconds: <int>
  TWENTY_FOUR_SEVEN:
    max_customer_attempts: <int>
    attempt_offsets_seconds: [<int>, ...]
    confirmation_window_seconds: <int>
technical_retry:
  max_retries_per_customer_attempt: <int>
  backoff_seconds: [<int>, ...]
  manual_retry_rule: <rule>
counted_dispositions: [<signed values>]
non_counted_dispositions: [<signed values>]
terminal_rules: [<signed rules>]
bundle_sha256: <sha256 of canonical bundle>
approval_refs:
  product: <signer/date/ref>
  order_core: <signer/date/ref>
  module_3: <signer/date/ref>
```

`policy_version` phải là version production mới. Không sửa enum approval hoặc DB row của
`mock-lab-v1` để hợp thức hóa candidate.

## 5. M8 proposal — chỉ có hiệu lực sau external sign-off

Đây là đề xuất triển khai, không phải business-number decision:

1. Giữ wire `version + snapshot` và exact `409` mismatch; registry là bảng canonical để IVR
   kiểm task do M3 phát.
2. Phát hành một immutable bundle gồm đủ cả hai program, có canonical hash và ba approval refs;
   publish atomically hoặc fail toàn bundle.
3. Đăng ký bundle đã ký ở mọi environment khi dispatch còn tắt; verify readback/hash/negative
   cases; sau đó mới cut M3 producer sang version mới.
4. Job đã accepted giữ snapshot/version cũ đến terminal. Không rewrite job in-flight.
5. Rollback bằng cách cho M3 producer quay về **previous signed version**, không xóa row cũ và
   không sửa candidate.
6. ATP-11 phải chọn một rule duy nhất: pre-dial exact-match job policy với signed active set, hoặc
   bỏ field version dư khỏi runtime config. Không giữ hai nguồn có thể drift.
7. Technical retry phải version-bound hoặc được ký như một policy production riêng với exact
   interaction tới attempt/window/capacity/token.
8. Cấm direct SQL production. Registration đi qua controlled release/admin path, four-eyes,
   approval refs và DB-level validation tương đương domain invariants.

## 6. State/error matrix đề xuất để ký

| Tình huống | Current behavior đã verify | Proposed production disposition | Ai ký |
| --- | --- | --- | --- |
| Version không có | intake hold, không job | giữ fail-closed; M3 không chờ callback, alert config | M3 + M8 |
| Version không cho execution mode | intake hold, không job | giữ fail-closed | Product + M8 |
| Version có nhưng snapshot lệch | `409 IVR_POLICY_MISMATCH`, không job | giữ exact mismatch, không auto-correct | M3 + M8 + Product |
| Chỉ có một program trong version bundle | registry hiện có thể lưu partial | reject activation toàn bundle | Product + M3 + M8 |
| Registry unavailable khi intake | không có signed production decision | reject/hold + retry ownership phải ký | M3 + Platform + M8 |
| Runtime flag khác job policy | current gate không so | block pre-dial + incident, hoặc bỏ redundant field | Platform + M8 + Release |
| Policy cutover khi job đang chạy | job giữ snapshot cũ | tiếp tục snapshot cũ; metrics tách version | Product + M3 + M8 |
| Technical error còn quota retry | non-counted, requeue | theo ATP-05 signed backoff/token/capacity rule | Product + M3 + Platform |
| Window hết | scheduler không claim sau expiry | terminal path/callback semantics giữ theo signed result contract | Product + M3 + M8 |
| Quiet hour bắt đầu giữa window | không có rule | phải chọn upstream reject/windowing hoặc IVR defer/truncate | Product + M3 |

## 7. Acceptance evidence trước khi mở code và production

- [ ] ATP-01..ATP-15 có câu trả lời, signer/date/authority/scope và approval refs.
- [ ] Policy bundle canonical + SHA-256; hai program đầy đủ, không partial.
- [ ] Product, Order Core và M3 ký exact attempts/offsets/window/T0/counting/retry/quiet hours.
- [ ] M3 giao producer commit/OpenAPI/schema/CDC và sandbox payload cho cả hai program.
- [ ] Registry lifecycle/four-eyes/database constraints và pre-dial coherence design được ký.
- [ ] Shared tests: unknown/disallowed/mismatch/partial/drift/cutover/rollback/technical retry/window.
- [ ] Capacity + telephony + dial-token được recalibrate theo signed matrix và retry envelope.
- [ ] Owner business sửa/supersede các nguồn số xung đột.
- [ ] Release packet giữ `REAL_CUSTOMER_CALL_ALLOWED=NO` cho tới final go/no-go.

## 8. Stop rule và bước tiếp theo

Không promote/rename `mock-lab-v1`; không sửa scheduler, registry, OpenAPI, DB, seed, config hoặc
feature gate chỉ từ đề xuất này. Bước tiếp theo là gửi ATP-01..ATP-15 và policy-bundle template cho
Product, Order Core và M3; nhận chữ ký cùng producer artifact trước khi mở work implementation mới.
