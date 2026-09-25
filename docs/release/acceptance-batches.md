# Danh sách đề nghị nghiệm thu — `da97faf78307e595e6aeb6e2c3779e8a87c2d04a`

Script **chỉ đọc**: không sửa tracker. Chỉ Toàn chuyển một dòng sang `ACCEPTED`, sau khi
đọc bằng chứng — danh sách này không thay cho việc đọc. `ACCEPTED` ở đây là **tự nghiệm thu** của dev M8,
không phải nghiệm thu độc lập, và không thay chữ ký của M3, Tech Lead hay Sếp.

| Nguồn | Trạng thái |
| --- | --- |
| Kết quả test (`C2`) | 4 file `.trx`, 1337 kết quả · SHA/hash/đủ project đã kiểm |
| Gate sweep (`C4`) | ✅ GATE_SWEEP_PASS 44/44 |
| Gate mở rộng: image, K8s, oasdiff, security scan (`C2`) | ✅ EXTENDED_SWEEP_PASS 5/5 |

## Nấc 1

Prompt đã lên kế hoạch đã xong (`ACCEPTED`, `N/A`, `CANCELLED`): **46/54**
· `BLOCKED_INTERNAL`: **0**. Nấc 1 đạt khi hai số này là
**54/54** và **0**.

## Tổng theo đợt

| Đợt | Ứng viên | ĐẠT | XEM | KHÔNG ĐẠT | CHƯA KIỂM |
| --- | ---: | ---: | ---: | ---: | ---: |
| `P5` | 1 | 0 | 1 | 0 | 0 |
| `UNPLANNED` | 12 | 0 | 12 | 0 | 0 |

`ĐẠT`: đủ bốn điều. `XEM`: C1, C2, C4 đạt, còn cột Residual hoặc một ghi chú cần Toàn đọc (điều kiện
của gate, việc tài liệu, ID đã gỡ hay chỉ được nhắc). `KHÔNG ĐẠT`: hỏng ít nhất một điều, lý do ở dưới. `CHƯA KIỂM`: thiếu kết quả test
hoặc log gate sweep để kết luận.

## Đợt `P5`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0037` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0037` — PT-SOAK-02 được chứng minh bằng lượt chạy đã ghi, không phải test trong suite: docs/evidence/W-0037/pt-soak-02.json · Residual: Sửa 25/09, tối: soak PT-SOAK-02 PASS ở lượt thứ tư (d989c31, 19:35–23:35, 1092 vòng, 12 337 task, 0 lỗi, docs/evidence/W-0037/pt-soak-02.json, README §10); ba lượt trước dừng giữa chừng, không verdict (§8, §9). Quý đầ…

## Đợt `UNPLANNED`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0171` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0335` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0347` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0353` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0354` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0355` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0356` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0357` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0358` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0359` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0360` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0361` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |

- `W-0171` — Việc tài liệu, không có test phần mềm (5 tài liệu có tại commit): Đối soát tài liệu theo code: sửa spec database, functional, workflow và ui cho khớp constraint và quyền thật; ngoài tài liệu chỉ sửa một comment trong DevToolingEndpoints.cs. · Residual: Sửa 18/09 (W-0316, Lô 3 mục 12): (a) đã xử lý ở W-0312 (draft.31, T5) — OAS ghi quyền endpoint thực đòi, IVR_DEV_TOOLING không còn trong spec. (c) đã xanh: progressive-selftest đọc migration thật và chạy trong gate sw…
- `W-0335` — Residual: Bản sửa chưa deploy worker vận hành. Soak dùng15s/phần, max10.099s; recovery dùng30s/phần, max19.352s gồm busy. Chưa chốt budget cuối hoặc nghiệm thu scheduler/SIP S5; S2/mirror mở; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0347` — Residual: W-0012, W-0015, W-0031 đã ACCEPTED (A-0773..A-0775). Chờ Toàn quyết: W-0036 bảy ID §8 theo nhóm (OpenAPI parse/ref/enum, task schema + policy mismatch); W-0037 vế rate-limit và soak dài; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0353` — Gate đạt có điều kiện: DG-DR-03 PASS_SINGLE_HOST: RPO=0 và RTO trong ngân sách, nhưng trên một host hai container, không phải multi-AZ; DG-DR-REBUILD-05 PASS_SINGLE_HOST: dựng lại standby đồng bộ sau failover đưa RPO về 0, nhưng trên một host hai container, không phải multi-AZ · ID chỉ được nhắc, không phải claim của việc này: PT-SOAK-02; lý do trong acceptance-tests.json · Residual: W-0037 và W-0347 chờ lượt soak bốn giờ; W-0121, W-0171 và W-0335 chờ Toàn hoặc bên ngoài, lý do trong closeout.json; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0354` — Residual: C22 chờ nguyên văn bảng ivr-cancel-reason-map.v1; C23, N1, B8 (2)–(4), A1 (1)–(3), (5), (6) và B17 chờ Toàn quyết; B7, C13, C17 chờ M3 trả lời M3-14; A2, B11 (1)–(4), C16, C19 chờ Sếp (mục B2 phiếu 25/09); C18 chờ chi…
- `W-0355` — Residual: W-0055 được ghi ACCEPTED sáng 25/09 (A-1008) dựa trên BI-DRIFT-06, trong khi lỗi (3) nằm ở chính phần đó; giữ hay mở lại là Toàn quyết; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0356` — Residual: Không còn việc IVR trong lô; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0357` — Residual: Dẫn FIX_OWNER_CORE.md:160 cho D07 chưa làm vì file không có trên máy dev; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0358` — Residual: C22 chờ nguyên văn bảng v1 từ chief; K-52 (job CAPACITY_HELD không bao giờ đóng, không callback) tách việc riêng; gửi lại IR-07 cho anh Mạnh là việc của Toàn; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0359` — Residual: Phần còn lại của K-31 là W-0360 (L5); chính sách guard tên hàng và tên khách là Q-12; RecordHealthy xoá chuỗi lỗi SIM là K-45 đã có; ExceptionType ngoài allowlist log là K-53 mới; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0360` — Residual: AnyLiveAsync trong RuntimeGateApprovals.cs không còn nơi gọi từ SIP-04 và chú thích của AnyLiveForEnvironmentAsync ngược với IT-GATE-APPROVAL-10, nên dọn riêng; phần TryHangupAsync thuộc W-0359; k8s-selftest và image-…
- `W-0361` — Residual: Lối quay production của test chặn số là Q-28.2; mã hoá hoặc xoá số sau N ngày là Q-32 (Sếp); câu “không bao giờ thấy số” của inventory là CB-10; REAL_CUSTOMER_CALL_ALLOWED=NO

