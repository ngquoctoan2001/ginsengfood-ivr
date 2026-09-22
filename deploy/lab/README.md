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
- Node.js 22 trở lên để chạy kiểm tra VieNeu/media trước cuộc gọi.
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
sidecar, kiểm tra VieNeu/media rồi tải/mở MicroSIP với account `LAB-A`. Script **không tự gửi task gọi**; sau khi MicroSIP
hiện `Online`, chạy:

```powershell
.\deploy\lab\Invoke-FreeSoftphoneCall.ps1
```

Nếu cần đúng hành vi một-lệnh cũ cho một lượt preflight có giám sát, dùng
`Start-FreeSoftphoneLab.ps1 -InvokePreflightCall`.

`Start-FreeSoftphoneLab.ps1` và mỗi lần gọi `Invoke-FreeSoftphoneCall.ps1` đều chạy
`check-speech-preflight.mjs` trước khi mở UI/gửi task. Kiểm tra yêu cầu các container đúng lab,
`LAB_REAL_SIM`/`NO`, backend VieNeu thật, chung network namespace với worker; chờ readiness `200`
tối đa 180 giây (đổi bằng `-SpeechReadyTimeoutSeconds`, từ 1 đến 600).
Sau đó helper cô lập, không mạng, dùng image TTS đang chạy và UID/GID worker `1654:1654` để ghi
file ngẫu nhiên lên đúng volume chung; Asterisk phải đọc đúng hash và không ghi được. File probe
được dọn cả khi kiểm thất bại. Mỗi lệnh Docker có timeout riêng; không phải chờ vô hạn.
Mount helper dùng `volume-nocopy` để không chép quyền root từ image vào volume còn rỗng.
Đây là kiểm quyền filesystem với cùng UID/GID, chưa chứng minh worker tổng hợp/phát tiếng đúng.
Thiếu bất kỳ điều kiện nào thì script dừng trước gửi task; nghe mối nối/DTMF vẫn là nghiệm thu riêng.

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

## Profile nghiệm thu toàn lời thoại (W-0321)

Mặc định overlay VieNeu vẫn tắt segmentation, nhưng dùng timeout `60000 ms` cho toàn câu và
giới hạn các pool xử lý ở một thread theo phép đo W-0321. API dùng `UNSELECTED` vì chỉ worker
tổng hợp audio. Profile nghiệm thu dưới đây là lựa chọn **lab**
tường minh, dùng image ID đã build/kiểm; không tự bật trong production:

```powershell
$ttsImage = (docker image inspect --format '{{.Id}}' <tag-image-đã-kiểm>).Trim()
node deploy/lab/speech-lab-profile.mjs segmented $ttsImage .artifacts/lab-segmented.json
node deploy/lab/speech-lab-profile.mjs whole $ttsImage .artifacts/lab-whole.json
```

Nạp đúng một file vừa tạo sau ba compose file dev, softphone, vieneu-tts, với cùng môi trường
model/voice/ARI/SIP của lab. Khi đổi profile, recreate cả `ivr-worker` và `ivr-tts` vì sidecar
dùng network namespace của worker. Chạy lại `check-speech-preflight.mjs` trước gửi task.

Profile `segmented` ghép catalog 12 đoạn với ba giá trị đơn, timeout `5000 ms` mỗi request.
Profile `whole` là rollback lab, timeout `60000 ms` cho toàn câu. Cả hai giới hạn ONNX, BLAS,
OpenMP và MKL ở một thread; đây là cấu hình đã đo local, chưa phải sizing máy S5.
Phép đo và giới hạn xem [W-0321](../../docs/evidence/W-0321/README.md).

```powershell
.\deploy\lab\Invoke-FreeSoftphoneCall.ps1 -Region North -OrderVariant A
.\deploy\lab\Invoke-FreeSoftphoneCall.ps1 -Region North -OrderVariant B
```

