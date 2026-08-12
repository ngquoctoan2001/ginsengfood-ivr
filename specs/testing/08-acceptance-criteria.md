# TEST-08 — Acceptance Criteria and Gates

Trạng thái: `TARGET_V1_DRAFT` · Cập nhật: `2026-08-12`.

## A. Implementation complete behind mocks

- [ ] .NET solution/API/Worker/domain/infrastructure, PostgreSQL migrations và Next.js admin build/lint/test pass.
- [ ] Fake Sales provider phủ cả Golden Hour ONLINE và 24/7 COD; target DTO/API/ACK/failures.
- [ ] Mock SIM phủ dial/speech/DTMF/disposition/health/errors; không có đường gọi thật ở MOCK.
- [ ] Policy registry versioned; candidate không hard-code production.
- [ ] Speech renderer đọc items/quantity/total/short area; PII leak tests pass.
- [ ] Callback target + current-compat adapters tách biệt, outbox/idempotency/retry/DLQ test pass.
- [ ] V1 notification disabled/no-op; IVR không gửi SMS.
- [ ] Docker/Helm/config/observability/runbooks/evidence scaffolding hoàn tất.

Mốc này không chứng minh external integration hay real calls.

## B. Lab real SIM verified

- [ ] 1 SIM thật, destination allowlist và kill switch được kiểm chứng.
- [ ] DTMF 1/0/no input/invalid và technical dispositions được evidence.
- [ ] one-active-call, cooldown, recovery và PII masking pass.
- [ ] `REAL_CUSTOMER_CALL_ALLOWED=NO` vẫn được enforce.

## C. Real Sales integration verified

- [ ] Sales producer đủ hai program + speech summary + dial-token.
- [ ] target callback/ACK/version/revalidation pass consumer-driven tests.
- [ ] production auth/mTLS decision và negative tests pass.
- [ ] no-answer/timeout race pass.

## D. Production eligible

- [ ] Owner chốt attempt policy.
- [ ] 32 eSIM procurement/config/capacity/failover/caller-ID pass.
- [ ] script/privacy/legal/retention/security approvals pass.
- [ ] staging/pilot/rollback/kill switch/on-call evidence accepted.
- [ ] Release owner ký mở `REAL_CUSTOMER_CALL_ALLOWED`.

## Fail gates

FAIL nếu IVR tự transition order, xử lý payment, gửi notification, gọi ngoài allowlist/gate, lưu/lộ raw phone/full address, tính technical lỗi thành no-answer, hard-code external candidate thành production truth, hoặc tuyên bố readiness khi còn gate chưa evidence.
