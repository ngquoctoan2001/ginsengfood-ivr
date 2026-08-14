# W-0066 required test matrix

| Test ID | Result | Proof |
| --- | --- | --- |
| `UT-TTS-SNAPSHOT-01` | PASS | one/many-item text, deterministic replay and same-length/different-content collision guard |
| `UT-TTS-VND-02` | PASS | `12,5 hộp`; `123.456.789 đồng` |
| `UT-TTS-PRON-03` | PASS | exact task hint, Vietnamese Unicode and emoji survive synthesis |
| `UT-TTS-PII-04` | PASS | final phone/street text and unsafe configured voice are blocked with `IVR_PII_POLICY_VIOLATION` |
| `UT-TTS-TIMEOUT-05` | PASS | `TTS_TIMEOUT` normalizes to non-counted `IVR_TECHNICAL_EXCEPTION`, not no-answer |
| `UT-TTS-NOTCONFIGURED-06` | PASS | external skeleton throws `TTS_NOT_CONFIGURED` while `OD-V1-19` is open |
| `UT-TTS-WHITELIST-07` | PASS | production without approval record fails before provider call |
| `IT-TTS-CACHE-08` | PASS | tuple dimensions, hit/miss, shortest TTL, retention dry-run and purge |
| `IT-TTS-MODE-09` | PASS | MOCK resolves only deterministic provider and rejects endpoint/credential/external provider |

Focused execution: `7 unit + 2 integration = 9/9 PASS`.

Full regression after fixture remediation: `21 contract + 164 unit + 79 integration = 264/264 PASS`.

Coverage merge: `94.7%` line coverage (`25,645/27,080`) from three green
Cobertura reports. The integration coverage retry passed `79/79` after one transient
P2-8 idempotency-header rejection; the independent full regression remained
`264/264 PASS`.
