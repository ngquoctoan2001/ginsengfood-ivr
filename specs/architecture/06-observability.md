# ARCH-06 — Observability & Ops

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p08` · Nguồn: `phase-8/16`,`/18`; docx §16; DO-06 (health/error convention).

## 1. Metrics (docx §16)
| Metric | Ý nghĩa | Theo dõi |
| --- | --- | --- |
| `call_success_rate` | kết nối thành công | ngày/phiên/program |
| `confirm_rate` | tỷ lệ bấm `1` / task đủ điều kiện | chất lượng đơn/contact |
| `cancel_rate` | tỷ lệ bấm `0` | phát hiện đơn ảo/đặt nhầm |
| `no_answer_rate` | không nghe sau policy | chất lượng lead/contact |
| `technical_exception_rate` | lỗi kỹ thuật | cảnh báo SIM/server/DTMF |
| `missed_deadline_count` | đơn quá window chưa gọi kịp | capacity incident |
| `sim_failure_rate` | SIM lỗi theo slot | tự disable/thay SIM |
| `cost_per_confirmed_order` | chi phí/đơn xác nhận | tài chính vận hành |

## 2. Health probes và telemetry surface (đồng bộ convention ops — DO-06)

`GET /health/live` (process), `GET /health/ready` (503 nếu DB/dep unhealthy) và
`GET /health/startup`. Order Core dùng `/health/ready` để fail-closed khi revalidate.

Không có `GET /metrics` trên API. API endpoint đó sẽ bỏ sót toàn bộ metric của Worker và tạo hai
surface vận hành khác nhau. API và Worker đều xuất trace, metric và log qua OTLP; collector/backend
là Prometheus surface chuẩn. Collector mất kết nối không được làm business request, liveness hoặc
readiness fail.

## 3. Tracing, log & audit

- W3C `TraceId` được capture tại `ivr.intake`, lưu nullable cùng task và dùng làm parent cho
  `ivr.eligibility.evaluate` → `ivr.scheduler.dispatch` → `ivr.result.normalize` →
  `ivr.callback.deliver`. Task cũ thiếu context vẫn chạy với `ivr.trace_context_missing=true`.
- `X-Correlation-Id` xuyên chuỗi. `task/job/attempt/callback` là trace/log attribute để điều tra,
  không bao giờ là metric label.
- OTLP log chỉ xuất template/attribute allowlist qua `PiiGuard`; không xuất raw phone, địa chỉ,
  DTMF, payload, token, header/credential, formatted message hoặc exception message.
- Audit append-only (TECH-01) cho: intake, eligibility, attempt dispatch, SIM reserve/release, DTMF, normalization, callback sent/ack/reject, admin action, technical exception, capacity incident.
- Blocker evidence kèm `sale_lock_id`/`recall_case_id` (DO-07).

Sampling là `ParentBased(TraceIdRatioBased)`: dev/lab/staging mặc định `1.0`, production `0.1`, có
thể override qua cấu hình. Endpoint/protocol/header dùng chuẩn `OTEL_EXPORTER_OTLP_*`; bật
observability với endpoint sai phải fail host startup.

## 4. Alerts
- SIM `fail_count≥3/10′` → disable + alert. 
- `capacity_incident` mở (pending/expired/missed-deadline vượt ngưỡng).
- Ops/Core down khi revalidate (fail-closed) → alert.
- Callback retry exhausted → admin review.

## 5. Admin dashboard (phase-8/08, chi tiết ở UI specs p12)
Queue/capacity/SIM health/incidents; call-job detail (masked); audit/evidence. **Không** raw phone/full profile; không force order.

## 6. Runbook (phase-8/18) — tóm tắt
- Pause/resume queue (có incident); disable/enable SIM (health); manual technical retry; admin review — đều RBAC + audit + `no_policy_bypass`.
- SIM chưa mua: chạy MOCK; runbook thật cập nhật khi có gateway (DT-01).
