# IR-07 — Phiếu chốt một lần: Module 3 ↔ IVR (Module 8)

**Phiên bản contract:** `1.0.0-draft.25` · **Ngày phát:** 2026-09-10 · **Người phát:** owner IVR

---

## Cách dùng phiếu này — đọc trước, 60 giây

Phiếu này được viết để **chốt trong đúng một vòng**. Không có mục nào yêu cầu Module 3 tự soạn
phương án từ đầu: **mỗi mục đã có sẵn vị trí của IVR**, kèm lý do và hậu quả nếu chọn khác.

| Luật | Nội dung |
| --- | --- |
| **1** | Mỗi mục ở Phần B có ô **Trả lời**. Điền đúng một trong hai: `ĐỒNG Ý`, hoặc `KHÁC:` + giá trị của M3. |
| **2** | **Không điền = đồng ý.** Vị trí của IVR ở mục đó thành chốt, và IVR triển khai theo nó. |
| **3** | Ngoài phiếu này, **IVR không hỏi gì thêm ở vòng này**. Phần A là thông báo, Phần D/E là việc sau khi ký. |
| **4** | Trả lại **chính file này** đã điền, một lần, kèm chữ ký ở cuối. |
| **5** | Nếu một mục nào đó M3 chưa quyết được, ghi `KHÁC: cần thêm <chính xác thứ cần>` — nêu **cái cần**, không nêu "cần bàn thêm". |

> **Vì sao có luật 2.** Vòng qua lại là thứ đắt nhất trong tích hợp hai module. Điều khoản im lặng
> chuyển chi phí của việc *không trả lời* về đúng chỗ: nếu M3 không phản đối, IVR cứ thế làm, và M3
> không phải trả lời những mục mình vốn không có ý kiến khác.

---

## Phần 0 — Bốn thứ cần lấy trước khi đọc tiếp

| # | Lấy gì | Đường dẫn chính xác trong repo IVR |
| ---: | --- | --- |
| 1 | Contract intake **hiện hành** | `specs/api/openapi/ivr-order-confirmation.v1.yaml` — `1.0.0-draft.25` |
| 2 | Contract callback | `specs/api/openapi/order-core-ivr-callback.target-v1.yaml` |
| 3 | **Đọc trước khi sinh client** — hai bản đều **có breaking** | `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.23-to-v1.0.0-draft.24.md` và `…draft.24-to-v1.0.0-draft.25.md` |
| 4 | Fixture để tự kiểm producer trước khi gọi thật | `seed/sales-target-v1.sample.json` |
| 5 | Nhãn tiếng Việt mọi enum (nếu dựng console) | `specs/ui/enum-labels.vi.json` + đặc tả màn hình `specs/ui/` |
| 6 | Tài liệu tham chiếu đầy đủ (~1.370 dòng, **không** cần đọc hết để ký phiếu này) | `integration-requirements/06-module-3-api-handover.md` |

**Fixture ở mục 4 có sẵn 35 ca:** 9 task hợp lệ, 13 ca sai schema (đều trả `400 IVR_MALFORMED_REQUEST`),
13 ca hợp lệ schema nhưng bị domain từ chối. Chạy producer của M3 qua bộ này trước buổi lab thì
gần như mọi lỗi tích hợp thường gặp lộ ra trước, không tốn một cuộc gọi nào.

### Ba thay đổi **breaking** cần biết trước khi code

Ở `draft.24`:

1. **`phone_validation_status`** thành `required` + `enum: [VALID]`. Gửi giá trị khác nay bị **schema**
   chặn: `400 IVR_MALFORMED_REQUEST`, **không còn** `422 IVR_CONTACT_INVALID`.
2. **`X-Correlation-Id`** và **`Idempotency-Key`** bị siết cú pháp: `1..128` ký tự trong
   `[A-Za-z0-9._:-]`. Header đang hợp lệ vẫn hợp lệ; ký tự lạ nay bị từ chối sớm hơn.

Ở `draft.25` — **thay đổi này sinh ra vì phiếu này**:

