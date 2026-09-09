# W-0248 — Owner chọn `B` cho `2.5`; fence phải nằm ở đâu, và nó **không** đóng hết được

Ngày: 2026-09-09 · Baseline: `main@ce2f7d1` · Trạng thái: **TESTS_PASS**.

`2.5` là mục duy nhất trong nhóm 2 mà cái giá của việc không quyết rơi vào khách hàng. Owner chọn
**`B` — M3 phát revoke**.

## 1. Cảnh báo trong mục là toàn bộ độ khó

> *"technical lease generation hiện có **không phải** order-revocation generation."*

Đọc code thì đúng như vậy. `PostgresTelephonyDispatchStore.EnsureCurrentLease:451-469` kiểm **bảy**
điều kiện — job id, channel id, attempt status, `ActiveCallJobId`, `LeaseToken`,
`LeaseFencingGeneration`, `LeaseExpiresAt` — và **không điều nào** là trạng thái đơn hàng.

Nó chặn worker trùng và lease cũ. Một đơn bị hủy sau khi claim **đi qua nó không vướng gì**.

## 2. Đúng hai chỗ, cả hai đã xác minh

### Fence 1 — claim, rẻ nhất

`PostgresSchedulerStore.TryClaimDueDispatchAsync` **đã join sẵn** bảng task:

```sql
FROM ivr_call_jobs job
JOIN ivr_confirmation_tasks task ON task.task_id = job.task_id
WHERE job.eligible IS TRUE ...
```

⇒ **một vị từ**: `AND task.revoked_at IS NULL`. Không thêm join, không thêm truy vấn.

Chặn mọi revoke đến trước claim — phần lớn trường hợp thật, vì cửa sổ xác nhận dài hàng phút còn
claim→dial tính bằng giây.

### Fence 2 — lần đọc task cuối trước dial

`LoadAsync` đọc task rồi kiểm TTL ngay:

```csharp
ConfirmationTaskEntity task = await context.ConfirmationTasks.AsNoTracking()...
if (task.DialTokenExpiresAt > lease.Deadline) { throw ... }
```

Fence thu hồi nằm **ngay cạnh** guard đó — chỗ cuối cùng IVR còn thấy trạng thái task.

## 3. Giới hạn, nói trước khi ai hứa nhầm

`LoadAsync` đọc `AsNoTracking`, **không** `FOR UPDATE`. Revoke rơi vào khoảng
`LoadAsync` → gateway dial **vẫn lọt**.

Và **không nên** đóng: đóng nghĩa là giữ một transaction DB xuyên suốt một cuộc gọi ra ngoài.

> `B` **giảm** cửa sổ từ *"cả cửa sổ xác nhận"* xuống *"vài mili-giây"*. Nó **không** làm cửa sổ
> bằng không. Một cuộc gọi ra ngoài không phải thao tác giao dịch, và không thiết kế nào đổi được.

Ghi ra đây vì đó chính là loại hứa hẹn mà sáu tháng sau ai đó sẽ đọc thành *"đã chặn được"*.

## 4. Chỗ dễ bỏ sót nhất: `order_version`

Task đã mang `OrderVersion` từ intake (một trong 48 cột của `ConfirmationTaskEntity`).

Revoke **phải** mang version, và IVR **phải** so. Không so thì **một revoke đến muộn có thể hủy một
đơn mới hơn** — đơn đã được tạo lại sau khi bản cũ bị hủy.

## 5. Vì sao lượt này dừng ở đặc tả

Bước còn thiếu là **endpoint cho M3**, và OAS chỉ nên mở **một** lần: `7.1` (header) và `2.2`
(`phone_validation_status`) đã xếp hàng ở đó. Mở lần thứ ba cho revoke là re-pin hash OAS ở **5 nơi**
thêm một lượt nữa.

Hai fence và migration thì dựng được ngay khi hình dạng contract chốt — chúng chỉ phụ thuộc **có
cột hay không**, không phụ thuộc endpoint trông thế nào.

## 6. Kiểm chứng

```text
EnsureCurrentLease:451-469     7 điều kiện, 0 liên quan trạng thái đơn
claim SQL                      đã JOIN ivr_confirmation_tasks — fence 1 là một vị từ
LoadAsync                      đọc task + guard TTL — fence 2 nằm cạnh
ConfirmationTaskEntity         48 cột, có OrderState/OrderVersion, KHÔNG có cột revocation
dotnet test Ivr.sln            949/949 (không đổi — lượt này không sửa code)
gate-status.mjs                GATE_STATUS_PASS
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| — | chốt hình dạng revoke (`task_id` + `order_version` + `reason`) | Owner + dev M3 |
| — | migration + hai fence + test | **M8**, ngay sau khi chốt |
| — | endpoint + OAS | **lượt phát hành contract**, cùng `7.1` và `2.2` |
