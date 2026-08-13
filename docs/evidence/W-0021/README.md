# W-0021 / P2-4 — speech rendering, dial-token and MOCK SIM adapter

## Verdict

`TESTS_PASS` for the local, deterministic `MOCK` scope only.

P2-4 connects a fenced `SchedulerDispatchLease` to a software-only SIM gateway. The
gateway renders the approved Vietnamese order-confirmation text, resolves an opaque
dial-token fingerprint to an explicitly allowlisted fake destination, simulates the
provider interaction and appends a raw provider event for P2-5 normalization.

This evidence does **not** prove live TTS, a physical SIM/eSIM, a modem, a carrier,
a real phone destination, a Sales API callback or production readiness.
`REAL_CUSTOMER_CALL_ALLOWED=NO` remains mandatory for this slice.

## Implemented boundary

- `ISpeechRenderer` accepts only an approved script template/version for the active
  execution mode. The MOCK implementation returns exact text plus fake audio metadata
  (`FAKE_TEXT_ONLY`); it opens no TTS or media connection.
- Vietnamese rendering includes the customer display name, short order code, spoken
  items/quantities, total, short delivery area and the `1` confirm / `0` cancel prompt.
  Items beyond the first three collapse into a count, and exact approved pronunciation
  hints are applied before privacy validation.
- `IDialTokenResolver` rejects expired, unknown, non-allowlisted and replayed tokens.
  The MOCK vault persists only `enc:mock-sha256:*`; token-to-destination mappings remain
  process-local configuration and cannot be reversed from the database value.
- `ISimGateway` provides vendor-neutral dial, play, DTMF capture, disposition, hang-up
  and channel-health operations. `FakeSimGateway` has no HTTP, socket, serial, SIP or
  vendor dependency and refuses recording-enabled requests.
- `MockSchedulerDispatchGateway` accepts only `MOCK` leases/providers while the mock
  telephony flag is enabled, its kill switch is disengaged, the runtime mode/provider
  is `MOCK`, and real-customer calling remains disabled.
- The PostgreSQL dispatch store rechecks job/channel/attempt lease token plus fencing
  generation before every mutation. Completion releases the lease, increments the
  fence and applies channel cooldown; unhealthy channels are quarantined and reach
  `HEALTH_FAILED` after three failures.
- Provider output is stored as `PROVIDER_EVENT_PENDING_NORMALIZATION` /
  `DISPOSITION_PENDING_NORMALIZATION`, is not counted as a customer attempt and cannot
  mutate an order. P2-5 owns DTMF/disposition normalization and retry meaning.

## Approved synthetic speech snapshot

The following is synthetic and intentionally contains neither a phone number nor a
full street address:

> Xin chào anh/chị Anh Đạt. Anh/chị có đơn DH-2026-001 gồm 2 Hộp Sâm lát, tổng tiền 1.250.000 đồng, giao đến Phường Bến Nghé, Quận 1. Bấm phím 1 để xác nhận đơn hàng, bấm phím 0 để hủy.

Verified metadata:

| Field | Result |
| --- | --- |
| locale | `vi-VN` |
| audio format | `FAKE_TEXT_ONLY` |
| content hash | deterministic and non-empty |
| recording | `DISABLED` |
| retained exact text | in-memory playback object only; absent from audit/evidence payloads |

## Safe local configuration shape

Defaults remain fail-closed (`Enabled=false`, `KillSwitchEngaged=true`, empty
allowlist/mappings/scenarios). A test environment may opt in with synthetic references:

```json
{
  "Ivr": {
    "Telephony": {
      "Mock": {
        "Enabled": true,
        "KillSwitchEngaged": false,
        "DtmfTimeoutSeconds": 10,
        "CooldownSeconds": 5,
        "TokenDestinations": {
          "*": "mock-destination-001"
        },
        "DestinationAllowlist": ["mock-destination-001"],
        "Scenarios": {
          "*": {
            "Disposition": "Answered",
            "DtmfKey": "1"
          }
        }
      }
    }
  }
}
```

The wildcard is useful only for deterministic local fake data. Do not put a real phone
number, source-system credential or production dial token in this configuration.

## Scenario and safety matrix

| Case | Expected raw provider result before P2-5 |
| --- | --- |
| answered + `1` | `ANSWERED`, raw DTMF `1`, pending normalization |
| answered + `0` | `ANSWERED`, raw DTMF `0`, pending normalization |
| no input | `ANSWERED`, null raw DTMF, pending normalization |
| invalid key | `ANSWERED`, sanitized raw DTMF `INVALID`, pending normalization |
| ring timeout / busy / rejected / unreachable / invalid destination / dropped | corresponding raw disposition, no playback/DTMF for unconnected session |
| token missing/expired/replayed/not allowlisted | fail closed; no real destination is dialed |
| audio/DTMF/network/SIM failure | technical raw event; unhealthy channel quarantined where applicable |
| concurrent use of one channel | second dial rejected until the first session hangs up |
| recording requested | request rejected before a provider event is emitted |
| kill switch/config disabled | scheduler dispatch gateway reports not ready |

## Verification evidence

| Gate | Result |
| --- | --- |
| Release build with warnings as errors | PASS — 0 warnings, 0 errors |
| Contract tests | PASS — 21/21 |
| Unit tests | PASS — 106/106 |
| PostgreSQL integration tests | PASS — 59/59 |
| Total regression | PASS — 186/186 |
| Aggregate line coverage | PASS — 94.55% (20,505/21,687), threshold 60% |
| EF pending model changes | PASS — none |
| Admin UI lint/build/npm High audit | PASS — 0 vulnerabilities |
| CI config/OpenAPI/docs/drift/negative gates | PASS |
| NuGet High/Critical policy | PASS |
| Docker Compose mocks profile | PASS |
| Gitleaks 8.30.0 working-tree scan | PASS — no leaks found |
| Locale-stable PII self-test + `docs/evidence` scan | PASS — 24 text files, 2 binary files skipped |
| Official Markdown map | PASS — 415 files, 375 resolved links, 0 unresolved |

Named P2-4 proof includes:

- `UT-TEL-SPEECH-01`, `UT-TEL-SPEECH-02`, `UT-TEL-PRONUNCIATION-09`
- `UT-TEL-TOKEN-03`, `UT-TEL-VAULT-11`
- `UT-TEL-SCENARIO-04`, `UT-TEL-CHANNEL-05`, `UT-TEL-DTMF-10`
- `UT-TEL-SAFETY-06`, `UT-TEL-RECORDING-07`, `UT-TEL-DELAY-12`
- `UT-TEL-DI-08`, `UT-TEL-NONMOCK-13`
- `IT-TEL-DISPATCH-01`, `IT-TEL-TOKENFAIL-02`, `IT-TEL-HEALTH-03`

## Residual gates and ownership

- P2-5/W-0022 must normalize DTMF/dispositions, classify counted vs technical
  attempts and drive the next state/retry.
- P2-9/W-0066 owns the real/fake TTS provider port and audio-provider adapter. P2-4
  proves text and fake metadata only.
- P8-1/W-0048 plus external lab decisions own the physical one-SIM test, allowlisted
  test number, modem/eSIM hardware, carrier behavior and vendor adapter.
- The future 32-eSIM design requires capacity evidence, device/channel inventory,
  vendor selection and operational monitoring; it is not inferred from this fake.
- Sales/Order Core still owes approved API/auth/data contracts and real token delivery.
  No source-system endpoint or credential is embedded here.
- Real integration, LAB, staging, production, legal/security approval and a real
  customer call are `NOT_RUN`.
