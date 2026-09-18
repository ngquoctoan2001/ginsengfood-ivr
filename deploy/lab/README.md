# W-0104 / W-0122 — Free Asterisk + MicroSIP lab, đọc bằng VieNeu

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

## Giọng đọc: chỉ VieNeu

Lab chỉ có một bộ đọc là **VieNeu-TTS tự host** (`W-0122`), chạy làm sidecar của worker qua
`docker-compose.vieneu-tts.yml`, cũng là bộ đọc duy nhất của production. Không có bộ đọc dự phòng:
thiếu overlay thì worker không có bộ đọc nào và cuộc gọi fail closed.

- Ba giọng miền là ba giọng Owner đã nghe và duyệt ngày `2026-08-28` (`OD-VOICE-06`): Bắc
  `Ngọc Linh`, Trung `Ngọc Trân`, Nam `Mỹ Duyên`. `Start-FreeSoftphoneLab.ps1` đọc chúng thẳng
  từ `docs/evidence/W-0122/voice-acceptance-manifest.json`.
- `12` đoạn cố định của kịch bản (`4` câu × `3` miền) do VieNeu render sẵn, đã chuyển về PCM
  16-bit/8 kHz/mono và ghim SHA-256 trong `asterisk/audio/SHA256SUMS`. Phần giá trị đơn (món,
  tổng tiền, nơi giao) do sidecar tổng hợp lúc gọi.
- Model không nằm trong git. Tải bằng `deploy/tts/scripts/fetch-model-nonprod.py` và kiểm bằng
  `verify-model.py --mode nonprod` theo `deploy/tts/README.md`.

## Chạy lab

Từ repository root, trỏ tới bundle model VieNeu đã kiểm:

```powershell
.\deploy\lab\Start-FreeSoftphoneLab.ps1 -ModelBundle <thư mục bundle đã verify>
```

Script tạo ARI/SIP password ngẫu nhiên chỉ trong process hiện tại, khởi động stack cùng VieNeu
sidecar và tải/mở MicroSIP với account `LAB-A`. Script **không tự gửi task gọi**; sau khi MicroSIP
hiện `Online`, chạy:

```powershell
.\deploy\lab\Invoke-FreeSoftphoneCall.ps1
```

Nếu cần đúng hành vi một-lệnh cũ cho một lượt preflight có giám sát, dùng
`Start-FreeSoftphoneLab.ps1 -InvokePreflightCall`.

Khi MicroSIP đổ chuông:

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

## Nghe theo từng miền

`Invoke-FreeSoftphoneCall.ps1 -Region North|Central|South` tạo một đơn fake giao tới miền đó, để
nghe đúng giọng VieNeu của miền. Mỗi cuộc gọi phải được trả lời và bấm `1` hoặc `0` để kiểm
playback không làm hỏng DTMF. Acceptance chỉ thuộc software lab; `REAL_CUSTOMER_CALL_ALLOWED=NO`
giữ nguyên.

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
