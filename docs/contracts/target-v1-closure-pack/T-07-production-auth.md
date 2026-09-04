# T-07 — Production service auth: JWT issuer/audience/scope/TTL/JWKS + mTLS

External work `W-0006` · quyết định `OD-V1-07` · gate **real integration** · trạng thái `OPEN`

Owner: **Security/Platform**.

Due: chốt **trước khi bắt đầu `P4-4`** — đây là ticket chặn nhiều thứ nhất trong gói. Ngày cam kết của owner: `<owner điền>`.

## 1. Current evidence — đã đọc từ nguồn

**Trong toàn bộ `src/` và `tests/` không có một dòng JWT, JWKS hay mTLS nào.** Đã grep `Jwks`, `JwtBearer`, `JsonWebKey`, `IssuerSigningKey`, `ClientCertificate` → 0 kết quả.

Xác thực service-to-service hiện tại là **so chuỗi bí mật tĩnh**:

| Hướng | Cơ chế | Vị trí |
| --- | --- | --- |
| Sales → IVR (`POST /tasks`) | biến `ORDER_CORE_SERVICE_TOKEN`, so sánh hằng thời gian | [`src/Ivr.Api/Auth/OrderCoreAllowlistMiddleware.cs:46`](../../../src/Ivr.Api/Auth/OrderCoreAllowlistMiddleware.cs) |
| IVR worker/adapter → IVR internal | biến `IVR_INTERNAL_SERVICE_TOKEN` | [`src/Ivr.Api/Internal/InternalServiceOptions.cs:16`](../../../src/Ivr.Api/Internal/InternalServiceOptions.cs) |
| Admin console → IVR | bearer token ba tier (read/write/danger), scheme `IvrAdminServiceToken` | [`src/Ivr.Api/Auth/AdminTokenAuthenticationHandler.cs:29`](../../../src/Ivr.Api/Auth/AdminTokenAuthenticationHandler.cs) |
| IVR → Sales (callback) | port `IServiceTokenProvider`, chưa có implementation production | [`src/Ivr.Domain/Ports/ProviderPorts.cs:261`](../../../src/Ivr.Domain/Ports/ProviderPorts.cs) |

Phần đã làm đúng: so sánh token dùng `CryptographicOperations.FixedTimeEquals`, và token in ra log là `[REDACTED_SERVICE_TOKEN]`. **Vấn đề không phải chất lượng cài đặt — vấn đề là cơ chế.** Chuỗi tĩnh không có hạn dùng, không xoay được mà không downtime, không phân biệt được caller nào, và không mang scope.

**Contract đã chừa chỗ, và chừa một cách trung thực.**

Outbound callback [`order-core-ivr-callback.target-v1.yaml:101`](../../../specs/api/openapi/order-core-ivr-callback.target-v1.yaml):

```yaml
serviceJwt:
  type: oauth2
  flows:
    clientCredentials:
      tokenUrl: https://identity.invalid/oauth2/token
      scopes:
        ivr.result.write: Submit IVR result signals
```

`identity.invalid` là TLD dành riêng — placeholder cố ý, để không ai nhầm là đã cấu hình.

Internal API [`ivr-order-confirmation.v1.yaml:652`](../../../specs/api/openapi/ivr-order-confirmation.v1.yaml) mới chỉ có `bearerAuth: { type: http, scheme: bearer }`.

**Bốn scope ingress đã được đề xuất, chưa được duyệt** — [`specs/api/01-conventions.md:21`](../../../specs/api/01-conventions.md):

| Scope | Cho ai |
| --- | --- |
| `ivr.task.write` | Sales → `POST /tasks` |
| `ivr.internal.write` | IVR worker/adapter → internal lifecycle |
| `ivr.admin.read` | console đọc |
| `ivr.admin.write` | console ghi |

Cùng file ghi rõ một hạn chế kỹ thuật: `bearerAuth` kiểu `http/bearer` **không thể mang scope**; muốn gắn scope per-operation thì phải chuyển sang `oauth2`/`clientCredentials` hoặc công bố claim mang scope.

## 2. Target delta — chính xác là gì

**(a) Chưa có issuer.** Cần: URL issuer, JWKS endpoint, thuật toán ký, chu kỳ xoay khoá, audience đặt là gì cho IVR, TTL token.

**(b) Chưa có sandbox credential.** Không có credential nghĩa là **không thể chạy một test tích hợp thật nào**. Đây là lý do `P4-1` và `P4-4` chỉ đạt được `TESTS_PASS` mock-only chứ không hơn.

