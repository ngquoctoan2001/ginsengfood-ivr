# IR-05 — Open Contract Questions and Closure Evidence

Trạng thái: `OPEN` · Cập nhật: `2026-08-12`. Không đóng bằng suy luận hoặc chỉ bằng code IVR.

| ID | Câu hỏi cần trả lời | Owner | Chặn | Evidence để đóng |
| --- | --- | --- | --- | --- |
| `OQ-SALES-01` | Xác nhận Golden Hour ONLINE và 24/7 COD đều tạo task khi `ivr_confirmation_required=true`; callable states cụ thể? | Sales/Product | real producer | signed matrix + tests |
| `OQ-SALES-02` | Chấp nhận path callback Target V1 và ACK taxonomy hay cung cấp phương án thay thế? | Sales API owner | real callback | OpenAPI + contract tests |
| `OQ-SALES-03` | `order_version` có bắt buộc, bump khi nào; stale/idempotency conflict xử lý ra sao? | Sales/Order Core | race safety | implementation tests |
| `OQ-SALES-04` | Schema/sample `privacy_safe_order_summary`, limit item và quy tắc vùng giao rút gọn? | Sales/Product/Privacy | business acceptance | schema + fixtures + approval |
| `OQ-SALES-05` | `dial_token` được issue/resolve ở đâu, TTL/one-use/redemption audit? | Sales/Security/Telephony | real call | threat model + API/tests |
| `OQ-SALES-06` | No-answer timeout worker và revalidation order/state/version hoạt động cụ thể thế nào? | Sales/Product | end-to-end correctness | sequence + tests |
| `OQ-POLICY-01` | Chốt max attempts/window/offset cho hai program | Product/Order Core | production policy | owner decision ID/version |
| `OQ-AUTH-01` | JWT issuer/audience/scopes/TTL/JWKS; mTLS có bắt buộc? | Security/Platform | real integration | auth profile + test credential |
| `OQ-TEL-01` | Protocol/SDK, DTMF, disposition, resolver, caller ID cho 1 SIM lab | Infra/vendor | lab | vendor docs + lab pass |
| `OQ-TEL-02` | 32 eSIM concurrency/capacity/rate/cost/failover | Infra/procurement | production capacity | procurement + load evidence |
| `OQ-LEGAL-01` | Script, legal basis, do-not-call, retention; recording giữ OFF | Legal/Privacy | customer calls | signed review |
| `OQ-REL-01` | Pilot scope, release authority, rollback/kill switch | Release owner | go-live | accepted go/no-go packet |

## Được phép làm trước

Toàn bộ IVR side được xây qua interfaces và deterministic mocks/fakes. Khi thiếu field/API trong lúc implement, phải thêm Work ID tuần tự vào `prompt/_execution/prompt-execution-tracker.md`, thêm/update requirement tại đây hoặc file owner tương ứng, và đánh `BLOCKED_EXTERNAL`; không được tự invent production behavior.

## Câu hỏi gửi dev Sales ngay

1. Gửi OpenAPI/samples của current endpoints và target proposal phản hồi.
2. Cho biết producer Golden Hour hiện enqueue ở điều kiện nào và nơi sẽ thêm producer 24/7 COD.
3. Chỉ ra entity/projection có thể sinh speech summary không lộ full address.
4. Chỉ ra token vault/resolver hiện có hoặc xác nhận cần build mới.
5. Chỉ ra auth middleware/service-account convention hiện tại.
