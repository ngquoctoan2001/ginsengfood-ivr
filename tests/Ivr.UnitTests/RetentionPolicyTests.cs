using Ivr.Domain.Retention;
using Ivr.Infrastructure.Retention;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.UnitTests;

public sealed class RetentionPolicyTests
{
    [Fact]
    [Trait("TestId", "UT-RET-CONFIG-01")]
    public async Task MissingOwnerPeriodsFailClosedForEveryDataClass()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ivr:Retention:Enabled"] = "true",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddIvrRetention(configuration);
        await using ServiceProvider provider = services.BuildServiceProvider();
        IRetentionPolicyProvider policies = provider
            .GetRequiredService<IRetentionPolicyProvider>();

        foreach (string dataClass in RetentionDataClasses.All)
        {
            Assert.Null(await policies.GetPeriodAsync(dataClass, CancellationToken.None));
        }

        Assert.Null(await policies.GetPeriodAsync("unknown", CancellationToken.None));
        Assert.True(new RetentionRunOptions().DryRun);
    }
}
