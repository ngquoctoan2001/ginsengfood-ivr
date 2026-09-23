# W-0334 — W-0042: nối lỗi local tới cảnh báo

REAL_CUSTOMER_CALL_ALLOWED=NO

Baseline hồ sơ `f2a7bc8`. Candidate runtime `aaba3d2c173c1ce3b2e6dcbf899515ea5e87c979`,
checkout detached sạch. Bộ thu riêng gọi scenario có sẵn trong binary chaos cùng candidate,
không sửa runtime, luật alert hoặc scenario để tạo kết quả mong muốn.

## Phạm vi lượt chạy

- Chạy CHAOS-SIM-03 ba lần trên ba task/kênh giả; mỗi kênh được seed hai lỗi trước, lỗi thứ ba
  đi qua PostgresTelephonyDispatchStore.FailAsync và counter runtime thật.
  Fixture có execution mode LAB_REAL_SIM/provider VENDOR để kiểm store; harness gọi method
  trực tiếp, không khởi chạy gateway/dialer. REAL_CUSTOMER_CALL_ALLOWED=NO; toàn bộ dữ liệu giả.
- MeterListener cộng đúng các delta của ivr_channel_quarantines_total, xuất một series tổng
  cho Prometheus local scrape. Giá trị ban đầu là 0; không bơm điểm dữ liệu hoặc chỉnh đồng hồ.
  Đây là cầu đo của harness, **không phải kiểm chứng exporter/collector OTLP của deployment**.
- Prometheus v2.54.1 chạy riêng, dùng nguyên byte file luật ở candidate, kể cả cửa sổ 10 phút,
  for: 0m và chu kỳ đánh giá rule group 30 giây. Capture qua API rules/query của Prometheus;
  không có Alertmanager, không kiểm giao thông báo cho người vận hành.
- Cùng lượt chạy gọi CHAOS-RECOVERY-04, CHAOS-DB-02 và CHAOS-DOWNSTREAM-01. Hai scenario DB
  cắt/nối socket thật qua Toxiproxy; downstream và SIM chèn lỗi ở tầng service theo P6-3 §5.
  Đây là harness local, không phải staging hoặc toàn bộ API/worker đã triển khai.

## Kết quả chốt

**PASS_LOCAL_SERVICE_TO_PROMETHEUS**. Lượt chạy 15:36:17–15:47:43 ngày 21/09/2026, UTC+7.
[Verification](verification.json) chứa hash source/DLL/rule/raw artifact và các capture tại ba mốc.

| Mốc | Kết quả |
| --- | --- |
| Trước lỗi, 15:36:58 | Counter 0, target up, alert inactive |
| Ba lỗi SIM, 15:37:00 | Ba delta thực 1; DB có 3 task, 3 job, 3 attempt, 3 kênh HEALTH_FAILED; 0 counted customer attempt |
| Alert bật, 15:37:04 | IvrChannelAutoDisableBurst firing, counter 3, increase trong 10 phút đạt ngưỡng |
| Recovery/DB/downstream | Ba scenario còn lại đạt: backlog của ca recovery gửi đúng một lần; DB trả readiness 503 khi đứt link, giữ dữ liệu và phục hồi; downstream giữ retry bounded, không xác nhận giả |
| Alert tắt, 15:47:39 | Inactive, increase=0, target vẫn up và counter vẫn 3; khoảng 640 giây sau delta cuối, gồm nhịp đánh giá/scrape |

**6 lượt gọi scenario thuộc 4 nhóm**, 69 capture Prometheus. Bộ thu gọi method của binary chaos,
không ghi đây là một lượt dotnet test mới. Bộ full proof cũ **1171 test / 42 gate + 24 entry phân
loại** tại cùng aaba3d2 đã được consumer kiểm lại hash/provenance; bảy TestId hỗ trợ đều Passed,
gồm partial partition và eligibility no-dispatch. Không áp số này cho HEAD tài liệu về sau.

Snapshot DB lúc 15:43:49 vẫn có ba kênh HEALTH_FAILED: việc alert tắt chỉ chứng minh các lỗi đã
ra khỏi cửa sổ. Số recovery probe của ca DB lần này là **16 ms**, một quan sát không phải SLO.
PostgreSQL/Toxiproxy/network và Prometheus riêng đã được dọn sau khi thu xong; không còn container
mang session/nhãn lượt này. Container DSAR thực hành và các stack khác được giữ nguyên.

Lần chuẩn bị đầu chưa chạy scenario vì Windows từ chối đăng ký URL HttpListener. Bộ thu chuyển
sang Kestrel, không sửa ACL hệ thống/runtime/luật alert; artifact lỗi vẫn giữ tại
.artifacts/w0334-chaos-alert/run-20260921a/. Lượt thành công là run-20260921b.

[Cách chạy lại và snapshot bộ thu](replay.md) đi kèm để kiểm lại chuỗi này. W-0334 khép khoảng
trống bằng chứng local service→counter→Prometheus; các giới hạn triển khai bên dưới vẫn còn.

## Giới hạn và phần còn lại

- Alert IvrChannelAutoDisableBurst đo một đợt chuyển trạng thái trong cửa sổ; hết firing không
  đồng nghĩa ba kênh HEALTH_FAILED đã được sửa hoặc được phép dùng lại. Không reset kênh bằng SQL.
- Số recovery là quan sát một lần, không phải SLO hay phân phối. SIM giả không chứng minh SIM thật.
- P6-3 vẫn cần diễn tập trên đích staging cùng người vận hành và bằng chứng triển khai/OTLP/
  thông báo phù hợp. Toàn bố trí đích/người; dev IVR chạy lại và thu cùng loại evidence.
- W-0042 giữ TESTS_PASS; lượt này không tự ACCEPTED và không tự thay DoD P6-3.

[ARCH-05](../../../specs/architecture/05-resilience.md) đã bỏ dòng gộp Trust/Contact lỗi thời;
[game-day](../../gameday-report.md) có đính chính tham chiếu test, instrumentation, coverage và
partition. Nội dung historical được giữ nguyên.

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0334 thuộc nhóm A6 và chuyển **EVIDENCE_SUBMITTED → ACCEPTED** cho phần đã làm: Diễn
tập cảnh báo local cho W-0042. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: OTLP, Alertmanager, staging ngoài. Mọi giới hạn trong cột
Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại `ca4f442` (1200/1200 test,
sweep 43/43), không coi commit tài liệu là một lượt test/sweep mới. REAL_CUSTOMER_CALL_ALLOWED=NO.
