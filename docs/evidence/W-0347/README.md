# W-0347 — Nhóm thiếu test của 5 việc trong kế hoạch

Ngày 23/09/2026 · Claude · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Đã nộp bằng chứng. Collector tại commit ứng viên `ca4f442`: 1200/1200 test .NET, `GATE_SWEEP_PASS 43/43`. W-0012, W-0015 và W-0031 sang XEM; W-0036 và W-0037 còn chờ Toàn quyết.**

## Vì sao có việc này

Sau [W-0346](../W-0346/README.md), năm việc trong kế hoạch vẫn trượt C2. Lý do: một TestId mà prompt
hoặc README của chúng trích **không còn test nào mang tên đó**. Toàn yêu cầu làm tiếp nhóm này. Với
từng ID, W-0347 tìm lại lịch sử test trong git rồi xử lý theo nguyên nhân thật.

## Kết quả theo việc

| Việc | Nguyên nhân | Xử lý | Hồ sơ |
| --- | --- | --- | --- |
| W-0015 | Test lease kênh bị xoá ở `fb1eb4c`, khi việc lease chuyển vào scheduler. Hai trong ba tính chất đã có test mới; "release bằng lease cũ bị từ chối" thì không | **Viết lại** `IT-DB-LEASE-05` trên code hiện hành, chạy Postgres thật. Tạm bỏ phép so token thì test đỏ | [README W-0015](../W-0015/README.md), mục cuối |
| W-0031 | Ba test trust-skip bị xoá ở `6760ba6`, theo quyết định OD-18 (M3 quyết định gọi) | **Retire có ghim quyết định**, cùng pin W-0324 đã dùng cho W-0019 (Toàn đã nghiệm thu). Test thay thế là nhóm M3-authority đang chạy | [đối chiếu](../W-0031/acceptance-addendum.md) |
| W-0012 | Hai test RBAC bị xoá ở `3e36b46`, khi W-0128 bỏ tài khoản console theo lệnh owner (changelog `a09f062`) | **Retire có ghim quyết định**. Thay bằng `IT-API-AUTHZ-01` (tầng đủ 200, tầng thiếu 403 `IVR_FORBIDDEN_CALLER`, không credential 401), `IT-API-AUTHZ-02` và `IT-DEV-AUTHZ-08` (header tự khai quyền bị 401) | [đối chiếu](../W-0012/acceptance-addendum.md) |
| W-0036 | 19 ID §8 đã được kiểm dưới tên khác. Mục 1 của README W-0036 có bảng đối chiếu, nhưng C2 không đọc được bảng đó | **Gắn thêm tag** §8 vào đúng test đang kiểm, không đổi tên test nào. Viết **ba test mới** cho các vế chưa có assertion: `CT-CB-02` gửi lại không tạo bản ghi trùng, `CT-CB-09` compat 422, và `E2E-NOANSWER-02` hai lượt A1 → A2 → final. Bảy ID theo nhóm chờ Toàn | [bảng gắn tag](../W-0036/acceptance-addendum.md) |
| W-0037 | Vế rate-limit của một ID, và soak dài của một ID, chưa có trong IVR. Hai điều này cần quyết định vận hành. ID thứ ba đã được kiểm dưới tên khác | **Chưa sửa**, chờ Toàn quyết. ID thứ ba sẽ được gắn tag cùng lượt đó | — |

Nguyên tắc chung:
- Chỉ retire khi có văn bản quyết định ghim được.
- Chỉ gắn tag khi các test cộng lại kiểm đủ mọi vế của định nghĩa.
- Không viết test cho hành vi đã bị bỏ theo quyết định.

## Kiểm chứng

- Test mới hoặc đổi: `IT-DB-LEASE-05`, `CT-CB-02`, `CT-CB-09`, `E2E-NOANSWER-02`, và `E2E-FLOW-CONFIRM-01`
  (kiểm thêm job đã đóng). Hai phép thử đột biến làm test đỏ đúng chỗ.
- Lớp `ResultNormalizationPersistenceTests` 13/13, `CallbackDeliveryTests` 49/49,
  `SchedulerPersistenceTests` đạt.
- Helper seed thêm tham số tuỳ chọn, mặc định giữ nguyên. `SeedPendingAsync` có impact MEDIUM: 10 caller
  đều trong cùng lớp, 0 execution flow.
- Traceability 777 dòng. Mọi dòng mới đều ghi đúng method.
- `CT-ACCEPTANCE-C2-01` xác nhận các khai báo retire: commit nằm trong lịch sử, hash và trích đoạn khớp,
  ID thay thế đang chạy.
- **Collector tại `ca4f442`**, trên cây sạch: 1200/1200 test .NET (Contract 24, Unit 782, Chaos 8, Integration 386), tức đúng
  4 test mới, rồi `GATE_SWEEP_PASS 43/43`. Cây sạch trước và sau cả hai lệnh. So với danh sách ở `6d0aa77`:
  W-0012, W-0015, W-0031 từ KHÔNG ĐẠT sang XEM, W-0346 vào danh sách ở XEM, không mục nào khác đổi.
  Trong kế hoạch nay có 15 mục XEM. Thời gian UTC: test `07:30:13` → `07:36:12`, sweep `07:36:13` →
  `07:41:54`. Bundle giữ trên máy: `.artifacts/acceptance-ca4f442/acceptance-run.json`, SHA-256 theo nhóm
  tám ký tự `e007a729-d9238058-c5a5ba2d-48ff73ee-dc320b8f-48d37598-33333a1b-0b7ce1be`.

## Còn chờ Toàn

- **W-0036:** bảy ID §8 theo nhóm (OpenAPI parse/ref/enum; task schema + policy mismatch). Prompt
  không định nghĩa từng số, nên gán test nào cho số nào là diễn giải yêu cầu gốc.
- **W-0037:** vế rate-limit (có làm trong IVR không, ngưỡng bao nhiêu) và soak dài (chạy ở đâu, bao
  lâu, hay dời sang staging).

Chỉ Toàn chuyển các việc này sang `ACCEPTED`.
