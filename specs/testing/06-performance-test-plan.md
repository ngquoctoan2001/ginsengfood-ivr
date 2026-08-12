# TEST-06 — Performance / Capacity Test Plan

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p11` · Nguồn: `architecture/04`, `functional/06`; docx §11. Chạy MOCK SIM (chưa mua).

## 1. Capacity baseline (AVG 35s, cycle 50s)
| SIM | ~5′ | ~15′ | ~45′ rolling |
| --- | --- | --- | --- |
| 12 | ~72 | ~216 | ~648 |
| 24 | ~144 | ~432 | ~1.296 |
| 32 | ~192 | ~576 | ~1.728 |

## 2. Ca test
| ID | Given | Then |
| --- | --- | --- |
| PT-01 | 12 SIM, load ≤ năng lực, GH window 300 | attempt đúng hạn, không miss deadline |
| PT-02 | 24/32 SIM, load ~800–1.200/5′ (SCN-015) | rolling queue; **không batch**; vượt năng lực → `capacity_incident` |
| PT-03 | ONE_SIM_ONE_ACTIVE_CALL | không giao trùng SIM |
| PT-04 | SIM fail_count≥3/10′ | auto-disable + alert; giảm capacity phản ánh incident |
| PT-05 | deadline adherence GH (5′) | attempt 2 @ T0+2:30, expire T0+5:00 chính xác |
| PT-06 (neg) | dồn cuộc cuối phiên | FAIL (M8-SCH-001 BATCH_PROHIBITED) |

## 3. Metrics theo dõi (architecture/06)
`call_success_rate`, `no_answer_rate`, `technical_exception_rate`, `missed_deadline_count`, `sim_failure_rate`, `cost_per_confirmed_order`.

## 4. Lưu ý
- Số SIM thật + throughput thật ⏳ chỉ đo được **sau khi mua SIM** (DT-04); hiện đo trên MOCK để kiểm scheduler/queue logic.
- Không đo trên khách thật.

## Báo cáo
6 performance case; baseline 12/24/32 có số; rolling-queue no-batch + capacity incident phủ. Throughput thật chờ mua SIM.
