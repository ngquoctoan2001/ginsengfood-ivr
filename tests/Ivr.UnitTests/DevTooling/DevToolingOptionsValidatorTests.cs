using Ivr.Infrastructure.DevTooling;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.DevTooling;

public sealed class DevToolingOptionsValidatorTests
{
    private readonly DevToolingOptionsValidator validator = new();

    [Fact]
    [Trait("TestId", "UT-DEV-SEEDPATH-11")]
    public void EmptySeedDirectoryRemainsAValidDisabledDefault()
    {
        ValidateOptionsResult result = validator.Validate(null, new DevToolingOptions());

        Assert.True(result.Succeeded);
    }

    [Fact]
    [Trait("TestId", "UT-DEV-SEEDPATH-12")]
    public void MissingSeedDirectoryFailsWithTheConfigurationKeyAndRecoveryAction()
    {
        string missing = Path.Combine(Path.GetTempPath(), $"ivr-missing-{Guid.NewGuid():N}");

        ValidateOptionsResult result = validator.Validate(
            null,
            new DevToolingOptions { SeedDirectory = missing });

        Assert.True(result.Failed);
        string failure = Assert.Single(result.Failures!);
        Assert.Contains("Ivr:DevTooling:SeedDirectory", failure, StringComparison.Ordinal);
        Assert.Contains("does not exist", failure, StringComparison.Ordinal);
        Assert.Contains("Restore", failure, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-DEV-SEEDPATH-13")]
    public void MissingRequiredFileIsNamedBeforeTheApiStarts()
    {
        string directory = NewTemporaryDirectory();
        try
        {
            File.WriteAllText(Path.Combine(directory, SeedCatalog.TaskFileName), "{}");
            File.WriteAllText(Path.Combine(directory, SeedCatalog.IntegrationStatusFileName), "{}");

            ValidateOptionsResult result = validator.Validate(
                null,
                new DevToolingOptions { SeedDirectory = directory });

            Assert.True(result.Failed);
            Assert.Contains(
                result.Failures!,
                failure => failure.Contains(
                    SeedCatalog.ScenarioFileName,
                    StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    [Trait("TestId", "UT-DEV-SEEDPATH-14")]
    public void CompleteSeedDirectoryPassesStartupValidation()
    {
        string directory = NewTemporaryDirectory();
        try
        {
            File.WriteAllText(Path.Combine(directory, SeedCatalog.TaskFileName), "{}");
            File.WriteAllText(Path.Combine(directory, SeedCatalog.ScenarioFileName), "{}");
            File.WriteAllText(Path.Combine(directory, SeedCatalog.IntegrationStatusFileName), "{}");

            ValidateOptionsResult result = validator.Validate(
                null,
                new DevToolingOptions { SeedDirectory = directory });

            Assert.True(result.Succeeded);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string NewTemporaryDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"ivr-seed-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
