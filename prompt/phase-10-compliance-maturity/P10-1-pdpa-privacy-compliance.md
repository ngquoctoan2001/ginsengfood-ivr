# PROMPT P10-1 — PDPA / Privacy Compliance & Consent

## 0. Meta
| | |
| --- | --- |
| **ID** | `P10-1` · **Phase** 10 — Compliance & Maturity |
| **Prereq** | `P0-3`, `P4-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · Legal gate trước PROD |
| **Stack** | .NET 10 · legal/process |

## 1. ROLE
Bạn là **Privacy/Compliance Engineer** phối hợp Legal. Bạn đảm bảo IVR tuân thủ pháp luật bảo vệ dữ liệu (PDPA/Nghị định VN) khi gọi khách: cơ sở pháp lý cuộc gọi transactional, tôn trọng do-not-call, xử lý quyền chủ thể dữ liệu (DSAR), và đánh giá tác động (PIA). Đây là **gate pháp lý** trước production.

## 2. CONTEXT
Gọi khách hàng = xử lý dữ liệu cá nhân. Không tuân thủ = rủi ro pháp lý + phạt. IVR đã fail-safe kỹ thuật (do-not-call, PII mask, retention), nhưng cần khung compliance chính thức: legal basis, PIA, DSAR, và phối hợp Legal ký (DF-07). Prompt này biến compliance từ ngầm định thành có bằng chứng.

## 3. SOURCE SPECS (đọc trước)
- `specs/data/05-pii-policy.md`, `specs/functional/08-evidence-audit-privacy.md`, `specs/database/05-retention-and-privacy.md`
- `plan/ivr-orther/decisions-log.md` §DC-01..04 (consent/do-not-call), §D-05, §DF-07 (retention), §DT-05 (recording) · `plan/ivr-orther/production-blockers-plan.md` §C

## 4. DECISIONS & CONSTRAINTS
- **Legal basis:** cuộc gọi confirm COD = transactional/hợp đồng — xác lập cơ sở pháp lý (phối hợp Legal); do-not-call/consent theo CRM registry (DC-01).
- **PII minimization (D-05):** không lưu raw phone/recording; token TTL ≤ window; log mask.
- **DSAR:** hỗ trợ quyền truy cập/xoá dữ liệu cá nhân (theo phạm vi IVR giữ: audit/evidence) — quy trình + API/hỗ trợ.
- **Retention (DF-07):** duration từng loại ký với Legal; retention job (P1-2/P7-2) thực thi.
- **PIA:** đánh giá tác động privacy trước go-live.
- **Recording OFF** (DT-05) trừ khi có consent+legal.

## 5. INPUTS / DEPENDENCIES
- Legal (cơ sở pháp lý, retention ký); CRM consent registry (P4-3); data inventory (P10-2).

## 6. BUILD STEPS
1. **Data inventory + legal basis map**: dữ liệu cá nhân IVR chạm (order ref, contact ref/token, DTMF outcome, audit) → mục đích + cơ sở pháp lý.
2. **PIA** (Privacy Impact Assessment) `docs/compliance/pia.md`: rủi ro + biện pháp giảm thiểu; ký Legal.
3. **DSAR support**: quy trình + hỗ trợ kỹ thuật (tìm/xuất/xoá dữ liệu cá nhân trong phạm vi IVR theo contact/order ref, tôn trọng retention/audit bất biến).
4. **Consent/do-not-call verify**: khẳng định IVR luôn tôn trọng `PHONE_CALL` do-not-call (DC-01/02) — evidence.
5. **Retention policy** `docs/compliance/retention.md` (DF-07) ký Legal; map tới retention job.
6. **Compliance checklist** cho release gate (P9-1).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/compliance/data-inventory.md`, `pia.md`, `retention.md` | Inventory + PIA + retention (ký Legal) |
| `docs/compliance/dsar-runbook.md` | Quy trình DSAR |
| `src/Ivr.Api/Admin/DsarEndpoint.cs` (nếu cần) | Hỗ trợ tìm/xoá theo scope |
| `docs/compliance/release-compliance-checklist.md` | Gate P9-1 |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `COMP-PII-01` | verification | data inventory khớp thực tế; không dữ liệu ngoài danh mục (D-05). |
| `COMP-DSAR-02` | integration | DSAR tìm/xuất/xoá đúng scope; tôn trọng retention/audit bất biến. |
| `COMP-DNC-03` | integration | do-not-call PHONE_CALL luôn được tôn trọng (evidence). |
| `COMP-RETENTION-04` | integration | retention thực thi đúng policy đã ký (DF-07). |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] legal basis xác lập; [ ] PIA ký; [ ] DSAR chạy; [ ] do-not-call tôn trọng; [ ] retention ký+thực thi.
**Reviewer (Legal + Privacy):** đủ tuân thủ để go-live; recording OFF; residual risk chấp nhận được.

## 10. EVIDENCE EXPECTED
Data inventory, PIA ký, DSAR run, do-not-call evidence, retention policy ký + job run, compliance checklist.

## 11. FORBIDDEN
- ❌ Go-live không legal basis/PIA. ❌ Lưu dữ liệu ngoài inventory (D-05). ❌ Bỏ qua do-not-call. ❌ Recording không consent (DT-05).

## 12. DEFINITION OF DONE
- [ ] Legal basis + PIA + DSAR + retention (ký Legal) + checklist; 4 verification §8 pass; evidence §10 đủ.
