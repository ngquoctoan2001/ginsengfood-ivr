# DEV PROMPT 05 — M8.2D SIM Gateway Adapter

## Mục tiêu
Adapter port thực thi cuộc gọi (MOCK), capture raw event; không quyền order.

## Requirement / Decision
`FR-IVR-CALL-001..007` · `DT-01/02/05`, `D-05`.

## Source spec
- `specs/srs/api/04-sim-adapter-contract.md`, `functional/04-call-execution-dtmf.md`
- `specs/srs/database/02-tables.md` (`ivr_raw_call_event`, `ivr_sim_channels`)

## Build scope
1. Adapter port impl: `dial(dial_token, script)`, `play_script` (biến whitelist), `capture_dtmf`, `report_disposition`, `health`.
2. `adapter_mode=MOCK`: đọc `seed/call-scenarios` → phát `raw_call_status`/`raw_dtmf`; ghi `ivr_raw_call_event` (recording_ref=null, DT-05).
3. Chỉ dùng `dial_token`/`phone_ref`; **không** raw phone (D-05); mapping token→số ở vault, không lưu.
4. REAL impl để **stub/disabled** tới khi mua SIM (DT-01).

## Done gate (docx M8.2D)
- [ ] Không assign trùng SIM; health monitor.
- [ ] Không lưu raw phone; recording OFF.
- [ ] Adapter **không** có credential ghi order/SMS.

## Evidence expected
Adapter MOCK run per scenario; no-raw-phone scan; SIM health/disable proof; no-order-write proof.

## Forbidden
KHÔNG bật REAL adapter; KHÔNG ghi order; KHÔNG gửi SMS; KHÔNG lưu raw phone/audio.

## Test
`testing/02` (UT-NORM upstream), `testing/07` (SEC-03/08/09/10), smoke disposition.
