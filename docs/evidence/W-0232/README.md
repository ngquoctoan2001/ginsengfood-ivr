# W-0232 — Điểm thiết kế chốt được trước khi hai câu kia trả lời

Ngày: 2026-09-08 · Baseline: `main@ede53bc` · Trạng thái: **TESTS_PASS**.

Còn hai câu (`3b`, `4`) và cả hai thuộc owner/vận hành. Nhưng `m8-16 §9` bước 5 — dựng cơ chế bank —
là việc của M8, và **hình dạng** của nó quyết được ngay. Quyết sai thì đắt.

## 1. Trước tiên: hai chỗ *không* mở lại

- **`2.1` TTL.** Ba tầng chồng nhau ra đúng một giá trị hợp lệ (`intake:411` đòi `>=`,
  `PersistenceInvariantValidator:120-121` đòi `<=`), nên task dựng theo `OD-V1-17` qua intake rồi
  ném ở persistence. **`W-0208` đã ghim có chủ đích** bằng `IT-INTAKE-DB-03`, và evidence của nó
  viết rõ *"test này cố ý sẽ đỏ khi TTL đổi"*. Sửa intake lúc này là mở lại một quyết định lượt
  khác đã cân nhắc, trên một mục worklist giao cho M3 + Security. **Không đụng.**
- **Bộ provenance VieNeu.** `W-0231` đã ghi: vẫn xanh, thành đồ thừa, nhưng là bộ máy bắt `2a4f45d`.
  Decommission là work item riêng.

## 2. Cám dỗ: render văn bản rồi cắt văn bản

Ranh giới clip **không nằm trong chữ**:

```text
     21  →  "hai mươi mốt"                        R-2: 1 clip  [21]
    121  →  "một trăm hai mươi mốt"               R-2: 3 clip  [1][trăm][21]
   1021  →  "một nghìn không trăm hai mươi mốt"   R-2: 5 clip  [1][nghìn][0][trăm][21]
```

Cùng ba chữ *"hai mươi mốt"*, ba ngữ cảnh. Cắt đúng cần biết nó **đến từ số nào** — tức viết ngược
lại speller: luật `mốt`/`tư`/`lăm`, phân biệt `mười`/`mươi`, và filler `không trăm` ở `1021`.

Quét `0..200.000` bằng chính speller: **0 va chạm**, cách đọc là song ánh. Nên về lý thuyết suy
ngược được — **vấn đề không phải có làm được không mà là cái giá**: một parser ngược phải khớp
speller mãi mãi.

> Đó đúng là lỗi `W-0221` vừa sửa trong chính phiên này: **một luật, hai bản, rồi chúng trôi khỏi
> nhau.** Biết trước thì không dựng thêm một cặp nữa.

## 3. Đảo chiều: clip là primitive, văn bản là hình chiếu

Renderer **đã có** `decimal` gốc; `Spell` đã duyệt theo nhóm ba chữ số với mảng `scales`. Cùng lượt
duyệt đó phát ra clip; **văn bản nối từ tên clip**.

```text
hôm nay   số ──Spell──> văn bản ──TTS──> audio
sai       số ──Spell──> văn bản ──parser ngược──> clip     ← hai luật, sẽ trôi
đúng      số ──walk──> clip ──join──> văn bản              ← một luật
```

Được ba thứ:

1. **không thể trôi** — văn bản dẫn xuất từ clip;
2. `TemplateHash` và mọi test đang so văn bản **chạy nguyên**, vì giá trị văn bản không đổi;
3. số mối nối thành **con số kiểm được bằng test**, thay vì cảm nhận sau khi nghe — và mối nối
   chính là rủi ro lớn nhất của cả hướng ghi âm (`m8-16 §6`).

Điểm 3 đáng nói: `R-2` được chọn **vì vị trí mối nối**, và cho tới giờ cách duy nhất để biết là
nghe. Clip list biến nó thành assertion.

## 4. `3b` cắm vào đúng một chỗ

Hàm tra `tên hàng → clip`. `null` thì áp luật `3b`. **Không** chạm phần số — bank A luôn đủ, vì
`0..99` phủ mọi số lượng và mọi nhóm ba chữ số.

⇒ `3b` và `4` chặn **thi hành**, không chặn **thiết kế**.

## 5. Kiểm chứng

```text
0..200000 qua VietnameseNumberSpeller   0 va chạm — song ánh
21 / 121 / 1021                        1 / 3 / 5 clip cho cùng chuỗi "hai mươi mốt"
gate-status.mjs                        GATE_STATUS_PASS — 230 work items
contract-freeze-verifier.mjs           CONTRACT_FREEZE=PASS
docs-selftest.mjs                      API_DOCS_SELFTEST_PASS
today-03 thân gói                      2f6c951f… không đổi
```

Project tạm ngoài repo, `git status` sạch. **Không sửa code** — lượt này chốt hình dạng, chưa dựng.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại

`3b` và `4`. Xong hai câu đó thì bước 5 dựng được, và §10 nói rõ dựng theo hướng nào — nên lượt dựng
sẽ không phải quyết lại kiến trúc giữa chừng.
