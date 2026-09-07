# W-0204 — P2.1 Chốt contract: cơ chế đóng băng + đối chiếu trạng thái chữ ký

Ngày: 2026-09-06. Trạng thái: **TESTS_PASS cho phần cơ chế; `BLOCKED_EXTERNAL` cho phần chữ ký.**
Không tự ACCEPTED, và **không thay chữ ký của Module 3** — §2 của bản kế hoạch cấm đúng điều đó.

## 1. Lượt này làm gì và không làm gì

P2.1 có bốn gạch đầu dòng. Ba gạch đầu là **chữ ký của người khác**; gạch thứ tư là **cơ chế**, và
cơ chế thì làm được ngay. Lượt này:

- **Làm**: đóng băng thứ mà chữ ký sẽ trỏ vào, và làm cho việc "đổi trạng thái draft" không còn là
  sửa một chuỗi trong JSON.
- **Không làm**: ký thay ai. Không dòng nào trong pack này chuyển một câu hỏi external thành đã trả
  lời.

## 2. Bốn lỗ hổng đóng băng, tìm bằng cách chạy chứ không bằng cách đọc

`openapi-contract-drift.mjs` đã ghim và cưỡng chế hash của hai spec — phần khó nhất, và đã có từ
trước. Cái nó **không** phủ, và P2.1 gạch 4 lại yêu cầu, là phần còn lại:

| Lỗ hổng | Vì sao nó quan trọng | Trạng thái |
|---|---|---|
| **Generated client không được ghim** | Manifest khai `generated:` bằng **đường dẫn**, chưa bao giờ bằng hash. Đó chính là những byte Module 3 biên dịch. Ghim spec mà không ghim artifact sinh ra từ spec là để hở đúng thứ consumer thật sự link vào | đã ghim `generatedSha256` cho cả hai |
| **Một pin viết ở hai nơi** | Closure pack — chính tài liệu mời owner ký — chép lại hash dưới dạng văn xuôi. Văn xuôi không được cập nhật khi spec xoay, nên pack đang gọi tên baseline `b59a644e…` từ thời `draft.18`, trong khi manifest ghim `b32a75ce…` của `draft.23`. **Ký vào pack đó là ký vào một artifact không xác định được** | đã bỏ số khỏi pack, trỏ về manifest; `FREEZE-02` chặn tái diễn |
| **Bảng field công bố có thể lệch spec** | Owner duyệt `06-module-3-api-handover.md`, không duyệt YAML. Hai bên lệch nhau thì chữ ký nằm trên một hợp đồng không ai thực thi | `FREEZE-03` so khớp từng tên field, cả required lẫn optional |
| **Trạng thái draft không ai đọc** | `TARGET_CONTRACT_V1=DRAFT` chỉ tồn tại trong một chuỗi JSON và một dòng ledger; **không gate nào đọc nó**, nên không gì ngăn việc sửa chuỗi đó thành "đã ký" | `FREEZE-04` từ chối trạng thái khác DRAFT khi không có `closureEvidence`, và từ chối bỏ hậu tố pre-release khi còn DRAFT |

Lỗ hổng thứ hai không phải giả thuyết: verifier đỏ ngay lần chạy đầu tiên và chỉ đúng vào nó.

## 3. Gate và bằng chứng nó có răng

`deploy/ci/scripts/contract-freeze-verifier.mjs` — năm kiểm tra:

| ID | Kiểm |
|---|---|
| `FREEZE-01` | mọi artifact chữ ký phủ tới đều được ghim và khớp: hai spec, compat fixture, **hai generated client** |
| `FREEZE-02` | trong tập tài liệu governance, mọi chuỗi 64-hex phải là một hash manifest đang ghim |
| `FREEZE-03` | tên field trong IR-06 §3.4/§4.2 == `required`/optional của spec, hai chiều |
| `FREEZE-04` | còn DRAFT thì mọi entry phải mang status draft và mọi spec phải giữ hậu tố pre-release; khác DRAFT thì phải có `closureEvidence` |
| `FREEZE-05` | `docs/contracts/target-v1-field-inventory.md` sinh lại phải trùng bản đã commit |

