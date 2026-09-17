# IR-07 — Phiếu chốt một lần: Module 3 ↔ IVR (Module 8)

**Phiên bản contract:** `1.0.0-draft.30` · **Ngày phát:** 2026-09-17 · **Người phát:** owner IVR
**Trạng thái:** `READY_TO_DISPATCH / NOT_SENT`

> ## Đây là phiếu **duy nhất** IVR gửi Module 3. Không còn phiếu nào khác.
>
> **30 câu**, trả lời đúng **một lượt** rồi gửi lại. Bảng điền nhanh ở ngay dưới.
>
> ### Tin tốt trước: một việc vừa được gỡ khỏi phần của Module 3
>
> Owner chốt ngày **`17/09`**: Module 3 **gửi thẳng số điện thoại** (`phone_e164`), thay vì phải tự
> dựng bộ cấp `dial_token` để IVR giải mã. **Module 3 không phải làm token issuer, không phải làm
> kho khoá.** Chi tiết ở `A-5`; đây là thay đổi duy nhất của `draft.30`.

---

## Bảng điền nhanh — điền hết bảng này là xong phiếu

Mỗi ô: `Đ` (đồng ý với đề xuất IVR) hoặc giá trị khác. Chi tiết từng câu ở Phần B — chỉ cần mở
ra khi muốn biết **vì sao** hoặc khi định trả lời khác.

| Câu | Hỏi gì (một dòng) | Trả lời |
| --- | --- | --- |
| `M3-01` | Đặt `ivr_confirmation_required=true` lúc chuyển sang `CONFIRMING`, không gửi task cho đơn không cần gọi | |
| `M3-02` | Rẽ nhánh theo `decision` trong body, **không** theo HTTP status | |
| `M3-03` | Bump `order_version` mỗi khi đổi thứ IVR đọc để gọi | |
| `M3-04` | M3 map ba chuỗi lệch ở lớp assembler (xem Phần C) | |
| `M3-05` | `delivery_area_short` = tên tỉnh/thành, không chi tiết hơn | |
| `M3-06` | `items[]` ≤ 5 dòng thực tế | |
| `M3-07` | `golden_hour_session_id` bắt buộc với GH, ổn định qua retry | |
| `M3-08` | Một endpoint callback generic cho cả hai chương trình | |
| `M3-09` | ACK taxonomy đúng contract (`DUPLICATE_ACCEPTED` phải đi với `200`) | |
| `M3-10` | Giữ `Idempotency-Key` tối thiểu 7 ngày | |
| `M3-11` | Revalidate đủ 6 điều kiện trước khi đổi trạng thái đơn | |
| `M3-12` | Tôn trọng `is_counted_customer_attempt` | |
| `M3-13` | Sau `IVR_NO_ANSWER_FINAL` chờ 24 giờ rồi cho hết hạn | |
| `M3-14` | Gọi endpoint thu hồi khi hủy đơn / bật `sale_lock` | |
| `M3-15` | Trả `429` kèm `Retry-After` hợp lệ | |
| `M3-16` | M3 dựng giao diện quản trị IVR | |
| `M3-17` | Ba token quản trị trong secret store, xoay vòng 90 ngày | |
| `M3-18` | `danger` chỉ owner/quản lý, không trùng người duyệt lời thoại | |
| `M3-19` | `X-Actor-Id` là id đục, không tên/email | |
| `M3-20` | UI bắt nhập `X-Action-Reason` ≥ 10 ký tự, không điền sẵn | |
| `M3-21` | Base URL callback: sandbox = ______ · production = ______ | |
| `M3-22` | "10 phút" ràng buộc cái gì — chọn `A` / `B` / `C`, và mốc `0` tính từ đâu | |
| `M3-23` | Technical retry có tính vào "hai lần" không — chọn `A` / `B` | |
| `M3-24` | Giới hạn theo đơn hay theo số điện thoại — chọn `A` / `B` | |
| `M3-25` | Nếu `M3-24`=B: tham chiếu nào để khóa theo đích | |
| `M3-26` | M3 còn gửi/đọc 3 field + 1 decision đã nghỉ hưu không (4 ô) | |
| `M3-27` | M3 đã tự lọc đơn không cần gọi trước khi gửi task chưa | |
| `M3-28` | `customer_trust_status` còn cần lưu không | |
| `M3-29` | Xóa enum/field ngay, hay giữ cửa sổ tương thích bao lâu | |
| `M3-30` | `risk_flags` giữ nguyên hay thay bằng field ưu tiên riêng | |

