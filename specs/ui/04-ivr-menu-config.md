# UI-04 — IVR Menu / Script Config

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Permission xem: `IVR_QUEUE_VIEW`; approve script = owner. Nguồn: `functional/04`, `seed/ivr-menu.sample.json`.

## Mục đích
Xem/quản cấu hình call script (template + version + biến được phép). Chủ yếu **read-only + approve**; script chỉ dùng khi `approved`.

## Bố cục
```
[ Script templates: id · version · status(draft/approved) · call_purpose=ORDER_CONFIRMATION_ONLY ]
[ Detail: text_template (preview) · allowed_variables · prohibited_variables ]
[ DTMF map: 1=confirm · 0=cancel · 9=NOT_ENABLED ]
```

## Dữ liệu / ràng buộc
- `allowed_variables` (current, approved): `order_code_short`, `total_amount_display`, (opt) `customer_name_short`, `program_name`.
- `allowed_variables` (Target V1 proposal, `OD-V1-15` **OWNER_DECISION_REQUIRED**): thêm `items[].public_name`, `items[].quantity`, `delivery_area_short`. UI phải hiển thị rõ biến nào thuộc bộ chờ duyệt và **chặn approve script production** dùng biến chưa duyệt; MOCK/LAB được phép.
- `prohibited_variables` (hiển thị để nhắc): FULL_ADDRESS, MEMBER_TIER, DIAMOND, PAYMENT_DETAIL, ORDER_HISTORY, AI/CRM content, HEALTH.
- KEY_9 = `NOT_ENABLED` (AS-07) — không cho bật ở UI giai đoạn đầu.

## Actions
| Action | Permission | Ràng buộc |
| --- | --- | --- |
| Submit script version | owner/approver | tạo version mới, status=draft |
| Approve script version | owner sign-off | chỉ approved mới dispatch; audit |

## P0
- UI **không** cho thêm biến ngoài whitelist; không cho bật KEY_9 nếu chưa có owner decision (Q-F2). Script chưa approved → task reject (`SCRIPT_NOT_APPROVED`).