3. **Ba header runtime vẫn luôn dùng nay được khai trong contract.** Trước đó chúng chỉ nằm trong
   phần mô tả security scheme, nên **client sinh từ `draft.24` không gửi chúng** và bị từ chối:

   | Header | Trước | Nay |
   | --- | --- | --- |
   | `X-Action-Reason` | **Bắt buộc** ở cả **8** endpoint `danger`, runtime chặn nếu thiếu — nhưng client sinh ra không biết | khai `required: true`, `1..500` ký tự |
   | `X-Script-Permissions` | Đọc để cấp quyền script; thiếu = không có quyền nào, nên **mọi** thao tác script bị từ chối | khai `required: false`, ≤ 512 ký tự |
   | `X-Destination-Ref` | Provenance tuỳ chọn cho kill switch | khai `required: false`, ≤ 128 ký tự |

   `oasdiff` gọi đây là **8 breaking** (`new-required-request-parameter`). Phân loại ấy đúng, nhưng
   đây là loại breaking hiếm gặp: nó **sửa** client chứ không phá — client cũ vốn đã bị từ chối ở 8
   route đó, chỉ là không có cách nào biết vì sao. **Sinh lại client từ `draft.25` là xong.**

---

## Phần A — Đã chốt rồi, M3 chỉ cần biết (không phải câu hỏi)

Những mục dưới đây **đã có chữ ký**, đã nằm trong code và có test giữ. Ghi ra để M3 khỏi mất thời
gian hỏi lại, và để nếu M3 thấy mục nào không dùng được thì **nói ngay ở vòng này**.

| # | Hạng mục | Giá trị đã chốt | Nguồn ký |
| ---: | --- | --- | --- |
| A-1 | Chính sách attempt — Golden Hour | **2** cuộc, mốc `[0s, 150s]`, cửa sổ **5 phút** | `OD-V1-08`+`OD-V1-16` · 2026-09-05 |
| A-2 | Chính sách attempt — 24/7 | **2** cuộc, mốc `[0s, 450s]`, cửa sổ **15 phút** | `OD-V1-08`+`OD-V1-16` · 2026-09-05 |
| A-3 | Tên version chính sách trên dây | `gh-247-prod-v1` | `W-0198` |
| A-4 | Giờ được phép gọi | **08:00–21:08** giờ địa phương (UTC+7). `21:07` còn trong, `21:08` ngoài | `OD-V1-16`, sửa ở `W-0220` |
| A-5 | `dial_token` | Sales cấp lúc tạo task; **dùng lại được**, gắn cứng `task_id`; TTL = **đúng** `confirmation_window_expires_at`; trần resolve = `max_customer_attempts` + trần technical retry; mỗi lần resolve ghi audit kèm `attempt_id` | `OD-V1-05`/`17`/`18` · 2026-09-05, vế TTL chốt lại 2026-09-09 |
| A-6 | Vị trí resolve `dial_token → E.164` | **Trong IVR**, trong biên adapter telephony. Số E.164 **chỉ tồn tại trong bộ nhớ tiến trình** cho đúng một lần quay số: không DB, không log, không evidence, không callback | `OD-V1-18` · 2026-09-05 |
| A-7 | Auth production | JWT **ký khóa bất đối xứng**, JWKS, TTL token **≤ 10 phút**, scope bắt buộc `ivr.task.write`. Token tĩnh dùng chung **bị từ chối**. mTLS **hoãn** tới khi có hạ tầng thật | `OD-V1-07` · 2026-09-05 |
| A-8 | Lời thoại | **Không dùng vendor TTS lúc chạy.** Kịch bản cố định thu giọng người; chỉ ghép mã đơn, tiền, vùng giao từ ngân hàng ghi âm. **Không đọc tên khách** | `OD-V1-19` · 2026-09-05 |
| A-9 | Ghi âm cuộc gọi | **TẮT vĩnh viễn** ở V1. Metadata cuộc gọi giữ **90 ngày** | `OD-V1-11` · 2026-09-05 |
| A-10 | IVR **không bao giờ** hủy đơn | `IVR_NO_ANSWER_FINAL` là **khuyến nghị**; Core không đổi trạng thái, đơn tự hết hạn theo timeout của M3 | `OD-V1-06` · 2026-09-05 |
| A-11 | Thu hồi đơn | IVR đã dựng **hai fence** (claim + lần đọc cuối trước khi quay số). Endpoint nhận lệnh thu hồi đi cùng lượt phát hành contract kế tiếp | `W-0248`/`W-0249` |
| A-12 | 23 field bắt buộc trên wire | Xem `IR-06 §3.4`. Có gate CI so bảng này với spec, nên nó không lệch được | `FREEZE-03` |
| A-13 | Ranh giới opt-out | **explicit-only**: chỉ coi là opt-out khi khách phát tín hiệu **tường minh**. **Không** suy ra từ số lần khách từ chối. Lưu ý thực tế: **V1 chưa có tín hiệu tường minh nào** — `DTMF-0` là phím **hủy đơn**, phím 9 ngoài scope. Nên ở V1, **không có opt-out**; M3 đừng dựng luồng trông chờ nó | `OD-V1-23` · 2026-09-10 |
| A-14 | Duyệt lời thoại cho production | Chính sách đã chốt, nhưng `PRODUCTION_REAL` đòi **ba actor id khác nhau**, nên khâu duyệt script production **hiện vẫn chặn** phía IVR. Không ảnh hưởng intake/callback | `OD-V1-11` · 2026-09-10 |

