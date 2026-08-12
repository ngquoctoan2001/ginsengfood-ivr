# PROMPT P9-1 — Release Gate Execution

## 0. Meta
| | |
| --- | --- |
| **ID** | `P9-1` · **Phase** 9 — Release & Operations |
| **Prereq** | `P8-2` (pilot pass) |
| **Governance** | Đây là bước **MỞ** `REAL_CUSTOMER_CALL_ALLOWED` cho production — chỉ sau khi mọi gate pass |
| **Stack** | Governance/process + config |

## 1. ROLE
Bạn là **Release Manager / Module 8 Owner**. Bạn chạy quy trình release gate cuối: tổng hợp evidence, xác nhận mọi điều kiện P0 đã đóng, lấy sign-off, rồi mới mở gọi khách thật ở production. Bạn không để "báo cáo hoàn thành" thay cho "gate pass" (MASTER-05).

## 2. CONTEXT
Toàn bộ P0–P8 xong: hệ chạy, tích hợp thật, quality/observability/deploy, pilot có kiểm soát pass. Bước cuối là **release gate** theo governance ladder: mở `REAL_CUSTOMER_CALL_ALLOWED` ở prod. Đây là quyết định có kiểm soát, cần evidence ACCEPTED + owner + security/privacy sign-off (DF-03).

## 3. SOURCE SPECS (đọc trước)
- `specs/testing/08-acceptance-criteria.md` (release gate + fail gate), `specs/_review/open-decisions-register.md`
- `prompt/README-governance.md` §6 (ladder)
- `plan/ivr-orther/decisions-log.md` §DF-03 (sign-off), §DF-07 (retention/legal), §DT-01/04/06 (SIM), §DS-* (đảm bảo scope COD-only)

## 4. DECISIONS & CONSTRAINTS
- **Điều kiện mở (P0 phải đóng):** SIM mua + verified (DT-01/04/06), DF-03 owner+security/privacy sign-off, Legal retention DF-07, pilot pass (P8-2), evidence packet **ACCEPTED** (không chỉ submitted).
- **MASTER-05:** completion report ≠ gate pass; evidence submitted ≠ accepted; owner reviewed ≠ signed-off.
- **Ladder:** đây là bậc cuối `SIM_INTERNAL_TEST → REAL_CUSTOMER_CALL_ALLOWED`; không nhảy cóc.
- **Scope production:** vẫn COD-only (DS-01); mọi bất biến governance giữ nguyên.
- **Kill-switch/rollback** (P7-3/P8-2) sẵn sàng trước khi mở.

## 5. INPUTS / DEPENDENCIES
- Evidence từ tất cả phase; pilot report (P8-2); sign-off DF-03; SIM procurement done.

## 6. BUILD STEPS
1. **Evidence consolidation**: gom evidence packet toàn phase (task/attempt/result/callback/admin/security/privacy/perf/pilot) vào 1 completion dossier; kiểm mọi P0/FR có evidence ACCEPTED.
2. **Gate checklist** theo `testing/08` §Release gate: smoke pass thật + evidence accepted + security/privacy review + owner sign-off + SIM mua + pilot scope duyệt.
3. **Open-decisions verify**: `open-decisions-register.md` — mọi P0 đóng (SIM, DF-03, DF-07); build items (OC1/OC2/OC3, DC-05/06, IR-CRM-01) đánh trạng thái (bật flag nếu đã build, else document giới hạn).
4. **Sign-off ritual**: owner + security/privacy ký (ghi vào `specs/decisions/`); ghi ngày, phạm vi.
5. **Flip gate**: mở `REAL_CUSTOMER_CALL_ALLOWED=true` ở **prod values** qua promotion gate (P7-3) — có approval; verify propagation; kill-switch sẵn sàng.
6. **Go/No-Go**: quyết định + rollback plan; công bố.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/release/completion-dossier.md` | Evidence tổng hợp + trace |
| `docs/release/release-gate-checklist.md` | Checklist ký |
| `specs/decisions/DF-03-signoff.md` | Sign-off record (owner+sec/privacy) |
| `deploy/helm/ivr/values-prod.yaml` (cập nhật) | `REAL_CUSTOMER_CALL_ALLOWED=true` sau gate |

## 8. TESTS TO WRITE (gate verification)
| Test ID | Loại | Assert |
| --- | --- | --- |
| `GATE-EVID-01` | verification | mọi P0/FR có evidence **ACCEPTED** (không submitted-only). |
| `GATE-SIGNOFF-02` | verification | không thể flip gate nếu thiếu sign-off DF-03 (process/tech guard). |
| `GATE-OPEN-03` | verification | open-decisions P0 = closed; build items flag đúng trạng thái. |
| `GATE-SMOKE-04` | verification | smoke thật pass bằng evidence thật (không hardcode). |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] evidence ACCEPTED không submitted; [ ] P0 đóng; [ ] sign-off có; [ ] kill-switch sẵn; [ ] scope COD-only giữ.
**Reviewer (owner+sec/privacy):** đủ điều kiện mở gọi thật; rollback rõ; không vi phạm fail-gate.

## 10. EVIDENCE EXPECTED
Completion dossier, signed gate checklist, DF-03 sign-off record, prod flag flip proof (post-approval), smoke-with-real-evidence.

## 11. FORBIDDEN
- ❌ Mở `REAL_CUSTOMER_CALL_ALLOWED` khi thiếu bất kỳ P0/sign-off (DF-03/MASTER-05). ❌ Coi report = gate pass. ❌ Bỏ kill-switch/rollback. ❌ Mở scope ngoài COD-only.

## 12. DEFINITION OF DONE
- [ ] Gate checklist ký + evidence accepted + flag mở có kiểm soát; 4 verification §8 pass; evidence §10 đủ.
