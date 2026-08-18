# W-0035 — Evidence: Unit & integration test suite (`P5-1`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS`

## 1. Đây là slice hợp nhất, không phải slice viết mới

Phase 2–4 đã để lại 345 test có `TestId`, chạy trên Postgres thật qua Testcontainers, với clock tiêm vào. `P5-1` không yêu cầu viết lại chúng — nó yêu cầu biến chúng thành **một bộ test chính thức**: có fail-gate, có traceability, có ngưỡng coverage.

Ba thứ đó đều thiếu. Đối chiếu §6:

| Mục §6 | Hiện trạng trước slice này |
| --- | --- |
| 1–3. unit / integration / builder + clock tiêm | **đã có** |
| 4. coverage đạt ngưỡng + traceability table | thiếu **cả hai** |
| 5. 8 fail-gate assertion (`testing/08`) | **không tồn tại** — grep `IT-FAILGATE` = 0 hit |
| 6. tốc độ CI | đã có container reuse |

## 2. Tám fail gate — và vì sao chúng là structural

`specs/testing/08` liệt kê 8 điều kiện FAIL. Trước slice này chúng được phủ **rải rác**: PII ở một chỗ, `DT-02` ở chỗ khác, notification ở W-0033. Không có chỗ nào một reviewer đọc được cả 8 cùng lúc.

Giờ có `IT-FAILGATE-01..08`, mỗi gate một test.

**Phần lớn là structural, có chủ ý.** Một test runtime chứng minh gate giữ được **trên lối nó đi qua**; một test structural chứng minh **không có lối nào**. Với một acceptance gate — cái danh sách người ta đọc trước khi quyết định hệ thống này có được gọi khách thật hay không — loại thứ hai mới là thứ câu hỏi xứng đáng nhận.

| Gate | Cách khoá |
| --- | --- |
| 01 không transition order | Mọi giá trị `RecommendedCoreAction` trên wire đều bắt đầu bằng `CORE_` — **từ vựng không có cách diễn đạt** một transition do IVR làm; cộng quét symbol `SetOrderState`/`TransitionOrder`/… |
| 02 không xử lý payment | Quét symbol: không `PaymentClient`, `ChargeAsync`, `RefundAsync`, `AuthorizePayment`, `CapturePayment`, `PaymentGateway` |
| 03 không gửi notification | Cờ mặc định tắt + quét symbol (chi tiết ở `UT-NOTIF-*`) |
| 04 không gọi ngoài allowlist/kill switch | Mặc định an toàn là mặc định hạn chế: `RealCustomerCallAllowed=false`, kill switch **bật**, allowlist rỗng; bật gọi khách thật không phải mutation admin thường |
| 05 không lưu/lộ raw phone hay địa chỉ đầy đủ | `PiiGuard` + `ShortDeliveryArea`: một địa chỉ phố **không chứa chữ số nào** vẫn bị từ chối — đúng ca mà kiểm tra thuần pattern sẽ cho qua |
| 06 lỗi kỹ thuật ≠ no-answer | 5 disposition kỹ thuật → `IVR_TECHNICAL_EXCEPTION`, `IsCounted=false`, `IsNoAnswer=false` |
| 07 không hard-code candidate thành production truth | `CandidateMockLabOnly` là **giá trị enum**, không phải comment; `Mode=Real` và provider `TARGET_V1` đều **từ chối boot** |
| 08 không tuyên bố readiness khi gate chưa có evidence | Key source chết → từ chối, không "vẫn ổn vì phút trước ổn" |

Gate 01 là chỗ đáng nói nhất. Bản đầu tôi assert tên enum C# bắt đầu bằng `Core` — **sai**, tên C# là `RevalidateAndConfirmOrder`. Test đỏ ngay. Bất biến thật nằm ở **giá trị trên wire**: `CORE_REVALIDATE_AND_CONFIRM_ORDER`. Đó mới là điều đáng khoá, vì nó nói rằng cái IVR gửi đi luôn được đặt tên theo **việc Core nên làm**, không bao giờ theo việc IVR đã làm.

## 3. Traceability sinh ra, không viết tay

`docs/traceability-tests.md` liệt kê **234 test** có tag, nhóm theo prefix, kèm method và file.

Sinh bằng `deploy/ci/scripts/generate-test-traceability.mjs`. Viết tay thì nó trôi ngay lần đầu ai đó đổi tên một test — và **một bảng traceability đã trôi còn tệ hơn không có bảng**: nó đọc như một mức phủ không còn tồn tại.

Sinh ra mới là nửa câu trả lời. `UT-TRACE-01` khoá nửa còn lại: mọi `TestId` trong suite phải có **một dòng bảng** tương ứng. Assertion cố ý khớp dòng `| \`ID\` |` chứ không khớp "có nhắc tới ID" — chính header của file generator có nhắc một ID trong văn xuôi, và chấp nhận nhắc-suông sẽ để đúng ID đó lọt qua mà không có dòng nào đứng sau.

`npm run test:traceability` đã nối vào job `validate` của CI.

## 4. Coverage: đo trước, nâng ngưỡng sau

`P5-1` §4 yêu cầu ≥ 80%, nâng từ nền 60% của P0-2.

Đo thật trước: **`TOTAL_LINE_COVERAGE=87.96%`** (13.760/15.643 dòng, 3 report). Rồi mới nâng cổng CI 60 → **80**.

Thứ tự đó quan trọng. Nâng ngưỡng trước khi đo là đặt cược CI sẽ xanh; nếu độ phủ thật là 74% thì cổng đỏ và người tiếp theo sẽ hạ nó xuống thay vì viết test. Ngưỡng 80 để lại biên ~8 điểm — đủ cho một slice refactor mà không phải hạ cổng.

Không thêm exclude nào. `coverage.runsettings` giữ nguyên đúng những loại trừ đã có từ P0-2 (migration, `*.g.cs`, `obj/`).

## 5. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **354/354** (22 contract + 204 unit + 128 integration), +9 |
| coverage (cobertura, 3 report) | **87.96%** — trên ngưỡng mới 80 |
| `npm run test:traceability` | `TEST_TRACEABILITY_CURRENT=234` |
| `ci-config-selftest.mjs` | `CI_CONFIG_SELFTEST_PASS` sau khi thêm bước mới |
| `dotnet build -warnaserror` | 0 warning / 0 error |

Integration vẫn chạy Postgres thật qua Testcontainers — không in-memory thay thế, đúng §11.

## 6. Cái này KHÔNG chứng minh

- **Không phải bằng chứng production.** Toàn bộ chạy `MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- **Coverage không phải chất lượng.** 87.96% nói dòng nào được chạy qua, không nói assertion có đúng không. Giá trị thật nằm ở fail-gate và ở việc không test cũ nào bị nới.
- **Traceability là test↔source, chưa phải test↔spec đầy đủ.** Bảng map `TestId` sang file và method; ánh xạ ngược tới từng mục `specs/testing/02`/`03` vẫn cần một vòng đọc spec — đó là việc `P5-2`/`P5-4` sẽ chạm.
- **`TESTS_PASS` là trần.** Chỉ reviewer/owner chuyển `ACCEPTED`.
