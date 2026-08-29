# W-0139 staging observability handoff

Trạng thái: `OWNER_DATA_REQUIRED / NOT_RUN`

Tài liệu này là handoff cho Platform. Nó không chứa endpoint hay credential thật và không phải bằng
chứng staging. Chỉ artifact sinh bởi manual job trên protected staging environment mới được review
để đóng B-06.

## 1. Platform chuẩn bị

- Deploy đúng Git SHA và cả hai image API/Worker pin bằng `@sha256:`.
- Bật Helm `observability` với OTLP endpoint, protocol, namespace selector, pod selector và
  `headersSecret` reference thật; không đưa header value vào values hoặc manifest evidence.
- Xác nhận API/Worker chạy `IVR_ADAPTER_MODE=MOCK` và `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- Cấp quyền read-only để query Tempo, Prometheus, Loki và Grafana.
- Chốt retention ngày cho metrics/traces/logs, nhóm được đọc, owner và approval reference.
- Cấu hình GitLab environment `staging` là protected.

## 2. Tạo input không chứa secret

Sao chép `deploy/observability/staging-evidence.template.json` ra file ngoài repository, rồi điền:

- exact Git SHA, API image digest, Worker image digest và deployment capture reference;
- OTLP endpoint/protocol, collector selectors và Kubernetes secret **reference**;
- task ID, correlation ID, TraceId của đúng một MOCK task;
- observation window và HTTPS query URL của bốn backend;
- alert name cùng cửa sổ có ít nhất một trạng thái firing và trạng thái recovered tại cuối cửa sổ;
- retention/access approval metadata.

Không thêm field ngoài template. Verifier từ chối token, password, authorization, header hoặc secret
value trong input.

## 3. GitLab variables

| Variable | Loại | Bắt buộc | Nội dung |
| --- | --- | --- | --- |
| `IVR_STAGING_EVIDENCE_INPUT_FILE` | File | Có | Manifest không secret ở §2 |
| `IVR_STAGING_DASHBOARD_SCREENSHOT_FILE` | File | Có | PNG/JPEG dashboard của đúng observation window |
| `IVR_STAGING_TEMPO_HEADERS_JSON` | Masked | Khi backend yêu cầu | JSON object chứa query headers |
| `IVR_STAGING_PROMETHEUS_HEADERS_JSON` | Masked | Khi backend yêu cầu | JSON object chứa query headers |
| `IVR_STAGING_LOKI_HEADERS_JSON` | Masked | Khi backend yêu cầu | JSON object chứa query headers |
| `IVR_STAGING_GRAFANA_HEADERS_JSON` | Masked | Khi backend yêu cầu | JSON object chứa query headers |

Không bật debug shell tracing và không in các biến masked trong job log.

## 4. Thứ tự chạy

1. Chờ `observability_rules`, `observability_contract`, `observability_helm`,
   `observability_runtime` và `observability_staging_contract` xanh trên default branch.
2. Tạo đúng một MOCK task và ghi task/correlation/TraceId vào input.
3. Thực hiện controlled alert drill đã được Platform cho phép; ghi cửa sổ firing/recovery.
4. Trigger manual job `observability_staging_evidence`.
5. Tải `artifacts/observability/staging/closure-manifest.json` và ảnh dashboard.

## 5. Điều kiện review để đóng B-06

- manifest có `status=B06_STAGING_EVIDENCE_PASS`;
- exact SHA và hai image digest khớp deployment capture của cluster;
- trace đủ năm stage, có API + Worker và outbound HTTP child;
- bốn metric query có kết quả;
- log có TraceId/correlation ID, PII scan pass và không persist credential/raw backend response;
- dashboard UID đúng, ảnh có SHA-256;
- alert có firing sample và `recoveredAtEnd=true`;
- retention/access metadata có owner và approval reference.

Sau review mới đổi B-06 sang đóng và chỉ gạch phần observability endpoint/credential của W-0063.
Các residual registry/K8s/secret store/progressive delivery/warehouse của W-0063 vẫn mở. Không dùng
artifact này để tuyên bố production-ready hoặc cho phép gọi khách hàng thật.
