# Workflow — Invalid Phone

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p04` · Nguồn: `docx` §7,§13; `phase-8/03`,`/07`.

**Kết quả:** `IVR_INVALID_PHONE_FINAL` (NOT counted, final) → Core/admin review hoặc cancel theo policy. Phone invalid **KHÔNG** phải no-answer (P0-IVR-005).

> ⚠️ **Thực tế Core (DS-02):** như no-answer/technical, `INVALID_PHONE_FINAL` **không** tự transition order — order ở `CONFIRMING` chờ `timeout→EXPIRED` (trừ khi admin thao tác). "cancel per policy" ở diagram = **advisory/target** (IR-SALES-OC3), Core hiện chưa hủy chủ động.

```mermaid
sequenceDiagram
    participant OrderCore
    participant IVR
    participant Norm
    participant Evid
    OrderCore->>IVR: IvrConfirmationTaskV1 (phone_validation_status / resolver)
    IVR->>IVR: eligibility: phone invalid (format/unreachable/not official)
    Note over IVR: KHÔNG dispatch SIM
    IVR->>Norm: result IVR_INVALID_PHONE_FINAL (not counted)
    Norm->>Evid: evidence(invalid phone)
    Norm->>OrderCore: callback (IVR_INVALID_PHONE_FINAL)
    OrderCore->>OrderCore: revalidate -> admin review OR cancel per policy
```

**Owner Decision:** OD-DR-05 / M8-OD-004 — invalid phone xử lý **cancel** hay **admin review**? Mặc định V0.2: admin review hoặc Core policy. OD-09: tiêu chí permanent invalid phone.
