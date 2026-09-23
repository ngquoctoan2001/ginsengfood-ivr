# W-0345 — toàn luồng đơn giả chương trình 24/7 trên lab và S5

Ngày 23/09/2026. `REAL_CUSTOMER_CALL_ALLOWED=NO`; production vẫn `BLOCKED`.

**S5_TARGET_PASS và LOCAL_REHEARSAL_PASS: chương trình 24/7 lần đầu chạy trọn 7 ca, trên lab riêng lẫn S5. Chờ Toàn nghiệm thu.**

## Vì sao có việc này

[So sánh Giờ Vàng và 24/7](../../../plan/ivr-orther/so-sanh-gio-vang-vs-24-7-2026-09-22.md) (§6, mục 4)
ghi rằng bộ kiểm full-flow S5 chỉ chạy `GOLDEN_HOUR`, nên **luồng 24/7 chưa từng chạy end-to-end
trên lab**, kể cả ở mức 1 attempt. W-0344 vừa đạt trên S5 với Giờ Vàng. W-0345 chạy đúng 7 ca đó
với `TWENTY_FOUR_SEVEN` + `COD`.

## Thay đổi

- [cases.py](../../../deploy/lab/full-flow-s5/cases.py) đọc `IVR_FLOW_PROGRAM` và chỉ chấp nhận đúng
  hai cặp của ma trận wire: `GOLDEN_HOUR` + `ONLINE` (mặc định, như W-0344) và `TWENTY_FOUR_SEVEN` +
  `COD`. Payload intake lấy chương trình, thanh toán và tên hiển thị từ cặp đã chọn. Sau khi nhận
  đơn, mỗi ca đọc `program_type|payment_method_snapshot` từ `ivr_confirmation_tasks` và đòi đúng cặp
  đó. Diff so với gói gốc −3/+19 dòng; 51 assert cũ giữ nguyên, thêm 2 assert.
- Launcher, app (`66a6baa`), image, model, lời thoại và quota giữ nguyên byte. Kịch bản đã duyệt không
  đọc tên chương trình, nên audio và số đo độ dài tiếng của W-0344 vẫn dùng được.
- Lab dùng `lab-softphone-v1`, có cho cả hai chương trình (1 attempt, cửa sổ 300 s). Vì vậy lượt này
  **không** kiểm cửa sổ 15 phút hay mốc `[0, 450]` của 24/7 production; nó kiểm luồng của 24/7 qua
  intake, eligibility, chuẩn bị lời thoại, SIP/RTP/DTMF, kết quả và callback.
- Callback đi theo hợp đồng Target V1 (`FAKE_TARGET_V1`), không qua endpoint Giờ Vàng cũ, vì endpoint
  cũ ném lỗi với 24/7 (`SalesCallbackContractSelector`).
- Gói `cpu-r4` = `cpu-r3` + `cases.py` mới, dựng bằng [profile](cpu-retry-build.json); [pin](cpu-retry-pins.json)
  và [manifest](cpu-retry-manifest.json) riêng của W-0345, để pin của W-0344 vẫn trỏ vào `cpu-r3`.
  Script retry thêm `-PinsPath` và `-Program` (mặc định Giờ Vàng).

## Kết quả

| Lượt | Kết quả |
| --- | --- |
| Local `rehearsal-247-1` (04:32–04:38 UTC) | **PASS** trọn vẹn: 7/7 ca, cả 7 task lưu `TWENTY_FOUR_SEVEN\|COD`, đối soát 7/7/4/3/9/6/6 với 0 đếm nhầm hay trùng, container có sẵn không đổi |
| S5 `20260923-113341-bc374828` (04:33–04:40 UTC) | **`S5_TARGET_SYNTHETIC_SIP_PASS`**: 7/7 ca, cả 7 task lưu `TWENTY_FOUR_SEVEN\|COD`; đối soát như trên; 43 container có sẵn không đổi, dọn 11 container; manifest trong receipt khớp gói `cpu-r4` (`0c75d4e8…`) |

Từng ca trên S5, cùng kết quả cuối như Giờ Vàng ở W-0344: xác nhận `IVR_CONFIRMED` · khách hủy
`IVR_CUSTOMER_CANCELLED` · TTS lỗi rồi thử lại `IVR_CONFIRMED` (2 attempt, lần 1 không tính) · hết
hạn trong hàng chờ `IVR_CAPACITY_EXCEPTION` (0 attempt) · hết hạn lúc tạo tiếng
`IVR_CONFIRMATION_WINDOW_EXPIRED` · không bấm phím `IVR_NO_ANSWER_FINAL` · người vận hành hủy
`IVR_TECHNICAL_EXCEPTION`. Sáu kết quả cuối, mỗi kết quả đúng một callback Target V1 được nhận;
ca người vận hành hủy không có callback. Tiếng phát đúng thời gian thực, mỗi lần bấm phím đều đã
nhận đủ gói (1104/1104, 1593/1593, 1280/1280).

**Kiểm chứng:**
- Test lab: 50 trên Windows (1 test chỉ chạy POSIX) · 39/39 trên Linux.
- [test_full_flow_program.py](../../../deploy/lab/tests/test_full_flow_program.py): đỏ với `cases.py`
  của `cpu-r3` (không có `PROGRAMS`, `admit` viết cứng `GOLDEN_HOUR`), xanh với bản mới.
- Test tách phiên SSH 9/9, thêm kiểm `IVR_FLOW_PROGRAM` tới được installer.
- Verify `cpu-r4`: 20 file, 53 assert (51 assert cũ giữ nguyên, thêm 2).
- `cpu-r3` của W-0344 vẫn đạt verify và `-VerifyOnly` tại HEAD; pin của nó không đổi.

**File của W-0344 đổi sau khi W-0344 nộp bằng chứng**, đều tương thích ngược: `retry-s5-full-flow.ps1`
(`-PinsPath`, `-Program`), `build-cpu-retry.py` (`--profile`), `verify-cpu-retry.py` (`--evidence-dir`,
cho phép thêm assert nhưng không cho bớt), `test-retry-detach.py`, và `cases.py` (chọn chương trình).
Bằng chứng W-0344 gắn với commit `317fca5`.

## Chạy lại

```powershell
& 'C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0344\retry-s5-full-flow.ps1' -PinsPath 'C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0345\cpu-retry-pins.json' -Program TWENTY_FOUR_SEVEN
```

Không cần mật khẩu vì máy này đăng nhập S5 bằng SSH key.

## Giới hạn

Chưa chứng minh SIM thật, Sales/M3 chung, nhiều worker hay production. Bộ số 24/7 production
(`2 / [0,450] / 900s`) không còn trong database từ `W-0300`, và chỉ vào lại được qua bước vận hành có
approval. Chỉ Toàn chuyển W-0345 sang `ACCEPTED`.
