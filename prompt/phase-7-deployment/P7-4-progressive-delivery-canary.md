# PROMPT P7-4 — Progressive Delivery & Canary

## 0. Meta
| | |
| --- | --- |
| **ID** | `P7-4` · **Phase** 7 — Deployment |
| **Prereq** | `P7-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Kubernetes · Argo Rollouts/Flagger (env) |

## 1. ROLE
Bạn là **Senior Release/Platform Engineer**. Bạn nâng CD (P7-3) lên **progressive delivery**: canary/blue-green với traffic shifting theo SLO, tự động rollback khi SLO vi phạm, và phối hợp feature-flag ramp. Bạn giảm rủi ro mỗi lần deploy tới mức tối thiểu.

## 2. CONTEXT
Deploy thẳng 100% rủi ro cao — đặc biệt khi tiến gần production gọi khách thật. Cần canary: chỉ 5-10% traffic vào version mới, quan sát SLO, tăng dần hoặc rollback tự động. Kết hợp feature-flag (P0-4) để tách "deploy" khỏi "release".

## 3. SOURCE SPECS (đọc trước)
- `specs/architecture/04-deployment-architecture.md`, `specs/architecture/05-resilience.md`, `specs/architecture/06-observability.md` (SLO)
- `plan/ivr-orther/decisions-log.md` §DF-03 · `prompt/phase-6-observability/P6-2-dashboards-slo-alerting.md` (SLO)

## 4. DECISIONS & CONSTRAINTS
- **Canary theo SLO:** promote dần chỉ khi SLO (callback latency, error rate, fail-closed rate) trong ngưỡng; vi phạm → **auto-rollback**.
- **Deploy ≠ release:** feature-flag (P0-4) tách bật tính năng khỏi rollout image; `REAL_CUSTOMER_CALL_ALLOWED` vẫn qua gate riêng (P9-1).
- **Worker đặc thù:** `ivr-worker` (scheduler/SIM) — canary cẩn trọng để không double-dispatch (advisory lock P2-3); ưu tiên blue-green cho worker.
- **Rollback nhanh + an toàn**; DB migration backward-compatible (expand-contract).

## 5. INPUTS / DEPENDENCIES
- Helm (P7-2), CD (P7-3), SLO/metrics (P6-2); Argo Rollouts/Flagger (`NEED_CONFIRMATION`).

## 6. BUILD STEPS
1. **Canary cho `ivr-api`**: Rollout/Flagger với analysis theo SLO (Prometheus query); step 10%→50%→100% với gate metric; auto-rollback khi vi phạm.
2. **Blue-green cho `ivr-worker`**: tránh 2 scheduler chạy song song gây double-dispatch; switch atomic; verify advisory lock.
3. **DB migration expand-contract**: backward-compatible để canary chạy song 2 version.
4. **Flag ramp**: phối hợp bật tính năng dần qua P0-4 (không dựa vào rollout %).
5. **Rollback automation** + drill; smoke sau mỗi step.
6. Tài liệu progressive-delivery + ngưỡng SLO gate.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `deploy/rollouts/**` | Argo Rollout/Flagger config (api canary, worker blue-green) |
| `deploy/rollouts/analysis-slo.yaml` | SLO gate query |
| `docs/progressive-delivery.md` | Chiến lược + ngưỡng |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-CANARY-01` | ci | canary api promote theo SLO; vi phạm SLO → auto-rollback. |
| `IT-BG-WORKER-02` | ci | blue-green worker switch atomic; không 2 scheduler double-dispatch. |
| `IT-MIGRATE-03` | ci | migration expand-contract: 2 version chạy song không lỗi. |
| `IT-FLAG-RAMP-04` | ci | bật tính năng qua flag độc lập rollout %. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] SLO-gated canary + auto-rollback; [ ] worker không double-dispatch; [ ] migration backward-compat; [ ] deploy≠release.
**Reviewer:** ngưỡng SLO hợp lý; rollback drilled; REAL_CALL vẫn qua gate riêng.

## 10. EVIDENCE EXPECTED
Canary run (promote + auto-rollback demo), blue-green worker switch, migration dual-version proof, flag-ramp demo.

## 11. FORBIDDEN
- ❌ Canary worker gây 2 scheduler double-dispatch. ❌ Migration phá backward-compat. ❌ Tự bật REAL qua rollout (phải P9-1 gate). ❌ Promote bỏ qua SLO gate.

## 12. DEFINITION OF DONE
- [ ] Canary + blue-green + SLO-gate + auto-rollback; 4 test §8 xanh; evidence §10 đủ.
