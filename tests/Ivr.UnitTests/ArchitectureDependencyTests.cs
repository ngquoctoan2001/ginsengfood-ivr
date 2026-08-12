namespace Ivr.UnitTests;

public sealed class ArchitectureDependencyTests
{
    [Fact]
    [Trait("TestId", "UT-BOOT-03")]
    public void DomainAssemblyDoesNotReferenceInfrastructure()
    {
        string[] referencedAssemblies = typeof(Domain.AssemblyMarker)
            .Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("Ivr.Infrastructure", referencedAssemblies);
    }
}
