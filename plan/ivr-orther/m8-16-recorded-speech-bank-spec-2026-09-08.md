# M8-16 — Đặc tả ngân hàng ghi âm, suy ra từ code

**Ngày lập:** `2026-09-08` · **Work ID:** `W-0228`
**Nguồn quyết định:** Owner, `2026-09-08` — *"`items_spoken` thu trước được, catalog GinsengFood
chỉ vài chục món"* · nối tiếp `OD-V1-19` (`2026-09-05`)

**Trạng thái:** `SPEC_ONLY / NOT_APPROVED / NO_RECORDING_PERFORMED`
**Quyền gọi khách thật:** `REAL_CUSTOMER_CALL_ALLOWED=NO`

> Đây **không** phải phê duyệt bỏ VieNeu. Đây là đếm chính xác cái giá của hướng đã ký, để quyết
> định được ra trên số thật. Mọi con số dưới đây **suy ra từ code**, không ước lượng — nguồn ghi
> ngay cạnh từng bảng.

---

## 1. Vì sao đếm được chính xác

`OD-V1-19` bỏ tên khách khỏi lời thoại, nên mọi thứ còn lại đều **hữu hạn và biết trước**. Template
`v3-test-approved` còn ba placeholder động, và cả ba phân rã thành các bộ đóng:

| Placeholder | Phân rã thành |
| --- | --- |
| `{{total_amount_display}}` | bộ từ số + `đồng` — **đóng**, suy ra từ `VietnameseNumberSpeller` |
| `{{delivery_area_short}}` | danh sách phường/quận phục vụ — **đóng**, do vận hành định |
| `{{items_spoken}}` | số lượng (dùng lại bộ số) + đơn vị + tên hàng + từ nối — **đóng** |

## 2. Bank A — bộ từ số

Suy ra từ `src/Ivr.Domain/Speech/VietnameseNumberSpeller.cs` và
`VietnameseOrderScriptRenderer.cs:169`. Đây là **toàn bộ** từ mà speller có thể phát ra:

| Nhóm | Token | Số |
| --- | --- | ---: |
| chữ số | `không` `một` `hai` `ba` `bốn` `năm` `sáu` `bảy` `tám` `chín` | 10 |
| hàng | `mười` `mươi` `trăm` | 3 |
| thang | `triệu` `tỷ` | 2 |
| dạng đặc biệt | `mốt` (đơn vị 1 khi chục ≥ 2) · `tư` (4 khi chục ≥ 2) · `lăm` (5) | 3 |
| thập phân | `phẩy` | 1 |
| tiền tệ | `đồng` | 1 |
| **khác theo miền** | `nghìn` (Bắc) / `ngàn` (Trung, Nam) | 1 |
| **khác theo miền** | `linh` (Bắc) / `lẻ` (Trung, Nam) | 1 |
| | **mỗi miền** | **22** |

Hai dòng cuối là lý do bộ này **không dùng chung ba miền được** — `VietnameseNumberStyle` khai
`Northern = ("nghìn","linh")`, `CentralDefault = Southern = ("ngàn","lẻ")`.

## 3. Bank B — từ nối

Suy ra từ `VietnameseOrderScriptRenderer.cs:289-300`:

| Token | Dùng khi |
| --- | --- |
| `và` | nối hai món cuối — `1 => x`, `2 => "a và b"`, `_ => "a, b và c"` |
| `sản phẩm khác` | phần dư khi vượt `MaximumSpokenItems`: *"…, và hai sản phẩm khác"* |
| *(khoảng lặng)* | dấu phẩy — **không cần thu**, là im lặng giữa clip |

**2 clip** mỗi miền.

## 4. Bank C, D, E — phụ thuộc dữ liệu, không suy ra từ code được

| Bank | Nguồn | Số lượng |
| --- | --- | --- |
| **C — đơn vị** | `item.UnitLabel` | ✅ **owner chốt `09/09`: đúng một — `hộp`** |
| **D — tên hàng** | `item.PublicName` | ⚠️ **20 SKU, nhưng tên còn trống ở Product Master** — xem §13 |
| **E — vùng giao** | `delivery_area_short` | ⚠️ **không phải việc dữ liệu** — xem §11 |

## 5. Tổng, và ràng buộc `MaximumSpokenItems`

Với `D = 40` và `E` chưa biết:

```text
mỗi miền  = 107 (A, theo R-2) + 2 (B) + 1 (C, chốt 09/09) + ~40 (D) + E
          ≈ 150 + E
ba miền   ≈ 450 + 3E
```

> Con số `≈ 222 + 3E` ở bản đầu tính theo `R-1` (`22` clip số). Owner chốt `R-2`, nên bank A là
> `107` và tổng tăng lên `≈ 477 + 3E`. Phần tăng là **100 dòng dùng chung một kịch bản văn bản cho
> cả ba giọng**, nên công soạn không nhân ba — chỉ công thu.

Cộng **4 đoạn cố định × 3 miền = 12** — hiện đã có nhưng là **audio do VieNeu render**, nên phải thu
lại nếu đi hướng giọng người (xem §7).

`VietnameseOrderScriptRenderer` chặn `maximumSpokenItems` ngoài `1..20`, và món vượt ngưỡng gộp
thành *"và N sản phẩm khác"* — nên số clip **không** tăng theo độ dài đơn hàng.

## 6. ⚠️ Rủi ro thật: mối nối, không phải số lượng

