# Phiếu duyệt một lượt — 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Toàn yêu cầu gom các việc duyệt nhanh để duyệt một lần. Phiếu này lấy 84 mục đang ở XEM trong
[danh sách nghiệm thu](../../release/acceptance-batches.md) tại candidate `ca4f442`, đọc đủ cột
Residual của từng mục trong tracker, rồi chia ba nhóm.

Cả 84 mục đã đạt C1, C2 và C4 tại `ca4f442`: 1200/1200 test .NET và sweep 43/43 trên cây sạch.
Điều còn lại là C3, tức cột Residual còn việc của IVR hay không. Chỉ Toàn chuyển một dòng sang
`ACCEPTED`.

| Nhóm | Số mục | Nghĩa |
| --- | ---: | --- |
| A | 73 | Residual chỉ còn việc bên ngoài, hoặc câu hỏi đã được việc sau trả lời. 9 mục thuộc kế hoạch |
| B | 5 | Mô hình capacity: gate đạt có điều kiện, chưa hiệu chỉnh |
| C | 6 | Chưa trình: còn việc phía IVR, hoặc cần rà lại phạm vi |

Duyệt nhóm A đưa tổng `ACCEPTED` từ 71 lên 144/336 (tính cả W-0348), và Nấc 1 từ 25 lên 34/54.
Duyệt thêm nhóm B thì thành 149/336 và 35/54.

## Duyệt nghĩa là gì

- Mỗi mục chuyển sang `ACCEPTED` cho phần đã làm, đúng phạm vi hồ sơ của mục đó.
- Mọi giới hạn trong cột Residual của tracker giữ nguyên. Cột "Còn mở" dưới đây chỉ là tóm tắt.
- Không mở cuộc gọi khách thật, không phê duyệt production, không đóng việc của M3, Sales,
  Platform hay Security.
- Bằng chứng giữ ghim `ca4f442`. Các commit tài liệu sau đó không được coi là lượt test mới.

## Nhóm A — 73 mục

### A1. Việc trong kế hoạch — tính vào Nấc 1 (9)

Phần đã làm chạy local hoặc MOCK. Phần còn lại thuộc Platform (runner, cluster, OTLP, staging), tier GitLab, M3/Sales hoặc quyết định của owner.

| Việc | Phần đã làm | Còn mở, thuộc ai |
| --- | --- | --- |
| [W-0013](../W-0013/README.md) | P0-4: cờ mode/provider và kill switch | OD-V1-20 chờ người ngoài M8 xác nhận; independent approval của W-0061 (tier GitLab) |
| [W-0014](../W-0014/README.md) | P1-1: OpenAPI hai phía, codegen, khung contract | contract còn TARGET_DRAFT; W-0002, W-0005, W-0006 chờ bên ngoài; independent approval W-0061 |
| [W-0030](../W-0030/README.md) | P4-2: kiểm hợp đồng blocker do Sales sở hữu | shape là đề xuất IVR, OD-V1-03 chờ M3; sandbox thật chưa chạy |
| [W-0035](../W-0035/README.md) | P5-1: bộ test unit, integration, Testcontainers | ánh xạ tới từng mục specs/testing thuộc P5-2 và P5-4 |
| [W-0038](../W-0038/README.md) | P5-4: cổng review, static, security | GitLab hosted chưa chạy (W-0061); SAST cần tier GitLab; ZAP thuộc P7-3 |
| [W-0042](../W-0042/README.md) | P6-3: diễn tập chaos, fail-closed, phục hồi (local) | staging, OTLP collector, Alertmanager (W-0063). Hai câu hỏi owner trong Residual đã đóng, xem mục đối soát W-0334 của hồ sơ |
| [W-0045](../W-0045/README.md) | P7-3: CI/CD, promotion, evidence (as-code) | chưa pipeline deploy nào chạy: runner, registry, cluster (W-0061, W-0063) |
| [W-0046](../W-0046/README.md) | P7-4: canary, rollback, ramp cờ (as-code) | canary và rollback chưa chạy: Argo Rollouts, Prometheus thuộc W-0063 |
| [W-0064](../W-0064/README.md) | P1-5: job retention và vòng đời dữ liệu | thời hạn lưu production chờ owner quyết (DF-07, OD-V1-11) |