**Ô ký ở cuối file.** Phiếu chưa ký = `PENDING_M3_SIGNOFF`.

---

## Cách dùng phiếu này — đọc trước, 60 giây

Phiếu này được viết để **chốt trong đúng một vòng**. Không có mục nào yêu cầu Module 3 tự soạn
phương án từ đầu: **mỗi mục đã có sẵn vị trí của IVR**, kèm lý do và hậu quả nếu chọn khác.

| Luật | Nội dung |
| --- | --- |
| **1** | Mỗi mục ở Phần B có ô **Trả lời**. Điền đúng một trong hai: `ĐỒNG Ý`, hoặc `KHÁC:` + giá trị của M3. |
| **2** | **Ô trống chỉ theo mặc định IVR trong phiếu đã được M3 trả lại và ký nhận toàn bộ điều khoản.** Chưa nhận phiếu hoặc thiếu chữ ký = `PENDING_M3_SIGNOFF`; không coi im lặng là phê duyệt. |
| **3** | Ngoài phiếu này, **IVR không hỏi gì thêm ở vòng này**. Phần A là thông báo, Phần D/E là việc sau khi ký. |
| **4** | Trả lại **chính file này** đã điền, một lần, kèm chữ ký ở cuối. |
| **5** | Nếu một mục nào đó M3 chưa quyết được, ghi `KHÁC: cần thêm <chính xác thứ cần>` — nêu **cái cần**, không nêu "cần bàn thêm". |
| **6** | Trả lời được nhóm nào thì đóng nhóm đó. **Không** phải chờ đủ 30 câu mới gửi lại — nhưng B5 và B6 đều có câu `P1`, xin đừng để lại vòng sau. |

> **Phạm vi luật 2.** M3 ký nhận phiếu mới xác nhận các giá trị mặc định chưa sửa.
> Không nhận phản hồi không tạo ra một chữ ký hay đóng bất kỳ cổng tích hợp nào.

---

## Phần 0 — Tài liệu cần lấy trước khi đọc tiếp

| # | Lấy gì | Đường dẫn chính xác trong repo IVR |
| ---: | --- | --- |
| 1 | Contract intake **hiện hành** | `specs/api/openapi/ivr-order-confirmation.v1.yaml` — `1.0.0-draft.30` |
| 2 | Contract callback | `specs/api/openapi/order-core-ivr-callback.target-v1.yaml` |
| 3 | **Đọc trước khi sinh client** — hai bản đều **có breaking** | `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.23-to-v1.0.0-draft.24.md` và `…draft.24-to-v1.0.0-draft.25.md` |
| 3a | **Enum response có WARN:** `MOCK/VENDOR/ASTERISK_ARI`, dashboard thêm `NONE` | [25→26](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.25-to-v1.0.0-draft.26.md), [26→27](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.26-to-v1.0.0-draft.27.md) |
| 3b | **`27` → `30`: không breaking, không cần sửa client** | [27→28](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.27-to-v1.0.0-draft.28.md) *(no changes to report, but the specs are different)*, [28→29](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.28-to-v1.0.0-draft.29.md) *(thêm `GET /audit-evidence`, `info`)*, [29→30](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.29-to-v1.0.0-draft.30.md) *(thêm field tùy chọn `phone_e164`, `info`)* |
| 4 | Fixture để tự kiểm producer trước khi gọi thật | `seed/sales-target-v1.sample.json` |
| 5 | Nhãn tiếng Việt mọi enum (nếu dựng console) | `specs/ui/enum-labels.vi.json` + đặc tả màn hình `specs/ui/` |
| 6 | Tài liệu tham chiếu đầy đủ (~1.370 dòng, **không** cần đọc hết để ký phiếu này) | `integration-requirements/06-module-3-api-handover.md` |

