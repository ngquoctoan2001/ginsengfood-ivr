# PROMPT P8-1 — Vendor Adapter and One-Real-SIM Lab

## 0. Meta

Work `W-0048` · prereq P2-4 + **P2-9 (`W-0066`, nguồn audio)** + W-0008 vendor protocol/test SIM · mode `LAB_REAL_SIM` · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 1. Outcome

Implement the real vendor adapter and verify it with exactly the available lab channel(s), initially **1 real SIM**, against approved test numbers only. Keep core/domain unchanged and future 32-eSIM scaling configuration-driven.

## 2. Build

1. Implement vendor adapter behind `ISimGateway`: dial/play/capture/hangup/disposition/health with protocol timeouts/cancellation.
2. Integrate real dial-token resolver at the defined trust boundary; redaction around every provider request/response.
3. Enforce destination allowlist, lab environment, global kill switch and one active call per channel before any dial.
4. Create disposition/DTMF verification harness for answer+1, answer+0, no input, invalid key, busy/reject/unreachable where feasible, disconnect and technical faults.
5. Update provider mapping only from evidence; unknown disposition is technical/review, never guessed no-answer.
6. Prove health/reconnect/lease/cooldown/quarantine. Channel count is config; add 32-channel simulator test but no claim about real eSIM capacity.

## 3. Evidence/acceptance

Vendor/protocol baseline, approved allowlist, call-to-test-number logs, DTMF/disposition table, PII scan, kill-switch and one-call tests. Update W-0048/W-0008. Lab pass remains non-production and cannot call customers.

## 4. Forbidden

No customer number, no full address/raw phone evidence, no recording/SMS, no fallback around allowlist/kill switch, no claim that one SIM validates 32 eSIM throughput.

## 6. Definition of Done
- [ ] Vendor adapter implement sau `ISimGateway`; **domain không đổi** khi thay provider.
- [ ] Speech/audio lấy từ `P2-9` (`ITtsProvider`) — không tự chế nguồn audio.
- [ ] Allowlist + kill switch + one-active-call-per-channel enforce trước mọi dial, có test âm.
- [ ] Chỉ đạt `LAB_REAL_SIM_VERIFIED` cho phạm vi đã test; **không** suy ra 32-eSIM throughput hay integration.
- [ ] `W-0008`/`OD-V1-09`/`OD-V1-18`/`OD-V1-19` vẫn mở nếu vendor chưa cung cấp artifact.
