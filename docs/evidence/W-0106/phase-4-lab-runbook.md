# W-0106 Giai đoạn 4 — Runbook lab: PCM 8 kHz, ghim SHA-256, 6 lượt MicroSIP

Ngày: `2026-08-22`
Trạng thái: `PLUMBING_READY_AWAITING_MP3`

> Toàn bộ chuỗi xử lý đã dựng và kiểm. Chỉ còn **bước 1 phải làm tay** — render 3 file MP3 —
> vì nó cần phiên đăng nhập ElevenLabs. Từ bước 2 trở đi là chạy lệnh.
>
> Dữ liệu **fake toàn bộ**. `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi.

---

## Bước 1 — Render 3 MP3 (làm tay, ~5 phút)

Vào ElevenLabs web app, tìm **theo tên** trong Voice Library (đừng dán voice ID — xem cảnh
báo ở [kit §5](voice-audition-kit.md)):

| Miền | Giọng | Kịch bản |
| --- | --- | --- |
| Bắc | **Thắm** | [kit §4.2](voice-audition-kit.md) — bản có `nghìn` |
| Trung | **Zara** | [kit §4.1](voice-audition-kit.md) — bản có `ngàn`, Đà Nẵng |
| Nam | **Giang** | [kit §4.3](voice-audition-kit.md) — bản có `ngàn`, Vĩnh Long |

Cấu hình: Eleven v3, stability `0.35–0.50`, similarity `~0.75`, style thấp, speed `-3%`.
Giữ **y hệt nhau** cả ba — khác cấu hình giữa các miền là ba giọng lệch nhau vì lý do
không ai truy được.

Tải MP3 về `artifacts/w-0106-voice-audition/` (đã nằm trong `.gitignore`).

**Ghi lại ngay, không để sau**: tên giọng, **voice ID thật copy từ app**, model + settings,
ngày, tài khoản. Thiếu voice ID thì `manifest.txt` không đóng được.

---

## Bước 2 — Chuyển PCM và ghim checksum

```powershell
./deploy/lab/Convert-LabVoiceAudio.ps1 `
    -NorthMp3   ./artifacts/w-0106-voice-audition/tham.mp3 `
    -CentralMp3 ./artifacts/w-0106-voice-audition/zara.mp3 `
    -SouthMp3   ./artifacts/w-0106-voice-audition/giang.mp3
```

Script làm: chuẩn hóa loudness **trước** khi hạ 8 kHz (ngược lại thì giọng nhỏ hoặc vỡ trên
PCMU), xuất PCM signed 16-bit / 8 kHz / mono, tự verify lại định dạng đầu ra, rồi cập nhật
`SHA256SUMS` và `manifest.txt`.

ffmpeg chạy `bitexact` + `-map_metadata -1` để không nhét metadata encoder vào WAV. Không có
cái đó thì cùng một file nguồn ra hash khác nhau giữa hai phiên bản ffmpeg, và việc ghim
checksum thành vô nghĩa.

**Tên file là `-region-north|central|south`, không phải `-n|-c|-s`.** Hậu tố `-c` đã thuộc
về voice C của W-0104; dùng lại sẽ đè lên evidence cũ.

Script in ra bảng `Region / Voice / Seconds / Sha256` — **giữ lại độ dài thật cho bước 3**.

> ⚠️ Cột `Seconds` trên console hiện theo culture của máy, nên có thể thấy `16,77` với dấu
> **phẩy** — máy này khai `en-US` nhưng đặt dấu thập phân là phẩy. Đó chỉ là hiển thị:
> `manifest.txt` luôn ghi dấu **chấm** (`16.77`), khớp định dạng W-0104. Đã kiểm bằng dry-run.

**Đã chạy thử toàn chuỗi bằng audio giả (`2026-08-22`)** trước khi có MP3 thật, để bước 2–4
không vỡ vì lỗi script. Kết quả: WAV ra đúng `pcm_s16le / 8000 Hz / mono`; `SHA256SUMS` giữ
nguyên 3 dòng W-0104 và thêm đúng 3 dòng vùng miền; `manifest.txt` ghi UTF-8 đúng tên giọng
có dấu; tên file khớp giữa script ↔ `entrypoint.sh` ↔ `docker-compose.softphone.yml`. Toàn bộ
artefact giả đã được **hoàn tác**, repo không giữ checksum giả.

---

## Bước 3 — Bật định tuyến giọng trong compose

Sửa [`docker-compose.softphone.yml`](../../../docker-compose.softphone.yml):

1. `Ivr__Speech__Tts__RegionalVoices__Enabled` → `"true"`
2. Thay ba `FileDurationSeconds` bằng **độ dài thật** từ bước 2 (đang để tạm `18`)
3. `VoiceId` giữ nguyên (`w0106-lab-north-tham`…) — đây là định danh LAB, không phải voice ID
   ElevenLabs. Voice ID thật nằm trong `manifest.txt` để truy nguồn

Validator sẽ **chặn khởi động** nếu ba `VoiceId` trùng nhau, ba media reference trùng nhau,
hoặc thiếu file nào — nên bật sớm sẽ đỏ ngay chứ không âm thầm phát audio một miền cho tất cả.

---

