# TEST-02 — Unit and Property Test Plan

Trạng thái: `TARGET_V1_DRAFT`.

| Area | Required cases |
| --- | --- |
| intake matrix | accept GH+ONLINE and 24/7+COD with flag; reject crossed/unknown/flag false |
| contract/time | missing order version, expired window/token, path/body mismatch |
| policy | candidate mock-lab-v1; alternate policy; bounds/order/expiry; unapproved PROD rejection |
| speech | one/many items, collapse, VND, Unicode, short area; reject full address/unknown/free text |
| privacy | no raw phone/address/token in logs/errors/UI models |
| scheduling | deterministic priority, final cancels future, technical not counted, capacity result |
| channel | lease/fencing/one-active, kill switch, mode/allowlist guards |
| normalizer | 1/0/no input/invalid and every technical disposition |
| callback | target ACK/status/retry matrix, immutable replay; current adapter isolation |
| no-answer | `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`, no transition/notification |

Every fixture records contract/policy/provider/mode version. Property tests must prove no fixed two-attempt assumption in domain logic.
