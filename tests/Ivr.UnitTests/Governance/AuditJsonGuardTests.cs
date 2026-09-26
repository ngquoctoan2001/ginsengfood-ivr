using System.Text.Json;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivr.UnitTests.Governance;

/// <summary>
/// W-0365 / K-55 (remediation plan 2026-09-25). The audit guards read serialized JSON as text, and
/// <c>JsonSerializer</c>'s default encoder escapes <c>+</c> and every accented letter. A +84 number
/// and an accented address marker therefore reached the audit table past a guard that would have
/// refused the same value written out. <see cref="PiiGuard.EnsureSafeJsonText"/> checks the decoded
/// values as well as the text.
/// </summary>
public sealed class AuditJsonGuardTests
{
    public static TheoryData<string> EscapedRestrictedValues => new()
    {
        // A +84 number: '+' is written as +, and the digits after it then follow a letter,
        // which the phone pattern's boundary rejects.
        "+84912345678",
        // Accented address markers: every one of them is written as \u escapes.
        "đường Lê Lợi",
        "số nhà 12",
        "ngõ 5 Tràng Tiền",
        "hẻm 3",
        "tổ 7 phường 2",
    };

    [Theory]
    [MemberData(nameof(EscapedRestrictedValues))]
    [Trait("TestId", "UT-PII-JSON-01")]
    public void AValueTheSerializerEscapesIsRefusedWhereverItSits(string value)
    {
        string asValue = JsonSerializer.Serialize(new Dictionary<string, object?> { ["note"] = value });
        string nested = JsonSerializer.Serialize(new { outer = new { items = new[] { "ok", value } } });
        string asName = JsonSerializer.Serialize(new Dictionary<string, object?> { [value] = "x" });

        // The premise, stated so the test fails if it stops holding: the text alone passes.
        Assert.Contains("\\u", asValue, StringComparison.Ordinal);
        Assert.True(PiiGuard.IsSafeText(asValue));

        Assert.Throws<InvalidOperationException>(() => PiiGuard.EnsureSafeJsonText(asValue));
        Assert.Throws<InvalidOperationException>(() => PiiGuard.EnsureSafeJsonText(nested));
        Assert.Throws<InvalidOperationException>(() => PiiGuard.EnsureSafeJsonText(asName));
    }

    [Fact]
    [Trait("TestId", "UT-PII-JSON-01")]
    public void TheWrittenTextIsStillChecked()
    {
        // A number sent as a JSON number has no string to decode; only the text shows it.
        Assert.Throws<InvalidOperationException>(
            () => PiiGuard.EnsureSafeJsonText("{\"n\":84912345678}"));
        Assert.Throws<InvalidOperationException>(
            () => PiiGuard.EnsureSafeJsonText("{\"n\":\"0912345678\"}"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"decision\":\"ELIGIBLE_FOR_IVR\",\"eligible\":true,\"attempts\":[1,2]}")]
    [Trait("TestId", "UT-PII-JSON-01")]
    public void CleanJsonPasses(string? json)
    {
        PiiGuard.EnsureSafeJsonText(json);
    }

    [Theory]
    // Accented text as such is not refused: only the markers, followed by a space, are.
    [InlineData("Hồng sâm Hàn Quốc 6 năm tuổi")]
    [InlineData("tổng đài")]
    [InlineData("091****678")]
    [Trait("TestId", "UT-PII-JSON-01")]
    public void AccentedTextThatIsNotAnAddressPasses(string value)
    {
        PiiGuard.EnsureSafeJsonText(
            JsonSerializer.Serialize(new Dictionary<string, object?> { ["note"] = value }));
    }

    [Theory]
    [InlineData("+84912345678")]
    [InlineData("số nhà 12")]
    [Trait("TestId", "UT-PII-JSON-02")]
    public async Task BothAuditLoggersRefuseAnEscapedValueBeforeStoringIt(string value)
    {
        var auditEvent = new AuditEvent(
            "worker-k55",
            "DISPATCH_RESOLVE",
            "confirmation-task:TASK-K55",
            null,
            "corr-k55",
            new Dictionary<string, object?> { ["note"] = value });

        var memory = new InMemoryAuditLogger(TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => memory.AppendAsync(auditEvent));
        Assert.Empty(memory.Entries);

        // The Postgres logger refuses before it opens a context. The factory throws a different
        // type, so an event that got that far would fail this assertion instead of passing it.
        var postgres = new PostgresAuditLogger(new NoDatabase(), TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => postgres.AppendAsync(auditEvent));
    }

    private sealed class NoDatabase : IDbContextFactory<IvrDbContext>
    {
        public IvrDbContext CreateDbContext() =>
            throw new NotSupportedException("The audit row reached the database.");
    }
}
