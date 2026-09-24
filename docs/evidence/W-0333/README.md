# W-0333 — Bộ đo VieNeu cho Ubuntu S5

Ngày `2026-09-21`, baseline `185548c`; checkout dùng chung, không commit/push hoặc đổi nhánh.
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.** Không yêu cầu nghe lại.

**Cập nhật mới nhất:** đã nhận và đối chiếu file gốc từ vps61. Xem
[phân tích S5](s5-findings.md) và [metadata/hash](s5-target-results.json).
2CPU/4GiB/capacity1: 5/18 phần vượt5s; burst5s có6 timeout, burst10s có6 thành công;
cả hai nhóm có8 request bị503. Sau client disconnect, model còn bận8871ms.
25 PCM thành công khớp bản duyệt. Chưa đóng S5/production; ưu tiên kiểm retry/điều phối
worker, không chỉ tăng quota. Các mục NOT_RUN bên dưới là snapshot trước khi nhận gói.

## Thông tin owner đã cung cấp

Target dự kiến `vps61`: Ubuntu22.04.4, x86_64, 10vCPU Xeon Silver4216, Hyper-V;
RAM Docker báo14669688832 bytes (khoảng13.7GiB), khoảng11GiB available theo `free`.
Disk root còn khoảng402GB. Có bảy container nghiệp vụ đang chạy.
Đây là output owner gửi trong chat, agent chưa truy cập server hoặc đo tải tại đó.

Snapshot `docker stats` sau đó ghi giới hạn47.84GiB/container, khác tổng RAM đã báo.
Chưa kết luận do daemon khác, cấu hình giới hạn container hay snapshot khác thời điểm.
Launcher thu tên host/daemon/context, tổng RAM và giới hạn riêng của từng container để đối chiếu.
Vị trí code/model và số cuộc gọi đồng thời mong muốn vẫn chưa có câu trả lời.
Bundle tự đủ thành phần nên không cần đoán các thư mục đó để chuẩn bị công cụ.

## Đã chuẩn bị

- `deploy/lab/run-vieneu-s5.py`: Python chuẩn, chạy Ubuntu; mặc định chỉ thu inventory.
- Với `--run --expected-host vps61`: kiểm local Unix Docker context, host/daemon đúng tên,
  RAM khớp, headroom, checksum tất cả file rồi mới nạp image nếu còn thiếu.
- Quota chẩn đoán ban đầu2CPU/4GiB, không swap, pids256, capacity1/thread1. Không coi
  quota này là cấu hình production được duyệt. Người chạy có thể chọn trong phạm vi1–4CPU/2–4GiB.
- Container riêng, network none, ghim UID người chạy, rootfs/model/script read-only,
  không mount Docker socket, không publish port, không có telephony/audio output.
- Chỉ tự xóa container riêng; không dừng/xóa container hay volume khác. Kết quả lỗi được giữ.
- Image archive đã ghim từ W-0326, giữ đúng bytes/scan cũ; kiểm cả OCI index và config digest
  để hỗ trợ cách Docker Desktop và engine Linux báo image ID khác nhau. Không build image mới.

Gói `.artifacts/W-0333/vieneu-s5-ubuntu.tar.gz` khoảng247MiB, kèm file `.sha256`, gồm
22 file đã kê checksum (model, image, fixture, voice manifest, script và notices).
Không chứa `.env`, credential, DB khách hoặc backup SIP. Nguồn và hash đầy đủ trong
[local-results.json](local-results.json). Hướng dẫn owner: [ubuntu-steps.md](ubuntu-steps.md).

## Kiểm chứng local trước bàn giao, chưa phải S5

8 guard tests PASS: sai host/daemon, chênh RAM, quota/headroom, context remote,
DOCKER_HOST mâu thuẫn, bundle bị sửa, và container không có mạng/socket/cổng công khai.
Hai lần đầu sandbox Windows không cleanup được thư mục tạm; chạy cùng tests ngoài sandbox
đạt8/8. Không sửa assertion để vượt lỗi. Python compile và checksum bundle PASS.

