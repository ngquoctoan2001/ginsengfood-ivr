# IR-03 — Telephony / SIM Gateway Requirements

Trạng thái: `REQUIREMENTS` · Nguồn: DT-01..DT-06; `api/04-sim-adapter-contract`, `functional/06`; `phase-8/06`,`/10`,`/16`.
⏳ **SIM gateway CHƯA MUA (sẽ mua).** Làm trước bằng **adapter port + mock**; các mục dưới điền/verify khi mua.

| ID | Yêu cầu | Prio | I/O | idempotency | mock? | Ai build | Trạng thái |
| --- | --- | --- | --- | --- | --- | --- | --- |
| IR-TEL-01 | **SIM Gateway + protocol**: cung cấp qua **adapter port** (`dial/play_script/capture_dtmf/report_disposition/health`). Protocol phần cứng (AT command/SIP-to-SIM/vendor API) điền khi mua | P0 | port ops | — | có (MOCK) | Infra/procurement | ⏳ DT-01 (port ✅, protocol PENDING) |
| IR-TEL-02 | **Disposition mapping re-verify**: đối chiếu mã disposition telco thật với bảng DT-02 (busy/rejected→NO_ANSWER; unreachable/sai số→INVALID_PHONE_FINAL; SIM/audio/DTMF/network error→TECHNICAL_EXCEPTION) | P0 | out: mapping table | — | có | Infra | ✅ DT-02 (locked, re-verify khi có SIM) |
| IR-TEL-03 | **DTMF capture**: `1`/`0` qua RFC2833 hoặc in-band; timeout sau script; phím sai/không bấm theo rule | P1 | in: DTMF | — | có | Infra/gateway | ⏳ DT-03 |
| IR-TEL-04 | **Capacity/health/cooldown**: `cooldown=5s`, `fail_count≥3/10′→disable+alert`; số SIM thật (giả định pilot 12 → launch 24–32); `ONE_SIM_ONE_ACTIVE_CALL` | P1 | out: SIM metrics | — | có | Infra | ✅ rule; ⏳ số SIM DT-04 |
| IR-TEL-05 | **Recording**: OFF mặc định; nếu bật cần consent + legal + retention; lưu `recording_ref` | P1 | — | — | n/a | Owner+Legal | ✅ OFF (DT-05) |
| IR-TEL-06 | **Caller-ID/brandname**: số gọi ra nhất quán, đáng tin (giảm bị chặn spam) | P1 | — | — | n/a | Telco/procurement | ⏳ DT-06 |

## Ghi chú
- `NEED_CONFIRMATION`: **telephony webhook provider KHÔNG dùng** ở mô hình internal SIM (chỉ xét nếu đổi sang cloud/SIP provider — future owner decision).
- Cho tới khi mua SIM: `adapter_mode=MOCK`, SIM channel `enabled=false`, `REAL_CUSTOMER_CALL_ALLOWED=NO`.
