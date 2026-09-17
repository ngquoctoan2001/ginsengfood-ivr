# W-0311 — Phương án `B`: Module 3 gửi thẳng số điện thoại

**Ngày:** `2026-09-17` · **Baseline:** `main@ca8d13b` · **Nguồn:** quyết định owner `2026-09-17`

`REAL_CUSTOMER_CALL_ALLOWED=NO`

> ### ⚠️ Commit `ca8d13b` ghi nhầm `W-0310` ở tiêu đề
>
> `W-0310` là của phiên `ivr-98` (`5f00838`, phiếu M3 gộp), commit **trước** commit của tôi. Khi
> tôi cấp ID thì tracker đã sang `W-0311`. **Không sửa lại lịch sử commit** — ghi ở đây và ở
> tracker để sổ là nguồn đúng và chỗ lệch nhìn thấy được, thay vì được hoà giải lặng lẽ.

---

## 0. Quyết định và cái giá của nó

Owner chọn ngày `17/09`, sau khi được trình bảng so sánh: **M3 gửi số điện thoại**, thay vì phát
hành một `dial_token` để M8 giải mã bằng khoá do Platform quản lý.

| Được | Mất |
| --- | --- |
| **Bỏ được kho khoá** — mục `2` của phiếu cho Sếp biến mất hoàn toàn | ⚠️ **DB của IVR từ nay giữ số khách** |
| Bỏ được **bộ phát hành token** mà M3 chưa xây | Phải dọn `3` cột cũ sau khi M3 chuyển xong |
| `OD-V1-05` · `OD-V1-17` · `OD-V1-18` **hết đối tượng** — `3` quyết định treo tự đóng | |
| `B11` của bản `16/09` (ba nguồn sự thật về vị trí E.164) cũng hết đối tượng | |

**Cái giá được ghi ở cả `3` nơi nó được trả** — doc-comment của entity, doc-comment của migration,
và `docs/compliance/data-inventory.md` — chứ không chỉ trong commit message. Nguyên văn ghi trong
danh mục: *trường **nhạy cảm nhất** IVR giữ, và là trường **duy nhất định danh trực tiếp một con
người** thay vì qua một khoá do bên khác giữ. Khác token: **nó không tự hết hạn**, nên xoá là cách
duy nhất làm nó biến mất.*

**Không giữ trong RAM được.** Cửa sổ xác nhận `5–15` phút và lần gọi thứ hai ở `+150s`/`+450s`, nên
số phải sống qua một lần restart worker — đúng yêu cầu bền vững đã đặt ledger vào Postgres.

---

## 1. Cảnh báo `CRITICAL`, và vì sao không cần sửa cái được cảnh báo

`gitnexus_impact` trước khi sửa:

| Symbol | Risk | Ảnh hưởng |
| --- | --- | --- |
| `IDialTokenResolver` | 🔴 **CRITICAL** | `214` ký hiệu · `53` trực tiếp · `5` implementation · `4` module |
| `TaskIntakeService` | 🟡 MEDIUM | `56` ký hiệu · `5` trực tiếp |

Đã báo owner trước khi sửa, theo `CLAUDE.md`. **Nhưng phản ứng đúng với một cảnh báo blast-radius
không phải là xin duyệt rồi sửa liều — mà là đọc lại thiết kế cho tới khi không phải sửa nó.**

Đọc kỹ thì ra điều làm cả lô nhỏ hẳn đi:

> `DialAuthorization` **cũng** chặn số trần — `OpaqueReferenceGuard.EnsureNotRawPhone` nằm trong
> constructor riêng của nó.

Nghĩa là **đường quay số chưa bao giờ mang số**. Nó mang **địa chỉ quay**: `sip:+84…@carrier-host`.
Cho nên `A` và `B` **giống hệt nhau từ sau bước lấy số ra** — parse, format theo nhà mạng, dựng địa
chỉ SIP đều không đổi. Khác biệt đúng **một chỗ**: giải mã token, hay đọc cột.

⇒ `B` là **một nhánh `if` trong một method**, không phải vault thứ sáu, không phải decorator, không
đụng `IDialTokenResolver`. Record `DialTokenResolutionRequest` chỉ thêm **một field optional có giá
trị mặc định**, nên `214` ký hiệu kia không đổi một dòng nào.

---

## 2. Ba gate bắt lỗi trước khi tôi kịp nhớ chúng tồn tại

| Gate | Từ chối cái gì |
| --- | --- |
| `PersonalDataInventoryTests` (lần `1`) | Một cột dữ liệu cá nhân **không có trong danh mục** |
| `PersonalDataInventoryTests` (lần `2`) | **Tài liệu tuân thủ không khớp code** |
| `TaskIntakeSchemaParityTests` | Contract có field mà **allowlist endpoint không có** |

