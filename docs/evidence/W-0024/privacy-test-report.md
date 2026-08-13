# W-0024 privacy test report

Scope is synthetic unit/integration data only. No customer data, Sales endpoint, SIM, audio or recording was used.

| Attack/input class | Expected behavior | Evidence |
| --- | --- | --- |
| unknown or malformed placeholder | reject before draft creation | `UT-SCRIPT-TEMPLATE-GUARD-04` |
| missing required Target V1 field | reject summary/template creation | `UT-SCRIPT-INPUT-GUARD-05` |
| oversized template/item field | reject before preview | `UT-SCRIPT-INPUT-GUARD-05` |
| HTML/control character | reject before persistence/render | `UT-SCRIPT-TEMPLATE-GUARD-04`, `UT-SCRIPT-INPUT-GUARD-05` |
| raw phone pattern | reject through `PiiGuard` | `UT-SCRIPT-TEMPLATE-GUARD-04`, `UT-SCRIPT-INPUT-GUARD-05` |
| full-address-like short area | reject through `ShortDeliveryArea` | `UT-SCRIPT-INPUT-GUARD-05` |
| unsupported key 9 | reject template; KEY_9 remains disabled | `UT-SCRIPT-TEMPLATE-GUARD-04` |
| creator self-approval / missing permission | reject | `UT-SCRIPT-LIFECYCLE-02` |
| same actor for Content and Privacy/Legal | production gate cannot pass | lifecycle policy tests |
| content update after approval | PostgreSQL trigger rejects | `IT-SCRIPT-PERSISTENCE-08` |
| approval update/delete | PostgreSQL append-only trigger rejects | `IT-SCRIPT-PERSISTENCE-08` |
| audit payload | ref/status/hash only; no template or rendered input | `UT-SCRIPT-LIFECYCLE-02`, `IT-SCRIPT-PERSISTENCE-08` |

Production statement: the implementation does not authorize real calls. `ProductionTargetV1FieldsApproved` defaults to `NO`, `REAL_CUSTOMER_CALL_ALLOWED=NO`, and `OD-V1-15` remains open.
