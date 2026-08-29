# TODAY-03 — TTS gate handoff pack

Ngày khóa gói: `2026-08-29`  
Evidence baseline: `main@0baed74cd384cd661aed068c263a92ef97ead1f4`  
Trạng thái: `M8_LOCAL_COMPLETE / HANDOFF_READY / BLOCKED_EXTERNAL`  
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

> [!IMPORTANT]
> **Ba giọng được Owner chọn không phải là phê duyệt production.** W-0122 chỉ được đi tiếp khi
> Legal/Privacy, Security/Release và Platform/Infra/Telephony trả lời bằng artifact có người ký,
> ngày ký và reference kiểm chứng được. Im lặng, tin nhắn “OK” hoặc local test xanh không đóng gate.

## 1. Trạng thái có bằng chứng

| Hạng mục | Trạng thái | Kết luận được phép dùng |
| --- | --- | --- |
| Nghe 11 candidate và chọn 3 miền | `OWNER_ACCEPTED 2026-08-28` | Bắc Ngọc Linh, Trung Ngọc Trân, Nam Mỹ Duyên |
| Voice manifest gate | `PASS` | Exact signed manifest hợp lệ với audio/model binding hiện tại |
| 12 fixed WAV | `FILES_AND_IMAGE_PASS` | File/checksum/catalog sẵn sàng; **chưa có human listening** |
| Local ONNX/container/provenance | `LOCAL_PASS` | Chỉ chứng minh nonprod; không thay chữ ký external |
| Legal authority guard | `PASS — FAIL_CLOSED` | Owner-only `legal_gate=PASS` đã bị thu hồi; lock trở lại `OWNER_DATA_REQUIRED`, current blocker là `LEGAL` |
| 6 MicroSIP calls | `NOT_RUN` | Không có real-audio acceptance |
| Retention drill | `NOT_RUN` | Chưa chứng minh purge đúng file trên disposable lab |
| Rollback drill | `NOT_RUN` | Chưa chứng minh deliberate restore |
| Legal/Privacy | `EXTERNAL_RESPONSE_REQUIRED` | `OD-VOICE-07` chưa đóng |
| Security/Release | `EXTERNAL_RESPONSE_REQUIRED` | 13 HIGH + 3 CRITICAL chưa có disposition |
| Platform/Infra/Telephony | `EXTERNAL_RESPONSE_REQUIRED` | Mirror, target hardware và `OD-VOICE-08` chưa đóng |
| Production/real customer | `RELEASE_BLOCKED` | Không được bật cờ gọi khách thật |

## 2. Ba phiếu phải chuyển đúng owner

### Legal / Privacy

Gửi [questions-to-legal-od-voice-07.md](questions-to-legal-od-voice-07.md).

Phải trả lại đủ `L1`–`L6`, người/ngày ký và approval reference áp dụng cho đúng source/model/codec
pin cùng ba preset Ngọc Linh / Ngọc Trân / Mỹ Duyên. Nếu chấp nhận, machine gate yêu cầu
`decision_authority=LEGAL_PRIVACY`; chữ ký Owner Module 8 một mình không hợp lệ.

### Security / Release

Gửi [questions-to-security-w0122-cve-disposition.md](questions-to-security-w0122-cve-disposition.md).

Phải chọn `SEC-A` kèm hạn review/điều kiện re-scan hoặc `SEC-B` kèm base image đích và deadline.
Không trả lời đồng nghĩa `RELEASE_BLOCKED`; `0 fixable` không có nghĩa là được tự bỏ qua.

### Platform / Infra / Telephony

Gửi [questions-to-platform-w0122-infrastructure.md](questions-to-platform-w0122-infrastructure.md).

Phải trả internal mirror URI/digest cho đủ artifact, target hardware/resources để đo và quyết định
production media sink/topology cho `OD-VOICE-08`. Kết quả laptop hoặc local Compose không thay thế.

**External dispatch:** `NOT_PERFORMED`. Gói này chuẩn bị nội dung và routing; không giả vờ rằng ba
bên đã nhận hoặc đã đồng ý.

## 3. Owner lab acceptance còn phải chạy

Runbook điều khiển: [lab-runbook.md](../../docs/evidence/W-0122/lab-runbook.md).

### 3.1 Nghe đủ 12 fixed segments

| Miền / giọng | Segment 1 | Segment 3 | Segment 5 | Segment 7 | Owner verdict | Evidence ref |
| --- | --- | --- | --- | --- | --- | --- |
| Bắc / Ngọc Linh | `PENDING` | `PENDING` | `PENDING` | `PENDING` | | |
| Trung / Ngọc Trân | `PENDING` | `PENDING` | `PENDING` | `PENDING` | | |
| Nam / Mỹ Duyên | `PENDING` | `PENDING` | `PENDING` | `PENDING` | | |

Mỗi giọng phải nghe đủ bốn đoạn. Metadata, checksum và decode pass không chứng minh âm lượng, ngữ
điệu hoặc chất lượng mối nối ở đầu softphone.

### 3.2 Sáu cuộc gọi MicroSIP bằng dữ liệu giả

| Call | Fake order | Miền / giọng | Nội dung + số/tiền/khu vực | 6 mối nối `1→2→3→4→5→6→7` | DTMF | Media round-trip | Owner verdict |
| ---: | --- | --- | --- | --- | --- | --- | --- |
| 1 | A | Bắc / Ngọc Linh | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | |
| 2 | B | Bắc / Ngọc Linh | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | |
| 3 | A | Trung / Ngọc Trân | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | |
| 4 | B | Trung / Ngọc Trân | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | |
| 5 | A | Nam / Mỹ Duyên | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | |
| 6 | B | Nam / Mỹ Duyên | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | `NOT_RUN` | |

Chỉ dùng fake order và allowlisted lab destination. Sáu cuộc gọi này vẫn không cấp quyền gọi khách
thật.

### 3.3 Retention và rollback

- [ ] Khóa scheduler/dial; xác nhận không có playback active.
- [ ] Dùng disposable lab DB/volume; không chạy purge trên môi trường đang phục vụ call.
- [ ] Chứng minh expired dynamic file bị xóa, fresh dynamic/fixed/baseline giữ nguyên.
- [ ] Khôi phục safe retention defaults.
- [ ] Deliberately restore previous image/provider config; ghi thời gian và kết quả.
- [ ] Chứng minh không có silent SaaS fallback.

## 4. Stop rule

TODAY-03 chỉ được chuyển sang `EXTERNAL_GATES_PASS` khi có đủ ba phản hồi ký duyệt và Owner lab
evidence ở §3. Trước thời điểm đó:

- không ghi `ACCEPTED`, `PRODUCTION_READY` hoặc `REAL_CUSTOMER_CALL_ALLOWED=YES`;
- không tái lập `MODELS.lock.legal_gate=PASS` bằng chữ ký Owner Module 8;
- không chạy retention purge nếu thiếu disposable DB hoặc chưa khóa dispatch;
- không mở production deploy chỉ từ container/ONNX/Helm proof.

## 5. Xác nhận phía Module 8

**Người ký handoff:** **Tôi — Module 8 / Project Owner**  
**Ngày:** `2026-08-29`  
**Phạm vi chữ ký:** xác nhận trạng thái, routing và stop rule của gói TODAY-03; không thay chữ ký
Legal/Privacy, Security/Release hoặc Platform/Infra/Telephony.
