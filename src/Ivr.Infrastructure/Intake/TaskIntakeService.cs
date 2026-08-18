using System.Globalization;
using System.Text.Json;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Ports;
using Ivr.Domain.Policies;
using Ivr.Domain.Privacy;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Contracts;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Security;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Intake;

public sealed class TaskIntakeService(
    ITaskIntakeStore store,
    IAttemptPolicyRegistry policyRegistry,
    IScriptRegistry scriptRegistry,
    IOpaqueValueProtector opaqueValueProtector,
    SpeechSummaryLimits speechLimits,
    TimeProvider timeProvider,
    IOptions<IvrOptions> options) : ITaskIntakeService
{
    private const string MockEvidencePolicyVersion = "mock-evidence-v1";
    private const string MockPrivacyPolicyVersion = "mock-privacy-v1";

    public Task<TaskIntakeOutcome> IntakeAsync(
        TaskIntakeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        return store.ExecuteAsync(
            command,
            token => EvaluateAsync(command, token),
            cancellationToken);
    }

    private async Task<TaskIntakePersistencePlan> EvaluateAsync(
        TaskIntakeCommand command,
        CancellationToken cancellationToken)
    {
        IvrConfirmationTaskV1 source = command.Source;
        DateTimeOffset now = timeProvider.GetUtcNow();

        TaskIntakeOutcome? identityFailure = ValidateIdentityAndWindow(source, command, now);
        if (identityFailure is not null)
        {
            return new TaskIntakePersistencePlan(identityFailure);
        }

        (IvrProgramCode program, PaymentMethod payment) = MapProgramPayment(source);
        try
        {
            _ = ProgramPaymentPolicy.EnsureAllowed(
                program,
                payment,
                source.Ivr_confirmation_required);
        }
        catch (InvalidOperationException)
        {
            return Rejected(
                source,
                TaskIntakeDecisions.RejectedPolicyMismatch,
                "PROGRAM_PAYMENT_MATRIX_REJECTED");
        }

        AttemptPolicySnapshot policy;
        try
        {
            policy = await policyRegistry.ResolveAsync(
                PolicyVersion.Create(source.Attempt_policy_version),
                program,
                command.ExecutionMode,
                cancellationToken).ConfigureAwait(false);
        }
        catch (KeyNotFoundException)
        {
            return HeldPolicy(source, "ATTEMPT_POLICY_NOT_FOUND");
        }
        catch (InvalidOperationException)
        {
            return HeldPolicy(source, "ATTEMPT_POLICY_NOT_APPROVED_FOR_MODE");
        }

        ConfirmationWindow window;
        try
        {
            window = ConfirmationWindow.Create(
                source.Confirmation_window_started_at,
                source.Confirmation_window_expires_at);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Rejected(
                source,
                TaskIntakeDecisions.RejectedPolicyMismatch,
                "CONFIRMATION_WINDOW_INVALID");
        }

        if (!WirePolicyMatches(source, policy, window))
        {
            return Failed(
                source,
                TaskIntakeDecisions.RejectedPolicyMismatch,
                IvrErrorCodes.PolicyMismatch,
                "The task policy snapshot does not match the approved IVR policy.",
                "ATTEMPT_POLICY_SNAPSHOT_MISMATCH");
        }

        if (!ContactIsValid(source, window, now))
        {
            return Failed(
                source,
                TaskIntakeDecisions.RejectedContactInvalid,
                IvrErrorCodes.ContactInvalid,
                "The task contact or dial token is not valid for the confirmation window.",
                "CONTACT_OR_DIAL_TOKEN_INVALID");
        }

        Ivr.Domain.Confirmation.PrivacySafeOrderSummary speech;
        try
        {
            EnsureNoSpelledPhone(source.Privacy_safe_order_summary);
            speech = TargetV1TaskMapper.MapSpeechSummary(
                source.Privacy_safe_order_summary,
                speechLimits);
            ValidateAllowedScriptVariables(source.Allowed_script_variables);
            ValidatePersistedMetadata(source);
        }
        catch (Exception exception) when (
            exception is InvalidOperationException
                or ArgumentException
                or JsonException)
        {
            return Failed(
                source,
                TaskIntakeDecisions.HeldAdminReview,
                IvrErrorCodes.PiiPolicyViolation,
                "The request violates the IVR privacy policy.",
                "PRIVACY_SAFE_SPEECH_REJECTED");
        }

        TaskIntakePersistencePlan? eligibility = ValidateEligibilityAndEvidence(
            source,
            command.ExecutionMode);
        if (eligibility is not null)
        {
            return eligibility;
        }

        (string templateId, string scriptVersion) = ResolveRequestedScript(source);
        ApprovedScript? approvedScript;
        try
        {
            approvedScript = await scriptRegistry.TryGetApproved(
                templateId,
                scriptVersion,
                command.ExecutionMode,
                cancellationToken).ConfigureAwait(false);
        }
        catch (ArgumentException)
        {
            approvedScript = null;
        }

        if (approvedScript is null)
        {
            return Failed(
                source,
                TaskIntakeDecisions.RejectedScriptNotApproved,
                IvrErrorCodes.ScriptNotApproved,
                "The requested IVR script is not approved for this execution mode.",
                "SCRIPT_VERSION_NOT_APPROVED");
        }

        if (command.ExecutionMode == ExecutionMode.ProductionReal
            && !options.Value.RealCustomerCallAllowed)
        {
            return new TaskIntakePersistencePlan(new TaskIntakeOutcome(
                TaskIntakeDecisions.HeldAdminReview,
                null,
                ["REAL_CUSTOMER_CALL_ALLOWED_NO"],
                source.Evidence_ref));
        }

        ConfirmationTaskSnapshot snapshot = TargetV1TaskMapper.CreateDomainSnapshot(
            source,
            command.ExecutionMode,
            policy,
            speech);
        string evidencePolicy = ResolvePolicyVersion(
            source.Evidence_policy_version,
            command.ExecutionMode,
            MockEvidencePolicyVersion);
        string privacyPolicy = ResolvePolicyVersion(
            source.Privacy_policy_version,
            command.ExecutionMode,
            MockPrivacyPolicyVersion);
        string protectedDialToken;
        try
        {
            protectedDialToken = opaqueValueProtector.Protect(
                "ivr-confirmation-task-dial-token",
                source.Dial_token);
        }
        catch (InvalidOperationException)
        {
            return Rejected(
                source,
                TaskIntakeDecisions.BlockedOperational,
                "DIAL_TOKEN_PROTECTION_UNAVAILABLE");
        }

        return Accepted(
            command,
            snapshot,
            source,
            approvedScript,
            protectedDialToken,
            evidencePolicy,
            privacyPolicy,
            now);
    }

    private static TaskIntakeOutcome? ValidateIdentityAndWindow(
        IvrConfirmationTaskV1 source,
        TaskIntakeCommand command,
        DateTimeOffset now)
    {
        try
        {
            _ = TaskId.Create(source.Task_id);
            _ = OrderId.Create(source.Order_id);
            _ = OrderVersion.Create(source.Order_version);
            _ = CorrelationId.Create(command.CorrelationId);
            _ = EvidenceReference.Create(source.Evidence_ref);
            PiiGuard.EnsureSafeText(source.Task_id);
            PiiGuard.EnsureSafeText(source.Order_id);
            PiiGuard.EnsureSafeText(source.Order_code);
            PiiGuard.EnsureSafeText(source.Order_version);
            PiiGuard.EnsureSafeText(source.Order_state);
            PiiGuard.EnsureSafeText(source.Attempt_policy_version);
            PiiGuard.EnsureSafeText(source.Evidence_ref);
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            return FailureOutcome(
                source,
                TaskIntakeDecisions.RejectedNotOfficialOrder,
                IvrErrorCodes.NotOfficialOrder,
                "The task does not identify an Official Order.",
                "OFFICIAL_ORDER_IDENTITY_INVALID");
        }

        if (source.Order_state is "QUOTE" or "CART" or "DRAFT")
        {
            return FailureOutcome(
                source,
                TaskIntakeDecisions.RejectedNotOfficialOrder,
                IvrErrorCodes.NotOfficialOrder,
                "Quote, cart and draft records are not Official Orders.",
                "NOT_OFFICIAL_ORDER");
        }

        if (source.Is_ivr_callable == false
            || source.Confirmation_window_started_at > now
            || source.Confirmation_window_expires_at <= now)
        {
            return FailureOutcome(
                source,
                TaskIntakeDecisions.RejectedStateNotCallable,
                IvrErrorCodes.StateNotCallable,
                "The order is not callable in its current confirmation window.",
                "ORDER_NOT_CALLABLE_OR_WINDOW_EXPIRED");
        }

        if (!string.IsNullOrWhiteSpace(source.Correlation_id)
            && !string.Equals(
                source.Correlation_id,
                command.CorrelationId,
                StringComparison.Ordinal))
        {
            return FailureOutcome(
                source,
                TaskIntakeDecisions.RejectedStateNotCallable,
                IvrErrorCodes.MissingTrace,
                "The body correlation identifier does not match the request header.",
                "CORRELATION_MISMATCH");
        }

        return null;
    }

    private static (IvrProgramCode Program, PaymentMethod Payment) MapProgramPayment(
        IvrConfirmationTaskV1 source)
    {
        IvrProgramCode program = source.Program_code switch
        {
            ProgramCode.GOLDEN_HOUR => IvrProgramCode.GoldenHour,
            ProgramCode.TWENTY_FOUR_SEVEN => IvrProgramCode.TwentyFourSeven,
            _ => throw IvrErrors.PolicyMismatch("Unsupported IVR program."),
        };
        PaymentMethod payment = source.Payment_method_snapshot switch
        {
            IvrConfirmationTaskV1Payment_method_snapshot.ONLINE => PaymentMethod.Online,
            IvrConfirmationTaskV1Payment_method_snapshot.COD => PaymentMethod.CashOnDelivery,
            _ => throw IvrErrors.PolicyMismatch("Unsupported payment method."),
        };
        return (program, payment);
    }

    private static bool WirePolicyMatches(
        IvrConfirmationTaskV1 source,
        AttemptPolicySnapshot policy,
        ConfirmationWindow window)
    {
        int[] offsets = [.. policy.AttemptOffsets.Select(offset =>
            checked((int)offset.TotalSeconds))];
        return source.Max_customer_attempts == policy.MaxCustomerAttempts
            && source.Attempt_offsets_seconds.SequenceEqual(offsets)
            && window.Duration == policy.ConfirmationWindowDuration;
    }

    private static bool ContactIsValid(
        IvrConfirmationTaskV1 source,
        ConfirmationWindow window,
        DateTimeOffset now)
    {
        if (!string.Equals(source.Phone_validation_status, "VALID", StringComparison.Ordinal)
            || !source.Phone_masked.Any(character => character is 'x' or 'X' or '*')
            || source.Dial_token_expires_at < window.ExpiresAt
            || source.Dial_token_expires_at <= now
            || LooksLikeRawPhone(source.Phone_ref)
            || LooksLikeRawPhone(source.Dial_token))
        {
            return false;
        }

        try
        {
            _ = DialTokenReference.Create(source.Dial_token, source.Dial_token_expires_at);
            PiiGuard.EnsureSafeText(source.Phone_ref);
            PiiGuard.EnsureSafeText(source.Phone_masked);
            return true;
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }

    private static bool LooksLikeRawPhone(string value)
    {
        string digits = string.Concat(value.Where(char.IsDigit));
        bool onlyPhoneCharacters = value.All(character =>
            char.IsDigit(character)
            || character is '+' or '-' or '(' or ')' or ' '
            || char.IsWhiteSpace(character));
        return onlyPhoneCharacters
            && digits.Length is >= 10 and <= 12
            && (digits.StartsWith('0') || digits.StartsWith("84", StringComparison.Ordinal));
    }

    private static void EnsureNoSpelledPhone(
        Ivr.Contracts.Generated.IvrServer.V1.PrivacySafeOrderSummary source)
    {
        string[] values =
        [
            source.Customer_display_name,
            source.Order_code_short,
            source.Delivery_area_short,
            source.Program_display_name,
            .. source.Items.Select(item => item.Public_name),
        ];
        foreach (string value in values)
        {
            string normalized = value.ToLowerInvariant();
            if (normalized.Contains("sđt", StringComparison.Ordinal)
                || normalized.Contains("số điện thoại", StringComparison.Ordinal)
                || normalized.Contains("so dien thoai", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Spoken phone data is forbidden.");
            }
        }
    }

    private static void ValidateAllowedScriptVariables(object? variables)
    {
        if (variables is null)
        {
            return;
        }

        JsonElement element = JsonSerializer.SerializeToElement(variables);
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Script variables must be an object.");
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!TargetV1SpeechPolicy.AllowedInputFields.Contains(
                    property.Name,
                    StringComparer.Ordinal))
            {
                throw new InvalidOperationException("A script variable is not approved.");
            }

            PiiGuard.EnsureSafeText(property.Value.GetRawText());
        }
    }

    private static void ValidatePersistedMetadata(IvrConfirmationTaskV1 source)
    {
        string?[] optionalValues =
        [
            source.Order_code_short,
            source.Customer_ref,
            source.Customer_trust_status,
            source.Phone_validation_status,
            source.Call_script_template_id,
            source.Call_script_version,
            source.Evidence_policy_version,
            source.Privacy_policy_version,
        ];
        foreach (string? value in optionalValues)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                PiiGuard.EnsureSafeText(value);
            }
        }

        if (source.Risk_flags is not null)
        {
            PiiGuard.EnsureSafeText(JsonSerializer.Serialize(source.Risk_flags));
        }

        if (source.Sellable_status is not null)
        {
            PiiGuard.EnsureSafeText(JsonSerializer.Serialize(source.Sellable_status));
        }

        PiiGuard.EnsureSafeText(JsonSerializer.Serialize(source.Eligibility_snapshot));
    }

    private static TaskIntakePersistencePlan? ValidateEligibilityAndEvidence(
        IvrConfirmationTaskV1 source,
        ExecutionMode mode)
    {
        JsonElement eligibility = JsonSerializer.SerializeToElement(
            source.Eligibility_snapshot);
        if (eligibility.ValueKind != JsonValueKind.Object)
        {
            return HeldPolicy(source, "ELIGIBILITY_EVIDENCE_MISSING");
        }

        if (source.Call_restriction)
        {
            return Failed(
                source,
                TaskIntakeDecisions.BlockedOperational,
                IvrErrorCodes.OperationalBlocked,
                "The task is restricted from phone calls.",
                "PHONE_CALL_RESTRICTED");
        }

        if (mode != ExecutionMode.Mock
            && (string.IsNullOrWhiteSpace(source.Evidence_policy_version)
                || string.IsNullOrWhiteSpace(source.Privacy_policy_version)))
        {
            return HeldPolicy(source, "NON_MOCK_DEPENDENCY_EVIDENCE_MISSING");
        }

        return null;
    }

    private static (string TemplateId, string Version) ResolveRequestedScript(
        IvrConfirmationTaskV1 source)
    {
        bool templateMissing = string.IsNullOrWhiteSpace(source.Call_script_template_id);
        bool versionMissing = string.IsNullOrWhiteSpace(source.Call_script_version);
        if (templateMissing != versionMissing)
        {
            return ("INVALID_PARTIAL_SCRIPT_REFERENCE", "INVALID");
        }

        return templateMissing
            ? (TargetV1SpeechPolicy.MockTemplateId, TargetV1SpeechPolicy.MockTemplateVersion)
            : (source.Call_script_template_id!, source.Call_script_version!);
    }

    private static string ResolvePolicyVersion(
        string? supplied,
        ExecutionMode mode,
        string mockDefault)
    {
        if (!string.IsNullOrWhiteSpace(supplied))
        {
            PiiGuard.EnsureSafeText(supplied);
            return supplied;
        }

        if (mode == ExecutionMode.Mock)
        {
            return mockDefault;
        }

        throw new InvalidOperationException("A non-MOCK policy version is missing.");
    }

    private static TaskIntakePersistencePlan Accepted(
        TaskIntakeCommand command,
        ConfirmationTaskSnapshot snapshot,
        IvrConfirmationTaskV1 source,
        ApprovedScript approvedScript,
        string protectedDialToken,
        string evidencePolicy,
        string privacyPolicy,
        DateTimeOffset now)
    {
        string callJobId = string.Concat("JOB-", Guid.NewGuid().ToString("N"));
        string decision = command.ExecutionMode == ExecutionMode.Mock
            ? TaskIntakeDecisions.AcceptedDryRunOnly
            : TaskIntakeDecisions.AcceptedCallJobCreated;
        var outcome = new TaskIntakeOutcome(
            decision,
            callJobId,
            [],
            source.Evidence_ref);
        string attemptOffsetsJson = JsonSerializer.Serialize(
            snapshot.AttemptPolicy.AttemptOffsets.Select(offset =>
                checked((int)offset.TotalSeconds)));
        string scheduleJson = JsonSerializer.Serialize(
            snapshot.AttemptPolicy.AttemptOffsets.Select(offset =>
                snapshot.ConfirmationWindow.StartedAt.Add(offset)));
        string summaryJson = JsonSerializer.Serialize(source.Privacy_safe_order_summary);
        string eligibilityJson = JsonSerializer.Serialize(source.Eligibility_snapshot);
        // W-0030 / P4-2 §2.3. Fingerprints exactly the evidence bag IVR stores and later
        // evaluates, so the result callback can be traced to the bytes the decision was made on.
        // Only the digest is ever carried forward — the snapshot body stays in the task row.
        string eligibilitySnapshotHash = DeterministicSnapshotHasher.Compute(eligibilityJson);
        string? riskFlagsJson = source.Risk_flags is null
            ? null
            : JsonSerializer.Serialize(source.Risk_flags);
        string? sellableJson = source.Sellable_status is null
            ? null
            : JsonSerializer.Serialize(source.Sellable_status);
        PiiGuard.EnsureSafeText(summaryJson);
        PiiGuard.EnsureSafeText(eligibilityJson);
        if (riskFlagsJson is not null)
        {
            PiiGuard.EnsureSafeText(riskFlagsJson);
        }

        if (sellableJson is not null)
        {
            PiiGuard.EnsureSafeText(sellableJson);
        }

        var task = new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = snapshot.TaskId.Value,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = command.ScopedIdempotencyKey,
            CorrelationId = command.CorrelationId,
            OfficialOrderId = snapshot.OrderId.Value,
            OrderCode = source.Order_code,
            OrderVersion = snapshot.OrderVersion.Value,
            OrderState = snapshot.OrderState,
            PaymentMethodSnapshot = source.Payment_method_snapshot.ToString(),
            IvrConfirmationRequired = true,
            CustomerId = source.Customer_ref,
            CustomerTrustStatus = source.Customer_trust_status,
            TrustedSkipAllowed = source.Trusted_skip_allowed,
            RiskFlagsJson = riskFlagsJson,
            ProgramType = source.Program_code.ToString(),
            AttemptPolicyVersion = snapshot.AttemptPolicy.Version.Value,
            MaxAttempts = snapshot.AttemptPolicy.MaxCustomerAttempts,
            AttemptOffsetsSecondsJson = attemptOffsetsJson,
            ConfirmationWindowStartedAt = snapshot.ConfirmationWindow.StartedAt,
            ConfirmationWindowExpiresAt = snapshot.ConfirmationWindow.ExpiresAt,
            PhoneRef = source.Phone_ref,
            PhoneMasked = source.Phone_masked,
            PhoneValidationStatus = source.Phone_validation_status,
            DialTokenCiphertext = protectedDialToken,
            DialTokenExpiresAt = snapshot.DialToken.ExpiresAt,
            PrivacySafeOrderSummaryJson = summaryJson,
            CallScriptTemplateId = approvedScript.Version.Key.TemplateId,
            CallScriptVersion = approvedScript.Version.Key.Version,
            EvidencePolicyVersion = evidencePolicy,
            PrivacyPolicyVersion = privacyPolicy,
            EligibilityDecision = null,
            EligibilitySnapshotJson = eligibilityJson,
            EligibilitySnapshotHash = eligibilitySnapshotHash,
            SellableStatusJson = sellableJson,
            SellableCapturedAt = source.Sellable_status is { Count: > 0 }
                ? source.Sellable_status.Max(line => line.Captured_at)
                : null,
            CallRestriction = source.Call_restriction,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = now,
            ExpiresAt = snapshot.ConfirmationWindow.ExpiresAt,
            AcceptedAt = now,
            EvidenceRefsJson = JsonSerializer.Serialize(new[] { source.Evidence_ref }),
        };
        var job = new CallJobEntity
        {
            IvrCallJobId = callJobId,
            TaskId = snapshot.TaskId.Value,
            OfficialOrderId = snapshot.OrderId.Value,
            OrderVersionSnapshot = snapshot.OrderVersion.Value,
            ProgramType = source.Program_code.ToString(),
            AttemptPolicyCode = snapshot.AttemptPolicy.Version.Value,
            Status = command.ExecutionMode == ExecutionMode.Mock ? "DRY_RUN" : "CREATED",
            MaxAttempts = snapshot.AttemptPolicy.MaxCustomerAttempts,
            AttemptOffsetsSecondsJson = attemptOffsetsJson,
            ConfirmationWindowSeconds = checked((int)snapshot.ConfirmationWindow.Duration.TotalSeconds),
            AttemptScheduleJson = scheduleJson,
            T0At = snapshot.ConfirmationWindow.StartedAt,
            ExpiresAt = snapshot.ConfirmationWindow.ExpiresAt,
            Eligible = false,
            EligibilityDecision = EligibilityDecisions.Pending,
            QueueStatus = command.ExecutionMode == ExecutionMode.Mock
                ? "HELD_MOCK"
                : "HELD_ELIGIBILITY",
            ScriptVersion = approvedScript.Version.Key.ToString(),
            PrivacyPolicyVersion = privacyPolicy,
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = now,
            EvidenceRefsJson = JsonSerializer.Serialize(new[] { source.Evidence_ref }),
        };
        var outbox = new TaskIntakeOutboxEntity
        {
            OutboxId = Guid.NewGuid(),
            TaskId = snapshot.TaskId.Value,
            IvrCallJobId = callJobId,
            EventType = command.ExecutionMode == ExecutionMode.Mock
                ? "IVR_TASK_DRY_RUN_RECORDED"
                : "IVR_TASK_READY_FOR_ELIGIBILITY",
            Status = command.ExecutionMode == ExecutionMode.Mock
                ? "HELD_MOCK"
                : "READY_FOR_ELIGIBILITY",
            CorrelationId = command.CorrelationId,
            PayloadSha256 = command.PayloadHash,
            CreatedAt = now,
        };
        return new TaskIntakePersistencePlan(outcome, task, job, outbox);
    }

    private static TaskIntakePersistencePlan Rejected(
        IvrConfirmationTaskV1 source,
        string decision,
        string reason) =>
        new(new TaskIntakeOutcome(
            decision,
            null,
            [reason],
            source.Evidence_ref));

    private static TaskIntakePersistencePlan HeldPolicy(
        IvrConfirmationTaskV1 source,
        string reason) =>
        new(new TaskIntakeOutcome(
            TaskIntakeDecisions.HeldPolicyMissing,
            null,
            [reason],
            source.Evidence_ref));

    private static TaskIntakePersistencePlan Failed(
        IvrConfirmationTaskV1 source,
        string decision,
        string errorCode,
        string message,
        string reason) =>
        new(FailureOutcome(source, decision, errorCode, message, reason));

    private static TaskIntakeOutcome FailureOutcome(
        IvrConfirmationTaskV1 source,
        string decision,
        string errorCode,
        string message,
        string reason) =>
        new(
            decision,
            null,
            [reason],
            source.Evidence_ref,
            errorCode,
            message);
}
