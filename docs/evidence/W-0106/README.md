# W-0106 — Định tuyến giọng đọc theo vùng miền (3 giọng nữ Bắc/Trung/Nam)

Ngày: `2026-08-22`
Baseline: `main@f7c9be9`
Trạng thái: `TESTS_PASS` — Giai đoạn 2, 3, 5 xong; Giai đoạn 4 chờ 3 file MP3; Giai đoạn 1 bỏ bước nghe theo `OD-VOICE-05`. Rà soát as-built `2026-08-22`, xem §8.

> Trạng thái này **không phải** owner UAT, không phải production readiness, và **không phải
> `ACCEPTED`**. Tiền lệ W-0104 là owner nghe qua MicroSIP rồi mới ký; owner đã hoãn bước đó
> (`OD-VOICE-05`), nên trần trạng thái W-0106 là `TESTS_PASS`.
>
> `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi.

Plan: [`W-0106-regional-voice-routing-plan.md`](../../../plan/ivr-orther/W-0106-regional-voice-routing-plan.md)

---

## 1. Phạm vi đã triển khai

**Domain (Giai đoạn 2)**

- `VietnamRegion`, `DeliveryRegionResolver` — bảng **34 đơn vị cấp tỉnh** theo Nghị quyết
  `202/2025/QH15` + **29 tên tỉnh cũ** đã bị xóa.
- `VietnameseNumberSpeller` + `VietnameseNumberStyle` — số → chữ, biến thể `nghìn`/`ngàn`
  và `linh`/`lẻ` theo miền.
- `VietnameseTextNormalizer` — chuẩn hóa khớp tên địa danh.
- `VietnameseOrderScriptRenderer` — suy miền tại chỗ, đọc tiền và số lượng bằng chữ.

**Infrastructure (Giai đoạn 3)**

- `RegionalVoiceMap` + `RegionalVoiceOptions` + validator fail-start.
- `SpeechSynthesisService` chọn giọng theo `delivery_area_short` thay vì hằng số global.
- `StaticFileTtsProvider` tra file LAB theo `VoiceId`.
- Telemetry `ivr_tts_voice_selected_total{region}`, `ivr_tts_region_unresolved_total`.

**Lab (Giai đoạn 4 — chuỗi xử lý)**

- `Convert-LabVoiceAudio.ps1`, `entrypoint.sh` cài 3 giọng song song, compose block
  `RegionalVoices`, `Invoke-FreeSoftphoneCall.ps1 -Region`.

**Console (Giai đoạn 5)**

- Trường `voice_region` trong `IvrCallJobDetail`, suy tại read time, **không lộ địa chỉ**.
- Màn chi tiết cuộc gọi hiện "Giọng đọc theo miền".

---

## 2. Automated evidence

| Gate | Kết quả |
| --- | --- |
| Unit suite | `419/419 PASS` |
| Integration suite | `225/226` — cái đỏ duy nhất là `IT-ACCOUNT-LOCK-03` của **W-0105**, xem §5 |
| `AdminReadApiTests` (gồm `IT-ADMIN-READ-10` mới) | `10/10 PASS` |
| Contract suite | `22/22 PASS` |
| Chaos suite | `6/6 PASS` |
| Admin UI vitest | `199/200` — cái đỏ duy nhất là `E2E-UI-REVIEW-05` của **W-0107**, xem §8.1 |
| Admin UI `tsc --noEmit` | ⚠️ đỏ ở `integration/page.tsx` — **W-0107** thiếu import, xem §8.1 |
| Admin UI `eslint --max-warnings 0` | ⚠️ đỏ ở `integration/page.tsx` — **W-0107**, xem §8.1 |
| `dotnet build Ivr.sln` | `0 warning / 0 error` |
| OpenAPI codegen (`regenerate-openapi.ps1`) | `OPENAPI_CODEGEN_COMPLETE=YES` |
| Traceability | `395` test ID, bảng đã đồng bộ |
| `docker compose config` (dev + softphone) | `PASS` |
| OpenAPI drift | `draft.11 → draft.12`, baseline đã accept |
| PowerShell parse (2 script mới) | `PASS` |
| PII scan | `PASS` — 297 file |
| `dotnet format --verify-no-changes` | `PASS` |
| Gitleaks | ⚠️ binary không có trên máy này — **phải chạy ở CI** |

**Test mới của W-0106** — 21 unit + 1 integration:

| Nhóm | Test ID | Nội dung |
| --- | --- | --- |
| Phân miền | `UT-VOICE-REGION-01..10` | 34 tỉnh mới, 29 tên cũ, không dấu, tiền tố đơn vị, ca bẫy tên xã trùng tên tỉnh, `Thuế` không khớp `Huế` |
| Đọc số | `UT-VOICE-NUM-01..09` | `mười lăm`, `hai mươi mốt`, `hai mươi tư`, `không trăm linh năm`, `nghìn`/`ngàn`, chặn số lẻ và vượt ngưỡng |
| Cấu hình giọng | `UT-VOICE-CFG-01..04` | 3 giọng trùng nhau ⇒ **fail-start**; media reference phải khác nhau và an toàn |
| Định tuyến | `UT-SPEECH-VOICE-01..03` | Địa chỉ → giọng; fallback có cờ `unresolved`; 3 miền ⇒ **3 entry cache riêng** |
| Telemetry | `UT-TTS-TELEMETRY-04` | Đếm `unresolved` tách khỏi lưu lượng bình thường |
| Static file | `UT-TTS-STATIC-REGION-05` | File theo giọng; giọng không có file ⇒ **ném lỗi**, không phát nhầm |
| Renderer | `UT-SCRIPT-VI-REGION-09` | 3 miền, 3 cách đọc, **cùng một `TemplateHash`** |
| Console | `IT-ADMIN-READ-10` | `voice_region` đúng theo miền; payload **không chứa** `delivery_area_short` |

---

## 3. Impact analysis (GitNexus)

| Symbol | Impacted | Direct | Risk | Flow |
| --- | ---: | ---: | --- | ---: |
| `VietnameseOrderScriptRenderer` | 4 | 4 | LOW | 0 |
| `SpeechSynthesisService` | 10 | 2 | LOW | 0 |
| `StaticFileTtsProvider` | 1 | 1 | LOW | 0 |
| `TtsUsageMeter` | 10 | 2 | LOW | 0 |
| `ApprovedVietnameseSpeechRenderer` | 0 | 0 | LOW | 0 |
| `CallJobDetailApiResult` | 2 | 1 | LOW | 0 |
| `TtsProviderOptions` | 14 | 8 | **MEDIUM** | 0 |
| `PrivacySafeOrderSummary` | 95 | 20 | **HIGH** | 2 |

`PrivacySafeOrderSummary` **không bị đụng tới**. Thiết kế cố ý suy miền như hàm thuần túy
của `ShortDeliveryArea.Value`, nên không đổi contract intake, không migration, không cần
Sales bổ sung field. `TtsProviderOptions` là MEDIUM duy nhất và chỉ thêm property lồng nhau.

`detect_changes` báo `critical` cho toàn working tree, nhưng **0 execution flow** thuộc
W-0106 — toàn bộ flow bị ảnh hưởng đi qua `readSession`/`requireAdmin` của **W-0105** đang
chưa commit. Chi tiết §9.1 của plan.

---

## 4. Lỗi phát hiện và đã sửa

**F2 — renderer đọc chữ số trong khi audio đã duyệt đọc chữ.**
`VietnameseOrderScriptRenderer` sinh `"560.000 đồng"` và `"2 hộp"`, nhưng audio v3 owner
chấp nhận ở W-0104 lại nói `"năm trăm sáu mươi nghìn đồng"` và `"hai hộp"`. Bản audio đó
được gõ tay trên web ElevenLabs, nên **nhánh chữ số chưa từng có ai nghe**. Cách một engine
TTS đọc `"560.000"` là tùy engine — và đây là con số khách đang được hỏi để bấm phím xác nhận.

Bảy test cũ đang pin chính cái lỗi này, đã cập nhật: `ScriptContentTests` (×3),
`MockTelephonyTests` (×3), `TtsProviderTests` (×2).

**Va chạm tên file suýt xảy ra.** Plan ban đầu đặt tên file lab `-n|-c|-s`; hậu tố `-c` đã
thuộc về voice C của W-0104 và sẽ **đè lên evidence cũ**. Đổi sang `-region-north|central|south`.

---

## 5. Việc còn nợ

| # | Việc | Vì sao chưa xong |
| --- | --- | --- |
| 1 | **3 file MP3** từ ElevenLabs | Cần phiên đăng nhập ElevenLabs của owner |
| 2 | 6 lượt gọi MicroSIP | Phụ thuộc (1) |
| 3 | ✅ **Đã xong** — integration `225/226`, chaos `6/6`, `AdminReadApiTests` `10/10` | — |
| 4 | ✅ **Đã xong** — codegen C# regenerate được sau khi gỡ manifest lạc, xem §7 | — |
| 5 | Sếp nghe và ký nhận 3 giọng | Hoãn theo `OD-VOICE-05` |

### `IT-ACCOUNT-LOCK-03` đỏ — thuộc W-0105, không phải W-0106

`ConsoleAccountApiTests.FifthBadPasswordLocksTheAccountAndResponsesStayGeneric`:
`Expected: Unauthorized · Actual: TooManyRequests`.

`ConsoleSignInRateLimiter` mới thêm đặt `PerUsernameLimit = 5`, đúng bằng ngưỡng lockout.
Test gọi sai mật khẩu 5 lần (đếm 1..5, `5 > 5` sai ⇒ vẫn `401`), rồi gọi **lần thứ 6** để
kiểm lockout — lần đó đếm `6 > 5` nên limiter trả `429` **trước khi** chạm nhánh lockout, che
mất `401`.

Chính doc comment của limiter viết *"per-username limit is intentionally lower than the lockout
threshold"* — nhưng code để **bằng**, không thấp hơn. Đây là mâu thuẫn nội bộ của W-0105 và
việc chọn hướng sửa (hạ `PerUsernameLimit`, hay viết lại kỳ vọng của test) là quyết định
chính sách bảo mật của owner W-0105, không phải lỗi đánh máy. W-0106 không đụng vào.

---

## 6. Ranh giới

W-0106 là software lab evidence. Nó **không** chứng minh PSTN, SIM, carrier, caller ID,
32 eSIM, Sales API thật hay quyền gọi khách hàng.

`OD-VOICE-01` (nguồn giọng production) vẫn `OPEN` — đã đảo hướng ba lần, hiện đề xuất
ElevenLabs Starter `$6`/tháng; xem §7.1 của plan. Trước production vẫn phải đóng: commercial
license, xác nhận ToS với audio sinh trong kỳ trả phí, DPA/privacy, data residency, và
fallback khi voice ID biến mất khỏi Voice Library.

---

## 7. Sự cố hạ tầng đã sửa: manifest `dotnet tool` lạc

Codegen OpenAPI im lặng hỏng trên máy local. Nguyên nhân: tồn tại **hai** manifest.

| File | Trạng thái git | Nội dung |
| --- | --- | --- |
| `dotnet-tools.json` (root) | **tracked** — manifest thật của repo | `nswag.consolecore` + `dotnet-ef` |
| `.config/dotnet-tools.json` | **untracked**, tạo `2026-08-22 15:05` | chỉ `dotnet-ef` |

Manifest của repo nằm ở **root**, không phải vị trí chuẩn `.config/`. Khi ai đó chạy
`dotnet tool install dotnet-ef`, lệnh đó không thấy manifest ở root nên **tạo mới** một
manifest ở `.config/` — và `.config/` được ưu tiên, che mất manifest repo. Từ đó
`dotnet tool restore` báo thành công nhưng chỉ khôi phục `dotnet-ef`; `dotnet nswag` biến mất.

Đã gỡ file untracked đó (`dotnet-ef` cùng version `10.0.11` đã có sẵn trong manifest repo nên
không mất gì). Bản sao lưu nằm ngoài repo.

**Hệ quả rộng hơn W-0106**: file generated còn thiếu **toàn bộ contract console account của
W-0105** (`draft.10`) chứ không chỉ `voice_region` (`draft.12`). Lần regenerate này thêm
**497 dòng** — 20 type của W-0105 cộng `IvrCallJobDetailVoice_region`. Codegen đã hỏng cho cả
hai work item và không ai phát hiện.

**Khuyến nghị (chưa làm, ngoài phạm vi W-0106)**: chuyển `dotnet-tools.json` về
`.config/dotnet-tools.json` để sự cố này không lặp lại, và thêm một gate CI so file generated
với spec — hiện **không có gate nào** bắt được drift này, đó là lý do nó sống sót qua nhiều đợt.

---

## 8. Rà soát as-built (`2026-08-22`) — chênh lệch giữa plan và code

Đợt rà soát sau khi code xong tìm ra bốn chỗ tài liệu nói khác thực tế. Tất cả đã sửa; ghi lại
vì bản plan là thứ người sau đọc để hiểu vì sao hệ thống có hình dạng như vậy.

| # | Plan nói | Thực tế | Đã xử |
| --- | --- | --- | --- |
| 1 | §5.3 dùng `FileMediaReferenceByRegion` riêng và `-n/-c/-s` | Media nằm trong từng entry `RegionalVoices`; tên `-region-north\|central\|south` | Viết lại §5.3 thành bảng "phác thảo vs as-built" kèm lý do đổi |
| 2 | §5.3 mở rộng `Set-AsteriskLabVoice.ps1` để **chuyển** giữa 3 biến thể | Cả ba file **cùng tồn tại**, app chọn theo từng cuộc gọi | Viết lại; nêu rõ W-0104 chỉ cần một giọng cho cả lab nên cách cũ không dùng được |
| 3 | §5.2 thiếu `VietnameseTextNormalizer` và `ScriptRenderOptions.FallbackRegion` | Cả hai đều tồn tại và đều có lý do thiết kế | Bổ sung §5.2 (b2) và (e) |
| 4 | §5.2 ghi `Resolve(summary.DeliveryArea, configured)` trả chuỗi | Trả `RegionalVoiceSelection` có cờ `ResolvedFromDeliveryArea` | Sửa đoạn code trong §5.2 (d) |

### 8.1 Hòa hợp với W-0107 (hệ từ điển enum)

Trong lúc W-0106 chạy, một session khác dựng W-0107 — `EnumLabel` + `enums.vi.json`, đưa nhãn
enum ra khỏi `vi.json`. Họ **đã tự thêm** `voiceRegion` vào từ điển, nên bản dựng tay của
W-0106 trở thành trùng lặp.

Đã chuyển màn chi tiết sang `<EnumLabel family="voiceRegion" …>` và **xóa 3 key trùng** khỏi
`vi.json`. Giữ lại hai key vẫn đúng chỗ: `detail.voiceRegion` là **nhãn hàng** (interface
copy) và `detail.voiceRegionUnknown` là **fallback** khi không suy được miền — `EnumLabel`
trả `—` cho giá trị vắng, nhưng ở đây "không suy được từ khu vực giao hàng" là tín hiệu chất
lượng dữ liệu Sales, không phải một ô trống.

Trong lúc chuyển có phát hiện `page.tsx` dùng `EnumLabel` ở 16 chỗ mà **chưa có import** —
lỗi của W-0107 đang viết dở, không phải do W-0106. Đã thêm import (1 dòng) để gỡ kẹt
typecheck cho cả hai.

⚠️ **Ba thứ còn đỏ, đều thuộc W-0107, W-0106 không đụng tới:**

| # | Triệu chứng | Nguyên nhân |
| --- | --- | --- |
| 1 | `tsc` đỏ ở `integration/page.tsx` | thiếu import `tEnum` |
| 2 | `eslint` đỏ ở `integration/page.tsx` (6 chỗ) | thiếu import `EnumLabel` |
| 3 | `E2E-UI-REVIEW-05` đỏ | `config/page.tsx` đã chuyển sang `EnumLabelList family="approvalType"`, nhưng test vẫn assert chuỗi nối cũ `MOCK_TEST, LAB, CONTENT, PRIVACY_LEGAL` |

Hai file test phủ trực tiếp thay đổi của W-0106 — `console-screens.test.ts` và
`back-office.test.tsx` — chạy riêng vẫn **21/21 PASS**.

### 8.2 Đính chính doc W-0104

`docs/evidence/W-0104/voice-modernization-proposal.md` §7 viết *"Code renderer tạo số
lượng/tổng tiền từ dữ liệu có cấu trúc"* — câu này **không đúng tại thời điểm viết**, vì
renderer sinh dạng chữ số còn bản audio được duyệt lại đọc dạng chữ. Đã thêm §8 vào doc đó để
đính chính và ghi lại quan hệ kế thừa W-0104 → W-0106.
