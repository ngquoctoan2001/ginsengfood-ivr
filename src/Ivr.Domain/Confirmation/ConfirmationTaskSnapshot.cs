namespace Ivr.Domain.Confirmation;

public sealed class ConfirmationTaskSnapshot
{
    private ConfirmationTaskSnapshot(
        TaskId taskId,
        OrderId orderId,
        OrderVersion orderVersion,
        string orderState,
        ProgramPayment programPayment,
        ConfirmationWindow confirmationWindow,
        AttemptPolicySnapshot attemptPolicy,
        DialTokenReference dialToken,
        PrivacySafeOrderSummary speechSummary,
        EvidenceReference evidenceReference,
        CorrelationId? correlationId)
    {
        TaskId = taskId;
        OrderId = orderId;
        OrderVersion = orderVersion;
        OrderState = orderState;
        ProgramPayment = programPayment;
        ConfirmationWindow = confirmationWindow;
        AttemptPolicy = attemptPolicy;
        DialToken = dialToken;
        SpeechSummary = speechSummary;
        EvidenceReference = evidenceReference;
        CorrelationId = correlationId;
    }

    public TaskId TaskId { get; }
    public OrderId OrderId { get; }
    public OrderVersion OrderVersion { get; }
    public string OrderState { get; }
    public ProgramPayment ProgramPayment { get; }
    public ConfirmationWindow ConfirmationWindow { get; }
    public AttemptPolicySnapshot AttemptPolicy { get; }
    public DialTokenReference DialToken { get; }
    public PrivacySafeOrderSummary SpeechSummary { get; }
    public EvidenceReference EvidenceReference { get; }
    public CorrelationId? CorrelationId { get; }

    public static ConfirmationTaskSnapshot Create(
        TaskId taskId,
        OrderId orderId,
        OrderVersion orderVersion,
        string orderState,
        ProgramPayment programPayment,
        ConfirmationWindow confirmationWindow,
        AttemptPolicySnapshot attemptPolicy,
        DialTokenReference dialToken,
        PrivacySafeOrderSummary speechSummary,
        EvidenceReference evidenceReference,
        CorrelationId? correlationId,
        ExecutionMode executionMode)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        ArgumentNullException.ThrowIfNull(orderId);
        ArgumentNullException.ThrowIfNull(orderVersion);
        ArgumentNullException.ThrowIfNull(programPayment);
        ArgumentNullException.ThrowIfNull(confirmationWindow);
        ArgumentNullException.ThrowIfNull(attemptPolicy);
        ArgumentNullException.ThrowIfNull(dialToken);
        ArgumentNullException.ThrowIfNull(speechSummary);
        ArgumentNullException.ThrowIfNull(evidenceReference);
        string safeState = PrivacySafeOrderSummary.EnsureSafeBounded(orderState, 120, nameof(orderState));
        attemptPolicy.EnsureEnvironmentAllowed(executionMode);
        if (attemptPolicy.Program != programPayment.Program)
        {
            throw new InvalidOperationException("Task program does not match the attempt-policy snapshot.");
        }

        if (attemptPolicy.MaxCustomerAttempts != attemptPolicy.AttemptOffsets.Count)
        {
            throw new InvalidOperationException("Attempt-policy snapshot is inconsistent.");
        }

        if (confirmationWindow.Duration != attemptPolicy.ConfirmationWindowDuration)
        {
            throw new InvalidOperationException("Task window does not match the versioned attempt policy.");
        }

        if (dialToken.ExpiresAt < confirmationWindow.ExpiresAt)
        {
            throw new InvalidOperationException("Dial token expires before the confirmation window.");
        }

        return new ConfirmationTaskSnapshot(
            taskId,
            orderId,
            orderVersion,
            safeState,
            programPayment,
            confirmationWindow,
            attemptPolicy,
            dialToken,
            speechSummary,
            evidenceReference,
            correlationId);
    }
}
