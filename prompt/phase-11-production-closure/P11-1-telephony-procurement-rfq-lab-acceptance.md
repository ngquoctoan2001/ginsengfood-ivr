# PROMPT P11-1 — Telephony RFQ, One-SIM Lab and 32-eSIM Closure

## 0. Meta

Work `W-0057`; owns external W-0008; start at project beginning.

## 1. Outcome

Produce actionable procurement/vendor artifacts in two stages: acquire/enable one real SIM for safe lab verification now, and specify/evaluate 32 eSIM channels for production later.

## 2. Deliverables

1. Vendor requirements: protocol/SDK/version/support, auth, dial/play/DTMF/hangup/disposition/health, token resolver integration, caller ID, rate/cost/SLA/security.
2. Lab package: one SIM, approved test numbers, network topology/secrets, disposition scenario checklist, kill switch and acceptance report template.
3. 32-eSIM package: channel lifecycle/provisioning, real concurrent calls, quotas, pooling/failover/quarantine, throughput/latency, cost, observability and disaster mode.
4. Weighted vendor scorecard and gap register; contract clauses for API/version/disposition changes.
5. Decision records contain actual product/protocol/channel evidence; unresolved items remain blocked.

## 3. Acceptance

W-0008 closes lab portion only after P8 evidence; production portion closes only after 32 eSIM procurement and measured capacity/failover. Do not infer 32-channel readiness from simulator or one-SIM results.

Update canonical tracker throughout; vendor follow-ups are unplanned Work IDs in sequence.

## 5. Forbidden
- ❌ Chọn vendor thay owner/procurement.
- ❌ Đóng `W-0008`/`G-LAB-SIM`/`G-ESIM32` bằng RFQ hay báo giá; chỉ artifact thật mới đóng.
- ❌ Ghi số SIM pilot mặc định (12/24/32) như đã chốt khi chưa có throughput đo thật.

## 6. Definition of Done
- [ ] RFQ/checklist bao phủ: protocol/SDK, DTMF mode, codec/format, disposition mapping, concurrency/channel, health API, caller ID, secret provisioning, CDR, **TTS/audio capability (`OD-V1-19`)**.
- [ ] Mỗi mục có owner + due + closure artifact định nghĩa rõ.
- [ ] Trạng thái giữ `BLOCKED_EXTERNAL` cho tới khi vendor phản hồi; đạt tối đa `EVIDENCE_SUBMITTED` cho phần tài liệu.
