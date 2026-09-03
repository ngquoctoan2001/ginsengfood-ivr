# FR — Scheduler and Attempt Policy

Trạng thái: `TARGET_V1_DRAFT`; W-0151 xác nhận D-10 chỉ là candidate/history và production
vẫn `OWNER_DECISION_REQUIRED` từ Product + Order Core + Module 3.

## Policy model

- Task mang `attempt_policy_version`, `max_customer_attempts`, `attempt_offsets_seconds`, window start/expiry.
- Intake resolve policy registry theo exact version/program/execution mode, rồi so exact max,
  ordered offsets và window duration với snapshot trên wire; mismatch trả
  `409 IVR_POLICY_MISMATCH` và không tạo job.
- Database không CHECK exact `2/300/150/900/450`; chỉ enforce bounds/invariants.
- Candidate code `mock-lab-v1`: Golden Hour `[0,150]` trong 300s; 24/7 `[0,450]` trong
  900s; max 2. Dev seed chỉ cho `MOCK`; dev loader có thể thêm `LAB_REAL_SIM`; lab seed mặc định
  dùng version riêng `lab-softphone-v1`.
- `PRODUCTION_REAL` hiện fail-closed ở hai điểm tách rời: intake từ chối policy không được registry
  cho production; pre-dial feature guard từ chối literal candidate/`UNAPPROVED`. Chưa có
  registry-wide startup activation gate, và pre-dial flag chưa được so với policy snapshot của job.
- Accepted task/job giữ immutable policy snapshot; registry change không rewrite in-flight work.
- `TechnicalRetryLimit` là scheduler config riêng, không thuộc versioned attempt policy; production
  retry/backoff/counting cần owner ký theo [M8-11](../../plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md).

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

## Production decision gate

Không promote/rename `mock-lab-v1` hoặc sửa scheduler/registry trước khi `ATP-01..ATP-15` có chữ ký
và M3 giao producer/CDC. Quiet hours/timezone/holiday, technical retry/backoff, atomic bundle,
cutover/rollback và active-policy coherence vẫn chưa được production owner quyết định.

Capacity model phải mô phỏng 1 và 32 channels, nhưng production sizing chỉ được chốt từ throughput thực đo.
