using System.Collections.Immutable;
using System.Globalization;

namespace Ivr.Domain.Confirmation;

public enum IvrProgramCode
{
    GoldenHour,
    TwentyFourSeven,
}

public enum PaymentMethod
{
    Online,
    CashOnDelivery,
}

public enum ExecutionMode
{
    Mock,
    LabRealSim,
    ProductionReal,
}

public enum AttemptPolicyApproval
{
    CandidateMockLabOnly,
    OwnerApproved,
}

public sealed record ProgramPayment(
    IvrProgramCode Program,
    PaymentMethod PaymentMethod,
    bool IvrConfirmationRequired);

public static class ProgramPaymentPolicy
{
    public static ProgramPayment EnsureAllowed(
        IvrProgramCode program,
        PaymentMethod paymentMethod,
        bool ivrConfirmationRequired)
    {
        bool pairAllowed =
            (program == IvrProgramCode.GoldenHour && paymentMethod == PaymentMethod.Online)
            || (program == IvrProgramCode.TwentyFourSeven && paymentMethod == PaymentMethod.CashOnDelivery);

        if (!ivrConfirmationRequired || !pairAllowed)
        {
            throw new InvalidOperationException("Program/payment/IVR-required policy rejected the task.");
        }

        return new ProgramPayment(program, paymentMethod, true);
    }
}

public sealed record ConfirmationWindow
{
    private ConfirmationWindow(DateTimeOffset startedAt, DateTimeOffset expiresAt)
    {
        if (expiresAt <= startedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAt), "Confirmation window must end after it starts.");
        }

        StartedAt = startedAt;
        ExpiresAt = expiresAt;
    }

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset ExpiresAt { get; }

    public TimeSpan Duration => ExpiresAt - StartedAt;

    public static ConfirmationWindow Create(DateTimeOffset startedAt, DateTimeOffset expiresAt) =>
        new(startedAt, expiresAt);

    public bool Contains(DateTimeOffset instant) => instant >= StartedAt && instant < ExpiresAt;
}

public sealed class AttemptOffsets : IReadOnlyList<TimeSpan>
{
    private readonly ImmutableArray<TimeSpan> _values;

    private AttemptOffsets(ImmutableArray<TimeSpan> values)
    {
        _values = values;
    }

    public int Count => _values.Length;

    public TimeSpan this[int index] => _values[index];

    public static AttemptOffsets Create(
        IEnumerable<TimeSpan> values,
        int maxCustomerAttempts,
        TimeSpan confirmationWindowDuration)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (maxCustomerAttempts is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCustomerAttempts));
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            confirmationWindowDuration,
            TimeSpan.Zero);

        ImmutableArray<TimeSpan> offsets = [.. values];
        if (offsets.Length != maxCustomerAttempts || offsets[0] != TimeSpan.Zero)
        {
            throw new InvalidOperationException("Attempt offsets must match max attempts and start at zero.");
        }

        for (int index = 0; index < offsets.Length; index++)
        {
            if (offsets[index] < TimeSpan.Zero || offsets[index] >= confirmationWindowDuration)
            {
                throw new InvalidOperationException("Every attempt offset must be inside the confirmation window.");
            }

            if (index > 0 && offsets[index] <= offsets[index - 1])
            {
                throw new InvalidOperationException("Attempt offsets must be strictly increasing.");
            }
        }

        return new AttemptOffsets(offsets);
    }

    public IEnumerator<TimeSpan> GetEnumerator() =>
        ((IEnumerable<TimeSpan>)_values).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class AttemptPolicySnapshot
{
    private AttemptPolicySnapshot(
        PolicyVersion version,
        IvrProgramCode program,
        int maxCustomerAttempts,
        AttemptOffsets attemptOffsets,
        TimeSpan confirmationWindowDuration,
        AttemptPolicyApproval approval)
    {
        Version = version;
        Program = program;
        MaxCustomerAttempts = maxCustomerAttempts;
        AttemptOffsets = attemptOffsets;
        ConfirmationWindowDuration = confirmationWindowDuration;
        Approval = approval;
    }

    public PolicyVersion Version { get; }

    public IvrProgramCode Program { get; }

    public int MaxCustomerAttempts { get; }

    public AttemptOffsets AttemptOffsets { get; }

    public TimeSpan ConfirmationWindowDuration { get; }

    public AttemptPolicyApproval Approval { get; }

    public static AttemptPolicySnapshot Create(
        PolicyVersion version,
        IvrProgramCode program,
        int maxCustomerAttempts,
        IEnumerable<TimeSpan> attemptOffsets,
        TimeSpan confirmationWindowDuration,
        AttemptPolicyApproval approval)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(attemptOffsets);
        if (!Enum.IsDefined(program) || !Enum.IsDefined(approval))
        {
            throw new InvalidOperationException("Attempt policy contains an unknown enum value.");
        }
        if (maxCustomerAttempts is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCustomerAttempts));
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            confirmationWindowDuration,
            TimeSpan.Zero);

        AttemptOffsets offsets = AttemptOffsets.Create(
            attemptOffsets,
            maxCustomerAttempts,
            confirmationWindowDuration);

        return new AttemptPolicySnapshot(
            version,
            program,
            maxCustomerAttempts,
            offsets,
            confirmationWindowDuration,
            approval);
    }

    public void EnsureEnvironmentAllowed(ExecutionMode executionMode)
    {
        if (!Enum.IsDefined(executionMode))
        {
            throw new InvalidOperationException("Unknown execution mode is forbidden.");
        }

        if (Approval == AttemptPolicyApproval.CandidateMockLabOnly
            && executionMode == ExecutionMode.ProductionReal)
        {
            throw new InvalidOperationException("Candidate attempt policy is forbidden in production mode.");
        }
    }

    public string ComputeHash() => DeterministicSnapshotHasher.Compute(
        Version.Value,
        Program.ToString(),
        MaxCustomerAttempts.ToString(CultureInfo.InvariantCulture),
        ConfirmationWindowDuration.TotalSeconds.ToString(CultureInfo.InvariantCulture),
        Approval.ToString(),
        string.Join(",", AttemptOffsets.Select(offset =>
            offset.TotalSeconds.ToString(CultureInfo.InvariantCulture))));
}
