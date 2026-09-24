# W-0321 — Toàn lời thoại VieNeu, sáu cuộc gọi lab và S2/S5

Cập nhật kế tiếp: [W-0323](../W-0323/README.md) chọn Chainguard và kiểm đơn nhiều món/tên dài.
Các con số/trạng thái bên dưới là snapshot của W-0321.

Ngày `2026-09-21`. Baseline đầu lượt `main@d7a3bd4`; WIP W-0320 được giữ nguyên.
Phiên W-0319 tiếp tục trên cùng checkout; bằng chứng lượt này ràng vào source bytes và image IDs,
không chứng nhận một commit sạch hay hosted CI. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## Đo lời thoại thật

Harness gọi `VietnameseOrderScriptRenderer` thật với template canonical `v3-test-approved`,
hai đơn giả A/B × ba miền; giữ nguyên toàn câu và bảy segment. Sau đó gọi HTTP shim/model thật,
không chỉ đo một câu tiền ngắn. Artifact local: `.artifacts/W-0321/worker-texts.json`,
`audio-blas1/measure-1.json`, `RendererProbe/`, `measure-worker.py`.

| Miền / đơn | Tổng hợp toàn câu (ms) | Tổng ba phần động (ms) | Audio ghép (ms) |
| --- | ---: | ---: | ---: |
| Bắc A | 15204 | 2982 | 17680 |
| Bắc B | 19650 | 2574 | 16880 |
| Trung A | 15047 | 3099 | 20560 |
| Trung B | 27988 | 2412 | 20240 |
| Nam A | 21704 | 4326 | 27600 |
| Nam B | 39046 | 3291 | 26320 |

Máy local có tải khác, không phải máy S5. Đơn vị trên là thời gian chờ HTTP; tổng phần động
chưa tính overhead worker/cache/filesystem. Mỗi phần động dưới `1654 ms` trong lượt đo này.
Giới hạn một thread cho ONNX **và** OpenBLAS/OpenMP/MKL giúp tránh số thread nhân lên;
chỉ giảm ONNX vẫn từng quan sát CPU `1856.67%` và latency cao. Lượt ONNX-only dừng sau ba ca,
giữ checkpoint và exit `137`, không gọi là phép đo sáu ca PASS.

`speech-lab-profile.mjs` sinh overlay tường minh: segmentation/catalog với `5000 ms` mỗi
request; rollback toàn câu `60000 ms`. Image phải là ID SHA-256; cả hai profile vẫn `LAB_REAL_SIM/NO`.
Production segmentation và timeout không bị đổi. Overlay lab thường giữ segmentation tắt,
nhận timeout toàn câu `60000 ms` và các pool một thread để lần Start kế tiếp dùng được cấu hình đã đo.
`VIE_NEU_ORT_THREADS` chỉ nhận `0..8`,
mặc định `0` giữ hành vi tự chọn của engine; lab chọn `1`.

## Hai lỗi tích hợp phát hiện khi chạy thật

1. API lab dừng vì provider mặc định `FAKE_DETERMINISTIC` không được phép trong LAB. Overlay
   đặt API `UNSELECTED`; worker vẫn là nơi duy nhất gọi VieNeu. API sau sửa readiness `200`.
2. .NET gửi `application/json; charset=utf-8`; shim so sánh nguyên chuỗi nên trả `415` trước
   quay số. Shim giờ đọc media type/charset, vẫn từ chối type khác và charset ngoài UTF-8.
   Container regression mới đỏ trên image cũ đúng `HTTP 415`; kết quả sau vá nằm trong gói local.

Lần startup không tạo task; lần HTTP lỗi tạo technical exception không tính customer attempt.
Không xóa hay sửa các kết quả lỗi để biến thành PASS. GitNexus: backend/handler/config LOW;
đọc source bổ sung vì graph Python không thể hiện đầy đủ caller HTTP động.

## Nghe và thẩm quyền

Bộ nghe `.artifacts/W-0321/listening/index.html`: 12 WAV cố định + 6 câu ghép, từng file có hash
trong `audio-manifest.json`; đã kiểm `18/18`. Câu ghép dùng PCM nối trực tiếp, không phải bản thu
MicroSIP/RTP. Checkbox ở trình duyệt chỉ giúp người nghe theo dõi, không tự ghi chữ ký.

Owner trả lời **“tôi nè”** cho vai trò người nghe và quyết định S2. Đây chưa phải kết quả nghe,
lựa chọn image, chấp nhận rủi ro hoặc phê duyệt production. Phiếu cần chốt:
[owner-decisions.md](owner-decisions.md).

## Cuộc gọi thực tế và rollback

Đã chạy **6 cuộc có kiểm soát**, trên image Debian cuối: Bắc/Trung/Nam × A/B. Mỗi ca A nhận
phím `1` và `IVR_CONFIRMED`; B nhận `0` và `IVR_CUSTOMER_CANCELLED`. Cả sáu kết quả có
`final=true`, `counted=true`, đúng voice ID/miền được lưu tại dispatch, `region_resolved=true`.

