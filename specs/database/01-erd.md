# DB-01 — ERD

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p07` · Nguồn: `phase-8/12` §3 + OD-DR-03 (`ivr_raw_call_event`).

```mermaid
erDiagram
  ivr_confirmation_tasks ||--o{ ivr_call_jobs : creates
  ivr_call_jobs ||--o{ ivr_call_attempts : schedules
  ivr_call_attempts ||--o| ivr_raw_call_event : captures
  ivr_call_jobs ||--o{ ivr_call_results : produces
  ivr_call_results ||--o{ ivr_result_callbacks : sends
  ivr_call_attempts ||--o{ ivr_technical_exceptions : may_open
  ivr_call_jobs ||--o{ ivr_capacity_incidents : may_reference
  ivr_sim_channels ||--o{ ivr_call_attempts : used_by
  ivr_admin_actions ||--o{ ivr_capacity_incidents : controls
  ivr_admin_actions ||--o{ ivr_sim_channels : controls
  ivr_evidence_links }o--|| ivr_confirmation_tasks : links
  ivr_console_accounts ||--o{ ivr_console_sessions : authenticates
```

## Quan hệ chính
- 1 task → n call jobs (thường 1); 1 job → n attempts (≤2, D-10); 1 attempt → 0..1 raw_call_event.
- 1 job → n results; 1 result → n callbacks (retry cùng idempotency).
- Technical exception & capacity incident tách riêng để **không** trộn vào customer attempt.
- `ivr_evidence_links` là link table tùy chọn cho evidence/audit refs khi cần query.
- Order state **không** có bảng riêng trong IVR (chỉ snapshot trong task/job/result).

## Console account/session (W-0105)

| Bảng | Field chính | Invariant |
| --- | --- | --- |
| `ivr_console_accounts` | `id`, unique immutable `username`, `display_name`, `role`, `status`, `is_builtin`, `password_hash`, lockout/login/password timestamps, optimistic `version`, retention/legal-hold columns | role chỉ `Admin/Operator`; status chỉ `ACTIVE/DISABLED/DELETED`; built-in `admin` luôn active Admin; plaintext password không lưu |
| `ivr_console_sessions` | `id`, `account_id`, unique SHA-256 `token_hash`, `created_at`, `expires_at`, revoke fields, retention/legal-hold columns | raw bearer token không lưu; FK `RESTRICT`; expiry sau create; revoke time/reason cùng null hoặc cùng có giá trị |

Username đã soft-delete vẫn giữ unique để không tái gán audit identity. Session bị
revoke khi account đổi role/status, reset password hoặc bị xoá.
