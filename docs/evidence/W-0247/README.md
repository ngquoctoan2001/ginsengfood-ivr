# W-0247 — Đọc lại `2.2`–`2.6`: năm mục, năm kết luận khác nhau

Ngày: 2026-09-09 · Baseline: `main@aa241b4` · Trạng thái: **TESTS_PASS**.

`2.1` vừa cho thấy một mục có thể nhỏ hơn hẳn cách nó tự mô tả. Đọc lại bốn mục còn lại — lần này
**bắt đầu từ nguồn mà chính mục đó khai**, đúng bài học `W-0246`.

## Kết quả một dòng mỗi mục

| # | Trước | Sau khi kiểm |
| --- | --- | --- |
| `2.2` | *M3 + Security* | **anh + dev M3** — mâu thuẫn là thật, nhưng đó là OAS **của chính IVR** |
| `2.3` | *M3 + Product* | **một câu hỏi**, không phải dự án |
| `2.4` | việc chưa làm | **không phải việc** — trạng thái hiện tại **đúng spec** |
| `2.5` | *M3 + Security* | **anh quyết** · ⚠️ **mục duy nhất gây hại cho khách** |
| `2.6` | M3 dựng | không đổi — việc build của dev M3 |

## 1. `2.4` không phải việc, và spec nói ngược với worklist

Mục khai nguồn là `C3`/`C4`/`C7`. Ba mục đó khai nguồn là **spec V0.3 §6**. Đọc chính hai dòng đó:

> `ivr_task` — *"Active upstream contract **chưa có** session field hoặc `priority`; **`W-0146` đề
> xuất** `golden_hour_session_id` nhưng **chưa được M3 ký và chưa được phép triển khai**."*
>
> `capacity_incident` — *"`session_id` là capacity scope ID **nội bộ/synthetic của IVR**. `W-0146`
> **cấm map đè** upstream ID; sau chữ ký M3 chỉ được thêm **cột nullable riêng**."*

Worklist viết mục này như một khoảng trống M8 phải lấp: *"Việc: ký field → producer mapping → store
→ CDC → enforce"*. Spec thì nói **cấm triển khai trước chữ ký**, và giá trị tự sinh `SCHED-` **là
thiết kế**.

⇒ **M8 không nợ gì.** Trạng thái hiện tại là trạng thái spec yêu cầu.

Và `MASTER-03 §24.5` — chỗ duy nhất trong toàn bộ `docs/documents/` nêu `golden_hour_session_id` —
đặt nó trong danh sách **Required Trace Links của `quote_snapshot_id`**, giữa `quote_cart_id` và
*"Quote dẫn tới order draft"*. Không dòng nào gắn nó cho IVR; `grep` toàn bộ tài liệu hệ thống tìm
`golden_hour_session` cạnh `ivr`/`module 8`/`confirmation` → **0 hit**.

## 2. `2.2` — mâu thuẫn thật, nhưng là hợp đồng của chính mình

| | |
| --- | --- |
| OAS | `phone_validation_status: { type: string }` — **không** enum, **không** trong `required` |
| Runtime | `EligibilityRules.cs:225-227` từ chối mọi giá trị ≠ `"VALID"` (Ordinal) |

Mâu thuẫn đúng như mục mô tả. Nhưng `ivr-order-confirmation.v1.yaml` là **OAS của IVR** — M3 là bên
gọi, không phải bên sở hữu. Nên không cần "M3 + Security" duyệt một contract của mình; cần **anh
quyết** và **dev M3 biết trước**, rồi gộp vào lượt phát hành cùng `7.1`.

Phần token custody (`MockDialTokenVault`/`LabDialTokenVault`, chưa có production issuer) **vẫn là
việc thật của phía Sales** — kiểm lại `git grep "class.*DialTokenVault"` chỉ ra đúng hai lớp.

## 3. `2.3` — một câu hỏi, không phải một dự án

Phần M8 xong từ `27/08`. Phần còn lại chờ *"M3 công bố registry"* — mà registry đó **chưa tồn tại**,
và OAS hiện khai `enum: [GOLDEN_HOUR, TWENTY_FOUR_SEVEN]`, đúng bằng toàn bộ phạm vi V1.

⇒ Hỏi dev M3 một câu: **registry đó có kế hoạch ra đời không?** Không thì enum giữ nguyên và mục
đóng.

## 4. `2.5` — mục duy nhất gây hại cho khách

Kiểm lại cả hai vế, cả hai đúng:

```text
grep sale_lock|recall  trên src/Ivr.Infrastructure/Scheduling/   → 0 hit
grep revoke            trên OAS                                   → 0 hit
```

> **Khách đã hủy đơn vẫn nhận cuộc gọi hỏi xác nhận chính đơn đó.**

Bốn mục kia sai ở tài liệu; mục này sai ở **cuộc gọi**. Và nó từng mang nhãn *"M3 + Security"* — hai
bên không tồn tại — nên đã đứng yên. Nay là **anh quyết**, và quyết được ngay.

Ba phương án giữ nguyên (A/B/hybrid). Cảnh báo kỹ thuật giữ nguyên: nếu chọn B thì phải fence tới
**tận trước dial**, vì technical lease generation hiện có **không phải** order-revocation generation.

## 5. `2.6` — không đổi

Shape đã implement, `W-0207` đã sửa mâu thuẫn ACK. Đường gửi thật tắt **có chủ đích**. Việc còn lại
là dev M3 dựng consumer + auth + shared E2E. Đó là việc build thật, không phải một quyết định đang
kẹt.

## 6. Kiểm chứng

```text
spec V0.3 §6 dòng 164, 170     đọc nguyên văn, dẫn trong evidence
MASTER-03 §24.5                chủ ngữ là quote_snapshot_id; 0 hit gắn cho IVR
OAS phone_validation_status    { type: string }, không enum, không required
EligibilityRules.cs:225-227    Ordinal == "VALID"
DialTokenVault                 đúng 2 lớp: Mock, Lab
Scheduling/ grep recall        0 hit · OAS grep revoke  0 hit
dotnet test Ivr.sln            949/949 (không đổi — lượt này không sửa code)
gate-status.mjs                GATE_STATUS_PASS
```

Không sửa code. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Việc gần nhất

`2.5`. Nó là mục duy nhất trong nhóm 2 mà cái giá của việc **không** quyết rơi vào khách hàng, chứ
không rơi vào một bảng trạng thái.
