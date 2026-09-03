# M8-10 — Contact / dial-token production-path decision pack và handoff

Ngày: `2026-09-03`

IVR baseline: `main@b21ec676e490`

Cross-repo snapshot chỉ để đối chiếu, không phải shared contract đã ký:
`C:\Projects\ginsengfood-business-platform` · branch `PhucApu` ·
`a3aad246d986fbc273cf41aaa93eec6659669656`.

Trạng thái: **`EVIDENCE_SUBMITTED / LOCAL_PRIVACY_SEAM_PRESENT /
PRODUCTION_PATH_FAIL_CLOSED / CONTRACT_RUNTIME_MISMATCH_FOUND /
M3_CONTACT_PRODUCER_NOT_FOUND / EXTERNAL_DECISIONS_REQUIRED / CODE_NOT_AUTHORIZED`**

Người lập: **Codex — audit/handoff draft**. Tài liệu này không ký thay Project Owner, Module 3,
Security, Platform, Telephony/vendor, Privacy/Legal hoặc Release owner.

## 1. Kết luận audit

`B5 + C12` phải được xử lý như một production trust boundary duy nhất, không phải một ticket
"viết resolver" và một ticket "viết adapter" tách rời.

1. **Local seam có và fail-closed:** intake chặn raw phone, token hết hạn, token không phủ hết
   confirmation window và lỗi protection; DB chỉ nhận giá trị `enc:`. MOCK/LAB dùng fingerprint
   không đảo ngược và destination allowlist/pinned alias.
2. **Production path chưa tồn tại:** nhánh ngoài MOCK/LAB đăng ký
   `UnavailableSchedulerDispatchGateway`; non-MOCK foundation dùng
   `UnavailableOpaqueValueProtector`; không có production `IDialTokenResolver`, `ISimGateway`,
   resolver endpoint, credential mount hoặc egress destination.
3. **Shared contact contract đang lệch runtime:** OpenAPI có `phone_validation_status` nhưng không
   đánh dấu required và không khóa enum; runtime chỉ nhận đúng chuỗi `VALID`. Một payload hợp lệ
   theo OpenAPI nhưng bỏ field này sẽ bị runtime trả `422`.
4. **TTL current bị ép thành equality:** intake đòi token expiry không sớm hơn window end, trong
   khi persistence đòi expiry không muộn hơn window end. Task accepted và persisted vì vậy chỉ
   hợp lệ khi `dial_token_expires_at == confirmation_window_expires_at`.
5. **One-use chưa có contract production:** task chỉ mang một scalar token, không có refresh/reissue,
   nhưng có thể có nhiều customer attempt và retry kỹ thuật. MOCK/LAB chỉ ngăn resolve lặp lại với cùng
   `(fingerprint, attempt_id)`; cùng token vẫn resolve được cho attempt khác.
6. **Boundary current chỉ cho destination opaque:** `DialAuthorization` từ chối giá trị giống raw
   phone. Vì vậy mô tả "IVR resolver trả E.164 cho gateway" không khớp type contract hiện tại.
   Raw E.164 phải nằm sau một boundary ngoài application/domain/storage/log của IVR.
7. **M3 producer/issuer chưa thấy:** exact search trong snapshot M3 nêu trên không tìm thấy
   `dial_token`, `phone_ref`, `phone_validation_status` hoặc `OfficialContactResolver`. Nếu artifact
   nằm ở repo/service khác, M3 phải cung cấp owner, SHA và contract thay vì IVR suy đoán.

Verdict đúng là:

**`LOCAL_SEAM_VERIFIED / PRODUCTION_RESOLVER_ADAPTER_NOT_IMPLEMENTED /
SHARED_CONTRACT_NOT_SIGNED / REAL_CUSTOMER_CALL_ALLOWED=NO`**.

## 2. Bằng chứng current

