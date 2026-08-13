# IVR API Contract Changelog

Status: `TARGET_CONTRACT_V1=DRAFT` · Generator: `oasdiff v1.26.1`.

This file is the human index for machine-generated contract comparisons. The
two linked reports are regenerated from the pinned historical baselines and
compared byte-for-byte in GitLab CI. A breaking change fails
`api_contract_diff`; updating a baseline requires an explicit contract review
and does not approve the external Sales contract.

## Current comparisons

| Contract | Baseline | Current | Generated report |
| --- | --- | --- | --- |
| IVR-owned Target V1 draft | `1.0.0` | `1.0.0` | [IVR API changelog](api/changelog/ivr-order-confirmation.md) |
| Sales callback Target V1 draft | `1.0.0-draft` | `1.0.0-draft` | [Sales callback changelog](api/changelog/order-core-ivr-callback.md) |

Both reports currently say `No changes detected`; P1-4 establishes the first
reviewed documentation baseline rather than claiming that Target V1 is live.

## Change procedure

1. Change the authoritative OpenAPI file only after reviewing the consumer and provider impact.
2. Run lint, schema validation, generated-code drift and `api_contract_diff`.
3. Regenerate the matching report with the pinned oasdiff image.
4. Apply the SemVer and deprecation rules in [API versioning policy](api-versioning.md).
5. Update the historical baseline only when the reviewed version becomes the new comparison base.
6. Record external Sales approval separately; a clean diff never changes `TARGET_CONTRACT_V1=DRAFT` by itself.
