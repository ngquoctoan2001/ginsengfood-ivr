using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Persistence.Entities;

namespace Ivr.UnitTests.Confirmation;

public sealed class ProgramResultContractInvariantTests
{
    public static TheoryData<IvrResultType, bool, bool, CoreActionRecommendation> RuntimeResults =>
        new()
        {
            { IvrResultType.IvrConfirmed, true, true, CoreActionRecommendation.RevalidateAndConfirmOrder },
            { IvrResultType.IvrCustomerCancelled, true, true, CoreActionRecommendation.RevalidateAndCancelCustomerRequest },
            { IvrResultType.IvrNoAnswerAttempt, true, false, CoreActionRecommendation.NoStateChangeWaitForTimeout },
            { IvrResultType.IvrNoAnswerFinal, true, true, CoreActionRecommendation.NoStateChangeWaitForTimeout },
            { IvrResultType.IvrConfirmationWindowExpired, false, true, CoreActionRecommendation.RevalidateAndExpireConfirmation },
            { IvrResultType.IvrInvalidPhoneFinal, false, true, CoreActionRecommendation.RevalidateAndHoldAdminReview },
            { IvrResultType.IvrWrongInput, true, false, CoreActionRecommendation.NoStateChangeWaitForTimeout },
            { IvrResultType.IvrTechnicalException, false, false, CoreActionRecommendation.RevalidateAndHoldAdminReview },
            { IvrResultType.IvrCapacityException, false, true, CoreActionRecommendation.RevalidateAndHoldAdminReview },
        };

    [Fact]
    [Trait("TestId", "UT-RESULT-CONTRACT-01")]
    public void SharedRuntimeAndCallbackSetsAreClosedAndExact()
    {
        IvrResultType[] shared = Enum.GetValues<IvrResultType>();
        IvrResultType[] runtime = [.. shared.Where(ResultContractPolicy.IsRuntimeResult)];
        IvrResultType[] final = [.. shared.Where(ResultContractPolicy.IsFinalCallbackResult)];
        IvrResultType[] nonFinal = [.. runtime.Except(final)];
        IvrResultType[] preCallOnly = [.. shared.Except(runtime)];

        Assert.Equal(11, shared.Length);
        Assert.Equal(9, runtime.Length);
        Assert.Equal(6, final.Length);
        Assert.Equal(3, nonFinal.Length);
        Assert.Equal(
            [IvrResultType.IvrOperationalBlocked, IvrResultType.IvrPolicyBlocked],
            preCallOnly);
    }

    [Theory]
    [MemberData(nameof(RuntimeResults))]
    [Trait("TestId", "UT-RESULT-CONTRACT-02")]
    public void EveryRuntimeResultRequiresItsExactCountedFinalAndActionSemantics(
        IvrResultType resultType,
        bool counted,
        bool final,
        CoreActionRecommendation action)
    {
        CallResultSnapshot accepted = TestData.Result(
            resultType,
            counted,
            action,
            final: final);
        Assert.Equal(resultType, accepted.ResultType);

        Assert.Throws<InvalidOperationException>(() => TestData.Result(
            resultType,
            !counted,
            action,
            final: final));
        Assert.Throws<InvalidOperationException>(() => TestData.Result(
            resultType,
            counted,
            action,
            final: !final));
    }

    [Fact]
    [Trait("TestId", "UT-RESULT-CONTRACT-03")]
    public void OutboxRejectsAFlaggedFinalResultOutsideTheSixFinalTypes()
    {
        var job = new CallJobEntity
        {
            TaskId = "TASK-W0172",
            OfficialOrderId = "ORDER-W0172",
            OrderVersionSnapshot = "VERSION-W0172",
            ProgramType = "GOLDEN_HOUR",
        };
        var forged = new NormalizedResult(
            IvrResultType.IvrWrongInput,
            true,
            true,
            "FORGED_FINAL_FLAG",
            null,
            null,
            CoreActionRecommendation.NoStateChangeWaitForTimeout,
            false,
            false,
            0);

        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(() =>
            CallbackOutboxSnapshotFactory.Create(
                "RESULT-W0172",
                job,
                1,
                forged,
                "evidence-W0172",
                "audit-W0172",
                new DateTimeOffset(2026, 9, 4, 0, 0, 0, TimeSpan.Zero)));

        Assert.Equal(
            "Only the six final IVR result types may enter the callback outbox.",
            failure.Message);
    }
}