Sổ quyết định `specs/_review/open-decisions-register.md` nay còn **2 mục mở** trên tổng 28
(`OD-V1-09` chờ SIM thật, `OD-V1-10` cố ý chưa ký vì con số dung lượng chưa được đo). **Không mục
nào trong hai mục ấy chặn tích hợp M3.**

> **Nếu M3 phản đối mục nào ở Phần A, nói ở vòng này.** Sau khi ký, mở lại một mục Phần A là mở lại
> một contract đã đóng băng — tốn hơn nhiều lần so với nói bây giờ.

---

## Phần B — Câu Module 3 phải trả lời

21 mục. Mỗi mục: câu hỏi, vị trí IVR đề xuất, hậu quả nếu chọn khác, và ô trả lời.

### Nhóm B1 — Producer phía Module 3

---

**`M3-01` · Producer đặt `ivr_confirmation_required=true` ở bước nào, và có đường nào gửi `false` sang IVR không?**

| | |
| --- | --- |
| **IVR đề xuất** | Producer đặt `true` **tại thời điểm chuyển đơn sang `CONFIRMING`**, sau khi đã chạy xong lọc khách cũ/mới và risk policy. Đơn không cần gọi thì **không gửi task** — không gửi task với `false`. |
| **Vì sao** | `ivr_confirmation_required` trên OpenAPI là `enum: [true]`. Gửi `false` bị **schema** chặn (`400`), nên "gửi false để IVR tự bỏ qua" không phải một đường đi được. |
| **Nếu M3 chọn khác** | Nếu M3 muốn gửi cả đơn không cần gọi, phải mở lại enum — là breaking change cho contract đã publish, và IVR sẽ phải thêm nhánh quyết định mà `IR-06 §8` nói rõ là **không** thuộc IVR. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-02` · Producer rẽ nhánh theo trường `decision` trong body, hay theo HTTP status?**

| | |
| --- | --- |
| **IVR đề xuất** | Rẽ nhánh theo **`decision`**. HTTP `200` **không** có nghĩa là "đã tạo cuộc gọi". |
| **Vì sao** | `200` mang **năm** kết quả khác nhau, trong đó có hai kết quả **không** tạo cuộc gọi nào. Rẽ theo status sẽ coi task bị giữ lại là đã gọi. |
| **12 giá trị `decision`** | `TASK_ACCEPTED_CALL_JOB_CREATED` · `TASK_ACCEPTED_DRY_RUN_ONLY` · `TASK_SKIPPED_TRUSTED_CUSTOMER` *(legacy, không phát nữa)* · `TASK_REJECTED_NOT_OFFICIAL_ORDER` · `TASK_REJECTED_STATE_NOT_CALLABLE` · `TASK_REJECTED_POLICY_MISMATCH` · `TASK_REJECTED_CONTACT_INVALID` · `TASK_REJECTED_SCRIPT_NOT_APPROVED` · `TASK_REJECTED_INVALID_TRACE` · `TASK_BLOCKED_OPERATIONAL` · `TASK_HELD_ADMIN_REVIEW` · `TASK_HELD_POLICY_MISSING` |
| **IVR đề xuất cách xử lý** | `*_ACCEPTED_*` → chờ callback. `*_REJECTED_*` → **không** retry mù, sửa dữ liệu rồi gửi lại với `Idempotency-Key` **mới**. `TASK_BLOCKED_OPERATIONAL` → **retry được**, đây là trạng thái tạm (kill switch/hết dung lượng). `TASK_HELD_*` → **không** retry, chờ người xử lý phía IVR. |
| **Nếu M3 chọn khác** | Retry mù trên `*_REJECTED_*` sẽ lặp vô hạn: nguyên nhân là dữ liệu, không phải thời điểm. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-03` · `order_version` bump khi nào?**

