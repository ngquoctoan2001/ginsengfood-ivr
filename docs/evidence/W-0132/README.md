# W-0132 — Single declared source cho call-duration assumption

Ngày: `2026-08-28`
Baseline: `main@b4d8903`, worktree bẩn (candidate W-0128/W-0129/W-0131 + procurement/TTS WIP chưa commit)
Trạng thái: `TESTS_PASS`
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Mục tiêu và giới hạn

B1 sub-task 1 (calibrate capacity model) chờ số đo W-0008 và **không** làm được bây giờ. Nhưng nó
có một mẩu tách được: ba con số chu kỳ cuộc gọi đang sống độc lập ở ba nơi mà không gate nào so
chúng với nhau — bất kỳ con nào nhúc nhích một mình cũng không ai biết.

W-0132 gom chúng về một nguồn khai báo và dựng gate drift. **Không đổi một giá trị nào**, không
calibrate, không sửa production.

## 2. Hiện trạng trước W-0132

| Con số | Nơi | Ý nghĩa |
| --- | --- | --- |
| 40s + 5s cooldown = **45s** | `capacity-model.mjs:188`, `capacity-selftest.mjs:61,68,71,81,87,91` | giả định của mô hình |
| **50s** | hàm ý bởi spec §23 `M8-P0-009` (32 SIM × `floor(300/50)` = 192) | conservative của spec |
| **60s** | `SchedulerCapacity.cs:29` `ExpectedCallDurationSeconds` | mặc định runtime |

`callSeconds: 40` được hardcode ở **6 chỗ**. `cooldownSeconds: 5` thì đã single-source sẵn trong
`CHANNEL_CONSTRAINTS` và đã có assert — chỉ `callSeconds` là chưa.

## 3. Đã làm

1. Thêm `CALL_DURATION_ASSUMPTIONS` vào `tools/capacity-sim/capacity-model.mjs` — khai báo cả ba
   con số, cờ `calibrated: false`, và `calibrationWork: "W-0008"`.
2. Thay 6 literal `callSeconds: 40` bằng `CALL_DURATION_ASSUMPTIONS.modelCallSeconds`.
3. Sweep độ nhạy `[25, 40, 70]` thành `SENSITIVITY_CALL_SECONDS` với corner giữa lấy từ nguồn khai
   báo, để sweep không xoay quanh một giá trị cũ.
4. Gate mới `CAP-DRIFT-05` trong `deploy/ci/scripts/capacity-selftest.mjs`.
5. `docs/capacity-model.md` §4a ghi lại ba con số và luật của gate.

### Vì sao không hợp nhất ba con số

Hợp nhất nghĩa là tuyên bố một thời lượng cuộc gọi **đã đo**, mà chưa có cuộc gọi nào được quay.
Đó đúng là thứ dự án vẫn từ chối làm, và cũng là thứ `CAPACITY_SELFTEST_PASS_UNCALIBRATED` đang nói.
W-0132 chỉ biến bất đồng từ **tình cờ** thành **được khai báo**.

## 4. `CAP-DRIFT-05` kiểm gì

- Ba con số phải đúng bằng giá trị đã ghim — nhúc nhích một mình là đỏ.
- `~192` của spec phải còn khớp số học với 50s.
- Mặc định C# được **đọc ngược từ `SchedulerCapacity.cs`** bằng regex, không tin bản sao trong JS.
- Sweep độ nhạy phải còn chứa giả định hiện hành.
- **Cửa thoát có khóa**: nếu ba con số được làm cho bằng nhau mà `calibrated` vẫn `false` → đỏ.

Khi W-0008 có số đo: đặt giá trị, bật `calibrated`, trỏ `calibratedBy` vào evidence — gate khi đó
sẽ **đòi** ba con số thống nhất.

## 5. Mutation proof

| Mutation | Kết quả |
| --- | --- |
| `ExpectedCallDurationSeconds` 60 → 55 **trong C#** | `FAIL` — *"SchedulerCapacity.cs defaults ... to 55s while CALL_DURATION_ASSUMPTIONS declares 60s. One of them moved alone"* |
| Gộp model+scheduler về 50, giữ `calibrated:false` | `FAIL` — *"must be a deliberate update to CALL_DURATION_ASSUMPTIONS with evidence"* |
| Sửa **cả giá trị, cả pin, cả C#** về 50, giữ `calibrated:false` | `FAIL` — *"Agreeing on an unmeasured number is how an assumption becomes a fact by accident"* |

Mutation 1 chứng minh việc đọc chéo file thật sự có tác dụng. Mutation 3 chứng minh lớp phòng thủ
cuối chặn được người cố tình dọn dẹp cho gọn. Cả ba đã revert; `git diff` trên
`SchedulerCapacity.cs` trống.

## 6. Verification

| Gate | Kết quả |
| --- | --- |
| `capacity-selftest.mjs` | `CAPACITY_SELFTEST_PASS_UNCALIBRATED`; `CAP-DRIFT-05 PASS_DECLARED_DISAGREEMENT` |
| Bất biến hành vi | `CAP-MODEL-01` vẫn `21` kênh; `CAP-SENS-02` vẫn `27` corner / `7..72` — y hệt trước refactor |
| `CAP-CALIB-03` | `PASS_UNCALIBRATED` (doc mới không phá assert cũ) |
| `ci-config-selftest.mjs` | `CI_CONFIG_SELFTEST_PASS` |
| GitNexus impact (trước sửa) | `UNCALIBRATED_SCENARIO` LOW 0/0; `CHANNEL_CONSTRAINTS` LOW 0/0 |
| File chạm | 3 (`capacity-model.mjs` +34/−1, `capacity-selftest.mjs` +86/−7, `capacity-model.md` +26/−0) |
| Production C# | `0 file` — `SchedulerCapacity.cs` không đổi |

## 7. Residual gate

- `NOT_CALIBRATED`: W-0132 **chỉ khóa drift**, không calibrate gì. Model vẫn `UNCALIBRATED` và vẫn
  không được dùng để chốt mua.
- `OWNER_DATA_REQUIRED`: volume `800-1200` vẫn chưa được Owner xác nhận, và câu hỏi hiện đang bị
  đặt sai — selftest chạy `dailyOrders: 800/1_200` (**đơn/ngày**) trong khi spec nói
  **cuộc/phiên**, và chính spec tự khai độ dài một phiên Giờ Vàng *chưa được chốt ở đâu*. Không quy
  đổi ra số kênh được cho tới khi Owner chốt cả đơn vị lẫn độ dài phiên. Đây là B1 sub-task 2, vẫn
  mở.
- B1 sub-task 1 chỉ đóng được sau W-0008.
