# IVR change traceability

## Required mapping

| Source / decision | Contract / migration | Test ID and command | Evidence | Residual gate |
| --- | --- | --- | --- | --- |
| `path` + requirement/decision ID | OpenAPI/DB path or `N/A` with reason | `TEST-ID` + reproducible command | `docs/evidence/W-XXXX/` | `NONE`, `NOT_RUN`, `BLOCKED_EXTERNAL`, or owner decision |

Work ID: `W-XXXX`

Prompt ID: `PX-Y`

## Quality and safety checklist

- [ ] Source spec path and requirement/decision ID are supplied.
- [ ] Contract/migration impact is stated; generated code drift is checked when applicable.
- [ ] Tests include positive and negative paths with stable test IDs.
- [ ] Evidence is stored under the canonical Work ID directory.
- [ ] No direct order transition, payment/revenue processing, or customer notification was added to IVR.
- [ ] Missing dependency/policy/evidence fails closed.
- [ ] Logs, test output, and evidence contain no raw phone, full address, dial token, or secret.
- [ ] `IVR_EXECUTION_MODE=MOCK`, `SIM_PROVIDER=MOCK`, and `REAL_CUSTOMER_CALL_ALLOWED=NO` remain in force unless a separately accepted gate explicitly changes them.
- [ ] GitLab pipeline succeeds; no quality job is bypassed or marked `allow_failure`.

## Reviewer references

- [`docs/review-checklist.md`](../../docs/review-checklist.md) — what the gate already checks, and the governance items that block merge.
- [`docs/reviewer-guide.md`](../../docs/reviewer-guide.md) — what a machine cannot catch: races, idempotency keys, snapshot freshness, taxonomy mapping, and whether an assertion proves what it claims.

## Review notes

Reviewer:

Accepted evidence/status:

Remaining external or hosted checks:
