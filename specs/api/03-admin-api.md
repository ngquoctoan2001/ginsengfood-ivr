# API-03 — Admin API

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/11` §5,§8; `/08` (monitoring/privacy).
Base path `/v1/ivr/order-confirmation/*`. Mọi POST có `reason` + `X-Actor-Id` + audit + `Idempotency-Key`.

`W-0128`: IVR **không còn** phát hành hay lưu tài khoản. Không có `/auth/*`, không có `/accounts*`,
không có session và không có role. Module 3 sở hữu identity của nhân viên và gọi sang đây bằng
credential của service.

Uỷ quyền chạy theo **ba tầng**, không theo permission catalogue:

| Tầng | Credential | Header bắt buộc thêm |
| --- | --- | --- |
| `read` | `IVR_ADMIN_READ_TOKEN` | `X-Service-Scope: ivr.admin.read` |
| `write` | `IVR_ADMIN_WRITE_TOKEN` | `X-Service-Scope: ivr.admin.write` |
| `danger` | `IVR_ADMIN_DANGER_TOKEN` | `X-Service-Scope: ivr.admin.danger` + `X-Action-Reason` |

Tầng lồng nhau: `danger` ⊇ `write` ⊇ `read`. `X-Service-Scope` khai theo **tầng của token đang
cầm**, không phải tầng endpoint yêu cầu — khai theo endpoint sẽ vỡ tính lồng nhau và bị `403`.
`X-Actor-Id` bắt buộc trên mọi endpoint ở bảng dưới.

Mỗi tier có cặp rotation tùy chọn `*_PREVIOUS` + `*_PREVIOUS_RETIRES_AT`. Hai biến phải đi cùng
nhau; retirement là ISO-8601 instant tuyệt đối, không phải duration. Current/previous và mọi tier
phải dùng giá trị khác nhau, tối thiểu 24 ký tự. Runtime từ chối cấu hình thiếu, ngắn hoặc trùng để
không biến token `read` thành capability `danger`. Sau instant, previous bị từ chối dù biến còn tồn tại.

IVR không quyết định role người dùng nào được tier nào. Mapping role → tier thuộc Module 3 và hiện
`OWNER_DECISION_REQUIRED`; role lạ không nhận tier nào, còn `danger` phải grant tường minh.

Hợp đồng đầy đủ cho Module 3, kể cả các bẫy `403`, nằm ở
[`integration-requirements/06-module-3-api-handover.md`](../../integration-requirements/06-module-3-api-handover.md) §4A.

Chuỗi `IVR_SCRIPT_*` vẫn tồn tại nhưng **không còn là permission**: chúng là từ vựng trên dây của
header `X-Script-Permissions`, do Module 3 tự khai cho từng actor. Bốn mắt vẫn do IVR cưỡng chế
theo **danh tính** (`X-Actor-Id`), không theo quyền.

## 1. Endpoint & tầng
| Endpoint | Method | Tầng | Contract | Chức năng |
| --- | --- | --- | --- | --- |
| `/queue` | GET | `read` | Queue projection (masked) | Xem queue/capacity/incident |
| `/queue:pause` | POST | `danger` | `AdminMutationRequest` → `IvrAdminActionResult` | Pause queue (reason/evidence) |
| `/queue:resume` | POST | `danger` | `AdminMutationRequest` → `IvrAdminActionResult` | Resume sau khi incident resolved |
| `/sim-channels/{simChannelId}:disable` | POST | `danger` | `AdminMutationRequest` → `IvrAdminActionResult` | Disable SIM (health/failure reason) |
| `/sim-channels/{simChannelId}:enable` | POST | `danger` | `AdminMutationRequest` → `IvrAdminActionResult` | Enable SIM sau health pass |
| `/technical-retries` | POST | `danger` | `TechnicalRetryRequest` → `IvrTechnicalRetryResult` | Request technical retry (không tăng customer attempt) |
| `/admin-reviews` | POST | `write` | `AdminReviewRequest` → `IvrAdminReviewResult` | Ghi review/annotation |
| `/scripts/{templateId}/{version}` | GET | `read` | `IvrScriptVersionDetail` | Một phiên bản ở mọi trạng thái, gồm cả bản nháp |
| `/scripts` | POST | `write` | `IvrScriptDraftRequest` → `IvrScriptActionResult` | Tạo bản nháp; `/scripts/` vẫn là alias tương thích runtime; phiên bản là bất biến sau khi tạo |
| `/scripts/{templateId}/{version}:submit` | POST | `write` | `IvrScriptTransitionRequest` | Chuyển bản nháp sang chờ duyệt |
| `/scripts/{templateId}/{version}:approve` | POST | `write` + `X-Script-Permissions` chứa `IVR_SCRIPT_APPROVE_*` theo `approval_type` | `IvrScriptApprovalRequest` | Ghi một chữ ký duyệt |
| `/scripts/{templateId}/{version}:retire` | POST | `write` + `X-Script-Permissions` chứa `IVR_SCRIPT_RETIRE` | `IvrScriptTransitionRequest` | Thu hồi; fail-closed mọi chế độ, không xoá |
| `/call-jobs/{ivrCallJobId}:terminate` | POST | `danger` | `AdminMutationRequest` → `IvrAdminActionResult` | Cắt cuộc đang chạy; `409` nếu không có cuộc nào đang chạy |
| `/call-jobs:terminate-all` | POST | `danger` | `AdminMutationRequest` → `IvrAdminActionResult` | Cắt mọi cuộc đang chạy; hành động riêng, không gộp vào kill switch |
| `/dev/seed:load` | POST | `write` | `IvrSeedLoadRequest` → `IvrSeedLoadResult` | **Chỉ non-prod.** Production không đăng ký route ⇒ `404` |
| `/dev/scenarios/{scenarioId}:dry-run` | POST | `write` | `AdminMutationRequest` → `IvrScenarioDryRunResult` | **Chỉ non-prod.** Không phát cuộc gọi nào |
| `/dev/integration-profiles/{profileId}:apply` | POST | `write` | `AdminMutationRequest` → `IvrIntegrationProfileResult` | **Chỉ non-prod.** Chỉ `SIM_GATEWAY` được thi hành |

### Lối phát triển non-prod (W-0112) — vì sao `404` chứ không `403`

`403` trả lời một câu người gọi chưa hỏi: rằng ở địa chỉ này **có** một seed loader, và thứ duy
nhất chắn giữa họ với nó là một cái quyền. `404` không nói gì — và nó đúng theo nghĩa đen: ở
production ba route đó không được đăng ký.

Điều kiện phục vụ là **danh sách cho phép**, không phải danh sách cấm: tên môi trường phải nằm
trong `{Development, Testing, Test, Staging, Lab}`, `IVR_EXECUTION_MODE` phải là `MOCK` hoặc
`LAB_REAL_SIM`, và `REAL_CUSTOMER_CALL_ALLOWED` phải là `NO`. Mỗi điều kiện tự nó đủ để từ chối.
Quên cập nhật danh sách khi thêm môi trường mới ⇒ mất công cụ dev, không phải mở seed loader vào
một môi trường không ai kiểm.

Chốt được kiểm **hai lần**: một lần lúc đăng ký route, một lần trong service. Cái thứ hai phòng
đúng một tình huống — một thay đổi sau này thêm route hoặc caller mà quên chốt.

### Cắt ngang cuộc gọi (W-0111) — §2a được sửa lại

`§2a` trước đây ghi: *"Queue pause … chỉ chặn claim mới; active lease/call không bị cancel"*.
Câu đó vẫn đúng **cho queue pause**, nhưng nó từng là mô tả đầy đủ của hệ thống — không có cách
nào cắt một cuộc đang chạy. Giờ có, qua route riêng ở trên; queue pause vẫn **không** cắt.

Ba điểm ngữ nghĩa:

- Cuộc bị cắt ghi `IVR_TECHNICAL_EXCEPTION`, `customer_attempt_counted=false`. Khách chưa kịp
  trả lời, nên tiêu một lần gọi của họ cho quyết định của người vận hành là tính nhầm cho khách.
  Ràng buộc `ck_ivr_call_attempts_technical_not_counted` ở CSDL cũng ép điều này.
- Phím khách bấm sau khi người vận hành đã quyết định dừng **không** được ghi.
- Kênh SIM trả về `IDLE` và **không** bị đưa vào cooldown: cuộc bị cắt là do ta, không phải thiết
  bị hỏng, và phạt kênh sẽ lấy mất năng lực gọi như một tác dụng phụ của chốt an toàn.

`Ivr.Api` không có SIM gateway, nên endpoint **ghi yêu cầu**, không tự cắt. Worker đọc và cắt ở
lần kiểm tra kế tiếp (`TerminationPollMilliseconds`, mặc định `500 ms`, sàn cứng `200 ms`). Đây
là lý do phản hồi nói "đã yêu cầu" chứ không nói "đã cắt".

### Vòng đời kịch bản (W-0109) — mã lỗi phân biệt *ai* với *trạng thái*

`403` khi người gọi **sai người** cho chính phiên bản đó: là người tạo, hoặc là tài khoản đã ký
nửa còn lại của cặp production. Bấm lại không đổi được điều đó — cần người thứ hai.
`409` khi **trạng thái** từ chối: duyệt một bản nháp, thu hồi một bản nháp, hay trùng loại duyệt.

Trả `409` cho vế đầu sẽ đẩy người vận hành đi bấm lại, trong khi việc cần làm là đi tìm đồng nghiệp.

Bốn route mutation kịch bản chạy ở tầng `write`. Seam quyền MOCK (`X-Permissions`) đã bị gỡ cùng
`W-0128`; thứ thay nó là `X-Script-Permissions`, do Module 3 khai và IVR ghi nhận — nhưng bốn mắt
vẫn cưỡng chế theo `X-Actor-Id`, nên một actor tự khai đủ bảy quyền vẫn không ký được cả hai nửa của cặp duyệt production.

| Endpoint | Method | Tầng | Contract | Chức năng |
| --- | --- | --- | --- | --- |
| `/feature-flags/{environment}` | GET | `read` | `FeatureFlagReadResult` | Đọc fresh typed snapshot; provider lỗi trả fail-closed |
| `/feature-flags/{environment}/kill-switch` | GET | `read` | `KillSwitchVerification` | Xác minh revision và trạng thái kill switch effective |
| `/feature-flags/{environment}` | POST | `danger` + `Idempotency-Key` *(OD-V1-20 duyệt 2026-08-22)* | `FeatureFlagMutationRequest` | Mutation atomic, reason, idempotency, audit và four-eyes theo chiều rủi ro |

## 2. Ràng buộc admin action (P0)
Mỗi POST phải có: `X-Actor-Id`, tầng đủ mạnh cho endpoint, `reason`, `target_type`+`target_id`, audit record, evidence ref nếu ảnh hưởng queue/SIM/retry/result, `no_policy_bypass=true`.

`X-Actor-Id` là **nguồn**, không còn được đối chiếu với chủ thể phiên đăng nhập — `W-0128` xoá phiên đó và Module 3 khai actor theo từng request. IVR kiểm header có mặt, ≤ 128 ký tự, qua bộ lọc PII, rồi ghi thẳng vào audit. Mỗi mutation commit business state + `ivr_admin_actions` + append-only `ivr_audit_log` trong cùng transaction, gồm `before/after`, tên thao tác, correlation và `no_policy_bypass=true`.

Admin **KHÔNG** được:
- Gọi khách ngoài attempt policy (D-10) hoặc reset customer attempt count.
- **Force confirm/cancel order** (D-02: order state do Core; P0-IVR-002).
- Enable SIM khi health check đang fail.
- Resume queue khi capacity incident chưa xử lý.
- Bỏ qua blocker do-not-call — DC-01.

## 2a. Semantics đã khóa ở P2-8

- Queue pause tạo open hold incident và chỉ chặn **claim mới**; active lease/call không bị cancel. Resume chỉ resolve admin pause và fail-closed nếu còn non-admin hold incident.
- Disable channel đặt `enabled=false`; nếu đang active thì giữ nguyên active job/lease/fencing để call hiện tại hoàn tất. Enable trực tiếp channel `QUARANTINED`, `HEALTH_FAILED`, còn lease/active call, fail-count hoặc REAL adapter đều bị chặn cho đến reconciliation/cấu hình eSIM thật.
- Technical retry chỉ áp cho technical exception đã lưu, không counted, còn trong window, dưới bounded limit, chưa final, không blocker/queue hold. Không reset customer attempt. MOCK requeue về `HELD_MOCK`; real mode vẫn phải qua runtime/kill-switch/`REAL_CUSTOMER_CALL_ALLOWED`.
- Admin review chỉ resolve/annotate `ivr_review_items`; không sửa `ivr_call_results`, không fake result và không đổi order state.

## 3. Privacy (masked)
- `/queue`, `/call-jobs/{id}` chỉ hiển thị `phone_masked`, `order_code`, program, status, deadline. **Không** raw phone/full address/payment/health (phase-8/08; P0-IVR-007).

## 4. SIM/eSIM admin theo mode
- Dev dùng mock channels; lab ban đầu có 1 SIM thật và destination allowlist; production target 32 eSIM channels. Channel count là config. UI/API phải hiển thị mode/provider và không được bật real call permission chỉ vì channel được enable.

## Báo cáo (admin)
- OpenAPI có **31 operation admin**: 15 `read`, 5 `write`, 8 `danger` và 3 route dev `write` chỉ non-prod. Production không map ba route dev nên còn 28 operation runtime. Hai GET feature-flag không cần actor; 29 operation còn lại bắt buộc `X-Actor-Id`. Mutation feature flag nằm ở tầng `danger` (`OD-V1-20`, 2026-08-22), nhưng endpoint **vẫn fail-closed** ở tầng sau: `IRuntimeGateAuthorization` (bản production luôn `false`) trả `409 IVR_OPERATIONAL_BLOCKED`. Không endpoint nào cho phép force order/bypass blocker.

## Runtime-gate controls — bất đối xứng theo chiều an toàn

`OD-V1-20` (duyệt 2026-08-22, owner module IVR) đặt mutation runtime-gate vào tầng `danger`; chữ ký four-eyes của Security/Platform + Release owner vẫn còn thiếu. Tầng không chặn ai khi Module 3 cầm đúng token, nhưng `IRuntimeGateAuthorization` thì có — mọi mutation hiện trả `409` trước khi tới các quy tắc dưới đây. Khi lớp đó được mở, những quy tắc này **là** biện pháp kiểm soát, không phải lớp phụ:

- **Chiều giảm rủi ro luôn được phép** ở mọi environment: bật `globalDialKillSwitch`, thu hẹp/làm rỗng `labDestinationAllowlist`, đặt `realCustomerCallAllowed=false`. Chỉ cần token tầng `danger` + `X-Actor-Id` + `reason` + audit; **không** four-eyes, **không** chờ deployment. Một kill switch không bật được trong sự cố là kill switch hỏng.
- **Chiều tăng rủi ro luôn bị gate**: tắt kill switch, mở rộng allowlist → four-eyes + `reason`; ở `PRODUCTION_REAL` chỉ qua deployment có approval (P7-3/P9-1). `realCustomerCallAllowed=true` chỉ qua P9-1 sau DF-03. `v1NotificationEnabled`/`recordingEnabled` bật lên bị từ chối ở mọi mode.
- Không đọc được trạng thái kill switch ⇒ coi như **ON** (fail-closed).
- Actor thực hiện call không được tự mở rộng allowlist cho đích mình sắp gọi.
