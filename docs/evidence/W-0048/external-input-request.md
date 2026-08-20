# W-0048 — Phiếu lấy đầu vào Sales và one-SIM lab

Trạng thái: **`OWNER_DATA_REQUIRED`**

Nguyên tắc: **không dán raw token, password, private key hoặc số điện thoại thật vào file này, chat hay git**. Chỉ ghi tên secret reference/vault path và alias số test.

## 1. Owner/dev Sales cần trả lời

### 1.1 Quyết định thứ tự

Đánh dấu đúng một lựa chọn:

- [ ] **Khuyến nghị — Lane A trước:** kiểm endpoint Golden Hour hiện hữu trong MOCK, sau đó dev Sales xây Target V1.
- [ ] Lane C trước: dev Sales xây thẳng Target V1 task producer + generic callback + auth.

Người xác nhận: `[TÊN/VAI TRÒ]`

Ngày: `[YYYY-MM-DD]`

### 1.2 Lane A — endpoint hiện hữu

| Đầu vào | Giá trị cần cung cấp |
| --- | --- |
| Sales sandbox/base URL truy cập được từ IVR dev | `[URL KHÔNG CHỨA CREDENTIAL]` |
| Endpoint xác nhận | dự kiến `/api/v1/internal/ivr/golden-hour/callbacks`; dev Sales xác nhận `[YES/NO]` |
| Auth header | dự kiến `X-Internal-Token`; dev Sales xác nhận `[YES/NO]` |
| Secret reference phía IVR | `[TÊN BIẾN/VAULT PATH, KHÔNG GHI GIÁ TRỊ]` |
| Secret reference phía Sales | dự kiến `GOLDEN_HOUR_IVR_CALLBACK_TOKEN`; xác nhận `[YES/NO]` |
| Test `callId` | `[ID DỮ LIỆU TEST]` |
| Test `reservationId` | `[ID DỮ LIỆU TEST]` |
| Test `orderId` | `[ID DỮ LIỆU TEST]` |
| Test `customerId` | `[ID DỮ LIỆU TEST]` |
| Task ID IVR dùng để ánh xạ bốn ID trên | `[UUID TEST]` |
| Kết quả kỳ vọng cho CONFIRMED | `[beforeStatus -> afterStatus]` |
| Kết quả kỳ vọng cho REJECTED/NO_ANSWER/FAILED | `[beforeStatus -> afterStatus]` |
| Cách reset/seed dữ liệu E2E | `[LỆNH/RUNBOOK/OWNER]` |

Dev Sales cần đính kèm hoặc chỉ rõ vị trí:

- runtime OpenAPI đúng version đang deploy;
- ví dụ request + response `200` đã redacted;
- hành vi duplicate cùng `idempotencyKey`;
- hành vi late/deadline-passed và order state conflict;
- correlation/request ID nào có thể dùng tra log hai hệ thống.

### 1.3 Lane C — Target V1 (nếu chọn)

| Đầu vào | Giá trị cần cung cấp |
| --- | --- |
| Sales producer sẽ gọi IVR task endpoint từ event/hook nào | `[EVENT/SERVICE/OWNER]` |
| IVR task base URL trong sandbox | `[URL]` |
| Generic callback URL | `[URL, dự kiến /api/v1/internal/orders/{orderId}/ivr-result-callbacks]` |
| Auth type | `[OAuth2 client credentials / signed JWT / mTLS / khác]` |
| Token URL/issuer | `[URL]` |
| Audience | `[AUDIENCE, dự kiến sales-order-core nếu được duyệt]` |
| Scope | `[SCOPE]` |
| JWKS/certificate trust reference | `[URL/SECRET REF]` |
| Client credential reference | `[VAULT PATH/TÊN SECRET, KHÔNG GHI GIÁ TRỊ]` |
| Task OpenAPI version + immutable artifact | `[PATH/URL + SHA256]` |
| Callback OpenAPI version + immutable artifact | `[PATH/URL + SHA256]` |
| ACK matrix đã duyệt | `[200/409 + semantic codes]` |
| Retry/idempotency TTL | `[GIÁ TRỊ]` |

Tối thiểu cần contract tests cho `ACCEPTED`, `DUPLICATE_ACCEPTED`, `BLOCKED_BY_CORE`, `REVIEW_REQUIRED`, `REJECTED_STALE`, `IDEMPOTENCY_CONFLICT` hoặc bảng thay thế được hai owner ký duyệt.

## 2. Owner/vendor telephony cần trả lời

### 2.1 Chọn topology

Đánh dấu đúng một lựa chọn:

- [ ] **Khuyến nghị cho one-SIM:** Asterisk ARI + SIP-to-GSM gateway một cổng.
- [ ] Direct vendor HTTP/SDK — chỉ chọn khi đã có tài liệu API/SDK và thiết bị cụ thể.
- [ ] SIP trunk preflight trước, rồi GSM gateway — lưu ý SIP trunk không phải evidence SIM thật.

