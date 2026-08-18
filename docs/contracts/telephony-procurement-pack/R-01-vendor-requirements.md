# R-01 — Yêu cầu nhà cung cấp telephony

External work `W-0008` · quyết định `OD-V1-09`, `OD-V1-18` · gate `G-LAB-SIM`, `G-ESIM32` · trạng thái `OPEN`

Owner: **Infra** (yêu cầu kỹ thuật), **Security** (ranh giới tin cậy), **Procurement** (thương mại).

Due: hoàn thiện **trước khi gửi RFQ**. Ngày cam kết của owner: `<owner điền>`.

> Cách dùng: gửi nguyên file này cho nhà cung cấp. Mỗi bảng có cột cuối để họ điền. Câu trả lời "có, qua tuỳ chỉnh" phải kèm mô tả tuỳ chỉnh là gì và ai chịu chi phí.

## 1. Cổng adapter — đây là hợp đồng, không phải mong muốn

IVR nói chuyện với telephony qua đúng **6 operation** đã có trong code tại [`src/Ivr.Domain/Ports/ProviderPorts.cs:204`](../../../src/Ivr.Domain/Ports/ProviderPorts.cs) (`ISimGateway`). Nhà cung cấp không cần đoán:

| # | Operation | IVR gửi | IVR cần nhận lại | Nhà cung cấp ánh xạ bằng gì? |
| --- | --- | --- | --- | --- |
| 1 | `DialAsync` | `attempt_id`, `sim_channel_id`, lease token + fencing generation, dial authorization, `recording: DISABLED` | `provider_call_reference`, `started_at`, `is_connected` | `<vendor điền>` |
| 2 | `PlayAsync` | audio hoặc text đã render, locale `vi-VN`, `audio_format` | ack | `<vendor điền>` |
| 3 | `CaptureDtmfAsync` | timeout | một phím (`1`/`0`), hoặc "không bấm", hoặc mã lỗi kỹ thuật | `<vendor điền>` |
| 4 | `GetDispositionAsync` | — | disposition, `started_at`, `ended_at`, mã lỗi kỹ thuật, cờ kênh còn khoẻ | `<vendor điền>` |
| 5 | `HangupAsync` | — | — | `<vendor điền>` |
| 6 | `CheckHealthAsync` | `sim_channel_id` | trạng thái kênh, `checked_at`, `cooldown_until`, **`recording_disabled`** | `<vendor điền>` |

Nhà cung cấp nào không cung cấp được operation 6 với cờ `recording_disabled` đọc ngược được thì **không đáp ứng `DT-05`**.

## 2. Protocol, SDK, phiên bản

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Mô hình | GSM gateway phần cứng (AT command) / SIP-to-SIM / API đám mây / khác | `<vendor điền>` |
| Protocol | Tên và phiên bản chính xác | `<vendor điền>` |
| SDK | Ngôn ngữ hỗ trợ; có .NET không; nếu không thì tích hợp qua gì | `<vendor điền>` |
| Phiên bản | Chính sách phiên bản API, thời gian báo trước khi có breaking change | `<vendor điền>` |
| Hỗ trợ | Giờ hỗ trợ, kênh, thời gian phản hồi cam kết | `<vendor điền>` |
| Môi trường | Có sandbox/test riêng không, hay chỉ có production | `<vendor điền>` |

IVR chạy **.NET 10** trên Linux container. Nếu SDK chỉ có cho nền tảng khác, ghi rõ cách tích hợp (REST, gRPC, tiến trình phụ) và ai duy trì lớp đệm đó.

## 3. Xác thực và cấp phát bí mật

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Cơ chế | API key / OAuth client credentials / mTLS / IP allowlist / khác | `<vendor điền>` |
| Vòng đời | Bí mật có hạn không; xoay được không; xoay có downtime không | `<vendor điền>` |
| Phạm vi | Một credential cho toàn tài khoản hay tách được theo kênh/môi trường | `<vendor điền>` |
| Thu hồi | Thu hồi khẩn cấp mất bao lâu có hiệu lực | `<vendor điền>` |
| Lưu trữ | Có yêu cầu gì về nơi IVR giữ bí mật không | `<vendor điền>` |