| Câu hỏi | Bằng chứng current | Kết luận |
| --- | --- | --- |
| Wire contact/token có gì? | `ivr-order-confirmation.v1.yaml:1141-1163,1213-1217` | `phone_ref`, `phone_masked`, token + expiry required; `phone_validation_status` optional string |
| Runtime contact gate là gì? | `TaskIntakeService.cs:389-438` | Chỉ `VALID`; masked marker bắt buộc; token phải còn hạn, phủ hết window và không giống raw phone |
| Token được lưu ra sao? | `TaskIntakeService.cs:258-270,717-722`; `PersistenceInvariantValidator.cs:120-128` | Protect trước persistence; lỗi protector chặn task; DB/model chỉ nhận ciphertext `enc:` |
| TTL effective là gì? | intake `TaskIntakeService.cs:411-414`; persistence `PersistenceInvariantValidator.cs:120-123` | Kết hợp thành expiry bằng đúng window end |
| Task có refresh/reissue không? | Target V1 chỉ có scalar `dial_token`; exact search route/schema không có refresh/reissue | Không thể thay token trên task immutable sau intake |
| Resolver trả gì? | `ProviderPorts.cs:16-45` | `DialAuthorization` chứa provider destination reference opaque; guard cấm raw phone |
| MOCK reuse thế nào? | `MockDialTokenVault.cs:64-103` | Hết hạn/unknown/not-allowlisted bị chặn; duplicate cùng attempt bị chặn; khác attempt được reuse |
| LAB resolve thế nào? | `LabDialTokenVault.cs:10-67` | Fingerprint local, mỗi eligible token → một alias pinned; không map số khách thật |
| API và Worker có dùng chung in-memory map không? | `MockDialTokenVault.cs:76-86` | Không; code tự ghi rõ hai deployable tách process, wildcard chỉ là fake fallback |
| Production DI có gì? | `SchedulerCapacity.cs:480-537`; `ServiceCollectionExtensions.cs:161-169` | Chỉ MOCK/LAB có vault/resolver/gateway; production fail-closed |
| Production mode/call gate có mở không? | `IvrOptionsValidator.cs:49-52,72-90`; Helm prod `executionMode: MOCK`, `realCustomerCallAllowed: false` | `PRODUCTION_REAL` chưa có runnable path; real calls bị boot/config gate chặn |
| Secret/egress có sẵn không? | `deploy/secrets/external-secrets.yaml:72-95`; `values.yaml:230-234` | Chỉ có ExternalSecret template chưa được workload dùng; external egress rỗng |
| Resolution audit có không? | exact search source không có `DIAL_TOKEN_RESOLVED`; `SIM_CALL_STARTED` bắt đầu sau resolve/dial | Không đủ audit để phân biệt resolved/expired/replay/denied/outage |
| Retention có gì? | `RetentionTargetCatalog.cs:27-31`; `PersonalDataInventory.cs:118-121` | Ciphertext được redact, nhưng thời hạn production/one-use wording chưa có owner approval |
| M3 có producer/issuer current không? | snapshot M3 exact search các field/resolver đều `0` | `M3_CONTACT_PRODUCER_NOT_FOUND`, không suy thành artifact không tồn tại ở toàn hệ |

## 3. M8 position đề xuất để owner phê duyệt hoặc sửa

Đây là **đề xuất**, chưa phải contract production:

- M3/Official Contact service là authority chọn contact và phát `phone_ref`, masked value,
  validation status cùng dial authorization; IVR không chọn lại contact.
- `phone_validation_status` nên required và enum duy nhất `VALID` cho task được gửi. Invalid hoặc
  inconclusive không nên tạo task; IVR vẫn giữ fail-closed defense-in-depth.
- Giữ task không chứa E.164. IVR application/domain/storage/log chỉ thấy opaque token/ciphertext và
  một opaque provider destination handle. Việc biến handle thành E.164 nằm trong external
  vault/gateway boundary được Security và vendor duyệt.
- Nếu giữ scalar token hiện tại, token phải reusable trong TTL nhưng authorization/replay phải bind
  theo `task_id + attempt_id + provider/audience`, idempotent với cùng attempt và từ chối cross-task,
  cross-provider hoặc quá hạn. Nếu Security đòi globally one-use, phải ký wire/reissue contract mới;
  không thể giả vờ scalar hiện tại đáp ứng.
- Token phải usable xuyên hết confirmation window. Current code ép exact equality; owner cần ký exact
  equality hoặc một invariant mới có min/max rõ ràng trước khi sửa OpenAPI/persistence.
- API và Worker là hai deployable; production custody/resolution phải dùng shared durable external
  service, không dùng process-local map hay reversible key trong IVR.
- Resolver/gateway unavailable, expired, revoked, replayed, denied hoặc auth failure đều fail-closed.
  Technical failure không được tính customer attempt; retry phải bounded bởi deadline và attempt ID.
- Ghi audit outcome/code/timing/issuer/key-version/audience/attempt/provider correlation, không ghi
  raw token, ciphertext, destination handle hay E.164. Vault/vendor giữ phần audit resolution có PII.

## 4. Decision matrix `DTK-01..DTK-15`

