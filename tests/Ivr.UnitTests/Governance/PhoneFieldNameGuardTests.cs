using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivr.UnitTests.Governance;

/// <summary>
/// W-0361 / K-37 (remediation plan 2026-09-25). Since option B (W-0310) the number travels as
/// <c>phone_e164</c>, and carrier and analytics code call it <c>msisdn</c>. The field-name guard
/// knew neither, so both audit loggers accepted a row keyed <c>phone_e164</c>. The value guard would
/// still refuse a number in the value; the key is what tells a reviewer the row was meant to hold
/// one, and the guard exists to refuse that shape before anyone fills it.
/// </summary>
public sealed class PhoneFieldNameGuardTests
{
    [Theory]
    [InlineData("phone_e164")]
    [InlineData("phoneE164")]
    [InlineData("PhoneE164")]
    [InlineData("phone-e164")]
    [InlineData("msisdn")]
    [InlineData("MSISDN")]
    [Trait("TestId", "UT-PII-FIELD-01")]
    public void TheNamesTheNumberTravelsUnderAreRefused(string field)
    {
        Assert.Throws<InvalidOperationException>(() => PiiGuard.EnsureSafeField(field));
    }

    [Theory]
    [InlineData("phone_masked")]
    [InlineData("phone_ref")]
    [InlineData("phone_validation_status")]
    [Trait("TestId", "UT-PII-FIELD-01")]
    public void TheFormsMeantToTravelInsteadStillPass(string field)
    {
        // Exact names, as before. These are what every log, audit row and admin response carries in
        // place of the number, so refusing them would break the surfaces this guard protects.
        PiiGuard.EnsureSafeField(field);
    }

    [Fact]
    [Trait("TestId", "UT-PII-FIELD-02")]
    public async Task BothAuditLoggersRefuseARowKeyedByTheNumbersName()
    {
        var keyedByNumber = new AuditEvent(
            "worker-k37",
            "DISPATCH_RESOLVE",
            "confirmation-task:TASK-K37",
            null,
            "corr-k37",
            new Dictionary<string, object?> { ["phone_e164"] = "present" });

        var memory = new InMemoryAuditLogger(TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => memory.AppendAsync(keyedByNumber));
        Assert.Empty(memory.Entries);

        // The Postgres logger validates before it opens a context. The factory throws a different
        // type, so a row that got past validation would fail this assertion instead of passing it.
        var postgres = new PostgresAuditLogger(new NoDatabase(), TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => postgres.AppendAsync(
            keyedByNumber with
            {
                Data = new Dictionary<string, object?> { ["msisdn"] = "present" },
            }));
    }

    private sealed class NoDatabase : IDbContextFactory<IvrDbContext>
    {
        public IvrDbContext CreateDbContext() =>
            throw new NotSupportedException("The audit row reached the database.");
    }
}
