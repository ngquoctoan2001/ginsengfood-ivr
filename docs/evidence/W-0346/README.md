# W-0346 — C2 nhận assertion của gate trong full sweep

Ngày 23/09/2026 · Claude · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Commit ứng viên. Kết quả collector tại commit này sẽ được ghi ở commit tài liệu sau đó.**

## Vì sao có việc này

Danh sách nghiệm thu làm mới ở `414e84b` còn 21 mục trong kế hoạch ở KHÔNG ĐẠT. Phần lớn trong số
đó trượt C2 với lý do "TestId không có trong `docs/traceability-tests.md`". File đó sinh tự động từ
tag của test .NET. Ngoài nó, C2 chỉ nhận TestId đăng ký trong `GATE_TESTS`, mà registry mới có
`CT-CI-06*` và hai gate nghiệm thu.

Trong khi đó, nhiều TestId bị thiếu đã có phép kiểm thật, nằm trong một gate mà full sweep vẫn chạy
mỗi lần. Gate đó in `<TestId> PASS` ngay sau assertion cùng tên. Ví dụ `ci-config-selftest.mjs` in
`CT-CI-05 PASS`, `CT-CI-07 PASS`, `CT-CI-08 PASS`. Đây là nhóm "có TestId chưa đối chiếu được" mà
[W-0325](../W-0325/README.md) đã tách ra và giao làm tiếp.

## Thay đổi

- [`acceptance-test-plan.mjs`](../../../deploy/ci/scripts/acceptance-test-plan.mjs): `GATE_TESTS` tăng
  từ 10 lên 54 TestId, thêm 44 TestId của 11 runner. Chỉ đăng ký TestId mà một ứng viên thật sự trích.
- Luật registry, `gateRegistryErrors()`. Một mục chỉ đứng được khi:
  1. runner có `argv` trong `deploy/ci/gate-invocations.json`, tức là full sweep chạy nó;
  2. TestId không thuộc test .NET nào (nếu không, registry sẽ che kết quả .NET);
  3. runner in `<TestId> PASS` ở một dòng không phải comment;
  4. PASS có hậu tố (`PASS_…`) là PASS có điều kiện: phải khai báo trong `GATE_TEST_CONDITIONS`, và
     một khai báo cần có PASS như vậy.

  Hai gate nghiệm thu nằm trong `WHOLE_GATE_TESTS`: cả gate là phép kiểm nên chỉ cần chạy trong sweep.
- Luật này chạy trong `acceptance-batches.mjs --self-test`, tức gate `CT-ACCEPTANCE-C2-01` của full
  sweep. Registry sai thì sweep đỏ. C2 chỉ tính TestId của gate khi có full sweep đã xác minh tại
  đúng commit đó.
- PASS có điều kiện vẫn tính cho C2. Nhưng mục đó thành **XEM** kể cả khi Residual trống, và danh sách
  in điều kiện ngay cạnh mục, trước Residual. Năm TestId có điều kiện: `CAP-CALIB-03`
  (`PASS_UNCALIBRATED`), `CAP-ALERT-04` (`PASS_WITH_NOT_PROVEN=COST_METRIC`), `CAP-DRIFT-05`
  (`PASS_DECLARED_DISAGREEMENT`), `CAP-SESSION-06` (`PASS_UNANSWERED`), `DG-DR-03` (`PASS_SINGLE_HOST`).
- Không đổi: C1, C3, C4, manifest gate, các gate, collector, test .NET.

Impact trước khi sửa: `GATE_TESTS`, `judge`, `render`, `c2SelfTest` đều LOW, 0 execution flow.

## Gate kiểm gì so với định nghĩa

C2 xanh nghĩa là **phép kiểm mang tên đó trong repo đã chạy và đạt**. Nó không có nghĩa định nghĩa
trong prompt đã được chứng minh trọn vẹn. [gate-registry.json](gate-registry.json) ghi cho từng TestId
dòng PASS trong runner, nguồn định nghĩa và mức khớp:

| Mức | Số TestId | TestId |
| --- | ---: | --- |
| Khớp định nghĩa | 32 | `CT-CI-01/02/03/05/07/08/09/10/11/12`, `CT-GATE-01..04`, `CT-DOC-01`, `UT-DOC-PII-03`, `IT-CD-GATE-02`, `IT-CD-REAL-03`, `IT-CD-CONCURRENCY-05`, `DG-BACKUP-02`, `CAP-MODEL-01`, `CAP-SENS-02`, `CAP-INTAKE-*` (8), `CT-OBS-STAGING-13/14` |
| Kiểm tĩnh cấu hình thay cho lần chạy thật | 6 | `IT-CD-DEV-01`, `IT-CD-ROLLBACK-04`, `IT-CANARY-01`, `IT-BG-WORKER-02`, `IT-MIGRATE-03`, `IT-FLAG-RAMP-04` |
| Một phần của định nghĩa | 1 | `DG-CRYPTO-01`: chỉ in-transit TLS; at-rest và KMS chưa có |
| PASS có điều kiện | 5 | năm TestId ở trên |

