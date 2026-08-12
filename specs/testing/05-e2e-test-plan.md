# TEST-05 — E2E Test Plan (dry-run)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p11` · Nguồn: `workflows/*`; seed `SCN-*`. Chạy MOCK adapter, `REAL_CUSTOMER_CALL_ALLOWED=NO`.

| ID | Workflow | Seed | PASS path | BLOCK/negative |
| --- | --- | --- | --- | --- |
| E2E-01 | Confirm (phím 1) | SCN-001 | `IVR_CONFIRMED` → Core ACCEPTED | race recall → E2E-06 |
| E2E-02 | Cancel (phím 0) | SCN-002 | `IVR_CUSTOMER_CANCELLED` → Core cancel | — |
| E2E-03 | No-answer 2 attempt | SCN-003 | A1 no-answer → A2 → `NO_ANSWER_FINAL` | không tạo attempt 3 (D-10) |
| E2E-04 | Invalid phone | SCN-005 | `INVALID_PHONE_FINAL` (không dispatch) | không tính no-answer |
| E2E-05 | Technical exception | SCN-006 | `IVR_TECHNICAL_EXCEPTION` → retry/review | không cancel như no-answer |
| E2E-06 | Race (phím 1 + recall) | SCN-009 | result `CONFIRMED` nhưng Core `BLOCKED_BY_CORE` | order **không** confirm (D-06) |
| E2E-07 | Trusted skip | SCN-010 | `TASK_SKIPPED_TRUSTED_CUSTOMER`, không gọi | trusted+risk → vẫn gọi |
| E2E-08 | Capacity hold | SCN-015 | mở `capacity_incident`, không batch | miss deadline không log → FAIL |
| E2E-09 | Window expired | SCN-007 | `IVR_CONFIRMATION_WINDOW_EXPIRED` | — |
| E2E-10 | Opt-out block | SCN-012 | `TASK_BLOCKED_OPERATIONAL` (do-not-call) | — |
| E2E-11 | Duplicate callback | SCN-011 | idempotent ack cũ | không double transition |
| E2E-12 | Not official order | SCN-013 | reject NOT_OFFICIAL_ORDER | — |
| E2E-13 | Busy → confirm | SCN-004 | A1 busy=no-answer → A2 confirm | busy không technical |
| E2E-14 | Operational block (recall pre-dispatch) | SCN-008 | `TASK_BLOCKED_OPERATIONAL` | — |
| E2E-15 | KEY_9 not enabled | SCN-014 | `IVR_WRONG_INPUT` | không mở support handoff |

## Báo cáo
15 E2E case phủ 8 workflow chính + biến thể; mỗi ca dry-run; race/stale/block đều có. Không ca nào gọi khách thật.
