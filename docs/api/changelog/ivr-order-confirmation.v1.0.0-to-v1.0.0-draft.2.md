# API Changelog 1.0.0 vs. 1.0.0-draft.2


## API Changes

### POST /admin-reviews
- :warning: added the new required request property `resolution`
- :warning: added the new required request property `review_item_id`
- :warning: the `reason` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
- :warning: the `reason` request property's maxLength was set to `500`
- :warning: removed the request property `action_type`
- :warning: removed the request property `no_policy_bypass`
- :warning: removed the request property `target_id`
- :warning: removed the request property `target_type`
-  added the media type `application/json` for the response with the status `200`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `404`
-  added the non-success response with the status `409`
-  added the non-success response with the status `422`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /call-attempts
- :warning: added the new required `header` request parameter `x-service-scope`
- :warning: added the new required `header` request parameter `x-source-system`
- :warning: the request property `ivr_call_attempt_id` became required
- :warning: the request property `ivr_call_job_id` became required
- :warning: the `ivr_call_attempt_id` request property's minLength was increased from `0` to `1`
- :warning: the `ivr_call_job_id` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `422`
- :warning: removed the request property `attempt_number`
- :warning: removed the request property `disposition`
- :warning: removed the request property `dtmf_key`
- :warning: removed the request property `is_counted_customer_attempt`
- :warning: removed the request property `result_status`
- :warning: removed the request property `sim_channel_id`
- :warning: removed the request property `status`
-  added the media type `application/json` for the response with the status `200`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `403`
-  added the non-success response with the status `404`
-  added the non-success response with the status `409`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /call-jobs
- :warning: added the new required `header` request parameter `x-service-scope`
- :warning: added the new required `header` request parameter `x-source-system`
- :warning: the request property `ivr_call_job_id` became required
- :warning: the request property `task_id` became required
- :warning: the `ivr_call_job_id` request property's minLength was increased from `0` to `1`
- :warning: the `task_id` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `422`
- :warning: removed the request property `attempt_policy_version`
- :warning: removed the request property `attempt_spacing_seconds`
- :warning: removed the request property `confirmation_window_seconds`
- :warning: removed the request property `input_signal_only`
- :warning: removed the request property `max_attempts`
- :warning: removed the request property `no_direct_order_update`
- :warning: removed the request property `official_order_id`
- :warning: removed the request property `order_version`
- :warning: removed the request property `program_type`
- :warning: removed the request property `queue_status`
- :warning: removed the request property `status`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `403`
-  added the non-success response with the status `404`
-  added the non-success response with the status `409`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`
-  the response property `input_signal_only` became required for the status `200`
-  the response property `ivr_call_job_id` became required for the status `200`
-  the response property `max_attempts` became required for the status `200`
-  the response property `no_direct_order_update` became required for the status `200`
-  the response property `program_type` became required for the status `200`
-  the response property `queue_status` became required for the status `200`
-  the response property `status` became required for the status `200`
-  the response property `task_id` became required for the status `200`
-  added the required property `eligible` to the response with the `200` status
-  added the required property `expires_at` to the response with the `200` status


### GET /call-jobs/{ivrCallJobId}
- :warning: added the new required `header` request parameter `x-service-scope`
- :warning: added the new required `header` request parameter `x-source-system`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `404`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `403`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`
-  the response property `input_signal_only` became required for the status `200`
-  the response property `ivr_call_job_id` became required for the status `200`
-  the response property `max_attempts` became required for the status `200`
-  the response property `no_direct_order_update` became required for the status `200`
-  the response property `program_type` became required for the status `200`
-  the response property `queue_status` became required for the status `200`
-  the response property `status` became required for the status `200`
-  the response property `task_id` became required for the status `200`
-  added the required property `eligible` to the response with the `200` status
-  added the required property `expires_at` to the response with the `200` status


### POST /call-results
- :warning: added the new required `header` request parameter `x-service-scope`
- :warning: added the new required `header` request parameter `x-source-system`
- :warning: the request property `ivr_call_job_id` became required
- :warning: the request property `ivr_call_result_id` became required
- :warning: the `ivr_call_job_id` request property's minLength was increased from `0` to `1`
- :warning: the `ivr_call_result_id` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `422`
- :warning: removed the request property `final_result_status`
- :warning: removed the request property `input_signal_only`
- :warning: removed the request property `is_counted_customer_attempt`
- :warning: removed the request property `is_final_for_ivr`
- :warning: removed the request property `no_direct_order_update`
- :warning: removed the request property `no_payment_or_revenue_effect`
- :warning: removed the request property `recommended_core_action`
- :warning: removed the request property `result_type`
-  added the media type `application/json` for the response with the status `200`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `403`
-  added the non-success response with the status `404`
-  added the non-success response with the status `409`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /eligibility-checks
- :warning: added the new required `header` request parameter `x-service-scope`
- :warning: added the new required `header` request parameter `x-source-system`
- :warning: the request property `task_id` became required
- :warning: the `task_id` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `422`
- :warning: removed the request property `blocked_reasons`
- :warning: removed the request property `decision`
- :warning: removed the request property `evidence_ref`
- :warning: removed the request property `skip_trusted`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `403`
-  added the non-success response with the status `404`
-  added the non-success response with the status `409`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`
-  the response property `blocked_reasons` became required for the status `200`
-  the response property `evidence_ref` became required for the status `200`
-  the response property `task_id` became required for the status `200`


### GET /feature-flags/{environment}
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `409`


### POST /feature-flags/{environment}
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `400`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `409`


### GET /feature-flags/{environment}/kill-switch
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`


