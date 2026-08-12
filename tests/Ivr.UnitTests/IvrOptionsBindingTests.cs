using Ivr.Contracts.Sales;
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
        Assert.Equal(SalesProviderKind.FakeTargetV1, options.GetSalesProviderKind());
        Assert.Equal("MOCK", options.SimProvider);
        Assert.Equal("Host=test-db;Database=ivr_test;Username=ivr", options.ConnectionString);
        Assert.False(options.RealCustomerCallAllowed);
    }

    [Theory]
    [Trait("TestId", "UT-CONTRACT-PROVIDER-01")]
    [InlineData("MOCK", "TARGET_V1", "MOCK")]
    [InlineData("MOCK", "CURRENT_GOLDEN_HOUR_COMPAT", "MOCK")]
    [InlineData("LAB_REAL_SIM", "FAKE_TARGET_V1", "MOCK")]
    [InlineData("PRODUCTION_REAL", "FAKE_TARGET_V1", "VENDOR")]
    [InlineData("PRODUCTION_REAL", "TARGET_V1", "MOCK")]
    [InlineData("MOCK", "UNKNOWN", "MOCK")]
    public void InvalidModeProviderCombinationsFailStartupValidation(
        string executionMode,
        string salesProvider,
        string simProvider)
    {
        IvrOptionsValidator validator = new();

        ValidateOptionsResult result = validator.Validate(
            name: null,
            new IvrOptions
            {
                ExecutionMode = executionMode,
                SalesProvider = salesProvider,
                SimProvider = simProvider,
                ConnectionString = "Host=test-db;Database=ivr_test;Username=ivr",
            });

        Assert.False(result.Succeeded);
    }

    [Theory]
    [Trait("TestId", "UT-CONTRACT-PROVIDER-02")]
    [InlineData("MOCK", "FAKE_TARGET_V1", "MOCK", SalesProviderKind.FakeTargetV1)]
    [InlineData("LAB_REAL_SIM", "FAKE_TARGET_V1", "VENDOR", SalesProviderKind.FakeTargetV1)]
    [InlineData("PRODUCTION_REAL", "TARGET_V1", "VENDOR", SalesProviderKind.TargetV1)]
    public void ApprovedModeProviderCombinationsProduceTypedProvider(
        string executionMode,
        string salesProvider,
        string simProvider,
        SalesProviderKind expected)
    {
        IvrOptions options = new()
        {
            ExecutionMode = executionMode,
            SalesProvider = salesProvider,
            SimProvider = simProvider,
            ConnectionString = "Host=test-db;Database=ivr_test;Username=ivr",
        };

        ValidateOptionsResult result = new IvrOptionsValidator().Validate(null, options);

        Assert.True(result.Succeeded);
        Assert.Equal(expected, options.GetSalesProviderKind());
    }
}
