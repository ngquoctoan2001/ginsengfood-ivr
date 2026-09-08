# W-0233 — Clip thành primitive, và văn bản không đổi một byte

Ngày: 2026-09-08 · Baseline: `main@14484e6` · Trạng thái: **TESTS_PASS**.

`W-0232` chốt hình dạng. Lượt này dựng phần **không phụ thuộc** `3b`/`4`: đường số.

## 1. Vì sao dựng được ngay

`3b` cắm vào hàm tra `tên hàng → clip`; `4` là danh sách đơn vị và vùng giao. **Cả hai không chạm
phần số** — bank A luôn đủ vì `0..99` phủ mọi số lượng và mọi nhóm ba chữ số. Nên đường số dựng
được trước, và nó là phần khó nhất.

`gitnexus impact VietnameseNumberSpeller` → **LOW**, 0 execution flow, 0 module; caller thật: 4 chỗ
trong renderer, 20 trong test.

## 2. Đảo chiều

```text
trước   số ──Spell──> văn bản
sau     số ──SpellClips──> clip[] ──Join──> văn bản
```

`SpellGroup` bị **xoá**, thay bằng `AppendGroupClips`. `Spell` và `SpellQuantity` nay là **một
dòng** — hình chiếu của clip list. Không còn hai đường sinh ra cùng một chuỗi.

```csharp
public static string Spell(decimal amount, VietnameseNumberStyle style) =>
    Join(SpellClips(amount, style));
```

## 3. Chứng minh văn bản không đổi — bằng byte, không bằng lập luận

Trước khi sửa, chụp golden bằng một project tạm **ngoài repo** tham chiếu `Ivr.Domain`:

```text
3 style × (0..30.000 + 13 giá trị lớn + 8 quantity)  =  90.066 dòng
sha256 trước   6f607d4e58f6e46cc6d06755…
sha256 sau     6f607d4e58f6e46cc6d06755…
diff           0 dòng lệch
```

Cùng hash. Nghĩa là `TemplateHash`, `speech-segments.json`, mọi test so văn bản và chữ ký kịch bản
của owner **không bị chạm** — đúng điều `W-0232 §10` hứa.

## 4. Ghim vĩnh viễn — 4 test mới

Golden là bằng chứng một lần; test là bằng chứng mỗi lượt CI.

| TestId | Khẳng định |
| --- | --- |
| `UT-VOICE-CLIP-06` | `0..99` cho **đúng một clip**, và `Id` **giống hệt ở ba miền** — `Assert.Single` chính là assertion của `R-2` |
| `UT-VOICE-CLIP-07` | bank speller là **đúng 106** id mỗi miền |
| `UT-VOICE-CLIP-08` | `Spell` **bằng** clip text nối lại, `0..5.000` × 3 miền |
| `UT-VOICE-CLIP-09` | `560.000` → `[num-05][num-hundred][num-60][num-thousand]`; `1.021` → 5 clip |

`UT-VOICE-CLIP-06` là chỗ chứng minh cả hai phát hiện của `W-0229` cùng lúc: một cặp chục-đơn vị là
một clip (`R-2`), và kịch bản `0..99` dùng chung ba miền.

`UT-VOICE-CLIP-09` là thứ `W-0232 §10` gọi là *"số mối nối thành assertion thay vì cảm nhận"*.
`1.021` được chọn có chủ đích: nó chứa chuỗi `"hai mươi mốt"` nhưng ranh giới clip **không** ở đó —
đúng case một bộ tách theo chữ sẽ cắt sai.

## 5. Một con số tôi ghi sai, test bắt được

`UT-VOICE-CLIP-07` viết `107` đầu tiên và **đỏ**: thật là **`106`**.

`m8-16 §6` ghi *"Bank A theo `R-2` = 107 clip mỗi miền"* và liệt kê `đồng` trong đó. Nhưng `đồng`
do **`VietnameseOrderScriptRenderer:169`** ghép, không phải speller — đọc một con số không bao gồm
đơn vị tiền tệ, và `2,5 ký` chứng minh bằng cách không cần clip nào.

Bank vẫn là `107`. Ranh giới là `106 + 1`, và **ai làm đường clip cho renderer thì sở hữu clip đó**.
Đã ghi vào cả test lẫn `m8-16` để hai bên không cùng tưởng bên kia phát ra nó.

> Đây đúng là giá trị của việc viết test trước khi tin vào tài liệu của chính mình.

## 6. Kiểm chứng

```text
golden 90.066 dòng            sha256 trước = sau, diff 0
dotnet test Ivr.sln           904/904 PASS, 0 failed, 0 skipped   (900 → 904)
traceability                  TEST_TRACEABILITY_CURRENT=561       (557 → 561)
gate-status.mjs               GATE_STATUS_PASS — 231 work items
contract-freeze-verifier.mjs  CONTRACT_FREEZE=PASS
docs-selftest.mjs             API_DOCS_SELFTEST_PASS
gitnexus detect_changes       low · 0 affected processes
```

Project tạm ngoài repo, `git status` sạch. Không đổi một giá trị văn bản nào, không mở gate nào.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

> **Một lần đỏ giả, ghi lại để lượt sau không mất thời gian.** Lượt chạy `Ivr.sln` đầu tiên báo
> `601/602` — nhưng chạy lại riêng `Ivr.UnitTests` thì `602/602`. Nguyên nhân: lượt solution khởi
> động **song song** với chính lần sửa `107`→`106`, nên nó biên dịch bản test cũ. Không phải defect;
> nhưng một con số `Failed: 1` trong log là thứ dễ bị chép vào evidence rồi thành sai. Đã chạy lại
> sạch để lấy `904`.

## 7. Còn lại

Đường số xong. Chưa dựng: **đường tên hàng / đơn vị / vùng giao** — chờ `3b` và `4`, và chúng cắm
vào đúng một chỗ như `W-0232 §10` đã chỉ.

Renderer chưa có đường clip: `items_spoken` và `total_amount_display` vẫn ghép **văn bản**. Nối
renderer vào `SpellClips` là lượt kế, và nó **cần** `3b` để biết làm gì với món không có clip.
