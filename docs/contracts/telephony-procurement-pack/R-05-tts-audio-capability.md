# R-05 — Năng lực TTS và nguồn audio

External work `W-0008` · quyết định `OD-V1-19` ✅ `CLOSED` `2026-09-17` · gate `LAB_REAL_SIM`

Owner: **Product** (chấp nhận phát âm), **Infra** (máy chạy, kho bản cài nội bộ), **Owner** (ký rủi ro
licence model — `S2`).

## 1. Đã chốt: VieNeu-TTS tự host

| | |
| --- | --- |
| Bộ đọc | **VieNeu-TTS v3 Turbo** (ONNX int8), tự host — `W-0122`. Bộ đọc **duy nhất**, production lẫn lab |
| Chạy ở đâu | Sidecar trong pod worker, chỉ nghe loopback. Không nhà cung cấp TTS đám mây nào, nên **không văn bản đơn nào rời hệ thống** |
| Đọc gì | Phần cố định của kịch bản render sẵn (`12` đoạn, ba giọng `OD-VOICE-06`); món hàng, số lượng, số tiền, vùng giao đọc lúc gọi |
| Định dạng ra | `audio/L16`, mono 8 kHz — IVR ghi thành file `.sln` cho Asterisk phát |

Nhà mạng và gateway **không cần năng lực TTS nào**: IVR đưa cho media server audio đã render xong.
Câu hỏi duy nhất còn dính tới nhà mạng là codec/format gateway nhận ([R-01](R-01-vendor-requirements.md) §6).

## 2. Ràng buộc kỹ thuật đã có trong code

Nguồn: [`docs/capacity-model.md`](../../capacity-model.md) — ngân sách mặc định, **fail-closed**:

| Ràng buộc | Giá trị | Cưỡng chế ở đâu |
| --- | --- | --- |
| Ký tự tối đa mỗi lần tổng hợp | 1.200 | từ chối **trước khi** gọi sidecar |
| Yêu cầu tối đa mỗi tiến trình / phút | 60 | ngân sách cửa sổ cố định |
| Ký tự tối đa mỗi tiến trình / phút | 72.000 | ngân sách cửa sổ cố định |
| Thời lượng audio tối đa | 120 giây | từ chối kết quả nếu vượt |
| Timeout mỗi lần gọi sidecar | 5 giây | thành `IVR_TECHNICAL_EXCEPTION`, **không bao giờ** thành no-answer |
| TTL cache tối đa | 900 giây | còn bị chặn thêm bởi hạn xác nhận và retention lời thoại |
| Endpoint | loopback | validator từ chối khởi động nếu endpoint không phải loopback |

Định danh cache: `SHA-256` trên `(script_template_id, script_version, hash(privacy_safe_order_summary), voice_id, locale)` — **không** chứa nội dung tóm tắt, số liên lạc, khu vực hay văn bản đã render. Khởi động lại là mất cache; job retention `P1-5` gọi hook purge.

Ở `MOCK`, audio là **metadata mô phỏng** cùng định dạng — MOCK không mở socket mạng và không đại
diện cho codec thật nào. Codec thật phía gateway do câu trả lời ở [R-01](R-01-vendor-requirements.md) §6 quyết định.

## 3. Nghiệm thu phát âm tiếng Việt

Không đo bằng cảm nhận. VieNeu phải đọc đúng một bộ mẫu cố định, nghe bởi ít nhất 2 người, chấm
đạt/không đạt từng dòng:

| # | Loại | Ví dụ cần đọc đúng | Đạt? |
| --- | --- | --- | --- |
| 1 | Tên sản phẩm thuần Việt | `<điền>` | `<điền>` |
| 2 | Tên sản phẩm có từ nước ngoài | `<điền>` | `<điền>` |
| 3 | Tên có dung tích/khối lượng (`500ml`, `1kg`) | `<điền>` | `<điền>` |
| 4 | Số tiền lớn | `560.000` đọc thành gì | `<điền>` |
| 5 | Số lượng + đơn vị | `2 hộp`, `10 gói` | `<điền>` |
| 6 | Khu vực giao có số | `Quận 7`, `Phường 12` | `<điền>` |
| 7 | Mã đơn rút gọn | đọc từng ký tự hay đọc thành số | `<điền>` |
| 8 | Câu hướng dẫn bấm phím | `bấm 1`, `bấm 0` — phải rõ ràng tuyệt đối | `<điền>` |

Dòng 8 quan trọng nhất và hay bị coi nhẹ: khách nghe nhầm hướng dẫn thì bấm nhầm phím, và hệ thống ghi nhận **ngược lại** ý khách. Không có cơ chế nào phát hiện việc đó.

Dòng 4 và 7 cần Product chốt **quy ước đọc**: `560.000` đọc "năm trăm sáu mươi nghìn" hay "năm trăm
sáu mươi ngàn"; mã đơn `0001` đọc "không không không một" hay "một". Số tiền và số lượng được IVR
viết thành chữ trước khi đưa cho VieNeu, nên quy ước nằm trong code, không nằm trong model.

## 4. Chi phí

Tự host nên **không có phí theo ký tự**. Chi phí là máy chạy sidecar, và cái cần đo là tốc độ: mỗi câu
phải đọc xong trong `5` giây, còn dư `20%` trước lúc quay số (`deploy/ci/scripts/tts-helm-selftest.mjs`).

| Đầu vào | Nguồn | Giá trị |
| --- | --- | --- |
| CPU/RAM máy chạy sidecar | Infra (`S5`) | `<điền>` |
| Thời gian đọc p95 mỗi câu trên máy đó | lab trên máy thật | `<điền>` |
| Tỉ lệ cache hit đo được | lab/pilot | `<điền>` |
| Số đơn mỗi tháng | Business | `<điền>` |

Tỉ lệ cache hit quyết định số lần sidecar phải đọc: câu thoại chứa tên sản phẩm và số tiền của **từng
đơn**, nên cache chỉ trúng khi hai đơn giống hệt nhau về nội dung đọc. **Phải đo, không đoán.**

## 5. Còn thiếu trước khi bật ở production

`OD-V1-19` đã đóng; VieNeu chỉ bật ở production khi đủ:

- [ ] **Quyền dùng model** — `legal_gate` trong `deploy/tts/models/MODELS.lock` (model card khai Apache-2.0, revision đã ghim không có file `LICENSE`) — `S2` rủi ro `4`.
- [ ] **`16` lỗ hổng của base image** `ivr-tts` đã xử lý hoặc được ký chấp nhận — `S2` rủi ro `3`.
- [ ] **Kho bản cài nội bộ** cho `13` file model — `internal_mirror_gate`.
- [ ] **Đo trên máy thật** theo §4.
- [ ] **Bộ nghiệm thu phát âm §3 đã chấm**, tối thiểu 2 người nghe, và Product chốt quy ước đọc cho dòng 4 và 7.
- [ ] **Xác nhận codec khớp** giữa đầu ra sidecar và đầu vào gateway ([R-01](R-01-vendor-requirements.md) §6).

Cho tới lúc đó, phát âm, tốc độ trên máy thật và năng lực nhiều kênh đều giữ `NOT_RUN`.
