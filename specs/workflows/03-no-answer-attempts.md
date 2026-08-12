# Workflow — No-Answer & Attempts

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p04` · Nguồn: `docx` §8,§13; `phase-8/05`,`/07`.
Attempt policy: ✅ **D-10 (LOCKED)** — max 2 cả hai program; `T0`=lúc Core mở window.

**Kết quả:** A1 no-answer → `IVR_NO_ANSWER_ATTEMPT` (counted, không final) → A2 theo interval → nếu vẫn no-answer → `IVR_NO_ANSWER_FINAL` (final) → Core xử lý (reason `IVR_NO_ANSWER_AFTER_2_ATTEMPTS`).

> ⚠️ **Thực tế Core (DS-02):** no-answer-final **KHÔNG** làm Core transition order. Order ở `CONFIRMING` **chờ `timeout → EXPIRED`** khi qua `expires_at`. `recommended=CORE_..._CANCEL_NO_ANSWER` là **advisory** và explicit no-answer transition là **target** (IR-SALES-OC3) — Core hiện chưa hủy chủ động vì no-answer.

## Lịch attempt (D-10)
| Program | A1 | A2 (nếu A1 no-answer) | Expire |
| --- | --- | --- | --- |
| Giờ Vàng | T0 | T0 + 2:30 | T0 + 5:00 |
| 24/7 | T0 | T0 + 7:30 | T0 + 15:00 |

```mermaid
sequenceDiagram
    participant OrderCore
    participant Sched
    participant SIM
    participant Norm
    participant OrderCore2 as OrderCore
    Sched->>SIM: attempt 1 @ T0
    SIM-->>Sched: no answer (ring timeout)
    Sched->>Norm: normalize -> IVR_NO_ANSWER_ATTEMPT (counted, not final)
    Note over Sched: schedule attempt 2 @ T0 + interval (½ window)
    Sched->>SIM: attempt 2
    alt A2 answered + DTMF
        SIM-->>Sched: DTMF 1/0
        Note over Norm: -> IVR_CONFIRMED / IVR_CUSTOMER_CANCELLED (final)
    else A2 no answer
        SIM-->>Sched: no answer
        Sched->>Norm: normalize -> IVR_NO_ANSWER_FINAL (final)
        Norm->>OrderCore2: callback (IVR_NO_ANSWER_FINAL, recommended=CORE_REVALIDATE_AND_CANCEL_NO_ANSWER)
        Note over OrderCore2: DS-02 hiện tại: KHÔNG transition — order chờ timeout→EXPIRED<br/>(advisory only; explicit cancel = target IR-SALES-OC3)
    end
```

**P0:** Không tạo attempt vượt `MAX_ATTEMPT=2` (FR-IVR-SCH-005). No-answer final KHÔNG tự notification (P0-IVR-008). Hết window trước A2 → `IVR_CONFIRMATION_WINDOW_EXPIRED`.
