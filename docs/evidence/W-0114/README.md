# W-0114 — Cổng "rolling deploy không gãy giữa chừng"

Ngày: `2026-08-24`
Baseline: `main@d4ceb38`
Trạng thái: `TESTS_PASS`
Plan: [`remaining-work-plan-2026-08-22.md` §A8](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)

> Không đổi một dòng production nào. Bản này chỉ thêm cổng và tài liệu — cùng lý do: một cổng mà
> phải sửa code mới xanh được thì không còn là phép đo nữa.

---

## 1. Lỗi đã đóng, và nó có hai chiều

[`docs/owner-decisions-open.md`](../../owner-decisions-open.md) ghi một chiều:

> migration "code mới chịu được schema cũ" — chưa có cổng; là **việc kỹ thuật** còn lại.

[`deploy/ci/rollback.md`](../../../deploy/ci/rollback.md) §3 ghi chiều còn lại, và kết bằng đúng
bốn chữ: *"nó chưa có test nào ép"*. Câu đó đúng lúc viết (`W-0045`) và **hết đúng từ `W-0046`** —
`IT-MIGRATE-03` đã quét mã nguồn migration từ đó, chỉ là không ai quay lại sửa câu này. Ghi rõ ở
đây vì một tài liệu tự nhận "chưa có gì canh" trong khi đã có là loại sai nguy hiểm hơn im lặng.

Hai câu ấy nói về hai cửa sổ khác nhau của cùng một lần deploy. Chart chạy migration bằng Job
`pre-upgrade` (`hook-weight: -5`), nên trình tự thật là:

| Thời điểm | Code | Schema | Ai gặp ai |
| --- | --- | --- | --- |
| trước upgrade | `N-1` | `N-1` | — |
| hook đang chạy / vừa xong | `N-1` | `N` | **code cũ trên schema mới** |
| pod đang cuốn | trộn `N-1`+`N` | `N` | — |
| `helm rollback` | `N-1` | `N` | **code cũ trên schema mới** |
| deploy bỏ hook, hoặc trỏ nhầm DB chưa migrate | `N` | `N-1` | **code mới trên schema cũ** |

Chiều mà `helm rollback --atomic` — cơ chế rollback chính trong readiness board — thật sự đi qua là
**chiều đầu**. Plan §A8 đặt tên chiều thứ hai. Cổng này làm cả hai, vì làm một chiều rồi ghi
"rolling deploy đã có cổng" là câu nói rộng hơn thứ đã đo.

---

## 2. Chiều "code mới trên schema cũ" — `IT-SCHEMA-NEWCODE-01`

Điều phải giữ **không phải** "vẫn chạy". Nó không thể chạy: cột chưa có. Đo thẳng trên máy —
binary hiện tại, schema của migration liền trước:

```
PostgresException 42703: column i.voice_id does not exist
```

Nên điều phải giữ là **hỏng đúng một dạng mà rolling deploy sống được**:

| Phải | Nếu không |
| --- | --- |
| `/health/ready` = `503` `schema_behind` | pod nhận traffic rồi trả `42703` cho người gọi |
| `/health/live`, `/health/startup` = `200` | liveness đỏ ⇒ **crash-loop** mọi replica; `--atomic` cần rollout **đứng** chứ không cần nó giãy, và crash-loop đốt hết `--timeout` trước khi lùi |
| readiness tự xanh lại khi migration xong, **không restart** | API không được báo hook đã xong; readiness chốt cứng ⇒ rollout đứng vì một schema **đã đúng** |

Ba dòng ấy là ba assertion, theo đúng thứ tự đó, trong một tiến trình duy nhất.

Vài lựa chọn đáng nói:

- **Dựng schema tiến từ rỗng**, không lùi bằng `Down`. Lùi bằng `Down` là đang kiểm các hàm `Down`;
  schema mà một lần deploy thật gặp là schema do release trước **dựng lên theo thứ tự**.
