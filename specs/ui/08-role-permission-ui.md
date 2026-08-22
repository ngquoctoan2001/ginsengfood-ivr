# UI-08 — Role / Permission & Matrix

Trạng thái: `SRS_DRAFT` · Cập nhật bởi: `W-0105` · Nguồn: API OpenAPI `draft.10`, `api/03`, quyết định owner 2026-08-22.

## 1. Vai trò được hỗ trợ

Hệ thống chỉ có hai role canonical: `Admin` và `Operator`. Ivr.Api là nguồn sự
thật cho role/permission; client chỉ dùng projection từ session để ẩn/hiện UI,
không được tự cấp quyền hay tin claim do browser gửi.

## 2. Permission canonical

| Permission | Admin | Operator |
| --- | --- | --- |
| `IVR_QUEUE_VIEW` | ✅ | ✅ |
| `IVR_QUEUE_PAUSE` | ✅ | ❌ |
| `IVR_QUEUE_RESUME` | ✅ | ❌ |
| `IVR_SIM_ENABLE` | ✅ | ❌ |
| `IVR_SIM_DISABLE` | ✅ | ✅ |
| `IVR_MANUAL_RETRY` | ✅ | ✅ |
| `IVR_RESULT_REVIEW` | ✅ | ❌ |
| `IVR_ACCOUNT_VIEW` | ✅ | ❌ |
| `IVR_ACCOUNT_MANAGE` | ✅ | ❌ |
| `IVR_ACCOUNT_PASSWORD_RESET` | ✅ | ❌ |
| `IVR_ACCOUNT_SELF_VIEW` | ✅ | ✅ |

`IVR_FLAG_READ` và `IVR_RUNTIME_GATE_ADMIN` không được cấp cho hai role trong
W-0105; chúng tiếp tục fail-closed cho đến khi owner chốt gate riêng.

## 3. Ma trận màn hình và action

| Màn / Action | Permission | Phạm vi |
| --- | --- | --- |
| `/dashboard`, `/calls`, `/calls/{id}` | `IVR_QUEUE_VIEW` | Admin + Operator; chỉ projection masked |
| Pause/resume queue | `IVR_QUEUE_PAUSE` / `IVR_QUEUE_RESUME` | Admin |
| Disable SIM | `IVR_SIM_DISABLE` | Admin + Operator |
| Enable SIM | `IVR_SIM_ENABLE` | Admin |
| Technical retry | `IVR_MANUAL_RETRY` | Admin + Operator; không reset customer attempt |
| Admin review | `IVR_RESULT_REVIEW` | Admin |
| `/accounts`, `/roles` | `IVR_ACCOUNT_VIEW` | Admin |
| Create/update/disable/reactivate/delete account | `IVR_ACCOUNT_MANAGE` | Admin |
| Reset password/revoke sessions | `IVR_ACCOUNT_PASSWORD_RESET` | Admin |
| `/profile` | `IVR_ACCOUNT_SELF_VIEW` | Admin + Operator; chỉ subject hiện tại |
| `/reports`, `/review`, `/config`, `/integration`, `/seed` | Admin role | Operator nhận 403/không render dữ liệu |

Mọi page, Route Handler và server action phải kiểm quyền server-side. Ẩn nav hay
button chỉ là UX; gọi thẳng API sai quyền vẫn phải nhận
`403 IVR_FORBIDDEN_CALLER`.

## 4. Authentication/session

- Login dùng username/password và gọi Ivr.Api qua Next.js server.
- Browser chỉ nhận opaque token trong cookie `httpOnly`, `SameSite=Strict`,
  `Secure` ngoài development; không chứa permission directory hay password.
- Mỗi request bearer phải resolve session chưa revoke/chưa hết hạn và account
  đang `ACTIVE`, rồi derive permission từ role phía server.
- Sign-out revoke session server-side rồi xóa cookie.
- Operator chỉ xem profile chính mình; không có endpoint/client filter cho phép
  chọn subject khác.

## 5. Ràng buộc P0

- Không role nào được force confirm/cancel order, reset attempt count, vượt max
  attempt, bypass blocker, hoặc bật real-customer-call gate.
- Mutation vận hành phải có actor khớp subject, reason, audit, correlation và
  `no_policy_bypass=true` theo API contract.
- Không hiển thị raw phone, full address, recording hay secret/session token.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi chỉ vì account login thành công.

## 6. Test bắt buộc

- Unit drift test khóa đúng hai role và permission matrix trên.
- E2E kiểm Operator chỉ thấy dashboard/calls/profile, thực hiện được disable SIM
  và technical retry, nhưng bị chặn accounts/roles/admin-only actions.
- E2E kiểm Admin CRUD account, reset password, revoke session và soft-delete.
- Direct API probes phải xác nhận 401 generic cho credential sai và 403 cho
  permission thiếu; client-side hidden state không được tính là bằng chứng RBAC.
