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
| IVR-owned Target V1 draft | `1.0.0-draft.2` | `1.0.0-draft.2` | [IVR API changelog](api/changelog/ivr-order-confirmation.md) |
| Sales callback Target V1 draft | `1.0.0-draft` | `1.0.0-draft` | [Sales callback changelog](api/changelog/order-core-ivr-callback.md) |

Both current reports say `No changes detected`. The previous IVR baseline is
retained at `baselines/ivr-order-confirmation.v1.0.0.yaml`; its transition to
draft.2 contained `143` changes (`63` errors and `80` warnings) and is preserved
in [the archived transition report](api/changelog/ivr-order-confirmation.v1.0.0-to-v1.0.0-draft.2.md).
This reviewed draft reset repairs the comparison gate; it does not claim that
Target V1 is live, backward compatible, or approved by Sales.

## Change procedure

1. Change the authoritative OpenAPI file only after reviewing the consumer and provider impact.
2. Run lint, schema validation, generated-code drift and `api_contract_diff`.
3. Regenerate the matching report with the pinned oasdiff image.
4. Apply the SemVer and deprecation rules in [API versioning policy](api-versioning.md).
5. Update the historical baseline only when the reviewed version becomes the new comparison base.
6. Record external Sales approval separately; a clean diff never changes `TARGET_CONTRACT_V1=DRAFT` by itself.
