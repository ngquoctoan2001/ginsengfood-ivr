# M8-07 — Target V1 shared callback readiness and handoff

**Work ID:** `W-0147`

**Baseline kiểm tra:** `main@b21ec676e490`

**Trạng thái:** **`M8_LOCAL_CALLBACK_READY / RETRY_AFTER_FIXED / ACK_MEDIA_FAIL_CLOSED_W0173 / OFFLINE_REPORT_VALIDATOR_READY_W0174 / M3_SECURITY_PLATFORM_REQUIRED / SHARED_E2E_NOT_RUN / DELIVERY_DISABLED`**

**Người ký phía Module 8:** **Tôi — Module 8 / Project Owner** · **2026-09-03**

**External artifact / signature:** **NOT_RECEIVED**

> Module 8 đã hoàn tất phần producer/outbox/transport local và sửa một defect thật trong xử lý
> `Retry-After`. Việc còn lại không phải “M8 code thêm cho đủ”: Module 3 phải có consumer generic,
> Security phải ký auth profile, Platform phải cấp sandbox/network, rồi hai bên mới chạy shared E2E.

## 1. Kết luận bắt buộc

1. Target V1 là `POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks`, dùng cho cả
   `GOLDEN_HOUR` và `TWENTY_FOUR_SEVEN`. Endpoint Golden Hour hiện tại chỉ là compatibility và
   **không được** nhận 24/7.
2. Callback body là snapshot bất biến; retry phải giữ nguyên payload, hash, `callback_id` và
   `Idempotency-Key`.
3. M8 đã map đủ 6 semantic ACK terminal và các lớp auth/invalid/retryable. `ACCEPTED` chỉ nghĩa là
   M3 nhận tín hiệu vào decision path; M3 vẫn phải revalidate trước mọi order transition.
4. W-0147 đã sửa lỗi local: response `429` trước đây bị coi retryable nhưng bỏ qua
   `Retry-After`. Runtime nay mang server delay sang dispatcher và không retry trước thời điểm đó;
   nếu header vắng/không dương thì dùng bounded exponential backoff + jitter hiện có.
5. W-0173 đã sửa lỗi local thứ hai: ACK `200/409` malformed hoặc sai media type không còn bị biến
   thành transient retry; nó đi terminal `CALLBACK_ACK_INVALID` và giữ HTTP status để audit.
6. W-0174 đã thêm validator offline cho report shared E2E: exact M8/M3 SHA, OAS/auth/platform hash,
   đủ 11 case cùng candidate và năm sign-off. PASS chỉ là đủ điều kiện review guard, không tự gỡ.
7. Real `TARGET_V1` vẫn bị validator từ chối boot. Không được gỡ guard này trước khi có đủ
   consumer/auth/sandbox/shared E2E và approval đúng owner.

## 2. Current callback path đã đối chiếu

| Bước | Current source | Kết luận |
| --- | --- | --- |
| Final result → outbox | `CallbackOutboxSnapshotFactory.Create` | Chỉ final result được enqueue; body Target V1 được serialize một lần, gắn SHA-256 và idempotency key ổn định |
| Lease/dequeue | `CallbackOutboxRepository.DequeueReadyAsync` | `FOR UPDATE SKIP LOCKED`, lease token/expiry và reclaim `SENDING` hết hạn |
| Route contract | `SalesCallbackContractSelector` + `CallbackDispatcher.RunBatchAsync` | Target V1 phủ hai program; current compat bị cô lập |
| HTTP request | `TargetV1CallbackTransport.SendAsync` | Path/body order ID match; Bearer token, `Idempotency-Key`, `X-Correlation-Id`; timeout fail-safe |
| ACK identity | `TargetV1CallbackTransport.ClassifyAsync` | `200/409` chỉ được chấp nhận khi echo đúng `callback_id` và `correlation_id` |
| Terminal semantics | `CallbackDispatcher.CreateUpdate` | Accepted/duplicate, blocked/review, stale/conflict, invalid/auth đi đúng terminal state; không blind retry |
| Retry | `CallbackDispatcher.Retry` | Cùng immutable message; bounded retry count; exponential delay + deterministic jitter; `429` tôn trọng delay tối thiểu từ server |
| Circuit | `CallbackCircuitBreaker` | Chỉ transient failure mở circuit; half-open probe được giải phóng/reset đúng terminal behavior |
| Persistence/audit | `CallbackOutboxRepository.CompleteDeliveryAsync` | Compare lease token, persist HTTP/code/retry, tạo audit và review item khi cần |
| Production guard | `CallbackDeliveryOptionsValidator` | `Enabled + TARGET_V1` fail start do auth/sandbox W-0006/OD-V1-07 chưa đóng |