**Fixture ở mục 4 có sẵn 35 ca:** 9 task hợp lệ, 13 ca sai schema (đều trả `400 IVR_MALFORMED_REQUEST`),
13 ca hợp lệ schema nhưng bị domain từ chối. Chạy producer của M3 qua bộ này trước buổi lab thì
gần như mọi lỗi tích hợp thường gặp lộ ra trước, không tốn một cuộc gọi nào.

### Ba thay đổi **breaking** cần biết trước khi code

Cả ba nằm ở `draft.24` và `draft.25`. **Từ `draft.27` tới `draft.30` không có breaking nào** —
nếu client của M3 đã sinh từ `draft.27` trở lên thì không phải sinh lại vì ba bản mới.

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
   route đó, chỉ là không có cách nào biết vì sao. **Sinh lại client từ bản hiện hành `draft.30`; đọc thêm enum response ở 25→26→27.**

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
| A-5 | **Số điện thoại khách gửi thế nào** | **Module 3 gửi thẳng `phone_e164`** (`+84` + 9 chữ số). **Không** phải dựng token issuer, **không** phải dựng kho khoá. Ở `draft.30` field này **tuỳ chọn**, bản kế tiếp thành **bắt buộc**: trong giai đoạn chuyển, M3 gửi `phone_e164` **HOẶC** `dial_token` + `dial_token_expires_at`, ai sẵn trước đi trước, **không cần release đồng bộ**. Gửi cả hai thì `phone_e164` thắng, token bị bỏ qua | **owner · 2026-09-17**, thay cho `OD-V1-05`/`17`/`18` |
| A-6 | Số điện thoại sống ở đâu trong IVR | Chỉ trong **bộ nhớ tiến trình** tại biên adapter telephony, đủ lâu cho đúng một lần quay số: **không DB, không log, không evidence, không callback**. Quyết định `17/09` đổi **cách số đi vào**, **không** nới chỗ nó được phép nằm lại | `OD-V1-18` phần lưu trữ · 2026-09-05 |
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

30 mục. Mỗi mục: câu hỏi, vị trí IVR đề xuất, hậu quả nếu chọn khác, và ô trả lời.

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
| **Liên quan** | `M3-23` hỏi chiều ngược lại: technical retry **có được phép tạo ra một cuộc gọi thứ ba tới khách** hay không. Hai câu này phải trả lời cùng nhau. |
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

### Nhóm B5 — Giới hạn số cuộc gọi tới khách

> Yêu cầu *"gọi tối đa hai lần trong 10 phút"* được mô tả **bằng lời**. Trong code hiện có **hai
> chương trình với hai lịch khác nhau**, và **không cái nào** khớp trực tiếp với câu mô tả đó:
>
> | Chương trình | Số customer attempt | Offset | Cửa sổ xác nhận |
> | --- | --- | --- | --- |
> | `GOLDEN_HOUR` | 2 | `0s`, `150s` | `300s` |
> | `TWENTY_FOUR_SEVEN` | 2 | `0s`, `450s` | `900s` |
>
> Cả hai offset đều **dưới** 10 phút nên thoạt nhìn có vẻ đã thoả. Nhưng "10 phút" tính từ mốc nào
> và ràng buộc cái gì thì **ba cách đọc cho ra ba policy khác nhau**, và chúng khác nhau ở chỗ
> **khách nghe máy mấy lần**. IVR không tự chọn giúp được.
>
> **Chưa đổi gì.** Lịch hiện hành vẫn đang chạy. Nhóm này là để chốt **trước khi** sửa.

---

**`M3-22` · "10 phút" ràng buộc cái gì? (P1)**

