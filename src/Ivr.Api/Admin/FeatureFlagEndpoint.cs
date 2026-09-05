using Ivr.Api.Internal;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.FeatureFlags;

namespace Ivr.Api.Admin;

public static class FeatureFlagEndpoint
{
    public const string ActorHeaderName = "X-Actor-Id";
    public const string IdempotencyHeaderName = "Idempotency-Key";

    public static IEndpointRouteBuilder MapIvrFeatureFlagEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        RouteGroupBuilder group = endpoints.MapGroup(
            "/v1/ivr/order-confirmation/feature-flags");

        group.MapGet(
                "/{environment}",
                async (
                    string environment,
                    IFeatureFlags featureFlags,
                    CancellationToken cancellationToken) =>
                {
                    RequireKnownEnvironment(environment);
                    FeatureFlagReadResult result = await featureFlags.GetSnapshotAsync(
                        environment,
                        true,
                        cancellationToken);
                    if (!result.ProviderReadable)
                    {
                        throw IvrErrors.OperationalBlocked(
                            "Feature flag provider is unavailable.");
                    }

                    return Results.Ok(result);
                })
            .RequireAuthorization(AdminPolicies.Read);

        group.MapGet(
                "/{environment}/kill-switch",
                async (
                    string environment,
                    IFeatureFlags featureFlags,
                    CancellationToken cancellationToken) =>
                {
                    RequireKnownEnvironment(environment);
                    FeatureFlagReadResult result = await featureFlags.GetSnapshotAsync(
                        environment,
                        true,
                        cancellationToken);
                    bool realCallsEnabled = result.ProviderReadable
                        && !result.Snapshot.GlobalDialKillSwitch;
                    return Results.Ok(new KillSwitchVerification(
                        result.ProviderReadable,
                        result.Snapshot.Revision,
                        result.Snapshot.GlobalDialKillSwitch,
                        realCallsEnabled));
                })
            .RequireAuthorization(AdminPolicies.Read);

        group.MapPost(
                "/{environment}",
                ExecuteMutationAsync)
            .RequireAuthorization(AdminPolicies.Danger);
        return endpoints;
    }

    /// <summary>
    /// W-0193. Refuses an environment name the catalogue does not carry, before it reaches the
    /// platform.
    /// <para>
    /// The name arrives as a path segment, so it is caller input. Left unchecked it reached
    /// <c>FeatureFlagSnapshot.SafeDefault</c>, which throws <c>ArgumentOutOfRangeException</c> -
    /// an unhandled exception, and therefore a <c>500</c>. A typo in a URL is a client mistake and
    /// has to read as one; a <c>500</c> tells the caller the service is broken and tells whoever is
    /// on call to go looking for a fault that is not there.
    /// </para>
    /// </summary>
    private static void RequireKnownEnvironment(string environment)
    {
        if (!FeatureFlagEnvironments.All.Contains(environment))
        {
            throw IvrErrors.NotFound("The feature flag environment was not found.");
        }
    }

    private static async Task<IResult> ExecuteMutationAsync(
        string environment,
        FeatureFlagMutationRequest request,
        HttpContext httpContext,
        IFeatureFlagAdminService adminService,
        IFeatureFlagCommandIdempotency idempotency,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        RequireKnownEnvironment(environment);

        // W-0128. The header is the source now: there is no console session to cross-check it
        // against, because Module 3 owns operator identity. The danger-tier policy has already
        // required this header to be present and safe before the endpoint runs.
        string actorId = InternalRequestGuard.RequireAdminActor(httpContext);

        string idempotencyKey = httpContext.Request.Headers[IdempotencyHeaderName]
            .FirstOrDefault()
            ?? throw IvrErrors.MalformedRequest("Idempotency-Key is required.");
        FeatureFlagMutationCommand command = new(
            environment,
            request.Changes,
            request.Reason,
            actorId,
            httpContext.Request.Headers[FeatureFlagClaims.DestinationRefHeaderName]
                .FirstOrDefault(),
            request.ApprovalReference,
            correlationContext.GetOrCreate());
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(
            new { environment, request });
        string payloadHash = Convert.ToHexString(SHA256.HashData(payload));
        FeatureFlagMutationApiResult result = await idempotency.ExecuteAsync(
            idempotencyKey,
            payloadHash,
            async token => FeatureFlagMutationApiResult.From(
                await adminService.MutateAsync(command, token)),
            cancellationToken);
        return Results.Ok(result);
    }
}

public sealed record FeatureFlagMutationRequest(
    FeatureFlagChangeSet Changes,
    string Reason,
    string? ApprovalReference);

public sealed record KillSwitchVerification(
    bool ProviderReadable,
    long Revision,
    bool GlobalDialKillSwitch,
    bool RealCallsEnabled);

public sealed record FeatureFlagMutationApiResult(
    FeatureFlagSnapshotApiResult Snapshot,
    string? ApprovedBy,
    IReadOnlyCollection<string> IncreasedRiskKeys)
{
    public static FeatureFlagMutationApiResult From(FeatureFlagMutationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new FeatureFlagMutationApiResult(
            FeatureFlagSnapshotApiResult.From(result.Snapshot),
            result.ApprovedBy,
            result.IncreasedRiskKeys.Order(StringComparer.Ordinal).ToArray());
    }
}

public sealed record FeatureFlagSnapshotApiResult(
    string Environment,
    long Revision,
    string ExecutionMode,
    string SalesProvider,
    string SimProvider,
    string AttemptPolicyVersion,
    bool RealCustomerCallAllowed,
    IReadOnlyCollection<string> LabDestinationAllowlist,
    bool GlobalDialKillSwitch,
    bool V1NotificationEnabled,
    bool RecordingEnabled)
{
    public static FeatureFlagSnapshotApiResult From(FeatureFlagSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new FeatureFlagSnapshotApiResult(
            snapshot.Environment,
            snapshot.Revision,
            snapshot.ExecutionMode,
            snapshot.SalesProvider,
            snapshot.SimProvider,
            snapshot.AttemptPolicyVersion,
            snapshot.RealCustomerCallAllowed,
            snapshot.LabDestinationAllowlist.Order(StringComparer.Ordinal).ToArray(),
            snapshot.GlobalDialKillSwitch,
            snapshot.V1NotificationEnabled,
            snapshot.RecordingEnabled);
    }
}
