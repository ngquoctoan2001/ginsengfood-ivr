# W-0057 — Evidence: Telephony Procurement Pack (`P11-1`)

Ngày: `2026-08-18` · Trạng thái đạt được: `EVIDENCE_SUBMITTED` (mức tối đa của một gói tài liệu, theo DoD `P11-1`)

## 1. Artifact

| File | Nội dung |
| --- | --- |
| `docs/contracts/telephony-procurement-pack/README.md` | Index, hai giai đoạn tách bạch, ba ràng buộc bất biến |
| `…/R-01-vendor-requirements.md` | Yêu cầu nhà cung cấp — 12 mục, dựng quanh 6 operation của `ISimGateway` |
| `…/R-02-lab-package.md` | Gói lab 1 SIM: 9 điều kiện tiên quyết, quy tắc số test, 22 kịch bản, 4 quy tắc dừng |
| `…/R-03-esim32-package.md` | Gói production nhiều kênh: mô hình nhu cầu, quota, failover, đo tải, chi phí, disaster |
| `…/R-04-scorecard-and-gaps.md` | 4 hạng mục loại trừ, scorecard 16 tiêu chí có trọng số, gap register, 10 điều khoản hợp đồng |
| `…/R-05-tts-audio-capability.md` | `OD-V1-19`: TTS vs file thu sẵn, ràng buộc kỹ thuật, nghiệm thu phát âm, chi phí |
| `…/lab-acceptance-report-template.md` | Biểu mẫu nghiệm thu lab — 8 mục, điền khi chạy `P8-1` |

Tổng 7 file. Không sửa code, không sửa contract, không sửa `docs/documents/`, không sửa `specs/_review/`.

## 2. Phủ DoD

`P11-1` §6 liệt kê 10 hạng mục bắt buộc:

| Hạng mục DoD | Ở đâu |
| --- | --- |
| protocol / SDK | R-01 §2 |
| DTMF mode | R-01 §5 |
| codec / format | R-01 §6, R-05 §2 |
| disposition mapping | R-01 §7, biểu mẫu §4 |
| concurrency / channel | R-01 §8, R-03 §5 |
| health API | R-01 §8 |
| caller ID | R-01 §9 |
| secret provisioning | R-01 §3, R-02 §4 |
| CDR | R-01 §10 |
| **TTS / audio capability (`OD-V1-19`)** | R-05 (cả file) |

Mỗi hạng mục R-01…R-05 có owner, due và mục closure artifact riêng.

## 3. Cách gói này được dựng

Điểm khác biệt so với một RFQ thông thường: **cổng adapter đã tồn tại dưới dạng code**, nên yêu cầu không phải mô tả bằng văn xuôi mà là một bảng ánh xạ.

| Nguồn trong repo | Dùng làm gì |
| --- | --- |
| `ISimGateway` (6 operation) trong `src/Ivr.Domain/Ports/ProviderPorts.cs:204` | Bộ khung của R-01 §1 — nhà cung cấp điền cột "ánh xạ bằng gì" |
| `SimProviderDisposition` (11 giá trị) | Bảng ánh xạ disposition R-01 §7 và biểu mẫu §4 |
| `IvrSimChannel` trong OpenAPI | Mô hình trạng thái kênh R-03 §5 |
| `MockTelephonyDispatchGateway` (cooldown 5s, `fail_count ≥ 3`) | Mặc định `DT-04` cần nhà cung cấp xác nhận |
| `seed/call-scenarios.sample.json` (15 kịch bản) | Lọc ra 7 kịch bản chạm telephony cho R-02 §6a |
| `docs/capacity-model.md` | Toàn bộ ràng buộc TTS ở R-05 §2 |
| `specs/api/04-sim-adapter-contract.md` | `DT-05` recording, `DT-01` protocol pending, ranh giới `dial_token` |

## 4. Ba điều đáng chú ý phát hiện khi soạn

**1. Lab bị chặn bởi một quyền không tồn tại.** `OD-V1-20` ghi bộ permission `DF-01` (LOCKED, 7 quyền) không có quyền nào cho phép sửa `labDestinationAllowlist` hay `globalDialKillSwitch`. Nghĩa là hôm nay **không ai được phép bấm** hai control an toàn quan trọng nhất của lab qua console. Đây là điều kiện tiên quyết của lịch lab, không phải việc dọn dẹp sau — đã ghi thành mục số 7 trong 9 điều kiện tiên quyết của R-02 và thành gap `G-A` trong R-04.