### A2. Tính năng, API và tài liệu vận hành chạy local (26)

Không còn việc phía IVR trong phạm vi từng mục. Phần còn lại là chữ ký M3, Security, Platform, pháp chế hoặc việc owner gửi đi.

| Việc | Phần đã làm | Còn mở, thuộc ai |
| --- | --- | --- |
| [W-0087](../W-0087/README.md) | Sửa liên tục runtime Phase 1/2 | không SIM, không cuộc gọi thật |
| [W-0095](../W-0095/README.md) | API đọc cho admin | GitLab hosted chưa chạy |
| [W-0096](../W-0096/README.md) | API đọc back-office | dependency chưa thăm dò nên ghi NOT_WIRED; quyền thuộc Permission Core |
| [W-0098](../W-0098/README.md) | API đọc analytics | warehouse là P10-4 (W-0055), việc riêng |
| [W-0111](../W-0111/README.md) | Cắt ngang cuộc gọi đang diễn ra | giới hạn đã ghi: trễ theo chu kỳ poll; worker chết giữa chừng thì không cắt được |
| [W-0114](../W-0114/README.md) | Cổng rolling deploy không gãy giữa chừng | cổng không đọc dữ liệu đang có; phần đó thuộc A9 |
| [W-0129](../W-0129/README.md) | Truy vết lý do từ chối ở intake | phơi lý do chi tiết ra wire cần M3 và owner ký |
| [W-0131](../W-0131/README.md) | Test quá tải capacity 32 kênh, 800 job | hiệu chỉnh chờ đo cuộc gọi thật (W-0008); khối lượng chờ owner |
| [W-0144](../W-0144/README.md) | DT-04: cửa sổ lỗi và preflight adapter production | adapter production chờ vendor, Security, Vault/KMS |
| [W-0173](../W-0173/README.md) | ACK Target V1 sai định dạng thì fail-closed | M3, Security, Platform. Lượt chạy lại Postgres/Chaos mà Residual nhắc nay đã có trong collector ca4f442 |
| [W-0190](../W-0190/README.md) | Sửa API feature-flag (P0.1) | phê duyệt mutation, staging, production nằm ngoài |
| [W-0243](../W-0243/README.md) | Guard riêng cho public_name | đánh đổi đã ghi trong hồ sơ |
| [W-0290](../W-0290/README.md) | Chốt pause và kiểm chứng image E2E | chữ ký M3; lab, production, cuộc gọi thật chưa mở |
| [W-0298](../W-0298/README.md) | Chặn task mà mọi lần gọi rơi ngoài khung giờ | IR-06 nay đã ghi hành vi này (bổ sung W-0304) |
| [W-0299](../W-0299/README.md) | Trả cờ duyệt field Target V1 về NO | xác nhận rủi ro cấp công ty (OD-V1-11) |
| [W-0300](../W-0300/README.md) | Gỡ phê duyệt production khỏi migration | OD-V1-08 cần Product, Order Core, M3 đối ký |
| [W-0301](../W-0301/README.md) | RUNTIME_GATE_ADMIN phải nêu môi trường | OD-V1-20 chờ người ngoài M8 xác nhận |
| [W-0302](../W-0302/README.md) | Token hết hạn muộn hơn cửa sổ trả 422; draft.28 | DTK-02, DTK-06 chờ phần đơn hàng của M3 |
| [W-0305](../W-0305/README.md) | Sổ tay vận hành luồng quay số production | giá trị nhà mạng cần hợp đồng; bật lại kênh một chiều là quyết định của owner |
| [W-0306](../W-0306/README.md) | Lô vận hành 4 mục từ smoke 16/09 | nợ cổng image-selftest (cố định cổng) đã ghi riêng |
| [W-0307](../W-0307/README.md) | Endpoint audit-evidence | quyền riêng chờ DF-01; M3 chốt lọc và phân trang |
| [W-0308](../W-0308/README.md) | Nâng trần gọi đồng thời | giới hạn toàn hệ thống do bảng kênh giữ; values.yaml thuộc Platform |
| [W-0310](../W-0310/README.md) | Gộp phiếu hỏi M3 thành IR-07 | owner gửi phiếu |
| [W-0311](../W-0311/README.md) | Phương án B: M3 gửi thẳng phone_e164 | các stage sau chờ M3 chuyển sang |
| [W-0312](../W-0312/README.md) | draft.31: M3 gửi số là nhận được | owner gửi changelog; stage bắt buộc số chờ M3 |
| [W-0314](../W-0314/README.md) | Luồng xoá dữ liệu phủ phone_e164 | DSAR đã nghiệm thu ở W-0052 và W-0330; thời hạn lưu chờ pháp chế |

