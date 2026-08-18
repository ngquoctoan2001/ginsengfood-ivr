# W-0032 — Evidence: Service JWT, optional mTLS and audit (`P4-4`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS` (mock-only — trần của slice này theo DoD `P4-4` §5)

## 1. Điểm xuất phát

Trước slice này, **không có một dòng JWT, JWKS hay mTLS nào** trong `src/` hay `tests/`. Xác thực service-to-service là **so chuỗi bí mật tĩnh**: `ORDER_CORE_SERVICE_TOKEN` cho Sales→IVR, `IVR_INTERNAL_SERVICE_TOKEN` cho nội bộ.

Phần cài đặt đã đúng — so sánh hằng thời gian, token in log là `[REDACTED_SERVICE_TOKEN]`. Vấn đề là **cơ chế**: chuỗi tĩnh không có hạn dùng, không xoay được mà không downtime, không phân biệt được caller nào gọi, và không mang scope. Đây là finding đã ghi trong [T-07](../../contracts/target-v1-closure-pack/T-07-production-auth.md).

## 2. Ba quyết định thiết kế

**(a) Thư viện, không phải handler ASP.NET.** Dùng `Microsoft.IdentityModel.JsonWebTokens` chứ không `Microsoft.AspNetCore.Authentication.JwtBearer`. Lý do: validator thuộc về `Ivr.Infrastructure`, nơi nó **unit-test được** và nơi mock issuer sống; `Ivr.Api` chỉ giữ một adapter mỏng cạnh các handler viết tay sẵn có. Chữ ký và vòng đời token thì **không bao giờ tự viết** — đó là phần giao cho thư viện.

**(b) Khoá sinh theo process, KHÔNG theo nghĩa "deterministic keys" của prompt.** Đây là sai lệch có chủ ý và tôi ghi rõ: một private key commit vào repo là **một secret thật trong source**, sẽ đỏ cổng gitleaks, và chỉ cách một lần copy-paste là bị dùng lại ở nơi có thật. Prompt cần "deterministic for Compose/CI"; cái CI thực sự cần là **kết quả test tái lập được**, và điều đó đạt được bằng cách issuer và validator dùng chung một instance. Khoá công khai phát qua JWKS; khoá riêng không rời process.

**(c) Compat isolation khoá theo provider profile, không theo một cờ rời.** `ServiceIdentityCompatPolicy` cho phép bí mật tĩnh dưới `FAKE_TARGET_V1` (profile toàn bộ suite chạy) và `CURRENT_GOLDEN_HOUR_COMPAT` (nơi nó thực sự phục vụ), **từ chối dưới `TARGET_V1`**. Nghĩa là không cấu hình nào chạy được target path trên một bí mật tĩnh. Profile lạ hoặc rỗng cũng bị từ chối — không nhận diện được không phải lý do để dễ dãi.

## 3. Đã xây gì

| Thành phần | Vị trí | Vai trò |
| --- | --- | --- |
| `ServiceIdentityOptions` + validator | `src/Ivr.Infrastructure/Auth/` | cấu hình; **từ chối boot** khi `Mode=Real` hoặc khi bật mTLS mà chưa có profile đã ký |
| `MockOidcIssuer` | cùng nơi | phát token RS256, phơi JWKS công khai, mô phỏng sự cố key source |
| `ServiceJwtValidator` | cùng nơi | chữ ký, issuer, audience, exp/nbf, thuật toán, `sub`, scope |
| `ServiceIdentityCompatPolicy` | cùng nơi | bí mật tĩnh chỉ sống ở profile compat |
| `MockClientCredentialsTokenProvider` | cùng nơi | egress: cache, refresh trước hạn, **single-flight** |
| Ingress | `OrderCoreAllowlistMiddleware` | thử JWT trước; bí mật tĩnh chỉ là phương án lùi theo profile |

**Thuật toán là allowlist đúng một phần tử: `RS256`.** Đó là điểm chặn cặp bypass kinh điển của JWT — `alg: none` và token HS256 ký bằng chính public key.

**Fail-closed khi nhà cung cấp danh tính chết.** Key source không trả lời → **từ chối**. "Phút trước nó verify được" không phải bằng chứng về request này. `UT-AUTH-JWKS-06` khẳng định điều đó, và khẳng định luôn là hệ thống tự phục hồi khi key source sống lại — không cần restart.

**Token đã verify mà không có `sub` cũng bị từ chối.** Caller không quy trách nhiệm được thì không audit được; chữ ký hợp lệ là chưa đủ.

## 4. Một lỗi trong chính code tôi vừa viết

Bản đầu, `ServiceJwtValidator` nhận `TimeProvider` nhưng **không dùng** — analyzer báo `CS9113: Parameter 'timeProvider' is unread`, và tôi xử lý bằng cách **xoá tham số**. Sai. Tham số không được đọc vì tôi quên nối nó vào kiểm tra vòng đời — nghĩa là exp/nbf đang được kiểm theo **đồng hồ máy**, không theo đồng hồ tiêm vào.

Test bắt ngay: 6/11 test auth đỏ, gồm cả case token hợp lệ, vì token mint ở giờ test còn vòng đời kiểm ở giờ thật.

