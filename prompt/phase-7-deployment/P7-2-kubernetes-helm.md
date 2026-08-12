# PROMPT P7-2 — Kubernetes & Helm

## 0. Meta
| | |
| --- | --- |
| **ID** | `P7-2` · **Phase** 7 — Deployment |
| **Prereq** | `P7-1`, `P6-2` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Kubernetes · Helm |

## 1. ROLE
Bạn là **Senior Kubernetes/Platform Engineer**. Bạn đóng gói IVR thành Helm chart production-grade: deployment cho api/worker/ui, HPA scale theo SIM concurrency, config/secret an toàn, NetworkPolicy, probe, và CronJob retention. Bạn map governance ladder vào values per-env.

## 2. CONTEXT
IVR chạy trên K8s (DTS-04). Cần manifest chuẩn cho 4 môi trường (dev/staging/pilot/prod) với values khác nhau — đặc biệt `REAL_CUSTOMER_CALL_ALLOWED` và `IVR_ADAPTER_MODE` chỉ mở dần theo ladder. Scaling worker phải tôn trọng one-sim-one-call.

## 3. SOURCE SPECS (đọc trước)
- `specs/architecture/04-deployment-architecture.md`, `specs/architecture/05-resilience.md`
- `prompt/README-governance.md` §6 (ladder→env), `specs/database/05-retention-and-privacy.md`
- `plan/ivr-orther/decisions-log.md` §DTS-04, §DT-04 (SIM pool/concurrency), §DF-07 (retention), §DO-06 (readiness)

## 4. DECISIONS & CONSTRAINTS
- **Deployables:** `ivr-api` (HTTP), `ivr-worker` (scheduler/SIM), `ivr-admin-ui`.
- **HPA:** scale theo tải, **nhưng** worker concurrency bị chặn bởi SIM pool (one-sim-one-call) — HPA không được tạo double-dispatch (scheduler advisory lock P2-3 đảm bảo). Document rõ ràng ceiling.
- **Config/secret:** ConfigMap (non-secret) + Secret/Vault (secret); `REAL_CUSTOMER_CALL_ALLOWED` per-env values (chỉ prod/pilot sau DF-03).
- **Probe:** liveness/readiness/startup map `/health/*` (DO-06); readiness 503 → out of rotation.
- **NetworkPolicy:** least-privilege egress (chỉ Core/ops/CRM/SIM/DB/otel); ingress hạn chế.
- **Retention:** CronJob chạy `IRetentionJob` (P1-2) theo DF-07.
- **PDB, resource limits, anti-affinity** cho HA.

## 5. INPUTS / DEPENDENCIES
- Image P7-1; dashboards/alerts P6-2; secret store (`NEED_CONFIRMATION`: K8s Secret dev → Vault/KMS prod).

## 6. BUILD STEPS
1. **Helm chart** `deploy/helm/ivr/`: templates deployment×3, service, HPA, ConfigMap, Secret ref, CronJob (retention), NetworkPolicy, PDB, ServiceAccount + RBAC (K8s).
2. **values per-env** `values-{dev,staging,pilot,prod}.yaml`: image tag, replicas, HPA min/max, `REAL_CUSTOMER_CALL_ALLOWED` (false trừ prod/pilot sau gate), `IVR_ADAPTER_MODE` (MOCK trừ pilot/prod), resource limits, downstream URLs.
3. **Probes**: startup (migration/warmup), readiness (DB/downstream/adapter), liveness.
4. **Secret handling**: ExternalSecret/Vault injection prod; không secret literal trong values prod.
5. **SIM concurrency guard**: annotate/document HPA ceiling; worker leader/lock đảm bảo không double-dispatch khi scale.
6. **Retention CronJob** theo DF-07 schedule.
7. Helm lint + `helm template` + kubeconform/kubeval trong CI.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `deploy/helm/ivr/templates/**` | Deployment/Service/HPA/ConfigMap/Secret/CronJob/NetworkPolicy/PDB |
| `deploy/helm/ivr/values-*.yaml` | 4 env |
| `deploy/helm/README.md` | Install/upgrade/ladder mapping |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-K8S-LINT-01` | ci | helm lint + kubeconform pass mọi env. |
| `IT-K8S-GATE-02` | ci | values dev/staging → `REAL_CUSTOMER_CALL_ALLOWED=false` & MODE=MOCK; prod chỉ true sau flag gate. |
| `IT-K8S-PROBE-03` | integration | readiness 503 khi DB/downstream down → pod out of rotation. |
| `IT-K8S-NETPOL-04` | integration | egress ngoài allowlist bị chặn (NetworkPolicy). |
| `IT-K8S-RETENTION-05` | integration | CronJob retention chạy đúng lịch, xoá đúng class (DF-07). |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] ladder→env đúng (REAL_CALL/MODE); [ ] readiness fail-closed; [ ] NetworkPolicy least-privilege; [ ] secret không literal prod; [ ] HPA không double-dispatch.
**Reviewer:** resource limits/PDB/anti-affinity hợp lý; retention khớp DF-07; startup probe chờ migration.

## 10. EVIDENCE EXPECTED
helm lint/template output, per-env values diff (gate), readiness-503 demo, NetworkPolicy block test, CronJob run log.

## 11. FORBIDDEN
- ❌ `REAL_CUSTOMER_CALL_ALLOWED=true` ở dev/staging. ❌ Secret literal trong values prod. ❌ HPA gây double-dispatch. ❌ Egress mở toàn bộ.

## 12. DEFINITION OF DONE
- [ ] Helm chart + 4 env values + probe/netpol/retention; 5 test §8 xanh; evidence §10 đủ.
