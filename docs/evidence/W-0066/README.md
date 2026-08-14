# W-0066 — P2-9 speech/TTS provider boundary evidence

Status: `TESTS_PASS`

Baseline: `62049711f6bca9a77c0d1f63d5936b8aa5fbc3e1`

Implementation commit: `c93dace1614d8fc192a077ad4027a521f34bf711`

Evidence date: `2026-08-14`

Real-customer-call gate: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## Coverage correction — 2026-08-14

The original `94.7% (25,645/27,080)` ReportGenerator value below is historical
and included EF migration plus generated `.g.cs` source. A clean full
regression using committed `coverage.runsettings` plus the merge-policy
exclusion reports `88.80% (10,350/11,656)` from three Cobertura reports.
Coverlet still emitted one Worker source-generator class on Windows;
`Ivr.CiPolicy` explicitly excluded it, and the exclusion negative-control
fixture passed. The result remains above the 60% policy threshold.

## Delivered boundary

- `Ivr.Domain.Speech.ITtsProvider` accepts a redacted `SpeechScript` plus vendor-neutral
  `TtsOptions`, and returns `RenderedAudio` metadata with an opaque content reference.
- `FakeDeterministicTtsProvider` models an 8 kHz `audio/L16` tone from a deterministic
  hash. It has no HTTP/socket/SIP/serial/vendor dependency and is the only provider
  resolvable in `MOCK`.
- `ConfigurableExternalTtsProvider` reads the endpoint/credential/format seam but always
  throws `TTS_NOT_CONFIGURED`; no vendor or protocol is invented before `OD-V1-19`.
- `SpeechPrivacyGuard` runs after final text rendering and before synthesis. Phone,
  street-address or unsafe hint/config text is blocked as
  `IVR_PII_POLICY_VIOLATION`.
- `SpeechSynthesisService` applies the Vietnamese dictionary plus exact task hints,
  production whitelist approval guard, request/character budget, timeout, duration
  bound and cache. No text-only fallback exists.
- Cache keys are SHA-256 identities over the required tuple, never raw text. Expiry is
  the minimum of the confirmation deadline, configured cache TTL and speech-snapshot
  retention. P1-5 `IRetentionJob` invokes the ephemeral cache purge hook and honors
  dry-run.
- `MockSchedulerDispatchGateway` now passes synthesized audio metadata to the existing
  `ISimGateway.PlayAsync` seam. TTS timeout/provider/privacy failures persist as audio
  technical events with `is_counted_customer_attempt=false`; they cannot become a
  customer no-answer.
- Aggregate request/character/cache/purge metrics and the vendor-neutral cost formula
  are documented in `docs/capacity-model.md`.

No generated customer audio is stored in this evidence folder. The snapshot is a
synthetic privacy-safe fixture and the JSON contains metadata only.

## Test evidence

The nine required groups are listed in [test-report.md](test-report.md). Current focused
result is `9/9 PASS`; the full regression after implementation is `264/264 PASS`.
The historical three-report merge produced `94.7%` line coverage
(`25,645/27,080`) with ReportGenerator 5.5.11. One coverage-instrumented P2-8
internal-admin test transiently rejected a generated idempotency header; its isolated
coverage rerun passed `79/79`, while the independent non-coverage full regression
passed `264/264`. No TTS assertion failed.

Privacy-safe artifacts:

- `docs/evidence/W-0066/sanitized-speech-snapshot.txt`
- `docs/evidence/W-0066/audio-metadata.json`
- `docs/evidence/W-0066/pii-scan-report.txt`
- [capacity model](../../capacity-model.md)

## Safety interpretation

- `MOCK` has no provider egress and cannot instantiate the external adapter through DI.
- Recording remains `DISABLED`; IVR still sends no SMS/notification and performs no
  order/payment transition.
- Exact rendered text exists only in the in-memory playback object. `ToString()` for
  script/audio/options/provider config is redacted.
- The provider budget is process-local MOCK protection, not production distributed
  quota proof.

## Residual gates

- `OD-V1-19`: vendor, DPA/data residency, provider protocol, price and pronunciation
  set are `OWNER_DECISION_REQUIRED` / `NOT_RUN`.
- `OD-V1-15`: Product + Privacy/Legal approval for Target V1 items and short-area
  whitelist remains open; production synthesis fails closed without an approval ref.
- `W-0008` / `W-0048`: accepted gateway codec, physical one-SIM/eSIM path, allowlisted
  test destination, carrier behavior and Vietnamese lab acceptance are `NOT_RUN`.
- Future 32-eSIM concurrency/failover/caller-ID/cost evidence is `NOT_RUN`.
- Real Sales/auth/data, hosted deployment, reviewer/owner acceptance and production
  release remain outside this MOCK evidence.

Maximum status for P2-9 is `TESTS_PASS`; no statement here promotes mock evidence to
LAB or production readiness.

## Commit and remote handoff

The implementation commit was pushed directly to `main` and verified exact on both
GitHub and GitLab. The progress-ledger handoff is finalized in the immediate
documentation follow-up commit; no branch or merge request was created.

## Local validation summary

- `dotnet format Ivr.sln --verify-no-changes --no-restore`: PASS.
- Release build: PASS, `0` warnings / `0` errors.
- Required P2-9 matrix: `9/9 PASS`.
- Full regression: contract `21`, unit `164`, integration `79` = `264/264 PASS`.
- Historical merged line coverage: `94.7%` (`25,645/27,080`, three green
  reports), including migration/generated source.
- Corrected full-tree line coverage: `88.80%` (`10,350/11,656`, three reports,
  one generated class explicitly excluded), threshold `60%` PASS.
- PII evidence scan: PASS (`6` text files, `0` binary skipped, `C.UTF-8`).
- Official Markdown map: PASS (`421` files, `377` resolved links, `0` unresolved).
- Staged-diff Gitleaks 8.30.0: PASS (approximately `106 KB`, no leaks).
