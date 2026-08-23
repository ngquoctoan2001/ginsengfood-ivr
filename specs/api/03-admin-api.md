# API-03 — Admin API

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/11` §5,§8; `/08` (monitoring/privacy); DF-01 (RBAC).
Base path `/v1/ivr/order-confirmation/*`. Admin RBAC server-side; mọi POST có `reason` + `X-Actor-Id` + audit + `Idempotency-Key`.

## 0. Authentication và account API (W-0105)

Ivr.Api phát opaque bearer session 8 giờ và là authority cho đúng hai role
`Admin`/`Operator`. Login failure luôn dùng generic `401 IVR_UNAUTHENTICATED`;
không phân biệt username sai, password sai, account disabled hay locked.

| Endpoint | Method | Permission | Chức năng |
| --- | --- | --- | --- |
| `/auth/sign-in` | POST | anonymous + rate limit | username/password → opaque session projection |
| `/auth/session` | GET | authenticated | resolve subject/role/permissions hiện tại |
| `/auth/sign-out` | POST | authenticated | revoke session hiện tại |
| `/accounts/me` | GET | `IVR_ACCOUNT_SELF_VIEW` | profile của chính subject |
| `/accounts` | GET | `IVR_ACCOUNT_VIEW` | danh sách account |
| `/accounts/{accountId}` | GET | `IVR_ACCOUNT_VIEW` | chi tiết account |
| `/accounts` | POST | `IVR_ACCOUNT_MANAGE` | tạo account; username immutable/non-reusable |
| `/accounts/{accountId}` | PATCH | `IVR_ACCOUNT_MANAGE` | sửa display name/role/status với version |
| `/accounts/{accountId}:reset-password` | POST | `IVR_ACCOUNT_PASSWORD_RESET` | admin đặt password mới và revoke session đích |
| `/accounts/{accountId}:delete` | DELETE | `IVR_ACCOUNT_MANAGE` | soft-delete và revoke session đích |
| `/account-roles` | GET | `IVR_ACCOUNT_VIEW` | hai role và permission matrix canonical |

Operator có đúng bốn quyền: `IVR_QUEUE_VIEW`, `IVR_SIM_DISABLE`,
`IVR_MANUAL_RETRY`, `IVR_ACCOUNT_SELF_VIEW`. Admin có 11 quyền được liệt kê ở
`specs/ui/08-role-permission-ui.md`. Backend luôn re-derive permission từ role;
bearer request không fallback sang mock header.

## 1. Endpoint & permission
| Endpoint | Method | Permission (DF-01) | Contract | Chức năng |
| --- | --- | --- | --- | --- |
| `/queue` | GET | `IVR_QUEUE_VIEW` | Queue projection (masked) | Xem queue/capacity/incident |
| `/queue:pause` | POST | `IVR_QUEUE_PAUSE` | `AdminMutationRequest` → `IvrAdminActionResult` | Pause queue (reason/evidence) |
| `/queue:resume` | POST | `IVR_QUEUE_RESUME` | `AdminMutationRequest` → `IvrAdminActionResult` | Resume sau khi incident resolved |
| `/sim-channels/{simChannelId}:disable` | POST | `IVR_SIM_DISABLE` | `AdminMutationRequest` → `IvrAdminActionResult` | Disable SIM (health/failure reason) |
| `/sim-channels/{simChannelId}:enable` | POST | `IVR_SIM_ENABLE` | `AdminMutationRequest` → `IvrAdminActionResult` | Enable SIM sau health pass |
| `/technical-retries` | POST | `IVR_MANUAL_RETRY` | `TechnicalRetryRequest` → `IvrTechnicalRetryResult` | Request technical retry (không tăng customer attempt) |
| `/admin-reviews` | POST | `IVR_RESULT_REVIEW` | `AdminReviewRequest` → `IvrAdminReviewResult` | Ghi review/annotation |
| `/scripts/{templateId}/{version}` | GET | `IVR_QUEUE_VIEW` | `IvrScriptVersionDetail` | Một phiên bản ở mọi trạng thái, gồm cả bản nháp |
| `/scripts/` | POST | `IVR_SCRIPT_EDIT` | `IvrScriptDraftRequest` → `IvrScriptActionResult` | Tạo bản nháp; phiên bản là bất biến sau khi tạo |
| `/scripts/{templateId}/{version}:submit` | POST | `IVR_SCRIPT_REVIEW` | `IvrScriptTransitionRequest` | Chuyển bản nháp sang chờ duyệt |
| `/scripts/{templateId}/{version}:approve` | POST | `IVR_SCRIPT_APPROVE_*` theo `approval_type` | `IvrScriptApprovalRequest` | Ghi một chữ ký duyệt |
| `/scripts/{templateId}/{version}:retire` | POST | `IVR_SCRIPT_RETIRE` | `IvrScriptTransitionRequest` | Thu hồi; fail-closed mọi chế độ, không xoá |
| `/call-jobs/{ivrCallJobId}:terminate` | POST | `IVR_CALL_TERMINATE` | `AdminMutationRequest` → `IvrAdminActionResult` | Cắt cuộc đang chạy; `409` nếu không có cuộc nào đang chạy |
| `/call-jobs:terminate-all` | POST | `IVR_CALL_TERMINATE` | `AdminMutationRequest` → `IvrAdminActionResult` | Cắt mọi cuộc đang chạy; hành động riêng, không gộp vào kill switch |

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

Bốn route mutation được ghim vào **console session scheme**. Seam quyền MOCK (`X-Permissions`)
mint bất cứ quyền nào được yêu cầu, MOCK là chế độ mặc định, và một trong các quyền này ký duyệt
lời thoại đọc cho khách nghe — nên seam đó không được chạm tới chúng.
| `/feature-flags/{environment}` | GET | `IVR_FLAG_READ` | `FeatureFlagReadResult` | Đọc fresh typed snapshot; provider lỗi trả fail-closed |
| `/feature-flags/{environment}/kill-switch` | GET | `IVR_FLAG_READ` | `KillSwitchVerification` | Xác minh revision và trạng thái kill switch effective |
| `/feature-flags/{environment}` | POST | `IVR_RUNTIME_GATE_ADMIN` *(OD-V1-20 duyệt 2026-08-22 — cấp cho `Admin`)* | `FeatureFlagMutationRequest` | Mutation atomic, reason, idempotency, audit và four-eyes theo chiều rủi ro |

## 2. Ràng buộc admin action (P0)
Mỗi POST phải có: authenticated actor (`X-Actor-Id`), permission server-side, `reason`, `target_type`+`target_id`, audit record, evidence ref nếu ảnh hưởng queue/SIM/retry/result, `no_policy_bypass=true`.

P2-8 thực thi `X-Actor-Id == authenticated NameIdentifier`; mỗi mutation commit business state + `ivr_admin_actions` + append-only `ivr_audit_log` trong cùng transaction, gồm `before/after`, permission, correlation và `no_policy_bypass=true`.

Admin **KHÔNG** được:
- Gọi khách ngoài attempt policy (D-10) hoặc reset customer attempt count.
- **Force confirm/cancel order** (D-02: order state do Core; P0-IVR-002).
- Enable SIM khi health check đang fail.
- Resume queue khi capacity incident chưa xử lý.
- Bỏ qua blocker (sellable/recall/sale-lock/do-not-call) — DO-*/DC-01.

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
- **10 endpoint admin** (3 GET + 7 POST), mỗi cái map 1 permission `IVR_*`. Ba endpoint feature-flag do P0-4 bổ sung; quyền mutation `IVR_RUNTIME_GATE_ADMIN` được cấp cho role `Admin` từ 2026-08-22 (`OD-V1-20`), nhưng endpoint **vẫn fail-closed** ở tầng sau: `IRuntimeGateAuthorization` (bản production luôn `false`) trả `409 IVR_OPERATIONAL_BLOCKED`. Không endpoint nào cho phép force order/bypass blocker.