Phần thiếu của nhóm tĩnh và nhóm một phần đã nằm trong Residual của chính mục đó. W-0045 ghi chưa
pipeline deploy nào chạy. W-0046 ghi canary, auto-rollback và blue-green đều NOT_RUN. W-0053 ghi
không multi-AZ, không mã hoá volume at-rest, không KMS. Toàn đọc Residual khi xét XEM.

**Không đăng ký:**
- `DG-RETENTION-04`: đã là test .NET có tag.
- `IT-IMG-BUILD-01`: `ci-config-selftest.mjs` chỉ nhắc tên nó trong comment.
- `IT-IMG-*`, `IT-K8S-*`, `IT-OBS-TRACE-02`, `CT-DOC-02`, `CT-CI-04`: gate của chúng không chạy trong
  full sweep, vì cần docker build, cluster, binary oasdiff hoặc scanner có mạng.

## Kiểm chứng

- `acceptance-batches.mjs --self-test`: **PASS**, 58 kiểm tra C2 (thêm 13 so với 45), 54 TestId gate
  khớp runner thật.
- `acceptance-evidence-selftest.mjs`: **PASS**, 47 kiểm tra.
- Ca âm của luật registry, chạy trên runner tổng hợp: runner ngoài sweep, TestId của .NET, tên chỉ
  có trong comment, ID dài hơn (`UT-Z-04b` không thay `UT-Z-04`), điều kiện chưa khai báo, khai báo thừa,
  khai báo cho ID không có trong registry. Cả 7 ca đều bị từ chối.
- Ca âm chạy trên runner thật: `DG-RETENTION-04`, `IT-IMG-BUILD-01`, `IT-K8S-LINT-01`, `CT-DOC-02`,
  `CT-CI-04` và `CAP-DRIFT-05` khi chưa khai báo điều kiện. Cả 6 đều bị từ chối, lý do ghi trong JSON.
- **Trước/sau trên cùng dữ liệu, cùng bộ kết quả.** Tool mới chạy `--root` trên một bản clone chỉ đọc tại
  `2a002e3`, dùng bundle `.artifacts/acceptance-2a002e3` (1196 kết quả `.trx`, `GATE_SWEEP_PASS 43/43`).
  Đó chính là dữ liệu và bundle đã sinh danh sách ở `414e84b`. Kết quả: đúng **27 mục** đổi từ KHÔNG ĐẠT
  sang XEM, không mục nào đổi theo chiều khác, không mục nào thành ĐẠT. XEM từ 56 lên 83, KHÔNG ĐẠT từ
  162 xuống 135.
  - Trong kế hoạch có 8 mục: W-0013, W-0014 (`CT-CI-05/07/08`), W-0038 (`CT-GATE-01..04`),
    W-0041 (`CAP-ALERT-04`), W-0045 (`IT-CD-*`), W-0046 (`IT-CANARY-01`, `IT-BG-WORKER-02`,
    `IT-MIGRATE-03`, `IT-FLAG-RAMP-04`), W-0053 (`DG-*`), W-0054 (`CAP-*`).
  - Ngoài kế hoạch có 19 mục: W-0095, W-0098, W-0114, W-0132, W-0134, W-0155…W-0159, W-0212, W-0223,
    W-0275…W-0279, W-0305, W-0310.

## Còn lại trong kế hoạch

13 mục trong kế hoạch vẫn KHÔNG ĐẠT. Không mục nào còn trượt vì một gate trong sweep đã kiểm mà chưa
đăng ký:

| Nguyên nhân | Mục |
| --- | --- |
| Không test hay gate nào mang TestId đó: chưa viết, hoặc đã đổi tên mà chưa có quyết định retire | W-0012 (`UT-FND-RBAC-03/08`), W-0015 (`IT-DB-LEASE-05`), W-0031 (`IT-ELIG-TRUST-14`, `UT-ELIG-TRUST-16/17`), W-0036 (19 TestId contract/E2E), W-0037 (`PT-SOAK-02`, `SEC-AUTHZ-05`, `SEC-ERR-06`) |
| Gate ngoài full sweep | W-0011 (`CT-CI-04`), W-0040 (`IT-OBS-TRACE-02`), W-0043 (`IT-IMG-*`), W-0044, W-0047 (`IT-K8S-*`), W-0055 (`CT-DOC-02`) |
| UI đã ra khỏi phạm vi | W-0039 (`UI-*`): cần quyết định retire như bốn việc P3 ở W-0324 |
| Chưa khai báo phép kiểm | W-0016 |

Chỉ Toàn chuyển một mục sang `ACCEPTED`, sau khi đọc bằng chứng và Residual.
