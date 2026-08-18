# W-0031 — Evidence: Voice call restriction and trust snapshot (`P4-3`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS` (mock-only — trần của slice này theo DoD `P4-3` §5)

## 1. Điều đáng nói nhất: hai loại fail-closed đóng ngược chiều nhau

Slice này xử lý hai quyết định trông giống nhau nhưng **an toàn theo hướng ngược nhau**:

| | Voice restriction | Trust skip |
| --- | --- | --- |
| Câu hỏi | Có được phép gọi khách này không? | Có được phép **bỏ qua** cuộc gọi không? |
| Thiếu bằng chứng thì | **CHẶN** | **VẪN GỌI** |
| Hại nếu sai | gọi cho khách đã từ chối nhận cuộc gọi | thêm một cuộc gọi xác nhận |

Cả hai đều fail-closed. Nhưng "closed" nghĩa ngược nhau, vì mức thiệt hại khác nhau. Gộp chúng vào một cờ là **âm thầm chọn một trong hai thiệt hại** mà không ai nhìn thấy quyết định đó.

Vì vậy chúng là hai kiểu dữ liệu riêng — `VoiceContactEvidence` và `TrustResolverEvidence` — và `UT-ELIG-TRUST-17` khoá đúng tính chất này bằng một test duy nhất chạy cả hai chiều.

## 2. Tách marketing khỏi voice: từ "quy tắc nhớ bỏ qua" sang "không có gì để đọc"

Trước slice này, `EligibilitySnapshot` có field `SmsOptOut`. Test `UT-ELIG-DNC-02` khẳng định SMS opt-out không chặn cuộc gọi thoại — và nó **xanh**.

Nhưng xanh vì lý do sai: `SmsOptOut` **không được đọc ở bất kỳ rule nào**, và `EligibilityService.Map` truyền cứng `false` cho nó. Tách bạch được chứng minh bởi một quy tắc *tình cờ* không đọc một field chết.

Một quy tắc có thể bị sửa bởi người không biết vì sao nó ở đó. Một field không tồn tại thì không thể đọc.

Giờ `VoiceContactEvidence` **không có** member nào cho SMS, email, marketing, consent, newsletter hay promo — và `UT-ELIG-DNC-02` kiểm điều đó bằng reflection trên chính kiểu dữ liệu. Parser cũng không đọc các khoá đó; `IT-ELIG-VOICE-13` case `marketing-noise` đẩy vào một bag mang **cả năm** tín hiệu marketing ở giá trị hạn chế nhất và khẳng định kết quả không đổi.

Lý do nghiệp vụ, ghi thẳng trong code và schema: **khách từ chối nhận marketing không có nghĩa là từ chối một cuộc gọi xác nhận đơn hàng.** Hai thứ dựa trên cơ sở pháp lý khác nhau.

## 3. Đã xây gì

**Voice (`P4-3` §2.1) — đóng về phía chặn:**

| Điều kiện | Reason code | Quyết định |
| --- | --- | --- |
| `voice_restriction.source_available: false` | `PHONE_CALL_RESTRICTION_SOURCE_UNAVAILABLE` | held |
| không xác định được `restricted` | `PHONE_CALL_RESTRICTION_MISSING` | held |
| `restricted: true` | `PHONE_CALL_RESTRICTED` | blocked |

Nhánh "resolver không trả lời" trước đây **không tồn tại**. Cột DB `call_restriction` là `bool` non-nullable và field trên wire là bắt buộc, nên nhánh `PHONE_CALL_RESTRICTION_MISSING` cũ **không thể chạm tới từ luồng thật** — chỉ unit test dựng được. Giờ trạng thái "nguồn không sẵn sàng" có chỗ biểu diễn thật, trong evidence bag.

**Trust (`P4-3` §2.3) — đóng về phía gọi:**

`TrustResolverEvidence.CanSkip` chỉ đúng khi **tất cả** có mặt: feature bật, Sales cho phép, resolver sẵn sàng, resolver **có version**, có risk evidence, trạng thái `TRUSTED`, và không có risk flag. Thiếu bất kỳ phần nào → vẫn gọi, kèm advisory **nói rõ thiếu phần nào**:

`TRUST_SKIP_DISABLED_REQUIRE_IVR` · `TRUST_RESOLVER_UNAVAILABLE` · `TRUST_RESOLVER_VERSION_MISSING` · `TRUST_RISK_EVIDENCE_UNAVAILABLE`

Nói rõ thiếu phần nào là thứ biến mặc định an toàn thành **mặc định kiểm toán được**, thay vì chỉ là bảo thủ.

**Feature trust skip vẫn tắt.** Bật nó là quyết định owner cần một hợp đồng resolver có phiên bản từ Sales — chưa tồn tại. `IT-ELIG-TRUST-14` dựng đúng trường hợp duy nhất có thể biện minh cho việc bỏ qua cuộc gọi (khách `TRUSTED`, resolver sẵn sàng có version, có risk evidence, không risk flag) và khẳng định **vẫn gọi**. Phần plumbing đã sẵn cho ngày cổng mở; cổng thì chưa mở.

## 4. Test