### A3. Công cụ kiểm chứng và chuỗi provenance (14)

Công cụ chạy offline, đã có self-test. Chưa có bản nộp, chữ ký hay phiếu nào từ bên ngoài để chúng kiểm.

| Việc | Phần đã làm | Còn mở, thuộc ai |
| --- | --- | --- |
| [W-0155](../W-0155/README.md) | Validator bốn bản nộp B1 | 0/4 bản nộp từ bên ngoài |
| [W-0156](../W-0156/README.md) | Receipt kiểm chứng B1 an toàn PII | chưa có receipt ngoài |
| [W-0157](../W-0157/README.md) | Bộ kiểm receipt độc lập B1 | trust anchor ngoài |
| [W-0158](../W-0158/README.md) | Ledger nhận receipt append-only B1 | checkpoint head ngoài |
| [W-0159](../W-0159/README.md) | Checkpoint ledger-head B1 | trust store ngoài |
| [W-0161](../W-0161/README.md) | Bằng chứng Postgres local cho W-0145, W-0148..W-0151 | chữ ký, E2E chung, production ngoài |
| [W-0162](../W-0162/README.md) | Bằng chứng Postgres/Chaos local cho W-0147 | phần còn lại đều ngoài |
| [W-0176](../W-0176/README.md) | Xoay pin chuỗi provenance W-0170 | GitLab hosted; 0/5 phiếu đã gửi |
| [W-0177](../W-0177/README.md) | Đóng băng candidate local | chữ ký, E2E chung ngoài |
| [W-0179](../W-0179/README.md) | Self-test luồng đóng S-06 (opt-out) | quyết định ngoài chưa nhận |
| [W-0184](../W-0184/README.md) | Self-test luồng đóng S-08 (contact, dial-token) | quyết định ngoài chưa nhận |
| [W-0186](../W-0186/README.md) | Khôi phục chuỗi provenance C5 | 0/5 phiếu đã gửi |
| [W-0187](../W-0187/README.md) | Validator bundle quyết định opt-out C9 | bundle và chữ ký ngoài |
| [W-0188](../W-0188/README.md) | Khôi phục provenance intake capacity B1 | dữ liệu 0/4, chữ ký ngoài |

### A4. Mục worklist 07/09 đã được quyết hoặc làm tiếp ở việc sau (11)

Câu hỏi mà Residual nêu đã có lời đáp; cột bên phải chỉ nơi đáp.