| ID | Quyết định bắt buộc | Đề xuất M8 | Owner phải ký | Artifact chấp nhận |
| --- | --- | --- | --- | --- |
| `DTK-01` | Contact authority và producer | M3/Official Contact chọn contact; IVR chỉ validate/execute | M3 + Contact/CRM owner + M8 | Producer code/OAS/schema, owner + exact SHA, positive/negative CDC |
| `DTK-02` | Contact field requiredness/taxonomy | `phone_validation_status` required enum `[VALID]`; invalid/inconclusive không phát task | M3 + M8 + Product | Signed field matrix, examples và compatibility plan |
| `DTK-03` | Issuer và token format | Một issuer có định danh/version; token opaque, không chứa/log PII | M3 + Security | Issuer spec, token claims/envelope, entropy/size limits và sample redacted |
| `DTK-04` | Subject/scope/audience binding | Bind task/contact/attempt policy/provider/environment; cấm cross-task/env/provider replay | Security + M3 + Telephony | Threat model + signed validation rules và negative tests |
| `DTK-05` | Scalar/reissue/bundle model | Ưu tiên scalar reusable theo TTL + per-attempt authorization nếu threat model chấp nhận; nếu không, đổi contract có version | M3 + Security + M8 + Product | Chọn a/b/c/d của OD-V1-17, OAS/schema + CDC + migration/cutover nếu đổi |
| `DTK-06` | TTL/time semantics | Token phải phủ toàn window; ký exact equality current hoặc min/max/clock-skew mới | M3 + Security + M8 | Issued/not-before/expires/clock-skew table + boundary tests |
| `DTK-07` | Resolver topology và output | Raw E.164 chỉ xuất hiện bên trong external vault/gateway; IVR nhận opaque provider handle | Security + Platform + Telephony + M8 | Duyệt sequence/data-flow diagram, data owner và deployment topology |
| `DTK-08` | Resolve protocol/auth | Versioned API/SDK, mTLS/JWT audience/scope, timeout, error taxonomy, idempotency | Security + Platform + issuer/vendor | Authoritative API spec, sandbox credential và conformance tests |
| `DTK-09` | Custody/key/credential | Không decryption/mapping key trong IVR; credential workload identity/secret store, least privilege | Security + Platform | KMS/Vault path, RBAC, mount/reference, access review và evidence cluster thật |
| `DTK-10` | Rotation/revocation | Issuer key ID/version, overlap hữu hạn, emergency revoke, credential rotation không downtime | Security + Platform + M3 | Runbook + executed rotation/revocation drills và audit refs |
| `DTK-11` | Replay/concurrency/one-use | Same attempt idempotent hoặc deterministic reject; different attempt theo model đã ký; atomic consumption | Security + M3 + Telephony | Race matrix + parallel replay tests phía vault/gateway |
| `DTK-12` | Failure/retry/refresh | Fail closed; resolver outage là technical failure không counted; bounded retry trước deadline; refresh chỉ khi có signed route | Product + M3 + M8 + Platform | Error/retry/timeout table, outage drill, alert/SLO và refresh contract nếu chọn |
| `DTK-13` | Audit/privacy/retention | Split audit IVR/vault/vendor bằng correlation; không log secret/PII; ký retention/DSAR | Security + Privacy/Legal + M8 + vendor | Audit schema/query, redaction tests, retention periods, purge proof và access policy |
| `DTK-14` | Telephony capability/safety | Vendor xác nhận opaque destination flow, disposition/DTMF/caller ID, recording OFF, allowlist/kill switch | Telephony/vendor + Legal + Platform | Vendor capability statement + 1-SIM lab pack, recording read-back và failure matrix |
| `DTK-15` | Rollout/rollback/release | Contract/custody/network first; dual-version nếu đổi wire; sandbox → allowlisted lab → pilot; kill switch mặc định | M3 + M8 + Platform + Release | Exact-SHA shared E2E, cutover/rollback drill, go/no-go và production config review |

Không owner nào được đóng thay owner khác. Ví dụ vendor xác nhận API nhận token không thay Security
threat model, và M3 cung cấp producer không thay Platform custody/network evidence.

## 5. State/error matrix bắt buộc

