# W-0223 — Hoãn calibration, và hoãn cho đúng

Ngày: 2026-09-07 · Baseline: `main@1804241` · Trạng thái: **TESTS_PASS**.

Quyết định của owner cho mục `1.3`: **giữ `UNCALIBRATED`, chờ `W-0008`**.

## 1. Quyết định giữ nguyên trạng, không phải quán tính

Trạng thái sau lượt này y hệt trước:

```text
CAP-CALIB-03  PASS_UNCALIBRATED
CAP-DRIFT-05  PASS_DECLARED_DISAGREEMENT — model 40s, spec 35s, spec cycle 50s, runtime 60s
CAPACITY_SELFTEST_PASS_UNCALIBRATED
EXTERNAL_INTAKE_DEFERRED_BY_OWNER   (W-0189, không đụng)
```

Khác biệt là giờ nó **được quyết** thay vì mặc định. `PT-CAP-02` (M8-P0-009) đã có từ 29/08 — đừng
giao lại như việc chưa làm.

## 2. Nhưng checklist thoát vẫn nói "ba con số"

Nếu chỉ ghi lại quyết định rồi dừng thì lượt này rỗng. Đi kiểm thì ra một chỗ hỏng thật.

`W-0212` khai báo con số thứ tư — `specAverageCallSeconds: 35`, con số occupancy của **chính spec** —
và ghim nó trong `CAP-DRIFT-05`. Nhưng nó **không sửa** đoạn hướng dẫn ngay phía trên:

> *When W-0008 produces measurements: set model/runtime to measured channel occupancy, set the spec
> full-cycle value to occupancy + measured cooldown … it deliberately does NOT require all **three**
> numbers to be equal*

Ai làm `W-0008` sẽ đọc đúng đoạn đó, calibrate ba số, **bỏ lại số thứ tư** — tái tạo chính xác thiếu
sót mà `W-0212` vừa vá. Rồi `CAP-DRIFT-05` đỏ vì `35` không còn khớp, ở một chỗ trông như bí ẩn thay
vì như một lời nhắc.

Hoãn một việc thì được. Hoãn nó với một checklist sẽ dẫn người sau đi sai thì không.

## 3. Đã sửa

### 3.1. Checklist thoát — bốn số, nói rõ số nào lấy gì

```text
modelCallSeconds        = measured channel occupancy
schedulerDefaultSeconds = the same measurement, and SchedulerCapacity.cs with it
specAverageCallSeconds  = the same measurement -- the spec's own occupancy figure, and
                          the one a three-number calibration forgets
specConservativeSeconds = occupancy + measured cooldown
```

### 3.2. Đường calibrated nay từ chối bỏ quên số thứ tư

`assertCalibratedDurationSemantics` thêm một assertion: khi `calibrated`, `specAverageCallSeconds`
phải bằng `modelCallSeconds`. Kèm một mutation `TEST_ONLY` chứng minh nó **bị từ chối** khi ba số
được calibrate còn số thứ tư giữ nguyên `35`.

Chỗ này đáng nói vì nó **ngược** với quyết định của `W-0212`, và có lý do:

| | uncalibrated | calibrated |
| --- | --- | --- |
| có phép đo? | **không** | **có** |
| quan hệ giữa `35` và `50` | **không ép** — chỉ ghim | ép: cả hai dẫn lại từ phép đo |

Uncalibrated mà ép một công thức là **đóng băng phỏng đoán thành luật** — đúng điều `W-0212` từ
chối làm. Calibrated thì cả bốn cùng mô tả **một cuộc gọi đã đo**, nên ép là đúng, y như assertion
`schedulerDefaultSeconds == modelCallSeconds` vốn đã có.

### 3.3. Và đó là thứ đóng được mười giây

Spec công bố occupancy `35s`, cycle `50s`, cooldown `5s` — mà `35 + 5 = 40`. **Không ai nói được số
nào sai nếu không có phép đo.**

Calibration không phân xử. Nó **dẫn lại cả hai số của spec từ phép đo**: occupancy thành giá trị đo
được, cycle thành giá trị đó cộng cooldown đo được. Khoảng chênh biến mất vì cả hai đầu được viết
lại, không phải vì ai đó chọn bên.

Nay điều đó được **cưỡng chế** chứ không chỉ được mô tả trong `docs/capacity-model.md`.

## 4. Kiểm chứng

```text
capacity-selftest.mjs
  CAP-MODEL-01    PASS — peak sizing 21 channels        (không đổi)
  CAP-SENS-02     PASS — 27 corners, 7..72 channels      (không đổi)
  CAP-CALIB-03    PASS_UNCALIBRATED
  CAP-DRIFT-05    PASS_DECLARED_DISAGREEMENT — 40s / 35s / 50s / 60s
  CAP-SESSION-06  PASS_UNANSWERED
  CAPACITY_SELFTEST_PASS_UNCALIBRATED

dotnet test Ivr.sln                      900/900 PASS, 0 failed, 0 skipped
gate-status.mjs                          GATE_STATUS_PASS — 221 work items
contract-freeze-verifier.mjs             CONTRACT_FREEZE=PASS
generate-test-traceability.mjs --check   TEST_TRACEABILITY_CURRENT=557 (không đổi)
docs-selftest.mjs                        API_DOCS_SELFTEST_PASS
```

Hai bất biến hành vi (`21` kênh, `27` corner / `7..72`) **giống hệt trước** — bằng chứng số học
không đổi. Không sửa runtime C#, không thêm/xoá test .NET, không đổi một giá trị giả định nào.

## 5. Còn lại

Không có gì cho M8 tới khi `W-0008` có cuộc gọi đo được. Khi đó, checklist trong
`capacity-model.mjs` là thứ phải theo, và `CAP-DRIFT-05` sẽ từ chối nếu theo thiếu.

Điều kiện thoát vẫn nguyên: `calibratedBy` phải trỏ một artifact có thật dưới
`docs/evidence/W-0008/`, và `CAP-CALIB-03` vẫn cấm dán nhãn `P5-3` thành bằng chứng cuộc gọi.