Ghép từng **từ** một sẽ nghe tệ hơn TTS. Tiếng Việt có thanh điệu và ngữ điệu cấp cụm; nối 6 clip
rời cho *"năm trăm sáu mươi nghìn đồng"* nghe máy móc dù từng clip đều đúng.

Và repo **đã biết điều này** — comment tại `VietnameseOrderScriptRenderer.cs:163-164`:

> *renderer used to emit `"560.000 đồng"` while the audio the owner approved in `W-0104` says
> `"năm trăm sáu mươi nghìn đồng"`*

Bản owner duyệt là một câu đọc **liền mạch**, không phải sáu clip ghép.

### Hai cách, phải chọn một

| | Cách | Số clip mỗi miền | Chất lượng mối nối |
| --- | --- | ---: | --- |
| **R-1** | thu từng từ như §2 | **22** | rủi ro — 6 mối nối trong một số tiền |
| **R-2** | thu **cụm** `0..99` liền mạch + thang + hàng trăm | **~120** | ít mối nối hơn hẳn; là cách IVR truyền thống vẫn làm |

### ✅ Owner chốt `R-2` — `2026-09-08`

Và sau khi **đo bằng chính speller**, tôi phải sửa lại câu *"ít mối nối hơn hẳn"* ở trên: **không
hẳn**. Số thật, trên bảy mức tiền đơn hàng thực tế:

| Số tiền | Đọc thành | R-1 | **R-2** | R-3 (`0..999`) |
| ---: | --- | ---: | ---: | ---: |
| `150.000` | một trăm năm mươi nghìn đồng | 6 | **5** | 3 |
| `560.000` | năm trăm sáu mươi nghìn đồng | 6 | **5** | 3 |
| `1.200.000` | một triệu hai trăm nghìn đồng | 6 | **6** | 5 |
| `2.350.000` | hai triệu ba trăm năm mươi nghìn đồng | 8 | **7** | 5 |
| | **mối nối trung bình** | `5,6` | **`4,7`** | `2,9` |
| | **bank mỗi miền** | `22` | **`107`** | `1005` |

`R-2` tốn **gấp ~5 lần** công thu để bớt **~1 mối nối**. Nếu chỉ đếm mối nối thì đó là món hời kém.

**Nhưng nó vẫn là lựa chọn đúng, vì vị trí mối nối quan trọng hơn số lượng.** Dưới `R-1`,
*"sáu mươi"* bị cắt thành `[sáu][mươi]` — mối nối nằm **giữa một cụm ngữ điệu**, chỗ tệ nhất có thể
cắt trong tiếng Việt. `R-2` xoá đúng loại mối nối đó. Số còn lại rơi vào ranh giới tự nhiên
(`trăm`, `nghìn`, `đồng`), nơi người đọc vốn đã ngắt hơi.

Và với `items_spoken` thì `R-2` thắng rõ: **mọi số lượng `1..20` là đúng một clip**, trong khi `R-1`
cần tới ba (`21` = `[hai][mươi][mốt]`).

> **Nếu ưu tiên là số mối nối chứ không phải vị trí**, `R-3` (`0..999`) gần như **giảm một nửa**
> — nhưng `1005` clip mỗi miền, gấp mười `R-2`. Không đề xuất, chỉ ghi ra để lựa chọn được ra trên
> số thật.

### Kịch bản thu `R-2` — 100 dòng, sinh từ `VietnameseNumberSpeller`

> **Cả 100 chuỗi giống hệt nhau ở ba miền** — kiểm bằng cách spell `0..99` với cả
> `Northern`/`CentralDefault`/`Southern` rồi so: `100` chuỗi phân biệt mỗi miền, **không dòng nào
> lệch**. Nghĩa là **một kịch bản văn bản, ba giọng đọc**. Khác biệt miền chỉ xuất hiện ở
> `nghìn`/`ngàn` và `linh`/`lẻ`, cả hai đều **ngoài** dải `0..99`.

| ` 0` không | `20` hai mươi | `40` bốn mươi | `60` sáu mươi | `80` tám mươi |
| ` 1` một | `21` hai mươi mốt | `41` bốn mươi mốt | `61` sáu mươi mốt | `81` tám mươi mốt |
| ` 2` hai | `22` hai mươi hai | `42` bốn mươi hai | `62` sáu mươi hai | `82` tám mươi hai |
| ` 3` ba | `23` hai mươi ba | `43` bốn mươi ba | `63` sáu mươi ba | `83` tám mươi ba |
| ` 4` bốn | `24` hai mươi tư | `44` bốn mươi tư | `64` sáu mươi tư | `84` tám mươi tư |
| ` 5` năm | `25` hai mươi lăm | `45` bốn mươi lăm | `65` sáu mươi lăm | `85` tám mươi lăm |
| ` 6` sáu | `26` hai mươi sáu | `46` bốn mươi sáu | `66` sáu mươi sáu | `86` tám mươi sáu |
| ` 7` bảy | `27` hai mươi bảy | `47` bốn mươi bảy | `67` sáu mươi bảy | `87` tám mươi bảy |
| ` 8` tám | `28` hai mươi tám | `48` bốn mươi tám | `68` sáu mươi tám | `88` tám mươi tám |
| ` 9` chín | `29` hai mươi chín | `49` bốn mươi chín | `69` sáu mươi chín | `89` tám mươi chín |
| `10` mười | `30` ba mươi | `50` năm mươi | `70` bảy mươi | `90` chín mươi |
| `11` mười một | `31` ba mươi mốt | `51` năm mươi mốt | `71` bảy mươi mốt | `91` chín mươi mốt |
| `12` mười hai | `32` ba mươi hai | `52` năm mươi hai | `72` bảy mươi hai | `92` chín mươi hai |
| `13` mười ba | `33` ba mươi ba | `53` năm mươi ba | `73` bảy mươi ba | `93` chín mươi ba |
| `14` mười bốn | `34` ba mươi tư | `54` năm mươi tư | `74` bảy mươi tư | `94` chín mươi tư |
| `15` mười lăm | `35` ba mươi lăm | `55` năm mươi lăm | `75` bảy mươi lăm | `95` chín mươi lăm |
| `16` mười sáu | `36` ba mươi sáu | `56` năm mươi sáu | `76` bảy mươi sáu | `96` chín mươi sáu |
| `17` mười bảy | `37` ba mươi bảy | `57` năm mươi bảy | `77` bảy mươi bảy | `97` chín mươi bảy |
| `18` mười tám | `38` ba mươi tám | `58` năm mươi tám | `78` bảy mươi tám | `98` chín mươi tám |
| `19` mười chín | `39` ba mươi chín | `59` năm mươi chín | `79` bảy mươi chín | `99` chín mươi chín |

