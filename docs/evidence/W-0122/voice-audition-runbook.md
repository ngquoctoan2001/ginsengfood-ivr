# W-0122 — Owner voice-audition runbook qua Asterisk/MicroSIP 8 kHz

Trạng thái: `OWNER_ACCEPTED 2026-08-28 — RE-RUN ONLY IF BINDING DRIFTS`

Phạm vi: software lab, fake/non-customer audio, không có outbound trunk  
`REAL_CUSTOMER_CALL_ALLOWED=NO`

## Automated pre-Owner evidence (`2026-08-27`)

| Gate | Kết quả |
| --- | --- |
| Static harness selftest | `PASS` — 11 voices, 12 exact extensions, outbound applications denied |
| BusyBox runtime verifier | `PASS` — 11/11 size/hash-bound WAVs, read-only mounts, `network=none` |
| Asterisk media decode | `PASS` — 11/11 WAV → `.sln`, 8 kHz source contract |
| Asterisk route probe | `PASS` — `12201` executed `Playback(...truc-ly)` và channel drain sạch |
| Catch-all deny probe | `PASS` — `5555` chỉ `NoOp` → `Hangup`, không `Dial()`/`Stasis()` |
| Profile probe `2026-08-28` | `PASS` — dựng thật với `-NoLaunchMicroSip`: verifier `11/11`, Asterisk healthy, `12200` load đủ 24 priority, `W0122_AUDITION_PROFILE_READY`, rồi dừng sạch |
| MicroSIP listening/Owner decision | `OWNER_ACCEPTED 2026-08-28` — đủ 11 candidate; chọn Ngọc Linh / Ngọc Trân / Mỹ Duyên; artifact `voice-acceptance-manifest.json` |

Automated probe chỉ chứng minh file/dialplan có thể phát. Owner đã hoàn tất phần nghe riêng ngày
`2026-08-28`; hướng dẫn bên dưới được giữ làm procedure bắt buộc nếu binding thay đổi và gate yêu
cầu nghe lại, không phải một việc còn mở ở TODAY-03.

## 1. Khởi động profile cô lập

Đóng MicroSIP đang chạy, sau đó từ repo root chạy:

```powershell
.\deploy\lab\Start-W0122VoiceAudition.ps1 -SkipBuild
```

Script sẽ fail closed nếu thiếu/thừa WAV, sai size/hash, verifier không exit `0`, Asterisk không
healthy hoặc extension `12200` không được load. Profile dùng project Compose riêng
`ivr-w0122-audition`; không start API/worker/scheduler và không có `Dial()`/`Stasis()`.

## 2. Nghe và ghi lựa chọn

Trong MicroSIP gọi `12200` để nghe đủ 11 giọng theo manifest. Gọi từng số để nghe lại:

| Số | Miền | Giọng |
| --- | --- | --- |
| `12201` | Bắc | Trúc Ly |
| `12202` | Bắc | Ngọc Linh |
| `12203` | Bắc | Đoan Trang |
| `12204` | Bắc | Mai Anh |
| `12205` | Bắc | Quỳnh Anh |
| `12206` | Bắc | Ngọc Huyền |
| `12207` | Trung | Ngọc Trân |
| `12208` | Nam | Thục Đoan |
| `12209` | Nam | Thùy Dung |
| `12210` | Nam | Mỹ Duyên |
| `12211` | Nam | Kim Thanh |

Owner phải nghe đủ 11 ở chính tuyến này rồi chọn đúng một Bắc, một Trung và một Nam. Nếu Ngọc
Trân không đạt thì dừng W-0122; không tự thay bằng voice clone hoặc giọng không thuộc roster.

Manifest có 23 key chính xác, 11 kết quả đúng thứ tự roster và tám binding hash. Viết tay gần như
chắc chắn bị gate từ chối vài lần, và mỗi lần từ chối là một lần dễ nảy ra ý "sửa cho nó qua".
Dùng script; nó lấy binding từ `voices.json` và chỉ hỏi thứ chỉ Owner mới biết:

```powershell
.\deploy\lab\New-W0122VoiceAcceptance.ps1 `
  -North "Trúc Ly" -Central "Ngọc Trân" -South "Thùy Dung" `
  -Rejected "Mai Anh","Kim Thanh" `
  -Listener "<ten owner>" `
  -DeviceAndLabRoute "MicroSIP -> Asterisk lab 8 kHz, tai nghe có dây" `
  -ApprovalReference "OD-VOICE-06 owner sign-off <ngay>" `
  -ConfirmAllElevenHeard
```

Script tự chạy gate và tự xoá file nếu gate đỏ. Nếu vẫn muốn điền tay thì copy
`voice-acceptance-manifest.template.json` (không sửa template gốc) rồi kiểm bằng:

```powershell
node deploy/ci/scripts/tts-voice-acceptance-gate.mjs `
  --acceptance <duong-dan-toi-voice-acceptance-manifest.json>
```

Chỉ artifact riêng trả `TTS_VOICE_ACCEPTANCE_PASS` mới được mount vào TTS và dùng để render catalog
12 file. Template pending và fixture `TEST_ONLY` luôn bị từ chối; không tự chọn voice thay Owner.

## 3. Dừng sạch

```powershell
.\deploy\lab\Stop-W0122VoiceAudition.ps1
```

Lệnh chỉ dừng project `ivr-w0122-audition`. Không xóa WAV/model/evidence và không thay đổi profile
lab W-0104/W-0106 đã được chấp nhận.
