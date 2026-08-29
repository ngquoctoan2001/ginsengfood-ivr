# BÁO CÁO TIẾN ĐỘ — HỆ THỐNG IVR XÁC NHẬN ĐƠN HÀNG

**Kỳ báo cáo:** 23/08 → 29/08/2026 (7 ngày) · **Người thực hiện:** Nguyễn Quốc Toàn · **Nguồn:** 62 commit từ sau `573dc8a` đến `main@b082ed1`

**Trạng thái an toàn:** hệ thống vẫn chạy bằng MOCK/lab; `REAL_CUSTOMER_CALL_ALLOWED=NO`; chưa đủ điều kiện gọi khách hàng thật hoặc phát hành production.

## 1. TIẾN ĐỘ HIỆN TẠI

### 1.1 · Trạng thái theo bằng chứng

| Hạng mục | Trạng thái đã xác minh | Giới hạn còn lại |
| --- | --- | --- |
| Lõi IVR, DB, scheduler, callback | `TESTS_PASS` local | Target DB, M3 sandbox và shared E2E chưa chạy |
| Giao diện quản trị | 12 route, 176/176 test; lint/typecheck/build PASS | Chỉ là reference implementation; M3 sở hữu operator UI/identity |
| Giọng nói/TTS | Ghép audio động; VieNeu-TTS self-hosted và 3 profile giọng đã chuẩn bị | Chưa nghe đủ 12 đoạn, chưa chạy 6 cuộc MicroSIP, thiếu Legal/Security/Platform |
| Tổng đài | Có Asterisk software lab, kill switch và cắt cuộc gọi đang diễn ra | Chưa có bằng chứng 1 SIM thật/carrier; chưa đo capacity thật |
| Tích hợp Module 3 | Đã chốt ranh giới: M3 quyết định cần gọi, IVR chỉ thực thi và báo kết quả | Contract Target V1 còn DRAFT; producer/consumer, auth và sign-off còn thiếu |
| Quan sát hệ thống | Trace/metric/log OTLP và LGTM local đã PASS | Staging observability chưa có endpoint, credential và evidence |
| Phát hành | 11 gate/140 work item/23 quyết định được mirror đúng | Đang ở nấc 0; chưa đạt bất kỳ nấc readiness nào |

### 1.2 · Sổ tiến độ hiện hành

**140 work item:** 8 `ACCEPTED` · 91 `TESTS_PASS` · 21 `EVIDENCE_SUBMITTED` · 16 `BLOCKED_EXTERNAL` · 2 `DEFERRED_TARGET` · 1 `CODE_DONE` · 1 `N/A`.

**Kiểm lại trên HEAD ngày 29/08:** .NET **760/760** (495 unit + 233 integration + 24 contract + 8 chaos); Admin UI **176/176**; Next production build PASS; `GATE_STATUS_PASS`.

## 2. NHỮNG VIỆC ĐÃ LÀM TRONG TUẦN

| Ngày | Nhóm việc | Kết quả chính |
| --- | --- | --- |
| 23/08 | Speech + vận hành console | Ghép đoạn audio cố định/TTS động; quản lý script; màn runtime gate; cắt cuộc gọi; seed/scenario; lưu đúng giọng đã phát |
| 24–26/08 | DB, CI, tiếng Việt và lab | Gate schema hai chiều; 16 CHECK enum; telemetry tiếng Việt; OpenAPI sạch; sửa pipeline audio 3 miền và lỗi dấu tiếng Việt trong runtime invariant |
| 27/08 | M3 boundary + TTS self-hosted | Gỡ quyền tự quyết định gọi khỏi IVR; M3 là decision owner; đóng gói VieNeu-TTS, model lock, Helm/Compose/CI và lab audition fail-closed |
| 28/08 | Auth/admin + intake | Chuyển sang service credential ba tầng; xoá console account/session; UI thành reference; thêm 9 lý do intake nội bộ và candidate W-0128/W-0129 có SHA riêng |
| 28/08 | Capacity + hồ sơ mua sắm | Thêm test quá tải 32 kênh/800 job; khóa drift duration/session; sửa mốc 3G, yêu cầu VoLTE, số kênh và attempt policy chưa được ký |
| 29/08 | Observability + governance | Hoàn tất local observability; đồng bộ tracker/readiness; thu hồi có thể hoàn tác DOCX V0.3 lỗi thời; chuẩn bị data-intake cho capacity calibration |

### 2.1 · Hai thay đổi quan trọng so với báo cáo 22/08

