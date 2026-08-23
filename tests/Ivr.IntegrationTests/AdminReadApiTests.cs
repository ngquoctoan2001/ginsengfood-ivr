using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IvrServer = Ivr.Contracts.Generated.IvrServer.V1;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0095 — read-only admin projections consumed by the P3-2 console.
///
/// Responses are deserialised into the OpenAPI-generated DTOs rather than into
/// ad-hoc shapes, so a contract drift between the runtime projection and the
/// committed schema fails these tests instead of surfacing in the browser.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class AdminReadApiTests(PostgresPersistenceFixture fixture)
{
    private const string GoldenHourJob = "JOB-READ-GH";
    private const string TwentyFourSevenJob = "JOB-READ-247";
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-01")]
    public async Task ReadRoutesAreMappedAndGatedByQueueViewPermission()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        string[] expected =
        [
            "GET /v1/ivr/order-confirmation/dashboard",
            "GET /v1/ivr/order-confirmation/call-jobs",
            "GET /v1/ivr/order-confirmation/call-jobs/{ivrCallJobId}/detail",
        ];
        string[] actual = app.Services.GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .SelectMany(endpoint => endpoint.Metadata.GetMetadata<HttpMethodMetadata>()!
                .HttpMethods.Select(method =>
                    string.Concat(method, " ", endpoint.RoutePattern.RawText)))
            .Where(route => expected.Contains(route, StringComparer.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expected.Order(StringComparer.Ordinal), actual);

        // An actor holding only an unrelated permission must be refused by the
        // server, whatever the console chose to render.
        foreach (string route in new[]
        {
            "/v1/ivr/order-confirmation/dashboard",
            "/v1/ivr/order-confirmation/call-jobs",
            $"/v1/ivr/order-confirmation/call-jobs/{GoldenHourJob}/detail",
        })
        {
            using HttpResponseMessage forbidden = await SendAdminAsync(
                app,
                route,
                IvrPermissions.ManualRetry);
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
            using JsonDocument envelope = JsonDocument.Parse(
                await forbidden.Content.ReadAsStringAsync());
            Assert.Equal(
                IvrErrorCodes.ForbiddenCaller,
                envelope.RootElement.GetProperty("error").GetProperty("code").GetString());
        }
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-02")]
    public async Task DashboardComputesQueueResultAttemptAndSimAggregatesServerSide()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAdminAsync(
            app,
            "/v1/ivr/order-confirmation/dashboard",
            IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IvrServer.IvrDashboardProjection dashboard =
            (await response.Content.ReadFromJsonAsync<IvrServer.IvrDashboardProjection>())!;

        Assert.Equal(IvrOptions.MockExecutionMode, dashboard.Execution_mode);
        Assert.False(dashboard.Real_customer_call_allowed);

        Assert.Equal(1, dashboard.Queue.Queued);
        Assert.Equal(1, dashboard.Queue.Held_mock);
        Assert.Equal(2, dashboard.Queue.Open_total);
        Assert.Equal(0, dashboard.Queue.Closed_total);
        Assert.Equal(1, dashboard.Queue.Near_expiry);
        Assert.False(dashboard.Queue.Paused);

        // Two results, one confirmed and one final no-answer.
        Assert.Equal(2, dashboard.Results.Total);
        Assert.Equal(0.5d, dashboard.Results.Confirm_rate);
        Assert.Equal(0.5d, dashboard.Results.No_answer_rate);
        Assert.Equal(0d, dashboard.Results.Cancel_rate);
        Assert.Equal(0d, dashboard.Results.Technical_exception_rate);
        Assert.Equal(1, dashboard.Results.By_result_type["IVR_CONFIRMED"]);
        Assert.Equal(1, dashboard.Results.By_result_type["IVR_NO_ANSWER_FINAL"]);

        Assert.Equal(3, dashboard.Attempts.Total);
        Assert.Equal(1, dashboard.Attempts.Counted_customer_attempts);
        Assert.Equal(1, dashboard.Attempts.Technical_retries);

        Assert.Equal(2, dashboard.Sim.Total);
        Assert.Equal(1, dashboard.Sim.Enabled);
        Assert.Equal(1, dashboard.Sim.Idle);
        Assert.Equal(1, dashboard.Sim.Disabled);
        Assert.Equal("MOCK", dashboard.Sim.Adapter_mode);

        Assert.Single(dashboard.Open_incidents);
        Assert.Equal("SCHEDULER_DEADLINE", dashboard.Open_incidents.Single().Scope);
        Assert.Equal(3, dashboard.Missed_deadline_count);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-03")]
    public async Task DashboardHonoursProgramFilterAndRejectsAnUnknownProgram()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage filtered = await SendAdminAsync(
            app,
            "/v1/ivr/order-confirmation/dashboard?program=GOLDEN_HOUR",
            IvrPermissions.QueueView);
        IvrServer.IvrDashboardProjection dashboard =
            (await filtered.Content.ReadFromJsonAsync<IvrServer.IvrDashboardProjection>())!;

        Assert.Equal(1, dashboard.Queue.Open_total);
        Assert.Equal(1, dashboard.Results.Total);
        Assert.Equal(1d, dashboard.Results.Confirm_rate);
        // The SIM pool is machine-wide state and must ignore the program filter.
        Assert.Equal(2, dashboard.Sim.Total);

        using HttpResponseMessage rejected = await SendAdminAsync(
            app,
            "/v1/ivr/order-confirmation/dashboard?program=NOT_A_PROGRAM",
            IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-04")]
    public async Task CallJobListReturnsMaskedRowsWithPagingMetadata()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAdminAsync(
            app,
            "/v1/ivr/order-confirmation/call-jobs?page=1&page_size=1",
            IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string payload = await response.Content.ReadAsStringAsync();
        IvrServer.IvrCallJobPage page =
            JsonSerializer.Deserialize<IvrServer.IvrCallJobPage>(payload)!;

        Assert.Equal(1, page.Page);
        Assert.Equal(1, page.Page_size);
        Assert.Equal(2, page.Total_count);
        IvrServer.IvrCallJobListItem item = Assert.Single(page.Items);
        Assert.Equal("GF-247", item.Order_code_short);
        Assert.Equal("84xxxxx0247", item.Phone_masked);
        Assert.Equal(1, item.Attempt_count);
        Assert.Equal(2, item.Max_attempts);
        Assert.Equal("IVR_NO_ANSWER_FINAL", item.Result_type);
        Assert.False(item.Near_expiry);

        // The full order code is a filter input only; it must never be echoed.
        Assert.DoesNotContain("GF-ORDER-247-FULL", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("GF-ORDER-GH-FULL", payload, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-05")]
    public async Task CallJobListAppliesEveryDocumentedFilter()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        await AssertSingleJobAsync(
            app,
            "/v1/ivr/order-confirmation/call-jobs?order_code=GF-ORDER-GH-FULL",
            GoldenHourJob);
        await AssertSingleJobAsync(
            app,
            "/v1/ivr/order-confirmation/call-jobs?correlation_id=corr-read-gh",
            GoldenHourJob);
        await AssertSingleJobAsync(
            app,
            "/v1/ivr/order-confirmation/call-jobs?result_type=IVR_CONFIRMED",
            GoldenHourJob);
        await AssertSingleJobAsync(
            app,
            "/v1/ivr/order-confirmation/call-jobs?queue_status=HELD_MOCK",
            TwentyFourSevenJob);
        await AssertSingleJobAsync(
            app,
            "/v1/ivr/order-confirmation/call-jobs?program=TWENTY_FOUR_SEVEN",
            TwentyFourSevenJob);
        await AssertSingleJobAsync(
            app,
            "/v1/ivr/order-confirmation/call-jobs?near_expiry=true",
            GoldenHourJob);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-06")]
    public async Task DetailReturnsTheFullTraceIncludingIdsTheAdminMutationsNeed()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAdminAsync(
            app,
            $"/v1/ivr/order-confirmation/call-jobs/{GoldenHourJob}/detail",
            IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IvrServer.IvrCallJobDetail detail =
            (await response.Content.ReadFromJsonAsync<IvrServer.IvrCallJobDetail>())!;

        Assert.Equal(GoldenHourJob, detail.Ivr_call_job_id);
        Assert.Equal("GF-GH", detail.Order_code_short);
        Assert.Equal("84xxxxx0065", detail.Phone_masked);
        Assert.Equal("CONFIRMING", detail.Order_state);
        Assert.Equal("corr-read-gh", detail.Correlation_id);
        Assert.True(detail.Input_signal_only);
        Assert.True(detail.No_direct_order_update);

        Assert.Equal(2, detail.Attempts.Count);
        IvrServer.IvrCallAttemptDetail technicalAttempt = detail.Attempts.First();
        Assert.Equal(1, technicalAttempt.Attempt_number);
        Assert.Equal("MOCK_ADAPTER_FAULT", technicalAttempt.Technical_exception_type);
        // DT-02: a technical failure is never charged to the customer.
        Assert.False(technicalAttempt.Is_counted_customer_attempt);

        IvrServer.IvrCallAttemptDetail confirmedAttempt = detail.Attempts.Last();
        Assert.Equal(2, confirmedAttempt.Attempt_number);
        Assert.Equal("1", confirmedAttempt.Dtmf_key);
        Assert.Equal("CONFIRMED", confirmedAttempt.Disposition);
        Assert.True(confirmedAttempt.Is_counted_customer_attempt);

        IvrServer.IvrCallResultDetail result = Assert.Single(detail.Results);
        Assert.Equal("IVR_CONFIRMED", result.Result_type);
        Assert.True(result.Is_final_for_ivr);

        IvrServer.IvrResultCallbackDetail callback = Assert.Single(detail.Callbacks);
        Assert.Equal(200, callback.Core_http_status);
        Assert.Equal("ACCEPTED", callback.Core_response_code);

        // Without these two ids the P2-8 retry and review operations are
        // unreachable from any browser-facing surface.
        Assert.Equal("TECH-READ-GH", Assert.Single(detail.Technical_exceptions).Technical_exception_id);
        Assert.Equal("REVIEW-READ-GH", Assert.Single(detail.Review_items).Review_item_id);

        Assert.Contains("evidence://ivr/read/task", detail.Evidence_refs);
        Assert.Contains("evidence://ivr/read/result", detail.Evidence_refs);
        Assert.Contains("audit://ivr/read/result", detail.Audit_refs);
        Assert.Contains("DO_NOT_CALL", detail.Blocked_reasons);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-07")]
    public async Task DetailReturnsNotFoundForAnUnknownJob()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAdminAsync(
            app,
            "/v1/ivr/order-confirmation/call-jobs/JOB-DOES-NOT-EXIST/detail",
            IvrPermissions.QueueView);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument envelope = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            IvrErrorCodes.NotFound,
            envelope.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-10")]
    public async Task DetailDerivesTheVoiceRegionWithoutEverExposingTheDeliveryArea()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        // Asserted on the raw wire payload rather than a generated DTO: voice_region is a new
        // response field and the generated client is only refreshed by the nswag codegen step.
        using HttpResponseMessage southResponse = await SendAdminAsync(
            app,
            $"/v1/ivr/order-confirmation/call-jobs/{GoldenHourJob}/detail",
            IvrPermissions.QueueView);
        string southPayload = await southResponse.Content.ReadAsStringAsync();
        using JsonDocument south = JsonDocument.Parse(southPayload);

        using HttpResponseMessage northResponse = await SendAdminAsync(
            app,
            $"/v1/ivr/order-confirmation/call-jobs/{TwentyFourSevenJob}/detail",
            IvrPermissions.QueueView);
        string northPayload = await northResponse.Content.ReadAsStringAsync();
        using JsonDocument north = JsonDocument.Parse(northPayload);

        // Vĩnh Long absorbed Bến Tre in 2025 and is Southern; Hà Nội is Northern. Two jobs,
        // two regions — a single-region result would pass a weaker assertion while regional
        // routing was in fact broken.
        Assert.Equal(
            "South",
            south.RootElement.GetProperty("voice_region").GetString());
        Assert.Equal(
            "North",
            north.RootElement.GetProperty("voice_region").GetString());

        // The console gets the three-value region and nothing more. Putting the ward and
        // province on an admin screen would be a privacy expansion needing its own review.
        Assert.DoesNotContain("Phú Khương", southPayload, StringComparison.Ordinal);
        Assert.DoesNotContain("Vĩnh Long", southPayload, StringComparison.Ordinal);
        Assert.DoesNotContain("delivery_area_short", southPayload, StringComparison.Ordinal);
        Assert.DoesNotContain("Cửa Nam", northPayload, StringComparison.Ordinal);
    }

    /// <summary>
    /// W-0113. The recorded voice wins over the derived one, and the response says which it is.
    /// <para>
    /// The seeded attempts carry no voice, so this job reads as DERIVED — which is the honest
    /// answer for every call made before the columns existed. Then one attempt is given a
    /// recorded voice that deliberately DISAGREES with what the delivery area would derive, and
    /// the recorded value is the one that comes back. Choosing a disagreeing value is the whole
    /// test: a matching one would pass whether or not the read path had changed at all.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-11")]
    public async Task DetailPrefersTheRecordedVoiceAndSaysWhereTheRegionCameFrom()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using (HttpResponseMessage derivedResponse = await SendAdminAsync(
            app,
            $"/v1/ivr/order-confirmation/call-jobs/{GoldenHourJob}/detail",
            IvrPermissions.QueueView))
        {
            using JsonDocument derived = JsonDocument.Parse(
                await derivedResponse.Content.ReadAsStringAsync());

            // Vĩnh Long is Southern, and nothing recorded a voice, so the answer is derived.
            Assert.Equal("South", derived.RootElement.GetProperty("voice_region").GetString());
            Assert.Equal(
                "DERIVED",
                derived.RootElement.GetProperty("voice_region_source").GetString());
            foreach (JsonElement attempt in derived.RootElement.GetProperty("attempts").EnumerateArray())
            {
                Assert.Equal(JsonValueKind.Null, attempt.GetProperty("voice_region").ValueKind);
            }
        }

        // Now record a voice on the LAST attempt, and record a region the delivery area would
        // never produce. A config change between the call and this read is exactly how the two
        // come to disagree in real life.
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using (IvrDbContext context = await factory.CreateDbContextAsync())
        {
            CallAttemptEntity attempt = await context.CallAttempts
                .SingleAsync(row => row.IvrCallAttemptId == "ATTEMPT-READ-GH-2");
            attempt.VoiceId = "voice-central-a";
            attempt.VoiceRegion = "Central";
            attempt.VoiceRegionResolved = true;
            await context.SaveChangesAsync();
        }

        using HttpResponseMessage recordedResponse = await SendAdminAsync(
            app,
            $"/v1/ivr/order-confirmation/call-jobs/{GoldenHourJob}/detail",
            IvrPermissions.QueueView);
        string payload = await recordedResponse.Content.ReadAsStringAsync();
        using JsonDocument recorded = JsonDocument.Parse(payload);

        Assert.Equal("Central", recorded.RootElement.GetProperty("voice_region").GetString());
        Assert.Equal(
            "RECORDED",
            recorded.RootElement.GetProperty("voice_region_source").GetString());

        // The per-attempt rows keep their own answers. Attempt 1 recorded nothing and still
        // says nothing — it is not back-filled from attempt 2, because two attempts of one job
        // can genuinely have gone out in different voices.
        JsonElement attempts = recorded.RootElement.GetProperty("attempts");
        JsonElement first = attempts[0];
        JsonElement second = attempts[1];
        Assert.Equal(JsonValueKind.Null, first.GetProperty("voice_region").ValueKind);
        Assert.Equal("Central", second.GetProperty("voice_region").GetString());
        Assert.Equal("voice-central-a", second.GetProperty("voice_id").GetString());
        Assert.True(second.GetProperty("voice_region_resolved").GetBoolean());

        // Still no delivery area on the wire, recorded or derived.
        Assert.DoesNotContain("Phú Khương", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("delivery_area_short", payload, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-09")]
    public async Task DashboardAndDetailCarryTheTilesTheUiSpecsAskFor()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage dashboardResponse = await SendAdminAsync(
            app,
            "/v1/ivr/order-confirmation/dashboard",
            IvrPermissions.QueueView);
        IvrServer.IvrDashboardProjection dashboard =
            (await dashboardResponse.Content.ReadFromJsonAsync<IvrServer.IvrDashboardProjection>())!;

        // `specs/ui/01` asks for these four tiles; before W-0101 no field
        // existed behind any of them.
        Assert.InRange(dashboard.Results.Call_success_rate, 0d, 1d);
        Assert.InRange(dashboard.Sim.Failure_rate, 0d, 1d);
        Assert.True(dashboard.Queue.Attempt_two_pending >= 0);
        Assert.True(dashboard.Queue.Blocked >= 0);

        // A cancel is a successful call: the line worked, the answer was no. So
        // call success is at least confirm plus cancel.
        Assert.True(
            dashboard.Results.Call_success_rate
                >= dashboard.Results.Confirm_rate + dashboard.Results.Cancel_rate - 0.0001d,
            $"call_success_rate={dashboard.Results.Call_success_rate} "
                + $"confirm={dashboard.Results.Confirm_rate} cancel={dashboard.Results.Cancel_rate}");

        using HttpResponseMessage detailResponse = await SendAdminAsync(
            app,
            $"/v1/ivr/order-confirmation/call-jobs/{GoldenHourJob}/detail",
            IvrPermissions.QueueView);
        IvrServer.IvrCallJobDetail detail =
            (await detailResponse.Content.ReadFromJsonAsync<IvrServer.IvrCallJobDetail>())!;

        // `specs/ui/03` wants the per-line snapshot, read back exactly as
        // Order Core captured it.
        IvrServer.SellableStatusLine line = Assert.Single(detail.Sellable_status);
        Assert.Equal("SKU-READ-01", line.Sku_id);
        Assert.Equal("BATCH-READ-01", line.Batch_id);
        Assert.True(line.Sale_lock);
        Assert.False(line.Recall_hold);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-READ-08")]
    public async Task NoReadProjectionCarriesRawContactDataOrADialToken()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        foreach (string route in new[]
        {
            "/v1/ivr/order-confirmation/dashboard",
            "/v1/ivr/order-confirmation/call-jobs",
            $"/v1/ivr/order-confirmation/call-jobs/{GoldenHourJob}/detail",
        })
        {
            using HttpResponseMessage response = await SendAdminAsync(
                app,
                route,
                IvrPermissions.QueueView);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string payload = await response.Content.ReadAsStringAsync();

            foreach (string forbidden in new[]
            {
                "dial_token",
                "phone_ref",
                "recording",
                "enc:read-dial-token",
                "0912341234",
                "phone-ref-read-gh",
            })
            {
                Assert.DoesNotContain(forbidden, payload, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private static async Task AssertSingleJobAsync(
        InternalAdminApiTestApplication app,
        string route,
        string expectedJobId)
    {
        using HttpResponseMessage response = await SendAdminAsync(
            app,
            route,
            IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IvrServer.IvrCallJobPage page =
            (await response.Content.ReadFromJsonAsync<IvrServer.IvrCallJobPage>())!;
        Assert.Equal(1, page.Total_count);
        Assert.Equal(expectedJobId, Assert.Single(page.Items).Ivr_call_job_id);
    }

    private Task<InternalAdminApiTestApplication> StartAsync() =>
        InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

    private static Task<HttpResponseMessage> SendAdminAsync(
        InternalAdminApiTestApplication app,
        string route,
        string permission)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, route);
        request.Headers.Add(MockPermissionAuthenticationHandler.HeaderName, permission);
        request.Headers.Add(MockPermissionAuthenticationHandler.ActorHeaderName, "operator-read");
        request.Headers.Add("X-Actor-Id", "operator-read");
        request.Headers.Add(
            "X-Correlation-Id",
            string.Concat("corr-", Guid.NewGuid().ToString("N")));
        return app.Client.SendAsync(request);
    }

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();

    /// <summary>
    /// Two jobs across both programs so the filters have something to separate:
    /// a Golden Hour job that confirmed and is close to its deadline, and a
    /// 24/7 job that ended in a final no-answer.
    /// </summary>
    private async Task SeedAsync()
    {
        DateTimeOffset startedAt = Now.AddMinutes(-2);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();

        context.ConfirmationTasks.AddRange(
            BuildTask(
                "TASK-READ-GH",
                "GF-ORDER-GH-FULL",
                "GF-GH",
                "84xxxxx0065",
                "GOLDEN_HOUR",
                "corr-read-gh",
                startedAt,
                Now.AddMinutes(3)),
            BuildTask(
                "TASK-READ-247",
                "GF-ORDER-247-FULL",
                "GF-247",
                "84xxxxx0247",
                "TWENTY_FOUR_SEVEN",
                "corr-read-247",
                startedAt,
                Now.AddHours(4)));

        context.CallJobs.AddRange(
            BuildJob(GoldenHourJob, "TASK-READ-GH", "GOLDEN_HOUR", "QUEUED", startedAt, Now.AddMinutes(3)),
            BuildJob(TwentyFourSevenJob, "TASK-READ-247", "TWENTY_FOUR_SEVEN", "HELD_MOCK", startedAt, Now.AddHours(4)));

        context.CallAttempts.AddRange(
            // Attempt 1 fails technically. ck_ivr_call_attempts_technical_not_counted
            // enforces DT-02: a technical exception is never a customer attempt.
            new CallAttemptEntity
            {
                IvrCallAttemptId = "ATTEMPT-READ-GH-1",
                IvrCallJobId = GoldenHourJob,
                TaskId = "TASK-READ-GH",
                AttemptNumber = 1,
                MaxAttemptsSnapshot = 2,
                ScheduledAt = startedAt,
                ScheduledWindowExpiresAt = Now.AddMinutes(3),
                StartedAt = startedAt,
                EndedAt = startedAt.AddSeconds(5),
                Status = "TECHNICAL_FAILED",
                ResultStatus = "IVR_TECHNICAL_EXCEPTION",
                IsCountedCustomerAttempt = false,
                TechnicalRetryAllowed = true,
                TechnicalRetryCount = 1,
                TechnicalExceptionType = "MOCK_ADAPTER_FAULT",
                SimChannelId = "SIM-READ-01",
                PolicyVersion = "gh-v1-candidate",
                ScriptVersion = "v1-test-approved",
                EvidenceRefsJson = "[\"evidence://ivr/read/attempt\"]",
            },
            // Attempt 2 reaches the customer and is confirmed with key 1.
            new CallAttemptEntity
            {
                IvrCallAttemptId = "ATTEMPT-READ-GH-2",
                IvrCallJobId = GoldenHourJob,
                TaskId = "TASK-READ-GH",
                AttemptNumber = 2,
                MaxAttemptsSnapshot = 2,
                ScheduledAt = startedAt.AddSeconds(150),
                ScheduledWindowExpiresAt = Now.AddMinutes(3),
                StartedAt = startedAt.AddSeconds(150),
                EndedAt = startedAt.AddSeconds(170),
                Status = "COMPLETED",
                ResultStatus = "IVR_CONFIRMED",
                DtmfKey = "1",
                Disposition = "CONFIRMED",
                IsCountedCustomerAttempt = true,
                TechnicalRetryAllowed = false,
                TechnicalRetryCount = 0,
                SimChannelId = "SIM-READ-01",
                PolicyVersion = "gh-v1-candidate",
                ScriptVersion = "v1-test-approved",
            },
            new CallAttemptEntity
            {
                IvrCallAttemptId = "ATTEMPT-READ-247",
                IvrCallJobId = TwentyFourSevenJob,
                TaskId = "TASK-READ-247",
                AttemptNumber = 1,
                MaxAttemptsSnapshot = 2,
                ScheduledAt = startedAt,
                ScheduledWindowExpiresAt = Now.AddHours(4),
                Status = "COMPLETED",
                ResultStatus = "IVR_NO_ANSWER_FINAL",
                IsCountedCustomerAttempt = false,
                NoAnswer = true,
                PolicyVersion = "247-v1-candidate",
                ScriptVersion = "v1-test-approved",
            });

        context.TechnicalExceptions.Add(new TechnicalExceptionEntity
        {
            TechnicalExceptionId = "TECH-READ-GH",
            IvrCallAttemptId = "ATTEMPT-READ-GH-1",
            ExceptionType = "MOCK_ADAPTER_FAULT",
            CustomerAttemptCounted = false,
            TechnicalRetryAllowed = true,
            TechnicalRetryCount = 1,
            CorrelationId = "corr-read-gh",
            CreatedAt = startedAt,
        });

        context.CallResults.AddRange(
            BuildResult("RESULT-READ-GH", GoldenHourJob, "TASK-READ-GH", "IVR_CONFIRMED", "1", startedAt),
            BuildResult("RESULT-READ-247", TwentyFourSevenJob, "TASK-READ-247", "IVR_NO_ANSWER_FINAL", null, startedAt));

        context.ResultCallbacks.Add(new ResultCallbackEntity
        {
            CallbackId = "CALLBACK-READ-GH",
            IvrCallResultId = "RESULT-READ-GH",
            TaskId = "TASK-READ-GH",
            OfficialOrderId = "ORDER-READ-GH",
            IdempotencyKey = "callback-read-gh",
            ResultStatus = "IVR_CONFIRMED",
            ResultState = "DELIVERED",
            DeliveryStatus = "ACKNOWLEDGED",
            RequiresCoreRevalidation = true,
            PayloadJson = "{}",
            PayloadSha256 = new string('C', 64),
            CoreHttpStatus = 200,
            CoreResponseCode = "ACCEPTED",
            CreatedAt = startedAt.AddSeconds(30),
            SentAt = startedAt.AddSeconds(31),
            AcknowledgedAt = startedAt.AddSeconds(32),
        });

        context.ReviewItems.Add(new ReviewItemEntity
        {
            ReviewItemId = "REVIEW-READ-GH",
            SourceType = "IVR_CALL_RESULT",
            SourceId = "RESULT-READ-GH",
            Reason = "verify confirmed evidence",
            Status = "OPEN",
            CorrelationId = "corr-read-gh",
            CreatedAt = startedAt.AddSeconds(30),
        });

        context.SimChannels.AddRange(
            new SimChannelEntity
            {
                SimChannelId = "SIM-READ-01",
                SimNumberRef = "sim-ref-read-01",
                Enabled = true,
                Status = "IDLE",
                AdapterMode = "MOCK",
                ExecutionMode = IvrOptions.MockExecutionMode,
                ProviderName = "MOCK",
                LastHealthCheckAt = Now,
            },
            new SimChannelEntity
            {
                SimChannelId = "SIM-READ-02",
                SimNumberRef = "sim-ref-read-02",
                Enabled = false,
                Status = "DISABLED",
                AdapterMode = "MOCK",
                ExecutionMode = IvrOptions.MockExecutionMode,
                ProviderName = "MOCK",
                DisabledReason = "health check failed",
            });

        context.CapacityIncidents.Add(new CapacityIncidentEntity
        {
            CapacityIncidentId = "INCIDENT-READ-01",
            SessionId = "session-read",
            ProgramCode = "GOLDEN_HOUR",
            Status = "OPEN",
            Scope = "SCHEDULER_DEADLINE",
            HoldNewCalls = false,
            ActiveSimCount = 1,
            PendingCallJobs = 2,
            ExpiredCallJobs = 0,
            MissedDeadlineCount = 3,
            ShortageReason = "MOCK_CAPACITY_SHORTAGE",
            OpenedAt = startedAt,
        });

        await context.SaveChangesAsync();
    }

    private static ConfirmationTaskEntity BuildTask(
        string taskId,
        string orderCode,
        string orderCodeShort,
        string phoneMasked,
        string program,
        string correlationId,
        DateTimeOffset startedAt,
        DateTimeOffset deadline) => new()
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = string.Concat(taskId, "-idempotency"),
            CorrelationId = correlationId,
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderCode = orderCode,
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            // ck_ivr_confirmation_tasks_matrix: Golden Hour pairs with ONLINE,
            // 24/7 pairs with COD.
            PaymentMethodSnapshot = program == "GOLDEN_HOUR" ? "ONLINE" : "COD",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = program,
            AttemptPolicyVersion = "mock-lab-v1",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = startedAt,
            ConfirmationWindowExpiresAt = deadline,
            PhoneRef = string.Concat("phone-ref-", taskId.ToLowerInvariant()),
            PhoneMasked = phoneMasked,
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:read-dial-token",
            DialTokenExpiresAt = deadline,
            // W-0106: Golden Hour carries a Southern area and 24/7 a Northern one, so the
            // derived voice_region has something real to resolve and the two jobs differ.
            PrivacySafeOrderSummaryJson =
                $"{{\"order_code_short\":\"{orderCodeShort}\",\"currency\":\"VND\","
                + $"\"delivery_area_short\":\"{(program == "GOLDEN_HOUR"
                    ? "Phường Phú Khương, tỉnh Vĩnh Long"
                    : "Phường Cửa Nam, thành phố Hà Nội")}\"}}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "v1-test-approved",
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE_FOR_IVR\"}",
            BlockedReasonsJson = "[\"DO_NOT_CALL\"]",
            SellableStatusJson = "[{\"sku_id\":\"SKU-READ-01\",\"batch_id\":\"BATCH-READ-01\","
                + "\"decision\":\"BLOCKED\",\"recall_hold\":false,\"sale_lock\":true,"
                + "\"quality_hold\":false,\"captured_at\":\"2026-08-15T01:00:00+00:00\"}]",
            SellableCapturedAt = startedAt,
            CallRestriction = false,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            ExpiresAt = deadline,
            AcceptedAt = startedAt,
            EvidenceRefsJson = "[\"evidence://ivr/read/task\"]",
        };

    private static CallJobEntity BuildJob(
        string jobId,
        string taskId,
        string program,
        string queueStatus,
        DateTimeOffset startedAt,
        DateTimeOffset deadline) => new()
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderVersionSnapshot = "1",
            ProgramType = program,
            AttemptPolicyCode = "mock-lab-v1",
            Status = "DRY_RUN",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowSeconds = 300,
            AttemptScheduleJson = "[]",
            T0At = startedAt,
            ExpiresAt = deadline,
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = queueStatus,
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            PrivacyPolicyVersion = "privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            EvidenceRefsJson = "[\"evidence://ivr/read/job\"]",
        };

    private static CallResultEntity BuildResult(
        string resultId,
        string jobId,
        string taskId,
        string resultType,
        string? dtmfKey,
        DateTimeOffset startedAt) => new()
        {
            IvrCallResultId = resultId,
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderVersionSnapshot = "1",
            OrderVersionSeenByIvr = "1",
            FinalResultStatus = resultType,
            ResultType = resultType,
            DtmfKey = dtmfKey,
            IsCountedCustomerAttempt = true,
            IsFinalForIvr = true,
            RecommendedCoreAction = "CORE_REVALIDATE_AND_CONTINUE",
            CoreOrderHandoffRequired = true,
            HumanReviewRequired = false,
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            NoPaymentOrRevenueEffect = true,
            CreatedAt = startedAt.AddSeconds(25),
            EvidenceRefsJson = "[\"evidence://ivr/read/result\"]",
            AuditRefsJson = "[\"audit://ivr/read/result\"]",
        };
}
