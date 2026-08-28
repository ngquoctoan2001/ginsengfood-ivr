# W-0106 — Định tuyến giọng đọc theo vùng miền (3 giọng nữ Bắc/Trung/Nam)

Trạng thái tài liệu: `PLAN_IN_EXECUTION`
Trạng thái triển khai: `TESTS_PASS` — Giai đoạn 2/3/5 xong; Giai đoạn 4 chuỗi xử lý xong chờ 3 file MP3; Giai đoạn 1 bỏ bước nghe theo `OD-VOICE-05`
Ngày lập: `2026-08-22` · Cập nhật: `2026-08-24` (ghim cấu hình audition; đóng metadata manifest; dọn §5.3 as-built)
Baseline source đã đọc: `main@f7c9be9`
Origin: `UNPLANNED` — owner requested
Prereq: `W-0104` (ACCEPTED)

> Work ID `W-0106` khớp `NEXT_WORK_ID` trong tracker §2. Chưa ghi `START`; tài liệu này
> là bản plan để owner duyệt trước khi cấp ID và thực thi.

---

## 1. Yêu cầu và kết luận rà soát

### 1.1 Yêu cầu từ sếp

1. Hệ thống hiện chỉ có **một** giọng. Cần **ba** giọng cho **ba miền**.
2. Phân loại theo **địa chỉ giao hàng**, dùng **đơn vị hành chính mới** của Việt Nam.
3. Khách ở miền nào → nghe giọng miền đó.
4. Giọng **nữ**, lấy từ ElevenLabs (giọng free) hoặc phương án tốt hơn.
5. Tiêu chí chất lượng: mới mẻ, có nhấn nhá, **không công nghiệp**, không quá "thuần AI".

### 1.2 Kết luận

Yêu cầu **khả thi và rẻ về mặt code**. Kiến trúc hiện tại đã sẵn sàng hơn dự kiến:
`AudioCacheKey` đã bao gồm `VoiceId` + `Locale`, nên đa giọng **không** gây đụng độ cache.
Điểm phải sửa chỉ là một chỗ: `SpeechSynthesisService` đang đọc `configured.VoiceId` —
một hằng số global — thay vì chọn giọng theo từng đơn.

Nhưng có **năm phát hiện** làm thay đổi phạm vi so với cách hiểu ban đầu, cần owner đọc kỹ
mục 4, 5 và 7 trước khi duyệt:

| # | Phát hiện | Hệ quả |
| --- | --- | --- |
| **F1** | Sáp nhập 2025 **không** cắt ngang ranh giới Bắc/Trung/Nam truyền thống | Map 34 tỉnh → 3 miền là **xác định, không mơ hồ**. Đây là tin tốt. |
| **F2** | Renderer sinh tiền dạng **chữ số** (`"560.000 đồng"`), nhưng audio đã được owner duyệt lại đọc dạng **chữ** ("năm trăm sáu mươi nghìn đồng") | Audio v3 được duyệt **không** sinh ra từ renderer. Thiếu bộ số→chữ. Đây là lỗi phải sửa, không phải "nice to have". |
| **F3** | Miền Bắc nói **"nghìn"**, miền Nam nói **"ngàn"** | ⇒ Owner chốt **1 template** (`2026-08-22`); biến thể này nằm trong bộ đọc số, không nằm trong template ⇒ **không** migration, **không** approval lại. Xem §5.5. |
| **F4** | ElevenLabs **Free không có commercial license**, và **Voice Library không dùng được qua API ở free tier**; default voice **hết hạn 2026-12-31** | ⇒ Đã đảo hướng 3 lần, chốt lại **ElevenLabs Starter `$6`/tháng** — xem §7.1. Free tier chỉ dùng để audition. |
| **F5** | Vendor Việt **có mất phí**, nhưng rẻ hơn ElevenLabs ~20–25×; FPT.AI có free `100.000` ký tự/tháng **dùng được thương mại** | Giai đoạn audition **không tốn tiền**. Production ở 1.000 cuộc/ngày + hybrid ≈ `1.000.000đ`/tháng. |

Giọng `Trung Caha` (`ueSxRO0nLF1bj93J2hVt`) mà `W-0104` đã `ACCEPTED` là **giọng nam**.
Yêu cầu mới là giọng nữ ⇒ **W-0104 acceptance bị thay thế về mặt nội dung giọng**.
Điều này phải được ghi nhận rõ trong tracker, không được im lặng ghi đè.

---

## 2. Hiện trạng đã xác minh từ source

### 2.1 Chuỗi tổng hợp giọng

```
ConfirmationTask (delivery_area_short)
  └─ VietnameseOrderScriptRenderer.Render()      → exactText + contentHash
       └─ SpeechSynthesisService.SynthesizeAsync()
            ├─ TtsOptions.Create(locale, configured.VoiceId, …)   ← ĐIỂM PHẢI SỬA
            ├─ AudioCacheKey.Create(tplId, ver, summaryHash, VoiceId, Locale)  ← đã đủ đa giọng
            └─ ITtsProvider.SynthesizeAsync()
                 ├─ FakeDeterministicTtsProvider   (MOCK)
                 ├─ StaticFileTtsProvider          (LAB_REAL_SIM, file PCM pin SHA-256)
                 └─ ConfigurableExternalTtsProvider (chưa implement — chờ OD-V1-19 / P8-1)
```

