using System.Globalization;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Scripts;
using Microsoft.Extensions.Options;

#pragma warning disable CA2000 // Harness owns and disposes its in-memory dependencies.

namespace Ivr.UnitTests.Intake;

/// <summary>
/// B17 (2026-09-25). The W-0298 intake guard asks one question now: does the confirmation window
/// open - its T0 - inside calling hours? It used to ask whether any attempt did, and at the
/// morning edge that disagreed with the scheduler, which does not drop an attempt that fell due
/// before 08:00 but dials it the moment the hours open, if its window is still open. So the old
/// answer refused tasks the scheduler would still have called, and admitted tasks whose first
/// call slid to 08:00 and closed up on the second.
/// <para>
/// Q-22 (2026-09-26) closed the evening the same way: every attempt has to fall inside calling
/// hours, so a T0 late enough to push the last attempt to 21:08 or later - from 21:00:30 for 24/7,
/// 21:05:30 for Golden Hour - is refused like a night order instead of getting one call.
/// </para>
/// <para>
/// Each case sets the clock to T0 - the task has just arrived - on a fixed day, with the default
/// calling window of 08:00-21:08 at +07:00, so nothing here reads the machine's clock or time
/// zone.
/// </para>
/// </summary>
public sealed class IntakeMorningBoundaryTests
{
    // The default CallingWindowOptions, restated in seconds of the local day so that the
    // expectations below are not computed by the class under test.
    private const int CallingHoursOpen = 8 * 3600;
    private const int CallingHoursClose = (21 * 3600) + (8 * 60);

    // The candidate policies' last attempt, in seconds after T0 (the first is always at T0).
    private const int GoldenHourLastOffset = 150;
    private const int TwentyFourSevenLastOffset = 450;

    // The decision and the reason stay what W-0298 put on the wire; only the condition changed.
    private const string ClosedReason =
        EligibilityReasonCodes.CallingWindowClosedForWholeConfirmationWindow;

    private const string Correlation = "corr-b17-1";

    // A fixed local day at the default offset.
    private static readonly DateTimeOffset LocalMidnight =
        new(2026, 8, 13, 0, 0, 0, TimeSpan.FromMinutes(420));

    private static readonly ProgramCode[] Programs =
        [ProgramCode.GOLDEN_HOUR, ProgramCode.TWENTY_FOUR_SEVEN];

