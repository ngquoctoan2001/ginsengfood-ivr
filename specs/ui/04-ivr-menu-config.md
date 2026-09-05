# UI-04 — IVR Menu / Script Config

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Permission xem: `IVR_QUEUE_VIEW`; lifecycle backend = W-0024; API/UI implementation = P2-8/P3. Nguồn: `functional/04`, `seed/ivr-menu.sample.json`.

## Mục đích
Xem/quản cấu hình call script (template + version + biến được phép). Script chỉ dùng khi version được duyệt đúng execution mode và chưa retired.

## Bố cục
```
[ Script templates: id · version · status(draft/in_review/approved/retired) · call_purpose=ORDER_CONFIRMATION_ONLY ]
[ Detail: text_template · exact sanitized preview · estimated duration · allowed/prohibited fields · template/input/content hash ]
[ Approval matrix: MOCK_TEST · LAB · CONTENT · PRIVACY_LEGAL · actor · reason · timestamp ]
[ DTMF map: 1=confirm · 0=cancel · 9=NOT_ENABLED ]
```

## Dữ liệu / ràng buộc
- `allowed_input_fields` Target V1: `customer_display_name`, `order_code_short`, `items[].public_name`, `items[].quantity`, optional `items[].unit_label`, `total_amount`, `currency`, `delivery_area_short`, `program_display_name`, `locale`, optional `pronunciation_hints`.
- `OD-V1-15` ✅ **ĐÃ KÝ `2026-09-05`** (`W-0194`): bộ rộng ở trên là whitelist production, và
  `ProductionTargetV1FieldsApproved` mặc định `YES` từ `W-0195`. UI vẫn phải hiển thị giá trị thật
  của khóa này thay vì giả định — nó có thể bị đặt lại `NO` cho một deployment cụ thể.
- Khóa đó **không** phải thứ duy nhất chặn production, và UI không được trình bày như vậy:
  `CONTENT` + `PRIVACY_LEGAL` phải đến từ **hai actor khác nhau**, và người tạo bản kịch bản không
  được duyệt chính nó. Ba actor id phân biệt cho một bản `PRODUCTION_REAL`.
- `prohibited_variables` (hiển thị để nhắc): FULL_ADDRESS, MEMBER_TIER, DIAMOND, PAYMENT_DETAIL, ORDER_HISTORY, AI/CRM content, HEALTH.
- KEY_9 = `NOT_ENABLED` (AS-07) — không cho bật ở UI giai đoạn đầu.

## Actions
Trạng thái `IMPLEMENTED` từ `W-0109`. Cột permission dùng **quyền console** (`IVR_SCRIPT_*`);
mỗi quyền ánh xạ 1-1 sang quyền domain `ivr.script.*` mà `ScriptActor` đòi.

| Action | Permission (console) | Ràng buộc |
| --- | --- | --- |
| Create draft version | `IVR_SCRIPT_EDIT` | tạo version mới; version là **bất biến** sau khi tạo, không overwrite |
| Submit for review | `IVR_SCRIPT_REVIEW` | `DRAFT → IN_REVIEW`; actor + reason + audit |
| Approve MOCK | `IVR_SCRIPT_APPROVE_MOCK` | cần `MOCK_TEST`; creator không tự duyệt (`403`) |
| Approve LAB | `IVR_SCRIPT_APPROVE_LAB` | cần `LAB`; không tự mở real-customer gate |
| Approve production content | `IVR_SCRIPT_APPROVE_CONTENT` | một nửa production gate |
| Approve Privacy/Legal | `IVR_SCRIPT_APPROVE_PRIVACY_LEGAL` | actor khác Content approver (`403`); vẫn chịu `OD-V1-15` |
| Retire version | `IVR_SCRIPT_RETIRE` | không delete; retired version fail-closed mọi mode |

**Vì sao màn này từng không có nút.** `W-0096` cố ý để read-only, lý do ghi trong
`AdminConfigReadService`: duyệt là quyết định của owner theo `OD-V1-15`, không phải nút bấm.
`W-0109` đảo lại **theo yêu cầu owner**, vì đường duy nhất còn lại để chữ ký Pháp chế vào hệ
thống là **sửa tay dữ liệu** — mà sửa tay thì mất audit, mất `creator ≠ approver`, và mất luôn
ý nghĩa của chính cái cổng đó. Mở qua khuôn admin mutation đặt chữ ký trở lại bên trong kiểm soát.

Hai chốt cứng **không** đổi: màn không có ô nào thêm biến ngoài whitelist, và không có ô nào bật
`KEY_9`. Cả hai nằm trong `TargetV1SpeechPolicy.ValidateTemplate`, chạy phía server cho mọi bản nháp.

## P0
- UI **không** cho thêm biến ngoài whitelist; không cho bật KEY_9 nếu chưa có owner decision (Q-F2). Script chưa approved đúng mode → task reject (`IVR_SCRIPT_NOT_APPROVED`). Không có A/B/random version selection.
