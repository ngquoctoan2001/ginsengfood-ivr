using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Analytics;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Scripts;
using Ivr.Infrastructure.Speech;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Ivr.IntegrationTests.Governance;

/// <summary>
/// W-0354 / B11 step (0) (chief worklist 2026-09-25). Since option B (W-0310) Module 3 may send a
/// customer's number in the clear and IVR stores it in <c>ivr_confirmation_tasks.phone_e164</c>.
/// The promise made to Module 3 (IR-07, correction to <c>A-6</c>) is narrower than "the number is
/// stored": log lines, callbacks and the admin API carry only <c>phone_masked</c>, and the number is
/// used in exactly one place. Until now that was checked by hand, once, on the chief's smoke server.
/// <para>
/// This runs a number-only task through intake, a MOCK dial, normalisation, the callback outbox
/// and the analytics ETL on real PostgreSQL, reads it back through the admin and internal APIs, and
/// then looks for the number everywhere it could have gone: every text cell of every table, every
/// response, every log line the API wrote. Since W-0361 / K-40 MOCK does not keep the number at all,
/// so there is no place it is allowed to be. The check holds whichever way the owner decides option
/// B (phiếu Sếp 25/09, mục B2), which is why it does not wait for that answer.
/// </para>
/// <para>
/// W-0361 / K-41 widened it: the search is for the national number <c>900000001</c>, which every
/// spelling contains; the full guard runs over every text cell, for the reformatted spellings a
/// plain search misses; the warehouse, the SIM, script, integration and feature-flag reads and the
/// worker's own call-job read are searched too; and the log recorder keeps whole exceptions.
/// The HTTP intake surface is IT-PHONE-CONTAIN-02 in <c>TaskIntakeApiTests</c>. The production dial
/// path is IT-PHONE-CONTAIN-03 (<c>Q-28.2</c>).
/// </para>
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class PhoneNumberContainmentTests(PostgresPersistenceFixture fixture)
{
    // A number of the contract's shape; MOCK never dials it. Searched as the national number,
    // without the '+' or the country code, so a copy that lost either, or gained a leading zero,
    // is still found.
    private const string SentNumber = "+84900000001";
    private const string Nsn = "900000001";
    private const string TaskIdValue = "TASK-PHONE-CONTAIN";
    private const string ChannelId = "SIM-MOCK-CONTAIN";


    [Fact]
    [Trait("TestId", "IT-PHONE-CONTAIN-01")]
    public async Task ANumberSentByModule3IsNeitherKeptNorRepeatedAnywhereInMock()
    {
        // The admin API evaluates eligibility on the real clock, so the whole run is anchored to
        // it; the calling window is opened to the whole day so the test does not depend on the hour.
        DateTimeOffset now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAndChannelAsync(factory, now);
        var clock = new FixedTimeProvider(now);
        await using InternalAdminApiTestApplication app =
            await InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

        // 1. Intake, number-only, the shape Module 3 will send.
        TaskIntakeOutcome outcome = await CreateIntake(factory, clock).IntakeAsync(
            new TaskIntakeCommand(
                CreateTask(now),
                "idem-phone-contain",
                "corr-phone-contain",
                new string('C', 64),
                ExecutionMode.Mock));
        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);

        // 2. Eligibility, the way the worker asks for it: through the internal API.
        using (HttpRequestMessage eligibility = InternalRequest(
            HttpMethod.Post, "/v1/ivr/order-confirmation/eligibility-checks"))
        {
            eligibility.Headers.Add("Idempotency-Key", "idem-phone-contain-eligibility");
            eligibility.Content = JsonContent.Create(new Ivr.Api.Internal.EligibilityLifecycleRequest(TaskIdValue));
            using HttpResponseMessage answered = await app.Client.SendAsync(eligibility);
            string body = await answered.Content.ReadAsStringAsync();
            Assert.True(answered.StatusCode == HttpStatusCode.OK, $"eligibility answered {(int)answered.StatusCode}: {body}");
            Assert.DoesNotContain(Nsn, body, StringComparison.Ordinal);
        }

        // 3. A MOCK dial of it, answered with "1".
        SchedulerDispatchLease? claimed = await new PostgresSchedulerStore(factory, clock).TryClaimDueDispatchAsync(
            "worker-phone-contain",
            IvrOptions.MockExecutionMode,
            TimeSpan.FromMinutes(2));
        if (claimed is null)
        {
            await using IvrDbContext diagnosis = await factory.CreateDbContextAsync();
            CallJobEntity job = await diagnosis.CallJobs.AsNoTracking().SingleAsync();
            Assert.Fail($"nothing to claim: job status={job.Status} queue={job.QueueStatus} eligible={job.Eligible} "
                + $"decision={job.EligibilityDecision} schedule={job.AttemptScheduleJson} expires={job.ExpiresAt:O} now={now:O}");
        }

        SchedulerDispatchLease lease = claimed!;
        await CreateGateway(factory, clock, lease).DispatchAsync(lease);

        // 4. Normalisation, which writes the result and puts the callback on the outbox.
        await new ResultRepository(
            factory,
            new RawEventRepository(),
            Options.Create(new SchedulerOptions { TechnicalRetryLimit = 1 }),
            new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
            new FixedTimeProvider(now.AddSeconds(30)))
            .NormalizeNextAsync("normalizer-phone-contain");

        string jobId;
        await using (IvrDbContext context = await factory.CreateDbContextAsync())
        {
            Assert.Equal("IVR_CONFIRMED", (await context.CallResults.AsNoTracking().SingleAsync()).ResultType);
            Assert.NotNull(await context.ResultCallbacks.AsNoTracking().SingleOrDefaultAsync());
            jobId = (await context.CallJobs.AsNoTracking().SingleAsync()).IvrCallJobId;
        }

        // 5. W-0361 / K-41. The analytics warehouse, filled the way the worker fills it, so its
        //    tables are searched with this order in them rather than empty.
        AnalyticsEtlRunReport etl = await new AnalyticsEtlJob(factory, new FixedTimeProvider(now.AddMinutes(1)))
            .RunAsync(
                new AnalyticsEtlRunOptions { RebuildAggregates = true, Now = now.AddMinutes(1) },
                CancellationToken.None);
        Assert.True(etl.LoadedRows > 0, $"the ETL loaded nothing: {etl}");

        // 6. Every surface an operator or the worker would open for this order.
        string[] routes =
        [
            $"/v1/ivr/order-confirmation/call-jobs/{jobId}/detail",
            "/v1/ivr/order-confirmation/call-jobs",
            "/v1/ivr/order-confirmation/queue",
            "/v1/ivr/order-confirmation/dashboard",
            "/v1/ivr/order-confirmation/review-items",
            "/v1/ivr/order-confirmation/sim-channels",
            "/v1/ivr/order-confirmation/scripts",
            "/v1/ivr/order-confirmation/integration-status",
            "/v1/ivr/order-confirmation/feature-flags/dev",
            "/v1/ivr/order-confirmation/audit-evidence"
                + $"?target_type=confirmation-task&target_id={TaskIdValue}"
                + $"&reason={Uri.EscapeDataString("phone containment check")}",
        ];
        foreach (string route in routes)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, route);
            TestAdminTokens.AuthorizeForPermission(request, IvrPermissions.QueueView, "operator-contain");
            request.Headers.Add("X-Correlation-Id", "corr-phone-contain-read");
            using HttpResponseMessage response = await app.Client.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"{route} answered {(int)response.StatusCode}: {body}");
            Assert.DoesNotContain(Nsn, body, StringComparison.Ordinal);
        }

        using (HttpRequestMessage internalRead = InternalRequest(
            HttpMethod.Get, $"/v1/ivr/order-confirmation/call-jobs/{jobId}"))
        {
            using HttpResponseMessage response = await app.Client.SendAsync(internalRead);
            string body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"internal call-job read answered {(int)response.StatusCode}: {body}");
            Assert.DoesNotContain(Nsn, body, StringComparison.Ordinal);
        }

        // 7. Every text cell in the database, after all of the above has written what it writes.
        //    Since W-0361 / K-40 MOCK keeps no copy at all, so the number is in no column, and no
        //    cell holds anything the guard reads as a phone number or an address either - which
        //    catches a copy re-spaced or re-punctuated past the plain search.
        Assert.Empty(await ColumnsHoldingAsync(Nsn));
        Assert.Empty(await CellsTheGuardRefusesAsync());

        // 8. Everything the API logged, exception text included.
        Assert.DoesNotContain(
            app.Logs.Entries,
            entry => entry.Contains(Nsn, StringComparison.Ordinal));
    }

    /// <summary>
    /// Q-28.2 (W-0365, 2026-09-26). On the production dial path the number is used at the carrier
    /// edge and nowhere before it. The vault resolves a number Module 3 sent (option B) and writes
    /// its resolve audit; the dispatch gate's production gates are asked with what the gateway
    /// shows them - the pilot fingerprint - and answer from their approval rows. The SIP
    /// destination handed to the dial carries the number; the gate reference, every text cell of
    /// every table and everything the guard reads as a telephone number do not.
    /// <para>
    /// Stops short of a dial: the Asterisk gateway stays closed to PRODUCTION_REAL until SIP-04
    /// (UT-TRUNK-DI-05), so the gateway's own step - passing <c>GateReference</c>, never the
    /// destination, to the gate - is pinned by the unit tests of the production branch.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-PHONE-CONTAIN-03")]
    public async Task TheProductionDialPathShowsTheNumberOnlyToTheCarrierEdge()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        byte[] key = [.. Enumerable.Range(1, 32).Select(value => (byte)value)];
        string fingerprint = ProductionPilotFingerprint.Of(key, Nsn);
        var trunk = new SipTrunkOptions
        {
            Enabled = true,
            ExecutionMode = ExecutionModes.ProductionReal,
            ProviderName = "CARRIER",
            Environment = FeatureFlagEnvironments.Production,
            TrunkEndpoint = "carrier-trunk",
            CarrierSipHost = "sip.carrier.example.vn",
            OutboundCallerId = "02873001234",
            NumberFormat = SipTrunkNumberFormat.E164Plus,
            ContractedChannels = 8,
            MaxCallStartsPerSecond = 2,
            PilotFingerprintKey = Convert.ToBase64String(key),
            PilotDestinations = [fingerprint],
        };
        Assert.True(new SipTrunkOptionsValidator().Validate(null, trunk).Succeeded);

        // The release and the pilot list approved, as the runbook has an operator insert them.
        await using (IvrDbContext approvals = await factory.CreateDbContextAsync())
        {
            await approvals.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO ivr_runtime_gate_approvals (
                    approval_reference, approval_kind, environment, proposer_actor_id,
                    approver_actor_id, change_fingerprint, reason, signed_decision_ref,
                    granted_at, expires_at, revoked_at, revoked_reason, correlation_id)
                VALUES
                    ('contain-release', {RuntimeGateApprovalKinds.ProductionCall}, 'prod', 'operator-1',
                     'operator-2', NULL, 'containment release', 'OD-V1-12@2026-09-15', now(),
                     NULL, NULL, NULL, 'corr-phone-contain'),
                    ('contain-pilot', {RuntimeGateApprovalKinds.ProductionPilotList}, 'prod', 'operator-1',
                     'operator-2', {ProductionPilotFingerprint.ListHash([fingerprint])},
                     'containment pilot list', 'Q-28@2026-09-26', now(), NULL, NULL, NULL,
                     'corr-phone-contain')
                """);
        }

        // 1. The vault, with the durable ledger and the real audit logger.
        var vault = new ProductionDialTokenVault(
            Options.Create(trunk),
            new UnavailableOpaqueValueProtector(),
            new PostgresDialTokenResolveLedger(factory),
            fixture.Services.GetRequiredService<IAuditLogger>());
        DialAuthorization authorization = await vault.ResolveAsync(
            new DialTokenResolutionRequest(
                DialTokenReference.Create("dial-token-contain-prod", now.AddMinutes(30)),
                AttemptId.Create("ATTEMPT-CONTAIN-PROD"),
                TaskId.Create(TaskIdValue),
                3,
                SentNumber),
            now,
            CancellationToken.None);

        // 2. The production gates, shown what the gateway shows them.
        Assert.Equal(fingerprint, authorization.GateReference);
        Assert.DoesNotContain(Nsn, authorization.GateReference, StringComparison.Ordinal);
        Assert.True(await new PostgresProductionCallGate(factory, TimeProvider.System)
            .IsApprovedAsync(FeatureFlagEnvironments.Production));
        Assert.True((await new PostgresProductionPilotGate(factory, TimeProvider.System, Options.Create(trunk))
            .EvaluateAsync(FeatureFlagEnvironments.Production, authorization.GateReference)).Allowed);

        // 3. The carrier edge holds the number, and nothing else does.
        Assert.Equal("sip:+84900000001@sip.carrier.example.vn", authorization.RevealToTrustedGateway());
        Assert.Empty(await ColumnsHoldingAsync(Nsn));
        Assert.Empty(await CellsTheGuardRefusesAsync());
    }

    /// <summary>
    /// The worker's side of the internal API: its token, source system, scope and a correlation id.
    /// </summary>
    private static HttpRequestMessage InternalRequest(HttpMethod method, string route)
    {
        HttpRequestMessage request = new(method, route);
        request.Headers.Authorization = new("Bearer", InternalAdminApiTestApplication.InternalToken);
        request.Headers.Add("X-Source-System", "ivr-worker");
        request.Headers.Add("X-Service-Scope", Ivr.Api.Internal.InternalServiceOptions.RequiredScope);
        request.Headers.Add("X-Correlation-Id", "corr-phone-contain");
        return request;
    }

    /// <summary>
    /// Every text-like column in the IVR schemas that holds the value, as <c>schema.table.column</c>.
    /// Read from the catalogue rather than listed, so a table added next month is searched too.
    /// </summary>
    private async Task<IReadOnlyList<string>> ColumnsHoldingAsync(string value)
    {
        var holders = new List<string>();
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        foreach ((string schema, string table, string column) in await TextColumnsAsync(connection))
        {
            await using var probe = new NpgsqlCommand(
                $"SELECT COUNT(*) FROM \"{schema}\".\"{table}\" WHERE \"{column}\"::text LIKE @pattern",
                connection);
            probe.Parameters.AddWithValue("pattern", string.Concat("%", value, "%"));
            long count = (long)(await probe.ExecuteScalarAsync())!;
            if (count > 0)
            {
                holders.Add($"{schema}.{table}.{column}");
            }
        }

        return holders;
    }

    /// <summary>
    /// W-0361 / K-41. Every text cell the full guard refuses, as <c>schema.table.column</c>. The plain
    /// search above finds the digits as sent; this finds the shapes the guard knows - grouped, dotted,
    /// with a leading zero - which is how a copy that was reformatted on the way would look.
    /// </summary>
    private async Task<IReadOnlyList<string>> CellsTheGuardRefusesAsync()
    {
        var refused = new List<string>();
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        foreach ((string schema, string table, string column) in await TextColumnsAsync(connection))
        {
            await using var read = new NpgsqlCommand(
                $"SELECT \"{column}\"::text FROM \"{schema}\".\"{table}\" WHERE \"{column}\" IS NOT NULL",
                connection);
            await using NpgsqlDataReader cells = await read.ExecuteReaderAsync();
            while (await cells.ReadAsync())
            {
                if (!PiiGuard.IsSafeText(cells.GetString(0)))
                {
                    refused.Add($"{schema}.{table}.{column}");
                    break;
                }
            }
        }

        return refused;
    }

    private static async Task<IReadOnlyList<(string Schema, string Table, string Column)>> TextColumnsAsync(
        NpgsqlConnection connection)
    {
        var columns = new List<(string Schema, string Table, string Column)>();
        await using (var catalogue = new NpgsqlCommand(
            """
            SELECT c.table_schema, c.table_name, c.column_name
            FROM information_schema.columns c
            JOIN information_schema.tables t
              ON t.table_schema = c.table_schema AND t.table_name = c.table_name
            WHERE t.table_type = 'BASE TABLE'
              AND c.table_schema NOT IN ('pg_catalog', 'information_schema')
              AND c.data_type IN ('text', 'character varying', 'character', 'json', 'jsonb')
            ORDER BY 1, 2, 3
            """,
            connection))
        await using (NpgsqlDataReader reader = await catalogue.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                columns.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
            }
        }

        Assert.NotEmpty(columns);
        return columns;
    }

    private static TaskIntakeService CreateIntake(
        IDbContextFactory<IvrDbContext> factory,
        TimeProvider clock) =>
        new(
            new PostgresTaskIntakeStore(factory, clock),
            new PostgresAttemptPolicyRegistry(factory),
            new PostgresScriptRegistry(factory, clock, Options.Create(new ScriptContentOptions())),
            // Production's protector: none. A number-only task is the one shape that intake then
            // still accepts, which is the case this test exists for.
            new UnavailableOpaqueValueProtector(),
            SpeechSummaryLimits.Create(100, 100),
            clock,
            new CallingWindow(Options.Create(new CallingWindowOptions
            {
                Enabled = true,
                StartMinuteOfLocalDay = 0,
                EndMinuteOfLocalDay = 1440,
            })),
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SalesProvider = "FAKE_TARGET_V1",
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }));

    private static MockSchedulerDispatchGateway CreateGateway(
        IDbContextFactory<IvrDbContext> factory,
        TimeProvider clock,
        SchedulerDispatchLease lease)
    {
        Dictionary<string, string> tokens = new()
        {
            ["*"] = "mock-destination-allowlisted",
        };
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
        };
        IOptions<TtsProviderOptions> tts = Options.Create(new TtsProviderOptions
        {
            ExecutionMode = IvrOptions.MockExecutionMode,
            Provider = TtsProviderOptions.FakeProvider,
        });
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            new Dictionary<string, SimChannelHealthState>(),
            clock);
        return new MockSchedulerDispatchGateway(
            new PostgresTelephonyDispatchStore(
                factory,
                SpeechSummaryLimits.Create(100, 100),
                Options.Create(new SchedulerOptions()),
                clock),
            new FakeDialTokenResolver(tokens, options.DestinationAllowlist),
            new FakeSpeechRenderer(),
            new SpeechSynthesisService(
                new FakeDeterministicTtsProvider(tts),
                new AudioCache(clock),
                new TtsRequestBudget(clock),
                new TtsUsageMeter(),
                new RegionalVoiceMap(tts),
                tts,
                clock),
            sim,
            Options.Create(options),
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }),
            new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
            clock);
    }

    private static IvrConfirmationTaskV1 CreateTask(DateTimeOffset now)
    {
        DateTimeOffset start = now.AddMinutes(-1);
        return new IvrConfirmationTaskV1
        {
            Contract_version = IvrConfirmationTaskV1Contract_version.IvrOrderConfirmation_v1,
            Task_id = TaskIdValue,
            Correlation_id = "corr-phone-contain",
            Created_at = start,
            Order_id = "ORDER-PHONE-CONTAIN",
            Order_code = "GF-CONTAIN-001",
            Order_code_short = "CT001",
            Order_version = "17",
            Order_state = "CONFIRMING",
            Payment_method_snapshot = IvrConfirmationTaskV1Payment_method_snapshot.ONLINE,
            Ivr_confirmation_required = true,
            Is_ivr_callable = true,
            Program_code = ProgramCode.GOLDEN_HOUR,
            Confirmation_window_started_at = start,
            Confirmation_window_expires_at = start.AddMinutes(5),
            Attempt_policy_version = CandidateAttemptPolicies.Version,
            Max_customer_attempts = 2,
            Attempt_offsets_seconds = [0, 150],
            Phone_ref = "phone-ref-contain",
            Phone_masked = "84xxxxx0001",
            Phone_validation_status = IvrConfirmationTaskV1Phone_validation_status.VALID,
            Phone_e164 = SentNumber,
            Privacy_safe_order_summary = new Ivr.Contracts.Generated.IvrServer.V1.PrivacySafeOrderSummary
            {
                Customer_display_name = "chị An",
                Order_code_short = "CT001",
                Items =
                [
                    new OrderSpeechItem
                    {
                        Public_name = "Nước hồng sâm",
                        Quantity = 2,
                        Unit_label = "hộp",
                    },
                ],
                Total_amount = 560_000,
                Currency = PrivacySafeOrderSummaryCurrency.VND,
                Delivery_area_short = "Phường Bến Nghé, Quận Một",
                Program_display_name = "Giờ Vàng",
                Locale = PrivacySafeOrderSummaryLocale.ViVN,
            },
            Call_restriction = false,
            // The shape the local E2E harness sends: eligibility holds for admin review without a
            // source version and a capture time inside the window.
            Eligibility_snapshot = new
            {
                decision = "ELIGIBLE",
                source_version = "sales-eligibility-v1",
                captured_at = start,
                source_available = true,
                blockers = Array.Empty<string>(),
            },
            Evidence_ref = "evidence://phone-containment",
        };
    }

    private static async Task SeedPoliciesAndChannelAsync(
        IDbContextFactory<IvrDbContext> factory,
        DateTimeOffset now)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.AttemptPolicies.AddRange(
            new AttemptPolicyEntity
            {
                PolicyVersion = CandidateAttemptPolicies.Version,
                ProgramType = "GOLDEN_HOUR",
                MaxAttempts = 2,
                AttemptOffsetsSecondsJson = "[0,150]",
                ConfirmationWindowSeconds = 300,
                AllowedExecutionModesJson = "[\"MOCK\",\"LAB_REAL_SIM\"]",
                ApprovedForProduction = false,
                CreatedAt = now,
            },
            new AttemptPolicyEntity
            {
                PolicyVersion = CandidateAttemptPolicies.Version,
                ProgramType = "TWENTY_FOUR_SEVEN",
                MaxAttempts = 2,
                AttemptOffsetsSecondsJson = "[0,450]",
                ConfirmationWindowSeconds = 900,
                AllowedExecutionModesJson = "[\"MOCK\",\"LAB_REAL_SIM\"]",
                ApprovedForProduction = false,
                CreatedAt = now,
            });
        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = ChannelId,
            SimNumberRef = string.Concat("sim-ref-", ChannelId),
            Enabled = true,
            Status = "IDLE",
            AdapterMode = "MOCK",
            ExecutionMode = IvrOptions.MockExecutionMode,
            ProviderName = "MOCK",
            LastHealthCheckAt = now,
        });
        await context.SaveChangesAsync();
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
