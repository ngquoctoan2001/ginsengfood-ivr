# FR — Scheduler and Attempt Policy

Trạng thái: `TARGET_V1_DRAFT`; D-10 là `OWNER_DECISION_REQUIRED` cho production.

## Policy model

- Task mang `attempt_policy_version`, `max_customer_attempts`, `attempt_offsets_seconds`, window start/expiry.
- Policy registry/config kiểm bounds, program và environment approval.
- Database không CHECK exact `2/300/150/900/450`; chỉ enforce bounds/invariants.
- Candidate `mock-lab-v1`: Golden Hour `[0,150]` trong 300s; 24/7 `[0,450]` trong 900s; max 2.
- `PRODUCTION_REAL` fail startup/dispatch nếu policy version chưa được owner approve.

## Requirements

| ID | Yêu cầu |
| --- | --- |
| `FR-IVR-SCH-001` | deadline-aware rolling queue; không batch cuối window |
| `FR-IVR-SCH-002` | ưu tiên deadline/program/attempt/risk bằng deterministic ordering |
| `FR-IVR-SCH-003` | lease/fencing đảm bảo one active call per channel |
| `FR-IVR-SCH-004` | channel count/cooldown/health/quarantine là config, không hard-code |
| `FR-IVR-SCH-005` | không vượt policy max; result final ngăn attempt sau |
| `FR-IVR-SCH-006` | technical retry tách customer attempt |
| `FR-IVR-SCH-007` | không thể dispatch trước expiry → capacity incident + normalized result |
| `FR-IVR-SCH-008` | MOCK không chạm real adapter; LAB chỉ 1 SIM/allowlist; PROD cần gates |
| `FR-IVR-SCH-009` | policy config change versioned/audited; task đang chạy giữ snapshot cũ |

Capacity model phải mô phỏng 1 và 32 channels, nhưng production sizing chỉ được chốt từ throughput thực đo.
