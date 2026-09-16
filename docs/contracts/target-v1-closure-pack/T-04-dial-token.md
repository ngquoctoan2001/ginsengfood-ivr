# T-04 — Dial-token: issue / resolve / TTL / one-use / audit

External work `W-0004` · quyết định `OD-V1-05`, `OD-V1-17`, `OD-V1-18` · gate **real call** · `LAB_REAL_SIM` · trạng thái `OPEN`

Owner: **Module 3 / Official Contact service** (chọn contact, phát token), **Security** (trust boundary,
threat model), **Platform** (custody/network) và **Telephony vendor** (khả năng gateway).

Due: chốt **trước `P8-1`** (real SIM lab) — không quay số thật khi chưa có semantics token. Ngày cam kết của owner: `<owner điền>`.

Correction hiện hành: `W-0150` · `2026-09-03` · đọc ticket này cùng
[M8-10 decision pack](../../../plan/ivr-orther/m8-10-contact-dial-token-production-decision-pack-2026-09-03.md).
Audit không đóng T-04; nó sửa factual drift về TTL, reuse và resolver output, đồng thời chuyển câu hỏi
thành `DTK-01..DTK-15` để M3/Security/Platform/Telephony ký.

## 1. Current evidence — đã đọc từ nguồn

**Task mang đúng một token scalar.** [`ivr-order-confirmation.v1.yaml`](../../../specs/api/openapi/ivr-order-confirmation.v1.yaml) — `IvrConfirmationTaskV1`:

```yaml
dial_token: { type: string, description: Opaque token; never a raw phone number. }
dial_token_expires_at: { type: string, format: date-time }
```

Cả hai đều `required`. Không có mảng, không có endpoint reissue, không có refresh — trong **bất kỳ** contract nào của dự án.

**Cùng task đó có thể cần nhiều lần quay.** Wire cho phép policy từ 1 đến 10 customer attempt;
production policy còn chờ owner ký. Cùng schema:

```yaml
max_customer_attempts: { type: integer, minimum: 1, maximum: 10 }
attempt_offsets_seconds: { type: array, minItems: 1, maxItems: 10, items: { type: integer, minimum: 0 } }
```

Cộng thêm `DT-02`: **technical exception không tính là customer attempt**. Vì vậy số lần dial có thể
lớn hơn số customer attempt nếu policy retry kỹ thuật cho phép.

**Nhiều tài liệu cũ ghi token là one-use/attempt.** Đây là target proposal cần owner ký, không phải
behavior production đã có. Implementation MOCK/LAB hiện chỉ ngăn resolve trùng theo cặp
`(token fingerprint, attempt_id)`; một scalar token vẫn dùng được cho attempt khác.

**IVR đã dựng seam nhưng chưa có nguồn thật.** [`ProviderPorts.cs`](../../../src/Ivr.Domain/Ports/ProviderPorts.cs)
khai `IDialTokenResolver`; `DialAuthorization` chứa `providerDestinationReference`, từ chối raw phone,
chỉ lộ opaque reference qua `RevealToTrustedGateway()` và `ToString()` trả
`[REDACTED_DIAL_AUTHORIZATION]`. Implementation hiện có chỉ là fake/MOCK/LAB.

## 2. Target delta — chính xác là gì

**(a) `OD-V1-17` — một token, nhiều lần quay. Đây là mâu thuẫn số học, không phải mơ hồ.**

| Cần | Có |
| --- | --- |
| 1..10 customer attempt + retry kỹ thuật theo policy | 1 token scalar |
| Nếu chọn token globally/per-attempt one-use | không có cách lấy token tiếp theo trên contract current |

Bốn phương án, Sales/Security chọn một:

| # | Phương án | Đánh đổi |
| --- | --- | --- |
| a | `dial_tokens[]` per-attempt trong task | Sales phải biết trước số lần quay, kể cả retry kỹ thuật — mà retry kỹ thuật thì không đoán được |
| b | Endpoint reissue/refresh | Thêm một round-trip đồng bộ ngay trước mỗi lần quay; thêm một điểm chết |
| c | Token bundle (n token, n do policy quyết) | Vẫn phải bao được retry kỹ thuật |
| d | Token reusable có TTL + risk control ghi rõ | Bỏ tính chất one-use; cần threat model nói rõ chấp nhận rủi ro gì |

Đây **không** phải quyết định IVR được tự chọn: token là thứ Sales/Security phát ra.

**(b) `OD-V1-18` — resolve `dial_token → destination` xảy ra ở đâu.** Hai tài liệu cũ nói khác
nhau. Type contract current không cho resolver trả raw E.164 vào IVR; nó chỉ cho một opaque provider
destination reference. Nếu vendor cuối cùng bắt buộc E.164, bước biến reference thành E.164 phải nằm
sau external vault/gateway boundary đã được Security/vendor duyệt.