| | |
| --- | --- |
| **Chọn một** | **A** — hai lượt gọi phải **bắt đầu** trong vòng 10 phút kể từ lượt đầu → gần với lịch hiện tại; cửa sổ xác nhận là chuyện khác.<br>**B** — **toàn bộ** việc xác nhận đơn phải xong trong 10 phút → `TWENTY_FOUR_SEVEN` đang là `900s` = 15 phút, **phải đổi**.<br>**C** — khoảng cách giữa hai lượt **không quá** 10 phút → cả hai chương trình hiện đã thoả, không phải đổi gì. |
| **Câu phụ bắt buộc** | Mốc `0` tính từ **lúc M3 giao task**, hay từ **lúc IVR quay số lần đầu**? Hai mốc này lệch nhau bằng độ trễ hàng đợi, và độ trễ đó **không cố định**. |
| **Trả lời** | ☐ A ☐ B ☐ C · mốc `0` = ☐ lúc M3 giao task ☐ lúc IVR quay số lần đầu |

---

**`M3-23` · Technical retry có tính vào "hai lần" không? (P1)**

| | |
| --- | --- |
| **Vì sao đây là câu nặng nhất nhóm** | Hiện `technical exception` **không** được tính là customer attempt (`DispositionMapper.cs`), và trần retry kỹ thuật mặc định là `1`. Budget quay số của token = `MaxAttempts + TechnicalRetryLimit`. Nghĩa là **về lý thuyết khách có thể nhận cuộc thứ ba**, nếu một lượt hỏng vì lý do kỹ thuật **sau khi đã đổ chuông**. |
| **Chọn một** | **A** — "hai lần" = hai **customer attempt**; technical retry không tính → giữ nguyên code, khách có thể nghe chuông **3 lần**.<br>**B** — "hai lần" = hai **cuộc gọi tới khách**, bất kể lý do → phải chặn technical retry tạo cuộc thứ ba; đổi budget. |
| **Nếu chọn B, trả lời thêm** | **Đổ chuông rồi mới hỏng** có tính không? Còn **gửi originate nhưng nhà mạng từ chối trước khi đổ chuông** thì sao? Hai cái này khác nhau ở chỗ khách **có bị làm phiền hay không**. |
| **Trả lời** | ☐ A ☐ B · nếu B: đổ chuông rồi hỏng ☐ tính ☐ không tính · nhà mạng từ chối trước khi đổ chuông ☐ tính ☐ không tính |

---

**`M3-24` · Giới hạn theo đơn hay theo số điện thoại? (P1)**

| | |
| --- | --- |
| **Hiện trạng** | Lịch và budget gắn theo **job/task**, tức **theo đơn**. Nếu một khách có **hai đơn** trong cùng khung giờ, họ nhận **bốn** cuộc. **Không có gì trong hệ thống hiện chặn điều đó.** |
| **Chọn một** | **A** — giới hạn theo đơn; bốn cuộc là chấp nhận được → không cần gì thêm.<br>**B** — giới hạn theo đích liên hệ, tối đa một cuộc đang hoạt động trên mỗi số → **phải trả lời `M3-25`**. |
| **Trả lời** | ☐ A ☐ B |

---

**`M3-25` · Nếu chọn `M3-24`=B: tham chiếu nào để khóa theo đích? (P2)**

| | |
| --- | --- |
| **Ràng buộc** | IVR **không lưu số điện thoại thô** — số chỉ sống trong bộ nhớ biên telephony đủ lâu để quay số (`A-6`), mọi nơi khác chỉ có tham chiếu opaque. Để khóa "một cuộc đang hoạt động trên mỗi đích", IVR cần **một tham chiếu ổn định cho cùng một số thuê bao qua nhiều đơn**. |
| **Quyết định `17/09` đổi gì ở câu này** | Từ `draft.30`, `phone_e164` đi thẳng vào intake, nên IVR **về mặt kỹ thuật** đã có đủ dữ kiện để tự dẫn xuất một khóa ổn định — trước đây thì không, vì chỉ có token mờ. Nên đây **không còn là chuyện bất khả thi, mà là một lựa chọn**: tự băm số ở phía IVR nghĩa là dựng thêm **một định danh khách hàng mới**, ở một module vốn cố tình không giữ danh tính khách. IVR đề xuất **không** làm vậy, và xin M3 cấp tham chiếu. |
| **Cần biết** | 1. `phone_ref` hiện tại có **ổn định qua các đơn khác nhau của cùng một số** không, hay mỗi task một giá trị?<br>2. Nếu không ổn định: M3 có cấp được một `contact_ref` ổn định không?<br>3. Nếu M3 không cấp và cũng không muốn IVR tự dẫn xuất → `M3-24` **phải chọn A**. |
| **Trả lời** | `phone_ref` ổn định qua nhiều đơn: ☐ Có ☐ Không · cấp được `contact_ref` ổn định: ☐ Có ☐ Không · tên field đề xuất: ______________ |