Ngoài 100 dòng trên, mỗi miền còn **7 clip**:

| Clip | Ghi chú |
| --- | --- |
| `trăm` `triệu` `tỷ` | chung ba miền |
| `phẩy` | phần thập phân số lượng — *"hai phẩy năm ký"* |
| `đồng` | ghép ở `renderer:169` |
| `nghìn` **hoặc** `ngàn` | Bắc dùng `nghìn`; Trung, Nam dùng `ngàn` |
| `linh` **hoặc** `lẻ` | Bắc `linh`; Trung, Nam `lẻ` — chỉ xuất hiện ở hàng trăm, *"một trăm linh năm"* |

**Bank A theo `R-2` = `107` clip mỗi miền**, thay cho `22` ở §2.

> **Ranh giới `106` / `107`, ghim bằng `UT-VOICE-CLIP-07`.** `VietnameseNumberSpeller` phát ra
> **106**; clip thứ `107` là **`đồng`**, do `VietnameseOrderScriptRenderer:169` ghép. Đọc một con
> số không bao gồm đơn vị tiền tệ — `2,5 ký` chứng minh điều đó bằng cách không cần clip nào.
> Ai làm đường clip cho renderer thì sở hữu clip đó; ghi ra đây để hai bên không cùng tưởng bên kia
> phát ra nó.
>
> **`W-0234` đã thi hành**: `VietnameseOrderScriptRenderer.TotalAmountClips` sở hữu
> `new SpeechNumberClip("num-dong", "đồng")`, và `UT-VOICE-CLIP-10` ghim
> `560.000` → `[num-05][num-hundred][num-60][num-thousand][num-dong]` — **5 clip, 4 mối nối**,
> đúng con số §6 dự đoán cho `R-2`.

> **6 cuộc MicroSIP trong `today-03 §3.2` chính là bài kiểm cho câu này** — cột *"6 mối nối
> `1→2→3→4→5→6→7`"*. Bài kiểm đã thiết kế từ `29/08`, chỉ chưa chạy.

## 7. Hai câu chưa ai trả lời, và chúng không nhỏ

### 7.1 — 12 đoạn cố định hiện có: giữ bản VieNeu hay thu lại?

| Giữ bản VieNeu render | Thu lại bằng giọng người |
| --- | --- |
| `L3` (quyền thương mại 3 preset) **vẫn phải trả lời** | `L1`–`L4` **đóng hết** — không còn dùng model |
| chữ ký giọng `28/08` còn hiệu lực | phải chọn giọng lại, chữ ký `28/08` hết hiệu lực |
| lời thoại nửa VieNeu nửa người → **nguy cơ lệch chất giọng ngay mối nối** | đồng nhất |

Vế thứ ba là vế dễ bỏ sót: đoạn cố định do model đọc, đoạn động do người đọc, nối vào nhau ở đúng
chỗ khách đang nghe.

### ✅ Owner chốt `2026-09-08`: **thu lại 12 đoạn cố định**

VieNeu biến mất **hoàn toàn** — không chỉ khỏi runtime mà khỏi cả audio giao đi.

#### Cơ chế phát **không phải đổi gì** — tên file khoá theo *văn bản*, không theo giọng

`TargetV1SpeechPolicy.FixedSegmentHashes` băm **text của từng đoạn**, và catalog trỏ file bằng chính
hash đó:

```json
{ "TextHash": "4612fb85a1d0c3df431aa831b7fe2ec8deeae0a689f6ffc21237665af9aae8b2",
  "MediaReference": "sound:ivr-seg-north-4612fb85a1d0c3df",
  "DurationMilliseconds": 6400 }
```

Văn bản không đổi ⇒ `TextHash` không đổi ⇒ `MediaReference` không đổi ⇒ `ValidateSegmentation`
không đỏ. **Thay nội dung WAV, giữ nguyên tên.** Danh sách phải cập nhật rất ngắn:

