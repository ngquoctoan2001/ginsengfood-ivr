# UI-08 — Module 3 role to IVR tier handover

Trạng thái: `OWNER_DECISION_REQUIRED` · Work item: `W-0128` · Authority: IR-06 §4A.

IVR không định nghĩa role người dùng. Module 3 xác thực operator, quyết định role nào được dùng
capability nào, rồi BFF của Module 3 gọi Ivr.Api bằng token tier tương ứng. Token không được đi tới
browser; `X-Actor-Id` phải là subject ổn định của operator, không phải display name tự nhập.

## 1. Màn hình → tier tối thiểu

| Màn/chức năng | Tier tối thiểu | Evidence thêm |
| --- | --- | --- |
| Dashboard, call log/detail, reports, review list, config/integration read | `read` | `X-Actor-Id` |
| Tạo admin review; script draft/submit/approve/retire | `write` | `X-Actor-Id`; script approval còn `X-Script-Permissions` |
| Seed/dry-run/integration profile non-prod | `write` | `X-Actor-Id`; route không tồn tại ở production |
| Pause/resume queue, terminate, retry, SIM enable/disable | `danger` | `X-Actor-Id` + `X-Action-Reason` |
| Runtime feature-flag mutation | `danger` | actor + reason + idempotency + các approval gate riêng |

Tier lồng nhau `danger ⊇ write ⊇ read`, nhưng UI/BFF phải chọn token thấp nhất cần thiết. Việc một
credential tier cao có thể qua endpoint tier thấp không phải lý do phát nó cho mọi role.

## 2. Mapping role của Module 3

| Module 3 role/claim | Read | Write | Danger | Trạng thái |
| --- | --- | --- | --- | --- |
| _M3 điền_ | _M3 điền_ | _M3 điền_ | _M3 điền_ | `OWNER_DECISION_REQUIRED` |

Fail-closed: role không có trong mapping đã ký không nhận tier nào. `danger` phải được grant tường
minh và không được suy ra chỉ từ tên role như `Admin`.

## 3. Acceptance bắt buộc phía Module 3

1. Token chỉ tồn tại trong secret store và BFF/server runtime; kiểm bundle/HTML/browser storage
   không có token.
2. Actor lấy từ subject đã xác thực và được ghi xuyên suốt IVR audit; không cho browser tự chọn.
3. Mỗi role có positive/negative integration test ở đúng tier; đặc biệt role read/write phải bị
   từ chối trên mọi danger endpoint.
4. Rotation hỗ trợ current/previous trong cửa sổ hữu hạn và xoá previous sau retirement instant.
5. Client được sinh lại từ OpenAPI `1.0.0-draft.22`; không còn route account/session.

Chưa có sign-off/mapping từ Module 3 thì trạng thái chỉ là `LOCAL_CONTRACT_READY`, không phải
integration-ready hay production-ready.