Không có consumer Target V1 trong repo IVR — và cũng không nên có. Endpoint đích là bề mặt do M3
sở hữu. Source hiện hành chỉ có outbound transport, fake fixture và adapter compatibility.

## 3. Defect W-0147 đã sửa

### Trước sửa

- Target OAS và IR-06 quy định `429` có `Retry-After`.
- Fixture đã phát header `Retry-After: 1`.
- Transport trả chung `CALLBACK_RETRYABLE_RESPONSE`; dispatcher chỉ tính local backoff và có thể gửi
  lại trước thời điểm M3 cho phép.

### Sau sửa

- `CallbackTransportResult` mang optional `RetryAfter`.
- `TargetV1CallbackTransport` chỉ lấy delta dương trên response `429`.
- Dispatcher chọn delay lớn hơn giữa local backoff và server `Retry-After`; retry budget, payload và
  idempotency behavior không đổi.
- Unit test mới khóa cả parsing và scheduling:
  `UT-CALLBACK-RETRY-AFTER-02B`, `UT-CALLBACK-RETRY-AFTER-09B`.

Phạm vi impact đã được cảnh báo trước sửa: GitNexus xếp `ClassifyAsync` và `CreateUpdate` **HIGH**,
ảnh hưởng chuỗi transport → dispatcher → worker cùng chaos flow. Thay đổi không chạm producer,
payload schema, order state hoặc production enablement.

### Follow-up W-0173 — malformed/media-type ACK

`ReadFromJsonAsync` ném `NotSupportedException` với ACK như `text/html`; trước đó exception thoát khỏi
transport và dispatcher retry như lỗi bất ngờ. W-0173 thêm parse boundary fail-closed cho cả
`JsonException` và `NotSupportedException`, trả `CALLBACK_ACK_INVALID` với HTTP status gốc. Test
`UT-CALLBACK-ACK-MEDIA-02C` phủ `200 text/html` và `409` JSON bị cắt; focused callback `40/40`, full
Unit `499/499`, Contract `24/24`, build 0 warning/error. Current PostgreSQL/Chaos rerun không chạy
được vì Docker server pipe vắng; bằng chứng W-0162 `7/7 + 8/8` giữ nguyên ở baseline lịch sử riêng.

## 4. ACK và retry contract phía M8 đã khóa

| HTTP / code | M8 persistence behavior | Retry tự động |
| --- | --- | --- |
| `200 ACCEPTED` | `DELIVERED_ACCEPTED` | Không |
| `200 DUPLICATE_ACCEPTED` | `DELIVERED_ACCEPTED` | Không |
| `200 BLOCKED_BY_CORE` | `DELIVERED_BLOCKED` + review | Không |
| `200 REVIEW_REQUIRED` | `DELIVERED_REVIEW` + review | Không |
| `409 REJECTED_STALE` | `REJECTED_STALE` + review | Không |
| `409 IDEMPOTENCY_CONFLICT` | `IDEMPOTENCY_CONFLICT` + review | Không |
| `401/403` | `AUTH_REJECTED` + review | Không |
| `422` hoặc ACK sai identity/schema | `INVALID_DEAD_LETTER` + review | Không |
| `429` | `RETRY_PENDING`; không sớm hơn `Retry-After` | Có, trong retry budget |
| `5xx`, timeout, transport failure | `RETRY_PENDING`; local backoff + jitter | Có, trong retry budget |
| Hết retry budget | `RETRY_EXHAUSTED` + review | Không |

