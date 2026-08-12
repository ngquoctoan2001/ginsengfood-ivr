# TEST-08 — Acceptance Criteria & Gates

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p11` · Nguồn: `phase-8/09`; docx §21 (evidence plan), §23 (done/fail gate); `MASTER-05`; DF-03.

## 1. Evidence packet (docx §21) — bắt buộc, phải ACCEPTED
Architecture · Task intake (log reject quote/cart/draft + accept official) · Eligibility (pass/fail từng gate) · Scheduler (attempt timing, no batch, no overdue silent) · SIM (one-sim-one-call, health, auto-disable) · DTMF (1/0/wrong/no-input/timeout/error) · Order Core callback (ack/reject, idempotency, transition-only-from-Core) · Privacy/security (PII masking, access audit, recording retention, RBAC) · Capacity (12/24/32 load, incident) · Release (smoke pass, owner sign-off, real-call permission).

## 2. Done gate (docx §23 — M8-DONE-001..010)
- [ ] Internal SIM Gateway model + SIM channel pool (mock chấp nhận cho non-prod).
- [ ] Call script chuẩn + biến được phép.
- [ ] Phím 1/0; KEY_9 not enabled.
- [ ] Rule GH & 24/7 đúng window/attempt (**D-10: 2 attempt cả hai**).
- [ ] Capacity baseline 12/24/32 + incident rule.
- [ ] Core state-machine boundary + callback contract.
- [ ] Technical exception boundary (technical≠no-answer).
- [ ] Admin UI/monitoring/evidence **không** bypass Core.
- [ ] P0 smoke matrix pass bằng **evidence thật**.
- [ ] Owner sign-off trước `REAL_CUSTOMER_CALL_ALLOWED`.

## 3. Fail gate (docx §23 — M8-FAIL-001..008) — FAIL nếu
1. IVR gọi entity không phải Official Order.
2. IVR/SIM tự hủy/xác nhận/chuyển trạng thái đơn.
3. IVR xử lý payment/COD/paid/ORDER_VERIFIED.
4. Lỗi kỹ thuật bị tính là khách không nghe.
5. Scheduler batch cuối phiên / miss deadline không incident.
6. Admin sửa result giả / hủy đơn ngoài Core.
7. PII nhạy cảm trong log/UI/script.
8. Tài liệu/impl tự gọi production-ready khi chưa có evidence.

## 4. Release gate (P0)
- `REAL_CUSTOMER_CALL_ALLOWED = NO` cho tới khi: smoke pass + evidence **ACCEPTED** + security/privacy review + **owner sign-off** (DF-03) + **mua SIM** (DT-01) + pilot scope duyệt.
- Governance state gates mở tuần tự (phase-8/00 §13): DOCS_APPROVED → CONTRACT_APPROVED → TASK_INTAKE_ENABLED → SCHEDULER_ENABLED → SIM_INTERNAL_TEST_ENABLED → REAL_CUSTOMER_CALL_ALLOWED.

## 5. Không hardcode PASS
- Completion report ≠ completion gate pass; evidence submitted ≠ accepted; owner reviewed ≠ signed-off (MASTER-05).

## Điều kiện còn thiếu để mở release gate (hiện tại)
- ⏳ **Mua SIM** (DT-01/04) — chưa có gateway thật để smoke SIM thật.
- ⏳ **DF-07** retention + **DT-05** recording (Legal) cho privacy review.
- ⏳ Owner sign-off (DF-03).
- ✅ **Q-C1/DC-01** đã có nguồn, hết chặn P0; còn IR-CRM-01 P1 build rich fields/Core wiring.
- ✅ **DG-03/DS-01..05** đã trả lời; còn IR-SALES-OC1/OC2/OC3 là target/deferred, không phải P0 release gate.
