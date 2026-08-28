# IVR Order Confirmation — Plan Index (canonical)

Trạng thái: `LIVING` · Cập nhật: `2026-08-12` · Module: **IVR Order Confirmation**.

## 1. Thứ tự nguồn điều khiển

1. [target-contract-v1-draft.md](target-contract-v1-draft.md) — Target V1 để build song song; còn các owner/external gates.
2. [decisions-log.md](decisions-log.md) — lịch sử quyết định và current-compat; `TV1-*` supersede các giả định cũ khi mâu thuẫn.
3. `specs/api/openapi/*` — machine-readable contract; Target Sales callback vẫn là draft cho tới khi hai team ký.
4. `specs/*`, `integration-requirements/*`, rồi `prompt/*`.
5. `docs/documents/*` — tài liệu gốc của business, không sửa; dùng để truy nguồn.

Không được tuyên bố `CONTRACT_LOCKED`, `PRODUCTION_READY` hoặc “chỉ cấu hình là chạy” khi các gate ngoài còn mở.

## 2. Deliverables đang dùng

| Nơi | Nội dung |
| --- | --- |
| `specs/` | SRS/SDS, workflow, API/OpenAPI, data, database, architecture, testing, UI. |
| `integration-requirements/` | Contract/API/data/auth/SIM cần các team khác cung cấp. |
| `seed/` | Fake Sales/SIM data cho `MOCK`; phải bám Target V1 DTO. |
| `prompt/` | Chuỗi prompt triển khai .NET/Next.js từ foundation đến release. |
| `prompt/_execution/prompt-execution-tracker.md` | **Sổ tiến độ duy nhất**, bao gồm planned và unplanned theo thứ tự phát sinh. |

## 3. Kiến trúc và scope

- IVR: service riêng, backend .NET 10, PostgreSQL, admin Next.js, Docker/Kubernetes.
- Console identity: Ivr.Api + PostgreSQL sở hữu account/session; Admin quản lý
  account, Operator có đúng self-profile + queue view + SIM disable + manual retry.
- Sales Platform: Java/Spring Boot + Next.js; giao tiếp qua versioned API, không chia sẻ DB/source.
- Program V1: Golden Hour ONLINE và 24/7 COD theo ma trận `TV1-01`.
- Dev trước bằng fake Sales provider + mock telephony; kiểm thử lab bằng 1 SIM thật/allowlist; target sau này 32 eSIM channels.
- V1 không gửi SMS/notification.

## 4. Các gate còn mở

- Sales: task producer đủ hai program, generic callback + ACK, `order_version`, speech summary, dial-token, timeout/revalidation và OpenAPI/sandbox.
- Owner: attempt policy D-10 và policy gọi.
- Security/Platform: auth production và mTLS.
- Telephony: protocol/SDK, DTMF/disposition, 1 SIM lab rồi 32 eSIM capacity.
- Legal/Privacy/Release: nội dung lời thoại, retention, allowlist/pilot và go-live sign-off.

Các mục trên **không chặn build sau ports/mocks**, nhưng chặn integration thật hoặc production tương ứng. Xem [production-blockers-plan.md](production-blockers-plan.md) và `integration-requirements/05-open-contract-questions.md`.

## 5. Lịch sử

`_archive/`, các file `questions-to-*` và `prompt/_legacy-mock/` là lịch sử/reference, không phải nguồn điều khiển implementation mới. Phiếu [questions-to-module-3-od15-risk-evidence.md](questions-to-module-3-od15-risk-evidence.md) là `SUPERSEDED` bởi `OD-18`/`W-0123`; Module 3 không còn nợ trust/risk-evidence field cho IVR tự skip. Lượt rà soát `W-0123` và phần khắc phục của nó nằm ở [W-0124-w0123-review-remediation-plan.md](W-0124-w0123-review-remediation-plan.md); `W-0124` xoay baseline so sánh OpenAPI sang `draft.20` nhưng **giữ** baseline `draft.2` và báo cáo chuyển tiếp của nó làm lịch sử audit. Phiếu đang hoạt động gửi Module 3 là [questions-to-module-3-od18-authority.md](questions-to-module-3-od18-authority.md) (`W-0125`). Nhãn dùng: `CURRENT_COMPAT`, `TARGET_DRAFT`, `OWNER_DECISION_REQUIRED`, `BLOCKED_EXTERNAL`, `IMPLEMENTED`, `VERIFIED`.
