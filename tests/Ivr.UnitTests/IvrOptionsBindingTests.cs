using Ivr.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests;

public sealed class IvrOptionsBindingTests
{
    [Fact]
    [Trait("TestId", "UT-BOOT-01")]
    public void CanonicalConfigurationKeysBindToIvrOptions()
    {
        Dictionary<string, string?> values = new()
        {
            ["IVR_EXECUTION_MODE"] = "MOCK",
            ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
            ["SIM_PROVIDER"] = "MOCK",
            ["ConnectionStrings:IvrDb"] = "Host=test-db;Database=ivr_test;Username=ivr",
            ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        ServiceCollection services = new();
        services.AddIvrFoundation(configuration);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IvrOptions options = serviceProvider.GetRequiredService<IOptions<IvrOptions>>().Value;

        Assert.Equal("MOCK", options.ExecutionMode);
        Assert.Equal("FAKE_TARGET_V1", options.SalesProvider);
        Assert.Equal("MOCK", options.SimProvider);
        Assert.Equal("Host=test-db;Database=ivr_test;Username=ivr", options.ConnectionString);
        Assert.False(options.RealCustomerCallAllowed);
    }
}