Model thật chạy trong container local với chính file của bundle và quota2CPU/4GiB:
18 phần động + burst1/2/4 client + disconnect/recovery; toàn bộ assertion PASS,
30 PCM thành công khớp bản đã duyệt. Cgroup thực tế `cpu.max=200000 100000`, memory4GiB.
Chạy bằng launcher PowerShell local có cùng tham số container; **chưa chạy launcher đầy đủ
trên Ubuntu thật**, không gắn socket Docker host vào container để giả lập máy S5.
Host local còn hoạt động khác/đóng gói file; số đo không dùng để chứng nhận capacity của vps61.

Probe local tuần tự p50=1158ms, p95/max=5802ms; một phần vượt5s. RSS đỉnh1583628KiB.
Đây là kiểm công cụ với model thật, không đổi kết luận timeout production vẫn chưa đạt.
Launcher không chứa client retry C# W-0331, không gọi SIP, không thay kiểm end-to-end.

GitNexus impact launcher mới: UNKNOWN/not indexed; source chỉ có entrypoint lab mới.
Không sửa symbol production, probe HTTP cũ, model hoặc image. Detect-changes là advisory
trên toàn WIP dùng chung, không chứng nhận riêng candidate hay hosted CI.

## Việc tiếp theo

Owner đã chạy và gửi đủ kết quả lượt đầu. Tiếp theo kiểm worker retry/điều phối với
khoảng bận thực tế, rồi đo đủ đơn, cold/warm nhiều lần và tải kéo dài trên S5.
Mirror và S2 pháp lý vẫn mở. Gọi khách thật tiếp tục tắt.

### Sửa file checksum sau khi owner chuyển gói

Owner đã chuyển đủ hai file lên server. Ubuntu đọc ký tự CR trong checksum CRLF thành
một phần tên archive nên phép đo chưa chạy. Đã sửa checksum và builder sang ASCII/LF;
archive, model và image giữ nguyên. Hướng dẫn sửa tại chỗ trong `ubuntu-steps.md`.
Hash của file đã sửa được kiểm bằng Linux `sha256sum`: PASS. Reader Linux local cũng
chấp nhận bản CRLF cũ, nên không nhận lượt local là tái hiện lỗi Ubuntu của owner.
Chi tiết [checksum-fix.json](checksum-fix.json); source builder cũ được giữ riêng đúng hash.

### Owner báo hoàn tất lượt S5 đầu tiên

Sau sửa checksum, owner gửi `S5_MEASUREMENT_COMPLETE` từ vps61 với quota2CPU/4GiB.
18 phần động tuần tự: p50=1920ms, p95/max=9272ms, 5 phần vượt5s và 0 phần vượt10s.
Đây là 18 phần tạo tiếng, không phải 18 cuộc gọi. Budget5s không bao phủ hết các mẫu
đã đo; chưa chọn10s làm budget production chỉ từ tóm tắt này.

Đã lưu [tóm tắt do owner gửi](owner-reported-s5-summary.json). Ở thời điểm chỉ có tóm tắt, chưa nhận `inventory.json`,
`host.json`, `load.json` và log gốc; chưa đối chiếu image/hash/quota, tải đồng thời,
thời gian trả capacity hoặc nguyên nhân chênh RAM. Kết quả local và snapshot NOT_RUN
ở các JSON cũ giữ nguyên theo thời điểm thu. Bước tiếp theo là nhận `s5-result.tar.gz`
từ thư mục đo của owner rồi phân tích đầy đủ. Bước này nay đã hoàn tất ở addendum S5
đầu tài liệu; giữ tóm tắt lịch sử theo thời điểm nhận. Khách thật vẫn tắt.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Bằng chứng chạy lab: Bộ đo VieNeu cho Ubuntu S5 và kết quả đo thật trên vps61; chỉ thêm launcher lab cùng test Python chạy ngoài CI, không đổi mã ứng dụng, test .NET hay script gate, nên bằng chứng là số đo và hash máy đích. Danh sách 9 tệp nằm trong khai báo.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0333 chuyển **EVIDENCE_SUBMITTED → ACCEPTED** cho phần đã làm. Đo S5
và toàn luồng làm ở W-0343 và W-0344. Claude chọn việc theo tiêu chí phiếu W-0348, không tự cấp phê
duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
