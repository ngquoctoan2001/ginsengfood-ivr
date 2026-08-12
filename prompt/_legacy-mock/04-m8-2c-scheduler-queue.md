# DEV PROMPT 04 — M8.2C Scheduler & Queue

## Mục tiêu
Deadline-aware rolling queue; attempt policy D-10; không batch cuối phiên.

## Requirement / Decision
`FR-IVR-SCH-001..008` · `D-10`, `DT-04`.

## Source spec
- `specs/srs/functional/03-scheduler-attempt-policy.md`
- `specs/srs/architecture/04-deployment-architecture.md`, `database/04-indexes.md`

## Build scope
1. `SCHEDULER_MODEL=DEADLINE_AWARE_ROLLING_QUEUE`; ưu tiên near-expiry → GH → attempt2-due → risk → còn-thời-gian.
2. Attempt: A1@T0, A2@T0+spacing (GH 150 / 24-7 450), expire theo window (300/900); **max 2** (D-10); A1 có kết quả cuối → không A2.
3. SIM Channel Manager: `ONE_SIM_ONE_ACTIVE_CALL`, cooldown 5s, `fail_count≥3/10′`→disable+alert.
4. Capacity: vượt năng lực → `capacity_incident` (không im lặng).
5. Dispatch qua adapter **MOCK** (dry-run).

## Done gate (docx M8.2C)
- [ ] Attempt đúng thời gian; **không tạo attempt 3** (D-10).
- [ ] Không batch cuối phiên (FAIL nếu có).
- [ ] Không giao trùng SIM; health monitor hoạt động.
- [ ] Vượt capacity → incident.

## Evidence expected
Attempt timing proof; no-batch proof; one-sim-one-call; capacity incident sample.

## Forbidden
KHÔNG gọi khách thật (MOCK); KHÔNG kéo dài window thương mại; KHÔNG vượt max.

## Test
`testing/02` (UT-SCH), `testing/06` (PT-01..06), smoke `M8-P0-002/003/009`.
