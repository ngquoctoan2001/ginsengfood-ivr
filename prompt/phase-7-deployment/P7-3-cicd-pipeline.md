# PROMPT P7-3 — CD Pipeline & Environment Promotion

## 0. Meta
| | |
| --- | --- |
| **ID** | `P7-3` · **Phase** 7 — Deployment |
| **Work ID** | `W-0045` (canonical tracker §5) |
| **Prereq** | `P7-2`, `P5-4` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | GitLab CI/CD (`CONFIRMED_2026-08-12`) · Helm · K8s |

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
- GitLab CI P0-2/P5-4; Helm P7-2; GitLab Container Registry hoặc registry được owner chốt; K8s cluster credentials theo environment trong masked/protected CI/CD variables; protected environments/manual job approval.

## 6. BUILD STEPS
1. Mở rộng root `.gitlab-ci.yml` bằng fragment `deploy/ci/cd.gitlab-ci.yml`; dùng `rules` cho protected tag/default branch và `needs` để giữ dependency graph rõ.
2. **Build+publish**: trên protected tag/default branch → build 3 image, test, scan (fail High/Critical), push với tag immutable. Ghi registry digest vào artifact/evidence; không chỉ ghi mutable tag.
3. **Deploy dev**: auto `helm upgrade --install` với values-dev; smoke; rollback nếu fail. Dùng GitLab `environment` và `resource_group` để tránh hai deployment cùng môi trường chạy đồng thời.
4. **Promote staging**: sau dev xanh → deploy staging (MOCK); E2E/smoke; rollback on fail.
5. **Promote pilot/prod**: job `when: manual`, `allow_failure: false`, protected environment và authorized approver; kiểm điều kiện SIM/evidence `ACCEPTED`; deploy values tương ứng; canary/blue-green nếu được chọn; smoke; auto-rollback.
6. **Governance enforcement**: pipeline kiểm `REAL_CUSTOMER_CALL_ALLOWED` chỉ true trong protected production promotion sau approval; verify job không bật ở môi trường thấp.
7. **Post-deploy smoke** + alert wiring (P6-2); publish JUnit/evidence artifact và environment/deployment link.
8. Rollback runbook tự động + thủ công; rollback job production cũng phải protected/audited.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `.gitlab-ci.yml` (mở rộng) | Include CD fragments và routing root |
| `deploy/ci/cd.gitlab-ci.yml` | Build→scan→push→deploy staged + gates |
| `deploy/ci/promote.gitlab-ci.yml` | Manual protected-environment promotion (DF-03) |
| `deploy/ci/rollback.md` + script | Rollback |
| `deploy/ci/README.md` (mở rộng) | Ladder→env mapping |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-CD-DEV-01` | gitlab-ci | protected default-branch push → build/scan/push/deploy dev + smoke pass; scan fail chặn. |
| `IT-CD-GATE-02` | gitlab-ci | promote pilot/prod cần manual job + protected-environment authorization (DF-03); không auto. |
| `IT-CD-REAL-03` | gitlab-ci | pipeline không set `REAL_CUSTOMER_CALL_ALLOWED=true` ngoài protected prod-approved job. |
| `IT-CD-ROLLBACK-04` | gitlab-ci | deploy fail health → auto rollback về revision trước; evidence giữ nguyên. |
| `IT-CD-CONCURRENCY-05` | gitlab-ci | `resource_group` serialize deployment cùng environment; pipeline superseded không deploy chồng. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] ladder→env enforced; [ ] manual/protected gate pilot/prod; [ ] không tự bật REAL; [ ] rollback hoạt động; [ ] scan chặn; [ ] deployment serialized.
**Reviewer:** digest immutable; credentials chỉ qua masked/protected variables; protected environment/approver đúng; canary/rollback an toàn.

## 10. EVIDENCE EXPECTED
GitLab pipeline run (dev auto, staging, gated pilot/prod), protected-environment approval evidence, rollback demo, scan-block, digest và REAL-not-set proof. Nếu chưa có runner/cluster/registry/credential, từng evidence phải là `NOT_RUN` hoặc `BLOCKED_EXTERNAL`; YAML/local simulation không được gọi là deploy proof.

## 11. FORBIDDEN
- ❌ Auto-promote prod không approval (DF-03). ❌ `allow_failure: true` cho promotion gate. ❌ Tự bật `REAL_CUSTOMER_CALL_ALLOWED`. ❌ Mutable tag. ❌ Deploy khi scan/test đỏ. ❌ Tạo GitHub Actions workflow.

## 12. DEFINITION OF DONE
- [ ] GitLab CD staged + gates + rollback; 5 test §8 xanh; evidence §10 đủ theo đúng lớp local/hosted/runtime. **Kết thúc Phase 7: deploy K8s an toàn, staged, có gate — vẫn MOCK tới pilot.**
