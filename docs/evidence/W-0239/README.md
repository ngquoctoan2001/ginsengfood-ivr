# W-0239 — `19` hay `20`, và vì sao IVR cần danh sách khi M3 đã có

Ngày: 2026-09-09 · Baseline: `main@dae621c` · Trạng thái: **TESTS_PASS**.

Owner hỏi hai câu. Cả hai đều đúng chỗ cần hỏi.

## 1. `19` hay `20` — tài liệu nói `20`, ở khắp nơi

```text
"20 SKU"   233 lần   — gồm MASTER-01-SOURCE-OF-TRUTH, MASTER-00-INDEX-REGISTRY,
                        MASTER-08-CROSS-SYSTEM-DECISION-LOG, PACK-02, DOC-READING-ORDER
"19 SKU"     0 lần
```

Hai con số khác cũng có nhưng không phải danh mục nền: `40-50 SKU` là **dự trù mở rộng**
(`TECH-02:1387`), `16 SKU` nằm trong một tài liệu phase-3.1 khác ngữ cảnh.

Nếu thực tế là **19**, thì lệch nằm ở **tài liệu gốc**, không ở IVR — và sửa phải sửa tại
`MASTER-01-SOURCE-OF-TRUTH` cùng 232 chỗ dẫn theo nó, chứ IVR im lặng dùng 19 thì tạo thêm một
nguồn sự thật thứ hai.

**Với IVR thì con số gần như không đổi gì**: bank D cần **danh sách tên**, `19` hay `20` chỉ đổi số
dòng phải thu. Nhưng nó là một tín hiệu: danh mục **chưa chốt**, và đó lại là lý do §13 đưa ra —
điền registry trước, thu sau.

## 2. "M3 có rồi thì IVR cần dữ liệu đó làm gì?"

Vì mô hình vừa đổi từ **đọc máy** sang **phát bản thu**.

| | M3 gửi tên lúc gọi | IVR cần danh sách trước? |
| --- | --- | --- |
| **TTS** (hôm nay) | có | **không** — máy đọc chuỗi nào cũng được |
| **Ghi âm** (`OD-V1-19`) | vẫn có | **có** — phải có người **đã đọc** chuỗi đó vào micro |

Lúc chạy, IVR **vẫn** lấy `public_name` từ M3, y như bây giờ. Nó không lưu danh sách như một sự
thật. Cái nó cần là **file đã tồn tại** cho đúng chuỗi ấy — một cuộc gọi ghi âm chỉ phát được thứ
đã thu.

⇒ Danh sách 20 tên **không phải dữ liệu IVR sở hữu. Nó là kịch bản buổi thu.**

### Thứ IVR giữ lại sau buổi thu

Một map `tên → file clip`, trả lời *"tôi có bản thu của chuỗi này không"* — **không** trả lời
*"sản phẩm nào tồn tại"*. Đó là **kho bản thu**, không phải bản sao Product Master.

IVR không cần `sku_id`, `product_group`, công thức, BOM hay bất kỳ trường nào khác của `PACK-02
§5.2`. Chỉ `public_product_name`, và phải **đúng chuỗi M3 sẽ gửi**.

### Nên thứ cần từ Product Master chỉ có hai

1. **Một lần** — danh sách tên để thu.
2. **Về sau** — tín hiệu khi có tên mới, thu trước khi sản phẩm đó `Sellable`.

## 3. Và đây là chỗ tôi trình bày `4d` nặng hơn thực tế

`W-0238` gọi `4d` là *"mắt xích chặn cả mạch"*. Đúng với **độ phủ đầy đủ**, nhưng **không đúng với
việc bắt đầu** — vì `3b` đã lo phần thiếu:

| Bank D có | Hệ quả |
| --- | --- |
| **rỗng** | mọi cuộc gọi rơi chặn đáy — không gọi được |
| **vài tên bán chạy** | chạy được; đơn chỉ có hàng hiếm thì gộp *"N sản phẩm khác"* |
| **đủ 20** | không đơn nào bị gộp vì thiếu clip |

`4d` là **độ phủ**, không phải cổng nhị phân. Thu nhóm bán chạy trước là bắt đầu được ngay.

Điều kiện duy nhất không nhân nhượng: mỗi tên phải thu **đúng chuỗi Product Master chốt**. Thu theo
chuỗi tạm thì §13 vẫn áp — thu lại.

## 4. Kiểm chứng

```text
git grep -c "20 SKU"    233 lần trên 6+ tài liệu master/pack
git grep -c "19 SKU"    0
dotnet test Ivr.sln     912/912 (không đổi — lượt này không sửa code)
gate-status.mjs         GATE_STATUS_PASS — 237 work items
docs-selftest.mjs       API_DOCS_SELFTEST_PASS
```

Không sửa code. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 5. Còn lại

Không đổi so với `W-0238`, chỉ đổi **mức khẩn** của `4d`: từ *chặn* thành *độ phủ*.

`4a` (vùng giao: cả chuỗi hay tỉnh) vẫn là câu owner quyết một mình được, và nó mở nốt đoạn động
cuối cùng.
