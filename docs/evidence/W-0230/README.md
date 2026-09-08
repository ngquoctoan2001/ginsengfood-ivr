# W-0230 — Hint bị bỏ qua, và cái lỗ nó để lộ ra

Ngày: 2026-09-08 · Baseline: `main@bba37aa` · Trạng thái: **TESTS_PASS**.

Owner: *"hint bị bỏ qua, tên hàng có clip thì dùng clip"* — quyết định 3/4 của `m8-16 §9`.

## 1. Vì sao đây là quyết định giữ được đường bỏ TTS

Ba quyết định kia đổi **kích thước** ngân hàng ghi âm. Câu này đổi **có bỏ được TTS khỏi runtime hay
không**: chọn *"hint thắng ⇒ rơi về TTS"* thì mọi task mang `pronunciation_hints` vẫn gọi provider,
và lập luận đóng `INF-A` (mirror) cùng 16 CVE Security sụp theo.

Owner chọn vế giữ được đường đi. Hệ quả contract phải **ghi vào IR-06**, không để trôi: M3 vẫn được
gửi `pronunciation_hints`, và với món **đã có clip** thì hint **không có tác dụng** — im lặng không
có tác dụng, không phải lỗi. Không ghi ra thì M3 gửi hint rồi tự hỏi vì sao không nghe thấy.

## 2. Nhưng câu vừa chốt mở ngay câu kế

*"Tên hàng **có clip** thì dùng clip"* hàm ý có trường hợp **không có clip**. Đi kiểm contract thì
không có gì chặn một tên hàng lạ:

| Trường | Ràng buộc thật | Catalog? |
| --- | --- | --- |
| `public_name` | non-blank, **≤ 160 ký tự**, PII-safe — `SpeechItem.Create` | **không** |
| `unit_label` | optional, **≤ 40 ký tự** | **không** |
| `items[]` | `1..100` phần tử — `TaskIntakeEndpoint.cs:245` | — |

Cả `public_name` lẫn `unit_label` là **free text từ M3**. Hôm nay vô hại vì TTS đọc được mọi chuỗi.
Dưới mô hình ghi âm, **Sales thêm một món ngày mai là một cuộc gọi không đọc được** — rủi ro vận
hành thật, không phải giả định.

Bank C (đơn vị) và D (tên hàng) chỉ **đóng** được nếu có chỗ nào cưỡng chế. **Hôm nay không có
chỗ nào.**

## 3. Đề xuất, dựa trên cơ chế đã có

| | Cách | Hệ quả |
| --- | --- | --- |
| 1 | rơi về TTS cho món lạ | **TTS ở lại** — mâu thuẫn chính quyết định vừa ký |
| 2 | từ chối ở intake | M3 phải biết catalog ghi âm; đơn thật hỏng vì một món mới |
| **3** | **gộp thành *"N sản phẩm khác"*** | **tiền lệ đã có trong code** |
| 4 | đẩy sang admin review | `REVALIDATE_AND_HOLD_ADMIN_REVIEW` đã có trong ma trận |

Cách `3` không phải phát minh: `VietnameseOrderScriptRenderer` **đã** gộp phần vượt
`MaximumSpokenItems` thành *"và N sản phẩm khác"*, có `VietnameseNumberSpeller` đọc số N. Mở rộng
sang "món không có clip" là **cùng một cơ chế**, đã có test.

> **Đề xuất: `3` cộng một chặn đáy.** Món có clip đọc tên; món không có gộp vào *"và N sản phẩm
> khác"*. **Nhưng nếu không món nào có clip thì không gọi** — *"Quý khách có đơn hàng gồm một sản
> phẩm"* không xác nhận được gì, và gọi khách để đọc một câu vô nghĩa tệ hơn là không gọi. Trường
> hợp đó đi `4`.
>
> Không cần M3 biết catalog, không thêm đường hỏng mới, dùng lại cơ chế đã có test.

## 4. Và một việc không phải code

Ai báo cho IVR khi Sales thêm sản phẩm? Bank D chỉ đúng **tới lần thay đổi catalog kế tiếp**. Đó là
quy trình vận hành và phải có tên người, nếu không thì mô hình ghi âm tự hỏng dần theo thời gian mà
không ai thấy — cho tới khi một khách nghe *"đơn hàng gồm một sản phẩm"*.

## 5. Kiểm chứng

```text
gate-status.mjs                GATE_STATUS_PASS — 228 work items
contract-freeze-verifier.mjs   CONTRACT_FREEZE=PASS
docs-selftest.mjs              API_DOCS_SELFTEST_PASS
today-03 thân gói              git hash-object = 2f6c951f… (không đổi)
```

`dotnet test` không chạy lại: không file `.cs`/`.mjs`/`.py` nào đổi. **Không sửa code** — quyết định
đã ghi, phần thi hành chờ `3b` và hai câu còn lại. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| 2 | giữ hay thu lại 12 đoạn cố định | Owner + Legal |
| **3b** | **món chưa có clip xử lý ra sao** + ai báo khi catalog đổi | Owner + M3 + Vận hành |
| 4 | danh sách đơn vị và vùng giao | Vận hành |

`3b` là câu mới, sinh ra từ chính quyết định hôm nay. Nó **không** chặn đường bỏ TTS như `3` đã
chặn — cả bốn cách đều bỏ được TTS trừ cách `1` — nhưng nó quyết định khách nghe gì khi catalog
lệch, và đó là thứ xảy ra thường xuyên hơn mọi gate trong tài liệu này.
