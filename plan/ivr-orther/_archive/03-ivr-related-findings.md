# 03 — IVR-Related Findings

Toàn bộ phát hiện liên quan IVR. Đây là ghi nhận + phân tích sơ bộ + liệt kê khoảng trống. **Không** thiết kế đầy đủ ở đây.

## 1. Tài liệu nhắc đến IVR

- CONFIRMED: **Nguồn lõi IVR**: `2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`, `3. tech/10-TECH-09-...ANTI-FAKE-ORDER-CONTROL.md`, toàn bộ `4. phase/phase-8/` (00–26), và `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.2_CLEAN_FINAL.docx`.
- CONFIRMED: **Nhắc IVR ở sales**: `phase-3/00` §13.6 (IVR là reserved pack, không triển khai logic IVR trong Commerce), `phase-3.1/07` (connector IVR: risk decision + order_code gating), `phase-3.1/5. bo sung/03` (`IVR_confirmation_extra_time = 5 phút`).
- CONFIRMED: **Nhắc IVR ở governance**: `MASTER-00 §5.9` (IVR config baseline), `MASTER-01 SRC-IVR-001` (IVR Confirmation source), `MASTER-03 §27` (DOMAIN-12 ORDER/IVR traceability IDs), `MASTER-04 §27` (IVR resolvers/guard).

## 2. IVR đã được mô tả ở đâu (mức độ chín)

- CONFIRMED: phase-8 là bộ **SRS_BASELINE (00–09)** + **SDS_BASELINE (10–20)** + gap-closure/traceability (24–26). Đây là mức rất chín — gần như đủ để chưng cất thẳng vào `specs/srs`. Nguồn: header trạng thái từng file phase-8.

## 3. Có workflow IVR nào chưa?

- CONFIRMED: Có. `phase-8/14` mô tả 8 luồng: confirm (phím 1), cancel (phím 0), no-answer theo attempt, invalid phone, technical exception, race condition (phím 1 + Sale Lock), trusted skip, capacity hold. `phase-8/07` khóa result taxonomy + state machine result.

## 4. Có API/webhook nào chưa?

- CONFIRMED: Có API design (`phase-8/11`): nhóm internal (`tasks`, `eligibility-checks`, `call-jobs`, `call-attempts`, `call-results`, `result-callbacks`) + admin (`queue:pause/resume`, `sim-channels:enable/disable`, `technical-retries`, `admin-reviews`) dưới `/v1/ivr/order-confirmation/*`. Có contract `IvrConfirmationTaskV1`, `IvrConfirmationResultCallbackV1`.
- `GAP`: File OpenAPI `openapi/business-platform/ivr-order-confirmation.v1.yaml` được trích nhưng **chưa tồn tại**.
- `NEED_CONFIRMATION`: **Không có telephony webhook provider** theo mô hình mặc định (Internal SIM Gateway, không phải cloud provider). "Webhook cuộc gọi" chỉ xuất hiện nếu chuyển sang provider ngoài (future decision).

## 5. Có telephony/SIM provider nào được nhắc chưa?

- CONFIRMED: Mô hình = **Internal SIM Gateway Server** (SIM nội bộ), `ONE_SIM_ONE_ACTIVE_CALL`, capacity baseline 12/24/32 SIM. SIM adapter capture DTMF + call disposition, **không** có credential ghi order. Nguồn: phase-8/06, /16, PACK-09.
- `Owner Decision Required`: Production SIM Gateway hardware/API protocol, mapping busy/rejected/unreachable từ SIM thật. Nguồn: phase-8/24.

## 6. Có yêu cầu call log / recording / callback chưa?

- CONFIRMED: Call log kỹ thuật (metadata) — có; **recording mặc định TẮT**, chỉ bật khi owner + pháp lý duyệt. Callback = `IvrConfirmationResultCallbackV1` về Order Core (có retry kỹ thuật bounded). Nguồn: phase-8/08, /12 §11, /07 §14.
- `Owner Decision Required`: recording policy, retention duration cho call log/DTMF/recording. `RISK`: recording là PII/privacy.

## 7. Có yêu cầu order qua điện thoại (inbound) chưa?

- CONFIRMED: **KHÔNG**. phase-8/00 §4 ghi rõ "ngoài phạm vi: tạo Quote/Cart/Order Draft/Official Order". IVR không đặt hàng, chỉ xác nhận đơn đã có.
- `NEED_CONFIRMATION`: Nếu brief muốn inbound order-taking → scope mới, chưa có nguồn.

## 8. Có yêu cầu tra cứu trạng thái đơn hàng (khách hỏi) chưa?

- CONFIRMED: **KHÔNG** trong phase-8 (outbound confirm, không phải inbound tra cứu). Tuy nhiên report sales gợi ý các nhu cầu tra cứu (order status/payment/shipping ETA) nếu mở inbound — nhưng đó là **ASSUMPTION của phân tích**, không có trong scope phase-8.
- `NEED_CONFIRMATION`: Tra cứu trạng thái đơn cho khách gọi vào = tính năng inbound, cần owner duyệt.

## 9. Có yêu cầu gặp nhân viên (agent handoff) chưa?

- CONFIRMED (một phần): phase-8/07 có result type `IVR_CUSTOMER_NEEDS_SUPPORT` "nếu future key enabled" — tức đã dự trù nhưng **chưa bật**. `PACK-05 §3.5` có "handoff sang con người" ở kênh AI advisor (không phải IVR). Nguồn: phase-8/07 §5.
- `NEED_CONFIRMATION`: Có bật phím "gặp nhân viên" trong IVR script không, và route tới đâu.

## 10. Các điểm còn thiếu (tổng hợp GAP/Owner Decision)

- `GAP`: `docs/source-map.md`, `ivr-pre-srs-gap-closure.md`, `openapi/.../ivr-order-confirmation.v1.yaml` được trích nhưng chưa tồn tại.
- `Owner Decision Required` (từ phase-8/24 §13 & /25 §5):
  1. Ngưỡng trusted customer (skip IVR).
  2. Risk flags buộc trusted vẫn phải IVR.
  3. Tiêu chí permanent invalid phone.
  4. Technical retry count/backoff.
  5. Mapping tín hiệu SIM thật (busy/rejected/unreachable/dropped).
  6. Recording enabled hay không.
  7. Retention duration từng loại dữ liệu.
  8. Production SIM gateway protocol.
  9. Pilot real customer scope.
  10. Notification template sau khi Core hủy/expire.
- `NEED_CONFIRMATION` (tension): thứ tự IVR ↔ order_code (phase-3.1/07 vs phase-8/00) — xem [02](02-current-understanding.md) §12, [10](10-integration-gap-analysis.md).

## 11. Kết luận sơ bộ

- IVR đã có bộ tài liệu baseline rất đầy đủ (phase-8) → công việc specs chủ yếu là **chưng cất + chuẩn hóa + đóng open decisions**, không phải viết mới.
- Rủi ro lớn nhất không nằm ở nội bộ IVR mà ở **hợp đồng tích hợp với sales/ops** (API chưa hiện thực) và **các Owner Decision còn treo**.
