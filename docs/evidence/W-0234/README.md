# W-0234 — Đoạn số tiền đi bằng clip, và clip thứ 107 về đúng chủ

Ngày: 2026-09-08 · Baseline: `main@f69c731` · Trạng thái: **TESTS_PASS**.

`W-0233` dựng đường số trong speller. Lượt này nối **renderer** vào — phần vẫn không cần `3b`/`4`.

## 1. Vì sao `total_amount_display` dựng được ngay

Ba placeholder động:

| | Cần gì |
| --- | --- |
| `{{items_spoken}}` | tên hàng + đơn vị → **chờ `3b` và `4`** |
| `{{delivery_area_short}}` | danh sách vùng → **chờ `4`** |
| **`{{total_amount_display}}`** | **số + `đồng`** — không dính cái nào |

Và nó là đoạn **khách bấm phím để xác nhận**, nên đáng đi trước.

## 2. Cảnh báo impact, nói trước

`gitnexus impact VietnameseOrderScriptRenderer` → **MEDIUM**, 6 direct, 1 module (`Telephony`),
0 execution flow — kèm cảnh báo `epistemic: lower-bound`: *"CreateVietnameseNumbers is an interface
with 1 interface-level consumer; actual impact may be higher."*

Nên bản sửa được giữ **tối thiểu**: API chuỗi không đổi một chữ ký nào, chỉ đảo chiều bên trong một
biến cục bộ.

```csharp
// trước
string totalAmount = string.Concat(Spell(amount, style), " đồng");
// sau
string totalAmount = SpeechNumberClip.Join(TotalAmountClips(amount, style));
```

## 3. Clip thứ 107 về đúng chủ

`W-0233` phát hiện speller chỉ sở hữu **106** clip; `đồng` là của renderer. Nay điều đó **có trong
code** chứ không chỉ trong tài liệu:

```csharp
private static readonly SpeechNumberClip CurrencyClip = new("num-dong", "đồng");
```

`UT-VOICE-CLIP-10` ghim: `560.000` → `[num-05][num-hundred][num-60][num-thousand][num-dong]`.
Năm clip, bốn mối nối — con số `m8-16 §6` dự đoán cho `R-2`, nay là assertion.

Và ghim luôn phát hiện của `W-0229` ở tầng renderer: đổi sang giọng Nam thì **`Id` không đổi một
cái nào**, chỉ `Text` của `num-thousand` từ `nghìn` thành `ngàn`. Một kịch bản thu, ba giọng.

## 4. Một chỗ suýt thành bản sao thứ hai

`Join` đang là `private` trong speller. Renderer cũng cần nối clip thành chữ — chép sang là có
**hai bộ nối**, và hai bộ nối là hai cách một cuộc gọi khác với kịch bản đã duyệt.

Nên `Join` chuyển thành `SpeechNumberClip.Join`, dùng chung. Speller lẫn renderer gọi cùng một hàm.

> Cùng bài học `W-0221` và `W-0233`: **một luật viết một lần thì không tự mâu thuẫn với chính nó.**
> Lần này bắt được lúc đang gõ, không phải sáu tuần sau.

## 5. Guard của repo bắt lỗi của tôi

Suite đỏ một test — **không phải** test tôi vừa viết:

```text
FailGateTests.TheTraceabilityTableMatchesTheSuiteItClaimsToDescribe [FAIL]
```

Tôi thêm `UT-VOICE-CLIP-10` mà chưa sinh lại `docs/traceability-tests.md`. Đó chính là việc của
gate đó, và nó làm đúng. Sinh lại → `562`.

Đáng ghi vì lượt trước tôi gặp một lần **đỏ giả** (build cũ) và lần này là **đỏ thật** — hai thứ
trông giống nhau trong log, và phân biệt được chỉ bằng cách chạy lại chứ không bằng cách đoán.

## 6. Kiểm chứng

```text
golden 90.066 dòng             sha256 không đổi so với trước W-0233
dotnet test Ivr.sln            905/905 PASS, 0 failed, 0 skipped   (904 → 905)
traceability                   TEST_TRACEABILITY_CURRENT=562        (561 → 562)
gate-status.mjs                GATE_STATUS_PASS — 232 work items
contract-freeze-verifier.mjs   CONTRACT_FREEZE=PASS
docs-selftest.mjs              API_DOCS_SELFTEST_PASS
gitnexus impact renderer       MEDIUM · 6 direct · 1 module · 0 process
```

Không đổi một giá trị văn bản nào, không đổi chữ ký public nào đang có, không mở gate nào.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Còn lại

Hai đoạn động còn lại **thật sự** chờ quyết định:

- `{{items_spoken}}` — cần `3b` (món không có clip) và `4` (đơn vị)
- `{{delivery_area_short}}` — cần `4` (danh sách vùng)

Không có phần nào của hai cái đó dựng trước được: `3b` quyết **hình dạng** của đường tra cứu, không
chỉ giá trị trả về.