## Bước 4 — Dựng lại image và khởi động lab

```powershell
./deploy/lab/Start-FreeSoftphoneLab.ps1
```

Entrypoint sẽ `sha256sum --check --strict` **toàn bộ** `SHA256SUMS` (3 file W-0104 + 3 file
W-0106) trước khi Asterisk chạy, rồi cài cả ba file vùng miền song song vào
`/var/lib/asterisk/sounds/`.

Khác W-0104: ba giọng vùng miền **cùng tồn tại**, không chọn ở lúc boot. App chọn theo từng
cuộc gọi dựa vào `delivery_area_short`. Biến `IVR_LAB_VOICE_VARIANT` (A/B/C) vẫn giữ nguyên
cho nhánh một-giọng của W-0104.

Kỳ vọng log:

```
W-0104 pinned voice variant A selected.
W-0106 regional voice north installed.
W-0106 regional voice central installed.
W-0106 regional voice south installed.
```

---

## Bước 5 — Sáu lượt gọi MicroSIP

Ba miền × hai phím. Mỗi lượt: chạy lệnh, MicroSIP đổ chuông, nhấc máy, nghe, bấm phím.

| # | Lệnh | Phím | Disposition kỳ vọng |
| --- | --- | --- | --- |
| 1 | `./deploy/lab/Invoke-FreeSoftphoneCall.ps1 -Region Central` | `1` | `IVR_CONFIRMED` |
| 2 | `./deploy/lab/Invoke-FreeSoftphoneCall.ps1 -Region Central` | `0` | `IVR_CUSTOMER_CANCELLED` |
| 3 | `./deploy/lab/Invoke-FreeSoftphoneCall.ps1 -Region North` | `1` | `IVR_CONFIRMED` |
| 4 | `./deploy/lab/Invoke-FreeSoftphoneCall.ps1 -Region North` | `0` | `IVR_CUSTOMER_CANCELLED` |
| 5 | `./deploy/lab/Invoke-FreeSoftphoneCall.ps1 -Region South` | `1` | `IVR_CONFIRMED` |
| 6 | `./deploy/lab/Invoke-FreeSoftphoneCall.ps1 -Region South` | `0` | `IVR_CUSTOMER_CANCELLED` |

**Miền Trung chạy trước** — cùng lý do như audition: nếu có gì sai thì biết sớm.

Region **không** được gửi như một field. Runner chỉ đổi `delivery_area_short` giữa ba khu
vực fake; `DeliveryRegionResolver` tự suy ra miền — đúng như sẽ chạy ở production.

| Miền | `delivery_area_short` fake |
| --- | --- |
| Bắc | `Phường Cửa Nam, thành phố Hà Nội` |
| Trung | `Phường Hải Châu, thành phố Đà Nẵng` |
| Nam | `Phường Phú Khương, tỉnh Vĩnh Long` |

---

## Bước 6 — Điều phải nghe được ở mỗi lượt

Nếu chỉ kiểm disposition thì đã bỏ sót đúng thứ W-0106 sinh ra để làm.

| # | Kiểm | Vì sao |
| --- | --- | --- |
| 1 | **Ba lượt ba giọng khác nhau** | Nếu ba lượt nghe giống hệt nhau thì định tuyến hỏng, dù disposition vẫn xanh |
| 2 | Lượt Bắc đọc **"nghìn"**, lượt Trung và Nam đọc **"ngàn"** | Chứng minh `VietnameseNumberSpeller` chạy đúng theo miền |
| 3 | Tiền đọc bằng **chữ**, không đọc "năm trăm sáu mươi chấm không không không" | Đây là lỗi F2 mà Giai đoạn 2 sửa |
| 4 | Số lượng đọc **"hai hộp"**, không phải "2 hộp" | Cùng lỗi F2 |
| 5 | `phím một` / `phím không` rõ | Chỗ khách phải hành động |

---

## Bước 7 — Ghi evidence

Cập nhật `docs/evidence/W-0106/README.md`:

- 6 `task_id` + disposition quan sát được
- 3 SHA-256 của PCM + 3 SHA-256 của MP3 nguồn
- Voice ID ElevenLabs thật của cả ba giọng
- Ảnh/log xác nhận ba giọng khác nhau và lexicon `nghìn`/`ngàn` đúng miền
- Baseline commit

MP3 nguồn **để ngoài repo** theo tiền lệ W-0104. Repo chỉ chứa PCM 8 kHz đã ghim checksum.

---

## Ranh giới

Giai đoạn 4 là **software lab evidence**. Nó không chứng minh PSTN, SIM, carrier, caller ID,
32 eSIM, Sales API thật hay quyền gọi khách hàng.

Và W-0106 vẫn **chỉ đạt `TESTS_PASS`, không lên `ACCEPTED`** chừng nào sếp chưa nghe và ký
nhận ba giọng — xem `OD-VOICE-05` (§7.2 của plan). Nếu sếp nghe ở bước 5 và duyệt luôn thì
đóng được cả hai việc trong một buổi; nếu sếp bác một giọng **sau khi** evidence đã chụp thì
phải render lại + hash lại + build lại image + chụp lại — đúng chuyện đã xảy ra ở W-0104.
