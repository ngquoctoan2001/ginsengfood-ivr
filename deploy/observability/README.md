# Local observability (`W-0139`)

Stack này chỉ dùng development/test. Nó ghép collector, Prometheus, Tempo, Loki và Grafana bằng
`grafana/otel-lgtm:0.30.0`; không được dùng làm production backend.

```powershell
docker compose -f docker-compose.dev.yml -f docker-compose.observability.yml up -d
```

Mặc định Grafana ở `http://127.0.0.1:53000`; có thể đổi bằng `IVR_GRAFANA_PORT`. API và Worker gửi
OTLP/gRPC nội bộ tới `otel-lgtm:4317`. Dashboard read-only
`dashboards/ivr-slo-health.json` được provision tự động.

Full proof fail-closed:

```powershell
node deploy/ci/scripts/image-selftest.mjs --skip-compose --skip-scan --observability-runtime
```

Lệnh tạo đúng một MOCK task, không gọi SIM/provider/khách thật, rồi kiểm Tempo, Prometheus, Loki và
Grafana. JSON query evidence nằm ở `artifacts/observability/runtime-proof.json`. Sau khi thu evidence,
test dừng LGTM để chứng minh liveness và idempotent business replay vẫn hoạt động.

Staging/production dùng cùng contract OTLP nhưng backend, credential, retention và quyền truy cập do
Platform cấp. Không đặt secret/header thật trong repository.

## Staging closure evidence

Verifier staging là `deploy/ci/scripts/observability-staging-evidence.mjs`. Nó chỉ đọc backend sau
khi Platform đã deploy exact candidate, tạo một task MOCK và chạy controlled alert drill; verifier
không tự tạo cuộc gọi, không thay đổi staging và luôn yêu cầu `REAL_CUSTOMER_CALL_ALLOWED=NO`.

1. Sao chép `staging-evidence.template.json` ra một file evidence ngoài repository và điền exact Git
   SHA, image digest, collector selectors/secret reference, task/correlation/TraceId, backend URL,
   alert window và retention/access approval.
2. Cung cấp ảnh dashboard qua GitLab **file variable**
   `IVR_STAGING_DASHBOARD_SCREENSHOT_FILE`.
3. Cung cấp manifest qua file variable `IVR_STAGING_EVIDENCE_INPUT_FILE`. Nếu query backend cần
   credential, dùng các masked variable `IVR_STAGING_TEMPO_HEADERS_JSON`,
   `IVR_STAGING_PROMETHEUS_HEADERS_JSON`, `IVR_STAGING_LOKI_HEADERS_JSON` và
   `IVR_STAGING_GRAFANA_HEADERS_JSON`. Không đặt header value vào manifest.
4. Chạy manual job `observability_staging_evidence` từ default branch sau khi năm automatic
   observability prerequisite đã xanh. GitLab environment `staging` phải được Platform cấu hình
   protected trước khi dùng job.

Verifier fail nếu endpoint không phải HTTPS, image không pin bằng digest, thiếu năm stage span/API/
Worker/HTTP child, thiếu metric, log thiếu trace/correlation hoặc có PII, dashboard/ảnh không hợp lệ,
alert không có cả firing và recovered state, hay retention/access chưa đủ. Artifact thành công gồm
`closure-manifest.json` đã lược bỏ raw backend response/credential và ảnh dashboard có SHA-256.

Contract test không cần staging:

```powershell
node deploy/ci/scripts/observability-staging-evidence.mjs --self-test
```

`CT-OBS-STAGING-13/14` chỉ chứng minh verifier đúng và fail-closed; chúng không phải staging
evidence và không đóng B-06.
