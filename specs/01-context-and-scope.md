# SRS-01 — Context and Scope

Trạng thái: `TARGET_V1_DRAFT` · Cập nhật: `2026-08-12`.

Nguồn: tài liệu gốc `docs/documents/4. phase/phase-8`, các tài liệu master/pack/tech; current Sales source/OpenAPI đã rà soát; và `docs/contracts/target-v1-closure-pack/README.md`.

## System context

IVR là service outbound riêng (.NET/PostgreSQL/Next.js) nhận task từ Sales Platform (Java), gọi khách qua SIM gateway, đọc tóm tắt đơn privacy-safe, nhận DTMF 1/0 và gửi result signal về Sales. Sales giữ order truth, eligibility, revalidation và mọi state transition.

```text
Sales Core -> versioned IVR task -> IVR API/Worker -> SIM adapter -> customer
Sales Core <- versioned result callback <- IVR normalizer <- DTMF/disposition
```

Không chia sẻ database/source/entity giữa hai service.

## In scope V1

- `GOLDEN_HOUR + ONLINE + ivr_confirmation_required=true`.
- `TWENTY_FOUR_SEVEN + COD + ivr_confirmation_required=true`.
- intake/idempotency, eligibility snapshot, scheduler/policy registry, channel pool, dial/TTS/DTMF/disposition, normalized results, callback/outbox, audit/evidence, admin UI và observability.
- Lời thoại đọc tên ngắn, mã đơn, items + quantity, tổng tiền và vùng giao rút gọn từ `privacy_safe_order_summary`.
- `1` = signal xác nhận; `0` = signal khách hủy. Core mới quyết định transition.
- `NO_ANSWER_FINAL` = advisory/chờ Core timeout; technical exception không tính customer attempt.

## Out of scope V1

- inbound IVR, đặt hàng/tư vấn/upsell, payment processing hoặc xác nhận `PAID`;
- IVR tự sửa/xác nhận/hủy/expire order;
- đọc full address, payment details, history, member tier, health/sensitive notes;
- recording (OFF mặc định);
- SMS/CRM/customer notification;
- key 9/human handoff trừ contract mới được duyệt.

## Runtime modes

| Mode | Dữ liệu/telephony | Call permission |
| --- | --- | --- |
| `MOCK` | fake Sales + mock SIM | không gọi thật |
| `LAB_REAL_SIM` | fake/sandbox Sales + 1 SIM thật | allowlisted test numbers only |
| `PRODUCTION_REAL` | real Sales + target 32 eSIM | sau toàn bộ release gates |

`REAL_CUSTOMER_CALL_ALLOWED=NO` là default. Lab pass không phải production proof.

## Trạng thái contract

- Target V1 chưa khóa: callback, auth, attempt policy, speech payload và dial-token cần owner/Sales/Security xác nhận.
- Current Golden Hour callback chỉ là `CURRENT_COMPAT` adapter.
- Code được phép hoàn tất sau interfaces/mocks. Integration thật và customer calls giữ `BLOCKED_EXTERNAL` tới khi có evidence.
