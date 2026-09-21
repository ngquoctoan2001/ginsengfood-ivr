# Danh sách đề nghị nghiệm thu — `3cf695928ce0e2602ae660a7c6f999b08f7dee0b`

Script **chỉ đọc**: không sửa tracker. Chỉ Toàn chuyển một dòng sang `ACCEPTED`, sau khi
đọc bằng chứng — danh sách này không thay cho việc đọc.

| Nguồn | Trạng thái |
| --- | --- |
| Kết quả test (`C2`) | 4 file `.trx`, 1151 kết quả · SHA/hash/đủ project đã kiểm |
| Gate sweep (`C4`) | ✅ GATE_SWEEP_PASS 42/42 |

## Nấc 1

Prompt đã lên kế hoạch đã xong (`ACCEPTED`, `N/A`, `CANCELLED`): **6/54**
· `BLOCKED_INTERNAL`: **0**. Nấc 1 đạt khi hai số này là
**54/54** và **0**.

## Tổng theo đợt

| Đợt | Ứng viên | ĐẠT | XEM | KHÔNG ĐẠT | CHƯA KIỂM |
| --- | ---: | ---: | ---: | ---: | ---: |
| `P0` | 3 | 0 | 0 | 3 | 0 |
| `P1` | 4 | 0 | 1 | 3 | 0 |
| `P2` | 9 | 0 | 9 | 0 | 0 |
| `P3` | 4 | 0 | 0 | 4 | 0 |
| `P4` | 4 | 0 | 1 | 3 | 0 |
| `P5` | 5 | 0 | 1 | 4 | 0 |
| `P6` | 3 | 0 | 0 | 3 | 0 |
| `P7` | 5 | 0 | 0 | 5 | 0 |
| `P10` | 4 | 0 | 0 | 4 | 0 |
| `UNPLANNED` | 180 | 0 | 45 | 135 | 0 |

`ĐẠT`: đủ bốn điều. `XEM`: C1, C2, C4 đạt, còn cột Residual cần Toàn đọc xem có việc
của IVR không. `KHÔNG ĐẠT`: hỏng ít nhất một điều, lý do ở dưới. `CHƯA KIỂM`: thiếu kết quả test
hoặc log gate sweep để kết luận.

## Đợt `P0`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0011` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0012` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0013` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0011` — docs/evidence/W-0011/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO (gói ghi dạng cũ: "Real customer calls: NO") · CT-CI-01 không có trong docs/traceability-tests.md; CT-CI-02 không có trong docs/traceability-tests.md; CT-CI-03 không có trong docs/traceability-tests.md; và 6 TestId khác · Residual: CI + hosted enforcement complete except required independent approval; giữ TESTS_PASS tới khi Premium/Ultimate + second reviewer proof đóng W-0061
- `W-0012` — docs/evidence/W-0012/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO (gói ghi dạng cũ: "Real customer calls: NO") · CT-CI-05 không có trong docs/traceability-tests.md; UT-FND-RBAC-03 không có trong docs/traceability-tests.md; UT-FND-RBAC-08 không có trong docs/traceability-tests.md · Residual: local MOCK implementation complete; P1-2 owns persistence migrations, P4-4 owns production auth; no Sales/SIM/real call; W-0061 push setting PASS, independent approval remains
- `W-0013` — docs/evidence/W-0013/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO (gói ghi dạng cũ: "Real customer calls: NO") · CT-CI-05 không có trong docs/traceability-tests.md · Residual: local MOCK complete; OD-V1-20 pending/fail-closed; P1-2 owns migration/persistent mutation; W-0061 push setting PASS, independent approval remains

## Đợt `P1`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0014` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0015` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0016` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0064` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0014` — docs/evidence/W-0014/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO (gói ghi dạng cũ: "real-customer calls: NO") · CT-CI-05 không có trong docs/traceability-tests.md · Residual: Contract remains TARGET_DRAFT; current compat runtime-disabled; W-0002/W-0005/W-0006 external; W-0061 push setting PASS, independent approval remains
- `W-0015` — docs/evidence/W-0015/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · IT-DB-LEASE-05 không có trong docs/traceability-tests.md · Residual: local P0-4 persistence gap closed; DF-07/KMS/backup-staging-prod remain; W-0061 push setting PASS, independent approval remains; no real Sales/SIM/call
- `W-0016` — docs/evidence/W-0016/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: local MOCK complete; Target V1/policy approvals and real Sales/SIM/customer calls remain external/NOT_RUN; W-0061 push setting PASS, independent approval remains
- `W-0064` — Residual: owner/reviewer acceptance pending; production periods vẫn OWNER_DECISION_REQUIRED theo DF-07/OD-V1-11; REAL_CUSTOMER_CALL_ALLOWED=NO

