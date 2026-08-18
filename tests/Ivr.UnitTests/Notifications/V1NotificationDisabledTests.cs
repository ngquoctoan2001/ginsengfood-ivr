using System.Reflection;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence;

namespace Ivr.UnitTests.Notifications;

/// <summary>
/// W-0033 / P4-5. V1 sends no customer notification of any kind. The point of these tests is that
/// the guarantee should not rest on a flag alone: a flag can be flipped by whoever owns the
/// config, while a type that does not exist cannot be called by anyone.
/// </summary>
public sealed class V1NotificationDisabledTests
{
    private static readonly string[] ForbiddenSurfaceTerms =
    [
        "notification",
        "sms",
        "zalo",
        "email",
        "push",
        "messagetemplate",
        "customermessage",
    ];

    [Fact]
    [Trait("TestId", "UT-NOTIF-SURFACE-01")]
    public void NoNotificationSinkTemplateOrOutboxTypeExistsInTheRuntime()
    {
        // If a delivery surface existed, disabling it would be a configuration promise. Proving
        // the surface is absent makes "V1 cannot notify" a structural fact instead.
        Assembly[] runtime =
        [
            typeof(FeatureFlagSnapshot).Assembly,
            typeof(IvrErrors).Assembly,
        ];

        var offenders = new List<string>();
        foreach (Assembly assembly in runtime)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.FullName is not { } name)
                {
                    continue;
                }

                // The feature-flag key itself must stay: it is what records the decision.
                if (name.Contains("FeatureFlag", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (string term in ForbiddenSurfaceTerms)
                {
                    if (name.Contains(term, StringComparison.OrdinalIgnoreCase))
                    {
                        offenders.Add(name);
                    }
                }
            }
        }

        Assert.Empty(offenders);
    }

    [Fact]
    [Trait("TestId", "UT-NOTIF-STORE-02")]
    public void ThePersistenceModelHasNowhereToQueueACustomerMessage()
    {
        // A notification could also be smuggled in as an outbox row. There must be no table that
        // could hold one — the only outbound queues are task intake and the Sales result callback.
        string[] sets = typeof(IvrDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType.IsGenericType
                && property.PropertyType.GetGenericTypeDefinition().Name.StartsWith(
                    "DbSet",
                    StringComparison.Ordinal))
            .Select(property => property.Name)
            .ToArray();

        Assert.NotEmpty(sets);
        foreach (string set in sets)
        {
            foreach (string term in ForbiddenSurfaceTerms)
            {
                Assert.DoesNotContain(term, set, StringComparison.OrdinalIgnoreCase);
            }
        }

        // Positive half: the outbound queues that DO exist are named, so adding a third one is a
        // deliberate act that fails this test rather than passing unnoticed.
        string[] outboundQueues = sets
            .Where(name => name.Contains("Outbox", StringComparison.Ordinal)
                || name.Contains("Callback", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["ResultCallbacks", "TaskIntakeOutbox"], outboundQueues);
    }

    [Fact]
    [Trait("TestId", "UT-NOTIF-FLAG-03")]
    public void NotificationIsImmutableOffAndCannotBeReachedByAnyConfiguration()
    {
        FeatureFlagSnapshot off = FeatureFlagSnapshot.SafeDefault(FeatureFlagEnvironments.Lab);
        Assert.False(off.V1NotificationEnabled);

        // Turning it on through an admin mutation is refused...
        IvrFailureException mutation = Assert.Throws<IvrFailureException>(() =>
            FeatureFlagGuardrails.ValidateAdminMutation(
                off,
                off with { V1NotificationEnabled = true }));
        Assert.Contains("immutable-off", mutation.Message, StringComparison.Ordinal);

        // ...and refused again as an effective state, so a snapshot that arrived enabled by any
        // other path (seed, restore, direct write) still cannot start the service.
        IvrFailureException effective = Assert.Throws<IvrFailureException>(() =>
            FeatureFlagGuardrails.ValidateEffective(off with { V1NotificationEnabled = true }));
        Assert.Contains("must remain disabled", effective.Message, StringComparison.Ordinal);
    }
}