| | |
| --- | --- |
| **IVR đề xuất** | Bump **mỗi lần thay đổi bất kỳ thứ gì IVR đọc để gọi**: món hàng, số lượng, tổng tiền, vùng giao, số điện thoại, trạng thái đơn. |
| **Vì sao** | IVR trả nguyên `order_version` trong callback (`order_version_seen_by_ivr`) để M3 phát hiện kết quả cũ. Nếu đơn đổi mà version không đổi, M3 **không phân biệt được** kết quả của bản cũ với bản mới — và sẽ chấp nhận một xác nhận cho nội dung khách không nghe. |
| **Ghi chú kỹ thuật** | IVR coi `order_version` là **chuỗi mờ**, trả nguyên, **không so sánh thứ tự**. Thứ tự thuộc về M3. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-04` · Ba chuỗi đang lệch — M3 map ở lớp assembler trước khi gọi?**

| | |
| --- | --- |
| **IVR đề xuất** | **M3 map khi gửi.** Chi tiết ở **Phần C**. IVR **không** nhận thêm biến thể. |
| **Vì sao** | Nhận hai chuỗi cho một khái niệm nghĩa là từ đó dữ liệu lưu có hai dạng, và mọi truy vấn/báo cáo phải nhớ cả hai. Map một điểm phía producer thì kiểm bằng một test. |
| **Nếu M3 chọn khác** | Mở enum là breaking change cho contract đã publish, và đẩy chi phí sang mọi nơi đọc field đó, mãi mãi. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-05` · `delivery_area_short` chuẩn hóa thế nào?**

