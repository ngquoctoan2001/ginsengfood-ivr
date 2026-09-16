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
    /// W-0301. The shipped schema grants runtime-gate administration in no environment, and the
    /// row that once granted it in all of them is still on file, revoked.
    /// <para>
    /// <c>W0195</c> seeded a <c>RUNTIME_GATE_ADMIN</c> approval with <c>environment</c> null, and
    /// the reader asked only whether any live one existed — so that single row opened every
    /// environment that could reach this database. It is revoked rather than deleted because the
    /// append-only trigger permits nothing else, and because an approval log that loses its
    /// history is not one.
    /// </para>
    /// <para>
    /// Asserted across every environment rather than one: "no environment" is the claim, and
    /// naming a single one would pass while another stayed open.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-01")]
    public async Task TheShippedSchemaGrantsRuntimeGateAdministrationNowhere()
    {
        await fixture.ResetAsync();
        IRuntimeGateAuthorization authorization = fixture.Services
            .GetRequiredService<IRuntimeGateAuthorization>();

        foreach (string environment in FeatureFlagEnvironments.All)
        {
            Assert.False(
                await authorization.IsApprovedAsync(environment),
                $"runtime-gate administration is open in {environment}");
        }

        // The history survives: the seeded row is still there, revoked and holding its reason.
        Assert.Equal("1", await ScalarAsync(
            "SELECT count(*)::text FROM ivr_runtime_gate_approvals "
            + "WHERE approval_kind = 'RUNTIME_GATE_ADMIN' AND revoked_at IS NOT NULL"));
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

        Assert.False(await gate.IsApprovedAsync(FeatureFlagEnvironments.Production));
        Assert.Equal(
            "0",
            await ScalarAsync(
                "SELECT count(*)::text FROM ivr_runtime_gate_approvals "
                + "WHERE approval_kind = 'PRODUCTION_CALL'"));
    }

    /// <summary>Revoking a grant closes the gate again, without deleting the history.</summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-03")]
    public async Task RevokingTheApprovalClosesTheGateAndKeepsTheRow()
    {
        await fixture.ResetAsync();
        IRuntimeGateAuthorization authorization = fixture.Services
            .GetRequiredService<IRuntimeGateAuthorization>();

        // W-0301. The shipped schema grants nothing, so this grants a scoped approval first -
        // otherwise the test would prove only that a closed gate stays closed.
        await InsertAdminApprovalAsync("approval-admin-lab", FeatureFlagEnvironments.Lab);
        Assert.True(await authorization.IsApprovedAsync(FeatureFlagEnvironments.Lab));

        await ExecuteAsync(
            "UPDATE ivr_runtime_gate_approvals "
            + "SET revoked_at = now(), revoked_reason = 'test revocation' "
            + "WHERE approval_reference = 'approval-admin-lab'");

        Assert.False(await authorization.IsApprovedAsync(FeatureFlagEnvironments.Lab));
        Assert.Equal("2", await ScalarAsync(
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
    /// The <c>environment</c> column now scopes every approval kind, and this pins the one that
    /// scopes twice.
    /// <para>
    /// <c>FEATURE_FLAG_CHANGE</c> is narrowed by the column <b>and</b> by the change fingerprint,
    /// which hashes <c>snapshot.Environment</c> as its first field. A lab approval therefore
    /// cannot travel to a production change even if someone reuses the reference — the fingerprint
    /// separates them before the column predicate is reached.
    /// </para>
    /// <para>
    /// W-0301 rewrote the second half of this test. It used to assert the opposite: that on
    /// <c>RUNTIME_GATE_ADMIN</c> the column was "recorded, never read", and that whoever wrote
    /// <c>lab</c> there believing it limited the grant was mistaken. That was true, and it was the
    /// bug rather than a caveat — an approver could fill the column in, believe they had limited
    /// themselves to lab, and have opened production. The column is read now, and what follows
    /// proves the grant stays where it was given.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-10")]
    public async Task EnvironmentScopesBothTheFlagChangeApprovalAndTheAdminGrant()
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

        // The admin grant is scoped by the column, and only by it.
        IRuntimeGateAuthorization authorization = fixture.Services
            .GetRequiredService<IRuntimeGateAuthorization>();

        await InsertAdminApprovalAsync("approval-admin-lab", FeatureFlagEnvironments.Lab);

        Assert.True(await authorization.IsApprovedAsync(FeatureFlagEnvironments.Lab));
        Assert.False(await authorization.IsApprovedAsync(FeatureFlagEnvironments.Production));
        Assert.False(await authorization.IsApprovedAsync(FeatureFlagEnvironments.Staging));

        // Narrowing a granted approval in place is still refused outright: the append-only trigger
        // allows revocation and nothing else. Re-scoping is a new row, never an edit - which is
        // why W-0301 had to revoke the unscoped seed rather than correct it.
        PostgresException immutable = await Assert.ThrowsAsync<PostgresException>(() =>
            ExecuteAsync(
                "UPDATE ivr_runtime_gate_approvals SET environment = 'prod' "
                + "WHERE approval_reference = 'approval-admin-lab'"));
        Assert.Contains("only revocation may change", immutable.MessageText, StringComparison.Ordinal);

        // Revoking the lab grant closes lab and changes nothing elsewhere, because nothing
        // elsewhere was ever open.
        await ExecuteAsync(
            "UPDATE ivr_runtime_gate_approvals SET revoked_at = now(), "
            + "revoked_reason = 'IT-GATE-APPROVAL-10' "
            + "WHERE approval_reference = 'approval-admin-lab'");

        Assert.False(await authorization.IsApprovedAsync(FeatureFlagEnvironments.Lab));
    }

    /// <summary>
    /// W-0301. The database refuses a <b>live</b> runtime-gate admin approval that names no
    /// environment.
    /// <para>
    /// The query alone would make such a row inert, which is the quieter half of the same bug: an
    /// approver would believe they had granted administration and it would silently do nothing.
    /// The constraint exempts revoked rows, and has to — the row <c>W0195</c> seeded keeps its null
    /// environment for ever because the append-only trigger will not let anyone change it. A
    /// revoked approval opens nothing whatever its columns say, so the exemption costs no part of
    /// the invariant.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-13")]
    public async Task AnUnscopedRuntimeGateAdminApprovalIsRefusedByTheDatabase()
    {
        await fixture.ResetAsync();

        Exception? failure = await Record.ExceptionAsync(
            () => InsertAdminApprovalAsync("approval-admin-unscoped", environment: null));

        Assert.NotNull(failure);
        Assert.Equal(
            "0",
            await ScalarAsync(
                "SELECT count(*)::text FROM ivr_runtime_gate_approvals "
                + "WHERE approval_kind = 'RUNTIME_GATE_ADMIN' AND revoked_at IS NULL"));
    }

    /// <summary>
    /// SIP-04. A production-call approval opens the environment it names, and only that one.
    /// <para>
    /// Before this the gate asked only whether any live <c>PRODUCTION_CALL</c> approval existed, so
    /// the pilot signature below would have opened production as well - and the column that says
    /// otherwise was already on the row, unused. That is the failure this pins, in the direction
    /// that matters: not "does an approval work" but "does it stay where it was granted".
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-11")]
    public async Task AProductionCallApprovalOpensOnlyTheEnvironmentItNames()
    {
        await fixture.ResetAsync();
        IProductionCallGate gate = fixture.Services.GetRequiredService<IProductionCallGate>();

        await InsertProductionCallApprovalAsync("approval-pilot", FeatureFlagEnvironments.Pilot);

        Assert.True(await gate.IsApprovedAsync(FeatureFlagEnvironments.Pilot));
        Assert.False(await gate.IsApprovedAsync(FeatureFlagEnvironments.Production));
        Assert.False(await gate.IsApprovedAsync(FeatureFlagEnvironments.Staging));

        // Revoking the pilot signature closes the pilot and changes nothing elsewhere, because
        // nothing elsewhere was ever open.
        await ExecuteAsync(
            "UPDATE ivr_runtime_gate_approvals SET revoked_at = now(), "
            + "revoked_reason = 'IT-GATE-APPROVAL-11' "
            + "WHERE approval_reference = 'approval-pilot'");

        Assert.False(await gate.IsApprovedAsync(FeatureFlagEnvironments.Pilot));
    }

    /// <summary>
    /// The database refuses a production-call approval that names no environment.
    /// <para>
    /// The query alone would make such a row inert, which is the quieter half of the same bug: an
    /// approver would believe they had granted something, and it would silently do nothing. A row
    /// authorising real customer calls should not be storable in a shape nobody can act on.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-GATE-APPROVAL-12")]
    public async Task AnUnscopedProductionCallApprovalIsRefusedByTheDatabase()
    {
        await fixture.ResetAsync();

        Exception? failure = await Record.ExceptionAsync(
            () => InsertProductionCallApprovalAsync("approval-unscoped", environment: null));

        Assert.NotNull(failure);
        Assert.Equal(
            "0",
            await ScalarAsync(
                "SELECT count(*)::text FROM ivr_runtime_gate_approvals "
                + "WHERE approval_kind = 'PRODUCTION_CALL'"));
    }

    private Task InsertAdminApprovalAsync(string reference, string? environment)
    {
        string environmentSql = environment is null ? "NULL" : $"'{environment}'";
        return ExecuteAsync(
            $"""
            INSERT INTO ivr_runtime_gate_approvals (
                approval_reference, approval_kind, environment, proposer_actor_id,
                approver_actor_id, change_fingerprint, reason, signed_decision_ref,
                granted_at, expires_at, revoked_at, revoked_reason, correlation_id)
            VALUES (
                '{reference}', 'RUNTIME_GATE_ADMIN', {environmentSql}, NULL,
                'operator-3', NULL, 'test administration grant', 'OD-V1-20@2026-09-05',
                now(), NULL, NULL, NULL, 'corr-test')
            """);
    }

    private Task InsertProductionCallApprovalAsync(string reference, string? environment)
    {
        string environmentSql = environment is null ? "NULL" : $"'{environment}'";
        return ExecuteAsync(
            $"""
            INSERT INTO ivr_runtime_gate_approvals (
                approval_reference, approval_kind, environment, proposer_actor_id,
                approver_actor_id, change_fingerprint, reason, signed_decision_ref,
                granted_at, expires_at, revoked_at, revoked_reason, correlation_id)
            VALUES (
                '{reference}', 'PRODUCTION_CALL', {environmentSql}, 'operator-1',
                'operator-2', NULL, 'test release approval', 'OD-V1-12@2026-09-15',
                now(), NULL, NULL, NULL, 'corr-test')
            """);
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
