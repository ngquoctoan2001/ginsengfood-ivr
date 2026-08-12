# PROMPT P4-1 — Sales Platform Provider Wiring and Contract Verification

## 0. Meta

Work `W-0029` · prereq P2-6 plus W-0002..W-0006 external inputs. Build provider code with mocks now; real verification remains `BLOCKED_EXTERNAL` until inputs exist.

## 1. Outcome

Wire Sales task/callback/auth through typed providers without changing domain rules. Support Target V1 and explicitly bounded current Golden Hour compatibility; produce evidence that distinguishes WireMock, sandbox and staging.

## 2. Inputs to demand

Sales task/callback OpenAPI and examples; program/callable matrix; speech schema; dial-token resolver; sandbox/base URLs; JWT/mTLS metadata/test credentials; idempotency/version/timeout semantics.

If any is missing, update its external Work ID and continue only through fake provider—never invent it.

## 3. Build

1. Config profiles `FakeSales`, `CurrentGoldenHourCompat`, `TargetV1`; validate allowed mode/provider matrix at startup.
2. Bind incoming task auth/idempotency/correlation; reject schema/matrix/privacy/policy violations.
3. Bind target callback client/semantic ACK mapping and current adapter from P2-6.
4. Implement service JWT acquisition/cache/refresh and optional mTLS hook; secrets external.
5. Add readiness/circuit/timeout/backoff/metrics without weakening fail-closed behavior.
6. Add consumer-driven contract suite runnable against fake and supplied sandbox; capture provider version/OpenAPI hash.

## 4. Acceptance/evidence

- fake/WireMock suite passes first;
- current compat is labeled and cannot carry 24/7;
- real sandbox evidence separately covers both programs, speech/token, every target ACK, stale/idempotency, auth negatives and no-answer timeout;
- update W-0029 and external W-0002..6; only close each with its exact artifact.

No production flag or customer call is enabled by this prompt.

## 4. Forbidden
- ❌ IVR transition/ghi order state (D-02); `recommended_core_action` là advisory.
- ❌ Coi mock/fake evidence là đã đóng external gate (W-0002..W-0006).
- ❌ Bật real provider hoặc gọi khách thật trong slice này.
- ❌ Trộn internal record DTO với outbound Sales callback DTO.

## 5. Definition of Done
- [ ] Toàn bộ fake/WireMock suite xanh → đạt **`TESTS_PASS`** (mock-only). Đây là mức tối đa slice này có thể đạt.
- [ ] Real sandbox evidence là **hạng mục riêng**, `NOT_RUN`/`BLOCKED_EXTERNAL` cho tới khi Sales cung cấp endpoint/credential; **không** phải điều kiện của `TESTS_PASS`.
- [ ] Cập nhật Work ID trong tracker: artifacts, command/kết quả, evidence link, residual external gate; chỉ reviewer/owner chuyển `ACCEPTED`.
