CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE ivr_admin_actions (
    admin_action_id text NOT NULL,
    action_type text NOT NULL,
    permission text NOT NULL,
    actor_id text NOT NULL,
    target_type text NOT NULL,
    target_id text NOT NULL,
    reason text NOT NULL,
    before_state_json jsonb,
    after_state_json jsonb,
    correlation_id text NOT NULL,
    evidence_ref text,
    no_policy_bypass boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_admin_actions" PRIMARY KEY (admin_action_id),
    CONSTRAINT ck_ivr_admin_actions_no_bypass CHECK (no_policy_bypass IS TRUE)
);

CREATE TABLE ivr_attempt_policies (
    policy_version text NOT NULL,
    program_type text NOT NULL,
    max_attempts integer NOT NULL,
    attempt_offsets_seconds_json jsonb NOT NULL,
    confirmation_window_seconds integer NOT NULL,
    allowed_execution_modes_json jsonb NOT NULL,
    approved_for_production boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_attempt_policies" PRIMARY KEY (policy_version, program_type),
    CONSTRAINT ck_ivr_attempt_policies_attempt_bounds CHECK (max_attempts BETWEEN 1 AND 10),
    CONSTRAINT ck_ivr_attempt_policies_window_positive CHECK (confirmation_window_seconds > 0)
);

CREATE TABLE ivr_audit_log (
    audit_id uuid NOT NULL,
    actor_id text NOT NULL,
    actor_type text NOT NULL,
    action text NOT NULL,
    target_type text NOT NULL,
    target_id text NOT NULL,
    reason text,
    before_state_json jsonb,
    after_state_json jsonb,
    correlation_id text NOT NULL,
    data_json jsonb NOT NULL,
    created_at timestamp with time zone NOT NULL,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_audit_log" PRIMARY KEY (audit_id)
);

CREATE TABLE ivr_capacity_incidents (
    capacity_incident_id text NOT NULL,
    session_id text NOT NULL,
    program_code text NOT NULL,
    status text NOT NULL,
    scope text NOT NULL,
    hold_new_calls boolean NOT NULL,
    active_sim_count integer NOT NULL,
    pending_call_jobs integer NOT NULL,
    expired_call_jobs integer NOT NULL,
    missed_deadline_count integer NOT NULL,
    shortage_reason text,
    opened_at timestamp with time zone NOT NULL,
    resolved_at timestamp with time zone,
    reason text,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_capacity_incidents" PRIMARY KEY (capacity_incident_id)
);

CREATE TABLE ivr_confirmation_tasks (
    id uuid NOT NULL,
    task_id text NOT NULL,
    contract_version text NOT NULL,
    idempotency_key text NOT NULL,
    correlation_id text NOT NULL,
    official_order_id text NOT NULL,
    order_code text NOT NULL,
    order_version text NOT NULL,
    order_state text NOT NULL,
    payment_method_snapshot text NOT NULL,
    ivr_confirmation_required boolean NOT NULL,
    customer_id text,
    customer_trust_status text,
    trusted_skip_allowed boolean,
    risk_flags_json jsonb,
    program_type text NOT NULL,
    attempt_policy_version text NOT NULL,
    max_attempts integer NOT NULL,
    attempt_offsets_seconds_json jsonb NOT NULL,
    confirmation_window_started_at timestamp with time zone NOT NULL,
    confirmation_window_expires_at timestamp with time zone NOT NULL,
    official_contact_id text,
    phone_ref text NOT NULL,
    phone_masked text NOT NULL,
    phone_validation_status text,
    dial_token_ciphertext text NOT NULL,
    dial_token_expires_at timestamp with time zone NOT NULL,
    privacy_safe_order_summary_json jsonb NOT NULL,
    eligibility_decision text,
    eligibility_snapshot_json jsonb,
    blocked_reasons_json jsonb,
    sellable_status_json jsonb,
    sellable_captured_at timestamp with time zone,
    call_restriction boolean NOT NULL,
    not_for_quote_cart_draft boolean NOT NULL,
    no_direct_order_update boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    accepted_at timestamp with time zone,
    rejected_at timestamp with time zone,
    reject_reason text,
    evidence_refs_json jsonb,
    audit_refs_json jsonb,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_confirmation_tasks" PRIMARY KEY (id),
    CONSTRAINT "AK_ivr_confirmation_tasks_task_id" UNIQUE (task_id),
    CONSTRAINT ck_ivr_confirmation_tasks_attempt_bounds CHECK (max_attempts BETWEEN 1 AND 10),
    CONSTRAINT ck_ivr_confirmation_tasks_masked_phone CHECK (phone_masked ~ '[xX*]' AND phone_masked !~ '(^|[^0-9])(0|84)[0-9]{9}([^0-9]|$)'),
    CONSTRAINT ck_ivr_confirmation_tasks_matrix CHECK ((program_type = 'GOLDEN_HOUR' AND payment_method_snapshot = 'ONLINE') OR (program_type = 'TWENTY_FOUR_SEVEN' AND payment_method_snapshot = 'COD')),
    CONSTRAINT ck_ivr_confirmation_tasks_required_flag CHECK (ivr_confirmation_required IS TRUE),
    CONSTRAINT ck_ivr_confirmation_tasks_signal_only CHECK (not_for_quote_cart_draft IS TRUE AND no_direct_order_update IS TRUE),
    CONSTRAINT ck_ivr_confirmation_tasks_token_ttl CHECK (dial_token_expires_at >= confirmation_window_started_at AND dial_token_expires_at <= confirmation_window_expires_at),
    CONSTRAINT ck_ivr_confirmation_tasks_window CHECK (confirmation_window_started_at < confirmation_window_expires_at AND expires_at = confirmation_window_expires_at)
);