Đây là loại gate không quan tâm tôi *định* làm gì. Nó chỉ hỏi: cột chứa số điện thoại thì đã khai
chưa. Không gate nào trong ba cái này đọc ý định của tôi; cả ba đọc **hiện vật**.

### Và một test đã tự dự đoán chính xác lượt này

`MigrationPreflightNamesLegacyRowsThatViolateTheSignedTaxonomy` có sẵn comment:

> *"The fragility is real and worth naming: **the next column added to `ivr_confirmation_tasks` will
> need the same line.**"*

Đúng. Đã thêm đúng dòng đó, kèm ghi chú trỏ ngược lại lời dự đoán.

---

## 3. Lỗi của tôi, do phiên `ivr-98` bắt

Tôi chạy replace trần `1.0.0-draft.29` → `1.0.0-draft.30` trên `IR-06` và `IR-07`. Nó **rewrite cả
một tên file**:

```
docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.28-to-v1.0.0-draft.29.md   ← có thật
docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.28-to-v1.0.0-draft.30.md   ← tôi tạo ra, không tồn tại
```

Trong **chính phiếu owner sắp gửi M3**. Bài học cụ thể hơn *"cẩn thận hơn"*: **một phép replace đủ
hẹp để bắt một version pointer thì cũng vừa đủ rộng để phá một filename chứa version đó** — phải
neo vào ngữ cảnh (backtick, khoảng trắng), không replace trần.

Họ sửa link. Tôi sửa phần còn lại thuộc về mình:

- `5` chỗ lẫn phiên bản trong `IR-07` (dòng `92` `103` `104` `126`) → nhất quán `draft.30`
- **Sinh changelog `29→30`** — thứ phiếu đang trỏ tới mà không có trên đĩa
- Đóng băng `baselines/…draft.30.yaml` cạnh `27`/`28`/`29` để tái tạo được
- Quét lại toàn bộ link `../` trong `IR-07`: **`0` gãy**

---

## 4. Contract

`1.0.0-draft.29` → **`1.0.0-draft.30`**, thêm **một field tuỳ chọn**.

```
1 changes: 0 error, 0 warning, 1 info
info  [new-optional-request-property] in API POST /tasks
      added the new optional request property `phone_e164`
```

`oasdiff` image ghim `v1.26.1`, cả `29→30` lẫn `27→current`: **không breaking**, `exit 0`.

Pattern cố ý hẹp — `^\+84[0-9]{9}$` — vì hệ thống này chỉ quay di động Việt Nam, và số `0` đứng đầu
hoặc thiếu `+` là lỗi producer phổ biến nhất. Chặn ở schema cho `400` nêu tên field, thay vì một
cuộc gọi không bao giờ kết nối.

**Hai shape cùng được chấp nhận, chọn ở lúc quay số chứ không phải lúc nhận.** Một task nhận hôm
qua bằng token có thể được quay hôm nay bởi worker đã hiểu số — cả hai phải chạy trong cùng một
tiến trình. Nhờ vậy **M3 chuyển khi nào họ sẵn sàng, không cần release đồng bộ**.

---

## 5. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Toàn bộ solution | **`1151/1151`** (`781` unit · `338` integration · `24` contract · `8` chaos), `0` failed `0` skipped |
| Gate sweep | **`39/39`** |
| `TestId` mới | `UT-TRUNK-DIAL-08..12` |
| Traceability | `716` → **`721`** |
| Mutation | tắt nhánh số trực tiếp ⇒ `UT-TRUNK-DIAL-08` đỏ |
| Re-pin | `8` nơi, tính trên **byte LF** |

### Một chi tiết khi re-pin đáng ghi

`docs/evidence/W-0183/…template.json` giữ một hash mà **một ký tự viết dạng escape** `9`. Đó
không phải lỗi gõ — nó là **fixture** chứng minh parser của validator giải escape **trước khi** so
sánh (`JSON_SHAPE_SINGLE_SOURCE`). Re-pin bằng một hash trần sẽ **lặng lẽ khai tử** tính chất đó,
nên hash mới giữ nguyên cách viết ấy ở đúng vị trí cũ.

---

## 6. Còn lại

| Stage | Việc | Ghi chú |
| --- | --- | --- |
| `3` | `phone_e164` thành **bắt buộc**, `dial_token` thành optional | **Đây mới là lần breaking** |
| `4` | `OD-V1-05` · `OD-V1-17` · `OD-V1-18` → `SUPERSEDED` | Và `B11` của bản `16/09` |
| `5` | Drop `3` cột dial-token | **Chỉ sau khi M3 đã cắt sang** |

Không stage nào trong ba cái trên làm được **trước khi M3 xác nhận đã chuyển** — đó là ranh giới
expand/contract, không phải việc chờ ngày công.
