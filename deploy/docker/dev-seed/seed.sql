-- W-0043 / P7-1 — the one row the dev stack was missing before a task could reach a dial.
--
-- Scope is deliberately narrow. Going looking for what to seed turned up that most of it is
-- already owned elsewhere and seeding it again would create a second source of truth:
--
--   the approved script     migration P2_7_ScriptContentLifecycle (ids 27000000-…)
--   its MOCK_TEST approval  the same migration
--   the MOCK SIM channel    MockSimChannelProvisioner, in the worker at startup
--   the ATTEMPT POLICY      nobody -- which is the gap
--
-- So this file inserts the attempt policy and ASSERTS the rest. If a migration stops seeding the
-- script, the assertion fails here with a sentence rather than surfacing later as a task that was
-- accepted and then silently never called.
--
-- What keeps it honest: every value has to be right for IT-IMG-E2E-05 to go green. The seed is
-- validated by USE, not by review -- a wrong enum or a drifted offset shows up as a red test.
--
-- SAFETY. This cannot enable a real call, structurally rather than by promise: the policy allows
-- execution mode MOCK and nothing else, so PostgresAttemptPolicyRegistry refuses it outside MOCK,
-- and approved_for_production is false. No row here is a credential and none carries a phone
-- number.
--
-- Idempotent: applied on every run of the smoke, and safe against an already-seeded stack.

BEGIN;

-- Mirrors CandidateAttemptPolicies (mock-lab-v1) field for field, because TaskIntakeService
-- compares the wire snapshot against the stored policy and rejects the task when max attempts,
-- offsets or window length disagree.
INSERT INTO ivr_attempt_policies (
    policy_version, program_type, max_attempts, attempt_offsets_seconds_json,
    confirmation_window_seconds, allowed_execution_modes_json, approved_for_production,
    created_at, retention_class, retain_until)
VALUES
    ('mock-lab-v1', 'GOLDEN_HOUR', 2, '[0,150]', 300, '["MOCK"]', false,
     TIMESTAMPTZ '2026-08-12 00:00:00+00', 'LEGAL_DECISION_PENDING', NULL),
    ('mock-lab-v1', 'TWENTY_FOUR_SEVEN', 2, '[0,450]', 900, '["MOCK"]', false,
     TIMESTAMPTZ '2026-08-12 00:00:00+00', 'LEGAL_DECISION_PENDING', NULL)
ON CONFLICT (policy_version, program_type) DO NOTHING;

-- A one-attempt policy, and it exists for a reason the smoke could not work around. IVR_NO_ANSWER
-- is only FINAL once the last attempt is spent (DT-02), and under mock-lab-v1 that means waiting
-- out the second attempt offset -- 150s for Golden Hour, 450s for 24/7. A smoke that slept seven
-- minutes to observe one taxonomy value would be turned off, so the policy is what changes rather
-- than the clock. Same shape, same guards, one attempt.
INSERT INTO ivr_attempt_policies (
    policy_version, program_type, max_attempts, attempt_offsets_seconds_json,
    confirmation_window_seconds, allowed_execution_modes_json, approved_for_production,
    created_at, retention_class, retain_until)
VALUES
    ('mock-e2e-single-v1', 'GOLDEN_HOUR', 1, '[0]', 300, '["MOCK"]', false,
     TIMESTAMPTZ '2026-08-12 00:00:00+00', 'LEGAL_DECISION_PENDING', NULL),
    ('mock-e2e-single-v1', 'TWENTY_FOUR_SEVEN', 1, '[0]', 900, '["MOCK"]', false,
     TIMESTAMPTZ '2026-08-12 00:00:00+00', 'LEGAL_DECISION_PENDING', NULL)
ON CONFLICT (policy_version, program_type) DO NOTHING;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM ivr_script_versions version
        JOIN ivr_script_approvals approval
          ON approval.script_version_id = version.id
        WHERE version.template_id = 'SCRIPT-ORDER-CONFIRM'
          AND version.version = 'v1-test-approved'
          AND version.status = 'APPROVED'
          AND approval.approval_type = 'MOCK_TEST')
    THEN
        RAISE EXCEPTION
            'The MOCK script fixture is missing. Migration P2_7_ScriptContentLifecycle used to '
            'seed SCRIPT-ORDER-CONFIRM/v1-test-approved with a MOCK_TEST approval; without it the '
            'stack accepts a task and then never speaks.';
    END IF;
END $$;

COMMIT;
