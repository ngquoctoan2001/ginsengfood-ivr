# W-0142 — M8-01 capacity calibration preflight và data-intake handoff

Ngày: `2026-08-29`

Baseline: `main@0baed74` + shared working-tree WIP

Trạng thái: `EVIDENCE_SUBMITTED / DATA_INTAKE_READY / CALIBRATION_NOT_RUN`

Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

Người ký phía Module 8: **Tôi — Module 8 / Project Owner**

Phạm vi chữ ký: xác nhận hiện trạng, bộ dữ liệu bắt buộc và stop rule; **không** ký thay Business,
M3, Product/Order Core, Infra/vendor hoặc chọn một con số capacity chưa đo.

## 1. Kết luận

M8-01 **chưa thể calibration**. Repo hiện không có `docs/evidence/W-0008/`, CDR/per-attempt timing
dataset được chấp nhận, arrival profile theo thời gian hoặc production attempt policy đã ký.

Kết quả đúng của lượt này là chuẩn bị đường vào có kiểm soát và sửa gate để sau này nhận dữ liệu
không tính cooldown hai lần. Model vẫn `UNCALIBRATED`; không đổi 40/50/60 giây, không dùng 2700
giây, không đổi `simPoolSize`, không chốt mua 4/12/32 kênh.

## 2. Sự thật đã đối chiếu

| Nguồn | Chứng minh | Không chứng minh |
| --- | --- | --- |
| `W-0008` | tracker ghi `BLOCKED_EXTERNAL`; R-02/R-03 vẫn OPEN | không có cuộc gọi thật hoặc measured capacity |
| `W-0131` / `PT-CAP-02` | 32 channel / 800 job / 5 phút tạo đúng 768 capacity incident, không mất job | không đo channel rotation, call duration hoặc throughput thiết bị |
| `W-0132` | khai báo và canh drift 40s model + 5s cooldown / 50s spec cycle / 60s runtime estimate | không chọn số nào là đúng |
| `W-0133` | chỉ ra `M8-OD-C` là owner của session/peak volume và model không tiêu thụ được câu trả lời | không đóng `M8-OD-C` hoặc `OD-19` |
| `W-0134` | giữ `sessionSeconds:null`, `arrivalProfile:null`; chặn thay thẳng 2700s | không cho phép giả định khách đến đều |
| selftest hiện hành | 6 check PASS; model peak 21; sensitivity 7..72; `PASS_UNCALIBRATED` | không phải căn cứ mua hoặc production readiness |

GitNexus query keyword bị suy giảm vì index thiếu FTS; không re-index shared repo. Context trực tiếp
vẫn xác nhận `poolForProgramme` có 3 caller, gọi `channelsForWindow`/`attemptsFor`, 0 execution
process. Impact trước sửa: `CALL_DURATION_ASSUMPTIONS` LOW 0/0/0; hai selftest function LOW,
mỗi function 1 caller trực tiếp và 0 process.

## 3. Phát hiện phải sửa ngay — calibrated path cũ sai nghĩa

Gate cũ bảo khi có W-0008 thì làm ba giá trị bằng nhau. Nhưng công thức là:

```text
full_cycle = callSeconds + cooldownSeconds
```

`callSeconds` và `SchedulerOptions.ExpectedCallDurationSeconds` cùng mô tả channel occupancy;
con số spec là full channel cycle. Làm cả ba bằng nhau rồi vẫn cộng cooldown sẽ tính cooldown hai
lần.

W-0142 đã sửa đường thoát, **không đổi giá trị hiện hành**:

- model/runtime phải cùng channel occupancy đã đo;
- chu kỳ spec phải bằng `occupancy + cooldown`;
- `calibratedBy` phải trỏ artifact tồn tại dưới `docs/evidence/W-0008/`;
- `CAP-CALIB-03` không cho relabel P5-3 thành bằng chứng cuộc gọi;
- một shape `TEST_ONLY` chạy mỗi selftest để đường calibrated không còn là nhánh chết.

## 4. Bộ dữ liệu bắt buộc

### 4.1. W-0008 — timing thật, PII-safe

Dùng [lab acceptance report §5](../../contracts/telephony-procurement-pack/lab-acceptance-report-template.md)
đã được W-0142 siết lại. Mỗi attempt phải có:

