# W-0108 — Bộ hướng dẫn render 12 file đoạn cố định

Ngày: `2026-08-26` · Baseline: `main@55ea48b`
Dành cho: **owner** (mục 6.1 của [`README.md`](README.md) §6 — việc duy nhất chỉ owner làm được)
Nối tiếp: [`voice-audition-kit.md`](../W-0106/voice-audition-kit.md) — bộ kia chọn **giọng**, bộ này lấy **file**

> Dữ liệu trong bộ này là **fake toàn bộ**. `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi.
> Bộ này không đóng nửa **production** của `OD-VOICE-01` và không cấp quyền gọi khách thật.
>
> **Cập nhật `2026-08-27`:** owner đã quyết dùng **free tier** cho lab, mua gói để dành tới lúc
> lên production. §2 dưới đây đã viết lại theo quyết định đó — **không còn việc chặn nào** trước
> khi mở ElevenLabs.

---

## 1. Trong 30 giây

Một cuộc gọi được lắp từ **7 mảnh**: 4 mảnh văn xuôi cố định (thu sẵn) xen 3 mảnh giá trị đơn
(TTS sinh lúc gọi). Anh cần cung cấp **4 mảnh cố định × 3 giọng = 12 file**.

| Mảnh | Loại | Nội dung |
| --- | --- | --- |
| 1 | 🎙️ **cố định** | `Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Quý khách có đơn hàng gồm ` |
| 2 | 🤖 TTS | *danh sách món* |
| 3 | 🎙️ **cố định** | `, tổng tiền ` |
| 4 | 🤖 TTS | *tổng tiền bằng chữ + "đồng"* |
| 5 | 🎙️ **cố định** | `, giao đến ` |
| 6 | 🤖 TTS | *vùng giao* |
| 7 | 🎙️ **cố định** | `. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.` |

**203 / 266 ký tự là cố định.** Thu một lần, dùng mãi, chi phí runtime bằng 0 — và **nội dung đơn
của khách không rời mạng nội bộ** ở 4 mảnh này.

Lấy lại danh sách này bất cứ lúc nào, không cần mở tài liệu:

```bash
pwsh ./deploy/lab/Convert-LabSegmentAudio.ps1 -ListOnly
```

---

## 2. Không còn việc chặn — nhưng có một ranh giới phải giữ

### 2.1 · Free tier dùng được cho lượt render này (`OD-VOICE-01`, quyết `2026-08-27`)

Owner đã quyết: đang dev/test thì dùng free tier, chuyện mua API để dành tới lúc lên production.
Quyết định này **hợp lệ** — và lý do nó hợp lệ chính là ranh giới phải giữ:

| | |
| --- | --- |
| Free tier có commercial license không | **Không** |
| Vậy sao lab vẫn dùng được | Vì lab chạy **dữ liệu fake** và `REAL_CUSTOMER_CALL_ALLOWED=NO` — **không khách nào nghe**, nên không có hành vi thương mại nào để mà cần license |
| Cái gì giữ ranh giới đó | Dòng `w0106_production_provider_authorized=NO` trong `manifest.txt`, chứ không phải trí nhớ của ai |
| Lượt render này tốn bao nhiêu | Phần cố định là **203 ký tự/miền × 3 = 609 ký tự**. Phương án B (render rời từng câu) tốn đúng 609; phương án A (render cả câu liền rồi cắt) tốn 266 × 3 = **798** |

> Con số cũ trong bộ này ghi *"12 × ~110 ≈ 1.300 ký tự"* — **sai**, vì nó nhân đều 12 lần render
> với nhau trong khi bốn câu dài rất lệch: `107` / `12` / `11` / `73` ký tự. Số đúng đo từ
> `deploy/lab/speech-segments.json` (`fixedCharacters: 203`) là **609**.

**Hệ quả với production:** 12 file sinh ra ở đây là **tài sản lab**. Khi mua gói, chúng phải được
**render lại** rồi ghim checksum mới — không được chép sang.

### 2.2 · Vì sao nên render **ngay bây giờ**, không đợi mua gói

Thắm/Zara/Giang là **community voice**. Chủ giọng có quyền gỡ khỏi Voice Library bất cứ lúc nào
và ElevenLabs không cam kết giữ hộ. Nếu một giọng biến mất trước lúc mua gói thì thứ mất **không
phải** ba file đã render — chúng nằm trong repo, ghim bằng SHA-256 — mà là **khả năng render
thêm**. Tức là buổi nghe và ký vừa xong `2026-08-26` phải làm lại từ đầu với một giọng khác.

Render 12 đoạn bây giờ ghim vĩnh viễn **203/266 ký tự mỗi kịch bản** bằng hash nội dung. Phần còn
lại (tên, tiền, số lượng) vẫn cần endpoint TTS sống, và phần đó dù sao cũng phải chờ gói trả phí.

### 2.3 · Hai việc chuyển sang **điều kiện production**, không chặn lượt này

Vẫn phải làm, chỉ là làm trước khi lên production chứ không phải trước khi mở ElevenLabs:

| # | Việc | Vì sao |
| --- | --- | --- |
| `R18` | Đọc và **trích dẫn** ToS: *huỷ gói sau một tháng thì license thương mại của audio đã sinh trong kỳ trả phí còn hiệu lực không?* | Câu này phải có câu trả lời **bằng văn bản trích từ ToS**, không phải bằng suy đoán. Không rõ ⇒ **duy trì gói trả phí**; `$6`/tháng vẫn rẻ hơn mọi phương án khác |
| `R16` | Dùng **tài khoản doanh nghiệp**, không phải cá nhân, và ghi lại nhãn tài khoản | FPT.AI đã ngừng phục vụ khách hàng cá nhân từ `6/7/2026` — tín hiệu vendor đổi chính sách, và ElevenLabs không miễn nhiễm |

Nhãn tài khoản thì **ghi ngay từ lượt này** (§7 mục 5), kể cả khi đang là free tier — để sau này
truy được file nào sinh từ tài khoản nào.

---

### 2.4 · Nghe ba giọng ở đâu — **không có file mẫu nào**

`OD-VOICE-05` chốt ba giọng **không qua bước nghe**, dựa trên mô tả văn bản trong catalog bên thứ
ba. Nên không có file nào trong repo phát ra giọng Thắm, Zara hay Giang. **Muốn nghe thì phải
render** — và đó chính là lý do §2.1 (mua gói) phải làm **trước**, không phải sau.

Cách nghe, trong ElevenLabs web app:

| Bước | Việc |
| --- | --- |
| 1 | **Voice Library** → lọc `Language = Vietnamese`, `Gender = Female` |
| 2 | Tìm **theo tên**: `Thắm` · `Zara` · `Giang`. **Đừng tra bằng Voice ID ở §3** — ID đó chưa verify |
| 3 | `Add to My Voices` cho cả ba |
| 4 | **Text to Speech** → chọn giọng → dán **câu liền của miền tương ứng** (§4-A bước 1) → đặt settings §3 → Generate |
| 5 | Nghe. Chấm theo 5 tiêu chí ở [`voice-audition-kit.md` §6](../W-0106/voice-audition-kit.md) |
| 6 | Ưng ⇒ **giữ luôn file đó**, nó là nguồn để cắt ở §4-A. Không ưng ⇒ fallback 1 rồi fallback 2 ở §3 |

Bước 5 và bước render chính **là một**: file anh nghe và duyệt cũng là file anh cắt ra 4 mảnh.
Không cần render hai lần.

**Mốc so sánh có sẵn để hiệu chỉnh tai:** `deploy/lab/asterisk/audio/ivr-lab-order-confirmation-c.wav`
— giọng ElevenLabs `Trung Caha` mà anh **đã duyệt** ở W-0104, đã hạ về PCM 8 kHz mono đúng như
khách nghe qua điện thoại. Nghe file này trước để biết chất lượng ở đầu dây thật nghe ra sao;
studio 44 kHz trong app luôn hay hơn thứ khách thực sự nghe.

---

## 3. Cấu hình render — giống hệt nhau ở cả ba giọng

Khác cấu hình giữa các miền là ba giọng nghe lệch nhau vì lý do không ai truy được.

| Tham số | Giá trị |
| --- | --- |
| Model | **Eleven v3** |
| Language | **Auto detect** |
| `Stability` | **0.40** |
| `Similarity` | **0.75** |
| `Style` | **thấp** |
| Speed | **-3%** |

Không nhạc nền. Không hiệu ứng.

**Ba giọng đã chốt** (`OD-VOICE-05`, kèm fallback ở
[`voice-audition-kit.md` §5](../W-0106/voice-audition-kit.md)):

| Miền | Giọng | Voice ID trong tài liệu |
| --- | --- | --- |
| Bắc | **Thắm** | `0ggMuQ1r9f9jqBu50nJn` |
| Trung | **Zara** | `QocxxnxEa0x8mrL2d4VT` |
| Nam | **Giang** | `X0V9HEDEuaVhVqzVPUKM` |

> ⚠️ **Copy Voice ID thật từ app, đừng lấy từ bảng này.** Bảng lấy từ catalog bên thứ ba, và
> catalog đó **đã sai ít nhất một lần**: nó gán `ueSxRO0nLF1bj93J2hVt` cho một giọng nam miền Bắc
> tên khác, trong khi `manifest.txt` của chính repo ghi ID đó là `Trung Caha`.

---

## 4. Cách render — chọn một trong hai

### 🟢 Phương án A — render câu liền rồi cắt **(khuyến nghị)**

**Vì sao khuyến nghị.** Mảnh 3 (`, tổng tiền `) và mảnh 5 (`, giao đến `) là **mảnh giữa câu**.
Render rời, TTS sẽ đọc chúng như hai câu độc lập, ngữ điệu rơi ở cuối — ghép lại nghe như đọc
danh sách chứ không như một câu. Mảnh 1 cũng vậy: nó phải kết thúc bằng ngữ điệu **còn tiếp**,
không phải ngữ điệu hết câu.

Cắt từ một lần đọc liền thì mọi mối nối **thật sự** đến từ một hơi đọc duy nhất. Không có kỹ
thuật nào tái tạo được điều đó. Đây là cách xử lý rủi ro `R12` ("concatenative nghe chói/gãy ở
mối nối") của kế hoạch W-0106.

**Bước 1 — render 3 câu liền, mỗi giọng một câu.** Ba bản này đã có sẵn nguyên văn ở
[`voice-audition-kit.md` §4](../W-0106/voice-audition-kit.md#4-kịch-bản--dán-đúng-bản-của-từng-miền);
chúng chính là template đã duyệt với 3 chỗ trống điền sẵn, nên cắt ra là khớp từng chữ.

**Bước 2 — cắt 4 mảnh.** Mốc cắt nằm đúng ở ranh giới giữa văn xuôi và giá trị đơn:

| Mảnh | Cắt từ | Cắt đến | Miền Bắc mốc nghe là | Trung / Nam mốc nghe là |
| --- | --- | --- | --- | --- |
| **s1** | đầu file | ngay trước `hai hộp` | …có đơn hàng **gồm** \| hai hộp… | như Bắc |
| **s3** | ngay sau `hạt sen` | ngay trước `năm trăm` | …hạt **sen** \| , tổng tiền \| **năm** trăm… | như Bắc |
| **s5** | ngay sau `đồng` | ngay trước `phường` | …**đồng** \| , giao đến \| **phường**… | như Bắc |
| **s7** | ngay sau `Hà Nội` | hết file | …Hà **Nội** \| . Bấm phím một… | Trung: …Đà **Nẵng** \| … · Nam: …Vĩnh **Long** \| … |

Ba chỗ khác nhau giữa ba miền chỉ là: **`nghìn`** (Bắc) vs **`ngàn`** (Trung, Nam), và tên địa
danh. Cả ba đều nằm trong mảnh **động**, không nằm trong mảnh cố định — nên 4 mảnh cố định của ba
miền **giống nhau từng chữ**, chỉ khác giọng.

**Bước 3 — cắt ở chỗ im lặng, không cắt giữa âm.** Cắt tại điểm biên độ gần 0 giữa hai từ. Thừa
một chút im lặng ở đầu/cuối mảnh **tốt hơn** thiếu một phần âm: chuỗi ghép sẽ nghe như có nhịp
nghỉ tự nhiên, còn cắt cụt âm thì nghe như nuốt chữ.

Công cụ nào cũng được (Audacity miễn phí). Xuất **MP3**, script sẽ tự chuyển sang PCM.

### 🟡 Phương án B — render rời 4 đoạn

Nhanh hơn, nhưng chấp nhận rủi ro ngữ điệu ở §4-A. Dán **đúng** 4 chuỗi dưới, không thêm bớt một
ký tự — kể cả dấu phẩy đầu dòng và khoảng trắng cuối:

```text
Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Quý khách có đơn hàng gồm 
```

```text
, tổng tiền 
```

```text
, giao đến 
```

```text
. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.
```

> **Nếu chọn B thì bắt buộc nghe thử ghép ở bước cuối trước khi kết luận.** Nghiệm thu bằng tai
> trên MicroSIP, không bằng waveform.

---

## 5. Đặt tên file — sai tên là script không tìm ra

Đúng **12 file MP3**, đặt trong **một thư mục**, theo mẫu `<miền>-s<số>.mp3`:

```text
north-s1.mp3    central-s1.mp3    south-s1.mp3
north-s3.mp3    central-s3.mp3    south-s3.mp3
north-s5.mp3    central-s5.mp3    south-s5.mp3
north-s7.mp3    central-s7.mp3    south-s7.mp3
```

Số là **1, 3, 5, 7** — đó là vị trí thật trong template (2, 4, 6 là mảnh TTS). Không đổi thành
1-2-3-4.

**Để MP3 nguồn NGOÀI repo** theo tiền lệ W-0104. Repo chỉ chứa PCM 8 kHz đã ghim checksum.
Ví dụ `C:\ivr-audio\w0108-segments\`.

---

## 6. Bàn giao — một lệnh

```bash
pwsh ./deploy/lab/Convert-LabSegmentAudio.ps1 -SourceDirectory C:\ivr-audio\w0108-segments
```

Script tự làm hết phần còn lại: chuyển PCM s16le/8 kHz/mono + loudnorm, **tự kiểm định dạng**, đo
độ dài thật, ghim SHA-256, và sinh **hai** khối cấu hình dán thẳng được —
`segments-compose-env.yml` (cho lab) và `segments-appsettings.json` (cho deployment có appsettings).

**Không chép tay mã băm.** Một ký tự sai trong 64 ký tự là một câu không tra ra được, và nó chỉ
lộ ra lúc đang gọi khách.

**Đã kiểm chứng ngày 26/08** bằng audio giả: 12 file vào → 12 PCM ra, `LASTEXITCODE=0`, khối env
merge sạch vào compose (`docker compose config` exit 0, 36 khoá tới cả `ivr-api` lẫn `ivr-worker`).
Hai lỗi tìm thấy trong lượt kiểm đó đã được sửa — xem [`README.md` §9](README.md#9-kiểm-chứng-khô-chuỗi-bàn-giao-2026-08-26).

---

## 7. Ghi lại 5 thứ — thiếu một mục thì bước sau không chạy

| # | Mục | Giá trị |
| --- | --- | --- |
| 1 | Tên giọng đúng như hiển thị trong ElevenLabs | Bắc: ______ · Trung: ______ · Nam: ______ |
| 2 | **Voice ID thật**, copy từ app | Bắc: ______ · Trung: ______ · Nam: ______ |
| 3 | Model + settings đã dùng | v3 · stability ____ · similarity ____ · style ____ · speed ____ |
| 4 | SHA-256 của từng MP3 nguồn | script tự ghi vào `segments-manifest.txt` |
| 5 | Ngày render + nhãn tài khoản | ______ · ______ |
| 6 | **Trích dẫn ToS** về audio sinh trong kỳ trả phí (§2.3 `R18`) | *bỏ trống được ở lượt free-tier này — bắt buộc trước production* |
| 7 | Phương án đã dùng: A (cắt) hay B (rời) | ______ |

---

## 8. Ranh giới — bộ này **không** làm gì

- **Không** đóng nửa **production** của `OD-VOICE-01`. Nửa **lab** đã `APPROVED` `2026-08-27`
  (free tier). Nửa còn lại vẫn mở: gói trả phí, trích dẫn ToS, DPA, data residency, cost model và
  phương án lui khi voice ID biến mất khỏi Voice Library.
- `OD-VOICE-05` **đã đóng** `2026-08-26` — owner nghe cả ba giọng qua MicroSIP ở 8 kHz và chấp
  nhận; W-0106 đã lên `ACCEPTED` (phạm vi lab, dữ liệu fake). Điều đó **không** miễn bước 6.4:
  nghe ba giọng ≠ nghe **mối nối** giữa đoạn thu sẵn và đoạn tổng hợp, và mối nối mới là thứ bộ
  này sinh ra.
- **Không** mở quyền gọi khách thật. `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- **Không** thay nửa còn lại. 3 mảnh động vẫn cần endpoint TTS thật
  (`Ivr__Speech__Tts__External__*`). Bật `Segmentation.Enabled=true` khi catalog thiếu một câu ⇒
  service **từ chối khởi động** — đó là hành vi đúng, không phải lỗi.
