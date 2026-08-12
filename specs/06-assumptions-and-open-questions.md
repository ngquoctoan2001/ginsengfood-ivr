# SRS-06 — Assumptions and Open Questions

Trạng thái: `LIVING` · Cập nhật: `2026-08-12`. Register chi tiết: [_review/open-decisions-register.md](_review/open-decisions-register.md).

## Assumptions được phép dùng khi dev

| ID | Assumption | Phạm vi |
| --- | --- | --- |
| `AS-V1-01` | .NET service riêng; Sales Java giao tiếp qua OpenAPI/HTTP | implementation |
| `AS-V1-02` | Program matrix = Golden Hour ONLINE và 24/7 COD, đều có IVR-required flag | mock/target contract; chờ Sales sign-off |
| `AS-V1-03` | Candidate attempts = 2; GH 300/[0,150], 24/7 900/[0,450] | chỉ `MOCK`/`LAB_REAL_SIM`; không production |
| `AS-V1-04` | Dev auth = mock JWT; prod = short-lived service JWT | mock; production profile còn mở |
| `AS-V1-05` | 1 SIM thật cho lab; target 32 eSIM | owner direction; vendor details còn mở |
| `AS-V1-06` | recording OFF; V1 notification disabled | mọi mode |

Assumption phải nằm trong config/policy/provider abstraction và có test; không hard-code thành production truth.

## Open P0 cho integration/acceptance

- Sales producer đủ hai program + callable states/flag.
- Target callback path/DTO/ACK/idempotency/version/revalidation.
- `privacy_safe_order_summary` schema/examples/privacy approval.
- dial-token issuer/resolver/TTL.
- auth production + quyết định mTLS.
- owner chốt attempt policy.
- telephony protocol/DTMF/disposition cho lab và 32 eSIM capacity cho production.
- legal/privacy/retention/release sign-off.

## Quy tắc xử lý

Không đóng bằng suy luận. Khi thiếu dữ liệu/API trong lúc prompt chạy: tạo Work ID tiếp theo trong tracker, ghi owner/impact/mock fallback/evidence cần có, tiếp tục phần có thể build; không tự invent production behavior.
