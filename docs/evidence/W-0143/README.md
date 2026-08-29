# W-0143 — M8-03 admin audit/capacity surface reconciliation và M3 handoff

Ngày: `2026-08-29`  
Baseline rà soát: `main@b082ed1`  
Trạng thái: `M8_LOCAL_COMPLETE / EXISTING_SURFACE_VERIFIED / M3_SECURITY_ACCEPTANCE_REQUIRED`  
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Kết luận

Mô tả cũ của M8-03 — “ký ownership trước, sau đó bổ sung audit-evidence/capacity-incident
endpoints” — **không còn đúng với codebase hiện tại**.

- Ownership phía Module 8 đã được ký tại S-03: Module 3 sở hữu operator identity/UI; IVR sở hữu
  service-to-service admin API.
- `GET /dashboard` đã trả `open_incidents` và `missed_deadline_count`, gồm
  `capacity_incident_id`, scope, trạng thái, cờ hold và số deadline bị lỡ.
- `GET /call-jobs/{ivrCallJobId}/detail` đã trả `evidence_refs` và `audit_refs`, cùng attempts,
  results, callbacks, technical exceptions và review items.
- OpenAPI, integration tests và reference UI hiện đều có các field trên.

Vì vậy W-0143 **không thêm route mới và không dựng thêm UI**. Tạo một endpoint raw/global để dump
audit/evidence khi chưa có màn hình/use case đã ký vừa trùng surface hiện có, vừa mở rộng dữ liệu
nhạy cảm mà không có acceptance contract. Bên yêu cầu route mới phải chỉ ra trước: màn hình hoặc
consumer, filter/paging, retention/redaction, authorization tier và negative tests.

## 2. Ownership đã khóa

| Phần | Owner | Ranh giới |
|---|---|---|
| Operator identity, role/claim, màn hình quản trị, BFF và deployment UI | Module 3 | Token IVR không được xuống browser; actor lấy từ authenticated subject của M3. |
| Admin read/write/danger API, fail-closed tier validation và audit actor/reason | Module 8 / IVR | Chỉ nhận service credential + actor identity; không phát hành account/session riêng. |
| Secret custody, network selector, rotation và security acceptance | Security/Platform + M3 | Chưa có artifact production nên vẫn external. |

