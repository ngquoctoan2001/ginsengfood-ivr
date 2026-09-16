using Ivr.Api.Application;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0307 / B9. What the audit-evidence read refuses, and why each refusal is not merely input
/// validation.
/// </summary>
/// <remarks>
/// Every case here is proved without a database on purpose: each refusal happens before the query
/// is built, so the stubs below throw if the service ever reaches them. That is the assertion —
/// a refused request must not touch the audit table or write an access row for a call that was
/// never served.
/// </remarks>
public sealed class AuditEvidenceReadServiceTests
{
    private const string Actor = "admin-1";
    private const string Correlation = "corr-1";
    private const string GoodReason = "incident review 2026-09-16";

    /// <summary>
    /// The one that is not ordinary validation. A call with no selector would return the newest
    /// rows across every object in the system — a bulk extract of the audit trail wearing the
    /// clothes of a lookup. Defaulting an absent filter to "everything" is the wrong direction for
    /// a trail, so it is refused.
    /// </summary>
    [Theory]
    [Trait("TestId", "IT-AUDITEV-01")]
    [InlineData(null, "TASK-1")]
    [InlineData("", "TASK-1")]
    [InlineData("   ", "TASK-1")]
    [InlineData("confirmation-task", null)]
    [InlineData("confirmation-task", "")]
    [InlineData("confirmation-task", "   ")]
    public async Task ACallWithoutBothSelectorsIsRefusedRatherThanTreatedAsSelectEverything(
        string? targetType,
        string? targetId)
    {
        IvrFailureException failure = await Assert.ThrowsAsync<IvrFailureException>(
            () => Service().GetAsync(
                targetType, targetId, null, GoodReason, Actor, Correlation, CancellationToken.None));

        Assert.Equal(IvrErrors.MalformedRequest("x").ErrorCode, failure.ErrorCode);
    }

    /// <summary>
    /// reason is mandatory because the read is itself audited, and an audit row whose reason is
    /// blank records that someone looked without recording why — which is the part worth keeping.
    /// </summary>
    [Theory]
    [Trait("TestId", "IT-AUDITEV-02")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    public async Task AMissingOrTooShortReasonIsRefused(string? reason)
    {
        await Assert.ThrowsAsync<IvrFailureException>(
            () => Service().GetAsync(
                "confirmation-task", "TASK-1", null, reason, Actor, Correlation,
                CancellationToken.None));
    }

    [Theory]
    [Trait("TestId", "IT-AUDITEV-03")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(AuditEvidenceReadService.MaxLimit + 1)]
    public async Task ALimitOutsideTheAllowedRangeIsRefused(int limit)
    {
        await Assert.ThrowsAsync<IvrFailureException>(
            () => Service().GetAsync(
                "confirmation-task", "TASK-1", limit, GoodReason, Actor, Correlation,
                CancellationToken.None));
    }

    /// <summary>
    /// A target id is an identifier this system issued, never free text a person typed. Contact
    /// detail arriving here means the caller is searching by the wrong thing, and letting it
    /// through would write the phone number into the access audit row — creating the exact
    /// disclosure the endpoint exists to let someone investigate.
    /// </summary>
    [Theory]
    [Trait("TestId", "IT-AUDITEV-04")]
    [InlineData("0912345678")]
    [InlineData("+84912345678")]
    public async Task ContactDetailInASelectorIsRefusedBeforeItReachesTheAuditRow(string targetId)
    {
        IvrFailureException failure = await Assert.ThrowsAsync<IvrFailureException>(
            () => Service().GetAsync(
                "confirmation-task", targetId, null, GoodReason, Actor, Correlation,
                CancellationToken.None));

        // A published API-06 code, not the raw InvalidOperationException PiiGuard throws. That
        // distinction is the whole test: unhandled, the guard's exception reaches the caller as
        // 500 IVR_INTERNAL_ERROR, which producers are allowed to retry -- forever, for a request
        // that can never become valid. Same defect W-0302 fixed on the intake path.
        Assert.Equal(IvrErrors.PiiPolicyViolation().ErrorCode, failure.ErrorCode);
    }

    private static AuditEvidenceReadService Service() =>
        new(new UnreachableDbContextFactory(), new UnreachableAuditLogger());

    private sealed class UnreachableDbContextFactory : IDbContextFactory<IvrDbContext>
    {
        public IvrDbContext CreateDbContext() => throw new InvalidOperationException(
            "A refused audit-evidence request must not open a database connection.");
    }

    private sealed class UnreachableAuditLogger : IAuditLogger
    {
        public Task<AuditLogEntry> AppendAsync(
            AuditEvent auditEvent,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(
                "A refused audit-evidence request must not write an access record.");
    }
}
