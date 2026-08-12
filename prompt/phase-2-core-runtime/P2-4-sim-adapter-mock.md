# PROMPT P2-4 — Mock Telephony, Dial Token and Speech Execution

## 0. Meta

Work `W-0021` · prereq P2-3 · mode `MOCK` only.

## 1. Outcome

Implement vendor-neutral telephony port plus deterministic fake resolver/SIM/TTS so complete calls can run without any real phone network while exercising the boss-required order speech.

## 2. Build

1. Define `ISimGateway` operations dial/play/capture/hangup/disposition/health and provider event types.
2. Define `IDialTokenResolver`; fake maps opaque test token to fake/test destination only in memory. No raw phone persistence/log/evidence.
3. Define `ISpeechRenderer`; render approved Vietnamese template from immutable summary: short name/code, item names+qty, total VND, short area, program, key 1/0.
4. Implement item-collapse/pronunciation/length/locale behavior; never render full address or unknown free text.
5. Deterministic fake SIM scenarios: 1, 0, no input, invalid key, busy/reject/unreachable, token failure, audio/DTMF/network/SIM error and delays.
6. Implement channel lease/health/cooldown interactions and guarantee MOCK has no socket/serial/SIP/vendor egress.

## 3. Tests/evidence

Snapshot tests for Vietnamese speech and multi-item collapse; PII leak scan; every fake disposition; concurrent one-channel safety; resolver expiry; kill switch; test proving MOCK cannot instantiate real provider. Update W-0021 with sample rendered text/audio metadata and reports (no real number/audio with PII).

## 4. Forbidden/DoD

No real call, raw phone, full address, recording, SMS or order credential. Do not claim TTS pronunciation or vendor disposition is live-verified.
