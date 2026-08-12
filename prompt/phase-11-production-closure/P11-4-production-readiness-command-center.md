# PROMPT P11-4 — Production Readiness Command Center

## 0. Meta

Work `W-0060` · continuous. The canonical tracker remains the work source; readiness views are derived artifacts, never a second backlog.

## 1. Outcome

Generate decision views for implementation/mock, one-SIM lab, real Sales integration and production. Preserve exact blocker/evidence truth and prevent “all prompts done” from being interpreted as go-live.

## 2. Build

1. Read tracker and validate every readiness item maps to a Work ID.
2. Produce dashboard/ledger grouped by four outcome levels and by owner/gate.
3. Verify code/test/evidence/acceptance links; flag stale baseline or mock-only proof.
4. Track Target vs current-compat provider/config and sunset; notification disabled.
5. Before go/no-go require two-program Sales flow, speech/token, callback/auth, attempt policy, 1-SIM lab, **32 eSIM production capacity**, legal/security/release evidence.
6. Produce concise blocker-first handoff and rollback/kill-switch readiness.

## 3. Forbidden

No independent task statuses, no percentage-based readiness, no global COD-only scope, no closing external item from a ticket/report without executable/accepted evidence, and no automatic production flag mutation.

## 5. Forbidden
- ❌ Tạo tracker/backlog thứ hai; file này **mirror** `_execution/prompt-execution-tracker.md`, không thay thế.
- ❌ Đóng gate bằng ticket/report thay vì artifact thật.
- ❌ Tự bật production flag.

## 6. Definition of Done
- [ ] Sinh `docs/release/gate-status.yaml` **machine-readable** (input cho guardrail `P0-4` và cho `P9-1`), phủ đủ `G-*` ở tracker §3 và `OD-V1-01..21`.
- [ ] Mỗi readiness item map tới một Work ID + evidence link thật.
- [ ] Board hiển thị đúng 4 nấc ladder (`IMPLEMENTATION_COMPLETE_BEHIND_MOCKS` → `LAB_REAL_SIM_VERIFIED` → `REAL_SALES_INTEGRATION_VERIFIED` → `PRODUCTION_REAL_ELIGIBLE`), không dùng phần trăm.
- [ ] Đạt tối đa `EVIDENCE_SUBMITTED`; chỉ Release owner chuyển `ACCEPTED`.
