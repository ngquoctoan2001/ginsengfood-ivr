# W-0197 — P1.1 Ma trận hành vi HTTP API

Ngày: 2026-09-05. Trạng thái: **IN_PROGRESS — chờ lượt xác minh cuối**.

## Phạm vi và evidence

- Nguồn operation: OpenAPI Target V1 `1.0.0-draft.23`; không dùng danh sách route viết tay làm authority.
- Host: `WebApplicationFactory<Program>` chạy composition root thật của `Ivr.Api`, không thay DI; PostgreSQL 16 Testcontainers, migrate/reset riêng từng operation.
- Chỉ dữ liệu tổng hợp; `MOCK/MOCK/NO`, không worker, không vendor/customer call.
- Báo cáo máy đọc được: [`api-matrix.json`](api-matrix.json), chứa từng case HTTP, response, status/correlation/schema/PII assertion và N/A có lý do.
- Evidence gắn `base_commit` và SHA-256 từng file/source tổng trong JSON; đây là **working-tree candidate**, không phải bằng chứng clean exact-SHA hay hosted CI.
- Báo cáo tuần là WIP ngoài scope, đang có thay đổi/đổi tên đồng thời; lượt này không sửa/stage các path đó và không đưa chúng vào fingerprint source.

## Ma trận kiểm tra

- Mỗi operation: happy path, malformed body/query khi contract có khai báo, auth thiếu/sai credential/sai tier, sai scope, not-found/conflict và correlation ID thiếu/sai/hợp lệ.
- Mỗi mutation: retry cùng key, replay payload đảo thứ tự JSON property, đổi payload phải `409/IVR_IDEMPOTENCY_CONFLICT`; so response và business state PostgreSQL trước/sau replay.
- Happy path có fixture tạo tác động thật: intake tạo job MOCK, seed 9 task/8 accepted, profile bật SIM đang tắt, queue resume từ trạng thái pause, script đủ lifecycle, analytics đủ k-anonymity.
- GET không có body/typed query hoặc lookup không bị gán một nhánh 404/409 giả; lý do N/A được ghi riêng, không tính là request đã chạy.
- `dryRunDevScenario` là POST đọc/simulation, không mutation/idempotency contract; lặp được, bỏ timestamp/correlation khi so, payload-conflict là N/A.
- Read-only không có tier thấp hơn Read: thử credential không có admin tier; Write/Danger có kiểm tra token thấp quyền hơn. Scope nội bộ và admin được kiểm tra theo guard thực tế.
- 11 wire code: 9 runtime code qua HTTP từ fixture PostgreSQL đã qua invariant domain; `IVR_OPERATIONAL_BLOCKED` và `IVR_POLICY_BLOCKED` bị từ chối tạo `CallResultSnapshot`. Pre-call restriction trả 409, policy thiếu trả 200/held, không sinh job/attempt; contact-invalid là error 422 riêng, không phải result code thứ 12. Đây không phải evidence normalize/worker E2E.
- Mọi response được validate schema OpenAPI và allowlist object đệ quy; field nhạy cảm/raw phone/address/payment/recording bị từ chối. Ref, giá trị đã mask và boolean nghiệp vụ đã khai báo vẫn hợp lệ.

## Sửa lỗi phát hiện trong ma trận

- Giữ `X-Correlation-Id` sau khi clear error response; malformed request vẫn trả envelope có correlation nhất quán.
- Feature flag: chặn body thiếu changes/reason, key/correlation sai trước mutation; mô tả rõ correlation được tự sinh khi bỏ header.
- Terminate one/all: validate body trước lookup/mutation, tránh null reason thành 500.
- Script: duplicate immutable version trả 409; thêm replay bền vững khi cung cấp key, giữ tương thích caller cũ không gửi key.
- Seed/profile: thêm replay response có ràng buộc operation, typed payload, actor/permissions; không thay đổi endpoint hay quyền truy cập.
- Profile bật SIM tái hiện 500 với SQLSTATE `40001`: transaction replay Serializable xung đột với command receipt commit độc lập. Tách đường coordinated replay ReadCommitted + advisory lock theo key; caller Serializable cũ giữ nguyên.
- Chỉ thu SQLSTATE nội bộ trong test, không exception message/SQL/parameter/connection string. Test duplicate version chủ động sinh `23505` và được map về 409.
- OpenAPI sửa nullable enum và các response/header hiện hữu; codegen không thay DTO/client, contract vẫn TARGET_DRAFT.

## Tái lập

Yêu cầu: SDK theo `global.json`, Node/npm, Docker đang chạy; không cần secret ngoài repo.

```powershell
npm --prefix deploy/ci ci --no-audit --no-fund
dotnet restore Ivr.sln --locked-mode
dotnet test tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj --filter TestId=IT-API-MATRIX-38
node deploy/ci/scripts/api-behavior-matrix-selftest.mjs
node deploy/ci/scripts/verify-api-behavior-matrix.mjs .artifacts/api-matrix/http-observations.json docs/evidence/W-0197/api-matrix.json
```

Verifier nằm ngay trong integration test: thiếu Node/dependency, thiếu operation/case, response ngoài schema/allowlist hoặc source drift đều làm test FAIL. CI dotnet job đã tích hợp và xuất JSON cho PII scan.

## Giới hạn

HTTP retry/replay sau response thành công không chứng minh atomicity khi process crash giữa business commit và receipt commit. Crash/lease/duplicate-delivery E2E vẫn thuộc P1.2; không dùng evidence này để đóng mục đó.

Hosted CI, lab SIM/trunking, staging và production: **NOT_RUN**. Chưa có cuộc gọi thật và không cần mua gói trunking cho bước này.