`contract-freeze-selftest.mjs` **11/11 PASS**: mỗi case phá đúng một thứ trong bản sao tạm của repo
và đòi đúng mã kiểm tra tương ứng đỏ lên; một case làm đỏ thêm mã khác cũng bị tính là hỏng. Lý do
có selftest nằm ở `W-0126`: một gate lấy kỳ vọng từ cùng nguồn với thứ nó kiểm thì không bao giờ đỏ
được, và sẽ bị đọc nhầm thành bằng chứng.

Case đáng chú ý nhất là `draft-suffix-dropped-while-still-draft`: nó thực hiện **trọn vẹn** một lượt
xoay hợp pháp — repin spec, mang hash mới vào human diff, sinh lại inventory — để thứ duy nhất còn
lại bị phàn nàn là chuỗi version. Đó đúng là luận điểm của `W-0200`: một lượt xoay có thể làm hoàn
hảo về kỹ thuật mà vẫn là một tuyên bố phê duyệt sai sự thật.

## 4. Bề mặt wire — đã đối chiếu bằng máy

| Thứ | Tài liệu công bố | Spec đã ghim | Khớp |
|---|---:|---:|---|
| Intake `IvrConfirmationTaskV1` required | 22 | 22 | ✅ từng tên |
| Intake optional | — | 14 | ✅ có trong inventory |
| Callback `IvrResultCallbackV1` required | 13 | 13 | ✅ từng tên |
| Callback optional (`result_reason`) | 1 | 1 | ✅ |

Cả hai schema đều `additionalProperties: false`. Bản đầy đủ field + enum:
[`target-v1-field-inventory.md`](../../contracts/target-v1-field-inventory.md) — **sinh ra từ spec
đã ghim**, nên nó không thể mô tả một hợp đồng khác với cái gate đang cưỡng chế. Đây là trang duy
nhất cần đọc khi duyệt bề mặt wire.

Enum được tách hai bảng: **value set** (nhiều giá trị, cần cân nhắc) và **pinned constant** (một giá
trị, ví dụ `recordingEnabled: false`). Không lọc bỏ hằng số: một enum một giá trị vẫn là điều khoản
ràng buộc consumer, bỏ đi là giấu điều khoản.

## 5. Trạng thái chữ ký thật, đối chiếu với repo chứ không chép lại

`specs/_review/open-decisions-register.md` cho thấy **owner đã ký đóng `OD-V1-01..08` và
`OD-V1-12..18` ngày 2026-09-05** — nhiều hơn hẳn những gì `IR-05` (viết `2026-09-03`) còn mô tả.

| Gạch P2.1 | Phần owner | Phần bên kia |
|---|---|---|
| 22 field / 13+1 field | ✅ đã đối chiếu bằng gate | ⛔ M3 chưa ký bề mặt (IR-06 §10) |
| enums | ✅ có inventory sinh tự động | ⛔ chưa ký |
| versioning | ✅ `docs/api-versioning.md` | — |
| **compatibility window** | ✅ `OD-V1-22` ký `2026-09-06` — **≥ 90 ngày**, đồng hồ chạy khi successor vừa được duyệt **vừa** có ở non-prod | ⛔ M3 xác nhận |
| producer theo `decision` | ✅ `OD-V1-14` CLOSED | ⛔ producer SHA/OpenAPI/CDC |
| program × payment | ✅ `OD-V1-01`, `OD-V1-13` CLOSED | ⛔ M3 phát task `GOLDEN_HOUR+ONLINE` |
| freshness / revoke | ✅ `OD-V1-03`, `OD-V1-06` CLOSED | ⛔ |
| `golden_hour_session_id` | ✅ M8 ký đề xuất (`W-0146`) | ⛔ M3 chưa đồng ký; **code/OpenAPI/DB chưa được phép đổi** |
| attempt policy version | ✅ `OD-V1-08`+`16` CLOSED, `gh-247-prod-v1` đã đăng ký (`W-0198`) | ⛔ M3 producer/CDC |
| `dial_token` opaque | ✅ `OD-V1-05`,`17`,`18` CLOSED, ledger đã có (`W-0199`) | ⛔ vendor/Security artifact |
| auth profile / rotation | ✅ `OD-V1-07` CLOSED (JWT bất đối xứng, JWKS, TTL ≤ 10′, scope `ivr.task.write`; mTLS hoãn) | ⛔ credential sandbox (`OQ-AUTH-01`) |
| **opt-out boundary** | ⏳ `OD-V1-23` ký `2026-09-06` — **explicit-only V1**, không suy từ số lần `Rejected`; hằng số `2/3` hiện tại là khoảng trống, không phải quy tắc | ⛔ Legal/Privacy + CRM/M3 chưa đồng ký |
| freeze version/client/hash | ✅ **lượt này** | — |

