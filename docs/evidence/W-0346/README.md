# W-0346 — C2 nhận assertion của gate trong full sweep

Ngày 23/09/2026 · Claude · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Đã nộp bằng chứng. Collector tại commit ứng viên `9c4f7c3`: 1196/1196 test .NET, `GATE_SWEEP_PASS 43/43`.
27 mục đổi từ KHÔNG ĐẠT sang XEM, trong đó 8 mục trong kế hoạch. Chờ Toàn đọc.**

## Vì sao có việc này

Danh sách nghiệm thu làm mới ở `414e84b` còn 21 mục trong kế hoạch ở KHÔNG ĐẠT. Phần lớn trượt C2 với
lý do "TestId không có trong `docs/traceability-tests.md`". File đó sinh tự động từ tag của test .NET.
Ngoài nó, C2 chỉ nhận TestId đăng ký trong `GATE_TESTS`, mà registry mới có `CT-CI-06*` và hai gate
nghiệm thu.

Trong khi đó, nhiều TestId bị thiếu đã có phép kiểm thật, nằm trong một gate mà full sweep vẫn chạy
mỗi lần. Gate đó in `<TestId> PASS` ngay sau assertion cùng tên. Ví dụ `ci-config-selftest.mjs` in
`CT-CI-05 PASS`, `CT-CI-07 PASS`, `CT-CI-08 PASS`. Đây là nhóm "có TestId chưa đối chiếu được" mà
[W-0325](../W-0325/README.md) đã tách ra và giao làm tiếp.

## Thay đổi

- [`acceptance-test-plan.mjs`](../../../deploy/ci/scripts/acceptance-test-plan.mjs): `GATE_TESTS` tăng
  từ 10 lên 54 mục, thêm 44 TestId của 11 runner. Chỉ đăng ký TestId mà một ứng viên thật sự trích.
- Luật registry, `gateRegistryErrors()`. Một mục chỉ đứng được khi:
  1. runner có `argv` trong `deploy/ci/gate-invocations.json`, tức là full sweep chạy nó;
  2. TestId không thuộc test .NET nào, vì nếu thuộc thì registry sẽ che kết quả .NET;
  3. runner in `<TestId> PASS` ở một dòng không phải comment;
  4. PASS có hậu tố (`PASS_…`) là PASS có điều kiện: phải khai báo trong `GATE_TEST_CONDITIONS`, và
     một khai báo cần có PASS như vậy.

  `WHOLE_GATE_TESTS` giữ hai gate nghiệm thu mà cả gate là phép kiểm, nên chỉ cần chạy trong sweep.
- Luật này chạy trong `acceptance-batches.mjs --self-test`, tức gate `CT-ACCEPTANCE-C2-01` của full
  sweep. Registry sai thì sweep đỏ. C2 chỉ tính TestId của gate khi có full sweep đã xác minh tại
  đúng commit đó.
- PASS có điều kiện vẫn tính cho C2. Nhưng mục đó thành **XEM** kể cả khi Residual trống, và danh sách
  in điều kiện ngay cạnh mục, trước Residual. Năm TestId thuộc loại này: bốn của `capacity-selftest.mjs`
  và một của `dr-selftest.mjs`. Hậu tố của chúng là `PASS_UNCALIBRATED`, `PASS_WITH_NOT_PROVEN=COST_METRIC`,
  `PASS_DECLARED_DISAGREEMENT`, `PASS_UNANSWERED` và `PASS_SINGLE_HOST`. Tên và nghĩa từng điều kiện
  có trong `GATE_TEST_CONDITIONS` và [gate-registry.json](gate-registry.json).
- Không đổi: C1, C3, C4, manifest gate, các gate, collector, test .NET.

Impact trước khi sửa: `GATE_TESTS`, `judge`, `render`, `c2SelfTest` đều LOW, 0 execution flow.

## Gate kiểm gì so với định nghĩa

C2 xanh nghĩa là **phép kiểm mang tên đó trong repo đã chạy và đạt**. Nó không có nghĩa định nghĩa
trong prompt đã được chứng minh trọn vẹn. [gate-registry.json](gate-registry.json) ghi cho từng TestId
dòng PASS trong runner, nguồn định nghĩa, mức khớp và những ứng viên trích nó.

| Mức | Số TestId | Runner |
| --- | ---: | --- |
| Khớp định nghĩa | 32 | `selftest-openapi`, `selftest-dotnet-policy`, `ci-config-selftest`, `review-gate-selftest`, `docs-selftest`, `cd-selftest` (3), `dr-selftest` (1), `capacity-selftest` (2), `capacity-data-intake-validator` (8), `observability-staging-evidence` (2) |
| Kiểm tĩnh cấu hình thay cho lần chạy thật | 6 | `cd-selftest` (2), `progressive-selftest` (4) |
| Một phần của định nghĩa | 1 | `dr-selftest`: chỉ TLS in-transit; at-rest và KMS chưa có |
| PASS có điều kiện | 5 | `capacity-selftest` (4), `dr-selftest` (1) |

