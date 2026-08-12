# P06 — Generate Data Mapping

## Tên nhiệm vụ
Sinh data ownership, data mapping (sales/ops), missing data, PII policy.

## Bối cảnh
Ranh giới sở hữu dữ liệu đã khóa ở `IVR-00` §5, `IVR-02`. IVR chỉ giữ snapshot/ref, KHÔNG là source-of-truth của order state/payment/inventory/recall. Prompt này lập bảng ánh xạ từng trường IVR cần ↔ owner ↔ cách lấy.

## Input cần đọc
- `specs/srs/api/*`, `specs/srs/functional/*`
- `docs/documents/1. master/02-MASTER-01-SOURCE-OF-TRUTH.md`, `04-MASTER-03-TRACEABILITY-ID.md`
- `docs/documents/4. phase/phase-8/02, 04, 07, 08, 12`
- `plan/ivr-orther/04-module-dependency-map.md`, `10-integration-gap-analysis.md`, `13-seed-and-mock-strategy-plan.md`

## Output cần tạo
- `specs/srs/data/`:
  - `00-index.md`
  - `01-data-ownership.md` (bảng: nhóm dữ liệu → owner → IVR read/write/none)
  - `02-mapping-sales-platform.md` (field IVR ↔ field sales, chiều, resolver)
  - `03-mapping-ops-core.md` (sale-lock/recall/availability snapshots)
  - `04-missing-data.md` (`GAP` dữ liệu chưa có nguồn)
  - `05-pii-policy.md` (phone_ref/masked/token, cấm raw phone, cấm full address/payment/health)

## Quy tắc
- Mỗi trường ghi: owner, chiều (read/snapshot/none), privacy class, resolver, `CONFIRMED/ASSUMPTION/GAP`.
- Bám danh sách "Data allowed / Data prohibited" của `IVR-02` §11.
- Trace ID theo `MASTER-03` (order_code, ivr_call_id, ivr_call_result_event_id, correlation_id).

## Checklist hoàn thành
- [ ] Mọi trường trong `IvrConfirmationTaskV1` có dòng ownership.
- [ ] PII policy nêu rõ cấm lưu raw phone/recording mặc định.
- [ ] Missing data list gắn priority + owner.

## Điều cấm
- KHÔNG map IVR như owner của order/payment/inventory.
- KHÔNG cho phép trường PII vượt policy.

## Báo cáo cuối
1. Số trường mapped (sales/ops).
2. Số `GAP` dữ liệu.
3. Điểm PII rủi ro cao.
