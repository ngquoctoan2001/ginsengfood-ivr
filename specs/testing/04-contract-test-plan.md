# TEST-04 — Contract Test Plan

Trạng thái: `TARGET_V1_DRAFT`.

## IVR task API

- OAS parse/ref/codegen drift;
- required contract/order version, flag, policy, token and speech summary;
- exact program/payment matrix;
- reject additional/PII/full-address fields;
- require auth/idempotency/correlation.

## Sales callback Target V1

- proposed OAS parse/ref/codegen drift;
- path/body order ID and version;
- 200 semantic codes, 409 codes, 422, 429 and 5xx schemas;
- duplicate/conflict/retry classification and no-answer action.

## Current compatibility

Separate verified pact for `/api/v1/internal/ivr/golden-hour/callbacks`; label 200/422 current-only. Target tests run against fake/WireMock now and real Sales sandbox when supplied. A skipped real-provider test is `BLOCKED_EXTERNAL`, never pass.
