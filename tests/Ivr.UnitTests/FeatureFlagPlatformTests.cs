using Ivr.Domain.Errors;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.FeatureFlags;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ivr.UnitTests;

public sealed class FeatureFlagPlatformTests
{
    [Fact]
    [Trait("TestId", "UT-FLAG-DEFAULT-01")]
    public async Task RiskyFlagsDefaultSafeAndProviderFailureCannotReusePermissiveCache()
    {
        InMemoryAuditLogger audit = new(TimeProvider.System);
        using InMemoryFeatureFlagStore store = new(audit);
        FeatureFlagPlatform platform = new(store, TimeProvider.System);
        FeatureFlagReadResult defaults = await platform.GetSnapshotAsync(
            FeatureFlagEnvironments.Lab);

        Assert.False(defaults.Snapshot.RealCustomerCallAllowed);
        Assert.True(defaults.Snapshot.GlobalDialKillSwitch);
        Assert.False(defaults.Snapshot.V1NotificationEnabled);
        Assert.False(defaults.Snapshot.RecordingEnabled);
        Assert.Empty(defaults.Snapshot.LabDestinationAllowlist);

        store.Available = false;
        FeatureFlagReadResult failed = await platform.GetSnapshotAsync(
            FeatureFlagEnvironments.Lab,
            forceFresh: true);
        Assert.False(failed.ProviderReadable);
        Assert.True(failed.Snapshot.GlobalDialKillSwitch);
        Assert.False(await new KillSwitch(platform).RealCallsEnabledAsync(
            FeatureFlagEnvironments.Lab));
    }