| Phải đổi | Không đổi |
| --- | --- |
| bytes của 12 file `ivr-seg-*.wav` | `TextHash`, `MediaReference`, tên file |
| `SHA256SUMS` trong `deploy/lab/asterisk/audio/` | template, `FixedSegmentHashes`, cấu hình catalog |
| `DurationMilliseconds` từng entry — người đọc dài ngắn khác model | cơ chế `Segmentation` |

> **`DurationMilliseconds` nối sang chỗ khác:** độ dài đoạn cố định cộng vào thời lượng cuộc gọi,
> tức đúng con số `W-0008` sẽ đo và `CAP-DRIFT-05` đang ghim (`40/35/50/60`). Thu xong thì có phép
> đo thật cho phần cố định — **không** thay được `W-0008`, nhưng là dữ liệu vào cho nó.

#### Bộ provenance VieNeu thành đồ thừa — nhưng **đừng gỡ vội**

Chuỗi thứ hai khoá theo **model + preset**, không theo văn bản:

```text
tts-provenance-gate.mjs:160        throw "voice manifest drift"
tts-voice-acceptance-lib.mjs:19    voice_manifest_sha256 → voices_v3_turbo.json
b3-telephony-evidence-validator:22 ghim voice-acceptance-manifest.json = 90927e16…
```

Chúng vẫn **xanh** vì vẫn mô tả đúng artifact đang nằm trong repo — chỉ là artifact đó thôi được
giao đi. Gỡ chúng là **một work item riêng, làm cẩn thận**: đây chính là bộ máy đã bắt được
`2a4f45d` tự ký `legal_gate`. Gỡ ẩu thì mất luôn phần chống tự ký.

#### Chữ ký giọng `28/08` hết hiệu lực

Ngọc Linh / Ngọc Trân / Mỹ Duyên là **preset của model**, không phải người. Thu bằng giọng người
thì phải chọn giọng lại — và lần này là **chọn người**, kèm hợp đồng.

| Đóng lại | Mở ra |
| --- | --- |
| `L1` licence không có file LICENSE | **hợp đồng giọng người** — phạm vi sử dụng, thời hạn, thu lại khi catalog đổi |
| `L2` training data | quyền dùng bản thu cho mục đích thương mại |
| `L3` quyền thương mại 3 preset | |
| `L4` attribution / NOTICE | |

**Bốn đóng, một mở** — và câu mở là câu Legal quen thuộc hơn nhiều câu về training data của model.

> Chỉ đóng khi **audio thu lại đã thay 12 file cũ**, không phải khi quyết định được ký. Tới lúc đó
> 12 file đang giao vẫn là bản VieNeu render.

#### Gộp một buổi thu

`107` clip số + `12` đoạn cố định + đơn vị + tên hàng + vùng giao — **cùng một session mỗi giọng**,
thay vì hai lần vào phòng thu.

### 7.2 — `pronunciationHints` mất tác dụng, và đó là đổi hành vi contract

`PrivacySafeSpeech` cho **tới 100** `pronunciationHints` mỗi task; renderer dùng chúng để đọc đúng
tên hàng: `pronunciationHints.GetValueOrDefault(item.PublicName, item.PublicName)`.

Cơ chế đó tồn tại **vì** TTS đọc sai tên riêng. Thu trước thì không đọc lại được — một clip đã thu
là một clip đã thu.

### ✅ Owner chốt `2026-09-08`: **hint bị bỏ qua; tên hàng có clip thì dùng clip**

Đó là lựa chọn giữ được đường bỏ TTS khỏi runtime. Phải ghi vào IR-06: **M3 vẫn được gửi
`pronunciation_hints`, và với món đã có clip thì hint không có tác dụng** — im lặng không có tác
dụng, không phải lỗi. Không ghi ra thì M3 sẽ gửi hint rồi tự hỏi vì sao không nghe thấy.

### ⚠️ Nhưng câu vừa chốt mở ngay câu kế: **món chưa có clip thì sao?**

Kiểm contract hiện tại thì **không có gì chặn một tên hàng lạ**:

| Trường | Ràng buộc thật | Catalog? |
| --- | --- | --- |
| `public_name` | non-blank, **≤ 160 ký tự**, PII-safe (`SpeechItem.Create`) | **không** |
| `unit_label` | optional, **≤ 40 ký tự** | **không** |
| `items[]` | `1..100` phần tử (`TaskIntakeEndpoint.cs:245`) | — |

Cả hai là **free text từ M3**. Hôm nay điều đó vô hại vì TTS đọc được mọi chuỗi. Dưới mô hình ghi
âm thì **Sales thêm một món ngày mai là một cuộc gọi không đọc được** — rủi ro vận hành thật, không
phải giả định.

Bank C và D chỉ **đóng** được nếu có chỗ nào đó cưỡng chế. Hôm nay không có chỗ nào.

#### Bốn cách, và một đề xuất

| | Cách | Hệ quả |
| --- | --- | --- |
| 1 | rơi về TTS cho món lạ | **TTS ở lại runtime** — mâu thuẫn chính quyết định vừa ký |
| 2 | từ chối ở intake | M3 phải biết catalog ghi âm; đơn thật hỏng vì một món mới |
| 3 | gộp thành *"N sản phẩm"* | **đã có tiền lệ trong code** — renderer gộp phần vượt `MaximumSpokenItems` thành *"và N sản phẩm khác"* |
| 4 | đẩy sang admin review | `REVALIDATE_AND_HOLD_ADMIN_REVIEW` đã có trong ma trận kết quả |

