# W-0240 — `4a` không phải chọn ngang nhau: một vế chờ chữ ký chưa có

Ngày: 2026-09-09 · Baseline: `main@e2e0ecd` · Trạng thái: **TESTS_PASS**.

Owner chỉ `4a`. Như `4d`, đi kiểm xem hệ thống đã quyết chưa trước khi coi là câu mở — và nó đã
quyết một nửa, theo cách đổi hẳn cách so sánh hai lựa chọn.

## 1. Việc đọc vùng giao **đã được ký**, nhưng ký một nửa

Spec V0.3 `§12.2` xếp `delivery_area_short` (và cả `items[]`) là **"ĐANG TRANH CHẤP — `OD-V1-15`"**,
kèm câu:

> *"Mở rộng whitelist tự nó là một quyết định privacy, cần Privacy/Legal ký."*

`OD-V1-15` **đã ký `2026-09-05`**: whitelist **bộ rộng**, gồm *"tên món + số lượng + vùng giao rút
gọn"*.

Nhưng cột owner của `OD-V1-15` trong spec là **Product + Privacy/Legal**, còn chữ ký `05/09` là của
**IVR owner**. Tức đây **đúng dạng dòng** worklist `3.2` bảo phải ghi
`M8_POSITION_SIGNED / <owner> NOT_RECEIVED` — không phải `CLOSED`.

## 2. Nên hai lựa chọn của `4a` không ngang nhau

| | Đọc cả chuỗi (phường + tỉnh) | **Đọc tỉnh** |
| --- | --- | --- |
| so với `OD-V1-15` | ở **rìa ngoài** phần vừa mở rộng | **hẹp hơn** — nằm gọn bên trong |
| chữ ký Privacy/Legal | spec nói **cần**, **chưa có** | không cần thêm gì |
| clip mỗi miền | hàng nghìn phường | **~34** tỉnh |
| biến thể chính tả | phải tự chuẩn hoá | `ToMatchKey` **đã giải** |

**Đọc tỉnh đọc ít dữ liệu cá nhân hơn mức đã ký.** Một quyết định thu hẹp phạm vi không cần phê
duyệt privacy mới — chỉ mở rộng mới cần. Còn đọc cả chuỗi thì nằm ở đúng chỗ spec nói cần
Privacy/Legal, và nó vào **cùng hàng đợi** với `L5`–`L7` của phiếu Legal chưa gửi.

⇒ Đọc tỉnh là **lựa chọn duy nhất không chờ một chữ ký chưa có**. Đó là lập luận mạnh hơn cả lập
luận `~34` clip so với hàng nghìn.

## 3. Một chỗ spec cũ 4 ngày, không tự sửa

`docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md:293-294` vẫn ghi `items[]` và
`delivery_area_short` là **"ĐANG TRANH CHẤP"**, trong khi `OD-V1-15` đã ký `05/09`.

Cùng lớp lỗi `3.5` (`:472` ghi `result_type` có 11 giá trị trong khi ràng buộc DB cho 6): **spec mô
tả một hệ thống không còn đúng**. Ai dựng theo spec sẽ tưởng hai trường này còn tranh chấp và không
gửi.

**Không tự sửa** — thẩm quyền sửa spec thuộc chief auditor/Owner, y như `3.5` đã ghi và giữ.

## 4. Vì sao lượt này không viết code

Giống `W-0236`: cơ chế đọc vùng giao chỉ dựng được sau khi biết **đọc gì**. Khác là bây giờ có thêm
một lý do để chọn — và nếu owner chốt "đọc tỉnh" thì phần code rất nhỏ: `DeliveryRegionResolver`
đã parse chuỗi và đã có bảng `71` key, chỉ cần một hàm trả **tên tỉnh khớp** thay vì chỉ trả miền.

## 5. Kiểm chứng

```text
spec V0.3 §12.2:293-294        "ĐANG TRANH CHẤP — OD-V1-15"      (cũ 4 ngày)
spec V0.3 §16 dòng 673         OD-V1-15 owner = Product + Privacy/Legal
od-v1-signoff-2026-09-05:30    OD-V1-15 = bộ rộng, ký bởi IVR owner
dotnet test Ivr.sln            912/912 (không đổi — lượt này không sửa code)
gate-status.mjs                GATE_STATUS_PASS — 238 work items
docs-selftest.mjs              API_DOCS_SELFTEST_PASS
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| **4a** | chốt **đọc tỉnh** (đề xuất) → M8 dựng ngay, không chờ ai | Owner |
| 4d | vài tên đầu tiên để thu — **độ phủ**, không phải cổng | Owner + Product Master |
| 4c | Sales còn phát dạng chỉ-có-quận không | Bên nắm master data |
| 3c | gắn vào `Activated → Sellable` | Product Master + Vận hành |
| — | spec `:293-294` cần cập nhật sau chữ ký `OD-V1-15` | Chief auditor |
