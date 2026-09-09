# W-0250 — `draft.24` phát hành, và ba thứ im lặng lộ ra trên đường

Ngày: 2026-09-09 · Baseline: `main@3a2b866` · Trạng thái: **TESTS_PASS**.

Lượt phát hành contract mà `W-0248`/`W-0249` xếp hàng chờ: ba header và một field, đưa OAS về đúng
thứ runtime đã ép từ lâu. Việc chính nhỏ. Ba thứ tìm thấy trên đường mới là phần đáng đọc.

## 1. Đổi gì trên contract

| | |
| --- | --- |
| `X-Correlation-Id`, `Idempotency-Key`, `OptionalIdempotencyKey` | `minLength: 1`, `maxLength: 128`, `pattern` `^[A-Za-z0-9._:-]+$` — đúng cú pháp `TraceHeaderSyntax` của `W-0221` |
| `phone_validation_status` | `required` + `enum: [VALID]` |
| Version | `1.0.0-draft.23` → `1.0.0-draft.24`, baseline xoay lần thứ tư |

Mục `C6` trong worklist 07/09 yêu cầu đúng chữ này: *"phone_validation_status required enum
[VALID]"*, và tiên đoán luôn *"oasdiff sẽ báo breaking khi required hóa → ghi cutover"*. Nó báo
thật, và cutover đã ghi.

## 2. Đính chính con số của chính tôi

Đầu lượt tôi viết vào `docs/api-changelog.md` rằng oasdiff cho **"162 warnings, và zero errors"**.
**Sai.** Chạy lại bằng đúng image ghim `tufin/oasdiff:v1.26.1`:

```text
oasdiff breaking draft.23 → live      exit=1    110 errors    52 warnings
  request-parameter-pattern-added          56   header, thắt về đúng thứ runtime đã ép
  request-parameter-min-length-increased   52   header, cùng lý do
  request-property-became-required          1   phone_validation_status
  request-property-became-enum              1   phone_validation_status
```

162 là **tổng**, không phải số warning. Tôi đọc một con số rồi gọi tên loại của nó theo trí nhớ.
Changelog đã sửa: 110 errors, trong đó **108 là header** và **2 là field** — và hai cái đó mới là
hai dòng đáng đọc.

## 3. Cái giá thật của việc đóng enum, và tại sao vẫn đóng

Đóng `enum: [VALID]` khiến NSwag sinh **enum một thành viên** thay cho `string`. Ba chỗ gọi trong
`TaskIntakeService` phải đổi kiểu — nhưng hệ quả thật không nằm ở kiểu, mà **trên wire**:

> Một status sai nay bị chặn ở **schema**, trả `400 IVR_MALFORMED_REQUEST`, chứ không còn
> `422 IVR_CONTACT_INVALID` như trước.

Đây là **thay đổi breaking M3 nhìn thấy**. Ba lý do vẫn chọn đóng:

1. **Đã có tiền lệ ký trong chính spec này.** `ivr_confirmation_required` là `enum: [true]`, và
   `specs/api/06-error-codes.md` đã ghi sẵn hai dòng Policy đúng khuôn *"schema chặn trước service →
   400"*. Dòng contact này chỉ là dòng thứ ba nhập bọn.
2. **Không đóng thì `C6` không được giải.** Than phiền của `C6` là *"payload hợp lệ theo OAS vẫn bị
   422"*. Chỉ `required` thôi không chữa được điều đó; `PENDING` vẫn hợp lệ theo OAS và vẫn 422.
3. **Mất mát nhỏ hơn vẻ ngoài.** Chính `06-error-codes.md` §2a ghi: *"M3 chưa nhìn thấy chín mã chi
   tiết qua public intake route"*. M3 chưa bao giờ đọc được `PHONE_VALIDATION_STATUS_NOT_VALID`; nó
   chỉ thấy mã envelope, và mã envelope là thứ đổi.

Mã lý do đó **vẫn còn và vẫn nổ** cho sáu rule contact còn lại; chỉ riêng trigger này wire không với
tới. Rule trong `ContactRejectionReason` giữ nguyên cho các caller dựng DTO trong tiến trình — y hệt
cách rule `ivr_confirmation_required` sống dưới enum của nó.

Đã sửa theo ở **bốn** nơi từng hứa `422`: mô tả trong OAS (tôi viết sai ngay lượt này — nó nói
*"refused with 422"* trong khi chính nó làm điều ngược lại), `06-error-codes.md`, và **hai** bảng
trong IR-06, gồm bảng đối chiếu tiền-lab mà M3 đọc trực tiếp.

## 4. Selftest của contract-freeze đã im lặng, đúng kiểu nó sinh ra để chặn

`contract-freeze-selftest.mjs` đỏ 13/14 ở case `inventory-no-longer-describes-the-pinned-spec`.
Nguyên nhân **không** phải version: `phone_validation_status` thành required đẩy inventory từ
`Required: 22` lên `23`, nên `text.replace("Required: **22**", …)` **không khớp gì cả** — mutation
trả về bản sao nguyên vẹn, case assert trên một cây sạch.

Đó đúng là failure mode `W-0126` mà file này tồn tại để chặn, **quay lại chính nó**: một gate mà kỳ
vọng lấy từ cùng nguồn với thứ nó kiểm thì không đỏ được, nên xanh mãi và bị nhầm là bằng chứng.

