using Ivr.Api.Auth;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests.Auth;

/// <summary>W-0128. Capability separation and bounded rotation for the admin service tokens.</summary>
public sealed class AdminCredentialSourceTests
{
    private const string Read = "current-admin-read-token-not-a-real-secret";
    private const string ReadPrevious = "previous-admin-read-token-not-a-real-secret";
    private const string Write = "current-admin-write-token-not-a-real-secret";
    private const string Danger = "current-admin-danger-token-not-a-real-secret";
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "SEC-ADMIN-ROT-01")]
    public void CurrentTokensResolveOnlyToTheirConfiguredTier()
    {
        AdminCredentialSource source = Build(new AdminAccessOptions
        {
            ReadToken = Read,
            WriteToken = Write,
            DangerToken = Danger,
        });

        Assert.Equal(AdminScope.Read, source.Match(Read));
        Assert.Equal(AdminScope.Write, source.Match(Write));
        Assert.Equal(AdminScope.Danger, source.Match(Danger));
        Assert.Null(source.Match("unknown-admin-token-not-a-real-secret"));
    }

    [Fact]
    [Trait("TestId", "SEC-ADMIN-ROT-02")]
    public void PreviousTokenStopsMatchingAtItsAbsoluteRetirementInstant()
    {
        var clock = new MutableClock(Now);
        AdminCredentialSource source = Build(new AdminAccessOptions
        {
            ReadToken = Read,
            ReadTokenPrevious = ReadPrevious,
            ReadTokenPreviousRetiresAt = Now.AddMinutes(15),
        }, clock);

        Assert.Equal(AdminScope.Read, source.Match(ReadPrevious));
        clock.Advance(TimeSpan.FromMinutes(15));
        Assert.Null(source.Match(ReadPrevious));
        Assert.Equal(AdminScope.Read, source.Match(Read));
    }

    [Fact]
    [Trait("TestId", "SEC-ADMIN-ROT-03")]
    public void DuplicateAcrossTiersIsRefusedInsteadOfWideningCapability()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            Build(new AdminAccessOptions
            {
                ReadToken = Read,
                DangerToken = Read,
            }));

        Assert.Contains("distinct", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("TestId", "SEC-ADMIN-ROT-04")]
    public void PreviousTokenWithoutRetirementInstantIsRefused()
    {
        Assert.Throws<InvalidOperationException>(() => Build(new AdminAccessOptions
        {
            ReadToken = Read,
            ReadTokenPrevious = ReadPrevious,
        }));
    }

    [Fact]
    [Trait("TestId", "SEC-ADMIN-ROT-05")]
    public void UnconfiguredSourceAuthenticatesNobody()
    {
        AdminCredentialSource source = Build(new AdminAccessOptions());

        Assert.Null(source.Match(string.Empty));
        Assert.Null(source.Match(Read));
    }

    private static AdminCredentialSource Build(
        AdminAccessOptions options,
        TimeProvider? timeProvider = null) =>
        new(Options.Create(options), timeProvider ?? new MutableClock(Now));

    private sealed class MutableClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan duration) => now = now.Add(duration);
    }
}
