# T-06 — No-answer, wait-for-timeout và race lúc revalidation

External work `W-0005` · quyết định `OD-V1-06` · gate **real integration** · trạng thái `OPEN`

Owner: **Sales Product/Core**.

Due: chốt **trước pilot `P8-2`** — race chỉ xuất hiện khi có hai hệ thống thật chạy song song. Ngày cam kết của owner: `<owner điền>`.

## 1. Current evidence — đã đọc từ nguồn

**Đề xuất Target V1 hiện tại** — xem [closure-pack index](README.md) và contract được kiểm ở dưới:

- `NO_ANSWER_FINAL` **không** yêu cầu Sales huỷ ngay. Callback là advisory với `recommended_core_action = CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`.
- Sales timeout worker **có thể** chuyển `EXPIRED` khi hết window, nhưng **phải revalidate** state/version/blocker trước khi transition.
- `TECHNICAL_EXCEPTION` tách khỏi no-answer và **không** tính là customer attempt (`DT-02`).

**Contract đã mang đủ tín hiệu để phân biệt.** Trong [`order-core-ivr-callback.target-v1.yaml`](../../../specs/api/openapi/order-core-ivr-callback.target-v1.yaml):

| Tín hiệu | Ý nghĩa |
| --- | --- |
| `IVR_NO_ANSWER_ATTEMPT` | không nghe máy lần này, **còn** lượt |
| `IVR_NO_ANSWER_FINAL` | không nghe máy, **hết** lượt |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | hết window trước khi dùng hết lượt |
| `is_final_for_ivr` | IVR đã xong với task này |
| `is_counted_customer_attempt` | phân biệt `DT-02` |
| `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` | advisory: đừng đổi gì cả |

IVR đã dùng các giá trị này ở [`AdminReadService.cs:64`](../../../src/Ivr.Api/Application/AdminReadService.cs) và [`AnalyticsReadService.cs:87`](../../../src/Ivr.Api/Application/AnalyticsReadService.cs).

## 2. Target delta — chính xác là gì

**(a) "Sales timeout worker **có thể** chuyển `EXPIRED`" — chữ "có thể" phải biến thành hành vi xác định.** Đây là đề xuất của IVR, chưa phải cam kết của Sales. Cần trả lời: sau `NO_ANSWER_FINAL`, đơn nằm ở `CONFIRMING` bao lâu, ai chuyển nó đi, chuyển sang đâu. Nếu không ai chuyển, đơn treo vô thời hạn.

**(b) Ba đồng hồ chạy song song và chưa ai định nghĩa cái nào thắng.**

| Đồng hồ | Ai giữ | Nguồn |
| --- | --- | --- |
| `confirmation_window_expires_at` | Sales đặt trong task | task field |
| `attempt_offsets_seconds` | policy, IVR lên lịch | task field |
| `dial_token_expires_at` | Sales đặt | task field — xem [T-04](T-04-dial-token.md) |

Nếu offset lần 2 rơi **sau** `confirmation_window_expires_at`, IVR nên bỏ lần gọi đó hay vẫn gọi? Hiện IVR chọn bỏ và phát `IVR_CONFIRMATION_WINDOW_EXPIRED`. Cần Sales xác nhận đó là hành vi mong muốn, hoặc sửa policy để offset luôn nằm trong window.

**(c) Race thật, cần test chứ không cần thoả thuận miệng.** Bốn tình huống:

| # | Tình huống | Cần Sales trả lời |
| --- | --- | --- |
| 1 | Khách bấm phím **đúng lúc** window hết hạn | Kết quả có được nhận không, hay `REJECTED_STALE`? |
| 2 | Sales huỷ đơn trong lúc IVR đang gọi | ACK là `BLOCKED_BY_CORE`? Đơn ở state nào? |
| 3 | Khách bấm xác nhận, `order_version` đã bump vì lý do khác | `REJECTED_STALE` hay revalidate rồi nhận? |
| 4 | IVR gửi `NO_ANSWER_FINAL`, Sales chưa expire, khách **gọi lại** tổng đài | Ai thắng? Có cần IVR biết không? |

