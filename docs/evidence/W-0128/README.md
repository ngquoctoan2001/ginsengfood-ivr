# W-0128 — Admin/auth transition remediation evidence

Ngày: `2026-08-28`  
Baseline triển khai: `main@b4d8903` sau hai commit concurrent `a09f062`/`b4d8903`  
Trạng thái: `TESTS_PASS_LOCAL / M3_AND_PRODUCTION_BLOCKED`  
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Phạm vi đã khắc phục

- Runtime/OpenAPI không còn account/session route hoặc console-owned RBAC. OpenAPI hiện là
  `1.0.0-draft.22`; generated DTO không đổi khi regenerate.
- Ba admin capability tier `read/write/danger` dùng credential riêng. Mỗi tier hỗ trợ current +
  previous với retirement instant tuyệt đối; previous hết hiệu lực tự động.
- Startup/runtime từ chối token ngắn, previous thiếu current/retirement và mọi giá trị trùng giữa
  tier/rotation slot. Token trùng không thể biến read capability thành danger capability.
- Contract parity test so 31 operation `tags: [admin]` với mọi runtime endpoint gắn
  `AdminPolicies.*`; route chỉ có trong OpenAPI hoặc chỉ có runtime sẽ làm test đỏ.
- Reference UI đã gửi `X-Action-Reason` trên đủ 8 danger operation. Header là input tường minh,
  không được suy ra từ một body bất kỳ trong helper HTTP chung.
- `admin-ui` được định nghĩa lại là reference local. Helm từ chối `ui.enabled=true`, không render
  UI Deployment/Service. API chỉ nhận ingress từ Module 3 BFF khi Platform cấu hình cả namespace
  selector và pod selector; default ngoài dev selftest là `ingress: []`.
- Helm API pod nhận ba current token từ Kubernetes Secret và có optional previous+retirement cho
  từng tier. Không token nào được inject vào browser/UI pod.
- SRS/API/UI/DB/retention/IR-06/README/portal/changelog đã đồng bộ. Tool account bootstrap và
  `pnpm db:seed` đã nghỉ hưu.
- Evidence W-0105 nguyên bản được khôi phục với banner `SUPERSEDED/HISTORICAL`.

## 2. Migration history

Không có evidence cho phép khẳng định migration drop chưa từng chạy ở shared environment. Vì vậy
W-0128 chọn phương án an toàn: **không đổi** technical migration ID
`20260828040458_W0122DropConsoleAccounts`. Tên này là historical mislabel; tracker/evidence hiện
alias thay đổi về W-0128. Rewrite một migration có thể đã apply sẽ làm lệch `__EFMigrationsHistory`.

## 3. Test và gate local

| Gate | Kết quả |
| --- | --- |
| Build/analyzer | `0 warning / 0 error` |
| Format | `dotnet format --verify-no-changes` PASS |
| Unit | `485/485` PASS |
| Integration | `229/229` PASS; tăng 6 test cho rotation/parity |
| Contract | `24/24` PASS; ghim `draft.22` |
| Chaos | `8/8` PASS |
| Admin UI | lint + typecheck + `176/176` + Next build PASS |
| Danger-header focused | `api-client.test.ts` `10/10` PASS |
| OpenAPI | lint, parse/fixtures, negative selftest, drift/hash, NSwag regenerate PASS |
| Portal/docs | build `14` artifact + docs selftest PASS |
| Traceability | regenerate/check `462` tagged test PASS |
| Helm | lint/render dev/staging/lab/prod PASS; UI absent; prod ingress empty; admin tokens in API |
| Compose | `docker compose ... config --quiet` PASS |
| Markdown map | 594 file, 663 link resolved, 200 unresolved; file/link bổ sung không tạo unresolved mới |
| GitNexus final change audit | aggregate dirty worktree: 78 tracked file, 454 symbol, 24 process, `CRITICAL`; trọng tâm là HTTP helper dùng chung đã được impact-check/cảnh báo trước sửa và phủ full UI suite |

Full PII/Gitleaks scan không được ghi thành PASS giả:

- PII scan giới hạn đúng hai evidence pack W-0105/W-0128: `2 file PASS`.
- PII scan còn đỏ chỉ ở evidence W-0122/W-0124 đã tồn tại trước W-0128.
- Gitleaks full worktree còn 45 hit ở migration designer/generated API docs/TTS artifacts và một
  seed reference; report không có source/config/evidence mới của W-0128. Đây là
  `PREEXISTING_GATE_FAILURE`, không phải release PASS.

## 4. Acceptance boundary và residual gates

Local implementation **không** tự chốt các quyết định của Module 3/Platform:

1. Module 3 phải ký mapping role/claim → `read|write|danger`; role lạ deny-by-default và `danger`
   không được grant ngầm.
2. Platform/M3 phải cung cấp secret-store paths, rotation owner/schedule và real NetworkPolicy
   namespace/pod identity. Dev placeholder không phải production secret.
3. Module 3 phải regenerate client từ exact OpenAPI `draft.22`, chứng minh token không vào browser,
   actor lấy từ authenticated subject và chạy positive/negative shared E2E cho từng tier.
4. M3 contract/sign-off, hosted GitLab CI, target/shared DB migration state, deployment/UAT và
   production remain `NOT_RUN`/`OWNER_DATA_REQUIRED`/`BLOCKED_EXTERNAL`.
5. W-0129 intake rejection-reason traceability là work item riêng; W-0128 không nhận scope đó.

Do đó phán quyết là `TESTS_PASS_LOCAL`, không phải `ACCEPTED`, integration-ready hay
production-ready.
