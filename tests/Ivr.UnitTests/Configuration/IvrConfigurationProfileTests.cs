using Ivr.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace Ivr.UnitTests.Configuration;

/// <summary>
/// W-0203 / P1.2. The named configuration profile that carries the LocalMockE2E switch-on
/// settings, and the two things it must refuse.
/// </summary>
public sealed class IvrConfigurationProfileTests
{
    [Fact]
    [Trait("TestId", "UT-PROFILE-01")]
    public void NoProfileRequestedResolvesToNothing()
    {
        Assert.Null(IvrConfigurationProfile.ResolveFileName((string?)null, "Development"));
        Assert.Null(IvrConfigurationProfile.ResolveFileName(string.Empty, "Development"));
        Assert.Null(IvrConfigurationProfile.ResolveFileName("   ", "Development"));
    }

    [Fact]
    [Trait("TestId", "UT-PROFILE-02")]
    public void ProfileNameBecomesAFileNameBesideTheEnvironmentSettings()
    {
        Assert.Equal(
            "appsettings.Profile.LocalMockE2E.json",
            IvrConfigurationProfile.ResolveFileName("LocalMockE2E", "Development"));
        Assert.Equal(
            "appsettings.Profile.LocalMockE2E.json",
            IvrConfigurationProfile.ResolveFileName("  LocalMockE2E  ", "Staging"));
    }

    /// <summary>
    /// The name becomes part of a path, so anything that could leave the content root is refused
    /// rather than sanitised. Sanitising invites an argument about which escapes were covered.
    /// </summary>
    [Theory]
    [Trait("TestId", "UT-PROFILE-03")]
    [InlineData("../secrets")]
    [InlineData("Local/MockE2E")]
    [InlineData("Local\\MockE2E")]
    [InlineData("Local.MockE2E")]
    [InlineData("Local-Mock-E2E")]
    [InlineData("Local Mock E2E")]
    [InlineData("..")]
    [InlineData("ThisProfileNameIsFarTooLongToBeAcceptedHere")]
    public void MalformedProfileNamesAreRefused(string profileName)
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => IvrConfigurationProfile.ResolveFileName(profileName, "Development"));
        Assert.Contains("IVR_CONFIG_PROFILE", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A profile turns worker loops on, and the loops it turns on place calls. It is refused
    /// outside the same non-production allowlist that guards the developer surface, and refused
    /// loudly: a profile that silently failed to load would leave a host that looks configured
    /// and is not.
    /// </summary>
    [Theory]
    [Trait("TestId", "UT-PROFILE-04")]
    [InlineData("Production")]
    [InlineData("production")]
    [InlineData("Prod")]
    [InlineData("")]
    [InlineData(null)]
    public void ProfilesAreRefusedOutsideANonProductionEnvironment(string? environmentName)
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => IvrConfigurationProfile.ResolveFileName("LocalMockE2E", environmentName));
        Assert.Contains("non-production", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [Trait("TestId", "UT-PROFILE-05")]
    [InlineData("Development")]
    [InlineData("Testing")]
    [InlineData("Test")]
    [InlineData("Staging")]
    [InlineData("Lab")]
    public void EveryNonProductionEnvironmentMayLayerAProfile(string environmentName)
    {
        Assert.Equal(
            "appsettings.Profile.LocalMockE2E.json",
            IvrConfigurationProfile.ResolveFileName("LocalMockE2E", environmentName));
        Assert.True(NonProductionSurface.IsNonProductionEnvironment(environmentName));
    }

    [Fact]
    [Trait("TestId", "UT-PROFILE-06")]
    public void TheProfileIsReadFromTheSettingOfThatName()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [IvrConfigurationProfile.SettingName] = "LocalMockE2E",
            })
            .Build();

        Assert.Equal(
            "appsettings.Profile.LocalMockE2E.json",
            IvrConfigurationProfile.ResolveConfiguredFileName(configuration, "Development"));
        Assert.Null(IvrConfigurationProfile.ResolveConfiguredFileName(
            new ConfigurationBuilder().Build(),
            "Development"));
    }

    /// <summary>
    /// One list, not two. If this ever fails it means the developer surface and the profile loader
    /// have come to disagree about what production is, and the one consulted less often would be
    /// the one that is wrong.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-PROFILE-07")]
    public void TheProfileGuardAndTheDeveloperSurfaceShareOneEnvironmentList()
    {
        foreach (string environmentName in new[] { "Development", "Testing", "Test", "Staging", "Lab" })
        {
            Assert.True(NonProductionSurface.IsAvailable(environmentName, "MOCK", false));
            Assert.NotNull(IvrConfigurationProfile.ResolveFileName("Any", environmentName));
        }

        foreach (string environmentName in new[] { "Production", "Prod", "Live" })
        {
            Assert.False(NonProductionSurface.IsAvailable(environmentName, "MOCK", false));
            Assert.Throws<InvalidOperationException>(
                () => IvrConfigurationProfile.ResolveFileName("Any", environmentName));
        }
    }
}