---

### Nhóm B6 — `OD-18`: Module 3 quyết định gọi, IVR chỉ thực thi

> Ưu tiên `P1`. Chặn `ACCEPTED` của `W-0123`; **không** chặn hành vi runtime hiện tại.
>
> **Điều đã đổi phía IVR.** Owner Module 8 khóa `OD-18` (`27/08`): *Module 3 quyết định nghiệp vụ,
> IVR chỉ thực thi cuộc gọi.* Đã triển khai xong ở `W-0123`. IVR **đã gỡ** khả năng tự bỏ cuộc gọi:
>
> | Trước (`OD-15`) | Nay (`OD-18`) |
> | --- | --- |
> | IVR đọc `trust.risk_evidence_available` + `risk_flags` rỗng → `TASK_SKIPPED_TRUSTED_CUSTOMER` | Không còn nhánh nào. Runtime **không bao giờ** phát sinh decision này |
> | `trusted_skip_allowed=false` là veto | Field `deprecated`, được nhận và lưu nhưng **bị bỏ qua** |
> | `customer_trust_status` dùng cho audit | `deprecated`, `LEGACY_READ` |
> | `risk_flags` tham gia quyết định gọi/bỏ | Chỉ còn audit + ưu tiên scheduler, **không** đảo quyết định |
>
> **Hệ quả quan trọng nhất, xin đọc kỹ:** nếu Module 3 từng dựa vào IVR để bỏ qua khách cũ, thì kể
> từ bản này **những đơn đó sẽ được gọi**. IVR không còn lọc hộ. Đơn nào không cần gọi thì Module 3
> phải **không gửi task**.
>
> Contract giữ tương thích để rolling deploy không vỡ; `oasdiff` xác nhận **không có breaking change**.
>
> **Điều IVR *không* yêu cầu nữa:** phiếu `OD-15` cũ yêu cầu M3 gửi
> `eligibility_snapshot.trust.risk_evidence_available` để bật trusted-skip. **Yêu cầu đó đã bị huỷ.**
> Không cần build gì cho nó. Nếu đang làm dở thì **dừng lại**.

Mỗi câu xin trả lời kèm **commit / phiên bản OpenAPI / ảnh chụp runtime** — không nhận trả lời suy đoán.

---

**`M3-26` · Hiện Module 3 có gửi hay đọc bốn thứ này không? (P1)**

| Field / giá trị | Chiều | Trả lời của M3 | Bằng chứng |
| --- | --- | --- | --- |
| `customer_trust_status` | M3 **gửi** trong `POST /tasks`? | ☐ Có ☐ Không | |
| `trusted_skip_allowed` | M3 **gửi**? | ☐ Có ☐ Không | |
| `eligibility_snapshot.trust.risk_evidence_available` | M3 **gửi**? | ☐ Có ☐ Không | |
| `TASK_SKIPPED_TRUSTED_CUSTOMER` | M3 **đọc** decision này từ response? | ☐ Có ☐ Không | |

**Vì sao cần:** đây là điều kiện để IVR biết có được **xoá hẳn** ba field khỏi contract hay phải giữ
một cửa sổ tương thích. Trả lời "Không" cho cả bốn ⇒ `M3-29` chọn remove; có bất kỳ "Có" nào ⇒ giữ deprecate.

---

**`M3-27` · Module 3 đã lọc đơn không cần gọi trước khi gửi task chưa? (P1)**