### GET /queue
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
-  added the media type `application/json` for the response with the status `200`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /queue:pause
- :warning: the `reason` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
- :warning: the `reason` request property's maxLength was set to `500`
- :warning: removed the request property `action_type`
- :warning: removed the request property `no_policy_bypass`
- :warning: removed the request property `target_id`
- :warning: removed the request property `target_type`
-  added the media type `application/json` for the response with the status `200`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `409`
-  added the non-success response with the status `422`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /queue:resume
- :warning: the `reason` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
- :warning: the `reason` request property's maxLength was set to `500`
- :warning: removed the request property `action_type`
- :warning: removed the request property `no_policy_bypass`
- :warning: removed the request property `target_id`
- :warning: removed the request property `target_type`
-  added the media type `application/json` for the response with the status `200`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `409`
-  added the non-success response with the status `422`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /result-callbacks
- :warning: added the new required `header` request parameter `x-service-scope`
- :warning: added the new required `header` request parameter `x-source-system`
- :warning: added the new required request property `ivr_call_result_id`
- :warning: the `callback_id` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `422`
- :warning: removed the required property `code` from the response with the `200` status
- :warning: removed the request property `attempt_id`
- :warning: removed the request property `attempt_no`
- :warning: removed the request property `audit_ref`
- :warning: removed the request property `call_job_id`
- :warning: removed the request property `contract_version`
- :warning: removed the request property `created_at`
- :warning: removed the request property `dtmf_key`
- :warning: removed the request property `evidence_ref`
- :warning: removed the request property `is_counted_customer_attempt`
- :warning: removed the request property `is_final_for_ivr`
- :warning: removed the request property `max_attempts`
- :warning: removed the request property `order_id`
- :warning: removed the request property `order_version_seen_by_ivr`
- :warning: removed the request property `privacy_policy_version`
- :warning: removed the request property `program_code`
- :warning: removed the request property `recommended_core_action`
- :warning: removed the request property `result_reason`
- :warning: removed the request property `result_type`
- :warning: removed the request property `script_version`
- :warning: removed the request property `task_id`
- :warning: removed the request property `technical_error_code`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `403`
-  added the non-success response with the status `404`
-  added the non-success response with the status `409`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`
-  added the required property `callback_id` to the response with the `200` status
-  added the required property `delivery_status` to the response with the `200` status
-  added the required property `ivr_call_result_id` to the response with the `200` status
-  added the required property `requires_core_revalidation` to the response with the `200` status
-  added the required property `result_state` to the response with the `200` status
-  added the required property `retry_count` to the response with the `200` status


### POST /sim-channels/{simChannelId}:disable
- :warning: the `reason` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
- :warning: the `reason` request property's maxLength was set to `500`
- :warning: removed the request property `action_type`
- :warning: removed the request property `no_policy_bypass`
- :warning: removed the request property `target_id`
- :warning: removed the request property `target_type`
-  added the media type `application/json` for the response with the status `200`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `404`
-  added the non-success response with the status `409`
-  added the non-success response with the status `422`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /sim-channels/{simChannelId}:enable
- :warning: the `reason` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
- :warning: the `reason` request property's maxLength was set to `500`
- :warning: removed the request property `action_type`
- :warning: removed the request property `no_policy_bypass`
- :warning: removed the request property `target_id`
- :warning: removed the request property `target_type`
-  added the media type `application/json` for the response with the status `200`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `404`
-  added the non-success response with the status `409`
-  added the non-success response with the status `422`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /tasks
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `409`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `422`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`


### POST /technical-retries
- :warning: added the new required request property `technical_exception_id`
- :warning: the request property `reason` became required
- :warning: the request property `target_attempt_id` became required
- :warning: the `reason` request property's minLength was increased from `0` to `1`
- :warning: the `target_attempt_id` request property's minLength was increased from `0` to `1`
- :warning: the `error/details` response's property `type` changed from `array<object>` to `object` for status `403`
- :warning: the `reason` request property's maxLength was set to `500`
- :warning: removed the request property `customer_attempt_counted`
- :warning: removed the request property `exception_type`
- :warning: removed the request property `technical_retry_allowed`
-  added the new optional request property `evidence_ref`
-  added the media type `application/json` for the response with the status `200`
-  added the non-success response with the status `400`
-  added the non-success response with the status `401`
-  added the non-success response with the status `404`
-  added the non-success response with the status `409`
-  added the non-success response with the status `422`
-  added the non-success response with the status `429`
-  added the non-success response with the status `500`




## Components
-  removed the schema `CallbackCoreResponseTarget`
-  removed the schema `IvrAdminAction`
-  removed the schema `IvrConfirmationResultCallbackTargetV1`
-  removed the schema `IvrTechnicalException`
-  removed the schema `RecommendedCoreAction`