CREATE TABLE ivr_evidence (
    evidence_ref text NOT NULL,
    kind text NOT NULL,
    correlation_id text NOT NULL,
    work_id text NOT NULL,
    payload_ref text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_evidence" PRIMARY KEY (evidence_ref)
);

CREATE TABLE ivr_evidence_links (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    owner_table text NOT NULL,
    owner_id text NOT NULL,
    evidence_ref text NOT NULL,
    audit_ref text,
    CONSTRAINT "PK_ivr_evidence_links" PRIMARY KEY (id)
);

CREATE TABLE ivr_feature_flags (
    key character varying(80) NOT NULL,
    env character varying(24) NOT NULL,
    enabled boolean NOT NULL,
    revision bigint NOT NULL,
    value_json jsonb NOT NULL,
    updated_by character varying(128) NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    reason character varying(500) NOT NULL,
    CONSTRAINT "PK_ivr_feature_flags" PRIMARY KEY (key, env)
);

CREATE TABLE ivr_idempotency_keys (
    scope text NOT NULL,
    key text NOT NULL,
    payload_hash text NOT NULL,
    response_snapshot_json jsonb NOT NULL,
    created_at timestamp with time zone NOT NULL,
    expires_at timestamp with time zone,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_idempotency_keys" PRIMARY KEY (scope, key)
);

CREATE TABLE ivr_review_items (
    review_item_id text NOT NULL,
    source_type text NOT NULL,
    source_id text NOT NULL,
    reason text NOT NULL,
    status text NOT NULL,
    assigned_to text,
    resolution text,
    correlation_id text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    resolved_at timestamp with time zone,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_review_items" PRIMARY KEY (review_item_id)
);

CREATE TABLE ivr_sim_channels (
    sim_channel_id text NOT NULL,
    sim_number_ref text NOT NULL,
    enabled boolean NOT NULL,
    status text NOT NULL,
    adapter_mode text NOT NULL,
    execution_mode text NOT NULL,
    provider_name text NOT NULL,
    active_call_job_id text,
    fail_count integer NOT NULL,
    last_health_check_at timestamp with time zone,
    cooldown_until timestamp with time zone,
    quarantine_until timestamp with time zone,
    disabled_reason text,
    lease_token uuid,
    lease_fencing_generation bigint NOT NULL,
    leased_by_worker_id text,
    lease_acquired_at timestamp with time zone,
    lease_expires_at timestamp with time zone,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_sim_channels" PRIMARY KEY (sim_channel_id),
    CONSTRAINT ck_ivr_sim_channels_fencing CHECK (lease_fencing_generation >= 0),
    CONSTRAINT ck_ivr_sim_channels_lease CHECK (active_call_job_id IS NULL OR lease_token IS NOT NULL),
    CONSTRAINT ck_ivr_sim_channels_mode CHECK (execution_mode IN ('MOCK','LAB_REAL_SIM','PRODUCTION_REAL'))
);