- **Cặp migration đọc từ assembly mỗi lần chạy** (`GetMigrations()[^2]`, `[^1]`). Ghi tên migration
  vào test là ghim cổng vào một cặp, mà cặp cần kiểm luôn là hai cái mới nhất — sau migration kế
  tiếp, một cổng ghim cứng vẫn xanh trong khi nó đã kiểm nhầm.
- **Khởi động đúng entry point của bản ship** (`WebApplicationFactory<Program>`), không phải host
  ráp trong test. Thứ đang kiểm là hành vi của binary sẽ chạy thật.

### `IT-SCHEMA-NEWCODE-02` — khoá tiền đề

Một test kiểu "hệ thống từ chối phục vụ khi schema tụt hậu" xanh được vì hai lý do rỗng: schema
không hề tụt hậu, hoặc migration mới nhất chẳng đổi gì. Test này khoá cả hai — đúng một bậc, bậc đó
là migration mới nhất, và migration đó có thao tác thật.

Rồi nó đo tiếp **vì sao** từ chối traffic là bắt buộc: với mọi bảng mà release này thêm cột, câu
`SELECT` mà chính EF sinh ra bị `42703`. Có điều kiện, và phải có điều kiện — một release chỉ tạo
bảng mới thì mọi câu đọc cũ vẫn chạy, đòi nó phải hỏng là đòi sai. Hôm nay điều kiện đúng với
`ivr_call_attempts` vì `W-0113` thêm ba cột vào đó.

---

## 3. Chiều "code cũ trên schema mới" — `UT-SCHEMA-BACKCOMPAT-01`

Chiều này **không thể** kiểm bằng cách chạy thứ gì: binary phải chạy là binary của release trước,
và CI không có nó. Nhưng `rollback.md` §3 đã tự nói ra thứ có thể kiểm — đây là ràng buộc lên
**cách viết migration**, không phải lên rollback.

`IT-MIGRATE-03` (`W-0046`) đã canh phần này bằng cách **quét mã nguồn**. `W-0114` mở rộng nó và
chuyển sang **mô hình thao tác có kiểu** của EF. Cổng đọc `Up` của **cả 12** migration qua
`Migration.UpOperations`, và từ chối bảy dạng:

| Thao tác | Release trước gãy ở đâu |
| --- | --- |
| `DropColumn` | EF gọi tên cột trong mọi câu đọc entity ⇒ `42703` |
| `DropTable` | như trên, ở mức bảng |
| `RenameColumn` / `RenameTable` | nhìn từ release trước, đổi tên = xoá + thêm |
| `AddColumn` `NOT NULL` không default | `INSERT` của release trước không nêu cột ⇒ `23502` |
| `AlterColumn` **thu hẹp** (siết nullable, rút ngắn, đổi kiểu) | từ chối chính những lệnh ghi đang chạy dở; nới rộng thì không, nên không bị chặn |
| `AddUniqueConstraint` / unique index | quy tắc duy nhất mà release trước không biết để giữ |
| `AddCheckConstraint` lên cột **đã có từ trước** | luật mới áp lên lệnh ghi đang bay |

### Vì sao có hai cổng cho một tính chất — và vì sao giữ cả hai

Ba dòng cuối bảng trên là thứ `IT-MIGRATE-03` **không thấy**. Đọc văn bản có đúng hai điểm mù:

1. Nó tìm `Up` bằng `indexOf` rồi dừng ở `Down`. Một thao tác do **hàm phụ** phát ra — chuyện rất
   bình thường trong migration viết tay — nằm ngoài lát cắt đó và vô hình; trong `UpOperations` thì
   nó hiện ra như mọi thao tác khác.
2. Nó thấy **lời gọi**, không thấy tham số. Nên nó buộc phải chặn **mọi** `AlterColumn`, kể cả nới
   rộng cột (an toàn theo chiều này). Thao tác có kiểu mang theo cả trạng thái trước và sau, nên
   phân biệt được nới rộng với thu hẹp.

Đổi lại, `IT-MIGRATE-03` chạy trong image node **không cần .NET toolchain**, nên nó đỏ ở stage
`validate` chứ không phải sau một lần build Release. Rẻ hơn và sớm hơn. Vì vậy **giữ cả hai**, và
mỗi bên đều có ghi chú trỏ sang bên kia — hai cổng không biết nhau là cách một trong hai bị xoá
"vì trùng".

