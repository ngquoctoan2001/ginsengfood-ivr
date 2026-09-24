# Danh sách đề nghị nghiệm thu — `16fa1e4cb2f7efa3566e45fe5c25404af0a83eae`

Script **chỉ đọc**: không sửa tracker. Chỉ Toàn chuyển một dòng sang `ACCEPTED`, sau khi
đọc bằng chứng — danh sách này không thay cho việc đọc.

| Nguồn | Trạng thái |
| --- | --- |
| Kết quả test (`C2`) | 4 file `.trx`, 1200 kết quả · SHA/hash/đủ project đã kiểm |
| Gate sweep (`C4`) | ✅ GATE_SWEEP_PASS 43/43 |
| Gate mở rộng: image, K8s, oasdiff, security scan (`C2`) | ✅ EXTENDED_SWEEP_PASS 5/5 |

## Nấc 1

Prompt đã lên kế hoạch đã xong (`ACCEPTED`, `N/A`, `CANCELLED`): **37/54**
· `BLOCKED_INTERNAL`: **0**. Nấc 1 đạt khi hai số này là
**54/54** và **0**.

## Tổng theo đợt

| Đợt | Ứng viên | ĐẠT | XEM | KHÔNG ĐẠT | CHƯA KIỂM |
| --- | ---: | ---: | ---: | ---: | ---: |
| `P0` | 1 | 0 | 1 | 0 | 0 |
| `P5` | 2 | 0 | 0 | 2 | 0 |
| `P6` | 2 | 0 | 2 | 0 | 0 |
| `P7` | 3 | 0 | 3 | 0 | 0 |
| `P10` | 2 | 0 | 2 | 0 | 0 |
| `UNPLANNED` | 27 | 0 | 23 | 4 | 0 |

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
| `W-0036` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0037` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |

- `W-0036` — CT-OAS-01 không có trong docs/traceability-tests.md; CT-OAS-02 không có trong docs/traceability-tests.md; CT-OAS-03 không có trong docs/traceability-tests.md; và 4 TestId khác · Residual: không có Pact — khác biệt có chủ ý, pact chỉ có giá trị khi cả hai bên cùng chạy mà Sales chưa có gì; không có E2E trình duyệt — pane preview không composite frame; provider thật vẫn BLOCKED_EXTERNAL
- `W-0037` — PT-SOAK-02 không có trong docs/traceability-tests.md; SEC-AUTHZ-05 không có trong docs/traceability-tests.md; SEC-ERR-06 không có trong docs/traceability-tests.md · Residual: rate limiting CHƯA CÓ (chỉ có ánh xạ 429, không middleware) — ngưỡng là quyết định vận hành chưa ai duyệt; soak 4–8h NOT_RUN; không tuyên bố ngưỡng latency D-04 vì cần Sales thật trong vòng lặp; năng lực 32 kênh thật …

## Đợt `P6`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0040` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0041` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0040` — Residual: OTLP export BLOCKED_EXTERNAL (W-0063) — instrumentation dùng BCL nên gắn exporter sau không sửa call site; mới instrument 1/5 chặng (callback), 4 chặng còn lại chưa có span; dependency_probing_available vẫn false — đó…
- `W-0041` — Gate đạt có điều kiện: CAP-ALERT-04 PASS_WITH_NOT_PROVEN=COST_METRIC: pool prod khớp đỉnh của mô hình; cost_per_confirmed_order chưa đo vì còn chờ báo giá (W-0008) · Residual: chưa exporter OTLP (W-0063) nên chưa tín hiệu nào rời tiến trình — đây là artifact as-code đã qua bộ đánh giá luật thật, KHÔNG phải hệ đang chạy; không có screenshot dashboard (§10) vì không có Grafana để chụp và tôi …

## Đợt `P7`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0043` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0044` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0047` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0043` — Residual: chưa có registry (§5 NEED_CONFIRMATION) nên chưa push lần nào; ~~compose smoke chưa chạy hết luồng DTMF tới callback; chưa seed attempt policy~~ đã đóng 2026-08-19 bằng IT-IMG-E2E-05 + deploy/docker/dev-seed/seed.sql …
- `W-0044` — Residual: ~~IT-K8S-NETPOL-04 NOT_PROVEN~~ đã đóng 2026-08-19 — kết luận cũ "cluster không thực thi, cần Calico/Cilium" là sai: cluster có thực thi, phép đo bị đua với thời điểm kube-router cài luật iptables cho pod. Nay 5/5 K8S…
- `W-0047` — Residual: drill overlap đã chạy trên HTTP thật: token cũ được nhận 21/21 lượt trong cửa sổ rồi chuyển 403 đúng tại thời điểm hết hạn, token mới 0/28 lượt bị 403, token chưa cấu hình 403 cả 28 lượt — ~~nhưng drill chạy trên một …

## Đợt `P10`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0053` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0055` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |

