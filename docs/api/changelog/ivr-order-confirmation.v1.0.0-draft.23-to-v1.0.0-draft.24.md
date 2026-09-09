# API Changelog 1.0.0-draft.23 vs. 1.0.0-draft.24


## API Changes

### POST /admin-reviews
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /analytics/breakdown
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /analytics/export
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /analytics/summary
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /analytics/trend
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /call-attempts
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /call-jobs
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /call-jobs
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /call-jobs/{ivrCallJobId}
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /call-jobs/{ivrCallJobId}/detail
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /call-jobs/{ivrCallJobId}:terminate
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /call-jobs:terminate-all
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /call-results
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /dashboard
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /dev/integration-profiles/{profileId}:apply
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /dev/scenarios/{scenarioId}:dry-run
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /dev/seed:load
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /eligibility-checks
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /feature-flags/{environment}
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`


### GET /integration-status
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /queue
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /queue:pause
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /queue:resume
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /result-callbacks
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /review-items
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /scripts
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /scripts
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /scripts/{templateId}/{version}
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /scripts/{templateId}/{version}:approve
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /scripts/{templateId}/{version}:retire
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /scripts/{templateId}/{version}:submit
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### GET /sim-channels
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /sim-channels/{simChannelId}:disable
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /sim-channels/{simChannelId}:enable
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`


### POST /tasks
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: request property `phone_validation_status` was restricted to a list of enum values
- :warning: the request property `phone_validation_status` became required
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`
-  added the new `VALID` enum value to the request property `phone_validation_status`


### POST /technical-retries
- :warning: for the `header` request parameter `idempotency-key`, the minLength was increased from `0` to `1`
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `idempotency-key`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `idempotency-key`, the maxLength was set to `128`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`