- run/attempt label, programme, carrier label, scenario và disposition;
- `started_at`, `ended_at`, `cooldown_until` hoặc thời điểm kênh available lại;
- channel occupancy và full channel cycle;
- CDR correlation ref nối được sang attempt, không chứa số điện thoại/token/recording;
- `N` cùng p50/p95/p99; thiếu mẫu phải ghi `INSUFFICIENT_SAMPLE`, không nội suy.

### 4.2. Business/M3 — volume có gốc thời gian

- định nghĩa session, timezone, start/end;
- eligible orders theo bucket đủ để tính mọi rolling window 5 phút cho GH và 15 phút cho 24/7;
- nguồn dữ liệu, khoảng thời gian, cách lọc và chữ ký owner;
- nếu chỉ giao tổng phiên mà không có arrival buckets/profile: **REJECT**.

### 4.3. Product/Order Core và pilot

- production attempt policy: window, max attempts, offsets;
- outcome/no-answer/retry distribution theo programme, kèm `N`;
- không lấy candidate mock-lab-v1 làm production policy.

### 4.4. Infra/vendor

- cooldown được duyệt, quota và concurrency thật;
- reserve/failure factor cho quarantine/failover;
- báo cáo nhiều kênh trên đúng số kênh thật; không nhân tuyến tính từ 1 SIM.

## 5. Quy trình resume M8-01

1. Validate đủ bốn nhóm §4, PII-safe và đúng owner ký.
2. Tái tính occupancy/full-cycle distribution từ dòng nguồn; không tin bảng tổng hợp đơn độc.
3. Chạy model theo p50/p95/p99 và toàn bộ rolling-window arrival profile; không giấu percentile.
4. Cập nhật `CALL_DURATION_ASSUMPTIONS`, `CHANNEL_CONSTRAINTS`, `SESSION_LENGTH`/arrival input và
   production attempt policy theo artifact đã ký.
5. Chạy `node deploy/ci/scripts/capacity-selftest.mjs`; kết quả bắt buộc chuyển thành
   `CAP-CALIB-03 PASS_CALIBRATED` và `CAP-DRIFT-05 PASS_CALIBRATED` với link W-0008 thật.
6. Chạy load/failover trên số kênh thật ở staging. Owner chỉ chốt số mua sau sensitivity + reserve,
   không từ một single-point output.

## 6. Stop rule

- Không dùng `45 phút`/`2700s`: spec tự khai đó là giả định không nguồn.
- Không làm 40/50/60 bằng nhau để “dọn drift”.
- Không dùng `PT-CAP-02`, simulator hoặc 1 SIM để tuyên bố 32 eSIM ready.
- Không dùng trung bình call duration không có dòng nguồn/percentile.
- Không chốt kênh nếu thiếu arrival profile, attempt policy, outcome rate hoặc reserve factor.
- Không nâng `ACCEPTED`, không đóng `G-LAB-SIM`/`G-ESIM32`, không bật real call từ pack này.

## 7. Verification

| Kiểm | Kết quả |
| --- | --- |
| GitNexus impact trước sửa | LOW; 0 production process |
| GitNexus detect-changes sau sửa | aggregate shared worktree `CRITICAL` 68 file / 196 symbol / 45 flow; không quy cho W-0142. Scoped W-0142 chỉ có capacity selftest/model metadata + docs, direct impact LOW/0 process |
| `capacity-selftest.mjs` | 6/6 PASS; `CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| calibrated relationship probe | PASS với shape `TEST_ONLY` occupancy 40 / cycle 45 / runtime 40; equal-values 40/40/40 bị reject vì double-count cooldown |
| Model output hiện hành | bất biến: peak 21, sensitivity 7..72 |
| Production symbol / public contract | không sửa |
| Calibration / real call / purchase decision | `NOT_RUN / NO / NOT_APPROVED` |

Không commit/push/deploy trong W-0142. Kết quả aggregate của detect-changes chứa W-0139 và các WIP
khác đang dùng chung checkout; direct file inventory và gate theo scope mới là attribution của lượt
này.

## 8. Handoff

Owner nhận dữ liệu: Business/M3, Product/Order Core, Infra/vendor. Module 8 resume ngay khi đủ §4.
Trước thời điểm đó, M8-01 ở trạng thái **`DATA_INTAKE_READY / BLOCKED_EXTERNAL`**, không phải done.

**Người ký:** **Tôi — Module 8 / Project Owner** · **29/08/2026**.
