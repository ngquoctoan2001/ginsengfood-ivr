# TEST-01 — Strategy

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p11` · Nguồn: `MASTER-05`, `phase-8/09`,`/19`; `TECH-10`.

## 1. Tầng test (bám MASTER-05 5-layer smoke)
| Tầng | Phạm vi | Ví dụ |
| --- | --- | --- |
| Unit smoke | 1 resolver/guard/service block | intake validation, DT-02 mapping |
| Domain smoke | 1 domain đầy đủ | attempt policy D-10, callback state machine |
| Cross-pack smoke | luồng qua nhiều pack (mock) | task → calljob → callback → Core revalidate |
| E2E smoke | hành trình đầy đủ (dry-run) | 8 workflow |
| Release smoke | đủ điều kiện gate + owner sign-off | evidence packet + REAL_CALL gate |

## 2. Mock-first
- `EXECUTION_MODE=MOCK`, `SALES_PROVIDER=FAKE_TARGET_V1`, `SIM_PROVIDER=MOCK`; dùng `seed/sales-target-v1.sample.json` và deterministic fake scenarios. Legacy seed only for explicit compatibility tests.
- Bỏ mock dần theo `integration-requirements/*` khi có API/hạ tầng thật.
- **KHÔNG gọi khách thật** cho tới release gate (DF-03) + mua SIM (DT-01).

## 3. Evidence / smoke / completion-gate (MASTER-05)
- Mỗi smoke: `smoke_id`, PASS path **và** BLOCK/negative path, accepted evidence.
- Evidence status: DRAFT→SUBMITTED→UNDER_REVIEW→**ACCEPTED**→(REJECTED/VOID). Chỉ **ACCEPTED** mới dùng PASS.
- Completion/Release: không GATE_PASS khi còn điểm chặn (missing evidence/smoke/sign-off).

## 4. Loại test & công cụ (đề xuất — theo convention repo)
- Unit/integration: framework repo; mock adapter/port.
- Contract: validate OpenAPI 3.1 (DF-02) + consumer-driven contract cho task/callback.
- E2E: dry-run qua MOCK adapter + seed.
- Performance: mô phỏng SIM pool + rolling queue.
- Security/privacy: RBAC/allowlist/PII scan (no raw phone in log).

## 5. Traceability & exit
- Test map FR/P0 → seed SCN → smoke (09) → evidence.
- Exit tiêu chí: mọi P0 test có evidence ACCEPTED; không P0 nào thiếu negative case; release gate rõ.
- **Không** tự động chuyển production-ready từ test PASS (cần owner sign-off — DF-03).
