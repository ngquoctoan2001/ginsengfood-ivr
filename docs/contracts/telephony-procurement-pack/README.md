# Telephony Procurement Pack — 1 SIM lab và 32 eSIM production

Trạng thái: `OPEN` · Work `W-0057` (prompt `P11-1`) · Tạo `2026-08-18`

Gói artifact để Infra/Procurement làm việc với nhà cung cấp telephony. **Hai giai đoạn tách bạch**, không gộp:

| Giai đoạn | Mục tiêu | Gate | Đóng khi nào |
| --- | --- | --- | --- |
| **Giai đoạn 1 — Lab** | 1 SIM thật, danh sách số test được duyệt, xác minh an toàn | `G-LAB-SIM` | chỉ sau khi có evidence `P8-1`/`P8-2` |
| **Giai đoạn 2 — Production** | Năng lực nhiều kênh eSIM, đo thật | `G-ESIM32` | chỉ sau khi mua và **đo** được capacity/failover |

> **Không suy ra giai đoạn 2 từ giai đoạn 1.** Một SIM chạy tốt không chứng minh n kênh chạy tốt. Simulator lại càng không. Con số `32` trong tên gate là **mục tiêu cần đo**, chưa phải năng lực đã chốt — và số kênh cho pilot chưa được quyết định ở bất kỳ đâu.

## 1. Vì sao có gói này

IVR hiện chạy hoàn toàn trên `MockTelephonyDispatchGateway`. Toàn bộ Phase 2–3 xanh, nhưng **chưa một cuộc gọi thật nào xảy ra**, và `IVR_ADAPTER_MODE` chưa bao giờ ở `REAL`. `W-0008` là external work chặn `G-LAB-SIM` và `G-ESIM32`; hai gate đó chặn `P8-1`, `P8-2`, rồi `P9`.

Điểm mạnh của tình huống này: **cổng adapter đã được đặc tả bằng code**, không phải bằng mong muốn. Nhà cung cấp không cần đoán chúng ta muốn gì — họ chỉ cần trả lời sản phẩm của họ có ánh xạ được vào 6 operation trong [R-01](R-01-vendor-requirements.md) hay không.

## 2. Bảng artifact

| File | Nội dung | Owner | Due (chặn cái gì) |
| --- | --- | --- | --- |
| [R-00](R-00-voice-gateway-rfq.md) | **Bản gửi thẳng nhà cung cấp.** Gộp §13.2 (7 điều kiện loại trừ thiết bị) + §13.3 (bảng 11 call disposition) của tài liệu Module 8, cộng 9 câu hỏi. Gửi file này **trước** | Infra (soạn) + owner IVR (quyết định gửi) | gửi được ngay — mở khoá `B-01` |
| [R-01](R-01-vendor-requirements.md) | Annex kỹ thuật cho vòng đàm phán sau R-00: protocol/SDK, auth, 6 operation, DTMF, codec, disposition, health, caller ID, CDR, secret, bảo mật | Infra + Telephony vendor | trước khi gửi RFQ |
| [R-02](R-02-lab-package.md) | Gói lab 1 SIM: topology, allowlist số test, kill switch, checklist kịch bản disposition | Infra + Security | trước `P8-1` |
| [R-03](R-03-esim32-package.md) | Gói production nhiều kênh: lifecycle, pooling/failover/quarantine, throughput, cost, observability, disaster mode | Infra + Procurement | trước `P9-1` |
| [R-04](R-04-scorecard-and-gaps.md) | Scorecard có trọng số, gap register, điều khoản hợp đồng | Procurement + Infra | trước khi chọn nhà cung cấp |
| [R-05](R-05-tts-audio-capability.md) | Năng lực TTS/audio (`OD-V1-19`) — vendor, DPA, phát âm tiếng Việt, codec | Product + Infra + Privacy/Legal | trước `P8-1` |
| [R-06](R-06-to-trinh-mua-thiet-bi.md) | **Tờ trình duyệt mua** cho người không làm IT — điền giá vào rồi trình sếp | Nguyễn Quốc Toàn | sau khi có báo giá, **trước 15/09/2026** |
| [lab-acceptance-report-template.md](lab-acceptance-report-template.md) | Biểu mẫu báo cáo nghiệm thu lab — điền khi chạy `P8-1` | Infra | dùng ở `P8-1` |