Điểm chung của cả hai: chỉ đọc `Up`. `Down` toàn lệnh xoá theo đúng định nghĩa của nó, nên một
phép quét không phân biệt được hai bên sẽ báo gần như mọi migration trong repo.

### Dòng `AddCheckConstraint` là dòng duy nhất có phán đoán

Ràng buộc chỉ nói về cột **cùng migration này thêm vào** thì vô hại: release trước để null. Ràng
buộc động đến cột cũ thì là luật mới áp lên lệnh ghi đang bay. Phân biệt hai thứ đó bằng cách lấy
tên cột **từ EF model**, không phải đoán token nào trong SQL là identifier — nên `IS`, `NULL`, `IN`
không bao giờ bị nhận nhầm là cột, chuỗi trong nháy đơn (`'North'`) bị bỏ trước khi quét, và một
cột tên `value` không bao giờ bị bỏ sót.

Cả hai ràng buộc `W-0113` đã ship đều thuộc dạng vô hại, và `UT-SCHEMA-BACKCOMPAT-03` khoá đúng
hình dạng đó lại làm control.

### Danh sách miễn trừ đang rỗng — và đó là kết quả đo, không phải mặc định

Cả 12 migration đều đã thuần bổ sung. Danh sách vẫn tồn tại vì cách hợp lệ để xoá một cột là
expand/contract — thêm, ship code thôi đọc cột cũ, rồi mới xoá ở release sau — và cái release cuối
cùng ấy cần chỗ để nói ra điều đó. Không có chỗ nói thì người ta xoá cổng.

Nó là hằng số **trong chính file test**, không phải JSON để bên cạnh: thêm một miễn trừ khi ấy hiện
lên trong review là *"có người sửa cổng tương thích schema"*, chứ không phải một dòng trong file
không ai mở. Khoá miễn trừ là `{migration}::{thao tác}::{bảng.cột}` — hẹp vừa đủ để hết khớp khi
migration bị sửa, nên một miễn trừ không lặng lẽ che được thay đổi thứ hai.

### `UT-SCHEMA-MIGRATION-04` — cái bẫy tìm thấy khi viết cổng

EF tìm migration bằng attribute `[Migration]`, không bằng lớp cha. Một lớp kế thừa `Migration` mà
thiếu attribute vẫn biên dịch, vẫn đọc như đã áp trong review, và **không bao giờ chạy** — schema
lặng lẽ thiếu đúng thứ nó mô tả, và lỗi hiện ra muộn hơn nhiều, ở production, dưới dạng một cột
không tồn tại. Test này khoá attribute, khoá id không trùng, và khoá id sắp đúng thứ tự — vì
"migration liền trước" của toàn bộ cổng được định nghĩa bằng thứ tự đó.

---

## 4. Cổng CI

Job riêng `schema_compat_gate` (`stage: test`), `allow_failure: false`.

Job riêng chứ không phải một dòng trong `build_test_dotnet`, vì nó trả lời câu hỏi **triển khai**
chứ không phải câu hỏi code: khi nó đỏ, cách sửa thường là đổi cách viết migration, và điều đó cần
hiện lên như một đèn đỏ có tên chứ không phải như một trong vài trăm test hỏng.

`ci-config-selftest` giữ hai điều: job phải **tồn tại** (nằm trong `requiredJobs`), và ảnh phải khớp
`global.json`. Xoá job đi là pipeline đỏ, không phải mất cổng trong im lặng.

### `TreatNoTestsAsError` không phải trang trí

Cả hai bước chọn test theo trait. Trait bị đổi tên, file bị dời — `dotnet test` **exit 0** sau khi
chạy đúng không test nào. Đã đo:

| Lệnh | Exit |
| --- | --- |
| filter không khớp gì, **có** cờ | `1` |
| filter không khớp gì, **không** cờ | `0` |

Một cổng xanh vì nó không chạy còn tệ hơn không có cổng.

---

## 5. Đối chiếu yêu cầu (plan §A8)