| Tình huống | Current local behavior | Behavior production phải ký |
| --- | --- | --- |
| `phone_validation_status` thiếu/khác `VALID` | runtime reject `422`, dù OpenAPI hiện cho phép thiếu | Requiredness, enum và producer negative CDC |
| token hết hạn lúc intake | reject | Issuer clock/skew và producer refresh/recreate rule |
| token hết trước window end | reject | Token phải phủ hết window hoặc contract retry/reissue khác |
| token expiry sau window end | persistence reject | Ký exact equality hoặc đổi invariant có max rõ |
| protection/custody unavailable tại intake | task blocked operational | Retry/alert/DLQ contract; không persist plaintext |
| resolve token không biết/revoked/denied | dispatch technical failure | Error taxonomy, counted=false, retry/terminal rule |
| resolve cùng attempt lần hai | local resolver reject | Chọn idempotent same authorization hay deterministic replay error |
| cùng token ở attempt khác | local resolver cho phép | Chọn reusable/per-attempt/bundle/reissue, không để mock quyết định production |
| resolver timeout/outage | dispatch fail path | Timeout budget, retry/backoff, breaker, deadline và alert |
| resolve xong nhưng dial response mất | chưa có shared resolution receipt contract | Idempotency key/receipt để retry không tạo cuộc gọi thứ hai |
| token hết hạn giữa các attempt | không có refresh path | Close task, reissue hay new task; authority và audit phải rõ |
| key/credential rotation giữa attempt | chưa có production path | Key-version overlap, revoke and recovery drill |
| worker/vault concurrent resolve | chỉ process-local test | Atomic cross-process replay/consumption proof |

## 6. Artifact bắt buộc trước khi mở code production

1. `DTK-01..DTK-15` có signer, authority, ngày, scope và approval reference.
2. Authoritative contact/task schema của M3, issuer/resolve API hoặc vendor SDK, code/contract SHA.
3. Security threat model + data-flow/trust-boundary diagram, gồm nơi duy nhất E.164 được lộ.
4. Platform custody/network design: workload identity/secret mount, KMS/Vault, egress/ingress/TLS,
   rotation và revocation.
5. Telephony capability statement, disposition/error matrix, caller ID, recording OFF proof.
6. Privacy retention/DSAR periods cho contact refs, token ciphertext, resolve receipt và audit.
7. Sandbox credentials và test fixtures không chứa số khách thật.
8. Shared CDC/E2E matrix: valid, missing validation status, expired/before-window/after-window,
   cross-task/provider replay, same/different attempt, outage, rotation, lost response và rollback.

Thiếu bất kỳ nhóm artifact chịu trách nhiệm nào: **`CODE_NOT_AUTHORIZED`**.

## 7. Kế hoạch implementation sau khi được ký

1. Freeze contract/SHA, threat model, error matrix, migration/cutover và rollback.
2. Impact-analyze trước khi sửa các symbol intake, contract mapper/domain token types, persistence,
   scheduler/dispatch gateway, DI/options, retention/audit và Helm config.
3. Viết failing contract/unit/integration/concurrency tests theo semantics đã ký.
4. Implement production protector/custody client và resolver client theo interface; không đưa raw
   E.164 vào IVR domain/model/log.
5. Implement vendor adapter/gateway và signed idempotency/error mapping; giữ recording OFF,
   allowlist/kill switch.
6. Thêm audit resolution receipt redacted, metrics/alerts và retention policy.
7. Chạy local tests, target DB, sandbox CDC, rotation/outage/replay drills, 1-SIM allowlisted lab và
   shared exact-SHA E2E.
8. Chỉ Release owner mới xem xét `PRODUCTION_REAL`; `REAL_CUSTOMER_CALL_ALLOWED` vẫn `NO` cho tới
   go/no-go riêng.

## 8. Approval record

| Role | Signer / authority / date / approval reference | Trạng thái |
| --- | --- | --- |
| Module 8 / Project Owner | Chưa cung cấp provenance độc lập cho `DTK-01..15` | `NOT_RECEIVED` |
| Module 3 / Contact producer / issuer | Chưa cung cấp | `NOT_RECEIVED` |
| Security | Chưa cung cấp | `NOT_RECEIVED` |
| Platform | Chưa cung cấp | `NOT_RECEIVED` |
| Telephony / vendor | Chưa cung cấp | `NOT_RECEIVED` |
| Product | Chưa cung cấp | `NOT_RECEIVED` |
| Privacy / Legal | Chưa cung cấp | `NOT_RECEIVED` |
| Release owner | Chưa cung cấp | `NOT_RECEIVED` |

## 9. Stop rule

- Không sửa OpenAPI/contact requiredness, token model, TTL invariant, resolver/protector, adapter,
  persistence, audit/retention hoặc production config trước signed decision/artifact.
- Không dùng MOCK/LAB fingerprint, wildcard hoặc alias `LAB-A` làm production design.
- Không đưa raw E.164 vào IVR để "nối tạm" vendor API.
- Không áp dụng ExternalSecret template, mở egress hoặc bật `PRODUCTION_REAL`/real call trong W-0150.
- Không gọi local green tests là production proof.

`W-0150` đóng ở mức **audit/evidence/decision handoff**, không phải runtime completion.
