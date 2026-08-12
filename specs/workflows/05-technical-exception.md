# Workflow — Technical Exception

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p04` · Nguồn: `docx` §15; `phase-8/07`,`/16`.

**Kết quả:** `IVR_TECHNICAL_EXCEPTION` (NOT counted) → technical retry bounded HOẶC admin review. **Lỗi kỹ thuật ≠ khách không nghe** (P0-IVR-004).

```mermaid
sequenceDiagram
    participant Sched
    participant SIM
    participant Norm
    participant Evid
    participant Admin
    Sched->>SIM: dispatch attempt
    SIM-->>Sched: technical error (SIM/server/DTMF/audio)
    Sched->>Norm: normalize -> IVR_TECHNICAL_EXCEPTION (is_counted_customer_attempt=false)
    Norm->>Evid: evidence(technical exception + error_code)
    alt technical retry allowed (bounded, OD-10)
        Norm->>Sched: technical retry (same idempotency, not new customer attempt)
    else retry exhausted / SIM channel failure
        Norm->>Admin: admin review (+ disable SIM if channel failure)
    end
```

**P0:** Không cộng vào customer attempt count; không map thành `IVR_NO_ANSWER_*`. `INTERNAL_CALLBACK_ERROR` → retry callback bounded cùng idempotency. `Owner Decision Required` OD-10 (retry count/backoff), OD-11 (mapping tín hiệu SIM thật).