IVR **không** nhúng bí mật vào image hay repo. Xem thêm phụ thuộc secret store ở `W-0063`.

## 4. Ranh giới `dial_token` — hạng mục loại trừ

`D-05`: IVR không bao giờ giữ mapping `dial_token → số thật`. Ranh giới mục tiêu:

```text
IVR  →  dial_token (mờ)  →  resolver/gateway tin cậy  →  E.164  →  mạng viễn thông
```

| Câu hỏi | Trả lời |
| --- | --- |
| API của nhà cung cấp **bắt buộc** nhận E.164, hay nhận được một định danh mờ do bên thứ ba resolve? | `<vendor điền>` |
| Nếu bắt buộc E.164: có hỗ trợ một thành phần proxy/resolver chạy ngoài IVR không? | `<vendor điền>` |
| Nhà cung cấp có lưu số đã quay không, lưu bao lâu, ai truy cập được? | `<vendor điền>` |
| Có xoá theo yêu cầu được không (PDPA)? | `<vendor điền>` |

Đây là câu hỏi **quyết định `OD-V1-18`** cùng với Security. Xem [T-04](../target-v1-closure-pack/T-04-dial-token.md) §2(b) — mâu thuẫn hiện tại giữa `specs/api/04` và `P2-4` chưa được chốt, và nó cần **vendor capability statement** mới chốt được.

## 5. DTMF

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Chế độ | RFC 2833 / SIP INFO / inband / khác — liệt kê tất cả hỗ trợ | `<vendor điền>` |
| Độ tin cậy | Tỉ lệ bắt nhầm/bỏ sót đo được, trên mạng di động Việt Nam | `<vendor điền>` |
| Timeout | Cấu hình được thời gian chờ phím không; giới hạn bao nhiêu | `<vendor điền>` |
| Barge-in | Khách bấm phím **trong lúc** đang phát câu thoại thì có bắt được không | `<vendor điền>` |
| Nhiều phím | Bấm liên tiếp nhiều phím thì trả về gì | `<vendor điền>` |

IVR chỉ dùng **`1`** và **`0`**. `KEY_9` là `NOT_ENABLED` (`AS-07`) — đừng đề xuất luồng dựa trên nó.

Barge-in là câu hỏi có ảnh hưởng thật: nếu không hỗ trợ, câu thoại phải phát hết mới nhận phím, làm tăng thời lượng cuộc gọi và tỉ lệ khách cúp máy giữa chừng.

## 6. Codec và định dạng audio

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Codec hỗ trợ | Liệt kê đầy đủ (G.711 a-law/µ-law, G.729, Opus…) | `<vendor điền>` |
| Định dạng nạp vào | Nhận file audio dạng gì; sample rate; mono/stereo | `<vendor điền>` |
| Nạp trước hay streaming | Phải upload trước rồi phát theo id, hay stream trực tiếp | `<vendor điền>` |
| Độ dài tối đa | Giới hạn thời lượng một lần phát | `<vendor điền>` |
| TTS sẵn có | Nhà cung cấp có TTS tiếng Việt tích hợp sẵn không | `<vendor điền>` |

Mô hình MOCK hiện tại dùng **mono 8 kHz, 16-bit linear PCM (`audio/L16`)**, khoảng 16 kB/giây — xem [`docs/capacity-model.md`](../../capacity-model.md). **Đó là metadata mô phỏng, không phải codec thật đã hỗ trợ.** Câu trả lời của nhà cung cấp ở mục này quyết định lại toàn bộ mô hình dung lượng, và nối sang [R-05](R-05-tts-audio-capability.md).

## 7. Disposition — ánh xạ thô sang 11 giá trị của IVR

IVR chuẩn hoá mọi kết quả thành 11 giá trị (`SimProviderDisposition` trong [`ProviderPorts.cs:144`](../../../src/Ivr.Domain/Ports/ProviderPorts.cs)). Nhà cung cấp điền cột giữa:

