# PROMPT P2-7 — Script, Content Approval and Speech Safety

## 0. Meta

Work `W-0024` · prereq `P1-3` (`W-0016` — domain/ports) · mode independent.

> **Gỡ vòng phụ thuộc (2026-08-12, W-0069).** Trước đây `P2-7` khai prereq `P2-1`, trong khi `P2-1` lại phải fail-closed trên script approval state do chính `P2-7` tạo — vòng tròn khiến executor `W-0018` không có gì để validate. **Giải pháp cuối:** toàn bộ `P2-7` chuyển lên **trước** `P2-1`; prereq của `P2-7` là `P1-3`, và `P2-1` nhận `P2-7` làm prereq. Không tồn tại prompt `P2-7a`/`P2-7b` — đó chỉ là cách diễn đạt trung gian đã bị bỏ.

## 1. Outcome

Build versioned Vietnamese script lifecycle so calls can accurately read order items, total and short delivery area without exposing forbidden data. No script is active until content/privacy approval state permits its execution mode.

## 2. Build

1. Model draft/review/approved/retired versions, immutable after approval; record actor/reason/audit. Expose `IScriptRegistry.TryGetApproved(templateId, version, mode)` để `P2-1` fail-closed khi script chưa duyệt (`IVR_SCRIPT_NOT_APPROVED`). Seed sẵn một template test-approved cho `MOCK`.
2. Whitelist only Target V1 speech fields. Template supports plural/items/collapse, currency and 1/0 instructions.
3. Validate missing/oversized/unknown/HTML-control/PII fields; forbid full address/raw phone/payment/history/free CRM text.
4. Preview renders exact sanitized text and estimated duration; preserve input snapshot/hash.
5. Separate approval by mode: MOCK may use test-approved; LAB needs lab-approved; PROD needs Content+Privacy/Legal approved version.
6. Do not implement notification templates or A/B behavior that changes order meaning.

## 3. Tests/evidence

Snapshot/golden tests Vietnamese diacritics, one/many items, collapse remainder, large amounts, short area and malicious/PII inputs; RBAC/audit/immutability; mode approval gates. Update W-0024 with approved test fixture and privacy test report.

## 4. Forbidden/DoD

No full address, raw phone, recording or customer notification. Fake speech data does not close upstream W-0003.
