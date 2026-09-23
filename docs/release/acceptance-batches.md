# Danh sách đề nghị nghiệm thu — `185924339be3af81e7126bd10fb9d330effa0479`

Script **chỉ đọc**: không sửa tracker. Chỉ Toàn chuyển một dòng sang `ACCEPTED`, sau khi
đọc bằng chứng — danh sách này không thay cho việc đọc.

| Nguồn | Trạng thái |
| --- | --- |
| Kết quả test (`C2`) | 4 file `.trx`, 1200 kết quả · SHA/hash/đủ project đã kiểm |
| Gate sweep (`C4`) | ✅ GATE_SWEEP_PASS 43/43 |

## Nấc 1

Prompt đã lên kế hoạch đã xong (`ACCEPTED`, `N/A`, `CANCELLED`): **35/54**
· `BLOCKED_INTERNAL`: **0**. Nấc 1 đạt khi hai số này là
**54/54** và **0**.

## Tổng theo đợt

| Đợt | Ứng viên | ĐẠT | XEM | KHÔNG ĐẠT | CHƯA KIỂM |
| --- | ---: | ---: | ---: | ---: | ---: |
| `P0` | 1 | 0 | 0 | 1 | 0 |
| `P1` | 1 | 0 | 1 | 0 | 0 |
| `P5` | 3 | 0 | 0 | 3 | 0 |
| `P6` | 2 | 0 | 1 | 1 | 0 |
| `P7` | 3 | 0 | 0 | 3 | 0 |
| `P10` | 2 | 0 | 1 | 1 | 0 |
| `UNPLANNED` | 128 | 0 | 99 | 29 | 0 |

`ĐẠT`: đủ bốn điều. `XEM`: C1, C2, C4 đạt, còn cột Residual, điều kiện kèm PASS của gate, hoặc
nội dung của việc thuần tài liệu cần Toàn đọc. `KHÔNG ĐẠT`: hỏng ít nhất một điều, lý do ở dưới. `CHƯA KIỂM`: thiếu kết quả test
hoặc log gate sweep để kết luận.

## Đợt `P0`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0011` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0011` — CT-CI-04 không có trong docs/traceability-tests.md · Residual: CI + hosted enforcement complete except required independent approval; giữ TESTS_PASS tới khi Premium/Ultimate + second reviewer proof đóng W-0061

## Đợt `P1`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0016` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0016` — Residual: local MOCK complete; Target V1/policy approvals and real Sales/SIM/customer calls remain external/NOT_RUN; W-0061 push setting PASS, independent approval remains

## Đợt `P5`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0036` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0037` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0039` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0036` — CT-OAS-01 không có trong docs/traceability-tests.md; CT-OAS-02 không có trong docs/traceability-tests.md; CT-OAS-03 không có trong docs/traceability-tests.md; và 4 TestId khác · Residual: không có Pact — khác biệt có chủ ý, pact chỉ có giá trị khi cả hai bên cùng chạy mà Sales chưa có gì; không có E2E trình duyệt — pane preview không composite frame; provider thật vẫn BLOCKED_EXTERNAL
- `W-0037` — PT-SOAK-02 không có trong docs/traceability-tests.md; SEC-AUTHZ-05 không có trong docs/traceability-tests.md; SEC-ERR-06 không có trong docs/traceability-tests.md · Residual: rate limiting CHƯA CÓ (chỉ có ánh xạ 429, không middleware) — ngưỡng là quyết định vận hành chưa ai duyệt; soak 4–8h NOT_RUN; không tuyên bố ngưỡng latency D-04 vì cần Sales thật trong vòng lặp; năng lực 32 kênh thật …
- `W-0039` — UI-A11Y-01 không có trong docs/traceability-tests.md; UI-I18N-02 không có trong docs/traceability-tests.md; UI-VISUAL-04 không có trong docs/traceability-tests.md; và 2 TestId khác · Residual: UI-XBROWSER-03 và visual-regression NOT_RUN — pane preview không composite frame, không thêm job CI cho việc không chạy được (sẽ đỏ vĩnh viễn rồi bị allow_failure hoá); axe không dùng (cần DOM thật) — thay bằng kiểm c…

## Đợt `P6`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0040` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0041` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0040` — IT-OBS-TRACE-02 không có trong docs/traceability-tests.md · Residual: OTLP export BLOCKED_EXTERNAL (W-0063) — instrumentation dùng BCL nên gắn exporter sau không sửa call site; mới instrument 1/5 chặng (callback), 4 chặng còn lại chưa có span; dependency_probing_available vẫn false — đó…
- `W-0041` — Gate đạt có điều kiện: CAP-ALERT-04 PASS_WITH_NOT_PROVEN=COST_METRIC: pool prod khớp đỉnh của mô hình; cost_per_confirmed_order chưa đo vì còn chờ báo giá (W-0008) · Residual: chưa exporter OTLP (W-0063) nên chưa tín hiệu nào rời tiến trình — đây là artifact as-code đã qua bộ đánh giá luật thật, KHÔNG phải hệ đang chạy; không có screenshot dashboard (§10) vì không có Grafana để chụp và tôi …

## Đợt `P7`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0043` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0044` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0047` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0043` — IT-IMG-BUILD-01 không có trong docs/traceability-tests.md; IT-IMG-COMPOSE-03 không có trong docs/traceability-tests.md; IT-IMG-E2E-05 không có trong docs/traceability-tests.md; và 4 TestId khác · Residual: chưa có registry (§5 NEED_CONFIRMATION) nên chưa push lần nào; ~~compose smoke chưa chạy hết luồng DTMF tới callback; chưa seed attempt policy~~ đã đóng 2026-08-19 bằng IT-IMG-E2E-05 + deploy/docker/dev-seed/seed.sql …
- `W-0044` — IT-K8S-GATE-02 không có trong docs/traceability-tests.md; IT-K8S-LINT-01 không có trong docs/traceability-tests.md; IT-K8S-NETPOL-04 không có trong docs/traceability-tests.md; và 2 TestId khác · Residual: ~~IT-K8S-NETPOL-04 NOT_PROVEN~~ đã đóng 2026-08-19 — kết luận cũ "cluster không thực thi, cần Calico/Cilium" là sai: cluster có thực thi, phép đo bị đua với thời điểm kube-router cài luật iptables cho pod. Nay 5/5 K8S…
- `W-0047` — IT-K8S-NETPOL-04 không có trong docs/traceability-tests.md; IT-K8S-ROTATE-07 không có trong docs/traceability-tests.md · Residual: drill overlap đã chạy trên HTTP thật: token cũ được nhận 21/21 lượt trong cửa sổ rồi chuyển 403 đúng tại thời điểm hết hạn, token mới 0/28 lượt bị 403, token chưa cấu hình 403 cả 28 lượt — ~~nhưng drill chạy trên một …

