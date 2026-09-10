# W-0264 — Wire schema định nghĩa hai lần, giờ bị buộc vào nhau

Ngày: 2026-09-10 · Baseline: `main@300d58b` · Trạng thái: **TESTS_PASS**.

S5 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).

Không đổi mã nguồn. Toàn bộ thay đổi là một tệp test.

## 1. Vấn đề

`TaskIntakeEndpoint` kiểm request body bằng tay, dựa trên sáu `HashSet<string>` chứa khoảng năm
mươi tên field. Những tên đó **cũng** tồn tại trong `IvrServerModels.g.cs`, sinh từ
`specs/api/openapi/ivr-order-confirmation.v1.yaml`.

Hai định nghĩa độc lập cho một wire schema, giữ đồng bộ bằng trí nhớ của người sửa.

Hỏng theo cả hai chiều và **im lặng cả hai**: thêm một field vào spec, regenerate, và endpoint vẫn
từ chối nó như field lạ — trong khi mọi test xanh, vì mỗi bên đều tự nhất quán với chính nó.

## 2. Đo, ba lần, vì hai lần đầu sai

Trước khi sửa gì, tôi đối chiếu sáu tập viết tay với contract sinh tự động. **Phép đo đầu báo ba
chênh lệch. Hai trong ba là lỗi của phép đo.**

| Lần | Kết quả | Điều gì sai |
| --- | --- | --- |
| 1 | 3 "DIFFERS" | `re.search('TaskProperties =')` khớp trúng **chuỗi con** của `RequiredTaskProperties`; và enum sinh ra không phải `string` nên bị coi là không-phải-chuỗi dù trên wire là chuỗi |
| 2 | 2 "DIFFERS" | Regex kiểu không cho phép **dấu cách trong generic**, nên bỏ sót `IDictionary<string, string>` — tức `pronunciation_hints` |
| 3 | **1 DIFFERS** | đáng tin |

Nếu tôi hành động theo lần đo thứ hai, tôi đã **gỡ `pronunciation_hints`** khỏi allowlist — một
field **có trong spec** (dòng 1172) và **được `PrivacySafeSpeech` trong Domain đọc**. Tức là sẽ tự
tay tạo ra đúng loại drift mà phát hiện này nói đến.

Và một khám phá nữa chỉ lộ ra khi chạy test thật: `IvrConfirmationTaskV1` kế thừa `Anonymous`, và
**cả hai lớp cùng khai báo `program_code`**. `GetProperties` trả về cả hai, nên phép giải phải là
"lớp dẫn xuất nhất thắng" — nếu để `GetProperties` quyết theo thứ tự trả về thì tập required sẽ phụ
thuộc vào thứ tự reflection, thứ không được đặc tả.

## 3. Chênh lệch thật, và tại sao không sửa nó ở đây

**`phone_validation_status`**: nằm trong danh sách `required` của spec
(`ivr-order-confirmation.v1.yaml:1193`) và được sinh là `required`, nhưng endpoint **chấp nhận task
thiếu nó**.

Nghĩa là IVR đang nhận những body mà chính contract nó công bố nói là không hợp lệ.

**Không sửa ở đây.** Bắt buộc nó sẽ **từ chối những body hôm nay đang được nhận** — một breaking
change trên API service-to-service, mà đầu bên kia là Module 3. Bên nào sai, contract hay endpoint,
là quyết định của owner cùng producer. Cái lượt này làm là khiến nó **không còn vô hình**.

Nó được khai báo như một ngoại lệ **được assert**, không phải một ngoại lệ được bỏ qua: nếu ai đó
giải quyết bằng cách bắt endpoint yêu cầu nó, dòng assert sẽ đỏ và danh sách ngoại lệ phải được gỡ.

## 4. Vì sao là test chứ không phải suy ra từ contract

Cho endpoint đọc thẳng type sinh tự động sẽ xoá hẳn trùng lặp — nhưng nó **đổi thứ intake chấp
nhận**, và chênh lệch ở mục 3 cho thấy nó sẽ từ chối body đang thành công. Đó là quyết định hợp
đồng, không phải refactor.

Nên: giữ hai bên, buộc chúng vào nhau, và nêu tên khác biệt.

Test đọc allowlist của endpoint bằng **reflection** chứ không chép lại — một bản chép trong test sẽ
là **định nghĩa thứ ba** của cùng một schema, mà test này tồn tại vì hai đã là quá một.

## 5. Test

| TestId | Khẳng định |
| --- | --- |
| `UT-INTAKE-PARITY-01` | Allowlist task nêu đúng 36 field của contract |
| `UT-INTAKE-PARITY-02` | Required list khớp contract, trừ đúng một ngoại lệ đã khai báo; và **không** field nào endpoint đòi mà contract không định nghĩa |
| `UT-INTAKE-PARITY-03` | `privacy_safe_order_summary`: 9 field, 8 required |
| `UT-INTAKE-PARITY-04` | `items[]`: khớp `OrderSpeechItem` |
| `UT-INTAKE-PARITY-05` | Mọi field bị kiểm "chuỗi không rỗng" đều là chuỗi trên wire — enum tính là chuỗi, vì generator sinh enum cho tập chuỗi đóng |

Đã làm cho đỏ thật **cả hai chiều drift**:

- Endpoint quên một field contract có → `Expected: [..., "phone_validation_status", ...] Actual: [...]`
- Endpoint đòi một field contract không có → `Assert.Empty() Failure: Collection: ["invented_field"]`

Đặt ở `Ivr.IntegrationTests` vì `Ivr.UnitTests` không tham chiếu `Ivr.Api`, còn integration thì có
sẵn. Nó không chạm database; đặt ở đó để **không** phải thêm project reference thứ hai và sinh lại
lock lần nữa trong cùng một phiên.

## 6. Một lỗi của tôi trong lượt này

Tôi tạo test ở `tests/Ivr.UnitTests/Intake/` trước, rồi khi chuyển nó sang integration đã chạy
`rm -rf tests/Ivr.UnitTests/Intake` — **thư mục đó đã tồn tại sẵn** với
`SignedProductionAttemptPolicyTests.cs` và `TaskIntakeServiceTests.cs`.

Thông báo lỗi Python ngay trước đó đã nói thẳng `The directory is not empty`, và tôi vẫn xoá.

Phát hiện khi unit suite tụt **667 → 636**. Khôi phục bằng `git checkout --`; không mất gì, nhưng
đó là nhờ git theo dõi chúng, không phải nhờ tôi cẩn thận. `git status` đã được kiểm để xác nhận
không còn xoá nào khác trong cây.

## 7. Kết quả

```
Unit         667/667
Integration  283/283   (+5)
Contract      24/24
Chaos          8/8
             ─────────
             982/982
```

`GATE_SWEEP_PASS 39/39` · `dotnet build Ivr.sln` — 0 warning, 0 error.

## 8. Còn lại

**18/26 đã đóng, 1 rút lại vì sai.** Không còn HIGH.

Cần owner — giờ là **bốn** câu hỏi, không phải ba:

- **`phone_validation_status`** — contract nói required, intake nhận thiếu. Sửa bên nào?
- **P2** — 109 index. Cần `pg_stat_user_indexes.idx_scan` từ staging hoặc production.
- **Retention chưa arm** — `PeriodDays` rỗng.
- **`"MOCK"` có hai chủ sở hữu** — tách tên xong thì 22 site thay được an toàn.

Thuần kỹ thuật: phần S4 còn lại (`assertString`, `assertExactKeys`, `readStrictJson` — không cái
nào là kiểm bảo mật), **B9**, **B10**, **C1**, **C3**, **C4**, **C5**.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