| | |
| --- | --- |
| **IVR đề xuất** | Gửi **tên tỉnh/thành**, không gửi phường/xã/đường/số nhà. IVR đọc vùng giao từ **ngân hàng ghi âm 34 tỉnh** đã thu sẵn. |
| **Vì sao** | Lời thoại là ghi âm giọng người, không phải TTS. Một chuỗi ngoài 34 tỉnh thì **không có clip để phát**. Ngoài ra `PiiGuard` chặn địa chỉ chi tiết ở tầng intake. |
| **Nếu M3 chọn khác** | Gửi cấp quận/huyện → task bị từ chối `422 IVR_PII_POLICY_VIOLATION`, hoặc lọt vào nhưng không có clip vùng giao để phát. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-06` · Giới hạn `items[]` trong `privacy_safe_order_summary`?**

| | |
| --- | --- |
| **Giới hạn đang thực thi** | `items[]` tối đa **100** dòng (`SpeechSummaryLimits`, `ServiceCollectionExtensions.cs:85`); `public_name` ≤ **160** ký tự, `unit_label` ≤ **40** (OpenAPI). Đây là con số **kỹ thuật**, đã kiểm trong code. |
| **IVR đề xuất** | Giữ **≤ 5 dòng** trong thực tế, và `public_name` là **tên bán hàng công khai** — không phải mã nội bộ, không kèm ghi chú. |
| **Vì sao đề xuất thấp hơn giới hạn** | Lời thoại là ghi âm giọng người đọc **từng dòng**. 100 dòng lọt qua validation nhưng cuộc gọi sẽ dài hơn cửa sổ xác nhận (5 phút Golden Hour), và khách cúp máy trước khi nghe hết. Giới hạn kỹ thuật không bảo vệ được điều đó. |
| **Nếu M3 chọn khác** | Nêu số dòng tối đa **thực tế** M3 cần; IVR tính lại thời lượng thoại và trả lời trong **cùng** vòng này. |
| **Trả lời** | ☐ ĐỒNG Ý (≤ 5) ☐ KHÁC: số dòng tối đa thực tế = ______ |

---

**`M3-07` · `golden_hour_session_id` — namespace, thời điểm phát, tính duy nhất, ổn định qua retry/replay?**

| | |
| --- | --- |
| **IVR đề xuất** | **Bắt buộc, khác null** với `GOLDEN_HOUR`; **vắng mặt** với `TWENTY_FOUR_SEVEN`. Sinh **một lần** khi mở phiên Giờ Vàng, **không đổi** qua mọi retry/replay của cùng phiên. Duy nhất trong phạm vi M3. |
| **Vì sao** | IVR dùng nó để gom các cuộc gọi cùng một phiên khuyến mãi. Nếu nó đổi giữa các lần retry, một phiên bị đếm thành nhiều phiên. |
| **Nếu M3 chọn khác** | Nếu M3 dùng tên field khác, trả **đúng một** tên thay thế + nguồn nghiệp vụ. **Không gửi hai alias** — IVR sẽ không nhận cả hai. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

### Nhóm B2 — Callback về Module 3

---

**`M3-08` · Endpoint callback dùng chung cho cả hai chương trình?**

| | |
| --- | --- |
| **IVR đề xuất** | **Một** endpoint generic: `POST {sales}/api/v1/internal/orders/{orderId}/ivr-result-callbacks`, phục vụ **cả** Golden Hour và 24/7. |
| **Vì sao** | Endpoint compat riêng của Golden Hour **không** dùng thay được: nó không mang `order_version_seen_by_ivr`, tức M3 mất khả năng phát hiện kết quả cũ (xem `M3-03`). |
| **M3 phải giao** | OpenAPI **authoritative** của endpoint này. IVR không tự đoán shape. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-09` · ACK taxonomy và body — đúng như contract?**

| | |
| --- | --- |
| **IVR đề xuất** | Giữ nguyên contract. **`200`**: `ACCEPTED` · `DUPLICATE_ACCEPTED` · `BLOCKED_BY_CORE` · `REVIEW_REQUIRED`. **`409`**: `REJECTED_STALE` · `IDEMPOTENCY_CONFLICT`. Body bắt buộc ba field: `code`, `callback_id`, `correlation_id`. |
| **Bẫy đã có thật** | Trả `DUPLICATE_ACCEPTED` kèm **`409`** là **dead letter vĩnh viễn**: contract buộc mã đó đi với `200`. Mỗi replay đúng sẽ bị chôn. Đã có gate `FREEZE-06` chặn ở phía IVR, nhưng phía M3 thì chỉ có M3 kiểm được. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-10` · Ranh giới idempotency và thời gian giữ key?**

| | |
| --- | --- |
| **IVR đề xuất** | Khóa theo **`Idempotency-Key`** IVR gửi, giữ **tối thiểu 7 ngày**. Cùng key + cùng body → `200 DUPLICATE_ACCEPTED`. Cùng key + **khác** body → `409 IDEMPOTENCY_CONFLICT`. |
| **Vì sao** | 7 ngày phủ trọn mọi lần retry kỹ thuật cộng thời gian xử lý sự cố cuối tuần. Ngắn hơn thì một replay muộn thành bản ghi trùng. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: giữ ______ ngày |

---

**`M3-11` · M3 revalidate những gì trước khi đổi trạng thái đơn?**

| | |
| --- | --- |
| **IVR đề xuất** | Trước mỗi transition, kiểm đủ **sáu**: `order_version`, trạng thái đơn, tồn kho, thu hồi/recall, `sale_lock`, quality-hold. |
| **Vì sao** | Callback của IVR là **tín hiệu**, không phải lệnh. Khách xác nhận lúc `T`, M3 xử lý lúc `T+n`; trong khoảng đó đơn có thể đã đổi. IVR **không** biết những điều kiện này và không thể kiểm hộ. |
| **Nếu M3 chọn khác** | Bỏ bớt điều kiện nào thì ghi rõ điều kiện đó và ai chịu rủi ro tương ứng. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-12` · M3 tôn trọng `is_counted_customer_attempt`?**

