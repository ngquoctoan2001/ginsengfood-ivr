# W-0091 — Phase 1/2 retention and acceptance-test remediation

Status: `TESTS_PASS`

Valid findings `E-17` through `E-19`, `E-21` and `E-23` are remediated:

- `task_metadata` retention declares the intake-outbox dependency; PostgreSQL
  proof shows a held child blocks parent task/job deletion instead of creating
  an orphan or partial purge;
- missing retention periods execute the job and persist a `NOT_CONFIGURED`
  report/checkpoint with zero target mutation;
- all 13 `domain_negative` seed records execute via HTTP, rather than being
  counted only by parsing JSON;
- internal/admin privacy coverage now exercises the endpoint matrix and
  captures logs, with raw phone/address/token probes rejected or absent;
- queue, SIM, TTS-mode, TTS-PII and telephony safety assertions now cover the
  requested negative/concurrency/no-egress sides. No-egress proof inspects IL
  call targets instead of private field names; one-channel competition is
  concurrent rather than sequential;
- the eight P2-7 negative cases assert exact exception types; provider fake
  determinism runs the same scenario twice; mapper proof asserts the registry
  instance and serialized domain absence of phone fields.

The report claim that `IsCountedCustomerAttempt` is always false was incorrect:
unit/integration proof covers both non-counted technical outcomes and counted
customer no-answer outcomes. No source change was made for that false positive.

Retention JSON now truthfully records configuration mode `LAB_REAL_SIM` and
environment `DISPOSABLE_TEST_DB`; no telephony/provider was started. Final full
regression: `281/281 PASS`.

## Cập nhật chỉ dấu nghiệm thu — W-0325, 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Đây là giới hạn ủy quyền hiện hành theo tracker §2 tại commit `4346f6a`, bổ sung để kiểm C1.
Kết quả, thời điểm và phạm vi kiểm chứng lịch sử ở trên giữ nguyên; mục này không xác nhận
một lượt chạy mới và không thay chữ ký nghiệm thu. Xem [hồ sơ bổ sung W-0325](../W-0325/README.md).

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `IT-API-PII-05`, `IT-API-QUEUE-08`, `IT-TTS-MODE-09`, `UT-TEL-SAFETY-06`, `UT-TTS-PII-04`, `UT-TEL-CHANNEL-05`, `UT-SCRIPT-TEMPLATE-GUARD-04`, `UT-SCRIPT-INPUT-GUARD-05`, `IT-RET-OUTBOX-09`, `IT-RET-CONFIG-10`, `IT-INTAKE-NEGATIVE-18`, `UT-FAKE-PORT-08`, `UT-FAKE-REGISTRY-09`, `UT-RESULT-MAPPER-10`, `IT-API-SIM-12`.

Các test cho outbox giữ task, báo cáo NOT_CONFIGURED, fixture âm, fake provider và mapper đã có nhưng trước chưa mang TestId; W-0349 gắn thêm.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.
