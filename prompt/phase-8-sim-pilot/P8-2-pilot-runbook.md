# PROMPT P8-2 — Pilot Runbook (Limited Real Customers)

## 0. Meta
| | |
| --- | --- |
| **ID** | `P8-2` · **Phase** 8 — SIM Pilot |
| **Prereq** | `P8-1`, `P7-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED` = **chỉ bật cho pilot scope hạn chế sau DF-03** · `IVR_ADAPTER_MODE=REAL` (pilot) |
| **Stack** | K8s pilot env · runbook |

## 1. ROLE
Bạn là **Senior SRE / Release Manager**. Bạn thiết kế và vận hành **pilot có kiểm soát**: gọi một tập nhỏ khách thật, thu evidence, có kill-switch tức thời và rollback. Bạn coi an toàn khách hàng + tuân thủ là trên hết; pilot để học, không để "chạy thật đại trà".

## 2. CONTEXT
Sau khi REAL adapter verified (P8-1) và deploy pilot env (P7-3), pilot mở `REAL_CUSTOMER_CALL_ALLOWED` cho **scope hạn chế** (DF-03) để kiểm chứng thực địa trước production. Đây là bước rủi ro cao nhất — cần guardrail chặt: giới hạn số lượng, giờ, kill-switch, giám sát sát sao.

## 3. SOURCE SPECS (đọc trước)
- `specs/testing/08-acceptance-criteria.md` (release gate), `specs/architecture/05-resilience.md`, `specs/functional/08-evidence-audit-privacy.md`
- `plan/ivr-orther/decisions-log.md` §DF-03 (sign-off + pilot scope), §DT-05 (recording OFF), §DF-07 (retention), §DO-06 (fail-closed)

## 4. DECISIONS & CONSTRAINTS
- **DF-03:** pilot scope + `REAL_CUSTOMER_CALL_ALLOWED` bật chỉ sau **owner sign-off + security/privacy review**; giới hạn số khách/ngày/giờ.
- **Kill-switch:** tắt gọi thật tức thời (flag/config) → về MOCK/loopback; rollback deploy.
- **Recording OFF** (DT-05); **retention** theo DF-07; consent/legal đã duyệt.
- **Fail-closed** (DO-06) áp dụng nghiêm khi pilot.
- **Evidence:** mọi cuộc gọi pilot có evidence packet (task/attempt/result/callback/audit) để review.

## 5. INPUTS / DEPENDENCIES
- Pilot env (P7-3) + REAL adapter (P8-1); DF-03 sign-off; SIM pool thật; danh sách pilot scope (khách/số lượng/giờ) đã duyệt.
- Dashboards/alerts (P6-2) + on-call.

## 6. BUILD STEPS
1. **Pilot config**: feature flag `REAL_CUSTOMER_CALL_ALLOWED` bật **chỉ pilot env**, giới hạn (max calls/day, allowed hours theo program, whitelist scope); enforce ở code + config.
2. **Kill-switch**: cơ chế tắt tức thời (flag flip + verify propagation) + rollback helm; test kill-switch trước khi gọi thật.
3. **Runbook** `docs/pilot-runbook.md`: pre-flight checklist (sign-off, SIM, health, alerts, kill-switch test), quy trình chạy, tiêu chí abort, escalation, on-call rota.
4. **Monitoring sát**: dashboard pilot riêng, alert nhạy hơn (bất kỳ fail-closed spike/technical bất thường → cân nhắc dừng).
5. **Evidence capture**: mỗi cuộc gọi pilot → evidence packet + review hằng ngày; báo cáo pilot.
6. **Exit criteria**: định nghĩa điều kiện pilot pass → chuyển production (P9) hoặc rollback/học lại.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `deploy/helm/ivr/values-pilot.yaml` (mở rộng) | Pilot limits + flag |
| `src/Ivr.*/FeatureFlags/PilotGuard.cs` | Enforce scope/limit/kill-switch |
| `docs/pilot-runbook.md`, `docs/pilot-report-template.md` | Runbook + báo cáo |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-PILOT-LIMIT-01` | integration | vượt max calls/day hoặc ngoài allowed hours/scope → không gọi. |
| `IT-PILOT-KILL-02` | integration | kill-switch → dừng gọi thật tức thời, về MOCK/loopback. |
| `IT-PILOT-EVID-03` | integration | mỗi cuộc gọi pilot sinh evidence packet đầy đủ. |
| `IT-PILOT-FAILCLOSED-04` | integration | fail-closed spike → alert + (tuỳ) auto-pause. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] scope/limit enforce; [ ] kill-switch verified TRƯỚC khi gọi thật; [ ] recording OFF; [ ] evidence mỗi call; [ ] sign-off DF-03 có.
**Reviewer:** guardrail đủ chặt; abort criteria rõ; on-call sẵn; retention/legal ok.

## 10. EVIDENCE EXPECTED
Pilot pre-flight checklist ký, kill-switch test log, limit-enforce demo, sample evidence packet, pilot daily report.

## 11. FORBIDDEN
- ❌ Mở REAL ngoài pilot scope/không sign-off (DF-03). ❌ Bỏ kill-switch test trước gọi thật. ❌ Bật recording không consent (DT-05). ❌ Bỏ qua fail-closed khi pilot.

## 12. DEFINITION OF DONE
- [ ] Pilot guard + kill-switch + runbook + evidence; 4 test §8 xanh; pre-flight ký; evidence §10 đủ. **Kết thúc Phase 8: pilot có kiểm soát, sẵn sàng đánh giá lên production.**
