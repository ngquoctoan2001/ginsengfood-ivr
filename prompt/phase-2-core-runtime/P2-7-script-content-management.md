# PROMPT P2-7 — Script / Content Management

## 0. Meta
| | |
| --- | --- |
| **ID** | `P2-7` · **Phase** 2 — Core Runtime |
| **Prereq** | `P2-1` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · PostgreSQL |

## 1. ROLE
Bạn là **Senior .NET Backend Engineer**. Bạn xây dịch vụ quản lý **call script/template**: lưu trữ, versioning, quy trình duyệt (approve), biến (variables) cho phép, và khung A/B. Intake (P2-1) chỉ được dùng script **approved** — bạn cung cấp nguồn sự thật đó.

## 2. CONTEXT
Nhiều prompt tham chiếu "call_script_template_id/version/approved" và "allowed_script_variables" nhưng chưa có nơi *xây*. Script nói với khách nội dung gì là nhạy cảm (compliance) → cần duyệt trước khi dùng, versioning để truy vết, và biến an toàn (không chèn PII/nội dung tuỳ tiện). A/B để tối ưu tỉ lệ xác nhận.

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/04-call-execution-dtmf.md` (script + variables), `specs/ui/04-ivr-menu-config.md`
- `specs/data/05-pii-policy.md`
- `plan/ivr-orther/decisions-log.md` §AS-07 (KEY_9 disabled), §D-05 (PII), §DF-01 (RBAC approve)

## 4. DECISIONS & CONSTRAINTS
- **Approved-only:** intake/dispatch chỉ dùng script `status=APPROVED` + version cụ thể; draft không ra gọi.
- **Versioning:** immutable version; đổi = version mới + re-approve.
- **Allowed variables:** whitelist biến (order_code_short, total_amount_display…) — **không** biến chứa PII thô/địa chỉ đầy đủ (D-05).
- **Approve workflow:** RBAC (`IVR_RESULT_REVIEW`/quyền script) + audit + reason (DF-01).
- **A/B (khung):** gán script variant theo policy (không hardcode); ghi variant vào result để đo (feed P10-4).
- KEY_9 handoff = NOT_ENABLED (AS-07) — script không mời bấm 9.

## 5. INPUTS / DEPENDENCIES
- DB (P1-2) thêm `ivr_scripts`/`ivr_script_versions`; foundation RBAC/audit (P0-3).

## 6. BUILD STEPS
1. Entity + migration `ivr_scripts`, `ivr_script_versions{script_id, version, body_template, allowed_variables[], status(DRAFT/APPROVED/RETIRED), approved_by, approved_at}`.
2. `ScriptService`: CRUD draft, submit→approve (RBAC+audit+reason), retire; resolve **approved** version cho intake/dispatch.
3. **Variable guard**: chỉ render allowed variables; reject template chứa biến ngoài whitelist/PII (D-05).
4. **A/B assignment**: policy chọn variant (vd theo bucket order_id) → ghi `script_version`/`variant` vào result; không hardcode.
5. Intake (P2-1) + SIM play (P2-4) đọc script qua service; script chưa approved → intake `TASK_REJECTED_SCRIPT_NOT_APPROVED`.
6. Preview an toàn (không PII) cho UI (P3-3).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Domain/Scripts/**`, `src/Ivr.Infrastructure/Scripts/ScriptService.cs` | Domain + service |
| migration `ivr_scripts`, `ivr_script_versions` | Store |
| `src/Ivr.Domain/Scripts/VariableGuard.cs` | Whitelist biến |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-SCRIPT-APPROVE-01` | unit | draft không dùng được; approve (RBAC+reason+audit) → dùng được; version immutable. |
| `UT-SCRIPT-VAR-02` | unit | biến ngoài whitelist/PII → reject (D-05). |
| `UT-SCRIPT-INTAKE-03` | integration | intake với script chưa approved → `SCRIPT_NOT_APPROVED`. |
| `UT-SCRIPT-AB-04` | unit | A/B gán variant theo policy, ghi vào result; không hardcode. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] approved-only; [ ] version immutable; [ ] variable whitelist (no PII); [ ] approve audit+reason; [ ] KEY_9 không mời.
**Reviewer:** A/B đo được (feed analytics); intake gate đúng; preview no-PII.

## 10. EVIDENCE EXPECTED
Approve workflow audit, variable-reject sample, intake reject script-not-approved, A/B variant assignment log.

## 11. FORBIDDEN
- ❌ Dùng script chưa approved ra gọi. ❌ Biến chứa PII thô (D-05). ❌ Mời bấm KEY_9 (AS-07). ❌ Sửa version đã approved (phải version mới).

## 12. DEFINITION OF DONE
- [ ] Script service + approve + versioning + A/B khung; 4 test §8 xanh; evidence §10 đủ.
