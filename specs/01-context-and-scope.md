# SRS-01 — Context & Scope

Trạng thái: `SRS_DRAFT` · Sinh bởi: `plan/ivr-orther/prompts/p02-generate-context-and-scope.md`
Module: IVR Order Confirmation (`ivr-order-confirmation`; working name `ivr-orther`) — PACK-09 / TECH-09 / phase-8 / Module 8.

## 1. Nguồn tham chiếu
| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/4. phase/phase-8/00-QUẢN TRỊ NGUỒN SỰ THẬT VÀ PHẠM VI.md` | Scope IN/OUT, source-of-truth, governance gates |
| `docs/documents/4. phase/phase-8/01-MỤC ĐÍCH KINH DOANH VÀ CA SỬ DỤNG XÁC NHẬN.md` | Use case xác nhận |
| `docs/documents/4. phase/phase-8/02-RANH GIỚI SỞ HỮU VÀ HỆ THỐNG KẾT NỐI.md` | Ranh giới kết nối |
| `docs/MODULE_8_...V0.2_CLEAN_FINAL.docx` §0–2, §18 | Bản consolidated mới nhất |
| `specs/srs/05-current-docs-review.md` | Mâu thuẫn C-01..C-08 |

## 2. Bối cảnh hệ thống (System Context)

- CONFIRMED: IVR Order Confirmation thuộc **ginsengfood-business-platform**, là **downstream consumer** của Commerce Order Core (module 3/3.1), Operational Core (module 1/2) và các kênh khác. Nguồn: docx §1, phase-8/02.
- CONFIRMED: IVR là hợp phần **gọi tự động OUTBOUND xác nhận Official Order** qua **Internal SIM Gateway Server**, gồm: task intake → eligibility → scheduler/queue → SIM call + DTMF → result normalizer → callback về Order Core. Nguồn: docx §3, phase-8/00.
- CONFIRMED: **IVR result là input signal**; **Order Core** là lớp quyết định trạng thái đơn cuối cùng (revalidate rồi transition). IVR không tự đổi order state. Nguồn: phase-8/00 §5, docx §14.

```text
[Order Core] --IvrConfirmationTaskV1--> [IVR Runtime] --dial--> [Internal SIM Gateway] --DTMF 1/0--> [IVR]
[IVR] --IvrConfirmationResultCallbackV1(signal)--> [Order Core] --revalidate--> transition
[Operational Core] --sale-lock/recall/suppression--> (blocker consumed by IVR/Order Core)
[Evidence Registry/Audit] <--evidence/audit-- [IVR];  [Admin/Ops Console] --RBAC actions--> [IVR]
```

## 3. Trong phạm vi (IN SCOPE)

CONFIRMED (phase-8/00 §4, docx §2):
- Gọi xác nhận **Official Order đủ điều kiện** bằng cuộc gọi tự động.
- Ghi nhận **phím `1`** = xác nhận tiếp tục xử lý đơn; **phím `0`** = khách hủy/không đặt.
- Ghi nhận **no-answer** theo attempt policy; **invalid phone** theo policy; **technical exception** (tách khỏi no-answer); **window expired**.
- Gửi **callback** kết quả (signal) về Order Core để revalidate.
- Ghi **audit/evidence** cho task, attempt, result, callback, admin action, incident.
- Admin/Ops: monitor queue/capacity/SIM health, pause/resume, disable/enable SIM, technical retry, review — theo RBAC + audit.

## 4. Ngoài phạm vi (OUT OF SCOPE)

CONFIRMED (phase-8/00 §4, docx §2):
- Tạo Quote / Cart / Order Draft / Official Order; tự hủy đơn.
- Sửa giá, tồn kho, chương trình, quyền lợi thành viên, phí ship, payment, MISA.
- Tự xác nhận `PAID` / `COD_VERIFIED` / `DELIVERED` / `VERIFIED_REVENUE` / commission / ROAS / payout.
- Tự transition order state; SIM Gateway ghi order.
- Tự gửi SMS/notification sau mỗi attempt hay sau no-answer max (notification chỉ do owner khác sau khi Core quyết).
- Marketing, upsell, cross-sell, tư vấn sản phẩm, đọc combo/chương trình, mời member/Diamond, CRM/chăm sóc đại trà.
- Đọc/log full profile, full address, payment detail, order history, health note, CRM/AI content.

## 5. Ranh giới scope cần xác nhận

- ✅ CONFIRMED (D-08, Module 3 xác nhận 2026-07-02): **Giữ scope outbound-only.** Nhóm **inbound** (tra cứu đơn theo số, đặt hàng qua điện thoại, gặp nhân viên, tư vấn) = **future scope**, chưa làm; chỉ mở khi có Owner Decision + tài liệu mới. Nguồn: decisions-log D-08, `05-current-docs-review` §10.
- `NEED_CONFIRMATION` (KEY_9 gặp CSKH): phím `9` "human support" = **NOT_ENABLED** giai đoạn đầu (docx §9, M8-OD-006). Có bật hay không do owner.

## 6. Mô hình triển khai & governance gates

- CONFIRMED: Deployment = **INTERNAL_SIM_GATEWAY_SERVER**, `ONE_SIM_ONE_ACTIVE_CALL`; Cloud IVR/SIP/Voice Brandname KHÔNG mặc định (future owner decision). Nguồn: docx §10, §22 P0-01.
- ⏳ **SIM gateway CHƯA MUA (sẽ mua)** — thiết kế **adapter port** (`dial/play_script/capture_dtmf/call_disposition/health`) độc lập protocol; p05–p08 + smoke chạy với **mock/dry-run**; protocol/số SIM/caller-ID điền khi mua (DT-01/DT-04/DT-06). `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ nguyên. Xem `plan/ivr-orther/decisions-log.md` DT-*.
- CONFIRMED: `REAL_CUSTOMER_CALL_ALLOWED = NO`, `IVR_GATE = BLOCKED`, `PRODUCTION_READY = NO` cho tới khi smoke + evidence + security/privacy review + owner sign-off PASS. Nguồn: docx §0, §25; phase-8/00 §13.
- CONFIRMED: Governance state gates (mặc định `NO`, mở theo điều kiện): `IVR_DOCS_APPROVED`, `IVR_CONTRACT_APPROVED`, `IVR_TASK_INTAKE_ENABLED`, `IVR_SCHEDULER_ENABLED`, `IVR_SIM_INTERNAL_TEST_ENABLED`, `REAL_CUSTOMER_CALL_ALLOWED`, `DOWNSTREAM_IVR_DEPENDENCY_ALLOWED`. Nguồn: phase-8/00 §13.

## 7. Quyết định đã khóa (tham chiếu [06-assumptions-and-open-questions.md](06-assumptions-and-open-questions.md) + `plan/ivr-orther/decisions-log.md`)
- ✅ Attempt policy: **rule mới PACK-09 V1.0 — ĐÃ KHÓA (D-10)**: 2 cuộc cả hai program; GH 5′ (T0/T0+2:30), 24/7 15′ (T0/T0+7:30); **T0 = lúc Core mở window/tạo task**.
- ✅ Scope = **outbound confirmation only** (D-08); inbound = future.
- ✅ order_code (D-01): cấp khi tạo Official Order; state chờ IVR = **`CONFIRMING`** (DS-01); fulfillment khóa (shipment cần `CONFIRMED`) tới khi IVR confirm.
- ✅ Order state (D-02+DS-01): `order_status` thật `CONFIRMING/CONFIRMED/…/EXPIRED`; `is_ivr_callable` Core derive; transition do Core.
- 🆕 **COD-only (DS-01):** IVR **chỉ** áp dụng cho đơn **COD** (`payment_method_snapshot=COD`) ở state `CONFIRMING`. Đơn prepaid/non-COD → **không** IVR. (Hợp lý: xác nhận chống đơn ảo chủ yếu cho COD.)
