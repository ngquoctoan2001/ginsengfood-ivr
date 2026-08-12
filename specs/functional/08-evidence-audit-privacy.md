# FR — Evidence, Audit & Privacy

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p03`
Nguồn: `phase-8/08` (giám sát/audit/privacy), `phase-8/15` (security/privacy), `docx` §17, §21 (evidence plan); `MASTER-05` (evidence/smoke/gate), `TECH-01` (foundation audit/idempotency/evidence).

**Actor:** IVR services (writer) → Evidence Registry / Audit (owner: Foundation).
**Precondition:** Có action/state cần ghi.
**Trigger:** task intake, eligibility, attempt, SIM reserve/release, DTMF, result, callback, admin action, incident.
**Postcondition:** Evidence/audit append-only; privacy-safe; đủ cho release gate.

## Privacy (docx §17; phase-8/08 §6) — P0
- CONFIRMED: Chỉ dùng `phone_ref`/dial_token để gọi; UI/log mặc định `phone_masked`. Không lưu số đầy đủ ở màn không cần.
- CONFIRMED: Không đọc/log full address, member tier, Diamond, payment detail, order history, health note, CRM note.
- CONFIRMED: Recording OFF mặc định (AS-06); nếu bật cần `recording_ref` + retention policy + quyền nghe + audit truy cập.
- CONFIRMED: Opt-out / privacy block → chặn dispatch.

## FR
| ID | Yêu cầu | Nguồn | Acceptance hint |
| --- | --- | --- | --- |
| FR-IVR-EVID-001 | Ghi evidence/audit cho: task intake (+decision), eligibility (per gate), attempt schedule/execution, DTMF/call disposition, result normalization, callback (sent/ack/reject/block), admin action, technical/capacity incident | phase-8/04 §7, /08; docx §21 | Mỗi bước có evidence refs |
| FR-IVR-EVID-002 | Audit fields: `actor_type/id`, `permission`, `action`, `target_ref`, `reason`, `before/after`, `correlation_id`, `evidence_ref`, timestamp | phase-8/08 §6; TECH-01 | Đủ trường; append-only |
| FR-IVR-EVID-003 | Audit **append-only**; không sửa/xóa thủ công; soft delete không che audit | phase-8/12 §2; TECH-01 | Sửa audit → chặn |
| FR-IVR-EVID-004 | Evidence chỉ `ACCEPTED` mới dùng PASS; **không hardcode PASS/production-ready** | MASTER-05; phase-8/00 P0-005; docx P0-09 | PASS thiếu evidence → FAIL (P0-IVR-006) |
| FR-IVR-EVID-005 | Callback final yêu cầu evidence tồn tại; evidence write fail → technical exception/hold | phase-8/02 §10; /07 §11 | No evidence → hold/reject |
| FR-IVR-EVID-006 | PII redaction trong log/UI; không raw phone/full profile | phase-8/08,/15; docx §17 | Raw phone trong log → FAIL (P0-IVR-007) |
| FR-IVR-EVID-007 | Retention tách theo loại: task/job/result/callback metadata, call log, DTMF evidence, recording, admin audit, raw phone/token (TTL ngắn nhất) | phase-8/12 §11; docx §17 | Có bảng retention (`Owner Decision Required` OD-13) |
| FR-IVR-EVID-008 | Evidence packet cho release gate: architecture, task intake, eligibility, scheduler, SIM, DTMF, callback, privacy/security, capacity, release | docx §21; phase-8/09 | Packet đủ mục trước sign-off |

## Owner Decision
- OD-12 (recording enabled), OD-13 (retention duration), Q-A2 (evidence registry integration/allowlist).
Chi tiết test/evidence mapping: p11 (`specs/srs/testing/*`).
