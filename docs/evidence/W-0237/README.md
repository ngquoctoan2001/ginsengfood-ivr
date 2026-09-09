# W-0237 — Một đơn vị, và cái bẫy hoa thường suýt nuốt một đơn bình thường

Ngày: 2026-09-09 · Baseline: `main@943b085` · Trạng thái: **TESTS_PASS**.

Owner chốt `4b`: **đơn vị là `hộp`**.

## 1. Repo đồng ý với câu trả lời

Đếm mọi chuỗi đơn vị trong repo:

```text
"hộp"  ×33      "gói" ×7    "chai" ×5    "kg" ×3    "túi" ×2    "thùng" ×1
"Hộp"  ×4
```

Năm cái không phải `hộp` **chỉ nằm trong test fixture** — không spec, không source, không tài liệu
nào khai chúng. Nên câu trả lời nhất quán với những gì repo biết.

Bank C: ước lượng `~10` → **`1`**. Tổng mỗi miền `≈ 159 + E` → **`≈ 150 + E`**.

## 2. Nhưng `"Hộp"` viết hoa có thật, và nó suýt lọt

`RecordedSpeechCatalog` tôi viết ở `W-0235` so **ordinal, phân biệt hoa thường**, kèm lý do:

> *"Not case-insensitive and not accent-folded: two product names that differ only by case or
> diacritic are two different readings…"*

Lý do đó **đúng cho tên hàng**. Với đơn vị thì không: `Hộp` và `hộp` là **một từ tiếng Việt**, hoa
thường chỉ là cách gõ trường dữ liệu. Và `"Hộp"` viết hoa **đang có trong repo**, ở
`CallResultAndMapperTests` và `DomainPolicyAndPrivacyTests`.

Nếu để nguyên: một đơn `2 Hộp Sâm Ngọc Linh` **trượt** tra cứu → `3b` gộp thành *"một sản phẩm
khác"* → và nếu đó là món duy nhất thì **chặn đáy bắn, không gọi khách**. Vì một chữ H viết hoa.

Đã tách hai luật:

| | So sánh | Vì sao |
| --- | --- | --- |
| tên hàng | **ordinal**, phân biệt hoa thường | danh từ riêng — hoa thường có thể mang nghĩa |
| **đơn vị** | **`OrdinalIgnoreCase`** | danh từ chung — hoa thường là cách gõ |

**Không bên nào bỏ dấu**, và đó là chỗ dễ làm quá tay: `hộp`, `hợp`, `họp` là **ba từ khác nhau**;
`hop` không phải từ nào trong ba. Bỏ dấu cho "tiện" là đọc sai từ cho khách.

`UT-VOICE-4B-07` ghim cả bốn dạng (`hộp` · `Hộp` · `HỘP` · `  hộp  `) đều đọc được, và `hop` thì
gộp.

## 3. Test tôi viết sai lần thứ ba, code đúng lần thứ ba

Bản đầu của `UT-VOICE-4B-07` khẳng định `hop` cho ra *"một sản phẩm khác"*. Đỏ — vì với **một món
duy nhất** không đọc được thì **chặn đáy** bắn (`TryCompose = false`), không phải gộp. Gộp cần ít
nhất một món đọc được.

Sửa test: thêm một dòng đọc được để thấy được cụm gộp, **và** khẳng định riêng ca một-món-duy-nhất
trả `false`.

> Ba lượt liền test bắt ra chỗ tôi hiểu sai chính đặc tả mình vừa viết. Đó là lý do mỗi quyết định
> được ghi thành assertion chứ không thành câu văn.

## 4. Rủi ro tồn dư, nói rõ

`unit_label` trong OAS là `{ type: string, maxLength: 40 }` — **không enum**. Một đơn vị khác `hộp`
vẫn tới được, và `3b` sẽ **gộp** nó chứ không đọc sai. Đúng thiết kế — nhưng **im lặng**: một đơn
bán theo `chai` mất đơn vị mà không gì báo đỏ.

> Nếu catalog có sản phẩm bán theo chai, bank C phải mở rộng **trước** khi sản phẩm đó lên sàn,
> không phải sau khi khách nghe thiếu.

## 5. Kiểm chứng

```text
dotnet test Ivr.sln            912/912 PASS, 0 failed, 0 skipped   (911 → 912)
traceability                   TEST_TRACEABILITY_WRITTEN=569        (568 → 569)
gate-status.mjs                GATE_STATUS_PASS — 235 work items
contract-freeze-verifier.mjs   CONTRACT_FREEZE=PASS
docs-selftest.mjs              API_DOCS_SELFTEST_PASS
```

Không nối vào renderer — bank D (tên hàng) vẫn rỗng, nên bật lúc này vẫn là mọi món đều gộp.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| **4a** | vùng giao: cả chuỗi hay tỉnh (đề xuất: tỉnh) | Owner + Product |
| **D** | **danh sách tên hàng** — *"vài chục món"* nhưng chưa ai liệt kê | Owner + Vận hành |
| 4c | Sales master data còn phát dạng chỉ-có-quận không | Bên nắm master data |
| 3c | ai báo cho IVR khi Sales thêm sản phẩm | Vận hành |

Bank D là thứ chưa từng được nêu thành một mục riêng — `m8-16` xếp nó trong bảng §4 như dữ liệu, và
nó đúng là dữ liệu, nhưng **chưa ai đưa**. Không có nó thì composer không đọc nổi một dòng nào.
