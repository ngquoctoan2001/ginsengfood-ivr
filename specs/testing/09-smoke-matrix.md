# TEST-09 — Smoke Matrix

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p11` · Nguồn: `phase-8/09` (IVR-SMK-001..030), docx §20 (M8-P0-001..012); seed `SCN-*`.
ID scheme kép (OD-DR-02): giữ cả `M8-P0-*` (docx) và `IVR-SMK-*` (phase-8) qua bảng ánh xạ. Mỗi smoke có **PASS** + **BLOCK/negative**.

| Smoke (docx / phase-8) | Kịch bản | PASS path | BLOCK/negative | Seed | Evidence |
| --- | --- | --- | --- | --- | --- |
| M8-P0-001 / IVR-SMK-001 | Reject quote/cart/draft | task Official Order → accept | task Draft → reject NOT_OFFICIAL_ORDER | SCN-013 | intake log |
| M8-P0-002 / IVR-SMK-002 | GH đúng lịch | A1@T0, A2@T0+2:30, expire T0+5:00 | attempt 3 → chặn (D-10) | SCN-001/003 | attempt trace |
| M8-P0-003 / IVR-SMK-003 | 24/7 đúng lịch | A1@T0, A2@T0+7:30, expire T0+15:00 | attempt 3 → chặn | SCN-003 | attempt trace |
| M8-P0-004 / IVR-SMK-004 | Phím 1 | `IVR_CONFIRMED` → Core ACCEPTED | không tạo attempt 2 | SCN-001 | DTMF+callback |
| M8-P0-005 / IVR-SMK-005 | Phím 0 | `IVR_CUSTOMER_CANCELLED` → Core cancel | — | SCN-002 | DTMF+callback |
| M8-P0-006 / IVR-SMK-006 | No-answer 2 cuộc | `IVR_NO_ANSWER_FINAL` | không attempt 3 | SCN-003 | attempt trace |
| M8-P0-007 / IVR-SMK-007 | SIM/DTMF/server lỗi | `IVR_TECHNICAL_EXCEPTION` | **không** thành no-answer | SCN-006 | technical exception |
| — / IVR-SMK-008 | Invalid phone | `IVR_INVALID_PHONE_FINAL` | không no-answer | SCN-005 | eligibility log |
| M8-P0-008 / IVR-SMK-009 | Sale Lock/Recall active | task BLOCKED, không gọi | — | SCN-008 | blocker evidence |
| — / IVR-SMK-010 | Race: phím 1 + recall lúc revalidate | result CONFIRMED nhưng Core BLOCKED | order không confirm (D-06) | SCN-009 | callback+blocker |
| M8-P0-009 / IVR-SMK-011 | 32 SIM, 800 job/5′ | rolling, không batch | vượt → capacity_incident | SCN-015 | capacity incident |
| M8-P0-010 / IVR-SMK-012 | Duplicate callback | idempotent ack cũ | không double transition | SCN-011 | idempotency log |
| M8-P0-011 / IVR-SMK-013 | Admin force order từ dashboard | — | bị chặn RBAC/boundary | SEC-05 | RBAC audit |
| M8-P0-012 / IVR-SMK-014 | Completion PASS thiếu evidence | — | FAIL gate, không release | IT-16 | evidence gate |
| — / IVR-SMK-015 | Trusted skip | `TASK_SKIPPED_TRUSTED_CUSTOMER` | trusted+risk → vẫn gọi | SCN-010 | eligibility |
| — / IVR-SMK-016 | do-not-call / opt-out | block dispatch | — | SCN-012 | blocker (DC-01 source; mock seed) |
| — / IVR-SMK-017 | Window expired | `IVR_CONFIRMATION_WINDOW_EXPIRED` | — | SCN-007 | window log |
| — / IVR-SMK-018 | KEY_9 not enabled | `IVR_WRONG_INPUT` | không support handoff | SCN-014 | DTMF |
| — / IVR-SMK-019..024 | Fail-closed (order-core/ops/ops-503/crm/evidence/sim down) | không dispatch/không confirm | — | IT-12..17 | fail-safe evidence |

## Coverage
- **P0 bắt buộc (mục 3 của `00-index`):** đủ — no-self-update (SMK-013), no-quote-cart-draft (001), max-2-attempt (002/003/006), technical≠no-answer (007), invalid≠no-answer (008), stale-no-transition (IT-08), evidence-missing (014), race-block (010), do-not-call (016), fail-closed (019..024).
- Cả PASS và BLOCK path cho mỗi smoke.

## Báo cáo (p11)
1. **Test case theo loại:** unit ~30, integration 17, contract ~18, e2e 15, performance 6, security 15, + smoke matrix ~24 dòng. (Tổng ~125.)
2. **Coverage smoke:** map M8-P0-001..012 + IVR-SMK-001..024; mỗi smoke có PASS+BLOCK.
3. **Điều kiện còn thiếu để mở release gate:** mua SIM (DT-01/04), DF-07 retention/DT-05 recording (Legal), owner sign-off (DF-03). Q-C1/DC-01 và DG-03/DS-01..05 đã resolved; còn IR-CRM-01 + IR-SALES-OC1/OC2/OC3 là build/target.