Hai mục đó **đã được hỏi và đã quyết trong chính lượt này** (`2026-09-06`), sau khi lượt rà soát
phát hiện chúng bị bỏ sót vì nằm giữa hai tài liệu: compatibility window chỉ sống trong
`api-versioning.md` như một "candidate default", còn opt-out boundary không có dòng `OD-V1` nào cả.

- `OD-V1-22` — compatibility window **≥ 90 ngày**, hai điều kiện khởi động (duyệt **và** có ở
  non-prod). Nhận nguyên con số đang viết sẵn vì nó bảo thủ; đổi lúc này chỉ tạo ra một số chưa ai đo.
- `OD-V1-23` — opt-out **explicit-only V1**. Hệ quả cần nói thẳng: hằng số `2/3` (suy opt-out từ
  nhiều lần `Rejected`) **không có authority** và từ nay phải được xử lý như một khoảng trống, không
  phải như một quy tắc đang chạy. Đây **mới là vị trí owner**, chưa phải quorum — Legal/Privacy và
  CRM/M3 vẫn phải đồng ký trước khi có bất kỳ code opt-out nào.

## 6. Vì sao trạng thái vẫn là `TARGET_CONTRACT_V1=DRAFT`

Vì nó đúng. Owner đã ký các quyết định; **Module 3 chưa đồng ký bề mặt và chưa giao producer
artifact**, mà closure evidence của chính những dòng `OD-V1` đó liệt kê producer SHA/OpenAPI/CDC là
điều kiện đóng. Đổi chuỗi trạng thái bây giờ sẽ là tuyên bố có chữ ký mà chưa có.

Điều đã đổi là: từ nay **không đổi được bằng cách sửa chuỗi**. `FREEZE-04` đòi `closureEvidence`
tồn tại; khi M3 ký, việc lật trạng thái là một hành động có kiểm chứng, không phải một lần soạn thảo.

## 7. Tái lập

```bash
npm --prefix deploy/ci ci --no-audit --no-fund
npm --prefix deploy/ci run contract:freeze
npm --prefix deploy/ci run test:contract-freeze
```

Sau khi review một thay đổi hợp đồng, làm mới phần dẫn xuất:

```bash
npm --prefix deploy/ci run contract:freeze:write
```

Cả hai lệnh kiểm đã được thêm vào job `openapi_lint` (`allow_failure: false`) trong
`deploy/ci/ci.gitlab-ci.yml`; `ci-config-selftest` và `docs-selftest` vẫn xanh sau thay đổi.

## 8. Giới hạn

- Đây là **cơ chế đóng băng**, không phải chữ ký. P2.1 chưa đạt exit và không được đọc thành đã đạt.
- `FREEZE-02` chỉ soi tập tài liệu governance được khai báo, không phải toàn repo — evidence pack có
  hash của chính nó là hợp lệ.
- `FREEZE-03` đọc bảng markdown theo cấu trúc; đổi tiêu đề mục trong IR-06 sẽ làm gate đỏ với lý do
  "không đọc được bảng", và đó là ý muốn: bảng đó là hợp đồng công bố, không phải văn xuôi tự do.
- Ghim generated client nghĩa là **mọi lần sinh lại client đều phải repin có chủ ý**. Đó là chi phí
  cố tình: nó biến một thay đổi im lặng thành một quyết định nhìn thấy được.
- Chưa ai ký. Trạng thái là `TESTS_PASS` cho cơ chế, không phải `ACCEPTED`.
