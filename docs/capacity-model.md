# IVR Capacity and Cost Model

Status: `ENGINEERING_MODEL` · Values are configuration defaults, not production sizing approval.

## TTS synthesis boundary (W-0066)

P2-9 measures provider demand without selecting a vendor. The default MOCK budget is:

| Input | Default | Enforcement |
| --- | ---: | --- |
| Maximum characters per synthesis request | 1,200 | request rejected before provider call |
| Maximum provider requests per process/minute | 60 | fixed-window fail-closed budget |
| Maximum provider characters per process/minute | 72,000 | fixed-window fail-closed budget |
| Maximum rendered audio duration | 120 seconds | provider result rejected if it exceeds the bound |
| Provider timeout | 5 seconds | becomes `IVR_TECHNICAL_EXCEPTION`, never no-answer |
| Audio cache maximum TTL | 900 seconds | additionally capped by confirmation deadline and speech retention |

The deterministic MOCK adapter models mono 8 kHz, 16-bit linear PCM metadata
(`audio/L16`). At that format, uncompressed media is approximately 16 kB/second,
960 kB/minute, before gateway/container overhead. It does not open a network socket
and does not represent a supported real-gateway codec; W-0008/P8-1 must measure the
selected hardware/vendor path.

Runtime metrics expose only aggregates:

- `ivr_tts_provider_requests_total`
- `ivr_tts_characters_total`
- `ivr_tts_cache_operations_total{result=hit|miss}`
- `ivr_tts_cache_purged_total`

The cache identity is SHA-256 over `(script_template_id, script_version,
hash(privacy_safe_order_summary), voice_id, locale)`. It contains no raw summary,
phone, address or rendered text. A restart clears the process-local cache, and the
P1-5 retention job invokes its purge hook; dry-run reports without mutation.

## Cost formula pending OD-V1-19

No currency estimate is asserted because no TTS vendor or price sheet has been
approved. Once Product, Infra and Privacy/Legal close `OD-V1-19`, use:

```text
billable_characters = provider_characters_after_cache
monthly_tts_cost = billable_characters / vendor_billing_unit
                   * vendor_price_per_billing_unit
```

Sizing inputs still required from the selected vendor/lab:

- billing treatment for punctuation, SSML and pronunciation hints;
- request/concurrency quotas and regional endpoint availability;
- accepted codec/sample rate for the SIM gateway;
- measured cache-hit ratio, p50/p95/p99 synthesis latency and error rate;
- DPA/data residency, encryption and provider content-retention controls;
- Vietnamese product-name, amount, quantity and delivery-area pronunciation acceptance;
- one-SIM lab throughput followed by the future 32-eSIM concurrency/failover model.

Until those inputs exist, this section is a bounded engineering model only;
pronunciation, vendor cost and 32-channel production capacity remain `NOT_RUN`.
