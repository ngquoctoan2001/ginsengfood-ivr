# PROMPT P7-3 — CD Pipeline & Environment Promotion

## 0. Meta
| | |
| --- | --- |
| **ID** | `P7-3` · **Phase** 7 — Deployment |
| **Prereq** | `P7-2`, `P5-4` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | CI/CD (GitHub Actions default) · Helm · K8s |

## 1. ROLE
Bạn là **Senior CD/Release Engineer**. Bạn nối CI (P0-2/P5-4) thành pipeline giao hàng: build→test→scan→push image→deploy staged theo môi trường, với **governance gate** chặn mở `REAL_CUSTOMER_CALL_ALLOWED` cho tới khi đủ điều kiện. Bạn làm deploy an toàn, có rollback.

## 2. CONTEXT
Có image (P7-1) + Helm (P7-2) + quality gate (P5-4). Cần tự động hoá promotion qua dev→staging→pilot→prod, mỗi bậc có gate. Governance ladder (README §6) map thành cổng pipeline — không nhảy cóc, không tự bật gọi thật.

## 3. SOURCE SPECS (đọc trước)
- `prompt/README-governance.md` §6 (ladder→env), `specs/architecture/04-deployment-architecture.md`, `specs/testing/08-acceptance-criteria.md` §Release gate
- `plan/ivr-orther/decisions-log.md` §DF-03 (release gate sign-off), §DTS-05 (CI/CD)

## 4. DECISIONS & CONSTRAINTS
- **Ladder→env:** `DOCS/CONTRACT_APPROVED`→dev; `TASK_INTAKE/SCHEDULER_ENABLED`→staging (MOCK); `SIM_INTERNAL_TEST`→pilot-prep; **`REAL_CUSTOMER_CALL_ALLOWED`→prod chỉ sau DF-03 sign-off + mua SIM**.
- **Gate:** promotion cần CI xanh + quality gate (P5-4) + (staging→pilot/prod) **approval thủ công** (DF-03).
- **Deploy an toàn:** helm upgrade với health check, rollback tự động khi fail, smoke sau deploy.
- **Immutable image:** tag = semver+sha; không mutate.
- **Không tự bật REAL:** pipeline không set `REAL_CUSTOMER_CALL_ALLOWED=true` trừ manual gate prod có sign-off.

## 5. INPUTS / DEPENDENCIES
- CI P0-2/P5-4; Helm P7-2; registry; K8s cluster creds (per env, CI secret); approval mechanism (environments/protection rules).

## 6. BUILD STEPS
1. **Build+publish**: on tag/main → build 3 image, test, scan (fail High/Critical), push với tag immutable.
2. **Deploy dev**: auto helm upgrade values-dev; smoke; rollback nếu fail.
3. **Promote staging**: sau dev xanh → deploy staging (MOCK); E2E/smoke; rollback on fail.
4. **Promote pilot/prod**: **manual approval gate** (DF-03) + kiểm điều kiện (SIM mua, evidence ACCEPTED); deploy values-pilot/prod; canary/blue-green (optional); smoke; auto-rollback.
5. **Governance enforcement**: pipeline kiểm `REAL_CUSTOMER_CALL_ALLOWED` chỉ true ở prod values sau approval; step verify không bật ở env thấp.
6. **Post-deploy smoke** + alert wiring (P6-2); mark evidence.
7. Rollback runbook tự động + thủ công.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `deploy/ci/cd.yml` | Build→scan→push→deploy staged + gates |
| `deploy/ci/promote.yml` | Manual approval promotion (DF-03) |
| `deploy/ci/rollback.md` + script | Rollback |
| `deploy/ci/README.md` (mở rộng) | Ladder→env mapping |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-CD-DEV-01` | ci | push → build/scan/push/deploy dev + smoke pass; scan fail chặn. |
| `IT-CD-GATE-02` | ci | promote pilot/prod **cần manual approval** (DF-03); không auto. |
| `IT-CD-REAL-03` | ci | pipeline không set `REAL_CUSTOMER_CALL_ALLOWED=true` ngoài prod-approved. |
| `IT-CD-ROLLBACK-04` | ci | deploy fail health → auto rollback về revision trước. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] ladder→env enforced; [ ] manual gate pilot/prod; [ ] không tự bật REAL; [ ] rollback hoạt động; [ ] scan chặn.
**Reviewer:** immutable tag; creds qua CI secret; canary/rollback an toàn.

## 10. EVIDENCE EXPECTED
Pipeline run (dev auto, staging, gated pilot/prod), approval gate demo, rollback demo, scan-block, REAL-not-set proof.

## 11. FORBIDDEN
- ❌ Auto-promote prod không approval (DF-03). ❌ Tự bật `REAL_CUSTOMER_CALL_ALLOWED`. ❌ Mutable tag. ❌ Deploy khi scan/test đỏ.

## 12. DEFINITION OF DONE
- [ ] CD staged + gates + rollback; 4 test §8 xanh; evidence §10 đủ. **Kết thúc Phase 7: deploy K8s an toàn, staged, có gate — vẫn MOCK tới pilot.**
