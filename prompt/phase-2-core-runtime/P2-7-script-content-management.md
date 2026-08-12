# PROMPT P2-7 — Script, Content Approval and Speech Safety

## 0. Meta

Work `W-0024` · prereq P2-1 · mode independent.

## 1. Outcome

Build versioned Vietnamese script lifecycle so calls can accurately read order items, total and short delivery area without exposing forbidden data. No script is active until content/privacy approval state permits its execution mode.

## 2. Build

1. Model draft/review/approved/retired versions, immutable after approval; record actor/reason/audit.
2. Whitelist only Target V1 speech fields. Template supports plural/items/collapse, currency and 1/0 instructions.
3. Validate missing/oversized/unknown/HTML-control/PII fields; forbid full address/raw phone/payment/history/free CRM text.
4. Preview renders exact sanitized text and estimated duration; preserve input snapshot/hash.
5. Separate approval by mode: MOCK may use test-approved; LAB needs lab-approved; PROD needs Content+Privacy/Legal approved version.
6. Do not implement notification templates or A/B behavior that changes order meaning.

## 3. Tests/evidence

Snapshot/golden tests Vietnamese diacritics, one/many items, collapse remainder, large amounts, short area and malicious/PII inputs; RBAC/audit/immutability; mode approval gates. Update W-0024 with approved test fixture and privacy test report.

## 4. Forbidden/DoD

No full address, raw phone, recording or customer notification. Fake speech data does not close upstream W-0003.
