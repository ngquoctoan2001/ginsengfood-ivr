namespace Ivr.CiFailingTests;

public sealed class DeliberatelyFailingTest
{
    [Xunit.Fact]
    public void CtCi02DeliberatelyFails() => Xunit.Assert.Fail("CT-CI-02 expected failure");
}