## Runtime-gate controls — bất đối xứng theo chiều an toàn

`OD-V1-20` (duyệt 2026-08-22, owner module IVR) cấp quyền `IVR_RUNTIME_GATE_ADMIN` cho role `Admin`; chữ ký four-eyes của Security/Platform + Release owner vẫn còn thiếu. Permission không còn chặn ai, nhưng `IRuntimeGateAuthorization` thì có — mọi mutation hiện trả `409` trước khi tới các quy tắc dưới đây. Khi lớp đó được mở, những quy tắc này **là** biện pháp kiểm soát, không phải lớp phụ:

- **Chiều giảm rủi ro luôn được phép** ở mọi environment: bật `globalDialKillSwitch`, thu hẹp/làm rỗng `labDestinationAllowlist`, đặt `realCustomerCallAllowed=false`. Chỉ cần permission + `reason` + audit; **không** four-eyes, **không** chờ deployment. Một kill switch không bật được trong sự cố là kill switch hỏng.
- **Chiều tăng rủi ro luôn bị gate**: tắt kill switch, mở rộng allowlist → four-eyes + `reason`; ở `PRODUCTION_REAL` chỉ qua deployment có approval (P7-3/P9-1). `realCustomerCallAllowed=true` chỉ qua P9-1 sau DF-03. `v1NotificationEnabled`/`recordingEnabled` bật lên bị từ chối ở mọi mode.
- Không đọc được trạng thái kill switch ⇒ coi như **ON** (fail-closed).
- Actor thực hiện call không được tự mở rộng allowlist cho đích mình sắp gọi.