| | |
| --- | --- |
| **Vì sao cần** | Đây là câu **duy nhất** quyết định `OD-18` có an toàn trên dữ liệu thật hay không. Nếu M3 đang dựa vào IVR bỏ qua, thì **lượng cuộc gọi sẽ tăng ngay khi bản này lên**, và hai bên cần chốt kế hoạch **trước** khi deploy chứ không phải sau. |
| **Đo được, không phải tin nhau** | IVR `W-0124` thêm counter `ivr_legacy_skip_candidate_total`, đếm tại intake mỗi task mang đúng hình dạng predicate đã nghỉ hưu. Task đó **vẫn được gọi bình thường** — counter chỉ ghi lại. Sau deploy, con số này chính là **số đơn M3 vẫn đánh dấu theo cách cũ**: bằng `0` là hai bên đã khớp; khác `0` thì mỗi đơn trong đó là **một cuộc gọi thật tới một khách hàng thật**. |
| **Trả lời** | ☐ Đã lọc — tiêu chí: `_______________________`<br>☐ Chưa lọc, đang dựa vào IVR bỏ qua<br>☐ Không áp dụng — mọi đơn `CONFIRMING` đều cần gọi |

---

**`M3-28` · `customer_trust_status` có còn cần lưu không? (P2)**

| | |
| --- | --- |
| **Vì sao** | IVR đang lưu cột này nhưng **không dùng vào bất cứ quyết định nào**. Giữ một trường phân loại khách hàng mà không có mục đích là **rủi ro privacy**, không phải tài sản. |
| **Chữ ký** | Cần **M3 + M8**. Công ty không có đội Privacy riêng; theo tiền lệ `OD-V1-11` (`10/09`), owner tự nhận rủi ro và ghi vào văn bản. |
| **Trả lời** | ☐ Giữ cho audit — use case + thời hạn lưu: `_______________________`<br>☐ Bỏ khỏi payload theo nguyên tắc tối thiểu hóa dữ liệu |

---

**`M3-29` · Xoá enum/field ngay ở draft kế tiếp, hay giữ một cửa sổ tương thích? (P2)**

| | |
| --- | --- |
| **Lưu ý** | Remove là **breaking change**: cần chạy `oasdiff` và consumer contract test **hai phía** trước khi chốt. |
| **Trả lời** | ☐ Remove ở draft kế tiếp sau `1.0.0-draft.30` *(chỉ chọn được nếu `M3-26` toàn "Không")*<br>☐ Giữ `deprecated` thêm ______ tuần rồi mới remove |

---

**`M3-30` · `risk_flags` giữ nguyên hay thay bằng field ưu tiên tường minh? (P3)**

| | |
| --- | --- |
| **Hiện trạng** | IVR tính điểm ưu tiên từ `risk_flags` (`SchedulerCapacityMapper.RiskScore`). Điều này **không** ảnh hưởng quyết định gọi/bỏ, nhưng dùng một trường rủi ro **nghiệp vụ** làm tín hiệu xếp hàng **kỹ thuật** là một sự lẫn lộn đáng dọn khi có dịp. |
| **Trả lời** | ☐ Giữ `risk_flags`, IVR tiếp tục dùng cho ưu tiên scheduler<br>☐ Thay bằng một field ưu tiên riêng — đề xuất tên/kiểu: `_______________________` |

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
2. **Chỉ áp dụng nếu M3 còn gửi `dial_token`** — gửi `phone_e164` thì bỏ qua mục này.
   `dial_token_expires_at` phải bằng **đúng** `confirmation_window_expires_at`. Sớm hơn **và** muộn hơn
   đều → `422 IVR_CONTACT_INVALID`, mỗi chiều một reason code riêng. *(Vế "muộn hơn" trước đây lọt qua
   intake rồi hỏng ở tầng dưới thành `500`; `W-0302` đã sửa thành `422` ở `draft.28`.)*
   Đây là một trong những lý do đường `phone_e164` rẻ hơn cho M3: nó không có cặp mốc thời gian nào
   phải khớp nhau.

---

## Phần D — Sau khi ký, Module 3 làm những việc này

**Đây không phải câu hỏi, và không cần trả lời ở phiếu này.** Liệt kê để M3 ước lượng công sức.