## Đợt `P10`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0053` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0055` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0053` — Gate đạt có điều kiện: DG-DR-03 PASS_SINGLE_HOST: RPO=0 và RTO trong ngân sách, nhưng trên một host hai container, không phải multi-AZ · Residual: không multi-AZ — một host, hai container; không mã hoá volume at-rest và không KMS (W-0063); không PITR (logical dump); RTO chưa gồm phát hiện/quyết định/chuyển traffic; chưa có fencing nên promote khi chỉ mất liên lạ…
- `W-0055` — CT-DOC-02 không có trong docs/traceability-tests.md · Residual: không có kho dữ liệu riêng — là schema analytics trong cùng database (W-0063), lệch với P10-4 §7 vì DTS-04 chốt 3 deployable; không BI tool nào từng kết nối (grant SELECT là thứ cấp được, chưa ai cấp); chưa đo trên kh…

## Đợt `UNPLANNED`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0078` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0079` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0080` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0081` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0082` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0083` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0084` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0089` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0090` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0091` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0092` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0094` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0097` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0099` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0100` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0101` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0103` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0108` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0110` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0112` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0113` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0115` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0116` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0117` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0123` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0124` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0126` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0127` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0128` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0130` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0135` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0136` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0137` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0138` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0139` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0140` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0141` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0164` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0165` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0167` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0168` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0170` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0172` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0174` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0178` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0180` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0181` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0182` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0183` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0185` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0191` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0192` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0193` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0194` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0195` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0198` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0199` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0200` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0201` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0202` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0203` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0205` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0206` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0216` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0217` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0218` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0219` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0222` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0224` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0225` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0226` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0227` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0247` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0248` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0249` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0250` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0251` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0252` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0253` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0254` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0265` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0266` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0267` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0270` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0273` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0280` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0281` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0282` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0283` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0284` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0286` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0287` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0288` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0289` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0291` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0293` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0294` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0295` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0296` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0297` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0303` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0304` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0309` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0313` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0315` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0316` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0317` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0318` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0319` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0320` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0321` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0322` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0323` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0326` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0329` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0331` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0333` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0335` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0336` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0337` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0340` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0341` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0342` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0343` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0344` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0345` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0347` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0348` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0078` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: retain pin until Testcontainers declares a patched dependency; no production SSH/SIM/Sales execution
- `W-0079` — Residual: local implementation complete; hosted GitLab evidence vẫn NOT_RUN dưới W-0061
- `W-0080` — Residual: local implementation complete; hosted artifact topology vẫn NOT_RUN dưới W-0061
- `W-0081` — Residual: local catalog complete; Target V1 remains DRAFT and owner acceptance is separate
- `W-0082` — Residual: HIGH blast radius regression passed locally; production runtime proof remains outside this work
- `W-0083` — Residual: local guard complete; any new source project/reference must update reviewed matrix
- `W-0084` — Residual: local PostgreSQL proof complete; no staging/production database mutation performed
- `W-0089` — Residual: Target V1 remains DRAFT; no external contract approval implied
- `W-0090` — Residual: callbacks remain disabled by default and external Sales is NOT_RUN
- `W-0091` — Residual: evidence labels now match asserted behavior; real providers remain NOT_RUN
- `W-0092` — CT-DOC-02 không có trong docs/traceability-tests.md · Residual: contract stays TARGET_CONTRACT_V1=DRAFT; Sales approval remains BLOCKED_EXTERNAL
- `W-0094` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: recording readback retained as defense in depth; provider boundaries/no-egress preserved
- `W-0097` — UI đã ngừng phạm vi theo quyết định đã ghim; hồ sơ đề nghị CANCELLED, chờ Toàn. Test backend không chứng nhận UI. · Residual: chỉ thay đổi trình bày, không đụng API/contract/permission/governance; skill cài ở user level chứ không vào repo; KHÔNG tải web font; KHÔNG dùng green làm CTA (green giữ nghĩa healthy); component library vẫn NEED_CONF…
- `W-0099` — E2E-UI-LOG-01 không có trong docs/traceability-tests.md; UT-UI-SIM-05 không có trong docs/traceability-tests.md · Residual: roster KHÔNG chiếu sim_number_ref (D-05) và KHÔNG chiếu lease/fencing (cơ chế scheduler, tránh can thiệp tay); chỉ hiện control có nghĩa theo trạng thái + RequirePermission; tắt kênh đang bận được chấp nhận nhưng copy…
- `W-0100` — E2E-UI-REVIEW-05 không có trong docs/traceability-tests.md; UT-UI-CONTRACT-06 không có trong docs/traceability-tests.md; UT-UI-ROLE-04 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: guard drift mới bắt lỗi ngay lần chạy đầu theo chiều ngược: rule substring gắn cờ nhầm invalid_phone (là số đếm result type, không phải số ĐT) → chuyển sang khớp tên chính xác; E2E dashboard đỏ đúng lúc W-0099 thêm re…
- `W-0101` — E2E-UI-DETAIL-02 không có trong docs/traceability-tests.md; E2E-UI-LOG-01 không có trong docs/traceability-tests.md · Residual: attempt_two_pending cố ý không đặt tên "due": due-ness cần offset schedule riêng từng job, aggregate không parse; call_success_rate = confirmed+cancelled+wrong_input (huỷ vẫn tính là gọi thành công) — công thức ghi th…
- `W-0103` — IT-IMG-E2E-05 không có trong docs/traceability-tests.md · Residual: MOCK-only; REAL_CUSTOMER_CALL_ALLOWED=NO; Sales endpoint/auth/real payload + one-SIM lab + 32-eSIM provisioning vẫn NOT_RUN/BLOCKED_EXTERNAL; Target V1 vẫn DRAFT; bước kế tiếp W-0048/one-SIM lab chỉ bắt đầu sau khi nh…
- `W-0108` — Residual: Lỗi contract duy nhất không thuộc W-0108: luồng OD-V1-20 sửa 1 dòng summary trong OpenAPI mà chưa re-pin contract-manifest.json (98d226b1… → 7b921b3a…); khôi phục file về bản commit ⇒ contract 22/22. 12 MP3 đoạn cố đị…
- `W-0110` — E2E-UI-FLAGS-06 không có trong docs/traceability-tests.md; UT-FLAGS-ASYMMETRY-01 không có trong docs/traceability-tests.md · Residual: Phát hiện: chốt tự-duyệt-đích hiện KHÔNG có hiệu lực với phiên console — RejectSelfAuthorization phụ thuộc claim ivr_destination_ref mà chỉ MockPermissionAuthenticationHandler cấp; đề xuất mở OD-FLAG-01. Không đổi bac…
- `W-0112` — UI-I18N-02 không có trong docs/traceability-tests.md; UT-L10N-COVER-03 không có trong docs/traceability-tests.md; UT-UI-SEED-PROD-03 không có trong docs/traceability-tests.md · Residual: Phát hiện: file mẫu sales-target-v1.sample.json đã hết hạn — mốc tuyệt đối 12/8/2026, nạp nguyên trạng thì cả 9 tác vụ bị từ chối vì cửa sổ hết hạn; loader dời cửa sổ từng tác vụ về hiện tại và ghi rõ trong phản hồi. …
- `W-0113` — UT-UI-VOICE-05 không có trong docs/traceability-tests.md · Residual: Ba trường giọng ở mức attempt ghi kể cả khi null, trái luật bỏ-null toàn cục của API: trường vắng mặt và trường null là không phân biệt được với người đọc, mà 'có được ghi không' đúng là câu hỏi việc này sinh ra để tr…
- `W-0115` — UT-SCHEMA-BACKCOMPAT-04 không có trong docs/traceability-tests.md · Residual: Production distinct-value scan=OWNER_DATA_REQUIRED; A10 giữ OWNER_DECISION_REQUIRED; không đổi contract/API; không sinh evidence SIM/carrier/real-call; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0116` — CT-DOC-02 không có trong docs/traceability-tests.md; UT-UI-CONTRACT-06 không có trong docs/traceability-tests.md · Residual: openapi:lint còn 14 lỗi baseline ngoài W-0116; hosted CI/deploy/UAT NOT_RUN; không sinh SIM/carrier/real-call evidence; REAL_CUSTOMER_CALL_ALLOWED=NO; không push
- `W-0117` — CT-DOC-02 không có trong docs/traceability-tests.md · Residual: Không đổi business semantics hay quyền; hosted CI/deploy/UAT và external closures NOT_RUN; không gọi khách thật; REAL_CUSTOMER_CALL_ALLOWED=NO; không nhận external closure pack vào scope
- `W-0123` — CT-DOC-02 không có trong docs/traceability-tests.md · Residual: Conservative compatibility: deprecate/ignore wire fields, giữ legacy DB read/rollback, không drop; M3 usage/sign-off OWNER_DATA_REQUIRED; target DB OWNER_DATA_REQUIRED / TARGET_DB_NOT_RUN; hosted CI NOT_RUN; local wor…
- `W-0124` — CT-DOC-02 không có trong docs/traceability-tests.md · Residual: .NET 804/804; job api_contract_diff full exit 0 (trước đó exit 1 cả tại HEAD lẫn W-0123); UI typecheck/223/223/build; 7 node gate; traceability 487; detect-changes HIGH 200/85/12 process — toàn bộ do một call site add…
- `W-0126` — Residual: Không gate external nào được đóng. Image sha256:4c76d318… thành STALE_REBUILD_REQUIRED vì build từ context CRLF cũ; Tám phát hiện đều đã xử lý. F7 chỉ giảm nhẹ được bằng tài liệu: 11 WAV audition và model bundle vẫn c…
- `W-0127` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): W-0127 chỉ viết phiếu hỏi, ghi lượt dựng thử profile nghe và lượt quét lại Trivy, và thêm một script lab sinh manifest cho Owner; không sửa src/, tests/ hay gate CI nên không có claim phần mềm để test tự động. · Residual: Không gate nào được đóng. Owner vẫn phải nghe; Legal/Security/Infra/Platform vẫn phải trả lời phiếu. REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0128` — Residual: M3 role→tier/sign-off/client/shared E2E, Platform custody/labels, hosted CI/target DB/deploy/UAT/production remain external; migration W0122 name retained as historical alias; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0130` — Việc tài liệu, không có test phần mềm (1 tài liệu có tại commit): W-0130 chỉ dựng lại và đóng băng đúng delta của W-0128/W-0129 lên một commit cô lập làm mốc provenance; bản thân nó không viết mã, test hay gate mới trên main, và kết quả test trên candidate thuộc về W-0128/W-0129. · Residual: Branch chưa push/merge; hosted CI, target DB/deploy/UAT và M3/Platform approvals vẫn external; không rewrite main@2a4f45d; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0135` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): W-0135 chỉ sửa ba lỗi sự thật (mốc tắt 3G, số lần gọi chưa ký, số kênh chưa chốt) trong hồ sơ mua sắm R-00/R-06; không đụng mã, test hay gate nên không có claim phần mềm. · Residual: Không tự chốt số kênh hay attempt policy — W-0135 chỉ gỡ fact sai và trả các con số chưa ký về đúng trạng thái chưa ký. Mốc 2G 15/09/2026 giữ nguyên vì tra lại thấy đúng; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0136` — Việc tài liệu, không có test phần mềm (2 tài liệu có tại commit): W-0136 chỉ thêm yêu cầu VoLTE (#0) vào §13.2, đổi tên thiết bị trong luồng tích hợp và ghi errata 21 trong spec nguồn Module 8; không sửa mã, test hay gate. · Residual: .docx cùng tên không được cập nhật — đúng thông lệ đang có (cf2d884 cũng chỉ sửa .md), nhưng nghĩa là hai bản đã lệch. Tên bước "GSM/SIM Call Execution" cố ý giữ; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0137` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): W-0137 chỉ đo bản .docx V0.3, ghi errata 22 vào spec và mở quyết định OD-20 cho Owner; không sửa mã, test hay gate. · Residual: OD-20=IMPLEMENTED / OPTION_1_WITHDRAW; W-0137 vẫn TESTS_PASS, không tự nâng ACCEPTED. Artifact _SUPERSEDED chỉ giữ audit/recovery, không phải tài liệu hiện hành; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0138` — Residual: Không tự đóng hay sửa nội dung quyết định nào — chỉ làm chúng hiện ra. OD-VOICE-05 có dấu hiệu mâu thuẫn với evidence W-0128 §12.3, ghi thành residual chứ không xử trong W-0138. REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0139` — IT-IMG-E2E-05 không có trong docs/traceability-tests.md; IT-OBS-EXPORT-11 không có trong docs/traceability-tests.md; IT-OBS-RESILIENCE-12 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: B-06 vẫn mở: manual staging job chưa chạy vì thiếu endpoint/credential/retention/access + exact image/SHA + dashboard/query + alert fire/recovery OWNER_DATA_REQUIRED; W-0063 không đổi; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0140` — Việc tài liệu, không có test phần mềm (5 tài liệu có tại commit): W-0140 chỉ đối chiếu bằng chứng TODAY-01..04, đồng bộ trạng thái vào tracker, sinh lại readiness mirror và ghi OD-20 đã quyết; không sửa mã, test hay gate. · Residual: Rung 0, 8/138 ACCEPTED, 90 TESTS_PASS, 16 BLOCKED_EXTERNAL; không Work ID nào được nâng ACCEPTED; W-0122/external signatures/target DB/hosted CI/shared integration vẫn blocked; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0141` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): W-0141 chỉ đổi tên một file .docx đã thu hồi theo OD-20 (giữ nguyên byte) và cập nhật quyết định, errata 22 và hồ sơ; không sửa mã, test hay gate. File .docx đã đổi tên sau đó bị W-0169 xoá khỏi cây; bản đủ byte còn trong lịch sử git. · Residual: W-0137/W-0141 không nâng ACCEPTED; 11 external gate/23 open decision/rung 0 giữ nguyên; artifact _SUPERSEDED chỉ dành audit/recovery, không được phát hành; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0164` — Residual: LOCAL_OFFLINE_ROUTING_VALIDATOR_READY; partial-ready 1..4 batch được kiểm độc lập nhưng authority vẫn metadata-only. 0 external routing input, 0/5 dispatch/response; không source/runtime/outbound mutation; REAL_CUSTOM…
- `W-0165` — Residual: LOCAL_RESPONSE_PROVENANCE_VALIDATOR_READY / EXTERNAL_AUTHORITY_UNVERIFIED; no external response or approval ledger mutation. W-0163 vẫn 0/5 dispatch/response; no source/runtime/outbound change; REAL_CUSTOMER_CALL_ALLO…
- `W-0167` — Việc tài liệu, không có test phần mềm (1 tài liệu có tại commit): W-0167 chỉ ghi lại một lượt chạy toàn bộ Ivr.sln (765/765 PASS) trên cây làm việc lúc đó để lấp khoảng trống D8; không sửa mã, test hay gate nên không có claim phần mềm riêng. · Residual: FULL_OFFLINE_SOLUTION_PASS / NO_SOURCE_CHANGE. Chỉ đóng local D8 current-tree gap; không suy thành M3/shared/staging/UAT/production evidence. W-0163 vẫn blocked; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0168` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_SECURITY_GATE_PASS / HOSTED_CI_NOT_RUN. GitNexus pre-edit impact LOW 0 process; no app/runtime/OpenAPI/DB/prod-config change. External secret custody/vendor/staging/UAT/production unchanged; REAL_CUSTOMER_CALL_A…
- `W-0170` — Residual: LOCAL_PROVENANCE_CHAIN_COMPLETE / EXTERNAL_EVIDENCE_NOT_RECEIVED / 0_OF_5_DISPATCHED / NO_GATE_PROMOTION; authority truth và external receipt hash vẫn cần chief auditor đối chiếu system-of-record; REAL_CUSTOMER_CALL_A…
- `W-0172` — Residual: Local scope hoàn tất. M3 còn phải giao assembler/CDC + generic consumer, Product approval và shared E2E; exact candidate/hosted verdict thuộc W-0177; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0174` — Residual: OFFLINE_VALIDATOR_READY / EXTERNAL_E2E_NOT_RUN / DELIVERY_DISABLED. Không network/DB/runtime/guard mutation; M3 consumer/OAS/CDC, Security auth/custody, Platform sandbox/network/TLS và 5 sign-off vẫn external; validat…
- `W-0178` — Residual: M3_D06_EVIDENCE_NOT_RECEIVED / STRATEGY_UNSIGNED / CODE_NOT_AUTHORIZED; validator PASS chỉ cho phép shared-E2E review, không authorize revoke/runtime/production/call thật
- `W-0180` — Residual: OFFLINE_VALIDATOR_READY / EXTERNAL_INPUT_NOT_RECEIVED / PRODUCTION_POLICY_NOT_APPROVED / CODE_NOT_AUTHORIZED; PASS chỉ cho phép runtime implementation review riêng sau authority/shared-E2E/cutover/release evidence; RE…
- `W-0181` — Residual: OFFLINE_M3_SIGNOFF_VALIDATOR_READY / M3_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED. PASS chỉ cho phép mở implementation review riêng; không authorize field implementation, migration, release hoặc cuộc gọi thật; REAL_CUSTO…
- `W-0182` — Residual: OFFLINE_REGISTRY_DECISION_VALIDATOR_READY / EXTERNAL_PROVIDER_AND_SIGNATURES_REQUIRED / DATA_0_OF_4 / CALIBRATION_NOT_RUN / CODE_NOT_AUTHORIZED; PASS chỉ cho phép provider-specific adapter review; REAL_CUSTOMER_CALL_A…
- `W-0183` — Residual: OFFLINE_PRODUCTION_BUNDLE_VALIDATOR_READY / EXTERNAL_DECISIONS_NOT_RECEIVED / CODE_NOT_AUTHORIZED; PASS chỉ cho phép implementation review riêng, không authorize OpenAPI/DB/vault/resolver/adapter/egress/secret/real call
- `W-0185` — Residual: OFFLINE_EVIDENCE_VALIDATOR_READY / REAL_SIM_AND_PRODUCTION_EVIDENCE_NOT_RECEIVED; Helm/container/converter rerun ENV_BLOCKED/NOT_RUN do Docker engine; validator PASS chỉ cho phép evidence review, không đạt lab/product…
- `W-0191` — Residual: LOCAL_P0_2_COMPLETE / DEVELOPMENT_MOCK_ONLY / NO_WORKER_OR_VENDOR_CALL; P0.3 image UI là blocker nội bộ kế; Module 3, staging, lab/SIM và production vẫn ngoài scope; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0192` — Việc tài liệu, không có test phần mềm (5 tài liệu có tại commit): W-0192 không sửa src/, tests/ hay script gate nào: phần của nó là script dev chạy tay `pnpm e2e:local` (kết quả 5/5 chỉ là một lần chạy tay, không runner nào trong sweep chạy lại), mục §0.2 đối soát lab plan và báo cáo ngày 05/09, nên không có claim phần mềm nào để test tự động xác nhận. · Residual: LOCAL_FULL_LIFECYCLE_RUNNABLE / MOCK_ONLY; blocker nội bộ còn lại là migration expand-contract; gói ký OD-V1 trên worktree-gd0-fixes chưa merge và sẽ cần W-0193 vì nhánh đó dùng trùng W-0190/W-0191; REAL_CUSTOMER_CALL…
- `W-0193` — Residual: Chạy trên worktree cô lập worktree-gd0-fixes vì một session Codex đang ghi song song vào main worktree; merge về main là bước riêng. Không đổi contract/OpenAPI/migration, không đóng gate ngoài nào; REAL_CUSTOMER_CALL_…
- `W-0194` — Việc tài liệu, không có test phần mềm (2 tài liệu có tại commit): Việc ký quyết định: ghi chữ ký owner cho 19 dòng OD-V1/OD-VOICE vào register và gói ký, thêm cột Current cho bảng bốn cột; không đổi code, test hay gate. · Residual: Docs-only, không đổi runtime/OpenAPI/migration. Bốn dòng còn mở: OD-V1-09 nửa sau và OD-V1-10 chờ số đo; OD-V1-11 và OD-V1-21 chờ người thứ hai — chữ ký không tạo ra người. Việc code mà chữ ký mở ra thuộc GĐ 2, liệt k…
- `W-0195` — Residual: PRODUCTION_CALL không được seed nên quay số thật vẫn bị từ chối ở mọi môi trường. Vế bốn mắt vẫn cần người thứ hai (OD-V1-11/OD-V1-21). Còn lại của GĐ 2: attempt policy production + khung giờ (OD-V1-08/16), dial token…
- `W-0198` — Residual: Đăng ký một policy không phải là quyết định dùng nó: intake phân giải version mà task khai, nên phải có task mang gh-247-prod-v1 tới thì mới có gì chạy trên nó. Khung giờ mặc định bật; tắt nó là một quyết định nhìn th…
- `W-0199` — Residual: Sổ là process-local, giống hai vault sở hữu nó: nó chặn trong phạm vi một worker, còn ràng buộc thật ở production đến từ SIM vault. Đã ghi rõ trong XML doc để không ai đọc test xanh thành lời hứa mạnh hơn. Còn lại của…
- `W-0200` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Bump này không đổi một byte nào của hợp đồng — M3 đã sinh client từ draft.22 thì không cần sinh lại, đã ghi vào §4A.7 của bản handover. TARGET_CONTRACT_V1 vẫn DRAFT: một chuỗi phiên bản không có hậu tố draft từ nay kh…
- `W-0201` — Residual: Hai luật đều đúng và nói về hai việc khác nhau — "đường dẫn tuyệt đối có tính đúng không" với "cuối đường dẫn đó có gì không" — nên fixture của test nay thoả luật thứ hai, chứ không nới luật nào để luật kia qua được. …
- `W-0202` — Việc tài liệu, không có test phần mềm (4 tài liệu có tại commit): Việc quyết định hợp đồng theo phương án (a) của owner: xoay baseline so sánh OpenAPI sang draft.23 và đóng băng báo cáo 11 cảnh báo; không đổi code, test hay gate runner của sweep. · Residual: Xoay baseline không phải phê duyệt: nó nói owner chấp nhận thay đổi này, không nói consumer nào đã migrate, và TARGET_CONTRACT_V1 vẫn DRAFT. Bản 1.0.0 của OD-V1-02 không đi kèm lượt xoay này — nó đã bị rút ở W-0200 và…
- `W-0203` — Residual: F-1 (HIGH) mở: 10 request /eligibility-checks đồng thời → 9 trả HTTP 500 vì PostgresIdempotencyStore.ExecuteAsync chạy SERIALIZABLE đọc-rồi-ghi ivr_idempotency_keys, 40001 bị ánh xạ thành IVR_INTERNAL_ERROR. Không vá …
- `W-0205` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Một phát hiện phụ đáng giữ: ApiBehaviorMatrixTests đỏ trên clone sạch vì thiếu deploy/ci/node_modules — nó fail chứ không tự skip, đúng như evidence W-0197 tuyên bố. Nghĩa là freeze checkout phải chạy npm --prefix dep…
- `W-0206` — Residual: Dòng này được ghi hồi tố. A-0588 chọn bỏ qua id W-0206 vì 4d0c761 đã dùng nó trong commit message mà chưa có dòng ledger nào. Nhưng bỏ qua để lại việc thật vô hình với ledger và với readiness board — gate-status không…
- `W-0216` — IT-K8S-NETPOL-04 không có trong docs/traceability-tests.md · Residual: Bài học ghi lại để lần sau không lặp: gate-status.yaml phải sinh bằng node scripts/gate-status.mjs --write, đừng gõ tay; Status §5 chỉ nhận một token trong 12 token, sắc thái để cột Residual; ký tự gạch đứng trong ô b…
- `W-0217` — Việc tài liệu, không có test phần mềm (2 tài liệu có tại commit): Việc sổ sách và tài liệu: ghi hồi tố dòng W-0206 vào tracker và thêm cầu nối tên result code sang business source ở m8-05 §3.1; không sửa runtime, test hay gate. · Residual: Không tự nhận là đã chốt cách đọc PACK-09: bốn dòng đầu của cầu nối chưa có trong errata V0.3 (errata mới ghi IVR_OPT_OUT và ba code thừa), nên §3.1 ghi rõ cần chief auditor/Owner xác nhận trước khi M3 ký §4.2. 52 wor…
- `W-0218` — Việc tài liệu, không có test phần mềm (1 tài liệu có tại commit): Việc rà soát tài liệu: kiểm lại bốn mục C1/C2/C5/C10 trên tài liệu và code rồi viết lại worklist kèm bằng chứng; không đổi code, test hay gate. · Residual: Phát hiện một câu lạc, cố ý không tự sửa: docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md:472 — bảng field callback — ghi result_type là "Một trong 11 giá trị ở §16", trong khi ck_ivr_result_callbacks_result_status…
- `W-0219` — Việc tài liệu, không có test phần mềm (1 tài liệu có tại commit): Việc dọn tài liệu: xếp lại worklist 07/09 theo người phải quyết và gom phần đã xong thành một bảng trỏ evidence; không đổi code, test hay gate. · Residual: Chỉ dọn cách trình bày, không đổi kết luận nào: mọi phát hiện, số liệu và câu tự đính chính đều giữ nguyên chữ. Phần đã xong rút gọn xuống một dòng vì chi tiết đã nằm trong docs/evidence/W-02xx/ — không xoá thông tin,…
- `W-0222` — Việc tài liệu, không có test phần mềm (4 tài liệu có tại commit): Lượt này chỉ xoá hai ref git đã merge và ghi mục miễn trừ đích danh một nhánh vào CLAUDE.md/AGENTS.md; không đổi src/, tests/ hay script gate nên không có claim phần mềm. · Residual: Phần bền của lượt này là luật, không phải hai ref: owner giữ nhánh nên mâu thuẫn CLAUDE.md (cấm nhánh, xoá nhánh lạ) ⟷ W-0130 (cần một nhánh sống lâu dài) phải được gỡ chứ không để treo — giá của việc treo đã thấy: au…
- `W-0224` — Việc tài liệu, không có test phần mềm (4 tài liệu có tại commit): Lượt này xoá một nhánh remote đã merge, ghi cảnh báo ref github/main vào CLAUDE.md/AGENTS.md và rút hai phát hiện sai; không đổi src/, tests/ hay script gate nên không có claim phần mềm. · Residual: Bài học đắt nhất lượt này: ref tracking không phải trạng thái remote, và tôi đã trình một con số 43 cho owner trước khi hỏi remote. Hai claim (a) và (c) đều là tôi lặp lại một nguồn (ref cũ, bullet audit) mà không kiể…
- `W-0225` — Residual: Gate thứ hai tìm thấy vì nó nói dối: verify-api-behavior-matrix.mjs in "verdict": "FAIL" ngay trên "failures": [] với 38/38 pass, vì verdict quyết bởi mảng failures cấp cao còn console.log in mảng failures theo từng o…
- `W-0226` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): Lượt này khôi phục và cập nhật hai phiếu hỏi Legal/Privacy và Platform bằng markdown, sửa routing trong evidence W-0122; không đổi src/, tests/ hay script gate nên không có claim phần mềm. Hai phiếu khôi phục ở lượt này sau đó bị W-0297 xoá; còn lại README này, routing trong hồ sơ W-0122 và worklist 07/09. · Residual: Hai phiếu vẫn NOT_SENT — việc gửi thuộc owner/chief auditor, cùng nhóm với 3.4. Nên chốt chỗ rẽ items_spoken trước khi Platform bắt tay vào INF-A, kẻo phí công dựng mirror cho artifact có thể bị bỏ. Nhưng có một vế đi…
- `W-0227` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): Lượt này khôi phục gói routing today-03 và phiếu Security bằng markdown, cập nhật routing trong evidence W-0122 và worklist; không đổi src/, tests/ hay script gate nên không có claim phần mềm. Gói routing và phiếu khôi phục ở lượt này sau đó bị W-0297 xoá; còn lại README này, routing trong hồ sơ W-0122 và worklist. · Residual: Ba đính chính ghi ở phụ lục thay vì sửa thân: §2 của gói ghi "đủ L1–L6" trong khi phiếu Legal có bảy câu; internal_mirror_gate chưa có dòng riêng trong bảng §1 ngày 29/08 mà nay là gate thật sau W-0225; và việc gói bị…
- `W-0247` — Việc tài liệu, không có test phần mềm (2 tài liệu có tại commit): Lượt này chỉ đọc lại nguồn và xếp lại năm mục 2.2–2.6 trong worklist; kết luận nằm trong tài liệu, không đổi src/, tests/ hay script gate nên không có claim phần mềm. · Residual: 2.5 là mục duy nhất trong nhóm 2 mà cái giá của việc không quyết rơi vào khách hàng: cả hai vế của nó đã kiểm lại và đều đúng, nghĩa là khách đã hủy đơn vẫn nhận cuộc gọi hỏi xác nhận chính đơn đó. Bốn mục kia sai ở t…
- `W-0248` — Việc tài liệu, không có test phần mềm (2 tài liệu có tại commit): Lượt này ghi quyết định phương án B của owner và đặc tả hai fence thu hồi đơn; code, migration và test do W-0249 dựng, lượt này không đổi src/, tests/ hay script gate. · Residual: Giới hạn phải nói trước để không ai hứa nhầm: LoadAsync đọc AsNoTracking không FOR UPDATE, nên revoke rơi vào khoảng LoadAsync → gateway dial vẫn lọt — và không nên đóng, vì đóng nghĩa là giữ một transaction DB xuyên …
- `W-0249` — Residual: Đề xuất m8-17 §5 của tôi sai và đã sửa trước khi dựng: nó viết "revoke mang version ≤ version đã lưu thì từ chối", nhưng order_version là chuỗi mờ IVR trả nguyên — OAS gọi là stale-result guard snapshot, IR-06 ghi "IV…
- `W-0250` — Residual: Đính chính con số của chính tôi: đầu lượt tôi viết vào docs/api-changelog.md rằng oasdiff cho "162 warnings, và zero errors" — sai. 162 là tổng; phân loại thật là 110 errors + 52 warnings, trong đó 108 error là header…
- `W-0251` — Residual: Tại sao đỏ suốt ba mươi work item mà không ai thấy — không phải vì ai bỏ qua, mà vì không gì chạy chúng. Bắc cầu mọi đường gọi (job trong deploy/ci/.yml, npm script trong hai package.json, import giữa các script) thì …
- `W-0252` — Residual: Luật áp dụng, viết ra để lần sau khỏi cãi: cập nhật con trỏ (đường dẫn dùng để tìm tệp), không đụng chứng thực (dòng hash). Vì thế W-0186/W-0188 — hai manifest tự liệt kê manifest khác — chỉ đổi cột đường dẫn, hash gi…
- `W-0253` — Residual: Hai assertion đã sai sẵn, lộ ra khi rà. image-selftest.mjs đòi ivr-admin-ui phải là service đang chạy trong compose, nhưng docker-compose.dev.yml chưa bao giờ định nghĩa service đó — assertion ấy đỏ với bất kỳ ai chạy…
- `W-0254` — Residual: Thứ chặn thật là nhóm (2), và nó lớn hơn vẻ ngoài. Đối chiếu §9 với specs/_review/open-decisions-register.md: 23 trong 28 quyết định đã CLOSED, nhưng IR-06 vẫn ghi "Auth profile + sandbox credential — Security/Platfor…
- `W-0265` — Residual: Contract phải sửa vì nếu không thì cầm chắc vòng hai. Ba header runtime vẫn dùng nhưng contract không khai: X-Action-Reason bắt buộc ở cả 8 endpoint danger (AdminAccessOptions.HasDangerEvidence từ chối nếu thiếu) mà k…
- `W-0266` — Việc tài liệu, không có test phần mềm (4 tài liệu có tại commit): Mục này là quyết định của owner đóng OD-V1-11/21/23, ghi vào sổ quyết định, IR-06 §9a và IR-07 (A-13, A-14); không đổi mã nguồn hay test, còn hai validator chỉ được ghim lại hash IR-06 và giá trị đó đã bị W-0275 ghim đè. · Residual: Đóng quyết định không mở ra mọi thứ, và đây là chỗ dễ đọc nhầm nhất nên phải ghi thẳng. Hai trong ba mục vướng ràng buộc kỹ thuật mà chữ ký không gỡ được: OD-V1-11 — PRODUCTION_REAL đòi ba actor id khác nhau (ScriptCo…
- `W-0267` — Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0270` — IT-IMG-BUILD-01 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0273` — Việc tài liệu, không có test phần mềm (2 tài liệu có tại commit): Mục này chỉ dựng phiếu điền kỳ hạn lưu trữ từ mã nguồn để pháp chế điền số; không đổi mã nguồn, test, cấu hình hay cổng kiểm nào. · Residual: Sửa 18/09 (W-0316, Lô 3 mục 1): hết chờ bên ngoài. Ngày 17/09 owner chọn S3 — giữ toàn bộ dữ liệu, không đặt kỳ hạn nào — nên không ai điền phiếu; phiếu nay ghi "đóng — không điền số nào". BLOCKED_EXTERNAL → EVIDENCE_…
- `W-0280` — IT-IMG-BUILD-01 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY / MOCK; còn một câu hỏi nghiệp vụ chưa đóng: tạm dừng hàng đợi có tính là IVR_CAPACITY_EXCEPTION không, hay IVR_CONFIRMATION_WINDOW_EXPIRED như runtime đang trả — hai nhãn dẫn tới hai hành động khác nhau ph…
- `W-0281` — Việc tài liệu, không có test phần mềm (1 tài liệu có tại commit): Mục này chỉ soạn phiếu yêu cầu gửi Hạ tầng và đính chính báo cáo tuần 12/09; không đổi mã nguồn, test hay cổng kiểm nào. Tệp phiếu sau đó bị W-0297 xoá; nội dung chuyển vào phiếu quyết định 17/09, còn lại một dòng trong báo cáo 12/09. · Residual: Phiếu ở trạng thái READY_TO_DISPATCH / NOT_SENT — chỉ owner gửi được. Không đổi mã nguồn, không đổi cổng kiểm
- `W-0282` — IT-IMG-COMPOSE-03 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY / MOCK; REAL_CUSTOMER_CALL_ALLOWED=NO. Một khoảng trống phải chủ Module 8 quyết, không tự lấp: POST /eligibility-checks là thứ đưa task rời HELD_MOCK, nhưng worker không có vòng lặp nào gọi nó (10 hosted se…
- `W-0283` — Residual: LOCAL_ONLY/MOCK; vòng mặc định tắt; M3 giữ CALL_REQUIRED và order revalidation; CI/SIM/M3 thật vẫn mở; full initial matrix bị invalidated do source thay đổi, có rerun cuối; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0284` — Việc tài liệu, không có test phần mềm (7 tài liệu có tại commit): Mục này là đối soát tài liệu bàn giao và sổ tiến độ (khôi phục 13 dòng tracker, sửa chỉ dẫn phiên bản IR-06/07/08, sinh hai changelog OpenAPI); phần chạm bốn validator chỉ là ghim lại hash IR-06 và đã bị W-0304 ghim đè, nên không có khẳng định phần mềm. · Residual: Không suy chữ ký từ im lặng; nhãn pause đang chờ owner; real calls NO
- `W-0286` — Residual: W-0292 tại 179a5eb có 31 job PASS gồm full suite/image/K8s/security/privacy/publish; dev thiếu cluster, staging skipped; M3/SIM/production NOT_RUN, real calls NO
- `W-0287` — Residual: Chaos lộ loopback fixture, review gate lộ false green, xử lý tiếp; hosted chưa push; pause/scan pending; real calls NO
- `W-0288` — Residual: Không đổi upstream fault target hay runtime; hosted NOT_RUN; real calls NO
- `W-0289` — Residual: Không nới gate; không chạy security audit; hosted NOT_RUN; real calls NO
- `W-0291` — CT-CI-04 không có trong docs/traceability-tests.md · Residual: Không thay API contract/runtime hoặc nới scanner; hosted là bước kế tiếp
- `W-0293` — Việc tài liệu, không có test phần mềm (4 tài liệu có tại commit): Mục này chỉ sửa câu chữ trong 45 tệp evidence cho khớp PII gate; không đổi scanner, pattern, allowlist, test hay mã nguồn nào. · Residual: Hosted pipeline cần chạy trên commit sửa; không đổi allowlist hoặc runtime
- `W-0294` — Residual: Cần pipeline chứa sửa JUnit; dev/staging đích chưa xác nhận; runtime không đổi
- `W-0295` — Residual: Cần pipeline mới; không ký bundle hoặc đổi authority/readiness
- `W-0296` — Việc tài liệu, không có test phần mềm (1 tài liệu có tại commit): Khôi phục runner là thao tác vận hành ngoài repo (đổi tên thư mục socket Docker cũ, mở lại Docker Desktop, bấm Retry trên GitLab), không đổi mã, test hay gate; bằng chứng là job CI hosted (số job ghi trong README). · Residual: Không reset data hoặc thay runner config; toàn pipeline/publish/deploy còn theo W-0292
- `W-0297` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): Chỉ dồn, xoá tài liệu kế hoạch trong plan/ivr-orther và viết lại link; commit 257cbef không chạm src/, tests/ hay script gate nên không có khẳng định phần mềm nào để test kiểm. · Residual: Phân loại sai một lần và đã sửa: m8-05…m8-10 mở đầu bằng M8_OWNER_SIGNED/đã ký nên lượt đọc đầu xếp vào nhóm xong — đọc hết chuỗi trạng thái thì cả sáu đều kèm M3_..._SIGNOFF_REQUIRED hoặc CODE_NOT_AUTHORIZED, tức mới…
- `W-0303` — Residual: Vault production không tự làm protector — lab là protector của chính nó vì bảo vệ của nó là vân tay tự sinh được; production nhận khóa Platform, nên deployment còn UnavailableOpaqueValueProtector fail closed ngay lần …
- `W-0304` — Residual: W-0297 đã làm 11 gate đỏ và không ai biết: nó xóa 27 file, 12 trong đó là nguồn ghim hash trong external-decision-artifacts.sha256 — chính file mà header của nó viết “update the matching line here in the same commit”;…
- `W-0309` — Việc tài liệu, không có test phần mềm (4 tài liệu có tại commit): Chỉ khôi phục và định tuyến lại các phiếu hỏi nhóm A và sửa sổ sách kế hoạch; commit d655989 không chạm file .cs, test hay script gate nên không có khẳng định phần mềm. · Residual: Lượt này bắt đầu bằng việc bắt một câu sai của chính tôi, đã lặp 3 lần trong 3 phiên: “việc gỡ được nhiều nhất là gửi 6 phiếu nhóm A — đã soạn xong, tốn 0 ngày công”. Cả hai tiền đề sai. (a) W-0297 đã xoá cả 6 file kh…
- `W-0313` — Residual: Hai chỗ trong trích dẫn nguyên văn (OD-V1-11, lập luận bác A1) đổi đúng một từ và đánh dấu bằng ngoặc vuông — một trích dẫn bị đổi chữ mà không đánh dấu là trích dẫn sai. Đổi manifest thay vì ghi luật vào tài liệu: kế…
- `W-0315` — UT-AST-AUDIO-03 không có trong docs/traceability-tests.md; UT-TTS-STATIC-REGION-05 không có trong docs/traceability-tests.md; UT-VOICE-3B-01 không có trong docs/traceability-tests.md; và 4 TestId khác · Residual: Lô 4 phần làm được ngay (Trivy, bản cài nền, cấu hình production nháp) · chờ S1/S2/S5 · chạy lại lab bằng VieNeu khi có bundle model. REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0316` — Việc tài liệu, không có test phần mềm (11 tài liệu có tại commit): Chỉ ghi các quyết định 17–18/09 vào register, tracker, kế hoạch và phiếu; thay đổi ngoài tài liệu duy nhất là trường timeoutMs của dr-selftest trong manifest gate, không đổi mã, test hay script gate. · Residual: Lô 3 mục 13 chờ Sếp ký S2 · Lô 4 bước 1, 2, 4 · Lô 5 · REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0317` — Việc tài liệu, không có test phần mềm (4 tài liệu có tại commit): Phần mã của lô này (ghim 4 gói Debian trong Dockerfile.tts) đã bị W-0323 thay bằng nền Chainguard nên không còn gì cho test kiểm; phần còn lại là đo Trivy, thử nền Chainguard và bản nháp Helm production mà không gate nào đọc. · Residual: Một lượt đọc thật khi có bundle model (tải cần Toàn cho phép) · S2 rủi ro 3: ký với 44 lỗ hay đổi nền Chainguard · phần Chờ của Lô 4 · REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0318` — UT-SCHEMA-BACKCOMPAT-04 không có trong docs/traceability-tests.md; UT-X-01 không có trong docs/traceability-tests.md; UT-X-02 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: Toàn duyệt từng đợt và tự chuyển ACCEPTED (bước 3), rồi gate-status.mjs --write (bước 4) · REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0319` — Residual: Đã thu gói kiểm chứng đầy đủ; kết quả chỉ chứng nhận commit bd9ea5c, không chứng nhận HEAD khác. Toàn đọc Residual và tự nghiệm thu; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0320` — Residual: Timeout worker 5s còn thấp hơn câu đơn đo 6,3–19,8s; chưa gọi/nghe lab, full acceptance sweep, scan CVE mới hoặc đổi image mặc định; S1/S2/S5/S6 và production giữ mở; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0321` — Residual: S1 nghe/mối nối còn chờ xác nhận; S2 lựa chọn nền/bản quyền và mirror/máy S5 chưa có; chỉ lab local, chưa commit/production; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0322` — E2E-UI-REPLAY-05 không có trong docs/traceability-tests.md; E2E-UI-REVIEW-05 không có trong docs/traceability-tests.md; UT-ELIG-BLOCK-01 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: Owner quyết định nghiệm thu P2 và kết thúc 4 P3 theo W-0253; F1 bộ sinh bỏ lọt test UI retired cần sửa ở work tiếp theo; không đổi 13 trạng thái; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0323` — Residual: Giọng/câu ghép OWNER_ACCEPTED. Bốn chốt 21/09 đã đóng bằng bằng chứng sau (residual-closeout.md): cold-start/tải S5 (W-0333/0335/0338/0343), pháp lý model (W-0341/0342/0343), mirror (W-0340/0343), DTMF (W-0329/0339/03…
- `W-0326` — Residual: W-0329 đã tái hiện timeout10s dưới tải và kiểm đủ DTMF; máy S5/mirror/Legal còn chờ; không yêu cầu nghe lại; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0329` — Bằng chứng chạy lab, không có test phần mềm trong sweep (6 tệp có tại commit): Lượt kiểm lab DTMF qua đầu SIP im lặng và đo tải VieNeu bằng model thật; chỉ thêm công cụ lab, không đổi mã ứng dụng, test .NET hay script gate, nên bằng chứng là log và số đo lab. · Residual: W-0331 sửa retry503; timeout10s còn tái hiện cold; W-0333 đã đối chiếu raw S5 vps61, còn kiểm worker/đủ đơn/tải kéo dài; sizing/mirror/Legal mở; giọng đã chốt; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0331` — Residual: Cold lab vẫn timeout10s; W-0333 raw S5 xác nhận model còn bận8871ms sau disconnect, dài hơn tổng backoff6750ms; cần tái hiện bằng worker và điều phối; mirror/Legal mở; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0333` — Bằng chứng chạy lab, không có test phần mềm trong sweep (9 tệp có tại commit): Bộ đo VieNeu cho Ubuntu S5 và kết quả đo thật trên vps61; chỉ thêm launcher lab cùng test Python chạy ngoài CI, không đổi mã ứng dụng, test .NET hay script gate, nên bằng chứng là số đo và hash máy đích. · Residual: Đã đo thực tế lượt đầu, chưa đóng S5: còn worker retry/đủ đơn/cold-warm/tải kéo dài. Model bận thêm8871ms sau disconnect; chưa tăng timeout/quota. S2/mirror mở; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0335` — Residual: Bản sửa chưa deploy worker vận hành. Soak dùng15s/phần, max10.099s; recovery dùng30s/phần, max19.352s gồm busy. Chưa chốt budget cuối hoặc nghiệm thu scheduler/SIP S5; S2/mirror mở; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0336` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): Gói trình duyệt phạm vi local của chín việc, đối chiếu bằng chứng và Residual; ngoài một chú thích XML trong SimAdapters.cs không đổi mã, test hay gate nên không có khẳng định phần mềm để test. · Residual: Toàn đã duyệt 9 phạm vi local theo phiếu tại 4dfd500; chín việc gốc ACCEPTED, tổng57; gói W-0336 giữ EVIDENCE_SUBMITTED; M3/target/rollback release sau và W-0042 staging còn mở; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0337` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): Gói trình owner quyết định chín việc P2 và bốn việc P3 UI đã ngừng, kèm đối chiếu C1/C2/C4 tại aaba3d2; không đổi mã, test hay gate nên không có khẳng định phần mềm để test. · Residual: Toàn đã duyệt cả9 P2 ACCEPTED local và4 P3 CANCELLED theo phiếu 8763dc1; tổng66 ACCEPTED; W-0337 giữ EVIDENCE_SUBMITTED; M3/lab/production ngoài scope; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0340` — Bằng chứng chạy lab, không có test phần mềm trong sweep (8 tệp có tại commit): Hồ sơ owner ký nhận bảy rủi ro S2, quét CVE đúng image bằng DB mới và biên nhận kho artifact trên vps61; không đổi mã, test hay gate, chỉ có tài liệu, số đo và script bàn giao. · Residual: W-0341 đã xác minh công bố quyền đúng revision và đính chính kết luận thiếu chứng từ; W-0342 đã ghi vào provenance cùng13mirrorURI; W-0343 đã nhận phê duyệt Legal/Privacy có thẩm quyền. Image cuối đã kiểm local,chưa b…
- `W-0341` — Việc tài liệu, không có test phần mềm (4 tài liệu có tại commit): Rà soát quyền dùng model/codec VieNeu và MOSS theo nguồn công bố đúng revision; chỉ thêm tài liệu và hash nguồn, không đổi mã, test hay gate. · Residual: W-0342 đã tích hợp bằng chứng và mirror vào provenance/lock; W-0343 đã nhận phê duyệt Legal/Privacy riêng có thẩm quyền;image cuối chờ S5. W-0341 là bằng chứng rà nguồn lịch sử, không xác nhận image mới. Giữ S2 đã ký;…
- `W-0342` — Residual: License evidence PASS,model mirror PASS;W-0343 đã nhận LEGAL decision thật và dựng image cuối676133c7. Image cuối chưa mirror/deploy S5,không dùng phép đo cũ để nhận tải cho image mới. Giữ2CPU/4GiB,profile30/90/120;RE…
- `W-0343` — Residual: S5 đã đo 23/09 (s5-target-result.md), receipt kiểm đạt; chờ Toàn nghiệm thu. W0344 riêng full-flow. Production BLOCKED;REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0344` — Bằng chứng chạy lab, không có test phần mềm trong sweep (32 tệp có tại commit): Lượt chạy toàn luồng đơn giả trên S5 và diễn tập lab; không đổi mã ứng dụng, test .NET hay script gate, chỉ có bộ kiểm lab chạy ngoài CI, nên bằng chứng là receipt, log và đối soát DB của lượt chạy. · Residual: Toàn nghiệm thu (docs/evidence/W-0344/cpu-retry.md mục 8). Không chứng minh SIM thật, M3, nhiều worker hay production; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0345` — Bằng chứng chạy lab, không có test phần mềm trong sweep (12 tệp có tại commit): Lượt chạy toàn luồng đơn giả chương trình 24/7 trên lab và S5; chỉ đổi bộ kiểm lab chạy ngoài CI, không đổi mã ứng dụng, test .NET hay script gate, nên bằng chứng là receipt và đối soát DB. · Residual: Toàn nghiệm thu. Lab vẫn 1 attempt/300s (lab-softphone-v1): chưa kiểm cửa sổ 15 phút, [0,450] của 24/7 production; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0347` — Residual: W-0012, W-0015, W-0031 đã ACCEPTED (A-0773..A-0775). Chờ Toàn quyết: W-0036 bảy ID §8 theo nhóm (OpenAPI parse/ref/enum, task schema + policy mismatch); W-0037 vế rate-limit và soak dài; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0348` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Toàn đã duyệt nhóm A và B theo phiếu tại 7fc9806: 78 việc ACCEPTED (A-0777..A-0854), tổng 149/336, Nấc 1 35/54; gói W-0348 giữ EVIDENCE_SUBMITTED; nhóm C còn việc hoặc cần rà lại (W-0041, W-0053, W-0108, W-0249, W-030…

