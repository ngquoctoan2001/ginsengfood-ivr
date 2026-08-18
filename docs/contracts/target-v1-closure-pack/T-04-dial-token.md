# T-04 — Dial-token: issue / resolve / TTL / one-use / audit

External work `W-0004` · quyết định `OD-V1-05`, `OD-V1-17`, `OD-V1-18` · gate **real call** · `LAB_REAL_SIM` · trạng thái `OPEN`

Owner: **Sales** (phát token), **Security** (trust boundary, threat model), **Telephony vendor** (khả năng gateway).

Due: chốt **trước `P8-1`** (real SIM lab) — không quay số thật khi chưa có semantics token. Ngày cam kết của owner: `<owner điền>`.

## 1. Current evidence — đã đọc từ nguồn

**Task mang đúng một token scalar.** [`ivr-order-confirmation.v1.yaml`](../../../specs/api/openapi/ivr-order-confirmation.v1.yaml) — `IvrConfirmationTaskV1`:

```yaml
dial_token: { type: string, description: Opaque token; never a raw phone number. }
dial_token_expires_at: { type: string, format: date-time }
```

Cả hai đều `required`. Không có mảng, không có endpoint reissue, không có refresh — trong **bất kỳ** contract nào của dự án.

**Cùng task đó bắt buộc phải quay ít nhất hai lần.** Cùng schema:

```yaml
max_customer_attempts: { type: integer, minimum: 1, maximum: 10 }
attempt_offsets_seconds: { type: array, minItems: 1, maxItems: 10, items: { type: integer, minimum: 0 } }
```

Cộng thêm `DT-02`: **technical exception không tính là customer attempt**. Nghĩa là số lần quay thật > `max_customer_attempts`.

**Năm tài liệu ghi token là one-use/attempt.** Ví dụ [`specs/data/05-pii-policy.md`](../../../specs/data/05-pii-policy.md) §2: "Token TTL ≤ window, one-use/attempt (D-05); mapping token→số thật nằm ở SIM adapter/token vault, không ở IVR."

**IVR đã dựng seam nhưng chưa có nguồn thật.** [`src/Ivr.Domain/Ports/ProviderPorts.cs:34`](../../../src/Ivr.Domain/Ports/ProviderPorts.cs) khai `IDialTokenResolver`; `DialAuthorization` chỉ lộ số qua `RevealToTrustedGateway()` và `ToString()` trả `[REDACTED_DIAL_AUTHORIZATION]`. Hai implementation hiện có: `FakeDialTokenResolver` và `MockDialTokenVault`.

## 2. Target delta — chính xác là gì

**(a) `OD-V1-17` — một token, nhiều lần quay. Đây là mâu thuẫn số học, không phải mơ hồ.**

| Cần | Có |
| --- | --- |
| ≥ 2 lần quay khách + n lần retry kỹ thuật | 1 token scalar |
| Token one-use/attempt | không có cách lấy token thứ hai |

Bốn phương án, Sales/Security chọn một:

| # | Phương án | Đánh đổi |
| --- | --- | --- |
| a | `dial_tokens[]` per-attempt trong task | Sales phải biết trước số lần quay, kể cả retry kỹ thuật — mà retry kỹ thuật thì không đoán được |
| b | Endpoint reissue/refresh | Thêm một round-trip đồng bộ ngay trước mỗi lần quay; thêm một điểm chết |
| c | Token bundle (n token, n do policy quyết) | Vẫn phải bao được retry kỹ thuật |
| d | Token reusable có TTL + risk control ghi rõ | Bỏ tính chất one-use; cần threat model nói rõ chấp nhận rủi ro gì |

Đây **không** phải quyết định IVR được tự chọn: token là thứ Sales/Security phát ra.

**(b) `OD-V1-18` — resolve `dial_token → E.164` xảy ra ở đâu.** Hai tài liệu nói khác nhau, và [`specs/api/04-sim-adapter-contract.md:18`](../../../specs/api/04-sim-adapter-contract.md) đã ghi thẳng mâu thuẫn này:

> tài liệu này ghi adapter chỉ nhận `dial_token`, trong khi `P2-4` đặt `IDialTokenResolver` bên trong IVR và gateway thương mại quay số E.164.

Ranh giới **mục tiêu** đã được phác: `IVR → opaque dial_token → trusted resolver/gateway → E.164`, và IVR không giữ mapping key. Nhưng "phác" không phải "đã duyệt". Câu hỏi thật là: **cái vault giữ mapping chạy ở đâu, ai vận hành, ai audit nó**. Nếu câu trả lời là "trong process của IVR", thì `D-05` bị vi phạm trên thực tế dù code có che `ToString()` đi nữa.

**(c) TTL chưa nối với window.** `dial_token_expires_at` là timestamp độc lập; quy tắc "TTL ≤ confirmation window" nằm trong tài liệu chứ không có ràng buộc nào trên wire. Nếu Sales phát token hết hạn trước lần quay thứ hai, lần đó chết mà không ai lường trước.

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
| `MockDialTokenVault` suite | `tests/Ivr.IntegrationTests/MockTelephonyPersistenceTests.cs` | Resolve chỉ qua port, không lưu mapping ở IVR |
| **`CDC-DIALTOKEN-01`** *(Sales/Security viết)* | phía phát token | Token hết hạn → resolve fail; token đã dùng → resolve fail theo đúng semantics đã chọn |
| **`CDC-DIALTOKEN-02`** *(Sales/Security viết)* | phía phát token | Replay: cùng token, hai attempt khác nhau → hành vi đúng như phương án đã chốt |

## 5. Mock fallback

`FakeDialTokenResolver` + `MockDialTokenVault` cho phép toàn bộ Phase 2 chạy. Mock **cố tình dễ dãi** về reuse — nó không mô phỏng one-use, vì one-use chưa được định nghĩa. Đây là chỗ mock và thực tế lệch nhau nhiều nhất trong cả dự án; đừng đọc "test xanh" ở đây thành "đã sẵn sàng".

## 6. Closure artifact — owner điền

- [ ] **Chọn phương án (a)/(b)/(c)/(d)** cho `OD-V1-17`, có chữ ký Security. Kèm contract issue/resolve/reissue tương ứng.
- [ ] **Sơ đồ trust boundary đã duyệt** cho `OD-V1-18`: vault ở đâu, ai vận hành, IVR thấy gì, gateway thấy gì.
- [ ] **Threat model** + **vendor capability statement**: gateway có nhận token mờ không, hay bắt buộc E.164 ở API của nó.
- [ ] **Test TTL / replay / audit đã merge** phía phát token.

## 7. Rủi ro nếu để mở

Ticket này chặn `LAB_REAL_SIM` — tức chặn `P8-1`, và qua đó chặn mọi thứ downstream. Nó cũng là ticket có khả năng **buộc sửa contract** cao nhất: nếu chọn phương án (a) hoặc (c), `IvrConfirmationTaskV1` đổi shape, kéo theo migration, mapper, fixture và toàn bộ test intake. Càng chốt muộn, sửa càng đắt.
