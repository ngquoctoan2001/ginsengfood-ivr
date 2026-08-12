# 14 — Risk Register

> **Target V1 update 2026-08-12:** ưu tiên hiện tại gồm Sales producer cho hai program, speech summary, dial-token, generic callback/version/ACK, auth và owner attempt policy; lab dùng 1 SIM thật/allowlist, production target 32 eSIM. Các risk/closure cũ bên dưới là lịch sử nếu mâu thuẫn với `target-contract-v1-draft.md`.

| ID | New/changed Target V1 risk | Priority | Mitigation/owner |
| --- | --- | --- | --- |
| `R-V1-01` | Golden Hour ONLINE/24-7 COD producer matrix chưa khóa/implement đủ | P0 | Target task OAS + fake producer; Sales/Product sign-off |
| `R-V1-02` | Không có speech payload nên lời gọi không đọc được đơn theo yêu cầu | P0 | required privacy-safe summary + fixtures; Sales/Product/Privacy |
| `R-V1-03` | Dial-token/resolver chưa có | P0 | token port/fake; Security/Telephony contract |
| `R-V1-04` | Generic callback/version/ACK/auth chưa có | P0 | target client/WireMock + current compat; Sales/Security evidence |
| `R-V1-05` | D-10 candidate bị hard-code rồi dùng production | P0 | versioned registry + PROD approval guard; Product owner |
| `R-V1-06` | Một SIM lab bị hiểu nhầm là 32-eSIM production proof | P0 | separate gates/evidence; Infra/Release |
| `R-V1-07` | Legacy prompt/seed làm dev quay lại COD-only/current behavior | P0 | source priority, legacy labels, CI contract tests |
| `R-V1-08` | Tracker thiếu việc phát sinh làm mất kiểm soát scope | P0 | single sequential ledger; every prompt updates it |

Impact/Probability: Cao/Trung bình/Thấp. Priority: P0 (chặn) / P1 / P2.

> ✅ **Cập nhật 2026-07-02 (Module 3/3.1 trả lời — [decisions-log.md](decisions-log.md)):** giảm/đóng: **R-02** (order_code → D-01), **R-04** (order status → D-02), R-03 phần dial token (D-05), R-13 (race guard `order_version` → D-02/D-04). R-01 (chưa có API sales) hạ xuống "đã có hợp đồng, chờ hiện thực". Còn P0: R-06 (SIM protocol), R-16 (technical≠no-answer), R-09 (PII), R-21 (release gate).
>
> ✅ **Ops-Core cũng đã trả lời (DO-01..DO-09):** R-11 (ops down) & R-12 (blocker realtime) **được mitigate** — có sellable gate `availability/check` + fail-closed qua `/health/ready` (DO-06) + Core revalidate (DO-03). ✅ **CRM đã trả Q-C1/DC-01:** do-not-call/opt-out không thuộc ops nhưng đã có nguồn Customer Identity; còn IR-CRM-01 P1 cho rich response/Core wiring.

