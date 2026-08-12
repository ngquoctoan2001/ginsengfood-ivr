# P07 — Generate Database Design

## Tên nhiệm vụ
Sinh ERD, table specs, indexes, enum/status, retention, migration plan cho IVR.

## Bối cảnh
Baseline `IVR-12` đã đề xuất 9 bảng chính: `ivr_confirmation_tasks`, `ivr_call_jobs`, `ivr_call_attempts`, `ivr_call_results`, `ivr_result_callbacks`, `ivr_sim_channels`, `ivr_capacity_incidents`, `ivr_technical_exceptions`, `ivr_admin_actions` (+ `ivr_evidence_links`). Prompt này chuẩn hóa thành DB design chính thức, không đổi semantic.

## Input cần đọc
- `specs/srs/data/*`, `specs/srs/functional/*`, `specs/srs/workflows/09-state-machines.md`
- `docs/documents/4. phase/phase-8/12-THIẾT KẾ CƠ SỞ DỮ LIỆU.md`
- `docs/documents/4. phase/phase-8/13-THIẾT KẾ HÀM VÀ DỊCH VỤ.md`
- `docs/documents/1. master/04-MASTER-03-TRACEABILITY-ID.md`
- `docs/documents/3. tech/02-TECH-01-...IDEMPOTENCY-EVIDENCE...md`

## Output cần tạo
- `specs/srs/database/`:
  - `00-index.md`
  - `01-erd.md` (Mermaid ERD)
  - `02-tables.md` (mọi bảng: cột, type semantic, required, index, constraint)
  - `03-enums-and-status.md` (program_type, call-job-status, attempt-status, result-status/type, callback state)
  - `04-indexes.md` (scheduler deadline query, idempotency unique, race guard)
  - `05-retention-and-privacy.md` (retention theo loại dữ liệu, TTL raw phone, recording OFF mặc định)
  - `06-migration-plan.md` (migration gates, rollback/forward-fix, seed non-prod SIM disabled)

## Quy tắc
- Constraint bắt buộc (**D-10**): Golden Hour `max_attempts=2, window=300` (spacing 150s); 24/7 `max_attempts=2, window=900` (spacing 450s); `T0`=lúc Core mở window; không tạo attempt vượt max.
- `is_counted_customer_attempt=false` khi có `technical_exception_type`.
- Không cột nào lưu order state như source-of-truth (chỉ snapshot/version).
- Không cột bắt buộc lưu full phone/recording nếu chưa có owner decision → đánh `Owner Decision Required`.
- Unique index cho task_id, callback_id, idempotency keys.

## Checklist hoàn thành
- [ ] ERD đủ 9–10 bảng + quan hệ.
- [ ] Constraint attempt/program có mặt.
- [ ] Index scheduler + idempotency có mặt.
- [ ] Retention/PII có bảng riêng.
- [ ] Migration gates rõ.

## Điều cấm
- KHÔNG tạo migration SQL production (chỉ design + plan).
- KHÔNG seed dữ liệu thật (để p10).

## Báo cáo cuối
1. Số bảng + enum.
2. Constraint P0 đã đưa vào.
3. Điểm retention/PII cần owner quyết.
