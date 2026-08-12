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

## 2. Health probes (đồng bộ convention ops — DO-06)
`GET /health/live` (process), `GET /health/ready` (503 nếu DB/dep unhealthy), `GET /health/startup`, `GET /metrics` (Prometheus). Order Core dùng `/health/ready` để fail-closed khi revalidate.

## 3. Tracing & audit
- `X-Correlation-Id` xuyên chuỗi; mỗi bước log `task_id/order_id/correlation_id/idempotency_key/evidence_ref`; `order_version` log optional/target khi IR-SALES-OC1 expose.
- Audit append-only (TECH-01) cho: intake, eligibility, attempt dispatch, SIM reserve/release, DTMF, normalization, callback sent/ack/reject, admin action, technical exception, capacity incident.
- Blocker evidence kèm `sale_lock_id`/`recall_case_id` (DO-07).

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
