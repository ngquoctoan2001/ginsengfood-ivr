# PROMPT P6-3 — Chaos & Resilience Game-Days

## 0. Meta
| | |
| --- | --- |
| **ID** | `P6-3` · **Phase** 6 — Observability & Reliability |
| **Work ID** | `W-0042` (canonical tracker §5) |
| **Prereq** | `P6-2`, `P4-2`, `P4-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · chaos tooling · K8s |

## 1. ROLE
Bạn là **Senior Resilience/SRE Engineer**. Bạn chủ động phá hệ thống có kiểm soát (fault injection, chaos) để chứng minh IVR **thật sự fail-closed** và phục hồi đúng khi downstream/SIM/DB gặp sự cố — không chỉ tin vào unit test. Bạn biến "fail-closed trên giấy" thành "fail-closed đã kiểm chứng".

## 2. CONTEXT
Fail-closed (DO-06) và resilience là bất biến sống còn. Unit/integration (P5) test từng nhánh, nhưng hành vi hệ thống dưới sự cố thật (timeout dây chuyền, DB mất kết nối, SIM chập chờn, webhook trùng lặp) cần game-day. Đây là điều kiện tin cậy trước pilot/production.

## 3. SOURCE SPECS (đọc trước)
- `specs/architecture/05-resilience.md`, `specs/testing/03-integration-test-plan.md` (fail-closed profiles IT-12..17)
- `plan/ivr-orther/decisions-log.md` §DO-06 (fail-closed), §DT-04 (SIM), §D-04 (callback retry)

## 4. DECISIONS & CONSTRAINTS
- **Fail-closed under fault:** Order Core/ops/CRM/evidence down/chậm → không dispatch/không confirm sai/không mất signal.
- **SIM fault:** chập chờn/dropped → `TECHNICAL_EXCEPTION` (không no-answer), auto-disable đúng (DT-04).
- **DB/infra fault:** mất DB → `ready=503`, không mất task; phục hồi khi DB trở lại.
- **No data loss:** task/attempt/callback không mất qua sự cố; idempotency giữ.
- **Controlled:** chaos chỉ ở dev/staging (MOCK); có blast-radius limit.

## 5. INPUTS / DEPENDENCIES
- Fault injection (Toxiproxy/chaos-mesh/mã inject); integration-status profiles (seed); observability (P6-1/2).

## 6. BUILD STEPS
1. **Fault scenarios**: downstream timeout/500/503; DB drop/reconnect; SIM dropped/chậm; webhook duplicate/out-of-order; evidence store down; partial network partition.
2. **Game-day harness**: chạy scenario trên staging MOCK; đo hành vi (fail-closed? mất task? recovery time?); assert qua metric/trace (P6).
3. **Verify fail-closed**: mỗi scenario → hệ chặn an toàn, không confirm sai, không mất signal; alert fires (P6-2).
4. **Recovery**: sau khi khôi phục fault → hệ tự phục hồi, xử lý backlog đúng, không double.
5. **Runbook** ghi kết quả + điểm yếu phát hiện → vé sửa.
6. Blast-radius limit; không chạy prod.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `tests/chaos/**` | Scenario + harness |
| `docs/gameday-report.md` | Kết quả + điểm yếu |
| `deploy/chaos/**` | Fault injection config (staging) |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `CHAOS-DOWNSTREAM-01` | chaos | Core/ops down/chậm → fail-closed, không dispatch/confirm sai, alert. |
| `CHAOS-DB-02` | chaos | DB drop → `ready=503`, không mất task; reconnect → phục hồi. |
| `CHAOS-SIM-03` | chaos | SIM dropped/chập chờn → TECHNICAL (không no-answer), auto-disable (DT-04). |
| `CHAOS-RECOVERY-04` | chaos | sau fault → backlog xử lý đúng, không double, idempotency giữ. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] fail-closed kiểm chứng thực; [ ] không mất task; [ ] recovery đúng; [ ] blast-radius limited (không prod).
**Reviewer:** scenario phủ profiles IT-12..17; điểm yếu có vé sửa; alert đúng.

## 10. EVIDENCE EXPECTED
Game-day report per scenario (fail-closed proof, recovery time, no-data-loss), alert-fire capture, danh sách điểm yếu + vé.

## 11. FORBIDDEN
- ❌ Chạy chaos ở prod/khách thật. ❌ Chấp nhận mất task/confirm sai dưới fault. ❌ Coi SIM fault = no-answer. ❌ Bỏ qua điểm yếu phát hiện.

## 12. DEFINITION OF DONE
- [ ] Chaos harness + game-day report + fixes; 4 scenario §8 pass; evidence §10 đủ.