**(c) Chưa có quyết định mTLS.** Có/không, ai cấp cert, xoay thế nào, cert bị thu hồi thì hành vi ra sao. `specs/api/01-conventions.md:19` ghi "mTLS is pending owner decision (`OD-V1-07`)".

**(d) Chưa có lộ trình khai tử `X-Internal-Token`.** Tài liệu ghi nó "chỉ current compatibility". Cần ngày tắt cụ thể, nếu không nó sẽ sống mãi.

**(e) `bearerAuth` phải đổi kiểu nếu muốn scope.** Đây là thay đổi contract, không phải cấu hình. Cần nằm trong quyết định `OD-V1-07` để `P4-4` làm một lần.

**(f) Admin identity là chuyện khác, đừng gộp.** `MockPermissionAuthenticationHandler` phục vụ RBAC admin (`DF-01`), do **Permission Core** sở hữu, không phải Security/Platform. Ticket này chỉ nói về **service identity**. Gộp hai thứ sẽ tạo ra một hệ thống mà console và service dùng chung đường xác thực — đúng thứ `DF-01` cấm.

## 3. Sample payload

Token IVR mong đợi nhận từ Sales (claim tối thiểu):

```json
{
  "iss": "<Security điền>",
  "aud": "ivr-order-confirmation",
  "sub": "sales-platform",
  "scope": "ivr.task.write",
  "exp": 1755500000,
  "iat": 1755499100
}
```

Token IVR mong đợi **phát ra** khi gọi callback:

```json
{
  "iss": "<Security điền>",
  "aud": "<Sales điền>",
  "sub": "ivr-order-confirmation",
  "scope": "ivr.result.write",
  "exp": 1755500000
}
```

## 4. Acceptance test — phải xanh khi đóng

| Test | Ở đâu | Khẳng định |
| --- | --- | --- |
| `IT-API-AUTHZ-01/02` | `tests/Ivr.IntegrationTests/` | Caller không hợp lệ → `403 IVR_FORBIDDEN_CALLER` |
| `IT-API-AUDIT-04` | cùng nơi | Mọi hành động admin có audit |
| `IT-FND-ERR-12` | cùng nơi | Lỗi auth dùng envelope đã che, có correlation ID |
| **`UT-AUTH-JWT-*`** *(P4-4 sẽ viết)* | mock issuer | issuer/audience/scope/chữ ký sai; hết hạn; chưa hiệu lực; JWKS xoay; refresh race; issuer chết → fail-closed |
| **`CDC-AUTH-01`** *(Security cấp điều kiện)* | sandbox thật | Token sandbox thật gọi được `POST /tasks`; token sai scope bị từ chối |

Ba dòng đầu **đã xanh** trên cơ chế hiện tại. Hai dòng cuối chưa chạy được — dòng cuối cần credential ở mục §6.

## 5. Mock fallback

Chuỗi bí mật tĩnh + `X-Permissions` header, đủ để toàn bộ Phase 0–3 chạy và để console kiểm RBAC 3 vai trò. `P4-4` sẽ dựng mock OIDC issuer với khoá tất định cho Compose/CI. **Mock issuer không bao giờ là bằng chứng production** — ghi rõ ở `specs/api/01-conventions.md:21` và ở DoD của `P4-4`.

## 6. Closure artifact — owner điền

- [ ] **Auth profile đã ký**: issuer URL, JWKS URL, audience của IVR, scope set, TTL, thuật toán, chu kỳ xoay khoá.
- [ ] **Sandbox credential** + hướng dẫn lấy token, để chạy được `CDC-AUTH-01`.
- [ ] **Quyết định mTLS**: có/không; nếu có thì ai cấp cert và xoay ra sao.
- [ ] **Ngày tắt `X-Internal-Token`**.
- [ ] **Chốt kiểu security scheme** cho internal API: giữ `http/bearer` (không scope) hay chuyển `oauth2` (có scope).

## 7. Rủi ro nếu để mở

Ticket này chặn nhiều thứ nhất. `P4-1` cần token provider của `P4-4`; `P4-4` cần profile của ticket này. Không có sandbox credential thì **không một test tích hợp thật nào chạy được** — nghĩa là mọi thứ sau `P4` đều chỉ chứng minh được ở mức mock, bất kể viết thêm bao nhiêu code.

Đồng thời, đây là ticket duy nhất mà **hiện trạng đang chạy được** — chuỗi bí mật tĩnh hoạt động. Đúng vì thế mà nó dễ bị hoãn: không có gì đỏ, cho tới ngày cần xoay khoá hoặc cần biết caller nào đã gọi.
