# W-0094 — Phase 1/2 lifecycle and dead-code remediation

Status: `TESTS_PASS`

Valid lifecycle/dead-code findings are closed:

- callback dispatcher/current transport are scoped and the worker creates a
  scope per batch, avoiding singleton retention of typed-client state;
- shared audio synthesis no longer captures the first waiter's cancellation
  token; cancelling one waiter does not poison the cached operation for others;
- Fake SIM active dictionaries were already removed at hangup. Remaining event
  and played-speech history is now bounded (`4096` and `1024` respectively);
- retention one-pass completion no longer calls host-wide `StopApplication`, so
  scheduler/normalizer/callback workers keep running;
- unused `SimChannelLeaseRepository` and its DI registration were removed;
- current Golden Hour transport reuses `CurrentGoldenHourCompatMapper` at
  runtime instead of duplicating mapping logic.

The report's recording-readback branch claim is not accepted as a defect. The
current fake returns recording disabled, but the gateway verifies the provider
health response as defense in depth; tests also reject an enabled recording
request. Removing that check would weaken D-05 rather than remove harmful dead
code.

Proof includes shared-cache cancellation isolation, concurrent one-channel
safety, bounded fake retention behavior, scoped callback regression and full
`281/281` solution tests. Release build remains `0 warnings / 0 errors`.

## Cập nhật chỉ dấu nghiệm thu — W-0325, 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Đây là giới hạn ủy quyền hiện hành theo tracker §2 tại commit `4346f6a`, bổ sung để kiểm C1.
Kết quả, thời điểm và phạm vi kiểm chứng lịch sử ở trên giữ nguyên; mục này không xác nhận
một lượt chạy mới và không thay chữ ký nghiệm thu. Xem [hồ sơ bổ sung W-0325](../W-0325/README.md).

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. Trạng thái khai báo:

- Chưa khai phép kiểm. Sáu thay đổi của việc này còn ở HEAD, nhưng chỉ một có test: test chia sẻ lượt tổng hợp giọng, nay đã gắn TestId ở W-0349. Giới hạn lịch sử sự kiện của SIM giả, việc bỏ StopApplication và scope theo lô của callback chưa có test nào, nên chưa khai phép kiểm cho tới khi có test cho ba phần đó.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Khai báo phép kiểm C2 — W-0353, 24/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Mục W-0349 ở trên ghi ba thay đổi chưa có test. W-0353 viết test cho cả ba và khai phép kiểm trong
[`acceptance-tests.json`](acceptance-tests.json). Sáu thay đổi của việc này ứng với:

| Thay đổi | Phép kiểm |
|----------|-----------|
| Callback dispatcher và transport Golden Hour hiện hành là scoped; mỗi lượt bơm mở một scope rồi đóng | `UT-WORKER-CALLBACK-SCOPE-06` (mới) |
| Lượt tổng hợp giọng dùng chung không mang token hủy của người chờ đầu tiên | `IT-TTS-SHARED-10` (gắn TestId ở W-0349) |
| Lịch sử sự kiện và giọng đã phát của SIM giả có giới hạn 4096 và 1024, giữ phần mới nhất | `UT-FAKE-HISTORY-10` (mới); `UT-TEL-CHANNEL-05` giữ phần một kênh chỉ một cuộc gọi |
| Host retention chạy dài không dừng worker; chỉ host chạy một lần của CronJob dừng | `UT-WORKER-RETENTION-05` (mới) |
| Transport Golden Hour hiện hành dùng lại `CurrentGoldenHourCompatMapper` lúc chạy | `UT-CALLBACK-GH-COMPAT-06` (thân request mang kết quả đã ánh xạ `CONFIRMED`) |
| Gỡ `SimChannelLeaseRepository` không dùng | Không có test: bằng chứng là lớp này không còn trong `src/`; chỉ một chú thích trong `tests/Ivr.IntegrationTests/SchedulerPersistenceTests.cs` còn nhắc tên nó |

Ba test mới được kiểm bằng đột biến ngày 24/09/2026: bỏ giới hạn sự kiện, bỏ giới hạn giọng đã
phát, bỏ lệnh dừng của host chạy một lần, cho host chạy dài gọi dừng, cho vòng bơm giữ một scope
suốt đời, và đổi dispatcher thành singleton. Mỗi đột biến làm đúng test tương ứng đỏ; mã nguồn đã
khôi phục nguyên vẹn sau đó.

Phần đọc lại cấu hình ghi âm vẫn giữ như quyết định lịch sử ở trên: đó là lớp phòng vệ, không phải
mã chết.

## Owner nghiệm thu — 25/09/2026

Toàn chỉ thị “tiếp đi cho full luôn”, trong khuôn ủy quyền “thì cái nào xong cho xong luôn đi”. Theo
[danh sách W-0353](../W-0353/README.md), W-0094 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm.
Sáu thay đổi đều có phép kiểm; ba test mới UT-FAKE-HISTORY-10, UT-WORKER-RETENTION-05 và
UT-WORKER-CALLBACK-SCOPE-06 bắt được cả sáu đột biến (W-0353). Giữ phần đọc lại cấu hình ghi âm là
quyết định đã ghi. Claude chọn theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo
quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`b92942e`. REAL_CUSTOMER_CALL_ALLOWED=NO.