Tình huống 1 và 3 quyết định một điều rất cụ thể: **khách đã bấm phím rồi mà hệ thống vứt kết quả** — đó là trải nghiệm tệ nhất trong toàn luồng, và nó là quyết định của Sales chứ không phải của IVR.

**(d) `DT-02` phải được Sales tôn trọng, không chỉ IVR.** IVR gửi `is_counted_customer_attempt: false` cho lỗi kỹ thuật. Nếu Sales đếm attempt theo số callback nhận được thay vì theo cờ này, số lần gọi khách thực tế sẽ vượt policy đã duyệt — vi phạm chính cái policy ở [T-09](T-09-attempt-policy.md).

## 3. Sample payload

Không nghe máy, còn lượt:

```json
{
  "result_type": "IVR_NO_ANSWER_ATTEMPT",
  "is_counted_customer_attempt": true,
  "is_final_for_ivr": false,
  "attempt_number": 1,
  "recommended_core_action": "CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT"
}
```

Không nghe máy, hết lượt:

```json
{
  "result_type": "IVR_NO_ANSWER_FINAL",
  "is_counted_customer_attempt": true,
  "is_final_for_ivr": true,
  "attempt_number": 2,
  "recommended_core_action": "CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT"
}
```

Lỗi kỹ thuật — **không** tính lượt (`DT-02`):

```json
{
  "result_type": "IVR_TECHNICAL_EXCEPTION",
  "is_counted_customer_attempt": false,
  "is_final_for_ivr": false,
  "attempt_number": 1,
  "recommended_core_action": "CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT"
}
```

## 4. Acceptance test — phải xanh khi đóng

| Test | Ở đâu | Khẳng định |
| --- | --- | --- |
| `ck_ivr_call_attempts_technical_not_counted` | migration `20260812142435` | `DT-02` được enforce ở tầng database, không chỉ ở code |
| `IT-ELIG-SCHED-09` | `tests/Ivr.IntegrationTests/EligibilityPersistenceTests.cs` | Lịch attempt tôn trọng window |
| `CT-CONTRACT-TARGET-ACK-04` | `tests/Ivr.ContractTests/SalesContractScaffoldTests.cs` | `REJECTED_STALE` không bị transport-retry |
| **`CDC-RACE-01..04`** *(Sales viết)* | phía Sales | Bốn tình huống race ở §2(c), mỗi cái một test |
| **`CDC-DT02-01`** *(Sales viết)* | phía Sales | Sales đếm attempt theo `is_counted_customer_attempt`, không theo số callback |

## 5. Mock fallback

`P2-3` lên lịch attempt theo policy candidate; WireMock trả `REJECTED_STALE` theo kịch bản; `DT-02` đã có CHECK constraint ở database nên không thể lách bằng code. Nhưng **race thật cần hai hệ thống thật** — mock chỉ chứng minh IVR xử lý đúng phía mình.

## 6. Closure artifact — owner điền

- [ ] **Sequence diagram đã duyệt** cho 4 tình huống race ở §2(c), kèm ai thắng trong từng cái.
- [ ] **Cam kết hành vi timeout worker**: sau `NO_ANSWER_FINAL`, bao lâu, chuyển sang state gì, revalidate cái gì trước.
- [ ] **Xác nhận thứ tự ưu tiên ba đồng hồ** ở §2(b).
- [ ] **Runtime test đã merge** phía Sales cho `DT-02` và cho ít nhất tình huống 1 và 3.

## 7. Rủi ro nếu để mở

Không chặn build, nhưng chặn **pilot**. Chạy pilot mà chưa chốt §2(c) nghĩa là mỗi ca race sẽ thành một cuộc điều tra thủ công, và không bên nào có tài liệu để nói ai đúng.