**2. `OD-V1-15` và `OD-V1-19` ràng buộc nhau, chưa ai nối hai cái lại.** Whitelist lời thoại quyết định phương án audio: bộ hẹp (mã đơn + số tiền) có thể ghép file thu sẵn, rẻ và không có dữ liệu rời mạng nội bộ; bộ rộng (thêm tên sản phẩm và khu vực giao) gần như buộc phải có TTS động, kéo theo PDPA và chi phí theo ký tự. Hai quyết định đang nằm ở hai owner khác nhau và chưa có ai nói cho nhau biết. Đã ghi ở R-05 §1.

**3. Mock cố tình dễ dãi ở đúng chỗ lab cần đo.** 15 kịch bản trong seed chỉ có 7 chạm telephony, và cả 7 đều là những thứ mock dựng được dễ dàng. Những thứ mock **không** dựng được — hộp thư thoại báo là "answered", barge-in, kill switch giữa cuộc gọi, caller ID bị nhà mạng gắn nhãn — lại chính là những thứ có thể buộc sửa code. Đã tách thành 15 kịch bản `L-01…L-15` ở R-02 §6b, và đánh dấu `L-02`, `L-09` nên chạy sớm.

## 5. Kiểm chứng cơ học

| Lệnh | Kết quả |
| --- | --- |
| Kiểm link nội bộ toàn gói (7 file) | 0 link hỏng |
| `sh deploy/ci/scripts/scan-pii.sh docs/evidence` | xem §7 |
| `node deploy/ci/scripts/docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |

Gói này thuần tài liệu; không chạy lại build/test vì không chạm code.

## 6. Cái này KHÔNG chứng minh

- **Không đóng `W-0008`, `G-LAB-SIM` hay `G-ESIM32`.** RFQ và báo giá không đóng gate.
- **Không chọn nhà cung cấp.** Scorecard là công cụ cho Procurement.
- **Không chốt số kênh cho pilot.** Con số `32` được ghi rõ là mục tiêu cần đo, không phải năng lực đã chốt. Mô hình nhu cầu R-03 §2 để trống toàn bộ đầu vào vì ba trong số đó là câu hỏi business chưa ai trả lời.
- **Không phê duyệt** giao thức lab, ranh giới resolve token, nhà cung cấp TTS hay permission runtime-gate.
- **Không gửi cho ai.** IVR soạn; owner IVR quyết định gửi.

## 7. Ghi chú về cổng PII

File này và biểu mẫu nghiệm thu khi điền xong đều nằm trong phạm vi quét. Quy tắc đã ghi thẳng vào biểu mẫu: dùng nhãn `LAB-A`/`LAB-B`/`LAB-C`, bảng ánh xạ nhãn sang số liên lạc giữ ngoài repo, và chạy `sh deploy/ci/scripts/scan-pii.sh docs/evidence` trước khi coi là xong.

Đây là lần thứ ba trong dự án mà cổng PII ảnh hưởng tới cách viết tài liệu (sau `A-0190` và `A-0193`). Việc đưa quy tắc vào **chính biểu mẫu** thay vì để trong một tài liệu hướng dẫn riêng là có chủ ý: người điền biểu mẫu ở lab sẽ đọc nó, người đọc tài liệu hướng dẫn thì chưa chắc.

## 8. Việc kế tiếp

| Việc | Ai | Ghi chú |
| --- | --- | --- |
| Quyết định gửi gói cho Infra/Procurement/vendor | **owner IVR** | IVR không tự gửi |
| Xin permission `OD-V1-20` | Security/Platform + Release owner | chặn lịch lab, không chặn RFQ |
| Nối `OD-V1-15` với `OD-V1-19` | Product | quyết định whitelist ảnh hưởng trực tiếp phương án audio |
| `P11-3` / `W-0059` — gói legal/retention | chưa làm | prereq `W-0052`, `W-0053` chưa chạy |
| `P4-2` → `P4-3` → `P4-4` → `P4-1` | tiếp theo trong lộ trình | mock-only cho tới khi hai gói closure có câu trả lời |
