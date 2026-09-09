# M8-17 — Fence thu hồi đơn giữa cửa sổ · phương án `B`

**Ngày:** `2026-09-09` · **Work ID:** `W-0248` · **Quyết định:** owner chọn **`B` — M3 phát revoke**
**Trạng thái:** `FENCES_IMPLEMENTED (W-0249) / ENDPOINT_PENDING` · `REAL_CUSTOMER_CALL_ALLOWED=NO`

> Worklist `2.5` cảnh báo một câu, và nó là toàn bộ độ khó của phương án này:
> *"technical lease generation hiện có **không phải** order-revocation generation."*
> Tài liệu này chỉ ra **đúng hai chỗ** fence phải nằm, và **giới hạn** của nó.

## 1. Vấn đề, nói bằng thời gian

```text
M3 tạo task ──► scheduler claim ──► LoadAsync ──► DIAL ──► khách nghe chuông
                      ▲                  ▲
                      │                  └── fence 2: lần đọc task CUỐI CÙNG
                      └── fence 1: chỗ rẻ nhất, chặn phần lớn
```

Hôm nay **không có fence nào**. `EnsureCurrentLease` chặn worker trùng, lease hết hạn, generation cũ
— **toàn bộ là kỹ thuật**. Một đơn bị hủy sau khi claim đi qua nó **không vướng gì**.

## 2. Fence 1 — claim

`PostgresSchedulerStore.TryClaimDueDispatchAsync`, câu SQL claim **đã join sẵn** bảng task:

```sql
FROM ivr_call_jobs job
JOIN ivr_confirmation_tasks task ON task.task_id = job.task_id
WHERE job.eligible IS TRUE
  ...
```

⇒ thêm **một vị từ**: `AND task.revoked_at IS NULL`.

Rẻ nhất, và chặn mọi revoke đến **trước** lúc claim — phần lớn trường hợp thật, vì cửa sổ xác nhận
dài hàng phút còn claim→dial tính bằng giây.

## 3. Fence 2 — lần đọc task cuối trước dial

`PostgresTelephonyDispatchStore.LoadAsync` đọc task rồi kiểm TTL:

```csharp
ConfirmationTaskEntity task = await context.ConfirmationTasks.AsNoTracking()...
if (task.DialTokenExpiresAt > lease.Deadline) { throw ... }
```

Fence thu hồi nằm **ngay cạnh** guard đó. Đây là chỗ cuối cùng IVR còn nhìn thấy trạng thái task
trước khi gateway quay số.

## 4. ⚠️ Giới hạn: cửa sổ này **không đóng được hết**

`LoadAsync` đọc `AsNoTracking`, không `FOR UPDATE`. Một revoke rơi vào khoảng
**`LoadAsync` → gateway dial** vẫn lọt.

Và **không nên** đóng nó: đóng nghĩa là giữ một transaction DB xuyên suốt một cuộc gọi ra ngoài —
đúng thứ không được làm.

> **Nói thẳng ra để không ai hứa nhầm:** phương án `B` **giảm** cửa sổ từ *"cả cửa sổ xác nhận"*
> xuống *"vài mili-giây giữa `LoadAsync` và dial"*. Nó **không** làm cửa sổ bằng không. Một cuộc gọi
> ra ngoài không phải thao tác giao dịch, và không thiết kế nào đổi được điều đó.

## 5. Hình dạng contract — đề xuất, chưa tự chốt

M3 gọi IVR để thu hồi. Ba trường tối thiểu:

| Trường | Vì sao |
| --- | --- |
| `task_id` | định danh |
| `order_version` | **ghi lại và trả lại** — xem đính chính dưới |
| `reason` | phân biệt *hủy đơn* với *thu hồi kỹ thuật* — hai thứ này khác nhau ở `call_result` |

> ⛔ **Đính chính khi dựng (`W-0249`).** Bản đầu của mục này viết *"revoke mang version ≤ version đã
> lưu thì từ chối"*. **Không thi hành được, và sai nguyên tắc.** `order_version` là **chuỗi mờ IVR
> trả nguyên** — OAS gọi nó là *stale-result guard snapshot*, IR-06 ghi *"IVR trả nguyên giá trị
> trong callback"*, `OrderVersion` là `DomainStringValue` **không** `IComparable`, và không chỗ nào
> trong repo so hai giá trị với nhau.
>
> IVR **không có thứ tự** trên trường này nên **không thể** biết revoke nào mới hơn. Thứ tự thuộc
> **Order Core** — đó chính là lý do trường này tồn tại. Cột `revoke_order_version` vì vậy **ghi lại
> và trả lại**, để bên có thứ tự phán. Trường vẫn cần trong contract; vai trò của nó là **audit +
> echo**, không phải *"IVR từ chối revoke cũ"*.

## 6. Cần gì để dựng

| # | Việc | Ghi chú |
| ---: | --- | --- |
| 1 | Cột `revoked_at` + `revoke_reason` + `revoke_order_version` trên `ivr_confirmation_tasks` | migration mới; **không** sửa migration cũ (`8.1`) |
| 2 | Fence 1 — một vị từ trong claim SQL | rẻ |
| 3 | Fence 2 — cạnh guard TTL trong `LoadAsync` | rẻ |
| 4 | Endpoint nội bộ cho M3 | **đi cùng lượt phát hành contract** với `7.1` và `2.2` |
| 5 | Test: revoke trước claim · revoke giữa claim và dial · revoke cũ bị từ chối | |

Bước `4` là lý do lượt này dừng ở đặc tả: OAS chỉ nên mở **một** lần, và `7.1` cùng `2.2` đã xếp
hàng ở đó.

## 7. Việc M3 phải làm

Gọi endpoint khi đơn bị hủy hoặc `sale_lock` bật. **Không** dựa vào ACK của callback — ACK là phản
hồi cho một kết quả đã xảy ra, không phải một lệnh.
