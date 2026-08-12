# P6 - Analysis Only

**PROMPT-P6 - ADS MEASUREMENT / ATTRIBUTION / ROAS / SCALE GATE ANALYSIS HANDOFF**

## Mode

ANALYSIS ONLY. Khong sua code, khong tao dashboard, khong migration, khong seed, khong scale Ads.

## Entry Gate

Chi bat dau Phase 6 Analysis khi:

- P3 Verified Revenue boundary co evidence toi thieu.
- P3 Payment/COD/Order Verified boundary co evidence.
- Phase 5 event/channel identity co evidence neu dung Meta/Facebook signal.
- Neu Verified Revenue chua pass, ghi blocker `P6-ENTRY-BLOCKED-BY-VERIFIED-REVENUE`.

**GLOBAL GATEWAY:** BLOCKED.

## Source Of Truth

- `docs/documents/3. tech/08-TECH-07-ADS-MEASUREMENT-ATTRIBUTION-ROAS-CPA-AOV-VERIFIED-REVENUE-SCALE-GATE.md`
- `docs/documents/2. pack/07-PACK-07-ADS-ROAS-ATTRIBUTION.md`
- `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md`
- `docs/documents/3. tech/07-TECH-06-FACEBOOK-GATEWAY-META-LIVE-MESSENGER-CHANNEL-DELIVERY-COMMENT-TO-MESSENGER.md`
- `docs/documents/3. tech/13-TECH-12-PHASE-BACKLOG-PACK-DEV-TASK-BREAKDOWN-SOURCE-TO-BACKLOG-MATRIX.md`
- `docs/documents/2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md`

Chua thay source-of-truth `docs/docs-ginsengfood-project/` trong workspace hien tai. Neu sau nay file do xuat hien, phai doi chieu lai truoc khi sua backend that.

## Scope Can Phan Tich

1. Event taxonomy / funnel signal boundary.
2. Pixel/CAPI dedup/idempotency.
3. Attribution source resolver.
4. Verified Revenue consumption from Commerce.
5. Exclusion rules for quote/cart/draft/unpaid/WAITING.
6. CPA/AOV/ROAS computation boundary.
7. Data Quality Gate.
8. Scale Gate / owner approval.
9. Sale Lock / Recall / Suppression blocks scale.
10. Smoke/evidence pack.

## Khong Duoc Lam

- Khong tinh revenue tu quote, cart, order draft, payment WAITING, COD WAITING.
- Khong double count Pixel/CAPI/event duplicate.
- Khong lam dep dashboard bang du lieu chua verified.
- Khong scale Ads khi Data Quality fail hoac owner chua accept.
- Khong bo Sale Lock/Recall/Suppression de scale.

## Output Bat Buoc

- Current-state map cho event, attribution, revenue, dashboard.
- Gap matrix ADS-BLG-001 -> ADS-BLG-010.
- Conflict matrix voi Commerce Verified Revenue.
- Data quality risk register.
- Attribution and dedup risk register.
- Proposed smoke matrix.
- Owner decision required.

## Done Gate

Analysis chi ket thuc khi co report ro evidence/missing evidence. Khong goi ROAS pass, scale-ready hoac release-ready.

