# TEST-04 — Contract Test Plan

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p11` · Nguồn: `api/*` + `openapi/ivr-order-confirmation.v1.yaml`; error `api/06`.

## 1. OpenAPI hygiene (DF-02)
| ID | Kiểm |
| --- | --- |
| CT-OAS-01 | `ivr-order-confirmation.v1.yaml` parse OpenAPI 3.1 pass (CI) |
| CT-OAS-02 | Mọi `$ref` resolve; required fields đầy đủ |
| CT-OAS-03 | Enum `program_code`/`max_attempts=2`/`window∈{300,900}` khớp D-10 |

## 2. `IvrConfirmationTaskV1` (schema + business)
| ID | Given | Then |
| --- | --- | --- |
| CT-TASK-01 | seed TASK-0001 | validate schema pass; sellable_status[] per-line có captured_at |
| CT-TASK-02 (neg) | thiếu `sellable_status` | reject |
| CT-TASK-03 (neg) | `max_attempts≠2` | `409 IVR_POLICY_MISMATCH` |
| CT-TASK-04 (neg) | field cấm (full_address/payment) trong payload | reject (privacy) |

## 3. Callback current/target + response
> ⚠️ **DS-03/DS-04:** OpenAPI đã tách **current** (`IvrConfirmationResultCallbackCurrentV1`, `CallbackCoreResponseCurrent`: `200/422`, không `order_version_seen_by_ivr`) và **target** (`IvrConfirmationResultCallbackTargetV1`, `CallbackCoreResponseTarget`: `order_version_seen_by_ivr` + `CALLBACK_*` codes). Target cần IR-SALES-OC1/OC2.

| ID | Given | Then |
| --- | --- | --- |
| CT-CB-01 | callback current đủ field, không có `order_version_seen_by_ivr` | Validate schema current pass; Core current trả `200` hoặc `422` |
| CT-CB-02 (target/neg) | target callback thiếu `order_version_seen_by_ivr` | Validate schema target fail. Core reject theo version vẫn **deferred** (IR-SALES-OC1) |
| CT-CB-03 (neg) | thiếu `evidence_ref` | reject/hold |
| CT-CB-04 | `recommended_core_action` ∈ enum | advisory, Core vẫn revalidate |

## 4. Error model (api/06 §1b/§1c)
| ID | Given | Then |
| --- | --- | --- |
| CT-ERR-01 | intake `ACCEPTED*`/`SKIPPED`/`HELD` | **200 + decision** (không 4xx) |
| CT-ERR-02 | intake `REJECTED_*`/`BLOCKED` | **4xx + envelope** với `code` §1c |
| CT-ERR-03 | mỗi `code` (IVR_*) map đúng HTTP + decision | bảng §1c khớp |
| CT-ERR-04 | error envelope shape `{error:{code,message,details,correlationId}}` | đồng bộ ops (DO-06) |

## 5. Consume ops SellableStatus / error codes (DO-01/DO-06)
| ID | Given | Then |
| --- | --- | --- |
| CT-OPS-01 | SellableStatus decision/flags từ seed inventory | parse đúng; block khi NOT_SELLABLE/BLOCKED |
| CT-OPS-02 | ops code SALE_LOCK_ACTIVE/RECALL_IMPACT_ACTIVE/... | Core map → BLOCKED_BY_CORE / fail-closed |
| CT-OPS-03 | ops `/health/ready=503` | fail-closed |

## Báo cáo
~18 contract case; OpenAPI + task/callback current/target + error model (200 vs 4xx) + ops consume phủ. Consumer-driven contract cho task (Order Core) và callback (Core) đề xuất.