| | |
| --- | --- |
| **IVR đề xuất** | Có. Chỉ đếm là "đã tiếp cận khách" khi field này `true`. |
| **Vì sao** | Một lần quay số hỏng vì lý do kỹ thuật **không** phải một lần làm phiền khách. Đếm nhầm sẽ khiến M3 kết luận đã gọi đủ trong khi khách chưa từng nghe chuông. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-13` · Sau `IVR_NO_ANSWER_FINAL`, M3 chờ bao lâu rồi cho đơn hết hạn?**

| | |
| --- | --- |
| **IVR đề xuất** | M3 tự chọn con số, nhưng phải **≥ cửa sổ xác nhận** của chương trình tương ứng (**5 phút** Golden Hour, **15 phút** 24/7) tính từ lúc nhận callback. Đề xuất: **24 giờ**. |
| **Vì sao** | `OD-V1-06` đã chốt: IVR **không bao giờ** hủy đơn. Đơn hết hạn là hành vi của M3, nên con số phải do M3 chọn — nhưng ngắn hơn cửa sổ xác nhận thì đơn chết trước khi cuộc gọi cuối kịp về. |
| **Trả lời** | ☐ ĐỒNG Ý (24 giờ) ☐ KHÁC: ______ |

---

**`M3-14` · M3 gọi endpoint thu hồi khi đơn bị hủy hoặc bật `sale_lock`?**

| | |
| --- | --- |
| **IVR đề xuất** | **Có.** Payload: `task_id` + `order_version` + `reason`. IVR đã dựng sẵn hai fence phía trong; thiếu lệnh từ M3 thì chúng không bao giờ kích hoạt. |
| **Giới hạn phải nói trước** | Fence thu hồi **giảm** cửa sổ rủi ro từ "cả cửa sổ xác nhận" xuống "vài mili-giây", **không** làm nó bằng không. Một cuộc gọi ra ngoài không phải thao tác giao dịch: revoke rơi đúng vào khoảnh khắc giữa lần đọc cuối và lúc quay số thì vẫn lọt. Ghi ra đây để sáu tháng sau không ai đọc thành "đã chặn được". |
| **Trạng thái** | Endpoint đi cùng lượt phát hành contract kế tiếp. IVR cần **câu trả lời của M3 ở phiếu này** để chốt shape trước khi mở OAS. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-15` · Khi M3 trả `429`, có kèm `Retry-After` hợp lệ không?**

| | |
| --- | --- |
| **IVR đề xuất** | Có. IVR sẽ **tôn trọng** `Retry-After`: lần thử kế tiếp không sớm hơn mốc header chỉ định. |
| **Vì sao** | Không có header thì IVR chỉ còn backoff mặc định, và lúc M3 quá tải thì đó đúng là lúc backoff mặc định sai nhất. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

### Nhóm B3 — Bề mặt quản trị (`IR-06 §4A`)

---

**`M3-16` · M3 có dựng giao diện quản trị IVR không?**

