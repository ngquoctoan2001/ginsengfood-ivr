# P4 - PHỤ LỤC KHÓA RUNTIME THỰC CHIẾN AI ADVISOR

## Khóa chống conflict Phase 4

Phụ lục này là lock thực thi ngắn trong bộ Phase 4, nhưng không override `12-BẢNG GOM GIAI ĐOẠN 4.md`. Nếu phụ lục thiếu chi tiết hoặc khác Bảng Gôm/SRS handoff, áp dụng `12-BẢNG GOM GIAI ĐOẠN 4.md` và `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md`, sau đó xử lý fail-closed.

Các cụm `READY_FOR_OWNER_REVIEW`, `DONE`, `local-ready` hoặc `PASS cục bộ` chỉ là trạng thái hồ sơ cục bộ. Chúng không phải `RUNTIME_IMPLEMENTATION_READY`, không phải `GATEWAY_PASS`, không phải `COMPLETION_PASS`, không phải `RELEASE_READY`, không phải `PRODUCTION_READY` và không phải Go-live.

**Ngày viết lại:** 2026-06-05
**Phạm vi:** Practical lock tự chứa cho Phase 4 sau khi các nguồn runtime, commerce, CRM, finance, evidence, Gateway, Ads và Live đã được nhập thành rule trong bộ file này.
**Trạng thái:** `ADDENDUM_REQUIRED_BEFORE_IMPLEMENTATION`
**Gateway:** `PRODUCTION_BLOCKED`

## 1. Vai trò addendum

Addendum này là checklist khóa ngắn cho dev/Codex/Copilot trước khi chạy các file 00-08. Bản chi tiết nằm ngay trong từng file 00-08 ở phần `Khối nguồn bắt buộc đã nhập vào file này`, gồm nguồn đã nhập, conflict lock, trạng thái evidence và khóa riêng của từng shard.

Trước khi code, dev bắt buộc dùng `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` để tạo SRS/RTM và implementation work package. Nếu chưa có SRS accepted hoặc chưa chứng minh module hiện hữu map tương đương, chỉ được làm analysis/design/gap report.

## 2. Runtime boundary không được vi phạm

AI Advisor được làm:

- Resolve context khách/kênh.
- Tư vấn sản phẩm public-safe.
- Gợi ý combo có lý do.
- Render quote/order text từ runtime.
- Route Messenger/private/CRM/human handoff.
- Ghi DecisionEnvelope, resolver trace, guard trace, evidence.

AI Advisor không được làm:

- Tự tính giá, tồn, quyền lợi, ship, VAT, order_code, paid, verified revenue, commission, ROAS hoặc payout.
- Tự xác nhận sản phẩm chay/vegan nếu proof conflict.
- Tự xác nhận Giờ Vàng/24/7 khi policy version missing.
- Public final quote/PII/payment/order/member/commission/health-sensitive details.
- Mở Gateway production hoặc gọi Completion/Release/Production Ready.

## 3. Conflict phải fail-closed

| Conflict | Practical rule |
| --- | --- |
| 24/7/Giờ Vàng pricing conflict | Chỉ dùng QuoteSnapshot/DailySalesContext có policy version. |
| Golden Hour schedule conflict | Không nói giờ cố định nếu GoldenHourResolver chưa pass. |
| Member/Diamond benefit priority | Không cộng dồn thủ công; QuoteSnapshot quyết định. |
| Vegan/formula conflict | Không claim vegan/chay, không recommend cho khách ăn chay. |
| Message file 3/6 conflict | Chỉ dùng canonical message registry. |
| Page Registry conflict | Không giả định Gateway production. |
| Diamond/commission/payout | Chỉ nói theo Finance/Diamond state; no payout fake. |
| IVR timing conflict | Không gọi IVR implementation/real-call ready. |

## 4. Contracts tối thiểu phải có

Implementation planning không được bắt đầu nếu thiếu contract cho:

- `AIAdvisorRequestEnvelope`
- `ChannelContext`
- `DailySalesContext`
- `CustomerAdvisoryContext`
- `ProductRoleMatrix`
- `QuoteSnapshotConsumerView`
- `OrderRuntimeConsumerView`
- `PaymentPublicResponsePolicy`
- `ShippingETAResponsePolicy`
- `CRMMemberContext`
- `DiamondReferralContext`
- `FinalResponseGuard`
- `TypingDelayPolicy`
- `DecisionEnvelope`
- `Phase4EvidenceItem`

## 5. Customer-facing language lock

Không được gửi ra khách các từ/khái niệm:

- hệ thống
- SKU
- runtime
- resolver
- core
- gateway
- QuoteSnapshot
- internal
- nội bộ
- cost
- supplier
- MISA
- audit
- API
- worker
- ROAS/CPA/campaign internals

Thay bằng ngôn ngữ tự nhiên: "bên Em", "Ginsengfood ghi nhận", "Em kiểm tra theo thông tin hiện tại", "báo giá của Mình", "theo quyền lợi hiện tại của Mình".

## 6. P0 smoke tối thiểu

Phase 4 phải có ít nhất các smoke sau:

| Smoke | Expected |
| --- | --- |
| Khách hỏi hôm nay bán gì | Chỉ sản phẩm public sellable. |
| Khách hỏi giá private | QuoteSnapshot hoặc fail-safe. |
| Khách hỏi giá public | Safe ack + private handoff. |
| Golden Hour policy missing | No hardcode, policy pending. |
| Product sale lock/recall | Không bán/gợi ý trong combo. |
| Vegan conflict | Block vegan recommendation. |
| Member runtime missing | Không bịa hạng/doanh số/ngày còn lại. |
| Diamond commission candidate | Không nói đã ghi nhận/đã chi trả. |
| Bank transfer image | Không nói paid. |
| Shipping info exists | Không hỏi lại địa chỉ, ETA theo runtime. |
| Opt-out/open case | Suppress sales/CRM. |
| Banned language | Guard blocks/rewrite required. |
| Duplicate event | No duplicate side effect. |
| Evidence missing | Gate not PASS. |

## 7. Evidence lock

Mỗi P0 evidence phải có:

- evidence_id
- gate_code
- fixture/test ref
- DecisionEnvelope nếu liên quan AI
- resolver trace nếu có resolver
- guard trace
- quote snapshot nếu có giá
- customer confirmation/order_code nếu có order
- handoff log nếu có Messenger/Gateway
- idempotency key nếu có side effect
- PII/secret mask status
- owner review status

Evidence narrative-only không đủ để PASS.

## 8. Done gate của addendum

Addendum này DONE khi:

1. File 00-08 đã chứa đầy đủ khối nguồn bắt buộc, conflict lock và trạng thái evidence ở đầu file.
2. README Phase 4 trỏ đúng thứ tự đọc.
3. `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` được dùng làm SRS/implementation handoff.
4. Các contract tối thiểu được liệt kê.
5. P0 smoke/evidence không bị gọi PASS trước khi có artifact.
6. Gateway vẫn `PRODUCTION_BLOCKED`.

## 9. Final status

```text
P4_PRACTICAL_RUNTIME_LOCK_READY_FOR_OWNER_REVIEW
P4_DEV_SRS_HANDOFF_REQUIRED
PENDING_EVIDENCE
GATEWAY_PRODUCTION_BLOCKED
NOT_RELEASE_READY
```


