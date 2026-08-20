using Ivr.Worker;
using Ivr.Infrastructure.Analytics;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Retention;
using Ivr.Worker.Jobs;
using Ivr.Worker.Normalization;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddIvrFoundation(builder.Configuration);
builder.Services.AddIvrFeatureFlags(builder.Configuration);
builder.Services.AddIvrRetention(builder.Configuration);

// W-0055 / P10-4. Registered before the run-once branch on purpose: the analytics
// retention hook comes with it, and a retention CronJob that purged the operational
// rows while leaving their copies in the warehouse would turn a deletion into a move.
builder.Services.AddIvrAnalyticsPipeline(builder.Configuration);

// W-0047 / P7-5. One pass, then exit. A CronJob pod that never terminates is recorded as failed,
// which is what happened when W-0044 first scheduled one: RetentionJobHost completed its pass and
// returned, but the scheduler, normalisation and callback hosts kept the process alive.
//
// The other hosted services are not registered at all in this mode rather than merely stopped
// afterwards. A retention pod that briefly ran the scheduler could dispatch a call, and a job whose
// name says "retention" must not be able to place one.
bool runOnce = builder.Configuration.GetValue<bool>($"{RetentionOptions.SectionName}:RunOnce");
if (runOnce)
{
    builder.Services.AddHostedService<RetentionRunOnceHost>();
}
else
{
    builder.Services.AddIvrCallbackDelivery(builder.Configuration);
    // Singleton, and registered before the loops that stamp it. W-0043 §2: a wedged loop is
    // indistinguishable from a healthy one when the only liveness signal is whether the
    // process exited, and a wedge does not exit.
    builder.Services.AddSingleton<WorkerLiveness>();
    builder.Services.Configure<WorkerHealthOptions>(
        builder.Configuration.GetSection(WorkerHealthOptions.SectionName));
    builder.Services.AddHostedService<WorkerHealthEndpoint>();
    builder.Services.AddHostedService<MockSimChannelProvisioner>();
    builder.Services.AddHostedService<IvrHeartbeat>();
    builder.Services.AddHostedService<RetentionJobHost>();
    builder.Services.AddHostedService<SchedulerJobHost>();
    builder.Services.AddSingleton<ResultNormalizer>();
    builder.Services.AddHostedService<NormalizationJobHost>();
    builder.Services.AddHostedService<CallbackDeliveryJobHost>();
    builder.Services.AddHostedService<AnalyticsEtlJobHost>();
}

var host = builder.Build();
host.Run();
