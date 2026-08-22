-- W-0104 local preflight only. This script is intentionally not a migration and
-- must only be mounted by docker-compose.softphone.yml against a disposable lab DB.
BEGIN;

INSERT INTO ivr_attempt_policies (
    policy_version, program_type, max_attempts, attempt_offsets_seconds_json,
    confirmation_window_seconds, allowed_execution_modes_json, approved_for_production,
    created_at, retention_class, retain_until)
VALUES
    ('lab-softphone-v1', 'GOLDEN_HOUR', 1, '[0]', 300, '["LAB_REAL_SIM"]', false,
     TIMESTAMPTZ '2026-08-20 00:00:00+00', 'LEGAL_DECISION_PENDING', NULL),
    ('lab-softphone-v1', 'TWENTY_FOUR_SEVEN', 1, '[0]', 300, '["LAB_REAL_SIM"]', false,
     TIMESTAMPTZ '2026-08-20 00:00:00+00', 'LEGAL_DECISION_PENDING', NULL)
ON CONFLICT (policy_version, program_type) DO NOTHING;

INSERT INTO ivr_script_approvals (
    id, script_version_id, approval_type, actor_id, reason, correlation_id, approved_at)
VALUES (
    '27000000-0000-0000-0000-000000000104',
    '27000000-0000-0000-0000-000000000001',
    'LAB',
    'w0104-lab-reviewer',
    'W-0104 synthetic softphone lab fixture only',
    'W-0104-SEED',
    TIMESTAMPTZ '2026-08-20 00:00:00+00')
ON CONFLICT (script_version_id, approval_type) DO NOTHING;

INSERT INTO ivr_script_approvals (
    id, script_version_id, approval_type, actor_id, reason, correlation_id, approved_at)
SELECT
    '27000000-0000-0000-0000-000000000105',
    version.id,
    'LAB',
    'w0104-lab-reviewer',
    'W-0104 synthetic softphone lab script v2 only',
    'W-0104-SCRIPT-V2-LAB',
    TIMESTAMPTZ '2026-08-22 03:40:00+00'
FROM ivr_script_versions version
WHERE version.template_id = 'SCRIPT-ORDER-CONFIRM'
  AND version.version = 'v2-test-approved'
  AND version.status = 'APPROVED'
ON CONFLICT (script_version_id, approval_type) DO NOTHING;

INSERT INTO ivr_script_approvals (
    id, script_version_id, approval_type, actor_id, reason, correlation_id, approved_at)
SELECT
    '27000000-0000-0000-0000-000000000106',
    version.id,
    'LAB',
    'w0104-lab-reviewer',
    'W-0104 synthetic softphone lab script v3 only',
    'W-0104-SCRIPT-V3-LAB',
    TIMESTAMPTZ '2026-08-22 04:10:00+00'
FROM ivr_script_versions version
WHERE version.template_id = 'SCRIPT-ORDER-CONFIRM'
  AND version.version = 'v3-test-approved'
  AND version.status = 'APPROVED'
ON CONFLICT (script_version_id, approval_type) DO NOTHING;

UPDATE ivr_feature_flags
SET value_json = CASE key
        WHEN 'executionMode' THEN '"LAB_REAL_SIM"'::jsonb
        WHEN 'salesProvider' THEN '"FAKE_TARGET_V1"'::jsonb
        WHEN 'simProvider' THEN '"VENDOR"'::jsonb
        WHEN 'attemptPolicyVersion' THEN '"lab-softphone-v1"'::jsonb
        WHEN 'realCustomerCallAllowed' THEN 'false'::jsonb
        WHEN 'labDestinationAllowlist' THEN '["LAB-A"]'::jsonb
        WHEN 'globalDialKillSwitch' THEN 'false'::jsonb
        WHEN 'v1NotificationEnabled' THEN 'false'::jsonb
        WHEN 'recordingEnabled' THEN 'false'::jsonb
        ELSE value_json
    END,
    enabled = CASE key
        WHEN 'labDestinationAllowlist' THEN true
        WHEN 'globalDialKillSwitch' THEN false
        WHEN 'realCustomerCallAllowed' THEN false
        WHEN 'v1NotificationEnabled' THEN false
        WHEN 'recordingEnabled' THEN false
        ELSE true
    END,
    revision = 104,
    updated_by = 'w0104-local-seed',
    updated_at = TIMESTAMPTZ '2026-08-20 00:00:00+00',
    reason = 'W-0104 disposable local softphone preflight'
WHERE env = 'lab';

DO $$
DECLARE
    flag_count integer;
BEGIN
    SELECT count(*) INTO flag_count
    FROM ivr_feature_flags
    WHERE env = 'lab' AND revision = 104;

    IF flag_count <> 9 THEN
        RAISE EXCEPTION 'W-0104 requires all 9 lab feature-flag rows at revision 104';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM ivr_script_versions version
        JOIN ivr_script_approvals approval
          ON approval.script_version_id = version.id
        WHERE version.template_id = 'SCRIPT-ORDER-CONFIRM'
          AND version.version = 'v3-test-approved'
          AND version.status = 'APPROVED'
          AND approval.approval_type = 'LAB')
    THEN
        RAISE EXCEPTION 'W-0104 LAB script v3 approval was not installed';
    END IF;
END $$;

COMMIT;