Collector đối chiếu database + log Asterisk theo khoảng thời gian riêng của từng task:
`7/7` media mỗi ca đúng thứ tự fixed/dynamic; hash tên media động khớp PCM phép đo; thời gian
active dài hơn audio và RTP có packet gửi/nhận. Automation chỉ gửi DTMF sau audio + `5s`.
Đây là bằng chứng luồng phát và nhận phím; độ tự nhiên/mối nối vẫn cần Owner nghe duyệt.

Rollback: recreate worker/TTS với profile `whole`, cache worker mới; North-A tổng hợp một file,
phát qua MicroSIP, phím `1` ra confirmed. Sau đó khôi phục `segmented`, preflight PASS; cuối lượt
`0 active channels`, `0 active calls`. Không thay default production hoặc mở customer calling.

Trước ma trận có `4` cuộc tương tác tay. Owner xác nhận sẽ chỉ nghe để automation gửi phím;
các cuộc này được giữ riêng, không dùng thay cho sáu ca có kiểm soát. Tổng actual lab calls
của lượt là `11` (`4` tương tác + `6` chính + `1` rollback); thêm `2` attempt technical của
`1` task lỗi HTTP 415, không tính customer attempt. Startup lỗi API chưa tạo task.

Registry `v3-test-approved` có **template text khớp từng ký tự** với canonical đã đo. Registry
lưu hash UTF-8 thuần, fixture renderer tính hash qua `DeterministicSnapshotHasher`; không dùng
việc hai cách hash khác nhau để khẳng định nội dung khác nhau, cũng không sửa registry approval.

## Image cuối và quét lại

| Candidate TTS | Image ID prefix | HIGH | CRITICAL | Có bản vá theo scanner |
| --- | --- | ---: | ---: | ---: |
| Debian dùng cho lab | `6d0001b9cc66` | 44 | 0 | 0 |
| Chainguard thử nghiệm | `2470470ae999` | 0 | 0 | 0 |

Trivy `0.73.0`, DB UpdatedAt `2026-09-20T19:19:55Z`, tải `2026-09-21T02:15:23Z`.
Quét đúng hai image **sau** vá HTTP; scan trước vá giữ làm lịch sử. [local-results.json](local-results.json)
ghi full image IDs, scanner ID, DB metadata, 44 findings và SHA-256 report. Chưa có lựa chọn S2.

Cả hai image: `16/16` unit tests + HTTP contract (JSON thuần, charset UTF-8, type/charset sai bị từ
chối), nonroot/network-none/read-only PASS. Chainguard cuối còn chạy `12/12` request model thật,
PCM khớp W-0320 `12/12`. Lần smoke thiếu voice allowlist bị readiness từ chối; giữ log riêng,
thêm đúng allowlist rồi mới PASS, không mở rộng tập giọng được chấp nhận.

Các phép đo sáu toàn câu dùng image trước vá HTTP với injection threads trong harness; runtime
cuối dùng cấu hình env thật. Collector xác nhận media PCM cuối khớp từng segment đã đo và một
toàn câu rollback; không nhận kết quả cũ là benchmark sáu toàn câu trên image cuối.

## Kiểm cuối và bàn giao

CI config, docs, provenance, voice acceptance, Helm guards và lab regression PASS. Mỗi image cuối
có `7/7` file shim khớp byte với workspace. Lab regression
gồm `17` refusal, `4` entry guards, `6` payload stub cases và `2` valid/`2` refusal profile.
Các stub cases độc lập với sáu cuộc thật trên. GitNexus detect-changes LOW, `0` affected processes
trên tracked diff chung; không coi đó là bao phủ mọi file mới hoặc tách riêng WIP của phiên khác.
PII scan của gói W-0321 PASS `3` file; whitespace diff PASS; gate-status PASS không nhận production rung.

Source bytes và log hashes trong [local-results.json](local-results.json); checksum có hậu tố
`sha256_base64` mã hóa đầy đủ 256 bit ở base64 để không nhầm chuỗi số trong hash với số điện thoại.
Artifacts/audio đầy đủ
ở `.artifacts/W-0321/` trên máy này, không commit model/secret. HEAD đã chuyển tới `423b573` do
phiên W-0319; thay đổi lượt này chưa commit, không nhận hosted CI/exact-commit acceptance.
Lab còn chạy ở profile segmented, bộ nghe tại `http://127.0.0.1:58121`; volume cũ được giữ nguyên.

S1 nghe/mối nối `PENDING_OWNER`; S2 `OWNER_DECISION_REQUIRED`; máy/mirror S5 `OWNER_DATA_REQUIRED`.
Production `BLOCKED`.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Gate trong full sweep có self-test kiểm thay đổi của việc này: `tts-container-selftest.mjs`, `lab-speech-preflight-selftest.mjs`.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0321 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. S5, mirror và
pháp lý làm ở W-0340…W-0344; giọng và câu ghép đã chốt ở W-0323. Claude chọn việc theo tiêu chí
phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
