using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0195 / <c>OD-V1-20</c>. The three runtime gates, read from the approval table the owner's
/// signature is recorded in.
/// <para>
/// These run against real PostgreSQL because the properties under test are database properties:
/// the table refuses to forget an approval, refuses an approver who is also the proposer, and
/// refuses an approval for a flag change that does not say which change. A rule the application
/// checks is a rule a future caller can forget.
/// </para>
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class RuntimeGateApprovalTests(PostgresPersistenceFixture fixture)
{
    /// <summary>
    /// The signature exists as a row. Before <c>OD-V1-20</c> the answer was a hard-coded
    /// <c>false</c>, and nobody could tell whether that meant "refused" or "never wired".
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-01")]
    public async Task RuntimeGateAdministrationIsApprovedBecauseTheSignatureIsARow()
    {
        await fixture.ResetAsync();
        IRuntimeGateAuthorization authorization = fixture.Services
            .GetRequiredService<IRuntimeGateAuthorization>();

        Assert.True(await authorization.IsApprovedAsync());

        string? seeded = await ScalarAsync(
            "SELECT signed_decision_ref FROM ivr_runtime_gate_approvals "
            + "WHERE approval_kind = 'RUNTIME_GATE_ADMIN'");
        Assert.Equal("OD-V1-20@2026-09-05", seeded);
    }

    /// <summary>
    /// Real customer dialling stays refused. No migration seeds a <c>PRODUCTION_CALL</c> approval
    /// and none should: it is the last gate before a real telephone rings.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-02")]
    public async Task TheProductionCallGateIsRefusedBecauseNothingGrantsIt()
    {
        await fixture.ResetAsync();
        IProductionCallGate gate = fixture.Services.GetRequiredService<IProductionCallGate>();

        Assert.False(await gate.IsApprovedAsync());
        Assert.Equal(
            "0",
            await ScalarAsync(
                "SELECT count(*)::text FROM ivr_runtime_gate_approvals "
                + "WHERE approval_kind = 'PRODUCTION_CALL'"));
    }

    /// <summary>Revoking the signature closes the gate again, without deleting the history.</summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-03")]
    public async Task RevokingTheApprovalClosesTheGateAndKeepsTheRow()
    {
        await fixture.ResetAsync();
        IRuntimeGateAuthorization authorization = fixture.Services
            .GetRequiredService<IRuntimeGateAuthorization>();
        Assert.True(await authorization.IsApprovedAsync());

        await ExecuteAsync(
            "UPDATE ivr_runtime_gate_approvals "
            + "SET revoked_at = now(), revoked_reason = 'test revocation' "
            + "WHERE approval_kind = 'RUNTIME_GATE_ADMIN'");

        Assert.False(await authorization.IsApprovedAsync());
        Assert.Equal("1", await ScalarAsync(
            "SELECT count(*)::text FROM ivr_runtime_gate_approvals "
            + "WHERE approval_kind = 'RUNTIME_GATE_ADMIN'"));
    }

    /// <summary>
    /// The database refuses to forget. Delete is refused outright and every granted field is
    /// immutable, so an approval cannot be quietly rewritten into one that authorises more.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-04")]
    public async Task AGrantedApprovalCannotBeDeletedOrRewritten()
    {
        await fixture.ResetAsync();

        PostgresException deleted = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync("DELETE FROM ivr_runtime_gate_approvals"));
        Assert.Contains("append-only", deleted.MessageText, StringComparison.Ordinal);

        PostgresException rewritten = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync(
                "UPDATE ivr_runtime_gate_approvals SET approver_actor_id = 'someone-else'"));
        Assert.Contains("immutable", rewritten.MessageText, StringComparison.Ordinal);
    }

    /// <summary>
    /// Four eyes, enforced by the table. An approval naming the same actor as proposer and
    /// approver is one pair of eyes wearing two hats.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-05")]
    public async Task AnApprovalCannotNameItsOwnProposerAsApprover()
    {
        await fixture.ResetAsync();

        PostgresException failure = await Assert.ThrowsAsync<PostgresException>(
            () => InsertApprovalAsync(
                reference: "self-approved",
                proposer: "operator-1",
                approver: "operator-1",
                fingerprint: new string('a', 64)));
        Assert.Contains("four_eyes", failure.MessageText, StringComparison.Ordinal);
    }

    /// <summary>
    /// An approval for a flag change must carry the fingerprint of that change. Without it the
    /// reference would authorise any change at all, which makes it a password rather than a
    /// decision.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-06")]
    public async Task AFlagChangeApprovalMustNameTheChangeItApproves()
    {
        await fixture.ResetAsync();

        PostgresException failure = await Assert.ThrowsAsync<PostgresException>(
            () => InsertApprovalAsync(
                reference: "unbound",
                proposer: "operator-1",
                approver: "operator-2",
                fingerprint: null));
        Assert.Contains("change_binding", failure.MessageText, StringComparison.Ordinal);
    }

    /// <summary>
    /// The verifier resolves an approval only for the exact change it was granted for, and only
    /// for an approver who is not the proposer.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-07")]
    public async Task TheVerifierAcceptsTheApprovedChangeAndRefusesEveryOtherOne()
    {
        await fixture.ResetAsync();
        FeatureFlagSnapshot before = FeatureFlagSnapshot.SafeDefault(
            FeatureFlagEnvironments.Lab);
        FeatureFlagSnapshot approved = before.Apply(
            new FeatureFlagChangeSet(GlobalDialKillSwitch: false));
        FeatureFlagSnapshot other = before.Apply(
            new FeatureFlagChangeSet(RecordingEnabled: true));

        await InsertApprovalAsync(
            reference: "approval-lab-1",
            proposer: "operator-1",
            approver: "operator-2",
            fingerprint: RuntimeGateFingerprint.Of(before, approved),
            environment: FeatureFlagEnvironments.Lab);

        IFourEyesApprovalVerifier verifier = fixture.Services
            .GetRequiredService<IFourEyesApprovalVerifier>();

        Assert.Equal(
            "operator-2",
            await verifier.VerifyAsync("approval-lab-1", "operator-1", before, approved));

        // A different change carries a different fingerprint, so the same reference does not
        // travel to it.
        Assert.Null(await verifier.VerifyAsync("approval-lab-1", "operator-1", before, other));

        // And the approver cannot use their own approval as the proposer.
        Assert.Null(await verifier.VerifyAsync("approval-lab-1", "operator-2", before, approved));
    }

    /// <summary>
    /// A revoked or expired approval verifies as nothing. Both are the same question asked of
    /// time rather than of intent.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-08")]
    public async Task AnExpiredApprovalDoesNotVerify()
    {
        await fixture.ResetAsync();
        FeatureFlagSnapshot before = FeatureFlagSnapshot.SafeDefault(
            FeatureFlagEnvironments.Lab);
        FeatureFlagSnapshot after = before.Apply(
            new FeatureFlagChangeSet(GlobalDialKillSwitch: false));

        await InsertApprovalAsync(
            reference: "approval-expired",
            proposer: "operator-1",
            approver: "operator-2",
            fingerprint: RuntimeGateFingerprint.Of(before, after),
            environment: FeatureFlagEnvironments.Lab,
            expiresAtSql: "TIMESTAMPTZ '2000-01-01 00:00:00+00'",
            grantedAtSql: "TIMESTAMPTZ '1999-01-01 00:00:00+00'");

        IFourEyesApprovalVerifier verifier = fixture.Services
            .GetRequiredService<IFourEyesApprovalVerifier>();
        Assert.Null(
            await verifier.VerifyAsync("approval-expired", "operator-1", before, after));
    }

    /// <summary>
    /// The fingerprint is stable across allowlist ordering. A set has no order, and two runs that
    /// disagreed about it would produce two fingerprints for one change - which would leave an
    /// approver unable to authorise anything.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-09")]
    public void TheFingerprintDoesNotDependOnAllowlistOrder()
    {
        FeatureFlagSnapshot before = FeatureFlagSnapshot.SafeDefault(
            FeatureFlagEnvironments.Lab);
        FeatureFlagSnapshot ascending = before with
        {
            LabDestinationAllowlist = new HashSet<string>(
                ["alpha", "beta", "gamma"], StringComparer.Ordinal),
        };
        FeatureFlagSnapshot descending = before with
        {
            LabDestinationAllowlist = new HashSet<string>(
                ["gamma", "beta", "alpha"], StringComparer.Ordinal),
        };

        Assert.Equal(
            RuntimeGateFingerprint.Of(before, ascending),
            RuntimeGateFingerprint.Of(before, descending));

        // And it does change when the change changes, which is the other half of being useful.
        Assert.NotEqual(
            RuntimeGateFingerprint.Of(before, ascending),
            RuntimeGateFingerprint.Of(
                before,
                before with { GlobalDialKillSwitch = false }));
    }

    /// <summary>
    /// The <c>environment</c> column scopes one approval kind and is inert for the other two, and
    /// nothing in the schema says which is which.
    /// <para>
    /// <c>FEATURE_FLAG_CHANGE</c> is scoped twice over: the verifier's query filters
    /// <c>environment</c>, and the fingerprint it also matches on hashes
    /// <c>snapshot.Environment</c> as its first field. A lab approval therefore cannot travel to a
    /// production change even if someone reuses the reference.
    /// </para>
    /// <para>
    /// <c>RUNTIME_GATE_ADMIN</c> and <c>PRODUCTION_CALL</c> are read through
    /// <c>RuntimeGateApprovalReader.AnyLiveAsync</c>, which asks only for kind, revocation and
    /// expiry. Their <c>environment</c> value is written, stored, and never consulted. That is
    /// defensible - administration is a coarse capability and every individual risk-increasing
    /// change is still bound to an environment by four eyes - but it is a trap for whoever inserts
    /// the next row, because setting <c>environment</c> to a single environment looks like scoping
    /// and does nothing. W-0213.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-10")]
    public async Task EnvironmentScopesTheFlagChangeApprovalAndIsInertForTheAdminGrant()
    {
        await fixture.ResetAsync();

        // Scoped: an approval granted for Lab does not verify the same change in Production.
        FeatureFlagSnapshot lab = FeatureFlagSnapshot.SafeDefault(FeatureFlagEnvironments.Lab);
        FeatureFlagSnapshot labAfter = lab.Apply(
            new FeatureFlagChangeSet(GlobalDialKillSwitch: false));
        FeatureFlagSnapshot production =
            FeatureFlagSnapshot.SafeDefault(FeatureFlagEnvironments.Production);
        FeatureFlagSnapshot productionAfter = production.Apply(
            new FeatureFlagChangeSet(GlobalDialKillSwitch: false));

        await InsertApprovalAsync(
            reference: "approval-env-1",
            proposer: "operator-1",
            approver: "operator-2",
            fingerprint: RuntimeGateFingerprint.Of(lab, labAfter),
            environment: FeatureFlagEnvironments.Lab);

        IFourEyesApprovalVerifier verifier = fixture.Services
            .GetRequiredService<IFourEyesApprovalVerifier>();

        Assert.Equal(
            "operator-2",
            await verifier.VerifyAsync("approval-env-1", "operator-1", lab, labAfter));
        Assert.Null(await verifier.VerifyAsync(
            "approval-env-1", "operator-1", production, productionAfter));

        // The fingerprint alone already separates them, before the column predicate is reached.
        Assert.NotEqual(
            RuntimeGateFingerprint.Of(lab, labAfter),
            RuntimeGateFingerprint.Of(production, productionAfter));

        // Inert, in two steps, because the table will not let this be shown in one.
        IRuntimeGateAuthorization authorization = fixture.Services
            .GetRequiredService<IRuntimeGateAuthorization>();

        Assert.True(await authorization.IsApprovedAsync());

        // Narrowing a granted approval in place is refused outright: the append-only trigger
        // allows revocation and nothing else. So an admin grant cannot be scoped after the fact
        // even by someone with database access.
        PostgresException immutable = await Assert.ThrowsAsync<PostgresException>(() =>
            ExecuteAsync(
                "UPDATE ivr_runtime_gate_approvals SET environment = 'lab' "
                + "WHERE approval_kind = 'RUNTIME_GATE_ADMIN'"));
        Assert.Contains("only revocation may change", immutable.MessageText, StringComparison.Ordinal);

        // Revoke the seeded grant, and the gate closes - revocation is the switch.
        await ExecuteAsync(
            "UPDATE ivr_runtime_gate_approvals SET revoked_at = now(), "
            + "revoked_reason = 'IT-GATE-APPROVAL-10' "
            + "WHERE approval_kind = 'RUNTIME_GATE_ADMIN'");

        Assert.False(await authorization.IsApprovedAsync());

        // Now grant it again, scoped to lab only. The gate answers yes with no environment asked
        // for and none supplied, which is the whole point: on this kind the column is recorded,
        // never read. Whoever writes 'lab' here believing it limits the grant is mistaken.
        await ExecuteAsync(
            """
            INSERT INTO ivr_runtime_gate_approvals (
                approval_reference, approval_kind, environment, proposer_actor_id,
                approver_actor_id, change_fingerprint, reason, signed_decision_ref,
                granted_at, expires_at, revoked_at, revoked_reason, correlation_id)
            VALUES (
                'IT-GATE-APPROVAL-10/lab-scoped', 'RUNTIME_GATE_ADMIN', 'lab', NULL,
                'operator-3', NULL, 'scoped to lab on purpose', 'OD-V1-20@2026-09-05',
                now(), NULL, NULL, NULL, 'corr-test')
            """);

        Assert.True(await authorization.IsApprovedAsync());
    }

    private Task InsertApprovalAsync(
        string reference,
        string proposer,
        string approver,
        string? fingerprint,
        string environment = "lab",
        string grantedAtSql = "now()",
        string? expiresAtSql = null)
    {
        string fingerprintSql = fingerprint is null ? "NULL" : $"'{fingerprint}'";
        return ExecuteAsync(
            $"""
            INSERT INTO ivr_runtime_gate_approvals (
                approval_reference, approval_kind, environment, proposer_actor_id,
                approver_actor_id, change_fingerprint, reason, signed_decision_ref,
                granted_at, expires_at, revoked_at, revoked_reason, correlation_id)
            VALUES (
                '{reference}', 'FEATURE_FLAG_CHANGE', '{environment}', '{proposer}',
                '{approver}', {fingerprintSql}, 'test approval', 'OD-V1-20@2026-09-05',
                {grantedAtSql}, {expiresAtSql ?? "NULL"}, NULL, NULL, 'corr-test')
            """);
    }

    private async Task ExecuteAsync(string sql)
    {
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using IvrDbContext dbContext = await factory.CreateDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(sql);
    }

    private async Task<string?> ScalarAsync(string sql)
    {
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using IvrDbContext dbContext = await factory.CreateDbContextAsync();
        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        await dbContext.Database.OpenConnectionAsync();
        object? value = await command.ExecuteScalarAsync();
        return value as string;
    }
}
