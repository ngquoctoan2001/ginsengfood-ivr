# W-0131 — Exact M8-P0-009 capacity overload acceptance evidence

Ngày: `2026-08-28`
Baseline: `main@b4d8903`, worktree bẩn (candidate W-0128/W-0129 + procurement/TTS WIP chưa commit)
Trạng thái: `TESTS_PASS`
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Mục tiêu và giới hạn

Cross-audit `docs/review/2026-08-28-m8-worklist-cross-audit.md` F-08 ghi: *"Exact acceptance
`32 channels / 800 jobs / 5 minutes → incident` vẫn chưa có."* W-0131 dựng đúng acceptance đó và
không làm gì khác.

W-0131 là **test-only, additive**. Không sửa một dòng production nào. Không calibrate capacity
model, không chốt số kênh mua, không đổi `PT-CAP-01`.

## 2. Khoảng trống thật trước W-0131

`PT-CAP-01` (`OverCapacityHoldsJobsWithoutLosingOneAndNeverDoubleBooksAChannel`) chạy hai shape
`[1 kênh/8 job]` và `[4 kênh/24 job]`. Nó chứng minh ba điều: không double-book, không mất job,
không tiêu attempt của khách.

Nó **không assert capacity incident nào**. Đây đúng là counter dùng để size đơn mua SIM (M8-OD-A),
nên một shortage im lặng sẽ đọc thành "đủ kênh".

## 3. Test đã thêm

| Test ID | Chứng minh | Kết quả |
| --- | --- | --- |
| `PT-CAP-02` | 32 kênh / 800 job / window 5 phút: ≤32 lease, không double-book, 800 job còn nguyên, **768 capacity incident** `SCHEDULER_DEADLINE` + `NO_DISPATCH_BEFORE_DEADLINE` + `IVR_CAPACITY_EXCEPTION`, 0 counted customer attempt | `1/1 PASS` (11s) |

`DeliberateOverloadRecordsCapacityIncidentForEveryJobThatNeverGotAChannel`,
`tests/Ivr.IntegrationTests/SchedulerPersistenceTests.cs`.

Hai helper seed bulk (`SeedChannelPoolAsync`, `SeedReadyJobBurstAsync`) được thêm riêng thay vì sửa
`SeedReadyJobAsync`/`SeedChannelAsync` — hai helper cũ mở một DbContext + một SaveChanges mỗi dòng,
đúng cho vài dòng và sai cho 832 dòng, nhưng hơn hai mươi test khác đang phụ thuộc chúng.

Sweep chạy vòng lặp vì `ValidateBatchSize` chặn `batchSize > 512`; drain tới khi sweep trả 0 chính
là nửa "không batch" của M8-P0-009.

## 4. Mutation proof — gate có răng

Dự án đã từng bị bắt lỗi gate không thể đỏ (W-0126 F2), nên assertion chính được kiểm bằng mutation:

| Mutation | Kết quả |
| --- | --- |
| `expectedMisses = (jobs - channels) - 1` | `FAIL` — `Assert.Equal() Failure: Expected 767, Actual 768` |

Test thật sự quan sát 768 incident do runtime ghi, không phải hằng số tự khớp. Mutation đã revert,
build lại `0 Error`.

Ghi chú trung thực: lượt mutation đầu (`Assert.Equal(0, incidents.Count)`) build lỗi nên
`--no-build` chạy trên binary cũ và báo `Passed!` — kết quả đó vô nghĩa và **không** được dùng làm
bằng chứng. Mutation ở bảng trên là lượt đã build sạch.

## 5. Verification

| Gate | Kết quả |
| --- | --- |
| Build `Ivr.IntegrationTests` | `0 Warning, 0 Error` |
| `PT-CAP-02` | `1/1 PASS`, 11s |
| Full integration suite | `233/233 PASS`, 3m42s |
| `UT-TRACE-01` | `PASS` |
| GitNexus impact (trước sửa) | `TryClaimDueDispatchAsync` **HIGH** 19/16; `CloseMissedDeadlinesAsync` MEDIUM 11/8 — đã cảnh báo owner; **không symbol nào bị sửa** |
| GitNexus detect_changes | W-0131 đóng góp `8` symbol, toàn bộ trong `SchedulerPersistenceTests.cs`, `0` symbol production |

`detect_changes` tổng worktree trả `critical` / 492 symbol / 85 file. Con số đó thuộc về worktree
bẩn dùng chung (W-0128 + W-0129 + procurement/TTS WIP), **không quy cho W-0131**.

## 6. Phát hiện ngoài scope — traceability doc trên đĩa đang lệch 10 test

Khi regenerate `docs/traceability-tests.md`, script quét ra 10 test chưa có trong file:

| Nguồn | Test | Số |
| --- | --- | --- |
| W-0128 | `CT-API-ADMIN-PARITY-01`, `SEC-ADMIN-ROT-01..05` | 6 |
| W-0129 | `IT-INTAKE-REASON-WIRE-15`, `UT-INTAKE-REASON-COMPAT-14`, `UT-INTAKE-REASON-TAXONOMY-13` | 3 |
| **W-0131** | **`PT-CAP-02`** | **1** |

`HEAD@b4d8903` ghi `456`; file working tree trước lượt này vẫn `456`; sau regenerate là `466`.

Cộng dồn khớp chính xác: `456 + 6 = 462` — đúng con số `docs/evidence/W-0128/README.md` §12.2 tự
khai (`regenerate/check 462 tagged test PASS`). Tức **W-0128 đã tính đúng nhưng artifact chưa bao
giờ được ghi xuống đĩa**; W-0129 sau đó thêm 3 test mà không regenerate.

W-0131 buộc phải regenerate vì `UT-TRACE-01` đỏ nếu doc lệch suite. Ghi rõ ở đây để không ai đọc
`466` thành "W-0131 thêm 10 test": **W-0131 đóng góp đúng 1**. Việc đóng lại evidence traceability
của W-0128/W-0129 thuộc về hai work item đó, không phải W-0131.

## 7. Residual gate

- `OWNER_DATA_REQUIRED`: volume `800-1200 cuộc/phiên` vẫn là giả định, chưa được Owner xác nhận.
- `NOT_CALIBRATED`: `PT-CAP-02` **không** model kênh quay vòng giữa các cuộc, nên **không** chứng
  minh con số năng lực `~192/5 phút`. `ExpectedCallDurationSeconds` (60s), `capacity-selftest`
  (45s) và spec conservative (50s) vẫn là ba con số ở ba nơi — chỉ đóng được sau khi W-0008 có cuộc
  gọi đo được.
- `BLOCKED_EXTERNAL`: chưa mua SIM gateway (DT-01), nên không có measured capacity trên phần cứng
  thật.
- Test này **không** là căn cứ để chốt mua 4 kênh hay 32 kênh. F-08 vẫn mở ở phần procurement.