Sửa đúng hướng: `ValidateLifetime = false` ở tầng thư viện, rồi tự kiểm `ValidFrom`/`ValidTo` với clock skew **theo `TimeProvider` tiêm vào**. Vừa xác định được, vừa cho ra đúng `Expired` / `NotYetValid` thay vì một lỗi vòng đời chung chung.

Bài học đáng ghi: cảnh báo "tham số không dùng" đôi khi không phải mời bạn xoá tham số — mà là báo bạn quên nối nó.

## 5. Test — phủ đủ 9 nhóm `P4-4` §3

| Nhóm §3 | Test |
| --- | --- |
| valid token | `UT-AUTH-JWT-01` |
| wrong issuer/audience/scope/signature | `UT-AUTH-JWT-02` (4 case, mỗi case một failure riêng) |
| expired / not-yet-valid | `UT-AUTH-JWT-03` |
| alg/kid (bypass cổ điển) | `UT-AUTH-JWT-04` (`alg:none` + 4 dạng malformed) |
| service identity | `UT-AUTH-JWT-05` (verify được nhưng không có `sub`) |
| auth outage | `UT-AUTH-JWKS-06` (fail-closed + tự phục hồi) |
| JWKS rotation/cache | `UT-AUTH-JWKS-07` |
| refresh race | `UT-AUTH-EGRESS-08` (32 caller đồng thời → **1** lần lấy token) |
| token/log secret scan | `UT-AUTH-SECRET-09` |
| optional mTLS profile validation | `UT-AUTH-MTLS-10` |
| compat isolation | `UT-AUTH-COMPAT-11` |
| ingress thật | `IT-AUTH-INGRESS-12` |

`UT-AUTH-EGRESS-08` đáng nói: 32 caller cùng lúc phải sinh **một** lần lấy token, không phải 32. Nếu không, chính cơn bão retry của mình sẽ bị issuer thật rate-limit đúng vào lúc token cũ vừa hết hạn — thời điểm tệ nhất có thể.

`UT-AUTH-SECRET-09` kiểm ba mặt: `ToString()` của token, lý do từ chối (không chứa mảnh nào của token), và JWKS phát ra (chỉ nửa công khai — không `d`, `p`, `q`).

`IT-AUTH-INGRESS-12` chứng minh validator **thực sự nằm trên request path**: token hợp lệ vào được `POST /tasks`; token đúng hình dạng đúng claim nhưng **sai người ký** bị chặn; caller verify được nhưng thiếu scope cũng bị chặn.

## 6. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln -warnaserror` | 0 warning / 0 error |
| `dotnet test Ivr.sln` | xem §8 |
| `dotnet list package --vulnerable --include-transitive` | **0 vulnerable** trên cả 9 project |
| `impact` (GitNexus) | `OrderCoreAllowlistMiddleware` LOW, 6 symbol, 1 direct |
| `docs-selftest.mjs` / `openapi-contract-drift.mjs` | PASS (không đụng OpenAPI) |

Thêm đúng **một** package: `Microsoft.IdentityModel.JsonWebTokens 8.22.0`, đặt trên `Ivr.Infrastructure`. Lockfile đã sinh lại bằng `dotnet restore --force-evaluate`.

## 7. Cái này KHÔNG chứng minh

- **Không đóng `W-0006` hay `OD-V1-07`.** Production auth profile vẫn `BLOCKED_EXTERNAL`.
- **Không suy ra production từ mock.** `ServiceIdentityOptionsValidator` **từ chối boot** khi `Mode=Real` — không deployment nào lặng lẽ tuyên bố đã có auth thật.
- **mTLS chưa được duyệt.** Bật mà không có profile đã ký → refuse ở startup.
- **Chưa có ngày tắt `X-Internal-Token`.** IVR không đặt được một mình vì credential này dùng chung với Sales; đó là closure artifact của [T-07](../../contracts/target-v1-closure-pack/T-07-production-auth.md).
- **`X-Source-System` vẫn chỉ là metadata**, không bao giờ là xác thực (P4-4 §4).
- **Admin RBAC không đụng tới.** `DF-01` do Permission Core sở hữu và tách khỏi service identity — slice này không lấn sang.
- **`TESTS_PASS` là trần.** Chỉ reviewer/owner chuyển `ACCEPTED`.

## 8. Số liệu test

`dotnet test Ivr.sln` — **336/336 pass, 0 fail, 0 skip**:

| Project | Sau W-0031 | Sau W-0032 | Thêm |
| --- | ---: | ---: | ---: |
| `Ivr.ContractTests` | 21 | 21 | 0 |
| `Ivr.UnitTests` | 178 | 189 | +11 |
| `Ivr.IntegrationTests` | 125 | 126 | +1 |
| **Tổng** | **324** | **336** | **+12** |

Không test cũ nào bị sửa hay nới. 125 test integration sẵn có đi qua middleware đã đổi và vẫn xanh — đó là bằng chứng lối compat còn nguyên vẹn dưới profile mock trong khi JWT được thêm vào phía trước nó.
