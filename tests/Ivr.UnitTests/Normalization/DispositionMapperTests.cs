using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;

namespace Ivr.UnitTests.Normalization;

public sealed class DispositionMapperTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 14, 2, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("1", IvrResultType.IvrConfirmed, "1")]
    [InlineData("0", IvrResultType.IvrCustomerCancelled, "0")]
    [Trait("TestId", "UT-NORM-01")]
    public void AnsweredOneOrZeroProducesCountedFinalSignal(
        string key,
        IvrResultType expected,
        string semanticKey)
    {
        NormalizedResult result = DispositionMapper.Normalize(
            SimProviderDisposition.Answered,
            key,
            null,
            Context());

        Assert.Equal(expected, result.ResultType);
        Assert.Equal(semanticKey, result.DtmfKey);
        Assert.True(result.IsCounted);
        Assert.True(result.IsFinal);
        Assert.False(result.TechnicalRetryAllowed);
    }

    [Fact]
    [Trait("TestId", "UT-NORM-02")]
    public void AnsweredWithoutInputCountsAndBecomesFinalOnlyAtAttemptLimit()
    {
        NormalizedResult first = DispositionMapper.Normalize(
            SimProviderDisposition.Answered,
            null,
            null,
            Context());
        NormalizedResult last = DispositionMapper.Normalize(
            SimProviderDisposition.Answered,
            null,
            null,
            Context(attemptNumber: 3));

        Assert.Equal(IvrResultType.IvrNoAnswerAttempt, first.ResultType);
        Assert.True(first.IsCounted);
        Assert.False(first.IsFinal);
        Assert.Equal(IvrResultType.IvrNoAnswerFinal, last.ResultType);
        Assert.True(last.IsCounted);
        Assert.True(last.IsFinal);
    }

    [Theory]
    [InlineData(SimProviderDisposition.RingTimeout, false)]
    [InlineData(SimProviderDisposition.Busy, false)]
    [InlineData(SimProviderDisposition.Rejected, true)]
    [Trait("TestId", "UT-NORM-03")]
    public void NonConnectionDispositionsAreCountedNoAnswerNotCancellation(
        SimProviderDisposition disposition,
        bool reviewRequired)
    {
        NormalizedResult result = DispositionMapper.Normalize(
            disposition,
            null,
            null,
            Context());

        Assert.Equal(IvrResultType.IvrNoAnswerAttempt, result.ResultType);
        Assert.NotEqual(IvrResultType.IvrCustomerCancelled, result.ResultType);
        Assert.True(result.IsCounted);
        Assert.False(result.IsFinal);
        Assert.Equal(reviewRequired, result.HumanReviewRequired);
    }

    [Theory]
    [InlineData(SimProviderDisposition.Dropped)]
    [InlineData(SimProviderDisposition.NetworkError)]
    [InlineData(SimProviderDisposition.SimError)]
    [InlineData(SimProviderDisposition.AudioError)]
    [InlineData(SimProviderDisposition.DtmfError)]
    [Trait("TestId", "UT-NORM-04")]
    public void TechnicalDispositionsNeverBecomeNoAnswerOrCounted(
        SimProviderDisposition disposition)
    {
        NormalizedResult result = DispositionMapper.Normalize(
            disposition,
            null,
            "SAFE_TECH_CODE",
            Context());

        Assert.Equal(IvrResultType.IvrTechnicalException, result.ResultType);
        Assert.False(result.IsNoAnswer);
        Assert.False(result.IsCounted);
        Assert.False(result.IsFinal);
        Assert.True(result.TechnicalRetryAllowed);
        Assert.Equal(1, result.TechnicalRetryCount);
    }

    [Theory]
    [InlineData(SimProviderDisposition.Unreachable)]
    [InlineData(SimProviderDisposition.InvalidDestination)]
    [Trait("TestId", "UT-NORM-05")]
    public void InvalidPhoneIsFinalAndDoesNotCountCustomerAttempt(
        SimProviderDisposition disposition)
    {
        NormalizedResult result = DispositionMapper.Normalize(
            disposition,
            null,
            null,
            Context());

        Assert.Equal(IvrResultType.IvrInvalidPhoneFinal, result.ResultType);
        Assert.False(result.IsCounted);
        Assert.True(result.IsFinal);
        Assert.True(result.HumanReviewRequired);
    }

    [Fact]
    [Trait("TestId", "UT-NORM-06")]
    public void KeyNineIsWrongInputWhileSupportKeyIsDisabled()
    {
        NormalizedResult result = DispositionMapper.Normalize(
            SimProviderDisposition.Answered,
            "9",
            null,
            Context());

        Assert.Equal(IvrResultType.IvrWrongInput, result.ResultType);
        Assert.Equal("INVALID", result.DtmfKey);
        Assert.True(result.IsCounted);
        Assert.False(result.IsFinal);
    }

    [Fact]
    [Trait("TestId", "UT-NORM-UNMAP-07")]
    public void UnknownDispositionFailsSafeAsTechnicalException()
    {
        NormalizedResult result = DispositionMapper.Normalize(
            null,
            "1",
            "unsafe technical code!",
            Context());

        Assert.Equal(IvrResultType.IvrTechnicalException, result.ResultType);
        Assert.Equal("UNSAFETECHNICALCODE", result.TechnicalErrorCode);
        Assert.Null(result.DtmfKey);
        Assert.False(result.IsCounted);
        Assert.False(result.IsNoAnswer);
    }

    [Fact]
    [Trait("TestId", "UT-NORM-RETRY-08")]
    public void TechnicalRetryIsBoundedAndWindowAware()
    {
        NormalizedResult exhausted = DispositionMapper.Normalize(
            SimProviderDisposition.NetworkError,
            null,
            null,
            Context(priorTechnicalRetryCount: 1));
        NormalizedResult expired = DispositionMapper.Normalize(
            SimProviderDisposition.SimError,
            null,
            null,
            Context(occurredAt: Now.AddMinutes(6)));

        Assert.False(exhausted.TechnicalRetryAllowed);
        Assert.Equal(2, exhausted.TechnicalRetryCount);
        Assert.True(exhausted.HumanReviewRequired);
        Assert.False(expired.TechnicalRetryAllowed);
        Assert.True(expired.HumanReviewRequired);
        Assert.False(exhausted.IsFinal);
        Assert.False(expired.IsFinal);
    }

    [Fact]
    [Trait("TestId", "UT-NORM-CAP-09")]
    public void CapacityIsFinalButNeverCountsCustomerAttempt()
    {
        NormalizedResult result = DispositionMapper.Capacity(Context());

        Assert.Equal(IvrResultType.IvrCapacityException, result.ResultType);
        Assert.False(result.IsCounted);
        Assert.True(result.IsFinal);
        Assert.True(result.HumanReviewRequired);
    }

    [Fact]
    [Trait("TestId", "UT-NORM-LASTKEY-10")]
    public void WrongInputAtLastAttemptClosesAsCountedNoAnswerFinal()
    {
        NormalizedResult result = DispositionMapper.Normalize(
            SimProviderDisposition.Answered,
            "INVALID",
            null,
            Context(attemptNumber: 3));

        Assert.Equal(IvrResultType.IvrNoAnswerFinal, result.ResultType);
        Assert.True(result.IsCounted);
        Assert.True(result.IsFinal);
        Assert.Equal("WRONG_INPUT_MAX_ATTEMPTS", result.Reason);
    }

    private static AttemptNormalizationContext Context(
        int attemptNumber = 1,
        int priorTechnicalRetryCount = 0,
        DateTimeOffset? occurredAt = null) => new(
            attemptNumber,
            3,
            occurredAt ?? Now,
            Now.AddMinutes(5),
            priorTechnicalRetryCount,
            1);
}
