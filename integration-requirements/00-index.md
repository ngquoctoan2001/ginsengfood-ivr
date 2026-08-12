# Integration Requirements — Index (IVR Order Confirmation)

Trạng thái: `TARGET_V1_DRAFT` · Cập nhật: `2026-08-12`.

## Mục đích

Đây là contract pack gửi các team để IVR .NET có thể tích hợp với Sales Platform Java, Telephony và Foundation. Tài liệu phân biệt rõ:

- `CURRENT_COMPAT`: source/API hiện có, dùng tạm sau adapter;
- `TARGET_DRAFT`: hai bên có thể build song song nhưng chưa được coi là khóa;
- `OWNER_DECISION_REQUIRED`: không được tự chọn cho production;
- `BLOCKED_EXTERNAL`: không chặn code sau mock, nhưng chặn integration/vận hành thật.

Nguồn điều khiển: `plan/ivr-orther/target-contract-v1-draft.md` → `decisions-log.md` → OpenAPI/spec.

## Cấu trúc

| File | Owner | Nội dung |
| --- | --- | --- |
| [01-sales-platform-requirements.md](01-sales-platform-requirements.md) | Sales/Order Core | producer, speech data, dial-token, callback/revalidation, timeout |
| [02-ops-core-requirements.md](02-ops-core-requirements.md) | Ops-Core qua Sales Core | sellable/recall/sale-lock evidence; IVR không gọi trực tiếp |
| [03-telephony-sim-requirements.md](03-telephony-sim-requirements.md) | Telephony/Infra | mock → 1 SIM lab → 32 eSIM target |
| [04-shared-auth-audit-requirements.md](04-shared-auth-audit-requirements.md) | Security/Foundation | auth, RBAC, audit, retention, release gates |
| [05-open-contract-questions.md](05-open-contract-questions.md) | các owner | câu hỏi còn mở và acceptance evidence cần trả |

## Mock-first rule

Mọi external dependency phải có port + deterministic fake provider + failure scenarios. Điều này cho phép đạt `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS`, nhưng không biến `BLOCKED_EXTERNAL` thành `VERIFIED`.

Ba mode: `MOCK`, `LAB_REAL_SIM`, `PRODUCTION_REAL`. `REAL_CUSTOMER_CALL_ALLOWED=NO` cho tới release gate.
