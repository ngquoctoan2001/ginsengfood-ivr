# API Changelog 1.0.0-draft.22 vs. 1.0.0-draft.23


## API Changes

### GET /call-jobs/{ivrCallJobId}/detail
- :warning: added the new `undefined` enum value to the `attempts/items/voice_region` response property for the response status `200`
- :warning: added the new `undefined` enum value to the `voice_region_source` response property for the response status `200`


### GET /feature-flags/{environment}
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`
-  the `header` request parameter `x-correlation-id` became optional
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `404`
-  added the non-success response with the status `422`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /feature-flags/{environment}
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`
-  the `header` request parameter `x-correlation-id` became optional
-  added the non-success response with the status `401`
-  added the non-success response with the status `404`
-  added the non-success response with the status `422`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### GET /feature-flags/{environment}/kill-switch
- :warning: for the `header` request parameter `x-correlation-id`, the minLength was increased from `0` to `1`
- :warning: added the pattern `^[A-Za-z0-9._:-]+$` to the `header` request parameter `x-correlation-id`
- :warning: for the `header` request parameter `x-correlation-id`, the maxLength was set to `128`
-  the `header` request parameter `x-correlation-id` became optional
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `404`
-  added the non-success response with the status `422`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /scripts
-  added the new optional `header` request parameter `idempotency-key`


### POST /scripts/{templateId}/{version}:approve
-  added the new optional `header` request parameter `idempotency-key`


### POST /scripts/{templateId}/{version}:retire
-  added the new optional `header` request parameter `idempotency-key`


### POST /scripts/{templateId}/{version}:submit
-  added the new optional `header` request parameter `idempotency-key`
