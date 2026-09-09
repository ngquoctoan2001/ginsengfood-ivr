# W-0243 — Guard riêng cho `public_name`, và ranh giới của nó

Ngày: 2026-09-09 · Baseline: `main@7d798a6` · Trạng thái: **TESTS_PASS**.

Owner chốt phương án `2` của `W-0242 §3`: guard riêng cho `public_name`, bỏ bốn từ nhập nhằng, giữ
`số nhà`/`ngõ`/`hẻm`/`ngách`.

## 1. Nới một PII guard thì phải chứng minh nới đúng chỗ

`gitnexus impact PiiGuard` → **LOW**, 0 direct, 0 process. Nhưng `EnsureSafeBounded` dùng chung
**10 field** — `resultReason`, `orderState`, `customerDisplayName`, `orderCodeShort`,
`pronunciationHints`, `delivery_area_short`… — nên sửa nó là nới hết. **Không đụng nó.**

Chỉ `publicName` và `unitLabel` đổi đường.

## 2. Tách hằng số trước, chứng minh không đổi hành vi

Nhánh địa chỉ tách làm hai, theo đúng nghĩa của từ:

| Nhóm | Từ | Vì sao |
| --- | --- | --- |
| **chỉ có nghĩa địa chỉ** | `số nhà` `ngõ` `hẻm` `ngách` | không xuất hiện ở đâu khác |
| **nhập nhằng** | `đường` `thôn` `ấp` `tổ` | `đường` là **đường ăn** trước khi là đường phố; `tổ` là **tổ yến** |

`RestrictedValuePattern` ghép lại từ **cùng bốn tập** đó. Golden `61` chuỗi qua `IsSafeText` —
địa chỉ thật, số điện thoại, dial token, tên hàng, vùng giao, tên người, chuỗi biên — **giống hệt
trước và sau**. Tách hằng số không đổi một câu trả lời nào.

## 3. Guard mới, và cái giá nói thẳng

`RestrictedProductTextPattern` = phone + dial-token + **chỉ** nhóm địa chỉ thuần.

```text
PHẢI QUA (tên hàng thật)          PHẢI VẪN CHẶN (địa chỉ/điện thoại)
  ok   Tổ yến                       chặn   số nhà 12
  ok   Tổ yến chưng đường phèn      chặn   Ngõ 5 Kim Mã
  ok   Đường phèn                   chặn   Hẻm 3 Lê Lợi
  ok   Đường thốt nốt               chặn   Ngách 12/4
  ok   Chè đường phèn               chặn   0912345678 · +84912345678
  ok   Mật ong đường mía            chặn   so nha 12 · ngo 5 Kim Ma
  ok   Ấp trứng · Thôn quê          chặn   dial_token: abcdefgh12345
```

> **Cái giá, ghi ra chứ không giấu:** một tên hàng viết `"đường Nguyễn Huệ"` **nay qua được**
> guard này, trong khi `IsSafeText` vẫn từ chối. Đổi lại, `tổ yến` đặt hàng được. Đó là thứ owner
> chấp nhận, và `UT-PII-PRODUCT-02` ghim rằng những dạng **thật sự mang địa chỉ giao được** —
> `số nhà`, `ngõ`, `hẻm`, `ngách`, số điện thoại, dial token — vẫn bị chặn.

## 4. Ranh giới là phần đáng ghim nhất

`UT-PII-PRODUCT-03`:

```csharp
SpeechItem.Create("Đường phèn", 1m, "hộp");                    // qua
Assert.Throws(() => ShortDeliveryArea.Create("Đường phèn"));   // vẫn chặn
```

**Cùng một chuỗi, hai câu trả lời, có chủ đích.** Vùng giao **là** field địa chỉ nên giữ
`IsSafeText`. Nếu test này có ngày xanh cả hai vế thì guard hẹp đã rò ra khỏi field nó được viết
cho — và đó là thứ duy nhất biến lượt này thành một lỗ hổng.

## 5. Một chỗ tránh nhân đôi