M3 phải ký đúng ranh giới idempotency sau: cùng key + cùng immutable body →
`DUPLICATE_ACCEPTED`; cùng key + body khác → `IDEMPOTENCY_CONFLICT`. Nếu M3 dùng semantics khác,
consumer contract phải nêu rõ trước khi bật delivery.

## 5. Artifact bắt buộc từ bên nhận handoff

### Module 3 / Sales API-Core

- Consumer commit cho đúng path Target V1 và phủ cả Golden Hour + 24/7.
- OpenAPI authoritative từ repo M3, có version và compatibility/deprecation policy.
- ACK taxonomy, echo identity, idempotency retention window và concurrency rule.
- Revalidation order: version/state/program/payment/inventory/recall/sale-lock/quality-hold/evidence.
- Provider CDC chạy trong pipeline M3, có owner sửa khi đỏ.

### Security

- Auth profile đã ký: issuer, JWKS/trust source, audience, scope `ivr.result.write`, TTL, rotation,
  clock skew, revocation và mTLS decision.
- Credential reference qua secret manager; không gửi token trong ticket/chat/evidence.
- Negative test cho missing/expired/wrong audience/wrong scope/rotated credential.

### Platform

- Reachable sandbox base URL, DNS/TLS/network policy/egress route và service identity wiring.
- Timeout/rate-limit contract, `Retry-After` behavior và observability correlation.
- Runbook rotate/revoke/rollback; owner/on-call và cửa sổ shared test.

Phản hồi “endpoint sẽ có”, screenshot, Postman chạy tay không có SHA, token gửi trực tiếp hoặc local
WireMock **không** phải artifact chấp nhận.

## 6. Shared E2E matrix bắt buộc

| Case | Evidence phải chứng minh |
| --- | --- |
| Golden Hour accepted | M3 nhận đúng body/header, revalidate và trả ACK identity đúng |
| 24/7 accepted | Cùng endpoint generic; không rơi vào Golden Hour compat |
| Exact replay | Cùng key/body → `DUPLICATE_ACCEPTED`, một decision effect |
| Changed-body replay | Cùng key/body khác → `IDEMPOTENCY_CONFLICT`, không transition |
| Stale version/state | `REJECTED_STALE`, M8 không retry |
| Core blocker | `BLOCKED_BY_CORE` hoặc `REVIEW_REQUIRED`, order truth thuộc M3 |
| Auth negative | `401/403`, M8 terminal review; không retry mù |
| Invalid schema/result | `422`, M8 dead-letter/review |
| Rate limit | `429 + Retry-After`; DB `next_retry_at` không sớm hơn header, retry giữ nguyên key/body |
| M3 outage/timeout | Bounded retry, circuit open/half-open/recover, không mất/nhân callback |
| No-answer final | Advisory `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`; M8 không tự hủy đơn |

Report phải ghim exact SHA repo M8 và M3, environment/config version, thời gian, request/ACK đã che
secret, các state row trước/sau và kết quả từng case. Selected green cases không được đổi thành
“shared E2E pass”.

### 6.1. Intake report offline — W-0174

- Template pending: [`shared-e2e-report.template.json`](../../docs/evidence/W-0174/shared-e2e-report.template.json).
- Validator: [`target-v1-shared-e2e-report-validator.mjs`](../../deploy/ci/scripts/target-v1-shared-e2e-report-validator.mjs).
- Evidence/hướng dẫn: [`W-0174`](../../docs/evidence/W-0174/README.md).
- Chế độ report thật bắt buộc reviewer truyền độc lập exact M8/M3 SHA và hash M3 OAS,
  consumer/CDC, Security auth/custody, Platform sandbox/network/TLS.
