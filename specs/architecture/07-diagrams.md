# ARCH-07 — Diagrams (tổng hợp)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p08`. Diagram context/component/deployment ở `01`,`02`,`04`; workflow/sequence ở `specs/srs/workflows/*`.

## 1. Fail-closed decision (revalidate lúc callback)
```mermaid
flowchart TD
  CB[IVR callback tới Order Core] --> V{Core revalidate P0}
  V -->|idempotency/version/state OK| B{Blocker check via Ops<br/>DO-03}
  V -->|stale/mismatch| STALE[REJECTED_STALE<br/>không transition]
  B -->|ops 2xx & no blocker| E{Evidence OK?}
  B -->|blocker active OR ops down/timeout/503| BLK[BLOCKED_BY_CORE<br/>fail-closed DO-06]
  E -->|có| OK[ACCEPTED_FOR_REVALIDATION<br/>Core transition theo result]
  E -->|thiếu| REV[NEEDS_ADMIN_REVIEW]
```

## 2. Attempt/scheduler timeline (D-10)
```mermaid
flowchart LR
  T0[T0 = Core mở window/tạo task] --> A1[Attempt 1]
  A1 -->|no-answer| W{GH: +2:30 / 24-7: +7:30}
  W --> A2[Attempt 2]
  A2 -->|no-answer| EXP[expire GH T0+5:00 / 24-7 T0+15:00<br/>NO_ANSWER_FINAL]
  A1 -->|DTMF 1/0| DONE[final: signal → Core]
  A2 -->|DTMF 1/0| DONE
```

## 3. Adapter mode (SIM chưa mua — DT-01)
```mermaid
flowchart LR
  SCH[Scheduler] --> AD{adapter_mode}
  AD -->|MOCK| MK[Mô phỏng raw_call_status<br/>theo seed p10]
  AD -->|REAL| RG[SIM Gateway thật<br/>- sau khi mua + release gate]
  MK --> NORM[Result Normalizer DT-02]
  RG --> NORM
```

## Ghi chú render
- Tất cả block dùng `flowchart`/`sequenceDiagram`/`stateDiagram` chuẩn Mermaid. Không nhúng ký tự đặc biệt gây lỗi parse.
