using System.Text.Json.Serialization;
using Ivr.Api.Auth;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Foundation;

public static class IvrApiServiceCollectionExtensions
{
    public const string RegisterMockPermissionProviderKey =
        "REGISTER_MOCK_PERMISSION_PROVIDER";

    public static IServiceCollection AddIvrApiFoundation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string executionMode = configuration["IVR_EXECUTION_MODE"]
            ?? configuration[$"{IvrOptions.SectionName}:{nameof(IvrOptions.ExecutionMode)}"]
            ?? IvrOptions.MockExecutionMode;
        bool isMock = string.Equals(
            executionMode,
            IvrOptions.MockExecutionMode,
            StringComparison.OrdinalIgnoreCase);
        bool registerMockProvider = configuration.GetValue<bool?>(
                RegisterMockPermissionProviderKey)
            ?? isMock;

        if (registerMockProvider && !isMock)
        {
            throw new InvalidOperationException(
                "The mock permission provider may only be registered in MOCK mode.");
        }

        string authenticationScheme = registerMockProvider
            ? MockPermissionAuthenticationHandler.SchemeName
            : FailClosedAuthenticationHandler.SchemeName;
        AuthenticationBuilder authentication = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = authenticationScheme;
            options.DefaultChallengeScheme = authenticationScheme;
            options.DefaultForbidScheme = authenticationScheme;
        });

        if (registerMockProvider)
        {
            authentication.AddScheme<AuthenticationSchemeOptions, MockPermissionAuthenticationHandler>(
                MockPermissionAuthenticationHandler.SchemeName,
                _ => { });
        }
        else
        {
            authentication.AddScheme<AuthenticationSchemeOptions, FailClosedAuthenticationHandler>(
                FailClosedAuthenticationHandler.SchemeName,
                _ => { });
        }

        services.AddAuthorization(options =>
        {
            foreach (string permission in IvrPermissions.All)
            {
                options.AddPolicy(
                    permission,
                    policy => policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });
        services.AddSingleton<IPermissionEvaluator, ClaimsPermissionEvaluator>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler,
            IvrAuthorizationMiddlewareResultHandler>();
        services.AddSingleton<IvrErrorResponseWriter>();
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddOptions<OrderCoreAllowlistOptions>()
            .Configure(options =>
            {
                options.ServiceToken = configuration[
                        OrderCoreAllowlistOptions.TokenConfigurationKey]
                    ?? string.Empty;
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ServiceToken),
                $"{OrderCoreAllowlistOptions.TokenConfigurationKey} is required.")
            .ValidateOnStart();

        return services;
    }
}