    /// <summary>
    /// W-0193. A fail-closed fallback has to say so.
    /// <para>
    /// The fallback itself was always right — an unreadable store degrades to the safe default
    /// rather than to the last permissive value. What it did not do was leave a trace, and a
    /// silent fallback is indistinguishable from a working read: every caller saw plausible safe
    /// values, so a store holding no environments at all looked exactly like a healthy one.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-FLAG-FALLBACKLOG-11")]
    public async Task AFallbackToTheSafeDefaultIsLoggedRatherThanSwallowed()
    {
        InMemoryAuditLogger audit = new(TimeProvider.System);
        using InMemoryFeatureFlagStore store = new(audit);
        CapturingLogger<FeatureFlagPlatform> logger = new();
        FeatureFlagPlatform platform = new(store, TimeProvider.System, logger);

        FeatureFlagReadResult healthy = await platform.GetSnapshotAsync(
            FeatureFlagEnvironments.Lab,
            forceFresh: true);
        Assert.True(healthy.ProviderReadable);
        Assert.Empty(logger.Warnings);

        store.Available = false;
        FeatureFlagReadResult degraded = await platform.GetSnapshotAsync(
            FeatureFlagEnvironments.Lab,
            forceFresh: true);

        Assert.False(degraded.ProviderReadable);
        string warning = Assert.Single(logger.Warnings);
        Assert.Contains(FeatureFlagEnvironments.Lab, warning, StringComparison.Ordinal);

        // The exception TYPE, never its message: the message can carry a store detail, and this
        // line is written on the path that runs before every dispatch decision.
        Assert.Contains(nameof(IvrFailureException), warning, StringComparison.Ordinal);
        Assert.DoesNotContain("unavailable", warning, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// W-0195. Stopping something must never need a permission that has not been granted yet.
    /// <para>
    /// <c>W-0068</c> settled the asymmetry — engaging the kill switch is always available, only
    /// disengaging it needs approval — but <c>MutateAsync</c> asked the runtime-gate
    /// authorization before it looked at what the change was. With the gate ungranted the API
    /// could not engage the kill switch either, so the one control that exists for an emergency
    /// was removed by the absence of a permission. This asserts both halves of the asymmetry
    /// against the same ungranted gate.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-FLAG-KILLSWITCH-12")]
    public async Task AnUngrantedRuntimeGateStillLetsTheKillSwitchBeEngaged()
    {
        TestPlatform test = TestPlatform.Create(runtimeGateApproved: false);
        await using (test)
        {
            FeatureFlagMutationResult engaged = await test.Admin.MutateAsync(
                new FeatureFlagMutationCommand(
                    FeatureFlagEnvironments.Lab,
                    new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
                    "emergency stop",
                    "operator-1",
                    null,
                    null,
                    "corr-killswitch-on"));
            Assert.True(engaged.Snapshot.GlobalDialKillSwitch);

            // The other direction is exactly what the ungranted gate is for.
            IvrFailureException blocked = await Assert.ThrowsAsync<IvrFailureException>(
                () => test.Admin.MutateAsync(new FeatureFlagMutationCommand(
                    FeatureFlagEnvironments.Lab,
                    new FeatureFlagChangeSet(GlobalDialKillSwitch: false),
                    "resume dialling",
                    "operator-1",
                    null,
                    "approval-1",
                    "corr-killswitch-off")));
            Assert.Equal(IvrErrorCodes.OperationalBlocked, blocked.ErrorCode);
        }
    }

    [Fact]
    [Trait("TestId", "UT-FLAG-GUARD-02")]
    public async Task AdminCannotEnableRealCustomerCallsEvenWithRuntimeApproval()
    {
        TestPlatform test = TestPlatform.Create();
        await using (test)
        {
            IvrFailureException failure = await Assert.ThrowsAsync<IvrFailureException>(
                () => test.Admin.MutateAsync(new FeatureFlagMutationCommand(
                    FeatureFlagEnvironments.Production,
                    new FeatureFlagChangeSet(
                        ExecutionMode: "PRODUCTION_REAL",
                        SalesProvider: FeatureFlagValues.TargetV1,
                        SimProvider: FeatureFlagValues.VendorSimProvider,
                        AttemptPolicyVersion: "approved-v1",
                        RealCustomerCallAllowed: true),
                    "prepare production calling",
                    "operator-1",
                    null,
                    "approval-1",
                    "corr-guard-1")));

            Assert.Equal(IvrErrorCodes.PolicyMismatch, failure.ErrorCode);
            FeatureFlagSnapshot unchanged = await test.Store.ReadFreshAsync(
                FeatureFlagEnvironments.Production);
            Assert.False(unchanged.RealCustomerCallAllowed);
        }
    }

    [Fact]
    [Trait("TestId", "UT-FLAG-AUDIT-03")]
    public async Task MutationAuditsActorReasonBeforeAndAfterBeforeAtomicStoreChange()
    {
        TestPlatform test = TestPlatform.Create();
        await using (test)
        {
            FeatureFlagMutationResult result = await test.Admin.MutateAsync(
                new FeatureFlagMutationCommand(
                    FeatureFlagEnvironments.Lab,
                    new FeatureFlagChangeSet(
                        LabDestinationAllowlist: ["lab-destination-a"]),
                    "allow bounded lab test",
                    "operator-1",
                    null,
                    "approval-1",
                    "corr-audit-1"));

            Assert.Equal("approver-2", result.ApprovedBy);
            AuditLogEntry entry = Assert.Single(test.Audit.Entries);
            Assert.Equal("operator-1", entry.Actor);
            Assert.Equal("allow bounded lab test", entry.Reason);
            Assert.Contains("before", entry.DataJson, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("after", entry.DataJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                typeof(IAuditLogger).GetMethods(),
                method => method.Name.StartsWith("Update", StringComparison.Ordinal)
                    || method.Name.StartsWith("Delete", StringComparison.Ordinal));
        }

        ThrowingAuditLogger failingAudit = new();
        FeatureFlagSnapshot seed = LabRealSeed(globalDialKillSwitch: false);
        using InMemoryFeatureFlagStore failingStore = new(failingAudit, [seed]);
        FeatureFlagPlatform failingPlatform = new(failingStore, TimeProvider.System);
        FeatureFlagAdminService failingAdmin = new(
            failingPlatform,
            failingStore,
            failingPlatform,
            new ApprovedRuntimeGateAuthorization(),
            new ApprovedFourEyesVerifier());
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => failingAdmin.MutateAsync(new FeatureFlagMutationCommand(
                FeatureFlagEnvironments.Lab,
                new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
                "emergency stop",
                "operator-1",
                null,
                null,
                "corr-audit-2")));
        FeatureFlagSnapshot unchangedAfterAuditFailure = await failingStore.ReadFreshAsync(
            FeatureFlagEnvironments.Lab);
        Assert.Equal(seed, unchangedAfterAuditFailure);
    }

    [Fact]
    [Trait("TestId", "UT-FLAG-ALLOWLIST-06")]
    public async Task AllowlistExpansionRequiresReasonIndependentApprovalAndNoSelfAuthorization()
    {
        TestPlatform test = TestPlatform.Create();
        await using (test)
        {
            FeatureFlagMutationCommand withoutReason = new(
                FeatureFlagEnvironments.Lab,
                new FeatureFlagChangeSet(
                    LabDestinationAllowlist: ["lab-destination-a"]),
                string.Empty,
                "operator-1",
                null,
                "approval-1",
                "corr-allow-1");
            Assert.Equal(
                IvrErrorCodes.MalformedRequest,
                (await Assert.ThrowsAsync<IvrFailureException>(
                    () => test.Admin.MutateAsync(withoutReason))).ErrorCode);

            FeatureFlagMutationCommand withoutApproval = withoutReason with
            {
                Reason = "allow lab test",
                ApprovalReference = null,
                CorrelationId = "corr-allow-2",
            };
            Assert.Equal(
                IvrErrorCodes.PolicyMismatch,
                (await Assert.ThrowsAsync<IvrFailureException>(
                    () => test.Admin.MutateAsync(withoutApproval))).ErrorCode);

            FeatureFlagMutationCommand selfAuthorization = withoutApproval with
            {
                ActorDestinationReference = "lab-destination-a",
                ApprovalReference = "approval-1",
                CorrelationId = "corr-allow-3",
            };
            Assert.Equal(
                IvrErrorCodes.PolicyMismatch,
                (await Assert.ThrowsAsync<IvrFailureException>(
                    () => test.Admin.MutateAsync(selfAuthorization))).ErrorCode);
            Assert.Empty((await test.Store.ReadFreshAsync(
                FeatureFlagEnvironments.Lab)).LabDestinationAllowlist);
        }
    }

    [Fact]
    [Trait("TestId", "UT-FLAG-MODEL-11")]
    public void EfModelDefinesCanonicalFeatureFlagTableAndCompositeIdentity()
    {
        Microsoft.EntityFrameworkCore.DbContextOptions<Ivr.Infrastructure.Persistence.IvrDbContext>
            options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<
                    Ivr.Infrastructure.Persistence.IvrDbContext>()
                .UseNpgsql("Host=localhost;Database=model_only;Username=ivr")
                .Options;
        using Ivr.Infrastructure.Persistence.IvrDbContext context = new(options);
        IModel designTimeModel = context.GetService<IDesignTimeModel>().Model;
        IEntityType entity = designTimeModel.FindEntityType(
                typeof(FeatureFlagEntity))
            ?? throw new InvalidOperationException("FeatureFlagEntity mapping is missing.");

        Assert.Equal("ivr_feature_flags", entity.GetTableName());
        Assert.Equal(
            [nameof(FeatureFlagEntity.Key), nameof(FeatureFlagEntity.Environment)],
            entity.FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Equal(45, entity.GetSeedData().Count());
    }

    private static FeatureFlagSnapshot LabRealSeed(bool globalDialKillSwitch) =>
        FeatureFlagSnapshot.SafeDefault(FeatureFlagEnvironments.Lab) with
        {
            Revision = 7,
            ExecutionMode = "LAB_REAL_SIM",
            SimProvider = FeatureFlagValues.VendorSimProvider,
            GlobalDialKillSwitch = globalDialKillSwitch,
            LabDestinationAllowlist = new HashSet<string>(
                ["lab-destination-a"],
                StringComparer.Ordinal),
        };

    /// <summary>Records warning-level lines so a test can assert the platform said something.</summary>
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        private readonly List<string> warnings = [];

        public IReadOnlyList<string> Warnings => warnings;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            if (logLevel >= LogLevel.Warning)
            {
                warnings.Add(formatter(state, exception));
            }
        }
    }

    private sealed class TestPlatform : IAsyncDisposable
    {
        private TestPlatform(
            InMemoryAuditLogger audit,
            InMemoryFeatureFlagStore store,
            FeatureFlagAdminService admin)
        {
            Audit = audit;
            Store = store;
            Admin = admin;
        }

        public InMemoryAuditLogger Audit { get; }

        public InMemoryFeatureFlagStore Store { get; }

        public FeatureFlagAdminService Admin { get; }

        public static TestPlatform Create(bool runtimeGateApproved = true)
        {
            InMemoryAuditLogger audit = new(TimeProvider.System);
            InMemoryFeatureFlagStore store = new(audit);
            FeatureFlagPlatform platform = new(store, TimeProvider.System);
            FeatureFlagAdminService admin = new(
                platform,
                store,
                platform,
                runtimeGateApproved
                    ? new ApprovedRuntimeGateAuthorization()
                    : new PendingRuntimeGateAuthorization(),
                new ApprovedFourEyesVerifier());
            return new TestPlatform(audit, store, admin);
        }

        public ValueTask DisposeAsync()
        {
            Store.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ApprovedRuntimeGateAuthorization : IRuntimeGateAuthorization
    {
        public Task<bool> IsApprovedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class ApprovedFourEyesVerifier : IFourEyesApprovalVerifier
    {
        public Task<string?> VerifyAsync(
            string approvalReference,
            string proposerActorId,
            FeatureFlagSnapshot before,
            FeatureFlagSnapshot after,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(approvalReference == "approval-1" ? "approver-2" : null);
    }

    private sealed class ThrowingAuditLogger : IAuditLogger
    {
        public Task<AuditLogEntry> AppendAsync(
            AuditEvent auditEvent,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("audit unavailable");
    }
}
