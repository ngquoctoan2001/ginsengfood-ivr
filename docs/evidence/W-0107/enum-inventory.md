# W-0107 — Inventory enum (sinh tự động)

> File này do `enum-inventory.mjs` sinh ra ở GĐ 1. Không sửa tay.
> Ba nguồn độc lập; một họ chỉ được coi là **đóng** khi có mặt ở nguồn 1 hoặc 2.
> `TASK_SKIPPED_TRUSTED_CUSTOMER` vẫn có mặt do `LEGACY_READ` compatibility; `OD-18`/W-0123 đã
> dừng active runtime emission. Inventory enum không đồng nghĩa enum còn là active outcome.

## Nguồn 1 — OpenAPI (enum khai báo tĩnh)

| Thuộc tính / schema | Dòng | Số giá trị | Giá trị |
| --- | --- | --- | --- |
| `schema` | 859 | 2 | `sales-platform`, `order-core` |
| `schema` | 864 | 2 | `ivr-worker`, `ivr-adapter` |
| `schema` | 869 | 2 | `GOLDEN_HOUR`, `TWENTY_FOUR_SEVEN` |
| `schema` | 884 | 2 | `DAY`, `HOUR` |
| `schema` | 889 | 3 | `RESULT_TYPE`, `SCRIPT_VARIANT`, `PROGRAM` |
| `schema` | 904 | 1 | `ivr.internal.write` |
| `schema` | 909 | 5 | `dev`, `staging`, `lab`, `pilot`, `prod` |
| `ErrorCode` | 930 | 16 | `IVR_UNAUTHENTICATED`, `IVR_FORBIDDEN_CALLER`, `IVR_MALFORMED_REQUEST`, `IVR_MISSING_TRACE`, `IVR_IDEMPOTENCY_CONFLICT`, `IVR_VERSION_CONFLICT`, `IVR_NOT_OFFICIAL_ORDER`, `IVR_STATE_NOT_CALLABLE`, `IVR_POLICY_MISMATCH`, `IVR_CONTACT_INVALID`, `IVR_SCRIPT_NOT_APPROVED`, `IVR_PII_POLICY_VIOLATION`, `IVR_OPERATIONAL_BLOCKED`, `IVR_NOT_FOUND`, `IVR_RATE_LIMITED`, `IVR_INTERNAL_ERROR` |
| `ConsoleAccountErrorCode` | 964 | 18 | `IVR_UNAUTHENTICATED`, `IVR_FORBIDDEN_CALLER`, `IVR_MALFORMED_REQUEST`, `IVR_MISSING_TRACE`, `IVR_IDEMPOTENCY_CONFLICT`, `IVR_VERSION_CONFLICT`, `IVR_NOT_OFFICIAL_ORDER`, `IVR_STATE_NOT_CALLABLE`, `IVR_POLICY_MISMATCH`, `IVR_CONTACT_INVALID`, `IVR_SCRIPT_NOT_APPROVED`, `IVR_PII_POLICY_VIOLATION`, `IVR_OPERATIONAL_BLOCKED`, `IVR_NOT_FOUND`, `IVR_RATE_LIMITED`, `IVR_ACCOUNT_CONFLICT`, `IVR_ACCOUNT_POLICY_VIOLATION`, `IVR_INTERNAL_ERROR` |
| `ConsoleRole` | 999 | 2 | `Admin`, `Operator` |
| `ConsoleAccountStatus` | 1002 | 3 | `ACTIVE`, `DISABLED`, `DELETED` |
| `token_type` | 1046 | 1 | `Bearer` |
| `ProgramCode` | 1132 | 2 | `GOLDEN_HOUR`, `TWENTY_FOUR_SEVEN` |
| `currency` | 1161 | 1 | `VND` |
| `locale` | 1177 | 1 | `vi-VN` |
| `decision` | 1187 | 4 | `SELLABLE`, `NOT_SELLABLE`, `BLOCKED`, `UNKNOWN` |
| `contract_version` | 1230 | 1 | `ivr-order-confirmation.v1` |
| `payment_method_snapshot` | 1241 | 2 | `ONLINE`, `COD` |
| `decision` | 1300 | 12 | `TASK_ACCEPTED_CALL_JOB_CREATED`, `TASK_ACCEPTED_DRY_RUN_ONLY`, `TASK_SKIPPED_TRUSTED_CUSTOMER`, `TASK_REJECTED_NOT_OFFICIAL_ORDER`, `TASK_REJECTED_STATE_NOT_CALLABLE`, `TASK_REJECTED_POLICY_MISMATCH`, `TASK_REJECTED_CONTACT_INVALID`, `TASK_REJECTED_SCRIPT_NOT_APPROVED`, `TASK_REJECTED_INVALID_TRACE`, `TASK_BLOCKED_OPERATIONAL`, `TASK_HELD_ADMIN_REVIEW`, `TASK_HELD_POLICY_MISSING` |
| `ResultType` | 1472 | 11 | `IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, `IVR_NO_ANSWER_ATTEMPT`, `IVR_NO_ANSWER_FINAL`, `IVR_CONFIRMATION_WINDOW_EXPIRED`, `IVR_INVALID_PHONE_FINAL`, `IVR_WRONG_INPUT`, `IVR_TECHNICAL_EXCEPTION`, `IVR_CAPACITY_EXCEPTION`, `IVR_OPERATIONAL_BLOCKED`, `IVR_POLICY_BLOCKED` |
| `voice_region` | 1839 | 3 | `North`, `Central`, `South` |
| `state` | 1983 | 4 | `UP`, `DOWN`, `READY_503`, `NOT_WIRED` |
| `status` | 2138 | 3 | `FRESH`, `STALE`, `NO_DATA` |
| `warehouse_status` | 2150 | 4 | `NOT_RUN`, `COMPLETE`, `BACKLOG`, `MISMATCH` |
| `program` | 2160 | 2 | `GOLDEN_HOUR`, `TWENTY_FOUR_SEVEN` |
| `bucket` | 2163 | 2 | `DAY`, `HOUR` |
| `dimension` | 2275 | 3 | `RESULT_TYPE`, `SCRIPT_VARIANT`, `PROGRAM` |
| `dimension` | 2296 | 3 | `RESULT_TYPE`, `SCRIPT_VARIANT`, `PROGRAM` |
| `status` | 2330 | 1 | `APPLIED` |
| `status` | 2365 | 1 | `RESOLVED` |
| `environment` | 2385 | 5 | `dev`, `staging`, `lab`, `pilot`, `prod` |

## Nguồn 2 — CHECK constraint trong EF model

| Cột | Số giá trị | Giá trị |
| --- | --- | --- |
| `role` | 2 | `Admin`, `Operator` |
| `status` | 3 | `ACTIVE`, `DISABLED`, `DELETED` |
| `status` | 3 | `HELD_MOCK`, `READY_FOR_ELIGIBILITY`, `PUBLISHED` |
| `execution_mode` | 3 | `MOCK`, `LAB_REAL_SIM`, `PRODUCTION_REAL` |
| `status` | 4 | `DRAFT`, `IN_REVIEW`, `APPROVED`, `RETIRED` |
| `approval_type` | 4 | `MOCK_TEST`, `LAB`, `CONTENT`, `PRIVACY_LEGAL` |

## Nguồn 3 — write-site trong `src/**/*.cs` (họ MỞ, không đảm bảo đầy đủ)

| Trường | Số giá trị tìm thấy | Giá trị |
| --- | --- | --- |
| `ClosedReason` | 1 | `IVR_CAPACITY_EXCEPTION` |
| `DeliveryStatus` | 3 | `READY`, `RETRY_PENDING`, `SENDING` |
| `ExecutionMode` | 3 | `LAB_REAL_SIM`, `MOCK`, `PRODUCTION_REAL` |
| `QueueStatus` | 12 | `BLOCKED`, `CLOSED_CAPACITY`, `HELD_ADMIN_REVIEW`, `HELD_CALLBACK`, `HELD_CAPACITY`, `HELD_LEASE_RECOVERY`, `HELD_MOCK`, `HELD_NORMALIZATION`, `HELD_TECHNICAL_REVIEW`, `LEASED`, `QUEUED`, `SKIPPED` |
| `Reason` | 4 | `IVR_CAPACITY_EXCEPTION`, `LEASE_EXPIRED_RECONCILIATION_REQUIRED`, `NO_DISPATCH_BEFORE_DEADLINE`, `SIGN_OUT` |
| `RecommendedCoreAction` | 1 | `REVALIDATE_AND_HOLD_ADMIN_REVIEW` |
| `ResultState` | 1 | `PENDING_CORE_REVALIDATION` |
| `ResultType` | 6 | `IVR_CAPACITY_EXCEPTION`, `IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, `IVR_INVALID_PHONE_FINAL`, `IVR_TECHNICAL_EXCEPTION`, `RESULT_TYPE` |
| `Scope` | 3 | `ADMIN_QUEUE_PAUSE`, `ELIGIBILITY_DEADLINE`, `SCHEDULER_DEADLINE` |
| `SourceType` | 4 | `ELIGIBILITY_DECISION`, `IVR_CALL_RESULT`, `IVR_OPTOUT_PROPOSAL`, `IVR_RESULT_CALLBACK` |
| `Status` | 37 | `ACTIVE_CALL`, `BLOCKED`, `CAPACITY_HELD`, `CAPACITY_MISSED`, `CLOSED_CAPACITY`, `DIALING`, `DISABLED`, `DISPATCH_LEASED`, `DISPOSITION_PENDING_NORMALIZATION`, `HEALTH_FAILED`, `HELD_ADMIN_REVIEW`, `HELD_CALLBACK`, `HELD_CAPACITY`, `HELD_LEASE_RECOVERY`, `HELD_MOCK`, `HELD_NORMALIZATION`, `HELD_TECHNICAL_REVIEW`, `IDLE`, `IVR_CAPACITY_EXCEPTION`, `LEASED`, `LEASED_PENDING_DISPATCH`, `NOT_RUN`, `OPEN`, `PROVIDER_EVENT_PENDING_NORMALIZATION`, `PUBLISHED`, `QUARANTINED`, `QUEUED`, `READY`, `READY_FOR_SCHEDULER`, `RECOVERY_REQUIRED`, `RESERVED`, `RESOLVED`, `RESULT_READY_FOR_CALLBACK`, `RETRY_PENDING`, `SENDING`, `SKIPPED`, `TECHNICAL_RETRY_QUEUED` |

> Nguồn 3 chỉ bắt được gán hằng trực tiếp (`X = "VALUE"`). Giá trị đi qua biến,
> qua migration data, hoặc do nhà cung cấp trả về **không** xuất hiện ở đây.
> Đó là lý do `NT-4` (fallback hiển-lỗi) là bắt buộc, không phải tuỳ chọn.
