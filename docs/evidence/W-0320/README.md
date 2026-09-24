# W-0320 — VieNeu lab preflight và model thật trên hai image

Ngày `2026-09-21`; baseline `main@e33927d03257492de93727fbdcab4b33e6f3f25e`.
Phạm vi: thay đổi local chưa commit, bảo toàn WIP `W-0319`; không phải gói nghiệm thu exact-commit.
Cuối lượt, phiên W-0319 đã commit thành `d7a3bd4`; W-0320 vẫn chưa commit. Không gán các phép
thử đã chạy cho SHA mới này; JSON ràng vào source bytes và image IDs đã thực sự dùng.

`REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Kết quả và giới hạn

Đã chặn hai lối vào lab khi VieNeu/media chưa sẵn sàng; đã tải và xác minh bundle nonprod
`13` file / `210763608` byte. Hai image Debian đã vá và Chainguard đều sinh được audio thật:
`12/12` request mỗi image; checksum tương ứng giống nhau `12/12`. Đây là `24` HTTP syntheses
với hai văn bản giả, **không phải 24 cuộc gọi hoặc nghiệm thu 12 đoạn cố định**.

Production còn chặn `LEGAL,INTERNAL_MIRROR`. Chưa đổi Dockerfile mặc định, chữ ký, segmentation
hoặc timeout. Chưa gọi MicroSIP, chưa nghe/duyệt mối nối, chưa đo máy đích, chưa quét Trivy mới.

## 2. Thay đổi code

- [check-speech-preflight.mjs](../../../deploy/lab/check-speech-preflight.mjs) dùng chung cho
  [Start](../../../deploy/lab/Start-FreeSoftphoneLab.ps1) và
  [Invoke](../../../deploy/lab/Invoke-FreeSoftphoneCall.ps1), trước mở UI/gửi task.
- Yêu cầu container đang chạy đúng project/service; worker và TTS có `LAB_REAL_SIM`/`NO`;
  backend `vieneu-onnx`, sidecar cùng network namespace worker; worker UID/GID `1654:1654`.
- Chờ readiness `200` theo đồng hồ monotonic, mặc định `180s`, giới hạn cấu hình `1..600s`;
  từng lệnh Docker có timeout. Không in môi trường container/credential.
- Kiểm worker RW và Asterisk RO trên cùng named volume. Helper không mạng, cùng UID/GID,
  dùng đúng image TTS đang chạy, ghi probe ngẫu nhiên; Asterisk đọc hash và phải bị từ chối ghi.
  Cleanup chỉ chạm tên container/file ngẫu nhiên do lượt probe tạo, kể cả khi lỗi.
- Docker thử thật phát hiện cần `volume-nocopy`: volume rỗng đã có owner `1654:1654`, mode `0750`
  vẫn bị probe từ chối ghi khi Docker copy metadata từ image. Trước sửa đỏ `PermissionError`;
  sau sửa xanh, owner/mode giữ nguyên, không còn file probe. Regression giữ điều kiện này.
- Thêm selftest, thực thi chính hai script PowerShell bằng stub để kiểm điểm dừng;
  nối vào `tts.gitlab-ci.yml` và `gate-invocations.json`.
- [smoke_real_model.py](../../../deploy/tts/tests/smoke_real_model.py) là smoke opt-in, không nằm
  trong discovery `test_*.py`; khởi tạo backend thật rồi gọi HTTP loopback, kiểm PCM và ghi metadata.

GitNexus impact ban đầu trên hai script LOW, `0` process. Hàm mới `checkSpeechPreflight` chưa có
trong index (`UNKNOWN`), nên đã kiểm caller trực tiếp bằng source: hai entry script và selftest.
Detect-changes trên tracked diff chung: LOW, `0` process; có cả WIP W-0319, không coi đó là thống kê
riêng W-0320 hoặc bằng chứng bao phủ các file mới chưa được index.

## 3. Bằng chứng đã chạy

| Kiểm tra | Kết quả |
| --- | --- |
| Preflight selftest | PASS: `1` valid, `17` refusal, `4` entry-script cases; chạy Windows và PowerShell image ghim |
| Gate runner sau bản sửa cuối | `GATE_SWEEP_PASS 1/1 run, 22 skipped by manifest`; chỉ gate preflight, không phải full sweep |
| Docker volume fixture | PASS readiness thật, ghi/đọc checksum, RO denial, owner/mode bất biến, `probe_files=0` |
| Lab hiện có đang dừng | Đúng `LAB_SPEECH_CONTAINER_NOT_READY`; không gửi task |
| Bundle verifier nonprod | PASS `13/13`; hash/size theo MODELS.lock |
| Bundle verifier production | Từ chối đúng, exit `1`; blocker LEGAL/INTERNAL_MIRROR còn mở |
| Image source/lock binding | `45/45` file mỗi image khớp từng byte với workspace |
| CI config / docs selftest | PASS; docs chạy lại ngoài sandbox vì Node child process ban đầu bị EPERM |
| Kiểm cuối | Gate-status PASS; PII scan PASS `393` text files / `2` binary skipped; diff whitespace PASS; `10/10` source hashes vẫn khớp |

Volume fixture dùng container thay vai worker/Asterisk để kiểm lệnh Docker thực tế; TTS nạp model
thật. Nó không chạy .NET worker, Asterisk ARI hay cuộc gọi. Bundle và mọi mount input đều read-only;
hai smoke image chạy `--network none`, `--read-only`, UID/GID image `1654:1654`, không publish port.

| Phép đo local, chạy tuần tự | Debian đã vá | Chainguard thử nghiệm |
| --- | --- | --- |
| Image ID prefix | `3d679172e034` | `0d3c1b95ee78` |
| Python | `3.12.14` | `3.14.7` |
| Nạp model + startup smoke | `6197 ms` | `8658 ms` |
| Câu tiền, 3 giọng × 2 lượt | `2644–4525 ms` | `979–4925 ms` |
| Câu đơn giả, 3 giọng × 2 lượt | `6295–11847 ms` | `7135–19840 ms` |
| HTTP/PCM hợp lệ | `12/12` | `12/12` |
| Request vượt `5000 ms` | `6/12` | `6/12` |

Smoke gửi hai văn bản tổng hợp, không dùng payload đầy đủ của worker. PCM headerless mono 16-bit
`8000 Hz`, HTTP `audio/L16`, không rỗng/không toàn zero; checksum các lần lặp và giữa hai image khớp.
Máy Windows dùng Docker Desktop, không cô lập tải nền hoặc ghim CPU/RAM: số đo này chỉ cảnh báo
ngân sách thời gian, không xếp hạng hiệu năng hai image và không chứng nhận S5.

Kết quả số, full image IDs, từng request/checksum, source/log hashes:
[local-results.json](local-results.json). Hash source là byte workspace lúc chạy, không phải commit mới.
Hash log dùng SHA-256 mã hóa base64: một digest hex bị scanner nhận nhầm thành số điện thoại;
đổi cách biểu diễn digest, giữ nguyên log và quy tắc scanner.
Log thô và script fixture/summarize: `.artifacts/W-0320/` (thư mục cục bộ trên máy chạy, không commit vì `.artifacts/` nằm trong `.gitignore`).
Lượt đầu Chainguard dùng nhầm trường `regions` thay vì `selections`, bị readiness từ chối;
đã giữ log riêng rồi chạy lại đúng allowlist. Không tính lượt sai tham số vào `24` syntheses PASS.

## 4. Lặp lại smoke model thật

Từ repo root trong PowerShell, sau `verify-model.py --mode nonprod`:

```powershell
$repoRoot = (Get-Location).Path
$accepted = Get-Content docs/evidence/W-0122/voice-acceptance-manifest.json -Raw | ConvertFrom-Json
$voices = ($accepted.selections.North.voice_id, $accepted.selections.Central.voice_id, $accepted.selections.South.voice_id) -join ','
$image = 'sha256:3d679172e034bd5df20e3f8ea2a357c6365a6c14555eb4be8fb7f46864e7fc46'
docker run --rm --network none --read-only --cap-drop ALL --security-opt no-new-privileges:true `
  --tmpfs /tmp:rw,noexec,nosuid,nodev,size=64m,uid=1654,gid=1654 `
  --mount "type=bind,src=$repoRoot/artifacts/w-0122-models,dst=/models,readonly" `
  --mount "type=bind,src=$repoRoot/docs/evidence/W-0122/voice-acceptance-manifest.json,dst=/run/ivr-tts/voice-acceptance-manifest.json,readonly" `
  --mount "type=bind,src=$repoRoot/deploy/tts/tests/smoke_real_model.py,dst=/smoke.py,readonly" `
  --env IVR_EXECUTION_MODE=LAB_REAL_SIM --env REAL_CUSTOMER_CALL_ALLOWED=NO `
  --env VIE_NEU_AUDITION_MODE=0 --env "VIE_NEU_ALLOWED_VOICE_IDS=$voices" `
  --entrypoint python $image /smoke.py
```

