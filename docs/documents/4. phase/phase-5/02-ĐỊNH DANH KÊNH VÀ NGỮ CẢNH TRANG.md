# P5.2A - Channel Identity / Page Context

**Backlog:** `GW-BLG-001`  
**Mapped FB file:** FB-01 - Meta App / Business / Page Registry / Token Governance  
**Gateway:** `PRODUCTION_BLOCKED`

## 1. Mục tiêu

Chuẩn hóa Page Registry và channel identity để mọi inbound event biết đến từ Page nào, role nào, live/session nào, có quyền outbound nào và có được production hay không.

## 2. Page Registry owner decisions

| Page/Profile | ID | Vai trò | Live | Messenger | CRM | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- |
| Ái Vy Phạm | 61557082440623 | Page CSKH / live spoke | Có | Không | Không | Không chốt Messenger riêng. |
| Hương Xuân | 61585392260203 | Page live spoke | Có | Không | Không | Handoff về Commerce Hub. |
| Huyền Trân | 61585211732304 | Page test | Không | Không | Không | Chỉ dùng test. |
| Sâm Biển Việt Nam | 102437252680397 | Page chạy ads/live spoke | Có | Không | Không | Handoff về Commerce Hub. |
| Ginsengfood - Cháo Sâm Mỗi Ngày | 101044219306135 | Page chính / Commerce Hub | Có | Có | Có | Page chính thức, AI auto reply và CRM. |
| Ginsengfood - Thực Dưỡng Gia Đình | 161431274523980 | Page phụ | Không | Không | Không | Audience spoke. |
| Ginsengfood - Chăm Sóc Ba Mẹ | 105347285574705 | Page phụ | Không | Không | Không | Audience spoke. |
| Ginsengfood - Món Chay Thực Dưỡng | 395032681300769 | Page phụ | Không | Không | Không | Audience spoke. |
| Ginsengfood - Quà Tặng Sức Khỏe | 330237488279968 | Page phụ | Không | Không | Không | Cần owner xác nhận lại Business/App mapping nếu lệch nguồn. |

Owner decision hiện tại:

- Page chính thức: Ginsengfood - Cháo Sâm Mỗi Ngày.
- Page được AI auto reply: Ginsengfood - Cháo Sâm Mỗi Ngày.
- Page được CRM: Ginsengfood - Cháo Sâm Mỗi Ngày.
- Page test: Huyền Trân.
- Live pages: Ái Vy Phạm, Hương Xuân, Sâm Biển Việt Nam, Ginsengfood - Cháo Sâm Mỗi Ngày.

## 3. ChannelContext contract

```yaml
ChannelContext:
  source_platform: facebook | messenger | live_comment | post_comment
  source_page_id: required
  commerce_hub_page_id: required
  page_role: commerce_hub | live_spoke | ads_spoke | audience_spoke | test
  live_session_id: required_if_live
  comment_id: required_if_public_comment
  messenger_thread_id: required_if_private
  customer_channel_identity_id: required
  page_registry_version: required
  production_allowed: boolean
  outbound_allowed:
    public_reply: boolean
    private_reply: boolean
    messenger_handoff: boolean
    crm: boolean
```

Nếu `production_allowed=false`, Gateway chỉ được dry-run/evidence.

## 4. Meta App / token governance

MUST:

- App ID/App Secret/Verify Token/Page Access Token không ghi trực tiếp trong docs/evidence.
- Chỉ lưu `secret_ref`.
- Token rotation có audit.
- App mode, app review, subscribed fields phải rõ.
- Page subscription status phải inspect được.

MUST NOT:

- Hardcode Page ID hoặc token trong logic.
- Gộp public comment và Messenger private thread.
- Cho Page spoke tự chốt Messenger sales riêng nếu policy yêu cầu Commerce Hub.

## 5. Owner decision cần chốt

| Decision ID | Nội dung | Trạng thái |
| --- | --- | --- |
| P5-OD-001 | Xác nhận Business ID/App mapping cho Page Quà Tặng Sức Khỏe. | `OWNER_CONFIRM_REQUIRED` |
| P5-OD-002 | Chốt Page nào được outbound public reply trong pilot. | `OWNER_CONFIRM_REQUIRED` |
| P5-OD-003 | Chốt Page nào được private reply từ comment trong pilot. | `OWNER_CONFIRM_REQUIRED` |
| P5-OD-004 | Chốt permission/subscribed fields cần xin App Review. | `OWNER_CONFIRM_REQUIRED` |

## 6. Acceptance criteria

- Mỗi inbound event có ChannelContext.
- Unknown page bị reject/quarantine.
- Page chưa production_allowed không gửi outbound production.
- Commerce Hub mapping preserve khi handoff từ spoke.
- Evidence sample đã mask token/secret/PII.

