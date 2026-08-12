# PROMPT P1-3 — Domain Model, Policies, Speech and Provider Ports

## 0. Meta

Work `W-0016` · prereq W-0014/W-0015 · mode `MOCK`.

## 1. Role/outcome

Bạn là Senior DDD/.NET Engineer. Tạo domain không phụ thuộc Java/vendor DTO, mapper fail-closed và các ports để hệ thống hoàn thiện bằng mocks rồi thay provider thật.

## 2. Read first

Governance/tracker · Target V1 draft · functional 01/03/04/05 · API/data/database specs.

## 3. Build

1. Model value objects: IDs/version, `ProgramPayment`, window, `AttemptPolicySnapshot`, offsets, execution mode, dial-token ref, speech summary/items/money/short area, result and callback ACK.
2. `ProgramPaymentPolicy` accepts only GH+ONLINE or 24/7+COD with required flag.
3. `IAttemptPolicyRegistry` loads versioned config; environment approval forbids candidate in PROD. Validate ordered offsets/max/window.
4. `PrivacySafeOrderSummary` enforces allowed shape/limits and exposes no generic free-text/full address field.
5. Define ports: `ISimGateway`, `IDialTokenResolver`, `ISpeechRenderer`, `IOrderCoreCallbackClient`, `IServiceTokenProvider`, time/ID/audit/evidence.
6. Separate target and current-compat anti-corruption mappings. No domain behavior branches on HTTP 200/422 legacy details.
7. Ensure immutable snapshots and deterministic hashes for idempotency/replay.

## 4. Tests/evidence

Property/unit tests for program matrix, policy bounds/candidate environment gate, Unicode/items/total/area speech, PII rejection, no-answer advisory, technical-not-counted, target/current mapping separation and immutable hash. Record commands/results in W-0016.

## 5. Forbidden/DoD

No magic policy numbers, raw phone/full address, order transition or notification dependency. Done only when every port has a deterministic fake and tests pass.