Lặp lại cho `Central` và `South`: A có 2 hộp / 560.000 đồng, B có 3 hộp / 735.000 đồng.
`-NoUi` cho phép điều khiển MicroSIP bằng CLI khi chạy automation; kiểm readiness/media/SIP
vẫn bắt buộc. Nghe hết lời thoại rồi thử phím `1`/`0`; automated PASS không thay chữ ký nghe duyệt.

W-0323 bổ sung `-OrderVariant MultiItem` (ba món, số lượng 2/3/1) và `LongName`
(một tên món dài, số lượng 4). Dữ liệu synthetic nằm trong `fake-orders.json`; worker vẫn dùng
renderer thật. Sáu ca mở rộng dùng image Chainguard và overlay lab riêng nâng timeout mỗi phần
động lên `10000 ms`, vì giọng Nam nhiều món vượt `5000 ms`. Không tự áp dụng mức này cho production.
Xem [W-0323](../../docs/evidence/W-0323/README.md) để lấy kết quả và cách lặp phép đo.

## DTMF tự động và đo tải không cần nghe (W-0329)

Owner đã chốt ba giọng và câu ghép tại W-0323. Các lượt sau kiểm kỹ thuật bằng đầu SIP
không có thiết bị âm thanh; không yêu cầu nghe lại. Khi stack lab đang rảnh:

```powershell
python -X utf8 deploy/lab/run-headless-dtmf.py --out .artifacts/dtmf-new-run --measurements .artifacts/W-0326/final-audio/measurement.json --texts .artifacts/W-0323/worker-texts.json
```

Harness giữ bí danh `LAB-A`, tạm đổi AOR outbound sang peer Asterisk nội bộ cùng image,
gửi phím bằng RFC4733 sau đủ thời lượng audio và khôi phục file PJSIP ban đầu cùng checksum.
Không đổi đăng ký MicroSIP hoặc code worker. Sáu ca Bắc/Trung/Nam × nhiều món/tên dài phải khớp
phím, voice, bảy media, RTP và kết quả cuối; thêm ca không gửi phím. Chỉ chạy khi không có
job/cuộc gọi đang dở. Thư mục output phải mới và ở `.artifacts`; backup cấu hình có credential,
giữ local/ignored, không đính kèm báo cáo hoặc commit.

Đo tải VieNeu độc lập với telephony, không network và không phát/lưu audio:

```powershell
.\deploy\lab\Measure-VieNeuLoad.ps1 -Image $exactImageId -ModelBundle $modelPath -VoiceManifest $manifestPath -WorkerTexts $syntheticTextsPath -OutputDirectory .artifacts/load-new-run
```

Mặc định ghi `LOCAL_LAB`. Chỉ dùng `-EvidenceScope S5_TARGET -OwnerConfirmedS5` trên máy Owner
đã xác nhận là S5. `-CpuLimit`/`-MemoryGiB` mặc định 0 (không đặt quota riêng); truyền đúng giới
hạn triển khai khi đo máy đích. Probe ghi CPU/RAM host, tài nguyên Docker/cgroup, image ID,
latency và HTTP200/503/timeout theo các burst 1/2/4 client với ngân sách 5/10 giây, rồi thử
client ngắt giữa inference và phục hồi. HTTP503 là từ chối khi quá tải, không phải synthesize
thành công. Kết quả local không tự đóng S5 hoặc mở gọi khách thật.

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

## Đo chuẩn bị lời thoại trên S5, không gọi điện — W-0335

`run-vieneu-worker-s5.py` chạy renderer và dịch vụ speech .NET với đơn giả, kiểm đủ bảy
phần audio, cache lạnh/ấm, 2/4 đơn đồng thời và tải liên tục. Hướng dẫn gói offline:
[W-0335](../../docs/evidence/W-0335/ubuntu-steps.md). TTS giữ 2 CPU/4 GiB; probe .NET riêng
thêm tối đa 0.5 CPU/512 MiB. Mọi kết quả local và target được ghi scope riêng.
Các timeout 10/15/30 giây trong probe là chẩn đoán, không đổi cấu hình vận hành.
