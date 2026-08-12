# External Closure Plan — IVR Order Confirmation

Trạng thái: `LIVING` · Cập nhật: `2026-08-12`.

Mục tiêu là tách rõ ba mốc: (1) build hoàn chỉnh sau mocks, (2) lab bằng SIM thật, (3) vận hành khách thật. Không dùng phần trăm ước lượng làm bằng chứng readiness.

## 1. Mốc và gate

| Mốc | Có thể hoàn thành ngay | Còn chặn |
| --- | --- | --- |
| `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS` | .NET API/Worker/domain/DB, Next.js admin, scheduler/dialer/normalizer/callback adapters, fake Sales provider, mock SIM, tests, observability, deploy manifests | không cần Sales/SIM thật để hoàn tất code |
| `LAB_REAL_SIM_VERIFIED` | nối 1 SIM thật, chỉ gọi allowlist test, kiểm tra DTMF/disposition/kill switch | gateway protocol, test SIM, số test được duyệt, lab evidence |
| `PRODUCTION_REAL_ELIGIBLE` | wiring Sales API thật và 32 eSIM config sau nghiệm thu | tất cả contract/auth/policy/legal/security/capacity/release gates |

## 2. Hard dependencies cho Sales integration/business acceptance

| ID | Sales/owner phải cung cấp | IVR chuẩn bị trước | Gate |
| --- | --- | --- | --- |
| `IR-SALES-TASK-V1` | producer cho Golden Hour ONLINE + 24/7 COD, flag `ivr_confirmation_required` | intake port + fake producer | real integration |
| `IR-SALES-SPEECH-V1` | `privacy_safe_order_summary` có tên ngắn, items, tổng tiền, vùng giao rút gọn | DTO, validator, renderer/TTS abstraction, fake data | business acceptance |
| `IR-SALES-DIAL-V1` | `dial_token` issue/resolve, TTL/one-use semantics | token-only storage + resolver port/mock | real call |
| `IR-SALES-CB-V1` | generic callback endpoint + ACK taxonomy + idempotency + revalidation/version | target client + current-compat adapter + WireMock | real integration |
| `IR-SALES-TIMEOUT-V1` | timeout worker/no-answer policy | advisory result + no-transition invariant | end-to-end correctness |
| `IR-AUTH-V1` | issuer/audience/scope/JWKS/TTL; quyết định mTLS | mock JWT + auth abstraction/negative tests | real integration |
| `D-10-OWNER` | attempt policy cuối | config/policy registry; candidate chỉ MOCK/LAB | production |

Đây không phải các mục “soft” nếu mục tiêu là luồng thật đầy đủ. Chỉ việc **viết code IVR** mới có thể tiếp tục nhờ mocks.

## 3. Telephony/SIM/eSIM

### Hiện tại — lab

- 1 SIM thật là đủ cho bước kiểm chứng đầu tiên.
- Chỉ gọi số trong `LAB_DESTINATION_ALLOWLIST`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- Phải có kill switch, one-active-call-per-channel, cooldown, health, DTMF, disposition mapping và audit không lộ số thô.

### Tương lai — vận hành

- Target 32 eSIM channels; channel count và concurrency là config, không hard-code.
- Cần vendor cung cấp protocol/SDK, DTMF mode, health API, disposition semantics, rate/cost, caller ID và secret provisioning.
- Capacity test phải dùng throughput thực đo; không còn mặc định pilot 12 SIM.

## 4. Notification

V1 không gửi SMS/notification. `P4-5` là deferred/no-op extension boundary để chứng minh IVR không phát message. Mọi CRM notification là future contract riêng, không chặn V1.

## 5. Legal, security và release

- Approve script/privacy: không đọc địa chỉ đầy đủ; recording OFF mặc định.
- Chốt retention cho task/attempt/result/audit và evidence.
- Chốt transaction-call legal basis/do-not-call behavior.
- Security review auth, secret rotation và egress allowlist.
- `DF-03` release sign-off chỉ sau integration, lab/pilot, rollback/kill-switch và evidence được chấp nhận.

## 6. Trình tự khuyến nghị

1. Chạy P0–P7 bằng `MOCK`; ghi mọi việc vào tracker duy nhất.
2. Song song gửi Sales contract pack và Telephony lab checklist; đòi dữ liệu còn thiếu ngay khi phát hiện.
3. Khi có 1 SIM/gateway: chạy P8 trong `LAB_REAL_SIM`, allowlist only.
4. Khi Sales endpoint/auth sẵn: chạy P4 real-provider contract tests trên sandbox.
5. Nghiệm thu 32 eSIM/capacity, legal/security/release rồi mới xét `PRODUCTION_REAL`.