- `W-0053` — Gate đạt có điều kiện: DG-DR-03 PASS_SINGLE_HOST: RPO=0 và RTO trong ngân sách, nhưng trên một host hai container, không phải multi-AZ · Residual: không multi-AZ — một host, hai container; không mã hoá volume at-rest và không KMS (W-0063); không PITR (logical dump); RTO chưa gồm phát hiện/quyết định/chuyển traffic; chưa có fencing nên promote khi chỉ mất liên lạ…
- `W-0055` — Residual: không có kho dữ liệu riêng — là schema analytics trong cùng database (W-0063), lệch với P10-4 §7 vì DTS-04 chốt 3 deployable; không BI tool nào từng kết nối (grant SELECT là thứ cấp được, chưa ai cấp); chưa đo trên kh…

## Đợt `UNPLANNED`

| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |
| --- | --- | :---: | :---: | :---: | --- |
| `W-0078` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0092` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0094` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0103` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0108` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0116` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0117` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0123` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0124` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0139` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0168` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0169` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0171` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0203` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0205` | `TESTS_PASS` | ✅ | ❌ | 👀 | **KHÔNG ĐẠT** |
| `W-0216` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0225` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0249` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0270` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0280` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0282` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0291` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0297` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0303` | `TESTS_PASS` | ✅ | ✅ | 👀 | **XEM** |
| `W-0335` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0347` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |
| `W-0351` | `EVIDENCE_SUBMITTED` | ✅ | ✅ | 👀 | **XEM** |

- `W-0078` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: retain pin until Testcontainers declares a patched dependency; no production SSH/SIM/Sales execution
- `W-0092` — Residual: contract stays TARGET_CONTRACT_V1=DRAFT; Sales approval remains BLOCKED_EXTERNAL
- `W-0094` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: recording readback retained as defense in depth; provider boundaries/no-egress preserved
- `W-0103` — Residual: MOCK-only; REAL_CUSTOMER_CALL_ALLOWED=NO; Sales endpoint/auth/real payload + one-SIM lab + 32-eSIM provisioning vẫn NOT_RUN/BLOCKED_EXTERNAL; Target V1 vẫn DRAFT; bước kế tiếp W-0048/one-SIM lab chỉ bắt đầu sau khi nh…
- `W-0108` — Residual: Lỗi contract duy nhất không thuộc W-0108: luồng OD-V1-20 sửa 1 dòng summary trong OpenAPI mà chưa re-pin contract-manifest.json (98d226b1… → 7b921b3a…); khôi phục file về bản commit ⇒ contract 22/22. 12 MP3 đoạn cố đị…
- `W-0116` — ID của bề mặt đã gỡ theo quyết định đã ghim, không có test thay thế: UT-UI-CONTRACT-06 · Residual: openapi:lint còn 14 lỗi baseline ngoài W-0116; hosted CI/deploy/UAT NOT_RUN; không sinh SIM/carrier/real-call evidence; REAL_CUSTOMER_CALL_ALLOWED=NO; không push
- `W-0117` — Residual: Không đổi business semantics hay quyền; hosted CI/deploy/UAT và external closures NOT_RUN; không gọi khách thật; REAL_CUSTOMER_CALL_ALLOWED=NO; không nhận external closure pack vào scope
- `W-0123` — Residual: Conservative compatibility: deprecate/ignore wire fields, giữ legacy DB read/rollback, không drop; M3 usage/sign-off OWNER_DATA_REQUIRED; target DB OWNER_DATA_REQUIRED / TARGET_DB_NOT_RUN; hosted CI NOT_RUN; local wor…
- `W-0124` — Residual: .NET 804/804; job api_contract_diff full exit 0 (trước đó exit 1 cả tại HEAD lẫn W-0123); UI typecheck/223/223/build; 7 node gate; traceability 487; detect-changes HIGH 200/85/12 process — toàn bộ do một call site add…
- `W-0139` — Residual: B-06 vẫn mở: manual staging job chưa chạy vì thiếu endpoint/credential/retention/access + exact image/SHA + dashboard/query + alert fire/recovery OWNER_DATA_REQUIRED; W-0063 không đổi; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0168` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: LOCAL_SECURITY_GATE_PASS / HOSTED_CI_NOT_RUN. GitNexus pre-edit impact LOW 0 process; no app/runtime/OpenAPI/DB/prod-config change. External secret custody/vendor/staging/UAT/production unchanged; REAL_CUSTOMER_CALL_A…
- `W-0169` — Việc tài liệu, không có test phần mềm (1 tài liệu có tại commit): Dọn tài liệu theo chỉ đạo owner ở commit c213bf7: xoá 63 tệp SUPERSEDED/HISTORICAL, sửa link, thêm .artifacts/ vào .gitignore; không đổi code hay test. README này ghi lại lượt dọn và phép kiểm. · Residual: Cảnh báo tồn đọng: (a) W-0105 và W-0118 giờ evidence: null trong gate-status.yaml — W-0118 vẫn mang status TESTS_PASS mà không còn evidence pack, owner cần quyết định hạ status hay khôi phục evidence; (b) OD-20 yêu cầ…
- `W-0171` — Việc tài liệu, không có test phần mềm (5 tài liệu có tại commit): Đối soát tài liệu theo code: sửa spec database, functional, workflow và ui cho khớp constraint và quyền thật; ngoài tài liệu chỉ sửa một comment trong DevToolingEndpoints.cs. · Residual: Sửa 18/09 (W-0316, Lô 3 mục 12): (a) đã xử lý ở W-0312 (draft.31, T5) — OAS ghi quyền endpoint thực đòi, IVR_DEV_TOOLING không còn trong spec. (c) đã xanh: progressive-selftest đọc migration thật và chạy trong gate sw…
- `W-0203` — Residual: F-1 (HIGH) mở: 10 request /eligibility-checks đồng thời → 9 trả HTTP 500 vì PostgresIdempotencyStore.ExecuteAsync chạy SERIALIZABLE đọc-rồi-ghi ivr_idempotency_keys, 40001 bị ánh xạ thành IVR_INTERNAL_ERROR. Không vá …
- `W-0205` — không có TestId hoặc khai báo kiểm chứng để xét C2 · Residual: Một phát hiện phụ đáng giữ: ApiBehaviorMatrixTests đỏ trên clone sạch vì thiếu deploy/ci/node_modules — nó fail chứ không tự skip, đúng như evidence W-0197 tuyên bố. Nghĩa là freeze checkout phải chạy npm --prefix dep…
- `W-0216` — Gate đạt có điều kiện: DG-DR-03 PASS_SINGLE_HOST: RPO=0 và RTO trong ngân sách, nhưng trên một host hai container, không phải multi-AZ · Residual: Bài học ghi lại để lần sau không lặp: gate-status.yaml phải sinh bằng node scripts/gate-status.mjs --write, đừng gõ tay; Status §5 chỉ nhận một token trong 12 token, sắc thái để cột Residual; ký tự gạch đứng trong ô b…
- `W-0225` — Residual: Gate thứ hai tìm thấy vì nó nói dối: verify-api-behavior-matrix.mjs in "verdict": "FAIL" ngay trên "failures": [] với 38/38 pass, vì verdict quyết bởi mảng failures cấp cao còn console.log in mảng failures theo từng o…
- `W-0249` — Residual: Đề xuất m8-17 §5 của tôi sai và đã sửa trước khi dựng: nó viết "revoke mang version ≤ version đã lưu thì từ chối", nhưng order_version là chuỗi mờ IVR trả nguyên — OAS gọi là stale-result guard snapshot, IR-06 ghi "IV…
- `W-0270` — Residual: LOCAL_ONLY; không đóng external gates; real calls NO
- `W-0280` — Residual: LOCAL_ONLY / MOCK; còn một câu hỏi nghiệp vụ chưa đóng: tạm dừng hàng đợi có tính là IVR_CAPACITY_EXCEPTION không, hay IVR_CONFIRMATION_WINDOW_EXPIRED như runtime đang trả — hai nhãn dẫn tới hai hành động khác nhau ph…
- `W-0282` — Residual: LOCAL_ONLY / MOCK; REAL_CUSTOMER_CALL_ALLOWED=NO. Một khoảng trống phải chủ Module 8 quyết, không tự lấp: POST /eligibility-checks là thứ đưa task rời HELD_MOCK, nhưng worker không có vòng lặp nào gọi nó (10 hosted se…
- `W-0291` — Residual: Không thay API contract/runtime hoặc nới scanner; hosted là bước kế tiếp
- `W-0297` — Việc tài liệu, không có test phần mềm (3 tài liệu có tại commit): Chỉ dồn, xoá tài liệu kế hoạch trong plan/ivr-orther và viết lại link; commit 257cbef không chạm src/, tests/ hay script gate nên không có khẳng định phần mềm nào để test kiểm. · Residual: Phân loại sai một lần và đã sửa: m8-05…m8-10 mở đầu bằng M8_OWNER_SIGNED/đã ký nên lượt đọc đầu xếp vào nhóm xong — đọc hết chuỗi trạng thái thì cả sáu đều kèm M3_..._SIGNOFF_REQUIRED hoặc CODE_NOT_AUTHORIZED, tức mới…
- `W-0303` — Residual: Vault production không tự làm protector — lab là protector của chính nó vì bảo vệ của nó là vân tay tự sinh được; production nhận khóa Platform, nên deployment còn UnavailableOpaqueValueProtector fail closed ngay lần …
- `W-0335` — Residual: Bản sửa chưa deploy worker vận hành. Soak dùng15s/phần, max10.099s; recovery dùng30s/phần, max19.352s gồm busy. Chưa chốt budget cuối hoặc nghiệm thu scheduler/SIP S5; S2/mirror mở; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0347` — Residual: W-0012, W-0015, W-0031 đã ACCEPTED (A-0773..A-0775). Chờ Toàn quyết: W-0036 bảy ID §8 theo nhóm (OpenAPI parse/ref/enum, task schema + policy mismatch); W-0037 vế rate-limit và soak dài; REAL_CUSTOMER_CALL_ALLOWED=NO
- `W-0351` — Residual: W-0121, W-0169, W-0171 giữ lại; 18 việc trích test của gate Docker (image, K8s, oasdiff, security-scan): Toàn chọn “cái nào ko cần thì xóa đi, còn cái nào cần thì cho xong đi”, làm ở việc kế tiếp; REAL_CUSTOMER_CALL_A…