## Đợt `P2`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0018` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0019` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0020` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0021` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0022` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0023` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0024` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0065` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0066` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0018` — Residual: owner/reviewer acceptance, real Sales/auth, LAB/PROD script/key/SIM and P2-2 eligibility remain open
- `W-0019` — Test lịch sử đã thay thế: UT-ELIG-BLOCK-01, UT-ELIG-TRUST-03; xem acceptance-tests.json trong gói bằng chứng · Residual: Đã ghim quyết định và 11 test thay thế cho 2 ID retired; Toàn xét riêng sau kiểm chứng mới; M3/auth/SIM thật ngoài phạm vi MOCK; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0020` — Residual: scheduler flag off + dispatch gateway unavailable until P2-4; policy owner/LAB/1 SIM/32 eSIM/PROD NOT_RUN; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0021` — Residual: MOCK-only; no live TTS/real provider/egress/customer call; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0022` — Residual: MOCK/local only; normalization disabled by default; production retry/vendor mapping owner approval; P2-6 callback pending
- `W-0023` — Residual: local/MOCK only; callbacks disabled; Target contract DRAFT; W-0005/W-0006, real Sales/auth/LAB/PROD remain blocked; no notification
- `W-0024` — Residual: GitHub main pushed; GitLab main push BLOCKED_EXTERNAL by protected-branch rule; MOCK fixture only; LAB/real Sales/PROD NOT_RUN; OD-V1-15 + W-0003 remain open
- `W-0065` — Residual: local/MOCK only; Target contract DRAFT; hosted GitLab/reviewer/real Sales-auth/SIM-eSIM/LAB/PROD NOT_RUN; REAL_CUSTOMER_CALL_ALLOWED=NO; no order transition/Sales write/SMS/call thật
- `W-0066` — Residual: prereq của W-0048; OD-V1-15, OD-V1-19, W-0008 vẫn mở; lab/vendor/pronunciation NOT_RUN; MOCK no-egress, real calls NO

## Đợt `P3`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0025` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0026` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0027` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0028` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0025` — UI đã ngừng phạm vi theo quyết định đã ghim; hồ sơ đề nghị CANCELLED, chờ Toàn. Test backend không chứng nhận UI. · Residual: UI đã retire theo W-0253; closeout hoàn thiện, đề nghị CANCELLED chờ Toàn; M3 chưa xác nhận người nhận/triển khai UI; xem scope-closeout.md; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0026` — UI đã ngừng phạm vi theo quyết định đã ghim; hồ sơ đề nghị CANCELLED, chờ Toàn. Test backend không chứng nhận UI. · Residual: UI đã retire theo W-0253; closeout hoàn thiện, đề nghị CANCELLED chờ Toàn; M3 chưa xác nhận người nhận/triển khai UI; xem scope-closeout.md; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0027` — UI đã ngừng phạm vi theo quyết định đã ghim; hồ sơ đề nghị CANCELLED, chờ Toàn. Test backend không chứng nhận UI. · Residual: UI đã retire theo W-0253; closeout hoàn thiện, đề nghị CANCELLED chờ Toàn; M3 chưa xác nhận người nhận/triển khai UI; xem scope-closeout.md; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0028` — UI đã ngừng phạm vi theo quyết định đã ghim; hồ sơ đề nghị CANCELLED, chờ Toàn. Test backend không chứng nhận UI. · Residual: UI đã retire theo W-0253; closeout hoàn thiện, đề nghị CANCELLED chờ Toàn; M3 chưa xác nhận người nhận/triển khai UI; xem scope-closeout.md; REAL_CUSTOMER_CALL_ALLOWED=NO

## Đợt `P4`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0029` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0030` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0031` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0032` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |

- `W-0029` — docs/evidence/W-0029/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: mock-only là trần; real provider vẫn từ chối boot — blocker còn lại là W-0006/OD-V1-07, không phải P4-1; real sandbox CDC NOT_RUN (chưa có endpoint/credential); /health/ready không đụng — thuộc W-0040
- `W-0030` — Residual: mock-only là trần; shape ở specs/api/evidence/ là đề xuất IVR, OD-V1-03 vẫn mở nên W-0002/W-0005 giữ BLOCKED_EXTERNAL; real sandbox NOT_RUN; IVR vẫn không sở hữu ops transition (D-02)
- `W-0031` — docs/evidence/W-0031/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · IT-ELIG-TRUST-14 không có trong docs/traceability-tests.md; UT-ELIG-TRUST-16 không có trong docs/traceability-tests.md; UT-ELIG-TRUST-17 không có trong docs/traceability-tests.md · Residual: mock-only là trần; trust skip vẫn tắt — bật là quyết định owner cần hợp đồng resolver có phiên bản từ Sales; shape voice/trust là đề xuất IVR trong linked evidence reference, OD-V1-03 vẫn mở; real sandbox NOT_RUN; not…
- `W-0032` — docs/evidence/W-0032/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: mock-only là trần; production auth profile vẫn BLOCKED_EXTERNAL (W-0006/OD-V1-07) và Mode=Real từ chối boot; mTLS bật mà chưa có profile đã ký cũng từ chối boot; chưa có ngày tắt X-Internal-Token — closure artifact củ…

## Đợt `P5`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0035` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0036` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0037` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0038` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0039` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0035` — Residual: traceability hiện là test↔source, ánh xạ đầy đủ tới từng mục specs/testing/02/03 còn lại cho P5-2/P5-4; mock-only, REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0036` — CT-CB-01 không có trong docs/traceability-tests.md; CT-CB-02 không có trong docs/traceability-tests.md; CT-CB-03 không có trong docs/traceability-tests.md; và 16 TestId khác · Residual: không có Pact — khác biệt có chủ ý, pact chỉ có giá trị khi cả hai bên cùng chạy mà Sales chưa có gì; không có E2E trình duyệt — pane preview không composite frame; provider thật vẫn BLOCKED_EXTERNAL
- `W-0037` — PT-SOAK-02 không có trong docs/traceability-tests.md; SEC-AUTHZ-05 không có trong docs/traceability-tests.md; SEC-ERR-06 không có trong docs/traceability-tests.md · Residual: rate limiting CHƯA CÓ (chỉ có ánh xạ 429, không middleware) — ngưỡng là quyết định vận hành chưa ai duyệt; soak 4–8h NOT_RUN; không tuyên bố ngưỡng latency D-04 vì cần Sales thật trong vòng lặp; năng lực 32 kênh thật …
- `W-0038` — docs/evidence/W-0038/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-GATE-01 không có trong docs/traceability-tests.md; CT-GATE-02 không có trong docs/traceability-tests.md; CT-GATE-03 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: hosted GitLab evidence NOT_RUN — W-0061 còn BLOCKED_EXTERNAL, self-test chứng minh công cụ từ chối đúng thứ chứ không chứng minh GitLab đã chặn một merge; không bật SAST/Semgrep/Sonar (tier chưa có, bật scanner không …
- `W-0039` — docs/evidence/W-0039/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · UI-A11Y-01 không có trong docs/traceability-tests.md; UI-I18N-02 không có trong docs/traceability-tests.md; UI-VISUAL-04 không có trong docs/traceability-tests.md; và 2 TestId khác · Residual: UI-XBROWSER-03 và visual-regression NOT_RUN — pane preview không composite frame, không thêm job CI cho việc không chạy được (sẽ đỏ vĩnh viễn rồi bị allow_failure hoá); axe không dùng (cần DOM thật) — thay bằng kiểm c…

## Đợt `P6`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0040` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0041` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0042` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |

- `W-0040` — docs/evidence/W-0040/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · IT-OBS-TRACE-02 không có trong docs/traceability-tests.md · Residual: OTLP export BLOCKED_EXTERNAL (W-0063) — instrumentation dùng BCL nên gắn exporter sau không sửa call site; mới instrument 1/5 chặng (callback), 4 chặng còn lại chưa có span; dependency_probing_available vẫn false — đó…
- `W-0041` — docs/evidence/W-0041/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CAP-ALERT-04 không có trong docs/traceability-tests.md · Residual: chưa exporter OTLP (W-0063) nên chưa tín hiệu nào rời tiến trình — đây là artifact as-code đã qua bộ đánh giá luật thật, KHÔNG phải hệ đang chạy; không có screenshot dashboard (§10) vì không có Grafana để chụp và tôi …
- `W-0042` — docs/evidence/W-0042/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: không có staging — chaos chạy trong harness tự dựng của bộ test, chưa lượt nào trên hệ đã triển khai; deploy/chaos/ là config cho staging chưa tồn tại (W-0063); không có alert-fire capture thật (§10) vì không có Alert…

## Đợt `P7`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0043` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0044` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0045` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0046` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0047` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0043` — IT-IMG-BUILD-01 không có trong docs/traceability-tests.md; IT-IMG-COMPOSE-03 không có trong docs/traceability-tests.md; IT-IMG-E2E-05 không có trong docs/traceability-tests.md; và 4 TestId khác · Residual: chưa có registry (§5 NEED_CONFIRMATION) nên chưa push lần nào; ~~compose smoke chưa chạy hết luồng DTMF tới callback; chưa seed attempt policy~~ đã đóng 2026-08-19 bằng IT-IMG-E2E-05 + deploy/docker/dev-seed/seed.sql …
- `W-0044` — IT-K8S-GATE-02 không có trong docs/traceability-tests.md; IT-K8S-LINT-01 không có trong docs/traceability-tests.md; IT-K8S-NETPOL-04 không có trong docs/traceability-tests.md; và 2 TestId khác · Residual: ~~IT-K8S-NETPOL-04 NOT_PROVEN~~ đã đóng 2026-08-19 — kết luận cũ "cluster không thực thi, cần Calico/Cilium" là sai: cluster có thực thi, phép đo bị đua với thời điểm kube-router cài luật iptables cho pod. Nay 5/5 K8S…
- `W-0045` — docs/evidence/W-0045/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · IT-CD-CONCURRENCY-05 không có trong docs/traceability-tests.md; IT-CD-DEV-01 không có trong docs/traceability-tests.md; IT-CD-GATE-02 không có trong docs/traceability-tests.md; và 2 TestId khác · Residual: TESTS_PASS chỉ cho static/configuration gates; CHƯA PIPELINE DEPLOY NÀO CHẠY — không runner/registry/credential cluster (W-0061,W-0063), nên mọi deploy/approval/rollback evidence là NOT_RUN/BLOCKED_EXTERNAL; không suy…
- `W-0046` — IT-BG-WORKER-02 không có trong docs/traceability-tests.md; IT-CANARY-01 không có trong docs/traceability-tests.md; IT-FLAG-RAMP-04 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: TESTS_PASS chỉ cho static/configuration + migration gates; canary run / auto-rollback / blue-green switch demo đều NOT_RUN — Argo Rollouts và Prometheus backend vẫn thuộc W-0063; không suy diễn progressive-delivery ru…
- `W-0047` — docs/evidence/W-0047/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · IT-K8S-NETPOL-04 không có trong docs/traceability-tests.md; IT-K8S-ROTATE-07 không có trong docs/traceability-tests.md · Residual: drill overlap đã chạy trên HTTP thật: token cũ được nhận 21/21 lượt trong cửa sổ rồi chuyển 403 đúng tại thời điểm hết hạn, token mới 0/28 lượt bị 403, token chưa cấu hình 403 cả 28 lượt — ~~nhưng drill chạy trên một …

## Đợt `P10`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0052` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0053` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0054` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0055` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0052` — docs/evidence/W-0052/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: KHÔNG chữ ký nào — PIA DRAFT_UNSIGNED, chu kỳ retention UNSIGNED, cơ sở pháp lý là đề xuất kỹ thuật (W-0009); không endpoint DSAR — cần permission Permission Core chưa cấp (OD-V1-20/DF-01), treo lên IVR_QUEUE_VIEW ngh…
- `W-0053` — docs/evidence/W-0053/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · DG-BACKUP-02 không có trong docs/traceability-tests.md; DG-CRYPTO-01 không có trong docs/traceability-tests.md; DG-DR-03 không có trong docs/traceability-tests.md · Residual: không multi-AZ — một host, hai container; không mã hoá volume at-rest và không KMS (W-0063); không PITR (logical dump); RTO chưa gồm phát hiện/quyết định/chuyển traffic; chưa có fencing nên promote khi chỉ mất liên lạ…
- `W-0054` — docs/evidence/W-0054/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CAP-ALERT-04 không có trong docs/traceability-tests.md; CAP-CALIB-03 không có trong docs/traceability-tests.md; CAP-MODEL-01 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: UNCALIBRATED — P5-3 đo API/scheduler, chưa bao giờ đo một cuộc quay số; mô hình không được dùng quyết định mua hàng tới khi W-0008 cho thời lượng đo được; không dự báo khối lượng từ business — mà dailyOrders chính là …
- `W-0055` — docs/evidence/W-0055/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-DOC-02 không có trong docs/traceability-tests.md; IT-MIGRATE-03 không có trong docs/traceability-tests.md · Residual: không có kho dữ liệu riêng — là schema analytics trong cùng database (W-0063), lệch với P10-4 §7 vì DTS-04 chốt 3 deployable; không BI tool nào từng kết nối (grant SELECT là thứ cấp được, chưa ai cấp); chưa đo trên kh…

## Đợt `UNPLANNED`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0078` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0079` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0080` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0081` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0082` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0083` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0084` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0087` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0088` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0089` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0090` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0091` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0092` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0094` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0095` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0096` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0097` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0098` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0099` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0100` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0101` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0103` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0108` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0110` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0111` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0112` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0113` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0114` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0115` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0116` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0117` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0123` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0124` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0125` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0126` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0127` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0128` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0129` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0130` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0131` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0132` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0134` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0135` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0136` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0137` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0138` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0139` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0140` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0141` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0144` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0155` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0156` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0157` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0158` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0159` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0161` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0162` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0164` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0165` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0167` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0168` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0170` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0172` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0173` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0174` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0176` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0177` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0178` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0179` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0180` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0181` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0182` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0183` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0184` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0185` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0186` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0187` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0188` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0189` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0190` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0191` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0192` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0193` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0194` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0195` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0196` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0197` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0198` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0199` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0200` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0201` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0202` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0203` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0205` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0206` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0207` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0208` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0209` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0210` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0211` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0212` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0213` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0214` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0215` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0216` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0217` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0218` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0219` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0220` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0221` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0222` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0223` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0224` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0225` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0226` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0227` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0243` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0245` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0246` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0247` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0248` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0249` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0250` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0251` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0252` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0253` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0254` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0265` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0266` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0267` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0268` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0269` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0270` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0272` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0273` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0274` | `TESTS_PASS` | ❌ | ✅ | 👀 | **KHÔNG ĐẠT** |
| `W-0275` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0276` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0277` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0278` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0279` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0280` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0281` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0282` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0283` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0284` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0286` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0287` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0288` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0289` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0290` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0291` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0293` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0294` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0295` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0296` | `TESTS_PASS` | ❌ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0297` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0298` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0299` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0300` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0301` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0302` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0303` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0304` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0305` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0306` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0307` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0308` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0309` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0310` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0311` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0312` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0313` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0314` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0315` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0316` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0317` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0318` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0319` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0322` | `EVIDENCE_SUBMITTED` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0078` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: retain pin until Testcontainers declares a patched dependency; no production SSH/SIM/Sales execution
- `W-0079` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: local implementation complete; hosted GitLab evidence vẫn NOT_RUN dưới W-0061
- `W-0080` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: local implementation complete; hosted artifact topology vẫn NOT_RUN dưới W-0061
- `W-0081` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: local catalog complete; Target V1 remains DRAFT and owner acceptance is separate
- `W-0082` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: HIGH blast radius regression passed locally; production runtime proof remains outside this work
- `W-0083` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: local guard complete; any new source project/reference must update reviewed matrix
- `W-0084` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: local PostgreSQL proof complete; no staging/production database mutation performed
- `W-0087` — Residual: no real SIM/customer call; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0088` — docs/evidence/W-0088/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: explicit admin pause remains the only global queue hold
- `W-0089` — docs/evidence/W-0089/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Target V1 remains DRAFT; no external contract approval implied
- `W-0090` — docs/evidence/W-0090/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: callbacks remain disabled by default and external Sales is NOT_RUN
- `W-0091` — docs/evidence/W-0091/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: evidence labels now match asserted behavior; real providers remain NOT_RUN
- `W-0092` — docs/evidence/W-0092/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-DOC-02 không có trong docs/traceability-tests.md · Residual: contract stays TARGET_CONTRACT_V1=DRAFT; Sales approval remains BLOCKED_EXTERNAL
- `W-0094` — docs/evidence/W-0094/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: recording readback retained as defense in depth; provider boundaries/no-egress preserved
- `W-0095` — CT-CI-01 không có trong docs/traceability-tests.md · Residual: read-only, không ghi; aggregate tính ở server; order_code chỉ là filter, không echo; order_state opaque (D-02); không thêm permission mới; Sales contract không đổi; hosted GitLab NOT_RUN
- `W-0096` — Residual: read-only tuyệt đối (POST → 405); dependency_probing_available=false, dependency chưa thăm dò để NOT_WIRED chứ không tô xanh; KHÔNG expose script lifecycle mutation (OD-V1-15), KHÔNG seed writer, KHÔNG gán quyền (DF-0…
- `W-0097` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: chỉ thay đổi trình bày, không đụng API/contract/permission/governance; skill cài ở user level chứ không vào repo; KHÔNG tải web font; KHÔNG dùng green làm CTA (green giữ nghĩa healthy); component library vẫn NEED_CONF…
- `W-0098` — CT-DOC-01 không có trong docs/traceability-tests.md; UT-DOC-PII-03 không có trong docs/traceability-tests.md · Residual: KHÔNG phải P10-4: W-0055 vẫn NOT_STARTED, không có ETL/warehouse/fact-dimension/idempotent replay; mọi payload tự khai warehouse_backed=false + pipeline_work_id=W-0055; k-anonymity min_bucket_size=5 là hằng số server,…
- `W-0099` — E2E-UI-LOG-01 không có trong docs/traceability-tests.md; UT-UI-SIM-05 không có trong docs/traceability-tests.md · Residual: roster KHÔNG chiếu sim_number_ref (D-05) và KHÔNG chiếu lease/fencing (cơ chế scheduler, tránh can thiệp tay); chỉ hiện control có nghĩa theo trạng thái + RequirePermission; tắt kênh đang bận được chấp nhận nhưng copy…
- `W-0100` — CT-DOC-01 không có trong docs/traceability-tests.md; E2E-UI-REVIEW-05 không có trong docs/traceability-tests.md; UT-DOC-PII-03 không có trong docs/traceability-tests.md; và 3 TestId khác · Residual: guard drift mới bắt lỗi ngay lần chạy đầu theo chiều ngược: rule substring gắn cờ nhầm invalid_phone (là số đếm result type, không phải số ĐT) → chuyển sang khớp tên chính xác; E2E dashboard đỏ đúng lúc W-0099 thêm re…
- `W-0101` — E2E-UI-DETAIL-02 không có trong docs/traceability-tests.md; E2E-UI-LOG-01 không có trong docs/traceability-tests.md · Residual: attempt_two_pending cố ý không đặt tên "due": due-ness cần offset schedule riêng từng job, aggregate không parse; call_success_rate = confirmed+cancelled+wrong_input (huỷ vẫn tính là gọi thành công) — công thức ghi th…
- `W-0103` — IT-IMG-E2E-05 không có trong docs/traceability-tests.md · Residual: MOCK-only; REAL_CUSTOMER_CALL_ALLOWED=NO; Sales endpoint/auth/real payload + one-SIM lab + 32-eSIM provisioning vẫn NOT_RUN/BLOCKED_EXTERNAL; Target V1 vẫn DRAFT; bước kế tiếp W-0048/one-SIM lab chỉ bắt đầu sau khi nh…
- `W-0108` — Residual: Lỗi contract duy nhất không thuộc W-0108: luồng OD-V1-20 sửa 1 dòng summary trong OpenAPI mà chưa re-pin contract-manifest.json (98d226b1… → 7b921b3a…); khôi phục file về bản commit ⇒ contract 22/22. 12 MP3 đoạn cố đị…
- `W-0110` — docs/evidence/W-0110/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · E2E-UI-FLAGS-06 không có trong docs/traceability-tests.md; UT-FLAGS-ASYMMETRY-01 không có trong docs/traceability-tests.md · Residual: Phát hiện: chốt tự-duyệt-đích hiện KHÔNG có hiệu lực với phiên console — RejectSelfAuthorization phụ thuộc claim ivr_destination_ref mà chỉ MockPermissionAuthenticationHandler cấp; đề xuất mở OD-FLAG-01. Không đổi bac…
- `W-0111` — Residual: IVR_CALL_TERMINATE cấp cho cả Operator, không chỉ Admin — chiều giảm rủi ro, không khởi động được gì. Không cắt tức thì (độ trễ = chu kỳ poll) và không cắt được cuộc mà worker đã chết giữa chừng — ghi rõ ở evidence §6…
- `W-0112` — docs/evidence/W-0112/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · UI-I18N-02 không có trong docs/traceability-tests.md; UT-L10N-COVER-03 không có trong docs/traceability-tests.md; UT-UI-SEED-PROD-03 không có trong docs/traceability-tests.md · Residual: Phát hiện: file mẫu sales-target-v1.sample.json đã hết hạn — mốc tuyệt đối 12/8/2026, nạp nguyên trạng thì cả 9 tác vụ bị từ chối vì cửa sổ hết hạn; loader dời cửa sổ từng tác vụ về hiện tại và ghi rõ trong phản hồi. …
- `W-0113` — docs/evidence/W-0113/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · UT-UI-VOICE-05 không có trong docs/traceability-tests.md · Residual: Ba trường giọng ở mức attempt ghi kể cả khi null, trái luật bỏ-null toàn cục của API: trường vắng mặt và trường null là không phân biệt được với người đọc, mà 'có được ghi không' đúng là câu hỏi việc này sinh ra để tr…
- `W-0114` — docs/evidence/W-0114/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-CI-08 không có trong docs/traceability-tests.md; IT-MIGRATE-03 không có trong docs/traceability-tests.md · Residual: Không sửa một dòng production nào — một cổng phải sửa code mới xanh được thì không còn là phép đo. Danh sách miễn trừ rỗng là kết quả đo (cả 12 migration đã thuần bổ sung), không phải mặc định bỏ qua; nó nằm trong chí…
- `W-0115` — UT-SCHEMA-BACKCOMPAT-04 không có trong docs/traceability-tests.md · Residual: Production distinct-value scan=OWNER_DATA_REQUIRED; A10 giữ OWNER_DECISION_REQUIRED; không đổi contract/API; không sinh evidence SIM/carrier/real-call; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0116` — CT-DOC-02 không có trong docs/traceability-tests.md; UT-UI-CONTRACT-06 không có trong docs/traceability-tests.md · Residual: openapi:lint còn 14 lỗi baseline ngoài W-0116; hosted CI/deploy/UAT NOT_RUN; không sinh SIM/carrier/real-call evidence; REAL_CUSTOMER_CALL_ALLOWED=NO; không push
- `W-0117` — CT-DOC-02 không có trong docs/traceability-tests.md · Residual: Không đổi business semantics hay quyền; hosted CI/deploy/UAT và external closures NOT_RUN; không gọi khách thật; REAL_CUSTOMER_CALL_ALLOWED=NO; không nhận external closure pack vào scope
- `W-0123` — CT-DOC-02 không có trong docs/traceability-tests.md · Residual: Conservative compatibility: deprecate/ignore wire fields, giữ legacy DB read/rollback, không drop; M3 usage/sign-off OWNER_DATA_REQUIRED; target DB OWNER_DATA_REQUIRED / TARGET_DB_NOT_RUN; hosted CI NOT_RUN; local wor…
- `W-0124` — docs/evidence/W-0124/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-DOC-02 không có trong docs/traceability-tests.md · Residual: .NET 804/804; job api_contract_diff full exit 0 (trước đó exit 1 cả tại HEAD lẫn W-0123); UI typecheck/223/223/build; 7 node gate; traceability 487; detect-changes HIGH 200/85/12 process — toàn bộ do một call site add…
- `W-0125` — docs/evidence/W-0125/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: Không gate nào được đóng. M3 sign-off vẫn OWNER_DATA_REQUIRED; target DB OWNER_DATA_REQUIRED / TARGET_DB_NOT_RUN vì không có psql, secret, endpoint, credential hoặc authority/ticket; hosted CI vẫn NOT_RUN; local Postg…
- `W-0126` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Không gate external nào được đóng. Image sha256:4c76d318… thành STALE_REBUILD_REQUIRED vì build từ context CRLF cũ; Tám phát hiện đều đã xử lý. F7 chỉ giảm nhẹ được bằng tài liệu: 11 WAV audition và model bundle vẫn c…
- `W-0127` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Không gate nào được đóng. Owner vẫn phải nghe; Legal/Security/Infra/Platform vẫn phải trả lời phiếu. REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0128` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: M3 role→tier/sign-off/client/shared E2E, Platform custody/labels, hosted CI/target DB/deploy/UAT/production remain external; migration W0122 name retained as historical alias; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0129` — Residual: Chín reason chi tiết hiện chỉ observable ở service boundary; M3 public route vẫn 400 IVR_MALFORMED_REQUEST hoặc 422 IVR_CONTACT_INVALID. Bất kỳ wire exposure nào cần M3/owner ký; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0130` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Branch chưa push/merge; hosted CI, target DB/deploy/UAT và M3/Platform approvals vẫn external; không rewrite main@2a4f45d; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0131` — Residual: Không calibrate ExpectedCallDurationSeconds/callSeconds — cần đo thật W-0008; volume 800–1200 cuộc/phiên vẫn OWNER_DATA_REQUIRED; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0132` — CAP-CALIB-03 không có trong docs/traceability-tests.md; CAP-DRIFT-05 không có trong docs/traceability-tests.md; CAP-MODEL-01 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: Vẫn UNCALIBRATED: W-0132 chỉ khóa drift, không calibrate. Volume 800-1200 vẫn OWNER_DATA_REQUIRED và còn lệch đơn vị đơn/ngày vs cuộc/phiên; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0134` — CAP-ALERT-04 không có trong docs/traceability-tests.md; CAP-DRIFT-05 không có trong docs/traceability-tests.md; CAP-MODEL-01 không có trong docs/traceability-tests.md; và 2 TestId khác · Residual: Hướng 1 không đóng được bằng W-0134: nó còn cần một quyết định thứ ba chưa ai hỏi, là arrival profile. M8-OD-C và OD-19 vẫn PENDING; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0135` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Không tự chốt số kênh hay attempt policy — W-0135 chỉ gỡ fact sai và trả các con số chưa ký về đúng trạng thái chưa ký. Mốc 2G 15/09/2026 giữ nguyên vì tra lại thấy đúng; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0136` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: .docx cùng tên không được cập nhật — đúng thông lệ đang có (cf2d884 cũng chỉ sửa .md), nhưng nghĩa là hai bản đã lệch. Tên bước "GSM/SIM Call Execution" cố ý giữ; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0137` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: OD-20=IMPLEMENTED / OPTION_1_WITHDRAW; W-0137 vẫn TESTS_PASS, không tự nâng ACCEPTED. Artifact _SUPERSEDED chỉ giữ audit/recovery, không phải tài liệu hiện hành; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0138` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Không tự đóng hay sửa nội dung quyết định nào — chỉ làm chúng hiện ra. OD-VOICE-05 có dấu hiệu mâu thuẫn với evidence W-0128 §12.3, ghi thành residual chứ không xử trong W-0138. REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0139` — CT-OBS-STAGING-13 không có trong docs/traceability-tests.md; IT-IMG-E2E-05 không có trong docs/traceability-tests.md; IT-OBS-EXPORT-11 không có trong docs/traceability-tests.md; và 2 TestId khác · Residual: B-06 vẫn mở: manual staging job chưa chạy vì thiếu endpoint/credential/retention/access + exact image/SHA + dashboard/query + alert fire/recovery OWNER_DATA_REQUIRED; W-0063 không đổi; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0140` — docs/evidence/W-0140/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO (gói ghi dạng cũ: "REAL_CUSTOMER_CALL_ALLOWED=false") · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Rung 0, 8/138 ACCEPTED, 90 TESTS_PASS, 16 BLOCKED_EXTERNAL; không Work ID nào được nâng ACCEPTED; W-0122/external signatures/target DB/hosted CI/shared integration vẫn blocked; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0141` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: W-0137/W-0141 không nâng ACCEPTED; 11 external gate/23 open decision/rung 0 giữ nguyên; artifact _SUPERSEDED chỉ dành audit/recovery, không được phát hành; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0144` — Residual: DT04_LOCAL_COMPLETE; production adapter vẫn BLOCKED_EXTERNAL: vendor API/sandbox, raw disposition matrix, recording-off/caller-ID/DTMF/health, Security-signed resolver/trust boundary, Vault/KMS custody/rotation và R-0…
- `W-0155` — CAP-INTAKE-MODE-02 không có trong docs/traceability-tests.md; CAP-INTAKE-MODE-03 không có trong docs/traceability-tests.md; CAP-INTAKE-TEMPLATE-04 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: Validator chỉ xác nhận authority metadata, không xác nhận signer authority ngoài đời và không thay owner approval/calibration/shared E2E; external submission 0/4; không sửa runtime/model/scheduler/policy/channel count…
- `W-0156` — CAP-INTAKE-MODE-02 không có trong docs/traceability-tests.md; CAP-INTAKE-MODE-03 không có trong docs/traceability-tests.md; CAP-INTAKE-RECEIPT-05 không có trong docs/traceability-tests.md; và 2 TestId khác · Residual: Receipt không xác minh authority ngoài đời, không phải approval/calibration/shared E2E; external receipt 0, data 0/4; không lưu raw data/secret/PII; không sửa runtime/model/scheduler/policy/channel count; REAL_CUSTOME…
- `W-0157` — CAP-INTAKE-RECEIPT-VERIFY-06 không có trong docs/traceability-tests.md · Residual: Verifier không tự tạo trust anchor, không xác minh signer authority ngoài đời và không thay raw bundle validation/calibration/shared E2E/approval; receipt validator-hash cũ fail current binding; không sửa runtime/mode…
- `W-0158` — CAP-INTAKE-LEDGER-07 không có trong docs/traceability-tests.md; CAP-INTAKE-RECEIPT-VERIFY-06 không có trong docs/traceability-tests.md · Residual: Không dùng ledger như authority/approval/calibration; tail truncation cần external head checkpoint mới phát hiện; không lưu raw bundle/rows/path/signer data; không sửa runtime/model/scheduler/policy/channel count; REA…
- `W-0159` — CAP-INTAKE-CHECKPOINT-08 không có trong docs/traceability-tests.md; CAP-INTAKE-LEDGER-07 không có trong docs/traceability-tests.md · Residual: LOCAL_LEDGER_HEAD_CHECKPOINT_VERIFIER_READY, không external trust-store integration; caller phải lấy latest/monotonic hash ngoài ledger+checkpoint, nếu dùng checkpoint cũ hợp lệ thì stale rollback không tự phát hiện. …
- `W-0161` — Residual: Chỉ đóng local disposable-PostgreSQL gap. External signatures/artifacts/shared E2E/calibration/production giữ nguyên; Docker AI local bị disable để engine khởi động, stale runtime dirs được move làm backup chứ không x…
- `W-0162` — Residual: AUTONOMOUS_LOCAL_QUEUE=EMPTY sau khi rà 13 overlay workstream; residual đều external input/authority/shared E2E hoặc CODE_NOT_AUTHORIZED, B2 N/A. Không gọi local fault injection là shared/staging/UAT/production proof;…
- `W-0164` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_OFFLINE_ROUTING_VALIDATOR_READY; partial-ready 1..4 batch được kiểm độc lập nhưng authority vẫn metadata-only. 0 external routing input, 0/5 dispatch/response; không source/runtime/outbound mutation; REAL_CUSTOM…
- `W-0165` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_RESPONSE_PROVENANCE_VALIDATOR_READY / EXTERNAL_AUTHORITY_UNVERIFIED; no external response or approval ledger mutation. W-0163 vẫn 0/5 dispatch/response; no source/runtime/outbound change; REAL_CUSTOMER_CALL_ALLO…
- `W-0167` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: FULL_OFFLINE_SOLUTION_PASS / NO_SOURCE_CHANGE. Chỉ đóng local D8 current-tree gap; không suy thành M3/shared/staging/UAT/production evidence. W-0163 vẫn blocked; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0168` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_SECURITY_GATE_PASS / HOSTED_CI_NOT_RUN. GitNexus pre-edit impact LOW 0 process; no app/runtime/OpenAPI/DB/prod-config change. External secret custody/vendor/staging/UAT/production unchanged; REAL_CUSTOMER_CALL_A…
- `W-0170` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_PROVENANCE_CHAIN_COMPLETE / EXTERNAL_EVIDENCE_NOT_RECEIVED / 0_OF_5_DISPATCHED / NO_GATE_PROMOTION; authority truth và external receipt hash vẫn cần chief auditor đối chiếu system-of-record; REAL_CUSTOMER_CALL_A…
- `W-0172` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Local scope hoàn tất. M3 còn phải giao assembler/CDC + generic consumer, Product approval và shared E2E; exact candidate/hosted verdict thuộc W-0177; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0173` — Residual: M8_LOCAL_CALLBACK_READY / ACK_MEDIA_FAIL_CLOSED / EXTERNAL_E2E_NOT_RUN / DELIVERY_DISABLED. Current PostgreSQL/Chaos rerun ENV_BLOCKED vì Docker server pipe không có; W-0162 7/7+8/8 không được tái gán cho current fix.…
- `W-0174` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: OFFLINE_VALIDATOR_READY / EXTERNAL_E2E_NOT_RUN / DELIVERY_DISABLED. Không network/DB/runtime/guard mutation; M3 consumer/OAS/CDC, Security auth/custody, Platform sandbox/network/TLS và 5 sign-off vẫn external; validat…
- `W-0176` — Residual: CURRENT_PIN_CHAIN_VALID / BLOCKED_EXTERNAL / 0_OF_5_DISPATCHED / NO_GATE_PROMOTION. Exact clean candidate and hosted/Docker gates remain; no runtime/schema/dispatch mutation; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0177` — Residual: Candidate là local verification boundary, không production approval. External signatures/evidence và shared E2E vẫn thiếu; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0178` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: M3_D06_EVIDENCE_NOT_RECEIVED / STRATEGY_UNSIGNED / CODE_NOT_AUTHORIZED; validator PASS chỉ cho phép shared-E2E review, không authorize revoke/runtime/production/call thật
- `W-0179` — Residual: SYNTHETIC_ONLY / EXTERNAL_DECISIONS_NOT_RECEIVED / RUNTIME_NOT_AUTHORIZED; validator PASS không thay dispatch/receipt/signature/authority/shared E2E; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0180` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: OFFLINE_VALIDATOR_READY / EXTERNAL_INPUT_NOT_RECEIVED / PRODUCTION_POLICY_NOT_APPROVED / CODE_NOT_AUTHORIZED; PASS chỉ cho phép runtime implementation review riêng sau authority/shared-E2E/cutover/release evidence; RE…
- `W-0181` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: OFFLINE_M3_SIGNOFF_VALIDATOR_READY / M3_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED. PASS chỉ cho phép mở implementation review riêng; không authorize field implementation, migration, release hoặc cuộc gọi thật; REAL_CUSTO…
- `W-0182` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: OFFLINE_REGISTRY_DECISION_VALIDATOR_READY / EXTERNAL_PROVIDER_AND_SIGNATURES_REQUIRED / DATA_0_OF_4 / CALIBRATION_NOT_RUN / CODE_NOT_AUTHORIZED; PASS chỉ cho phép provider-specific adapter review; REAL_CUSTOMER_CALL_A…
- `W-0183` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: OFFLINE_PRODUCTION_BUNDLE_VALIDATOR_READY / EXTERNAL_DECISIONS_NOT_RECEIVED / CODE_NOT_AUTHORIZED; PASS chỉ cho phép implementation review riêng, không authorize OpenAPI/DB/vault/resolver/adapter/egress/secret/real call
- `W-0184` — Residual: SYNTHETIC_ONLY / EXTERNAL_DECISIONS_NOT_RECEIVED / CODE_NOT_AUTHORIZED; không thay dispatch/receipt/signature/authority/producer/shared E2E; không tự chọn issuer/resolver/TTL/custody/trust boundary; REAL_CUSTOMER_CALL…
- `W-0185` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: OFFLINE_EVIDENCE_VALIDATOR_READY / REAL_SIM_AND_PRODUCTION_EVIDENCE_NOT_RECEIVED; Helm/container/converter rerun ENV_BLOCKED/NOT_RUN do Docker engine; validator PASS chỉ cho phép evidence review, không đạt lab/product…
- `W-0186` — Residual: CURRENT_PROVENANCE_CHAIN_VALID / BLOCKED_EXTERNAL / 0_OF_5_DISPATCHED / NO_GATE_PROMOTION; không dispatch/receipt/authority giả, không runtime; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0187` — Residual: OFFLINE_DECISION_BUNDLE_VALIDATOR_READY / EXTERNAL_BUNDLE_AND_SIGNATURES_NOT_RECEIVED / RUNTIME_NOT_AUTHORIZED; PASS chỉ cho phép implementation review sau structured bundle + S-06 closure thật; REAL_CUSTOMER_CALL_ALL…
- `W-0188` — Residual: LOCAL_INTAKE_AND_REGISTRY_CHAIN_CLEAN / DATA_0_OF_4 / CALIBRATION_NOT_RUN / EXTERNAL_SIGNATURES_REQUIRED / CODE_NOT_AUTHORIZED; không provider selection, trust-store connection, adapter hoặc real call
- `W-0189` — Residual: ACTIVE_DOCS_ALIGNED / HISTORICAL_EVIDENCE_PRESERVED / B1_EXTERNAL_INTAKE_DEFERRED_BY_OWNER; full clean candidate/security/UI/hosted CI là work riêng; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0190` — Residual: LOCAL_P0_1_COMPLETE / MOCK_ONLY / PROVIDER_OUTAGE_FAIL_CLOSED; mutation approval, staging/production và external gates không đổi; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0191` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_P0_2_COMPLETE / DEVELOPMENT_MOCK_ONLY / NO_WORKER_OR_VENDOR_CALL; P0.3 image UI là blocker nội bộ kế; Module 3, staging, lab/SIM và production vẫn ngoài scope; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0192` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_FULL_LIFECYCLE_RUNNABLE / MOCK_ONLY; blocker nội bộ còn lại là migration expand-contract; gói ký OD-V1 trên worktree-gd0-fixes chưa merge và sẽ cần W-0193 vì nhánh đó dùng trùng W-0190/W-0191; REAL_CUSTOMER_CALL…
- `W-0193` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Chạy trên worktree cô lập worktree-gd0-fixes vì một session Codex đang ghi song song vào main worktree; merge về main là bước riêng. Không đổi contract/OpenAPI/migration, không đóng gate ngoài nào; REAL_CUSTOMER_CALL_…
- `W-0194` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Docs-only, không đổi runtime/OpenAPI/migration. Bốn dòng còn mở: OD-V1-09 nửa sau và OD-V1-10 chờ số đo; OD-V1-11 và OD-V1-21 chờ người thứ hai — chữ ký không tạo ra người. Việc code mà chữ ký mở ra thuộc GĐ 2, liệt k…
- `W-0195` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: PRODUCTION_CALL không được seed nên quay số thật vẫn bị từ chối ở mọi môi trường. Vế bốn mắt vẫn cần người thứ hai (OD-V1-11/OD-V1-21). Còn lại của GĐ 2: attempt policy production + khung giờ (OD-V1-08/16), dial token…
- `W-0196` — docs/evidence/W-0196/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: Cleanup only in a later release after consumer inventory/observation and rollback-window closure; MOCK/MOCK/NO
- `W-0197` — docs/evidence/W-0197/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: Content-hash-bound working-tree evidence, không clean exact-SHA/hosted proof; crash/full-worker thuộc P1.2; không vendor/customer call; giữ WIP báo cáo tuần
- `W-0198` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Đăng ký một policy không phải là quyết định dùng nó: intake phân giải version mà task khai, nên phải có task mang gh-247-prod-v1 tới thì mới có gì chạy trên nó. Khung giờ mặc định bật; tắt nó là một quyết định nhìn th…
- `W-0199` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Sổ là process-local, giống hai vault sở hữu nó: nó chặn trong phạm vi một worker, còn ràng buộc thật ở production đến từ SIM vault. Đã ghi rõ trong XML doc để không ai đọc test xanh thành lời hứa mạnh hơn. Còn lại của…
- `W-0200` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Bump này không đổi một byte nào của hợp đồng — M3 đã sinh client từ draft.22 thì không cần sinh lại, đã ghi vào §4A.7 của bản handover. TARGET_CONTRACT_V1 vẫn DRAFT: một chuỗi phiên bản không có hậu tố draft từ nay kh…
- `W-0201` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Hai luật đều đúng và nói về hai việc khác nhau — "đường dẫn tuyệt đối có tính đúng không" với "cuối đường dẫn đó có gì không" — nên fixture của test nay thoả luật thứ hai, chứ không nới luật nào để luật kia qua được. …
- `W-0202` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Xoay baseline không phải phê duyệt: nó nói owner chấp nhận thay đổi này, không nói consumer nào đã migrate, và TARGET_CONTRACT_V1 vẫn DRAFT. Bản 1.0.0 của OD-V1-02 không đi kèm lượt xoay này — nó đã bị rút ở W-0200 và…
- `W-0203` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: F-1 (HIGH) mở: 10 request /eligibility-checks đồng thời → 9 trả HTTP 500 vì PostgresIdempotencyStore.ExecuteAsync chạy SERIALIZABLE đọc-rồi-ghi ivr_idempotency_keys, 40001 bị ánh xạ thành IVR_INTERNAL_ERROR. Không vá …
- `W-0205` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Một phát hiện phụ đáng giữ: ApiBehaviorMatrixTests đỏ trên clone sạch vì thiếu deploy/ci/node_modules — nó fail chứ không tự skip, đúng như evidence W-0197 tuyên bố. Nghĩa là freeze checkout phải chạy npm --prefix dep…
- `W-0206` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Dòng này được ghi hồi tố. A-0588 chọn bỏ qua id W-0206 vì 4d0c761 đã dùng nó trong commit message mà chưa có dòng ledger nào. Nhưng bỏ qua để lại việc thật vô hình với ledger và với readiness board — gate-status không…
- `W-0207` — docs/evidence/W-0207/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: P2.2 chưa đạt exit: mới có một nửa của cả ba chiều. Producer của M3, M3 revalidate rồi đổi trạng thái đơn, và BFF của M3 gọi admin API không quan sát được từ phía này. Chữ ký thật cần năm vai (M8_OWNER/M3_OWNER/SECURI…
- `W-0208` — Residual: Runtime không đổi một dòng — có chủ đích: đổi TTL là contract change cần M3/Security (DTK-02/DTK-06). OpenAPI cố ý chưa sửa: bump hash đã ghim cần re-pin có review (W-0204), đúng thứ tự là gộp vào lượt sửa cùng lúc kh…
- `W-0209` — Residual: Runtime không đổi một dòng. OpenAPI cố ý chưa sửa: CorrelationId/IdempotencyKey vẫn {type: string} trần trong khi GeneratedCorrelationId đã ghi đúng minLength:1, maxLength:128, pattern:'^[A-Za-z0-9._:-]+$' — sửa bump …
- `W-0210` — Residual: Không đụng quyết định của owner — explicit-only là vị trí đúng, chỉ hai ví dụ tín hiệu sai; không đổi trạng thái dòng register (việc của chief auditor, mục A2). V1 không có tín hiệu opt-out tường minh nào và lời thoại…
- `W-0211` — Residual: Mục 0.4 đóng hẳn — không còn việc bên ngoài. Không sửa runtime, không sửa OpenAPI (ResultType ở đó vốn đã đúng: enum 11 + description giải thích IVR không phát hai mã pre-call). Phát hiện phụ đáng giữ: bản changelog .…
- `W-0212` — CAP-CALIB-03 không có trong docs/traceability-tests.md; CAP-DRIFT-05 không có trong docs/traceability-tests.md; CAP-MODEL-01 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: Không đổi một giá trị nào và không ép quan hệ nào giữa 35 và 50 — chọn đáp án cho mười giây đó là tuyên bố một phép đo, đúng thứ constant này sinh ra để không làm; pin thì bắt được drift, assert một công thức thì đóng…
- `W-0213` — Residual: Không sửa hành vi — admin gate có cần scope theo env hay không là câu của Security/Platform. Đã sửa hai đề xuất không thi hành được trong B4 của audit gốc: "seed theo environment" hỏng theo hai cách độc lập, và "test …
- `W-0214` — Residual: X2 giữ gate BẬT rồi nới cả ngày, không tắt — tắt thì smoke thôi không phủ đoạn code ra quyết định. X3 chỉ log lúc đổi trạng thái: loop quay 100ms dưới LocalMockE2E, một dòng mỗi vòng sẽ chôn vùi chính cái đêm nó cần g…
- `W-0215` — Residual: Không đổi tham số nào — chọn End là quyết định owner; mục này tồn tại để owner có con số thật mà quyết. Sửa đề xuất của audit gốc: End ≥ 21:05 chỉ cứu Giờ Vàng và vẫn bỏ rơi 24/7; con số đủ cho cả hai là End ≥ 21:07:3…
- `W-0216` — DG-DR-03 không có trong docs/traceability-tests.md; IT-K8S-NETPOL-04 không có trong docs/traceability-tests.md · Residual: Bài học ghi lại để lần sau không lặp: gate-status.yaml phải sinh bằng node scripts/gate-status.mjs --write, đừng gõ tay; Status §5 chỉ nhận một token trong 12 token, sắc thái để cột Residual; ký tự gạch đứng trong ô b…
- `W-0217` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Không tự nhận là đã chốt cách đọc PACK-09: bốn dòng đầu của cầu nối chưa có trong errata V0.3 (errata mới ghi IVR_OPT_OUT và ba code thừa), nên §3.1 ghi rõ cần chief auditor/Owner xác nhận trước khi M3 ký §4.2. 52 wor…
- `W-0218` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Phát hiện một câu lạc, cố ý không tự sửa: docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md:472 — bảng field callback — ghi result_type là "Một trong 11 giá trị ở §16", trong khi ck_ivr_result_callbacks_result_status…
- `W-0219` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Chỉ dọn cách trình bày, không đổi kết luận nào: mọi phát hiện, số liệu và câu tự đính chính đều giữ nguyên chữ. Phần đã xong rút gọn xuống một dòng vì chi tiết đã nằm trong docs/evidence/W-02xx/ — không xoá thông tin,…
- `W-0220` — Residual: Lượt đầu tiên trong chuỗi W-0208→W-0220 đổi một giá trị runtime — vì là lượt đầu tiên có một quyết định để thi hành. OD-V1-16 ký 08:00–21:00 còn giá trị chạy nay là 08:00–21:08: register đã có dòng correction nhưng tr…
- `W-0221` — Residual: Rẻ đúng lúc này: BFF của M3 chưa tồn tại và admin UI đã ra khỏi phạm vi module — muộn hơn thì đắt hơn; repo không nhìn thấy caller ngoài nên đây là điều đã cân nhắc chứ không phải đã chứng minh. OpenAPI hoãn với lý do…
- `W-0222` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Phần bền của lượt này là luật, không phải hai ref: owner giữ nhánh nên mâu thuẫn CLAUDE.md (cấm nhánh, xoá nhánh lạ) ⟷ W-0130 (cần một nhánh sống lâu dài) phải được gỡ chứ không để treo — giá của việc treo đã thấy: au…
- `W-0223` — docs/evidence/W-0223/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CAP-CALIB-03 không có trong docs/traceability-tests.md; CAP-DRIFT-05 không có trong docs/traceability-tests.md; CAP-MODEL-01 không có trong docs/traceability-tests.md; và 2 TestId khác · Residual: Assertion mới ngược với quyết định của W-0212, có lý do: uncalibrated không có phép đo nên ép một công thức là đóng băng phỏng đoán thành luật (chỉ ghim); calibrated thì cả bốn cùng mô tả một cuộc gọi đã đo nên ép là …
- `W-0224` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Bài học đắt nhất lượt này: ref tracking không phải trạng thái remote, và tôi đã trình một con số 43 cho owner trước khi hỏi remote. Hai claim (a) và (c) đều là tôi lặp lại một nguồn (ref cũ, bullet audit) mà không kiể…
- `W-0225` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Gate thứ hai tìm thấy vì nó nói dối: verify-api-behavior-matrix.mjs in "verdict": "FAIL" ngay trên "failures": [] với 38/38 pass, vì verdict quyết bởi mảng failures cấp cao còn console.log in mảng failures theo từng o…
- `W-0226` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Hai phiếu vẫn NOT_SENT — việc gửi thuộc owner/chief auditor, cùng nhóm với 3.4. Nên chốt chỗ rẽ items_spoken trước khi Platform bắt tay vào INF-A, kẻo phí công dựng mirror cho artifact có thể bị bỏ. Nhưng có một vế đi…
- `W-0227` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Ba đính chính ghi ở phụ lục thay vì sửa thân: §2 của gói ghi "đủ L1–L6" trong khi phiếu Legal có bảy câu; internal_mirror_gate chưa có dòng riêng trong bảng §1 ngày 29/08 mà nay là gate thật sau W-0225; và việc gói bị…
- `W-0243` — Residual: Cái giá ghi ra chứ không giấu: một tên hàng viết "đường Nguyễn Huệ" nay qua được guard sản phẩm, trong khi IsSafeText vẫn từ chối — đổi lại tổ yến đặt hàng được. UT-PII-PRODUCT-02 ghim rằng dạng thật sự mang địa chỉ g…
- `W-0245` — Residual: 2.1 nhỏ hơn hẳn nó tự mô tả: ba guard nhận đúng một giá trị và không gì đã ký nói khác, nên không có mâu thuẫn để gỡ và không phải sửa cùng lúc sáu chỗ; việc còn lại là ký equality thành quyết định, mà nay owner ký đư…
- `W-0246` — Residual: Owner ký: TTL = đúng confirmation-window end, thay thế vế +60s. +60s chưa từng thi hành được không phải vì thiếu guard mà vì thừa ba — giao của luật intake và luật persistence là đúng một giá trị. Equality đứng vững c…
- `W-0247` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: 2.5 là mục duy nhất trong nhóm 2 mà cái giá của việc không quyết rơi vào khách hàng: cả hai vế của nó đã kiểm lại và đều đúng, nghĩa là khách đã hủy đơn vẫn nhận cuộc gọi hỏi xác nhận chính đơn đó. Bốn mục kia sai ở t…
- `W-0248` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Giới hạn phải nói trước để không ai hứa nhầm: LoadAsync đọc AsNoTracking không FOR UPDATE, nên revoke rơi vào khoảng LoadAsync → gateway dial vẫn lọt — và không nên đóng, vì đóng nghĩa là giữ một transaction DB xuyên …
- `W-0249` — Residual: Đề xuất m8-17 §5 của tôi sai và đã sửa trước khi dựng: nó viết "revoke mang version ≤ version đã lưu thì từ chối", nhưng order_version là chuỗi mờ IVR trả nguyên — OAS gọi là stale-result guard snapshot, IR-06 ghi "IV…
- `W-0250` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Đính chính con số của chính tôi: đầu lượt tôi viết vào docs/api-changelog.md rằng oasdiff cho "162 warnings, và zero errors" — sai. 162 là tổng; phân loại thật là 110 errors + 52 warnings, trong đó 108 error là header…
- `W-0251` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Tại sao đỏ suốt ba mươi work item mà không ai thấy — không phải vì ai bỏ qua, mà vì không gì chạy chúng. Bắc cầu mọi đường gọi (job trong deploy/ci/.yml, npm script trong hai package.json, import giữa các script) thì …
- `W-0252` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Luật áp dụng, viết ra để lần sau khỏi cãi: cập nhật con trỏ (đường dẫn dùng để tìm tệp), không đụng chứng thực (dòng hash). Vì thế W-0186/W-0188 — hai manifest tự liệt kê manifest khác — chỉ đổi cột đường dẫn, hash gi…
- `W-0253` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Hai assertion đã sai sẵn, lộ ra khi rà. image-selftest.mjs đòi ivr-admin-ui phải là service đang chạy trong compose, nhưng docker-compose.dev.yml chưa bao giờ định nghĩa service đó — assertion ấy đỏ với bất kỳ ai chạy…
- `W-0254` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Thứ chặn thật là nhóm (2), và nó lớn hơn vẻ ngoài. Đối chiếu §9 với specs/_review/open-decisions-register.md: 23 trong 28 quyết định đã CLOSED, nhưng IR-06 vẫn ghi "Auth profile + sandbox credential — Security/Platfor…
- `W-0265` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Contract phải sửa vì nếu không thì cầm chắc vòng hai. Ba header runtime vẫn dùng nhưng contract không khai: X-Action-Reason bắt buộc ở cả 8 endpoint danger (AdminAccessOptions.HasDangerEvidence từ chối nếu thiếu) mà k…
- `W-0266` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Đóng quyết định không mở ra mọi thứ, và đây là chỗ dễ đọc nhầm nhất nên phải ghi thẳng. Hai trong ba mục vướng ràng buộc kỹ thuật mà chữ ký không gỡ được: OD-V1-11 — PRODUCTION_REAL đòi ba actor id khác nhau (ScriptCo…
- `W-0267` — docs/evidence/W-0267/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0268` — docs/evidence/W-0268/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0269` — docs/evidence/W-0269/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0270` — docs/evidence/W-0270/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-CI-11 không có trong docs/traceability-tests.md; IT-IMG-BUILD-01 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0272` — docs/evidence/W-0272/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0273` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Sửa 18/09 (W-0316, Lô 3 mục 1): hết chờ bên ngoài. Ngày 17/09 owner chọn S3 — giữ toàn bộ dữ liệu, không đặt kỳ hạn nào — nên không ai điền phiếu; phiếu nay ghi "đóng — không điền số nào". BLOCKED_EXTERNAL → EVIDENCE_…
- `W-0274` — docs/evidence/W-0274/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0275` — docs/evidence/W-0275/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-CI-12 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0276` — docs/evidence/W-0276/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-CI-12 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0277` — docs/evidence/W-0277/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-CI-12 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0278` — docs/evidence/W-0278/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-CI-12 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0279` — docs/evidence/W-0279/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-CI-12 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0280` — IT-IMG-BUILD-01 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY / MOCK; còn một câu hỏi nghiệp vụ chưa đóng: tạm dừng hàng đợi có tính là IVR_CAPACITY_EXCEPTION không, hay IVR_CONFIRMATION_WINDOW_EXPIRED như runtime đang trả — hai nhãn dẫn tới hai hành động khác nhau ph…
- `W-0281` — gate-status.yaml không trỏ tới gói bằng chứng nào · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Phiếu ở trạng thái READY_TO_DISPATCH / NOT_SENT — chỉ owner gửi được. Không đổi mã nguồn, không đổi cổng kiểm
- `W-0282` — IT-IMG-COMPOSE-03 không có trong docs/traceability-tests.md · Residual: LOCAL_ONLY / MOCK; REAL_CUSTOMER_CALL_ALLOWED=NO. Một khoảng trống phải chủ Module 8 quyết, không tự lấp: POST /eligibility-checks là thứ đưa task rời HELD_MOCK, nhưng worker không có vòng lặp nào gọi nó (10 hosted se…
- `W-0283` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_ONLY/MOCK; vòng mặc định tắt; M3 giữ CALL_REQUIRED và order revalidation; CI/SIM/M3 thật vẫn mở; full initial matrix bị invalidated do source thay đổi, có rerun cuối; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0284` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Không suy chữ ký từ im lặng; nhãn pause đang chờ owner; real calls NO
- `W-0286` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: W-0292 tại 179a5eb có 31 job PASS gồm full suite/image/K8s/security/privacy/publish; dev thiếu cluster, staging skipped; M3/SIM/production NOT_RUN, real calls NO
- `W-0287` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Chaos lộ loopback fixture, review gate lộ false green, xử lý tiếp; hosted chưa push; pause/scan pending; real calls NO
- `W-0288` — docs/evidence/W-0288/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Không đổi upstream fault target hay runtime; hosted NOT_RUN; real calls NO
- `W-0289` — docs/evidence/W-0289/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Không nới gate; không chạy security audit; hosted NOT_RUN; real calls NO
- `W-0290` — Residual: Không thay runtime hay chữ ký M3; lab/production/real calls chưa được mở
- `W-0291` — docs/evidence/W-0291/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · CT-CI-04 không có trong docs/traceability-tests.md · Residual: Không thay API contract/runtime hoặc nới scanner; hosted là bước kế tiếp
- `W-0293` — docs/evidence/W-0293/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Hosted pipeline cần chạy trên commit sửa; không đổi allowlist hoặc runtime
- `W-0294` — docs/evidence/W-0294/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Cần pipeline chứa sửa JUnit; dev/staging đích chưa xác nhận; runtime không đổi
- `W-0295` — docs/evidence/W-0295/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Cần pipeline mới; không ký bundle hoặc đổi authority/readiness
- `W-0296` — docs/evidence/W-0296/README.md thiếu REAL_CUSTOMER_CALL_ALLOWED=NO · không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Không reset data hoặc thay runner config; toàn pipeline/publish/deploy còn theo W-0292
- `W-0297` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Phân loại sai một lần và đã sửa: m8-05…m8-10 mở đầu bằng M8_OWNER_SIGNED/đã ký nên lượt đọc đầu xếp vào nhóm xong — đọc hết chuỗi trạng thái thì cả sáu đều kèm M3_..._SIGNOFF_REQUIRED hoặc CODE_NOT_AUTHORIZED, tức mới…
- `W-0298` — Residual: Không đổi wire: TASK_BLOCKED_OPERATIONAL đã có trong enum decision (OAS :1365), blocked_reasons là array of string nên thêm reason là additive. Phương án đầu sai, ghi lại: kế hoạch định cho IVR dời T0/cửa sổ tới giờ m…
- `W-0299` — Residual: Không đổi code runtime, không đổi wire. Quét deploy/ và mọi docker-compose.yml: không môi trường nào đặt IVR_PRODUCTION_TARGET_V1_FIELDS_APPROVED, nên trước lượt này mọi môi trường chạy cờ bật và sau lượt này chạy cờ …
- `W-0300` — Residual: Phương án đầu bị database từ chối, ghi lại: UPDATE … SET approved_for_production=FALSE bị trg_ivr_attempt_policies_immutable raise vô điều kiện (BEFORE UPDATE FOR EACH ROW, không phân biệt cột) — đó là W-0151 cưỡng ch…
- `W-0301` — Residual: gitnexus_impact = HIGH (20 điểm chạm, 4 implementation, 13 test) — đã báo owner và owner chọn phương án A trước khi sửa, theo CLAUDE.md. Tự lật lập luận cũ: doc-comment RuntimeGateApprovalKinds từng viết administratio…
- `W-0302` — Residual: gitnexus_impact = HIGH (23 điểm chạm, 4 execution flow, 21 test) — đã báo owner trước khi sửa. Giữ hai reason code riêng thay vì một MISMATCH để câu trả lời nêu chiều nào sai. Guard persistence giữ nguyên: lớp cuối ch…
- `W-0303` — Residual: Vault production không tự làm protector — lab là protector của chính nó vì bảo vệ của nó là vân tay tự sinh được; production nhận khóa Platform, nên deployment còn UnavailableOpaqueValueProtector fail closed ngay lần …
- `W-0304` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: W-0297 đã làm 11 gate đỏ và không ai biết: nó xóa 27 file, 12 trong đó là nguồn ghim hash trong external-decision-artifacts.sha256 — chính file mà header của nó viết “update the matching line here in the same commit”;…
- `W-0305` — UT-DOC-PII-03 không có trong docs/traceability-tests.md · Residual: Ba phát hiện khi đọc mã: (1) InternalAdminApiService.cs:760 từ chối enable mọi kênh AdapterMode != MOCK trong khi disable không có ràng buộc nào tương ứng ⇒ kênh trunk quarantine không đưa lại vận hành được qua API; g…
- `W-0306` — Residual: Nguyên nhân krb5 chỉ hiện khi có postgres thật trả lời: lần thử đầu với host không tồn tại không tái lập được vì DNS hỏng trước khi Npgsql kịp thương lượng. Sửa ở một chỗ thay vì 7 chuỗi kết nối — đó mới là thứ giải q…
- `W-0307` — Residual: Ba dòng trong bốn của chính kế hoạch này sai khi đối chiếu mã, và kế hoạch đó do tôi viết: (a) cột là TargetType/TargetId chứ không phải object_; (b) PiiMaskingFilter không mask — nó là guard fail-closed, nên dựa vào …
- `W-0308` — Residual: Kế hoạch đòi 6 gạch đầu dòng, 4 đã đạt sẵn từ lượt 15/09 (trần dùng chung, vòng bảo trì khi đầy, drain, tách rate) — chiều ngược với W-0307, nơi 3/4 dòng kế hoạch sai; ở đây kế hoạch đòi ít hơn thực tế đã có. Việc thậ…
- `W-0309` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Lượt này bắt đầu bằng việc bắt một câu sai của chính tôi, đã lặp 3 lần trong 3 phiên: “việc gỡ được nhiều nhất là gửi 6 phiếu nhóm A — đã soạn xong, tốn 0 ngày công”. Cả hai tiền đề sai. (a) W-0297 đã xoá cả 6 file kh…
- `W-0310` — UT-DOC-PII-03 không có trong docs/traceability-tests.md · Residual: Phiếu ghi sai phiên bản contract 2 bản. IR-07 ghi draft.27 và bảo M3 “sinh lại client từ bản hiện hành draft.27”; spec thực tế đã là draft.29. Đã sửa, và kiểm hai changelog để nói đúng mức độ: 27→28 no changes to repo…
- `W-0311` — Residual: ⚠️ Cảnh báo CRITICAL đã báo owner trước khi sửa, và đã thiết kế vòng qua. IDialTokenResolver có 214 ký hiệu phụ thuộc, 53 trực tiếp, 5 implementation. Không đụng: chỉ thêm một field optional có giá trị mặc định vào re…
- `W-0312` — Residual: ⚠️ Cảnh báo CRITICAL/HIGH đã báo Toàn ở kế hoạch; cả năm luồng bị chạm đều thuộc phía nhận task, và thiết kế giữ nguyên chữ ký CreateDomainSnapshot, DialTokenReference, ConfirmationTaskSnapshot, IDialTokenResolver. Lô…
- `W-0313` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Hai chỗ trong trích dẫn nguyên văn (OD-V1-11, lập luận bác A1) đổi đúng một từ và đánh dấu bằng ngoặc vuông — một trích dẫn bị đổi chữ mà không đánh dấu là trích dẫn sai. Đổi manifest thay vì ghi luật vào tài liệu: kế…
- `W-0314` — Residual: S8 — DSAR chưa có lối chạy, chờ Sếp + Toàn quyết ai được xoá và bằng gì · IR-07 A-9 hứa "xoá khi khách yêu cầu", cùng gốc S8 · retention-period-proposal.md còn "chờ pháp chế điền số" (Lô 3 mục 1) · REAL_CUSTOMER_CALL_…
- `W-0315` — UT-AST-AUDIO-03 không có trong docs/traceability-tests.md; UT-TTS-STATIC-REGION-05 không có trong docs/traceability-tests.md; UT-VOICE-3B-01 không có trong docs/traceability-tests.md; và 4 TestId khác · Residual: Lô 4 phần làm được ngay (Trivy, bản cài nền, cấu hình production nháp) · chờ S1/S2/S5 · chạy lại lab bằng VieNeu khi có bundle model. REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0316` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Lô 3 mục 13 chờ Sếp ký S2 · Lô 4 bước 1, 2, 4 · Lô 5 · REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0317` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Một lượt đọc thật khi có bundle model (tải cần Toàn cho phép) · S2 rủi ro 3: ký với 44 lỗ hay đổi nền Chainguard · phần Chờ của Lô 4 · REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0318` — UT-SCHEMA-BACKCOMPAT-04 không có trong docs/traceability-tests.md; UT-X-01 không có trong docs/traceability-tests.md; UT-X-02 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: Toàn duyệt từng đợt và tự chuyển ACCEPTED (bước 3), rồi gate-status.mjs --write (bước 4) · REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0319` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Đã thu gói kiểm chứng đầy đủ; kết quả chỉ chứng nhận commit bd9ea5c, không chứng nhận HEAD khác. Toàn đọc Residual và tự nghiệm thu; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0322` — E2E-UI-REPLAY-05 không có trong docs/traceability-tests.md; E2E-UI-REVIEW-05 không có trong docs/traceability-tests.md; UT-ELIG-BLOCK-01 không có trong docs/traceability-tests.md; và 1 TestId khác · Residual: Owner quyết định nghiệm thu P2 và kết thúc 4 P3 theo W-0253; F1 bộ sinh bỏ lọt test UI retired cần sửa ở work tiếp theo; không đổi 13 trạng thái; REAL_CUSTOMER_CALL_ALLOWED=NO

