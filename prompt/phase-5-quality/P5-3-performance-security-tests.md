# PROMPT P5-3 — Performance, Load & Security/Privacy Tests

## 0. Meta
| | |
| --- | --- |
| **ID** | `P5-3` · **Phase** 5 — Quality Engineering |
| **Prereq** | `P2-*`, `P4-*` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · k6/NBomber · security tooling |

## 1. ROLE
Bạn là **Senior Performance & Security Test Engineer**. Bạn kiểm IVR chịu tải theo capacity SIM, ổn định qua soak, và an toàn về bảo mật/privacy (PII, fail-closed, authz). Bạn tìm điểm gãy trước khi khách hàng gãy.

## 2. CONTEXT
IVR bị chặn bởi SIM concurrency (one-sim-one-call) và deadline attempt. Cần biết throughput thật, hành vi khi quá capacity (không được biến technical thành no-answer, không mất signal), và không rò PII. Đây là bằng chứng an toàn cho pilot/production.

## 3. SOURCE SPECS (đọc trước)
- `specs/testing/06-performance-test-plan.md`, `specs/testing/07-security-privacy-test-plan.md`
- `specs/architecture/05-resilience.md`, `specs/data/05-pii-policy.md`
- `plan/ivr-orther/decisions-log.md` §DT-04 (capacity/cooldown), §DO-06 (fail-closed), §D-05 (PII)

## 4. DECISIONS & CONSTRAINTS
- **Capacity:** simulate 1-channel lab and 32-channel target with multiple policy versions; real 32-eSIM proof remains vendor/load-gate evidence. Overload → `IVR_CAPACITY_EXCEPTION` not-counted, stable queue/outbox.
- **Fail-closed (DO-06):** dưới tải, downstream chậm/timeout → block/không dispatch, không "mở cửa".
- **PII (D-05):** scan log/metric/trace/UI không lộ phone thô/recording/token→số.
- **Perf target:** callback revalidate 3–5s (D-04); intake/scheduler latency ngưỡng; no memory leak qua soak.
- **Security:** authz (allowlist/RBAC), injection, secret exposure, rate-limit.

## 5. INPUTS / DEPENDENCIES
- Load tool (k6/NBomber); mock SIM scale; security scanners (OWASP ZAP baseline, dotnet vuln, gitleaks — nối P0-2); log/trace sink.

## 6. BUILD STEPS
1. **Load/throughput**: run 1- and 32-channel simulations plus alternate attempt policies; measure latency/queue/deadlines and one-channel-one-call.
2. **Capacity/backpressure**: đẩy quá capacity → `CAPACITY_EXCEPTION` (not counted), không mất task, phục hồi khi tải giảm.
3. **Soak**: chạy dài (vd 4–8h mock) → không leak memory/connection; deadline không trôi.
4. **Resilience under load**: downstream chậm/timeout → fail-closed (không dispatch/không confirm sai); technical≠no-answer giữ đúng dưới tải.
5. **Security/privacy**: PII scan toàn log/trace/UI; authz negative (caller lạ, thiếu scope); rate-limit; secret exposure; error không leak stack/PII.
6. Report ngưỡng đạt/không + điểm nghẽn.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `tests/perf/**` (k6/NBomber) | Load/capacity/soak scripts |
| `tests/security/**` | Authz/PII/injection scripts + scan config |
| `docs/perf-security-report.md` | Kết quả + ngưỡng |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `PT-CAP-01` | perf | vượt capacity → `CAPACITY_EXCEPTION` not-counted, không mất task. |
| `PT-SOAK-02` | perf | soak dài → không leak; deadline giữ. |
| `PT-FAILCLOSED-03` | perf | downstream chậm → fail-closed under load. |
| `SEC-PII-04` | security | no raw phone/full address/recording/token mapping in log/trace/UI/evidence; speech summary whitelist enforced. |
| `SEC-AUTHZ-05` | security | caller lạ/thiếu scope → 403; rate-limit hoạt động. |
| `SEC-ERR-06` | security | error không leak stack/PII; envelope chuẩn. |

Trace: `specs/testing/06`, `07`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] capacity/soak/fail-closed đạt; [ ] PII scan sạch; [ ] authz negative pass; [ ] technical≠no-answer under load.
**Reviewer:** ngưỡng perf hợp lý; scan coverage đủ; không mất signal khi tải.

## 10. EVIDENCE EXPECTED
Load/soak report (throughput/latency/leak), capacity behavior, fail-closed-under-load, PII scan clean, authz/rate-limit results.

## 11. FORBIDDEN
- ❌ Test với PII thật/khách thật (MOCK). ❌ Chấp nhận mất task khi quá tải. ❌ Bỏ qua PII leak. ❌ "Mở cửa" khi downstream chậm.

## 12. DEFINITION OF DONE
- [ ] Perf + security suite + report; test §8 đạt ngưỡng; evidence §10 đủ.
