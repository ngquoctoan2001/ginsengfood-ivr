# W-0344 — sửa Asterisk không khởi động trên S5, và bộ kiểm bấm phím quá sớm

`REAL_CUSTOMER_CALL_ALLOWED=NO`

**S5 chưa đạt.** Lượt `cpu-r2` đầu tiên trên S5 (23/09, `093745-1c9bd160`) cho thấy bản sửa Asterisk
đã có tác dụng, nhưng sidecar TTS không đọc được model: mirror chỉ `ssv` đọc được, còn TTS chạy
bằng uid 1654 (mục 6). Installer đã sửa để chép model sang bản đọc được; chờ chạy lại.
Gói `cpu-r1` (22/09 14:06) chưa từng gửi lên S5 và đã bị thay: installer bị sửa sau khi ghim
hash nên script retry tự dừng ở bước kiểm checksum, và lượt diễn tập thứ hai của nó hỏng
(mục 2). Toàn bộ hash và kết quả nằm ở [cpu-retry-verification.json](cpu-retry-verification.json).

## 1. Asterisk exit 132 trên S5

Lần `135115-e0b16d79` dừng ở Asterisk exit 132, trước khởi động API/worker/TTS và trước bất
kỳ đơn kiểm nào. [Receipt đã kiểm](s5-first-run.json): đủ 26 hash, 13 model/card khớp,
43 container có sẵn không đổi. Dọn 10 container và các network của lượt thất bại, giữ 2 volume.