`EnsureSafeBounded` và `EnsureSafeBoundedProductText` dùng **chung một thân** (`EnsureBounded`),
khác nhau đúng một tham số guard. Chép thân ra hai bản thì giới hạn độ dài hoặc chuẩn hoá Unicode
có thể trôi giữa field địa chỉ và field sản phẩm — mà **trôi chuẩn hoá thì vô hình**: hai chuỗi nhìn
y hệt nhau thôi khớp cùng một clip ghi âm, và không ai thấy cho tới lúc nghe.

`unitLabel` dùng cùng guard với tên nó bổ nghĩa: `hộp`/`gói` không va với gì hôm nay, nhưng một đơn
vị bị chặn trong khi sản phẩm được nhận sẽ làm hỏng đơn vì một lý do không đọc được từ thông báo.

## 6. Kiểm chứng

```text
gitnexus impact PiiGuard        LOW · 0 direct · 0 process
golden IsSafeText 61 chuỗi      trước = sau, diff 0        (tách hằng số vô hại)
10 tên hàng                     qua hết
9 dạng địa chỉ/điện thoại       chặn hết
UT-PII-PRODUCT-03               ranh giới: cùng chuỗi, product qua / delivery-area chặn
dotnet test Ivr.sln             940/940 PASS, 0 failed, 0 skipped   (922 → 940)
                                3 method = 18 case: hai [Theory] mang 8 và 9 InlineData
traceability                    TEST_TRACEABILITY_CURRENT=576
gate-status.mjs                 GATE_STATUS_PASS — 241 work items
```

`IsSafeText` **không đổi một dòng hành vi**; mọi field ngoài `publicName`/`unitLabel` giữ nguyên.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. ⚠️ `detect_changes` báo `CRITICAL` — và vì sao nó không thành sự thật

Phải nói trước: lượt này `gitnexus detect_changes` trả **`risk_level: critical`, 32 execution
flow**. `CLAUDE.md` bắt báo owner khi gặp HIGH/CRITICAL, nên đây là chỗ báo.

Đọc kỹ thì `32` flow đó **đều đi qua `IsSafeText` / `EnsureSafeText` / `EnsureSafeField`** — tool
đánh dấu chúng `touched` vì tôi sửa **hằng số** mà `RestrictedValuePattern` ghép từ đó. Đó là đọc
**cấu trúc**, đúng theo nghĩa của tool.

Hành vi thì không đổi, và có **hai** bằng chứng độc lập:

**1. Đại số, không phải mẫu thử.** Việc tách là một **phân hoạch đúng nghĩa**:

```text
diacritic   gốc 8 · mới 4+4=8 · hợp KHỚP · giao rỗng
ascii       gốc 7 · mới 4+3=7 · hợp KHỚP · giao rỗng
```

Tiền tố `(?<![\p{L}\p{N}])` và hậu tố `\s+` / `[A-Za-z0-9]` sao chép y nguyên sang cả hai nửa, nên
`X|(A|B)` ≡ `X|A|B`. Pattern hợp thành **là cùng một ngôn ngữ**, không phải một pattern gần giống.

**2. Golden 61 chuỗi** — địa chỉ thật, điện thoại, dial token, tên hàng, vùng giao, tên người, chuỗi
biên — `diff` **rỗng** trước và sau.

Thay đổi hành vi **duy nhất** nằm ở `SpeechItem.Create`, xuất hiện đúng ở `proc_52` và `proc_73`
(`DispatchAsync → SpeechItem`). Ba mươi flow còn lại đi qua một hàm không đổi.

> Ghi ra đây thay vì bỏ qua, vì lần sau tool lại báo `CRITICAL` cho cùng lý do — và người đọc cần
> biết đâu là ngưỡng để bác: **một phân hoạch chứng minh được, cộng một golden**, chứ không phải
> *"tôi thấy nó ổn"*.

## 8. Còn lại

`4d` nay chỉ còn cần **danh sách tên**. Bốn từ kia không còn chặn nữa, nên khi có danh sách tôi vẫn
chạy qua `SpeechItem.Create` — nhưng để bắt phần **còn lại** của guard, không phải để bắt `tổ yến`.
