# W-0265 — Phiếu chốt một lần cho Module 3, và contract nó buộc phải sửa

Ngày: 2026-09-10 · Baseline: `main@632ad23` · Trạng thái: **TESTS_PASS**.

Owner yêu cầu: dọn tài liệu để **gửi qua Module 3 đúng một lần**, họ chốt, gửi lại, rồi triển khai.
Không qua lại nhiều vòng.

Yêu cầu ấy quyết định toàn bộ hình dạng lượt này. "Ép chốt một lần" **không** phải viết gọn hơn —
nó là bỏ hết những chỗ M3 *có thể* phải hỏi lại.

## 1. Vì sao `IR-06` một mình không chốt được một vòng

`IR-06 §10` là checklist ~40 ô. Đọc kỹ thì nó trộn **ba loại yêu cầu khác hẳn nhau**:

| Loại | Ví dụ | Trả lời trong một vòng được không |
| --- | --- | --- |
| **Quyết định** | chọn A hay B | **Được** |
| **Xác nhận** | "xác nhận M3 là owner duy nhất quyết đơn nào cần gọi" | **Được** |
| **Giao nộp** | *"chạy shared E2E exact SHA"*, *"giao producer commit/OpenAPI/schema/CDC"* | **Không** — đó là một dự án, không phải một câu trả lời |

Trộn ba loại thì **không thể** chốt một vòng: một nửa danh sách là việc phải làm xong mới trả lời
được. Tệ hơn, `§10` còn trỏ M3 sang **ba tài liệu khác** để ký `ATP-01..15`, `DTK-01..15`,
`RVK-01..12` — **42 quyết định con** nằm ngoài trang họ đang đọc.

Và phần lớn trong số đó **đã được quyết rồi**: chính sách attempt đã ký `OD-V1-08`+`OD-V1-16`, đã
nằm trong code (`SignedProductionAttemptPolicies`, `gh-247-prod-v1`), có test giữ. Bảo M3 "ký
`ATP-01..15`" là bảo họ quyết lại thứ đã quyết.

## 2. `IR-07` — cơ chế làm cho một vòng là đủ

Phiếu mới `integration-requirements/07-module-3-decision-sheet.md`. Ba cơ chế, theo thứ tự quan
trọng:

1. **Mỗi mục đã có sẵn vị trí của IVR**, kèm lý do và hậu quả nếu chọn khác. M3 chỉ cần `ĐỒNG Ý`
   hoặc `KHÁC: <giá trị>`. Họ không phải soạn phương án — soạn phương án là thứ sinh ra vòng sau.
2. **Im lặng = đồng ý.** Không điền thì vị trí của IVR thành chốt. Điều khoản này chuyển chi phí của
   việc *không trả lời* về đúng chỗ, và là thứ duy nhất thật sự chặn được vòng hai.
3. **Tách bạch ba loại yêu cầu.** Phần A = đã chốt (chỉ để biết). Phần B = 21 câu phải trả lời.
   Phần D = việc M3 làm **sau khi ký**, không phải điều kiện để ký.

Cấu trúc: Phần 0 (bốn thứ cần lấy + ba breaking) · A (12 mục đã ký) · B (21 câu) · C (ba chuỗi
lệch) · D (việc M3) · E (việc IVR) · ô ký.

## 3. Contract phải sửa, vì nếu không thì chắc chắn có vòng hai

Trong lúc kiểm từng con số cho phiếu, phát hiện **ba header runtime vẫn dùng nhưng contract không
khai**:

| Header | Runtime | Contract trước `draft.25` |
| --- | --- | --- |
| `X-Action-Reason` | **Bắt buộc** ở cả 8 endpoint `danger` — `AdminAccessOptions.HasDangerEvidence` từ chối nếu thiếu | **không khai ở đâu cả**; chỉ nằm trong phần mô tả security scheme |
| `X-Script-Permissions` | Đọc để cấp quyền script; thiếu = không quyền nào, mọi thao tác script bị từ chối | không khai |
| `X-Destination-Ref` | Provenance tuỳ chọn cho kill switch | không khai |

Hậu quả: **client M3 sinh từ `draft.24` không gửi `X-Action-Reason`, và bị từ chối ở cả 8 route
`danger`** — mà không có cách nào biết vì sao, vì yêu cầu ấy nằm trong văn xuôi chứ không trên
operation.

Gửi cho M3 một contract mà client sinh ra không gọi được một phần ba bề mặt quản trị thì **cầm chắc
vòng hai**. Nên `draft.25` khai cả ba.

`oasdiff`: **8 errors, 0 warnings**, tất cả `new-required-request-parameter` — đúng 8 endpoint
`danger`. Phân loại đúng, nhưng đây là loại breaking hiếm: nó **sửa** client chứ không phá, vì client
cũ vốn đã bị từ chối. Sinh lại từ `draft.25` là xong.