> **Đề xuất: `3` cộng một chặn đáy.** Món có clip thì đọc tên; món không có gộp vào *"và N sản phẩm
> khác"* bằng đúng cơ chế đang chạy. **Nhưng nếu không món nào có clip thì không gọi** — câu
> *"Quý khách có đơn hàng gồm một sản phẩm"* không xác nhận được gì, và gọi khách để đọc một câu vô
> nghĩa tệ hơn là không gọi. Trường hợp đó đi `4`.
>
> Cách này không cần M3 biết catalog, không thêm đường hỏng mới, và dùng lại cơ chế đã có test.

**Còn một việc không phải code:** ai báo cho IVR khi Sales thêm sản phẩm? Bank D chỉ đúng tới lần
thay đổi catalog kế tiếp. Đó là quy trình vận hành, phải có tên người.

## 8. Cái này đóng được gì — và chưa đóng được gì

| Mục | Trạng thái sau quyết định của owner |
| --- | --- |
| `INF-A` — mirror 201 MiB weights | **không còn cần**, *sau khi* đường ghi âm dựng xong và VieNeu rời runtime |
| Security 16 CVE | **không còn là câu hỏi release**, cùng điều kiện |
| Legal `L1` `L2` `L4` | đóng **nếu** §7.1 chọn thu lại |
| Legal **`L3`** | **vẫn cần** nếu giữ 12 đoạn VieNeu |
| Legal `L5`–`L7` | **cần dù đường nào** |
| Hợp đồng giọng người | **câu hỏi mới** — thay chỗ câu hỏi licence model |

> Cả cột phải đều là **điều kiện**, không phải đã xảy ra. Hôm nay `Segmentation` chỉ phục vụ đoạn
> **cố định** từ file; đoạn động vẫn tới provider — `TtsTelemetry.DynamicSynthesized` ghi thẳng
> *"Variable pieces that reached the provider"*. **Chưa có cơ chế nào phục vụ đoạn động từ ngân
> hàng ghi âm.** Đó là code chưa viết, không phải cờ chưa bật.

## 9. Việc phải làm, theo thứ tự

| # | Việc | Ai |
| ---: | --- | --- |
| ~~1~~ | ~~Chốt `R-1` hay `R-2`~~ → ✅ **owner chốt `R-2` `2026-09-08`**; kịch bản 100 dòng đã sinh ở §6 | — |
| ~~2~~ | ~~Chốt §7.1 — giữ hay thu lại 12 đoạn cố định~~ → ✅ **owner chốt 08/09: thu lại**; `L1`–`L4` đóng khi audio mới thay xong, mở ra hợp đồng giọng người | — |
| ~~3~~ | ~~Chốt §7.2 — hint bị bỏ qua hay thắng~~ → ✅ **owner chốt 08/09: hint bị bỏ qua** |  — |
| ~~3b~~ | ~~món chưa có clip xử lý ra sao~~ → ✅ **owner chốt 08/09 theo đề xuất `3`+chặn đáy**; đã thi hành ở `RecordedSpeechComposer` (`W-0235`), 6 test `UT-VOICE-3B-01..06`. **Chưa nối vào renderer** — bank C/D còn rỗng nên bật lúc này là mọi món đều gộp | — |
| **3c** | ai báo cho IVR khi Sales thêm sản phẩm — tách khỏi `3b` vì là quy trình, không phải code | Vận hành |
| **4a** | vùng giao: đọc cả chuỗi hay **đọc tỉnh** — §11. **Đề xuất: tỉnh**, và nó là lựa chọn duy nhất không chờ chữ ký Privacy/Legal | Owner + Product |
| ~~4b~~ | ~~danh sách đơn vị~~ → ✅ **owner chốt `09/09`: một đơn vị, `hộp`**; đã vào code ở `RecordedSpeechCatalog.SettledUnitClipIds`, ghim bởi `UT-VOICE-4B-07` (`W-0237`) | — |
| **4c** | Sales master data còn phát dạng **chỉ-có-quận** không? (§11) — nếu có thì sai giọng **từ hôm nay**, độc lập với ghi âm | Bên nắm Sales master data |
| **4d** | **bank D** — không phải việc gõ danh sách: `PACK-02 §32.2` có **20 ô tên còn là `{{placeholder}}`**. Điền registry **trước**, thu âm **sau** — xem §13 | Owner + Product Master |
| 5 | Dựng cơ chế phục vụ đoạn động từ bank | **M8** — sau khi 1–4 xong |
| 6 | Thu âm, rồi 6 cuộc MicroSIP `today-03 §3.2` | Owner |

**M8 chưa làm được bước 5 khi 1–4 chưa chốt** — mỗi lựa chọn ở trên đổi hình dạng của bank và của
cơ chế tra cứu.

---

## 10. Một điểm thiết kế không phụ thuộc câu nào còn mở

Bước 5 chưa dựng được, nhưng **hình dạng của nó thì quyết được ngay**, và quyết sai thì đắt.

### Cám dỗ: render ra văn bản rồi cắt văn bản thành clip

Cách này hỏng vì ranh giới clip **không nằm trong chữ**:

```text
     21  →  "hai mươi mốt"                        R-2: 1 clip  [21]
    121  →  "một trăm hai mươi mốt"               R-2: 3 clip  [1][trăm][21]
   1021  →  "một nghìn không trăm hai mươi mốt"   R-2: 5 clip  [1][nghìn][0][trăm][21]
```

