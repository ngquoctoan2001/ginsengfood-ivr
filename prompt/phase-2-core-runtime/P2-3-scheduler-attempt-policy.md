# PROMPT P2-3 — Versioned Scheduler, Attempts and Channel Leases

## 0. Meta

Work `W-0020` · prereq P2-2 · default mode `MOCK`.

## 1. Outcome

Implement deadline-aware scheduler using task policy snapshots and dynamic channel leases. Candidate `mock-lab-v1` (2 attempts; GH 300/[0,150], 24/7 900/[0,450]) is fixture/config only and must fail in production mode until owner-approved.

## 2. Build

1. Implement policy registry, startup/config validation, immutable per-job snapshot and audited policy changes.
2. Queue ordering: deadline, program priority, due offset, risk, creation; deterministic tie-breaker.
3. Atomic claim/lease/fencing across multiple workers; one active call/channel; lease recovery after crash.
4. Dispatch only at configured offsets and before expiry; final result cancels future work; technical retry uses a separate bounded counter.
5. Dynamic channel pool supports mock counts, 1 lab SIM and 32 target eSIM without code change.
6. Capacity miss creates incident/result; never silently expires. MOCK cannot call real adapter; LAB requires allowlist; PROD requires approved policy/release.

## 3. Tests/evidence

Unit/property/integration tests: candidate schedules; alternate approved policy proves no hard-code; unknown/unapproved production policy; clock boundary; duplicate workers/fencing; crash recovery; final result stops next attempt; technical not counted; 1/32-channel simulations; capacity incident. Update W-0020 with exact timing assertions and commands.

## 4. Forbidden/DoD

No hard-coded D-10 in DB/domain, no batch-end scheduling, no over-attempt, no adapter-driven permission. `TESTS_PASS` remains mock/lab-policy evidence only.
