# FR — Task Intake

Trạng thái: `TARGET_V1_DRAFT` · Actor: Sales Order Core → IVR API.

## Program/payment gate

| Program | Payment | Required flag | Result |
| --- | --- | --- | --- |
| `GOLDEN_HOUR` | `ONLINE` | `ivr_confirmation_required=true` | eligible for validation |
| `TWENTY_FOUR_SEVEN` | `COD` | `ivr_confirmation_required=true` | eligible for validation |
| mọi tổ hợp khác | bất kỳ | bất kỳ | reject fail-closed |

`24_7` chỉ được current-compat adapter normalize thành `TWENTY_FOUR_SEVEN`.

## Requirements

| ID | Yêu cầu | Acceptance |
| --- | --- | --- |
| `FR-IVR-INTAKE-001` | Chỉ caller/auth hợp lệ; `Idempotency-Key` và `X-Correlation-Id` bắt buộc | 401/403/422 tương ứng |
| `FR-IVR-INTAKE-002` | Validate `contract_version`, official order identity, required flag, program/payment matrix, callable/window/version | invalid/stale reject, không tạo job |
| `FR-IVR-INTAKE-003` | Validate policy version/max/offsets theo registry; candidate chỉ MOCK/LAB | unknown/unapproved policy held/rejected |
| `FR-IVR-INTAKE-004` | Validate phone refs/dial token/expiry; không nhận raw phone | PII leak/schema violation reject |
| `FR-IVR-INTAKE-005` | Validate `privacy_safe_order_summary`, không full address, có items/total/area | invalid speech payload reject/hold |
| `FR-IVR-INTAKE-006` | Validate call restriction + eligibility/evidence snapshot fail-closed | unavailable/blocked không dispatch |
| `FR-IVR-INTAKE-007` | Same key+same payload replay response; same key+different payload conflict | không tạo job trùng |
| `FR-IVR-INTAKE-008` | Persist privacy-safe snapshot, policy version, source baseline và audit | traceable without raw PII |
| `FR-IVR-INTAKE-009` | Mode controls dispatch: MOCK no real call; LAB allowlist only; PROD requires release gates | negative tests pass |

Fake Sales provider phải cung cấp happy/negative fixtures cho cả hai program, stale/duplicate, missing speech, PII violation, invalid dial-token, block/opt-out và dependency outage.
