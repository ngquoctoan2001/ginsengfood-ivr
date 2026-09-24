# W-0090 — Phase 1/2 callback and admin atomicity remediation

Status: `TESTS_PASS` (local fake transport + disposable PostgreSQL)

Valid findings `E-09`, `E-12`, `E-15` and `E-16` are closed:

- technical retry compares against canonical
  `EligibilityDecisions.Eligible` (`ELIGIBLE_FOR_IVR`) and has a covered happy
  path plus kill-switch/allowlist fail-closed paths;
- unexpected callback transport exceptions become bounded transient results
  and release the half-open probe; cancellation is propagated; readiness
  reflects the half-open probe state;
- admin mutation, idempotency record and append-only audit commit in one
  serializable transaction; an injected idempotency snapshot failure proves
  the business mutation rolls back;
- callback completion is a lease-conditioned SQL update. A stale token changes
  zero rows, and concurrent completion from one lease permits exactly one
  winner without stale overwrite.

Focused proof includes `UnexpectedTransportFailureBecomesRetryAndReleasesHalfOpenProbe`,
`AdminMutationRollsBackWhenIdempotencySnapshotCannotCommit`, technical retry
success/fail-closed tests, and
`DeliveryCompletionUsesLeaseFencingAndCreatesAdminVisibleReview`.

Final full regression: `281/281 PASS`. Callback delivery remains disabled by
default; real Sales/auth/CDC and production retry behavior are `NOT_RUN`.

## Cập nhật chỉ dấu nghiệm thu — W-0325, 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Đây là giới hạn ủy quyền hiện hành theo tracker §2 tại commit `4346f6a`, bổ sung để kiểm C1.
Kết quả, thời điểm và phạm vi kiểm chứng lịch sử ở trên giữ nguyên; mục này không xác nhận
một lượt chạy mới và không thay chữ ký nghiệm thu. Xem [hồ sơ bổ sung W-0325](../W-0325/README.md).

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `IT-CALLBACK-OUTBOX-06`, `IT-API-RETRY-06`, `UT-CALLBACK-TOKEN-CIRCUIT-10`, `UT-CALLBACK-TRANSPORT-RETRY-16`, `IT-API-ATOMIC-11`.

Hai test cho phần chốt breaker và ghi admin nguyên tử đã có nhưng trước chưa mang TestId; W-0349 gắn thêm.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0090 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
