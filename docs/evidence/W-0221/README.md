# W-0221 — Một luật cho `Idempotency-Key`, ở một chỗ

Ngày: 2026-09-07 · Baseline: `main@6993e3f` · Trạng thái: **TESTS_PASS**.

Quyết định của owner: **siết admin theo intake**. `W-0209` tìm ra chỗ lệch và đặt câu hỏi; đây là
phần thi hành.

## 1. Cảnh báo phải nói trước

`gitnexus impact RequireIdempotencyKey` → **HIGH**, **17 call site trực tiếp**, 2 execution flow
(`MutationReplayFilter.InvokeAsync`, `FeatureFlagEndpoint.ExecuteMutationAsync`), 3 module.

Đây không phải blast radius ngoài ý muốn — nó **chính là** phạm vi của quyết định: siết một guard
dùng chung nghĩa là mọi endpoint mutation admin/internal nay từ chối một lớp key mà trước đây nhận.

Kiểm trước khi sửa:

- **Không test nào** trong repo phụ thuộc hành vi lỏng cũ.
- **Mọi key đang được dùng** — `idem-<guid>`, `flag-test-<guid>`, `matrix-<id>`, `elig-<taskId>` —
  đều nằm trong bảng chữ cái, nên không có caller nội bộ nào gãy.
- Repo **không nhìn thấy** caller ngoài. Nhưng BFF của M3 **chưa tồn tại** và admin UI đã ra khỏi
  phạm vi module, nên đây là thời điểm rẻ nhất để siết. Muộn hơn thì đắt hơn.

## 2. Vì sao gộp chứ không thêm bản sao thứ tư

Cùng một predicate sống ở **ba** chỗ trong `src/Ivr.Api/`:

| Nơi | Dài | `PiiGuard` | Bảng chữ cái |
| --- | --- | --- | --- |
| `TaskIntakeEndpoint.RequiredHeader` | `1-128` | có | **có** |
| `CorrelationMiddleware.IsValid` | `1-128` | có | **có** |
| `InternalServiceOptions.RequireIdempotencyKey` | `≤128` | có | **không** |

Hai đồng ý, một lệch. Cách sửa tối thiểu là chép điều kiện bảng chữ cái vào chỗ thứ ba — nhưng **ba
bản sao chính là cách chúng trôi khỏi nhau lần đầu**. Một luật viết ra một lần thì không thể tự mâu
thuẫn với chính nó.

Nên luật chuyển vào `src/Ivr.Api/Foundation/TraceHeaderSyntax.cs`, và cả ba nơi gọi nó.

Ghi luôn trong file **vì sao** bảng chữ cái hẹp hơn HTTP cho phép: những giá trị này được ghi vào
audit row, log scope, idempotency key, và **được trả ngược lại cho caller**. Giới hạn ở ký tự không
cần escape ở bất kỳ chỗ nào trong số đó là thứ làm việc trả ngược an toàn.

## 3. Ghim — `IT-API-IDEMP-04`

`tests/Ivr.IntegrationTests/InternalAdminApiTests.cs`, gọi HTTP thật vào một route admin:

| Gửi | Kết quả |
| --- | --- |
| `sB3+xQ/9dGVzdA==` | `400` + `IVR_MALFORMED_REQUEST` |
| `129` ký tự | `400` + `IVR_MALFORMED_REQUEST` |
| `idem-<guid>` | `200 OK` |

Case thứ ba có chủ đích: chứng minh việc siết **không** làm hỏng key mà mọi caller đang thật sự gửi.

Test đặt ở route admin chứ không phải ở `TraceHeaderSyntax`, vì câu hỏi là *"việc siết có tới được
cửa admin không"*, không phải *"predicate có đúng không"*.

## 4. OpenAPI: nay mô tả được, nhưng là một lượt phát hành

`CorrelationId` và `IdempotencyKey` vẫn là `{ type: string }` trần. Trước `W-0221` thì **không thể**
sửa cho đúng: hai route hai luật, một `$ref`, viết kiểu gì cũng sai với một bên. Giờ thì mô tả được.

Nhưng sửa nó là **phát hành contract**, không phải sửa code:

| | |
| --- | --- |
| siết một header bắt buộc | **breaking** theo oasdiff |
| kéo theo | bump draft · sinh lại client · changelog |
| re-pin hash OAS | **5 nơi**: `contract-manifest.json`, `dial-token-production-bundle-validator.mjs`, `docs/api/portal-manifest.json`, `docs/contracts/openapi-contract-diff.md`, `docs/contracts/target-v1-field-inventory.md` |

Và cùng file OAS còn đang chờ sửa `dial_token_expires_at` khi `2.1` chốt số TTL. **Gộp hai thứ vào
một lần phát hành rẻ hơn hai lần re-pin cho hai quyết định độc lập**, nên hoãn — với lý do mới, chứ
không phải lý do cũ. Lý do cũ (*chưa quyết siết hay nới*) đã hết hôm nay.

## 5. Kiểm chứng

```text
gitnexus impact RequireIdempotencyKey   HIGH · 17 direct · 2 process · 3 module
dotnet test --filter IT-API-IDEMP-04    1/1 PASS
dotnet test Ivr.sln                     900/900 PASS, 0 failed, 0 skipped
gate-status.mjs                         GATE_STATUS_PASS — 219 work items
contract-freeze-verifier.mjs            CONTRACT_FREEZE=PASS
generate-test-traceability.mjs --check  TEST_TRACEABILITY_CURRENT=557
docs-selftest.mjs                       API_DOCS_SELFTEST_PASS
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1=DRAFT`.

## 6. Còn lại

- **Lượt phát hành OpenAPI**, gộp với `2.1` (TTL). Xem §4.
- IR-06 `§3.1.1` đã ghi quyết định. Với M3 thì **API A không đổi gì** — `1-128` và bảng chữ cái vốn
  đã là luật của route intake; chỉ route admin/internal đổi, tức phần BFF M3 sẽ gọi sau này.
