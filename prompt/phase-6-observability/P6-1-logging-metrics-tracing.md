# PROMPT P6-1 — Logging, Metrics & Tracing (OpenTelemetry)

## 0. Meta
| | |
| --- | --- |
| **ID** | `P6-1` · **Phase** 6 — Observability & Reliability |
| **Prereq** | `P2-*` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · OpenTelemetry (OTLP) |

## 1. ROLE
Bạn là **Senior Observability Engineer**. Bạn trang bị cho IVR khả năng quan sát: structured logging, metrics nghiệp vụ, distributed tracing với correlation xuyên service — tất cả **PII-safe**. Bạn làm cho mọi cuộc gọi, attempt, callback có thể truy vết mà không lộ dữ liệu khách.

## 2. CONTEXT
IVR chạy nhiều thành phần (api/worker) + tích hợp downstream. Không quan sát được = không vận hành/điều tra được sự cố. Đây là nền cho dashboard/alert (P6-2) và điều kiện release. Correlation `X-Correlation-Id` (P0-3) mở rộng thành trace đầy đủ.

## 3. SOURCE SPECS (đọc trước)
- `specs/architecture/06-observability.md`, `specs/data/05-pii-policy.md`
- `plan/ivr-orther/decisions-log.md` §DF-05 (correlation), §D-05 (PII), §DTS-05 (OTel), §DO-06 (health)

## 4. DECISIONS & CONSTRAINTS
- **OTel:** log + metric + trace qua OTLP; resource attributes (service, version, env).
- **DF-05:** correlation propagate inbound→outbound→log/trace; trace span qua boundary (Core/ops/CRM/SIM).
- **D-05 (PII-safe):** không log/metric/trace phone thô/recording/token→số/DTMF raw; dùng mask + id (task/order/correlation).
- **Metrics nghiệp vụ:** intake theo program/payment/decision/mode/provider, attempt theo policy version, result, callback HTTP+semantic ACK (current compat tách label), no-answer/technical, channel utilization, capacity/fail-closed và latency; không dùng PII/high-cardinality IDs làm label.
- Không đo sai lệch (đo từ nguồn thật, không suy đoán).

## 5. INPUTS / DEPENDENCIES
- OTel SDK .NET; collector/backend (env — `NEED_CONFIRMATION`: Tempo/Jaeger + Prometheus + Loki hoặc APM).
- Correlation context (P0-3).

## 6. BUILD STEPS
1. **Logging**: Serilog/OTel structured; enrich `correlationId`, `taskId`, `orderId`, `program`, `attemptNo`; **PII redaction pipeline** (chặn field cấm); level chuẩn.
2. **Tracing**: ActivitySource cho intake/scheduler/dispatch/normalize/callback; propagate context qua HttpClient (W3C traceparent + correlation); span cho outbound Core/ops/CRM/SIM.
3. **Metrics**: Meter với counter/histogram nghiệp vụ (danh sách §4); export Prometheus/OTLP.
4. **Health/readiness** (nối P0-1/DO-06): `/health/live|ready|startup` phản ánh DB/adapter/downstream; `ready=503` khi không an toàn.
5. **PII guard test**: đảm bảo không rò trong log/trace/metric label.
6. Cấu hình sampling hợp lý (đủ điều tra, không nổ chi phí).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.*/Observability/**` | OTel setup, ActivitySource, Meter |
| `src/Ivr.*/Logging/PiiRedaction.cs` | Redaction pipeline |
| `src/Ivr.Api/Health/**` (mở rộng) | live/ready/startup |
| `deploy/otel/**` | Collector config (env) |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-OBS-PII-01` | unit | log/trace/metric label không chứa phone/recording/token (D-05). |
| `IT-OBS-TRACE-02` | integration | 1 task → trace liên tục qua intake→dispatch→callback với cùng correlation. |
| `UT-OBS-METRIC-03` | unit | counter/histogram nghiệp vụ phát đúng (intake decision, callback latency…). |
| `IT-OBS-HEALTH-04` | integration | downstream/DB down → `ready=503` (fail-closed). |

Trace: `specs/architecture/06`, `specs/testing/07` (PII).

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] PII-safe mọi tín hiệu; [ ] trace xuyên service; [ ] metrics nghiệp vụ đủ; [ ] health fail-closed.
**Reviewer:** correlation propagate đúng; sampling hợp lý; không đo sai lệch.

## 10. EVIDENCE EXPECTED
Trace waterfall 1 task, metrics sample, PII-redaction proof, readiness 503 demo.

## 11. FORBIDDEN
- ❌ Log/trace/metric chứa PII thô (D-05). ❌ Health luôn 200 khi downstream chết. ❌ Đo KPI suy đoán không từ nguồn.

## 12. DEFINITION OF DONE
- [ ] OTel log/metric/trace + PII redaction + health; 4 test §8 xanh; evidence §10 đủ.
