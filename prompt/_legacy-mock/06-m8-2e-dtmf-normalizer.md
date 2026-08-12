# DEV PROMPT 06 — M8.2E DTMF & Result Normalizer

## Mục tiêu
Chuẩn hóa raw SIM/DTMF → result_type (DT-02); tách technical ≠ no-answer.

## Requirement / Decision
`FR-IVR-CALL-003/004`, `FR-IVR-TECH-001..004`, `P0-IVR-004/005` · `DT-02`, `D-10`.

## Source spec
- `specs/srs/functional/06-technical-exception-capacity.md` (bảng disposition DT-02)
- `specs/srs/functional/05-result-normalization-callback.md`, `database/03-enums-and-status.md`

## Build scope
1. Normalizer theo bảng DT-02: answered+1/0→confirm/cancel; no-key→no-answer-attempt/wrong-input; ring-timeout/busy/rejected→NO_ANSWER (counted); unreachable/sai số→INVALID_PHONE_FINAL (không counted); SIM/audio/DTMF/network error/dropped→TECHNICAL_EXCEPTION (không counted); capacity→CAPACITY_EXCEPTION.
2. `is_counted_customer_attempt=false` cho technical/invalid/capacity.
3. Ghi `ivr_call_results` (+ `input_signal_only/no_direct_order_update/no_payment_or_revenue_effect=true`).
4. Technical retry riêng (không tăng customer attempt).

## Done gate (docx M8.2E)
- [ ] DTMF 1/0 mapping đúng.
- [ ] **Lỗi kỹ thuật KHÔNG thành no-answer** (P0-IVR-004).
- [ ] Invalid phone KHÔNG thành no-answer (P0-IVR-005).

## Evidence expected
DTMF evidence (1/0/wrong/no-input/timeout/error); technical≠no-answer proof; invalid-phone proof.

## Forbidden
KHÔNG để raw provider event đi thẳng Core; KHÔNG đếm technical như customer attempt.

## Test
`testing/02` (UT-NORM-01..06), `testing/05` (E2E-04/05/13/15), smoke `M8-P0-007`.