Nguồn: [`SpeechSynthesisService.cs:70`](../../src/Ivr.Infrastructure/Speech/SpeechSynthesisService.cs#L70),
[`SpeechServiceCollectionExtensions.cs`](../../src/Ivr.Infrastructure/Speech/SpeechServiceCollectionExtensions.cs),
[`StaticFileTtsProvider.cs`](../../src/Ivr.Infrastructure/Speech/StaticFileTtsProvider.cs).

### 2.2 Dữ liệu vị trí sẵn có

IVR **không** nhận địa chỉ đầy đủ. Trường duy nhất là `delivery_area_short`:

- Kiểu: chuỗi tự do, `maxLength: 160`
  ([`ivr-order-confirmation.v1.yaml:1155`](../../specs/api/openapi/ivr-order-confirmation.v1.yaml#L1155)).
- Ngữ nghĩa: "phường/xã, quận/huyện, tỉnh/thành" — privacy-reviewed.
- [`ShortDeliveryArea.Create()`](../../src/Ivr.Domain/Confirmation/PrivacySafeSpeech.cs#L105)
  chủ động **từ chối** mọi dấu hiệu địa chỉ chi tiết (số nhà, tên phố, ngõ, hẻm, ngách).
- **Không có** `province_code` hay bất kỳ field có cấu trúc nào.

⇒ Phân loại miền **bắt buộc** phải suy ra từ chuỗi `delivery_area_short`.
Không được yêu cầu Sales bổ sung field mới trong W-0106 (việc đó sẽ mở lại `OD-V1-04`
và làm kẹt cả work item vào lịch của team Sales).

Điểm thuận lợi: từ 2025-07-01 cấp huyện bị bỏ, nên chuỗi chỉ còn **2 tầng**
(`phường/xã` + `tỉnh/TP`) — dễ parse hơn trước.

### 2.3 Script hiện hành

`v3-test-approved`, immutable, trong
[`TargetV1SpeechPolicy.cs`](../../src/Ivr.Domain/Scripts/TargetV1SpeechPolicy.cs):

> Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood.
> Quý khách có đơn hàng gồm `{{items_spoken}}`, tổng tiền `{{total_amount_display}}`,
> giao đến `{{delivery_area_short}}`. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím
> không để hủy đơn hàng.

`ValidateTemplate()` ép buộc: đủ 3 placeholder bắt buộc, giữ nguyên hướng dẫn phím `1/0`,
cấm `phím 9`, cấm HTML/control char. Mọi biến thể script mới **phải qua hàm này**.

---

## 3. Bản đồ 34 đơn vị hành chính → 3 miền

Căn cứ Nghị quyết `202/2025/QH15` (12/6/2025), hiệu lực 1/7/2025: **34 đơn vị cấp tỉnh**
= 6 thành phố trực thuộc trung ương + 28 tỉnh; bỏ cấp huyện; cấp xã gồm xã/phường/đặc khu.

### 3.1 MIỀN BẮC — 15 đơn vị

| Đơn vị mới | Gộp từ |
| --- | --- |
| **Hà Nội** | (giữ nguyên) |
| **Hải Phòng** | Hải Phòng + Hải Dương |
| **Quảng Ninh** | (giữ nguyên) |
| **Cao Bằng** | (giữ nguyên) |
| **Lạng Sơn** | (giữ nguyên) |
| **Lai Châu** | (giữ nguyên) |
| **Điện Biên** | (giữ nguyên) |
| **Sơn La** | (giữ nguyên) |
| **Lào Cai** | Lào Cai + Yên Bái |
| **Tuyên Quang** | Tuyên Quang + Hà Giang |
| **Thái Nguyên** | Thái Nguyên + Bắc Kạn |
| **Phú Thọ** | Phú Thọ + Vĩnh Phúc + Hòa Bình |
| **Bắc Ninh** | Bắc Ninh + Bắc Giang |
| **Hưng Yên** | Hưng Yên + Thái Bình |
| **Ninh Bình** | Ninh Bình + Hà Nam + Nam Định |

### 3.2 MIỀN TRUNG — 11 đơn vị

| Đơn vị mới | Gộp từ |
| --- | --- |
| **Thanh Hóa** | (giữ nguyên) |
| **Nghệ An** | (giữ nguyên) |
| **Hà Tĩnh** | (giữ nguyên) |
| **Quảng Trị** | Quảng Trị + Quảng Bình |
| **Huế** | (giữ nguyên) |
| **Đà Nẵng** | Đà Nẵng + Quảng Nam |
| **Quảng Ngãi** | Quảng Ngãi + Kon Tum |
| **Gia Lai** | Gia Lai + Bình Định |
| **Đắk Lắk** | Đắk Lắk + Phú Yên |
| **Khánh Hòa** | Khánh Hòa + Ninh Thuận |
| **Lâm Đồng** | Lâm Đồng + Đắk Nông + Bình Thuận |

### 3.3 MIỀN NAM — 8 đơn vị

| Đơn vị mới | Gộp từ |
| --- | --- |
| **TP. Hồ Chí Minh** | TP.HCM + Bình Dương + Bà Rịa – Vũng Tàu |
| **Đồng Nai** | Đồng Nai + Bình Phước |
| **Tây Ninh** | Tây Ninh + Long An |
| **Cần Thơ** | Cần Thơ + Sóc Trăng + Hậu Giang |
| **Vĩnh Long** | Vĩnh Long + Bến Tre + Trà Vinh |
| **Đồng Tháp** | Đồng Tháp + Tiền Giang |
| **An Giang** | An Giang + Kiên Giang |
| **Cà Mau** | Cà Mau + Bạc Liêu |

15 + 11 + 8 = **34** ✓

### 3.4 Vì sao map này an toàn

Kiểm từng đơn vị được gộp: **không đơn vị nào gộp qua ranh giới Bắc/Trung hoặc Trung/Nam**.

- Ranh Bắc|Trung: `Ninh Bình` (gộp Nam Định, Hà Nam — đều Bắc) tiếp giáp `Thanh Hóa`
  (giữ nguyên). Ranh giới **nguyên vẹn**.
- Ranh Trung|Nam: `Lâm Đồng` (gộp Bình Thuận — Nam Trung Bộ) tiếp giáp `Đồng Nai`
  (gộp Bình Phước — đều Đông Nam Bộ). Ranh giới **nguyên vẹn**.

⇒ Không cần dữ liệu cấp xã để phân miền. **Chỉ cần tên tỉnh/thành.**

### 3.5 `OD-VOICE-02` — ĐÃ CHỐT

Owner quyết định ngày `2026-08-22`: **chia 3 miền thuần theo đơn vị tỉnh/thành, không
biệt lệ.** `Gia Lai`, `Đắk Lắk`, `Lâm Đồng` (Tây Nguyên) và phần Kon Tum (nay thuộc
`Quảng Ngãi`) → **Trung**, theo quy ước địa lý "Trung Bộ và Tây Nguyên".

⇒ Bảng §3.1–§3.3 là **bảng chuẩn duy nhất**. Không có ngoại lệ theo xã/phường, không có
override theo nhân khẩu học. Implement đúng bảng, không suy diễn thêm.

---

## 4. Chọn giọng

> **Trạng thái (`2026-08-22`, sau 3 vòng đảo hướng — xem §7.1)**: đề xuất
> **ElevenLabs Starter `$6`/tháng** cho production, audition trước bằng free tier.
> FPT.AI đã nghe và **loại vì chất lượng**. Viettel AI vẫn để mở nếu muốn đối chứng.
> §4.2–§4.3 giữ lại làm dữ liệu giá vendor Việt, không còn là hướng đã chọn.

### 4.1 Sự thật về "giọng free" của ElevenLabs

Đây là lý do ElevenLabs không làm production được:

| Ràng buộc | Chi tiết | Hệ quả |
| --- | --- | --- |
| Free = 10.000 credits/tháng | Script ~230 ký tự ⇒ khoảng **43 cuộc gọi/tháng** | Chỉ đủ lab, không đủ production |
| Free **không có** commercial license | Commercial license bắt đầu từ **Starter ($6/tháng)** | Gọi khách hàng thật bằng free tier là **vi phạm điều khoản** |
| Voice Library **không** gọi được qua API ở free tier | Free chỉ dùng được trên web app | `W-0104` đã sinh audio thủ công trên web — đúng cách duy nhất ở free |
| Default voices **hết hạn 2026-12-31** | ElevenLabs công bố trong docs | Không được ghim default voice ID vào production |
| Voice Library là giọng cộng đồng | Chủ giọng có thể gỡ chia sẻ bất cứ lúc nào | **Bắt buộc** có fallback khi voice ID biến mất |

Kết luận thẳng: **"3 giọng free của ElevenLabs" chạy được cho lab và cho buổi demo với sếp,
nhưng không phải là đáp án production.** Plan này tách rõ hai chuyện đó.

### 4.2 Chi phí vendor Việt (giữ làm dữ liệu đối chứng)

**Trả lời thẳng câu hỏi "vendor Việt có mất phí không": CÓ mất phí — nhưng rẻ hơn
ElevenLabs khoảng 20–25 lần, và FPT.AI có free tier dùng được cho mục đích thương mại.**

| Vendor | Free tier | Gói rẻ nhất | Giá/1 triệu ký tự | Giọng nữ 3 miền |
| --- | --- | --- | --- | --- |
| **FPT.AI** | **100.000 ký tự/tháng** — nhưng chậm, giới hạn request/ngày, giới hạn thời gian | Premium `500.000đ`/tháng = 1,5M ký tự | `333k` → `185k` (Elite) | ✅ đủ 3 miền |
| **Viettel AI** | Không công bố | Pay-as-you-go `320.000đ`/1M ký tự | `320k` → `280k` (Big), **chưa VAT 10%** | ✅ 6 giọng = nam+nữ × 3 miền |
| **VBee** | 2.000 ký tự nghe thử | Standard `799.000đ`/tháng | Không công bố số ký tự — **phải hỏi sales** | ⚠️ chưa xác minh |
| **ElevenLabs** | 10.000 credits, **không commercial license** | Starter `$6` = 30k credits | ~`2.500.000đ`/1M | ✅ nhiều lựa chọn nhất |

Gói FPT.AI đầy đủ: Premium `500k`đ/1,5M · Professional `1tr`đ/4M · Advanced `2tr`đ/10M ·
Elite `5tr`đ/27M ký tự.
Viettel: Small `3,2tr`đ/tháng/10M · Big `15tr`đ/tháng/50M.

**Điểm mấu chốt về free tier FPT.AI**: nhược điểm của nó (chậm, giới hạn request/ngày)
**không ảnh hưởng** kiến trúc lai ở §4.5 — vì pre-render là job batch offline, không phải
realtime. Đây là lý do kỹ thuật khiến free tier FPT.AI thực sự dùng được, khác hẳn free
tier ElevenLabs (bị chặn bởi license, không phải bởi tốc độ).

### 4.3 Giọng nữ 3 miền của vendor Việt (đã thử, xem §7.1)

**FPT.AI** — 7 giọng, trong đó **5 giọng nữ phủ đủ 3 miền**
([docs chính thức](https://docs.fpt.ai/docs/vi/speech/api/text-to-speech.html)):

| Miền | Giọng nữ | Ghi chú |
| --- | --- | --- |
| **Bắc** | `banmai`, `thuminh` | 2 lựa chọn — thoải mái chọn |
| **Trung** | `myan` | ⚠️ **chỉ 1 lựa chọn duy nhất** |
| **Nam** | `lannhi`, `linhsan` | 2 lựa chọn |

**Viettel AI** — 6 giọng = nam + nữ × 3 miền ⇒ đúng 1 giọng nữ/miền, không có lựa chọn thay thế.

⚠️ **Rủi ro tập trung vào giọng miền Trung.** Cả FPT.AI lẫn Viettel đều chỉ có **một**
giọng nữ miền Trung. Nếu sếp không ưng giọng đó thì **không có phương án thay thế trong
cùng vendor** — phải nhảy sang §4.4. Vì vậy Giai đoạn 1 phải nghe **giọng Trung trước
tiên**, không để đến cuối. Đây là điểm quyết định của cả work item.

> Một search phụ có nhắc giọng nữ `ngoclam` (giọng Huế) của FPT.AI nhưng docs API chính
> thức chỉ liệt kê 7 giọng ở trên. Phải xác minh trong console FPT.AI ở task 1.1 — nếu
> `ngoclam` có thật thì miền Trung có 2 lựa chọn, giảm được rủi ro trên.

### 4.4 Shortlist giọng nữ ElevenLabs (dự phòng)

⚠️ **Cảnh báo về voice ID**: bảng dưới lấy từ catalog bên thứ ba. Tôi đã tìm được bằng
chứng catalog đó sai: nó gán `ueSxRO0nLF1bj93J2hVt` cho "Giọng đọc Trung — nam, miền Bắc",
trong khi chính repo này ghi ID đó là `Trung Caha - Clear, Firm and Informative`
([`manifest.txt`](../../deploy/lab/asterisk/audio/manifest.txt)). ElevenLabs cũng xác nhận
tên "Trung Caha — Clear, Firm and Informative" là có thật.
⇒ **Mọi voice ID trong bảng phải được xác minh lại trực tiếp trong ElevenLabs app** trước
khi ghim vào manifest. Coi bảng này là danh sách **tên giọng để tìm**, không phải ID đã chốt.

| Miền | Ưu tiên 1 | Ưu tiên 2 | Dự phòng |
| --- | --- | --- | --- |
| **Bắc** | **Thắm** — trẻ, dịu, ấm, giọng đáng tin | **Mai** — Hà Nội, tự nhiên, sáng | **Hien** — phát thanh viên chuyên nghiệp, Hà Nội |
| **Trung** | **Zara** — giọng Đà Nẵng rõ, ấm, tự nhiên, biểu cảm | **Huyen** — Đà Nẵng, bình tĩnh, thân thiện, rõ | **Duyen** — sáng, rõ, ấm chất Nam Trung Bộ |
| **Nam** | **Thanh** — giọng Sài Gòn chất lượng cao | **Giang** — Sài Gòn, ấm, tự tin | **HTN** — bình tĩnh, ấm, tự nhiên |

**Lý do chọn theo tiêu chí "không công nghiệp":**

- **Bắc — chọn Thắm chứ không chọn Hien.** Giọng phát thanh viên đúng là chuẩn, nhưng đó
  chính xác là cái "công nghiệp" mà sếp không muốn. Cuộc gọi xác nhận đơn hàng cần cảm giác
  *người thật gọi cho mình*, không phải bản tin.
- **Trung — bắt buộc chọn giọng Đà Nẵng, không chọn Huế.** Miền Trung trải từ Thanh Hóa tới
  Khánh Hòa, giọng Huế/Quảng Trị rất đặc trưng và khó nghe với người ngoài vùng. Đà Nẵng là
  "giọng Trung an toàn", dễ hiểu nhất trên toàn miền.
- **Nam — chọn giọng Sài Gòn, không chọn miền Tây.** Miền Nam gồm cả Đồng bằng sông Cửu Long;
  giọng Sài Gòn trung tính phủ tốt cả vùng, giọng miền Tây đặc sệt thì không.

### 4.5 Cấu hình để giọng có nhấn nhá, không "thuần AI"

Áp dụng cho cả 3 giọng, bám theo mục 3 của
[`voice-modernization-proposal.md`](../../docs/evidence/W-0104/voice-modernization-proposal.md):

- Model **Eleven v3**, Language **Auto detect** (đúng model đã dùng cho voice C).
- `stability` **0.40** — đủ nhấn nhá nhưng không trôi giọng giữa các lần render.
- `similarity_boost` **0.75**; `style` **thấp**. Không bật style cao: nó tạo kịch tính giả.
- **Ngắt nhịp thật trong text**, không dựa vào dấu phẩy: nghỉ sau lời chào, sau danh sách
  hàng, sau tổng tiền, và **trước** câu hướng dẫn phím.
- Nhấn rõ **"phím một"** và **"phím không"**. Không nhạc nền.
- Chuẩn hóa loudness **trước** khi hạ 8 kHz, tránh giọng nhỏ hoặc vỡ trên PCMU.
- Tốc độ **`-3%`** so với mặc định.

### 4.6 Kiến trúc lai — cắt 68% chi phí ký tự

Script v3 dài **300 ký tự**, tách được thành hai loại (đo trên chính script đã duyệt):

| Phần | Ký tự | Tỉ lệ | Tính chất |
| --- | --- | --- | --- |
| **Cố định** — lời chào, "đây là cuộc gọi tự động…", "tổng tiền", "giao đến", "bấm phím một…" | **203** | **68%** | Render **một lần duy nhất, vĩnh viễn** |
| **Biến thiên** — `items_spoken`, `total_amount_display`, `delivery_area_short` | **97** | **32%** | Cần TTS động, nhưng cache được |

**Cách làm**: pre-render 4 đoạn cố định × 3 miền = **12 file**, ghim SHA-256, nhúng vào
image. Chi phí TTS runtime của phần này = **0**. Latency = **0**. Và quan trọng nhất:
**nội dung đơn hàng của phần này không rời khỏi mạng nội bộ**.

Tổng chi phí một lần cho toàn bộ đoạn cố định: `203 × 3 = 609 ký tự`. Con số này lọt vào
**bất kỳ** free tier nào, kể cả ElevenLabs.

**Khối lượng ký tự/tháng theo lưu lượng** (script 300 ký tự, hybrid chỉ tính 97):

| Lưu lượng | Full-TTS | **Hybrid** | Gói FPT.AI cần cho hybrid |
| --- | --- | --- | --- |
| 200 cuộc/ngày | 1,80M | **0,58M** | Premium `500.000đ`/tháng |
| 500 cuộc/ngày | 4,50M | **1,46M** | Premium `500.000đ`/tháng |
| 1.000 cuộc/ngày | 9,00M | **2,91M** | Professional `1.000.000đ`/tháng |
| 2.000 cuộc/ngày | 18,00M | **5,82M** | Advanced `2.000.000đ`/tháng |

**Và cache còn kéo xuống nữa.** Cả 3 ô biến đều thuộc tập hữu hạn:
- `delivery_area_short` — 34 tỉnh (hoặc ~3.321 phường/xã, vẫn là tập đóng);
- `total_amount_display` — tập giá trị đơn hàng thực tế, lặp lại nhiều;
- `items_spoken` — tổ hợp SKU trong catalog, lặp lại nhiều.

`AudioCacheKey` đã ghép sẵn `summaryHash` + `VoiceId` ⇒ cache tự hoạt động, không cần code
thêm. Ở trạng thái cache ấm, chi phí TTS định kỳ tiệm cận **0**; chỉ tổ hợp SKU mới / số
tiền mới / phường mới mới phát sinh render.

⇒ Với hybrid + cache ấm, lưu lượng thực tế nhiều khả năng nằm ở **gói rẻ nhất**, thậm chí
lọt free tier FPT.AI ở giai đoạn pilot (`100.000 ký tự ÷ 97 ≈ 1.030 cuộc gọi/tháng`).

Kiến trúc này tận dụng đúng `StaticFileTtsProvider` + `AudioCache` đã có sẵn.

> **Phạm vi**: W-0106 **chưa** implement hybrid. W-0106 chỉ làm định tuyến giọng theo miền
> và giữ `ITtsProvider` làm ranh giới độc lập vendor. Hybrid là work item riêng, gắn với
> `P8-1`/`OD-V1-19`. Ghi ở đây vì nó quyết định con số trong `OD-VOICE-01`.

### 4.7 So sánh cuối — sau khi nghe thử thật

Bảng này thay bản so sánh cũ (vốn kết luận "vendor Việt thắng"). Kết luận đó dựa trên phép
tính full-TTS mỗi cuộc gọi và trên giả định chất lượng FPT.AI đủ dùng — **cả hai đều sai**.
Lịch sử đảo hướng ghi ở §7.1.

| Tiêu chí | **ElevenLabs Starter** | FPT.AI | Viettel AI |
| --- | --- | --- | --- |
| Giá thực với kiến trúc lai (§4.6) | **`$6` ≈ `150.000đ`/tháng** | `500.000đ`/tháng | `320.000đ`/1M ký tự |
| Chất lượng giọng | **Tốt nhất** — owner đã chấp nhận ở W-0104 | ❌ **Owner đánh giá không đạt** (`2026-08-22`) | Chưa nghe |
| Catalog giọng nữ miền Trung | **Hàng chục** | Đúng 1 (`myan`) ⚠️ | Đúng 1 ⚠️ |
| Commercial license | Từ `$6`/tháng | Có từ free tier | Có |
| Data residency trong nước | ❌ Không | ✅ Có | ✅ Có |
| Rủi ro giọng biến mất | ⚠️ **Cao** — giọng cộng đồng; default voice hết hạn `31/12/2026` | Thấp | Thấp |
| Giới hạn free tier | 10.000 credits, **không commercial** | 100k ký tự **+ cap request/ngày** | Không công bố |

**Hai rủi ro còn lại của việc chọn ElevenLabs**, cả hai đều phải xử trước production:

1. **Data residency** — nội dung đơn rời khỏi Việt Nam. Kiến trúc lai giảm mạnh (68% câu nói
   là đoạn cố định, render một lần, không chứa dữ liệu đơn), nhưng không triệt tiêu. Vẫn thuộc
   `OD-V1-19`.
2. **Giọng cộng đồng có thể biến mất.** Bắt buộc có `FallbackRegion` (đã code) + ghim SHA-256
   file PCM trong image (Giai đoạn 4) để một voice ID bị gỡ không làm đứt dịch vụ.

---

### 4.8 Tự host / clone giọng — phân tích đầy đủ

Câu hỏi của owner (`2026-08-22`): *"không có cách nào clone giọng về dùng luôn hả? Chỉ đọc
văn bản thôi mà. Thuê ngoài tiền nào chịu nổi. Nhúng Python vào được không?"*

Câu hỏi đúng. Nhưng có một **bãi mìn license** phải đi qua trước.

#### 4.8.1 Bãi mìn: model tiếng Việt open-source hầu hết KHÔNG dùng thương mại được

| Model | Code | **Weights** | Tiếng Việt | Dùng thương mại? |
| --- | --- | --- | --- | --- |
| `viXTTS` (capleaf) | — | **CPML** | ✅ fine-tune viVoice | ❌ **KHÔNG** — và Coqui đã đóng cửa 1/2024 nên **không còn ai để mua license**. Ngõ cụt tuyệt đối |
| `F5-TTS` (SWivid) official | MIT | **CC-BY-NC-4.0** | ✅ có bản finetune VN | ❌ **KHÔNG** (weights) |
| `OpenF5-TTS-Base` | MIT | **Apache 2.0** ✅ | ❌ **chỉ tiếng Anh** | ✅ nhưng **phải tự fine-tune tiếng Việt** |
| `Piper` | MIT ✅ | **tùy từng giọng** | `vi_VN-vais1000`, `vi_VN-vivos`, `vi_VN-25hours` | ⚠️ phải đọc MODEL_CARD từng giọng; giọng `vivos` dựa trên dataset CC-BY-NC ⇒ **không** thương mại. Chất lượng `x_low`/`low` thấp |
| Tự train VITS/F5 trên **dữ liệu của mình** | MIT/Apache | **của mình** | ✅ | ✅ **Sạch hoàn toàn** |

> **Kết luận thẳng: không có model tiếng Việt open-source nào vừa chất lượng cao vừa sạch
> pháp lý để bê về dùng ngay.** Mọi hướng sạch đều dẫn về cùng một chỗ:
> **phải có dữ liệu giọng của chính mình.**

#### 4.8.2 Clone giọng của ai — đây là câu hỏi pháp lý, không phải câu hỏi kỹ thuật

| Clone từ | Hợp pháp? |
| --- | --- |
| Output của FPT.AI / Viettel / ElevenLabs | ❌ Vi phạm ToS của họ (cấm dùng output để train model cạnh tranh) + vấn đề IP |
| Giọng người thật không xin phép (MC, ca sĩ, người quen) | ❌ Giọng nói là dữ liệu cá nhân theo **Nghị định 13/2023/NĐ-CP**; thêm rủi ro quyền nhân thân |
| **Voice actor thuê có hợp đồng + license, hoặc nhân viên công ty ký đồng ý** | ✅ **Sạch. Đây là hướng duy nhất.** |

#### 4.8.3 Nhúng Python vào .NET?

| Cách | Đánh giá |
| --- | --- |
| `Python.NET` / `IronPython` nhúng trong process .NET | ❌ **Không.** GIL, và base image runtime là **chiseled + globalization-invariant** (xem chính comment trong [`VietnameseOrderScriptRenderer.cs`](../../src/Ivr.Domain/Scripts/VietnameseOrderScriptRenderer.cs)) — không có Python trong đó. Thêm Python vào là phá vỡ toàn bộ mô hình image hiện tại |
| Python thành **service riêng** (FastAPI) sau `ITtsProvider` | ✅ Sạch về kiến trúc. Thêm `SelfHostedTtsProvider` gọi HTTP |
| **Python thành công cụ BATCH offline, không phải service runtime** | ✅✅ **Tốt nhất — xem 4.8.4** |

#### 4.8.4 Phát hiện quan trọng nhất: hệ thống này KHÔNG cần TTS lúc chạy

`StaticFileTtsProvider` + `SHA256SUMS` + file sound trong image Asterisk — thứ đang được gắn
nhãn "LAB-only" — **chính là kiến trúc production đúng** cho một IVR script cố định.

Vì script là template cố định, mọi câu nói đều pre-render được. Điều đó có nghĩa:

- **Không** service TTS nào chạy trong production ⇒ không latency budget, không availability
  risk, không scaling, không GPU.
- Python chỉ chạy **lúc build/ops** để sinh file WAV, đúng như quy trình
  [`deploy/lab/`](../../deploy/lab/) đang làm.
- Model chậm cũng **không sao** — batch offline, không ai chờ.
- **Nội dung đơn hàng không bao giờ rời khỏi mạng nội bộ** ⇒ đóng luôn phần lớn
  `OD-V1-19` về privacy/data residency.

#### 4.8.5 Ba phương án

| | **A — Vendor Việt** (đang trong plan) | **B — Tự host model open-source** | **C — Thu âm người thật + concatenative** |
| --- | --- | --- | --- |
| Chi phí định kỳ | ~`1.000.000đ`/tháng | **0đ** | **0đ** |
| Chi phí một lần | 0 | Effort engineering + fine-tune | **~10–15tr** thuê 3 voice actor |
| Chất lượng | Đã biết, ổn | Rủi ro — phải tự fine-tune tiếng Việt | ✅ **100% người thật** |
| Tiêu chí "không công nghiệp, ko thuần AI" | Khá | Khá | ✅ **Đạt tuyệt đối — vì nó không phải AI** |
| Pháp lý | Sạch | ⚠️ Chỉ sạch nếu tự train trên data của mình | ✅ Sạch (hợp đồng voice actor) |
| Giọng nữ miền Trung | ⚠️ **Chỉ 1 lựa chọn** (`myan`) | Tùy data | ✅ **Chọn thoải mái** — đóng luôn rủi ro `R0` |
| Đọc được text tùy ý | ✅ | ✅ | ❌ Chỉ đọc được tập đã thu |
| Rủi ro chính | Phụ thuộc vendor | Chất lượng + effort | Prosody ở mối nối |

#### 4.8.6 Khuyến nghị: **C + B lai** (`OD-VOICE-04`)

Tách nội dung theo tính đóng/mở của tập giá trị:

| Thành phần | Tập | Cách làm | Số clip/giọng |
| --- | --- | --- | --- |
| 4 đoạn cố định (203 ký tự) | Đóng | **Người thật đọc** | 4 |
| Từ số tiếng Việt (`một`…`tỷ`, `lăm`, `tư`, `linh/lẻ`, `nghìn/ngàn`) | Đóng, ~40 đơn vị | **Người thật đọc**, thu ở 3 vị trí ngữ điệu (đầu/giữa/cuối) để mối nối mượt | ~120 |
| 34 tên tỉnh/thành | Đóng | **Người thật đọc** | 34 |
| Tên SKU + đơn vị tính | Đóng (theo catalog) | **Người thật đọc** | ~60 |
| **Tên phường/xã** (~3.321) | **MỞ** — điểm nghẽn duy nhất | Model tự host, **clone từ chính giọng voice actor đó** (hợp pháp, vì đã có hợp đồng) + pre-render dần theo đơn thực tế, cache vĩnh viễn | tăng dần |

Voice actor đọc ~220 clip/giọng ≈ **2–3 tiếng studio**. Ba giọng ⇒ một buổi thu.

Vì sao lai được: đã ký hợp đồng với voice actor ⇒ **có quyền clone chính giọng đó** ⇒ đuôi
do model đọc **khớp timbre** với phần người thật, không bị lệch giọng giữa câu.

Và tập phường/xã thực tế **nhỏ hơn 3.321 rất nhiều** — công ty thực phẩm không giao tới mọi
phường. Pre-render dần theo đơn thật, sau vài tháng là phủ gần hết vùng giao hàng.

#### 4.8.7 Quan hệ giữa audition và đích dài hạn

Phương án C (thu âm người thật) cần **buổi thu + hợp đồng voice actor** — không làm xong
trong tuần này. ElevenLabs free tier **dựng được mẫu 3 miền ngay hôm nay** để sếp nghe.

⇒ **Không mâu thuẫn**: ElevenLabs chốt hướng và chốt kịch bản ngay; C là đích dài hạn nếu
muốn cắt hẳn chi phí định kỳ. Cả hai đều nằm sau `ITtsProvider` — đổi qua lại chỉ là đổi
config + thay file, không sửa code.

**Phép so tiền để cân nhắc C**: `$6`/tháng × **83 tháng ≈ 10–15tr** = chi phí thu âm một
lần. Nghĩa là C chỉ đáng làm khi hệ thống đã chạy ổn định và muốn dứt phụ thuộc vendor —
không phải việc cần làm bây giờ.

---

## 5. Thiết kế kỹ thuật

### 5.1 Nguyên tắc: không đụng vào contract

Impact analysis (mục 9) cho thấy `PrivacySafeOrderSummary` là **HIGH risk**: 95 symbol,
20 caller trực tiếp, 2 execution flow. Vì vậy:

> **Không thêm field vào `PrivacySafeOrderSummary`, không sửa OpenAPI, không migration schema
> cho việc phân miền.** Miền được suy ra như một **hàm thuần túy** của
> `ShortDeliveryArea.Value` ngay tại tầng speech.

Đây là điểm mấu chốt giúp W-0106 rẻ và an toàn.

### 5.2 Thành phần mới

**(a) `src/Ivr.Domain/Speech/VietnamRegion.cs`**

```csharp
public enum VietnamRegion { North, Central, South }
```

**(b) `src/Ivr.Domain/Speech/DeliveryRegionResolver.cs`** — hàm thuần túy, không I/O

- `FrozenDictionary<string, VietnamRegion>` với **34 khóa** đã chuẩn hóa
  (bỏ dấu, lowercase, bỏ tiền tố `tinh|thanh pho|tp|t\.p`).
- Thuật toán: chuẩn hóa `delivery_area_short` → tách theo `,` → **quét token từ phải sang
  trái** (tỉnh luôn ở cuối) → khớp chính xác với bảng 34.
- **Alias tên cũ**: chấp nhận 29 tên tỉnh đã bị xóa (Hà Giang, Bến Tre, Bình Dương…) map về
  đơn vị mới. Lý do: Sales có thể còn dữ liệu đơn cũ hoặc chưa cập nhật master data.
  Không có alias thì mọi đơn tồn sẽ rơi về fallback.
- **Không khớp → `null`**, không đoán. Caller quyết định fallback.

**(b2) `src/Ivr.Domain/Speech/VietnameseTextNormalizer.cs`** — *(bổ sung khi triển khai)*

Chuẩn hóa khớp tên địa danh. Cố ý **không** dùng chung với bản gấp dấu riêng tư của
`ShortDeliveryArea`: bản đó là **guard privacy**, bản này là **helper tra cứu**. Gộp lại thì
một thay đổi để nới khớp tên địa danh có thể âm thầm nới lỏng guard địa chỉ.
`UT-VOICE-REGION-09` ghim hai bản vào cùng một corpus để chúng không trôi khỏi nhau.

**(c) `src/Ivr.Infrastructure/Speech/RegionalVoiceMap.cs`** — cấu hình, không hard-code

Hình dạng **as-built** (khác bản phác thảo: mỗi miền mang cả media reference cho LAB):

```jsonc
"Ivr:Speech:Tts:RegionalVoices": {
  "Enabled": true,
  "FallbackRegion": "North",          // dùng khi resolver trả null
  "North":   { "VoiceId": "…", "SpeakingRate": 0, "FileMediaReference": "sound:…", "FileDurationSeconds": 18 },
  "Central": { "VoiceId": "…", "SpeakingRate": 0, "FileMediaReference": "sound:…", "FileDurationSeconds": 18 },
  "South":   { "VoiceId": "…", "SpeakingRate": 0, "FileMediaReference": "sound:…", "FileDurationSeconds": 18 }
}
```

`Resolve` trả `RegionalVoiceSelection(Region, ResolvedFromDeliveryArea, VoiceId, SpeakingRate)`
chứ không trả mỗi chuỗi — cờ `ResolvedFromDeliveryArea` là thứ tách "khách miền Nam thật" khỏi
"không suy được nên fallback" trong metric.

**(d) `src/Ivr.Infrastructure/Speech/SpeechSynthesisService.cs`**

Chọn giọng đúng một lần bằng `RegionalVoiceMap.Resolve(summary.DeliveryArea.Value)`, rồi truyền
`VoiceId` đã chọn xuyên suốt TTS, static-file lookup, telemetry và cache.

**(e) `ScriptRenderOptions.FallbackRegion`** — *(bổ sung khi triển khai)*

Renderer và voice map phải fallback về **cùng một miền**. Nếu mỗi bên tự mặc định thì một địa
chỉ không suy được sẽ cho giọng Nam đọc "nghìn" mà không có gì báo lỗi.
`ApprovedVietnameseSpeechRenderer` truyền `regionalVoices.FallbackRegion` xuống renderer.

`AudioCacheKey` đã có `VoiceId` ⇒ **không sửa cache**. Mỗi miền có không gian cache riêng,
tự nhiên, không cần đụng tới `AudioCache.cs`.

### 5.3 Lab assets (LAB_REAL_SIM) — as-built

Bản phác thảo ban đầu định thêm `FileMediaReferenceByRegion` và mở rộng
`Set-AsteriskLabVoice.ps1` để **chuyển** giữa ba biến thể. As-built khác ở ba điểm:

| | Phác thảo | **As-built** | Vì sao đổi |
| --- | --- | --- | --- |
| Tên file | `-n` / `-c` / `-s` | `-region-north` / `-central` / `-south` | Hậu tố `-c` **đã thuộc về voice C của W-0104**; dùng lại sẽ đè lên evidence cũ |
| Nơi khai media | `FileMediaReferenceByRegion` riêng | Nằm trong từng entry của `RegionalVoices` | Một chỗ khai giọng, không tách đôi giọng và file ra hai block dễ lệch |
| Cách chọn | `Set-AsteriskLabVoice.ps1` chuyển file lúc boot | **Cả ba file cùng tồn tại**, app chọn theo từng cuộc gọi | W-0104 chỉ cần một giọng cho cả lab; W-0106 phải phát giọng khác nhau cho từng cuộc gọi, nên không thể chọn lúc boot |

- 3 file PCM signed 16-bit / 8 kHz / mono, ghim SHA-256 trong
  [`SHA256SUMS`](../../deploy/lab/asterisk/audio/SHA256SUMS) và `manifest.txt`.
- [`entrypoint.sh`](../../deploy/lab/asterisk/entrypoint.sh) `sha256sum --check --strict`
  **toàn bộ** manifest trước khi Asterisk chạy, rồi cài cả ba file vùng miền song song.
  Thiếu file nào thì log rõ và bỏ qua — `StaticFileTtsProvider` tự fail-closed khi giọng được
  chọn không có media.
- `IVR_LAB_VOICE_VARIANT` (A/B/C) và `Set-AsteriskLabVoice.ps1` **giữ nguyên** cho nhánh
  một-giọng của W-0104. Không xóa file `a`/`b`/`c`.
- [`Convert-LabVoiceAudio.ps1`](../../deploy/lab/Convert-LabVoiceAudio.ps1) chuẩn hóa loudness
  **trước** khi hạ 8 kHz, chạy ffmpeg `bitexact` + `-map_metadata -1`, và ghi voice ID thật
  cùng cấu hình render cố định vào `manifest.txt`.

### 5.4 Sửa lỗi F2: số tiền phải đọc bằng chữ

`VietnameseOrderScriptRenderer` hiện sinh:

```csharp
summary.Total.Amount.ToString("N0", VietnameseNumbers) + " đồng"   // → "560.000 đồng"
```

Nhưng audio đã được owner duyệt lại đọc **"năm trăm sáu mươi nghìn đồng"**. Nghĩa là bản
audio v3 được gõ tay trên web ElevenLabs, **không** sinh từ renderer. Khi nối TTS thật vào,
engine sẽ nhận chuỗi `"560.000"` — và cách đọc chuỗi đó **không xác định** (có engine đọc
"năm trăm sáu mươi nghìn", có engine đọc "năm trăm sáu mươi phẩy không không không").

Không có bộ chuyển số→chữ nào trong `src/` (đã grep, 0 hit).

⇒ **Thêm `VietnameseNumberSpeller`** trong `Ivr.Domain`, deterministic, không phụ thuộc ICU
(cùng lý do đã ghi trong comment của `CreateVietnameseNumbers` — image chạy
globalization-invariant mode). Phủ: đơn vị → nghìn/triệu/tỷ, các ca `linh/lẻ`, `mươi/mười`,
`lăm/năm`, `tư/bốn`, và biến thể `nghìn`/`ngàn` theo miền (xem F3).

Đây là **điều kiện bắt buộc** để bất kỳ giọng nào nghe đúng, không riêng gì đa giọng.

### 5.5 `OD-VOICE-03` — ĐÃ CHỐT: một template

Owner quyết định ngày `2026-08-22`: **giữ đúng MỘT template** (`v3-test-approved`).
Không tách 3 biến thể script theo miền.

Hệ quả bắt buộc: biến thể `nghìn`/`ngàn` phải nằm **trong `VietnameseNumberSpeller`**
(tham số theo miền), **không** nằm trong template.

| | **A — 1 template (ĐÃ CHỌN)** | B — 3 template (loại) |
| --- | --- | --- |
| Số script version | **1** | 3 |
| Approval / migration | **1 lần** | 3 lần (`MOCK_TEST`+`LAB` mỗi bản) |
| Giọng Nam nói đúng "ngàn" | **Có** — nhờ speller theo miền | Có |
| Chi phí evidence | **Thấp** | Cao gấp 3 |
| Rủi ro drift giữa các bản | **Không** | Có |

Lý do phương án A vẫn đọc đúng "ngàn" cho miền Nam: **"nghìn/ngàn" là *cách đọc số*, không
phải *nội dung nghiệp vụ*.** Nó thuộc bộ đọc số, không thuộc template.

⇒ `TemplateText` không đổi ⇒ `TemplateHash` không đổi ⇒ **không cần migration script**,
không cần approval lại, `TargetV1SpeechPolicy.ValidateTemplate()` không phải sửa.

### 5.6 Quan sát được

Thêm vào `TtsTelemetry` (không log PII):
- `ivr_tts_voice_selected_total{region}` — đếm theo miền.
- `ivr_tts_region_unresolved_total` — đếm số lần resolver trả `null` và phải fallback.
  **Metric này là tín hiệu chất lượng dữ liệu Sales** — nếu nó tăng, master data đang drift.
- Cache hit rate hiện có sẽ tự tách theo giọng nhờ `AudioCacheKey`.

Admin UI: hiển thị miền đã chọn ở màn chi tiết cuộc gọi (`calls/[ivrCallJobId]`).
Chỉ đọc, không cho sửa.

---

## 6. Kế hoạch triển khai

### Giai đoạn 0 — Quyết định

| # | Việc | Ai | Trạng thái |
| --- | --- | --- | --- |
| 0.1 | `OD-VOICE-01` — nguồn giọng production | Owner + Product | 🟡 **ElevenLabs Starter đề xuất** sau 3 vòng đảo hướng (§7.1). Đóng sau khi audition + xác nhận ToS |
| 0.2 | `OD-VOICE-02` — phân miền theo tỉnh/thành, không biệt lệ | Owner | ✅ **CHỐT** `2026-08-22` |
| 0.3 | `OD-VOICE-03` — một template | Owner | ✅ **CHỐT** `2026-08-22` |
| 0.4 | Ghi nhận W-0104 acceptance bị thay bằng bộ 3 giọng nữ | Owner + tracker | ⬜ chưa làm |

### Giai đoạn 1 — Audition & chọn giọng (không đụng code)

Owner chốt bỏ qua bước nghe (`2026-08-22`) với lý do đổi giọng sau này rẻ — đúng, vì giọng
nằm trong config. Hệ quả và ranh giới ghi ở `OD-VOICE-05` §7.2.

Bộ kit đầy đủ — kịch bản từng miền, cấu hình render, 9 giọng, bảng chấm cho owner — nằm ở
[`docs/evidence/W-0106/voice-audition-kit.md`](../../docs/evidence/W-0106/voice-audition-kit.md).

| # | Việc | Đầu ra |
| --- | --- | --- |
| 1.1 | ❌ **FPT.AI đã thử và loại** (`2026-08-22`): owner đánh giá giọng không đạt, và free tier hết lượt ngay vì có **giới hạn request/ngày** chứ không chỉ giới hạn ký tự. Console vẫn còn trên tài khoản doanh nghiệp nếu cần đối chứng | — |
| 1.2 | Audition trên **ElevenLabs web app free tier** (10.000 credits/tháng) theo [`voice-audition-kit.md`](../../docs/evidence/W-0106/voice-audition-kit.md) | Bộ kit đã sẵn sàng |
| 1.3 | ✅ **Ba giọng đã chốt** (`2026-08-22`, owner bỏ qua bước nghe): **Thắm** (Bắc), **Zara** (Trung), **Giang** (Nam) — xem `OD-VOICE-05` §7.2 | Quyết định đã ghi |
| 1.4 | Render đúng 3 giọng đã chọn để lấy file cho Giai đoạn 4 | **900 / 10.000** credits |
| 1.5 | ⏸️ **HOÃN** — sếp nghe và ký nhận. Không chặn Giai đoạn 4, nhưng W-0106 chỉ đạt `TESTS_PASS` chừng nào chưa có | Biên bản chọn giọng |
| 1.6 | **Xác minh voice ID thật trong app** (không lấy từ catalog bên thứ ba) | Bảng tên↔ID đã verify |
| 1.7 | Mua **ElevenLabs Starter `$6`** ⇒ sinh lại bản production có commercial license ⇒ đóng `OD-VOICE-01` | Quyết định + cost model + xác nhận ToS |
| 1.8 | Hạ về PCM 16-bit/8 kHz/mono, chuẩn hóa loudness, ghim SHA-256 vào `manifest.txt` | file PCM + hash |

> Dữ liệu dùng ở giai đoạn này **chỉ là fixture fake** (chị An/đơn fake hiện có).
> Không số điện thoại, không địa chỉ đầy đủ, không dữ liệu khách thật.
> 9 lần render × ~300 ký tự ≈ **2.700 / 10.000 credits** — lọt free tier ElevenLabs, còn dư
> ~7.300 để render lại khi chỉnh tốc độ/ngắt nhịp. **Giai đoạn 1 không tốn tiền.**
>
> ⚠️ Free tier ElevenLabs **không có commercial license**. Bước này chỉ chứng minh chất lượng
> giọng; audio production phải sinh lại sau khi mua gói ở task 1.7.

### Giai đoạn 2 — Domain: phân miền + đọc số ✅ **ĐÃ XONG** (`2026-08-22`)

| # | Việc | Trạng thái | Test |
| --- | --- | --- | --- |
| 2.1 | [`VietnamRegion`](../../src/Ivr.Domain/Speech/VietnamRegion.cs) + [`DeliveryRegionResolver`](../../src/Ivr.Domain/Speech/DeliveryRegionResolver.cs) + bảng 34 + 29 alias tên cũ | ✅ | `UT-VOICE-REGION-01..10` |
| 2.2 | [`VietnameseNumberSpeller`](../../src/Ivr.Domain/Speech/VietnameseNumberSpeller.cs) + `VietnameseNumberStyle` theo miền | ✅ | `UT-VOICE-NUM-01..09` |
| 2.3 | Nối speller vào [`VietnameseOrderScriptRenderer`](../../src/Ivr.Domain/Scripts/VietnameseOrderScriptRenderer.cs) | ✅ | 7 test cũ cập nhật |
| 2.4 | [`VietnameseTextNormalizer`](../../src/Ivr.Domain/Speech/VietnameseTextNormalizer.cs) — chuẩn hóa khớp tên địa danh | ✅ | `UT-VOICE-REGION-09` |

**Impact analysis trước khi sửa** (`CLAUDE.md` bắt buộc): `VietnameseOrderScriptRenderer`
= **LOW**, 4 impacted, 4 direct, **0 execution flow**. `PrivacySafeOrderSummary`
(**HIGH**, 95) **không bị đụng tới** — miền suy ra tại chỗ từ `DeliveryArea.Value`.

**Kết quả kiểm chứng**

Xem §6 "Kết quả regression toàn bộ" bên dưới — cả 4 suite đã chạy xanh sau khi owner
tắt process Worker.

**Phát hiện bổ sung khi triển khai — F2 rộng hơn đã ghi**

Không chỉ số tiền. **Số lượng cũng bị emit dạng chữ số**: renderer sinh `"2 hộp"` trong khi
audio v3 owner đã duyệt nói `"hai hộp"`. Cả hai đều đã sửa, cộng `"và 1 sản phẩm khác"` →
`"và một sản phẩm khác"`.

Bảy test cũ đang **pin chính cái lỗi này** và đã được cập nhật sang dạng chữ:
`ScriptContentTests` (×3), `MockTelephonyTests` (×3), `TtsProviderTests` (×2).

**Một điểm còn mở**: số lượng **thập phân** (`2,5 kg`) vẫn giữ dạng chữ số — TTS đọc
"hai phẩy năm" chấp nhận được, nhưng **concatenative (§4.8.6) không ghép được số thập phân
từ clip thu sẵn**. Cần quyết định trước khi buổi thu âm diễn ra.

**Bộ test bắt buộc cho 2.1** — mỗi ca là một dòng test, không gộp:
- 34/34 tỉnh mới → đúng miền (bảng §3).
- 29/29 tên tỉnh cũ → đúng miền của đơn vị mới.
- Có/không dấu, có/không tiền tố `tỉnh`/`thành phố`/`TP.`/`TP`.
- Chuỗi 2 tầng mới (`"phường X, tỉnh Y"`) **và** chuỗi 3 tầng cũ (`"phường X, quận Y, TP Z"`).
- Ca bẫy: `"phường Phú Khương, tỉnh Vĩnh Long"` → **South** (Phú Khương vốn thuộc Bến Tre).
- Ca bẫy: tên xã trùng tên tỉnh (ví dụ có xã tên "Hà Giang") → quét phải-sang-trái phải
  vẫn lấy đúng tỉnh ở cuối chuỗi.
- Không khớp → `null`, **không** đoán bừa.

### Giai đoạn 3 — Infrastructure: định tuyến giọng ✅ **ĐÃ XONG** (`2026-08-22`)

| # | Việc | Trạng thái | Test |
| --- | --- | --- | --- |
| 3.1 | [`RegionalVoiceMap`](../../src/Ivr.Infrastructure/Speech/RegionalVoiceMap.cs) + `RegionalVoiceOptions` + validator | ✅ | `UT-VOICE-CFG-01..04` |
| 3.2 | [`SpeechSynthesisService`](../../src/Ivr.Infrastructure/Speech/SpeechSynthesisService.cs) chọn giọng theo miền | ✅ | `UT-SPEECH-VOICE-01..03` |
| 3.3 | [`StaticFileTtsProvider`](../../src/Ivr.Infrastructure/Speech/StaticFileTtsProvider.cs) map file theo giọng | ✅ | `UT-TTS-STATIC-REGION-05` |
| 3.4 | Telemetry `ivr_tts_voice_selected_total{region}` + `ivr_tts_region_unresolved_total` | ✅ | `UT-TTS-TELEMETRY-04` |
| 3.5 | Cache tách đúng theo giọng | ✅ | `UT-SPEECH-VOICE-03` |
| 3.6 | [`ApprovedVietnameseSpeechRenderer`](../../src/Ivr.Infrastructure/Telephony/ApprovedVietnameseSpeechRenderer.cs) dùng chung `FallbackRegion` | ✅ | — |
| 3.7 | Config template trong `appsettings.json` (Api + Worker), mặc định `Enabled=false` | ✅ | — |

**Impact analysis trước khi sửa**: `SpeechSynthesisService` **LOW** (10/2/0 flow),
`StaticFileTtsProvider` **LOW** (1/1), `TtsUsageMeter` **LOW** (10/2),
`ApprovedVietnameseSpeechRenderer` **LOW** (0/0), `TtsProviderOptions` **MEDIUM** (14/8) —
mức MEDIUM duy nhất, chỉ thêm property lồng nhau, không đổi property cũ.

**Ba quyết định thiết kế đáng ghi**

1. **Miền được suy đúng MỘT lần**, trong `RegionalVoiceMap`. Mọi thứ phía sau — cache,
   static-file provider, telemetry — khóa theo `VoiceId`. Nghĩa là chỉ có **một** chỗ duy
   nhất có thể quyết định sai miền cho khách.
2. **`AudioCacheKey` không phải sửa một dòng nào.** Nó đã ghép sẵn `VoiceId`, nên 3 giọng tự
   động có 3 không gian cache riêng. `UT-SPEECH-VOICE-03` chứng minh chứ không giả định.
3. **`StaticFileTtsProvider` tra file theo `VoiceId`, không theo miền.** Provider không cần
   biết miền là gì; đây là thứ chặn việc file và giọng lệch nhau thành cuộc gọi đọc thông tin
   một miền bằng giọng miền khác. Giọng không có file ⇒ **ném lỗi**, không phát bừa file khác.

**Ngữ nghĩa `Enabled=false` — cần đọc kỹ**

Tắt cờ chỉ trả **giọng** về một giọng duy nhất. **Lexicon vẫn bám theo địa chỉ giao hàng**:
đơn Vĩnh Long vẫn đọc "ngàn". Đây là chủ ý — đọc "ngàn" cho khách miền Nam là đúng bất kể
có một hay ba giọng — nhưng nó **không phải** rollback về đúng hành vi trước W-0106. Ghi rõ
ở đây để không ai trông đợi nhầm.

**Kết quả kiểm chứng**

### Kết quả regression toàn bộ (`2026-08-22`, sau Giai đoạn 2 + 3)

| Suite | Kết quả |
| --- | --- |
| `Ivr.UnitTests` | **404 / 404** pass (từ 383 trước W-0106; +21 test mới) |
| `Ivr.IntegrationTests` | **180 / 180** pass |
| `Ivr.ContractTests` | **22 / 22** pass |
| `Ivr.ChaosTests` | **6 / 6** pass |
| **Tổng** | **612 test, 0 fail** |

- `dotnet build Ivr.sln`: **0 warning / 0 error** (`TreatWarningsAsErrors` +
  `EnforceCodeStyleInBuild` đang bật, nên 0 warning cũng là một gate thật).
- Traceability: **372 tagged test** (từ 343); `UT-TRACE-01` xanh.
- Không migration, không đổi OpenAPI, không đổi `TemplateHash`.

### Giai đoạn 4 — Lab assets + gọi thật qua MicroSIP 🟡 **ASSET XONG `2026-08-26`, CHỜ 6 LƯỢT GỌI**

Runbook đầy đủ: [`phase-4-lab-runbook.md`](../../docs/evidence/W-0106/phase-4-lab-runbook.md)

| # | Việc | Trạng thái |
| --- | --- | --- |
| 4.1 | **Render 3 MP3 từ ElevenLabs** | ✅ **XONG `2026-08-26`** — owner render và **đã nghe trong app**; voice ID thật ghi ở `manifest.txt` |
| 4.2 | [`Convert-LabVoiceAudio.ps1`](../../deploy/lab/Convert-LabVoiceAudio.ps1) — MP3 → PCM s16le/8 kHz/mono, loudnorm, tự verify định dạng, cập nhật `SHA256SUMS` + `manifest.txt` gồm voice ID/settings/date/account label | ✅ **đã chạy** — 3 PCM, checksum đã ghim, chạy hai lượt liên tiếp ra manifest y hệt |
| 4.3 | [`entrypoint.sh`](../../deploy/lab/asterisk/entrypoint.sh) cài **cả 3 file vùng miền song song**, boot-check toàn bộ checksum | ✅ đã sửa |
| 4.4 | [`docker-compose.softphone.yml`](../../docker-compose.softphone.yml) — block `RegionalVoices` | ✅ **`Enabled=true`** từ `2026-08-26`, duration thật `22/19/18` s (làm tròn LÊN từ `21,16/18,44/17,48`) |
| 4.5 | [`Invoke-FreeSoftphoneCall.ps1`](../../deploy/lab/Invoke-FreeSoftphoneCall.ps1) thêm `-Region North\|Central\|South` | ✅ đã sửa |
| 4.6 | Gọi 6 lượt MicroSIP (3 miền × phím `1`/`0`), xác nhận disposition **và nghe** | ⛔ **việc kế tiếp** — cần dựng lại image Asterisk trước |

**Ba điều phát hiện khi nhận file thật `2026-08-26`** — ghi lại vì cả ba đều là thứ tài liệu
trước đó khẳng định sai:

1. **Voice ID miền Nam khác shortlist.** Giọng thật là `f5q6kePPoQAjCPYG6moa`, nhãn vendor
   `Giang - Northern female Narrator`; shortlist ghi `X0V9HEDEuaVhVqzVPUKM`. Owner đã nghe và
   xác nhận **giọng đúng chất Nam** — nhãn vendor đặt sai. Đây là lần thứ hai catalog bên thứ ba
   sai, đúng như §5 của audition kit cảnh báo.
2. **Settings lệch nhau giữa ba giọng** (`0.75/0.50/0.50`, speed `1.00/1.00/1.09`), trong khi
   audition kit yêu cầu giữ y hệt. Đo được: Thắm dài hơn Giang **21%** trên cùng kịch bản.
   **Owner chọn giữ nguyên**; ràng buộc "settings phải giống nhau" đã gỡ khỏi kit.
3. **`Convert-LabVoiceAudio.ps1` ghi cứng settings sai vào manifest.** Nó bắt buộc `-VoiceId`
   thật nhưng lại tự bịa `stability=0.40 / speed=-3%` cho mọi lượt render — bốn dòng sai nằm ngay
   cạnh SHA-256 của chính file chúng mô tả. Đã đổi sang `-RenderSettings` per-region, fail-closed
   như voice ID. Cùng lượt sửa: script **không idempotent** — bộ lọc `^w0106_` bỏ sót dòng
   `work_id_regional=`, nên mỗi lần chạy lại thêm một header và một dòng trống.

**Ba quyết định khi dựng, đáng ghi:**

1. **Tên file là `-region-north|central|south`, KHÔNG phải `-n|-c|-s`.** Hậu tố `-c` đã thuộc
   về voice C của W-0104 (`ivr-lab-order-confirmation-c.wav`); dùng lại sẽ **đè lên evidence
   cũ**. Suýt dính khi đặt tên theo bản plan ban đầu.
2. **Ba giọng vùng miền cùng tồn tại, không chọn lúc boot.** Khác hẳn W-0104 vốn copy đúng
   một biến thể vào một tên file cố định. App chọn theo từng cuộc gọi, nên Asterisk phải sẵn
   sàng phát bất kỳ giọng nào bất kỳ lúc nào. Đường `IVR_LAB_VOICE_VARIANT` (A/B/C) giữ nguyên.
3. **`ffmpeg` chạy bitexact + `-map_metadata -1`.** Không có thì metadata encoder lọt vào WAV,
   cùng một MP3 nguồn ra hash khác nhau giữa hai phiên bản ffmpeg, và ghim checksum thành vô nghĩa.

**Điều phải nghe được ở bước 5** — chỉ kiểm disposition là bỏ sót đúng thứ W-0106 làm ra:
ba lượt phải nghe **ba giọng khác nhau**; lượt Bắc đọc **"nghìn"**, Trung/Nam đọc **"ngàn"**;
tiền và số lượng đọc bằng **chữ** (lỗi F2 đã sửa ở Giai đoạn 2).

### Giai đoạn 5 — Admin UI + tài liệu ✅ **ĐÃ XONG** (`2026-08-22`)

| # | Việc | Trạng thái |
| --- | --- | --- |
| 5.1 | Trường `voice_region` trong OpenAPI `draft.11 → draft.12` (**0 operation mới, 1 field response**) | ✅ drift baseline đã accept |
| 5.2 | `CallJobDetailApiResult.VoiceRegion` + `AdminReadService.ReadVoiceRegion` | ✅ |
| 5.3 | Màn chi tiết cuộc gọi hiện "Giọng đọc theo miền" + 5 chuỗi i18n | ✅ |
| 5.4 | [`docs/evidence/W-0106/README.md`](../../docs/evidence/W-0106/README.md) | ✅ |
| 5.5 | Tracker §2 ledger (`NEXT_WORK_ID` → `W-0107`) + row §5 | ✅ |
| 5.6 | `OD-VOICE-01..05` vào [`open-decisions-register.md`](../../specs/_review/open-decisions-register.md) | ✅ |
| 5.7 | Test: `IT-ADMIN-READ-10` + assertion trong `E2E-UI-DETAIL-02` | ✅ |

**Quyết định thiết kế: chỉ lộ MIỀN, không lộ địa chỉ**

`CallJobDetailApiResult` trước nay **không** có `delivery_area_short` — đó là chủ ý privacy,
địa chỉ thuộc speech whitelist chứ không thuộc dữ liệu console. Nên console nhận **một enum
ba giá trị** thay vì chuỗi phường-tỉnh. Operator biết khách nghe giọng miền nào mà không cần
thấy khách ở đâu; lộ chuỗi địa chỉ ra màn admin sẽ là mở rộng privacy cần review riêng
(`OD-V1-15`). `IT-ADMIN-READ-10` assert payload **không chứa** `Phú Khương`, `Vĩnh Long`,
hay `delivery_area_short`.

**Miền được suy ở read time, và điều đó có giới hạn phải nói rõ**

`voice_region` là hàm của dữ liệu đã lưu, **không phải bản ghi audit của giọng đã phát**.
Voice map và fallback nằm trong config, nên một lần đổi config giữa lúc gọi và lúc đọc sẽ
làm hai thứ lệch nhau. Muốn audit đúng giọng đã phát thì phải **persist** nó — W-0106 cố ý
không làm, vì đó là migration và là thay đổi contract. Giới hạn này được ghi thẳng vào
mô tả OpenAPI, XML doc của C#, và JSDoc của TypeScript — ba chỗ người đọc sẽ gặp.

**Không nhân đôi bảng 34 tỉnh sang TypeScript.** Admin UI chỉ map enum ba giá trị sang nhãn
tiếng Việt. Nếu để client tự suy miền từ địa chỉ thì sẽ có hai bản bảng 63 tên, và bản lệch
sẽ là bản console hiển thị trong khi bản khách nghe vẫn đúng — chiều sai tệ nhất.

---

## 7. Quyết định cần owner chốt

| ID | Quyết định | Chủ | Hiện trạng | Bằng chứng đóng |
| --- | --- | --- | --- | --- |
| `OD-VOICE-01` | **Nguồn giọng production.** Đã đảo hướng hai lần, xem §7.1 | Product + Infra + Privacy/Legal | 🟡 `ELEVENLABS_STARTER_PROPOSED` — nối tiếp `OD-V1-19` | Gói đã mua + **xác nhận ToS về audio sinh trong kỳ trả phí** + DPA + cost model + fallback khi voice ID biến mất |
| `OD-VOICE-02` | **Phân miền theo tỉnh/thành.** Owner chốt: chia thuần theo 34 đơn vị cấp tỉnh, **không biệt lệ**; Tây Nguyên → Trung. Bảng §3.1–§3.3 là chuẩn duy nhất | Owner + Product | ✅ `CLOSED` `2026-08-22` | Bảng 34→3 miền §3 + test 34/34 tỉnh |
| `OD-VOICE-05` | **Chốt 3 giọng không qua bước nghe** — Thắm (Bắc), Zara (Trung), Giang (Nam). Cơ sở là mô tả văn bản, không phải nghe. Xem §7.2 | Owner | ✅ `CLOSED` `2026-08-22` | Voice ID đã verify trong app + (để đạt `ACCEPTED`) chữ ký sếp sau khi nghe |
| `OD-VOICE-04` | **Tự host / thu âm người thật thay vì thuê vendor.** Xem §4.8. Không model tiếng Việt open-source nào vừa chất lượng vừa sạch license (`viXTTS` = CPML non-commercial + Coqui đã đóng cửa; `F5-TTS` weights = CC-BY-NC). Đường sạch duy nhất là **dữ liệu giọng của chính mình**. Khuyến nghị: thu âm 3 voice actor + concatenative, model tự host chỉ đọc phần đuôi (tên phường/xã) | Owner + Product + Legal | 🆕 `OPEN` — mở `2026-08-22` theo câu hỏi của owner | Hợp đồng + license giọng của 3 voice actor; bộ clip đã thu; bằng chứng mối nối nghe mượt; model tự host (nếu dùng) có license Apache/MIT + train trên data của mình |
| `OD-VOICE-03` | **Một template.** Owner chốt: giữ đúng 1 script version `v3-test-approved`; biến thể `nghìn`/`ngàn` đặt trong `VietnameseNumberSpeller`, không đặt trong template | Product + Privacy/Legal | ✅ `CLOSED` `2026-08-22` | `TemplateHash` không đổi + test speller theo miền |

---

### 7.1 `OD-VOICE-01` — lịch sử đảo hướng, và vì sao

Ghi lại đầy đủ vì quyết định này đã đổi hai lần và lý do đổi mới là thứ quan trọng.

| Vòng | Kết luận | Lý do đổi |
| --- | --- | --- |
| 1 (`2026-08-22` sáng) | ElevenLabs → **loại** khỏi production | Tính theo **full-TTS mỗi cuộc gọi**: ~9M ký tự/tháng ở 1.000 cuộc/ngày ⇒ ~26–39 triệu đ/tháng. Đắt hơn vendor Việt 20–25× |
| 2 (`2026-08-22` chiều) | Vendor Việt (FPT.AI) → **loại** | Owner nghe `myan` và đánh giá **không đạt**. FPT.AI chỉ có **đúng 1** giọng nữ miền Trung ⇒ không có phương án thay thế cùng vendor. Free tier cũng hết lượt ngay vì giới hạn request/ngày |
| 3 (hiện tại) | **ElevenLabs Starter `$6`/tháng** | Phép tính vòng 1 **sai giả định**: script cố định nên phần cố định chỉ cần render **609 ký tự MỘT LẦN vĩnh viễn** (§4.6), không phải 9M ký tự/tháng. Ở mức đó, `$6` ≈ `150.000đ`/tháng — **rẻ hơn gói rẻ nhất FPT.AI (`500.000đ`) 3,3 lần** |

**Bài học ghi lại**: chi phí TTS phải tính theo **số câu nói duy nhất**, không theo **số cuộc
gọi**. Với IVR script cố định, hai con số này chênh nhau hai bậc độ lớn, và tính nhầm sẽ loại
đúng phương án tốt nhất.

**Dữ kiện quyết định**: phương án free tốt nhất (`vi-VN-HoaiMyNeural` + `vi-VN-NamMinhNeural`,
Azure/Edge neural, 500k ký tự/tháng) **đã được thử và owner từ chối cả hai** ở W-0104
([evidence §6](../../docs/evidence/W-0104/voice-modernization-proposal.md)). Không lặp lại.

**Vẫn để mở**: Viettel AI (nữ × 3 miền, `320.000đ`/1M ký tự) chưa được nghe thử. Nếu muốn
0đ định kỳ hoàn toàn thì §4.8.6 (thu âm người thật) vẫn là đích dài hạn — nhưng ~10–15tr một
lần tương đương **83 tháng** ElevenLabs Starter, nên chỉ đáng khi đã chạy ổn định.

### 7.2 `OD-VOICE-05` — chốt 3 giọng không qua bước nghe

Owner quyết định `2026-08-22`: **chốt luôn ba giọng, không nghe trước, đổi sau nếu cần.**

| Miền | Giọng | Voice ID (**chưa verify**) | Fallback 1 | Fallback 2 |
| --- | --- | --- | --- | --- |
| Bắc | **Thắm** | `0ggMuQ1r9f9jqBu50nJn` | Mai | Hien |
| Trung | **Zara** | `QocxxnxEa0x8mrL2d4VT` | Huyen | Duyen |
| Nam | **Giang** | `X0V9HEDEuaVhVqzVPUKM` | HTN | Thanh |

Chọn theo hai tiêu chí nhất quán: **giọng an toàn của vùng** (Đà Nẵng cho Trung không phải
Huế; Sài Gòn cho Nam không phải miền Tây) và **ấm/tự nhiên chứ không phải giọng phát thanh
viên** — `Hien` và `Thanh` bị đẩy xuống fallback dù "chuyên nghiệp hơn", vì giọng bản tin
chính là cái "công nghiệp" owner đã bác ở FPT.AI.

**Cơ sở của quyết định là mô tả văn bản, không phải nghe.** Không ai trong chuỗi ra quyết
định này đã nghe ba giọng đó. Ghi rõ để sau này không ai đọc nhầm thành đã thẩm định.

**Hai hệ quả:**

1. `Voice ID` **vẫn phải verify trong ElevenLabs app** trước khi vào `manifest.txt`. Đây
   không phải việc thêm — muốn có audio thì phải mở app chọn giọng, lúc đó ID thật hiện ra.
   Catalog bên thứ ba đã sai một lần (gán `ueSxRO0nLF1bj93J2hVt` cho giọng nam miền Bắc,
   trong khi repo ghi ID đó là `Trung Caha`), nên tìm **theo tên**, không dán ID.
2. **W-0106 không đạt `ACCEPTED` nếu thiếu chữ ký sếp.** Tiền lệ W-0104 là owner nghe qua
   MicroSIP rồi mới ghi `ACCEPTED`. Trần trạng thái hiện tại là `TESTS_PASS`.

**"Đổi sau rẻ" đúng ở đâu và không đúng ở đâu:**

- ✅ Đổi **config**: sửa `Ivr:Speech:Tts:RegionalVoices`, không sửa code, không migration.
- ⚠️ Đổi **ở LAB**: file PCM ghim SHA-256 trong image Asterisk ⇒ render lại + hash lại +
  build lại image + **chụp lại evidence MicroSIP**. Đây đúng là chuyện đã xảy ra ở W-0104:
  A/B bị từ chối *sau khi* đã có evidence, phải làm lại từ đầu với voice C.

⇒ Khuyến nghị: cho sếp nghe **trước khi** chạy Giai đoạn 4, vì nghe tốn 3 lần render còn
làm lại evidence tốn cả buổi. Nhưng đây là khuyến nghị, không phải cổng chặn.

---

## 8. Rủi ro

| # | Rủi ro | Mức | Giảm thiểu |
| --- | --- | --- | --- |
| R0 | **Chỉ có 1 giọng nữ miền Trung ở mỗi vendor Việt** (`myan` ở FPT.AI). Sếp không ưng ⇒ không có phương án thay thế cùng vendor | **CAO** (vendor) → **THẤP** nếu chọn §4.8.6 | Giai đoạn 1 nghe **giọng Trung trước tiên** (task 1.2); so cả FPT.AI và Viettel; xác minh `ngoclam` có tồn tại không. **Thu âm người thật (§4.8.6) đóng hẳn rủi ro này** — tự chọn voice actor thì không bị giới hạn catalog |
| R1 | ~~Voice ID cộng đồng biến mất~~ — **đã giảm mạnh** sau khi chọn vendor Việt (giọng thuộc vendor, không phải cộng đồng) | **THẤP** | Validator fail-start khi thiếu giọng; `FallbackRegion`; ghim SHA-256 file lab |
| R2 | Sales gửi `delivery_area_short` sai/thiếu tỉnh | **TRUNG BÌNH** | Resolver trả `null` → fallback rõ ràng; metric `region_unresolved_total` để phát hiện sớm |
| R3 | Dữ liệu đơn cũ còn tên tỉnh trước sáp nhập | **TRUNG BÌNH** | Bảng alias 29 tên cũ (task 2.1) |
| R4 | Đọc sai số tiền khi nối TTS thật (F2) | **CAO** | `VietnameseNumberSpeller` là hạng mục bắt buộc, không tùy chọn |
| R5 | ~~Chi phí ElevenLabs vượt ngân sách~~ — **đã đóng** bằng quyết định chọn vendor Việt | **THẤP** | Bảng giá §4.2 + khối lượng §4.6; hybrid đưa 1.000 cuộc/ngày về gói `1.000.000đ`/tháng |
| R9 | VBee **không công bố số ký tự** theo gói — không so sánh được giá thực | **TRUNG BÌNH** | Hỏi sales VBee trước Giai đoạn 1.6; nếu không trả lời rõ ⇒ loại khỏi so sánh |
| R17 | **Free tier ElevenLabs không có commercial license.** Audio audition không được dùng cho cuộc gọi thật | **CAO** | Kit ghi rõ ở §8; task 1.7 bắt buộc sinh lại bản production sau khi mua Starter; DoD chặn |
| R18 | Mua Starter một tháng rồi hủy — **chưa rõ license còn hiệu lực với audio đã sinh hay không** | **CAO** | Phải đọc và trích dẫn ToS trước khi hủy. Nếu không xác nhận được ⇒ duy trì gói trả phí, `$6`/tháng vẫn rẻ hơn mọi phương án khác |
| R16 | **FPT.AI Console đã ngừng phục vụ khách hàng cá nhân từ `6/7/2026`** (banner trong console, ngày đó đã qua). Nếu IVR chạy trên tài khoản cá nhân thì mất dịch vụ | **TRUNG BÌNH** | Đang dùng **tài khoản doanh nghiệp** `Tập đoàn Ssavigroup Sâm trên cát` ⇒ hiện không ảnh hưởng. Nhưng đây là tín hiệu vendor thay đổi chính sách: `OD-VOICE-01` phải ghi rõ ràng buộc "chỉ chạy trên tài khoản doanh nghiệp" và có điều khoản thông báo trước khi ngừng dịch vụ |
| R10 | **Dùng nhầm model open-source non-commercial** (`viXTTS`, `F5-TTS` official weights) cho hệ thống thương mại | **CAO** | §4.8.1 liệt kê rõ; DoD bắt buộc ghi license của mọi model/dataset dùng; `viXTTS` **không thể** hợp thức hóa vì Coqui đã đóng cửa |
| R11 | Clone giọng không có quyền (output vendor, hoặc người thật không xin phép) | **CAO** | §4.8.2; chỉ clone từ voice actor **đã ký hợp đồng + license**; Nghị định 13/2023 coi giọng nói là dữ liệu cá nhân |
| R12 | Concatenative nghe **chói/gãy ở mối nối** giữa các clip | **TRUNG BÌNH** | Thu từ số ở **3 vị trí ngữ điệu** (đầu/giữa/cuối câu); chuẩn hóa loudness đồng nhất; nghiệm thu bằng tai trên MicroSIP chứ không bằng waveform |
| R13 | Tên phường/xã là **tập mở** (~3.321), người thật không đọc hết | **TRUNG BÌNH** | Model tự host clone từ chính voice actor đọc phần đuôi; pre-render dần theo đơn thật + cache vĩnh viễn; vùng giao hàng thực tế nhỏ hơn nhiều |
| R14 | **Số lượng thập phân** (`2,5 kg`) không ghép được từ clip thu sẵn | **TRUNG BÌNH** | Đã khoanh vùng ở Giai đoạn 2; cần quyết định trước buổi thu: (a) thu thêm `phẩy` + `rưỡi`, (b) làm tròn đơn vị bán, (c) giữ TTS cho riêng ca này |
| R15 | `AudioCacheKey` dùng `summaryHash`, **không** gồm hash của text đã render ⇒ sau khi đổi cách đọc số, audio cũ trong cache vẫn được phục vụ | **THẤP** | TTL ≤ 900s và bị chặn thêm bởi confirmation window ⇒ tự lành trong vòng 15 phút sau deploy; hiện chỉ MOCK/LAB. Ghi lại để không bị bất ngờ khi lên production |
| R6 | Sếp nghe thử rồi đổi ý về giọng | **THẤP** | Giọng nằm trong config, không hard-code; đổi giọng = đổi config + thay file, không sửa code |
| R7 | Người nghe không nhận ra giọng vùng mình → phản tác dụng | **THẤP** | Chọn giọng "an toàn vùng" (Đà Nẵng cho Trung, Sài Gòn cho Nam) thay vì giọng đặc sệt |
| R8 | ~~Tăng 3× bề mặt evidence lab~~ — **đã đóng** bằng `OD-VOICE-03` (1 template) | **THẤP** | `TemplateHash` không đổi ⇒ không migration, không approval lại (§5.5) |

---

## 9. Impact analysis (GitNexus, repo `ginsengfood-ivr`)

Chạy theo yêu cầu bắt buộc của `CLAUDE.md` trước khi sửa symbol.

| Symbol | Direction | Impacted | Direct | Risk | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| `SpeechSynthesisService` | upstream | 10 | 2 | **LOW** | Điểm sửa chính. Cảnh báo epistemic: lower-bound, có consumer bind qua interface/DI |
| `TtsProviderOptions` | upstream | 14 | 8 | **MEDIUM** | Thêm section config; 4 hit ở `Ivr.IntegrationTests` |
| `ShortDeliveryArea` (ctor) | upstream | 30 | 1 | **MEDIUM** | **Chỉ đọc**, không sửa |
| `PrivacySafeOrderSummary` | upstream | 95 | 20 | **HIGH** ⚠ | 2 execution flow (`TaskIntakeEndpoint.HandleAsync`, `TaskIntakeService.EvaluateAsync`) |

> ⚠️ **Cảnh báo HIGH risk**: `PrivacySafeOrderSummary` có 95 symbol phụ thuộc và nằm trên
> 2 luồng thực thi chính. Thiết kế §5.1 **cố ý tránh** sửa symbol này bằng cách suy ra miền
> như hàm thuần túy của dữ liệu đã có. Nếu trong lúc triển khai phát sinh nhu cầu thêm field
> vào `PrivacySafeOrderSummary`, **phải dừng lại và báo owner** — đó là thay đổi contract,
> không còn thuộc phạm vi W-0106.

### 9.1 `detect_changes` sau Giai đoạn 2 + 3 (`2026-08-22`)

Chạy `gitnexus_detect_changes(scope: "all")` theo `CLAUDE.md`. Kết quả tổng:
**790 symbol / 137 file / `risk_level: critical`**.

(console auth, `IvrDbContext`, migration, admin-ui). Lọc riêng `affected_processes`:

| | Kết quả |
| --- | --- |
| Execution flow bị ảnh hưởng bởi W-0106 | **0** |
| Từ khóa `speech`/`tts`/`voice`/`synthes` trong `affected_processes` | **0 hit** |

Kết luận: W-0106 **không chạm execution flow nào**. Điều này khớp với impact analysis từng

> Nếu commit chung hai work item, mức `critical` sẽ che mất việc W-0106 vốn rất hẹp.
> **Nên commit tách riêng.**

---

## 10. Definition of Done

### Đã xong

- [x] `OD-VOICE-02`, `OD-VOICE-03`, `OD-VOICE-05` đã chốt (`2026-08-22`).
- [x] 3 giọng đã chốt: **Thắm** / **Zara** / **Giang** (§7.2).
- [x] 34/34 tỉnh mới + 29/29 tên cũ có test phân miền, xanh (`UT-VOICE-REGION-01..03`).
- [x] `VietnameseNumberSpeller` phủ nghìn/triệu/tỷ + `linh/lẻ`, `mươi/mười`, `lăm`, `tư`, chặn số lẻ và vượt ngưỡng (`UT-VOICE-NUM-01..09`).
- [x] Cấu hình 3 giọng trùng nhau ⇒ **fail-start** (`UT-VOICE-CFG-01`).
- [x] Giọng không có file media ⇒ **ném lỗi**, không phát nhầm file miền khác (`UT-TTS-STATIC-REGION-05`).
- [x] 3 miền ⇒ 3 entry cache riêng, không đụng nhau (`UT-SPEECH-VOICE-03`).
- [x] `voice_region` ra console **không kèm địa chỉ** (`IT-ADMIN-READ-10`).
- [x] `gitnexus_detect_changes()` đã chạy — **0 execution flow** thuộc W-0106 (§9.1).
- [x] PII scan `PASS` (297 file) · `dotnet format --verify-no-changes` `PASS` · OpenAPI drift baseline accepted · codegen `OPENAPI_CODEGEN_COMPLETE=YES`.
- [x] Không API key nào vào git. Script audition đọc key từ `$env:FPT_AI_API_KEY`; MP3 audition nằm trong `.gitignore`.
- [x] `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ nguyên.

### Còn lại

- [ ] `OD-VOICE-01` chốt nguồn giọng production kèm cost model và xác nhận ToS (§7.1).
- [ ] 3 voice ID **xác minh trực tiếp trong ElevenLabs app** — catalog bên thứ ba đã sai một lần.
- [ ] 3 file MP3 → PCM ghim SHA-256, image boot-check đủ 6 checksum.
- [ ] 6 lượt gọi MicroSIP (3 miền × phím `1`/`0`) ra đúng disposition.
- [ ] ⏸️ Sếp nghe và ký nhận — **hoãn theo `OD-VOICE-05`**. Chưa có mục này thì trần trạng thái W-0106 là `TESTS_PASS`, không phải `ACCEPTED`.
- [ ] **Gitleaks** — binary không có trên máy này, phải chạy ở CI. Đã bù bằng PII scan `PASS` và rà tay: không secret nào trong diff W-0106.

---

## 11. Ranh giới

W-0106 là **software lab evidence**, giống W-0104. Nó **không** chứng minh: PSTN, SIM,
carrier, caller ID, 32 eSIM, Sales API thật, hay quyền gọi khách hàng thật.

Việc sếp duyệt 3 giọng là **content acceptance**, không phải production readiness. Trước khi
gọi khách thật vẫn phải đóng: license/quyền dùng giọng thương mại, plan/quota/API của vendor,
privacy/DPA, retention/data residency, và fallback khi voice ID biến mất.
