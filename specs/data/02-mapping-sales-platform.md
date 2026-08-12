# DATA-02 — Mapping: Sales Platform ↔ IVR

Trạng thái: `TARGET_V1_DRAFT`.

## Task mapping

| Target field | Owner | IVR storage/use | Status |
| --- | --- | --- | --- |
| contract/task/order IDs, `order_version` | Sales | task snapshot/race callback | required target; version upstream gap |
| state, program, payment, IVR-required | Sales | validate matrix/snapshot | target matrix pending sign-off |
| window + attempt policy version/max/offsets | Sales/Product | policy snapshot/scheduler | candidate only MOCK/LAB |
| phone ref/masked/dial token/expiry | Sales/Identity | token/ref only | resolver contract missing |
| privacy-safe short name/code/items/total/area/program/locale | Sales/Product/Privacy | speech snapshot | P0 missing upstream |
| call restriction/eligibility/evidence | Sales aggregated | fail-closed intake | target producer work |
| script/policy versions | IVR/Governance selected in agreement with Sales | immutable snapshot | approval required |

## Callback mapping

| Field | Owner | Note |
| --- | --- | --- |
| callback/task/order IDs, version seen | IVR snapshot from Sales | target required |
| result/count/final/attempt/time | IVR | canonical, not raw provider payload |
| recommended action | IVR | advisory only; no-answer waits for timeout |
| evidence/audit refs | IVR/Foundation | required |
| HTTP + semantic ACK/order state | Sales | stored/displayed as Sales truth |

Target endpoint is `/api/v1/internal/orders/{orderId}/ivr-result-callbacks`; current Golden Hour endpoint is compatibility-only.

## Program rules

Golden Hour uses ONLINE; 24/7 uses COD. IVR does not process payment and rejects other combinations. `TWENTY_FOUR_SEVEN` is canonical; normalize legacy `24_7` at current adapter only.
