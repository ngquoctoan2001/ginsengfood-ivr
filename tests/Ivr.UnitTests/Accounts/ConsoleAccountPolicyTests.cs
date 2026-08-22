using Ivr.Domain.Accounts;
using Ivr.Infrastructure.Accounts;

namespace Ivr.UnitTests.Accounts;

public sealed class ConsoleAccountPolicyTests
{
    [Fact]
    [Trait("TestId", "UT-ACCOUNT-POLICY-01")]
    public void RequestedBootstrapPasswordAndUsernamesSatisfyPolicy()
    {
        const string password = "123123123zZ*";
        string[] usernames = ["admin", "ngquoctoan2001", "trcongphuc2003"];

        Assert.All(usernames, username =>
        {
            Assert.True(ConsoleUsernamePolicy.IsValid(username));
            Assert.True(ConsolePasswordPolicy.IsValid(password, username));
        });
    }

    [Fact]
    [Trait("TestId", "UT-ACCOUNT-POLICY-02")]
    public void PasswordPolicyRejectsMissingGroupsWhitespaceAndUsername()
    {
        Assert.False(ConsolePasswordPolicy.IsValid("short1A!", "operator"));
        Assert.False(ConsolePasswordPolicy.IsValid("ONLYUPPERCASE1!", "operator"));
        Assert.False(ConsolePasswordPolicy.IsValid("onlylowercase1!", "operator"));
        Assert.False(ConsolePasswordPolicy.IsValid("NoDigitsHere!!", "operator"));
        Assert.False(ConsolePasswordPolicy.IsValid("NoSymbolsHere123", "operator"));
        Assert.False(ConsolePasswordPolicy.IsValid("operator-Secret1!", "operator"));
        Assert.False(ConsolePasswordPolicy.IsValid("Whitespace Secret1!", "operator"));
    }

    [Fact]
    [Trait("TestId", "UT-ACCOUNT-HASH-03")]
    public void PasswordHashesAreSaltedVerifiableAndNeverEqualPlaintext()
    {
        const string password = "123123123zZ*";

        string first = ConsolePasswordHasher.Hash(password);
        string second = ConsolePasswordHasher.Hash(password);

        Assert.NotEqual(first, second);
        Assert.NotEqual(password, first);
        Assert.Equal(ConsolePasswordVerificationResult.Success,
            ConsolePasswordHasher.Verify(first, password));
        Assert.Equal(ConsolePasswordVerificationResult.Failed,
            ConsolePasswordHasher.Verify(first, "WrongPassword1!"));
        Assert.Equal(ConsolePasswordVerificationResult.Failed,
            ConsolePasswordHasher.Verify("corrupt", password));
    }

    /// <summary>
    /// The customer-PII guard's ASCII address branch matches <c>duong|ngo|ap|thon|hem</c>
    /// followed by a space, so it rejected the unaccented spelling of two ordinary Vietnamese
    /// surnames. A display name is the one field that exists to hold exactly that.
    /// </summary>
    [Theory]
    [InlineData("Nguyễn Quốc Toàn")]
    [InlineData("Trương Công Phúc")]
    [InlineData("Quản trị hệ thống")]
    [InlineData("Duong Minh Tuan")]
    [InlineData("Ngo Van A")]
    [InlineData("Ap Thi B")]
    [InlineData("Thon Van C")]
    [InlineData("Hem Thi D")]
    [InlineData("Tuấn Anh")]
    [Trait("TestId", "UT-ACCOUNT-NAME-05")]
    public void DisplayNamePolicyAcceptsVietnameseNamesWithAndWithoutDiacritics(string displayName)
    {
        Assert.True(ConsoleDisplayNamePolicy.IsValid(displayName));
        // The guard these names used to hit is unchanged; only the field's contract moved.
        Assert.True(Ivr.Domain.Privacy.PiiGuard.IsSafeContactText(displayName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Nhân viên 0912345678")]
    [InlineData("Lien he 091 234 5678")]
    [InlineData("Tuấn\nAnh")]
    [Trait("TestId", "UT-ACCOUNT-NAME-05")]
    public void DisplayNamePolicyStillRejectsBlankContactAndControlCharacters(string displayName)
    {
        Assert.False(ConsoleDisplayNamePolicy.IsValid(displayName));
    }

    [Fact]
    [Trait("TestId", "UT-ACCOUNT-NAME-05")]
    public void TheFullPiiGuardIsUnchangedForEverythingThatIsNotAStaffName()
    {
        // OD-OPEN-02 stands: the customer-facing contract still rejects these.
        Assert.False(Ivr.Domain.Privacy.PiiGuard.IsSafeText("Duong Minh Tuan"));
        Assert.False(Ivr.Domain.Privacy.PiiGuard.IsSafeText("giao den so nha 12"));
        Assert.False(Ivr.Domain.Privacy.PiiGuard.IsSafeText("đường Lê Lợi"));
        Assert.False(Ivr.Domain.Privacy.PiiGuard.IsSafeText("goi 0912345678"));

        // And the contact subset keeps the phone and dial-token branches.
        Assert.False(Ivr.Domain.Privacy.PiiGuard.IsSafeContactText("goi 0912345678"));
        Assert.False(Ivr.Domain.Privacy.PiiGuard.IsSafeContactText("dial_token: abcdefgh1234"));
        Assert.True(Ivr.Domain.Privacy.PiiGuard.IsSafeContactText("đường Lê Lợi"));
    }

    [Fact]
    [Trait("TestId", "UT-ACCOUNT-LOCK-04")]
    public void FifthFailureLocksForFifteenMinutes()
    {
        DateTimeOffset now = new(2026, 8, 22, 4, 0, 0, TimeSpan.Zero);

        Assert.Null(ConsoleLockoutPolicy.LockedUntil(4, now));
        Assert.Equal(now.AddMinutes(15), ConsoleLockoutPolicy.LockedUntil(5, now));
    }
}
