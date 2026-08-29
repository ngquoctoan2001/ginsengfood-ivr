# W-0139 — B-06 Observability code + local runtime

- Ngày: `2026-08-29`
- Baseline: `main@0baed74cd384cd661aed068c263a92ef97ead1f4`
- Trạng thái: `B06_CODE_AND_LOCAL_RUNTIME_PASS` / tracker `TESTS_PASS`
- B-06: **OPEN — STAGING EVIDENCE REQUIRED**
- Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Kết quả local đã chứng minh

- OpenTelemetry .NET `1.18.0` được locked cho hosting, OTLP, ASP.NET Core và HttpClient.
- API (`ginsengfood-ivr-api`) và Worker (`ginsengfood-ivr-worker`) xuất trace/metric/log qua OTLP.
- Task lưu nullable `trace_parent varchar(256)` và `trace_state varchar(512)`, không index; dữ liệu cũ
  null vẫn chạy với marker missing-context.
- Một task `TASK-E2E-CONFIRM` giữ TraceId `a1df526923273604d064695e50d0aed2` qua đủ năm stage.
- Tempo thấy cả API + Worker và outbound HTTP child; Prometheus thấy intake/attempt/result/callback;
  Loki có 466 stream, có trace/correlation context và scan không thấy raw phone/token; Grafana
  provision dashboard read-only.
- Sau khi dừng LGTM, `/health/live` vẫn HTTP 200; replay cùng byte payload vẫn idempotent và callback
  phía Sales vẫn đúng một bản.

Machine-readable snapshot: [`runtime-proof.json`](runtime-proof.json).

## 2. GitNexus blast radius trước edit

| Target | Risk | Affected/direct/flow |
| --- | --- | --- |
| `ConfirmationTaskEntity` | CRITICAL | 339 / 100 / 18 |
| `IvrTelemetry.StartSpan`, intake/persistence/lease/outbox/callback entrypoints | HIGH | tối đa 20 / 17 / 4 theo target |
| eligibility, normalization | MEDIUM | 15/14/1 và 13/12/1 |
| scheduler runtime/model config/test harness | LOW | additive/local |

Index được refresh về `50,910` node, `70,152` edge, `430` cluster, `300` flow trước khi sửa.

Sau sửa, `gitnexus detect-changes --scope all` báo `47` tracked file, `120` symbol, `45`
execution flow, mức `CRITICAL`. Kết quả khớp cảnh báo trước sửa: các flow nhạy nhất đi qua callback
`RunBatchAsync`, scheduler `RunOnceAsync`/lease quarantine và intake `IntakeAsync`; không phát hiện việc
đổi public contract hay business decision. Con số 47 của raw checkout có cả hai tài liệu WIP ngoài
scope được giữ nguyên; security candidate B-06 là 59 file sau khi loại các WIP đó và thêm file mới.

Sau khi thêm staging harness, lượt detect cuối trên raw checkout báo `56` tracked file, `151` symbol,
`45` flow, vẫn `CRITICAL`; phần tăng file/symbol có concurrent W-0122 WIP, còn số execution flow không
tăng. Riêng symbol CI được sửa ở lượt này, `assertCiTopology`, có impact `LOW`: 1 caller, 0 flow.
Verifier mới chưa tồn tại trong baseline index. Candidate W-0139 cuối là 62 file sau khi loại toàn bộ
WIP ngoài scope.

## 3. Verification local