| # | Việc | Xong khi nào thì tính là xong |
| ---: | --- | --- |
| D-1 | Sinh lại client từ `1.0.0-draft.30` | Client bỏ 11 endpoint đã gỡ, có header bắt buộc và enum response hiện hành |
| D-2 | Sửa ba chuỗi ở Phần C tại lớp assembler | Chạy hết 35 fixture ở `seed/sales-target-v1.sample.json` đúng kết quả mong đợi |
| D-3 | Dựng endpoint callback generic + giao OpenAPI authoritative | IVR sinh được client từ nó |
| D-4 | Cài revalidate sáu điều kiện ở `M3-11` | Có test chứng minh transition bị chặn khi một điều kiện sai |
| D-5 | Chạy shared E2E cho: accepted, duplicate, conflict, stale, block, review, auth, invalid, outage | Chạy trên môi trường chung, không phải fake local |
| D-6 | Dựng giao diện quản trị nếu trả lời `ĐỒNG Ý` ở `M3-16` | Đủ ba tầng, có `X-Actor-Id` và `X-Action-Reason` |
| D-7 | Nếu `M3-27` = "chưa lọc": chốt kế hoạch tăng tải **trước** khi deploy | Có con số dự kiến và ngày; `ivr_legacy_skip_candidate_total` về `0` |
| D-8 | Chuyển sang gửi `phone_e164` (`A-5`) | Gửi `+84` + 9 chữ số ở `POST /tasks`. **Tuỳ chọn ở `draft.30`, bắt buộc ở bản kế tiếp** — làm sớm thì bỏ được `dial_token`, `dial_token_expires_at` và toàn bộ phần token issuer chưa dựng |

> **`D-8` là việc trừ đi, không phải cộng thêm.** Nếu M3 chưa dựng bộ cấp `dial_token` thì **đừng dựng
> nữa**; nếu đã dựng dở thì dừng. Không có deadline đồng bộ: hai dạng cùng được nhận trong giai đoạn
> chuyển, chọn theo từng task lúc quay số.

---

## Phần E — Sau khi ký, IVR làm những việc này

| # | Việc | Người làm |
| ---: | --- | --- |
| E-1 | Dựng issuer/JWKS thật và **cấp sandbox credential** cho dev M3 | owner IVR |
| E-2 | Mở endpoint thu hồi theo shape chốt ở `M3-14`, kèm OAS và changelog | owner IVR |
| E-3 | Cấp base URL intake cho sandbox và production | owner IVR |
| E-4 | Chốt ngày tắt cơ chế `X-Internal-Token` compatibility | owner IVR |
| E-5 | Thu ngân hàng ghi âm: số 0–99, đơn vị, 34 tỉnh, tên hàng | owner IVR |
| E-6 | Áp policy chốt ở `M3-22`/`M3-23` vào `AttemptPolicyRegistries`, kèm version mới giữ snapshot job cũ | owner IVR |
| E-7 | Cập nhật `IR-06 §9` và chuyển `W-0123` sang đề nghị `ACCEPTED` | owner IVR |

> Nếu policy ở B5 phải đổi: phiên bản mới **giữ snapshot của job cũ**, và phải kiểm cả producer M3,
> intake, scheduler, TTL token, normalization và callback. **Không sửa hằng số riêng ở adapter.**

---

## Ràng buộc kiến trúc — không đổi, không nằm trong phạm vi thương lượng

M3 quyết đơn nào cần gọi · IVR thực thi và trả kết quả · M3 kiểm lại trạng thái/phiên bản đơn trước
khi cập nhật nghiệp vụ. Phím `0` là **yêu cầu huỷ đơn**, **không** phải yêu cầu ngừng mọi liên hệ.

---

## Ô ký

Ký là xác nhận: **đã đọc Phần A** (và không phản đối mục nào chưa nêu ở trên), **đã trả lời Phần B**
(cả sáu nhóm), và **chấp nhận Phần D**.

| Vai trò | Xác nhận | Tên | Ngày |
| --- | --- | --- | --- |
| Owner / dev Module 3 | ____________ | ____________ | ______ |
| Owner Module 8 / IVR | ✅ Đồng ý toàn bộ phía M8 | **Toàn — Module 8** | **2026-09-17** |

**Mục M3 muốn nêu thêm ngoài 30 câu trên** *(nếu để trống, IVR hiểu là không có)*:

______________________________________________________________________________

______________________________________________________________________________
