using System.Globalization;
using Ivr.Api.Auth;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Quota;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Middleware;

/// <summary>
/// W-0282 / B2. Enforces the per-account request ceiling, and is the first thing in this solution
/// that can return the <c>429</c> the contract has always declared.
/// <para>
/// Registered LAST in the request pipeline, after authentication, authorisation and the Order Core
/// allowlist. That order is load-bearing in two directions. A rejected caller must still receive
/// <c>401</c> or <c>403</c> rather than <c>429</c> — 266 of the 460 cases in the API behaviour
/// matrix assert exactly that, and a ceiling in front of authentication would turn a documented
/// auth failure into a throttling failure. And a quota is a budget for work actually done, so a
/// request that was never going to be served should not spend one.
/// </para>
/// <para>
/// What this therefore does NOT do: absorb a flood of unauthenticated requests. That is a denial-
/// of-service concern belonging to whatever sits in front of the API, not to a per-account budget,
/// and saying so is better than implying a protection that is not here.
/// </para>
/// </summary>
public sealed class ServiceQuotaMiddleware(
    RequestDelegate next,
    IOptions<ServiceQuotaOptions> options,
    ServiceQuotaCounter counter)
{
    public async Task InvokeAsync(HttpContext context, IvrErrorResponseWriter errorWriter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(errorWriter);

        ServiceQuotaOptions quota = options.Value;
        if (!quota.Enabled)
        {
            await next(context);
            return;
        }

        if (ServiceQuotaIdentity.Resolve(context) is not { } identity)
        {
            // No identity means nothing to bill. Every route that reaches here is behind either the
            // admin scheme or the Order Core allowlist, so this is the health and metrics surface
            // rather than an unauthenticated caller who slipped past a guard.
            await next(context);
            return;
        }

        ServiceQuotaAccountOptions? configured =
            quota.Accounts.GetValueOrDefault(identity.Name);
        int limit = configured?.RequestsPerWindow ?? quota.RequestsPerWindow;
        TimeSpan window = TimeSpan.FromSeconds(
            configured?.WindowSeconds ?? quota.WindowSeconds);

        ServiceQuotaDecision decision = counter.Admit(identity.Key, limit, window);
        if (decision.Allowed)
        {
            await next(context);
            return;
        }

        // Rounded UP, and never below one: a caller told to retry after zero seconds retries
        // immediately and is refused again, which reads as a broken endpoint rather than a budget.
        int retryAfterSeconds = Math.Max(
            1,
            (int)Math.Ceiling(decision.RetryAfter.TotalSeconds));
        await errorWriter.WriteAsync(
            context,
            IvrErrors.RateLimited(retryAfterSeconds, decision.Limit));
    }
}

/// <summary>
/// The account a request is billed to: <see cref="Key"/> counts it, <see cref="Name"/> looks up its
/// override in configuration.
/// <para>
/// Two values rather than one because configuration keys are <c>:</c>-separated, so a counting key
/// of <c>service:order-core</c> could not be written in a values file without becoming a nested
/// section. The counting key keeps a readable prefix for logs; configuration names the account
/// plainly.
/// </para>
/// </summary>
/// <param name="Key">Counting key, unique across identity kinds.</param>
/// <param name="Name">Configuration name, as an operator writes it.</param>
public readonly record struct ServiceQuotaIdentity(string Key, string Name)
{
    /// <summary>Set by <see cref="OrderCoreAllowlistMiddleware"/> once a caller authenticates.</summary>
    public const string ServiceIdentityItemKey = "ivr.service_identity.subject";

    public static ServiceQuotaIdentity? Resolve(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Items.TryGetValue(ServiceIdentityItemKey, out object? subject)
            && subject is string text
            && !string.IsNullOrWhiteSpace(text))
        {
            return new ServiceQuotaIdentity($"service/{text}", text);
        }

        // The admin surface is billed by TIER and nothing finer. X-Actor-Id names a person, which
        // is what the audit trail wants, but the caller writes that header about itself — keying a
        // ceiling on it would let one client hold an unlimited number of budgets by varying a
        // string. The credential is the account here, so the tier is the account.
        string? scope = context.User
            .FindFirst(AdminTokenAuthenticationHandler.ScopeClaimType)?.Value;
        return string.IsNullOrWhiteSpace(scope)
            ? null
            : new ServiceQuotaIdentity($"admin/{scope}", scope);
    }
}