> Task/intake/storage của IVR chỉ được thấy opaque token/ciphertext. Raw E.164, nếu vendor cần, chỉ
> được lộ bên trong external trusted gateway/vault chứ không quay lại IVR application/domain/log.

Ranh giới **mục tiêu** đã được phác: `IVR → opaque token/ciphertext → external trusted
resolver/gateway → provider destination/E.164`, và IVR không giữ mapping key. Nhưng "phác" không
phải "đã duyệt". Câu hỏi thật là: **vault/mapping chạy ở đâu, ai vận hành, ai audit và vendor nhận
opaque handle hay bắt buộc E.164**.

**(c) TTL current bị ép thành exact equality nhưng wire không nói rõ.** Intake đòi expiry không sớm
hơn window end; persistence lại đòi expiry không muộn hơn window end. Accepted persisted task vì vậy
chỉ hợp lệ khi hai timestamp bằng nhau. OpenAPI chưa diễn đạt cross-field invariant này; owner phải ký
exact equality hoặc invariant min/max khác.

**(d) Audit chưa được định nghĩa.** Ai ghi lại việc một token được resolve? Nếu vault ở phía Sales/vendor thì log ở đó; IVR chỉ có `attempt_id`. Cần chốt để điều tra sự cố "tại sao số này bị gọi" có đường đi rõ ràng.

## 3. Sample payload

Task (phần liên quan):

```json
{
  "phone_ref": "phref-test-0001",
  "phone_masked": "84xxxxx0001",
  "dial_token": "<opaque>",
  "dial_token_expires_at": "2026-08-18T03:05:00Z",
  "max_customer_attempts": 2,
  "attempt_offsets_seconds": [0, 150]
}
```

`phone_masked` dùng dải test `84xxxxx….` `dial_token` là chuỗi mờ — **không bao giờ** là số điện thoại, kể cả đã mã hoá.

Nếu chọn phương án (a), delta sẽ là:

```json
{ "dial_tokens": ["<opaque-1>", "<opaque-2>"], "dial_tokens_expire_at": "2026-08-18T03:05:00Z" }
```

## 4. Acceptance test — phải xanh khi đóng

| Test | Ở đâu | Khẳng định |
| --- | --- | --- |
| `IT-API-PII-05` | `tests/Ivr.IntegrationTests/` | Token không rò ra API surface |
| PII gate CI | [`deploy/ci/scripts/scan-pii.sh`](../../../deploy/ci/scripts/scan-pii.sh) + `deploy/ci/pii-patterns.txt` | Pattern chặn `dial_token: <giá trị>` xuất hiện trong evidence |
| `MockDialTokenVault` suite | `tests/Ivr.UnitTests/Telephony/MockTelephonyTests.cs` | Expiry/allowlist/per-attempt duplicate fail-closed; không suy thành production one-use |
| **`CDC-DIALTOKEN-01`** *(Sales/Security viết)* | phía phát token | Token hết hạn → resolve fail; token đã dùng → resolve fail theo đúng semantics đã chọn |
| **`CDC-DIALTOKEN-02`** *(Sales/Security viết)* | phía phát token | Replay: cùng token, hai attempt khác nhau → hành vi đúng như phương án đã chốt |

## 5. Mock fallback

`FakeDialTokenResolver` + `MockDialTokenVault` cho phép Phase 2 chạy. MOCK/LAB fingerprint không đảo
ngược, nhưng state là process-local; API `Protect()` và Worker `ResolveAsync()` là hai deployable nên
strict map không dùng được cross-process nếu không có wildcard fake. Reuse current là một lần cho mỗi
`(fingerprint, attempt_id)`, không phải globally one-use. Đừng đọc test xanh thành production semantics.

## 6. Closure artifact — owner điền

- [ ] Ký đủ `DTK-01..DTK-15` trong M8-10, gồm contact requiredness, issuer, scope/audience,
  TTL, custody, error/audit/retention và rollout.
- [ ] **Chọn phương án (a)/(b)/(c)/(d)** cho `OD-V1-17`, có chữ ký Security. Kèm contract issue/resolve/reissue tương ứng.
- [ ] **Sơ đồ trust boundary đã duyệt** cho `OD-V1-18`: vault ở đâu, ai vận hành, IVR thấy gì, gateway thấy gì.
- [ ] **Threat model** + **vendor capability statement**: gateway có nhận token mờ không, hay bắt buộc E.164 ở API của nó.
- [ ] **Test TTL / replay / audit đã merge** phía phát token.

## 7. Rủi ro nếu để mở

Ticket này chặn `LAB_REAL_SIM` — tức chặn `P8-1`, và qua đó chặn mọi thứ downstream. Nó cũng là ticket có khả năng **buộc sửa contract** cao nhất: nếu chọn phương án (a) hoặc (c), `IvrConfirmationTaskV1` đổi shape, kéo theo migration, mapper, fixture và toàn bộ test intake. Càng chốt muộn, sửa càng đắt.
