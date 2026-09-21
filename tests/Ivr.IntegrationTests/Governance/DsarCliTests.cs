using Ivr.Dsar;
using Ivr.Infrastructure.Governance;
using Npgsql;

namespace Ivr.IntegrationTests.Governance;

public sealed class DsarCliTests
{
    [Theory]
    [InlineData("", GssEncryptionMode.Disable)]
    [InlineData(";gSs EnCrYpTiOn MoDe=Require", GssEncryptionMode.Require)]
    [InlineData(";GSS Encryption Mode=Require", GssEncryptionMode.Require)]
    [InlineData(";GSS Encryption Mode=Prefer", GssEncryptionMode.Prefer)]
    [InlineData(";GSS Encryption Mode=Disable", GssEncryptionMode.Disable)]
    [Trait("TestId", "COMP-DSAR-18")]
    public void ConnectionPreservesExplicitEncryptionAndTls(string setting, GssEncryptionMode expected)
    {
        var effective = new NpgsqlConnectionStringBuilder(DsarProgram.CreateConnectionString(
            "Host=test-db;Database=ivr_test;Username=ivr;SSL Mode=VerifyFull" + setting));
        Assert.Equal(expected, effective.GssEncryptionMode);
        Assert.Equal(SslMode.VerifyFull, effective.SslMode);
        Assert.Equal("test-db", effective.Host);
        Assert.Equal("ivr_test", effective.Database);
        Assert.Equal("ivr", effective.Username);
        Assert.Equal("Ivr.Dsar", effective.ApplicationName);
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-18")]
    public void UnsupportedProviderKeywordsAreRefusedInsteadOfSilentlyDiscarded()
    {
        Assert.Throws<ArgumentException>(() => DsarProgram.CreateConnectionString("Host=test-db;gssencmode=Require"));
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-14")]
    public void OneOrderDefaultsToPreviewAndExecutionNeedsExplicitConfirmation()
    {
        DsarRequest preview = DsarRequest.Parse(["--order-code", "ORDER-A", "--request-ref", "DSAR-A"]);
        Assert.False(preview.Execute);
        Assert.Equal("ORDER-A", preview.OrderCode);
        DsarRequest execution = DsarRequest.Parse([
            "--order-code", "ORDER-A", "--request-ref", "DSAR-A", "--execute",
            "--confirm-order", "ORDER-A", "--subject-verified"]);
        Assert.True(execution.Execute);
    }

    [Theory]
    [InlineData("--order-code ORDER-A --request-ref DSAR-A --execute")]
    [InlineData("--order-code ORDER-A --request-ref DSAR-A --execute --confirm-order ORDER-B --subject-verified")]
    [InlineData("--order-code ORDER-A --request-ref DSAR-A --execute --confirm-order ORDER-A")]
    [InlineData("--order-code ORDER-A --request-ref DSAR-A --confirm-order ORDER-A")]
    [InlineData("--order-code ORDER-A --request-ref DSAR-A --actor someone-else")]
    [InlineData("--order-code ORDER-A --order-code ORDER-B --request-ref DSAR-A")]
    [InlineData("--order-code * --request-ref DSAR-A")]
    [InlineData("--order-code ORDER-A,ORDER-B --request-ref DSAR-A")]
    [InlineData("--order-code ORDER-A --request-ref")]
    [InlineData("--order-code ORDER-A --request-ref DSAR-A --execute false")]
    [Trait("TestId", "COMP-DSAR-15")]
    public void AmbiguousBulkOrUnconfirmedCommandsAreRefused(string arguments)
    {
        Assert.Throws<ArgumentException>(() => DsarRequest.Parse(arguments.Split(' ')));
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-16")]
    public async Task AnUnassignedOperatorCannotEvenReadHoldings()
    {
        var service = new RecordingService();
        DsarRequest request = DsarRequest.Parse(["--order-code", "ORDER-A", "--request-ref", "DSAR-A"]);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => DsarCommand.ExecuteAsync(
            service, request, new DsarOperatorPolicy(1, "assigned-operator"), "other-operator"));
        Assert.Equal(0, service.Calls);
        Assert.Throws<UnauthorizedAccessException>(() => DsarCommand.Authorize(new DsarOperatorPolicy(0, "operator"), "operator"));
        Assert.Throws<UnauthorizedAccessException>(() => DsarCommand.Authorize(new DsarOperatorPolicy(1, ""), "operator"));
    }

    private sealed class RecordingService : IDsarService
    {
        public int Calls { get; private set; }

        public Task<DsarFindReport> FindAsync(string orderCode, CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("An unauthorized call reached the service.");
        }

        public Task<DsarErasureReport> EraseAsync(string orderCode, string reason, string actorId,
            string correlationId, bool dryRun, CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("An unauthorized call reached the service.");
        }
    }
}