Lặp với Chainguard ID trong JSON; image phải có sẵn hoặc build lại theo Dockerfile trial W-0317.
Mỗi lần build khác phải ghi ID mới, kiểm binding lại; không lấy số CVE ngày 18/09 gán cho image mới.

## 5. Việc tiếp theo

1. **Xử lý ngân sách TTS trước sáu cuộc gọi lab.** Worker hiện đặt `TimeoutMilliseconds=5000`
   trong `src/Ivr.Worker/appsettings.json`; các compose lab không override. Cả `12/12` lượt câu
   đơn giả trên hai image đều vượt mức đó. Đây là nguy cơ timeout suy ra từ phép đo sidecar và
   source, chưa phải lỗi .NET worker đã tái hiện. Đo toàn câu thật của worker; đánh giá segmentation
   và cấu hình lab có giới hạn, giữ tiêu chí production riêng, không chỉ tăng timeout để gọi PASS.
2. Nghiệm thu `2` đơn giả × `3` miền, nghe `12` đoạn cố định và mối nối, DTMF/media/rollback.
   Ba giọng đã được Owner ký; S1 về đoạn cố định/mối nối vẫn cần người nghe.
3. Chọn image với S2, quét lại **đúng image candidate**, hoàn tất Legal và mirror S5; bản thử
   Chainguard đã vượt qua trở ngại chưa có inference thật, chưa thành lựa chọn production.
4. Đo target hardware, provision fixed WAV vào media sink; nối production verifier/chữ ký/measurement
   vào gate phát hành theo F1/F2 của [báo cáo review](../../reports/21-09-ra-soat-vieneu-va-buoc-tiep-theo.md).
5. Sau commit phù hợp và checkout sạch, thu toàn solution/full sweep theo W-0319; kết quả local
   của W-0320 không thay bộ nghiệm thu đó. S1/S2/S5/S6 và quyền phát hành vẫn thuộc người phụ trách.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Gate trong full sweep có self-test kiểm thay đổi của việc này: `lab-speech-preflight-selftest.mjs`.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0320 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. S5, mirror và
pháp lý làm ở W-0340…W-0344; timeout theo dõi tiếp ở W-0335. Claude chọn việc theo tiêu chí phiếu
W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