### 2.2 Thiết bị và giao thức

| Đầu vào | Giá trị cần cung cấp |
| --- | --- |
| Hãng/model gateway hoặc modem | `[MODEL]` |
| Firmware/version | `[VERSION]` |
| Serial/asset ID | `[ALIAS, KHÔNG GHI SERIAL NHẠY CẢM NẾU CÓ]` |
| Tài liệu protocol/API/SDK | `[FILE/URL/VERSION]` |
| Asterisk version (nếu dùng) | `[VERSION]` |
| ARI/SIP endpoint + port | `[HOST ALIAS/IP LAB + PORT, KHÔNG CREDENTIAL]` |
| Credential reference | `[VAULT PATH/TÊN SECRET]` |
| DTMF | `[RFC2833/RFC4733, SIP INFO, in-band; xác nhận barge-in]` |
| Audio codec/rate | `[PCMU/PCMA/G722/... + Hz]` |
| Raw disposition codes | `[ANSWERED/BUSY/NO_ANSWER/REJECTED/FAILED/... theo vendor]` |
| Health endpoint/readback | `[CÁCH KIỂM TRA SIM/SIP/CHANNEL]` |
| Reconnect/cooldown/quarantine | `[KHUYẾN NGHỊ VENDOR]` |
| Recording | phải là `OFF`; cách đọc lại `[COMMAND/API]` |
| Caller ID/CDR | `[CÁCH TRA CỨU, RETENTION, REDACTION]` |
| Số kênh đồng thời cho lab | `1` — xác nhận `[YES/NO]` |

### 2.3 SIM và allowlist

Không ghi số thật. Chỉ điền alias và người kiểm soát:

| Alias trong IVR | Nhà mạng | Chủ sở hữu/thiết bị test | Mapping thật nằm ở đâu ngoài IVR |
| --- | --- | --- | --- |
| `LAB-A` | `[CARRIER]` | `[OWNER/DEVICE ALIAS]` | `[ASTERISK DIALPLAN/SECRET STORE]` |
| `LAB-B` | `[CARRIER]` | `[OWNER/DEVICE ALIAS]` | `[LOCATION]` |
| `LAB-C` | `[CARRIER]` | `[OWNER/DEVICE ALIAS]` | `[LOCATION]` |

Gói nghiệm thu đầy đủ yêu cầu tối thiểu ba alias đích trên ít nhất hai nhà mạng và một alias không hợp lệ. Nếu lượt đầu chỉ có một số, evidence phải ghi rõ là preflight rút gọn, chưa phải full lab acceptance.

SIM test cần xác nhận:

- [ ] đã kích hoạt và gọi ra được;
- [ ] còn tiền/hạn sử dụng;
- [ ] không phải số khách hàng;
- [ ] không phải SIM cá nhân quan trọng;
- [ ] owner biết cách tháo/ngắt khẩn cấp.

## 3. Audio/TTS cho lab

Đánh dấu một:

- [ ] **Khuyến nghị:** audio thu sẵn, đi qua một `ITtsProvider` file-playback cần triển khai và privacy guard hiện có.
- [ ] TTS vendor đã được phê duyệt; điền model/provider, data residency, retention, credential reference và pronunciation test pack.

Nếu dùng audio thu sẵn, owner cung cấp file ngoài git hoặc test fixture không có PII thật cho các đoạn: chào, tóm tắt đơn, tổng tiền, khu vực giao rút gọn, mời bấm 1/0, sai phím, xác nhận kết quả, tạm biệt.

## 4. Bốn quyết định an toàn cần owner xác nhận

| Quyết định | Mặc định đề xuất | Xác nhận |
| --- | --- | --- |
| Mapping `alias -> raw phone` nằm ngoài IVR | Asterisk/vendor secret config | `[YES/NO]` |
| Allowlist thay qua config + restart, không qua API trong lab | Có | `[YES/NO]` |
| Không ghi âm, không gọi số khách hàng | Bắt buộc | `[YES/NO]` |
| `REAL_CUSTOMER_CALL_ALLOWED=NO` suốt lab | Bắt buộc | `[YES/NO]` |

Người phê duyệt lab: `[TÊN/VAI TRÒ]`

Người có quyền kill switch/ngắt thiết bị: `[TÊN/VAI TRÒ]`

## 5. Cách bàn giao an toàn

1. Commit file này chỉ với alias, version, URL không có credential và secret reference.
2. Raw secret cấp qua GitLab protected/masked variable hoặc secret store cục bộ được thống nhất.
3. Raw phone chỉ điền trực tiếp trên máy lab vào Asterisk dialplan/vendor config không commit.
4. Gửi tài liệu vendor/OpenAPI artifact; không gửi ảnh có token/password.
5. Sau khi nhận đủ, IVR sẽ kiểm handshake read-only trước rồi mới xin phép chạy đúng một cuộc gọi allowlist.