- “Đăng nhập bằng tài khoản console” đã bị thay thế: operator identity/UI thuộc Module 3; IVR chỉ giữ service credential theo tier và reference UI không deploy production.
- “IVR tự bỏ qua khách cũ” đã bị huỷ khỏi runtime: Module 3 quyết định `CALL_REQUIRED`; IVR chỉ kiểm gate kỹ thuật/an toàn, thực thi cuộc gọi và callback.

## 3. DỰ KIẾN HOÀN THÀNH

| Mốc | Khi nào có thể hoàn thành | Bằng chứng bắt buộc |
| --- | --- | --- |
| Candidate local/CI | Sau khi freeze exact SHA và hosted pipeline xanh | Build/test/gate đầy đủ gắn đúng SHA, không dùng mixed `save` làm release evidence |
| Nấc 1 — hoàn tất sau MOCK | Khi toàn bộ planned work được owner/reviewer `ACCEPTED` | Hiện mới 8/140 work item `ACCEPTED` |
| Nấc 2 — lab 1 SIM thật | Sau khi có thiết bị/SIM, allowlist, giọng duyệt và người nghiệm thu | Lab report, kill-switch, 6 cuộc gọi, media/retention/rollback evidence |
| Nấc 3 — tích hợp M3 thật | Sau khi Target V1 được ký và có sandbox/auth thật | Producer, callback consumer, CDC/shared E2E và target-DB preflight |
| Nấc 4 — đủ điều kiện production | Sau capacity 32 eSIM, Legal/Security/Platform và Release sign-off | Measured capacity/failover, DF-03 và go/no-go được chấp nhận |

**Chưa thể chốt ngày go-live.** Lịch cũ “cuối tháng 9/2026” chỉ còn khả thi nếu các đầu vào bên ngoài được bàn giao và nghiệm thu đúng chuỗi trên.

## 4. KHÓ KHĂN & CẦN GÌ ĐỂ HOÀN THIỆN

| Ưu tiên | Cần từ ai | Đầu vào/bằng chứng còn thiếu |
| --- | --- | --- |
| P0 | Module 3/Product | Ký program/result/policy/session/revoke/opt-out; producer + callback consumer; sandbox và shared E2E |
| P0 | Hạ tầng/Telephony | 1 SIM + gateway VoLTE, destination allowlist, vendor/credential custody, target DB access và quyền chạy preflight |
| P0 | Legal/Security/Platform | TTS license, privacy/retention, CVE disposition, internal mirror, target hardware/media sink và observability staging |
| P1 | Release owner/Reviewer | Chấp nhận evidence còn treo; xác nhận go/no-go; reviewer độc lập và GitLab approval policy |
| P1 | M3/Infra/Telephony | Arrival buckets, session profile, per-attempt timing, outcome rate, reserve/failure factor để calibrate capacity |

Rủi ro lớn nhất không còn là thiếu code local, mà là thiếu contract đã ký, hạ tầng thật, dữ liệu đo thật và người có thẩm quyền chấp nhận bằng chứng.

## 5. KẾ HOẠCH TRIỂN KHAI TIẾP THEO

1. Freeze candidate tuần theo exact SHA; chạy hosted CI và chỉ giữ kết quả gắn đúng candidate.
2. Chốt contract trước code: M8-05/M8-06/M8-08/M8-09 và các quyết định owner/M3/Legal/Security liên quan.
3. Chạy target-DB preflight bằng credential/ticket được cấp; dừng ở `OWNER_DATA_REQUIRED` nếu chưa có quyền.
4. Nối M3 sandbox: task producer → IVR intake → call MOCK/lab → callback consumer; chạy CDC/shared E2E và kiểm idempotency/revalidation.
5. Hoàn tất TTS/lab 1 SIM: nghe 12 đoạn, 6 cuộc MicroSIP, allowlist/kill switch, media round-trip, retention và rollback.
6. Chạy observability staging với exact image/SHA; chứng minh trace đủ 5 stage, dashboard, alert fire/recovery và outage resilience.
7. Thu dữ liệu timing/arrival/session thật, calibrate capacity; chỉ sau đó mới trình Owner chốt số kênh pilot/production.
8. Chỉ mở pilot/go-live khi bốn nấc readiness đạt, DF-03/go-no-go được ký và `REAL_CUSTOMER_CALL_ALLOWED` được Release owner cho phép.

*Lập ngày 29/08/2026 · Báo cáo dựa trên commit, source, tracker/gate mirror và kiểm thử chạy lại trên `main@b082ed1`; local/MOCK evidence không được dùng thay cho lab, integration hoặc production evidence.*