| Gate | Kết quả |
| --- | --- |
| `dotnet restore Ivr.sln --locked-mode` | PASS |
| `dotnet build Ivr.sln -c Release --no-restore` | PASS — 0 warning, 0 error |
| .NET tests sau regenerate traceability | PASS — Unit 495, Contract 24, Integration 233, Chaos 8; tổng 760/760 |
| DB migration integration `IT-INTAKE-DB-01` | PASS; fresh/upgrade path chạy migration mới |
| `UT-OBS|UT-DASH` | 12/12 PASS |
| LGTM runtime | `IT-OBS-TRACE-02`, `IT-OBS-EXPORT-11`, `IT-OBS-RESILIENCE-12`, `IT-IMG-E2E-05`, `IMAGE_SELFTEST_PASS` |
| Helm | positive render + thiếu endpoint + thiếu selector + half-secret negative controls PASS |
| Prometheus | 6 rules parse; 4 promtool rule-test file PASS |
| Compose | merged config PASS; LGTM image pinned `0.30.0` |
| Docs/CI topology | PASS; six observability jobs root-included, `allow_failure: false` |
| Staging evidence verifier | `CT-OBS-STAGING-13/14` PASS; positive proof + incomplete-trace/mutable-image/credential-field/HTTP negative controls |
| Test traceability | `471` TestId source mappings CURRENT |
| Gitleaks 8.30.0 candidate | PASS — 62 file, 1.61 MB, no leaks; 5 exact EF-designer seed false positives được line-ignore |
| Gitleaks 8.30.0 full tree | **FAIL — pre-existing/concurrent WIP findings**, không dùng làm PASS cho W-0139 |
| PII scan W-0139 evidence | PASS — 3 text file, `C.UTF-8` |
| PII scan full evidence tree | **FAIL — pre-existing/concurrent hits** ngoài W-0139 |
| `git diff --check` | PASS toàn checkout ở lần chốt; 4 trailing-space dòng concurrent WIP thấy giữa lượt đã được luồng sở hữu sửa, Codex không chạm các file đó |

## 4. Các lỗi runtime test đã bắt và đã sửa

1. Image Worker dùng base `dotnet/runtime` nhưng shared observability composition cần
   `Microsoft.AspNetCore.App`; đổi sang ASP.NET chiseled image cùng digest pin.
2. Full-flow fixture còn gửi `sellable_status` đã rời current intake contract; bỏ field stale, không
   nới schema.
3. Bản resilience đầu tái tạo timestamp nên payload hash khác và idempotency conflict đúng thiết kế;
   test cuối replay đúng cùng byte payload.

## 5. Staging closure pack — chưa chạy

Repo đã có verifier read-only `deploy/ci/scripts/observability-staging-evidence.mjs`, manifest mẫu
`deploy/observability/staging-evidence.template.json`, contract job fail-closed và manual job
`observability_staging_evidence` trên default branch. Verifier yêu cầu exact SHA/image digest,
collector endpoint/selectors/secret reference, năm stage span, bốn metric, PII-safe trace/correlation
log, dashboard + ảnh, alert firing + recovered và retention/access approval trước khi ghi
`closure-manifest.json`. Credential chỉ nhận từ masked CI variables và không được persist.
Platform handoff từng bước: [`staging-handoff.md`](staging-handoff.md).

`CT-OBS-STAGING-13/14` đã PASS local nhưng chỉ kiểm verifier bằng backend-shaped fixture. Máy hiện tại
không có kube context, staging/OTLP biến môi trường hoặc GitLab CLI, nên job staging thật vẫn chưa
được chạy và không có closure manifest staging nào được tạo.

Các mục sau đều `NOT_RUN` / `OWNER_DATA_REQUIRED`:

- Platform OTLP endpoint, credential secret, collector namespace/pod selectors;
- retention và access policy;
- exact deployed image digest + Git SHA + environment;
- staging task ID, correlation ID và TraceId;
- dashboard screenshot/query; trace đủ 5 stage; metric query; log-redaction query;
- alert fire + recovery evidence.

Vì vậy không đổi B-06 thành đóng, không gọi production-ready và không gạch phần observability của
`W-0063` ở lượt local này. Các phần registry/K8s/secret store/progressive delivery/warehouse của
`W-0063` vẫn mở bất kể staging observability sau này có đạt.

Không cập nhật `.docx` vì `OD-20` còn mở. Không có real SIM/provider/customer call.
