# Danh sách đề nghị nghiệm thu — `b92942e98fec9d82fccdafc13cb63f2654a15da1`

Script **chỉ đọc**: không sửa tracker. Chỉ Toàn chuyển một dòng sang `ACCEPTED`, sau khi
đọc bằng chứng — danh sách này không thay cho việc đọc.

> ⚠️ Cây làm việc có 1 file chưa commit. Danh sách đọc từ commit b92942e98fec9d82fccdafc13cb63f2654a15da1; chỉ nhận kết quả đã thu trên cây sạch tại đúng commit đó.

| Nguồn | Trạng thái |
| --- | --- |
| Kết quả test (`C2`) | 4 file `.trx`, 1233 kết quả · SHA/hash/đủ project đã kiểm |
| Gate sweep (`C4`) | ✅ GATE_SWEEP_PASS 43/43 |
| Gate mở rộng: image, K8s, oasdiff, security scan (`C2`) | ✅ EXTENDED_SWEEP_PASS 5/5 |

## Nấc 1

Prompt đã lên kế hoạch đã xong (`ACCEPTED`, `N/A`, `CANCELLED`): **41/54**
· `BLOCKED_INTERNAL`: **0**. Nấc 1 đạt khi hai số này là
**54/54** và **0**.

## Tổng theo đợt

| Đợt | Ứng viên | ĐẠT | XEM | KHÔNG ĐẠT | CHƯA KIỂM |
| --- | ---: | ---: | ---: | ---: | ---: |
| `P0` | 1 | 0 | 1 | 0 | 0 |
| `P5` | 2 | 0 | 1 | 1 | 0 |
| `P6` | 1 | 0 | 1 | 0 | 0 |
| `P10` | 2 | 0 | 2 | 0 | 0 |
| `UNPLANNED` | 13 | 0 | 13 | 0 | 0 |

`ĐẠT`: đủ bốn điều. `XEM`: C1, C2, C4 đạt, còn cột Residual hoặc một ghi chú cần Toàn đọc (điều kiện
của gate, việc tài liệu, ID đã gỡ hay chỉ được nhắc). `KHÔNG ĐẠT`: hỏng ít nhất một điều, lý do ở dưới. `CHƯA KIỂM`: thiếu kết quả test
hoặc log gate sweep để kết luận.

## Đợt `P0`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0011` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0011` — Residual: CI + hosted enforcement complete except required independent approval; giữ TESTS_PASS tới khi Premium/Ultimate + second reviewer proof đóng W-0061

## Đợt `P5`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0036` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0037` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0036` — Residual: không có Pact — khác biệt có chủ ý, pact chỉ có giá trị khi cả hai bên cùng chạy mà Sales chưa có gì; không có E2E trình duyệt — pane preview không composite frame; provider thật vẫn BLOCKED_EXTERNAL
- `W-0037` — PT-SOAK-02 không có trong docs/traceability-tests.md · Residual: rate limiting CHƯA CÓ (chỉ có ánh xạ 429, không middleware) — ngưỡng là quyết định vận hành chưa ai duyệt; soak 4–8h NOT_RUN; không tuyên bố ngưỡng latency D-04 vì cần Sales thật trong vòng lặp; năng lực 32 kênh thật …

## Đợt `P6`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0041` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0041` — Gate đạt có điều kiện: CAP-ALERT-04 PASS_WITH_NOT_PROVEN=COST_METRIC: pool prod khớp đỉnh của mô hình; cost_per_confirmed_order chưa đo vì còn chờ báo giá (W-0008) · Residual: chưa exporter OTLP (W-0063) nên chưa tín hiệu nào rời tiến trình — đây là artifact as-code đã qua bộ đánh giá luật thật, KHÔNG phải hệ đang chạy; không có screenshot dashboard (§10) vì không có Grafana để chụp và tôi …

## Đợt `P10`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0053` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0055` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0053` — Gate đạt có điều kiện: DG-DR-03 PASS_SINGLE_HOST: RPO=0 và RTO trong ngân sách, nhưng trên một host hai container, không phải multi-AZ; DG-DR-REBUILD-05 PASS_SINGLE_HOST: dựng lại standby đồng bộ sau failover đưa RPO về 0, nhưng trên một host hai container, không phải multi-AZ · Residual: không multi-AZ — một host, hai container; không mã hoá volume at-rest và không KMS (W-0063); không PITR (logical dump); RTO chưa gồm phát hiện/quyết định/chuyển traffic; chưa có fencing nên promote khi chỉ mất liên lạ…
- `W-0055` — Residual: không có kho dữ liệu riêng — là schema analytics trong cùng database (W-0063), lệch với P10-4 §7 vì DTS-04 chốt 3 deployable; không BI tool nào từng kết nối (grant SELECT là thứ cấp được, chưa ai cấp); chưa đo trên kh…

