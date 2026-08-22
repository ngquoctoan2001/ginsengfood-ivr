# UI-08 — Role / Permission & Matrix

Trạng thái: `SRS_DRAFT` · Cập nhật bởi: `W-0105` · Nguồn: API OpenAPI `draft.10`, `api/03`, quyết định owner 2026-08-22 (gồm `OD-V1-20`).

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
| `IVR_FLAG_READ` | ✅ | ❌ |
| `IVR_RUNTIME_GATE_ADMIN` | ✅ | ❌ |
| `IVR_ACCOUNT_VIEW` | ✅ | ❌ |
| `IVR_ACCOUNT_MANAGE` | ✅ | ❌ |
| `IVR_ACCOUNT_PASSWORD_RESET` | ✅ | ❌ |
| `IVR_ACCOUNT_SELF_VIEW` | ✅ | ✅ |

`IVR_FLAG_READ` và `IVR_RUNTIME_GATE_ADMIN` được cấp cho Admin từ 2026-08-22 theo
`OD-V1-20`; Operator không có cả hai. Chữ ký thứ hai của four-eyes còn trống —
xem `plan/ivr-orther/decisions-log.md`.

Hai quyền này có hiệu lực **khác nhau**:

- `IVR_FLAG_READ` có hiệu lực ngay: hai endpoint GET flag/kill-switch trả `200`
  cho Admin thay vì `403`.
- `IVR_RUNTIME_GATE_ADMIN` **chưa mở được gì**. `FeatureFlagAdminService`
  kiểm `IRuntimeGateAuthorization` trước, và bản đăng ký trong production
  (`PendingRuntimeGateAuthorization`) luôn trả `false`, nên `POST` chuyển từ
  `403 IVR_FORBIDDEN_CALLER` sang `409 IVR_OPERATIONAL_BLOCKED`.

Điều thay đổi là **thứ tự khóa**: permission không còn là lớp ngoài cùng. Mọi tài
liệu từng dựa vào mệnh đề "chưa gán cho vai trò nào" phải suy lại từ
`PendingRuntimeGateAuthorization`, giá trị cờ hiện tại, four-eyes trên chính
mutation, và audit log.

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
| Đọc feature flag / kill switch (API, chưa có màn riêng) | `IVR_FLAG_READ` | Admin |
| `POST /feature-flags/{env}` — đổi `executionMode`, `realCustomerCallAllowed`, `labDestinationAllowlist`, `globalDialKillSwitch`, `recordingEnabled` | `IVR_RUNTIME_GATE_ADMIN` | Admin qua được permission, nhưng hiện vẫn `409 IVR_OPERATIONAL_BLOCKED` do `PendingRuntimeGateAuthorization`; khi mở phải có `X-Actor-Id` khớp subject, `Idempotency-Key`, four-eyes, audit |
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
  attempt, hoặc bypass blocker.
- Từ `OD-V1-20`, Admin có **permission** gọi `POST /feature-flags/{env}`, nhưng
  mutation vẫn bị `PendingRuntimeGateAuthorization` chặn (`409
  IVR_OPERATIONAL_BLOCKED`). Không role nào bật được real-customer-call gate hôm
  nay. Điều kiện để mệnh đề đó thôi đúng là **thay implementation** đó — khi thay,
  ràng buộc P0 chuyển sang "mỗi lần bấm phải có four-eyes + `X-Actor-Id` khớp
  subject + audit". `REAL_CUSTOMER_CALL_ALLOWED=NO` vẫn là mặc định cho tới khi
  `DF-03` và `DT-01` đóng.
- Mutation vận hành phải có actor khớp subject, reason, audit, correlation và
  `no_policy_bypass=true` theo API contract.
- Không hiển thị raw phone, full address, recording hay secret/session token.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi chỉ vì account login thành công.

## 6. Test bắt buộc

- Unit drift test khóa đúng hai role và permission matrix trên, gồm cả việc Admin
  giữ `IVR_FLAG_READ`/`IVR_RUNTIME_GATE_ADMIN` và Operator thì không.
- E2E kiểm Operator chỉ thấy dashboard/calls/profile, thực hiện được disable SIM
  và technical retry, nhưng bị chặn accounts/roles/admin-only actions.
- E2E kiểm Admin CRUD account, reset password, revoke session và soft-delete.
- Direct API probes phải xác nhận 401 generic cho credential sai và 403 cho
  permission thiếu; client-side hidden state không được tính là bằng chứng RBAC.
