# DB-06 — Migration Plan

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p07` · Nguồn: `phase-8/12` §12; D-10, DT-01/05, DF-07.

## 1. Migration gates (trước khi merge)
- [ ] Unique index cho `task_id`, `callback_id`, các `idempotency_key` (DF-04).
- [ ] Index scheduler-deadline (`ivr_call_jobs(status, expires_at)`, `t0_at`).
- [ ] Constraint/app-guard: `max_attempts BETWEEN 1 AND 10`; offsets ordered/nonnegative/before expiry; `attempt_number ≤ max_attempts`; technical ≠ counted attempt. Candidate timings live in versioned config, not DB CHECK.
- [ ] **KHÔNG** cột bắt buộc full phone / raw recording (D-05/DT-05/DF-07).
- [ ] Có migration rollback hoặc forward-fix plan.
- [ ] Seed tối thiểu: chỉ `ivr_sim_channels` ở **non-prod**, `enabled=false`, `adapter_mode=MOCK` (DT-01).

## 2. Thứ tự migration đề xuất
1. `ivr_confirmation_tasks` (+ CHECK D-10) → 2. `ivr_call_jobs` → 3. `ivr_call_attempts` → 4. `ivr_raw_call_event` → 5. `ivr_call_results` → 6. `ivr_result_callbacks` → 7. `ivr_sim_channels` → 8. `ivr_capacity_incidents` → 9. `ivr_technical_exceptions` → 10. `ivr_admin_actions` → 11. `ivr_evidence_links`.

## 3. Điểm khác biệt cần chú ý khi hiện thực (so với phase-8/12 gốc)
- ⚠️ **CHECK 24/7 `max_attempts`**: gốc phase-8/12 = 3 → **đổi thành 2** (D-10). Golden Hour window gốc 600 → **300** (D-10). Nếu tái dùng migration cũ, **phải sửa constraint**.
- ➕ Thêm bảng `ivr_raw_call_event` (OD-DR-03).
- ➕ Thêm cột `sellable_status_json` + `sellable_captured_at`, `t0_at`, `attempt_spacing_seconds`, `order_state`, `payment_method_snapshot`, `is_ivr_callable`(nullable/derived), `call_restriction`(nullable), `adapter_mode`.
- Target V1 requires `order_version_seen_by_ivr`; current-compat data, if retained, stays in explicit compatibility columns/table and never weakens target validation. Store HTTP + semantic ACK separately.

## 4. Sau khi có nguồn thật (bỏ mock)
- `call_restriction`: bật NOT NULL/logic sau khi IR-CRM-01 build rich response/Core wiring từ nguồn DC-01.
- `sellable_captured_at`/policy_version: sau khi ops bổ sung (DO-02).
- `ivr_sim_channels.adapter_mode=REAL` + số SIM thật: sau khi mua SIM (DT-01/DT-04) và release gate pass (DF-03).
- Retention: đặt TTL/purge job sau DF-07.

## 5. Outbox/event
- AsyncAPI/outbox: **không bắt buộc tạo** production trong baseline (phase-8/12 §10). Nếu dùng, tái dùng pattern ops-core (`HttpWebhookOutboxEventDispatcher` — DF-05), event khớp `events/business-platform/ivr/*`, không thay callback Order Core.

## Báo cáo (p07)
- **11 bảng** (10 gốc + `ivr_raw_call_event`); enums/status đủ; index scheduler+idempotency+current callback status; race-guard fields target/nullable; retention **PENDING DF-07**; migration gate rõ. Constraint P0 đã đưa vào (đặc biệt sửa 24/7 max=2 theo D-10).
