# Gói ký — vế TTL của `OD-V1-17` · 2026-09-09

Trạng thái: `SIGNED` · Work ID: `W-0246` · Baseline: `main@55d552d`
Người ký: IVR owner (`marketingssv2024@gmail.com`) · Người soạn phương án: Claude
Nguồn: [open-decisions-register.md](../../specs/_review/open-decisions-register.md)

> Hồ sơ này giữ **lý do**. Trạng thái sống nằm ở register; tiến độ nằm ở tracker.

## 1. Đã ký gì

**`dial_token_expires_at` = đúng confirmation-window end.**

Quyết định này **thay thế** vế TTL của `OD-V1-17`, đóng ngày `2026-09-05` với
*"TTL = cửa sổ xác nhận + 60s"*.

## 2. Vì sao thay

`+60s` **chưa từng thi hành được**, và không phải vì thiếu một guard — vì thừa ba:

| Tầng | Luật |
| --- | --- |
| Intake `TaskIntakeService` | từ chối nếu expiry **<** window end |
| Persistence `PersistenceInvariantValidator` | throw nếu expiry **>** window end |
| Dispatch `PostgresTelephonyDispatchStore` | throw nếu expiry **>** `lease.Deadline` |

Giao của hai luật đầu là **đúng một giá trị**. Một task dựng theo `+60s` qua được intake rồi ném
exception ở persistence — chế độ hỏng khó chẩn đoán nhất trong cả seam, và là lý do
`IT-INTAKE-DB-03` tồn tại.

Hai đường đi được: sửa ba tầng để nhận `+60s`, hoặc ký equality. Chọn equality.

## 3. Vì sao equality đứng vững, không chỉ là chấp nhận hiện trạng

Token hết hạn đúng cuối cửa sổ nghĩa là **không quyền quay số nào sống lâu hơn cửa sổ gọi**. Đó là
tính chất muốn có, không phải hệ quả tình cờ của ba guard.

Điều `+60s` định bảo vệ — cuộc gọi bắt đầu sát mép cửa sổ vẫn quay xong — **đã có cơ chế riêng**:
dispatch ràng `DialTokenExpiresAt <= lease.Deadline`, nên lease lo phần in-flight. `+60s` là một
giải pháp thứ hai cho một vấn đề đã được giải, và nó mua thêm 60 giây quyền quay số sau khi cửa sổ
đóng.

## 4. Hệ quả

| | |
| --- | --- |
| Code | **không đổi gì** — ba tầng đã ép equality |
| M3 | **không đổi gì** — equality là thứ M3 vẫn gửi |
| OpenAPI | không đổi; lượt phát hành contract không còn phải chờ mục này |
| `IT-INTAKE-DB-03` | chuyển từ ghim **hiện trạng** sang ghim một **hợp đồng đã ký** |
| Worklist `2.1` | đóng |

## 5. Một lượt sai trên đường tới đây, ghi lại

`W-0245` (cùng ngày) khẳng định `+60s` *"không tồn tại trong bất kỳ tài liệu ký nào"* và rút mục
`2.1`. **Sai.** Nó nằm ở dòng `OD-V1-17` của register — nguồn mà `od-v1-signoff-2026-09-05` khai
ngay ở header.

Lượt đó kiểm bốn nguồn rồi tuyên bố đã kiểm hết, mà không kiểm nguồn tài liệu tự khai. Đã rút, và
`W-0208` được khôi phục là đúng.

Ghi ở đây vì hồ sơ quyết định phải chịu được việc đọc lại sáu tháng sau, kể cả phần đường đi vòng.
