using System.Xml.Linq;

namespace Ivr.UnitTests;

public sealed class ArchitectureDependencyTests
{
    [Fact]
    [Trait("TestId", "UT-BOOT-03")]
    public void DomainAssemblyDoesNotReferenceInfrastructure()
    {
        string repositoryRoot = FindRepositoryRoot();
        Dictionary<string, string[]> approvedReferences = new(StringComparer.Ordinal)
        {
            ["Ivr.Domain"] = [],
            ["Ivr.Contracts"] = [],
            ["Ivr.Infrastructure"] = ["Ivr.Contracts", "Ivr.Domain"],
            ["Ivr.Api"] = ["Ivr.Contracts", "Ivr.Infrastructure"],
            ["Ivr.Worker"] = ["Ivr.Contracts", "Ivr.Infrastructure"],
        };
        string[] projectFiles = Directory.GetFiles(
            Path.Combine(repositoryRoot, "src"),
            "*.csproj",
            SearchOption.AllDirectories);

        Assert.Equal(
            approvedReferences.Keys.Order(StringComparer.Ordinal),
            projectFiles.Select(Path.GetFileNameWithoutExtension).Order(StringComparer.Ordinal));

        foreach (string projectFile in projectFiles)
        {
            string projectName = Path.GetFileNameWithoutExtension(projectFile);
            string projectDirectory = Path.GetDirectoryName(projectFile)!;
            XDocument project = XDocument.Load(projectFile);
            string[] actualReferences = project
                .Descendants("ProjectReference")
                .Select(reference => reference.Attribute("Include")?.Value)
                .Where(include => !string.IsNullOrWhiteSpace(include))
                .Select(include => GetReferencedProjectName(include!, projectDirectory))
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(
                approvedReferences[projectName].Order(StringComparer.Ordinal),
                actualReferences);
        }
    }

    [Theory]
    [Trait("TestId", "UT-BOOT-03-LINUX-PATH")]
    [InlineData(@"..\Ivr.Contracts\Ivr.Contracts.csproj")]
    [InlineData("../Ivr.Contracts/Ivr.Contracts.csproj")]
    public void ProjectReferenceNameSupportsWindowsAndUnixSeparators(string include)
    {
        ArgumentNullException.ThrowIfNull(include);
        string projectDirectory = Path.Combine(Path.GetTempPath(), "Ivr.Api");

        Assert.Equal("Ivr.Contracts", GetReferencedProjectName(include, projectDirectory));
    }

    private static string GetReferencedProjectName(string include, string projectDirectory)
    {
        string platformPath = include
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFileNameWithoutExtension(
            Path.GetFullPath(platformPath, projectDirectory));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Ivr.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the IVR repository root.");
    }
}
