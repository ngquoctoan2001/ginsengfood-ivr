# W-0280 — Three defects that only appear when the thing is run

Status: `TESTS_PASS`
Date: `2026-09-12`
Baseline: `main@2613da5`, clean tree, no concurrent writer
Execution boundary: local, MOCK, fake data; `REAL_CUSTOMER_CALL_ALLOWED=NO`

## Why these three were invisible

Each one hid the next. The suite was green at 985 tests throughout, and none of
the three is the kind of defect a unit test is shaped to catch: one lives in a
script's summary line, one only fires against a real column constraint, and one
is an integration whose other half was deleted two weeks earlier.

## 1. The image gate died on its own success message

`W-0253` removed the admin-ui image and with it the local `images` variable that
filtered it out, switching the loop to `IMAGES`. One reference survived, in the
line that prints `IT-IMG-BUILD-01 PASS`. Every check ran and passed; the script
then threw `ReferenceError: images is not defined` while announcing it.

Fix: one word. The gate now reports `5/5`.

## 2. A classification overflowed a one-character column, permanently

`analytics_fact.dtmf_key` is `varchar(1)` under `ck_analytics_fact_dtmf`
(`^[0-9*#]$`): it holds the key the customer pressed, or nothing. The
operational column it is copied from is plain `text`, and `SanitizeDtmf` on the
dispatch path writes `INVALID` (any key that is not 0 or 1) and `NO_INPUT`
(answered, nothing pressed) into it. Both are correct there. Neither is a key.

The ETL copied them across verbatim. Because a batch loads in one transaction,
a single such row failed that run **and every later run**, forever: the
warehouse silently stopped moving while every test stayed green. One customer
pressing 7 was enough.

Fix: `KeypadDigitOrNull` at the load boundary — the layer that owns the
constraint. Nothing is lost; "pressed something else" and "pressed nothing" are
already carried by `ResultTypeKey` and `FinalResultStatus`.

Backing test: `AClassificationInTheOperationalDtmfColumnLoadsAsNoKeyRatherThanKillingTheRun`.
Verified as a guard, not decoration: reverting the fix turns it red, restoring
it turns it green.

## 3. Half the image gate had been dead for fifteen days

With defect 1 fixed the script ran on, and every admin call answered `401`. It
authenticates with `X-Permissions` and `X-Mock-Actor-Id` — the MOCK permission
seam `W-0128` deleted on 28/08 when the admin surface moved to credential
tiers. The server does not reject those headers; it ignores them.

Fix: tier bearer tokens, plus the actor and reason the danger tier requires, and
the three admin tokens added to `docker-compose.dev.yml` (the image runs outside
Development, so `appsettings.Development.json` never applied).

**This is the finding worth keeping.** A required gate rotted for fifteen days
and was found by hand, not by CI. No pipeline in this repository has ever run.

## Verification

| Check | Result |
|---|---|
| GitNexus impact, pre-edit, `ExtractAsync` | `LOW`, 1 caller, 1 process |
| GitNexus detect-changes, pre-commit | `MEDIUM`, 22 symbols / 5 files / 3 flows, all three the ETL flow |
| `image-selftest` | `5/5` IT-IMG PASS |
| Unit / Integration / Contract / Chaos | `670` / `284` / `24` / `8` = **986/986** |
| Release build | 0 warnings, 0 errors |
| Test traceability | `613`, regenerated |

## Open, and deliberately not decided here

The image gate's capacity drill pauses the queue so a confirmation window closes
undialled, and expects `IVR_CAPACITY_EXCEPTION`. The runtime answers
`IVR_CONFIRMATION_WINDOW_EXPIRED`. The safety half is right — the pause held and
nothing was dialled — but the two labels map to different recommended core
actions, so Module 3 does different things with the order. `specs/functional/
06-technical-exception-capacity.md` says "capacity không xử lý kịp" is the
former; whether an operator pause counts as capacity is an owner decision, not
mine.

## Governance gap recorded, not filled

`W-0267`..`W-0279` are committed and carry evidence packs but have no row in the
tracker, so the readiness board undercounts by thirteen. The tracker header also
read `NEXT_WORK_ID = W-0250`, an id already spent; left alone it would have
issued a duplicate. The header is corrected here. Writing the thirteen missing
rows belongs to whoever did that work.
