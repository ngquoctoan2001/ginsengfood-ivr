# FR — Scheduler & Attempt Policy

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p03`
Nguồn: `phase-8/05` (chính sách gọi lại/scheduler/queue), `docx` §8 (attempt policy), §10 (SIM gateway), §11 (capacity), §12 (scheduler rules).

✅ **Attempt policy đã KHÓA — D-10 (Module 3.1 xác nhận 2026-07-02).** Dùng rule mới PACK-09 V1.0. Rule cũ (2/10 & 3/15) **bị thay thế, không áp dụng**. Xem `plan/ivr-orther/decisions-log.md` D-10 và `questions-to-module-3-and-3.1.md` Q10.

**Actor:** IVR Call Scheduler (deadline-aware rolling queue) + SIM Channel Manager.
**Precondition:** Eligibility PASS.
**Trigger:** Tạo CallJob.
**Postcondition:** Attempt được dispatch đúng thời điểm; không vượt max; không dồn cuối phiên.

## Attempt policy (D-10 — LOCKED)
| Program | Window | Attempt 1 | Attempt 2 | Expire | Max |
| --- | --- | --- | --- | --- | --- |
| Giờ Vàng (GOLDEN_HOUR) | 5 phút (300s) | T0 | T0 + 2:30 (nếu A1 no-answer) | T0 + 5:00 | 2 |
| 24/7 (TWENTY_FOUR_SEVEN) | 15 phút (900s) | T0 | T0 + 7:30 (nếu A1 no-answer) | T0 + 15:00 | 2 |
- CONFIRMED (D-10): `MAX_ATTEMPT_PER_ORDER = 2` cho **cả hai** program; `ATTEMPT_INTERVAL = ½ confirmation_window`. Nếu A1 có kết quả cuối → không gọi A2. Nguồn: docx §8; Module 3.1 xác nhận.
- CONFIRMED (D-10): **`T0` = thời điểm Order Core mở IVR confirmation window / tạo task**, KHÔNG phải timestamp thô lúc khách bấm đặt nếu task bị delay. → scheduler tính A1/A2/expire theo `T0` này.

> Rule cũ phase-8 md (Giờ Vàng 2 cuộc/10 phút; 24/7 **3 cuộc**/15 phút) đã **bị thay thế** bởi D-10 — không dùng.

## FR
| ID | Yêu cầu | Nguồn | Acceptance hint |
| --- | --- | --- | --- |
| FR-IVR-SCH-001 | `SCHEDULER_MODEL = DEADLINE_AWARE_ROLLING_QUEUE`; **cấm** batch cuối phiên | docx §12 M8-SCH-001/002/003 | FIFO/batch làm trễ Giờ Vàng → FAIL (P0) |
| FR-IVR-SCH-002 | Ưu tiên: (1) đơn sắp hết window, (2) Giờ Vàng, (3) attempt 2 đúng hạn, (4) risk cao, (5) còn thời gian | docx §12 | Thứ tự dispatch đúng ưu tiên |
| FR-IVR-SCH-003 | `ONE_SIM_ONE_ACTIVE_CALL`; không giao trùng SIM | docx §10, §12 M8-SCH-004, P0-02 | Giao trùng SIM → FAIL |
| FR-IVR-SCH-004 | `SIM_COOLDOWN_AFTER_CALL = 5s` + health check; `fail_count ≥ 3 / 10 phút` → auto disable + alert | docx §10, §12 M8-SCH-005/006 | SIM lỗi vẫn dispatch → FAIL |
| FR-IVR-SCH-005 | Không tạo attempt vượt `MAX_ATTEMPT`; attempt 2 chỉ khi A1 no-answer (không phải final) | docx §8; phase-8/12 constraint | GH/24-7 không có attempt vượt max |
| FR-IVR-SCH-006 | Attempt customer-counted tách khỏi technical retry (`is_counted_customer_attempt`) | phase-8/12 §6; docx §15 | Technical retry không tăng attempt_number |
| FR-IVR-SCH-007 | Nếu không thể dispatch trước expiry do capacity → mở capacity incident, không im lặng | docx §11 CAPACITY GUARD | Miss deadline không log → FAIL |
| FR-IVR-SCH-008 | Hết window chưa có xác nhận hợp lệ → result `IVR_CONFIRMATION_WINDOW_EXPIRED`; Order Core xử lý | docx §8,§13 | Window expired → signal, Core decides |

## Capacity baseline (docx §11)
`AVG_CALL_DURATION=35s`, `CONSERVATIVE_CYCLE=50s/cuộc/SIM`. 12 SIM ≈ 72/5′, 216/15′; 24 SIM ≈ 144/432; 32 SIM ≈ 192/576. Roadmap: pilot 12 → launch 24–32 → 64 → 96 theo volume. `Owner Decision Required` OD-05 (số SIM launch).
