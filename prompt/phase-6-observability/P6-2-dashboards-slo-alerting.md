# PROMPT P6-2 — Dashboards, SLO & Alerting

## 0. Meta
| | |
| --- | --- |
| **ID** | `P6-2` · **Phase** 6 — Observability & Reliability |
| **Prereq** | `P6-1` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Grafana/Prometheus (env) · Alertmanager |

## 1. ROLE
Bạn là **Senior SRE**. Bạn biến metrics/trace (P6-1) thành dashboard vận hành, định nghĩa SLO/SLI, và cấu hình alert đúng-đủ (không nhiễu). Bạn đảm bảo đội vận hành thấy sức khoẻ IVR real-time và được đánh thức khi cần.

## 2. CONTEXT
Trước pilot/production, phải có "mắt và tai": dashboard trạng thái, ngưỡng SLO, và alert khi vi phạm (SIM down, fail-closed tăng, callback chậm, capacity cạn). Đây là điều kiện release (DF-03) và nền ops runbook (P9-2).

## 3. SOURCE SPECS (đọc trước)
- `specs/architecture/06-observability.md`, `specs/architecture/05-resilience.md`, `specs/ui/05-integration-status.md`
- `plan/ivr-orther/decisions-log.md` §DT-04 (SIM auto-disable/capacity), §DO-06 (fail-closed), §D-04 (callback latency)

## 4. DECISIONS & CONSTRAINTS
- **SLO đề xuất:** callback revalidate p95 ≤ 5s (D-04); intake success rate; scheduler on-time dispatch (miss-deadline = incident); SIM availability; fail-closed rate ngưỡng.
- **Alert:** SIM fail-count auto-disable (DT-04); downstream `ready=503`/fail-closed spike (DO-06); callback error/latency; capacity exhaustion; queue backlog; deadline miss.
- **Không nhiễu:** alert có ngưỡng + for-duration + severity; runbook link.
- PII-safe (không hiển thị số thật trên dashboard).

## 5. INPUTS / DEPENDENCIES
- Metrics từ P6-1; Grafana/Prometheus/Alertmanager (env — `NEED_CONFIRMATION`).

## 6. BUILD STEPS
1. **Dashboards** (as-code, JSON/Grafana): tổng quan (task/attempt/result rate), SIM pool health/utilization, callback latency + 200/422, fail-closed/capacity, queue depth + deadline adherence, downstream health.
2. **SLO/SLI**: định nghĩa + error budget; panel burn-rate.
3. **Alert rules**: theo §4, severity (page vs ticket), for-duration, annotation + runbook link.
4. **Integration status** panel khớp UI (P3-3) — nguồn thật.
5. Test alert (synthetic) + tài liệu ngưỡng.
6. Đảm bảo dashboard PII-safe.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `deploy/observability/dashboards/**` | Grafana JSON (as-code) |
| `deploy/observability/alerts/**` | Prometheus alert rules |
| `docs/slo.md` | SLO/SLI + error budget |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-SLO-ALERT-01` | integration | inject fail-closed spike → alert fires (đúng severity/for-duration). |
| `IT-SLO-SIM-02` | integration | SIM auto-disable (DT-04) → alert. |
| `IT-SLO-LAT-03` | integration | callback p95 > 5s → SLO breach alert. |
| `UT-DASH-PII-04` | unit | dashboard query/label không lộ PII. |

Trace: `specs/architecture/06`, `specs/testing/06`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] SLO đo được; [ ] alert đúng-đủ không nhiễu; [ ] runbook link; [ ] PII-safe.
**Reviewer:** ngưỡng hợp lý; severity phân tầng; dashboard as-code (versioned).

## 10. EVIDENCE EXPECTED
Dashboard screenshots, alert-fire demo (3 loại), SLO doc, PII-safe check.

## 11. FORBIDDEN
- ❌ Alert nhiễu (no for-duration/severity). ❌ Dashboard lộ PII. ❌ SLO không đo từ metric thật.

## 12. DEFINITION OF DONE
- [ ] Dashboards + SLO + alerts as-code; 4 test §8 xanh; evidence §10 đủ. **Kết thúc Phase 6: quan sát + cảnh báo sẵn sàng.**
