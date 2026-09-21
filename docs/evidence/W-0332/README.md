# W-0332 — Kiểm lại W-0207 E2E local đúng candidate

REAL_CUSTOMER_CALL_ALLOWED=NO

**PASS_LOCAL_IVR_SIDE**, candidate **aaba3d2c173c1ce3b2e6dcbf899515ea5e87c979**.
Khoảng trống bằng chứng local của W-0207 đã được kiểm lại. Không sửa harness hoặc logic sản phẩm.
Không tự ACCEPTED; phần M3 và signer thật vẫn thiếu.

## Kết quả

- Release build 0 warning/0 error; 32 migration và seed trên PostgreSQL mới, riêng cổng 55442.
- E2E chạy 14:44:25–14:53:45 ngày 21/09/2026, UTC+7: **20/20 vòng, 330/330 task trong vòng lặp,
  0 failure, 0 open finding**, hai worker; lượt mở rộng chạy mỗi hai vòng.
- **11/11 ca** đúng danh sách callback_matrix của template W-0174; contract callback khớp hash đã ghim.
- Probe 10 request đồng thời không client retry: intake 10/10, eligibility 10/10, **0 HTTP 500**.
- **0 final trùng, 0 callback trùng, 0 lần đếm sai customer attempt**.
- Journal của fake receiver: **327 callback ID, 24 ID được gửi lại, 0 khác biệt payload/idempotency key**.

| Tình huống lỗi thật trên stack giả | Quan sát |
| --- | --- |
| Kill worker giữa cuộc, đợi lease hết | Một attempt chuyển RECOVERY_REQUIRED/held; không quay lại mù; ba task còn lại hoàn tất |
| Kill switch | 100 lượt poll không có attempt/result; bỏ chặn thì các task hoàn tất đúng một lần |
| Operator terminate giữa cuộc | IVR_TECHNICAL_EXCEPTION, không đếm customer attempt |
| Receiver biến mất | 4 phục hồi, 0 dead-letter; retry cao nhất 2/3 |
| Receiver trả 503 rồi phục hồi trong ngân sách | 3/3 giao lại, retry cao nhất 2/3 |
| Dead-letter và replay | 3 callback đạt trần 3, cả ba được replay |
| Retention thử nghiệm | 20 snapshot cũ redact, 20 job giữ vì outbox còn hạn; 20 task/callback còn hạn nguyên vẹn |

Retention dùng kỳ hạn thử được cấu hình tường minh trên DB giả, không đổi quyết định S3 giữ vĩnh viễn.
20 vòng kiểm tính đúng lặp lại; không thay lượt endurance 100 vòng hoặc đo SLO trên phần cứng đích.

## Ràng buộc bằng chứng

[Verification](verification.json) lưu source trước/sau sạch, SHA/tree, lệnh/exit/time/hash của build,
migration và E2E; hash harness, DLL Release, contract/template; đầy đủ kết quả 11 ca và fault.
Raw: .artifacts/w0332-e2e-aaba3d2/ gồm run.json, local-mock-e2e.json, log lệnh và runtime-logs/.
Không ghi đè artifact cũ W-0207 hoặc W-0286.

Cùng candidate đã qua [W-0330](../W-0330/verification.json): **1171/1171 .NET, full sweep 42/42**,
24 entry được manifest phân loại không có invocation. Bộ test .NET là Debug; E2E này dùng binary Release.
Consumer kiểm lại cùng SHA/hash và mười TestId trong gói W-0207, bao gồm:
UT-CALLBACK-TARGET-ACK-CROSS-01; IT-API-AUTHZ-01/02; IT-API-AUDIT-04; IT-API-IDEMP-03;
IT-API-QUEUE-08; IT-API-RETRY-06; IT-API-SIM-09; IT-API-TERMINATE-09/10 — đều Passed.

API, worker và fake receiver của lượt chạy đã được harness dừng. PostgreSQL W-0332 và volume riêng
được dọn sau khi lưu artifact; không chạm stack dev hoặc database thực hành DSAR của owner.

## Việc còn lại và người phụ trách

- **Đủ đề nghị phần local IVR của W-0207**, chờ Toàn xét phạm vi; không còn thiếu lượt E2E đúng SHA.
- **Dev M3 + dev IVR:** chứng minh producer call-ready, M3 revalidation/thay đổi trạng thái đơn và BFF
  thực sự, trên cùng contract. Fake Sales không thể chứng minh các phần đó.
- **Toàn:** bố trí credential/issuer/đích chung và người ký thật. Validator hiện yêu cầu năm signer
  riêng cho M8_OWNER/M3_OWNER/SECURITY/PLATFORM/RELEASE_OWNER, verifier tách signer. Nếu nhóm không
  đáp ứng mô hình này thì cần quyết định đổi quy tắc; không nhân tên hoặc tự ký.
- Template kiểm lại ra **SHARED_E2E_TEMPLATE_VALID_NOT_READY**. Không có shared E2E/go-live mới.
