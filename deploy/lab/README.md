# W-0104 — Free Asterisk + MicroSIP lab

Profile này kiểm tra miễn phí đường đi `scheduler -> DispatchGate -> Asterisk ARI -> MicroSIP -> audio/DTMF -> normalizer` bằng dữ liệu giả. Nó không dùng modem, SIM, PSTN hay số điện thoại thật và không thay thế one-SIM evidence của W-0048.

## Safety boundary

- Chỉ chạy khi `IVR_EXECUTION_MODE=LAB_REAL_SIM`, adapter `ASTERISK_ARI` và alias đích đúng bằng `LAB-A`.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`, recording tắt, một kênh `SIM-ASTERISK-001`.
- Dial token chỉ phân giải một lần thành alias `LAB-A`; adapter không nhận raw phone number.
- `DispatchGate` chạy trước render, health check và mọi thao tác gateway.
- Sales và đơn hàng đều là fake/local; không có API hoặc credential Sales thật.

## Prerequisite

- Windows + PowerShell 7 hoặc Windows PowerShell 5.1.
- Docker Desktop đang chạy Linux containers. Không cần cài Ubuntu hoặc Asterisk trực tiếp trên Windows.
- UDP `5060` và `10000-10020` trên localhost chưa bị ứng dụng khác chiếm.
- Lần build Asterisk đầu tiên tải và biên dịch source nên có thể mất vài phút.

Asterisk 22.10.1 LTS được build từ source chính thức với SHA-256 đã ghim trong `asterisk/Dockerfile`. MicroSIP portable 3.22.12 được tải từ trang chính thức, kiểm SHA-256 đã ghim và lưu trong `deploy/lab/.local-tools/`; thư mục này không được commit.

Audio lab dùng ba mẫu cùng lời thoại fake, đã chuyển về PCM 16-bit/8 kHz/mono và ghim SHA-256 trong `asterisk/audio/SHA256SUMS`:

- `A`: `vi-VN-HoaiMyNeural` (nữ);
- `B`: `vi-VN-NamMinhNeural` (nam);
- `C`: ElevenLabs `Trung Caha - Clear, Firm and Informative`, voice ID `ueSxRO0nLF1bj93J2hVt`.

A/B được sinh bằng `edge-tts 7.2.8`; C được owner tạo bằng 302 credits trên ElevenLabs web app với script `v2-test-approved`. Cả ba chỉ là asset lab, không phải provider/API/SLA production và không được dùng với dữ liệu khách thật. Trước production phải duyệt riêng license/quyền dùng voice, plan/quota, API, privacy/DPA và tính sẵn sàng của voice ID.

## Chạy lab

Từ repository root:

```powershell
.\deploy\lab\Start-FreeSoftphoneLab.ps1
```

Để boot trực tiếp bằng voice C:

```powershell
.\deploy\lab\Start-FreeSoftphoneLab.ps1 -VoiceVariant C
```

Script tạo ARI/SIP password ngẫu nhiên chỉ trong process hiện tại, khởi động stack, tải/mở MicroSIP với account `LAB-A`, seed policy/flag fake và gửi một task fake. Khi MicroSIP đổ chuông:

1. Bấm **Answer**.
2. Nghe lời thoại đơn fake.
3. Bấm `1` để xác nhận hoặc `0` để hủy.
4. Giữ cửa sổ terminal đến khi script in disposition cuối.

Không nhập số thật hay sửa account MicroSIP sang PSTN trunk. Có thể tạo task fake tiếp theo khi stack và MicroSIP vẫn mở:

```powershell
.\deploy\lab\Invoke-FreeSoftphoneCall.ps1
```

Helper sau có thể click phím trên cửa sổ MicroSIP cho một lần test có giám sát; nó di chuyển con trỏ chuột và không nên chạy khi đang thao tác ứng dụng khác:

```powershell
.\deploy\lab\Invoke-MicroSipDtmf.ps1 -Digit 1
```

## Nghe và chọn giọng A/B/C

Khi stack và MicroSIP đang chạy, chọn từng file qua media reference cố định rồi tạo cuộc gọi mới. Runner tự đưa cửa sổ MicroSIP đang ẩn ở system tray ra foreground trước khi queue task:

```powershell
.\deploy\lab\Set-AsteriskLabVoice.ps1 -Variant A
.\deploy\lab\Invoke-FreeSoftphoneCall.ps1

.\deploy\lab\Set-AsteriskLabVoice.ps1 -Variant B
.\deploy\lab\Invoke-FreeSoftphoneCall.ps1

.\deploy\lab\Set-AsteriskLabVoice.ps1 -Variant C
.\deploy\lab\Invoke-FreeSoftphoneCall.ps1
```

Mỗi cuộc gọi phải được trả lời và bấm `1` hoặc `0` để kiểm playback không làm hỏng DTMF. Chỉ sau khi owner chấp nhận rõ một variant, xác nhận nội dung/âm lượng/tốc độ/độ tự nhiên và hai disposition vẫn đúng thì W-0104 mới được chuyển từ `TESTS_PASS` sang `ACCEPTED`. Nếu chưa chấp nhận, `REAL_CUSTOMER_CALL_ALLOWED=NO` và W-0105 chưa bắt đầu.

## Dừng và dọn lab

Dừng container và MicroSIP nhưng giữ volume database:

```powershell
.\deploy\lab\Stop-FreeSoftphoneLab.ps1
```

Chỉ khi muốn xóa cả dữ liệu Docker local của stack:

```powershell
.\deploy\lab\Stop-FreeSoftphoneLab.ps1 -PurgeData
```

`-PurgeData` là thao tác phá hủy dữ liệu local trong volume của compose project; không dùng nếu đang giữ dữ liệu dev khác trong cùng stack.

## Tiêu chí đọc kết quả

| Kết quả | Ý nghĩa |
| --- | --- |
| MicroSIP hiện `Online` và Asterisk có contact `LAB-A` | SIP registration PASS |
| Cuộc gọi tới MicroSIP và nghe audio | ARI dial/playback PASS |
| Phím `1` tạo final confirmed; `0` tạo final cancelled | DTMF + normalization PASS |
| Không bấm phím đến timeout | `IVR_NO_ANSWER_FINAL`, đúng policy một attempt của seed |
| Physical SIM/PSTN/carrier | Không thuộc profile này; vẫn `NOT_RUN` ở W-0048 |
