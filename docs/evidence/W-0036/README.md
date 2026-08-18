# W-0036 — Evidence: Contract & E2E test suite (`P5-2`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS`

## 1. Dùng bảng traceability để hỏi, không để khoe

`W-0035` vừa sinh `docs/traceability-tests.md`. Việc đầu tiên của slice này là dùng nó đúng mục đích: đối chiếu 16 test ID mà `P5-2` §8 yêu cầu với những gì thật sự tồn tại — **theo assertion, không theo tên**.

Kết quả đối chiếu:

| §8 yêu cầu | Đã có chưa | Ở đâu |
| --- | --- | --- |
| `CT-OAS-01..03` OpenAPI parse/ref/enum | **có** | `CT-API-OAS-10`, `validate-openapi.mjs` (9 schema hợp lệ + 12 negative bị từ chối) |
| `CT-TASK-01..04` task schema + policy mismatch | **có** | `CT-INTAKE-OPENAPI-01`, `CT-INTAKE-FIXTURES-02`, `IT-INTAKE-*` |
| `CT-CB-01` `200 ACCEPTED` → `DELIVERED_ACCEPTED` | **có** | `UT-CALLBACK-STATE-08` |
| `CT-CB-02` `200 DUPLICATE_ACCEPTED` → `DELIVERED_ACCEPTED`, không bản ghi trùng | **THIẾU nửa sau** | xem §2 |
| `CT-CB-03..07` blocked/review/stale/conflict/invalid | **có** | `UT-CALLBACK-STATE-08` |
| `CT-CB-08` 429/500/503/timeout → retry bounded → exhausted | **có** | `UT-CALLBACK-STATE-08`, `UT-CALLBACK-TIMEOUT-03`, `UT-CALLBACK-RETRY-IDENTITY-02`, `UT-CALLBACK-RETRY-EXHAUSTED-09` |
| `CT-CB-09` GH compat 200/422 + 24/7 bị từ chối | **có** | `UT-CALLBACK-GH-COMPAT-06`, `UT-CALLBACK-GH-ISOLATION-07`, `CT-CONTRACT-CURRENT-06` |
| `E2E-CONFIRM-01` / `E2E-NOANSWER-02` / `E2E-RACE-03` | **THIẾU 2/3** | xem §3 |
| §7 `deploy/ci/contract-e2e.gitlab-ci.yml` root-included | **KHÔNG TỒN TẠI** | xem §4 |

**Tôi không đổi tên test cũ sang ID mới của §8.** Đổi tên một test đã khoá đúng bất biến không thêm một chút bảo đảm nào; nó chỉ làm bảng trông đầy hơn. Chỗ nào assertion đã tồn tại thì bảng trên là câu trả lời, và bốn chỗ thiếu thật là nội dung slice này.

## 2. Lỗ hổng thật thứ nhất: `DUPLICATE_ACCEPTED` không có trạng thái đích

`UT-CALLBACK-TARGET-ACK-01` phân loại được `DUPLICATE_ACCEPTED` ở tầng transport. Nhưng `UT-CALLBACK-STATE-08` — theory ánh xạ outcome sang trạng thái admin nhìn thấy — liệt kê **8 outcome và bỏ sót đúng cái này**.

Hậu quả nếu code sai mà không ai biết: Sales nói "tôi đã có kết quả này rồi" và IVR có thể lặng lẽ đẩy nó vào hàng đợi review, biến mỗi lần gửi lại thành một việc operator phải xử lý tay.

Code vốn đã đúng — chỉ là **không có assertion nào đứng sau**. Giờ theory có 9 dòng, và dòng mới khẳng định `DUPLICATE_ACCEPTED` về đúng `DELIVERED_ACCEPTED` với `RequiresReview = false`.

## 3. Lỗ hổng thật thứ hai: hai luồng nghiệp vụ đầu-cuối

