# W-0238 — Bank D không nằm ở IVR, và ở nơi nó nằm thì 20 ô tên còn trống

Ngày: 2026-09-09 · Baseline: `main@39e9d25` · Trạng thái: **TESTS_PASS**.

Owner chỉ `4d`. Danh sách tên hàng là thứ tôi **không được bịa**, nên đi tìm xem hệ thống đã biết gì
— và nó biết nhiều hơn tôi tưởng, theo hướng xấu.

## 1. Chỗ của bank D đã có, và đang trống

`docs/documents/2. pack/02-PACK-02-PRODUCT-MASTER-SKU-RECIPE-ACTIVATION.md`:

| Mục | Nội dung |
| --- | --- |
| §4.1 | *"**20 SKU canonical** là danh mục sản phẩm nền của Ginsengfood"* |
| §5.2 | mỗi SKU **bắt buộc** có `public_product_name` và `internal_product_name` |
| §32.2 | registry 20 dòng — **cả 20 ô tên là `{{placeholder}}`**, status `waiting_CONFIG / READY` |
| §33 | *"Khi triển khai dữ liệu thật, không được để placeholder đi vào production runtime."* |

```text
01 | SKU-01 | {{public_product_name_01}} | … | waiting_CONFIG / READY
…
20 | SKU-20 | {{public_product_name_20}} | … | waiting_CONFIG / READY
```

Đếm được **đúng 20** placeholder phân biệt. Không dòng nào đã điền.

Và đây **là** trường IVR dùng: IR-06 dòng `465` — ví dụ duy nhất trong toàn bộ integration spec —
ghi `{ "public_name": "Nước hồng sâm", "quantity": 2, "unit_label": "hộp" }`, khớp luôn đơn vị
`hộp` owner chốt hôm qua.

## 2. Nên `4d` không phải việc gõ danh sách

Nó là: **điền `PACK-02 §32.2`**. Hai mươi tên, ở một registry đã có cấu trúc, đã có owner, đã có
lifecycle — chỉ chưa có nội dung.

Con số cũng chính xác hơn: owner nói *"vài chục món"*, Product Master nói **20**.

## 3. Hệ quả về thứ tự — và làm ngược thì phải thu lại

Tra tên hàng là **so chuỗi chính xác**. Đó là quyết định có chủ đích ở `W-0237 §12`: tên hàng là
danh từ riêng, hoa thường có thể mang nghĩa, nên không bỏ qua hoa thường và không bỏ dấu.

M3 lấy `public_name` **từ Product Master**. Nên nếu thu âm theo một danh sách tạm rồi Product Master
điền chuỗi khác — `"Nước hồng sâm Ginsengfood 500ml"` thay vì `"Nước hồng sâm"` — thì **mọi clip
đều trượt**, mọi món gộp, mọi cuộc gọi rơi vào chặn đáy. Cả buổi thu bỏ.

> **Điền registry trước, thu âm sau.** Bắt buộc, không phải khuyến nghị. Và nó không tốn thêm gì:
> `PACK-02` vốn phải điền để SKU đi tiếp vòng đời của nó.

## 4. `3c` có câu trả lời từ cấu trúc sẵn có

Câu *"ai báo cho IVR khi Sales thêm sản phẩm"* (`m8-16 §7.2`) không cần một quy trình mới.

Sản phẩm mới **sinh ra ở Product Master**, nơi đã có `sku_lifecycle_status` và `activation_status`.
Và `PACK-02 §5.4` đã tách sẵn hai trạng thái: *"SKU Activated không đồng nghĩa Sellable"*.

**Chỗ để cắm điều kiện ghi âm nằm đúng giữa hai cái đó**: SKU `Activated` rồi, nhưng chỉ `Sellable`
khi bank D đã có clip cho `public_product_name` của nó. Không có clip thì IVR không đọc được, và
`3b` sẽ gộp — nên điều kiện này bảo vệ đúng thứ nó cần bảo vệ.

Đề xuất, không tự áp: thêm ghi âm vào điều kiện `Activated → Sellable` thay vì dựng một kênh báo
riêng cho IVR.

## 5. Vì sao lượt này không viết code

`RecordedSpeechCatalog` nhận từ điển và đã có test cho ca rỗng — `TryCompose` trả `false`, chặn đáy
bắn. Không thiếu cơ chế nào. Thiếu **hai mươi chuỗi**, và chúng phải là chuỗi thật chứ không phải
chuỗi tạm.

## 6. Kiểm chứng

```text
grep '{{public_product_name_\d+}}' PACK-02      20 placeholder phân biệt, 0 dòng đã điền
IR-06:465                                        public_name "Nước hồng sâm", unit_label "hộp"
dotnet test Ivr.sln                              912/912 (không đổi — lượt này không sửa code)
gate-status.mjs                                  GATE_STATUS_PASS — 236 work items
docs-selftest.mjs                                API_DOCS_SELFTEST_PASS
```

Không sửa code, không mở gate nào. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| **4d** | điền **20 `public_product_name`** ở `PACK-02 §32.2` | Owner + Product Master |
| **4a** | vùng giao: cả chuỗi hay tỉnh (đề xuất: tỉnh) | Owner + Product |
| 4c | Sales còn phát dạng chỉ-có-quận không | Bên nắm master data |
| 3c | → đề xuất: gắn vào `Activated → Sellable`, không dựng kênh mới | Product Master + Vận hành |

`4d` là mắt xích chặn cả mạch, và nó **không thuộc IVR** — đó là điều đáng nói nhất lượt này.
