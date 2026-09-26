# IR-07 — Phiếu chốt một lần: Module 3 ↔ IVR (Module 8)

**Phiên bản contract:** `1.0.0-draft.30` lúc phát · hiện hành **`1.0.0-draft.33`** *(sửa `25/09`)* · **Ngày phát:** 2026-09-17 · **Người phát:** owner IVR
**Trạng thái:** đã ghi là gửi Module 3 ngày `17/09`, nhưng anh Mạnh báo `25/09` **chưa nhận** — sẽ gửi lại kèm đính chính `25/09` (ngày gửi lại ghi ở mục đính chính) *(sửa `25/09`; trước đó ghi `SENT / AWAITING_REPLY` — sửa `18/09`, `W-0316`; trạng thái lúc phát: `READY_TO_DISPATCH / NOT_SENT`)*

> ## Đây là phiếu **duy nhất** IVR gửi Module 3. Không còn phiếu nào khác.
>
> **32 câu**, trả lời đúng **một lượt** rồi gửi lại. Bảng điền nhanh ở ngay dưới. *(Sửa `25/09`: thêm `M3-31`,
> `M3-32`; rút `M3-13`, `M3-30`; việc "gửi thẳng số điện thoại" ngay dưới chưa chốt ở cấp công ty — xem đính
> chính `25/09` ở cuối phiếu.)*
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
| `M3-02` | Rẽ nhánh theo `decision` trong body, **không** theo HTTP status *(xem đính chính 25/09)* | |
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
| `M3-13` | **RÚT — xem đính chính 25/09.** Sau `IVR_NO_ANSWER_FINAL` chờ 24 giờ rồi cho hết hạn | |
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
| `M3-30` | **RÚT — xem đính chính 25/09.** `risk_flags` giữ nguyên hay thay bằng field ưu tiên riêng | |
| `M3-31` | `total_amount` = số đồng nguyên khách phải trả, sau lần làm tròn cuối, cùng nguồn `final_payable` *(thêm 25/09)* | |
| `M3-32` | Màn sự cố dung lượng: lọc `status`/`scope`/`program`/`opened_at`, phân trang `page`/`page_size` *(thêm 25/09)* | |

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
| **6** | Trả lời được nhóm nào thì đóng nhóm đó. **Không** phải chờ đủ 32 câu mới gửi lại — nhưng B5 và B6 đều có câu `P1`, xin đừng để lại vòng sau. |

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
| A-8 | Lời thoại | **VieNeu-TTS tự host** đọc lời thoại, chạy trên máy chủ IVR — không vendor TTS đám mây, nội dung đơn không rời hệ thống. Phần cố định render sẵn; món hàng, tổng tiền và vùng giao đọc lúc gọi. **Không đọc tên khách** | `OD-V1-19` · 2026-09-17 |
| A-9 | Ghi âm cuộc gọi | **TẮT vĩnh viễn** ở V1. Metadata cuộc gọi giữ **90 ngày** | `OD-V1-11` · 2026-09-05 |
| A-10 | IVR **không bao giờ** hủy đơn | `IVR_NO_ANSWER_FINAL` là **khuyến nghị**; Core không đổi trạng thái, đơn tự hết hạn theo timeout của M3 | `OD-V1-06` · 2026-09-05 |
| A-11 | Thu hồi đơn | IVR đã dựng **hai fence** (claim + lần đọc cuối trước khi quay số). Endpoint nhận lệnh thu hồi đi cùng lượt phát hành contract kế tiếp | `W-0248`/`W-0249` |
| A-12 | 23 field bắt buộc trên wire | Xem `IR-06 §3.4`. Có gate CI so bảng này với spec, nên nó không lệch được | `FREEZE-03` |
| A-13 | Ranh giới opt-out | **explicit-only**: chỉ coi là opt-out khi khách phát tín hiệu **tường minh**. **Không** suy ra từ số lần khách từ chối. Lưu ý thực tế: **V1 chưa có tín hiệu tường minh nào** — `DTMF-0` là phím **hủy đơn**, phím 9 ngoài scope. Nên ở V1, **không có opt-out**; M3 đừng dựng luồng trông chờ nó | `OD-V1-23` · 2026-09-10 |
| A-14 | Duyệt lời thoại cho production | Chính sách đã chốt, nhưng `PRODUCTION_REAL` đòi **ba actor id khác nhau**, nên khâu duyệt script production **hiện vẫn chặn** phía IVR. Không ảnh hưởng intake/callback | `OD-V1-11` · 2026-09-10 |

Sổ quyết định `specs/_review/open-decisions-register.md` nay còn **12 mục mở** trên tổng 29 dòng (24 `OD-V1` + 5
`OD-VOICE`) — *sửa 25/09 theo chốt chief; bản trước ghi 2/28*. Sáu dòng chờ Module 3 hoặc Tech Lead (vai M3) ký
(`OD-V1-01`, `02`, `03`, `05`, `06`, `07`); hai dòng chờ Sếp theo N16 (`OD-V1-08`, `16`); hai dòng phụ thuộc
phương án B, chờ Sếp trả lời mục B2 (`OD-V1-17`, `18`); `OD-V1-09` chờ SIM thật; `OD-V1-10` chờ số đo dung lượng.

> **Nếu M3 phản đối mục nào ở Phần A, nói ở vòng này.** Sau khi ký, mở lại một mục Phần A là mở lại
> một contract đã đóng băng — tốn hơn nhiều lần so với nói bây giờ.

---

## Phần B — Câu Module 3 phải trả lời

