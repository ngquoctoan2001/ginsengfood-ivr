# PROMPT P11-3 — Legal Retention & DF-03 Sign-Off Pack

## 0. Meta
| | |
| --- | --- |
| **ID** | `P11-3` · **Phase** 11 — External Production Closure |
| **Work ID** | `W-0059` (canonical tracker §5) |
| **Prereq** | `P10-1`, `P10-2` inputs preferred; must finish before `P9-1` |
| **Governance** | No legal/sign-off auto-approval; prepare package for owner/legal/security |
| **Stack** | Compliance artifacts + decision records + release evidence |

## 1. ROLE
Bạn là **Privacy/Compliance Release Coordinator**. Bạn chuẩn bị hồ sơ retention, PDPA/legal basis, recording OFF stance, evidence acceptance, và DF-03 sign-off package để owner/legal/security có thể ký. Bạn không đưa tư vấn pháp lý giả; mọi chỗ cần legal sign phải ghi rõ `LEGAL_SIGNOFF_REQUIRED`.

## 2. CONTEXT
DF-07 retention và DF-03 sign-off là hard production blockers. P10-1/P10-2 xây privacy/data-governance implementation, nhưng production cần decision records được owner/legal/security chấp nhận. Prompt này gom policy, risk, evidence và sign-off checklist thành hồ sơ release.

## 3. SOURCE SPECS (đọc trước)
- `plan/toan-viec-can-lam-m8-2026-09-03.md` — các dòng privacy, suppression, freshness và telephony hiện hành
- `specs/data/05-pii-policy.md`
- `specs/testing/08-acceptance-criteria.md`
- `specs/_review/open-decisions-register.md`
- `integration-requirements/04-shared-auth-audit-requirements.md`
- `prompt/phase-10-compliance-maturity/P10-1-pdpa-privacy-compliance.md`
- `prompt/phase-10-compliance-maturity/P10-2-data-governance-backup-dr.md`
- `plan/ivr-orther/decisions-log.md` §DF-03/DF-07/DT-05/D-05

## 4. DECISIONS & CONSTRAINTS
- **DF-07:** retention duration per data class must be signed by owner/legal.
- **DT-05:** recording OFF by default; if recording ever turns ON, consent + retention + legal basis must be re-opened.
- **D-05:** raw phone/token restricted; token→number only at SIM boundary.
- **DF-03:** production sign-off requires Module 8 Owner + security/privacy review + evidence ACCEPTED.
- **MASTER-05:** submitted evidence is not accepted evidence; report is not gate pass.

## 5. INPUTS / DEPENDENCIES
- Evidence packet from P5/P6/P7/P8/P10.
- Draft retention values from owner/legal; if absent, produce options and mark not approved.
- Security/privacy review participants and approval channel.

## 6. BUILD STEPS
1. Build data inventory: call logs, attempts, DTMF, callback, raw SIM event ref, audit, evidence links, dial token, recording (OFF), admin annotations.
2. Propose retention table with options, risk, purge method, restore/backup interaction; mark every unapproved value `LEGAL_SIGNOFF_REQUIRED`.
3. Build PDPA/legal basis memo for transactional COD confirmation call, do-not-call respect, opt-out handling, DSAR/export/delete constraints.
4. Build recording OFF decision record; include re-open criteria if recording ON later.
5. Build privacy/security review checklist: PII masking, logs, UI, exports, vendor logs, token vault, access control, audit.
6. Assemble DF-03 sign-off input: scope, pilot/prod blast radius, kill-switch, rollback, evidence accepted list, residual limitations (OC1/OC2/DC-05 if deferred).
7. Route package for owner/legal/security signatures; only after signatures create final decision records for P9-1.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/compliance/ivr-data-inventory.md` | Data classes + owner + sensitivity + storage |
| `docs/compliance/ivr-retention-options.md` | Retention options before legal sign |
| `specs/decisions/DF-07-retention-policy.md` | Signed retention policy; do not create as final without approval |
| `specs/decisions/DT-05-recording-off-policy.md` | Recording OFF + re-open conditions |
| `docs/compliance/ivr-pdpa-legal-basis-pack.md` | Legal basis/DSAR/do-not-call evidence pack |
| `docs/release/df03-signoff-input.md` | Sign-off input for P9-1 |
| `specs/decisions/DF-03-signoff.md` | Final sign-off record, only after approval |

## 8. TESTS / VERIFICATION TO RUN
| Test ID | Loại | Assert |
| --- | --- | --- |
| `LEGAL-RET-01` | review | Every data class has retention owner, value or `LEGAL_SIGNOFF_REQUIRED`, purge mechanism. |
| `LEGAL-PII-02` | review | Raw phone/recording/token constraints match D-05/DT-05. |
| `LEGAL-DSAR-03` | review | DSAR/export/delete process excludes immutable audit incorrectly but documents lawful handling. |
| `SIGNOFF-DF03-04` | gate | DF-03 final record cannot exist without owner+security/privacy approval fields. |
| `GATE-EVID-05` | gate | Evidence accepted list traces to P5/P6/P7/P8/P10 artifacts. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] no legal conclusion without signer; [ ] all data classes covered; [ ] recording OFF locked; [ ] DF-03 package contains residual limitations; [ ] P9-1 can consume artifacts.

**Reviewer:** Legal signs retention/legal basis; security/privacy signs PII/access/logging; owner signs release scope.

## 10. EVIDENCE EXPECTED
Signed retention policy, privacy review notes, access-control proof, purge/backup proof, DF-03 sign-off record, list of residual deferred items with flags.

## 11. FORBIDDEN
- ❌ Mark retention approved without legal/owner sign. ❌ Turn recording ON. ❌ Delete/alter audit to satisfy DSAR without policy. ❌ Open production if DF-03 is unsigned. ❌ Hide deferred target gaps.

## 12. DEFINITION OF DONE
- [ ] Compliance pack + signed DF-07/DT-05/DF-03 records exist or are explicitly blocked; P9-1 has an unambiguous go/no-go input.