| | |
| --- | --- |
| **IVR đề xuất** | **Có, M3 dựng.** IVR đã **xoá** thư mục console khỏi repo (`W-0253`) và không giữ màn hình đăng nhập, bảng tài khoản hay endpoint `/api/auth/*` nào. |
| **IVR giao sẵn** | **31 endpoint** chia ba tầng (**15** `read`, **5** `write` + **3** dev ngoài production, **8** `danger`), cộng bộ nhãn tiếng Việt **42 họ enum** ở `specs/ui/enum-labels.vi.json` và **8 đặc tả màn hình** ở `specs/ui/` (`01-dashboard` … `08-role-permission-ui`, cộng `00-index`). |
| **Nếu M3 trả lời không** | IVR sẽ không có bề mặt vận hành nào ngoài API trần — nêu rõ ai vận hành kill switch và duyệt lời thoại. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-17` · Ba token quản trị cất ở đâu, ai xoay vòng, chu kỳ nào?**

| | |
| --- | --- |
| **IVR đề xuất** | `IVR_ADMIN_READ/WRITE/DANGER_TOKEN` cất trong secret store của M3, **không** hard-code, **không** để trong biến môi trường của trình duyệt. Xoay vòng **90 ngày**, và ngay lập tức khi có người rời nhóm. |
| **Lưu ý** | Ba token này **tách rời** JWT nghiệp vụ ở `A-7`, có chủ đích: token cầm giao diện quản trị **không** được đồng thời giao được task. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-18` · Vai trò nào của M3 ánh xạ sang tầng nào, và vai nào được chạm `danger`?**

| | |
| --- | --- |
| **IVR đề xuất** | `read` → mọi nhân viên vận hành. `write` → trưởng nhóm vận hành. **`danger` → chỉ owner/quản lý cấp trên**, và phải là người **không** đồng thời duyệt nội dung lời thoại. |
| **Vì sao** | Tầng `danger` chứa kill switch và cắt cuộc gọi đang chạy. Một người vừa duyệt lời thoại vừa cắt được cuộc gọi thì không còn ai kiểm chéo. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-19` · Định dạng `X-Actor-Id`?**

| | |
| --- | --- |
| **IVR đề xuất** | **Id đục** (opaque), ổn định, **không** phải tên hiển thị, **không** phải email. Ví dụ: `m3-user-8f2a41`. Tối đa **128** ký tự, và `PiiGuard` từ chối chuỗi trông như số điện thoại hoặc địa chỉ. |
| **Vì sao** | Header này đi thẳng vào audit log. Tên và email là dữ liệu cá nhân; id đục cho cùng khả năng truy vết mà không mang PII vào log của module khác. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-20` · Giao diện có bắt nhập `X-Action-Reason` trước mọi thao tác `danger` không?**

| | |
| --- | --- |
| **IVR đề xuất** | Có, **bắt buộc**, tối thiểu 10 ký tự, không cho giá trị mặc định sẵn. |
| **Vì sao** | Runtime đã bắt buộc header này ở cả 8 endpoint `danger` từ khi có tầng ấy, và **từ `draft.25` nó được khai trong contract**, nên client sinh ra sẽ có sẵn tham số. Nếu UI tự điền sẵn một chuỗi mặc định, trường audit vẫn đầy nhưng **không còn nói gì** — tệ hơn là không có, vì nó trông như có. Giới hạn: `1..500` ký tự, và `PiiGuard` từ chối lý do chứa số điện thoại hoặc địa chỉ. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

### Nhóm B4 — Môi trường

---

**`M3-21` · Base URL từng môi trường, hai chiều.**

| | |
| --- | --- |
| **IVR cần từ M3** | Base URL endpoint callback cho `sandbox` và `production`. |
| **IVR sẽ cấp** | Base URL intake + issuer/JWKS + sandbox credential — xem Phần E. |
| **Chính sách version** | IVR giữ `1.0.0-draft.N` cho tới khi ký `TARGET_CONTRACT_V1`. Mỗi lần bump có changelog lưu trữ và báo trước; **breaking** thì nêu rõ ở đầu changelog như `draft.24` lần này. |
| **Trả lời** | sandbox: ______________________ · production: ______________________ |

---

## Phần C — Ba chuỗi lệch, sửa ở lớp assembler của M3

Bảng này đã đối chiếu **trực tiếp với code IVR**, không chép từ tài liệu.