30 mục. Mỗi mục: câu hỏi, vị trí IVR đề xuất, hậu quả nếu chọn khác, và ô trả lời. *(`M3-31`, `M3-32` thêm ngày
`25/09` nằm ở mục đính chính cuối phiếu.)*

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
| **IVR đề xuất cách xử lý** | `*_ACCEPTED_*` → chờ callback. `*_REJECTED_*` → **không** retry mù, sửa dữ liệu rồi gửi lại với `Idempotency-Key` **mới**. `TASK_BLOCKED_OPERATIONAL` → **retry được**, đây là trạng thái tạm (kill switch/hết dung lượng) *(xem đính chính 25/09)*. `TASK_HELD_*` → **không** retry, chờ người xử lý phía IVR. |
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
| **IVR đề xuất** | Gửi **tên tỉnh/thành**, không gửi phường/xã/đường/số nhà. VieNeu đọc đúng chuỗi M3 gửi. |
| **Vì sao** | Tên tỉnh **đủ để khách nhận ra đơn** mà không đọc địa chỉ chi tiết cho người nhấc máy. Tỉnh cũng quyết định **giọng miền**: IVR tra tỉnh trong bảng `34` đơn vị để chọn giọng Bắc, Trung hay Nam. Ngoài ra `PiiGuard` chặn địa chỉ chi tiết ở tầng intake. |
| **Nếu M3 chọn khác** | Gửi địa chỉ chi tiết → task bị từ chối `422 IVR_PII_POLICY_VIOLATION`. Gửi cấp quận/huyện mà không có tên tỉnh → vẫn đọc được, nhưng không xác định được miền nên giọng rơi về miền mặc định. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-06` · Giới hạn `items[]` trong `privacy_safe_order_summary`?**

| | |
| --- | --- |
| **Giới hạn đang thực thi** | `items[]` tối đa **100** dòng (`SpeechSummaryLimits`, `ServiceCollectionExtensions.cs:85`); `public_name` ≤ **160** ký tự, `unit_label` ≤ **40** (OpenAPI). Đây là con số **kỹ thuật**, đã kiểm trong code. |
| **IVR đề xuất** | Giữ **≤ 5 dòng** trong thực tế, và `public_name` là **tên bán hàng công khai** — không phải mã nội bộ, không kèm ghi chú. |
| **Vì sao đề xuất thấp hơn giới hạn** | Lời thoại đọc **từng dòng** hàng thành tiếng. 100 dòng lọt qua validation nhưng cuộc gọi sẽ dài hơn cửa sổ xác nhận (5 phút Golden Hour), và khách cúp máy trước khi nghe hết. Giới hạn kỹ thuật không bảo vệ được điều đó. |
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

**`M3-13` · Sau `IVR_NO_ANSWER_FINAL`, M3 chờ bao lâu rồi cho đơn hết hạn?** — **RÚT — xem đính chính 25/09.**

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
| **Trạng thái** | IVR mở endpoint sau khi Module 3 trả lời `M3-14` *(sửa `25/09`; bản gửi `17/09` ghi "đi cùng lượt phát hành contract kế tiếp")*. IVR cần **câu trả lời của M3 ở phiếu này** để chốt shape trước khi mở OAS. |
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

**`M3-30` · `risk_flags` giữ nguyên hay thay bằng field ưu tiên tường minh? (P3)** — **RÚT — xem đính chính 25/09.**

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
| `eligibility_snapshot.decision` | `ELIGIBLE` | `ELIGIBLE_FOR_IVR` | → **`200 TASK_HELD_ADMIN_REVIEW`**. **Im lặng** — mọi task dồn vào hàng đợi review mà không ai báo lỗi *(xem đính chính 26/09)* |

> **Dòng thứ ba là dòng nguy hiểm nhất**, vì hai dòng trên hỏng ồn ào còn nó hỏng im lặng *(xem đính chính 26/09)*. Trong repo
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
| E-5 | Nghe thử VieNeu đọc tên hàng và vùng giao thật của M3 trên sandbox; tên nào đọc sai thì bổ sung `pronunciation_hints` | owner IVR |
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

**Mục M3 muốn nêu thêm ngoài 32 câu (`M3-01`…`M3-32`)** *(nếu để trống, IVR hiểu là không có)*:

______________________________________________________________________________

______________________________________________________________________________

---

## Đính chính `2026-09-17` — contract `1.0.0-draft.31` (`W-0312`)

> Phần trên là **bản đã gửi Module 3**, giữ nguyên từng chữ — trừ bốn dòng sửa tại chỗ ngày `18/09`, liệt
> kê ở mục cuối. Mục này sửa những câu trong đó **không còn đúng**, hoặc **chưa từng đúng**, tính tới
> `1.0.0-draft.31`. Chỗ nào mục này nói khác phần trên thì **mục này thắng**.

**Nếu chỉ đọc một câu:** *`draft.30` vẫn bắt buộc `dial_token`; từ `draft.31`, gửi riêng `phone_e164` là đủ.*

| Chỗ trong phiếu | Phiếu ghi | Đúng là |
| --- | --- | --- |
| Đầu phiếu · Phần 0 dòng `1` · `D-1` | Contract hiện hành `1.0.0-draft.30` | **`1.0.0-draft.31`** — [changelog `30→31`](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.30-to-v1.0.0-draft.31.md). **Không breaking**: body nào hợp lệ theo contract `draft.30` vẫn hợp lệ ở `draft.31`, và từ `draft.27` tới `draft.31` vẫn không có breaking nào (Phần 0 dòng `3b`) |
| `A-5` | Ở `draft.30`, M3 gửi `phone_e164` **hoặc** cặp token | **Ở `draft.30` câu này sai.** Contract vẫn bắt buộc `dial_token` + `dial_token_expires_at`, nên body chỉ có số bị `400`. Ở production còn nặng hơn: task nào cũng bị từ chối, kể cả task gửi cả số lẫn token. **Từ `draft.31` câu này đúng** |
| `A-5` | *"Gửi cả hai thì `phone_e164` thắng, token bị bỏ qua"* | Số thắng **lúc quay**. Nhưng token **không** bị bỏ qua **lúc nhận**: đã gửi token thì phải gửi đủ cặp, và `dial_token_expires_at` vẫn phải bằng đúng window end (Phần C mục `2`). Token sai ⇒ task bị từ chối **dù số đúng**. Cách đơn giản nhất: **gửi số thì đừng gửi field token nào** |
| `A-6` | Số chỉ nằm trong bộ nhớ tiến trình, *"không DB"* | **Không còn đúng từ `17/09` (`W-0311`).** IVR **lưu** `phone_e164` trong bảng task, vì cửa sổ xác nhận dài `5–15` phút và lần gọi thứ hai phải sống qua một lần khởi động lại worker. Vẫn đúng: log, callback và API quản trị chỉ mang `phone_masked`; số chỉ được **dùng** ở đúng một chỗ — lúc quay |
| `A-12` | `23` field bắt buộc | **`21`.** `dial_token` và `dial_token_expires_at` không còn bắt buộc riêng lẻ; luật *"số **hoặc** cặp token"* nằm ở [`IR-06`](06-module-3-api-handover.md) §3.4.0 |
| Phần 0 dưới bảng · `D-2` | Fixture có `35` ca: `9` task hợp lệ, `13` sai schema, `13` bị domain từ chối | **`39` ca**: `10` task hợp lệ — thêm `golden-hour-online-number-only`, body **chỉ có số**, đúng dạng M3 sẽ gửi — `16` sai schema (thêm `3` ca: thiếu cả số lẫn token · số bên cạnh nửa cặp token · số có `0` đứng đầu), `13` bị domain từ chối |
| Phần C mục `2` | *"gửi `phone_e164` thì bỏ qua mục này"* | Bỏ qua được khi gửi số **không kèm** field token nào — xem dòng `A-5` thứ hai ở trên |
| `D-8` | `phone_e164` *"tuỳ chọn ở `draft.30`, bắt buộc ở bản kế tiếp"* | `draft.31` **chưa** bắt buộc số — nó **bỏ bắt buộc token**. Bản bắt buộc `phone_e164` là bản **breaking**; IVR chỉ phát bản đó **sau khi Module 3 xác nhận đã gửi số**, kèm changelog báo trước |

### `draft.31` trả `400 IVR_MALFORMED_REQUEST` khi

- Không có số, cũng không có token.
- Có **nửa** cặp token — `dial_token` mà thiếu `dial_token_expires_at`, hoặc ngược lại — **kể cả khi có số**.
- `phone_e164` sai mẫu `^\+84[0-9]{9}$`: `0` đứng đầu, thiếu `+`, thừa hoặc thiếu chữ số, có khoảng trắng,
  xuống dòng ở cuối, chuỗi rỗng. *(Mẫu này đã có trong contract từ `draft.30` nhưng runtime chưa kiểm;
  `draft.31` kiểm.)*

Mã `400` **không nêu tên field**. Mô tả cũ trong contract hứa điều đó; `draft.31` bỏ lời hứa thay vì để
M3 dựng xử lý lỗi dựa vào nó.

### Thử ngay trên sandbox

`task_id` = `TASK-M3-NUMBER`: body chỉ có `phone_e164`, ra `IVR_CONFIRMED`, Module 3 nhận callback —
[`IR-08`](08-module-3-sandbox-guide.md) mục `6`. `pnpm sandbox:examples` chạy **28** ví dụ, gồm cả lượt này
và ca *"nửa cặp token"* bị `400`.

---

## Đính chính bổ sung `2026-09-17` — thời hạn lưu dữ liệu (`W-0313`)

> Viết **sau** mục đính chính `draft.31` ở trên, nên gửi riêng được. Không đổi contract, M3 không phải
> sửa gì trong code.

| Chỗ trong phiếu | Phiếu ghi | Đúng là |
| --- | --- | --- |
| `A-9` | Metadata cuộc gọi giữ **90 ngày** | **Không đặt kỳ hạn xoá.** Owner quyết định ngày `17/09`: IVR giữ **toàn bộ** dữ liệu, không tự xoá theo thời gian. Dữ liệu cá nhân của một khách chỉ bị xoá khi **khách yêu cầu**. Ghi âm vẫn **TẮT** như cũ |

---

## Đính chính bổ sung `2026-09-18` — giọng đọc (`W-0315`)

> Owner quyết định ngày `17/09` (`S4`): **VieNeu-TTS tự host là bộ đọc duy nhất.** Bốn dòng dưới đây đã
> sửa **tại chỗ** ở phần trên; bản gửi `17/09` còn nguyên trong lịch sử git. Không đổi contract, M3
> không phải sửa gì trong code, và đề xuất ở `M3-05`, `M3-06` giữ nguyên.

| Chỗ trong phiếu | Nay là |
| --- | --- |
| `A-8` | VieNeu-TTS tự host đọc lời thoại trên máy chủ IVR; phần cố định render sẵn, món hàng, tổng tiền và vùng giao đọc lúc gọi. Không vendor TTS đám mây, không đọc tên khách |
| `M3-05` | Vẫn đề xuất gửi **tên tỉnh/thành**. Lý do: đủ để khách nhận ra đơn, không lộ địa chỉ chi tiết, và tỉnh quyết định giọng miền |
| `M3-06` | Vẫn đề xuất **≤ 5 dòng**. Lý do: mỗi dòng được đọc thành tiếng, nhiều dòng thì cuộc gọi dài quá cửa sổ xác nhận |
| `E-5` | Nghe thử VieNeu đọc tên hàng và vùng giao thật của M3 trên sandbox; tên nào đọc sai thì bổ sung `pronunciation_hints` |

---

## Đính chính bổ sung `2026-09-25` — theo danh sách của chief (`W-0354`)

> Module 8 viết mục này theo [danh sách việc chief lập ngày `25/09`](../plan/toan-viec-can-lam-m8-2026-09-25.md);
> chief lập danh sách đó thay mặt Tech Lead. Chỗ nào mục này nói khác phần trên hoặc các đính chính trước thì
> **mục này thắng**. Chỉ **một** dòng đổi contract: `total_amount` phải là số đồng nguyên từ `draft.33`. Một dòng
> đổi hành vi thấy được trên dây nhưng không đổi schema: biên buổi sáng (`B17`). Hai câu hỏi mới, `M3-31` và
> `M3-32`, nằm ở cuối mục. Các dòng còn lại nói về cách Module 3 xử lý những giá trị IVR **đã phát từ trước**,
> hoặc sửa những câu trong phiếu không còn đúng.
>
> **Trạng thái gửi:** phiếu ghi đã gửi Module 3 ngày `17/09`. Ngày `25/09` anh Mạnh báo **chưa nhận** bản
> `draft.30` lẫn các đính chính. Phiếu được gửi lại kèm mục này; ngày gửi lại ghi ở đây khi gửi. Tới khi
> Module 3 xác nhận đã nhận, các dòng chờ Module 3 trong sổ quyết định giữ hậu tố `M3_NOT_RECEIVED`.

**Nếu chỉ đọc một câu:** *đơn 24/7 phát sinh ngoài `08:00–21:00:30` thì Module 3 giữ lại tới `08:00` rồi mới gửi;
và sau `IVR_NO_ANSWER_FINAL`, Module 3 xử lý đơn ngay theo chương trình, không chờ `24` giờ.*

| Chỗ trong phiếu | Phiếu ghi | Đúng là |
| --- | --- | --- |
| Đầu phiếu · Phần 0 dòng `1` · `D-1` | Contract hiện hành `1.0.0-draft.30`, đính chính `17/09` nâng lên `draft.31` | **`1.0.0-draft.33`**. [`31→32`](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.31-to-v1.0.0-draft.32.md) thêm một endpoint quản trị phát lại callback đã chết. `draft.33` có hai việc, ghi ở [`docs/api-changelog.md`](../docs/api-changelog.md) (mục `draft.33`, đầu phần *Current comparisons*): siết `total_amount` (dòng ngay dưới), và ghi đúng kiểu OpenAPI 3.1 cho ba field có thể null trong phản hồi `audit-evidence` (`reason`, `before_state_json`, `after_state_json`). Báo cáo oasdiff `32→33` không có dòng nào cho `total_amount`, vì oasdiff không xét `multipleOf`, nên không dẫn nó ở đây. oasdiff không báo breaking ở bước nào |
| `privacy_safe_order_summary.total_amount` | `number`, `minimum: 0` | **Số đồng nguyên**: số khách phải trả sau lần làm tròn cuối, cùng nguồn với số phải thu phía Module 3. Từ `draft.33` (`multipleOf: 1`), số lẻ như `210636.8` bị `400 IVR_MALFORMED_REQUEST`. Trước đó IVR vẫn nhận số lẻ, rồi hỏng lúc quay số vì tiền đọc cho khách nghe không có phần lẻ; lỗi đó còn cách ly nhầm một kênh SIM. Module 3 làm tròn trước khi gửi. Đây là một phép **siết** field dùng chung, dù oasdiff không tính là breaking. Câu xác nhận nghĩa của field này là `M3-31`, cuối mục |
| `M3-02` | `TASK_BLOCKED_OPERATIONAL` → **retry được**, trạng thái tạm (kill switch/hết dung lượng) | Sai với **cả hai** reason mà intake trả kèm `200 TASK_BLOCKED_OPERATIONAL` — không reason nào retry được với cùng payload. **(1) `CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW`** (`W-0298`). Từ `26/09` (`Q-22`) điều kiện là **có attempt (`T0 + offset`, `T0` là `confirmation_window_started_at`) rơi ngoài giờ gọi `08:00–21:08`** (giờ VN): `T0` trước `08:00`, hoặc `T0` từ `21:00:30` (24/7) / `21:05:30` (Giờ Vàng). Từ `25/09` (`B17`) tới đó chỉ xét `T0`; trước nữa là "không attempt nào rơi vào giờ gọi" — xem dòng *Biên buổi sáng* ngay dưới. Gửi lại trong cùng cửa sổ thì bị từ chối lại. Hướng chief chốt `25/09`, **không phải câu hỏi lựa chọn**: Module 3 giữ đơn `TWENTY_FOUR_SEVEN` (COD) phát sinh ngoài `08:00–21:00:30` (trước `26/09`: `08:00–21:08`), rồi gửi task **từ `08:00` trở đi** với cửa sổ mới và `Idempotency-Key` **mới**; được dùng lại `task_id`, vì task bị chặn không được lưu. Đơn `GOLDEN_HOUR` phát sinh trong phiên (`12:15–13:00`, `20:15–21:00`) có `T0` trong giờ gọi nên không gặp lý do này; biên giờ vẫn áp dụng như nhau cho cả hai chương trình. Gửi dồn lúc `08:00` vượt dung lượng thì đơn có thể không được gọi: task đã nhận mà hết cửa sổ vẫn chưa quay được thì Module 3 nhận kết quả `IVR_CAPACITY_EXCEPTION`, kể cả task bị giữ vì hết dung lượng ngay ở bước kiểm eligibility *(sửa `26/09`, xem đính chính `26/09`)* — nên rải việc gửi lại. **(2) `DIAL_TOKEN_PROTECTION_UNAVAILABLE`**: task gửi `dial_token` **không kèm** `phone_e164`, trên triển khai chưa có bộ mã hoá dial token — tức production (sandbox `MOCK` có bộ mã hoá giả nên không gặp). Hôm nay production còn chưa tới bước này: khi `REAL_CUSTOMER_CALL_ALLOWED=NO`, intake giữ mọi task ở `TASK_HELD_ADMIN_REVIEW` (reason `REAL_CUSTOMER_CALL_ALLOWED_NO`) trước đó. Gửi lại cùng payload thì bị từ chối lại; muốn qua phải gửi kèm `phone_e164` (phương án B — đang chờ Sếp, xem dòng phương án B bên dưới) hoặc chờ có bộ mã hoá. Hạn chế gọi (`call_restriction`) **không** thuộc nhóm này: nó trả `409 IVR_OPERATIONAL_BLOCKED` |
| Biên buổi sáng (`B17`) | *(phiếu và `IR-06` cũ)* Task có attempt sau rơi vào giờ gọi vẫn được nhận | **Đổi hành vi từ `25/09`.** Intake xét `T0`: `T0` trước `08:00:00` hoặc từ `21:08:00` là từ chối (`200 TASK_BLOCKED_OPERATIONAL`, reason như dòng trên), cho cả hai chương trình. Trước đây đơn 24/7 có `T0` `07:52:30–07:59:59` (Giờ Vàng `07:57:30–07:59:59`) được nhận vì attempt 2 rơi vào giờ gọi, và scheduler **gọi bù** attempt 1 ngay lúc `08:00` — hai cuộc có thể gần như liền nhau; còn đơn `T0` `07:45:01–07:52:29` (Giờ Vàng `07:55:01–07:57:29`) bị từ chối dù scheduler gọi được. Nay mọi `T0` trước `08:00` bị từ chối — khớp đúng luật Module 3 giữ đơn ngoài `08:00–21:08`. **Biên tối đổi từ `26/09` (`Q-22`).** Đơn 24/7 có `T0` `21:00:30–21:07:59` (Giờ Vàng `21:05:30–21:07:59`) trước đây được nhận nhưng chỉ kịp gọi `1` cuộc, vì attempt 2 rơi từ `21:08` trở đi và không bao giờ được quay; khách không nghe cuộc 1 thì kết quả là `IVR_CONFIRMATION_WINDOW_EXPIRED`, không phải `IVR_NO_ANSWER_FINAL`. Nay intake từ chối đơn đó như đơn đêm (cùng quyết định, cùng reason), nên mọi đơn được nhận đều đủ hai cuộc trong giờ gọi. Đây là thay đổi thấy được trên dây (cùng payload, quyết định đổi), **không** đổi schema OpenAPI |
| `A-10` | Core không đổi trạng thái, đơn tự hết hạn theo timeout của M3 | `A-10` chỉ còn một nghĩa: **IVR không tự hủy đơn**. Sau `IVR_NO_ANSWER_FINAL`, Module 3 xử lý theo flow 04 và chốt `25/09`: đơn `TWENTY_FOUR_SEVEN` (COD) **hủy** với lý do `IVR_NO_ANSWER_MAX`; đơn `GOLDEN_HOUR` cho **xác nhận hết hiệu lực** và nhả suất theo flow 05 (lý do ghi `IVR_NO_ANSWER_MAX`), **không** hủy như đơn COD |
| `A-10` · callback | *(không nói)* | Trên dây, `IVR_NO_ANSWER_FINAL` vẫn đi kèm `recommended_core_action = CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`. Giá trị này bị một ràng buộc trong DB của IVR khoá và **không đổi** ở lượt này. **Module 3 không làm theo nó**; xử lý theo dòng ngay trên |
| `M3-13` | Đề xuất chờ **24 giờ** rồi cho đơn hết hạn | **Rút đề xuất; câu hỏi không còn.** Module 3 xử lý đơn ngay khi nhận callback, theo dòng `A-10`. Chờ `24` giờ thì hàng của phiên Giờ Vàng bị giữ suốt thời gian đó |
| `M3-13` dòng *Vì sao* · `A-10` cột *Nguồn ký* | `OD-V1-06` đã chốt: IVR không bao giờ hủy đơn, đơn hết hạn là hành vi của M3 | Sổ quyết định sửa theo chốt chief `25/09`: `OD-V1-06` **không còn** `CLOSED` với nghĩa "đơn tự hết hạn". Dòng này được viết lại theo flow 04 — đơn 24/7 COD hủy với `IVR_NO_ANSWER_MAX`, đơn Giờ Vàng hết hiệu lực xác nhận — owner Tech Lead (vai M3), chờ bản ký. Vế "IVR không tự hủy đơn" giữ nguyên (dòng `A-10` ở trên) |
| `A-11` | Endpoint thu hồi *"đi cùng lượt phát hành contract kế tiếp"* | `draft.30`, `draft.31`, `draft.32`, `draft.33` đều **không** có endpoint này, và đúng là phải vậy: IVR chỉ mở nó **sau khi Module 3 trả lời `M3-14`** (shape `task_id` + `order_version` + `reason`). Tới lúc đó, hai fence thu hồi phía IVR không kích hoạt được |
| `M3-30` | Hỏi Module 3 có thay `risk_flags` bằng một field ưu tiên riêng không | **Rút câu hỏi.** Trên dây **không có** field ưu tiên và sẽ không có (`IR-06` §3.5, bổ sung `W-0304`). Scheduler tự xếp thứ tự; đòn bẩy duy nhất của Module 3 là `expires_at`. Muốn mở một field ưu tiên thì phải trình chief, vì đó là field dùng chung |
| `M3-16` · `M3-20` · Phần 0 mục `3` | IVR giao **31 endpoint**: `15` read, `5` write + `3` dev, `8` danger; `X-Action-Reason` bắt buộc ở *"cả 8 endpoint `danger`"* | **33 endpoint**: `16` read (thêm `GET /audit-evidence`, `draft.29`), `5` write + `3` dev, `9` danger (thêm `POST /result-callbacks/{callbackId}:replay`, `draft.32`). `X-Action-Reason` bắt buộc ở cả `9` endpoint `danger`; con số `8` ở Phần 0 mục `3` đúng tại `draft.25`, trước khi có endpoint phát lại. `X-Actor-Id` bắt buộc trên `31/33`, thiếu là `403` (`IR-06` §4A.2, Bẫy 2) |
| `M3-10` · `D-5` | *(không nói về phát lại)* | Endpoint phát lại callback đã chết (`draft.32`) từ `draft.34` chỉ nhận callback chưa quá `7` ngày (đề xuất `M3-10`; cấu hình `1–30`), quá thì `409 IVR_VERSION_CONFLICT` và không ghi gì — *sửa tại chỗ `26/09` (`Q-19`); bản trước ghi "không giới hạn tuổi"*. Callback trong `7` ngày vẫn có thể tới sau khi đơn đã đổi, và phát lại sau thời hạn giữ key thì M3 chỉ còn revalidate (`M3-11`) để chặn, nên M3 phải xét `order_version` và trạng thái đơn trên **mọi** callback. Thêm vào `D-5` ca: phát lại `IVR_CONFIRMED` sau khi đơn đã hết hạn ⇒ `REJECTED_STALE`. Chi tiết ở `IR-06` §4A.3 |
| Phần E | *(không có việc nào về phát lại callback)* | **`E-8`**: IVR giữ giới hạn tuổi của endpoint phát lại khớp với thời hạn giữ key ở `M3-10`. *Sửa tại chỗ `26/09` (`Q-19`):* giới hạn đã đặt ở `draft.34`, `7` ngày theo đề xuất; nếu `M3-10` chốt số khác thì IVR đổi cấu hình, không đổi contract (`IR-06` §4A.3). Người làm: owner IVR |
| Đầu phiếu (*"Tin tốt trước"*) · `A-5` · `A-6` · `A-9` · `D-8` · các dòng `A-5`, `A-6`, `D-8` của đính chính `17/09` · đính chính `A-9` (`W-0313`) | Owner chốt `17/09`: M3 gửi thẳng `phone_e164`, không phải dựng token issuer; IVR lưu số trong bảng task; IVR giữ toàn bộ dữ liệu, không đặt kỳ hạn xoá | **Chưa chốt ở cấp công ty.** Phương án B (M3 gửi số `phone_e164`, IVR lưu số) đang chờ Sếp trả lời mục B2 phiếu `25/09` (giữ số dạng đọc được và thời hạn giữ); vế "giữ toàn bộ dữ liệu" ở đính chính `A-9` chờ cùng câu trả lời đó. Tới khi Sếp trả lời, Module 3 **chưa nối** producer gửi số thật sang IVR (chief ghi vào FIX_M3 ngày `25/09`). Contract `draft.33` vẫn nhận cả hai dạng (số, hoặc cặp token) — không đổi. Ghi âm vẫn **TẮT** |
| `A-8` · đính chính `18/09` dòng `A-8` | VieNeu-TTS tự host đọc lời thoại; phần cố định render sẵn, **món hàng, tổng tiền và vùng giao đọc lúc gọi** | Luật quá độ Tech Lead `24/09`: chưa có Model Gateway thì production **không sinh giọng lúc gọi**. Lời thoại production là audio dựng sẵn từ template đã duyệt; phần động (nếu có) ghép từ clip do VieNeu render trước (offline). Phạm vi phần động — đọc tên hàng/vùng giao, hay chỉ mã đơn + tổng tiền theo PACK-09 §17.2 — Tech Lead đang chốt. Tới lúc đó Module 3 **không** dựng dữ liệu riêng cho việc đọc tên hàng lúc gọi; `pronunciation_hints` không được dùng lúc gọi. Vẫn đúng: không vendor TTS đám mây, không đọc tên khách |
| `E-5` · đính chính `18/09` dòng `E-5` | Nghe thử VieNeu đọc tên hàng và vùng giao thật của M3 trên sandbox; tên nào đọc sai thì bổ sung `pronunciation_hints` | **Tạm rút**, theo dòng `A-8` ngay trên: production không sinh giọng lúc gọi, và phạm vi phần động chưa chốt |
| `M3-05` · `M3-06` · đính chính `18/09` hai dòng tương ứng | *"VieNeu đọc đúng chuỗi M3 gửi"*, tên tỉnh *"đủ để khách nhận ra đơn"* (`M3-05`); *"Lời thoại đọc từng dòng hàng thành tiếng"* (`M3-06`) | Giữ cả hai đề xuất: gửi **tên tỉnh/thành**, không gửi địa chỉ chi tiết (hiện tỉnh chọn giọng miền, `OD-VOICE-02`), và ≤ `5` dòng hàng. Nhưng tỉnh và tên hàng có được **đọc** cho khách nghe hay không phụ thuộc phạm vi phần động Tech Lead đang chốt (dòng `A-8` ở trên); hai lý do dựa trên việc đọc chỉ còn đúng nếu phạm vi đó có đọc chúng |
| `M3-16` · `M3-18` | *(phiếu không nói về chuỗi `(perm IVR_…)` và header `X-Script-Permissions`)* | Chuỗi `(perm IVR_…)` trong `summary` của OpenAPI và ở `IR-06` §4A phần lớn là **nhãn**: IVR đóng lên dòng admin action để audit, hoặc chỉ còn là tên cũ; chúng không cấp quyền gì. **Trừ** `IVR_SCRIPT_*`: đó là giá trị header `X-Script-Permissions` mà IVR **kiểm thật**. Module 3 phải ánh xạ vai trò của mình sang các giá trị này cho `4` endpoint kịch bản — `POST /scripts` (`IVR_SCRIPT_EDIT`), `:submit` (`IVR_SCRIPT_REVIEW`), `:approve` (`IVR_SCRIPT_APPROVE_*` theo `approval_type`), `:retire` (`IVR_SCRIPT_RETIRE`). Thiếu giá trị cần thiết thì `403 IVR_FORBIDDEN_CALLER`, dù token đúng tầng (`IR-06` §4A.5) |
| Phần A · đoạn dưới bảng | Sổ quyết định còn **2 mục mở** trên tổng `28` (`OD-V1-09`, `OD-V1-10`); không mục nào chặn tích hợp M3 | Sau khi sửa sổ theo chốt chief `25/09`: **`12` mục mở** trên tổng `29` dòng (`24` dòng `OD-V1-*`, `5` dòng `OD-VOICE-*`). Chờ Module 3 đối ký qua phiếu này: `OD-V1-01`, `05`. Chờ Tech Lead ký: `OD-V1-02`, `03`, `06` (vai M3) và `07`. Chờ Sếp: `OD-V1-08`, `16` (phiếu `25/09` mục N16), `OD-V1-17`, `18` (mục B2). Chờ số đo: `OD-V1-09`, `10`. Câu *"không mục nào chặn tích hợp M3"* vì vậy không còn đúng. Phần A dẫn nhiều mục trong số này làm nguồn ký (`A-1`, `A-2`, `A-4` tới `A-7`, `A-10`); với các dòng đó, *"đã có chữ ký"* ở đầu Phần A chỉ đúng với chữ ký phía Module 8 |

### Câu hỏi bổ sung `25/09`

Hai câu mới, trả lời cùng lượt với Phần B; cả hai đã có dòng trong bảng điền nhanh ở đầu phiếu. Ô trống theo luật
`2`: chỉ lấy mặc định IVR khi phiếu đã được Module 3 ký nhận.

---

**`M3-31` · `total_amount` là số nào?**

| | |
| --- | --- |
| **IVR đề xuất** | `privacy_safe_order_summary.total_amount` là **số đồng nguyên khách phải trả**, sau lần làm tròn cuối, cùng nguồn với `final_payable` phía Module 3. Từ `draft.33` số lẻ bị `400 IVR_MALFORMED_REQUEST`. |
| **Vì sao** | Trong kịch bản hiện hành, đây là số tiền khách nghe rồi bấm phím xác nhận. Nó khác số Module 3 thu thì khách xác nhận một số tiền khác số sẽ trả. IVR dùng nguyên số nhận được: không làm tròn, không tính lại. |
| **Nếu M3 chọn khác** | Ghi rõ `total_amount` lấy từ trường nào và làm tròn ở bước nào. Số lẻ vẫn bị từ chối ở schema. |
| **Mặc định** | Phiếu đã ký mà ô này trống = đồng ý đề xuất trên. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

**`M3-32` · Màn sự cố dung lượng (capacity incidents) trên console của Module 3 cần lọc và phân trang gì?**

| | |
| --- | --- |
| **Hiện trạng** | IVR ghi mỗi sự cố dung lượng vào một bảng riêng, nhưng **chưa có endpoint đọc lịch sử**: `GET /dashboard` chỉ trả tóm tắt tối đa `20` sự cố **đang mở**. |
| **IVR đề xuất** | `GET /capacity-incidents`, tầng `read`, chỉ đọc (không xoá). Lọc theo `status`, `scope`, `program`, khoảng `opened_at`; phân trang `page`/`page_size` (mặc định `25`, tối đa `100`) như `GET /call-jobs` và `GET /review-items`. **Không** trả `session_id` — đó là mã phạm vi dung lượng nội bộ, không phải phiên Giờ Vàng (`IR-06` §3.5A) — và **không** trả `reason`, như bản tóm tắt trên `GET /dashboard` hiện nay. |
| **Vì sao** | Theo phán quyết `16/09` của chief, endpoint này làm khi Module 3 nêu cần lọc và phân trang gì. Mỗi endpoint mới là một lần đổi contract, nên IVR chỉ code **sau khi Module 3 trả lời câu này**, đúng một lần. |
| **Nếu M3 chọn khác** | Nêu bộ lọc, thứ tự sắp xếp và các cột màn hình cần. |
| **Mặc định** | Phiếu đã ký mà ô này trống = đồng ý đề xuất trên. |
| **Trả lời** | ☐ ĐỒNG Ý ☐ KHÁC: ____________________________________________ |

---

## Đính chính bổ sung `2026-09-26` — task bị giữ sau intake nay có callback khi hết cửa sổ (`K-59`)

> Viết sau đính chính `25/09`. Chỗ nào mục này nói khác phần trên hoặc các đính chính trước thì **mục này thắng**.
> Không đổi contract, không đổi schema. Đổi một hành vi thấy được trên dây: có callback ở chỗ trước đây không có gì.
> Module 3 không phải sửa code vì mục này; chỉ cần biết callback nào sẽ tới.

**Nếu chỉ đọc một câu:** *task IVR đã nhận mà bước kiểm eligibility giữ lại, hoặc chưa kịp xét, nay có callback khi
hết cửa sổ: `IVR_CONFIRMATION_WINDOW_EXPIRED` + `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW`, hoặc `IVR_CAPACITY_EXCEPTION`
nếu bị giữ vì hết dung lượng.*

| Chỗ trong phiếu | Phiếu ghi | Đúng là |
| --- | --- | --- |
| Phần C · dòng `eligibility_snapshot.decision` · câu *"Dòng thứ ba là dòng nguy hiểm nhất"* | `ELIGIBLE_FOR_IVR` → `200 TASK_HELD_ADMIN_REVIEW`; hỏng im lặng, mọi task dồn vào hàng đợi review mà không ai báo lỗi | Intake **không** kiểm giá trị này: task vẫn được nhận (`200 TASK_ACCEPTED_CALL_JOB_CREATED`). Bước kiểm eligibility sau intake giữ job chờ người duyệt (`TASK_HELD_ADMIN_REVIEW`, reason `ELIGIBILITY_SNAPSHOT_UNKNOWN`), và khách không được gọi. Từ `26/09` (`K-54`), hết cửa sổ thì Module 3 nhận callback `IVR_CONFIRMATION_WINDOW_EXPIRED` + `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW`; trước đó job bị giữ không bao giờ đóng và Module 3 không nhận gì. Dòng này **vẫn** nguy hiểm nhất: lúc gửi không có tín hiệu nào, và callback tới khi hết cửa sổ không phân biệt được với đơn không gọi được vì lỗi phía IVR. Việc của Module 3 không đổi: map `ELIGIBLE_FOR_IVR` → `ELIGIBLE` ở lớp assembler (`M3-04`) |
| Phần C · các lý do giữ khác | *(không nói)* | Cùng callback đó khi hết cửa sổ, không tính lượt khách. Lý do giữ phần lớn nằm ở `eligibility_snapshot` Module 3 gửi: thiếu `source_version`, `captured_at` trước lúc mở cửa sổ, `source_available=false`… Task mà eligibility chưa kịp xét trước khi hết cửa sổ cũng vậy: lượt xét tới sau lúc đó bị từ chối, và task đóng với đúng cặp này (`K-60`). Module 3 xử lý theo bảng map ở [`IR-06`](06-module-3-api-handover.md) §4.3, khối đính chính `C22`: đơn `TWENTY_FOUR_SEVEN` (COD) **không** tự hủy, chuyển người trực; đơn `GOLDEN_HOUR` cho xác nhận hết hiệu lực theo flow 05 |
| Đính chính `25/09` · dòng `M3-02` | Task bị giữ vì hết dung lượng ngay ở bước kiểm eligibility chưa phát kết quả nào về Module 3; IVR đang sửa | **Đã sửa** (`K-52`, `26/09`): task đó nhận `IVR_CAPACITY_EXCEPTION` khi hết cửa sổ, như task đã xếp hàng mà không có kênh. Dòng `M3-02` của đính chính `25/09` đã sửa tại chỗ |

## Đính chính bổ sung `2026-09-26` — contract `1.0.0-draft.34` (`Q-16`, `Q-19`)

> Viết sau đính chính `K-59` cùng ngày. Chỗ nào mục này nói khác phần trên hoặc các đính chính trước thì **mục này
> thắng**. Contract lên `1.0.0-draft.34`; chỉ mô tả đổi, schema không đổi, `oasdiff` không báo breaking. Hai hành vi
> thấy được trên dây: intake từ chối sớm một loại task trước đây được nhận, và endpoint phát lại callback có giới hạn tuổi.

**Nếu chỉ đọc một câu:** *task mà IVR không đọc được thành lời nay bị `422 IVR_PII_POLICY_VIOLATION` ngay lúc gửi, thay
vì `200` rồi không bao giờ được gọi.*

| Chỗ trong phiếu | Phiếu ghi | Đúng là |
| --- | --- | --- |
| Đầu phiếu · Phần 0 dòng `1` · `D-1` | Contract hiện hành `1.0.0-draft.33` (đính chính `25/09`) | **`1.0.0-draft.34`**. [`33→34`](../docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.33-to-v1.0.0-draft.34.md); hai việc ghi ở [`docs/api-changelog.md`](../docs/api-changelog.md), mục `draft.34`. Module 3 không phải sinh lại client vì bản này |
| `privacy_safe_order_summary` | IVR nhận task rồi mới dựng lời thoại lúc quay | Từ `draft.34` (`Q-16`) intake dựng thử lời thoại bằng đúng kịch bản đã duyệt và đúng bộ dựng lời thoại của lúc quay, **trước khi nhận**. Không dựng được thì `422 IVR_PII_POLICY_VIOLATION`, cùng mã với tóm tắt đơn không qua guard: `total_amount` lớn hơn `999999999999`, lời thoại quá `1.200` ký tự, hoặc giá trị bị guard lời nói từ chối. Trước đó intake trả `200`, rồi mọi lần quay đều hỏng tới hết cửa sổ mà không ai được gọi. Module 3 sửa tóm tắt rồi gửi lại, như với lỗi tóm tắt khác |
| Tên hàng có chữ trùng dấu địa chỉ | *(không nói)* | Tên hàng được kiểm bằng guard sản phẩm ở intake, và từ `26/09` (`Q-12`) cũng ở lúc quay, nên các món yến sào, chất tạo ngọt… Module 3 gửi có dấu được nhận và được gọi. Tên hàng viết **không dấu** mà trùng chữ địa chỉ vẫn bị intake từ chối như trước |
| Endpoint phát lại callback · trạng thái nhận | `RETRY_EXHAUSTED`, `INVALID_DEAD_LETTER` | Thêm `AUTH_REJECTED` (`Q-19`, `draft.34`): khi IVR xác thực callback, một credential sai làm mọi kết quả rơi vào trạng thái này |
| Endpoint phát lại callback · tuổi | *(dòng `M3-10` · `D-5` của đính chính `25/09`)* không giới hạn tuổi | Giới hạn `7` ngày, cấu hình `1–30`; quá thì `409 IVR_VERSION_CONFLICT`. Dòng đó đã sửa tại chỗ, cùng dòng `E-8` |