| Việc | Phần đã làm | Còn mở, thuộc ai |
| --- | --- | --- |
| [W-0208](../W-0208/README.md) | TTL dial_token mâu thuẫn ba tầng | owner đã chốt ở W-0246 |
| [W-0209](../W-0209/README.md) | Ràng buộc header lệch tài liệu và runtime | OpenAPI hiện hành đã khai ràng buộc |
| [W-0210](../W-0210/README.md) | DTMF-0 bị gọi nhầm là opt-out | trạng thái register là việc auditor; opt-out tường minh cần Legal |
| [W-0211](../W-0211/README.md) | Ba câu tài liệu sai về hệ thống | đã đóng hẳn |
| [W-0213](../W-0213/README.md) | Scope environment của approval | đã làm ở W-0301 |
| [W-0214](../W-0214/README.md) | Bẫy khung giờ compose, log thiếu, ba nhánh git | nhánh mốc W-0130 đã có miễn trừ trong CLAUDE.md; hệ quả khung giờ xử lý ở W-0298 |
| [W-0215](../W-0215/README.md) | Mốc cắt End của khung giờ | owner chốt ở W-0220 |
| [W-0220](../W-0220/README.md) | Thi hành End = 21:08 | trạng thái register là việc auditor; M3 xác nhận producer |
| [W-0221](../W-0221/README.md) | Siết Idempotency-Key admin theo intake | OpenAPI hiện hành đã khai ràng buộc |
| [W-0245](../W-0245/README.md) | Rút số TTL gán sai, re-pin validator | owner ký ở W-0246 |
| [W-0246](../W-0246/README.md) | Owner ký TTL bằng mép cửa sổ | không còn gì |

### A5. Vệ sinh contract và tài liệu (6)

Chỉ local, không đổi runtime.

| Việc | Phần đã làm | Còn mở, thuộc ai |
| --- | --- | --- |
| [W-0189](../W-0189/README.md) | Dọn lệch tài liệu và code | các phần lớn hơn là việc riêng |
| [W-0275](../W-0275/README.md) | adapter_mode thành enum | chỉ local |
| [W-0276](../W-0276/README.md) | Ba nguồn baseline nói giống nhau | chỉ local |
| [W-0277](../W-0277/README.md) | oasdiff soi enum | chỉ local; lượt đỏ có chủ ý đã đóng ở W-0279 |
| [W-0278](../W-0278/README.md) | Một field hai từ vựng | chỉ local |
| [W-0279](../W-0279/README.md) | Đóng trạng thái đỏ của W-0277 | chỉ local |

### A6. Việc quy trình nghiệm thu (7)

Công cụ và hồ sơ phục vụ nghiệm thu, đã dùng trong các lượt duyệt trước.

| Việc | Phần đã làm | Còn mở, thuộc ai |
| --- | --- | --- |
| [W-0324](../W-0324/README.md) | Sửa C2, hoàn thiện hồ sơ P2/P3 | xong |
| [W-0325](../W-0325/README.md) | Chỉ dấu C1 và phân loại hồ sơ | xong |
| [W-0327](../W-0327/README.md) | Rà Residual 13 việc | việc theo sau đã làm (W-0330, W-0332, W-0334) |
| [W-0328](../W-0328/README.md) | C2 đọc TestId viết tắt | xong |
| [W-0332](../W-0332/README.md) | E2E local cho W-0207 | W-0207 đã ACCEPTED; phần thật chờ M3 và credential |
| [W-0334](../W-0334/README.md) | Diễn tập cảnh báo local cho W-0042 | OTLP, Alertmanager, staging ngoài |
| [W-0346](../W-0346/README.md) | C2 nhận assertion của gate | xong |

## Nhóm B — Duyệt kèm điều kiện: mô hình capacity chưa hiệu chỉnh (5)

Gate capacity đạt ở mức chưa hiệu chỉnh, có khai báo. Duyệt nghĩa là nhận mô hình như một khung tính. Không dùng số của nó để mua kênh cho tới khi có số đo cuộc gọi thật (W-0008).

