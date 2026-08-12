# PROMPT P4-2 — Sales Eligibility/Blocker Snapshot Contract

## 0. Meta

Work `W-0030` · prereq P2-2 · mode `MOCK`; Sales owns Ops orchestration/revalidation.

## 1. Outcome

Validate and consume the eligibility/blocker evidence that Sales puts in the task, and verify callback expectations for Sales revalidation. IVR must not become a second Ops orchestrator.

## 2. Build

1. Define typed `eligibility_snapshot`/sellable/recall/sale-lock evidence shape in Target task contract or linked evidence reference.
2. Validate required decisions/freshness/source version; unknown/missing/blocking values fail closed before dispatch.
3. Persist immutable privacy-safe snapshot/hash and link it to result evidence.
4. Fake Sales provides pass/block/stale/source-unavailable scenarios.
5. Callback CDC asserts Sales revalidates current blockers and may return `BLOCKED_BY_CORE` after key 1.
6. Do not add direct Ops credentials/client/webhook unless a separately approved architecture change updates Target V1.

## 3. Tests/evidence

Pass/block/unknown/stale/missing snapshot; race blocker after DTMF; no direct Ops egress/secret. Update W-0030 and any missing Sales field under W-0002/W-0005.

## 4. Forbidden
- ❌ IVR transition/ghi order state (D-02); `recommended_core_action` là advisory.
- ❌ Coi mock/fake evidence là đã đóng external gate (W-0002..W-0006).
- ❌ Bật real provider hoặc gọi khách thật trong slice này.
- ❌ Trộn internal record DTO với outbound Sales callback DTO.

## 5. Definition of Done
- [ ] Toàn bộ fake/WireMock suite xanh → đạt **`TESTS_PASS`** (mock-only). Đây là mức tối đa slice này có thể đạt.
- [ ] Real sandbox evidence là **hạng mục riêng**, `NOT_RUN`/`BLOCKED_EXTERNAL` cho tới khi Sales cung cấp endpoint/credential; **không** phải điều kiện của `TESTS_PASS`.
- [ ] Cập nhật Work ID trong tracker: artifacts, command/kết quả, evidence link, residual external gate; chỉ reviewer/owner chuyển `ACCEPTED`.