CREATE TABLE ivr_call_jobs (
    ivr_call_job_id text NOT NULL,
    task_id text NOT NULL,
    official_order_id text NOT NULL,
    order_version_snapshot text NOT NULL,
    program_type text NOT NULL,
    attempt_policy_code text NOT NULL,
    status text NOT NULL,
    max_attempts integer NOT NULL,
    attempt_offsets_seconds_json jsonb NOT NULL,
    confirmation_window_seconds integer NOT NULL,
    attempt_schedule_json jsonb NOT NULL,
    t0_at timestamp with time zone NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    eligible boolean NOT NULL,
    eligibility_decision text NOT NULL,
    queue_status text NOT NULL,
    capacity_incident_id text,
    script_version text NOT NULL,
    privacy_policy_version text NOT NULL,
    input_signal_only boolean NOT NULL,
    no_direct_order_update boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    closed_at timestamp with time zone,
    closed_reason text,
    evidence_refs_json jsonb,
    audit_refs_json jsonb,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_call_jobs" PRIMARY KEY (ivr_call_job_id),
    CONSTRAINT ck_ivr_call_jobs_attempt_bounds CHECK (max_attempts BETWEEN 1 AND 10),
    CONSTRAINT ck_ivr_call_jobs_signal_only CHECK (input_signal_only IS TRUE AND no_direct_order_update IS TRUE),
    CONSTRAINT ck_ivr_call_jobs_window_positive CHECK (confirmation_window_seconds > 0 AND t0_at < expires_at),
    CONSTRAINT "FK_ivr_call_jobs_ivr_confirmation_tasks_task_id" FOREIGN KEY (task_id) REFERENCES ivr_confirmation_tasks (task_id) ON DELETE RESTRICT
);

CREATE TABLE ivr_call_attempts (
    ivr_call_attempt_id text NOT NULL,
    ivr_call_job_id text NOT NULL,
    task_id text NOT NULL,
    attempt_number integer NOT NULL,
    max_attempts_snapshot integer NOT NULL,
    scheduled_at timestamp with time zone NOT NULL,
    scheduled_window_expires_at timestamp with time zone NOT NULL,
    started_at timestamp with time zone,
    ended_at timestamp with time zone,
    status text NOT NULL,
    result_status text,
    dtmf_key text,
    disposition text,
    is_counted_customer_attempt boolean NOT NULL,
    technical_retry_allowed boolean NOT NULL,
    technical_retry_count integer NOT NULL,
    no_answer boolean NOT NULL,
    invalid_phone boolean NOT NULL,
    technical_exception_type text,
    sim_channel_id text,
    provider_call_id text,
    raw_call_event_id text,
    blocked_reason text,
    policy_version text NOT NULL,
    script_version text NOT NULL,
    evidence_refs_json jsonb,
    audit_refs_json jsonb,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_call_attempts" PRIMARY KEY (ivr_call_attempt_id),
    CONSTRAINT ck_ivr_call_attempts_number_snapshot CHECK (attempt_number >= 1 AND attempt_number <= max_attempts_snapshot AND max_attempts_snapshot BETWEEN 1 AND 10),
    CONSTRAINT ck_ivr_call_attempts_retry_nonnegative CHECK (technical_retry_count >= 0),
    CONSTRAINT ck_ivr_call_attempts_technical_not_counted CHECK (technical_exception_type IS NULL OR is_counted_customer_attempt IS FALSE),
    CONSTRAINT "FK_ivr_call_attempts_ivr_call_jobs_ivr_call_job_id" FOREIGN KEY (ivr_call_job_id) REFERENCES ivr_call_jobs (ivr_call_job_id) ON DELETE RESTRICT
);

CREATE TABLE ivr_call_results (
    ivr_call_result_id text NOT NULL,
    ivr_call_job_id text NOT NULL,
    task_id text NOT NULL,
    official_order_id text NOT NULL,
    order_version_snapshot text,
    order_version_seen_by_ivr text NOT NULL,
    final_result_status text NOT NULL,
    result_type text NOT NULL,
    result_reason text,
    dtmf_key text,
    is_counted_customer_attempt boolean NOT NULL,
    is_final_for_ivr boolean NOT NULL,
    recommended_core_action text NOT NULL,
    core_order_handoff_required boolean NOT NULL,
    human_review_required boolean NOT NULL,
    input_signal_only boolean NOT NULL,
    no_direct_order_update boolean NOT NULL,
    no_payment_or_revenue_effect boolean NOT NULL,
    technical_error_code text,
    created_at timestamp with time zone NOT NULL,
    evidence_refs_json jsonb,
    audit_refs_json jsonb,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_call_results" PRIMARY KEY (ivr_call_result_id),
    CONSTRAINT ck_ivr_call_results_signal_only CHECK (input_signal_only IS TRUE AND no_direct_order_update IS TRUE AND no_payment_or_revenue_effect IS TRUE),
    CONSTRAINT "FK_ivr_call_results_ivr_call_jobs_ivr_call_job_id" FOREIGN KEY (ivr_call_job_id) REFERENCES ivr_call_jobs (ivr_call_job_id) ON DELETE RESTRICT
);

CREATE TABLE ivr_raw_call_events (
    raw_event_id text NOT NULL,
    ivr_call_attempt_id text NOT NULL,
    ivr_call_job_id text NOT NULL,
    provider_internal_payload_ref text,
    raw_call_status text NOT NULL,
    raw_dtmf text,
    audio_status text,
    technical_error_code text,
    recording_ref text,
    received_at timestamp with time zone NOT NULL,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_raw_call_events" PRIMARY KEY (raw_event_id),
    CONSTRAINT ck_ivr_raw_call_events_recording_off CHECK (recording_ref IS NULL),
    CONSTRAINT "FK_ivr_raw_call_events_ivr_call_attempts_ivr_call_attempt_id" FOREIGN KEY (ivr_call_attempt_id) REFERENCES ivr_call_attempts (ivr_call_attempt_id) ON DELETE RESTRICT
);