Cùng ba chữ *"hai mươi mốt"*, ba ngữ cảnh khác nhau. Muốn cắt đúng thì phải biết nó **đến từ số
nào** — tức phải **viết ngược lại speller**: luật `mốt`/`tư`/`lăm`, phân biệt `mười`/`mươi`, và cả
filler `không trăm` ở `1021`.

> Quét `0..200.000` thì **0 va chạm** — cách đọc là song ánh, nên về lý thuyết suy ngược được.
> Vấn đề không phải *có làm được không* mà là **cái giá**: một bộ phân tích ngược phải khớp với
> speller **mãi mãi**. Đó đúng là kiểu lỗi `7.1` vừa sửa — một luật, hai bản, rồi chúng trôi khỏi
> nhau.

### Thay vào đó: danh sách clip là **primitive**, văn bản là **hình chiếu**

Renderer **đã có** `decimal` gốc — số lượng và số tiền. `VietnameseNumberSpeller.Spell` đã duyệt
theo nhóm ba chữ số với mảng `scales`. Cùng một lượt duyệt đó phát ra danh sách clip; **văn bản
sinh ra bằng cách nối tên clip lại**.

```text
hôm nay      số ──Spell──> văn bản ──TTS──> audio
sai          số ──Spell──> văn bản ──parser ngược──> clip     ← hai luật, sẽ trôi
đúng         số ──walk──> clip ──join──> văn bản               ← một luật
```

Được ba thứ cùng lúc:

- **không thể trôi** — văn bản dẫn xuất từ clip, nên chúng luôn mô tả cùng một thứ;
- `TemplateHash` và mọi test đang so văn bản **vẫn chạy nguyên**, vì văn bản không đổi giá trị;
- và clip list là thứ **đếm được**, nên "bao nhiêu mối nối trong một cuộc gọi" thành một con số
  kiểm được bằng test thay vì một cảm nhận sau khi nghe.

### Câu `3b` cắm vào đâu

Đúng một chỗ: hàm tra `tên hàng → clip`. Trả về `null` thì áp luật `3b`. **Không** ảnh hưởng phần
số — bank A luôn đủ, vì `0..99` phủ mọi số lượng và mọi nhóm ba chữ số.

Nên `3b` và `4` chặn **thi hành**, không chặn **thiết kế**. Điểm này chốt được ngay.

---

## 11. Bank E không cùng loại với bank D — `4` giấu một quyết định

Bank C và D tra bằng **chuỗi chính xác**: M3 gửi đúng tên trong catalog, `RecordedSpeechCatalog`
so ordinal sau khi trim. Với tên hàng thì đúng — nó là một danh mục có kiểm soát.

**Với vùng giao thì không.** Kiểm 12 chuỗi `delivery_area_short` thật đang có trong repo:

```text
Phường Bến Nghé, TPHCM
Phường Bến Nghé, TP. Hồ Chí Minh
Phường Bến Nghé, Quận 1
Phường 12, Thành phố Hồ Chí Minh
Quận 7, TP Hồ Chí Minh
phường 12, quận Bình Thạnh
Phường Phú Khương, tỉnh Vĩnh Long
…
```

Hai vấn đề, cả hai chí mạng cho tra-chuỗi-chính-xác:

1. **Một nơi, nhiều cách viết.** `TPHCM` / `TP. Hồ Chí Minh` / `Thành phố Hồ Chí Minh` là ba chuỗi
   khác nhau cho cùng một nơi. Tra ordinal sẽ **trượt cả ba trừ một**.
2. **Chuỗi mang cả phường.** Phường không phải `~40` như tên hàng. `ShortDeliveryArea` chỉ chặn
   `160` ký tự và cấm chi tiết địa chỉ — nó **không** giới hạn tập giá trị.

⇒ **`4` không phải "đưa danh sách" cho vùng giao.** Nó là một quyết định nội dung.

### Đề xuất: đọc **tỉnh**, không đọc cả chuỗi

`DeliveryRegionResolver` **đã** parse chuỗi này để chọn giọng, và nó xử lý biến thể chính tả tốt —
chạy trên đúng 12 mẫu trên:

```text
parse nhận ra tỉnh: 9/12
  TPHCM / TP. Hồ Chí Minh / Thành phố Hồ Chí Minh  →  cùng South   ✅
  3 chuỗi trượt đều là dạng chỉ có QUẬN, không có tỉnh
```

| | Đọc cả chuỗi | **Đọc tỉnh** |
| --- | --- | --- |
| số clip mỗi miền | **hàng nghìn** phường | **~34** tỉnh |
| biến thể chính tả | phải tự chuẩn hoá | `ToMatchKey` **đã giải** |
| khách nghe | *"giao đến phường Phú Khương, tỉnh Vĩnh Long"* | *"giao đến Vĩnh Long"* |

Cái giá là **mất tên phường**. Đáng cân nhắc, nhưng: khách biết địa chỉ của chính họ; thứ cuộc gọi
cần xác nhận là **đơn này có phải của họ không**, mà món hàng và số tiền đã trả lời rồi.

Chi phí kỹ thuật nhỏ: resolver hiện trả `VietnamRegion?`, cần thêm một hàm trả **tên tỉnh khớp**.

### Và lập luận mạnh nhất không phải kỹ thuật — nó là privacy

