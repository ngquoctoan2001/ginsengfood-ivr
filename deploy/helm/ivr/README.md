# IVR Helm observability values

`observability.enabled` mặc định `false`. Khi bật, chart yêu cầu đồng thời:

- `endpoint` là URL HTTP(S) OTLP tuyệt đối;
- `protocol` là `grpc` hoặc `http/protobuf`;
- `traceSamplingRatio` trong `0..1`;
- cả `collector.namespaceLabels` và `collector.podLabels` để NetworkPolicy không mở wildcard/CIDR;
- `headersSecret.existingSecret` và `headersSecret.key` phải cùng có hoặc cùng vắng.

Chart đưa cùng cấu hình vào API và Worker, với `service.name` riêng. Header bí mật chỉ đi qua
`secretKeyRef`; không lưu giá trị thật trong values. Fixture positive/negative nằm trong `ci/` và
được job `observability_helm` chạy fail-closed.