| IVR cần | Trạng thái thô tương ứng của nhà cung cấp | Ghi chú ràng buộc |
| --- | --- | --- |
| `Answered` | `<vendor điền>` | phân biệt được "máy người" và "hộp thư thoại" không? |
| `RingTimeout` | `<vendor điền>` | cấu hình được số giây đổ chuông không |
| `Busy` | `<vendor điền>` | → `IVR_NO_ANSWER`, **có** tính lượt khách |
| `Rejected` | `<vendor điền>` | khách bấm từ chối → `IVR_NO_ANSWER`, **không** phải cancel |
| `Unreachable` | `<vendor điền>` | → `IVR_INVALID_PHONE_FINAL`, **không** tính lượt |
| `InvalidDestination` | `<vendor điền>` | thuê bao không tồn tại / sai số |
| `Dropped` | `<vendor điền>` | rớt giữa chừng → lỗi kỹ thuật |
| `NetworkError` | `<vendor điền>` | → `IVR_TECHNICAL_EXCEPTION`, **không** tính lượt |
| `SimError` | `<vendor điền>` | |
| `AudioError` | `<vendor điền>` | |
| `DtmfError` | `<vendor điền>` | |

Hai điểm dễ sai và tốn tiền nếu sai:

- **`Rejected` không phải là "khách huỷ đơn".** Khách bấm nút từ chối cuộc gọi là *không nghe máy*, và **được** tính là một lần gọi khách. Ánh xạ nhầm thành cancel là huỷ đơn của khách không hề yêu cầu.
- **`DT-02`: lỗi kỹ thuật không tính là lượt gọi khách.** Ràng buộc này được enforce ở tầng database (`ck_ivr_call_attempts_technical_not_counted`), nên ánh xạ sai sẽ làm ghi dữ liệu thất bại chứ không âm thầm trôi qua.

Danh sách trạng thái thô **phải được xác minh lại trên thiết bị thật** (`DT-01`) — bảng nhà cung cấp cung cấp trên giấy chỉ là điểm khởi đầu.

## 8. Kênh, đồng thời và sức khoẻ

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Một SIM một cuộc | Một SIM có gọi được đồng thời >1 cuộc không? (IVR giả định **không**) | `<vendor điền>` |
| Định danh kênh | Kênh có id ổn định không; id đổi khi nào | `<vendor điền>` |
| Bật/tắt kênh | Tắt một kênh qua API được không; cuộc đang chạy thì sao | `<vendor điền>` |
| Health API | Có endpoint kiểm tra sức khoẻ từng kênh không; trả về gì | `<vendor điền>` |
| Cooldown | Có khái niệm cooldown sau mỗi cuộc không | `<vendor điền>` |
| Quota | Giới hạn cuộc/phút, cuộc/giờ, cuộc/ngày trên mỗi SIM và trên tài khoản | `<vendor điền>` |

IVR đã có sẵn mô hình trạng thái kênh và đang phơi qua `GET /sim-channels`: `enabled`, `status`, `busy`, `fail_count`, `quarantined`, `quarantine_until`, `cooldown_until`, `last_health_check_at`, `disabled_reason`. Mặc định `MockTelephonyDispatchGateway`: cooldown **5 giây**, `fail_count ≥ 3` → quarantine (`DT-04`). Các số này là **mặc định kỹ thuật**, sẽ chỉnh theo giới hạn thật của nhà cung cấp.

Concurrency dùng **lease token + fencing generation** — một cuộc gọi cũ không thể "sống lại" và chiếm kênh sau khi lease đã chuyển. Nhà cung cấp cần cho biết `provider_call_reference` có đủ để phân biệt hai cuộc trên cùng kênh cách nhau vài giây không.