- Local verification: `1 valid / 46 refusal`; template đủ `11/11` case nhưng giữ
  `EXTERNAL_E2E_NOT_RUN`.
- Output PASS là `SHARED_E2E_REPORT_VALID_ELIGIBLE_FOR_GUARD_REVIEW_ONLY`; không phải lệnh gỡ
  guard, enable production hoặc cho phép gọi khách thật.

## 7. Điều kiện gỡ fail-closed delivery guard

Chỉ mở một change riêng để xem xét gỡ guard khi **đồng thời** đủ:

1. M3 consumer + authoritative OAS + CDC đã merge.
2. Security auth profile và secret custody đã ký.
3. Platform sandbox/network/TLS đã provision và smoke pass.
4. Toàn bộ matrix §6 pass trên cùng candidate SHA/config.
5. M8 owner, M3 owner, Security/Platform và Release owner ký phạm vi tương ứng.

Thiếu bất kỳ mục nào thì giữ:

**`TARGET_CONTRACT_V1=DRAFT / CALLBACK_DELIVERY_DISABLED / SHARED_E2E_NOT_RUN / REAL_CUSTOMER_CALL_ALLOWED=NO`**.

## 8. Phản hồi bị từ chối

- Yêu cầu M8 dựng luôn consumer Sales trong repo IVR.
- Dùng endpoint Golden Hour compat cho 24/7.
- Gỡ validator chỉ vì local unit/contract test xanh.
- Coi `ACCEPTED` là order đã confirmed hoặc bỏ revalidation phía M3.
- Retry `409`, `401/403`, `422`, ACK sai identity hoặc dùng payload/key mới cho mỗi lần retry.
- Bỏ qua `Retry-After`, hoặc tuyên bố shared integration pass bằng fake/WireMock.
- Dùng chữ ký M8 để ký thay M3, Security, Platform hay Release.

## 9. Mẫu phản hồi bắt buộc

| Field | Bên nhận phải điền |
| --- | --- |
| Consumer/OAS commit | Repo + branch/ref + exact SHA + link |
| Endpoint/environment | Sandbox base URL + version; không ghi secret |
| ACK/idempotency/revalidation | Decision record + signer + date |
| Auth/trust/custody | Security record + secret reference + rotation owner |
| Network/TLS/observability | Platform record + smoke evidence |
| CDC/shared E2E | Pipeline/report + exact SHA hai repo + từng case §6 |
| Rollout/rollback | Owner, sequence, kill/disable path |
| Residual blocker | `NONE` hoặc exact blocker/owner/due date |

“OK”, “đã hiểu”, “dev tự phối hợp” hoặc thiếu signer/SHA/report không phải sign-off.

## 10. Chữ ký

| Bên | Người ký | Ngày | Phạm vi |
| --- | --- | --- | --- |
| Module 8 / Project Owner | **Tôi — Module 8 / Project Owner** | **2026-09-03** | Local callback behavior, W-0147 fix, handoff matrix và stop rule |
| Module 3 / Sales API-Core | `<chưa nhận>` | `<chưa nhận>` | Consumer, ACK/idempotency/revalidation, OAS và CDC |
| Security | `<chưa nhận>` | `<chưa nhận>` | Auth/trust/credential custody |
| Platform | `<chưa nhận>` | `<chưa nhận>` | Sandbox/network/TLS/operations |
| Release owner | `<chưa nhận>` | `<chưa nhận>` | Cho phép enable/deploy sau shared evidence |

Chữ ký M8 không đóng `G-CONTRACT`, `G-AUTH`, `G-PLATFORM` hay `G-RELEASE`; không cho phép bật
Target V1 hoặc gọi khách thật.
