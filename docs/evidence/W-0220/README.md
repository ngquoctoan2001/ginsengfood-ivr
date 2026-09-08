# W-0220 — Owner chốt khung giờ, và ba mươi giây không biểu diễn được

Ngày: 2026-09-07 · Baseline: `main@e391ca8` · Trạng thái: **TESTS_PASS**.

Quyết định của owner, không phải phát hiện của tôi. `W-0215` đo ra mốc cắt và đưa hai lựa chọn;
owner chọn **nới `End` sang `21:07:30`**.

## 1. Vì sao đặt `21:08` chứ không phải `21:07:30`

`CallingWindowOptions.EndMinuteOfLocalDay` là **một số phút**, và `CallingWindow.Evaluate` tính
`minuteOfDay = hour*60 + minute` — **bỏ phần giây**. Nên `21:07:30` không có chỗ để ghi.

Câu hỏi thật không phải "làm tròn lên hay xuống" mà "làm tròn xuống thì có còn là quyết định của
owner nữa không". Chạy thử cả hai:

```text
End=1267 (21:07)  →  tại 21:07:30 minuteOfDay=1267  open=false
End=1268 (21:08)  →  tại 21:07:30 minuteOfDay=1267  open=true
```

`21:07` **trông như tuân lệnh và làm quyết định không xảy ra**: đúng khoảnh khắc cần mở thì gate
đóng. `21:08` là giá trị nhỏ nhất thi hành được ý owner, và nó đắt thêm 30 giây.

Lựa chọn còn lại — đổi trường sang đơn vị giây — sửa contract config, validator và mọi nơi khai
giá trị, để đổi lấy 30 giây. Tôi không tự làm; nếu owner cần đúng từng giây thì đó là một quyết
định riêng.

## 2. Con số này mua được gì

| Program | offset attempt 2 | cutoff trước | cutoff sau |
| --- | --- | ---: | ---: |
| `TWENTY_FOUR_SEVEN` | `450s` | 20:52:30 | **21:00:30** |
| `GOLDEN_HOUR` | `150s` | 20:57:30 | **21:05:30** |

Đơn 24/7 cuối cùng nhận **đúng lúc `21:00:00`** giờ đủ hai cuộc. Trước đây cuộc 2 của nó rơi vào
`21:07:30`, ngoài khung giờ, và khách chỉ được gọi một lần — rồi Sales nhận
`IVR_CONFIRMATION_WINDOW_EXPIRED` thay vì `IVR_NO_ANSWER_FINAL`, tức **cùng một hành vi khách hàng
dẫn Core đi hai đường khác nhau tuỳ giờ đặt đơn**.

Đó là thứ `W-0215` đo được và là thứ quyết định này đóng lại.

## 3. Test đã đỏ, và đó là chủ đích

`UT-SCH-WINDOW-09` được viết ở `W-0215` để **suy** mốc cắt từ `SignedProductionAttemptPolicies` +
`CallingWindowOptions` thay vì gõ tay, kèm câu: *"đổi policy hoặc window thì test đỏ và có người
phải nhìn"*.

Đổi `End` xong nó đỏ ngay. Đã sửa **có chủ đích**, không phải sửa cho xanh: cập nhật hai mốc cắt và
**thêm một khẳng định mới** — đơn 24/7 nhận lúc `21:00:00` nay đủ cả hai cuộc, chính là điều
`W-0220` mua.

`UT-SCH-WINDOW-01` (theory biên) cũng đổi: `21:00` từ `false` thành `true`, thêm `21:07` (mở) và
`21:08` (đóng). `UT-SCH-WINDOW-06` nay khẳng định default bằng `21:08` **và** giải thích tám phút
đó là gì, để ai "dọn" nó về `21:00` sẽ bị `UT-SCH-WINDOW-09` chỉ vào đúng quyết định họ vừa gỡ.

## 4. Đã sửa ở đâu

| | |
| --- | --- |
| `CallingWindow.cs:43` | `21 * 60` → `(21 * 60) + 8`, kèm lý do đầy đủ trong doc |
| `CallingWindowTests.cs` | helper default · `UT-SCH-WINDOW-01` biên · `-06` default · `-09` mốc cắt + khẳng định mới |
| `integration-requirements/06-module-3-api-handover.md` `§3.4.2` | hai mốc cắt mới + ghi rõ vì sao `21:08` |
| `specs/_review/open-decisions-register.md` | correction trên `OD-V1-16` (2 chỗ) |
| `plan/toan-viec-can-lam-m8-2026-09-07.md` | mục `1.1` → đã chốt và thi hành |

**Không đụng** `docker-compose.e2e.yml` (đã là `0..1440` từ `W-0214`) và không có helm/deploy nào
khai giá trị này — mặc định trong code là nguồn duy nhất.

## 5. Kiểm chứng

```text
gitnexus impact CallingWindowOptions     MEDIUM · 12 impacted · 0 process · module Scheduling
dotnet test --filter UT-SCH-WINDOW       18/18 PASS
dotnet test Ivr.sln                      899/899 PASS, 0 failed, 0 skipped
                                         (897→899: hai InlineData mới trên UT-SCH-WINDOW-01)
gate-status.mjs                          GATE_STATUS_PASS — 218 work items
contract-freeze-verifier.mjs             CONTRACT_FREEZE=PASS
generate-test-traceability.mjs --check   TEST_TRACEABILITY_CURRENT=556 (không đổi)
docs-selftest.mjs                        API_DOCS_SELFTEST_PASS
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1=DRAFT`. Đây là lượt **đầu tiên** trong chuỗi
`W-0208`→`W-0220` đổi một giá trị runtime — vì đây là lượt đầu tiên có một quyết định để thi hành.

## 6. Còn lại

- **M3 cần biết**: cutoff phát task đổi sang `21:00:30` (24/7) / `21:05:30` (GH). Đã công bố ở
  IR-06 `§3.4.2`; M3 xác nhận producer theo mốc mới.
- `OD-V1-16` ký `08:00–21:00`; giá trị chạy nay là `08:00–21:08`. Register đã có dòng correction,
  nhưng **trạng thái dòng vẫn `CLOSED`** — sửa trạng thái là việc chief auditor (mục `3.2`).
- Nếu owner muốn đúng `21:07:30` từng giây: đổi trường sang đơn vị giây, sửa validator + mọi nơi
  khai giá trị. Một quyết định riêng, không nằm trong lượt này.