| Việc | Phần đã làm | Còn mở, thuộc ai |
| --- | --- | --- |
| [W-0054](../W-0054/README.md) | P10-3: mô hình capacity và chi phí 1/32 kênh | chưa hiệu chỉnh: chờ đo cuộc gọi thật (W-0008), dự báo khối lượng, báo giá |
| [W-0132](../W-0132/README.md) | Một nguồn khai báo thời lượng cuộc gọi | chưa hiệu chỉnh (W-0008); khối lượng chờ owner |
| [W-0134](../W-0134/README.md) | Độ dài phiên là input mở | M8-OD-C, OD-19 và arrival profile chờ owner |
| [W-0212](../W-0212/README.md) | Con số thứ tư của họ call-duration | chưa hiệu chỉnh (W-0008) |
| [W-0223](../W-0223/README.md) | Giữ UNCALIBRATED chờ W-0008 | owner đã chốt; hiệu chỉnh chờ W-0008 |

## Nhóm C — Chưa đưa vào phiếu (6)

Residual còn việc phía IVR, hoặc phạm vi cần rà lại trước.

| Việc | Nội dung | Vì sao chưa trình |
| --- | --- | --- |
| [W-0041](../W-0041/README.md) | P6-2: dashboard, SLO, alert | còn ba món as-code phía IVR chưa làm: alert backlog hàng đợi, panel burn-rate, panel integration-status; ngưỡng đề xuất chưa được duyệt |
| [W-0053](../W-0053/README.md) | P10-2: backup và DR | chạy một host; chưa có fencing và phép kiểm dựng lại standby; gate đạt có điều kiện |
| [W-0108](../W-0108/README.md) | Ghép audio động | dựa trên 12 đoạn MP3 cố định, trong khi W-0315 đã chuyển TTS sang chỉ VieNeu; cần rà lại phạm vi |
| [W-0249](../W-0249/README.md) | Hai fence thu hồi đơn | endpoint thu hồi chưa có trong OpenAPI |
| [W-0303](../W-0303/README.md) | Luồng quay số production PD-01 | Residual tự ghi còn thiếu test ghim cổng quay số production đang đóng |
| [W-0335](../W-0335/README.md) | Chuẩn bị lời thoại và đo trên S5 | bản sửa chưa deploy lên worker S5; budget cuối chưa chốt |

## Kiểm lại theo cây hiện tại

Sáu Residual nhắc tới việc có thể đã làm sau đó. Lượt đọc đối chiếu chúng với cây hiện tại:
- W-0209, W-0221: OpenAPI hiện hành đã khai `minLength`, `maxLength` và `pattern` cho header
  idempotency và correlation.
- W-0298: IR-06 đã có mục bổ sung của W-0304 về task bị chặn trên cả cửa sổ xác nhận.
- W-0042: mục "Đối soát sau W-0327" trong hồ sơ ghi hai câu hỏi owner cũ đã đóng.
- W-0249: OpenAPI chưa có endpoint thu hồi, nên mục này ở nhóm C.
- W-0303: chưa có test nào ghim cổng quay số production đang đóng, nên mục này ở nhóm C.

## Danh sách để trả lời

- Nhóm A: W-0013, W-0014, W-0030, W-0035, W-0038, W-0042, W-0045, W-0046, W-0064, W-0087, W-0095, W-0096, W-0098, W-0111, W-0114, W-0129, W-0131, W-0144, W-0173, W-0190, W-0243, W-0290, W-0298, W-0299, W-0300, W-0301, W-0302, W-0305, W-0306, W-0307, W-0308, W-0310, W-0311, W-0312, W-0314, W-0155, W-0156, W-0157, W-0158, W-0159, W-0161, W-0162, W-0176, W-0177, W-0179, W-0184, W-0186, W-0187, W-0188, W-0208, W-0209, W-0210, W-0211, W-0213, W-0214, W-0215, W-0220, W-0221, W-0245, W-0246, W-0189, W-0275, W-0276, W-0277, W-0278, W-0279, W-0324, W-0325, W-0327, W-0328, W-0332, W-0334, W-0346.
- Nhóm B: W-0054, W-0132, W-0134, W-0212, W-0223.