| Yêu cầu | Test | Kết quả |
| --- | --- | --- |
| Job CI chạy binary mới trên schema của migration trước đó | `schema_compat_gate` | ✅ job riêng, `allow_failure: false` |
| Yêu cầu smoke pass | `IT-SCHEMA-NEWCODE-01` | ✅ live/startup `200`, ready `503 schema_behind`, tự phục hồi |
| Điều kiện để rolling deploy không gãy giữa chừng | `IT-SCHEMA-NEWCODE-01/02` + `UT-SCHEMA-BACKCOMPAT-01` | ✅ cả hai chiều |
| `helm rollback --atomic` là cơ chế rollback chính | `UT-SCHEMA-BACKCOMPAT-01` | ✅ chiều mà rollback thật sự đi qua |

---

## 6. Kết quả kiểm chứng

| Suite | Kết quả |
| --- | --- |
| `Ivr.UnitTests` | **484 / 484** (+4) |
| `Ivr.IntegrationTests` | **254 / 254** (+2) |
| `Ivr.ContractTests` | **22 / 22** |
| `Ivr.ChaosTests` | **8 / 8** |
| **Tổng .NET** | **768 / 768** |
| admin-ui | lint + `tsc` + **221 / 221** + build |
| `dotnet format --verify-no-changes` | PASS |
| `ci-config-selftest` | PASS (`CT-CI-08` gồm cả job mới) |

Không đổi contract: 0 operation mới, 0 field mới, không `oasdiff`, không re-pin manifest.

### Cổng thật sự bắt được — đã thử làm nó hỏng

Tắt đúng một nhánh trong `IvrReadinessProbe` (nhánh trả `schema_behind`), dựng lại, chạy lại:

```
Assert.Equal() Failure: Values differ
Expected: ServiceUnavailable
Actual:   OK
```

Nghĩa là: bỏ nhánh đó ra, pod **báo Healthy trên schema cũ** và sẽ nhận traffic. Đã hoàn nguyên
ngay sau phép đo; không dòng production nào thay đổi trong bản này.

`UT-SCHEMA-BACKCOMPAT-02` làm việc tương tự nhưng thường trực: bảy migration tổng hợp, mỗi cái mang
một dạng thao tác gãy, và cổng phải chỉ đúng dạng đó. Kèm `-03` làm control — một cổng bắt mọi thứ
cũng vô dụng ngang một cổng không bắt gì.

---

## 7. Những gì bản này **không** làm

- **Không chạy binary của release trước.** CI không có nó. Chiều "code cũ trên schema mới" được ép
  bằng ràng buộc lên cách viết migration, đúng như `rollback.md` §3 đã phát biểu — không phải bằng
  cách chạy thử.
- **Không xoá `IT-MIGRATE-03`.** Nó rẻ hơn, sớm hơn, và một cổng đang chạy tốt không phải thứ nên
  bỏ đi kèm theo một việc khác. Quan hệ giữa hai cổng được ghi ở cả hai đầu.
- **Không thấy `migrationBuilder.Sql(...)`.** SQL thô là một chuỗi với cả hai cổng. Một lệnh
  `ALTER TABLE ... DROP COLUMN` viết tay đi lọt — giới hạn có thật, chung cho cả hai, và cách đóng
  là không viết migration bằng SQL thô.
- **Không kiểm ở mức container.** `IT-SCHEMA-NEWCODE-01` khởi động đúng entry point của bản ship
  nhưng trong tiến trình test, không phải image đã publish. Entrypoint, user và biến môi trường của
  image đã thuộc `image_selftest`.
- **Không kiểm dữ liệu hiện có có vi phạm ràng buộc mới không.** Cổng đọc *hình dạng* migration,
  không đọc dữ liệu. Một `AddCheckConstraint` hợp lệ theo cổng vẫn có thể hỏng lúc áp nếu bảng đang
  chứa dòng vi phạm — đó là việc của A9.
- **Không sửa migration nào.** Cả 12 đã đạt; danh sách miễn trừ rỗng là kết quả, không phải một
  lượt dọn dẹp.
