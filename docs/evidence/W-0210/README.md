# W-0210 — Phím hủy đơn không phải lệnh cấm liên hệ

Ngày: 2026-09-07 · Baseline: `main@0bd1a2f` · Trạng thái: **TESTS_PASS** cho phần sửa và ghim;
`OD-V1-23` vẫn **`QUORUM_PENDING`**.

## 1. Vấn đề

`OD-V1-23` được ký `2026-09-06` như **vị trí của owner** (chưa đủ quorum), chọn *explicit-only V1*:
chỉ coi là opt-out khi khách phát tín hiệu tường minh — và nêu ví dụ **"(DTMF-0 / handoff)"**.

Nguyên tắc thì đúng. Hai ví dụ thì sai cả hai:

| Tín hiệu được nêu | Thực tế |
| --- | --- |
| `DTMF-0` | **phím hủy đơn** |
| `handoff` / phím 9 | **ngoài scope**, và bị policy lời thoại từ chối |

### 1.1. Khách được nghe gì

Lời thoại khóa cứng, một câu duy nhất được phép:

> *"… Bấm phím 1 để xác nhận đơn hàng, **bấm phím 0 để hủy**."*

`TargetV1SpeechPolicy.ValidateTemplate` bắt buộc template phải giữ đúng cặp chỉ dẫn `1`/`0` này, và
`DispositionMapper` map `DTMF "0"` → `IvrCustomerCancelled` + `RevalidateAndCancelCustomerRequest`,
reason `CUSTOMER_PRESSED_0`.

Không có câu nào nói với khách rằng bấm 0 sẽ ảnh hưởng tới việc có được gọi lần sau hay không.

### 1.2. Phím 9 còn không tồn tại

`specs/01-context-and-scope.md:34` để *"key 9 / human handoff"* **ngoài scope trừ khi có contract
mới được duyệt**. Và `TargetV1SpeechPolicy.ValidateTemplate` **ném `InvalidOperationException`** với
bất kỳ template nào chứa chữ "phím 9". Nên tín hiệu thứ hai mà `OD-V1-23` nêu không những chưa có —
hệ thống đang **chủ động từ chối** đưa nó vào lời thoại.

### 1.3. Chính gói được dẫn làm bằng chứng lại nói ngược

Cột *Closure evidence* của `OD-V1-23` dẫn
[M8-08](../../../plan/ivr-orther/m8-08-opt-out-suppression-decision-pack-2026-09-03.md). M8-08 §4:

> 2. DTMF `1` là xác nhận đơn, DTMF `0` là yêu cầu huỷ đơn. **Không tái dùng hai phím này cho
>    opt-out.**
> 3. Current V1 **không có explicit opt-out signal**. Muốn thêm phải có wording/script, signal
>    source, proof và Legal/Privacy approval riêng.

Tức quyết định và bằng chứng của nó mâu thuẫn nhau, cách nhau ba ngày.

## 2. Vì sao đây không phải một lỗi tài liệu bình thường

Mọi mục khác trong nhóm 0 hỏng theo kiểu *M3 gửi sai → request lỗi → có người thấy*. Mục này hỏng
im lặng và hỏng về phía khách hàng: nếu CRM/M3 build theo register, một khách **hủy một đơn** sẽ bị
ghi là **đã yêu cầu ngừng liên hệ**, vĩnh viễn, dựa trên một thao tác mà lời thoại mô tả là "hủy".

Đó là suy diễn consent — đúng thứ mà chính `OD-V1-23` ra đời để cấm.

Điểm cộng cho thiết kế hiện tại: `OptOutSuppressionTests` đã tồn tại từ `W-0034` với câu hỏi khung
*"can a customer end up suppressed without having asked to be?"*, `SuppressionDecision
.SuppressedLocally` là `false`, và `OptOutSuppressionPolicy` **không có caller nào** trong runtime.
Nghĩa là lỗi này mới ở tầng tài liệu, **chưa** vào code — và mục đích của lượt này là giữ nguyên như
vậy.

## 3. Đã làm gì

**Không sửa một dòng runtime nào**, và **không đụng vào quyết định của owner.**
"Explicit-only V1" là vị trí đúng; chỉ hai ví dụ tín hiệu là sai.

### 3.1. Ghim — `UT-OPTOUT-DTMF0-05`

`tests/Ivr.UnitTests/Policies/OptOutSuppressionTests.cs`, đặt cạnh các test cùng chủ đề:

| Khẳng định | |
| --- | --- |
| `DTMF "0"` → `IvrCustomerCancelled` | phím 0 là hủy đơn |
| → `RevalidateAndCancelCustomerRequest` | và chỉ là hủy đơn |
| reason **không phải** `REJECTED_REVIEW_REQUIRED` | không phải cả tín hiệu opt-out yếu |
| `HumanReviewRequired == false` | không vào hàng đợi review |
| lời thoại chứa `"bấm phím 0 để hủy"` | đúng cái khách được nghe |
| template có "phím 9" → `InvalidOperationException` | tín hiệu thứ hai bị từ chối, không chỉ là thiếu |

### 3.2. Sửa register

`specs/_review/open-decisions-register.md` dòng `OD-V1-23` — thêm `Sửa 2026-09-07 (W-0210)`:
hai ví dụ tín hiệu sai, nguyên tắc explicit-only giữ, V1 hiện **không có** tín hiệu opt-out tường
minh nào, và thêm một cái cần wording/script + signal source + proof + Legal/Privacy ký riêng.

**Không đổi trạng thái dòng.** `OWNER_POSITION_SIGNED / QUORUM_PENDING` là mô tả đúng, và tách
trạng thái là việc của chief auditor (mục A2).

## 4. Kiểm chứng

```text
dotnet test Ivr.sln                        894/894 PASS, 0 failed, 0 skipped
                                           (Unit 594 · Integration 268 · Contract 24 · Chaos 8)
dotnet test --filter UT-OPTOUT-DTMF0-05    1/1 PASS
contract-freeze-verifier.mjs               CONTRACT_FREEZE=PASS
generate-test-traceability.mjs --check     TEST_TRACEABILITY_CURRENT=553  (552→553)
docs-selftest.mjs                          API_DOCS_SELFTEST_PASS
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1=DRAFT`.

## 5. Còn lại — thuộc quorum, không thuộc M8

1. **Product + CRM/M3 + Legal/Privacy đồng ký `OD-V1-23`.** Vị trí owner đã có; quorum thì chưa.
2. **Nếu V1 cần một tín hiệu opt-out thật:** phải có wording mới trong lời thoại (nghĩa là sửa
   `TargetV1SpeechPolicy` và duyệt lại script), signal source, proof, và Legal/Privacy ký riêng.
   Không có phím trống nào trong lời thoại hiện tại: `1` và `0` đã có nghĩa, `9` bị cấm.
3. **Hằng số `2/3`** giữ `TEST_ONLY_CANDIDATE`, không wire trước chữ ký (M8-08 §4.7). Đây là khoảng
   trống, không phải quy tắc.
4. `OPT-01..11` vẫn chưa đủ quorum; `UT-ARCH-NO-CRM-EGRESS-06` tiếp tục khóa boundary IVR↛CRM.
