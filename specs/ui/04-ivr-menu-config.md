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
- `OD-V1-15` vẫn **OWNER_DECISION_REQUIRED**: UI phải hiển thị khóa `ProductionTargetV1FieldsApproved=NO` và không diễn giải CONTENT+PRIVACY_LEGAL là production-ready khi khóa này chưa đóng.
- `prohibited_variables` (hiển thị để nhắc): FULL_ADDRESS, MEMBER_TIER, DIAMOND, PAYMENT_DETAIL, ORDER_HISTORY, AI/CRM content, HEALTH.
- KEY_9 = `NOT_ENABLED` (AS-07) — không cho bật ở UI giai đoạn đầu.

## Actions
| Action | Permission | Ràng buộc |
| --- | --- | --- |
| Create draft version | `ivr.script.edit` | tạo version mới; không overwrite version cũ |
| Submit for review | `ivr.script.review` | `DRAFT → IN_REVIEW`; actor + reason + audit |
| Approve MOCK | `ivr.script.approve.mock` | cần `MOCK_TEST`; creator không tự duyệt |
| Approve LAB | `ivr.script.approve.lab` | cần `LAB`; không tự mở real-customer gate |
| Approve production content | `ivr.script.approve.content` | một nửa production gate |
| Approve Privacy/Legal | `ivr.script.approve.privacy-legal` | actor khác Content approver; vẫn chịu `OD-V1-15` |
| Retire version | `ivr.script.retire` | không delete; retired version fail-closed mọi mode |

## P0
- UI **không** cho thêm biến ngoài whitelist; không cho bật KEY_9 nếu chưa có owner decision (Q-F2). Script chưa approved đúng mode → task reject (`IVR_SCRIPT_NOT_APPROVED`). Không có A/B/random version selection.
