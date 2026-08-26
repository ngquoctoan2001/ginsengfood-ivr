# UI-07 — Seed / Mock Management (NON-PROD only)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Permission: `IVR_SIM_ENABLE`/`IVR_SIM_DISABLE` (+ non-prod guard). Nguồn: `seed/*`, DT-01.

## Mục đích
Điều khiển môi trường test: `adapter_mode` (MOCK/REAL), nạp seed, chọn call-scenario, bật/tắt integration-status profile để chạy dry-run/smoke.

## Bố cục
```
[ Environment banner: NON-PROD · REAL_CUSTOMER_CALL_ALLOWED=NO ]
[ Adapter: adapter_mode = MOCK (REAL disabled tới khi mua SIM + release gate) ]
[ Seed loader: customers/orders/products/inventory/tasks ]
[ Scenario runner: chọn SCN-* -> chạy dry-run -> xem result mong đợi vs thực tế ]
[ Integration-status profile: chọn STATUS-* (all-up / *-down / ready-503) ]
[ SIM channels (mock): enable/disable (non-prod) ]
```

## Actions
| Action | Permission | Ràng buộc |
| --- | --- | --- |
| Đổi adapter_mode | `IVR_SIM_ENABLE` + non-prod | **REAL bị khóa** tới khi mua SIM (DT-01) + release gate (DF-03) |
| Chạy scenario dry-run | ops non-prod | không gọi khách thật |
| Áp integration-status profile | ops non-prod | để test fail-closed |

## P0
- Màn này **chỉ hiện ở non-prod**; production ẩn. Không cho set `REAL` khi chưa pass release gate. Không seed vào prod.

## Đã triển khai (W-0112)

Ba action ở trên giờ có API. Ba điểm khác với bản draft này, ghi lại để bản sau không "sửa lại
cho đúng spec" một cách sai:

**1. Quyền là `IVR_DEV_TOOLING`, không phải `IVR_SIM_ENABLE`/`IVR_SIM_DISABLE`.** Nạp seed và
chạy scenario không phải thao tác SIM. Gộp vào quyền SIM nghĩa là một operator được phép tắt kênh
hỏng cũng ghi được dữ liệu vào cơ sở dữ liệu. Quyền mới chỉ cấp cho Admin.

**2. Production trả `404`, không phải `403`.** Route **không được đăng ký** khi triển khai là
production — theo tên môi trường (danh sách cho phép, không phải danh sách cấm), theo
`IVR_EXECUTION_MODE`, hoặc theo `REAL_CUSTOMER_CALL_ALLOWED`. `403` sẽ xác nhận với người gọi
rằng có một seed loader ở địa chỉ này và chỉ còn một cái quyền chắn giữa họ với nó.

**3. "Áp integration-status profile" chỉ thi hành được `SIM_GATEWAY`.** Bốn phụ thuộc còn lại
(`ORDER_CORE`, `CRM_DO_NOT_CALL`, `EVIDENCE_REGISTRY`) được **khai báo chứ
không thi hành**: IVR không thăm dò chúng và báo `NOT_WIRED` (xem `AdminConfigReadService`), nên
không có gì trong hệ đang chạy đọc trạng thái vừa đặt. Phản hồi và màn hình nói rõ cái nào là
cái nào. Đây là giới hạn thật, gỡ được khi `W-0040` có probe thật.

### Seed loader — điều nó sửa của file mẫu

`seed/sales-target-v1.sample.json` ghi mốc thời gian tuyệt đối tháng 8/2026 với cửa sổ xác nhận
dài 5–15 phút. Nạp nguyên trạng thì **cả 9 tác vụ đều bị từ chối** vì
`ORDER_NOT_CALLABLE_OR_WINDOW_EXPIRED`. Nên loader dời cửa sổ của **từng** tác vụ về hiện tại
(mặc định; tắt bằng `rebase_windows: false`).

Dời theo từng tác vụ chứ không dời chung một khoảng: dời chung giữ nguyên độ lệch 2 giờ 20 phút
của file, tức là mỗi lúc chỉ có đúng một tác vụ gọi được và buổi nghiệm thu phải đợi. Độ lệch đó
là cách file mô tả một dòng thời gian để **phát lại**, không phải hình dạng một môi trường demo
cần có.

Loader đi qua đúng `ITaskIntakeService` mà production dùng, nên: `TASK-TARGET-247-0005`
(`call_restriction: true`) vẫn bị chặn `IVR_OPERATIONAL_BLOCKED`, và loader **không** có lối nào
đưa một khách đã từ chối nhận cuộc gọi vào hàng đợi.

Loader cũng đăng ký sẵn attempt policy `mock-lab-v1` (chỉ MOCK/LAB). Không có bước đó thì mọi tác
vụ trả `TASK_HELD_POLICY_MISSING` trên một cơ sở dữ liệu mới.

**Chạy lần hai không làm mới cửa sổ.** Khoá idempotency giữ nguyên nhưng nội dung đã đổi (cửa sổ
mới), nên mỗi tác vụ báo `IVR_IDEMPOTENCY_CONFLICT` và không thêm gì. Muốn cửa sổ mới thì dựng
lại cơ sở dữ liệu.

### Scenario runner — nó trả lời được câu hỏi nào

Phát lại các lần gọi đã ghi qua `DispositionMapper` rồi đối chiếu với `expected_result_type` /
`expected_counted`. **Không phát cuộc gọi nào** — và đó là tính chất cấu trúc: `CallScenarioDryRun`
nằm ở `Ivr.Domain`, không giữ cổng telephony nào, nên không có mã gọi điện trên lối đó để mà tắt.

Scenario mà kết quả mong đợi **không** do chuẩn hoá disposition sinh ra (`IVR_CONFIRMATION_WINDOW_EXPIRED`
do luồng quét hết hạn, `IVR_OPERATIONAL_BLOCKED` do tiếp nhận) trả `NOT_REPLAYABLE` và **không**
đưa ra phán quyết. Gọi đó là "lệch" sẽ đẩy người đọc đi tìm lỗi ở sai chỗ.