| ID | Rủi ro | Impact | Prob | Priority | Mitigation | Owner đề xuất | Specs liên quan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| R-01 | **Chưa có API thật từ sales platform** (task/callback/IVR-required chỉ SRS) | Cao | Cao | P0 | Mock qua adapter/port; gửi integration-requirements sớm; contract-first | Sales/Order Core | api/05, integration-requirements/01 |
| R-02 ✅ | ~~Tension thứ tự IVR ↔ order_code~~ **ĐÃ ĐÓNG** | Cao | Thấp | ~~P0~~ đóng | ✅ RESOLVED **D-01**: order_code cấp khi tạo Official Order; `CONFIRMATION_REQUIRED/IVR_PENDING`; fulfillment gated | Order Core | decisions-log D-01 |
| R-03 | **Chưa thống nhất customer identity** (phone vs customer_id; projection) | Cao | TB | P0 | Chốt official contact projection + dial token; không hardcode | Customer/Commerce | data/02,05 |
| R-04 ✅ | ~~Chưa thống nhất order status~~ **ĐÃ ĐÓNG** | Cao | Thấp | ~~P0~~ đóng | ✅ RESOLVED **D-02**: `order_state` đục + `order_version` + `is_ivr_callable`; transition do Core | Order Core | decisions-log D-02 |
| R-05 | **Chưa thống nhất delivery/payment status** (nếu mở inbound) | TB | TB | P2 | Chỉ khi mở inbound; đến lúc đó chốt mask/whitelist | Sales | (khi mở scope) |
| R-06 | **Chưa rõ telephony/SIM provider protocol** | Cao | Cao | P0 | Owner quyết protocol; dry-run trước; adapter tách biệt | IVR Infra + Owner | api/04, architecture/04 |
| R-07 | **Webhook/telephony gửi trùng** | TB | Cao | P1 | Idempotency-key bắt buộc; dedup; trả ack cũ | IVR | api/07-idempotency |
| R-08 | **Call recording rủi ro privacy** | Cao | TB | P1 | Recording OFF mặc định; consent+legal trước khi bật; retention rõ | Owner + Legal | database/05, security |
| R-09 | **Dữ liệu khách là PII** (phone/profile) | Cao | Cao | P0 | phone_ref/masked/token; cấm raw phone/full profile trong log/UI | Privacy owner | data/05, ui/* |
| R-10 | **Sales platform down** | Cao | TB | P0 | Fail-safe: không tạo/không tiếp task; admin review; callback retry bounded | IVR + Order Core | architecture/05 |
| R-11 | **Ops-core down** (blocker check unavailable) | Cao | TB | P0 | Không dispatch khi blocker check down (fail-closed) | IVR + Ops | architecture/05 |
| R-12 | **Tồn kho / blocker không realtime** (snapshot cũ) | Cao | TB | P0 | Core revalidate realtime khi callback; không chỉ dựa snapshot task | Order Core + Ops | data/03, workflows/06 |
| R-13 | **Trạng thái đơn không đồng bộ** (race, order_version mismatch) | Cao | TB | P0 | `order_version_seen_by_ivr` race guard; stale → no transition | Order Core | workflows/06, api/05 |
| R-14 | **Chưa có môi trường test tích hợp** | TB | Cao | P1 | Seed/mock + dry-run mode; INTEGRATION_MODE flag | IVR | seed/*, testing/* |
| R-15 | **IVR bị thiết kế phụ thuộc quá chặt vào sales platform** | Cao | TB | P1 | Adapter/port + contract; IVR chỉ consume signal; không ôm logic sales | IVR Architect | architecture/02,03 |
| R-16 | **Nhầm technical failure với no-answer** (FAIL P0) | Cao | TB | P0 | `is_counted_customer_attempt=false` cho technical; mapping disposition rõ | IVR | functional/06 |
| R-17 | **IVR tự ý update order / gửi notification** (FAIL P0) | Cao | Thấp | P0 | Không endpoint/đường ghi order; V1 notification disabled/no-egress | IVR Architect | api/*, architecture/03 |
| R-18 | **Open decisions treo trôi vào code** (trust threshold, retention, SIM protocol…) | Cao | Cao | P0 | p14 duy trì open-decisions-register; chặn code tới khi P0 đóng | Owner | _review/open-decisions-register |
| R-19 | **Lệch version tài liệu** (.docx V0.2 vs md; file tham chiếu thiếu) | TB | TB | P1 | p01 đối chiếu; ghi ADR chọn version; tạo source-map | IVR + Owner | 05-current-docs-review, decisions/ |
| R-20 | **Scope creep sang inbound** (lookup/order-by-phone/tư vấn) | TB | TB | P1 | Khóa scope outbound; inbound chỉ khi owner duyệt + có nguồn | Product owner | context/scope |
| R-21 | **Release gate bị bỏ qua / tuyên bố production-ready sớm** (FAIL) | Cao | TB | P0 | `REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi gate pass; evidence/owner sign-off | Release owner | testing/*, phase-8/09 |

## Nhóm rủi ro lớn nhất (P0 nổi bật)
1. Hợp đồng tích hợp chưa hiện thực (R-01) + order status/identity chưa chốt (R-03,R-04).
2. Tension order_code (R-02).
3. Telephony/SIM protocol (R-06) + technical≠no-answer (R-16).
4. Privacy/PII & recording (R-09, R-08).
5. Fail-safe & realtime blocker (R-10,R-11,R-12,R-13).
6. Giữ boundary IVR (R-17) + đóng open decisions (R-18) + release gate (R-21).
