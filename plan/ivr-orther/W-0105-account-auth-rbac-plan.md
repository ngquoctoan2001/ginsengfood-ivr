# W-0105 — Tài khoản đăng nhập, Role và phân quyền cho IVR

Trạng thái tài liệu: `PLAN_REVIEWED_AND_IMPLEMENTED`  
Trạng thái triển khai: `TESTS_PASS`  
Ngày rà soát: `2026-08-22`  
Baseline source đã đọc: `main@845b237`  
Origin: `UNPLANNED` — owner requested

> `W-0105` đã được cấp và ghi `START` trong tracker ngày 2026-08-22. Triển khai
> local/lab và regression đã đạt `TESTS_PASS`; bằng chứng nằm tại
> [`docs/evidence/W-0105/README.md`](../../docs/evidence/W-0105/README.md). Trạng
> thái này không phải owner UAT, production deployment hay production readiness.
>
> `W-0104` đã được owner `ACCEPTED` tại activity `A-0313`; `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi. W-0105 chỉ bổ sung đăng nhập người dùng cho console IVR, không đóng `G-AUTH/W-0006` về service identity JWT/mTLS với Sales.

---

## 1. Kết luận rà soát bản Opus 5

Bản Opus 5 có nền tảng đúng ở các điểm: Ivr.Api phải sở hữu danh tính; mật khẩu phải hash; session phải thu hồi được; authorization phải enforce ở server; admin action phải có audit; và UI chỉ là lớp hỗ trợ trải nghiệm.

Các điểm sau đã được sửa trong bản kế hoạch này:

| Mức | Vấn đề trong bản cũ | Điều chỉnh bắt buộc |
| --- | --- | --- |
| **ĐÃ CHỐT** | Phạm vi `Operator` từng mâu thuẫn giữa câu “chỉ xem profile” và nghiệp vụ vận hành | Owner đã chọn `OD-ACC-01/B` ngày 2026-08-22: ngoài profile cá nhân, Operator có đúng `IVR_QUEUE_VIEW`, `IVR_SIM_DISABLE`, `IVR_MANUAL_RETRY`; không tự mở thêm quyền Ops cũ khác |
| **CAO** | Seed tên thật và hash của mật khẩu dùng chung ngay trong EF migration / `seed/agents.sample.json` | Migration chỉ tạo schema. Tài khoản thật được bootstrap qua công cụ có kiểm soát cho local/lab; không commit plaintext/hash credential và không đưa tên nhân viên thật vào sample fixture |
| **CAO** | Soft-delete giải phóng username để dùng lại | Username/`actor_id` là định danh audit, phải unique vĩnh viễn và **không được tái sử dụng** |
| **CAO** | `IvrDbContext` được xem như thay đổi DB nhỏ | GitNexus đánh giá `IvrDbContext` và `PersistenceModelConfiguration` là `CRITICAL`; plan bổ sung regression model, governance, feature flag, readiness và chaos |
| **CAO** | Đổi `callIvrApi` sang bearer nhưng chỉ dự kiến sửa vài test | GitNexus đánh giá `callIvrApi` là `CRITICAL`: 18 caller trực tiếp, 36 symbol, 17 flow. Phải regression toàn bộ màn admin và export |
| **TRUNG BÌNH** | Tách base mới `/v1/ivr/admin` nhưng lại thêm path vào OpenAPI có server base `/v1/ivr/order-confirmation` | Giữ base hiện tại để không tạo hai contract/BFF base. Route mới nằm dưới `/v1/ivr/order-confirmation` |
| **TRUNG BÌNH** | Ghi “thêm 7 operation” nhưng thiết kế gồm auth, account và role matrix | Thêm **11 operation**: 3 auth + 7 account + 1 role matrix |
| **TRUNG BÌNH** | Yêu cầu khác 5 mật khẩu gần nhất nhưng không có bảng/column password history | Loại password history khỏi W-0105. Không ghi yêu cầu mà data model không enforce được |
| **TRUNG BÌNH** | Nói admin mở khóa tài khoản nhưng không có endpoint/contract | W-0105 dùng lockout tự hết hạn; reset mật khẩu cũng xóa lockout. Không tuyên bố có thao tác unlock riêng |
| **TRUNG BÌNH** | `proxy.ts` được yêu cầu thêm `/accounts`, `/profile` vào matcher dù matcher hiện đã bắt mọi route | Không sửa matcher vì lý do này. Bổ sung authorization server-side tại từng page/route; `proxy` vẫn chỉ là optimistic cookie-presence check |
| **TRUNG BÌNH** | Chỉ cập nhật docs compliance, bỏ sót source governance | Bắt buộc sửa `DataClassification.cs`, `PersonalDataInventory.cs` và các guard test tương ứng |
| **THẤP** | Baseline `f39badc`, 25 `DbSet`, các test count cố định đã cũ | Baseline triển khai là `845b237`; source có 28 `DbSet`. DoD dùng “0 fail” thay vì hard-code tổng test dễ drift |

---

## 2. Yêu cầu đã chuẩn hóa

### 2.1 Role

Hệ thống chỉ có hai role người dùng console:

1. `Admin` — nhãn UI **Quản trị viên**.
2. `Operator` — nhãn UI **Nhân viên vận hành**.

Các role `OpsViewer`, `Ops`, `AdminIM` bị loại khỏi UI, fixture, documentation và contract mới. Mock authentication header vẫn tồn tại cho test backend cũ, nhưng không còn là cách người dùng đăng nhập console.

### 2.2 Tài khoản seed theo yêu cầu owner

| Họ tên | Username / actor ID | Role | Mật khẩu bootstrap |
| --- | --- | --- | --- |
| Quản trị hệ thống | `admin` | `Admin` | `123123123zZ*` |
| Nguyễn Quốc Toàn | `ngquoctoan2001` | `Operator` | `123123123zZ*` |
| Trương Công Phúc | `trcongphuc2003` | `Operator` | `123123123zZ*` |

Diễn giải an toàn của từ **“cố định”**:

- `admin` là username built-in, không được đổi tên, xóa, disable hoặc hạ role.
- `123123123zZ*` là mật khẩu **bootstrap ban đầu**, không phải mật khẩu bất biến vĩnh viễn.
- Admin có thể đổi/reset mật khẩu. Reset chính tài khoản đang đăng nhập sẽ thu hồi cả phiên hiện tại và buộc đăng nhập lại.
- Ba tài khoản trên chỉ được seed vào local/lab theo chính sách hiện có của [`seed/README.md`](../../seed/README.md): non-production only. Không tự động seed credential mặc định vào production.

### 2.3 Quyền theo quyết định `OD-ACC-01/B`

- Admin có toàn quyền trong **phạm vi console đã được phê duyệt** và toàn quyền quản lý tài khoản.
- Operator được đăng nhập, xem profile của chính mình, xem queue, disable SIM, manual retry và đăng xuất.
- Operator không được xem danh sách tài khoản/role matrix; không được pause/resume queue, enable SIM, review result, chỉnh config/integration/seed hay thực hiện bất kỳ account mutation nào.
- **Cập nhật 2026-08-22 — `OD-V1-20` đã duyệt (owner module IVR):** `IVR_FLAG_READ` và `IVR_RUNTIME_GATE_ADMIN` **được cấp cho `Admin`**; Operator không có cả hai. Hai câu bên dưới là trạng thái cũ của W-0105, giữ lại làm lịch sử.
  - ~~`IVR_RUNTIME_GATE_ADMIN` vẫn không cấp cho role nào vì `OD-V1-20` chưa được duyệt. W-0105 không được dùng để mở runtime safety gate.~~
  - ~~`IVR_FLAG_READ` giữ nguyên chưa cấp. Nếu cần mở quyền đọc feature flag cho Admin, phải xử lý cùng `OD-V1-20`, không lẫn vào account CRUD.~~
- Hệ quả cần theo dõi: `IVR_FLAG_READ` có hiệu lực ngay (hai GET flag/kill-switch trả `200` cho Admin). `IVR_RUNTIME_GATE_ADMIN` **chưa mở được gì** — `FeatureFlagAdminService` kiểm `IRuntimeGateAuthorization` trước, bản production `PendingRuntimeGateAuthorization` luôn `false`, nên POST đổi từ `403` sang `409 IVR_OPERATIONAL_BLOCKED`. `REAL_CUSTOMER_CALL_ALLOWED=NO` từ nay được giữ bằng **lớp khóa đó** chứ không còn bằng việc thiếu permission. Chữ ký thứ hai của four-eyes chưa có — xem `decisions-log.md`.

### 2.4 Quản lý account

Admin được:

- xem danh sách và chi tiết account;
- tạo account với role `Admin` hoặc `Operator`;
- sửa `display_name`, `role`, `status` của account thường;
- đặt mật khẩu mới theo policy; server không sinh/echo mật khẩu tạm trong W-0105;
- soft-delete account thường;
- xem ma trận hai role cố định.

Admin không được:

- đọc/lấy lại mật khẩu hiện tại — hash một chiều làm việc đó không thể và không nên có;
- đổi username sau khi tạo;
- tái sử dụng username đã soft-delete;
- xóa/disable/hạ role tài khoản built-in `admin`;
- tự cấp `IVR_RUNTIME_GATE_ADMIN` hoặc bypass bất biến P0 của IVR.

---

## 3. Baseline source đã xác minh trước triển khai

### 3.1 Backend authentication/authorization

- [`IvrPermissions.cs`](../../src/Ivr.Api/Auth/IvrPermissions.cs) có 9 permission và `All` là `FrozenSet`.
- [`RequirePermissionAttribute.cs`](../../src/Ivr.Api/Auth/RequirePermissionAttribute.cs) từ chối permission không nằm trong catalog; mọi permission mới phải thêm vào `All` trước khi map endpoint.
- [`MockPermissionAuthenticationHandler.cs`](../../src/Ivr.Api/Auth/MockPermissionAuthenticationHandler.cs) tạo claim trực tiếp từ `X-Permissions` trong MOCK.
- [`FailClosedAuthenticationHandler.cs`](../../src/Ivr.Api/Auth/FailClosedAuthenticationHandler.cs) trả `NoResult()` ngoài MOCK; hiện chưa có login bằng username/password.
- [`MockPermissionHeaderGuardMiddleware.cs`](../../src/Ivr.Api/Auth/MockPermissionHeaderGuardMiddleware.cs) chặn mock header ngoài MOCK.
- [`IvrApiServiceCollectionExtensions.cs`](../../src/Ivr.Api/Foundation/IvrApiServiceCollectionExtensions.cs) chọn một default scheme duy nhất: mock hoặc fail-closed; chưa có policy scheme cho bearer session.
- [`InternalServiceOptions.cs`](../../src/Ivr.Api/Internal/InternalServiceOptions.cs) enforce `X-Actor-Id == ClaimTypes.NameIdentifier`.
- [`IvrAdminEndpoints.cs`](../../src/Ivr.Api/Admin/IvrAdminEndpoints.cs) map route dưới `/v1/ivr/order-confirmation` và gắn `PiiMaskingFilter`.

Kết luận: authorization framework đã có; user authentication và account store chưa có. Header mock là test seam, không phải production identity.

### 3.2 Admin UI

- [`directory.ts`](../../admin-ui/src/lib/auth/directory.ts) hard-code 3 actor không có password.
- `admin-ui/src/app/login/LoginForm.tsx` là dropdown chọn actor.
- `admin-ui/src/app/api/auth/sign-in/route.ts` chỉ hoạt động ở MOCK.
- [`session.ts`](../../admin-ui/src/lib/auth/session.ts) tự chứng session bằng HMAC cookie, mang sẵn role/permission trong cookie và sống 8 giờ.
- [`session-cookie.ts`](../../admin-ui/src/lib/auth/session-cookie.ts) dùng `cache()` cho một render pass.
- [`client.ts`](../../admin-ui/src/lib/api/client.ts) gửi `X-Mock-Actor-Id` + `X-Permissions`; ngoài MOCK chủ động fail.
- [`proxy.ts`](../../admin-ui/src/proxy.ts) chỉ kiểm cookie có tồn tại, và matcher đã bao phủ toàn app.
- [`RequirePermission.tsx`](../../admin-ui/src/components/rbac/RequirePermission.tsx) chỉ ẩn/hiện ở client; comment trong source cũng khẳng định nó không phải security control.

Kết luận: phải bổ sung server-side page guard. Chỉ ẩn navigation/button không đủ để đáp ứng Operator chỉ xem profile.

### 3.3 Database và governance

- [`IvrDbContext.cs`](../../src/Ivr.Infrastructure/Persistence/IvrDbContext.cs) hiện có 28 `DbSet`, chưa có account/session.
- [`IvrPersistenceEntities.cs`](../../src/Ivr.Infrastructure/Persistence/Entities/IvrPersistenceEntities.cs) đã có `AuditLogEntity`; không tạo audit table thứ hai.
- [`PersistenceModelConfiguration.cs`](../../src/Ivr.Infrastructure/Persistence/PersistenceModelConfiguration.cs) điều khiển table/check/index và storage conventions.
- [`DataClassification.cs`](../../src/Ivr.Infrastructure/Governance/DataClassification.cs) yêu cầu mọi physical table được phân loại; test `EveryShippedTableIsClassified` sẽ đỏ nếu thiếu.
- [`PersonalDataInventory.cs`](../../src/Ivr.Infrastructure/Governance/PersonalDataInventory.cs) là source field-level privacy inventory; tên nhân viên và staff actor ID phải được ghi nhận có chủ đích.
- Retention/Privacy hiện vẫn có phần cần Legal ký; W-0105 không được tự khóa con số lưu account/session mà không có authority.

### 3.4 Contract và tracker tại baseline

- [`ivr-order-confirmation.v1.yaml`](../../specs/api/openapi/ivr-order-confirmation.v1.yaml) là `1.0.0-draft.9`, server base `/v1/ivr/order-confirmation`, hiện có 28 operation.
- Tracker đã ghi W-0104 `ACCEPTED` tại `A-0313`; W-0105 sau đó được cấp và ghi `START`/quyết định B tại `A-0314`.
- Worktree đang có WIP UI không thuộc W-0105. Triển khai được giữ theo diff có
  phạm vi trên baseline `845b237`, bảo toàn WIP ngoài scope. Trong lúc thực hiện,
  owner fast-forward `main` sang `f7c9be9` để nghiệm thu W-0104; không reset và
  không phát sinh xung đột chức năng với W-0105.

### 3.5 Trạng thái triển khai W-0105 hiện tại

Sau khi khóa `OD-ACC-01/B`, source đã có schema-only migration, account/session
store, bearer authentication, RBAC hai role, 11 OpenAPI operation `draft.10`,
bootstrap tool local/lab, account/profile UI và test tương ứng. Các mô tả §3.1–
§3.4 ở trên được giữ làm baseline review; không còn là mô tả current source sau
W-0105. Trạng thái cuối chỉ được nâng theo bằng chứng ở §16 và tracker.

---

## 4. Kiến trúc mục tiêu

```text
Browser
  └─ httpOnly SameSite=Strict cookie: opaque session token
      └─ Next.js Route Handler / Server Component
          ├─ POST auth/sign-in: username + password
          ├─ GET auth/session: resolve current account
          └─ Authorization: Bearer <opaque token> + X-Actor-Id
              └─ Ivr.Api policy scheme
                  ├─ bearer present -> ConsoleSession scheme
                  ├─ no bearer + MOCK -> existing MockPermissions scheme
                  └─ no bearer + non-MOCK -> FailClosed
                      └─ PostgreSQL
                          ├─ ivr_console_accounts
                          ├─ ivr_console_sessions (token hash only)
                          └─ ivr_audit_log + ivr_idempotency_keys (reuse)