`E2E-RACE-03` đã có từ `W-0030` (`IT-ELIG-RACE-12`). Hai luồng còn lại thì không.

| Test | Khẳng định |
| --- | --- |
| `E2E-FLOW-CONFIRM-01` | Bấm phím → normalize → outbox → ACK Sales → dòng operator xem được. Mỗi nửa đã có unit test; cái này khẳng định **các nửa còn nối được với nhau**. Kèm: confirm được chấp nhận **không** tạo review item — nếu có, mọi cuộc gọi thành công đều rơi vào hàng đợi operator |
| `E2E-FLOW-NOANSWER-02` | `DS-02`/`D-02`: sau khi IVR bỏ cuộc, đơn vẫn **đúng như Sales đặt** — cùng `order_state`, cùng `order_version`. "Chúng tôi không liên hệ được" không được phép già đi thành "đơn đã đổi" |

Cả hai chạy trên Postgres thật.

**Không có E2E cấp trình duyệt.** Pane preview trong môi trường này không composite frame (đã ghi từ `W-0097`/`W-0102`), nên Playwright không chạy được. Bộ e2e của console vẫn drive HTTP thật vào một server `next start` thật — đó là thứ thay thế trung thực, và tôi không gọi nó là Playwright.

## 4. Lỗ hổng thật thứ ba: job CI không tồn tại

`P5-2` §7 nói rõ: fragment **phải** được root `.gitlab-ci.yml` include, `allow_failure: false`, **và có test chứng minh job xuất hiện trong rendered pipeline**.

Root chỉ include 3 fragment; `contract-e2e.gitlab-ci.yml` không tồn tại.

Giờ có, với hai job `contract_suite` và `e2e_flow_suite`, cả hai `allow_failure: false` và ghim `IVR_ADAPTER_MODE=MOCK` + `REAL_CUSTOMER_CALL_ALLOWED=NO` trong `variables`.

Phần "có test chứng minh" mở rộng `assertCiTopology()`: nó kiểm include, kiểm cả hai job tồn tại, fail-closed, và **ghim đúng hai biến governance**.

**Đã kiểm chứng theo chiều âm.** Tôi gỡ dòng include ra, chạy lại `docs-selftest.mjs` — nó **đỏ**; khôi phục, nó xanh. Một cổng chưa từng thấy đỏ thì chưa biết nó có bắt được gì không.

## 5. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **357/357** (22 contract + 205 unit + 130 integration), +3 |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS`, `API_DOCS_SELFTEST_PASS` |
| bỏ include → `docs-selftest.mjs` | **đỏ** (kiểm chứng âm) |
| `test:traceability` | `TEST_TRACEABILITY_WRITTEN=236` |
| `dotnet test --filter "TestId~E2E-FLOW"` | 2/2 — đúng filter mà job `e2e_flow_suite` dùng |
| `dotnet build -warnaserror` | 0 warning / 0 error |

Filter của job CI được chạy thật ở đây chứ không chỉ viết vào YAML — một filter sai chính tả sẽ cho job xanh mà không chạy test nào.

## 6. Cái này KHÔNG chứng minh

- **Không có bằng chứng provider thật.** Toàn bộ contract chạy trên fake/WireMock; real Sales vẫn `BLOCKED_EXTERNAL` (`W-0002`…`W-0006`). Không case nào bị giả pass — chỗ nào cần provider thật thì không tồn tại chứ không "pending mà xanh".
- **Không có Pact.** §6.1 nhắc pact consumer/provider; slice này dùng schema round-trip + WireMock + hash ghim thay vì thêm một framework nữa. Pact chỉ có giá trị khi **cả hai bên** cùng chạy nó — mà Sales chưa có gì để chạy. Ghi lại như một khác biệt có chủ ý so với prompt, không phải một mục đã xong.
- **Không có E2E trình duyệt** (§3 ở trên).
- **`TESTS_PASS` là trần.** Chỉ reviewer/owner chuyển `ACCEPTED`.
