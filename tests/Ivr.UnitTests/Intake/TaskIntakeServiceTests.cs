using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Ports;
using Ivr.Domain.Policies;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Scripts;
using Microsoft.Extensions.Options;

#pragma warning disable CA2000 // TestContext owns and disposes its in-memory dependencies.

namespace Ivr.UnitTests.Intake;

public sealed class TaskIntakeServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 6, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(ProgramCode.GOLDEN_HOUR, IvrConfirmationTaskV1Payment_method_snapshot.ONLINE)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, IvrConfirmationTaskV1Payment_method_snapshot.COD)]
    public async Task BothSupportedProgramsCreateOneMockDryRunUnit(
        ProgramCode program,
        IvrConfirmationTaskV1Payment_method_snapshot payment)
    {
        TestContext test = CreateContext();
        IvrConfirmationTaskV1 source = CreateTask(program, payment);

        TaskIntakeOutcome outcome = await test.Service.IntakeAsync(Command(source));

        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);
        Assert.NotNull(outcome.IvrCallJobId);
        Assert.Equal(1, test.Store.TaskCount);
        Assert.Equal(1, test.Store.CallJobCount);
        Assert.Equal(1, test.Store.OutboxCount);
    }

    [Fact]
    public async Task ConcurrentDuplicateReplaysExactlyAndCreatesOnlyOneAtomicUnit()
    {
        TestContext test = CreateContext();
        IvrConfirmationTaskV1 source = CreateTask();

        TaskIntakeOutcome[] outcomes = await Task.WhenAll(
            Enumerable.Range(0, 12).Select(_ =>
                test.Service.IntakeAsync(Command(source))));

        Assert.Single(outcomes.Select(outcome => outcome.IvrCallJobId).Distinct());
        Assert.All(outcomes, outcome => Assert.Equal(
            TaskIntakeDecisions.AcceptedDryRunOnly,
            outcome.Decision));
        Assert.Equal(1, test.Store.TaskCount);
        Assert.Equal(1, test.Store.CallJobCount);
        Assert.Equal(1, test.Store.OutboxCount);
        Assert.Single(test.Audit.Entries);
    }

    [Fact]
    public async Task SameKeyWithChangedPayloadFailsIdempotencyConflict()
    {
        TestContext test = CreateContext();
        IvrConfirmationTaskV1 source = CreateTask();
        await test.Service.IntakeAsync(Command(source));

        IvrFailureException failure = await Assert.ThrowsAsync<IvrFailureException>(() =>
            test.Service.IntakeAsync(Command(source, payloadHash: new string('B', 64))));

        Assert.Equal(IvrErrorCodes.IdempotencyConflict, failure.ErrorCode);
        Assert.Equal(1, test.Store.CallJobCount);
    }

    /// <summary>
    /// W-0120. Every rejection here leaves as HTTP 200, so <c>blocked_reasons</c> is the only
    /// thing the producer ever learns about why its task died. Before this, two unrelated policy
    /// failures both reported <c>PROGRAM_PAYMENT_MATRIX_REJECTED</c> and seven unrelated contact
    /// failures all reported <c>CONTACT_OR_DIAL_TOKEN_INVALID</c> — so an integrating team that
    /// got one field wrong could not tell which, and had to ask.
    /// </summary>
    [Theory]
    [InlineData("confirmation-not-required", "IVR_CONFIRMATION_REQUIRED_NOT_TRUE")]
    [InlineData("program-payment-pair", "PROGRAM_PAYMENT_MATRIX_REJECTED")]
    [InlineData("phone-status", "PHONE_VALIDATION_STATUS_NOT_VALID")]
    [InlineData("phone-not-masked", "PHONE_MASKED_NOT_MASKED")]
    public async Task EachRejectionNamesTheFieldThatCausedIt(
        string caseId,
        string expectedReason)
    {
        IvrConfirmationTaskV1 source = caseId switch
        {
            "confirmation-not-required" => CreateTask(ivrConfirmationRequired: false),
            "program-payment-pair" => CreateTask(
                program: ProgramCode.GOLDEN_HOUR,
                payment: IvrConfirmationTaskV1Payment_method_snapshot.COD),
            "phone-status" => CreateTask(phoneStatus: "PHONE_VALID"),
            "phone-not-masked" => CreateTask(phoneMasked: "84901234567"),
            _ => throw new ArgumentOutOfRangeException(nameof(caseId)),
        };
        TestContext test = CreateContext();

        TaskIntakeOutcome outcome = await test.Service.IntakeAsync(Command(source));

        Assert.Contains(expectedReason, outcome.BlockedReasons);
        Assert.Null(outcome.IvrCallJobId);
        Assert.Equal(0, test.Store.CallJobCount);
    }

    /// <summary>
    /// W-0120. The reason code must be specific, but the decision must not have moved: these are
    /// the two shapes M3's producer will send once it maps its enums, and both must still be
    /// accepted exactly as before.
    /// </summary>
    [Theory]
    [InlineData(ProgramCode.GOLDEN_HOUR, IvrConfirmationTaskV1Payment_method_snapshot.ONLINE)]
    [InlineData(ProgramCode.TWENTY_FOUR_SEVEN, IvrConfirmationTaskV1Payment_method_snapshot.COD)]
    public async Task TheTwoBusinessApprovedPairsAreStillAccepted(
        ProgramCode program,
        IvrConfirmationTaskV1Payment_method_snapshot payment)
    {
        TestContext test = CreateContext();

        TaskIntakeOutcome outcome = await test.Service.IntakeAsync(
            Command(CreateTask(program: program, payment: payment)));

        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);
    }

    [Theory]
    [InlineData("unknown-policy", 2, false, "VALID", TaskIntakeDecisions.HeldPolicyMissing)]
    [InlineData(CandidateAttemptPolicies.Version, 3, false, "VALID", TaskIntakeDecisions.RejectedPolicyMismatch)]
    [InlineData(CandidateAttemptPolicies.Version, 2, false, "UNKNOWN", TaskIntakeDecisions.RejectedContactInvalid)]
    public async Task PolicyAndContactFailuresCreateNoJob(
        string policyVersion,
        int maxAttempts,
        bool callRestriction,
        string phoneStatus,
        string expectedDecision)
    {
        TestContext test = CreateContext();
        IvrConfirmationTaskV1 source = CreateTask(
            policyVersion: policyVersion,
            maxAttempts: maxAttempts,
            callRestriction: callRestriction,
            phoneStatus: phoneStatus);

        TaskIntakeOutcome outcome = await test.Service.IntakeAsync(Command(source));

        Assert.Equal(expectedDecision, outcome.Decision);
        Assert.Null(outcome.IvrCallJobId);
        Assert.Equal(0, test.Store.TaskCount);
        Assert.Equal(0, test.Store.CallJobCount);
        Assert.Equal(0, test.Store.OutboxCount);
        if (expectedDecision == TaskIntakeDecisions.RejectedPolicyMismatch)
        {
            Assert.Equal(IvrErrorCodes.PolicyMismatch, outcome.FailureCode);
        }
        else if (expectedDecision == TaskIntakeDecisions.RejectedContactInvalid)
        {
            Assert.Equal(IvrErrorCodes.ContactInvalid, outcome.FailureCode);
        }
    }

    [Fact]
    public async Task CallRestrictionBlocksBeforeAnyWorkIsPersisted()
    {
        TestContext test = CreateContext();
        IvrConfirmationTaskV1 source = CreateTask(callRestriction: true);

        TaskIntakeOutcome outcome = await test.Service.IntakeAsync(Command(source));
        Assert.Equal(TaskIntakeDecisions.BlockedOperational, outcome.Decision);
        Assert.Equal(IvrErrorCodes.OperationalBlocked, outcome.FailureCode);
        Assert.Null(await test.Store.FindAsync(source.Task_id));
        Assert.Equal(0, test.Store.CallJobCount);
    }

    [Fact]
    public async Task NewKeyReevaluatesTransientHoldButSameKeyStillReplays()
    {
        TestContext test = CreateContext();
        IvrConfirmationTaskV1 missingPolicy = CreateTask(policyVersion: "not-yet-present");
        TaskIntakeCommand firstCommand = Command(
            missingPolicy,
            idempotencyKey: "idem-transient-1");

        TaskIntakeOutcome first = await test.Service.IntakeAsync(firstCommand);
        TaskIntakeOutcome sameKey = await test.Service.IntakeAsync(firstCommand);
        TaskIntakeOutcome reevaluated = await test.Service.IntakeAsync(Command(
            CreateTask(),
            payloadHash: new string('B', 64),
            idempotencyKey: "idem-transient-2"));

        Assert.Equal(TaskIntakeDecisions.HeldPolicyMissing, first.Decision);
        Assert.Equal(first, sameKey);
        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, reevaluated.Decision);
        Assert.Equal(1, test.Store.CallJobCount);
    }

    [Theory]
    [InlineData("full-address")]
    [InlineData("spoken-phone")]
    public async Task SpeechPiiFailsClosedWithoutPersistingWork(string caseId)
    {
        (string area, string customerDisplayName) = caseId switch
        {
            "full-address" => ("Đường Nguyễn Huệ, Phường Bến Nghé, Quận Một", "chị An"),
            "spoken-phone" => ("Phường Bến Nghé, Quận Một", "chị An - số điện thoại tám bốn chín"),
            _ => throw new ArgumentOutOfRangeException(nameof(caseId), caseId, null),
        };

        TestContext test = CreateContext();
        IvrConfirmationTaskV1 source = CreateTask(
            deliveryArea: area,
            customerDisplayName: customerDisplayName);

        TaskIntakeOutcome outcome = await test.Service.IntakeAsync(Command(source));

        Assert.True(outcome.IsFailure);
        Assert.Equal(IvrErrorCodes.PiiPolicyViolation, outcome.FailureCode);
        Assert.Equal(0, test.Store.CallJobCount);
        Assert.DoesNotContain("Nguyễn Huệ", test.Audit.Entries.Single().DataJson,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("điện thoại", test.Audit.Entries.Single().DataJson,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PersistedMetadataPiiFailsBeforeAnyWorkIsCreated()
    {
        TestContext test = CreateContext();
        IvrConfirmationTaskV1 source = CreateTask(customerRef: "0901234567");

        TaskIntakeOutcome outcome = await test.Service.IntakeAsync(Command(source));

        Assert.True(outcome.IsFailure);
        Assert.Equal(IvrErrorCodes.PiiPolicyViolation, outcome.FailureCode);
        Assert.Equal(0, test.Store.TaskCount);
        Assert.Equal(0, test.Store.CallJobCount);
        Assert.Equal(0, test.Store.OutboxCount);
        Assert.DoesNotContain(source.Customer_ref!, test.Audit.Entries.Single().DataJson,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnapprovedScriptAndExpiredWindowFailBeforeAnyJob()
    {
        TestContext scriptTest = CreateContext();
        TaskIntakeOutcome scriptOutcome = await scriptTest.Service.IntakeAsync(Command(
            CreateTask(scriptTemplateId: "not-approved", scriptVersion: "999")));
        Assert.Equal(IvrErrorCodes.ScriptNotApproved, scriptOutcome.FailureCode);
        Assert.Equal(0, scriptTest.Store.CallJobCount);

        TestContext staleTest = CreateContext();
        TaskIntakeOutcome staleOutcome = await staleTest.Service.IntakeAsync(Command(
            CreateTask(windowStart: Now.AddMinutes(-10))));
        Assert.Equal(IvrErrorCodes.StateNotCallable, staleOutcome.FailureCode);
        Assert.Equal(0, staleTest.Store.CallJobCount);
    }

    [Fact]
    public async Task AuditContainsOnlySafeDecisionMetadata()
    {
        TestContext test = CreateContext();
        IvrConfirmationTaskV1 source = CreateTask(customerDisplayName: "chị Tuyết");

        await test.Service.IntakeAsync(Command(source));

        AuditLogEntry entry = Assert.Single(test.Audit.Entries);
        Assert.DoesNotContain("chị Tuyết", entry.DataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(source.Dial_token, entry.DataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(source.Phone_ref, entry.DataJson, StringComparison.Ordinal);
        Assert.Contains(TaskIntakeDecisions.AcceptedDryRunOnly, entry.DataJson,
            StringComparison.Ordinal);
    }

    private static TestContext CreateContext()
    {
        var clock = new FixedTimeProvider(Now);
        var audit = new InMemoryAuditLogger(clock);
        var scripts = new InMemoryScriptRegistry(
            audit,
            clock,
            Options.Create(new ScriptContentOptions()));
        var store = new InMemoryTaskIntakeStore(audit, clock);
        IAttemptPolicyRegistry policies = new FakeAttemptPolicyRegistry(
            CandidateAttemptPolicies.Create());
        var service = new TaskIntakeService(
            store,
            policies,
            scripts,
            new MockOnlyOpaqueValueProtector(),
            SpeechSummaryLimits.Create(100, 100),
            clock,
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SalesProvider = "FAKE_TARGET_V1",
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }));
        return new TestContext(service, store, audit, scripts);
    }

    private static TaskIntakeCommand Command(
        IvrConfirmationTaskV1 source,
        string? payloadHash = null,
        string idempotencyKey = "idem-p2-1") =>
        new(
            source,
            idempotencyKey,
            source.Correlation_id!,
            payloadHash ?? new string('A', 64),
            ExecutionMode.Mock);

    private static IvrConfirmationTaskV1 CreateTask(
        ProgramCode program = ProgramCode.GOLDEN_HOUR,
        IvrConfirmationTaskV1Payment_method_snapshot payment =
            IvrConfirmationTaskV1Payment_method_snapshot.ONLINE,
        string policyVersion = CandidateAttemptPolicies.Version,
        int maxAttempts = 2,
        bool callRestriction = false,
        string phoneStatus = "VALID",
        string deliveryArea = "Phường Bến Nghé, Quận Một",
        string customerDisplayName = "chị An",
        string? scriptTemplateId = null,
        string? scriptVersion = null,
        string? customerRef = null,
        DateTimeOffset? windowStart = null,
        bool ivrConfirmationRequired = true,
        string phoneMasked = "84xxxxx0001")
    {
        DateTimeOffset start = windowStart ?? Now.AddMinutes(-1);
        int windowSeconds = program == ProgramCode.GOLDEN_HOUR ? 300 : 900;
        int secondOffset = program == ProgramCode.GOLDEN_HOUR ? 150 : 450;
        return new IvrConfirmationTaskV1
        {
            Contract_version = IvrConfirmationTaskV1Contract_version.IvrOrderConfirmation_v1,
            Task_id = string.Concat("TASK-P2-1-", program),
            Correlation_id = "corr-p2-1",
            Created_at = start,
            Order_id = string.Concat("ORDER-P2-1-", program),
            Order_code = "GF-P2-1-001",
            Order_code_short = "P21001",
            Order_version = "17",
            Order_state = "CONFIRMING",
            Payment_method_snapshot = payment,
            Ivr_confirmation_required = ivrConfirmationRequired,
            Is_ivr_callable = true,
            Program_code = program,
            Confirmation_window_started_at = start,
            Confirmation_window_expires_at = start.AddSeconds(windowSeconds),
            Attempt_policy_version = policyVersion,
            Max_customer_attempts = maxAttempts,
            Attempt_offsets_seconds = [0, secondOffset],
            Customer_ref = customerRef,
            Phone_ref = "phone-ref-p2-1",
            Phone_masked = phoneMasked,
            Phone_validation_status = phoneStatus,
            Dial_token = "dial-token-p2-1",
            Dial_token_expires_at = start.AddSeconds(windowSeconds),
            Privacy_safe_order_summary = new Ivr.Contracts.Generated.IvrServer.V1.PrivacySafeOrderSummary
            {
                Customer_display_name = customerDisplayName,
                Order_code_short = "P21001",
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
                Delivery_area_short = deliveryArea,
                Program_display_name = program == ProgramCode.GOLDEN_HOUR
                    ? "Giờ Vàng"
                    : "Bán hàng hai mươi tư trên bảy",
                Locale = PrivacySafeOrderSummaryLocale.ViVN,
            },
            Call_script_template_id = scriptTemplateId,
            Call_script_version = scriptVersion,
            Allowed_script_variables = new { },
            Call_restriction = callRestriction,
            Eligibility_snapshot = new { decision = "ELIGIBLE", source = "unit-test" },
            Evidence_ref = "evidence://unit/p2-1",
        };
    }

    private sealed record TestContext(
        TaskIntakeService Service,
        InMemoryTaskIntakeStore Store,
        InMemoryAuditLogger Audit,
        InMemoryScriptRegistry Scripts) : IDisposable
    {
        public void Dispose()
        {
            Store.Dispose();
            Scripts.Dispose();
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