Dockerfile trước đây để Asterisk bật `BUILD_NATIVE`, biên dịch cho CPU máy build i7-11700,
còn S5 là Xeon Silver 4216. Upstream xác nhận chế độ này thêm `-march=native` và khuyến nghị
tắt khi chuyển binary sang máy khác:
[hướng dẫn chính thức](https://docs.asterisk.org/Getting-Started/Installing-Asterisk/Installing-Asterisk-From-Source/Building-and-Installing-Asterisk/),
[Makefile.rules đúng 22.10.1](https://github.com/asterisk/asterisk/blob/22.10.1/Makefile.rules#L132).
[Dockerfile](../../../deploy/lab/asterisk/Dockerfile) đã tắt `BUILD_NATIVE`, đặt
`-march=x86-64 -mtune=generic`, giữ version 22.10.1 và SHA tarball cũ. 690 lệnh compile mang
baseline generic, 0 lệnh `-march=native`. Image cũ tái hiện `Illegal instruction`/132 trên
QEMU Nehalem, image mới tới `Asterisk Ready`. Đây là chẩn đoán ISA, không phải đo tải trên S5.

## 2. Diễn tập r2 của cpu-r1 tìm ra lỗi bộ kiểm

Lượt diễn tập local đầu tiên (`rehearsal`, 14:06) qua 7/7 ca nhưng tổng kết FAIL, vì container
của một job CI GitLab Runner có sẵn trên máy kết thúc giữa lượt. Lượt thứ hai (`rehearsal-r2`,
14:17) chỉ qua 2/7: ca `technical-retry` nhận **1196** gói RTP trong khi cần **1280**, tức thiếu
khoảng 1,7 giây tiếng lúc bấm phím.

Không thiếu tiếng: 0 gói mất. Tiếng **đến muộn**. Cùng file, cùng máy:

| Ca `technical-retry` | Đoạn 1 | Đoạn 2 | Bốn đoạn sau | Bấm phím sau trả lời |
| --- | --- | --- | --- | --- |
| `rehearsal` (đạt) | 7,22 s | 7,07 s | 0,88 · 2,42 · 1,14 · 2,26 s | 28,09 s — đủ 1280 gói |
| `rehearsal-r2` (hỏng) | **9,80 s** | **9,36 s** | 0,88 · 2,40 · 1,12 · 2,24 s | 28,12 s — mới 1196 gói |

Worker gửi cả 7 đoạn trong **một** lệnh ARI play, nên khoảng chậm nằm trong Asterisk, không ở
worker. Lượt `rehearsal` có đúng chuỗi lỗi TTS cố ý như `rehearsal-r2` mà không chậm. Khác biệt
duy nhất ghi nhận được: trong `rehearsal-r2` có một bộ test Testcontainers khác chạy trên cùng
Docker (3 container có trong ảnh chụp trước và mất ở ảnh chụp sau). Asterisk chỉ được cấp
**0,5 CPU**, nên tụt khỏi thời gian thực khoảng 4,9 giây trong hai đoạn đầu.

Bộ kiểm cũ bấm DTMF khi *đồng hồ* tính từ lúc nghe máy vượt `độ dài tiếng + 2 s`, nên bấm lúc
tiếng còn đang phát. IVR vẫn nhận phím và ghi `IVR_CONFIRMED` đúng thiết kế; assertion cuối cùng
(`số gói RTP ≥ độ dài tiếng / 20 ms`) bắt được, và nó bắt đúng. **Đây là lỗi của bộ kiểm, không
phải của IVR.**

**Sửa trong [cases.py](../../../deploy/lab/full-flow-s5/cases.py)** (Codex soạn 22/09 14:28,
hoàn tất 23/09): mốc thời gian chỉ còn là cận dưới. Bộ kiểm đọc `pjsip show channelstats` và chỉ
bấm khi số gói đã nhận đạt cùng ngưỡng mà assertion cuối dùng. Vòng chờ vẫn nằm trong giới hạn
360 giây mỗi ca. Diff −2/+7 dòng; **51/51 dòng assert giữ nguyên**. Test hồi quy
[test_full_flow_dtmf_gate.py](../../../deploy/lab/tests/test_full_flow_dtmf_gate.py) đỏ với
`cases.py` của gói S5 gốc, xanh với bản mới.

Quan sát cho production, **không phải cổng của W-0344**: với 0,5 CPU, Asterisk có thể phát chậm
hơn thời gian thực khi máy bị tranh CPU, và người nghe có thể gặp khoảng lặng giữa câu. Receipt S5
sẽ được đo cùng dòng thời gian này.

## 3. Gói cpu-r2

| | |
| --- | --- |
| Thành phần | `patch.json`, `manifest.json`, `cases.py`, `images/asterisk.tar` |
| Image Asterisk | **cùng byte** với cpu-r1, chép từ archive r1 đã ghim |
| So với gói gốc đã có trên S5 | chỉ khác image Asterisk và `cases.py`. API/worker/migration, TTS, model, lời thoại, 7 ca, quota 30/90/120 và launcher giữ nguyên byte |
| Installer | chỉ chấp nhận đúng hai thay đổi đó; từ chối đổi image ứng dụng, guard, launcher hoặc bật gọi khách thật |
| Dựng / kiểm | [build-cpu-retry.py](build-cpu-retry.py) · [verify-cpu-retry.py](verify-cpu-retry.py) · [pin](cpu-retry-pins.json) · [manifest](cpu-retry-manifest.json) |

**Script retry chạy tách khỏi phiên SSH.** Hai lần kết nối tới S5 ngày 22/09 đều bị đóng giữa
chừng (exec 255 và SCP bị đóng). Trước đây installer chạy thẳng trong phiên SSH nên rớt kết nối là
giết bài kiểm. Nay bài kiểm chạy bằng `setsid`, SSH chỉ theo dõi log. Nếu rớt kết nối, script in
lệnh `-ResumeRunId`: lệnh này chỉ chờ lượt đó kết thúc rồi tải receipt, **không chạy lại**. Lượt đã
chết mà chưa có kết quả được báo `W0344_RETRY_INTERRUPTED`, không chờ vô hạn.
[test-retry-detach.py](test-retry-detach.py) chạy chính đoạn bash của script trong container
Debian với installer giả, qua 8 kiểm tra: chạy thường, installer lỗi, SIGHUP rồi SIGKILL cả nhóm
phiên SSH giả lập rồi resume, tiến trình bị giết, chạy trùng thư mục, sai hash.

## 4. Kiểm chứng trước bàn giao

| Kiểm | Kết quả |
| --- | --- |
| Diễn tập local với `cpu-r2` (`rehearsal-1`, 23/09) | **PASS**: 7/7 ca. DB đúng 7 task, 7 attempt, 4 lượt khách, 3 lỗi kỹ thuật, 9 kết quả, 6 kết quả cuối, 6 callback; 0 đếm nhầm hay trùng. Container có sẵn không đổi. Dọn 11 container và network, giữ 2 volume |
| Cổng chờ RTP trong lượt đó | không phải giữ lần bấm nào: tiếng phát đúng thời gian thực, mỗi lần bấm đều đã nhận đủ gói |
| Test Python | 41 trên Windows (3.13, 1 test symlink chỉ chạy POSIX) · 30/30 trên Linux (3.14) |
| Test hồi quy DTMF | đỏ với `cases.py` của gói S5 gốc, xanh với bản mới |
| Tách phiên SSH | 8/8 trong container Debian. Đối chứng: script cũ bị ngắt ở giây thứ 3 thì mất cả kết quả lẫn receipt |
| Chuỗi hash | 20/20 file dựng lại khớp manifest; image Asterisk cùng byte r1; gói gốc khớp lượt S5 `135115`; `-VerifyOnly` đạt trên PowerShell 5.1 và 7 |

Đây là kết quả **LOCAL_REHEARSAL**, chưa phải S5.

## 5. Chạy lại trên S5

**Chỉ chạy sau khi W-0343 đã trả receipt**; hai bài không được chạy cùng lúc trên S5.
Gói vá khoảng 132 MiB, dùng lại gói gốc đã có ở
`/home/ssv/ivr-full-flow-w0344-20260922-135115-e0b16d79/full-flow-s5` sau khi kiểm hash.
Installer tạo bản sao mới, database/network mới; không sửa hoặc chạy đè lượt cũ.

```powershell
& 'C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0344\retry-s5-full-flow.ps1'
```

Từ 23/09 máy Windows đăng nhập S5 bằng SSH key, nên script không còn hỏi mật khẩu. Thiếu key thì
script hỏi khoảng 4 lần; nhập trong terminal, không gửi vào chat. Script in `RunId`, theo dõi log
khoảng 10 phút, rồi kéo receipt về `.artifacts/W-0344/cpu-portable-r2/s5-<RunId>/`, kể cả khi lỗi.
Nếu rớt SSH, chạy đúng lệnh `-ResumeRunId` mà script in ra.

**Còn phải làm:** chạy lại retry; kiểm receipt mới (SIP/RTP/DTMF, đối soát DB, dòng thời gian
phát tiếng, bảo toàn dịch vụ); Toàn nghiệm thu sau khi S5 thực sự đạt. Chưa chuyển `ACCEPTED`.
Không xác nhận production, M3, SIM thật hay nhiều worker.

## 6. Lượt S5 23/09 `093745-1c9bd160`: Asterisk chạy, TTS không đọc được model

Receipt (`.artifacts/W-0344/cpu-portable-r2/s5-20260923-093745-1c9bd160/`) hash khớp. Bản vá dựng
đúng (`W0344_CPU_PATCH_VERIFIED`), image Asterisk portable load được và **Asterisk lên `Healthy`
trên Xeon Silver 4216**, nên lỗi exit 132 đã hết. API, worker và TTS đều khởi động, nhưng sau 180
giây chờ, launcher dừng với `Isolated stack failed readiness`. Log TTS chỉ có một dòng, in ngay
lúc khởi động: `tts_event=startup status=not_ready`. Chưa có đơn kiểm nào chạy. Launcher dọn
container của lượt đó và giữ volume.

**Nguyên nhân** (đọc trên S5 qua SSH, không thay đổi gì): mirror
`/home/ssv/ivr-artifact-mirror/releases/vieneu-w0340` có thư mục `700` và cả 13/13 file `600`,
chủ là `ssv` (uid 1000). Image TTS `79e910…` chạy bằng user **1654**, nên không đọc được model qua
bind mount. Launcher kiểm hash model bằng quyền của `ssv` nên không bắt được. Các lượt đo S5
trước đó (W-0338, W-0343) chạy TTS bằng `--user` uid của `ssv`, nên không gặp. Docker Desktop trên
Windows bỏ qua quyền Unix của bind mount, nên mọi diễn tập local đều đạt. Đây đúng là loại lỗi
chỉ lộ ra khi chạy trên máy thật.

**Sửa trong installer, không đụng launcher hay mirror:** `stage_models` chép 13 file đã kiểm hash
sang `models-readable/` riêng của lượt chạy (thư mục `755`, file `644`), kiểm hash từng bản chép,
đưa thư mục đó cho launcher, và xóa sau khi chạy xong. TTS vẫn chạy bằng user non-root 1654 như
thiết kế. Launcher, kit gốc, 7 ca và archive `cpu-r2` giữ nguyên byte; chỉ installer đổi, và đã
ghim lại hash.

**Tái hiện có quyền Unix thật** ([test-model-staging.py](test-model-staging.py)): model thật đặt
trong volume Docker theo đúng bố cục mirror S5 (chủ uid 1000, `700`/`600`); cùng image TTS `79e910…`
chạy bằng user 1654. Gắn thẳng mirror ra `status=not_ready`, giống hệt S5. Gắn bản do
`stage_models` chép (thư mục cha `700` như thư mục chạy trên S5) ra `status=ready`. Mirror vẫn
`600`. Test installer: 13/13 trên Linux, 12/13 trên Windows (1 test symlink chỉ chạy trên POSIX).
