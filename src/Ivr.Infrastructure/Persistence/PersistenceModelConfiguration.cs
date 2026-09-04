using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Scripts;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Persistence;

internal static class PersistenceModelConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        ConfigureTask(modelBuilder);
        ConfigurePolicy(modelBuilder);
        ConfigureJob(modelBuilder);
        ConfigureTaskIntakeOutbox(modelBuilder);
        ConfigureAttempt(modelBuilder);
        ConfigureRawEvent(modelBuilder);
        ConfigureResult(modelBuilder);
        ConfigureCallback(modelBuilder);
        ConfigureChannel(modelBuilder);
        ConfigureOperations(modelBuilder);
        ConfigureFoundation(modelBuilder);
        ConfigureRetention(modelBuilder);
        ConfigureScripts(modelBuilder);
        ApplyStorageConventions(modelBuilder);
    }

    private static void ConfigureTask(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<ConfirmationTaskEntity>();
        builder.ToTable(
            "ivr_confirmation_tasks",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_confirmation_tasks_attempt_bounds",
                    "max_attempts BETWEEN 1 AND 10");
                table.HasCheckConstraint(
                    "ck_ivr_confirmation_tasks_matrix",
                    "(program_type = 'GOLDEN_HOUR' AND payment_method_snapshot = 'ONLINE') OR "
                    + "(program_type = 'TWENTY_FOUR_SEVEN' AND payment_method_snapshot = 'COD')");
                table.HasCheckConstraint(
                    "ck_ivr_confirmation_tasks_required_flag",
                    "ivr_confirmation_required IS TRUE");
                table.HasCheckConstraint(
                    "ck_ivr_confirmation_tasks_signal_only",
                    "not_for_quote_cart_draft IS TRUE AND no_direct_order_update IS TRUE");
                table.HasCheckConstraint(
                    "ck_ivr_confirmation_tasks_window",
                    "confirmation_window_started_at < confirmation_window_expires_at "
                    + "AND expires_at = confirmation_window_expires_at");
                table.HasCheckConstraint(
                    "ck_ivr_confirmation_tasks_token_ttl",
                    "dial_token_expires_at >= confirmation_window_started_at "
                    + "AND dial_token_expires_at <= confirmation_window_expires_at");
                table.HasCheckConstraint(
                    "ck_ivr_confirmation_tasks_masked_phone",
                    "phone_masked ~ '[xX*]' AND phone_masked !~ '(^|[^0-9])(0|84)[0-9]{9}([^0-9]|$)'");
                // W-0030 / P4-2 §2.3. Lowercase hex only, matching ck_ivr_script_versions_hash.
                // The column may be null for rows written before this migration; it may never
                // hold anything that is not a SHA-256 digest, so no snapshot body can leak here.
                table.HasCheckConstraint(
                    "ck_ivr_confirmation_tasks_eligibility_hash",
                    "eligibility_snapshot_hash IS NULL "
                    + "OR eligibility_snapshot_hash ~ '^[a-f0-9]{64}$'");
                // OD-18 LEGACY_READ: retain the old value for historical rows and rollback.
                // New runtime code has no branch that emits it; do not narrow this constraint
                // until target-environment counts and the rollback window are closed.
                table.HasCheckConstraint(
                    "ck_ivr_confirmation_tasks_eligibility_decision",
                    "eligibility_decision IS NULL OR eligibility_decision IN ("
                    + "'PENDING_ELIGIBILITY','ELIGIBLE_FOR_IVR','TASK_BLOCKED_OPERATIONAL',"
                    + "'TASK_HELD_ADMIN_REVIEW','TASK_SKIPPED_TRUSTED_CUSTOMER',"
                    + "'IVR_CAPACITY_EXCEPTION')");
            });
        builder.HasKey(entity => entity.Id);
        builder.HasAlternateKey(entity => entity.TaskId);
        builder.Property(entity => entity.TraceParent)
            .HasMaxLength(Observability.TraceContextSnapshot.TraceParentMaxLength);
        builder.Property(entity => entity.TraceState)
            .HasMaxLength(Observability.TraceContextSnapshot.TraceStateMaxLength);
        builder.HasIndex(entity => entity.IdempotencyKey).IsUnique();
        builder.HasIndex(entity => entity.CorrelationId);
        builder.HasIndex(entity => entity.OfficialOrderId);
        builder.HasIndex(entity => entity.OrderVersion);
        builder.HasIndex(entity => entity.OrderState);
        builder.HasIndex(entity => entity.PaymentMethodSnapshot);
        builder.HasIndex(entity => entity.IvrConfirmationRequired);
        builder.HasIndex(entity => entity.CustomerId);
        builder.HasIndex(entity => entity.ProgramType);
        builder.HasIndex(entity => entity.AttemptPolicyVersion);
        builder.HasIndex(entity => entity.ConfirmationWindowStartedAt);
        builder.HasIndex(entity => entity.ConfirmationWindowExpiresAt);
        builder.HasIndex(entity => entity.OfficialContactId);
        builder.HasIndex(entity => entity.PhoneValidationStatus);
        builder.HasIndex(entity => entity.DialTokenExpiresAt);
        builder.HasIndex(entity => entity.EligibilityDecision);
        builder.HasIndex(entity => entity.CreatedAt);
        builder.HasIndex(entity => entity.ExpiresAt);
    }

    private static void ConfigurePolicy(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<AttemptPolicyEntity>();
        builder.ToTable(
            "ivr_attempt_policies",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_attempt_policies_attempt_bounds",
                    "max_attempts BETWEEN 1 AND 10");
                table.HasCheckConstraint(
                    "ck_ivr_attempt_policies_window_positive",
                    "confirmation_window_seconds > 0");
            });
        builder.HasKey(entity => new { entity.PolicyVersion, entity.ProgramType });
        builder.HasIndex(entity => entity.ApprovedForProduction);
    }

    private static void ConfigureJob(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<CallJobEntity>();
        builder.ToTable(
            "ivr_call_jobs",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_call_jobs_attempt_bounds",
                    "max_attempts BETWEEN 1 AND 10");
                table.HasCheckConstraint(
                    "ck_ivr_call_jobs_window_positive",
                    "confirmation_window_seconds > 0 AND t0_at < expires_at");
                table.HasCheckConstraint(
                    "ck_ivr_call_jobs_signal_only",
                    "input_signal_only IS TRUE AND no_direct_order_update IS TRUE");
                // SKIPPED remains LEGACY_READ because older jobs and retention paths may carry it.
                // OD-18 removes only the new trusted-customer write path, not historical reads.
                table.HasCheckConstraint(
                    "ck_ivr_call_jobs_status",
                    "status IN ('CREATED','DRY_RUN','OPEN','QUEUED','READY_FOR_SCHEDULER',"
                    + "'LEASED','LEASED_PENDING_DISPATCH','DISPATCH_LEASED','DIALING',"
                    + "'ACTIVE_CALL','DISPOSITION_PENDING_NORMALIZATION',"
                    + "'PROVIDER_EVENT_PENDING_NORMALIZATION','RESULT_READY_FOR_CALLBACK',"
                    + "'TECHNICAL_RETRY_QUEUED','HELD_MOCK','HELD_ADMIN_REVIEW',"
                    + "'HELD_ELIGIBILITY','HELD_CAPACITY','HELD_CALLBACK',"
                    + "'HELD_TECHNICAL_REVIEW','HELD_NORMALIZATION','HELD_LEASE_RECOVERY',"
                    + "'CAPACITY_HELD','CAPACITY_MISSED','CLOSED_CAPACITY','WINDOW_EXPIRED',"
                    + "'RECOVERY_REQUIRED','BLOCKED','SKIPPED','CLOSED')");
                table.HasCheckConstraint(
                    "ck_ivr_call_jobs_queue_status",
                    "queue_status IN ('QUEUED','HELD_MOCK','HELD_ELIGIBILITY','LEASED',"
                    + "'HELD_LEASE_RECOVERY','HELD_NORMALIZATION','HELD_CALLBACK',"
                    + "'HELD_TECHNICAL_REVIEW','HELD_CAPACITY','HELD_ADMIN_REVIEW',"
                    + "'SKIPPED','BLOCKED','CLOSED_CAPACITY','CLOSED_WINDOW_EXPIRED')");
                // OD-18 LEGACY_READ: see the task constraint above. No new eligibility evaluation
                // may produce TASK_SKIPPED_TRUSTED_CUSTOMER.
                table.HasCheckConstraint(
                    "ck_ivr_call_jobs_eligibility_decision",
                    "eligibility_decision IN ('PENDING_ELIGIBILITY','ELIGIBLE_FOR_IVR',"
                    + "'TASK_BLOCKED_OPERATIONAL','TASK_HELD_ADMIN_REVIEW',"
                    + "'TASK_SKIPPED_TRUSTED_CUSTOMER','IVR_CAPACITY_EXCEPTION')");
            });
        builder.HasKey(entity => entity.IvrCallJobId);
        builder.HasOne<ConfirmationTaskEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.TaskId)
            .HasPrincipalKey(entity => entity.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.Status, entity.ExpiresAt });
        builder.HasIndex(entity => entity.T0At);
        builder.HasIndex(entity => new { entity.ProgramType, entity.Status });
        builder.HasIndex(entity => new { entity.OfficialOrderId, entity.Status });
        builder.HasIndex(entity => entity.OrderVersionSnapshot);
        builder.HasIndex(entity => entity.Eligible);
        builder.HasIndex(entity => entity.EligibilityDecision);
        builder.HasIndex(entity => entity.QueueStatus);
        builder.HasIndex(entity => entity.CapacityIncidentId);
        builder.HasIndex(entity => entity.CreatedAt);
        builder.HasIndex(entity => entity.ClosedAt);
    }

    private static void ConfigureAttempt(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<CallAttemptEntity>();
        builder.ToTable(
            "ivr_call_attempts",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_call_attempts_number_snapshot",
                    "attempt_number >= 1 AND attempt_number <= max_attempts_snapshot "
                    + "AND max_attempts_snapshot BETWEEN 1 AND 10");
                table.HasCheckConstraint(
                    "ck_ivr_call_attempts_technical_not_counted",
                    "technical_exception_type IS NULL OR is_counted_customer_attempt IS FALSE");

                // W-0118. The sibling of ck_ivr_call_results_non_customer_not_counted, on the
                // table the scheduler reads to decide how many attempts the customer has left.
                //
                // The constraint above it is not made redundant by this one and does not make this
                // one redundant either: that one keys off technical_exception_type, so it still
                // catches a technically failed attempt whose result_status has not been written
                // yet, while this one covers capacity, operational and policy — which have no
                // exception type and were therefore uncovered.
                //
                // TryClaimDueDispatchAsync counts rows here to decide whether another call is
                // owed. A non-customer result counted on this table does not merely misreport;
                // it silently spends one of the two attempts the policy promised the customer.
                table.HasCheckConstraint(
                    "ck_ivr_call_attempts_counted_matches_type",
                    "result_status IS NULL"
                    + " OR (is_counted_customer_attempt IS TRUE AND result_status IN ("
                    + "'IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT',"
                    + "'IVR_NO_ANSWER_FINAL','IVR_WRONG_INPUT'))"
                    + " OR (is_counted_customer_attempt IS FALSE AND result_status IN ("
                    + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                    + "'IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION'))");
                table.HasCheckConstraint(
                    "ck_ivr_call_attempts_retry_nonnegative",
                    "technical_retry_count >= 0");
                table.HasCheckConstraint(
                    "ck_ivr_call_attempts_status",
                    "status IN ('LEASED_PENDING_DISPATCH','DIALING','ACTIVE_CALL',"
                    + "'PROVIDER_EVENT_PENDING_NORMALIZATION','NORMALIZED_ATTEMPT_COMPLETE',"
                    + "'NORMALIZED_FINAL','NORMALIZED_TECHNICAL_RETRY',"
                    + "'NORMALIZED_REVIEW_REQUIRED','TECHNICAL_RETRY_QUEUED',"
                    + "'RECOVERY_REQUIRED')");
                table.HasCheckConstraint(
                    "ck_ivr_call_attempts_result_status",
                    "result_status IS NULL OR result_status IN ('IVR_CONFIRMED',"
                    + "'IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL',"
                    + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                    + "'IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION')");

                // W-0111. All three termination columns move together. A row carrying a
                // timestamp with no actor would be an operator action nobody can be asked
                // about, which is the one thing an audited cut must never become.
                table.HasCheckConstraint(
                    "ck_ivr_call_attempts_termination_complete",
                    "(termination_requested_at IS NULL"
                    + " AND termination_requested_by IS NULL"
                    + " AND termination_reason IS NULL)"
                    + " OR (termination_requested_at IS NOT NULL"
                    + " AND termination_requested_by IS NOT NULL"
                    + " AND termination_reason IS NOT NULL)");

                // W-0113. Either the whole voice record is present or none of it is. A region
                // with no voice id is a claim about what a customer heard that cannot be traced
                // to a configured voice, which is exactly the kind of half-record this column
                // exists to replace.
                table.HasCheckConstraint(
                    "ck_ivr_call_attempts_voice_complete",
                    "(voice_id IS NULL"
                    + " AND voice_region IS NULL"
                    + " AND voice_region_resolved IS NULL)"
                    + " OR (voice_id IS NOT NULL"
                    + " AND voice_region IS NOT NULL"
                    + " AND voice_region_resolved IS NOT NULL)");

                // The three regions are a closed set. Enforced here as well as in code so a
                // direct write cannot introduce a fourth region that no voice map knows about.
                table.HasCheckConstraint(
                    "ck_ivr_call_attempts_voice_region",
                    "voice_region IS NULL"
                    + " OR voice_region IN ('North', 'Central', 'South')");
            });
        builder.HasKey(entity => entity.IvrCallAttemptId);
        builder.HasOne<CallJobEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.IvrCallJobId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.IvrCallJobId, entity.AttemptNumber })
            .IsUnique()
            .HasFilter("is_counted_customer_attempt IS TRUE");
        builder.HasIndex(entity => entity.TaskId);
        builder.HasIndex(entity => entity.ScheduledAt);
        builder.HasIndex(entity => entity.ScheduledWindowExpiresAt);
        builder.HasIndex(entity => entity.Status);
        builder.HasIndex(entity => entity.ResultStatus);
        builder.HasIndex(entity => entity.Disposition);
        builder.HasIndex(entity => entity.IsCountedCustomerAttempt);
        builder.HasIndex(entity => entity.TechnicalExceptionType);
        builder.HasIndex(entity => entity.SimChannelId);
        builder.HasIndex(entity => entity.ProviderCallId);
        builder.HasIndex(entity => entity.RawCallEventId);

        // Partial index: the dispatch loop asks "was this one asked to stop", and all but a
        // handful of rows never carry a request at all.
        builder.HasIndex(entity => entity.TerminationRequestedAt)
            .HasFilter("termination_requested_at IS NOT NULL");
        builder.Property(entity => entity.TerminationRequestedBy).HasMaxLength(120);
        builder.Property(entity => entity.TerminationReason).HasMaxLength(500);
        builder.Property(entity => entity.VoiceId).HasMaxLength(120);
        builder.Property(entity => entity.VoiceRegion).HasMaxLength(16);

        // Indexed because the question worth asking of this column is a count: how many attempts
        // went out in each voice, and how many fell back. Without it that is a table scan over
        // every attempt ever made.
        builder.HasIndex(entity => entity.VoiceRegion)
            .HasFilter("voice_region IS NOT NULL");
    }

    private static void ConfigureTaskIntakeOutbox(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<TaskIntakeOutboxEntity>();
        builder.ToTable(
            "ivr_task_intake_outbox",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_task_intake_outbox_hash",
                    "payload_sha256 ~ '^[A-F0-9]{64}$'");
                table.HasCheckConstraint(
                    "ck_ivr_task_intake_outbox_status",
                    "status IN ('HELD_MOCK','READY_FOR_ELIGIBILITY','PUBLISHED')");
            });
        builder.HasKey(entity => entity.OutboxId);
        builder.HasOne<ConfirmationTaskEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.TaskId)
            .HasPrincipalKey(entity => entity.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CallJobEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.IvrCallJobId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.TaskId).IsUnique();
        builder.HasIndex(entity => entity.IvrCallJobId).IsUnique();
        builder.HasIndex(entity => new { entity.Status, entity.CreatedAt });
        builder.HasIndex(entity => entity.CorrelationId);
        builder.HasIndex(entity => entity.PublishedAt);
    }

    private static void ConfigureRawEvent(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<RawCallEventEntity>();
        builder.ToTable(
            "ivr_raw_call_events",
            table => table.HasCheckConstraint(
                "ck_ivr_raw_call_events_recording_off",
                "recording_ref IS NULL"));
        builder.HasKey(entity => entity.RawEventId);
        builder.HasOne<CallAttemptEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.IvrCallAttemptId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.IvrCallAttemptId);
        builder.HasIndex(entity => entity.IvrCallJobId);
        builder.HasIndex(entity => entity.TechnicalErrorCode);
        builder.HasIndex(entity => entity.ReceivedAt);
    }

    private static void ConfigureResult(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<CallResultEntity>();
        builder.ToTable(
            "ivr_call_results",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_call_results_signal_only",
                    "input_signal_only IS TRUE AND no_direct_order_update IS TRUE "
                    + "AND no_payment_or_revenue_effect IS TRUE");
                table.HasCheckConstraint(
                    "ck_ivr_call_results_result_type",
                    "result_type IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED',"
                    + "'IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL',"
                    + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                    + "'IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION')");
                table.HasCheckConstraint(
                    "ck_ivr_call_results_recommended_core_action",
                    "recommended_core_action IN ('REVALIDATE_AND_CONFIRM_ORDER',"
                    + "'REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST',"
                    + "'NO_STATE_CHANGE_WAIT_FOR_TIMEOUT','REVALIDATE_AND_EXPIRE_CONFIRMATION',"
                    + "'REVALIDATE_AND_HOLD_ADMIN_REVIEW','IGNORE_STALE_CALLBACK',"
                    + "'BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT')");
                table.HasCheckConstraint(
                    "ck_ivr_call_results_final_matches_type",
                    "final_result_status = result_type");

                table.HasCheckConstraint(
                    "ck_ivr_call_results_counted_matches_type",
                    "(is_counted_customer_attempt IS TRUE AND result_type IN ("
                    + "'IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT',"
                    + "'IVR_NO_ANSWER_FINAL','IVR_WRONG_INPUT'))"
                    + " OR (is_counted_customer_attempt IS FALSE AND result_type IN ("
                    + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                    + "'IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION'))");
                table.HasCheckConstraint(
                    "ck_ivr_call_results_finality_matches_type",
                    "(is_final_for_ivr IS TRUE AND result_type IN ("
                    + "'IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_FINAL',"
                    + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                    + "'IVR_CAPACITY_EXCEPTION'))"
                    + " OR (is_final_for_ivr IS FALSE AND result_type IN ("
                    + "'IVR_NO_ANSWER_ATTEMPT','IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION'))");
                table.HasCheckConstraint(
                    "ck_ivr_call_results_action_matches_type",
                    "(result_type = 'IVR_CONFIRMED' AND recommended_core_action = "
                    + "'REVALIDATE_AND_CONFIRM_ORDER')"
                    + " OR (result_type = 'IVR_CUSTOMER_CANCELLED' AND recommended_core_action = "
                    + "'REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST')"
                    + " OR (result_type IN ('IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL',"
                    + "'IVR_WRONG_INPUT') AND recommended_core_action = "
                    + "'NO_STATE_CHANGE_WAIT_FOR_TIMEOUT')"
                    + " OR (result_type = 'IVR_CONFIRMATION_WINDOW_EXPIRED' AND "
                    + "recommended_core_action IN ('REVALIDATE_AND_EXPIRE_CONFIRMATION',"
                    + "'REVALIDATE_AND_HOLD_ADMIN_REVIEW'))"
                    + " OR (result_type IN ('IVR_INVALID_PHONE_FINAL','IVR_TECHNICAL_EXCEPTION',"
                    + "'IVR_CAPACITY_EXCEPTION') AND recommended_core_action = "
                    + "'REVALIDATE_AND_HOLD_ADMIN_REVIEW')");
            });
        builder.HasKey(entity => entity.IvrCallResultId);
        builder.HasOne<CallJobEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.IvrCallJobId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.TaskId);
        builder.HasIndex(entity => entity.OfficialOrderId);
        builder.HasIndex(entity => entity.OrderVersionSnapshot);
        builder.HasIndex(entity => entity.OrderVersionSeenByIvr);
        builder.HasIndex(entity => entity.FinalResultStatus);
        builder.HasIndex(entity => entity.ResultType);
        builder.HasIndex(entity => entity.IsCountedCustomerAttempt);
        builder.HasIndex(entity => entity.IsFinalForIvr);
        builder.HasIndex(entity => entity.HumanReviewRequired);
        builder.HasIndex(entity => entity.CreatedAt);
    }

    private static void ConfigureCallback(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<ResultCallbackEntity>();
        builder.ToTable(
            "ivr_result_callbacks",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_result_callbacks_revalidation",
                    "requires_core_revalidation IS TRUE");
                table.HasCheckConstraint(
                    "ck_ivr_result_callbacks_retry_nonnegative",
                    "retry_count >= 0");
                table.HasCheckConstraint(
                    "ck_ivr_result_callbacks_hash",
                    "payload_sha256 ~ '^[A-F0-9]{64}$'");
                table.HasCheckConstraint(
                    "ck_ivr_result_callbacks_result_status",
                    "result_status IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED',"
                    + "'IVR_NO_ANSWER_FINAL',"
                    + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                    + "'IVR_CAPACITY_EXCEPTION')");
                table.HasCheckConstraint(
                    "ck_ivr_result_callbacks_result_state",
                    "result_state IN ('PENDING_CORE_REVALIDATION')");
                table.HasCheckConstraint(
                    "ck_ivr_result_callbacks_delivery_status",
                    "delivery_status IN ('READY','SENDING','RETRY_PENDING','RETRY_EXHAUSTED',"
                    + "'DELIVERED_ACCEPTED','DELIVERED_BLOCKED','DELIVERED_REVIEW',"
                    + "'REJECTED_STALE','IDEMPOTENCY_CONFLICT','INVALID_DEAD_LETTER',"
                    + "'AUTH_REJECTED')");
            });
        builder.HasKey(entity => entity.CallbackId);
        builder.HasOne<CallResultEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.IvrCallResultId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.IdempotencyKey).IsUnique();
        builder.HasIndex(entity => new { entity.ResultStatus, entity.ResultState });
        builder.HasIndex(entity => entity.TaskId);
        builder.HasIndex(entity => entity.OfficialOrderId);
        builder.HasIndex(entity => new { entity.DeliveryStatus, entity.NextRetryAt });
        builder.HasIndex(entity => entity.CoreHttpStatus);
        builder.HasIndex(entity => entity.CoreResponseCode);
        builder.HasIndex(entity => entity.SentAt);
        builder.HasIndex(entity => entity.AcknowledgedAt);
        builder.HasIndex(entity => entity.LeaseExpiresAt);
    }

    private static void ConfigureChannel(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<SimChannelEntity>();
        builder.ToTable(
            "ivr_sim_channels",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_sim_channels_lease",
                    "active_call_job_id IS NULL OR lease_token IS NOT NULL");
                table.HasCheckConstraint(
                    "ck_ivr_sim_channels_fencing",
                    "lease_fencing_generation >= 0");
                table.HasCheckConstraint(
                    "ck_ivr_sim_channels_mode",
                    "execution_mode IN ('MOCK','LAB_REAL_SIM','PRODUCTION_REAL')");
                table.HasCheckConstraint(
                    "ck_ivr_sim_channels_status",
                    "status IN ('IDLE','RESERVED','LEASED','DIALING','ACTIVE_CALL','DISABLED',"
                    + "'QUARANTINED','HEALTH_FAILED')");
            });
        builder.HasKey(entity => entity.SimChannelId);
        builder.HasIndex(entity => entity.LeaseToken).IsUnique()
            .HasFilter("lease_token IS NOT NULL");
        builder.HasIndex(entity => new { entity.Status, entity.Enabled, entity.CooldownUntil });
        builder.HasIndex(entity => entity.LeaseExpiresAt);
        builder.HasIndex(entity => entity.QuarantineUntil);
    }

    private static void ConfigureOperations(ModelBuilder modelBuilder)
    {
        var capacity = modelBuilder.Entity<CapacityIncidentEntity>();
        capacity.ToTable(
            "ivr_capacity_incidents",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_capacity_incidents_status",
                    "status IN ('OPEN','RESOLVED')");
                table.HasCheckConstraint(
                    "ck_ivr_capacity_incidents_scope",
                    "scope IN ('ADMIN_QUEUE_PAUSE','ELIGIBILITY_DEADLINE',"
                    + "'SCHEDULER_DEADLINE')");
            });
        capacity.HasKey(entity => entity.CapacityIncidentId);
        capacity.HasIndex(entity => new { entity.Status, entity.OpenedAt });

        var technical = modelBuilder.Entity<TechnicalExceptionEntity>();
        technical.ToTable(
            "ivr_technical_exceptions",
            table => table.HasCheckConstraint(
                "ck_ivr_technical_exceptions_not_counted",
                "customer_attempt_counted IS FALSE AND technical_retry_count >= 0"));
        technical.HasKey(entity => entity.TechnicalExceptionId);
        technical.HasOne<CallAttemptEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.IvrCallAttemptId)
            .OnDelete(DeleteBehavior.Restrict);
        technical.HasIndex(entity => entity.CorrelationId);
        technical.HasIndex(entity => entity.CreatedAt);

        var admin = modelBuilder.Entity<AdminActionEntity>();
        admin.ToTable(
            "ivr_admin_actions",
            table => table.HasCheckConstraint(
                "ck_ivr_admin_actions_no_bypass",
                "no_policy_bypass IS TRUE"));
        admin.HasKey(entity => entity.AdminActionId);
        admin.HasIndex(entity => entity.CorrelationId);
        admin.HasIndex(entity => entity.CreatedAt);

        var evidenceLink = modelBuilder.Entity<EvidenceLinkEntity>();
        evidenceLink.ToTable("ivr_evidence_links");
        evidenceLink.HasKey(entity => entity.Id);
        evidenceLink.Property(entity => entity.Id).UseIdentityByDefaultColumn();
        evidenceLink.HasIndex(entity => new { entity.OwnerTable, entity.OwnerId });
        evidenceLink.HasIndex(entity => entity.EvidenceRef);
        evidenceLink.HasIndex(entity => entity.CreatedAt);
        evidenceLink.HasIndex(entity => entity.AcceptedAt);
    }

    private static void ConfigureFoundation(ModelBuilder modelBuilder)
    {
        var idempotency = modelBuilder.Entity<IdempotencyKeyEntity>();
        idempotency.ToTable("ivr_idempotency_keys");
        idempotency.HasKey(entity => new { entity.Scope, entity.Key });
        idempotency.HasIndex(entity => entity.CreatedAt);
        idempotency.HasIndex(entity => entity.ExpiresAt);

        var audit = modelBuilder.Entity<AuditLogEntity>();
        audit.ToTable("ivr_audit_log");
        audit.HasKey(entity => entity.AuditId);
        audit.HasIndex(entity => entity.CorrelationId);
        audit.HasIndex(entity => entity.CreatedAt);
        audit.HasIndex(entity => new { entity.TargetType, entity.TargetId });

        var evidence = modelBuilder.Entity<EvidenceEntity>();
        evidence.ToTable("ivr_evidence");
        evidence.HasKey(entity => entity.EvidenceRef);
        evidence.HasIndex(entity => entity.CorrelationId);
        evidence.HasIndex(entity => entity.WorkId);
        evidence.HasIndex(entity => entity.CreatedAt);
        evidence.HasIndex(entity => entity.AcceptedAt);

        var review = modelBuilder.Entity<ReviewItemEntity>();
        review.ToTable(
            "ivr_review_items",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_review_items_source_type",
                    "source_type IN ('IVR_CALL_RESULT','IVR_RESULT_CALLBACK',"
                    + "'ELIGIBILITY_DECISION','IVR_OPTOUT_PROPOSAL')");
                table.HasCheckConstraint(
                    "ck_ivr_review_items_status",
                    "status IN ('OPEN','RESOLVED','PENDING_CRM','ACCEPTED_BY_CRM')");
            });
        review.HasKey(entity => entity.ReviewItemId);
        review.HasIndex(entity => new { entity.Status, entity.CreatedAt });
        review.HasIndex(entity => entity.CorrelationId);
        review.HasIndex(entity => new { entity.SourceType, entity.SourceId });
    }

    private static void ConfigureRetention(ModelBuilder modelBuilder)
    {
        var checkpoint = modelBuilder.Entity<RetentionCheckpointEntity>();
        checkpoint.ToTable("ivr_retention_checkpoints");
        checkpoint.HasKey(entity => new { entity.DataClass, entity.Segment });
        checkpoint.HasIndex(entity => entity.RunId);
        checkpoint.HasIndex(entity => new { entity.Status, entity.UpdatedAt });
        checkpoint.HasIndex(entity => entity.FirstNotConfiguredAt);
    }

    private static void ConfigureScripts(ModelBuilder modelBuilder)
    {
        var version = modelBuilder.Entity<ScriptVersionEntity>();
        version.ToTable(
            "ivr_script_versions",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ivr_script_versions_status",
                    "status IN ('DRAFT','IN_REVIEW','APPROVED','RETIRED')");
                table.HasCheckConstraint(
                    "ck_ivr_script_versions_hash",
                    "template_hash ~ '^[a-f0-9]{64}$'");
            });
        version.HasKey(entity => entity.Id);
        version.HasIndex(entity => new { entity.TemplateId, entity.Version }).IsUnique();
        version.HasIndex(entity => entity.Status);
        version.HasIndex(entity => entity.CreatedAt);

        var approval = modelBuilder.Entity<ScriptApprovalEntity>();
        approval.ToTable(
            "ivr_script_approvals",
            table => table.HasCheckConstraint(
                "ck_ivr_script_approvals_type",
                "approval_type IN ('MOCK_TEST','LAB','CONTENT','PRIVACY_LEGAL')"));
        approval.HasKey(entity => entity.Id);
        approval.HasOne(entity => entity.ScriptVersion)
            .WithMany(entity => entity.Approvals)
            .HasForeignKey(entity => entity.ScriptVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        approval.HasIndex(entity => new { entity.ScriptVersionId, entity.ApprovalType }).IsUnique();
        approval.HasIndex(entity => entity.ApprovedAt);
    }

    private static void ApplyStorageConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType == typeof(FeatureFlagEntity))
            {
                continue;
            }

            if (typeof(RetainedEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(RetainedEntity.RetainUntil));
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(RetainedEntity.LegalHoldUntil));
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(RetainedEntity.AnonymizedAt));
            }

            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
                if (property.ClrType == typeof(string)
                    && property.Name.EndsWith("Json", StringComparison.Ordinal)
                    && !ByteExactJsonColumns.Contains(
                        (entityType.ClrType.Name, property.Name)))
                {
                    property.SetColumnType("jsonb");
                }
            }
        }
    }

    /// <summary>
    /// Columns that keep <c>text</c> even though the name ends in <c>Json</c>.
    /// <para>
    /// <c>jsonb</c> stores the MEANING of a document, not its bytes: PostgreSQL reorders keys,
    /// drops insignificant whitespace and collapses duplicates. That is exactly what makes it a
    /// good default -- and exactly what makes it wrong for a value whose bytes are the subject of
    /// a hash. The callback payload is sealed with <c>payload_sha256</c> at enqueue and verified
    /// again before the HTTP send, so the text that comes back has to be the text that went in.
    /// </para>
    /// <para>
    /// It was not. Every callback written through Postgres came back re-serialized, failed
    /// <c>CallbackPayloadIntegrity</c>, and dead-lettered as <c>CALLBACK_PAYLOAD_INVALID</c>
    /// without one request ever leaving the process. The unit tests hand the transport a message
    /// they built in memory, so the round trip -- the only place the defect lives -- was never on
    /// the path. <c>IT-IMG-E2E-05</c> and <c>IT-CB-ROUNDTRIP-11</c> are what close it.
    /// </para>
    /// </summary>
    private static readonly HashSet<(string Entity, string Property)> ByteExactJsonColumns =
    [
        (nameof(ResultCallbackEntity), nameof(ResultCallbackEntity.PayloadJson)),
    ];

    private static string ToSnakeCase(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length + 8);
        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            if (char.IsUpper(character) && index > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }
}
