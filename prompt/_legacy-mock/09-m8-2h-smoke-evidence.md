# DEV PROMPT 09 — M8.2H Smoke / Evidence Pack

## Mục tiêu
Chạy smoke nội bộ (MOCK), gom evidence packet, chuẩn bị release gate — KHÔNG mở gọi khách thật.

## Requirement / Decision
`testing/*`, `P0-IVR-006/010` · `DF-03`, `MASTER-05`.

## Source spec
- `specs/srs/testing/08-acceptance-criteria.md`, `09-smoke-matrix.md`
- `specs/srs/_review/traceability-matrix.md`, `open-decisions-register.md`

## Build scope
1. Chạy toàn bộ smoke matrix (M8-P0-001..012 + IVR-SMK) trên **MOCK adapter + seed**; mỗi smoke PASS path + BLOCK path.
2. Gom evidence packet (docx §21): architecture, intake, eligibility, scheduler, SIM, DTMF, callback, privacy/security, capacity, release.
3. Đánh giá done gate (M8-DONE-001..010) + fail gate (M8-FAIL-001..008).
4. Báo cáo điều kiện còn thiếu để mở release gate (từ `open-decisions-register`).

## Done gate (docx M8.2H)
- [ ] Evidence **ACCEPTED** (không hardcode PASS).
- [ ] Mọi P0 smoke có evidence.
- [ ] `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ; không gọi khách thật.

## Evidence expected
Smoke report (PASS+BLOCK từng case), evidence packet đầy đủ, traceability matrix cập nhật.

## Forbidden
KHÔNG mở gọi khách thật; KHÔNG tuyên bố production-ready khi còn open decision P0 (mua SIM, DF-03 sign-off); KHÔNG bỏ qua evidence gate. Q-C1/DC-01 và DG-03/DS-01..05 đã resolved.

## Release gate (P0)
Chỉ owner (bạn) + security/privacy sign-off mới mở `REAL_CUSTOMER_CALL_ALLOWED` — sau khi: smoke pass + evidence ACCEPTED + mua SIM (DT-01) + pilot scope duyệt (DF-03). Q-C1 đã đóng bằng DC-01; DG-03 đã đóng bằng DS-01..05.

## Test
Toàn bộ `testing/*` + `_review/*`.