## Đợt `UNPLANNED`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0078` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0094` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0108` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0168` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0169` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0171` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0205` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0225` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0249` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0297` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0303` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0335` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0347` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |

- `W-0078` — Residual: retain pin until Testcontainers declares a patched dependency; no production SSH/SIM/Sales execution
- `W-0094` — Residual: recording readback retained as defense in depth; provider boundaries/no-egress preserved
- `W-0108` — Residual: Lỗi contract duy nhất không thuộc W-0108: luồng OD-V1-20 sửa 1 dòng summary trong OpenAPI mà chưa re-pin contract-manifest.json (98d226b1… → 7b921b3a…); khôi phục file về bản commit ⇒ contract 22/22. 12 MP3 đoạn cố đị…
- `W-0168` — Residual: LOCAL_SECURITY_GATE_PASS / HOSTED_CI_NOT_RUN. GitNexus pre-edit impact LOW 0 process; no app/runtime/OpenAPI/DB/prod-config change. External secret custody/vendor/staging/UAT/production unchanged; REAL_CUSTOMER_CALL_A…
- `W-0169` — Việc tài liệu, không có test phần mềm (1 tài liệu có tại commit): Dọn tài liệu theo chỉ đạo owner ở commit c213bf7: xoá 63 tệp SUPERSEDED/HISTORICAL, sửa link, thêm .artifacts/ vào .gitignore; không đổi code hay test. README này ghi lại lượt dọn và phép kiểm. · Residual: Cảnh báo tồn đọng: (a) W-0105 và W-0118 giờ evidence: null trong gate-status.yaml — W-0118 vẫn mang status TESTS_PASS mà không còn evidence pack, owner cần quyết định hạ status hay khôi phục evidence; (b) OD-20 yêu cầ…
- `W-0171` — Việc tài liệu, không có test phần mềm (5 tài liệu có tại commit): Đối soát tài liệu theo code: sửa spec database, functional, workflow và ui cho khớp constraint và quyền thật; ngoài tài liệu chỉ sửa một comment trong DevToolingEndpoints.cs. · Residual: Sửa 18/09 (W-0316, Lô 3 mục 12): (a) đã xử lý ở W-0312 (draft.31, T5) — OAS ghi quyền endpoint thực đòi, IVR_DEV_TOOLING không còn trong spec. (c) đã xanh: progressive-selftest đọc migration thật và chạy trong gate sw…
- `W-0205` — Residual: Một phát hiện phụ đáng giữ: ApiBehaviorMatrixTests đỏ trên clone sạch vì thiếu deploy/ci/node_modules — nó fail chứ không tự skip, đúng như evidence W-0197 tuyên bố. Nghĩa là freeze checkout phải chạy npm --prefix dep…
- `W-0225` — Residual: Gate thứ hai tìm thấy vì nó nói dối: verify-api-behavior-matrix.mjs in "verdict": "FAIL" ngay trên "failures": [] với 38/38 pass, vì verdict quyết bởi mảng failures cấp cao còn console.log in mảng failures theo từng o…
- `W-0249` — Residual: Đề xuất m8-17 §5 của tôi sai và đã sửa trước khi dựng: nó viết "revoke mang version ≤ version đã lưu thì từ chối", nhưng order_version là chuỗi mờ IVR trả nguyên — OAS gọi là stale-result guard snapshot, IR-06 ghi "IV…
- `W-0297` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): Chỉ dồn, xoá tài liệu kế hoạch trong plan/ivr-orther và viết lại link; commit 257cbef không chạm src/, tests/ hay script gate nên không có khẳng định phần mềm nào để test kiểm. · Residual: Phân loại sai một lần và đã sửa: m8-05…m8-10 mở đầu bằng M8_OWNER_SIGNED/đã ký nên lượt đọc đầu xếp vào nhóm xong — đọc hết chuỗi trạng thái thì cả sáu đều kèm M3_..._SIGNOFF_REQUIRED hoặc CODE_NOT_AUTHORIZED, tức mới…
- `W-0303` — Residual: Vault production không tự làm protector — lab là protector của chính nó vì bảo vệ của nó là vân tay tự sinh được; production nhận khóa Platform, nên deployment còn UnavailableOpaqueValueProtector fail closed ngay lần …
- `W-0335` — Residual: Bản sửa chưa deploy worker vận hành. Soak dùng15s/phần, max10.099s; recovery dùng30s/phần, max19.352s gồm busy. Chưa chốt budget cuối hoặc nghiệm thu scheduler/SIP S5; S2/mirror mở; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0347` — Residual: W-0012, W-0015, W-0031 đã ACCEPTED (A-0773..A-0775). Chờ Toàn quyết: W-0036 bảy ID §8 theo nhóm (OpenAPI parse/ref/enum, task schema + policy mismatch); W-0037 vế rate-limit và soak dài; REAL_CUSTOMER_CALL_ALLOWED=NO