## 3. Quyết định mở mà gói này phục vụ

| ID | Nội dung | Owner | Artifact ở đây |
| --- | --- | --- | --- |
| `OD-V1-09` | Giao thức lab 1 SIM, DTMF, disposition, allowlist | Infra/vendor | [R-00](R-00-voice-gateway-rfq.md), [R-01](R-01-vendor-requirements.md), [R-02](R-02-lab-package.md) |
| `OD-V1-10` | Năng lực 32 eSIM, failover, caller ID, chi phí | Infra/procurement | [R-03](R-03-esim32-package.md) |
| `OD-V1-18` | Vị trí resolve `dial_token → E.164` | Security + vendor | [R-01](R-01-vendor-requirements.md) §4 — cần **vendor capability statement**, xem thêm [T-04](../target-v1-closure-pack/T-04-dial-token.md) |
| `OD-V1-19` | Nhà cung cấp TTS/audio | Product + Infra + Privacy/Legal | [R-05](R-05-tts-audio-capability.md) |

Nguồn bảng quyết định: [`specs/_review/open-decisions-register.md`](../../../specs/_review/open-decisions-register.md) (chỉ đọc).

## 4. Ba ràng buộc bất biến trong mọi cuộc nói chuyện với nhà cung cấp

**Không có số điện thoại thô đi vào IVR (`D-05`).** IVR cầm `dial_token` mờ. Nếu API của nhà cung cấp **bắt buộc** nhận E.164 thì phải có một thành phần tin cậy đứng giữa để resolve, và thành phần đó không chạy trong process IVR. Đây là câu hỏi loại-trừ: nhà cung cấp không đáp ứng được ranh giới này thì cần một quyết định kiến trúc riêng chứ không phải một ngoại lệ.

**Recording mặc định TẮT (`DT-05`).** `dial()` mang tham số `recording: DISABLED` và nhà cung cấp phải cho **đọc ngược lại** trạng thái đó qua `health()`. Giá trị khác `DISABLED` bị từ chối fail-closed cho tới khi có legal sign-off. Nhà cung cấp nào bật ghi âm mặc định và không tắt được ở mức API thì không dùng được.

**Không gọi khách thật cho tới khi có release gate.** `REAL_CUSTOMER_CALL_ALLOWED=NO`. Lab chỉ quay số nằm trong allowlist. Kill switch phải chặn được ở tầng IVR, độc lập với nhà cung cấp.

## 5. Cái gói này KHÔNG làm

- **Không chọn nhà cung cấp.** Scorecard là công cụ cho Procurement, không phải kết luận.
- **Không đóng `W-0008`, `G-LAB-SIM` hay `G-ESIM32`.** RFQ và báo giá không đóng gate; chỉ artifact thật mới đóng.
- **Không chốt số kênh cho pilot.** Chưa có throughput đo thật thì mọi con số đều là giả định.
- **Không gửi cho ai.** IVR soạn; owner IVR quyết định gửi.

## 6. Liên quan

- Gói hợp đồng Sales/Auth: [`docs/contracts/target-v1-closure-pack/`](../target-v1-closure-pack/README.md) — `W-0058`, sở hữu `W-0002…W-0007`.
- Mô hình dung lượng và chi phí hiện có: [`docs/capacity-model.md`](../../capacity-model.md) — `ENGINEERING_MODEL`, không phải sizing đã duyệt.
- Sổ tiến độ duy nhất: [`prompt/_execution/prompt-execution-tracker.md`](../../../prompt/_execution/prompt-execution-tracker.md).
