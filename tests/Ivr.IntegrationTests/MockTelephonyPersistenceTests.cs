using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Speech;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class MockTelephonyPersistenceTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 16, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-TEL-REVOKE-02")]
    public async Task ARevokeLandingAfterTheClaimStillStopsTheDial()
    {
        // Fence 2 of 2. This is the case fence 1 cannot catch: the claim already happened, the
        // channel is reserved, the lease is valid -- and EnsureCurrentLease will happily confirm
        // all of that, because it answers a technical question. Whether the order still wants
        // calling is a different question, and LoadAsync is the last place IVR can ask it.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-REVOKE-02", "JOB-TEL-REVOKE-02", "SIM-MOCK-01");
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-mock-tel",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));

        // The revoke arrives here: after the claim, before the dial.
        await using (IvrDbContext revoking = await factory.CreateDbContextAsync())
        {
            ConfirmationTaskEntity task = await revoking.ConfirmationTasks
                .SingleAsync(candidate => candidate.TaskId == "TASK-TEL-REVOKE-02");
            task.RevokedAt = Now;
            task.RevokeReason = "ORDER_CANCELLED";
            task.RevokeOrderVersion = "18";
            await revoking.SaveChangesAsync();
        }

        PostgresTelephonyDispatchStore store = CreateStore(factory);

        InvalidOperationException refused = await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.LoadAsync(lease));
        Assert.Contains("revoked", refused.Message, StringComparison.OrdinalIgnoreCase);

        // And the lease itself was still perfectly valid, which is the whole point: the technical
        // fence would have let this through.
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        Assert.Equal("LEASED_PENDING_DISPATCH", attempt.Status);
        Assert.Equal(lease.JobId, channel.ActiveCallJobId);
        Assert.Empty(await verification.RawCallEvents.AsNoTracking().ToListAsync());
    }

    [Fact]
    [Trait("TestId", "IT-TEL-DISPATCH-01")]
    public async Task SchedulerLeaseRunsThroughMockGatewayAndPersistsFencedProviderEvent()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-01", "JOB-TEL-01", "SIM-MOCK-01");
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-mock-tel",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            timeProvider: new FixedTimeProvider(Now));
        PostgresTelephonyDispatchStore store = CreateStore(factory);
        MockSchedulerDispatchGateway gateway = CreateGateway(
            store,
            lease,
            sim,
            includeToken: true);

        await gateway.DispatchAsync(lease);

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        RawCallEventEntity rawEvent = await verification.RawCallEvents.AsNoTracking().SingleAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("PROVIDER_EVENT_PENDING_NORMALIZATION", attempt.Status);
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Equal("ANSWERED", attempt.Disposition);
        Assert.Equal("1", attempt.DtmfKey);
        Assert.Equal(rawEvent.RawEventId, attempt.RawCallEventId);
        Assert.Equal("ANSWERED", rawEvent.RawCallStatus);
        Assert.Null(rawEvent.RecordingRef);
        Assert.Equal("IDLE", channel.Status);
        Assert.Null(channel.LeaseToken);
        Assert.Null(channel.ActiveCallJobId);
        Assert.Equal(lease.FencingGeneration + 1, channel.LeaseFencingGeneration);
        Assert.Equal(Now.AddSeconds(5), channel.CooldownUntil);
        Assert.Equal("DISPOSITION_PENDING_NORMALIZATION", job.Status);
        Assert.Equal("HELD_NORMALIZATION", job.QueueStatus);
        Assert.Equal(6, sim.Events.Count);
        RenderedSpeech played = Assert.Single(sim.PlayedSpeech).Value;
        Assert.NotNull(played.Audio);
        Assert.Equal("audio/L16", played.Audio.Format);
        Assert.Equal(8_000, played.Audio.SampleRate);
        Assert.StartsWith("memory://tts/fake/", played.Audio.ContentRef, StringComparison.Ordinal);
        Assert.DoesNotContain(
            await verification.AuditLog.Select(row => row.DataJson).ToListAsync(),
            json => json.Contains("Anh Đạt", StringComparison.Ordinal)
                || json.Contains("enc:mock-token", StringComparison.Ordinal));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.LoadAsync(lease));
    }

    /// <summary>
    /// W-0113. The voice reaches the attempt row, and it reaches the audit log too.
    /// <para>
    /// Both, on purpose. The column is what the console reads and it can be corrected by a later
    /// write; the audit row is append-only. An evidence pack an owner signs deserves the copy
    /// nobody can quietly amend, and the two agreeing is what makes either of them worth
    /// believing.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TEL-VOICE-05")]
    public async Task DispatchRecordsTheVoiceItDialledWithOnTheAttemptAndInTheAudit()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-05", "JOB-TEL-05", "SIM-MOCK-05");
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-mock-voice",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            timeProvider: new FixedTimeProvider(Now));
        PostgresTelephonyDispatchStore store = CreateStore(factory);
        MockSchedulerDispatchGateway gateway = CreateGateway(store, lease, sim, includeToken: true);

        await gateway.DispatchAsync(lease);

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();

        // The seed delivery area is Vietnamese but the harness leaves regional voices off, so the
        // recorded region is the configured fallback and the flag says exactly that. Asserting
        // the flag matters more than asserting the region: a recorded "North" that silently means
        // "we could not tell" is the sort of number an owner would sign without knowing.
        Assert.False(string.IsNullOrWhiteSpace(attempt.VoiceId));
        Assert.Equal("North", attempt.VoiceRegion);
        Assert.False(attempt.VoiceRegionResolved);

        RenderedSpeech played = Assert.Single(sim.PlayedSpeech).Value;
        Assert.Equal(played.Audio!.Voice!.VoiceId, attempt.VoiceId);
        Assert.Equal(played.Audio.Voice.RegionWireForm, attempt.VoiceRegion);

        List<string> startedAudits = await verification.AuditLog.AsNoTracking()
            .Where(row => row.Action == "SIM_CALL_STARTED")
            .Select(row => row.DataJson)
            .ToListAsync();
        string audit = Assert.Single(startedAudits);
        Assert.Contains(attempt.VoiceId!, audit, StringComparison.Ordinal);
        Assert.Contains("voice_region", audit, StringComparison.Ordinal);
    }

    /// <summary>
    /// Half a voice record is refused by the database, not just by the code that writes it.
    /// A region with no voice id is a claim about what a customer heard that cannot be traced to
    /// a configured voice — precisely the kind of half-fact these columns exist to replace.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TEL-VOICE-06")]
    public async Task ThePartialVoiceRecordIsRefusedByTheDatabase()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-06", "JOB-TEL-06", "SIM-MOCK-06");

        // The attempt row is created by the scheduler claim, not by the seed.
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-mock-voice-constraint",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));

        await using (IvrDbContext half = await factory.CreateDbContextAsync())
        {
            CallAttemptEntity attempt = await half.CallAttempts.SingleAsync();
            attempt.VoiceRegion = "South";
            await Assert.ThrowsAsync<DbUpdateException>(() => half.SaveChangesAsync());
        }

        await using (IvrDbContext unknown = await factory.CreateDbContextAsync())
        {
            CallAttemptEntity attempt = await unknown.CallAttempts.SingleAsync();
            attempt.VoiceId = "voice-x";
            attempt.VoiceRegion = "Northeast";
            attempt.VoiceRegionResolved = true;
            await Assert.ThrowsAsync<DbUpdateException>(() => unknown.SaveChangesAsync());
        }

        // And the complete, in-vocabulary record is accepted, so the two refusals above are
        // about the shape of the record rather than about the column being unwritable.
        await using (IvrDbContext whole = await factory.CreateDbContextAsync())
        {
            CallAttemptEntity attempt = await whole.CallAttempts.SingleAsync();
            attempt.VoiceId = "voice-south";
            attempt.VoiceRegion = "South";
            attempt.VoiceRegionResolved = true;
            await whole.SaveChangesAsync();
        }
    }

    [Fact]
    [Trait("TestId", "IT-TEL-TOKENFAIL-02")]
    public async Task MissingMockTokenFailsClosedReleasesLeaseAndDoesNotCountAttempt()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-02", "JOB-TEL-02", "SIM-MOCK-02");
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-token-fail",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            timeProvider: new FixedTimeProvider(Now));
        MockSchedulerDispatchGateway gateway = CreateGateway(
            CreateStore(factory),
            lease,
            sim,
            includeToken: false);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => gateway.DispatchAsync(lease));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        RawCallEventEntity rawEvent = await verification.RawCallEvents.AsNoTracking().SingleAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        Assert.Equal("PROVIDER_EVENT_PENDING_NORMALIZATION", attempt.Status);
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Equal("MOCK_DEPENDENCY_NOT_FOUND", attempt.TechnicalExceptionType);
        Assert.Equal("MOCK_DEPENDENCY_NOT_FOUND", rawEvent.TechnicalErrorCode);
        Assert.Equal("IDLE", channel.Status);
        Assert.Null(channel.LeaseToken);
        Assert.Single(sim.Events);
        Assert.Equal(SimProviderEventType.HealthChecked, sim.Events.Single().Type);
    }

    /// <summary>
    /// W-0354 / B16 (chief worklist 2026-09-25). An amount the speller refuses - here one past its
    /// range, which intake does not bound - used to reach the gateway's generic catch arm, which
    /// reports the channel unhealthy: the SIM went into quarantine for a fault in the order's data,
    /// and three such orders in ten minutes disabled it. The renderer now names the fault as the
    /// order's, so the attempt is an uncounted technical exception and the channel is handed back.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TEL-RENDER-DATA-09")]
    public async Task AnOrderTheRendererRefusesLeavesTheChannelIdleAndUnquarantined()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(
            factory,
            "TASK-TEL-09",
            "JOB-TEL-09",
            "SIM-MOCK-09",
            totalAmount: 1_000_000_000_000m);
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-render-data",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            timeProvider: new FixedTimeProvider(Now));
        var clock = new FixedTimeProvider(Now);
        IOptions<TtsProviderOptions> tts = Options.Create(new TtsProviderOptions
        {
            ExecutionMode = IvrOptions.MockExecutionMode,
            Provider = TtsProviderOptions.FakeProvider,
        });
        using var scripts = new Ivr.Infrastructure.Scripts.InMemoryScriptRegistry(
            new Ivr.Infrastructure.Audit.InMemoryAuditLogger(clock),
            clock,
            Options.Create(new Ivr.Infrastructure.Scripts.ScriptContentOptions()));
        var approvedRenderer = new ApprovedVietnameseSpeechRenderer(
            scripts,
            new Ivr.Domain.Scripts.VietnameseOrderScriptRenderer(),
            new RegionalVoiceMap(tts));
        MockSchedulerDispatchGateway gateway = CreateGateway(
            CreateStore(factory),
            lease,
            sim,
            includeToken: true,
            renderer: approvedRenderer);

        await Assert.ThrowsAsync<SpeechRenderRejectedException>(() => gateway.DispatchAsync(lease));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        RawCallEventEntity rawEvent = await verification.RawCallEvents.AsNoTracking().SingleAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Equal(SpeechRenderRejectedException.TechnicalCode, attempt.TechnicalExceptionType);
        Assert.Equal(SpeechRenderRejectedException.TechnicalCode, rawEvent.TechnicalErrorCode);
        Assert.Equal("IDLE", channel.Status);
        Assert.Equal(0, channel.FailCount);
        Assert.Null(channel.QuarantineUntil);
        Assert.Null(channel.LeaseToken);
        // The refusal happens before anything touches the SIM.
        Assert.Empty(sim.Events);
    }

    /// <summary>
    /// W-0359 / K-30 (B16). The refusal IT-TEL-RENDER-DATA-09 records once, followed to the end
    /// of the order's window: the retry is refused as well, the order is held for a person, and
    /// the hold is closed when the window runs out - and the SIM comes out of it untouched.
    /// <para>
    /// This pins the sequence the code actually runs. The first refusal normalises to an uncounted
    /// technical exception with a retry left, which puts the job back in the queue. The retry is
    /// attempt 1 again - nothing reached the customer, so nothing was counted - and waits out the
    /// cooldown the first refusal left on the channel. The second refusal spends the one technical
    /// retry: the job is held for admin review and a review item is opened. A hold is not a close,
    /// so the window sweep closes it once the window has passed, as a window expiry nobody was
    /// reached for, which asks Core to put the order in front of a person rather than expire it.
    /// The channel is handed back healthy after each refusal, so its failure count never moves and
    /// it is never quarantined: the fault travels with the order, not with the SIM.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TEL-RENDER-DATA-10")]
    public async Task AnOrderRefusedOnItsRetryIsHeldForReviewThenClosedWhenItsWindowPasses()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(
            factory,
            "TASK-TEL-10",
            "JOB-TEL-10",
            "SIM-MOCK-10",
            totalAmount: 1_000_000_000_000m);
        var clock = new FixedTimeProvider(Now);
        IOptions<TtsProviderOptions> tts = Options.Create(new TtsProviderOptions
        {
            ExecutionMode = IvrOptions.MockExecutionMode,
            Provider = TtsProviderOptions.FakeProvider,
        });
        using var scripts = new Ivr.Infrastructure.Scripts.InMemoryScriptRegistry(
            new Ivr.Infrastructure.Audit.InMemoryAuditLogger(clock),
            clock,
            Options.Create(new Ivr.Infrastructure.Scripts.ScriptContentOptions()));
        var approvedRenderer = new ApprovedVietnameseSpeechRenderer(
            scripts,
            new Ivr.Domain.Scripts.VietnameseOrderScriptRenderer(),
            new RegionalVoiceMap(tts));

        // One fake SIM for both dispatches, answering anything. Neither refusal may reach it, and
        // its event log is where it would show.
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                ["*"] = new(SimProviderDisposition.Answered, "1"),
            },
            timeProvider: clock);

        // Refusal 1, at the start of the window.
        DateTimeOffset firstAt = Now;
        SchedulerDispatchLease first = Assert.IsType<SchedulerDispatchLease>(
            await new PostgresSchedulerStore(factory, new FixedTimeProvider(firstAt))
                .TryClaimDueDispatchAsync(
                    "worker-render-twice",
                    IvrOptions.MockExecutionMode,
                    TimeSpan.FromMinutes(2)));
        await Assert.ThrowsAsync<SpeechRenderRejectedException>(() => CreateGateway(
                CreateStore(factory, firstAt),
                first,
                sim,
                includeToken: true,
                renderer: approvedRenderer)
            .DispatchAsync(first));
        NormalizationPersistenceResult retry = Assert.IsType<NormalizationPersistenceResult>(
            await CreateNormalizer(factory, firstAt.AddSeconds(10))
                .NormalizeNextAsync("normalizer-render-twice-1"));
        Assert.Equal("IVR_TECHNICAL_EXCEPTION", retry.ResultStatus);
        Assert.False(retry.IsCounted);
        Assert.False(retry.IsFinal);
        Assert.True(retry.TechnicalRetryAllowed);
        Assert.Equal(1, retry.TechnicalRetryCount);
        Assert.False(retry.HumanReviewRequired);
        await using (IvrDbContext requeued = await factory.CreateDbContextAsync())
        {
            // Back in the MOCK queue rather than held: one technical retry is still allowed.
            CallJobEntity job = await requeued.CallJobs.AsNoTracking().SingleAsync();
            Assert.Equal("DRY_RUN", job.Status);
            Assert.Equal("HELD_MOCK", job.QueueStatus);
        }

        // Refusal 2, the retry, once the five-second cooldown the first refusal left has passed.
        DateTimeOffset secondAt = firstAt.AddSeconds(30);
        SchedulerDispatchLease second = Assert.IsType<SchedulerDispatchLease>(
            await new PostgresSchedulerStore(factory, new FixedTimeProvider(secondAt))
                .TryClaimDueDispatchAsync(
                    "worker-render-twice",
                    IvrOptions.MockExecutionMode,
                    TimeSpan.FromMinutes(2)));

        // Attempt 1 again, not attempt 2: a refused render never reached the customer, so nothing
        // was counted and the retry is the same customer attempt.
        Assert.Equal(1, second.AttemptNumber);
        Assert.NotEqual(first.AttemptId, second.AttemptId);
        Assert.Equal(first.SimChannelId, second.SimChannelId);
        await Assert.ThrowsAsync<SpeechRenderRejectedException>(() => CreateGateway(
                CreateStore(factory, secondAt),
                second,
                sim,
                includeToken: true,
                renderer: approvedRenderer)
            .DispatchAsync(second));
        NormalizationPersistenceResult held = Assert.IsType<NormalizationPersistenceResult>(
            await CreateNormalizer(factory, secondAt.AddSeconds(10))
                .NormalizeNextAsync("normalizer-render-twice-2"));
        Assert.Equal("IVR_TECHNICAL_EXCEPTION", held.ResultStatus);
        Assert.False(held.IsCounted);
        Assert.False(held.IsFinal);
        Assert.False(held.TechnicalRetryAllowed);
        Assert.Equal(2, held.TechnicalRetryCount);
        Assert.True(held.HumanReviewRequired);
        await using (IvrDbContext review = await factory.CreateDbContextAsync())
        {
            CallJobEntity job = await review.CallJobs.AsNoTracking().SingleAsync();
            Assert.Equal("HELD_ADMIN_REVIEW", job.Status);
            Assert.Equal("HELD_TECHNICAL_REVIEW", job.QueueStatus);
            Assert.Null(job.ClosedAt);
        }

        // Held means held: nothing claims it a third time...
        var scheduler = new PostgresSchedulerStore(
            factory,
            new FixedTimeProvider(secondAt.AddSeconds(30)));
        Assert.Null(await scheduler.TryClaimDueDispatchAsync(
            "worker-render-twice",
            IvrOptions.MockExecutionMode,
            TimeSpan.FromMinutes(2)));

        // ...and a hold is not a close: the sweep leaves it alone until the window has passed, and
        // closes it after.
        DateTimeOffset windowCloses = first.Deadline;
        Assert.Equal(0, await scheduler.CloseMissedDeadlinesAsync(windowCloses.AddSeconds(-1), 32));
        Assert.Equal(1, await scheduler.CloseMissedDeadlinesAsync(windowCloses.AddSeconds(1), 32));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallJobEntity closed = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("WINDOW_EXPIRED", closed.Status);
        Assert.Equal("CLOSED_WINDOW_EXPIRED", closed.QueueStatus);
        Assert.Equal(windowCloses.AddSeconds(1), closed.ClosedAt);
        Assert.Equal("IVR_CONFIRMATION_WINDOW_EXPIRED", closed.ClosedReason);

        // One final result, a window expiry nobody was reached for: Core is asked to put the order
        // in front of a person, not to expire it.
        CallResultEntity expiry = Assert.Single(await verification.CallResults
            .AsNoTracking()
            .Where(result => result.IsFinalForIvr)
            .ToListAsync());
        Assert.Equal("IVR_CONFIRMATION_WINDOW_EXPIRED", expiry.ResultType);
        Assert.Equal("WINDOW_EXPIRED_BEFORE_FINAL_RESULT", expiry.ResultReason);
        Assert.False(expiry.IsCountedCustomerAttempt);
        Assert.True(expiry.HumanReviewRequired);
        Assert.Equal("REVALIDATE_AND_HOLD_ADMIN_REVIEW", expiry.RecommendedCoreAction);
        Assert.Single(await verification.ResultCallbacks.AsNoTracking().ToListAsync());

        // Not a SIM shortage either: the job was dispatched, twice, and a channel was there both
        // times, so the sweep opens no capacity incident for it.
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        ReviewItemEntity reviewItem = Assert.Single(
            await verification.ReviewItems.AsNoTracking().ToListAsync());
        Assert.Equal("OPEN", reviewItem.Status);

        // Both refusals: uncounted attempt 1s, named as the order's data fault.
        CallAttemptEntity[] attempts = await verification.CallAttempts
            .AsNoTracking()
            .OrderBy(attempt => attempt.EndedAt)
            .ToArrayAsync();
        Assert.Equal(2, attempts.Length);
        Assert.All(attempts, attempt =>
        {
            Assert.Equal(1, attempt.AttemptNumber);
            Assert.False(attempt.IsCountedCustomerAttempt);
            Assert.Equal(SpeechRenderRejectedException.TechnicalCode, attempt.TechnicalExceptionType);
        });
        Assert.Equal("NORMALIZED_TECHNICAL_RETRY", attempts[0].Status);
        Assert.Equal("NORMALIZED_REVIEW_REQUIRED", attempts[1].Status);

        // The channel: handed back healthy both times, never counted against, never quarantined.
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        Assert.Equal("IDLE", channel.Status);
        Assert.Equal(0, channel.FailCount);
        Assert.Null(channel.FailureWindowStartedAt);
        Assert.Null(channel.QuarantineUntil);
        Assert.Null(channel.LeaseToken);
        Assert.Null(channel.ActiveCallJobId);
        Assert.Empty(sim.Events);
    }

    /// <summary>
    /// Cutting a call that is already in progress (W-0111).
    /// <para>
    /// The load-bearing assertion is not that the call stopped — it is what the attempt says
    /// afterwards. A cut customer never got to answer, so recording it as a customer outcome
    /// would spend one of their attempts on a decision the operator made. It has to land as a
    /// technical exception, uncounted, with the channel handed back.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TEL-TERMINATE-04")]
    public async Task TerminatingALiveCallReleasesTheLeaseAndRecordsAnUncountedTechnicalException()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-04", "JOB-TEL-04", "SIM-MOCK-04");
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-terminate",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                // The customer is "on the line" for two seconds while the operator decides.
                // Without a slow capture the fake answers instantly and there is no call in
                // progress left to cut.
                [lease.AttemptId] = new(
                    SimProviderDisposition.Answered,
                    "1",
                    CaptureDelay: TimeSpan.FromSeconds(2)),
            },
            timeProvider: TimeProvider.System);

        MockSchedulerDispatchGateway gateway = CreateGateway(
            CreateStore(factory),
            lease,
            sim,
            includeToken: true,
            terminationPollMilliseconds: 200);

        Task dispatch = gateway.DispatchAsync(lease);
        await WaitForActiveCallAsync(factory, lease.AttemptId);
        await RequestTerminationAsync(factory, lease.AttemptId, "operator-1", "wrong script on air");

        await Assert.ThrowsAsync<CallTerminatedException>(() => dispatch);

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();

        Assert.Equal(CallTerminatedException.TechnicalCode, attempt.TechnicalExceptionType);
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.NotNull(attempt.EndedAt);
        Assert.Equal("operator-1", attempt.TerminationRequestedBy);

        // Lease handed back and the channel usable again. A cut that left the channel pinned
        // would cost capacity for the rest of the shift.
        Assert.Equal("IDLE", channel.Status);
        Assert.Null(channel.LeaseToken);
        Assert.Null(channel.ActiveCallJobId);

        // The customer's keypress is not recorded. They pressed 1 into a call the operator had
        // already decided to end, and treating that as a confirmation is the specific mistake
        // this whole work item exists to prevent.
        Assert.Null(attempt.DtmfKey);
        Assert.Contains(sim.Events, item => item.Type == SimProviderEventType.HangupCompleted);
    }

    private static async Task WaitForActiveCallAsync(
        IDbContextFactory<IvrDbContext> factory,
        string attemptId)
    {
        for (int attemptNumber = 0; attemptNumber < 100; attemptNumber++)
        {
            await using IvrDbContext context = await factory.CreateDbContextAsync();
            bool active = await context.CallAttempts
                .AsNoTracking()
                .AnyAsync(item => item.IvrCallAttemptId == attemptId && item.ProviderCallId != null);
            if (active)
            {
                return;
            }

            await Task.Delay(50);
        }

        Assert.Fail("The dispatch loop never marked the call active.");
    }

    /// <summary>
    /// Writes what <c>InternalAdminApiService.TerminateCallAsync</c> writes. The API path has its
    /// own coverage; this test is about what the dispatch loop does once the row exists.
    /// </summary>
    private static async Task RequestTerminationAsync(
        IDbContextFactory<IvrDbContext> factory,
        string attemptId,
        string actorId,
        string reason)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await context.CallAttempts
            .SingleAsync(item => item.IvrCallAttemptId == attemptId);
        attempt.TerminationRequestedAt = DateTimeOffset.UtcNow;
        attempt.TerminationRequestedBy = actorId;
        attempt.TerminationReason = reason;
        await context.SaveChangesAsync();
    }

    [Fact]
    [Trait("TestId", "IT-TEL-HEALTH-03")]
    public async Task UnhealthyMockChannelIsQuarantinedBeforeDial()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-03", "JOB-TEL-03", "SIM-MOCK-03");
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-health-fail",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            new Dictionary<string, SimChannelHealthState>
            {
                [lease.SimChannelId] = SimChannelHealthState.Unavailable,
            },
            new FixedTimeProvider(Now));
        MockSchedulerDispatchGateway gateway = CreateGateway(
            CreateStore(factory),
            lease,
            sim,
            includeToken: true);

        MockSimOperationException error = await Assert.ThrowsAsync<MockSimOperationException>(
            () => gateway.DispatchAsync(lease));

        Assert.Equal("MOCK_CHANNEL_HEALTH_NOT_READY", error.TechnicalErrorCode);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        RawCallEventEntity rawEvent = await verification.RawCallEvents.AsNoTracking().SingleAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        Assert.Equal("QUARANTINED", channel.Status);
        Assert.Equal(1, channel.FailCount);
        Assert.Equal(Now.AddSeconds(5), channel.QuarantineUntil);
        Assert.Null(channel.LeaseToken);
        Assert.Equal("SIMERROR", rawEvent.RawCallStatus);
        Assert.Equal("MOCK_CHANNEL_HEALTH_NOT_READY", attempt.TechnicalExceptionType);
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Single(sim.Events);
        Assert.Equal(SimProviderEventType.HealthChecked, sim.Events.Single().Type);
    }

    [Fact]
    [Trait("TestId", "IT-TEL-HEALTH-WINDOW-07")]
    public async Task FailureAfterTenMinuteWindowStartsANewCounterInsteadOfAutoDisabling()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-07", "JOB-TEL-07", "SIM-MOCK-07");
        await using (IvrDbContext setup = await factory.CreateDbContextAsync())
        {
            SimChannelEntity channel = await setup.SimChannels.SingleAsync();
            channel.FailCount = 2;
            channel.FailureWindowStartedAt = Now.AddMinutes(-10).AddTicks(-1);
            await setup.SaveChangesAsync();
        }

        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-health-window-expired",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        await CreateStore(factory).FailAsync(
            lease,
            session: null,
            SimProviderDisposition.NetworkError,
            "PROVIDER_NETWORK_ERROR",
            channelHealthy: false,
            TimeSpan.FromSeconds(5));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        SimChannelEntity persisted = await verification.SimChannels.AsNoTracking().SingleAsync();
        Assert.Equal(1, persisted.FailCount);
        Assert.Equal(Now, persisted.FailureWindowStartedAt);
        Assert.Equal("QUARANTINED", persisted.Status);
    }

    [Fact]
    [Trait("TestId", "IT-TEL-HEALTH-RESET-08")]
    public async Task HealthyOutcomeClearsFailureCounterAndWindow()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-08", "JOB-TEL-08", "SIM-MOCK-08");
        await using (IvrDbContext setup = await factory.CreateDbContextAsync())
        {
            SimChannelEntity channel = await setup.SimChannels.SingleAsync();
            channel.FailCount = 2;
            channel.FailureWindowStartedAt = Now.AddMinutes(-5);
            await setup.SaveChangesAsync();
        }

        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-health-window-reset",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        await CreateStore(factory).FailAsync(
            lease,
            session: null,
            SimProviderDisposition.NetworkError,
            "PROVIDER_NETWORK_ERROR",
            channelHealthy: true,
            TimeSpan.FromSeconds(5));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        SimChannelEntity persisted = await verification.SimChannels.AsNoTracking().SingleAsync();
        Assert.Equal(0, persisted.FailCount);
        Assert.Null(persisted.FailureWindowStartedAt);
        Assert.Equal("IDLE", persisted.Status);
    }

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();

    /// <param name="at">
    /// W-0359. When the store believes it is. Omitted, it is <see cref="Now"/>, which is right for
    /// every single-dispatch test; a test that dispatches twice moves it with the claim, so the
    /// second failure is not recorded as ending before its own lease began.
    /// </param>
    private static PostgresTelephonyDispatchStore CreateStore(
        IDbContextFactory<IvrDbContext> factory,
        DateTimeOffset? at = null) => new(
        factory,
        SpeechSummaryLimits.Create(100, 100),
        Options.Create(new SchedulerOptions()),
        new FixedTimeProvider(at ?? Now));

    /// <summary>
    /// W-0359. The normaliser the worker runs, with the technical-retry limit pinned at one rather
    /// than inherited: IT-TEL-RENDER-DATA-10's whole shape - one retry, then a hold - is that number.
    /// </summary>
    private static ResultRepository CreateNormalizer(
        IDbContextFactory<IvrDbContext> factory,
        DateTimeOffset at) => new(
        factory,
        new RawEventRepository(),
        Options.Create(new SchedulerOptions { TechnicalRetryLimit = 1 }),
        new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
        new FixedTimeProvider(at));

    private static MockSchedulerDispatchGateway CreateGateway(
        ITelephonyDispatchStore store,
        SchedulerDispatchLease lease,
        FakeSimGateway sim,
        bool includeToken,
        int terminationPollMilliseconds = 500,
        ISpeechRenderer? renderer = null)
    {
        Dictionary<string, string> tokens = includeToken
            ? new Dictionary<string, string>
            {
                ["enc:mock-token"] = "mock-destination-allowlisted",
            }
            : [];
        var options = new MockTelephonyOptions
        {
            Enabled = true,
            KillSwitchEngaged = false,
            DtmfTimeoutSeconds = 10,
            CooldownSeconds = 5,
            TokenDestinations = tokens,
            DestinationAllowlist = ["mock-destination-allowlisted"],
            Scenarios = new Dictionary<string, MockSimScenarioOptions>
            {
                [lease.AttemptId] = new() { Disposition = "Answered", DtmfKey = "1" },
            },
            TerminationPollMilliseconds = terminationPollMilliseconds,
        };
        var timeProvider = new FixedTimeProvider(Now);
        IOptions<TtsProviderOptions> ttsOptions = Options.Create(new TtsProviderOptions
        {
            ExecutionMode = IvrOptions.MockExecutionMode,
            Provider = TtsProviderOptions.FakeProvider,
        });
        var usage = new TtsUsageMeter();
        var synthesis = new SpeechSynthesisService(
            new FakeDeterministicTtsProvider(ttsOptions),
            new AudioCache(timeProvider),
            new TtsRequestBudget(timeProvider),
            usage,
            new RegionalVoiceMap(ttsOptions),
            ttsOptions,
            timeProvider);
        return new MockSchedulerDispatchGateway(
            store,
            new FakeDialTokenResolver(tokens, options.DestinationAllowlist),
            renderer ?? new FakeSpeechRenderer(),
            synthesis,
            sim,
            Options.Create(options),
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }),
            new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
            timeProvider);
    }

    private static async Task SeedMockDispatchAsync(
        IDbContextFactory<IvrDbContext> factory,
        string taskId,
        string jobId,
        string channelId,
        decimal totalAmount = 1_250_000)
    {
        DateTimeOffset deadline = Now.AddMinutes(5);
        string summaryJson = JsonSerializer.Serialize(new
        {
            customer_display_name = "Anh Đạt",
            order_code_short = "DH-TEL-001",
            items = new[]
            {
                new { public_name = "Sâm lát", quantity = 2, unit_label = "hộp" },
            },
            total_amount = totalAmount,
            currency = "VND",
            delivery_area_short = "Quận 7",
            program_display_name = "Giờ Vàng",
            locale = "vi-VN",
        });
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = string.Concat("mock-telephony:", taskId),
            CorrelationId = string.Concat("corr-", taskId),
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderCode = "GF-TEL-001",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = CandidateAttemptPolicies.Version,
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = Now,
            ConfirmationWindowExpiresAt = deadline,
            PhoneRef = "phone-ref-mock-test",
            PhoneMasked = "84xxxxx0021",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:mock-token",
            DialTokenExpiresAt = deadline,
            PrivacySafeOrderSummaryJson = summaryJson,
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = Ivr.Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion,
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
            CallRestriction = false,
            CreatedAt = Now,
            ExpiresAt = deadline,
            AcceptedAt = Now,
        });
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = CandidateAttemptPolicies.Version,
            Status = "DRY_RUN",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowSeconds = 300,
            AttemptScheduleJson = JsonSerializer.Serialize(new[]
            {
                Now,
                Now.AddSeconds(150),
            }),
            T0At = Now,
            ExpiresAt = deadline,
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = "HELD_MOCK",
            ScriptVersion = string.Concat(
                "SCRIPT-ORDER-CONFIRM:",
                Ivr.Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion),
            PrivacyPolicyVersion = "privacy-v1",
            CreatedAt = Now,
        });
        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = channelId,
            SimNumberRef = string.Concat("sim-ref-", channelId),
            Enabled = true,
            Status = "IDLE",
            AdapterMode = "MOCK",
            ExecutionMode = IvrOptions.MockExecutionMode,
            ProviderName = "MOCK",
            LastHealthCheckAt = Now,
        });
        await context.SaveChangesAsync();
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