## 7. Smoke

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P5.2A-SMK-001 | Known Commerce Hub event | Resolve đúng page/channel/session. |
| P5.2A-SMK-002 | Known live spoke comment | Resolve source_page_id và commerce_hub_page_id. |
| P5.2A-SMK-003 | Unknown page id | Reject/quarantine. |
| P5.2A-SMK-004 | Page production_allowed=false | Dry-run only. |
| P5.2A-SMK-005 | Evidence screenshot/log | Không lộ token/secret. |

## 8. Final status

`LIMITED_IMPLEMENTATION_REPORT_ONLY`

`GLOBAL_GATEWAY_PRODUCTION_BLOCKED`

## 9. SRS hardening addendum - Page Registry và Channel Identity

### 9.1. Mục tiêu SRS

File này là SRS slice cho Page Registry. Dev phải dùng nó để thiết kế bảng, API admin, import/normalize dữ liệu bổ sung và gate production theo từng Page. Raw list từ tài liệu bổ sung không được dùng trực tiếp để bật webhook hoặc outbound.

### 9.2. Entity resolution rules

| Rule ID | Rule |
| --- | --- |
| P5-2A-R001 | Mỗi Meta object phải có `meta_object_type`; Page/Profile/Business/App không được trộn vào cùng một semantics. |
| P5-2A-R002 | `meta_object_id` chỉ là external id, không phải primary key nội bộ. |
| P5-2A-R003 | Page dùng production phải có `page_registry_id`, `business_id_ref`, `app_id_ref`, `token_secret_ref`, permission evidence và owner approval. |
| P5-2A-R004 | Unknown object hoặc object không khớp owner role phải `quarantined`, không `production_enabled`. |
| P5-2A-R005 | Commerce Hub là Page nhận chốt private chính; spoke page chỉ chuyển/handoff nếu policy cho phép. |
| P5-2A-R006 | Test page chỉ dùng staging/dry-run; mọi production outbound từ test page là P0 fail. |
| P5-2A-R007 | Audience spoke chỉ được preserve attribution nếu chưa có permission delivery. |

### 9.3. Registry schema

```yaml
FacebookPageRegistryEntry:
  page_registry_id: string
  meta_object_id: string
  meta_object_type: page|profile|business|app|unknown
  display_name: string
  canonical_name: string
  owner_department: marketing|sales|cskh|operations|test|unknown
  page_role: commerce_hub|live_spoke|cskh_spoke|audience_spoke|test|unknown
  commerce_hub_page_registry_id: nullable
  business_id_ref: nullable
  app_id_ref: nullable
  token_secret_ref: nullable
  verify_token_secret_ref: nullable
  webhook_subscribed_fields: list
  permission_status: missing|pending|approved|rejected|expired|unknown
  app_review_status: not_required|pending|approved|rejected|unknown
  public_reply_allowed: boolean
  private_reply_allowed: boolean
  live_allowed: boolean
  crm_delivery_allowed: boolean
  ads_attribution_allowed: boolean
  production_status: blocked|pilot|enabled|disabled
  quarantine_reason: nullable
  evidence_refs: list
  owner_decision_refs: list
```

### 9.4. Validation matrix

| Scenario | Required result |
| --- | --- |
| Page ID known, permission approved, app review approved, owner pilot approved | `production_status=pilot` hoặc `enabled` theo owner decision. |
| Page ID known nhưng token ref thiếu | `production_status=blocked`; dry-run only. |
| Page ID known nhưng object type là profile | Không gửi page outbound; yêu cầu owner normalize. |
| Page spoke comment hỏi giá | Resolve spoke + hub; public safe ack; private handoff nếu permitted. |
| Unknown Page ID | Quarantine event; no reply. |
| Same display name, different ID | Không merge tự động; owner review. |
| Same ID, different display name | Keep ID stable; update display name with audit. |

### 9.5. Admin workflows

| Workflow ID | Steps | Acceptance |
| --- | --- | --- |
| P5-2A-W001 | Import raw Page list -> classify object type -> assign owner role -> review unknown. | No raw row bypasses registry validation. |
| P5-2A-W002 | Add page token ref -> attach permission evidence -> set delivery flags. | Secret never displayed; evidence ref required. |
| P5-2A-W003 | Enable pilot for one page. | Requires owner decision, app review/permission status and smoke plan. |
| P5-2A-W004 | Disable page. | Immediately blocks outbound and records reason. |
| P5-2A-W005 | Map spoke to commerce hub. | Handoff preserves source page and hub page. |

### 9.6. Production lock

Production enablement requires all:

1. `meta_object_type=page`.
2. `page_role` not `unknown`.
3. `permission_status=approved`.
4. `app_review_status=approved` or documented `not_required`.
5. `token_secret_ref` exists for delivery.
6. Webhook subscribed fields are documented.
7. Owner decision ref exists.
8. Smoke evidence ref exists.
9. Public/private policy for page role exists.
10. Kill-switch exists.

If any condition is missing, output stays `GLOBAL_GATEWAY_PRODUCTION_BLOCKED`.
