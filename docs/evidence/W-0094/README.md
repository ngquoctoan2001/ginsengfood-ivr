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
