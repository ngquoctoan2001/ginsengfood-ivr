# W-0311 — Phương án `B`: Module 3 gửi thẳng số điện thoại

**Ngày:** `2026-09-17` · **Baseline:** `main@ca8d13b` · **Nguồn:** quyết định owner `2026-09-17`

`REAL_CUSTOMER_CALL_ALLOWED=NO`

> ### ⚠️ Commit `ca8d13b` ghi nhầm `W-0310` ở tiêu đề
>
> `W-0310` là của phiên `ivr-98` (`5f00838`, phiếu M3 gộp), commit **trước** commit của tôi. Khi
> tôi cấp ID thì tracker đã sang `W-0311`. **Không sửa lại lịch sử commit** — ghi ở đây và ở
> tracker để sổ là nguồn đúng và chỗ lệch nhìn thấy được, thay vì được hoà giải lặng lẽ.
>
> **Sửa `18/09` (`W-0316`) — không chỉ ở tiêu đề.** Nhãn sai nằm cả trong nội dung, và do **ba** commit
> ghi chứ không phải một. Đếm lại tại `53c2eb5`:
>
> ```sh
> git grep -n -E "W-0310|W0310" 53c2eb5 -- . ':!prompt/_execution/prompt-execution-tracker.md'
> ```
>
> Bỏ các dòng nói về `W-0310` thật (phiếu M3), các dòng nói về chính chỗ nhầm này, và một dòng trong
> migration `W-0314` gọi đúng tên class migration. Còn lại **`18` dòng / `16` file** mang `W-0310` theo
> nghĩa *phương án B*, tức `W-0311`:
>
> | Commit ghi | Nơi còn nhãn sai tại `53c2eb5` | Dòng |
> | --- | --- | ---: |
> | `ca8d13b` — stage 1, tiêu đề sai | `IvrPersistenceEntities.cs` · `PersonalDataInventory.cs` · `data-inventory.md` · migration `20260917013914_W0310PhoneE164FromModule3`: `1` dòng chú thích, `3` dòng là tên (class, id `[Migration]`, class ở Designer) | `7` |
> | `7c4204e` — stage 2, tiêu đề **đúng** `W-0311` | `api-changelog.md` + `api-changelog.html` · baseline `draft.30` · `ProviderPorts.cs` · `PostgresTelephonyDispatchStore.cs` · `ProductionDialTokenVault.cs` · `SchedulerPersistenceTests.cs` · `SipTrunkProductionDialTests.cs` | `8` |
> | `53c2eb5` — `W-0314`, **tôi chép lại nhãn** từ danh mục | `RetentionTargetCatalog.cs` · `ComplianceTests.cs` · `retention-period-proposal.md` | `3` |
>
> `7c4204e` còn ghi nhãn ở `5` file nữa (OAS, model sinh, intake endpoint, intake service, trang HTML
> thứ hai); `W-0312` (`f7bb52b`) viết lại các dòng đó nên nay không còn. Con số *"17 dòng / 15 file"* ở
> Lô 3 mục `9` của [bản vướng mắc `17/09`](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md)
> không tái tạo được bằng lệnh trên; số ở đây thay cho nó.
>
> **Không sửa dòng nào.** `4` dòng nằm trong migration đã áp — luật `4` của
> [kế hoạch `16/09`](../../../plan/ke-hoach-khac-phuc-m8-2026-09-16.md) cấm sửa, và đổi tên class là đổi
> lịch sử DB. `1` dòng ở baseline đông cứng. `13` dòng còn lại là chú thích và tài liệu: sửa rải rác `13`
> nơi đúng là kiểu hoà giải lặng lẽ mà ghi chú trên muốn tránh. Bảng này là chỗ tra — gặp `W-0310` đi
> cùng *phương án B* hoặc `phone_e164` thì đọc là `W-0311`.

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

Nghĩa là **luồng quay số chưa bao giờ mang số**. Nó mang **địa chỉ quay**: `sip:+84…@carrier-host`.
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
| `5` | Drop `3` cột dial-token | **Chỉ sau khi M3 đã cắt sang** · **và** khi đã có lối có kiểm soát qua gate expand (`T6`, xem dưới) |

Không stage nào trong ba cái trên làm được **trước khi M3 xác nhận đã chuyển** — đó là ranh giới
expand/contract, không phải việc chờ ngày công.

**Điều kiện thứ hai của stage `5` — `T6`.** Toàn duyệt `17/09`; ghi ngày `18/09` ở `W-0316`; **chưa làm**.
Drop cột là `DropColumn`, và cả hai gate expand từ chối nó ở **mọi** migration mới:
`RollingDeploySchemaCompatibility` (unit test, đọc `UpOperations`) và `migration-expand-guard.mjs` (CI,
đọc mã nguồn). Lối miễn duy nhất hôm nay là `deploy/ci/migration-expand-baseline.json`, và nó **chỉ dành
cho lịch sử**: nhận migration có id **trước** mốc `20260827024438_W0118AttemptCountedInvariant`, ghim bằng
SHA-256 — hiện `2` migration, kèm câu *"No drop-table exception is permitted in the current expand
phase."* Migration của stage `5` sinh sau mốc nên không lọt được, và như thế là đúng.

Vì vậy stage `5` chỉ bắt đầu khi đã có một **lối có kiểm soát** dành riêng cho bản contract. Tối thiểu
gồm: đúng một migration được nêu tên và ghim hash; kiểm được rằng bản chạy trước đó (`N−1`) không còn
đọc `3` cột; Toàn duyệt. **Không** đi lối tắt: không dời mốc, không thêm migration mới vào danh sách lịch
sử, không tắt gate. Chưa ai dựng lối đó.

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0311 thuộc nhóm A2 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm: Phương án B: M3
gửi thẳng phone_e164. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: các stage sau chờ M3 chuyển sang. Mọi giới hạn trong cột
Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại `ca4f442` (1200/1200 test,
sweep 43/43), không coi commit tài liệu là một lượt test/sweep mới. REAL_CUSTOMER_CALL_ALLOWED=NO.