```

Nguyên tắc:

1. Ivr.Api là nguồn sự thật cho account, role, permission và session.
2. UI không được tự mint role/permission.
3. Browser không giữ bearer token ở JavaScript/localStorage; token chỉ nằm trong httpOnly cookie.
4. DB chỉ lưu hash của token và hash của password.
5. Mỗi request bearer tra account đang `ACTIVE`, session chưa revoke/chưa hết hạn, rồi derive permission từ role ở server.
6. Mock scheme chỉ giữ để regression backend; bearer luôn thắng nếu request có `Authorization: Bearer`.
7. Không refresh token ở W-0105; session có absolute TTL 8 giờ, hết hạn phải đăng nhập lại.

---

## 5. Role và permission catalog

### 5.1 Permission mới

| Permission | Mục đích |
| --- | --- |
| `IVR_ACCOUNT_VIEW` | list/detail account và xem role matrix |
| `IVR_ACCOUNT_MANAGE` | create/update/disable/reactivate/soft-delete account |
| `IVR_ACCOUNT_PASSWORD_RESET` | đặt/reset password của account |
| `IVR_ACCOUNT_SELF_VIEW` | xem profile của chính subject hiện tại |

Không thêm `IVR_ACCOUNT_SELF_PASSWORD` trong W-0105 vì owner đã giới hạn Operator ở read-only profile. Không dựng feature flag/endpoint chết để “phòng khi dùng”.

Tổng catalog sau thay đổi: **13 permission** = 9 hiện có + 4 mới.

### 5.2 Ma trận role

| Permission | Admin | Operator |
| --- | :---: | :---: |
| `IVR_QUEUE_VIEW` | ✅ | ✅ |
| `IVR_QUEUE_PAUSE` | ✅ | ❌ |
| `IVR_QUEUE_RESUME` | ✅ | ❌ |
| `IVR_SIM_ENABLE` | ✅ | ❌ |
| `IVR_SIM_DISABLE` | ✅ | ✅ |
| `IVR_MANUAL_RETRY` | ✅ | ✅ |
| `IVR_RESULT_REVIEW` | ✅ | ❌ |
| `IVR_FLAG_READ` | ❌ | ❌ |
| `IVR_RUNTIME_GATE_ADMIN` | ❌ | ❌ |
| `IVR_ACCOUNT_VIEW` | ✅ | ❌ |
| `IVR_ACCOUNT_MANAGE` | ✅ | ❌ |
| `IVR_ACCOUNT_PASSWORD_RESET` | ✅ | ❌ |
| `IVR_ACCOUNT_SELF_VIEW` | ✅ | ✅ |

Role mapping nằm duy nhất trong backend `IvrRoles`; UI lấy role matrix từ API và chỉ giữ TypeScript union để type-check. Contract-drift test phải so UI union với OpenAPI/backend response.

### 5.3 P0 không thay đổi

Không role nào được force confirm/cancel order, reset customer attempt count, vượt attempt policy, bypass blocker, tự bật real customer calls hoặc tự cấp runtime-gate permission.

---

## 6. Data model

### 6.1 `ivr_console_accounts`

Tên `console_accounts` được dùng thay cho `admin_accounts` vì bảng chứa cả Admin và Operator.

| Column | Type | Rule |
| --- | --- | --- |
| `id` | `uuid` | PK, server generated |
| `username` | `text` | global unique, lowercase ASCII, 3..64; immutable; không tái sử dụng sau delete |
| `display_name` | `text` | trim, Unicode, 1..128; personal data |
| `role` | `text` | `CHECK IN ('Admin','Operator')` |
| `status` | `text` | `CHECK IN ('ACTIVE','DISABLED','DELETED')` |
| `is_builtin` | `boolean` | `admin=true`; built-in không delete/disable/demote |
| `password_hash` | `text` | PBKDF2-SHA512 payload versioned (210.000 iteration, salt/hash riêng); không có plaintext |
| `failed_login_count` | `integer` | `0..100`, update atomically |
| `locked_until` | `timestamptz?` | 5 lần sai -> khóa 15 phút |
| `last_login_at` | `timestamptz?` | chỉ update khi login thành công |
| `password_changed_at` | `timestamptz` | revoke session khi đổi |
| `created_at` / `updated_at` | `timestamptz` | actor nằm trong append-only audit log |
| `deleted_at` | `timestamptz?` | soft-delete timestamp |
| `version` | `bigint` | optimistic concurrency token |
| retention columns | inherited | class `staff_account`; thời hạn prod còn `OWNER_DATA_REQUIRED` |

Constraint/index:

- unique index thường trên `username`, **không** partial index;
- check regex `^[a-z][a-z0-9._-]{2,63}$`;
- index `(status, role)` phục vụ list/invariant;
- không lưu `actor_id` trùng lặp; API/audit dùng `username` làm actor ID, tránh hai cột drift.

### 6.2 `ivr_console_sessions`

| Column | Type | Rule |
| --- | --- | --- |
| `id` | `uuid` | PK |
| `account_id` | `uuid` | FK `RESTRICT` tới account; account soft-delete |
| `token_hash` | `char(64)` | SHA-256 hex, unique; raw token không lưu |
| `created_at` | `timestamptz` | |
| `expires_at` | `timestamptz` | `created_at < expires_at`; default absolute TTL 8h |
| `revoked_at` | `timestamptz?` | |
| `revoke_reason` | `text?` | safe enum/string, không chứa secret |
| retention columns | inherited | class `console_session`; retention prod còn `OWNER_DATA_REQUIRED` |

Index: unique `token_hash`, `(account_id, expires_at)`, `revoked_at` và retention/legal-hold indexes.

Không update `last_seen_at` ở mọi request để tránh write amplification. Nếu sau này cần idle timeout, mở work riêng có throttled touch; W-0105 chỉ dùng absolute TTL.

### 6.3 Không có password-history table

W-0105 enforce:

- tối thiểu 12 ký tự;
- có chữ hoa, chữ thường, số và ký tự đặc biệt;
- không chứa username;
- hash có salt riêng và PBKDF2 parameters được ghim trong payload/versioned hasher, không phụ thuộc ngầm vào default package.

Yêu cầu “khác 5 mật khẩu gần nhất” bị loại khỏi scope vì cần một password-history store và retention riêng. Có thể mở work sau nếu Security yêu cầu.

### 6.4 Audit và idempotency

Tái sử dụng `ivr_audit_log` và `ivr_idempotency_keys`; không tạo cơ chế song song.

Audit actions được hiện thực: `ACCOUNT_SIGN_IN`, `ACCOUNT_SIGN_OUT`,
`ADMIN_ACCOUNT_CREATE`, `ADMIN_ACCOUNT_UPDATE`,
`ADMIN_ACCOUNT_PASSWORD_RESET`, `ADMIN_ACCOUNT_DELETE`, và
`ADMIN_ACCOUNT_BOOTSTRAP`. Failed-login/lockout vẫn được lưu durable trên account
nhưng không ghi riêng một audit event có thể làm lộ username không tồn tại.

`before_state_json`, `after_state_json`, log, trace và error envelope cấm chứa password, password hash, raw session token hoặc Authorization header. Với username không tồn tại, security event chỉ ghi fingerprint/hash an toàn, không tạo account enumeration surface.

### 6.5 Migration

EF migration gồm:

- 2 table, FK, check, index;
- append-only/audit protection không bị thay đổi;
- model snapshot/designer được regenerate đúng công cụ;
- **không có `InsertData` cho ba credential thật**;
- `Down` chỉ dùng cho môi trường chưa có dữ liệu; production rollback là forward-fix/approved runbook, không tuyên bố “drop an toàn”.

Do blast radius `CRITICAL`, migration gate phải chạy thêm model classification, privacy inventory, readiness, feature-flag persistence và chaos regression.

---

## 7. Bootstrap/seed account

### 7.1 Cách seed

Tạo công cụ `tools/Ivr.AccountBootstrap` dùng cùng versioned hasher và DbContext runtime.

Yêu cầu của công cụ:

- interactive/secret input; không nhận password qua command-line argument để tránh shell history/process list;
- hỗ trợ secret file mount nằm ngoài repo hoặc stdin bảo mật cho CI/local automation;
- chỉ chạy khi `--environment local|lab` hoặc `IVR_ACCOUNT_BOOTSTRAP_ENVIRONMENT=local|lab`; mọi giá trị khác, bao gồm production, fail trước khi kết nối DB;
- nhận connection string từ `ConnectionStrings__IvrDb` hoặc `IVR_ACCOUNT_BOOTSTRAP_CONNECTION_STRING`;
- gọi migration trước khi tạo dữ liệu;
- idempotent nếu username + display name + role đã đúng;
- không tự rotate/ghi đè account đang tồn tại; metadata lệch thì fail;
- output chỉ username/role/status/audit ID, không in password/hash.

### 7.2 Dữ liệu thực thi local/lab

Bootstrap tạo đúng ba account ở §2.2. Sau khi chạy:

1. query DB xác nhận 3 username, role, status, `is_builtin`;
2. xác nhận `password_hash` khác plaintext và verify được qua runtime hasher;
3. login smoke cả ba account;
4. xác nhận Operator được chuyển tới `/profile` và mọi account/admin API khác trả 403;
5. lưu evidence đã redacted, không chụp/raw-log password.

### 7.3 Không sửa fixture thành dữ liệu nhân sự thật

[`seed/agents.sample.json`](../../seed/agents.sample.json) vẫn là sample non-production và không được chứa Nguyễn Quốc Toàn/Trương Công Phúc hay password thật. File này chỉ đổi role shape thành hai actor giả (`TEST-ADMIN`, `TEST-OPERATOR`) để test drift; account thật chỉ vào database mục tiêu qua bootstrap.

PII scan không nhận diện tên người **không có nghĩa** tên người hết là PII. `display_name` vẫn phải vào classification/inventory và access control.

### 7.4 Production bootstrap

Production không dùng shared default trong kế hoạch này. Khi cần production:

- password admin đầu tiên phải lấy từ secret manager/one-time secure input;
- bootstrap job chạy một lần, có audit và bị disable sau thành công;
- Security/Platform owner phê duyệt cách lưu/cấp secret;
- nếu owner bắt buộc giữ literal password đã công khai trong plan ở production thì trạng thái là `RELEASE_BLOCKED`, không được tự nới guard.

---

## 8. API contract

Giữ server base hiện có `/v1/ivr/order-confirmation`. Version dự kiến `1.0.0-draft.9 -> 1.0.0-draft.10`.

### 8.1 Authentication — 3 operation

| Method | Path | Auth | Contract |
| --- | --- | --- | --- |
| `POST` | `/auth/sign-in` | anonymous + rate limit | username/password -> token + current profile |
| `GET` | `/auth/session` | bearer | current profile + derived permissions |
| `POST` | `/auth/sign-out` | bearer | revoke current token; idempotent |

Sign-in rule:

- normalize username lowercase/trim trước lookup;
- sai username, sai password, disabled, deleted hoặc đang lock đều trả `401 IVR_UNAUTHENTICATED` với cùng message/body shape;
- missing username vẫn chạy dummy password verify trước response để giảm timing enumeration;
- 5 lần sai liên tiếp -> lock 15 phút; update bằng row lock/transaction để request đồng thời không làm mất count;
- success reset failed count, set `last_login_at`, tạo session và audit trong cùng transaction;
- response có `Cache-Control: no-store` và không đi qua body logging.

### 8.2 Account — 7 operation

| Method | Path | Permission | Idempotency |
| --- | --- | --- | --- |
| `GET` | `/accounts` | `IVR_ACCOUNT_VIEW` | — |
| `GET` | `/accounts/me` | `IVR_ACCOUNT_SELF_VIEW` | — |
| `GET` | `/accounts/{accountId}` | `IVR_ACCOUNT_VIEW` | — |
| `POST` | `/accounts` | `IVR_ACCOUNT_MANAGE` | bắt buộc |
| `PATCH` | `/accounts/{accountId}` | `IVR_ACCOUNT_MANAGE` | bắt buộc |
| `POST` | `/accounts/{accountId}:reset-password` | `IVR_ACCOUNT_PASSWORD_RESET` | bắt buộc |
| `DELETE` | `/accounts/{accountId}` | `IVR_ACCOUNT_MANAGE` | bắt buộc |

Route detail dùng UUID constraint; `/accounts/me` vẫn map rõ ràng và không dựa
vào client-supplied username.

`update` chỉ nhận field whitelist: `display_name`, `role`, `status`, `reason`. Username không có trong update DTO.

Reset password:

- body bắt buộc có `new_password`, `version` và `reason`;
- validate rồi set; response **không echo** password;
- response `no-store`, audit không chứa password/hash;
- revoke toàn bộ session của target và clear lockout;
- reset chính mình làm phiên hiện tại mất hiệu lực; UI phải chuyển về login.

### 8.3 Role matrix — 1 operation

| Method | Path | Permission | Contract |
| --- | --- | --- | --- |
| `GET` | `/account-roles` | `IVR_ACCOUNT_VIEW` | hai role + label + permission[] |

Endpoint này làm backend thành nguồn sự thật cho màn role. Không đọc `MOCK_DIRECTORY` trong UI nữa.

### 8.4 Bất biến transaction

1. `admin` built-in không rename/delete/disable/demote.
2. Username immutable, global unique kể cả sau soft-delete.
3. Role/status/password/delete thay đổi -> revoke mọi session của target trong cùng transaction.
4. Account mutation + idempotency response snapshot + audit commit cùng nhau.
5. Hai request tạo cùng username: một thành công, một `409 IVR_ACCOUNT_CONFLICT`; không 500.
6. Vi phạm built-in/last-active-admin/policy trả `422 IVR_ACCOUNT_POLICY_VIOLATION`.
7. Account không tồn tại trả `404 IVR_NOT_FOUND`.
8. Operator gọi endpoint quản trị trả `403 IVR_FORBIDDEN_CALLER` trước khi lộ resource tồn tại.

### 8.5 Error codes

Thêm đúng hai code vào [`IvrErrorCodes.cs`](../../src/Ivr.Domain/Errors/IvrErrorCodes.cs), [`06-error-codes.md`](../../specs/api/06-error-codes.md), OpenAPI và TypeScript mirror:

- `IVR_ACCOUNT_CONFLICT` -> 409;
- `IVR_ACCOUNT_POLICY_VIOLATION` -> 422.

Không thêm `IVR_ACCOUNT_LOCKED` vào sign-in response vì sẽ làm lộ account. `IVR_ACCOUNT_LOCKED` chỉ là audit action nội bộ.

### 8.6 OpenAPI/change pipeline

Thêm **11 operation**, security scheme opaque bearer, request/response schema, error response và examples không chứa account/password thật.

Chạy đủ:

1. Redocly/OpenAPI parse + local refs;
2. NSwag codegen;
3. UI handwritten type/contract drift tests;
4. `contract-manifest.json` re-pin bằng command được duyệt;
5. changelog draft.2 -> draft.10;
6. pinned `oasdiff breaking --fail-on WARN` phải không breaking;
7. API portal/Redoc regenerate;
8. `openapi-contract-drift.mjs` pass.

---

## 9. Backend implementation map

### 9.1 Domain

| File/nhóm | Thay đổi |
| --- | --- |
| `Accounts/ConsoleAccountPolicies.cs` | role/status catalog, username/password/lockout policy |
| `Infrastructure/Accounts/ConsolePasswordHasher.cs` | PBKDF2 hash/verify + rehash signal |
| `Accounts/ConsoleAccountService.cs` | raw token 256-bit + SHA-256 hash, account/session lifecycle |
| `Errors/IvrErrorCodes.cs`, `IvrErrors.cs` | hai account error code |

Domain không tham chiếu EF entity, `HttpContext`, ASP.NET Identity type hoặc package Infrastructure.

### 9.2 Infrastructure

| File/nhóm | Thay đổi |
| --- | --- |
| `Accounts/ConsolePasswordHasher.cs` | versioned PBKDF2-SHA512 hash/verify, constant-time compare |
| `Persistence/Entities/ConsoleAccountEntities.cs` | account + session entity |
| `Persistence/PersistenceModelConfiguration.cs` | `ConfigureConsoleAccounts` |
| `Persistence/IvrDbContext.cs` | 2 `DbSet` mới |
| `Persistence/Migrations/*W0105ConsoleAccountAuth*.cs` | schema-only migration + snapshot |
| `Governance/DataClassification.cs` | classify 2 table |
| `Governance/PersonalDataInventory.cs` | username/display_name/actor metadata |
| `Retention/*` | strategy/config cho session/account chỉ sau policy; chưa ký thì fail-closed `NOT_CONFIGURED` |
| `tools/Ivr.AccountBootstrap` | seed có kiểm soát |

Không thêm package identity/crypto mới; dùng API cryptography của .NET đã ghim
bởi target framework để không phát sinh lock-file drift.

### 9.3 API

| File/nhóm | Thay đổi |
| --- | --- |
| `Auth/IvrPermissions.cs` | thêm 4 permission vào `All` |
| `Auth/IvrRoles.cs` | mapping role -> permission, một nguồn backend |
| `Auth/ConsoleSessionAuthenticationHandler.cs` | bearer -> session/account -> claims |
| `Accounts/ConsoleAccountEndpoints.cs` | 11 route |
| `Accounts/ConsoleAccountContracts.cs` | DTO, whitelist field, no secret output |
| `Accounts/ConsoleAccountService.cs` | auth, token, CRUD, invariant, audit/idempotency transaction |
| `Foundation/IvrApiServiceCollectionExtensions.cs` | policy-scheme selector bearer/mock/fail-closed, policies, rate limiter, DI |
| `Program.cs` | map account/auth endpoints |

`NameIdentifier` luôn là ASCII username, không phải `display_name`; `X-Actor-Id` phải khớp claim như hiện tại.

---

## 10. Admin UI implementation map

### 10.1 Authentication/session

| File/nhóm | Thay đổi |
| --- | --- |
| `src/lib/auth/directory.ts` | bỏ khỏi runtime; test fixtures chuyển sang fake generic |
| `src/lib/auth/session.ts` | chỉ giữ type; bỏ HMAC seal/unseal |
| `src/lib/auth/session-cookie.ts` | cookie chứa raw opaque token; giữ httpOnly/SameSite=Strict/Secure/path |
| `src/lib/auth/session-cookie.ts` | đồng thời resolve `GET /auth/session`, `cache()` một render pass |
| `src/lib/auth/guard.ts` | `requireSession` + `requirePermission`/`requireAdmin` |
| `src/lib/auth/sign-in.ts` | giữ `safeRedirectTarget`; thêm role-aware home route |
| `src/app/api/auth/sign-in/route.ts` | form username/password -> API -> set cookie |
| `src/app/api/auth/sign-out/route.ts` | revoke best-effort rồi luôn clear local cookie |
| `src/app/login/LoginForm.tsx` | username/password input, autocomplete đúng chuẩn |
| `src/app/login/page.tsx` | login chạy mọi execution mode; không render account directory |
| `src/lib/config/env.ts` | bỏ session HMAC secret; giữ API origin/server config |

Không lưu password/token trong query string, `localStorage`, client React state dài hạn, analytics hay error text.

### 10.2 API client — vùng `CRITICAL`

[`callIvrApi`](../../admin-ui/src/lib/api/client.ts) có 18 caller trực tiếp. Thay đổi hẹp:

- giữ nguyên correlation/idempotency/error-envelope behavior;
- thêm `Authorization: Bearer` từ server-only session token;
- vẫn gửi `X-Actor-Id` bằng username để thỏa backend invariant;
- không gửi `X-Permissions`/`X-Mock-Actor-Id` cho user session;
- tạo client auth riêng cho anonymous sign-in để không ép `AdminSession` giả;
- regression tất cả wrapper trong `api/admin.ts`, analytics client, export route, Server Actions và mọi screen.

### 10.3 Route authorization

Operator được `/profile` và các màn hình tối thiểu cần cho xem queue, disable SIM, manual retry. Vì `RequirePermission` là client-only, từng server page/action phải gọi `requirePermission` trước data fetch/render và từng mutation vẫn phải được backend kiểm quyền riêng.

Các route hiện có phải được guard theo permission:

- `/dashboard` -> `IVR_QUEUE_VIEW` (Admin + Operator)
- `/calls`, `/calls/[id]` -> `IVR_QUEUE_VIEW` (Admin + Operator); action retry vẫn cần `IVR_MANUAL_RETRY`
- `/reports` và export
- `/review`
- `/config`
- `/integration`
- `/seed`
- `/roles`
- `/accounts`

Thêm static guard test có inventory route -> required permission để không sót page sau này.

Role-aware navigation:

- Admin login mặc định -> `/dashboard`;
- Operator login mặc định -> `/dashboard`;
- Operator được vào `/dashboard`, `/calls`, `/calls/[id]`, `/profile`; route ngoài tập quyền bị chuyển về `/dashboard` mà không render dữ liệu;
- navigation của Operator chỉ có Tổng quan queue, Cuộc gọi, Profile và Đăng xuất; các nút chỉ hiện đúng `IVR_SIM_DISABLE`/`IVR_MANUAL_RETRY`.

### 10.4 Màn hình

| Route/component | Nội dung |
| --- | --- |
| `/accounts` | list/filter account, trạng thái, role, last login; Admin only |
| account create/edit dialog | field whitelist; không sửa username sau create |
| reset-password form | Admin nhập mật khẩu mới theo policy; server không echo/generate secret |
| `/profile` | username, display name, role label, status, last login, password updated; read-only |
| `/roles` | đọc `/account-roles`; không còn `MOCK_DIRECTORY` |
| `ConsoleNav`/`AppShell` | role label Việt, nav theo permission, không lộ count/account list cho Operator |

Profile read-only test không chỉ tìm button/input chung chung vì nút Đăng xuất nằm ở shell; test phải giới hạn trong vùng profile content.

### 10.5 UI E2E infrastructure

E2E auth hiện cố tình đặt `IVR_API_BASE_URL` tới port không tồn tại. Sau W-0105 giả định đó không còn đúng.

Phải tách hai lớp:

1. UI E2E nhanh: khởi động HTTP auth/account stub, không chứa dữ liệu thật, trả token/profile deterministic.
2. Cross-stack E2E: PostgreSQL + Ivr.Api + Next.js; chạy bootstrap test account, login thật, kiểm revoke/403.

Không sửa test để quay lại mint cookie/permission trong Next.js.

---

## 11. Bảo mật bắt buộc

| Control | Thiết kế/gate |
| --- | --- |
| Hash password | Versioned PBKDF2-SHA512 payload, 210.000 iteration, salt riêng, constant-time compare, rehash-on-login |
| Token | 256-bit CSPRNG; DB lưu SHA-256 hash; raw token chỉ ở httpOnly cookie |
| Enumeration | generic 401, dummy verify cho user không tồn tại, timing test theo tolerance |
| Brute force | 5 sai/15 phút + rate limit theo IP và normalized username |
| Session revoke | role/status/password/delete -> revoke all; sign-out -> revoke current |
| Session TTL | absolute 8h, no refresh token |
| CSRF | giữ same-origin POST check + SameSite=Strict |
| Cookie | httpOnly, SameSite=Strict, Secure ở deployment HTTPS, Path=/, Max-Age <= server expiry |
| Secret response | `Cache-Control: no-store`; body logging/tracing redaction; không echo/ghi password vào audit |
| Header injection | Bearer request không fallback sang mock; mock headers ngoài MOCK vẫn 403 |
| Account invariant | built-in admin immutable username/role/status; username never reused |
| Seed | schema migration không chứa credential; real names/password không vào sample fixture |
| Audit | mutation + audit + idempotency atomic; before/after redacted |

Bootstrap validation:

- chỉ `--environment local|lab` hoặc env tương ứng được chạy; production,
  unknown và missing đều fail trước DB access;
- password chỉ qua secret environment/STDIN/hidden prompt, không qua command-line
  argument và không được log;
- không dựa vào so sánh salted password hash với một constant.

---

## 12. Kế hoạch kiểm thử

### 12.1 Unit backend

- hash/verify success, wrong/corrupt hash fail-safe, `SuccessRehashNeeded`;
- cùng password tạo hash khác nhau;
- password policy nhận `123123123zZ*`, từ chối ngắn/thiếu nhóm/chứa username;
- username normalization/ASCII pattern/Unicode/length;
- lock lần 5, tự mở sau 15 phút, success reset counter;
- token đủ entropy, base64url, hash deterministic, raw token != stored value;
- role matrix: Admin có đúng approved permissions; Operator có đúng self-view + queue-view + SIM-disable + manual-retry; runtime gate không role nào có;
- built-in `admin` invariant;
- error/audit redaction không chứa password/hash/token.

### 12.2 Integration backend + PostgreSQL

- migration up từ clean DB và từ latest baseline; model snapshot không drift;
- bootstrap local/lab tạo đúng ba account, idempotent, không overwrite;
- production/default bootstrap fail-closed;
- login đúng cho ba account; mọi nhánh sai trả cùng 401 envelope;
- 5 concurrent bad login không làm mất count; locked account không login dù password đúng;
- bearer session hợp lệ, expired/revoked/deleted/disabled đều 401;
- Operator: `/accounts/me` và queue read 200; SIM disable/manual retry 2xx; `/accounts`, `/account-roles`, queue pause/resume, SIM enable và mọi admin-only action 403;
- Admin CRUD trọn vòng; username cũ không được reuse sau delete;
- built-in `admin` không delete/disable/demote;
- reset password làm password cũ fail, password mới pass, tất cả session cũ revoke;
- role/status change revoke session;
- duplicate username race trả 409 typed error, không 500;
- idempotency replay same payload trả same response; different payload conflict;
- `X-Permissions` không nâng quyền của bearer;
- `X-Actor-Id` lệch subject trả 403;
- audit mỗi mutation đúng một lần, before/after không có secret;
- MOCK header test cũ vẫn pass; non-MOCK mock-header guard vẫn pass;
- DataClassification/PersonalDataInventory/retention tests nhận đủ table/field mới;
- readiness, feature flag persistence, PostgreSQL append-only và chaos regression vẫn pass.

### 12.3 Admin UI

- login form dùng `username`/`password`, không render directory;
- giữ open-redirect và cross-site POST negative tests;
- invalid login không phát cookie;
- opaque cookie có đúng flags và không giải mã thành role/permission;
- session resolver cache một lần/render pass, expired/revoked -> login;
- Admin thấy accounts/actions; Operator không thấy nav/action/account count;
- server route inventory chỉ cho Operator vào dashboard/queue-call/profile và chặn mọi route ngoài permission;
- profile content read-only;
- reset nhận mật khẩu mới do Admin nhập, không sinh/echo lại mật khẩu từ server;
- sign-out clear cookie kể cả API revoke lỗi, đồng thời ghi safe operational error;
- mọi `callIvrApi` caller gửi bearer/correlation/idempotency đúng;
- lint, typecheck, Vitest và Next build không lỗi.

### 12.4 Cross-stack acceptance local/lab

1. PostgreSQL migrate sạch.
2. Bootstrap ba account qua secret input.
3. Login `admin` bằng mật khẩu owner yêu cầu -> `/dashboard`.
4. Login `ngquoctoan2001` và `trcongphuc2003` -> `/dashboard`; profile vẫn read-only.
5. Operator xem queue, disable SIM và manual retry thành công; gọi trực tiếp `/accounts`, pause/resume queue, enable SIM hoặc review result -> 403/không render data.
6. Admin tạo account, sửa role/status, reset password, soft-delete; kiểm audit DB.
7. Token cũ chết ngay sau reset/delete/status/role change.
8. Evidence redacted; không lưu screenshot/log có password.

### 12.5 Regression/gates

```powershell
dotnet format --verify-no-changes
dotnet test Ivr.sln
npm --prefix admin-ui run lint
npm --prefix admin-ui run typecheck
npm --prefix admin-ui test
npm --prefix admin-ui run build
```

Ngoài ra chạy OpenAPI validate/codegen/drift/oasdiff/portal, traceability regeneration, PII self-test/scan, security scan, Gitleaks theo staged diff, migration apply, image/config smoke liên quan và `gitnexus detect-changes` trước commit.

Không hard-code “489 PASS” hoặc tổng test cụ thể. Tiêu chuẩn là toàn bộ suite hiện hành được discover, **0 fail**, không test bị xóa/skip để làm xanh.

---

## 13. Thứ tự triển khai

| Phase | Phạm vi | Exit gate |
| --- | --- | --- |
| **P0 — Freeze** | `OD-ACC-01/B` đã được owner xác nhận; cấp W-0105 trong tracker; kiểm kê và bảo toàn WIP UI hiện tại; chạy GitNexus impact cho từng symbol trước sửa | baseline SHA + scope + impact log; HIGH/CRITICAL đã báo owner |
| **P1 — Contract/domain** | role, permissions, account/password/lockout policies, OpenAPI draft.10 skeleton, error taxonomy | unit policy + OpenAPI parse; no owner ambiguity |
| **P2 — Persistence/governance** | entity, model config, migration schema-only, repository, audit/idempotency, classification/inventory | migration + model/governance/PostgreSQL regression xanh |
| **P3 — Auth/session** | hasher, token, scheme selector, sign-in/session/sign-out, lockout/rate limit/revoke | auth integration + mock continuity xanh |
| **P4 — Account APIs** | CRUD/reset/profile/role matrix + invariants | full account integration + OpenAPI/codegen/oasdiff xanh |
| **P5 — Bootstrap** | controlled local/lab bootstrap, seed đúng ba account, production guard | idempotent seed + login smoke + redacted evidence |
| **P6 — Admin UI** | login, cookie, resolver, bearer client, server route guards, accounts/profile/roles | UI unit/component/E2E + cross-stack xanh |
| **P7 — Closure** | docs, tracker, risk, traceability, full regression, detect changes | evidence complete; residual gates ghi đúng status |

Ước lượng kỹ thuật: **8–12 ngày công** cho một người, chưa gồm thời gian owner/security/privacy review. Vùng DB model và `callIvrApi` đều `CRITICAL`, nên không rút ngắn bằng cách bỏ full regression.

---

## 14. Tài liệu/source phải đồng bộ khi triển khai

- [`specs/ui/08-role-permission-ui.md`](../../specs/ui/08-role-permission-ui.md)
- [`specs/api/03-admin-api.md`](../../specs/api/03-admin-api.md)
- [`specs/api/06-error-codes.md`](../../specs/api/06-error-codes.md)
- [`specs/api/openapi/ivr-order-confirmation.v1.yaml`](../../specs/api/openapi/ivr-order-confirmation.v1.yaml)
- `specs/database/01-erd.md`, `02-tables.md`, `04-indexes.md`, `05-retention-and-privacy.md`
- [`DataClassification.cs`](../../src/Ivr.Infrastructure/Governance/DataClassification.cs)
- [`PersonalDataInventory.cs`](../../src/Ivr.Infrastructure/Governance/PersonalDataInventory.cs)
- [`docs/compliance/ivr-data-inventory.md`](../../docs/compliance/ivr-data-inventory.md)
- [`docs/compliance/retention.md`](../../docs/compliance/retention.md)
- [`seed/agents.sample.json`](../../seed/agents.sample.json) — chỉ fake actors, không real staff/password
- [`seed/README.md`](../../seed/README.md)
- [`admin-ui/README.md`](../../admin-ui/README.md)
- [`00-index.md`](00-index.md)
- [`14-risk-register.md`](14-risk-register.md)
- [`decisions-log.md`](decisions-log.md)
- [`prompt/_execution/prompt-execution-tracker.md`](../../prompt/_execution/prompt-execution-tracker.md)
- `docs/traceability-tests.md`
- `docs/evidence/W-0105/README.md`

---

## 15. Owner decision và residual gate

### `OD-ACC-01` — phạm vi thật của Operator — `LOCKED: B`

Owner đã chọn **B — operational** ngày `2026-08-22`. Operator có đúng bốn permission:

- `IVR_ACCOUNT_SELF_VIEW`;
- `IVR_QUEUE_VIEW`;
- `IVR_SIM_DISABLE`;
- `IVR_MANUAL_RETRY`.

Tên “role Ops cũ” không được dùng để tự suy diễn thêm `QUEUE_PAUSE`, `QUEUE_RESUME`, `SIM_ENABLE`, `RESULT_REVIEW` hoặc quyền quản lý tài khoản.

### `OWNER_DATA_REQUIRED` — retention account/session

Legal/Privacy phải chốt thời hạn/anonymization cho staff account và session trước production. Trong lúc chưa chốt:

- schema và local/lab test được phép triển khai;
- retention job báo `NOT_CONFIGURED`/fail-closed cho class mới;
- không tuyên bố production-ready.

### Các quyết định không mở lại trong W-0105

- Soft-delete + username không reuse.
- Không self-service đổi/quên password cho Operator.
- TTL session 8 giờ, không refresh.
- Shared default chỉ local/lab.
- `IVR_RUNTIME_GATE_ADMIN` và `IVR_FLAG_READ` không được cấp qua work này.

---

## 16. Định nghĩa hoàn thành

- [x] Tracker đã cấp W-0105 và ghi `START`; diff có phạm vi bảo toàn WIP ngoài scope và hấp thụ owner fast-forward W-0104 không xung đột.
- [x] Backend/OpenAPI/UI chỉ còn hai role `Admin`, `Operator` cho user console.
- [x] Operator có đúng `IVR_ACCOUNT_SELF_VIEW`, `IVR_QUEUE_VIEW`, `IVR_SIM_DISABLE`, `IVR_MANUAL_RETRY`; mọi route/API ngoài bốn quyền này fail-closed.
- [x] `admin` built-in tồn tại, username/role/status được bảo vệ.
- [x] Hai account Toàn/Phúc được bootstrap đúng username/role trong local/lab.
- [x] Ba account login được bằng bootstrap password theo yêu cầu trong evidence local/lab redacted.
- [x] Admin create/update/disable/reactivate/reset/soft-delete account qua UI và API.
- [x] Không có API/UI nào “lấy lại” plaintext password hiện tại.
- [x] Username đã delete không thể tái sử dụng.
- [x] Password/token chỉ lưu hash; role/status/password/delete revoke session đúng transaction.
- [x] Audit/idempotency atomic và không chứa secret.
- [x] Migration schema-only; không có real staff/password/hash trong migration hoặc sample seed.
- [x] DataClassification, PersonalDataInventory và tài liệu privacy đồng bộ.
- [x] OpenAPI draft.10 có đúng 11 operation mới; codegen/manifest/changelog/portal/oasdiff/drift đều pass.
- [x] Full .NET regression, UI lint/typecheck/test/build, migration, security/PII/Gitleaks và GitNexus change detection hoàn tất với 0 test/gate failure; blast radius giữ nguyên `CRITICAL` và được review bằng full regression.
- [x] `docs/evidence/W-0105/` ghi rõ local/lab proof, residual `OWNER_DATA_REQUIRED`, `G-AUTH/W-0006` vẫn `BLOCKED_EXTERNAL`.
- [x] `REAL_CUSTOMER_CALL_ALLOWED=NO` không đổi; không suy diễn account login thành telephony/Sales/production readiness.
