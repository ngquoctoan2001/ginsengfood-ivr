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

## Chạy lab

Từ repository root:

```powershell
.\deploy\lab\Start-FreeSoftphoneLab.ps1
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