`OD-V1-15` (*"whitelist biến đọc trong call script: bộ hẹp 4 biến hay bộ rộng có danh sách sản
phẩm?"*) đã được ký `2026-09-05`: **bộ rộng**, gồm *"tên món + số lượng + vùng giao rút gọn"*.

Nhưng cột owner của nó là **Product + Privacy/Legal**, và spec V0.3 `§12.2` ghi thẳng:

> *"Mở rộng whitelist tự nó là một quyết định privacy, cần Privacy/Legal ký."*

Chữ ký `05/09` là của **IVR owner** — tức đây đúng dạng dòng mà worklist `3.2` bảo phải ghi
`M8_POSITION_SIGNED / <owner> NOT_RECEIVED`.

Trong bối cảnh đó, hai lựa chọn **không ngang nhau**:

| | Đọc cả chuỗi (phường + tỉnh) | **Đọc tỉnh** |
| --- | --- | --- |
| so với `OD-V1-15` | ở **rìa ngoài** phần vừa mở rộng | **hẹp hơn** — nằm gọn bên trong |
| chữ ký Privacy/Legal | spec nói **cần**, **chưa có** | không cần thêm gì |
| clip mỗi miền | hàng nghìn phường | **~34** tỉnh |
| biến thể chính tả | phải tự chuẩn hoá | `ToMatchKey` **đã giải** |

**Đọc tỉnh là lựa chọn duy nhất không phụ thuộc một chữ ký chưa có.** Đọc cả chuỗi thì phải chờ
Privacy/Legal, và nó nằm cùng hàng đợi với `L5`–`L7` của phiếu Legal.

> ⚠️ **Spec V0.3 `§12.2` dòng `293-294` vẫn ghi `delivery_area_short` là "ĐANG TRANH CHẤP —
> OD-V1-15"** — cũ 4 ngày so với chữ ký `05/09`. Cùng lớp lỗi `3.5`: spec mô tả một hệ thống không
> còn đúng. **Không tự sửa** — thẩm quyền sửa spec thuộc chief auditor/Owner, y như `3.5`.

### Một câu hỏi kèm theo, **không** phải defect

Ba chuỗi trượt — `"Phường Bến Nghé, Quận 1"`, `"Quận Một"`, `"quận Bình Thạnh"` — chỉ có **quận**,
không có tỉnh. Resolver ghi rõ là **có chủ đích**: *"The 2025 reform removed the district tier, so a
delivery area is normally just ward plus province."*

Nhưng bảng đã mang **29 tên tỉnh trước sáp nhập** vì *"Sales master data and in-flight orders can
still carry the old names"* — cùng lý do đó áp cho quận thì không có gì. Và ba chuỗi kia đang là
**fixture khắp repo**, tuy **không test nào khẳng định** giọng nào đúng cho chúng.

Hôm nay chúng rơi về `FallbackRegion = North`. Một đơn ở **Bình Thạnh** được đọc bằng giọng **Bắc**.

> **Câu hỏi cho bên nắm Sales master data:** dữ liệu có còn phát ra dạng chỉ-có-quận không? Có thì
> đây là việc phải sửa và nó **độc lập với ghi âm** — sai giọng đã sai từ hôm nay. Không thì ba
> fixture kia nên đổi sang dạng sau sáp nhập để khỏi mô tả một đầu vào không còn tồn tại.

---

## 12. `4b` đã chốt — một đơn vị, và một cái bẫy hoa thường

Owner `2026-09-09`: **đơn vị là `hộp`.** Bank C từ ước lượng `~10` xuống **`1`**; tổng mỗi miền
`≈ 150 + E`.

Repo đồng ý: năm đơn vị khác từng xuất hiện (`gói` `chai` `kg` `túi` `thùng`) **chỉ nằm trong test
fixture**, không spec nào khai.

### Nhưng `"Hộp"` viết hoa có thật trong repo

```text
"hộp"  ×33      "Hộp"  ×4   ← CallResultAndMapperTests, DomainPolicyAndPrivacyTests
```

`RecordedSpeechCatalog` ban đầu so **ordinal, phân biệt hoa thường**, nên `"Hộp"` sẽ **trượt** và
`3b` gộp một đơn hoàn toàn bình thường thành *"một sản phẩm khác"* — im lặng, không gì đỏ.

Đã tách hai luật, vì hai loại chuỗi khác nhau:

| | So sánh | Vì sao |
| --- | --- | --- |
| tên hàng | **ordinal, phân biệt hoa thường** | danh từ riêng — hoa thường có thể mang nghĩa |
| **đơn vị** | **bỏ qua hoa thường** | danh từ chung — hoa thường chỉ là cách gõ trường dữ liệu |

**Không bên nào bỏ dấu.** `hộp`, `hợp`, `họp` là ba từ khác nhau, và `hop` không phải từ nào trong
ba. `UT-VOICE-4B-07` ghim cả bốn dạng viết lẫn ca `hop` bị gộp.

### Rủi ro tồn dư, nói rõ

`unit_label` trong OAS là `{ type: string, maxLength: 40 }` — **không enum**. Nên một đơn vị khác
`hộp` vẫn tới được, và nó sẽ **gộp** theo `3b` chứ không đọc sai. Đúng thiết kế, nhưng **im lặng**:
một đơn giao bằng `chai` mất đơn vị mà không gì báo. Nếu catalog sau này có sản phẩm bán theo chai
thì bank C phải mở rộng **trước**, không phải sau.

---

## 13. Bank D không nằm ở IVR — nó nằm ở Product Master, và ở đó nó chưa có

Đi tìm danh sách tên hàng thì nó **đã có chỗ**, và chỗ đó **còn trống**.

`docs/documents/2. pack/02-PACK-02-PRODUCT-MASTER-SKU-RECIPE-ACTIVATION.md`:

- **§4.1** — *"20 SKU canonical là danh mục sản phẩm nền của Ginsengfood"*
- **§5.2** — mỗi SKU **bắt buộc** có `public_product_name` và `internal_product_name`
- **§32.2** — registry 20 dòng, và cả 20 ô tên là placeholder:

```text
01 | SKU-01 | {{public_product_name_01}} | … | waiting_CONFIG / READY
…
20 | SKU-20 | {{public_product_name_20}} | … | waiting_CONFIG / READY
```

- **§33** — *"Khi triển khai dữ liệu thật, không được để placeholder đi vào production runtime."*

Và `public_name` của IVR **chính là** trường đó — IR-06 dòng `465` nêu ví dụ duy nhất:
`{ "public_name": "Nước hồng sâm", "quantity": 2, "unit_label": "hộp" }` — khớp luôn đơn vị `hộp`
owner vừa chốt.

### Hệ quả về thứ tự, và nó đắt nếu làm ngược

Tra tên hàng là **so chuỗi chính xác** (§12: cố ý, vì danh từ riêng). M3 lấy `public_name` từ
Product Master. Nên nếu thu âm theo một danh sách tạm rồi Product Master điền chuỗi khác — ví dụ
`"Nước hồng sâm Ginsengfood 500ml"` thay vì `"Nước hồng sâm"` — thì **mọi clip đều trượt**, mọi
món gộp, và mọi cuộc gọi rơi vào chặn đáy. Buổi thu phải làm lại.

> **Điền `PACK-02 §32.2` trước, thu âm sau.** Đây là thứ tự bắt buộc, không phải khuyến nghị.

### Và `3c` có câu trả lời từ cấu trúc, không cần dựng quy trình mới

Sản phẩm mới **sinh ra ở Product Master**, và ở đó đã có `sku_lifecycle_status` và
`activation_status` (§5.2, §5.3). Nên câu *"ai báo cho IVR khi Sales thêm sản phẩm"* không cần một
quy trình mới: nó là **một bước trong vòng đời SKU đã có** — SKU chuyển sang `Activated` thì bank D
phải có clip trước khi SKU đó `Sellable`.

`PACK-02 §5.4` đã tách sẵn hai trạng thái đó: *"SKU Activated không đồng nghĩa Sellable"*. Chỗ để
cắm điều kiện ghi âm nằm đúng giữa hai cái.

---

## 14. "M3 có rồi thì IVR cần dữ liệu đó làm gì?"

Câu hỏi đúng, và câu trả lời nằm ở chỗ mô hình vừa đổi.

| | M3 gửi tên lúc gọi | IVR cần danh sách trước? |
| --- | --- | --- |
| **TTS** (hôm nay) | có | **không** — máy đọc chuỗi nào cũng được |
| **Ghi âm** (`OD-V1-19`) | vẫn có | **có** — phải có người **đã đọc** chuỗi đó vào micro |

**IVR không lưu danh sách như một sự thật.** Lúc chạy nó vẫn lấy `public_name` **từ M3**, y như bây
giờ. Cái nó cần là **đã có sẵn bản thu của đúng chuỗi ấy** — vì một cuộc gọi ghi âm chỉ phát được
file đã tồn tại.

Nên danh sách 20 tên **không phải dữ liệu IVR sở hữu**. Nó là **kịch bản buổi thu**.

### Thứ IVR giữ lại sau buổi thu

Một map `tên → file clip`. Map đó trả lời *"tôi có bản thu của chuỗi này không"*, **không** trả lời
*"sản phẩm nào tồn tại"*. Nó là **kho bản thu**, không phải bản sao Product Master — và nó chỉ lớn
lên khi có tên mới được thu.

IVR **không** cần `sku_id`, `product_group`, công thức, BOM, hay bất cứ trường nào khác của §5.2.
Chỉ `public_product_name`, và phải là **đúng chuỗi M3 sẽ gửi** (§13: tra so chuỗi chính xác).

### Nên thứ cần từ Product Master chỉ có hai

1. **Một lần** — danh sách tên để thu.
2. **Về sau** — tín hiệu khi có tên mới, để thu trước khi sản phẩm đó `Sellable` (§13, `3c`).

### Và danh sách **không cần đủ** để bắt đầu

Đây là chỗ `3b` trả công. Tên chưa có clip thì **gộp** vào *"và N sản phẩm khác"* — cuộc gọi vẫn
chạy, chỉ kém chi tiết. Nên:

| Bank D có | Hệ quả |
| --- | --- |
| rỗng | **mọi** cuộc gọi rơi chặn đáy — không gọi được |
| vài tên bán chạy | chạy được; đơn chỉ có hàng hiếm thì gộp |
| đủ 20 | không đơn nào bị gộp vì thiếu clip |

⇒ `4d` **không phải cổng chặn nhị phân**, nó là **độ phủ**. Thu nhóm bán chạy trước là bắt đầu được
ngay, phần đuôi bổ sung sau — miễn là mỗi tên được thu **đúng chuỗi Product Master chốt**, nếu
không thì §13 vẫn áp: thu lại.