| Test | Khẳng định |
| --- | --- |
| `UT-ELIG-DNC-02` (viết lại) | restricted chặn; **kiểu dữ liệu voice không có member marketing/consent nào** (reflection) |
| `UT-ELIG-VOICE-15` | resolver không trả lời và "không ai nói" đều chặn, đều không tính lượt khách |
| `UT-ELIG-TRUST-16` | bộ evidence đầy đủ → skip; bỏ **từng** phần một → vẫn gọi + đúng advisory tương ứng; risk flag đơn lẻ cũng huỷ skip |
| `UT-ELIG-TRUST-17` | hai chiều fail-closed ngược nhau trong một test |
| `UT-ARCH-NO-CRM-EGRESS-06` | không có CRM client, không có mutation consent, không có publisher notification trong `src/` |
| `IT-ELIG-VOICE-13` | allowed / restricted / resolver-down / **marketing-noise** chạy thật trên Postgres |
| `IT-ELIG-TRUST-14` | bộ evidence đầy đủ nhất vẫn **không** tạo skip khi cổng owner chưa mở |

`UT-ARCH-NO-CRM-EGRESS-06` cố ý **không** khớp các từ chung như "consent" hay "notification": chúng xuất hiện hợp lệ trong văn bản chính sách và trong guard `v1NotificationEnabled`. Khớp chúng sẽ đỏ vì lý do sai, và một guard đỏ vì lý do sai sẽ bị xoá chứ không được tôn trọng. Nó khớp **tên thứ phải viết ra** để phá ranh giới: `CrmClient`, `UpdateConsent`, `SendSms`, `PublishNotification`…

## 5. Refactor có kiểm soát

`EligibilitySnapshot` giảm từ 21 xuống 17 tham số: 4 field rời (`PhoneCallRestriction`, `SmsOptOut`) và 4 field trust (`CustomerTrustStatus`, `TrustedSkipAllowed`, `RiskFlags`, `TrustSkipFeatureEnabled`) gom thành 2 kiểu có nghĩa.

`impact` trước khi sửa: **LOW**, 13 symbol, 2 direct — `EligibilityService.Map` và test builder. Đúng như dự đoán, chỉ hai chỗ phải sửa.

Parser để lại ở tầng Api chứ **không** đưa xuống Domain, dù để ở Domain thì unit-test được. Lý do: không file nguồn nào trong `Ivr.Domain` dùng `System.Text.Json` — layer này cố ý sạch serialization. Đổi lấy tiện lợi test bằng cách phá layering là một đánh đổi tệ. Thay vào đó, bất biến tách-marketing được chứng minh ở **mức kiểu dữ liệu** (unit, reflection) và ở **mức hành vi** (integration, bag thật).

## 6. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln -warnaserror` | 0 warning / 0 error |
| `dotnet test Ivr.sln` | xem §8 |
| `impact` (GitNexus) | LOW, 13 symbol, 2 direct |
| `detect_changes` (GitNexus) | risk **low**, 63 symbol / 16 file, **0 affected process** |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |
| `openapi-contract-drift.mjs` | `OPENAPI_HASHES_PINNED=3` |
| `validate-openapi.mjs` | schema hợp lệ; negative case vẫn bị từ chối |
| Schema evidence | JSON hợp lệ |

**Không đụng OpenAPI trong slice này.** Shape voice/trust được bổ sung vào cùng linked evidence reference `specs/api/evidence/eligibility-snapshot.v1.schema.json` mà `W-0030` lập ra — cùng lý do: `OD-V1-03` còn mở, không siết contract Sales chưa ký.

## 7. Cái này KHÔNG chứng minh

- **Không đóng `W-0002`…`W-0006`.** Shape voice/trust là **đề xuất của IVR**.
- **Không mở trust skip.** Feature vẫn tắt; bật là quyết định owner cần hợp đồng resolver có phiên bản.
- **Không có sandbox thật.** Real sandbox `NOT_RUN`.
- **Không ghi CRM, không đổi consent, không gửi thông báo nào.** `UT-ARCH-NO-CRM-EGRESS-06` khoá điều này. `P4-5` chứng minh riêng phần notification no-op — slice này không lấn sang.
- **Không đổi order state** (D-02).
- **`TESTS_PASS` là trần.** Chỉ reviewer/owner chuyển `ACCEPTED`.

## 8. Số liệu test

`dotnet test Ivr.sln` — **324/324 pass, 0 fail, 0 skip**:

| Project | Sau W-0030 | Sau W-0031 | Thêm |
| --- | ---: | ---: | ---: |
| `Ivr.ContractTests` | 21 | 21 | 0 |
| `Ivr.UnitTests` | 174 | 178 | +4 |
| `Ivr.IntegrationTests` | 120 | 125 | +5 |
| **Tổng** | **315** | **324** | **+9** |

Một test cũ **bị viết lại**, có chủ ý và theo hướng mạnh hơn: `UT-ELIG-DNC-02` chuyển từ "một rule chọn không đọc field SMS" sang "kiểu dữ liệu không có field nào để đọc" — lý do ở §2. Không test nào bị nới lỏng.