Phần thiếu của nhóm tĩnh và nhóm một phần đã nằm trong Residual của chính mục đó. W-0045 ghi chưa
pipeline deploy nào chạy. W-0046 ghi canary, auto-rollback và blue-green đều NOT_RUN. W-0053 ghi
không multi-AZ, không mã hoá volume at-rest, không KMS. Toàn đọc Residual khi xét XEM.

**Không đăng ký**, tên cụ thể ở `not_registered` trong JSON:
- một TestId đã thuộc test .NET có tag;
- một TestId mà `ci-config-selftest.mjs` chỉ nhắc trong comment;
- TestId của `image-selftest`, `k8s-selftest`, `observability-runtime-selftest`, `selftest-oasdiff` và
  `security-scan`. Full sweep không chạy các gate đó, vì chúng cần docker build, cluster, binary
  oasdiff hoặc scanner có mạng.

## Kiểm chứng

- `CT-ACCEPTANCE-C2-01` (`acceptance-batches.mjs --self-test`): **PASS**. Có 58 kiểm tra C2, thêm 13
  so với 45. Cả 54 mục registry đều khớp runner thật.
- `CT-ACCEPTANCE-EVIDENCE-01` (`acceptance-evidence-selftest.mjs`): **PASS**, 47 kiểm tra.
- Ca âm của luật registry trên runner tổng hợp. Bảy ca đều bị từ chối:
  - runner ngoài sweep;
  - TestId của .NET;
  - tên chỉ có trong comment;
  - ID dài hơn cùng tiền tố (ID có thêm hậu tố chữ không thay được ID gốc);
  - điều kiện chưa khai báo;
  - khai báo thừa cho một PASS thường;
  - khai báo cho ID không có trong registry.
- Ca âm trên runner thật: sáu mục đều bị từ chối, lý do ở `negative_checks_against_real_runners`.
- **Trước/sau trên cùng dữ liệu, cùng bộ kết quả.** Tool mới chạy `--root` trên một bản clone chỉ đọc
  tại `2a002e3`, dùng bundle `.artifacts/acceptance-2a002e3` (1196 kết quả `.trx`,
  `GATE_SWEEP_PASS 43/43`). Đó chính là dữ liệu và bundle đã sinh danh sách ở `414e84b`. Kết quả: đúng
  **27 mục** đổi từ KHÔNG ĐẠT sang XEM, không mục nào đổi theo chiều khác, không mục nào thành ĐẠT.
  XEM từ 56 lên 83, KHÔNG ĐẠT từ 162 xuống 135.
  - Trong kế hoạch có 8 mục: W-0013, W-0014, W-0038, W-0041, W-0045, W-0046, W-0053, W-0054.
  - Ngoài kế hoạch có 19 mục: W-0095, W-0098, W-0114, W-0132, W-0134, W-0155…W-0159, W-0212, W-0223,
    W-0275…W-0279, W-0305, W-0310.
- **Collector tại commit ứng viên `9c4f7c3`**, trên cây sạch: 1196/1196 test .NET (Contract 24, Unit 781, Chaos 8, Integration 383), rồi `GATE_SWEEP_PASS 43/43`,
  trong đó `CT-ACCEPTANCE-C2-01` giữ 54 mục registry. Cây sạch trước và sau cả hai lệnh. Danh sách sinh lại
  trùng từng dòng với bản xem trước, chỉ khác SHA ở tiêu đề: XEM 83, KHÔNG ĐẠT 135, 7 mục có dòng
  "Gate đạt có điều kiện". Thời gian UTC: test `06:23:51` → `06:30:05`, sweep `06:30:06` → `06:34:36`.
  Bundle giữ trên máy: `.artifacts/acceptance-9c4f7c3/acceptance-run.json`, SHA-256 theo nhóm tám ký tự
  `4b5f8693-6a6ef2e9-f8bfe2dc-a78674da-d3c796a6-fed6d9cc-b0399dbe-791babea`.

## Còn lại trong kế hoạch

13 mục trong kế hoạch vẫn KHÔNG ĐẠT. Không mục nào còn trượt vì có gate trong sweep đã kiểm mà chưa
đăng ký. TestId cụ thể của từng mục có trong [danh sách nghiệm thu](../../release/acceptance-batches.md).

| Nguyên nhân | Mục |
| --- | --- |
| Không test hay gate nào mang TestId đó: chưa viết, hoặc đã đổi tên mà chưa có quyết định retire | W-0012, W-0015, W-0031, W-0036, W-0037 |
| Gate ngoài full sweep | W-0011, W-0040, W-0043, W-0044, W-0047, W-0055 |
| UI đã ra khỏi phạm vi, cần quyết định retire như bốn việc P3 ở W-0324 | W-0039 |
| Chưa khai báo phép kiểm | W-0016 |

Chỉ Toàn chuyển một mục sang `ACCEPTED`, sau khi đọc bằng chứng và Residual.

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0346 thuộc nhóm A6 và chuyển **EVIDENCE_SUBMITTED → ACCEPTED** cho phần đã làm: C2 nhận
assertion của gate. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: xong. Mọi giới hạn trong cột Residual của tracker giữ nguyên.
Bằng chứng giữ đúng lượt collector tại `ca4f442` (1200/1200 test, sweep 43/43), không coi commit tài
liệu là một lượt test/sweep mới. REAL_CUSTOMER_CALL_ALLOWED=NO.