## 4. Bốn con số tôi viết sai và đã sửa trước khi phát

Phiếu này chỉ có giá trị nếu **mọi con số trong nó đúng**. Một con số sai là một vòng hai. Tôi viết
bản đầu rồi kiểm lại từng cái với code/spec, và **bốn** cái sai:

| Tôi viết | Thực tế | Nguồn |
| --- | --- | --- |
| `items[]` tối đa **5** | **100** | `ServiceCollectionExtensions.cs:85` |
| `public_name` ≤ **60** ký tự | **160** | OpenAPI `OrderSpeechItem` |
| *"API đã bắt buộc `X-Action-Reason`"* | Runtime bắt buộc, **contract thì không** | §3 ở trên |
| **9** đặc tả màn hình | **8** + 1 index | `specs/ui/` |

Hai cái đầu tôi bịa từ trực giác "danh sách nên ngắn". Trực giác ấy **đúng** — 100 dòng lọt
validation nhưng cuộc gọi sẽ dài hơn cửa sổ xác nhận — nhưng đó là **khuyến nghị**, không phải giới
hạn đang thực thi, và phiếu phải nói rõ cái nào là cái nào. Nay `M3-06` ghi cả hai: giới hạn kỹ
thuật **100/160**, khuyến nghị thực tế **≤ 5**, kèm lý do vì sao khuyến nghị thấp hơn.

## 5. Một lỗ trong chính gate tôi vừa dựng

`FREEZE-03` của `W-0254` ghim con trỏ phiên bản trong `IR-06`. Lượt này nó bắt được hai con trỏ
lệch ngay khi bump `draft.25` — đúng việc của nó.

Nhưng nó **bỏ sót một con trỏ thứ ba**: anchor khớp `bản hiện hành`, còn dòng tôi thêm ở `§0` viết
`Contract hiện hành`. Hai cách viết cho cùng một con trỏ, và gate chỉ biết một. Đã thu anchor về
`hiện hành` để bắt mọi cách viết, và thử lại bằng đột biến để chắc nó đỏ.

## 6. Va chạm với luồng khác — **không** commit phần của họ

Giữa lượt này, một luồng khác đang refactor `deploy/ci/scripts/`: tách
`rejectDuplicateJsonKeys` sang `json-shape-lib.mjs` (tệp mới, chưa track) qua ~7 validator, cộng
thêm ~30 test.

Phân loại từng tệp đã sửa:

| Nhóm | Số tệp | Xử lý |
| --- | ---: | --- |
| Chỉ của tôi (re-pin hash) | 2 | commit |
| Chỉ của họ (refactor) | 6 | **không đụng** |
| Lẫn cả hai | 2 | **không commit** — gỡ phần tôi ra khỏi phần họ trong cùng tệp là cách chắc chắn tạo ra rối |

Vì thế `gate-sweep` hiện **đỏ ở 4 validator**, và đó là pin trên `IR-06`/`target-v1-shared-e2e`
đang chờ refactor của luồng kia hạ cánh. **Không phải lỗi của lượt này**, và cũng không sửa được
sạch sẽ cho tới lúc đó.

## 7. Kiểm chứng

```text
dotnet test Ivr.sln                    982/982 PASS, 0 failed, 0 skipped
                                       (952 → 982; +30 là test của luồng kia)
CONTRACT_FREEZE=PASS                   FREEZE-03 bắt đúng 2 con trỏ lệch khi bump
CONTRACT_FREEZE_SELFTEST=PASS          15/15
validate-openapi.mjs                   OPENAPI_FILES_VALID=2 · SCHEMA_NEGATIVE_REJECTED=13
docs-selftest.mjs                      API_DOCS_SELFTEST_PASS · 17 trang
oasdiff draft.24 → draft.25            8 errors / 0 warnings, đều new-required-request-parameter
8/8 endpoint danger                    khai X-Action-Reason (kiểm bằng script, không đọc mắt)
IR-06                                  15/15 đường dẫn tồn tại
gate-sweep                             ĐỎ 4 validator — pin của luồng kia, xem §6
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1` vẫn `DRAFT`.

## 8. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| — | Gửi `IR-07` cho dev M3; nhận lại bản đã điền + chữ ký | **owner** |
| — | Re-pin `IR-06` trong `opt-out` + `upstream` validator sau khi refactor luồng kia hạ cánh | tôi |
| — | Chọn đường cho `IR-06 §9a` (quorum ba mục treo) | **owner** |
| — | Mở endpoint thu hồi theo shape M3 chốt ở `M3-14` | tôi |