Hai sửa, không phải một:

- `editText` **ném** khi mutation không đổi gì. Bắt được **mọi** case tương lai cùng loại, không chỉ
  case này. Bảy case dùng `editText` đều bắt buộc phải đổi text, nên guard này không có ngoại lệ.
- Case đếm field tự trừ từ con số đang có bằng regex, nên nó sống qua các lượt xoay field mà không
  cần re-pin.

## 5. Catalog dùng chung nói sai status, và không gì so hai bên

`seed/sales-target-v1.sample.json` là catalog M3 dựng test âm từ đó. `_layer_rules` trong chính file
ghi *"schema_negative → HTTP 400"*, `SchemaError` trong endpoint trả `400` từ đầu, và **cả mười hai**
fixture schema ghi `expect_http: 422`. Không gì so hai con số ấy, nên nó sai suốt.

Phát hiện được là vì `validate-openapi.mjs` **bắt buộc** mọi fixture `domain_negative` phải
schema-hợp-lệ, và `NEG-DOMAIN-PHONE-01` vừa hết hợp lệ khi enum đóng lại. Tức tầng của fixture bị
enum ép đổi, chứ không phải tôi thích đổi.

| | |
| --- | --- |
| `NEG-DOMAIN-PHONE-01` → `NEG-SCHEMA-PHONE-01` | chuyển tầng, `400 IVR_MALFORMED_REQUEST`, ghi rõ nó từng là gì |
| `NEG-DOMAIN-PHONE-02` (mới) | `phone_masked` chưa che — giữ một fixture hình-dạng-phone ở tầng domain |
| 12 fixture schema | `expect_http` từ `422` về `400` |
| `validate-openapi.mjs` | **ghim tầng vào status**, để fixture sau không thêm được vào nhầm bên |

Cùng lý do, evidence matrix đổi trigger: `ApiBehaviorMatrixTests` dùng status sai để chứng minh
`422` trên `POST /tasks`; nay dùng `phone_masked` chưa che, một trong sáu rule wire còn với tới —
nếu không, `422` đã thành **khai báo không có bằng chứng thực thi**.

## 6. Một gate chẩn đoán sai khi thiếu công cụ

`selftest-oasdiff.sh` không có `oasdiff` trên PATH thì rơi vào nhánh control và in
*"unchanged contract was reported as breaking"* — một chẩn đoán **tự tin và sai** về contract, trong
khi sự thật là thiếu binary. Đã thêm `command -v` đứng trước, nói đúng chuyện đang xảy ra.

## 7. Hai gate đỏ **không** phải của lượt này

`external-decision-response-validator` và `external-decision-closure-validator` (phụ thuộc cái
trước) đang đỏ vì `m8-05-program-result-contract-signoff-2026-09-03.md` lệch manifest. Đã kiểm bằng
`git stash`: **đỏ y hệt trên cây đã commit**, trước mọi thay đổi của tôi.

Nguồn: `a4a0cee` (`W-0217`) sửa file plan được ghim mà không re-pin hai manifest
`docs/evidence/W-0152/` và `W-0170/`. Ghim thật là `6525d2df…`, file hiện `a03fc6ca…`.

**Không sửa trong lượt này, có chủ ý.** Sửa nó nghĩa là ghi đè manifest nằm trong evidence pack đã
đóng, và việc đó cần lý do riêng đứng tên riêng — không nên lẫn vào một lượt phát hành contract.
Đáng thành work item kế tiếp; nó cũng phơi ra một điểm thiết kế: `artifact-sha256.txt` trong một
evidence pack đang **vừa là bản ghi đóng băng vừa là pin sống**, và hai vai đó xung đột.

## 8. Kiểm chứng

```text
detect_changes                             low · 0 affected processes · 62 symbols · 33 files
dotnet test Ivr.sln                        952/952 PASS, 0 failed, 0 skipped   (951 → 952)
traceability                               TEST_TRACEABILITY_WRITTEN=581
contract-freeze-verifier.mjs               CONTRACT_FREEZE=PASS
contract-freeze-selftest.mjs               CONTRACT_FREEZE_SELFTEST=PASS 14/14   (13/14 → 14/14)
validate-openapi.mjs                       SCHEMA_NEGATIVE_REJECTED=13  DOMAIN_NEGATIVE_SCHEMA_VALID=13
verify-api-behavior-matrix.mjs             verdict=PASS  failures=[]
docs-selftest.mjs                          API_DOCS_SELFTEST_PASS
opt-out / d06 / dial-token / upstream      4 validator re-pin, cả bốn SELFTEST_PASS
oasdiff breaking (image ghim)              110 errors + 52 warnings, đã phân loại ở §2
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1` vẫn `DRAFT` — xoay baseline không phải phê
duyệt, và không phải phát hành.

## 9. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| — | re-pin `m8-05` cho hai manifest `W-0152`/`W-0170`, và tách vai *bản ghi đóng băng* khỏi *pin sống* | tôi, lượt sau |
| — | endpoint revoke + OAS + IR-06 — `2.5`, đi cùng `7.1`/`2.2` ở lượt phát hành sau | tôi |
| — | M3 gửi đúng `VALID`, và biết status sai nay là `400` chứ không `422` | dev M3 |
