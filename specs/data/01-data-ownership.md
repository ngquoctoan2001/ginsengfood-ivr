# DATA-01 — Data Ownership

Trạng thái: `TARGET_V1_DRAFT`.

| Data | Owner | IVR usage |
| --- | --- | --- |
| order identity/version/state/transition | Sales Order Core | immutable task snapshot; callback reference; never write |
| program/payment/IVR-required/attempt policy | Sales/Product | validated snapshot; policy version/config |
| speech-safe customer/order summary | Sales/Product/Privacy | immutable snapshot for approved script |
| raw address/profile/payment/history | Sales/CRM | none; forbidden |
| phone truth/dial-token issue | Sales/Identity | refs/token only |
| raw phone resolution | Telephony trust boundary | never persist/log in IVR |
| eligibility/do-not-call | Sales aggregated from owners | task snapshot; Sales revalidates blocker tồn kho/thu hồi ở callback |
| call job/attempt/result/callback/outbox | IVR | owner |
| SIM channel lease/health/capacity incident | IVR runtime + vendor observations | owner of operational state, not telco truth |
| audit/evidence acceptance | IVR writes refs; Evidence/Release owner accepts | never self-mark accepted |

IVR database contains no order transition, payment mutation, raw phone, full address, recording or notification delivery state in V1.