CREATE TABLE ivr_technical_exceptions (
    technical_exception_id text NOT NULL,
    ivr_call_attempt_id text NOT NULL,
    exception_type text NOT NULL,
    customer_attempt_counted boolean NOT NULL,
    technical_retry_allowed boolean NOT NULL,
    technical_retry_count integer NOT NULL,
    retry_reason text,
    correlation_id text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_technical_exceptions" PRIMARY KEY (technical_exception_id),
    CONSTRAINT ck_ivr_technical_exceptions_not_counted CHECK (customer_attempt_counted IS FALSE AND technical_retry_count >= 0),
    CONSTRAINT "FK_ivr_technical_exceptions_ivr_call_attempts_ivr_call_attempt~" FOREIGN KEY (ivr_call_attempt_id) REFERENCES ivr_call_attempts (ivr_call_attempt_id) ON DELETE RESTRICT
);

CREATE TABLE ivr_result_callbacks (
    callback_id text NOT NULL,
    ivr_call_result_id text NOT NULL,
    task_id text NOT NULL,
    official_order_id text NOT NULL,
    idempotency_key text NOT NULL,
    result_status text NOT NULL,
    result_state text NOT NULL,
    delivery_status text NOT NULL,
    requires_core_revalidation boolean NOT NULL,
    payload_json jsonb NOT NULL,
    payload_sha256 text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    sent_at timestamp with time zone,
    acknowledged_at timestamp with time zone,
    core_http_status integer,
    core_response_code text,
    retry_count integer NOT NULL,
    last_retry_at timestamp with time zone,
    next_retry_at timestamp with time zone,
    last_error text,
    lease_token text,
    lease_expires_at timestamp with time zone,
    retention_class text NOT NULL,
    retain_until timestamp with time zone,
    CONSTRAINT "PK_ivr_result_callbacks" PRIMARY KEY (callback_id),
    CONSTRAINT ck_ivr_result_callbacks_hash CHECK (payload_sha256 ~ '^[A-F0-9]{64}$'),
    CONSTRAINT ck_ivr_result_callbacks_retry_nonnegative CHECK (retry_count >= 0),
    CONSTRAINT ck_ivr_result_callbacks_revalidation CHECK (requires_core_revalidation IS TRUE),
    CONSTRAINT "FK_ivr_result_callbacks_ivr_call_results_ivr_call_result_id" FOREIGN KEY (ivr_call_result_id) REFERENCES ivr_call_results (ivr_call_result_id) ON DELETE RESTRICT
);

INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('dev', 'attemptPolicyVersion', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"mock-lab-v1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('lab', 'attemptPolicyVersion', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"mock-lab-v1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('pilot', 'attemptPolicyVersion', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"mock-lab-v1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('prod', 'attemptPolicyVersion', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"mock-lab-v1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('staging', 'attemptPolicyVersion', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"mock-lab-v1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('dev', 'executionMode', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('lab', 'executionMode', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('pilot', 'executionMode', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('prod', 'executionMode', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('staging', 'executionMode', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('dev', 'globalDialKillSwitch', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'true');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('lab', 'globalDialKillSwitch', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'true');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('pilot', 'globalDialKillSwitch', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'true');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('prod', 'globalDialKillSwitch', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'true');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('staging', 'globalDialKillSwitch', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'true');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('dev', 'labDestinationAllowlist', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '[]');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('lab', 'labDestinationAllowlist', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '[]');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('pilot', 'labDestinationAllowlist', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '[]');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('prod', 'labDestinationAllowlist', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '[]');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('staging', 'labDestinationAllowlist', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '[]');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('dev', 'realCustomerCallAllowed', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('lab', 'realCustomerCallAllowed', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('pilot', 'realCustomerCallAllowed', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('prod', 'realCustomerCallAllowed', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('staging', 'realCustomerCallAllowed', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('dev', 'recordingEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('lab', 'recordingEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('pilot', 'recordingEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('prod', 'recordingEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('staging', 'recordingEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('dev', 'salesProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"FAKE_TARGET_V1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('lab', 'salesProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"FAKE_TARGET_V1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('pilot', 'salesProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"FAKE_TARGET_V1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('prod', 'salesProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"FAKE_TARGET_V1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('staging', 'salesProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"FAKE_TARGET_V1"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('dev', 'simProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('lab', 'simProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('pilot', 'simProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('prod', 'simProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('staging', 'simProvider', TRUE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', '"MOCK"');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('dev', 'v1NotificationEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('lab', 'v1NotificationEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('pilot', 'v1NotificationEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('prod', 'v1NotificationEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');
INSERT INTO ivr_feature_flags (env, key, enabled, reason, revision, updated_at, updated_by, value_json)
VALUES ('staging', 'v1NotificationEnabled', FALSE, 'safe bootstrap seed', 0, TIMESTAMPTZ '1970-01-01T00:00:00+00:00', 'bootstrap', 'false');

CREATE INDEX "IX_ivr_admin_actions_correlation_id" ON ivr_admin_actions (correlation_id);

CREATE INDEX "IX_ivr_admin_actions_created_at" ON ivr_admin_actions (created_at);

CREATE INDEX "IX_ivr_attempt_policies_approved_for_production" ON ivr_attempt_policies (approved_for_production);

CREATE INDEX "IX_ivr_audit_log_correlation_id" ON ivr_audit_log (correlation_id);

CREATE INDEX "IX_ivr_audit_log_created_at" ON ivr_audit_log (created_at);

CREATE INDEX "IX_ivr_audit_log_target_type_target_id" ON ivr_audit_log (target_type, target_id);

CREATE INDEX "IX_ivr_call_attempts_disposition" ON ivr_call_attempts (disposition);

CREATE INDEX "IX_ivr_call_attempts_is_counted_customer_attempt" ON ivr_call_attempts (is_counted_customer_attempt);

CREATE UNIQUE INDEX "IX_ivr_call_attempts_ivr_call_job_id_attempt_number" ON ivr_call_attempts (ivr_call_job_id, attempt_number) WHERE is_counted_customer_attempt IS TRUE;

CREATE INDEX "IX_ivr_call_attempts_provider_call_id" ON ivr_call_attempts (provider_call_id);

CREATE INDEX "IX_ivr_call_attempts_raw_call_event_id" ON ivr_call_attempts (raw_call_event_id);

CREATE INDEX "IX_ivr_call_attempts_result_status" ON ivr_call_attempts (result_status);

CREATE INDEX "IX_ivr_call_attempts_scheduled_at" ON ivr_call_attempts (scheduled_at);

CREATE INDEX "IX_ivr_call_attempts_scheduled_window_expires_at" ON ivr_call_attempts (scheduled_window_expires_at);

CREATE INDEX "IX_ivr_call_attempts_sim_channel_id" ON ivr_call_attempts (sim_channel_id);

CREATE INDEX "IX_ivr_call_attempts_status" ON ivr_call_attempts (status);

CREATE INDEX "IX_ivr_call_attempts_task_id" ON ivr_call_attempts (task_id);

CREATE INDEX "IX_ivr_call_attempts_technical_exception_type" ON ivr_call_attempts (technical_exception_type);

CREATE INDEX "IX_ivr_call_jobs_capacity_incident_id" ON ivr_call_jobs (capacity_incident_id);

CREATE INDEX "IX_ivr_call_jobs_closed_at" ON ivr_call_jobs (closed_at);

CREATE INDEX "IX_ivr_call_jobs_created_at" ON ivr_call_jobs (created_at);

CREATE INDEX "IX_ivr_call_jobs_eligibility_decision" ON ivr_call_jobs (eligibility_decision);

CREATE INDEX "IX_ivr_call_jobs_eligible" ON ivr_call_jobs (eligible);

CREATE INDEX "IX_ivr_call_jobs_official_order_id_status" ON ivr_call_jobs (official_order_id, status);

CREATE INDEX "IX_ivr_call_jobs_order_version_snapshot" ON ivr_call_jobs (order_version_snapshot);

CREATE INDEX "IX_ivr_call_jobs_program_type_status" ON ivr_call_jobs (program_type, status);

CREATE INDEX "IX_ivr_call_jobs_queue_status" ON ivr_call_jobs (queue_status);

CREATE INDEX "IX_ivr_call_jobs_status_expires_at" ON ivr_call_jobs (status, expires_at);

CREATE INDEX "IX_ivr_call_jobs_t0_at" ON ivr_call_jobs (t0_at);

CREATE INDEX "IX_ivr_call_jobs_task_id" ON ivr_call_jobs (task_id);

CREATE INDEX "IX_ivr_call_results_created_at" ON ivr_call_results (created_at);

CREATE INDEX "IX_ivr_call_results_final_result_status" ON ivr_call_results (final_result_status);

CREATE INDEX "IX_ivr_call_results_human_review_required" ON ivr_call_results (human_review_required);

CREATE INDEX "IX_ivr_call_results_is_counted_customer_attempt" ON ivr_call_results (is_counted_customer_attempt);

CREATE INDEX "IX_ivr_call_results_is_final_for_ivr" ON ivr_call_results (is_final_for_ivr);

CREATE INDEX "IX_ivr_call_results_ivr_call_job_id" ON ivr_call_results (ivr_call_job_id);

CREATE INDEX "IX_ivr_call_results_official_order_id" ON ivr_call_results (official_order_id);

CREATE INDEX "IX_ivr_call_results_order_version_seen_by_ivr" ON ivr_call_results (order_version_seen_by_ivr);

CREATE INDEX "IX_ivr_call_results_order_version_snapshot" ON ivr_call_results (order_version_snapshot);

CREATE INDEX "IX_ivr_call_results_result_type" ON ivr_call_results (result_type);

CREATE INDEX "IX_ivr_call_results_task_id" ON ivr_call_results (task_id);

CREATE INDEX "IX_ivr_capacity_incidents_status_opened_at" ON ivr_capacity_incidents (status, opened_at);

CREATE INDEX "IX_ivr_confirmation_tasks_attempt_policy_version" ON ivr_confirmation_tasks (attempt_policy_version);

CREATE INDEX "IX_ivr_confirmation_tasks_confirmation_window_expires_at" ON ivr_confirmation_tasks (confirmation_window_expires_at);

CREATE INDEX "IX_ivr_confirmation_tasks_confirmation_window_started_at" ON ivr_confirmation_tasks (confirmation_window_started_at);

CREATE INDEX "IX_ivr_confirmation_tasks_correlation_id" ON ivr_confirmation_tasks (correlation_id);

CREATE INDEX "IX_ivr_confirmation_tasks_created_at" ON ivr_confirmation_tasks (created_at);

CREATE INDEX "IX_ivr_confirmation_tasks_customer_id" ON ivr_confirmation_tasks (customer_id);

CREATE INDEX "IX_ivr_confirmation_tasks_dial_token_expires_at" ON ivr_confirmation_tasks (dial_token_expires_at);

CREATE INDEX "IX_ivr_confirmation_tasks_eligibility_decision" ON ivr_confirmation_tasks (eligibility_decision);

CREATE INDEX "IX_ivr_confirmation_tasks_expires_at" ON ivr_confirmation_tasks (expires_at);

CREATE UNIQUE INDEX "IX_ivr_confirmation_tasks_idempotency_key" ON ivr_confirmation_tasks (idempotency_key);

CREATE INDEX "IX_ivr_confirmation_tasks_ivr_confirmation_required" ON ivr_confirmation_tasks (ivr_confirmation_required);

CREATE INDEX "IX_ivr_confirmation_tasks_official_contact_id" ON ivr_confirmation_tasks (official_contact_id);

CREATE INDEX "IX_ivr_confirmation_tasks_official_order_id" ON ivr_confirmation_tasks (official_order_id);

CREATE INDEX "IX_ivr_confirmation_tasks_order_state" ON ivr_confirmation_tasks (order_state);

CREATE INDEX "IX_ivr_confirmation_tasks_order_version" ON ivr_confirmation_tasks (order_version);

CREATE INDEX "IX_ivr_confirmation_tasks_payment_method_snapshot" ON ivr_confirmation_tasks (payment_method_snapshot);

CREATE INDEX "IX_ivr_confirmation_tasks_phone_validation_status" ON ivr_confirmation_tasks (phone_validation_status);

CREATE INDEX "IX_ivr_confirmation_tasks_program_type" ON ivr_confirmation_tasks (program_type);

CREATE INDEX "IX_ivr_evidence_correlation_id" ON ivr_evidence (correlation_id);

CREATE INDEX "IX_ivr_evidence_created_at" ON ivr_evidence (created_at);

CREATE INDEX "IX_ivr_evidence_work_id" ON ivr_evidence (work_id);

CREATE INDEX "IX_ivr_evidence_links_evidence_ref" ON ivr_evidence_links (evidence_ref);

CREATE INDEX "IX_ivr_evidence_links_owner_table_owner_id" ON ivr_evidence_links (owner_table, owner_id);

CREATE INDEX "IX_ivr_feature_flags_env_revision" ON ivr_feature_flags (env, revision);

CREATE UNIQUE INDEX "IX_ivr_feature_flags_key_env" ON ivr_feature_flags (key, env);

CREATE INDEX "IX_ivr_idempotency_keys_created_at" ON ivr_idempotency_keys (created_at);

CREATE INDEX "IX_ivr_idempotency_keys_expires_at" ON ivr_idempotency_keys (expires_at);

CREATE INDEX "IX_ivr_raw_call_events_ivr_call_attempt_id" ON ivr_raw_call_events (ivr_call_attempt_id);

CREATE INDEX "IX_ivr_raw_call_events_ivr_call_job_id" ON ivr_raw_call_events (ivr_call_job_id);

CREATE INDEX "IX_ivr_raw_call_events_received_at" ON ivr_raw_call_events (received_at);

CREATE INDEX "IX_ivr_raw_call_events_technical_error_code" ON ivr_raw_call_events (technical_error_code);

CREATE INDEX "IX_ivr_result_callbacks_acknowledged_at" ON ivr_result_callbacks (acknowledged_at);

CREATE INDEX "IX_ivr_result_callbacks_core_http_status" ON ivr_result_callbacks (core_http_status);

CREATE INDEX "IX_ivr_result_callbacks_core_response_code" ON ivr_result_callbacks (core_response_code);

CREATE INDEX "IX_ivr_result_callbacks_delivery_status_next_retry_at" ON ivr_result_callbacks (delivery_status, next_retry_at);

CREATE UNIQUE INDEX "IX_ivr_result_callbacks_idempotency_key" ON ivr_result_callbacks (idempotency_key);

CREATE INDEX "IX_ivr_result_callbacks_ivr_call_result_id" ON ivr_result_callbacks (ivr_call_result_id);

CREATE INDEX "IX_ivr_result_callbacks_lease_expires_at" ON ivr_result_callbacks (lease_expires_at);

CREATE INDEX "IX_ivr_result_callbacks_official_order_id" ON ivr_result_callbacks (official_order_id);

CREATE INDEX "IX_ivr_result_callbacks_result_status_result_state" ON ivr_result_callbacks (result_status, result_state);

CREATE INDEX "IX_ivr_result_callbacks_sent_at" ON ivr_result_callbacks (sent_at);

CREATE INDEX "IX_ivr_result_callbacks_task_id" ON ivr_result_callbacks (task_id);

CREATE INDEX "IX_ivr_review_items_correlation_id" ON ivr_review_items (correlation_id);

CREATE INDEX "IX_ivr_review_items_source_type_source_id" ON ivr_review_items (source_type, source_id);

CREATE INDEX "IX_ivr_review_items_status_created_at" ON ivr_review_items (status, created_at);

CREATE INDEX "IX_ivr_sim_channels_lease_expires_at" ON ivr_sim_channels (lease_expires_at);

CREATE UNIQUE INDEX "IX_ivr_sim_channels_lease_token" ON ivr_sim_channels (lease_token) WHERE lease_token IS NOT NULL;

CREATE INDEX "IX_ivr_sim_channels_quarantine_until" ON ivr_sim_channels (quarantine_until);

CREATE INDEX "IX_ivr_sim_channels_status_enabled_cooldown_until" ON ivr_sim_channels (status, enabled, cooldown_until);

CREATE INDEX "IX_ivr_technical_exceptions_correlation_id" ON ivr_technical_exceptions (correlation_id);

CREATE INDEX "IX_ivr_technical_exceptions_created_at" ON ivr_technical_exceptions (created_at);

CREATE INDEX "IX_ivr_technical_exceptions_ivr_call_attempt_id" ON ivr_technical_exceptions (ivr_call_attempt_id);

CREATE OR REPLACE FUNCTION ivr_enforce_confirmation_task_snapshot_immutable()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
BEGIN
    IF NEW.contract_version IS DISTINCT FROM OLD.contract_version
       OR NEW.official_order_id IS DISTINCT FROM OLD.official_order_id
       OR NEW.order_version IS DISTINCT FROM OLD.order_version
       OR NEW.order_state IS DISTINCT FROM OLD.order_state
       OR NEW.payment_method_snapshot IS DISTINCT FROM OLD.payment_method_snapshot
       OR NEW.program_type IS DISTINCT FROM OLD.program_type
       OR NEW.attempt_policy_version IS DISTINCT FROM OLD.attempt_policy_version
       OR NEW.max_attempts IS DISTINCT FROM OLD.max_attempts
       OR NEW.attempt_offsets_seconds_json IS DISTINCT FROM OLD.attempt_offsets_seconds_json
       OR NEW.confirmation_window_started_at IS DISTINCT FROM OLD.confirmation_window_started_at
       OR NEW.confirmation_window_expires_at IS DISTINCT FROM OLD.confirmation_window_expires_at
       OR NEW.phone_ref IS DISTINCT FROM OLD.phone_ref
       OR NEW.dial_token_ciphertext IS DISTINCT FROM OLD.dial_token_ciphertext
       OR NEW.dial_token_expires_at IS DISTINCT FROM OLD.dial_token_expires_at
       OR NEW.privacy_safe_order_summary_json IS DISTINCT FROM OLD.privacy_safe_order_summary_json THEN
        RAISE EXCEPTION 'confirmation-task contract/policy/speech snapshot is immutable';
    END IF;

    RETURN NEW;
END;
$function$;

CREATE TRIGGER trg_ivr_confirmation_tasks_snapshot_immutable
BEFORE UPDATE ON ivr_confirmation_tasks
FOR EACH ROW
EXECUTE FUNCTION ivr_enforce_confirmation_task_snapshot_immutable();

CREATE OR REPLACE FUNCTION ivr_reject_attempt_policy_update()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
BEGIN
    RAISE EXCEPTION 'attempt-policy versions are immutable; create a new version';
END;
$function$;

CREATE TRIGGER trg_ivr_attempt_policies_immutable
BEFORE UPDATE ON ivr_attempt_policies
FOR EACH ROW
EXECUTE FUNCTION ivr_reject_attempt_policy_update();

CREATE OR REPLACE FUNCTION ivr_enforce_call_job_policy_snapshot_immutable()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
BEGIN
    IF NEW.attempt_policy_code IS DISTINCT FROM OLD.attempt_policy_code
       OR NEW.max_attempts IS DISTINCT FROM OLD.max_attempts
       OR NEW.attempt_offsets_seconds_json IS DISTINCT FROM OLD.attempt_offsets_seconds_json
       OR NEW.confirmation_window_seconds IS DISTINCT FROM OLD.confirmation_window_seconds
       OR NEW.attempt_schedule_json IS DISTINCT FROM OLD.attempt_schedule_json THEN
        RAISE EXCEPTION 'call-job policy snapshot is immutable';
    END IF;

    RETURN NEW;
END;
$function$;

CREATE TRIGGER trg_ivr_call_jobs_policy_snapshot_immutable
BEFORE UPDATE ON ivr_call_jobs
FOR EACH ROW
EXECUTE FUNCTION ivr_enforce_call_job_policy_snapshot_immutable();

CREATE OR REPLACE FUNCTION ivr_enforce_attempt_snapshot()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
DECLARE
    expected_max_attempts integer;
BEGIN
    IF TG_OP = 'UPDATE' THEN
        IF NEW.max_attempts_snapshot IS DISTINCT FROM OLD.max_attempts_snapshot
           OR NEW.attempt_number IS DISTINCT FROM OLD.attempt_number THEN
            RAISE EXCEPTION 'call-attempt numbering snapshot is immutable';
        END IF;

        RETURN NEW;
    END IF;

    SELECT max_attempts
    INTO expected_max_attempts
    FROM ivr_call_jobs
    WHERE ivr_call_job_id = NEW.ivr_call_job_id;

    IF expected_max_attempts IS NULL THEN
        RAISE EXCEPTION 'call job % does not exist', NEW.ivr_call_job_id;
    END IF;

    NEW.max_attempts_snapshot := expected_max_attempts;
    RETURN NEW;
END;
$function$;

CREATE TRIGGER trg_ivr_call_attempts_snapshot
BEFORE INSERT OR UPDATE ON ivr_call_attempts
FOR EACH ROW
EXECUTE FUNCTION ivr_enforce_attempt_snapshot();

CREATE OR REPLACE FUNCTION ivr_enforce_callback_payload_immutable()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
BEGIN
    IF NEW.ivr_call_result_id IS DISTINCT FROM OLD.ivr_call_result_id
       OR NEW.task_id IS DISTINCT FROM OLD.task_id
       OR NEW.official_order_id IS DISTINCT FROM OLD.official_order_id
       OR NEW.idempotency_key IS DISTINCT FROM OLD.idempotency_key
       OR NEW.result_status IS DISTINCT FROM OLD.result_status
       OR NEW.result_state IS DISTINCT FROM OLD.result_state
       OR NEW.payload_json IS DISTINCT FROM OLD.payload_json
       OR NEW.payload_sha256 IS DISTINCT FROM OLD.payload_sha256
       OR NEW.requires_core_revalidation IS DISTINCT FROM OLD.requires_core_revalidation THEN
        RAISE EXCEPTION 'result-callback business payload is immutable';
    END IF;

    RETURN NEW;
END;
$function$;

CREATE TRIGGER trg_ivr_result_callbacks_payload_immutable
BEFORE UPDATE ON ivr_result_callbacks
FOR EACH ROW
EXECUTE FUNCTION ivr_enforce_callback_payload_immutable();

CREATE OR REPLACE FUNCTION ivr_reject_audit_mutation()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
BEGIN
    RAISE EXCEPTION 'audit rows are append-only';
END;
$function$;

CREATE TRIGGER trg_ivr_audit_log_append_only
BEFORE UPDATE OR DELETE ON ivr_audit_log
FOR EACH ROW
EXECUTE FUNCTION ivr_reject_audit_mutation();

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260812142435_P1_2_InitialTargetV1Persistence', '10.0.11');

COMMIT;
