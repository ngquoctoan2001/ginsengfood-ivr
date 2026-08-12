# UI-02 — Call Log (danh sách CallJob)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Permission: `IVR_QUEUE_VIEW`. Nguồn: `api/02`,`/03`, `data/05`.

## Mục đích
Danh sách call-job để theo dõi/lọc; vào chi tiết (UI-03).

## Bố cục
```
[ Filter: program · status · queue_status · deadline(near-expiry) · result_type · date ]
[ Table columns: order_code_short · phone_masked · program · job status · attempt (n/2) · result_type · deadline · updated_at ]
[ Row action: > Xem chi tiết ]
```

## Dữ liệu hiển thị / ẩn
- Hiển thị: `order_code_short`, `phone_masked`, program, status, attempt count (≤2), result_type, deadline.
- **Ẩn**: raw phone, `dial_token`, full address, payment, customer full name, health.

## Filter chính
- `deadline near-expiry` (ưu tiên GH 5′), `status=held/blocked`, `result_type=TECHNICAL_EXCEPTION` (soi lỗi kỹ thuật), `program`.

## Actions
- Chỉ **xem** (view-only) ở màn list. Không action thay đổi state ở đây.

## P0
- Không cột nào lộ raw phone (P0-IVR-007). Không export chứa PII.
