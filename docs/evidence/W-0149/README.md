# W-0149 — M8-09 revoke/recall/freshness reconciliation

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

Trạng thái: **`EVIDENCE_SUBMITTED / CURRENT_OPTION_A_BEHAVIOR_PRESENT /
OWNER_PROVENANCE_REQUIRED / M3_D06_RUNTIME_NOT_FOUND / OPTION_B_NOT_IMPLEMENTED /
CODE_NOT_AUTHORIZED`**

## 1. Phạm vi

- Gộp `C10 + C11 + C13` thành lifecycle revoke/recall/freshness.
- Audit intake/idempotency, evidence freshness, scheduler claim, claim→dial, admin controls, DB,
  result/callback taxonomy, tests và M3 current snapshot.
- Lập decision/race matrix; không sửa source/runtime/OpenAPI/migration.

## 2. Kết quả chính

- IVR hiện validate snapshot ở intake, không recheck current business state trước mỗi attempt.
- `POST /tasks` không phải update: same payload replay, changed payload conflict.
- Không có revoke route, persisted revoke state/generation, scheduler filter hay pre-dial business fence.
- Admin queue pause và live-call terminate là operational controls, không phải M3 business revoke.
- Callback ACK đã có blocked/stale semantics, nhưng `IT-ELIG-RACE-12` chỉ mô phỏng IVR bookkeeping.
- Snapshot M3 `PhucApu@a3aad246d986` không có exact hit cho generic Target V1 callback consumer,
  target ACK hoặc IVR revoke path; D-06 runtime chưa được chứng minh.
- `OD-17` ghi nhận stale-call trade-off, nhưng provenance chữ ký và M3 acceptance chưa đủ để nâng
  `ACCEPTED` hoặc cho phép production.

## 3. Artifact

- [M8-09 decision pack](../../../plan/ivr-orther/m8-09-revoke-freshness-decision-pack-2026-09-03.md)
- S-07/TODAY-01, M8 worklist và target plan status
- IR-02/IR-06 và functional eligibility clarification
- Tracker/readiness/gate mirror và official Markdown map

## 4. Verification

| Gate | Kết quả 03/09/2026 |
| --- | --- |
| Focused unit — eligibility freshness/evidence, callback ACK mapping, no-Ops-egress architecture | **PASS `49/49`** |
| Focused contract — Target V1 callback/ACK vocabulary | **PASS `20/20`** |
| Focused PostgreSQL integration — blocked callback, final-attempt stop, queue pause | **PASS_LOCAL_POSTGRES `3/3`** — W-0161 full integration `236/236`, 0 fail/skip; gồm `IT-ELIG-RACE-12`, `IT-SCH-FINAL-04`, `IT-API-QUEUE-08` |
| API docs/OpenAPI | **PASS** — docs `14` artifact; lint `2` spec; validate `2` file/`9` fixture/`12` schema-negative/`13` domain-negative/`1` compatibility; negative OpenAPI + `3` pinned hash PASS |
| Tracker/readiness mirror | **PASS** — `11` gate, `147` work item, `23` open decision; no rung claimed; production flag `false` |
| Official Markdown map | **PASS** — `629` Markdown file; decision pack và W-0149 evidence đều `0` unresolved link |
| `git diff --check` | **PASS** — chỉ có line-ending conversion warnings của shared worktree |
| GitNexus advisory | Stale index; direct current source/tests là authority. Concrete scheduler method có runtime caller `SchedulerRuntime.RunOnceAsync`; caller set chỉ là lower bound vì interface dispatch |
| Source/OpenAPI/DB change thuộc W-0149 | **Không có** — decision/evidence only |

Ghi chú lịch sử: lần chạy W-0149 ban đầu dừng ở fixture. W-0161 đã chạy assertion thật qua local
Docker/Testcontainers; xem [evidence W-0161](../W-0161/README.md). Approval provenance, M3 D-06 và
shared E2E vẫn không được suy từ kết quả này.

## 5. Residual gates

- Strategy A/B/hybrid và approval provenance: `NOT_RECEIVED`.
- M3 D-06 code/CDC/OAS/auth/sandbox: `NOT_FOUND / NOT_RECEIVED`.
- Nếu chọn B: `RVK-01..RVK-12`, authoritative command schema và race/fencing design: `NOT_SIGNED`.
- Shared E2E và rollback/failure drill: `NOT_RUN`.

Không nâng `ACCEPTED`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.
