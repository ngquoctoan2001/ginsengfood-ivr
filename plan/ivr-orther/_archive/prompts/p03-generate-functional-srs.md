# P03 — Generate Functional SRS

## Tên nhiệm vụ
Sinh functional requirements chi tiết cho IVR Order Confirmation.

## Bối cảnh
Baseline phase-8 đã có FR rải rác theo từng doc (mã `IVRxx-FR-xxx`, `IVRxx-P0-xxx`). Prompt này gom, chuẩn hóa, khử trùng lặp và bổ sung acceptance-friendly wording thành bộ functional SRS mạch lạc, giữ nguyên mã traceable.

## Input cần đọc
- `specs/srs/01-context-and-scope.md`, `03-stakeholders-and-actors.md`, `04-glossary.md`
- `docs/documents/4. phase/phase-8/` files 00, 03, 04, 05, 06, 07, 13, 14, 22, 23
- `docs/documents/3. tech/10-TECH-09-...md` (lifecycle & classification)
- `plan/ivr-orther/03-ivr-related-findings.md`

## Output cần tạo
- `specs/srs/functional/` gồm tối thiểu:
  - `00-index.md`
  - `01-task-intake.md` (nhận task từ Order Core, validation, idempotency)
  - `02-eligibility-and-blockers.md` (trusted skip, phone validation, Sale Lock/Recall/Suppression)
  - `03-scheduler-attempt-policy.md` (**D-10**: GH 5′/2 cuộc/A2@T0+2:30; 24/7 15′/2 cuộc/A2@T0+7:30; `T0`=lúc Core mở window)
  - `04-call-execution-dtmf.md` (script, phím 1/0, no-answer, invalid phone)
  - `05-result-normalization-callback.md` (result taxonomy, callback, revalidation)
  - `06-technical-exception-capacity.md` (tách technical vs no-answer)
  - `07-admin-operations.md` (pause/resume, disable SIM, manual retry, review)
  - `08-evidence-audit-privacy.md`

## Quy tắc
- Mỗi FR có: ID, mô tả, actor, precondition, trigger, postcondition, nguồn docs, acceptance hint.
- Giữ mã P0 gốc (`IVR00-P0-001` …) và tạo bảng traceability.
- Mọi rule số (max attempts, window seconds) phải trích nguồn.
- Đánh dấu `Owner Decision Required` cho các residual open decisions (ngưỡng trusted, retry count/backoff, recording policy, retention…).

## Checklist hoàn thành
- [ ] Mọi FR phase-8 đã được gom và không mất mã.
- [ ] Có bảng traceability FR → nguồn.
- [ ] Các P0 rule (không tự transition order, không gọi Quote/Cart/Draft, tách technical/no-answer, không tự notification) đều có FR tương ứng.
- [ ] Residual open decisions được liệt kê.

## Điều cấm
- KHÔNG thiết kế schema/DB (để p07) hay API endpoint (để p05) — chỉ mô tả hành vi.
- KHÔNG tạo FR cho tính năng ngoài scope (inbound, upsell) trừ khi owner duyệt.

## Báo cáo cuối
1. Tổng số FR + số P0.
2. Số FR có `Owner Decision Required`.
3. Coverage so với các doc phase-8 nguồn.
