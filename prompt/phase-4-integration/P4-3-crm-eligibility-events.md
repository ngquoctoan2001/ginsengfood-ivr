# PROMPT P4-3 — Voice Call Restriction and Trust Snapshot

## 0. Meta

Work `W-0031` · prereq P2-2 · default provider is fake Sales snapshot.

## 1. Outcome

Consume Sales-provided voice call restriction/trust/risk evidence fail-closed. Do not build post-decision notification/event consumption in V1.

## 2. Build

1. Validate `call_restriction` and supporting eligibility evidence/source version in task; true/unknown/unavailable blocks dispatch.
2. Keep voice restriction separate from SMS/marketing consent semantics; IVR uses only transactional voice decision supplied by Sales.
3. Trust skip remains disabled unless a versioned Sales resolver decision and risk evidence are contractually available; default require-IVR.
4. Fake scenarios cover eligible, restricted, unknown, stale, resolver unavailable and trust+risk.
5. Record privacy-safe evidence; no direct CRM write or consent mutation.
6. P4-5 separately proves notification no-op; do not consume/publish outcome events here.

## 3. Tests/evidence

Fail-closed restrictions, SMS-vs-voice separation in fixtures, trust-unavailable requires IVR, no CRM notification/consent egress. Update W-0031; missing rich fields remain external work.

## 4. Forbidden
- ❌ IVR transition/ghi order state (D-02); `recommended_core_action` là advisory.
- ❌ Coi mock/fake evidence là đã đóng external gate (W-0002..W-0006).
- ❌ Bật real provider hoặc gọi khách thật trong slice này.
- ❌ Trộn internal record DTO với outbound Sales callback DTO.

## 5. Definition of Done
- [ ] Toàn bộ fake/WireMock suite xanh → đạt **`TESTS_PASS`** (mock-only). Đây là mức tối đa slice này có thể đạt.
- [ ] Real sandbox evidence là **hạng mục riêng**, `NOT_RUN`/`BLOCKED_EXTERNAL` cho tới khi Sales cung cấp endpoint/credential; **không** phải điều kiện của `TESTS_PASS`.
- [ ] Cập nhật Work ID trong tracker: artifacts, command/kết quả, evidence link, residual external gate; chỉ reviewer/owner chuyển `ACCEPTED`.