Nguồn contract: [IR-06 §4A](../../../integration-requirements/06-module-3-api-handover.md#4a-api-c--bề-mặt-quản-trị-module-3-điều-khiển-và-quan-sát-ivr),
[S-03 trong TODAY-01](../../../plan/ivr-orther/today-01-decision-signoff-pack-2026-08-29.md#s-03--admin-identityui-handoff-w-0128)
và [W-0128](../W-0128/README.md).

## 3. Surface M3 phải dùng — không được giao IVR làm lại

| Nhu cầu UI/BFF | Route hiện có | Field chính | Bằng chứng |
|---|---|---|---|
| Dashboard và capacity incident đang mở | `GET /v1/ivr/order-confirmation/dashboard` | `open_incidents[]`, `missed_deadline_count`, queue/result/attempt/SIM aggregates | `AdminReadService.GetDashboardAsync`; `IT-ADMIN-READ-02`; OpenAPI `IvrDashboardProjection` |
| Tìm call job | `GET /v1/ivr/order-confirmation/call-jobs` | paging + filter theo program/status/result/expiry | `AdminReadService.ListCallJobsAsync`; `IT-ADMIN-READ-04/05` |
| Audit/evidence theo call job | `GET /v1/ivr/order-confirmation/call-jobs/{ivrCallJobId}/detail` | `evidence_refs`, `audit_refs`, attempts, results, callbacks, exceptions, review items | `AdminReadService.GetCallJobDetailAsync`; `IT-ADMIN-READ-06/08`; OpenAPI `IvrCallJobDetail` |
| Trạng thái và năng lực từng kênh | `GET /v1/ivr/order-confirmation/sim-channels` | trạng thái, enabled, adapter/execution mode, health metadata | `AdminReadService.ListSimChannelsAsync`; IR-06 §4A.3 |
| Báo cáo/đối soát | `GET /analytics/summary`, `/trend`, `/breakdown`, `/export` | projection đã redaction; export là read có audit | IR-06 §4A.3; OpenAPI hiện hành |

Code/UI tham chiếu:

- Admin read endpoint map: `src/Ivr.Api/Admin/IvrAdminEndpoints.cs`
- Admin read projections: `src/Ivr.Api/Application/AdminReadService.cs`
- Admin read contracts: `src/Ivr.Api/Admin/AdminReadContracts.cs`
- Integration tests: `tests/Ivr.IntegrationTests/AdminReadApiTests.cs`
- OpenAPI draft.22: `specs/api/openapi/ivr-order-confirmation.v1.yaml`
- Reference dashboard: `admin-ui/src/app/(console)/dashboard/page.tsx`
- Reference call detail: `admin-ui/src/app/(console)/calls/[ivrCallJobId]/page.tsx`

Reference UI trong repo IVR chỉ là implementation/reference proof; W-0128 quy định
`admin-ui=NOT_DEPLOYED_BY_IVR`. Nó không phải lý do để M3 đẩy ownership giao diện trở lại Module 8.

## 4. Stop rule cho yêu cầu endpoint mới

Module 8 từ chối mở route/schema mới nếu request chỉ nói chung chung “cần audit”, “cần evidence”
hoặc “cần capacity screen”. Request hợp lệ phải có đủ:

1. screen/consumer và câu hỏi vận hành mà surface hiện có chưa trả lời được;
2. query/filter/paging, stable identifier và freshness requirement;
3. field allowlist, PII/redaction, retention và audit policy;
4. tier `read|write|danger`, actor/reason semantics và negative authorization cases;
5. M3 contract owner + Security/Privacy owner ký;
6. OpenAPI/client/shared E2E acceptance.

Thiếu một mục thì trạng thái đúng là `CONTRACT_DECISION_REQUIRED`, không phải “IVR chưa làm”.

## 5. Handoff bắt buộc cho Module 3 / Security / Platform

| Bên nhận | Artifact phải trả | Trạng thái |
|---|---|---|
| M3 contract/UI owner | Ký IR-06 §4A; role/claim → `read|write|danger`; regenerate client từ exact draft.22; BFF giữ token server-side; actor-id mapping | `NOT_RECEIVED` |
| M3 + IVR | Shared positive/negative E2E cho ba tier, thiếu actor/reason, token rotation/retirement và các route dashboard/detail | `NOT_RUN` |
| Security/Platform | Secret-store paths, rotation owner/schedule, real namespace/pod selectors, ingress/NetworkPolicy và credential smoke | `OWNER_DATA_REQUIRED` |
| Privacy/Security nếu đòi raw/global search mới | Field allowlist, redaction, retention và access-audit approval | `NOT_REQUESTED / NOT_APPROVED` |

## 6. Acceptance của W-0143

- [x] Đối chiếu ownership với W-0128 và S-03.
- [x] Đối chiếu runtime route/projection với OpenAPI.
- [x] Xác nhận capacity incident và evidence/audit refs đã có test.
- [x] Xác nhận reference UI đã render hai surface này.
- [x] Sửa worklist để không giao làm lại endpoint/UI đã tồn tại.
- [ ] M3/Security/Platform ký và chạy shared/production evidence — ngoài quyền của W-0143.

W-0143 được phép kết thúc ở `EVIDENCE_SUBMITTED`; không được nâng `ACCEPTED`,
`CONTRACT_LOCKED`, `INTEGRATION_READY` hoặc `PRODUCTION_READY` khi các artifact external ở §5 chưa có.

## 7. Verification

| Kiểm tra | Kết quả |
|---|---|
| `AdminReadApiTests` focused integration | `11/11 PASS` |
| Admin UI contract drift + console screen tests | `18/18 PASS` |
| OpenAPI lint | `2/2 API descriptions valid` |
| OpenAPI parse/schema negative/current compatibility | `PASS` |
| OpenAPI pinned-hash/human-diff drift | `PASS` |
| API docs selftest | `PASS`, 14 generated artifacts |
| Markdown corpus map | W-0143 `0 unresolved`; repo-wide legacy debt được giữ ngoài scope |
| Gate-status mirror | `PASS` — 11 gate, 141 work item, 23 open decision; rung 0, production flag false |
| GitNexus detect-changes | Không báo indexed code symbol hoặc execution flow nào; scope là docs/tracker/mirror |

Lần gọi đầu bằng `pnpm --dir` không được tính là test failure: nó dừng ở package-manager policy
`ERR_PNPM_IGNORED_BUILDS` và tạo một lockfile phụ. Repo dùng `npm --prefix` cho hai workspace này;
lockfile phụ đã bị xóa, và các lệnh canonical bằng npm đã pass như bảng trên.

## 8. Người ký handoff phía Module 8

**Tôi — Module 8 / Project Owner** · **29/08/2026**.

Chữ ký này xác nhận surface IVR hiện có, ownership phía Module 8 và stop rule. Nó không thay chữ ký
của Module 3, Security, Platform hoặc Privacy.