    /// <summary>
    /// The morning edge. Module 3 holds an order that arrives before 08:00 and sends it again from
    /// 08:00 with a new window, so intake refuses it - also where a later attempt would already
    /// fall inside calling hours, which the old rule took as enough to accept.
    /// </summary>
    [Theory]
    // Refused by the old rule too: both attempts fall before 08:00.
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 7, 44, 0)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 7, 45, 0)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 7, 50, 0)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 7, 52, 29)]
    // Accepted by the old rule: the second attempt lands at or after 08:00.
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 7, 52, 30)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 7, 55, 0)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 7, 59, 59)]
    [InlineData(ProgramCode.GOLDEN_HOUR, 7, 57, 30)]
    [InlineData(ProgramCode.GOLDEN_HOUR, 7, 59, 59)]
    [Trait("TestId", "UT-INTAKE-MORNING-01")]
    public async Task AWindowOpeningBeforeEightIsRefused(
        ProgramCode program,
        int hour,
        int minute,
        int second)
    {
        DateTimeOffset t0 = At(SecondOfDay(hour, minute, second));
        using Harness harness = Harness.Create(t0);

        TaskIntakeOutcome outcome = await harness.Service.IntakeAsync(
            Command(CreateTask(program, t0, "TASK-B17-MORNING-01")));

        AssertRefusedForCallingHours(outcome);
        Assert.Equal(0, harness.Store.TaskCount);
        Assert.Equal(0, harness.Store.CallJobCount);
        Assert.Equal(0, harness.Store.OutboxCount);
    }

    /// <summary>The first second of calling hours opens a window for both programmes.</summary>
    [Theory]
    [InlineData(ProgramCode.GOLDEN_HOUR)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN)]
    [Trait("TestId", "UT-INTAKE-MORNING-02")]
    public async Task AWindowOpeningAtEightIsAccepted(ProgramCode program)
    {
        DateTimeOffset t0 = At(SecondOfDay(8, 0, 0));
        using Harness harness = Harness.Create(t0);

        TaskIntakeOutcome outcome = await harness.Service.IntakeAsync(
            Command(CreateTask(program, t0, "TASK-B17-MORNING-02")));

        AssertAccepted(outcome);
        Assert.Equal(1, harness.Store.TaskCount);
        Assert.Equal(1, harness.Store.CallJobCount);
        Assert.Equal(1, harness.Store.OutboxCount);
    }

    /// <summary>
    /// The evening edge is the last T0 whose last attempt still falls before 21:08: 21:00:29 for
    /// 24/7 (second attempt at 21:07:59, inside, because the calling window drops seconds) and
    /// 21:05:29 for Golden Hour. One second later that attempt lands on 21:08:00, is never
    /// dialled, and the task is refused (Q-22, 2026-09-26). Until then these rows were accepted
    /// and got one call; 21:08:00 was already refused and still is.
    /// </summary>
    [Theory]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 21, 0, 29, true)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 21, 0, 30, false)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 21, 8, 0, false)]
    [InlineData(ProgramCode.GOLDEN_HOUR, 21, 5, 29, true)]
    [InlineData(ProgramCode.GOLDEN_HOUR, 21, 5, 30, false)]
    [InlineData(ProgramCode.GOLDEN_HOUR, 21, 8, 0, false)]
    [Trait("TestId", "UT-INTAKE-EVENING-01")]
    public async Task TheEveningEdgeIsTheLastT0WhoseLastAttemptFits(
        ProgramCode program,
        int hour,
        int minute,
        int second,
        bool accepted)
    {
        DateTimeOffset t0 = At(SecondOfDay(hour, minute, second));
        using Harness harness = Harness.Create(t0);

        TaskIntakeOutcome outcome = await harness.Service.IntakeAsync(
            Command(CreateTask(program, t0, "TASK-B17-EVENING-01")));

        if (accepted)
        {
            AssertAccepted(outcome);
        }
        else
        {
            AssertRefusedForCallingHours(outcome);
        }

        Assert.Equal(accepted ? 1 : 0, harness.Store.CallJobCount);
    }

    /// <summary>
    /// Q-22 (2026-09-26). The evening stretch that used to get a single call - T0 21:00:30-21:07:59
    /// for 24/7, 21:05:30-21:07:59 for Golden Hour - is refused the way a night order is: the same
    /// decision, the same reason on the wire, and nothing stored, so Module 3 holds the order and
    /// sends it again from 08:00 under the same task id.
    /// </summary>
    [Theory]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 21, 0, 30)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 21, 4, 0)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, 21, 7, 59)]
    [InlineData(ProgramCode.GOLDEN_HOUR, 21, 5, 30)]
    [InlineData(ProgramCode.GOLDEN_HOUR, 21, 7, 59)]
    [Trait("TestId", "UT-INTAKE-EVENING-02")]
    public async Task TheEveningStretchWithRoomForOneCallIsRefusedLikeANightOrder(
        ProgramCode program,
        int hour,
        int minute,
        int second)
    {
        DateTimeOffset t0 = At(SecondOfDay(hour, minute, second));
        using Harness harness = Harness.Create(t0);

        TaskIntakeOutcome outcome = await harness.Service.IntakeAsync(
            Command(CreateTask(program, t0, "TASK-Q22-EVENING-02")));

        AssertRefusedForCallingHours(outcome);
        Assert.Equal(0, harness.Store.TaskCount);
        Assert.Equal(0, harness.Store.CallJobCount);
        Assert.Equal(0, harness.Store.OutboxCount);
    }

    /// <summary>
    /// Every minute of the local day for both programmes, plus the seconds at the edges (07:59:59,
    /// 08:00:00, 21:00:29, 21:00:30, 21:05:29, 21:05:30, 21:07:59, 21:08:00). A task is accepted
    /// exactly when every attempt falls inside calling hours - T0 in [08:00, 21:08) and T0 plus the
    /// last offset before 21:08 as well - and refused for calling hours otherwise, and a refusal
    /// persists nothing.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-INTAKE-WINDOW-SWEEP-01")]
    public async Task ATaskIsAcceptedExactlyWhenEveryAttemptFallsInsideCallingHours()
    {
        // The expectation below restates these hours. If the defaults move, fail here once rather
        // than as a thousand mismatches.
        var defaults = new CallingWindowOptions();
        Assert.True(defaults.Enabled);
        Assert.Equal(420, defaults.UtcOffsetMinutes);
        Assert.Equal(CallingHoursOpen / 60, defaults.StartMinuteOfLocalDay);
        Assert.Equal(CallingHoursClose / 60, defaults.EndMinuteOfLocalDay);

        // 08:00:00 and 21:08:00 are on the minute grid as well, so they are probed twice; each
        // probe is its own task, so that costs nothing.
        int[] probes =
        [
            .. Enumerable.Range(0, 24 * 60).Select(minute => minute * 60),
            SecondOfDay(7, 59, 59),
            SecondOfDay(8, 0, 0),
            SecondOfDay(21, 0, 29),
            SecondOfDay(21, 0, 30),
            SecondOfDay(21, 5, 29),
            SecondOfDay(21, 5, 30),
            SecondOfDay(21, 7, 59),
            SecondOfDay(21, 8, 0),
        ];

        List<string> mismatches = [];
        foreach (ProgramCode program in Programs)
        {
            using Harness harness = Harness.Create(At(0));
            int lastOffset = program == ProgramCode.GOLDEN_HOUR
                ? GoldenHourLastOffset
                : TwentyFourSevenLastOffset;
            int expectedAccepted = 0;
            for (int index = 0; index < probes.Length; index++)
            {
                int secondOfDay = probes[index];
                DateTimeOffset t0 = At(secondOfDay);
                harness.Clock.Set(t0);
                string taskId = string.Concat(
                    "TASK-B17-SWEEP-",
                    program.ToString(),
                    "-",
                    index.ToString(CultureInfo.InvariantCulture));

                TaskIntakeOutcome outcome = await harness.Service.IntakeAsync(
                    Command(CreateTask(program, t0, taskId)));

                // The first attempt is at T0 and the last at T0 + lastOffset; in between the
                // hours cannot shut, so these two bounds are every attempt.
                bool expectAccepted = secondOfDay >= CallingHoursOpen
                    && secondOfDay + lastOffset < CallingHoursClose;
                bool asExpected = expectAccepted
                    ? IsAccepted(outcome)
                    : IsRefusedForCallingHours(outcome);
                if (!asExpected)
                {
                    mismatches.Add(string.Concat(
                        program.ToString(),
                        " T0 ",
                        LocalMidnight.AddSeconds(secondOfDay)
                            .ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                        expectAccepted ? ": expected accepted, got " : ": expected refused, got ",
                        outcome.Decision,
                        " [",
                        string.Join(',', outcome.BlockedReasons),
                        "]"));
                }

                expectedAccepted += expectAccepted ? 1 : 0;
            }

            // What was accepted and what was stored are the same set.
            Assert.Equal(expectedAccepted, harness.Store.TaskCount);
            Assert.Equal(expectedAccepted, harness.Store.CallJobCount);
        }

        Assert.Empty(mismatches);
    }

    private static DateTimeOffset At(int secondOfLocalDay) =>
        LocalMidnight.AddSeconds(secondOfLocalDay).ToUniversalTime();

    private static int SecondOfDay(int hour, int minute, int second) =>
        (hour * 3600) + (minute * 60) + second;

    private static void AssertAccepted(TaskIntakeOutcome outcome)
    {
        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);
        Assert.NotNull(outcome.IvrCallJobId);
        Assert.Empty(outcome.BlockedReasons);
    }

    private static void AssertRefusedForCallingHours(TaskIntakeOutcome outcome)
    {
        Assert.Equal(TaskIntakeDecisions.BlockedOperational, outcome.Decision);
        Assert.Equal(ClosedReason, Assert.Single(outcome.BlockedReasons));
        Assert.Null(outcome.FailureCode);
        Assert.Null(outcome.IvrCallJobId);
    }

    private static bool IsAccepted(TaskIntakeOutcome outcome) =>
        outcome.Decision == TaskIntakeDecisions.AcceptedDryRunOnly
        && outcome.IvrCallJobId is not null
        && outcome.BlockedReasons.Length == 0;

    private static bool IsRefusedForCallingHours(TaskIntakeOutcome outcome) =>
        outcome.Decision == TaskIntakeDecisions.BlockedOperational
        && outcome.IvrCallJobId is null
        && outcome.FailureCode is null
        && outcome.BlockedReasons is [ClosedReason];

    private static TaskIntakeCommand Command(IvrConfirmationTaskV1 source) =>
        new(
            source,
            string.Concat("idem-", source.Task_id),
            Correlation,
            new string('A', 64),
            ExecutionMode.Mock);

    private static IvrConfirmationTaskV1 CreateTask(
        ProgramCode program,
        DateTimeOffset t0,
        string taskId)
    {
        bool goldenHour = program == ProgramCode.GOLDEN_HOUR;
        DateTimeOffset expiresAt = t0.AddSeconds(goldenHour ? 300 : 900);
        return new IvrConfirmationTaskV1
        {
            Contract_version = IvrConfirmationTaskV1Contract_version.IvrOrderConfirmation_v1,
            Task_id = taskId,
            Correlation_id = Correlation,
            Created_at = t0,
            Order_id = string.Concat("ORDER-", taskId),
            Order_code = "GF-B17-001",
            Order_code_short = "B17001",
            Order_version = "17",
            Order_state = "CONFIRMING",
            Payment_method_snapshot = goldenHour
                ? IvrConfirmationTaskV1Payment_method_snapshot.ONLINE
                : IvrConfirmationTaskV1Payment_method_snapshot.COD,
            Ivr_confirmation_required = true,
            Is_ivr_callable = true,
            Program_code = program,
            Confirmation_window_started_at = t0,
            Confirmation_window_expires_at = expiresAt,
            Attempt_policy_version = CandidateAttemptPolicies.Version,
            Max_customer_attempts = 2,
            Attempt_offsets_seconds = [0, goldenHour ? GoldenHourLastOffset : TwentyFourSevenLastOffset],
            Phone_ref = "phone-ref-b17-1",
            Phone_masked = "84xxxxx0001",
            Phone_validation_status = IvrConfirmationTaskV1Phone_validation_status.VALID,
            Dial_token = "dial-token-b17-1",
            Dial_token_expires_at = expiresAt,
            Privacy_safe_order_summary = new Ivr.Contracts.Generated.IvrServer.V1.PrivacySafeOrderSummary
            {
                Customer_display_name = "chị An",
                Order_code_short = "B17001",
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
                Program_display_name = goldenHour ? "Giờ Vàng" : "Bán hàng hai mươi tư trên bảy",
                Locale = PrivacySafeOrderSummaryLocale.ViVN,
            },
            Allowed_script_variables = new { },
            Call_restriction = false,
            Eligibility_snapshot = new { decision = "ELIGIBLE", source = "unit-test" },
            Evidence_ref = "evidence://unit/b17",
        };
    }

    private sealed record Harness(
        TaskIntakeService Service,
        InMemoryTaskIntakeStore Store,
        InMemoryScriptRegistry Scripts,
        SettableTimeProvider Clock) : IDisposable
    {
        public static Harness Create(DateTimeOffset now)
        {
            var clock = new SettableTimeProvider(now);
            var audit = new InMemoryAuditLogger(clock);
            var scripts = new InMemoryScriptRegistry(
                audit,
                clock,
                Options.Create(new ScriptContentOptions()));
            var store = new InMemoryTaskIntakeStore(audit, clock);
            var service = new TaskIntakeService(
                store,
                new FakeAttemptPolicyRegistry(CandidateAttemptPolicies.Create()),
                scripts,
                new MockOnlyOpaqueValueProtector(),
                SpeechSummaryLimits.Create(100, 100),
                clock,
                new CallingWindow(Options.Create(new CallingWindowOptions())),
                Options.Create(new IvrOptions
                {
                    ExecutionMode = IvrOptions.MockExecutionMode,
                    SalesProvider = "FAKE_TARGET_V1",
                    SimProvider = "MOCK",
                    RealCustomerCallAllowed = false,
                }));
            return new Harness(service, store, scripts, clock);
        }

        public void Dispose()
        {
            Store.Dispose();
            Scripts.Dispose();
        }
    }

    private sealed class SettableTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset utcNow = start;

        public void Set(DateTimeOffset instant) => utcNow = instant;

        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