| Field | IVR chờ đúng chuỗi | M3 hiện dùng | Sai thì hỏng thế nào |
| --- | --- | --- | --- |
| `program_code` | `GOLDEN_HOUR` / `TWENTY_FOUR_SEVEN` | `24_7` | Enum lỗi → **`400`**. Ồn ào, phát hiện ngay |
| `phone_validation_status` | `VALID` | `PHONE_VALID` | Enum lỗi → **`400 IVR_MALFORMED_REQUEST`** từ `draft.24`. Ồn ào, phát hiện ngay |
| `eligibility_snapshot.decision` | `ELIGIBLE` | `ELIGIBLE_FOR_IVR` | → **`200 TASK_HELD_ADMIN_REVIEW`**. **Im lặng** — mọi task dồn vào hàng đợi review mà không ai báo lỗi |

> **Dòng thứ ba là dòng nguy hiểm nhất**, vì hai dòng trên hỏng ồn ào còn nó hỏng im lặng. Trong repo
> IVR, `ELIGIBLE_FOR_IVR` **cũng** tồn tại — nhưng đó là decision **IVR tự phát ra**, không phải giá
> trị IVR chờ nhận. Chiều vào dùng `ELIGIBLE`.

### Hai điều kiện chỉ đọc được từ code, ghi ra để M3 khỏi dò

1. **`phone_masked` phải chứa ít nhất một ký tự che** (`x`, `X` hoặc `*`). Gửi số chưa che →
   `422 IVR_CONTACT_INVALID`.
2. **`dial_token_expires_at` phải bằng đúng `confirmation_window_expires_at`.** Sớm hơn →
   `422 IVR_CONTACT_INVALID`. **Muộn hơn → intake nhận, rồi hỏng ở tầng dưới** lúc quay số.

---

## Phần D — Sau khi ký, Module 3 làm những việc này

**Đây không phải câu hỏi, và không cần trả lời ở phiếu này.** Liệt kê để M3 ước lượng công sức.

| # | Việc | Xong khi nào thì tính là xong |
| ---: | --- | --- |
| D-1 | Sinh lại client từ `1.0.0-draft.24` | Client không còn 11 endpoint `auth`/`accounts` đã gỡ |
| D-2 | Sửa ba chuỗi ở Phần C tại lớp assembler | Chạy hết 35 fixture ở `seed/sales-target-v1.sample.json` đúng kết quả mong đợi |
| D-3 | Dựng endpoint callback generic + giao OpenAPI authoritative | IVR sinh được client từ nó |
| D-4 | Cài revalidate sáu điều kiện ở `M3-11` | Có test chứng minh transition bị chặn khi một điều kiện sai |
| D-5 | Chạy shared E2E cho: accepted, duplicate, conflict, stale, block, review, auth, invalid, outage | Chạy trên môi trường chung, không phải fake local |
| D-6 | Dựng giao diện quản trị nếu trả lời `ĐỒNG Ý` ở `M3-16` | Đủ ba tầng, có `X-Actor-Id` và `X-Action-Reason` |

---

## Phần E — Sau khi ký, IVR làm những việc này

| # | Việc | Người làm |
| ---: | --- | --- |
| E-1 | Dựng issuer/JWKS thật và **cấp sandbox credential** cho dev M3 | owner IVR |
| E-2 | Mở endpoint thu hồi theo shape chốt ở `M3-14`, kèm OAS và changelog | owner IVR |
| E-3 | Cấp base URL intake cho sandbox và production | owner IVR |
| E-4 | Chốt ngày tắt cơ chế `X-Internal-Token` compatibility | owner IVR |
| E-5 | Thu ngân hàng ghi âm: số 0–99, đơn vị, 34 tỉnh, tên hàng | owner IVR |

---

## Ô ký

Ký là xác nhận: **đã đọc Phần A** (và không phản đối mục nào chưa nêu ở trên), **đã trả lời Phần B**,
và **chấp nhận Phần D**.

| Vai trò | Xác nhận | Tên | Ngày |
| --- | --- | --- | --- |
| Owner / dev Module 3 | ____________ | ____________ | ______ |
| Owner Module 8 / IVR | ____________ | ____________ | ______ |

**Mục M3 muốn nêu thêm ngoài 21 câu trên** *(nếu để trống, IVR hiểu là không có)*:

______________________________________________________________________________

______________________________________________________________________________