## 9. Caller ID

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Hiển thị | Số hiện trên máy khách là số nào; đặt được không | `<vendor điền>` |
| Tên thương hiệu | Hỗ trợ hiển thị tên doanh nghiệp không; thủ tục và chi phí | `<vendor điền>` |
| Nhất quán | Nhiều SIM thì khách thấy nhiều số khác nhau, hay gộp được về một | `<vendor điền>` |
| Chặn spam | Nhà mạng có gắn nhãn/chặn số gọi tự động không; kinh nghiệm thực tế | `<vendor điền>` |
| Pháp lý | Yêu cầu đăng ký gì với nhà mạng/cơ quan quản lý | `<vendor điền>` |

Nhất quán caller ID là rủi ro business thật, không phải chi tiết kỹ thuật: khách nhận cuộc gọi từ một số lạ khác nhau mỗi lần thì tỉ lệ nghe máy giảm, và đó chính là chỉ số mà toàn bộ dự án này tồn tại để cải thiện.

## 10. CDR và đối soát

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| CDR | Có bản ghi chi tiết cuộc gọi không; lấy qua API hay file | `<vendor điền>` |
| Trường dữ liệu | Liệt kê trường; có `provider_call_reference` để nối với `attempt_id` của IVR không | `<vendor điền>` |
| Độ trễ | CDR có sẵn sau bao lâu | `<vendor điền>` |
| Lưu trữ | Nhà cung cấp giữ CDR bao lâu; xoá theo yêu cầu được không | `<vendor điền>` |
| Đối soát | Cơ chế đối soát khi số liệu của hai bên lệch nhau | `<vendor điền>` |

CDR là bằng chứng duy nhất nối cước phí với cuộc gọi. Không có `provider_call_reference` nối được sang `attempt_id` thì mọi tranh chấp hoá đơn đều không giải quyết được.

## 11. Ghi âm — hạng mục loại trừ

| Câu hỏi | Trả lời |
| --- | --- |
| Ghi âm có tắt được hoàn toàn ở mức API không | `<vendor điền>` |
| Trạng thái tắt có **đọc ngược lại** được qua health không | `<vendor điền>` |
| Mặc định của tài khoản mới là bật hay tắt | `<vendor điền>` |
| Nếu bật, audio lưu ở đâu, bao lâu, ai truy cập | `<vendor điền>` |

`DT-05`: recording **OFF** mặc định. `dial()` mang `recording: DISABLED` và giá trị khác bị từ chối fail-closed. Nhà cung cấp không tắt được, hoặc tắt được nhưng không xác nhận lại được, thì loại.

## 12. Thương mại, SLA và bảo mật

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Giá | Cấu trúc: thuê bao / phút / cuộc / kênh; block cước | `<vendor điền>` |
| Chi phí thiết lập | Thiết bị, lắp đặt, tích hợp | `<vendor điền>` |
| SLA | Uptime cam kết, cách đo, bồi thường | `<vendor điền>` |
| Sự cố | Quy trình báo sự cố, thời gian phản hồi theo mức độ | `<vendor điền>` |
| Bảo mật | Chứng chỉ, mã hoá khi truyền, cô lập tài khoản | `<vendor điền>` |
| Dữ liệu | Dữ liệu lưu ở đâu (trong nước / ngoài nước); có DPA không | `<vendor điền>` |
| Thoát | Điều kiện chấm dứt, khoá tài khoản, lấy lại dữ liệu | `<vendor điền>` |

## 13. Closure artifact

`OD-V1-09` và `OD-V1-18` chỉ đóng khi có:

- [ ] **Bản trả lời đầy đủ file này** từ ít nhất 2 nhà cung cấp, để [R-04](R-04-scorecard-and-gaps.md) có gì để so.
- [ ] **Vendor capability statement** cho §4 — bằng văn bản, vì Security cần nó để chốt `OD-V1-18`.
- [ ] **Bảng ánh xạ disposition §7** đã điền, kèm cam kết báo trước khi danh sách trạng thái thô thay đổi.
- [ ] **Xác nhận §11** bằng văn bản: recording tắt được và đọc ngược được.

Báo giá và bản trả lời RFQ **không** đóng `W-0008`. Phần lab chỉ đóng sau evidence `P8-1`/`P8-2`; phần production chỉ đóng sau khi mua và đo thật.
