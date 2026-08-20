# DT-06 — Semantics của operational/policy block

Trạng thái: `DECIDED`

Ngày: `2026-08-20`
Áp dụng: Current system / Target V1 draft trước Sales và one-SIM lab.

## Quyết định

1. `IVR_OPERATIONAL_BLOCKED` và `IVR_POLICY_BLOCKED` là **quyết định trước cuộc gọi**, không phải
   sự kiện cuộc gọi. Current IVR không được persist hoặc gửi chúng như `IvrResultCallbackV1`.
2. `IVR_OPERATIONAL_BLOCKED` vẫn là error code đồng bộ hợp lệ khi intake/runtime fail-closed.
   Nếu request bị chặn trước persistence thì không có call job, attempt hay call result.
3. Policy/eligibility block được lưu ở eligibility decision/blocked reasons khi có task snapshot;
   scheduler không dispatch và không tạo customer attempt.
4. Nếu khách đã bấm phím rồi Sales revalidate và phát hiện Sale Lock/Recall, dữ kiện IVR quan sát
   vẫn giữ nguyên. Sales trả ACK `BLOCKED_BY_CORE`; callback chuyển `DELIVERED_BLOCKED` và tạo
   review item. Không đổi result thành một mã blocked và IVR không đổi order state.
5. `operational_blocked_rate` và trường trend tương ứng trả `null` trong current system. Không
   được tính từ `ivr_call_results` và không được hiển thị `0`. Chỉ chuyển sang số khi có một fact
   source riêng ghi nhận intake/pre-call block với mẫu số được định nghĩa và kiểm thử.

## Tương thích

Hai enum blocked được giữ trong OpenAPI/domain taxonomy để không phá Target V1 draft và dữ liệu
lịch sử. Mapper outbound fail-closed nếu code cố dùng chúng. Đây là giữ mã tương thích, không phải
cam kết producer sẽ phát mã.

`IVR_CONFIRMATION_WINDOW_EXPIRED` cũng là mã dùng chung nhưng do timeout worker của Sales sở hữu;
quyết định này không thay đổi semantics đó.

## Bằng chứng bắt buộc

- Unit test: cả hai blocked result bị mapper outbound từ chối.
- Integration test: `BLOCKED_BY_CORE` giữ nguyên call result đã quan sát và tạo review item.
- Image E2E: tám result mà IVR thực sự sinh vẫn đi hết DTMF/disposition → callback.
- Analytics contract/UI: unavailable được biểu diễn bằng `null`/`—`, không phải zero.
