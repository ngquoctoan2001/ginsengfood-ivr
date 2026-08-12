# PROMPT P2-3 — Scheduler & Attempt Policy

## 0. Meta
| | |
| --- | --- |
| **ID** | `P2-3` · **Phase** 2 — Core Runtime (mock SIM) |
| **Prereq** | `P2-2` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 Worker Service · PostgreSQL |

## 1. ROLE
Bạn là **Senior .NET Engineer (background processing)**. Bạn xây scheduler chạy trong `Ivr.Worker`: một **rolling queue** đẩy attempt theo đúng attempt policy D-10, không batch cuối phiên, idempotent, chịu tải, và tôn trọng cooldown/fail-count SIM.

## 2. CONTEXT
Task eligible (P2-2) cần được dispatch theo lịch: attempt 1 tại T0, attempt 2 sau spacing, expire sau window. Scheduler là trái tim thời gian của IVR — sai lịch = gọi sai giờ/quá số lần. Ở MODE=MOCK, "dispatch" gọi `ISimGateway` mock (P2-4).

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/03-scheduler-attempt-policy.md`, `specs/workflows/03-no-answer-attempts.md`
- `specs/architecture/05-resilience.md`
- `plan/ivr-orther/decisions-log.md` §D-10 · §DT-04 (cooldown 5s, fail-count≥3/10′ auto-disable) · §DS-02 (no-answer không transition order)

## 4. DECISIONS & CONSTRAINTS
- **D-10:** `max_attempts=2` cả hai; GH `[T0, T0+150]` window 300; 24-7 `[T0, T0+450]` window 900; `T0` = lúc mở window/tạo task. Dùng `AttemptPolicy` (P1-3) — 1 nguồn.
- **No-batch:** rolling/continuous, không gom cuối phiên; miss deadline = incident (log/alert).
- **DT-04:** `SIM_COOLDOWN_AFTER_CALL=5s`; SIM `fail_count≥3/10′` → auto-disable + alert.
- **DS-02:** no-answer final/technical **không** transition order — scheduler chỉ cập nhật `ivr_call_queue`/attempt; order chờ expire (Core).
- Idempotent: không tạo attempt vượt 2 (FR-IVR-SCH-005); không double-dispatch khi retry/restart.

## 5. INPUTS / DEPENDENCIES
- DB `ivr_call_jobs`/`ivr_call_attempts` + index scheduler-deadline (P1-2).
- `ISimGateway` (P2-4) — dispatch qua adapter (mock).
- Clock injectable (test thời gian).

## 6. BUILD STEPS
1. `SchedulerHostedService : BackgroundService` poll job đến hạn (dùng index deadline; SKIP LOCKED / advisory lock để tránh double-pick khi scale nhiều worker).
2. Với job eligible: tạo attempt theo `AttemptPolicy`; dispatch attempt 1 @T0; nếu no-answer → schedule attempt 2 @T0+spacing; nếu quá `expires_at` trước A2 → `IVR_CONFIRMATION_WINDOW_EXPIRED`.
3. **SIM pool管理**: chọn SIM rảnh (one-sim-one-active-call), áp cooldown 5s sau call; đếm fail-count, auto-disable SIM khi ≥3/10′ + alert.
4. Idempotency dispatch: mỗi attempt có key; restart worker không tạo trùng.
5. Ghi attempt/queue state; **không** transition order (DS-02). Miss-deadline → log incident + metric.
6. Concurrency & backpressure: giới hạn concurrent theo SIM pool size; hàng đợi ổn định.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Worker/Scheduling/SchedulerHostedService.cs` | Loop poll+dispatch |
| `src/Ivr.Worker/Scheduling/SimPoolManager.cs` | Pool, cooldown, fail-count, auto-disable |
| `src/Ivr.Domain/Policies/AttemptPolicy.cs` (dùng lại) | Lịch D-10 |
| `src/Ivr.Infrastructure/Repositories/CallJobRepository.cs` | Claim job (SKIP LOCKED) |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-SCH-A1A2-01` | integration | GH: A1@T0, A2@T0+150 khi A1 no-answer; expire T0+300. |
| `UT-SCH-MAX-02` | unit | không tạo attempt thứ 3 (D-10). |
| `IT-SCH-EXPIRE-03` | integration | quá window trước A2 → `WINDOW_EXPIRED`, order không bị IVR transition (DS-02). |
| `UT-SCH-COOLDOWN-04` | unit | cooldown 5s giữa call; fail-count≥3/10′ → SIM auto-disable+alert (DT-04). |
| `IT-SCH-IDEMP-05` | integration | restart worker giữa chừng → không double-dispatch. |
| `IT-SCH-LOCK-06` | integration | 2 worker → 1 job chỉ dispatch 1 lần (SKIP LOCKED). |

Trace: `specs/testing/02/03`, smoke `M8-P0-*` scheduler.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] lịch đúng D-10 (property test); [ ] no-batch; [ ] không double-dispatch; [ ] no order transition.
**Reviewer:** advisory lock chống double-pick; cooldown/fail-count đúng DT-04; clock injectable; miss-deadline có incident.

## 10. EVIDENCE EXPECTED
Timeline log A1/A2/expire, max-2 proof, cooldown/auto-disable log, double-dispatch prevention (restart + 2-worker).

## 11. FORBIDDEN
- ❌ Batch cuối phiên. ❌ Attempt > 2. ❌ Transition order (DS-02). ❌ Gọi thật (MODE=MOCK). ❌ Hardcode số attempt/window ngoài `AttemptPolicy`.

## 12. DEFINITION OF DONE
- [ ] Scheduler rolling + pool + idempotent; 6 test §8 xanh; evidence §10 đủ.
